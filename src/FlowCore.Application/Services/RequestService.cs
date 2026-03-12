using FlowCore.Application.DTOs.Request;
using FlowCore.Application.Interfaces;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Enums;
using FlowCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowCore.Application.Services;

public class RequestService : IRequestService
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Workflow> _workflowRepository;
    private readonly IRepository<Approval> _approvalRepository;
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationService;

    public RequestService(
        IRepository<Request> requestRepository,
        IRepository<Workflow> workflowRepository,
        IRepository<Approval> approvalRepository,
        IRepository<User> userRepository,
        INotificationService notificationService)
    {
        _requestRepository = requestRepository;
        _workflowRepository = workflowRepository;
        _approvalRepository = approvalRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<RequestDto>> GetAllAsync(int userId, string role)
    {
        Func<IQueryable<Request>, IQueryable<Request>> include =
            q => q.Include(r => r.RequestType).Include(r => r.Requester);

        IEnumerable<Request> requests;
        if (role == "Admin")
            requests = await _requestRepository.FindAsync(_ => true, include);
        else
            requests = await _requestRepository.FindAsync(r => r.RequesterId == userId, include);

        return requests.Select(MapToDto);
    }

    public async Task<RequestDto?> GetByIdAsync(int id, int userId, string role)
    {
        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null) return null;

        if (role != "Admin" && request.RequesterId != userId) return null;

        return MapToDto(request);
    }

    public async Task<RequestDto> CreateAsync(CreateRequestDto dto, int requesterId)
    {
        var request = new Request
        {
            RequestTypeId = dto.RequestTypeId,
            RequesterId = requesterId,
            Title = dto.Title,
            Status = RequestStatus.Draft,
            CurrentStep = 0
        };

        await _requestRepository.AddAsync(request);
        return MapToDto(request);
    }

    public async Task<RequestDto> UpdateAsync(int id, CreateRequestDto dto, int userId)
    {
        var request = await _requestRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Request {id} not found");

        if (request.RequesterId != userId)
            throw new UnauthorizedAccessException("Cannot edit another user's request");

        if (request.Status != RequestStatus.Draft && request.Status != RequestStatus.Returned)
            throw new InvalidOperationException("Only Draft or Returned requests can be edited");

        request.Title = dto.Title;
        request.RequestTypeId = dto.RequestTypeId;
        request.UpdatedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request);
        return MapToDto(request);
    }

    public async Task<RequestDto> SubmitAsync(int id, int userId)
    {
        var request = await _requestRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Request {id} not found");

        if (request.RequesterId != userId)
            throw new UnauthorizedAccessException("Cannot submit another user's request");

        if (request.Status != RequestStatus.Draft && request.Status != RequestStatus.Returned)
            throw new InvalidOperationException("Only Draft or Returned requests can be submitted");

        // Find active workflow for this request type
        var workflows = await _workflowRepository.FindAsync(
            w => w.RequestTypeId == request.RequestTypeId && w.IsActive,
            include: q => q.Include(w => w.Steps));
        var workflow = workflows.FirstOrDefault()
            ?? throw new InvalidOperationException("No active workflow found for this request type");

        var firstStep = workflow.Steps.OrderBy(s => s.StepOrder).FirstOrDefault()
            ?? throw new InvalidOperationException("Workflow has no steps");

        request.Status = RequestStatus.Pending;
        request.CurrentStep = firstStep.StepOrder;
        request.UpdatedAt = DateTime.UtcNow;

        // Create approval task for the first step
        var approvers = await ResolveApproversAsync(firstStep);
        foreach (var approver in approvers)
        {
            var approval = new Approval
            {
                RequestId = request.Id,
                StepOrder = firstStep.StepOrder,
                ApproverId = approver.Id
            };
            await _approvalRepository.AddAsync(approval);
            await _notificationService.NotifyApproverAsync(approver.Email, request.Title, request.Id);
        }

        await _requestRepository.UpdateAsync(request);
        return MapToDto(request);
    }

    private async Task<List<User>> ResolveApproversAsync(Domain.Entities.WorkflowStep step)
    {
        if (step.ApproverType == ApproverType.User)
        {
            if (!int.TryParse(step.ApproverValue, out var uid))
                return new List<User>();
            return (await _userRepository.FindAsync(u => u.Id == uid && u.IsActive)).ToList();
        }

        return step.ApproverType switch
        {
            ApproverType.Role => (await _userRepository.FindAsync(
                u => u.Role.Name == step.ApproverValue && u.IsActive)).ToList(),

            ApproverType.DepartmentManager => (await _userRepository.FindAsync(
                u => u.Department.Manager != null &&
                     u.Department.ManagerId == u.Id && u.IsActive)).ToList(),

            _ => new List<User>()
        };
    }

    private static RequestDto MapToDto(Request r) => new()
    {
        Id = r.Id,
        RequestTypeId = r.RequestTypeId,
        RequestTypeName = r.RequestType?.Name ?? "",
        RequesterId = r.RequesterId,
        RequesterName = r.Requester?.Username ?? "",
        Title = r.Title,
        Status = r.Status,
        CurrentStep = r.CurrentStep,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Approvals = r.Approvals.Select(a => new ApprovalHistoryDto
        {
            Id = a.Id,
            StepOrder = a.StepOrder,
            ApproverName = a.Approver?.Username ?? "",
            Action = a.Action?.ToString(),
            Comment = a.Comment,
            CreatedAt = a.CreatedAt
        }).ToList(),
        Attachments = r.Attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FileSize = a.FileSize,
            UploadedAt = a.UploadedAt
        }).ToList()
    };
}

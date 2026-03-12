using FlowCore.Application.DTOs.Approval;
using FlowCore.Application.Interfaces;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Enums;
using FlowCore.Domain.Interfaces;

namespace FlowCore.Application.Services;

public class ApprovalService : IApprovalService
{
    private readonly IRepository<Approval> _approvalRepository;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Workflow> _workflowRepository;
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationService;

    public ApprovalService(
        IRepository<Approval> approvalRepository,
        IRepository<Request> requestRepository,
        IRepository<Workflow> workflowRepository,
        IRepository<User> userRepository,
        INotificationService notificationService)
    {
        _approvalRepository = approvalRepository;
        _requestRepository = requestRepository;
        _workflowRepository = workflowRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<PendingApprovalDto>> GetPendingAsync(int userId)
    {
        var pending = await _approvalRepository.FindAsync(
            a => a.ApproverId == userId && a.Action == null);

        return pending.Select(a => new PendingApprovalDto
        {
            ApprovalId = a.Id,
            RequestId = a.RequestId,
            RequestTitle = a.Request?.Title ?? "",
            RequestTypeName = a.Request?.RequestType?.Name ?? "",
            RequesterName = a.Request?.Requester?.Username ?? "",
            StepOrder = a.StepOrder,
            StepName = "",
            CreatedAt = a.CreatedAt
        });
    }

    public async Task ApproveAsync(int approvalId, ApprovalActionDto dto, int approverId)
    {
        var approval = await GetValidApprovalAsync(approvalId, approverId);
        var request = approval.Request;

        approval.Action = ApprovalAction.Approve;
        approval.Comment = dto.Comment;
        await _approvalRepository.UpdateAsync(approval);

        // Check if all approvers at current step have approved
        var stepApprovals = await _approvalRepository.FindAsync(
            a => a.RequestId == request.Id && a.StepOrder == request.CurrentStep);

        bool allApproved = stepApprovals.All(a => a.Action == ApprovalAction.Approve);
        if (!allApproved) return;

        // Find workflow to get next step
        var workflows = await _workflowRepository.FindAsync(
            w => w.RequestTypeId == request.RequestTypeId && w.IsActive);
        var workflow = workflows.FirstOrDefault();
        var nextStep = workflow?.Steps
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault(s => s.StepOrder > request.CurrentStep);

        if (nextStep == null)
        {
            // Final step approved
            request.Status = RequestStatus.Approved;
            request.UpdatedAt = DateTime.UtcNow;
            await _notificationService.NotifyRequesterAsync(
                request.Requester?.Email ?? "", request.Title, "Approved");
        }
        else
        {
            // Move to next step
            request.CurrentStep = nextStep.StepOrder;
            request.UpdatedAt = DateTime.UtcNow;

            var approvers = await ResolveApproversAsync(nextStep);
            foreach (var approver in approvers)
            {
                var newApproval = new Approval
                {
                    RequestId = request.Id,
                    StepOrder = nextStep.StepOrder,
                    ApproverId = approver.Id
                };
                await _approvalRepository.AddAsync(newApproval);
                await _notificationService.NotifyApproverAsync(approver.Email, request.Title, request.Id);
            }
        }

        await _requestRepository.UpdateAsync(request);
    }

    public async Task RejectAsync(int approvalId, ApprovalActionDto dto, int approverId)
    {
        var approval = await GetValidApprovalAsync(approvalId, approverId);
        var request = approval.Request;

        approval.Action = ApprovalAction.Reject;
        approval.Comment = dto.Comment;
        await _approvalRepository.UpdateAsync(approval);

        request.Status = RequestStatus.Rejected;
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(request);

        await _notificationService.NotifyRequesterAsync(
            request.Requester?.Email ?? "", request.Title, "Rejected");
    }

    public async Task ReturnAsync(int approvalId, ApprovalActionDto dto, int approverId)
    {
        var approval = await GetValidApprovalAsync(approvalId, approverId);
        var request = approval.Request;

        approval.Action = ApprovalAction.Return;
        approval.Comment = dto.Comment;
        await _approvalRepository.UpdateAsync(approval);

        request.Status = RequestStatus.Returned;
        request.CurrentStep = 0;
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(request);

        await _notificationService.NotifyRequesterAsync(
            request.Requester?.Email ?? "", request.Title, "Returned for revision");
    }

    private async Task<Approval> GetValidApprovalAsync(int approvalId, int approverId)
    {
        var approval = await _approvalRepository.GetByIdAsync(approvalId)
            ?? throw new KeyNotFoundException($"Approval {approvalId} not found");

        if (approval.ApproverId != approverId)
            throw new UnauthorizedAccessException("You are not the approver for this task");

        if (approval.Action != null)
            throw new InvalidOperationException("This approval has already been processed");

        return approval;
    }

    private async Task<List<User>> ResolveApproversAsync(WorkflowStep step)
    {
        return step.ApproverType switch
        {
            ApproverType.User => (await _userRepository.FindAsync(
                u => u.Id == int.Parse(step.ApproverValue) && u.IsActive)).ToList(),

            ApproverType.Role => (await _userRepository.FindAsync(
                u => u.Role.Name == step.ApproverValue && u.IsActive)).ToList(),

            ApproverType.DepartmentManager => (await _userRepository.FindAsync(
                u => u.Department.ManagerId == u.Id && u.IsActive)).ToList(),

            _ => new List<User>()
        };
    }
}

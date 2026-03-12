using FlowCore.Application.DTOs.Workflow;
using FlowCore.Application.Interfaces;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowCore.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IRepository<Workflow> _workflowRepository;
    private readonly IRepository<RequestType> _requestTypeRepository;

    public WorkflowService(
        IRepository<Workflow> workflowRepository,
        IRepository<RequestType> requestTypeRepository)
    {
        _workflowRepository = workflowRepository;
        _requestTypeRepository = requestTypeRepository;
    }

    public async Task<IEnumerable<WorkflowDto>> GetAllAsync()
    {
        var workflows = await _workflowRepository.FindAsync(
            _ => true,
            include: q => q.Include(w => w.Steps));
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto?> GetByIdAsync(int id)
    {
        var workflows = await _workflowRepository.FindAsync(
            w => w.Id == id,
            include: q => q.Include(w => w.Steps));
        var workflow = workflows.FirstOrDefault();
        return workflow == null ? null : MapToDto(workflow);
    }

    public async Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto)
    {
        var workflow = new Workflow
        {
            RequestTypeId = dto.RequestTypeId,
            Name = dto.Name,
            IsActive = true,
            Steps = dto.Steps.Select(s => new WorkflowStep
            {
                StepOrder = s.StepOrder,
                StepName = s.StepName,
                ApproverType = s.ApproverType,
                ApproverValue = s.ApproverValue
            }).ToList()
        };

        await _workflowRepository.AddAsync(workflow);
        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> UpdateAsync(int id, CreateWorkflowDto dto)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Workflow {id} not found");

        workflow.Name = dto.Name;
        workflow.RequestTypeId = dto.RequestTypeId;
        workflow.Steps = dto.Steps.Select(s => new WorkflowStep
        {
            WorkflowId = id,
            StepOrder = s.StepOrder,
            StepName = s.StepName,
            ApproverType = s.ApproverType,
            ApproverValue = s.ApproverValue
        }).ToList();

        await _workflowRepository.UpdateAsync(workflow);
        return MapToDto(workflow);
    }

    private static WorkflowDto MapToDto(Workflow w) => new()
    {
        Id = w.Id,
        RequestTypeId = w.RequestTypeId,
        RequestTypeName = w.RequestType?.Name ?? "",
        Name = w.Name,
        IsActive = w.IsActive,
        Steps = w.Steps.OrderBy(s => s.StepOrder).Select(s => new WorkflowStepDto
        {
            Id = s.Id,
            StepOrder = s.StepOrder,
            StepName = s.StepName,
            ApproverType = s.ApproverType.ToString(),
            ApproverValue = s.ApproverValue
        }).ToList()
    };
}

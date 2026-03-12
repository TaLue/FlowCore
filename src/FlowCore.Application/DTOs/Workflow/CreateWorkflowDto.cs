using FlowCore.Domain.Enums;

namespace FlowCore.Application.DTOs.Workflow;

public class CreateWorkflowDto
{
    public int RequestTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CreateWorkflowStepDto> Steps { get; set; } = new();
}

public class CreateWorkflowStepDto
{
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public ApproverType ApproverType { get; set; }
    public string ApproverValue { get; set; } = string.Empty;
}

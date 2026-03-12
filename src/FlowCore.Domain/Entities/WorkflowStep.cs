using FlowCore.Domain.Enums;

namespace FlowCore.Domain.Entities;

public class WorkflowStep
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public ApproverType ApproverType { get; set; }
    public string ApproverValue { get; set; } = string.Empty;

    public Workflow Workflow { get; set; } = null!;
}

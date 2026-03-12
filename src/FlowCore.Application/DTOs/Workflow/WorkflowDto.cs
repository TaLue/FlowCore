namespace FlowCore.Application.DTOs.Workflow;

public class WorkflowDto
{
    public int Id { get; set; }
    public int RequestTypeId { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto
{
    public int Id { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ApproverType { get; set; } = string.Empty;
    public string ApproverValue { get; set; } = string.Empty;
}

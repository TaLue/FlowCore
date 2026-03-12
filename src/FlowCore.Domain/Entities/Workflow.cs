namespace FlowCore.Domain.Entities;

public class Workflow
{
    public int Id { get; set; }
    public int RequestTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public RequestType RequestType { get; set; } = null!;
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
}

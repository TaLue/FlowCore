namespace FlowCore.Domain.Entities;

public class RequestType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}

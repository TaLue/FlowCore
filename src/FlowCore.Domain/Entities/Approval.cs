using FlowCore.Domain.Enums;

namespace FlowCore.Domain.Entities;

public class Approval
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int StepOrder { get; set; }
    public int ApproverId { get; set; }
    public ApprovalAction? Action { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Request Request { get; set; } = null!;
    public User Approver { get; set; } = null!;
}

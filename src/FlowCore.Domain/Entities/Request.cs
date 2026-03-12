using FlowCore.Domain.Enums;

namespace FlowCore.Domain.Entities;

public class Request
{
    public int Id { get; set; }
    public int RequestTypeId { get; set; }
    public int RequesterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public int CurrentStep { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public RequestType RequestType { get; set; } = null!;
    public User Requester { get; set; } = null!;
    public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

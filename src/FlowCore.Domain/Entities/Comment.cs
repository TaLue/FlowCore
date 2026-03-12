namespace FlowCore.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Request Request { get; set; } = null!;
    public User User { get; set; } = null!;
}

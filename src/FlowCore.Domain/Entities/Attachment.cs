namespace FlowCore.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Request Request { get; set; } = null!;
    public User Uploader { get; set; } = null!;
}

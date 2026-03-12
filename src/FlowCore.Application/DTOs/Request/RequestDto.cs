using FlowCore.Domain.Enums;

namespace FlowCore.Application.DTOs.Request;

public class RequestDto
{
    public int Id { get; set; }
    public int RequestTypeId { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int CurrentStep { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ApprovalHistoryDto> Approvals { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class ApprovalHistoryDto
{
    public int Id { get; set; }
    public int StepOrder { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

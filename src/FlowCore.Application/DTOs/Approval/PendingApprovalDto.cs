namespace FlowCore.Application.DTOs.Approval;

public class PendingApprovalDto
{
    public int ApprovalId { get; set; }
    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = string.Empty;
    public string RequestTypeName { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

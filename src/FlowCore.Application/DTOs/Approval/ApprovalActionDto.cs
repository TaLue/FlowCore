using System.ComponentModel.DataAnnotations;

namespace FlowCore.Application.DTOs.Approval;

public class ApprovalActionDto
{
    [StringLength(1000)]
    public string? Comment { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace FlowCore.Application.DTOs.Request;

public class CreateRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "RequestTypeId must be a positive integer")]
    public int RequestTypeId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;
}

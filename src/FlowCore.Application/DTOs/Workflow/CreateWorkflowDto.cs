using System.ComponentModel.DataAnnotations;
using FlowCore.Domain.Enums;

namespace FlowCore.Application.DTOs.Workflow;

public class CreateWorkflowDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int RequestTypeId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Workflow must have at least one step")]
    public List<CreateWorkflowStepDto> Steps { get; set; } = new();
}

public class CreateWorkflowStepDto
{
    [Range(1, int.MaxValue)]
    public int StepOrder { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string StepName { get; set; } = string.Empty;

    public ApproverType ApproverType { get; set; }

    [StringLength(200)]
    public string ApproverValue { get; set; } = string.Empty;
}

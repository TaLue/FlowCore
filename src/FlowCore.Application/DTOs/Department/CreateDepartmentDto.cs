using System.ComponentModel.DataAnnotations;

namespace FlowCore.Application.DTOs.Department;

public class CreateDepartmentDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int? ManagerId { get; set; }
}

public class UpdateDepartmentDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int? ManagerId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace FlowCore.Application.DTOs.User;

public class UpdateUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>If provided, resets the user's password.</summary>
    [StringLength(200, MinimumLength = 6)]
    public string? NewPassword { get; set; }
}

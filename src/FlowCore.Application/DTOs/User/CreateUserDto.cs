using System.ComponentModel.DataAnnotations;

namespace FlowCore.Application.DTOs.User;

public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3-50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

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
}

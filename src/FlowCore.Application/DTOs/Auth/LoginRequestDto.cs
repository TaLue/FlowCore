using System.ComponentModel.DataAnnotations;

namespace FlowCore.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;
}

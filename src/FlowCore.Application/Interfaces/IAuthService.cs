using FlowCore.Application.DTOs.Auth;

namespace FlowCore.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using FlowCore.Application.DTOs.Auth;
using FlowCore.Application.Interfaces;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FlowCore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IConfiguration _config;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var users = await _userRepository.FindAsync(
            u => u.Username == dto.Username && u.IsActive,
            include: q => q.Include(u => u.Role).Include(u => u.Department))!;
        var user = users.FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return await GenerateTokensAsync(user);
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var tokens = await _refreshTokenRepository.FindAsync(
            rt => rt.Token == refreshToken && !rt.IsRevoked,
            include: q => q.Include(rt => rt.User)
                           .ThenInclude(u => u.Role)
                           .Include(rt => rt.User)
                           .ThenInclude(u => u.Department));

        var existing = tokens.FirstOrDefault();

        if (existing == null)
            return null;

        if (existing.ExpiresAt < DateTime.UtcNow)
        {
            // Expired — revoke and return null
            existing.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(existing);
            return null;
        }

        if (!existing.User.IsActive)
            return null;

        // Revoke the old token (rotation)
        existing.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(existing);

        return await GenerateTokensAsync(existing.User);
    }

    private async Task<LoginResponseDto> GenerateTokensAsync(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _config["Jwt:Issuer"] ?? "FlowCore";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var mins) ? mins : 60;
        var refreshExpiryDays = int.TryParse(_config["Jwt:RefreshExpiryDays"], out var days) ? days : 7;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new Claim("departmentId", user.DepartmentId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtIssuer,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        // Generate and persist refresh token
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = rawToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.AddAsync(refreshToken);

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = rawToken,
            ExpiresAt = expiry,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role?.Name ?? "User",
                Department = user.Department?.Name ?? ""
            }
        };
    }
}

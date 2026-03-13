using FlowCore.Application.DTOs.User;
using FlowCore.Application.Interfaces;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowCore.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.FindAsync(
            _ => true,
            include: q => q.Include(u => u.Role).Include(u => u.Department));
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var users = await _userRepository.FindAsync(
            u => u.Id == id,
            include: q => q.Include(u => u.Role).Include(u => u.Department));
        return users.Select(MapToDto).FirstOrDefault();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var existing = await _userRepository.FindAsync(u => u.Username == dto.Username);
        if (existing.Any())
            throw new InvalidOperationException($"Username '{dto.Username}' is already taken");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DepartmentId = dto.DepartmentId,
            RoleId = dto.RoleId,
            IsActive = true
        };

        await _userRepository.AddAsync(user);

        // Reload with navigation properties
        var created = await _userRepository.FindAsync(
            u => u.Id == user.Id,
            include: q => q.Include(u => u.Role).Include(u => u.Department));
        return MapToDto(created.First());
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto)
    {
        var users = await _userRepository.FindAsync(u => u.Id == id);
        var user = users.FirstOrDefault()
            ?? throw new KeyNotFoundException($"User {id} not found");

        user.Email = dto.Email;
        user.DepartmentId = dto.DepartmentId;
        user.RoleId = dto.RoleId;
        user.IsActive = dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _userRepository.UpdateAsync(user);

        // Reload with navigation properties
        var updated = await _userRepository.FindAsync(
            u => u.Id == id,
            include: q => q.Include(u => u.Role).Include(u => u.Department));
        return MapToDto(updated.First());
    }

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        DepartmentId = u.DepartmentId,
        DepartmentName = u.Department?.Name ?? "",
        RoleId = u.RoleId,
        RoleName = u.Role?.Name ?? "",
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}

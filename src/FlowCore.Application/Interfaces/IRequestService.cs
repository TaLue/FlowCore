using FlowCore.Application.DTOs.Request;

namespace FlowCore.Application.Interfaces;

public interface IRequestService
{
    Task<IEnumerable<RequestDto>> GetAllAsync(int userId, string role);
    Task<RequestDto?> GetByIdAsync(int id, int userId, string role);
    Task<RequestDto> CreateAsync(CreateRequestDto dto, int requesterId);
    Task<RequestDto> UpdateAsync(int id, CreateRequestDto dto, int userId);
    Task<RequestDto> SubmitAsync(int id, int userId);
}

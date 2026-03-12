using FlowCore.Application.DTOs.Workflow;

namespace FlowCore.Application.Interfaces;

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetAllAsync();
    Task<WorkflowDto?> GetByIdAsync(int id);
    Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto);
    Task<WorkflowDto> UpdateAsync(int id, CreateWorkflowDto dto);
}

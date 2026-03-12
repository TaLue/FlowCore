using FlowCore.Application.DTOs.Approval;

namespace FlowCore.Application.Interfaces;

public interface IApprovalService
{
    Task<IEnumerable<PendingApprovalDto>> GetPendingAsync(int userId);
    Task ApproveAsync(int approvalId, ApprovalActionDto dto, int approverId);
    Task RejectAsync(int approvalId, ApprovalActionDto dto, int approverId);
    Task ReturnAsync(int approvalId, ApprovalActionDto dto, int approverId);
}

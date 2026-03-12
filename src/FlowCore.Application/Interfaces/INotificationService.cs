namespace FlowCore.Application.Interfaces;

public interface INotificationService
{
    Task NotifyApproverAsync(string approverEmail, string requestTitle, int requestId);
    Task NotifyRequesterAsync(string requesterEmail, string requestTitle, string action);
}

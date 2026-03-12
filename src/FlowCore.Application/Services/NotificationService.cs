using FlowCore.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlowCore.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyApproverAsync(string approverEmail, string requestTitle, int requestId)
    {
        _logger.LogInformation("[NOTIFICATION] Approver {Email} - New request pending approval: '{Title}' (ID: {Id})",
            approverEmail, requestTitle, requestId);
        return Task.CompletedTask;
    }

    public Task NotifyRequesterAsync(string requesterEmail, string requestTitle, string action)
    {
        _logger.LogInformation("[NOTIFICATION] Requester {Email} - Request '{Title}' has been {Action}",
            requesterEmail, requestTitle, action);
        return Task.CompletedTask;
    }
}

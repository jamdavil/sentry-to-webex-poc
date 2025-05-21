using SentryWebexBot.Models;

namespace SentryWebexBot.Services;

public interface IWebexService
{
    Task<bool> SendSentryEventAsync(SentryWebhookPayload payload, CancellationToken cancellationToken = default);
}

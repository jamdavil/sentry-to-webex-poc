using System.Text;
using System.Text.Json;
using SentryWebexBot.Models;

namespace SentryWebexBot.Services.Impl;

public class WebexWebhookService : IWebexService
{
    private readonly HttpClient _httpClient;
    private readonly string _webexWebhookUrl;
    private readonly ILogger<WebexWebhookService> _logger;

    public WebexWebhookService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WebexWebhookService> logger)
    {
        _httpClient = httpClient;
        _webexWebhookUrl = configuration["Webex:WebhookUrl"] 
            ?? throw new ArgumentNullException("Webex:WebhookUrl is not configured");
        _logger = logger;
    }

    public async Task<bool> SendSentryEventAsync(SentryWebhookPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            #if DEBUG
            var serializedPayload = JsonSerializer.Serialize(payload);
            _logger.LogInformation($"Payload json: {serializedPayload}");
            #endif
            
            if (payload?.Data == null)
            {
                _logger.LogWarning("Payload data is null, not sending message to Webex.");
                return false;
            }
            
            var message = FormatSentryMessage(payload);
            var response = await _httpClient.PostAsJsonAsync(
                _webexWebhookUrl, 
                new { markdown = message },
                cancellationToken);
            
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Successfully sent message to Webex");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to Webex");
            return false;
        }
    }

    private static string FormatSentryMessage(SentryWebhookPayload payload)
    {
        if (payload.Data.Issue != null)
        {
            return FormatSentryIssue(payload);
        }
        
        if (payload.Data.Event != null)
        {
            return FormatSentryEvent(payload);
        }

        return $"🚨 **Received Sentry notification: {FormatAction(payload.Action)}**";
    }

    private static string FormatSentryIssue(SentryWebhookPayload payload)
    {
        var issue = payload.Data.Issue;
        var sb = new StringBuilder();
        
        var level = !string.IsNullOrEmpty(issue.Level) ? issue.Level : "unknown";
        var action = !string.IsNullOrEmpty(payload.Action) ? payload.Action : "updated";
        
        sb.AppendLine($"🚨 **{GetEmojiForLevel(level)} {FormatAction(action)}: {level.ToUpper()} issue**");
        sb.AppendLine();
        
        sb.AppendLine($"**{issue.Title ?? "Untitled issue"}**");
        if (!string.IsNullOrEmpty(issue.Culprit))
        {
            sb.AppendLine($"*Caused by:* `{issue.Culprit}`");
        }
        
        sb.AppendLine();
        sb.AppendLine($"**Status**: {FormatStatus(issue.Status ?? "unknown")}");
        sb.AppendLine($"**First Seen**: {DateTime.UtcNow:g} UTC");
        
        if (!string.IsNullOrEmpty(issue.Permalink))
        {
            sb.AppendLine();
            sb.AppendLine($"🔗 [View in Sentry]({issue.Permalink})");
        }
        
        return sb.ToString();
    }

    private static string FormatSentryEvent(SentryWebhookPayload payload)
    {
        var sentryEvent = payload.Data.Event;
        var sb = new StringBuilder();
        
        const string defaultLevel = "info";
        var action = !string.IsNullOrEmpty(payload.Action) ? payload.Action : "received";
        
        sb.AppendLine($"🔔 **{GetEmojiForLevel(defaultLevel)} {FormatAction(action)}: {defaultLevel.ToUpper()} event**");
        sb.AppendLine();
        
        var title = !string.IsNullOrEmpty(sentryEvent.Message) 
            ? sentryEvent.Message 
            : !string.IsNullOrEmpty(sentryEvent.EventId)
                ? $"Event {sentryEvent.EventId}"
                : "Untitled event";
                
        sb.AppendLine($"**{title}**");
        
        if (!string.IsNullOrEmpty(sentryEvent.EventId))
        {
            sb.AppendLine($"**Event ID**: `{sentryEvent.EventId}`");
        }
        
        if (!string.IsNullOrEmpty(sentryEvent.Platform))
        {
            sb.AppendLine($"**Platform**: {sentryEvent.Platform}");
        }
        
        if (sentryEvent.Timestamp.HasValue)
        {
            sb.AppendLine($"**When**: {sentryEvent.Timestamp.Value:g} UTC");
        }
        
        return sb.ToString();
    }

    private static string GetEmojiForLevel(string level) 
    {
        if (string.IsNullOrEmpty(level))
            return "🔔";
            
        return level.ToLower() switch
        {
            "error" => "❌",
            "warning" => "⚠️",
            "info" => "ℹ️",
            "debug" => "🐛",
            _ => "🔔"
        };
    }

    private static string FormatStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            return "❓ Unknown";

        return status.ToLower() switch
        {
            "resolved" => "✅ Resolved",
            "unresolved" => "❌ Unresolved",
            "ignored" => "👻 Ignored",
            _ => status
        };
    }
    
    private static string FormatAction(string action)
    {
        if (string.IsNullOrEmpty(action))
            return "Updated";
            
        return action.ToLower() switch
        {
            "created" => "Created",
            "resolved" => "Resolved",
            "assigned" => "Assigned",
            "ignored" => "Ignored",
            _ => action
        };
    }
}

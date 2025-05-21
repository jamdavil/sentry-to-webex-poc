using System.Text.Json.Serialization;

namespace SentryWebexBot.Models;

public class SentryWebhookPayload
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public WebhookData? Data { get; set; }
    
    [JsonPropertyName("installation")]
    public InstallationInfo? Installation { get; set; }
}

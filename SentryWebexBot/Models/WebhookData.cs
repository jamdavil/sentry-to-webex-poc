using System.Text.Json.Serialization;

namespace SentryWebexBot.Models;

public class WebhookData
{
    [JsonPropertyName("issue")]
    public Issue? Issue { get; set; }
    
    [JsonPropertyName("event")]
    public Event? Event { get; set; }
}

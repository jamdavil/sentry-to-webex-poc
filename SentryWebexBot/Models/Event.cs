using System.Text.Json.Serialization;

namespace SentryWebexBot.Models;

public class Event
{
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("platform")]
    public string? Platform { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

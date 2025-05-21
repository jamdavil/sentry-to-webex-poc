using System.Text.Json.Serialization;

namespace SentryWebexBot.Models;

public class Issue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("culprit")]
    public string? Culprit { get; set; }
    
    [JsonPropertyName("level")]
    public string? Level { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }
}

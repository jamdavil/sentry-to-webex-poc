using System.Text.Json.Serialization;

namespace SentryWebexBot.Models;

public class InstallationInfo
{
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }
}

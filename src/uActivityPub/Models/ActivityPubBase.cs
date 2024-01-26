using System.Text.Json.Serialization;
using uActivityPub.Helpers;

namespace uActivityPub.Models;

public class ActivityPubBase
{
    public string? Id { get; set; }
    public string Type { get; set; } = "Object";

    [JsonPropertyName("@context")]
    [JsonConverter(typeof(SingleOrArrayConverter<string>))]
    [JsonPropertyOrder(-100)]
    public IEnumerable<string> Context { get; set; } = default!;

}
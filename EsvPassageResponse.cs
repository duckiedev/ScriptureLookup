using System.Text.Json.Serialization;

namespace ScriptureLookup;

public class EsvPassageResponse
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = "";

    [JsonPropertyName("canonical")]
    public string Canonical { get; set; } = "";

    [JsonPropertyName("passages")]
    public List<string> Passages { get; set; } = [];
}
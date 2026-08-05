using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScriptureLookup;

public class EsvApiClient
{
    private static readonly HttpClient Http = new();
    private const string BaseUrl = "https://api.esv.org/v3/passage/text/";

    private readonly string _apiKey;

    public EsvApiClient(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    public async Task<EsvPassageResponse?> GetPassageAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"{BaseUrl}?q={Uri.EscapeDataString(query)}" +
            "&include-footnotes=false" +
            "&include-headings=true" +
            "&include-verse-numbers=true" +
            "&include-short-copyright=true" +
            "&include-passage-references=true");

        request.Headers.Add("Authorization", $"Token {_apiKey}");

        var response = await Http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EsvPassageResponse>(json);
    }
}
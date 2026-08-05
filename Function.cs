using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ScriptureLookup;

public class Function
{
    private readonly ScriptureRepository _repo;
    private readonly AmazonSimpleSystemsManagementClient _ssm;
    private string? _cachedApiKey;

    public Function()
    {
        var dynamoClient = new AmazonDynamoDBClient();
        _repo = new ScriptureRepository(dynamoClient);
        _ssm = new AmazonSimpleSystemsManagementClient();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var path = request.Path?.ToLowerInvariant() ?? "";
            var method = request.HttpMethod?.ToUpperInvariant() ?? "GET";

            if (method != "GET")
                return CreateResponse(405, new { error = "Method not allowed" });
            
            string passageQuery;

            if (path ==  "/votd" || path == "/verse-of-the-day")
            {
                passageQuery = VerseOfTheDay.GetTodaysVerse();
            }
            else if (path == "/passage")
            {
                string? q = null;
                request.QueryStringParameters?.TryGetValue("q", out q);
                if (string.IsNullOrWhiteSpace(q))
                    return CreateResponse(400, new 
                    { 
                        error = "Missing query parameter 'q'",
                        example = "/passages?q=John+3:16" 
                    });
                passageQuery = q;
            }
            else
            {
                return CreateResponse(404, new
                {
                    error = "Unknown route",
                    routes = new[] 
                    { 
                        "GET /passage?q=John+3:16", 
                        "GET /votd"
                }
                });
            }

            // check cache first
            var cached = await _repo.GetCachedPassageAsync(passageQuery);
            if (cached != null)
            {
                return CreateResponse(200, new
                {
                    reference = cached.Canonical,
                    text = cached.Text,
                    source = "cache",
                    cachedAt = cached.CachedAtUtc
                });
            }

            // cache miss - fetch from ESV API
            context.Logger.LogInformation($"Cache miss for '{passageQuery}'");
            var apiKey = await GetApiKeyAsync();
            var client = new EsvApiClient(apiKey);
            var result = await client.GetPassageAsync(passageQuery);

            if (result == null || result.Passages.Count == 0)
                return CreateResponse(404, new { error = "Passage not found" });

            var passageText = string.Join("\n\n", result.Passages).Trim();

            // store in cache
            await _repo.CachePassageAsync(passageQuery, result.Canonical, passageText);

            return CreateResponse(200, new
            {
                reference = result.Canonical,
                text = passageText,
                source = "esv-api"
            });
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled exception: {ex}");
            return CreateResponse(500, new { error = "Internal server error" });
        }
    }
    
    private async Task<string> GetApiKeyAsync()
    {
        if (_cachedApiKey != null)
            return _cachedApiKey;

        var response = await _ssm.GetParameterAsync(new GetParameterRequest
        {
            Name = "/ScriptureLookup/EsvApiKey",
            WithDecryption = true
        });
        
        _cachedApiKey = response.Parameter?.Value;
        if (string.IsNullOrEmpty(_cachedApiKey))
            throw new InvalidOperationException("ESV API key parameter is missing or empty.");

        return _cachedApiKey;
    }

    private static APIGatewayProxyResponse CreateResponse(int statusCode, object body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" }
            },
            Body = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }),
        };
    }
}
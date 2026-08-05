using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace ScriptureLookup;

public class ScriptureRepository
{
    private readonly IAmazonDynamoDB _dynamo;
    private const string TableName = "ScriptureCache";

    public ScriptureRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamo = dynamoDbClient;
    }

    public async Task<CachedPassage?> GetCachedPassageAsync(string passageKey)
    {
        var response = await _dynamo.GetItemAsync(new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PassageKey", new AttributeValue { S = passageKey.ToLowerInvariant() } }
            }
        });

        if (!response.IsItemSet)
            return null;

        var item = response.Item;
        return new CachedPassage
        {
            PassageKey = item["PassageKey"].S,
            Canonical = item["Canonical"].S,
            Text = item["Text"].S,
            CachedAtUtc = item["CachedAtUtc"].S
        };
    }

    public async Task CachePassageAsync(string passageKey, string canonical, string text)
    {
        await _dynamo.PutItemAsync(new PutItemRequest
        {
            TableName = TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "PassageKey", new AttributeValue (passageKey.ToLowerInvariant()) },
                { "Canonical", new AttributeValue (canonical) },
                { "Text", new AttributeValue(text) },
                { "CachedAtUtc", new AttributeValue(DateTime.UtcNow.ToString("O")) }
            }
        });
    }
}
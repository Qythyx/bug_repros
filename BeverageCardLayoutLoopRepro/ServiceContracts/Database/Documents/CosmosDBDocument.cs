using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Documents;

public abstract record CosmosDBDocument(
	[property: JsonPropertyName("id")] string ID,
	[property: JsonPropertyName("_etag")] string ETag
)
{
	
	public string PartitionKey => GetType().Name;
}

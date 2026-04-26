using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Documents;

public record AccountPendingVerification(
	string ID,
	DateTime Timestamp,
	string AccountID,
	string ClientToken,
	string ETag = null!
) : CosmosDBDocument(ID, ETag)
{
	/// <summary>Time-to-live, in seconds.</summary>
	[JsonPropertyName("ttl")]
#pragma warning disable CA1822 // Mark members as static
	public int TTL => (int)TimeSpan.FromHours(1).TotalSeconds;
#pragma warning restore CA1822 // Mark members as static

	[JsonIgnore]
	public string ServerToken => ID;
}

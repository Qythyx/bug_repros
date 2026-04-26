using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Documents;

public record AccountCredentials(string ID, string AccountID, string Token, string ETag = null!)
	: CosmosDBDocument(ID, ETag)
{
	[JsonIgnore]
	public string EmailAddress => ID;
}

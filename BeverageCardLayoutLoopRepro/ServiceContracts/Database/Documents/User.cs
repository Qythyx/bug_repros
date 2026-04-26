using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Documents;

public record User(
	string ID,
	string[] CredentialIds,
	string Name,
	UserRole Role,
	IReadOnlyList<string> SessionIds,
	string ETag = null!
) : CosmosDBDocument(ID, ETag)
{
	public User SetRole(UserRole role) => this with { Role = role };

	[JsonIgnore]
	public string Email => ID;
}

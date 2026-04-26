using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.Service.Contracts.Database.Documents;

public record Beverage(
	string ID,
	BeverageKind Kind,
	BeverageLanguages Details,
	string ABV,
	string ImageUrl,
	UntappdDetails UntappdDetails,
	bool IsAvailable,
	string ETag = null!
) : CosmosDBDocument(ID, ETag);

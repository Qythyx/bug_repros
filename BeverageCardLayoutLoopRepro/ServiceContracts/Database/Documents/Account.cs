using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.Service.Contracts.Database.Documents;

public record Account(
	string ID,
	ContactInformation ContactInformation,
	Address ShippingAddress,
	RegisteredPaymentMethod PaymentMethod,
	string? UntappdToken,
	string ETag = null!
) : CosmosDBDocument(ID, ETag);

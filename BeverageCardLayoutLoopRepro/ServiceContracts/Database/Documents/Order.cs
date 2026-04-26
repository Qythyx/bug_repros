using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.Service.Contracts.Database.Documents;

public record Order(
	string ID,
	string AccountID,
	DateTime EndDate,
	DateTime ShipDate,
	IReadOnlyList<OrderEntry> Entries,
	int ShippingFreeThreshold,
	int NonFreeShippingFee,
	bool IsClosed,
	bool ClosingSent,
	string ETag = null!
) : CosmosDBDocument(ID, ETag)
{
	public Order SetEntry(OrderEntry entry) =>
		this with
		{
			Entries =
				entry.Quantity == 0
					? [.. Entries.Where(e => e.OfferID != entry.OfferID)]
					: [.. Entries.Where(e => e.OfferID != entry.OfferID).Append(entry)],
		};
}

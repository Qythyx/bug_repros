
namespace Beerbox.Service.Contracts.Database.Documents;

public record Offer(
	string ID,
	int AllocatedQuantity,
	int AvailableQuantity,
	string BeverageID,
	DateTime Begins,
	DateTime Ends,
	bool Exclusive,
	int MaxQuantity,
	int MinQuantity,
	bool NotificationSent,
	int OriginalQuantity,
	int Price,
	bool Rare,
	bool Sale,
	string ETag = null!
) : CosmosDBDocument(ID ?? Guid.NewGuid().ToString(), ETag)
{
	public Offer Allocate(int toRemove) =>
		this with
		{
			AllocatedQuantity = AllocatedQuantity + toRemove,
			AvailableQuantity = AvailableQuantity - toRemove,
		};

	public Offer MarkNotificationSent() => this with { NotificationSent = true };
}

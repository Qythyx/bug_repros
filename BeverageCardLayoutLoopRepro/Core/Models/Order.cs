using System.Text.Json.Serialization;

namespace Beerbox.App.Core.Models;

public record Order(
	string ID,
	string AccountID,
	DateTime EndDate,
	DateTime EstimatedShipDate,
	IList<OrderEntry> Items,
	int ShippingFreeThreshold,
	int NonFreeShippingFee
)
{
	private const string TemporaryID = "Temporary ID";

	public DateTime EndDate { get; init; } = EndDate.ToLocalTime();

	public DateTime EstimatedShipDate { get; init; } = EstimatedShipDate.ToLocalTime();

	public IList<OrderEntry> Items { get; init; } = Items.Where(e => e.Quantity > 0).ToList().AsReadOnly();

	public bool IsTemporary => ID == TemporaryID;

	public int CountDistinct => Items.Count;

	[JsonIgnore]
	public int CountTotal { get; init; } = Items.Sum(_ => _.Quantity);

	[JsonIgnore]
	public int PriceBeverages { get; init; } = Items.Sum(i => i.Offer.Price.Amount * i.Quantity);

	[JsonIgnore]
	public int PriceShipping => PriceBeverages == 0 || PriceBeverages >= ShippingFreeThreshold ? 0 : NonFreeShippingFee;

	[JsonIgnore]
	public int PriceTotal => PriceBeverages + PriceShipping;

	public static async Task<Order> FromDocAsync(
		Service.Contracts.Database.Documents.Order doc,
		Func<IEnumerable<Service.Contracts.Database.Fragments.OrderEntry>, Task<IList<OrderEntry>>> converter
	) =>
		new(
			doc.ID,
			doc.AccountID,
			doc.EndDate,
			doc.ShipDate,
			await converter(doc.Entries),
			doc.ShippingFreeThreshold,
			doc.NonFreeShippingFee
		);

	public static Order CreateTemporary(IEnumerable<OrderEntry> entries) =>
		new(TemporaryID, Account.Temporary.ID, DateTime.MaxValue, DateTime.MaxValue, [.. entries], 0, 0);
}

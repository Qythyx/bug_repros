namespace Beerbox.App.Core.Models;

public record Offer(
	string ID,
	int AvailableQuantity,
	Beverage Beverage,
	DateTime Begins,
	DateTime Ends,
	bool Exclusive,
	int MaxQuantity,
	int MinQuantity,
	Price Price,
	bool Rare,
	bool Sale
)
{
	public DateTime Begins { get; init; } = Begins.ToLocalTime();
	public DateTime Ends { get; init; } = Ends.ToLocalTime();
}

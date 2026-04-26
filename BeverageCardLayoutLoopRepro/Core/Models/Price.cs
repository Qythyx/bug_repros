namespace Beerbox.App.Core.Models;

public record Price(int Amount)
{
	public Currency Currency { get; } = Currency.Yen;
}

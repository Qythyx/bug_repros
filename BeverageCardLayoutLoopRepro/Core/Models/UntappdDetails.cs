namespace Beerbox.App.Core.Models;

public record UntappdDetails(int BeverageID, double GlobalRating, int RatingCount)
{
	public double PersonalRating { get; set; }

	internal static UntappdDetails FromDocument(Service.Contracts.Database.Fragments.UntappdDetails doc) =>
		new(doc.BeverageID, doc.GlobalRating, doc.RatingCount);
}

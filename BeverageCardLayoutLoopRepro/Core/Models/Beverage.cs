using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Beerbox.Service.Contracts;

namespace Beerbox.App.Core.Models;

public record Beverage(
	string ID,
	BeverageKind Kind,
	IDictionary<Language, BeverageDetails> Details,
	string ABV,
	string? ImageUrl,
	UntappdDetails UntappdDetails,
	bool IsAvailable
)
{
	public IDictionary<Language, BeverageDetails> Details { get; init; } =
		new ReadOnlyDictionary<Language, BeverageDetails>(Details);

	public static Beverage FromDocument(Service.Contracts.Database.Documents.Beverage document) =>
		new(
			document.ID,
			document.Kind,
			new Dictionary<Language, BeverageDetails>()
			{
				{ Language.English, new BeverageDetails(document.Details.English) },
				{ Language.Japanese, new BeverageDetails(document.Details.Japanese) },
			},
			document.ABV,
			document.ImageUrl,
			UntappdDetails.FromDocument(document.UntappdDetails),
			document.IsAvailable
		);

	public virtual bool Equals(Beverage? other) =>
		other != null
		&& ID == other.ID
		&& Kind == other.Kind
		&& Details.All(d => d.Value.Equals(other.Details[d.Key]))
		&& ABV == other.ABV
		&& ImageUrl == other.ImageUrl
		&& UntappdDetails.Equals(other.UntappdDetails)
		&& IsAvailable == other.IsAvailable;

	public override int GetHashCode() => ID.GetHashCode();

	protected virtual bool PrintMembers(StringBuilder builder)
	{
		_ = builder.Append(CultureInfo.InvariantCulture, $"{ID}, {Details.First().Value.Name}");
		return true;
	}
}

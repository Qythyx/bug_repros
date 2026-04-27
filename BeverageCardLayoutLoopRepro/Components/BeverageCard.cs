using System.Globalization;
using Beerbox.App.Theming;
using MauiReactor;

namespace Beerbox.App.Components;

public sealed partial class BeverageCard : Component
{
	public override VisualNode Render()
	{
		const string name = "Grey Goose";
		const string maker = "Grey Goose";
		const string style = "Wheat-Based";
		const string abv = "40%";
		string[] tags = ["smooth", "wheat", "citrus", "almond", "premium"];
		const string size = "750ml";
		const decimal price = 5000m;
		const string imageUrl = "https://beerboxstaging.blob.core.windows.net/images/_mock%20Grey%20Goose.webp";
		const string kindLabel = "Vodka";
		const double globalRating = 3.9;
		const long untappdBeverageId = 200004;

		const string sep = "  •  ";

		Console.WriteLine($"*** TRACE Render entry @ {DateTime.Now:HH:mm:ss.fff}");

		return Grid(
				"*,Auto",
				"*",
				RenderBeverageImage(imageUrl)
					.GridRow(0)
					.OnLoaded(() => Console.WriteLine($"*** LOADED image @ {DateTime.Now:HH:mm:ss.fff}"))
					.OnSizeChanged(
						(sender, _) =>
						{
							if (sender is VisualElement v)
							{
								Console.WriteLine(
									$"*** SIZECHG image {v.Width:G17}x{v.Height:G17} @ {DateTime.Now:HH:mm:ss.fff}"
								);
							}
						}
					)
					.OnPropertyChanged(
						(sender, e) =>
						{
							if (e.PropertyName is "Height" or "Y" or "Width" or "X" && sender is VisualElement v)
							{
								var val = e.PropertyName switch
								{
									"Height" => v.Height,
									"Width" => v.Width,
									"X" => v.X,
									"Y" => v.Y,
									_ => double.NaN,
								};
								Console.WriteLine($"*** PROPCHG image {e.PropertyName}={val:G17}");
							}
						}
					),
				Border(Label(kindLabel).Margin(AppStyles.Spacing, AppStyles.Spacing / 2).VCenter().ThemeHeader())
					.ThemeSemiTransparent()
					.HCenter()
					.VEnd()
					.Margin(AppStyles.Spacing / 2)
					.StrokeCornerRadius(AppStyles.ChipIconSize)
					.GridRow(0)
					.InputTransparent(true),
				VStack(
						Label(name).ID(AutomationIds.Offer.BeverageName).ThemeBeverageName(),
						Label(maker).ID(AutomationIds.Offer.Maker).ThemeMaker(),
						FlexLayout(
								Label($"{style}{sep}").LineBreakMode(LineBreakMode.TailTruncation),
								Label($"ABV {abv}").LineBreakMode(LineBreakMode.NoWrap)
							)
							.Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
							.AlignItems(Microsoft.Maui.Layouts.FlexAlignItems.Start),
						Label(string.Join(sep, tags)),
						Label($"size {size}{sep}price {price:¥#,0} (tax incl.)").HStart(),
						RenderStarRating("community", globalRating, untappdBeverageId, AutomationIds.Offer.GlobalRating)
					)
					.Margin(AppStyles.MarginLR)
					.InputTransparent(true)
					.CascadeInputTransparent(false)
					.GridRow(1)
					.OnLoaded(() => Console.WriteLine($"*** LOADED vstack @ {DateTime.Now:HH:mm:ss.fff}"))
					.OnSizeChanged(
						(sender, _) =>
						{
							if (sender is VisualElement v)
							{
								Console.WriteLine(
									$"*** SIZECHG vstack {v.Width:G17}x{v.Height:G17} @ {DateTime.Now:HH:mm:ss.fff}"
								);
							}
						}
					)
					.OnPropertyChanged(
						(sender, e) =>
						{
							if (e.PropertyName is "Height" or "Y" or "Width" or "X" && sender is VisualElement v)
							{
								var val = e.PropertyName switch
								{
									"Height" => v.Height,
									"Width" => v.Width,
									"X" => v.X,
									"Y" => v.Y,
									_ => double.NaN,
								};
								Console.WriteLine($"*** PROPCHG vstack {e.PropertyName}={val:G17}");
							}
						}
					)
			)
			.OnLoaded(() => Console.WriteLine($"*** LOADED grid @ {DateTime.Now:HH:mm:ss.fff}"))
			.OnSizeChanged(
				(sender, _) =>
				{
					if (sender is VisualElement v)
					{
						Console.WriteLine(
							$"*** SIZECHG grid {v.Width:G17}x{v.Height:G17} @ {DateTime.Now:HH:mm:ss.fff}"
						);
					}
				}
			);
	}

	private static MauiReactor.Image RenderBeverageImage(string imageUrl) =>
		Image()
			.Source(
				new UriImageSource
				{
					Uri = new Uri(imageUrl),
					CacheValidity = TimeSpan.FromDays(28),
					CachingEnabled = false,
				}
			)
			.Aspect(Aspect.AspectFill)
			.InputTransparent(true);

	#region Star rating

	private static MauiReactor.Grid RenderStarRating(
		string label,
		double rating,
		long beverageId,
		AutoId<MauiReactor.Label> ratingAutomationId
	)
	{
		var starFile = $"rating_star_{Math.Round(rating * 4) * 0.25:0.00}".Replace(".", "_") + ".png";
		const double starHeight = AppFonts.FontSizeMedium;
		const double starWidth = starHeight * 5.0;

		return Grid(
				"Auto",
				"Auto,Auto,*",
				Label(label).TextColor(AppStyles.StarRatingColor).LineBreakMode(LineBreakMode.NoWrap).GridColumn(0),
				Image(starFile).Aspect(Aspect.Fill).HeightRequest(starHeight).WidthRequest(starWidth).GridColumn(1),
				Label(rating.ToString("0.##", CultureInfo.InvariantCulture))
					.ID(ratingAutomationId)
					.TextColor(AppStyles.StarRatingColor)
					.LineBreakMode(LineBreakMode.NoWrap)
					.GridColumn(2)
			)
			.HStart()
			.OnTapped(async () => await HandleUntappdRatingTapped(beverageId));
	}

	private static async Task HandleUntappdRatingTapped(long beverageId)
	{
		if (await Launcher.OpenAsync($"untappd://beer/{beverageId}"))
		{
			return;
		}
		_ = await Launcher.OpenAsync($"https://untappd.com/beer/{beverageId}");
	}

	#endregion Star rating
}

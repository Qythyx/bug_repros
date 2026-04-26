using System.Globalization;
using Beerbox.App.Core.Models;
using Beerbox.App.Core.Services;
using Beerbox.App.Core.Services.Settings;
using Beerbox.App.Extensions;
using Beerbox.App.Resources.Localization;
using Beerbox.App.Theming;
using Beerbox.Service.Contracts;
using MauiReactor;

namespace Beerbox.App.Components;

public sealed partial class BeverageCard : Component
{
	[Prop]
	private Offer? _offer;

	[Prop]
	private VisualNode[] _overlays = [];

	[Inject]
	private readonly AppSettings _settings;

	public override VisualNode Render()
	{
		if (_offer is null)
		{
			throw new InvalidOperationException("Offer must not be null");
		}
		var details = _offer.Beverage.LocalizedDetails;
		var untappd = _offer.Beverage.UntappdDetails;
		var showPersonalRating =
			_settings.UntappdSetting.HasFlag(UntappdSetting.PersonalRating) && untappd.PersonalRating > 0;
		var showGlobalRating =
			_settings.UntappdSetting.HasFlag(UntappdSetting.GlobalRating)
			|| (untappd.PersonalRating == 0 && _settings.UntappdSetting.HasFlag(UntappdSetting.PersonalRating));

		var kindLabel = _offer.Beverage.Kind switch
		{
			BeverageKind.Beer => AppResources.BeverageKindBeer,
			BeverageKind.Cider => AppResources.BeverageKindCider,
			BeverageKind.Gin => AppResources.BeverageKindGin,
			BeverageKind.Mead => AppResources.BeverageKindMead,
			BeverageKind.Vodka => AppResources.BeverageKindVodka,
			BeverageKind.Whisky => AppResources.BeverageKindWhisky,
			BeverageKind.Wine => AppResources.BeverageKindWine,
			_ => _offer.Beverage.Kind.ToString(),
		};

		var grid = Grid(
			"*,Auto",
			"*",
			RenderBeverageImage(_offer.Beverage.ImageUrl).GridRow(0),
			Border(Label(kindLabel).Margin(AppStyles.Spacing, AppStyles.Spacing / 2).VCenter().ThemeHeader())
				.ThemeSemiTransparent()
				.HCenter()
				.VEnd()
				.Margin(AppStyles.Spacing / 2)
				.StrokeCornerRadius(AppStyles.ChipIconSize)
				.GridRow(0)
				.InputTransparent(true),
			VStack(
					Label(details.Name).ID(AutomationIds.Offer.BeverageName).ThemeBeverageName(),
					Label(details.Maker).ID(AutomationIds.Offer.Maker).ThemeMaker(),
					FlexLayout(
							Label($"{details.Style}{AppResources.BeverageDetailsSeperator}")
								.LineBreakMode(LineBreakMode.TailTruncation),
							Label($"{AppResources.BeverageDetailsABV} {_offer.Beverage.ABV}")
								.LineBreakMode(LineBreakMode.NoWrap)
						)
						.Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
						.AlignItems(Microsoft.Maui.Layouts.FlexAlignItems.Start),
					details.Tags.Count != 0
						? Label(string.Join(AppResources.BeverageDetailsSeperator, details.Tags))
							.LineBreakMode(LineBreakMode.WordWrap)
						: null,
					Label(
							$"{AppResources.BeverageDetailsSize} {details.Size}"
								+ AppResources.BeverageDetailsSeperator
								+ $"{AppResources.OfferPrice} {_offer.Price.Amount:¥#,0} {AppResources.TaxIncluded}"
						)
						.HStart()
						.LineBreakMode(LineBreakMode.WordWrap),
					showGlobalRating
						? RenderStarRating(
							AppResources.UntappdRating,
							untappd.GlobalRating,
							untappd.BeverageID,
							AutomationIds.Offer.GlobalRating
						)
						: null,
					showPersonalRating
						? RenderStarRating(
							AppResources.UntappdPersonalRating,
							untappd.PersonalRating,
							untappd.BeverageID,
							AutomationIds.Offer.PersonalRating
						)
						: null
				)
				.Margin(AppStyles.MarginLR)
				.InputTransparent(true)
				.CascadeInputTransparent(false)
				.GridRow(1)
		);
		grid.AddChildren(_overlays.Select(child => child.GridRowSpan(2)));
		return grid;
	}

	private static MauiReactor.Image RenderBeverageImage(string? imageUrl) =>
		string.IsNullOrEmpty(imageUrl)
			? Image().InputTransparent(true)
			: Image()
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
		var starFile = OfferHelpers.GetStarRatingImage(rating);
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
#if ANDROID
		try
		{
			if (await Launcher.OpenAsync($"untappd://beer/{beverageId}"))
			{
				return;
			}
		}
		catch (Android.Content.ActivityNotFoundException) { }
#elif IOS
		if (await Launcher.OpenAsync($"untappd://beer/{beverageId}"))
		{
			return;
		}
#endif
		_ = await Launcher.OpenAsync($"https://untappd.com/beer/{beverageId}");
	}

	#endregion Star rating
}

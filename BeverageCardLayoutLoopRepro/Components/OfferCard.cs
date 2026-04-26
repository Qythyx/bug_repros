using System.Globalization;
using Beerbox.App.Core.Models;
using Beerbox.App.Resources.Localization;
using Beerbox.App.Theming;
using MauiReactor;
using BackendSettings = Beerbox.Service.Contracts.Database.Documents.Settings;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Components;

/// <summary>
/// Renders a single offer card with beverage image, chip overlays, and a FAB button.
/// </summary>
public sealed partial class OfferCard : Component
{
	[Prop]
	private Offer _offer = null!;

	[Prop]
	private int _quantityInOrder;

	[Prop]
	private Action _onFabTapped = null!;

	[Prop]
	private BackendSettings _backendSettings = null!;

	[Prop]
	private int _filteredCount;

	[Prop]
	private int _totalCount;

	[Prop]
	private bool _showingAll;

	[Prop]
	private Action? _onFilterChipTapped;

	public override VisualNode Render()
	{
		static OfferChip? Chip(bool condition, ImageSource image, string text) =>
			condition ? new OfferChip().ChipImage(image).ChipText(text) : null;

		var chipsOverlay = VStack(
				Chip(
					OfferHelpers.IsAlmostGone(_offer, _backendSettings),
					AppImages.OfferEnding,
					AppResources.OfferAlmostGone
				),
				Chip(_offer.Sale, AppImages.OfferSale, AppResources.OfferSale),
				Chip(_offer.Exclusive, AppImages.OfferExclusive, AppResources.OfferExclusive),
				Chip(_offer.Rare, AppImages.OfferRare, AppResources.OfferRare)
			)
			.Spacing(AppStyles.SpacingMedium)
			.Margin(AppStyles.SpacingMedium)
			.GridRow(0)
			.GridRowSpan(2);

		var overlays = new List<VisualNode> { chipsOverlay.HEnd().VStart(), RenderFab().HEnd().VEnd() };

		if (_filteredCount != _totalCount)
		{
			var hiddenCount = _totalCount - _filteredCount;
			var chipText = _showingAll
				? AppResources.OffersTotalCount.Replace("__COUNT__", _totalCount.ToString(CultureInfo.InvariantCulture))
				: AppResources.OffersHiddenCount.Replace(
					"__COUNT__",
					hiddenCount.ToString(CultureInfo.InvariantCulture)
				);
			var filterChip = Border(
					Label(chipText)
						.Margin(new Thickness(AppStyles.Spacing, 0))
						.VCenter()
						.ThemeHeader()
						.TextColor(_showingAll ? AppColors.BodyTextDimmed.Resolve : AppColors.BodyText.Resolve)
						.ID(AutomationIds.Offers.FilterChip)
				)
				.ThemeSemiTransparent()
				.HeightRequest(AppStyles.ChipIconSize)
				.HStart()
				.VStart()
				.Margin(AppStyles.SpacingMedium)
				.GridRow(0)
				.GridRowSpan(2)
				.OnTapped(() => _onFilterChipTapped?.Invoke());

			overlays.Add(filterChip);
		}

		return new BeverageCard().Offer(_offer).Overlays([.. overlays]);
	}

	private MauiReactor.Border RenderFab() =>
		Border(
				ImageButton()
					.ID(AutomationIds.Offer.Fab)
					.Source(OfferHelpers.GetFabImage(_quantityInOrder))
					.BackgroundColor(Colors.Transparent)
					.OnClicked(() => _onFabTapped?.Invoke())
			)
			.HeightRequest(AppStyles.FabSize)
			.WidthRequest(AppStyles.FabSize)
			.Padding(new Thickness(2))
			.StrokeThickness(0)
			.BackgroundColor(AppColors.Primary.Resolve)
			.Set(MauiControls.Border.StrokeShapeProperty, new MauiControls.Shapes.Ellipse())
			.HEnd()
			.VEnd()
			.Margin(AppStyles.Margin)
			.Set(VisualElement.ShadowProperty, AppStyles.CreateShadow());
}

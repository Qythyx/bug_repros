using System.Globalization;
using System.Net;
using System.Text;
using Beerbox.App.Core.Extensions;
using Beerbox.App.Core.Models;
using Beerbox.App.Core.Services;
using Beerbox.App.Extensions;
using Beerbox.App.Resources.Localization;
using Beerbox.App.Theming;
using MauiReactor;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Components;

public sealed partial class QuantitySelector : Component<QuantitySelector.MyState>
{
	public sealed record MyState
	{
		public int NewQuantity { get; set; }
		public bool IsSaving { get; set; }
	}

	private static readonly CompositeFormat YenFormat = CompositeFormat.Parse(AppResources.YenBeforeNumber);

	[Prop]
	private Offer? _offer;

	[Prop]
	private int _quantityInOrder;

	[Prop]
	private Action? _onClose;

	[Inject]
	private readonly Services.IPlatformContext _platformContext;

	[Inject]
	private readonly DataManager _dataManager;

	[Inject]
	private readonly IVibrator _vibrator;

	[Inject]
	private readonly BeerboxTelemetryService _telemetry;

	protected override void OnMountedOrPropsChanged()
	{
		base.OnMountedOrPropsChanged();
		SetState(s => s.NewQuantity = _quantityInOrder);
	}

	public override VisualNode Render()
	{
		if (_offer is null)
		{
			throw new InvalidOperationException("Offer must not be null");
		}

		var details = _offer.Beverage.LocalizedDetails;
		var newQuantity = State.NewQuantity;
		var total = newQuantity * _offer.Price.Amount;
		var quantityLimits = AppResources
			.OfferQuantityLimits.Replace("__MIN__", $"{_offer.MinQuantity}")
			.Replace("__MAX__", $"{_offer.MaxQuantity}");

		return Grid(
				"*,Auto,*",
				"*",
				// Card
				Border(
						VStack(
								Label(details.Name)
									.ID(AutomationIds.QuantitySelector.BeverageName)
									.ThemeBeverageName()
									.Margin(0, 0, AppStyles.Spacing, 0), // spacing for the close button
								HStack(
									Label(AppResources.BeverageDetailsSize),
									Label(details.Size).TextTransform(TextTransform.Default)
								),
								Grid(
										"Auto,Auto,Auto",
										"Auto,Auto,Auto",
										Label(AppResources.OfferPrice).ThemeHeader().GridRow(0).GridColumn(0),
										Label()
											.Text(
												string.Format(
													CultureInfo.CurrentCulture,
													YenFormat,
													_offer.Price.Amount
												)
											)
											.HorizontalTextAlignment(TextAlignment.End)
											.GridRow(0)
											.GridColumn(1),
										Label(AppResources.TaxIncluded)
											.TextTransform(TextTransform.Default)
											.ThemeTip()
											.GridRow(0)
											.GridColumn(2),
										Label(AppResources.OfferQuantity).ThemeHeader().GridRow(1).GridColumn(0),
										Label($"{newQuantity}")
											.ID(AutomationIds.QuantitySelector.Value)
											.HorizontalTextAlignment(TextAlignment.End)
											.GridRow(1)
											.GridColumn(1),
										Label(AppResources.OfferTotal).ThemeHeader().GridRow(2).GridColumn(0),
										Label()
											.Text(string.Format(CultureInfo.CurrentCulture, YenFormat, total))
											.HorizontalTextAlignment(TextAlignment.End)
											.GridRow(2)
											.GridColumn(1)
									)
									.HCenter(),
								Label(quantityLimits).TextTransform(TextTransform.Default).ThemeTip(),
								Grid(
									"Auto",
									"*,*",
									Button(AppResources.OfferQuantityDecrement)
										.ID(AutomationIds.QuantitySelector.Decrement)
										.FontSize(AppStyles.QuantityButtonFontSize)
										.Padding(Thickness.Zero)
										.IsEnabled(newQuantity != 0)
										.OnClicked(() => HandleChangeQuantity(-1))
										.GridColumn(0),
									Button(AppResources.OfferQuantityIncrement)
										.ID(AutomationIds.QuantitySelector.Increment)
										.FontSize(AppStyles.QuantityButtonFontSize)
										.Padding(Thickness.Zero)
										.IsEnabled(newQuantity < _offer.MaxQuantity)
										.OnClicked(() => HandleChangeQuantity(1))
										.GridColumn(1)
								),
								Grid(
									"Auto",
									"*",
									_quantityInOrder == 0
										? Button(AppResources.OfferConfirmAdd)
											.ID(AutomationIds.QuantitySelector.Add)
											.IsEnabled(newQuantity != _quantityInOrder)
											.OnClicked(HandleSaveQuantity)
										: null,
									_quantityInOrder > 0
										? Button(AppResources.OfferConfirmUpdate)
											.ID(AutomationIds.QuantitySelector.Update)
											.IsEnabled(newQuantity != _quantityInOrder)
											.OnClicked(HandleSaveQuantity)
										: null
								)
							)
							.Spacing(AppStyles.SpacingExtra)
							.Padding(new Thickness(AppStyles.SpacingExtra))
					)
					.Margin(new Thickness(AppStyles.SpacingExtra))
					.Padding(Thickness.Zero)
					.StrokeThickness(0)
					.Set(
						MauiControls.Border.StrokeShapeProperty,
						new MauiControls.Shapes.RoundRectangle
						{
							CornerRadius = new CornerRadius(AppStyles.CornerRadiusXL),
						}
					)
					.BackgroundColor(AppColors.PageBackground.Resolve)
					.Set(VisualElement.ShadowProperty, AppStyles.CreateShadow())
					.HFill()
					.VCenter()
					.GridRow(1),
				// Close button
				new FAB()
					.Shadow(Shadow().Brush(Colors.Transparent))
					.Size(AppStyles.FabCloseSize)
					.Symbol(FAB.Symbols.Close)
					.Fill(AppColors.ControlArea.Resolve)
					.Color(AppColors.BodyText.Resolve)
					.OnClicked(() => _onClose?.Invoke())
					.ViewHorizontalOptions(LayoutOptions.End)
					.ViewVerticalOptions(LayoutOptions.Start)
					.ViewMargin(AppStyles.FabCloseSize)
					.GridRow(1)
			)
			.BackgroundColor(AppColors.SemiTransparent.Resolve)
			.CascadeInputTransparent(false)
			.InputTransparent(false)
			.AbsoluteLayoutFill();
	}

	private void HandleChangeQuantity(int delta)
	{
		if (_offer is null)
		{
			throw new InvalidOperationException("Offer must not be null");
		}

		_vibrator.GenerateSelectionVibration();
		var clamped = OfferHelpers.ClampQuantity(State.NewQuantity + delta, _offer);
		SetState(s => s.NewQuantity = clamped);
	}

	private async void HandleSaveQuantity()
	{
		if (_offer is null)
		{
			throw new InvalidOperationException("Offer must not be null");
		}

		SetState(s => s.IsSaving = true);
		_vibrator.GenerateSuccessVibration();
		var desiredQuantity = State.NewQuantity;
		var adjustment = OfferHelpers.GetAdjustmentType(_quantityInOrder, desiredQuantity);
		var result = await ActivityOverlay.ShowWhile(
			ContainerPage,
			() =>
				_dataManager
					.SetOfferQuantityAsync(_offer, desiredQuantity)
					.HandleException(_telemetry, new(HttpStatusCode.InternalServerError)),
			true
		);

		if (result == null)
		{
			SetState(s => s.IsSaving = false);
			return;
		}

		var adjustmentResult = BeerboxTelemetryService.OrderItemAdjustmentResult.Success;
		var newQuantityInOrder = _dataManager.Order.Items.FirstOrDefault(i => i.Offer.ID == _offer.ID)?.Quantity ?? 0;
		if (result.Status == HttpStatusCode.PreconditionFailed)
		{
			adjustmentResult = BeerboxTelemetryService.OrderItemAdjustmentResult.NoInventory;
			await ShowAlert(AppResources.OfferNoInventoryTitle, AppResources.OfferNoInventory);
		}
		else if (!result.IsSuccessStatusCode)
		{
			adjustmentResult = BeerboxTelemetryService.OrderItemAdjustmentResult.Problem;
			await ShowAlert(AppResources.OfferProblemAddingTitle, AppResources.OfferProblemAdding);
		}
		else if (desiredQuantity > newQuantityInOrder)
		{
			adjustmentResult = BeerboxTelemetryService.OrderItemAdjustmentResult.InsufficientInventory;
			await ShowAlert(
				AppResources.OfferInsufficientInventoryTitle,
				AppResources.OfferInsufficientInventory.Replace("__NUMBER__", $"{newQuantityInOrder}")
			);
		}

		_telemetry.TrackOrderItems(_offer.ID, adjustment, adjustmentResult);
		SetState(s => s.IsSaving = false);
		_onClose?.Invoke();
	}

	private async Task ShowAlert(string title, string message) =>
		await _platformContext.DisplayAlertAsync(title, message, AppResources.ButtonAccept);
}

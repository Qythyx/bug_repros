using Beerbox.App.Core.Services;
using Beerbox.Service.Contracts.Database.Documents;

namespace Beerbox.App.Core.Models;

/// <summary>
/// Pure helper functions for offer-related business logic, extracted from UI components for testability.
/// </summary>
public static class OfferHelpers
{
	/// <summary>
	/// Determines whether an offer is almost gone based on time remaining and available inventory.
	/// </summary>
	/// <param name="offer">The offer to check.</param>
	/// <param name="settings">The backend settings containing threshold values.</param>
	public static bool IsAlmostGone(Offer offer, Settings settings) =>
		offer.Ends > DateTime.Now
		&& (
			offer.Ends.Subtract(DateTime.Now).TotalHours < settings.OfferAlmostGoneHours
			|| offer.AvailableQuantity < settings.OfferAlmostGoneInventory
		);

	/// <summary>
	/// Returns the FAB image filename based on the quantity currently in the order.
	/// </summary>
	/// <param name="quantityInOrder">The quantity of this offer in the current order.</param>
	public static string GetFabImage(int quantityInOrder) =>
		quantityInOrder switch
		{
			0 => "icon_fab_add.png",
			> 0 and < 10 => $"icon_fab_edit_{quantityInOrder}.png",
			_ => "icon_fab_edit_9plus.png",
		};

	/// <summary>
	/// Returns the star rating image filename for a given rating value.
	/// Ratings are rounded to the nearest 0.25 increment.
	/// </summary>
	/// <param name="rating">The rating value (typically 0.0 to 5.0).</param>
	public static string GetStarRatingImage(double rating) =>
		$"rating_star_{Math.Round(rating * 4) * 0.25:0.00}".Replace(".", "_") + ".png";

	/// <summary>
	/// Clamps a desired quantity to the valid range [offer.MinQuantity, offer.MaxQuantity], with special
	/// handling: values between 0 (exclusive) and MinQuantity snap up to MinQuantity, while
	/// values at or below 0 become 0 (allowing removal).
	/// </summary>
	/// <param name="desired">The desired quantity after adjustment.</param>
	/// <param name="offer">The offer whose min/max quantity constraints apply.</param>
	public static int ClampQuantity(int desired, Offer offer) =>
		desired switch
		{
			_ when desired >= offer.MinQuantity && desired <= offer.MaxQuantity => desired,
			_ when desired > 0 && desired < offer.MinQuantity => offer.MinQuantity,
			_ when desired > offer.MaxQuantity => offer.MaxQuantity,
			_ => 0,
		};

	/// <summary>
	/// Determines the telemetry adjustment type based on old and new quantities.
	/// </summary>
	/// <param name="oldQuantity">The quantity before the change.</param>
	/// <param name="newQuantity">The quantity after the change.</param>
	public static BeerboxTelemetryService.OrderItemAdjustment GetAdjustmentType(int oldQuantity, int newQuantity) =>
		oldQuantity == 0 ? BeerboxTelemetryService.OrderItemAdjustment.AddItem
		: newQuantity == 0 ? BeerboxTelemetryService.OrderItemAdjustment.RemoveItem
		: newQuantity > oldQuantity ? BeerboxTelemetryService.OrderItemAdjustment.IncreaseItem
		: BeerboxTelemetryService.OrderItemAdjustment.DecreaseItem;
}

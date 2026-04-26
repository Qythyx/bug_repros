namespace Beerbox.Service.Contracts.Notifications;

/// <summary>
/// The supported notification tags.
/// Because this enum is Flags, you should not change the value of these, since that will
/// change the meaning of any existing device registrations. You can add new values, though.
/// <para>
/// Tag docs: https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-tags-segment-push-message.
/// Tags are used to target a subset of app instances; only those that register for
/// specific tags. In the future we could possibly have tags for styles (lager, porter, etc)
/// and target those only.
/// </para>
/// </summary>
[Flags]
public enum NotificationTag
{
	None = 0,

	/// <summary>
	/// Designates a message that is only for testing purposes.
	/// Devices can opt in to receive these via the hidden settings.
	/// </summary>
	Test = 1 << 0,
	Announcement = 1 << 1,
	Offer = 1 << 2,

	/// <summary>Per-BeverageKind offer notification tags.</summary>
	/// <remarks>Devices register for the kinds they want; the backend sends to the specific kind tag.</remarks>
	OfferBeer = 1 << 3,
	OfferCider = 1 << 4,
	OfferGin = 1 << 5,
	OfferMead = 1 << 6,
	OfferVodka = 1 << 7,
	OfferWhisky = 1 << 8,
	OfferWine = 1 << 9,
}

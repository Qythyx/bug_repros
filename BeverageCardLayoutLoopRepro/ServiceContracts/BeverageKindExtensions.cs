using Beerbox.Service.Contracts.Notifications;

namespace Beerbox.Service.Contracts;

/// <summary>
/// Maps between <see cref="BeverageKind"/> and per-kind <see cref="NotificationTag"/> flags.
/// </summary>
public static class BeverageKindExtensions
{
	/// <summary>
	/// Converts a <see cref="BeverageKind"/> to its corresponding <see cref="NotificationTag"/>.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kind"/> is not a recognized value.</exception>
	extension(BeverageKind kind)
	{
		public NotificationTag NotificationTag =>
			kind switch
			{
				BeverageKind.Beer => NotificationTag.OfferBeer,
				BeverageKind.Cider => NotificationTag.OfferCider,
				BeverageKind.Gin => NotificationTag.OfferGin,
				BeverageKind.Mead => NotificationTag.OfferMead,
				BeverageKind.Vodka => NotificationTag.OfferVodka,
				BeverageKind.Whisky => NotificationTag.OfferWhisky,
				BeverageKind.Wine => NotificationTag.OfferWine,
				_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
			};
	}
}

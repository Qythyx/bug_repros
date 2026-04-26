using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Documents;

public record Settings(
	int NonFreeShippingFee,
	int ShippingFreeThreshold,
	int OrderClosingSoonHours,
	string EmailAddress,
	string EmailName,
	int SessionLifetimeDays,
	int MinScheduledClosingsWindowDays,
	int MinOrderCreationBeforeNextCloseDays,
	int DefaultShipmentDelayDays,
	int OfferAlmostGoneInventory,
	int OfferAlmostGoneHours,
	int OrderEndingSoonHours,
	string MinimumAppVersionRequired,
	string ETag = null!
) : CosmosDBDocument(SettingsID, ETag)
{
	[JsonIgnore]
	public const string SettingsID = "global";

	[JsonIgnore]
	public static readonly Settings Default = new(
		1000,
		11000,
		48,
		"order@beercats.jp",
		"beerbox",
		30,
		31,
		5,
		3,
		6,
		24,
		72,
		"1.0.0.0"
	);

	[JsonIgnore]
	public Settings ApplyDefaults =>
		this with
		{
			EmailAddress = string.IsNullOrEmpty(EmailAddress) ? Default.EmailAddress : EmailAddress,
			EmailName = string.IsNullOrEmpty(EmailName) ? Default.EmailName : EmailName,
			SessionLifetimeDays = SessionLifetimeDays == 0 ? Default.SessionLifetimeDays : SessionLifetimeDays,
			MinScheduledClosingsWindowDays =
				MinScheduledClosingsWindowDays == 0
					? Default.MinScheduledClosingsWindowDays
					: MinScheduledClosingsWindowDays,
			MinOrderCreationBeforeNextCloseDays =
				MinOrderCreationBeforeNextCloseDays == 0
					? Default.MinOrderCreationBeforeNextCloseDays
					: MinOrderCreationBeforeNextCloseDays,
			DefaultShipmentDelayDays =
				DefaultShipmentDelayDays == 0 ? Default.DefaultShipmentDelayDays : DefaultShipmentDelayDays,
			OfferAlmostGoneInventory =
				OfferAlmostGoneInventory == 0 ? Default.OfferAlmostGoneInventory : OfferAlmostGoneInventory,
			OfferAlmostGoneHours = OfferAlmostGoneHours == 0 ? Default.OfferAlmostGoneHours : OfferAlmostGoneHours,
			OrderEndingSoonHours = OrderEndingSoonHours == 0 ? Default.OrderEndingSoonHours : OrderEndingSoonHours,
		};
}

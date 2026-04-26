namespace Beerbox.Service.Contracts;

/// <summary>
/// All Stripe errors are defined at https://stripe.com/docs/error-codes
/// </summary>
public enum PaymentMethodStatus
{
	/// <summary>This should be first so deserializing an object with no PaymentMethodStatus gets this value.</summary>
	Unknown = 0,

	/// <summary>The card was declined.</summary>
	CardDeclined = 1,

	/// <summary>The card has expired.</summary>
	ExpiredCard = 2,

	/// <summary>The card's security code is incorrect.</summary>
	IncorrectCvc = 3,

	/// <summary>The card's security code is invalid.</summary>
	InvalidCvc = 4,

	/// <summary>The card number is incorrect.</summary>
	IncorrectNumber = 5,

	/// <summary>The card number is not a valid credit card number.</summary>
	InvalidNumber = 6,

	/// <summary>The card's expiration month is invalid.</summary>
	InvalidExpiryMonth = 7,

	/// <summary>The card's expiration year is invalid.</summary>
	InvalidExpiryYear = 8,

	/// <summary>The card's zip code failed validation.</summary>
	IncorrectZip = 9,

	/// <summary>Status is ok without any problems.</summary>
	OK = 10,

	/// <summary>There is no card on a customer that is being charged.</summary>
	Missing = 11,

	/// <summary>An error occurred while processing the card.</summary>
	ProcessingError = 12,

	/// <summary>An error occurred due to requests hitting the API too quickly. Please let us know if you're consistently running into this error.</summary>
	RateLimit = 13,
}

namespace Beerbox.App.Core.Exceptions;

public sealed class OfferNotAvailableException : DatabaseItemException
{
	public OfferNotAvailableException() { }

	public OfferNotAvailableException(string message)
		: base(message) { }

	public OfferNotAvailableException(string message, Exception innerException)
		: base(message, innerException) { }

	public static OfferNotAvailableException ForId(string id) =>
		new($"Offer with ID '{id}' is not available.") { ID = id };
}

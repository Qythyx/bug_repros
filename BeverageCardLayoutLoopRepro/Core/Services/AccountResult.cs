namespace Beerbox.App.Core.Services;

public enum AccountResult
{
	ContactInformationInvalid = 0,
	CreditCardInvalid = 1,
	EmailAlreadyExists = 2,
	Forbidden = 3,
	ShippingAddressInvalid = 4,
	Success = 5,
	UnknownError = 6,
}

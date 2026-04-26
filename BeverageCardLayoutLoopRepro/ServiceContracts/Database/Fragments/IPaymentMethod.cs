namespace Beerbox.Service.Contracts.Database.Fragments;

public interface IPaymentMethod
{
	PaymentMethodType Type { get; }
}

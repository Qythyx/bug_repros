using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Fragments;

public record CreditCard(
	string NameOnCard,
	string CardNumber,
	string Month,
	string Year,
	string VerificationCode,
	Address Address
) : IPaymentMethod
{
	[JsonIgnore]
	public PaymentMethodType Type { get; } = PaymentMethodType.CreditCard;
}

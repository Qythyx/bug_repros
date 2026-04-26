using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Database.Fragments;

public record COD : IPaymentMethod
{
	[JsonIgnore]
	public PaymentMethodType Type { get; } = PaymentMethodType.COD;
}

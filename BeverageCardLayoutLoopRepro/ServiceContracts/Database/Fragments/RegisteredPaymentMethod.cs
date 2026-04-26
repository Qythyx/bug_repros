
namespace Beerbox.Service.Contracts.Database.Fragments;

public record RegisteredPaymentMethod(PaymentMethodType Type, string? ID, PaymentMethodStatus Status);

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.Service.Contracts.Database;

public sealed class PaymentMethodConverter : JsonConverter<IPaymentMethod>
{
	[UnconditionalSuppressMessage(
		"Trimming",
		"IL2026",
		Justification = "CreditCard and COD are concrete reference types in this assembly and are preserved."
	)]
	[UnconditionalSuppressMessage(
		"AOT",
		"IL3050",
		Justification = "CreditCard and COD are concrete reference types in this assembly and are preserved."
	)]
	public override IPaymentMethod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;
		return root.TryGetProperty(nameof(CreditCard.NameOnCard), out _)
			? root.Deserialize<CreditCard>(options)
			: root.Deserialize<COD>(options);
	}

	[UnconditionalSuppressMessage(
		"Trimming",
		"IL2026",
		Justification = "CreditCard and COD are concrete reference types in this assembly and are preserved."
	)]
	[UnconditionalSuppressMessage(
		"AOT",
		"IL3050",
		Justification = "CreditCard and COD are concrete reference types in this assembly and are preserved."
	)]
	public override void Write(Utf8JsonWriter writer, IPaymentMethod value, JsonSerializerOptions options)
	{
		switch (value)
		{
			case CreditCard card:
				JsonSerializer.Serialize(writer, card, options);
				break;
			case COD cod:
				JsonSerializer.Serialize(writer, cod, options);
				break;
			default:
				throw new JsonException($"Unsupported payment method type: {value.GetType()}");
		}
	}
}

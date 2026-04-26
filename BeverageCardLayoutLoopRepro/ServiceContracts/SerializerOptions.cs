using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts;

/// <summary>
/// Single <see cref="JsonSerializerOptions"/> instance used across the service and app.
/// <para>
/// Design decisions:
/// </para>
/// <list type="bullet">
/// <item><b>String enums</b> via <see cref="JsonStringEnumConverter"/> — self-documenting in logs
/// and Cosmos documents. <see cref="FlagsAttribute"/> enums serialize as comma-separated strings
/// (e.g. <c>"Announcement, OfferBeer"</c>). The converter is tolerant on read and still accepts
/// integer values, which keeps legacy Cosmos documents readable until the migration in
/// <c>AdministrationManager.RewriteEnumsAsStringsAsync</c> rewrites them.</item>
/// <item><b>DateTimes normalized to UTC</b> via <see cref="DateTimeKindConverter"/> — every
/// <see cref="DateTime"/> that comes off the wire is coerced to <see cref="DateTimeKind.Utc"/>.
/// Callers that need local time for display call <see cref="DateTime.ToLocalTime"/> explicitly.</item>
/// <item><b>Pascal casing, case-insensitive reads, no ignore-on-null, relaxed encoder</b> —
/// preserves the existing wire contract with the React webapp and keeps characters like <c>+</c>
/// unescaped in phone numbers and URLs.</item>
/// </list>
/// </summary>
public static class SerializerOptions
{
	public static JsonSerializerOptions Default { get; } = Build();

	private static JsonSerializerOptions Build()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = null,
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never,
			WriteIndented = false,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		};
		options.Converters.Add(new DateTimeKindConverter());
		options.Converters.Add(new JsonStringEnumConverter());
		return options;
	}
}

/// <summary>
/// Coerces every deserialized <see cref="DateTime"/> to <see cref="DateTimeKind.Utc"/>. Unzoned
/// strings (no trailing <c>Z</c> or offset) are treated as UTC; zoned strings are converted via
/// <see cref="DateTime.ToUniversalTime"/>. On write, the value is emitted using STJ's default
/// format (ISO 8601 with <c>Z</c> for UTC kinds).
/// </summary>
internal sealed class DateTimeKindConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetDateTime();
		return value.Kind == DateTimeKind.Unspecified
			? DateTime.SpecifyKind(value, DateTimeKind.Utc)
			: value.ToUniversalTime();
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value);
}

using System.Text.Json.Serialization;

namespace Beerbox.App.Core.Models;

public record BeverageDetails
{
	public string Name { get; init; }
	public string Description { get; init; }
	public string Maker { get; init; }

	[JsonPropertyName(nameof(Tags))]
	public string TagString { get; init; }

	[JsonIgnore]
	public IReadOnlyList<string> Tags =>
		field ??= [.. TagString.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())];

	public string Style { get; init; }
	public string Container { get; init; }
	public string Size { get; init; }

	public BeverageDetails(Service.Contracts.Database.Documents.BeverageDetails document)
		: this(
			document.Name,
			document.Description,
			document.Maker,
			document.Tags,
			document.Style,
			document.Container,
			document.Size
		) { }

	[JsonConstructor]
	private BeverageDetails(
		string name,
		string description,
		string maker,
		string tagString,
		string style,
		string container,
		string size
	)
	{
		Name = name;
		Description = description;
		Maker = maker;
		TagString = tagString;
		Style = style;
		Container = container;
		Size = size;
	}
}

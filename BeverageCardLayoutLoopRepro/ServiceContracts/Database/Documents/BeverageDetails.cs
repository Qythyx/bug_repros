
namespace Beerbox.Service.Contracts.Database.Documents;

public record BeverageDetails(
	string Name,
	string Description,
	string Maker,
	string Tags,
	string Style,
	string Container,
	string Size
);

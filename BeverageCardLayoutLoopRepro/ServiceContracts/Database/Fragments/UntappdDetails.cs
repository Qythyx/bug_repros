
namespace Beerbox.Service.Contracts.Database.Fragments;

public record UntappdDetails(int BeverageID, double GlobalRating, int RatingCount, DateTime LastUpdated = default);

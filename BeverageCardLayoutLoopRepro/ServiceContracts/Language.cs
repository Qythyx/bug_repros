
namespace Beerbox.Service.Contracts;

public enum Language
{
	English = 0,
	Japanese = 1,
}

public static class LanguageExtensions
{
	public static string GetTwoLetterISO(this Language language) =>
		language switch
		{
			Language.English => "en",
			Language.Japanese => "ja",
			_ => "ja",
		};
}

using Beerbox.Service.Contracts;
using Foundation;

namespace Beerbox.App.Core.Services;

public static class DefaultLanguageProvider
{
	public static Language DefaultLanguage =>
		NSLocale.PreferredLanguages.Select(l => l[..2].ToLowerInvariant()).FirstOrDefault(c => c == "en" || c == "ja")
		== "en"
			? Language.English
			: Language.Japanese;
}

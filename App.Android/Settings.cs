using System.Globalization;
using BeerApp.ServiceDetails;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeerApp.App.Droid.Settings))]
namespace BeerApp.App.Droid
{
	public class Settings : Forms.Services.Settings
	{
		public Settings() : base(GetDefaultLanguage()) { }

		private static Language GetDefaultLanguage()
		{
			var locales = MainActivity.CurrentMainActivity!.Resources?.Configuration?.Locales;
			for(var i = 0; i < locales?.Size(); i++) {
				var language = new CultureInfo(locales!.Get(i)!.Language).TwoLetterISOLanguageName;
				if(language == "en") {
					return Language.English;
				}
			}
			return Language.Japanese;
		}
	}
}

using Beerbox.App.Core.Services;
using Beerbox.Service.Contracts;

namespace Beerbox.App.Services;

public class BeerboxAppSettings(Language defaultLanguage) : AppSettings(defaultLanguage)
{
	protected override bool GetBoolean(string name, bool defaultValue) => Preferences.Get(name, defaultValue);

	protected override void SetBoolean(string name, bool value) => Preferences.Set(name, value);

	protected override DateTime GetDateTime(string name, DateTime defaultValue) => Preferences.Get(name, defaultValue);

	protected override void SetDateTime(string name, DateTime value) => Preferences.Set(name, value);

	protected override int GetInt(string name, int defaultValue) => Preferences.Get(name, defaultValue);

	protected override void SetInt(string name, int value) => Preferences.Set(name, value);

	protected override string? GetString(string name, string? defaultValue) => Preferences.Get(name, defaultValue);

	protected override void SetString(string name, string? value) => Preferences.Set(name, value);
}

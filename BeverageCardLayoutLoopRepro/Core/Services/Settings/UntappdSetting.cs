namespace Beerbox.App.Core.Services.Settings;

[Flags]
public enum UntappdSetting
{
	None = 0,
	GlobalRating = 1,
	PersonalRating = 1 << 1,
}

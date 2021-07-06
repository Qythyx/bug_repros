using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace BeerApp.App.Droid
{
	[Activity(
		Theme = "@style/MainTheme",
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.FontScale,
		LaunchMode = LaunchMode.SingleTop,
		ScreenOrientation = ScreenOrientation.Portrait
	)]
	[IntentFilter(
		new[] { Intent.ActionView },
		Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
		DataScheme = "beerbox",
		DataHost = "open"
	)]
	public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		internal static MainActivity? CurrentMainActivity;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			CurrentMainActivity = this;
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			Xamarin.Forms.Forms.Init(this, savedInstanceState);
			LoadApplication(new Forms.App());
		}
	}
}
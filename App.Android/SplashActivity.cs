using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;

namespace BeerApp.App.Droid
{
	[Activity(
		Icon = "@mipmap/icon",
		MainLauncher = true,
		NoHistory = true,
		RoundIcon = "@mipmap/icon_round",
		ScreenOrientation = ScreenOrientation.Portrait,
		Theme = "@style/MyTheme.Splash"
	)]
	public class SplashActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			if(Window != null && Window.DecorView != null) {
				if(Build.VERSION.SdkInt >= (BuildVersionCodes)30) {
#pragma warning disable XA0001 // Find issues with Android API usage
					Window.SetDecorFitsSystemWindows(false);
#pragma warning restore XA0001 // Find issues with Android API usage
				} else {
#pragma warning disable CS0618 // Type or member is obsolete
					var uiOptions = (int)Window.DecorView.SystemUiVisibility
					| (int)SystemUiFlags.LowProfile
					| (int)SystemUiFlags.Fullscreen
					| (int)SystemUiFlags.HideNavigation
					| (int)SystemUiFlags.ImmersiveSticky;
					Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
#pragma warning restore CS0618 // Type or member is obsolete
				}
			}

			StartActivity(new Intent(Application.Context, typeof(MainActivity)));
			Finish();
		}

		// Prevent the back button from canceling the startup process
		public override void OnBackPressed() { }
	}
}
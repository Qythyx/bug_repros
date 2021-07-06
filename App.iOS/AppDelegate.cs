using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeerApp.App.iOS.AppDelegate))]
namespace BeerApp.App.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			Xamarin.Forms.Forms.Init();
			PanCardView.iOS.CardsViewRenderer.Preserve();
			LoadApplication(new Forms.App());
			return base.FinishedLaunching(app, options);
		}
	}
}

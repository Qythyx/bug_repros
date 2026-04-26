using Foundation;

namespace Beerbox.App;

#pragma warning disable CA1711
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
#pragma warning restore CA1711
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

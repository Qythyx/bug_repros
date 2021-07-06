using BeerApp.App.Forms.Views;
using Xamarin.Forms;

namespace BeerApp.App.Forms
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
			MainPage = new ShellMenu();
			_ = ((ShellMenu)MainPage).GoToAsync();
		}

		public static App CurrentApp => (App)Current;
	}
}

using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BeerApp.App.Forms.Views
{
	public partial class ShellMenu : Shell
	{
		public ShellMenu()
		{
			BindingContext = this;
			NavigateTo = new Command(contentPage => {
				FlyoutIsPresented = false;
				var page = (ShellContent)contentPage;
				_ = GoToAsync(page);
			});
			InitializeComponent();
		}

		public async Task GoToAsync()
			=> await GoToAsync(OffersContent);

		private async Task GoToAsync(ShellContent content)
			=> await Current.GoToAsync($"//{content.Route}", false);

		public ICommand NavigateTo { get; }

		public static ShellMenu CurrentShell => (ShellMenu)Current;
	}
}

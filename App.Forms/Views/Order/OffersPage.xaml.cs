using BeerApp.Core.ViewModels;

namespace BeerApp.App.Forms.Views
{
	public partial class OffersPage
	{
		public static OffersPage? CurrentInstance { get; private set; }

		public OffersPage() : base(page => CreateBindingContext((OffersPage)page))
		{
			InitializeComponent();
			CurrentInstance = this;
		}

		private static OffersViewModel CreateBindingContext(OffersPage page)
			=> new(Xamarin.Forms.Device.BeginInvokeOnMainThread);

		protected override void OnAppearing()
		{
			base.OnAppearing();
			TypedContext.IsRefreshing = true;
		}
	}
}

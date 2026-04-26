using System.Runtime.CompilerServices;
using Beerbox.App.Components;
using R = MauiReactor;

namespace Beerbox.App;

public readonly struct AutoId<TControl>(string value)
{
	private readonly string _value = value;

	public static implicit operator string(AutoId<TControl> id) => id._value;
}

public static class AutomationIds
{
	public static class TitleView
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(TitleView)}_{name}");

		public static readonly AutoId<R.Image> Back = GetID<R.Image>();
		public static readonly AutoId<R.Image> HamburgerMenu = GetID<R.Image>();
		public static readonly AutoId<R.Image> Offers = GetID<R.Image>();
		public static readonly AutoId<R.Image> Summary = GetID<R.Image>();
	}

	public static class Shell
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Shell)}_{name}");

		public static readonly AutoId<R.ShellContent> About = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> ContactUs = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> CreateAccount = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> EditAccount = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> HowItWorks = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> Login = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> Logout = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> Offers = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> OrderHistory = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> Settings = GetID<R.ShellContent>();
		public static readonly AutoId<R.ShellContent> Summary = GetID<R.ShellContent>();
	}

	public static class HowItWorks
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(HowItWorks)}_{name}");

		public static readonly AutoId<PanCarouselView> Carousel = GetID<PanCarouselView>();
		public static readonly AutoId<R.Button> Button = GetID<R.Button>();
		public static readonly AutoId<R.Grid> Panel = GetID<R.Grid>();
	}

	public static class Offers
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Offers)}_{name}");

		public static readonly AutoId<PanCarouselView> Carousel = GetID<PanCarouselView>();
		public static readonly AutoId<R.VStack> Empty = GetID<R.VStack>();
		public static readonly AutoId<R.Label> FilterChip = GetID<R.Label>();
		public static readonly AutoId<R.VStack> FilteredEmpty = GetID<R.VStack>();
	}

	public static class Offer
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Offer)}_{name}");

		public static readonly AutoId<R.ImageButton> Fab = GetID<R.ImageButton>();
		public static readonly AutoId<R.Label> BeverageName = GetID<R.Label>();
		public static readonly AutoId<R.Label> Maker = GetID<R.Label>();
		public static readonly AutoId<R.Label> GlobalRating = GetID<R.Label>();
		public static readonly AutoId<R.Label> PersonalRating = GetID<R.Label>();
	}

	public static class QuantitySelector
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") =>
			new($"{nameof(QuantitySelector)}_{name}");

		public static readonly AutoId<R.Label> BeverageName = GetID<R.Label>();
		public static readonly AutoId<R.Label> Value = GetID<R.Label>();
		public static readonly AutoId<R.Button> Decrement = GetID<R.Button>();
		public static readonly AutoId<R.Button> Increment = GetID<R.Button>();
		public static readonly AutoId<R.Button> Add = GetID<R.Button>();
		public static readonly AutoId<R.Button> Update = GetID<R.Button>();
		public static readonly AutoId<R.Button> Close = GetID<R.Button>();
	}

	public static class Login
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Login)}_{name}");

		public static readonly AutoId<R.Entry> Email = GetID<R.Entry>();
		public static readonly AutoId<R.Label> AccountUnknown = GetID<R.Label>();
		public static readonly AutoId<R.Label> HasProblem = GetID<R.Label>();
		public static readonly AutoId<R.Button> Submit = GetID<R.Button>();
		public static readonly AutoId<R.Button> GotoCreateAccount = GetID<R.Button>();
	}

	public static class CreateAccount
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") =>
			new($"{nameof(CreateAccount)}_{name}");

		public static readonly AutoId<R.DatePicker> Birthdate = GetID<R.DatePicker>();
		public static readonly AutoId<R.RadioButton> CreditCard = GetID<R.RadioButton>();
		public static readonly AutoId<R.RadioButton> COD = GetID<R.RadioButton>();
		public static readonly AutoId<R.Switch> AgreeToTerms = GetID<R.Switch>();
		public static readonly AutoId<R.Label> Problem = GetID<R.Label>();
		public static readonly AutoId<R.Button> Submit = GetID<R.Button>();
	}

	public static class ContactInfo
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(ContactInfo)}_{name}");

		public static readonly AutoId<R.Entry> Email = GetID<R.Entry>();
		public static readonly AutoId<R.Entry> Name = GetID<R.Entry>();
		public static readonly AutoId<R.Entry> Phone = GetID<R.Entry>();
	}

	public static class Address
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Address)}_{name}");

		public static readonly AutoId<R.Entry> PostalCode = GetID<R.Entry>();
		public static readonly AutoId<R.Picker> Prefecture = GetID<R.Picker>();
		public static readonly AutoId<R.Entry> Local1 = GetID<R.Entry>();
		public static readonly AutoId<R.Entry> Local2 = GetID<R.Entry>();
	}

	public static class EditAccount
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(EditAccount)}_{name}");

		public static readonly AutoId<R.Button> EditContactInfo = GetID<R.Button>();
		public static readonly AutoId<R.Button> EditShippingAddress = GetID<R.Button>();
		public static readonly AutoId<R.Button> EditPaymentMethod = GetID<R.Button>();
		public static readonly AutoId<R.Button> Save = GetID<R.Button>();
		public static readonly AutoId<R.Button> Cancel = GetID<R.Button>();
		public static readonly AutoId<R.RadioButton> CreditCard = GetID<R.RadioButton>();
		public static readonly AutoId<R.RadioButton> COD = GetID<R.RadioButton>();
	}

	public static class Order
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Order)}_{name}");

		public static readonly AutoId<R.Label> EndDate = GetID<R.Label>();
		public static readonly AutoId<R.Label> ShipDate = GetID<R.Label>();
		public static readonly AutoId<R.Label> NotLoggedIn = GetID<R.Label>();
		public static readonly AutoId<R.Button> GoToLogin = GetID<R.Button>();
		public static readonly AutoId<R.Label> CountDistinct = GetID<R.Label>();
		public static readonly AutoId<R.Label> CountTotal = GetID<R.Label>();
		public static readonly AutoId<R.Label> PriceBeverages = GetID<R.Label>();
		public static readonly AutoId<R.Label> PriceTotal = GetID<R.Label>();
		public static readonly AutoId<R.Button> ShowChosenBeverages = GetID<R.Button>();
	}

	public static class OrderHistory
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") =>
			new($"{nameof(OrderHistory)}_{name}");

		public static readonly AutoId<R.Grid> List = GetID<R.Grid>();
		public static readonly AutoId<R.Label> FirstRow = GetID<R.Label>();
		public static readonly AutoId<R.Label> Empty = GetID<R.Label>();
		public static readonly AutoId<R.Picker> Year = GetID<R.Picker>();
	}

	public static class Settings
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Settings)}_{name}");

		public static readonly AutoId<R.Picker> Language = GetID<R.Picker>();
		public static readonly AutoId<R.Picker> Theme = GetID<R.Picker>();
		public static readonly AutoId<R.CheckBox> UntappdGlobal = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> UntappdPersonal = GetID<R.CheckBox>();
		public static readonly AutoId<R.Button> UntappdLogin = GetID<R.Button>();
		public static readonly AutoId<R.CheckBox> BeverageFilterBeer = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> BeverageFilterCider = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> BeverageFilterGin = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> BeverageFilterMead = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> BeverageFilterVodka = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> BeverageFilterWhisky = GetID<R.CheckBox>();
		public static readonly AutoId<R.CheckBox> BeverageFilterWine = GetID<R.CheckBox>();
	}

	public static class About
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(About)}_{name}");

		public static readonly AutoId<R.Label> AppVersion = GetID<R.Label>();
		public static readonly AutoId<R.Label> TermsBody = GetID<R.Label>();
		public static readonly AutoId<R.Label> Copyright = GetID<R.Label>();
	}

	public static class ContactUs
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(ContactUs)}_{name}");

		public static readonly AutoId<R.Entry> Subject = GetID<R.Entry>();
		public static readonly AutoId<R.Editor> Message = GetID<R.Editor>();
		public static readonly AutoId<R.Button> Send = GetID<R.Button>();
	}

	public static class WaitForVerification
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") =>
			new($"{nameof(WaitForVerification)}_{name}");

		public static readonly AutoId<R.Button> Cancel = GetID<R.Button>();
		public static readonly AutoId<R.Grid> Indicator = GetID<R.Grid>();
	}

	public static class Connectivity
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") =>
			new($"{nameof(Connectivity)}_{name}");

		public static readonly AutoId<R.Border> Banner = GetID<R.Border>();
	}

	public static class ActivityOverlay
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") =>
			new($"{nameof(ActivityOverlay)}_{name}");

		public static readonly AutoId<R.VStack> Overlay = GetID<R.VStack>();
		public static readonly AutoId<R.ActivityIndicator> Indicator = GetID<R.ActivityIndicator>();
	}

	public static class OldVersion
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(OldVersion)}_{name}");

		public static readonly AutoId<R.Label> Explanation = GetID<R.Label>();
		public static readonly AutoId<R.Button> Update = GetID<R.Button>();
	}
}

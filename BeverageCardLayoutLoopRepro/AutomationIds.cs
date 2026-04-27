using System.Runtime.CompilerServices;
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
	}

	public static class Shell
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Shell)}_{name}");

		public static readonly AutoId<R.ShellContent> OrderHistory = GetID<R.ShellContent>();
	}

	public static class Offer
	{
		private static AutoId<T> GetID<T>([CallerMemberName] string? name = "") => new($"{nameof(Offer)}_{name}");

		public static readonly AutoId<R.Label> BeverageName = GetID<R.Label>();
		public static readonly AutoId<R.Label> Maker = GetID<R.Label>();
		public static readonly AutoId<R.Label> GlobalRating = GetID<R.Label>();
	}
}

using System.Text.RegularExpressions;
using Beerbox.Service.Contracts;

namespace Beerbox.App.Core.Models;

public partial record Address(string PostalCode, string Prefecture, string Local1, string Local2)
{
	internal Service.Contracts.Database.Fragments.Address ToDocument() => new(PostalCode, Prefecture, Local1, Local2);

	internal static Address FromDocument(Service.Contracts.Database.Fragments.Address address) =>
		new(address.PostalCode, address.Prefecture, address.Local1, address.Local2);

	public static Address Empty { get; } = new("", "", "", "");

	#region Address Checker
	/// <summary>
	/// Checks if all fields in the <see cref="Address"/> are valid.
	/// </summary>
	/// <param name="address">The <see cref="Address"/>.</param>
	/// <returns><c>true</c> if the <see cref="Address"/> is valid;
	/// otherwise <c>false</c>.</returns>
	public static bool IsValid(Address address) =>
		IsPostalCodeValid(address.PostalCode)
		&& IsPrefectureValid(address.Prefecture)
		&& (IsLocalValid(address.Local1) || IsLocalValid(address.Local2));
	#endregion Address Checker

	#region Local Address Checker
	/// <summary>
	/// Checks if the given text is a partial valid local address.
	/// To be used during input. This does not do any actual address
	/// checking and is always true.
	/// </summary>
	public static Func<string, bool> IsLocalInputValid { get; } = (_) => true;

	/// <summary>
	/// Checks if the given text is a valid local address.
	/// This does not do any actual address
	/// checking and just confirms there is something entered.
	/// </summary>
	/// <param name="local">The local address to validate.</param>
	public static bool IsLocalValid(string local) => local.Length > 0;
	#endregion Local Address Checker

	#region Postal Code Checker
	/// <summary>
	/// Checks if the given text is a partial valid postal code prefix.
	/// To be used during input.
	/// </summary>
	public static Func<string, bool> IsPostalCodeInputValid { get; } =
		text => PostalCodeInputValidChecker().IsMatch(text);

	[GeneratedRegex("^\\d{0,3}$|^\\d{3}-\\d{0,4}$")]
	private static partial Regex PostalCodeInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid postal code format.
	/// This does not attempt to verify it is a known Japanese postal code.
	/// </summary>
	/// <param name="code">The postal code to validate.</param>
	public static bool IsPostalCodeValid(string code) => PostalCodeValidChecker().IsMatch(code);

	[GeneratedRegex("^\\d{3}-\\d{4}$")]
	private static partial Regex PostalCodeValidChecker();
	#endregion Postal Code Checker

	#region Prefecture Checker
	/// <summary>
	/// Checks if the given text is a valid prefecture.
	/// </summary>
	/// <param name="prefecture">The prefecture to validate.</param>
	public static bool IsPrefectureValid(string prefecture) => PrefectureExtensions.TryGetPrefecture(prefecture, out _);
	#endregion Prefecture Checker
}

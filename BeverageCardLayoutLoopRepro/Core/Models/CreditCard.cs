using System.Text.RegularExpressions;

namespace Beerbox.App.Core.Models;

public partial record CreditCard(
	string NameOnCard,
	string CardNumber,
	string Month,
	string Year,
	string VerificationCode,
	Address Address
) : IPaymentMethod
{
	public Service.Contracts.Database.Fragments.IPaymentMethod ToDocument() =>
		new Service.Contracts.Database.Fragments.CreditCard(
			NameOnCard.Trim(),
			CardNumber.Trim(),
			Month.Trim(),
			Year.Trim(),
			VerificationCode.Trim(),
			Address.ToDocument()
		);

	public static CreditCard Empty => new("", "", "", "", "", Address.Empty);

	#region Credit Card Checker
	public bool IsValid(bool ignoreAddress = false) =>
		IsNameOnCardValid(NameOnCard)
		&& IsCardNumberValid(CardNumber)
		&& IsExpirationDateValid(Month, Year)
		&& IsVerificationCodeValid(VerificationCode)
		&& (ignoreAddress || Address.IsValid(Address));
	#endregion Credit Card Checker

	#region Card Number Checker
	/// <summary>
	/// Checks if the given text is a partial valid card number.
	/// To be used during input.
	/// <para>
	/// The actual allowed card numbers are complex,
	/// so we do minimal checks here and rely on the service.
	/// </para>
	/// </summary>
	public static Func<string, bool> IsCardNumberInputValid { get; } =
		text => CardNumberInputValidChecker().IsMatch(text);

	[GeneratedRegex("^\\d{0,19}$")]
	private static partial Regex CardNumberInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid name.
	/// <para>
	/// The actual allowed card numbers are complex,
	/// so we do minimal checks here and rely on the service.
	/// </para>
	/// </summary>
	/// <param name="cardNumber">The card number to validate.</param>
	public static bool IsCardNumberValid(string cardNumber) => CardNumberChecker().IsMatch(cardNumber);

	[GeneratedRegex("^\\d{12,19}$")]
	private static partial Regex CardNumberChecker();
	#endregion Card Number Checker

	#region Expiration Date Checker
	/// <summary>
	/// Checks if the given text is a valid expiration date.
	/// </summary>
	/// <param name="month">The expiration month.</param>
	/// <param name="year">The expiration year.</param>
	public static bool IsExpirationDateValid(string month, string year)
	{
		if (!int.TryParse(month, out var m) || !int.TryParse(year, out var y) || m < 1 || m > 12 || y < 1)
		{
			return false;
		}
		// find last day of the expiration month
		var date = new DateTime(y, m, 1).AddMonths(1).AddDays(-1);
		return date > DateTime.Today;
	}
	#endregion Expiration Date Checker

	#region Name on Card Checker
	/// <summary>
	/// Checks if the given text is a partial valid name.
	/// To be used during input.
	/// <para>
	/// The actual rules used by credit cards are not well documented,
	/// so we do minimal checks here and rely on the service.
	/// </para>
	/// </summary>
	public static Func<string, bool> IsNameOnCardInputValid { get; } =
		name => name.Length == 0 || NameInputValidChecker().IsMatch(name);

	[GeneratedRegex("^[^\\s]")]
	private static partial Regex NameInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid name.
	/// <para>
	/// The actual rules used by credit cards are not well documented,
	/// so we do minimal checks here and rely on the service.
	/// </para>
	/// </summary>
	/// <param name="name">The name on the card to validate.</param>
	public static bool IsNameOnCardValid(string name) => name.Length > 0 && NameChecker().IsMatch(name);

	[GeneratedRegex(@"^\w+(?: \w+)*$")]
	private static partial Regex NameChecker();
	#endregion Name on Card Checker

	#region Verification Code Checker
	/// <summary>
	/// Checks if the given text is a partial valid verification code.
	/// To be used during input.
	/// </summary>
	public static Func<string, bool> IsVerificationCodeInputValid { get; } =
		text => VerificationCodeInputValidChecker().IsMatch(text);

	[GeneratedRegex("^\\d{0,4}$")]
	private static partial Regex VerificationCodeInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid verification code.
	/// </summary>
	/// <param name="code">The verification code to validate.</param>
	public static bool IsVerificationCodeValid(string code) => VerificationCodeChecker().IsMatch(code);

	[GeneratedRegex("^\\d{3,4}$")]
	private static partial Regex VerificationCodeChecker();
	#endregion Verification Code Checker
}

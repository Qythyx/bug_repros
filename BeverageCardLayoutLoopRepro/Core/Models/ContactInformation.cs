using System.Text.RegularExpressions;

namespace Beerbox.App.Core.Models;

public partial record ContactInformation(string EmailAddress, string Name, string PhoneNumber)
{
	public Service.Contracts.Database.Fragments.ContactInformation ToDocument() =>
		new(EmailAddress.Trim().ToLowerInvariant(), Name.Trim(), PhoneNumber.Trim());

	public static ContactInformation FromDocument(Service.Contracts.Database.Fragments.ContactInformation document) =>
		new(document.EmailAddress, document.Name, document.PhoneNumber);

	public static ContactInformation Empty => new("", "", "");

	#region Contact Information Checker
	public static bool IsValid(ContactInformation contactInformation) =>
		IsEmailAddressValid(contactInformation.EmailAddress)
		&& IsNameValid(contactInformation.Name)
		&& IsPhoneNumberValid(contactInformation.PhoneNumber);
	#endregion Contact Information Checker

	#region Email Address Checker
	/// <summary>
	/// Checks if the given text is a partial valid email address.
	/// To be used during input.
	/// <para>
	/// The actual RFC for email address is too complex, so do minimal
	/// checks here and rely on the fact they need to receive the email
	/// we send.
	/// </para>
	/// </summary>
	public static Func<string, bool> IsEmailAddressInputValid { get; } =
		text => text.Length == 0 || EmailAddressInputValidChecker().IsMatch(text);

	[GeneratedRegex("^[^@\\.\\s][^\\s]*$")]
	private static partial Regex EmailAddressInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid email address.
	/// <para>
	/// The actual RFC for email address is too complex, so do minimal
	/// checks here and rely on the fact they need to receive the email
	/// we send.
	/// </para>
	/// </summary>
	/// <param name="emailAddress">The email address to validate.</param>
	public static bool IsEmailAddressValid(string emailAddress) =>
		System.Net.Mail.MailAddress.TryCreate(emailAddress, out var _);
	#endregion Email Address Checker

	#region Name Checker
	/// <summary>
	/// Checks if the given text is a partial valid name.
	/// To be used during input.
	/// </summary>
	public static Func<string, bool> IsNameInputValid { get; } =
		text => text.Length == 0 || NameInputValidChecker().IsMatch(text);

	[GeneratedRegex("^[^\\s]")]
	private static partial Regex NameInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid name.
	/// </summary>
	/// <param name="name">The name to validate.</param>
	public static bool IsNameValid(string name) => !string.IsNullOrEmpty(name) && NameValidChecker().IsMatch(name);

	[GeneratedRegex("^[^\\s]")]
	private static partial Regex NameValidChecker();
	#endregion Name Checker

	#region PhoneNumber Checker
	/// <summary>
	/// Checks if the given text is a partial valid PhoneNumber number.
	/// To be used during input.
	/// </summary>
	public static Func<string, bool> IsPhoneNumberInputValid { get; } =
		text => PhoneNumberInputValidChecker().IsMatch(text);

	[GeneratedRegex("^\\d{0,11}$")]
	private static partial Regex PhoneNumberInputValidChecker();

	/// <summary>
	/// Checks if the given text is a valid PhoneNumber number.
	/// </summary>
	/// <param name="phoneNumber">The phone number to validate.</param>
	public static bool IsPhoneNumberValid(string phoneNumber) => PhoneNumberValidChecker().IsMatch(phoneNumber);

	[GeneratedRegex("^\\d{10,11}$")]
	private static partial Regex PhoneNumberValidChecker();
	#endregion PhoneNumber Checker
}

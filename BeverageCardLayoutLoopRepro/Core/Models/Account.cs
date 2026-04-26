using System.Globalization;
using System.Text;
using Beerbox.App.Core.Exceptions;
using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.App.Core.Models;

public record Account(
	string ID,
	ContactInformation ContactInformation,
	Address Shipping,
	RegisteredPaymentMethod PaymentMethod,
	string? UntappdToken
)
{
	private const string TemporaryID = "Temporary ID";

	protected virtual bool PrintMembers(StringBuilder builder)
	{
		_ = builder.Append(CultureInfo.InvariantCulture, $"ID = {ID}, {ContactInformation}");
		return true;
	}

	public bool IsTemporary => ID == TemporaryID;

	public static Account Temporary => new(TemporaryID, ContactInformation.Empty, Address.Empty, null!, null);

	public void AssertLoggedIn()
	{
		if (IsTemporary)
		{
			throw new NotLoggedInException();
		}
	}
}

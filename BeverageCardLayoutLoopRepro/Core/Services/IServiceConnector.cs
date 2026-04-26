using System.Net;
using Beerbox.Service.Contracts.Database.Documents;
using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.App.Core.Services;

/// <summary>
/// Defines the methods that connect to the service.
/// None of these should throw an exception for network issues or server processing issues.
/// Instead they should return a <see cref="ServiceResult"/> with
/// <see cref="HttpStatusCode.BadRequest"/>.
/// </summary>
public interface IServiceConnector
{
	/// <summary>Sets the quantity of items for the <see cref="Offer"/>
	/// in the current <see cref="Order"/>.</summary>
	/// <param name="accountID">The account ID.</param>
	/// <param name="offerId">The ID of the <see cref="Offer"/>.</param>
	/// <param name="quantity">The quantity.</param>
	/// <returns>A <see cref="ServiceResult{OrderDocument}"/>.</returns>
	Task<ServiceResult<Order>> SetOfferQuantityAsync(string accountID, string offerId, int quantity);

	/// <summary>
	/// Checks if a previous login initiated though <see cref="InitiateLoginAsync"/>
	/// has been completed successfully. This call should be repeated until successfull
	/// or the user cancels.
	/// </summary>
	/// <param name="email">The email address.</param>
	/// <param name="clientToken">The token received from <see cref="InitiateLoginAsync"/>.</param>
	/// <returns>A <see cref="ServiceResult{AccountDocument}"/>.</returns>
	Task<ServiceResult<Account>> GetAccountAsync(string email, string clientToken);

	/// <summary>Creates a new account.</summary>
	/// <param name="contactInformation">The contact information.</param>
	/// <param name="shippingAddress">The shipping sddress.</param>
	/// <param name="paymentMethod">The payment method.</param>
	/// <returns>A <see cref="ServiceResult"/>.</returns>
	Task<ServiceResult> CreateAccountAsync(
		ContactInformation contactInformation,
		Address shippingAddress,
		IPaymentMethod paymentMethod
	);

	/// <summary>Returns all active offers.</summary>
	/// <returns>A <see cref="ServiceResult{T}"/> containing the <see cref="Offer"/>s.</returns>
	Task<ServiceResult<Offer[]>> GetActiveOffersAsync();

	/// <summary>
	/// Returns the active <see cref="Order"/> for the given
	/// <paramref name="accountID"/>. If the previous active <see cref="Order"/>
	/// has been closed then a new one should be returned.
	/// </summary>
	/// <param name="accountID">The account ID.</param>
	/// <returns>A <see cref="ServiceResult{OrderDocument}"/>.</returns>
	Task<ServiceResult<Order>> GetActiveOrderAsync(string accountID);

	/// <summary>Returns the <see cref="Beverage"/> for the given ID.</summary>
	/// <param name="beverageID">The ID.</param>
	/// <returns>A <see cref="ServiceResult{BeverageDocument}"/>.</returns>
	Task<ServiceResult<Beverage>> GetBeverageAsync(string beverageID);

	/// <summary>Returns the <see cref="Offer"/>s for the given IDs.</summary>
	/// <param name="offerIDs">The IDs.</param>
	/// <returns>A <see cref="ServiceResult{T}"/> containing the <see cref="Offer"/>s.</returns>
	Task<ServiceResult<Offer[]>> GetOffersAsync(IEnumerable<string> offerIDs);

	/// <summary>
	/// Initiates login. The entire process is:
	/// <ol>
	///   <li>User initiates login by calling <see cref="DataManager.InitiateLoginAsync(string)"/></li>
	///   <li>That calls <see cref="InitiateLoginAsync(string)"/></li>
	///   <li>The implementation of that generates a new client token and calls the actual service</li>
	///   <li>
	///		The service creates an <see cref="AccountPendingVerification"/> and then sends
	///		an email to the user that contains a link that will complete the verification.
	///	  </li>
	///   <li>
	///		If the above was successful then the client token is retured back to <see cref="DataManager"/>
	///		to be saved locally.
	///   </li>
	/// </ol>
	/// After the above the user will need to click on the link in the email to complete the
	/// verification process. Once that is done a call to <see cref="GetAccountAsync(string, string)"/>
	/// with the provided client token will succeed. Until the verification is complete that call
	/// will return a failure code. If there is an <see cref="AccountPendingVerification"/>
	/// with the requested token then <see cref="HttpStatusCode.NotFound"/> is returned.
	/// If a matching token is not found then <see cref="HttpStatusCode.FailedDependency"/> is
	/// returned instead and the initiation process must be begun again.
	/// </summary>
	/// <param name="emailAddress">The email address.</param>
	/// <returns>A <see cref="ServiceResult{T}"/> of <see cref="string"/> containing the client token
	/// that is passed to <see cref="GetAccountAsync"/>.</returns>
	Task<ServiceResult<string>> InitiateLoginAsync(string emailAddress);

	/// <summary>Returns global application settings from the backend.</summary>
	/// <returns>A <see cref="ServiceResult{Settings}"/>.</returns>
	Task<ServiceResult<Service.Contracts.Database.Documents.Settings>> GetSettingsAsync();
}

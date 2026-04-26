using System.Collections.ObjectModel;
using System.Net;
using Beerbox.App.Core.Models;

namespace Beerbox.App.Core.Services;

public class DataManager
{
	private const string CacheIdAccount = "Account";
	private const string CacheIdAccountEmail = "AccountEmail";
	private const string CacheIdClientToken = "ClientToken";
	private const string CacheIdOrder = "Order";
	private const string CacheIdOrderHistory = "OrderHistory";
	private static readonly TimeSpan CacheExpireTime = TimeSpan.FromDays(1);

	private readonly Dictionary<string, Beverage> _beverages;
	private readonly Cache? _cache;
	private readonly BeerboxTelemetryService _telemetry;
	private readonly Lock _orderHistoryLock = new();
	protected const string LoggedOutOrderID = "logged out";
	protected IServiceConnector ServiceConnector { get; }

	public DataManager(
		IServiceConnector connector,
		Cache? cache, // TODO: refactor cache to be required
		BeerboxTelemetryService telemetry
	)
	{
		ServiceConnector = connector;
		_cache = cache;
		_telemetry = telemetry;

		_beverages = [];

		if (_cache != null)
		{
			if (_cache.TryGet<Account>(CacheIdAccount, out var account))
			{
				SetAccount(account!);

				if (_cache.TryGet<Order>(CacheIdOrder, out var order))
				{
					_ = SetOrder(order!);
				}

				if (_cache.TryGet<Order[]>(CacheIdOrderHistory, out var orderHistory))
				{
					MergeOrderHistory(orderHistory!);
				}
			}
			else if (_cache.TryGet<string>(CacheIdAccountEmail, out var email))
			{
				Task.Run(async () => await TryLoginAccountAsync(email!)).Wait();
			}

			if (!Account.IsTemporary)
			{
				_telemetry.TrackLogin(
					BeerboxTelemetryService.LoginState.Cached,
					Account.ContactInformation.EmailAddress
				);
			}
		}
	}

	public Service.Contracts.Database.Documents.Settings BackendSettings { get; private set; } =
		Service.Contracts.Database.Documents.Settings.Default;

	#region State properties

	public Account Account { get; private set; } = Account.Temporary;
	public ReadOnlyCollection<Offer> Offers { get; private set; } = new(Array.Empty<Offer>());
	public Order Order { get; private set; } = Order.CreateTemporary([]);
	public ReadOnlyCollection<Order> OrderHistory { get; private set; } = new(Array.Empty<Order>());

	private void SetAccount(Account account)
	{
		Account = account;
		_cache?.Add(CacheIdAccount, account, TimeSpan.MaxValue);
		if (!account.IsTemporary)
		{
			_cache?.Add(CacheIdAccountEmail, account.ContactInformation.EmailAddress, TimeSpan.MaxValue);
		}
	}

	private Order SetOrder(Order order)
	{
		Order = order;
		_cache?.Add(CacheIdOrder, order, TimeSpan.MaxValue);
		return order;
	}

	private void SetOffers(IEnumerable<Offer> offers) => Offers = new([.. offers]);

	/// <summary>
	/// Adds <see cref="Order"/>s to the <see cref="OrderHistory"/> collection
	/// ensuring there are no duplicates and that the end result is sorted.
	/// </summary>
	/// <param name="orders">The orders to merge into OrderHistory.</param>
	private void MergeOrderHistory(IEnumerable<Order> orders)
	{
		lock (_orderHistoryLock)
		{
			var distinct = new Dictionary<string, Order>();
			foreach (var existing in orders.Concat(OrderHistory))
			{
				_ = distinct.TryAdd(existing.ID, existing);
			}
			OrderHistory = distinct.Values.OrderBy(o => o.EndDate).ToList().AsReadOnly();
			_cache?.Add(CacheIdOrderHistory, OrderHistory, TimeSpan.MaxValue);
		}
	}

	private void ClearOrderHistory()
	{
		OrderHistory = new(Array.Empty<Order>());
		_cache?.Remove(CacheIdOrderHistory);
	}

	#endregion State properties

	protected string? ClientToken
	{
		get => _cache != null && _cache.TryGet<string>(CacheIdClientToken, out var token) ? token : field;
		set
		{
			if (_cache != null)
			{
				_cache.Add(CacheIdClientToken, value, TimeSpan.MaxValue);
			}
			else
			{
				field = value;
			}
		}
	}

	private static string GetCacheId<T>(string id) => $"{typeof(T).FullName}|{id}";

	/// <summary>Sets the quantity of items for the <see cref="Offer"/> in the current <see cref="Order"/>.</summary>
	/// <param name="offer">The offer.</param>
	/// <param name="quantity">The quantity.</param>
	/// <returns>A <see cref="ServiceResult"/> containing the updated <see cref="Order"/>.</returns>
	public async Task<ServiceResult<Order>> SetOfferQuantityAsync(Offer offer, int quantity) =>
		Account.IsTemporary
			? new(SetTemporaryOrder(Order.Items.Where(i => i.Offer.ID != offer.ID).Append(new(offer, quantity))))
			: await UpdateActiveOrderAsync(
					await ServiceConnector.SetOfferQuantityAsync(Account.ID, offer.ID, quantity).ConfigureAwait(false)
				)
				.ConfigureAwait(false);

	private Order SetTemporaryOrder(IEnumerable<OrderEntry> entries) => SetOrder(Order.CreateTemporary(entries));

	private async Task<ServiceResult<Order>> UpdateActiveOrderAsync(
		ServiceResult<Service.Contracts.Database.Documents.Order> orderDocResult
	)
	{
		if (!orderDocResult.IsSuccessStatusCode)
		{
			return new(orderDocResult.Status);
		}

		var result = await GetOrderFromDocumentAsync(orderDocResult.Document);
		if (result.IsSuccessStatusCode)
		{
			_ = SetOrder(result.Document);
		}
		return result;
	}

	private async Task<ServiceResult<Order>> GetOrderFromDocumentAsync(
		Service.Contracts.Database.Documents.Order orderDoc
	)
	{
		var status = HttpStatusCode.OK;
		async Task<IList<OrderEntry>> convertEntries(
			IEnumerable<Service.Contracts.Database.Fragments.OrderEntry> docEntries
		)
		{
			var entries = new List<OrderEntry>();
			// If the server is down or there is no network then GetOffersAsync will fail.
			// This will prevent the app opening at all, even with a logged-out order.
			// So, skip trying to get when there are no entries.
			if (docEntries.Any())
			{
				var documents = await ServiceConnector
					.GetOffersAsync(docEntries.Select(o => o.OfferID))
					.ConfigureAwait(false);
				if (!documents.IsSuccessStatusCode)
				{
					status = documents.Status;
					return entries;
				}

				var offers = await GetOffersAsync(documents.Document).ConfigureAwait(false);
				if (!offers.IsSuccessStatusCode)
				{
					status = offers.Status;
					return entries;
				}

				var offersByID = offers.Document.ToDictionary(o => o.ID);
				entries.AddRange(
					docEntries
						.Select(e => new OrderEntry(offersByID[e.OfferID], e.Quantity))
						.OrderBy(e => e.Offer.Begins)
				);
			}
			return entries;
		}

		var order = await Order.FromDocAsync(orderDoc, convertEntries);
		return status == HttpStatusCode.OK ? new(order) : new(status);
	}

	/// <summary>Creates a new <see cref="Account"/>.</summary>
	/// <param name="contactInformation">The <see cref="ContactInformation"/>.</param>
	/// <param name="shippingAddress">The shipping <see cref="Address"/>.</param>
	/// <param name="paymentMethod">The payment method.</param>
	/// <returns>A <see cref="ServiceResult"/> containing the
	/// <see cref="AccountResult"/> of the operation.</returns>
	public async Task<ServiceResult<AccountResult>> CreateAccountAsync(
		ContactInformation contactInformation,
		Address shippingAddress,
		IPaymentMethod paymentMethod
	)
	{
		var accountResult = CheckAccountInputValues(contactInformation, paymentMethod, shippingAddress);
		if (accountResult != AccountResult.Success)
		{
			return new(accountResult);
		}

		var result = await ServiceConnector
			.CreateAccountAsync(
				contactInformation.ToDocument(),
				shippingAddress.ToDocument(),
				paymentMethod.ToDocument()
			)
			.ConfigureAwait(false);
		var serviceResult = new ServiceResult<AccountResult>(
			result.Status switch
			{
				HttpStatusCode.Conflict => AccountResult.EmailAlreadyExists,
				HttpStatusCode.OK => AccountResult.Success,
				_ => AccountResult.UnknownError,
			}
		);
		_telemetry.TrackCreateAccount(serviceResult.Document);
		return serviceResult;
	}

	/// <summary>Returns all active <see cref="Offer"/>s.</summary>
	/// <returns>A <see cref="ServiceResult"/> containing the active <see cref="Offer"/>s.</returns>
	public async Task<ServiceResult<IEnumerable<Offer>>> GetActiveOffersAsync()
	{
		var offerDocs = await ServiceConnector.GetActiveOffersAsync().ConfigureAwait(false);
		if (!offerDocs.IsSuccessStatusCode)
		{
			return new(offerDocs.Status);
		}

		var offersResult = await GetOffersAsync(offerDocs.Document).ConfigureAwait(false);
		if (!offersResult.IsSuccessStatusCode)
		{
			return new(offersResult.Status);
		}

		SetOffers(offersResult.Document.OrderByDescending(o => o.Begins));
		return new(Offers);
	}

	/// <summary>Returns the <see cref="Beverage"/> for the <paramref name="beverageID"/>.</summary>
	/// <param name="beverageID">The ID of the <see cref="Beverage"/>.</param>
	/// <returns>A <see cref="ServiceResult"/> containing the <see cref="Beverage"/>.</returns>
	private async Task<ServiceResult<Beverage>> GetBeverageAsync(string beverageID)
	{
		var cacheID = GetCacheId<Beverage>(beverageID);
		if (
			!_beverages.TryGetValue(beverageID, out var beverage)
			&& (_cache == null || !_cache.TryGet(cacheID, out beverage))
		)
		{
			var result = await ServiceConnector.GetBeverageAsync(beverageID).ConfigureAwait(false);
			if (!result.IsSuccessStatusCode)
			{
				return new(result.Status);
			}
			beverage = Beverage.FromDocument(result.Document);
		}

		lock (_beverages)
		{
			_beverages[beverageID] = beverage!;
			_cache?.Add(cacheID, beverage, CacheExpireTime);
		}

		return new(beverage!);
	}

	/// <summary>Returns the <see cref="Offer"/>s for the <paramref name="offerDocuments"/>s.</summary>
	/// <param name="offerDocuments">The <see cref="Service.Contracts.Database.Documents.Offer"/>s.</param>
	/// <returns>A <see cref="ServiceResult"/> containing the <see cref="Offer"/>s.</returns>.
	private async Task<ServiceResult<Offer[]>> GetOffersAsync(
		params Service.Contracts.Database.Documents.Offer[] offerDocuments
	)
	{
		var results = await Task.WhenAll(
			offerDocuments.Select(offer => offer.BeverageID).Distinct().Select(GetBeverageAsync)
		);
		var firstError = results.FirstOrDefault(result => !result.IsSuccessStatusCode);
		if (firstError != null)
		{
			return new(firstError.Status);
		}
		var beverages = results.Select(r => r.Document).ToDictionary(d => d.ID);
		var offers = offerDocuments.Select(offer => new Offer(
			offer.ID,
			offer.AvailableQuantity,
			beverages[offer.BeverageID],
			offer.Begins,
			offer.Ends,
			offer.Exclusive,
			offer.MaxQuantity,
			offer.MinQuantity,
			new(offer.Price),
			offer.Rare,
			offer.Sale
		));
		return new([.. offers]);
	}

	/// <summary>
	/// Returns a <see cref="ServiceResult"/> from a login that was previously initiated
	/// though <see cref="InitiateLoginAsync"/>, which has been completed successfully.
	/// This call should be repeated until successful or the user cancels.
	/// </summary>
	/// <param name="emailAddress">The email address.</param>
	/// <returns>A <see cref="ServiceResult"/> that denotes if the login was successful or not.</returns>
	public async Task<ServiceResult> TryLoginAccountAsync(string emailAddress)
	{
		if (ClientToken == null)
		{
			return new(HttpStatusCode.FailedDependency);
		}

		emailAddress = emailAddress.ToLowerInvariant();
		var result = await ServiceConnector.GetAccountAsync(emailAddress, ClientToken).ConfigureAwait(false);
		if (!result.IsSuccessStatusCode)
		{
			return new(result.Status);
		}

		var doc = result.Document;
		SetAccount(
			new Account(
				doc.ID,
				ContactInformation.FromDocument(doc.ContactInformation),
				Address.FromDocument(doc.ShippingAddress),
				doc.PaymentMethod,
				doc.UntappdToken
			)
		);

		var previousOrder = Order;

		var updatedOrder = await UpdateActiveOrderAsync(
				await ServiceConnector.GetActiveOrderAsync(Account.ID).ConfigureAwait(false)
			)
			.ConfigureAwait(false);

		foreach (var entry in previousOrder.Items)
		{
			if (updatedOrder.IsSuccessStatusCode)
			{
				updatedOrder = await SetOfferQuantityAsync(entry.Offer, entry.Quantity).ConfigureAwait(false);
			}
		}

		return new(HttpStatusCode.OK);
	}

	/// <summary>Sends an email to the user with a link they click on to confirm login.</summary>
	/// <param name="emailAddress">The email address.</param>
	/// <returns>A <see cref="ServiceResult"/> that indicates if the operation succeeded.</returns>
	public async Task<ServiceResult> InitiateLoginAsync(string emailAddress)
	{
		var result = await ServiceConnector.InitiateLoginAsync(emailAddress.ToLowerInvariant()).ConfigureAwait(false);
		_telemetry.TrackLogin(
			result.IsSuccessStatusCode
				? BeerboxTelemetryService.LoginState.InitiateSuccess
				: BeerboxTelemetryService.LoginState.InitiateFail,
			Account.ContactInformation.EmailAddress
		);
		ClientToken = result.Document;
		return result;
	}

	/// <summary>
	/// Generally this shouldn't be needed, but if <see cref="SetOfferQuantityAsync"/>
	/// times out then the user will want to refresh.
	/// </summary>
	/// <returns>A <see cref="ServiceResult"/> containing the current <see cref="Order"/>.</returns>
	/// <exception cref="Exceptions.NotLoggedInException">Thrown when the user is not logged in.</exception>
	public async Task<ServiceResult<Order>> RefreshActiveOrderAsync() =>
		Account.IsTemporary
			? new(Order)
			: await UpdateActiveOrderAsync(await ServiceConnector.GetActiveOrderAsync(Account.ID).ConfigureAwait(false))
				.ConfigureAwait(false);

	private static AccountResult CheckAccountInputValues(
		ContactInformation? contactInformation,
		IPaymentMethod? paymentMethod,
		Address? shippingAddress
	) =>
		contactInformation != null && !ContactInformation.IsValid(contactInformation)
			? AccountResult.ContactInformationInvalid
		: paymentMethod != null && paymentMethod is CreditCard cc && !cc.IsValid() ? AccountResult.CreditCardInvalid
		: shippingAddress != null && !Address.IsValid(shippingAddress) ? AccountResult.ShippingAddressInvalid
		: AccountResult.Success;
}

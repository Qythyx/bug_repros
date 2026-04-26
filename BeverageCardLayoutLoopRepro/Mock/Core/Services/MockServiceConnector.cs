using System.Net;
using Beerbox.App.Core.Exceptions;
using Beerbox.Service.Contracts;
using Beerbox.Service.Contracts.Database.Documents;
using Beerbox.Service.Contracts.Database.Fragments;

namespace Beerbox.App.Core.Services;

public class MockServiceConnector(int shippingFreeThreshold, int shippingFee) : IServiceConnector
{
	// TODO: change this so the key is the ID, not the email
	private readonly Dictionary<string, (string? clientToken, Account account)> _accountDocuments = [];
	private readonly Dictionary<string, (string email, string clientToken)> _loginsPendingVerification = [];
	private readonly Dictionary<string, Order> _orderDocuments = [];
	private readonly Dictionary<string, Beverage> _beverageDocuments = [];
	private readonly Dictionary<string, Offer> _offerDocuments = [];
	private readonly Random _random = new();
	private readonly Dictionary<Type, int> _nextIds = [];
	private readonly int _shippingFreeThreshold = shippingFreeThreshold;
	private readonly int _shippingFee = shippingFee;

	public double ArtificialErrorRate { get; set; }
	public int ArtificialLatencyMillis { get; set; }
	public Queue<Exception?> ArtificialExceptions { get; private set; } = new();
	public Queue<HttpStatusCode> ArtificialStatuses { get; private set; } = new();
	public PaymentMethodStatus PaymentMethodStatus { get; set; } = PaymentMethodStatus.OK;
	public bool OrderEndingSoon { get; set; }

	internal async Task<HttpStatusCode> CheckArtificialProblems(bool extraLatency = false)
	{
		await Task.Delay(ArtificialLatencyMillis + (extraLatency && ArtificialLatencyMillis > 0 ? 3000 : 0));
		return ArtificialExceptions.TryDequeue(out var exception) && exception != null ? throw exception
			: ArtificialStatuses.TryDequeue(out var status) ? status
			: _random.NextDouble() < ArtificialErrorRate ? HttpStatusCode.InternalServerError
			: HttpStatusCode.OK;
	}

	#region methods for manipulating mock data during tests
	public void ForTestingAddBeverage(Beverage beverage) => _beverageDocuments.Add(beverage.ID, beverage);

	public IEnumerable<string> ForTestingGetBeverageIds() => _beverageDocuments.Select(b => b.Key);

	public string ForTestingAddOffer(
		string beverageId,
		DateTime begins,
		int quantity,
		int priceInYen,
		int min,
		int max,
		bool exclusive,
		bool rare,
		bool sale
	)
	{
		var offer = new Offer(
			GetNextId<Offer>(),
			0,
			quantity,
			beverageId,
			begins,
			begins.AddDays(2),
			exclusive,
			max,
			min,
			false,
			quantity,
			priceInYen,
			rare,
			sale
		);
		_offerDocuments.Add(offer.ID, offer);
		return offer.ID;
	}

	public string? ForTestingGetEmailVerificationToken(string email)
	{
		var match = _loginsPendingVerification.FirstOrDefault(x => x.Value.email == email);
		return match.Equals(default) ? null : match.Key;
	}

	public void ForTestingSetEmailVerified(string serverToken)
	{
		if (
			_loginsPendingVerification.TryGetValue(serverToken, out var info)
			&& _accountDocuments.TryGetValue(info.email, out var accountInfo)
		)
		{
			_ = _loginsPendingVerification.Remove(serverToken);
			_accountDocuments[info.email] = (info.clientToken, accountInfo.account);
		}
	}

	public Order ForTestingAddOldOrder(
		string accountID,
		DateTime endDate,
		DateTime shipDate,
		IEnumerable<(string, int)> entries,
		int shippingFreeThreshold,
		int shippingFee
	)
	{
		if (!_accountDocuments.Any(_ => _.Value.account.ID == accountID))
		{
			throw ItemNotFound.ForId<Account>(accountID);
		}
		var order = new Order(
			GetNextId<Order>(),
			accountID,
			endDate,
			shipDate,
			entries.Select(e => new OrderEntry(e.Item1, e.Item2)).ToList().AsReadOnly(),
			shippingFreeThreshold,
			shippingFee,
			true,
			true
		);
		_orderDocuments.Add(order.ID, order);
		return order;
	}
	#endregion methods for manipulating mock data during tests

	async Task<ServiceResult<Order>> IServiceConnector.SetOfferQuantityAsync(
		string accountID,
		string offerId,
		int quantity
	)
	{
		var artificialResult = await CheckArtificialProblems(true);
		if (artificialResult != HttpStatusCode.OK)
		{
			return new(artificialResult);
		}

		if (!_offerDocuments.TryGetValue(offerId, out var offer))
		{
			throw ItemNotFound.ForId<Offer>(offerId);
		}
		var result = await ((IServiceConnector)this).GetActiveOrderAsync(accountID);
		if (!result.IsSuccessStatusCode)
		{
			return result;
		}
		var order = result.Document;
		var previousQuantity = order.Entries.FirstOrDefault(e => e.OfferID == offerId)?.Quantity ?? 0;
		quantity =
			quantity == 0
				? 0
				: new[]
				{
					offer.AvailableQuantity + previousQuantity,
					offer.MaxQuantity,
					Math.Max(offer.MinQuantity, quantity),
				}.Min();
		_offerDocuments[offerId] = offer.Allocate(quantity);
		return new(_orderDocuments[order.ID] = order.SetEntry(new(offerId, quantity)));
	}

	async Task<ServiceResult<Account>> IServiceConnector.GetAccountAsync(string email, string clientToken)
	{
		var artificialResult = await CheckArtificialProblems();
		return artificialResult == HttpStatusCode.OK
			? _accountDocuments.TryGetValue(email, out var info) && info.clientToken == clientToken
				? new(info.account)
				: new(HttpStatusCode.NotFound)
			: new(artificialResult);
	}

	async Task<ServiceResult> IServiceConnector.CreateAccountAsync(
		ContactInformation contactInformation,
		Address shippingAddress,
		IPaymentMethod paymentMethod
	)
	{
		var artificialResult = await CheckArtificialProblems(true);
		if (artificialResult != HttpStatusCode.OK)
		{
			return new(artificialResult);
		}

		if (_accountDocuments.ContainsKey(contactInformation.EmailAddress))
		{
			return new(HttpStatusCode.Conflict);
		}
		_accountDocuments.Add(
			contactInformation.EmailAddress,
			(
				Guid.NewGuid().ToString(),
				new Account(
					GetNextId<Account>(),
					contactInformation,
					shippingAddress,
					GetRegisteredPaymentMethod(paymentMethod),
					null
				)
			)
		);

		return new(HttpStatusCode.OK);
	}

	private RegisteredPaymentMethod GetRegisteredPaymentMethod(IPaymentMethod paymentMethod) =>
		paymentMethod is COD
			? new(PaymentMethodType.COD, null, PaymentMethodStatus.OK)
			: new(PaymentMethodType.CreditCard, GetNextId<RegisteredPaymentMethod>(), PaymentMethodStatus);

	async Task<ServiceResult<Offer[]>> IServiceConnector.GetActiveOffersAsync()
	{
		var artificialResult = await CheckArtificialProblems();
		return artificialResult == HttpStatusCode.OK
			? new(_offerDocuments.Values.Where(o => o.AvailableQuantity > 0 && o.Begins <= DateTime.Now).ToArray())
			: new(artificialResult);
	}

	async Task<ServiceResult<Order>> IServiceConnector.GetActiveOrderAsync(string accountID)
	{
		var artificialResult = await CheckArtificialProblems();
		if (artificialResult != HttpStatusCode.OK)
		{
			return new(artificialResult);
		}

		if (!_accountDocuments.Any(_ => _.Value.account.ID == accountID))
		{
			throw ItemNotFound.ForId<Account>(accountID);
		}
		var order = _orderDocuments.Values.FirstOrDefault(o => o.AccountID == accountID && o.EndDate > DateTime.Now);
		if (order == null)
		{
			var endDate = DateTime.Now.AddDays(OrderEndingSoon ? 2 : 14);
			order = new Order(
				GetNextId<Order>(),
				accountID,
				endDate,
				endDate.AddDays(1),
				Array.Empty<OrderEntry>().ToList().AsReadOnly(),
				_shippingFreeThreshold,
				_shippingFee,
				false,
				false
			);
			_orderDocuments.Add(order.ID, order);
		}
		return new(order);
	}

	async Task<ServiceResult<Beverage>> IServiceConnector.GetBeverageAsync(string beverageID)
	{
		var artificialResult = await CheckArtificialProblems();
		return artificialResult == HttpStatusCode.OK
			? _beverageDocuments.TryGetValue(beverageID, out var beverage)
				? new ServiceResult<Beverage>(beverage)
				: new ServiceResult<Beverage>(HttpStatusCode.NotFound)
			: new(artificialResult);
	}

	async Task<ServiceResult<Offer[]>> IServiceConnector.GetOffersAsync(IEnumerable<string> offerIDs)
	{
		var artificialResult = await CheckArtificialProblems();
		if (artificialResult != HttpStatusCode.OK)
		{
			return new(artificialResult);
		}

		var offers = new List<Offer>();
		foreach (var offerID in offerIDs)
		{
			if (!_offerDocuments.TryGetValue(offerID, out var offer))
			{
				return new(HttpStatusCode.NotFound);
			}
			offers.Add(offer);
		}
		return new([.. offers]);
	}

	async Task<ServiceResult<string>> IServiceConnector.InitiateLoginAsync(string emailAddress)
	{
		var artificialResult = await CheckArtificialProblems(true);
		if (artificialResult != HttpStatusCode.OK)
		{
			return new(artificialResult);
		}

		string serverToken;
		if (_accountDocuments.ContainsKey(emailAddress))
		{
			var clientToken = Guid.NewGuid().ToString();
			serverToken = Guid.NewGuid().ToString();
			_loginsPendingVerification.Add(serverToken, (emailAddress, clientToken));
			return new ServiceResult<string>(clientToken);
		}
		return new ServiceResult<string>(HttpStatusCode.NotFound);
	}

	private string GetNextId<T>()
	{
		var type = typeof(T);
		return type.Name + (_nextIds[type] = _nextIds.TryGetValue(type, out var value) ? value + 1 : 1);
	}

	public async Task<ServiceResult<Service.Contracts.Database.Documents.Settings>> GetSettingsAsync()
	{
		var artificialResult = await CheckArtificialProblems();
		return artificialResult == HttpStatusCode.OK
			? new(Service.Contracts.Database.Documents.Settings.Default)
			: new(artificialResult);
	}
}

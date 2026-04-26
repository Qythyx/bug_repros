using System.Reflection;
using System.Text.Json;
using Beerbox.App.Core.Models;
using Beerbox.Service.Contracts;
using BeverageDetails = Beerbox.Service.Contracts.Database.Documents.BeverageDetails;
using BeverageDoc = Beerbox.Service.Contracts.Database.Documents.Beverage;
using BeverageLanguages = Beerbox.Service.Contracts.Database.Documents.BeverageLanguages;
using UntappdDetails = Beerbox.Service.Contracts.Database.Fragments.UntappdDetails;

namespace Beerbox.App.Core.Services;

public class MockDataManager : DataManager
{
	public const string UntappdTokenOk = "ok token";
	public const string UntappdTokenBad = "bad token";

	public MockDataManager(Cache? cache = null, MockServiceConnector? serviceConnector = null)
		: this(serviceConnector ?? new MockServiceConnector(12345, 321), new MockTelemetryService(), cache) { }

	private MockDataManager(MockServiceConnector connector, MockTelemetryService telemetry, Cache? cache)
		: base(connector, cache, telemetry) => MockTelemetry = telemetry;

	public MockServiceConnector MockServiceConnector => (MockServiceConnector)ServiceConnector;

	public string? MockClientToken
	{
		get => ClientToken;
		set => ClientToken = value;
	}

	public MockTelemetryService MockTelemetry { get; }

	private MockServiceConnector Connector => (MockServiceConnector)ServiceConnector;

	private sealed record TestBeverageJson(
		string Id,
		BeverageKind Kind,
		BeverageDetails English,
		BeverageDetails Japanese,
		string Abv,
		string ImageUrl,
		UntappdDetails Untappd,
		int Quantity,
		int PriceInYen,
		bool Exclusive,
		bool Rare,
		bool Sale
	);

	private static readonly Lazy<TestBeverageJson[]> LazyTestBeverages = new(LoadTestBeverages);
	private static TestBeverageJson[] TestBeverages => LazyTestBeverages.Value;

	private static TestBeverageJson[] LoadTestBeverages()
	{
		using var stream = Assembly
			.GetExecutingAssembly()
			.GetManifestResourceStream("Beerbox.App.Mock.Core.Services.TestBeverages.json")!;
		using var reader = new StreamReader(stream);
		return JsonSerializer.Deserialize<TestBeverageJson[]>(reader.ReadToEnd(), SerializerOptions.Default)!;
	}

	private static BeverageDoc ToBeverageDoc(TestBeverageJson j) =>
		new(j.Id, j.Kind, new BeverageLanguages(j.English, j.Japanese), j.Abv, j.ImageUrl, j.Untappd, true);

	public void CreateTestBeverages()
	{
		foreach (var b in TestBeverages)
		{
			Connector.ForTestingAddBeverage(ToBeverageDoc(b));
		}
	}

	public void CreateTestOffers()
	{
		if (!Connector.ForTestingGetBeverageIds().Any())
		{
			CreateTestBeverages();
		}

		var begins = DateTime.Now.AddHours(-24);
		for (var i = 0; i < TestBeverages.Length; i++)
		{
			var b = TestBeverages[i];
			_ = Connector.ForTestingAddOffer(
				b.Id,
				begins,
				b.Quantity,
				b.PriceInYen,
				i + 1,
				(i + 1) * 2,
				b.Exclusive,
				b.Rare,
				b.Sale
			);
		}
	}

	public async Task AddOffersToActiveOrderAsync()
	{
		var offers = (await GetActiveOffersAsync()).Document.ToList();
		if (offers.Count > 0)
		{
			_ = await SetOfferQuantityAsync(offers[1], 2);
			_ = await SetOfferQuantityAsync(offers[3], 1);
		}
	}

	public void CreateTestOrderHistory()
	{
		var rand = new Random(0);
		IEnumerable<(string, int)> adder(DateTime date)
		{
			foreach (var id in Connector.ForTestingGetBeverageIds())
			{
				var x = (int)(rand.NextDouble() * 5);
				if (x > 0)
				{
					yield return (
						Connector.ForTestingAddOffer(
							id,
							date,
							0,
							800 + (x * 50),
							1,
							10,
							rand.NextDouble() > 0.5,
							rand.NextDouble() > 0.5,
							rand.NextDouble() > 0.5
						),
						x
					);
				}
			}
		}

		var referenceDate = new DateTime(2025, 1, 1);
		for (var i = -20; i < 0; i++)
		{
			var date = referenceDate.AddMonths(i);
			_ = Connector.ForTestingAddOldOrder(
				Account.ID,
				date.AddDays(20),
				date.AddDays(23),
				adder(date),
				12000,
				1000
			);
		}
	}

	public async Task CreateTestAccountAsync(bool isLoggedInToUntappd = false)
	{
		const string email = "foo@bar.com";
		var contact = new ContactInformation(email, "Baz Foo", "1234567890");
		var address = new Address("123-4567", Prefecture.Tokyo.GetLabel(Language.English), "89 Thatplace", "");
		var card = new CreditCard("Baz Foo", "1234567890123456", "01", "2030", "321", address);
		_ = await CreateAccountAsync(contact, address, card);

		_ = await InitiateLoginAsync(email);
		var serverToken = Connector.ForTestingGetEmailVerificationToken(email);
		Connector.ForTestingSetEmailVerified(serverToken!);

		_ = await TryLoginAccountAsync(email);
	}
}

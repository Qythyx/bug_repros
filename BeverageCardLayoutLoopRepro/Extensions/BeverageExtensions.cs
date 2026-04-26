using Beerbox.App.Core.Models;
using Beerbox.App.Core.Services;
using Beerbox.Service.Contracts;

namespace Beerbox.App.Extensions;

public static class BeverageExtensions
{
	extension(Beverage beverage)
	{
		public BeverageDetails LocalizedDetails =>
			beverage.Details[
				IPlatformApplication.Current?.Services.GetRequiredService<AppSettings>().Language ?? Language.English
			];
	}
}

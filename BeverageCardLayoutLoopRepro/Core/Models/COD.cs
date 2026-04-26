namespace Beerbox.App.Core.Models;

public record COD : IPaymentMethod
{
	public Service.Contracts.Database.Fragments.IPaymentMethod ToDocument() =>
		new Service.Contracts.Database.Fragments.COD();
}

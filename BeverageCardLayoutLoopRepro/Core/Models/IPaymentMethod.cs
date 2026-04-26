namespace Beerbox.App.Core.Models;

public interface IPaymentMethod
{
	Service.Contracts.Database.Fragments.IPaymentMethod ToDocument();
}

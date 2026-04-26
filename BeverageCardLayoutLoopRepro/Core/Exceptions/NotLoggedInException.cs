namespace Beerbox.App.Core.Exceptions;

public sealed class NotLoggedInException : Exception
{
	public NotLoggedInException() { }

	public NotLoggedInException(string message)
		: base(message) { }

	public NotLoggedInException(string message, Exception innerException)
		: base(message, innerException) { }
}

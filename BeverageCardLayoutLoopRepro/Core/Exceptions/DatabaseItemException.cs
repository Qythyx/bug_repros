namespace Beerbox.App.Core.Exceptions;

public abstract class DatabaseItemException : Exception
{
	protected DatabaseItemException() { }

	protected DatabaseItemException(string message)
		: base(message) { }

	protected DatabaseItemException(string message, Exception innerException)
		: base(message, innerException) { }

	public string ID { get; init; } = string.Empty;
}

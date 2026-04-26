namespace Beerbox.App.Core.Exceptions;

public static class ItemNotFound
{
	public static ItemNotFoundException<T> ForId<T>(string id) =>
		new($"{typeof(T).Name} with ID '{id}' was not found.") { ID = id };
}

public sealed class ItemNotFoundException<T> : DatabaseItemException
{
	public ItemNotFoundException() { }

	public ItemNotFoundException(string message)
		: base(message) { }

	public ItemNotFoundException(string message, Exception innerException)
		: base(message, innerException) { }
}

namespace Beerbox.App.Core.Extensions;

public static class TaskExtensions
{
	public static async Task HandleException(this Task task, IExceptionHandler handler)
	{
		try
		{
			await task;
		}
		catch (Exception exception)
		{
			handler.HandleException(exception);
		}
	}

	public static async Task<T> HandleException<T>(this Task<T> task, IExceptionHandler handler, T errorValue)
	{
		try
		{
			return await task;
		}
		catch (Exception exception)
		{
			handler.HandleException(exception);
			return errorValue;
		}
	}
}

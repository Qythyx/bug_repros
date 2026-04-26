using System.Threading.Channels;

namespace Beerbox.App.Core.Services;

/// <summary>
/// Abstract base for telemetry services. Buffers telemetry items in a
/// <see cref="Channel{T}"/> and dispatches them to <see cref="ProcessItem"/>
/// on a background thread.
/// </summary>
public abstract class TelemetryService : IExceptionHandler, IDisposable
{
	private readonly Channel<TelemetryItem> _channel = Channel.CreateUnbounded<TelemetryItem>();

	protected TelemetryService() => _ = Task.Run(ConsumeAsync);

	public void TrackEvent(string name, Dictionary<string, string> properties) =>
		TrackItem(new EventItem(name, properties));

	public void TrackException(Exception ex) => TrackItem(new ExceptionItem(ex));

	public void SetUserId(string? userId) => TrackItem(new UserIdItem(userId));

	/// <summary>
	/// Waits until all previously queued items have been processed.
	/// Useful in tests to ensure assertions run after processing completes.
	/// </summary>
	public async Task FlushAsync()
	{
		var tcs = new TaskCompletionSource();
		TrackItem(new FlushItem(tcs));
		await tcs.Task.ConfigureAwait(false);
	}

	/// <summary>
	/// Synchronous version of <see cref="FlushAsync"/>. Blocks until all queued items are processed.
	/// Uses <see cref="Task.Run(Func{Task})"/> to avoid deadlocks when a
	/// <see cref="SynchronizationContext"/> is present (e.g. NUnit).
	/// </summary>
	public void Flush() => Task.Run(FlushAsync).GetAwaiter().GetResult();

	/// <summary>
	/// Called on a background thread for each queued item.
	/// Subclasses pattern-match on the <see cref="TelemetryItem"/> subtypes.
	/// Return <c>false</c> to requeue the item (e.g. when the backend is not yet ready).
	/// </summary>
	/// <param name="item">The item to process.</param>
	/// <returns><c>true</c> if the item was processed; <c>false</c> to retry after a short delay.</returns>
	protected abstract bool ProcessItem(TelemetryItem item);

	public void Dispose()
	{
		_channel.Writer.Complete();
		GC.SuppressFinalize(this);
	}

	void IExceptionHandler.HandleException(Exception ex) => TrackException(ex);

	private void TrackItem(TelemetryItem item) => _channel.Writer.TryWrite(item);

	private async Task ConsumeAsync()
	{
		await foreach (var item in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
		{
			if (item is FlushItem flush)
			{
				flush.Tcs.SetResult();
			}
			else
			{
				while (!ProcessItem(item))
				{
					await Task.Delay(100).ConfigureAwait(false);
				}
			}
		}
	}

	public abstract record TelemetryItem;

	public sealed record EventItem(string Name, Dictionary<string, string> Properties) : TelemetryItem;

	public sealed record ExceptionItem(Exception Exception) : TelemetryItem;

	public sealed record UserIdItem(string? UserId) : TelemetryItem;

	private sealed record FlushItem(TaskCompletionSource Tcs) : TelemetryItem;
}

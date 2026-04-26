using D = System.Diagnostics.Debug;

namespace Beerbox.App.Core.Services;

public class Logger : LoggerBase
{
	protected override void LogDebug(string message) => D.WriteLine(message);

	protected override void LogError(string message) => Console.Error.WriteLine(message);

	protected override void LogInfo(string message) => D.WriteLine(message);

	protected override void LogVerbose(string message) => D.WriteLine(message);

	protected override void LogWarn(string message) => Console.Error.WriteLine(message);
}

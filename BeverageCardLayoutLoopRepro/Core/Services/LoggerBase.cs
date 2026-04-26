namespace Beerbox.App.Core.Services;

public abstract class LoggerBase
{
	public enum Level
	{
		Debug = 0,
		Error = 1,
		Info = 2,
		Verbose = 3,
		Warn = 4,
	}

	public void Log(Level level, string message)
	{
		switch (level)
		{
			case Level.Debug:
				LogDebug(message);
				break;
			case Level.Error:
				LogError(message);
				break;
			case Level.Info:
				LogInfo(message);
				break;
			case Level.Verbose:
				LogVerbose(message);
				break;
			case Level.Warn:
				LogWarn(message);
				break;
		}
	}

	protected abstract void LogDebug(string message);
	protected abstract void LogError(string message);
	protected abstract void LogInfo(string message);
	protected abstract void LogVerbose(string message);
	protected abstract void LogWarn(string message);
}

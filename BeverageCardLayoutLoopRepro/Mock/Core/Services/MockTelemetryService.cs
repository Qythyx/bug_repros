using System.Text.Json;
using System.Text.RegularExpressions;

namespace Beerbox.App.Core.Services;

/// <summary>
/// Telemetry backend for MOCK builds. Logs events to <see cref="System.Diagnostics.Debug"/>
/// and collects all items in memory for test assertions.
/// </summary>
public sealed partial class MockTelemetryService : BeerboxTelemetryService
{
	private readonly List<(string Name, Dictionary<string, string> Data)> _events = [];
	private readonly List<Exception> _exceptions = [];
	private readonly List<string?> _userIds = [];

	protected override bool ProcessItem(TelemetryItem item)
	{
		switch (item)
		{
			case EventItem e:
				System.Diagnostics.Debug.WriteLine(
					$"Event: {e.Name}\n{IndentBlock(JsonSerializer.Serialize(e.Properties, s_indented))}"
				);
				_events.Add((e.Name, e.Properties));
				break;
			case ExceptionItem e:
				_exceptions.Add(e.Exception);
				break;
			case UserIdItem e:
				_userIds.Add(e.UserId);
				break;
		}

		return true;
	}

	private static readonly JsonSerializerOptions s_indented = new() { WriteIndented = true };

	private static string IndentBlock(string block) => LineMatcher().Replace(block, "\t");

	[GeneratedRegex("(?m)^")]
	private static partial Regex LineMatcher();
}

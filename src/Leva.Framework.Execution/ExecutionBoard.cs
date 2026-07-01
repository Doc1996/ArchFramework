using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Stores execution contracts and tracks current and completed execution attempts.
/// </summary>
public sealed class ExecutionBoard(IClock clock, ILogSink? logSink = null)
{
	private readonly Lock _lock = new();
	private readonly IClock _clock = clock;
	private readonly ILogSink? _logSink = logSink;
	private readonly Dictionary<ExecutionId, ExecutionEntry> _entries = [];
	private readonly Dictionary<(Type RequestType, Type ResultType), object> _executions = [];

	public IReadOnlyCollection<ExecutionEntry> Entries
	{
		get
		{
			lock (_lock)
				return _entries.Values.ToList();
		}
	}

	public IReadOnlyCollection<string> RegisteredNames
	{
		get
		{
			lock (_lock)
				return _executions.Values.Select(execution => execution.GetType().Name).ToList();
		}
	}

	public ExecutionBoard Register<TRequest, TResult>(IExecution<TRequest, TResult> execution)
	{
		ArgumentNullException.ThrowIfNull(execution);
		lock (_lock)
			_executions[(typeof(TRequest), typeof(TResult))] = execution;

		return this;
	}

	public bool TryGet(ExecutionId executionId, out ExecutionEntry? entry)
	{
		lock (_lock)
			return _entries.TryGetValue(executionId, out entry);
	}

	public bool Clear(ExecutionId executionId)
	{
		lock (_lock)
		{
			var removed = _entries.Remove(executionId);
			if (removed)
				Log(executionId, "Execution cleared.");

			return removed;
		}
	}

	public void ClearAll()
	{
		lock (_lock)
		{
			if (_entries.Count == 0)
				return;

			_entries.Clear();
			Log("All executions cleared.");
		}
	}

	internal bool TryGetExecution<TRequest, TResult>(out IExecution<TRequest, TResult>? execution)
	{
		lock (_lock)
		{
			if (_executions.TryGetValue((typeof(TRequest), typeof(TResult)), out var value))
			{
				execution = (IExecution<TRequest, TResult>)value;
				return true;
			}
		}

		execution = null;
		return false;
	}

	internal ExecutionEntry Start(ExecutionId executionId, string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		lock (_lock)
		{
			if (_entries.ContainsKey(executionId))
				throw new InvalidOperationException($"Execution '{executionId.Value}' is already tracked.");

			var utcNow = _clock.UtcNow;
			var entry = new ExecutionEntry(executionId, name, ExecutionStatus.Running, utcNow, utcNow);

			_entries[executionId] = entry;
			Log(entry, "Execution started.");
			return entry;
		}
	}

	internal ExecutionEntry Report(ExecutionId executionId, string message, double? percent = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(message);
		lock (_lock)
		{
			var currentEntry = GetTrackedEntry(executionId);
			var entry = currentEntry with
			{
				UpdatedAt = _clock.UtcNow,
				ProgressMessage = message,
				ProgressPercent = percent,
			};

			_entries[executionId] = entry;
			Log(entry, "Execution progress reported.");
			return entry;
		}
	}

	internal ExecutionEntry Complete(ExecutionId executionId) =>
		Finish(executionId, ExecutionStatus.Completed, null, "Execution completed.");

	internal ExecutionEntry Cancel(ExecutionId executionId) =>
		Finish(executionId, ExecutionStatus.Cancelled, null, "Execution cancelled.");

	internal ExecutionEntry Fail(ExecutionId executionId, Error error) =>
		Finish(executionId, ExecutionStatus.Failed, error, "Execution failed.");

	private ExecutionEntry Finish(ExecutionId executionId, ExecutionStatus status, Error? error, string message)
	{
		lock (_lock)
		{
			var currentEntry = GetTrackedEntry(executionId);
			var utcNow = _clock.UtcNow;
			var entry = currentEntry with { Status = status, UpdatedAt = utcNow, FinishedAt = utcNow, Error = error };

			_entries[executionId] = entry;
			Log(entry, message);
			return entry;
		}
	}

	private ExecutionEntry GetTrackedEntry(ExecutionId executionId) =>
		_entries.TryGetValue(executionId, out var entry)
			? entry
			: throw new InvalidOperationException($"Execution '{executionId.Value}' is not tracked.");

	private void Log(ExecutionEntry entry, string message)
	{
		if (_logSink is null)
			return;

		var logEntry = new LogEntry(
			nameof(ExecutionBoard),
			message,
			LogCategory.Execution,
			EntryLogLevel(entry),
			_clock.UtcNow,
			CreateProperties(entry)
		);

		_logSink.Write(logEntry);
	}

	private void Log(ExecutionId executionId, string message)
	{
		if (_logSink is null)
			return;

		var logEntry = new LogEntry(
			nameof(ExecutionBoard),
			message,
			LogCategory.Execution,
			LogLevel.Info,
			_clock.UtcNow,
			CreateProperties(executionId)
		);

		_logSink.Write(logEntry);
	}

	private void Log(string message)
	{
		if (_logSink is null)
			return;

		var logEntry = new LogEntry(
			nameof(ExecutionBoard),
			message,
			LogCategory.Execution,
			LogLevel.Info,
			_clock.UtcNow
		);

		_logSink.Write(logEntry);
	}

	private static Dictionary<string, object?> CreateProperties(ExecutionEntry entry) =>
		new()
		{
			["ExecutionId"] = entry.Id.Value,
			["ExecutionName"] = entry.Name,
			["Status"] = entry.Status,
			["ProgressMessage"] = entry.ProgressMessage,
			["ProgressPercent"] = entry.ProgressPercent,
			["Error"] = entry.Error?.Code,
		};

	private static Dictionary<string, object?> CreateProperties(ExecutionId executionId) =>
		new() { ["ExecutionId"] = executionId.Value };

	private static LogLevel EntryLogLevel(ExecutionEntry entry) =>
		entry.Status == ExecutionStatus.Failed ? LogLevel.Error : LogLevel.Info;
}

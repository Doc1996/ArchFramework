using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Stores structured in-memory engine history and mirrors each entry to the configured log sink.
/// </summary>
public sealed class RuntimeLog(IClock clock, ILogSink logSink)
{
	private readonly SyncList<LogEntry> _logEntries = new();
	public IReadOnlyList<LogEntry> LogEntries => _logEntries.List();

	public LogEntry Add(LogCategory category, string message, params object?[] details) =>
		AddEntry(category, LogLevel.Info, message, GetCallerSource(), details);

	public LogEntry Add(LogCategory category, LogLevel level, string message, params object?[] details) =>
		AddEntry(category, level, message, GetCallerSource(), details);

	public void Clear() => _logEntries.Clear();

	private LogEntry AddEntry(LogCategory category, LogLevel level, string message, string source, object?[] details)
	{
		var properties = CreateProperties(details);
		var logEntry = new LogEntry(source, message, category, level, clock.UtcNow, properties);
		_logEntries.Add(logEntry);

		logSink.Write(logEntry);
		return logEntry;
	}

	private static IReadOnlyDictionary<string, object?>? CreateProperties(object?[] details)
	{
		var nonNullDetails = details.Where(detail => detail is not null).ToArray();
		if (nonNullDetails.Length == 0)
			return null;

		var properties = new Dictionary<string, object?>();
		var complexDetailCount = nonNullDetails.Count(IsComplexDetail);

		foreach (var detail in nonNullDetails)
			AddProperties(properties, detail!, complexDetailCount > 1);

		return properties.Count == 0 ? null : properties;
	}

	private static void AddProperties(Dictionary<string, object?> properties, object detail, bool prefixComplexDetails)
	{
		if (detail is IReadOnlyDictionary<string, object?> readOnlyDictionary)
		{
			Merge(properties, readOnlyDictionary);
			return;
		}

		if (detail is IDictionary dictionary)
		{
			foreach (DictionaryEntry entry in dictionary)
			{
				if (entry.Key is string key)
					properties[key] = Normalize(entry.Value);
			}
			return;
		}

		if (detail is IEvent appEvent)
		{
			properties["EventId"] = Normalize(appEvent.Id);
			properties["EventName"] = appEvent.Name;
			return;
		}

		var type = detail.GetType();
		if (IsIdValue(type))
		{
			properties[CleanName(type)] = Normalize(detail);
			return;
		}

		if (IsSimple(type))
		{
			properties[CleanName(type)] = Normalize(detail);
			return;
		}

		var prefix = prefixComplexDetails && !IsAnonymous(type) ? CleanName(type) + "." : string.Empty;
		foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
		{
			if (!property.CanRead || property.GetIndexParameters().Length > 0)
				continue;

			var value = property.GetValue(detail);
			if (value is not null && !IsSimple(value.GetType()) && !IsIdValue(value.GetType()))
				continue;

			properties[prefix + property.Name] = Normalize(value);
		}
	}

	private static void Merge(
		Dictionary<string, object?> properties,
		IReadOnlyDictionary<string, object?> additionalProperties
	)
	{
		foreach (var property in additionalProperties)
			properties[property.Key] = Normalize(property.Value);
	}

	private static object? Normalize(object? value)
	{
		if (value is null)
			return null;

		var type = value.GetType();
		var valueProperty = type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

		if (IsIdValue(type) && valueProperty is not null)
			return valueProperty.GetValue(value);

		return value;
	}

	private static bool IsComplexDetail(object? value)
	{
		if (value is null || IsDictionary(value) || value is IEvent)
			return false;

		var type = value.GetType();
		return !IsIdValue(type) && !IsSimple(type);
	}

	private static bool IsDictionary(object? value) => value is IReadOnlyDictionary<string, object?> or IDictionary;

	private static bool IsIdValue(Type type) =>
		type.Name.EndsWith("Id", StringComparison.Ordinal)
		&& type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public) is not null;

	private static bool IsSimple(Type type) =>
		type.IsPrimitive
		|| type.IsEnum
		|| type == typeof(string)
		|| type == typeof(decimal)
		|| type == typeof(DateTime)
		|| type == typeof(DateTimeOffset)
		|| type == typeof(TimeSpan)
		|| type == typeof(Guid);

	private static bool IsAnonymous(Type type) =>
		type.GetCustomAttribute<CompilerGeneratedAttribute>() is not null && type.Name.Contains("AnonymousType");

	private static string CleanName(Type type)
	{
		var name = type.Name;
		var genericSeparatorIndex = name.IndexOf('`', StringComparison.Ordinal);
		return genericSeparatorIndex < 0 ? name : name[..genericSeparatorIndex];
	}

	private static string GetCallerSource()
	{
		var frame = new System.Diagnostics.StackTrace().GetFrames()?.FirstOrDefault(IsCallerFrame);
		var method = frame?.GetMethod();
		var typeName = method?.DeclaringType?.Name;
		var methodName = method?.Name;

		if (string.IsNullOrWhiteSpace(typeName))
			return nameof(RuntimeLog);

		return string.IsNullOrWhiteSpace(methodName) ? typeName : $"{typeName}.{methodName}";
	}

	private static bool IsCallerFrame(System.Diagnostics.StackFrame frame)
	{
		var declaringType = frame.GetMethod()?.DeclaringType;
		return declaringType is not null && declaringType != typeof(RuntimeLog);
	}
}

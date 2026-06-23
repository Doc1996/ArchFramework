namespace Leva.Framework.Core;

/// <summary>
/// Base capability surface available to runtime objects while they execute.
/// </summary>
public interface IAccess
{
	ITransition Transition { get; }
	ILogSink LogSink { get; }
}

namespace Leva.Framework.Core;

/// <summary>
/// Represents one application state with its own event handling and entry/exit lifecycle.
/// </summary>
public interface IState<TAccess>
	where TAccess : IAccess
{
	StateId Id { get; }
	string Name { get; }

	Task EnterAsync(TAccess access, CancellationToken token);
	Task ExitAsync(TAccess access, CancellationToken token);
	Task<bool> HandleAsync(TAccess access, IEvent appEvent, CancellationToken token);
}

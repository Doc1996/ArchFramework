namespace Leva.Framework.Core;

/// <summary>
/// Allows runtime objects to request state changes without directly controlling the state machine.
/// </summary>
public interface ITransition
{
	void To(StateId stateId, string? reason = null);
	void Reenter(string? reason = null);
}

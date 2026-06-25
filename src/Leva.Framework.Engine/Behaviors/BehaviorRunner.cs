using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Runs fallback behaviors in binding order after state and routine handling have declined an event.
/// </summary>
public sealed class BehaviorRunner
{
	private readonly IReadOnlyList<IBehaviorBinding> _behaviors;
	private readonly IAccess _access;
	private readonly RuntimeLog _runtimeLog;

	internal BehaviorRunner(IEnumerable<IBehaviorBinding> behaviors, IAccess access, RuntimeLog runtimeLog)
	{
		_behaviors = behaviors.ToList();
		_access = access;
		_runtimeLog = runtimeLog;
	}

	public async Task<bool> HandleAsync(IEvent appEvent, CancellationToken token)
	{
		foreach (var behavior in _behaviors)
		{
			if (await behavior.HandleAsync(_access, appEvent, token))
			{
				_runtimeLog.Add(
					LogCategory.Behavior,
					"Event handled by behavior.",
					appEvent,
					new { BehaviorName = behavior.Name }
				);
				return true;
			}
		}

		return false;
	}
}

using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Type-erased behavior binding used by the behavior runner.
/// </summary>
internal interface IBehaviorBinding
{
	string Name { get; }
	Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token);
}

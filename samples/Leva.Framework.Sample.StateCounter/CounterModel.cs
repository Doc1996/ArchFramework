namespace Leva.Framework.Sample.StateCounter;

internal sealed class CounterModel
{
	public const int Target = 2;
	public const int WarningLimit = 2;

	public int Count { get; private set; }
	public string Report { get; private set; } = string.Empty;
	public bool TargetEvaluated { get; private set; }

	public void Increment() => Count++;

	public void MarkTargetEvaluated() => TargetEvaluated = true;

	public void SetReport(string report) => Report = report;
}

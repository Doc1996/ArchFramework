namespace Leva.Framework.Sample.LiveDashboard;

internal sealed record DashboardSelfCheck(bool Passed, string Message, int StoredBefore, int StoredAfter);

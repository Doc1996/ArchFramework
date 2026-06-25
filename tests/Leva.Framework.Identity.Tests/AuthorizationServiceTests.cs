using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class AuthorizationServiceTests
{
	[Fact]
	public async Task AuthorizeAsync_ReturnsDeniedWhenPrincipalIsMissing()
	{
		var service = new AuthorizationService([], new MemoryAuditSink());
		var requirement = AuthorizationRequirement.SignedIn;
		var result = await service.AuthorizeAsync(new AuthorizationRequest(null, requirement));
		var authorization = ResultAssert.Success(result);

		Assert.False(authorization.IsAuthorized);
		Assert.Equal(requirement, authorization.Requirement);
	}

	[Fact]
	public async Task AuthorizeAsync_RunsApplicablePoliciesUntilAuthorized()
	{
		var requirement = AuthorizationRequirement.Permission(new PrincipalPermission("plans.edit"));
		var first = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Ok(AuthorizationResult.Denied(requirement, "no")),
		};
		var second = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Ok(AuthorizationResult.Allowed(requirement)),
		};

		var audit = new MemoryAuditSink();
		var service = new AuthorizationService([first, second], audit);
		var principal = new Principal(new PrincipalId("user-1"), "User One");

		var result = await service.AuthorizeAsync(new AuthorizationRequest(principal, requirement));
		var authorization = ResultAssert.Success(result);

		Assert.True(authorization.IsAuthorized);
		Assert.Equal(1, first.AuthorizeCount);
		Assert.Equal(1, second.AuthorizeCount);

		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.Authorized && entry.PrincipalId == principal.Id
		);
	}

	[Fact]
	public async Task AuthorizeAsync_DeniesWhenNoPolicyAllowsRequirement()
	{
		var requirement = AuthorizationRequirement.Role(new PrincipalRole("admin"));
		var policy = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Ok(AuthorizationResult.Denied(requirement, "missing role")),
		};

		var audit = new MemoryAuditSink();
		var service = new AuthorizationService([policy], audit);
		var principal = new Principal(new PrincipalId("user-1"), "User One");

		var result = await service.AuthorizeAsync(new AuthorizationRequest(principal, requirement));
		var authorization = ResultAssert.Success(result);

		Assert.False(authorization.IsAuthorized);
		Assert.Equal("missing role", authorization.Reason);
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.AuthorizationFailed && entry.Reason == "missing role"
		);
	}

	[Fact]
	public async Task AuthorizeAsync_ReturnsPolicyFailure()
	{
		var requirement = AuthorizationRequirement.SignedIn;
		var policy = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Fail(PrincipalErrors.Failed("authorize", "boom")),
		};

		var service = new AuthorizationService([policy], new MemoryAuditSink());
		var principal = new Principal(new PrincipalId("user-1"), "User One");
		var result = await service.AuthorizeAsync(new AuthorizationRequest(principal, requirement));

		Assert.True(result.IsFailure);
		Assert.Equal("principal.failed", result.Error.Code);
	}
}

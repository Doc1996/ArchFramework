using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Registers ASP.NET Core adapters for framework identity principals, auth sessions, and authorization.
/// </summary>
public static class AspNetPrincipalServices
{
	public static IServiceCollection AddAspNetPrincipalServices(
		this IServiceCollection services,
		Action<AspNetIdentityOptions>? configure = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);
		var options = new AspNetIdentityOptions();
		configure?.Invoke(options);

		services.AddSingleton(Options.Create(options));
		services.AddHttpContextAccessor();
		services.AddSingleton<AspNetAuthSessionReader>();
		services.AddSingleton<AspNetAuthSessionWriter>();
		services.AddSingleton<AspNetClaimsPrincipalMapper>();

		services.AddScoped<AspNetSignInService>();
		services.AddScoped<AspNetAuthSessionSource>();
		services.AddScoped<IAuthorizationHandler, AspNetAuthorizationHandler>();

		services
			.AddAuthentication(options.AuthenticationScheme)
			.AddScheme<AspNetAuthenticationOptions, AspNetAuthenticationHandler>(
				options.AuthenticationScheme,
				_ => { }
			);

		return services;
	}

	public static IServiceCollection AddAspNetGooglePrincipalServices(
		this IServiceCollection services,
		Action<AspNetGoogleOptions>? configure = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);
		var options = new AspNetGoogleOptions();
		configure?.Invoke(options);

		services.AddSingleton(Options.Create(options));
		services.AddSingleton<AspNetGooglePrincipalMapper>();
		services.AddSingleton<AspNetGoogleAuthenticationPolicy>();
		services.AddSingleton<AuthenticationPolicy>(provider =>
			provider.GetRequiredService<AspNetGoogleAuthenticationPolicy>()
		);

		services.AddScoped<AspNetGoogleSignInService>();
		services.AddSingleton<HttpClient>();
		return services;
	}

	public static IServiceCollection AddAspNetJwtPrincipalServices(
		this IServiceCollection services,
		Action<AspNetJwtOptions>? configure = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);
		var options = new AspNetJwtOptions();
		configure?.Invoke(options);

		services.AddSingleton(Options.Create(options));
		services.AddSingleton<AspNetJwtClaimsMapper>();
		services.AddSingleton<AspNetJwtTokenService>();
		services.AddSingleton<AspNetJwtAuthenticationPolicy>();
		services.AddSingleton<AuthenticationPolicy>(provider =>
			provider.GetRequiredService<AspNetJwtAuthenticationPolicy>()
		);

		services
			.AddAuthentication()
			.AddScheme<AspNetJwtAuthenticationOptions, AspNetJwtAuthenticationHandler>(
				options.AuthenticationScheme,
				_ => { }
			);

		return services;
	}
}

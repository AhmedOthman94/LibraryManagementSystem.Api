using System.Text;
using LibraryManagementSystemAPI.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagementSystemAPI.Auth
{
	public static class AuthServiceCollectionExtensions
	{
		public static IServiceCollection AddLibraryAuth(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			// 1) JWT settings, validated at startup (missing key = app won't start)
			services.AddOptions<JwtOptions>()
				.Bind(configuration.GetSection(JwtOptions.SectionName))
				.ValidateDataAnnotations()
				.ValidateOnStart();

			// 2) Authentication: "read the Bearer token on every request"
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer();

			// Configure how tokens are validated, using JwtOptions
			// (read lazily, so tests can override the settings)
			services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
				.Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
				{
					var jwt = jwtOptions.Value;

					// Keep claim names exactly as we wrote them ("role", "sub"...)
					bearer.MapInboundClaims = false;

					bearer.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidIssuer = jwt.Issuer,

						ValidateAudience = true,
						ValidAudience = jwt.Audience,

						ValidateLifetime = true,
						ClockSkew = TimeSpan.FromSeconds(30),

						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),

						NameClaimType = AuthClaims.Email,
						RoleClaimType = AuthClaims.Role
					};
				});

			// 3) Authorization: secure by default + named policies
			services.AddAuthorizationBuilder()
				.SetFallbackPolicy(new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build())
				.AddPolicy(AuthPolicies.Staff, policy =>
					policy.RequireRole(nameof(UserRole.Librarian), nameof(UserRole.Admin)))
				.AddPolicy(AuthPolicies.Admin, policy =>
					policy.RequireRole(nameof(UserRole.Admin)))
				.AddPolicy(AuthPolicies.Member, policy =>
					policy.RequireRole(nameof(UserRole.Member)));

			// 4) Helpers
			services.AddHttpContextAccessor();
			services.AddScoped<ICurrentUser, CurrentUser>();
			services.AddSingleton<ITokenService, JwtTokenService>();
			services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

			return services;
		}
	}
}
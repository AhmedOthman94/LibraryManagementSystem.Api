using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace LibraryManagementSystemAPI.Auth
{
	public class AuthRateLimitOptions
	{
		public const string SectionName = "RateLimiting:Auth";

		[Range(1, 100000)]
		public int PermitLimit { get; set; } = 5;

		[Range(1, 3600)]
		public int WindowSeconds { get; set; } = 60;
	}

	public static class AuthRateLimiting
	{
		public const string PolicyName = "auth";

		public static IServiceCollection AddAuthRateLimiting(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddOptions<AuthRateLimitOptions>()
				.Bind(configuration.GetSection(AuthRateLimitOptions.SectionName))
				.ValidateDataAnnotations()
				.ValidateOnStart();

			services.AddRateLimiter(options =>
			{
				options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

				// One counter per client IP address
				options.AddPolicy(PolicyName, httpContext =>
				{
					var limits = httpContext.RequestServices
						.GetRequiredService<IOptions<AuthRateLimitOptions>>().Value;

					var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

					return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ =>
						new FixedWindowRateLimiterOptions
						{
							PermitLimit = limits.PermitLimit,
							Window = TimeSpan.FromSeconds(limits.WindowSeconds),
							QueueLimit = 0   // don't wait in line: reject immediately
						});
				});

				// Tell the client how long to wait
				options.OnRejected = (context, _) =>
				{
					if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
					{
						context.HttpContext.Response.Headers.RetryAfter =
							((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
					}

					return ValueTask.CompletedTask;
				};
			});

			return services;
		}
	}
}
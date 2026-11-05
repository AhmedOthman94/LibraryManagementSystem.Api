using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystemAPI.Auth
{
	public class JwtOptions
	{
		public const string SectionName = "Jwt";

		[Required]
		public string Issuer { get; set; } = "";

		[Required]
		public string Audience { get; set; } = "";

		// Comes from user secrets / environment, NEVER from appsettings.json
		[Required, MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters.")]
		public string SigningKey { get; set; } = "";

		[Range(5, 1440)]
		public int AccessTokenMinutes { get; set; } = 60;

		[Range(1, 90)]
		public int RefreshTokenDays { get; set; } = 14;
	}
}

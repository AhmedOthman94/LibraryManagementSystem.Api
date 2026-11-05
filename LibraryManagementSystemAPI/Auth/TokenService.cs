using System.Security.Claims;
using System.Text;
using LibraryManagementSystemAPI.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagementSystemAPI.Auth
{
	public record AccessToken(string Token, DateTime ExpiresAt);

	public interface ITokenService
	{
		AccessToken CreateAccessToken(User user);
	}

	public class JwtTokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
	{
		private readonly JwtOptions _jwt = jwtOptions.Value;
		private readonly JsonWebTokenHandler _handler = new();

		public AccessToken CreateAccessToken(User user)
		{
			// Token lifetimes are checked against the REAL clock by the JWT library,
			// so we create them with the real clock too (not our business TimeProvider).
			var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

			var claims = new List<Claim>
			{
				new(AuthClaims.Subject, user.Id.ToString()),
				new(AuthClaims.Email, user.Email),
				new(AuthClaims.Role, user.Role.ToString()),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			if (user.MemberId is Guid memberId)
			{
				claims.Add(new(AuthClaims.MemberId, memberId.ToString()));
			}

			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));

			var descriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Issuer = _jwt.Issuer,
				Audience = _jwt.Audience,
				Expires = expiresAt,
				SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
			};

			return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
		}
	}
}
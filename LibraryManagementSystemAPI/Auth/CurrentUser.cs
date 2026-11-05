using System.Security.Claims;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Auth
{
	// "Who is making this request?" in one clean place
	public interface ICurrentUser
	{
		bool IsAuthenticated { get; }
		Guid? UserId { get; }
		UserRole? Role { get; }
		Guid? MemberId { get; }
		bool IsStaff { get; }
	}

	public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
	{
		private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

		public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

		public Guid? UserId =>
			Guid.TryParse(Principal?.FindFirstValue(AuthClaims.Subject), out var id) ? id : null;

		public UserRole? Role =>
			Enum.TryParse<UserRole>(Principal?.FindFirstValue(AuthClaims.Role), out var role) ? role : null;

		public Guid? MemberId =>
			Guid.TryParse(Principal?.FindFirstValue(AuthClaims.MemberId), out var id) ? id : null;

		public bool IsStaff => Role is UserRole.Librarian or UserRole.Admin;
	}
}
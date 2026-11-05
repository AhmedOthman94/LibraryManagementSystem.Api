using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Dtos
{
	public record LoginRequest(string Email, string Password);

	// A member activates an online account with the details the library already has
	public record RegisterMemberAccountRequest(string CardNumber, string Email, string Password);

	// Admin creates staff accounts
	public record CreateStaffUserRequest(string Email, string Password, UserRole Role);

	public record UserResponse(Guid Id, string Email, UserRole Role, Guid? MemberId);

	public record AuthResponse(
	string AccessToken,
	DateTime AccessTokenExpiresAt,
	string RefreshToken,
	DateTime RefreshTokenExpiresAt,
	UserResponse User
);

	public record RefreshTokenRequest(string RefreshToken);

	public record LogoutRequest(string RefreshToken);

	public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Auth
{
	public interface IAuthService
	{
		Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
		Task<Result<AuthResponse>> RegisterMemberAccountAsync(RegisterMemberAccountRequest request, CancellationToken ct);
		Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken ct);
		Task LogoutAsync(LogoutRequest request, CancellationToken ct);
		Task LogoutAllAsync(Guid userId, CancellationToken ct);
		Task<Result<AuthResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct);
		Task<Result<UserResponse>> CreateStaffUserAsync(CreateStaffUserRequest request, CancellationToken ct);
		Task<Result<UserResponse>> GetUserAsync(Guid id, CancellationToken ct);
	}
}
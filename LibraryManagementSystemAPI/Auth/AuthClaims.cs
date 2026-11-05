namespace LibraryManagementSystemAPI.Auth
{
	// Claim names inside our tokens, in one place
	public static class AuthClaims
	{
		public const string Subject = "sub";
		public const string Email = "email";
		public const string Role = "role";
		public const string MemberId = "member_id";
	}
}
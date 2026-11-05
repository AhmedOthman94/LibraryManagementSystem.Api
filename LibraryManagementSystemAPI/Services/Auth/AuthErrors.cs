using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Services.Auth
{
	public static class AuthErrors
	{
		// Same message for "unknown email" and "wrong password" (no user enumeration)
		public static Error InvalidCredentials =>
			Error.Unauthorized("Email or password is incorrect.");

		// Same message whether the card or the email was wrong
		public static Error MemberNotMatched =>
			Error.Validation("The card number and email don't match our records.");

		public static Error MemberAlreadyHasAccount =>
			Error.Conflict("This member already has an online account.");

		public static Error EmailAlreadyUsed =>
			Error.Conflict("An account with this email already exists.");

		public static Error UserNotFound(Guid id) =>
			Error.NotFound($"User with Id: {id} was not found.");
	}
}
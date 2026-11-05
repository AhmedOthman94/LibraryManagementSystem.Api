using LibraryManagementSystemAPI.Common;

namespace LibraryManagementSystemAPI.Services.Members
{
	public static class MemberErrors
	{
		public static Error NotFound(Guid id) =>
			Error.NotFound($"Member with Id: {id} was not found.");

		public static Error EmailAlreadyExists(string email) =>
			Error.Conflict($"A member with email: {email} already exists.");

		public static Error AlreadySuspended =>
			Error.Conflict("This member is already suspended.");

		public static Error NotSuspended =>
			Error.Conflict("This member is not suspended.");

		public static Error SuspendedCannotRenew =>
			Error.Conflict("A suspended member can't renew. Reactivate the member first.");
	}
}
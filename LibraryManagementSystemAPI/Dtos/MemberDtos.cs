using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Dtos
{
	public interface IMemberRequest
	{
		string FirstName { get; }
		string LastName { get; }
		string Email { get; }
		string? PhoneNumber { get; }
	}

	public record CreateMemberRequest(
		string FirstName,
		string LastName,
		string Email,
		string? PhoneNumber
	) : IMemberRequest;

	public record UpdateMemberRequest(
		string FirstName,
		string LastName,
		string Email,
		string? PhoneNumber
	) : IMemberRequest;

	public record SuspendMemberRequest(string Reason);

	public record MemberResponse(
		Guid Id,
		string CardNumber,
		string FirstName,
		string LastName,
		string FullName,
		string Email,
		string? PhoneNumber,
		MemberStatus Status,
		string? SuspensionReason,
		DateOnly MembershipExpiresOn,
		bool IsMembershipExpired,
		bool CanBorrow,
		DateTime CreatedAt,
		DateTime? UpdatedAt
	);

	public class MemberQueryParameters
	{
		// Name, email, or card number
		public string? Search { get; set; }

		// "Active" or "Suspended"
		public string? Status { get; set; }

		public bool? ExpiredOnly { get; set; }

		public int? Page { get; set; }
		public int? PageSize { get; set; }
	}
}
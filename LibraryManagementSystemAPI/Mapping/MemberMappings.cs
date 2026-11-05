using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class MemberMappings
	{
		// "today" is needed for IsMembershipExpired and CanBorrow
		public static MemberResponse ToResponse(this Member member, DateOnly today) =>
			new(
				member.Id,
				MembershipCard.Format(member.MembershipNumber),
				member.FirstName,
				member.LastName,
				$"{member.FirstName} {member.LastName}",
				member.Email,
				member.PhoneNumber,
				member.Status,
				member.SuspensionReason,
				member.MembershipExpiresOn,
				member.IsMembershipExpired(today),
				member.CanBorrow(today),
				member.CreatedAt,
				member.UpdatedAt
			);

		public static Member ToEntity(this CreateMemberRequest request, DateOnly membershipExpiresOn) =>
			new()
			{
				FirstName = request.FirstName.Trim(),
				LastName = request.LastName.Trim(),
				Email = EmailHelper.Normalize(request.Email),
				PhoneNumber = request.PhoneNumber?.Trim(),
				MembershipExpiresOn = membershipExpiresOn
			};

		public static void ApplyTo(this UpdateMemberRequest request, Member member)
		{
			member.FirstName = request.FirstName.Trim();
			member.LastName = request.LastName.Trim();
			member.Email = EmailHelper.Normalize(request.Email);
			member.PhoneNumber = request.PhoneNumber?.Trim();
		}
	}
}
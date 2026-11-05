using LibraryManagementSystemAPI.Dtos;
using LibraryManagementSystemAPI.Entities;

namespace LibraryManagementSystemAPI.Mappings
{
	public static class UserMappings
	{
		public static UserResponse ToResponse(this User user) =>
			new(user.Id, user.Email, user.Role, user.MemberId);
	}
}
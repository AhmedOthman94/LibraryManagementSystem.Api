using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystemAPI.Common
{
	// Business settings, read from the "LibraryPolicy" section of appsettings.json
	public class LibraryPolicyOptions
	{
		public const string SectionName = "LibraryPolicy";

		[Range(1, 120, ErrorMessage = "MembershipDurationMonths must be between 1 and 120.")]
		public int MembershipDurationMonths { get; set; } = 12;
	}
}
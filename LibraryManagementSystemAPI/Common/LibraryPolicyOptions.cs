using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystemAPI.Common
{
	// Business settings, read from the "LibraryPolicy" section of appsettings.json
	public class LibraryPolicyOptions
	{
		public const string SectionName = "LibraryPolicy";

		[Range(1, 120, ErrorMessage = "MembershipDurationMonths must be between 1 and 120.")]
		public int MembershipDurationMonths { get; set; } = 12;

		[Range(1, 90, ErrorMessage = "LoanPeriodDays must be between 1 and 90.")]
		public int LoanPeriodDays { get; set; } = 14;

		[Range(1, 50, ErrorMessage = "MaxActiveLoansPerMember must be between 1 and 50.")]
		public int MaxActiveLoansPerMember { get; set; } = 5;

		[Range(0, 10, ErrorMessage = "MaxRenewals must be between 0 and 10.")]
		public int MaxRenewals { get; set; } = 2;

		[Range(typeof(decimal), "0", "1000", ErrorMessage = "DailyFineAmount must be between 0 and 1000.")]
		public decimal DailyFineAmount { get; set; } = 5m;

		[Range(typeof(decimal), "0", "100000", ErrorMessage = "MaxFinePerLoan must be between 0 and 100000.")]
		public decimal MaxFinePerLoan { get; set; } = 200m;
	}
}
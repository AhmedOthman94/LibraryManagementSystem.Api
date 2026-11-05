namespace LibraryManagementSystemAPI.Common
{
	public static class FineCalculator
	{
		// 3 days late × 5 = 15, but never more than the maximum
		public static decimal Calculate(int daysOverdue, LibraryPolicyOptions policy)
		{
			if (daysOverdue <= 0)
			{
				return 0m;
			}

			var fine = daysOverdue * policy.DailyFineAmount;

			return Math.Min(fine, policy.MaxFinePerLoan);
		}
	}
}
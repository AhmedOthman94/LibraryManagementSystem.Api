namespace LibraryManagementSystemAPI.Entities
{
	// Calculated from dates, never stored in the database
	public enum LoanStatus
	{
		Active,
		Overdue,
		Returned
	}
}
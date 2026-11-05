namespace LibraryManagementSystemAPI.Common
{
	public static class EmailHelper
	{
		// " Ahmed@Mail.com " -> "ahmed@mail.com"
		public static string Normalize(string email) =>
			email.Trim().ToLowerInvariant();
	}
}
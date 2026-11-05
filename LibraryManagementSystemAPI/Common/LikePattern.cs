namespace LibraryManagementSystemAPI.Common
{
	public static class LikePattern
	{
		public static string Contains(string term)
		{
			var escaped = term.Trim()
				.Replace(@"\", @"\\")
				.Replace("%", @"\%")
				.Replace("_", @"\_");

			return $"%{escaped}%";
		}
	}
}
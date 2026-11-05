namespace LibraryManagementSystemAPI.Common
{
	public static class BarcodeHelper
	{
		// " lib-000123 " -> "LIB-000123"
		public static string Normalize(string barcode) =>
			barcode.Trim().ToUpperInvariant();
	}
}
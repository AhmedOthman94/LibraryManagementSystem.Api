using System.Globalization;

namespace LibraryManagementSystemAPI.Common
{
	public static class MembershipCard
	{
		// 123 -> "M-000123"
		public static string Format(int membershipNumber) =>
			$"M-{membershipNumber:D6}";

		// Accepts "M-000123", "m000123", or "123"
		public static bool TryParse(string? text, out int membershipNumber)
		{
			membershipNumber = 0;

			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			var value = text.Trim().ToUpperInvariant();

			if (value.StartsWith("M-"))
			{
				value = value[2..];
			}
			else if (value.StartsWith('M'))
			{
				value = value[1..];
			}

			return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out membershipNumber)
				&& membershipNumber > 0;
		}
	}
}
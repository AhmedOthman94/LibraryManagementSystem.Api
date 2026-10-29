namespace LibraryManagementSystemAPI.Common
{
	public static class IsbnHelper
	{
		public static string Normalize(string isbn) =>
			isbn.Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();

		public static bool IsValid(string? isbn)
		{
			if (string.IsNullOrWhiteSpace(isbn))
			{
				return false;
			}

			var value = Normalize(isbn);

			return value.Length switch
			{
				10 => IsValidIsbn10(value),
				13 => IsValidIsbn13(value),
				_ => false
			};
		}

		private static bool IsValidIsbn10(string isbn)
		{
			var sum = 0;

			for (var i = 0; i < 10; i++)
			{
				int digit;

				if (i == 9 && isbn[i] == 'X')
				{
					digit = 10;
				}
				else if (char.IsAsciiDigit(isbn[i]))
				{
					digit = isbn[i] - '0';
				}
				else
				{
					return false;
				}

				sum += digit * (10 - i);
			}

			return sum % 11 == 0;
		}

		private static bool IsValidIsbn13(string isbn)
		{
			if (!isbn.All(char.IsAsciiDigit))
			{
				return false;
			}

			var sum = 0;

			for (var i = 0; i < 13; i++)
			{
				sum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);
			}

			return sum % 10 == 0;
		}
	}
}
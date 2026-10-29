namespace LibraryManagementSystemAPI.Common
{
	public static class UrlHelper
	{
		public static bool IsValidHttpUrl(string? url) =>
			Uri.TryCreate(url, UriKind.Absolute, out var uri)
			&& (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
	}
}
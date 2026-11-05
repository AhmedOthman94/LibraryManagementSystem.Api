namespace LibraryManagementSystemAPI.Common
{
	public enum ErrorType
	{
		Validation,
		NotFound,
		Conflict
	}

	public record Error(ErrorType Type, string Message)
	{
		public static Error Validation(string message) => new(ErrorType.Validation, message);
		public static Error NotFound(string message) => new(ErrorType.NotFound, message);
		public static Error Conflict(string message) => new(ErrorType.Conflict, message);
	}
}
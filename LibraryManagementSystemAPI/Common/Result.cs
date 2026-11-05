using System.Diagnostics.CodeAnalysis;

namespace LibraryManagementSystemAPI.Common
{
	public class Result
	{
		protected Result(Error? error)
		{
			Error = error;
		}

		public Error? Error { get; }

		[MemberNotNullWhen(false, nameof(Error))]
		public bool IsSuccess => Error is null;

		[MemberNotNullWhen(true, nameof(Error))]
		public bool IsFailure => Error is not null;

		public static Result Success() => new(null);
		public static Result Failure(Error error) => new(error);

		public static implicit operator Result(Error error) => Failure(error);
	}

	public class Result<T> : Result
	{
		private readonly T? _value;

		private Result(T value) : base(null)
		{
			_value = value;
		}

		private Result(Error error) : base(error)
		{
		}

		public T Value => IsSuccess
			? _value!
			: throw new InvalidOperationException("Can't read the value of a failed result.");

		public static Result<T> Success(T value) => new(value);
		public static new Result<T> Failure(Error error) => new(error);

		public static implicit operator Result<T>(T value) => Success(value);
		public static implicit operator Result<T>(Error error) => Failure(error);
	}
}
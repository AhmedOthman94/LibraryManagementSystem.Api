using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Loans
{
	public interface ILoanService
	{
		Task<PagedResponse<LoanResponse>> GetAllAsync(LoanQueryParameters parameters, CancellationToken ct);
		Task<Result<LoanResponse>> GetByIdAsync(Guid id, Guid? restrictToMemberId, CancellationToken ct);
		Task<Result<LoanResponse>> RenewAsync(Guid id, Guid? restrictToMemberId, CancellationToken ct);
		Task<Result<LoanResponse>> BorrowAsync(BorrowBookRequest request, CancellationToken ct);
		Task<Result<LoanResponse>> ReturnAsync(Guid id, ReturnLoanRequest? request, CancellationToken ct);

	}
}
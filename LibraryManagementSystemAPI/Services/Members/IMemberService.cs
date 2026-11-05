using LibraryManagementSystemAPI.Common;
using LibraryManagementSystemAPI.Dtos;

namespace LibraryManagementSystemAPI.Services.Members
{
	public interface IMemberService
	{
		Task<PagedResponse<MemberResponse>> GetAllAsync(MemberQueryParameters parameters, CancellationToken ct);
		Task<Result<MemberResponse>> GetByIdAsync(Guid id, CancellationToken ct);
		Task<Result<MemberResponse>> CreateAsync(CreateMemberRequest request, CancellationToken ct);
		Task<Result<MemberResponse>> UpdateAsync(Guid id, UpdateMemberRequest request, CancellationToken ct);
		Task<Result<MemberResponse>> SuspendAsync(Guid id, SuspendMemberRequest request, CancellationToken ct);
		Task<Result<MemberResponse>> ReactivateAsync(Guid id, CancellationToken ct);
		Task<Result<MemberResponse>> RenewAsync(Guid id, CancellationToken ct);
		Task<Result> DeleteAsync(Guid id, CancellationToken ct);
	}
}
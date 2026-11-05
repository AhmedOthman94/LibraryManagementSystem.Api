using LibraryManagementSystemAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystemAPI.Common
{
	public static class QueryableExtensions
	{
		public static async Task<PagedResponse<TResponse>> ToPagedResponseAsync<TEntity, TResponse>(
			this IQueryable<TEntity> query,
			int page,
			int pageSize,
			Func<TEntity, TResponse> map,
			CancellationToken ct)
		{
			var totalCount = await query.CountAsync(ct);

			var entities = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(ct);

			var items = entities.Select(map).ToList();

			return new PagedResponse<TResponse>(items, page, pageSize, totalCount);
		}
	}
}
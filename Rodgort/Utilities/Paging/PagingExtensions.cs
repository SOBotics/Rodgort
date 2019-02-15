using System;
using System.Linq;

namespace Rodgort.Utilities.Paging
{
    public static class PagingExtensions
    {
        public static PagingResponse<T> Page<T>(this IOrderedQueryable<T> query, PagingRequest pagingRequest)
        {
            return query.Page(pagingRequest.PageNumber ?? 1, pagingRequest.PageSize ?? 50);
        }
        public static PagingResponse<T> Page<T>(this IOrderedQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageSize > 100)
                pageSize = 100;

            var count = query.Count();
            var data =
                query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            return new PagingResponse<T>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(1.0 * count / pageSize),
                Data = data
            };
        }
    }
}

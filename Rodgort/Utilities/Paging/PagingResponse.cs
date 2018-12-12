using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rodgort.Utilities.Paging
{
    public class PagingResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<T> Data { get; set; }
    }
}

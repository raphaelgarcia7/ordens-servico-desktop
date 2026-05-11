using System.Collections.Generic;

namespace GestaoOS.Application.Common
{
    public class PagedResult<T>
    {
        public IList<T> Items { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedResult()
        {
            Items = new List<T>();
        }
    }
}

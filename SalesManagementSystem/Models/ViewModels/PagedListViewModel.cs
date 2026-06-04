using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public abstract class PagedListBase
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 1;
        public string Keyword { get; set; }
        public string ActionName { get; set; } = "Index";
    }

    public class PagedListViewModel<T> : PagedListBase
    {
        public IEnumerable<T> Items { get; set; }
    }
}

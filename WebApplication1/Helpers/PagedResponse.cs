namespace DefaultHRManagementSystem.Helpers
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } // The list of items for the current page
        public int Page { get; set; } // Current page number
        public int PageSize { get; set; } // Number of items per page
        public int TotalCount { get; set; } // Total number of items
        public int TotalPages { get; set; } // Total number of pages
    }
}

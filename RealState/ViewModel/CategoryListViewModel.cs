using RealStateModel;

namespace RealState.ViewModel
{
    public class CategoryListViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = [];

        public string? SearchString { get; set; }

        public string? SortOrder { get; set; }

        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }
    }
}

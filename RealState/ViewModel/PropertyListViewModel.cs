using RealStateModel;

namespace RealState.ViewModel
{
    public class PropertyListViewModel
    {
        public IEnumerable<Property> Properties { get; set; } = [];
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}

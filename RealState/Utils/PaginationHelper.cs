namespace RealState.Utils
{
    public static class PaginationHelper
    {
        public static List<T> Paginate<T>(IQueryable<T> source, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;
            return source.Skip(skip).Take(pageSize).ToList();
        }
    }
}

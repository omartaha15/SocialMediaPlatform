namespace SocialMedia.Web.ViewModels.Search
{
    public class SearchResultViewModel<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
namespace SocialMedia.Web.ViewModels.Search
{
    public class SearchRequestViewModel
    {
        public string Query { get; set; } = null!;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
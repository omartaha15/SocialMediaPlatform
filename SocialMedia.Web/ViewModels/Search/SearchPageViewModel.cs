using SocialMedia.Web.ViewModels.User;

namespace SocialMedia.Web.ViewModels.Search
{
    public class SearchPageViewModel
    {
        public string Query { get; set; } = string.Empty;
        public SearchResultViewModel<UserSearchViewModel> Result { get; set; } = new();
    }
}

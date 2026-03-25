namespace SocialMedia.Application.DTOs.Search
{
    public class SearchRequestDto
    {
        public string Query { get; set; } = null!;
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
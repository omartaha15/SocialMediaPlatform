using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Web.ViewModels.User;
using SocialMedia.Web.ViewModels.Search;
using SocialMedia.Application.DTOs.Search;
using SocialMedia.Application.Interfaces.Services;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] SearchRequestViewModel request)
        {
            var dtoRequest = new SearchRequestDto
            {
                Query = request.Query,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var result = await _searchService.SearchUsersAsync(dtoRequest);

            var viewModel = new SearchResultViewModel<UserSearchViewModel>
            { 
                Data = result.Data
                    .Select(u => new UserSearchViewModel
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        ProfileImage = u.ProfileImage
                    })
                    .ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Json(viewModel);
        }
    }
}
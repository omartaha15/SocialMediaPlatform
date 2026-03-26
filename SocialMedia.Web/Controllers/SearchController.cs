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
        public async Task<IActionResult> Index([FromQuery] SearchRequestViewModel request)
        {
            var result = await SearchUsersInternalAsync(request);
            var viewModel = new SearchPageViewModel
            {
                Query = request.Query ?? string.Empty,
                Result = result
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] SearchRequestViewModel request)
        {
            var viewModel = await SearchUsersInternalAsync(request);
            return Json(viewModel);
        }

        private async Task<SearchResultViewModel<UserSearchViewModel>> SearchUsersInternalAsync(SearchRequestViewModel request)
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

            return viewModel;
        }
    }
}
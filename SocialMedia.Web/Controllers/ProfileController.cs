using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs.ProfileDTOs;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using SocialMedia.Web.ViewModels;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IFriendshipService _friendshipService;
        private readonly IPostService _postService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(
            IProfileService profileService,
            IFriendshipService friendshipService,
            IPostService postService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) 
        {
            _profileService = profileService;
            _friendshipService = friendshipService;
            _postService = postService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

       
        [HttpGet]
        public async Task<IActionResult> Index(string id = null)
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (loggedInUserId == null)
                return RedirectToAction("Login", "Account");

            var targetUserId = string.IsNullOrEmpty(id) ? loggedInUserId : id;

            var profile = await _profileService.GetProfileAsync(targetUserId);
            if (profile == null) return NotFound();


            var posts = await _postService.GetPostsByUserIdAsync(Guid.Parse(targetUserId)); ViewBag.UserPosts = posts;

            ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(targetUserId);
            ViewBag.IsOwner = (loggedInUserId == targetUserId);

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(EditProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var currentProfile = await _profileService.GetProfileAsync(userId);
                return View("Index", currentProfile);
            }

            try
            {

                await _profileService.UpdateProfileAsync(userId, new EditProfileDto
                {
                    UserName = model.UserName,
                    Bio = model.Bio,
                    Location = model.Location,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender
                });

       
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _signInManager.RefreshSignInAsync(user);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("UserName", ex.Message);


                var profile = await _profileService.GetProfileAsync(userId);
                profile.UserName = model.UserName;


                ViewBag.ShowEditModal = true;

                return View("Index", profile);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("ProfileImage", "Please select an image.");
                var profile = await _profileService.GetProfileAsync(userId);
                ViewBag.ShowProfileImageModal = true;
                ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                ViewBag.IsOwner = true;
                ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                return View("Index", profile);
            }

            try
            {
                await _profileService.UploadProfileImageAsync(userId, image);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ProfileImage", ex.Message);
                var profile = await _profileService.GetProfileAsync(userId);
                ViewBag.ShowProfileImageModal = true;
                ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                ViewBag.IsOwner = true;
                ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                return View("Index", profile);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCover(IFormFile image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("CoverImage", "Please select an image.");
                var profile = await _profileService.GetProfileAsync(userId);
                ViewBag.ShowCoverModal = true;
                ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                ViewBag.IsOwner = true;
                ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                return View("Index", profile);
            }

            try
            {
                await _profileService.UploadCoverAsync(userId, image);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CoverImage", ex.Message);
                var profile = await _profileService.GetProfileAsync(userId);
                ViewBag.ShowCoverModal = true;
                ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                ViewBag.IsOwner = true;
                ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                return View("Index", profile);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            await _profileService.RemoveProfileImageAsync(userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCover()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            await _profileService.RemoveCoverAsync(userId);
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.ShowChangePasswordModal = true;
                var currentProfile = await _profileService.GetProfileAsync(userId);
                ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                ViewBag.IsOwner = true;
                ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                return View("Index", currentProfile);
            }

            try
            {
                var dto = new ChangePasswordDto
                {
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                };

                var result = await _profileService.ChangePasswordAsync(userId, dto);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ViewBag.ShowChangePasswordModal = true;
                    var currentProfile = await _profileService.GetProfileAsync(userId);
                    ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                    ViewBag.IsOwner = true;
                    ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                    return View("Index", currentProfile);
                }

                TempData["SuccessMessage"] = "Password updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while changing the password.");
                ViewBag.ShowChangePasswordModal = true;
                var currentProfile = await _profileService.GetProfileAsync(userId);
                ViewBag.FriendsCount = await _friendshipService.GetFriendsCountAsync(userId);
                ViewBag.IsOwner = true;
                ViewBag.UserPosts = await _postService.GetPostsByUserIdAsync(Guid.Parse(userId));
                return View("Index", currentProfile);
            }
        }
    }
}
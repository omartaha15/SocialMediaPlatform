using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTOs.AuthenticationDTOs;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.ViewModels;

namespace SocialMedia.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }
        #region register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var existingUserByName = await _userService.FindByUserNameAsync(model.UserName);
            if (existingUserByName != null)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
                return View(model);
            }

            var existingUserByEmail = await _userService.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            try
            {
                RegisterDto dto = new RegisterDto();

                dto.UserName = model.UserName;
                dto.Email = model.Email;
                dto.Password = model.Password;
                dto.ConfirmPassword = model.ConfirmPassword;
                dto.Bio = model.Bio;
                dto.Location = model.Location;
                dto.DateOfBirth = model.DateOfBirth;
                dto.Gender = model.Gender;
                dto.ProfileImage = model.ProfileImage;

                var result = await _userService.RegisterAsync(dto);

                if (result.Succeeded)
                    return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ProfileImage", ex.Message);
            }

            return View(model);
        }
        #endregion
        #region login
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
               LoginDto dto = new LoginDto();
                dto.Email = model.Email;
                dto.Password = model.Password;
                dto.RememberMe = model.RememberMe;


                var result = await _userService.LoginAsync(dto);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                ModelState.AddModelError("", "Invalid login attempt.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }
        #endregion
        #region signout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }
        #endregion

    }
}

using Microsoft.AspNetCore.Identity;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialMedia.Application.DTOs.AuthenticationDTOs;
using SocialMedia.Application.Interfaces.Services;


namespace SocialMedia.Application.Services
{
   
    public class UserService: IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IImageService _imageService;
        
        public UserService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IImageService imageService
                            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageService = imageService;
           
        }
        #region register
        public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
        {
            string imagePath = "/images/user.jpg"; // default

            if (dto.ProfileImage != null)
            {
                imagePath = await _imageService.UploadImageAsync(dto.ProfileImage);
            }

            ApplicationUser user = new ApplicationUser();
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.Bio = dto.Bio;
            user.Location = dto.Location;
            user.DateOfBirth = dto.DateOfBirth;
            user.Gender = dto.Gender;
            user.ProfilePictureUrl = imagePath;
            var result = await _userManager.CreateAsync(user, dto.Password);
            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            else
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result;

        }
        #endregion
        #region login
        public async Task<SignInResult> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return SignInResult.Failed;

            var result = await _signInManager.PasswordSignInAsync(
                user,
                dto.Password,
                isPersistent: dto.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

             
            }

            return result;
        }
        #endregion
        #region logout
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        #endregion
        public async Task<ApplicationUser?> FindByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }
        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


    }
}

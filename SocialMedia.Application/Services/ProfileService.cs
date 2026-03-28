using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SocialMedia.Application.DTOs.ProfileDTOs;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Services
{
    public class ProfileService: IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageService _imageService;

        public ProfileService(UserManager<ApplicationUser> userManager,
                              IImageService imageService)
        {
            _userManager = userManager;
            _imageService = imageService;
        }
        public async Task<ProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found!");

            return new ProfileDto
            {
                UserName = user.UserName,
                Bio = user.Bio,
                Location = user.Location,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CoverPhotoUrl = user.CoverPhotoUrl
            };
        }
        public async Task UpdateProfileAsync(string userId, EditProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found!");

         
            if (user.UserName != dto.UserName)
            {
               
                var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null && existingUser.Id != userId)
                {
                   
                    throw new Exception("Username is already taken. Please choose another one.");
                }

          
                user.UserName = dto.UserName;
            }

            user.Bio = dto.Bio;
            user.Location = dto.Location;
            user.DateOfBirth = dto.DateOfBirth;
            user.Gender = dto.Gender;

          
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to update profile: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        public async Task<string> UploadProfileImageAsync(string userId, IFormFile image)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found!");
            _imageService.DeleteImageAsync(user.ProfilePictureUrl);

            var path = await _imageService.UploadImageAsync(image);
            user.ProfilePictureUrl = path;

            await _userManager.UpdateAsync(user);
            return path;
        }
        public async Task<string> UploadCoverAsync(string userId, IFormFile image)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found!");
            _imageService.DeleteImageAsync(user.CoverPhotoUrl);

            var path = await _imageService.UploadImageAsync(image);
            user.CoverPhotoUrl = path;

            await _userManager.UpdateAsync(user);
            return path;
        }
        public async Task RemoveProfileImageAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found!");
            _imageService.DeleteImageAsync(user.ProfilePictureUrl);
            user.ProfilePictureUrl = "/images/user.jpg";

            await _userManager.UpdateAsync(user);
        }
        public async Task RemoveCoverAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found!");
            _imageService.DeleteImageAsync(user.CoverPhotoUrl);
            user.CoverPhotoUrl = "/images/coverImage.png";

            await _userManager.UpdateAsync(user);
        }
        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            return await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        }
    }
}

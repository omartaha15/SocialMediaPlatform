using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SocialMedia.Application.DTOs.ProfileDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfileAsync(string userId);
        Task UpdateProfileAsync(string userId, EditProfileDto dto);
        Task<string> UploadProfileImageAsync(string userId, IFormFile image);
        Task<string> UploadCoverAsync(string userId, IFormFile image);
        Task RemoveProfileImageAsync(string userId);
        Task RemoveCoverAsync(string userId);

        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
   
}

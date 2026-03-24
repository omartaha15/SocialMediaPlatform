using Microsoft.AspNetCore.Identity;
using SocialMedia.Application.DTOs.AuthenticationDTOs;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public  interface IUserService 
    {

        Task<IdentityResult> RegisterAsync(RegisterDto dto);
        Task<SignInResult> LoginAsync(LoginDto dto);  
        Task LogoutAsync();
        Task<ApplicationUser?> FindByUserNameAsync(string userName);
        Task<ApplicationUser?> FindByEmailAsync(string email);
    }
}

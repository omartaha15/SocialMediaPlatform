using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
namespace SocialMedia.Application.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
       void DeleteImageAsync(string imagePath);
    }
}

using SocialMedia.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
namespace SocialMedia.Application.Services
{
   public  class ImageService: IImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "/images/user.jpg";

     
            var regex = new System.Text.RegularExpressions.Regex(@"\.(jpg|jpeg|png|gif|bmp)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!regex.IsMatch(file.FileName))
                throw new Exception("Invalid file type. Only image files are allowed.");

         
            long maxFileSize = 2 * 1024 * 1024; 
            if (file.Length > maxFileSize)
                throw new Exception("File size exceeds 2 MB limit.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }
    }
}

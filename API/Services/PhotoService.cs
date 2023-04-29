using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account
            (
                config.Value.CloudName, 
                config.Value.ApiKey, 
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }
        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file) // the function gets a file of photo
        {
            var uploadResult = new ImageUploadResult();
            
            if (file.Length > 0) // if the file is not empty
            {
                using var stream = file.OpenReadStream(); // open stream to read the file
                var uploadParams = new ImageUploadParams // creating upload params
                {
                    File = new FileDescription(file.FileName, stream), // take the file name and the stream
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"), // transform to be square
                    Folder = "da-net7" // where to put the image in cloudinary
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams); // uploading the photo to cloudinary
            }

            return uploadResult; // return the result of uploading an image
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
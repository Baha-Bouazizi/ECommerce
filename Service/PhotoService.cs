using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using CommerceElectronique.Helpers;
using CommerceElectronique.Interfaces;
using Microsoft.Extensions.Options;

namespace CommerceElectronique.Service
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.Name, file.OpenReadStream()),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult;
            }
            catch (Exception ex)
            {
                // Handle or log the error here
                throw new Exception("There was an error uploading the image to Cloudinary.", ex);
            }
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result;
            }
            catch (Exception ex)
            {
                // Handle or log the error here
                throw new Exception("There was an error deleting the image from Cloudinary.", ex);
            }
        }
    }
}

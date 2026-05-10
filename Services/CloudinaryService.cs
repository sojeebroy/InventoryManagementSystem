using System;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Inventory_Management_System.Services.Interfaces;
namespace Inventory_Management_System.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {

        try
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        }
        catch (System.Exception ex)
        {

            throw new InvalidOperationException("Failed to load .env file.", ex);
        }


        var CloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");

        this._cloudinary = new Cloudinary(CloudinaryUrl) { Api = { Secure = true } };
    }

    public async Task<string?> UploadImageAsync(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            throw new ArgumentException("Image is missing.");
        }

        using var stream = image.OpenReadStream();


        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(image.FileName, stream),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true,
            Folder = "Inventory Manager"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }
}
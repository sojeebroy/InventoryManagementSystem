using Microsoft.AspNetCore.Http;

namespace Inventory_Management_System.Services.Interfaces;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile image);
}
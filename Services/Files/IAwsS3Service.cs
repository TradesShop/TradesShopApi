using System.Threading.Tasks;
using TradePlatform.Api.DTOs;

namespace TradePlatform.Api.Services.Files
{
    public interface IAwsS3Service
    {
        string GetPreSignedUploadUrl(string key, string contentType);
        string GetPreSignedReadUrl(string key);
        string GetObjectUrl(string key);
        //Task DeleteObjectAsync(string key);
        Task DeleteFileAsync(FileDeleteRequestDto dto);
    }
}

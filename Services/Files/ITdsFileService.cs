using TradePlatform.Api.DTOs.Files;

namespace TradePlatform.Api.Services.Files
{
    public interface ITdsFileService
    {
        Task<IEnumerable<UploadFilesDto>> GetUploadFilesAsync(FilesGetRequestDto fgrDto);
    }
}

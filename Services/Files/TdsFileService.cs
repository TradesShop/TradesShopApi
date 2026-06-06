using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Files
{
    public class TdsFileService : ITdsFileService
    {
        private readonly IFileRepository _fileRepo;
        
        public TdsFileService(
             IFileRepository fileRepo
            )
        {
            _fileRepo = fileRepo;
            
        }
        public async Task<IEnumerable<UploadFilesDto>> GetUploadFilesAsync(FilesGetRequestDto fgrDto)
        {
            return await _fileRepo.GetUploadFilesAsync(fgrDto);
        }
    }
}

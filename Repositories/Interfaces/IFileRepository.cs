using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.FileIO;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<(uFile file, uFilelink link)> InsertFileWithLinkAsync(
            string file_name,
            string file_url,
            string file_type,
            int size_kb,
            int entity_type,
            Guid entity_id,
            string upload_type,
            string work_stage,
            bool is_primary
        );

        Task<IEnumerable<UploadFilesDto>> GetUploadFilesAsync(FilesGetRequestDto fgrDto);
        Task UpdateDescriptionAsync(Guid fileId, string description);

        Task DeleteFileAsync(Guid fileId);
    }
}

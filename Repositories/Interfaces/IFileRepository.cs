using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IFileRepository
{
	Task<(uFile file, uFilelink link)> InsertFileWithLinkAsync(FileUploadRequestDto fu_req);

	Task<IEnumerable<UploadFilesDto>> GetUploadFilesAsync(FilesGetRequestDto fgrDto);

	Task UpdateDescriptionAsync(Guid fileId, string description);

	Task DeleteFileAsync(Guid fileId);
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Files;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureFilesController : BaseController
    {
        private readonly IFileRepository _repo;
        private readonly AzureBlobService _blob;
        private readonly ITdsFileService _fileService;

        public AzureFilesController(IFileRepository repo
            , AzureBlobService blob
            , ITdsFileService fileService
            ,IHttpContextAccessor http
        ) : base(http)
        {
            _repo = repo;
            _blob = blob;
            _fileService = fileService;
        }

        [HttpPost("get")]
        public async Task<IActionResult> Get(FilesGetRequestDto fgrDto)
        {
            var files = await _fileService.GetUploadFilesAsync(fgrDto);
            var result = files.Select(f => new
            {
                f.id,
                f.file_name,
                f.file_url,
                f.file_type,
                f.file_size,
                f.created_at,
                f.work_stage,
                readUrl = _blob.GetReadSasUrl(f.file_name)
            });
            return Ok(result);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] FileUploadRequestDto dto)
        {
            var sasUrl = _blob.GetUploadSasUrl(dto.filename, dto.content_type);
            var blobUrl = _blob.GetBlobUrl(dto.filename);
            var readUrl = _blob.GetReadSasUrl(dto.filename);

            var (file, link) = await _repo.InsertFileWithLinkAsync(
                dto.filename,
                blobUrl,
                dto.content_type,
                0,
                dto.entity_type,
                dto.entity_id,
                dto.upload_type,
                dto.work_stage,

                false
            );

            return Ok(new
            {
                uploadUrl = sasUrl,
                blobUrl,
                readUrl,
                dbRecord = file,
                linkRecord = link
            });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] FileDeleteRequestDto fdrDto)
        {
            // 2. Delete blob from Azure
            await _blob.DeleteBlobAsync(fdrDto);
            await _repo.DeleteFileAsync(fdrDto.id);
            return ApiOk();
        }
        [HttpPost("description")]
        public async Task<IActionResult> UpdateDescription([FromBody] UpdateDescriptionDto dto)
        {
            await _repo.UpdateDescriptionAsync(dto.file_id, dto.description);
            return Ok();
        }

    }
}

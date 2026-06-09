using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Files;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AwsFilesController : BaseController
    {
        private readonly IFileRepository _repo;
        private readonly IAwsS3Service _s3;
        private readonly ITdsFileService _fileService;

        public AwsFilesController(
            IFileRepository repo,
            IAwsS3Service s3,
            ITdsFileService fileService,
            IHttpContextAccessor http
        ) : base(http)
        {
            _repo = repo;
            _s3 = s3;
            _fileService = fileService;
        }

        [HttpPost("get")]
        public async Task<IActionResult> Get(FilesGetRequestDto dto)
        {
            var files = await _fileService.GetUploadFilesAsync(dto);

            var result = files.Select(f => new
            {
                f.id,
                f.file_name,
                f.file_url,
                f.file_type,
                f.file_size,
                f.created_at,
                f.work_stage,
                readUrl = _s3.GetPreSignedReadUrl(f.file_name)
            });

            return Ok(result);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] FileUploadRequestDto dto)
        {
            var uploadUrl = _s3.GetPreSignedUploadUrl(dto.filename, dto.content_type);
            var fileUrl = _s3.GetObjectUrl(dto.filename);
            var readUrl = _s3.GetPreSignedReadUrl(dto.filename);

            var (file, link) = await _repo.InsertFileWithLinkAsync(
                dto.filename,
                fileUrl,
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
                uploadUrl,
                fileUrl,
                readUrl,
                dbRecord = file,
                linkRecord = link
            });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] FileDeleteRequestDto dto)
        {
            await _s3.DeleteObjectAsync(dto.file_name);
            await _repo.DeleteFileAsync(dto.id);
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

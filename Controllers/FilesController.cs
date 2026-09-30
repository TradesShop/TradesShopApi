using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Files;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : BaseController
{
	private readonly IFileRepository _repo;

	private readonly IAwsS3Service _s3;

	private readonly ITdsFileService _fileService;

	public FilesController(IFileRepository repo, IAwsS3Service s3, ITdsFileService fileService)
	{
		_repo = repo;
		_s3 = s3;
		_fileService = fileService;
	}

	[HttpPost("get")]
	public async Task<IActionResult> Get(FilesGetRequestDto dto)
	{
		var result = (await _fileService.GetUploadFilesAsync(dto)).Select((UploadFilesDto f) => new
		{
			id = f.id,
			file_name = f.file_name,
			file_url = f.file_url,
			file_type = f.file_type,
			file_size = f.file_size,
			created_at = f.created_at,
			work_stage = f.work_stage,
			readUrl = _s3.GetPreSignedReadUrl(f.file_name)
		});
		return ApiOk(result);
	}

	[HttpPost("upload")]
	public async Task<IActionResult> Upload([FromBody] FileUploadRequestDto fu_req)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		fu_req.user_id = user_id;
		fu_req.upload_url = _s3.GetPreSignedUploadUrl(fu_req.filename, fu_req.content_type);
		fu_req.file_url = _s3.GetObjectUrl(fu_req.filename);
		fu_req.read_url = _s3.GetPreSignedReadUrl(fu_req.filename);
		var (file, link) = await _repo.InsertFileWithLinkAsync(fu_req);
		return ApiOk(new
		{
			uploadUrl = fu_req.upload_url,
			fileUrl = fu_req.file_url,
			readUrl = fu_req.read_url,
			dbRecord = file,
			linkRecord = link
		});
	}

	[HttpPost("delete")]
	public async Task<IActionResult> Delete([FromBody] FileDeleteRequestDto dto)
	{
		await _s3.DeleteFileAsync(dto);
		await _repo.DeleteFileAsync(dto.id);
		return ApiOk();
	}

	[HttpPost("description")]
	public async Task<IActionResult> UpdateDescription([FromBody] UpdateDescriptionDto dto)
	{
		await _repo.UpdateDescriptionAsync(dto.file_id, dto.description);
		return ApiOk();
	}
}

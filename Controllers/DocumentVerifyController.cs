using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Documents;
using TradePlatform.Api.Models.document;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Services.Google_OCR;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentVerifyController : BaseController
{
	private readonly GoogleVisionService _vision;

	private readonly DocumentTypeService _docType;

	private readonly UnifiedDocumentParserService _parser;

	private readonly VerificationRepository _repo;

	public DocumentVerifyController(GoogleVisionService vision, DocumentTypeService docType, UnifiedDocumentParserService parser, VerificationRepository repo)
	{
		_vision = vision;
		_docType = docType;
		_parser = parser;
		_repo = repo;
	}

	[HttpPost("verify-blob")]
	public async Task<IActionResult> VerifyBlob(IFormFile file, [FromForm] bool forceupdate = false)
	{
		if (file == null || file.Length == 0L)
		{
			return BadRequest("File is required.");
		}
		if (file.Length > 10485760)
		{
			return BadRequest("File size exceeds 10MB limit.");
		}
		string text;
		using (Stream stream = file.OpenReadStream())
		{
			text = await _vision.ExtractTextFromStreamAsync(stream);
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return BadRequest("Unable to extract text from the provided document.");
		}
		text = OcrNormalizer.Normalize(text);
		string type = _docType.Detect(text);
		VerifiedDocument parsed = _parser.Parse(type, text);
		bool autoVerified = parsed.is_valid;
		bool finalIsValid = parsed.is_valid;
		if (!parsed.is_valid & forceupdate)
		{
			finalIsValid = true;
			autoVerified = false;
		}
		VerifiedDocument documentToSave = new VerifiedDocument
		{
			user_id = GetUserId(),
			document_type = type,
			document_number = parsed.document_number,
			surname = parsed.surname,
			given_names = parsed.given_names,
			nationality = parsed.nationality,
			date_of_birth = parsed.date_of_birth,
			expiry_date = parsed.expiry_date,
			issue_date = parsed.issue_date,
			address = parsed.address,
			visa_type = parsed.visa_type,
			is_valid = finalIsValid,
			raw_text = text,
			auto_verified = autoVerified
		};
		if (!finalIsValid)
		{
			return ApiError(documentToSave);
		}
		return ApiOk(await _repo.InsertVerifiedDocument(documentToSave));
	}

	[HttpPost("verifyupsert")]
	public async Task<IActionResult> verifyupsert([FromBody] VerifiedDocument dvrDto)
	{
		return ApiOk(await _repo.InsertVerifiedDocument(dvrDto));
	}

	[HttpPost("verify")]
	public async Task<IActionResult> Verify([FromBody] DocumentVerifyRequestDto body)
	{
		string readUrl = body.readUrl;
		if (string.IsNullOrEmpty(readUrl))
		{
			return BadRequest("readUrl is required");
		}
		string text = await _vision.ExtractTextAsync(readUrl);
		string type = _docType.Detect(text);
		VerifiedDocument parsed = _parser.Parse(type, text);
		VerifiedDocument doc = new VerifiedDocument
		{
			user_id = GetUserId(),
			document_type = type,
			document_number = parsed.document_number,
			surname = parsed.surname,
			given_names = parsed.given_names,
			nationality = parsed.nationality,
			date_of_birth = parsed.date_of_birth,
			expiry_date = parsed.expiry_date,
			issue_date = parsed.issue_date,
			address = parsed.address,
			visa_type = parsed.visa_type,
			is_valid = parsed.is_valid,
			raw_text = text
		};
		await _repo.InsertVerifiedDocument(doc);
		if (!parsed.is_valid)
		{
			return ApiError(parsed);
		}
		return ApiOk(new
		{
			status = (parsed.is_valid ? "verified" : "failed"),
			documentType = type,
			fields = parsed,
			rawText = text
		});
	}
}

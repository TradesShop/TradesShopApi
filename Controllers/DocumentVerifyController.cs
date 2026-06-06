using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.DTOs.Documents;
using TradePlatform.Api.Models.document;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Services.Azure_OCR;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentVerifyController : BaseController
    {
        private readonly AzureVisionService _vision;
        private readonly DocumentTypeService _docType;
        private readonly UnifiedDocumentParserService _parser;
        private readonly VerificationRepository _repo;

        public DocumentVerifyController(
            AzureVisionService vision,
            DocumentTypeService docType,
            UnifiedDocumentParserService parser,
            VerificationRepository repo,
            IHttpContextAccessor http
        ) : base(http)
        {
            _vision = vision;
            _docType = docType;
            _parser = parser;
            _repo = repo;
        }
        [HttpPost("verify-blob")]        
        public async Task<IActionResult> VerifyBlob(IFormFile file, [FromForm] bool forceupdate = false)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            // 1. Convert file to stream
            using var stream = file.OpenReadStream();

            // 2. OCR using Azure Vision (stream overload)
            string text = await _vision.ExtractTextFromStreamAsync(stream);

            // 3. Detect document type
            string type = _docType.Detect(text);

            // 4. Parse fields
            var parsed = _parser.Parse(type, text);

            // ⭐ If not valid AND not forced → return OCR result only
           

            // ⭐ If valid OR forced → save to DB
            var anydoc = new VerifiedDocument
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
            //if (forceupdate)
            //{
            //    anydoc.is_valid = true;
            //}

            if (!parsed.is_valid )
            {
                return ApiError(anydoc);
            }

            var upddoc=await _repo.InsertVerifiedDocument(anydoc);

            return ApiOk(anydoc);
        }
        [HttpPost("verifyupsert")]
        public async Task<IActionResult> verifyupsert([FromBody] VerifiedDocument dvrDto)
        {
           var anydoc= await _repo.InsertVerifiedDocument(dvrDto);
            return ApiOk(anydoc);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] DocumentVerifyRequestDto body)
        {
            string readUrl = body.readUrl;
            //int userId = body.userId;

            if (string.IsNullOrEmpty(readUrl))
                return BadRequest("readUrl is required");

            // 1. OCR
            string text = await _vision.ExtractTextAsync(readUrl);

            // 2. Detect document type
            string type = _docType.Detect(text);

            // 3. Unified parsing (passport, driving licence, BRP)
            var parsed = _parser.Parse(type, text);

            // 4. Save to DB using Dapper + Stored Procedure
            var doc = new VerifiedDocument
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

            // 5. Return result to frontend
            if (!parsed.is_valid)
            {
                return ApiError(parsed);
            }

            return ApiOk(new
            {
                status = parsed.is_valid ? "verified" : "failed",
                documentType = type,
                fields = parsed,
                rawText = text
            });
        }
    }
}

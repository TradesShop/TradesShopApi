using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Services.Questions;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class questionsController : BaseController
    {
        private readonly QuestionRepository _repoQue;
        private readonly IQuestionsService _queService;

        public questionsController(
            QuestionRepository repoQue
            , IQuestionsService queService
            ,IHttpContextAccessor http
        ) : base(http)
        {
            _repoQue = repoQue;
            _queService = queService;
        }
        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var anyresult = await _repoQue.GetQuestionsByCategory(categoryId);
            return ApiOk(anyresult);
        }
        [HttpPost("nextquestion")]
        public async Task<object> GetNextStep([FromBody] RequestForNextQue nQue)
        {

            var anyresult= await _queService.GetNextStep(nQue);
            return ApiOk(anyresult);

        }
        [HttpGet("postjob/{job_id}")]
        public async Task<IActionResult> GetQuestionsForPostJob(Guid job_id)
        {
            var questions = await _queService.GetQuestionsForPostJob(job_id);

            if (questions == null || questions.Count == 0)
            {
                return Ok(new { data = (object?)null });
            }

            return ApiOk(questions);
        }
        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertAnswerAsync([FromBody] AnswerUpsertDto auDto)
        {
            await _queService.UpsertAnswerAsync(auDto);

            return ApiOk(new { success = true });
        }


        // GET /api/categories
        //[HttpGet]
        // public async Task<IActionResult> Get()
        // {
        //     var subcategories = await _repo.GetQuestion();
        //     return Ok(subcategories);
        // }

        // GET /api/categories/5
        //[HttpGet("{id:int}")]
        //public async Task<IActionResult> Get(int id)
        //{
        //    var subcategories = await _repoQue.GetQuestion(id);
        //    return Ok(subcategories);
        //}



    }
}

using TradePlatform.Api.DTOs.Questions;

namespace TradePlatform.Api.Services.Questions
{
    public interface IQuestionsService
    {
        Task<object> GetNextStep(RequestForNextQue nQue);
        Task<List<QuestionDto>> GetQuestionsForPostJob(Guid job_id);
        Task UpsertAnswerAsync(AnswerUpsertDto auDto);
    }
}

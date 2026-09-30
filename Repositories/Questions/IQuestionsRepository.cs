using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Questions;

namespace TradePlatform.Api.Repositories.Questions;

public interface IQuestionsRepository
{
	Task<IEnumerable<QuestionsDto>> GetAllAsync();

	Task<QuestionsDto> GetByIdAsync(int id);

	Task<QuestionsDto> CreateAsync(QuestionCreateDto model);

	Task<QuestionsDto> UpdateAsync(QuestionUpdateDto model);

	Task<IEnumerable<QuestionFlowDto>> GetFlowsAsync(int question_id);

	Task<QuestionFlowDto> CreateFlowAsync(QuestionFlowCreateDto model);

	Task<QuestionFlowDto> UpdateFlowAsync(QuestionFlowUpdateDto model);

	Task DeleteFlowAsync(int id);
}

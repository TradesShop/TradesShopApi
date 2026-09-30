using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models.Answers;

namespace TradePlatform.Api.Repositories.Answers;

public interface IAnswerRepository
{
	Task<IEnumerable<AnswersDto>> GetByGroupAsync(int answer_group_id);

	Task<AnswersDto> CreateAsync(AnswerCreateModel model);

	Task<AnswersDto> UpdateAsync(AnswerUpdateModel model);

	Task DeleteAnswerAsync(int id);
}

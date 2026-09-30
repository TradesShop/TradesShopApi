using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models.AnswerGroups;

namespace TradePlatform.Api.Repositories.AnswerGroups;

public interface IAnswerGroupRepository
{
	Task<IEnumerable<AnswerGroupDto>> GetAllAsync();

	Task<AnswerGroupDto> CreateAsync(AnswerGroupCreateModel model);

	Task<AnswerGroupDto> UpdateAsync(AnswerGroupUpdateModel model);
}

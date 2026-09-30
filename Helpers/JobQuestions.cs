using System.Collections.Generic;
using System.Linq;
using TradePlatform.Api.DTOs.Jobs;

namespace TradePlatform.Api.Helpers;

public class JobQuestions
{
	public static List<JobQuestionDto> MapQuestions(IEnumerable<QuestionAnswerRow?> qaRows)
	{
		return (from r in qaRows
			group r by new { r.question_id, r.question_title, r.que_group_id } into g
			select new JobQuestionDto
			{
				question_id = g.Key.question_id,
				question_title = g.Key.question_title,
				que_group_id = g.Key.que_group_id,
				answers = g.Select((QuestionAnswerRow a) => new AnswerDto
				{
					answer_id = a.answer_id,
					answer_title = a.answer_title
				}).ToList(),
				answers_csv = string.Join(", ", g.Select((QuestionAnswerRow a) => a.answer_title))
			}).ToList();
	}
}

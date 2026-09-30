using System;

namespace TradePlatform.Api.DTOs.Questions;

public class QuestionCreateDto
{
	public string title { get; set; }

	public string description { get; set; }

	public string answertype { get; set; }

	public bool isactive { get; set; } = true;

	public int? group_id { get; set; }

	public int? answer_group_id { get; set; }

	public int? category_id { get; set; }

	public Guid updated_by { get; set; }

	public bool is_first_question { get; set; }
}

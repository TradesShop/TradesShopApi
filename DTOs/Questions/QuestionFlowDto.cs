namespace TradePlatform.Api.DTOs.Questions;

public class QuestionFlowDto
{
	public int id { get; set; }

	public int answer_id { get; set; }

	public string answ { get; set; }

	public int? next_question_id { get; set; }

	public string next_question { get; set; }

	public string condition_json { get; set; }

	public int question_id { get; set; }

	public string question { get; set; }
}

namespace TradePlatform.Api.DTOs.Questions;

public class QuestionsDto
{
	public int id { get; set; }

	public string title { get; set; }

	public string description { get; set; }

	public string answertype { get; set; }

	public bool isactive { get; set; }

	public int? group_id { get; set; }

	public string group_name { get; set; }

	public int? answer_group_id { get; set; }

	public string answer_group_name { get; set; }

	public int category_id { get; set; }

	public string category { get; set; }

	public bool is_first_question { get; set; }
}

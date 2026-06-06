using TradePlatform.Api.DTOs.Questions;

public class QuestionDto
{
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string type { get; set; }
    public int sortorder { get; set; }
    public List<AnswerQeDto> answers { get; set; } = new();

}
public class RequestForNextQue
{
    public int question_id { get; set; }
    public List<int> answer_ids { get; set; }
}
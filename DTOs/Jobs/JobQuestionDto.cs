namespace TradePlatform.Api.DTOs.Jobs
{
    public class JobQuestionDto
    {
        public int question_id { get; set; }
        public string question_title { get; set; }
        public int que_group_id { get; set; }
        public List<AnswerDto> answers { get; set; }
        public string answers_csv { get; set; }
    }
}

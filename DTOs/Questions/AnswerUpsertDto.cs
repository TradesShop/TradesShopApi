namespace TradePlatform.Api.DTOs.Questions
{
    public class AnswerUpsertDto
    {
        public Guid job_id { get; set; }
        public int question_id { get; set; }
        public int answer_id { get; set; }
    }
}

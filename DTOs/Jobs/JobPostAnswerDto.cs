namespace TradePlatform.Api.DTOs.Jobs
{
    public class JobPostAnswerDto
    {
        public int question_id { get; set; }

        public int? answer_id { get; set; }

        public List<int> answer_ids { get; set; }
    }
}

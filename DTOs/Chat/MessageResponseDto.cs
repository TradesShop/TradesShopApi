namespace TradePlatform.Api.DTOs.Chat
{
    public class MessageResponseDto
    {
        public Guid? message_id { get; set; }
        public Guid? conversation_id { get; set; }
        public string? message_text { get; set; }
        public string? message_type { get; set; }
        public DateTime? created_at { get; set; }
        public string? sender_name { get; set; }
        public string email { get; set; }
        public string? title { get; set; }
        public string? message { get; set; }

    }
}

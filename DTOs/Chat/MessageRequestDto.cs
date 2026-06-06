namespace TradePlatform.Api.DTOs.Chat
{
    public class MessageRequestDto
    {
        public int entity_type_id { get; set; }
        public Guid  entity_id { get; set; }
        //public Guid user_id { get; set; }
        public string message_text { get; set; }
        public Guid receiver_user_id { get; set; }
        public Guid sender_user_id { get; set; }
    }
}

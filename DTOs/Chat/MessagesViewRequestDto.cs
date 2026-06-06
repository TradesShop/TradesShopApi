namespace TradePlatform.Api.DTOs.Chat
{
    public class MessagesViewRequestDto
    {
        public Guid user_id { get; set; }
        public int entity_type_id { get; set; }
        public Guid  entity_id { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        
    }
}

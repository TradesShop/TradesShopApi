using TradePlatform.Api.DTOs.Chat;

namespace TradePlatform.Api.Services.Chat
{
    public interface IChatService
    {
        Task<MessageResponseDto> chat_message_send(MessageRequestDto msg_req_dto);
        Task<IEnumerable<MessageResponseDto>> chat_messages_get_async(MessagesViewRequestDto msg_req_dto);
    }
}

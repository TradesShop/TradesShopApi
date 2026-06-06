using TradePlatform.Api.DTOs.Chat;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<MessageResponseDto?> chat_message_send(MessageRequestDto msg_req_dto);
        Task<IEnumerable<MessageResponseDto>> chat_messages_get_async(MessagesViewRequestDto msgs_req_dto);

    }
}

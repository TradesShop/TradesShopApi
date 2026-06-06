using System.Data;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models.Jobs;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;

        public ChatService(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        public async Task<MessageResponseDto> chat_message_send(MessageRequestDto msg_req_dto)
        {
            return await _chatRepo.chat_message_send(msg_req_dto);
        }

        public async Task<IEnumerable<MessageResponseDto>> chat_messages_get_async(MessagesViewRequestDto msgs_req_dto)
        {
            return await _chatRepo.chat_messages_get_async(msgs_req_dto);
        }

    }
}

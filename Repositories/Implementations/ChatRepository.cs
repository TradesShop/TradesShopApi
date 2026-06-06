using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class ChatRepository: IChatRepository
    {
        private readonly DapperContext _context;
        public ChatRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<MessageResponseDto?> chat_message_send(MessageRequestDto msg_req_dto)
        {
            using var connection = _context.CreateOpenConnection();

            return await connection.QueryFirstOrDefaultAsync<MessageResponseDto>(
                "dbo.usp_chat_message_send",
                new {
                    entity_type_id = msg_req_dto.entity_type_id,
                    entity_id= msg_req_dto.entity_id,
                    sender_user_id= msg_req_dto.sender_user_id,
                    receiver_user_id = msg_req_dto.receiver_user_id,
                    message_text=msg_req_dto.message_text
                },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<MessageResponseDto>> chat_messages_get_async(MessagesViewRequestDto msgs_req_dto)
        {
            using var connection = _context.CreateOpenConnection();
            return await connection.QueryAsync<MessageResponseDto>(
               "dbo.usp_chat_messages_get_async",
               new
               {
                   user_id= msgs_req_dto.user_id,
                   entity_type_id = msgs_req_dto.entity_type_id,
                   entity_id = msgs_req_dto.entity_id,
                   page = msgs_req_dto.page,
                   pagesize = msgs_req_dto.pagesize
                   
               },
               commandType: CommandType.StoredProcedure
           );
            
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.BackgroundEmailQueue;

namespace TradePlatform.Api.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
	private readonly DapperContext _context;

	private readonly IBackgroundEmailQueue _backgroundEmailQueue;

	public ChatRepository(DapperContext context, IBackgroundEmailQueue backgroundEmailQueue)
	{
		_context = context;
		_backgroundEmailQueue = backgroundEmailQueue;
	}

	public async Task<MessageResponseDto?> chat_message_send(MessageRequestDto msg_req_dto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { msg_req_dto.entity_type_id, msg_req_dto.entity_id, msg_req_dto.sender_user_id, msg_req_dto.recipient_user_id, msg_req_dto.sender_user_type, msg_req_dto.recipient_user_type, msg_req_dto.message_text, msg_req_dto.has_attachments };
		CommandType? commandType = CommandType.StoredProcedure;
		MessageResponseDto MessageResponse = await connection.QueryFirstOrDefaultAsync<MessageResponseDto>("dbo.usp_chat_message_send", param, null, null, commandType);
		MessageEmailNotifyReq msg_email_req = new MessageEmailNotifyReq
		{
			sender_user_id = msg_req_dto.sender_user_id,
			recipient_user_id = msg_req_dto.recipient_user_id,
			message_id = MessageResponse.message_id,
			conversation_id = MessageResponse.conversation_id,
			entity_type_id = msg_req_dto.entity_type_id,
			recipient_user_type = msg_req_dto.recipient_user_type,
			sender_user_type = msg_req_dto.sender_user_type
		};
		await _backgroundEmailQueue.QueueJobMessageEmailAsync(msg_email_req);
		return MessageResponse;
	}

	public async Task<IEnumerable<MessageResponseDto>> chat_messages_get_async(MessagesViewRequestDto msgs_req_dto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { msgs_req_dto.user_id, msgs_req_dto.entity_type_id, msgs_req_dto.entity_id, msgs_req_dto.page, msgs_req_dto.pagesize };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryAsync<MessageResponseDto>("dbo.usp_chat_messages_get_async", param, null, null, commandType);
	}

	public async Task<NotifyEmailDataModel?> message_notification_email_details(MessageEmailNotifyReq msg_email_req)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { msg_email_req.sender_user_id, msg_email_req.recipient_user_id, msg_email_req.sender_user_type, msg_email_req.recipient_user_type, msg_email_req.entity_type_id, msg_email_req.message_id, msg_email_req.conversation_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<NotifyEmailDataModel>("[dbo].[usp_chat_message_notification_email_get]", param, null, null, commandType);
	}
}

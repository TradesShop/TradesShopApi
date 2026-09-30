using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IChatRepository
{
	Task<MessageResponseDto?> chat_message_send(MessageRequestDto msg_req_dto);

	Task<IEnumerable<MessageResponseDto>> chat_messages_get_async(MessagesViewRequestDto msgs_req_dto);

	Task<NotifyEmailDataModel?> message_notification_email_details(MessageEmailNotifyReq msg_email_req);
}

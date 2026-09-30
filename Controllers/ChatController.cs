using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Chat;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : BaseController
{
	private readonly IChatService _chatService;

	public ChatController(IChatService chatService)
	{
		_chatService = chatService;
	}

	[HttpPost("send")]
	public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto msg_req_dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		msg_req_dto.sender_user_id = user_id;
		if (msg_req_dto.sender_user_id == Guid.Empty)
		{
			return ApiError(new
			{
				message = "Invalid user"
			});
		}
		MessageResponseDto result = await _chatService.chat_message_send(msg_req_dto);
		if (!result.message_id.HasValue || result.message_id == Guid.Empty)
		{
			return ApiError(result);
		}
		return ApiOk(result);
	}

	[HttpPost("messages")]
	public async Task<IActionResult> chat_messages_get([FromBody] MessagesViewRequestDto msgs_req_dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		msgs_req_dto.user_id = user_id;
		if (msgs_req_dto.user_id == Guid.Empty)
		{
			return ApiError(new
			{
				message = "Invalid user"
			});
		}
		return ApiOk(await _chatService.chat_messages_get_async(msgs_req_dto));
	}
}

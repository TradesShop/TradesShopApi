using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Chat;
using TradePlatform.Api.Services.users;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService
            , IHttpContextAccessor http
        ) : base(http)
        {
            _chatService = chatService;

        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequestDto msg_req_dto)
        {
            var (callerId, callerType) = GetIdentity();
            msg_req_dto.sender_user_id= callerId; // from JWT (IMPORTANT)
            
            if (msg_req_dto.sender_user_id == Guid.Empty)
                return ApiError(new { message = "Invalid user" });
           
            var result = await _chatService.chat_message_send(msg_req_dto);
            if(!result.message_id.HasValue ||  result.message_id== Guid.Empty)
                return ApiError(result);

            return ApiOk(result);
        }
        [HttpPost("messages")]
        public async Task<IActionResult> chat_messages_get([FromBody] MessagesViewRequestDto msgs_req_dto)
        {
            var (callerId, callerType) = GetIdentity();
            msgs_req_dto.user_id = callerId; // from JWT (IMPORTANT)

            if (msgs_req_dto.user_id == Guid.Empty)
                return ApiError(new { message = "Invalid user" });

            var result = await _chatService.chat_messages_get_async(msgs_req_dto);
            //if (!result.message_id.HasValue || result.message_id == Guid.Empty)
                //return ApiError(result);

            return ApiOk(result);
        }

        
    }
}

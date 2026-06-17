using CommunicationService.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.WebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class ChatUsersController : ControllerBase
    {
        IChatUserService _chatUserService;
        public ChatUsersController(IChatUserService chatUserService) 
        {
            _chatUserService = chatUserService;
        }

        [HttpGet("chat/{chatId}/chatUsers")]
        public async Task<IActionResult> GetChatUsersAsync(Guid chatId, CancellationToken cancellationToken)
        {
            var chatUsers = await _chatUserService.GetChatUsersAsync(chatId, cancellationToken);
            return Ok(chatUsers);
        }
    }
}

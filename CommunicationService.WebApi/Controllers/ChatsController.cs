using CommunicationService.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.WebApi.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        IChatService _chatService;
        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetChatsAsync(CancellationToken cancellationToken)
        {
            var chats = await _chatService.GetChatsAsync(cancellationToken);
            return Ok(chats);
        }
    }
}

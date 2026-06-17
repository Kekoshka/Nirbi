using CommunicationService.WebApi.Common.DTO;
using CommunicationService.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NpgsqlTypes;

namespace CommunicationService.WebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        IMessageService _messageService;
        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("messages/group")]
        public async Task<IActionResult> CreateMessageGroupChatAsync(CreateMessageGroupChatRequest request, CancellationToken cancellationToken)
        {
            var messageId = await _messageService.CreateMessageGroupChatAsync(request, cancellationToken);
            return Ok(messageId);
        }
        [HttpPost("messages/private")]
        public async Task<IActionResult> CreateMessagePrivateChatAsync(CreateMessagePrivateChatRequest request, CancellationToken cancellationToken)
        {
            var messageId =  await _messageService.CreateMessagePrivateChatAsync(request, cancellationToken);
            return Ok(messageId);
        }
        [HttpPut("messages")]
        public async Task<IActionResult> UpdateMessageAsync(UpdateMessageRequest request, CancellationToken cancellationToken)
        {
            await _messageService.UpdateMessageAsync(request, cancellationToken);
            return NoContent();
        }
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessageAsync(Guid messageId,CancellationToken cancellationToken)
        {
            await _messageService.DeleteMessageAsync(messageId, cancellationToken);
            return NoContent();
        }
        [HttpGet("chats/{chatId}/messages")]
        public async Task<IActionResult> GetMessagesByChatIdAsync(Guid chatId, CancellationToken cancellationToken)
        {
            var messages = await _messageService.GetMessagesByChatIdAsync(chatId, cancellationToken);
            return Ok(messages);
        }
        [HttpGet("messages/preview")]
        public async Task<IActionResult> GetPreviewMessagesAsync(List<Guid> chatIds, CancellationToken cancellationToken)
        {
            var previewMessages = await _messageService.GetPreviewMessagesAsync(chatIds, cancellationToken);
            return Ok(previewMessages);
        }
    }
}

using CommunicationService.WebApi.Common.DTO;
using CommunicationService.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NpgsqlTypes;

namespace CommunicationService.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        IMessageService _messageService;
        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<IActionResult> CreateMessageGroupChatAsync(CreateMessageGroupChatRequest request, CancellationToken cancellationToken)
        {
            var messageId = await _messageService.CreateMessageGroupChatAsync(request, cancellationToken);
            return Ok(messageId);
        }

        public async Task<IActionResult> CreateMessagePrivateChatAsync(CreateMessagePrivateChatRequest request, CancellationToken cancellationToken)
        {
            var messageId =  await _messageService.CreateMessagePrivateChatAsync(request, cancellationToken);
            return Ok(messageId);
        }

        public async Task<IActionResult> UpdateMessageAsync(UpdateMessageRequest request, CancellationToken cancellationToken)
        {
            await _messageService.UpdateMessageAsync(request, cancellationToken);
            return NoContent();
        }

        public async Task<IActionResult> DeleteMessageAsync(Guid messageId,CancellationToken cancellationToken)
        {
            await _messageService.DeleteMessageAsync(messageId, cancellationToken);
            return NoContent();
        }

        public async Task<IActionResult> GetMessagesByChatIdAsync(Guid chatId, CancellationToken cancellationToken)
        {
            var messages = await _messageService.GetMessagesByChatIdAsync(chatId, cancellationToken);
            return Ok(messages);
        }
        public async Task<IActionResult> GetPreviewMessagesAsync(List<Guid> chatIds, CancellationToken cancellationToken)
        {
            var previewMessages = await _messageService.GetPreviewMessagesAsync(chatIds, cancellationToken);
            return Ok(previewMessages);
        }
    }
}

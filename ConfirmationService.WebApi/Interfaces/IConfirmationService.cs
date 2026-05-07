
using ConfirmationService.DataAccess.Enums;
using ConfirmationService.DataAccess.Models;
using ConfirmationService.WebApi.Common.DTO;
using ConfirmationService.WebApi.Common.DTO.ServiceDTO;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ConfirmationService.WebApi.Services
{
    public interface IConfirmationService
    {
        Task<Guid> CreateConfirmationAsync(CreateConfirmationRequest request); 
        
        Task<Confirmation> GetConfirmationAsync(Guid confirmationId);
        
        Task<ICollection<Confirmation>> GetConfirmationsByReviewerAsync();
        
        Task<IEnumerable<Confirmation>> GetConfirmationsByInitiatorAsync();
        
        Task RespondToConfirmationAsync(RespondToConfirmationDTO dto);
        
        Task RevokeConfirmationAsync(Guid confirmationId);
    }
}
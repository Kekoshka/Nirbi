using ConfirmationService.DataAccess.Context;
using ConfirmationService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using ConfirmationService.WebApi.Common.DTO;
using ExceptionHandler.Exceptions;
using ConfirmationService.WebApi.Common.Mappers;
using Nirbi.ServiceAuth.Identity;
using ConfirmationService.WebApi.Interfaces;
using ConfirmationService.WebApi.Common.DTO.ServiceDTO;
using ConfirmationService.DataAccess.Enums;

namespace ConfirmationService.WebApi.Services;

public class ConfirmationService : IConfirmationService
{
    ConfirmationDbContext _context;
    ICurrentUserService _currentUserService;

    public ConfirmationService(
        ConfirmationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> CreateConfirmationAsync(CreateConfirmationRequest request)
    {
        var existingConfirmation = await _context.Confirmations
            .FirstOrDefaultAsync(c =>
                c.ConfirmationType == request.ConfirmationType &&
                c.EntityId == request.EntityId &&
                c.InitiatorId == _currentUserService.GetUserId() &&
                c.ReviewerId == request.ReviewerId &&
                c.Status != ConfirmationStatus.Expired.ToString() &&
                c.Status != ConfirmationStatus.Revoked.ToString());

        if (existingConfirmation is not null)
            throw new ConflictException($"Active confirmation already exists for this combination");

        Confirmation confirmation = new(
            request.ConfirmationType,
            request.EntityId,
            _currentUserService.GetUserId(),
            request.ReviewerId,
            request.MetaData != null
                ? JsonSerializer.Serialize(request.MetaData)
                : string.Empty,
            DateTime.UtcNow.AddHours(request.ExpirationHours));
        _context.Confirmations.Add(confirmation);
        await _context.SaveChangesAsync();

        return confirmation.Id;
    }

    public async Task<Confirmation> GetConfirmationAsync(Guid confirmationId)
    {
        var confirmation = await _context.Confirmations
            .Include(c => c.Audits)
            .FirstOrDefaultAsync(c => c.Id == confirmationId);

        if (confirmation == null)
            throw new NotFoundException($"Confirmation with id {confirmationId} not found");

        var userId = _currentUserService.GetUserId();
        if (userId != confirmation.ReviewerId &&
            userId != confirmation.InitiatorId)
            throw new ForbiddenException();

        return confirmation;
    }

    public async Task<ICollection<ConfirmationDTO>> GetConfirmationsByReviewerAsync()
    {
        var confirmations = await _context.Confirmations
            .Where(c => c.ReviewerId == _currentUserService.GetUserId())
            .Include(c => c.Audits)
            .ToListAsync();

        return confirmations.ToConfirmationsDTO();
    }

    public async Task<ICollection<ConfirmationDTO>> GetConfirmationsByInitiatorAsync()
    {
        var confirmations = await _context.Confirmations
            .Where(c => c.InitiatorId == _currentUserService.GetUserId())
            .Include(c => c.Audits)
            .ToListAsync();

        return confirmations.ToConfirmationsDTO();
    }
    public async Task<ConfirmationDTO> GetConfirmationsByEntityId(Guid entityId)
    {
        var confirmation = await _context.Confirmations
            .FirstOrDefaultAsync(c => c.EntityId == entityId);
        if (confirmation is null)
            throw new NotFoundException();

        return confirmation.ToConfirmationDTO();
    }

    public async Task RespondToConfirmationAsync(RespondToConfirmationDTO dto)
    {
        var confirmation = await _context.Confirmations
            .Include(c => c.Audits)
            .FirstOrDefaultAsync(c => c.Id == dto.ConfirmationId);

        if (confirmation is null)
            throw new NotFoundException($"Confirmation with id {dto.ConfirmationId} not found");

        if (confirmation.ReviewerId != _currentUserService.GetUserId())
            throw new ForbiddenException("Only the reviewer can respond to this confirmation");

        if (confirmation.Status != ConfirmationStatus.Created.ToString())
            throw new BadRequestException($"Cannot respond to confirmation with status: {confirmation.Status}");

        if (DateTime.UtcNow > confirmation.ExpiresAt)
            throw new BadRequestException("Confirmation has expired");


        var oldStatus = confirmation.Status;
        confirmation.Respond(dto.IsAccepted,
            _currentUserService.GetUserId(),
            dto.RejectionReason);
        var audit = new ConfirmationAudit(
            confirmationId: confirmation.Id,
            changedBy: _currentUserService.GetUserId(),
            newStatus: confirmation.Status,
            oldStatus: oldStatus);

        await _context.ConfirmationAudits.AddAsync(audit);

        await _context.SaveChangesAsync();
    }

    public async Task RevokeConfirmationAsync(
        Guid confirmationId)
    {
        var confirmation = await _context.Confirmations
            .Include(c => c.Audits)
            .FirstOrDefaultAsync(c => c.Id == confirmationId);

        if (confirmation == null)
            throw new NotFoundException($"Confirmation with id {confirmationId} not found");

        if (confirmation.InitiatorId != _currentUserService.GetUserId())
            throw new ForbiddenException("Only the initiator can revoke this confirmation");

        if (confirmation.Status != ConfirmationStatus.Created.ToString())
            throw new BadRequestException($"Cannot revoke confirmation with status: {confirmation.Status}");

        //todo лучше перенести это в паттерн Repository, а то добавление аудитов может где-то забыться
        ConfirmationAudit confirmationAudit = new(
            confirmation.Id,
            confirmation.InitiatorId,
            ConfirmationStatus.Revoked.ToString(),
            confirmation.Status);
        _context.ConfirmationAudits.Add(confirmationAudit);
        confirmation.Revoke(_currentUserService.GetUserId());

        await _context.SaveChangesAsync();
    }
}
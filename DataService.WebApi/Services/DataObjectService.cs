using DataService.DataAccess.Postgres.Context;
using DataService.DataAccess.Postgres.Models;
using DataService.WebApi.Common.DTO;
using DataService.WebApi.Interfaces;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using MinorTaskService.WebApi.Interfaces;
using Nirbi.ServiceAuth.Identity;

namespace DataService.WebApi.Services;

public sealed class DataObjectService : IDataObjectService
{
    private readonly DataDbContext _db;
    private readonly IObjectStorageService _storage;
    ICallerContext _caller;
    ICurrentUserService _currentUserService;
    public DataObjectService(
        DataDbContext db, 
        IObjectStorageService storage,
        ICallerContext caller,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _storage = storage;
        _caller = caller;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> CreateCollectionAsync(
        CancellationToken cancellationToken = default)
    {
        var entity = new FileCollection
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            Owners = new List<Owner> { new() { UserId = _currentUserService.GetUserId()} },
        };
        _db.FileCollections.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }

    public async Task DeleteCollectionAsync(
        Guid collectionId,
        CancellationToken cancellationToken = default)
    {
        var collection = await _db.FileCollections
            .Include(c => c.Owners)
            .Include(c => c.Files)
            .FirstOrDefaultAsync(c => c.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
            return;

        if (!collection.Owners.Any(o => o.UserId == _currentUserService.GetUserId()))
            throw new ForbiddenException();

        foreach (var file in collection.Files)
            await _storage.DeleteObjectAsync(file.StorageKey, cancellationToken).ConfigureAwait(false);

        _db.FileCollections.Remove(collection);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Guid> UploadStandaloneAsync(
        Stream content,
        string contentType,
        string? originalFileName,
        long? knownSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var key = BuildStorageKey(id);
        await _storage.PutObjectAsync(key, content, contentType, cancellationToken).ConfigureAwait(false);

        var size = knownSizeBytes ?? (content.CanSeek ? content.Length : 0L);

        var entity = new StoredFile
        {
            Id = id,
            FileCollectionId = null,
            SortOrder = 0,
            StorageKey = key,
            ContentType = contentType,
            SizeBytes = size,
            OriginalFileName = originalFileName,
            CreatedAtUtc = DateTime.UtcNow,
            Owners = new List<Owner> { new() { UserId = _currentUserService.GetUserId() } },
        };

        _db.StoredFiles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return id;
    }

    public async Task<Guid> UploadToCollectionAsync(
        Guid collectionId,
        Stream content,
        string contentType,
        string? originalFileName,
        long? knownSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var collection = await _db.FileCollections
            .Include(c => c.Owners)
            .FirstOrDefaultAsync(c => c.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
            throw new InvalidOperationException($"Collection '{collectionId}' was not found.");

        if (!collection.Owners.Any(o => o.UserId == _currentUserService.GetUserId()))
            throw new ForbiddenException();

        var id = Guid.NewGuid();
        var key = BuildStorageKey(id);
        await _storage.PutObjectAsync(key, content, contentType, cancellationToken).ConfigureAwait(false);

        var size = knownSizeBytes ?? (content.CanSeek ? content.Length : 0L);

        var maxOrder = await _db.StoredFiles
            .Where(f => f.FileCollectionId == collectionId)
            .Select(f => (int?)f.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = new StoredFile
        {
            Id = id,
            FileCollectionId = collectionId,
            SortOrder = maxOrder + 1,
            StorageKey = key,
            ContentType = contentType,
            SizeBytes = size,
            OriginalFileName = originalFileName,
            CreatedAtUtc = DateTime.UtcNow,
            // файлы в коллекции не имеют своих владельцев Ч владелец наследуетс€ от коллекции
            Owners = new List<Owner>(),
        };

        _db.StoredFiles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return id;
    }

    public async Task<FileMetadataDto?> GetMetadataAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.StoredFiles.AsNoTracking()
            .Include(f => f.Owners)
            .Include(f => f.FileCollection)
                .ThenInclude(c => c!.Owners)
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return null;

        if (!IsOwner(entity, _currentUserService.GetUserId()))
            throw new ForbiddenException();

        return ToDto(entity);
    }

    public async Task<IReadOnlyList<FileMetadataDto>> ListByCollectionAsync(
        Guid collectionId,
        CancellationToken cancellationToken = default)
    {
        var collection = await _db.FileCollections.AsNoTracking()
            .Include(c => c.Owners)
            .FirstOrDefaultAsync(c => c.Id == collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
            return [];

        if (!collection.Owners.Any(o => o.UserId == _currentUserService.GetUserId()))
            throw new ForbiddenException();

        var list = await _db.StoredFiles.AsNoTracking()
            .Where(f => f.FileCollectionId == collectionId)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return list.Select(ToDto).ToList();
    }

    public async Task<FileDownloadResult?> OpenReadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.StoredFiles.AsNoTracking()
            .Include(f => f.Owners)
            .Include(f => f.FileCollection)
                .ThenInclude(c => c!.Owners)
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return null;

        if (!IsOwner(entity, _currentUserService.GetUserId()))
            throw new ForbiddenException();

        var buffer = await _storage.GetObjectStreamAsync(entity.StorageKey, cancellationToken).ConfigureAwait(false);
        return new FileDownloadResult(buffer, entity.ContentType, entity.OriginalFileName);
    }

    public async Task DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.StoredFiles
            .Include(f => f.Owners)
            .Include(f => f.FileCollection)
                .ThenInclude(c => c!.Owners)
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return;

        if (!IsOwner(entity, _currentUserService.GetUserId()))
            throw new ForbiddenException();

        await _storage.DeleteObjectAsync(entity.StorageKey, cancellationToken).ConfigureAwait(false);
        _db.StoredFiles.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// ƒл€ standalone-файла провер€ем его собственных Owners.
    /// ƒл€ файла в коллекции Ч провер€ем Owners родительской коллекции.
    /// </summary>
    private bool IsOwner(StoredFile file, Guid userId) =>
        file.FileCollectionId is null
            ? file.Owners.Any(o => o.UserId == userId) || _caller.IsService
            : file.FileCollection!.Owners.Any(o => o.UserId == userId) || _caller.IsService;

    private static string BuildStorageKey(Guid fileId) => $"v1/objects/{fileId:N}";

    private static FileMetadataDto ToDto(StoredFile f) =>
        new(f.Id, f.FileCollectionId, f.SortOrder, f.ContentType, f.SizeBytes, f.OriginalFileName, f.CreatedAtUtc);
}
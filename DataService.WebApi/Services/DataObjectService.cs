using DataService.DataAccess.Postgres.Context;
using DataService.DataAccess.Postgres.Models;
using DataService.WebApi.Common;
using DataService.WebApi.Common.DTO;
using DataService.WebApi.Interfaces;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using Nirbi.ServiceAuth.Identity;
using System.Linq.Expressions;
using static Amazon.S3.Util.S3EventNotification;

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
        bool isPublic = false,
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
            IsPublic = isPublic,
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
        bool isPublic = false,
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
            IsPublic = isPublic,
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

        return entity.ToDto();
    }

    public async Task<List<FileMetadataDto>> ListByCollectionAsync(
        Guid collectionId,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.GetUserId();
        var collection = await _db.FileCollections.AsNoTracking()
            .Include(c => c.Owners)
            .Include(c => c.Files)
            .ThenInclude(c => c.Owners)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == collectionId &&
                (
                c.Owners.Any(o => o.UserId == currentUserId) ||
                c.Files.Any(f => f.IsPublic) ||
                c.Files.Any(f => f.Owners.Any(o => o.UserId == currentUserId))
                )
                , cancellationToken)
            .ConfigureAwait(false);
        
        if (collection is null)
            throw new NotFoundException($"Forbidden or collection with id {collectionId} not found");


        if (collection.Owners.Any(o => o.UserId == currentUserId))
            return collection.Files.ToDto();    
        return collection.Files.Where(f => f.IsPublic || f.Owners.Any(o => o.UserId == currentUserId)).ToDto();
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

        if (!IsOwner(entity, _currentUserService.GetUserId()) &&
            !entity.IsPublic)
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

    public async Task<List<CollectionPreviewDto>> GetCollectionPreviewsAsync(
    IReadOnlyCollection<Guid> collectionIds,
    CancellationToken cancellationToken = default)
    {
        if (collectionIds is null || collectionIds.Count == 0)
            return [];

        var currentUserId = _currentUserService.GetUserId();

        // ”бираем дубликаты, чтобы не делать лишних запросов к хранилищу
        var ids = collectionIds.Distinct().ToList();

        // “€нем коллекции вместе с файлами и владельцами одним запросом.
        // ‘ильтр доступа повтор€ет ListByCollectionAsync: пускаем коллекцию,
        // если пользователь Ч владелец, либо в ней есть публичный/его файл.
        var collections = await _db.FileCollections.AsNoTracking()
            .Include(c => c.Owners)
            .Include(c => c.Files)
                .ThenInclude(f => f.Owners)
            .Where(c => ids.Contains(c.Id) &&
                (
                    c.Owners.Any(o => o.UserId == currentUserId) ||
                    c.Files.Any(f => f.IsPublic) ||
                    c.Files.Any(f => f.Owners.Any(o => o.UserId == currentUserId))
                ))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (collections.Count == 0)
            return [];

        // ƒл€ каждой коллекции выбираем первый доступный файл по SortOrder.
        var picks = new List<(Guid CollectionId, StoredFile File)>();
        foreach (var collection in collections)
        {
            var isOwner = collection.Owners.Any(o => o.UserId == currentUserId);

            var visibleFiles = isOwner
                ? collection.Files
                : collection.Files.Where(f => f.IsPublic || f.Owners.Any(o => o.UserId == currentUserId));

            var first = visibleFiles.OrderBy(f => f.SortOrder).FirstOrDefault();
            if (first is not null)
                picks.Add((collection.Id, first));
        }

        if (picks.Count == 0)
            return [];

        // ѕараллельно скачиваем байты только выбранных файлов из S3.
        var previewTasks = picks.Select(async pick =>
        {
            try
            {
                await using var stream = await _storage
                    .GetObjectStreamAsync(pick.File.StorageKey, cancellationToken)
                    .ConfigureAwait(false);

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);

                return new CollectionPreviewDto(
                    pick.CollectionId, 
                    pick.File.Id, 
                    pick.File.ContentType, 
                    Convert.ToBase64String(ms.ToArray()));
            }
            catch
            {
                // ≈сли конкретный файл недоступен в хранилище Ч пропускаем,
                // карточка просто останетс€ без превью.
                return null;
            }
        });

        var previews = await Task.WhenAll(previewTasks).ConfigureAwait(false);
        return previews.Where(p => p is not null).Select(p => p!).ToList();
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
}
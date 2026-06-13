using ExceptionHandler.Exceptions;
using GatewayService.WebApi.Common.DTO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayService.WebApi.Services;

public interface IMinorTaskAggregator
{
    Task<MinorTaskDetailResponse?> GetTaskWithImagesAsync(Guid minorTaskId, string? authHeader);
    Task<List<MinorTaskListItemResponse>> GetTasksWithPreviewAsync(int? limit, int? from, int? to, string? authHeader);
    Task<MinorTaskDetailResponse?> CreateTaskWithFilesAsync(CreateMinorTaskGatewayRequest request, string? authHeader);

    // Ленивая подгрузка превью: по списку task ID → превью (base64) батчем.
    Task<List<TaskPreviewResponse>> GetTaskPreviewsAsync(IReadOnlyCollection<Guid> taskIds, string? authHeader);

    // Возвращают HTTP статус для проброса в контроллер
    Task<System.Net.HttpStatusCode> UpdateTaskAsync(Guid minorTaskId, UpdateMinorTaskGatewayRequest request, string? authHeader);
    Task<System.Net.HttpStatusCode> UpdateTaskStatusAsync(Guid minorTaskId, Guid statusId, string? authHeader);
    Task<System.Net.HttpStatusCode> DeleteTaskAsync(Guid minorTaskId, string? authHeader);
    Task<System.Net.HttpStatusCode> DeleteTaskParticipantAsync(Guid minorTaskId, Guid participantId, string? authHeader);
}

public class MinorTaskAggregator : IMinorTaskAggregator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public MinorTaskAggregator(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // ─── GET /api/tasks/{id} ─────────────────────────────────────────────────

    public async Task<MinorTaskDetailResponse?> GetTaskWithImagesAsync(Guid minorTaskId, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);
        var dataClient = CreateClient("DataService", authHeader);

        var taskResponse = await taskClient.GetAsync($"/api/tasks/{minorTaskId}");
        if (!taskResponse.IsSuccessStatusCode) return null;

        var task = await DeserializeAsync<MinorTaskResponse>(taskResponse);
        if (task is null) return null;

        // Внутренний GetMinorTaskResponse не возвращает Id — подставляем из path
        if (task.Id == Guid.Empty) task.Id = minorTaskId;

        var detail = MapToDetail(task);

        if (task.FileCollectionId.HasValue)
        {
            var filesResponse = await dataClient.GetAsync($"/api/collections/{task.FileCollectionId}/files");
            if (filesResponse.IsSuccessStatusCode)
            {
                var files = await DeserializeAsync<List<FileMetadataDto>>(filesResponse) ?? [];
                var enriched = await Task.WhenAll(
                    files.Select(f => EnrichWithFileDataAsync(f, dataClient)));
                detail.Images = [.. enriched];
            }
        }

        return detail;
    }

    // ─── GET /api/tasks ──────────────────────────────────────────────────────
    //
    //  БЫСТРЫЙ СПИСОК: картинки больше НЕ скачиваются здесь. Возвращаем только
    //  метаданные задач + FileCollectionId, чтобы фронт догрузил превью отдельно
    //  через GetTaskPreviewsAsync. Это убирает основную задержку (скачивание и
    //  base64-кодирование байтов всех превью внутри одного ответа).

    public async Task<List<MinorTaskListItemResponse>> GetTasksWithPreviewAsync(int? limit, int? from, int? to, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);

        var query = BuildQueryString(limit, from, to);
        var tasksResponse = await taskClient.GetAsync($"/api/tasks{query}");
        if (!tasksResponse.IsSuccessStatusCode)
            throw new ForbiddenException();

        var tasks = await DeserializeAsync<List<MinorTaskResponse>>(tasksResponse) ?? [];

        return tasks.Select(task => new MinorTaskListItemResponse
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Latitude = task.Latitude,
            Longitude = task.Longitude,
            NumberVolunteers = task.NumberVolunteers,
            Encouragement = task.Encouragement,
            Status = task.Status,
            ConsumerId = task.ConsumerId,
            FileCollectionId = task.FileCollectionId,   // превью догрузит фронт
            // PreviewImageData / ContentType намеренно null в быстром списке
        }).ToList();
    }

    // ─── POST /api/tasks/previews — батч-превью для ленивой загрузки ─────────
    //
    //  Фронт присылает пачку task ID (например, по 5–10). Мы:
    //   1) резолвим их FileCollectionId (батч-запросом к MinorTaskService, если
    //      есть /api/tasks/names-подобный батч; иначе — параллельными GET),
    //   2) одним запросом к DataService /api/collections/previews получаем
    //      первый файл каждой коллекции в base64,
    //   3) сопоставляем обратно task ID → превью.

    public async Task<List<TaskPreviewResponse>> GetTaskPreviewsAsync(
        IReadOnlyCollection<Guid> taskIds, string? authHeader)
    {
        if (taskIds is null || taskIds.Count == 0) return [];

        var taskClient = CreateClient("MinorTaskService", authHeader);
        var dataClient = CreateClient("DataService", authHeader);

        var ids = taskIds.Distinct().ToList();

        // 1. task ID → FileCollectionId. Тянем задачи параллельно.
        //    (Если в MinorTaskService появится батч-эндпоинт, отдающий
        //     пары id/fileCollectionId — заменить этот блок на один запрос.)
        var taskToCollection = new Dictionary<Guid, Guid>();
        var fetches = ids.Select(async id =>
        {
            try
            {
                var resp = await taskClient.GetAsync($"/api/tasks/{id}");
                if (!resp.IsSuccessStatusCode) return (id, (Guid?)null);
                var task = await DeserializeAsync<MinorTaskResponse>(resp);
                return (id, task?.FileCollectionId);
            }
            catch { return (id, (Guid?)null); }
        });
        var resolved = await Task.WhenAll(fetches);
        foreach (var (id, collId) in resolved)
            if (collId.HasValue && collId.Value != Guid.Empty)
                taskToCollection[id] = collId.Value;

        if (taskToCollection.Count == 0) return [];

        // 2. Батч-превью из DataService: collectionId → base64.
        var collectionIds = taskToCollection.Values.Distinct().ToList();
        var body = JsonSerializer.Serialize(new { CollectionIds = collectionIds }, _jsonOptions);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        using var previewsResp = await dataClient.PostAsync("/api/collections/previews", content);
        if (!previewsResp.IsSuccessStatusCode) return [];

        var previews = await DeserializeAsync<List<CollectionPreviewDto>>(previewsResp) ?? [];
        var byCollection = previews
            .GroupBy(p => p.CollectionId)
            .ToDictionary(g => g.Key, g => g.First());

        // 3. Сопоставляем task ID → превью.
        var result = new List<TaskPreviewResponse>(taskToCollection.Count);
        foreach (var (taskId, collId) in taskToCollection)
        {
            if (byCollection.TryGetValue(collId, out var preview))
            {
                result.Add(new TaskPreviewResponse
                {
                    TaskId = taskId,
                    PreviewImageData = preview.Data,
                    PreviewImageContentType = preview.ContentType,
                });
            }
        }

        return result;
    }

    // ─── POST /api/tasks (с загрузкой файлов) ────────────────────────────────

    public async Task<MinorTaskDetailResponse?> CreateTaskWithFilesAsync(CreateMinorTaskGatewayRequest request, string? authHeader)
    {
        var dataClient = CreateClient("DataService", authHeader);
        var taskClient = CreateClient("MinorTaskService", authHeader);

        // 1. Создаём коллекцию файлов
        var collectionResponse = await dataClient.PostAsync("/api/collections", null);
        collectionResponse.EnsureSuccessStatusCode();
        var collectionIdStr = await collectionResponse.Content.ReadAsStringAsync();
        var collectionId = JsonSerializer.Deserialize<Guid>(collectionIdStr, _jsonOptions);

        // 2. Загружаем файлы, если переданы
        if (request.Images is { Count: > 0 })
        {
            foreach (var image in request.Images)
            {
                using var form = new MultipartFormDataContent();
                var streamContent = new StreamContent(image.OpenReadStream());
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);
                form.Add(streamContent, "file", image.FileName);

                var uploadResponse = await dataClient.PostAsync($"/api/collections/{collectionId}/files?isPublic=true", form);
                uploadResponse.EnsureSuccessStatusCode();
            }
        }

        // 3. Создаём задачу с FileCollectionId
        var taskPayload = new
        {
            request.Name,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.NumberVolunteers,
            request.Encouragement,
            Images = collectionId
        };

        var json = JsonSerializer.Serialize(taskPayload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await taskClient.PostAsync("/api/tasks", content);
        createResponse.EnsureSuccessStatusCode();

        var createdId = await DeserializeAsync<Guid>(createResponse);

        // 4. Возвращаем задачу с полным списком изображений
        return await GetTaskWithImagesAsync(createdId, authHeader);
    }

    // ─── PATCH /api/tasks/{id} — обновление полей задачи ────────────────────

    public async Task<System.Net.HttpStatusCode> UpdateTaskAsync(
        Guid minorTaskId, UpdateMinorTaskGatewayRequest request, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpReq = new HttpRequestMessage(HttpMethod.Patch, $"/api/tasks/{minorTaskId}")
        {
            Content = content
        };
        using var response = await taskClient.SendAsync(httpReq);
        return response.StatusCode;
    }

    // ─── PUT /api/tasks/{id} — обновление статуса ───────────────────────────

    public async Task<System.Net.HttpStatusCode> UpdateTaskStatusAsync(
        Guid minorTaskId, Guid statusId, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);
        var json = JsonSerializer.Serialize(new { StatusId = statusId }, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await taskClient.PutAsync($"/api/tasks/{minorTaskId}", content);
        return response.StatusCode;
    }

    // ─── DELETE /api/tasks/{id} ──────────────────────────────────────────────

    public async Task<System.Net.HttpStatusCode> DeleteTaskAsync(Guid minorTaskId, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);
        using var response = await taskClient.DeleteAsync($"/api/tasks/{minorTaskId}");
        return response.StatusCode;
    }

    // ─── DELETE /api/tasks/{id}/participants/{participantId} ────────────────

    public async Task<System.Net.HttpStatusCode> DeleteTaskParticipantAsync(
        Guid minorTaskId, Guid participantId, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);
        using var response = await taskClient.DeleteAsync(
            $"/api/tasks/{minorTaskId}/participants/{participantId}");
        return response.StatusCode;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private HttpClient CreateClient(string name, string? authHeader)
    {
        var client = _httpClientFactory.CreateClient(name);
        if (!string.IsNullOrEmpty(authHeader))
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
        return client;
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
    }

    private static MinorTaskDetailResponse MapToDetail(MinorTaskResponse task) => new()
    {
        Id = task.Id,
        Name = task.Name,
        Description = task.Description,
        Latitude = task.Latitude,
        Longitude = task.Longitude,
        NumberVolunteers = task.NumberVolunteers,
        Encouragement = task.Encouragement,
        Status = task.Status,
        ConsumerId = task.ConsumerId
    };

    private async Task<FileMetadataDto> EnrichWithFileDataAsync(FileMetadataDto file, HttpClient dataClient)
    {
        var response = await dataClient.GetAsync($"/api/files/{file.Id}");
        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            file.Data = Convert.ToBase64String(bytes);
        }
        return file;
    }

    private static string BuildQueryString(int? limit, int? from, int? to)
    {
        var parts = new List<string>();
        if (limit.HasValue) parts.Add($"limit={limit}");
        if (from.HasValue) parts.Add($"from={from}");
        if (to.HasValue) parts.Add($"to={to}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
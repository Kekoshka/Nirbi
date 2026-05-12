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

    public async Task<List<MinorTaskListItemResponse>> GetTasksWithPreviewAsync(int? limit, int? from, int? to, string? authHeader)
    {
        var taskClient = CreateClient("MinorTaskService", authHeader);
        var dataClient = CreateClient("DataService", authHeader);

        var query = BuildQueryString(limit, from, to);
        var tasksResponse = await taskClient.GetAsync($"/api/tasks{query}");
        if (!tasksResponse.IsSuccessStatusCode)
            throw new ForbiddenException();
        if (!tasksResponse.IsSuccessStatusCode) return [];

        var tasks = await DeserializeAsync<List<MinorTaskResponse>>(tasksResponse) ?? [];

        // Параллельно получаем первый файл из каждой коллекции
        var results = await Task.WhenAll(tasks.Select(async task =>
        {
            var item = new MinorTaskListItemResponse
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Latitude = task.Latitude,
                Longitude = task.Longitude,
                NumberVolunteers = task.NumberVolunteers,
                Encouragement = task.Encouragement
            };

            if (task.FileCollectionId.HasValue)
            {
                var filesResponse = await dataClient.GetAsync($"/api/collections/{task.FileCollectionId}/files");
                if (filesResponse.IsSuccessStatusCode)
                {
                    var files = await DeserializeAsync<List<FileMetadataDto>>(filesResponse) ?? [];
                    var first = files.OrderBy(f => f.SortOrder).FirstOrDefault();
                    if (first is not null)
                    {
                        // Скачиваем байты только первого файла
                        var fileResponse = await dataClient.GetAsync($"/api/files/{first.Id}");
                        if (!fileResponse.IsSuccessStatusCode)
                            throw new ForbiddenException();
                        if (fileResponse.IsSuccessStatusCode)
                        {
                            var bytes = await fileResponse.Content.ReadAsByteArrayAsync();
                            item.PreviewImageData = Convert.ToBase64String(bytes);
                            item.PreviewImageContentType = first.ContentType;
                        }
                    }
                }
            }

            return item;
        }));

        return [.. results];
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

                var uploadResponse = await dataClient.PostAsync($"/api/collections/{collectionId}/files", form);
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

        var created = await DeserializeAsync<MinorTaskResponse>(createResponse);
        if (created is null) return null;

        // 4. Возвращаем задачу с полным списком изображений
        return await GetTaskWithImagesAsync(created.Id, authHeader);
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
        Encouragement = task.Encouragement
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

    private string BuildFileDownloadUrl(Guid fileId)
    {
        // Gateway предоставляет публичный URL для скачивания файлов
        var gatewayBase = _configuration["GatewayBaseUrl"] ?? string.Empty;
        return $"{gatewayBase}/api/files/{fileId}";
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

using GatewayService.WebApi.Common.DTO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayService.WebApi.Services;

public interface IConfirmationsAggregator
{
    Task<List<EnrichedConfirmationResponse>> GetByReviewerAsync(Guid reviewerId, string? authHeader);
    Task<List<EnrichedConfirmationResponse>> GetByInitiatorAsync(Guid initiatorId, string? authHeader);
}

public class ConfirmationsAggregator : IConfirmationsAggregator
{
    IHttpClientFactory _httpClientFactory;
    ILogger<ConfirmationsAggregator> _logger;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public ConfirmationsAggregator(IHttpClientFactory httpClientFactory, ILogger<ConfirmationsAggregator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ── Public ────────────────────────────────────────────────────────────────

    public async Task<List<EnrichedConfirmationResponse>> GetByReviewerAsync(
        Guid reviewerId, string? authHeader)
    {
        var confirmations = await FetchConfirmationsAsync(
            $"/api/Confirmations/reviewer/{reviewerId}", authHeader);
        return await EnrichAsync(confirmations, authHeader);
    }

    public async Task<List<EnrichedConfirmationResponse>> GetByInitiatorAsync(
        Guid initiatorId, string? authHeader)
    {
        var confirmations = await FetchConfirmationsAsync(
            $"/api/Confirmations/initiator/{initiatorId}", authHeader);
        return await EnrichAsync(confirmations, authHeader);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task<List<ConfirmationBaseResponse>> FetchConfirmationsAsync(
        string url, string? authHeader)
    {
        var client = CreateClient("ConfirmationService", authHeader);
        using var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];
        return await DeserializeAsync<List<ConfirmationBaseResponse>>(response) ?? [];
    }

    private async Task<List<EnrichedConfirmationResponse>> EnrichAsync(
        List<ConfirmationBaseResponse> confirmations, string? authHeader)
    {
        if (confirmations.Count == 0) return [];

        // Собираем уникальные user ID и task ID
        var userIds = confirmations
            .SelectMany(c => new[] { c.InitiatorId, c.ReviewerId })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var taskIds = confirmations
            .Select(c => c.EntityId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Параллельно тянем usernames и task names
        var usernamesTask  = FetchUsernamesAsync(userIds, authHeader);
        var taskNamesTask  = FetchTaskNamesAsync(taskIds, authHeader);
        await Task.WhenAll(usernamesTask, taskNamesTask);

        var usernames  = await usernamesTask;
        var taskNames  = await taskNamesTask;

        return confirmations.Select(c => new EnrichedConfirmationResponse
        {
            Id                  = c.Id,
            ConfirmationType    = c.ConfirmationType,
            EntityId            = c.EntityId,
            EntityName          = taskNames.GetValueOrDefault(c.EntityId),
            InitiatorId         = c.InitiatorId,
            InitiatorUsername   = usernames.GetValueOrDefault(c.InitiatorId),
            ReviewerId          = c.ReviewerId,
            ReviewerUsername    = usernames.GetValueOrDefault(c.ReviewerId),
            Status              = c.Status,
            CreatedAt           = c.CreatedAt,
            ExpiresAt           = c.ExpiresAt,
            RespondedAt         = c.RespondedAt,
            RejectionReason     = c.RejectionReason,
            MetaData            = c.MetaData,
        }).ToList();
    }

    /// <summary>
    /// Получает username для каждого userId из AuthService.
    /// Запросы параллельные; неудачи игнорируются — ID просто останется без username.
    /// </summary>
    private async Task<Dictionary<Guid, string>> FetchUsernamesAsync(
        List<Guid> userIds, string? authHeader)
    {
        if (userIds.Count == 0) return [];

        var authClient = _httpClientFactory.CreateClient("AuthService");

        var tasks = userIds.Select(async id =>
        {
            try
            {
                using var response = await authClient.GetAsync($"/api/Users/{id}");
                if (!response.IsSuccessStatusCode) return (id, null as string);
                var user = await DeserializeAsync<UserLookupDto>(response);
                return (id, user?.Username);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return (id, null); 
            }
        });

        var results = await Task.WhenAll(tasks);
        return results
            .Where(r => r.Item2 is not null)
            .ToDictionary(r => r.id, r => r.Item2!);
    }

    /// <summary>
    /// Batch-запрос имён задач по IDs через POST /api/tasks/names в MinorTaskService.
    /// </summary>
    private async Task<Dictionary<Guid, string>> FetchTaskNamesAsync(
        List<Guid> taskIds, string? authHeader)
    {
        if (taskIds.Count == 0) return [];
        try
        {
            var taskClient = CreateClient("MinorTaskService", authHeader);
            var body = JsonSerializer.Serialize(new { Ids = taskIds }, _json);
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await taskClient.PostAsync("/api/tasks/names", content);

            if (!response.IsSuccessStatusCode) return [];

            var taskNames = await DeserializeAsync<List<TaskNameLookupDto>>(response);
            return taskNames?.ToDictionary(t => t.Id, t => t.Name) ?? [];
        }
        catch { return []; }
    }

    private HttpClient CreateClient(string name, string? authHeader)
    {
        var client = _httpClientFactory.CreateClient(name);
        if (!string.IsNullOrEmpty(authHeader))
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
        return client;
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>();
    }
}

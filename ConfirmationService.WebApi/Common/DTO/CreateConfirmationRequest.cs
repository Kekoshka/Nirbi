namespace ConfirmationService.WebApi.Common.DTO;

public class CreateConfirmationRequest
{
    /// <summary>
    /// Тип подтверждения
    /// </summary>
    public string ConfirmationType { get; set; }

    /// <summary>
    /// ID сущности (TaskId, UserId, и т.д.)
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// ID пользователя для проверки (рецензента)
    /// </summary>
    public Guid ReviewerId { get; set; }

    /// <summary>
    /// Время жизни подтверждения в часах (по умолчанию 48)
    /// </summary>
    public int ExpirationHours { get; set; } = 48;

    /// <summary>
    /// Дополнительные данные (JSON)
    /// </summary>
    public Dictionary<string, object> MetaData { get; set; }
}
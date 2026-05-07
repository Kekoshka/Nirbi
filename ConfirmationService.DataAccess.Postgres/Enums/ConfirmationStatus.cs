namespace ConfirmationService.DataAccess.Enums
{
    public enum ConfirmationStatus
    {
        Created = 1,      // Подтверждение создано, ждет ответа
        Pending = 2,      // В процессе обработки
        Accepted = 3,     // Принято
        Rejected = 4,     // Отклонено
        Expired = 5,      // Истекло по времени
        Revoked = 6       // Отозвано инициатором
    }
}
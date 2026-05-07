namespace ConfirmationService.DataAccess.Enums
{
    public enum ConfirmationType
    {
        TaskJoin = 1,           // Присоединение к задаче
        UserAdd = 2,            // Добавление пользователя
        TaskCompletion = 3,     // Подтверждение завершения задачи
        UserRemoval = 4,        // Удаление пользователя из задачи
        TaskApproval = 5        // Общее одобрение задачи
    }
}

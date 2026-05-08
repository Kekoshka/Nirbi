namespace NotificationService.WebApi.Common.Options
{
    /// <summary>
    /// Настройки слушателей Kafka
    /// </summary>
    public class KafkaConsumersOptions
    {
        /// <summary>
        /// Id группы слушателей
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Стартовая точка offset, в случае если он не задан
        /// </summary>
        public string AutoOffsetReset { get; set; }

        /// <summary>
        /// Возможность автоматических коммитов сообщений
        /// </summary>
        public string EnableAutoCommit { get; set; }
    }
}

namespace NotificationService.WebApi.Common.Options
{
    /// <summary>
    /// Настройки JWT токенов
    /// </summary>
    public class JWTOptions
    {
        /// <summary>
        /// Сервис, выпускающий токены
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Ключ для шифрования токена
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Время жизни токена
        /// </summary>
        public int LifeTimeFromMinutes { get; set; }
    }
}

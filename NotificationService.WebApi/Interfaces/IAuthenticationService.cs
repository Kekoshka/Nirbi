namespace NotificationService.WebApi.Interfaces
{
    /// <summary>
    /// Необходим для аутентификации пользователей в системе
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Авторизация и получение JWT-токена 
        /// </summary>
        /// <param name="email">Email, указанный при заказе</param>
        /// <returns></returns>
        public string GetJWT(string email);
    }
}

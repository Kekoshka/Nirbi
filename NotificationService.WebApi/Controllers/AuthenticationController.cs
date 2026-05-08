using Microsoft.AspNetCore.Mvc;
using NotificationService.WebApi.Interfaces;

namespace NotificationService.WebApi.Controllers
{
    /// <summary>
    /// Проводит аутентификацию пользователей 
    /// </summary>
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        /// <summary>
        /// Сервис аутентификации
        /// </summary>
        IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Аутентифицирует пользователей и выдает JWT-токен
        /// </summary>
        /// <param name="email">Email пользователя, указанный при заказе</param>
        /// <returns></returns>
        [HttpPost("authenticate")]
        public IActionResult Authenticate(string email)
        {
            var jwt = _authenticationService.GetJWT(email);
            return Ok(jwt);
        }
    }
}

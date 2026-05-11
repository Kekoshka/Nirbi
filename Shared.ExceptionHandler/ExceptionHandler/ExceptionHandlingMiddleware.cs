using ExceptionHandler.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Refit;
using System.Text.Json;


namespace ExceptionHandler
{
    /// <summary>
    /// Middleware для обработки исключений
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        RequestDelegate _next;
        ILogger<ExceptionHandlingMiddleware> _logger;
        string _jsonContentType = "application/json";

        /// <summary>
        /// Инициализация нового Middleware для обработки ошибок
        /// </summary>
        /// <param name="next">Делегат следующего обработчика</param>
        /// <param name="logger">Logger</param>
        /// <param name="contentTypes">типы контента</param>
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        /// <summary>
        /// Метод обработки ошибок, если в последующих методах выбросится исключение, оно будет обработано
        /// </summary>
        /// <param name="context">HttpContext</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred: {message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                ForbiddenException => StatusCodes.Status403Forbidden,
                InternalServerErrorException => StatusCodes.Status500InternalServerError,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                UnprocessableEntityException => StatusCodes.Status422UnprocessableEntity,
                ApiException => StatusCodes.Status400BadRequest,
                Exception => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = _jsonContentType;

            string error = "Internal server error";
            if (statusCode != StatusCodes.Status500InternalServerError)
                error = exception.Message;
            if(exception is ApiException ex)
            {
                var content = await ex.GetContentAsAsync<object>();
                error = content.ToString();
            }

            var response = new
            {
                error,
                status = statusCode
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}

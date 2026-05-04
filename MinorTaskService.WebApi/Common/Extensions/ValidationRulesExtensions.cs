using FluentValidation;
using MinorTaskService.WebApi.Common.Options;
using System.Numerics;

namespace MinorTaskService.WebApi.Common.Extensions
{
    public static class ValidationRulesExtensions 
    {


        /// <summary>
        /// Правила для проверки логина
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки логина</returns>
        /// <remarks>
        /// Логин не должен быть пустым, должен иметь длинну от 3 до 50 символов,
        /// должен содержать только буквы, цифры и нижнее подчеркивание
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidateLogin<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Login cannot be empty")
                .Length(ValidationOptions.MinLengthLogin, ValidationOptions.MaxLengthLogin).WithMessage("Length of the login must be from 5 to 50 characters")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Login can contain only letters numbers and underscores");
        }
        /// <summary>
        /// Правила для проверки пароля
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки пароля</returns>
        /// <remarks>
        /// Пароль не должен быть пустым, должен иметь длинну от 8 до 50 символов,
        /// Должен содержать хотя бы по одну строчную и заглавную буквы, 
        /// должен содержать хотя бы одну цифру,
        /// должен содержать хотя бы один специальный символ
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Login cannot be empty")
                .Length(ValidationOptions.MinLengthPassword, ValidationOptions.MaxLengthPassword).WithMessage("Length of the password nust be from 8 to 50 characters")
                .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$_!%*?&])[A-Za-z\\d@$_!%*?&]{8,}$").WithMessage("The password must contain at least 8 characters, at least one lowercase letter, at least one uppercase letter, at least one digit, and at least one special character.");
        }
        /// <summary>
        /// Правила для проверки названия
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки названия</returns>
        /// <remarks>
        /// Название не должно быть пустым и должно содержать от 1 до 100 символов
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidateName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Name cannot be empty")
                .Length(ValidationOptions.MinLengthName, ValidationOptions.MaxLengthName).WithMessage("Length of the name must be from 1 to 100 characters");
        }
        /// <summary>
        /// Правила для проверки описания
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки описания</returns>
        /// <remarks>
        /// Описание не должно быть более 500 символов
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidateDescription<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(ValidationOptions.MaxLengthDescription).WithMessage("Description should not exceed 500 characters");
        }
        /// <summary>
        /// Правила для проверки положительных чисел
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки положительных чисел</returns>
        /// <remarks>
        /// Число должно быть больше или равно 0
        /// </remarks>
        public static IRuleBuilderOptions<T, int> ValidatePositiveNumber<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(ValidationOptions.MinValuePositiveNumber).WithMessage("Number must be a positive number or equal to 0");
        }
        /// <summary>
        /// Правила для проверки сообщения
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки сообщений</returns>
        /// <remarks>
        /// Сообщение не должно быть пустым и не должно содержать более 1000 символов
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidateMessage<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Message cannot be empty")
                .MaximumLength(ValidationOptions.MaxLengthMessage).WithMessage("Message should not exceed 1000 characters");
        }
        /// <summary>
        /// Правила для проверки даты
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки даты</returns>
        /// <remarks>
        /// Дата не должна быть в прошедшем времени
        /// Есть запас 10 секунд на задержку
        /// </remarks>
        public static IRuleBuilderOptions<T, DateTime> ValidateFutureDate<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(DateTime.UtcNow - TimeSpan.FromSeconds(ValidationOptions.TimeReserveForDelay)) //запас 10 секунд на задержку 
                .WithMessage("Date cannot be in the past");
        }
        /// <summary>
        /// Правила для проверки приоритета
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки приоритета</returns>
        /// <remarks>
        /// Приоритет задачи должен быть выставлен в приделах от 1 до 10
        /// </remarks>
        public static IRuleBuilderOptions<T, int> ValidatePriority<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .InclusiveBetween(ValidationOptions.MinPriority, ValidationOptions.MaxPriority)
                .WithMessage("Priority must be from 1 to 10");
        }
        /// <summary>
        /// Правила для проверки Сообщения
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки сообщений</returns>
        /// <remarks>
        /// Описание действия не должно быть пустым, максимальная длина описания не более 500 символов
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidateAction<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Action cannot be empty")
                .MaximumLength(ValidationOptions.MaxLengthAction).WithMessage("Message should not exceed 500 characters");
        }
        /// <summary>
        /// Правила для проверки имени пользователя
        /// </summary>
        /// <typeparam name="T">Тип модели, для которой будут использоваться правила</typeparam>
        /// <param name="ruleBuilder">Построитель правил</param>
        /// <returns>Настроенный построитель правил для проверки имени пользователя</returns>
        /// <remarks>
        /// Сообщение не должно быть пустым и не должно содержать более 1000 символов
        /// </remarks>
        public static IRuleBuilderOptions<T, string> ValidateUserName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("UserName cannot be empty")
                .Length(ValidationOptions.MinLengthUserName, ValidationOptions.MaxLengthUserName).WithMessage("Length of the UserName must be from 2 to 50 characters")
                .Matches("^[а-яА-Яa-zA-Z]+$").WithMessage("UserName must contain only letters");
        }
    }
}

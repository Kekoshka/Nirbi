using Confluent.SchemaRegistry;
using ExceptionHandler.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotificationService.WebApi.Common.Options;
using NotificationService.WebApi.Services.BackgroundServices;
using Refit;
using System.Reflection;
using System.Text;

namespace NotificationService.WebApi.Common.Extensions
{
    public static class ServicesExtensions
    {
        /// <summary>
        /// Расширение для настройки аутентификации по JWT токенам
        /// </summary>
        public static void AddJWTAuthentication(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            JWTOptions jwtOptions = serviceProvider.GetService<IOptions<JWTOptions>>()!.Value;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                });
        }

        /// <summary>
        /// Расширение для настройки клиента schema registry
        /// </summary>
        public static void AddSchemaRegistryClient(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddSingleton<ISchemaRegistryClient>(sp =>
                new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = configuration.GetSection(nameof(ExternalServicesOptions)).GetValue<string>(nameof(ExternalServicesOptions.SchemaRegistryAddress))
                }));
        }

        /// <summary>
        /// Расширение для регистрации Hosted сервисов слушателей Kafka
        /// </summary>
        public static void AddKafkaConsumers(this IServiceCollection services)
        {
            services.AddHostedService<OrderCreatedConsumer>();
            services.AddHostedService<PaymentUpdatedConsumer>();
        }

        /// <summary>
        /// Регистрация всех сервисов, находящихся в проекте
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterExecutingAsseblyServices(this IServiceCollection services)
        {
            var serviceTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(st => st.IsClass && !st.IsAbstract && st.Name.EndsWith("Service"));
            foreach (var serviceType in serviceTypes)
            {
                var interfaceType = serviceType.GetInterfaces()
                    .FirstOrDefault(it => it.Name == $"I{serviceType.Name}");
                if (interfaceType is not null)
                    services.AddScoped(interfaceType, serviceType);
            }
        }

        /// <summary>
        /// Регистрирует все мапперы из текущей сборки, имена которых заканчиваются на "Mapper".
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        public static void RegisterMappers(this IServiceCollection services)
        {
            var mappers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(st => st.IsClass && st.Name.EndsWith("Mapper"));
            foreach (var mapper in mappers)
            {
                if (mapper is not null)
                    services.AddSingleton(mapper);
            }
        }

        /// <summary>
        /// Связка классов опций приложения с данными из appsettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ContentTypesOptions>(configuration.GetSection(nameof(ContentTypesOptions)));
            services.Configure<ExternalServicesOptions>(configuration.GetSection(nameof(ExternalServicesOptions)));
            services.Configure<DataTypesOptions>(configuration.GetSection(nameof(DataTypesOptions)));
            services.Configure<KafkaConsumersOptions>(configuration.GetSection(nameof(KafkaConsumersOptions)));
            services.Configure<JWTOptions>(configuration.GetSection(nameof(JWTOptions)));
        }

        /// <summary>
        /// Регистрирует Http клиенты для обращения к внешним сервисам через Refit
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <exception cref="NotFoundException">Базовый адрес в appsettings в секции ExternalServicesOptions не найден</exception>
        public static void AddRefit(this IServiceCollection services, IConfiguration configuration)
        {
            var apiInterfaces = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(st => st.IsInterface && st.Name.StartsWith("I") && st.Name.EndsWith("Api"));
            foreach (var apiInterface in apiInterfaces)
            {
                var serviceName = apiInterface.Name.Substring(1, apiInterface.Name.Length - 4);
                var baseAddress = configuration
                    .GetSection(nameof(ExternalServicesOptions))
                    .GetValue<string>(serviceName + "Address");
                if (baseAddress is null)
                    throw new NotFoundException($"Base address for {serviceName} not found");

                services.AddRefitClient(apiInterface)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));
            }
        }
    }
}

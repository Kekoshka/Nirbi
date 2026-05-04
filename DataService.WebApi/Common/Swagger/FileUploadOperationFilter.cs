using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Проверяем наличие IFormFile в параметрах метода
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                        p.ParameterType == typeof(IEnumerable<IFormFile>));

        if (fileParams.Any())
        {
            // Настраиваем тело запроса как multipart/form-data
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Properties = fileParams.ToDictionary(
                                p => p.Name,
                                p => new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Format = "binary" // Это ключевой момент для появления кнопки выбора файла
                                }) as IDictionary<string, IOpenApiSchema>
                        }
                    }
                }
            };
        }
    }
}
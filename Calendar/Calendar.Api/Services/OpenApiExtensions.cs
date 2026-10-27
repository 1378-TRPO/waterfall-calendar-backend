using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Calendar.Api.Services
{
    public static class OpenApiExtensions
    {
        private const string BearerScheme = "Bearer";

        /// <summary>
        /// Описывает JWT Bearer в OpenAPI-документе: в Swagger UI появляется кнопка Authorize,
        /// а замок и заголовок Authorization — только у эндпоинтов с [Authorize].
        /// </summary>
        public static OpenApiOptions AddBearerSecurity(this OpenApiOptions options)
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Токен из POST /api/auth/login. Вставьте только сам токен, без слова Bearer."
                };
                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requiresAuth = metadata.OfType<IAuthorizeData>().Any()
                    && !metadata.OfType<IAllowAnonymous>().Any();

                if (requiresAuth)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = []
                    });
                }

                return Task.CompletedTask;
            });

            return options;
        }
    }
}

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.OpenApi
{
    internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        private const string SchemeId = "Bearer";

        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste a JWT access token to authorize requests."
            };

            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeId, document)] = new List<string>()
            });

            return Task.CompletedTask;
        }
    }
}

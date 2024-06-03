using System;
using System.IO;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AddSwaggerServicesExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            var openApiInformation = new OpenApiInfo
            {
                Title = "KEDA API",
                Version = "v1"
            };

            services.AddSwaggerGen(swaggerGenerationOptions =>  
            {
                swaggerGenerationOptions.SwaggerDoc("v1", openApiInformation);
                swaggerGenerationOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    "Keda.Samples.DotNet.Web.Open-Api.xml"));
            });
        }
    }
}
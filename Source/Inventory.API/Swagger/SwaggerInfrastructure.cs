using Inventory.API.Swagger;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Inventory.API;

public static class SwaggerInfrastructure
{
    public static IServiceCollection AddSwaggerInfrastructure(this IServiceCollection services, string xmlCommentsFilePath)
        {
            services.AddApiVersioning(
                options =>
                {
                    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
                    options.AssumeDefaultVersionWhenUnspecified = true;

                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = false;
                });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                options =>
                {
                    // Swagger 2.+ support                    
                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    };

                    options.AddSecurityDefinition("Bearer", securityScheme);
                    options.AddSecurityRequirement(
                        new OpenApiSecurityRequirement {
                            { securityScheme, Array.Empty<string>()}
                        });

                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    if (!string.IsNullOrEmpty(xmlCommentsFilePath))
                    {
                        // integrate xml comments
                        options.IncludeXmlComments(xmlCommentsFilePath);
                    }
                });

            return services;
        }

        public static IApplicationBuilder UseSwaggerInfrastructure(this IApplicationBuilder builder, IWebHostEnvironment environment)
        {
            if (!environment.EnvironmentName.Equals("Development"))
            {
                builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
            }
            builder.UseSwagger();
            builder.UseSwaggerUI();

            return builder;
        }
}
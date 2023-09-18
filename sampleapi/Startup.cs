using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag;

namespace sampleapi
{
    internal class Startup
    {
        private static readonly byte[] ROBOTS_TXT = Encoding.UTF8.GetBytes("User-agent: *\nDisallow: /\n");
        readonly string? _baseroot;

        public Startup(IConfiguration configuration, Parameters p)
        {
            Configuration = configuration;
            _baseroot = p.BaseRoot;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore(options =>
                {
                    if (!string.IsNullOrEmpty(_baseroot)) options.Conventions.Add(new RoutePrefixConvention(new RouteAttribute(_baseroot)));
                }).AddJsonOptions(options => // irelevant
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            //services.AddScoped<IGenericDataAccess>(_ => new DataAccess.DataAccess());

            services.AddOpenApiDocument(config =>
            {
                config.Title = "Sample api";
                config.DocumentName = "Sample api";
                config.Version = "666";
                config.AllowReferencesWithProperties = true;
            });

            services
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseExceptionHandler("/Error");

            // Response headers
            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path == "/robots.txt")
                {
                    await ctx.Response.BodyWriter.WriteAsync(ROBOTS_TXT);
                }
                else
                {
                    ctx.Response.OnStarting(() =>
                    {
                        // Expect-CT ... the doesn't care if the cert hosted (optionally) by the HTTP server is valid. That is somebody's else problem.
                        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
                        ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                        ctx.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
                        ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                        ctx.Response.Headers["Cache-Control"] = "no-store";
                        ctx.Response.Headers["Expires"] = "0";
                        ctx.Response.Headers["Pragma"] = "no-cache";
                        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=15552000";

                        // Content-Security-Policy ... that should be set also for the Converge UI
                        // This will work as NSwag as it is right now:
                        ctx.Response.Headers["Content-Security-Policy"] = "default-src 'none';script-src 'self' 'unsafe-inline';connect-src 'self';style-src 'self' 'unsafe-inline';img-src 'self' data:;";
                        // This is the policy I would prefer to use
                        // "default-src 'none';script-src 'self';connect-src 'self';style-src 'self'");

                        return Task.CompletedTask;
                    });
                    await next();
                }
            });

            //app.UseIecExceptionHandler();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwaggerUi3((settings) =>
            {
                settings.TagsSorter = "alpha";
                settings.OperationsSorter = "alpha";
            });
            app.UseOpenApi(a =>
            {
                a.PostProcess = (document, request) =>
                {
                    document.Schemes = new[] { OpenApiSchema.Http, OpenApiSchema.Https };
                };
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

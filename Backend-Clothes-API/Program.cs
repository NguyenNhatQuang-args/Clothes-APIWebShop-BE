using Backend_Clothes_API.Extensions;
using Backend_Clothes_API.Middleware;
using DotNetEnv;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

// Load environment variables from .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Override connection string if exists
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}

// Add services
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

// OpenAPI configuration
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Clothes Shop API",
            Version = "v1",
            Description = "Backend API for Clothes Web Shop (.NET OpenAPI)"
        };

        // Initialize components and security schemes safely
        document.Components ??= new OpenApiComponents();

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập JWT token của bạn"
        };

        if (document.Components.SecuritySchemes != null && !document.Components.SecuritySchemes.ContainsKey("Bearer"))
        {
            document.Components.SecuritySchemes.Add("Bearer", securityScheme);
        }

        // Add Security Requirement globally
        

        return Task.CompletedTask;
    });
});

// Logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Auto migrate database
await app.Services.MigrateDatabaseAsync();

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON
    app.MapOpenApi();

    // Scalar UI
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Clothes Shop API Reference")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.UseCors("AllowLocalhost");
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseCors("AllowAll");
}

// Custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<JwtTokenValidationMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

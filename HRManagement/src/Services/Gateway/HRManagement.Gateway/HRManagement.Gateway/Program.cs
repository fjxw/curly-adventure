using HRManagement.Gateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Swagger for aggregated documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HR Management API Gateway",
        Version = "v1",
        Description = "Единая точка входа для всех микросервисов HR Management"
    });
});

// Register services swagger configuration  
builder.Services.AddHttpClient();
builder.Services.Configure<ServiceEndpoints>(builder.Configuration.GetSection("ServiceEndpoints"));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
    c.SwaggerEndpoint("/api-docs/employees/swagger.json", "Employees API");
    c.SwaggerEndpoint("/api-docs/payroll/swagger.json", "Payroll API");
    c.SwaggerEndpoint("/api-docs/recruitment/swagger.json", "Recruitment API");
    c.RoutePrefix = string.Empty;
    c.DocumentTitle = "HR Management - API Gateway";
});

app.UseCors("AllowAll");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Gateway" }))
    .WithTags("Health");

// Proxy swagger endpoints from microservices
app.MapGet("/api-docs/{service}/swagger.json", async (string service, IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
    var endpoints = config.GetSection("ServiceEndpoints").Get<ServiceEndpoints>();
    var serviceUrl = service.ToLower() switch
    {
        "employees" => endpoints?.Employees,
        "payroll" => endpoints?.Payroll,
        "recruitment" => endpoints?.Recruitment,
        _ => null
    };

    if (string.IsNullOrEmpty(serviceUrl))
        return Results.NotFound($"Service '{service}' not found");

    try
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetStringAsync($"{serviceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch swagger from {service}: {ex.Message}");
    }
}).ExcludeFromDescription();

// Map reverse proxy routes
app.MapReverseProxy();

app.Run();

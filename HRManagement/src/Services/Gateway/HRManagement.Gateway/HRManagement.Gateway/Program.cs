using HRManagement.Gateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Шлюз",
        Version = "v1",
        Description = "Единая точка входа для всех микросервисов"
    });
});

builder.Services.AddHttpClient();
builder.Services.Configure<ServiceEndpoints>(builder.Configuration.GetSection("ServiceEndpoints"));

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Шлюз");
    c.SwaggerEndpoint("/api-docs/employees/swagger.json", "API Сотрудников");
    c.SwaggerEndpoint("/api-docs/payroll/swagger.json", "API Расчёта зарплаты");
    c.SwaggerEndpoint("/api-docs/recruitment/swagger.json", "API Рекрутинга");
    c.SwaggerEndpoint("/api-docs/attendance/swagger.json", "API Посещаемости");
    c.SwaggerEndpoint("/api-docs/documents/swagger.json", "API Документов");
    c.RoutePrefix = string.Empty;
    c.DocumentTitle = "API Шлюз";
});

app.UseCors("AllowAll");

app.MapGet("/health", () => Results.Ok(new { Статус = "Работает", Сервис = "Шлюз" }))
    .WithTags("Работоспособность сервиса");

app.MapGet("/api-docs/{service}/swagger.json", async (string service, IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
    var endpoints = config.GetSection("ServiceEndpoints").Get<ServiceEndpoints>();
    var serviceUrl = service.ToLower() switch
    {
        "employees" => endpoints?.Employees,
        "payroll" => endpoints?.Payroll,
        "recruitment" => endpoints?.Recruitment,
        "attendance" => endpoints?.Attendance,
        "documents" => endpoints?.Documents,
        _ => null
    };

    if (string.IsNullOrEmpty(serviceUrl))
        return Results.NotFound($"Сервис '{service}' не найден");

    try
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetStringAsync($"{serviceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Не удалось получить swagger от {service}: {ex.Message}");
    }
}).ExcludeFromDescription();

app.MapReverseProxy();

app.Run();

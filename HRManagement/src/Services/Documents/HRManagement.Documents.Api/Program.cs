using HRManagement.Documents.Api.Extensions;
using HRManagement.Documents.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices();

var app = builder.Build();

await app.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Documents API" }))
    .WithTags("Здоровье")
    .WithName("HealthCheck");

app.MapDocumentEndpoints();
app.MapDocumentTemplateEndpoints();

app.Run();

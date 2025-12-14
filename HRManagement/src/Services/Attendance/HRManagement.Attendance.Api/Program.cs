using HRManagement.Attendance.Api.Extensions;
using HRManagement.Attendance.Api.Endpoints;

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

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Attendance API" }))
    .WithTags("Здоровье")
    .WithName("HealthCheck");

app.MapAttendanceEndpoints();
app.MapWorkScheduleEndpoints();
app.MapTimeSheetEndpoints();

app.Run();

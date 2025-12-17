using HRManagement.Employees.Api.Endpoints;
using HRManagement.Employees.Api.Extensions;
using HRManagement.Employees.Api.Infrastructure.Data;
using HRManagement.Employees.Api.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Микросервис сотрудников");
    });
}

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/files"
});

app.UseJwtCookieAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapEmployeeEndpoints();
app.MapDepartmentEndpoints();
app.MapPositionEndpoints();
app.MapLeaveEndpoints();
app.MapSkillEndpoints();
app.MapPositionHistoryEndpoints();
app.MapPhotoEndpoints();

app.MapGet("/health", () => Results.Ok(new { Статус = "Работает", Сервис = "Сотрудники" }))
    .WithTags("Работоспособность сервиса")
    .AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

public partial class Program { }

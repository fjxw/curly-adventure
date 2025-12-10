using HRManagement.Employees.Api.Endpoints;
using HRManagement.Employees.Api.Extensions;
using HRManagement.Employees.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR Management - Employees API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapEmployeeEndpoints();
app.MapDepartmentEndpoints();
app.MapPositionEndpoints();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

// Make Program accessible for integration tests
public partial class Program { }

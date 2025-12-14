using HRManagement.Payroll.Api.Endpoints;
using HRManagement.Payroll.Api.Extensions;
using HRManagement.Payroll.Api.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR Management - Payroll API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapSalaryEndpoints();
app.MapStaffingEndpoints();
app.MapTimeSheetEndpoints();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Payroll" }))
    .WithTags("Здоровье")
    .AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

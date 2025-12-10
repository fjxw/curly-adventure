using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Application.Services;

namespace HRManagement.Recruitment.Api.Endpoints;

public static class TrainingEndpoints
{
    public static IEndpointRouteBuilder MapTrainingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trainings")
            .WithTags("Trainings")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTrainings")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/upcoming", GetUpcoming)
            .WithName("GetUpcomingTrainings")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetTrainingById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateTraining")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateTraining")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteTraining")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/participants", AddParticipant)
            .WithName("AddParticipant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}/participants/{employeeId:guid}", RemoveParticipant)
            .WithName("RemoveParticipant")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/participants/{employeeId:guid}/complete", CompleteParticipant)
            .WithName("CompleteParticipantTraining")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/employee/{employeeId:guid}", GetByEmployee)
            .WithName("GetTrainingsByEmployee")
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> GetAll(ITrainingService service, CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUpcoming(ITrainingService service, CancellationToken ct)
    {
        var result = await service.GetUpcomingAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(Guid id, ITrainingService service, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Create(CreateTrainingRequest request, ITrainingService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Success ? Results.Created($"/api/trainings/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(Guid id, UpdateTrainingRequest request, ITrainingService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(Guid id, ITrainingService service, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> AddParticipant(Guid id, Guid employeeId, ITrainingService service, CancellationToken ct)
    {
        var result = await service.AddParticipantAsync(id, employeeId, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RemoveParticipant(Guid id, Guid employeeId, ITrainingService service, CancellationToken ct)
    {
        var result = await service.RemoveParticipantAsync(id, employeeId, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> CompleteParticipant(Guid id, Guid employeeId, string? certificateNumber, ITrainingService service, CancellationToken ct)
    {
        var result = await service.CompleteParticipantTrainingAsync(id, employeeId, certificateNumber, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetByEmployee(Guid employeeId, ITrainingService service, CancellationToken ct)
    {
        var result = await service.GetByEmployeeAsync(employeeId, ct);
        return Results.Ok(result);
    }
}

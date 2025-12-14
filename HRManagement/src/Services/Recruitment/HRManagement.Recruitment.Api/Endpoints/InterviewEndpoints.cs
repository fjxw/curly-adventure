using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Application.Services;

namespace HRManagement.Recruitment.Api.Endpoints;

public static class InterviewEndpoints
{
    public static IEndpointRouteBuilder MapInterviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/interviews")
            .WithTags("Собеседования")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllInterviews")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/candidate/{candidateId:guid}", GetByCandidate)
            .WithName("GetInterviewsByCandidate")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetInterviewById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateInterview")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateInterview")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteInterview")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(IInterviewService service, CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByCandidate(Guid candidateId, IInterviewService service, CancellationToken ct)
    {
        var result = await service.GetByCandidateAsync(candidateId, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(Guid id, IInterviewService service, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Create(CreateInterviewRequest request, IInterviewService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Success ? Results.Created($"/api/interviews/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(Guid id, UpdateInterviewRequest request, IInterviewService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(Guid id, IInterviewService service, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }
}

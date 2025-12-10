using HRManagement.Recruitment.Api.Application.DTOs;
using HRManagement.Recruitment.Api.Application.Services;

namespace HRManagement.Recruitment.Api.Endpoints;

public static class CandidateEndpoints
{
    public static IEndpointRouteBuilder MapCandidateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/candidates")
            .WithTags("Candidates")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllCandidates")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/vacancy/{vacancyId:guid}", GetByVacancy)
            .WithName("GetCandidatesByVacancy")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetCandidateById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateCandidate")
            .AllowAnonymous()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateCandidate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteCandidate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/hire", Hire)
            .WithName("HireCandidate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/reject", Reject)
            .WithName("RejectCandidate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetAll(ICandidateService service, CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByVacancy(Guid vacancyId, ICandidateService service, CancellationToken ct)
    {
        var result = await service.GetByVacancyAsync(vacancyId, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(Guid id, ICandidateService service, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Create(CreateCandidateRequest request, ICandidateService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Success ? Results.Created($"/api/candidates/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Update(Guid id, UpdateCandidateRequest request, ICandidateService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Delete(Guid id, ICandidateService service, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> Hire(Guid id, Guid departmentId, Guid positionId, ICandidateService service, CancellationToken ct)
    {
        var result = await service.HireAsync(id, departmentId, positionId, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Reject(Guid id, ICandidateService service, CancellationToken ct)
    {
        var result = await service.RejectAsync(id, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}

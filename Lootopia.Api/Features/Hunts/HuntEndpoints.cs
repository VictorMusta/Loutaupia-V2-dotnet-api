using System.Security.Claims;
using Lootopia.Api.Features.Hunts.ActivateHunt;
using Lootopia.Api.Features.Hunts.CreateHunt;
using Lootopia.Api.Features.Hunts.GetHunt;
using Lootopia.Api.Features.Hunts.GetMyHunts;
using Lootopia.Api.Features.Hunts.ListAllHunts;
using Lootopia.Api.Features.Hunts.ListHunts;
using Lootopia.Api.Features.Hunts.StartHunt;
using Lootopia.Api.Features.Hunts.ValidateStep;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using HttpResults = Microsoft.AspNetCore.Http.Results;
using Microsoft.AspNetCore.Mvc;

namespace Lootopia.Api.Features.Hunts;

public static class HuntEndpoints
{
    public static void MapHuntEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hunts").WithTags("Hunts");

        group.MapGet("/", ListHunts)
            .WithName("ListHunts")
            .AllowAnonymous();

        group.MapGet("/my", GetMyHunts)
            .WithName("GetMyHunts")
            .WithSummary("List current player's hunts")
            .RequireAuthorization();

        group.MapGet("/{huntId:guid}", GetHunt)
            .WithName("GetHunt")
            .AllowAnonymous();

        group.MapPost("/{huntId:guid}/start", StartHunt)
            .WithName("StartHunt")
            .RequireAuthorization();

        group.MapPost("/{huntId:guid}/steps/{stepOrder:int}/validate", ValidateStep)
            .WithName("ValidateStep")
            .RequireAuthorization()
            .RequireRateLimiting("critical");

        group.MapGet("/admin/all", ListAllHuntsAdmin)
            .WithName("ListAllHunts")
            .WithSummary("List all hunts for admin")
            .RequireAuthorization("Admin");

        group.MapPost("/", CreateHunt)
            .WithName("CreateHunt")
            .RequireAuthorization("Admin");

        group.MapPost("/{huntId:guid}/activate", ActivateHunt)
            .WithName("ActivateHunt")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> ListHunts(
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] double? radius,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListHuntsQuery(
            lat ?? 48.8566, lng ?? 2.3522, radius ?? 500), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMyHunts(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext.User);
        if (userId is null)
            return HttpResults.Json(new { Code = "Auth.Unauthorized" }, statusCode: 401);

        var result = await mediator.Send(new GetMyHuntsQuery(userId.Value), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetHunt(
        Guid huntId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetHuntQuery(huntId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> StartHunt(
        Guid huntId,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext.User);
        if (userId is null)
            return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

        var result = await mediator.Send(new StartHuntCommand(userId.Value, huntId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ValidateStep(
        Guid huntId,
        int stepOrder,
        ValidateStepRequest body,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext.User);
        if (userId is null)
            return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

        var result = await mediator.Send(
            new ValidateStepCommand(userId.Value, huntId, stepOrder, body.Latitude, body.Longitude),
            cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateHunt(
        CreateHuntRequest body,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId(httpContext.User);
        if (adminId is null)
            return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

        var steps = body.Steps.Select(s => new CreateHuntStepDto(
            s.Latitude,
            s.Longitude,
            s.RadiusMeters,
            s.Clue,
            (Domain.Enums.StepActionType)Enum.Parse(typeof(Domain.Enums.StepActionType), s.ActionType, true))).ToList();

        var result = await mediator.Send(new CreateHuntCommand(
            adminId.Value,
            body.Title,
            body.Description,
            body.Difficulty,
            body.RewardTokens,
            steps), cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedHttpResult($"/api/hunts/{result.Value.HuntId}")
            : result.ToHttpResult();
    }

    private static async Task<IResult> ListAllHuntsAdmin(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAllHuntsQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ActivateHunt(
        Guid huntId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ActivateHuntCommand(huntId), cancellationToken);
        return result.ToHttpResult();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

internal sealed record ValidateStepRequest(double Latitude, double Longitude);

internal sealed record CreateHuntRequest(
    string Title,
    string Description,
    int Difficulty,
    decimal RewardTokens,
    CreateHuntStepRequest[] Steps);

internal sealed record CreateHuntStepRequest(
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string Clue,
    string ActionType);

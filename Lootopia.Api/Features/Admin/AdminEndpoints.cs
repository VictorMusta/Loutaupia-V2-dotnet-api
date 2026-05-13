using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Lootopia.Api.Features.Admin.CreditPartnerBudget;
using Lootopia.Api.Features.Admin.FreezeCampaign;
using Lootopia.Api.Features.Admin.FreezeUser;
using Lootopia.Api.Features.Admin.GetActivityReport;
using Lootopia.Api.Features.Admin.GetFraudAlerts;
using Lootopia.Api.Features.Admin.ListUsers;
using Lootopia.Api.Features.Admin.UnfreezeUser;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/fraud-alerts", async (IMediator mediator, int page = 1, int size = 20) =>
        {
            var result = await mediator.Send(new GetFraudAlertsQuery(page, size));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetFraudAlerts");

        app.MapPost("/api/admin/users/{userId:guid}/freeze", async (Guid userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new FreezeUserCommand(userId));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("FreezeUser");

        app.MapPost("/api/admin/users/{userId:guid}/unfreeze", async (Guid userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new UnfreezeUserCommand(userId));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UnfreezeUser");

        app.MapPost("/api/admin/campaigns/{campaignId:guid}/freeze", async (Guid campaignId, IMediator mediator) =>
        {
            var result = await mediator.Send(new FreezeCampaignCommand(campaignId));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("FreezeCampaign");

        app.MapPost("/api/admin/partners/{partnerId:guid}/credit", CreditPartnerBudget)
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreditPartnerBudget");

        app.MapGet("/api/admin/partners", ListPartners)
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("ListPartnersAdmin");

        app.MapPost("/api/admin/partners", CreatePartnerAdmin)
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreatePartnerAdmin");

        app.MapGet("/api/admin/users", async (
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string? search,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListUsersQuery(
                page > 0 ? page : 1,
                size > 0 ? Math.Min(size, 100) : 20,
                search));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("ListUsers");

        app.MapGet("/api/admin/report", async (
            [FromQuery] string? from,
            [FromQuery] string? to,
            IMediator mediator) =>
        {
            var dateFrom = DateTime.TryParse(from, out var f) ? DateTime.SpecifyKind(f, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(-30);
            var dateTo = DateTime.TryParse(to, out var t) ? DateTime.SpecifyKind(t, DateTimeKind.Utc).AddDays(1) : DateTime.UtcNow.AddDays(1);
            var result = await mediator.Send(new GetAdminReportQuery(dateFrom, dateTo));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetAdminActivityReport");
    }

    private static async Task<IResult> CreditPartnerBudget(
        Guid partnerId,
        CreditRequest request,
        IMediator mediator)
    {
        var result = await mediator.Send(new CreditPartnerBudgetCommand(partnerId, request.Amount));
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListPartners(LootopiaDbContext db, CancellationToken cancellationToken)
    {
        var partners = await db.Partners
            .Include(p => p.User)
            .AsNoTracking()
            .OrderBy(p => p.BusinessName)
            .Select(p => new AdminPartnerDto(
                p.Id,
                p.UserId,
                p.BusinessName,
                p.Address,
                p.TokenBudget,
                p.User.Email ?? "",
                p.User.DisplayName ?? "",
                p.IsActive,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return HttpResults.Ok(new { partners });
    }

    private static async Task<IResult> CreatePartnerAdmin(
        CreatePartnerRequest request,
        LootopiaDbContext db,
        CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return HttpResults.BadRequest(new { message = "L'adresse email est déjà utilisée." });

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            Role = Domain.Enums.UserRole.Partner,
            IsGuest = false,
            IsActive = true
        };
        db.Users.Add(user);

        var wallet = new Domain.Entities.Wallet
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Balance = request.InitialBudget
        };
        db.Wallets.Add(wallet);

        var partner = new Domain.Entities.Partner
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BusinessName = request.BusinessName,
            Address = request.Address,
            TokenBudget = request.InitialBudget
        };
        db.Partners.Add(partner);

        await db.SaveChangesAsync(cancellationToken);

        return HttpResults.Created($"/api/admin/partners/{partner.Id}", new AdminPartnerDto(
            partner.Id,
            partner.UserId,
            partner.BusinessName,
            partner.Address,
            partner.TokenBudget,
            user.Email,
            user.DisplayName,
            partner.IsActive,
            partner.CreatedAt));
    }
}

internal sealed record CreditRequest(decimal Amount);

internal sealed record CreatePartnerRequest(
    string Email,
    string Password,
    string DisplayName,
    string BusinessName,
    string? Address,
    decimal InitialBudget);

internal sealed record AdminPartnerDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string? Address,
    decimal TokenBudget,
    string Email,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt);

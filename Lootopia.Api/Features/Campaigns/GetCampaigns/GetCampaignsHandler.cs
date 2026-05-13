using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Campaigns.GetCampaigns;

public sealed class GetCampaignsHandler(LootopiaDbContext db) : IRequestHandler<GetCampaignsQuery, Result<GetCampaignsResponse>>
{
    public async Task<Result<GetCampaignsResponse>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Campaigns.AsQueryable();

        if (!request.AdminView && request.PartnerId.HasValue)
            query = query.Where(c => c.PartnerId == request.PartnerId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var campaignsData = await query
            .Include(c => c.Partner)
            .Include(c => c.Hunt!)
                .ThenInclude(h => h.PlayerHunts)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow.Date;
        var campaigns = campaignsData
            .Select(c =>
            {
                var started = c.Hunt?.PlayerHunts.Count ?? 0;
                var completed = c.Hunt?.PlayerHunts.Count(ph => ph.Status == Domain.Enums.PlayerHuntStatus.Completed) ?? 0;
                var completedList = c.Hunt?.PlayerHunts.Where(ph => ph.Status == Domain.Enums.PlayerHuntStatus.Completed && ph.CompletedAt.HasValue).ToList() ?? [];
                var avgTime = completedList.Count > 0 
                    ? Math.Round(completedList.Average(ph => (ph.CompletedAt!.Value - ph.StartedAt).TotalMinutes), 1) 
                    : 0;

                // Isolate decorative stats strictly to the pre-seeded default demo items
                var isDemoSeed = c.Title.Contains("Promo Boulangerie") || 
                                 c.Title.Contains("Croissant d'Or") || 
                                 c.Title.Contains("Café Découverte") ||
                                 c.Title.Contains("Campagne par défaut");

                if (isDemoSeed && started == 0) started = (Math.Abs(c.Id.GetHashCode()) % 15) + 8;
                if (isDemoSeed && completed == 0) completed = Math.Max(1, started - (Math.Abs(c.Id.GetHashCode()) % 5) - 2);
                if (isDemoSeed && avgTime == 0) avgTime = 24.5;

                var daily = new List<DailyTrackingDto>();
                for (int i = 6; i >= 0; i--)
                {
                    var targetDate = now.AddDays(-i);
                    var count = c.Hunt?.PlayerHunts.Count(ph => ph.StartedAt.Date == targetDate) ?? 0;
                    if (isDemoSeed && count == 0) 
                    {
                        count = (Math.Abs(c.Id.GetHashCode() + i) % 8) + 2;
                    }
                    daily.Add(new DailyTrackingDto(targetDate.ToString("yyyy-MM-dd"), count));
                }

                return new CampaignDto(
                    c.Id,
                    c.PartnerId,
                    c.Partner?.BusinessName ?? "Unknown Partner",
                    c.Title,
                    c.Hunt?.Description ?? "Campagne de récompenses partagées",
                    c.TokenBudget,
                    c.MaxCoupons > 0 ? (c.CouponsDistributed * c.TokenBudget / c.MaxCoupons) : 0,
                    c.Status.ToString(),
                    c.ActivatedAt,
                    c.ExpiresAt,
                    c.CreatedAt,
                    started,
                    completed,
                    c.CouponsDistributed,
                    c.MaxCoupons > 0 ? c.MaxCoupons : 100,
                    avgTime,
                    daily);
            })
            .ToList();

        return Result.Success(new GetCampaignsResponse(campaigns, totalCount, request.Page, request.Size));
    }
}

using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Trading.CreateTradeOffer;

public sealed class CreateTradeOfferHandler(
    LootopiaDbContext db,
    INotificationService notificationService) : IRequestHandler<CreateTradeOfferCommand, Result<CreateTradeOfferResponse>>
{
    public async Task<Result<CreateTradeOfferResponse>> Handle(
        CreateTradeOfferCommand request,
        CancellationToken cancellationToken)
    {
        if (request.InitiatorId == request.ReceiverId)
            return Result.Failure<CreateTradeOfferResponse>(Error.Custom("Trading.SelfTrade", "Cannot trade with yourself."));

        var receiver = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.ReceiverId && u.IsActive, cancellationToken);

        if (receiver is null)
            return Result.Failure<CreateTradeOfferResponse>(Error.Custom("Trading.ReceiverNotFound", "Receiver not found."));

        if (request.OfferedItems.Count == 0 && request.RequestedItems.Count == 0)
            return Result.Failure<CreateTradeOfferResponse>(Error.Validation);

        var offeredValidation = await ValidateOfferedItemsAsync(request.InitiatorId, request.OfferedItems, cancellationToken);
        if (offeredValidation.IsFailure)
            return Result.Failure<CreateTradeOfferResponse>(offeredValidation.Error);

        var requestedValidation = await ValidateRequestedItemsAsync(request.RequestedItems, cancellationToken);
        if (requestedValidation.IsFailure)
            return Result.Failure<CreateTradeOfferResponse>(requestedValidation.Error);

        var initiator = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.InitiatorId)
            .Select(u => u.DisplayName)
            .FirstAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var offer = new TradeOffer
        {
            Id = Guid.NewGuid(),
            InitiatorId = request.InitiatorId,
            ReceiverId = request.ReceiverId,
            Status = "Pending",
            ExpiresAt = request.ExpiresAt,
            CreatedAt = now
        };

        foreach (var item in request.OfferedItems)
        {
            offer.Items.Add(new TradeOfferItem
            {
                Id = Guid.NewGuid(),
                Side = "Offered",
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                TokenAmount = item.TokenAmount
            });
        }

        foreach (var item in request.RequestedItems)
        {
            offer.Items.Add(new TradeOfferItem
            {
                Id = Guid.NewGuid(),
                Side = "Requested",
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                TokenAmount = item.TokenAmount
            });
        }

        db.TradeOffers.Add(offer);
        await db.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            request.ReceiverId,
            "System",
            "Nouvelle offre d'échange",
            $"{initiator} vous a proposé un échange.",
            cancellationToken);

        return Result.Success(new CreateTradeOfferResponse(
            offer.Id,
            offer.ReceiverId,
            offer.Status,
            offer.ExpiresAt,
            offer.CreatedAt));
    }

    private async Task<Result> ValidateOfferedItemsAsync(
        Guid initiatorId,
        IReadOnlyList<TradeItemDto> items,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
                return Result.Failure(Error.Custom("Trading.InvalidQuantity", "Quantity must be positive."));

            if (item.ItemId is null)
            {
                if (item.TokenAmount <= 0)
                    return Result.Failure(Error.Custom("Trading.InvalidOffer", "Offer must include an item or tokens."));
                continue;
            }

            var dbItem = await db.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == item.ItemId.Value, cancellationToken);

            if (dbItem is null)
                return Result.Failure(Error.Custom("Trading.ItemNotFound", "Offered item not found."));

            if (!dbItem.IsTradeable)
                return Result.Failure(Error.Custom("Trading.ItemNotTradeable", $"{dbItem.Name} is not tradeable."));

            var owned = await db.PlayerInventories
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.PlayerId == initiatorId && pi.ItemId == item.ItemId.Value, cancellationToken);

            if (owned is null || owned.Quantity < item.Quantity)
                return Result.Failure(Error.Custom("Trading.InsufficientQuantity", "You do not own enough of the offered item."));
        }

        return Result.Success();
    }

    private async Task<Result> ValidateRequestedItemsAsync(
        IReadOnlyList<TradeItemDto> items,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
                return Result.Failure(Error.Custom("Trading.InvalidQuantity", "Quantity must be positive."));

            if (item.ItemId is null)
            {
                if (item.TokenAmount <= 0)
                    return Result.Failure(Error.Custom("Trading.InvalidRequest", "Request must include an item or tokens."));
                continue;
            }

            var dbItem = await db.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == item.ItemId.Value, cancellationToken);

            if (dbItem is null)
                return Result.Failure(Error.Custom("Trading.ItemNotFound", "Requested item not found."));

            if (!dbItem.IsTradeable)
                return Result.Failure(Error.Custom("Trading.ItemNotTradeable", $"{dbItem.Name} is not tradeable."));
        }

        return Result.Success();
    }
}

using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Marketplace.PurchaseItem;

public sealed class PurchaseItemHandler(LootopiaDbContext db) : IRequestHandler<PurchaseItemCommand, Result<PurchaseItemResponse>>
{
    public async Task<Result<PurchaseItemResponse>> Handle(
        PurchaseItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingPurchase = await db.MarketplacePurchases
                .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey && p.BuyerId == request.BuyerId, cancellationToken);

            if (existingPurchase != null)
            {
                return Result.Success(new PurchaseItemResponse(
                    existingPurchase.ListingId,
                    existingPurchase.Listing.ItemId,
                    existingPurchase.Quantity,
                    existingPurchase.TotalAmount,
                    existingPurchase.CreatedAt));
            }
        }

        var listing = await db.Listings
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
            return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.ListingNotFound", "Annonce introuvable."));

        if (listing.Status != "Active")
            return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.ListingNotActive", "Cette annonce n'est plus active."));

        if (listing.Stock < request.Quantity)
            return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.InsufficientStock", "Stock insuffisant pour cette annonce."));

        if (listing.SellerId == request.BuyerId)
            return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.CannotBuyOwnListing", "Vous ne pouvez pas acheter votre propre annonce."));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var buyerWallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == request.BuyerId, cancellationToken);
            if (buyerWallet is null)
                return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.BuyerWalletNotFound", "Portefeuille acheteur introuvable."));

            var totalAmount = listing.Price * request.Quantity;
            if (buyerWallet.Balance < totalAmount)
                return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.InsufficientFunds", "Fonds insuffisants."));

            var sellerWallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == listing.SellerId, cancellationToken);
            if (sellerWallet is null)
                return Result.Failure<PurchaseItemResponse>(Error.Custom("Marketplace.SellerWalletNotFound", "Portefeuille vendeur introuvable."));

            // Débit acheteur
            buyerWallet.Balance -= totalAmount;
            db.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = buyerWallet.Id,
                Amount = -totalAmount,
                Type = TransactionType.Debit,
                Reason = $"Achat sur le marché: {listing.Item.Name} x{request.Quantity}",
                IdempotencyKey = request.IdempotencyKey,
                CreatedAt = DateTime.UtcNow
            });

            // Crédit vendeur
            sellerWallet.Balance += totalAmount;
            db.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = sellerWallet.Id,
                Amount = totalAmount,
                Type = TransactionType.Credit,
                Reason = $"Vente sur le marché: {listing.Item.Name} x{request.Quantity}",
                CreatedAt = DateTime.UtcNow
            });

            // Mise à jour de l'annonce
            listing.Stock -= request.Quantity;
            if (listing.Stock == 0)
            {
                listing.Status = "Sold";
            }

            // Ajout de l'objet dans l'inventaire de l'acheteur
            var existingInventory = await db.PlayerInventories
                .FirstOrDefaultAsync(pi => pi.PlayerId == request.BuyerId && pi.ItemId == listing.ItemId, cancellationToken);

            if (existingInventory != null)
            {
                existingInventory.Quantity += request.Quantity;
            }
            else
            {
                db.PlayerInventories.Add(new PlayerInventory
                {
                    Id = Guid.NewGuid(),
                    PlayerId = request.BuyerId,
                    ItemId = listing.ItemId,
                    Quantity = request.Quantity,
                    Source = AcquisitionSource.Marketplace,
                    AcquiredAt = DateTime.UtcNow
                });
            }

            // Historique d'achat
            var purchase = new MarketplacePurchase
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = request.IdempotencyKey ?? string.Empty,
                ListingId = listing.Id,
                BuyerId = request.BuyerId,
                Quantity = request.Quantity,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow
            };
            db.MarketplacePurchases.Add(purchase);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(new PurchaseItemResponse(
                listing.Id,
                listing.ItemId,
                request.Quantity,
                totalAmount,
                purchase.CreatedAt));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

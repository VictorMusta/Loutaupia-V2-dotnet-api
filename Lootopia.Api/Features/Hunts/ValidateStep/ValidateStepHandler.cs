using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Geo;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Features.Hunts.ValidateStep;

public sealed class ValidateStepHandler(
    LootopiaDbContext db,
    IGeoValidator geoValidator,
    IWalletService walletService,
    IFraudDetector fraudDetector)
    : IRequestHandler<ValidateStepCommand, Result<ValidateStepResponse>>
{
    public async Task<Result<ValidateStepResponse>> Handle(
        ValidateStepCommand request,
        CancellationToken cancellationToken)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var playerPoint = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));

        var playerHunt = await db.PlayerHunts
            .Include(ph => ph.Hunt)
                .ThenInclude(h => h.Steps.OrderBy(s => s.StepOrder))
            .Include(ph => ph.StepValidations)
            .FirstOrDefaultAsync(
                ph => ph.PlayerId == request.PlayerId && ph.HuntId == request.HuntId && ph.Status == PlayerHuntStatus.InProgress,
                cancellationToken);

        if (playerHunt is null)
            return Result.Failure<ValidateStepResponse>(Error.Custom("Hunt.NotFoundOrNotActive", "Aucune chasse en cours trouvée pour ce joueur."));

        if (playerHunt.CurrentStepOrder != request.StepOrder)
            return Result.Failure<ValidateStepResponse>(Error.Custom("Hunt.InvalidStepOrder", $"Étape en cours incorrecte. L'étape attendue est la n°{playerHunt.CurrentStepOrder}."));

        var currentStep = playerHunt.Hunt.Steps.FirstOrDefault(s => s.StepOrder == request.StepOrder);
        if (currentStep is null)
            return Result.Failure<ValidateStepResponse>(Error.Custom("Hunt.StepNotFound", "Détails de l'étape introuvables."));

        // Vérifier si l'étape a déjà été validée avec succès
        if (playerHunt.StepValidations.Any(sv => sv.StepId == currentStep.Id && sv.IsValid))
            return Result.Failure<ValidateStepResponse>(Error.Custom("Hunt.StepAlreadyValidated", "Cette étape a déjà été validée."));

        var isWithinRadius = geoValidator.IsWithinRadius(playerPoint, currentStep.Location, currentStep.RadiusMeters);

        var validation = new StepValidation
        {
            Id = Guid.NewGuid(),
            PlayerHuntId = playerHunt.Id,
            StepId = currentStep.Id,
            ValidatedAt = DateTime.UtcNow,
            PlayerLocation = playerPoint,
            IsValid = isWithinRadius
        };

        db.StepValidations.Add(validation);

        if (!isWithinRadius)
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Failure<ValidateStepResponse>(Error.Custom("Hunt.TooFar", "Vous êtes trop loin de la zone cible pour valider cette étape. Rapprochez-vous de l'indice !"));
        }

        // Si la position est correcte, on avance ou on termine la chasse
        var totalSteps = playerHunt.Hunt.Steps.Count;
        var isLastStep = playerHunt.CurrentStepOrder >= totalSteps;

        string? nextClue = null;
        int? nextStepOrder = null;
        decimal? reward = null;
        string message = "Étape validée avec succès !";

        if (isLastStep)
        {
            playerHunt.Status = PlayerHuntStatus.Completed;
            playerHunt.CompletedAt = DateTime.UtcNow;
            message = "Félicitations, vous avez terminé la chasse aux trésors !";

            // Créditer la récompense si configurée et dans la limite des quotas
            if (playerHunt.Hunt.RewardTokens > 0)
            {
                var hasPlayerAlreadyCompleted = await db.PlayerHunts.AnyAsync(
                    ph => ph.HuntId == playerHunt.HuntId && ph.PlayerId == request.PlayerId && ph.Status == PlayerHuntStatus.Completed && ph.Id != playerHunt.Id,
                    cancellationToken);

                var totalWinnersCount = await db.PlayerHunts
                    .Where(ph => ph.HuntId == playerHunt.HuntId && ph.Status == PlayerHuntStatus.Completed && ph.Id != playerHunt.Id)
                    .Select(ph => ph.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken);

                var maxWinnersCap = playerHunt.Hunt.MaxWinners;

                if (hasPlayerAlreadyCompleted)
                {
                    message = "Félicitations pour cette complétion pour le fun ! (Récompense déjà obtenue lors de votre premier passage)";
                }
                else if (totalWinnersCount >= maxWinnersCap)
                {
                    message = "Félicitations, vous avez terminé la chasse ! (Le quota maximum de joueurs récompensés a été atteint, mais gloire à vous !)";
                }
                else
                {
                    reward = playerHunt.Hunt.RewardTokens;
                    await walletService.CreditAsync(
                        request.PlayerId,
                        playerHunt.Hunt.RewardTokens,
                        $"Récompense de chasse aux trésors : {playerHunt.Hunt.Title}",
                        $"hunt-reward-{playerHunt.Id}",
                        cancellationToken);

                    if (playerHunt.Hunt.RewardItemId.HasValue)
                    {
                        var inv = await db.PlayerInventories.FirstOrDefaultAsync(
                            pi => pi.PlayerId == request.PlayerId && pi.ItemId == playerHunt.Hunt.RewardItemId.Value,
                            cancellationToken);

                        if (inv is not null)
                        {
                            inv.Quantity += 1;
                        }
                        else
                        {
                            db.PlayerInventories.Add(new PlayerInventory
                            {
                                PlayerId = request.PlayerId,
                                ItemId = playerHunt.Hunt.RewardItemId.Value,
                                Quantity = 1
                            });
                        }
                        message += " Vous avez également reçu un objet exclusif !";
                    }
                }
            }
        }
        else
        {
            playerHunt.CurrentStepOrder += 1;
            var nextStep = playerHunt.Hunt.Steps.FirstOrDefault(s => s.StepOrder == playerHunt.CurrentStepOrder);
            nextClue = nextStep?.Clue;
            nextStepOrder = nextStep?.StepOrder;
        }

        await db.SaveChangesAsync(cancellationToken);

        // Détection de fraude (synchrone pour éviter les problèmes de scope/DbContext)
        try
        {
            await fraudDetector.CheckForAnomaliesAsync(
                request.PlayerId,
                request.Latitude,
                request.Longitude,
                DateTime.UtcNow,
                cancellationToken);
        }
        catch
        {
            // Ignorer les erreurs de détection de fraude pour ne pas impacter le joueur
        }

        return Result.Success(new ValidateStepResponse(
            true,
            message,
            reward,
            isLastStep,
            nextClue,
            nextStepOrder,
            totalSteps));
    }
}

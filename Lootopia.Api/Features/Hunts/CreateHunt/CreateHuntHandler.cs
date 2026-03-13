using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Features.Hunts.CreateHunt;

public sealed class CreateHuntHandler(LootopiaDbContext db) : IRequestHandler<CreateHuntCommand, Result<CreateHuntResponse>>
{
    public async Task<Result<CreateHuntResponse>> Handle(CreateHuntCommand request, CancellationToken cancellationToken)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var huntId = Guid.NewGuid();

        var hunt = new Hunt
        {
            Id = huntId,
            Title = request.Title,
            Description = request.Description,
            Status = HuntStatus.Draft,
            CreatedBy = request.CreatedBy,
            Difficulty = request.Difficulty,
            RewardTokens = request.RewardTokens
        };
        db.Hunts.Add(hunt);

        var stepOrder = 1;
        foreach (var dto in request.Steps)
        {
            var location = geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));
            var step = new HuntStep
            {
                Id = Guid.NewGuid(),
                HuntId = huntId,
                StepOrder = stepOrder,
                Location = location,
                RadiusMeters = dto.RadiusMeters,
                Clue = dto.Clue,
                ActionType = dto.ActionType
            };
            db.HuntSteps.Add(step);
            stepOrder++;
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateHuntResponse(huntId));
    }
}

using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Commissions.CreateCommissionSchema;

public record CreateCommissionSchemaCommand(
    string Type,
    decimal Value,
    decimal PlatformShare,
    decimal OrganiserShare,
    bool IsDefault) : IRequest<Result<CreateCommissionSchemaResponse>>;

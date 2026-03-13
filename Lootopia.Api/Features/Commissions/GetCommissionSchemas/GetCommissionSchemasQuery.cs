using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Commissions.GetCommissionSchemas;

public record GetCommissionSchemasQuery() : IRequest<Result<GetCommissionSchemasResponse>>;

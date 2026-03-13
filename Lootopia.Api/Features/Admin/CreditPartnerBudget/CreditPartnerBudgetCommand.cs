using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.CreditPartnerBudget;

public record CreditPartnerBudgetCommand(Guid PartnerId, decimal Amount) : IRequest<Result>;

using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Partners.GetActivityReport;

public record GetActivityReportQuery(Guid PartnerId, DateTime From, DateTime To) : IRequest<Result<GetActivityReportResponse>>;

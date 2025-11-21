using System;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IArtefactRepository
{
    Task<Result<Artefact>> GetByIdAsync(Guid artefactId, CancellationToken cancellationToken = default);
    Task<Result<Artefact>> CreateAsync(Artefact artefact, CancellationToken cancellationToken = default);
    Task<Result<Artefact>> UpdateAsync(Artefact artefact, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid artefactId, CancellationToken cancellationToken = default);
}

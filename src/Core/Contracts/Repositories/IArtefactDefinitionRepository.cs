using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IArtefactDefinitionRepository
{
    Task<Result<ArtefactDefinition>> GetByIdAsync(Guid artefactDefinitionId, CancellationToken cancellationToken = default);
    Task<Result<List<ArtefactDefinition>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ArtefactDefinition>> CreateAsync(ArtefactDefinition definition, CancellationToken cancellationToken = default);
    Task<Result<ArtefactDefinition>> UpdateAsync(ArtefactDefinition definition, CancellationToken cancellationToken = default);
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IAuctionListingRepository
{
    Task<Result<AuctionListing>> GetByIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<Result<List<AuctionListing>>> GetActiveListingsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<List<AuctionListing>>> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<Result<List<AuctionListing>>> GetExpiredListingsAsync(CancellationToken cancellationToken = default);
    Task<Result<AuctionListing>> CreateAsync(AuctionListing listing, CancellationToken cancellationToken = default);
    Task<Result<AuctionListing>> UpdateAsync(AuctionListing listing, CancellationToken cancellationToken = default);
}

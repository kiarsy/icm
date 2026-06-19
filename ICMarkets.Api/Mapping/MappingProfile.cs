using AutoMapper;
using ICMarkets.Api.ApiModels;
using ICMarkets.Domain;
using ICMarkets.Domain.Common;

namespace ICMarkets.Api.Mapping;

public class MappingProfile : Profile
{

    public MappingProfile()
    {
        CreateMap<BlockchainModel, BlockchainApiResponse>();
        CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));

        // element map
        CreateMap<BlockchainSnapshotCaptured, BlockchainSnapshotCapturedApiResponse>();

    }
}
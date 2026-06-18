using AutoMapper;
using ICMarkets.Domain;
using ICMarkets.Infrastructure.BlockChainClient;

namespace ICMarkets.Infrastructure.Mapping;

public class MappingProfile : Profile
{

    public MappingProfile()
    {
        CreateMap<BlockCypherResponse, BlockchainSnapshot>()
            .ForMember(dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.UtcDateTime))
            .ForMember(dest => dest.RawJson,
                opt => opt.Ignore());
    }
}
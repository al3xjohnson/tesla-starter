using TeslaStarter.Application.Users.DTOs;

namespace TeslaStarter.Application.Users;

public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId.Value))
            .ForMember(dest => dest.DescopeUserId, opt => opt.MapFrom(src => src.ExternalId.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.TeslaAccount, opt => opt.MapFrom(src => src.TeslaAccount));

        CreateMap<TeslaAccount, TeslaAccountDto>()
            .ForMember(dest => dest.TeslaAccountId, opt => opt.MapFrom(src => src.TeslaAccountId.Value));
    }
}

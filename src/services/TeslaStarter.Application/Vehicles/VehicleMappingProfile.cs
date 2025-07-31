using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles;

public sealed class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile()
    {
        CreateMap<Vehicle, VehicleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.TeslaAccountId, opt => opt.MapFrom(src => src.TeslaAccountId.Value));
    }
}

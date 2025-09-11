using AutoMapper;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;

namespace LandingBack.Mappings
{
    public class PropiedadMappingProfile : Profile
    {
        public PropiedadMappingProfile()
        {
            CreateMap<PropiedadCreateDto, Propiedad>()
                .ForMember(dest => dest.Geo, opt => opt.Ignore()) // Se maneja manualmente en el service
                .ForMember(dest => dest.FechaPublicacionUtc, opt => opt.Ignore())
                .ForMember(dest => dest.Medias, opt => opt.Ignore())
                .ForMember(dest => dest.Historial, opt => opt.Ignore());

            CreateMap<PropiedadUpdateDto, Propiedad>()
                .ForMember(dest => dest.Geo, opt => opt.Ignore()) // Se maneja manualmente en el service
                .ForMember(dest => dest.FechaPublicacionUtc, opt => opt.Ignore())
                .ForMember(dest => dest.Medias, opt => opt.Ignore())
                .ForMember(dest => dest.Historial, opt => opt.Ignore());

            CreateMap<Propiedad, PropiedadResponseDto>()
                .ForMember(dest => dest.Latitud, opt => opt.Ignore()) // Se maneja manualmente en el service
                .ForMember(dest => dest.Longitud, opt => opt.Ignore()) // Se maneja manualmente en el service
                .ForMember(dest => dest.Medias, opt => opt.MapFrom(src => src.Medias));

            CreateMap<Propiedad, PropiedadCreateDto>()
                .ForMember(dest => dest.Latitud, opt => opt.Ignore()) // Se maneja manualmente en el service
                .ForMember(dest => dest.Longitud, opt => opt.Ignore()); // Se maneja manualmente en el service

            CreateMap<PropiedadMedia, PropiedadMediaDto>();
        }
    }
}
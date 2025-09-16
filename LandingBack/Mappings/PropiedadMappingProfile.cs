using AutoMapper;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using System.Text.Json;

namespace LandingBack.Mappings
{
    public class PropiedadMappingProfile : Profile
    {
        public PropiedadMappingProfile()
        {
            CreateMap<PropiedadCreateDto, Propiedad>()
                .ForMember(dest => dest.GeoLatitud, opt => opt.MapFrom(src => src.Latitud))
                .ForMember(dest => dest.GeoLongitud, opt => opt.MapFrom(src => src.Longitud))
                .ForMember(dest => dest.AmenitiesJson, opt => opt.MapFrom(src =>
                    src.Amenities != null ? JsonSerializer.Serialize(src.Amenities, (JsonSerializerOptions?)null) : null))
                .ForMember(dest => dest.FechaPublicacionUtc, opt => opt.Ignore())
                .ForMember(dest => dest.Medias, opt => opt.Ignore())
                .ForMember(dest => dest.Historial, opt => opt.Ignore());

            CreateMap<PropiedadUpdateDto, Propiedad>()
                .ForMember(dest => dest.GeoLatitud, opt => opt.MapFrom(src => src.Latitud))
                .ForMember(dest => dest.GeoLongitud, opt => opt.MapFrom(src => src.Longitud))
                .ForMember(dest => dest.AmenitiesJson, opt => opt.MapFrom(src =>
                    src.Amenities != null ? JsonSerializer.Serialize(src.Amenities, (JsonSerializerOptions?)null) : null))
                .ForMember(dest => dest.FechaPublicacionUtc, opt => opt.Ignore())
                .ForMember(dest => dest.Medias, opt => opt.Ignore())
                .ForMember(dest => dest.Historial, opt => opt.Ignore());

            CreateMap<Propiedad, PropiedadResponseDto>()
                .ForMember(dest => dest.Latitud, opt => opt.MapFrom(src => src.GeoLatitud))
                .ForMember(dest => dest.Longitud, opt => opt.MapFrom(src => src.GeoLongitud))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.AmenitiesJson) ?
                    JsonSerializer.Deserialize<Dictionary<string, object>>(src.AmenitiesJson, (JsonSerializerOptions?)null) : null))
                .ForMember(dest => dest.Medias, opt => opt.MapFrom(src => src.Medias));

            CreateMap<Propiedad, PropiedadCreateDto>()
                .ForMember(dest => dest.Latitud, opt => opt.MapFrom(src => src.GeoLatitud))
                .ForMember(dest => dest.Longitud, opt => opt.MapFrom(src => src.GeoLongitud))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.AmenitiesJson) ?
                    JsonSerializer.Deserialize<Dictionary<string, object>>(src.AmenitiesJson, (JsonSerializerOptions?)null) : null));

            CreateMap<PropiedadMedia, PropiedadMediaDto>();
        }
    }
}
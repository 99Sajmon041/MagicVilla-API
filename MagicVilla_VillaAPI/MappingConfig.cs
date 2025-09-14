using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Villa, VillaDTO>();   // zbytečné, i další řádek - jde rád ReverseMap();
            CreateMap<VillaDTO, Villa>();
            CreateMap<Villa, VillaCreateDTO>().ReverseMap(); //nemusím deklarovat opačné mapování, AutoMapper to udělá automaticky
            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();

            CreateMap<VillaNumber, VillaNumberDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberCreateDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberUpdateDTO>().ReverseMap();
            CreateMap<RegistrationRequestDTO, LocalUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}

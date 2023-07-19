using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Characters, GetCharacterDto>();
            CreateMap<AddCharacterDto, Characters>();
            CreateMap<UpdateCharacterDto, Characters>();
            CreateMap<UserCharacter, GetCharacterDto>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.character.id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.character.name))
                .ForMember(dest => dest.hitpoints, opt => opt.MapFrom(src => src.character.hitpoints))
                .ForMember(dest => dest.strength, opt => opt.MapFrom(src => src.character.strength))
                .ForMember(dest => dest.defense, opt => opt.MapFrom(src => src.character.defense))
                .ForMember(dest => dest.intelligence, opt => opt.MapFrom(src => src.character.intelligence))
                .ForMember(dest => dest.fighterClass, opt => opt.MapFrom(src => src.character.fighterClass));
        }
    }
}
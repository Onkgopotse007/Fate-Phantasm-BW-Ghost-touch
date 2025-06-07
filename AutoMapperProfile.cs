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
            // Character mappings
            CreateMap<Characters, GetCharacterDto>();
            CreateMap<AddCharacterDto, Characters>();
            CreateMap<UpdateCharacterDto, Characters>();

            // Ability mappings
            CreateMap<Ability, GetAbilityDto>();
            CreateMap<AddAbilityDto, Ability>();

            // Loadout mappings
            CreateMap<CreateLoadoutDto, Loadout>();
            CreateMap<LoadoutCharacter, GetCharacterDto>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.character.id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.character.name))
                .ForMember(dest => dest.hitpoints, opt => opt.MapFrom(src => src.character.hitpoints))
                .ForMember(dest => dest.strength, opt => opt.MapFrom(src => src.character.strength))
                .ForMember(dest => dest.defense, opt => opt.MapFrom(src => src.character.defense))
                .ForMember(dest => dest.intelligence, opt => opt.MapFrom(src => src.character.intelligence))
                .ForMember(dest => dest.mana, opt => opt.MapFrom(src => src.character.mana))
                .ForMember(dest => dest.movement, opt => opt.MapFrom(src => src.character.movement))
                .ForMember(dest => dest.baseDamage, opt => opt.MapFrom(src => src.character.baseDamage))
                .ForMember(dest => dest.manaGainPerAttack, opt => opt.MapFrom(src => src.character.manaGainPerAttack))
                .ForMember(dest => dest.role, opt => opt.MapFrom(src => src.character.role))
                .ForMember(dest => dest.fighterClass, opt => opt.MapFrom(src => src.character.fighterClass))
                .ForMember(dest => dest.abilities, opt => opt.MapFrom(src => src.character.abilities));
            CreateMap<Loadout, GetLoadoutDto>()
                .ForMember(dest => dest.loadoutId, opt => opt.MapFrom(src => src.loadoutId))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.characters, opt => opt.MapFrom(src =>
                    src.characters.Select(lc => lc.character)
                ));
            //session automapper
            CreateMap<GameSession, GetGameSessionDto>();

            CreateMap<SessionCharacterState, GetSessionCharacterStateDto>()
                .ForMember(dest => dest.characterName, opt => opt.MapFrom(src => src.character.name));
        }
    }
}
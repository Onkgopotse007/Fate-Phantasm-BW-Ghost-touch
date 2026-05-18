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

            //User mappings
            CreateMap<User, GetUserDto>();

            //session automapper
            CreateMap<GameSession, GetGameSessionDto>()
                .ForMember(dest => dest.creatorUsername, opt => opt.MapFrom(src => src.creatorUser.userName))
                .ForMember(dest => dest.opponentUsername, opt => opt.MapFrom(src => src.opponentUser.userName));

            CreateMap<SessionCharacterState, GetSessionCharacterStateDto>()
                .ForMember(dest => dest.characterName, opt => opt.MapFrom(src => src.character.name));

            CreateMap<GameActionLog, GetGameActionLogDto>();
        }
    }
}
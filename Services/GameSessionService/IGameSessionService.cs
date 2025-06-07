using RPG_dotnet.Dtos.GameSession;
using RPG_dotnet.Models;

namespace RPG_dotnet.Services.GameSessionService
{
    public interface IGameSessionService
    {
        Task<ServiceResponse<GetGameSessionDto>> CreateGameSessionAsync(int userId, CreateGameSessionDto dto);
        Task<ServiceResponse<List<GetGameSessionDto>>> GetActiveGameSessionsAsync(int userId);
        Task<ServiceResponse<GetGameSessionDto>> GetGameSessionByIdAsync(int sessionId);
        Task<ServiceResponse<GetGameSessionDto>> MoveCharacterAsync(int userId, MoveActionDto dto);
        Task<ServiceResponse<GetGameSessionDto>> AttackCharacterAsync(int userId, AttackCharacterDto dto);
        Task<ServiceResponse<GetGameSessionDto>> AbandonSessionAsync(int userId, int sessionId);
        Task<ServiceResponse<GetGameSessionDto>> AcceptSessionAsync(int userId, AcceptGameSessionDto dto);
        Task<ServiceResponse<GetGameSessionDto>> RejectSessionAsync(int userId, int sessionId);
        Task<ServiceResponse<GetGameSessionDto>> CastSpellAsync(int userId,CastSpellDto dto);
        Task<ServiceResponse<List<GetGameSessionDto>>> GetGameSessionsByUserIdAsync(int userId, GameSessionState? state = null);
        Task<ServiceResponse<GetGameSessionDto>> JoinGameSessionAsync(JoinGameSessionDto dto);

        
    }
}
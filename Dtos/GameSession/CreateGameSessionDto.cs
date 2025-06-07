using System;

namespace RPG_dotnet.Dtos.GameSession
{
    public class CreateGameSessionDto
    {
        public int opponentUserId { get; set; }
        [MinLength(2), MaxLength(2)]
        public List<int> characterIds { get; set; }
    }
}
namespace RPG_dotnet.Dtos.GameSession
{
    public class CastSpellDto
    {
        public int sessionId { get; set; }
        public int casterId { get; set; }
        public int targetId { get; set; }
        public int abilityId { get; set; }
    }


}
namespace RPG_dotnet.Dtos.GameSession
{
    public class AcceptGameSessionDto
    {
        public int sessionId { get; set; }
        [MinLength(2), MaxLength(2)]
        public List<int> characterIds { get; set; }
    }
}
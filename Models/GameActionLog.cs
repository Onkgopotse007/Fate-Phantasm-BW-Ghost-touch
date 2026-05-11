namespace RPG_dotnet.Models;

public class GameActionLog
{
    public int id { get; set; }

    public int sessionId { get; set; }
    public GameSession session { get; set; }

    public int actorCharacterId { get; set; }
    public int? targetCharacterId { get; set; }

    public GameActionType actionType { get; set; }

    public int? abilityId { get; set; }

    public int damageDealt { get; set; }
    public double manaSpent { get; set; }
    public double manaGained { get; set; }
    public float? newPosition { get; set; }

    public int turnIndex { get; set; }
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
}

public enum GameActionType
{
    Move,
    Attack,
    CastSpell,
    EndTurn
}
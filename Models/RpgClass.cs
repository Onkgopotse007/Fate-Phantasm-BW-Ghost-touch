using System.Text.Json.Serialization;

namespace RPG_dotnet.Models
{
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RpgClass
    {
        Archer = 1,
        Saber = 2,
        Lancer = 3,
        Rider = 4,
        Caster = 5,
        Assassin = 6,
        Berserker = 7,
        Avenger = 8
    }


    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RoleType
    {
        Vanguard,
        Support
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GameSessionState
    {
        PENDING,
        REJECTED,
        ACTIVE,
        COMPLETED,
        ABANDONED
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TeamSide
    {
        CREATOR,
        OPPONENT
    }

    // New: drives the special effect a Noble Phantasm applies on cast
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AbilityEffect
    {
        None,
        Heal,
        ManaDrain,
        Stun,
        Poison,
        MultiTarget,
        DefensePierce,
        PositionPush,
        Revive
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AbilityTargetType
    {
        Enemy,
        Ally,
        AllEnemies
    }
}
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
}
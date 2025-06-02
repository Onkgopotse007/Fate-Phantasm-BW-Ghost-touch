namespace RPG_dotnet.Models
{
    public class CharacterAbility
    {
        public int characterId { get; set; }
        public Characters character { get; set; }

        public int abilityId { get; set; }
        public Ability ability { get; set; }
    }
}

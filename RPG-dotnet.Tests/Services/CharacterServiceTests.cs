using Xunit;
using Moq;
using RPG_dotnet.Services.CharactersService;
using RPG_dotnet.Models;
using RPG_dotnet.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RPG_dotnet.Dtos.Characters;
using RPG_dotnet;
using AutoMapper;

public class CharacterServiceTests
{
    private readonly ICharacterService _service;
    private readonly IMapper _mapper;
    private readonly DataContext _context;

    public CharacterServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "CharacterServiceTestDb")
            .Options;
        _context = new DataContext(options);

        // Reset database between test runs
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = config.CreateMapper();

        _service = new CharacterService(_mapper, _context);
    }

    [Fact]
    public async Task AddCharacter_ShouldSucceed()
    {
        // Arrange
        var newCharacter = new AddCharacterDto
        {
            name = "Test Hero",
            hitpoints = 100,
            strength = 15,
            defense = 10,
            intelligence = 8,
            movement = 5,
            baseDamage = 25,
            manaGainPerAttack = 12,
            mana = 30,
            fighterClass = RpgClass.Saber,
            role = RoleType.Vanguard,
            abilities = new List<AddAbilityDto>
            {
                new AddAbilityDto
                {
                    name = "Fireball",
                    description = "Launches a fireball at enemies",
                    manaCost = 10,
                    damage = 25
                },
                new AddAbilityDto
                {
                    name = "Ice Shard",
                    description = "Shoots a shard of ice",
                    manaCost = 5,
                    damage = 15
                }
            }
        };

        // Act
        var result = await _service.AddCharacter(newCharacter);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.success);
        var createdChar = result.data.Find(c => c.name == "Test Hero");
        Assert.NotNull(createdChar);
        Assert.Equal(2, createdChar.abilities.Count);
        Assert.Contains(createdChar.abilities, a => a.name == "Fireball");
        Assert.Contains(createdChar.abilities, a => a.name == "Ice Shard");
    }
}
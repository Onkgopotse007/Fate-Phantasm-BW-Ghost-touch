namespace RPG_dotnet.Hubs;

public interface IGameSessionClient
{
    Task SessionUpdated(GetGameSessionDto session);
    Task SessionCompleted(GetGameSessionDto session);
    Task Error(string message);
}
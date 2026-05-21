using Microsoft.AspNetCore.SignalR;

namespace RPG_dotnet.Hubs;

    [Authorize]
    public class GameSessionHub : Hub<IGameSessionClient>
    {
        private readonly DataContext _context;

        public GameSessionHub(DataContext context)
        {
            _context = context;
        }

        public async Task JoinSession(int sessionId)
        {
            var userId = GetUserId();
            var isParticipant = await _context.GameSessions
                .AnyAsync(gs => gs.gameSessionId == sessionId &&
                               (gs.creatorUserId == userId || gs.opponentUserId == userId));

            if (!isParticipant)
            {
                await Clients.Caller.Error("You are not a participant in this session.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
        }

        public async Task LeaveSession(int sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(sessionId));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public static string GroupName(int sessionId) => $"session-{sessionId}";

        private int GetUserId()
        {
            var claim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new HubException("Unauthorised.");
            return int.Parse(claim.Value);
        }
    }
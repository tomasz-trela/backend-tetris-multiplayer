using System.Net.WebSockets;
using System.Text;
using backend_tetris.entities;

namespace backend_tetris.services;

public class GameService
{
    private readonly List<Room> _multiplayerRooms = [new()];
    private readonly Dictionary<string,Room> _customRooms = [];
    private readonly ILogger<GameService> _logger;

    public GameService(ILogger<GameService> logger)
    {
        _logger = logger;
    }
    public async Task<Room> AddUser(MyUser user)
    {
        var lastRoom = _multiplayerRooms.Last();
        if (_multiplayerRooms.Last().State == RoomState.Open)
        {
            await lastRoom.AddPlayer(user);
            return lastRoom;
        }

        lastRoom = new Room(user);
        _multiplayerRooms.Add(lastRoom);
        return lastRoom;
    }

    public async Task RemovePlayer(MyUser user, WebSocketReceiveResult result, Room room)
    {
        await user.CloseConnection();
        await room.RemovePlayer(user, result);
    }
    
    public (string, Room) CreateCustomRoom(MyUser user)
    {
        var room = new Room(user, RoomState.Closed);
        var inviteCode = GenerateUniqueInviteCode();
        _customRooms.Add(inviteCode, room);
        return (inviteCode, room);
    }

    public async Task<(bool, Room)> JoinCustomRoom(MyUser user, string inviteCode)
    {
        var room = _customRooms[inviteCode];
        var isAdded = await room.AddPlayer(user);
        return (isAdded, room);
    }
    
    private string GenerateUniqueInviteCode()
    {
        string inviteCode;
        do
        {
            inviteCode = InviteCodeGenerator.GenerateInviteCode();
        } while (_customRooms.ContainsKey(inviteCode));

        return inviteCode;
    }
    
    
    public async Task HandleWebSocketCommunication(MyUser user, Room room)
    {
        var webSocket = user.WebSocketConnection!;
        var buffer = new byte[1024 * 2];

        while (webSocket.State == WebSocketState.Open)
        {
            _logger.LogInformation("Receive message from {name}", user.Username);

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
             
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await RemovePlayer(user, result, room);
                _logger.LogInformation("People in room: {count}", room.Players.Count);
                room.Dispose();
                return;
            }
            await room.ReceiveMessage(user, message);
        }
    }
}
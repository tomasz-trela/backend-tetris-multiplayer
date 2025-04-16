using System.Net.WebSockets;
using backend_tetris.entities;

namespace backend_tetris.services;

public class GameService
{
    private readonly List<Room> _multiplayerRooms = [new()];
    private readonly Dictionary<string,Room> _customRooms = [];
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

    public static async Task RemovePlayer(MyUser user, WebSocketReceiveResult result, Room room)
    {
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
}
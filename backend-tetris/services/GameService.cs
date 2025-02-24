using System.Net.WebSockets;
using backend_tetris.entities;

namespace backend_tetris.services;

public class GameService
{
    private readonly List<Room> _rooms = [new()];
    public async Task<Room> AddPlayer(Player player)
    {
        var lastRoom = _rooms.Last();
        if (_rooms.Last().State == RoomState.Open)
        {
            await lastRoom.AddPlayer(player);
            return lastRoom;
        }

        lastRoom = new Room(player);
        _rooms.Add(lastRoom);
        return lastRoom;
    }

    public async Task RemovePlayer(Player player, WebSocketReceiveResult result, Room room)
    {
        await room.RemovePlayer(player, result);
    }
    
}
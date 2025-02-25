using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace backend_tetris.entities;

public class Room(MyUser? user = null, RoomState state = RoomState.Open, int maxPlayers = 2)
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<MyUser> Players { get; } = user == null ? [] : [user];
    public RoomState State { get; set; } = state;
    public async Task<bool> AddPlayer(MyUser player)
    {
        if(Players.Count >= maxPlayers)
            return false;
        Players.Add(player);
        State = Players.Count.IntToRoomState(maxPlayers);
        if (State == RoomState.Closed)
            await Broadcast("Game started"u8.ToArray());
        return true;
    }

    public async Task Broadcast(ArraySegment<byte> message)
    {
        foreach (var player in Players.Where(player => player.WebSocketConnection?.State == WebSocketState.Open))
        {
            await player.WebSocketConnection?.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None)!;
        }
    }

    public async Task<RoomState> RemovePlayer(MyUser player, WebSocketReceiveResult result)
    {
        Players.Remove(player);
        await player.WebSocketConnection?.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        return State;
    }
}

public enum RoomState
{
    Open,
    Closed,
}

public static class RoomStateExtensions
{
    public static RoomState IntToRoomState(this int numberOfPlayers, int maxPlayers)
    {
        return numberOfPlayers >= maxPlayers 
            ? RoomState.Closed 
            : RoomState.Open;
    }
}
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace backend_tetris.entities;

public class Room(Player? player = null, int maxPlayers = 2 )
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<Player> Players { get; } = player == null ? [] : [player];
    public RoomState State { get; private set; } = RoomState.Open;

    public async Task<RoomState> AddPlayer(Player player)
    {
        Players.Add(player);
        State = Players.Count.IntToRoomState(maxPlayers);
        if (State == RoomState.Closed)
            await Broadcast("Game started"u8.ToArray());
        return State;
    }

    public async Task Broadcast(ArraySegment<byte> message)
    {
        foreach (var player in Players.Where(player => player.Socket.State == WebSocketState.Open))
        {
            await player.Socket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task<RoomState> RemovePlayer(Player player, WebSocketReceiveResult result)
    {
        Players.Remove(player);
        await player.Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
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
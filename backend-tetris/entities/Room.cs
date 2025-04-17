using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using backend_tetris.DTOs;

using backend_tetris.utils;

namespace backend_tetris.entities;

public class Room(MyUser? user = null, RoomState state = RoomState.Open, int maxPlayers = 2)
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<MyUser> Players { get; } = user == null ? [] : [user];
    public RoomState State { get; private set; } = state;

    private readonly CountdownTimer _timer = new(180);


    public async Task<bool> AddPlayer(MyUser player)
    {
        if(Players.Count >= maxPlayers)
            return false;
        Players.Add(player);
        State = Players.Count.IntToRoomState(maxPlayers);
        if (State == RoomState.Closed)
        {
            _timer.Start();
            await Broadcast("Game started"u8.ToArray());
        }

        return true;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    public async Task ReceiveMessage(MyUser user, string message)
    {
        var resp = JsonSerializer.Deserialize<PlayerResponse>(message);
        if (resp is not null)
            user.score = resp.score;
        
        var enemy = Players.FirstOrDefault(x => x.Id != user.Id);
        
        if (enemy is null)
        {
            await Broadcast("No oponent"u8.ToArray());
            return;
        }
         
        var broadcastMessage = JsonSerializer.Serialize(
            new GameStateDto(user.score, enemy.score, enemy.Username, _timer.TimeLeft)
        );
        await SendToPlayer(new ArraySegment<byte>(Encoding.UTF8.GetBytes(broadcastMessage)), enemy);
    }

    public async Task Broadcast(ArraySegment<byte> message)
    {
        foreach (var player in Players.Where(player => player.WebSocketConnection?.State == WebSocketState.Open))
        {
            await player.WebSocketConnection?.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None)!;
        }
    }

    private async Task SendToPlayer(ArraySegment<byte> message, MyUser user)
    {
        await user.WebSocketConnection?.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None)!;
    }

    public async Task<RoomState> RemovePlayer(MyUser player, WebSocketReceiveResult result)
    {
        Players.Remove(player);
        if (result.CloseStatus.HasValue)
            await player.CloseConnection();
        return State;
    }
    public async Task Clear()
    {
        foreach (var myUser in Players)
        {
            await myUser.CloseConnection();
        }
        Players.Clear();
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
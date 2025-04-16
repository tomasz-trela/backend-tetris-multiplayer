using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using backend_tetris.DTOs;
using System.Timers;
using Timer = System.Timers.Timer;

namespace backend_tetris.entities;

public class Room(MyUser? user = null, RoomState state = RoomState.Open, int maxPlayers = 2)
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<MyUser> Players { get; } = user == null ? [] : [user];
    public RoomState State { get; set; } = state;
    public int TimeLeft { get; set; } = 180;

    private Timer? _timer;
    
    public async Task<bool> AddPlayer(MyUser player)
    {
        if(Players.Count >= maxPlayers)
            return false;
        Players.Add(player);
        State = Players.Count.IntToRoomState(maxPlayers);
        if (State == RoomState.Closed)
        {
            StartTimer();
            await Broadcast("Game started"u8.ToArray());
        }

        return true;
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
         
        var broadcastMessage = JsonSerializer.Serialize<GameStateDto>(
            new GameStateDto(user.score, enemy.score, enemy.Username, TimeLeft)
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
        await player.WebSocketConnection?.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        return State;
    }
    
    private void StartTimer()
    {
        if (_timer != null)
            return; 

        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerTick;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        TimeLeft--;

        Console.WriteLine($"Time left: {TimeLeft}");

        if (TimeLeft > 0) return;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
            
        _ = Broadcast("Game over"u8.ToArray());
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
using System.Net.WebSockets;

namespace backend_tetris.entities;

public class Player(string name, WebSocket socket, int id = 1)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public WebSocket Socket { get; } = socket;
}
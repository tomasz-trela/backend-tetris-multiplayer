using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using backend_tetris.database;
using backend_tetris.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_tetris.controllers;

[Route("connect")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ILogger<GameController> _logger;
    private readonly AppDbContext _dbContext;

    public GameController(GameService gameService, ILogger<GameController> logger, AppDbContext dbContext)
    {
        _gameService = gameService;
        _logger = logger;
        _dbContext = dbContext;
    }

    [Authorize]
    [Route("")]
    public async Task Join()
    {
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("User ID is null");
            HttpContext.Response.StatusCode = 401;
            return;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        user!.WebSocketConnection = webSocket;

        var room = await _gameService.AddUser(user);
        _logger.LogInformation("Room {id}", room.Id);
        _logger.LogInformation("People in room:  {count}", room.Players.Count);

        var buffer = new byte[1024 * 2];
        while (webSocket.State == WebSocketState.Open)
        {
            _logger.LogInformation("Receive message from {name}", user.Username);

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await GameService.RemovePlayer(user, result, room);
                _logger.LogInformation("People in room: {count}", room.Players.Count);
            }

            await room.Broadcast(new ArraySegment<byte>(buffer, 0, result.Count));
        }
    }
    
    [Authorize]
    [Route("{inviteCode}")]
    public async Task JoinCustom(string inviteCode)
    {
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("User ID is null");
            HttpContext.Response.StatusCode = 401;
            return;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        user!.WebSocketConnection = webSocket;

        var (isJoined,room) = await _gameService.JoinCustomRoom(user, inviteCode);
        if (!isJoined) return;
        _logger.LogInformation("Room {id}", room.Id);
        _logger.LogInformation("People in room:  {count}", room.Players.Count);

        var buffer = new byte[1024 * 2];
        while (webSocket.State == WebSocketState.Open)
        {
            _logger.LogInformation("Receive message from {name}", user.Username);

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await GameService.RemovePlayer(user, result, room);
                _logger.LogInformation("People in room: {count}", room.Players.Count);
                user.WebSocketConnection = null;
            }

            await room.Broadcast(new ArraySegment<byte>(buffer, 0, result.Count));
        }
    }
    
    [Authorize]
    [Route("create")]
    public async Task CreateCustom()
    {
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("User ID is null");
            HttpContext.Response.StatusCode = 401;
            return;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        user!.WebSocketConnection = webSocket;

        var (inviteCode,room) =  _gameService.CreateCustomRoom(user);
        
        room.Broadcast(Encoding.UTF8.GetBytes(inviteCode));
        
        _logger.LogInformation("Room {id}", room.Id);
        _logger.LogInformation("People in room:  {count}", room.Players.Count);

        var buffer = new byte[1024 * 2];
        while (webSocket.State == WebSocketState.Open)
        {
            _logger.LogInformation("Receive message from {name}", user.Username);

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await GameService.RemovePlayer(user, result, room);
                _logger.LogInformation("People in room: {count}", room.Players.Count);
                user.WebSocketConnection = null;
            }

            await room.Broadcast(new ArraySegment<byte>(buffer, 0, result.Count));
        }
    }
}

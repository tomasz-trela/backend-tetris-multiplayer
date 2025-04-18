using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using backend_tetris.database;
using backend_tetris.entities;
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
        var user = await GetAuthenticatedUser();
        if (user == null) return;

        var room = await _gameService.AddUser(user);
        _logger.LogInformation("Room {id}", room.Id);

        await _gameService.HandleWebSocketCommunication(user, room);
    }

    [Authorize]
    [Route("{inviteCode}")]
    public async Task JoinCustom(string inviteCode)
    {
        var user = await GetAuthenticatedUser();
        if (user == null) return;

        var (isJoined, room) = await _gameService.JoinCustomRoom(user, inviteCode);
        if (!isJoined) return;

        _logger.LogInformation("Room {id}", room.Id);

        await _gameService.HandleWebSocketCommunication(user, room);
    }

    [Authorize]
    [Route("create")]
    public async Task CreateCustom()
    {
        var user = await GetAuthenticatedUser();
        if (user == null) return;

        var (inviteCode, room) = _gameService.CreateCustomRoom(user);
        await room.Broadcast(Encoding.UTF8.GetBytes(inviteCode));

        _logger.LogInformation("Room {id}", room.Id);

        await _gameService.HandleWebSocketCommunication(user, room);
    }

    private async Task<MyUser?> GetAuthenticatedUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("User ID is null");
            HttpContext.Response.StatusCode = 401;
            return null;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("User not found");
            HttpContext.Response.StatusCode = 404;
            return null;
        }

        user.WebSocketConnection = await HttpContext.WebSockets.AcceptWebSocketAsync();
        return user;
    }
}
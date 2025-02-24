using System.Net.WebSockets;
using System.Security.Claims;
using backend_tetris.entities;
using backend_tetris.services;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_tetris.controllers;

public class GameController(GameService gameService, ILogger<GameController> logger) : ControllerBase
{
    [Authorize]
    [Route("connect")]
    public async Task GameWebSocket()
    {
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var player = new Player(name!, webSocket);
        var room = await gameService.AddPlayer(player);
        logger.LogInformation("Room {id}", room.Id);
        logger.LogInformation("People in room:  {count}", room.Players.Count);

        var buffer = new byte[1024 * 2];
        while (webSocket.State == WebSocketState.Open)
        {
            logger.LogInformation("Receive message from {name}", name);
            
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await gameService.RemovePlayer(player, result, room);
                logger.LogInformation("People in room: {count}", room.Players.Count);
            }

            await room.Broadcast(new ArraySegment<byte>(buffer, 0, result.Count));
        }
    }
}
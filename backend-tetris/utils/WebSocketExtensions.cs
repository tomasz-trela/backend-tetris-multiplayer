using System.Net.WebSockets;

namespace backend_tetris.tools;

public static class WebSocketExtensions
{
    public static async Task<byte[]?> ReceiveFullMessage(this WebSocket socket, byte[] buffer)
    {
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                return null;
            }

            ms.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);
        
        return ms.ToArray();
    }
}
namespace MessageService.Services
{
    using System;
    using System.Net.WebSockets;
    using System.Text;

    public class WebSocketConnectionManager
    {
        private readonly List<WebSocket> _sockets = new List<WebSocket>();

        public void AddSocket(WebSocket socket)
        {
            _sockets.Add(socket);
        }

        public async Task ReceiveMessages(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Клиент завершил соединение", CancellationToken.None);
                }
            }
        }

        public async Task BroadcastMessage(string message)
        {
            var byteMessage = Encoding.UTF8.GetBytes(message);
            foreach (var socket in _sockets.ToList())
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(byteMessage), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
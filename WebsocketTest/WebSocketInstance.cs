using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace WebsocketTest
{
    public class WebSocketInstance
    {
        internal List<WebSocket> Connections { get; set; } = [];

        public WebSocketInstance(WebApplication app, string path)
        {
            app.Map(path, async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    await Initalize(context);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            });
        }

        internal virtual async Task Initalize(HttpContext context)
        {
            await Task.Run(() => { Console.WriteLine("Default Initalize method that is supposed to be overridden"); });
        }

        internal static async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        internal async Task Broadcast(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var socket in Connections)
            {
                if (socket.State == WebSocketState.Open)
                {
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
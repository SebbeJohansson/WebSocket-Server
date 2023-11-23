using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebsocketTest
{
    public class Chat(WebApplication app, string path) : WebSocketInstance(app, path)
    {
        internal class Message {
            public string Name { get; set; } = "Anonymous";
            public string Text { get; set; } = "Nothing";
        }

        internal override async Task Initalize(HttpContext context)
        {
            var curName = context.Request.Query["name"].ToString() ?? null;

            using WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();

            Connections.Add(ws);

            await Broadcast($"{curName} joined the room");
            await Broadcast($"{Connections.Count} users connected");
            await ReceiveMessage(ws,
                async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Message message = new();
                        if (curName == null || curName == "") {
                            /* Let's assume that the message is in json format
                            {
                                "Name": "John"
                                "Text": "Hello"
                            }
                            */
                            string jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            message = JsonSerializer.Deserialize<Message>(Regex.Unescape(jsonString));
                        } else {
                            /* Let's assume that the message is in plain text format */
                            message.Text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            message.Name = curName;
                        }
                        await Broadcast(JsonSerializer.Serialize(message));
                    }
                    else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
                    {
                        Connections.Remove(ws);
                        if (curName != null) {
                            await Broadcast($"{curName} left the room");
                        }
                        await Broadcast($"A connection disconnected, {Connections.Count} connections still connected");
                        await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                });
        }
    }
}
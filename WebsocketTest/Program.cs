using System.Net;
using System.Net.WebSockets;
using System.Text;
using WebsocketTest;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

var webSocketInstances = new List<WebSocketInstance>
{
    new Chat(app, "/ws/chat"),
    new(app, "/ws")
};

await app.RunAsync();
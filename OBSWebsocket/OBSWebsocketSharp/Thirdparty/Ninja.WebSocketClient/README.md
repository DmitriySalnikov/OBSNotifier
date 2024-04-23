# Ninja WebSocket

[![NuGet](https://img.shields.io/nuget/v/Ninja.WebSocketClient)](https://www.nuget.org/packages/Ninja.WebSocketClient) 
[![GitHub](https://img.shields.io/github/license/ninjastacktech/ninja-websocket-net)](https://github.com/ninjastacktech/ninja-websocket-net/blob/master/LICENSE)

NinjaWebSocket is an easy-to-use .NET6 WebSocket client with auto-reconnect and keep-alive capabilities. 

Lightweight library, user-friendly API, inspired by the javascript WebSocket API and SignalR.

## snippets
The repo also contains two usage examples:
- [Connecting a bot to a Discord websocket channel](https://github.com/ninjastacktech/ninja-websocket-net/blob/master/test/Ninja.WebSocket.DemoConsole/DiscordWebSocketClient.cs)
- [Subscribing to Infura/Ethereum transaction events](https://github.com/ninjastacktech/ninja-websocket-net/blob/master/test/Ninja.WebSocket.DemoConsole/EthereumWebSocketClient.cs)

Basic usage:
```C#
var ws = new NinjaWebSocket("<websocket_url>")
    .SetOptions(options => options.SetRequestHeader("X-Ninja-WebSocket", "hello world"))
    .SetKeepAlive(keepAliveIntervalSeconds: 5)
    .SetAutomaticReconnect(autoReconnectIntervalSeconds: 5);

ws.OnConnected += async () =>
{
    // Notify users the connection was established.
    Console.WriteLine("Connected.");

    // Send messages
    await ws.SendAsync("hello world!");
};

ws.OnReceived += data =>
{
    Console.WriteLine(Encoding.UTF8.GetString(data!.Value.ToArray()));

    return Task.CompletedTask;
};

ws.OnKeepAlive += () =>
{
    Console.WriteLine("Ping.");

    return Task.CompletedTask;
};

ws.OnReconnecting += (ex) =>
{
    // Notify users the connection was lost and the client is reconnecting.
    Console.WriteLine($"Reconnecting: {ex?.Message}");

    return Task.CompletedTask;
};

ws.OnClosed += (ex) =>
{
    // Notify users the connection has been closed.
    Console.WriteLine($"Closed: {ex?.Message}");

    return Task.CompletedTask;
};

// Connect to the ws and start listening
await ws.StartAsync();

```

---
### MIT License

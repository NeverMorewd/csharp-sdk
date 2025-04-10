﻿using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Microsoft.Extensions.AI;

// Connect to an MCP server
Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
Console.WriteLine("Connecting client to MCP 'everything' server");
var mcpClient = await McpClientFactory.CreateAsync(
    new()
    {
        Id = "everything",
        Name = "Everything",
        TransportType = TransportTypes.StdIo,
        TransportOptions = new()
        {
            ["command"] = "mcpserver.everything.stdio",
        }
    });

// Get all available tools
Console.WriteLine("Tools available:");
var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"  {tool}");
}
Console.WriteLine();

// Create an IChatClient. (This shows using OllamaChatClient, but it could be any other IChatClient implementation.)
using IChatClient chatClient =
    new OllamaChatClient("http://localhost:11434/", "llama3.2:latest")
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

// Have a conversation, making all tools available to the LLM.
List<ChatMessage> messages = [];
while (true)
{
    Console.Write("Q: ");
    messages.Add(new(ChatRole.User, Console.ReadLine()));

    List<ChatResponseUpdate> updates = [];
    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        Console.Write(update);
        updates.Add(update);
    }
    Console.WriteLine();

    messages.AddMessages(updates);
}
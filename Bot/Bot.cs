using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Bot
{
    // Sockets
    private readonly DiscordSocketClient client;
    private readonly Socket serverSocket;
    private readonly Socket clientSocket;

    // String datas
    private readonly string token;
    private readonly string commandPrefix;
    private readonly string shortCommandPrefix;

    public Bot(string token, string commandPrefix, string shortCommandPrefix = "")
    {
        DiscordSocketConfig config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        };

        client = new DiscordSocketClient(config);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverSocket.Bind(new IPEndPoint(IPAddress.Any, 1677));
        serverSocket.Listen();

        clientSocket = serverSocket.Accept();

        // Add client events
        client.Log += Log;
        client.Ready += Ready;
        client.InteractionCreated += InteractionCreated;
        client.MessageReceived += MessageReceived;

        // Initialize variables
        this.token = token;
        this.commandPrefix = commandPrefix;
        this.shortCommandPrefix = shortCommandPrefix;
    }

    // The main entry point of the bot.
    public async Task Run()
    {
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());

        return Task.CompletedTask;
    }

    // Event functions
    private Task Ready()
    {
        Console.WriteLine($"{client.CurrentUser} is ready!");

        return Task.CompletedTask;
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        if (interaction.Type != InteractionType.MessageComponent)
        {
            return;
        }

        SocketMessageComponent? messageComponent = interaction as SocketMessageComponent;

        if (messageComponent == null)
        {
            return;
        }

        await messageComponent.Channel.SendMessageAsync("Move successful!");

        switch (messageComponent.Data.CustomId)
        {
            case "left":
                clientSocket?.Send(Encoding.UTF8.GetBytes("0"), 0);

                break;
            case "right":
                clientSocket?.Send(Encoding.UTF8.GetBytes("1"), 0);

                break;
            case "up":
                clientSocket?.Send(Encoding.UTF8.GetBytes("2"), 0);

                break;
            case "down":
                clientSocket?.Send(Encoding.UTF8.GetBytes("3"), 0);

                break;
        }
    }

    private async Task MessageReceived(SocketMessage message)
    {
        // Cannot run if message author is a bot.
        if (message.Author.Id == client.CurrentUser.Id)
        {
            return;
        }

        // Check prefix.
        string? command = null;

        if (message.Content.StartsWith(commandPrefix))
        {
            command = message.Content.Substring(commandPrefix.Length);
        }
        else if (message.Content.StartsWith(shortCommandPrefix))
        {
            command = message.Content.Substring(shortCommandPrefix.Length);
        }

        if (command == null)
        {
            return;
        }

        // Run command reality.
        await ExecuteCommand(message, command);
    }

    private async Task ExecuteCommand(SocketMessage message, string command)
    {
        switch (command)
        {
            case "Move.Left":
            case "L":
                await message.Channel.SendMessageAsync("Move left!");

                clientSocket?.Send(Encoding.UTF8.GetBytes("0"), 0);

                break;
            case "Move.Right":
            case "R":
                await message.Channel.SendMessageAsync("Move right!");

                clientSocket?.Send(Encoding.UTF8.GetBytes("1"), 0);

                break;
            case "Move.Up":
            case "U":
                await message.Channel.SendMessageAsync("Move up!");

                clientSocket?.Send(Encoding.UTF8.GetBytes("2"), 0);

                break;
            case "Move.Down":
            case "D":
                await message.Channel.SendMessageAsync("Move down!");

                clientSocket?.Send(Encoding.UTF8.GetBytes("3"), 0);

                break;
            case "Show.MoveButtons":
            case "S_MB":
                EmbedBuilder embedBuilder = new EmbedBuilder();
                ButtonComponent leftButton = new ButtonBuilder("←", "left", ButtonStyle.Success).Build();
                ButtonComponent rightButton = new ButtonBuilder("→", "right", ButtonStyle.Success).Build();
                ButtonComponent upButton = new ButtonBuilder("↑", "up", ButtonStyle.Success).Build();
                ButtonComponent downButton = new ButtonBuilder("↓", "down", ButtonStyle.Success).Build();
                ActionRowBuilder rowBuilder = new ActionRowBuilder();
                ComponentBuilder componentBuilder = new ComponentBuilder();

                // Add generic informations of the embed.
                embedBuilder.Title = "Discord Character Movement Controller UI";
                embedBuilder.Description = "There's move Buttons";
                embedBuilder.Color = new Color(31, 136, 15);
                embedBuilder.ThumbnailUrl = "https://cafeptthumb-phinf.pstatic.net/MjAyMTExMTBfOTcg/MDAxNjM2NDcyNDM0MTE3.xcHb55u9Kuu1Y8UoppvCy8UreenOS70FlSmcQujFUycg.QAsJYIDWe7DGZRXX76HPo3g-S_Hm2x7Sg6ey1WBd3p4g.JPEG/20210529_133301.jpg?type=f150_150_mask";

                // Add buttons in row.
                rowBuilder.AddComponent(leftButton);
                rowBuilder.AddComponent(rightButton);
                rowBuilder.AddComponent(upButton);
                rowBuilder.AddComponent(downButton);

                // Build components
                componentBuilder.AddRow(rowBuilder);

                // Send embed to channel.
                await message.Channel.SendMessageAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());

                break;
            case "Network.Close":
            case "Net.Cls":
                await message.Channel.SendMessageAsync("Closed!");

                clientSocket?.Close();
                serverSocket?.Close();

                Process.GetCurrentProcess().Kill();

                break;
        }
    }
}

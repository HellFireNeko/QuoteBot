using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

static void ExecuteWithChance(int percentChance, Action callback)
{
    if (percentChance < 0 || percentChance > 100)
    {
        return;
    }

    int randomNumber = Rand.random.Next(0, 101);

    if (randomNumber <= percentChance)
    {
        callback.Invoke();
    }
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt")
    .CreateLogger();

if (File.Exists("token.txt") is false)
{
    Log.Fatal("The token file does not exist!");
    return;
}

Log.Information("Preparing startup");

var token = File.ReadAllText("token.txt");

var loggerFactory = new LoggerFactory().AddSerilog();

var services = new ServiceCollection()
    .AddSingleton<Random>()
    .BuildServiceProvider();

var conf = new DiscordConfiguration()
{
    LoggerFactory = loggerFactory,
    Token = token,
    Intents = DiscordIntents.All,
    LogUnknownEvents = false
};

var client = new DiscordClient(conf);

var interConf = new InteractivityConfiguration()
{
    PollBehaviour = PollBehaviour.DeleteEmojis,
    Timeout = TimeSpan.FromMinutes(2),
    ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
};

client.UseInteractivity(interConf);

var slashConf = new SlashCommandsConfiguration()
{
    Services = services
};

var slash = client.UseSlashCommands(slashConf);

slash.RegisterCommands<Commands>();

client.Ready += Client_Ready;

client.MessageCreated += Client_MessageCreated;

await client.ConnectAsync();

while (Commands.KeepRunning) { await Task.Delay(100); }

static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
{
    Log.Information("Client ready");

    return Task.CompletedTask;
}

static async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs args)
{
    if (args.Author.IsBot) return;

    await Task.Run(() =>
    {
        var message = args.Message;

        Log.Information("({User}) Said: \"{Content}\"", message.Author.Username, message.Content);

        if (message.Channel.IsPrivate) return;

        if (message.Content.Length > 1500) return;

        var conf = GuildConfig.Get(args.Guild.Id);

        if (conf.Equals(GuildConfig.Default)) return;

        ExecuteWithChance(conf.Chance, async () =>
        {
            if (args.Guild.Channels.ContainsKey(conf.ChannelID) is false) return;

            if (conf.AllowNSFW is false && message.Channel.IsNSFW) return;

            var channel = args.Guild.GetChannel(conf.ChannelID);

            var button = new DiscordLinkButtonComponent(message.JumpLink.ToString(), "Jump to message");

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Get quoted bozo")
                .WithDescription($"**{message.Author.Mention} said:**\n{message.Content}")
                .WithColor(GetRandomColor())
                .WithTimestamp(message.Timestamp)
                .WithAuthor(message.Author.Username, null, message.Author.AvatarUrl)
                .Build();

            var msgBuilder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new[]
                {
                        button
                }
            );

            await channel.SendMessageAsync(msgBuilder);
        });
    });
}

static DiscordColor GetRandomColor()
{
    Random random = new();

    byte red = (byte)random.Next(256); // Random red component (0-255)
    byte green = (byte)random.Next(256); // Random green component (0-255)
    byte blue = (byte)random.Next(256); // Random blue component (0-255)

    return new DiscordColor(red, green, blue);
}

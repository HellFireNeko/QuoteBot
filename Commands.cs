using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

internal class Commands : ApplicationCommandModule
{
    public static bool KeepRunning = true;

    [SlashRequireOwner]
    [SlashCommand("shutdown", "Stops the bot from taking over the world")]
    public async Task Shutdown(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Goodbye cruel world", true);

        KeepRunning = false;

        await ctx.Client.DisconnectAsync();
    }

    [SlashRequireGuild]
    [SlashRequirePermissions(Permissions.Administrator)]
    [SlashCommand("set", "Sets the quote channel to this channel!")]
    public async Task Set(InteractionContext ctx, [Option("Nsfw", "Does it allow nsfw?", true)] bool allowNsfw, [Option("Chance", "Sets the chance of the quote happening, between 1 and 99")] long chance)
    {
        await ctx.DeferAsync(true);

        if (chance < 1 || chance > 99)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Chance must be between 1 and 99"));
            return;
        }

        var config = GuildConfig.Default;

        config.ChannelID = ctx.Channel.Id;
        config.AllowNSFW = allowNsfw;
        config.Chance = (int)chance;

        config.ToFile(ctx.Guild.Id);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This is now the quote channel!"));
    }
}
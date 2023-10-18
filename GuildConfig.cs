using Newtonsoft.Json;

internal struct GuildConfig
{
    public ulong ChannelID;
    public bool AllowNSFW;
    public int Chance;

    public static GuildConfig Default => new(0, false, 10);

    public static GuildConfig Get(ulong guildId)
    {
        if (File.Exists($"{guildId}.json"))
        {
            return JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText($"{guildId}.json"));
        }
        return Default;
    }

    public readonly void ToFile(ulong guildId) => File.WriteAllText($"{guildId}.json", JsonConvert.SerializeObject(this));


    public GuildConfig(ulong channelID, bool allowNSFW, int chance)
    {
        ChannelID = channelID;
        AllowNSFW = allowNSFW;
        Chance = chance;
    }
}
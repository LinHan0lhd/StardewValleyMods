namespace ArtisanPond;

public class ModConfig
{
    public bool EnableMoodSystem { get; set; } = true;
    public bool EnableLegendaryPondExpansion { get; set; } = true;
    public int LegendaryPondMaxCapacity { get; set; } = 3;
    public bool EnableInfinitePondQuests { get; set; } = true;
    public int InfiniteQuestRefreshInterval { get; set; } = 3;
}

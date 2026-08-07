using StardewModdingAPI;

namespace JunimoKartTweaks;
public class ModConfig
{
    /// <summary>初始生命数（原版：3）</summary>
    public int InitialLives { get; set; } = 5;

    /// <summary>多少金币兑换1条命（原版：100）</summary>
    public int CoinsPerLife { get; set; } = 50;
}

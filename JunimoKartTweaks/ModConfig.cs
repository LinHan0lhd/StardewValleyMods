using StardewModdingAPI;

namespace JunimoKartTweaks;
public class ModConfig
{
    /// <summary>初始生命数（原版：3）</summary>
    public int InitialLives { get; set; } = 5;

    /// <summary>每关结束后增加的生命数（原版：逐次补充至3）</summary>
    public int LivesRefillPerLevel { get; set; } = 1;

    /// <summary>集齐3个水果奖励的生命数（原版：1）</summary>
    public int FruitBonusLives { get; set; } = 3;

    /// <summary>集齐3个水果额外奖励的金币数（原版：30）</summary>
    public int FruitBonusCoins { get; set; } = 15;

    /// <summary>多少金币兑换1条命（原版：100）</summary>
    public int CoinsPerLife { get; set; } = 50;
}
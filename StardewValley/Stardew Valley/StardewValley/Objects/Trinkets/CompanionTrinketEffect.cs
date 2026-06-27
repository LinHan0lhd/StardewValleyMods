using System;
using StardewValley.Companions;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001C0 RID: 448
	public class CompanionTrinketEffect : TrinketEffect
	{
		// Token: 0x06001FF1 RID: 8177 RVA: 0x0016D3CF File Offset: 0x0016B5CF
		public CompanionTrinketEffect(Trinket trinket) : base(trinket)
		{
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x0016D3D8 File Offset: 0x0016B5D8
		public override bool GenerateRandomStats(Trinket trinket)
		{
			Random r = Utility.CreateRandom((double)trinket.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			if (r.NextBool(0.2))
			{
				this.Variant = 0;
			}
			else if (r.NextBool(0.8))
			{
				this.Variant = r.Next(3);
			}
			else if (r.NextBool(0.8))
			{
				this.Variant = r.Next(3) + 3;
			}
			else
			{
				this.Variant = r.Next(2) + 6;
			}
			trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:frog_variant_" + this.Variant.ToString());
			return true;
		}

		// Token: 0x06001FF3 RID: 8179 RVA: 0x0016D4AC File Offset: 0x0016B6AC
		public override void Apply(Farmer farmer)
		{
			this.Companion = new HungryFrogCompanion(this.Variant);
			if (Game1.gameMode == 3)
			{
				farmer.AddCompanion(this.Companion);
			}
		}

		// Token: 0x06001FF4 RID: 8180 RVA: 0x0016D4D3 File Offset: 0x0016B6D3
		public override void Unapply(Farmer farmer)
		{
			farmer.RemoveCompanion(this.Companion);
		}

		// Token: 0x0400137A RID: 4986
		public int Variant;
	}
}

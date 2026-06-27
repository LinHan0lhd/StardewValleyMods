using System;
using Microsoft.Xna.Framework;
using StardewValley.Companions;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001C5 RID: 453
	public class TrinketEffect
	{
		// Token: 0x06002009 RID: 8201 RVA: 0x0016DF7E File Offset: 0x0016C17E
		public TrinketEffect(Trinket trinket)
		{
			this.Trinket = trinket;
		}

		// Token: 0x0600200A RID: 8202 RVA: 0x0016DF8D File Offset: 0x0016C18D
		public virtual void OnUse(Farmer farmer)
		{
		}

		// Token: 0x0600200B RID: 8203 RVA: 0x0016DF8F File Offset: 0x0016C18F
		public virtual void Apply(Farmer farmer)
		{
			if (this.Trinket.ItemId == "ParrotEgg")
			{
				this.Companion = new FlyingCompanion(1, -1);
				if (Game1.gameMode == 3)
				{
					farmer.AddCompanion(this.Companion);
				}
			}
		}

		// Token: 0x0600200C RID: 8204 RVA: 0x0016DFC9 File Offset: 0x0016C1C9
		public virtual void Unapply(Farmer farmer)
		{
			farmer.RemoveCompanion(this.Companion);
		}

		// Token: 0x0600200D RID: 8205 RVA: 0x0016DFD7 File Offset: 0x0016C1D7
		public virtual void OnFootstep(Farmer farmer)
		{
		}

		// Token: 0x0600200E RID: 8206 RVA: 0x0016DFD9 File Offset: 0x0016C1D9
		public virtual void OnReceiveDamage(Farmer farmer, int damageAmount)
		{
		}

		// Token: 0x0600200F RID: 8207 RVA: 0x0016DFDC File Offset: 0x0016C1DC
		public virtual void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
		{
			if (this.Trinket.ItemId == "ParrotEgg" && monster != null && monster.Health <= 0)
			{
				double chance = (double)(this.GeneralStat + 1) * 0.1;
				while (Game1.random.NextDouble() <= chance)
				{
					monster.objectsToDrop.Add("GoldCoin");
				}
			}
		}

		// Token: 0x06002010 RID: 8208 RVA: 0x0016E040 File Offset: 0x0016C240
		public virtual bool GenerateRandomStats(Trinket trinket)
		{
			Random r = Utility.CreateRandom((double)trinket.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			string itemId = trinket.ItemId;
			if (itemId == "IridiumSpur")
			{
				this.GeneralStat = r.Next(5, 11);
				trinket.descriptionSubstitutionTemplates.Clear();
				trinket.descriptionSubstitutionTemplates.Add(this.GeneralStat.ToString());
				return true;
			}
			if (!(itemId == "ParrotEgg"))
			{
				return false;
			}
			int maxLevel = Math.Min(4, (int)(1U + Game1.player.totalMoneyEarned / 750000U));
			int wasLevel = this.GeneralStat;
			this.GeneralStat = r.Next(0, maxLevel);
			trinket.descriptionSubstitutionTemplates.Clear();
			trinket.descriptionSubstitutionTemplates.Add((this.GeneralStat + 1).ToString());
			trinket.descriptionSubstitutionTemplates.Add(TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:ParrotEgg_Chance_" + this.GeneralStat.ToString()));
			return maxLevel > 1 || this.GeneralStat != wasLevel;
		}

		// Token: 0x06002011 RID: 8209 RVA: 0x0016E169 File Offset: 0x0016C369
		public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
		{
		}

		// Token: 0x0400138A RID: 5002
		public Trinket Trinket;

		// Token: 0x0400138B RID: 5003
		public int GeneralStat;

		// Token: 0x0400138C RID: 5004
		public Companion Companion;
	}
}

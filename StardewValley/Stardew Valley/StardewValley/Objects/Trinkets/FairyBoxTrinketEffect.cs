using System;
using Microsoft.Xna.Framework;
using StardewValley.Companions;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001C1 RID: 449
	public class FairyBoxTrinketEffect : TrinketEffect
	{
		// Token: 0x06001FF5 RID: 8181 RVA: 0x0016D4E1 File Offset: 0x0016B6E1
		public FairyBoxTrinketEffect(Trinket trinket) : base(trinket)
		{
		}

		// Token: 0x06001FF6 RID: 8182 RVA: 0x0016D500 File Offset: 0x0016B700
		public override bool GenerateRandomStats(Trinket trinket)
		{
			Random r = Utility.CreateRandom((double)trinket.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			int level = 1;
			if (r.NextBool(0.45))
			{
				level = 2;
			}
			else if (r.NextBool(0.25))
			{
				level = 3;
			}
			else if (r.NextBool(0.125))
			{
				level = 4;
			}
			else if (r.NextBool(0.0675))
			{
				level = 5;
			}
			this.HealDelay = (float)(5000 - level * 300);
			this.Power = 0.7f + (float)level * 0.1f;
			trinket.descriptionSubstitutionTemplates.Clear();
			trinket.descriptionSubstitutionTemplates.Add(level.ToString());
			return true;
		}

		// Token: 0x06001FF7 RID: 8183 RVA: 0x0016D5DD File Offset: 0x0016B7DD
		public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
		{
			this.DamageSinceLastHeal += damageAmount;
			base.OnDamageMonster(farmer, monster, damageAmount, isBomb, isCriticalHit);
		}

		// Token: 0x06001FF8 RID: 8184 RVA: 0x0016D5FA File Offset: 0x0016B7FA
		public override void OnReceiveDamage(Farmer farmer, int damageAmount)
		{
			this.DamageSinceLastHeal += damageAmount;
			base.OnReceiveDamage(farmer, damageAmount);
		}

		// Token: 0x06001FF9 RID: 8185 RVA: 0x0016D614 File Offset: 0x0016B814
		public override void Update(Farmer farmer, GameTime time, GameLocation location)
		{
			this.HealTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.HealTimer >= this.HealDelay)
			{
				if (farmer.health < farmer.maxHealth && this.DamageSinceLastHeal >= 0)
				{
					int healAmount = (int)Math.Min(Math.Pow((double)this.DamageSinceLastHeal, 0.33000001311302185), (double)((float)farmer.maxHealth / 10f));
					healAmount = (int)((float)healAmount * this.Power);
					healAmount += Game1.random.Next((int)((float)(-(float)healAmount) * 0.25f), (int)((float)healAmount * 0.25f) + 1);
					if (healAmount > 0)
					{
						farmer.health = Math.Min(farmer.maxHealth, farmer.health + healAmount);
						location.debris.Add(new Debris(healAmount, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
						Game1.playSound("fairy_heal", null);
						this.DamageSinceLastHeal = 0;
					}
				}
				this.HealTimer = 0f;
			}
			base.Update(farmer, time, location);
		}

		// Token: 0x06001FFA RID: 8186 RVA: 0x0016D730 File Offset: 0x0016B930
		public override void Apply(Farmer farmer)
		{
			this.HealTimer = 0f;
			this.DamageSinceLastHeal = 0;
			this.Companion = new FlyingCompanion(0, -1);
			if (Game1.gameMode == 3)
			{
				farmer.AddCompanion(this.Companion);
			}
			base.Apply(farmer);
		}

		// Token: 0x06001FFB RID: 8187 RVA: 0x0016D76C File Offset: 0x0016B96C
		public override void Unapply(Farmer farmer)
		{
			farmer.RemoveCompanion(this.Companion);
		}

		// Token: 0x0400137B RID: 4987
		public float HealTimer;

		// Token: 0x0400137C RID: 4988
		public float HealDelay = 4000f;

		// Token: 0x0400137D RID: 4989
		public float Power = 0.25f;

		// Token: 0x0400137E RID: 4990
		public int DamageSinceLastHeal;
	}
}

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001C2 RID: 450
	public class IceOrbTrinketEffect : TrinketEffect
	{
		// Token: 0x06001FFC RID: 8188 RVA: 0x0016D77A File Offset: 0x0016B97A
		public IceOrbTrinketEffect(Trinket trinket) : base(trinket)
		{
		}

		// Token: 0x06001FFD RID: 8189 RVA: 0x0016D799 File Offset: 0x0016B999
		public override void Apply(Farmer farmer)
		{
			this.ProjectileTimer = 0f;
			base.Apply(farmer);
		}

		// Token: 0x06001FFE RID: 8190 RVA: 0x0016D7B0 File Offset: 0x0016B9B0
		public override bool GenerateRandomStats(Trinket trinket)
		{
			Random r = Utility.CreateRandom((double)trinket.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			this.ProjectileDelay = (float)r.Next(3000, 5001);
			this.FreezeTime = r.Next(2000, 4001);
			if (r.NextDouble() < 0.05)
			{
				trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:PerfectIceRod");
				this.ProjectileDelay = 3000f;
				this.FreezeTime = 4000;
			}
			trinket.descriptionSubstitutionTemplates.Clear();
			trinket.descriptionSubstitutionTemplates.Add(Math.Round((double)(this.ProjectileDelay / 1000f), 1).ToString(CultureInfo.InvariantCulture));
			trinket.descriptionSubstitutionTemplates.Add(Math.Round((double)((float)this.FreezeTime / 1000f), 1).ToString(CultureInfo.InvariantCulture));
			return true;
		}

		// Token: 0x06001FFF RID: 8191 RVA: 0x0016D8C0 File Offset: 0x0016BAC0
		public override void Update(Farmer farmer, GameTime time, GameLocation location)
		{
			if (!Game1.shouldTimePass(false))
			{
				return;
			}
			this.ProjectileTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.ProjectileTimer >= this.ProjectileDelay)
			{
				Monster monster = Utility.findClosestMonsterWithinRange(location, farmer.getStandingPosition(), 600, false, null);
				if (monster != null)
				{
					Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), monster.getStandingPosition(), 5f);
					DebuffingProjectile p = new DebuffingProjectile("frozen", 17, 0, 0, 0f, motion.X, motion.Y, farmer.getStandingPosition() - new Vector2(32f, 48f), location, farmer, true, false);
					p.wavyMotion.Value = false;
					p.piercesLeft.Value = 99999;
					p.maxTravelDistance.Value = 3000;
					p.IgnoreLocationCollision = true;
					p.ignoreObjectCollisions.Value = true;
					p.maxVelocity.Value = 12f;
					p.projectileID.Value = 15;
					p.alpha.Value = 0.001f;
					p.alphaChange.Value = 0.05f;
					p.light.Value = true;
					p.debuffIntensity.Value = this.FreezeTime;
					p.boundingBoxWidth.Value = 32;
					location.projectiles.Add(p);
					location.playSound("fireball", null, null, SoundContext.Default);
				}
				this.ProjectileTimer = 0f;
			}
			base.Update(farmer, time, location);
		}

		// Token: 0x0400137F RID: 4991
		public const int Range = 600;

		// Token: 0x04001380 RID: 4992
		public float ProjectileTimer;

		// Token: 0x04001381 RID: 4993
		public float ProjectileDelay = 4000f;

		// Token: 0x04001382 RID: 4994
		public int FreezeTime = 4000;
	}
}

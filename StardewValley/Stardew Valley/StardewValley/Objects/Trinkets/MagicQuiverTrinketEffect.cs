using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001C3 RID: 451
	public class MagicQuiverTrinketEffect : TrinketEffect
	{
		// Token: 0x06002000 RID: 8192 RVA: 0x0016DA58 File Offset: 0x0016BC58
		public MagicQuiverTrinketEffect(Trinket trinket) : base(trinket)
		{
		}

		// Token: 0x06002001 RID: 8193 RVA: 0x0016DA7C File Offset: 0x0016BC7C
		public override void Apply(Farmer farmer)
		{
			this.ProjectileTimer = 0f;
			base.Apply(farmer);
		}

		// Token: 0x06002002 RID: 8194 RVA: 0x0016DA90 File Offset: 0x0016BC90
		public override bool GenerateRandomStats(Trinket trinket)
		{
			Random r = Utility.CreateRandom((double)trinket.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			if (r.NextBool(0.04))
			{
				trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:PerfectMagicQuiver");
				this.MinDamage = 30;
				this.MaxDamage = 35;
				this.ProjectileDelay = 900f;
			}
			else if (r.NextBool(0.1))
			{
				if (r.NextBool(0.5))
				{
					trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:RapidMagicQuiver");
					this.MinDamage = r.Next(10, 15);
					this.MinDamage -= 2;
					this.MaxDamage = this.MinDamage + 5;
					this.ProjectileDelay = (float)(600 + r.Next(11) * 10);
				}
				else
				{
					trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:HeavyMagicQuiver");
					this.MinDamage = r.Next(25, 41);
					this.MinDamage -= 2;
					this.MaxDamage = this.MinDamage + 5;
					this.ProjectileDelay = (float)(1500 + r.Next(6) * 100);
				}
			}
			else
			{
				this.MinDamage = r.Next(15, 31);
				this.MinDamage -= 2;
				this.MaxDamage = this.MinDamage + 5;
				this.ProjectileDelay = (float)(1100 + r.Next(11) * 100);
			}
			trinket.descriptionSubstitutionTemplates.Clear();
			trinket.descriptionSubstitutionTemplates.Add(Math.Round((double)this.ProjectileDelay / 1000.0, 2).ToString(CultureInfo.InvariantCulture));
			trinket.descriptionSubstitutionTemplates.Add(this.MinDamage.ToString());
			trinket.descriptionSubstitutionTemplates.Add(this.MaxDamage.ToString());
			return true;
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x0016DCA4 File Offset: 0x0016BEA4
		public override void Update(Farmer farmer, GameTime time, GameLocation location)
		{
			base.Update(farmer, time, location);
			if (!Game1.shouldTimePass(false))
			{
				return;
			}
			this.ProjectileTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.ProjectileTimer >= this.ProjectileDelay)
			{
				this.ProjectileTimer = 0f;
				HashSet<string> ignoreLocations = this.GetIgnoredLocations();
				if (ignoreLocations.Contains(location.NameOrUniqueName) || ignoreLocations.Contains(location.Name))
				{
					return;
				}
				HashSet<string> ignoreMonsterNames = this.GetIgnoredMonsterNames();
				Monster monster = Utility.findClosestMonsterWithinRange(location, farmer.getStandingPosition(), 500, true, (Monster m) => !ignoreMonsterNames.Contains(m.Name));
				if (monster != null)
				{
					Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), monster.getStandingPosition(), 2f);
					float projectileRotation = (float)Math.Atan2((double)motion.Y, (double)motion.X) + 1.5707964f;
					BasicProjectile p = new BasicProjectile(Game1.random.Next(this.MinDamage, this.MaxDamage + 1), 16, 0, 0, 0f, motion.X, motion.Y, farmer.getStandingPosition() - new Vector2(32f, 48f), null, null, null, false, true, location, farmer, null, null);
					p.IgnoreLocationCollision = true;
					p.ignoreObjectCollisions.Value = true;
					p.acceleration.Value = motion;
					p.maxVelocity.Value = 24f;
					p.projectileID.Value = 14;
					p.startingRotation.Value = projectileRotation;
					p.alpha.Value = 0.001f;
					p.alphaChange.Value = 0.05f;
					p.light.Value = true;
					p.collisionSound.Value = "magic_arrow_hit";
					location.projectiles.Add(p);
					location.playSound("magic_arrow", null, null, SoundContext.Default);
				}
			}
		}

		// Token: 0x06002004 RID: 8196 RVA: 0x0016DEA4 File Offset: 0x0016C0A4
		public HashSet<string> GetIgnoredLocations()
		{
			if (MagicQuiverTrinketEffect.CachedIgnoreLocations == null)
			{
				TrinketData trinketData = this.Trinket.GetTrinketData();
				string input;
				if (trinketData == null)
				{
					input = null;
				}
				else
				{
					Dictionary<string, string> customFields = trinketData.CustomFields;
					input = ((customFields != null) ? customFields.GetValueOrDefault("IgnoreLocations") : null);
				}
				MagicQuiverTrinketEffect.CachedIgnoreLocations = new HashSet<string>(ArgUtility.SplitQuoteAware(input, '/', StringSplitOptions.None, false), StringComparer.OrdinalIgnoreCase);
			}
			return MagicQuiverTrinketEffect.CachedIgnoreLocations;
		}

		// Token: 0x06002005 RID: 8197 RVA: 0x0016DF00 File Offset: 0x0016C100
		public HashSet<string> GetIgnoredMonsterNames()
		{
			if (MagicQuiverTrinketEffect.CachedIgnoreMonsters == null)
			{
				TrinketData trinketData = this.Trinket.GetTrinketData();
				string input;
				if (trinketData == null)
				{
					input = null;
				}
				else
				{
					Dictionary<string, string> customFields = trinketData.CustomFields;
					input = ((customFields != null) ? customFields.GetValueOrDefault("IgnoreMonsters") : null);
				}
				MagicQuiverTrinketEffect.CachedIgnoreMonsters = new HashSet<string>(ArgUtility.SplitQuoteAware(input, '/', StringSplitOptions.None, false), StringComparer.OrdinalIgnoreCase);
			}
			return MagicQuiverTrinketEffect.CachedIgnoreMonsters;
		}

		// Token: 0x04001383 RID: 4995
		public static HashSet<string> CachedIgnoreLocations;

		// Token: 0x04001384 RID: 4996
		public static HashSet<string> CachedIgnoreMonsters;

		// Token: 0x04001385 RID: 4997
		public const int Range = 500;

		// Token: 0x04001386 RID: 4998
		public float ProjectileTimer;

		// Token: 0x04001387 RID: 4999
		public float ProjectileDelay = 1000f;

		// Token: 0x04001388 RID: 5000
		public int MinDamage = 10;

		// Token: 0x04001389 RID: 5001
		public int MaxDamage = 10;
	}
}

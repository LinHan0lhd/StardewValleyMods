using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValley.Tools
{
	// Token: 0x0200012F RID: 303
	public class Pan : Tool
	{
		// Token: 0x06001885 RID: 6277 RVA: 0x001211C6 File Offset: 0x0011F3C6
		public Pan() : base("Copper Pan", 1, 12, 12, false, 0)
		{
		}

		// Token: 0x06001886 RID: 6278 RVA: 0x001211E6 File Offset: 0x0011F3E6
		public Pan(int upgradeLevel) : base("Copper Pan", upgradeLevel, 12, 12, false, 0)
		{
		}

		// Token: 0x06001887 RID: 6279 RVA: 0x00121206 File Offset: 0x0011F406
		protected override Item GetOneNew()
		{
			if (this.upgradeLevel.Value == -1)
			{
				base.UpgradeLevel = 1;
			}
			return new Pan(base.UpgradeLevel);
		}

		// Token: 0x06001888 RID: 6280 RVA: 0x00121228 File Offset: 0x0011F428
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.finishEvent, "finishEvent");
			this.finishEvent.onEvent += this.doFinish;
		}

		// Token: 0x06001889 RID: 6281 RVA: 0x00121260 File Offset: 0x0011F460
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			if (this.upgradeLevel.Value <= 0)
			{
				base.UpgradeLevel = 1;
			}
			base.CurrentParentTileIndex = 12;
			base.IndexOfMenuItemView = 12;
			int reach = 4;
			if (base.hasEnchantmentOfType<ReachingToolEnchantment>())
			{
				reach++;
			}
			bool overrideCheck = false;
			Rectangle orePanRect = new Rectangle(location.orePanPoint.X * 64 - (int)(64f * ((float)reach / 2f)), location.orePanPoint.Y * 64 - (int)(64f * ((float)reach / 2f)), 64 * reach, 64 * reach);
			Point playerPixel = who.StandingPixel;
			if (orePanRect.Contains(x, y) && Utility.distance((float)playerPixel.X, (float)orePanRect.Center.X, (float)playerPixel.Y, (float)orePanRect.Center.Y) <= (float)(reach * 64))
			{
				overrideCheck = true;
			}
			who.lastClick = Vector2.Zero;
			x = (int)who.GetToolLocation(false).X;
			y = (int)who.GetToolLocation(false).Y;
			who.lastClick = new Vector2((float)x, (float)y);
			if (location.orePanPoint != null && !location.orePanPoint.Equals(Point.Zero))
			{
				Rectangle panRect = who.GetBoundingBox();
				if (overrideCheck || panRect.Intersects(orePanRect))
				{
					who.faceDirection(2);
					who.FarmerSprite.animateOnce(303, 50f, 4);
					return true;
				}
			}
			who.forceCanMove();
			return true;
		}

		// Token: 0x0600188A RID: 6282 RVA: 0x001213D8 File Offset: 0x0011F5D8
		public static void playSlosh(Farmer who)
		{
			who.playNearbySoundLocal("slosh", null, SoundContext.Default);
		}

		// Token: 0x0600188B RID: 6283 RVA: 0x001213FA File Offset: 0x0011F5FA
		public override void tickUpdate(GameTime time, Farmer who)
		{
			this.lastUser = who;
			base.tickUpdate(time, who);
			this.finishEvent.Poll();
		}

		// Token: 0x0600188C RID: 6284 RVA: 0x00121418 File Offset: 0x0011F618
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			Vector2 toolLocation = who.GetToolLocation(false);
			x = (int)toolLocation.X;
			y = (int)toolLocation.Y;
			base.CurrentParentTileIndex = 12;
			base.IndexOfMenuItemView = 12;
			location.localSound("coin", new Vector2?(toolLocation / 64f), null, SoundContext.Default);
			who.addItemsByMenuIfNecessary(this.getPanItems(location, who), null, false);
			location.orePanPoint.Value = Point.Zero;
			int i = 0;
			while (i < this.upgradeLevel.Value - 1 && !location.performOrePanTenMinuteUpdate(Game1.random) && (Game1.random.NextDouble() >= 0.5 || !location.performOrePanTenMinuteUpdate(Game1.random) || location is IslandNorth))
			{
				i++;
			}
			this.finish();
		}

		// Token: 0x0600188D RID: 6285 RVA: 0x001214F9 File Offset: 0x0011F6F9
		private void finish()
		{
			this.finishEvent.Fire();
		}

		// Token: 0x0600188E RID: 6286 RVA: 0x00121506 File Offset: 0x0011F706
		private void doFinish()
		{
			this.lastUser.CanMove = true;
			this.lastUser.UsingTool = false;
			this.lastUser.canReleaseTool = true;
		}

		// Token: 0x0600188F RID: 6287 RVA: 0x0012152C File Offset: 0x0011F72C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.IndexOfMenuItemView = 12;
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		// Token: 0x06001890 RID: 6288 RVA: 0x00121554 File Offset: 0x0011F754
		public List<Item> getPanItems(GameLocation location, Farmer who)
		{
			List<Item> items = new List<Item>();
			string whichOre = "378";
			who.stats.Increment("TimesPanned", 1);
			Random r = Utility.CreateRandom((double)location.orePanPoint.X, (double)location.orePanPoint.Y * 1000.0, Game1.stats.DaysPlayed, who.stats.Get("TimesPanned") * 77U, 0.0);
			double roll = r.NextDouble() - (double)who.luckLevel.Value * 0.001 - who.DailyLuck;
			roll -= (double)(this.upgradeLevel.Value - 1) * 0.05;
			if (roll < 0.01)
			{
				whichOre = "386";
			}
			else if (roll < 0.241)
			{
				whichOre = "384";
			}
			else if (roll < 0.6)
			{
				whichOre = "380";
			}
			if (whichOre != "386" && r.NextDouble() < 0.1 + (base.hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.1 : 0.0))
			{
				whichOre = "881";
			}
			int orePieces = r.Next(2, 7) + 1 + (int)((r.NextDouble() + 0.1 + (double)((float)who.luckLevel.Value / 10f) + who.DailyLuck) * 2.0);
			int extraPieces = r.Next(5) + 1 + (int)((r.NextDouble() + 0.1 + (double)((float)who.luckLevel.Value / 10f)) * 2.0);
			orePieces += this.upgradeLevel.Value - 1;
			roll = r.NextDouble() - who.DailyLuck;
			int numRolls = this.upgradeLevel.Value;
			bool gotRing = false;
			double extraChance = (double)(this.upgradeLevel.Value - 1) * 0.04;
			if (this.enchantments.Count > 0)
			{
				extraChance *= 1.25;
			}
			if (base.hasEnchantmentOfType<GenerousEnchantment>())
			{
				numRolls += 2;
			}
			while (r.NextDouble() - who.DailyLuck < 0.4 + (double)who.LuckLevel * 0.04 + extraChance && numRolls > 0)
			{
				roll = r.NextDouble() - who.DailyLuck;
				roll -= (double)(this.upgradeLevel.Value - 1) * 0.005;
				string whichExtra = "382";
				if (roll < 0.02 + (double)who.LuckLevel * 0.002 && r.NextDouble() < 0.75)
				{
					whichExtra = "72";
					extraPieces = 1;
				}
				else if (roll < 0.1 && r.NextDouble() < 0.75)
				{
					whichExtra = (60 + r.Next(5) * 2).ToString();
					extraPieces = 1;
				}
				else if (roll < 0.36)
				{
					whichExtra = "749";
					extraPieces = Math.Max(1, extraPieces / 2);
				}
				else if (roll < 0.5)
				{
					whichExtra = r.Choose("82", "84", "86");
					extraPieces = 1;
				}
				if (roll < (double)who.LuckLevel * 0.002 && !gotRing && r.NextDouble() < 0.33)
				{
					items.Add(new Ring("859"));
					gotRing = true;
				}
				if (roll < 0.01 && r.NextDouble() < 0.5)
				{
					items.Add(Utility.getRandomCosmeticItem(r));
				}
				if (r.NextDouble() < 0.1 && base.hasEnchantmentOfType<FisherEnchantment>())
				{
					Item f = location.getFish(1f, null, r.Next(1, 6), who, 0.0, who.Tile, null);
					if (f != null && f.Category == -4)
					{
						items.Add(f);
					}
				}
				if (r.NextDouble() < 0.02 + (base.hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.05 : 0.0))
				{
					Item artifact = location.tryGetRandomArtifactFromThisLocation(who, r, 1.0);
					if (artifact != null)
					{
						items.Add(artifact);
					}
				}
				if (Utility.tryRollMysteryBox(0.05, r))
				{
					items.Add(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 1, 0, false));
				}
				if (whichExtra != null)
				{
					items.Add(new Object(whichExtra, extraPieces, false, -1, 0));
				}
				numRolls--;
			}
			int amount = 0;
			while (r.NextDouble() < 0.05 + (base.hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.15 : 0.0))
			{
				amount++;
			}
			if (amount > 0)
			{
				items.Add(ItemRegistry.Create("(O)275", amount, 0, false));
			}
			items.Add(new Object(whichOre, orePieces, false, -1, 0));
			IslandNorth islandNorth = location as IslandNorth;
			if (islandNorth == null)
			{
				if (location is IslandLocation)
				{
					if (r.NextDouble() < 0.2)
					{
						items.Add(ItemRegistry.Create("(O)831", r.Next(2, 6), 0, false));
					}
				}
			}
			else if (islandNorth.bridgeFixed.Value && r.NextDouble() < 0.2)
			{
				items.Add(ItemRegistry.Create("(O)822", 1, 0, false));
			}
			if (who != null)
			{
				who.gainExperience(3, orePieces + extraPieces);
				who.gainExperience(2, items.Count * 7);
			}
			return items;
		}

		// Token: 0x04000EE5 RID: 3813
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0(false);
	}
}

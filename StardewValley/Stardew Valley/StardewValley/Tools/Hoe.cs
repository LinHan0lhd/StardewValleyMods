using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace StardewValley.Tools
{
	// Token: 0x0200012B RID: 299
	public class Hoe : Tool
	{
		// Token: 0x06001837 RID: 6199 RVA: 0x0011B293 File Offset: 0x00119493
		public Hoe() : base("Hoe", 0, 21, 47, false, 0)
		{
		}

		// Token: 0x06001838 RID: 6200 RVA: 0x0011B2A8 File Offset: 0x001194A8
		protected override void MigrateLegacyItemId()
		{
			switch (base.UpgradeLevel)
			{
			case 0:
				base.ItemId = "Hoe";
				return;
			case 1:
				base.ItemId = "CopperHoe";
				return;
			case 2:
				base.ItemId = "SteelHoe";
				return;
			case 3:
				base.ItemId = "GoldHoe";
				return;
			case 4:
				base.ItemId = "IridiumHoe";
				return;
			default:
				base.ItemId = "Hoe";
				return;
			}
		}

		// Token: 0x06001839 RID: 6201 RVA: 0x0011B31F File Offset: 0x0011951F
		protected override Item GetOneNew()
		{
			return new Hoe();
		}

		// Token: 0x0600183A RID: 6202 RVA: 0x0011B328 File Offset: 0x00119528
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			Vector2 initialTile = new Vector2((float)(x / 64), (float)(y / 64));
			base.DoFunction(location, x, y, power, who);
			if (MineShaft.IsGeneratedLevel(location))
			{
				power = 1;
			}
			if (!this.isEfficient.Value)
			{
				who.Stamina -= (float)(2 * power) - (float)who.FarmingLevel * 0.1f;
			}
			power = who.toolPower.Value;
			who.stopJittering();
			if (this.PlayUseSounds)
			{
				location.playSound("woodyHit", new Vector2?(initialTile), null, SoundContext.Default);
			}
			List<Vector2> tileLocations = base.tilesAffected(initialTile, power, who);
			foreach (Vector2 tileLocation in tileLocations)
			{
				TerrainFeature terrainFeature;
				if (location.terrainFeatures.TryGetValue(tileLocation, out terrainFeature))
				{
					if (terrainFeature.performToolAction(this, 0, tileLocation))
					{
						location.terrainFeatures.Remove(tileLocation);
					}
				}
				else
				{
					Object obj;
					if (location.objects.TryGetValue(tileLocation, out obj) && obj.performToolAction(this))
					{
						if (obj.Type == "Crafting" && obj.fragility.Value != 2)
						{
							location.debris.Add(new Debris(obj.QualifiedItemId, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel)));
						}
						obj.performRemoveAction();
						location.Objects.Remove(tileLocation);
					}
					if (location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back", false) != null)
					{
						if (location is MineShaft && !location.IsTileOccupiedBy(tileLocation, CollisionMask.All, CollisionMask.None, true))
						{
							if (location.makeHoeDirt(tileLocation, false))
							{
								if (this.PlayUseSounds)
								{
									location.playSound("hoeHit", new Vector2?(tileLocation), null, SoundContext.Default);
								}
								location.checkForBuriedItem((int)tileLocation.X, (int)tileLocation.Y, false, false, who);
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(12, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
								});
								if (tileLocations.Count > 2)
								{
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(6, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), Vector2.Distance(initialTile, tileLocation) * 30f, 0, -1, -1f, -1, 0)
									});
								}
							}
						}
						else if (location.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && location.makeHoeDirt(tileLocation, false))
						{
							if (this.PlayUseSounds)
							{
								location.playSound("hoeHit", new Vector2?(tileLocation), null, SoundContext.Default);
							}
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							});
							if (tileLocations.Count > 2)
							{
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(6, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), Vector2.Distance(initialTile, tileLocation) * 30f, 0, -1, -1f, -1, 0)
								});
							}
							location.checkForBuriedItem((int)tileLocation.X, (int)tileLocation.Y, false, false, who);
						}
						Stats stats = Game1.stats;
						uint dirtHoed = stats.DirtHoed;
						stats.DirtHoed = dirtHoed + 1U;
					}
				}
			}
		}
	}
}

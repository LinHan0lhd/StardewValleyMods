using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002DA RID: 730
	public class IslandLocation : GameLocation
	{
		// Token: 0x06002FFE RID: 12286 RVA: 0x0025EDC4 File Offset: 0x0025CFC4
		public IslandLocation()
		{
		}

		// Token: 0x06002FFF RID: 12287 RVA: 0x0025EE04 File Offset: 0x0025D004
		public void ApplyUnsafeMapOverride(string override_map, Microsoft.Xna.Framework.Rectangle? source_rect, Microsoft.Xna.Framework.Rectangle dest_rect)
		{
			base.ApplyMapOverride(override_map, source_rect, new Microsoft.Xna.Framework.Rectangle?(dest_rect));
			Microsoft.Xna.Framework.Rectangle nontile_rect = new Microsoft.Xna.Framework.Rectangle(dest_rect.X * 64, dest_rect.Y * 64, dest_rect.Width * 64, dest_rect.Height * 64);
			if (this == Game1.player.currentLocation)
			{
				Microsoft.Xna.Framework.Rectangle playerBounds = Game1.player.GetBoundingBox();
				if (nontile_rect.Intersects(playerBounds) && this.isCollidingPosition(playerBounds, Game1.viewport, true, 0, false, Game1.player))
				{
					Game1.player.TemporaryPassableTiles.Add(nontile_rect);
				}
			}
		}

		// Token: 0x06003000 RID: 12288 RVA: 0x0025EE92 File Offset: 0x0025D092
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.parrotUpgradePerches, "parrotUpgradePerches").AddField(this.buriedNutPoints, "buriedNutPoints").AddField(this.locationGemBird, "locationGemBird");
		}

		// Token: 0x06003001 RID: 12289 RVA: 0x0025EED1 File Offset: 0x0025D0D1
		public override string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName, bool ignoreTileSheetProperties = false)
		{
			if (layerName == "Back" && propertyName == "Diggable" && this.IsBuriedNutLocation(new Point(xTile, yTile)))
			{
				return "T";
			}
			return base.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ignoreTileSheetProperties);
		}

		// Token: 0x06003002 RID: 12290 RVA: 0x0025EF10 File Offset: 0x0025D110
		public virtual void SetBuriedNutLocations()
		{
		}

		// Token: 0x06003003 RID: 12291 RVA: 0x0025EF12 File Offset: 0x0025D112
		public virtual List<Vector2> GetAdditionalWalnutBushes()
		{
			return null;
		}

		// Token: 0x06003004 RID: 12292 RVA: 0x0025EF18 File Offset: 0x0025D118
		public IslandLocation(string map, string name) : base(map, name)
		{
			this.SetBuriedNutLocations();
			foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
			{
				Bush bush = largeTerrainFeature as Bush;
				if (bush != null)
				{
					bush.setUpSourceRect();
				}
			}
		}

		// Token: 0x06003005 RID: 12293 RVA: 0x0025EFB8 File Offset: 0x0025D1B8
		public override bool SeedsIgnoreSeasonsHere()
		{
			return true;
		}

		// Token: 0x06003006 RID: 12294 RVA: 0x0025EFBC File Offset: 0x0025D1BC
		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			string text;
			FishAreaData fishAreaData;
			return !this.TryGetFishAreaForTile(new Vector2((float)x, (float)y), out text, out fishAreaData);
		}

		// Token: 0x06003007 RID: 12295 RVA: 0x0025EFE0 File Offset: 0x0025D1E0
		public override bool answerDialogue(Response answer)
		{
			using (List<ParrotPlatform>.Enumerator enumerator = this.parrotPlatforms.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.AnswerQuestion(answer))
					{
						return true;
					}
				}
			}
			using (NetList<ParrotUpgradePerch, NetRef<ParrotUpgradePerch>>.Enumerator enumerator2 = this.parrotUpgradePerches.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.AnswerQuestion(answer))
					{
						return true;
					}
				}
			}
			return base.answerDialogue(answer);
		}

		// Token: 0x06003008 RID: 12296 RVA: 0x0025F088 File Offset: 0x0025D288
		public override void cleanupBeforePlayerExit()
		{
			foreach (ParrotPlatform parrotPlatform in this.parrotPlatforms)
			{
				parrotPlatform.Cleanup();
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.Cleanup();
			}
			this._dayParallaxTexture = null;
			this._nightParallaxTexture = null;
			this.underwaterSprites.Clear();
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06003009 RID: 12297 RVA: 0x0025F138 File Offset: 0x0025D338
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
		{
			using (List<ParrotPlatform>.Enumerator enumerator = this.parrotPlatforms.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CheckCollisions(position))
					{
						return true;
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement, false);
		}

		// Token: 0x0600300A RID: 12298 RVA: 0x0025F1A8 File Offset: 0x0025D3A8
		protected void addMoonlightJellies(int numTries, Random r, Microsoft.Xna.Framework.Rectangle exclusionRect)
		{
			for (int i = 0; i < numTries; i++)
			{
				Point tile = new Point(r.Next(base.Map.Layers[0].LayerWidth), r.Next(base.Map.Layers[0].LayerHeight));
				if (base.isOpenWater(tile.X, tile.Y) && !exclusionRect.Contains(tile) && FishingRod.distanceToLand(tile.X, tile.Y, this, false) >= 2)
				{
					bool tooClose = false;
					foreach (TemporaryAnimatedSprite t in this.underwaterSprites)
					{
						Point otherTile = new Point((int)t.position.X / 64, (int)t.position.Y / 64);
						if (Utility.distance((float)tile.X, (float)otherTile.X, (float)tile.Y, (float)otherTile.Y) <= 2f)
						{
							tooClose = true;
							break;
						}
					}
					if (!tooClose)
					{
						TemporaryAnimatedSpriteList temporaryAnimatedSpriteList = this.underwaterSprites;
						TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle((r.NextDouble() < 0.2) ? 304 : 256, (r.NextDouble() < 0.01) ? 32 : 16, 16, 16), 250f, 3, 9999, new Vector2((float)tile.X, (float)tile.Y) * 64f, false, false, 0.1f, 0f, Color.White * 0.66f, 4f, 0f, 0f, 0f, false);
						temporaryAnimatedSprite.yPeriodic = (Game1.random.NextDouble() < 0.76);
						temporaryAnimatedSprite.yPeriodicRange = 12f;
						temporaryAnimatedSprite.yPeriodicLoopTime = (float)Game1.random.Next(5500, 8000);
						temporaryAnimatedSprite.xPeriodic = (Game1.random.NextDouble() < 0.76);
						temporaryAnimatedSprite.xPeriodicLoopTime = (float)Game1.random.Next(5500, 8000);
						temporaryAnimatedSprite.xPeriodicRange = 16f;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
						defaultInterpolatedStringHandler.AppendFormatted(base.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral("_MoonlightJelly_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(tile.X);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(tile.Y);
						temporaryAnimatedSprite.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
						temporaryAnimatedSprite.lightcolor = Color.Black;
						temporaryAnimatedSprite.lightRadius = 1f;
						temporaryAnimatedSprite.pingPong = true;
						temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite);
					}
				}
			}
		}

		// Token: 0x0600300B RID: 12299 RVA: 0x0025F47C File Offset: 0x0025D67C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (Game1.currentLocation == this)
			{
				foreach (ParrotPlatform parrotPlatform in this.parrotPlatforms)
				{
					parrotPlatform.Update(time);
				}
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.Update(time);
			}
			this.underwaterSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			base.UpdateWhenCurrentLocation(time);
		}

		// Token: 0x0600300C RID: 12300 RVA: 0x0025F550 File Offset: 0x0025D750
		public override void tryToAddCritters(bool onlyIfOnScreen = false)
		{
			if (Game1.random.NextDouble() < 0.20000000298023224 && !base.IsRainingHere() && !Game1.isDarkOut(this))
			{
				Vector2 origin;
				if (Game1.random.NextDouble() < 0.75)
				{
					origin = new Vector2((float)Game1.viewport.X + Utility.RandomFloat(0f, (float)Game1.viewport.Width, null), (float)(Game1.viewport.Y - 64));
				}
				else
				{
					origin = new Vector2((float)(Game1.viewport.X + Game1.viewport.Width + 64), Utility.RandomFloat(0f, (float)Game1.viewport.Height, null));
				}
				int parrots_to_spawn = 1;
				if (Game1.random.NextBool())
				{
					parrots_to_spawn++;
				}
				if (Game1.random.NextBool())
				{
					parrots_to_spawn++;
				}
				for (int i = 0; i < parrots_to_spawn; i++)
				{
					base.addCritter(new OverheadParrot(origin + new Vector2((float)(i * 64), (float)(-(float)i * 64))));
				}
			}
			if (!base.IsRainingHere())
			{
				double mapArea = (double)(this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight);
				double butterflyChance = Math.Max(0.1, Math.Min(0.25, mapArea / 15000.0));
				base.addButterflies(butterflyChance, onlyIfOnScreen);
			}
		}

		// Token: 0x0600300D RID: 12301 RVA: 0x0025F6C7 File Offset: 0x0025D8C7
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.locationGemBird.Value = null;
		}

		// Token: 0x0600300E RID: 12302 RVA: 0x0025F6DC File Offset: 0x0025D8DC
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.UpdateEvenIfFarmerIsntHere(time);
			}
			if (this.locationGemBird.Value != null && this.locationGemBird.Value.Update(time, this) && Game1.IsMasterGame)
			{
				this.locationGemBird.Value = null;
			}
		}

		// Token: 0x0600300F RID: 12303 RVA: 0x0025F76C File Offset: 0x0025D96C
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.UpdateCompletionStatus();
			}
			IslandLocation islandLocation = l as IslandLocation;
			if (islandLocation != null)
			{
				this.locationGemBird.Value = islandLocation.locationGemBird.Value;
			}
		}

		// Token: 0x06003010 RID: 12304 RVA: 0x0025F7E4 File Offset: 0x0025D9E4
		public void AddAdditionalWalnutBushes()
		{
			List<Vector2> additional_bushes = this.GetAdditionalWalnutBushes();
			if (additional_bushes != null)
			{
				foreach (Vector2 point in additional_bushes)
				{
					Bush bush = base.getLargeTerrainFeatureAt((int)point.X, (int)point.Y) as Bush;
					if (bush == null || bush.size.Value != 4)
					{
						this.largeTerrainFeatures.Add(new Bush(new Vector2((float)((int)point.X), (float)((int)point.Y)), 4, this, -1));
					}
				}
			}
		}

		// Token: 0x06003011 RID: 12305 RVA: 0x0025F888 File Offset: 0x0025DA88
		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			foreach (ParrotUpgradePerch perch in this.parrotUpgradePerches)
			{
				if (perch.IsAtTile(xTile, yTile) && perch.IsAvailable(true) && perch.parrotPresent)
				{
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		// Token: 0x06003012 RID: 12306 RVA: 0x0025F900 File Offset: 0x0025DB00
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (this.IsBuriedNutLocation(new Point(xLocation, yLocation)))
			{
				Game1.player.team.MarkCollectedNut(string.Concat(new string[]
				{
					"Buried_",
					base.Name,
					"_",
					xLocation.ToString(),
					"_",
					yLocation.ToString()
				}));
				Game1.multiplayer.broadcastNutDig(this, new Point(xLocation, yLocation));
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		// Token: 0x06003013 RID: 12307 RVA: 0x0025F990 File Offset: 0x0025DB90
		public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random r = Utility.CreateDaySaveRandom((double)(xLocation * 2000), (double)yLocation, 0.0);
			string toDigUp = null;
			int stack = 1;
			if (Game1.netWorldState.Value.GoldenCoconutCracked && r.NextDouble() < 0.1)
			{
				toDigUp = "(O)791";
			}
			else if (r.NextDouble() < 0.33)
			{
				toDigUp = "(O)831";
				stack = r.Next(2, 5);
			}
			else if (r.NextDouble() < 0.15)
			{
				toDigUp = "(O)275";
				stack = r.Next(1, 3);
			}
			if (toDigUp != null)
			{
				for (int i = 0; i < stack; i++)
				{
					Game1.createItemDebris(ItemRegistry.Create(toDigUp, 1, 0, false), new Vector2((float)xLocation, (float)yLocation) * 64f, -1, this, -1, false);
				}
			}
			base.digUpArtifactSpot(xLocation, yLocation, who);
		}

		// Token: 0x06003014 RID: 12308 RVA: 0x0025FA64 File Offset: 0x0025DC64
		public virtual bool IsBuriedNutLocation(Point point)
		{
			using (NetList<Point, NetPoint>.Enumerator enumerator = this.buriedNutPoints.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == point)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06003015 RID: 12309 RVA: 0x0025FAC0 File Offset: 0x0025DCC0
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			using (NetList<ParrotUpgradePerch, NetRef<ParrotUpgradePerch>>.Enumerator enumerator = this.parrotUpgradePerches.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CheckAction(tileLocation, who))
					{
						return true;
					}
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06003016 RID: 12310 RVA: 0x0025FB24 File Offset: 0x0025DD24
		public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.stats.TimesFished, Game1.uniqueIDForThisGame, 0.0, 0.0).NextDouble() < 0.15)
			{
				int foundCount;
				if (!Game1.player.team.limitedNutDrops.TryGetValue("IslandFishing", out foundCount))
				{
					foundCount = 0;
				}
				if (foundCount < 5)
				{
					if (!Game1.IsMultiplayer)
					{
						Game1.player.team.limitedNutDrops["IslandFishing"] = foundCount + 1;
						return ItemRegistry.Create("(O)73", 1, 0, false);
					}
					Game1.player.team.RequestLimitedNutDrops("IslandFishing", this, (int)bobberTile.X * 64, (int)bobberTile.Y * 64, 5, 1);
					return null;
				}
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		// Token: 0x06003017 RID: 12311 RVA: 0x0025FC10 File Offset: 0x0025DE10
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (ParrotPlatform parrotPlatform in this.parrotPlatforms)
			{
				parrotPlatform.Draw(b);
			}
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.Draw(b);
			}
			IslandGemBird value = this.locationGemBird.Value;
			if (value == null)
			{
				return;
			}
			value.Draw(b);
		}

		// Token: 0x06003018 RID: 12312 RVA: 0x0025FCC0 File Offset: 0x0025DEC0
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.DrawAboveAlwaysFrontLayer(b);
			}
		}

		// Token: 0x06003019 RID: 12313 RVA: 0x0025FD18 File Offset: 0x0025DF18
		public override bool IsLocationSpecificOccupantOnTile(Vector2 tileLocation)
		{
			using (List<ParrotPlatform>.Enumerator enumerator = this.parrotPlatforms.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.OccupiesTile(tileLocation))
					{
						return true;
					}
				}
			}
			return base.IsLocationSpecificOccupantOnTile(tileLocation);
		}

		// Token: 0x0600301A RID: 12314 RVA: 0x0025FD78 File Offset: 0x0025DF78
		protected override void resetLocalState()
		{
			this.parrotPlatforms.Clear();
			this.parrotPlatforms = ParrotPlatform.CreateParrotPlatformsForArea(this);
			foreach (ParrotUpgradePerch parrotUpgradePerch in this.parrotUpgradePerches)
			{
				parrotUpgradePerch.ResetForPlayerEntry();
			}
			base.resetLocalState();
		}

		// Token: 0x0600301B RID: 12315 RVA: 0x0025FDE8 File Offset: 0x0025DFE8
		public override void seasonUpdate(bool onLoad = false)
		{
		}

		// Token: 0x0600301C RID: 12316 RVA: 0x0025FDEA File Offset: 0x0025DFEA
		public override void updateSeasonalTileSheets(Map map = null)
		{
		}

		// Token: 0x0600301D RID: 12317 RVA: 0x0025FDEC File Offset: 0x0025DFEC
		public override void drawWater(SpriteBatch b)
		{
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.underwaterSprites)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			base.drawWater(b);
		}

		// Token: 0x0600301E RID: 12318 RVA: 0x0025FE48 File Offset: 0x0025E048
		public virtual void DrawParallaxHorizon(SpriteBatch b, bool horizontal_parallax = true)
		{
			float draw_zoom = 4f;
			if (this._dayParallaxTexture == null || this._dayParallaxTexture.IsDisposed)
			{
				this._dayParallaxTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cloudy_Ocean_BG");
			}
			if (this._nightParallaxTexture == null || this._nightParallaxTexture.IsDisposed)
			{
				this._nightParallaxTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cloudy_Ocean_BG_Night");
			}
			float horizontal_parallax_amount = (float)this._dayParallaxTexture.Width * draw_zoom - (float)this.map.DisplayWidth;
			float t = 0f;
			int background_y_adjustment = -640;
			int y = (int)((float)Game1.viewport.Y * 0.2f + (float)background_y_adjustment);
			if (horizontal_parallax)
			{
				if (this.map.DisplayWidth - Game1.viewport.Width < 0)
				{
					t = 0.5f;
				}
				else if (this.map.DisplayWidth - Game1.viewport.Width > 0)
				{
					t = (float)Game1.viewport.X / (float)(this.map.DisplayWidth - Game1.viewport.Width);
				}
			}
			else
			{
				t = 0.5f;
			}
			if (Game1.game1.takingMapScreenshot)
			{
				y = background_y_adjustment;
				t = 0.5f;
			}
			float arc = 0.25f;
			t = Utility.Lerp(0.5f + arc, 0.5f - arc, t);
			float day_night_transition = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay + (int)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameMinute % 10f) - Game1.getStartingToGetDarkTime(this)) / (float)Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime(this) - Game1.getStartingToGetDarkTime(this));
			day_night_transition = Utility.Clamp(day_night_transition, 0f, 1f);
			b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle(0, 0, this.map.DisplayWidth, this.map.DisplayHeight)), new Color(1, 122, 217, 255));
			b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle(0, 0, this.map.DisplayWidth, this.map.DisplayHeight)), new Color(0, 7, 63, 255) * day_night_transition);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)(-horizontal_parallax_amount * t), y, (int)((float)this._dayParallaxTexture.Width * draw_zoom), (int)((float)this._dayParallaxTexture.Height * draw_zoom));
			Microsoft.Xna.Framework.Rectangle source_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, this._dayParallaxTexture.Width, this._dayParallaxTexture.Height);
			int left_boundary = 0;
			if (rectangle.X < left_boundary)
			{
				int offset = left_boundary - rectangle.X;
				rectangle.X += offset;
				rectangle.Width -= offset;
				source_rect.X += (int)((float)offset / draw_zoom);
				source_rect.Width -= (int)((float)offset / draw_zoom);
			}
			int right_boundary = this.map.DisplayWidth;
			if (rectangle.X + rectangle.Width > right_boundary)
			{
				int offset2 = rectangle.X + rectangle.Width - right_boundary;
				rectangle.Width -= offset2;
				source_rect.Width -= (int)((float)offset2 / draw_zoom);
			}
			if (source_rect.Width > 0 && rectangle.Width > 0)
			{
				b.Draw(this._dayParallaxTexture, Game1.GlobalToLocal(Game1.viewport, rectangle), new Microsoft.Xna.Framework.Rectangle?(source_rect), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				b.Draw(this._nightParallaxTexture, Game1.GlobalToLocal(Game1.viewport, rectangle), new Microsoft.Xna.Framework.Rectangle?(source_rect), Color.White * day_night_transition, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
		}

		// Token: 0x0600301F RID: 12319 RVA: 0x002601DB File Offset: 0x0025E3DB
		public bool AreMoonlightJelliesOut()
		{
			return Game1.IsWinter && (!base.IsOutdoors || (!base.IsRainingHere() && Game1.isDarkOut(this)));
		}

		// Token: 0x04002081 RID: 8321
		public const int TOTAL_WALNUTS = 130;

		// Token: 0x04002082 RID: 8322
		[XmlIgnore]
		public List<ParrotPlatform> parrotPlatforms = new List<ParrotPlatform>();

		// Token: 0x04002083 RID: 8323
		[XmlIgnore]
		public NetList<ParrotUpgradePerch, NetRef<ParrotUpgradePerch>> parrotUpgradePerches = new NetList<ParrotUpgradePerch, NetRef<ParrotUpgradePerch>>();

		// Token: 0x04002084 RID: 8324
		[XmlIgnore]
		public NetList<Point, NetPoint> buriedNutPoints = new NetList<Point, NetPoint>();

		// Token: 0x04002085 RID: 8325
		[XmlElement("locationGemBird")]
		public NetRef<IslandGemBird> locationGemBird = new NetRef<IslandGemBird>();

		// Token: 0x04002086 RID: 8326
		[XmlIgnore]
		protected Texture2D _dayParallaxTexture;

		// Token: 0x04002087 RID: 8327
		[XmlIgnore]
		protected Texture2D _nightParallaxTexture;

		// Token: 0x04002088 RID: 8328
		[XmlIgnore]
		protected TemporaryAnimatedSpriteList underwaterSprites = new TemporaryAnimatedSpriteList();
	}
}

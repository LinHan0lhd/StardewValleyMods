using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley.Buildings
{
	// Token: 0x02000386 RID: 902
	public class JunimoHut : Building
	{
		// Token: 0x060037B2 RID: 14258 RVA: 0x002C2B34 File Offset: 0x002C0D34
		public JunimoHut(Vector2 tileLocation) : base("Junimo Hut", tileLocation)
		{
		}

		// Token: 0x060037B3 RID: 14259 RVA: 0x002C2BC3 File Offset: 0x002C0DC3
		public JunimoHut() : this(Vector2.Zero)
		{
		}

		// Token: 0x060037B4 RID: 14260 RVA: 0x002C2BD0 File Offset: 0x002C0DD0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.noHarvest, "noHarvest").AddField(this.wasLit, "wasLit").AddField(this.shouldSendOutJunimos, "shouldSendOutJunimos").AddField(this.raisinDays, "raisinDays");
			this.wasLit.fieldChangeVisibleEvent += delegate(NetBool field, bool old_value, bool new_value)
			{
				this.updateLightState();
			};
		}

		// Token: 0x060037B5 RID: 14261 RVA: 0x002C2C41 File Offset: 0x002C0E41
		public override Rectangle getRectForAnimalDoor(BuildingData data)
		{
			return new Rectangle((1 + this.tileX.Value) * 64, (this.tileY.Value + 1) * 64, 64, 64);
		}

		// Token: 0x060037B6 RID: 14262 RVA: 0x002C2C6C File Offset: 0x002C0E6C
		public override Rectangle? getSourceRectForMenu()
		{
			return new Rectangle?(new Rectangle(Game1.GetSeasonIndexForLocation(base.GetParentLocation()) * 48, 0, 48, 64));
		}

		// Token: 0x060037B7 RID: 14263 RVA: 0x002C2C8B File Offset: 0x002C0E8B
		public Chest GetOutputChest()
		{
			return base.GetBuildingChest("Output");
		}

		// Token: 0x060037B8 RID: 14264 RVA: 0x002C2C98 File Offset: 0x002C0E98
		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			this.myJunimos.Clear();
			this.wasLit.Value = false;
			this.shouldSendOutJunimos.Value = true;
			if (this.raisinDays.Value > 0 && !Game1.IsWinter)
			{
				NetInt netInt = this.raisinDays;
				int value = netInt.Value;
				netInt.Value = value - 1;
			}
			if (this.raisinDays.Value == 0 && !Game1.IsWinter)
			{
				Chest output = this.GetOutputChest();
				if (output.Items.CountId("(O)Raisins") > 0)
				{
					this.raisinDays.Value += 7;
					output.Items.ReduceId("(O)Raisins", 1);
				}
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.isActive() && f.currentLocation != null && (f.currentLocation is FarmHouse || f.currentLocation.isStructure.Value))
				{
					this.shouldSendOutJunimos.Value = false;
				}
			}
		}

		// Token: 0x060037B9 RID: 14265 RVA: 0x002C2DC0 File Offset: 0x002C0FC0
		public void sendOutJunimos()
		{
			this.junimoSendOutTimer = 1000;
		}

		// Token: 0x060037BA RID: 14266 RVA: 0x002C2DCD File Offset: 0x002C0FCD
		public override void performActionOnConstruction(GameLocation location, Farmer who)
		{
			base.performActionOnConstruction(location, who);
			this.sendOutJunimos();
		}

		// Token: 0x060037BB RID: 14267 RVA: 0x002C2DDD File Offset: 0x002C0FDD
		public override void resetLocalState()
		{
			base.resetLocalState();
			this.updateLightState();
		}

		// Token: 0x060037BC RID: 14268 RVA: 0x002C2DEC File Offset: 0x002C0FEC
		public void updateLightState()
		{
			if (base.IsInCurrentLocation())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler.AppendFormatted("JunimoHut");
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<NetInt>(this.tileX);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<NetInt>(this.tileY);
				string lightSourceId = defaultInterpolatedStringHandler.ToStringAndClear();
				if (this.wasLit.Value)
				{
					if (Utility.getLightSource(lightSourceId) == null)
					{
						Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)(this.tileX.Value + 1), (float)(this.tileY.Value + 1)) * 64f + new Vector2(32f, 32f), 0.5f, LightSource.LightContext.None, 0L, this.parentLocationName.Value));
					}
					AmbientLocationSounds.addSound(new Vector2((float)(this.tileX.Value + 1), (float)(this.tileY.Value + 1)), 1);
					return;
				}
				Utility.removeLightSource(lightSourceId);
				AmbientLocationSounds.removeSound(new Vector2((float)(this.tileX.Value + 1), (float)(this.tileY.Value + 1)));
			}
		}

		// Token: 0x060037BD RID: 14269 RVA: 0x002C2F24 File Offset: 0x002C1124
		public int getUnusedJunimoNumber()
		{
			for (int i = 0; i < 3; i++)
			{
				if (i >= this.myJunimos.Count)
				{
					return i;
				}
				bool found = false;
				using (List<JunimoHarvester>.Enumerator enumerator = this.myJunimos.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.whichJunimoFromThisHut == i)
						{
							found = true;
							break;
						}
					}
				}
				if (!found)
				{
					return i;
				}
			}
			return 2;
		}

		// Token: 0x060037BE RID: 14270 RVA: 0x002C2FA0 File Offset: 0x002C11A0
		public override void updateWhenFarmNotCurrentLocation(GameTime time)
		{
			base.updateWhenFarmNotCurrentLocation(time);
			GameLocation location = base.GetParentLocation();
			Chest output = this.GetOutputChest();
			if (((output != null) ? output.mutex : null) != null)
			{
				output.mutex.Update(location);
				if (output.mutex.IsLockHeld() && Game1.activeClickableMenu == null)
				{
					output.mutex.ReleaseLock();
				}
			}
			if (Game1.IsMasterGame && this.junimoSendOutTimer > 0 && this.shouldSendOutJunimos.Value)
			{
				this.junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.junimoSendOutTimer <= 0 && this.myJunimos.Count < 3 && !location.IsWinterHere() && !location.IsRainingHere() && this.areThereMatureCropsWithinRadius() && (location.NameOrUniqueName != "Farm" || Game1.farmEvent == null))
				{
					int junimoNumber = this.getUnusedJunimoNumber();
					bool isPrismatic = false;
					Color? gemColor = this.getGemColor(ref isPrismatic);
					JunimoHarvester i = new JunimoHarvester(location, new Vector2((float)(this.tileX.Value + 1), (float)(this.tileY.Value + 1)) * 64f + new Vector2(0f, 32f), this, junimoNumber, gemColor);
					i.isPrismatic.Value = isPrismatic;
					location.characters.Add(i);
					this.myJunimos.Add(i);
					this.junimoSendOutTimer = 1000;
					if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2((float)(this.tileX.Value + 1), (float)(this.tileY.Value + 1))), 64, location))
					{
						try
						{
							location.playSound("junimoMeep1", null, null, SoundContext.Default);
						}
						catch (Exception)
						{
						}
					}
				}
			}
		}

		// Token: 0x060037BF RID: 14271 RVA: 0x002C3190 File Offset: 0x002C1390
		public override void Update(GameTime time)
		{
			if (!this.shouldSendOutJunimos.Value)
			{
				this.shouldSendOutJunimos.Value = true;
			}
			base.Update(time);
		}

		// Token: 0x060037C0 RID: 14272 RVA: 0x002C31B4 File Offset: 0x002C13B4
		private Color? getGemColor(ref bool isPrismatic)
		{
			List<Color> gemColors = new List<Color>();
			foreach (Item item in this.GetOutputChest().Items)
			{
				if (item != null && (item.Category == -12 || item.Category == -2))
				{
					Color? gemColor = TailoringMenu.GetDyeColor(item);
					if (item.QualifiedItemId == "(O)74")
					{
						isPrismatic = true;
					}
					if (gemColor != null)
					{
						gemColors.Add(gemColor.Value);
					}
				}
			}
			if (gemColors.Count > 0)
			{
				return new Color?(gemColors[Game1.random.Next(gemColors.Count)]);
			}
			return null;
		}

		// Token: 0x060037C1 RID: 14273 RVA: 0x002C3280 File Offset: 0x002C1480
		public bool areThereMatureCropsWithinRadius()
		{
			GameLocation location = base.GetParentLocation();
			for (int x = this.tileX.Value + 1 - this.cropHarvestRadius; x < this.tileX.Value + 2 + this.cropHarvestRadius; x++)
			{
				for (int y = this.tileY.Value - this.cropHarvestRadius + 1; y < this.tileY.Value + 2 + this.cropHarvestRadius; y++)
				{
					TerrainFeature terrainFeature;
					if (location.terrainFeatures.TryGetValue(new Vector2((float)x, (float)y), out terrainFeature))
					{
						if (location.isCropAtTile(x, y) && ((HoeDirt)terrainFeature).readyForHarvest())
						{
							this.lastKnownCropLocation = new Point(x, y);
							return true;
						}
						Bush bush = terrainFeature as Bush;
						if (bush != null && bush.readyForHarvest())
						{
							this.lastKnownCropLocation = new Point(x, y);
							return true;
						}
					}
				}
			}
			this.lastKnownCropLocation = Point.Zero;
			return false;
		}

		// Token: 0x060037C2 RID: 14274 RVA: 0x002C336C File Offset: 0x002C156C
		public override void performTenMinuteAction(int timeElapsed)
		{
			base.performTenMinuteAction(timeElapsed);
			GameLocation location = base.GetParentLocation();
			if (this.myJunimos.Count > 0)
			{
				for (int i = this.myJunimos.Count - 1; i >= 0; i--)
				{
					if (!location.characters.Contains(this.myJunimos[i]))
					{
						this.myJunimos.RemoveAt(i);
					}
					else
					{
						this.myJunimos[i].pokeToHarvest();
					}
				}
			}
			if (this.myJunimos.Count < 3 && Game1.timeOfDay < 1900)
			{
				this.junimoSendOutTimer = 1;
			}
			if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400)
			{
				if (!location.IsWinterHere() && Game1.random.NextDouble() < 0.2)
				{
					this.wasLit.Value = true;
					return;
				}
			}
			else if (Game1.timeOfDay == 2400 && !location.IsWinterHere())
			{
				this.wasLit.Value = false;
			}
		}

		// Token: 0x060037C3 RID: 14275 RVA: 0x002C3468 File Offset: 0x002C1668
		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
			{
				return false;
			}
			if (base.occupiesTile(tileLocation, false))
			{
				Chest output = this.GetOutputChest();
				if (output.Items.Count > 36)
				{
					output.clearNulls();
				}
				output.mutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new ItemGrabMenu(output.Items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(output.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(output.grabItemFromChest), false, true, true, true, true, 1, null, 1, this, ItemExitBehavior.ReturnToPlayer, false);
				}, null);
				return true;
			}
			return base.doAction(tileLocation, who);
		}

		// Token: 0x060037C4 RID: 14276 RVA: 0x002C3510 File Offset: 0x002C1710
		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			this.drawShadow(b, x, y);
			b.Draw(this.texture.Value, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(0, 0, 48, 64)), this.color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
		}

		// Token: 0x060037C5 RID: 14277 RVA: 0x002C3578 File Offset: 0x002C1778
		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if (this.daysOfConstructionLeft.Value > 0)
			{
				this.drawInConstruction(b);
				return;
			}
			this.drawShadow(b, -1, -1);
			Rectangle sourceRect = this.getSourceRectForMenu() ?? this.getSourceRect();
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64))), new Rectangle?(sourceRect), this.color * this.alpha, 0f, new Vector2(0f, (float)this.texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value - 1) * 64) / 10000f);
			if (this.raisinDays.Value > 0 && !Game1.IsWinter)
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 12), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 + 20))), new Rectangle?(new Rectangle(246, 46, 10, 18)), this.color * this.alpha, 0f, new Vector2(0f, 18f), 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value - 1) * 64 + 2) / 10000f);
			}
			bool containsOutput = false;
			Chest output = this.GetOutputChest();
			if (output != null)
			{
				foreach (Item item in output.Items)
				{
					if (item != null && item.Category != -12 && item.Category != -2)
					{
						containsOutput = true;
						break;
					}
				}
			}
			if (containsOutput)
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 128 + 12), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 32))), new Rectangle?(this.bagRect), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value - 1) * 64 + 1) / 10000f);
			}
			if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && this.wasLit.Value && !base.GetParentLocation().IsWinterHere())
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 64))), new Rectangle?(this.lightInteriorRect), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value - 1) * 64 + 1) / 10000f);
			}
		}

		// Token: 0x04002436 RID: 9270
		public int cropHarvestRadius = 8;

		// Token: 0x04002437 RID: 9271
		[XmlElement("output")]
		public Chest obsolete_output;

		// Token: 0x04002438 RID: 9272
		[XmlElement("noHarvest")]
		public readonly NetBool noHarvest = new NetBool();

		// Token: 0x04002439 RID: 9273
		[XmlElement("wasLit")]
		public readonly NetBool wasLit = new NetBool(false);

		// Token: 0x0400243A RID: 9274
		private int junimoSendOutTimer;

		// Token: 0x0400243B RID: 9275
		[XmlIgnore]
		public List<JunimoHarvester> myJunimos = new List<JunimoHarvester>();

		// Token: 0x0400243C RID: 9276
		[XmlIgnore]
		public Point lastKnownCropLocation = Point.Zero;

		// Token: 0x0400243D RID: 9277
		public NetInt raisinDays = new NetInt();

		// Token: 0x0400243E RID: 9278
		[XmlElement("shouldSendOutJunimos")]
		public NetBool shouldSendOutJunimos = new NetBool(false);

		// Token: 0x0400243F RID: 9279
		private Rectangle lightInteriorRect = new Rectangle(195, 0, 18, 17);

		// Token: 0x04002440 RID: 9280
		private Rectangle bagRect = new Rectangle(208, 51, 15, 13);
	}
}

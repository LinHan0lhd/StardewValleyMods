using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Tools
{
	// Token: 0x02000135 RID: 309
	public class WateringCan : Tool
	{
		// Token: 0x170002BB RID: 699
		// (get) Token: 0x060018CC RID: 6348 RVA: 0x00123EF0 File Offset: 0x001220F0
		// (set) Token: 0x060018CD RID: 6349 RVA: 0x00123F0C File Offset: 0x0012210C
		public int WaterLeft
		{
			get
			{
				if (!this.IsBottomless)
				{
					return this.waterLeft.Value;
				}
				return this.waterCanMax;
			}
			set
			{
				this.waterLeft.Value = value;
			}
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x060018CE RID: 6350 RVA: 0x00123F1A File Offset: 0x0012211A
		// (set) Token: 0x060018CF RID: 6351 RVA: 0x00123F27 File Offset: 0x00122127
		public bool IsBottomless
		{
			get
			{
				return this.isBottomless.Value;
			}
			set
			{
				this.isBottomless.Value = value;
			}
		}

		// Token: 0x060018D0 RID: 6352 RVA: 0x00123F35 File Offset: 0x00122135
		public WateringCan() : base("Watering Can", 0, 273, 296, false, 0)
		{
		}

		// Token: 0x060018D1 RID: 6353 RVA: 0x00123F70 File Offset: 0x00122170
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.isBottomless, "isBottomless").AddField(this.waterLeft, "waterLeft");
			this.upgradeLevel.fieldChangeVisibleEvent += delegate(NetInt <p0>, int <p1>, int <p2>)
			{
				this.OnUpgradeLevelChanged();
			};
		}

		// Token: 0x060018D2 RID: 6354 RVA: 0x00123FC4 File Offset: 0x001221C4
		protected override void MigrateLegacyItemId()
		{
			switch (base.UpgradeLevel)
			{
			case 0:
				base.ItemId = "WateringCan";
				return;
			case 1:
				base.ItemId = "CopperWateringCan";
				return;
			case 2:
				base.ItemId = "SteelWateringCan";
				return;
			case 3:
				base.ItemId = "GoldWateringCan";
				return;
			case 4:
				base.ItemId = "IridiumWateringCan";
				return;
			default:
				base.ItemId = "WateringCan";
				return;
			}
		}

		// Token: 0x060018D3 RID: 6355 RVA: 0x0012403B File Offset: 0x0012223B
		protected override Item GetOneNew()
		{
			return new WateringCan();
		}

		// Token: 0x060018D4 RID: 6356 RVA: 0x00124044 File Offset: 0x00122244
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			WateringCan other = source as WateringCan;
			if (other != null)
			{
				this.WaterLeft = other.WaterLeft;
				this.IsBottomless = other.IsBottomless;
			}
		}

		// Token: 0x060018D5 RID: 6357 RVA: 0x0012407C File Offset: 0x0012227C
		protected virtual void OnUpgradeLevelChanged()
		{
			switch (this.upgradeLevel.Value)
			{
			case 0:
				this.waterCanMax = 40;
				break;
			case 1:
				this.waterCanMax = 55;
				break;
			case 2:
				this.waterCanMax = 70;
				break;
			case 3:
				this.waterCanMax = 85;
				break;
			default:
				this.waterCanMax = 100;
				break;
			}
			this.waterLeft.Value = this.waterCanMax;
		}

		// Token: 0x060018D6 RID: 6358 RVA: 0x001240F0 File Offset: 0x001222F0
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location + (Game1.player.hasWateringCanEnchantment ? new Vector2(0f, -4f) : new Vector2(0f, -12f)), scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
			if (drawStackNumber != StackDrawType.Hide && !Game1.player.hasWateringCanEnchantment)
			{
				spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(4f, 44f), new Rectangle?(new Rectangle(297, 420, 14, 5)), Color.White * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 0.0001f);
				spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 8, (int)location.Y + 64 - 16, (int)((float)this.WaterLeft / (float)this.waterCanMax * 48f), 8), this.IsBottomless ? (Color.BlueViolet * 1f * transparency) : (Color.DodgerBlue * 0.7f * transparency));
			}
		}

		// Token: 0x060018D7 RID: 6359 RVA: 0x00124228 File Offset: 0x00122428
		public override string getDescription()
		{
			return Game1.parseText(base.description + (Game1.player.hasWateringCanEnchantment ? (Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan_enchant")) : ""), Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x060018D8 RID: 6360 RVA: 0x00124284 File Offset: 0x00122484
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			power = who.toolPower.Value;
			who.stopJittering();
			List<Vector2> tileLocations = base.tilesAffected(new Vector2((float)(x / 64), (float)(y / 64)), power, who);
			if (Game1.currentLocation.CanRefillWateringCanOnTile(x / 64, y / 64))
			{
				who.jitterStrength = 0.5f;
				this.WaterLeft = this.waterCanMax;
				if (this.PlayUseSounds)
				{
					who.playNearbySoundAll("slosh", null, SoundContext.Default);
					DelayedAction.playSoundAfterDelay("glug", 250, location, new Vector2?(who.Tile), -1, false);
					return;
				}
			}
			else if (this.WaterLeft > 0 || who.hasWateringCanEnchantment)
			{
				if (!this.isEfficient.Value)
				{
					who.Stamina -= (float)(2 * (power + 1)) - (float)who.FarmingLevel * 0.1f;
				}
				int i = 0;
				foreach (Vector2 tileLocation in tileLocations)
				{
					TerrainFeature terrainFeature;
					if (location.terrainFeatures.TryGetValue(tileLocation, out terrainFeature))
					{
						terrainFeature.performToolAction(this, 0, tileLocation);
					}
					Object obj;
					if (location.objects.TryGetValue(tileLocation, out obj))
					{
						obj.performToolAction(this);
					}
					location.performToolAction(this, (int)tileLocation.X, (int)tileLocation.Y);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(13, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 10, Game1.random.NextBool(), 70f, 0, 64, (tileLocation.Y * 64f + 32f) / 10000f - 0.01f, -1, 0)
						{
							delayBeforeAnimationStart = 200 + i * 10
						}
					});
					i++;
				}
				if (!this.IsBottomless)
				{
					this.WaterLeft -= power + 1;
				}
				Vector2 basePosition = new Vector2(who.Position.X - 32f - 4f, who.Position.Y - 16f - 4f);
				switch (who.FacingDirection)
				{
				case 0:
					basePosition = Vector2.Zero;
					break;
				case 1:
					basePosition.X += 136f;
					break;
				case 2:
					basePosition.X += 72f;
					basePosition.Y += 44f;
					break;
				}
				if (!basePosition.Equals(Vector2.Zero))
				{
					Rectangle playerBounds = who.GetBoundingBox();
					for (int j = 0; j < 30; j++)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite("", new Rectangle(0, 0, 1, 1), 999f, 1, 999, basePosition + new Vector2((float)(Game1.random.Next(-3, 0) * 4), (float)(Game1.random.Next(2) * 4)), false, false, (float)(playerBounds.Bottom + 32) / 10000f, 0.04f, Game1.random.Choose(Color.DeepSkyBlue, Color.LightBlue), 4f, 0f, 0f, 0f, false)
							{
								delayBeforeAnimationStart = j * 15,
								motion = new Vector2((float)Game1.random.Next(-10, 11) / 100f, 0.5f),
								acceleration = new Vector2(0f, 0.1f)
							}
						});
					}
					return;
				}
			}
			else if (!this._emptyCanPlayed)
			{
				this._emptyCanPlayed = true;
				who.doEmote(4);
				if (who == Game1.player)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14335"), true);
				}
			}
		}

		// Token: 0x060018D9 RID: 6361 RVA: 0x0012468C File Offset: 0x0012288C
		public override bool CanUseOnStandingTile()
		{
			return true;
		}

		// Token: 0x060018DA RID: 6362 RVA: 0x00124690 File Offset: 0x00122890
		public override void tickUpdate(GameTime time, Farmer who)
		{
			base.tickUpdate(time, who);
			if (who.IsLocalPlayer)
			{
				if (Game1.areAllOfTheseKeysUp(Game1.input.GetKeyboardState(), Game1.options.useToolButton) && Game1.input.GetMouseState().LeftButton == ButtonState.Released && Game1.input.GetGamePadState().IsButtonUp(Buttons.X))
				{
					this._emptyCanPlayed = false;
					return;
				}
			}
			else
			{
				this._emptyCanPlayed = false;
			}
		}

		// Token: 0x04000EFC RID: 3836
		[XmlElement("isBottomless")]
		public readonly NetBool isBottomless = new NetBool();

		// Token: 0x04000EFD RID: 3837
		[XmlIgnore]
		protected bool _emptyCanPlayed;

		// Token: 0x04000EFE RID: 3838
		[XmlIgnore]
		public int waterCanMax = 40;

		// Token: 0x04000EFF RID: 3839
		private readonly NetInt waterLeft = new NetInt(40);
	}
}

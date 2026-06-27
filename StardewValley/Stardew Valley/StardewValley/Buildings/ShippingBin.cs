using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Buildings
{
	// Token: 0x02000389 RID: 905
	public class ShippingBin : Building
	{
		// Token: 0x060037D4 RID: 14292 RVA: 0x002C3E44 File Offset: 0x002C2044
		public ShippingBin(Vector2 tileLocation) : base("Shipping Bin", tileLocation)
		{
			this.initLid();
		}

		// Token: 0x060037D5 RID: 14293 RVA: 0x002C3E58 File Offset: 0x002C2058
		public ShippingBin() : this(Vector2.Zero)
		{
		}

		// Token: 0x060037D6 RID: 14294 RVA: 0x002C3E68 File Offset: 0x002C2068
		public void initLid()
		{
			this.shippingBinLid = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(134, 226, 30, 25), new Vector2((float)this.tileX.Value, (float)(this.tileY.Value - 1)) * 64f + new Vector2(1f, -7f) * 4f, false, 0f, Color.White)
			{
				holdLastFrame = true,
				destroyable = false,
				interval = 20f,
				animationLength = 13,
				paused = true,
				scale = 4f,
				layerDepth = (float)((this.tileY.Value + 1) * 64) / 10000f + 0.0001f,
				pingPong = true,
				pingPongMotion = 0
			};
			this.shippingBinLidOpenArea = new Rectangle((this.tileX.Value - 1) * 64, (this.tileY.Value - 1) * 64, 256, 192);
			this._lidGenerationPosition = new Vector2((float)this.tileX.Value, (float)this.tileY.Value);
		}

		// Token: 0x060037D7 RID: 14295 RVA: 0x002C3FA6 File Offset: 0x002C21A6
		public override Rectangle? getSourceRectForMenu()
		{
			return new Rectangle?(new Rectangle(0, 0, this.texture.Value.Bounds.Width, this.texture.Value.Bounds.Height));
		}

		// Token: 0x060037D8 RID: 14296 RVA: 0x002C3FDE File Offset: 0x002C21DE
		public override void resetLocalState()
		{
			base.resetLocalState();
			if (this.shippingBinLid != null)
			{
				Rectangle rectangle = this.shippingBinLidOpenArea;
				return;
			}
			this.initLid();
		}

		// Token: 0x060037D9 RID: 14297 RVA: 0x002C3FFC File Offset: 0x002C21FC
		public override void Update(GameTime time)
		{
			base.Update(time);
			if (this.farm == null)
			{
				this.farm = Game1.getFarm();
			}
			if (this.shippingBinLid != null)
			{
				Rectangle rectangle = this.shippingBinLidOpenArea;
				if (this._lidGenerationPosition.X == (float)this.tileX.Value && this._lidGenerationPosition.Y == (float)this.tileY.Value)
				{
					bool opening = false;
					using (FarmerCollection.Enumerator enumerator = base.GetParentLocation().farmers.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.GetBoundingBox().Intersects(this.shippingBinLidOpenArea))
							{
								this.openShippingBinLid();
								opening = true;
							}
						}
					}
					if (!opening)
					{
						this.closeShippingBinLid();
					}
					this.updateShippingBinLid(time);
					return;
				}
			}
			this.initLid();
		}

		// Token: 0x060037DA RID: 14298 RVA: 0x002C40E0 File Offset: 0x002C22E0
		public override void performActionOnBuildingPlacement()
		{
			base.performActionOnBuildingPlacement();
			this.initLid();
		}

		// Token: 0x060037DB RID: 14299 RVA: 0x002C40F0 File Offset: 0x002C22F0
		private void openShippingBinLid()
		{
			if (this.shippingBinLid != null)
			{
				if (this.shippingBinLid.pingPongMotion != 1 && base.IsInCurrentLocation())
				{
					Game1.currentLocation.localSound("doorCreak", null, null, SoundContext.Default);
				}
				this.shippingBinLid.pingPongMotion = 1;
				this.shippingBinLid.paused = false;
			}
		}

		// Token: 0x060037DC RID: 14300 RVA: 0x002C4158 File Offset: 0x002C2358
		private void closeShippingBinLid()
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.shippingBinLid;
			if (temporaryAnimatedSprite != null && temporaryAnimatedSprite.currentParentTileIndex > 0)
			{
				if (this.shippingBinLid.pingPongMotion != -1 && base.IsInCurrentLocation())
				{
					Game1.currentLocation.localSound("doorCreakReverse", null, null, SoundContext.Default);
				}
				this.shippingBinLid.pingPongMotion = -1;
				this.shippingBinLid.paused = false;
			}
		}

		// Token: 0x060037DD RID: 14301 RVA: 0x002C41CC File Offset: 0x002C23CC
		private void updateShippingBinLid(GameTime time)
		{
			if (this.isShippingBinLidOpen(true) && this.shippingBinLid.pingPongMotion == 1)
			{
				this.shippingBinLid.paused = true;
			}
			else if (this.shippingBinLid.currentParentTileIndex == 0 && this.shippingBinLid.pingPongMotion == -1)
			{
				if (!this.shippingBinLid.paused && base.IsInCurrentLocation())
				{
					Game1.currentLocation.localSound("woodyStep", null, null, SoundContext.Default);
				}
				this.shippingBinLid.paused = true;
			}
			this.shippingBinLid.update(time);
		}

		// Token: 0x060037DE RID: 14302 RVA: 0x002C4269 File Offset: 0x002C2469
		private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
		{
			return this.shippingBinLid != null && this.shippingBinLid.currentParentTileIndex >= (requiredToBeFullyOpen ? (this.shippingBinLid.animationLength - 1) : 1);
		}

		// Token: 0x060037DF RID: 14303 RVA: 0x002C4298 File Offset: 0x002C2498
		private void shipItem(Item i, Farmer who)
		{
			if (i != null)
			{
				who.removeItemFromInventory(i);
				Farm farm = this.farm;
				if (farm != null)
				{
					farm.getShippingBin(who).Add(i);
				}
				this.showShipment(i, false);
				this.farm.lastItemShipped = i;
				if (Game1.player.ActiveItem == null)
				{
					Game1.player.showNotCarrying();
					Game1.player.Halt();
				}
			}
		}

		// Token: 0x060037E0 RID: 14304 RVA: 0x002C42FC File Offset: 0x002C24FC
		public override bool CanLeftClick(int x, int y)
		{
			Rectangle hit_rect = new Rectangle(this.tileX.Value * 64, this.tileY.Value * 64, this.tilesWide.Value * 64, this.tilesHigh.Value * 64);
			hit_rect.Y -= 64;
			hit_rect.Height += 64;
			return hit_rect.Contains(x, y);
		}

		// Token: 0x060037E1 RID: 14305 RVA: 0x002C436C File Offset: 0x002C256C
		public override bool leftClicked()
		{
			Item item = Game1.player.ActiveItem;
			bool? flag = (item != null) ? new bool?(item.canBeShipped()) : null;
			if (flag != null && flag.GetValueOrDefault() && this.farm != null && Vector2.Distance(Game1.player.Tile, new Vector2((float)this.tileX.Value + 0.5f, (float)this.tileY.Value)) <= 2f)
			{
				Game1.player.ActiveItem = null;
				Game1.player.showNotCarrying();
				this.farm.getShippingBin(Game1.player).Add(item);
				this.farm.lastItemShipped = item;
				this.showShipment(item, true);
				return true;
			}
			return base.leftClicked();
		}

		// Token: 0x060037E2 RID: 14306 RVA: 0x002C4440 File Offset: 0x002C2640
		public void showShipment(Item item, bool playThrowSound = true)
		{
			if (this.farm == null)
			{
				return;
			}
			GameLocation parentLocation = base.GetParentLocation();
			if (playThrowSound)
			{
				parentLocation.localSound("backpackIN", null, null, SoundContext.Default);
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0, null, null, -1, false);
			int id = Game1.random.Next();
			parentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(524, 218, 34, 22), new Vector2((float)this.tileX.Value, (float)(this.tileY.Value - 1)) * 64f + new Vector2(-1f, 5f) * 4f, false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				alpha = this.alpha,
				scale = 4f,
				layerDepth = (float)((this.tileY.Value + 1) * 64) / 10000f + 0.0002f,
				id = id,
				extraInfoForEndBehavior = id,
				endFunction = new TemporaryAnimatedSprite.endBehavior(parentLocation.removeTemporarySpritesWithID)
			});
			parentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(524, 230, 34, 10), new Vector2((float)this.tileX.Value, (float)(this.tileY.Value - 1)) * 64f + new Vector2(-1f, 17f) * 4f, false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				alpha = this.alpha,
				scale = 4f,
				layerDepth = (float)((this.tileY.Value + 1) * 64) / 10000f + 0.0003f,
				id = id,
				extraInfoForEndBehavior = id
			});
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
			ColoredObject coloredObj = item as ColoredObject;
			Vector2 initialPosition = new Vector2((float)this.tileX.Value, (float)(this.tileY.Value - 1)) * 64f + new Vector2((float)(7 + Game1.random.Next(6)), 2f) * 4f;
			foreach (bool isColorOverlay in new bool[]
			{
				default(bool),
				true
			})
			{
				if (!isColorOverlay || (coloredObj != null && !coloredObj.ColorSameIndexAsParentSheetIndex))
				{
					parentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect((isColorOverlay > false) ? 1 : 0, null), initialPosition, false, 0f, Color.White)
					{
						interval = 9999f,
						scale = 4f,
						alphaFade = 0.045f,
						layerDepth = (float)((this.tileY.Value + 1) * 64) / 10000f + 0.000225f,
						motion = new Vector2(0f, 0.3f),
						acceleration = new Vector2(0f, 0.2f),
						scaleChange = -0.05f,
						color = ((coloredObj != null) ? coloredObj.color.Value : Color.White)
					});
				}
			}
		}

		// Token: 0x060037E3 RID: 14307 RVA: 0x002C47F8 File Offset: 0x002C29F8
		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (this.daysOfConstructionLeft.Value > 0 || tileLocation.X < (float)this.tileX.Value || tileLocation.X > (float)(this.tileX.Value + 1) || tileLocation.Y != (float)this.tileY.Value)
			{
				return base.doAction(tileLocation, who);
			}
			if (!Game1.didPlayerJustRightClick(true))
			{
				return false;
			}
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, true, false, new InventoryMenu.highlightThisItem(Utility.highlightShippableObjects), new ItemGrabMenu.behaviorOnItemSelect(this.shipItem), "", null, true, true, false, true, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
			itemGrabMenu.initializeUpperRightCloseButton();
			itemGrabMenu.setBackgroundTransparency(false);
			itemGrabMenu.setDestroyItemOnClick(true);
			itemGrabMenu.initializeShippingBin();
			Game1.activeClickableMenu = itemGrabMenu;
			if (who.IsLocalPlayer)
			{
				Game1.playSound("shwip", null);
			}
			if (Game1.player.FacingDirection == 1)
			{
				Game1.player.Halt();
			}
			Game1.player.showCarrying();
			return true;
		}

		// Token: 0x060037E4 RID: 14308 RVA: 0x002C4900 File Offset: 0x002C2B00
		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			base.drawInMenu(b, x, y);
			b.Draw(Game1.mouseCursors, new Vector2((float)(x + 4), (float)(y - 20)), new Rectangle?(new Rectangle(134, 226, 30, 25)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}

		// Token: 0x060037E5 RID: 14309 RVA: 0x002C4964 File Offset: 0x002C2B64
		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			base.draw(b);
			if (this.shippingBinLid != null && this.daysOfConstructionLeft.Value <= 0)
			{
				this.shippingBinLid.color = this.color;
				this.shippingBinLid.draw(b, false, 0, 0, this.alpha * ((this.newConstructionTimer.Value > 0) ? ((1000f - (float)this.newConstructionTimer.Value) / 1000f) : 1f));
			}
		}

		// Token: 0x04002447 RID: 9287
		private TemporaryAnimatedSprite shippingBinLid;

		// Token: 0x04002448 RID: 9288
		private Farm farm;

		// Token: 0x04002449 RID: 9289
		private Rectangle shippingBinLidOpenArea;

		// Token: 0x0400244A RID: 9290
		protected Vector2 _lidGenerationPosition;
	}
}

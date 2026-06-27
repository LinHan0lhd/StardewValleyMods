using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects
{
	// Token: 0x020001BB RID: 443
	public class WoodChipper : Object
	{
		// Token: 0x17000338 RID: 824
		// (get) Token: 0x06001FAD RID: 8109 RVA: 0x0016B8CF File Offset: 0x00169ACF
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x0016B8D6 File Offset: 0x00169AD6
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.depositedItem, "depositedItem");
			this.depositedItem.fieldChangeVisibleEvent += this.OnDepositedItemChange;
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x0016B90C File Offset: 0x00169B0C
		public void OnDepositedItemChange(NetRef<Object> field, Object old_value, Object new_value)
		{
			if (Game1.gameMode != 6 && new_value != null)
			{
				this.shakeTimer = 1000;
				this._isAnimatingChip = true;
			}
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x0016B92B File Offset: 0x00169B2B
		public WoodChipper()
		{
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x0016B940 File Offset: 0x00169B40
		public WoodChipper(Vector2 position) : base(position, "211", false)
		{
			this.Name = "Wood Chipper";
			this.type.Value = "Crafting";
			this.bigCraftable.Value = true;
			this.canBeSetDown.Value = true;
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x0016B998 File Offset: 0x00169B98
		public override void addWorkingAnimation()
		{
			GameLocation environment = this.Location;
			if (environment == null || !environment.farmers.Any())
			{
				return;
			}
			if (Game1.random.NextDouble() < 0.35)
			{
				for (int i = 0; i < 8; i++)
				{
					environment.temporarySprites.Add(new TemporaryAnimatedSprite(47, this.tileLocation.Value * 64f + new Vector2(0f, (float)(-76 + Game1.random.Next(-48, 0))), new Color(200, 110, 17), 8, false, 50f, 0, -1, 0.003f + Math.Max(0f, ((this.tileLocation.Y + 1f) * 64f - 24f) / 10000f) + this.tileLocation.X * 1E-05f, -1, 0)
					{
						delayBeforeAnimationStart = i * 100
					});
				}
				environment.playSound("woodchipper_occasional", null, null, SoundContext.Default);
				this.shakeTimer = 1500;
			}
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x0016BAC0 File Offset: 0x00169CC0
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			Object dropped_in_object = dropInItem as Object;
			if (this.heldObject.Value != null || this.depositedItem.Value != null)
			{
				return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
			}
			if (dropped_in_object == null)
			{
				return false;
			}
			if (base.PlaceInMachine(base.GetMachineData(), dropped_in_object, probe, who, true, true))
			{
				if (!probe)
				{
					this.depositedItem.Value = (dropInItem.getOne() as Object);
					this.shakeTimer = 1800;
					for (int i = 0; i < 12; i++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(47, this.tileLocation.Value * 64f + new Vector2(0f, (float)(-76 + Game1.random.Next(-48, 0))), new Color(200, 110, 17), 8, false, 50f, 0, -1, 0.003f + Math.Max(0f, ((this.tileLocation.Y + 1f) * 64f - 24f) / 10000f) + this.tileLocation.X * 1E-05f, -1, 0)
						{
							delayBeforeAnimationStart = i * 100
						});
					}
					if (returnFalseIfItemConsumed)
					{
						return false;
					}
				}
				return true;
			}
			return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
		}

		// Token: 0x06001FB4 RID: 8116 RVA: 0x0016BC1D File Offset: 0x00169E1D
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			this.TileLocation = new Vector2((float)(x / 64), (float)(y / 64));
			return true;
		}

		// Token: 0x06001FB5 RID: 8117 RVA: 0x0016BC38 File Offset: 0x00169E38
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (who.IsLocalPlayer && this.heldObject.Value != null && this.readyForHarvest.Value)
			{
				if (!justCheckingForActivity)
				{
					Object collected_object = this.heldObject.Value;
					this.heldObject.Value = null;
					if (who.isMoving())
					{
						Game1.haltAfterCheck = false;
					}
					if (!who.addItemToInventoryBool(collected_object, false))
					{
						this.heldObject.Value = collected_object;
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
						return false;
					}
					Game1.playSound("coin", null);
					this.readyForHarvest.Value = false;
					this.depositedItem.Value = null;
					this.heldObject.Value = null;
					this.AttemptAutoLoad(who);
				}
				return true;
			}
			return base.checkForAction(who, justCheckingForActivity);
		}

		// Token: 0x06001FB6 RID: 8118 RVA: 0x0016BD14 File Offset: 0x00169F14
		public override void updateWhenCurrentLocation(GameTime time)
		{
			if (this.Location != null && this.depositedItem.Value != null && base.MinutesUntilReady > 0)
			{
				this.nextShakeTime -= time.ElapsedGameTime.Milliseconds;
				this.nextSmokeTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextSmokeTime <= 0)
				{
					this.nextSmokeTime = Game1.random.Next(3000, 6000);
				}
				if (this.nextShakeTime <= 0)
				{
					this.nextShakeTime = Game1.random.Next(1000, 2000);
					if (this.shakeTimer <= 0)
					{
						this._isAnimatingChip = false;
						this.shakeTimer = 0;
					}
				}
			}
			base.updateWhenCurrentLocation(time);
		}

		// Token: 0x06001FB7 RID: 8119 RVA: 0x0016BDE4 File Offset: 0x00169FE4
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			Vector2 scale_factor = Vector2.One;
			scale_factor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
			Rectangle destination = new Rectangle((int)(position.X - scale_factor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scale_factor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scale_factor.X), (int)(128f + scale_factor.Y / 2f));
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
			ParsedItemData baseDraw = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D baseTexture = baseDraw.GetTexture();
			spriteBatch.Draw(baseTexture, destination, new Rectangle?(baseDraw.GetSourceRect((this.readyForHarvest.Value > false) ? 1 : 0, null)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			if (this.shakeTimer > 0)
			{
				spriteBatch.Draw(baseTexture, new Rectangle(destination.X, destination.Y + 4, destination.Width, 60), new Rectangle?(new Rectangle(80, 833, 16, 15)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer + 0.0035f);
			}
			if (this.depositedItem.Value != null && this.shakeTimer > 0 && this._isAnimatingChip)
			{
				float completion = 1f - (float)this.shakeTimer / 1000f;
				Vector2 end_position = position + new Vector2(32f, 32f);
				Vector2 start_position = end_position + new Vector2(0f, -16f);
				Vector2 draw_position = default(Vector2);
				draw_position.X = Utility.Lerp(start_position.X, end_position.X, completion);
				draw_position.Y = Utility.Lerp(start_position.Y, end_position.Y, completion);
				draw_position.X += (float)(Game1.random.Next(-1, 2) * 2);
				draw_position.Y += (float)(Game1.random.Next(-1, 2) * 2);
				float draw_scale = Utility.Lerp(1f, 0.75f, completion);
				ParsedItemData itemDraw = ItemRegistry.GetDataOrErrorItem(this.depositedItem.Value.QualifiedItemId);
				Texture2D itemTexture = itemDraw.GetTexture();
				spriteBatch.Draw(itemTexture, draw_position, new Rectangle?(itemDraw.GetSourceRect(0, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * draw_scale, this.depositedItem.Value.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, draw_layer + 0.00175f);
			}
			if (this.depositedItem.Value != null && base.MinutesUntilReady > 0)
			{
				int frame = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 200.0) / 50;
				spriteBatch.Draw(baseTexture, position + new Vector2(6f, 17f) * 4f, new Rectangle?(new Rectangle(80 + frame % 2 * 8, 848 + frame / 2 * 7, 8, 7)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-05f);
				spriteBatch.Draw(baseTexture, position + new Vector2(3f, 9f) * 4f + new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)), new Rectangle?(new Rectangle(51, 841, 10, 6)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-05f);
			}
			if (this.readyForHarvest.Value)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 - 8), (float)(y * 64 - 96 - 16) + yOffset)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + this.tileLocation.X / 10000f);
				if (this.heldObject.Value != null)
				{
					ParsedItemData itemDraw2 = ItemRegistry.GetDataOrErrorItem(this.heldObject.Value.QualifiedItemId);
					Texture2D itemTexture2 = itemDraw2.GetTexture();
					spriteBatch.Draw(itemTexture2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + yOffset)), new Rectangle?(itemDraw2.GetSourceRect(0, null)), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + this.tileLocation.X / 10000f);
					ColoredObject coloredObject = this.heldObject.Value as ColoredObject;
					if (coloredObject != null)
					{
						Rectangle coloredSourceRect = itemDraw2.GetSourceRect(1, new int?(base.ParentSheetIndex));
						spriteBatch.Draw(itemTexture2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + yOffset)), new Rectangle?(coloredSourceRect), coloredObject.color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + this.tileLocation.X / 10000f);
					}
				}
			}
		}

		// Token: 0x0400136A RID: 4970
		public const int CHIP_TIME = 1000;

		// Token: 0x0400136B RID: 4971
		public readonly NetRef<Object> depositedItem = new NetRef<Object>();

		// Token: 0x0400136C RID: 4972
		protected bool _isAnimatingChip;

		// Token: 0x0400136D RID: 4973
		public int nextSmokeTime;

		// Token: 0x0400136E RID: 4974
		public int nextShakeTime;
	}
}

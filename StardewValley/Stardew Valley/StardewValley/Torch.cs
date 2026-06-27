using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace StardewValley
{
	// Token: 0x020000EF RID: 239
	public class Torch : Object
	{
		// Token: 0x0600138B RID: 5003 RVA: 0x000EF27B File Offset: 0x000ED47B
		public Torch() : this(1)
		{
		}

		// Token: 0x0600138C RID: 5004 RVA: 0x000EF284 File Offset: 0x000ED484
		public Torch(int initialStack)
		{
			this.ashes = new Vector2[3];
			base..ctor("93", initialStack, false, -1, 0);
		}

		// Token: 0x0600138D RID: 5005 RVA: 0x000EF2A1 File Offset: 0x000ED4A1
		public Torch(int initialStack, string itemId)
		{
			this.ashes = new Vector2[3];
			base..ctor(itemId, initialStack, false, -1, 0);
		}

		// Token: 0x0600138E RID: 5006 RVA: 0x000EF2BA File Offset: 0x000ED4BA
		public Torch(string index, bool bigCraftable)
		{
			this.ashes = new Vector2[3];
			base..ctor(Vector2.Zero, index, false);
		}

		// Token: 0x0600138F RID: 5007 RVA: 0x000EF2D5 File Offset: 0x000ED4D5
		public override void RecalculateBoundingBox()
		{
			this.boundingBox.Value = new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, 64, 64);
		}

		// Token: 0x06001390 RID: 5008 RVA: 0x000EF30C File Offset: 0x000ED50C
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = this.parentSheetIndex.Value.ToString();
		}

		// Token: 0x06001391 RID: 5009 RVA: 0x000EF332 File Offset: 0x000ED532
		protected override Item GetOneNew()
		{
			if (!this.bigCraftable.Value)
			{
				return new Torch(1, base.ItemId);
			}
			return new Torch(base.ItemId, true);
		}

		// Token: 0x06001392 RID: 5010 RVA: 0x000EF35A File Offset: 0x000ED55A
		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			if (this.bigCraftable.Value && this.isOn.Value)
			{
				AmbientLocationSounds.addSound(this.tileLocation.Value, 1);
			}
		}

		// Token: 0x06001393 RID: 5011 RVA: 0x000EF390 File Offset: 0x000ED590
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (!this.bigCraftable.Value)
			{
				return base.checkForAction(who, justCheckingForActivity);
			}
			if (justCheckingForActivity)
			{
				return true;
			}
			if (base.QualifiedItemId == "(BC)278")
			{
				Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
				Game1.activeClickableMenu = new CraftingPage((int)center.X, (int)center.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true, null);
				return true;
			}
			this.isOn.Value = !this.isOn.Value;
			if (this.isOn.Value)
			{
				if (this.bigCraftable.Value)
				{
					if (who != null)
					{
						Game1.playSound("fireball", null);
					}
					this.initializeLightSource(this.tileLocation.Value, false);
					AmbientLocationSounds.addSound(this.tileLocation.Value, 1);
				}
			}
			else if (this.bigCraftable.Value)
			{
				this.performRemoveAction();
				if (who != null)
				{
					Game1.playSound("woodyHit", null);
				}
			}
			return true;
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x000EF4C0 File Offset: 0x000ED6C0
		public override bool placementAction(GameLocation location, int x, int y, Farmer who)
		{
			Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
			Torch toPlace = this.bigCraftable.Value ? new Torch(base.ItemId, true) : new Torch(1, base.ItemId);
			if (this.bigCraftable.Value)
			{
				toPlace.isOn.Value = false;
			}
			location.objects.Add(placementTile, toPlace);
			toPlace.initializeLightSource(placementTile, false);
			if (who != null)
			{
				Game1.playSound("woodyStep", null);
			}
			return true;
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x000EF54D File Offset: 0x000ED74D
		public override bool isPassable()
		{
			return !this.bigCraftable.Value;
		}

		// Token: 0x06001396 RID: 5014 RVA: 0x000EF560 File Offset: 0x000ED760
		public override void updateWhenCurrentLocation(GameTime time)
		{
			base.updateWhenCurrentLocation(time);
			GameLocation environment = this.Location;
			if (environment == null)
			{
				return;
			}
			this.updateAshes((int)(this.tileLocation.X * 2000f + this.tileLocation.Y));
			this.smokePuffTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.smokePuffTimer <= 0f)
			{
				this.smokePuffTimer = 1000f;
				if (base.QualifiedItemId == "(BC)278")
				{
					Utility.addSmokePuff(environment, this.tileLocation.Value * 64f + new Vector2(32f, -32f), 0, 2f, 0.02f, 0.75f, 0.002f);
				}
			}
		}

		// Token: 0x06001397 RID: 5015 RVA: 0x000EF630 File Offset: 0x000ED830
		private void updateAshes(int identifier)
		{
			if (Utility.isOnScreen(this.tileLocation.Value * 64f, 256))
			{
				for (int i = this.ashes.Length - 1; i >= 0; i--)
				{
					Vector2 temp = this.ashes[i];
					temp.Y -= 1f * ((float)(i + 1) * 0.25f);
					if (i % 2 != 0)
					{
						temp.X += (float)Math.Sin((double)this.ashes[i].Y / 6.283185307179586) / 2f;
					}
					this.ashes[i] = temp;
					if (Game1.random.NextDouble() < 0.0075 && this.ashes[i].Y < -100f)
					{
						this.ashes[i] = new Vector2((float)(Game1.random.Next(-1, 3) * 4) * 0.75f, 0f);
					}
				}
				this.color = Math.Max(-0.8f, Math.Min(0.7f, this.color + this.ashes[0].Y / 1200f));
			}
		}

		// Token: 0x06001398 RID: 5016 RVA: 0x000EF779 File Offset: 0x000ED979
		public override void performRemoveAction()
		{
			AmbientLocationSounds.removeSound(this.TileLocation);
			if (this.bigCraftable.Value)
			{
				this.isOn.Value = false;
			}
			base.performRemoveAction();
		}

		// Token: 0x06001399 RID: 5017 RVA: 0x000EF7A8 File Offset: 0x000ED9A8
		public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Rectangle sourceRect = itemData.GetSourceRect(0, new int?(base.ParentSheetIndex)).Clone();
			sourceRect.Y += 8;
			sourceRect.Height /= 2;
			spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)(yNonTile + 32))), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
			sourceRect.X = 276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(xNonTile * 320) + (double)(yNonTile * 49)) % 700.0 / 100.0) * 8;
			sourceRect.Y = 1965;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32 + 4), (float)(yNonTile + 16 + 4))), new Rectangle?(sourceRect), Color.White * 0.75f, 0f, new Vector2(4f, 4f), 3f, SpriteEffects.None, layerDepth + 1E-05f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32 + 4), (float)(yNonTile + 16 + 4))), new Rectangle?(new Rectangle(88, 1779, 30, 30)), Color.PaleGoldenrod * (Game1.currentLocation.IsOutdoors ? 0.35f : 0.43f), 0f, new Vector2(15f, 15f), 8f + (float)(32.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(xNonTile * 777) + (double)(yNonTile * 9746)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
		}

		// Token: 0x0600139A RID: 5018 RVA: 0x000EF9C4 File Offset: 0x000EDBC4
		public static void drawBasicTorch(SpriteBatch spriteBatch, float x, float y, float layerDepth, float alpha = 1f)
		{
			Rectangle sourceRect = new Rectangle(336, 48, 16, 16);
			sourceRect.Y += 8;
			sourceRect.Height /= 2;
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y + 32f)), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x + 32f + 2f, y + 16f)), new Rectangle?(new Rectangle(88, 1779, 30, 30)), Color.PaleGoldenrod * (Game1.currentLocation.IsOutdoors ? 0.35f : 0.43f), 0f, new Vector2(15f, 15f), 4f + (float)(64.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 777f) + (double)(y * 9746f)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
			sourceRect.X = 276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3204f) + (double)(y * 49f)) % 700.0 / 100.0) * 8;
			sourceRect.Y = 1965;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x + 32f + 4f, y + 16f + 4f)), new Rectangle?(sourceRect), Color.White * 0.75f, 0f, new Vector2(4f, 4f), 3f, SpriteEffects.None, layerDepth + 0.0001f);
		}

		// Token: 0x0600139B RID: 5019 RVA: 0x000EFBE0 File Offset: 0x000EDDE0
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (Game1.eventUp)
			{
				GameLocation currentLocation = Game1.currentLocation;
				bool flag;
				if (currentLocation == null)
				{
					flag = false;
				}
				else
				{
					Event currentEvent = currentLocation.currentEvent;
					flag = ((currentEvent != null) ? new bool?(currentEvent.showGroundObjects) : null).GetValueOrDefault();
				}
				if (!flag && !Game1.currentLocation.IsFarm)
				{
					return;
				}
			}
			if (!this.bigCraftable.Value)
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Rectangle sourceRect = itemData.GetSourceRect(0, new int?(base.ParentSheetIndex)).Clone();
				Rectangle bounds = this.GetBoundingBoxAt(x, y);
				sourceRect.Y += 8;
				sourceRect.Height /= 2;
				spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 + 32))), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, (this.scale.Y > 1f) ? this.getScale().Y : 4f, SpriteEffects.None, (float)(bounds.Center.Y - 16) / 10000f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + 2), (float)(y * 64 + 16))), new Rectangle?(new Rectangle(88, 1779, 30, 30)), Color.PaleGoldenrod * (Game1.currentLocation.IsOutdoors ? 0.35f : 0.43f), 0f, new Vector2(15f, 15f), 4f + (float)(64.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 64 * 777) + (double)(y * 64 * 9746)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, (float)(bounds.Center.Y - 15) / 10000f);
				sourceRect.X = 276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3204) + (double)(y * 49)) % 700.0 / 100.0) * 8;
				sourceRect.Y = 1965;
				sourceRect.Width = 8;
				sourceRect.Height = 8;
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + 4), (float)(y * 64 + 16 + 4))), new Rectangle?(sourceRect), Color.White * 0.75f, 0f, new Vector2(4f, 4f), 3f, SpriteEffects.None, (float)(bounds.Center.Y - 16) / 10000f);
				for (int i = 0; i < this.ashes.Length; i++)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32) + this.ashes[i].X, (float)(y * 64 + 32) + this.ashes[i].Y)), new Rectangle?(new Rectangle(344 + i % 3, 53, 1, 1)), Color.White * 0.5f * ((-100f - this.ashes[i].Y / 2f) / -100f), 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)(bounds.Center.Y - 16) / 10000f);
				}
				return;
			}
			base.draw(spriteBatch, x, y, alpha);
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
			if (this.isOn.Value)
			{
				if (ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "campfire_item"))
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 16 - 4), (float)(y * 64 - 8))), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.0008f);
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 - 12), (float)(y * 64))), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.0009f);
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 - 20), (float)(y * 64 + 12))), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2077) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.001f);
					if (base.QualifiedItemId == "(BC)278")
					{
						ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
						Rectangle r = itemData2.GetSourceRect(1, new int?(base.ParentSheetIndex)).Clone();
						r.Height -= 16;
						Vector2 scaleFactor = this.getScale();
						scaleFactor *= 4f;
						Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64 + 12)));
						Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(64f + scaleFactor.Y / 2f));
						spriteBatch.Draw(itemData2.GetTexture(), destination, new Rectangle?(r), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer + 0.0028f);
						return;
					}
				}
				else
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 16 - 8), (float)(y * 64 - 64 + 8))), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 0.0008f);
				}
			}
		}

		// Token: 0x04000BE2 RID: 3042
		public const float yVelocity = 1f;

		// Token: 0x04000BE3 RID: 3043
		public const float yDissapearLevel = -100f;

		// Token: 0x04000BE4 RID: 3044
		public const double ashChance = 0.015;

		// Token: 0x04000BE5 RID: 3045
		private float color;

		// Token: 0x04000BE6 RID: 3046
		private Vector2[] ashes;

		// Token: 0x04000BE7 RID: 3047
		private float smokePuffTimer;
	}
}

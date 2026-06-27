using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Locations;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley
{
	// Token: 0x02000081 RID: 129
	public class Background
	{
		// Token: 0x060004DE RID: 1246 RVA: 0x00017898 File Offset: 0x00015A98
		public Background(Summit location)
		{
			this.location = location;
			this.summitBG = true;
			this.c = Color.White;
			this.initialViewportY = Game1.viewport.Y;
			this.cloudsTexture = Game1.content.Load<Texture2D>("Minigames\\Clouds");
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x000178F4 File Offset: 0x00015AF4
		public Background(GameLocation location, Color color, bool onlyMapBG)
		{
			this.location = location;
			this.c = color;
			this.onlyMapBG = onlyMapBG;
			this.tempSprites = new TemporaryAnimatedSpriteList();
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x00017928 File Offset: 0x00015B28
		public Background(GameLocation location, Texture2D bgImage, int seedValue, int chunksWide, int chunksHigh, int chunkWidth, int chunkHeight, float zoom, int defaultChunkIndex, int numChunksInSheet, double chanceForDeviation, Color c)
		{
			this.location = location;
			this.backgroundImage = bgImage;
			this.chunksWide = chunksWide;
			this.chunksHigh = chunksHigh;
			this.zoom = zoom;
			this.chunkWidth = chunkWidth;
			this.chunkHeight = chunkHeight;
			this.defaultChunkIndex = defaultChunkIndex;
			this.numChunksInSheet = numChunksInSheet;
			this.chanceForDeviationFromDefault = chanceForDeviation;
			this.c = c;
			Random r = Utility.CreateRandom((double)seedValue, 0.0, 0.0, 0.0, 0.0);
			this.chunks = new int[chunksWide * chunksHigh];
			for (int i = 0; i < chunksHigh * chunksWide; i++)
			{
				if (r.NextDouble() < this.chanceForDeviationFromDefault)
				{
					this.chunks[i] = r.Next(numChunksInSheet);
				}
				else
				{
					this.chunks[i] = defaultChunkIndex;
				}
			}
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x00017A14 File Offset: 0x00015C14
		public virtual void update(xTile.Dimensions.Rectangle viewport)
		{
			Layer backLayer = Game1.currentLocation.map.RequireLayer("Back");
			this.position.X = -((float)(viewport.X + viewport.Width / 2) / ((float)backLayer.LayerWidth * 64f) * ((float)(this.chunksWide * this.chunkWidth) * this.zoom - (float)viewport.Width));
			this.position.Y = -((float)(viewport.Y + viewport.Height / 2) / ((float)backLayer.LayerHeight * 64f) * ((float)(this.chunksHigh * this.chunkHeight) * this.zoom - (float)viewport.Height));
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x00017ACC File Offset: 0x00015CCC
		public virtual void draw(SpriteBatch b)
		{
			if (this.summitBG)
			{
				if (Game1.viewport.X <= -1000)
				{
					return;
				}
				Season season = Game1.GetSeasonForLocation(this.location);
				bool isWinter = season == Season.Winter;
				int seasonOffset;
				if (season != Season.Fall)
				{
					if (season != Season.Winter)
					{
						seasonOffset = 0;
					}
					else
					{
						seasonOffset = 2;
					}
				}
				else
				{
					seasonOffset = 1;
				}
				int yOffset = -Game1.viewport.Y / 4 + this.initialViewportY / 4;
				float alpha = 1f;
				float skyAlpha = 1f;
				Color bgColor = Color.White;
				int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
				int oceanColorAddition = isWinter ? 30 : 0;
				if (Game1.timeOfDay >= 1800)
				{
					this.c = new Color(255f, 255f - Math.Max(100f, (float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f - 1800f), 255f - Math.Max(100f, ((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f - 1800f) / 2f));
					bgColor = (isWinter ? (Color.Black * 0.5f) : (Color.Blue * 0.5f));
					alpha = Math.Max(0f, Math.Min(1f, (2000f - ((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f)) / 200f));
					skyAlpha = Math.Max(0f, Math.Min(1f, (2200f - ((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f)) / 400f));
					Game1.ambientLight = new Color((int)Utility.Lerp(0f, 30f, 1f - alpha), (int)Utility.Lerp(0f, 60f, 1f - alpha), (int)Utility.Lerp(0f, 15f, 1f - alpha));
				}
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(639, 858, 1, 144)), this.c * skyAlpha, 0f, Vector2.Zero, SpriteEffects.None, 5E-08f);
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Microsoft.Xna.Framework.Rectangle?((season == Season.Fall) ? new Microsoft.Xna.Framework.Rectangle(639, 1051, 1, 400) : new Microsoft.Xna.Framework.Rectangle(639 + (seasonOffset + 1), 1051, 1, 400)), this.c * alpha, 0f, Vector2.Zero, SpriteEffects.None, 1E-07f);
				if (Game1.timeOfDay >= 1800)
				{
					b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.viewport.Height / 2 - 780)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 1453, 638, 195)), Color.White * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
				}
				if (Game1.dayOfMonth == 28 && Game1.timeOfDay > 1900)
				{
					b.Draw(Game1.mouseCursors, new Vector2(((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) / 2600f * (float)Game1.viewport.Width / 4f, (float)(Game1.viewport.Height / 2 + 176) - ((float)(adjustedTime - 1900) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) / 700f * (float)Game1.viewport.Height / 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(642, 834, 43, 44)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 5E-08f);
				}
				if (!isWinter && (Game1.currentLocation.IsDebrisWeatherHere() || Game1.currentLocation.IsRainingHere()))
				{
					b.Draw(this.cloudsTexture, new Vector2((float)Game1.viewport.Width - ((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) / 2600f * (float)(Game1.viewport.Width + 2048), (float)(Game1.viewport.Height - 584 - 600 + yOffset / 2 + Game1.dayOfMonth * 6)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 512, 340)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 5.6E-08f);
				}
				if (!this.cursed)
				{
					b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(0, Game1.viewport.Height - 584 + yOffset / 2, Game1.viewport.Width, Game1.viewport.Height / 2), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1)), new Color((int)((float)oceanColorAddition + 60f * skyAlpha), (int)((float)(oceanColorAddition + 10) + 170f * skyAlpha), (int)((float)(oceanColorAddition + 20) + 205f * skyAlpha)), 0f, Vector2.Zero, SpriteEffects.None, 2E-07f);
					b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.viewport.Height - 596 + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149)), Color.White * Math.Max((float)this.c.A, 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
					b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.viewport.Height - 596 + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149)), bgColor * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 2E-06f);
					b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.viewport.Height - 596 + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149)), Color.White * Math.Max((float)this.c.A, 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
					b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.viewport.Height - 596 + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 736 + seasonOffset * 149, 639, 149)), bgColor * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 2E-06f);
					foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in Game1.currentLocation.temporarySprites)
					{
						temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
					}
					b.Draw(this.cloudsTexture, new Vector2(0f, (float)(Game1.viewport.Height - 568) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 554 + seasonOffset * 153, 164, 142)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
					b.Draw(this.cloudsTexture, new Vector2((float)(Game1.viewport.Width - 488), (float)(Game1.viewport.Height - 612) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(390, 543 + seasonOffset * 153, 122, 153)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
					b.Draw(this.cloudsTexture, new Vector2(0f, (float)(Game1.viewport.Height - 568) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 554 + seasonOffset * 153, 164, 142)), Color.Black * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(this.cloudsTexture, new Vector2((float)(Game1.viewport.Width - 488), (float)(Game1.viewport.Height - 612) + (float)yOffset * 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(390, 543 + seasonOffset * 153, 122, 153)), Color.Black * (1f - alpha), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					return;
				}
				if (Game1.options.screenFlash)
				{
					Random r = new Random((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds - Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0));
					for (int i = 0; i < 20; i++)
					{
						Texture2D t = r.Choose(new Texture2D[]
						{
							Game1.mouseCursors,
							Game1.mouseCursors2,
							Game1.objectSpriteSheet,
							Game1.menuTexture,
							Game1.uncoloredMenuTexture,
							Game1.mouseCursors_1_6,
							Game1.bigCraftableSpriteSheet,
							Game1.cropSpriteSheet
						});
						b.Draw(t, new Vector2((float)(r.Next(Game1.viewport.Width) - 100), (float)(r.Next(Game1.viewport.Height) - 100)) + new Vector2((float)((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0)) * 0.03f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(r.Next(t.Width / 16) * 16, r.Next(t.Height / 16) * 16, 16, 16)), Utility.getRandomRainbowColor(r), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
					}
					return;
				}
			}
			else
			{
				if (this.backgroundImage == null)
				{
					Microsoft.Xna.Framework.Rectangle display = new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
					if (this.onlyMapBG)
					{
						display.X = Math.Max(0, -Game1.viewport.X);
						display.Y = Math.Max(0, -Game1.viewport.Y);
						display.Width = Math.Min(Game1.viewport.Width, Game1.currentLocation.map.DisplayWidth);
						display.Height = Math.Min(Game1.viewport.Height, Game1.currentLocation.map.DisplayHeight);
					}
					b.Draw(Game1.staminaRect, display, new Microsoft.Xna.Framework.Rectangle?(Game1.staminaRect.Bounds), this.c, 0f, Vector2.Zero, SpriteEffects.None, 0f);
					for (int j = this.tempSprites.Count - 1; j >= 0; j--)
					{
						if (this.tempSprites[j].update(Game1.currentGameTime))
						{
							this.tempSprites.RemoveAt(j);
						}
						else
						{
							this.tempSprites[j].draw(b, false, 0, 0, 1f);
						}
					}
					return;
				}
				Vector2 v = Vector2.Zero;
				Microsoft.Xna.Framework.Rectangle r2 = new Microsoft.Xna.Framework.Rectangle(0, 0, this.chunkWidth, this.chunkHeight);
				for (int k = 0; k < this.chunks.Length; k++)
				{
					v.X = this.position.X + (float)(k * this.chunkWidth % (this.chunksWide * this.chunkWidth)) * this.zoom;
					v.Y = this.position.Y + (float)(k * this.chunkWidth / (this.chunksWide * this.chunkWidth) * this.chunkHeight) * this.zoom;
					if (this.backgroundImage == null)
					{
						b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)v.X, (int)v.Y, Game1.viewport.Width, Game1.viewport.Height), new Microsoft.Xna.Framework.Rectangle?(r2), this.c, 0f, Vector2.Zero, SpriteEffects.None, 0f);
					}
					else
					{
						r2.X = this.chunks[k] * this.chunkWidth % this.backgroundImage.Width;
						r2.Y = this.chunks[k] * this.chunkWidth / this.backgroundImage.Width * this.chunkHeight;
						b.Draw(this.backgroundImage, v, new Microsoft.Xna.Framework.Rectangle?(r2), this.c, 0f, Vector2.Zero, this.zoom, SpriteEffects.None, 0f);
					}
				}
			}
		}

		// Token: 0x040001D7 RID: 471
		public int defaultChunkIndex;

		// Token: 0x040001D8 RID: 472
		public int numChunksInSheet;

		// Token: 0x040001D9 RID: 473
		public double chanceForDeviationFromDefault;

		// Token: 0x040001DA RID: 474
		protected Texture2D backgroundImage;

		// Token: 0x040001DB RID: 475
		protected Texture2D cloudsTexture;

		// Token: 0x040001DC RID: 476
		protected Vector2 position = Vector2.Zero;

		// Token: 0x040001DD RID: 477
		protected int chunksWide;

		// Token: 0x040001DE RID: 478
		protected int chunksHigh;

		// Token: 0x040001DF RID: 479
		protected int chunkWidth;

		// Token: 0x040001E0 RID: 480
		protected int chunkHeight;

		// Token: 0x040001E1 RID: 481
		protected int[] chunks;

		// Token: 0x040001E2 RID: 482
		protected float zoom;

		// Token: 0x040001E3 RID: 483
		public Color c;

		// Token: 0x040001E4 RID: 484
		protected bool summitBG;

		// Token: 0x040001E5 RID: 485
		protected bool onlyMapBG;

		// Token: 0x040001E6 RID: 486
		public int yOffset;

		// Token: 0x040001E7 RID: 487
		public TemporaryAnimatedSpriteList tempSprites;

		// Token: 0x040001E8 RID: 488
		protected int initialViewportY;

		// Token: 0x040001E9 RID: 489
		public bool cursed;

		// Token: 0x040001EA RID: 490
		protected GameLocation location;
	}
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002EE RID: 750
	public class Sewer : GameLocation
	{
		// Token: 0x0600322E RID: 12846 RVA: 0x00280D6D File Offset: 0x0027EF6D
		public Sewer()
		{
		}

		// Token: 0x0600322F RID: 12847 RVA: 0x00280D8F File Offset: 0x0027EF8F
		public Sewer(string map, string name) : base(map, name)
		{
			this.waterColor.Value = Color.LimeGreen;
		}

		// Token: 0x06003230 RID: 12848 RVA: 0x00280DC4 File Offset: 0x0027EFC4
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			for (float x = -1000f * Game1.options.zoomLevel + this.steamPosition.X; x < (float)Game1.graphics.GraphicsDevice.Viewport.Width + 256f; x += 256f)
			{
				for (float y = -256f + this.steamPosition.Y; y < (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 128); y += 256f)
				{
					b.Draw(this.steamAnimation, new Vector2(x, y), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64)), this.steamColor * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
		}

		// Token: 0x06003231 RID: 12849 RVA: 0x00280EAC File Offset: 0x0027F0AC
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.steamPosition.Y = this.steamPosition.Y - (float)time.ElapsedGameTime.Milliseconds * 0.1f;
			this.steamPosition.Y = this.steamPosition.Y % -256f;
			this.steamPosition -= Game1.getMostRecentViewportMotion();
			if (Game1.random.NextDouble() < 0.001)
			{
				base.localSound("cavedrip", null, null, SoundContext.Default);
			}
		}

		// Token: 0x06003232 RID: 12850 RVA: 0x00280F44 File Offset: 0x0027F144
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "st");
			if (tileIndexAt == 21)
			{
				Game1.warpFarmer("Town", 35, 97, 2);
				DelayedAction.playSoundAfterDelay("stairsdown", 250, null, null, -1, false);
				return true;
			}
			if (tileIndexAt != 84)
			{
				return base.checkAction(tileLocation, viewport, who);
			}
			Utility.TryOpenShopMenu("ShadowShop", null, true);
			return true;
		}

		// Token: 0x06003233 RID: 12851 RVA: 0x00280FB3 File Offset: 0x0027F1B3
		protected override void resetSharedState()
		{
			base.resetSharedState();
			this.waterColor.Value = Color.LimeGreen * 0.75f;
		}

		// Token: 0x06003234 RID: 12852 RVA: 0x00280FD8 File Offset: 0x0027F1D8
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.steamPosition = new Vector2(0f, 0f);
			this.steamAnimation = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation");
			Game1.ambientLight = new Color(250, 140, 160);
		}

		// Token: 0x06003235 RID: 12853 RVA: 0x00281030 File Offset: 0x0027F230
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (Game1.getCharacterFromName("Krobus", true, false).isMarried())
			{
				base.setMapTile(31, 17, 84, "Buildings", "st", null, true);
				base.setMapTile(31, 16, 1, "Front", "st", null, true);
				return;
			}
			base.removeMapTile(31, 17, "Buildings");
			base.removeMapTile(31, 16, "Front");
		}

		// Token: 0x0400218F RID: 8591
		public const float steamZoom = 4f;

		// Token: 0x04002190 RID: 8592
		public const float steamYMotionPerMillisecond = 0.1f;

		// Token: 0x04002191 RID: 8593
		private Texture2D steamAnimation;

		// Token: 0x04002192 RID: 8594
		private Vector2 steamPosition;

		// Token: 0x04002193 RID: 8595
		private Color steamColor = new Color(200, 255, 200);
	}
}

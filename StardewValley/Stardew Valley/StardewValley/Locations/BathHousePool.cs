using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations
{
	// Token: 0x020002BF RID: 703
	public class BathHousePool : GameLocation
	{
		// Token: 0x06002DA2 RID: 11682 RVA: 0x0023A188 File Offset: 0x00238388
		public BathHousePool()
		{
		}

		// Token: 0x06002DA3 RID: 11683 RVA: 0x0023A190 File Offset: 0x00238390
		public BathHousePool(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002DA4 RID: 11684 RVA: 0x0023A19C File Offset: 0x0023839C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.steamPosition = new Vector2((float)(-(float)Game1.viewport.X), (float)(-(float)Game1.viewport.Y));
			this.steamAnimation = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation");
			this.swimShadow = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\swimShadow");
		}

		// Token: 0x06002DA5 RID: 11685 RVA: 0x0023A1FC File Offset: 0x002383FC
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (Game1.player.swimming.Value)
			{
				Game1.player.swimming.Value = false;
			}
			if (Game1.locationRequest != null && !Game1.locationRequest.Name.Contains("BathHouse"))
			{
				Game1.player.bathingClothes.Value = false;
			}
		}

		// Token: 0x06002DA6 RID: 11686 RVA: 0x0023A260 File Offset: 0x00238460
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.currentEvent != null)
			{
				using (List<NPC>.Enumerator enumerator = this.currentEvent.actors.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NPC i = enumerator.Current;
						if (i.swimming.Value)
						{
							b.Draw(this.swimShadow, Game1.GlobalToLocal(Game1.viewport, i.Position + new Vector2(0f, (float)(i.Sprite.SpriteHeight / 3 * 4 + 4))), new Rectangle?(new Rectangle(this.swimShadowFrame * 16, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
						}
					}
					return;
				}
			}
			foreach (NPC j in this.characters)
			{
				if (j.swimming.Value)
				{
					b.Draw(this.swimShadow, Game1.GlobalToLocal(Game1.viewport, j.Position + new Vector2(0f, (float)(j.Sprite.SpriteHeight / 3 * 4 + 4))), new Rectangle?(new Rectangle(this.swimShadowFrame * 16, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
			}
			foreach (Farmer f in this.farmers)
			{
				if (f.swimming.Value)
				{
					b.Draw(this.swimShadow, Game1.GlobalToLocal(Game1.viewport, f.Position + new Vector2(0f, (float)(f.Sprite.SpriteHeight / 4 * 4))), new Rectangle?(new Rectangle(this.swimShadowFrame * 16, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
			}
		}

		// Token: 0x06002DA7 RID: 11687 RVA: 0x0023A4C4 File Offset: 0x002386C4
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			for (float x = this.steamPosition.X; x < (float)Game1.graphics.GraphicsDevice.Viewport.Width + 256f; x += 256f)
			{
				for (float y = this.steamPosition.Y + this.steamYOffset; y < (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 128); y += 256f)
				{
					b.Draw(this.steamAnimation, new Vector2(x, y), new Rectangle?(new Rectangle(0, 0, 64, 64)), Color.White * 0.8f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
		}

		// Token: 0x06002DA8 RID: 11688 RVA: 0x0023A59C File Offset: 0x0023879C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.steamYOffset -= (float)time.ElapsedGameTime.Milliseconds * 0.1f;
			this.steamYOffset %= -256f;
			this.steamPosition -= Game1.getMostRecentViewportMotion();
			this.swimShadowTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.swimShadowTimer <= 0)
			{
				this.swimShadowTimer = 70;
				this.swimShadowFrame++;
				this.swimShadowFrame %= 10;
			}
		}

		// Token: 0x04001F4C RID: 8012
		public const float steamZoom = 4f;

		// Token: 0x04001F4D RID: 8013
		public const float steamYMotionPerMillisecond = 0.1f;

		// Token: 0x04001F4E RID: 8014
		private Texture2D steamAnimation;

		// Token: 0x04001F4F RID: 8015
		private Texture2D swimShadow;

		// Token: 0x04001F50 RID: 8016
		private Vector2 steamPosition;

		// Token: 0x04001F51 RID: 8017
		private float steamYOffset;

		// Token: 0x04001F52 RID: 8018
		private int swimShadowTimer;

		// Token: 0x04001F53 RID: 8019
		private int swimShadowFrame;
	}
}

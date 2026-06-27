using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E6 RID: 742
	public class MermaidHouse : GameLocation
	{
		// Token: 0x06003125 RID: 12581 RVA: 0x0026CB72 File Offset: 0x0026AD72
		public MermaidHouse()
		{
		}

		// Token: 0x06003126 RID: 12582 RVA: 0x0026CB7A File Offset: 0x0026AD7A
		public MermaidHouse(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06003127 RID: 12583 RVA: 0x0026CB84 File Offset: 0x0026AD84
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			Game1.ambientLight = Color.White;
			Game1.changeMusicTrack("none", false, MusicContext.Default);
			this.finalLeftMermaidAlpha = 0f;
			this.finalRightMermaidAlpha = 0f;
			this.finalBigMermaidAlpha = 0f;
			this.blackBGAlpha = 0f;
			this.bigMermaidAlpha = 0f;
			this.oldStopWatchTime = 0f;
			this.showTimer = 0f;
			this.curtainMovement = 0f;
			this.curtainOpenPercent = 0f;
			this.fairyTimer = 0f;
			this.stopWatch = new Stopwatch();
			this.bubbles = new List<Vector2>();
			this.sparkles = new TemporaryAnimatedSpriteList();
			this.alwaysFrontTempSprites = new TemporaryAnimatedSpriteList();
			this.lastFiveClamTones = new List<int>();
			this.pearlRecipient = null;
			this.mermaidFrames = new int[]
			{
				1,
				0,
				2,
				0,
				1,
				0,
				2,
				0,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				4,
				4,
				3,
				3,
				3,
				3,
				0,
				0,
				0,
				0,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				4,
				4,
				3,
				3,
				3,
				3,
				0,
				0,
				0,
				0,
				3,
				3,
				3,
				3,
				4,
				4,
				4,
				4,
				3,
				3,
				3,
				3,
				0,
				0,
				5,
				6,
				5,
				6,
				7,
				8,
				8
			};
		}

		// Token: 0x06003128 RID: 12584 RVA: 0x0026CC88 File Offset: 0x0026AE88
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			switch (base.getTileIndexAt(tileLocation, "Buildings", "mermaid_tiles"))
			{
			case 56:
				this.playClamTone(0, who);
				return true;
			case 57:
				this.playClamTone(1, who);
				return true;
			case 58:
				this.playClamTone(2, who);
				return true;
			case 59:
				this.playClamTone(3, who);
				return true;
			case 60:
				this.playClamTone(4, who);
				return true;
			default:
				return base.checkAction(tileLocation, viewport, who);
			}
		}

		// Token: 0x06003129 RID: 12585 RVA: 0x0026CD01 File Offset: 0x0026AF01
		public void playClamTone(int which)
		{
			this.playClamTone(which, null);
		}

		// Token: 0x0600312A RID: 12586 RVA: 0x0026CD0C File Offset: 0x0026AF0C
		public void playClamTone(int which, Farmer who)
		{
			if (this.oldStopWatchTime < 68000f)
			{
				return;
			}
			int pitch = 1200;
			switch (which)
			{
			case 0:
				pitch = 300;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.HotPink,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(35f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 1:
				pitch = 600;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Orange,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(51f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 2:
				pitch = 800;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Yellow,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(67f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 3:
				pitch = 1000;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Cyan,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(83f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 4:
				pitch = 1200;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Lime,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(99f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			}
			Game1.playSound("clam_tone", new int?(pitch));
			this.lastFiveClamTones.Add(which);
			if (this.lastFiveClamTones.Count > 5)
			{
				this.lastFiveClamTones.RemoveAt(0);
			}
			if (this.lastFiveClamTones.Count == 5 && this.lastFiveClamTones[0] == 0 && this.lastFiveClamTones[1] == 4 && this.lastFiveClamTones[2] == 3 && this.lastFiveClamTones[3] == 1 && this.lastFiveClamTones[4] == 2 && who != null && !who.mailReceived.Contains("gotPearl"))
			{
				who.freezePause = 4500;
				this.fairyTimer = 3500f;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 885,
					texture = this.mermaidSprites,
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.playClamTone),
					extraInfoForEndBehavior = 0
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 1270,
					texture = this.mermaidSprites,
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.playClamTone),
					extraInfoForEndBehavior = 4
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 1655,
					texture = this.mermaidSprites,
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.playClamTone),
					extraInfoForEndBehavior = 3
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 2040,
					texture = this.mermaidSprites,
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.playClamTone),
					extraInfoForEndBehavior = 1
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 2425,
					texture = this.mermaidSprites,
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.playClamTone),
					extraInfoForEndBehavior = 2
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 885,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(28f, 49f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 121
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 1270,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(108f, 49f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 117
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 1655,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(88f, 39f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 113
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 2040,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(48f, 39f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 19
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 2425,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(68f, 29f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 15
				});
				this.pearlRecipient = who;
			}
		}

		// Token: 0x0600312B RID: 12587 RVA: 0x0026D564 File Offset: 0x0026B764
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.sparkles)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(58f, 54f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.mermaidFrames[Math.Min((int)((float)this.stopWatch.ElapsedMilliseconds / 769.2308f), this.mermaidFrames.Length - 1)] * 28, 80, 28, 36)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(27f, 29f) * 4f + new Vector2((float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f)) * 4f * 4f, (float)Math.Cos((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f)) * 4f * 4f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(2 + (int)(this.showTimer % 400f / 100f) * 19, 127, 19, 18)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(97f, 29f) * 4f + new Vector2((float)Math.Cos((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f + 0.1f)) * 4f * 4f, (float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f + 0.1f)) * 4f * 4f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(2 + (int)(this.showTimer % 400f / 100f) * 19, 127, 19, 18)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(16f, 16f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)(144f + 57f * this.curtainOpenPercent), 119, (int)(57f * (1f - this.curtainOpenPercent)), 81)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(73f + 57f * this.curtainOpenPercent, 16f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(200, 119, (int)(57f * (1f - this.curtainOpenPercent)), 81)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}

		// Token: 0x0600312C RID: 12588 RVA: 0x0026D8BC File Offset: 0x0026BABC
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			b.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * this.blackBGAlpha);
			int spacing = Game1.graphics.GraphicsDevice.Viewport.Bounds.Height / 4;
			for (int i = -448; i < Game1.graphics.GraphicsDevice.Viewport.Width + 448; i += 448)
			{
				b.Draw(this.mermaidSprites, new Vector2((float)(i - (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f)), (float)(spacing - spacing * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48)), Color.Lime * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) - (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)spacing - (float)spacing / 4f + (float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f)) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(177, 0, 16, 16)), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i + (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f)), (float)(spacing * 2 - spacing * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48)), Color.Cyan * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) + (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 2) - (float)spacing / 4f + (float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f + 4f)) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(161, 0, 16, 16)), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i - (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f)), (float)(spacing * 3 - spacing * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48)), Color.Orange * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) - (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 3) - (float)spacing / 4f + (float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f + 3f)) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(129, 0, 16, 16)), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i + (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f)), (float)(spacing * 4 - spacing * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48)), Color.HotPink * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) + (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 4) - (float)spacing / 4f + (float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f + 2f)) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(145, 0, 16, 16)), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			}
			b.Draw(this.mermaidSprites, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - 112) + (float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f)) * 64f * 2f, (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y - 140) + (float)Math.Cos((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f * 2f) + 1.5707963267948966) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)(57L * (this.stopWatch.ElapsedMilliseconds % 1538L / 769L)), 0, 57, 70)), Color.White * this.bigMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.alwaysFrontTempSprites)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			foreach (Vector2 v in this.bubbles)
			{
				b.Draw(this.mermaidSprites, v + new Vector2((float)Math.Sin((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f * 4f + v.X)) * 4f * 6f, 0f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(132, 20, 8, 8)), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			}
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-20f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32)), Color.White * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-20f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32)), Color.Orange * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-30f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32)), Color.White * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-30f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32)), Color.Cyan * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-40f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32)), Color.White * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-40f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32)), Color.Lime * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(150f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32)), Color.White * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(150f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32)), Color.Orange * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(160f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32)), Color.White * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(160f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32)), Color.Cyan * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(170f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32)), Color.White * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(170f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32)), Color.Lime * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(43f, 180f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)(57L * (this.stopWatch.ElapsedMilliseconds % 1538L / 769L)), 0, 57, 70)), Color.White * this.finalBigMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}

		// Token: 0x0600312D RID: 12589 RVA: 0x0026E4C0 File Offset: 0x0026C6C0
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			ICue main_player_music = Game1.currentSong;
			if (!Game1.game1.IsMainInstance)
			{
				main_player_music = GameRunner.instance.gameInstances[0].instanceCurrentSong;
			}
			base.UpdateWhenCurrentLocation(time);
			if (this.stopWatch == null)
			{
				return;
			}
			if (!Game1.shouldTimePass(false))
			{
				if (this.stopWatch != null && this.stopWatch.IsRunning)
				{
					this.stopWatch.Stop();
				}
				if (((main_player_music != null) ? main_player_music.Name : null) == "mermaidSong" && !main_player_music.IsPaused && main_player_music.IsPlaying)
				{
					main_player_music.Pause();
				}
			}
			else
			{
				if (this.stopWatch != null && !this.stopWatch.IsRunning && ((main_player_music != null) ? main_player_music.Name : null) == "mermaidSong" && main_player_music.IsPaused)
				{
					this.stopWatch.Start();
				}
				if (((main_player_music != null) ? main_player_music.Name : null) == "mermaidSong" && main_player_music.IsPaused)
				{
					main_player_music.Resume();
				}
			}
			if (Game1.shouldTimePass(false))
			{
				float num = this.showTimer;
				this.showTimer += (float)time.ElapsedGameTime.Milliseconds;
				if (((((main_player_music != null) ? main_player_music.Name : null) == "mermaidSong" && main_player_music.IsPlaying) || (Game1.options.musicVolumeLevel <= 0f && Game1.options.ambientVolumeLevel <= 0f)) && !this.stopWatch.IsRunning)
				{
					this.stopWatch.Start();
				}
				if (this.curtainMovement != 0f)
				{
					this.curtainOpenPercent = Math.Max(0f, Math.Min(1f, this.curtainOpenPercent + this.curtainMovement * (float)time.ElapsedGameTime.Milliseconds));
				}
				if (num < 3000f && this.showTimer >= 3000f)
				{
					Game1.changeMusicTrack("mermaidSong", false, MusicContext.Default);
				}
				Stopwatch stopwatch = this.stopWatch;
				if (stopwatch != null && stopwatch.ElapsedMilliseconds > 0L && this.stopWatch.ElapsedMilliseconds < 1000L)
				{
					this.curtainMovement = 0.0004f;
				}
				this.sparkles.RemoveWhere((TemporaryAnimatedSprite sparkle) => sparkle.update(time));
				this.alwaysFrontTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
				if (this.stopWatch.ElapsedMilliseconds >= 30000L && this.stopWatch.ElapsedMilliseconds < 50000L && (this.blackBGAlpha < 1f || this.bigMermaidAlpha < 1f))
				{
					this.blackBGAlpha += 0.01f;
					this.bigMermaidAlpha += 0.01f;
				}
				if (this.stopWatch.ElapsedMilliseconds > 27692L && this.stopWatch.ElapsedMilliseconds < 55385L)
				{
					if (this.oldStopWatchTime % 769f > (float)(this.stopWatch.ElapsedMilliseconds % 769L))
					{
						this.bubbles.Add(new Vector2((float)Game1.random.Next((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel) - 64), (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel));
					}
					for (int i = 0; i < this.bubbles.Count; i++)
					{
						this.bubbles[i] = new Vector2(this.bubbles[i].X, this.bubbles[i].Y - 0.1f * (float)time.ElapsedGameTime.Milliseconds);
					}
				}
				if (this.oldStopWatchTime < 36923f && this.stopWatch.ElapsedMilliseconds >= 36923L)
				{
					this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = this.mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / 4f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 1)),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (this.oldStopWatchTime < 40000f && this.stopWatch.ElapsedMilliseconds >= 40000L)
				{
					this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = this.mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width * 3f / 4f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 1)),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel * 3f / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (this.oldStopWatchTime < 43077f && this.stopWatch.ElapsedMilliseconds >= 43077L)
				{
					this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = this.mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / 4f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 1)),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (this.oldStopWatchTime < 46154f && this.stopWatch.ElapsedMilliseconds >= 46154L)
				{
					this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = this.mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width * 3f / 4f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 1)),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel * 3f / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (this.stopWatch.ElapsedMilliseconds >= 52308L && (this.blackBGAlpha > 0f || this.bigMermaidAlpha > 0f))
				{
					this.blackBGAlpha -= 0.01f;
					this.bigMermaidAlpha -= 0.01f;
				}
				if (this.stopWatch.ElapsedMilliseconds >= 58462L && this.stopWatch.ElapsedMilliseconds < 60000L && this.finalLeftMermaidAlpha < 1f)
				{
					this.finalLeftMermaidAlpha += 0.01f;
				}
				if (this.stopWatch.ElapsedMilliseconds >= 60000L && this.stopWatch.ElapsedMilliseconds < 62000L && this.finalRightMermaidAlpha < 1f)
				{
					this.finalRightMermaidAlpha += 0.01f;
				}
				if (this.stopWatch.ElapsedMilliseconds >= 61538L && this.stopWatch.ElapsedMilliseconds < 63538L && this.finalBigMermaidAlpha < 1f)
				{
					this.finalBigMermaidAlpha += 0.01f;
				}
				if (this.stopWatch.ElapsedMilliseconds >= 64615L && (this.finalBigMermaidAlpha < 1f || this.finalRightMermaidAlpha < 1f || this.finalLeftMermaidAlpha < 1f))
				{
					this.finalBigMermaidAlpha -= 0.01f;
					this.finalRightMermaidAlpha -= 0.01f;
					this.finalLeftMermaidAlpha -= 0.01f;
				}
				if (this.oldStopWatchTime < 64808f && this.stopWatch.ElapsedMilliseconds >= 64808L)
				{
					for (int j = 0; j < 200; j++)
					{
						this.sparkles.Add(new TemporaryAnimatedSprite
						{
							texture = this.mermaidSprites,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 146, 16, 13),
							animationLength = 9,
							interval = 100f,
							delayBeforeAnimationStart = j * 10,
							position = Utility.getRandomPositionOnScreenNotOnMap(),
							scale = 4f
						});
					}
					Utility.addSprinklesToLocation(this, 5, 5, 9, 5, 2000, 100, Color.White, null, false);
				}
				if (this.oldStopWatchTime < 67500f && this.stopWatch.ElapsedMilliseconds >= 67500L)
				{
					this.curtainMovement = -0.0003f;
				}
				this.oldStopWatchTime = (float)this.stopWatch.ElapsedMilliseconds;
			}
			if (this.fairyTimer > 0f)
			{
				this.fairyTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.fairyTimer < 200f)
				{
					Farmer farmer = this.pearlRecipient;
					if (farmer != null && farmer.FacingDirection == 0)
					{
						this.pearlRecipient.faceDirection(1);
					}
				}
				if (this.fairyTimer < 100f && this.pearlRecipient != null)
				{
					this.pearlRecipient.faceDirection(2);
				}
				if (this.fairyTimer <= 0f && this.pearlRecipient != null)
				{
					foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.temporarySprites)
					{
						temporaryAnimatedSprite.alphaFade = 0.01f;
					}
					this.pearlRecipient.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)797", 1, 0, false), null, false);
					this.pearlRecipient.mailReceived.Add("gotPearl");
				}
			}
		}

		// Token: 0x040020EF RID: 8431
		private Texture2D mermaidSprites;

		// Token: 0x040020F0 RID: 8432
		private float showTimer;

		// Token: 0x040020F1 RID: 8433
		private float curtainMovement;

		// Token: 0x040020F2 RID: 8434
		private float curtainOpenPercent;

		// Token: 0x040020F3 RID: 8435
		private float blackBGAlpha;

		// Token: 0x040020F4 RID: 8436
		private float bigMermaidAlpha;

		// Token: 0x040020F5 RID: 8437
		private float oldStopWatchTime;

		// Token: 0x040020F6 RID: 8438
		private float finalLeftMermaidAlpha;

		// Token: 0x040020F7 RID: 8439
		private float finalRightMermaidAlpha;

		// Token: 0x040020F8 RID: 8440
		private float finalBigMermaidAlpha;

		// Token: 0x040020F9 RID: 8441
		private float fairyTimer;

		// Token: 0x040020FA RID: 8442
		private int[] mermaidFrames;

		// Token: 0x040020FB RID: 8443
		private Stopwatch stopWatch;

		// Token: 0x040020FC RID: 8444
		private List<Vector2> bubbles;

		// Token: 0x040020FD RID: 8445
		private TemporaryAnimatedSpriteList sparkles;

		// Token: 0x040020FE RID: 8446
		private TemporaryAnimatedSpriteList alwaysFrontTempSprites;

		// Token: 0x040020FF RID: 8447
		private List<int> lastFiveClamTones;

		// Token: 0x04002100 RID: 8448
		private Farmer pearlRecipient;
	}
}

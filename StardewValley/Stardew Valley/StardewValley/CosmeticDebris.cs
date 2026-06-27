using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley
{
	// Token: 0x0200008B RID: 139
	public class CosmeticDebris : TemporaryAnimatedSprite
	{
		// Token: 0x0600058C RID: 1420 RVA: 0x0001DB04 File Offset: 0x0001BD04
		public CosmeticDebris(Texture2D texture, Vector2 startingPosition, float rotationSpeed, float xVelocity, float yVelocity, int groundYLevel, Rectangle sourceRect, Color color, ICue tapSound, LightSource light, int lightTailLength, int disappearTime)
		{
			this.timeToDisappearAfterReachingGround = disappearTime;
			this.disappearTimer = this.timeToDisappearAfterReachingGround;
			this.texture = texture;
			this.position = startingPosition;
			this.rotationSpeed = rotationSpeed;
			this.xVelocity = xVelocity;
			this.yVelocity = yVelocity;
			this.sourceRect = sourceRect;
			this.groundYLevel = groundYLevel;
			this.color = color;
			this.tapSound = tapSound;
			this.light = light;
			this.id = Game1.random.Next();
			Game1.currentLightSources.Add(light);
			if (lightTailLength > 0)
			{
				this.lightTail = new Queue<Vector2>();
				this.lightTailLength = lightTailLength;
			}
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0001DBAC File Offset: 0x0001BDAC
		public override bool update(GameTime time)
		{
			LightSource lightSource = this.light;
			Utility.repositionLightSource((lightSource != null) ? lightSource.Id : null, this.position);
			this.yVelocity += 0.3f;
			this.position += new Vector2(this.xVelocity, this.yVelocity);
			this.rotation += this.rotationSpeed;
			if (this.position.Y >= (float)this.groundYLevel)
			{
				this.position.Y = (float)(this.groundYLevel - 1);
				this.yVelocity = -this.yVelocity;
				this.yVelocity *= 0.45f;
				this.xVelocity *= 0.45f;
				this.rotationSpeed *= 0.225f;
				if (!this.tapSound.IsPlaying)
				{
					Game1.playSound(this.tapSound.Name, out this.tapSound);
				}
				this.disappearTimer--;
			}
			if (this.disappearTimer < this.timeToDisappearAfterReachingGround)
			{
				this.disappearTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.disappearTimer <= 0)
				{
					LightSource lightSource2 = this.light;
					Utility.removeLightSource((lightSource2 != null) ? lightSource2.Id : null);
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0001DD0C File Offset: 0x0001BF0C
		public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position), new Rectangle?(this.sourceRect), this.color, this.rotation, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)(this.groundYLevel + 1) / 10000f);
		}

		// Token: 0x040002A4 RID: 676
		public const float gravity = 0.3f;

		// Token: 0x040002A5 RID: 677
		public const float bounciness = 0.45f;

		// Token: 0x040002A6 RID: 678
		private new Vector2 position;

		// Token: 0x040002A7 RID: 679
		private new float rotation;

		// Token: 0x040002A8 RID: 680
		private float rotationSpeed;

		// Token: 0x040002A9 RID: 681
		private float xVelocity;

		// Token: 0x040002AA RID: 682
		private float yVelocity;

		// Token: 0x040002AB RID: 683
		private new Rectangle sourceRect;

		// Token: 0x040002AC RID: 684
		private int groundYLevel;

		// Token: 0x040002AD RID: 685
		private int disappearTimer;

		// Token: 0x040002AE RID: 686
		private int lightTailLength;

		// Token: 0x040002AF RID: 687
		private int timeToDisappearAfterReachingGround;

		// Token: 0x040002B0 RID: 688
		private new int id;

		// Token: 0x040002B1 RID: 689
		private new Color color;

		// Token: 0x040002B2 RID: 690
		private ICue tapSound;

		// Token: 0x040002B3 RID: 691
		private LightSource light;

		// Token: 0x040002B4 RID: 692
		private Queue<Vector2> lightTail;

		// Token: 0x040002B5 RID: 693
		private new Texture2D texture;
	}
}

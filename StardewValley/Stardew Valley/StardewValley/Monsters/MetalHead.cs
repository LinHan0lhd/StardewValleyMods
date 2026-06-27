using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Locations;

namespace StardewValley.Monsters
{
	// Token: 0x0200021F RID: 543
	public class MetalHead : Monster
	{
		// Token: 0x060023E6 RID: 9190 RVA: 0x001885B2 File Offset: 0x001867B2
		public MetalHead()
		{
		}

		// Token: 0x060023E7 RID: 9191 RVA: 0x001885C5 File Offset: 0x001867C5
		public MetalHead(Vector2 tileLocation, MineShaft mine) : this(tileLocation, mine.getMineArea(-1))
		{
		}

		// Token: 0x060023E8 RID: 9192 RVA: 0x001885D8 File Offset: 0x001867D8
		public MetalHead(string name, Vector2 tileLocation) : base(name, tileLocation)
		{
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
			this.c.Value = Color.White;
			base.IsWalkingTowardPlayer = true;
		}

		// Token: 0x060023E9 RID: 9193 RVA: 0x00188628 File Offset: 0x00186828
		public MetalHead(Vector2 tileLocation, int mineArea) : base("Metal Head", tileLocation)
		{
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
			this.c.Value = Color.White;
			base.IsWalkingTowardPlayer = true;
			if (mineArea == 0)
			{
				this.c.Value = Color.White;
				return;
			}
			if (mineArea == 40)
			{
				this.c.Value = Color.Turquoise;
				base.Health *= 2;
				return;
			}
			if (mineArea != 80)
			{
				return;
			}
			this.c.Value = Color.White;
			base.Health *= 3;
		}

		// Token: 0x060023EA RID: 9194 RVA: 0x001886D7 File Offset: 0x001868D7
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.c, "c");
			this.position.Field.AxisAlignedMovement = true;
		}

		// Token: 0x060023EB RID: 9195 RVA: 0x00188707 File Offset: 0x00186907
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "clank");
		}

		// Token: 0x060023EC RID: 9196 RVA: 0x0018871C File Offset: 0x0018691C
		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position, Color.DarkGray, 10, false, 70f, 0, -1, -1f, -1, 0));
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position + new Vector2(-32f, 0f), Color.DarkGray, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 300
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position + new Vector2(32f, 0f), Color.DarkGray, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 600
			});
			base.currentLocation.localSound("monsterdead", null, null, SoundContext.Default);
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.MediumPurple, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 4, 64, 64);
			base.localDeathAnimation();
		}

		// Token: 0x060023ED RID: 9197 RVA: 0x0018887C File Offset: 0x00186A7C
		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				int standingY = base.StandingPixel.Y;
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 42f + this.yOffset), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3.5f + this.scale.Value + this.yOffset / 30f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(48 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.c.Value, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			}
		}

		// Token: 0x060023EE RID: 9198 RVA: 0x00188A0C File Offset: 0x00186C0C
		public override void shedChunks(int number, float scale)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, this.Sprite.getHeight() * 4, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, scale * 4f);
		}

		// Token: 0x060023EF RID: 9199 RVA: 0x00188A78 File Offset: 0x00186C78
		public override List<Item> getExtraDropItems()
		{
			List<Item> extraItems = new List<Item>();
			if ((Game1.stats.getMonstersKilled(this.name.Value) + (int)Game1.uniqueIDForThisGame) % 100 == 0)
			{
				extraItems.Add(ItemRegistry.Create("(H)51", 1, 0, false));
			}
			return extraItems;
		}

		// Token: 0x060023F0 RID: 9200 RVA: 0x00188AC0 File Offset: 0x00186CC0
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (!this.isMoving())
			{
				this.Sprite.StopAnimation();
				return;
			}
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.AnimateUp(time, 0, "");
				return;
			case 1:
				this.Sprite.AnimateRight(time, 0, "");
				return;
			case 2:
				this.Sprite.AnimateDown(time, 0, "");
				return;
			case 3:
				this.Sprite.AnimateLeft(time, 0, "");
				return;
			default:
				return;
			}
		}

		// Token: 0x0400154D RID: 5453
		[XmlElement("c")]
		public readonly NetColor c = new NetColor();
	}
}

using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Network;

namespace StardewValley.Monsters
{
	// Token: 0x0200021C RID: 540
	public class HotHead : MetalHead
	{
		// Token: 0x060023BA RID: 9146 RVA: 0x0018695A File Offset: 0x00184B5A
		public HotHead()
		{
		}

		// Token: 0x060023BB RID: 9147 RVA: 0x00186988 File Offset: 0x00184B88
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.lastAttacker.NetFields, "lastAttacker.NetFields").AddField(this.angry, "angry").AddField(this.timeUntilExplode, "timeUntilExplode");
		}

		// Token: 0x060023BC RID: 9148 RVA: 0x001869D8 File Offset: 0x00184BD8
		public HotHead(Vector2 position) : base("Hot Head", position)
		{
			base.Slipperiness *= 2;
		}

		// Token: 0x060023BD RID: 9149 RVA: 0x00186A28 File Offset: 0x00184C28
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			this.lastAttacker.Value = who;
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if (this.timeUntilExplode.Value == -1f && base.Health < 25)
			{
				base.currentLocation.netAudio.StartPlaying("fuse");
				this.timeUntilExplode.Value = 2.4f;
				base.Speed = 5;
				this.angry.Value = true;
			}
			return result;
		}

		// Token: 0x060023BE RID: 9150 RVA: 0x00186AA8 File Offset: 0x00184CA8
		public override void behaviorAtGameTick(GameTime time)
		{
			if (Game1.IsMasterGame && this.timeUntilExplode.Value > 0f)
			{
				this.timeUntilExplode.Value -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.timeUntilExplode.Value <= 0f)
				{
					base.currentLocation.netAudio.StopPlaying("fuse");
					this.timeUntilExplode.Value = 0f;
					this.DropBomb();
					base.Health = -9999;
					return;
				}
			}
			base.behaviorAtGameTick(time);
		}

		// Token: 0x060023BF RID: 9151 RVA: 0x00186B40 File Offset: 0x00184D40
		public virtual void DropBomb()
		{
			base.currentLocation.netAudio.StopPlaying("fuse");
			if (this.lastAttacker.Value != null)
			{
				Farmer who = this.lastAttacker.Value;
				int idNum = Game1.random.Next();
				base.currentLocation.playSound("thudStep", null, null, SoundContext.Default);
				Vector2 placementTile = base.Tile;
				float y = base.Position.Y;
				float bomb_life = 2.4f;
				if (this.timeUntilExplode.Value >= 0f)
				{
					bomb_life = this.timeUntilExplode.Value;
					base.currentLocation.netAudio.StartPlaying("fuse");
				}
				int loops = Math.Max(1, (int)(bomb_life * 1000f / 100f));
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("Characters\\Monsters\\Hot Head", new Rectangle(16, 64, 16, 16), 25f, 3, loops, placementTile * 64f, false, Game1.random.NextBool())
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = idNum,
						endFunction = new TemporaryAnimatedSprite.endBehavior(base.currentLocation.removeTemporarySpritesWithID),
						bombRadius = 2,
						bombDamage = base.DamageToFarmer,
						Parent = base.currentLocation,
						scale = 4f,
						owner = who
					}
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, true, false, (y + 7f) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
					{
						id = idNum
					}
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, true, false, (y + 7f) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
					{
						delayBeforeAnimationStart = 100,
						id = idNum
					}
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, true, false, (y + 7f) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
					{
						delayBeforeAnimationStart = 200,
						id = idNum
					}
				});
			}
		}

		// Token: 0x060023C0 RID: 9152 RVA: 0x00186E4D File Offset: 0x0018504D
		protected override void sharedDeathAnimation()
		{
			base.sharedDeathAnimation();
			this.DropBomb();
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x00186E5C File Offset: 0x0018505C
		public override void draw(SpriteBatch b)
		{
			if (this.angry.Value)
			{
				if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
				{
					Rectangle source_rect = this.Sprite.SourceRect;
					source_rect.Y += 80;
					int standingY = base.StandingPixel.Y;
					b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 42f + this.yOffset), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3.5f + this.scale.Value + this.yOffset / 30f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(48 + this.yJumpOffset)), new Rectangle?(source_rect), this.c.Value, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
					return;
				}
			}
			else
			{
				base.draw(b);
			}
		}

		// Token: 0x04001535 RID: 5429
		[XmlIgnore]
		public NetFarmerRef lastAttacker = new NetFarmerRef();

		// Token: 0x04001536 RID: 5430
		[XmlIgnore]
		public NetFloat timeUntilExplode = new NetFloat(-1f);

		// Token: 0x04001537 RID: 5431
		[XmlIgnore]
		public NetBool angry = new NetBool();
	}
}

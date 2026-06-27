using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Network;

namespace StardewValley.Monsters
{
	// Token: 0x02000225 RID: 549
	public class Serpent : Monster
	{
		// Token: 0x06002467 RID: 9319 RVA: 0x0018CE96 File Offset: 0x0018B096
		public Serpent()
		{
		}

		// Token: 0x06002468 RID: 9320 RVA: 0x0018CEC6 File Offset: 0x0018B0C6
		public Serpent(Vector2 position) : base("Serpent", position)
		{
			this.InitializeAttributes();
		}

		// Token: 0x06002469 RID: 9321 RVA: 0x0018CF04 File Offset: 0x0018B104
		public Serpent(Vector2 position, string name) : base(name, position)
		{
			this.InitializeAttributes();
			if (name == "Royal Serpent")
			{
				this.segmentCount.Value = Game1.random.Next(3, 7);
				if (Game1.random.NextDouble() < 0.1)
				{
					this.segmentCount.Value = Game1.random.Next(5, 10);
				}
				else if (Game1.random.NextDouble() < 0.01)
				{
					this.segmentCount.Value *= 3;
				}
				this.reloadSprite(false);
				base.MaxHealth += this.segmentCount.Value * 50;
				base.Health = base.MaxHealth;
			}
		}

		// Token: 0x0600246A RID: 9322 RVA: 0x0018CFF4 File Offset: 0x0018B1F4
		public virtual void InitializeAttributes()
		{
			base.Slipperiness = 24 + Game1.random.Next(10);
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			base.Scale = 0.75f;
			base.HideShadow = true;
		}

		// Token: 0x0600246B RID: 9323 RVA: 0x0018D04F File Offset: 0x0018B24F
		public bool IsRoyalSerpent()
		{
			return this.segmentCount.Value > 1;
		}

		// Token: 0x0600246C RID: 9324 RVA: 0x0018D060 File Offset: 0x0018B260
		public override bool TakesDamageFromHitbox(Rectangle area_of_effect)
		{
			if (base.TakesDamageFromHitbox(area_of_effect))
			{
				return true;
			}
			if (this.IsRoyalSerpent())
			{
				Rectangle bounds = this.GetBoundingBox();
				Vector2 offset = new Vector2((float)bounds.X - base.Position.X, (float)bounds.Y - base.Position.Y);
				foreach (Vector3 segment in this.segments)
				{
					bounds.X = (int)(segment.X + offset.X);
					bounds.Y = (int)(segment.Y + offset.Y);
					if (bounds.Intersects(area_of_effect))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0600246D RID: 9325 RVA: 0x0018D134 File Offset: 0x0018B334
		public override bool OverlapsFarmerForDamage(Farmer who)
		{
			if (base.OverlapsFarmerForDamage(who))
			{
				return true;
			}
			if (this.IsRoyalSerpent())
			{
				Rectangle monsterBounds = this.GetBoundingBox();
				Rectangle playerBounds = who.GetBoundingBox();
				Vector2 offset = new Vector2((float)monsterBounds.X - base.Position.X, (float)monsterBounds.Y - base.Position.Y);
				foreach (Vector3 segment in this.segments)
				{
					monsterBounds.X = (int)(segment.X + offset.X);
					monsterBounds.Y = (int)(segment.Y + offset.Y);
					if (monsterBounds.Intersects(playerBounds))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0600246E RID: 9326 RVA: 0x0018D214 File Offset: 0x0018B414
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.killer.NetFields, "killer.NetFields").AddField(this.segmentCount, "segmentCount");
			this.segmentCount.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
			{
				if (new_value > 0)
				{
					this.reloadSprite(false);
				}
			};
		}

		// Token: 0x0600246F RID: 9327 RVA: 0x0018D26C File Offset: 0x0018B46C
		public override void reloadSprite(bool onlyAppearance = false)
		{
			if (this.IsRoyalSerpent())
			{
				this.Sprite = new AnimatedSprite("Characters\\Monsters\\Royal Serpent");
				base.Scale = 1f;
			}
			else
			{
				this.Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent");
				base.Scale = 0.75f;
			}
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			base.HideShadow = true;
		}

		// Token: 0x06002470 RID: 9328 RVA: 0x0018D2DC File Offset: 0x0018B4DC
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
				this.wasHitCounter = 500;
				base.currentLocation.playSound("serpentHit", null, null, SoundContext.Default);
				if (base.Health <= 0)
				{
					this.killer.Value = who;
					base.deathAnimation();
				}
			}
			this.addedSpeed = (float)Game1.random.Next(-1, 1);
			return actualDamage;
		}

		// Token: 0x06002471 RID: 9329 RVA: 0x0018D39F File Offset: 0x0018B59F
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x06002472 RID: 9330 RVA: 0x0018D3A4 File Offset: 0x0018B5A4
		protected override void localDeathAnimation()
		{
			if (this.killer.Value == null)
			{
				return;
			}
			Rectangle bb = this.GetBoundingBox();
			bb.Inflate(-bb.Width / 2 + 1, -bb.Height / 2 + 1);
			Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(bb.Center, 4f, this.killer.Value);
			int xTrajectory = -(int)velocityTowardPlayer.X;
			int yTrajectory = -(int)velocityTowardPlayer.Y;
			if (this.IsRoyalSerpent())
			{
				base.currentLocation.localSound("serpentDie", null, null, SoundContext.Default);
				for (int i = -1; i < this.segments.Count; i++)
				{
					Vector2 segment_position;
					Rectangle source_rect;
					float current_rotation;
					float color_fade;
					if (i == -1)
					{
						segment_position = base.Position;
						source_rect = new Rectangle(0, 64, 32, 32);
						current_rotation = this.rotation;
						color_fade = 0f;
					}
					else
					{
						if (this.segments.Count <= 0 || i >= this.segments.Count)
						{
							return;
						}
						color_fade = (float)(i + 1) / (float)this.segments.Count;
						segment_position = new Vector2(this.segments[i].X, this.segments[i].Y);
						bb.X = (int)(segment_position.X - (float)(bb.Width / 2));
						bb.Y = (int)(segment_position.Y - (float)(bb.Height / 2));
						source_rect = new Rectangle(32, 64, 32, 32);
						if (i == this.segments.Count - 1)
						{
							source_rect = new Rectangle(64, 64, 32, 32);
						}
						current_rotation = this.segments[i].Z;
					}
					Color segment_color = default(Color);
					segment_color.R = (byte)Utility.Lerp(255f, 255f, color_fade);
					segment_color.G = (byte)Utility.Lerp(0f, 166f, color_fade);
					segment_color.B = (byte)Utility.Lerp(0f, 0f, color_fade);
					segment_color.A = byte.MaxValue;
					TemporaryAnimatedSprite current_sprite = new TemporaryAnimatedSprite(this.Sprite.textureName.Value, source_rect, 800f, 1, 0, segment_position, false, false, 0.9f, 0.001f, segment_color, 4f * this.scale.Value, 0.01f, current_rotation + 3.1415927f, (float)((double)Game1.random.Next(3, 5) * 3.141592653589793 / 64.0), false)
					{
						motion = new Vector2((float)xTrajectory, (float)yTrajectory),
						layerDepth = 1f
					};
					current_sprite.alphaFade = 0.025f;
					base.currentLocation.temporarySprites.Add(current_sprite);
					current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, false, 70f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 50,
						motion = new Vector2((float)xTrajectory, (float)yTrajectory),
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite);
					current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, false, 70f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 100,
						startSound = "cowboy_monsterhit",
						motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.8f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite);
					current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 150,
						startSound = "cowboy_monsterhit",
						motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.6f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite);
					current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center), Color.LightGreen * 0.6f, 10, false, 70f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 200,
						startSound = "cowboy_monsterhit",
						motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.4f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite);
					current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 250,
						startSound = "cowboy_monsterhit",
						motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.2f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite);
				}
				return;
			}
			Vector2 standingPixel = Utility.PointToVector2(base.StandingPixel);
			base.currentLocation.localSound("serpentDie", null, null, SoundContext.Default);
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 64, 32, 32), 200f, 4, 0, base.Position, false, false, 0.9f, 0.001f, Color.White, 4f * this.scale.Value, 0.01f, this.rotation + 3.1415927f, (float)((double)Game1.random.Next(3, 5) * 3.141592653589793 / 64.0), false)
			{
				motion = new Vector2((float)xTrajectory, (float)yTrajectory),
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 50,
				startSound = "cowboy_monsterhit",
				motion = new Vector2((float)xTrajectory, (float)yTrajectory),
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 100,
				startSound = "cowboy_monsterhit",
				motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.8f,
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 150,
				startSound = "cowboy_monsterhit",
				motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.6f,
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel, Color.LightGreen * 0.6f, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 200,
				startSound = "cowboy_monsterhit",
				motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.4f,
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 250,
				startSound = "cowboy_monsterhit",
				motion = new Vector2((float)xTrajectory, (float)yTrajectory) * 0.2f,
				layerDepth = 1f
			});
		}

		// Token: 0x06002473 RID: 9331 RVA: 0x0018DCD4 File Offset: 0x0018BED4
		public override List<Item> getExtraDropItems()
		{
			List<Item> items = new List<Item>();
			if (Game1.random.NextDouble() < 0.002)
			{
				items.Add(ItemRegistry.Create("(O)485", 1, 0, false));
			}
			return items;
		}

		// Token: 0x06002474 RID: 9332 RVA: 0x0018DD10 File Offset: 0x0018BF10
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			Vector2 last_position = base.Position;
			bool is_royal = this.IsRoyalSerpent();
			int standingY = base.StandingPixel.Y;
			for (int i = -1; i < this.segmentCount.Value; i++)
			{
				float sort_offset = (float)(i + 1) * -0.25f / 10000f;
				float max_offset = (float)this.segmentCount.Value * -0.25f / 10000f - 5E-05f;
				if ((float)(standingY - 1) / 10000f + max_offset < 0f)
				{
					sort_offset += -((float)(standingY - 1) / 10000f + max_offset);
				}
				Rectangle draw_rect = this.Sprite.SourceRect;
				Vector2 shadow_position = base.Position;
				Vector2 draw_position;
				float current_rotation;
				if (i == -1)
				{
					if (is_royal)
					{
						draw_rect = new Rectangle(0, 0, 32, 32);
					}
					draw_position = base.Position;
					current_rotation = this.rotation;
				}
				else
				{
					if (i >= this.segments.Count)
					{
						return;
					}
					Vector3 pos = this.segments[i];
					draw_position = new Vector2(pos.X, pos.Y);
					draw_rect = new Rectangle(32, 0, 32, 32);
					if (i == this.segments.Count - 1)
					{
						draw_rect = new Rectangle(64, 0, 32, 32);
					}
					current_rotation = pos.Z;
					shadow_position = (last_position + draw_position) / 2f;
				}
				if (Utility.isOnScreen(draw_position, 128))
				{
					Vector2 local_draw_position = Game1.GlobalToLocal(Game1.viewport, draw_position) + this.drawOffset + new Vector2(0f, (float)this.yJumpOffset);
					Vector2 local_shadow_position = Game1.GlobalToLocal(Game1.viewport, shadow_position) + this.drawOffset + new Vector2(0f, (float)this.yJumpOffset);
					int boundsHeight = this.GetBoundingBox().Height;
					b.Draw(Game1.shadowTexture, local_shadow_position + new Vector2(64f, (float)boundsHeight), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(standingY - 1) / 10000f + sort_offset);
					b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, (float)(boundsHeight / 2)), new Rectangle?(draw_rect), Color.White, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + sort_offset)));
					if (this.isGlowing)
					{
						b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, (float)(boundsHeight / 2)), new Rectangle?(draw_rect), this.glowingColor * this.glowingTransparency, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + 0.0001f + sort_offset)));
					}
					if (is_royal)
					{
						sort_offset += -5E-05f;
						current_rotation = 0f;
						draw_rect = new Rectangle(96, 0, 32, 32);
						local_draw_position = Game1.GlobalToLocal(Game1.viewport, last_position) + this.drawOffset + new Vector2(0f, (float)this.yJumpOffset);
						if (i > 0)
						{
							b.Draw(Game1.shadowTexture, local_draw_position + new Vector2(64f, (float)boundsHeight), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(standingY - 1) / 10000f + sort_offset);
						}
						b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, (float)(boundsHeight / 2)), new Rectangle?(draw_rect), Color.White, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + sort_offset)));
						if (this.isGlowing)
						{
							b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, (float)(boundsHeight / 2)), new Rectangle?(draw_rect), this.glowingColor * this.glowingTransparency, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + 0.0001f + sort_offset)));
						}
					}
				}
				last_position = draw_position;
			}
		}

		// Token: 0x06002475 RID: 9333 RVA: 0x0018E2C0 File Offset: 0x0018C4C0
		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 8, (int)position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 96);
		}

		// Token: 0x06002476 RID: 9334 RVA: 0x0018E2FC File Offset: 0x0018C4FC
		protected override void updateAnimation(GameTime time)
		{
			if (this.IsRoyalSerpent())
			{
				if (this.segments.Count < this.segmentCount.Value)
				{
					for (int i = 0; i < this.segmentCount.Value; i++)
					{
						Vector2 position = base.Position;
						this.segments.Add(new Vector3(position.X, position.Y, 0f));
					}
				}
				Vector2 last_position = base.Position;
				for (int j = 0; j < this.segments.Count; j++)
				{
					Vector2 current_position = new Vector2(this.segments[j].X, this.segments[j].Y);
					Vector2 offset = current_position - last_position;
					int segment_length = 64;
					int num = (int)offset.Length();
					offset.Normalize();
					if (num > segment_length)
					{
						current_position = offset * (float)segment_length + last_position;
					}
					double angle = Math.Atan2((double)offset.Y, (double)offset.X) - 1.5707963267948966;
					this.segments[j] = new Vector3(current_position.X, current_position.Y, (float)angle);
					last_position = current_position;
				}
			}
			base.updateAnimation(time);
			if (this.wasHitCounter >= 0)
			{
				this.wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			if (!this.IsRoyalSerpent())
			{
				this.Sprite.Animate(time, 0, 9, 40f);
			}
			if (this.withinPlayerThreshold() && this.invincibleCountdown <= 0)
			{
				Point monsterPixel = base.StandingPixel;
				Point standingPixel = base.Player.StandingPixel;
				float xSlope = (float)(-(float)(standingPixel.X - monsterPixel.X));
				float ySlope = (float)(standingPixel.Y - monsterPixel.Y);
				float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
				if (t < 64f)
				{
					this.xVelocity = Math.Max(-7f, Math.Min(7f, this.xVelocity * 1.1f));
					this.yVelocity = Math.Max(-7f, Math.Min(7f, this.yVelocity * 1.1f));
				}
				xSlope /= t;
				ySlope /= t;
				if (this.wasHitCounter <= 0)
				{
					this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.5707964f;
					if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextBool())
					{
						this.turningRight = true;
					}
					else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
					{
						this.turningRight = false;
					}
					if (this.turningRight)
					{
						this.rotation -= (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
					}
					else
					{
						this.rotation += (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
					}
					this.rotation %= 6.2831855f;
					this.wasHitCounter = 5 + Game1.random.Next(-1, 2);
				}
				float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
				xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
				ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
				this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * 7f))
				{
					this.xVelocity -= -xSlope * maxAccel / 6f;
				}
				if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * 7f))
				{
					this.yVelocity -= -ySlope * maxAccel / 6f;
				}
			}
			base.resetAnimationSpeed();
		}

		// Token: 0x06002477 RID: 9335 RVA: 0x0018E770 File Offset: 0x0018C970
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (double.IsNaN((double)this.xVelocity) || double.IsNaN((double)this.yVelocity))
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
			if (this.withinPlayerThreshold() && this.invincibleCountdown <= 0)
			{
				this.faceDirection(2);
			}
		}

		// Token: 0x04001582 RID: 5506
		public const float rotationIncrement = 0.049087387f;

		// Token: 0x04001583 RID: 5507
		[XmlIgnore]
		public int wasHitCounter;

		// Token: 0x04001584 RID: 5508
		[XmlIgnore]
		public float targetRotation;

		// Token: 0x04001585 RID: 5509
		[XmlIgnore]
		public bool turningRight;

		// Token: 0x04001586 RID: 5510
		[XmlIgnore]
		public readonly NetFarmerRef killer = new NetFarmerRef().Delayed(false);

		// Token: 0x04001587 RID: 5511
		public List<Vector3> segments = new List<Vector3>();

		// Token: 0x04001588 RID: 5512
		public NetInt segmentCount = new NetInt(0);
	}
}

using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewValley.Monsters
{
	// Token: 0x0200021E RID: 542
	public class Leaper : Monster
	{
		// Token: 0x060023D5 RID: 9173 RVA: 0x00187D30 File Offset: 0x00185F30
		public Leaper()
		{
		}

		// Token: 0x060023D6 RID: 9174 RVA: 0x00187D88 File Offset: 0x00185F88
		public Leaper(Vector2 position) : base("Spider", position)
		{
			this.forceOneTileWide.Value = true;
			base.IsWalkingTowardPlayer = false;
			this.nextLeap = Utility.RandomFloat(1f, 1.5f, null);
			this.isHardModeMonster.Value = true;
			this.reloadSprite(false);
		}

		// Token: 0x060023D7 RID: 9175 RVA: 0x00187E1F File Offset: 0x0018601F
		public override int GetBaseDifficultyLevel()
		{
			return 1;
		}

		// Token: 0x060023D8 RID: 9176 RVA: 0x00187E22 File Offset: 0x00186022
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060023D9 RID: 9177 RVA: 0x00187E50 File Offset: 0x00186050
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.leapDuration, "leapDuration").AddField(this.leapProgress, "leapProgress").AddField(this.leapStartPosition, "leapStartPosition").AddField(this.leapEndPosition, "leapEndPosition").AddField(this.leaping, "leaping");
			this.leapProgress.Interpolated(true, true);
			this.leaping.Interpolated(true, true);
			this.leaping.fieldChangeVisibleEvent += this.OnLeapingChanged;
		}

		// Token: 0x060023DA RID: 9178 RVA: 0x00187EEE File Offset: 0x001860EE
		public virtual void OnLeapingChanged(NetBool field, bool old_value, bool new_value)
		{
		}

		// Token: 0x060023DB RID: 9179 RVA: 0x00187EF0 File Offset: 0x001860F0
		public override bool isInvincible()
		{
			return this.leaping.Value || base.isInvincible();
		}

		// Token: 0x060023DC RID: 9180 RVA: 0x00187F07 File Offset: 0x00186107
		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		// Token: 0x060023DD RID: 9181 RVA: 0x00187F0C File Offset: 0x0018610C
		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("monsterdead", null, null, SoundContext.Default);
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.DarkRed, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 4, 64, 64);
		}

		// Token: 0x060023DE RID: 9182 RVA: 0x00187F8C File Offset: 0x0018618C
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x060023DF RID: 9183 RVA: 0x00187F8E File Offset: 0x0018618E
		public override void defaultMovementBehavior(GameTime time)
		{
		}

		// Token: 0x060023E0 RID: 9184 RVA: 0x00187F90 File Offset: 0x00186190
		public override void noMovementProgressNearPlayerBehavior()
		{
		}

		// Token: 0x060023E1 RID: 9185 RVA: 0x00187F94 File Offset: 0x00186194
		public override void update(GameTime time, GameLocation location)
		{
			this.farmerPassesThrough = true;
			base.update(time, location);
			if (this.leaping.Value)
			{
				this.yJumpGravity = 0f;
				float progress = this.leapProgress.Value;
				if (!Game1.IsMasterGame)
				{
					float total_length = (this.leapStartPosition.Value - this.leapEndPosition.Value).Length();
					if (total_length == 0f)
					{
						progress = 0f;
					}
					else
					{
						progress = (this.leapStartPosition.Value - base.Position).Length() / total_length;
					}
					if (progress < 0f)
					{
						progress = 0f;
					}
					if (progress > 1f)
					{
						progress = 1f;
					}
				}
				this.yJumpOffset = (int)(Math.Sin((double)progress * 3.141592653589793) * -64.0 * 3.0);
				return;
			}
			this.yJumpOffset = 0;
		}

		// Token: 0x060023E2 RID: 9186 RVA: 0x00188083 File Offset: 0x00186283
		protected override void updateAnimation(GameTime time)
		{
			if (this.leaping.Value)
			{
				this.Sprite.CurrentFrame = 2;
			}
			else
			{
				this.Sprite.Animate(time, 0, 2, 500f);
			}
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060023E3 RID: 9187 RVA: 0x001880C0 File Offset: 0x001862C0
		public virtual bool IsValidLandingTile(Vector2 tile, bool check_other_characters = false)
		{
			MineShaft mine = base.currentLocation as MineShaft;
			if (mine != null && !mine.isTileOnClearAndSolidGround(tile))
			{
				return false;
			}
			if (base.currentLocation.IsTileOccupiedBy(tile, ~(CollisionMask.Characters | CollisionMask.Farmers), CollisionMask.None, false) || !base.currentLocation.isTileOnMap(tile) || !base.currentLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
			{
				return false;
			}
			Microsoft.Xna.Framework.Rectangle my_bounding_box = this.GetBoundingBox();
			if (check_other_characters && base.currentLocation != null)
			{
				foreach (Character character in base.currentLocation.characters)
				{
					if (character != this && character.GetBoundingBox().Intersects(my_bounding_box))
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		// Token: 0x060023E4 RID: 9188 RVA: 0x001881A8 File Offset: 0x001863A8
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (this.leaping.Value)
			{
				this.leapProgress.Value += (float)time.ElapsedGameTime.TotalSeconds / this.leapDuration.Value;
				if (this.leapProgress.Value >= 1f)
				{
					this.leapProgress.Value = 1f;
				}
				base.Position = new Vector2(Utility.Lerp(this.leapStartPosition.X, this.leapEndPosition.X, this.leapProgress.Value), Utility.Lerp(this.leapStartPosition.Y, this.leapEndPosition.Y, this.leapProgress.Value));
				if (this.leapProgress.Value == 1f)
				{
					this.leaping.Value = false;
					this.leapProgress.Value = 0f;
					if (!this.IsValidLandingTile(base.Tile, true))
					{
						this.nextLeap = 0.1f;
						return;
					}
				}
			}
			else
			{
				if (this.nextLeap > 0f)
				{
					this.nextLeap -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (this.nextLeap <= 0f)
				{
					Vector2? found_tile = null;
					Vector2 current_tile = base.Tile;
					current_tile.X = (float)((int)current_tile.X);
					current_tile.X = (float)((int)current_tile.X);
					if (this.withinPlayerThreshold(5) && base.Player != null)
					{
						Vector2 target_tile = base.Tile;
						if (Game1.random.NextDouble() < 0.6000000238418579)
						{
							this.nextLeap = Utility.RandomFloat(1.25f, 1.5f, null);
							target_tile = base.Player.Tile;
							target_tile.X = (float)((int)Math.Round((double)target_tile.X));
							target_tile.Y = (float)((int)Math.Round((double)target_tile.Y));
							target_tile.X += (float)Game1.random.Next(-1, 2);
							target_tile.Y += (float)Game1.random.Next(-1, 2);
						}
						else
						{
							this.nextLeap = Utility.RandomFloat(0.1f, 0.2f, null);
							target_tile.X += (float)Game1.random.Next(-1, 2);
							target_tile.Y += (float)Game1.random.Next(-1, 2);
						}
						if (this.IsValidLandingTile(target_tile, false))
						{
							found_tile = new Vector2?(target_tile);
						}
					}
					if (found_tile == null)
					{
						for (int i = 0; i < 8; i++)
						{
							Vector2 offset = new Vector2((float)Game1.random.Next(-4, 5), (float)Game1.random.Next(-4, 5));
							if (!(offset == Vector2.Zero))
							{
								Vector2 tile = current_tile + offset;
								if (this.IsValidLandingTile(tile, false))
								{
									this.nextLeap = Utility.RandomFloat(0.6f, 1.5f, null);
									found_tile = new Vector2?(tile);
									break;
								}
							}
						}
					}
					if (found_tile != null)
					{
						if (Utility.isOnScreen(base.Position, 128))
						{
							base.currentLocation.playSound("batFlap", null, null, SoundContext.Default);
						}
						this.leapProgress.Value = 0f;
						this.leaping.Value = true;
						this.leapStartPosition.Value = base.Position;
						this.leapEndPosition.Value = found_tile.Value * 64f;
						return;
					}
					this.nextLeap = Utility.RandomFloat(0.25f, 0.5f, null);
				}
			}
		}

		// Token: 0x060023E5 RID: 9189 RVA: 0x00188554 File Offset: 0x00186754
		public override void shedChunks(int number, float scale)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, 64, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
		}

		// Token: 0x04001547 RID: 5447
		public NetFloat leapDuration = new NetFloat(0.75f);

		// Token: 0x04001548 RID: 5448
		public NetFloat leapProgress = new NetFloat(0f);

		// Token: 0x04001549 RID: 5449
		public NetBool leaping = new NetBool(false);

		// Token: 0x0400154A RID: 5450
		public NetVector2 leapStartPosition = new NetVector2();

		// Token: 0x0400154B RID: 5451
		public NetVector2 leapEndPosition = new NetVector2();

		// Token: 0x0400154C RID: 5452
		public float nextLeap;
	}
}

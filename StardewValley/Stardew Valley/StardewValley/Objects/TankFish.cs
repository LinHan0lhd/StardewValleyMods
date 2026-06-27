using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects.Trinkets;

namespace StardewValley.Objects
{
	// Token: 0x020001AB RID: 427
	public class TankFish
	{
		// Token: 0x06001E5B RID: 7771 RVA: 0x0015CEE8 File Offset: 0x0015B0E8
		public TankFish(FishTankFurniture tank, Item item)
		{
			this._tank = tank;
			this.fishItemId = item.ItemId;
			string rawAquariumData;
			if (!this._tank.GetAquariumData().TryGetValue(item.ItemId, out rawAquariumData))
			{
				rawAquariumData = "0/float";
				this.isErrorFish = true;
			}
			string[] aquarium_fish_split = rawAquariumData.Split('/', StringSplitOptions.None);
			string rawTexture = ArgUtility.Get(aquarium_fish_split, 6, null, false);
			if (rawTexture != null)
			{
				try
				{
					this._texture = Game1.content.Load<Texture2D>(rawTexture);
				}
				catch (Exception)
				{
					this.isErrorFish = true;
				}
			}
			if (this._texture == null)
			{
				this._texture = this._tank.GetAquariumTexture();
			}
			string rawHatOffset = ArgUtility.Get(aquarium_fish_split, 7, null, false);
			if (rawHatOffset != null)
			{
				try
				{
					string[] point_split = ArgUtility.SplitBySpace(rawHatOffset);
					this.hatPosition = new Point?(new Point(int.Parse(point_split[0]), int.Parse(point_split[1])));
				}
				catch (Exception)
				{
					this.hatPosition = null;
				}
			}
			this.fishIndex = int.Parse(aquarium_fish_split[0]);
			this.currentFrame = this.fishIndex;
			this.zPosition = Utility.RandomFloat(4f, 10f, null);
			this.fishScale = 0.75f;
			string fish_data;
			if (DataLoader.Fish(Game1.content).TryGetValue(item.ItemId, out fish_data))
			{
				string[] fish_split = fish_data.Split('/', StringSplitOptions.None);
				if (!(fish_split[1] == "trap"))
				{
					this.minimumVelocity = Utility.RandomFloat(0.25f, 0.35f, null);
					if (fish_split[2] == "smooth")
					{
						this.minimumVelocity = Utility.RandomFloat(0.5f, 0.6f, null);
					}
					if (fish_split[2] == "dart")
					{
						this.minimumVelocity = 0f;
					}
				}
			}
			string text = ArgUtility.Get(aquarium_fish_split, 1, null, true);
			if (text != null)
			{
				switch (text.Length)
				{
				case 3:
					if (text == "eel")
					{
						this.fishType = TankFish.FishType.Eel;
						this.minimumVelocity = Utility.Clamp(this.fishScale, 0.3f, 0.4f);
					}
					break;
				case 5:
				{
					char c = text[0];
					if (c != 'c')
					{
						if (c == 'f')
						{
							if (text == "float")
							{
								this.fishType = TankFish.FishType.Float;
							}
						}
					}
					else if (text == "crawl")
					{
						this.fishType = TankFish.FishType.Crawl;
						this.minimumVelocity = 0f;
					}
					break;
				}
				case 6:
				{
					char c = text[0];
					if (c != 'g')
					{
						if (c == 's')
						{
							if (text == "static")
							{
								this.fishType = TankFish.FishType.Static;
							}
						}
					}
					else if (text == "ground")
					{
						this.fishType = TankFish.FishType.Ground;
						this.zPosition = 4f;
						this.minimumVelocity = 0f;
					}
					break;
				}
				case 10:
					if (text == "cephalopod")
					{
						this.fishType = TankFish.FishType.Cephalopod;
						this.minimumVelocity = 0f;
					}
					break;
				case 11:
					if (text == "front_crawl")
					{
						this.fishType = TankFish.FishType.Crawl;
						this.zPosition = 3f;
						this.minimumVelocity = 0f;
					}
					break;
				}
			}
			string rawIdleAnimation = ArgUtility.Get(aquarium_fish_split, 2, null, false);
			if (rawIdleAnimation != null)
			{
				string[] array = ArgUtility.SplitBySpace(rawIdleAnimation);
				this.idleAnimation = new List<int>();
				foreach (string frame in array)
				{
					this.idleAnimation.Add(int.Parse(frame));
				}
				this.SetAnimation(this.idleAnimation);
			}
			string rawDartStartFrames = ArgUtility.Get(aquarium_fish_split, 3, null, false);
			if (rawDartStartFrames != null)
			{
				string[] array3 = ArgUtility.SplitBySpace(rawDartStartFrames);
				this.dartStartAnimation = new List<int>();
				foreach (string frame2 in array3)
				{
					this.dartStartAnimation.Add(int.Parse(frame2));
				}
			}
			string rawDartHoldFrames = ArgUtility.Get(aquarium_fish_split, 4, null, false);
			if (rawDartHoldFrames != null)
			{
				string[] array4 = ArgUtility.SplitBySpace(rawDartHoldFrames);
				this.dartHoldAnimation = new List<int>();
				foreach (string frame3 in array4)
				{
					this.dartHoldAnimation.Add(int.Parse(frame3));
				}
			}
			string rawDartEndFrames = ArgUtility.Get(aquarium_fish_split, 5, null, false);
			if (rawDartEndFrames != null)
			{
				string[] array5 = ArgUtility.SplitBySpace(rawDartEndFrames);
				this.dartEndAnimation = new List<int>();
				foreach (string frame4 in array5)
				{
					this.dartEndAnimation.Add(int.Parse(frame4));
				}
			}
			Rectangle tank_bounds_local = this._tank.GetTankBounds();
			tank_bounds_local.X = 0;
			tank_bounds_local.Y = 0;
			this.position = Vector2.Zero;
			this.position = Utility.getRandomPositionInThisRectangle(tank_bounds_local, Game1.random);
			this.nextSwim = Utility.RandomFloat(0.1f, 10f, null);
			this.nextBubble = Utility.RandomFloat(0.1f, 10f, null);
			this.facingLeft = (Game1.random.Next(2) == 1);
			if (this.facingLeft)
			{
				this.velocity = new Vector2(-1f, 0f);
			}
			else
			{
				this.velocity = new Vector2(1f, 0f);
			}
			this.velocity *= this.minimumVelocity;
			if (item.QualifiedItemId == "(TR)FrogEgg")
			{
				this.fishType = TankFish.FishType.Hop;
				this._texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
				this.frogVariant = ((item as Trinket).GetEffect() as CompanionTrinketEffect).Variant;
				this.isErrorFish = false;
			}
			if (this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Crawl || this.fishType == TankFish.FishType.Hop || this.fishType == TankFish.FishType.Static)
			{
				this.position.Y = 0f;
			}
			this.ConstrainToTank();
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x0015D51C File Offset: 0x0015B71C
		public void SetAnimation(List<int> frames)
		{
			if (this.fishType == TankFish.FishType.Hop)
			{
				return;
			}
			if (this.currentAnimation == frames)
			{
				return;
			}
			this.currentAnimation = frames;
			this.currentAnimationFrame = 0;
			this.currentFrameTime = 0f;
			List<int> list = this.currentAnimation;
			if (list != null && list.Count > 0)
			{
				this.currentFrame = frames[0];
			}
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x0015D57C File Offset: 0x0015B77C
		public virtual void Draw(SpriteBatch b, float alpha, float draw_layer)
		{
			SpriteEffects sprite_effects = SpriteEffects.None;
			int draw_offset = -12;
			int slice_size = 8;
			if (this.fishType == TankFish.FishType.Eel)
			{
				slice_size = 4;
			}
			int slice_offset = slice_size;
			if (this.facingLeft)
			{
				sprite_effects = SpriteEffects.FlipHorizontally;
				slice_offset *= -1;
				draw_offset = -draw_offset - slice_size;
			}
			float bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(this.position.X / 32f)) * 2f;
			if (this.fishType == TankFish.FishType.Crawl || this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Static)
			{
				bob = 0f;
			}
			float scale = this.GetScale();
			int cols = this._texture.Width / 24;
			int sprite_sheet_x = this.currentFrame % cols * 24;
			int sprite_sheet_y = this.currentFrame / cols * 48;
			int wiggle_start_pixels = 10;
			float wiggle_amount = 1f;
			if (this.fishType == TankFish.FishType.Eel)
			{
				wiggle_start_pixels = 20;
				bob *= 0f;
			}
			float hatOffsetY = -12f;
			float angle = 0f;
			if (this.isErrorFish)
			{
				angle = 0f;
				IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(F)");
				b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle?(itemType.GetErrorSourceRect()), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
			}
			else
			{
				switch (this.fishType)
				{
				case TankFish.FishType.Cephalopod:
				case TankFish.FishType.Float:
					angle = Utility.Clamp(this.velocity.X, -0.5f, 0.5f);
					b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle?(new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24)), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
					break;
				case TankFish.FishType.Ground:
				case TankFish.FishType.Crawl:
				case TankFish.FishType.Static:
					angle = 0f;
					b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle?(new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24)), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
					break;
				case TankFish.FishType.Hop:
				{
					int frame = 0;
					if (this.position.Y > 0f)
					{
						if ((double)this.velocity.Y > 0.2)
						{
							if ((double)this.velocity.Y > 0.3)
							{
								frame = 1;
							}
							else
							{
								frame = 2;
							}
						}
						else
						{
							frame = 3;
						}
					}
					else if (this.nextSwim <= 3f)
					{
						frame = ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 >= 200.0) ? 5 : 6);
					}
					Rectangle rect = new Rectangle(frame * 16, 16 + this.frogVariant * 16, 16, 16);
					Color c = Color.White;
					if (this.frogVariant == 7)
					{
						c = Utility.GetPrismaticColor(0, 1f);
					}
					b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(16f, -8f)), new Rectangle?(rect), c * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
					break;
				}
				default:
					for (int slice = 0; slice < 24 / slice_size; slice++)
					{
						float multiplier = (float)(slice * slice_size) / (float)wiggle_start_pixels;
						multiplier = 1f - multiplier;
						float velocity_multiplier = this.velocity.Length() / 1f;
						float time_multiplier = 1f;
						float position_multiplier = 0f;
						velocity_multiplier = Utility.Clamp(velocity_multiplier, 0.2f, 1f);
						multiplier = Utility.Clamp(multiplier, 0f, 1f);
						if (this.fishType == TankFish.FishType.Eel)
						{
							multiplier = 1f;
							velocity_multiplier = 1f;
							time_multiplier = 0.1f;
							position_multiplier = 4f;
						}
						if (this.facingLeft)
						{
							position_multiplier *= -1f;
						}
						float yOffset = (float)(Math.Sin((double)(slice * 20) + Game1.currentGameTime.TotalGameTime.TotalSeconds * 25.0 * (double)time_multiplier + (double)(position_multiplier * this.position.X / 16f)) * (double)wiggle_amount * (double)multiplier * (double)velocity_multiplier);
						if (slice == 24 / slice_size - 1)
						{
							hatOffsetY = -12f + yOffset;
						}
						b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2((float)(draw_offset + slice * slice_offset), bob + yOffset) * 4f * scale), new Rectangle?(new Rectangle(sprite_sheet_x + slice * slice_size, sprite_sheet_y, slice_size, 24)), Color.White * alpha, 0f, new Vector2(0f, 12f), 4f * scale, sprite_effects, draw_layer);
					}
					break;
				}
			}
			float hatOffsetX = (float)(this.facingLeft ? 12 : -12);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(this.GetWorldPosition().X, (float)this._tank.GetTankBounds().Bottom - this.zPosition * 4f)), null, Color.White * alpha * 0.75f, 0f, new Vector2((float)(Game1.shadowTexture.Width / 2), (float)(Game1.shadowTexture.Height / 2)), new Vector2(4f * scale, 1f), SpriteEffects.None, this._tank.GetFishSortRegion().X - 1E-07f);
			int hatsDrawn = 0;
			foreach (TankFish fish in this._tank.tankFish)
			{
				if (fish == this)
				{
					break;
				}
				if (fish.CanWearHat())
				{
					hatsDrawn++;
				}
			}
			if (this.CanWearHat())
			{
				int hatsSoFar = 0;
				foreach (Item item in this._tank.heldItems)
				{
					Hat hat = item as Hat;
					if (hat != null)
					{
						if (hatsSoFar == hatsDrawn)
						{
							Vector2 hatPlacementOffset = new Vector2((float)this.hatPosition.Value.X, (float)this.hatPosition.Value.Y);
							if (this.facingLeft)
							{
								hatPlacementOffset.X *= -1f;
							}
							Vector2 hatOffset = new Vector2(hatOffsetX, hatOffsetY) + hatPlacementOffset;
							if (angle != 0f)
							{
								float cos = (float)Math.Cos((double)angle);
								float sin = (float)Math.Sin((double)angle);
								hatOffset.X = hatOffset.X * cos - hatOffset.Y * sin;
								hatOffset.Y = hatOffset.X * sin + hatOffset.Y * cos;
							}
							hatOffset *= 4f * scale;
							Vector2 pos = Game1.GlobalToLocal(this.GetWorldPosition() + hatOffset);
							pos.Y += bob;
							int direction;
							if (this.fishType == TankFish.FishType.Cephalopod || this.fishType == TankFish.FishType.Static)
							{
								direction = 2;
							}
							else if (this.facingLeft)
							{
								direction = 3;
							}
							else
							{
								direction = 1;
							}
							pos -= new Vector2(10f, 10f);
							pos += new Vector2(3f, 3f) * scale * 3f;
							pos -= new Vector2(10f, 10f) * scale * 3f;
							hat.draw(b, pos, scale, 1f, draw_layer + 1E-08f, direction, false);
							hatsDrawn++;
							break;
						}
						hatsSoFar++;
					}
				}
			}
		}

		// Token: 0x06001E5E RID: 7774 RVA: 0x0015DE28 File Offset: 0x0015C028
		[MemberNotNullWhen(true, "hatPosition")]
		public bool CanWearHat()
		{
			return this.hatPosition != null;
		}

		// Token: 0x06001E5F RID: 7775 RVA: 0x0015DE38 File Offset: 0x0015C038
		public Vector2 GetWorldPosition()
		{
			return new Vector2((float)this._tank.GetTankBounds().X + this.position.X, (float)this._tank.GetTankBounds().Bottom - this.position.Y - this.zPosition * 4f);
		}

		// Token: 0x06001E60 RID: 7776 RVA: 0x0015DE94 File Offset: 0x0015C094
		public void ConstrainToTank()
		{
			Rectangle tank_bounds = this._tank.GetTankBounds();
			Rectangle bounds = this.GetBounds();
			tank_bounds.X = 0;
			tank_bounds.Y = 0;
			if (bounds.X < tank_bounds.X)
			{
				this.position.X = this.position.X + (float)(tank_bounds.X - bounds.X);
				bounds = this.GetBounds();
			}
			if (bounds.Y < tank_bounds.Y)
			{
				this.position.Y = this.position.Y - (float)(tank_bounds.Y - bounds.Y);
				bounds = this.GetBounds();
			}
			if (bounds.Right > tank_bounds.Right)
			{
				this.position.X = this.position.X + (float)(tank_bounds.Right - bounds.Right);
				bounds = this.GetBounds();
			}
			if (this.fishType == TankFish.FishType.Crawl || this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Static || this.fishType == TankFish.FishType.Hop)
			{
				if (this.position.Y > (float)tank_bounds.Bottom)
				{
					this.position.Y = this.position.Y - ((float)tank_bounds.Bottom - this.position.Y);
					return;
				}
			}
			else if (bounds.Bottom > tank_bounds.Bottom)
			{
				this.position.Y = this.position.Y - (float)(tank_bounds.Bottom - bounds.Bottom);
			}
		}

		// Token: 0x06001E61 RID: 7777 RVA: 0x0015DFEA File Offset: 0x0015C1EA
		public virtual float GetScale()
		{
			return this.fishScale;
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x0015DFF4 File Offset: 0x0015C1F4
		public Rectangle GetBounds()
		{
			Vector2 dimensions = new Vector2(24f, 18f);
			dimensions *= 4f * this.GetScale();
			if (this.fishType == TankFish.FishType.Crawl || this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Static || this.fishType == TankFish.FishType.Hop)
			{
				return new Rectangle((int)(this.position.X - dimensions.X / 2f), (int)((float)this._tank.GetTankBounds().Height - this.position.Y - dimensions.Y), (int)dimensions.X, (int)dimensions.Y);
			}
			return new Rectangle((int)(this.position.X - dimensions.X / 2f), (int)((float)this._tank.GetTankBounds().Height - this.position.Y - dimensions.Y / 2f), (int)dimensions.X, (int)dimensions.Y);
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x0015E0F4 File Offset: 0x0015C2F4
		public virtual void Update(GameTime time)
		{
			List<int> list = this.currentAnimation;
			if (list != null && list.Count > 0)
			{
				this.currentFrameTime += (float)time.ElapsedGameTime.TotalSeconds;
				float seconds_per_frame = 0.125f;
				if (this.currentFrameTime > seconds_per_frame)
				{
					this.currentAnimationFrame += (int)(this.currentFrameTime / seconds_per_frame);
					this.currentFrameTime %= seconds_per_frame;
					if (this.currentAnimationFrame >= this.currentAnimation.Count)
					{
						if (this.currentAnimation == this.idleAnimation)
						{
							this.currentAnimationFrame %= this.currentAnimation.Count;
							this.currentFrame = this.currentAnimation[this.currentAnimationFrame];
						}
						else if (this.currentAnimation == this.dartStartAnimation)
						{
							if (this.dartHoldAnimation != null)
							{
								this.SetAnimation(this.dartHoldAnimation);
							}
							else
							{
								this.SetAnimation(this.idleAnimation);
							}
						}
						else if (this.currentAnimation == this.dartHoldAnimation)
						{
							this.currentAnimationFrame %= this.currentAnimation.Count;
							this.currentFrame = this.currentAnimation[this.currentAnimationFrame];
						}
						else if (this.currentAnimation == this.dartEndAnimation)
						{
							this.SetAnimation(this.idleAnimation);
						}
					}
					else
					{
						this.currentFrame = this.currentAnimation[this.currentAnimationFrame];
					}
				}
			}
			if (this.fishType != TankFish.FishType.Static)
			{
				Rectangle local_tank_bounds = this._tank.GetTankBounds();
				local_tank_bounds.X = 0;
				local_tank_bounds.Y = 0;
				float velocity_x = this.velocity.X;
				if (this.fishType == TankFish.FishType.Crawl)
				{
					velocity_x = Utility.Clamp(velocity_x, -0.5f, 0.5f);
				}
				this.position.X = this.position.X + velocity_x;
				Rectangle bounds = this.GetBounds();
				if (bounds.Left < local_tank_bounds.Left || bounds.Right > local_tank_bounds.Right)
				{
					this.ConstrainToTank();
					bounds = this.GetBounds();
					this.velocity.X = this.velocity.X * -1f;
					this.facingLeft = !this.facingLeft;
				}
				this.position.Y = this.position.Y + this.velocity.Y;
				bounds = this.GetBounds();
				if (bounds.Top < local_tank_bounds.Top || bounds.Bottom > local_tank_bounds.Bottom)
				{
					this.ConstrainToTank();
					this.velocity.Y = this.velocity.Y * 0f;
				}
				float move_magnitude = this.velocity.Length();
				if (move_magnitude > this.minimumVelocity)
				{
					float deceleration = 0.015f;
					if (this.fishType == TankFish.FishType.Crawl || this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Hop)
					{
						deceleration = 0.03f;
					}
					move_magnitude = Utility.Lerp(move_magnitude, this.minimumVelocity, deceleration);
					if (move_magnitude < 0.0001f)
					{
						move_magnitude = 0f;
					}
					this.velocity.Normalize();
					this.velocity *= move_magnitude;
					if (this.currentAnimation == this.dartHoldAnimation && move_magnitude <= this.minimumVelocity + 0.5f)
					{
						List<int> list2 = this.dartEndAnimation;
						if (list2 != null && list2.Count > 0)
						{
							this.SetAnimation(this.dartEndAnimation);
						}
						else
						{
							List<int> list3 = this.idleAnimation;
							if (list3 != null && list3.Count > 0)
							{
								this.SetAnimation(this.idleAnimation);
							}
						}
					}
				}
				this.nextSwim -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.nextSwim <= 0f)
				{
					if (this.numberOfDarts == 0)
					{
						this.numberOfDarts = Game1.random.Next(1, 4);
						this.nextSwim = Utility.RandomFloat(6f, 12f, null);
						TankFish.FishType fishType = this.fishType;
						if (fishType != TankFish.FishType.Cephalopod)
						{
							if (fishType == TankFish.FishType.Hop)
							{
								this.numberOfDarts = 0;
							}
						}
						else
						{
							this.nextSwim = Utility.RandomFloat(2f, 5f, null);
						}
						if (Game1.random.NextDouble() < 0.30000001192092896)
						{
							this.facingLeft = !this.facingLeft;
						}
					}
					else
					{
						this.nextSwim = Utility.RandomFloat(0.1f, 0.5f, null);
						this.numberOfDarts--;
						if (Game1.random.NextDouble() < 0.05000000074505806)
						{
							this.facingLeft = !this.facingLeft;
						}
					}
					List<int> list4 = this.dartStartAnimation;
					if (list4 != null && list4.Count > 0)
					{
						this.SetAnimation(this.dartStartAnimation);
					}
					else
					{
						List<int> list5 = this.dartHoldAnimation;
						if (list5 != null && list5.Count > 0)
						{
							this.SetAnimation(this.dartHoldAnimation);
						}
					}
					this.velocity.X = 1.5f;
					if (this._tank.getTilesWide() <= 2)
					{
						this.velocity.X = this.velocity.X * 0.5f;
					}
					if (this.facingLeft)
					{
						this.velocity.X = this.velocity.X * -1f;
					}
					switch (this.fishType)
					{
					case TankFish.FishType.Cephalopod:
						this.velocity.Y = Utility.RandomFloat(0.5f, 0.75f, null);
						goto IL_5A9;
					case TankFish.FishType.Ground:
						this.velocity.X = this.velocity.X * 0.5f;
						this.velocity.Y = Utility.RandomFloat(0.5f, 0.25f, null);
						goto IL_5A9;
					case TankFish.FishType.Hop:
						this.velocity.Y = Utility.RandomFloat(0.35f, 0.65f, null);
						goto IL_5A9;
					}
					this.velocity.Y = Utility.RandomFloat(-0.5f, 0.5f, null);
					IL_5A9:
					if (this.fishType == TankFish.FishType.Crawl)
					{
						this.velocity.Y = 0f;
					}
				}
			}
			if (this.fishType == TankFish.FishType.Cephalopod || this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Crawl || this.fishType == TankFish.FishType.Static || this.fishType == TankFish.FishType.Hop)
			{
				float fall_speed = 0.2f;
				if (this.fishType == TankFish.FishType.Static)
				{
					fall_speed = 0.6f;
				}
				if (this.position.Y > 0f)
				{
					this.position.Y = this.position.Y - fall_speed;
				}
			}
			this.nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.nextBubble <= 0f)
			{
				this.nextBubble = Utility.RandomFloat(1f, 10f, null);
				float x_offset = 0f;
				if (this.fishType == TankFish.FishType.Ground || this.fishType == TankFish.FishType.Normal || this.fishType == TankFish.FishType.Eel)
				{
					x_offset = 32f;
				}
				if (this.facingLeft)
				{
					x_offset *= -1f;
				}
				x_offset *= this.fishScale;
				this._tank.bubbles.Add(new Vector4(this.position.X + x_offset, this.position.Y + this.zPosition, this.zPosition, 0.25f));
			}
			this.ConstrainToTank();
		}

		// Token: 0x040012A3 RID: 4771
		public const int field_spriteIndex = 0;

		// Token: 0x040012A4 RID: 4772
		public const int field_type = 1;

		// Token: 0x040012A5 RID: 4773
		public const int field_idleAnimations = 2;

		// Token: 0x040012A6 RID: 4774
		public const int field_dartStartFrames = 3;

		// Token: 0x040012A7 RID: 4775
		public const int field_dartHoldFrames = 4;

		// Token: 0x040012A8 RID: 4776
		public const int field_dartEndFrames = 5;

		// Token: 0x040012A9 RID: 4777
		public const int field_texture = 6;

		// Token: 0x040012AA RID: 4778
		public const int field_hatOffset = 7;

		// Token: 0x040012AB RID: 4779
		protected FishTankFurniture _tank;

		// Token: 0x040012AC RID: 4780
		public Vector2 position;

		// Token: 0x040012AD RID: 4781
		public float zPosition;

		// Token: 0x040012AE RID: 4782
		public bool facingLeft;

		// Token: 0x040012AF RID: 4783
		public Vector2 velocity = Vector2.Zero;

		// Token: 0x040012B0 RID: 4784
		protected Texture2D _texture;

		// Token: 0x040012B1 RID: 4785
		public float nextSwim;

		// Token: 0x040012B2 RID: 4786
		public string fishItemId = "";

		// Token: 0x040012B3 RID: 4787
		public int fishIndex;

		// Token: 0x040012B4 RID: 4788
		public int currentFrame;

		// Token: 0x040012B5 RID: 4789
		public Point? hatPosition;

		// Token: 0x040012B6 RID: 4790
		public int frogVariant;

		// Token: 0x040012B7 RID: 4791
		public int numberOfDarts;

		// Token: 0x040012B8 RID: 4792
		public TankFish.FishType fishType;

		// Token: 0x040012B9 RID: 4793
		public float minimumVelocity;

		// Token: 0x040012BA RID: 4794
		public float fishScale = 1f;

		// Token: 0x040012BB RID: 4795
		public List<int> currentAnimation;

		// Token: 0x040012BC RID: 4796
		public List<int> idleAnimation;

		// Token: 0x040012BD RID: 4797
		public List<int> dartStartAnimation;

		// Token: 0x040012BE RID: 4798
		public List<int> dartHoldAnimation;

		// Token: 0x040012BF RID: 4799
		public List<int> dartEndAnimation;

		// Token: 0x040012C0 RID: 4800
		public int currentAnimationFrame;

		// Token: 0x040012C1 RID: 4801
		public float currentFrameTime;

		// Token: 0x040012C2 RID: 4802
		public float nextBubble;

		// Token: 0x040012C3 RID: 4803
		public bool isErrorFish;

		// Token: 0x02000556 RID: 1366
		public enum FishType
		{
			// Token: 0x04002B46 RID: 11078
			Normal,
			// Token: 0x04002B47 RID: 11079
			Eel,
			// Token: 0x04002B48 RID: 11080
			Cephalopod,
			// Token: 0x04002B49 RID: 11081
			Float,
			// Token: 0x04002B4A RID: 11082
			Ground,
			// Token: 0x04002B4B RID: 11083
			Crawl,
			// Token: 0x04002B4C RID: 11084
			Hop,
			// Token: 0x04002B4D RID: 11085
			Static
		}
	}
}

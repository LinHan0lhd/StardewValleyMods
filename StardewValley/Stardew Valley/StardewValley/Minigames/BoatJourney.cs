using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Minigames
{
	// Token: 0x02000232 RID: 562
	public class BoatJourney : IMinigame
	{
		// Token: 0x0600251D RID: 9501 RVA: 0x0019DB58 File Offset: 0x0019BD58
		public BoatJourney()
		{
			Game1.globalFadeToClear(null, 0.02f);
			Game1.changeMusicTrack("sweet", false, MusicContext.MiniGame);
			this.mapSourceRectangle = new Rectangle(0, 0, 640, 849);
			this.texture = Game1.temporaryContent.Load<Texture2D>(BoatJourney.GetAssetName());
			this.changeScreenSize();
			Rectangle cloud_start_rectangle = new Rectangle(0, 112, 640, 528);
			this._interpolatedPoints = new List<Vector2>();
			this._cumulativeDistances = new List<float>();
			this._interpolatedPoints.Add(this.points[0]);
			for (int i = 0; i < this.points.Length - 3; i++)
			{
				this._interpolatedPoints.Add(this.points[i + 1]);
				for (int t = 0; t < 10; t++)
				{
					Vector2 interpolated_point = Vector2.CatmullRom(this.points[i], this.points[i + 1], this.points[i + 2], this.points[i + 3], (float)t / 10f);
					this._interpolatedPoints.Add(interpolated_point);
				}
				this._interpolatedPoints.Add(this.points[i + 2]);
			}
			this._interpolatedPoints.Add(this.points[this.points.Length - 1]);
			Vector2 point_start = this._interpolatedPoints[0];
			this._totalPathDistance = 0f;
			for (int j = 0; j < this._interpolatedPoints.Count; j++)
			{
				this._totalPathDistance += (point_start - this._interpolatedPoints[j]).Length();
				point_start = this._interpolatedPoints[j];
				this._cumulativeDistances.Add(this._totalPathDistance);
			}
			this.entities = new List<BoatJourney.Entity>();
			for (int k = 0; k < 8; k++)
			{
				Vector2 cloud_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
				Rectangle cloud_rectangle = new Rectangle(640, 0, 150, 130);
				if (Game1.random.NextDouble() < 0.44999998807907104)
				{
					cloud_rectangle = new Rectangle(640, 136, 150, 120);
				}
				else if (Game1.random.NextDouble() < 0.25)
				{
					cloud_rectangle = new Rectangle(640, 256, 150, 80);
				}
				BoatJourney.Entity cloud_entity = new BoatJourney.Entity(this, BoatJourney.GetAssetName(), cloud_rectangle, new Vector2((float)(cloud_rectangle.Width / 2), (float)cloud_rectangle.Height), cloud_position);
				cloud_entity.velocity = new Vector2(-1f, -1f) * Utility.RandomFloat(0.05f, 0.15f, null);
				cloud_entity.drawOnTop = true;
				this.entities.Add(cloud_entity);
			}
			List<Vector2> boat_positions = new List<Vector2>();
			for (int l = 0; l < 2; l++)
			{
				if (Game1.random.NextDouble() < 0.30000001192092896)
				{
					this.SpawnBoat(new Rectangle(640, 416, 32, 32), new Vector2(-1f, 0f), boat_positions);
				}
			}
			if (Game1.random.NextDouble() < 0.20000000298023224)
			{
				this.SpawnBoat(new Rectangle(704, 416, 32, 32), new Vector2(-1f, 0f), boat_positions);
			}
			for (int m = 0; m < 2; m++)
			{
				if (Game1.random.NextDouble() < 0.30000001192092896)
				{
					this.SpawnBoat(new Rectangle(640, 448, 32, 32), new Vector2(1f, 0f), boat_positions);
				}
			}
			for (int n = 0; n < 16; n++)
			{
				Vector2 wave_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
				BoatJourney.Wave wave_entity = new BoatJourney.Wave(this, wave_position);
				this.entities.Add(wave_entity);
			}
			for (int i2 = 0; i2 < 8; i2++)
			{
				BoatJourney.WaterSparkle sparkle_entity = new BoatJourney.WaterSparkle(this);
				this.entities.Add(sparkle_entity);
			}
			Vector2 gull_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
			this.CreateFlockOfSeagulls((int)gull_position.X, (int)gull_position.Y, Game1.random.Next(4, 8));
			for (int i3 = 0; i3 < 3; i3++)
			{
				gull_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
				this.CreateFlockOfSeagulls((int)gull_position.X, (int)gull_position.Y, 1);
			}
			this._seagulls.Sort((BoatJourney.Entity a, BoatJourney.Entity b) => a.position.Y.CompareTo(b.position.Y));
			this._boat = new BoatJourney.Boat(this, BoatJourney.GetAssetName(), new Rectangle(640, 352, 32, 32), new Vector2(16f, 16f), new Vector2(293f, 53f));
			this._boat.smokeStack = new Vector2?(new Vector2(0f, -12f));
			this._boat.numFrames = 2;
			this.entities.Add(this._boat);
			BoatJourney.Entity dinosaur = new BoatJourney.Entity(this, BoatJourney.GetAssetName(), new Rectangle(643, 538, 29, 17), Vector2.Zero, new Vector2(16f, 829f));
			dinosaur.numFrames = 2;
			dinosaur.frameInterval = 0.75f;
			this.entities.Add(dinosaur);
		}

		// Token: 0x0600251E RID: 9502 RVA: 0x0019E206 File Offset: 0x0019C406
		private static string GetAssetName()
		{
			return "Minigames\\" + Game1.currentSeason + "_boatJourneyMap";
		}

		// Token: 0x0600251F RID: 9503 RVA: 0x0019E21C File Offset: 0x0019C41C
		public void SpawnBoat(Rectangle boat_sprite_rect, Vector2 direction, List<Vector2> other_boat_positions)
		{
			Vector2 potential_point;
			for (;;)
			{
				potential_point = Game1.random.ChooseFrom(this._interpolatedPoints);
				if (new Rectangle(0, 112, 640, 528).Contains((int)potential_point.X, (int)potential_point.Y))
				{
					potential_point += direction * Utility.RandomFloat(8f, 64f, null);
					bool fail = false;
					using (List<Vector2>.Enumerator enumerator = other_boat_positions.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if ((enumerator.Current - potential_point).Length() < 24f)
							{
								fail = true;
								break;
							}
						}
					}
					if (!fail)
					{
						break;
					}
				}
			}
			BoatJourney.Boat boat = new BoatJourney.Boat(this, BoatJourney.GetAssetName(), boat_sprite_rect, new Vector2(16f, 14f), potential_point);
			boat.velocity = direction * Utility.RandomFloat(0.05f, 0.1f, null);
			boat.numFrames = 2;
			boat.frameInterval = 0.75f;
			other_boat_positions.Add(potential_point);
			this.entities.Add(boat);
		}

		// Token: 0x06002520 RID: 9504 RVA: 0x0019E33C File Offset: 0x0019C53C
		public void CreateFlockOfSeagulls(int x, int y, int depth)
		{
			Vector2 velocity = new Vector2(-0.15f, -0.25f);
			BoatJourney.Entity seagull = new BoatJourney.Entity(this, BoatJourney.GetAssetName(), new Rectangle(646, 560, 5, 14), new Vector2(2f, 14f), new Vector2((float)x, (float)y));
			seagull.numFrames = 8;
			seagull.currentFrame = Game1.random.Next(0, 8);
			seagull.velocity = velocity + new Vector2(Utility.RandomFloat(-0.001f, 0.001f, null), Utility.RandomFloat(-0.001f, 0.001f, null));
			seagull.frameInterval = Utility.RandomFloat(0.1f, 0.15f, null);
			this.entities.Add(seagull);
			this._seagulls.Add(seagull);
			Vector2 left = new Vector2((float)x, (float)y);
			Vector2 right = new Vector2((float)x, (float)y);
			for (int i = 1; i < depth; i++)
			{
				left.X -= (float)Game1.random.Next(5, 8);
				left.Y += (float)Game1.random.Next(6, 9);
				right.X += (float)Game1.random.Next(5, 8);
				right.Y += (float)Game1.random.Next(6, 9);
				seagull = new BoatJourney.Entity(this, BoatJourney.GetAssetName(), new Rectangle(646, 560, 5, 14), new Vector2(2f, 14f), left);
				seagull.numFrames = 8;
				seagull.currentFrame = Game1.random.Next(0, 8);
				seagull.velocity = velocity + new Vector2(Utility.RandomFloat(-0.001f, 0.001f, null), Utility.RandomFloat(-0.001f, 0.001f, null));
				seagull.frameInterval = Utility.RandomFloat(0.1f, 0.15f, null);
				this.entities.Add(seagull);
				this._seagulls.Add(seagull);
				seagull = new BoatJourney.Entity(this, BoatJourney.GetAssetName(), new Rectangle(646, 560, 5, 14), new Vector2(2f, 14f), right);
				seagull.numFrames = 8;
				seagull.currentFrame = Game1.random.Next(0, 8);
				seagull.velocity = velocity + new Vector2(Utility.RandomFloat(-0.001f, 0.001f, null), Utility.RandomFloat(-0.001f, 0.001f, null));
				seagull.frameInterval = Utility.RandomFloat(0.1f, 0.15f, null);
				this.entities.Add(seagull);
				this._seagulls.Add(seagull);
			}
		}

		// Token: 0x06002521 RID: 9505 RVA: 0x0019E5E4 File Offset: 0x0019C7E4
		public Vector2 TransformDraw(Vector2 position)
		{
			position.X = (float)((int)(position.X * this._zoomLevel) - (int)this._upperLeft.X);
			position.Y = (float)((int)(position.Y * this._zoomLevel) - (int)this._upperLeft.Y);
			return position;
		}

		// Token: 0x06002522 RID: 9506 RVA: 0x0019E638 File Offset: 0x0019C838
		public Rectangle TransformDraw(Rectangle dest)
		{
			dest.X = (int)((float)dest.X * this._zoomLevel) - (int)this._upperLeft.X;
			dest.Y = (int)((float)dest.Y * this._zoomLevel) - (int)this._upperLeft.Y;
			dest.Width = (int)((float)dest.Width * this._zoomLevel);
			dest.Height = (int)((float)dest.Height * this._zoomLevel);
			return dest;
		}

		// Token: 0x06002523 RID: 9507 RVA: 0x0019E6B8 File Offset: 0x0019C8B8
		public bool tick(GameTime time)
		{
			if (this._fadeComplete)
			{
				Game1.warpFarmer("IslandSouth", 21, 43, 0);
				return true;
			}
			this._age += (float)time.ElapsedGameTime.TotalSeconds;
			this.entities.RemoveAll((BoatJourney.Entity entity) => entity.Update(time));
			this.viewTarget.X = this._boat.position.X;
			this.viewTarget.Y = this._boat.position.Y;
			List<BoatJourney.Entity> seagulls = this._seagulls;
			if (seagulls != null && seagulls.Count > 0 && this._boat.position.Y > this._seagulls[0].position.Y)
			{
				if (Math.Abs(this._boat.position.X - this._seagulls[0].position.X) < 128f && Game1.random.NextDouble() < 0.25)
				{
					Game1.playSound("seagulls", null);
				}
				this._seagulls.RemoveAt(0);
			}
			if (this._interpolatedPoints.Count > 1)
			{
				if (this.departureDelay > 0f)
				{
					this.departureDelay -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					if (this.traveledBoatDistance < this._totalPathDistance)
					{
						float desired_boat_speed = this.boatSpeed;
						if (this._interpolatedPoints.Count <= 2)
						{
							desired_boat_speed = this.dockSpeed;
						}
						this._currentBoatSpeed = Utility.MoveTowards(this._currentBoatSpeed, desired_boat_speed, 0.01f);
						this.traveledBoatDistance += this._currentBoatSpeed;
						if (this.traveledBoatDistance > this._totalPathDistance)
						{
							this.traveledBoatDistance = this._totalPathDistance;
						}
					}
					this._nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this._nextSlosh <= 0f)
					{
						this._nextSlosh = 0.75f;
						Game1.playSound("waterSlosh", null);
					}
				}
				while (this._interpolatedPoints.Count >= 2 && this.traveledBoatDistance >= this._cumulativeDistances[1])
				{
					this._interpolatedPoints.RemoveAt(0);
					this._cumulativeDistances.RemoveAt(0);
				}
				if (this._interpolatedPoints.Count <= 1)
				{
					this._interpolatedPoints.Clear();
					this._cumulativeDistances.Clear();
					Game1.globalFadeToBlack(delegate
					{
						this._fadeComplete = true;
					}, 0.02f);
				}
				else
				{
					Vector2 direction = this._interpolatedPoints[1] - this._interpolatedPoints[0];
					if (Math.Abs(direction.X) > Math.Abs(direction.Y))
					{
						if (direction.X < 0f)
						{
							this._boat.SetSourceRect(new Rectangle(704, 384, 32, 32));
						}
						else
						{
							this._boat.SetSourceRect(new Rectangle(704, 352, 32, 32));
						}
					}
					else if (direction.Y > 0f)
					{
						this._boat.SetSourceRect(new Rectangle(640, 384, 32, 32));
					}
					else
					{
						this._boat.SetSourceRect(new Rectangle(640, 352, 32, 32));
					}
					float t = (this.traveledBoatDistance - this._cumulativeDistances[0]) / (this._cumulativeDistances[1] - this._cumulativeDistances[0]);
					this._boat.position = new Vector2(Utility.Lerp(this._interpolatedPoints[0].X, this._interpolatedPoints[1].X, t), Utility.Lerp(this._interpolatedPoints[0].Y, this._interpolatedPoints[1].Y, t));
				}
			}
			this._upperLeft.X = this.viewTarget.X * this._zoomLevel - (float)(Game1.viewport.Width / 2);
			this._upperLeft.Y = this.viewTarget.Y * this._zoomLevel - (float)(Game1.viewport.Height / 2);
			if (this._upperLeft.Y < 0f)
			{
				this._upperLeft.Y = 0f;
			}
			if (this._upperLeft.Y + (float)Game1.viewport.Height > (float)this.mapSourceRectangle.Height * this._zoomLevel)
			{
				this._upperLeft.Y = (float)this.mapSourceRectangle.Height * this._zoomLevel - (float)Game1.viewport.Height;
			}
			if (this.nextSmoke <= 0f)
			{
				this.nextSmoke = 0.75f;
				BoatJourney.Entity smoke_entity = new BoatJourney.Entity(this, BoatJourney.GetAssetName(), new Rectangle(640, 480, 16, 16), new Vector2(8f, 8f), new Vector2(350f, 665f));
				smoke_entity.numFrames = 7;
				Vector2 velocity = new Vector2(Utility.RandomFloat(-0.04f, -0.03f, null), Utility.RandomFloat(-0.1f, -0.2f, null));
				smoke_entity.velocity = velocity;
				smoke_entity.destroyAfterAnimation = true;
				this.entities.Add(smoke_entity);
			}
			else
			{
				this.nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			return false;
		}

		// Token: 0x06002524 RID: 9508 RVA: 0x0019EC84 File Offset: 0x0019CE84
		public void afterFade()
		{
			Game1.currentMinigame = null;
			Game1.globalFadeToClear(null, 0.02f);
			if (Game1.currentLocation.currentEvent != null)
			{
				Event currentEvent = Game1.currentLocation.currentEvent;
				int currentCommand = currentEvent.CurrentCommand;
				currentEvent.CurrentCommand = currentCommand + 1;
				Game1.currentLocation.temporarySprites.Clear();
			}
		}

		// Token: 0x06002525 RID: 9509 RVA: 0x0019ECD6 File Offset: 0x0019CED6
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x06002526 RID: 9510 RVA: 0x0019ECD9 File Offset: 0x0019CED9
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002527 RID: 9511 RVA: 0x0019ECDB File Offset: 0x0019CEDB
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002528 RID: 9512 RVA: 0x0019ECDD File Offset: 0x0019CEDD
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002529 RID: 9513 RVA: 0x0019ECDF File Offset: 0x0019CEDF
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x0600252A RID: 9514 RVA: 0x0019ECE1 File Offset: 0x0019CEE1
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x0600252B RID: 9515 RVA: 0x0019ECE3 File Offset: 0x0019CEE3
		public void receiveKeyPress(Keys k)
		{
			if (k == Keys.Escape)
			{
				this.forceQuit();
			}
		}

		// Token: 0x0600252C RID: 9516 RVA: 0x0019ECF1 File Offset: 0x0019CEF1
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x0600252D RID: 9517 RVA: 0x0019ECF4 File Offset: 0x0019CEF4
		public Color getWaterColorForSeason()
		{
			switch (Game1.season)
			{
			case Season.Summer:
				return new Color(51, 90, 174);
			case Season.Fall:
				return new Color(56, 70, 128);
			case Season.Winter:
				return new Color(43, 74, 164);
			default:
				return new Color(49, 79, 155);
			}
		}

		// Token: 0x0600252E RID: 9518 RVA: 0x0019ED58 File Offset: 0x0019CF58
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), null, this.getWaterColorForSeason(), 0f, Vector2.Zero, SpriteEffects.None, 0f);
			b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(-Game1.viewport.Width, 400, Game1.viewport.Width * 3, Game1.viewport.Height)), null, new Color(49, 79, 155), 0f, Vector2.Zero, SpriteEffects.None, 5E-06f);
			b.Draw(this.texture, this.TransformDraw(this.mapSourceRectangle), new Rectangle?(this.mapSourceRectangle), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
			b.Draw(this.texture, this.TransformDraw(new Rectangle(-640, 331, 640, 294)), new Rectangle?(new Rectangle(0, 337, 640, 294)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
			b.Draw(this.texture, this.TransformDraw(new Rectangle(640, 343, 640, 294)), new Rectangle?(new Rectangle(0, 337, 640, 294)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
			for (int i = 0; i < this.entities.Count; i++)
			{
				if (!this.entities[i].drawOnTop)
				{
					this.entities[i].Draw(b);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			for (int j = 0; j < this.entities.Count; j++)
			{
				if (this.entities[j].drawOnTop)
				{
					this.entities[j].Draw(b);
				}
			}
			b.End();
		}

		// Token: 0x0600252F RID: 9519 RVA: 0x0019EFB8 File Offset: 0x0019D1B8
		public void changeScreenSize()
		{
			this._zoomLevel = 4f;
			if ((float)this.mapSourceRectangle.Height * this._zoomLevel < (float)Game1.viewport.Height)
			{
				this._zoomLevel = (float)Game1.viewport.Height / (float)this.mapSourceRectangle.Height;
			}
		}

		// Token: 0x06002530 RID: 9520 RVA: 0x0019F00E File Offset: 0x0019D20E
		public void unload()
		{
			Game1.stopMusicTrack(MusicContext.MiniGame);
		}

		// Token: 0x06002531 RID: 9521 RVA: 0x0019F016 File Offset: 0x0019D216
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002532 RID: 9522 RVA: 0x0019F01D File Offset: 0x0019D21D
		public string minigameId()
		{
			return null;
		}

		// Token: 0x06002533 RID: 9523 RVA: 0x0019F020 File Offset: 0x0019D220
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x06002534 RID: 9524 RVA: 0x0019F023 File Offset: 0x0019D223
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x04001679 RID: 5753
		public float _age;

		// Token: 0x0400167A RID: 5754
		public Texture2D texture;

		// Token: 0x0400167B RID: 5755
		public Rectangle mapSourceRectangle;

		// Token: 0x0400167C RID: 5756
		protected float _zoomLevel = 1f;

		// Token: 0x0400167D RID: 5757
		protected Vector2 viewTarget = new Vector2(0f, 0f);

		// Token: 0x0400167E RID: 5758
		protected Vector2 _upperLeft;

		// Token: 0x0400167F RID: 5759
		public List<BoatJourney.Entity> entities;

		// Token: 0x04001680 RID: 5760
		protected float _currentBoatSpeed;

		// Token: 0x04001681 RID: 5761
		public float boatSpeed = 0.5f;

		// Token: 0x04001682 RID: 5762
		public float dockSpeed = 0.1f;

		// Token: 0x04001683 RID: 5763
		protected float _nextSlosh;

		// Token: 0x04001684 RID: 5764
		protected bool _fadeComplete;

		// Token: 0x04001685 RID: 5765
		public Vector2[] points = new Vector2[]
		{
			new Vector2(286f, 53f),
			new Vector2(286f, 60f),
			new Vector2(287f, 88f),
			new Vector2(340f, 121f),
			new Vector2(357f, 215f),
			new Vector2(204f, 633f),
			new Vector2(274f, 750f),
			new Vector2(352f, 720f),
			new Vector2(352f, 700f)
		};

		// Token: 0x04001686 RID: 5766
		protected List<Vector2> _interpolatedPoints;

		// Token: 0x04001687 RID: 5767
		protected List<float> _cumulativeDistances;

		// Token: 0x04001688 RID: 5768
		protected float _totalPathDistance;

		// Token: 0x04001689 RID: 5769
		protected float traveledBoatDistance;

		// Token: 0x0400168A RID: 5770
		protected float nextSmoke;

		// Token: 0x0400168B RID: 5771
		public float departureDelay = 1.5f;

		// Token: 0x0400168C RID: 5772
		protected BoatJourney.Boat _boat;

		// Token: 0x0400168D RID: 5773
		protected List<BoatJourney.Entity> _seagulls = new List<BoatJourney.Entity>();

		// Token: 0x02000595 RID: 1429
		public class WaterSparkle : BoatJourney.Entity
		{
			// Token: 0x060041E7 RID: 16871 RVA: 0x0030C898 File Offset: 0x0030AA98
			public WaterSparkle(BoatJourney context) : base(context, BoatJourney.GetAssetName(), new Rectangle(647, 524, 1, 1), new Vector2(0f, 0f), new Vector2(0f, 0f))
			{
				this.currentFrame = Game1.random.Next(0, 7);
				this.numFrames = 7;
				this.frameInterval = 0.1f;
				this._startPosition = this.position;
				this.RandomizePosition();
			}

			// Token: 0x060041E8 RID: 16872 RVA: 0x0030C918 File Offset: 0x0030AB18
			public void RandomizePosition()
			{
				Rectangle open_water = new Rectangle(0, 112, 640, 528);
				do
				{
					this._startPosition = (this.position = Utility.getRandomPositionInThisRectangle(open_water, Game1.random));
				}
				while (new Rectangle(508, 11, 125, 138).Contains((int)this._startPosition.X, (int)this._startPosition.Y));
				this.velocity.X = Utility.RandomFloat(-0.1f, 0.1f, null);
			}

			// Token: 0x060041E9 RID: 16873 RVA: 0x0030C9A3 File Offset: 0x0030ABA3
			public override void OnAnimationFinished()
			{
				this.RandomizePosition();
				base.OnAnimationFinished();
			}

			// Token: 0x060041EA RID: 16874 RVA: 0x0030C9B1 File Offset: 0x0030ABB1
			public override float GetLayerDepth()
			{
				if (this.layerDepth >= 0f)
				{
					return this.layerDepth;
				}
				return 0.0001f;
			}

			// Token: 0x04002C2F RID: 11311
			protected Vector2 _startPosition;
		}

		// Token: 0x02000596 RID: 1430
		public class Wave : BoatJourney.Entity
		{
			// Token: 0x060041EB RID: 16875 RVA: 0x0030C9CC File Offset: 0x0030ABCC
			public Wave(BoatJourney context, Vector2 position = default(Vector2)) : base(context, BoatJourney.GetAssetName(), new Rectangle(640, 506, 32, 12), new Vector2(16f, 6f), position)
			{
				this.numFrames = 2;
				this.frameInterval = 1.25f;
				this._startPosition = position;
			}

			// Token: 0x060041EC RID: 16876 RVA: 0x0030CA24 File Offset: 0x0030AC24
			public override bool Update(GameTime time)
			{
				this.position = this._startPosition + new Vector2(1f, 0f) * (float)Math.Sin((double)(this._startPosition.X * 0.333f + this._startPosition.Y * 0.1f + this._age)) * 3f;
				return base.Update(time);
			}

			// Token: 0x060041ED RID: 16877 RVA: 0x0030CA98 File Offset: 0x0030AC98
			public override float GetLayerDepth()
			{
				if (this.layerDepth >= 0f)
				{
					return this.layerDepth;
				}
				return 0.0003f;
			}

			// Token: 0x04002C30 RID: 11312
			protected Vector2 _startPosition;
		}

		// Token: 0x02000597 RID: 1431
		public class Boat : BoatJourney.Entity
		{
			// Token: 0x060041EE RID: 16878 RVA: 0x0030CAB3 File Offset: 0x0030ACB3
			public Boat(BoatJourney context, string texture_path, Rectangle source_rect, Vector2 origin = default(Vector2), Vector2 position = default(Vector2)) : base(context, texture_path, source_rect, origin, position)
			{
			}

			// Token: 0x060041EF RID: 16879 RVA: 0x0030CAD8 File Offset: 0x0030ACD8
			public override bool Update(GameTime time)
			{
				bool moved = false;
				if (this._lastPosition != this.position)
				{
					this._lastPosition = this.position;
					moved = true;
				}
				if (moved)
				{
					this.frameInterval = this.moveAnimationInterval;
				}
				else
				{
					this.frameInterval = this.idleAnimationInterval;
				}
				if (this.smokeStack != null)
				{
					if (this.nextSmokeStackSmoke <= 0f)
					{
						this.nextSmokeStackSmoke = 0.25f;
						if (moved)
						{
							BoatJourney.Entity smoke_entity = new BoatJourney.Entity(this._context, BoatJourney.GetAssetName(), new Rectangle(689, 337, 2, 2), new Vector2(1f, 1f), this.position + this.smokeStack.Value);
							smoke_entity.numFrames = 3;
							Vector2 velocity = new Vector2(Utility.RandomFloat(-0.04f, -0.03f, null), Utility.RandomFloat(-0.05f, -0.1f, null));
							smoke_entity.velocity = velocity;
							smoke_entity.destroyAfterAnimation = true;
							this._context.entities.Add(smoke_entity);
						}
					}
					else
					{
						this.nextSmokeStackSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
					}
				}
				if (this.nextRipple <= 0f)
				{
					this.nextRipple = 0.25f;
					if (moved)
					{
						BoatJourney.Entity ripple_entity = new BoatJourney.Entity(this._context, BoatJourney.GetAssetName(), new Rectangle(640, 336, 9, 16), new Vector2(4f, 0f), this.position + new Vector2(0f, 0f));
						ripple_entity.numFrames = 5;
						ripple_entity.layerDepth = 2E-05f;
						ripple_entity.destroyAfterAnimation = true;
						this._context.entities.Add(ripple_entity);
					}
				}
				else
				{
					this.nextRipple -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				return base.Update(time);
			}

			// Token: 0x04002C31 RID: 11313
			protected float nextSmokeStackSmoke;

			// Token: 0x04002C32 RID: 11314
			protected float nextRipple;

			// Token: 0x04002C33 RID: 11315
			public Vector2? smokeStack;

			// Token: 0x04002C34 RID: 11316
			public Vector2 _lastPosition;

			// Token: 0x04002C35 RID: 11317
			public float idleAnimationInterval = 0.75f;

			// Token: 0x04002C36 RID: 11318
			public float moveAnimationInterval = 0.25f;
		}

		// Token: 0x02000598 RID: 1432
		public class Entity
		{
			// Token: 0x060041F0 RID: 16880 RVA: 0x0030CCCC File Offset: 0x0030AECC
			public Entity(BoatJourney context, string texture_path, Rectangle source_rect, Vector2 origin = default(Vector2), Vector2 position = default(Vector2))
			{
				this._context = context;
				this._texture = Game1.temporaryContent.Load<Texture2D>(texture_path);
				this._sourceRect = source_rect;
				this.origin = origin;
				this.position = position;
			}

			// Token: 0x060041F1 RID: 16881 RVA: 0x0030CD2C File Offset: 0x0030AF2C
			public virtual bool Update(GameTime time)
			{
				this._age += (float)time.ElapsedGameTime.TotalSeconds;
				this._frameTime += (float)time.ElapsedGameTime.TotalSeconds;
				if (this.lifeTime > 0f && this.lifeTime >= this._age)
				{
					return true;
				}
				if (this.frameInterval > 0f && this._frameTime > this.frameInterval)
				{
					this._frameTime -= this.frameInterval;
					this.currentFrame++;
					if (this.currentFrame >= this.numFrames)
					{
						this.OnAnimationFinished();
						this.currentFrame -= this.numFrames;
						if (this.destroyAfterAnimation)
						{
							return true;
						}
					}
				}
				this.position += this.velocity;
				return false;
			}

			// Token: 0x060041F2 RID: 16882 RVA: 0x0030CE13 File Offset: 0x0030B013
			public virtual void OnAnimationFinished()
			{
			}

			// Token: 0x060041F3 RID: 16883 RVA: 0x0030CE15 File Offset: 0x0030B015
			public virtual void SetSourceRect(Rectangle rectangle)
			{
				this._sourceRect = rectangle;
			}

			// Token: 0x060041F4 RID: 16884 RVA: 0x0030CE20 File Offset: 0x0030B020
			public virtual Rectangle GetSourceRect()
			{
				int x = this.currentFrame;
				int y = 0;
				if (this.columns > 0)
				{
					y = x / this.columns;
					x %= this.columns;
				}
				return new Rectangle(this._sourceRect.X + x * this._sourceRect.Width, this._sourceRect.Y + y * this._sourceRect.Width, this._sourceRect.Width, this._sourceRect.Height);
			}

			// Token: 0x060041F5 RID: 16885 RVA: 0x0030CE9E File Offset: 0x0030B09E
			public virtual float GetLayerDepth()
			{
				if (this.layerDepth >= 0f)
				{
					return this.layerDepth;
				}
				return this.position.Y / 100000f;
			}

			// Token: 0x060041F6 RID: 16886 RVA: 0x0030CEC8 File Offset: 0x0030B0C8
			public virtual void Draw(SpriteBatch b)
			{
				b.Draw(this._texture, this._context.TransformDraw(this.position), new Rectangle?(this.GetSourceRect()), Color.White, 0f, this.origin, this._context._zoomLevel, this.flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.GetLayerDepth());
			}

			// Token: 0x04002C37 RID: 11319
			protected BoatJourney _context;

			// Token: 0x04002C38 RID: 11320
			public Vector2 position;

			// Token: 0x04002C39 RID: 11321
			protected Texture2D _texture;

			// Token: 0x04002C3A RID: 11322
			protected Rectangle _sourceRect;

			// Token: 0x04002C3B RID: 11323
			protected float lifeTime;

			// Token: 0x04002C3C RID: 11324
			protected float _age;

			// Token: 0x04002C3D RID: 11325
			public Vector2 velocity;

			// Token: 0x04002C3E RID: 11326
			public Vector2 origin;

			// Token: 0x04002C3F RID: 11327
			public bool flipX;

			// Token: 0x04002C40 RID: 11328
			protected float _frameTime;

			// Token: 0x04002C41 RID: 11329
			public float frameInterval = 0.25f;

			// Token: 0x04002C42 RID: 11330
			public int currentFrame;

			// Token: 0x04002C43 RID: 11331
			public int numFrames = 1;

			// Token: 0x04002C44 RID: 11332
			public int columns;

			// Token: 0x04002C45 RID: 11333
			public bool destroyAfterAnimation;

			// Token: 0x04002C46 RID: 11334
			public bool drawOnTop;

			// Token: 0x04002C47 RID: 11335
			public float layerDepth = -1f;
		}
	}
}

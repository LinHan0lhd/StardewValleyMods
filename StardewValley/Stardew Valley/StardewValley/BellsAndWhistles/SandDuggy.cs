using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A4 RID: 932
	public class SandDuggy : INetObject<NetFields>
	{
		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x060038CA RID: 14538 RVA: 0x002CFCB0 File Offset: 0x002CDEB0
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("SandDuggy");

		// Token: 0x060038CB RID: 14539 RVA: 0x002CFCB8 File Offset: 0x002CDEB8
		public SandDuggy()
		{
			this.InitNetFields();
		}

		// Token: 0x060038CC RID: 14540 RVA: 0x002CFD10 File Offset: 0x002CDF10
		public SandDuggy(GameLocation location, Point[] points) : this()
		{
			this.locationRef.Value = location;
			foreach (Point point in points)
			{
				this.holeLocations.Add(point);
			}
			this.currentHoleIndex.Value = this.FindRandomFreePoint();
		}

		// Token: 0x060038CD RID: 14541 RVA: 0x002CFD64 File Offset: 0x002CDF64
		public virtual int FindRandomFreePoint()
		{
			if (this.locationRef.Value == null)
			{
				return -1;
			}
			List<int> validHoleLocations = new List<int>();
			for (int i = 0; i < this.holeLocations.Count; i++)
			{
				Point holeTile = this.holeLocations[i];
				if (!this.locationRef.Value.isObjectAtTile(holeTile.X, holeTile.Y) && !this.locationRef.Value.isTerrainFeatureAt(holeTile.X, holeTile.Y) && !this.locationRef.Value.terrainFeatures.ContainsKey(Utility.PointToVector2(holeTile)))
				{
					validHoleLocations.Add(i);
				}
			}
			if (validHoleLocations.Count == 1)
			{
				return validHoleLocations[0];
			}
			validHoleLocations.RemoveAll(delegate(int index)
			{
				Point holeTile2 = this.holeLocations[index];
				foreach (Farmer farmer in this.locationRef.Value.farmers)
				{
					if (this.NearFarmer(holeTile2, farmer))
					{
						return true;
					}
				}
				return false;
			});
			if (validHoleLocations.Count > 0)
			{
				return Game1.random.ChooseFrom(validHoleLocations);
			}
			return -1;
		}

		// Token: 0x060038CE RID: 14542 RVA: 0x002CFE44 File Offset: 0x002CE044
		public virtual void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.holeLocations, "holeLocations").AddField(this.currentHoleIndex, "currentHoleIndex").AddField(this.locationRef.NetFields, "locationRef.NetFields").AddField(this.whacked, "whacked");
			this.whacked.fieldChangeVisibleEvent += this.OnWhackedChanged;
		}

		// Token: 0x060038CF RID: 14543 RVA: 0x002CFEBC File Offset: 0x002CE0BC
		public virtual void OnWhackedChanged(NetBool field, bool old_value, bool new_value)
		{
			if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback())
			{
				return;
			}
			if (this.whacked.Value)
			{
				if (Game1.IsMasterGame)
				{
					int index = this.currentHoleIndex.Value;
					if (index == -1)
					{
						index = 0;
					}
					Game1.player.team.MarkCollectedNut("SandDuggy");
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2((float)this.holeLocations[index].X, (float)this.holeLocations[index].Y) * 64f, -1, this.locationRef.Value, -1, false);
				}
				if (Game1.currentLocation == this.locationRef.Value)
				{
					this.AnimateWhacked();
				}
			}
		}

		// Token: 0x060038D0 RID: 14544 RVA: 0x002CFF84 File Offset: 0x002CE184
		public virtual void AnimateWhacked()
		{
			if (Game1.currentLocation != this.locationRef.Value)
			{
				return;
			}
			int index = this.currentHoleIndex.Value;
			if (index == -1)
			{
				index = 0;
			}
			Vector2 position = new Vector2((float)this.holeLocations[index].X, (float)this.holeLocations[index].Y);
			int ground_position = (int)(position.Y * 64f - 32f);
			if (Utility.isOnScreen((position + new Vector2(0.5f, 0.5f)) * 64f, 64))
			{
				Game1.playSound("axchop", null);
				Game1.playSound("rockGolemHit", null);
			}
			TemporaryAnimatedSprite duggy_sprite = new TemporaryAnimatedSprite("LooseSprites/SandDuggy", new Rectangle(0, 48, 16, 48), new Vector2(position.X * 64f, position.Y * 64f - 32f), false, 0f, Color.White)
			{
				motion = new Vector2(2f, -3f),
				acceleration = new Vector2(0f, 0.25f),
				interval = 1000f,
				animationLength = 1,
				alphaFade = 0.02f,
				layerDepth = 0.07682f,
				scale = 4f,
				yStopCoordinate = ground_position
			};
			duggy_sprite.reachedStopCoordinate = delegate(int extra_info)
			{
				duggy_sprite.motion.Y = -3f;
				duggy_sprite.acceleration.Y = 0.25f;
				duggy_sprite.yStopCoordinate = ground_position;
				duggy_sprite.flipped = !duggy_sprite.flipped;
			};
			Game1.currentLocation.temporarySprites.Add(duggy_sprite);
		}

		// Token: 0x060038D1 RID: 14545 RVA: 0x002D012F File Offset: 0x002CE32F
		public virtual void ResetForPlayerEntry()
		{
			this.texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SandDuggy");
		}

		// Token: 0x060038D2 RID: 14546 RVA: 0x002D0148 File Offset: 0x002CE348
		public virtual void PerformToolAction(Tool tool, int tile_x, int tile_y)
		{
			if (this.currentState == SandDuggy.State.Idle && this._localIndex >= 0)
			{
				Point point = this.holeLocations[this._localIndex];
				if (point.X == tile_x && point.Y == tile_y)
				{
					this.whacked.Value = true;
				}
			}
		}

		// Token: 0x060038D3 RID: 14547 RVA: 0x002D0197 File Offset: 0x002CE397
		public virtual bool NearFarmer(Point location, Farmer farmer)
		{
			return Math.Abs(location.X - farmer.TilePoint.X) <= 2 && Math.Abs(location.Y - farmer.TilePoint.Y) <= 2;
		}

		// Token: 0x060038D4 RID: 14548 RVA: 0x002D01D0 File Offset: 0x002CE3D0
		public virtual void Update(GameTime time)
		{
			if (this.whacked.Value)
			{
				return;
			}
			if (this.currentHoleIndex.Value >= 0)
			{
				Point synched_position = this.holeLocations[this.currentHoleIndex.Value];
				if (this.NearFarmer(synched_position, Game1.player) && this.FindRandomFreePoint() != this.currentHoleIndex.Value)
				{
					this.currentHoleIndex.Value = -1;
					DelayedAction.playSoundAfterDelay((Game1.random.NextDouble() < 0.1) ? "cowboy_gopher" : "tinyWhip", 200, null, null, -1, false);
				}
			}
			this.nextFrameUpdate -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.currentHoleIndex.Value < 0 && Game1.IsMasterGame)
			{
				this.currentHoleIndex.Value = this.FindRandomFreePoint();
			}
			if (this.currentState == SandDuggy.State.DigDown && this.frame == 0)
			{
				if (this.currentHoleIndex.Value >= 0)
				{
					this.currentState = SandDuggy.State.DigUp;
				}
				this._localIndex = this.currentHoleIndex.Value;
			}
			if (this.currentHoleIndex.Value == -1 || this.currentHoleIndex.Value != this._localIndex)
			{
				this.currentState = SandDuggy.State.DigDown;
			}
			if (this.nextFrameUpdate <= 0f)
			{
				if (this._localIndex >= 0)
				{
					switch (this.currentState)
					{
					case SandDuggy.State.DigUp:
						if (this._localIndex >= 0)
						{
							this.frame++;
							if (this.frame >= 4)
							{
								this.currentState = SandDuggy.State.Idle;
							}
						}
						break;
					case SandDuggy.State.Idle:
						this.frame++;
						if (this.frame > 7)
						{
							this.frame = 4;
						}
						break;
					case SandDuggy.State.DigDown:
						this.frame--;
						if (this.frame <= 0)
						{
							this.frame = 0;
						}
						break;
					}
				}
				this.nextFrameUpdate = 0.075f;
			}
		}

		// Token: 0x060038D5 RID: 14549 RVA: 0x002D03C0 File Offset: 0x002CE5C0
		public virtual void Draw(SpriteBatch b)
		{
			if (this.whacked.Value)
			{
				return;
			}
			if (this._localIndex >= 0)
			{
				Point point = this.holeLocations[this._localIndex];
				Vector2 draw_position = (new Vector2((float)point.X, (float)point.Y) + new Vector2(0.5f, 0.5f)) * 64f;
				b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, draw_position), new Rectangle?(new Rectangle(this.frame % 4 * 16, this.frame / 4 * 24, 16, 24)), Color.White, 0f, new Vector2(8f, 20f), 4f, SpriteEffects.None, draw_position.Y / 10000f);
			}
		}

		// Token: 0x0400254A RID: 9546
		[XmlIgnore]
		public NetList<Point, NetPoint> holeLocations = new NetList<Point, NetPoint>();

		// Token: 0x0400254B RID: 9547
		[XmlIgnore]
		public int frame;

		// Token: 0x0400254C RID: 9548
		[XmlIgnore]
		public NetInt currentHoleIndex = new NetInt(0);

		// Token: 0x0400254D RID: 9549
		[XmlIgnore]
		public int _localIndex;

		// Token: 0x0400254F RID: 9551
		[XmlIgnore]
		public NetLocationRef locationRef = new NetLocationRef();

		// Token: 0x04002550 RID: 9552
		[XmlIgnore]
		public SandDuggy.State currentState;

		// Token: 0x04002551 RID: 9553
		[XmlIgnore]
		public Texture2D texture;

		// Token: 0x04002552 RID: 9554
		[XmlIgnore]
		public float nextFrameUpdate;

		// Token: 0x04002553 RID: 9555
		[XmlElement("whacked")]
		public NetBool whacked = new NetBool(false);

		// Token: 0x020006BF RID: 1727
		public enum State
		{
			// Token: 0x040030A9 RID: 12457
			DigUp,
			// Token: 0x040030AA RID: 12458
			Idle,
			// Token: 0x040030AB RID: 12459
			DigDown
		}
	}
}

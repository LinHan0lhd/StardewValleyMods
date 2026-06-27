using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Locations
{
	// Token: 0x020002CE RID: 718
	public class Racer : INetObject<NetFields>
	{
		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x06002EF8 RID: 12024 RVA: 0x0024CD2A File Offset: 0x0024AF2A
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("DesertFestival.Racer");

		// Token: 0x06002EF9 RID: 12025 RVA: 0x0024CD34 File Offset: 0x0024AF34
		public Racer()
		{
			this.InitNetFields();
			this.direction.Value = 3;
			this._texture = Game1.content.Load<Texture2D>("LooseSprites\\DesertRacers");
		}

		// Token: 0x06002EFA RID: 12026 RVA: 0x0024CE36 File Offset: 0x0024B036
		public Racer(int index) : this()
		{
			this.racerIndex.Value = index;
			this.ResetMoveSpeed();
		}

		// Token: 0x06002EFB RID: 12027 RVA: 0x0024CE50 File Offset: 0x0024B050
		public virtual void ResetMoveSpeed()
		{
			this.minMoveSpeed = 1.5f;
			this.maxMoveSpeed = 4f;
			this.extraLuck = Utility.RandomFloat(-0.25f, 0.25f, null);
			if (this.racerIndex.Value == 3)
			{
				this.minMoveSpeed = 0.5f;
				this.maxMoveSpeed = 3.5f;
			}
			this.SpeedBurst();
		}

		// Token: 0x06002EFC RID: 12028 RVA: 0x0024CEB4 File Offset: 0x0024B0B4
		private void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.racerIndex, "racerIndex").AddField(this.position.NetFields, "position.NetFields").AddField(this.direction, "direction").AddField(this.jumpSegmentStart, "jumpSegmentStart").AddField(this.jumpSegmentEnd, "jumpSegmentEnd").AddField(this.jumping, "jumping").AddField(this.drawAboveMap, "drawAboveMap").AddField(this.tripping, "tripping").AddField(this.sabotages, "sabotages").AddField(this.moving, "moving");
			this.jumpSegmentStart.Interpolated(false, false);
			this.jumpSegmentEnd.Interpolated(false, false);
		}

		// Token: 0x06002EFD RID: 12029 RVA: 0x0024CF90 File Offset: 0x0024B190
		public virtual void UpdateRaceProgress(DesertFestival location)
		{
			if (this.currentTrackIndex < 0)
			{
				this.progress = (float)location.raceTrack.Length;
				return;
			}
			Vector2 segment = this.segmentEnd - this.segmentStart;
			float segment_length = segment.Length();
			segment.Normalize();
			Vector2 current_offset = this.position.Value - this.segmentStart;
			float position_in_segment = Vector2.Dot(segment, current_offset);
			if (segment_length > 0f)
			{
				segment_length = position_in_segment / segment_length;
			}
			this.progress = (float)this.currentTrackIndex + segment_length;
		}

		// Token: 0x06002EFE RID: 12030 RVA: 0x0024D014 File Offset: 0x0024B214
		public virtual void Update(DesertFestival location)
		{
			if (Game1.IsMasterGame)
			{
				bool has_moved = false;
				if (location.currentRaceState.Value == DesertFestival.RaceState.StartingLine && this.currentTrackIndex < 0)
				{
					if (this.horizontalPosition < 0f)
					{
						int index = location.netRacers.IndexOf(this);
						this.horizontalPosition = (float)index / (float)(location.racerCount - 1);
					}
					this.currentTrackIndex = 0;
					Vector3 track_position = location.GetTrackPosition(this.currentTrackIndex, this.horizontalPosition);
					this.segmentStart = this.position.Value;
					this.segmentEnd = new Vector2(track_position.X, track_position.Y);
				}
				float frame_travel = this.maxMoveSpeed;
				if (location.currentRaceState.Value == DesertFestival.RaceState.Go)
				{
					if (location.finishedRacers.Count <= 0)
					{
						if (this.burstDuration > 0f)
						{
							this.moveSpeed = this.maxMoveSpeed;
							this.burstDuration -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
							if (this.burstDuration <= 0f)
							{
								this.burstDuration = 0f;
								this.nextBurst = Utility.RandomFloat(0.75f, 1.5f, null);
								if (Game1.random.NextDouble() + (double)this.extraLuck < 0.25)
								{
									this.nextBurst *= 0.5f;
								}
								if (this.racerIndex.Value == 3)
								{
									this.nextBurst *= 0.25f;
								}
								float last_place = (float)location.raceTrack.Length;
								foreach (Racer racer in location.netRacers)
								{
									last_place = Math.Min(last_place, racer.progress);
								}
								if (this.progress > last_place && Game1.random.NextDouble() < (double)Math.Min(0.05f + (float)this.sabotages.Value * 0.2f, 0.5f))
								{
									this.tripping.Value = true;
									this.tripTimer = Utility.RandomFloat(1.5f, 2f, null);
								}
							}
						}
						else if (this.nextBurst > 0f)
						{
							this.moveSpeed = Utility.MoveTowards(this.moveSpeed, this.minMoveSpeed, 0.5f);
							this.nextBurst -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
							if (this.nextBurst <= 0f)
							{
								this.SpeedBurst();
								this.nextBurst = 0f;
							}
						}
						frame_travel = this.moveSpeed;
					}
					if (this.tripTimer > 0f)
					{
						this.tripTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
						if (this.tripTimer < 0f)
						{
							this.tripTimer = 0f;
							this.tripping.Value = false;
						}
					}
				}
				if (this.jumping.Value)
				{
					if ((this.segmentEnd - this.segmentStart).Length() / 64f > 3f)
					{
						frame_travel = 6f;
					}
					else
					{
						frame_travel = 3f;
					}
				}
				else if (this.tripping.Value)
				{
					frame_travel = 0.25f;
				}
				if (this.segmentStart == this.segmentEnd && this.position.Value == this.segmentEnd && this.currentTrackIndex < 0)
				{
					frame_travel = 0f;
				}
				while (frame_travel > 0f)
				{
					float moved_amount = Math.Min((this.segmentEnd - this.position.Value).Length(), frame_travel);
					frame_travel -= moved_amount;
					Vector2 delta = this.segmentEnd - this.position.Value;
					if (delta.X != 0f || delta.Y != 0f)
					{
						delta.Normalize();
						this.position.Value += delta * moved_amount;
						has_moved = true;
						if (Math.Abs(delta.Y) > Math.Abs(delta.X))
						{
							if (delta.Y < 0f)
							{
								this.direction.Value = 0;
							}
							else
							{
								this.direction.Value = 2;
							}
						}
						else if (delta.X < 0f)
						{
							this.direction.Value = 3;
						}
						else
						{
							this.direction.Value = 1;
						}
					}
					if ((this.position.Value - this.segmentEnd).Length() < 0.01f)
					{
						this.position.Value = this.segmentEnd;
						if (location.currentRaceState.Value == DesertFestival.RaceState.Go && this.currentTrackIndex >= 0)
						{
							Vector3 track_position2 = location.GetTrackPosition(this.currentTrackIndex, this.horizontalPosition);
							if (track_position2.Z > 0f)
							{
								this.tripping.Value = false;
								this.tripTimer = 0f;
								this.jumping.Value = true;
							}
							else
							{
								this.jumping.Value = false;
							}
							float z = track_position2.Z;
							if (z != 2f)
							{
								if (z == 3f)
								{
									this.drawAboveMap.Value = false;
								}
							}
							else
							{
								this.drawAboveMap.Value = true;
							}
							this.currentTrackIndex++;
							if (this.currentTrackIndex >= location.raceTrack.Length)
							{
								this.currentTrackIndex = -2;
								this.segmentStart = this.segmentEnd;
								this.segmentEnd = new Vector2(44.5f, 37.5f - (float)location.finishedRacers.Count) * 64f;
								this.horizontalPosition = (float)(location.racerCount - 1 - location.finishedRacers.Count) / (float)(location.racerCount - 1);
								location.finishedRacers.Add(this.racerIndex.Value);
								if (location.finishedRacers.Count == 1)
								{
									location.announceRaceEvent.Fire("Race_Finish");
									location.OnRaceWon(this.racerIndex.Value);
								}
							}
							else
							{
								track_position2 = location.GetTrackPosition(this.currentTrackIndex, this.horizontalPosition);
								this.segmentStart = this.segmentEnd;
								this.segmentEnd = new Vector2(track_position2.X, track_position2.Y);
							}
							if (this.jumping.Value)
							{
								this.jumpSegmentStart.Value = this.segmentStart;
								this.jumpSegmentEnd.Value = this.segmentEnd;
							}
						}
						else
						{
							frame_travel = 0f;
							this.segmentStart = this.segmentEnd;
							if (location.currentRaceState.Value >= DesertFestival.RaceState.StartingLine && location.currentRaceState.Value < DesertFestival.RaceState.Go)
							{
								this.direction.Value = 0;
							}
							else
							{
								this.direction.Value = 3;
							}
						}
					}
				}
				this.moving.Value = has_moved;
			}
			if (this.lastPosition == null)
			{
				this.lastPosition = new Vector2?(this.position.Value);
			}
			float distance_traveled = (this.lastPosition.Value - this.position.Value).Length();
			this.nextFrameSwap -= distance_traveled;
			while (this.nextFrameSwap <= 0f)
			{
				this.frame = !this.frame;
				this.nextFrameSwap += 8f;
			}
			this.lastPosition = new Vector2?(this.position.Value);
			if (!this.jumping.Value)
			{
				if (this.moving.Value)
				{
					if (this.tripping.Value && this.height == 0f)
					{
						if (this._tripLeaps == 0)
						{
							this.gravity = 1f;
						}
						else
						{
							this.gravity = Utility.RandomFloat(0.5f, 0.75f, null);
						}
						this._tripLeaps++;
					}
					else if (this.racerIndex.Value == 2 && this.height == 0f)
					{
						this.gravity = Utility.RandomFloat(0.25f, 0.5f, null);
					}
				}
				if (this.height != 0f || this.gravity != 0f)
				{
					this.height += this.gravity;
					this.gravity -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds * 2f;
					if (this.gravity == 0f)
					{
						this.gravity = -0.0001f;
					}
					if (this.height <= 0f)
					{
						this.gravity = 0f;
						this.height = 0f;
					}
				}
			}
			if (!this.tripping.Value)
			{
				this._tripLeaps = 0;
			}
			if (this.jumping.Value)
			{
				Vector2 segment = this.jumpSegmentEnd.Value - this.jumpSegmentStart.Value;
				float segment_length = segment.Length();
				segment.Normalize();
				Vector2 current_offset = this.position.Value - this.jumpSegmentStart.Value;
				float position_in_segment = Vector2.Dot(segment, current_offset);
				if (segment_length > 0f)
				{
					this.height = (float)Math.Sin((double)Utility.Clamp(position_in_segment / segment_length, 0f, 1f) * 3.141592653589793) * 48f;
					return;
				}
			}
			else if (this.gravity == 0f)
			{
				this.height = 0f;
			}
		}

		// Token: 0x06002EFF RID: 12031 RVA: 0x0024D9C4 File Offset: 0x0024BBC4
		public virtual void SpeedBurst()
		{
			this.burstDuration = Utility.RandomFloat(0.25f, 1f, null);
			if (Game1.random.NextDouble() + (double)this.extraLuck < 0.25)
			{
				this.burstDuration *= 2f;
			}
			if (this.racerIndex.Value == 3)
			{
				this.burstDuration *= 0.25f;
			}
			this.moveSpeed = this.maxMoveSpeed;
		}

		// Token: 0x06002F00 RID: 12032 RVA: 0x0024DA44 File Offset: 0x0024BC44
		public virtual void Draw(SpriteBatch sb)
		{
			float sort_y = (this.position.Y + (float)this.racerIndex.Value * 0.1f) / 10000f;
			float height_fade = Utility.Clamp(1f - this.height / 12f, 0f, 1f);
			sb.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position.Value), null, Color.White * 0.75f * height_fade, 0f, new Vector2((float)(Game1.shadowTexture.Width / 2), (float)(Game1.shadowTexture.Height / 2)), new Vector2(3f, 3f), SpriteEffects.None, sort_y / 10000f - 1E-07f);
			SpriteEffects effect = SpriteEffects.None;
			Rectangle source_rect = new Rectangle(0, 0, 16, 16);
			source_rect.Y = this.racerIndex.Value * 16;
			switch (this.direction.Value)
			{
			case 0:
				source_rect.X = 0;
				break;
			case 1:
				source_rect.X = 32;
				break;
			case 2:
				source_rect.X = 64;
				break;
			case 3:
				source_rect.X = 32;
				effect = SpriteEffects.FlipHorizontally;
				break;
			}
			if (this.frame)
			{
				source_rect.X += 16;
			}
			Vector2 offset = Vector2.Zero;
			if (this.tripping.Value)
			{
				source_rect.X = 96;
				offset.X += (float)Game1.random.Next(-1, 2) * 0.5f;
				offset.Y += (float)Game1.random.Next(-1, 2) * 0.5f;
			}
			sb.Draw(this._texture, Game1.GlobalToLocal(this.position.Value + new Vector2(offset.X, -this.height + offset.Y) * 4f), new Rectangle?(source_rect), Color.White, 0f, new Vector2(8f, 14f), 4f, effect, sort_y);
		}

		// Token: 0x04002003 RID: 8195
		public NetBool moving = new NetBool();

		// Token: 0x04002004 RID: 8196
		public Vector2? lastPosition;

		// Token: 0x04002005 RID: 8197
		public NetPosition position = new NetPosition();

		// Token: 0x04002006 RID: 8198
		public NetInt direction = new NetInt();

		// Token: 0x04002007 RID: 8199
		public float horizontalPosition = -1f;

		// Token: 0x04002008 RID: 8200
		public int currentTrackIndex = -1;

		// Token: 0x04002009 RID: 8201
		public Vector2 segmentStart = Vector2.Zero;

		// Token: 0x0400200A RID: 8202
		public Vector2 segmentEnd = Vector2.Zero;

		// Token: 0x0400200B RID: 8203
		public NetVector2 jumpSegmentStart = new NetVector2();

		// Token: 0x0400200C RID: 8204
		public NetVector2 jumpSegmentEnd = new NetVector2();

		// Token: 0x0400200D RID: 8205
		public NetBool jumping = new NetBool();

		// Token: 0x0400200E RID: 8206
		public NetBool tripping = new NetBool();

		// Token: 0x0400200F RID: 8207
		public NetBool drawAboveMap = new NetBool();

		// Token: 0x04002010 RID: 8208
		public float moveSpeed = 3f;

		// Token: 0x04002011 RID: 8209
		public float minMoveSpeed = 3f;

		// Token: 0x04002012 RID: 8210
		public float maxMoveSpeed = 6f;

		// Token: 0x04002013 RID: 8211
		public float height;

		// Token: 0x04002014 RID: 8212
		public float tripTimer;

		// Token: 0x04002015 RID: 8213
		public NetInt racerIndex = new NetInt();

		// Token: 0x04002016 RID: 8214
		protected Texture2D _texture;

		// Token: 0x04002017 RID: 8215
		public bool frame;

		// Token: 0x04002018 RID: 8216
		public float nextFrameSwap;

		// Token: 0x04002019 RID: 8217
		public float burstDuration;

		// Token: 0x0400201A RID: 8218
		public float nextBurst;

		// Token: 0x0400201B RID: 8219
		public float extraLuck;

		// Token: 0x0400201C RID: 8220
		public float gravity;

		// Token: 0x0400201D RID: 8221
		public int _tripLeaps;

		// Token: 0x0400201E RID: 8222
		public float progress;

		// Token: 0x0400201F RID: 8223
		public NetInt sabotages = new NetInt(0);
	}
}

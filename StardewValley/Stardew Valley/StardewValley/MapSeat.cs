using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x02000101 RID: 257
	public class MapSeat : INetObject<NetFields>, ISittable
	{
		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06001471 RID: 5233 RVA: 0x000F6A8A File Offset: 0x000F4C8A
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("MapSeat");

		// Token: 0x06001472 RID: 5234 RVA: 0x000F6A94 File Offset: 0x000F4C94
		public MapSeat()
		{
			this.NetFields.SetOwner(this).AddField(this.sittingFarmers, "sittingFarmers").AddField(this.tilePosition, "tilePosition").AddField(this.size, "size").AddField(this.direction, "direction").AddField(this.drawTilePosition, "drawTilePosition").AddField(this.seasonal, "seasonal").AddField(this.seatType, "seatType").AddField(this.textureFile, "textureFile");
		}

		// Token: 0x06001473 RID: 5235 RVA: 0x000F6BB4 File Offset: 0x000F4DB4
		public static MapSeat FromData(string data, int x, int y)
		{
			MapSeat instance = new MapSeat();
			try
			{
				string[] data_split = data.Split('/', StringSplitOptions.None);
				instance.tilePosition.Set(new Vector2((float)x, (float)y));
				instance.size.Set(new Vector2((float)int.Parse(data_split[0]), (float)int.Parse(data_split[1])));
				instance.seatType.Value = data_split[3];
				int direction;
				if (data_split[2] == "opposite")
				{
					instance.direction.Value = -2;
				}
				else if (Utility.TryParseDirection(data_split[2], out direction))
				{
					instance.direction.Value = direction;
				}
				else
				{
					instance.direction.Value = 2;
				}
				instance.drawTilePosition.Set(new Vector2((float)int.Parse(data_split[4]), (float)int.Parse(data_split[5])));
				instance.seasonal.Value = (data_split[6] == "true");
				if (data_split.Length > 7)
				{
					instance.textureFile.Value = data_split[7];
				}
				else
				{
					instance.textureFile.Value = null;
				}
			}
			catch (Exception)
			{
			}
			return instance;
		}

		// Token: 0x06001474 RID: 5236 RVA: 0x000F6CCC File Offset: 0x000F4ECC
		public bool IsBlocked(GameLocation location)
		{
			Rectangle rect = this.GetSeatBounds();
			rect.X *= 64;
			rect.Y *= 64;
			rect.Width *= 64;
			rect.Height *= 64;
			Rectangle extended_rect = rect;
			switch (this.direction.Value)
			{
			case 0:
				extended_rect.Y -= 32;
				extended_rect.Height += 32;
				break;
			case 1:
				extended_rect.Width += 32;
				break;
			case 2:
				extended_rect.Height += 32;
				break;
			case 3:
				extended_rect.X -= 32;
				extended_rect.Width += 32;
				break;
			}
			foreach (NPC character in ((Game1.CurrentEvent != null) ? Game1.CurrentEvent.actors : location.characters.ToList<NPC>()))
			{
				Rectangle character_rect = character.GetBoundingBox();
				if (character_rect.Intersects(rect))
				{
					return true;
				}
				if (!character.isMovingOnPathFindPath.Value && character_rect.Intersects(extended_rect))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001475 RID: 5237 RVA: 0x000F6E20 File Offset: 0x000F5020
		public bool IsSittingHere(Farmer who)
		{
			return this.sittingFarmers.ContainsKey(who.UniqueMultiplayerID);
		}

		// Token: 0x06001476 RID: 5238 RVA: 0x000F6E33 File Offset: 0x000F5033
		public bool HasSittingFarmers()
		{
			return this.sittingFarmers.Length > 0;
		}

		// Token: 0x06001477 RID: 5239 RVA: 0x000F6E44 File Offset: 0x000F5044
		public List<Vector2> GetSeatPositions(bool ignore_offsets = false)
		{
			this.customDrawValues = null;
			List<Vector2> seat_positions = new List<Vector2>();
			string value = this.seatType.Value;
			if (!(value == "playground"))
			{
				if (!(value == "ccdesk"))
				{
					if (this.seatType.Value.StartsWith("custom "))
					{
						float offset_x = 0f;
						float offset_y = 0f;
						float extra_height = 0f;
						string[] custom_values = ArgUtility.SplitBySpace(this.seatType.Value);
						try
						{
							if (custom_values.Length > 1)
							{
								offset_x = float.Parse(custom_values[1]);
							}
							if (custom_values.Length > 2)
							{
								offset_y = float.Parse(custom_values[2]);
							}
							if (custom_values.Length > 3)
							{
								extra_height = float.Parse(custom_values[3]);
							}
						}
						catch (Exception)
						{
						}
						this.customDrawValues = new Vector3?(new Vector3(offset_x, offset_y, extra_height));
						Vector2 seat = new Vector2(this.tilePosition.X + this.customDrawValues.Value.X, this.tilePosition.Y);
						if (!ignore_offsets)
						{
							seat.Y += this.customDrawValues.Value.Y;
						}
						seat_positions.Add(seat);
					}
					else
					{
						int x = 0;
						while ((float)x < this.size.X)
						{
							int y = 0;
							while ((float)y < this.size.Y)
							{
								Vector2 offset = new Vector2(0f, 0f);
								if (this.seatType.Value.StartsWith("bench"))
								{
									if (this.direction.Value == 2)
									{
										offset.Y += 0.25f;
									}
									else if ((this.direction.Value == 3 || this.direction.Value == 1) && y == 0)
									{
										offset.Y += 0.5f;
									}
								}
								if (this.seatType.Value.StartsWith("picnic"))
								{
									int value2 = this.direction.Value;
									if (value2 != 0)
									{
										if (value2 == 2)
										{
											offset.Y -= 0.25f;
										}
									}
									else
									{
										offset.Y += 0.25f;
									}
								}
								if (this.seatType.Value.EndsWith("swings"))
								{
									offset.Y -= 0.5f;
								}
								else if (this.seatType.Value.EndsWith("summitbench"))
								{
									offset.Y -= 0.2f;
								}
								else if (this.seatType.Value.EndsWith("tall"))
								{
									offset.Y -= 0.3f;
								}
								else if (this.seatType.Value.EndsWith("short"))
								{
									offset.Y += 0.3f;
								}
								if (ignore_offsets)
								{
									offset = Vector2.Zero;
								}
								seat_positions.Add(this.tilePosition.Value + new Vector2((float)x + offset.X, (float)y + offset.Y));
								y++;
							}
							x++;
						}
					}
				}
				else
				{
					Vector2 seat2 = new Vector2(this.tilePosition.X + 0.5f, this.tilePosition.Y);
					if (!ignore_offsets)
					{
						seat2.Y -= 0.4f;
					}
					seat_positions.Add(seat2);
				}
			}
			else
			{
				Vector2 seat3 = new Vector2(this.tilePosition.X + 0.75f, this.tilePosition.Y);
				if (!ignore_offsets)
				{
					seat3.Y -= 0.1f;
				}
				seat_positions.Add(seat3);
			}
			return seat_positions;
		}

		// Token: 0x06001478 RID: 5240 RVA: 0x000F71F4 File Offset: 0x000F53F4
		public virtual void Draw(SpriteBatch b)
		{
			if (this._loadedTextureFile != this.textureFile.Value)
			{
				this._loadedTextureFile = this.textureFile.Value;
				try
				{
					this.overlayTexture = Game1.content.Load<Texture2D>(this._loadedTextureFile);
				}
				catch (Exception)
				{
					this.overlayTexture = null;
				}
			}
			if (this.overlayTexture == null)
			{
				this.overlayTexture = MapSeat.mapChairTexture;
			}
			if (this.drawTilePosition.Value.X >= 0f)
			{
				if (!this.HasSittingFarmers())
				{
					return;
				}
				float extra_height = 0f;
				if (this.customDrawValues != null)
				{
					extra_height = this.customDrawValues.Value.Z;
				}
				else if (this.seatType.Value.StartsWith("highback_chair") || this.seatType.Value.StartsWith("ccdesk"))
				{
					extra_height = 1f;
				}
				Vector2 draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tilePosition.X * 64f, (this.tilePosition.Y - extra_height) * 64f));
				float sort_layer = (float)(((double)((float)((int)this.tilePosition.Y) + this.size.Y) + 0.1) * 64.0) / 10000f;
				Rectangle source_rect = new Rectangle((int)this.drawTilePosition.Value.X * 16, (int)(this.drawTilePosition.Value.Y - extra_height) * 16, (int)this.size.Value.X * 16, (int)(this.size.Value.Y + extra_height) * 16);
				if (this.seasonal.Value)
				{
					source_rect.X += source_rect.Width * Game1.currentLocation.GetSeasonIndex();
				}
				b.Draw(this.overlayTexture, draw_position, new Rectangle?(source_rect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
			}
		}

		// Token: 0x06001479 RID: 5241 RVA: 0x000F7404 File Offset: 0x000F5604
		public bool OccupiesTile(int x, int y)
		{
			return this.GetSeatBounds().Contains(x, y);
		}

		// Token: 0x0600147A RID: 5242 RVA: 0x000F7424 File Offset: 0x000F5624
		public virtual Vector2? AddSittingFarmer(Farmer who)
		{
			if (who == Game1.player)
			{
				this.localSittingDirection = this.direction.Value;
				if (this.seatType.Value.StartsWith("stool"))
				{
					this.localSittingDirection = Game1.player.FacingDirection;
				}
				if (this.direction.Value == -2)
				{
					this.localSittingDirection = Utility.GetOppositeFacingDirection(Game1.player.FacingDirection);
				}
				if (this.seatType.Value.StartsWith("bathchair") && this.localSittingDirection == 0)
				{
					this.localSittingDirection = 2;
				}
			}
			List<Vector2> seat_positions = this.GetSeatPositions(false);
			if (seat_positions.Count == 0)
			{
				return null;
			}
			bool[] overrideSeatsFilled;
			this.CheckSeatOccupancyIfTemporaryMap(who, seat_positions, out overrideSeatsFilled);
			if (overrideSeatsFilled.All((bool occupied) => occupied))
			{
				return null;
			}
			int seat_index = -1;
			Vector2? sit_position = null;
			float distance = 96f;
			for (int i = 0; i < seat_positions.Count; i++)
			{
				if (!this.sittingFarmers.Values.Contains(i) && !overrideSeatsFilled[i])
				{
					float curr_distance = ((seat_positions[i] + new Vector2(0.5f, 0.5f)) * 64f - who.getStandingPosition()).Length();
					if (curr_distance < distance)
					{
						distance = curr_distance;
						sit_position = new Vector2?(seat_positions[i]);
						seat_index = i;
					}
				}
			}
			if (sit_position != null)
			{
				this.sittingFarmers[who.UniqueMultiplayerID] = seat_index;
			}
			return sit_position;
		}

		// Token: 0x0600147B RID: 5243 RVA: 0x000F75D1 File Offset: 0x000F57D1
		public bool IsSeatHere(GameLocation location)
		{
			return location.mapSeats.Contains(this);
		}

		// Token: 0x0600147C RID: 5244 RVA: 0x000F75DF File Offset: 0x000F57DF
		public int GetSittingDirection()
		{
			return this.localSittingDirection;
		}

		// Token: 0x0600147D RID: 5245 RVA: 0x000F75E8 File Offset: 0x000F57E8
		public Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false)
		{
			int index;
			if (this.sittingFarmers.TryGetValue(who.UniqueMultiplayerID, out index))
			{
				return new Vector2?(this.GetSeatPositions(ignore_offsets)[index]);
			}
			return null;
		}

		// Token: 0x0600147E RID: 5246 RVA: 0x000F7628 File Offset: 0x000F5828
		public virtual Rectangle GetSeatBounds()
		{
			if (this.seatType.Value == "chair" && this.direction.Value == 0)
			{
				new Rectangle((int)this.tilePosition.X, (int)this.tilePosition.Y + 1, (int)this.size.X, (int)this.size.Y - 1);
			}
			return new Rectangle((int)this.tilePosition.X, (int)this.tilePosition.Y, (int)this.size.X, (int)this.size.Y);
		}

		// Token: 0x0600147F RID: 5247 RVA: 0x000F76C8 File Offset: 0x000F58C8
		public virtual void RemoveSittingFarmer(Farmer farmer)
		{
			this.sittingFarmers.Remove(farmer.UniqueMultiplayerID);
		}

		// Token: 0x06001480 RID: 5248 RVA: 0x000F76DC File Offset: 0x000F58DC
		public virtual int GetSittingFarmerCount()
		{
			return this.sittingFarmers.Length;
		}

		// Token: 0x06001481 RID: 5249 RVA: 0x000F76EC File Offset: 0x000F58EC
		private void CheckSeatOccupancyIfTemporaryMap(Farmer who, List<Vector2> seatPositions, out bool[] seatsFilled)
		{
			seatsFilled = new bool[seatPositions.Count];
			GameLocation location = who.currentLocation;
			if (location == null || !location.IsTemporary)
			{
				return;
			}
			FarmerCollection playersHere = location.farmers ?? Game1.getOnlineFarmers();
			if (playersHere.Count <= 1)
			{
				return;
			}
			List<Vector2> seatTilePositions = this.GetSeatPositions(true);
			Vector2 minPosition = seatTilePositions[0];
			Vector2 maxPosition = seatTilePositions[0];
			for (int i = 1; i < seatTilePositions.Count; i++)
			{
				Vector2 seatPosition = seatTilePositions[i];
				Vector2.Min(ref minPosition, ref seatPosition, out minPosition);
				Vector2.Max(ref maxPosition, ref seatPosition, out maxPosition);
			}
			minPosition -= new Vector2(1E-05f, 1E-05f);
			maxPosition += new Vector2(1E-05f, 1E-05f);
			int remaining = seatTilePositions.Count;
			foreach (Farmer farmer in playersHere)
			{
				if (farmer.isSitting.Value && !(farmer.uniqueMultiplayerID == who.uniqueMultiplayerID))
				{
					Vector2 sitPosition = farmer.mapChairSitPosition.Value;
					if (sitPosition.X > minPosition.X && sitPosition.X < maxPosition.X && sitPosition.Y > minPosition.Y && sitPosition.Y < maxPosition.Y)
					{
						for (int j = 0; j < seatTilePositions.Count; j++)
						{
							if (!seatsFilled[j])
							{
								Vector2 diff = seatTilePositions[j] - sitPosition;
								if (Math.Abs(diff.X) < 1E-05f && Math.Abs(diff.Y) < 1E-05f)
								{
									seatsFilled[j] = true;
									remaining--;
									break;
								}
							}
						}
						if (remaining == 0)
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x04000D1A RID: 3354
		[XmlIgnore]
		public static Texture2D mapChairTexture;

		// Token: 0x04000D1B RID: 3355
		[XmlIgnore]
		public NetLongDictionary<int, NetInt> sittingFarmers = new NetLongDictionary<int, NetInt>();

		// Token: 0x04000D1C RID: 3356
		[XmlIgnore]
		public NetVector2 tilePosition = new NetVector2();

		// Token: 0x04000D1D RID: 3357
		[XmlIgnore]
		public NetVector2 size = new NetVector2();

		// Token: 0x04000D1E RID: 3358
		[XmlIgnore]
		public NetInt direction = new NetInt();

		// Token: 0x04000D1F RID: 3359
		[XmlIgnore]
		public NetVector2 drawTilePosition = new NetVector2(new Vector2(-1f, -1f));

		// Token: 0x04000D21 RID: 3361
		[XmlIgnore]
		public NetBool seasonal = new NetBool();

		// Token: 0x04000D22 RID: 3362
		[XmlIgnore]
		public NetString seatType = new NetString();

		// Token: 0x04000D23 RID: 3363
		[XmlIgnore]
		public NetString textureFile = new NetString(null);

		// Token: 0x04000D24 RID: 3364
		[XmlIgnore]
		public string _loadedTextureFile;

		// Token: 0x04000D25 RID: 3365
		[XmlIgnore]
		public Texture2D overlayTexture;

		// Token: 0x04000D26 RID: 3366
		[XmlIgnore]
		public int localSittingDirection = 2;

		// Token: 0x04000D27 RID: 3367
		[XmlIgnore]
		public Vector3? customDrawValues;
	}
}

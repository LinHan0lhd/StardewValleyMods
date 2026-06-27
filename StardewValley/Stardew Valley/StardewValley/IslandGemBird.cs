using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;

namespace StardewValley
{
	// Token: 0x020000B8 RID: 184
	public class IslandGemBird : INetObject<NetFields>
	{
		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000CC5 RID: 3269 RVA: 0x0008EC87 File Offset: 0x0008CE87
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("IslandGemBird");

		// Token: 0x06000CC6 RID: 3270 RVA: 0x0008EC90 File Offset: 0x0008CE90
		public IslandGemBird()
		{
			this.texture = Game1.content.Load<Texture2D>("LooseSprites\\GemBird");
			this.InitNetFields();
		}

		// Token: 0x06000CC7 RID: 3271 RVA: 0x0008ED70 File Offset: 0x0008CF70
		public IslandGemBird(Vector2 tile_position, IslandGemBird.GemBirdType bird_type) : this()
		{
			this.position.Value = (tile_position + new Vector2(0.5f, 0.5f)) * 64f;
			this.color.Value = IslandGemBird.GetColor(bird_type);
			this.itemIndex.Value = IslandGemBird.GetItemIndex(bird_type);
		}

		// Token: 0x06000CC8 RID: 3272 RVA: 0x0008EDD0 File Offset: 0x0008CFD0
		public static Color GetColor(IslandGemBird.GemBirdType bird_type)
		{
			switch (bird_type)
			{
			case IslandGemBird.GemBirdType.Emerald:
				return new Color(67, 255, 83);
			case IslandGemBird.GemBirdType.Aquamarine:
				return new Color(74, 243, 255);
			case IslandGemBird.GemBirdType.Ruby:
				return new Color(255, 38, 38);
			case IslandGemBird.GemBirdType.Amethyst:
				return new Color(255, 67, 251);
			case IslandGemBird.GemBirdType.Topaz:
				return new Color(255, 156, 33);
			default:
				return Color.White;
			}
		}

		// Token: 0x06000CC9 RID: 3273 RVA: 0x0008EE54 File Offset: 0x0008D054
		public static string GetItemIndex(IslandGemBird.GemBirdType bird_type)
		{
			switch (bird_type)
			{
			case IslandGemBird.GemBirdType.Emerald:
				return "60";
			case IslandGemBird.GemBirdType.Aquamarine:
				return "62";
			case IslandGemBird.GemBirdType.Ruby:
				return "64";
			case IslandGemBird.GemBirdType.Amethyst:
				return "66";
			case IslandGemBird.GemBirdType.Topaz:
				return "68";
			default:
				return "0";
			}
		}

		// Token: 0x06000CCA RID: 3274 RVA: 0x0008EEA0 File Offset: 0x0008D0A0
		public static IslandGemBird.GemBirdType GetBirdTypeForLocation(string location)
		{
			List<string> island_locations = new List<string>
			{
				"IslandNorth",
				"IslandSouth",
				"IslandEast",
				"IslandWest"
			};
			if (!island_locations.Contains(location))
			{
				return IslandGemBird.GemBirdType.Aquamarine;
			}
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0, 0.0);
			List<IslandGemBird.GemBirdType> types = new List<IslandGemBird.GemBirdType>();
			for (int i = 0; i < 5; i++)
			{
				types.Add((IslandGemBird.GemBirdType)i);
			}
			Utility.Shuffle<IslandGemBird.GemBirdType>(r, types);
			return types[island_locations.IndexOf(location)];
		}

		// Token: 0x06000CCB RID: 3275 RVA: 0x0008EF48 File Offset: 0x0008D148
		public void Draw(SpriteBatch b)
		{
			if (this.currentAnimation != null)
			{
				int frame = this.currentAnimation[Math.Min(this.currentFrameIndex, this.currentAnimation.Length - 1)];
				b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position.Value + new Vector2(0f, -this.height.Value)), new Rectangle?(new Rectangle(frame * 32, 0, 32, 32)), Color.White * this.alpha.Value, 0f, new Vector2(16f, 32f), 4f, SpriteEffects.None, (this.position.Value.Y - 1f) / 10000f);
				b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position.Value + new Vector2(0f, -this.height.Value)), new Rectangle?(new Rectangle(frame * 32, 32, 32, 32)), this.color.Value * this.alpha.Value, 0f, new Vector2(16f, 32f), 4f, SpriteEffects.None, this.position.Value.Y / 10000f);
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position.Value), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * this.alpha.Value, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (this.position.Y - 2f) / 10000f);
			}
		}

		// Token: 0x06000CCC RID: 3276 RVA: 0x0008F150 File Offset: 0x0008D350
		public void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.position, "position").AddField(this.flying, "flying").AddField(this.height, "height").AddField(this.color, "color").AddField(this.alpha, "alpha").AddField(this.itemIndex, "itemIndex");
			this.position.Interpolated(true, true);
			this.height.Interpolated(true, true);
			this.alpha.Interpolated(true, true);
		}

		// Token: 0x06000CCD RID: 3277 RVA: 0x0008F1F4 File Offset: 0x0008D3F4
		public bool Update(GameTime time, GameLocation location)
		{
			if (this.currentAnimation == null)
			{
				this.currentAnimation = this.idleAnimation;
			}
			this.frameTimer += (float)time.ElapsedGameTime.TotalSeconds;
			float frame_time = 0.15f;
			if (this.flying.Value)
			{
				frame_time = 0.05f;
			}
			if (this.frameTimer >= frame_time)
			{
				this.frameTimer = 0f;
				this.currentFrameIndex++;
				if (this.currentFrameIndex >= this.currentAnimation.Length)
				{
					this.currentFrameIndex = 0;
					if (this.currentAnimation == this.flyAnimation && location == Game1.currentLocation && Utility.isOnScreen(this.position.Value + new Vector2(0f, -this.height.Value), 64))
					{
						Game1.playSound("batFlap", null);
					}
					if (this.currentAnimation == this.lookBackAnimation || this.currentAnimation == this.scratchAnimation)
					{
						this.currentAnimation = this.idleAnimation;
					}
				}
			}
			if (this.flying.Value)
			{
				this.currentAnimation = this.flyAnimation;
				if (Game1.IsMasterGame)
				{
					this.height.Value += 4f;
					this.position.X -= 3f;
					if (this.alpha.Value > 0f && this.height.Value >= 300f)
					{
						this.alpha.Value -= 0.01f;
						if (this.alpha.Value < 0f)
						{
							this.alpha.Value = 0f;
						}
					}
				}
			}
			else
			{
				if (this.currentAnimation == this.idleAnimation)
				{
					this.idleAnimationTime -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (this.idleAnimationTime <= 0f)
				{
					this.currentFrameIndex = 0;
					if (Game1.random.NextDouble() < 0.75)
					{
						this.currentAnimation = this.lookBackAnimation;
					}
					else
					{
						this.currentAnimation = this.scratchAnimation;
					}
					this.idleAnimationTime = Utility.RandomFloat(1f, 3f, null);
				}
			}
			if (Game1.IsMasterGame && !this.flying.Value)
			{
				foreach (Farmer farmer in location.farmers)
				{
					Vector2 offset = farmer.Position - this.position.Value;
					if (Math.Abs(offset.X) <= 128f && Math.Abs(offset.Y) <= 128f)
					{
						this.flying.Value = true;
						location.playSound("parrot", null, null, SoundContext.Default);
						Game1.createObjectDebris(this.itemIndex.Value, (int)(this.position.X / 64f), (int)(this.position.Y / 64f), location);
					}
				}
			}
			if (this.alpha.Value <= 0f)
			{
				if (this._destroyTimer == 0f)
				{
					this._destroyTimer = 3f;
				}
				else if (this._destroyTimer >= 0f)
				{
					this._destroyTimer -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this._destroyTimer <= 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x040008B5 RID: 2229
		[XmlIgnore]
		public Texture2D texture;

		// Token: 0x040008B6 RID: 2230
		[XmlElement("position")]
		public NetVector2 position = new NetVector2();

		// Token: 0x040008B7 RID: 2231
		[XmlIgnore]
		protected float _destroyTimer;

		// Token: 0x040008B8 RID: 2232
		[XmlElement("height")]
		public NetFloat height = new NetFloat();

		// Token: 0x040008B9 RID: 2233
		[XmlIgnore]
		public int[] idleAnimation = new int[1];

		// Token: 0x040008BA RID: 2234
		[XmlIgnore]
		public int[] lookBackAnimation = new int[]
		{
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1
		};

		// Token: 0x040008BB RID: 2235
		[XmlIgnore]
		public int[] scratchAnimation = new int[]
		{
			0,
			1,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2
		};

		// Token: 0x040008BC RID: 2236
		[XmlIgnore]
		public int[] flyAnimation = new int[]
		{
			4,
			5,
			6,
			7,
			7,
			6,
			6,
			5,
			5,
			4,
			4
		};

		// Token: 0x040008BD RID: 2237
		[XmlIgnore]
		public int[] currentAnimation;

		// Token: 0x040008BE RID: 2238
		[XmlIgnore]
		public float frameTimer;

		// Token: 0x040008BF RID: 2239
		[XmlIgnore]
		public int currentFrameIndex;

		// Token: 0x040008C0 RID: 2240
		[XmlIgnore]
		public float idleAnimationTime;

		// Token: 0x040008C1 RID: 2241
		[XmlElement("alpha")]
		public NetFloat alpha = new NetFloat(1f);

		// Token: 0x040008C2 RID: 2242
		[XmlElement("flying")]
		public NetBool flying = new NetBool();

		// Token: 0x040008C3 RID: 2243
		[XmlElement("color")]
		public NetColor color = new NetColor();

		// Token: 0x040008C4 RID: 2244
		[XmlElement("itemIndex")]
		public NetString itemIndex = new NetString("0");

		// Token: 0x02000469 RID: 1129
		public enum GemBirdType
		{
			// Token: 0x0400282A RID: 10282
			Emerald,
			// Token: 0x0400282B RID: 10283
			Aquamarine,
			// Token: 0x0400282C RID: 10284
			Ruby,
			// Token: 0x0400282D RID: 10285
			Amethyst,
			// Token: 0x0400282E RID: 10286
			Topaz,
			// Token: 0x0400282F RID: 10287
			MAX
		}
	}
}

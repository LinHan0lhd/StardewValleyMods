using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Locations
{
	// Token: 0x020002DF RID: 735
	public class IslandSouthEast : IslandLocation
	{
		// Token: 0x0600308C RID: 12428 RVA: 0x00265B1C File Offset: 0x00263D1C
		public IslandSouthEast()
		{
		}

		// Token: 0x0600308D RID: 12429 RVA: 0x00265BBC File Offset: 0x00263DBC
		public IslandSouthEast(string map, string name) : base(map, name)
		{
		}

		// Token: 0x0600308E RID: 12430 RVA: 0x00265C60 File Offset: 0x00263E60
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.mermaidPuzzleSuccess, "mermaidPuzzleSuccess").AddField(this.mermaidPuzzleFinished, "mermaidPuzzleFinished").AddField(this.fishWalnutEvent, "fishWalnutEvent").AddField(this.fishedWalnut, "fishedWalnut");
			this.mermaidPuzzleSuccess.onEvent += this.OnMermaidPuzzleSuccess;
			this.fishWalnutEvent.onEvent += this.OnFishWalnut;
		}

		// Token: 0x0600308F RID: 12431 RVA: 0x00265CEC File Offset: 0x00263EEC
		public virtual void OnMermaidPuzzleSuccess()
		{
			this.currentMermaidAnimation = this.mermaidReward;
			this.mermaidFrameTimer = 0f;
			if (Game1.currentLocation == this)
			{
				Game1.playSound("yoba", null);
			}
			if (Game1.IsMasterGame && !this.mermaidPuzzleFinished.Value)
			{
				Game1.player.team.MarkCollectedNut("Mermaid");
				this.mermaidPuzzleFinished.Value = true;
				for (int i = 0; i < 5; i++)
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(32f, 33f) * 64f, 0, this, 0, false);
				}
			}
		}

		// Token: 0x06003090 RID: 12432 RVA: 0x00265D9C File Offset: 0x00263F9C
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (base.IsRainingHere())
			{
				base.setMapTile(16, 27, 3, "Back", "untitled tile sheet3", "", true);
				base.setMapTile(18, 27, 4, "Back", "untitled tile sheet3", "", true);
				base.setMapTile(20, 27, 5, "Back", "untitled tile sheet3", "", true);
				base.setMapTile(22, 27, 6, "Back", "untitled tile sheet3", "", true);
				base.setMapTile(24, 27, 7, "Back", "untitled tile sheet3", "", true);
				base.setMapTile(26, 27, 8, "Back", "untitled tile sheet3", "", true);
			}
			else
			{
				base.setMapTile(16, 27, 39, "Back", "untitled tile sheet", "", true);
				base.setMapTile(18, 27, 39, "Back", "untitled tile sheet", "", true);
				base.setMapTile(20, 27, 39, "Back", "untitled tile sheet", "", true);
				base.setMapTile(22, 27, 39, "Back", "untitled tile sheet", "", true);
				base.setMapTile(24, 27, 39, "Back", "untitled tile sheet", "", true);
				base.setMapTile(26, 27, 39, "Back", "untitled tile sheet", "", true);
			}
			if (IslandSouthEastCave.isPirateNight())
			{
				base.setMapTile(29, 18, 36, "Buildings", "untitled tile sheet3", null, true);
				base.setTileProperty(29, 18, "Buildings", "Passable", "T");
				base.setMapTile(29, 19, 68, "Buildings", "untitled tile sheet3", null, true);
				base.setTileProperty(29, 19, "Buildings", "Passable", "T");
				base.setMapTile(30, 18, 99, "Buildings", "untitled tile sheet3", null, true);
				base.setTileProperty(30, 18, "Buildings", "Passable", "T");
				base.setMapTile(30, 19, 131, "Buildings", "untitled tile sheet3", null, true);
				base.setTileProperty(30, 19, "Buildings", "Passable", "T");
				return;
			}
			base.setMapTile(29, 18, 35, "Buildings", "untitled tile sheet3", null, true);
			base.setTileProperty(29, 18, "Buildings", "Passable", "T");
			base.setMapTile(29, 19, 67, "Buildings", "untitled tile sheet3", null, true);
			base.setTileProperty(29, 19, "Buildings", "Passable", "T");
			base.setMapTile(30, 18, 35, "Buildings", "untitled tile sheet3", null, true);
			base.setTileProperty(30, 18, "Buildings", "Passable", "T");
			base.setMapTile(30, 19, 67, "Buildings", "untitled tile sheet3", null, true);
			base.setTileProperty(30, 19, "Buildings", "Passable", "T");
		}

		// Token: 0x06003091 RID: 12433 RVA: 0x002660B4 File Offset: 0x002642B4
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			if (IslandSouthEastCave.isPirateNight())
			{
				Game1.changeMusicTrack("PIRATE_THEME(muffled)", true, MusicContext.SubLocation);
				if (!base.hasLightSource("IslandSouthEast"))
				{
					this.sharedLights.AddLight(new LightSource("IslandSouthEast", 1, new Vector2(30.5f, 18.5f) * 64f, 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				}
			}
			if (base.AreMoonlightJelliesOut())
			{
				base.addMoonlightJellies(50, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0, 0.0, 0.0), new Rectangle(0, 0, 0, 0));
			}
		}

		// Token: 0x06003092 RID: 12434 RVA: 0x00266185 File Offset: 0x00264385
		public override void cleanupBeforePlayerExit()
		{
			base.removeLightSource("IslandSouthEast");
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06003093 RID: 12435 RVA: 0x00266198 File Offset: 0x00264398
		public override void SetBuriedNutLocations()
		{
			base.SetBuriedNutLocations();
			this.buriedNutPoints.Add(new Point(25, 17));
		}

		// Token: 0x06003094 RID: 12436 RVA: 0x002661B4 File Offset: 0x002643B4
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.mermaidPuzzleSuccess.Poll();
			this.fishWalnutEvent.Poll();
			if (!this.fishedWalnut.Value && Game1.random.NextDouble() < 0.005)
			{
				base.playSound("waterSlosh", null, null, SoundContext.Default);
				this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(1216f, 1344f), false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false));
			}
			if (this.MermaidIsHere())
			{
				bool should_wave = false;
				if (this.mermaidPuzzleFinished.Value)
				{
					foreach (Farmer farmer in this.farmers)
					{
						Point point = farmer.TilePoint;
						if (point.X > 24 && point.Y > 25)
						{
							should_wave = true;
							break;
						}
					}
				}
				if (should_wave && (this.currentMermaidAnimation == null || this.currentMermaidAnimation == this.mermaidIdle))
				{
					this.currentMermaidAnimation = this.mermaidWave;
					this.mermaidFrameIndex = 0;
					this.mermaidFrameTimer = 0f;
				}
				if (this.mermaidDanceTime > 0f)
				{
					if (this.currentMermaidAnimation == null || this.currentMermaidAnimation == this.mermaidIdle)
					{
						this.currentMermaidAnimation = this.mermaidDance;
						this.mermaidFrameTimer = 0f;
					}
					this.mermaidDanceTime -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this.mermaidDanceTime < 0f && this.currentMermaidAnimation == this.mermaidDance)
					{
						this.currentMermaidAnimation = this.mermaidIdle;
						this.mermaidFrameTimer = 0f;
					}
				}
				this.mermaidFrameTimer += (float)time.ElapsedGameTime.TotalSeconds;
				if (this.mermaidFrameTimer > 0.25f)
				{
					this.mermaidFrameTimer = 0f;
					this.mermaidFrameIndex++;
					if (this.currentMermaidAnimation == null)
					{
						this.mermaidFrameIndex = 0;
						return;
					}
					if (this.mermaidFrameIndex >= this.currentMermaidAnimation.Length)
					{
						this.mermaidFrameIndex = 0;
						if (this.currentMermaidAnimation == this.mermaidReward)
						{
							if (should_wave)
							{
								this.currentMermaidAnimation = this.mermaidWave;
								return;
							}
							this.currentMermaidAnimation = this.mermaidIdle;
							return;
						}
						else if (!should_wave && this.currentMermaidAnimation == this.mermaidWave)
						{
							this.currentMermaidAnimation = this.mermaidIdle;
						}
					}
				}
			}
		}

		// Token: 0x06003095 RID: 12437 RVA: 0x00266478 File Offset: 0x00264678
		public bool MermaidIsHere()
		{
			return base.IsRainingHere();
		}

		// Token: 0x06003096 RID: 12438 RVA: 0x00266480 File Offset: 0x00264680
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.MermaidIsHere())
			{
				int frame = 0;
				int num = this.mermaidFrameIndex;
				int[] array = this.currentMermaidAnimation;
				int? num2 = (array != null) ? new int?(array.Length) : null;
				if (num < num2.GetValueOrDefault() & num2 != null)
				{
					frame = this.currentMermaidAnimation[this.mermaidFrameIndex];
				}
				b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(32f, 32f) * 64f + new Vector2(0f, -8f) * 4f), new Rectangle?(new Rectangle(304 + 28 * frame, 592, 28, 36)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			}
		}

		// Token: 0x06003097 RID: 12439 RVA: 0x00266564 File Offset: 0x00264764
		public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if ((int)bobberTile.X >= 18 && (int)bobberTile.X <= 20 && (int)bobberTile.Y >= 20 && (int)bobberTile.Y <= 22)
			{
				if (!this.fishedWalnut.Value)
				{
					Game1.player.team.MarkCollectedNut("StardropPool");
					if (!Game1.IsMultiplayer)
					{
						this.fishedWalnut.Value = true;
						return ItemRegistry.Create("(O)73", 1, 0, false);
					}
					this.fishWalnutEvent.Fire();
				}
				return null;
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, null);
		}

		// Token: 0x06003098 RID: 12440 RVA: 0x00266600 File Offset: 0x00264800
		public void OnFishWalnut()
		{
			if (!this.fishedWalnut.Value && Game1.IsMasterGame)
			{
				Vector2 tile = new Vector2(19f, 21f);
				Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), tile * 64f + new Vector2(0.5f, 1.5f) * 64f, 0, this, 0, false);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(28, 100f, 2, 1, tile * 64f, false, false)
					{
						layerDepth = ((tile.Y + 0.5f) * 64f + 2f) / 10000f
					}
				});
				base.playSound("dropItemInWater", null, null, SoundContext.Default);
				this.fishedWalnut.Value = true;
			}
		}

		// Token: 0x06003099 RID: 12441 RVA: 0x002666F8 File Offset: 0x002648F8
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandSouthEast islandSouthEast = l as IslandSouthEast;
			if (islandSouthEast != null)
			{
				this.mermaidPuzzleFinished.Value = islandSouthEast.mermaidPuzzleFinished.Value;
				this.fishedWalnut.Value = islandSouthEast.fishedWalnut.Value;
			}
		}

		// Token: 0x0600309A RID: 12442 RVA: 0x00266744 File Offset: 0x00264944
		public virtual void OnFlutePlayed(int pitch)
		{
			if (!this.MermaidIsHere())
			{
				return;
			}
			if (this.songIndex == -1)
			{
				this.lastPlayedNote = pitch;
				this.songIndex = 0;
			}
			int relative_pitch = pitch - this.lastPlayedNote;
			if (relative_pitch == 900)
			{
				this.songIndex = 1;
				this.mermaidDanceTime = 5f;
			}
			else
			{
				switch (this.songIndex)
				{
				case 1:
					if (relative_pitch == -200)
					{
						this.songIndex++;
						this.mermaidDanceTime = 5f;
					}
					else
					{
						this.songIndex = -1;
						this.mermaidDanceTime = 0f;
						this.currentMermaidAnimation = this.mermaidIdle;
					}
					break;
				case 2:
					if (relative_pitch == -400)
					{
						this.songIndex++;
						this.mermaidDanceTime = 5f;
					}
					else
					{
						this.songIndex = -1;
						this.mermaidDanceTime = 0f;
						this.currentMermaidAnimation = this.mermaidIdle;
					}
					break;
				case 3:
					if (relative_pitch == 200)
					{
						this.songIndex = 0;
						this.mermaidPuzzleSuccess.Fire();
						this.mermaidDanceTime = 0f;
					}
					else
					{
						this.songIndex = -1;
						this.mermaidDanceTime = 0f;
						this.currentMermaidAnimation = this.mermaidIdle;
					}
					break;
				}
			}
			this.lastPlayedNote = pitch;
		}

		// Token: 0x040020B3 RID: 8371
		private const string lightId = "IslandSouthEast";

		// Token: 0x040020B4 RID: 8372
		[XmlIgnore]
		public Texture2D mermaidSprites;

		// Token: 0x040020B5 RID: 8373
		[XmlIgnore]
		public int lastPlayedNote = -1;

		// Token: 0x040020B6 RID: 8374
		[XmlIgnore]
		public int songIndex = -1;

		// Token: 0x040020B7 RID: 8375
		[XmlIgnore]
		public int[] mermaidIdle = new int[1];

		// Token: 0x040020B8 RID: 8376
		[XmlIgnore]
		public int[] mermaidWave = new int[]
		{
			1,
			1,
			2,
			2
		};

		// Token: 0x040020B9 RID: 8377
		[XmlIgnore]
		public int[] mermaidReward = new int[]
		{
			3,
			3,
			3,
			3,
			3,
			4,
			4
		};

		// Token: 0x040020BA RID: 8378
		[XmlIgnore]
		public int[] mermaidDance = new int[]
		{
			5,
			5,
			5,
			6,
			6,
			6
		};

		// Token: 0x040020BB RID: 8379
		[XmlIgnore]
		public int mermaidFrameIndex;

		// Token: 0x040020BC RID: 8380
		[XmlIgnore]
		public int[] currentMermaidAnimation;

		// Token: 0x040020BD RID: 8381
		[XmlIgnore]
		public float mermaidFrameTimer;

		// Token: 0x040020BE RID: 8382
		[XmlIgnore]
		public float mermaidDanceTime;

		// Token: 0x040020BF RID: 8383
		[XmlIgnore]
		public NetEvent0 mermaidPuzzleSuccess = new NetEvent0(false);

		// Token: 0x040020C0 RID: 8384
		[XmlElement("mermaidPuzzleFinished")]
		public NetBool mermaidPuzzleFinished = new NetBool();

		// Token: 0x040020C1 RID: 8385
		[XmlIgnore]
		public NetEvent0 fishWalnutEvent = new NetEvent0(false);

		// Token: 0x040020C2 RID: 8386
		[XmlElement("fishedWalnut")]
		public NetBool fishedWalnut = new NetBool();
	}
}

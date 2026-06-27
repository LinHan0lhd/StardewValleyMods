using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Minigames
{
	// Token: 0x02000231 RID: 561
	[XmlInclude(typeof(AbigailGame.JOTPKProgress))]
	[InstanceStatics]
	public class AbigailGame : IMinigame
	{
		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x060024E3 RID: 9443 RVA: 0x00192D66 File Offset: 0x00190F66
		public static int TileSize
		{
			get
			{
				return 48;
			}
		}

		// Token: 0x060024E4 RID: 9444 RVA: 0x00192D6C File Offset: 0x00190F6C
		public bool LoadGame()
		{
			if (AbigailGame.playingWithAbigail)
			{
				return false;
			}
			if (Game1.player.jotpkProgress.Value == null)
			{
				return false;
			}
			AbigailGame.JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
			this.ammoLevel = save_data.ammoLevel.Value;
			this.bulletDamage = save_data.bulletDamage.Value;
			this.coins = save_data.coins.Value;
			this.died = save_data.died.Value;
			this.fireSpeedLevel = save_data.fireSpeedLevel.Value;
			this.lives = save_data.lives.Value;
			this.score = save_data.score.Value;
			this.runSpeedLevel = save_data.runSpeedLevel.Value;
			this.spreadPistol = save_data.spreadPistol.Value;
			this.whichRound = save_data.whichRound.Value;
			AbigailGame.whichWave = save_data.whichWave.Value;
			AbigailGame.waveTimer = save_data.waveTimer.Value;
			AbigailGame.world = save_data.world.Value;
			if (save_data.heldItem.Value != -100)
			{
				this.heldItem = new AbigailGame.CowboyPowerup(save_data.heldItem.Value, Point.Zero, 9999);
			}
			this.monsterChances = new List<Vector2>(save_data.monsterChances);
			this.ApplyLevelSpecificStates();
			if (AbigailGame.shootoutLevel)
			{
				this.playerPosition = new Vector2((float)(8 * AbigailGame.TileSize), (float)(3 * AbigailGame.TileSize));
			}
			return true;
		}

		// Token: 0x060024E5 RID: 9445 RVA: 0x00192EE8 File Offset: 0x001910E8
		public void SaveGame()
		{
			if (AbigailGame.playingWithAbigail)
			{
				return;
			}
			if (Game1.player.jotpkProgress.Value == null)
			{
				Game1.player.jotpkProgress.Value = new AbigailGame.JOTPKProgress();
			}
			AbigailGame.JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
			save_data.ammoLevel.Value = this.ammoLevel;
			save_data.bulletDamage.Value = this.bulletDamage;
			save_data.coins.Value = this.coins;
			save_data.died.Value = this.died;
			save_data.fireSpeedLevel.Value = this.fireSpeedLevel;
			save_data.lives.Value = this.lives;
			save_data.score.Value = this.score;
			save_data.runSpeedLevel.Value = this.runSpeedLevel;
			save_data.spreadPistol.Value = this.spreadPistol;
			save_data.whichRound.Value = this.whichRound;
			save_data.whichWave.Value = AbigailGame.whichWave;
			save_data.waveTimer.Value = AbigailGame.waveTimer;
			save_data.world.Value = AbigailGame.world;
			save_data.monsterChances.Clear();
			save_data.monsterChances.AddRange(this.monsterChances);
			if (this.heldItem == null)
			{
				save_data.heldItem.Value = -100;
				return;
			}
			save_data.heldItem.Value = this.heldItem.which;
		}

		// Token: 0x060024E6 RID: 9446 RVA: 0x00193054 File Offset: 0x00191254
		public AbigailGame(NPC abigail = null)
		{
			this.abigail = abigail;
			bool playingWithAbby = abigail != null;
			this.reset(playingWithAbby);
			if (!AbigailGame.playingWithAbigail && this.LoadGame())
			{
				AbigailGame.map = this.getMap(AbigailGame.whichWave);
			}
		}

		// Token: 0x060024E7 RID: 9447 RVA: 0x00193194 File Offset: 0x00191394
		public AbigailGame(int coins, int ammoLevel, int bulletDamage, int fireSpeedLevel, int runSpeedLevel, int lives, bool spreadPistol, int whichRound)
		{
			this.reset(false);
			this.coins = coins;
			this.ammoLevel = ammoLevel;
			this.bulletDamage = bulletDamage;
			this.fireSpeedLevel = fireSpeedLevel;
			this.runSpeedLevel = runSpeedLevel;
			this.lives = lives;
			this.spreadPistol = spreadPistol;
			this.whichRound = whichRound;
			this.ApplyNewGamePlus();
			this.SaveGame();
			AbigailGame.onStartMenu = false;
		}

		// Token: 0x060024E8 RID: 9448 RVA: 0x001932F8 File Offset: 0x001914F8
		public void ApplyNewGamePlus()
		{
			this.monsterChances[0] = new Vector2(0.014f + (float)this.whichRound * 0.005f, 0.41f + (float)this.whichRound * 0.05f);
			this.monsterChances[4] = new Vector2(0.002f, 0.1f);
		}

		// Token: 0x060024E9 RID: 9449 RVA: 0x00193358 File Offset: 0x00191558
		public void reset(bool playingWithAbby)
		{
			Rectangle r = new Rectangle(0, 0, 16, 16);
			this._borderTiles = new HashSet<Vector2>(Utility.getBorderOfThisRectangle(r));
			this.died = false;
			AbigailGame.topLeftScreenCoordinate = new Vector2((float)(Game1.viewport.Width / 2 - 384), (float)(Game1.viewport.Height / 2 - 384));
			AbigailGame.enemyBullets.Clear();
			AbigailGame.holdItemTimer = 0;
			AbigailGame.itemToHold = -1;
			AbigailGame.merchantArriving = false;
			AbigailGame.merchantLeaving = false;
			AbigailGame.merchantShopOpen = false;
			AbigailGame.monsterConfusionTimer = 0;
			AbigailGame.monsters.Clear();
			AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
			AbigailGame.scrollingMap = false;
			AbigailGame.shopping = false;
			AbigailGame.store = false;
			AbigailGame.temporarySprites.Clear();
			AbigailGame.waitingForPlayerToMoveDownAMap = false;
			AbigailGame.waveTimer = 80000;
			AbigailGame.whichWave = 0;
			AbigailGame.zombieModeTimer = 0;
			this.bulletDamage = 1;
			AbigailGame.deathTimer = 0f;
			AbigailGame.shootoutLevel = false;
			AbigailGame.betweenWaveTimer = 5000;
			AbigailGame.gopherRunning = false;
			AbigailGame.hasGopherAppeared = false;
			AbigailGame.playerMovementDirections.Clear();
			AbigailGame.outlawSong = null;
			AbigailGame.overworldSong = null;
			AbigailGame.endCutscene = false;
			AbigailGame.endCutscenePhase = 0;
			AbigailGame.endCutsceneTimer = 0;
			AbigailGame.gameOver = false;
			AbigailGame.deathTimer = 0f;
			AbigailGame.playerInvincibleTimer = 0;
			AbigailGame.playingWithAbigail = playingWithAbby;
			AbigailGame.beatLevelWithAbigail = false;
			AbigailGame.onStartMenu = true;
			AbigailGame.startTimer = 0;
			AbigailGame.powerups.Clear();
			AbigailGame.world = 0;
			Game1.changeMusicTrack("none", false, MusicContext.MiniGame);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if ((i == 0 || i == 15 || j == 0 || j == 15) && (i <= 6 || i >= 10) && (j <= 6 || j >= 10))
					{
						AbigailGame.map[i, j] = 5;
					}
					else if (i == 0 || i == 15 || j == 0 || j == 15)
					{
						AbigailGame.map[i, j] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (i == 1 || i == 14 || j == 1 || j == 14)
					{
						AbigailGame.map[i, j] = 2;
					}
					else
					{
						AbigailGame.map[i, j] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			this.playerPosition = new Vector2(384f, 384f);
			this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
			this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
			this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
			this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
			if (AbigailGame.playingWithAbigail)
			{
				AbigailGame.onStartMenu = false;
				AbigailGame.player2Position = new Vector2(432f, 384f);
				this.player2BoundingBox = new Rectangle(9 * AbigailGame.TileSize, 8 * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize);
				AbigailGame.betweenWaveTimer += 1500;
			}
			for (int k = 0; k < 4; k++)
			{
				this.spawnQueue[k] = new List<Point>();
			}
			this.noPickUpBox = new Rectangle(0, 0, AbigailGame.TileSize, AbigailGame.TileSize);
			this.merchantBox = new Rectangle(8 * AbigailGame.TileSize, 0, AbigailGame.TileSize, AbigailGame.TileSize);
			AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
			if (!Stats.AllowRetroactiveAchievements)
			{
				Game1.stats.checkForMiniGameAchievements(true);
			}
		}

		// Token: 0x060024EA RID: 9450 RVA: 0x001936E0 File Offset: 0x001918E0
		public float getMovementSpeed(float speed, int directions)
		{
			float movementSpeed = speed;
			if (directions > 1)
			{
				movementSpeed = (float)Math.Max(1, (int)Math.Sqrt((double)(2f * (movementSpeed * movementSpeed))) / 2);
			}
			return movementSpeed;
		}

		// Token: 0x060024EB RID: 9451 RVA: 0x00193710 File Offset: 0x00191910
		public bool getPowerUp(AbigailGame.CowboyPowerup c)
		{
			int which = c.which;
			switch (which)
			{
			case -3:
				this.usePowerup(-3);
				break;
			case -2:
				this.usePowerup(-2);
				break;
			case -1:
				this.usePowerup(-1);
				break;
			case 0:
				this.coins++;
				Game1.playSound("Pickup_Coin15", null);
				break;
			case 1:
				this.coins += 5;
				Game1.playSound("Pickup_Coin15", null);
				break;
			default:
				if (which != 8)
				{
					if (this.heldItem != null)
					{
						AbigailGame.CowboyPowerup tmp = this.heldItem;
						this.heldItem = c;
						this.noPickUpBox.Location = c.position;
						tmp.position = c.position;
						AbigailGame.powerups.Add(tmp);
						Game1.playSound("cowboy_powerup", null);
						return true;
					}
					this.heldItem = c;
					Game1.playSound("cowboy_powerup", null);
				}
				else
				{
					this.lives++;
					Game1.playSound("cowboy_powerup", null);
				}
				break;
			}
			return true;
		}

		// Token: 0x060024EC RID: 9452 RVA: 0x00193854 File Offset: 0x00191A54
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x060024ED RID: 9453 RVA: 0x00193860 File Offset: 0x00191A60
		public void usePowerup(int which)
		{
			if (this.activePowerups.ContainsKey(which))
			{
				this.activePowerups[which] = this.powerupDuration + 2000;
				return;
			}
			switch (which)
			{
			case -3:
				AbigailGame.itemToHold = 13;
				AbigailGame.holdItemTimer = 4000;
				Game1.playSound("Cowboy_Secret", null);
				AbigailGame.endCutscene = true;
				AbigailGame.endCutsceneTimer = 4000;
				AbigailGame.world = 0;
				goto IL_8CD;
			case -2:
			case -1:
				AbigailGame.itemToHold = ((which == -1) ? 12 : 11);
				AbigailGame.holdItemTimer = 2000;
				Game1.playSound("Cowboy_Secret", null);
				AbigailGame.gopherTrain = true;
				AbigailGame.gopherTrainPosition = -AbigailGame.TileSize * 2;
				goto IL_8CD;
			case 0:
				this.coins++;
				Game1.playSound("Pickup_Coin15", null);
				goto IL_8CD;
			case 1:
				this.coins += 5;
				Game1.playSound("Pickup_Coin15", null);
				Game1.playSound("Pickup_Coin15", null);
				goto IL_8CD;
			case 2:
			case 3:
			case 7:
				this.shotTimer = 0;
				Game1.playSound("cowboy_gunload", null);
				this.activePowerups.Add(which, this.powerupDuration + 2000);
				goto IL_8CD;
			case 4:
				Game1.playSound("cowboy_explosion", null);
				if (!AbigailGame.shootoutLevel)
				{
					foreach (AbigailGame.CowboyMonster c in AbigailGame.monsters)
					{
						AbigailGame.addGuts(c.position.Location, c.type);
					}
					AbigailGame.monsters.Clear();
				}
				else
				{
					foreach (AbigailGame.CowboyMonster c2 in AbigailGame.monsters)
					{
						c2.takeDamage(30);
						this.bullets.Add(new AbigailGame.CowboyBullet(c2.position.Center, 2, 1));
					}
				}
				for (int i = 0; i < 30; i++)
				{
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2((float)Game1.random.Next(1, 16), (float)Game1.random.Next(1, 16)) * (float)AbigailGame.TileSize + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
					{
						delayBeforeAnimationStart = Game1.random.Next(800)
					});
				}
				goto IL_8CD;
			case 5:
				if (AbigailGame.overworldSong != null && AbigailGame.overworldSong.IsPlaying)
				{
					AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
				}
				if (AbigailGame.zombieSong != null && AbigailGame.zombieSong.IsPlaying)
				{
					AbigailGame.zombieSong.Stop(AudioStopOptions.Immediate);
					AbigailGame.zombieSong = null;
				}
				Game1.playSound("Cowboy_undead", out AbigailGame.zombieSong);
				this.motionPause = 1800;
				AbigailGame.zombieModeTimer = 10000;
				goto IL_8CD;
			case 8:
				this.lives++;
				Game1.playSound("cowboy_powerup", null);
				goto IL_8CD;
			case 9:
			{
				Point teleportSpot = Point.Zero;
				int tries = 0;
				while ((Math.Abs((float)teleportSpot.X - this.playerPosition.X) < 8f || Math.Abs((float)teleportSpot.Y - this.playerPosition.Y) < 8f || AbigailGame.isCollidingWithMap(teleportSpot) || AbigailGame.isCollidingWithMonster(new Rectangle(teleportSpot.X, teleportSpot.Y, AbigailGame.TileSize, AbigailGame.TileSize), null)) && tries < 10)
				{
					teleportSpot = new Point(Game1.random.Next(AbigailGame.TileSize, 16 * AbigailGame.TileSize - AbigailGame.TileSize), Game1.random.Next(AbigailGame.TileSize, 16 * AbigailGame.TileSize - AbigailGame.TileSize));
					tries++;
				}
				if (tries < 10)
				{
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, this.playerPosition + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2((float)teleportSpot.X, (float)teleportSpot.Y) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2((float)(teleportSpot.X - AbigailGame.TileSize / 2), (float)teleportSpot.Y) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
					{
						delayBeforeAnimationStart = 200
					});
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2((float)(teleportSpot.X + AbigailGame.TileSize / 2), (float)teleportSpot.Y) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
					{
						delayBeforeAnimationStart = 400
					});
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2((float)teleportSpot.X, (float)(teleportSpot.Y - AbigailGame.TileSize / 2)) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
					{
						delayBeforeAnimationStart = 600
					});
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2((float)teleportSpot.X, (float)(teleportSpot.Y + AbigailGame.TileSize / 2)) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
					{
						delayBeforeAnimationStart = 800
					});
					this.playerPosition = new Vector2((float)teleportSpot.X, (float)teleportSpot.Y);
					AbigailGame.monsterConfusionTimer = 4000;
					AbigailGame.playerInvincibleTimer = 4000;
					Game1.playSound("cowboy_powerup", null);
					goto IL_8CD;
				}
				goto IL_8CD;
			}
			case 10:
				this.usePowerup(7);
				this.usePowerup(3);
				this.usePowerup(6);
				for (int j = 0; j < this.activePowerups.Count; j++)
				{
					Dictionary<int, int> dictionary = this.activePowerups;
					int key = this.activePowerups.ElementAt(j).Key;
					dictionary[key] *= 2;
				}
				goto IL_8CD;
			}
			this.activePowerups.Add(which, this.powerupDuration);
			Game1.playSound("cowboy_powerup", null);
			IL_8CD:
			if (this.whichRound > 0 && this.activePowerups.ContainsKey(which))
			{
				Dictionary<int, int> dictionary = this.activePowerups;
				dictionary[which] /= 2;
			}
		}

		// Token: 0x060024EE RID: 9454 RVA: 0x0019418C File Offset: 0x0019238C
		public static void addGuts(Point position, int whichGuts)
		{
			switch (whichGuts)
			{
			case 0:
			case 2:
			case 5:
			case 6:
			case 7:
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 80f, 6, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)position.X, (float)position.Y), false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 1696, 16, 16), 10000f, 1, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)position.X, (float)position.Y), false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
				{
					delayBeforeAnimationStart = 480
				});
				return;
			case 1:
			case 4:
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 80f, 4, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)position.X, (float)position.Y), false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
				return;
			case 3:
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)position.X, (float)position.Y), false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
				return;
			default:
				return;
			}
		}

		// Token: 0x060024EF RID: 9455 RVA: 0x001943B8 File Offset: 0x001925B8
		public void endOfGopherAnimationBehavior2(int extraInfo)
		{
			Game1.playSound("cowboy_gopher", null);
			if (Math.Abs(AbigailGame.gopherBox.X - 8 * AbigailGame.TileSize) > Math.Abs(AbigailGame.gopherBox.Y - 8 * AbigailGame.TileSize))
			{
				if (AbigailGame.gopherBox.X > 8 * AbigailGame.TileSize)
				{
					this.gopherMotion = new Point(-2, 0);
				}
				else
				{
					this.gopherMotion = new Point(2, 0);
				}
			}
			else if (AbigailGame.gopherBox.Y > 8 * AbigailGame.TileSize)
			{
				this.gopherMotion = new Point(0, -2);
			}
			else
			{
				this.gopherMotion = new Point(0, 2);
			}
			AbigailGame.gopherRunning = true;
		}

		// Token: 0x060024F0 RID: 9456 RVA: 0x00194474 File Offset: 0x00192674
		public void endOfGopherAnimationBehavior(int extrainfo)
		{
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(384, 1792, 16, 16), 120f, 4, 2, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.gopherBox.X + AbigailGame.TileSize / 2), (float)(AbigailGame.gopherBox.Y + AbigailGame.TileSize / 2)), false, false, (float)AbigailGame.gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
			{
				endFunction = new TemporaryAnimatedSprite.endBehavior(this.endOfGopherAnimationBehavior2)
			});
			Game1.playSound("cowboy_gopher", null);
		}

		// Token: 0x060024F1 RID: 9457 RVA: 0x0019453C File Offset: 0x0019273C
		public void updateBullets(GameTime time)
		{
			for (int i = this.bullets.Count - 1; i >= 0; i--)
			{
				AbigailGame.CowboyBullet cowboyBullet = this.bullets[i];
				cowboyBullet.position.X = cowboyBullet.position.X + this.bullets[i].motion.X;
				AbigailGame.CowboyBullet cowboyBullet2 = this.bullets[i];
				cowboyBullet2.position.Y = cowboyBullet2.position.Y + this.bullets[i].motion.Y;
				if (this.bullets[i].position.X <= 0 || this.bullets[i].position.Y <= 0 || this.bullets[i].position.X >= 768 || this.bullets[i].position.Y >= 768)
				{
					this.bullets.RemoveAt(i);
				}
				else if (AbigailGame.map[this.bullets[i].position.X / 16 / 3, this.bullets[i].position.Y / 16 / 3] == 7)
				{
					this.bullets.RemoveAt(i);
				}
				else
				{
					int j = AbigailGame.monsters.Count - 1;
					while (j >= 0)
					{
						if (AbigailGame.monsters[j].position.Intersects(new Rectangle(this.bullets[i].position.X, this.bullets[i].position.Y, 12, 12)))
						{
							int monsterhealth = AbigailGame.monsters[j].health;
							int monsterAfterDamageHealth;
							if (AbigailGame.monsters[j].takeDamage(this.bullets[i].damage))
							{
								monsterAfterDamageHealth = AbigailGame.monsters[j].health;
								AbigailGame.addGuts(AbigailGame.monsters[j].position.Location, AbigailGame.monsters[j].type);
								int loot = AbigailGame.monsters[j].getLootDrop();
								if (this.whichRound == 1 && Game1.random.NextBool())
								{
									loot = -1;
								}
								if (this.whichRound > 0 && (loot == 5 || loot == 8) && Game1.random.NextDouble() < 0.4)
								{
									loot = -1;
								}
								if (loot != -1 && AbigailGame.whichWave != 12)
								{
									AbigailGame.powerups.Add(new AbigailGame.CowboyPowerup(loot, AbigailGame.monsters[j].position.Location, this.lootDuration));
								}
								if (AbigailGame.shootoutLevel)
								{
									if (AbigailGame.whichWave == 12 && AbigailGame.monsters[j].type == -2)
									{
										Game1.playSound("cowboy_explosion", null);
										AbigailGame.powerups.Add(new AbigailGame.CowboyPowerup(-3, new Point(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize), 9999999));
										this.noPickUpBox = new Rectangle(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize);
										if (AbigailGame.outlawSong != null && AbigailGame.outlawSong.IsPlaying)
										{
											AbigailGame.outlawSong.Stop(AudioStopOptions.Immediate);
										}
										AbigailGame.screenFlash = 200;
										for (int k = 0; k < 30; k++)
										{
											AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 70f, 6, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
											{
												delayBeforeAnimationStart = k * 75
											});
											if (k % 4 == 0)
											{
												AbigailGame.addGuts(new Point(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), 7);
											}
											if (k % 4 == 0)
											{
												AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
												{
													delayBeforeAnimationStart = k * 75
												});
											}
											if (k % 3 == 0)
											{
												AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 100f, 4, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
												{
													delayBeforeAnimationStart = k * 75
												});
											}
										}
									}
									else if (AbigailGame.whichWave != 12)
									{
										AbigailGame.powerups.Add(new AbigailGame.CowboyPowerup((AbigailGame.world == 0) ? -1 : -2, new Point(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize), 9999999));
										if (AbigailGame.outlawSong != null && AbigailGame.outlawSong.IsPlaying)
										{
											AbigailGame.outlawSong.Stop(AudioStopOptions.Immediate);
										}
										AbigailGame.map[8, 8] = 10;
										AbigailGame.screenFlash = 200;
										for (int l = 0; l < 15; l++)
										{
											AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
											{
												delayBeforeAnimationStart = l * 75
											});
										}
									}
								}
								AbigailGame.monsters.RemoveAt(j);
								Game1.playSound("Cowboy_monsterDie", null);
							}
							else
							{
								monsterAfterDamageHealth = AbigailGame.monsters[j].health;
							}
							this.bullets[i].damage -= monsterhealth - monsterAfterDamageHealth;
							if (this.bullets[i].damage <= 0)
							{
								this.bullets.RemoveAt(i);
								break;
							}
							break;
						}
						else
						{
							j--;
						}
					}
				}
			}
			for (int m = AbigailGame.enemyBullets.Count - 1; m >= 0; m--)
			{
				AbigailGame.CowboyBullet cowboyBullet3 = AbigailGame.enemyBullets[m];
				cowboyBullet3.position.X = cowboyBullet3.position.X + AbigailGame.enemyBullets[m].motion.X;
				AbigailGame.CowboyBullet cowboyBullet4 = AbigailGame.enemyBullets[m];
				cowboyBullet4.position.Y = cowboyBullet4.position.Y + AbigailGame.enemyBullets[m].motion.Y;
				if (AbigailGame.enemyBullets[m].position.X <= 0 || AbigailGame.enemyBullets[m].position.Y <= 0 || AbigailGame.enemyBullets[m].position.X >= 762 || AbigailGame.enemyBullets[m].position.Y >= 762)
				{
					AbigailGame.enemyBullets.RemoveAt(m);
				}
				else if (AbigailGame.map[(AbigailGame.enemyBullets[m].position.X + 6) / 16 / 3, (AbigailGame.enemyBullets[m].position.Y + 6) / 16 / 3] == 7)
				{
					AbigailGame.enemyBullets.RemoveAt(m);
				}
				else if (AbigailGame.playerInvincibleTimer <= 0 && AbigailGame.deathTimer <= 0f && this.playerBoundingBox.Intersects(new Rectangle(AbigailGame.enemyBullets[m].position.X, AbigailGame.enemyBullets[m].position.Y, 15, 15)))
				{
					this.playerDie();
					return;
				}
			}
		}

		// Token: 0x060024F2 RID: 9458 RVA: 0x00194F80 File Offset: 0x00193180
		public void playerDie()
		{
			AbigailGame.gopherRunning = false;
			AbigailGame.hasGopherAppeared = false;
			this.spawnQueue = new List<Point>[4];
			for (int i = 0; i < 4; i++)
			{
				this.spawnQueue[i] = new List<Point>();
			}
			AbigailGame.enemyBullets.Clear();
			if (!AbigailGame.shootoutLevel)
			{
				AbigailGame.powerups.Clear();
				AbigailGame.monsters.Clear();
			}
			this.died = true;
			this.activePowerups.Clear();
			AbigailGame.deathTimer = 3000f;
			if (AbigailGame.overworldSong != null && AbigailGame.overworldSong.IsPlaying)
			{
				AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
			}
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, this.playerPosition + AbigailGame.topLeftScreenCoordinate, false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
			AbigailGame.waveTimer = Math.Min(80000, AbigailGame.waveTimer + 10000);
			AbigailGame.betweenWaveTimer = 4000;
			this.lives--;
			AbigailGame.playerInvincibleTimer = 5000;
			if (AbigailGame.shootoutLevel)
			{
				this.playerPosition = new Vector2((float)(8 * AbigailGame.TileSize), (float)(3 * AbigailGame.TileSize));
				Game1.playSound("Cowboy_monsterDie", null);
			}
			else
			{
				this.playerPosition = new Vector2((float)(8 * AbigailGame.TileSize - AbigailGame.TileSize), (float)(8 * AbigailGame.TileSize));
				this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
				this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
				if (this.playerBoundingBox.Intersects(this.player2BoundingBox))
				{
					this.playerPosition.X = this.playerPosition.X - (float)(AbigailGame.TileSize * 3 / 2);
					this.player2deathtimer = (int)AbigailGame.deathTimer;
					this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
					this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
					this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
					this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
				}
				Game1.playSound("cowboy_dead", null);
			}
			if (this.lives < 0)
			{
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 550f, 5, 0, this.playerPosition + AbigailGame.topLeftScreenCoordinate, false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
				{
					alpha = 0.001f,
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.afterPlayerDeathFunction)
				});
				AbigailGame.deathTimer *= 3f;
				Game1.player.jotpkProgress.Value = null;
				return;
			}
			if (!AbigailGame.shootoutLevel)
			{
				this.SaveGame();
			}
		}

		// Token: 0x060024F3 RID: 9459 RVA: 0x001952E8 File Offset: 0x001934E8
		public void afterPlayerDeathFunction(int extra)
		{
			if (this.lives < 0)
			{
				AbigailGame.gameOver = true;
				if (AbigailGame.overworldSong != null && !AbigailGame.overworldSong.IsPlaying)
				{
					AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
				}
				if (AbigailGame.outlawSong != null && !AbigailGame.outlawSong.IsPlaying)
				{
					AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
				}
				AbigailGame.monsters.Clear();
				AbigailGame.powerups.Clear();
				this.died = false;
				Game1.playSound("Cowboy_monsterDie", null);
				if (AbigailGame.playingWithAbigail && Game1.currentLocation.currentEvent != null)
				{
					this.unload();
					Game1.currentMinigame = null;
					Event currentEvent = Game1.currentLocation.currentEvent;
					int currentCommand = currentEvent.CurrentCommand;
					currentEvent.CurrentCommand = currentCommand + 1;
				}
			}
		}

		// Token: 0x060024F4 RID: 9460 RVA: 0x001953AC File Offset: 0x001935AC
		public void startAbigailPortrait(int whichExpression, string sayWhat)
		{
			if (this.abigail == null)
			{
				return;
			}
			if (this.abigailPortraitTimer <= 0)
			{
				this.abigailPortraitTimer = 6000;
				this.AbigailDialogue = sayWhat;
				this.abigailPortraitExpression = whichExpression;
				this.abigailPortraitYposition = Game1.viewport.Height;
				Game1.playSound("dwop", null);
			}
		}

		// Token: 0x060024F5 RID: 9461 RVA: 0x00195408 File Offset: 0x00193608
		public void startNewRound()
		{
			this.gamerestartTimer = 2000;
			Game1.playSound("Cowboy_monsterDie", null);
			this.whichRound++;
		}

		// Token: 0x060024F6 RID: 9462 RVA: 0x00195444 File Offset: 0x00193644
		protected void _UpdateInput()
		{
			if (Game1.options.gamepadControls)
			{
				GamePadState pad_state = Game1.input.GetGamePadState();
				ButtonCollection button_collection = new ButtonCollection(ref pad_state);
				if ((double)pad_state.ThumbSticks.Left.X < -0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.MoveLeft);
				}
				if ((double)pad_state.ThumbSticks.Left.X > 0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.MoveRight);
				}
				if ((double)pad_state.ThumbSticks.Left.Y < -0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.MoveDown);
				}
				if ((double)pad_state.ThumbSticks.Left.Y > 0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.MoveUp);
				}
				if ((double)pad_state.ThumbSticks.Right.X < -0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.ShootLeft);
				}
				if ((double)pad_state.ThumbSticks.Right.X > 0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.ShootRight);
				}
				if ((double)pad_state.ThumbSticks.Right.Y < -0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.ShootDown);
				}
				if ((double)pad_state.ThumbSticks.Right.Y > 0.2)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.ShootUp);
				}
				foreach (Buttons button in button_collection)
				{
					if (button <= Buttons.RightShoulder)
					{
						if (button <= Buttons.Start)
						{
							switch (button)
							{
							case Buttons.DPadUp:
								this._buttonHeldState.Add(AbigailGame.GameKeys.MoveUp);
								continue;
							case Buttons.DPadDown:
								this._buttonHeldState.Add(AbigailGame.GameKeys.MoveDown);
								continue;
							case Buttons.DPadUp | Buttons.DPadDown:
								continue;
							case Buttons.DPadLeft:
								this._buttonHeldState.Add(AbigailGame.GameKeys.MoveLeft);
								continue;
							default:
								if (button == Buttons.DPadRight)
								{
									this._buttonHeldState.Add(AbigailGame.GameKeys.MoveRight);
									continue;
								}
								if (button != Buttons.Start)
								{
									continue;
								}
								break;
							}
						}
						else
						{
							if (button == Buttons.Back)
							{
								this._buttonHeldState.Add(AbigailGame.GameKeys.Exit);
								continue;
							}
							if (button != Buttons.LeftShoulder && button != Buttons.RightShoulder)
							{
								continue;
							}
						}
					}
					else if (button <= Buttons.X)
					{
						if (button != Buttons.A)
						{
							if (button != Buttons.B)
							{
								if (button != Buttons.X)
								{
									continue;
								}
								this._buttonHeldState.Add(AbigailGame.GameKeys.ShootLeft);
								continue;
							}
							else
							{
								if (AbigailGame.gameOver)
								{
									this._buttonHeldState.Add(AbigailGame.GameKeys.Exit);
									continue;
								}
								if (Program.sdk.IsEnterButtonAssignmentFlipped)
								{
									this._buttonHeldState.Add(AbigailGame.GameKeys.ShootDown);
									continue;
								}
								this._buttonHeldState.Add(AbigailGame.GameKeys.ShootRight);
								continue;
							}
						}
						else
						{
							if (AbigailGame.gameOver)
							{
								this._buttonHeldState.Add(AbigailGame.GameKeys.SelectOption);
								continue;
							}
							if (Program.sdk.IsEnterButtonAssignmentFlipped)
							{
								this._buttonHeldState.Add(AbigailGame.GameKeys.ShootRight);
								continue;
							}
							this._buttonHeldState.Add(AbigailGame.GameKeys.ShootDown);
							continue;
						}
					}
					else
					{
						if (button == Buttons.Y)
						{
							this._buttonHeldState.Add(AbigailGame.GameKeys.ShootUp);
							continue;
						}
						if (button != Buttons.RightTrigger && button != Buttons.LeftTrigger)
						{
							continue;
						}
					}
					this._buttonHeldState.Add(AbigailGame.GameKeys.UsePowerup);
				}
			}
			if (this._binds == null)
			{
				this.SetupBinds();
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.MoveUp))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.MoveUp);
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.MoveDown))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.MoveDown);
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.MoveLeft))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.MoveLeft);
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.MoveRight))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.MoveRight);
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.ShootUp))
			{
				if (AbigailGame.gameOver)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.MoveUp);
				}
				else
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.ShootUp);
				}
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.ShootDown))
			{
				if (AbigailGame.gameOver)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.MoveDown);
				}
				else
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.ShootDown);
				}
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.ShootLeft))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.ShootLeft);
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.ShootRight))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.ShootRight);
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.UsePowerup))
			{
				if (AbigailGame.gameOver)
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.SelectOption);
				}
				else
				{
					this._buttonHeldState.Add(AbigailGame.GameKeys.UsePowerup);
				}
			}
			if (this.IsBoundButtonDown(AbigailGame.GameKeys.Exit))
			{
				this._buttonHeldState.Add(AbigailGame.GameKeys.Exit);
			}
		}

		// Token: 0x060024F7 RID: 9463 RVA: 0x001958F0 File Offset: 0x00193AF0
		public virtual void SetupBinds()
		{
			this._binds = new Dictionary<AbigailGame.GameKeys, List<Keys>>();
			this._binds[AbigailGame.GameKeys.MoveUp] = new List<Keys>(new Keys[]
			{
				Keys.W
			});
			this._binds[AbigailGame.GameKeys.MoveDown] = new List<Keys>(new Keys[]
			{
				Keys.S
			});
			this._binds[AbigailGame.GameKeys.MoveLeft] = new List<Keys>(new Keys[]
			{
				Keys.A
			});
			this._binds[AbigailGame.GameKeys.MoveRight] = new List<Keys>(new Keys[]
			{
				Keys.D
			});
			this._binds[AbigailGame.GameKeys.ShootUp] = new List<Keys>(new Keys[]
			{
				Keys.Up
			});
			this._binds[AbigailGame.GameKeys.ShootDown] = new List<Keys>(new Keys[]
			{
				Keys.Down
			});
			this._binds[AbigailGame.GameKeys.ShootLeft] = new List<Keys>(new Keys[]
			{
				Keys.Left
			});
			this._binds[AbigailGame.GameKeys.ShootRight] = new List<Keys>(new Keys[]
			{
				Keys.Right
			});
			this._binds[AbigailGame.GameKeys.UsePowerup] = new List<Keys>(new Keys[]
			{
				Keys.Enter,
				Keys.Space
			});
			this._binds[AbigailGame.GameKeys.Exit] = new List<Keys>(new Keys[]
			{
				Keys.Escape
			});
			Keys key = this.GetBoundKey(Game1.options.moveUpButton);
			if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
			{
				this._binds[AbigailGame.GameKeys.MoveUp] = new List<Keys>(new Keys[]
				{
					key
				});
			}
			key = this.GetBoundKey(Game1.options.moveDownButton);
			if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
			{
				this._binds[AbigailGame.GameKeys.MoveDown] = new List<Keys>(new Keys[]
				{
					key
				});
			}
			key = this.GetBoundKey(Game1.options.moveLeftButton);
			if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
			{
				this._binds[AbigailGame.GameKeys.MoveLeft] = new List<Keys>(new Keys[]
				{
					key
				});
			}
			key = this.GetBoundKey(Game1.options.moveRightButton);
			if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
			{
				this._binds[AbigailGame.GameKeys.MoveRight] = new List<Keys>(new Keys[]
				{
					key
				});
			}
			bool x_bound = false;
			using (Dictionary<AbigailGame.GameKeys, List<Keys>>.ValueCollection.Enumerator enumerator = this._binds.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Contains(Keys.X))
					{
						x_bound = true;
						break;
					}
				}
			}
			if (!x_bound)
			{
				this._binds[AbigailGame.GameKeys.UsePowerup].Add(Keys.X);
			}
		}

		// Token: 0x060024F8 RID: 9464 RVA: 0x00195B9C File Offset: 0x00193D9C
		public Keys GetBoundKey(InputButton[] button)
		{
			if (button == null || button.Length == 0)
			{
				return Keys.None;
			}
			for (int i = 0; i < button.Length; i++)
			{
				if (button[i].key != Keys.None)
				{
					return button[i].key;
				}
			}
			return Keys.None;
		}

		// Token: 0x060024F9 RID: 9465 RVA: 0x00195BDC File Offset: 0x00193DDC
		public bool IsBoundButtonDown(AbigailGame.GameKeys game_key)
		{
			List<Keys> binds;
			if (this._binds.TryGetValue(game_key, out binds))
			{
				foreach (Keys key in binds)
				{
					if (Game1.input.GetKeyboardState().IsKeyDown(key))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060024FA RID: 9466 RVA: 0x00195C54 File Offset: 0x00193E54
		public bool tick(GameTime time)
		{
			if (this._buttonHeldFrames == null)
			{
				this._buttonHeldFrames = new Dictionary<AbigailGame.GameKeys, int>();
				for (int i = 0; i < 11; i++)
				{
					this._buttonHeldFrames[(AbigailGame.GameKeys)i] = 0;
				}
			}
			this._buttonHeldState.Clear();
			if (AbigailGame.startTimer <= 0)
			{
				this._UpdateInput();
			}
			for (int j = 0; j < 11; j++)
			{
				if (this._buttonHeldState.Contains((AbigailGame.GameKeys)j))
				{
					Dictionary<AbigailGame.GameKeys, int> buttonHeldFrames = this._buttonHeldFrames;
					AbigailGame.GameKeys key2 = (AbigailGame.GameKeys)j;
					int num = buttonHeldFrames[key2];
					buttonHeldFrames[key2] = num + 1;
				}
				else
				{
					this._buttonHeldFrames[(AbigailGame.GameKeys)j] = 0;
				}
			}
			this._ProcessInputs();
			if (this.quit)
			{
				Game1.stopMusicTrack(MusicContext.MiniGame);
				return true;
			}
			if (AbigailGame.gameOver)
			{
				AbigailGame.startTimer = 0;
				return false;
			}
			if (AbigailGame.onStartMenu)
			{
				if (AbigailGame.startTimer > 0)
				{
					AbigailGame.startTimer -= time.ElapsedGameTime.Milliseconds;
					if (AbigailGame.startTimer <= 0)
					{
						this.shotTimer = 100;
						AbigailGame.onStartMenu = false;
					}
				}
				else
				{
					Game1.playSound("Pickup_Coin15", null);
					AbigailGame.startTimer = 1500;
				}
				return false;
			}
			if (this.gamerestartTimer > 0)
			{
				this.gamerestartTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.gamerestartTimer <= 0)
				{
					this.unload();
					if (this.whichRound == 0 || !AbigailGame.endCutscene)
					{
						Game1.currentMinigame = new AbigailGame(null);
					}
					else
					{
						Game1.currentMinigame = new AbigailGame(this.coins, this.ammoLevel, this.bulletDamage, this.fireSpeedLevel, this.runSpeedLevel, this.lives, this.spreadPistol, this.whichRound);
					}
				}
			}
			if (this.fadethenQuitTimer > 0 && (float)this.abigailPortraitTimer <= 0f)
			{
				this.fadethenQuitTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.fadethenQuitTimer <= 0)
				{
					if (Game1.currentLocation.currentEvent != null)
					{
						Event currentEvent = Game1.currentLocation.currentEvent;
						int num = currentEvent.CurrentCommand;
						currentEvent.CurrentCommand = num + 1;
						if (AbigailGame.beatLevelWithAbigail)
						{
							Game1.currentLocation.currentEvent.specialEventVariable1 = true;
						}
					}
					return true;
				}
			}
			if (this.abigailPortraitTimer > 0)
			{
				this.abigailPortraitTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.abigailPortraitTimer > 1000 && this.abigailPortraitYposition > Game1.viewport.Height - 240)
				{
					this.abigailPortraitYposition -= 16;
				}
				else if (this.abigailPortraitTimer <= 1000)
				{
					this.abigailPortraitYposition += 16;
				}
			}
			if (AbigailGame.endCutscene)
			{
				AbigailGame.endCutsceneTimer -= time.ElapsedGameTime.Milliseconds;
				if (AbigailGame.endCutsceneTimer < 0)
				{
					AbigailGame.endCutscenePhase++;
					if (AbigailGame.endCutscenePhase > 5)
					{
						AbigailGame.endCutscenePhase = 5;
					}
					switch (AbigailGame.endCutscenePhase)
					{
					case 1:
						Game1.player.stats.Increment("completedPrairieKing", 1U);
						if (!this.died)
						{
							Game1.player.stats.Increment("completedPrairieKingWithoutDying", 1U);
						}
						Game1.player.AddMissedMailAndRecipes();
						Game1.player.stats.checkForMiniGameAchievements(true);
						Game1.multiplayer.globalChatInfoMessage("PrairieKing", new string[]
						{
							Game1.player.Name
						});
						AbigailGame.endCutsceneTimer = 15500;
						Game1.playSound("Cowboy_singing", null);
						AbigailGame.map = this.getMap(-1);
						break;
					case 2:
						this.playerPosition = new Vector2(0f, (float)(8 * AbigailGame.TileSize));
						AbigailGame.endCutsceneTimer = 12000;
						break;
					case 3:
						AbigailGame.endCutsceneTimer = 5000;
						break;
					case 4:
						AbigailGame.endCutsceneTimer = 1000;
						break;
					case 5:
						if (Game1.input.GetKeyboardState().GetPressedKeys().Length == 0)
						{
							Game1.input.GetGamePadState();
							if (Game1.input.GetGamePadState().Buttons.X != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.Start != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.A != ButtonState.Pressed)
							{
								break;
							}
						}
						if (this.gamerestartTimer <= 0)
						{
							this.startNewRound();
						}
						break;
					}
				}
				if (AbigailGame.endCutscenePhase == 2 && this.playerPosition.X < (float)(9 * AbigailGame.TileSize))
				{
					this.playerPosition.X = this.playerPosition.X + 1f;
					this.playerMotionAnimationTimer += (float)time.ElapsedGameTime.Milliseconds;
					this.playerMotionAnimationTimer %= 400f;
				}
				return false;
			}
			if (this.motionPause > 0)
			{
				this.motionPause -= time.ElapsedGameTime.Milliseconds;
				if (this.motionPause <= 0 && this.behaviorAfterPause != null)
				{
					this.behaviorAfterPause();
					this.behaviorAfterPause = null;
				}
			}
			else if (AbigailGame.monsterConfusionTimer > 0)
			{
				AbigailGame.monsterConfusionTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (AbigailGame.zombieModeTimer > 0)
			{
				AbigailGame.zombieModeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (AbigailGame.holdItemTimer > 0)
			{
				AbigailGame.holdItemTimer -= time.ElapsedGameTime.Milliseconds;
				return false;
			}
			if (AbigailGame.screenFlash > 0)
			{
				AbigailGame.screenFlash -= time.ElapsedGameTime.Milliseconds;
			}
			if (AbigailGame.gopherTrain)
			{
				AbigailGame.gopherTrainPosition += 3;
				if (AbigailGame.gopherTrainPosition % 30 == 0)
				{
					Game1.playSound("Cowboy_Footstep", null);
				}
				if (AbigailGame.playerJumped)
				{
					this.playerPosition.Y = this.playerPosition.Y + 3f;
				}
				if (Math.Abs(this.playerPosition.Y - (float)(AbigailGame.gopherTrainPosition - AbigailGame.TileSize)) <= 16f)
				{
					AbigailGame.playerJumped = true;
					this.playerPosition.Y = (float)(AbigailGame.gopherTrainPosition - AbigailGame.TileSize);
				}
				if (AbigailGame.gopherTrainPosition > 16 * AbigailGame.TileSize + AbigailGame.TileSize)
				{
					AbigailGame.gopherTrain = false;
					AbigailGame.playerJumped = false;
					AbigailGame.whichWave++;
					AbigailGame.map = this.getMap(AbigailGame.whichWave);
					this.playerPosition = new Vector2((float)(8 * AbigailGame.TileSize), (float)(8 * AbigailGame.TileSize));
					AbigailGame.world = ((AbigailGame.world == 0) ? 2 : 1);
					AbigailGame.waveTimer = 80000;
					AbigailGame.betweenWaveTimer = 5000;
					AbigailGame.waitingForPlayerToMoveDownAMap = false;
					AbigailGame.shootoutLevel = false;
					this.SaveGame();
				}
			}
			if ((AbigailGame.shopping || AbigailGame.merchantArriving || AbigailGame.merchantLeaving || AbigailGame.waitingForPlayerToMoveDownAMap) && AbigailGame.holdItemTimer <= 0)
			{
				int oldTimer = AbigailGame.shoppingTimer;
				AbigailGame.shoppingTimer += time.ElapsedGameTime.Milliseconds;
				AbigailGame.shoppingTimer %= 500;
				if (!AbigailGame.merchantShopOpen && AbigailGame.shopping && ((oldTimer < 250 && AbigailGame.shoppingTimer >= 250) || oldTimer > AbigailGame.shoppingTimer))
				{
					Game1.playSound("Cowboy_Footstep", null);
				}
			}
			if (AbigailGame.playerInvincibleTimer > 0)
			{
				AbigailGame.playerInvincibleTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (AbigailGame.scrollingMap)
			{
				AbigailGame.newMapPosition -= AbigailGame.TileSize / 8;
				this.playerPosition.Y = this.playerPosition.Y - (float)(AbigailGame.TileSize / 8);
				this.playerPosition.Y = this.playerPosition.Y + 3f;
				this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
				this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
				AbigailGame.playerMovementDirections = new List<int>
				{
					2
				};
				this.playerMotionAnimationTimer += (float)time.ElapsedGameTime.Milliseconds;
				this.playerMotionAnimationTimer %= 400f;
				if (AbigailGame.newMapPosition <= 0)
				{
					AbigailGame.scrollingMap = false;
					AbigailGame.map = AbigailGame.nextMap;
					AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
					AbigailGame.shopping = false;
					AbigailGame.betweenWaveTimer = 5000;
					AbigailGame.waitingForPlayerToMoveDownAMap = false;
					AbigailGame.playerMovementDirections.Clear();
					this.ApplyLevelSpecificStates();
				}
			}
			if (AbigailGame.gopherRunning)
			{
				AbigailGame.gopherBox.X = AbigailGame.gopherBox.X + this.gopherMotion.X;
				AbigailGame.gopherBox.Y = AbigailGame.gopherBox.Y + this.gopherMotion.Y;
				for (int k = AbigailGame.monsters.Count - 1; k >= 0; k--)
				{
					if (AbigailGame.gopherBox.Intersects(AbigailGame.monsters[k].position))
					{
						AbigailGame.addGuts(AbigailGame.monsters[k].position.Location, AbigailGame.monsters[k].type);
						AbigailGame.monsters.RemoveAt(k);
						Game1.playSound("Cowboy_monsterDie", null);
					}
				}
				if (AbigailGame.gopherBox.X < 0 || AbigailGame.gopherBox.Y < 0 || AbigailGame.gopherBox.X > 16 * AbigailGame.TileSize || AbigailGame.gopherBox.Y > 16 * AbigailGame.TileSize)
				{
					AbigailGame.gopherRunning = false;
				}
			}
			AbigailGame.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if (this.motionPause <= 0)
			{
				for (int l = AbigailGame.powerups.Count - 1; l >= 0; l--)
				{
					if (Utility.distance((float)this.playerBoundingBox.Center.X, (float)(AbigailGame.powerups[l].position.X + AbigailGame.TileSize / 2), (float)this.playerBoundingBox.Center.Y, (float)(AbigailGame.powerups[l].position.Y + AbigailGame.TileSize / 2)) <= (float)(AbigailGame.TileSize + 3) && (AbigailGame.powerups[l].position.X < AbigailGame.TileSize || AbigailGame.powerups[l].position.X >= 16 * AbigailGame.TileSize - AbigailGame.TileSize || AbigailGame.powerups[l].position.Y < AbigailGame.TileSize || AbigailGame.powerups[l].position.Y >= 16 * AbigailGame.TileSize - AbigailGame.TileSize))
					{
						if (AbigailGame.powerups[l].position.X + AbigailGame.TileSize / 2 < this.playerBoundingBox.Center.X)
						{
							AbigailGame.CowboyPowerup cowboyPowerup = AbigailGame.powerups[l];
							cowboyPowerup.position.X = cowboyPowerup.position.X + 1;
						}
						if (AbigailGame.powerups[l].position.X + AbigailGame.TileSize / 2 > this.playerBoundingBox.Center.X)
						{
							AbigailGame.CowboyPowerup cowboyPowerup2 = AbigailGame.powerups[l];
							cowboyPowerup2.position.X = cowboyPowerup2.position.X - 1;
						}
						if (AbigailGame.powerups[l].position.Y + AbigailGame.TileSize / 2 < this.playerBoundingBox.Center.Y)
						{
							AbigailGame.CowboyPowerup cowboyPowerup3 = AbigailGame.powerups[l];
							cowboyPowerup3.position.Y = cowboyPowerup3.position.Y + 1;
						}
						if (AbigailGame.powerups[l].position.Y + AbigailGame.TileSize / 2 > this.playerBoundingBox.Center.Y)
						{
							AbigailGame.CowboyPowerup cowboyPowerup4 = AbigailGame.powerups[l];
							cowboyPowerup4.position.Y = cowboyPowerup4.position.Y - 1;
						}
					}
					AbigailGame.powerups[l].duration -= time.ElapsedGameTime.Milliseconds;
					if (AbigailGame.powerups[l].duration <= 0)
					{
						AbigailGame.powerups.RemoveAt(l);
					}
				}
				for (int m = this.activePowerups.Count - 1; m >= 0; m--)
				{
					int key = this.activePowerups.ElementAt(m).Key;
					Dictionary<int, int> dictionary = this.activePowerups;
					int num = key;
					dictionary[num] -= time.ElapsedGameTime.Milliseconds;
					if (this.activePowerups[key] <= 0)
					{
						this.activePowerups.Remove(key);
					}
				}
				if (AbigailGame.deathTimer <= 0f && AbigailGame.playerMovementDirections.Count > 0 && !AbigailGame.scrollingMap)
				{
					int effectiveDirections = AbigailGame.playerMovementDirections.Count;
					if (effectiveDirections >= 2 && AbigailGame.playerMovementDirections.Last<int>() == (AbigailGame.playerMovementDirections.ElementAt(AbigailGame.playerMovementDirections.Count - 2) + 2) % 4)
					{
						effectiveDirections = 1;
					}
					float speed = this.getMovementSpeed(3f, effectiveDirections);
					if (this.activePowerups.Keys.Contains(6))
					{
						speed *= 1.5f;
					}
					if (AbigailGame.zombieModeTimer > 0)
					{
						speed *= 1.5f;
					}
					for (int n = 0; n < this.runSpeedLevel; n++)
					{
						speed *= 1.25f;
					}
					for (int i2 = Math.Max(0, AbigailGame.playerMovementDirections.Count - 2); i2 < AbigailGame.playerMovementDirections.Count; i2++)
					{
						if (i2 != 0 || AbigailGame.playerMovementDirections.Count < 2 || AbigailGame.playerMovementDirections.Last<int>() != (AbigailGame.playerMovementDirections.ElementAt(AbigailGame.playerMovementDirections.Count - 2) + 2) % 4)
						{
							Vector2 newPlayerPosition = this.playerPosition;
							switch (AbigailGame.playerMovementDirections.ElementAt(i2))
							{
							case 0:
								newPlayerPosition.Y -= speed;
								break;
							case 1:
								newPlayerPosition.X += speed;
								break;
							case 2:
								newPlayerPosition.Y += speed;
								break;
							case 3:
								newPlayerPosition.X -= speed;
								break;
							}
							Rectangle newPlayerBox = new Rectangle((int)newPlayerPosition.X + AbigailGame.TileSize / 4, (int)newPlayerPosition.Y + AbigailGame.TileSize / 4, AbigailGame.TileSize / 2, AbigailGame.TileSize / 2);
							if (!AbigailGame.isCollidingWithMap(newPlayerBox) && (!this.merchantBox.Intersects(newPlayerBox) || this.merchantBox.Intersects(this.playerBoundingBox)) && (!AbigailGame.playingWithAbigail || !newPlayerBox.Intersects(this.player2BoundingBox)))
							{
								this.playerPosition = newPlayerPosition;
							}
						}
					}
					this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
					this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
					this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
					this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
					this.playerMotionAnimationTimer += (float)time.ElapsedGameTime.Milliseconds;
					this.playerMotionAnimationTimer %= 400f;
					this.playerFootstepSoundTimer -= (float)time.ElapsedGameTime.Milliseconds;
					if (this.playerFootstepSoundTimer <= 0f)
					{
						Game1.playSound("Cowboy_Footstep", null);
						this.playerFootstepSoundTimer = 200f;
					}
					AbigailGame.powerups.RemoveAll(delegate(AbigailGame.CowboyPowerup item)
					{
						if (this.playerBoundingBox.Intersects(new Rectangle(item.position.X, item.position.Y, AbigailGame.TileSize, AbigailGame.TileSize)) && !this.playerBoundingBox.Intersects(this.noPickUpBox))
						{
							if (this.heldItem != null)
							{
								this.usePowerup(item.which);
								return true;
							}
							if (this.getPowerUp(item))
							{
								return true;
							}
						}
						return false;
					});
					if (!this.playerBoundingBox.Intersects(this.noPickUpBox))
					{
						this.noPickUpBox.Location = new Point(0, 0);
					}
					if (AbigailGame.waitingForPlayerToMoveDownAMap && this.playerBoundingBox.Bottom >= 16 * AbigailGame.TileSize - AbigailGame.TileSize / 2)
					{
						this.SaveGame();
						AbigailGame.shopping = false;
						AbigailGame.merchantArriving = false;
						AbigailGame.merchantLeaving = false;
						AbigailGame.merchantShopOpen = false;
						this.merchantBox.Y = -AbigailGame.TileSize;
						AbigailGame.scrollingMap = true;
						AbigailGame.nextMap = this.getMap(AbigailGame.whichWave);
						AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
						AbigailGame.temporarySprites.Clear();
						AbigailGame.powerups.Clear();
					}
					if (!this.shoppingCarpetNoPickup.Intersects(this.playerBoundingBox))
					{
						this.shoppingCarpetNoPickup.X = -1000;
					}
				}
				if (AbigailGame.shopping)
				{
					if (this.merchantBox.Y < 8 * AbigailGame.TileSize - AbigailGame.TileSize * 3 && AbigailGame.merchantArriving)
					{
						this.merchantBox.Y = this.merchantBox.Y + 2;
						if (this.merchantBox.Y >= 8 * AbigailGame.TileSize - AbigailGame.TileSize * 3)
						{
							AbigailGame.merchantShopOpen = true;
							Game1.playSound("cowboy_monsterhit", null);
							AbigailGame.map[8, 15] = 3;
							AbigailGame.map[7, 15] = 3;
							AbigailGame.map[7, 15] = 3;
							AbigailGame.map[8, 14] = 3;
							AbigailGame.map[7, 14] = 3;
							AbigailGame.map[7, 14] = 3;
							this.shoppingCarpetNoPickup = new Rectangle(this.merchantBox.X - AbigailGame.TileSize, this.merchantBox.Y + AbigailGame.TileSize, AbigailGame.TileSize * 3, AbigailGame.TileSize * 2);
						}
					}
					else if (AbigailGame.merchantLeaving)
					{
						this.merchantBox.Y = this.merchantBox.Y - 2;
						if (this.merchantBox.Y <= -AbigailGame.TileSize)
						{
							AbigailGame.shopping = false;
							AbigailGame.merchantLeaving = false;
							AbigailGame.merchantArriving = true;
						}
					}
					else if (AbigailGame.merchantShopOpen)
					{
						for (int i3 = this.storeItems.Count - 1; i3 >= 0; i3--)
						{
							KeyValuePair<Rectangle, int> pair = this.storeItems.ElementAt(i3);
							if (!this.playerBoundingBox.Intersects(this.shoppingCarpetNoPickup) && this.playerBoundingBox.Intersects(pair.Key) && this.coins >= this.getPriceForItem(pair.Value))
							{
								Game1.playSound("Cowboy_Secret", null);
								AbigailGame.holdItemTimer = 2500;
								this.motionPause = 2500;
								AbigailGame.itemToHold = pair.Value;
								this.storeItems.Remove(pair.Key);
								AbigailGame.merchantLeaving = true;
								AbigailGame.merchantArriving = false;
								AbigailGame.merchantShopOpen = false;
								this.coins -= this.getPriceForItem(AbigailGame.itemToHold);
								switch (AbigailGame.itemToHold)
								{
								case 0:
								case 1:
								case 2:
									this.fireSpeedLevel++;
									break;
								case 3:
								case 4:
									this.runSpeedLevel++;
									break;
								case 5:
									this.lives++;
									break;
								case 6:
								case 7:
								case 8:
									this.ammoLevel++;
									this.bulletDamage++;
									break;
								case 9:
									this.spreadPistol = true;
									break;
								case 10:
									this.heldItem = new AbigailGame.CowboyPowerup(10, Point.Zero, 9999);
									break;
								}
							}
						}
					}
				}
				this.cactusDanceTimer += (float)time.ElapsedGameTime.Milliseconds;
				this.cactusDanceTimer %= 1600f;
				if (this.shotTimer > 0)
				{
					this.shotTimer -= time.ElapsedGameTime.Milliseconds;
				}
				if (AbigailGame.deathTimer <= 0f && AbigailGame.playerShootingDirections.Count > 0 && this.shotTimer <= 0)
				{
					if (this.activePowerups.ContainsKey(2))
					{
						this.spawnBullets(new int[1], this.playerPosition);
						this.spawnBullets(new int[]
						{
							1
						}, this.playerPosition);
						this.spawnBullets(new int[]
						{
							2
						}, this.playerPosition);
						this.spawnBullets(new int[]
						{
							3
						}, this.playerPosition);
						this.spawnBullets(new int[]
						{
							0,
							1
						}, this.playerPosition);
						this.spawnBullets(new int[]
						{
							1,
							2
						}, this.playerPosition);
						this.spawnBullets(new int[]
						{
							2,
							3
						}, this.playerPosition);
						int[] array = new int[2];
						array[0] = 3;
						this.spawnBullets(array, this.playerPosition);
					}
					else if (AbigailGame.playerShootingDirections.Count == 1 || AbigailGame.playerShootingDirections.Last<int>() == (AbigailGame.playerShootingDirections.ElementAt(AbigailGame.playerShootingDirections.Count - 2) + 2) % 4)
					{
						this.spawnBullets(new int[]
						{
							(AbigailGame.playerShootingDirections.Count == 2 && AbigailGame.playerShootingDirections.Last<int>() == (AbigailGame.playerShootingDirections.ElementAt(AbigailGame.playerShootingDirections.Count - 2) + 2) % 4) ? AbigailGame.playerShootingDirections.ElementAt(1) : AbigailGame.playerShootingDirections.ElementAt(0)
						}, this.playerPosition);
					}
					else
					{
						this.spawnBullets(AbigailGame.playerShootingDirections, this.playerPosition);
					}
					Game1.playSound("Cowboy_gunshot", null);
					this.shotTimer = this.shootingDelay;
					if (this.activePowerups.ContainsKey(3))
					{
						this.shotTimer /= 4;
					}
					for (int i4 = 0; i4 < this.fireSpeedLevel; i4++)
					{
						this.shotTimer = this.shotTimer * 3 / 4;
					}
					if (this.activePowerups.ContainsKey(7))
					{
						this.shotTimer = this.shotTimer * 3 / 2;
					}
					this.shotTimer = Math.Max(this.shotTimer, 20);
				}
				this.updateBullets(time);
				foreach (AbigailGame.CowboyPowerup powerup in AbigailGame.powerups)
				{
					Vector2 tile_position = new Vector2((float)((powerup.position.X + AbigailGame.TileSize / 2) / AbigailGame.TileSize), (float)((powerup.position.Y + AbigailGame.TileSize / 2) / AbigailGame.TileSize));
					Vector2 corner_ = new Vector2((float)(powerup.position.X / AbigailGame.TileSize), (float)(powerup.position.Y / AbigailGame.TileSize));
					Vector2 corner_2 = new Vector2((float)((powerup.position.X + AbigailGame.TileSize) / AbigailGame.TileSize), (float)(powerup.position.Y / AbigailGame.TileSize));
					Vector2 corner_3 = new Vector2((float)(powerup.position.X / AbigailGame.TileSize), (float)(powerup.position.Y / AbigailGame.TileSize));
					Vector2 corner_4 = new Vector2((float)(powerup.position.X / AbigailGame.TileSize), (float)((powerup.position.Y + 64) / AbigailGame.TileSize));
					if (this._borderTiles.Contains(tile_position) || this._borderTiles.Contains(corner_) || this._borderTiles.Contains(corner_2) || this._borderTiles.Contains(corner_3) || this._borderTiles.Contains(corner_4))
					{
						Point push_direction = default(Point);
						if (Math.Abs(tile_position.X - 8f) > Math.Abs(tile_position.Y - 8f))
						{
							push_direction.X = Math.Sign(tile_position.X - 8f);
						}
						else
						{
							push_direction.Y = Math.Sign(tile_position.Y - 8f);
						}
						AbigailGame.CowboyPowerup cowboyPowerup5 = powerup;
						cowboyPowerup5.position.X = cowboyPowerup5.position.X - push_direction.X;
						AbigailGame.CowboyPowerup cowboyPowerup6 = powerup;
						cowboyPowerup6.position.Y = cowboyPowerup6.position.Y - push_direction.Y;
					}
				}
				if (AbigailGame.waveTimer > 0 && AbigailGame.betweenWaveTimer <= 0 && AbigailGame.zombieModeTimer <= 0 && !AbigailGame.shootoutLevel && (AbigailGame.overworldSong == null || !AbigailGame.overworldSong.IsPlaying))
				{
					Game1.playSound("Cowboy_OVERWORLD", out AbigailGame.overworldSong);
					Game1.musicPlayerVolume = Game1.options.musicVolumeLevel;
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
				}
				if (AbigailGame.deathTimer > 0f)
				{
					AbigailGame.deathTimer -= (float)time.ElapsedGameTime.Milliseconds;
				}
				if (AbigailGame.betweenWaveTimer > 0 && AbigailGame.monsters.Count == 0 && this.isSpawnQueueEmpty() && !AbigailGame.shopping && !AbigailGame.waitingForPlayerToMoveDownAMap)
				{
					AbigailGame.betweenWaveTimer -= time.ElapsedGameTime.Milliseconds;
					if (AbigailGame.betweenWaveTimer <= 0 && AbigailGame.playingWithAbigail)
					{
						this.startAbigailPortrait(7, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11896"));
					}
				}
				else if (AbigailGame.deathTimer <= 0f && !AbigailGame.waitingForPlayerToMoveDownAMap && !AbigailGame.shopping && !AbigailGame.shootoutLevel)
				{
					if (AbigailGame.waveTimer > 0)
					{
						int oldWaveTimer = AbigailGame.waveTimer;
						AbigailGame.waveTimer -= time.ElapsedGameTime.Milliseconds;
						if (AbigailGame.playingWithAbigail && oldWaveTimer > 40000 && AbigailGame.waveTimer <= 40000)
						{
							this.startAbigailPortrait(0, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11897"));
						}
						int u = 0;
						foreach (Vector2 v in this.monsterChances)
						{
							if (Game1.random.NextDouble() < (double)(v.X * (float)((AbigailGame.monsters.Count == 0) ? 2 : 1)))
							{
								int numMonsters = 1;
								while (Game1.random.NextDouble() < (double)v.Y && numMonsters < 15)
								{
									numMonsters++;
								}
								this.spawnQueue[(AbigailGame.whichWave == 11) ? (Game1.random.Next(1, 3) * 2 - 1) : Game1.random.Next(4)].Add(new Point(u, numMonsters));
							}
							u++;
						}
						if (!AbigailGame.hasGopherAppeared && AbigailGame.monsters.Count > 6 && Game1.random.NextDouble() < 0.0004 && AbigailGame.waveTimer > 7000 && AbigailGame.waveTimer < 50000)
						{
							AbigailGame.hasGopherAppeared = true;
							AbigailGame.gopherBox = new Rectangle(Game1.random.Next(16 * AbigailGame.TileSize), Game1.random.Next(16 * AbigailGame.TileSize), AbigailGame.TileSize, AbigailGame.TileSize);
							int tries = 0;
							while ((AbigailGame.isCollidingWithMap(AbigailGame.gopherBox) || AbigailGame.isCollidingWithMonster(AbigailGame.gopherBox, null) || Math.Abs((float)AbigailGame.gopherBox.X - this.playerPosition.X) < (float)(AbigailGame.TileSize * 6) || Math.Abs((float)AbigailGame.gopherBox.Y - this.playerPosition.Y) < (float)(AbigailGame.TileSize * 6) || Math.Abs(AbigailGame.gopherBox.X - 8 * AbigailGame.TileSize) < AbigailGame.TileSize * 4 || Math.Abs(AbigailGame.gopherBox.Y - 8 * AbigailGame.TileSize) < AbigailGame.TileSize * 4) && tries < 10)
							{
								AbigailGame.gopherBox.X = Game1.random.Next(16 * AbigailGame.TileSize);
								AbigailGame.gopherBox.Y = Game1.random.Next(16 * AbigailGame.TileSize);
								tries++;
							}
							if (tries < 10)
							{
								AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(256, 1664, 16, 32), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.gopherBox.X + AbigailGame.TileSize / 2), (float)(AbigailGame.gopherBox.Y - AbigailGame.TileSize + AbigailGame.TileSize / 2)), false, false, (float)AbigailGame.gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
								{
									endFunction = new TemporaryAnimatedSprite.endBehavior(this.endOfGopherAnimationBehavior)
								});
							}
						}
					}
					for (int p = 0; p < 4; p++)
					{
						if (this.spawnQueue[p].Count > 0)
						{
							if (this.spawnQueue[p][0].X == 1 || this.spawnQueue[p][0].X == 4)
							{
								List<Vector2> border = Utility.getBorderOfThisRectangle(new Rectangle(0, 0, 16, 16));
								Vector2 tile = border.ElementAt(Game1.random.Next(border.Count));
								int tries2 = 0;
								while (AbigailGame.isCollidingWithMonster(new Rectangle((int)tile.X * AbigailGame.TileSize, (int)tile.Y * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize), null) && tries2 < 10)
								{
									tile = border.ElementAt(Game1.random.Next(border.Count));
									tries2++;
								}
								if (tries2 < 10)
								{
									AbigailGame.CowboyMonster monster = new AbigailGame.CowboyMonster(this.spawnQueue[p][0].X, new Point((int)tile.X * AbigailGame.TileSize, (int)tile.Y * AbigailGame.TileSize));
									if (this.whichRound > 0)
									{
										monster.health += this.whichRound * 2;
									}
									AbigailGame.monsters.Add(monster);
									this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
									if (this.spawnQueue[p][0].Y <= 0)
									{
										this.spawnQueue[p].RemoveAt(0);
									}
								}
							}
							else
							{
								switch (p)
								{
								case 0:
								{
									int x = 7;
									while (x < 10)
									{
										if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(x * 16 * 3, 0, 48, 48), null))
										{
											AbigailGame.CowboyMonster monster2 = new AbigailGame.CowboyMonster(this.spawnQueue[p][0].X, new Point(x * AbigailGame.TileSize, 0));
											if (this.whichRound > 0)
											{
												monster2.health += this.whichRound * 2;
											}
											AbigailGame.monsters.Add(monster2);
											this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
											if (this.spawnQueue[p][0].Y <= 0)
											{
												this.spawnQueue[p].RemoveAt(0);
												break;
											}
											break;
										}
										else
										{
											x++;
										}
									}
									break;
								}
								case 1:
								{
									int y = 7;
									while (y < 10)
									{
										if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(720, y * AbigailGame.TileSize, 48, 48), null))
										{
											AbigailGame.CowboyMonster monster3 = new AbigailGame.CowboyMonster(this.spawnQueue[p][0].X, new Point(15 * AbigailGame.TileSize, y * AbigailGame.TileSize));
											if (this.whichRound > 0)
											{
												monster3.health += this.whichRound * 2;
											}
											AbigailGame.monsters.Add(monster3);
											this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
											if (this.spawnQueue[p][0].Y <= 0)
											{
												this.spawnQueue[p].RemoveAt(0);
												break;
											}
											break;
										}
										else
										{
											y++;
										}
									}
									break;
								}
								case 2:
								{
									int x2 = 7;
									while (x2 < 10)
									{
										if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(x2 * 16 * 3, 15 * AbigailGame.TileSize, 48, 48), null))
										{
											AbigailGame.CowboyMonster monster4 = new AbigailGame.CowboyMonster(this.spawnQueue[p][0].X, new Point(x2 * AbigailGame.TileSize, 15 * AbigailGame.TileSize));
											if (this.whichRound > 0)
											{
												monster4.health += this.whichRound * 2;
											}
											AbigailGame.monsters.Add(monster4);
											this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
											if (this.spawnQueue[p][0].Y <= 0)
											{
												this.spawnQueue[p].RemoveAt(0);
												break;
											}
											break;
										}
										else
										{
											x2++;
										}
									}
									break;
								}
								case 3:
								{
									int y2 = 7;
									while (y2 < 10)
									{
										if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(0, y2 * AbigailGame.TileSize, 48, 48), null))
										{
											AbigailGame.CowboyMonster monster5 = new AbigailGame.CowboyMonster(this.spawnQueue[p][0].X, new Point(0, y2 * AbigailGame.TileSize));
											if (this.whichRound > 0)
											{
												monster5.health += this.whichRound * 2;
											}
											AbigailGame.monsters.Add(monster5);
											this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
											if (this.spawnQueue[p][0].Y <= 0)
											{
												this.spawnQueue[p].RemoveAt(0);
												break;
											}
											break;
										}
										else
										{
											y2++;
										}
									}
									break;
								}
								}
							}
						}
					}
					if (AbigailGame.waveTimer <= 0 && AbigailGame.monsters.Count > 0 && this.isSpawnQueueEmpty())
					{
						bool onlySpikeys = true;
						using (List<AbigailGame.CowboyMonster>.Enumerator enumerator3 = AbigailGame.monsters.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								if (enumerator3.Current.type != 6)
								{
									onlySpikeys = false;
									break;
								}
							}
						}
						if (onlySpikeys)
						{
							foreach (AbigailGame.CowboyMonster cowboyMonster in AbigailGame.monsters)
							{
								cowboyMonster.health = 1;
							}
						}
					}
					if (AbigailGame.waveTimer <= 0 && AbigailGame.monsters.Count == 0 && this.isSpawnQueueEmpty())
					{
						AbigailGame.hasGopherAppeared = false;
						if (AbigailGame.playingWithAbigail)
						{
							this.startAbigailPortrait(1, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11898"));
						}
						AbigailGame.waveTimer = 80000;
						AbigailGame.betweenWaveTimer = 3333;
						AbigailGame.whichWave++;
						if (AbigailGame.playingWithAbigail)
						{
							AbigailGame.beatLevelWithAbigail = true;
							this.fadethenQuitTimer = 2000;
						}
						switch (AbigailGame.whichWave)
						{
						case 1:
						case 2:
						case 3:
							this.monsterChances[0] = new Vector2(this.monsterChances[0].X + 0.001f, this.monsterChances[0].Y + 0.02f);
							if (AbigailGame.whichWave > 1)
							{
								this.monsterChances[2] = new Vector2(this.monsterChances[2].X + 0.001f, this.monsterChances[2].Y + 0.01f);
							}
							this.monsterChances[6] = new Vector2(this.monsterChances[6].X + 0.001f, this.monsterChances[6].Y + 0.01f);
							if (this.whichRound > 0)
							{
								this.monsterChances[4] = new Vector2(0.002f, 0.1f);
							}
							break;
						case 4:
						case 5:
						case 6:
						case 7:
							if (this.monsterChances[5].Equals(Vector2.Zero))
							{
								this.monsterChances[5] = new Vector2(0.01f, 0.15f);
								if (this.whichRound > 0)
								{
									this.monsterChances[5] = new Vector2(0.01f + (float)this.whichRound * 0.004f, 0.15f + (float)this.whichRound * 0.04f);
								}
							}
							this.monsterChances[0] = Vector2.Zero;
							this.monsterChances[6] = Vector2.Zero;
							this.monsterChances[2] = new Vector2(this.monsterChances[2].X + 0.002f, this.monsterChances[2].Y + 0.02f);
							this.monsterChances[5] = new Vector2(this.monsterChances[5].X + 0.001f, this.monsterChances[5].Y + 0.02f);
							this.monsterChances[1] = new Vector2(this.monsterChances[1].X + 0.0018f, this.monsterChances[1].Y + 0.08f);
							if (this.whichRound > 0)
							{
								this.monsterChances[4] = new Vector2(0.001f, 0.1f);
							}
							break;
						case 8:
						case 9:
						case 10:
						case 11:
							this.monsterChances[5] = Vector2.Zero;
							this.monsterChances[1] = Vector2.Zero;
							this.monsterChances[2] = Vector2.Zero;
							if (this.monsterChances[3].Equals(Vector2.Zero))
							{
								this.monsterChances[3] = new Vector2(0.012f, 0.4f);
								if (this.whichRound > 0)
								{
									this.monsterChances[3] = new Vector2(0.012f + (float)this.whichRound * 0.005f, 0.4f + (float)this.whichRound * 0.075f);
								}
							}
							if (this.monsterChances[4].Equals(Vector2.Zero))
							{
								this.monsterChances[4] = new Vector2(0.003f, 0.1f);
							}
							this.monsterChances[3] = new Vector2(this.monsterChances[3].X + 0.002f, this.monsterChances[3].Y + 0.05f);
							this.monsterChances[4] = new Vector2(this.monsterChances[4].X + 0.0015f, this.monsterChances[4].Y + 0.04f);
							if (AbigailGame.whichWave == 11)
							{
								this.monsterChances[4] = new Vector2(this.monsterChances[4].X + 0.01f, this.monsterChances[4].Y + 0.04f);
								this.monsterChances[3] = new Vector2(this.monsterChances[3].X - 0.01f, this.monsterChances[3].Y + 0.04f);
							}
							break;
						}
						if (this.whichRound > 0)
						{
							for (int i5 = 0; i5 < this.monsterChances.Count; i5++)
							{
								Vector2 vector = this.monsterChances[i5];
								List<Vector2> list = this.monsterChances;
								int num = i5;
								list[num] *= 1.1f;
							}
						}
						if (AbigailGame.whichWave > 0 && AbigailGame.whichWave % 2 == 0)
						{
							this.startShoppingLevel();
						}
						else if (AbigailGame.whichWave > 0)
						{
							AbigailGame.waitingForPlayerToMoveDownAMap = true;
							if (!AbigailGame.playingWithAbigail)
							{
								AbigailGame.map[8, 15] = 3;
								AbigailGame.map[7, 15] = 3;
								AbigailGame.map[9, 15] = 3;
							}
						}
					}
				}
				if (AbigailGame.playingWithAbigail)
				{
					this.updateAbigail(time);
				}
				for (int i6 = AbigailGame.monsters.Count - 1; i6 >= 0; i6--)
				{
					AbigailGame.monsters[i6].move(this.playerPosition, time);
					if (i6 < AbigailGame.monsters.Count && AbigailGame.monsters[i6].position.Intersects(this.playerBoundingBox) && AbigailGame.playerInvincibleTimer <= 0)
					{
						if (AbigailGame.zombieModeTimer <= 0)
						{
							this.playerDie();
							break;
						}
						if (AbigailGame.monsters[i6].type != -2)
						{
							AbigailGame.addGuts(AbigailGame.monsters[i6].position.Location, AbigailGame.monsters[i6].type);
							AbigailGame.monsters.RemoveAt(i6);
							Game1.playSound("Cowboy_monsterDie", null);
						}
					}
					if (AbigailGame.playingWithAbigail && i6 < AbigailGame.monsters.Count && AbigailGame.monsters[i6].position.Intersects(this.player2BoundingBox) && this.player2invincibletimer <= 0)
					{
						Game1.playSound("Cowboy_monsterDie", null);
						this.player2deathtimer = 3000;
						AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, AbigailGame.player2Position + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true));
						this.player2invincibletimer = 4000;
						AbigailGame.player2Position = new Vector2(8f, 8f) * (float)AbigailGame.TileSize;
						this.player2BoundingBox.X = (int)AbigailGame.player2Position.X + AbigailGame.TileSize / 4;
						this.player2BoundingBox.Y = (int)AbigailGame.player2Position.Y + AbigailGame.TileSize / 4;
						this.player2BoundingBox.Width = AbigailGame.TileSize / 2;
						this.player2BoundingBox.Height = AbigailGame.TileSize / 2;
						if (this.playerBoundingBox.Intersects(this.player2BoundingBox))
						{
							AbigailGame.player2Position.X = (float)(this.playerBoundingBox.Right + 2);
						}
						this.player2BoundingBox.X = (int)AbigailGame.player2Position.X + AbigailGame.TileSize / 4;
						this.player2BoundingBox.Y = (int)AbigailGame.player2Position.Y + AbigailGame.TileSize / 4;
						this.player2BoundingBox.Width = AbigailGame.TileSize / 2;
						this.player2BoundingBox.Height = AbigailGame.TileSize / 2;
						this.startAbigailPortrait(5, Game1.random.NextBool() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11901") : Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11902"));
					}
				}
			}
			return false;
		}

		// Token: 0x060024FB RID: 9467 RVA: 0x001989C8 File Offset: 0x00196BC8
		protected void _ProcessInputs()
		{
			if (this._buttonHeldFrames[AbigailGame.GameKeys.MoveUp] > 0)
			{
				if (this._buttonHeldFrames[AbigailGame.GameKeys.MoveUp] == 1 && AbigailGame.gameOver)
				{
					this.gameOverOption = Math.Max(0, this.gameOverOption - 1);
					Game1.playSound("Cowboy_gunshot", null);
				}
				this.addPlayerMovementDirection(0);
			}
			else if (AbigailGame.playerMovementDirections.Contains(0))
			{
				AbigailGame.playerMovementDirections.Remove(0);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.MoveDown] > 0)
			{
				if (this._buttonHeldFrames[AbigailGame.GameKeys.MoveDown] == 1 && AbigailGame.gameOver)
				{
					this.gameOverOption = Math.Min(1, this.gameOverOption + 1);
					Game1.playSound("Cowboy_gunshot", null);
				}
				this.addPlayerMovementDirection(2);
			}
			else if (AbigailGame.playerMovementDirections.Contains(2))
			{
				AbigailGame.playerMovementDirections.Remove(2);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.MoveLeft] > 0)
			{
				this.addPlayerMovementDirection(3);
			}
			else if (AbigailGame.playerMovementDirections.Contains(3))
			{
				AbigailGame.playerMovementDirections.Remove(3);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.MoveRight] > 0)
			{
				this.addPlayerMovementDirection(1);
			}
			else if (AbigailGame.playerMovementDirections.Contains(1))
			{
				AbigailGame.playerMovementDirections.Remove(1);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.ShootUp] > 0)
			{
				this.addPlayerShootingDirection(0);
			}
			else if (AbigailGame.playerShootingDirections.Contains(0))
			{
				AbigailGame.playerShootingDirections.Remove(0);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.ShootDown] > 0)
			{
				this.addPlayerShootingDirection(2);
			}
			else if (AbigailGame.playerShootingDirections.Contains(2))
			{
				AbigailGame.playerShootingDirections.Remove(2);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.ShootLeft] > 0)
			{
				this.addPlayerShootingDirection(3);
			}
			else if (AbigailGame.playerShootingDirections.Contains(3))
			{
				AbigailGame.playerShootingDirections.Remove(3);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.ShootRight] > 0)
			{
				this.addPlayerShootingDirection(1);
			}
			else if (AbigailGame.playerShootingDirections.Contains(1))
			{
				AbigailGame.playerShootingDirections.Remove(1);
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.SelectOption] == 1 && AbigailGame.gameOver)
			{
				if (this.gameOverOption == 1)
				{
					this.quit = true;
				}
				else
				{
					this.gamerestartTimer = 1500;
					AbigailGame.gameOver = false;
					this.gameOverOption = 0;
					Game1.playSound("Pickup_Coin15", null);
				}
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.UsePowerup] == 1 && !AbigailGame.gameOver && this.heldItem != null && AbigailGame.deathTimer <= 0f && AbigailGame.zombieModeTimer <= 0)
			{
				this.usePowerup(this.heldItem.which);
				this.heldItem = null;
			}
			if (this._buttonHeldFrames[AbigailGame.GameKeys.Exit] == 1 && !AbigailGame.playingWithAbigail)
			{
				this.quit = true;
			}
		}

		// Token: 0x060024FC RID: 9468 RVA: 0x00198C98 File Offset: 0x00196E98
		public virtual void ApplyLevelSpecificStates()
		{
			if (AbigailGame.whichWave == 12)
			{
				AbigailGame.shootoutLevel = true;
				AbigailGame.Dracula monster = new AbigailGame.Dracula();
				if (this.whichRound > 0)
				{
					monster.health *= 2;
				}
				AbigailGame.monsters.Add(monster);
				return;
			}
			if (AbigailGame.whichWave > 0 && AbigailGame.whichWave % 4 == 0)
			{
				AbigailGame.shootoutLevel = true;
				AbigailGame.monsters.Add(new AbigailGame.Outlaw(new Point(8 * AbigailGame.TileSize, 13 * AbigailGame.TileSize), (AbigailGame.world == 0) ? 50 : 100));
				Game1.playSound("cowboy_outlawsong", out AbigailGame.outlawSong);
			}
		}

		// Token: 0x060024FD RID: 9469 RVA: 0x00198D34 File Offset: 0x00196F34
		public void updateAbigail(GameTime time)
		{
			this.player2TargetUpdateTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.player2deathtimer > 0)
			{
				this.player2deathtimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (this.player2invincibletimer > 0)
			{
				this.player2invincibletimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (this.player2deathtimer <= 0)
			{
				if (this.player2TargetUpdateTimer < 0)
				{
					this.player2TargetUpdateTimer = 500;
					AbigailGame.CowboyMonster closest = null;
					double closestDistance = 99999.0;
					foreach (AbigailGame.CowboyMonster i in AbigailGame.monsters)
					{
						double distance = Math.Sqrt(Math.Pow((double)((float)i.position.X - AbigailGame.player2Position.X), 2.0) - Math.Pow((double)((float)i.position.Y - AbigailGame.player2Position.Y), 2.0));
						if (closest == null || distance < closestDistance)
						{
							closest = i;
							closestDistance = Math.Sqrt(Math.Pow((double)((float)closest.position.X - AbigailGame.player2Position.X), 2.0) - Math.Pow((double)((float)closest.position.Y - AbigailGame.player2Position.Y), 2.0));
						}
					}
					this.targetMonster = closest;
				}
				this.player2ShootingDirections.Clear();
				this.player2MovementDirections.Clear();
				if (this.targetMonster != null)
				{
					if (Math.Sqrt(Math.Pow((double)((float)this.targetMonster.position.X - AbigailGame.player2Position.X), 2.0) - Math.Pow((double)((float)this.targetMonster.position.Y - AbigailGame.player2Position.Y), 2.0)) < (double)(AbigailGame.TileSize * 3))
					{
						if ((float)this.targetMonster.position.X > AbigailGame.player2Position.X)
						{
							this.addPlayer2MovementDirection(3);
						}
						else if ((float)this.targetMonster.position.X < AbigailGame.player2Position.X)
						{
							this.addPlayer2MovementDirection(1);
						}
						if ((float)this.targetMonster.position.Y > AbigailGame.player2Position.Y)
						{
							this.addPlayer2MovementDirection(0);
						}
						else if ((float)this.targetMonster.position.Y < AbigailGame.player2Position.Y)
						{
							this.addPlayer2MovementDirection(2);
						}
						using (List<int>.Enumerator enumerator2 = this.player2MovementDirections.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								int j = enumerator2.Current;
								this.player2ShootingDirections.Add((j + 2) % 4);
							}
							goto IL_4DC;
						}
					}
					if (Math.Abs((float)this.targetMonster.position.X - AbigailGame.player2Position.X) > Math.Abs((float)this.targetMonster.position.Y - AbigailGame.player2Position.Y) && Math.Abs((float)this.targetMonster.position.Y - AbigailGame.player2Position.Y) > 4f)
					{
						if ((float)this.targetMonster.position.Y > AbigailGame.player2Position.Y + 3f)
						{
							this.addPlayer2MovementDirection(2);
						}
						else if ((float)this.targetMonster.position.Y < AbigailGame.player2Position.Y - 3f)
						{
							this.addPlayer2MovementDirection(0);
						}
					}
					else if (Math.Abs((float)this.targetMonster.position.X - AbigailGame.player2Position.X) > 4f)
					{
						if ((float)this.targetMonster.position.X > AbigailGame.player2Position.X + 3f)
						{
							this.addPlayer2MovementDirection(1);
						}
						else if ((float)this.targetMonster.position.X < AbigailGame.player2Position.X - 3f)
						{
							this.addPlayer2MovementDirection(3);
						}
					}
					if ((float)this.targetMonster.position.X > AbigailGame.player2Position.X + 3f)
					{
						this.addPlayer2ShootingDirection(1);
					}
					else if ((float)this.targetMonster.position.X < AbigailGame.player2Position.X - 3f)
					{
						this.addPlayer2ShootingDirection(3);
					}
					if ((float)this.targetMonster.position.Y > AbigailGame.player2Position.Y + 3f)
					{
						this.addPlayer2ShootingDirection(2);
					}
					else if ((float)this.targetMonster.position.Y < AbigailGame.player2Position.Y - 3f)
					{
						this.addPlayer2ShootingDirection(0);
					}
				}
				IL_4DC:
				if (this.player2MovementDirections.Count > 0)
				{
					float speed = this.getMovementSpeed(3f, this.player2MovementDirections.Count);
					for (int k = 0; k < this.player2MovementDirections.Count; k++)
					{
						Vector2 newPlayerPosition = AbigailGame.player2Position;
						switch (this.player2MovementDirections[k])
						{
						case 0:
							newPlayerPosition.Y -= speed;
							break;
						case 1:
							newPlayerPosition.X += speed;
							break;
						case 2:
							newPlayerPosition.Y += speed;
							break;
						case 3:
							newPlayerPosition.X -= speed;
							break;
						}
						Rectangle newPlayerBox = new Rectangle((int)newPlayerPosition.X + AbigailGame.TileSize / 4, (int)newPlayerPosition.Y + AbigailGame.TileSize / 4, AbigailGame.TileSize / 2, AbigailGame.TileSize / 2);
						if (!AbigailGame.isCollidingWithMap(newPlayerBox) && (!this.merchantBox.Intersects(newPlayerBox) || this.merchantBox.Intersects(this.player2BoundingBox)) && !newPlayerBox.Intersects(this.playerBoundingBox))
						{
							AbigailGame.player2Position = newPlayerPosition;
						}
					}
					this.player2BoundingBox.X = (int)AbigailGame.player2Position.X + AbigailGame.TileSize / 4;
					this.player2BoundingBox.Y = (int)AbigailGame.player2Position.Y + AbigailGame.TileSize / 4;
					this.player2BoundingBox.Width = AbigailGame.TileSize / 2;
					this.player2BoundingBox.Height = AbigailGame.TileSize / 2;
					this.player2AnimationTimer += time.ElapsedGameTime.Milliseconds;
					this.player2AnimationTimer %= 400;
					this.player2FootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.player2FootstepSoundTimer <= 0)
					{
						Game1.playSound("Cowboy_Footstep", null);
						this.player2FootstepSoundTimer = 200;
					}
					AbigailGame.powerups.RemoveAll((AbigailGame.CowboyPowerup item) => this.player2BoundingBox.Intersects(new Rectangle(item.position.X, item.position.Y, AbigailGame.TileSize, AbigailGame.TileSize)) && !this.player2BoundingBox.Intersects(this.noPickUpBox));
				}
				this.player2shotTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.player2ShootingDirections.Count > 0 && this.player2shotTimer <= 0)
				{
					if (this.player2ShootingDirections.Count == 1)
					{
						this.spawnBullets(new int[]
						{
							this.player2ShootingDirections[0]
						}, AbigailGame.player2Position);
					}
					else
					{
						this.spawnBullets(this.player2ShootingDirections, AbigailGame.player2Position);
					}
					Game1.playSound("Cowboy_gunshot", null);
					this.player2shotTimer = this.shootingDelay;
				}
			}
		}

		// Token: 0x060024FE RID: 9470 RVA: 0x001994E0 File Offset: 0x001976E0
		public int[,] getMap(int wave)
		{
			int[,] newMap = new int[16, 16];
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if ((i == 0 || i == 15 || j == 0 || j == 15) && (i <= 6 || i >= 10) && (j <= 6 || j >= 10))
					{
						newMap[i, j] = 5;
					}
					else if (i == 0 || i == 15 || j == 0 || j == 15)
					{
						newMap[i, j] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (i == 1 || i == 14 || j == 1 || j == 14)
					{
						newMap[i, j] = 2;
					}
					else
					{
						newMap[i, j] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			switch (wave)
			{
			case -1:
				for (int k = 0; k < 16; k++)
				{
					for (int l = 0; l < 16; l++)
					{
						if (newMap[k, l] == 0 || newMap[k, l] == 1 || newMap[k, l] == 2 || newMap[k, l] == 5)
						{
							newMap[k, l] = 3;
						}
					}
				}
				newMap[3, 1] = 5;
				newMap[8, 2] = 5;
				newMap[13, 1] = 5;
				newMap[5, 0] = 0;
				newMap[10, 2] = 2;
				newMap[15, 2] = 1;
				newMap[14, 12] = 5;
				newMap[10, 6] = 7;
				newMap[11, 6] = 7;
				newMap[12, 6] = 7;
				newMap[13, 6] = 7;
				newMap[14, 6] = 7;
				newMap[14, 7] = 7;
				newMap[14, 8] = 7;
				newMap[14, 9] = 7;
				newMap[14, 10] = 7;
				newMap[14, 11] = 7;
				newMap[14, 12] = 7;
				newMap[14, 13] = 7;
				for (int m = 0; m < 16; m++)
				{
					newMap[m, 3] = ((m % 2 == 0) ? 9 : 8);
				}
				newMap[3, 3] = 10;
				newMap[7, 8] = 2;
				newMap[8, 8] = 2;
				newMap[4, 11] = 2;
				newMap[11, 12] = 2;
				newMap[9, 11] = 2;
				newMap[3, 9] = 2;
				newMap[2, 12] = 5;
				newMap[8, 13] = 5;
				newMap[12, 11] = 5;
				newMap[7, 14] = 0;
				newMap[6, 14] = 2;
				newMap[8, 14] = 2;
				newMap[7, 13] = 2;
				newMap[7, 15] = 2;
				return newMap;
			case 1:
				newMap[4, 4] = 7;
				newMap[4, 5] = 7;
				newMap[5, 4] = 7;
				newMap[12, 4] = 7;
				newMap[11, 4] = 7;
				newMap[12, 5] = 7;
				newMap[4, 12] = 7;
				newMap[5, 12] = 7;
				newMap[4, 11] = 7;
				newMap[12, 12] = 7;
				newMap[11, 12] = 7;
				newMap[12, 11] = 7;
				return newMap;
			case 2:
				newMap[8, 4] = 7;
				newMap[12, 8] = 7;
				newMap[8, 12] = 7;
				newMap[4, 8] = 7;
				newMap[1, 1] = 5;
				newMap[14, 1] = 5;
				newMap[14, 14] = 5;
				newMap[1, 14] = 5;
				newMap[2, 1] = 5;
				newMap[13, 1] = 5;
				newMap[13, 14] = 5;
				newMap[2, 14] = 5;
				newMap[1, 2] = 5;
				newMap[14, 2] = 5;
				newMap[14, 13] = 5;
				newMap[1, 13] = 5;
				return newMap;
			case 3:
				newMap[5, 5] = 7;
				newMap[6, 5] = 7;
				newMap[7, 5] = 7;
				newMap[9, 5] = 7;
				newMap[10, 5] = 7;
				newMap[11, 5] = 7;
				newMap[5, 11] = 7;
				newMap[6, 11] = 7;
				newMap[7, 11] = 7;
				newMap[9, 11] = 7;
				newMap[10, 11] = 7;
				newMap[11, 11] = 7;
				newMap[5, 6] = 7;
				newMap[5, 7] = 7;
				newMap[5, 9] = 7;
				newMap[5, 10] = 7;
				newMap[11, 6] = 7;
				newMap[11, 7] = 7;
				newMap[11, 9] = 7;
				newMap[11, 10] = 7;
				return newMap;
			case 4:
			case 8:
				for (int n = 0; n < 16; n++)
				{
					for (int j2 = 0; j2 < 16; j2++)
					{
						if (newMap[n, j2] == 5)
						{
							newMap[n, j2] = Game1.random.Choose(0, 1);
						}
					}
				}
				for (int i2 = 0; i2 < 16; i2++)
				{
					newMap[i2, 8] = Game1.random.Choose(8, 9);
				}
				newMap[8, 4] = 7;
				newMap[8, 12] = 7;
				newMap[9, 12] = 7;
				newMap[7, 12] = 7;
				newMap[5, 6] = 5;
				newMap[10, 6] = 5;
				return newMap;
			case 5:
				newMap[1, 1] = 5;
				newMap[14, 1] = 5;
				newMap[14, 14] = 5;
				newMap[1, 14] = 5;
				newMap[2, 1] = 5;
				newMap[13, 1] = 5;
				newMap[13, 14] = 5;
				newMap[2, 14] = 5;
				newMap[1, 2] = 5;
				newMap[14, 2] = 5;
				newMap[14, 13] = 5;
				newMap[1, 13] = 5;
				newMap[3, 1] = 5;
				newMap[13, 1] = 5;
				newMap[13, 13] = 5;
				newMap[1, 13] = 5;
				newMap[1, 3] = 5;
				newMap[13, 3] = 5;
				newMap[12, 13] = 5;
				newMap[3, 14] = 5;
				newMap[3, 3] = 5;
				newMap[13, 12] = 5;
				newMap[13, 12] = 5;
				newMap[3, 12] = 5;
				return newMap;
			case 6:
				newMap[4, 5] = 2;
				newMap[12, 10] = 5;
				newMap[10, 9] = 5;
				newMap[5, 12] = 2;
				newMap[5, 9] = 5;
				newMap[12, 12] = 5;
				newMap[3, 4] = 5;
				newMap[2, 3] = 5;
				newMap[11, 3] = 5;
				newMap[10, 6] = 5;
				newMap[5, 9] = 7;
				newMap[10, 12] = 7;
				newMap[3, 12] = 7;
				newMap[10, 8] = 7;
				return newMap;
			case 7:
				for (int i3 = 0; i3 < 16; i3++)
				{
					newMap[i3, 5] = ((i3 % 2 == 0) ? 9 : 8);
					newMap[i3, 10] = ((i3 % 2 == 0) ? 9 : 8);
				}
				newMap[4, 5] = 10;
				newMap[8, 5] = 10;
				newMap[12, 5] = 10;
				newMap[4, 10] = 10;
				newMap[8, 10] = 10;
				newMap[12, 10] = 10;
				return newMap;
			case 9:
				newMap[4, 4] = 5;
				newMap[5, 4] = 5;
				newMap[10, 4] = 5;
				newMap[12, 4] = 5;
				newMap[4, 5] = 5;
				newMap[5, 5] = 5;
				newMap[10, 5] = 5;
				newMap[12, 5] = 5;
				newMap[4, 10] = 5;
				newMap[5, 10] = 5;
				newMap[10, 10] = 5;
				newMap[12, 10] = 5;
				newMap[4, 12] = 5;
				newMap[5, 12] = 5;
				newMap[10, 12] = 5;
				newMap[12, 12] = 5;
				return newMap;
			case 10:
				for (int i4 = 0; i4 < 16; i4++)
				{
					newMap[i4, 1] = ((i4 % 2 == 0) ? 9 : 8);
					newMap[i4, 14] = ((i4 % 2 == 0) ? 9 : 8);
				}
				newMap[8, 1] = 10;
				newMap[7, 1] = 10;
				newMap[9, 1] = 10;
				newMap[8, 14] = 10;
				newMap[7, 14] = 10;
				newMap[9, 14] = 10;
				newMap[6, 8] = 5;
				newMap[10, 8] = 5;
				newMap[8, 6] = 5;
				newMap[8, 9] = 5;
				return newMap;
			case 11:
				for (int i5 = 0; i5 < 16; i5++)
				{
					newMap[i5, 0] = 7;
					newMap[i5, 15] = 7;
					if (i5 % 2 == 0)
					{
						newMap[i5, 1] = 5;
						newMap[i5, 14] = 5;
					}
				}
				return newMap;
			case 12:
			{
				for (int i6 = 0; i6 < 16; i6++)
				{
					for (int j3 = 0; j3 < 16; j3++)
					{
						if (newMap[i6, j3] == 0 || newMap[i6, j3] == 1)
						{
							newMap[i6, j3] = 5;
						}
					}
				}
				for (int i7 = 0; i7 < 16; i7++)
				{
					newMap[i7, 0] = ((i7 % 2 == 0) ? 9 : 8);
					newMap[i7, 15] = ((i7 % 2 == 0) ? 9 : 8);
				}
				Rectangle r = new Rectangle(1, 1, 14, 14);
				foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
				{
					newMap[(int)v.X, (int)v.Y] = 10;
				}
				r.Inflate(-1, -1);
				using (List<Vector2>.Enumerator enumerator = Utility.getBorderOfThisRectangle(r).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Vector2 v2 = enumerator.Current;
						newMap[(int)v2.X, (int)v2.Y] = 2;
					}
					return newMap;
				}
				break;
			}
			}
			newMap[4, 4] = 5;
			newMap[12, 4] = 5;
			newMap[4, 12] = 5;
			newMap[12, 12] = 5;
			return newMap;
		}

		// Token: 0x060024FF RID: 9471 RVA: 0x00199F7C File Offset: 0x0019817C
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002500 RID: 9472 RVA: 0x00199F7E File Offset: 0x0019817E
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002501 RID: 9473 RVA: 0x00199F80 File Offset: 0x00198180
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002502 RID: 9474 RVA: 0x00199F82 File Offset: 0x00198182
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x06002503 RID: 9475 RVA: 0x00199F84 File Offset: 0x00198184
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x06002504 RID: 9476 RVA: 0x00199F88 File Offset: 0x00198188
		public void spawnBullets(IList<int> directions, Vector2 spawn)
		{
			Point bulletSpawn = new Point((int)spawn.X + 24, (int)spawn.Y + 24 - 6);
			int speed = (int)this.getMovementSpeed(8f, 2);
			if (directions.Count == 1)
			{
				int playerShootingDirection = directions[0];
				switch (playerShootingDirection)
				{
				case 0:
					bulletSpawn.Y -= 22;
					break;
				case 1:
					bulletSpawn.X += 16;
					bulletSpawn.Y -= 6;
					break;
				case 2:
					bulletSpawn.Y += 10;
					break;
				case 3:
					bulletSpawn.X -= 16;
					bulletSpawn.Y -= 6;
					break;
				}
				this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, playerShootingDirection, this.bulletDamage));
				if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
				{
					switch (playerShootingDirection)
					{
					case 0:
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, -8), this.bulletDamage));
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, -8), this.bulletDamage));
						return;
					case 1:
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, -2), this.bulletDamage));
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, 2), this.bulletDamage));
						return;
					case 2:
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, 8), this.bulletDamage));
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, 8), this.bulletDamage));
						return;
					case 3:
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, -2), this.bulletDamage));
						this.bullets.Add(new AbigailGame.CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, 2), this.bulletDamage));
						return;
					default:
						return;
					}
				}
			}
			else if (directions.Contains(0) && directions.Contains(1))
			{
				bulletSpawn.X += AbigailGame.TileSize / 2;
				bulletSpawn.Y -= AbigailGame.TileSize / 2;
				this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(speed, -speed), this.bulletDamage));
				if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
				{
					int modifier = -2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(speed + modifier, -speed + modifier), this.bulletDamage));
					modifier = 2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(speed + modifier, -speed + modifier), this.bulletDamage));
					return;
				}
			}
			else if (directions.Contains(0) && directions.Contains(3))
			{
				bulletSpawn.X -= AbigailGame.TileSize / 2;
				bulletSpawn.Y -= AbigailGame.TileSize / 2;
				this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(-speed, -speed), this.bulletDamage));
				if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
				{
					int modifier2 = -2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(-speed - modifier2, -speed + modifier2), this.bulletDamage));
					modifier2 = 2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(-speed - modifier2, -speed + modifier2), this.bulletDamage));
					return;
				}
			}
			else if (directions.Contains(2) && directions.Contains(1))
			{
				bulletSpawn.X += AbigailGame.TileSize / 2;
				bulletSpawn.Y += AbigailGame.TileSize / 4;
				this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(speed, speed), this.bulletDamage));
				if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
				{
					int modifier3 = -2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(speed - modifier3, speed + modifier3), this.bulletDamage));
					modifier3 = 2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(speed - modifier3, speed + modifier3), this.bulletDamage));
					return;
				}
			}
			else if (directions.Contains(2) && directions.Contains(3))
			{
				bulletSpawn.X -= AbigailGame.TileSize / 2;
				bulletSpawn.Y += AbigailGame.TileSize / 4;
				this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(-speed, speed), this.bulletDamage));
				if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
				{
					int modifier4 = -2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(-speed + modifier4, speed + modifier4), this.bulletDamage));
					modifier4 = 2;
					this.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawn, new Point(-speed + modifier4, speed + modifier4), this.bulletDamage));
				}
			}
		}

		// Token: 0x06002505 RID: 9477 RVA: 0x0019A510 File Offset: 0x00198710
		public bool isSpawnQueueEmpty()
		{
			for (int i = 0; i < 4; i++)
			{
				if (this.spawnQueue[i].Count > 0)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002506 RID: 9478 RVA: 0x0019A53C File Offset: 0x0019873C
		public static bool isMapTilePassable(int tileType)
		{
			return tileType > 1 && tileType - 5 > 4;
		}

		// Token: 0x06002507 RID: 9479 RVA: 0x0019A54B File Offset: 0x0019874B
		public static bool isMapTilePassableForMonsters(int tileType)
		{
			return tileType != 5 && tileType - 7 > 2;
		}

		// Token: 0x06002508 RID: 9480 RVA: 0x0019A55C File Offset: 0x0019875C
		public static bool isCollidingWithMonster(Rectangle r, AbigailGame.CowboyMonster subject)
		{
			foreach (AbigailGame.CowboyMonster c in AbigailGame.monsters)
			{
				if ((subject == null || !subject.Equals(c)) && Math.Abs(c.position.X - r.X) < 48 && Math.Abs(c.position.Y - r.Y) < 48 && r.Intersects(new Rectangle(c.position.X, c.position.Y, 48, 48)))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002509 RID: 9481 RVA: 0x0019A618 File Offset: 0x00198818
		public static bool isCollidingWithMapForMonsters(Rectangle positionToCheck)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !AbigailGame.isMapTilePassableForMonsters(AbigailGame.map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600250A RID: 9482 RVA: 0x0019A69C File Offset: 0x0019889C
		public static bool isCollidingWithMap(Rectangle positionToCheck)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !AbigailGame.isMapTilePassable(AbigailGame.map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600250B RID: 9483 RVA: 0x0019A720 File Offset: 0x00198920
		public static bool isCollidingWithMap(Point position)
		{
			Rectangle positionToCheck = new Rectangle(position.X, position.Y, 48, 48);
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !AbigailGame.isMapTilePassable(AbigailGame.map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600250C RID: 9484 RVA: 0x0019A7BC File Offset: 0x001989BC
		private void addPlayer2MovementDirection(int direction)
		{
			if (!this.player2MovementDirections.Contains(direction))
			{
				if (this.player2MovementDirections.Count == 1 && direction == (this.player2MovementDirections[0] + 2) % 4)
				{
					this.player2MovementDirections.Clear();
				}
				this.player2MovementDirections.Add(direction);
				if (this.player2MovementDirections.Count > 2)
				{
					this.player2MovementDirections.RemoveAt(0);
				}
			}
		}

		// Token: 0x0600250D RID: 9485 RVA: 0x0019A82C File Offset: 0x00198A2C
		private void addPlayerMovementDirection(int direction)
		{
			if (AbigailGame.gopherTrain)
			{
				return;
			}
			if (!AbigailGame.playerMovementDirections.Contains(direction))
			{
				if (AbigailGame.playerMovementDirections.Count == 1)
				{
					int num = (AbigailGame.playerMovementDirections.ElementAt(0) + 2) % 4;
				}
				AbigailGame.playerMovementDirections.Add(direction);
			}
		}

		// Token: 0x0600250E RID: 9486 RVA: 0x0019A878 File Offset: 0x00198A78
		private void addPlayer2ShootingDirection(int direction)
		{
			if (!this.player2ShootingDirections.Contains(direction))
			{
				if (this.player2ShootingDirections.Count == 1 && direction == (this.player2ShootingDirections[0] + 2) % 4)
				{
					this.player2ShootingDirections.Clear();
				}
				this.player2ShootingDirections.Add(direction);
				if (this.player2ShootingDirections.Count > 2)
				{
					this.player2ShootingDirections.RemoveAt(0);
				}
			}
		}

		// Token: 0x0600250F RID: 9487 RVA: 0x0019A8E5 File Offset: 0x00198AE5
		private void addPlayerShootingDirection(int direction)
		{
			if (!AbigailGame.playerShootingDirections.Contains(direction))
			{
				AbigailGame.playerShootingDirections.Add(direction);
			}
		}

		// Token: 0x06002510 RID: 9488 RVA: 0x0019A900 File Offset: 0x00198B00
		public void startShoppingLevel()
		{
			this.merchantBox.Y = -AbigailGame.TileSize;
			AbigailGame.shopping = true;
			AbigailGame.merchantArriving = true;
			AbigailGame.merchantLeaving = false;
			AbigailGame.merchantShopOpen = false;
			ICue cue = AbigailGame.overworldSong;
			if (cue != null)
			{
				cue.Stop(AudioStopOptions.Immediate);
			}
			AbigailGame.monsters.Clear();
			AbigailGame.waitingForPlayerToMoveDownAMap = true;
			this.storeItems.Clear();
			if (AbigailGame.whichWave == 2)
			{
				this.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), 3);
				this.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), 0);
				this.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), 6);
			}
			else
			{
				this.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.runSpeedLevel >= 2) ? 5 : (3 + this.runSpeedLevel));
				this.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.fireSpeedLevel >= 3) ? ((this.ammoLevel >= 3 && !this.spreadPistol) ? 9 : 10) : this.fireSpeedLevel);
				this.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.ammoLevel < 3) ? (6 + this.ammoLevel) : 10);
			}
			if (this.whichRound > 0)
			{
				this.storeItems.Clear();
				this.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.runSpeedLevel >= 2) ? 5 : (3 + this.runSpeedLevel));
				this.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.fireSpeedLevel >= 3) ? ((this.ammoLevel >= 3 && !this.spreadPistol) ? 9 : 10) : this.fireSpeedLevel);
				this.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.ammoLevel < 3) ? (6 + this.ammoLevel) : 10);
			}
		}

		// Token: 0x06002511 RID: 9489 RVA: 0x0019ABFC File Offset: 0x00198DFC
		public void receiveKeyPress(Keys k)
		{
			if (AbigailGame.onStartMenu)
			{
				AbigailGame.startTimer = 1;
				return;
			}
		}

		// Token: 0x06002512 RID: 9490 RVA: 0x0019AC0C File Offset: 0x00198E0C
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x06002513 RID: 9491 RVA: 0x0019AC10 File Offset: 0x00198E10
		public int getPriceForItem(int whichItem)
		{
			switch (whichItem)
			{
			case 0:
				return 10;
			case 1:
				return 20;
			case 2:
				return 30;
			case 3:
				return 8;
			case 4:
				return 20;
			case 5:
				return 10;
			case 6:
				return 15;
			case 7:
				return 30;
			case 8:
				return 45;
			case 9:
				return 99;
			case 10:
				return 10;
			default:
				return 5;
			}
		}

		// Token: 0x06002514 RID: 9492 RVA: 0x0019AC74 File Offset: 0x00198E74
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (AbigailGame.onStartMenu)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.viewport.Width / 2 - 3 * AbigailGame.TileSize), AbigailGame.topLeftScreenCoordinate.Y + (float)(5 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(128, 1744, 96, 56)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
			}
			else if ((AbigailGame.gameOver || this.gamerestartTimer > 0) && !AbigailGame.endCutscene)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 7f) * (float)AbigailGame.TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 7f) * (float)AbigailGame.TileSize + new Vector2(-1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 7f) * (float)AbigailGame.TileSize + new Vector2(1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				string option = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11917");
				if (this.gameOverOption == 0)
				{
					option = "> " + option;
				}
				string option2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11919");
				if (this.gameOverOption == 1)
				{
					option2 = "> " + option2;
				}
				if (this.gamerestartTimer <= 0 || this.gamerestartTimer / 500 % 2 == 0)
				{
					b.DrawString(Game1.smallFont, option, AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 9f) * (float)AbigailGame.TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				}
				b.DrawString(Game1.smallFont, option2, AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 9f) * (float)AbigailGame.TileSize + new Vector2(0f, (float)(AbigailGame.TileSize * 2 / 3)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
			else if (AbigailGame.endCutscene)
			{
				switch (AbigailGame.endCutscenePhase)
				{
				case 0:
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(384, 1760, 16, 16)), Color.White * ((AbigailGame.endCutsceneTimer < 2000) ? (1f * ((float)AbigailGame.endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize * 2 / 3)) + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16)), Color.White * ((AbigailGame.endCutsceneTimer < 2000) ? (1f * ((float)AbigailGame.endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f);
					break;
				case 1:
				case 2:
				case 3:
					for (int i = 0; i < 16; i++)
					{
						for (int j = 0; j < 16; j++)
						{
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)i, (float)j) * 16f * 3f + new Vector2(0f, (float)(AbigailGame.newMapPosition - 16 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(464 + 16 * AbigailGame.map[i, j] + ((AbigailGame.map[i, j] == 5 && this.cactusDanceTimer > 800f) ? 16 : 0), 1680 - AbigailGame.world * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
						}
					}
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(6 * AbigailGame.TileSize), (float)(3 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(288, 1697, 64, 80)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.01f);
					if (AbigailGame.endCutscenePhase == 3)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(9 * AbigailGame.TileSize), (float)(7 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(544, 1792, 32, 32)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.05f);
						if (AbigailGame.endCutsceneTimer < 3000)
						{
							b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black * (1f - (float)AbigailGame.endCutsceneTimer / 3000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
						}
					}
					else
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(10 * AbigailGame.TileSize), (float)(8 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(272 - AbigailGame.endCutsceneTimer / 300 % 4 * 16, 1792, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.02f);
						if (AbigailGame.endCutscenePhase == 2)
						{
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(4f, 13f) * 3f, new Rectangle?(new Rectangle(484, 1760 + (int)(this.playerMotionAnimationTimer / 100f) * 3, 8, 3)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f + 0.001f);
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition, new Rectangle?(new Rectangle(384, 1760, 16, 13)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f + 0.001f);
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize * 2 / 3 - AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.005f);
						}
						b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black * ((AbigailGame.endCutscenePhase == 1 && AbigailGame.endCutsceneTimer > 12500) ? ((float)((AbigailGame.endCutsceneTimer - 12500) / 3000)) : 0f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
					}
					break;
				case 4:
				case 5:
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(6 * AbigailGame.TileSize), (float)(3 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(224, 1744, 64, 48)), Color.White * ((AbigailGame.endCutsceneTimer > 0) ? (1f - ((float)AbigailGame.endCutsceneTimer - 2000f) / 2000f) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					if (AbigailGame.endCutscenePhase == 5 && this.gamerestartTimer <= 0)
					{
						b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_PK_NewGame+"), AbigailGame.topLeftScreenCoordinate + new Vector2(3f, 10f) * (float)AbigailGame.TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
					}
					break;
				}
			}
			else
			{
				if (AbigailGame.zombieModeTimer > 8200)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition, new Rectangle?(new Rectangle(384 + ((AbigailGame.zombieModeTimer / 200 % 2 == 0) ? 16 : 0), 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					for (int y = (int)(this.playerPosition.Y - (float)AbigailGame.TileSize); y > -AbigailGame.TileSize; y -= AbigailGame.TileSize)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X, (float)y), new Rectangle?(new Rectangle(368 + ((y / AbigailGame.TileSize % 3 == 0) ? 16 : 0), 1744, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					}
					b.End();
					return;
				}
				for (int k = 0; k < 16; k++)
				{
					for (int l = 0; l < 16; l++)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)k, (float)l) * 16f * 3f + new Vector2(0f, (float)(AbigailGame.newMapPosition - 16 * AbigailGame.TileSize)), new Rectangle?(new Rectangle(464 + 16 * AbigailGame.map[k, l] + ((AbigailGame.map[k, l] == 5 && this.cactusDanceTimer > 800f) ? 16 : 0), 1680 - AbigailGame.world * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
					}
				}
				if (AbigailGame.scrollingMap)
				{
					for (int m = 0; m < 16; m++)
					{
						for (int n = 0; n < 16; n++)
						{
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)m, (float)n) * 16f * 3f + new Vector2(0f, (float)AbigailGame.newMapPosition), new Rectangle?(new Rectangle(464 + 16 * AbigailGame.nextMap[m, n] + ((AbigailGame.nextMap[m, n] == 5 && this.cactusDanceTimer > 800f) ? 16 : 0), 1680 - AbigailGame.world * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
						}
					}
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, -1, 16 * AbigailGame.TileSize, (int)AbigailGame.topLeftScreenCoordinate.Y), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y + 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize, (int)AbigailGame.topLeftScreenCoordinate.Y + 2), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
				if (AbigailGame.deathTimer <= 0f && (AbigailGame.playerInvincibleTimer <= 0 || AbigailGame.playerInvincibleTimer / 100 % 2 == 0))
				{
					if (AbigailGame.holdItemTimer > 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(384, 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize * 2 / 3)) + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f);
					}
					else if (AbigailGame.zombieModeTimer > 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(352 + ((AbigailGame.zombieModeTimer / 50 % 2 == 0) ? 16 : 0), 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
					}
					else if (AbigailGame.playerMovementDirections.Count == 0 && AbigailGame.playerShootingDirections.Count == 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(496, 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
					}
					else
					{
						int facingDirection = (AbigailGame.playerShootingDirections.Count == 0) ? AbigailGame.playerMovementDirections.ElementAt(0) : AbigailGame.playerShootingDirections.Last<int>();
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)) + new Vector2(4f, 13f) * 3f, new Rectangle?(new Rectangle(483, 1760 + (int)(this.playerMotionAnimationTimer / 100f) * 3, 10, 3)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(3f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(464 + facingDirection * 16, 1744, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f + 0.001f);
					}
				}
				if (AbigailGame.playingWithAbigail && this.player2deathtimer <= 0 && (this.player2invincibletimer <= 0 || this.player2invincibletimer / 100 % 2 == 0))
				{
					if (this.player2MovementDirections.Count == 0 && this.player2ShootingDirections.Count == 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + AbigailGame.player2Position + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(256, 1728, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
					}
					else
					{
						int facingDirection2 = (this.player2ShootingDirections.Count == 0) ? this.player2MovementDirections[0] : this.player2ShootingDirections[0];
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + AbigailGame.player2Position + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)) + new Vector2(4f, 13f) * 3f, new Rectangle?(new Rectangle(243, 1728 + this.player2AnimationTimer / 100 * 3, 10, 3)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, AbigailGame.player2Position.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + AbigailGame.player2Position + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(224 + facingDirection2 * 16, 1712, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, AbigailGame.player2Position.Y / 10000f + 0.002f + 0.001f);
					}
				}
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in AbigailGame.temporarySprites)
				{
					temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
				}
				foreach (AbigailGame.CowboyPowerup cowboyPowerup in AbigailGame.powerups)
				{
					cowboyPowerup.draw(b);
				}
				foreach (AbigailGame.CowboyBullet p in this.bullets)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)p.position.X, (float)p.position.Y), new Rectangle?(new Rectangle(518, 1760 + (this.bulletDamage - 1) * 4, 4, 4)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}
				foreach (AbigailGame.CowboyBullet p2 in AbigailGame.enemyBullets)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)p2.position.X, (float)p2.position.Y), new Rectangle?(new Rectangle(523, 1760, 5, 5)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}
				if (AbigailGame.shopping)
				{
					if ((AbigailGame.merchantArriving || AbigailGame.merchantLeaving) && !AbigailGame.merchantShopOpen)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.merchantBox.Location.X, (float)this.merchantBox.Location.Y), new Rectangle?(new Rectangle(464 + ((AbigailGame.shoppingTimer / 100 % 2 == 0) ? 16 : 0), 1728, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.merchantBox.Y / 10000f + 0.001f);
					}
					else
					{
						int whichFrame = (this.playerBoundingBox.X - this.merchantBox.X > AbigailGame.TileSize) ? 2 : ((this.merchantBox.X - this.playerBoundingBox.X > AbigailGame.TileSize) ? 1 : 0);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.merchantBox.Location.X, (float)this.merchantBox.Location.Y), new Rectangle?(new Rectangle(496 + whichFrame * 16, 1728, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.merchantBox.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.merchantBox.Location.X - AbigailGame.TileSize), (float)(this.merchantBox.Location.Y + AbigailGame.TileSize)), new Rectangle?(new Rectangle(529, 1744, 63, 32)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.merchantBox.Y / 10000f + 0.001f);
						foreach (KeyValuePair<Rectangle, int> v in this.storeItems)
						{
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)v.Key.Location.X, (float)v.Key.Location.Y), new Rectangle?(new Rectangle(320 + v.Value * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f);
							b.DrawString(Game1.smallFont, this.getPriceForItem(v.Value).ToString() ?? "", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString(this.getPriceForItem(v.Value).ToString() ?? "").X / 2f, (float)(v.Key.Location.Y + AbigailGame.TileSize + 3)), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, this.getPriceForItem(v.Value).ToString() ?? "", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString(this.getPriceForItem(v.Value).ToString() ?? "").X / 2f - 1f, (float)(v.Key.Location.Y + AbigailGame.TileSize + 3)), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, this.getPriceForItem(v.Value).ToString() ?? "", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString(this.getPriceForItem(v.Value).ToString() ?? "").X / 2f + 1f, (float)(v.Key.Location.Y + AbigailGame.TileSize + 3)), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
						}
					}
				}
				if (AbigailGame.waitingForPlayerToMoveDownAMap && (AbigailGame.merchantShopOpen || AbigailGame.merchantLeaving || !AbigailGame.shopping) && AbigailGame.shoppingTimer < 250)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(8.5f, 15f) * (float)AbigailGame.TileSize + new Vector2(-12f, 0f), new Rectangle?(new Rectangle(355, 1750, 8, 8)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.001f);
				}
				foreach (AbigailGame.CowboyMonster cowboyMonster in AbigailGame.monsters)
				{
					cowboyMonster.draw(b);
				}
				if (AbigailGame.gopherRunning)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)AbigailGame.gopherBox.X, (float)AbigailGame.gopherBox.Y), new Rectangle?(new Rectangle(320 + AbigailGame.waveTimer / 100 % 4 * 16, 1792, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)AbigailGame.gopherBox.Y / 10000f + 0.001f);
				}
				if (AbigailGame.gopherTrain && AbigailGame.gopherTrainPosition > -AbigailGame.TileSize)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X - (float)(AbigailGame.TileSize / 2), (float)AbigailGame.gopherTrainPosition), new Rectangle?(new Rectangle(384 + AbigailGame.gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X + (float)(AbigailGame.TileSize / 2), (float)AbigailGame.gopherTrainPosition), new Rectangle?(new Rectangle(384 + AbigailGame.gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X, (float)(AbigailGame.gopherTrainPosition - AbigailGame.TileSize * 3)), new Rectangle?(new Rectangle(320 + AbigailGame.gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X - (float)(AbigailGame.TileSize / 2), (float)(AbigailGame.gopherTrainPosition - AbigailGame.TileSize)), new Rectangle?(new Rectangle(400, 1728, 32, 32)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.97f);
					if (AbigailGame.holdItemTimer > 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(384, 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize * 2 / 3)) + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, (float)(-(float)AbigailGame.TileSize / 4)), new Rectangle?(new Rectangle(464, 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2((float)(AbigailGame.TileSize + 27), 0f), new Rectangle?(new Rectangle(294, 1782, 22, 22)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.25f);
					if (this.heldItem != null)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2((float)(AbigailGame.TileSize + 18), -9f), new Rectangle?(new Rectangle(272 + this.heldItem.which * 16, 1808, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2((float)(AbigailGame.TileSize * 2), (float)(-(float)AbigailGame.TileSize - 18)), new Rectangle?(new Rectangle(400, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + Math.Max(this.lives, 0).ToString(), AbigailGame.topLeftScreenCoordinate - new Vector2((float)AbigailGame.TileSize, (float)(-(float)AbigailGame.TileSize - AbigailGame.TileSize / 4 - 18)), Color.White);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2((float)(AbigailGame.TileSize * 2), (float)(-(float)AbigailGame.TileSize * 2 - 18)), new Rectangle?(new Rectangle(272, 1808, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + this.coins.ToString(), AbigailGame.topLeftScreenCoordinate - new Vector2((float)AbigailGame.TileSize, (float)(-(float)AbigailGame.TileSize * 2 - AbigailGame.TileSize / 4 - 18)), Color.White);
					for (int i2 = 0; i2 < AbigailGame.whichWave + this.whichRound * 12; i2++)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize * 16 + 3), (float)(i2 * 3 * 6)), new Rectangle?(new Rectangle(512, 1760, 5, 5)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					b.Draw(Game1.mouseCursors, new Vector2((float)((int)AbigailGame.topLeftScreenCoordinate.X), (float)((int)AbigailGame.topLeftScreenCoordinate.Y - AbigailGame.TileSize / 2 - 12)), new Rectangle?(new Rectangle(595, 1748, 9, 11)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					if (!AbigailGame.shootoutLevel)
					{
						b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X + 30, (int)AbigailGame.topLeftScreenCoordinate.Y - AbigailGame.TileSize / 2 + 3, (int)((float)(16 * AbigailGame.TileSize - 30) * ((float)AbigailGame.waveTimer / 80000f)), AbigailGame.TileSize / 4), (AbigailGame.waveTimer < 8000) ? new Color(188, 51, 74) : new Color(147, 177, 38));
					}
					if (AbigailGame.betweenWaveTimer > 0 && AbigailGame.whichWave == 0 && !AbigailGame.scrollingMap)
					{
						Vector2 pos = new Vector2((float)(Game1.viewport.Width / 2 - 120), (float)(Game1.viewport.Height - 144 - 3));
						if (!Game1.options.gamepadControls)
						{
							b.Draw(Game1.mouseCursors, pos, new Rectangle?(new Rectangle(352, 1648, 80, 48)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
						}
						else
						{
							b.Draw(Game1.controllerMaps, pos, new Rectangle?(Utility.controllerMapSourceRect(new Rectangle(681, 157, 160, 96))), Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.99f);
						}
					}
					if (this.bulletDamage > 1)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(-(float)AbigailGame.TileSize - 3), (float)(16 * AbigailGame.TileSize - AbigailGame.TileSize)), new Rectangle?(new Rectangle(416 + (this.ammoLevel - 1) * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (this.fireSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(-(float)AbigailGame.TileSize - 3), (float)(16 * AbigailGame.TileSize - AbigailGame.TileSize * 2)), new Rectangle?(new Rectangle(320 + (this.fireSpeedLevel - 1) * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (this.runSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(-(float)AbigailGame.TileSize - 3), (float)(16 * AbigailGame.TileSize - AbigailGame.TileSize * 3)), new Rectangle?(new Rectangle(368 + (this.runSpeedLevel - 1) * 16, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (this.spreadPistol)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(-(float)AbigailGame.TileSize - 3), (float)(16 * AbigailGame.TileSize - AbigailGame.TileSize * 4)), new Rectangle?(new Rectangle(464, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
				}
				if (AbigailGame.screenFlash > 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), new Rectangle?(Game1.staminaRect.Bounds), new Color(255, 214, 168), 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
			}
			if (this.fadethenQuitTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Rectangle?(Game1.staminaRect.Bounds), Color.Black * (1f - (float)this.fadethenQuitTimer / 2000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
			}
			if (this.abigailPortraitTimer > 0)
			{
				b.Draw(this.abigail.Portrait, new Vector2(AbigailGame.topLeftScreenCoordinate.X + (float)(16 * AbigailGame.TileSize), (float)this.abigailPortraitYposition), new Rectangle?(new Rectangle(64 * (this.abigailPortraitExpression % 2), 64 * (this.abigailPortraitExpression / 2), 64, 64)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (this.abigailPortraitTimer < 5500 && this.abigailPortraitTimer > 500)
				{
					int width = SpriteText.getWidthOfString("0" + this.AbigailDialogue + "0", 999999);
					int x = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? ((int)(AbigailGame.topLeftScreenCoordinate.X + (float)(16 * AbigailGame.TileSize)) + width / 4) : ((int)(AbigailGame.topLeftScreenCoordinate.X + (float)(16 * AbigailGame.TileSize)));
					SpriteText.drawString(b, this.AbigailDialogue, x, (int)((double)this.abigailPortraitYposition - 80.0), 999999, width, 999999, 1f, 0.88f, false, -1, "", new Color?(SpriteText.color_Purple), SpriteText.ScrollTextAlignment.Left);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (Game1.IsMultiplayer)
			{
				string time_of_day_string = Game1.getTimeOfDayString(Game1.timeOfDay);
				Vector2 draw_position = new Vector2((float)Game1.viewport.Width - Game1.dialogueFont.MeasureString(time_of_day_string).X - 16f, 16f);
				Color timeColor = Color.White;
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position, timeColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.01f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			}
			b.End();
		}

		// Token: 0x06002515 RID: 9493 RVA: 0x0019D9CC File Offset: 0x0019BBCC
		public void changeScreenSize()
		{
			AbigailGame.topLeftScreenCoordinate = new Vector2((float)(Game1.viewport.Width / 2 - 384), (float)(Game1.viewport.Height / 2 - 384));
		}

		// Token: 0x06002516 RID: 9494 RVA: 0x0019DA00 File Offset: 0x0019BC00
		public void unload()
		{
			if (AbigailGame.overworldSong != null && AbigailGame.overworldSong.IsPlaying)
			{
				AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
			}
			if (AbigailGame.outlawSong != null && AbigailGame.outlawSong.IsPlaying)
			{
				AbigailGame.outlawSong.Stop(AudioStopOptions.Immediate);
			}
			this.lives = 3;
			Game1.stopMusicTrack(MusicContext.MiniGame);
		}

		// Token: 0x06002517 RID: 9495 RVA: 0x0019DA56 File Offset: 0x0019BC56
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x06002518 RID: 9496 RVA: 0x0019DA58 File Offset: 0x0019BC58
		public string minigameId()
		{
			return "PrairieKing";
		}

		// Token: 0x06002519 RID: 9497 RVA: 0x0019DA5F File Offset: 0x0019BC5F
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x0600251A RID: 9498 RVA: 0x0019DA62 File Offset: 0x0019BC62
		public bool forceQuit()
		{
			if (AbigailGame.playingWithAbigail)
			{
				return false;
			}
			this.unload();
			return true;
		}

		// Token: 0x040015CD RID: 5581
		public const int mapWidth = 16;

		// Token: 0x040015CE RID: 5582
		public const int mapHeight = 16;

		// Token: 0x040015CF RID: 5583
		public const int pixelZoom = 3;

		// Token: 0x040015D0 RID: 5584
		public const int bulletSpeed = 8;

		// Token: 0x040015D1 RID: 5585
		public const double lootChance = 0.05;

		// Token: 0x040015D2 RID: 5586
		public const double coinChance = 0.05;

		// Token: 0x040015D3 RID: 5587
		public int lootDuration = 7500;

		// Token: 0x040015D4 RID: 5588
		public int powerupDuration = 10000;

		// Token: 0x040015D5 RID: 5589
		public const int abigailPortraitDuration = 6000;

		// Token: 0x040015D6 RID: 5590
		public const float playerSpeed = 3f;

		// Token: 0x040015D7 RID: 5591
		public const int baseTileSize = 16;

		// Token: 0x040015D8 RID: 5592
		public const int orcSpeed = 2;

		// Token: 0x040015D9 RID: 5593
		public const int ogreSpeed = 1;

		// Token: 0x040015DA RID: 5594
		public const int ghostSpeed = 3;

		// Token: 0x040015DB RID: 5595
		public const int spikeySpeed = 3;

		// Token: 0x040015DC RID: 5596
		public const int spikeyHealth = 2;

		// Token: 0x040015DD RID: 5597
		public const int cactusDanceDelay = 800;

		// Token: 0x040015DE RID: 5598
		public const int playerMotionDelay = 100;

		// Token: 0x040015DF RID: 5599
		public const int playerFootStepDelay = 200;

		// Token: 0x040015E0 RID: 5600
		public const int deathDelay = 3000;

		// Token: 0x040015E1 RID: 5601
		public const int MAP_BARRIER1 = 0;

		// Token: 0x040015E2 RID: 5602
		public const int MAP_BARRIER2 = 1;

		// Token: 0x040015E3 RID: 5603
		public const int MAP_ROCKY1 = 2;

		// Token: 0x040015E4 RID: 5604
		public const int MAP_DESERT = 3;

		// Token: 0x040015E5 RID: 5605
		public const int MAP_GRASSY = 4;

		// Token: 0x040015E6 RID: 5606
		public const int MAP_CACTUS = 5;

		// Token: 0x040015E7 RID: 5607
		public const int MAP_FENCE = 7;

		// Token: 0x040015E8 RID: 5608
		public const int MAP_TRENCH1 = 8;

		// Token: 0x040015E9 RID: 5609
		public const int MAP_TRENCH2 = 9;

		// Token: 0x040015EA RID: 5610
		public const int MAP_BRIDGE = 10;

		// Token: 0x040015EB RID: 5611
		public const int orc = 0;

		// Token: 0x040015EC RID: 5612
		public const int ghost = 1;

		// Token: 0x040015ED RID: 5613
		public const int ogre = 2;

		// Token: 0x040015EE RID: 5614
		public const int mummy = 3;

		// Token: 0x040015EF RID: 5615
		public const int devil = 4;

		// Token: 0x040015F0 RID: 5616
		public const int mushroom = 5;

		// Token: 0x040015F1 RID: 5617
		public const int spikey = 6;

		// Token: 0x040015F2 RID: 5618
		public const int dracula = 7;

		// Token: 0x040015F3 RID: 5619
		public const int desert = 0;

		// Token: 0x040015F4 RID: 5620
		public const int woods = 2;

		// Token: 0x040015F5 RID: 5621
		public const int graveyard = 1;

		// Token: 0x040015F6 RID: 5622
		public const int POWERUP_LOG = -1;

		// Token: 0x040015F7 RID: 5623
		public const int POWERUP_SKULL = -2;

		// Token: 0x040015F8 RID: 5624
		public const int coin1 = 0;

		// Token: 0x040015F9 RID: 5625
		public const int coin5 = 1;

		// Token: 0x040015FA RID: 5626
		public const int POWERUP_SPREAD = 2;

		// Token: 0x040015FB RID: 5627
		public const int POWERUP_RAPIDFIRE = 3;

		// Token: 0x040015FC RID: 5628
		public const int POWERUP_NUKE = 4;

		// Token: 0x040015FD RID: 5629
		public const int POWERUP_ZOMBIE = 5;

		// Token: 0x040015FE RID: 5630
		public const int POWERUP_SPEED = 6;

		// Token: 0x040015FF RID: 5631
		public const int POWERUP_SHOTGUN = 7;

		// Token: 0x04001600 RID: 5632
		public const int POWERUP_LIFE = 8;

		// Token: 0x04001601 RID: 5633
		public const int POWERUP_TELEPORT = 9;

		// Token: 0x04001602 RID: 5634
		public const int POWERUP_SHERRIFF = 10;

		// Token: 0x04001603 RID: 5635
		public const int POWERUP_HEART = -3;

		// Token: 0x04001604 RID: 5636
		public const int ITEM_FIRESPEED1 = 0;

		// Token: 0x04001605 RID: 5637
		public const int ITEM_FIRESPEED2 = 1;

		// Token: 0x04001606 RID: 5638
		public const int ITEM_FIRESPEED3 = 2;

		// Token: 0x04001607 RID: 5639
		public const int ITEM_RUNSPEED1 = 3;

		// Token: 0x04001608 RID: 5640
		public const int ITEM_RUNSPEED2 = 4;

		// Token: 0x04001609 RID: 5641
		public const int ITEM_LIFE = 5;

		// Token: 0x0400160A RID: 5642
		public const int ITEM_AMMO1 = 6;

		// Token: 0x0400160B RID: 5643
		public const int ITEM_AMMO2 = 7;

		// Token: 0x0400160C RID: 5644
		public const int ITEM_AMMO3 = 8;

		// Token: 0x0400160D RID: 5645
		public const int ITEM_SPREADPISTOL = 9;

		// Token: 0x0400160E RID: 5646
		public const int ITEM_STAR = 10;

		// Token: 0x0400160F RID: 5647
		public const int ITEM_SKULL = 11;

		// Token: 0x04001610 RID: 5648
		public const int ITEM_LOG = 12;

		// Token: 0x04001611 RID: 5649
		public const int option_retry = 0;

		// Token: 0x04001612 RID: 5650
		public const int option_quit = 1;

		// Token: 0x04001613 RID: 5651
		public int runSpeedLevel;

		// Token: 0x04001614 RID: 5652
		public int fireSpeedLevel;

		// Token: 0x04001615 RID: 5653
		public int ammoLevel;

		// Token: 0x04001616 RID: 5654
		public int whichRound;

		// Token: 0x04001617 RID: 5655
		public bool spreadPistol;

		// Token: 0x04001618 RID: 5656
		public const int waveDuration = 80000;

		// Token: 0x04001619 RID: 5657
		public const int betweenWaveDuration = 5000;

		// Token: 0x0400161A RID: 5658
		public static List<AbigailGame.CowboyMonster> monsters = new List<AbigailGame.CowboyMonster>();

		// Token: 0x0400161B RID: 5659
		protected HashSet<Vector2> _borderTiles = new HashSet<Vector2>();

		// Token: 0x0400161C RID: 5660
		public Vector2 playerPosition;

		// Token: 0x0400161D RID: 5661
		public static Vector2 player2Position = default(Vector2);

		// Token: 0x0400161E RID: 5662
		public Rectangle playerBoundingBox;

		// Token: 0x0400161F RID: 5663
		public Rectangle merchantBox;

		// Token: 0x04001620 RID: 5664
		public Rectangle player2BoundingBox;

		// Token: 0x04001621 RID: 5665
		public Rectangle noPickUpBox;

		// Token: 0x04001622 RID: 5666
		public static List<int> playerMovementDirections = new List<int>();

		// Token: 0x04001623 RID: 5667
		public static List<int> playerShootingDirections = new List<int>();

		// Token: 0x04001624 RID: 5668
		public List<int> player2MovementDirections = new List<int>();

		// Token: 0x04001625 RID: 5669
		public List<int> player2ShootingDirections = new List<int>();

		// Token: 0x04001626 RID: 5670
		public int shootingDelay = 300;

		// Token: 0x04001627 RID: 5671
		public int shotTimer;

		// Token: 0x04001628 RID: 5672
		public int motionPause;

		// Token: 0x04001629 RID: 5673
		public int bulletDamage;

		// Token: 0x0400162A RID: 5674
		public int lives = 3;

		// Token: 0x0400162B RID: 5675
		public int coins;

		// Token: 0x0400162C RID: 5676
		public int score;

		// Token: 0x0400162D RID: 5677
		public int player2deathtimer;

		// Token: 0x0400162E RID: 5678
		public int player2invincibletimer;

		// Token: 0x0400162F RID: 5679
		public List<AbigailGame.CowboyBullet> bullets = new List<AbigailGame.CowboyBullet>();

		// Token: 0x04001630 RID: 5680
		public static List<AbigailGame.CowboyBullet> enemyBullets = new List<AbigailGame.CowboyBullet>();

		// Token: 0x04001631 RID: 5681
		public static int[,] map = new int[16, 16];

		// Token: 0x04001632 RID: 5682
		public static int[,] nextMap = new int[16, 16];

		// Token: 0x04001633 RID: 5683
		public List<Point>[] spawnQueue = new List<Point>[4];

		// Token: 0x04001634 RID: 5684
		public static Vector2 topLeftScreenCoordinate;

		// Token: 0x04001635 RID: 5685
		public float cactusDanceTimer;

		// Token: 0x04001636 RID: 5686
		public float playerMotionAnimationTimer;

		// Token: 0x04001637 RID: 5687
		public float playerFootstepSoundTimer = 200f;

		// Token: 0x04001638 RID: 5688
		public AbigailGame.behaviorAfterMotionPause behaviorAfterPause;

		// Token: 0x04001639 RID: 5689
		public List<Vector2> monsterChances = new List<Vector2>
		{
			new Vector2(0.014f, 0.4f),
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero
		};

		// Token: 0x0400163A RID: 5690
		public Rectangle shoppingCarpetNoPickup;

		// Token: 0x0400163B RID: 5691
		public Dictionary<int, int> activePowerups = new Dictionary<int, int>();

		// Token: 0x0400163C RID: 5692
		public NPC abigail;

		// Token: 0x0400163D RID: 5693
		public static List<AbigailGame.CowboyPowerup> powerups = new List<AbigailGame.CowboyPowerup>();

		// Token: 0x0400163E RID: 5694
		public string AbigailDialogue = "";

		// Token: 0x0400163F RID: 5695
		public static TemporaryAnimatedSpriteList temporarySprites = new TemporaryAnimatedSpriteList();

		// Token: 0x04001640 RID: 5696
		public AbigailGame.CowboyPowerup heldItem;

		// Token: 0x04001641 RID: 5697
		public static int world = 0;

		// Token: 0x04001642 RID: 5698
		public int gameOverOption;

		// Token: 0x04001643 RID: 5699
		public int gamerestartTimer;

		// Token: 0x04001644 RID: 5700
		public int player2TargetUpdateTimer;

		// Token: 0x04001645 RID: 5701
		public int player2shotTimer;

		// Token: 0x04001646 RID: 5702
		public int player2AnimationTimer;

		// Token: 0x04001647 RID: 5703
		public int fadethenQuitTimer;

		// Token: 0x04001648 RID: 5704
		public int abigailPortraitYposition;

		// Token: 0x04001649 RID: 5705
		public int abigailPortraitTimer;

		// Token: 0x0400164A RID: 5706
		public int abigailPortraitExpression;

		// Token: 0x0400164B RID: 5707
		public static int waveTimer = 80000;

		// Token: 0x0400164C RID: 5708
		public static int betweenWaveTimer = 5000;

		// Token: 0x0400164D RID: 5709
		public static int whichWave;

		// Token: 0x0400164E RID: 5710
		public static int monsterConfusionTimer;

		// Token: 0x0400164F RID: 5711
		public static int zombieModeTimer;

		// Token: 0x04001650 RID: 5712
		public static int shoppingTimer;

		// Token: 0x04001651 RID: 5713
		public static int holdItemTimer;

		// Token: 0x04001652 RID: 5714
		public static int itemToHold;

		// Token: 0x04001653 RID: 5715
		public static int newMapPosition;

		// Token: 0x04001654 RID: 5716
		public static int playerInvincibleTimer;

		// Token: 0x04001655 RID: 5717
		public static int screenFlash;

		// Token: 0x04001656 RID: 5718
		public static int gopherTrainPosition;

		// Token: 0x04001657 RID: 5719
		public static int endCutsceneTimer;

		// Token: 0x04001658 RID: 5720
		public static int endCutscenePhase;

		// Token: 0x04001659 RID: 5721
		public static int startTimer;

		// Token: 0x0400165A RID: 5722
		public static float deathTimer;

		// Token: 0x0400165B RID: 5723
		public static bool onStartMenu;

		// Token: 0x0400165C RID: 5724
		public static bool shopping;

		// Token: 0x0400165D RID: 5725
		public static bool gopherRunning;

		// Token: 0x0400165E RID: 5726
		public static bool store;

		// Token: 0x0400165F RID: 5727
		public static bool merchantLeaving;

		// Token: 0x04001660 RID: 5728
		public static bool merchantArriving;

		// Token: 0x04001661 RID: 5729
		public static bool merchantShopOpen;

		// Token: 0x04001662 RID: 5730
		public static bool waitingForPlayerToMoveDownAMap;

		// Token: 0x04001663 RID: 5731
		public static bool scrollingMap;

		// Token: 0x04001664 RID: 5732
		public static bool hasGopherAppeared;

		// Token: 0x04001665 RID: 5733
		public static bool shootoutLevel;

		// Token: 0x04001666 RID: 5734
		public static bool gopherTrain;

		// Token: 0x04001667 RID: 5735
		public static bool playerJumped;

		// Token: 0x04001668 RID: 5736
		public static bool endCutscene;

		// Token: 0x04001669 RID: 5737
		public static bool gameOver;

		// Token: 0x0400166A RID: 5738
		public static bool playingWithAbigail;

		// Token: 0x0400166B RID: 5739
		public static bool beatLevelWithAbigail;

		// Token: 0x0400166C RID: 5740
		public Dictionary<Rectangle, int> storeItems = new Dictionary<Rectangle, int>();

		// Token: 0x0400166D RID: 5741
		public bool quit;

		// Token: 0x0400166E RID: 5742
		public bool died;

		// Token: 0x0400166F RID: 5743
		public static Rectangle gopherBox;

		// Token: 0x04001670 RID: 5744
		public Point gopherMotion;

		// Token: 0x04001671 RID: 5745
		private static ICue overworldSong;

		// Token: 0x04001672 RID: 5746
		private static ICue outlawSong;

		// Token: 0x04001673 RID: 5747
		private static ICue zombieSong;

		// Token: 0x04001674 RID: 5748
		protected Dictionary<AbigailGame.GameKeys, List<Keys>> _binds;

		// Token: 0x04001675 RID: 5749
		protected HashSet<AbigailGame.GameKeys> _buttonHeldState = new HashSet<AbigailGame.GameKeys>();

		// Token: 0x04001676 RID: 5750
		protected Dictionary<AbigailGame.GameKeys, int> _buttonHeldFrames;

		// Token: 0x04001677 RID: 5751
		private int player2FootstepSoundTimer;

		// Token: 0x04001678 RID: 5752
		public AbigailGame.CowboyMonster targetMonster;

		// Token: 0x0200058C RID: 1420
		// (Invoke) Token: 0x060041C8 RID: 16840
		public delegate void behaviorAfterMotionPause();

		// Token: 0x0200058D RID: 1421
		public enum GameKeys
		{
			// Token: 0x04002BDE RID: 11230
			MoveLeft,
			// Token: 0x04002BDF RID: 11231
			MoveRight,
			// Token: 0x04002BE0 RID: 11232
			MoveUp,
			// Token: 0x04002BE1 RID: 11233
			MoveDown,
			// Token: 0x04002BE2 RID: 11234
			ShootLeft,
			// Token: 0x04002BE3 RID: 11235
			ShootRight,
			// Token: 0x04002BE4 RID: 11236
			ShootUp,
			// Token: 0x04002BE5 RID: 11237
			ShootDown,
			// Token: 0x04002BE6 RID: 11238
			UsePowerup,
			// Token: 0x04002BE7 RID: 11239
			SelectOption,
			// Token: 0x04002BE8 RID: 11240
			Exit,
			// Token: 0x04002BE9 RID: 11241
			MAX
		}

		// Token: 0x0200058E RID: 1422
		public class CowboyPowerup
		{
			// Token: 0x060041CB RID: 16843 RVA: 0x00308F2E File Offset: 0x0030712E
			public CowboyPowerup(int which, Point position, int duration)
			{
				this.which = which;
				this.position = position;
				this.duration = duration;
			}

			// Token: 0x060041CC RID: 16844 RVA: 0x00308F4C File Offset: 0x0030714C
			public void draw(SpriteBatch b)
			{
				if (this.duration > 2000 || this.duration / 200 % 2 == 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y + this.yOffset), new Rectangle?(new Rectangle(272 + this.which * 16, 1808, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
				}
			}

			// Token: 0x04002BEA RID: 11242
			public int which;

			// Token: 0x04002BEB RID: 11243
			public Point position;

			// Token: 0x04002BEC RID: 11244
			public int duration;

			// Token: 0x04002BED RID: 11245
			public float yOffset;
		}

		// Token: 0x0200058F RID: 1423
		public class JOTPKProgress : INetObject<NetFields>
		{
			// Token: 0x170004E7 RID: 1255
			// (get) Token: 0x060041CD RID: 16845 RVA: 0x00309001 File Offset: 0x00307201
			public NetFields NetFields { get; } = new NetFields("JOTPKProgress");

			// Token: 0x060041CE RID: 16846 RVA: 0x0030900C File Offset: 0x0030720C
			public JOTPKProgress()
			{
				this.NetFields.SetOwner(this).AddField(this.bulletDamage, "bulletDamage").AddField(this.runSpeedLevel, "runSpeedLevel").AddField(this.fireSpeedLevel, "fireSpeedLevel").AddField(this.ammoLevel, "ammoLevel").AddField(this.lives, "lives").AddField(this.coins, "coins").AddField(this.score, "score").AddField(this.died, "died").AddField(this.spreadPistol, "spreadPistol").AddField(this.whichRound, "whichRound").AddField(this.whichWave, "whichWave").AddField(this.heldItem, "heldItem").AddField(this.world, "world").AddField(this.waveTimer, "waveTimer").AddField(this.monsterChances, "monsterChances");
			}

			// Token: 0x04002BEE RID: 11246
			public NetInt bulletDamage = new NetInt();

			// Token: 0x04002BEF RID: 11247
			public NetInt fireSpeedLevel = new NetInt();

			// Token: 0x04002BF0 RID: 11248
			public NetInt ammoLevel = new NetInt();

			// Token: 0x04002BF1 RID: 11249
			public NetBool spreadPistol = new NetBool();

			// Token: 0x04002BF2 RID: 11250
			public NetInt runSpeedLevel = new NetInt();

			// Token: 0x04002BF3 RID: 11251
			public NetInt lives = new NetInt();

			// Token: 0x04002BF4 RID: 11252
			public NetInt coins = new NetInt();

			// Token: 0x04002BF5 RID: 11253
			public NetInt score = new NetInt();

			// Token: 0x04002BF6 RID: 11254
			public NetBool died = new NetBool();

			// Token: 0x04002BF7 RID: 11255
			public NetInt whichRound = new NetInt();

			// Token: 0x04002BF8 RID: 11256
			public NetInt whichWave = new NetInt();

			// Token: 0x04002BF9 RID: 11257
			public NetInt heldItem = new NetInt(-100);

			// Token: 0x04002BFA RID: 11258
			public NetInt world = new NetInt();

			// Token: 0x04002BFB RID: 11259
			public NetInt waveTimer = new NetInt();

			// Token: 0x04002BFC RID: 11260
			public NetList<Vector2, NetVector2> monsterChances = new NetList<Vector2, NetVector2>();
		}

		// Token: 0x02000590 RID: 1424
		public class CowboyBullet
		{
			// Token: 0x060041CF RID: 16847 RVA: 0x003091D3 File Offset: 0x003073D3
			public CowboyBullet(Point position, Point motion, int damage)
			{
				this.position = position;
				this.motion = motion;
				this.damage = damage;
			}

			// Token: 0x060041D0 RID: 16848 RVA: 0x003091F0 File Offset: 0x003073F0
			public CowboyBullet(Point position, int direction, int damage)
			{
				this.position = position;
				switch (direction)
				{
				case 0:
					this.motion = new Point(0, -8);
					break;
				case 1:
					this.motion = new Point(8, 0);
					break;
				case 2:
					this.motion = new Point(0, 8);
					break;
				case 3:
					this.motion = new Point(-8, 0);
					break;
				}
				this.damage = damage;
			}

			// Token: 0x04002BFE RID: 11262
			public Point position;

			// Token: 0x04002BFF RID: 11263
			public Point motion;

			// Token: 0x04002C00 RID: 11264
			public int damage;
		}

		// Token: 0x02000591 RID: 1425
		public class CowboyMonster
		{
			// Token: 0x060041D1 RID: 16849 RVA: 0x00309268 File Offset: 0x00307468
			public CowboyMonster(int which, int health, int speed, Point position)
			{
				this.health = health;
				this.type = which;
				this.speed = speed;
				this.position = new Rectangle(position.X, position.Y, AbigailGame.TileSize, AbigailGame.TileSize);
				this.uninterested = (Game1.random.NextDouble() < 0.25);
			}

			// Token: 0x060041D2 RID: 16850 RVA: 0x003092E4 File Offset: 0x003074E4
			public CowboyMonster(int which, Point position)
			{
				this.type = which;
				this.position = new Rectangle(position.X, position.Y, AbigailGame.TileSize, AbigailGame.TileSize);
				switch (this.type)
				{
				case 0:
					this.speed = 2;
					this.health = 1;
					this.uninterested = (Game1.random.NextDouble() < 0.25);
					if (this.uninterested)
					{
						this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
					}
					break;
				case 1:
					this.speed = 2;
					this.health = 1;
					this.flyer = true;
					break;
				case 2:
					this.speed = 1;
					this.health = 3;
					break;
				case 3:
					this.health = 6;
					this.speed = 1;
					this.uninterested = (Game1.random.NextDouble() < 0.25);
					if (this.uninterested)
					{
						this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
					}
					break;
				case 4:
					this.health = 3;
					this.speed = 3;
					this.flyer = true;
					break;
				case 5:
					this.speed = 3;
					this.health = 2;
					break;
				case 6:
				{
					this.speed = 3;
					this.health = 2;
					int tries = 0;
					do
					{
						this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
						tries++;
					}
					while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries < 10);
					break;
				}
				}
				this.oppositeMotionGuy = Game1.random.NextBool();
			}

			// Token: 0x060041D3 RID: 16851 RVA: 0x003094F4 File Offset: 0x003076F4
			public virtual void draw(SpriteBatch b)
			{
				if (this.type != 6 || !this.special)
				{
					if (!this.invisible)
					{
						if (this.flashColorTimer > 0f)
						{
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(352 + this.type * 16, 1696, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
						}
						else
						{
							b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(352 + (this.type * 2 + ((this.movementAnimationTimer < 250f) ? 1 : 0)) * 16, 1712, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
						}
						if (AbigailGame.monsterConfusionTimer > 0)
						{
							b.DrawString(Game1.smallFont, "?", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f, (float)(this.position.Y - AbigailGame.TileSize / 2)), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)this.position.Y / 10000f);
							b.DrawString(Game1.smallFont, "?", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f + 1f, (float)(this.position.Y - AbigailGame.TileSize / 2)), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)this.position.Y / 10000f);
							b.DrawString(Game1.smallFont, "?", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f - 1f, (float)(this.position.Y - AbigailGame.TileSize / 2)), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)this.position.Y / 10000f);
						}
					}
					return;
				}
				if (this.flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(480, 1696, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
					return;
				}
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(576, 1712, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
			}

			// Token: 0x060041D4 RID: 16852 RVA: 0x0030990C File Offset: 0x00307B0C
			public virtual bool takeDamage(int damage)
			{
				this.health -= damage;
				this.health = Math.Max(0, this.health);
				if (this.health <= 0)
				{
					return true;
				}
				Game1.playSound("cowboy_monsterhit", null);
				this.flashColor = Color.Red;
				this.flashColorTimer = 100f;
				return false;
			}

			// Token: 0x060041D5 RID: 16853 RVA: 0x00309970 File Offset: 0x00307B70
			public virtual int getLootDrop()
			{
				if (this.type == 6 && this.special)
				{
					return -1;
				}
				if (Game1.random.NextDouble() < 0.05)
				{
					if (this.type != 0 && Game1.random.NextDouble() < 0.1)
					{
						return 1;
					}
					if (Game1.random.NextDouble() < 0.01)
					{
						return 1;
					}
					return 0;
				}
				else
				{
					if (Game1.random.NextDouble() >= 0.05)
					{
						return -1;
					}
					if (Game1.random.NextDouble() < 0.15)
					{
						return Game1.random.Next(6, 8);
					}
					if (Game1.random.NextDouble() < 0.07)
					{
						return 10;
					}
					int loot = Game1.random.Next(2, 10);
					if (loot == 5 && Game1.random.NextDouble() < 0.4)
					{
						loot = Game1.random.Next(2, 10);
					}
					return loot;
				}
			}

			// Token: 0x060041D6 RID: 16854 RVA: 0x00309A64 File Offset: 0x00307C64
			public virtual bool move(Vector2 playerPosition, GameTime time)
			{
				this.movementAnimationTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.movementAnimationTimer <= 0f)
				{
					this.movementAnimationTimer = (float)Math.Max(100, 500 - this.speed * 50);
				}
				if (this.flashColorTimer > 0f)
				{
					this.flashColorTimer -= (float)time.ElapsedGameTime.Milliseconds;
					return false;
				}
				if (AbigailGame.monsterConfusionTimer > 0)
				{
					return false;
				}
				if (AbigailGame.shopping)
				{
					AbigailGame.shoppingTimer -= time.ElapsedGameTime.Milliseconds;
					if (AbigailGame.shoppingTimer <= 0)
					{
						AbigailGame.shoppingTimer = 100;
					}
				}
				this.ticksSinceLastMovement++;
				switch (this.type)
				{
				case 0:
				case 2:
				case 3:
				case 5:
				case 6:
				{
					if (this.type == 6)
					{
						if (this.special || this.invisible)
						{
							break;
						}
						if (this.ticksSinceLastMovement > 20)
						{
							int tries = 0;
							do
							{
								this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
								tries++;
								if (!AbigailGame.isCollidingWithMap(this.targetPosition))
								{
									break;
								}
							}
							while (tries < 5);
						}
					}
					else if (this.ticksSinceLastMovement > 20)
					{
						int tries2 = 0;
						do
						{
							this.oppositeMotionGuy = !this.oppositeMotionGuy;
							this.targetPosition = new Point(Game1.random.Next(this.position.X - AbigailGame.TileSize * 2, this.position.X + AbigailGame.TileSize * 2), Game1.random.Next(this.position.Y - AbigailGame.TileSize * 2, this.position.Y + AbigailGame.TileSize * 2));
							tries2++;
						}
						while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries2 < 5);
					}
					Point point = this.targetPosition;
					Vector2 target = (!this.targetPosition.Equals(Point.Zero)) ? new Vector2((float)this.targetPosition.X, (float)this.targetPosition.Y) : playerPosition;
					if (AbigailGame.playingWithAbigail && target.Equals(playerPosition))
					{
						double distanceToPlayer = Math.Sqrt(Math.Pow((double)((float)this.position.X - target.X), 2.0) - Math.Pow((double)((float)this.position.Y - target.Y), 2.0));
						if (Math.Sqrt(Math.Pow((double)((float)this.position.X - AbigailGame.player2Position.X), 2.0) - Math.Pow((double)((float)this.position.Y - AbigailGame.player2Position.Y), 2.0)) < distanceToPlayer)
						{
							target = AbigailGame.player2Position;
						}
					}
					if (AbigailGame.gopherRunning)
					{
						target = new Vector2((float)AbigailGame.gopherBox.X, (float)AbigailGame.gopherBox.Y);
					}
					if (Game1.random.NextDouble() < 0.001)
					{
						this.oppositeMotionGuy = !this.oppositeMotionGuy;
					}
					if ((this.type == 6 && !this.oppositeMotionGuy) || Math.Abs(target.X - (float)this.position.X) > Math.Abs(target.Y - (float)this.position.Y))
					{
						if (target.X + (float)this.speed < (float)this.position.X && (this.movedLastTurn || this.movementDirection != 3))
						{
							this.movementDirection = 3;
						}
						else if (target.X > (float)(this.position.X + this.speed) && (this.movedLastTurn || this.movementDirection != 1))
						{
							this.movementDirection = 1;
						}
						else if (target.Y > (float)(this.position.Y + this.speed) && (this.movedLastTurn || this.movementDirection != 2))
						{
							this.movementDirection = 2;
						}
						else if (target.Y + (float)this.speed < (float)this.position.Y && (this.movedLastTurn || this.movementDirection != 0))
						{
							this.movementDirection = 0;
						}
					}
					else if (target.Y > (float)(this.position.Y + this.speed) && (this.movedLastTurn || this.movementDirection != 2))
					{
						this.movementDirection = 2;
					}
					else if (target.Y + (float)this.speed < (float)this.position.Y && (this.movedLastTurn || this.movementDirection != 0))
					{
						this.movementDirection = 0;
					}
					else if (target.X + (float)this.speed < (float)this.position.X && (this.movedLastTurn || this.movementDirection != 3))
					{
						this.movementDirection = 3;
					}
					else if (target.X > (float)(this.position.X + this.speed) && (this.movedLastTurn || this.movementDirection != 1))
					{
						this.movementDirection = 1;
					}
					this.movedLastTurn = false;
					Rectangle attemptedPosition = this.position;
					switch (this.movementDirection)
					{
					case 0:
						attemptedPosition.Y -= this.speed;
						break;
					case 1:
						attemptedPosition.X += this.speed;
						break;
					case 2:
						attemptedPosition.Y += this.speed;
						break;
					case 3:
						attemptedPosition.X -= this.speed;
						break;
					}
					if (AbigailGame.zombieModeTimer > 0)
					{
						attemptedPosition.X = this.position.X - (attemptedPosition.X - this.position.X);
						attemptedPosition.Y = this.position.Y - (attemptedPosition.Y - this.position.Y);
					}
					if (this.type == 2)
					{
						for (int i = AbigailGame.monsters.Count - 1; i >= 0; i--)
						{
							if (AbigailGame.monsters[i].type == 6 && AbigailGame.monsters[i].special && AbigailGame.monsters[i].position.Intersects(attemptedPosition))
							{
								AbigailGame.addGuts(AbigailGame.monsters[i].position.Location, AbigailGame.monsters[i].type);
								Game1.playSound("Cowboy_monsterDie", null);
								AbigailGame.monsters.RemoveAt(i);
							}
						}
					}
					if (!AbigailGame.isCollidingWithMapForMonsters(attemptedPosition) && !AbigailGame.isCollidingWithMonster(attemptedPosition, this) && AbigailGame.deathTimer <= 0f)
					{
						this.ticksSinceLastMovement = 0;
						this.position = attemptedPosition;
						this.movedLastTurn = true;
						if (this.position.Contains((int)target.X + AbigailGame.TileSize / 2, (int)target.Y + AbigailGame.TileSize / 2))
						{
							this.targetPosition = Point.Zero;
							int num = this.type;
							if (num != 0 && num != 3)
							{
								if (num == 6)
								{
									if (!this.invisible)
									{
										AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(352, 1728, 16, 16), 60f, 3, 0, new Vector2((float)this.position.X, (float)this.position.Y) + AbigailGame.topLeftScreenCoordinate, false, false, (float)this.position.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
										{
											endFunction = new TemporaryAnimatedSprite.endBehavior(this.spikeyEndBehavior)
										});
										this.invisible = true;
									}
								}
							}
							else if (this.uninterested)
							{
								this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
								if (Game1.random.NextBool())
								{
									this.uninterested = false;
									this.targetPosition = Point.Zero;
								}
							}
						}
					}
					break;
				}
				case 1:
				case 4:
				{
					if (this.ticksSinceLastMovement > 20)
					{
						int tries3 = 0;
						do
						{
							this.oppositeMotionGuy = !this.oppositeMotionGuy;
							this.targetPosition = new Point(Game1.random.Next(this.position.X - AbigailGame.TileSize * 2, this.position.X + AbigailGame.TileSize * 2), Game1.random.Next(this.position.Y - AbigailGame.TileSize * 2, this.position.Y + AbigailGame.TileSize * 2));
							tries3++;
						}
						while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries3 < 5);
					}
					Point point2 = this.targetPosition;
					Vector2 target = (!this.targetPosition.Equals(Point.Zero)) ? new Vector2((float)this.targetPosition.X, (float)this.targetPosition.Y) : playerPosition;
					Vector2 targetToFly = Utility.getVelocityTowardPoint(this.position.Location, target + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), (float)this.speed);
					float accelerationMultiplyer = (targetToFly.X != 0f && targetToFly.Y != 0f) ? 1.5f : 1f;
					if (targetToFly.X > this.acceleration.X)
					{
						this.acceleration.X = this.acceleration.X + 0.1f * accelerationMultiplyer;
					}
					if (targetToFly.X < this.acceleration.X)
					{
						this.acceleration.X = this.acceleration.X - 0.1f * accelerationMultiplyer;
					}
					if (targetToFly.Y > this.acceleration.Y)
					{
						this.acceleration.Y = this.acceleration.Y + 0.1f * accelerationMultiplyer;
					}
					if (targetToFly.Y < this.acceleration.Y)
					{
						this.acceleration.Y = this.acceleration.Y - 0.1f * accelerationMultiplyer;
					}
					if (!AbigailGame.isCollidingWithMonster(new Rectangle(this.position.X + (int)Math.Ceiling((double)this.acceleration.X), this.position.Y + (int)Math.Ceiling((double)this.acceleration.Y), AbigailGame.TileSize, AbigailGame.TileSize), this) && AbigailGame.deathTimer <= 0f)
					{
						this.ticksSinceLastMovement = 0;
						this.position.X = this.position.X + (int)Math.Ceiling((double)this.acceleration.X);
						this.position.Y = this.position.Y + (int)Math.Ceiling((double)this.acceleration.Y);
						if (this.position.Contains((int)target.X + AbigailGame.TileSize / 2, (int)target.Y + AbigailGame.TileSize / 2))
						{
							this.targetPosition = Point.Zero;
						}
					}
					break;
				}
				}
				return false;
			}

			// Token: 0x060041D7 RID: 16855 RVA: 0x0030A5AD File Offset: 0x003087AD
			public void spikeyEndBehavior(int extraInfo)
			{
				this.invisible = false;
				this.health += 5;
				this.special = true;
			}

			// Token: 0x04002C01 RID: 11265
			public const int MonsterAnimationDelay = 500;

			// Token: 0x04002C02 RID: 11266
			public int health;

			// Token: 0x04002C03 RID: 11267
			public int type;

			// Token: 0x04002C04 RID: 11268
			public int speed;

			// Token: 0x04002C05 RID: 11269
			public float movementAnimationTimer;

			// Token: 0x04002C06 RID: 11270
			public Rectangle position;

			// Token: 0x04002C07 RID: 11271
			public int movementDirection;

			// Token: 0x04002C08 RID: 11272
			public bool movedLastTurn;

			// Token: 0x04002C09 RID: 11273
			public bool oppositeMotionGuy;

			// Token: 0x04002C0A RID: 11274
			public bool invisible;

			// Token: 0x04002C0B RID: 11275
			public bool special;

			// Token: 0x04002C0C RID: 11276
			public bool uninterested;

			// Token: 0x04002C0D RID: 11277
			public bool flyer;

			// Token: 0x04002C0E RID: 11278
			public Color tint = Color.White;

			// Token: 0x04002C0F RID: 11279
			public Color flashColor = Color.Red;

			// Token: 0x04002C10 RID: 11280
			public float flashColorTimer;

			// Token: 0x04002C11 RID: 11281
			public int ticksSinceLastMovement;

			// Token: 0x04002C12 RID: 11282
			public Vector2 acceleration;

			// Token: 0x04002C13 RID: 11283
			private Point targetPosition;
		}

		// Token: 0x02000592 RID: 1426
		public class Dracula : AbigailGame.CowboyMonster
		{
			// Token: 0x060041D8 RID: 16856 RVA: 0x0030A5CC File Offset: 0x003087CC
			public Dracula() : base(-2, new Point(8 * AbigailGame.TileSize, 8 * AbigailGame.TileSize))
			{
				this.homePosition = this.position.Location;
				this.position.Y = this.position.Y + AbigailGame.TileSize * 4;
				this.health = 350;
				this.fullHealth = this.health;
				this.phase = -1;
				this.phaseInternalTimer = 4000;
				this.speed = 2;
			}

			// Token: 0x060041D9 RID: 16857 RVA: 0x0030A654 File Offset: 0x00308854
			public override void draw(SpriteBatch b)
			{
				if (this.phase != -1)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y + 16 * AbigailGame.TileSize + 3, (int)((float)(16 * AbigailGame.TileSize) * ((float)this.health / (float)this.fullHealth)), AbigailGame.TileSize / 3), new Color(188, 51, 74));
				}
				if (this.flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(464, 1696, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f);
					return;
				}
				int num = this.phase;
				if (num == -1 || num - 1 <= 2)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(592 + this.phaseInternalTimer / 100 % 3 * 16, 1760, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f);
					if (this.phase == -1)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)(this.position.Y + AbigailGame.TileSize) + (float)Math.Sin((double)((float)this.phaseInternalTimer / 1000f)) * 3f), new Rectangle?(new Rectangle(528, 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f);
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X - AbigailGame.TileSize / 2), (float)(this.position.Y - AbigailGame.TileSize * 2)), new Rectangle?(new Rectangle(608, 1728, 32, 32)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f);
						return;
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(592 + this.phaseInternalTimer / 100 % 2 * 16, 1712, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f);
				}
			}

			// Token: 0x060041DA RID: 16858 RVA: 0x0030A97D File Offset: 0x00308B7D
			public override int getLootDrop()
			{
				return -1;
			}

			// Token: 0x060041DB RID: 16859 RVA: 0x0030A980 File Offset: 0x00308B80
			public override bool takeDamage(int damage)
			{
				if (this.phase == -1)
				{
					return false;
				}
				this.health -= damage;
				if (this.health < 0)
				{
					return true;
				}
				this.flashColorTimer = 100f;
				Game1.playSound("cowboy_monsterhit", null);
				return false;
			}

			// Token: 0x060041DC RID: 16860 RVA: 0x0030A9D4 File Offset: 0x00308BD4
			public override bool move(Vector2 playerPosition, GameTime time)
			{
				if (this.flashColorTimer > 0f)
				{
					this.flashColorTimer -= (float)time.ElapsedGameTime.Milliseconds;
				}
				this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
				switch (this.phase)
				{
				case -1:
					if (this.phaseInternalTimer <= 0)
					{
						this.phaseInternalCounter = 0;
						Game1.playSound("cowboy_boss", out AbigailGame.outlawSong);
						this.phase = 0;
					}
					break;
				case 0:
					if (this.phaseInternalCounter == 0)
					{
						this.phaseInternalCounter++;
						this.phaseInternalTimer = Game1.random.Next(3000, 7000);
					}
					if (this.phaseInternalTimer < 0)
					{
						this.phaseInternalCounter = 0;
						this.phase = Game1.random.Next(1, 4);
						this.phaseInternalTimer = 9999;
					}
					if (AbigailGame.deathTimer <= 0f)
					{
						int movementDirection = -1;
						if (Math.Abs(playerPosition.X - (float)this.position.X) > Math.Abs(playerPosition.Y - (float)this.position.Y))
						{
							if (playerPosition.X + (float)this.speed < (float)this.position.X)
							{
								movementDirection = 3;
							}
							else if (playerPosition.X > (float)(this.position.X + this.speed))
							{
								movementDirection = 1;
							}
							else if (playerPosition.Y > (float)(this.position.Y + this.speed))
							{
								movementDirection = 2;
							}
							else if (playerPosition.Y + (float)this.speed < (float)this.position.Y)
							{
								movementDirection = 0;
							}
						}
						else if (playerPosition.Y > (float)(this.position.Y + this.speed))
						{
							movementDirection = 2;
						}
						else if (playerPosition.Y + (float)this.speed < (float)this.position.Y)
						{
							movementDirection = 0;
						}
						else if (playerPosition.X + (float)this.speed < (float)this.position.X)
						{
							movementDirection = 3;
						}
						else if (playerPosition.X > (float)(this.position.X + this.speed))
						{
							movementDirection = 1;
						}
						Rectangle attemptedPosition = this.position;
						switch (movementDirection)
						{
						case 0:
							attemptedPosition.Y -= this.speed;
							break;
						case 1:
							attemptedPosition.X += this.speed;
							break;
						case 2:
							attemptedPosition.Y += this.speed;
							break;
						case 3:
							attemptedPosition.X -= this.speed;
							break;
						}
						attemptedPosition.X = this.position.X - (attemptedPosition.X - this.position.X);
						attemptedPosition.Y = this.position.Y - (attemptedPosition.Y - this.position.Y);
						if (!AbigailGame.isCollidingWithMapForMonsters(attemptedPosition) && !AbigailGame.isCollidingWithMonster(attemptedPosition, this))
						{
							this.position = attemptedPosition;
						}
						this.shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.shootTimer < 0)
						{
							Vector2 trajectory = Utility.getVelocityTowardPoint(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y), playerPosition + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), 8f);
							if (AbigailGame.playerMovementDirections.Count > 0)
							{
								trajectory = Utility.getTranslatedVector2(trajectory, AbigailGame.playerMovementDirections.Last<int>(), 3f);
							}
							AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y + AbigailGame.TileSize / 2), new Point((int)trajectory.X, (int)trajectory.Y), 1));
							this.shootTimer = 250;
							Game1.playSound("Cowboy_gunshot", null);
						}
					}
					break;
				case 1:
					switch (this.phaseInternalCounter)
					{
					case 0:
					{
						Point oldPosition = this.position.Location;
						if (this.position.X > this.homePosition.X + 6)
						{
							this.position.X = this.position.X - 6;
						}
						else if (this.position.X < this.homePosition.X - 6)
						{
							this.position.X = this.position.X + 6;
						}
						if (this.position.Y > this.homePosition.Y + 6)
						{
							this.position.Y = this.position.Y - 6;
						}
						else if (this.position.Y < this.homePosition.Y - 6)
						{
							this.position.Y = this.position.Y + 6;
						}
						if (this.position.Location.Equals(oldPosition))
						{
							this.phaseInternalCounter++;
							this.phaseInternalTimer = 1500;
						}
						break;
					}
					case 1:
						if (this.phaseInternalTimer < 0)
						{
							this.phaseInternalCounter++;
							this.phaseInternalTimer = 2000;
							this.shootTimer = 200;
							this.fireSpread(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y + AbigailGame.TileSize / 2), 0.0);
						}
						break;
					case 2:
						this.shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.shootTimer < 0)
						{
							this.fireSpread(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y + AbigailGame.TileSize / 2), 0.0);
							this.shootTimer = 200;
						}
						if (this.phaseInternalTimer < 0)
						{
							this.phaseInternalCounter++;
							this.phaseInternalTimer = 500;
						}
						break;
					case 3:
						if (this.phaseInternalTimer < 0)
						{
							this.phaseInternalTimer = 2000;
							this.shootTimer = 200;
							this.phaseInternalCounter++;
							Vector2 trajectory2 = Utility.getVelocityTowardPoint(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y), playerPosition + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), 8f);
							AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y + AbigailGame.TileSize / 2), new Point((int)trajectory2.X, (int)trajectory2.Y), 1));
							Game1.playSound("Cowboy_gunshot", null);
						}
						break;
					case 4:
						this.shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.shootTimer < 0)
						{
							Vector2 trajectory3 = Utility.getVelocityTowardPoint(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y), playerPosition + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), 8f);
							trajectory3.X += (float)Game1.random.Next(-1, 2);
							trajectory3.Y += (float)Game1.random.Next(-1, 2);
							AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y + AbigailGame.TileSize / 2), new Point((int)trajectory3.X, (int)trajectory3.Y), 1));
							Game1.playSound("Cowboy_gunshot", null);
							this.shootTimer = 200;
						}
						if (this.phaseInternalTimer < 0)
						{
							if (Game1.random.NextDouble() < 0.4)
							{
								this.phase = 0;
								this.phaseInternalCounter = 0;
							}
							else
							{
								this.phaseInternalTimer = 500;
								this.phaseInternalCounter = 1;
							}
						}
						break;
					}
					break;
				case 2:
				case 3:
				{
					int num = this.phaseInternalCounter;
					if (num != 0)
					{
						if (num == 1)
						{
							if (this.phaseInternalTimer < 0)
							{
								this.summonEnemies(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y + AbigailGame.TileSize / 2), Game1.random.Next(0, 5));
								if (Game1.random.NextDouble() < 0.4)
								{
									this.phase = 0;
									this.phaseInternalCounter = 0;
								}
								else
								{
									this.phaseInternalTimer = 2000;
								}
							}
						}
					}
					else
					{
						Point oldPosition2 = this.position.Location;
						if (this.position.X > this.homePosition.X + 6)
						{
							this.position.X = this.position.X - 6;
						}
						else if (this.position.X < this.homePosition.X - 6)
						{
							this.position.X = this.position.X + 6;
						}
						if (this.position.Y > this.homePosition.Y + 6)
						{
							this.position.Y = this.position.Y - 6;
						}
						else if (this.position.Y < this.homePosition.Y - 6)
						{
							this.position.Y = this.position.Y + 6;
						}
						if (this.position.Location.Equals(oldPosition2))
						{
							this.phaseInternalCounter++;
							this.phaseInternalTimer = 1500;
						}
					}
					break;
				}
				}
				return false;
			}

			// Token: 0x060041DD RID: 16861 RVA: 0x0030B3E4 File Offset: 0x003095E4
			public void fireSpread(Point origin, double offsetAngle)
			{
				foreach (Vector2 p in Utility.getSurroundingTileLocationsArray(new Vector2((float)origin.X, (float)origin.Y)))
				{
					Vector2 trajectory = Utility.getVelocityTowardPoint(origin, p, 6f);
					if (offsetAngle > 0.0)
					{
						offsetAngle /= 2.0;
						trajectory.X = (float)(Math.Cos(offsetAngle) * (double)(p.X - (float)origin.X) - Math.Sin(offsetAngle) * (double)(p.Y - (float)origin.Y) + (double)origin.X);
						trajectory.Y = (float)(Math.Sin(offsetAngle) * (double)(p.X - (float)origin.X) + Math.Cos(offsetAngle) * (double)(p.Y - (float)origin.Y) + (double)origin.Y);
						trajectory = Utility.getVelocityTowardPoint(origin, trajectory, 8f);
					}
					AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(origin, new Point((int)trajectory.X, (int)trajectory.Y), 1));
				}
				Game1.playSound("Cowboy_gunshot", null);
			}

			// Token: 0x060041DE RID: 16862 RVA: 0x0030B510 File Offset: 0x00309710
			public void summonEnemies(Point origin, int which)
			{
				if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X - AbigailGame.TileSize - AbigailGame.TileSize / 2, origin.Y, AbigailGame.TileSize, AbigailGame.TileSize), null))
				{
					AbigailGame.monsters.Add(new AbigailGame.CowboyMonster(which, new Point(origin.X - AbigailGame.TileSize - AbigailGame.TileSize / 2, origin.Y)));
				}
				if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X + AbigailGame.TileSize + AbigailGame.TileSize / 2, origin.Y, AbigailGame.TileSize, AbigailGame.TileSize), null))
				{
					AbigailGame.monsters.Add(new AbigailGame.CowboyMonster(which, new Point(origin.X + AbigailGame.TileSize + AbigailGame.TileSize / 2, origin.Y)));
				}
				if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X, origin.Y + AbigailGame.TileSize + AbigailGame.TileSize / 2, AbigailGame.TileSize, AbigailGame.TileSize), null))
				{
					AbigailGame.monsters.Add(new AbigailGame.CowboyMonster(which, new Point(origin.X, origin.Y + AbigailGame.TileSize + AbigailGame.TileSize / 2)));
				}
				if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X, origin.Y - AbigailGame.TileSize - AbigailGame.TileSize * 3 / 4, AbigailGame.TileSize, AbigailGame.TileSize), null))
				{
					AbigailGame.monsters.Add(new AbigailGame.CowboyMonster(which, new Point(origin.X, origin.Y - AbigailGame.TileSize - AbigailGame.TileSize * 3 / 4)));
				}
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(origin.X - AbigailGame.TileSize - AbigailGame.TileSize / 2), (float)origin.Y), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(origin.X + AbigailGame.TileSize + AbigailGame.TileSize / 2), (float)origin.Y), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)origin.X, (float)(origin.Y - AbigailGame.TileSize - AbigailGame.TileSize * 3 / 4)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2((float)origin.X, (float)(origin.Y + AbigailGame.TileSize + AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				Game1.playSound("Cowboy_monsterDie", null);
			}

			// Token: 0x04002C14 RID: 11284
			public const int gloatingPhase = -1;

			// Token: 0x04002C15 RID: 11285
			public const int walkRandomlyAndShootPhase = 0;

			// Token: 0x04002C16 RID: 11286
			public const int spreadShotPhase = 1;

			// Token: 0x04002C17 RID: 11287
			public const int summonDemonPhase = 2;

			// Token: 0x04002C18 RID: 11288
			public const int summonMummyPhase = 3;

			// Token: 0x04002C19 RID: 11289
			public int phase = -1;

			// Token: 0x04002C1A RID: 11290
			public int phaseInternalTimer;

			// Token: 0x04002C1B RID: 11291
			public int phaseInternalCounter;

			// Token: 0x04002C1C RID: 11292
			public int shootTimer;

			// Token: 0x04002C1D RID: 11293
			public int fullHealth;

			// Token: 0x04002C1E RID: 11294
			public Point homePosition;
		}

		// Token: 0x02000593 RID: 1427
		public class Outlaw : AbigailGame.CowboyMonster
		{
			// Token: 0x060041DF RID: 16863 RVA: 0x0030B907 File Offset: 0x00309B07
			public Outlaw(Point position, int health) : base(-1, position)
			{
				this.homePosition = position;
				this.health = health;
				this.fullHealth = health;
				this.phaseCountdown = 4000;
				this.phase = -1;
			}

			// Token: 0x060041E0 RID: 16864 RVA: 0x0030B938 File Offset: 0x00309B38
			public override void draw(SpriteBatch b)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y + 16 * AbigailGame.TileSize + 3, (int)((float)(16 * AbigailGame.TileSize) * ((float)this.health / (float)this.fullHealth)), AbigailGame.TileSize / 3), new Color(188, 51, 74));
				if (this.flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(496, 1696, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
					return;
				}
				int num = this.phase;
				if (num - -1 <= 1)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(560 + ((this.phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
					if (this.phase == -1 && this.phaseCountdown > 1000)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X - AbigailGame.TileSize / 2), (float)(this.position.Y - AbigailGame.TileSize * 2)), new Rectangle?(new Rectangle(576 + ((AbigailGame.whichWave > 5) ? 32 : 0), 1792, 32, 32)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
						return;
					}
				}
				else
				{
					if (this.phase == 3 && this.phaseInternalCounter == 2)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(560 + ((this.phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
						return;
					}
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2((float)this.position.X, (float)this.position.Y), new Rectangle?(new Rectangle(592 + ((this.phaseCountdown / 80 % 2 == 0) ? 16 : 0), 1776, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
				}
			}

			// Token: 0x060041E1 RID: 16865 RVA: 0x0030BCAC File Offset: 0x00309EAC
			public override bool move(Vector2 playerPosition, GameTime time)
			{
				if (this.flashColorTimer > 0f)
				{
					this.flashColorTimer -= (float)time.ElapsedGameTime.Milliseconds;
				}
				this.phaseCountdown -= time.ElapsedGameTime.Milliseconds;
				if (this.position.X > 17 * AbigailGame.TileSize || this.position.X < -AbigailGame.TileSize)
				{
					this.position.X = 16 * AbigailGame.TileSize / 2;
				}
				switch (this.phase)
				{
				case -1:
				case 0:
					if (this.phaseCountdown < 0)
					{
						this.phase = Game1.random.Next(1, 5);
						this.dartLeft = (playerPosition.X < (float)this.position.X);
						if (playerPosition.X > (float)(7 * AbigailGame.TileSize) && playerPosition.X < (float)(9 * AbigailGame.TileSize))
						{
							if (Game1.random.NextDouble() < 0.66 || this.phase == 2)
							{
								this.phase = 4;
							}
						}
						else if (this.phase == 4)
						{
							this.phase = 3;
						}
						this.phaseInternalCounter = 0;
						this.phaseInternalTimer = 0;
					}
					break;
				case 1:
				{
					int motion = this.dartLeft ? -3 : 3;
					if (Math.Abs(this.position.Location.X - this.homePosition.X + AbigailGame.TileSize / 2) < AbigailGame.TileSize * 2 + 12 && this.phaseInternalCounter == 0)
					{
						this.position.X = this.position.X + motion;
						if (this.position.X > 256)
						{
							this.phaseInternalCounter = 2;
						}
					}
					else if (this.phaseInternalCounter == 2)
					{
						this.position.X = this.position.X - motion;
						if (Math.Abs(this.position.X - this.homePosition.X) < 4)
						{
							this.position.X = this.homePosition.X;
							this.phase = 0;
							this.phaseCountdown = Game1.random.Next(1000, 2000);
						}
					}
					else
					{
						if (this.phaseInternalCounter == 0)
						{
							this.phaseInternalCounter++;
							this.phaseInternalTimer = Game1.random.Next(1000, 2000);
						}
						this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						this.shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.shootTimer < 0)
						{
							AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y - AbigailGame.TileSize / 2), new Point(Game1.random.Next(-2, 3), -8), 1));
							this.shootTimer = 150;
							Game1.playSound("Cowboy_gunshot", null);
						}
						if (this.phaseInternalTimer <= 0)
						{
							this.phaseInternalCounter++;
						}
					}
					break;
				}
				case 2:
				{
					int num = this.phaseInternalCounter;
					if (num != 0)
					{
						if (num == 2)
						{
							if (this.position.X < this.homePosition.X)
							{
								this.position.X = this.position.X + 4;
							}
							else
							{
								this.position.X = this.position.X - 4;
							}
							if (Math.Abs(this.position.X - this.homePosition.X) < 5)
							{
								this.position.X = this.homePosition.X;
								this.phase = 0;
								this.phaseCountdown = Game1.random.Next(1000, 2000);
							}
							return false;
						}
					}
					else
					{
						this.phaseInternalCounter++;
						this.phaseInternalTimer = Game1.random.Next(4000, 7000);
					}
					this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if ((float)this.position.X > playerPosition.X && (float)this.position.X - playerPosition.X > 3f)
					{
						this.position.X = this.position.X - 2;
					}
					else if ((float)this.position.X < playerPosition.X && playerPosition.X - (float)this.position.X > 3f)
					{
						this.position.X = this.position.X + 2;
					}
					this.shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shootTimer < 0)
					{
						AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y - AbigailGame.TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
						this.shootTimer = 250;
						if (this.fullHealth > 50)
						{
							this.shootTimer -= 50;
						}
						if (Game1.random.NextDouble() < 0.2)
						{
							this.shootTimer = 150;
						}
						Game1.playSound("Cowboy_gunshot", null);
					}
					if (this.phaseInternalTimer <= 0)
					{
						this.phaseInternalCounter++;
					}
					break;
				}
				case 3:
				{
					switch (this.phaseInternalCounter)
					{
					case 0:
						this.phaseInternalCounter++;
						this.phaseInternalTimer = Game1.random.Next(3000, 6500);
						goto IL_AA4;
					case 2:
						this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.phaseInternalTimer <= 0)
						{
							this.phaseInternalCounter++;
							goto IL_AA4;
						}
						goto IL_AA4;
					case 3:
						if (this.position.X < this.homePosition.X)
						{
							this.position.X = this.position.X + 4;
						}
						else
						{
							this.position.X = this.position.X - 4;
						}
						if (Math.Abs(this.position.X - this.homePosition.X) < 5)
						{
							this.position.X = this.homePosition.X;
							this.phase = 0;
							this.phaseCountdown = Game1.random.Next(1000, 2000);
							goto IL_AA4;
						}
						goto IL_AA4;
					}
					int motion = this.dartLeft ? -3 : 3;
					this.position.X = this.position.X + motion;
					if (this.position.X < AbigailGame.TileSize || this.position.X > 15 * AbigailGame.TileSize)
					{
						this.dartLeft = !this.dartLeft;
					}
					this.shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shootTimer < 0)
					{
						AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y - AbigailGame.TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
						this.shootTimer = 250;
						if (this.fullHealth > 50)
						{
							this.shootTimer -= 50;
						}
						if (Game1.random.NextDouble() < 0.2)
						{
							this.shootTimer = 150;
						}
						Game1.playSound("Cowboy_gunshot", null);
					}
					this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.phaseInternalTimer <= 0)
					{
						if (this.phase == 2)
						{
							this.phaseInternalCounter = 3;
						}
						else
						{
							this.phaseInternalTimer = 3000;
							this.phaseInternalCounter++;
						}
					}
					break;
				}
				case 4:
				{
					int motion = this.dartLeft ? -3 : 3;
					if (this.phaseInternalCounter == 0 && (playerPosition.X <= (float)(7 * AbigailGame.TileSize) || playerPosition.X >= (float)(9 * AbigailGame.TileSize)))
					{
						this.phaseInternalCounter = 1;
						this.phaseInternalTimer = Game1.random.Next(500, 1500);
					}
					else if (Math.Abs(this.position.Location.X - this.homePosition.X + AbigailGame.TileSize / 2) < AbigailGame.TileSize * 7 + 12 && this.phaseInternalCounter == 0)
					{
						this.position.X = this.position.X + motion;
					}
					else if (this.phaseInternalCounter == 2)
					{
						motion = (this.dartLeft ? -4 : 4);
						this.position.X = this.position.X - motion;
						if (Math.Abs(this.position.X - this.homePosition.X) < 4)
						{
							this.position.X = this.homePosition.X;
							this.phase = 0;
							this.phaseCountdown = Game1.random.Next(1000, 2000);
						}
					}
					else
					{
						if (this.phaseInternalCounter == 0)
						{
							this.phaseInternalCounter++;
							this.phaseInternalTimer = Game1.random.Next(1000, 2000);
						}
						this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						this.shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.shootTimer < 0)
						{
							Vector2 trajectory = Utility.getVelocityTowardPoint(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y), playerPosition + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), 8f);
							AbigailGame.enemyBullets.Add(new AbigailGame.CowboyBullet(new Point(this.position.X + AbigailGame.TileSize / 2, this.position.Y - AbigailGame.TileSize / 2), new Point((int)trajectory.X, (int)trajectory.Y), 1));
							this.shootTimer = 120;
							Game1.playSound("Cowboy_gunshot", null);
						}
						if (this.phaseInternalTimer <= 0)
						{
							this.phaseInternalCounter++;
						}
					}
					break;
				}
				}
				IL_AA4:
				if (this.position.X <= 16 * AbigailGame.TileSize)
				{
					int x = this.position.X;
				}
				return false;
			}

			// Token: 0x060041E2 RID: 16866 RVA: 0x0030C781 File Offset: 0x0030A981
			public override int getLootDrop()
			{
				return 8;
			}

			// Token: 0x060041E3 RID: 16867 RVA: 0x0030C784 File Offset: 0x0030A984
			public override bool takeDamage(int damage)
			{
				if (Math.Abs(this.position.X - this.homePosition.X) < 5)
				{
					return false;
				}
				this.health -= damage;
				if (this.health < 0)
				{
					return true;
				}
				this.flashColorTimer = 150f;
				Game1.playSound("cowboy_monsterhit", null);
				return false;
			}

			// Token: 0x04002C1F RID: 11295
			public const int talkingPhase = -1;

			// Token: 0x04002C20 RID: 11296
			public const int hidingPhase = 0;

			// Token: 0x04002C21 RID: 11297
			public const int dartOutAndShootPhase = 1;

			// Token: 0x04002C22 RID: 11298
			public const int runAndGunPhase = 2;

			// Token: 0x04002C23 RID: 11299
			public const int runGunAndPantPhase = 3;

			// Token: 0x04002C24 RID: 11300
			public const int shootAtPlayerPhase = 4;

			// Token: 0x04002C25 RID: 11301
			public int phase;

			// Token: 0x04002C26 RID: 11302
			public int phaseCountdown;

			// Token: 0x04002C27 RID: 11303
			public int shootTimer;

			// Token: 0x04002C28 RID: 11304
			public int phaseInternalTimer;

			// Token: 0x04002C29 RID: 11305
			public int phaseInternalCounter;

			// Token: 0x04002C2A RID: 11306
			public bool dartLeft;

			// Token: 0x04002C2B RID: 11307
			public int fullHealth;

			// Token: 0x04002C2C RID: 11308
			public Point homePosition;
		}
	}
}

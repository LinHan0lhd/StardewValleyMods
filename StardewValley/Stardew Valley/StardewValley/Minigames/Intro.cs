using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Menus;

namespace StardewValley.Minigames
{
	// Token: 0x0200023A RID: 570
	[InstanceStatics]
	public class Intro : IMinigame
	{
		// Token: 0x060025D1 RID: 9681 RVA: 0x001A76D0 File Offset: 0x001A58D0
		public Intro()
		{
			this.texture = Game1.content.Load<Texture2D>("Minigames\\Intro");
			this.roadsideTexture = Game1.content.Load<Texture2D>("Maps\\spring_outdoorsTileSheet");
			this.cloudTexture = Game1.content.Load<Texture2D>("Minigames\\Clouds");
			this.treeStripTexture = Game1.content.Load<Texture2D>("Minigames\\treestrip");
			this.transformMatrix = Matrix.CreateScale((float)this.pixelScale);
			this.skyColor = new Color(64, 136, 248);
			this.roadColor = new Color(130, 130, 130);
			this.createBeginningOfLevel();
			Game1.player.FarmerSprite.SourceRect = new Rectangle(0, 0, 16, 32);
			this.bigCloudPosition = (float)this.cloudTexture.Width;
			Intro.roadNoise = Game1.soundBank.GetCue("roadnoise");
			this.currentState = 1;
			Game1.changeMusicTrack("spring_day_ambient", false, MusicContext.Default);
			this.changeScreenSize();
		}

		// Token: 0x060025D2 RID: 9682 RVA: 0x001A7850 File Offset: 0x001A5A50
		public Intro(int startingGameMode)
		{
			this.texture = Game1.content.Load<Texture2D>("Minigames\\Intro");
			this.roadsideTexture = Game1.content.Load<Texture2D>("Maps\\spring_outdoorsTileSheet");
			this.cloudTexture = Game1.content.Load<Texture2D>("Minigames\\Clouds");
			this.transformMatrix = Matrix.CreateScale((float)this.pixelScale);
			this.skyColor = new Color(102, 181, 255);
			this.roadColor = new Color(130, 130, 130);
			this.createBeginningOfLevel();
			this.currentState = startingGameMode;
			if (this.currentState == 4)
			{
				this.fadeAlpha = 1f;
			}
			this.changeScreenSize();
		}

		// Token: 0x060025D3 RID: 9683 RVA: 0x001A7981 File Offset: 0x001A5B81
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x060025D4 RID: 9684 RVA: 0x001A7990 File Offset: 0x001A5B90
		public void createBeginningOfLevel()
		{
			this.backClouds.Clear();
			this.road.Clear();
			this.sky.Clear();
			this.roadsideObjects.Clear();
			this.roadsideFences.Clear();
			for (int i = 0; i < this.screenWidth / this.tileSize + 6; i++)
			{
				this.road.Add((Game1.random.NextDouble() < 0.7) ? 0 : Game1.random.Next(0, 3));
				this.roadsideObjects.Add(-1);
				this.roadsideFences.Add(-1);
			}
			for (int j = 0; j < this.screenWidth / 112 + 2; j++)
			{
				this.sky.Add(Game1.random.Choose(0, 1, 1));
			}
			for (int k = 0; k < this.screenWidth / 170 + 2; k++)
			{
				this.backClouds.Add(new Point(Game1.random.Next(3), Game1.random.Next(this.screenHeight / 2)));
			}
			this.roadsideObjects.Add(-1);
			this.roadsideObjects.Add(-1);
			this.roadsideObjects.Add(-1);
			this.busPosition = new Vector2((float)(this.tileSize * 8), 240f);
		}

		// Token: 0x060025D5 RID: 9685 RVA: 0x001A7AE8 File Offset: 0x001A5CE8
		public void updateRoad(GameTime time)
		{
			this.roadPosition += (float)time.ElapsedGameTime.TotalMilliseconds * this.speed;
			if (this.roadPosition >= (float)(this.tileSize * 3))
			{
				this.roadPosition -= (float)(this.tileSize * 3);
				for (int i = 0; i < 3; i++)
				{
					this.road.Add((Game1.random.NextDouble() < 0.7) ? 0 : Game1.random.Next(0, 3));
				}
				this.road.RemoveRange(0, 3);
				if (this.fenceBuildStatus != -1 || (this.cameraCenteredOnBus && Game1.random.NextDouble() < 0.1))
				{
					for (int j = 0; j < 3; j++)
					{
						switch (this.fenceBuildStatus)
						{
						case -1:
							this.fenceBuildStatus = 0;
							this.roadsideFences.Add(0);
							break;
						case 0:
							this.fenceBuildStatus = 1;
							this.roadsideFences.Add(Game1.random.Next(3));
							break;
						case 1:
							if (Game1.random.NextDouble() < 0.1)
							{
								this.roadsideFences.Add(2);
								this.fenceBuildStatus = 2;
							}
							else
							{
								this.fenceBuildStatus = 1;
								this.roadsideFences.Add((Game1.random.NextDouble() < 0.1) ? 3 : Game1.random.Next(3));
							}
							break;
						case 2:
							this.fenceBuildStatus = -1;
							for (int k = j; k < 3; k++)
							{
								this.roadsideFences.Add(-1);
							}
							break;
						}
						if (this.fenceBuildStatus == -1)
						{
							break;
						}
					}
				}
				else
				{
					this.roadsideFences.Add(-1);
					this.roadsideFences.Add(-1);
					this.roadsideFences.Add(-1);
				}
				this.roadsideFences.RemoveRange(0, 3);
				if (this.cameraCenteredOnBus && !this.addedSign && Game1.random.NextDouble() < 0.25)
				{
					for (int l = 0; l < 3; l++)
					{
						if (l == 0 && Game1.random.NextDouble() < 0.3)
						{
							this.roadsideObjects.Add(Game1.random.Next(2));
							for (int m = l; m < 3; m++)
							{
								this.roadsideObjects.Add(-1);
							}
							break;
						}
						if (Game1.random.NextBool())
						{
							this.roadsideObjects.Add(Game1.random.Next(2, 5));
						}
						else
						{
							this.roadsideObjects.Add(-1);
						}
					}
				}
				else
				{
					this.roadsideObjects.Add(-1);
					this.roadsideObjects.Add(-1);
					this.roadsideObjects.Add(-1);
				}
				this.roadsideObjects.RemoveRange(0, 3);
			}
			this.skyPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 12f);
			if (this.skyPosition >= 112f)
			{
				this.skyPosition -= 112f;
				this.sky.Add(Game1.random.Next(2));
				this.sky.RemoveAt(0);
			}
			this.treePosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 2f);
			if (this.treePosition >= 256f)
			{
				this.treePosition -= 256f;
			}
			this.valleyPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 6f);
			if (this.carPosition.Equals(Vector2.Zero) && Game1.random.NextDouble() < 0.002 && !this.addedSign)
			{
				this.carPosition = new Vector2((float)this.screenWidth, 200f);
				this.carColor = new Color(Game1.random.Next(100, 255), Game1.random.Next(100, 255), Game1.random.Next(100, 255));
				return;
			}
			if (!this.carPosition.Equals(Vector2.Zero))
			{
				this.carPosition.X = this.carPosition.X - 0.1f * (float)time.ElapsedGameTime.TotalMilliseconds * ((float)this.carColor.G / 60f);
				if (this.carPosition.X < -200f)
				{
					this.carPosition = Vector2.Zero;
				}
			}
		}

		// Token: 0x060025D6 RID: 9686 RVA: 0x001A7FA4 File Offset: 0x001A61A4
		public void updateUpperClouds(GameTime time)
		{
			this.bigCloudPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 24f);
			if (this.bigCloudPosition >= (float)(this.cloudTexture.Width * 3))
			{
				this.bigCloudPosition -= (float)(this.cloudTexture.Width * 3);
			}
			this.backCloudPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 36f);
			if (this.backCloudPosition > 170f)
			{
				this.backCloudPosition %= 170f;
				this.backClouds.Add(new Point(Game1.random.Next(3), Game1.random.Next(this.screenHeight / 2)));
				this.backClouds.RemoveAt(0);
			}
			if (Game1.random.NextDouble() < 0.0002)
			{
				this.balloons.Add(new Intro.Balloon(this.screenWidth, this.screenHeight));
				if (Game1.random.NextDouble() < 0.1)
				{
					Vector2 position = new Vector2((float)Game1.random.Next(this.screenWidth / 3, this.screenWidth), (float)this.screenHeight);
					this.balloons.Add(new Intro.Balloon(this.screenWidth, this.screenHeight)
					{
						position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
					});
					this.balloons.Add(new Intro.Balloon(this.screenWidth, this.screenHeight)
					{
						position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
					});
					this.balloons.Add(new Intro.Balloon(this.screenWidth, this.screenHeight)
					{
						position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
					});
					this.balloons.Add(new Intro.Balloon(this.screenWidth, this.screenHeight)
					{
						position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
					});
				}
			}
			for (int i = this.balloons.Count - 1; i >= 0; i--)
			{
				this.balloons[i].update(this.speed, time);
				if (this.balloons[i].position.X < (float)(-(float)this.tileSize) || this.balloons[i].position.Y < (float)(-(float)this.tileSize))
				{
					this.balloons.RemoveAt(i);
				}
			}
		}

		// Token: 0x060025D7 RID: 9687 RVA: 0x001A82BC File Offset: 0x001A64BC
		public bool tick(GameTime time)
		{
			if (this.hasQuit)
			{
				return true;
			}
			if (this.quit && !this.hasQuit)
			{
				Game1.warpFarmer("BusStop", 22, 11, false);
				ICue cue = Intro.roadNoise;
				if (cue != null)
				{
					cue.Stop(AudioStopOptions.Immediate);
				}
				Game1.exitActiveMenu();
				this.hasQuit = true;
				return true;
			}
			switch (this.currentState)
			{
			case 0:
				this.updateUpperClouds(time);
				break;
			case 1:
				this.globalYPanDY = Math.Min(4f, this.globalYPanDY + (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 140f));
				this.globalYPan -= this.globalYPanDY;
				this.updateUpperClouds(time);
				if (this.globalYPan < -1f)
				{
					this.globalYPan = (float)(this.screenHeight * this.pixelScale);
					this.currentState = 2;
					this.transformMatrix = Matrix.CreateScale((float)this.pixelScale);
					this.transformMatrix.Translation = new Vector3(0f, this.globalYPan, 0f);
					if (Intro.roadNoise != null)
					{
						Intro.roadNoise.SetVariable("Volume", 0);
						Intro.roadNoise.Play();
					}
					Game1.game1.loadForNewGame(false);
				}
				break;
			case 2:
			{
				int startPanY = this.screenHeight * this.pixelScale;
				int endPanY = -Math.Max(0, 900 - Game1.graphics.GraphicsDevice.Viewport.Height);
				endPanY = -(int)(240f * (540f / (float)Game1.graphics.GraphicsDevice.Viewport.Height));
				this.globalYPanDY = Math.Max(1f, this.globalYPan / 100f);
				this.globalYPan -= this.globalYPanDY;
				if (this.globalYPan <= (float)endPanY)
				{
					this.globalYPan = (float)endPanY;
				}
				this.transformMatrix = Matrix.CreateScale((float)this.pixelScale);
				this.transformMatrix.Translation = new Vector3(0f, this.globalYPan, 0f);
				this.updateRoad(time);
				if (Intro.roadNoise != null)
				{
					float vol = (this.globalYPan - (float)startPanY) / (float)(endPanY - startPanY) * 10f + 90f;
					Intro.roadNoise.SetVariable("Volume", vol);
				}
				if (this.globalYPan <= (float)endPanY)
				{
					this.currentState = 3;
				}
				break;
			}
			case 3:
				this.updateRoad(time);
				this.drivingTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.drivingTimer > 4700f)
				{
					this.drivingTimer = 0f;
					this.currentState = 4;
				}
				break;
			case 4:
				this.updateRoad(time);
				this.drivingTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.drivingTimer > 2000f)
				{
					this.busPosition.X = this.busPosition.X + (float)time.ElapsedGameTime.TotalMilliseconds / 8f;
					ICue cue2 = Intro.roadNoise;
					if (cue2 != null)
					{
						cue2.SetVariable("Volume", Math.Max(0f, Intro.roadNoise.GetVariable("Volume") - 1f));
					}
					this.speed = Math.Max(0f, this.speed - (float)time.ElapsedGameTime.TotalMilliseconds / 70000f);
					if (!this.addedSign)
					{
						this.addedSign = true;
						this.roadsideObjects.RemoveAt(this.roadsideObjects.Count - 1);
						this.roadsideObjects.Add(5);
						Game1.playSound("busDriveOff", null);
					}
					if (this.speed <= 0f && this.birdPosition.Equals(Vector2.Zero))
					{
						int position = 0;
						for (int i = 0; i < this.roadsideObjects.Count; i++)
						{
							if (this.roadsideObjects[i] == 5)
							{
								position = i;
								break;
							}
						}
						this.birdPosition = new Vector2((float)(position * 16) - this.roadPosition - 32f + 16f, -16f);
						Game1.playSound("SpringBirds", null);
						this.fadeAlpha = 0f;
					}
					if (!this.birdPosition.Equals(Vector2.Zero) && this.birdPosition.Y < 116f)
					{
						float dy = Math.Max(0.5f, (116f - this.birdPosition.Y) / 116f * 2f);
						this.birdPosition.Y = this.birdPosition.Y + dy;
						this.birdPosition.X = this.birdPosition.X + (float)Math.Sin((double)this.birdXTimer / 50.26548245743669) * dy / 2f;
						this.birdTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
						this.birdXTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
						if (this.birdTimer >= 100f)
						{
							this.birdFrame = (this.birdFrame + 1) % 4;
							this.birdTimer = 0f;
						}
					}
					else if (!this.birdPosition.Equals(Vector2.Zero))
					{
						this.birdFrame = ((this.birdTimer > 1500f) ? 5 : 4);
						this.birdTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
						if (this.birdTimer > 2400f || (this.birdTimer > 1800f && Game1.random.NextDouble() < 0.006))
						{
							this.birdTimer = 0f;
							if (Game1.random.NextBool())
							{
								Game1.playSound("SpringBirds", null);
								this.birdPosition.Y = this.birdPosition.Y - 4f;
							}
						}
					}
					if (this.drivingTimer > 14000f)
					{
						this.fadeAlpha += (float)time.ElapsedGameTime.TotalMilliseconds * 0.1f / 128f;
						if (this.fadeAlpha >= 1f)
						{
							Game1.warpFarmer("BusStop", 22, 11, false);
							ICue cue3 = Intro.roadNoise;
							if (cue3 != null)
							{
								cue3.Stop(AudioStopOptions.Immediate);
							}
							Game1.exitActiveMenu();
							return true;
						}
					}
				}
				break;
			}
			return false;
		}

		// Token: 0x060025D8 RID: 9688 RVA: 0x001A8944 File Offset: 0x001A6B44
		public void doneCreatingCharacter()
		{
			this.characterCreateMenu = null;
			this.currentState = 1;
			Game1.changeMusicTrack("spring_day_ambient", false, MusicContext.Default);
		}

		// Token: 0x060025D9 RID: 9689 RVA: 0x001A8960 File Offset: 0x001A6B60
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			CharacterCustomization characterCustomization = this.characterCreateMenu;
			if (characterCustomization != null)
			{
				characterCustomization.receiveLeftClick(x, y, true);
			}
			for (int i = this.balloons.Count - 1; i >= 0; i--)
			{
				if (new Rectangle((int)this.balloons[i].position.X * 4 + 16, (int)this.balloons[i].position.Y * 4 + 16, 32, 32).Contains(x, y))
				{
					this.balloons.RemoveAt(i);
					Game1.playSound("coin", null);
				}
			}
		}

		// Token: 0x060025DA RID: 9690 RVA: 0x001A8A06 File Offset: 0x001A6C06
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
			CharacterCustomization characterCustomization = this.characterCreateMenu;
			if (characterCustomization == null)
			{
				return;
			}
			characterCustomization.receiveRightClick(x, y, true);
		}

		// Token: 0x060025DB RID: 9691 RVA: 0x001A8A1B File Offset: 0x001A6C1B
		public void releaseLeftClick(int x, int y)
		{
			CharacterCustomization characterCustomization = this.characterCreateMenu;
			if (characterCustomization == null)
			{
				return;
			}
			characterCustomization.releaseLeftClick(x, y);
		}

		// Token: 0x060025DC RID: 9692 RVA: 0x001A8A2F File Offset: 0x001A6C2F
		public void leftClickHeld(int x, int y)
		{
			CharacterCustomization characterCustomization = this.characterCreateMenu;
			if (characterCustomization == null)
			{
				return;
			}
			characterCustomization.leftClickHeld(x, y);
		}

		// Token: 0x060025DD RID: 9693 RVA: 0x001A8A43 File Offset: 0x001A6C43
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x060025DE RID: 9694 RVA: 0x001A8A48 File Offset: 0x001A6C48
		public void receiveKeyPress(Keys k)
		{
			if (k == Keys.Escape && this.currentState != 1)
			{
				if (!this.quit)
				{
					Game1.playSound("bigDeSelect", null);
				}
				this.quit = true;
			}
		}

		// Token: 0x060025DF RID: 9695 RVA: 0x001A8A86 File Offset: 0x001A6C86
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x060025E0 RID: 9696 RVA: 0x001A8A88 File Offset: 0x001A6C88
		public void draw(SpriteBatch b)
		{
			switch (this.currentState)
			{
			case 0:
				break;
			case 1:
			{
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				b.GraphicsDevice.Clear(this.skyColor);
				int x = 64;
				int y = Game1.graphics.GraphicsDevice.Viewport.Height - 64;
				int w = 0;
				int h = 64;
				Utility.makeSafe(ref x, ref y, w, h);
				SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3689"), x, y, 999, -1, 999, 1f, 1f, false, 0, "", null, SpriteText.ScrollTextAlignment.Left);
				b.End();
				return;
			}
			case 2:
			case 3:
			case 4:
				this.drawRoadArea(b);
				break;
			default:
				return;
			}
		}

		// Token: 0x060025E1 RID: 9697 RVA: 0x001A8B64 File Offset: 0x001A6D64
		public void drawRoadArea(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, new Matrix?(this.transformMatrix));
			b.GraphicsDevice.Clear(this.roadColor);
			b.Draw(Game1.staminaRect, new Rectangle(0, -this.screenHeight * 2, this.screenWidth, this.screenHeight * 8), this.skyColor);
			b.Draw(Game1.staminaRect, new Rectangle(0, this.screenHeight / 2 + 80 - 100, this.screenWidth, this.screenHeight * 4), this.roadColor);
			for (int i = 0; i < this.screenWidth / 112 + 2; i++)
			{
				if (this.sky[i] == 0)
				{
					b.Draw(this.texture, new Vector2(-this.skyPosition + (float)(i * 112) - (float)(i * 2), -16f), new Rectangle?(new Rectangle(129, 0, 110, 96)), Color.White);
				}
				else
				{
					Rectangle srcRect = new Rectangle(128, 0, 1, 96);
					b.Draw(this.texture, new Rectangle((int)(-(int)this.skyPosition) - 1 + i * 112 - i * 2, -16, 114, 96), new Rectangle?(srcRect), Color.White);
				}
			}
			for (int j = 0; j < 12; j++)
			{
				b.Draw(Game1.mouseCursors, new Vector2(-10f + -this.valleyPosition / 2f + (float)(j * 639) - (float)(j * 2), 70f), new Rectangle?(new Rectangle(0, 886, 639, 148)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(-this.valleyPosition + (float)(j * 639) - (float)(j * 2), 80f), new Rectangle?(new Rectangle(0, 737, 639, 120)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.08f);
			}
			for (int k = 0; k < 8; k++)
			{
				b.Draw(this.treeStripTexture, new Vector2((float)(k * 256) - this.treePosition, 110f), new Rectangle?(new Rectangle(0, 0, 256, 64)), Color.White);
			}
			for (int l = 0; l < this.road.Count; l++)
			{
				if (l % 3 == 0)
				{
					b.Draw(this.texture, new Vector2((float)(l * 16) - this.roadPosition, 160f), new Rectangle?(new Rectangle(0, 176, 48, 48)), Color.White);
					b.Draw(this.texture, new Vector2((float)(l * 16 + this.tileSize) - this.roadPosition, 272f), new Rectangle?(new Rectangle(0, 64, 16, 16)), Color.White);
				}
				b.Draw(this.texture, new Vector2((float)(l * 16) - this.roadPosition, 208f), new Rectangle?(new Rectangle(this.road[l] * 16, 240, 16, 16)), Color.White);
			}
			for (int m = 0; m < this.roadsideObjects.Count; m++)
			{
				switch (this.roadsideObjects[m])
				{
				case 0:
					b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 96f), new Rectangle?(new Rectangle(48, 0, 48, 96)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					break;
				case 1:
					b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 96f), new Rectangle?(new Rectangle(0, 0, 48, 64)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 16f, 160f), new Rectangle?(new Rectangle(16, 64, 16, 32)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					break;
				case 2:
					b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 176f), new Rectangle?(new Rectangle(112, 144, 16, 16)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					break;
				case 3:
					b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 176f), new Rectangle?(new Rectangle(112, 160, 16, 16)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					break;
				case 5:
					b.Draw(this.texture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 128f), new Rectangle?(new Rectangle(48, 176, 64, 64)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					break;
				}
			}
			for (int n = 0; n < this.roadsideFences.Count; n++)
			{
				if (this.roadsideFences[n] != -1)
				{
					if (this.roadsideFences[n] == 3)
					{
						b.Draw(this.roadsideTexture, new Vector2((float)(n * 16) - this.roadPosition, 176f), new Rectangle?(new Rectangle(144, 256, 16, 32)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					}
					else
					{
						b.Draw(this.roadsideTexture, new Vector2((float)(n * 16) - this.roadPosition, 176f), new Rectangle?(new Rectangle(128 + this.roadsideFences[n] * 16, 224, 16, 32)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					}
				}
			}
			if (!this.carPosition.Equals(Vector2.Zero))
			{
				b.Draw(this.texture, this.carPosition, new Rectangle?(new Rectangle(160, 112, 80, 64)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				b.Draw(this.texture, this.carPosition, new Rectangle?(new Rectangle(160, 176, 80, 64)), this.carColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
			b.Draw(this.texture, this.busPosition, new Rectangle?(new Rectangle(0, 0, 128, 64)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			b.Draw(this.texture, this.busPosition + new Vector2(23.5f, 56.5f) * 1f, new Rectangle?(new Rectangle(21, 54, 5, 5)), Color.White, (float)((double)(this.roadPosition / 3f / 16f) * 3.141592653589793 * 2.0), new Vector2(2.5f, 2.5f), 1f, SpriteEffects.None, 0f);
			b.Draw(this.texture, this.busPosition + new Vector2(87.5f, 56.5f) * 1f, new Rectangle?(new Rectangle(21, 54, 5, 5)), Color.White, (float)((double)((this.roadPosition + 4f) / 3f / 16f) * 3.141592653589793 * 2.0), new Vector2(2.5f, 2.5f), 1f, SpriteEffects.None, 0f);
			if (!this.birdPosition.Equals(Vector2.Zero))
			{
				b.Draw(this.texture, this.birdPosition, new Rectangle?(new Rectangle(16 + this.birdFrame * 16, 64, 16, 16)), Color.White);
			}
			if (this.fadeAlpha > 0f)
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, this.screenWidth + 2, this.screenHeight * 2), Color.Black * this.fadeAlpha);
			}
			b.End();
		}

		// Token: 0x060025E2 RID: 9698 RVA: 0x001A94A4 File Offset: 0x001A76A4
		public void changeScreenSize()
		{
			if (Game1.graphics.GraphicsDevice.Viewport.Height < 1000)
			{
				this.pixelScale = 3;
			}
			else if (Game1.graphics.GraphicsDevice.Viewport.Width > 2600)
			{
				this.pixelScale = 5;
			}
			else
			{
				this.pixelScale = 4;
			}
			this.transformMatrix = Matrix.CreateScale((float)this.pixelScale);
			this.screenWidth = Game1.graphics.GraphicsDevice.Viewport.Width / this.pixelScale;
			this.screenHeight = Game1.graphics.GraphicsDevice.Viewport.Height / this.pixelScale;
			this.createBeginningOfLevel();
		}

		// Token: 0x060025E3 RID: 9699 RVA: 0x001A9566 File Offset: 0x001A7766
		public void unload()
		{
		}

		// Token: 0x060025E4 RID: 9700 RVA: 0x001A9568 File Offset: 0x001A7768
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060025E5 RID: 9701 RVA: 0x001A956F File Offset: 0x001A776F
		public string minigameId()
		{
			return null;
		}

		// Token: 0x060025E6 RID: 9702 RVA: 0x001A9572 File Offset: 0x001A7772
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x060025E7 RID: 9703 RVA: 0x001A9575 File Offset: 0x001A7775
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x0400171B RID: 5915
		public int pixelScale = 4;

		// Token: 0x0400171C RID: 5916
		public const int skyLoopWidth = 112;

		// Token: 0x0400171D RID: 5917
		public const int cloudLoopWidth = 170;

		// Token: 0x0400171E RID: 5918
		public const int tilesBeyondViewportToSimulate = 6;

		// Token: 0x0400171F RID: 5919
		public const int leftFence = 0;

		// Token: 0x04001720 RID: 5920
		public const int centerFence = 1;

		// Token: 0x04001721 RID: 5921
		public const int rightFence = 2;

		// Token: 0x04001722 RID: 5922
		public const int busYRest = 240;

		// Token: 0x04001723 RID: 5923
		public const int choosingCharacterState = 0;

		// Token: 0x04001724 RID: 5924
		public const int panningDownFromCloudsState = 1;

		// Token: 0x04001725 RID: 5925
		public const int panningDownToRoadState = 2;

		// Token: 0x04001726 RID: 5926
		public const int drivingState = 3;

		// Token: 0x04001727 RID: 5927
		public const int stardewInViewState = 4;

		// Token: 0x04001728 RID: 5928
		public float speed = 0.1f;

		// Token: 0x04001729 RID: 5929
		private float valleyPosition;

		// Token: 0x0400172A RID: 5930
		private float skyPosition;

		// Token: 0x0400172B RID: 5931
		private float roadPosition;

		// Token: 0x0400172C RID: 5932
		private float bigCloudPosition;

		// Token: 0x0400172D RID: 5933
		private float backCloudPosition;

		// Token: 0x0400172E RID: 5934
		private float globalYPan;

		// Token: 0x0400172F RID: 5935
		private float globalYPanDY;

		// Token: 0x04001730 RID: 5936
		private float drivingTimer;

		// Token: 0x04001731 RID: 5937
		private float fadeAlpha;

		// Token: 0x04001732 RID: 5938
		private float treePosition;

		// Token: 0x04001733 RID: 5939
		private int screenWidth;

		// Token: 0x04001734 RID: 5940
		private int screenHeight;

		// Token: 0x04001735 RID: 5941
		private int tileSize = 16;

		// Token: 0x04001736 RID: 5942
		private Matrix transformMatrix;

		// Token: 0x04001737 RID: 5943
		private Texture2D texture;

		// Token: 0x04001738 RID: 5944
		private Texture2D roadsideTexture;

		// Token: 0x04001739 RID: 5945
		private Texture2D cloudTexture;

		// Token: 0x0400173A RID: 5946
		private Texture2D treeStripTexture;

		// Token: 0x0400173B RID: 5947
		private List<Point> backClouds = new List<Point>();

		// Token: 0x0400173C RID: 5948
		private List<int> road = new List<int>();

		// Token: 0x0400173D RID: 5949
		private List<int> sky = new List<int>();

		// Token: 0x0400173E RID: 5950
		private List<int> roadsideObjects = new List<int>();

		// Token: 0x0400173F RID: 5951
		private List<int> roadsideFences = new List<int>();

		// Token: 0x04001740 RID: 5952
		private Color skyColor;

		// Token: 0x04001741 RID: 5953
		private Color roadColor;

		// Token: 0x04001742 RID: 5954
		private Color carColor;

		// Token: 0x04001743 RID: 5955
		private bool cameraCenteredOnBus = true;

		// Token: 0x04001744 RID: 5956
		private bool addedSign;

		// Token: 0x04001745 RID: 5957
		private Vector2 busPosition;

		// Token: 0x04001746 RID: 5958
		private Vector2 carPosition;

		// Token: 0x04001747 RID: 5959
		private Vector2 birdPosition = Vector2.Zero;

		// Token: 0x04001748 RID: 5960
		private CharacterCustomization characterCreateMenu;

		// Token: 0x04001749 RID: 5961
		private List<Intro.Balloon> balloons = new List<Intro.Balloon>();

		// Token: 0x0400174A RID: 5962
		private int birdFrame;

		// Token: 0x0400174B RID: 5963
		private float birdTimer;

		// Token: 0x0400174C RID: 5964
		private float birdXTimer;

		// Token: 0x0400174D RID: 5965
		public static ICue roadNoise;

		// Token: 0x0400174E RID: 5966
		private int fenceBuildStatus = -1;

		// Token: 0x0400174F RID: 5967
		private int currentState;

		// Token: 0x04001750 RID: 5968
		private bool quit;

		// Token: 0x04001751 RID: 5969
		private bool hasQuit;

		// Token: 0x020005AA RID: 1450
		public class Balloon
		{
			// Token: 0x06004230 RID: 16944 RVA: 0x0031084C File Offset: 0x0030EA4C
			public Balloon(int screenWidth, int screenHeight)
			{
				int g = Game1.random.Next(255);
				int b = 255 - g;
				int r = Game1.random.Choose(255, 0);
				this.position = new Vector2((float)Game1.random.Next(screenWidth / 5, screenWidth), (float)screenHeight);
				this.color = new Color(r, g, b);
			}

			// Token: 0x06004231 RID: 16945 RVA: 0x003108B4 File Offset: 0x0030EAB4
			public void update(float speed, GameTime time)
			{
				this.position.Y = this.position.Y - speed * (float)time.ElapsedGameTime.TotalMilliseconds / 16f;
				this.position.X = this.position.X - speed * (float)time.ElapsedGameTime.TotalMilliseconds / 32f;
			}

			// Token: 0x04002CA4 RID: 11428
			public Vector2 position;

			// Token: 0x04002CA5 RID: 11429
			public Color color;
		}
	}
}

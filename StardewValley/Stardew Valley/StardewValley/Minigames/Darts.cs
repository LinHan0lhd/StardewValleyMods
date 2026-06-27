using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Minigames
{
	// Token: 0x02000235 RID: 565
	public class Darts : IMinigame
	{
		// Token: 0x06002566 RID: 9574 RVA: 0x001A1D35 File Offset: 0x0019FF35
		public bool overrideFreeMouseMovement()
		{
			return false;
		}

		// Token: 0x06002567 RID: 9575 RVA: 0x001A1D38 File Offset: 0x0019FF38
		public Darts(int dart_count = 20)
		{
			this.dartCount = dart_count;
			this.startingDartCount = dart_count;
			this.changeScreenSize();
			this.texture = Game1.content.Load<Texture2D>("Minigames\\Darts");
			this.points = 301;
			this.SetGameState(Darts.GameState.Aiming);
		}

		// Token: 0x06002568 RID: 9576 RVA: 0x001A1E18 File Offset: 0x001A0018
		public virtual void SetGameState(Darts.GameState new_state)
		{
			Darts.GameState gameState = this.currentGameState;
			if (gameState != Darts.GameState.Charging)
			{
				if (gameState == Darts.GameState.Scoring)
				{
					this.previousPoints = this.points;
					this.shakeScore = false;
					this.alternateTextString = "";
				}
			}
			else if (Darts.chargeSound != null)
			{
				Darts.chargeSound.Stop(AudioStopOptions.Immediate);
				Darts.chargeSound = null;
			}
			this.currentGameState = new_state;
			switch (this.currentGameState)
			{
			case Darts.GameState.Aiming:
				this.dartTime = -1f;
				if (Game1.options.gamepadControls)
				{
					Game1.setMousePosition(Utility.Vector2ToPoint(this.TransformDraw(new Vector2((float)(this.screenWidth / 2), (float)(this.screenHeight / 2)))));
					return;
				}
				break;
			case Darts.GameState.Charging:
				if (Darts.chargeSound == null)
				{
					Game1.playSound("SinWave", out Darts.chargeSound);
				}
				this.chargeTime = 1f;
				this.chargeDirection = -1f;
				this.canCancelShot = true;
				return;
			case Darts.GameState.Firing:
				this.throwStartPosition = this.dartBoardCenter + new Vector2(Utility.RandomFloat(-64f, 64f, null), 200f);
				Game1.playSound("FishHit", null);
				this.hangTime = 0.25f;
				return;
			case Darts.GameState.ShowScore:
				this.stateTimer = 1f;
				return;
			case Darts.GameState.Scoring:
				break;
			case Darts.GameState.GameOver:
				if (this.points == 0)
				{
					this.gameOverString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
					Game1.playSound("yoba", null);
				}
				else
				{
					this.gameOverString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
					Game1.playSound("slimedead", null);
				}
				this.stateTimer = 3f;
				break;
			default:
				return;
			}
		}

		// Token: 0x06002569 RID: 9577 RVA: 0x001A1FD0 File Offset: 0x001A01D0
		public bool WasButtonHeld()
		{
			return Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.input.GetGamePadState().IsButtonDown(Buttons.A) || Game1.input.GetGamePadState().IsButtonDown(Buttons.X) || Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) || Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton);
		}

		// Token: 0x0600256A RID: 9578 RVA: 0x001A2068 File Offset: 0x001A0268
		public bool WasButtonPressed()
		{
			return (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released) || (Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonUp(Buttons.A)) || (Game1.input.GetGamePadState().IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonUp(Buttons.X)) || (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton)) || (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton));
		}

		// Token: 0x0600256B RID: 9579 RVA: 0x001A2158 File Offset: 0x001A0358
		public bool tick(GameTime time)
		{
			if (this.stateTimer > 0f)
			{
				this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.stateTimer <= 0f)
				{
					this.stateTimer = 0f;
					Darts.GameState gameState = this.currentGameState;
					if (gameState != Darts.GameState.ShowScore)
					{
						if (gameState == Darts.GameState.GameOver)
						{
							this.QuitGame();
							return true;
						}
					}
					else if (this.lastHitAmount == 0)
					{
						if (this.dartCount <= 0)
						{
							this.SetGameState(Darts.GameState.Scoring);
						}
						else
						{
							this.SetGameState(Darts.GameState.Aiming);
						}
					}
					else
					{
						this.nextPointTransferTime = 0.5f;
						this.SetGameState(Darts.GameState.Scoring);
					}
				}
			}
			if (this.currentGameState == Darts.GameState.GameOver && this.WasButtonPressed())
			{
				this.QuitGame();
				return true;
			}
			this.cursorPosition = (Utility.PointToVector2(Game1.getMousePosition()) - this.upperLeft) / this.GetPixelScale();
			switch (this.currentGameState)
			{
			case Darts.GameState.Aiming:
				this.chargeTime = 1f;
				this.aimPosition = this.cursorPosition;
				this.aimPosition.X = this.aimPosition.X + (float)Math.Sin(time.TotalGameTime.TotalSeconds * 0.75) * 32f;
				this.aimPosition.Y = this.aimPosition.Y + (float)Math.Sin(time.TotalGameTime.TotalSeconds * 1.5) * 32f;
				if (this.WasButtonPressed() && this.IsAiming())
				{
					this.SetGameState(Darts.GameState.Charging);
				}
				break;
			case Darts.GameState.Charging:
				if (Darts.chargeSound != null)
				{
					Game1.sounds.SetPitch(Darts.chargeSound, 2400f * (1f - this.chargeTime), true);
				}
				this.chargeTime += (float)time.ElapsedGameTime.TotalSeconds * this.chargeDirection;
				if (this.chargeDirection < 0f && this.chargeTime < 0f)
				{
					this.canCancelShot = false;
					this.chargeTime = 0f;
					this.chargeDirection = 1f;
				}
				else if (this.chargeDirection > 0f && this.chargeTime >= 1f)
				{
					this.chargeTime = 1f;
					this.chargeDirection = -1f;
				}
				if (!this.WasButtonHeld())
				{
					if (this.chargeTime > 0.8f && this.canCancelShot)
					{
						this.SetGameState(Darts.GameState.Aiming);
						this.chargeTime = 0f;
					}
					else
					{
						this.dartCount--;
						this.throwsCount++;
						this.FireDart(this.chargeTime);
					}
				}
				break;
			case Darts.GameState.Firing:
				if (this.hangTime > 0f)
				{
					this.hangTime -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this.hangTime <= 0f)
					{
						float random_angle = Utility.RandomFloat(0f, 6.2831855f, null);
						this.aimPosition += new Vector2((float)Math.Sin((double)random_angle), (float)Math.Cos((double)random_angle)) * Utility.RandomFloat(0f, this.GetRadiusFromCharge() * 32f, null);
						Game1.playSound("cast", null);
						this.dartTime = 0f;
						this.dartPosition = this.throwStartPosition;
					}
				}
				else if (this.dartTime >= 0f)
				{
					this.dartTime += (float)time.ElapsedGameTime.TotalSeconds / 0.75f;
					this.dartPosition.X = Utility.Lerp(this.throwStartPosition.X, this.aimPosition.X, this.dartTime);
					this.dartPosition.Y = Utility.Lerp(this.throwStartPosition.Y, this.aimPosition.Y, this.dartTime);
					if (this.dartTime >= 1f)
					{
						Game1.playSound("Cowboy_gunshot", null);
						this.lastHitAmount = this.GetPointsForAim();
						this.SetGameState(Darts.GameState.ShowScore);
					}
				}
				break;
			case Darts.GameState.Scoring:
				if (this.lastHitAmount > 0)
				{
					if (this.nextPointTransferTime > 0f)
					{
						this.nextPointTransferTime -= (float)time.ElapsedGameTime.TotalSeconds;
						if (this.nextPointTransferTime < 0f)
						{
							this.shakeScore = true;
							int transfer_amount = 1;
							if (this.lastHitAmount > 10 && this.points > 10)
							{
								transfer_amount = 10;
							}
							this.points -= transfer_amount;
							this.lastHitAmount -= transfer_amount;
							Game1.playSound("moneyDial", null);
							this.nextPointTransferTime = 0.05f;
							if (this.points < 0)
							{
								this.alternateTextString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11947");
								Game1.playSound("fishEscape", null);
								this.nextPointTransferTime = 1f;
								this.lastHitAmount = 0;
							}
						}
					}
				}
				else
				{
					if (this.nextPointTransferTime > 0f)
					{
						this.nextPointTransferTime -= (float)time.ElapsedGameTime.TotalSeconds;
					}
					if (this.nextPointTransferTime <= 0f)
					{
						this.nextPointTransferTime = 0f;
						if (this.points == 0)
						{
							this.SetGameState(Darts.GameState.GameOver);
						}
						else
						{
							if (this.points < 0)
							{
								this.points = this.previousPoints;
							}
							if (this.dartCount <= 0)
							{
								this.SetGameState(Darts.GameState.GameOver);
							}
							else
							{
								this.SetGameState(Darts.GameState.Aiming);
							}
						}
					}
				}
				break;
			}
			if (this.IsAiming() || this.currentGameState == Darts.GameState.Charging)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			else
			{
				Game1.mouseCursorTransparency = 1f;
			}
			return false;
		}

		// Token: 0x0600256C RID: 9580 RVA: 0x001A2738 File Offset: 0x001A0938
		public virtual bool IsAiming()
		{
			return this.currentGameState == Darts.GameState.Aiming && this.cursorPosition.X > 0f && this.cursorPosition.X < 320f && this.cursorPosition.Y > 0f && this.cursorPosition.Y < 320f;
		}

		// Token: 0x0600256D RID: 9581 RVA: 0x001A2798 File Offset: 0x001A0998
		public float GetRadiusFromCharge()
		{
			return (float)Math.Pow((double)this.chargeTime, 0.5);
		}

		// Token: 0x0600256E RID: 9582 RVA: 0x001A27B0 File Offset: 0x001A09B0
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600256F RID: 9583 RVA: 0x001A27B2 File Offset: 0x001A09B2
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x06002570 RID: 9584 RVA: 0x001A27B4 File Offset: 0x001A09B4
		public virtual int GetPointsForAim()
		{
			Vector2 hit_point = this.aimPosition;
			Vector2 offset = this.dartBoardCenter - hit_point;
			float radius = offset.Length();
			if (radius < 5f)
			{
				Game1.playSound("parrot", null);
				this.lastHitWasDouble = true;
				this.lastHitString = Game1.content.LoadString("Strings\\UI:Darts_Bullseye");
				return 50;
			}
			if (radius < 12f)
			{
				Game1.playSound("parrot", null);
				this.lastHitString = Game1.content.LoadString("Strings\\UI:Darts_Bull");
				return 25;
			}
			if (radius > 88f)
			{
				Game1.playSound("fishEscape", null);
				this.lastHitString = Game1.content.LoadString("Strings\\UI:Darts_OffTheIsland");
				return 0;
			}
			float angle = (float)(Math.Atan2((double)offset.Y, (double)offset.X) * 57.29577951308232);
			angle -= 81f;
			if (angle < 0f)
			{
				angle += 360f;
			}
			int region = (int)(angle / 18f);
			int[] points = new int[]
			{
				20,
				1,
				18,
				4,
				13,
				6,
				10,
				15,
				2,
				17,
				3,
				19,
				7,
				16,
				8,
				11,
				14,
				9,
				12,
				5
			};
			int base_points = 0;
			if (region < points.Length)
			{
				base_points = points[region];
			}
			if (radius >= 46f && radius < 55f)
			{
				Game1.playSound("parrot", null);
				this.lastHitString = base_points.ToString() + "x3";
				return base_points * 3;
			}
			if (radius >= 79f)
			{
				this.lastHitWasDouble = true;
				Game1.playSound("parrot", null);
				this.lastHitString = base_points.ToString() + "x2";
				return base_points * 2;
			}
			this.lastHitString = (base_points.ToString() ?? "");
			return base_points;
		}

		// Token: 0x06002571 RID: 9585 RVA: 0x001A2982 File Offset: 0x001A0B82
		public virtual void FireDart(float radius)
		{
			this.SetGameState(Darts.GameState.Firing);
		}

		// Token: 0x06002572 RID: 9586 RVA: 0x001A298B File Offset: 0x001A0B8B
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x06002573 RID: 9587 RVA: 0x001A298D File Offset: 0x001A0B8D
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002574 RID: 9588 RVA: 0x001A2990 File Offset: 0x001A0B90
		public void receiveKeyPress(Keys k)
		{
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) || k.Equals(Keys.Escape))
			{
				this.QuitGame();
				return;
			}
		}

		// Token: 0x06002575 RID: 9589 RVA: 0x001A29D0 File Offset: 0x001A0BD0
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x06002576 RID: 9590 RVA: 0x001A29D4 File Offset: 0x001A0BD4
		public void QuitGame()
		{
			this.unload();
			Game1.playSound("bigDeSelect", null);
			Game1.currentMinigame = null;
			if (this.currentGameState == Darts.GameState.GameOver)
			{
				if (this.points == 0)
				{
					bool perfect_game = this.IsPerfectVictory();
					if (perfect_game)
					{
						Game1.multiplayer.globalChatInfoMessage("DartsWinPerfect", new string[]
						{
							Game1.player.Name
						});
					}
					else
					{
						Game1.multiplayer.globalChatInfoMessage("DartsWin", new string[]
						{
							Game1.player.Name,
							this.throwsCount.ToString()
						});
					}
					if (Game1.currentLocation is IslandSouthEastCave)
					{
						string text = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win");
						if (perfect_game)
						{
							text = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win_Perfect");
						}
						text += "#";
						int won_dart_nuts = Game1.player.team.GetDroppedLimitedNutCount("Darts");
						if ((this.startingDartCount == 20 && won_dart_nuts == 0) || (this.startingDartCount == 15 && won_dart_nuts == 1) || (this.startingDartCount == 10 && won_dart_nuts == 2))
						{
							text += Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinPrize");
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								Game1.player.team.RequestLimitedNutDrops("Darts", Game1.currentLocation, 1984, 512, 3, 1);
							}));
						}
						else
						{
							text += Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinNoPrize");
						}
						Game1.drawDialogueNoTyping(text);
						return;
					}
				}
				else if (Game1.currentLocation is IslandSouthEastCave)
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Lose"));
				}
			}
		}

		// Token: 0x06002577 RID: 9591 RVA: 0x001A2B7C File Offset: 0x001A0D7C
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState(), null, null);
			b.Draw(this.texture, this.TransformDraw(new Rectangle(0, 0, 320, 320)), new Rectangle?(new Rectangle(0, 0, 320, 320)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			if (this.IsAiming() || this.currentGameState == Darts.GameState.Charging)
			{
				b.Draw(this.texture, this.TransformDraw(this.aimPosition), new Rectangle?(new Rectangle(0, 320, 64, 64)), Color.White * 0.5f, 0f, new Vector2(32f, 32f), this.GetPixelScale() * this.GetRadiusFromCharge(), SpriteEffects.None, 0f);
			}
			if (this.dartTime >= 0f)
			{
				Rectangle dart_rect = new Rectangle(0, 384, 16, 32);
				if (this.dartTime > 0.65f)
				{
					dart_rect.X = 16;
				}
				if (this.dartTime > 0.9f)
				{
					dart_rect.X = 32;
				}
				float y_offset = (float)Math.Sin((double)this.dartTime * 3.141592653589793) * 200f;
				float rotation = (float)Math.Atan2((double)(this.aimPosition.X - this.throwStartPosition.X), (double)(this.throwStartPosition.Y - this.aimPosition.Y));
				b.Draw(this.texture, this.TransformDraw(this.dartPosition - new Vector2(0f, y_offset)), new Rectangle?(dart_rect), Color.White, rotation, new Vector2(8f, 16f), this.GetPixelScale(), SpriteEffects.None, 0.02f);
			}
			Vector2 score_position = this.TransformDraw(new Vector2(160f, 16f));
			Vector2 score_shake = Vector2.Zero;
			if (this.shakeScore)
			{
				score_shake = new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
			}
			if (this.alternateTextString != "")
			{
				SpriteText.drawStringWithScrollCenteredAt(b, this.alternateTextString, (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, new Color?(SpriteText.color_Red), 0, 0.88f, false);
			}
			else if (this.points >= 0)
			{
				string points_string = Game1.content.LoadString("Strings\\UI:Darts_PointsToGo", this.points);
				if (this.points == 1)
				{
					points_string = Game1.content.LoadString("Strings\\UI:Darts_PointToGo", this.points);
				}
				SpriteText.drawStringWithScrollCenteredAt(b, points_string, (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, null, 0, 0.88f, false);
				if (this.currentGameState == Darts.GameState.ShowScore || this.currentGameState == Darts.GameState.Scoring)
				{
					if (this.shakeScore)
					{
						score_shake = new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
					}
					score_position.Y += 64f;
					string string_to_draw = (this.currentGameState == Darts.GameState.ShowScore) ? (" " + this.lastHitString + " ") : (" " + this.lastHitAmount.ToString() + " ");
					SpriteText.drawStringWithScrollCenteredAt(b, string_to_draw, (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, new Color?(SpriteText.color_Blue), 2, 0.88f, false);
				}
			}
			for (int i = 0; i < this.dartCount; i++)
			{
				Vector2 draw_position = new Vector2((float)(7 + i * 10), 317f);
				b.Draw(this.texture, this.TransformDraw(draw_position), new Rectangle?(new Rectangle(64, 384, 16, 32)), Color.White, 0f, new Vector2(0f, 32f), this.GetPixelScale(), SpriteEffects.None, 0.02f);
			}
			if (this.gameOverString != "")
			{
				b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight)), null, Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				if (this.points == 0)
				{
					score_position = this.TransformDraw(new Vector2(160f, 144f));
					SpriteText.drawStringWithScrollCenteredAt(b, this.gameOverString, (int)score_position.X, (int)score_position.Y, "", 1f, null, 0, 0.88f, false);
					score_position = this.TransformDraw(new Vector2(160f, 176f));
					if (this.IsPerfectVictory())
					{
						SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Darts_WinTextPerfect", this.throwsCount), (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, new Color?(SpriteText.color_Blue), 2, 0.88f, false);
					}
					else
					{
						SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Darts_WinText", this.throwsCount), (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, new Color?(SpriteText.color_Blue), 2, 0.88f, false);
					}
				}
				else
				{
					score_position = this.TransformDraw(new Vector2(160f, 160f));
					SpriteText.drawStringWithScrollCenteredAt(b, this.gameOverString, (int)score_position.X, (int)score_position.Y, "", 1f, null, 0, 0.88f, false);
				}
			}
			if (Game1.options.gamepadControls && !Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (Game1.options.snappyMenus && Game1.options.gamepadControls) ? 44 : 0, 16, 16)), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
		}

		// Token: 0x06002578 RID: 9592 RVA: 0x001A323E File Offset: 0x001A143E
		public float GetPixelScale()
		{
			return this.pixelScale;
		}

		// Token: 0x06002579 RID: 9593 RVA: 0x001A3248 File Offset: 0x001A1448
		public Rectangle TransformDraw(Rectangle dest)
		{
			dest.X = (int)Math.Round((double)((float)dest.X * this.pixelScale)) + (int)this.upperLeft.X;
			dest.Y = (int)Math.Round((double)((float)dest.Y * this.pixelScale)) + (int)this.upperLeft.Y;
			dest.Width = (int)((float)dest.Width * this.pixelScale);
			dest.Height = (int)((float)dest.Height * this.pixelScale);
			return dest;
		}

		// Token: 0x0600257A RID: 9594 RVA: 0x001A32D4 File Offset: 0x001A14D4
		public Vector2 TransformDraw(Vector2 dest)
		{
			dest.X = (float)((int)Math.Round((double)(dest.X * this.pixelScale)) + (int)this.upperLeft.X);
			dest.Y = (float)((int)Math.Round((double)(dest.Y * this.pixelScale)) + (int)this.upperLeft.Y);
			return dest;
		}

		// Token: 0x0600257B RID: 9595 RVA: 0x001A3334 File Offset: 0x001A1534
		public bool IsPerfectVictory()
		{
			return this.points == 0 && this.throwsCount <= 6;
		}

		// Token: 0x0600257C RID: 9596 RVA: 0x001A334C File Offset: 0x001A154C
		public void changeScreenSize()
		{
			this.screenWidth = 320;
			this.screenHeight = 320;
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			int viewport_width = Game1.game1.localMultiplayerWindow.Width;
			int viewport_height = Game1.game1.localMultiplayerWindow.Height;
			this.pixelScale = Math.Min(5f, Math.Min((float)viewport_width * pixel_zoom_adjustment / (float)this.screenWidth, (float)viewport_height * pixel_zoom_adjustment / (float)this.screenHeight));
			float snap = 0.1f;
			this.pixelScale = (float)((int)(this.pixelScale / snap)) * snap;
			this.upperLeft = new Vector2((float)(viewport_width / 2) * pixel_zoom_adjustment, (float)(viewport_height / 2) * pixel_zoom_adjustment);
			this.upperLeft.X = this.upperLeft.X - (float)(this.screenWidth / 2) * this.pixelScale;
			this.upperLeft.Y = this.upperLeft.Y - (float)(this.screenHeight / 2) * this.pixelScale;
			this.dartBoardCenter = new Vector2(160f, 160f);
		}

		// Token: 0x0600257D RID: 9597 RVA: 0x001A3450 File Offset: 0x001A1650
		public void unload()
		{
			if (Darts.chargeSound != null)
			{
				Darts.chargeSound.Stop(AudioStopOptions.Immediate);
				Darts.chargeSound = null;
			}
			Game1.stopMusicTrack(MusicContext.MiniGame);
			Game1.player.faceDirection(0);
		}

		// Token: 0x0600257E RID: 9598 RVA: 0x001A347B File Offset: 0x001A167B
		public bool forceQuit()
		{
			this.unload();
			return true;
		}

		// Token: 0x0600257F RID: 9599 RVA: 0x001A3484 File Offset: 0x001A1684
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002580 RID: 9600 RVA: 0x001A3486 File Offset: 0x001A1686
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002581 RID: 9601 RVA: 0x001A348D File Offset: 0x001A168D
		public string minigameId()
		{
			return "Darts";
		}

		// Token: 0x06002582 RID: 9602 RVA: 0x001A3494 File Offset: 0x001A1694
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x040016BA RID: 5818
		public Darts.GameState currentGameState;

		// Token: 0x040016BB RID: 5819
		public float stateTimer;

		// Token: 0x040016BC RID: 5820
		public float pixelScale = 4f;

		// Token: 0x040016BD RID: 5821
		public bool gamePaused;

		// Token: 0x040016BE RID: 5822
		public Vector2 upperLeft;

		// Token: 0x040016BF RID: 5823
		private int screenWidth;

		// Token: 0x040016C0 RID: 5824
		private int screenHeight;

		// Token: 0x040016C1 RID: 5825
		private Texture2D texture;

		// Token: 0x040016C2 RID: 5826
		public Vector2 cursorPosition = new Vector2(0f, 0f);

		// Token: 0x040016C3 RID: 5827
		public Vector2 aimPosition = new Vector2(0f, 0f);

		// Token: 0x040016C4 RID: 5828
		public Vector2 dartBoardCenter = Vector2.Zero;

		// Token: 0x040016C5 RID: 5829
		protected bool canCancelShot = true;

		// Token: 0x040016C6 RID: 5830
		public float chargeTime;

		// Token: 0x040016C7 RID: 5831
		public float chargeDirection = 1f;

		// Token: 0x040016C8 RID: 5832
		public float hangTime;

		// Token: 0x040016C9 RID: 5833
		public int previousPoints;

		// Token: 0x040016CA RID: 5834
		public int points;

		// Token: 0x040016CB RID: 5835
		public float nextPointTransferTime;

		// Token: 0x040016CC RID: 5836
		public static ICue chargeSound;

		// Token: 0x040016CD RID: 5837
		public Vector2 throwStartPosition;

		// Token: 0x040016CE RID: 5838
		public Vector2 dartPosition;

		// Token: 0x040016CF RID: 5839
		public float dartTime = -1f;

		// Token: 0x040016D0 RID: 5840
		public string lastHitString = "";

		// Token: 0x040016D1 RID: 5841
		public int lastHitAmount;

		// Token: 0x040016D2 RID: 5842
		public bool shakeScore;

		// Token: 0x040016D3 RID: 5843
		public int startingDartCount = 20;

		// Token: 0x040016D4 RID: 5844
		public int dartCount = 20;

		// Token: 0x040016D5 RID: 5845
		public int throwsCount;

		// Token: 0x040016D6 RID: 5846
		public string alternateTextString = "";

		// Token: 0x040016D7 RID: 5847
		public string gameOverString = "";

		// Token: 0x040016D8 RID: 5848
		public bool lastHitWasDouble;

		// Token: 0x020005A8 RID: 1448
		public enum GameState
		{
			// Token: 0x04002C9C RID: 11420
			Aiming,
			// Token: 0x04002C9D RID: 11421
			Charging,
			// Token: 0x04002C9E RID: 11422
			Firing,
			// Token: 0x04002C9F RID: 11423
			ShowScore,
			// Token: 0x04002CA0 RID: 11424
			Scoring,
			// Token: 0x04002CA1 RID: 11425
			GameOver
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames
{
	// Token: 0x02000233 RID: 563
	public class CalicoJack : IMinigame
	{
		// Token: 0x06002535 RID: 9525 RVA: 0x0019F030 File Offset: 0x0019D230
		public CalicoJack(int toBet = -1, bool highStakes = false)
		{
			this.coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
			this.highStakes = highStakes;
			this.startTimer = 1000;
			this.playerCards = new List<int[]>();
			this.dealerCards = new List<int[]>();
			if (toBet == -1)
			{
				this.currentBet = (highStakes ? 1000 : 100);
			}
			else
			{
				this.currentBet = toBet;
			}
			Club.timesPlayedCalicoJack++;
			this.r = Utility.CreateRandom((double)Club.timesPlayedCalicoJack, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 0.0, 0.0);
			this.hit = new ClickableComponent(new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924"), 999999)), Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 64, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + "  ", 999999), 64), "", " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + " ");
			this.stand = new ClickableComponent(new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927"), 999999)), Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 32, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + "  ", 999999), 64), "", " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + " ");
			this.doubleOrNothing = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"), 999999) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"), 999999) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"));
			this.playAgain = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"), 999999) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 16, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"), 999999) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"));
			this.quit = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"), 999999) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 96, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"), 999999) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"));
			this.RepositionButtons();
			if (Game1.options.SnappyMenus)
			{
				this.currentlySnappedComponent = this.hit;
				this.currentlySnappedComponent.snapMouseCursorToCenter();
			}
		}

		// Token: 0x06002536 RID: 9526 RVA: 0x0019F4B8 File Offset: 0x0019D6B8
		public void RepositionButtons()
		{
			this.hit.bounds = new Rectangle((int)((float)Game1.game1.localMultiplayerWindow.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924"), 999999)), Game1.viewport.Height / 2 - 64, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + "  ", 999999), 64);
			this.stand.bounds = new Rectangle((int)((float)Game1.game1.localMultiplayerWindow.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927"), 999999)), Game1.viewport.Height / 2 + 32, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + "  ", 999999), 64);
			this.doubleOrNothing.bounds = new Rectangle((int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"), 999999) + 64) / 2, (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"), 999999) + 64, 64);
			this.playAgain.bounds = new Rectangle((int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"), 999999) + 64) / 2, (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel) + 64 + 16, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"), 999999) + 64, 64);
			this.quit.bounds = new Rectangle((int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"), 999999) + 64) / 2, (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel) + 64 + 96, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"), 999999) + 64, 64);
		}

		// Token: 0x06002537 RID: 9527 RVA: 0x0019F76E File Offset: 0x0019D96E
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002538 RID: 9528 RVA: 0x0019F77A File Offset: 0x0019D97A
		public bool playButtonsActive()
		{
			return this.startTimer <= 0 && this.dealerTurnTimer < 0 && !this.showingResultsScreen;
		}

		// Token: 0x06002539 RID: 9529 RVA: 0x0019F79C File Offset: 0x0019D99C
		public bool tick(GameTime time)
		{
			for (int i = 0; i < this.playerCards.Count; i++)
			{
				if (this.playerCards[i][1] > 0)
				{
					this.playerCards[i][1] -= time.ElapsedGameTime.Milliseconds;
					if (this.playerCards[i][1] <= 0)
					{
						this.playerCards[i][1] = 0;
					}
				}
			}
			for (int j = 0; j < this.dealerCards.Count; j++)
			{
				if (this.dealerCards[j][1] > 0)
				{
					this.dealerCards[j][1] -= time.ElapsedGameTime.Milliseconds;
					if (this.dealerCards[j][1] <= 0)
					{
						this.dealerCards[j][1] = 0;
					}
				}
			}
			if (this.startTimer > 0)
			{
				int oldTimer = this.startTimer;
				this.startTimer -= time.ElapsedGameTime.Milliseconds;
				if (oldTimer % 250 < this.startTimer % 250)
				{
					switch (oldTimer / 250)
					{
					case 1:
						this.playerCards.Add(new int[]
						{
							this.r.Next(1, 10),
							400
						});
						break;
					case 2:
						this.playerCards.Add(new int[]
						{
							this.r.Next(1, 12),
							400
						});
						break;
					case 3:
						this.dealerCards.Add(new int[]
						{
							this.r.Next(1, 10),
							400
						});
						break;
					case 4:
						this.dealerCards.Add(new int[]
						{
							this.r.Next(1, 12),
							-1
						});
						break;
					}
					Game1.playSound("shwip", null);
				}
			}
			else if (this.bustTimer > 0)
			{
				this.bustTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.bustTimer <= 0)
				{
					this.endGame();
				}
			}
			else if (this.dealerTurnTimer > 0 && !this.showingResultsScreen)
			{
				this.dealerTurnTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.dealerTurnTimer <= 0)
				{
					int dealerTotal = 0;
					foreach (int[] k in this.dealerCards)
					{
						dealerTotal += k[0];
					}
					int playertotal = 0;
					foreach (int[] l in this.playerCards)
					{
						playertotal += l[0];
					}
					if (this.dealerCards[0][1] == -1)
					{
						this.dealerCards[0][1] = 400;
						Game1.playSound("shwip", null);
					}
					else if (dealerTotal < 18 || (dealerTotal < playertotal && playertotal <= 21))
					{
						int nextCard = this.r.Next(1, 10);
						int dealerDistance = 21 - dealerTotal;
						if (playertotal == 20 && this.r.NextBool())
						{
							nextCard = dealerDistance + this.r.Next(1, 4);
						}
						else if (playertotal == 19 && this.r.NextDouble() < 0.25)
						{
							nextCard = dealerDistance + this.r.Next(1, 4);
						}
						else if (playertotal == 18 && this.r.NextDouble() < 0.1)
						{
							nextCard = dealerDistance + this.r.Next(1, 4);
						}
						if (this.r.NextDouble() < Math.Max(0.0005, 0.001 + Game1.player.DailyLuck / 20.0 + (double)((float)Game1.player.LuckLevel * 0.002f)))
						{
							nextCard = 999;
							this.currentBet *= 3;
						}
						this.dealerCards.Add(new int[]
						{
							nextCard,
							400
						});
						dealerTotal += this.dealerCards.Last<int[]>()[0];
						Game1.playSound((nextCard == 999) ? "batScreech" : "shwip", null);
						if (dealerTotal > 21)
						{
							this.bustTimer = 2000;
						}
					}
					else
					{
						this.bustTimer = 50;
					}
					this.dealerTurnTimer = 1000;
				}
			}
			if (this.playButtonsActive())
			{
				this.hit.scale = (this.hit.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
				this.stand.scale = (this.stand.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			}
			else if (this.showingResultsScreen)
			{
				this.doubleOrNothing.scale = (this.doubleOrNothing.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
				this.playAgain.scale = (this.playAgain.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
				this.quit.scale = (this.quit.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			}
			return false;
		}

		// Token: 0x0600253A RID: 9530 RVA: 0x0019FDA8 File Offset: 0x0019DFA8
		public void endGame()
		{
			if (Game1.options.SnappyMenus)
			{
				this.currentlySnappedComponent = this.quit;
				this.currentlySnappedComponent.snapMouseCursorToCenter();
			}
			this.showingResultsScreen = true;
			int playertotal = 0;
			foreach (int[] i in this.playerCards)
			{
				playertotal += i[0];
			}
			if (playertotal == 21)
			{
				Game1.playSound("reward", null);
				this.playerWon = true;
				this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11944");
				Game1.player.clubCoins += this.currentBet;
				return;
			}
			if (playertotal > 21)
			{
				Game1.playSound("fishEscape", null);
				this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
				this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11947");
				Game1.player.clubCoins -= this.currentBet;
				if (Game1.player.clubCoins < 0)
				{
					Game1.player.clubCoins = 0;
				}
				return;
			}
			int dealerTotal = 0;
			foreach (int[] j in this.dealerCards)
			{
				dealerTotal += j[0];
			}
			if (dealerTotal > 21)
			{
				Game1.playSound("reward", null);
				this.playerWon = true;
				this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11950");
				Game1.player.clubCoins += this.currentBet;
				return;
			}
			if (playertotal == dealerTotal)
			{
				this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11951");
				this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11952");
				return;
			}
			if (playertotal > dealerTotal)
			{
				Game1.playSound("reward", null);
				this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11955", 21);
				Game1.player.clubCoins += this.currentBet;
				this.playerWon = true;
				return;
			}
			Game1.playSound("fishEscape", null);
			this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
			this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11958", 21);
			Game1.player.clubCoins -= this.currentBet;
			if (Game1.player.clubCoins < 0)
			{
				Game1.player.clubCoins = 0;
			}
		}

		// Token: 0x0600253B RID: 9531 RVA: 0x001A00B4 File Offset: 0x0019E2B4
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.playButtonsActive() && this.bustTimer <= 0)
			{
				if (this.hit.bounds.Contains(x, y))
				{
					int playertotal = 0;
					foreach (int[] i in this.playerCards)
					{
						playertotal += i[0];
					}
					int nextCard = this.r.Next(1, 10);
					int distance = 21 - playertotal;
					if (distance > 1 && distance < 6 && this.r.NextDouble() < (double)(1f / (float)distance))
					{
						nextCard = this.r.Choose(distance, distance - 1);
					}
					this.playerCards.Add(new int[]
					{
						nextCard,
						400
					});
					Game1.playSound("shwip", null);
					int total = 0;
					foreach (int[] j in this.playerCards)
					{
						total += j[0];
					}
					if (total == 21)
					{
						this.bustTimer = 1000;
					}
					else if (total > 21)
					{
						this.bustTimer = 1000;
					}
				}
				if (this.stand.bounds.Contains(x, y))
				{
					this.dealerTurnTimer = 1000;
					Game1.playSound("coin", null);
					return;
				}
			}
			else if (this.showingResultsScreen)
			{
				if (this.playerWon && this.doubleOrNothing.containsPoint(x, y))
				{
					Game1.currentMinigame = new CalicoJack(this.currentBet * 2, this.highStakes);
					Game1.playSound("bigSelect", null);
				}
				if (Game1.player.clubCoins >= this.currentBet && this.playAgain.containsPoint(x, y))
				{
					Game1.currentMinigame = new CalicoJack(-1, this.highStakes);
					Game1.playSound("smallSelect", null);
				}
				if (this.quit.containsPoint(x, y))
				{
					Game1.currentMinigame = null;
					Game1.playSound("bigDeSelect", null);
				}
			}
		}

		// Token: 0x0600253C RID: 9532 RVA: 0x001A0310 File Offset: 0x0019E510
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x0600253D RID: 9533 RVA: 0x001A0312 File Offset: 0x0019E512
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600253E RID: 9534 RVA: 0x001A0314 File Offset: 0x0019E514
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x0600253F RID: 9535 RVA: 0x001A0316 File Offset: 0x0019E516
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x06002540 RID: 9536 RVA: 0x001A0318 File Offset: 0x0019E518
		public void receiveKeyPress(Keys k)
		{
			if (Game1.options.SnappyMenus && this.currentlySnappedComponent != null)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
				{
					if (this.currentlySnappedComponent.Equals(this.stand))
					{
						this.currentlySnappedComponent = this.hit;
					}
					else if (this.currentlySnappedComponent.Equals(this.playAgain) && this.playerWon)
					{
						this.currentlySnappedComponent = this.doubleOrNothing;
					}
					else if (this.currentlySnappedComponent.Equals(this.quit) && Game1.player.clubCoins >= this.currentBet)
					{
						this.currentlySnappedComponent = this.playAgain;
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
				{
					if (this.currentlySnappedComponent.Equals(this.hit))
					{
						this.currentlySnappedComponent = this.stand;
					}
					else if (this.currentlySnappedComponent.Equals(this.doubleOrNothing))
					{
						this.currentlySnappedComponent = this.playAgain;
					}
					else if (this.currentlySnappedComponent.Equals(this.playAgain))
					{
						this.currentlySnappedComponent = this.quit;
					}
				}
				this.currentlySnappedComponent.snapMouseCursorToCenter();
			}
		}

		// Token: 0x06002541 RID: 9537 RVA: 0x001A0465 File Offset: 0x0019E665
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x06002542 RID: 9538 RVA: 0x001A0468 File Offset: 0x0019E668
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), this.highStakes ? new Color(130, 0, 82) : Color.DarkGreen);
			Vector2 coin_draw_pos = new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width - 192), 32f);
			SpriteText.drawStringWithScrollBackground(b, this.coinBuffer + Game1.player.clubCoins.ToString(), (int)coin_draw_pos.X, (int)coin_draw_pos.Y, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(coin_draw_pos.X + 4f, coin_draw_pos.Y + 4f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
			if (this.showingResultsScreen)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, this.endMessage, Game1.graphics.GraphicsDevice.Viewport.Width / 2, 48, "", 1f, null, 0, 0.88f, false);
				SpriteText.drawStringWithScrollCenteredAt(b, this.endTitle, Game1.graphics.GraphicsDevice.Viewport.Width / 2, 128, "", 1f, null, 0, 0.88f, false);
				if (!this.endTitle.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11951")))
				{
					SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11965", (this.playerWon ? "" : "-") + this.currentBet.ToString() + "   "), Game1.graphics.GraphicsDevice.Viewport.Width / 2, 256, "", 1f, null, 0, 0.88f, false);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 32 + SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11965", (this.playerWon ? "" : "-") + this.currentBet.ToString() + "   "), 999999) / 2), 260f) + new Vector2(8f, 0f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				}
				if (this.playerWon)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.doubleOrNothing.bounds.X, this.doubleOrNothing.bounds.Y, this.doubleOrNothing.bounds.Width, this.doubleOrNothing.bounds.Height, Color.White, 4f * this.doubleOrNothing.scale, true, -1f);
					SpriteText.drawString(b, this.doubleOrNothing.label, this.doubleOrNothing.bounds.X + 32, this.doubleOrNothing.bounds.Y + 8, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
				if (Game1.player.clubCoins >= this.currentBet)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.playAgain.bounds.X, this.playAgain.bounds.Y, this.playAgain.bounds.Width, this.playAgain.bounds.Height, Color.White, 4f * this.playAgain.scale, true, -1f);
					SpriteText.drawString(b, this.playAgain.label, this.playAgain.bounds.X + 32, this.playAgain.bounds.Y + 8, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.quit.bounds.X, this.quit.bounds.Y, this.quit.bounds.Width, this.quit.bounds.Height, Color.White, 4f * this.quit.scale, true, -1f);
				SpriteText.drawString(b, this.quit.label, this.quit.bounds.X + 32, this.quit.bounds.Y + 8, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}
			else
			{
				Vector2 start = new Vector2(128f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 320));
				int total = 0;
				foreach (int[] i in this.playerCards)
				{
					int cardHeight = 144;
					if (i[1] > 0)
					{
						cardHeight = (int)(Math.Abs((float)i[1] - 200f) / 200f * 144f);
					}
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, (i[1] > 200 || i[1] == -1) ? new Rectangle(399, 396, 15, 15) : new Rectangle(384, 396, 15, 15), (int)start.X, (int)start.Y + 72 - cardHeight / 2, 96, cardHeight, Color.White, 4f, true, -1f);
					if (i[1] == 0)
					{
						SpriteText.drawStringHorizontallyCenteredAt(b, i[0].ToString() ?? "", (int)start.X + 48 - 8 + 4, (int)start.Y + 72 - 16, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
					}
					start.X += 112f;
					if (i[1] == 0)
					{
						total += i[0];
					}
				}
				SpriteText.drawStringWithScrollBackground(b, Game1.player.Name + ": " + total.ToString(), 160, (int)start.Y + 144 + 32, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				start.X = 128f;
				start.Y = 128f;
				total = 0;
				foreach (int[] j in this.dealerCards)
				{
					int cardHeight2 = 144;
					if (j[1] > 0)
					{
						cardHeight2 = (int)(Math.Abs((float)j[1] - 200f) / 200f * 144f);
					}
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, (j[1] > 200 || j[1] == -1) ? new Rectangle(399, 396, 15, 15) : new Rectangle(384, 396, 15, 15), (int)start.X, (int)start.Y + 72 - cardHeight2 / 2, 96, cardHeight2, Color.White, 4f, true, -1f);
					if (j[1] == 0)
					{
						if (j[0] == 999)
						{
							b.Draw(Game1.objectSpriteSheet, new Vector2(start.X + 48f - 32f, start.Y + 72f - 32f), new Rectangle?(new Rectangle(16, 592, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
						}
						else
						{
							SpriteText.drawStringHorizontallyCenteredAt(b, j[0].ToString() ?? "", (int)start.X + 48 - 8 + 4, (int)start.Y + 72 - 16, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
						}
					}
					start.X += 112f;
					int num = j[1];
					if (num != -1)
					{
						if (num == 0)
						{
							total += j[0];
						}
					}
					else
					{
						total = -99999;
					}
				}
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11970", (total >= 999) ? "!!!" : ((total > 0) ? (total.ToString() ?? "") : "?")), 160, 32, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11972", this.currentBet.ToString() + this.coinBuffer), 160, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 48, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(172 + SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11972", this.currentBet), 999999)), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 4 - 48)), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				if (this.playButtonsActive())
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.hit.bounds.X, this.hit.bounds.Y, this.hit.bounds.Width, this.hit.bounds.Height, Color.White, 4f * this.hit.scale, true, -1f);
					SpriteText.drawString(b, this.hit.label, this.hit.bounds.X + 8, this.hit.bounds.Y + 8, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.stand.bounds.X, this.stand.bounds.Y, this.stand.bounds.Width, this.stand.bounds.Height, Color.White, 4f * this.stand.scale, true, -1f);
					SpriteText.drawString(b, this.stand.label, this.stand.bounds.X + 8, this.stand.bounds.Y + 8, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
			}
			if (Game1.IsMultiplayer)
			{
				Utility.drawTextWithColoredShadow(b, Game1.getTimeOfDayString(Game1.timeOfDay), Game1.dialogueFont, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width - Game1.dialogueFont.MeasureString(Game1.getTimeOfDayString(Game1.timeOfDay)).X - 16f, (float)Game1.graphics.GraphicsDevice.Viewport.Height - Game1.dialogueFont.MeasureString(Game1.getTimeOfDayString(Game1.timeOfDay)).Y - 10f), Color.White, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
			}
			if (!Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
		}

		// Token: 0x06002543 RID: 9539 RVA: 0x001A125C File Offset: 0x0019F45C
		public void changeScreenSize()
		{
			this.RepositionButtons();
		}

		// Token: 0x06002544 RID: 9540 RVA: 0x001A1264 File Offset: 0x0019F464
		public void unload()
		{
		}

		// Token: 0x06002545 RID: 9541 RVA: 0x001A1266 File Offset: 0x0019F466
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x06002546 RID: 9542 RVA: 0x001A1268 File Offset: 0x0019F468
		public string minigameId()
		{
			return "CalicoJack";
		}

		// Token: 0x06002547 RID: 9543 RVA: 0x001A126F File Offset: 0x0019F46F
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x06002548 RID: 9544 RVA: 0x001A1272 File Offset: 0x0019F472
		public bool forceQuit()
		{
			return true;
		}

		// Token: 0x0400168E RID: 5774
		public const int cardState_flipped = -1;

		// Token: 0x0400168F RID: 5775
		public const int cardState_up = 0;

		// Token: 0x04001690 RID: 5776
		public const int cardState_transitioning = 400;

		// Token: 0x04001691 RID: 5777
		public const int bet = 100;

		// Token: 0x04001692 RID: 5778
		public const int cardWidth = 96;

		// Token: 0x04001693 RID: 5779
		public const int dealTime = 1000;

		// Token: 0x04001694 RID: 5780
		public const int playingTo = 21;

		// Token: 0x04001695 RID: 5781
		public const int passNumber = 18;

		// Token: 0x04001696 RID: 5782
		public const int dealerTurnDelay = 1000;

		// Token: 0x04001697 RID: 5783
		public List<int[]> playerCards;

		// Token: 0x04001698 RID: 5784
		public List<int[]> dealerCards;

		// Token: 0x04001699 RID: 5785
		private Random r;

		// Token: 0x0400169A RID: 5786
		private int currentBet;

		// Token: 0x0400169B RID: 5787
		private int startTimer;

		// Token: 0x0400169C RID: 5788
		private int dealerTurnTimer = -1;

		// Token: 0x0400169D RID: 5789
		private int bustTimer;

		// Token: 0x0400169E RID: 5790
		private ClickableComponent hit;

		// Token: 0x0400169F RID: 5791
		private ClickableComponent stand;

		// Token: 0x040016A0 RID: 5792
		private ClickableComponent doubleOrNothing;

		// Token: 0x040016A1 RID: 5793
		private ClickableComponent playAgain;

		// Token: 0x040016A2 RID: 5794
		private ClickableComponent quit;

		// Token: 0x040016A3 RID: 5795
		private ClickableComponent currentlySnappedComponent;

		// Token: 0x040016A4 RID: 5796
		private bool showingResultsScreen;

		// Token: 0x040016A5 RID: 5797
		private bool playerWon;

		// Token: 0x040016A6 RID: 5798
		private bool highStakes;

		// Token: 0x040016A7 RID: 5799
		private string endMessage = "";

		// Token: 0x040016A8 RID: 5800
		private string endTitle = "";

		// Token: 0x040016A9 RID: 5801
		private string coinBuffer;
	}
}

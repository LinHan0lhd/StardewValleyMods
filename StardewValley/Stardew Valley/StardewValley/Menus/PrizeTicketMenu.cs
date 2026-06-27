using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Menus
{
	// Token: 0x0200029B RID: 667
	public class PrizeTicketMenu : IClickableMenu
	{
		// Token: 0x06002B99 RID: 11161 RVA: 0x00211168 File Offset: 0x0020F368
		public PrizeTicketMenu() : base((int)Utility.getTopLeftPositionForCenteringOnScreen(464, 376, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(464, 376, 0, 0).Y, 464, 376, true)
		{
			this.texture = Game1.content.Load<Texture2D>("LooseSprites\\PrizeTicketMenu");
			this.mainButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 192, this.yPositionOnScreen + 216, 92, 88), this.texture, new Rectangle(150, 29, 23, 22), 4f, false);
			Game1.playSound("machine_bell", null);
			this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)Game1.stats.Get("ticketPrizesClaimed")));
			this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 1U)));
			this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 2U)));
			this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 3U)));
			this.currentlySnappedComponent = this.mainButton;
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B9A RID: 11162 RVA: 0x002112C4 File Offset: 0x0020F4C4
		public override void performHoverAction(int x, int y)
		{
			if (this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && !this.gettingReward && !this.movingRewardTrack)
			{
				if (this.mainButton.sourceRect.Y == 29)
				{
					Game1.playSound("button_tap", null);
				}
				this.mainButton.sourceRect.Y = 51;
			}
			else
			{
				this.mainButton.sourceRect.Y = 29;
			}
			base.performHoverAction(x, y);
		}

		// Token: 0x06002B9B RID: 11163 RVA: 0x00211354 File Offset: 0x0020F554
		public static Item getPrizeItem(int prizeLevel)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.player.UniqueMultiplayerID, 0.0, 0.0, 0.0);
			switch (prizeLevel)
			{
			case 0:
				return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, r, 12);
			case 1:
				return ItemRegistry.Create(r.Choose("(O)631", "(O)630"), 1, 0, false);
			case 2:
				return r.Choose(ItemRegistry.Create("(O)770", 10, 0, false), ItemRegistry.Create("(O)MixedFlowerSeeds", 15, 0, false));
			case 3:
				return ItemRegistry.Create("(O)MysteryBox", 3, 0, false);
			case 4:
				return ItemRegistry.Create("(O)StardropTea", 1, 0, false);
			case 5:
				return ItemRegistry.Create((Game1.player.HouseUpgradeLevel > 0) ? "(F)BluePinstripeDoubleBed" : "(F)BluePinstripeBed", 1, 0, false);
			case 6:
				return ItemRegistry.Create(r.Choose("(O)621", "(BC)15", "(BC)MushroomLog"), 4, 0, false);
			case 7:
				return ItemRegistry.Create(r.Choose("(O)633", "(O)632"), 1, 0, false);
			case 8:
				return ItemRegistry.Create("(O)Book_Friendship", 1, 0, false);
			case 9:
				return r.Choose(ItemRegistry.Create("(O)286", 20, 0, false), ItemRegistry.Create("(O)287", 12, 0, false), ItemRegistry.Create("(O)288", 6, 0, false));
			case 10:
				return ItemRegistry.Create("(H)SportsCap", 1, 0, false);
			case 11:
				return ItemRegistry.Create(r.Choose("(BC)FishSmoker", "(BC)Dehydrator"), 1, 0, false);
			case 12:
				return ItemRegistry.Create(r.Choose("(O)275", "(O)MysteryBox"), 4, 0, false);
			case 13:
				return ItemRegistry.Create(r.Choose("(F)FancyHousePlant1", "(F)FancyHousePlant2", "(F)FancyHousePlant3"), 1, 0, false);
			case 14:
				return ItemRegistry.Create("(O)SkillBook_" + r.Next(5).ToString(), 1, 0, false);
			case 15:
				return ItemRegistry.Create("(O)StardropTea", 1, 0, false);
			case 16:
				return ItemRegistry.Create("(F)CowDecal", 1, 0, false);
			case 17:
				return ItemRegistry.Create("(O)749", 8, 0, false);
			case 18:
				return ItemRegistry.Create(r.Choose("(BC)10", "(BC)12"), 4, 0, false);
			case 19:
				return ItemRegistry.Create("(O)72", 5, 0, false);
			case 20:
				return ItemRegistry.Create("(O)MysteryBox", 5, 0, false);
			case 21:
				return ItemRegistry.Create("(O)279", 1, 0, false);
			default:
			{
				Random r2 = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)(prizeLevel - prizeLevel % 9), 0.0, 0.0, 0.0);
				switch (prizeLevel % 9)
				{
				case 0:
					return ItemRegistry.Create("(O)MysteryBox", 5, 0, false);
				case 1:
					return ItemRegistry.Create("(O)872", r2.Next(1, 3), 0, false);
				case 2:
					return ItemRegistry.Create(r2.Choose(new string[]
					{
						"(O)337",
						"(O)226",
						"(O)253",
						"(O)732",
						"(O)275"
					}), 5, 0, false);
				case 3:
					return ItemRegistry.Create(r2.Choose("(F)FancyHousePlant1", "(F)FancyHousePlant2", "(F)FancyHousePlant3"), 1, 0, false);
				case 4:
					return ItemRegistry.Create("(O)StardropTea", 1, 0, false);
				case 5:
					return ItemRegistry.Create("(O)166", 1, 0, false);
				case 6:
					return ItemRegistry.Create("(O)645", 1, 0, false);
				case 7:
					return ItemRegistry.Create(r2.Choose("(F)FancyTree1", "(F)FancyTree2", "(F)FancyTree3", "(F)PigPainting"), 1, 0, false);
				case 8:
					return r2.Choose(ItemRegistry.Create("(O)287", 15, 0, false), ItemRegistry.Create("(O)288", 8, 0, false));
				default:
					return ItemRegistry.Create("MysteryBox", 5, 0, false);
				}
				break;
			}
			}
		}

		// Token: 0x06002B9C RID: 11164 RVA: 0x0021173E File Offset: 0x0020F93E
		public override bool readyToClose()
		{
			return !this.gettingReward && base.readyToClose();
		}

		// Token: 0x06002B9D RID: 11165 RVA: 0x00211750 File Offset: 0x0020F950
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.gettingReward)
			{
				return;
			}
			if (this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && !this.movingRewardTrack)
			{
				Game1.playSound("button_press", null);
				this.pressedButtonTimer = 200f;
				if (Game1.player.Items.CountId("PrizeTicket") > 0)
				{
					this.gettingReward = true;
					this.getRewardTimer = 0f;
					DelayedAction.playSoundAfterDelay("discoverMineral", 750, null, null, -1, false);
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002B9E RID: 11166 RVA: 0x002117F8 File Offset: 0x0020F9F8
		public override void update(GameTime time)
		{
			if (this.pressedButtonTimer > 0f)
			{
				this.pressedButtonTimer -= (float)((int)time.ElapsedGameTime.TotalMilliseconds);
				this.mainButton.sourceRect.Y = 73;
			}
			if (this.pressedButtonTimer <= 0f && this.gettingReward)
			{
				this.getRewardTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.getRewardTimer > 2000f)
				{
					this.getRewardTimer = 2000f;
					Game1.playSound("coin", null);
					if (!Game1.player.addItemToInventoryBool(this.currentPrizeTrack[0], false))
					{
						Game1.createItemDebris(this.currentPrizeTrack[0], Game1.player.getStandingPosition(), 1, Game1.player.currentLocation, -1, false);
					}
					Game1.player.Items.ReduceId("PrizeTicket", 1);
					Game1.stats.Increment("ticketPrizesClaimed", 1U);
					this.currentPrizeTrack.RemoveAt(0);
					this.moveRewardTrackPreTimer = 500f;
					this.gettingReward = false;
					this.movingRewardTrack = true;
					this.moveRewardTrackTimer = 0f;
				}
			}
			else if (this.movingRewardTrack)
			{
				if (this.moveRewardTrackPreTimer > 0f)
				{
					this.moveRewardTrackPreTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					if (this.moveRewardTrackPreTimer <= 0f)
					{
						Game1.playSound("ticket_machine_whir", null);
					}
				}
				else
				{
					this.moveRewardTrackTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
					if (this.moveRewardTrackTimer >= 2000f)
					{
						this.movingRewardTrack = false;
						this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 3U)));
					}
				}
			}
			base.update(time);
		}

		// Token: 0x06002B9F RID: 11167 RVA: 0x002119F4 File Offset: 0x0020FBF4
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			b.Draw(this.texture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen) + new Vector2(25f, 18f) * 4f, new Rectangle?(new Rectangle(0, 106, 76, 22)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);
			for (int i = 0; i < this.currentPrizeTrack.Count; i++)
			{
				Vector2 posOffset = new Vector2((float)(28 + 22 * i), 21f) * 4f;
				if (this.movingRewardTrack)
				{
					float xOffset = 88f - this.moveRewardTrackTimer / 18f;
					if (xOffset > 0f)
					{
						posOffset.X += xOffset;
						if (this.moveRewardTrackPreTimer <= 0f)
						{
							posOffset.X += (float)Game1.random.Next(-1, 2);
							posOffset.Y += (float)Game1.random.Next(-1, 2);
						}
					}
				}
				if (i == 0)
				{
					b.Draw(Game1.fadeToBlackRect, new Rectangle((int)base.Position.X + 100, (int)base.Position.Y + 76, 88, 80), Color.LightYellow * 0.33f);
				}
				if (!this.gettingReward || i != 0)
				{
					this.currentPrizeTrack[i].drawInMenu(b, base.Position + posOffset, 1f);
				}
			}
			b.Draw(this.texture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(0, 0, 116, 94)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			if (this.gettingReward)
			{
				Vector2 posOffset2 = new Vector2(28f, 21f) * 4f;
				posOffset2.Y -= this.getRewardTimer / 13f;
				posOffset2.Y = Math.Max(posOffset2.Y, 0f);
				posOffset2.X += this.getRewardTimer / 1000f * (float)Game1.random.Next(-1, 2);
				posOffset2.Y += this.getRewardTimer / 1000f * (float)Game1.random.Next(-1, 2);
				this.currentPrizeTrack[0].drawInMenu(b, base.Position + posOffset2, 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, false);
			}
			string ticketCount = Game1.player.Items.CountId("PrizeTicket").ToString() ?? "";
			SpriteText.drawString(b, ticketCount, this.xPositionOnScreen + 360 - SpriteText.getWidthOfString(ticketCount, 999999) / 2, this.yPositionOnScreen + 276, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			this.mainButton.draw(b);
			base.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001D3C RID: 7484
		public const int WIDTH = 116;

		// Token: 0x04001D3D RID: 7485
		public const int HEIGHT = 94;

		// Token: 0x04001D3E RID: 7486
		public Texture2D texture;

		// Token: 0x04001D3F RID: 7487
		public ClickableTextureComponent mainButton;

		// Token: 0x04001D40 RID: 7488
		public float pressedButtonTimer;

		// Token: 0x04001D41 RID: 7489
		public List<Item> currentPrizeTrack = new List<Item>();

		// Token: 0x04001D42 RID: 7490
		public float getRewardTimer;

		// Token: 0x04001D43 RID: 7491
		public float moveRewardTrackTimer;

		// Token: 0x04001D44 RID: 7492
		public float moveRewardTrackPreTimer;

		// Token: 0x04001D45 RID: 7493
		public bool gettingReward;

		// Token: 0x04001D46 RID: 7494
		public bool movingRewardTrack;
	}
}

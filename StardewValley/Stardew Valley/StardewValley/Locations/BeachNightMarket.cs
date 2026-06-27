using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002C1 RID: 705
	public class BeachNightMarket : GameLocation
	{
		// Token: 0x06002DC2 RID: 11714 RVA: 0x0023C692 File Offset: 0x0023A892
		public BeachNightMarket()
		{
			this.forceLoadPathLayerLights = true;
		}

		// Token: 0x06002DC3 RID: 11715 RVA: 0x0023C6A1 File Offset: 0x0023A8A1
		public BeachNightMarket(string mapPath, string name) : base(mapPath, name)
		{
			this.forceLoadPathLayerLights = true;
		}

		// Token: 0x06002DC4 RID: 11716 RVA: 0x0023C6B4 File Offset: 0x0023A8B4
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.objects.Clear();
			this.hasReceivedFreeGift = false;
			this.paintingMailKey = string.Concat(new string[]
			{
				"NightMarketYear",
				Game1.year.ToString(),
				"Day",
				this.getDayOfNightMarket().ToString(),
				"_paintingSold"
			});
		}

		// Token: 0x06002DC5 RID: 11717 RVA: 0x0023C724 File Offset: 0x0023A924
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (Game1.timeOfDay < 1700)
			{
				b.Draw(this.shopClosedTexture, Game1.GlobalToLocal(new Vector2(39f, 29f) * 64f + new Vector2(-1f, -3f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(72, 167, 16, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.shopClosedTexture, Game1.GlobalToLocal(new Vector2(47f, 34f) * 64f + new Vector2(7f, -3f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(45, 170, 26, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(this.shopClosedTexture, Game1.GlobalToLocal(new Vector2(19f, 31f) * 64f + new Vector2(6f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(89, 164, 18, 23)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			}
			if (!Game1.player.mailReceived.Contains(this.paintingMailKey))
			{
				b.Draw(this.shopClosedTexture, Game1.GlobalToLocal(new Vector2(41f, 33f) * 64f + new Vector2(2f, 2f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(144 + (this.getDayOfNightMarket() - 1 + (Game1.year - 1) % 3 * 3) * 28, 201, 28, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22500001f);
			}
		}

		// Token: 0x06002DC6 RID: 11718 RVA: 0x0023C958 File Offset: 0x0023AB58
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "night market");
			if (tileIndexAt <= 595)
			{
				switch (tileIndexAt)
				{
				case 68:
					if (Game1.timeOfDay < 1700)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterClosed"));
						goto IL_22C;
					}
					if (Game1.player.mailReceived.Contains(this.paintingMailKey))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterSold"));
						goto IL_22C;
					}
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterQuestion"), base.createYesNoResponses(), "PainterQuestion");
					goto IL_22C;
				case 69:
					break;
				case 70:
					Utility.TryOpenShopMenu("Festival_NightMarket_MagicBoat_Day" + this.getDayOfNightMarket().ToString(), this, null, null, false, true, null);
					goto IL_22C;
				default:
					if (tileIndexAt == 399)
					{
						Utility.TryOpenShopMenu("Traveler", this, null, null, false, true, null);
						goto IL_22C;
					}
					if (tileIndexAt != 595)
					{
						goto IL_22C;
					}
					Utility.TryOpenShopMenu("Festival_NightMarket_DecorationBoat", this, null, null, false, true, null);
					goto IL_22C;
				}
			}
			else if (tileIndexAt != 653)
			{
				if (tileIndexAt != 877)
				{
					if (tileIndexAt != 1285)
					{
						goto IL_22C;
					}
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), base.createYesNoResponses(), "WarperQuestion");
					goto IL_22C;
				}
			}
			else
			{
				if (Game1.RequireLocation<Submarine>("Submarine", false).submerged.Value || Game1.netWorldState.Value.IsSubmarineLocked)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_SubmarineInUse"));
					return true;
				}
				goto IL_22C;
			}
			if (Game1.timeOfDay < 1700)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverClosed"));
			}
			else if (!this.hasReceivedFreeGift)
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverQuestion"), base.createYesNoResponses(), "GiftGiverQuestion");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverEnjoy"));
			}
			IL_22C:
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002DC7 RID: 11719 RVA: 0x0023CB9A File Offset: 0x0023AD9A
		public int getDayOfNightMarket()
		{
			return Utility.GetDayOfPassiveFestival("NightMarket");
		}

		// Token: 0x06002DC8 RID: 11720 RVA: 0x0023CBA8 File Offset: 0x0023ADA8
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "WarperQuestion_Yes"))
			{
				if (!(questionAndAnswer == "PainterQuestion_Yes"))
				{
					if (questionAndAnswer == "GiftGiverQuestion_Yes")
					{
						if (this.hasReceivedFreeGift)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_GiftGiverEnjoy"));
						}
						else
						{
							Game1.player.freezePause = 5000;
							this.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = this.shopClosedTexture,
								layerDepth = 0.2442f,
								scale = 4f,
								sourceRectStartingPos = new Vector2(354f, 168f),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(354, 168, 32, 32),
								animationLength = 1,
								id = 777,
								holdLastFrame = true,
								interval = 250f,
								position = new Vector2(13f, 36f) * 64f,
								delayBeforeAnimationStart = 500,
								endFunction = new TemporaryAnimatedSprite.endBehavior(this.getFreeGiftPartOne)
							});
							this.hasReceivedFreeGift = true;
						}
					}
				}
				else if (Game1.player.mailReceived.Contains(this.paintingMailKey))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_PainterSold"));
				}
				else if (Game1.player.Money < 1200)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
				}
				else
				{
					Game1.player.Money -= 1200;
					Game1.activeClickableMenu = null;
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(F)" + (1838 + ((this.getDayOfNightMarket() - 1) * 2 + (Game1.year - 1) % 3 * 6)).ToString(), 1, 0, false), null, false);
					Game1.multiplayer.globalChatInfoMessage("Lupini", new string[]
					{
						Game1.player.Name
					});
					Game1.multiplayer.broadcastPartyWideMail(this.paintingMailKey, Multiplayer.PartyWideMessageQueue.SeenMail, true);
				}
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			if (Game1.player.Money < 250)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 250;
				Game1.player.CanMove = true;
				ItemRegistry.Create<Object>("(O)688", 1, 0, false).performUseAction(this);
				Game1.player.freezePause = 5000;
			}
			return true;
		}

		// Token: 0x06002DC9 RID: 11721 RVA: 0x0023CE4C File Offset: 0x0023B04C
		public void getFreeGiftPartOne(int extra)
		{
			base.removeTemporarySpritesWithIDLocal(777);
			Game1.playSound("Milking", null);
			this.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = this.shopClosedTexture,
				layerDepth = 0.2442f,
				scale = 4f,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(386, 168, 32, 32),
				animationLength = 1,
				id = 778,
				holdLastFrame = true,
				interval = 9500f,
				position = new Vector2(13f, 36f) * 64f
			});
			for (int i = 0; i <= 2000; i += 100)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.shopClosedTexture,
					delayBeforeAnimationStart = i,
					id = 778,
					layerDepth = 0.24430001f,
					scale = 4f,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(362, 170, 2, 2),
					animationLength = 1,
					interval = 100f,
					position = new Vector2(13f, 36f) * 64f + new Vector2(8f, 12f) * 4f,
					motion = new Vector2(0f, 2f),
					endFunction = ((i == 2000) ? new TemporaryAnimatedSprite.endBehavior(this.getFreeGift) : null)
				});
			}
		}

		// Token: 0x06002DCA RID: 11722 RVA: 0x0023CFF9 File Offset: 0x0023B1F9
		public void getFreeGift(int extra)
		{
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)395", 1, 0, false), null, false);
			base.removeTemporarySpritesWithIDLocal(778);
		}

		// Token: 0x06002DCB RID: 11723 RVA: 0x0023D020 File Offset: 0x0023B220
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				this.hasShownCCUpgrade = false;
			}
			if (Game1.RequireLocation<Beach>("Beach", false).bridgeFixed.Value || NetWorldState.checkAnywhereForWorldStateID("beachBridgeFixed"))
			{
				Beach.fixBridge(this);
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				Beach.showCommunityUpgradeShortcuts(this, ref this.hasShownCCUpgrade);
			}
		}

		// Token: 0x06002DCC RID: 11724 RVA: 0x0023D08C File Offset: 0x0023B28C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.timeOfDay >= 1700)
			{
				Game1.changeMusicTrack("night_market", false, MusicContext.Default);
			}
			else
			{
				Game1.changeMusicTrack("ocean", false, MusicContext.Default);
			}
			this.shopClosedTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			this.temporarySprites.Add(new EmilysParrot(new Vector2(2968f, 2056f)));
			this.paintingMailKey = string.Concat(new string[]
			{
				"NightMarketYear",
				Game1.year.ToString(),
				"Day",
				this.getDayOfNightMarket().ToString(),
				"_paintingSold"
			});
		}

		// Token: 0x06002DCD RID: 11725 RVA: 0x0023D140 File Offset: 0x0023B340
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (timeOfDay == 1700 && Game1.currentLocation.Equals(this))
			{
				Game1.changeMusicTrack("night_market", false, MusicContext.Default);
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.shopClosedTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(89, 164, 18, 23),
					layerDepth = 0.001f,
					interval = 100f,
					position = new Vector2(19f, 31f) * 64f + new Vector2(6f, 10f) * 4f,
					scale = 4f,
					animationLength = 3
				});
			}
		}

		// Token: 0x06002DCE RID: 11726 RVA: 0x0023D214 File Offset: 0x0023B414
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.smokeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			if (this.smokeTimer <= 0f)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.shopClosedTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
					sourceRectStartingPos = new Vector2(0f, 180f),
					layerDepth = 1f,
					interval = 250f,
					position = new Vector2(35f, 38f) * 64f + new Vector2(9f, 6f) * 4f,
					scale = 4f,
					scaleChange = 0.005f,
					alpha = 0.75f,
					alphaFade = 0.005f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
					animationLength = 3,
					holdLastFrame = true
				});
				this.smokeTimer = 1250f;
			}
		}

		// Token: 0x04001F58 RID: 8024
		private Texture2D shopClosedTexture;

		// Token: 0x04001F59 RID: 8025
		private float smokeTimer;

		// Token: 0x04001F5A RID: 8026
		private string paintingMailKey;

		// Token: 0x04001F5B RID: 8027
		private bool hasReceivedFreeGift;

		// Token: 0x04001F5C RID: 8028
		private bool hasShownCCUpgrade;
	}
}

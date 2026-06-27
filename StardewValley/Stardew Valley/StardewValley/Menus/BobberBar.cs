using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	// Token: 0x02000250 RID: 592
	public class BobberBar : IClickableMenu
	{
		// Token: 0x06002756 RID: 10070 RVA: 0x001BF4C0 File Offset: 0x001BD6C0
		public BobberBar(string whichFish, float fishSize, bool treasure, List<string> bobbers, string setFlagOnCatch, bool isBossFish, string baitID = "", bool goldenTreasure = false) : base(0, 0, 96, 636, false)
		{
			this.fishObject = ItemRegistry.Create(whichFish, 1, 0, false);
			this.bobbers = bobbers;
			this.setFlagOnCatch = setFlagOnCatch;
			this.handledFishResult = false;
			this.treasure = treasure;
			this.goldenTreasure = goldenTreasure;
			this.treasureAppearTimer = (float)Game1.random.Next(1000, 3000);
			this.fadeIn = true;
			this.scale = 0f;
			this.whichFish = whichFish;
			Dictionary<string, string> dictionary = DataLoader.Fish(Game1.content);
			this.beginnersRod = (Game1.player.CurrentTool is FishingRod && Game1.player.CurrentTool.upgradeLevel.Value == 1);
			this.bobberBarHeight = 96 + Game1.player.FishingLevel * 8;
			if (Game1.player.FishingLevel < 5 && this.beginnersRod)
			{
				this.bobberBarHeight += 40 - Game1.player.FishingLevel * 8;
			}
			this.bossFish = isBossFish;
			string rawData;
			if (dictionary.TryGetValue(whichFish, out rawData))
			{
				string[] fields = rawData.Split('/', StringSplitOptions.None);
				this.difficulty = (float)Convert.ToInt32(fields[1]);
				string a = fields[2].ToLower();
				if (!(a == "mixed"))
				{
					if (!(a == "dart"))
					{
						if (!(a == "smooth"))
						{
							if (!(a == "floater"))
							{
								if (a == "sinker")
								{
									this.motionType = 3;
								}
							}
							else
							{
								this.motionType = 4;
							}
						}
						else
						{
							this.motionType = 2;
						}
					}
					else
					{
						this.motionType = 1;
					}
				}
				else
				{
					this.motionType = 0;
				}
				this.minFishSize = Convert.ToInt32(fields[3]);
				this.maxFishSize = Convert.ToInt32(fields[4]);
				this.fishSize = (int)((float)this.minFishSize + (float)(this.maxFishSize - this.minFishSize) * fishSize);
				this.fishSize++;
				this.perfect = true;
				this.fishQuality = (((double)fishSize < 0.33) ? 0 : (((double)fishSize < 0.66) ? 1 : 2));
				this.fishSizeReductionTimer = 800;
				for (int i = 0; i < Utility.getStringCountInList(bobbers, "(O)877"); i++)
				{
					this.fishQuality++;
					if (this.fishQuality > 2)
					{
						this.fishQuality = 4;
					}
				}
				if (this.beginnersRod)
				{
					this.fishQuality = 0;
					fishSize = (float)this.minFishSize;
				}
				if (Game1.player.stats.Get("blessingOfWaters") > 0U)
				{
					if (this.difficulty > 20f)
					{
						if (isBossFish)
						{
							this.difficulty *= 0.75f;
						}
						else
						{
							this.difficulty /= 2f;
						}
					}
					this.distanceFromCatchPenaltyModifier = 0.5f;
					Game1.player.stats.Decrement("blessingOfWaters", 1U);
					if (Game1.player.stats.Get("blessingOfWaters") == 0U)
					{
						Game1.player.buffs.Remove("statue_of_blessings_3");
					}
				}
			}
			NetStringIntArrayDictionary fishCaught = Game1.player.fishCaught;
			if (fishCaught != null && fishCaught.Length == 0)
			{
				this.distanceFromCatching = 0.1f;
				if (this.difficulty < 50f)
				{
					this.difficulty = 50f;
				}
			}
			this.Reposition();
			this.bobberBarHeight += Utility.getStringCountInList(bobbers, "(O)695") * 24;
			if (baitID == "(O)DeluxeBait")
			{
				this.bobberBarHeight += 12;
			}
			this.bobberBarPos = (float)(568 - this.bobberBarHeight);
			this.bobberPosition = 508f;
			this.bobberTargetPosition = (100f - this.difficulty) / 100f * 548f;
			if (baitID == "(O)ChallengeBait")
			{
				this.challengeBaitFishes = 3;
			}
			Game1.setRichPresence("fishing", Game1.currentLocation.Name);
		}

		// Token: 0x06002757 RID: 10071 RVA: 0x001BF8E4 File Offset: 0x001BDAE4
		public virtual void Reposition()
		{
			switch (Game1.player.FacingDirection)
			{
			case 0:
				this.xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
				this.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				break;
			case 1:
				this.xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
				this.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				break;
			case 2:
				this.xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
				this.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				break;
			case 3:
				this.xPositionOnScreen = (int)Game1.player.Position.X + 128;
				this.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				this.flipBubble = true;
				break;
			}
			this.xPositionOnScreen -= Game1.viewport.X;
			this.yPositionOnScreen -= Game1.viewport.Y + 64;
			if (this.xPositionOnScreen + 96 > Game1.viewport.Width)
			{
				this.xPositionOnScreen = Game1.viewport.Width - 96;
			}
			else if (this.xPositionOnScreen < 0)
			{
				this.xPositionOnScreen = 0;
			}
			if (this.yPositionOnScreen < 0)
			{
				this.yPositionOnScreen = 0;
				return;
			}
			if (this.yPositionOnScreen + 636 > Game1.viewport.Height)
			{
				this.yPositionOnScreen = Game1.viewport.Height - 636;
			}
		}

		// Token: 0x06002758 RID: 10072 RVA: 0x001BFABA File Offset: 0x001BDCBA
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.Reposition();
		}

		// Token: 0x06002759 RID: 10073 RVA: 0x001BFACA File Offset: 0x001BDCCA
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600275A RID: 10074 RVA: 0x001BFACC File Offset: 0x001BDCCC
		public override void performHoverAction(int x, int y)
		{
		}

		// Token: 0x0600275B RID: 10075 RVA: 0x001BFACE File Offset: 0x001BDCCE
		private static int SafeNext(Random random, int minValue, int maxValue)
		{
			if (minValue >= maxValue)
			{
				return maxValue;
			}
			return random.Next(minValue, maxValue);
		}

		// Token: 0x0600275C RID: 10076 RVA: 0x001BFAE0 File Offset: 0x001BDCE0
		public override void update(GameTime time)
		{
			this.Reposition();
			if (this.sparkleText != null && this.sparkleText.update(time))
			{
				this.sparkleText = null;
			}
			if (this.everythingShakeTimer > 0f)
			{
				this.everythingShakeTimer -= (float)time.ElapsedGameTime.Milliseconds;
				this.everythingShake = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)Game1.random.Next(-10, 11) / 10f);
				if (this.everythingShakeTimer <= 0f)
				{
					this.everythingShake = Vector2.Zero;
				}
			}
			if (this.fadeIn)
			{
				this.scale += 0.05f;
				if (this.scale >= 1f)
				{
					this.scale = 1f;
					this.fadeIn = false;
				}
			}
			else if (this.fadeOut)
			{
				if (this.everythingShakeTimer > 0f || this.sparkleText != null)
				{
					return;
				}
				this.scale -= 0.05f;
				if (this.scale <= 0f)
				{
					this.scale = 0f;
					this.fadeOut = false;
					FishingRod rod = Game1.player.CurrentTool as FishingRod;
					string text;
					if (rod == null)
					{
						text = null;
					}
					else
					{
						Object bait = rod.GetBait();
						text = ((bait != null) ? bait.QualifiedItemId : null);
					}
					string baitId = text;
					int numCaught = (!this.bossFish && baitId == "(O)774" && Game1.random.NextDouble() < 0.25 + Game1.player.DailyLuck / 2.0) ? 2 : 1;
					if (this.challengeBaitFishes > 0)
					{
						numCaught = this.challengeBaitFishes;
					}
					if (this.distanceFromCatching > 0.9f && rod != null)
					{
						rod.pullFishFromWater(this.whichFish, this.fishSize, this.fishQuality, (int)this.difficulty, this.treasureCaught, this.perfect, this.fromFishPond, this.setFlagOnCatch, this.bossFish, numCaught);
					}
					else
					{
						Game1.player.completelyStopAnimatingOrDoingAction();
						if (rod != null)
						{
							rod.doneFishing(Game1.player, true);
						}
					}
					Game1.exitActiveMenu();
					Game1.setRichPresence("location", Game1.currentLocation.Name);
				}
			}
			else
			{
				if (Game1.random.NextDouble() < (double)(this.difficulty * (float)((this.motionType == 2) ? 20 : 1) / 4000f) && (this.motionType != 2 || this.bobberTargetPosition == -1f))
				{
					float spaceBelow = 548f - this.bobberPosition;
					float spaceAbove = this.bobberPosition;
					float percent = Math.Min(99f, this.difficulty + (float)Game1.random.Next(10, 45)) / 100f;
					this.bobberTargetPosition = this.bobberPosition + (float)Game1.random.Next((int)Math.Min(-spaceAbove, spaceBelow), (int)spaceBelow) * percent;
				}
				int num = this.motionType;
				if (num != 3)
				{
					if (num == 4)
					{
						this.floaterSinkerAcceleration = Math.Max(this.floaterSinkerAcceleration - 0.01f, -1.5f);
					}
				}
				else
				{
					this.floaterSinkerAcceleration = Math.Min(this.floaterSinkerAcceleration + 0.01f, 1.5f);
				}
				if (Math.Abs(this.bobberPosition - this.bobberTargetPosition) > 3f && this.bobberTargetPosition != -1f)
				{
					this.bobberAcceleration = (this.bobberTargetPosition - this.bobberPosition) / ((float)Game1.random.Next(10, 30) + (100f - Math.Min(100f, this.difficulty)));
					this.bobberSpeed += (this.bobberAcceleration - this.bobberSpeed) / 5f;
				}
				else if (this.motionType != 2 && Game1.random.NextDouble() < (double)(this.difficulty / 2000f))
				{
					this.bobberTargetPosition = this.bobberPosition + (float)(Game1.random.NextBool() ? Game1.random.Next(-100, -51) : Game1.random.Next(50, 101));
				}
				else
				{
					this.bobberTargetPosition = -1f;
				}
				if (this.motionType == 1 && Game1.random.NextDouble() < (double)(this.difficulty / 1000f))
				{
					this.bobberTargetPosition = this.bobberPosition + (float)(Game1.random.NextBool() ? BobberBar.SafeNext(Game1.random, -100 - (int)this.difficulty * 2, -51) : BobberBar.SafeNext(Game1.random, 50, 101 + (int)this.difficulty * 2));
				}
				this.bobberTargetPosition = Math.Max(-1f, Math.Min(this.bobberTargetPosition, 548f));
				this.bobberPosition += this.bobberSpeed + this.floaterSinkerAcceleration;
				if (this.bobberPosition > 532f)
				{
					this.bobberPosition = 532f;
				}
				else if (this.bobberPosition < 0f)
				{
					this.bobberPosition = 0f;
				}
				this.bobberInBar = (this.bobberPosition + 12f <= this.bobberBarPos - 32f + (float)this.bobberBarHeight && this.bobberPosition - 16f >= this.bobberBarPos - 32f);
				if (this.bobberPosition >= (float)(548 - this.bobberBarHeight) && this.bobberBarPos >= (float)(568 - this.bobberBarHeight - 4))
				{
					this.bobberInBar = true;
				}
				bool flag = this.buttonPressed;
				this.buttonPressed = (Game1.oldMouseState.LeftButton == ButtonState.Pressed || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.X) || Game1.oldPadState.IsButtonDown(Buttons.A))));
				if (!flag && this.buttonPressed)
				{
					Game1.playSound("fishingRodBend", null);
				}
				float gravity = this.buttonPressed ? -0.25f : 0.25f;
				if (this.buttonPressed && gravity < 0f && (this.bobberBarPos == 0f || this.bobberBarPos == (float)(568 - this.bobberBarHeight)))
				{
					this.bobberBarSpeed = 0f;
				}
				if (this.bobberInBar)
				{
					gravity *= (this.bobbers.Contains("(O)691") ? 0.3f : 0.6f);
					if (this.bobbers.Contains("(O)691"))
					{
						for (int i = 0; i < Utility.getStringCountInList(this.bobbers, "(O)691"); i++)
						{
							if (this.bobberPosition + 16f < this.bobberBarPos + (float)(this.bobberBarHeight / 2))
							{
								this.bobberBarSpeed -= ((i > 0) ? 0.05f : 0.2f);
							}
							else
							{
								this.bobberBarSpeed += ((i > 0) ? 0.05f : 0.2f);
							}
							if (i > 0)
							{
								gravity *= 0.9f;
							}
						}
					}
				}
				float oldPos = this.bobberBarPos;
				this.bobberBarSpeed += gravity;
				this.bobberBarPos += this.bobberBarSpeed;
				if (this.bobberBarPos + (float)this.bobberBarHeight > 568f)
				{
					this.bobberBarPos = (float)(568 - this.bobberBarHeight);
					this.bobberBarSpeed = -this.bobberBarSpeed * 2f / 3f * (this.bobbers.Contains("(O)692") ? ((float)Utility.getStringCountInList(this.bobbers, "(O)692") * 0.1f) : 1f);
					if (oldPos + (float)this.bobberBarHeight < 568f)
					{
						Game1.playSound("shiny4", null);
					}
				}
				else if (this.bobberBarPos < 0f)
				{
					this.bobberBarPos = 0f;
					this.bobberBarSpeed = -this.bobberBarSpeed * 2f / 3f;
					if (oldPos > 0f)
					{
						Game1.playSound("shiny4", null);
					}
				}
				bool treasureInBar = false;
				if (this.treasure)
				{
					float oldTreasureAppearTimer = this.treasureAppearTimer;
					this.treasureAppearTimer -= (float)time.ElapsedGameTime.Milliseconds;
					if (this.treasureAppearTimer <= 0f)
					{
						if (this.treasureScale < 1f && !this.treasureCaught)
						{
							if (oldTreasureAppearTimer > 0f)
							{
								if (this.bobberBarPos > 274f)
								{
									this.treasurePosition = (float)Game1.random.Next(8, (int)this.bobberBarPos - 20);
								}
								else
								{
									int min = Math.Min(528, (int)this.bobberBarPos + this.bobberBarHeight);
									int max = 500;
									this.treasurePosition = (float)((min > max) ? (max - 1) : Game1.random.Next(min, max));
								}
								Game1.playSound("dwop", null);
							}
							this.treasureScale = Math.Min(1f, this.treasureScale + 0.1f);
						}
						treasureInBar = (this.treasurePosition + 12f <= this.bobberBarPos - 32f + (float)this.bobberBarHeight && this.treasurePosition - 16f >= this.bobberBarPos - 32f);
						if (treasureInBar && !this.treasureCaught)
						{
							this.treasureCatchLevel += 0.0135f;
							this.treasureShake = new Vector2((float)Game1.random.Next(-2, 3), (float)Game1.random.Next(-2, 3));
							if (this.treasureCatchLevel >= 1f)
							{
								Game1.playSound("newArtifact", null);
								this.treasureCaught = true;
							}
						}
						else if (this.treasureCaught)
						{
							this.treasureScale = Math.Max(0f, this.treasureScale - 0.1f);
						}
						else
						{
							this.treasureShake = Vector2.Zero;
							this.treasureCatchLevel = Math.Max(0f, this.treasureCatchLevel - 0.01f);
						}
					}
				}
				if (this.bobberInBar)
				{
					this.distanceFromCatching += 0.002f;
					this.reelRotation += 0.3926991f;
					this.fishShake.X = (float)Game1.random.Next(-10, 11) / 10f;
					this.fishShake.Y = (float)Game1.random.Next(-10, 11) / 10f;
					this.barShake = Vector2.Zero;
					Rumble.rumble(0.1f, 1000f);
					ICue cue = BobberBar.unReelSound;
					if (cue != null)
					{
						cue.Stop(AudioStopOptions.Immediate);
					}
					if (BobberBar.reelSound == null || BobberBar.reelSound.IsStopped || BobberBar.reelSound.IsStopping || !BobberBar.reelSound.IsPlaying)
					{
						Game1.playSound("fastReel", out BobberBar.reelSound);
					}
				}
				else if (!treasureInBar || this.treasureCaught || !this.bobbers.Contains("(O)693"))
				{
					if (!this.fishShake.Equals(Vector2.Zero))
					{
						Game1.playSound("tinyWhip", null);
						this.perfect = false;
						Rumble.stopRumbling();
						if (this.challengeBaitFishes > 0)
						{
							this.challengeBaitFishes--;
							if (this.challengeBaitFishes <= 0)
							{
								this.distanceFromCatching = 0f;
							}
						}
					}
					this.fishSizeReductionTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.fishSizeReductionTimer <= 0)
					{
						this.fishSize = Math.Max(this.minFishSize, this.fishSize - 1);
						this.fishSizeReductionTimer = 800;
					}
					if ((Game1.player.fishCaught != null && Game1.player.fishCaught.Length != 0) || Game1.currentMinigame != null)
					{
						if (this.bobbers.Contains("(O)694"))
						{
							float reduction = 0.003f;
							float amount = 0.001f;
							for (int j = 0; j < Utility.getStringCountInList(this.bobbers, "(O)694"); j++)
							{
								reduction -= amount;
								amount /= 2f;
							}
							reduction = Math.Max(0.001f, reduction);
							this.distanceFromCatching -= reduction * this.distanceFromCatchPenaltyModifier;
						}
						else
						{
							this.distanceFromCatching -= (this.beginnersRod ? 0.002f : 0.003f) * this.distanceFromCatchPenaltyModifier;
						}
					}
					float distanceAway = Math.Abs(this.bobberPosition - (this.bobberBarPos + (float)(this.bobberBarHeight / 2)));
					this.reelRotation -= 3.1415927f / Math.Max(10f, 200f - distanceAway);
					this.barShake.X = (float)Game1.random.Next(-10, 11) / 10f;
					this.barShake.Y = (float)Game1.random.Next(-10, 11) / 10f;
					this.fishShake = Vector2.Zero;
					ICue cue2 = BobberBar.reelSound;
					if (cue2 != null)
					{
						cue2.Stop(AudioStopOptions.Immediate);
					}
					if (BobberBar.unReelSound == null || BobberBar.unReelSound.IsStopped)
					{
						Game1.playSound("slowReel", 600, out BobberBar.unReelSound);
					}
				}
				this.distanceFromCatching = Math.Max(0f, Math.Min(1f, this.distanceFromCatching));
				if (Game1.player.CurrentTool != null)
				{
					Game1.player.CurrentTool.tickUpdate(time, Game1.player);
				}
				if (this.distanceFromCatching <= 0f)
				{
					this.fadeOut = true;
					this.everythingShakeTimer = 500f;
					Game1.playSound("fishEscape", null);
					this.handledFishResult = true;
					ICue cue3 = BobberBar.unReelSound;
					if (cue3 != null)
					{
						cue3.Stop(AudioStopOptions.Immediate);
					}
					ICue cue4 = BobberBar.reelSound;
					if (cue4 != null)
					{
						cue4.Stop(AudioStopOptions.Immediate);
					}
				}
				else if (this.distanceFromCatching >= 1f)
				{
					this.everythingShakeTimer = 500f;
					Game1.playSound("jingle1", null);
					this.fadeOut = true;
					this.handledFishResult = true;
					ICue cue5 = BobberBar.unReelSound;
					if (cue5 != null)
					{
						cue5.Stop(AudioStopOptions.Immediate);
					}
					ICue cue6 = BobberBar.reelSound;
					if (cue6 != null)
					{
						cue6.Stop(AudioStopOptions.Immediate);
					}
					if (this.perfect)
					{
						this.sparkleText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, false, 0.1, 1500, -1, 500, 1f);
						if (Game1.isFestival())
						{
							Game1.CurrentEvent.perfectFishing();
						}
					}
					else if (this.fishSize == this.maxFishSize)
					{
						this.fishSize--;
					}
				}
			}
			if (this.bobberPosition < 0f)
			{
				this.bobberPosition = 0f;
			}
			if (this.bobberPosition > 548f)
			{
				this.bobberPosition = 548f;
			}
		}

		// Token: 0x0600275D RID: 10077 RVA: 0x001C09F5 File Offset: 0x001BEBF5
		public override bool readyToClose()
		{
			return false;
		}

		// Token: 0x0600275E RID: 10078 RVA: 0x001C09F8 File Offset: 0x001BEBF8
		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			ICue cue = BobberBar.unReelSound;
			if (cue != null)
			{
				cue.Stop(AudioStopOptions.Immediate);
			}
			ICue cue2 = BobberBar.reelSound;
			if (cue2 != null)
			{
				cue2.Stop(AudioStopOptions.Immediate);
			}
			if (!this.handledFishResult)
			{
				Game1.playSound("fishEscape", null);
			}
			this.fadeOut = true;
			this.everythingShakeTimer = 500f;
			this.distanceFromCatching = -1f;
		}

		// Token: 0x0600275F RID: 10079 RVA: 0x001C0A66 File Offset: 0x001BEC66
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.menuButton.Contains(new InputButton(key)))
			{
				this.emergencyShutDown();
			}
		}

		// Token: 0x06002760 RID: 10080 RVA: 0x001C0A88 File Offset: 0x001BEC88
		public override void draw(SpriteBatch b)
		{
			Game1.StartWorldDrawInUI(b);
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen - (this.flipBubble ? 44 : 20) + 104), (float)(this.yPositionOnScreen - 16 + 314)) + this.everythingShake, new Rectangle?(new Rectangle(652, 1685, 52, 157)), Color.White * 0.6f * this.scale, 0f, new Vector2(26f, 78.5f) * this.scale, 4f * this.scale, this.flipBubble ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.001f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 70), (float)(this.yPositionOnScreen + 296)) + this.everythingShake, new Rectangle?(new Rectangle(644, 1999, 38, 150)), Color.White * this.scale, 0f, new Vector2(18.5f, 74f) * this.scale, 4f * this.scale, SpriteEffects.None, 0.01f);
			if (this.scale == 1f)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 64), (float)(this.yPositionOnScreen + 12 + (int)this.bobberBarPos)) + this.barShake + this.everythingShake, new Rectangle?(new Rectangle(682, 2078, 9, 2)), this.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 64), (float)(this.yPositionOnScreen + 12 + (int)this.bobberBarPos + 8)) + this.barShake + this.everythingShake, new Rectangle?(new Rectangle(682, 2081, 9, 1)), this.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, new Vector2(4f, (float)(this.bobberBarHeight - 16)), SpriteEffects.None, 0.89f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 64), (float)(this.yPositionOnScreen + 12 + (int)this.bobberBarPos + this.bobberBarHeight - 8)) + this.barShake + this.everythingShake, new Rectangle?(new Rectangle(682, 2085, 9, 2)), this.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 124, this.yPositionOnScreen + 4 + (int)(580f * (1f - this.distanceFromCatching)), 16, (int)(580f * this.distanceFromCatching)), Utility.getRedToGreenLerpColor(this.distanceFromCatching));
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 18), (float)(this.yPositionOnScreen + 514)) + this.everythingShake, new Rectangle?(new Rectangle(257, 1990, 5, 10)), Color.White, this.reelRotation, new Vector2(2f, 10f), 4f, SpriteEffects.None, 0.9f);
				if (this.goldenTreasure)
				{
					b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(this.xPositionOnScreen + 64 + 18), (float)(this.yPositionOnScreen + 12 + 24) + this.treasurePosition) + this.treasureShake + this.everythingShake, new Rectangle?(new Rectangle(256, 51, 20, 24)), Color.White, 0f, new Vector2(10f, 10f), 2f * this.treasureScale, SpriteEffects.None, 0.85f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 64 + 18), (float)(this.yPositionOnScreen + 12 + 24) + this.treasurePosition) + this.treasureShake + this.everythingShake, new Rectangle?(new Rectangle(638, 1865, 20, 24)), Color.White, 0f, new Vector2(10f, 10f), 2f * this.treasureScale, SpriteEffects.None, 0.85f);
				}
				if (this.treasureCatchLevel > 0f && !this.treasureCaught)
				{
					b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + 12 + (int)this.treasurePosition, 40, 8), Color.DimGray * 0.5f);
					b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + 12 + (int)this.treasurePosition, (int)(this.treasureCatchLevel * 40f), 8), Color.Orange);
				}
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 64 + 18), (float)(this.yPositionOnScreen + 12 + 24) + this.bobberPosition) + this.fishShake + this.everythingShake, new Rectangle?(new Rectangle(614 + (this.bossFish ? 20 : 0), 1840, 20, 20)), Color.White, 0f, new Vector2(10f, 10f), 2f, SpriteEffects.None, 0.88f);
				SparklingText sparklingText = this.sparkleText;
				if (sparklingText != null)
				{
					sparklingText.draw(b, new Vector2((float)(this.xPositionOnScreen - 16), (float)(this.yPositionOnScreen - 64)));
				}
				if (this.bobbers.Contains("(O)SonarBobber"))
				{
					int xPosition = ((float)this.xPositionOnScreen > (float)Game1.viewport.Width * 0.75f) ? (this.xPositionOnScreen - 80) : (this.xPositionOnScreen + 216);
					bool flip = xPosition < this.xPositionOnScreen;
					b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(xPosition - 12), (float)(this.yPositionOnScreen + 40)) + this.everythingShake, new Rectangle?(new Rectangle(227, 6, 29, 24)), Color.White, 0f, new Vector2(10f, 10f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.88f);
					this.fishObject.drawInMenu(b, new Vector2((float)xPosition, (float)this.yPositionOnScreen) + new Vector2((float)(flip ? -8 : -4), 4f) * 4f + this.everythingShake, 1f);
				}
				if (this.challengeBaitFishes > -1)
				{
					int xPosition2 = ((float)this.xPositionOnScreen > (float)Game1.viewport.Width * 0.75f) ? (this.xPositionOnScreen - 80) : (this.xPositionOnScreen + 216);
					int yPos = this.bobbers.Contains("(O)SonarBobber") ? (this.yPositionOnScreen + 136) : (this.yPositionOnScreen + 40);
					Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2((float)(xPosition2 - 24) + this.everythingShake.X, (float)(yPos - 16) + this.everythingShake.Y), new Rectangle(240, 31, 15, 38), Color.White, 0f, Vector2.Zero, 4f, false, -1f, -1, -1, 0.35f);
					for (int y = 0; y < 3; y++)
					{
						if (y < this.challengeBaitFishes)
						{
							Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2((float)(xPosition2 - 12), (float)yPos + (float)(y * 20) * 2f) + this.everythingShake, new Rectangle(236, 205, 19, 19), Color.White, 0f, new Vector2(0f, 0f), 2f, false, 0.88f, -1, -1, 0.35f);
						}
						else
						{
							b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(xPosition2 - 12), (float)yPos + (float)(y * 20) * 2f) + this.everythingShake, new Rectangle?(new Rectangle(217, 205, 19, 19)), Color.White, 0f, new Vector2(0f, 0f), 2f, SpriteEffects.None, 0.88f);
						}
					}
				}
			}
			NetStringIntArrayDictionary fishCaught = Game1.player.fishCaught;
			if (fishCaught != null && fishCaught.Length == 0)
			{
				Vector2 pos = new Vector2((float)(this.xPositionOnScreen + (this.flipBubble ? (this.width + 64 + 8) : -200)), (float)(this.yPositionOnScreen + 192));
				if (!Game1.options.gamepadControls)
				{
					b.Draw(Game1.mouseCursors, pos, new Rectangle?(new Rectangle(644, 1330, 48, 69)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				else
				{
					b.Draw(Game1.controllerMaps, pos, new Rectangle?(Utility.controllerMapSourceRect(new Rectangle(681, 0, 96, 138))), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
			}
			Game1.EndWorldDrawInUI(b);
		}

		// Token: 0x0400186B RID: 6251
		public const int timePerFishSizeReduction = 800;

		// Token: 0x0400186C RID: 6252
		public const int bobberTrackHeight = 548;

		// Token: 0x0400186D RID: 6253
		public const int bobberBarTrackHeight = 568;

		// Token: 0x0400186E RID: 6254
		public const int xOffsetToBobberTrack = 64;

		// Token: 0x0400186F RID: 6255
		public const int yOffsetToBobberTrack = 12;

		// Token: 0x04001870 RID: 6256
		public const int mixed = 0;

		// Token: 0x04001871 RID: 6257
		public const int dart = 1;

		// Token: 0x04001872 RID: 6258
		public const int smooth = 2;

		// Token: 0x04001873 RID: 6259
		public const int sink = 3;

		// Token: 0x04001874 RID: 6260
		public const int floater = 4;

		// Token: 0x04001875 RID: 6261
		public const int CHALLENGE_BAIT_MAX_FISHES = 3;

		// Token: 0x04001876 RID: 6262
		public bool handledFishResult;

		// Token: 0x04001877 RID: 6263
		public float difficulty;

		// Token: 0x04001878 RID: 6264
		public int motionType;

		// Token: 0x04001879 RID: 6265
		public string whichFish;

		// Token: 0x0400187A RID: 6266
		public float distanceFromCatchPenaltyModifier = 1f;

		// Token: 0x0400187B RID: 6267
		public string setFlagOnCatch;

		// Token: 0x0400187C RID: 6268
		public float bobberPosition = 548f;

		// Token: 0x0400187D RID: 6269
		public float bobberSpeed;

		// Token: 0x0400187E RID: 6270
		public float bobberAcceleration;

		// Token: 0x0400187F RID: 6271
		public float bobberTargetPosition;

		// Token: 0x04001880 RID: 6272
		public float scale;

		// Token: 0x04001881 RID: 6273
		public float everythingShakeTimer;

		// Token: 0x04001882 RID: 6274
		public float floaterSinkerAcceleration;

		// Token: 0x04001883 RID: 6275
		public float treasurePosition;

		// Token: 0x04001884 RID: 6276
		public float treasureCatchLevel;

		// Token: 0x04001885 RID: 6277
		public float treasureAppearTimer;

		// Token: 0x04001886 RID: 6278
		public float treasureScale;

		// Token: 0x04001887 RID: 6279
		public bool bobberInBar;

		// Token: 0x04001888 RID: 6280
		public bool buttonPressed;

		// Token: 0x04001889 RID: 6281
		public bool flipBubble;

		// Token: 0x0400188A RID: 6282
		public bool fadeIn;

		// Token: 0x0400188B RID: 6283
		public bool fadeOut;

		// Token: 0x0400188C RID: 6284
		public bool treasure;

		// Token: 0x0400188D RID: 6285
		public bool treasureCaught;

		// Token: 0x0400188E RID: 6286
		public bool perfect;

		// Token: 0x0400188F RID: 6287
		public bool bossFish;

		// Token: 0x04001890 RID: 6288
		public bool beginnersRod;

		// Token: 0x04001891 RID: 6289
		public bool fromFishPond;

		// Token: 0x04001892 RID: 6290
		public bool goldenTreasure;

		// Token: 0x04001893 RID: 6291
		public int bobberBarHeight;

		// Token: 0x04001894 RID: 6292
		public int fishSize;

		// Token: 0x04001895 RID: 6293
		public int fishQuality;

		// Token: 0x04001896 RID: 6294
		public int minFishSize;

		// Token: 0x04001897 RID: 6295
		public int maxFishSize;

		// Token: 0x04001898 RID: 6296
		public int fishSizeReductionTimer;

		// Token: 0x04001899 RID: 6297
		public int challengeBaitFishes = -1;

		// Token: 0x0400189A RID: 6298
		public List<string> bobbers;

		// Token: 0x0400189B RID: 6299
		public Vector2 barShake;

		// Token: 0x0400189C RID: 6300
		public Vector2 fishShake;

		// Token: 0x0400189D RID: 6301
		public Vector2 everythingShake;

		// Token: 0x0400189E RID: 6302
		public Vector2 treasureShake;

		// Token: 0x0400189F RID: 6303
		public float reelRotation;

		// Token: 0x040018A0 RID: 6304
		private SparklingText sparkleText;

		// Token: 0x040018A1 RID: 6305
		public float bobberBarPos;

		// Token: 0x040018A2 RID: 6306
		public float bobberBarSpeed;

		// Token: 0x040018A3 RID: 6307
		public float distanceFromCatching = 0.3f;

		// Token: 0x040018A4 RID: 6308
		public static ICue reelSound;

		// Token: 0x040018A5 RID: 6309
		public static ICue unReelSound;

		// Token: 0x040018A6 RID: 6310
		private Item fishObject;
	}
}

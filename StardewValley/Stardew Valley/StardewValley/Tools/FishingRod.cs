using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;

namespace StardewValley.Tools
{
	// Token: 0x02000129 RID: 297
	public class FishingRod : Tool
	{
		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x060017FC RID: 6140 RVA: 0x00113575 File Offset: 0x00111775
		// (set) Token: 0x060017FD RID: 6141 RVA: 0x0011358C File Offset: 0x0011178C
		public int CastDirection
		{
			get
			{
				if (this.fishCaught)
				{
					return 2;
				}
				return this.castDirection.Value;
			}
			set
			{
				this.castDirection.Value = value;
			}
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x0011359C File Offset: 0x0011179C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.bobber.NetFields, "bobber.NetFields").AddField(this.castDirection, "castDirection").AddField(this.pullFishFromWaterEvent, "pullFishFromWaterEvent").AddField(this.doneFishingEvent, "doneFishingEvent").AddField(this.startCastingEvent, "startCastingEvent").AddField(this.castingEndEnableMovementEvent, "castingEndEnableMovementEvent").AddField(this.putAwayEvent, "putAwayEvent").AddField(this._totalMotion, "_totalMotion").AddField(this.beginReelingEvent, "beginReelingEvent");
			this.pullFishFromWaterEvent.AddReaderHandler(new Action<BinaryReader>(this.doPullFishFromWater));
			this.doneFishingEvent.onEvent += this.doDoneFishing;
			this.startCastingEvent.onEvent += this.doStartCasting;
			this.castingEndEnableMovementEvent.onEvent += this.doCastingEndEnableMovement;
			this.beginReelingEvent.onEvent += this.beginReeling;
			this.putAwayEvent.onEvent += this.resetState;
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x001136D8 File Offset: 0x001118D8
		protected override void MigrateLegacyItemId()
		{
			switch (base.UpgradeLevel)
			{
			case 0:
				base.ItemId = "BambooPole";
				return;
			case 1:
				base.ItemId = "TrainingRod";
				return;
			case 2:
				base.ItemId = "FiberglassRod";
				return;
			case 3:
				base.ItemId = "IridiumRod";
				return;
			case 4:
				base.ItemId = "AdvancedIridiumRod";
				return;
			default:
				base.ItemId = "BambooPole";
				return;
			}
		}

		// Token: 0x06001800 RID: 6144 RVA: 0x0011374F File Offset: 0x0011194F
		public override void actionWhenStopBeingHeld(Farmer who)
		{
			this.putAwayEvent.Fire();
			base.actionWhenStopBeingHeld(who);
		}

		// Token: 0x06001801 RID: 6145 RVA: 0x00113764 File Offset: 0x00111964
		public FishingRod() : base("Fishing Rod", 0, 189, 8, false, 2)
		{
		}

		// Token: 0x06001802 RID: 6146 RVA: 0x00113864 File Offset: 0x00111A64
		public override void resetState()
		{
			this.isNibbling = false;
			this.fishCaught = false;
			this.isFishing = false;
			this.isReeling = false;
			this.isCasting = false;
			this.isTimingCast = false;
			this.doneWithAnimation = false;
			this.pullingOutOfWater = false;
			this.fromFishPond = false;
			this.numberOfFishCaught = 1;
			this.fishingBiteAccumulator = 0f;
			this.showingTreasure = false;
			this.fishingNibbleAccumulator = 0f;
			this.timeUntilFishingBite = -1f;
			this.timeUntilFishingNibbleDone = -1f;
			this.bobberTimeAccumulator = 0f;
			this.castingChosenCountdown = 0f;
			this.lastWaterColor = null;
			this.gotTroutDerbyTag = false;
			this._totalMotionBufferIndex = 0;
			for (int i = 0; i < this._totalMotionBuffer.Length; i++)
			{
				this._totalMotionBuffer[i] = Vector2.Zero;
			}
			if (this.lastUser != null && this.lastUser == Game1.player)
			{
				Game1.screenOverlayTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.id == 987654321);
			}
			this._totalMotion.Value = Vector2.Zero;
			this._lastAppliedMotion = Vector2.Zero;
			this.pullFishFromWaterEvent.Clear();
			this.doneFishingEvent.Clear();
			this.startCastingEvent.Clear();
			this.castingEndEnableMovementEvent.Clear();
			this.beginReelingEvent.Clear();
			this.bobber.Set(Vector2.Zero);
			this.CastDirection = -1;
		}

		// Token: 0x06001803 RID: 6147 RVA: 0x001139E8 File Offset: 0x00111BE8
		public FishingRod(int upgradeLevel) : base("Fishing Rod", upgradeLevel, 189, 8, false, (upgradeLevel == 4) ? 3 : 2)
		{
			base.IndexOfMenuItemView = 8 + upgradeLevel;
		}

		// Token: 0x06001804 RID: 6148 RVA: 0x00113AF8 File Offset: 0x00111CF8
		public FishingRod(int upgradeLevel, int numAttachmentSlots) : base("Fishing Rod", upgradeLevel, 189, 8, false, numAttachmentSlots)
		{
			base.IndexOfMenuItemView = 8 + upgradeLevel;
		}

		// Token: 0x06001805 RID: 6149 RVA: 0x00113C00 File Offset: 0x00111E00
		protected override Item GetOneNew()
		{
			return new FishingRod();
		}

		// Token: 0x06001806 RID: 6150 RVA: 0x00113C07 File Offset: 0x00111E07
		private int getAddedDistance(Farmer who)
		{
			if (who.FishingLevel >= 15)
			{
				return 4;
			}
			if (who.FishingLevel >= 8)
			{
				return 3;
			}
			if (who.FishingLevel >= 4)
			{
				return 2;
			}
			if (who.FishingLevel >= 1)
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x06001807 RID: 6151 RVA: 0x00113C37 File Offset: 0x00111E37
		private Vector2 calculateBobberTile()
		{
			return new Vector2(this.bobber.X / 64f, this.bobber.Y / 64f);
		}

		// Token: 0x06001808 RID: 6152 RVA: 0x00113C60 File Offset: 0x00111E60
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			FishingRod.<>c__DisplayClass81_0 CS$<>8__locals1 = new FishingRod.<>c__DisplayClass81_0();
			CS$<>8__locals1.<>4__this = this;
			who = (who ?? this.lastUser);
			if (this.fishCaught)
			{
				return;
			}
			if (!who.IsLocalPlayer && (this.isReeling || this.isFishing || this.pullingOutOfWater))
			{
				return;
			}
			this.hasDoneFucntionYet = true;
			Vector2 bobberTile = this.calculateBobberTile();
			int tileX = (int)bobberTile.X;
			int tileY = (int)bobberTile.Y;
			base.DoFunction(location, x, y, power, who);
			if (this.doneWithAnimation)
			{
				who.canReleaseTool = true;
			}
			if (Game1.isAnyGamePadButtonBeingPressed())
			{
				Game1.lastCursorMotionWasMouse = false;
			}
			if (!this.isFishing && !this.castedButBobberStillInAir && !this.pullingOutOfWater && !this.isNibbling && !this.hit && !this.showingTreasure)
			{
				if (!Game1.eventUp && who.IsLocalPlayer && !base.hasEnchantmentOfType<EfficientToolEnchantment>())
				{
					float oldStamina = who.Stamina;
					who.Stamina -= 8f - (float)who.FishingLevel * 0.1f;
					who.checkForExhaustion(oldStamina);
				}
				if (location.canFishHere() && location.isTileFishable(tileX, tileY))
				{
					this.clearWaterDistance = FishingRod.distanceToLand((int)(this.bobber.X / 64f), (int)(this.bobber.Y / 64f), who.currentLocation, false);
					this.isFishing = true;
					location.temporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, new Vector2(this.bobber.X - 32f, this.bobber.Y - 32f), false, false));
					if (who.IsLocalPlayer)
					{
						if (this.PlayUseSounds)
						{
							location.playSound("dropItemInWater", new Vector2?(bobberTile), null, SoundContext.Default);
						}
						Stats stats = Game1.stats;
						uint timesFished = stats.TimesFished;
						stats.TimesFished = timesFished + 1U;
					}
					this.timeUntilFishingBite = this.calculateTimeUntilFishingBite(bobberTile, true, who);
					if (location.fishSplashPoint != null)
					{
						bool frenzy = location.fishFrenzyFish.Value != null && !location.fishFrenzyFish.Equals("");
						Rectangle fishSplashRect = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
						if (frenzy)
						{
							fishSplashRect.Inflate(32, 32);
						}
						Rectangle bobberRect = new Rectangle((int)this.bobber.X - 32, (int)this.bobber.Y - 32, 64, 64);
						if (bobberRect.Intersects(fishSplashRect))
						{
							this.timeUntilFishingBite /= (float)(frenzy ? 2 : 4);
							location.temporarySprites.Add(new TemporaryAnimatedSprite(10, this.bobber.Value - new Vector2(32f, 32f), Color.Cyan, 8, false, 100f, 0, -1, -1f, -1, 0));
						}
					}
					who.UsingTool = true;
					who.canMove = false;
					return;
				}
				if (this.doneWithAnimation)
				{
					who.UsingTool = false;
				}
				if (this.doneWithAnimation)
				{
					who.canMove = true;
					return;
				}
			}
			else if (!this.isCasting && !this.pullingOutOfWater)
			{
				bool fromFishPond = location.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y);
				who.FarmerSprite.PauseForSingleAnimation = false;
				int num = who.FacingDirection;
				switch (num)
				{
				case 0:
					who.FarmerSprite.animateBackwardsOnce(299, 35f);
					break;
				case 1:
					who.FarmerSprite.animateBackwardsOnce(300, 35f);
					break;
				case 2:
					who.FarmerSprite.animateBackwardsOnce(301, 35f);
					break;
				case 3:
					who.FarmerSprite.animateBackwardsOnce(302, 35f);
					break;
				}
				if (this.isNibbling)
				{
					Object bait = this.GetBait();
					double baitPotency = (double)((bait != null) ? ((float)bait.Price / 10f) : 0f);
					bool splashPoint = false;
					if (location.fishSplashPoint != null)
					{
						Rectangle fishSplashRect2 = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
						Rectangle bobberRect2 = new Rectangle((int)this.bobber.X - 80, (int)this.bobber.Y - 80, 64, 64);
						splashPoint = fishSplashRect2.Intersects(bobberRect2);
					}
					CS$<>8__locals1.o = location.getFish(this.fishingNibbleAccumulator, (bait != null) ? bait.QualifiedItemId : null, this.clearWaterDistance + ((splashPoint > false) ? 1 : 0), who, baitPotency + (splashPoint ? 0.4 : 0.0), bobberTile, null);
					if (CS$<>8__locals1.o == null || ItemRegistry.GetDataOrErrorItem(CS$<>8__locals1.o.QualifiedItemId).IsErrorItem)
					{
						FishingRod.<>c__DisplayClass81_0 CS$<>8__locals2 = CS$<>8__locals1;
						string str = "(O)";
						num = Game1.random.Next(167, 173);
						CS$<>8__locals2.o = ItemRegistry.Create(str + num.ToString(), 1, 0, false);
					}
					Object @object = CS$<>8__locals1.o as Object;
					if (@object != null && @object.scale.X == (float)1)
					{
						this.favBait = true;
					}
					Dictionary<string, string> data = DataLoader.Fish(Game1.content);
					bool non_fishable_fish = false;
					string rawData;
					if (!CS$<>8__locals1.o.HasTypeObject())
					{
						non_fishable_fish = true;
					}
					else if (data.TryGetValue(CS$<>8__locals1.o.ItemId, out rawData))
					{
						if (!int.TryParse(rawData.Split('/', StringSplitOptions.None)[1], out num))
						{
							non_fishable_fish = true;
						}
					}
					else
					{
						non_fishable_fish = true;
					}
					this.lastCatchWasJunk = false;
					string qualifiedItemId = CS$<>8__locals1.o.QualifiedItemId;
					bool isJunk;
					if (qualifiedItemId != null)
					{
						num = qualifiedItemId.Length;
						if (num != 5)
						{
							if (num != 6)
							{
								goto IL_760;
							}
							switch (qualifiedItemId[5])
							{
							case '0':
								if (!(qualifiedItemId == "(O)890") && !(qualifiedItemId == "(O)820"))
								{
									goto IL_760;
								}
								break;
							case '1':
								if (!(qualifiedItemId == "(O)821"))
								{
									goto IL_760;
								}
								break;
							case '2':
								if (!(qualifiedItemId == "(O)152") && !(qualifiedItemId == "(O)842") && !(qualifiedItemId == "(O)822"))
								{
									goto IL_760;
								}
								break;
							case '3':
								if (!(qualifiedItemId == "(O)153") && !(qualifiedItemId == "(O)823"))
								{
									goto IL_760;
								}
								break;
							case '4':
								if (!(qualifiedItemId == "(O)824"))
								{
									goto IL_760;
								}
								break;
							case '5':
								if (!(qualifiedItemId == "(O)825"))
								{
									goto IL_760;
								}
								break;
							case '6':
								if (!(qualifiedItemId == "(O)826"))
								{
									goto IL_760;
								}
								break;
							case '7':
								if (!(qualifiedItemId == "(O)157") && !(qualifiedItemId == "(O)797") && !(qualifiedItemId == "(O)827"))
								{
									goto IL_760;
								}
								break;
							case '8':
								if (!(qualifiedItemId == "(O)828"))
								{
									goto IL_760;
								}
								break;
							default:
								goto IL_760;
							}
						}
						else
						{
							char c = qualifiedItemId[4];
							if (c != '3')
							{
								if (c != '9')
								{
									goto IL_760;
								}
								if (!(qualifiedItemId == "(O)79"))
								{
									goto IL_760;
								}
							}
							else if (!(qualifiedItemId == "(O)73"))
							{
								goto IL_760;
							}
						}
						isJunk = true;
						goto IL_789;
					}
					IL_760:
					isJunk = (CS$<>8__locals1.o.Category == -20 || CS$<>8__locals1.o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID);
					IL_789:
					if (isJunk || fromFishPond || non_fishable_fish)
					{
						this.lastCatchWasJunk = true;
						this.pullFishFromWater(CS$<>8__locals1.o.QualifiedItemId, -1, 0, 0, false, false, fromFishPond, CS$<>8__locals1.o.SetFlagOnPickup, false, 1);
						return;
					}
					if (!this.hit && who.IsLocalPlayer)
					{
						this.hit = true;
						Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(612, 1913, 74, 30), 1500f, 1, 0, Game1.GlobalToLocal(Game1.viewport, this.bobber.Value + new Vector2(-140f, -160f)), false, false, 1f, 0.005f, Color.White, 4f, 0.075f, 0f, 0f, true)
						{
							scaleChangeChange = -0.005f,
							motion = new Vector2(0f, -0.1f),
							endFunction = delegate(int _)
							{
								CS$<>8__locals1.<>4__this.startMinigameEndFunction(CS$<>8__locals1.o);
							},
							id = 987654321
						});
						if (this.PlayUseSounds)
						{
							who.playNearbySoundLocal("FishHit", null, SoundContext.Default);
						}
					}
					return;
				}
				else
				{
					if (fromFishPond && Game1.timeOfDay < 2600)
					{
						Item fishPondPull = location.getFish(-1f, null, -1, who, -1.0, bobberTile, null);
						if (fishPondPull != null)
						{
							this.pullFishFromWater(fishPondPull.QualifiedItemId, -1, 0, 0, false, false, true, null, false, 1);
							return;
						}
					}
					if (this.PlayUseSounds && who.IsLocalPlayer)
					{
						location.playSound("pullItemFromWater", new Vector2?(bobberTile), null, SoundContext.Default);
					}
					this.isFishing = false;
					this.pullingOutOfWater = true;
					Point playerPixel = who.StandingPixel;
					if (who.FacingDirection == 1 || who.FacingDirection == 3)
					{
						double num2 = (double)Math.Abs(this.bobber.X - (float)playerPixel.X);
						float gravity = 0.005f;
						float velocity = -(float)Math.Sqrt(num2 * (double)gravity / (double)2f);
						float t = 2f * (Math.Abs(velocity - 0.5f) / gravity);
						t *= 1.2f;
						Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, this.getBobberStyle(who), 16, 32);
						sourceRect.Height = 16;
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect, t, 1, 0, this.bobber.Value + new Vector2(-32f, -48f), false, false, (float)playerPixel.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f, false)
						{
							motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * (velocity + 0.2f), velocity - 0.8f),
							acceleration = new Vector2(0f, gravity),
							endFunction = new TemporaryAnimatedSprite.endBehavior(this.donefishingEndFunction),
							timeBasedMotion = true,
							alphaFade = 0.001f,
							flipped = (who.FacingDirection == 1 && this.flipCurrentBobberWhenFacingRight())
						});
					}
					else
					{
						float distance = this.bobber.Y - (float)playerPixel.Y;
						float height = Math.Abs(distance + 256f);
						float gravity2 = 0.005f;
						float velocity2 = (float)Math.Sqrt((double)(2f * gravity2 * height));
						float t2 = (float)(Math.Sqrt((double)(2f * (height - distance) / gravity2)) + (double)(velocity2 / gravity2));
						Rectangle sourceRect2 = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, this.getBobberStyle(who), 16, 32);
						sourceRect2.Height = 16;
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect2, t2, 1, 0, this.bobber.Value + new Vector2(-32f, -48f), false, false, this.bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f, false)
						{
							motion = new Vector2(((float)who.StandingPixel.X - this.bobber.Value.X) / 800f, -velocity2),
							acceleration = new Vector2(0f, gravity2),
							endFunction = new TemporaryAnimatedSprite.endBehavior(this.donefishingEndFunction),
							timeBasedMotion = true,
							alphaFade = 0.001f
						});
					}
					who.UsingTool = true;
					who.canReleaseTool = false;
				}
			}
		}

		// Token: 0x06001809 RID: 6153 RVA: 0x001148A8 File Offset: 0x00112AA8
		public int getBobberStyle(Farmer who)
		{
			if (this.GetTackleQualifiedItemIDs().Contains("(O)789"))
			{
				return 39;
			}
			if (who != null)
			{
				if (this.randomBobberStyle == -1 && who.usingRandomizedBobber && this.randomBobberStyle == -1)
				{
					who.bobberStyle.Value = Math.Min(FishingRod.NUM_BOBBER_STYLES - 1, Game1.random.Next(Game1.player.fishCaught.Count() / 2));
					this.randomBobberStyle = who.bobberStyle.Value;
				}
				return who.bobberStyle.Value;
			}
			return 0;
		}

		// Token: 0x0600180A RID: 6154 RVA: 0x00114938 File Offset: 0x00112B38
		public bool flipCurrentBobberWhenFacingRight()
		{
			int bobberStyle = this.getBobberStyle(base.getLastFarmerToUse());
			if (bobberStyle != 9)
			{
				switch (bobberStyle)
				{
				case 19:
				case 21:
				case 23:
					return true;
				case 20:
				case 22:
					break;
				default:
					if (bobberStyle == 36)
					{
						return true;
					}
					break;
				}
				return false;
			}
			return true;
		}

		// Token: 0x0600180B RID: 6155 RVA: 0x0011497C File Offset: 0x00112B7C
		public Color getFishingLineColor()
		{
			switch (this.getBobberStyle(base.getLastFarmerToUse()))
			{
			case 6:
			case 20:
				return new Color(255, 200, 255);
			case 7:
				return Color.Yellow;
			case 9:
				return new Color(255, 255, 200);
			case 10:
				return new Color(255, 208, 169);
			case 11:
				return new Color(170, 170, 255);
			case 12:
				return Color.DimGray;
			case 13:
				return new Color(228, 228, 172);
			case 14:
			case 22:
				return new Color(178, 255, 112);
			case 15:
				return new Color(250, 193, 70);
			case 16:
				return new Color(255, 170, 170);
			case 17:
				return new Color(200, 220, 255);
			case 25:
			case 27:
				return Color.White * 0.5f;
			case 29:
			case 32:
				return Color.Lime * 0.66f;
			case 31:
				return Color.Red * 0.5f;
			case 35:
			case 39:
				return new Color(180, 160, 255);
			case 37:
			case 38:
				return new Color(200, 255, 255);
			}
			return Color.White;
		}

		// Token: 0x0600180C RID: 6156 RVA: 0x00114B50 File Offset: 0x00112D50
		private float calculateTimeUntilFishingBite(Vector2 bobberTile, bool isFirstCast, Farmer who)
		{
			if (Game1.currentLocation.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y))
			{
				FishPond pond = Game1.currentLocation.getBuildingAt(bobberTile) as FishPond;
				if (pond != null && pond.currentOccupants.Value > 0)
				{
					return FishPond.FISHING_MILLISECONDS;
				}
			}
			List<string> tackleIds = this.GetTackleQualifiedItemIDs();
			Object bait = this.GetBait();
			string baitId = (bait != null) ? bait.QualifiedItemId : null;
			int reductionTime = 0;
			reductionTime += Utility.getStringCountInList(tackleIds, "(O)687") * 10000;
			reductionTime += Utility.getStringCountInList(tackleIds, "(O)686") * 5000;
			float time = (float)Game1.random.Next(FishingRod.minFishingBiteTime, Math.Max(FishingRod.minFishingBiteTime, FishingRod.maxFishingBiteTime - 250 * who.FishingLevel - reductionTime));
			if (isFirstCast)
			{
				time *= 0.75f;
			}
			if (baitId != null)
			{
				time *= 0.5f;
				if (!(baitId == "(O)774") && !(baitId == "(O)ChallengeBait"))
				{
					if (baitId == "(O)DeluxeBait")
					{
						time *= 0.66f;
					}
				}
				else
				{
					time *= 0.75f;
				}
			}
			return Math.Max(500f, time);
		}

		// Token: 0x0600180D RID: 6157 RVA: 0x00114C74 File Offset: 0x00112E74
		public Color getColor()
		{
			switch (this.upgradeLevel.Value)
			{
			case 0:
				return Color.Goldenrod;
			case 1:
				return Color.OliveDrab;
			case 2:
				return Color.White;
			case 3:
				return Color.Violet;
			case 4:
				return new Color(128, 143, 255);
			default:
				return Color.White;
			}
		}

		// Token: 0x0600180E RID: 6158 RVA: 0x00114CDC File Offset: 0x00112EDC
		public static int distanceToLand(int tileX, int tileY, GameLocation location, bool landMustBeAdjacentToWalkableTile = false)
		{
			Rectangle r = new Rectangle(tileX - 1, tileY - 1, 3, 3);
			bool foundLand = false;
			int distance = 1;
			while (!foundLand && r.Width <= 11)
			{
				foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
				{
					if (location.isTileOnMap(v) && !location.isWaterTile((int)v.X, (int)v.Y))
					{
						foundLand = true;
						distance = r.Width / 2;
						if (landMustBeAdjacentToWalkableTile)
						{
							foundLand = false;
							foreach (Vector2 surroundings in Utility.getSurroundingTileLocationsArray(v))
							{
								if (location.isTilePassable(surroundings) && !location.isWaterTile((int)v.X, (int)v.Y))
								{
									foundLand = true;
									break;
								}
							}
							break;
						}
						break;
					}
				}
				r.Inflate(1, 1);
			}
			if (r.Width > 11)
			{
				distance = 6;
			}
			distance--;
			return distance;
		}

		// Token: 0x0600180F RID: 6159 RVA: 0x00114DF4 File Offset: 0x00112FF4
		public void startMinigameEndFunction(Item fish)
		{
			fish.TryGetTempData<bool>("IsBossFish", out this.bossFish);
			Farmer who = this.lastUser;
			this.beginReelingEvent.Fire();
			this.isReeling = true;
			this.hit = false;
			int facingDirection = who.FacingDirection;
			if (facingDirection != 1)
			{
				if (facingDirection == 3)
				{
					who.FarmerSprite.setCurrentSingleFrame(48, 32000, false, true);
				}
			}
			else
			{
				who.FarmerSprite.setCurrentSingleFrame(48, 32000, false, false);
			}
			float fishSize = 1f;
			fishSize *= (float)this.clearWaterDistance / 5f;
			int minimumSizeContribution = 1 + who.FishingLevel / 2;
			fishSize *= (float)Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;
			if (this.favBait)
			{
				fishSize *= 1.2f;
			}
			fishSize *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
			fishSize = Math.Max(0f, Math.Min(1f, fishSize));
			Object bait = this.GetBait();
			string baitId = (bait != null) ? bait.QualifiedItemId : null;
			List<string> tackleIds = this.GetTackleQualifiedItemIDs();
			double extraTreasureChance = (double)Utility.getStringCountInList(tackleIds, "(O)693") * FishingRod.baseChanceForTreasure / 3.0;
			this.goldenTreasure = false;
			bool flag;
			if (!Game1.isFestival())
			{
				NetStringIntArrayDictionary netStringIntArrayDictionary = who.fishCaught;
				if (netStringIntArrayDictionary != null && netStringIntArrayDictionary.Length > 1)
				{
					flag = (Game1.random.NextDouble() < FishingRod.baseChanceForTreasure + (double)who.LuckLevel * 0.005 + ((baitId == "(O)703") ? FishingRod.baseChanceForTreasure : 0.0) + extraTreasureChance + who.DailyLuck / 2.0 + (who.professions.Contains(9) ? FishingRod.baseChanceForTreasure : 0.0));
					goto IL_1CC;
				}
			}
			flag = false;
			IL_1CC:
			bool treasure = flag;
			if (treasure && Game1.player.stats.Get(StatKeys.Mastery(1)) > 0U && Game1.random.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(null))
			{
				this.goldenTreasure = true;
			}
			Game1.activeClickableMenu = new BobberBar(fish.ItemId, fishSize, treasure, tackleIds, fish.SetFlagOnPickup, this.bossFish, baitId, this.goldenTreasure);
		}

		// Token: 0x06001810 RID: 6160 RVA: 0x00115040 File Offset: 0x00113240
		public List<Object> GetTackle()
		{
			List<Object> tack = new List<Object>();
			if (this.CanUseTackle())
			{
				for (int i = 1; i < this.attachments.Count; i++)
				{
					tack.Add(this.attachments[i]);
				}
			}
			return tack;
		}

		// Token: 0x06001811 RID: 6161 RVA: 0x00115084 File Offset: 0x00113284
		public List<string> GetTackleQualifiedItemIDs()
		{
			List<string> ids = new List<string>();
			foreach (Object o in this.GetTackle())
			{
				if (o != null)
				{
					ids.Add(o.QualifiedItemId);
				}
			}
			return ids;
		}

		// Token: 0x06001812 RID: 6162 RVA: 0x001150E8 File Offset: 0x001132E8
		public Object GetBait()
		{
			if (!this.CanUseBait())
			{
				return null;
			}
			return this.attachments[0];
		}

		// Token: 0x06001813 RID: 6163 RVA: 0x00115100 File Offset: 0x00113300
		public bool HasMagicBait()
		{
			Object bait = this.GetBait();
			return ((bait != null) ? bait.QualifiedItemId : null) == "(O)908";
		}

		// Token: 0x06001814 RID: 6164 RVA: 0x0011511E File Offset: 0x0011331E
		public bool HasCuriosityLure()
		{
			return this.GetTackleQualifiedItemIDs().Contains("(O)856");
		}

		// Token: 0x06001815 RID: 6165 RVA: 0x00115130 File Offset: 0x00113330
		public bool inUse()
		{
			return this.isFishing || this.isCasting || this.isTimingCast || this.isNibbling || this.isReeling || this.fishCaught;
		}

		// Token: 0x06001816 RID: 6166 RVA: 0x00115164 File Offset: 0x00113364
		public void donefishingEndFunction(int extra)
		{
			Farmer who = this.lastUser;
			this.isFishing = false;
			this.isReeling = false;
			who.canReleaseTool = true;
			who.canMove = true;
			who.UsingTool = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			this.pullingOutOfWater = false;
			this.doneFishing(who, false);
		}

		// Token: 0x06001817 RID: 6167 RVA: 0x001151B6 File Offset: 0x001133B6
		public static void endOfAnimationBehavior(Farmer f)
		{
		}

		// Token: 0x06001818 RID: 6168 RVA: 0x001151B8 File Offset: 0x001133B8
		public override void drawAttachments(SpriteBatch b, int x, int y)
		{
			y += ((this.enchantments.Count > 0) ? 8 : 4);
			if (this.CanUseBait())
			{
				this.DrawAttachmentSlot(0, b, x, y);
			}
			y += 68;
			if (this.CanUseTackle())
			{
				for (int i = 1; i < base.AttachmentSlotsCount; i++)
				{
					this.DrawAttachmentSlot(i, b, x, y);
					x += 68;
				}
			}
		}

		// Token: 0x06001819 RID: 6169 RVA: 0x0011521C File Offset: 0x0011341C
		protected override void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
		{
			base.GetAttachmentSlotSprite(slot, out texture, out sourceRect);
			if (slot == 0)
			{
				if (this.GetBait() == null)
				{
					sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 36, -1, -1);
					return;
				}
			}
			else if (this.attachments[slot] == null)
			{
				sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 37, -1, -1);
			}
		}

		// Token: 0x0600181A RID: 6170 RVA: 0x00115274 File Offset: 0x00113474
		protected override bool canThisBeAttached(Object o, int slot)
		{
			if (o.QualifiedItemId == "(O)789" && slot != 0)
			{
				return true;
			}
			if (slot != 0)
			{
				return o.Category == -22 && this.CanUseTackle();
			}
			return o.Category == -21 && this.CanUseBait();
		}

		// Token: 0x0600181B RID: 6171 RVA: 0x001152C0 File Offset: 0x001134C0
		public bool CanUseBait()
		{
			return base.AttachmentSlotsCount > 0;
		}

		// Token: 0x0600181C RID: 6172 RVA: 0x001152CB File Offset: 0x001134CB
		public bool CanUseTackle()
		{
			return base.AttachmentSlotsCount > 1;
		}

		// Token: 0x0600181D RID: 6173 RVA: 0x001152D8 File Offset: 0x001134D8
		public void playerCaughtFishEndFunction(bool isBossFish)
		{
			Farmer who = this.lastUser;
			who.Halt();
			who.armOffset = Vector2.Zero;
			this.castedButBobberStillInAir = false;
			this.fishCaught = true;
			this.isReeling = false;
			this.isFishing = false;
			this.pullingOutOfWater = false;
			who.canReleaseTool = false;
			if (who.IsLocalPlayer)
			{
				bool firstCatch = this.whichFish.QualifiedItemId.StartsWith("(O)") && !who.fishCaught.ContainsKey(this.whichFish.QualifiedItemId) && !this.whichFish.QualifiedItemId.Equals("(O)388") && !this.whichFish.QualifiedItemId.Equals("(O)390");
				if (!Game1.isFestival())
				{
					this.recordSize = who.caughtFish(this.whichFish.QualifiedItemId, this.fishSize, this.fromFishPond, this.numberOfFishCaught);
					who.faceDirection(2);
				}
				else
				{
					Game1.currentLocation.currentEvent.caughtFish(this.whichFish.QualifiedItemId, this.fishSize, who);
					this.fishCaught = false;
					this.doneFishing(who, false);
				}
				if (isBossFish)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
					Game1.multiplayer.globalChatInfoMessage("CaughtLegendaryFish", new string[]
					{
						who.Name,
						TokenStringBuilder.ItemName(this.whichFish.QualifiedItemId, null)
					});
				}
				else if (this.recordSize)
				{
					this.sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure, false, 0.1, 2500, -1, 500, 1f);
					if (!firstCatch)
					{
						who.playNearbySoundLocal("newRecord", null, SoundContext.Default);
					}
				}
				else
				{
					who.playNearbySoundLocal("fishSlap", null, SoundContext.Default);
				}
				if (firstCatch && who.fishCaught.ContainsKey(this.whichFish.QualifiedItemId))
				{
					this.sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\1_6_Strings:FirstCatch"), new Color(200, 255, 220), Color.White, false, 0.1, 2500, -1, 500, 1f);
					who.playNearbySoundLocal("discoverMineral", null, SoundContext.Default);
				}
			}
		}

		// Token: 0x0600181E RID: 6174 RVA: 0x00115544 File Offset: 0x00113744
		public void pullFishFromWater(string fishId, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, string setFlagOnCatch, bool isBossFish, int numCaught)
		{
			this.pullFishFromWaterEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(fishId);
				writer.Write(fishSize);
				writer.Write(fishQuality);
				writer.Write(fishDifficulty);
				writer.Write(treasureCaught);
				writer.Write(wasPerfect);
				writer.Write(fromFishPond);
				writer.Write(setFlagOnCatch ?? string.Empty);
				writer.Write(isBossFish);
				writer.Write(numCaught);
			});
		}

		// Token: 0x0600181F RID: 6175 RVA: 0x001155BC File Offset: 0x001137BC
		private void doPullFishFromWater(BinaryReader argReader)
		{
			Farmer who = this.lastUser;
			string fishId = argReader.ReadString();
			int fishSize = argReader.ReadInt32();
			int fishQuality = argReader.ReadInt32();
			int fishDifficulty = argReader.ReadInt32();
			bool treasureCaught = argReader.ReadBoolean();
			bool wasPerfect = argReader.ReadBoolean();
			bool fromFishPond = argReader.ReadBoolean();
			string setFlagOnCatch = argReader.ReadString();
			bool isBossFish = argReader.ReadBoolean();
			int numCaught = argReader.ReadInt32();
			this.treasureCaught = treasureCaught;
			this.fishSize = fishSize;
			this.fishQuality = fishQuality;
			this.whichFish = ItemRegistry.GetMetadata(fishId);
			this.fromFishPond = fromFishPond;
			this.setFlagOnCatch = ((setFlagOnCatch != string.Empty) ? setFlagOnCatch : null);
			this.numberOfFishCaught = numCaught;
			Vector2 bobberTile = this.calculateBobberTile();
			bool fishIsObject = this.whichFish.TypeIdentifier == "(O)";
			if (fishQuality >= 2 && wasPerfect)
			{
				this.fishQuality = 4;
			}
			else if (fishQuality >= 1 && wasPerfect)
			{
				this.fishQuality = 2;
			}
			if (who == null)
			{
				return;
			}
			if (!Game1.isFestival() && who.IsLocalPlayer && !fromFishPond && fishIsObject)
			{
				int experience = Math.Max(1, (fishQuality + 1) * 3 + fishDifficulty / 3);
				if (treasureCaught)
				{
					experience += (int)((float)experience * 1.2f);
				}
				if (wasPerfect)
				{
					experience += (int)((float)experience * 1.4f);
				}
				if (isBossFish)
				{
					experience *= 5;
				}
				who.gainExperience(1, experience);
			}
			if (this.fishQuality < 0)
			{
				this.fishQuality = 0;
			}
			string sprite_sheet_name;
			Rectangle sprite_rect;
			if (fishIsObject)
			{
				ParsedItemData parsedOrErrorData = this.whichFish.GetParsedOrErrorData();
				sprite_sheet_name = parsedOrErrorData.TextureName;
				sprite_rect = parsedOrErrorData.GetSourceRect(0, null);
			}
			else
			{
				sprite_sheet_name = "LooseSprites\\Cursors";
				sprite_rect = new Rectangle(228, 408, 16, 16);
			}
			float t;
			if (who.FacingDirection == 1 || who.FacingDirection == 3)
			{
				float distance = Vector2.Distance(this.bobber.Value, who.Position);
				float gravity = 0.001f;
				float height = 128f - (who.Position.Y - this.bobber.Y + 10f);
				double angle = 1.1423973285781066;
				float yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
				if (float.IsNaN(yVelocity))
				{
					yVelocity = 0.6f;
				}
				float xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
				t = distance / xVelocity;
				this.animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, this.bobber.Value, false, false, this.bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * -xVelocity, -yVelocity),
					acceleration = new Vector2(0f, gravity),
					timeBasedMotion = true,
					endFunction = delegate(int _)
					{
						this.playerCaughtFishEndFunction(isBossFish);
					},
					endSound = "tinyWhip"
				});
				if (this.numberOfFishCaught > 1)
				{
					for (int i = 1; i < this.numberOfFishCaught; i++)
					{
						distance = Vector2.Distance(this.bobber.Value, who.Position);
						gravity = 0.0008f - (float)i * 0.0001f;
						height = 128f - (who.Position.Y - this.bobber.Y + 10f);
						angle = 1.1423973285781066;
						yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
						if (float.IsNaN(yVelocity))
						{
							yVelocity = 0.6f;
						}
						xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
						t = distance / xVelocity;
						this.animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, this.bobber.Value, false, false, this.bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * -xVelocity, -yVelocity),
							acceleration = new Vector2(0f, gravity),
							timeBasedMotion = true,
							endSound = "fishSlap",
							Parent = who.currentLocation,
							delayBeforeAnimationStart = (i - 1) * 100
						});
					}
				}
			}
			else
			{
				int playerStandingY = who.StandingPixel.Y;
				float distance2 = this.bobber.Y - (float)(playerStandingY - 64);
				float height2 = Math.Abs(distance2 + 256f + 32f);
				if (who.FacingDirection == 0)
				{
					height2 += 96f;
				}
				float gravity2 = 0.003f;
				float velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
				t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
				float xVelocity2 = 0f;
				if (t != 0f)
				{
					xVelocity2 = (who.Position.X - this.bobber.X) / t;
				}
				this.animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, this.bobber.Value, false, false, this.bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(xVelocity2, -velocity),
					acceleration = new Vector2(0f, gravity2),
					timeBasedMotion = true,
					endFunction = delegate(int _)
					{
						this.playerCaughtFishEndFunction(isBossFish);
					},
					endSound = "tinyWhip"
				});
				if (this.numberOfFishCaught > 1)
				{
					for (int j = 1; j < this.numberOfFishCaught; j++)
					{
						distance2 = this.bobber.Y - (float)(playerStandingY - 64);
						height2 = Math.Abs(distance2 + 256f + 32f);
						if (who.FacingDirection == 0)
						{
							height2 += 96f;
						}
						gravity2 = 0.004f - (float)j * 0.0005f;
						velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
						t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
						xVelocity2 = 0f;
						if (t != 0f)
						{
							xVelocity2 = (who.Position.X - this.bobber.X) / t;
						}
						this.animations.Add(new TemporaryAnimatedSprite(sprite_sheet_name, sprite_rect, t, 1, 0, new Vector2(this.bobber.X, this.bobber.Y), false, false, this.bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(xVelocity2, -velocity),
							acceleration = new Vector2(0f, gravity2),
							timeBasedMotion = true,
							endSound = "fishSlap",
							Parent = who.currentLocation,
							delayBeforeAnimationStart = (j - 1) * 100
						});
					}
				}
			}
			if (this.PlayUseSounds && who.IsLocalPlayer)
			{
				who.currentLocation.playSound("pullItemFromWater", new Vector2?(bobberTile), null, SoundContext.Default);
				who.currentLocation.playSound("dwop", new Vector2?(bobberTile), null, SoundContext.Default);
			}
			this.castedButBobberStillInAir = false;
			this.pullingOutOfWater = true;
			this.isFishing = false;
			this.isReeling = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.animateBackwardsOnce(299, t);
				return;
			case 1:
				who.FarmerSprite.animateBackwardsOnce(300, t);
				return;
			case 2:
				who.FarmerSprite.animateBackwardsOnce(301, t);
				return;
			case 3:
				who.FarmerSprite.animateBackwardsOnce(302, t);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001820 RID: 6176 RVA: 0x00115E54 File Offset: 0x00114054
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Farmer who = this.lastUser;
			float scale = 4f;
			if (!this.bobber.Equals(Vector2.Zero) && this.isFishing)
			{
				Vector2 bobberPos = this.bobber.Value;
				if (this.bobberTimeAccumulator > this.timePerBobberBob)
				{
					if ((!this.isNibbling && !this.isReeling) || Game1.random.NextDouble() < 0.05)
					{
						if (this.PlayUseSounds)
						{
							who.playNearbySoundLocal("waterSlosh", null, SoundContext.Default);
						}
						who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(this.bobber.X - 32f, this.bobber.Y - 16f), false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false));
					}
					this.timePerBobberBob = (float)((this.bobberBob == 0) ? Game1.random.Next(1500, 3500) : Game1.random.Next(350, 750));
					this.bobberTimeAccumulator = 0f;
					if (this.isNibbling || this.isReeling)
					{
						this.timePerBobberBob = (float)Game1.random.Next(25, 75);
						bobberPos.X += (float)Game1.random.Next(-5, 5);
						bobberPos.Y += (float)Game1.random.Next(-5, 5);
						if (!this.isReeling)
						{
							scale += (float)Game1.random.Next(-20, 20) / 100f;
						}
					}
					else if (this.PlayUseSounds && Game1.random.NextDouble() < 0.1)
					{
						who.playNearbySoundLocal("bob", null, SoundContext.Default);
					}
				}
				float bobberLayerDepth = bobberPos.Y / 10000f;
				Rectangle position = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, this.getBobberStyle(base.getLastFarmerToUse()), 16, 32);
				position.Height = 16;
				position.Y += 16;
				b.Draw(Game1.bobbersTexture, Game1.GlobalToLocal(Game1.viewport, bobberPos), new Rectangle?(position), Color.White, 0f, new Vector2(8f, 8f), scale, (base.getLastFarmerToUse().FacingDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, bobberLayerDepth);
				position = new Rectangle(position.X, position.Y + 8, position.Width, position.Height - 8);
			}
			else if ((this.isTimingCast || this.castingChosenCountdown > 0f) && who.IsLocalPlayer)
			{
				int yOffset = (int)(-Math.Abs(this.castingChosenCountdown / 2f - this.castingChosenCountdown) / 50f);
				float alpha = (this.castingChosenCountdown > 0f && this.castingChosenCountdown < 100f) ? (this.castingChosenCountdown / 100f) : 1f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, base.getLastFarmerToUse().Position + new Vector2(-48f, (float)(-160 + yOffset))), new Rectangle?(new Rectangle(193, 1868, 47, 12)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.885f);
				b.Draw(Game1.staminaRect, new Rectangle((int)Game1.GlobalToLocal(Game1.viewport, base.getLastFarmerToUse().Position).X - 32 - 4, (int)Game1.GlobalToLocal(Game1.viewport, base.getLastFarmerToUse().Position).Y + yOffset - 128 - 32 + 12, (int)(164f * this.castingPower), 25), new Rectangle?(Game1.staminaRect.Bounds), Utility.getRedToGreenLerpColor(this.castingPower) * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0.887f);
			}
			for (int i = this.animations.Count - 1; i >= 0; i--)
			{
				this.animations[i].draw(b, false, 0, 0, 1f);
			}
			if (this.sparklingText != null && !this.fishCaught)
			{
				this.sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, base.getLastFarmerToUse().Position + new Vector2(-24f, -192f)));
			}
			else if (this.sparklingText != null && this.fishCaught)
			{
				this.sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, base.getLastFarmerToUse().Position + new Vector2(-64f, -352f)));
			}
			if (this.bobber.Value.Equals(Vector2.Zero) || (!this.isFishing && !this.pullingOutOfWater && !this.castedButBobberStillInAir) || who.FarmerSprite.CurrentFrame == 57 || (who.FacingDirection == 0 && this.pullingOutOfWater && this.whichFish != null))
			{
				if (this.fishCaught)
				{
					bool fishIsObject = this.whichFish.TypeIdentifier == "(O)";
					float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					int playerStandingY = who.StandingPixel.Y;
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-120f, -288f + yOffset2)), new Rectangle?(new Rectangle(31, 1870, 73, 49)), Color.White * 0.8f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.06f);
					if (fishIsObject)
					{
						ParsedItemData parsedOrErrorData = this.whichFish.GetParsedOrErrorData();
						Texture2D texture = parsedOrErrorData.GetTexture();
						Rectangle sourceRect = parsedOrErrorData.GetSourceRect(0, null);
						b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-124f, -284f + yOffset2) + new Vector2(44f, 68f)), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.0001f + 0.06f);
						if (this.numberOfFishCaught > 1)
						{
							Utility.drawTinyDigits(this.numberOfFishCaught, b, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-120f, -284f + yOffset2) + new Vector2(23f, 29f) * 4f), 3f, (float)playerStandingY / 10000f + 0.0001f + 0.061f, Color.White);
						}
						b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -56f)), new Rectangle?(sourceRect), Color.White, (this.fishSize == -1 || this.whichFish.QualifiedItemId == "(O)800" || this.whichFish.QualifiedItemId == "(O)798" || this.whichFish.QualifiedItemId == "(O)149" || this.whichFish.QualifiedItemId == "(O)151") ? 0f : 2.3561945f, new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
						if (this.numberOfFishCaught > 1)
						{
							for (int j = 1; j < this.numberOfFishCaught; j++)
							{
								b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2((float)(0 - 12 * j), -56f)), new Rectangle?(sourceRect), Color.White, (this.fishSize == -1 || this.whichFish.QualifiedItemId == "(O)800" || this.whichFish.QualifiedItemId == "(O)798" || this.whichFish.QualifiedItemId == "(O)149" || this.whichFish.QualifiedItemId == "(O)151") ? 0f : ((j == 2) ? 3.1415927f : 2.5132742f), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.058f);
							}
						}
					}
					else
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-124f, -284f + yOffset2) + new Vector2(44f, 68f)), new Rectangle?(new Rectangle(228, 408, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.0001f + 0.06f);
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -56f)), new Rectangle?(new Rectangle(228, 408, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
					}
					string name = fishIsObject ? this.whichFish.GetParsedOrErrorData().DisplayName : "???";
					b.DrawString(Game1.smallFont, name, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(26f - Game1.smallFont.MeasureString(name).X / 2f, -278f + yOffset2)), this.bossFish ? new Color(126, 61, 237) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
					if (this.fishSize != -1)
					{
						b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14082"), Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(20f, -214f + yOffset2)), Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
						b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en) ? Math.Round((double)this.fishSize * 2.54) : ((double)this.fishSize)), Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(85f - Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en) ? Math.Round((double)this.fishSize * 2.54) : ((double)this.fishSize))).X / 2f, -179f + yOffset2)), this.recordSize ? (Color.Blue * Math.Min(1f, yOffset2 / 8f + 1.5f)) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)playerStandingY / 10000f + 0.002f + 0.06f);
					}
				}
				return;
			}
			Vector2 bobberPos2 = this.isFishing ? this.bobber.Value : ((this.animations.Count > 0) ? (this.animations[0].position + new Vector2(0f, 4f * scale)) : Vector2.Zero);
			if (this.whichFish != null)
			{
				bobberPos2 += new Vector2(32f, 32f);
			}
			Vector2 lastPosition = Vector2.Zero;
			if (this.castedButBobberStillInAir)
			{
				switch (who.FacingDirection)
				{
				case 0:
					switch (who.FarmerSprite.currentAnimationIndex)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(22f, who.armOffset.Y - 96f + 4f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(32f, who.armOffset.Y - 96f + 4f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 64f + 40f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 16f));
						break;
					case 4:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 32f));
						break;
					case 5:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 32f));
						break;
					default:
						lastPosition = Vector2.Zero;
						break;
					}
					break;
				case 1:
					switch (who.FarmerSprite.currentAnimationIndex)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-48f, who.armOffset.Y - 96f - 8f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-16f, who.armOffset.Y - 96f - 20f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(84f, who.armOffset.Y - 96f - 20f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(112f, who.armOffset.Y - 32f - 20f));
						break;
					case 4:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(120f, who.armOffset.Y - 32f + 8f));
						break;
					case 5:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(120f, who.armOffset.Y - 32f + 8f));
						break;
					default:
						lastPosition = Vector2.Zero;
						break;
					}
					break;
				case 2:
					switch (who.FarmerSprite.currentAnimationIndex)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(8f, who.armOffset.Y - 96f + 4f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(22f, who.armOffset.Y - 96f + 4f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 64f + 40f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 8f));
						break;
					case 4:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y + 32f));
						break;
					case 5:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y + 32f));
						break;
					default:
						lastPosition = Vector2.Zero;
						break;
					}
					break;
				case 3:
					switch (who.FarmerSprite.currentAnimationIndex)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(112f, who.armOffset.Y - 96f - 8f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(80f, who.armOffset.Y - 96f - 20f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-20f, who.armOffset.Y - 96f - 20f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-48f, who.armOffset.Y - 32f - 20f));
						break;
					case 4:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, who.armOffset.Y - 32f + 8f));
						break;
					case 5:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, who.armOffset.Y - 32f + 8f));
						break;
					}
					break;
				default:
					lastPosition = Vector2.Zero;
					break;
				}
			}
			else if (this.isReeling)
			{
				if (who != null && who.IsLocalPlayer && Game1.didPlayerJustClickAtAll(false))
				{
					switch (who.FacingDirection)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(24f, who.armOffset.Y - 96f + 12f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(20f, who.armOffset.Y - 96f - 12f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(12f, who.armOffset.Y - 96f + 8f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(48f, who.armOffset.Y - 96f - 12f));
						break;
					}
				}
				else
				{
					switch (who.FacingDirection)
					{
					case 0:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(25f, who.armOffset.Y - 96f + 4f));
						break;
					case 1:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 96f - 8f));
						break;
					case 2:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(12f, who.armOffset.Y - 96f + 4f));
						break;
					case 3:
						lastPosition = Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(36f, who.armOffset.Y - 96f - 8f));
						break;
					}
				}
			}
			else
			{
				switch (who.FacingDirection)
				{
				case 0:
					lastPosition = (this.pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(22f, who.armOffset.Y - 96f + 4f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y - 64f - 12f)));
					break;
				case 1:
					lastPosition = (this.pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-48f, who.armOffset.Y - 96f - 8f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(120f, who.armOffset.Y - 64f + 16f)));
					break;
				case 2:
					lastPosition = (this.pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(8f, who.armOffset.Y - 96f + 4f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(28f, who.armOffset.Y + 64f - 12f)));
					break;
				case 3:
					lastPosition = (this.pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(112f, who.armOffset.Y - 96f - 8f)) : Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-56f, who.armOffset.Y - 64f + 16f)));
					break;
				default:
					lastPosition = Vector2.Zero;
					break;
				}
			}
			Vector2 localBobber = Game1.GlobalToLocal(Game1.viewport, bobberPos2 + new Vector2(0f, -2.5f * scale + (float)((this.bobberBob == 1) ? 4 : 0)));
			if (this.isTimingCast || (this.isCasting && !who.IsLocalPlayer))
			{
				return;
			}
			if (this.isReeling)
			{
				Utility.drawLineWithScreenCoordinates((int)lastPosition.X, (int)lastPosition.Y, (int)localBobber.X, (int)localBobber.Y, b, this.getFishingLineColor() * 0.5f, 1f, 1);
				return;
			}
			if (!this.isFishing)
			{
				localBobber += new Vector2(20f, 20f);
			}
			if (this.pullingOutOfWater && this.whichFish != null)
			{
				localBobber += new Vector2(-20f, -30f);
			}
			Vector2 v = lastPosition;
			Vector2 v2 = new Vector2(lastPosition.X + (localBobber.X - lastPosition.X) / 3f, lastPosition.Y + (localBobber.Y - lastPosition.Y) * 2f / 3f);
			Vector2 v3 = new Vector2(lastPosition.X + (localBobber.X - lastPosition.X) * 2f / 3f, lastPosition.Y + (localBobber.Y - lastPosition.Y) * (float)(this.isFishing ? 6 : 2) / 5f);
			Vector2 v4 = localBobber;
			float drawLayer = ((bobberPos2.Y > (float)who.StandingPixel.Y) ? (bobberPos2.Y / 10000f) : ((float)who.StandingPixel.Y / 10000f)) + ((who.FacingDirection != 0) ? 0.005f : -0.001f);
			for (float k = 0f; k < 1f; k += 0.025f)
			{
				Vector2 current = Utility.GetCurvePoint(k, v, v2, v3, v4);
				Utility.drawLineWithScreenCoordinates((int)lastPosition.X, (int)lastPosition.Y, (int)current.X, (int)current.Y, b, this.getFishingLineColor() * 0.5f, drawLayer, 1);
				lastPosition = current;
			}
		}

		// Token: 0x06001821 RID: 6177 RVA: 0x001178AC File Offset: 0x00115AAC
		public Color GetWaterColor()
		{
			if (this.lastWaterColor != null)
			{
				return this.lastWaterColor.Value;
			}
			Farmer lastUser = this.lastUser;
			GameLocation location = ((lastUser != null) ? lastUser.currentLocation : null) ?? Game1.currentLocation;
			Vector2 tile = this.calculateBobberTile();
			if (tile != Vector2.Zero)
			{
				foreach (Building building in location.buildings)
				{
					if (building.isTileFishable(tile))
					{
						this.lastWaterColor = building.GetWaterColor(tile);
						if (this.lastWaterColor != null)
						{
							return this.lastWaterColor.Value;
						}
						break;
					}
				}
			}
			this.lastWaterColor = new Color?(location.waterColor.Value);
			return this.lastWaterColor.Value;
		}

		// Token: 0x06001822 RID: 6178 RVA: 0x00117998 File Offset: 0x00115B98
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			if (who.Stamina <= 1f && who.IsLocalPlayer)
			{
				if (!who.isEmoting)
				{
					who.doEmote(36);
				}
				who.CanMove = !Game1.eventUp;
				who.UsingTool = false;
				who.canReleaseTool = false;
				this.doneFishing(null, false);
				return true;
			}
			this.usedGamePadToCast = false;
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X))
			{
				this.usedGamePadToCast = true;
			}
			this.bossFish = false;
			this.originalFacingDirection = who.FacingDirection;
			if (who.IsLocalPlayer || who.isFakeEventActor)
			{
				this.CastDirection = this.originalFacingDirection;
			}
			who.Halt();
			this.treasureCaught = false;
			this.showingTreasure = false;
			this.isFishing = false;
			this.hit = false;
			this.favBait = false;
			if (this.GetTackle().Count > 0)
			{
				bool foundTackle = false;
				using (List<Object>.Enumerator enumerator = this.GetTackle().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current != null)
						{
							foundTackle = true;
							break;
						}
					}
				}
				this.hadBobber = foundTackle;
			}
			this.isNibbling = false;
			this.lastUser = who;
			this.lastWaterColor = null;
			this.isTimingCast = true;
			this._totalMotionBufferIndex = 0;
			for (int i = 0; i < this._totalMotionBuffer.Length; i++)
			{
				this._totalMotionBuffer[i] = Vector2.Zero;
			}
			this._totalMotion.Value = Vector2.Zero;
			this._lastAppliedMotion = Vector2.Zero;
			who.UsingTool = true;
			this.whichFish = null;
			this.recastTimerMs = 0;
			who.canMove = false;
			this.fishCaught = false;
			this.doneWithAnimation = false;
			who.canReleaseTool = false;
			this.hasDoneFucntionYet = false;
			this.isReeling = false;
			this.pullingOutOfWater = false;
			this.castingPower = 0f;
			this.castingChosenCountdown = 0f;
			this.animations.Clear();
			this.sparklingText = null;
			this.setTimingCastAnimation(who);
			return true;
		}

		// Token: 0x06001823 RID: 6179 RVA: 0x00117BB8 File Offset: 0x00115DB8
		public void setTimingCastAnimation(Farmer who)
		{
			if (who.CurrentTool == null)
			{
				return;
			}
			switch (who.FacingDirection)
			{
			case 0:
				who.FarmerSprite.setCurrentFrame(295);
				who.CurrentTool.Update(0, 0, who);
				return;
			case 1:
				who.FarmerSprite.setCurrentFrame(296);
				who.CurrentTool.Update(1, 0, who);
				return;
			case 2:
				who.FarmerSprite.setCurrentFrame(297);
				who.CurrentTool.Update(2, 0, who);
				return;
			case 3:
				who.FarmerSprite.setCurrentFrame(298);
				who.CurrentTool.Update(3, 0, who);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001824 RID: 6180 RVA: 0x00117C67 File Offset: 0x00115E67
		public void doneFishing(Farmer who, bool consumeBaitAndTackle = false)
		{
			this.doneFishingEvent.Fire(consumeBaitAndTackle);
		}

		// Token: 0x06001825 RID: 6181 RVA: 0x00117C78 File Offset: 0x00115E78
		private void doDoneFishing(bool consumeBaitAndTackle)
		{
			Farmer who = this.lastUser;
			if (consumeBaitAndTackle && who != null && who.IsLocalPlayer)
			{
				float consumeChance = 1f;
				if (base.hasEnchantmentOfType<PreservingEnchantment>())
				{
					consumeChance = 0.5f;
				}
				Object bait = this.GetBait();
				if (bait != null && Game1.random.NextDouble() < (double)consumeChance && bait.ConsumeStack(1) == null)
				{
					this.attachments[0] = null;
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14085"));
				}
				int i = 1;
				foreach (Object tackle in this.GetTackle())
				{
					if (tackle != null && !this.lastCatchWasJunk && Game1.random.NextDouble() < (double)consumeChance)
					{
						if (tackle.QualifiedItemId == "(O)789")
						{
							break;
						}
						NetInt uses = tackle.uses;
						int value = uses.Value;
						uses.Value = value + 1;
						if (tackle.uses.Value >= FishingRod.maxTackleUses)
						{
							this.attachments[i] = null;
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14086"));
						}
					}
					i++;
				}
			}
			if (who != null && who.IsLocalPlayer)
			{
				this.bobber.Set(Vector2.Zero);
			}
			this.isNibbling = false;
			this.fishCaught = false;
			this.isFishing = false;
			this.isReeling = false;
			this.isCasting = false;
			this.isTimingCast = false;
			this.treasureCaught = false;
			this.showingTreasure = false;
			this.doneWithAnimation = false;
			this.pullingOutOfWater = false;
			this.fromFishPond = false;
			this.numberOfFishCaught = 1;
			this.fishingBiteAccumulator = 0f;
			this.fishingNibbleAccumulator = 0f;
			this.timeUntilFishingBite = -1f;
			this.timeUntilFishingNibbleDone = -1f;
			this.bobberTimeAccumulator = 0f;
			if (FishingRod.chargeSound != null && FishingRod.chargeSound.IsPlaying && who.IsLocalPlayer)
			{
				FishingRod.chargeSound.Stop(AudioStopOptions.Immediate);
				FishingRod.chargeSound = null;
			}
			if (FishingRod.reelSound != null && FishingRod.reelSound.IsPlaying)
			{
				FishingRod.reelSound.Stop(AudioStopOptions.Immediate);
				FishingRod.reelSound = null;
			}
			if (who != null)
			{
				who.UsingTool = false;
				who.CanMove = true;
				who.completelyStopAnimatingOrDoingAction();
				if (who == Game1.player)
				{
					who.faceDirection(this.originalFacingDirection);
				}
			}
		}

		// Token: 0x06001826 RID: 6182 RVA: 0x00117EE4 File Offset: 0x001160E4
		public static void doneWithCastingAnimation(Farmer who)
		{
			FishingRod rod = who.CurrentTool as FishingRod;
			if (rod != null)
			{
				rod.doneWithAnimation = true;
				if (rod.hasDoneFucntionYet)
				{
					who.canReleaseTool = true;
					who.UsingTool = false;
					who.canMove = true;
					Farmer.canMoveNow(who);
				}
			}
		}

		// Token: 0x06001827 RID: 6183 RVA: 0x00117F2C File Offset: 0x0011612C
		public void castingEndFunction(Farmer who)
		{
			this.lastWaterColor = null;
			this.castedButBobberStillInAir = false;
			if (who != null)
			{
				float oldStamina = who.Stamina;
				this.DoFunction(who.currentLocation, (int)this.bobber.X, (int)this.bobber.Y, 1, who);
				who.lastClick = Vector2.Zero;
				ICue cue = FishingRod.reelSound;
				if (cue != null)
				{
					cue.Stop(AudioStopOptions.Immediate);
				}
				FishingRod.reelSound = null;
				if (who.Stamina <= 0f && oldStamina > 0f)
				{
					who.doEmote(36);
				}
				if (!this.isFishing && this.doneWithAnimation)
				{
					this.castingEndEnableMovement();
				}
			}
		}

		// Token: 0x06001828 RID: 6184 RVA: 0x00117FD4 File Offset: 0x001161D4
		private void castingEndEnableMovement()
		{
			this.castingEndEnableMovementEvent.Fire();
		}

		// Token: 0x06001829 RID: 6185 RVA: 0x00117FE1 File Offset: 0x001161E1
		private void doCastingEndEnableMovement()
		{
			Farmer.canMoveNow(this.lastUser);
		}

		// Token: 0x0600182A RID: 6186 RVA: 0x00117FF0 File Offset: 0x001161F0
		public override void tickUpdate(GameTime time, Farmer who)
		{
			this.lastUser = who;
			this.beginReelingEvent.Poll();
			this.putAwayEvent.Poll();
			this.startCastingEvent.Poll();
			this.pullFishFromWaterEvent.Poll();
			this.doneFishingEvent.Poll();
			this.castingEndEnableMovementEvent.Poll();
			if (this.recastTimerMs > 0 && who.IsLocalPlayer && who.freezePause <= 0)
			{
				if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.didPlayerJustClickAtAll(false) || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton))
				{
					this.recastTimerMs -= time.ElapsedGameTime.Milliseconds;
					if (this.recastTimerMs <= 0)
					{
						this.recastTimerMs = 0;
						if (Game1.activeClickableMenu == null)
						{
							who.BeginUsingTool();
						}
					}
				}
				else
				{
					this.recastTimerMs = 0;
				}
			}
			if (this.isFishing && !Game1.shouldTimePass(false) && Game1.activeClickableMenu != null && !(Game1.activeClickableMenu is BobberBar))
			{
				return;
			}
			if (who.CurrentTool != null && who.CurrentTool.Equals(this) && who.UsingTool)
			{
				who.CanMove = false;
			}
			else if (Game1.currentMinigame == null && (!(who.CurrentTool is FishingRod) || !who.UsingTool))
			{
				if (FishingRod.chargeSound != null && FishingRod.chargeSound.IsPlaying && who.IsLocalPlayer)
				{
					FishingRod.chargeSound.Stop(AudioStopOptions.Immediate);
					FishingRod.chargeSound = null;
				}
				return;
			}
			this.animations.RemoveWhere((TemporaryAnimatedSprite animation) => animation.update(time));
			if (this.sparklingText != null && this.sparklingText.update(time))
			{
				this.sparklingText = null;
			}
			if (this.castingChosenCountdown > 0f)
			{
				this.castingChosenCountdown -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.castingChosenCountdown <= 0f && who.CurrentTool != null)
				{
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.animateOnce(295, 1f, 1);
						who.CurrentTool.Update(0, 0, who);
						break;
					case 1:
						who.FarmerSprite.animateOnce(296, 1f, 1);
						who.CurrentTool.Update(1, 0, who);
						break;
					case 2:
						who.FarmerSprite.animateOnce(297, 1f, 1);
						who.CurrentTool.Update(2, 0, who);
						break;
					case 3:
						who.FarmerSprite.animateOnce(298, 1f, 1);
						who.CurrentTool.Update(3, 0, who);
						break;
					}
					if (who.FacingDirection == 1 || who.FacingDirection == 3)
					{
						float distance = Math.Max(128f, this.castingPower * (float)(this.getAddedDistance(who) + 4) * 64f);
						distance -= 8f;
						float gravity = 0.005f;
						float velocity = (float)((double)distance * Math.Sqrt((double)(gravity / (2f * (distance + 96f)))));
						float t = 2f * (velocity / gravity) + (float)((Math.Sqrt((double)(velocity * velocity + 2f * gravity * 96f)) - (double)velocity) / (double)gravity);
						Point playerPixel = who.StandingPixel;
						if (who.IsLocalPlayer)
						{
							this.bobber.Set(new Vector2((float)playerPixel.X + (float)((who.FacingDirection == 3) ? -1 : 1) * distance, (float)playerPixel.Y));
						}
						Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, this.getBobberStyle(who), 16, 32);
						sourceRect.Height = 16;
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect, t, 1, 0, who.Position + new Vector2(0f, -96f), false, false, (float)playerPixel.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f, false)
						{
							motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * velocity, -velocity),
							acceleration = new Vector2(0f, gravity),
							endFunction = delegate(int _)
							{
								this.castingEndFunction(who);
							},
							timeBasedMotion = true,
							flipped = (who.FacingDirection == 1 && this.flipCurrentBobberWhenFacingRight())
						});
					}
					else
					{
						float distance2 = -Math.Max(128f, this.castingPower * (float)(this.getAddedDistance(who) + 3) * 64f);
						float height = Math.Abs(distance2 - 64f);
						if (who.FacingDirection == 0)
						{
							distance2 = -distance2;
							height += 64f;
						}
						float gravity2 = 0.005f;
						float velocity2 = (float)Math.Sqrt((double)(2f * gravity2 * height));
						float t2 = (float)(Math.Sqrt((double)(2f * (height - distance2) / gravity2)) + (double)(velocity2 / gravity2));
						t2 *= 1.05f;
						if (who.FacingDirection == 0)
						{
							t2 *= 1.05f;
						}
						if (who.IsLocalPlayer)
						{
							Point playerPixel2 = who.StandingPixel;
							this.bobber.Set(new Vector2((float)playerPixel2.X, (float)playerPixel2.Y - distance2));
						}
						Rectangle sourceRect2 = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, this.getBobberStyle(who), 16, 32);
						sourceRect2.Height = 16;
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", sourceRect2, t2, 1, 0, who.Position + new Vector2(0f, -96f), false, false, this.bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f, false)
						{
							alphaFade = 0.0001f,
							motion = new Vector2(0f, -velocity2),
							acceleration = new Vector2(0f, gravity2),
							endFunction = delegate(int _)
							{
								this.castingEndFunction(who);
							},
							timeBasedMotion = true
						});
					}
					this._hasPlayerAdjustedBobber = false;
					this.castedButBobberStillInAir = true;
					this.isCasting = false;
					if (this.PlayUseSounds && who.IsLocalPlayer)
					{
						who.playNearbySoundAll("cast", null, SoundContext.Default);
						Game1.playSound("slowReel", 1600, out FishingRod.reelSound);
					}
				}
			}
			else if (!this.isTimingCast && this.castingChosenCountdown <= 0f)
			{
				who.jitterStrength = 0f;
			}
			if (this.isTimingCast)
			{
				this.castingPower = Math.Max(0f, Math.Min(1f, this.castingPower + this.castingTimerSpeed * (float)time.ElapsedGameTime.Milliseconds));
				if (this.PlayUseSounds && who.IsLocalPlayer)
				{
					if (FishingRod.chargeSound == null || !FishingRod.chargeSound.IsPlaying)
					{
						Game1.playSound("SinWave", out FishingRod.chargeSound);
					}
					Game1.sounds.SetPitch(FishingRod.chargeSound, 2400f * this.castingPower, true);
				}
				if (this.castingPower == 1f || this.castingPower == 0f)
				{
					this.castingTimerSpeed = -this.castingTimerSpeed;
				}
				who.armOffset.Y = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				who.jitterStrength = Math.Max(0f, this.castingPower - 0.5f);
				if (who.IsLocalPlayer && ((!this.usedGamePadToCast && Game1.input.GetMouseState().LeftButton == ButtonState.Released) || (this.usedGamePadToCast && Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonUp(Buttons.X))) && Game1.areAllOfTheseKeysUp(Game1.GetKeyboardState(), Game1.options.useToolButton))
				{
					this.startCasting();
					return;
				}
			}
			else
			{
				if (this.isReeling)
				{
					if (who.IsLocalPlayer && Game1.didPlayerJustClickAtAll(false))
					{
						if (Game1.isAnyGamePadButtonBeingPressed())
						{
							Game1.lastCursorMotionWasMouse = false;
						}
						switch (who.FacingDirection)
						{
						case 0:
							who.FarmerSprite.setCurrentSingleFrame(76, 32000, false, false);
							break;
						case 1:
							who.FarmerSprite.setCurrentSingleFrame(72, 100, false, false);
							break;
						case 2:
							who.FarmerSprite.setCurrentSingleFrame(75, 32000, false, false);
							break;
						case 3:
							who.FarmerSprite.setCurrentSingleFrame(72, 100, false, true);
							break;
						}
						who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
						who.jitterStrength = 1f;
					}
					else
					{
						switch (who.FacingDirection)
						{
						case 0:
							who.FarmerSprite.setCurrentSingleFrame(36, 32000, false, false);
							break;
						case 1:
							who.FarmerSprite.setCurrentSingleFrame(48, 100, false, false);
							break;
						case 2:
							who.FarmerSprite.setCurrentSingleFrame(66, 32000, false, false);
							break;
						case 3:
							who.FarmerSprite.setCurrentSingleFrame(48, 100, false, true);
							break;
						}
						who.stopJittering();
					}
					who.armOffset = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)Game1.random.Next(-10, 11) / 10f);
					this.bobberTimeAccumulator += (float)time.ElapsedGameTime.Milliseconds;
					return;
				}
				if (this.isFishing)
				{
					if (who.IsLocalPlayer)
					{
						this.bobber.Y += (float)(0.11999999731779099 * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0));
					}
					who.canReleaseTool = true;
					this.bobberTimeAccumulator += (float)time.ElapsedGameTime.Milliseconds;
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.setCurrentFrame(44);
						break;
					case 1:
						who.FarmerSprite.setCurrentFrame(89);
						break;
					case 2:
						who.FarmerSprite.setCurrentFrame(70);
						break;
					case 3:
						who.FarmerSprite.setCurrentFrame(89, 0, 10, 1, true, false);
						break;
					}
					who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) + (float)((who.FacingDirection == 1 || who.FacingDirection == 3) ? 1 : -1);
					if (who.IsLocalPlayer)
					{
						if (this.timeUntilFishingBite != -1f)
						{
							this.fishingBiteAccumulator += (float)time.ElapsedGameTime.Milliseconds;
							if (this.fishingBiteAccumulator > this.timeUntilFishingBite)
							{
								this.fishingBiteAccumulator = 0f;
								this.timeUntilFishingBite = -1f;
								this.isNibbling = true;
								if (base.hasEnchantmentOfType<AutoHookEnchantment>())
								{
									this.timePerBobberBob = 1f;
									this.timeUntilFishingNibbleDone = (float)FishingRod.maxTimeToNibble;
									this.DoFunction(who.currentLocation, (int)this.bobber.X, (int)this.bobber.Y, 1, who);
									Rumble.rumble(0.95f, 200f);
									return;
								}
								who.PlayFishBiteChime();
								Rumble.rumble(0.75f, 250f);
								this.timeUntilFishingNibbleDone = (float)FishingRod.maxTimeToNibble;
								Point playerPixel3 = who.StandingPixel;
								Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(395, 497, 3, 8), new Vector2((float)(playerPixel3.X - Game1.viewport.X), (float)(playerPixel3.Y - 128 - 8 - Game1.viewport.Y)), false, 0.02f, Color.White)
								{
									scale = 5f,
									scaleChange = -0.01f,
									motion = new Vector2(0f, -0.5f),
									shakeIntensityChange = -0.005f,
									shakeIntensity = 1f
								});
								this.timePerBobberBob = 1f;
							}
						}
						if (this.timeUntilFishingNibbleDone != -1f && !this.hit)
						{
							this.fishingNibbleAccumulator += (float)time.ElapsedGameTime.Milliseconds;
							if (this.fishingNibbleAccumulator > this.timeUntilFishingNibbleDone)
							{
								this.fishingNibbleAccumulator = 0f;
								this.timeUntilFishingNibbleDone = -1f;
								this.isNibbling = false;
								this.timeUntilFishingBite = this.calculateTimeUntilFishingBite(this.calculateBobberTile(), false, who);
								return;
							}
						}
					}
				}
				else if (who.UsingTool && this.castedButBobberStillInAir)
				{
					Vector2 motion = Vector2.Zero;
					if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadDown) || Game1.input.GetGamePadState().ThumbSticks.Left.Y < 0f))) && who.FacingDirection != 2 && who.FacingDirection != 0)
					{
						motion.Y += 4f;
						this._hasPlayerAdjustedBobber = true;
					}
					if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadRight) || Game1.input.GetGamePadState().ThumbSticks.Left.X > 0f))) && who.FacingDirection != 1 && who.FacingDirection != 3)
					{
						motion.X += 2f;
						this._hasPlayerAdjustedBobber = true;
					}
					if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadUp) || Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0f))) && who.FacingDirection != 0 && who.FacingDirection != 2)
					{
						motion.Y -= 4f;
						this._hasPlayerAdjustedBobber = true;
					}
					if ((Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.DPadLeft) || Game1.input.GetGamePadState().ThumbSticks.Left.X < 0f))) && who.FacingDirection != 3 && who.FacingDirection != 1)
					{
						motion.X -= 2f;
						this._hasPlayerAdjustedBobber = true;
					}
					if (!this._hasPlayerAdjustedBobber)
					{
						Vector2 bobber_tile = this.calculateBobberTile();
						if (!who.currentLocation.isTileFishable((int)bobber_tile.X, (int)bobber_tile.Y))
						{
							if (who.FacingDirection == 3 || who.FacingDirection == 1)
							{
								int offset = 1;
								if (bobber_tile.Y % 1f < 0.5f)
								{
									offset = -1;
								}
								if (who.currentLocation.isTileFishable((int)bobber_tile.X, (int)bobber_tile.Y + offset))
								{
									motion.Y += (float)offset * 4f;
								}
								else if (who.currentLocation.isTileFishable((int)bobber_tile.X, (int)bobber_tile.Y - offset))
								{
									motion.Y -= (float)offset * 4f;
								}
							}
							if (who.FacingDirection == 0 || who.FacingDirection == 2)
							{
								int offset2 = 1;
								if (bobber_tile.X % 1f < 0.5f)
								{
									offset2 = -1;
								}
								if (who.currentLocation.isTileFishable((int)bobber_tile.X + offset2, (int)bobber_tile.Y))
								{
									motion.X += (float)offset2 * 4f;
								}
								else if (who.currentLocation.isTileFishable((int)bobber_tile.X - offset2, (int)bobber_tile.Y))
								{
									motion.X -= (float)offset2 * 4f;
								}
							}
						}
					}
					if (who.IsLocalPlayer)
					{
						this.bobber.Set(this.bobber.Value + motion);
						this._totalMotion.Set(this._totalMotion.Value + motion);
					}
					if (this.animations.Count > 0)
					{
						Vector2 applied_motion = Vector2.Zero;
						if (who.IsLocalPlayer)
						{
							applied_motion = this._totalMotion.Value;
						}
						else
						{
							this._totalMotionBuffer[this._totalMotionBufferIndex] = this._totalMotion.Value;
							for (int i = 0; i < this._totalMotionBuffer.Length; i++)
							{
								applied_motion += this._totalMotionBuffer[i];
							}
							applied_motion /= (float)this._totalMotionBuffer.Length;
							this._totalMotionBufferIndex = (this._totalMotionBufferIndex + 1) % this._totalMotionBuffer.Length;
						}
						this.animations[0].position -= this._lastAppliedMotion;
						this._lastAppliedMotion = applied_motion;
						this.animations[0].position += applied_motion;
						return;
					}
				}
				else
				{
					if (this.showingTreasure)
					{
						who.FarmerSprite.setCurrentSingleFrame(0, 32000, false, false);
						return;
					}
					if (this.fishCaught)
					{
						if (!Game1.isFestival())
						{
							who.faceDirection(2);
							who.FarmerSprite.setCurrentFrame(84);
						}
						if (Game1.random.NextDouble() < 0.025)
						{
							who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2((float)(Game1.random.Next(-3, 2) * 4), -32f), false, false, (float)who.StandingPixel.Y / 10000f + 0.002f, 0.04f, Color.LightBlue, 5f, 0f, 0f, 0f, false)
							{
								acceleration = new Vector2(0f, 0.25f)
							});
						}
						if (who.IsLocalPlayer && (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.didPlayerJustClickAtAll(false) || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton)))
						{
							this.doneHoldingFish(who, false);
							return;
						}
					}
					else
					{
						if (who.UsingTool && this.castedButBobberStillInAir && this.doneWithAnimation)
						{
							switch (who.FacingDirection)
							{
							case 0:
								who.FarmerSprite.setCurrentFrame(39);
								break;
							case 1:
								who.FarmerSprite.setCurrentFrame(89);
								break;
							case 2:
								who.FarmerSprite.setCurrentFrame(28);
								break;
							case 3:
								who.FarmerSprite.setCurrentFrame(89, 0, 10, 1, true, false);
								break;
							}
							who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
							return;
						}
						if (!this.castedButBobberStillInAir && this.whichFish != null && this.animations.Count > 0 && this.animations[0].timer > 500f && !Game1.eventUp)
						{
							who.faceDirection(2);
							who.FarmerSprite.setCurrentFrame(57);
						}
					}
				}
			}
		}

		// Token: 0x0600182B RID: 6187 RVA: 0x00119680 File Offset: 0x00117880
		public void doneHoldingFish(Farmer who, bool endOfNight = false)
		{
			if (this.PlayUseSounds)
			{
				who.playNearbySoundLocal("coin", null, SoundContext.Default);
			}
			if (!this.fromFishPond && Game1.IsSummer && this.whichFish.QualifiedItemId == "(O)138" && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21 && Game1.random.NextDouble() < 0.33 * (double)this.numberOfFishCaught)
			{
				this.gotTroutDerbyTag = true;
			}
			if (!this.treasureCaught && !this.gotTroutDerbyTag)
			{
				this.recastTimerMs = 200;
				Item item = this.CreateFish();
				bool fishIsObject = item.HasTypeObject();
				if ((item.Category == -4 || item.HasContextTag("counts_as_fish_catch")) && !this.fromFishPond)
				{
					Game1.player.stats.Increment("PreciseFishCaught", Math.Max(1, this.numberOfFishCaught));
				}
				if (item.QualifiedItemId == "(O)79" || item.QualifiedItemId == "(O)842")
				{
					item = who.currentLocation.tryToCreateUnseenSecretNote(who);
					if (item == null)
					{
						return;
					}
				}
				bool caughtFromFishPond = this.fromFishPond;
				who.completelyStopAnimatingOrDoingAction();
				this.doneFishing(who, !caughtFromFishPond);
				if (!Game1.isFestival() && !caughtFromFishPond && fishIsObject && who.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder in who.team.specialOrders)
					{
						Action<Farmer, Item> onFishCaught = specialOrder.onFishCaught;
						if (onFishCaught != null)
						{
							onFishCaught(who, item);
						}
					}
				}
				if (!Game1.isFestival() && !who.addItemToInventoryBool(item, false))
				{
					if (endOfNight)
					{
						Game1.createItemDebris(item, who.getStandingPosition(), -1, who.currentLocation, -1, false);
						return;
					}
					Game1.activeClickableMenu = new ItemGrabMenu(new List<Item>
					{
						item
					}, this).setEssential(true, false);
					return;
				}
			}
			else
			{
				this.fishCaught = false;
				this.showingTreasure = true;
				who.UsingTool = true;
				Item item2 = this.CreateFish();
				if ((item2.Category == -4 || item2.HasContextTag("counts_as_fish_catch")) && !this.fromFishPond)
				{
					Game1.player.stats.Increment("PreciseFishCaught", Math.Max(1, this.numberOfFishCaught));
				}
				if (who.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder2 in who.team.specialOrders)
					{
						Action<Farmer, Item> onFishCaught2 = specialOrder2.onFishCaught;
						if (onFishCaught2 != null)
						{
							onFishCaught2(who, item2);
						}
					}
				}
				bool hadRoomForFish = who.addItemToInventoryBool(item2, false);
				if (!endOfNight)
				{
					if (this.treasureCaught)
					{
						this.animations.Add(new TemporaryAnimatedSprite(this.goldenTreasure ? "LooseSprites\\Cursors_1_6" : "LooseSprites\\Cursors", this.goldenTreasure ? new Rectangle(256, 75, 32, 32) : new Rectangle(64, 1920, 32, 32), 500f, 1, 0, who.Position + new Vector2(-32f, -160f), false, false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(0f, -0.128f),
							timeBasedMotion = true,
							endFunction = new TemporaryAnimatedSprite.endBehavior(this.openChestEndFunction),
							extraInfoForEndBehavior = (hadRoomForFish ? 0 : item2.Stack),
							alpha = 0f,
							alphaFade = -0.002f
						});
						return;
					}
					if (this.gotTroutDerbyTag)
					{
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\Objects_2", new Rectangle(80, 16, 16, 16), 500f, 1, 0, who.Position + new Vector2(-8f, -128f), false, false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(0f, -0.128f),
							timeBasedMotion = true,
							endFunction = new TemporaryAnimatedSprite.endBehavior(this.openChestEndFunction),
							extraInfoForEndBehavior = (hadRoomForFish ? 0 : item2.Stack),
							alpha = 0f,
							alphaFade = -0.002f,
							id = 1074
						});
						return;
					}
				}
				else if (!hadRoomForFish)
				{
					Game1.createItemDebris(item2, who.getStandingPosition(), -1, who.currentLocation, -1, false);
				}
			}
		}

		// Token: 0x0600182C RID: 6188 RVA: 0x00119B6C File Offset: 0x00117D6C
		private Item CreateFish()
		{
			Item fish = this.whichFish.CreateItemOrErrorItem(1, this.fishQuality);
			fish.SetFlagOnPickup = this.setFlagOnCatch;
			if (fish.HasTypeObject())
			{
				if (fish.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID)
				{
					Object obj = fish as Object;
					if (obj != null)
					{
						obj.questItem.Value = true;
					}
				}
				else if (this.numberOfFishCaught > 1 && fish.QualifiedItemId != "(O)79" && fish.QualifiedItemId != "(O)842")
				{
					fish.Stack = this.numberOfFishCaught;
				}
			}
			return fish;
		}

		// Token: 0x0600182D RID: 6189 RVA: 0x00119C04 File Offset: 0x00117E04
		private void startCasting()
		{
			this.startCastingEvent.Fire();
		}

		// Token: 0x0600182E RID: 6190 RVA: 0x00119C11 File Offset: 0x00117E11
		public void beginReeling()
		{
			this.isReeling = true;
		}

		// Token: 0x0600182F RID: 6191 RVA: 0x00119C1C File Offset: 0x00117E1C
		private void doStartCasting()
		{
			Farmer who = this.lastUser;
			this.randomBobberStyle = -1;
			if (FishingRod.chargeSound != null && who.IsLocalPlayer)
			{
				FishingRod.chargeSound.Stop(AudioStopOptions.Immediate);
				FishingRod.chargeSound = null;
			}
			if (who.currentLocation == null)
			{
				return;
			}
			if (who.IsLocalPlayer)
			{
				if (this.PlayUseSounds)
				{
					who.playNearbySoundLocal("button1", null, SoundContext.Default);
				}
				Rumble.rumble(0.5f, 150f);
			}
			who.UsingTool = true;
			this.isTimingCast = false;
			this.isCasting = true;
			this.castingChosenCountdown = 350f;
			who.armOffset.Y = 0f;
			if (this.castingPower > 0.99f && who.IsLocalPlayer)
			{
				Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(545, 1921, 53, 19), 800f, 1, 0, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -192f)), false, false, 1f, 0.01f, Color.White, 2f, 0f, 0f, 0f, true)
				{
					motion = new Vector2(0f, -4f),
					acceleration = new Vector2(0f, 0.2f),
					delayBeforeAnimationStart = 200
				});
				if (this.PlayUseSounds)
				{
					DelayedAction.playSoundAfterDelay("crit", 200, null, null, -1, false);
				}
			}
		}

		// Token: 0x06001830 RID: 6192 RVA: 0x00119DB0 File Offset: 0x00117FB0
		public void openChestEndFunction(int remainingFish)
		{
			Farmer who = this.lastUser;
			if (this.gotTroutDerbyTag && !this.treasureCaught)
			{
				who.playNearbySoundLocal("discoverMineral", null, SoundContext.Default);
				this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\Objects_2", new Rectangle(80, 16, 16, 16), 800f, 1, 0, who.Position + new Vector2(-8f, -196f), false, false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.justGotDerbyTagEndFunction),
					extraInfoForEndBehavior = remainingFish,
					shakeIntensity = 0f
				});
				this.animations.AddRange(Utility.getTemporarySpritesWithinArea(new int[]
				{
					10,
					11
				}, new Rectangle((int)who.Position.X - 16, (int)who.Position.Y - 228 + 16, 32, 32), 4, Color.White, 100, 0, ""));
			}
			else
			{
				who.playNearbySoundLocal("openChest", null, SoundContext.Default);
				this.animations.Add(new TemporaryAnimatedSprite(this.goldenTreasure ? "LooseSprites\\Cursors_1_6" : "LooseSprites\\Cursors", this.goldenTreasure ? new Rectangle(256, 75, 32, 32) : new Rectangle(64, 1920, 32, 32), 200f, 4, 0, who.Position + new Vector2(-32f, -228f), false, false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.openTreasureMenuEndFunction),
					extraInfoForEndBehavior = remainingFish
				});
			}
			this.sparklingText = null;
		}

		// Token: 0x06001831 RID: 6193 RVA: 0x00119FCC File Offset: 0x001181CC
		public void justGotDerbyTagEndFunction(int remainingFish)
		{
			Farmer who = this.lastUser;
			who.UsingTool = false;
			this.doneFishing(who, true);
			Item tag = ItemRegistry.Create("(O)TroutDerbyTag", 1, 0, false);
			Item fish = null;
			if (remainingFish == 1)
			{
				fish = this.CreateFish();
			}
			if (this.PlayUseSounds)
			{
				Game1.playSound("coin", null);
			}
			this.gotTroutDerbyTag = false;
			if (!who.addItemToInventoryBool(tag, false))
			{
				List<Item> items = new List<Item>
				{
					tag
				};
				if (fish != null)
				{
					items.Add(fish);
				}
				ItemGrabMenu itemGrabMenu = new ItemGrabMenu(items, this).setEssential(true, false);
				itemGrabMenu.source = 3;
				Game1.activeClickableMenu = itemGrabMenu;
				who.completelyStopAnimatingOrDoingAction();
				return;
			}
			if (fish != null && !who.addItemToInventoryBool(fish, false))
			{
				ItemGrabMenu itemGrabMenu2 = new ItemGrabMenu(new List<Item>
				{
					fish
				}, this).setEssential(true, false);
				itemGrabMenu2.source = 3;
				Game1.activeClickableMenu = itemGrabMenu2;
				who.completelyStopAnimatingOrDoingAction();
				return;
			}
		}

		// Token: 0x06001832 RID: 6194 RVA: 0x0011A0AB File Offset: 0x001182AB
		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		// Token: 0x06001833 RID: 6195 RVA: 0x0011A0B0 File Offset: 0x001182B0
		public void openTreasureMenuEndFunction(int remainingFish)
		{
			Farmer who = this.lastUser;
			who.gainExperience(5, 10 * (this.clearWaterDistance + 1));
			who.UsingTool = false;
			who.completelyStopAnimatingOrDoingAction();
			bool flag = this.treasureCaught;
			this.doneFishing(who, true);
			List<Item> treasures = new List<Item>();
			if (remainingFish == 1)
			{
				treasures.Add(this.CreateFish());
			}
			float chance = 1f;
			if (flag)
			{
				Game1.player.stats.Increment("FishingTreasures", 1);
				while (Game1.random.NextDouble() <= (double)chance)
				{
					chance *= (this.goldenTreasure ? 0.6f : 0.4f);
					if (Game1.IsSpring && !(who.currentLocation is Beach) && Game1.random.NextDouble() < 0.1)
					{
						treasures.Add(ItemRegistry.Create("(O)273", Game1.random.Next(2, 6) + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), 0, false));
					}
					if (this.numberOfFishCaught > 1 && who.craftingRecipes.ContainsKey("Wild Bait") && Game1.random.NextBool())
					{
						treasures.Add(ItemRegistry.Create("(O)774", 2 + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), 0, false));
					}
					if (Game1.random.NextDouble() <= 0.33 && who.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
					{
						treasures.Add(ItemRegistry.Create("(O)890", Game1.random.Next(1, 3) + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), 0, false));
					}
					while (Utility.tryRollMysteryBox(0.08 + Game1.player.team.AverageDailyLuck(null) / 5.0, null))
					{
						treasures.Add(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 1, 0, false));
					}
					if (Game1.player.stats.Get(StatKeys.Mastery(0)) > 0U && Game1.random.NextDouble() < 0.05)
					{
						treasures.Add(ItemRegistry.Create("(O)GoldenAnimalCracker", 1, 0, false));
					}
					if (this.goldenTreasure && Game1.random.NextDouble() < 0.5)
					{
						switch (Game1.random.Next(13))
						{
						case 0:
							treasures.Add(ItemRegistry.Create("(O)337", Game1.random.Next(1, 6), 0, false));
							break;
						case 1:
							treasures.Add(ItemRegistry.Create("(O)SkillBook_" + Game1.random.Next(5).ToString(), 1, 0, false));
							break;
						case 2:
							treasures.Add(Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 8));
							break;
						case 3:
							treasures.Add(ItemRegistry.Create("(O)213", 1, 0, false));
							break;
						case 4:
							treasures.Add(ItemRegistry.Create("(O)872", Game1.random.Next(3, 6), 0, false));
							break;
						case 5:
							treasures.Add(ItemRegistry.Create("(O)687", 1, 0, false));
							break;
						case 6:
							treasures.Add(ItemRegistry.Create("(O)ChallengeBait", Game1.random.Next(3, 6), 0, false));
							break;
						case 7:
							treasures.Add(ItemRegistry.Create("(O)703", Game1.random.Next(3, 6), 0, false));
							break;
						case 8:
							treasures.Add(ItemRegistry.Create("(O)StardropTea", 1, 0, false));
							break;
						case 9:
							treasures.Add(ItemRegistry.Create("(O)797", 1, 0, false));
							break;
						case 10:
							treasures.Add(ItemRegistry.Create("(O)733", 1, 0, false));
							break;
						case 11:
							treasures.Add(ItemRegistry.Create("(O)728", 1, 0, false));
							break;
						case 12:
							treasures.Add(ItemRegistry.Create("(O)SonarBobber", 1, 0, false));
							break;
						}
					}
					else
					{
						switch (Game1.random.Next(4))
						{
						case 0:
							if (this.clearWaterDistance >= 5 && Game1.random.NextDouble() < 0.03)
							{
								treasures.Add(new Object("386", Game1.random.Next(1, 3), false, -1, 0));
							}
							else
							{
								List<int> possibles = new List<int>();
								if (this.clearWaterDistance >= 4)
								{
									possibles.Add(384);
								}
								if (this.clearWaterDistance >= 3 && (possibles.Count == 0 || Game1.random.NextDouble() < 0.6))
								{
									possibles.Add(380);
								}
								if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
								{
									possibles.Add(378);
								}
								if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
								{
									possibles.Add(388);
								}
								if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
								{
									possibles.Add(390);
								}
								possibles.Add(382);
								Item treasure = ItemRegistry.Create(Game1.random.ChooseFrom(possibles).ToString(), Game1.random.Next(2, 7) * ((Game1.random.NextDouble() < 0.05 + (double)who.luckLevel.Value * 0.015) ? 2 : 1), 0, false);
								if (Game1.random.NextDouble() < 0.05 + (double)who.LuckLevel * 0.03)
								{
									treasure.Stack *= 2;
								}
								treasures.Add(treasure);
							}
							break;
						case 1:
							if (this.clearWaterDistance >= 4 && Game1.random.NextDouble() < 0.1 && who.FishingLevel >= 6)
							{
								treasures.Add(ItemRegistry.Create("(O)687", 1, 0, false));
							}
							else if (Game1.random.NextDouble() < 0.25 && who.craftingRecipes.ContainsKey("Wild Bait"))
							{
								treasures.Add(ItemRegistry.Create("(O)774", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), 0, false));
							}
							else if (Game1.random.NextDouble() < 0.11 && who.FishingLevel >= 6)
							{
								treasures.Add(ItemRegistry.Create("(O)SonarBobber", 1, 0, false));
							}
							else if (who.FishingLevel >= 6)
							{
								treasures.Add(ItemRegistry.Create("(O)DeluxeBait", 5, 0, false));
							}
							else
							{
								treasures.Add(ItemRegistry.Create("(O)685", 10, 0, false));
							}
							break;
						case 2:
							if (Game1.random.NextDouble() < 0.1 && Game1.netWorldState.Value.LostBooksFound < 21 && who != null && who.hasOrWillReceiveMail("lostBookFound"))
							{
								treasures.Add(ItemRegistry.Create("(O)102", 1, 0, false));
							}
							else if (who.archaeologyFound.Length > 0)
							{
								if (Game1.random.NextDouble() < 0.25 && who.FishingLevel > 1)
								{
									treasures.Add(ItemRegistry.Create("(O)" + Game1.random.Next(585, 588).ToString(), 1, 0, false));
								}
								else if (Game1.random.NextBool() && who.FishingLevel > 1)
								{
									treasures.Add(ItemRegistry.Create("(O)" + Game1.random.Next(103, 120).ToString(), 1, 0, false));
								}
								else
								{
									treasures.Add(ItemRegistry.Create("(O)535", 1, 0, false));
								}
							}
							else
							{
								treasures.Add(ItemRegistry.Create("(O)382", Game1.random.Next(1, 3), 0, false));
							}
							break;
						case 3:
							switch (Game1.random.Next(3))
							{
							case 0:
							{
								Item treasure2;
								if (this.clearWaterDistance >= 4)
								{
									treasure2 = ItemRegistry.Create("(O)" + (537 + ((Game1.random.NextDouble() < 0.4) ? Game1.random.Next(-2, 0) : 0)).ToString(), Game1.random.Next(1, 4), 0, false);
								}
								else if (this.clearWaterDistance >= 3)
								{
									treasure2 = ItemRegistry.Create("(O)" + (536 + ((Game1.random.NextDouble() < 0.4) ? -1 : 0)).ToString(), Game1.random.Next(1, 4), 0, false);
								}
								else
								{
									treasure2 = ItemRegistry.Create("(O)535", Game1.random.Next(1, 4), 0, false);
								}
								if (Game1.random.NextDouble() < 0.05 + (double)who.LuckLevel * 0.03)
								{
									treasure2.Stack *= 2;
								}
								treasures.Add(treasure2);
								break;
							}
							case 1:
								if (who.FishingLevel < 2)
								{
									treasures.Add(ItemRegistry.Create("(O)382", Game1.random.Next(1, 4), 0, false));
								}
								else
								{
									Item treasure3;
									if (this.clearWaterDistance >= 4)
									{
										treasures.Add(treasure3 = ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.3) ? 82 : Game1.random.Choose(64, 60)).ToString(), Game1.random.Next(1, 3), 0, false));
									}
									else if (this.clearWaterDistance >= 3)
									{
										treasures.Add(treasure3 = ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.3) ? 84 : Game1.random.Choose(70, 62)).ToString(), Game1.random.Next(1, 3), 0, false));
									}
									else
									{
										treasures.Add(treasure3 = ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.3) ? 86 : Game1.random.Choose(66, 68)).ToString(), Game1.random.Next(1, 3), 0, false));
									}
									if (Game1.random.NextDouble() < 0.028 * (double)((float)this.clearWaterDistance / 5f))
									{
										treasures.Add(treasure3 = ItemRegistry.Create("(O)72", 1, 0, false));
									}
									if (Game1.random.NextDouble() < 0.05)
									{
										treasure3.Stack *= 2;
									}
								}
								break;
							case 2:
								if (who.FishingLevel < 2)
								{
									treasures.Add(new Object("770", Game1.random.Next(1, 4), false, -1, 0));
								}
								else
								{
									float luckModifier = (1f + (float)who.DailyLuck) * ((float)this.clearWaterDistance / 5f);
									if (Game1.random.NextDouble() < 0.05 * (double)luckModifier && !who.specialItems.Contains("14"))
									{
										Item weapon = MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)14", 1, 0, false), Game1.random, false, null);
										weapon.specialItem = true;
										treasures.Add(weapon);
									}
									if (Game1.random.NextDouble() < 0.05 * (double)luckModifier && !who.specialItems.Contains("51"))
									{
										Item weapon2 = MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)51", 1, 0, false), Game1.random, false, null);
										weapon2.specialItem = true;
										treasures.Add(weapon2);
									}
									if (Game1.random.NextDouble() < 0.07 * (double)luckModifier)
									{
										switch (Game1.random.Next(3))
										{
										case 0:
											treasures.Add(new Ring((516 + ((Game1.random.NextDouble() < (double)((float)who.LuckLevel / 11f)) ? 1 : 0)).ToString()));
											break;
										case 1:
											treasures.Add(new Ring((518 + ((Game1.random.NextDouble() < (double)((float)who.LuckLevel / 11f)) ? 1 : 0)).ToString()));
											break;
										case 2:
											treasures.Add(new Ring(Game1.random.Next(529, 535).ToString()));
											break;
										}
									}
									if (Game1.random.NextDouble() < 0.02 * (double)luckModifier)
									{
										treasures.Add(ItemRegistry.Create("(O)166", 1, 0, false));
									}
									if (who.FishingLevel > 5 && Game1.random.NextDouble() < 0.001 * (double)luckModifier)
									{
										treasures.Add(ItemRegistry.Create("(O)74", 1, 0, false));
									}
									if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
									{
										treasures.Add(ItemRegistry.Create("(O)127", 1, 0, false));
									}
									if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
									{
										treasures.Add(ItemRegistry.Create("(O)126", 1, 0, false));
									}
									if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
									{
										treasures.Add(new Ring("527"));
									}
									if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
									{
										treasures.Add(ItemRegistry.Create("(B)" + Game1.random.Next(504, 514).ToString(), 1, 0, false));
									}
									if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && Game1.random.NextDouble() < 0.01 * (double)luckModifier)
									{
										treasures.Add(ItemRegistry.Create("(O)928", 1, 0, false));
									}
									if (treasures.Count == 1)
									{
										treasures.Add(ItemRegistry.Create("(O)72", 1, 0, false));
									}
									if (Game1.player.stats.Get("FishingTreasures") > 3U)
									{
										Random r = Utility.CreateRandom(Game1.player.stats.Get("FishingTreasures") * 27973U, Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0);
										if (r.NextDouble() < 0.05 * (double)luckModifier)
										{
											treasures.Add(ItemRegistry.Create("(O)SkillBook_" + r.Next(5).ToString(), 1, 0, false));
											chance = 0f;
										}
									}
								}
								break;
							}
							break;
						}
					}
				}
				if (treasures.Count == 0)
				{
					treasures.Add(ItemRegistry.Create("(O)685", Game1.random.Next(1, 4) * 5, 0, false));
				}
				if (this.lastUser.hasQuest("98765") && Utility.GetDayOfPassiveFestival("DesertFestival") == 3 && !this.lastUser.Items.ContainsId("GoldenBobber", 1))
				{
					treasures.Clear();
					treasures.Add(ItemRegistry.Create("(O)GoldenBobber", 1, 0, false));
				}
				if (Game1.random.NextDouble() < 0.25 && this.lastUser.stats.Get("Book_Roe") > 0U)
				{
					Item fish = this.CreateFish();
					ObjectDataDefinition objectData = ItemRegistry.GetObjectTypeDefinition();
					if (objectData.CanHaveRoe(fish))
					{
						Object roe = objectData.CreateFlavoredRoe(fish as Object);
						roe.Stack = Game1.random.Next(1, 3);
						if (Game1.random.NextDouble() < 0.1 + this.lastUser.team.AverageDailyLuck(null))
						{
							Object @object = roe;
							int stack = @object.Stack;
							@object.Stack = stack + 1;
						}
						if (Game1.random.NextDouble() < 0.1 + this.lastUser.team.AverageDailyLuck(null))
						{
							roe.Stack *= 2;
						}
						treasures.Add(roe);
					}
				}
				if (Game1.player.fishingLevel.Value > 4 && Game1.player.stats.Get("FishingTreasures") > 2U && Game1.random.NextDouble() < 0.02 + ((!Game1.player.mailReceived.Contains("roeBookDropped")) ? (Game1.player.stats.Get("FishingTreasures") * 0.001) : 0.001))
				{
					treasures.Add(ItemRegistry.Create("(O)Book_Roe", 1, 0, false));
					Game1.player.mailReceived.Add("roeBookDropped");
				}
			}
			if (this.gotTroutDerbyTag)
			{
				treasures.Add(ItemRegistry.Create("(O)TroutDerbyTag", 1, 0, false));
				this.gotTroutDerbyTag = false;
			}
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(treasures, this).setEssential(true, false);
			itemGrabMenu.source = 3;
			Game1.activeClickableMenu = itemGrabMenu;
			who.completelyStopAnimatingOrDoingAction();
		}

		// Token: 0x04000E69 RID: 3689
		public const int BaitIndex = 0;

		// Token: 0x04000E6A RID: 3690
		public const int TackleIndex = 1;

		// Token: 0x04000E6B RID: 3691
		public const int sizeOfLandCheckRectangle = 11;

		// Token: 0x04000E6C RID: 3692
		public static int NUM_BOBBER_STYLES = 39;

		// Token: 0x04000E6D RID: 3693
		[XmlElement("bobber")]
		public readonly NetPosition bobber = new NetPosition();

		// Token: 0x04000E6E RID: 3694
		private readonly NetInt castDirection = new NetInt(-1);

		// Token: 0x04000E6F RID: 3695
		public static int minFishingBiteTime = 600;

		// Token: 0x04000E70 RID: 3696
		public static int maxFishingBiteTime = 30000;

		// Token: 0x04000E71 RID: 3697
		public static int maxTimeToNibble = 800;

		// Token: 0x04000E72 RID: 3698
		public static int maxTackleUses = 20;

		// Token: 0x04000E73 RID: 3699
		private int whichTackleSlotToReplace = 1;

		// Token: 0x04000E74 RID: 3700
		protected Vector2 _lastAppliedMotion = Vector2.Zero;

		// Token: 0x04000E75 RID: 3701
		protected Vector2[] _totalMotionBuffer = new Vector2[4];

		// Token: 0x04000E76 RID: 3702
		protected int _totalMotionBufferIndex;

		// Token: 0x04000E77 RID: 3703
		protected NetVector2 _totalMotion = new NetVector2(Vector2.Zero)
		{
			InterpolationEnabled = false,
			InterpolationWait = false
		};

		// Token: 0x04000E78 RID: 3704
		public static double baseChanceForTreasure = 0.15;

		// Token: 0x04000E79 RID: 3705
		[XmlIgnore]
		public int bobberBob;

		// Token: 0x04000E7A RID: 3706
		[XmlIgnore]
		public float bobberTimeAccumulator;

		// Token: 0x04000E7B RID: 3707
		[XmlIgnore]
		public float timePerBobberBob = 2000f;

		// Token: 0x04000E7C RID: 3708
		[XmlIgnore]
		public float timeUntilFishingBite = -1f;

		// Token: 0x04000E7D RID: 3709
		[XmlIgnore]
		public float fishingBiteAccumulator;

		// Token: 0x04000E7E RID: 3710
		[XmlIgnore]
		public float fishingNibbleAccumulator;

		// Token: 0x04000E7F RID: 3711
		[XmlIgnore]
		public float timeUntilFishingNibbleDone = -1f;

		// Token: 0x04000E80 RID: 3712
		[XmlIgnore]
		public float castingPower;

		// Token: 0x04000E81 RID: 3713
		[XmlIgnore]
		public float castingChosenCountdown;

		// Token: 0x04000E82 RID: 3714
		[XmlIgnore]
		public float castingTimerSpeed = 0.001f;

		// Token: 0x04000E83 RID: 3715
		[XmlIgnore]
		public bool isFishing;

		// Token: 0x04000E84 RID: 3716
		[XmlIgnore]
		public bool hit;

		// Token: 0x04000E85 RID: 3717
		[XmlIgnore]
		public bool isNibbling;

		// Token: 0x04000E86 RID: 3718
		[XmlIgnore]
		public bool favBait;

		// Token: 0x04000E87 RID: 3719
		[XmlIgnore]
		public bool isTimingCast;

		// Token: 0x04000E88 RID: 3720
		[XmlIgnore]
		public bool isCasting;

		// Token: 0x04000E89 RID: 3721
		[XmlIgnore]
		public bool castedButBobberStillInAir;

		// Token: 0x04000E8A RID: 3722
		[XmlIgnore]
		public bool gotTroutDerbyTag;

		// Token: 0x04000E8B RID: 3723
		protected Color? lastWaterColor;

		// Token: 0x04000E8C RID: 3724
		[XmlIgnore]
		protected bool _hasPlayerAdjustedBobber;

		// Token: 0x04000E8D RID: 3725
		[XmlIgnore]
		public bool lastCatchWasJunk;

		// Token: 0x04000E8E RID: 3726
		[XmlIgnore]
		public bool goldenTreasure;

		// Token: 0x04000E8F RID: 3727
		[XmlIgnore]
		public bool doneWithAnimation;

		// Token: 0x04000E90 RID: 3728
		[XmlIgnore]
		public bool pullingOutOfWater;

		// Token: 0x04000E91 RID: 3729
		[XmlIgnore]
		public bool isReeling;

		// Token: 0x04000E92 RID: 3730
		[XmlIgnore]
		public bool hasDoneFucntionYet;

		// Token: 0x04000E93 RID: 3731
		[XmlIgnore]
		public bool fishCaught;

		// Token: 0x04000E94 RID: 3732
		[XmlIgnore]
		public bool recordSize;

		// Token: 0x04000E95 RID: 3733
		[XmlIgnore]
		public bool treasureCaught;

		// Token: 0x04000E96 RID: 3734
		[XmlIgnore]
		public bool showingTreasure;

		// Token: 0x04000E97 RID: 3735
		[XmlIgnore]
		public bool hadBobber;

		// Token: 0x04000E98 RID: 3736
		[XmlIgnore]
		public bool bossFish;

		// Token: 0x04000E99 RID: 3737
		[XmlIgnore]
		public bool fromFishPond;

		// Token: 0x04000E9A RID: 3738
		[XmlIgnore]
		public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

		// Token: 0x04000E9B RID: 3739
		[XmlIgnore]
		public SparklingText sparklingText;

		// Token: 0x04000E9C RID: 3740
		[XmlIgnore]
		public int fishSize;

		// Token: 0x04000E9D RID: 3741
		[XmlIgnore]
		public int fishQuality;

		// Token: 0x04000E9E RID: 3742
		[XmlIgnore]
		public int clearWaterDistance;

		// Token: 0x04000E9F RID: 3743
		[XmlIgnore]
		public int originalFacingDirection;

		// Token: 0x04000EA0 RID: 3744
		[XmlIgnore]
		public int numberOfFishCaught = 1;

		// Token: 0x04000EA1 RID: 3745
		[XmlIgnore]
		public ItemMetadata whichFish;

		// Token: 0x04000EA2 RID: 3746
		[XmlIgnore]
		public string setFlagOnCatch;

		// Token: 0x04000EA3 RID: 3747
		[XmlIgnore]
		public int recastTimerMs;

		// Token: 0x04000EA4 RID: 3748
		protected const int RECAST_DELAY_MS = 200;

		// Token: 0x04000EA5 RID: 3749
		[XmlIgnore]
		private readonly NetEventBinary pullFishFromWaterEvent = new NetEventBinary();

		// Token: 0x04000EA6 RID: 3750
		[XmlIgnore]
		private readonly NetEvent1Field<bool, NetBool> doneFishingEvent = new NetEvent1Field<bool, NetBool>();

		// Token: 0x04000EA7 RID: 3751
		[XmlIgnore]
		private readonly NetEvent0 startCastingEvent = new NetEvent0(false);

		// Token: 0x04000EA8 RID: 3752
		[XmlIgnore]
		private readonly NetEvent0 castingEndEnableMovementEvent = new NetEvent0(false);

		// Token: 0x04000EA9 RID: 3753
		[XmlIgnore]
		private readonly NetEvent0 putAwayEvent = new NetEvent0(false);

		// Token: 0x04000EAA RID: 3754
		[XmlIgnore]
		private readonly NetEvent0 beginReelingEvent = new NetEvent0(false);

		// Token: 0x04000EAB RID: 3755
		public static ICue chargeSound;

		// Token: 0x04000EAC RID: 3756
		public static ICue reelSound;

		// Token: 0x04000EAD RID: 3757
		private int randomBobberStyle = -1;

		// Token: 0x04000EAE RID: 3758
		private bool usedGamePadToCast;
	}
}

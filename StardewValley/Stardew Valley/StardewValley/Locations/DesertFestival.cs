using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.GameData.Shops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002CD RID: 717
	public class DesertFestival : Desert
	{
		// Token: 0x06002EC0 RID: 11968 RVA: 0x00247864 File Offset: 0x00245A64
		public DesertFestival()
		{
			this.forceLoadPathLayerLights = true;
		}

		// Token: 0x06002EC1 RID: 11969 RVA: 0x00247D54 File Offset: 0x00245F54
		public DesertFestival(string mapPath, string name) : base(mapPath, name)
		{
			this.forceLoadPathLayerLights = true;
		}

		// Token: 0x06002EC2 RID: 11970 RVA: 0x00248244 File Offset: 0x00246444
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this._revealCactusEvent, "_revealCactusEvent").AddField(this._hideCactusEvent, "_hideCactusEvent").AddField(this.netRacers, "netRacers").AddField(this.announceRaceEvent, "announceRaceEvent").AddField(this.sabotages, "sabotages").AddField(this.raceGuesses, "raceGuesses").AddField(this.rewardsToCollect, "rewardsToCollect").AddField(this.specialRewardsCollected, "specialRewardsCollected").AddField(this.nextRaceGuesses, "nextRaceGuesses").AddField(this.lastRaceWinner, "lastRaceWinner").AddField(this.currentRaceState, "currentRaceState");
			this._revealCactusEvent.onEvent += this.CactusGuyRevealCactus;
			this._hideCactusEvent.onEvent += this.CactusGuyHideCactus;
			this.announceRaceEvent.onEvent += this.AnnounceRace;
		}

		// Token: 0x06002EC3 RID: 11971 RVA: 0x00248354 File Offset: 0x00246554
		public static void SetupMerchantSchedule(NPC character, int shop_index)
		{
			StringBuilder schedule = new StringBuilder();
			if (shop_index == 0)
			{
				schedule.Append("/a1130 Desert 15 40 2");
			}
			else
			{
				schedule.Append("/a1140 Desert 26 40 2");
			}
			schedule.Append("/2400 bed");
			schedule.Remove(0, 1);
			GameLocation defaultMap = Game1.getLocationFromName(character.DefaultMap);
			if (defaultMap != null)
			{
				Game1.warpCharacter(character, defaultMap, new Vector2((float)((int)(character.DefaultPosition.X / 64f)), (float)((int)(character.DefaultPosition.Y / 64f))));
			}
			character.islandScheduleName.Value = "festival_vendor";
			character.TryLoadSchedule("desertFestival", schedule.ToString());
			character.performSpecialScheduleChanges();
		}

		// Token: 0x06002EC4 RID: 11972 RVA: 0x00248404 File Offset: 0x00246604
		public override void OnCamel()
		{
			Game1.playSound("camel", null);
			this.ShowCamelAnimation();
			Game1.player.faceDirection(0);
			Game1.haltAfterCheck = false;
		}

		// Token: 0x06002EC5 RID: 11973 RVA: 0x0024843C File Offset: 0x0024663C
		public override void ShowCamelAnimation()
		{
			this.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
				sourceRect = new Microsoft.Xna.Framework.Rectangle(273, 524, 65, 49),
				sourceRectStartingPos = new Vector2(273f, 524f),
				animationLength = 1,
				totalNumberOfLoops = 1,
				interval = 300f,
				scale = 4f,
				position = new Vector2(536f, 340f) * 4f,
				layerDepth = 0.1332f,
				id = 999
			});
		}

		// Token: 0x06002EC6 RID: 11974 RVA: 0x002484F5 File Offset: 0x002466F5
		public override void checkForMusic(GameTime time)
		{
			Game1.changeMusicTrack(this.GetFestivalMusic(), true, MusicContext.Default);
		}

		// Token: 0x06002EC7 RID: 11975 RVA: 0x00248504 File Offset: 0x00246704
		public virtual string GetFestivalMusic()
		{
			if (Utility.IsPassiveFestivalOpen("DesertFestival"))
			{
				return "event2";
			}
			return "summer_day_ambient";
		}

		// Token: 0x06002EC8 RID: 11976 RVA: 0x0024851D File Offset: 0x0024671D
		public override string GetLocationSpecificMusic()
		{
			return this.GetFestivalMusic();
		}

		// Token: 0x06002EC9 RID: 11977 RVA: 0x00248528 File Offset: 0x00246728
		public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random r = Utility.CreateDaySaveRandom((double)(xLocation * 2000), (double)yLocation, 0.0);
			Game1.createMultipleObjectDebris("CalicoEgg", xLocation, yLocation, r.Next(3, 7), who.UniqueMultiplayerID, this);
			base.digUpArtifactSpot(xLocation, yLocation, who);
		}

		// Token: 0x06002ECA RID: 11978 RVA: 0x00248574 File Offset: 0x00246774
		public virtual void CollectRacePrizes()
		{
			List<Item> rewards = new List<Item>();
			bool collectedSpecialReward;
			if (this.specialRewardsCollected.TryGetValue(Game1.player.UniqueMultiplayerID, out collectedSpecialReward) && !collectedSpecialReward)
			{
				this.specialRewardsCollected[Game1.player.UniqueMultiplayerID] = true;
				rewards.Add(ItemRegistry.Create("CalicoEgg", 100, 0, false));
			}
			for (int i = 0; i < this.rewardsToCollect[Game1.player.UniqueMultiplayerID]; i++)
			{
				rewards.Add(ItemRegistry.Create("CalicoEgg", 20, 0, false));
			}
			this.rewardsToCollect[Game1.player.UniqueMultiplayerID] = 0;
			Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, "Rewards", null, false, true, false, false, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
		}

		// Token: 0x06002ECB RID: 11979 RVA: 0x00248638 File Offset: 0x00246838
		public override void performTouchAction(string full_action_string, Vector2 player_standing_position)
		{
			if (Game1.eventUp)
			{
				return;
			}
			if (full_action_string.Split(' ', StringSplitOptions.None)[0] == "DesertMakeover")
			{
				if (Game1.player.controller == null)
				{
					bool fail = false;
					string failMessageKey = null;
					NPC stylist = this.GetStylist();
					if (!fail && stylist == null)
					{
						stylist = null;
						failMessageKey = "Strings\\1_6_Strings:MakeOver_NoStylist";
						fail = true;
					}
					if (!fail && Game1.player.activeDialogueEvents.ContainsKey("DesertMakeover"))
					{
						failMessageKey = "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_AlreadyStyled";
						fail = true;
					}
					int required_space = 0;
					if (Game1.player.hat.Value != null)
					{
						required_space++;
					}
					if (Game1.player.shirtItem.Value != null)
					{
						required_space++;
					}
					if (Game1.player.pantsItem.Value != null)
					{
						required_space++;
					}
					if (!fail && Game1.player.freeSpotsInInventory() < required_space)
					{
						failMessageKey = "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_InventoryFull";
						fail = true;
					}
					if (fail)
					{
						Game1.freezeControls = true;
						Game1.displayHUD = false;
						int end_direction = 2;
						if (stylist != null)
						{
							end_direction = 3;
						}
						Game1.player.controller = new PathFindController(Game1.player, this, new Point(26, 52), end_direction, delegate(Character character, GameLocation location)
						{
							Game1.freezeControls = false;
							Game1.displayHUD = true;
							if (stylist != null)
							{
								stylist.faceTowardFarmerForPeriod(1000, 2, false, Game1.player);
								if (failMessageKey != null)
								{
									Game1.DrawDialogue(stylist, failMessageKey);
									return;
								}
							}
							else if (failMessageKey != null)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString(failMessageKey));
							}
						});
						return;
					}
					Game1.player.activeDialogueEvents["DesertMakeover"] = 0;
					Game1.freezeControls = true;
					Game1.displayHUD = false;
					Game1.player.controller = new PathFindController(Game1.player, this, new Point(27, 50), 0);
					Game1.globalFadeToBlack(delegate
					{
						Game1.freezeControls = false;
						Game1.forceSnapOnNextViewportUpdate = true;
						Event makeover_event = new Event(this.GetMakeoverEvent(), null);
						Event @event = makeover_event;
						@event.onEventFinished = (Action)Delegate.Combine(@event.onEventFinished, new Action(this.ReceiveMakeOver));
						this.startEvent(makeover_event);
						Game1.globalFadeToClear(null, 0.02f);
					}, 0.02f);
					return;
				}
			}
			else
			{
				base.performTouchAction(full_action_string, player_standing_position);
			}
		}

		// Token: 0x06002ECC RID: 11980 RVA: 0x00248804 File Offset: 0x00246A04
		public virtual string GetMakeoverEvent()
		{
			NPC stylist = this.GetStylist();
			Random r = Utility.CreateDaySaveRandom((double)Game1.year, 0.0, 0.0);
			StringBuilder sb = new StringBuilder();
			sb.Append("continue/26 51/farmer 27 50 2 ");
			foreach (NPC npc in this.characters)
			{
				if (!(npc.Name == stylist.Name) && !(npc.Name == "Sandy"))
				{
					StringBuilder stringBuilder = sb;
					string[] array = new string[8];
					array[0] = npc.Name;
					array[1] = " ";
					int num = 2;
					Vector2 tile = npc.Tile;
					array[num] = tile.X.ToString();
					array[3] = " ";
					int num2 = 4;
					tile = npc.Tile;
					array[num2] = tile.Y.ToString();
					array[5] = " ";
					array[6] = npc.FacingDirection.ToString();
					array[7] = " ";
					stringBuilder.Append(string.Concat(array));
				}
			}
			if (stylist.Name == "Emily")
			{
				sb.Append("Emily 25 52 2 Sandy 22 52 2/skippable/pause 1200/speak Emily \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_1"));
				sb.Append("\"/pause 100/");
				switch (r.Next(0, 3))
				{
				case 0:
					sb.Append("animate Emily false true 200 39 39/");
					break;
				case 1:
					sb.Append("animate Emily false true 300 16 17 18 19 20 21 22 23/");
					break;
				case 2:
					sb.Append("animate Emily false true 300 31 48 49/");
					break;
				}
				sb.Append("pause 1000/faceDirection Sandy 1 true/pause 2000/textAboveHead Emily \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_2"));
				sb.Append("\"/pause 3000/stopAnimation Emily 2/playSound dwop/shake Emily 100/jump Emily 4/pause 300/speak Emily \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_3"));
				sb.Append("\"/pause 100/advancedMove Emily false 1 0 0 -1 0 -1 0 -1 1 100/pause 100/");
				sb.Append("advancedMove Sandy false 1 0 1 0 1 0 1 0 2 100/pause 3000/playSound openChest/pause 1000/");
				List<string> reactions = new List<string>
				{
					string.Format("playSound dustMeep/pause 300/playSound dustMeep/pause 300/playSound dustMeep/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction1")),
					string.Format("playSound rooster/playSound dwop/shake Sandy 400/jump Sandy 4/pause 500/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction2")),
					string.Format("playSound slimeHit/pause 300/playSound slimeHit/pause 600/playSound slimedead/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction3")),
					string.Format("textAboveHead Emily \"{0}\"/playSound trashcanlid/pause 1000/playSound trashcan/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction4")),
					string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound cast/pause 500/playSound axe/pause 200/playSound ow/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction5")),
					string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound eat/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction6")),
					string.Format("textAboveHead Emily \"{0}\"/playSound scissors/pause 300/playSound scissors/pause 300/playSound scissors/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction7")),
					string.Format("textAboveHead Emily \"{0}\"/pause 500/playSound trashbear/pause 300/playSound trashbear/pause 300/playSound trashbear/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction8")),
					string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound fishingRodBend/pause 500/playSound fishingRodBend/pause 1000/playSound fishingRodBend/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction9"))
				};
				Utility.Shuffle<string>(r, reactions);
				for (int i = 0; i < 3; i++)
				{
					sb.Append("pause 500/");
					sb.Append(reactions[i]);
					sb.Append("pause 1500/");
				}
				sb.Append("pause 500/playSound money/textAboveHead Emily \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_4"));
				sb.Append("\"/playSound dwop/shake Sandy 400/jump Sandy 4/pause 750/advancedMove Sandy false -1 0 -1 0 -1 0 -1 0 1 100/pause 2000/advancedMove Emily false 0 1 0 1 0 1 2 100/pause 2000/speak Emily \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_5"));
			}
			else
			{
				sb.Append("Sandy 22 52 2/skippable/pause 2000/textAboveHead Sandy \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_1"));
				sb.Append("\"/");
				sb.Append("pause 1000/playSound dwop/shake Sandy 400/jump Sandy 4/textAboveHead Sandy \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_2"));
				sb.Append("\"/");
				sb.Append("pause 200/advancedMove Sandy false 1 0 1 0 1 0 1 0 4 100/");
				sb.Append("pause 2500/speak Sandy \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_3"));
				sb.Append("\"/");
				sb.Append("pause 500/advancedMove Sandy false 0 -1 0 -1 0 -1/pause 3000/playSound openChest/pause 1000/");
				sb.Append(string.Format("textAboveHead Sandy \"{0}\"/pause 1000/playSound fishingRodBend/pause 500/playSound fishingRodBend/pause 1000/playSound fishingRodBend/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_4")));
				sb.Append("pause 1500/");
				sb.Append("pause 500/playSound money/textAboveHead Sandy \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_5"));
				sb.Append("\"/pause 200/advancedMove Sandy false 0 1 0 1 0 1 2 100/pause 2000/speak Sandy \"");
				sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_6"));
			}
			sb.Append("\"/pause 500/end");
			return sb.ToString();
		}

		// Token: 0x06002ECD RID: 11981 RVA: 0x00248CE8 File Offset: 0x00246EE8
		private void ReceiveMakeOver()
		{
			this.ReceiveMakeOver(-1);
		}

		// Token: 0x06002ECE RID: 11982 RVA: 0x00248CF4 File Offset: 0x00246EF4
		public virtual void ReceiveMakeOver(int randomSeedOverride = -1)
		{
			Random r = (randomSeedOverride == -1) ? Utility.CreateDaySaveRandom((double)Game1.year, 0.0, 0.0) : Utility.CreateRandom((double)randomSeedOverride, 0.0, 0.0, 0.0, 0.0);
			if (randomSeedOverride == -1 && r.NextDouble() < 0.75)
			{
				r = Utility.CreateDaySaveRandom((double)Game1.year, (double)((int)Game1.player.uniqueMultiplayerID.Value), 0.0);
			}
			List<MakeoverOutfit> makeoverOutfits = DataLoader.MakeoverOutfits(Game1.content);
			if (makeoverOutfits != null)
			{
				List<MakeoverOutfit> valid_outfits = new List<MakeoverOutfit>(makeoverOutfits);
				for (int i = 0; i < valid_outfits.Count; i++)
				{
					MakeoverOutfit outfit = valid_outfits[i];
					if (outfit.Gender != null && outfit.Gender.Value != Game1.player.Gender)
					{
						valid_outfits.RemoveAt(i);
						i--;
					}
					else
					{
						bool match = false;
						foreach (MakeoverItem outfitPart in outfit.OutfitParts)
						{
							if (outfitPart.MatchesGender(Game1.player.Gender))
							{
								ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(outfitPart.ItemId);
								Hat value = Game1.player.hat.Value;
								bool flag;
								if (!(((value != null) ? value.QualifiedItemId : null) == itemData.QualifiedItemId))
								{
									Clothing value2 = Game1.player.shirtItem.Value;
									flag = (((value2 != null) ? value2.QualifiedItemId : null) == itemData.QualifiedItemId);
								}
								else
								{
									flag = true;
								}
								match = flag;
								if (match)
								{
									break;
								}
							}
						}
						if (match)
						{
							valid_outfits.RemoveAt(i);
							i--;
						}
					}
				}
				Farmer player = Game1.player;
				foreach (Item heldItem in new List<Item>
				{
					player.Equip<Clothing>(null, player.shirtItem),
					player.Equip<Clothing>(null, player.pantsItem),
					player.Equip<Hat>(null, player.hat)
				})
				{
					Item clothes = Utility.PerformSpecialItemGrabReplacement(heldItem);
					if (clothes != null && player.addItemToInventory(clothes) != null)
					{
						player.team.returnedDonations.Add(clothes);
						player.team.newLostAndFoundItems.Value = true;
					}
				}
				MakeoverOutfit selectedOutfit = r.ChooseFrom(valid_outfits);
				Random togaRandom = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
				if (Utility.GetDayOfPassiveFestival("DesertFestival") == 2 && togaRandom.NextDouble() < 0.03)
				{
					selectedOutfit = new MakeoverOutfit
					{
						OutfitParts = new List<MakeoverItem>
						{
							new MakeoverItem
							{
								ItemId = "(H)LaurelWreathCrown"
							},
							new MakeoverItem
							{
								ItemId = "(P)3",
								Color = "247 245 205"
							},
							new MakeoverItem
							{
								ItemId = "(S)1199"
							}
						}
					};
				}
				if (((selectedOutfit != null) ? selectedOutfit.OutfitParts : null) != null)
				{
					bool appliedHat = false;
					bool appliedShirt = false;
					bool appliedPants = false;
					foreach (MakeoverItem part in selectedOutfit.OutfitParts)
					{
						if (part.MatchesGender(Game1.player.Gender))
						{
							Item item = ItemRegistry.Create(part.ItemId, 1, 0, false);
							Hat hat = item as Hat;
							if (hat == null)
							{
								Clothing clothing = item as Clothing;
								if (clothing != null)
								{
									Color? color = Utility.StringToColor(part.Color);
									if (color != null)
									{
										clothing.clothesColor.Value = color.Value;
									}
									Clothing.ClothesType value3 = clothing.clothesType.Value;
									if (value3 != Clothing.ClothesType.SHIRT)
									{
										if (value3 == Clothing.ClothesType.PANTS && !appliedPants)
										{
											player.Equip<Clothing>(clothing, player.pantsItem);
											appliedPants = true;
										}
									}
									else if (!appliedShirt)
									{
										player.Equip<Clothing>(clothing, player.shirtItem);
										appliedShirt = true;
									}
								}
							}
							else if (!appliedHat)
							{
								player.Equip<Hat>(hat, player.hat);
								appliedHat = true;
							}
						}
					}
				}
			}
		}

		// Token: 0x06002ECF RID: 11983 RVA: 0x00249164 File Offset: 0x00247364
		public virtual void AfterMakeOver()
		{
			Game1.player.canOnlyWalk = false;
			Game1.freezeControls = false;
			Game1.displayHUD = true;
			NPC stylist = this.GetStylist();
			if (stylist != null)
			{
				Game1.DrawDialogue(stylist, "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_Done");
				stylist.faceTowardFarmerForPeriod(1000, 2, false, Game1.player);
			}
		}

		// Token: 0x06002ED0 RID: 11984 RVA: 0x002491C0 File Offset: 0x002473C0
		public NPC GetStylist()
		{
			NPC stylist = base.getCharacterFromName("Emily");
			if (stylist != null && stylist.TilePoint == new Point(25, 52))
			{
				return stylist;
			}
			stylist = base.getCharacterFromName("Sandy");
			if (stylist != null && stylist.TilePoint == new Point(22, 52))
			{
				NPC emily = base.getCharacterFromName("Emily");
				if (emily != null && emily.islandScheduleName.Value == "festival_vendor")
				{
					return stylist;
				}
			}
			return null;
		}

		// Token: 0x06002ED1 RID: 11985 RVA: 0x00249244 File Offset: 0x00247444
		public static void addCalicoStatueSpeedBuff()
		{
			BuffEffects speedBuff = new BuffEffects();
			speedBuff.Speed.Value = 1f;
			Game1.player.applyBuff(new Buff("CalicoStatueSpeed", "Calico Statue", Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue"), 300000, Game1.buffsIcons, 9, speedBuff, new bool?(false), Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Name_10"), null));
		}

		// Token: 0x06002ED2 RID: 11986 RVA: 0x002492B4 File Offset: 0x002474B4
		public override bool performAction(string action, Farmer who, Location tile_location)
		{
			string festival_id = "DesertFestival";
			DataLoader.Shops(Game1.content);
			if (action != null)
			{
				int length = action.Length;
				switch (length)
				{
				case 9:
					if (!(action == "DesertGil"))
					{
						goto IL_AE5;
					}
					if (Game1.Date == who.lastGotPrizeFromGil.Value)
					{
						if (Utility.GetDayOfPassiveFestival("DesertFestival") == 3)
						{
							Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_NextYear");
						}
						else
						{
							Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_ComeBack");
						}
					}
					else if (Game1.player.team.highestCalicoEggRatingToday.Value == 0)
					{
						Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_NoRating");
					}
					else
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Gil_SubmitRating", Game1.player.team.highestCalicoEggRatingToday.Value + 1), base.createYesNoResponses(), "Gil_EggRating");
					}
					return true;
				case 10:
					if (!(action == "DesertFood"))
					{
						goto IL_AE5;
					}
					Game1.player.faceDirection(0);
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro"), base.createYesNoResponses(), "Cook_Intro");
					goto IL_AE5;
				case 11:
				case 16:
				case 17:
					goto IL_AE5;
				case 12:
				{
					char c = action[6];
					if (c != 'M')
					{
						if (c != 'V')
						{
							goto IL_AE5;
						}
						if (!(action == "DesertVendor"))
						{
							goto IL_AE5;
						}
						Game1.player.faceDirection(0);
						if (!Utility.IsPassiveFestivalOpen(festival_id))
						{
							return false;
						}
						Microsoft.Xna.Framework.Rectangle shop_tile_rect = new Microsoft.Xna.Framework.Rectangle(tile_location.X, tile_location.Y - 1, 1, 1);
						using (List<NPC>.Enumerator enumerator = this.characters.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								NPC npc = enumerator.Current;
								if (shop_tile_rect.Contains(npc.TilePoint) && Utility.TryOpenShopMenu(festival_id + "_" + npc.Name, npc.Name, true))
								{
									return true;
								}
							}
							goto IL_AE5;
						}
						break;
					}
					else
					{
						if (!(action == "DesertMarlon"))
						{
							goto IL_AE5;
						}
						if (!Game1.player.mailReceived.Contains("Desert_Festival_Marlon"))
						{
							Game1.player.mailReceived.Add("Desert_Festival_Marlon");
							Game1.DrawDialogue(Game1.getCharacterFromName("Marlon", true, false), "Strings\\1_6_Strings:Marlon_Intro");
							goto IL_AE5;
						}
						bool order_chosen = false;
						bool order_complete = false;
						if (Game1.player.team.acceptedSpecialOrderTypes.Contains("DesertFestivalMarlon"))
						{
							order_complete = true;
							foreach (SpecialOrder order in Game1.player.team.specialOrders)
							{
								if (order.orderType.Value == "DesertFestivalMarlon")
								{
									order_chosen = true;
									if (order.questState.Value == SpecialOrderStatus.InProgress || order.questState.Value == SpecialOrderStatus.Failed)
									{
										order_complete = false;
										break;
									}
									break;
								}
							}
						}
						if (!order_complete)
						{
							if (order_chosen)
							{
								Game1.DrawDialogue(Game1.getCharacterFromName("Marlon", true, false), "Strings\\1_6_Strings:Marlon_Challenge_Chosen");
							}
							else
							{
								Game1.DrawDialogue(Game1.getCharacterFromName("Marlon", true, false), "Strings\\1_6_Strings:Marlon_" + Game1.random.Next(1, 5).ToString());
							}
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								Game1.activeClickableMenu = new SpecialOrdersBoard("DesertFestivalMarlon");
							}));
							return true;
						}
						if (Utility.GetDayOfPassiveFestival("DesertFestival") < 3)
						{
							Game1.DrawDialogue(Game1.getCharacterFromName("Marlon", true, false), "Strings\\1_6_Strings:Marlon_Challenge_Finished");
							return true;
						}
						Game1.DrawDialogue(Game1.getCharacterFromName("Marlon", true, false), "Strings\\1_6_Strings:Marlon_Challenge_Finished_LastDay");
						return true;
					}
					break;
				}
				case 13:
				{
					char c = action[6];
					if (c != 'E')
					{
						if (c != 'S')
						{
							goto IL_AE5;
						}
						if (!(action == "DesertScholar"))
						{
							goto IL_AE5;
						}
						if (!Utility.IsPassiveFestivalOpen(festival_id))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Closed"));
							return true;
						}
						if (Game1.player.mailReceived.Contains(this.GetScholarMail()))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_DoneThisYear"));
							return true;
						}
						if (this._currentScholarQuestion == -2)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Failed"));
							return true;
						}
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Intro"), base.createYesNoResponses(), "DesertScholar");
						goto IL_AE5;
					}
					else
					{
						if (!(action == "DesertEggShop"))
						{
							goto IL_AE5;
						}
						if (!Utility.IsPassiveFestivalOpen(festival_id))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:EggShop_Closed"));
							goto IL_AE5;
						}
						Utility.TryOpenShopMenu("DesertFestival_EggShop", "Vendor", true);
						goto IL_AE5;
					}
					break;
				}
				case 14:
				{
					char c = action[6];
					if (c != 'R')
					{
						if (c != 'S')
						{
							goto IL_AE5;
						}
						if (!(action == "DesertShadyGuy"))
						{
							goto IL_AE5;
						}
						Game1.player.faceDirection(0);
						if (!Utility.IsPassiveFestivalOpen(festival_id) && Game1.timeOfDay < 1000)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Closed"));
						}
						if (this.currentRaceState.Value >= DesertFestival.RaceState.Go && this.currentRaceState.Value < DesertFestival.RaceState.AnnounceWinner4)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Ongoing"));
						}
						else if (!this.CanMakeAnotherRaceGuess())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Ended"));
						}
						else if (this.sabotages.ContainsKey(Game1.player.UniqueMultiplayerID))
						{
							this.ShowSabotagedRaceText();
						}
						else if (!Game1.player.mailReceived.Contains("Desert_Festival_Shady_Guy"))
						{
							Game1.player.mailReceived.Add("Desert_Festival_Shady_Guy");
							Game1.multipleDialogues(new string[]
							{
								Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro"),
								Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro_2"),
								Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro_3")
							});
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy"), base.createYesNoResponses(), "Shady_Guy");
							}));
						}
						else
						{
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_2nd"), base.createYesNoResponses(), "Shady_Guy");
						}
						return true;
					}
					else
					{
						if (!(action == "DesertRacerMan"))
						{
							goto IL_AE5;
						}
						Game1.player.faceGeneralDirection(new Vector2((float)tile_location.X + 0.5f, (float)tile_location.Y + 0.5f) * 64f, 0, false);
						bool collectedSpecialReward;
						int toCollect;
						int guessedRacer2;
						if (this.specialRewardsCollected.TryGetValue(Game1.player.UniqueMultiplayerID, out collectedSpecialReward) && !collectedSpecialReward)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Collect_Prize_Special"));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(this.CollectRacePrizes));
						}
						else if (this.rewardsToCollect.TryGetValue(Game1.player.UniqueMultiplayerID, out toCollect) && toCollect > 0)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Collect_Prize"));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(this.CollectRacePrizes));
						}
						else if (!Utility.IsPassiveFestivalOpen(festival_id) && Game1.timeOfDay < 1000)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Closed"));
						}
						else if (this.currentRaceState.Value >= DesertFestival.RaceState.Go && this.currentRaceState.Value < DesertFestival.RaceState.AnnounceWinner4)
						{
							int guessedRacer;
							if (this.raceGuesses.TryGetValue(Game1.player.UniqueMultiplayerID, out guessedRacer) && this.currentRaceState.Value == DesertFestival.RaceState.Go)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Already_Made", Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + guessedRacer.ToString())));
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Ongoing"));
							}
						}
						else if (!this.CanMakeAnotherRaceGuess())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Ended"));
						}
						else if (this.nextRaceGuesses.TryGetValue(Game1.player.UniqueMultiplayerID, out guessedRacer2))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Already_Made", Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + guessedRacer2.ToString())));
						}
						else
						{
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Question"), base.createYesNoResponses(), "Race");
						}
						return true;
					}
					break;
				}
				case 15:
					if (!(action == "DesertCactusMan"))
					{
						goto IL_AE5;
					}
					break;
				case 18:
					if (!(action == "DesertFishingBoard"))
					{
						goto IL_AE5;
					}
					if (Game1.Date != who.lastDesertFestivalFishingQuest.Value)
					{
						List<Response> responses = new List<Response>
						{
							new Response("Yes", Game1.content.LoadString("Strings\\1_6_Strings:Accept")),
							new Response("No", Game1.content.LoadString("Strings\\1_6_Strings:Decline"))
						};
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Willy_DesertFishing" + Utility.GetDayOfPassiveFestival("DesertFestival").ToString()), responses.ToArray(), "Fishing_Quest");
						goto IL_AE5;
					}
					goto IL_AE5;
				default:
					if (length != 29)
					{
						goto IL_AE5;
					}
					if (!(action == "DesertFestivalMineExplanation"))
					{
						goto IL_AE5;
					}
					Game1.player.mailReceived.Add("Checked_DF_Mine_Explanation");
					this.checkedMineExplanation = true;
					Game1.multipleDialogues(new string[]
					{
						Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation"),
						Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation_2"),
						Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation_3")
					});
					goto IL_AE5;
				}
				Game1.player.faceDirection(0);
				if (!Utility.IsPassiveFestivalOpen(festival_id))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Closed"));
				}
				else if (Game1.player.isInventoryFull())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
				}
				else if (!Game1.player.mailReceived.Contains(this.GetCactusMail()))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Intro_" + Game1.random.Next(1, 4).ToString()));
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Question"), base.createYesNoResponses(), "CactusMan");
					}));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Collected"));
				}
			}
			IL_AE5:
			return base.performAction(action, who, tile_location);
		}

		// Token: 0x06002ED3 RID: 11987 RVA: 0x00249DD0 File Offset: 0x00247FD0
		public string GetCactusMail()
		{
			return "Y" + Game1.year.ToString() + "_Cactus";
		}

		// Token: 0x06002ED4 RID: 11988 RVA: 0x00249DEB File Offset: 0x00247FEB
		public string GetScholarMail()
		{
			return "Y" + Game1.year.ToString() + "_Scholar";
		}

		// Token: 0x06002ED5 RID: 11989 RVA: 0x00249E08 File Offset: 0x00248008
		public virtual Response[] GetRacerResponses()
		{
			List<Response> responses = new List<Response>();
			foreach (Racer racer in this.netRacers)
			{
				responses.Add(new Response(racer.racerIndex.ToString(), Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + racer.racerIndex.Value.ToString())));
			}
			responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			return responses.ToArray();
		}

		// Token: 0x06002ED6 RID: 11990 RVA: 0x00249EBC File Offset: 0x002480BC
		public virtual void ShowSabotagedRaceText()
		{
			int sabotagedRacer;
			if (!this.sabotages.TryGetValue(Game1.player.UniqueMultiplayerID, out sabotagedRacer))
			{
				return;
			}
			if (this._localSabotageText == -1)
			{
				this._localSabotageText = Game1.random.Next(1, 4);
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Selected_" + this._localSabotageText.ToString(), Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + sabotagedRacer.ToString())));
		}

		// Token: 0x06002ED7 RID: 11991 RVA: 0x00249F40 File Offset: 0x00248140
		private void generateNextScholarQuestion()
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0, 0.0);
			int whichQuestion = r.Next(3);
			whichQuestion += Game1.year;
			whichQuestion %= 3;
			string questionKey = "Scholar_Question_" + this._currentScholarQuestion.ToString() + "_" + whichQuestion.ToString();
			string optionsKey = string.Concat(new string[]
			{
				"Scholar_Question_",
				this._currentScholarQuestion.ToString(),
				"_",
				whichQuestion.ToString(),
				"_Options"
			});
			string answersKey = string.Concat(new string[]
			{
				"Scholar_Question_",
				this._currentScholarQuestion.ToString(),
				"_",
				whichQuestion.ToString(),
				"_Answers"
			});
			string[] options = null;
			int optionIndex = 0;
			try
			{
				options = Game1.content.LoadString("Strings\\1_6_Strings:" + optionsKey).Split(',', StringSplitOptions.None);
				optionIndex = r.Next(options.Length);
			}
			catch (Exception)
			{
			}
			string[] answers = Game1.content.LoadString("Strings\\1_6_Strings:" + answersKey).Split(',', StringSplitOptions.None);
			string question;
			if (options == null)
			{
				question = Game1.content.LoadString("Strings\\1_6_Strings:" + questionKey);
			}
			else
			{
				question = Game1.content.LoadString("Strings\\1_6_Strings:" + questionKey, options[optionIndex]);
			}
			List<Response> choices = new List<Response>();
			if (this._currentScholarQuestion == 2 && whichQuestion == 1)
			{
				choices.Add(new Response("Correct", Game1.stats.StepsTaken.ToString() ?? ""));
				choices.Add(new Response("Wrong", (Game1.stats.StepsTaken * 2U).ToString() ?? ""));
				choices.Add(new Response("Wrong", (Game1.stats.StepsTaken / 2U).ToString() ?? ""));
			}
			else
			{
				choices.Add(new Response("Correct", answers[optionIndex]));
				int index;
				for (index = optionIndex; index == optionIndex; index = r.Next(answers.Length))
				{
				}
				choices.Add(new Response("Wrong", answers[index]));
				int index2 = optionIndex;
				while (index2 == optionIndex || index2 == index)
				{
					index2 = r.Next(answers.Length);
				}
				choices.Add(new Response("Wrong", answers[index2]));
			}
			Utility.Shuffle<Response>(r, choices);
			base.createQuestionDialogue(question, choices.ToArray(), "DesertScholar_Answer_");
			this._currentScholarQuestion++;
		}

		// Token: 0x06002ED8 RID: 11992 RVA: 0x0024A218 File Offset: 0x00248418
		public override void customQuestCompleteBehavior(string questId)
		{
			if (questId == "98765")
			{
				switch (Utility.GetDayOfPassiveFestival("DesertFestival"))
				{
				case 1:
					Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 25, 0, false), null, false);
					break;
				case 2:
					Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 50, 0, false), null, false);
					break;
				case 3:
					Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 30, 0, false), null, false);
					break;
				}
			}
			base.customQuestCompleteBehavior(questId);
		}

		// Token: 0x06002ED9 RID: 11993 RVA: 0x0024A2AC File Offset: 0x002484AC
		public override bool answerDialogueAction(string question_and_answer, string[] question_params)
		{
			if (question_and_answer == null)
			{
				return false;
			}
			if (question_and_answer != null)
			{
				int length = question_and_answer.Length;
				if (length != 8)
				{
					switch (length)
					{
					case 12:
						if (question_and_answer == "CactusMan_No")
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_No"));
							return true;
						}
						break;
					case 13:
					{
						char c = question_and_answer[0];
						if (c != 'C')
						{
							if (c == 'S')
							{
								if (question_and_answer == "Shady_Guy_Yes")
								{
									if (Game1.player.Items.CountId("CalicoEgg") >= 1)
									{
										Game1.player.Items.ReduceId("CalicoEgg", 1);
										base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Question"), this.GetRacerResponses(), "Shady_Guy_Sabotage_");
									}
									else
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_NoEgg"));
									}
								}
							}
						}
						else if (question_and_answer == "CactusMan_Yes")
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Intro"));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								if (Game1.player.isInventoryFull())
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
									return;
								}
								int seed = Utility.CreateRandomSeed((double)Game1.player.UniqueMultiplayerID, (double)Game1.year, 0.0, 0.0, 0.0);
								Game1.player.freezePause = 4000;
								DelayedAction.functionAfterDelay(delegate
								{
									this._revealCactusEvent.Fire(seed);
								}, 1000);
								Game1.afterFadeFunction <>9__10;
								DelayedAction.functionAfterDelay(delegate
								{
									Random r = Utility.CreateRandom((double)seed, 0.0, 0.0, 0.0, 0.0);
									r.Next();
									r.Next();
									r.Next();
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_" + r.Next(1, 6).ToString()));
									Delegate afterDialogues = Game1.afterDialogues;
									Game1.afterFadeFunction b;
									if ((b = <>9__10) == null)
									{
										b = (<>9__10 = delegate()
										{
											RandomizedPlantFurniture cactus = new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed);
											if (Game1.player.addItemToInventoryBool(cactus, false))
											{
												Game1.playSound("coin", null);
												Game1.player.mailReceived.Add(this.GetCactusMail());
											}
											this._hideCactusEvent.Fire(seed);
											Game1.player.freezePause = 100;
										});
									}
									Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(afterDialogues, b);
								}, 3000);
							}));
							return true;
						}
						break;
					}
					case 17:
					{
						char c = question_and_answer[0];
						if (c != 'F')
						{
							if (c == 'G')
							{
								if (question_and_answer == "Gil_EggRating_Yes")
								{
									Game1.player.lastGotPrizeFromGil.Value = Game1.Date;
									Game1.player.freezePause = 1400;
									DelayedAction.playSoundAfterDelay("coin", 500, null, null, -1, false);
									DelayedAction.functionAfterDelay(delegate
									{
										int visibleRating = Game1.player.team.highestCalicoEggRatingToday.Value + 1;
										int eggPrize = 0;
										Item extraPrize = null;
										if (visibleRating >= 1000)
										{
											Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Gil_Rating_1000"));
										}
										else if (visibleRating >= 55)
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_50", new object[]
											{
												visibleRating
											});
											eggPrize = 500;
											extraPrize = new Object("279", 1, false, -1, 0);
										}
										else if (visibleRating >= 25)
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_25", new object[]
											{
												visibleRating
											});
											eggPrize = 200;
											if (!Game1.player.mailReceived.Contains("DF_Gil_Hat"))
											{
												extraPrize = new Hat("GilsHat");
												Game1.player.mailReceived.Add("DF_Gil_Hat");
											}
											else
											{
												extraPrize = new Object("253", 5, false, -1, 0);
											}
										}
										else if (visibleRating >= 20)
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_20to24", new object[]
											{
												visibleRating
											});
											eggPrize = 100;
											extraPrize = new Object("253", 5, false, -1, 0);
										}
										else if (visibleRating >= 15)
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_15to19", new object[]
											{
												visibleRating
											});
											eggPrize = 50;
											extraPrize = new Object("253", 3, false, -1, 0);
										}
										else if (visibleRating >= 10)
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_10to14", new object[]
											{
												visibleRating
											});
											eggPrize = 25;
											extraPrize = new Object("253", 1, false, -1, 0);
										}
										else if (visibleRating >= 5)
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_5to9", new object[]
											{
												visibleRating
											});
											eggPrize = 10;
											extraPrize = new Object("395", 1, false, -1, 0);
										}
										else
										{
											Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild", false).Gil, "Strings\\1_6_Strings:Gil_Rating_1to4", new object[]
											{
												visibleRating
											});
											eggPrize = 1;
											extraPrize = new Object("243", 1, false, -1, 0);
										}
										Game1.afterFadeFunction <>9__7;
										Game1.afterDialogues = delegate()
										{
											Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object("CalicoEgg", eggPrize, false, -1, 0), null, false);
											if (extraPrize != null)
											{
												Game1.afterFadeFunction afterDialogues;
												if ((afterDialogues = <>9__7) == null)
												{
													afterDialogues = (<>9__7 = delegate()
													{
														Game1.player.addItemByMenuIfNecessary(extraPrize, null, false);
													});
												}
												Game1.afterDialogues = afterDialogues;
											}
										};
									}, 1000);
								}
							}
						}
						else if (question_and_answer == "Fishing_Quest_Yes")
						{
							Quest q;
							if (Utility.GetDayOfPassiveFestival("DesertFestival") == 3)
							{
								q = new ItemDeliveryQuest("Willy", "GoldenBobber", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge"), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Description_" + Utility.GetDayOfPassiveFestival("DesertFestival").ToString()), "Strings\\1_6_Strings:Willy_GoldenBobber", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Return_" + Utility.GetDayOfPassiveFestival("DesertFestival").ToString()));
							}
							else
							{
								q = new FishingQuest((Utility.GetDayOfPassiveFestival("DesertFestival") == 1) ? "164" : "165", (Utility.GetDayOfPassiveFestival("DesertFestival") == 1) ? 3 : 1, "Willy", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge"), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Description_" + Utility.GetDayOfPassiveFestival("DesertFestival").ToString()), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Return_" + Utility.GetDayOfPassiveFestival("DesertFestival").ToString()));
							}
							q.daysLeft.Value = 1;
							q.id.Value = "98765";
							Game1.player.questLog.Add(q);
							Game1.player.lastDesertFestivalFishingQuest.Value = Game1.Date;
							return true;
						}
						break;
					}
					case 18:
						if (question_and_answer == "WarperQuestion_Yes")
						{
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
						break;
					}
				}
				else if (question_and_answer == "Race_Yes")
				{
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess"), this.GetRacerResponses(), "Race_Guess_");
					return true;
				}
			}
			if (question_and_answer.StartsWith("Race_Guess_"))
			{
				string s = question_and_answer.Substring("Race_Guess_".Length + 1);
				int guessed_racer = -1;
				if (int.TryParse(s, out guessed_racer))
				{
					if (this.currentRaceState.Value >= DesertFestival.RaceState.Go && this.currentRaceState.Value < DesertFestival.RaceState.AnnounceWinner4)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Late_Guess"));
						return true;
					}
					string racerNameKey = "Strings\\1_6_Strings:Racer_" + guessed_racer.ToString();
					string racer_name = Game1.content.LoadString(racerNameKey);
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Made", racer_name));
					Game1.multiplayer.globalChatInfoMessage("GuessRacer_" + Game1.random.Next(1, 11).ToString(), new string[]
					{
						Game1.player.Name,
						TokenStringBuilder.LocalizedText(racerNameKey)
					});
					this.nextRaceGuesses[Game1.player.UniqueMultiplayerID] = guessed_racer;
				}
				return true;
			}
			if (question_and_answer.StartsWith("Shady_Guy_Sabotage_"))
			{
				string s2 = question_and_answer.Substring("Shady_Guy_Sabotage_".Length + 1);
				int sabotaged_racer = -1;
				if (int.TryParse(s2, out sabotaged_racer))
				{
					if (this.currentRaceState.Value >= DesertFestival.RaceState.Go && this.currentRaceState.Value < DesertFestival.RaceState.AnnounceWinner4)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Late"));
						return true;
					}
					if (!this.sabotages.Any() && Game1.random.NextDouble() < 0.25)
					{
						Game1.multiplayer.globalChatInfoMessage("RaceSabotage_" + Game1.random.Next(1, 6).ToString(), Array.Empty<string>());
					}
					this.sabotages[Game1.player.UniqueMultiplayerID] = sabotaged_racer;
					this._localSabotageText = -1;
					this.ShowSabotagedRaceText();
				}
				return true;
			}
			if (question_and_answer.StartsWith("DesertScholar"))
			{
				if (question_and_answer == "DesertScholar_Yes")
				{
					this._currentScholarQuestion++;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Intro2"));
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
					{
						this.generateNextScholarQuestion();
					}));
				}
				else if (question_and_answer.StartsWith("DesertScholar_Answer_"))
				{
					if (question_and_answer == "DesertScholar_Answer__Wrong")
					{
						Game1.playSound("cancel", null);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Wrong"));
						this._currentScholarQuestion = -2;
					}
					else if (question_and_answer == "DesertScholar_Answer__Correct")
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Correct"));
						Game1.playSound("give_gift", null);
						if (this._currentScholarQuestion == 4)
						{
							Game1.player.mailReceived.Add(this.GetScholarMail());
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Win"));
								Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
								{
									Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 50, 0, false), null, false);
									Game1.playSound("coin", null);
								}));
							}));
						}
						else
						{
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								this.generateNextScholarQuestion();
							}));
						}
					}
				}
			}
			if (question_and_answer.StartsWith("Cook"))
			{
				if (question_and_answer.EndsWith("No"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_No"));
				}
				else if (question_and_answer.StartsWith("Cook_ChoseSauce"))
				{
					Game1.playSound("smallSelect", null);
					this._cookSauce = Convert.ToInt32(question_and_answer[question_and_answer.Length - 1].ToString() ?? "");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_ChoseSauce", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Sauce" + this._cookSauce.ToString())));
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\desert_festival_tilesheet", new Microsoft.Xna.Framework.Rectangle(320, 280, 29, 24), new Vector2(480f, 1372f), false, 0f, Color.White)
						{
							id = 1001,
							animationLength = 2,
							interval = 200f,
							totalNumberOfLoops = 9999,
							scale = 4f,
							layerDepth = 0.1343f
						});
						this.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\desert_festival_tilesheet", new Microsoft.Xna.Framework.Rectangle(378, 280, 29, 24), new Vector2(480f, 1372f), false, 0f, Color.White)
						{
							id = 1002,
							animationLength = 4,
							interval = 100f,
							totalNumberOfLoops = 4,
							delayBeforeAnimationStart = 400,
							scale = 4f,
							layerDepth = 0.1344f
						});
						DelayedAction.playSoundAfterDelay("hammer", 800, this, null, -1, false);
						DelayedAction.playSoundAfterDelay("hammer", 1200, this, null, -1, false);
						DelayedAction.playSoundAfterDelay("hammer", 1600, this, null, -1, false);
						DelayedAction.playSoundAfterDelay("hammer", 2000, this, null, -1, false);
						DelayedAction.playSoundAfterDelay("furnace", 2500, this, null, -1, false);
						for (int k = 0; k < 12; k++)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite(30, new Vector2(460.8f + (float)Game1.random.Next(-10, 10), (float)(1388 + Game1.random.Next(-10, 10))), Color.White, 4, false, 100f, 2, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = 2700 + k * 80,
								motion = new Vector2(-1f + (float)Game1.random.Next(-5, 5) / 10f, -1f + (float)Game1.random.Next(-5, 5) / 10f),
								drawAboveAlwaysFront = true
							});
							this.temporarySprites.Add(new TemporaryAnimatedSprite(30, new Vector2(544f + (float)Game1.random.Next(-10, 10), (float)(1388 + Game1.random.Next(-10, 10))), Color.White, 4, false, 100f, 2, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = 2700 + k * 80,
								motion = new Vector2(1f + (float)Game1.random.Next(-5, 5) / 10f, -1f + (float)Game1.random.Next(-5, 5) / 10f),
								drawAboveAlwaysFront = true
							});
							if (k % 2 == 0)
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), new Vector2(505.6f + (float)Game1.random.Next(-16, 16), 1344f), Game1.random.NextDouble() < 0.5, 0f, Color.Gray)
								{
									delayBeforeAnimationStart = 2700 + k * 80,
									motion = new Vector2(0f, -0.25f),
									animationLength = 8,
									interval = 70f,
									drawAboveAlwaysFront = true
								});
							}
						}
						Game1.player.freezePause = 4805;
						DelayedAction.functionAfterDelay(delegate
						{
							base.removeTemporarySpritesWithID(1001);
							base.removeTemporarySpritesWithID(1002);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Done", Game1.content.LoadString("Strings\\1_6_Strings:Cook_DishNames_" + this._cookIngredient.ToString() + "_" + this._cookSauce.ToString())));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								Object food = new Object();
								food.edibility.Value = Game1.player.maxHealth;
								string nameKey = "Strings\\1_6_Strings:Cook_DishNames_" + this._cookIngredient.ToString() + "_" + this._cookSauce.ToString();
								food.name = Game1.content.LoadString(nameKey);
								food.displayNameFormat = "[LocalizedText " + nameKey + "]";
								BuffEffects effects = new BuffEffects();
								switch (this._cookIngredient)
								{
								case 0:
									effects.Defense.Value = 3f;
									break;
								case 1:
									effects.MiningLevel.Value = 3f;
									break;
								case 2:
									effects.LuckLevel.Value = 3f;
									break;
								case 3:
									effects.Attack.Value = 3f;
									break;
								case 4:
									effects.FishingLevel.Value = 3f;
									break;
								}
								switch (this._cookSauce)
								{
								case 0:
									effects.Defense.Value = 1f;
									break;
								case 1:
									effects.MiningLevel.Value = 1f;
									break;
								case 2:
									effects.LuckLevel.Value = 1f;
									break;
								case 3:
									effects.Attack.Value = 1f;
									break;
								case 4:
									effects.Speed.Value = 1f;
									break;
								}
								food.customBuff = (() => new Buff("DesertFestival", food.Name, food.Name, 600 * Game1.realMilliSecondsPerGameMinute, null, -1, effects, null, null, null));
								int index = this._cookIngredient * 4 + this._cookSauce + ((this._cookSauce > this._cookIngredient) ? -1 : 0);
								Game1.player.tempFoodItemTextureName.Value = "TileSheets\\Objects_2";
								Game1.player.tempFoodItemSourceRect.Value = Utility.getSourceRectWithinRectangularRegion(0, 32, 128, index, 16, 16);
								Game1.player.faceDirection(2);
								Game1.player.eatObject(food, false);
							}));
						}, 4800);
					}));
				}
				else if (question_and_answer.StartsWith("Cook_PickedIngredient"))
				{
					Game1.playSound("smallSelect", null);
					this._cookIngredient = Convert.ToInt32(question_and_answer[question_and_answer.Length - 1].ToString() ?? "");
					List<Response> sauces = new List<Response>();
					for (int i = 0; i < 5; i++)
					{
						if (i != this._cookIngredient || this._cookIngredient == 4)
						{
							sauces.Add(new Response(i.ToString() ?? "", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Sauce" + i.ToString())));
						}
					}
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_ChoseIngredient", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Ingredient" + this._cookIngredient.ToString())), sauces.ToArray(), "Cook_ChoseSauce");
				}
				else if (!(question_and_answer == "Cook_Intro_Yes"))
				{
					if (question_and_answer == "Cook_Intro2_Yes")
					{
						Game1.playSound("smallSelect", null);
						Response[] ingredients = new Response[5];
						for (int j = 0; j < 5; j++)
						{
							ingredients[j] = new Response(j.ToString() ?? "", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Ingredient" + j.ToString()));
						}
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes3"), ingredients, "Cook_PickedIngredient");
					}
				}
				else
				{
					Game1.playSound("smallSelect", null);
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes"));
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
					{
						Game1.playSound("smallSelect", null);
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes2"), base.createYesNoResponses(), "Cook_Intro2");
					}));
				}
			}
			return base.answerDialogueAction(question_and_answer, question_params);
		}

		// Token: 0x06002EDA RID: 11994 RVA: 0x0024ACBA File Offset: 0x00248EBA
		public void CactusGuyHideCactus(int seed)
		{
			if (this._currentlyShownCactusID == seed)
			{
				this._cactusGuyRevealItem = null;
				this._cactusGuyRevealTimer = -1f;
				this._cactusShakeTimer = -1f;
				this._currentlyShownCactusID = -1;
			}
		}

		// Token: 0x06002EDB RID: 11995 RVA: 0x0024ACEC File Offset: 0x00248EEC
		public void CactusGuyRevealCactus(int seed)
		{
			RandomizedPlantFurniture cactus = new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed);
			this._currentlyShownCactusID = seed;
			this._cactusGuyRevealItem = (cactus.getOne() as RandomizedPlantFurniture);
			this._cactusGuyRevealTimer = 0f;
			this._cactusShakeTimer = -1f;
			Random random = Utility.CreateRandom((double)seed, 0.0, 0.0, 0.0, 0.0);
			random.Next();
			random.Next();
			List<string> sounds = new List<string>
			{
				"pig",
				"Duck",
				"dog_bark",
				"cat",
				"camel"
			};
			Game1.playSound("throwDownITem", null);
			DelayedAction.playSoundAfterDelay("thudStep", 500, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("thudStep", 750, null, null, -1, false);
			DelayedAction.playSoundAfterDelay(random.ChooseFrom(sounds), 1000, null, null, -1, false);
			DelayedAction.functionAfterDelay(delegate
			{
				this._cactusShakeTimer = 0.25f;
			}, 1000);
		}

		// Token: 0x06002EDC RID: 11996 RVA: 0x0024AE2B File Offset: 0x0024902B
		public bool CanMakeAnotherRaceGuess()
		{
			return Game1.timeOfDay < 2200 || this.currentRaceState.Value < DesertFestival.RaceState.Go;
		}

		// Token: 0x06002EDD RID: 11997 RVA: 0x0024AE4C File Offset: 0x0024904C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this._cactusShakeTimer > 0f)
			{
				this._cactusShakeTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._cactusShakeTimer <= 0f)
				{
					this._cactusShakeTimer = -1f;
				}
			}
			if (this._raceTextTimer > 0f)
			{
				this._raceTextTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._raceTextTimer < 0f)
				{
					this._raceTextTimer = 0f;
				}
			}
			if (this._cactusGuyRevealTimer >= 0f && this._cactusGuyRevealTimer < 1f)
			{
				this._cactusGuyRevealTimer += (float)time.ElapsedGameTime.TotalSeconds / 0.75f;
				if (this._cactusGuyRevealTimer >= 1f)
				{
					this._cactusGuyRevealTimer = 1f;
				}
			}
			this._revealCactusEvent.Poll();
			this._hideCactusEvent.Poll();
			this.announceRaceEvent.Poll();
			if (Game1.shouldTimePass(false))
			{
				if (Game1.IsMasterGame)
				{
					if (this._raceStateTimer >= 0f)
					{
						this._raceStateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
						if (this._raceStateTimer <= 0f)
						{
							this._raceStateTimer = 0f;
							switch (this.currentRaceState.Value)
							{
							case DesertFestival.RaceState.StartingLine:
								this.announceRaceEvent.Fire("Race_Ready");
								this._raceStateTimer = 3f;
								this.currentRaceState.Value = DesertFestival.RaceState.Ready;
								break;
							case DesertFestival.RaceState.Ready:
								this.currentRaceState.Value = DesertFestival.RaceState.Set;
								this.announceRaceEvent.Fire("Race_Set");
								this._raceStateTimer = 3f;
								break;
							case DesertFestival.RaceState.Set:
								this.currentRaceState.Value = DesertFestival.RaceState.Go;
								this.announceRaceEvent.Fire("Race_Go");
								this.raceGuesses.Clear();
								foreach (KeyValuePair<long, int> kvp in this.nextRaceGuesses.Pairs)
								{
									this.raceGuesses[kvp.Key] = kvp.Value;
								}
								this.nextRaceGuesses.Clear();
								foreach (Racer racer in this.netRacers)
								{
									racer.sabotages.Value = 0;
									using (NetDictionary<long, int, NetInt, SerializableDictionary<long, int>, NetLongDictionary<int, NetInt>>.ValuesCollection.Enumerator enumerator3 = this.sabotages.Values.GetEnumerator())
									{
										while (enumerator3.MoveNext())
										{
											if (enumerator3.Current == racer.racerIndex.Value)
											{
												NetInt netInt = racer.sabotages;
												int value = netInt.Value;
												netInt.Value = value + 1;
											}
										}
									}
									racer.ResetMoveSpeed();
								}
								this.sabotages.Clear();
								this._raceStateTimer = 3f;
								break;
							case DesertFestival.RaceState.AnnounceWinner:
							case DesertFestival.RaceState.AnnounceWinner2:
							case DesertFestival.RaceState.AnnounceWinner3:
							case DesertFestival.RaceState.AnnounceWinner4:
							{
								this._raceStateTimer = 2f;
								switch (this.currentRaceState.Value)
								{
								case DesertFestival.RaceState.AnnounceWinner:
									this.announceRaceEvent.Fire("Race_Comment_" + Game1.random.Next(1, 5).ToString());
									this._raceStateTimer = 4f;
									break;
								case DesertFestival.RaceState.AnnounceWinner2:
									this.announceRaceEvent.Fire("Race_Winner");
									this._raceStateTimer = 2f;
									break;
								case DesertFestival.RaceState.AnnounceWinner3:
									this.announceRaceEvent.Fire("Racer_" + this.lastRaceWinner.Value.ToString());
									this._raceStateTimer = 4f;
									break;
								case DesertFestival.RaceState.AnnounceWinner4:
									this.announceRaceEvent.Fire("RESULT");
									this._raceStateTimer = 2f;
									this.finishedRacers.Clear();
									break;
								}
								NetEnum<DesertFestival.RaceState> netEnum = this.currentRaceState;
								DesertFestival.RaceState value2 = netEnum.Value;
								netEnum.Value = value2 + 1;
								break;
							}
							case DesertFestival.RaceState.RaceEnd:
								if (!this.CanMakeAnotherRaceGuess())
								{
									if (Utility.GetDayOfPassiveFestival("DesertFestival") < 3)
									{
										this.announceRaceEvent.Fire("Race_Close");
									}
									else
									{
										this.announceRaceEvent.Fire("Race_Close_LastDay");
									}
									this.currentRaceState.Value = DesertFestival.RaceState.RacesOver;
								}
								else
								{
									this.currentRaceState.Value = DesertFestival.RaceState.PreRace;
								}
								break;
							}
						}
					}
					if (this.currentRaceState.Value == DesertFestival.RaceState.Go)
					{
						if (this.finishedRacers.Count >= this.racerCount)
						{
							this.currentRaceState.Value = DesertFestival.RaceState.AnnounceWinner;
							this._raceStateTimer = 2f;
						}
						else
						{
							foreach (Racer racer2 in this.netRacers)
							{
								racer2.UpdateRaceProgress(this);
							}
						}
					}
				}
				foreach (Racer racer3 in this.netRacers)
				{
					racer3.Update(this);
				}
			}
			this.festivalChimneyTimer -= (float)time.ElapsedGameTime.Milliseconds;
			if (this.festivalChimneyTimer <= 0f)
			{
				this.AddSmokePuff(new Vector2(7.25f, 16.25f) * 64f);
				this.AddSmokePuff(new Vector2(28.25f, 6f) * 64f);
				this.festivalChimneyTimer = 500f;
			}
			if (Game1.isStartingToGetDarkOut(this) && Game1.outdoorLight.R > 160)
			{
				Game1.outdoorLight.R = 160;
				Game1.outdoorLight.G = 160;
				Game1.outdoorLight.B = 0;
			}
			base.UpdateWhenCurrentLocation(time);
		}

		// Token: 0x06002EDE RID: 11998 RVA: 0x0024B478 File Offset: 0x00249678
		public void OnRaceWon(int winner)
		{
			this.lastRaceWinner.Value = winner;
			if (this.raceGuesses.FieldDict.Count > 0)
			{
				List<string> winning_farmers = new List<string>();
				foreach (KeyValuePair<long, int> kvp in this.raceGuesses.Pairs)
				{
					if (kvp.Value == winner)
					{
						if (winner == 3 && !this.specialRewardsCollected.ContainsKey(kvp.Key))
						{
							this.specialRewardsCollected[kvp.Key] = false;
						}
						else
						{
							if (!this.rewardsToCollect.ContainsKey(kvp.Key))
							{
								this.rewardsToCollect[kvp.Key] = 0;
							}
							NetLongDictionary<int, NetInt> netLongDictionary = this.rewardsToCollect;
							long key = kvp.Key;
							int num = netLongDictionary[key];
							netLongDictionary[key] = num + 1;
							Farmer winner_farmer = Game1.GetPlayer(kvp.Key, false);
							if (winner_farmer != null)
							{
								winning_farmers.Add(winner_farmer.Name);
							}
						}
					}
				}
				string tokenizedWinnerName = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:Racer_" + winner.ToString());
				switch (winning_farmers.Count)
				{
				case 0:
					Game1.multiplayer.globalChatInfoMessage("RaceWinners_Zero", new string[]
					{
						tokenizedWinnerName
					});
					return;
				case 1:
					Game1.multiplayer.globalChatInfoMessage("RaceWinners_One", new string[]
					{
						tokenizedWinnerName,
						winning_farmers[0]
					});
					return;
				case 2:
					Game1.multiplayer.globalChatInfoMessage("RaceWinners_Two", new string[]
					{
						tokenizedWinnerName,
						winning_farmers[0],
						winning_farmers[1]
					});
					return;
				default:
					Game1.multiplayer.globalChatInfoMessage("RaceWinners_Many", new string[]
					{
						tokenizedWinnerName
					});
					for (int i = 0; i < winning_farmers.Count; i++)
					{
						if (i < winning_farmers.Count - 1)
						{
							Game1.multiplayer.globalChatInfoMessage("RaceWinners_List", new string[]
							{
								winning_farmers[i]
							});
						}
						else
						{
							Game1.multiplayer.globalChatInfoMessage("RaceWinners_Final", new string[]
							{
								winning_farmers[i]
							});
						}
					}
					break;
				}
			}
		}

		// Token: 0x06002EDF RID: 11999 RVA: 0x0024B6BC File Offset: 0x002498BC
		public void AddSmokePuff(Vector2 v)
		{
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, false, 0.002f, Color.Gray)
			{
				alpha = 0.75f,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(0.002f, 0f),
				interval = 99999f,
				layerDepth = 1f,
				scale = 2f,
				scaleChange = 0.02f,
				drawAboveAlwaysFront = true,
				rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
			});
		}

		// Token: 0x06002EE0 RID: 12000 RVA: 0x0024B785 File Offset: 0x00249985
		public static void CleanupFestival()
		{
			Game1.player.team.itemsToRemoveOvernight.Add("CalicoEgg");
			SpecialOrder.RemoveAllSpecialOrders("DesertFestivalMarlon");
		}

		// Token: 0x06002EE1 RID: 12001 RVA: 0x0024B7AC File Offset: 0x002499AC
		public override void draw(SpriteBatch spriteBatch)
		{
			if (this._cactusGuyRevealTimer > 0f && this._cactusGuyRevealItem != null)
			{
				Vector2 start = new Vector2(29f, 66.5f) * 64f;
				Vector2 end = new Vector2(27.5f, 66.5f) * 64f;
				float bounce_point = 0.6f;
				float height;
				if (this._cactusGuyRevealTimer < bounce_point)
				{
					height = (float)Math.Sin((double)(this._cactusGuyRevealTimer / bounce_point) * 3.141592653589793) * 16f * 4f;
				}
				else
				{
					height = (float)Math.Sin((double)((this._cactusGuyRevealTimer - bounce_point) / (1f - bounce_point)) * 3.141592653589793) * 8f * 4f;
				}
				Vector2 position = new Vector2(Utility.Lerp(start.X, end.X, this._cactusGuyRevealTimer), Utility.Lerp(start.Y, end.Y, this._cactusGuyRevealTimer));
				float sort_y = position.Y;
				if (this._cactusShakeTimer > 0f)
				{
					position.X += (float)Game1.random.Next(-1, 2);
					position.Y += (float)Game1.random.Next(-1, 2);
				}
				this._cactusGuyRevealItem.DrawFurniture(spriteBatch, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, -height)), 1f, new Vector2(8f, 16f), 4f, sort_y / 10000f);
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position), null, Color.White * 0.75f, 0f, new Vector2((float)(Game1.shadowTexture.Width / 2), (float)(Game1.shadowTexture.Height / 2)), new Vector2(4f, 4f), SpriteEffects.None, sort_y / 10000f - 1E-07f);
			}
			foreach (Racer racer in this._localRacers)
			{
				if (!racer.drawAboveMap.Value)
				{
					racer.Draw(spriteBatch);
				}
			}
			if (Game1.Date != Game1.player.lastDesertFestivalFishingQuest.Value)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(984f, 842f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f), SpriteEffects.None, 1f);
			}
			if (!this.checkedMineExplanation)
			{
				float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(609.6f, 320f + yOffset2)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset2 / 16f), SpriteEffects.None, 1f);
			}
			if (Game1.timeOfDay < 1000)
			{
				spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(45f, 14f) * 64f + new Vector2(7f, 9f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(239, 317, 16, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.096f);
			}
			base.draw(spriteBatch);
		}

		// Token: 0x06002EE2 RID: 12002 RVA: 0x0024BC0C File Offset: 0x00249E0C
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "desert-festival");
			if (tileIndexAt - 792 <= 1)
			{
				base.playSound("pig", null, null, SoundContext.Default);
				return true;
			}
			if (tileIndexAt - 796 <= 1)
			{
				Utility.TryOpenShopMenu("Traveler", this, null, null, false, true, null);
				return true;
			}
			if (tileIndexAt != 1073)
			{
				return base.checkAction(tileLocation, viewport, who);
			}
			base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), base.createYesNoResponses(), "WarperQuestion");
			return true;
		}

		// Token: 0x06002EE3 RID: 12003 RVA: 0x0024BCB8 File Offset: 0x00249EB8
		public override void drawOverlays(SpriteBatch b)
		{
			SpecialCurrencyDisplay.Draw(b, new Vector2(16f, 0f), this.eggMoneyDial, Game1.player.Items.CountId("CalicoEgg"), Game1.mouseCursors_1_6, new Microsoft.Xna.Framework.Rectangle(0, 21, 0, 0));
			base.drawOverlays(b);
		}

		// Token: 0x06002EE4 RID: 12004 RVA: 0x0024BD0C File Offset: 0x00249F0C
		public override void drawAboveAlwaysFrontLayer(SpriteBatch sb)
		{
			base.drawAboveAlwaysFrontLayer(sb);
			this._localRacers.Sort((Racer a, Racer b) => a.position.Y.CompareTo(b.position.Y));
			foreach (Racer racer in this._localRacers)
			{
				if (racer.drawAboveMap.Value)
				{
					racer.Draw(sb);
				}
			}
			if (this._raceTextTimer > 0f && this._raceText != null)
			{
				Vector2 local = Game1.GlobalToLocal(new Vector2(44.5f, 39.5f) * 64f);
				if (this._raceTextShake)
				{
					local += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				}
				float alpha = Utility.Clamp(this._raceTextTimer / 0.25f, 0f, 1f);
				SpriteText.drawStringWithScrollCenteredAt(sb, this._raceText, (int)local.X, (int)local.Y - 192, "", alpha, null, 1, local.Y / 10000f + 0.001f, false);
			}
		}

		// Token: 0x06002EE5 RID: 12005 RVA: 0x0024BE68 File Offset: 0x0024A068
		public Vector3 GetTrackPosition(int track_index, float horizontal_position)
		{
			Vector2 inner_edge = new Vector2(this.raceTrack[track_index][0].X + 0.5f, this.raceTrack[track_index][0].Y + 0.5f);
			Vector2 outer_edge = new Vector2(this.raceTrack[track_index][1].X + 0.5f, this.raceTrack[track_index][1].Y + 0.5f);
			inner_edge == outer_edge;
			Vector2 delta = outer_edge - inner_edge;
			delta.Normalize();
			inner_edge *= 64f;
			outer_edge *= 64f;
			inner_edge -= delta * 64f / 4f;
			outer_edge += delta * 64f / 4f;
			return new Vector3(Utility.Lerp(inner_edge.X, outer_edge.X, horizontal_position), Utility.Lerp(inner_edge.Y, outer_edge.Y, horizontal_position), this.raceTrack[track_index][0].Z);
		}

		// Token: 0x06002EE6 RID: 12006 RVA: 0x0024BF8C File Offset: 0x0024A18C
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			string festival_id = "DesertFestival";
			base.performTenMinuteUpdate(timeOfDay);
			if (Game1.IsMasterGame && Utility.IsPassiveFestivalOpen(festival_id) && timeOfDay % 200 == 0 && timeOfDay < 2400 && this.currentRaceState.Value == DesertFestival.RaceState.PreRace)
			{
				this.announceRaceEvent.Fire("Race_Begin");
				this.currentRaceState.Value = DesertFestival.RaceState.StartingLine;
				if (this.nextRaceGuesses.FieldDict.Count > 0)
				{
					Game1.multiplayer.globalChatInfoMessage("RaceStarting", Array.Empty<string>());
				}
				this._raceStateTimer = 5f;
			}
		}

		// Token: 0x06002EE7 RID: 12007 RVA: 0x0024C024 File Offset: 0x0024A224
		public virtual void AnnounceRace(string text)
		{
			this._raceTextShake = false;
			this._raceTextTimer = 2f;
			if (text == "Race_Go" || text == "Race_Finish" || text.StartsWith("Racer_"))
			{
				this._raceTextShake = true;
			}
			if (text.StartsWith("Race_Close"))
			{
				this._raceTextTimer = 4f;
			}
			if (text == "RESULT")
			{
				this._raceTextTimer = 4f;
				int guessedRacer;
				if (this.raceGuesses.TryGetValue(Game1.player.UniqueMultiplayerID, out guessedRacer))
				{
					if (this.lastRaceWinner.Value == guessedRacer)
					{
						this._raceText = Game1.content.LoadString("Strings\\1_6_Strings:Race_Win");
						return;
					}
					this._raceText = Game1.content.LoadString("Strings\\1_6_Strings:Race_Lose");
					return;
				}
			}
			else
			{
				this._raceText = Game1.content.LoadString("Strings\\1_6_Strings:" + text);
				if (text.StartsWith("Racer_"))
				{
					this._raceText += "!";
				}
			}
		}

		// Token: 0x06002EE8 RID: 12008 RVA: 0x0024C134 File Offset: 0x0024A334
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			Game1.player.team.calicoEggSkullCavernRating.Value = 0;
			Game1.player.team.highestCalicoEggRatingToday.Value = 0;
			Game1.player.team.calicoStatueEffects.Clear();
			MineShaft.totalCalicoStatuesActivatedToday = 0;
			this.finishedRacers.Clear();
			this.lastRaceWinner.Value = -1;
			this.rewardsToCollect.Clear();
			this.specialRewardsCollected.Clear();
			this.raceGuesses.Clear();
			this.nextRaceGuesses.Clear();
			this.sabotages.Clear();
			this.currentRaceState.Value = DesertFestival.RaceState.PreRace;
			this._raceStateTimer = 0f;
			this._currentScholarQuestion = -1;
		}

		// Token: 0x06002EE9 RID: 12009 RVA: 0x0024C1F8 File Offset: 0x0024A3F8
		public override void cleanupBeforePlayerExit()
		{
			this._localRacers.Clear();
			this._cactusGuyRevealTimer = -1f;
			this._cactusGuyRevealItem = null;
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06002EEA RID: 12010 RVA: 0x0024C220 File Offset: 0x0024A420
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.player.mailReceived.Contains("Checked_DF_Mine_Explanation"))
			{
				this.checkedMineExplanation = true;
			}
			this._localRacers.Clear();
			this._localRacers.AddRange(this.netRacers);
			if (this.critters == null)
			{
				this.critters = new List<Critter>();
			}
			for (int i = 0; i < 8; i++)
			{
				this.critters.Add(new Butterfly(this, base.getRandomTile(null), false, true, -1, false));
			}
			this.eggMoneyDial = new MoneyDial(4, false);
			this.eggMoneyDial.currentValue = Game1.player.Items.CountId("CalicoEgg");
		}

		// Token: 0x06002EEB RID: 12011 RVA: 0x0024C2D4 File Offset: 0x0024A4D4
		public static void SetupFestivalDay()
		{
			string festival_id = "DesertFestival";
			int day_number = Utility.GetDayOfPassiveFestival(festival_id);
			Dictionary<string, ShopData> store_data_sheet = DataLoader.Shops(Game1.content);
			List<NPC> characters = Utility.getAllVillagers();
			characters.RemoveAll((NPC character) => !store_data_sheet.ContainsKey(festival_id + "_" + character.Name) || (character.Name == "Leo" && !Game1.MasterPlayer.mailReceived.Contains("leoMoved")) || character.getMasterScheduleRawData().ContainsKey(festival_id + "_" + day_number.ToString()));
			Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			for (int i = 0; i < day_number - 1; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					NPC character4 = r.ChooseFrom(characters);
					characters.Remove(character4);
					if (characters.Count == 0)
					{
						break;
					}
				}
			}
			if (characters.Count > 0)
			{
				NPC character2 = r.ChooseFrom(characters);
				characters.Remove(character2);
				DesertFestival.SetupMerchantSchedule(character2, 0);
			}
			if (characters.Count > 0)
			{
				NPC character3 = r.ChooseFrom(characters);
				characters.Remove(character3);
				DesertFestival.SetupMerchantSchedule(character3, 1);
			}
			DesertFestival festival_location = Game1.getLocationFromName("DesertFestival") as DesertFestival;
			if (festival_location != null)
			{
				festival_location.netRacers.Clear();
				List<int> racers = new List<int>();
				for (int k = 0; k < festival_location.totalRacers; k++)
				{
					racers.Add(k);
				}
				for (int l = 0; l < festival_location.racerCount; l++)
				{
					int racer_index = r.ChooseFrom(racers);
					racers.Remove(racer_index);
					Racer racer = new Racer(racer_index);
					racer.position.Value = new Vector2(44.5f, 37.5f - (float)l) * 64f;
					racer.segmentStart = racer.position.Value;
					racer.segmentEnd = racer.position.Value;
					festival_location.netRacers.Add(racer);
				}
			}
			SpecialOrder.UpdateAvailableSpecialOrders("DesertFestivalMarlon", true);
		}

		// Token: 0x04001FCE RID: 8142
		public const int CALICO_STATUE_GHOST_INVASION = 0;

		// Token: 0x04001FCF RID: 8143
		public const int CALICO_STATUE_SERPENT_INVASION = 1;

		// Token: 0x04001FD0 RID: 8144
		public const int CALICO_STATUE_SKELETON_INVASION = 2;

		// Token: 0x04001FD1 RID: 8145
		public const int CALICO_STATUE_BAT_INVASION = 3;

		// Token: 0x04001FD2 RID: 8146
		public const int CALICO_STATUE_ASSASSIN_BUGS = 4;

		// Token: 0x04001FD3 RID: 8147
		public const int CALICO_STATUE_THIN_SHELLS = 5;

		// Token: 0x04001FD4 RID: 8148
		public const int CALICO_STATUE_MEAGER_MEALS = 6;

		// Token: 0x04001FD5 RID: 8149
		public const int CALICO_STATUE_MONSTER_SURGE = 7;

		// Token: 0x04001FD6 RID: 8150
		public const int CALICO_STATUE_SHARP_TEETH = 8;

		// Token: 0x04001FD7 RID: 8151
		public const int CALICO_STATUE_MUMMY_CURSE = 9;

		// Token: 0x04001FD8 RID: 8152
		public const int CALICO_STATUE_SPEED_BOOST = 10;

		// Token: 0x04001FD9 RID: 8153
		public const int CALICO_STATUE_REFRESH = 11;

		// Token: 0x04001FDA RID: 8154
		public const int CALICO_STATUE_50_EGG_TREASURE = 12;

		// Token: 0x04001FDB RID: 8155
		public const int CALICO_STATUE_NO_EFFECT = 13;

		// Token: 0x04001FDC RID: 8156
		public const int CALICO_STATUE_TOOTH_FILE = 14;

		// Token: 0x04001FDD RID: 8157
		public const int CALICO_STATUE_25_EGG_TREASURE = 15;

		// Token: 0x04001FDE RID: 8158
		public const int CALICO_STATUE_10_EGG_TREASURE = 16;

		// Token: 0x04001FDF RID: 8159
		public const int CALICO_STATUE_100_EGG_TREASURE = 17;

		// Token: 0x04001FE0 RID: 8160
		public static readonly int[] CalicoStatueInvasionIds = new int[]
		{
			3,
			0,
			1,
			2
		};

		// Token: 0x04001FE1 RID: 8161
		public const int NUM_SCHOLAR_QUESTIONS = 4;

		// Token: 0x04001FE2 RID: 8162
		public const string FISHING_QUEST_ID = "98765";

		// Token: 0x04001FE3 RID: 8163
		protected RandomizedPlantFurniture _cactusGuyRevealItem;

		// Token: 0x04001FE4 RID: 8164
		protected float _cactusGuyRevealTimer = -1f;

		// Token: 0x04001FE5 RID: 8165
		protected float _cactusShakeTimer = -1f;

		// Token: 0x04001FE6 RID: 8166
		protected int _currentlyShownCactusID;

		// Token: 0x04001FE7 RID: 8167
		protected NetEvent1Field<int, NetInt> _revealCactusEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04001FE8 RID: 8168
		protected NetEvent1Field<int, NetInt> _hideCactusEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04001FE9 RID: 8169
		protected MoneyDial eggMoneyDial;

		// Token: 0x04001FEA RID: 8170
		[XmlIgnore]
		public NetList<Racer, NetRef<Racer>> netRacers = new NetList<Racer, NetRef<Racer>>();

		// Token: 0x04001FEB RID: 8171
		[XmlIgnore]
		protected List<Racer> _localRacers = new List<Racer>();

		// Token: 0x04001FEC RID: 8172
		[XmlIgnore]
		protected float festivalChimneyTimer;

		// Token: 0x04001FED RID: 8173
		[XmlIgnore]
		public List<int> finishedRacers = new List<int>();

		// Token: 0x04001FEE RID: 8174
		[XmlIgnore]
		public int racerCount = 3;

		// Token: 0x04001FEF RID: 8175
		[XmlIgnore]
		public int totalRacers = 5;

		// Token: 0x04001FF0 RID: 8176
		[XmlIgnore]
		public NetEvent1Field<string, NetString> announceRaceEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x04001FF1 RID: 8177
		[XmlIgnore]
		public NetEnum<DesertFestival.RaceState> currentRaceState = new NetEnum<DesertFestival.RaceState>(DesertFestival.RaceState.PreRace);

		// Token: 0x04001FF2 RID: 8178
		[XmlIgnore]
		public NetLongDictionary<int, NetInt> sabotages = new NetLongDictionary<int, NetInt>();

		// Token: 0x04001FF3 RID: 8179
		[XmlIgnore]
		public NetLongDictionary<int, NetInt> raceGuesses = new NetLongDictionary<int, NetInt>();

		// Token: 0x04001FF4 RID: 8180
		[XmlIgnore]
		public NetLongDictionary<int, NetInt> nextRaceGuesses = new NetLongDictionary<int, NetInt>();

		// Token: 0x04001FF5 RID: 8181
		[XmlIgnore]
		public NetLongDictionary<bool, NetBool> specialRewardsCollected = new NetLongDictionary<bool, NetBool>();

		// Token: 0x04001FF6 RID: 8182
		[XmlIgnore]
		public NetLongDictionary<int, NetInt> rewardsToCollect = new NetLongDictionary<int, NetInt>();

		// Token: 0x04001FF7 RID: 8183
		[XmlIgnore]
		public NetInt lastRaceWinner = new NetInt();

		// Token: 0x04001FF8 RID: 8184
		[XmlIgnore]
		protected float _raceStateTimer;

		// Token: 0x04001FF9 RID: 8185
		protected string _raceText;

		// Token: 0x04001FFA RID: 8186
		protected float _raceTextTimer;

		// Token: 0x04001FFB RID: 8187
		protected bool _raceTextShake;

		// Token: 0x04001FFC RID: 8188
		protected int _localSabotageText = -1;

		// Token: 0x04001FFD RID: 8189
		protected int _currentScholarQuestion = -1;

		// Token: 0x04001FFE RID: 8190
		protected int _cookIngredient = -1;

		// Token: 0x04001FFF RID: 8191
		protected int _cookSauce = -1;

		// Token: 0x04002000 RID: 8192
		public Vector3[][] raceTrack = new Vector3[][]
		{
			new Vector3[]
			{
				new Vector3(41f, 39f, 0f),
				new Vector3(42f, 39f, 0f)
			},
			new Vector3[]
			{
				new Vector3(41f, 29f, 0f),
				new Vector3(42f, 28f, 0f)
			},
			new Vector3[]
			{
				new Vector3(6f, 29f, 0f),
				new Vector3(5f, 28f, 0f)
			},
			new Vector3[]
			{
				new Vector3(6f, 35f, 0f),
				new Vector3(5f, 36f, 0f)
			},
			new Vector3[]
			{
				new Vector3(10f, 35f, 2f),
				new Vector3(10f, 36f, 2f)
			},
			new Vector3[]
			{
				new Vector3(12.5f, 35f, 0f),
				new Vector3(12.5f, 36f, 0f)
			},
			new Vector3[]
			{
				new Vector3(17.5f, 35f, 1f),
				new Vector3(17.5f, 36f, 1f)
			},
			new Vector3[]
			{
				new Vector3(23.5f, 35f, 0f),
				new Vector3(23.5f, 36f, 0f)
			},
			new Vector3[]
			{
				new Vector3(28.5f, 35f, 1f),
				new Vector3(28.5f, 36f, 1f)
			},
			new Vector3[]
			{
				new Vector3(31f, 35f, 0f),
				new Vector3(31f, 36f, 0f)
			},
			new Vector3[]
			{
				new Vector3(32f, 35f, 0f),
				new Vector3(31f, 36f, 0f)
			},
			new Vector3[]
			{
				new Vector3(32f, 38f, 3f),
				new Vector3(31f, 38f, 3f)
			},
			new Vector3[]
			{
				new Vector3(32f, 43f, 0f),
				new Vector3(31f, 43f, 0f)
			},
			new Vector3[]
			{
				new Vector3(32f, 46f, 0f),
				new Vector3(31f, 47f, 0f)
			},
			new Vector3[]
			{
				new Vector3(41f, 46f, 0f),
				new Vector3(42f, 47f, 0f)
			},
			new Vector3[]
			{
				new Vector3(41f, 39f, 0f),
				new Vector3(42f, 39f, 0f)
			}
		};

		// Token: 0x04002001 RID: 8193
		private bool checkedMineExplanation;

		// Token: 0x0200064F RID: 1615
		public enum RaceState
		{
			// Token: 0x04002F34 RID: 12084
			PreRace,
			// Token: 0x04002F35 RID: 12085
			StartingLine,
			// Token: 0x04002F36 RID: 12086
			Ready,
			// Token: 0x04002F37 RID: 12087
			Set,
			// Token: 0x04002F38 RID: 12088
			Go,
			// Token: 0x04002F39 RID: 12089
			AnnounceWinner,
			// Token: 0x04002F3A RID: 12090
			AnnounceWinner2,
			// Token: 0x04002F3B RID: 12091
			AnnounceWinner3,
			// Token: 0x04002F3C RID: 12092
			AnnounceWinner4,
			// Token: 0x04002F3D RID: 12093
			RaceEnd,
			// Token: 0x04002F3E RID: 12094
			RacesOver
		}
	}
}

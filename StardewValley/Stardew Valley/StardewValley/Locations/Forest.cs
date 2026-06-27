using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002D2 RID: 722
	public class Forest : GameLocation
	{
		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06002F5D RID: 12125 RVA: 0x00254B3B File Offset: 0x00252D3B
		// (set) Token: 0x06002F5E RID: 12126 RVA: 0x00254B48 File Offset: 0x00252D48
		[XmlIgnore]
		public bool travelingMerchantDay
		{
			get
			{
				return this.netTravelingMerchantDay.Value;
			}
			set
			{
				this.netTravelingMerchantDay.Value = value;
			}
		}

		// Token: 0x06002F5F RID: 12127 RVA: 0x00254B58 File Offset: 0x00252D58
		public Forest()
		{
		}

		// Token: 0x06002F60 RID: 12128 RVA: 0x00254BF8 File Offset: 0x00252DF8
		public Forest(string map, string name) : base(map, name)
		{
			this.marniesLivestock.Add(new FarmAnimal("Dairy Cow", Game1.multiplayer.getNewID(), -1L));
			this.marniesLivestock.Add(new FarmAnimal("Dairy Cow", Game1.multiplayer.getNewID(), -1L));
			this.marniesLivestock[0].Position = new Vector2((float)((this.MarnieLivestockArea.X + 4) * 64), (float)((this.MarnieLivestockArea.Y + 3) * 64));
			this.marniesLivestock[1].Position = new Vector2((float)((this.MarnieLivestockArea.X + 7) * 64), (float)((this.MarnieLivestockArea.Y + 3) * 64));
			this.resourceClumps.Add(new ResourceClump(602, 2, 2, new Vector2(1f, 6f), null, null));
		}

		// Token: 0x06002F61 RID: 12129 RVA: 0x00254D7C File Offset: 0x00252F7C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.marniesLivestock, "marniesLivestock").AddField(this.travelingMerchantBounds, "travelingMerchantBounds").AddField(this.netTravelingMerchantDay, "netTravelingMerchantDay").AddField(this.stumpFixed, "stumpFixed").AddField(this.derbyMutex.NetFields, "derbyMutex.NetFields");
			this.stumpFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					Forest.fixStump(this);
				}
			};
			this.characters.OnValueAdded += delegate(NPC newCharacter)
			{
				this.adjustDerbyFisherman(newCharacter);
			};
		}

		// Token: 0x06002F62 RID: 12130 RVA: 0x00254E1C File Offset: 0x0025301C
		public override void seasonUpdate(bool onLoad = false)
		{
			base.seasonUpdate(onLoad);
			if (!onLoad && Game1.season == Season.Spring)
			{
				Microsoft.Xna.Framework.Rectangle area = this.MarnieLivestockArea;
				base.loadPathsLayerObjectsInArea(area.X, area.Y, area.Width, area.Height);
			}
		}

		// Token: 0x06002F63 RID: 12131 RVA: 0x00254E60 File Offset: 0x00253060
		private void adjustDerbyFisherman(NPC npc)
		{
			if (npc.name.Equals("derby_contestent0"))
			{
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				AnimatedSprite sprite = npc.Sprite;
				if (((sprite != null) ? sprite.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 0, 16, 64);
				}
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent1"))
			{
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				AnimatedSprite sprite2 = npc.Sprite;
				if (((sprite2 != null) ? sprite2.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 16, 64);
				}
				npc.Sprite.CurrentFrame = 2;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent2"))
			{
				AnimatedSprite sprite3 = npc.Sprite;
				if (((sprite3 != null) ? sprite3.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 3, 16, 64);
				}
				npc.Sprite.CurrentFrame = 3;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent3"))
			{
				AnimatedSprite sprite4 = npc.Sprite;
				if (((sprite4 != null) ? sprite4.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 1, 16, 64);
				}
				npc.Sprite.CurrentFrame = 1;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent4"))
			{
				AnimatedSprite sprite5 = npc.Sprite;
				if (((sprite5 != null) ? sprite5.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 32, 64);
				}
				npc.Sprite.CurrentFrame = 2;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent5"))
			{
				AnimatedSprite sprite6 = npc.Sprite;
				if (((sprite6 != null) ? sprite6.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 8, 32, 32);
				}
				npc.Sprite.CurrentFrame = 8;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent6"))
			{
				AnimatedSprite sprite7 = npc.Sprite;
				if (((sprite7 != null) ? sprite7.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 9, 32, 32);
				}
				npc.Sprite.CurrentFrame = 9;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent7"))
			{
				AnimatedSprite sprite8 = npc.Sprite;
				if (((sprite8 != null) ? sprite8.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 10, 32, 32);
				}
				npc.Sprite.CurrentFrame = 10;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent8"))
			{
				AnimatedSprite sprite9 = npc.Sprite;
				if (((sprite9 != null) ? sprite9.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 11, 32, 32);
				}
				npc.Sprite.CurrentFrame = 11;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("derby_contestent9"))
			{
				AnimatedSprite sprite10 = npc.Sprite;
				if (((sprite10 != null) ? sprite10.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 12, 32, 32);
				}
				npc.Sprite.CurrentFrame = 12;
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
		}

		// Token: 0x06002F64 RID: 12132 RVA: 0x002552B0 File Offset: 0x002534B0
		public static void fixStump(GameLocation location)
		{
			if (!NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
			{
				NetWorldState.addWorldStateIDEverywhere("forestStumpFixed");
			}
			location.updateMap();
			for (int x = 52; x < 60; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					location.removeTile(x, y, "AlwaysFront");
				}
			}
			location.ApplyMapOverride("Forest_RaccoonHouse", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6)));
			location.largeTerrainFeatures.Remove(location.getLargeTerrainFeatureAt(55, 10));
			location.largeTerrainFeatures.Remove(location.getLargeTerrainFeatureAt(56, 13));
			location.largeTerrainFeatures.Remove(location.getLargeTerrainFeatureAt(61, 10));
			Game1.currentLightSources.Add(new LightSource("Forest_RaccoonHouse", 4, new Vector2(3540f, 357f), 0.75f, Color.Black * 0.6f, LightSource.LightContext.None, 0L, location.NameOrUniqueName));
		}

		// Token: 0x06002F65 RID: 12133 RVA: 0x002553A8 File Offset: 0x002535A8
		public void removeSewerTrash()
		{
			base.ApplyMapOverride("Forest-SewerClean", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(83, 97, 24, 12)));
			base.removeMapTile(43, 106, "Buildings");
			base.removeMapTile(17, 106, "Buildings");
			base.removeMapTile(13, 105, "Buildings");
			base.removeMapTile(4, 85, "Buildings");
			base.removeMapTile(2, 85, "Buildings");
		}

		// Token: 0x06002F66 RID: 12134 RVA: 0x00255424 File Offset: 0x00253624
		protected override void resetLocalState()
		{
			base.resetLocalState();
			base.addFrog();
			if (Game1.year > 2 && base.getCharacterFromName("TrashBear") != null && NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
			{
				this.characters.Remove(base.getCharacterFromName("TrashBear"));
			}
			if (this.numRaccoonBabies == -1)
			{
				this.numRaccoonBabies = Game1.netWorldState.Value.TimesFedRaccoons - 1;
				if (Game1.netWorldState.Value.Date.TotalDays - Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished < 7)
				{
					this.numRaccoonBabies--;
				}
				if (this.numRaccoonBabies < 0)
				{
					this.numRaccoonBabies = 0;
				}
				if (this.numRaccoonBabies >= 8)
				{
					Game1.getAchievement(39, true);
				}
			}
			if (!Game1.eventUp && !Game1.player.mailReceived.Contains("seenRaccoonFinishEvent") && this.numRaccoonBabies >= 8 && !Game1.isRaining && !Game1.isSnowing && Game1.timeOfDay < Game1.getStartingToGetDarkTime(this))
			{
				Game1.player.mailReceived.Add("seenRaccoonFinishEvent");
				string raccoon_event = "none/-10000 -1000/farmer 56 15 0/skippable/specificTemporarySprite raccoonCircle/viewport 56 6 true/pause 3000/specificTemporarySprite raccoonSong/playSound raccoonSong/precisePause 9505/specificTemporarySprite raccoonCircle2/precisePause 9405/specificTemporarySprite raccoonbutterflies/precisePause 9505/specificTemporarySprite raccoondance1/precisePause 9505/specificTemporarySprite raccoondance2/pause 6000/globalfade .003 false/viewport -10000 -1000/spriteText 6 \"" + Game1.content.LoadString("Strings\\1_6_Strings:RaccoonFinal") + "\"/pause 500/end";
				this.startEvent(new Event(raccoon_event, null));
			}
			if (Game1.stats.DaysPlayed > 3U)
			{
				Random asdfaTime = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
				int time = Utility.ModifyTime(1920, asdfaTime.Next(390));
				int delayBeforeStart = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, time) * Game1.realMilliSecondsPerGameMinute;
				if (delayBeforeStart > 0)
				{
					if (asdfaTime.NextDouble() < 0.5)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(146f, 3851f), true, 0f, Color.White)
						{
							animationLength = 8,
							totalNumberOfLoops = 99,
							interval = 100f,
							motion = new Vector2(-4f, 0f),
							scale = 5.5f,
							delayBeforeAnimationStart = delayBeforeStart
						});
					}
					else
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(318f, 1129f), true, 0f, Color.White)
						{
							animationLength = 8,
							totalNumberOfLoops = 99,
							interval = 100f,
							motion = new Vector2(-4f, 0f),
							scale = 5.5f,
							delayBeforeAnimationStart = delayBeforeStart
						});
					}
				}
			}
			if (Utility.doesAnyFarmerHaveMail("asdlkjfg1") && Utility.CreateDaySaveRandom(105.0, 0.0, 0.0).NextDouble() < 0.03)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(4f, 90f) * 64f, false, 0f, Color.White * 0.66f)
				{
					scale = 4f,
					layerDepth = 0f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(3.2f, 89f) * 64f, true, 0f, Color.White)
				{
					scale = 4f,
					layerDepth = 0f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(4f, 88f) * 64f, false, 0f, Color.White)
				{
					scale = 4f,
					layerDepth = 0f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(3f, 87f) * 64f, true, 0f, Color.White * 0.66f)
				{
					scale = 4f,
					layerDepth = 0f
				});
			}
		}

		// Token: 0x06002F67 RID: 12135 RVA: 0x002558B4 File Offset: 0x00253AB4
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			this.derbyMutex.ReleaseLock();
		}

		// Token: 0x06002F68 RID: 12136 RVA: 0x002558C8 File Offset: 0x00253AC8
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				this.hasShownCCUpgrade = false;
			}
			if (this.stumpFixed.Value)
			{
				Forest.fixStump(this);
			}
			else if (Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
			{
				for (int x = 52; x < 60; x++)
				{
					for (int y = 0; y < 2; y++)
					{
						base.removeTile(x, y, "AlwaysFront");
					}
				}
				base.ApplyMapOverride("Forest_RaccoonStump", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6)));
			}
			if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
			{
				this.removeSewerTrash();
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				this.showCommunityUpgradeShortcuts();
			}
			if (Game1.IsSummer && Game1.dayOfMonth >= 17 && Game1.dayOfMonth <= 19)
			{
				base.ApplyMapOverride(Game1.game1.xTileContent.Load<Map>("Maps\\Forest_FishingDerbySign"), "Forest_FishingDerbySign", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(69, 44, 2, 3)), new Action<Point>(base.cleanUpTileForMapOverride));
			}
			else if (this._appliedMapOverrides.Contains("Forest_FishingDerbySign"))
			{
				base.ApplyMapOverride("Forest_FishingDerbySign_Revert", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(69, 44, 2, 3)));
				this._appliedMapOverrides.Remove("Forest_FishingDerbySign");
				this._appliedMapOverrides.Remove("Forest_FishingDerbySign_Revert");
			}
			if (Game1.IsSummer && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21)
			{
				if (base.getCharacterFromName("derby_contestent0") == null && (Game1.IsMasterGame || !Game1.player.sleptInTemporaryBed.Value))
				{
					this.derbyMutex.RequestLock(delegate
					{
						if (base.getCharacterFromName("derby_contestent0") == null)
						{
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(66, 50))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 0, 16, 64), new Vector2(66f, 50f) * 64f, -1, "derby_contestent0", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(69, 50))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 16, 64), new Vector2(69f, 50f) * 64f, -1, "derby_contestent1", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(74, 50))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 3, 16, 64), new Vector2(74f, 50f) * 64f, -1, "derby_contestent2", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(43, 59))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 1, 16, 64), new Vector2(43f, 59f) * 64f, -1, "derby_contestent3", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(84, 40) && base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(85, 40))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 32, 64), new Vector2(84f, 40f) * 64f, -1, "derby_contestent4", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(88, 49))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 8, 32, 32), new Vector2(88f, 49f) * 64f, -1, "derby_contestent5", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(92, 54))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 9, 32, 32), new Vector2(91f, 54f) * 64f, -1, "derby_contestent6", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(20, 73))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 10, 32, 32), new Vector2(20f, 73f) * 64f, -1, "derby_contestent7", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(77, 48))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 11, 32, 32), new Vector2(76f, 48f) * 64f, -1, "derby_contestent8", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(83, 51))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen", 12, 32, 32), new Vector2(82f, 51f) * 64f, -1, "derby_contestent9", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
						}
						this.derbyMutex.ReleaseLock();
					}, null);
				}
				base.ApplyMapOverride(Game1.game1.xTileContent.Load<Map>("Maps\\Forest_FishingDerby"), "Forest_FishingDerby", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(63, 43, 11, 5)), new Action<Point>(base.cleanUpTileForMapOverride));
				Game1.currentLightSources.Add(new LightSource("FishingDerby_1", 1, new Vector2(4596f, 2968f), 3f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("FishingDerby_2", 1, new Vector2(4324f, 3044f), 3f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				return;
			}
			if (this._appliedMapOverrides.Contains("Forest_FishingDerby") || base.hasTileAt(63, 47, "Buildings", null))
			{
				base.ApplyMapOverride("Forest_FishingDerby_Revert", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(63, 43, 11, 5)));
				this._appliedMapOverrides.Remove("Forest_FishingDerby");
				this._appliedMapOverrides.Remove("Forest_FishingDerby_Revert");
				this.characters.RemoveWhere((NPC npc) => npc.Name.StartsWith("derby_contestent"));
			}
		}

		// Token: 0x06002F69 RID: 12137 RVA: 0x00255BE0 File Offset: 0x00253DE0
		private void showCommunityUpgradeShortcuts()
		{
			if (!this.hasShownCCUpgrade)
			{
				base.removeTile(119, 36, "Buildings");
				LargeTerrainFeature blockingBush = null;
				foreach (LargeTerrainFeature t in this.largeTerrainFeatures)
				{
					if (t.Tile == new Vector2(119f, 35f))
					{
						blockingBush = t;
						break;
					}
				}
				if (blockingBush != null)
				{
					this.largeTerrainFeatures.Remove(blockingBush);
				}
				this.hasShownCCUpgrade = true;
				this.warps.Add(new Warp(120, 35, "Beach", 0, 6, false, false));
				this.warps.Add(new Warp(120, 36, "Beach", 0, 6, false, false));
			}
		}

		// Token: 0x06002F6A RID: 12138 RVA: 0x00255CBC File Offset: 0x00253EBC
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (this.ShouldTravelingMerchantVisitToday())
			{
				if (this.travelingMerchantDay)
				{
					goto IL_102;
				}
				this.travelingMerchantDay = true;
				Point merchantOrigin = this.GetTravelingMerchantCartTile();
				this.travelingMerchantBounds.Clear();
				this.travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(merchantOrigin.X * 64, merchantOrigin.Y * 64, 492, 116));
				this.travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(merchantOrigin.X * 64 + 180, merchantOrigin.Y * 64 + 104, 76, 48));
				this.travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(merchantOrigin.X * 64 + 340, merchantOrigin.Y * 64 + 104, 104, 48));
				using (NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle>.Enumerator enumerator = this.travelingMerchantBounds.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Microsoft.Xna.Framework.Rectangle r = enumerator.Current;
						Utility.clearObjectsInArea(r, this);
					}
					goto IL_102;
				}
			}
			this.travelingMerchantDay = false;
			this.travelingMerchantBounds.Clear();
			IL_102:
			if (Game1.year > 2 && !base.IsRainingHere() && !Utility.isFestivalDay() && base.getCharacterFromName("TrashBear") == null && !NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
			{
				this.characters.Add(new TrashBear());
			}
			if (Game1.MasterPlayer.mailReceived.Contains("raccoonMovedIn"))
			{
				if (base.getCharacterFromName("Raccoon") == null)
				{
					this.characters.Add(new Raccoon(false));
				}
				if (base.getCharacterFromName("MrsRaccoon") == null && (Game1.netWorldState.Value.TimesFedRaccoons > 1 || (Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished != 0 && Game1.netWorldState.Value.Date.TotalDays - Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished >= 7)))
				{
					this.characters.Add(new Raccoon(true));
				}
			}
		}

		// Token: 0x06002F6B RID: 12139 RVA: 0x00255EBC File Offset: 0x002540BC
		public static bool isWizardHouseUnlocked()
		{
			if (Game1.player.mailReceived.Contains("wizardJunimoNote"))
			{
				return true;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				return true;
			}
			bool flag = Game1.MasterPlayer.mailReceived.Contains("ccFishTank");
			bool ccBulletin = Game1.MasterPlayer.mailReceived.Contains("ccBulletin");
			bool ccPantry = Game1.MasterPlayer.mailReceived.Contains("ccPantry");
			bool ccVault = Game1.MasterPlayer.mailReceived.Contains("ccVault");
			bool ccBoilerRoom = Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom");
			bool ccCraftsRoom = Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom");
			return flag && ccBulletin && ccPantry && ccVault && ccBoilerRoom && ccCraftsRoom;
		}

		// Token: 0x06002F6C RID: 12140 RVA: 0x00255F82 File Offset: 0x00254182
		public bool ShouldTravelingMerchantVisitToday()
		{
			return Game1.dayOfMonth % 7 % 5 == 0;
		}

		// Token: 0x06002F6D RID: 12141 RVA: 0x00255F90 File Offset: 0x00254190
		public Point GetTravelingMerchantCartTile()
		{
			Point tile;
			if (!base.TryGetMapPropertyAs("TravelingCartPosition", out tile, false))
			{
				return new Point(23, 10);
			}
			return tile;
		}

		// Token: 0x06002F6E RID: 12142 RVA: 0x00255FB8 File Offset: 0x002541B8
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexOfCheckLocation = base.getTileIndexAt(tileLocation, "Buildings", "outdoors");
			if (tileIndexOfCheckLocation == 901 && !Forest.isWizardHouseUnlocked())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Forest_WizardTower_Locked"));
				return false;
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			if (tileIndexOfCheckLocation != 1394)
			{
				if (tileIndexOfCheckLocation == 1972)
				{
					if (who.achievements.Count > 0)
					{
						Utility.TryOpenShopMenu("HatMouse", "HatMouse", true);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Forest_HatMouseStore_Abandoned"));
					}
				}
			}
			else if (who.mailReceived.Contains("OpenedSewer"))
			{
				Game1.warpFarmer("Sewer", 3, 48, 0);
				base.playSound("openChest", null, null, SoundContext.Default);
			}
			else if (who.hasRustyKey)
			{
				base.playSound("openBox", null, null, SoundContext.Default);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
				who.mailReceived.Add("OpenedSewer");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
			}
			if (this.travelingMerchantDay && Game1.timeOfDay < 2000)
			{
				Point cartOrigin = this.GetTravelingMerchantCartTile();
				if (tileLocation.X == cartOrigin.X + 4 && tileLocation.Y == cartOrigin.Y + 1)
				{
					Utility.TryOpenShopMenu("Traveler", null, true);
					return true;
				}
				if (tileLocation.X == cartOrigin.X && tileLocation.Y == cartOrigin.Y + 1)
				{
					base.playSound("pig", null, null, SoundContext.Default);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002F6F RID: 12143 RVA: 0x00256190 File Offset: 0x00254390
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
		{
			if (this.travelingMerchantBounds != null)
			{
				foreach (Microsoft.Xna.Framework.Rectangle r in this.travelingMerchantBounds)
				{
					if (position.Intersects(r))
					{
						return true;
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement, false);
		}

		// Token: 0x06002F70 RID: 12144 RVA: 0x0025620C File Offset: 0x0025440C
		public override bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
		{
			if (this.travelingMerchantBounds != null)
			{
				Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)v.X * 64, (int)v.Y * 64, 64, 64);
				foreach (Microsoft.Xna.Framework.Rectangle r in this.travelingMerchantBounds)
				{
					if (tileRect.Intersects(r))
					{
						return false;
					}
				}
			}
			return base.isTilePlaceable(v, itemIsPassable);
		}

		// Token: 0x06002F71 RID: 12145 RVA: 0x00256298 File Offset: 0x00254498
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.numRaccoonBabies = -1;
			if (Game1.IsMasterGame && this.ShouldTravelingMerchantVisitToday() && Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0)
			{
				NetWorldState value = Game1.netWorldState.Value;
				int num = value.VisitsUntilY1Guarantee;
				value.VisitsUntilY1Guarantee = num - 1;
			}
			if (base.IsSpringHere())
			{
				for (int i = 0; i < 7; i++)
				{
					Vector2 origin = new Vector2((float)Game1.random.Next(70, this.map.Layers[0].LayerWidth - 10), (float)Game1.random.Next(68, this.map.Layers[0].LayerHeight - 15));
					if (origin.Y > 30f)
					{
						foreach (Vector2 v in Utility.recursiveFindOpenTiles(this, origin, 16, 50))
						{
							string s = this.doesTileHaveProperty((int)v.X, (int)v.Y, "Diggable", "Back", false);
							if (!this.terrainFeatures.ContainsKey(v) && s != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(origin, v) * 0.15f))
							{
								this.terrainFeatures.Add(v, new HoeDirt(0, new Crop(true, "1", (int)v.X, (int)v.Y, this)));
							}
						}
					}
				}
			}
			if (Game1.year > 2 && base.getCharacterFromName("TrashBear") != null)
			{
				this.characters.Remove(base.getCharacterFromName("TrashBear"));
			}
			if (Game1.IsSummer)
			{
				this.characters.RemoveWhere((NPC npc) => npc.Name.StartsWith("derby_contestent"));
			}
			if (Game1.IsSpring)
			{
				int num = Game1.dayOfMonth;
				if (num == 17)
				{
					this.objects.TryAdd(new Vector2(52f, 98f), ItemRegistry.Create<Object>("(O)PotOfGold", 1, 0, false));
					return;
				}
				if (num != 18)
				{
					return;
				}
				Object valueOrDefault = this.objects.GetValueOrDefault(new Vector2(52f, 98f), null);
				if (((valueOrDefault != null) ? valueOrDefault.QualifiedItemId : null) == "(O)PotOfGold")
				{
					this.objects.Remove(new Vector2(52f, 98f));
				}
			}
		}

		// Token: 0x06002F72 RID: 12146 RVA: 0x00256530 File Offset: 0x00254730
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			this.derbyMutex.Update(this);
		}

		// Token: 0x06002F73 RID: 12147 RVA: 0x00256548 File Offset: 0x00254748
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (FarmAnimal farmAnimal in this.marniesLivestock)
			{
				farmAnimal.updateWhenCurrentLocation(time, this);
			}
			if (Game1.timeOfDay < 2000)
			{
				Point cartOrigin = this.GetTravelingMerchantCartTile();
				if (this.travelingMerchantDay)
				{
					if (Game1.random.NextDouble() < 0.001)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(99, 1423, 13, 19), new Vector2((float)(cartOrigin.X * 64), (float)(cartOrigin.Y * 64 + 32 - 4)), false, 0f, Color.White)
						{
							interval = (float)Game1.random.Next(500, 1500),
							layerDepth = 0.07682f,
							scale = 4f
						});
					}
					if (Game1.random.NextDouble() < 0.001)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(51, 1444, 5, 5), new Vector2((float)(cartOrigin.X * 64 + 32 - 4), (float)((cartOrigin.Y + 1) * 64 + 32 + 8)), false, 0f, Color.White)
						{
							interval = 500f,
							animationLength = 1,
							layerDepth = 0.07682f,
							scale = 4f
						});
					}
					if (Game1.random.NextDouble() < 0.003)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(89, 1445, 6, 3), new Vector2((float)((cartOrigin.X + 4) * 64 + 32 + 4), (float)(cartOrigin.Y * 64 + 24)), false, 0f, Color.White)
						{
							interval = 50f,
							animationLength = 3,
							pingPong = true,
							totalNumberOfLoops = 1,
							layerDepth = 0.07682f,
							scale = 4f
						});
					}
				}
				this.chimneyTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.chimneyTimer <= 0)
				{
					this.chimneyTimer = (this.travelingMerchantDay ? 500 : Game1.random.Next(200, 2000));
					Vector2 smokeSpot = this.travelingMerchantDay ? new Vector2((float)((cartOrigin.X + 6) * 64 + 12), (float)((cartOrigin.Y - 2) * 64 + 12)) : new Vector2(5592f, 608f);
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), smokeSpot, false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
					});
					if (this.stumpFixed.Value && Game1.MasterPlayer.mailReceived.Contains("raccoonMovedIn"))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(57.33f, 1.75f) * 64f, false, 0.002f, Color.Gray)
						{
							alpha = 0.75f,
							motion = new Vector2(0f, -0.5f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							drawAboveAlwaysFront = true,
							layerDepth = 1f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
						});
					}
					if (this.travelingMerchantDay)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(225, 1388, 7, 5), new Vector2((float)((cartOrigin.X + 6) * 64 + 12), (float)((cartOrigin.Y - 2) * 64 + 24)), false, 0f, Color.White)
						{
							interval = (float)(this.chimneyTimer - this.chimneyTimer / 5),
							animationLength = 1,
							layerDepth = 0.99f,
							scale = 4.3f,
							scaleChange = -0.015f
						});
					}
				}
			}
		}

		// Token: 0x06002F74 RID: 12148 RVA: 0x00256A64 File Offset: 0x00254C64
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen") && action.Length != 0 && action[0] == "FixRaccoonStump")
			{
				if (who.Items.ContainsId("(O)709", 100))
				{
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Question"), base.createYesNoResponses(), "ForestStump");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Hint"));
					if (!who.mailReceived.Contains("checkedRaccoonStump"))
					{
						who.addQuest("134");
						who.mailReceived.Add("checkedRaccoonStump");
					}
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06002F75 RID: 12149 RVA: 0x00256B24 File Offset: 0x00254D24
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == "ForestStump_Yes")
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.fadedForStumpFix), 0.02f);
				Game1.player.Items.ReduceId("(O)709", 100);
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.HasQuest, PlayerActionTarget.All, "134", false, null);
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x06002F76 RID: 12150 RVA: 0x00256B98 File Offset: 0x00254D98
		public void fadedForStumpFix()
		{
			Game1.freezeControls = true;
			DelayedAction.playSoundAfterDelay("crafting", 1000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("crafting", 1500, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("crafting", 2000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("crafting", 2500, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("axchop", 3000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("discoverMineral", 3200, null, null, -1, false);
			Game1.viewportFreeze = true;
			Game1.viewport.X = -10000;
			this.stumpFixed.Value = true;
			Game1.pauseThenDoFunction(4000, new Game1.afterFadeFunction(this.doneWithStumpFix));
			Forest.fixStump(this);
			Game1.addMailForTomorrow("raccoonMovedIn", true, true);
		}

		// Token: 0x06002F77 RID: 12151 RVA: 0x00256C96 File Offset: 0x00254E96
		public void doneWithStumpFix()
		{
			Game1.globalFadeToClear(delegate
			{
				if (!Game1.fadeToBlack)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Done"));
				}
			}, 0.02f);
			Game1.viewportFreeze = false;
			Game1.freezeControls = false;
		}

		// Token: 0x06002F78 RID: 12152 RVA: 0x00256CD0 File Offset: 0x00254ED0
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (this.travelingMerchantDay && Game1.random.NextDouble() < 0.4)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(57, 1430, 4, 12), new Vector2(1792f, 656f), false, 0f, Color.White)
				{
					interval = 50f,
					animationLength = 10,
					pingPong = true,
					totalNumberOfLoops = 1,
					layerDepth = 0.07682f,
					scale = 4f
				});
				if (Game1.random.NextDouble() < 0.66)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(89, 1445, 6, 3), new Vector2(1764f, 664f), false, 0f, Color.White)
					{
						interval = 50f,
						animationLength = 3,
						pingPong = true,
						totalNumberOfLoops = 1,
						layerDepth = 0.07683001f,
						scale = 4f
					});
				}
			}
			if (Game1.IsSummer && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21)
			{
				Random r = Utility.CreateDaySaveRandom((double)(Game1.timeOfDay * 20), 0.0, 0.0);
				NPC i = base.getCharacterFromName("derby_contestent" + r.Next(10).ToString());
				if (i != null)
				{
					i.shake(600);
					if (r.NextBool(0.25))
					{
						int whichSaying = r.Next(7);
						i.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerby_Exclamation" + whichSaying.ToString()), null, 2, 3000, 0);
						if (whichSaying == 0 || whichSaying == 6)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite(138, 1500f, 1, 1, i.Position, false, false, false, 0f)
							{
								motion = new Vector2((float)Game1.random.Next(-10, 10) / 10f, -7f),
								acceleration = new Vector2(0f, 0.1f),
								alphaFade = 0.001f,
								drawAboveAlwaysFront = true
							});
						}
						i.jump(4f);
					}
				}
			}
		}

		// Token: 0x06002F79 RID: 12153 RVA: 0x00256F50 File Offset: 0x00255150
		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			foreach (FarmAnimal farmAnimal in this.marniesLivestock)
			{
				farmAnimal.draw(spriteBatch);
			}
			if (this.travelingMerchantDay)
			{
				Point cartOrigin = this.GetTravelingMerchantCartTile();
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((cartOrigin.X + 1) * 64), (float)((cartOrigin.Y - 2) * 64))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(142, 1382, 109, 70)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0768f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)(cartOrigin.X * 64), (float)(cartOrigin.Y * 64 + 32))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(112, 1424, 30, 24)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07681f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((cartOrigin.X + 1) * 64), (float)((cartOrigin.Y + 1) * 64 + 32 - 8))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(142, 1424, 16, 3)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07682f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((cartOrigin.X + 1) * 64 + 8), (float)(cartOrigin.Y * 64 - 32 - 8))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(71, 1966, 18, 18)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07678001f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)(cartOrigin.X * 64), (float)(cartOrigin.Y * 64 - 32))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(167, 1966, 18, 18)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07678001f);
				if (Game1.timeOfDay >= 2000)
				{
					spriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle((cartOrigin.X + 4) * 64 + 16, cartOrigin.Y * 64, 64, 64)), new Microsoft.Xna.Framework.Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.076840006f);
				}
			}
			if (Game1.player.achievements.Count > 0)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(this.hatterPos), new Microsoft.Xna.Framework.Rectangle?(this.hatterSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6016f);
			}
			if (!this.stumpFixed.Value && Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen") && !Game1.player.mailReceived.Contains("checkedRaccoonStump"))
			{
				float yOffset = -8f + 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3576f, 272f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.050400995f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3616f, 312f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12)), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.050409995f);
			}
			else if (this.numRaccoonBabies > 0)
			{
				for (int i = 0; i < Math.Min(this.numRaccoonBabies, 8); i++)
				{
					switch (i)
					{
					case 0:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(3706f, 340f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5000.0 < 200.0) ? 10 : 0), 472, 10, 9)), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0448f);
						break;
					case 1:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(54f, 4f) * 64f + new Vector2(8f, -12f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(235 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4500.0 < 200.0) ? 9 : 0), 472, 9, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0448f);
						break;
					case 2:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(3462f, 433f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 6000.0 < 200.0) ? 10 : 0), 472, 10, 9)), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0448f);
						break;
					case 3:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(58f, 4f) * 64f + new Vector2(4f, -20f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(235 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4800.0 < 200.0) ? 9 : 0), 472, 9, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448f);
						break;
					case 4:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(3770f, 408f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5000.0 < 200.0) ? 10 : 0), 472, 10, 9)), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0448f);
						break;
					case 5:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(55f, 3f) * 64f + new Vector2(12f, 4f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5000.0 < 200.0) ? 10 : 0), 472, 10, 9)), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0064f);
						break;
					case 6:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(56f, 3f) * 64f + new Vector2(40f, -8f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(213 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 5200.0 < 200.0) ? 10 : 0), 472, 10, 9)), Color.White, 0f, new Vector2(5.5f, 9f), 4f, SpriteEffects.None, 0.0064f);
						break;
					case 7:
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(58f, 3f) * 64f + new Vector2(-20f, -48f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(235 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4600.0 < 200.0) ? 9 : 0), 472, 9, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448f);
						break;
					}
				}
			}
			if (Game1.IsSpring && Game1.dayOfMonth == 17)
			{
				spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(52f, 97f) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(257, 108, 136, 116)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x0400202D RID: 8237
		public const string raccoonStumpCheckFlag = "checkedRaccoonStump";

		// Token: 0x0400202E RID: 8238
		public const string raccoontreeFlag = "raccoonTreeFallen";

		// Token: 0x0400202F RID: 8239
		public Microsoft.Xna.Framework.Rectangle MarnieLivestockArea = new Microsoft.Xna.Framework.Rectangle(94, 17, 10, 5);

		// Token: 0x04002030 RID: 8240
		[XmlIgnore]
		public readonly NetObjectList<FarmAnimal> marniesLivestock = new NetObjectList<FarmAnimal>();

		// Token: 0x04002031 RID: 8241
		[XmlIgnore]
		public readonly NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle> travelingMerchantBounds = new NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle>();

		// Token: 0x04002032 RID: 8242
		[XmlIgnore]
		public readonly NetBool netTravelingMerchantDay = new NetBool(false);

		// Token: 0x04002033 RID: 8243
		[XmlElement("log")]
		public ResourceClump obsolete_log;

		// Token: 0x04002034 RID: 8244
		[XmlElement("stumpFixed")]
		public readonly NetBool stumpFixed = new NetBool();

		// Token: 0x04002035 RID: 8245
		[XmlIgnore]
		public NetMutex derbyMutex = new NetMutex();

		// Token: 0x04002036 RID: 8246
		private int numRaccoonBabies = -1;

		// Token: 0x04002037 RID: 8247
		private int chimneyTimer = 500;

		// Token: 0x04002038 RID: 8248
		private bool hasShownCCUpgrade;

		// Token: 0x04002039 RID: 8249
		private Microsoft.Xna.Framework.Rectangle hatterSource = new Microsoft.Xna.Framework.Rectangle(600, 1957, 64, 32);

		// Token: 0x0400203A RID: 8250
		private Vector2 hatterPos = new Vector2(2056f, 6016f);
	}
}

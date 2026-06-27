using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Network;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002C0 RID: 704
	public class Beach : GameLocation
	{
		// Token: 0x06002DA9 RID: 11689 RVA: 0x0023A643 File Offset: 0x00238843
		public Beach()
		{
		}

		// Token: 0x06002DAA RID: 11690 RVA: 0x0023A661 File Offset: 0x00238861
		public Beach(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002DAB RID: 11691 RVA: 0x0023A684 File Offset: 0x00238884
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.bridgeFixed, "bridgeFixed").AddField(this.derbyMutex.NetFields, "derbyMutex.NetFields");
			this.bridgeFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					Beach.fixBridge(this);
				}
			};
			this.characters.OnValueAdded += delegate(NPC newCharacter)
			{
				this.adjustDerbyFisherman(newCharacter);
			};
		}

		// Token: 0x06002DAC RID: 11692 RVA: 0x0023A6F4 File Offset: 0x002388F4
		private void adjustDerbyFisherman(NPC npc)
		{
			if (npc.name.Equals("winter_derby_contestent0"))
			{
				AnimatedSprite sprite = npc.Sprite;
				if (((sprite != null) ? sprite.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 0, 16, 64);
				}
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("winter_derby_contestent1"))
			{
				AnimatedSprite sprite2 = npc.Sprite;
				if (((sprite2 != null) ? sprite2.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 2, 16, 64);
				}
				npc.Sprite.CurrentFrame = 2;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("winter_derby_contestent2"))
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
			if (npc.name.Equals("winter_derby_contestent3"))
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
			if (npc.name.Equals("winter_derby_contestent4"))
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
			if (npc.name.Equals("winter_derby_contestent5"))
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
			if (npc.name.Equals("winter_derby_contestent6"))
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
			if (npc.name.Equals("winter_derby_contestent7"))
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
			if (npc.name.Equals("winter_derby_contestent8"))
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
			if (npc.name.Equals("winter_derby_contestent9"))
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
			if (npc.name.Equals("winter_derby_contestent10"))
			{
				AnimatedSprite sprite11 = npc.Sprite;
				if (((sprite11 != null) ? sprite11.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 6, 16, 64);
				}
				npc.Sprite.CurrentFrame = 6;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
			if (npc.name.Equals("winter_derby_contestent11"))
			{
				AnimatedSprite sprite12 = npc.Sprite;
				if (((sprite12 != null) ? sprite12.Texture : null) == null)
				{
					npc.Sprite = new AnimatedSprite("Characters\\Assorted_Fishermen", 7, 16, 64);
				}
				npc.Sprite.CurrentFrame = 7;
				npc.drawOffset = new Vector2(0f, 96f);
				npc.shouldShadowBeOffset = true;
				npc.SimpleNonVillagerNPC = true;
				npc.HideShadow = true;
				npc.Breather = false;
			}
		}

		// Token: 0x06002DAD RID: 11693 RVA: 0x0023AC34 File Offset: 0x00238E34
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			this.derbyMutex.Update(this);
		}

		// Token: 0x06002DAE RID: 11694 RVA: 0x0023AC4C File Offset: 0x00238E4C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this.wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			NPC npc = this.oldMariner;
			if (npc != null)
			{
				npc.update(time, this);
			}
			if (!Game1.eventUp && Game1.random.NextDouble() < 1E-06)
			{
				Vector2 position = new Vector2((float)(Game1.random.Next(15, 47) * 64), (float)(Game1.random.Next(29, 42) * 64));
				bool draw = true;
				for (float i = position.Y / 64f; i < (float)this.map.RequireLayer("Back").LayerHeight; i += 1f)
				{
					if (!base.isWaterTile((int)position.X / 64, (int)i) || !base.isWaterTile((int)position.X / 64 - 1, (int)i) || !base.isWaterTile((int)position.X / 64 + 1, (int)i))
					{
						draw = false;
						break;
					}
				}
				if (draw)
				{
					this.temporarySprites.Add(new SeaMonsterTemporarySprite(250f, 4, Game1.random.Next(7), position));
				}
			}
		}

		// Token: 0x06002DAF RID: 11695 RVA: 0x0023AD64 File Offset: 0x00238F64
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			this.oldMariner = null;
			this.derbyMutex.ReleaseLock();
		}

		// Token: 0x06002DB0 RID: 11696 RVA: 0x0023AD80 File Offset: 0x00238F80
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			Microsoft.Xna.Framework.Rectangle tidePools = new Microsoft.Xna.Framework.Rectangle(65, 11, 25, 12);
			float chance = 1f;
			while (Game1.random.NextDouble() < (double)chance)
			{
				string id = (Game1.random.NextDouble() < 0.2) ? "(O)397" : "(O)393";
				Vector2 position = new Vector2((float)Game1.random.Next(tidePools.X, tidePools.Right), (float)Game1.random.Next(tidePools.Y, tidePools.Bottom));
				if (this.CanItemBePlacedHere(position, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					this.dropObject(ItemRegistry.Create<Object>(id, 1, 0, false), position * 64f, Game1.viewport, true, null);
				}
				chance /= 2f;
			}
			Microsoft.Xna.Framework.Rectangle seaweedShore = new Microsoft.Xna.Framework.Rectangle(66, 24, 19, 1);
			chance = 0.25f;
			while (Game1.random.NextDouble() < (double)chance)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					Vector2 position2 = new Vector2((float)Game1.random.Next(seaweedShore.X, seaweedShore.Right), (float)Game1.random.Next(seaweedShore.Y, seaweedShore.Bottom));
					if (this.CanItemBePlacedHere(position2, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						this.dropObject(ItemRegistry.Create<Object>("(O)152", 1, 0, false), position2 * 64f, Game1.viewport, true, null);
					}
				}
				chance /= 2f;
			}
			if (base.IsSummerHere() && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 14)
			{
				for (int i = 0; i < 5; i++)
				{
					this.spawnObjects();
				}
				chance = 1.5f;
				while (Game1.random.NextDouble() < (double)chance)
				{
					string id2 = (Game1.random.NextDouble() < 0.2) ? "(O)397" : "(O)393";
					Vector2 position3 = base.getRandomTile(null);
					position3.Y /= 2f;
					string prop = this.doesTileHaveProperty((int)position3.X, (int)position3.Y, "Type", "Back", false);
					if (this.CanItemBePlacedHere(position3, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && (prop == null || !prop.Equals("Wood")))
					{
						this.dropObject(ItemRegistry.Create<Object>(id2, 1, 0, false), position3 * 64f, Game1.viewport, true, null);
					}
					chance /= 1.1f;
				}
			}
			if (Game1.IsWinter)
			{
				this.characters.RemoveWhere((NPC npc) => npc.Name.Contains("derby_contestent"));
			}
		}

		// Token: 0x06002DB1 RID: 11697 RVA: 0x0023B051 File Offset: 0x00239251
		public void doneWithBridgeFix()
		{
			Game1.globalFadeToClear(null, 0.02f);
			Game1.viewportFreeze = false;
			Game1.freezeControls = false;
		}

		// Token: 0x06002DB2 RID: 11698 RVA: 0x0023B06C File Offset: 0x0023926C
		public void fadedForBridgeFix()
		{
			Game1.freezeControls = true;
			DelayedAction.playSoundAfterDelay("crafting", 1000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("crafting", 1500, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("crafting", 2000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("crafting", 2500, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("axchop", 3000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("Ship", 3200, null, null, -1, false);
			Game1.viewportFreeze = true;
			Game1.viewport.X = -10000;
			this.bridgeFixed.Value = true;
			Game1.pauseThenDoFunction(4000, new Game1.afterFadeFunction(this.doneWithBridgeFix));
			Beach.fixBridge(this);
		}

		// Token: 0x06002DB3 RID: 11699 RVA: 0x0023B160 File Offset: 0x00239360
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == "BeachBridge_Yes")
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.fadedForBridgeFix), 0.02f);
				Game1.player.Items.ReduceId("(O)388", 300);
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x06002DB4 RID: 11700 RVA: 0x0023B1B4 File Offset: 0x002393B4
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet");
			if (tileIndexAt != 284)
			{
				if (tileIndexAt == 496)
				{
					if (Game1.Date.TotalDays < 1)
					{
						Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Beach_GoneFishingMessage").Replace('\n', '^'));
						return false;
					}
				}
			}
			else if (who.Items.ContainsId("(O)388", 300))
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Question"), base.createYesNoResponses(), "BeachBridge");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Hint"));
			}
			if (this.oldMariner != null && this.oldMariner.TilePoint.X == tileLocation.X && this.oldMariner.TilePoint.Y == tileLocation.Y)
			{
				string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));
				if (!who.isMarriedOrRoommates() && who.specialItems.Contains("460") && !Utility.doesItemExistAnywhere("(O)460"))
				{
					who.specialItems.RemoveWhere((string id) => id == "460");
				}
				if (who.isMarriedOrRoommates())
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerMarried", playerTerm)));
				}
				else if (who.specialItems.Contains("460"))
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerHasItem", playerTerm)));
				}
				else if (who.hasAFriendWithHeartLevel(10, true, 2147483647) && who.houseUpgradeLevel.Value == 0)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNotUpgradedHouse", playerTerm)));
				}
				else if (who.hasAFriendWithHeartLevel(10, true, 2147483647))
				{
					Response[] answers = new Response[]
					{
						new Response("Buy", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerYes")),
						new Response("Not", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerNo"))
					};
					base.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_Question", playerTerm)), answers, "mariner");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNoRelationship", playerTerm)));
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002DB5 RID: 11701 RVA: 0x0023B446 File Offset: 0x00239646
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return (this.oldMariner != null && position.Intersects(this.oldMariner.GetBoundingBox())) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		// Token: 0x06002DB6 RID: 11702 RVA: 0x0023B478 File Offset: 0x00239678
		public override void checkForMusic(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.003 && Game1.timeOfDay < 1900)
			{
				base.localSound("seagulls", null, null, SoundContext.Default);
			}
			base.checkForMusic(time);
		}

		// Token: 0x06002DB7 RID: 11703 RVA: 0x0023B4CC File Offset: 0x002396CC
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (base.IsSummerHere() && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 14)
			{
				this.waterColor.Value = new Color(0, 255, 0) * 0.4f;
			}
		}

		// Token: 0x06002DB8 RID: 11704 RVA: 0x0023B51C File Offset: 0x0023971C
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (Game1.IsWinter && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 13)
			{
				Random r = Utility.CreateDaySaveRandom((double)(Game1.timeOfDay * 20), 0.0, 0.0);
				NPC i = base.getCharacterFromName("winter_derby_contestent" + r.Next(10).ToString());
				if (i != null)
				{
					i.shake(600);
					if (r.NextBool(0.25))
					{
						int whichSaying = r.Next(7);
						i.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerby_Exclamation" + whichSaying.ToString()), null, 2, 3000, 0);
						if (whichSaying == 0 || whichSaying == 6)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite(151, 1500f, 1, 1, i.Position, false, false, false, 0f)
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

		// Token: 0x06002DB9 RID: 11705 RVA: 0x0023B67C File Offset: 0x0023987C
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				this.hasShownCCUpgrade = false;
			}
			if (this.bridgeFixed.Value)
			{
				Beach.fixBridge(this);
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				Beach.showCommunityUpgradeShortcuts(this, ref this.hasShownCCUpgrade);
			}
			if (Game1.IsWinter && Game1.dayOfMonth >= 9 && Game1.dayOfMonth <= 11)
			{
				base.ApplyMapOverride(Game1.game1.xTileContent.Load<Map>("Maps\\Forest_FishingDerbySign"), "Forest_FishingDerbySign", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(15, 5, 2, 3)), new Action<Point>(base.cleanUpTileForMapOverride));
			}
			else if (this._appliedMapOverrides.Contains("Forest_FishingDerbySign"))
			{
				base.ApplyMapOverride("Beach_SquidFestSign_Revert", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(15, 5, 2, 3)));
				this._appliedMapOverrides.Remove("Forest_FishingDerbySign");
				this._appliedMapOverrides.Remove("Beach_SquidFestSign_Revert");
			}
			if (Game1.IsWinter && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 13)
			{
				if (base.getCharacterFromName("winter_derby_contestent0") == null && (Game1.IsMasterGame || !Game1.player.sleptInTemporaryBed.Value))
				{
					this.derbyMutex.RequestLock(delegate
					{
						if (base.getCharacterFromName("winter_derby_contestent0") == null)
						{
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(15, 17))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 0, 16, 64), new Vector2(15f, 17f) * 64f, -1, "winter_derby_contestent0", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(30, 21))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 2, 16, 64), new Vector2(30f, 21f) * 64f, -1, "winter_derby_contestent1", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(13, 39))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 3, 16, 64), new Vector2(13f, 39f) * 64f, -1, "winter_derby_contestent2", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(42, 25))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 1, 16, 64), new Vector2(42f, 25f) * 64f, -1, "winter_derby_contestent3", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(50, 25) && base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(51, 25))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 2, 32, 64), new Vector2(50f, 25f) * 64f, -1, "winter_derby_contestent4", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(56, 19))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 8, 32, 32), new Vector2(56f, 19f) * 64f, -1, "winter_derby_contestent5", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(11, 28))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 9, 32, 32), new Vector2(10f, 28f) * 64f, -1, "winter_derby_contestent6", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(14, 39))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 10, 32, 32), new Vector2(14f, 39f) * 64f, -1, "winter_derby_contestent7", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(90, 40))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 11, 32, 32), new Vector2(90f, 40f) * 64f, -1, "winter_derby_contestent8", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(8, 12))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 12, 32, 32), new Vector2(7f, 12f) * 64f, -1, "winter_derby_contestent9", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 0f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(47, 21))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 6, 16, 64), new Vector2(47f, 21f) * 64f, -1, "winter_derby_contestent10", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
							if (base.checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(22, 8))
							{
								this.characters.Add(new NPC(new AnimatedSprite("Characters\\Assorted_Fishermen_Winter", 7, 16, 64), new Vector2(22f, 8f) * 64f, -1, "winter_derby_contestent11", null)
								{
									Breather = false,
									HideShadow = true,
									drawOffset = new Vector2(0f, 96f),
									shouldShadowBeOffset = true,
									SimpleNonVillagerNPC = true
								});
							}
						}
						this.derbyMutex.ReleaseLock();
					}, null);
				}
				base.ApplyMapOverride(Game1.game1.xTileContent.Load<Map>("Maps\\Beach_SquidFest"), "Beach_SquidFest", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(11, 3, 16, 5)), new Action<Point>(base.cleanUpTileForMapOverride));
				if (Game1.dayOfMonth == 13)
				{
					string tileSheetId = GameLocation.GetAddedMapOverrideTilesheetId("Beach_SquidFest", "16");
					base.setMapTile(13, 6, 51, "Front", tileSheetId, null, true);
					base.setMapTile(13, 5, 43, "AlwaysFront", tileSheetId, null, true);
				}
				base.setFireplace(true, 48, 20, false, 0, 64);
				Game1.currentLightSources.Add(new LightSource("SquidFest_1", 1, new Vector2(732f, 480f), 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("SquidFest_2", 1, new Vector2(1064f, 368f), 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("SquidFest_3", 1, new Vector2(1692f, 476f), 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("SquidFest_4", 1, new Vector2(1372f, 476f), 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("SquidFest_5", 1, new Vector2(1532f, 380f), 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("SquidFest_6", 1, new Vector2(15.5f, 17.5f) * 64f, 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("SquidFest_7", 1, new Vector2(30.5f, 21f) * 64f, 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				return;
			}
			if (this._appliedMapOverrides.Contains("Beach_SquidFest") || base.getTileIndexAt(11, 7, "Buildings", "16") == 45)
			{
				base.ApplyMapOverride("Beach_SquidFest_Revert", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(11, 3, 16, 5)));
				this._appliedMapOverrides.Remove("Beach_SquidFest");
				this._appliedMapOverrides.Remove("Beach_SquidFest_Revert");
				this.characters.RemoveWhere((NPC npc) => npc.Name.Contains("derby_contestent"));
			}
		}

		// Token: 0x06002DBA RID: 11706 RVA: 0x0023BA84 File Offset: 0x00239C84
		public override void drawOverlays(SpriteBatch b)
		{
			if (Game1.IsWinter && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 13)
			{
				SpecialCurrencyDisplay.Draw(b, new Vector2(16f, 0f), (int)Game1.stats.Get(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year)), Game1.objectSpriteSheet, new Microsoft.Xna.Framework.Rectangle(112, 96, 16, 16));
			}
			base.drawOverlays(b);
		}

		// Token: 0x06002DBB RID: 11707 RVA: 0x0023BAF4 File Offset: 0x00239CF4
		protected override void resetLocalState()
		{
			base.resetLocalState();
			int numSeagulls = Game1.random.Next(6);
			foreach (Vector2 tile in Utility.getPositionsInClusterAroundThisTile(new Vector2((float)Game1.random.Next(this.map.DisplayWidth / 64), (float)Game1.random.Next(12, this.map.DisplayHeight / 64)), numSeagulls))
			{
				if (base.isTileOnMap(tile) && (this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || base.isWaterTile((int)tile.X, (int)tile.Y)) && (tile.X < 23f || tile.X > 46f))
				{
					int state = 3;
					if (base.isWaterTile((int)tile.X, (int)tile.Y) && this.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Passable", "Buildings", false) == null)
					{
						state = 2;
						if (Game1.random.NextBool())
						{
							continue;
						}
					}
					this.critters.Add(new Seagull(tile * 64f + new Vector2(32f, 32f), state));
				}
			}
			base.tryAddPrismaticButterfly();
			if (base.IsRainingHere() && Game1.timeOfDay < 1900)
			{
				this.oldMariner = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner", null)
				{
					AllowDynamicAppearance = false
				};
			}
		}

		// Token: 0x06002DBC RID: 11708 RVA: 0x0023BCB8 File Offset: 0x00239EB8
		public static void showCommunityUpgradeShortcuts(GameLocation location, ref bool flag)
		{
			if (!flag)
			{
				flag = true;
				location.warps.Add(new Warp(-1, 4, "Forest", 119, 35, false, false));
				location.warps.Add(new Warp(-1, 5, "Forest", 119, 35, false, false));
				location.warps.Add(new Warp(-1, 6, "Forest", 119, 36, false, false));
				location.warps.Add(new Warp(-1, 7, "Forest", 119, 36, false, false));
				for (int x = 0; x < 5; x++)
				{
					for (int y = 4; y < 7; y++)
					{
						location.removeTile(x, y, "Buildings");
					}
				}
				location.removeTile(7, 6, "Buildings");
				location.removeTile(5, 6, "Buildings");
				location.removeTile(6, 6, "Buildings");
				location.setMapTile(3, 7, 107, "Back", "untitled tile sheet", null, true);
				location.removeTile(67, 5, "Buildings");
				location.removeTile(67, 4, "Buildings");
				location.removeTile(67, 3, "Buildings");
				location.removeTile(67, 2, "Buildings");
				location.removeTile(67, 1, "Buildings");
				location.removeTile(67, 0, "Buildings");
				location.removeTile(66, 3, "Buildings");
				location.removeTile(68, 3, "Buildings");
			}
		}

		// Token: 0x06002DBD RID: 11709 RVA: 0x0023BE18 File Offset: 0x0023A018
		public static void fixBridge(GameLocation location)
		{
			if (!NetWorldState.checkAnywhereForWorldStateID("beachBridgeFixed"))
			{
				NetWorldState.addWorldStateIDEverywhere("beachBridgeFixed");
			}
			location.updateMap();
			location.setMapTile(58, 13, 301, "Buildings", "untitled tile sheet", null, true);
			location.setMapTile(59, 13, 301, "Buildings", "untitled tile sheet", null, true);
			location.setMapTile(60, 13, 301, "Buildings", "untitled tile sheet", null, true);
			location.setMapTile(61, 13, 301, "Buildings", "untitled tile sheet", null, true);
			location.removeTileProperty(58, 13, "Buildings", "Action");
			location.setMapTile(58, 14, 336, "Back", "untitled tile sheet", null, true);
			location.setMapTile(59, 14, 336, "Back", "untitled tile sheet", null, true);
			location.setMapTile(60, 14, 336, "Back", "untitled tile sheet", null, true);
			location.setMapTile(61, 14, 336, "Back", "untitled tile sheet", null, true);
		}

		// Token: 0x06002DBE RID: 11710 RVA: 0x0023BF38 File Offset: 0x0023A138
		public override void draw(SpriteBatch b)
		{
			NPC npc = this.oldMariner;
			if (npc != null)
			{
				npc.draw(b);
			}
			base.draw(b);
			if (!this.bridgeFixed.Value)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3704f, 720f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.095401f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3744f, 760f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12)), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.09541f);
			}
		}

		// Token: 0x04001F54 RID: 8020
		private NPC oldMariner;

		// Token: 0x04001F55 RID: 8021
		[XmlElement("bridgeFixed")]
		public readonly NetBool bridgeFixed = new NetBool();

		// Token: 0x04001F56 RID: 8022
		[XmlIgnore]
		public NetMutex derbyMutex = new NetMutex();

		// Token: 0x04001F57 RID: 8023
		private bool hasShownCCUpgrade;
	}
}

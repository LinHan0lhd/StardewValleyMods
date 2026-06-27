using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.WorldMaps;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002DE RID: 734
	public class IslandSouth : IslandLocation
	{
		// Token: 0x06003069 RID: 12393 RVA: 0x00263AA8 File Offset: 0x00261CA8
		public IslandSouth()
		{
		}

		// Token: 0x0600306A RID: 12394 RVA: 0x00263B34 File Offset: 0x00261D34
		public IslandSouth(string map, string name) : base(map, name)
		{
			this.largeTerrainFeatures.Add(new Bush(new Vector2(31f, 5f), 4, this, -1));
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(17, 22), new Microsoft.Xna.Framework.Rectangle(12, 18, 14, 7), 20, delegate()
			{
				Game1.addMailForTomorrow("Island_Resort", true, true);
				this.resortRestored.Value = true;
			}, () => this.resortRestored.Value, "Resort", "Island_UpgradeHouse"));
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(5, 9), new Microsoft.Xna.Framework.Rectangle(1, 10, 3, 4), 10, delegate()
			{
				Game1.addMailForTomorrow("Island_Turtle", true, true);
				this.westernTurtleMoved.Value = true;
				this.moveTurtleEvent.Fire();
			}, () => this.westernTurtleMoved.Value, "Turtle", "Island_FirstParrot"));
		}

		// Token: 0x0600306B RID: 12395 RVA: 0x00263C74 File Offset: 0x00261E74
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.resortRestored, "resortRestored").AddField(this.westernTurtleMoved, "westernTurtleMoved").AddField(this.shouldToggleResort, "shouldToggleResort").AddField(this.resortOpenToday, "resortOpenToday").AddField(this.moveTurtleEvent, "moveTurtleEvent");
			this.resortRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyResortRestore();
				}
			};
			this.moveTurtleEvent.onEvent += this.ApplyWesternTurtleMove;
		}

		// Token: 0x0600306C RID: 12396 RVA: 0x00263D0C File Offset: 0x00261F0C
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			IslandSouth location = l as IslandSouth;
			if (location != null)
			{
				this.resortRestored.Value = location.resortRestored.Value;
				this.westernTurtleMoved.Value = location.westernTurtleMoved.Value;
				this.shouldToggleResort.Value = location.shouldToggleResort.Value;
				this.resortOpenToday.Value = location.resortOpenToday.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x0600306D RID: 12397 RVA: 0x00263D84 File Offset: 0x00261F84
		public override void DayUpdate(int dayOfMonth)
		{
			if (this.shouldToggleResort.Value)
			{
				this.resortOpenToday.Value = !this.resortOpenToday.Value;
				this.shouldToggleResort.Value = false;
				this.ApplyResortRestore();
			}
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x0600306E RID: 12398 RVA: 0x00263DD0 File Offset: 0x00261FD0
		public void ApplyResortRestore()
		{
			if (this.map != null)
			{
				base.ApplyUnsafeMapOverride("Island_Resort", null, new Microsoft.Xna.Framework.Rectangle(9, 15, 26, 16));
			}
			base.removeTile(new Location(41, 28), "Buildings");
			base.removeTile(new Location(42, 28), "Buildings");
			base.removeTile(new Location(42, 29), "Buildings");
			base.removeTile(new Location(42, 30), "Front");
			base.removeTileProperty(42, 30, "Back", "Passable");
			if (this.resortRestored.Value)
			{
				if (this.resortOpenToday.Value)
				{
					base.removeTile(new Location(22, 21), "Buildings");
					base.removeTile(new Location(22, 22), "Buildings");
					base.removeTile(new Location(24, 21), "Buildings");
					base.removeTile(new Location(24, 22), "Buildings");
					return;
				}
				base.setMapTile(22, 21, 1405, "Buildings", "untitled tile sheet", null, true);
				base.setMapTile(22, 22, 1437, "Buildings", "untitled tile sheet", null, true);
				base.setMapTile(24, 21, 1405, "Buildings", "untitled tile sheet", null, true);
				base.setMapTile(24, 22, 1437, "Buildings", "untitled tile sheet", null, true);
			}
		}

		// Token: 0x0600306F RID: 12399 RVA: 0x00263F48 File Offset: 0x00262148
		public void ApplyWesternTurtleMove()
		{
			TemporaryAnimatedSprite t = base.getTemporarySpriteByID(789);
			if (t != null)
			{
				t.motion = new Vector2(-2f, 0f);
				t.yPeriodic = true;
				t.yPeriodicRange = 8f;
				t.yPeriodicLoopTime = 300f;
				t.shakeIntensity = 1f;
			}
			base.localSound("shadowDie", null, null, SoundContext.Default);
		}

		// Token: 0x06003070 RID: 12400 RVA: 0x00263FC0 File Offset: 0x002621C0
		private void parrotBoyLands(int extra)
		{
			TemporaryAnimatedSprite v = base.getTemporarySpriteByID(888);
			if (v != null)
			{
				v.sourceRect.X = 0;
				v.sourceRect.Y = 32;
				v.sourceRectStartingPos.X = 0f;
				v.sourceRectStartingPos.Y = 32f;
				v.motion = new Vector2(4f, 0f);
				v.acceleration = Vector2.Zero;
				v.id = 888;
				v.animationLength = 4;
				v.interval = 100f;
				v.totalNumberOfLoops = 10;
				v.drawAboveAlwaysFront = false;
				v.layerDepth = 0.1f;
				this.temporarySprites.Add(v);
			}
		}

		// Token: 0x06003071 RID: 12401 RVA: 0x0026407C File Offset: 0x0026227C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.moveTurtleEvent.Poll();
			if (this.boatLight != null)
			{
				this.boatLight.position.Value = new Vector2(3f, 1f) * 64f + this.GetBoatPosition();
			}
			if (this.boatStringLight != null)
			{
				this.boatStringLight.position.Value = new Vector2(3f, 4f) * 64f + this.GetBoatPosition();
			}
			if (this._parrotBoyHiding && Utility.isThereAFarmerWithinDistance(new Vector2(29f, 16f), 4, this) == Game1.player)
			{
				TemporaryAnimatedSprite v = base.getTemporarySpriteByID(777);
				if (v != null)
				{
					v.sourceRect.X = 0;
					v.sourceRectStartingPos.X = 0f;
					v.motion = new Vector2(3f, -10f);
					v.acceleration = new Vector2(0f, 0.4f);
					v.yStopCoordinate = 992;
					v.shakeIntensity = 2f;
					v.id = 888;
					v.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.parrotBoyLands);
					base.localSound("parrot_squawk", null, null, SoundContext.Default);
				}
			}
			if (!this._exitsBlocked && !this._sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(18f, 11f), 5, this) == Game1.player)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_South", true, false);
				TemporaryAnimatedSprite v2 = base.getTemporarySpriteByID(999);
				if (v2 != null)
				{
					v2.yPeriodic = false;
					v2.xPeriodic = false;
					v2.sourceRect.Y = 0;
					v2.sourceRectStartingPos.Y = 0f;
					v2.motion = new Vector2(0f, -4f);
					v2.acceleration = new Vector2(0f, -0.04f);
				}
				base.localSound("magma_sprite_spot", null, null, SoundContext.Default);
				v2 = base.getTemporarySpriteByID(998);
				if (v2 != null)
				{
					v2.yPeriodic = false;
					v2.xPeriodic = false;
					v2.motion = new Vector2(0f, -4f);
					v2.acceleration = new Vector2(0f, -0.04f);
				}
				this._sawFlameSprite = true;
			}
			Event currentEvent = this.currentEvent;
			if (((currentEvent != null) ? currentEvent.id : null) == "-157039427")
			{
				if (this._boatDirection != 0)
				{
					this._boatOffset += this._boatDirection;
					foreach (NPC npc in this.currentEvent.actors)
					{
						npc.shouldShadowBeOffset = true;
						npc.drawOffset.Y = (float)this._boatOffset;
					}
					foreach (Farmer farmer in this.currentEvent.farmerActors)
					{
						farmer.shouldShadowBeOffset = true;
						farmer.drawOffset.Y = (float)this._boatOffset;
					}
				}
				if ((float)this._boatDirection != 0f)
				{
					if (this._nextBubble > 0f)
					{
						this._nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
					}
					else
					{
						Microsoft.Xna.Framework.Rectangle back_rectangle = new Microsoft.Xna.Framework.Rectangle(64, 256, 192, 64);
						back_rectangle.X += (int)this.GetBoatPosition().X;
						back_rectangle.Y += (int)this.GetBoatPosition().Y;
						Vector2 position = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
						TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, position, false, false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f, false);
						sprite.acceleration = new Vector2(0f, -0.25f * (float)Math.Sign(this._boatDirection));
						this.temporarySprites.Add(sprite);
						this._nextBubble = 0.01f;
					}
					if (this._nextSlosh > 0f)
					{
						this._nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
					}
					else
					{
						Game1.playSound("waterSlosh", null);
						this._nextSlosh = 0.5f;
					}
				}
				if (this._nextSmoke > 0f)
				{
					this._nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
					return;
				}
				Vector2 position2 = new Vector2(2f, 2.5f) * 64f + this.GetBoatPosition();
				TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position2, false, false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f, false);
				sprite2.acceleration = new Vector2(-0.25f, -0.15f);
				this.temporarySprites.Add(sprite2);
				this._nextSmoke = 0.2f;
			}
		}

		// Token: 0x06003072 RID: 12402 RVA: 0x00264618 File Offset: 0x00262818
		public override void cleanupBeforePlayerExit()
		{
			this.boatLight = null;
			this.boatStringLight = null;
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06003073 RID: 12403 RVA: 0x00264630 File Offset: 0x00262830
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return (this._exitsBlocked && position.Intersects(this.turtle1Spot)) || (!this.westernTurtleMoved.Value && position.Intersects(this.turtle2Spot)) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		// Token: 0x06003074 RID: 12404 RVA: 0x00264684 File Offset: 0x00262884
		public override bool isTilePlaceable(Vector2 tileLocation, bool itemIsPassable = false)
		{
			Point non_tile_position = Utility.Vector2ToPoint((tileLocation + new Vector2(0.5f, 0.5f)) * 64f);
			return (!this._exitsBlocked || !this.turtle1Spot.Contains(non_tile_position)) && (this.westernTurtleMoved.Value || !this.turtle2Spot.Contains(non_tile_position)) && base.isTilePlaceable(tileLocation, itemIsPassable);
		}

		// Token: 0x06003075 RID: 12405 RVA: 0x002646F3 File Offset: 0x002628F3
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (this.resortRestored.Value)
			{
				this.ApplyResortRestore();
			}
		}

		// Token: 0x06003076 RID: 12406 RVA: 0x00264710 File Offset: 0x00262910
		protected override void resetLocalState()
		{
			this._isFirstVisit = false;
			if (!Game1.player.hasOrWillReceiveMail("Visited_Island"))
			{
				WorldMapManager.ReloadData();
				Game1.addMailForTomorrow("Visited_Island", true, false);
				this._isFirstVisit = true;
			}
			Game1.getAchievement(40, true);
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_South"))
			{
				this._sawFlameSprite = true;
			}
			this._exitsBlocked = !Game1.MasterPlayer.hasOrWillReceiveMail("Island_FirstParrot");
			this.boatLight = new LightSource("IslandSouth_BoatLight", 4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName);
			this.boatStringLight = new LightSource("IslandSouth_BoatStringLight", 4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName);
			Game1.currentLightSources.Add(this.boatLight);
			Game1.currentLightSources.Add(this.boatStringLight);
			base.resetLocalState();
			this.boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
			if (Game1.random.NextDouble() < 0.25 || this._isFirstVisit)
			{
				base.addCritter(new CrabCritter(new Vector2(37f, 30f) * 64f));
			}
			if (this._isFirstVisit)
			{
				base.addCritter(new CrabCritter(new Vector2(21f, 35f) * 64f));
				base.addCritter(new CrabCritter(new Vector2(21f, 36f) * 64f));
				base.addCritter(new CrabCritter(new Vector2(35f, 31f) * 64f));
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy"))
				{
					this._parrotBoyHiding = true;
					this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\ParrotBoy", new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 32), new Vector2(29f, 15.5f) * 64f, false, 0f, Color.White)
					{
						id = 777,
						scale = 4f,
						totalNumberOfLoops = 99999,
						interval = 9999f,
						animationLength = 1,
						layerDepth = 1f,
						drawAboveAlwaysFront = true
					});
				}
			}
			if (this._exitsBlocked)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(208, 94, 48, 53), new Vector2(17f, 0f) * 64f, false, 0f, Color.White)
				{
					id = 555,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 0.001f
				});
			}
			else if (!this._sawFlameSprite)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 16, 16, 16), new Vector2(18f, 11f) * 64f, false, 0f, Color.White)
				{
					id = 999,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 70f,
					lightId = "IslandSouth_FlameSpirit",
					lightRadius = 1f,
					animationLength = 7,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicRange = 12f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(18.2f, 12.4f) * 64f, false, 0f, Color.White)
				{
					id = 998,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 1000f,
					animationLength = 1,
					layerDepth = 0.001f,
					yPeriodic = true,
					yPeriodicRange = 1f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
			}
			if (!this.westernTurtleMoved.Value)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(152, 101, 56, 40), new Vector2(0.5f, 10f) * 64f, false, 0f, Color.White)
				{
					id = 789,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 0.001f
				});
			}
			if (base.AreMoonlightJelliesOut())
			{
				base.addMoonlightJellies(50, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0, 0.0, 0.0), new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			this.ResetBoat();
		}

		// Token: 0x06003077 RID: 12407 RVA: 0x00264C98 File Offset: 0x00262E98
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (tileLocation.X == 14 && tileLocation.Y == 22)
			{
				Microsoft.Xna.Framework.Rectangle shopArea = new Microsoft.Xna.Framework.Rectangle(14, 21, 1, 1);
				if (Utility.TryOpenShopMenu("ResortBar", this, new Microsoft.Xna.Framework.Rectangle?(shopArea), null, false, true, null))
				{
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06003078 RID: 12408 RVA: 0x00264CF0 File Offset: 0x00262EF0
		public static bool CanVisitIslandToday(NPC npc)
		{
			if (!npc.IsVillager || !npc.CanSocialize || npc.daysUntilNotInvisible > 0 || npc.IsInvisible)
			{
				return false;
			}
			CharacterData data = npc.GetData();
			if (!GameStateQuery.CheckConditions((data != null) ? data.CanVisitIsland : null, npc.currentLocation, null, null, null, null, null))
			{
				return false;
			}
			GameLocation currentLocation = npc.currentLocation;
			return !(((currentLocation != null) ? currentLocation.NameOrUniqueName : null) == "Farm") && !Utility.IsHospitalVisitDay(npc.Name);
		}

		// Token: 0x06003079 RID: 12409 RVA: 0x00264D78 File Offset: 0x00262F78
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer == "LeaveIsland_Yes")
			{
				this.Depart();
				return true;
			}
			if (!(questionAndAnswer == "ToggleResort_Yes"))
			{
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			this.shouldToggleResort.Value = !this.shouldToggleResort.Value;
			bool open = this.resortOpenToday.Value;
			if (this.shouldToggleResort.Value)
			{
				open = !open;
			}
			if (open)
			{
				Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillOpenSign"));
			}
			else
			{
				Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillCloseSign"));
			}
			return true;
		}

		// Token: 0x0600307A RID: 12410 RVA: 0x00264E1C File Offset: 0x0026301C
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "ResortSign")
			{
				string key;
				if (this.resortOpenToday.Value)
				{
					key = (this.shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortOpenWillCloseSign" : "Strings\\Locations:IslandSouth_ResortOpenSign");
				}
				else
				{
					key = (this.shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortClosedWillOpenSign" : "Strings\\Locations:IslandSouth_ResortClosedSign");
				}
				base.createQuestionDialogue(Game1.content.LoadString(key), base.createYesNoResponses(), "ToggleResort");
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x0600307B RID: 12411 RVA: 0x00264EA8 File Offset: 0x002630A8
		public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
		{
			if (this.IgnoreTouchActions())
			{
				return;
			}
			if (ArgUtility.Get(action, 0, null, true) == "LeaveIsland")
			{
				Response[] returnOptions = new Response[]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Locations:Desert_Return_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Locations:Desert_Return_No"))
				};
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Return_Question"), returnOptions, "LeaveIsland");
				return;
			}
			base.performTouchAction(action, playerStandingPosition);
		}

		// Token: 0x0600307C RID: 12412 RVA: 0x00264F36 File Offset: 0x00263136
		public void Depart()
		{
			Game1.globalFadeToBlack(delegate
			{
				this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:IslandDepart"), "Data\\Events\\IslandSouth", "-157039427", Game1.player);
				Game1.eventUp = true;
			}, 0.02f);
		}

		// Token: 0x0600307D RID: 12413 RVA: 0x00264F4E File Offset: 0x0026314E
		public static Point GetDressingRoomPoint(NPC character)
		{
			if (character.Gender == Gender.Female)
			{
				return new Point(22, 19);
			}
			return new Point(24, 19);
		}

		// Token: 0x0600307E RID: 12414 RVA: 0x00264F6C File Offset: 0x0026316C
		public override bool HasLocationOverrideDialogue(NPC character)
		{
			Friendship friendship;
			return (!Game1.player.friendshipData.TryGetValue(character.Name, out friendship) || !friendship.IsDivorced()) && character.islandScheduleName.Value != null;
		}

		// Token: 0x0600307F RID: 12415 RVA: 0x00264FAC File Offset: 0x002631AC
		public override string GetLocationOverrideDialogue(NPC character)
		{
			if (Game1.timeOfDay < 1200 || (!character.shouldWearIslandAttire.Value && Game1.timeOfDay < 1730 && IslandSouth.HasIslandAttire(character)))
			{
				string dialogue_key = "Characters\\Dialogue\\" + character.Name + ":Resort_Entering";
				if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key, true) != null)
				{
					return dialogue_key;
				}
			}
			if (Game1.timeOfDay >= 1800)
			{
				string dialogue_key2 = "Characters\\Dialogue\\" + character.Name + ":Resort_Leaving";
				if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key2, true) != null)
				{
					return dialogue_key2;
				}
			}
			return "Characters\\Dialogue\\" + character.Name + ":Resort";
		}

		// Token: 0x06003080 RID: 12416 RVA: 0x00265054 File Offset: 0x00263254
		public static bool HasIslandAttire(NPC character)
		{
			try
			{
				Game1.temporaryContent.Load<Texture2D>("Characters\\" + NPC.getTextureNameForCharacter(character.name.Value) + "_Beach");
				if (((character != null) ? character.Name : null) == "Lewis")
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						if (((farmer != null) ? farmer.activeDialogueEvents : null) != null && farmer.activeDialogueEvents.ContainsKey("lucky_pants_lewis"))
						{
							return true;
						}
					}
					return false;
				}
				return true;
			}
			catch (Exception)
			{
			}
			return false;
		}

		// Token: 0x06003081 RID: 12417 RVA: 0x0026511C File Offset: 0x0026331C
		public static void SetupIslandSchedules()
		{
			Game1.netWorldState.Value.IslandVisitors.Clear();
			if (Utility.isFestivalDay() || Utility.IsPassiveFestivalDay())
			{
				return;
			}
			IslandSouth island = Game1.getLocationFromName("IslandSouth") as IslandSouth;
			if (island == null || !island.resortRestored.Value)
			{
				return;
			}
			if (island.IsRainingHere())
			{
				return;
			}
			if (!island.resortOpenToday.Value)
			{
				return;
			}
			Random seeded_random = Utility.CreateRandom(Game1.uniqueIDForThisGame * 1.21, Game1.stats.DaysPlayed * 2.5, 0.0, 0.0, 0.0);
			List<NPC> valid_visitors = new List<NPC>();
			Utility.ForEachVillager(delegate(NPC npc)
			{
				if (IslandSouth.CanVisitIslandToday(npc))
				{
					valid_visitors.Add(npc);
				}
				return true;
			}, false);
			List<NPC> visitors = new List<NPC>();
			if (seeded_random.NextDouble() < 0.4)
			{
				for (int i = 0; i < 5; i++)
				{
					NPC visitor = seeded_random.ChooseFrom(valid_visitors);
					if (visitor != null && visitor.age.Value != 2)
					{
						valid_visitors.Remove(visitor);
						visitors.Add(visitor);
						visitor.scheduleDelaySeconds = Math.Min((float)i * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
					}
				}
			}
			else
			{
				List<string>[] potentialGroups = new List<string>[]
				{
					new List<string>
					{
						"Sebastian",
						"Sam",
						"Abigail"
					},
					new List<string>
					{
						"Jodi",
						"Kent",
						"Vincent",
						"Sam"
					},
					new List<string>
					{
						"Jodi",
						"Vincent",
						"Sam"
					},
					new List<string>
					{
						"Pierre",
						"Caroline",
						"Abigail"
					},
					new List<string>
					{
						"Robin",
						"Demetrius",
						"Maru",
						"Sebastian"
					},
					new List<string>
					{
						"Lewis",
						"Marnie"
					},
					new List<string>
					{
						"Marnie",
						"Shane",
						"Jas"
					},
					new List<string>
					{
						"Penny",
						"Jas",
						"Vincent"
					},
					new List<string>
					{
						"Pam",
						"Penny"
					},
					new List<string>
					{
						"Caroline",
						"Marnie",
						"Robin",
						"Jodi"
					},
					new List<string>
					{
						"Haley",
						"Penny",
						"Leah",
						"Emily",
						"Maru",
						"Abigail"
					},
					new List<string>
					{
						"Alex",
						"Sam",
						"Sebastian",
						"Elliott",
						"Shane",
						"Harvey"
					}
				};
				List<string> group = potentialGroups[seeded_random.Next(potentialGroups.Length)];
				bool failed = false;
				foreach (string s in group)
				{
					if (!valid_visitors.Contains(Game1.getCharacterFromName(s, true, false)))
					{
						failed = true;
						break;
					}
				}
				if (!failed)
				{
					int j = 0;
					foreach (string name in group)
					{
						NPC visitor2 = Game1.getCharacterFromName(name, true, false);
						valid_visitors.Remove(visitor2);
						visitors.Add(visitor2);
						visitor2.scheduleDelaySeconds = Math.Min((float)j * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
						j++;
					}
				}
				for (int k = 0; k < 5 - visitors.Count; k++)
				{
					NPC visitor3 = seeded_random.ChooseFrom(valid_visitors);
					if (visitor3 != null && visitor3.age.Value != 2)
					{
						valid_visitors.Remove(visitor3);
						visitors.Add(visitor3);
						visitor3.scheduleDelaySeconds = Math.Min((float)k * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
					}
				}
			}
			List<IslandSouth.IslandActivityAssigments> activities = new List<IslandSouth.IslandActivityAssigments>();
			Dictionary<Character, string> last_activity_assignments = new Dictionary<Character, string>();
			activities.Add(new IslandSouth.IslandActivityAssigments(1200, visitors, seeded_random, last_activity_assignments));
			activities.Add(new IslandSouth.IslandActivityAssigments(1400, visitors, seeded_random, last_activity_assignments));
			activities.Add(new IslandSouth.IslandActivityAssigments(1600, visitors, seeded_random, last_activity_assignments));
			foreach (NPC visitor4 in visitors)
			{
				StringBuilder schedule = new StringBuilder("");
				bool should_dress = IslandSouth.HasIslandAttire(visitor4);
				bool had_first_activity = false;
				if (should_dress)
				{
					Point dressing_room = IslandSouth.GetDressingRoomPoint(visitor4);
					schedule.Append(string.Concat(new string[]
					{
						"/a1150 IslandSouth ",
						dressing_room.X.ToString(),
						" ",
						dressing_room.Y.ToString(),
						" change_beach"
					}));
					had_first_activity = true;
				}
				foreach (IslandSouth.IslandActivityAssigments islandActivityAssigments in activities)
				{
					string current_string = islandActivityAssigments.GetScheduleStringForCharacter(visitor4);
					if (current_string != "")
					{
						if (!had_first_activity)
						{
							current_string = "/a" + current_string.Substring(1);
							had_first_activity = true;
						}
						schedule.Append(current_string);
					}
				}
				if (should_dress)
				{
					Point dressing_room2 = IslandSouth.GetDressingRoomPoint(visitor4);
					schedule.Append(string.Concat(new string[]
					{
						"/a1730 IslandSouth ",
						dressing_room2.X.ToString(),
						" ",
						dressing_room2.Y.ToString(),
						" change_normal"
					}));
				}
				if (visitor4.Name == "Gus")
				{
					schedule.Append("/1800 Saloon 10 18 2/2430 bed");
				}
				else
				{
					schedule.Append("/1800 bed");
				}
				schedule.Remove(0, 1);
				if (visitor4.TryLoadSchedule("island", schedule.ToString()))
				{
					visitor4.islandScheduleName.Value = "island";
					Game1.netWorldState.Value.IslandVisitors.Add(visitor4.Name);
				}
				visitor4.performSpecialScheduleChanges();
			}
		}

		// Token: 0x06003082 RID: 12418 RVA: 0x0026588C File Offset: 0x00263A8C
		public virtual void ResetBoat()
		{
			this.boatPosition = new Vector2(14f, 37f) * 64f;
			this._boatOffset = 0;
			this._boatDirection = 0;
			this._nextBubble = 0f;
			this._nextSmoke = 0f;
			this._nextSlosh = 0f;
		}

		// Token: 0x06003083 RID: 12419 RVA: 0x002658E7 File Offset: 0x00263AE7
		public Vector2 GetBoatPosition()
		{
			return this.boatPosition + new Vector2(0f, (float)this._boatOffset);
		}

		// Token: 0x06003084 RID: 12420 RVA: 0x00265908 File Offset: 0x00263B08
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Vector2 boat_position = this.GetBoatPosition();
			b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 0, 96, 208)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.boatPosition.Y + 320f) / 10000f);
			b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(288, 0, 96, 208)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.boatPosition.Y + 616f) / 10000f);
			Event currentEvent = this.currentEvent;
			if (((currentEvent != null) ? currentEvent.id : null) != "-157039427")
			{
				b.Draw(this.boatTexture, Game1.GlobalToLocal(new Vector2(1184f, 2752f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 208, 32, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.272f);
			}
		}

		// Token: 0x06003085 RID: 12421 RVA: 0x00265A3D File Offset: 0x00263C3D
		public override bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			if (command_string == "boat_reset")
			{
				this.ResetBoat();
				return true;
			}
			if (!(command_string == "boat_depart"))
			{
				return false;
			}
			this._boatDirection = 1;
			return this._boatOffset >= 100;
		}

		// Token: 0x0400209F RID: 8351
		[XmlIgnore]
		protected int _boatDirection;

		// Token: 0x040020A0 RID: 8352
		[XmlIgnore]
		public Texture2D boatTexture;

		// Token: 0x040020A1 RID: 8353
		[XmlIgnore]
		public Vector2 boatPosition;

		// Token: 0x040020A2 RID: 8354
		[XmlIgnore]
		protected int _boatOffset;

		// Token: 0x040020A3 RID: 8355
		[XmlIgnore]
		protected float _nextBubble;

		// Token: 0x040020A4 RID: 8356
		[XmlIgnore]
		protected float _nextSlosh;

		// Token: 0x040020A5 RID: 8357
		[XmlIgnore]
		protected float _nextSmoke;

		// Token: 0x040020A6 RID: 8358
		[XmlIgnore]
		public LightSource boatLight;

		// Token: 0x040020A7 RID: 8359
		[XmlIgnore]
		public LightSource boatStringLight;

		// Token: 0x040020A8 RID: 8360
		[XmlElement("shouldToggleResort")]
		public readonly NetBool shouldToggleResort = new NetBool(false);

		// Token: 0x040020A9 RID: 8361
		[XmlElement("resortOpenToday")]
		public readonly NetBool resortOpenToday = new NetBool(true);

		// Token: 0x040020AA RID: 8362
		[XmlElement("resortRestored")]
		public readonly NetBool resortRestored = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x040020AB RID: 8363
		[XmlElement("westernTurtleMoved")]
		public readonly NetBool westernTurtleMoved = new NetBool();

		// Token: 0x040020AC RID: 8364
		[XmlIgnore]
		protected bool _parrotBoyHiding;

		// Token: 0x040020AD RID: 8365
		[XmlIgnore]
		protected bool _isFirstVisit;

		// Token: 0x040020AE RID: 8366
		[XmlIgnore]
		protected bool _exitsBlocked;

		// Token: 0x040020AF RID: 8367
		[XmlIgnore]
		protected bool _sawFlameSprite;

		// Token: 0x040020B0 RID: 8368
		[XmlIgnore]
		public NetEvent0 moveTurtleEvent = new NetEvent0(false);

		// Token: 0x040020B1 RID: 8369
		private Microsoft.Xna.Framework.Rectangle turtle1Spot = new Microsoft.Xna.Framework.Rectangle(1088, 0, 192, 192);

		// Token: 0x040020B2 RID: 8370
		private Microsoft.Xna.Framework.Rectangle turtle2Spot = new Microsoft.Xna.Framework.Rectangle(0, 640, 256, 256);

		// Token: 0x0200065F RID: 1631
		public class IslandActivityAssigments
		{
			// Token: 0x06004526 RID: 17702 RVA: 0x0031E26C File Offset: 0x0031C46C
			public IslandActivityAssigments(int time, List<NPC> visitors, Random seeded_random, Dictionary<Character, string> last_activity_assignments)
			{
				this.activityTime = time;
				this.visitors = new List<NPC>(visitors);
				this.random = seeded_random;
				Utility.Shuffle<NPC>(this.random, this.visitors);
				this.animationDescriptions = DataLoader.AnimationDescriptions(Game1.content);
				this.FindActivityForCharacters(last_activity_assignments);
			}

			// Token: 0x06004527 RID: 17703 RVA: 0x0031E468 File Offset: 0x0031C668
			public virtual void FindActivityForCharacters(Dictionary<Character, string> last_activity_assignments)
			{
				this.currentAssignments = new Dictionary<Character, string>();
				this.currentAnimationAssignments = new Dictionary<Character, string>();
				foreach (NPC character in this.visitors)
				{
					if (!this.currentAssignments.ContainsKey(character))
					{
						string name = character.Name;
						if (!(name == "Gus"))
						{
							if (!(name == "Sam"))
							{
								continue;
							}
						}
						else
						{
							this.currentAssignments[character] = "14 21 2";
							using (List<NPC>.Enumerator enumerator2 = this.visitors.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									NPC other_character = enumerator2.Current;
									if (!this.currentAssignments.ContainsKey(other_character) && other_character.Age != 2)
									{
										this.TryAssignment(other_character, this.drinkPoints, "Resort_Bar", other_character.name.Value.ToLower() + "_beach_drink", false, 0.5, last_activity_assignments);
									}
								}
								continue;
							}
						}
						if (this.TryAssignment(character, this.towelLoungePoints, "Resort_Towel", character.name.Value.ToLower() + "_beach_towel", true, 0.5, last_activity_assignments))
						{
							foreach (NPC other_character2 in this.visitors)
							{
								if (!this.currentAssignments.ContainsKey(other_character2) && this.animationDescriptions.ContainsKey(other_character2.Name.ToLower() + "_beach_dance"))
								{
									string[] array = ArgUtility.SplitBySpace(this.currentAssignments[character]);
									int x = int.Parse(array[0]);
									int y = int.Parse(array[1]);
									this.currentAssignments.Remove(other_character2);
									this.TryAssignment(other_character2, new List<Point>(new Point[]
									{
										new Point(x + 1, y + 1)
									}), "Resort_Dance", other_character2.Name.ToLower() + "_beach_dance", true, 1.0, last_activity_assignments);
									other_character2.currentScheduleDelay = 0f;
									character.currentScheduleDelay = 0f;
									break;
								}
							}
						}
					}
				}
				foreach (NPC character2 in this.visitors)
				{
					if (!this.currentAssignments.ContainsKey(character2) && !this.TryAssignment(character2, this.towelLoungePoints, "Resort_Towel", character2.name.Value.ToLower() + "_beach_towel", true, 0.5, last_activity_assignments) && !this.TryAssignment(character2, this.wanderPoints, "Resort_Wander", "square_3_3", false, 0.4, last_activity_assignments) && !this.TryAssignment(character2, this.umbrellaPoints, "Resort_Umbrella", character2.name.Value.ToLower() + "_beach_umbrella", true, (character2.Name == "Abigail") ? 0.5 : 0.1, null) && (character2.Age != 0 || !this.TryAssignment(character2, this.chairPoints, "Resort_Chair", "_beach_chair", false, 0.4, last_activity_assignments)))
					{
						this.TryAssignment(character2, this.shoreLoungePoints, "Resort_Shore", null, false, 1.0, last_activity_assignments);
					}
				}
				last_activity_assignments.Clear();
				foreach (Character key in this.currentAnimationAssignments.Keys)
				{
					last_activity_assignments[key] = this.currentAnimationAssignments[key];
				}
			}

			// Token: 0x06004528 RID: 17704 RVA: 0x0031E8F8 File Offset: 0x0031CAF8
			public bool TryAssignment(Character character, List<Point> points, string dialogue_key, string animation_name = null, bool animation_required = false, double chance = 1.0, Dictionary<Character, string> last_activity_assignments = null)
			{
				string assignment;
				if (last_activity_assignments != null && !string.IsNullOrEmpty(animation_name) && !animation_name.StartsWith("square_") && last_activity_assignments.TryGetValue(character, out assignment) && assignment == animation_name)
				{
					return false;
				}
				if (points.Count > 0 && (this.random.NextDouble() < chance || chance >= 1.0))
				{
					Point current_point = this.random.ChooseFrom(points);
					if (!string.IsNullOrEmpty(animation_name) && !animation_name.StartsWith("square_") && !this.animationDescriptions.ContainsKey(animation_name))
					{
						if (animation_required)
						{
							return false;
						}
						animation_name = null;
					}
					string assignment_string = string.IsNullOrEmpty(animation_name) ? (current_point.X.ToString() + " " + current_point.Y.ToString() + " 2") : string.Concat(new string[]
					{
						current_point.X.ToString(),
						" ",
						current_point.Y.ToString(),
						" ",
						animation_name
					});
					if (dialogue_key != null)
					{
						dialogue_key = this.GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":" + dialogue_key, this.random);
						if (dialogue_key == null)
						{
							dialogue_key = this.GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":Resort", this.random);
						}
						if (dialogue_key != null)
						{
							assignment_string = assignment_string + " \"" + dialogue_key + "\"";
						}
					}
					this.currentAssignments[character] = assignment_string;
					points.Remove(current_point);
					this.currentAnimationAssignments[character] = animation_name;
					return true;
				}
				return false;
			}

			// Token: 0x06004529 RID: 17705 RVA: 0x0031EA98 File Offset: 0x0031CC98
			public string GetRandomDialogueKey(string dialogue_key, Random random)
			{
				if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key, true) == null)
				{
					return null;
				}
				bool fail = false;
				int count = 0;
				while (!fail)
				{
					count++;
					if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key + "_" + (count + 1).ToString(), true) == null)
					{
						fail = true;
					}
				}
				int index = random.Next(count) + 1;
				if (index == 1)
				{
					return dialogue_key;
				}
				return dialogue_key + "_" + index.ToString();
			}

			// Token: 0x0600452A RID: 17706 RVA: 0x0031EB0C File Offset: 0x0031CD0C
			public string GetScheduleStringForCharacter(NPC character)
			{
				string assignment;
				if (this.currentAssignments.TryGetValue(character, out assignment))
				{
					return "/" + this.activityTime.ToString() + " IslandSouth " + assignment;
				}
				return "";
			}

			// Token: 0x04002F68 RID: 12136
			public int activityTime;

			// Token: 0x04002F69 RID: 12137
			public List<NPC> visitors;

			// Token: 0x04002F6A RID: 12138
			public Dictionary<Character, string> currentAssignments;

			// Token: 0x04002F6B RID: 12139
			public Dictionary<Character, string> currentAnimationAssignments;

			// Token: 0x04002F6C RID: 12140
			public Random random;

			// Token: 0x04002F6D RID: 12141
			public Dictionary<string, string> animationDescriptions;

			// Token: 0x04002F6E RID: 12142
			public List<Point> shoreLoungePoints = new List<Point>(new Point[]
			{
				new Point(9, 33),
				new Point(13, 33),
				new Point(17, 33),
				new Point(24, 33),
				new Point(28, 32),
				new Point(32, 31)
			});

			// Token: 0x04002F6F RID: 12143
			public List<Point> chairPoints = new List<Point>(new Point[]
			{
				new Point(20, 24),
				new Point(30, 29)
			});

			// Token: 0x04002F70 RID: 12144
			public List<Point> umbrellaPoints = new List<Point>(new Point[]
			{
				new Point(26, 26),
				new Point(28, 29),
				new Point(10, 27)
			});

			// Token: 0x04002F71 RID: 12145
			public List<Point> towelLoungePoints = new List<Point>(new Point[]
			{
				new Point(14, 27),
				new Point(17, 28),
				new Point(20, 27),
				new Point(23, 28)
			});

			// Token: 0x04002F72 RID: 12146
			public List<Point> drinkPoints = new List<Point>(new Point[]
			{
				new Point(12, 23),
				new Point(15, 23)
			});

			// Token: 0x04002F73 RID: 12147
			public List<Point> wanderPoints = new List<Point>(new Point[]
			{
				new Point(7, 16),
				new Point(31, 24),
				new Point(18, 13)
			});
		}
	}
}

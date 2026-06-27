using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Minigames;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002C2 RID: 706
	public class BoatTunnel : GameLocation
	{
		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06002DCF RID: 11727 RVA: 0x0023D372 File Offset: 0x0023B572
		// (set) Token: 0x06002DD0 RID: 11728 RVA: 0x0023D37A File Offset: 0x0023B57A
		public int TicketPrice { get; set; } = 1000;

		// Token: 0x06002DD1 RID: 11729 RVA: 0x0023D383 File Offset: 0x0023B583
		public BoatTunnel()
		{
		}

		// Token: 0x06002DD2 RID: 11730 RVA: 0x0023D3A8 File Offset: 0x0023B5A8
		public BoatTunnel(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002DD3 RID: 11731 RVA: 0x0023D3CF File Offset: 0x0023B5CF
		public override void cleanupBeforePlayerExit()
		{
			Game1.player.controller = null;
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06002DD4 RID: 11732 RVA: 0x0023D3E2 File Offset: 0x0023B5E2
		public virtual bool GateFinishedAnimating()
		{
			if (this._gateDirection < 0)
			{
				return this._gateFrame <= 0;
			}
			return this._gateDirection <= 0 || this._gateFrame >= 5;
		}

		// Token: 0x06002DD5 RID: 11733 RVA: 0x0023D411 File Offset: 0x0023B611
		public virtual bool PlankFinishedAnimating()
		{
			if (this._plankDirection < 0f)
			{
				return this._plankPosition <= 0f;
			}
			return this._plankDirection <= 0f || this._plankPosition >= 16f;
		}

		// Token: 0x06002DD6 RID: 11734 RVA: 0x0023D450 File Offset: 0x0023B650
		public virtual void SetCurrentState(BoatTunnel.TunnelAnimationState animation_state)
		{
			this.animationState = animation_state;
		}

		// Token: 0x06002DD7 RID: 11735 RVA: 0x0023D459 File Offset: 0x0023B659
		public virtual void UpdateGateTileProperty()
		{
			if (this._gateFrame == 0)
			{
				base.setTileProperty(6, 8, "Back", "TemporaryBarrier", "T");
				return;
			}
			base.removeTileProperty(6, 8, "Back", "TemporaryBarrier");
		}

		// Token: 0x06002DD8 RID: 11736 RVA: 0x0023D490 File Offset: 0x0023B690
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings", false) == "BoatTicket")
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine"))
				{
					if (who.Items.ContainsId("(O)787", 5))
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateBatteries"), base.createYesNoResponses(), "WillyBoatDonateBatteries");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateBatteriesHint"));
					}
				}
				else if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
				{
					if (Game1.player.isRidingHorse() && Game1.player.mount != null)
					{
						Game1.player.mount.checkAction(Game1.player, this);
					}
					else
					{
						string displayPrice = Utility.getNumberWithCommas(this.TicketPrice);
						if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.es)
						{
							base.createQuestionDialogueWithCustomWidth(Game1.content.LoadString("Strings\\Locations:BuyTicket", displayPrice), base.createYesNoResponses(), "Boat");
						}
						else
						{
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BuyTicket", displayPrice), base.createYesNoResponses(), "Boat");
						}
					}
				}
				return true;
			}
			if (!Game1.MasterPlayer.mailReceived.Contains("willyBoatFixed"))
			{
				if (tileLocation.X == 6 && tileLocation.Y == 8 && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull"))
				{
					if (who.Items.ContainsId("(O)709", 200))
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateHardwood"), base.createYesNoResponses(), "WillyBoatDonateHardwood");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateHardwoodHint"));
					}
					return true;
				}
				if (tileLocation.X == 8 && tileLocation.Y == 10 && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor"))
				{
					if (who.Items.ContainsId("(O)337", 5))
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateIridium"), base.createYesNoResponses(), "WillyBoatDonateIridium");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateIridiumHint"));
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002DD9 RID: 11737 RVA: 0x0023D6D1 File Offset: 0x0023B8D1
		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("willyBoatFixed"))
			{
				if (xTile == 6 && yTile == 8)
				{
					return true;
				}
				if (xTile == 8 && yTile == 10)
				{
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		// Token: 0x06002DDA RID: 11738 RVA: 0x0023D708 File Offset: 0x0023B908
		public override bool answerDialogue(Response answer)
		{
			if (this.lastQuestionKey != null && this.afterQuestion == null)
			{
				string questionAndAnswer = ArgUtility.SplitBySpaceAndGet(this.lastQuestionKey, 0, null) + "_" + answer.responseKey;
				int ticket_price = this.TicketPrice;
				if (questionAndAnswer == "Boat_Yes")
				{
					if (Game1.player.Money >= ticket_price)
					{
						Game1.player.Money -= ticket_price;
						this.StartDeparture();
					}
					else if (Game1.player.Money < ticket_price)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
					}
					return true;
				}
				if (questionAndAnswer == "WillyBoatDonateBatteries_Yes")
				{
					Game1.multiplayer.globalChatInfoMessage("RepairBoatMachine", new string[]
					{
						Game1.player.Name
					});
					Game1.player.Items.ReduceId("(O)787", 5);
					DelayedAction.playSoundAfterDelay("openBox", 600, null, null, -1, false);
					Game1.addMailForTomorrow("willyBoatTicketMachine", true, true);
					this.checkForBoatComplete();
					return true;
				}
				if (questionAndAnswer == "WillyBoatDonateHardwood_Yes")
				{
					Game1.multiplayer.globalChatInfoMessage("RepairBoatHull", new string[]
					{
						Game1.player.Name
					});
					Game1.player.Items.ReduceId("(O)709", 200);
					DelayedAction.playSoundAfterDelay("Ship", 600, null, null, -1, false);
					Game1.addMailForTomorrow("willyBoatHull", true, true);
					this.checkForBoatComplete();
					return true;
				}
				if (questionAndAnswer == "WillyBoatDonateIridium_Yes")
				{
					Game1.multiplayer.globalChatInfoMessage("RepairBoatAnchor", new string[]
					{
						Game1.player.Name
					});
					Game1.player.Items.ReduceId("(O)337", 5);
					DelayedAction.playSoundAfterDelay("clank", 600, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("clank", 1200, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("clank", 1800, null, null, -1, false);
					Game1.addMailForTomorrow("willyBoatAnchor", true, true);
					this.checkForBoatComplete();
					return true;
				}
			}
			return base.answerDialogue(answer);
		}

		// Token: 0x06002DDB RID: 11739 RVA: 0x0023D950 File Offset: 0x0023BB50
		private void checkForBoatComplete()
		{
			if (Game1.player.hasOrWillReceiveMail("willyBoatTicketMachine") && Game1.player.hasOrWillReceiveMail("willyBoatHull") && Game1.player.hasOrWillReceiveMail("willyBoatAnchor"))
			{
				Game1.player.freezePause = 1500;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.multiplayer.globalChatInfoMessage("RepairBoat", Array.Empty<string>());
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_boatcomplete"));
				}, 1500);
			}
		}

		// Token: 0x06002DDC RID: 11740 RVA: 0x0023D9CC File Offset: 0x0023BBCC
		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			return p.Y <= 8f || (p.Y <= 10f && p.X >= 4f && p.X <= 8f) || base.shouldShadowBeDrawnAboveBuildingsLayer(p);
		}

		// Token: 0x06002DDD RID: 11741 RVA: 0x0023DA18 File Offset: 0x0023BC18
		public virtual void StartDeparture()
		{
			xTile.Dimensions.Rectangle viewport = Game1.viewport;
			Vector2 player_position = Game1.player.Position;
			int player_direction = Game1.player.FacingDirection;
			StringBuilder event_string = new StringBuilder();
			event_string.Append("none/0 0/farmer 0 0 0 Willy 6 12 0/playMusic none/skippable");
			if (Game1.stats.Get("boatRidesToIsland") <= 0U)
			{
				event_string.Append("/textAboveHead Willy \"" + Game1.content.LoadString("Strings\\Locations:BoatTunnel_willyText_firstRide") + "\"");
			}
			else if (Game1.random.NextDouble() < 0.2)
			{
				event_string.Append("/textAboveHead Willy \"" + Game1.content.LoadString("Strings\\Locations:BoatTunnel_willyText_random" + Game1.random.Next(2).ToString()) + "\"");
			}
			event_string.Append("/move Willy 0 -3 0/pause 500/locationSpecificCommand open_gate/viewport move 0 -1 1000/pause 500/move Willy 0 -2 3/move Willy -1 0 1/locationSpecificCommand path_player 6 5 2/move Willy 1 0 2/move Willy 0 1 2/pause 250/playSound clubhit/animate Willy false false 500 27/locationSpecificCommand retract_plank/jump Willy 4/pause 750/move Willy 0 -1 0/locationSpecificCommand close_gate/pause 200/move Willy 3 0 1/locationSpecificCommand offset_willy/move Willy 1 0 1");
			event_string.Append("/locationSpecificCommand non_blocking_pause 1000/playerControl boatRide/playSound furnace/locationSpecificCommand animate_boat_start/locationSpecificCommand non_blocking_pause 1000/locationSpecificCommand boat_depart/locationSpecificCommand animate_boat_move/fade/viewport -5000 -5000/end tunnelDepart");
			this._boatEvent = new Event(event_string.ToString(), null, "-78765", Game1.player)
			{
				showWorldCharacters = true,
				showGroundObjects = true,
				ignoreObjectCollisions = false
			};
			Event boatEvent = this._boatEvent;
			boatEvent.onEventFinished = (Action)Delegate.Combine(boatEvent.onEventFinished, new Action(this.OnBoatEventEnd));
			this.currentEvent = this._boatEvent;
			this._boatEvent.Update(this, Game1.currentGameTime);
			Game1.eventUp = true;
			Game1.viewport = viewport;
			this._farmerActor = (this.currentEvent.getCharacterByName("farmer") as Farmer);
			this._farmerActor.Position = player_position;
			this._farmerActor.faceDirection(player_direction);
			(this.currentEvent.getCharacterByName("Willy") as NPC).IsInvisible = false;
			Game1.stats.Increment("boatRidesToIsland", 1U);
		}

		// Token: 0x06002DDE RID: 11742 RVA: 0x0023DBD8 File Offset: 0x0023BDD8
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (this._boatDirection != 0)
			{
				this._boatOffset += this._boatDirection;
				if (this.currentEvent != null)
				{
					foreach (NPC npc in this.currentEvent.actors)
					{
						npc.shouldShadowBeOffset = true;
						npc.drawOffset.X = (float)this._boatOffset;
					}
					foreach (Farmer farmer in this.currentEvent.farmerActors)
					{
						farmer.shouldShadowBeOffset = true;
						farmer.drawOffset.X = (float)this._boatOffset;
					}
				}
			}
			if (!this.PlankFinishedAnimating())
			{
				this._plankPosition += this._plankDirection;
				if (this.PlankFinishedAnimating())
				{
					this._plankDirection = 0f;
				}
			}
			if (!this.GateFinishedAnimating())
			{
				this._gateFrameTimer += (float)time.ElapsedGameTime.TotalSeconds;
				if (this._gateFrameTimer >= 0.1f)
				{
					this._gateFrameTimer -= 0.1f;
					this._gateFrame += this._gateDirection;
				}
			}
			else
			{
				this._gateFrameTimer = 0f;
			}
			if (this._plankShake > 0f)
			{
				this._plankShake -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._plankShake < 0f)
				{
					this._plankShake = 0f;
				}
			}
			Microsoft.Xna.Framework.Rectangle back_rectangle = new Microsoft.Xna.Framework.Rectangle(24, 188, 16, 220);
			back_rectangle.X += (int)this.GetBoatPosition().X;
			back_rectangle.Y += (int)this.GetBoatPosition().Y;
			if ((float)this._boatDirection != 0f)
			{
				if (this._nextBubble > 0f)
				{
					this._nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					Vector2 position = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
					TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, position, false, false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f, false);
					sprite.acceleration = new Vector2(-0.25f * (float)Math.Sign(this._boatDirection), 0f);
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
			if (this._boatAnimating)
			{
				if (this._nextSmoke > 0f)
				{
					this._nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
					return;
				}
				Vector2 position2 = new Vector2(80f, -32f) * 4f + this.GetBoatPosition();
				TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position2, false, false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f, false);
				sprite2.acceleration = new Vector2(-0.25f, -0.15f);
				this.temporarySprites.Add(sprite2);
				this._nextSmoke = 0.2f;
			}
		}

		// Token: 0x06002DDF RID: 11743 RVA: 0x0023DFCC File Offset: 0x0023C1CC
		public virtual void OnBoatEventEnd()
		{
			if (this._boatEvent != null)
			{
				foreach (NPC npc in this._boatEvent.actors)
				{
					npc.shouldShadowBeOffset = false;
					npc.drawOffset.X = 0f;
				}
				foreach (Farmer farmer in this._boatEvent.farmerActors)
				{
					farmer.shouldShadowBeOffset = false;
					farmer.drawOffset.X = 0f;
				}
				this.ResetBoat();
				this._boatEvent = null;
				if (!Game1.player.hasOrWillReceiveMail("seenBoatJourney"))
				{
					Game1.addMailForTomorrow("seenBoatJourney", true, false);
					Game1.currentMinigame = new BoatJourney();
				}
			}
		}

		// Token: 0x06002DE0 RID: 11744 RVA: 0x0023E0C8 File Offset: 0x0023C2C8
		public override bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			if (command_string != null)
			{
				switch (command_string.Length)
				{
				case 9:
					if (command_string == "open_gate")
					{
						if (first_run)
						{
							Game1.playSound("openChest", null);
						}
						this._gateDirection = 1;
						if (this.GateFinishedAnimating())
						{
							this.UpdateGateTileProperty();
						}
						return this.GateFinishedAnimating();
					}
					break;
				case 10:
					if (command_string == "close_gate")
					{
						this._gateDirection = -1;
						if (this.GateFinishedAnimating())
						{
							this.UpdateGateTileProperty();
						}
						return this.GateFinishedAnimating();
					}
					break;
				case 11:
				{
					char c = command_string[0];
					if (c != 'b')
					{
						if (c == 'p')
						{
							if (command_string == "path_player")
							{
								int x = ArgUtility.GetInt(args, 0, 0);
								int y = ArgUtility.GetInt(args, 1, 0);
								int direction = ArgUtility.GetInt(args, 2, 2);
								if (first_run)
								{
									this._playerPathing = true;
									Character player = Game1.player;
									PathFindController controller;
									(controller = new PathFindController(Game1.player, this, new Point(x, y), direction, new PathFindController.endBehavior(this.OnReachedBoatDeck))).allowPlayerPathingInEvent = true;
									player.controller = controller;
									Game1.player.canOnlyWalk = false;
									Game1.player.setRunning(true, true);
									if (Game1.player.mount != null)
									{
										Game1.player.mount.farmerPassesThrough = true;
									}
									this.forceWarpTimer = 8000;
								}
								if (this.forceWarpTimer > 0)
								{
									this.forceWarpTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
									if (this.forceWarpTimer <= 0)
									{
										this.forceWarpTimer = 0;
										Game1.player.controller = null;
										Game1.player.setTileLocation(new Vector2((float)x, (float)y));
										Game1.player.faceDirection(direction);
										this.OnReachedBoatDeck(Game1.player, this);
									}
								}
								return !this._playerPathing;
							}
						}
					}
					else if (command_string == "boat_depart")
					{
						if (first_run)
						{
							this._boatDirection = 1;
						}
						return this._boatOffset >= 100;
					}
					break;
				}
				case 12:
				{
					char c = command_string[0];
					if (c != 'e')
					{
						if (c == 'o')
						{
							if (command_string == "offset_willy")
							{
								if (first_run)
								{
									this._boatEvent.getActorByName("Willy", false).drawOffset.Y = -24f;
								}
							}
						}
					}
					else if (command_string == "extend_plank")
					{
						if (first_run)
						{
							this._plankDirection = -0.25f;
						}
						return true;
					}
					break;
				}
				case 13:
					if (command_string == "retract_plank")
					{
						if (first_run)
						{
							this._plankDirection = 0.25f;
						}
						return true;
					}
					break;
				case 18:
				{
					char c = command_string[0];
					if (c != 'a')
					{
						if (c == 'n')
						{
							if (command_string == "non_blocking_pause")
							{
								if (first_run)
								{
									this.nonBlockingPause = ArgUtility.GetInt(args, 0, 0);
									return false;
								}
								this.nonBlockingPause -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
								if (this.nonBlockingPause < 0)
								{
									this.nonBlockingPause = 0;
									return true;
								}
								return false;
							}
						}
					}
					else if (command_string == "animate_boat_start")
					{
						if (first_run)
						{
							this._boatAnimating = true;
							Game1.player.canOnlyWalk = false;
						}
						return true;
					}
					break;
				}
				}
			}
			return base.RunLocationSpecificEventCommand(current_event, command_string, first_run, args);
		}

		// Token: 0x06002DE1 RID: 11745 RVA: 0x0023E454 File Offset: 0x0023C654
		public virtual void OnReachedBoatDeck(Character character, GameLocation location)
		{
			this._playerPathing = false;
			Game1.player.controller = null;
			Game1.player.canOnlyWalk = true;
			this.forceWarpTimer = 0;
		}

		// Token: 0x06002DE2 RID: 11746 RVA: 0x0023E47A File Offset: 0x0023C67A
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			this.UpdateGateTileProperty();
		}

		// Token: 0x06002DE3 RID: 11747 RVA: 0x0023E48C File Offset: 0x0023C68C
		protected override void resetLocalState()
		{
			this.critters = new List<Critter>();
			base.resetLocalState();
			this.boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				base.addCritter(new CrabCritter(new Vector2(128f, 640f)));
			}
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				base.addCritter(new CrabCritter(new Vector2(576f, 672f)));
			}
			this.ResetBoat();
		}

		// Token: 0x06002DE4 RID: 11748 RVA: 0x0023E524 File Offset: 0x0023C724
		public virtual void ResetBoat()
		{
			this._nextSmoke = 0f;
			this._nextBubble = 0f;
			this._boatAnimating = false;
			this.boatPosition = new Vector2(52f, 36f) * 4f;
			this._gateFrameTimer = 0f;
			this._gateDirection = 0;
			this._gateFrame = 0;
			this._boatOffset = 0;
			this._boatDirection = 0;
			this._plankPosition = 0f;
			this._plankDirection = 0f;
			this.UpdateGateTileProperty();
		}

		// Token: 0x06002DE5 RID: 11749 RVA: 0x0023E5B0 File Offset: 0x0023C7B0
		public Vector2 GetBoatPosition()
		{
			return this.boatPosition + new Vector2((float)this._boatOffset, 0f);
		}

		// Token: 0x06002DE6 RID: 11750 RVA: 0x0023E5D0 File Offset: 0x0023C7D0
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Vector2 boat_position = this.GetBoatPosition();
			if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed") && Game1.farmEvent == null)
			{
				b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(4, 0, 156, 118)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, this.boatPosition.Y / 10000f);
				b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position + new Vector2(8f, 0f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 160, 128, 96)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.boatPosition.Y + 408f) / 10000f);
				Vector2 plank_shake = Vector2.Zero;
				if (!this.PlankFinishedAnimating() || this._plankShake > 0f)
				{
					plank_shake = new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				}
				b.Draw(this.boatTexture, Game1.GlobalToLocal(new Vector2(6f, 9f) * 64f + new Vector2(0f, (float)((int)this._plankPosition)) * 4f + plank_shake), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(128, 176, 17, 33)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (512f + this._plankPosition * 4f) / 10000f);
				Microsoft.Xna.Framework.Rectangle gate_draw_rect = this.gateRect;
				gate_draw_rect.X = this._gateFrame * this.gateRect.Width;
				b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position + new Vector2(35f, 81f) * 4f), new Microsoft.Xna.Framework.Rectangle?(gate_draw_rect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.boatPosition.Y + 428f) / 10000f);
			}
			else
			{
				b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(4, 259, 156, 122)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, this.boatPosition.Y / 10000f);
				b.Draw(this.boatTexture, Game1.GlobalToLocal(new Vector2(6f, 9f) * 64f + new Vector2(0f, (float)((int)this._plankPosition)) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(128, 176, 17, 33)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (512f + this._plankPosition * 4f) / 10000f);
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
				if (!Game1.eventUp)
				{
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull"))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(416f, 456f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 4f), SpriteEffects.None, 1f);
					}
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine"))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(288f, 520f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 4f), SpriteEffects.None, 1f);
					}
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor"))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 520f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 4f), SpriteEffects.None, 1f);
					}
				}
			}
			b.Draw(this.boatTexture, Game1.GlobalToLocal(new Vector2(4f, 8f) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(160, 192, 16, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0512f);
		}

		// Token: 0x04001F5D RID: 8029
		private Texture2D boatTexture;

		// Token: 0x04001F5E RID: 8030
		private Vector2 boatPosition;

		// Token: 0x04001F5F RID: 8031
		public Microsoft.Xna.Framework.Rectangle gateRect = new Microsoft.Xna.Framework.Rectangle(0, 120, 32, 40);

		// Token: 0x04001F60 RID: 8032
		protected int _gateFrame;

		// Token: 0x04001F61 RID: 8033
		protected int _gateDirection;

		// Token: 0x04001F62 RID: 8034
		protected float _gateFrameTimer;

		// Token: 0x04001F63 RID: 8035
		public const float GATE_SECONDS_PER_FRAME = 0.1f;

		// Token: 0x04001F64 RID: 8036
		public const int GATE_FRAMES = 5;

		// Token: 0x04001F65 RID: 8037
		protected int _boatOffset;

		// Token: 0x04001F66 RID: 8038
		protected int _boatDirection;

		// Token: 0x04001F67 RID: 8039
		public const int PLANK_MAX_OFFSET = 16;

		// Token: 0x04001F68 RID: 8040
		public float _plankPosition;

		// Token: 0x04001F69 RID: 8041
		public float _plankDirection;

		// Token: 0x04001F6A RID: 8042
		protected Farmer _farmerActor;

		// Token: 0x04001F6B RID: 8043
		protected Event _boatEvent;

		// Token: 0x04001F6C RID: 8044
		protected bool _playerPathing;

		// Token: 0x04001F6D RID: 8045
		protected int nonBlockingPause;

		// Token: 0x04001F6E RID: 8046
		protected float _nextBubble;

		// Token: 0x04001F6F RID: 8047
		protected float _nextSlosh;

		// Token: 0x04001F70 RID: 8048
		protected float _nextSmoke;

		// Token: 0x04001F71 RID: 8049
		protected float _plankShake;

		// Token: 0x04001F72 RID: 8050
		protected int forceWarpTimer;

		// Token: 0x04001F73 RID: 8051
		protected bool _boatAnimating;

		// Token: 0x04001F74 RID: 8052
		public BoatTunnel.TunnelAnimationState animationState;

		// Token: 0x02000645 RID: 1605
		public enum TunnelAnimationState
		{
			// Token: 0x04002F15 RID: 12053
			Idle,
			// Token: 0x04002F16 RID: 12054
			MoveWillyToGate,
			// Token: 0x04002F17 RID: 12055
			OpenGate,
			// Token: 0x04002F18 RID: 12056
			MoveWillyToCockpit,
			// Token: 0x04002F19 RID: 12057
			MoveFarmer,
			// Token: 0x04002F1A RID: 12058
			MovePlank,
			// Token: 0x04002F1B RID: 12059
			CloseGate,
			// Token: 0x04002F1C RID: 12060
			MoveBoat
		}
	}
}

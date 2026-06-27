using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002D6 RID: 726
	public class IslandFieldOffice : IslandLocation
	{
		// Token: 0x06002FBA RID: 12218 RVA: 0x0025ACA8 File Offset: 0x00258EA8
		public IslandFieldOffice()
		{
		}

		// Token: 0x06002FBB RID: 12219 RVA: 0x0025AD58 File Offset: 0x00258F58
		public IslandFieldOffice(string map, string name) : base(map, name)
		{
			while (this.piecesDonated.Count < 11)
			{
				this.piecesDonated.Add(false);
			}
		}

		// Token: 0x06002FBC RID: 12220 RVA: 0x0025AE24 File Offset: 0x00259024
		public NPC getSafariGuy()
		{
			return this.safariGuy;
		}

		// Token: 0x06002FBD RID: 12221 RVA: 0x0025AE2C File Offset: 0x0025902C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.piecesDonated, "piecesDonated").AddField(this.centerSkeletonRestored, "centerSkeletonRestored").AddField(this.snakeRestored, "snakeRestored").AddField(this.batRestored, "batRestored").AddField(this.frogRestored, "frogRestored").AddField(this.plantsRestoredLeft, "plantsRestoredLeft").AddField(this.plantsRestoredRight, "plantsRestoredRight").AddField(this.uncollectedRewards, "uncollectedRewards").AddField(this.hasFailedSurveyToday, "hasFailedSurveyToday").AddField(this.safariGuyMutex.NetFields, "safariGuyMutex.NetFields");
			this.centerSkeletonRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplySkeletonRestore();
				}
			};
			this.snakeRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplySnakeRestore();
				}
			};
			this.batRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyBatRestore();
				}
			};
			this.frogRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyFrogRestore();
				}
			};
			this.plantsRestoredLeft.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyPlantRestoreLeft();
				}
			};
			this.plantsRestoredRight.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyPlantRestoreRight();
				}
			};
		}

		// Token: 0x06002FBE RID: 12222 RVA: 0x0025AF78 File Offset: 0x00259178
		private void ApplyPlantRestoreLeft()
		{
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f, new Color(0, 220, 150), 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				layerDepth = 1f,
				motion = new Vector2(1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				flipped = true,
				layerDepth = 1f,
				motion = new Vector2(-1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				delayBeforeAnimationStart = 50,
				layerDepth = 1f,
				motion = new Vector2(1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				flipped = true,
				delayBeforeAnimationStart = 100,
				layerDepth = 1f,
				motion = new Vector2(-1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(250, 100, 250) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				flipped = true,
				delayBeforeAnimationStart = 150,
				layerDepth = 1f,
				motion = new Vector2(0f, -3f),
				acceleration = new Vector2(0f, 0.1f)
			});
			if (Game1.gameMode != 6 && !Utility.ShouldIgnoreValueChangeCallback())
			{
				if (Game1.currentLocation == this)
				{
					Game1.playSound("leafrustle", null);
					DelayedAction.playSoundAfterDelay("leafrustle", 150, null, null, -1, false);
				}
				if (Game1.IsMasterGame)
				{
					Game1.player.team.MarkCollectedNut("IslandLeftPlantRestored");
					if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(1.5f, 3.3f) * 64f, 1, this, 256, false);
					}
				}
			}
		}

		// Token: 0x06002FBF RID: 12223 RVA: 0x0025B3DC File Offset: 0x002595DC
		private void ApplyPlantRestoreRight()
		{
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(7.5f, 3.3f) * 64f, new Color(0, 220, 150), 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				layerDepth = 1f,
				motion = new Vector2(1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				flipped = true,
				layerDepth = 1f,
				motion = new Vector2(-1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8.3f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 200, 120) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				delayBeforeAnimationStart = 50,
				layerDepth = 1f,
				motion = new Vector2(1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				flipped = true,
				delayBeforeAnimationStart = 100,
				layerDepth = 1f,
				motion = new Vector2(-1f, -4f),
				acceleration = new Vector2(0f, 0.1f)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8.5f, 3.3f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), new Color(0, 250, 180) * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				scale = 0.75f,
				flipped = true,
				delayBeforeAnimationStart = 150,
				layerDepth = 1f,
				motion = new Vector2(0f, -3f),
				acceleration = new Vector2(0f, 0.1f)
			});
			if (Game1.gameMode != 6 && !Utility.ShouldIgnoreValueChangeCallback())
			{
				if (Game1.currentLocation == this)
				{
					Game1.playSound("leafrustle", null);
					DelayedAction.playSoundAfterDelay("leafrustle", 150, null, null, -1, false);
				}
				if (Game1.IsMasterGame)
				{
					Game1.player.team.MarkCollectedNut("IslandRightPlantRestored");
					if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(7.5f, 3.3f) * 64f, 3, this, 256, false);
					}
				}
			}
		}

		// Token: 0x06002FC0 RID: 12224 RVA: 0x0025B83C File Offset: 0x00259A3C
		private void ApplyFrogRestore()
		{
			if (Game1.gameMode != 6 && !Utility.ShouldIgnoreValueChangeCallback() && Game1.currentLocation == this)
			{
				Game1.playSound("dirtyHit", null);
			}
			for (int i = 0; i < 3; i++)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(6.5f + (float)Game1.random.Next(-10, 11) / 100f, 3f) * 64f, false, 0.007f, Color.White)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -1f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 4f,
					scaleChange = 0.02f,
					rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
					delayBeforeAnimationStart = i * 100
				});
			}
		}

		// Token: 0x06002FC1 RID: 12225 RVA: 0x0025B974 File Offset: 0x00259B74
		private void ApplyBatRestore()
		{
			if (Game1.gameMode != 6 && !Utility.ShouldIgnoreValueChangeCallback() && Game1.currentLocation == this)
			{
				Game1.playSound("dirtyHit", null);
			}
			for (int i = 0; i < 3; i++)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2.5f + (float)Game1.random.Next(-10, 11) / 100f, 3f) * 64f, false, 0.007f, Color.White)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -1f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 4f,
					scaleChange = 0.02f,
					rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
					delayBeforeAnimationStart = i * 100
				});
			}
		}

		// Token: 0x06002FC2 RID: 12226 RVA: 0x0025BAAB File Offset: 0x00259CAB
		private void ApplySnakeRestore()
		{
		}

		// Token: 0x06002FC3 RID: 12227 RVA: 0x0025BAAD File Offset: 0x00259CAD
		private void ApplySkeletonRestore()
		{
		}

		// Token: 0x06002FC4 RID: 12228 RVA: 0x0025BAB0 File Offset: 0x00259CB0
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandFieldOffice loc = l as IslandFieldOffice;
			this.uncollectedRewards.Clear();
			this.uncollectedRewards.Set(loc.uncollectedRewards);
			this.piecesDonated.Clear();
			this.piecesDonated.Set(loc.piecesDonated);
			this.centerSkeletonRestored.Value = loc.centerSkeletonRestored.Value;
			this.snakeRestored.Value = loc.snakeRestored.Value;
			this.batRestored.Value = loc.batRestored.Value;
			this.frogRestored.Value = loc.frogRestored.Value;
			this.plantsRestoredLeft.Value = loc.plantsRestoredLeft.Value;
			this.plantsRestoredRight.Value = loc.plantsRestoredRight.Value;
			this.hasFailedSurveyToday.Value = loc.hasFailedSurveyToday.Value;
		}

		// Token: 0x06002FC5 RID: 12229 RVA: 0x0025BBA0 File Offset: 0x00259DA0
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened") && this.safariGuy == null)
			{
				this.safariGuy = new NPC(new AnimatedSprite("Characters\\SafariGuy", 0, 16, 32), new Vector2(8f, 6f) * 64f, "IslandFieldOFfice", 2, "Professor Snail", false, Game1.content.Load<Texture2D>("Portraits\\SafariGuy"));
				this.safariGuy.AllowDynamicAppearance = false;
				this.safariGuy.displayName = Game1.content.LoadString("Strings\\NPCNames:ProfessorSnail");
			}
			if (this.safariGuy != null && !Game1.player.hasOrWillReceiveMail("safariGuyIntro"))
			{
				this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Intro_Event"), null));
				Game1.player.mailReceived.Add("safariGuyIntro");
				Game1.player.Halt();
				return;
			}
			if (this.safariGuy != null)
			{
				Game1.changeMusicTrack("fieldofficeTentMusic", false, MusicContext.Default);
				if (Game1.random.NextBool())
				{
					this.safariGuy.Halt();
					this.safariGuy.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Welcome_" + Game1.random.Next(4).ToString()), null, 2, 3000, 0);
					this.safariGuy.faceTowardFarmerForPeriod(60000, 5, false, Game1.player);
				}
				else
				{
					this.safariGuy.Sprite.CurrentAnimation = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(18, 900, 0, false, false, null, false, 0),
						new FarmerSprite.AnimationFrame(19, 900, 0, false, false, null, false, 0)
					};
				}
			}
			if (!Game1.player.hasOrWillReceiveMail("fieldOfficeFinale") && this.isRangeAllTrue(0, 11) && this.plantsRestoredRight.Value && this.plantsRestoredLeft.Value && this.currentEvent == null)
			{
				this._StartFinaleEvent();
			}
		}

		// Token: 0x06002FC6 RID: 12230 RVA: 0x0025BDA8 File Offset: 0x00259FA8
		public bool donatePiece(int which)
		{
			this.piecesDonated[which] = true;
			if (!this.centerSkeletonRestored.Value && this.isRangeAllTrue(0, 6))
			{
				this.centerSkeletonRestored.Value = true;
				if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
				{
					this.uncollectedRewards.Add(ItemRegistry.Create("(O)73", 6, 0, false));
				}
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)69", 1, 0, false));
				Game1.player.team.MarkCollectedNut("IslandCenterSkeletonRestored");
				return true;
			}
			if (!this.snakeRestored.Value && this.isRangeAllTrue(6, 9))
			{
				this.snakeRestored.Value = true;
				if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
				{
					this.uncollectedRewards.Add(ItemRegistry.Create("(O)73", 3, 0, false));
				}
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)835", 1, 0, false));
				Game1.player.team.MarkCollectedNut("IslandSnakeRestored");
				return true;
			}
			if (!this.batRestored.Value && this.piecesDonated[9])
			{
				this.batRestored.Value = true;
				if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
				{
					this.uncollectedRewards.Add(ItemRegistry.Create("(O)73", 1, 0, false));
				}
				else
				{
					this.uncollectedRewards.Add(ItemRegistry.Create("(O)TentKit", 1, 0, false));
				}
				Game1.player.team.MarkCollectedNut("IslandBatRestored");
				return true;
			}
			if (!this.frogRestored.Value && this.piecesDonated[10])
			{
				this.frogRestored.Value = true;
				if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
				{
					this.uncollectedRewards.Add(ItemRegistry.Create("(O)73", 1, 0, false));
				}
				else
				{
					this.uncollectedRewards.Add(ItemRegistry.Create("(O)926", 1, 0, false));
				}
				Game1.player.team.MarkCollectedNut("IslandFrogRestored");
				return true;
			}
			return false;
		}

		// Token: 0x06002FC7 RID: 12231 RVA: 0x0025BFD0 File Offset: 0x0025A1D0
		public bool isRangeAllTrue(int low, int high)
		{
			for (int i = low; i < high; i++)
			{
				if (!this.piecesDonated[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002FC8 RID: 12232 RVA: 0x0025BFFA File Offset: 0x0025A1FA
		public void triggerFinaleCutscene()
		{
			this._shouldTriggerFinalCutscene = true;
		}

		// Token: 0x06002FC9 RID: 12233 RVA: 0x0025C003 File Offset: 0x0025A203
		private void _triggerFinaleCutsceneActual()
		{
			Game1.player.Halt();
			Game1.player.freezePause = 500;
			DelayedAction.functionAfterDelay(delegate
			{
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu = null;
				}
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this._StartFinaleEvent), 0.02f);
			}, 500);
			this._shouldTriggerFinalCutscene = false;
		}

		// Token: 0x06002FCA RID: 12234 RVA: 0x0025C03C File Offset: 0x0025A23C
		protected void _StartFinaleEvent()
		{
			NPC npc = this.safariGuy;
			if (npc != null)
			{
				npc.clearTextAboveHead();
			}
			this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:FieldOfficeFinale"), null));
		}

		// Token: 0x06002FCB RID: 12235 RVA: 0x0025C06C File Offset: 0x0025A26C
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.safariGuy != null && !Game1.eventUp)
			{
				this.safariGuy.draw(b);
			}
			if (this.centerSkeletonRestored.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(3f, 4f) * 64f + new Vector2(0f, 4f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(210, 184, 46, 43)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0512f);
			}
			if (this.snakeRestored.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(1f, 5f) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(195, 185, 14, 42)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448f);
			}
			if (this.batRestored.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(2.5f, 2.7f) * 64f + new Vector2(1f, 1f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(212, 171, 16, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
			}
			if (this.frogRestored.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(6f, 2f) * 64f + new Vector2(9f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(232, 169, 14, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
			}
			if (this.plantsRestoredLeft.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(1f, 4f) * 64f + new Vector2(0f, -7f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194, 167, 16, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.032f);
			}
			if (this.plantsRestoredRight.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(7f, 3f) * 64f + new Vector2(8f, 3f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(224, 148, 32, 21)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.032f);
			}
			if (this.safariGuy != null && (!this.plantsRestoredLeft.Value || !this.plantsRestoredRight.Value) && !Game1.eventUp)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(324f, 144f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(220, 160, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f), SpriteEffects.None, 1f);
			}
		}

		// Token: 0x06002FCC RID: 12236 RVA: 0x0025C47A File Offset: 0x0025A67A
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			NPC npc = this.safariGuy;
			if (npc == null)
			{
				return;
			}
			npc.drawAboveAlwaysFrontLayer(b);
		}

		// Token: 0x06002FCD RID: 12237 RVA: 0x0025C494 File Offset: 0x0025A694
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.safariGuyMutex.Update(this);
			if (this.safariGuy != null)
			{
				this.safariGuy.update(time, this);
				this.speakerTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.speakerTimer <= 0f)
				{
					this.speakerTimer = 600f;
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(211, 161, 5, 5), new Vector2(74.75f, 20.75f) * 4f, false, 0f, Color.White)
					{
						scale = 5f,
						scaleChange = -0.05f,
						motion = new Vector2(0.125f, 0.125f),
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 400f,
						layerDepth = 1f
					});
				}
			}
			if (Game1.currentLocation == this && this._shouldTriggerFinalCutscene && Game1.activeClickableMenu == null)
			{
				this._triggerFinaleCutsceneActual();
			}
		}

		// Token: 0x06002FCE RID: 12238 RVA: 0x0025C5BC File Offset: 0x0025A7BC
		public virtual void OnCollectReward(Item item, Farmer farmer)
		{
			ItemGrabMenu grab_menu = Game1.activeClickableMenu as ItemGrabMenu;
			if (grab_menu != null && grab_menu.context == this)
			{
				if (Game1.player.addItemToInventoryBool(grab_menu.heldItem, false))
				{
					this.uncollectedRewards.Remove(item);
					grab_menu.ItemsToGrabMenu.actualInventory = new List<Item>(this.uncollectedRewards);
					grab_menu.heldItem = null;
					if (item.QualifiedItemId != "(O)73")
					{
						Game1.playSound("coin", null);
						return;
					}
				}
				else
				{
					Game1.playSound("cancel", null);
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					grab_menu.ItemsToGrabMenu.actualInventory = new List<Item>(this.uncollectedRewards);
					grab_menu.heldItem = null;
				}
			}
		}

		// Token: 0x06002FCF RID: 12239 RVA: 0x0025C690 File Offset: 0x0025A890
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer != null)
			{
				int length = questionAndAnswer.Length;
				switch (length)
				{
				case 10:
					if (questionAndAnswer == "Survey_Yes")
					{
						if (!this.plantsRestoredLeft.Value)
						{
							List<Response> responses = new List<Response>();
							for (int i = 18; i < 25; i++)
							{
								responses.Add(new Response((i == 22) ? "Correct" : "Wrong", i.ToString() ?? ""));
							}
							responses.Add(new Response("No", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")).SetHotKey(Keys.Escape));
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Question"), responses.ToArray(), "PurpleFlowerSurvey");
						}
						else if (!this.plantsRestoredRight.Value)
						{
							List<Response> responses2 = new List<Response>();
							for (int j = 11; j < 19; j++)
							{
								responses2.Add(new Response((j == 18) ? "Correct" : "Wrong", j.ToString() ?? ""));
							}
							responses2.Add(new Response("No", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")).SetHotKey(Keys.Escape));
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_PurpleStarfish_Question"), responses2.ToArray(), "PurpleStarfishSurvey");
						}
					}
					break;
				case 11:
					if (questionAndAnswer == "Safari_Hint")
					{
						int bone = this.getRandomUnfoundBoneIndex();
						if (bone == 823)
						{
							bone = 824;
						}
						Game1.DrawDialogue(this.safariGuy, "Data\\ExtraDialogue:ProfessorSnail_Hint_" + bone.ToString());
					}
					break;
				case 12:
					if (questionAndAnswer == "Safari_Leave")
					{
						this.safariGuyMutex.ReleaseLock();
					}
					break;
				case 13:
					if (questionAndAnswer == "Safari_Donate")
					{
						Game1.activeClickableMenu = new FieldOfficeMenu(this);
						IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
						activeClickableMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(activeClickableMenu.exitFunction, new IClickableMenu.onExit(this.safariGuyMutex.ReleaseLock));
					}
					break;
				case 14:
					if (questionAndAnswer == "Safari_Collect")
					{
						Game1.activeClickableMenu = new ItemGrabMenu(new List<Item>(this.uncollectedRewards), false, true, null, null, "Rewards", new ItemGrabMenu.behaviorOnItemSelect(this.OnCollectReward), false, true, false, false, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
						IClickableMenu activeClickableMenu2 = Game1.activeClickableMenu;
						activeClickableMenu2.exitFunction = (IClickableMenu.onExit)Delegate.Combine(activeClickableMenu2.exitFunction, new IClickableMenu.onExit(this.safariGuyMutex.ReleaseLock));
					}
					break;
				default:
					switch (length)
					{
					case 24:
						if (questionAndAnswer == "PurpleFlowerSurvey_Wrong")
						{
							Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Wrong");
							this.hasFailedSurveyToday.Value = true;
						}
						break;
					case 26:
					{
						char c = questionAndAnswer[6];
						if (c != 'F')
						{
							if (c == 'S')
							{
								if (questionAndAnswer == "PurpleStarfishSurvey_Wrong")
								{
									Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Wrong");
									this.hasFailedSurveyToday.Value = true;
								}
							}
						}
						else if (questionAndAnswer == "PurpleFlowerSurvey_Correct")
						{
							Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Correct");
							this.plantsRestoredLeft.Value = true;
							Game1.multiplayer.globalChatInfoMessage("FinishedSurvey", new string[]
							{
								Game1.player.name.Value
							});
						}
						break;
					}
					case 28:
						if (questionAndAnswer == "PurpleStarfishSurvey_Correct")
						{
							Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Correct");
							this.plantsRestoredRight.Value = true;
							Game1.multiplayer.globalChatInfoMessage("FinishedSurvey", new string[]
							{
								Game1.player.name.Value
							});
						}
						break;
					}
					break;
				}
			}
			if (!Game1.player.hasOrWillReceiveMail("fieldOfficeFinale") && this.isRangeAllTrue(0, 11) && this.plantsRestoredRight.Value && this.plantsRestoredLeft.Value)
			{
				this.triggerFinaleCutscene();
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x06002FD0 RID: 12240 RVA: 0x0025CAF8 File Offset: 0x0025ACF8
		public override void DayUpdate(int dayOfMonth)
		{
			this.hasFailedSurveyToday.Value = false;
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x06002FD1 RID: 12241 RVA: 0x0025CB10 File Offset: 0x0025AD10
		public virtual void TalkToSafariGuy()
		{
			List<Response> responses = new List<Response>();
			responses.Add(new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")));
			if (this.uncollectedRewards.Count > 0)
			{
				responses.Add(new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")));
			}
			if (this.getRandomUnfoundBoneIndex() != -1)
			{
				responses.Add(new Response("Hint", Game1.content.LoadString("Strings\\Locations:Hint")));
			}
			responses.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave")));
			base.createQuestionDialogue("", responses.ToArray(), "Safari");
		}

		// Token: 0x06002FD2 RID: 12242 RVA: 0x0025CBCC File Offset: 0x0025ADCC
		private int getRandomUnfoundBoneIndex()
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
			for (int i = 0; i < 25; i++)
			{
				int index = r.Next(11);
				if (!this.piecesDonated[index])
				{
					return FieldOfficeMenu.getDonationPieceIndexNeededForSpot(index);
				}
			}
			for (int j = 0; j < this.piecesDonated.Count; j++)
			{
				if (!this.piecesDonated[j])
				{
					return FieldOfficeMenu.getDonationPieceIndexNeededForSpot(j);
				}
			}
			return -1;
		}

		// Token: 0x06002FD3 RID: 12243 RVA: 0x0025CC64 File Offset: 0x0025AE64
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			string a = ArgUtility.Get(action, 0, null, true);
			if (!(a == "FieldOfficeDesk"))
			{
				if (a == "FieldOfficeSurvey")
				{
					if (this.safariGuy != null)
					{
						if (this.hasFailedSurveyToday.Value)
						{
							Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_Failed");
							return true;
						}
						if (!this.plantsRestoredLeft.Value)
						{
							Response[] responses = new Response[]
							{
								new Response("Yes", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Yes")),
								new Response("No", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Notyet"))
							};
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Prompt_LeftPlant"), responses, "Survey");
							(Game1.activeClickableMenu as DialogueBox).aboveDialogueImage = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(194, 167, 16, 17), 1f, 1, 1, Vector2.Zero, false, false)
							{
								scale = 4f
							};
						}
						else if (!this.plantsRestoredRight.Value)
						{
							Response[] responses2 = new Response[]
							{
								new Response("Yes", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Yes")),
								new Response("No", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Notyet"))
							};
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Prompt_RightPlant"), responses2, "Survey");
							(Game1.activeClickableMenu as DialogueBox).aboveDialogueImage = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(193, 150, 16, 16), 1f, 1, 1, Vector2.Zero, false, false)
							{
								scale = 4f
							};
						}
						return true;
					}
				}
			}
			else if (this.safariGuy != null)
			{
				this.safariGuyMutex.RequestLock(new Action(this.TalkToSafariGuy), null);
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x0400204E RID: 8270
		public const int totalPieces = 11;

		// Token: 0x0400204F RID: 8271
		public const int piece_Skeleton_Back_Leg = 0;

		// Token: 0x04002050 RID: 8272
		public const int piece_Skeleton_Ribs = 1;

		// Token: 0x04002051 RID: 8273
		public const int piece_Skeleton_Front_Leg = 2;

		// Token: 0x04002052 RID: 8274
		public const int piece_Skeleton_Tail = 3;

		// Token: 0x04002053 RID: 8275
		public const int piece_Skeleton_Spine = 4;

		// Token: 0x04002054 RID: 8276
		public const int piece_Skeleton_Skull = 5;

		// Token: 0x04002055 RID: 8277
		public const int piece_Snake_Tail = 6;

		// Token: 0x04002056 RID: 8278
		public const int piece_Snake_Spine = 7;

		// Token: 0x04002057 RID: 8279
		public const int piece_Snake_Skull = 8;

		// Token: 0x04002058 RID: 8280
		public const int piece_Bat = 9;

		// Token: 0x04002059 RID: 8281
		public const int piece_Frog = 10;

		// Token: 0x0400205A RID: 8282
		[XmlElement("uncollectedRewards")]
		public NetList<Item, NetRef<Item>> uncollectedRewards = new NetList<Item, NetRef<Item>>();

		// Token: 0x0400205B RID: 8283
		[XmlIgnore]
		public NetMutex safariGuyMutex = new NetMutex();

		// Token: 0x0400205C RID: 8284
		private NPC safariGuy;

		// Token: 0x0400205D RID: 8285
		[XmlElement("piecesDonated")]
		public NetList<bool, NetBool> piecesDonated = new NetList<bool, NetBool>(11);

		// Token: 0x0400205E RID: 8286
		[XmlElement("centerSkeletonRestored")]
		public readonly NetBool centerSkeletonRestored = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x0400205F RID: 8287
		[XmlElement("snakeRestored")]
		public readonly NetBool snakeRestored = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x04002060 RID: 8288
		[XmlElement("batRestored")]
		public readonly NetBool batRestored = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x04002061 RID: 8289
		[XmlElement("frogRestored")]
		public readonly NetBool frogRestored = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x04002062 RID: 8290
		[XmlElement("plantsRestoredLeft")]
		public readonly NetBool plantsRestoredLeft = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x04002063 RID: 8291
		[XmlElement("plantsRestoredRight")]
		public readonly NetBool plantsRestoredRight = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x04002064 RID: 8292
		public readonly NetBool hasFailedSurveyToday = new NetBool();

		// Token: 0x04002065 RID: 8293
		private bool _shouldTriggerFinalCutscene;

		// Token: 0x04002066 RID: 8294
		private float speakerTimer;
	}
}

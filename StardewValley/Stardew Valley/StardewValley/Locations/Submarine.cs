using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002F0 RID: 752
	public class Submarine : GameLocation
	{
		// Token: 0x0600323B RID: 12859 RVA: 0x0028183C File Offset: 0x0027FA3C
		public Submarine()
		{
		}

		// Token: 0x0600323C RID: 12860 RVA: 0x0028185A File Offset: 0x0027FA5A
		public Submarine(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x0600323D RID: 12861 RVA: 0x0028187A File Offset: 0x0027FA7A
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.submerged, "submerged").AddField(this.ascending, "ascending");
		}

		// Token: 0x0600323E RID: 12862 RVA: 0x002818AC File Offset: 0x0027FAAC
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			b.Draw(this.submarineSprites, Game1.GlobalToLocal(new Vector2(9f, 7f) * 64f) + new Vector2(0f, -2f) * 4f, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)(257f + 100f * this.curtainOpenPercent), 0, (int)(100f * (1f - this.curtainOpenPercent)), 80)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.submarineSprites, Game1.GlobalToLocal(new Vector2(15f, 7f) * 64f + new Vector2(-3f, -2f) * 4f + new Vector2(100f * this.curtainOpenPercent, 0f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(357, 0, (int)(100f * (1f - this.curtainOpenPercent)), 80)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.submarineSprites, Game1.GlobalToLocal(new Vector2(82f, 123f) * 4f + new Vector2(0f, (this.submerged.Value && !this.doneUntilReset) ? (104f * (1f - this.submergeTimer / 20000f)) : 0f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(457, 0, 9, 4)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}

		// Token: 0x0600323F RID: 12863 RVA: 0x00281A9C File Offset: 0x0027FC9C
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.hasLitSubmergeLight = false;
			this.curtainOpenPercent = 0f;
			this.curtainMovement = 0f;
			this.submergeTimer = 0f;
			this.submerged.Value = false;
			this.hasLitAscendLight = false;
			this.doneUntilReset = false;
			if (this.submerged.Value)
			{
				this.submerged.Value = false;
			}
			if (this.ascending.Value)
			{
				this.ascending.Value = false;
			}
			Game1.netWorldState.Value.IsSubmarineLocked = false;
		}

		// Token: 0x06003240 RID: 12864 RVA: 0x00281B34 File Offset: 0x0027FD34
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (base.getTileIndexAt(tileLocation, "Buildings", "submarine tiles") != 217)
			{
				return base.checkAction(tileLocation, viewport, who);
			}
			if (this.doneUntilReset)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Submarine_Done"));
				return false;
			}
			if (!this.submerged.Value)
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Submarine_SubmergeQuestion"), base.createYesNoResponses(), "SubmergeQuestion");
			}
			else if (this.submergeTimer <= 0f && this.curtainOpenPercent >= 1f)
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Submarine_AscendQuestion"), base.createYesNoResponses(), "AscendQuestion");
			}
			return true;
		}

		// Token: 0x06003241 RID: 12865 RVA: 0x00281BF0 File Offset: 0x0027FDF0
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "SubmergeQuestion_Yes"))
			{
				if (questionAndAnswer == "AscendQuestion_Yes")
				{
					this.ascending.Value = true;
					this.localAscending = true;
				}
			}
			else if (Game1.player.Money < 1000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 1000;
				this.submerged.Value = true;
				Game1.netWorldState.Value.IsSubmarineLocked = true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x06003242 RID: 12866 RVA: 0x00281C94 File Offset: 0x0027FE94
		private void changeSubmergeLight(bool red, bool clear = false)
		{
			if (clear)
			{
				base.setMapTile(3, 4, 98, "Buildings", "submarine tiles", null, true);
				base.setMapTile(4, 4, 99, "Buildings", "submarine tiles", null, true);
				base.setMapTile(3, 5, 122, "Buildings", "submarine tiles", null, true);
				base.setMapTile(4, 5, 123, "Buildings", "submarine tiles", null, true);
				return;
			}
			if (red)
			{
				base.setMapTile(3, 4, 425, "Buildings", "submarine tiles", null, true);
				base.setMapTile(4, 4, 426, "Buildings", "submarine tiles", null, true);
				base.setMapTile(3, 5, 449, "Buildings", "submarine tiles", null, true);
				base.setMapTile(4, 5, 450, "Buildings", "submarine tiles", null, true);
				return;
			}
			base.setMapTile(3, 4, 427, "Buildings", "submarine tiles", null, true);
			base.setMapTile(4, 4, 428, "Buildings", "submarine tiles", null, true);
			base.setMapTile(3, 5, 451, "Buildings", "submarine tiles", null, true);
			base.setMapTile(4, 5, 452, "Buildings", "submarine tiles", null, true);
		}

		// Token: 0x06003243 RID: 12867 RVA: 0x00281DD5 File Offset: 0x0027FFD5
		protected override void resetSharedState()
		{
			base.resetSharedState();
			this.submerged.Value = false;
			this.ascending.Value = false;
			Game1.netWorldState.Value.IsSubmarineLocked = false;
		}

		// Token: 0x06003244 RID: 12868 RVA: 0x00281E08 File Offset: 0x00280008
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.submarineSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			Game1.ambientLight = Color.Black;
			this.ambientLightTargetColor = Color.Black;
			this.hasLitSubmergeLight = false;
			Game1.background = new Background(this, new Color(0, 50, 255), true);
			this.curtainOpenPercent = 0f;
			this.curtainMovement = 0f;
			this.submergeTimer = 0f;
			this.hasLitAscendLight = false;
			this.doneUntilReset = false;
			this.localAscending = false;
		}

		// Token: 0x06003245 RID: 12869 RVA: 0x00281E9B File Offset: 0x0028009B
		public override bool canFishHere()
		{
			return this.curtainOpenPercent >= 1f;
		}

		// Token: 0x06003246 RID: 12870 RVA: 0x00281EB0 File Offset: 0x002800B0
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			Random r = Utility.CreateDaySaveRandom((double)timeOfDay, 0.0, 0.0);
			if (this.fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 1.0 && this.curtainOpenPercent >= 1f)
			{
				for (int tries = 0; tries < 2; tries++)
				{
					Point p = new Point(r.Next(9, 21), r.Next(7, 12));
					if (base.isOpenWater(p.X, p.Y))
					{
						int toLand = FishingRod.distanceToLand(p.X, p.Y, this, false);
						if (toLand > 1 && toLand < 5)
						{
							if (Game1.player.currentLocation.Equals(this))
							{
								base.playSound("waterSlosh", null, null, SoundContext.Default);
							}
							this.fishSplashPoint.Value = p;
							return;
						}
					}
				}
				return;
			}
			if (!this.fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.25)
			{
				this.fishSplashPoint.Value = Point.Zero;
			}
		}

		// Token: 0x06003247 RID: 12871 RVA: 0x00281FFC File Offset: 0x002801FC
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (!Game1.player.currentLocation.Equals(this))
			{
				return;
			}
			if (!Game1.shouldTimePass(false))
			{
				return;
			}
			if (this.curtainMovement != 0f)
			{
				float old = this.curtainOpenPercent;
				this.curtainOpenPercent = Math.Max(0f, Math.Min(1f, this.curtainOpenPercent + this.curtainMovement * (float)time.ElapsedGameTime.Milliseconds));
				if (this.curtainOpenPercent >= 1f && old < 1f)
				{
					this.curtainMovement = 0f;
					this.changeSubmergeLight(false, false);
					this.ambientLightTargetColor = new Color(200, 150, 100);
					Game1.playSound("newArtifact", null);
					Game1.changeMusicTrack("submarine_song", false, MusicContext.Default);
				}
			}
			if (this.submerged.Value && !this.hasLitSubmergeLight)
			{
				this.changeSubmergeLight(true, false);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 200, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 400, null, null, -1, false);
				Game1.changeMusicTrack("Hospital_Ambient", false, MusicContext.Default);
				this.submergeTimer = 20000f;
				this.hasLitSubmergeLight = true;
				this.ignoreWarps = true;
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.submarineSprites,
					sourceRectStartingPos = new Vector2(457f, 11f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(457, 11, 14, 18),
					initialPosition = new Vector2(21f, 143f) * 4f,
					animationLength = 3,
					pingPong = true,
					position = new Vector2(21f, 143f) * 4f,
					scale = 4f
				});
			}
			if (this.ascending.Value && !this.hasLitAscendLight)
			{
				this.changeSubmergeLight(true, false);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 200, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 400, null, null, -1, false);
				Game1.changeMusicTrack("Hospital_Ambient", false, MusicContext.Default);
				this.submergeTimer = 1f;
				this.hasLitAscendLight = true;
				this.curtainMovement = -0.0002f;
				Game1.playSound("submarine_landing", null);
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.submarineSprites,
					sourceRectStartingPos = new Vector2(457f, 11f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(457, 11, 14, 18),
					initialPosition = new Vector2(21f, 143f) * 4f,
					animationLength = 3,
					pingPong = true,
					position = new Vector2(21f, 143f) * 4f,
					scale = 4f
				});
				if (Game1.IsMasterGame)
				{
					this.fishSplashPoint.Value = Point.Zero;
				}
				if (Game1.activeClickableMenu is BobberBar)
				{
					Game1.activeClickableMenu.emergencyShutDown();
				}
				if (Game1.player.UsingTool)
				{
					FishingRod rod = Game1.player.CurrentTool as FishingRod;
					if (rod != null)
					{
						rod.doneFishing(Game1.player, false);
					}
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in Game1.background.tempSprites)
				{
					temporaryAnimatedSprite.yStopCoordinate = ((temporaryAnimatedSprite.position.X > 320f) ? 320 : 896);
					temporaryAnimatedSprite.motion = new Vector2(0f, 2f);
					temporaryAnimatedSprite.yPeriodic = false;
				}
			}
			if (this.submergeTimer > 0f)
			{
				if (this.ascending.Value && !this.localAscending)
				{
					this.localAscending = true;
				}
				this.submergeTimer -= (float)((this.localAscending ? -1 : 1) * time.ElapsedGameTime.Milliseconds);
				Game1.background.c.B = (byte)(Math.Max(this.submergeTimer / 20000f, 0.2f) * 255f);
				Game1.background.c.G = (byte)(Math.Max(this.submergeTimer / 20000f, 0f) * 50f);
				if (this.submergeTimer <= 0f)
				{
					this.curtainMovement = 0.0002f;
					Game1.changeMusicTrack("none", false, MusicContext.Default);
					Game1.playSound("submarine_landing", null);
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(0f, -1f),
						yStopCoordinate = 120,
						texture = this.submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(257, 98, 182, 25),
						animationLength = 1,
						interval = 999999f,
						position = new Vector2(148f, 56f) * 4f,
						scale = 4f
					});
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(0f, -1f),
						yStopCoordinate = 460,
						texture = this.submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(441, 86, 66, 37),
						animationLength = 1,
						interval = 999999f,
						position = new Vector2(18f, 149f) * 4f,
						scale = 4f
					});
				}
				else
				{
					this.ambientLightTargetColor = new Color((int)((byte)(250f - this.submergeTimer / 20000f * 250f)), (int)((byte)(200f - this.submergeTimer / 20000f * 200f)), (int)((byte)(150f - this.submergeTimer / 20000f * 150f)));
					if (Game1.random.NextDouble() < 0.11)
					{
						Vector2 pos = new Vector2((float)Game1.random.Next(12, this.map.DisplayWidth - 64), (float)(this.ascending.Value ? 1 : 640));
						int which = Game1.random.Next(3);
						Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
						{
							motion = new Vector2(0f, (float)(this.ascending.Value ? -1 : 1) * (-3f + (float)which)),
							yStopCoordinate = (this.ascending.Value ? 832 : 1),
							texture = this.submarineSprites,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(132 + which * 8, 20, 8, 8),
							xPeriodic = true,
							xPeriodicLoopTime = 1500f,
							xPeriodicRange = 12f,
							initialPosition = pos,
							animationLength = 1,
							interval = 5000f,
							position = pos,
							scale = 4f
						});
					}
				}
				if (this.submergeTimer >= 20000f)
				{
					Game1.changeMusicTrack("night_market", false, MusicContext.Default);
					this.ignoreWarps = false;
					this.changeSubmergeLight(true, true);
					Game1.playSound("pullItemFromWater", null);
					Game1.ambientLight = Color.Black;
					this.ambientLightTargetColor = Color.Black;
					this.hasLitSubmergeLight = false;
					Game1.background = new Background(this, new Color(0, 50, 255), true);
					this.curtainOpenPercent = 0f;
					this.curtainMovement = 0f;
					this.submergeTimer = 0f;
					this.submerged.Value = false;
					this.ascending.Value = false;
					Game1.netWorldState.Value.IsSubmarineLocked = false;
					this.hasLitAscendLight = false;
					this.doneUntilReset = false;
					this.localAscending = false;
				}
			}
			else if (this.submerged.Value && !this.doneUntilReset)
			{
				if (Game1.random.NextDouble() < 0.01)
				{
					Vector2 pos2 = new Vector2((float)Game1.random.Next(384, this.map.DisplayWidth - 64), 320f);
					int which2 = Game1.random.Next(3);
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(0f, -1f + (float)which2 * 0.2f),
						yStopCoordinate = 1,
						texture = this.submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(132 + which2 * 8, 20, 8, 8),
						animationLength = 1,
						interval = 20000f,
						xPeriodic = true,
						xPeriodicLoopTime = 1500f,
						xPeriodicRange = 12f,
						initialPosition = pos2,
						position = pos2,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					Vector2 pos3 = new Vector2(1344f, (float)Game1.random.Next(448, 704));
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(-0.5f, 0f),
						xStopCoordinate = 448,
						texture = this.submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(3, 194, 16, 16),
						animationLength = 1,
						interval = 50000f,
						alpha = 0.5f,
						yPeriodic = true,
						yPeriodicLoopTime = 5500f,
						yPeriodicRange = 32f,
						initialPosition = pos3,
						position = pos3,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					Game1.background.tempSprites.Insert(0, new TemporaryAnimatedSprite
					{
						texture = this.submarineSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 146, 16, 13),
						animationLength = 9,
						interval = 100f,
						position = new Vector2((float)(Game1.random.Next(96, 381) * 4), (float)(Game1.random.Next(24, 66) * 4)),
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 5E-05)
				{
					Vector2 pos4 = new Vector2(3f, 10f) * 64f;
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(--0f, -1f),
						color = new Color(0, 50, 150),
						yStopCoordinate = 64,
						texture = this.submarineSprites,
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 50,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						xPeriodic = true,
						xPeriodicLoopTime = 3500f,
						xPeriodicRange = 12f,
						initialPosition = pos4,
						position = pos4,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.00035)
				{
					Vector2 pos5 = new Vector2(24f, 2f) * 64f;
					int which3 = Game1.random.Next(3);
					Game1.background.tempSprites.Add(new TemporaryAnimatedSprite
					{
						motion = new Vector2(-0.5f, 0f),
						xStopCoordinate = 64,
						texture = this.submarineSprites,
						sourceRectStartingPos = new Vector2((float)(257 + which3 * 48), 81f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(257 + which3 * 48, 81, 16, 16),
						totalNumberOfLoops = 250,
						animationLength = 3,
						interval = 200f,
						pingPong = true,
						yPeriodic = true,
						yPeriodicLoopTime = 3500f,
						yPeriodicRange = 12f,
						initialPosition = pos5,
						position = pos5,
						scale = 4f
					});
				}
			}
			if (!Game1.ambientLight.Equals(this.ambientLightTargetColor))
			{
				if (Game1.ambientLight.R < this.ambientLightTargetColor.R)
				{
					byte b = Game1.ambientLight.R;
					Game1.ambientLight.R = b + 1;
				}
				else if (Game1.ambientLight.R > this.ambientLightTargetColor.R)
				{
					byte b = Game1.ambientLight.R;
					Game1.ambientLight.R = b - 1;
				}
				if (Game1.ambientLight.G < this.ambientLightTargetColor.G)
				{
					byte b = Game1.ambientLight.G;
					Game1.ambientLight.G = b + 1;
				}
				else if (Game1.ambientLight.G > this.ambientLightTargetColor.G)
				{
					byte b = Game1.ambientLight.G;
					Game1.ambientLight.G = b - 1;
				}
				if (Game1.ambientLight.B < this.ambientLightTargetColor.B)
				{
					byte b = Game1.ambientLight.B;
					Game1.ambientLight.B = b + 1;
					return;
				}
				if (Game1.ambientLight.B > this.ambientLightTargetColor.B)
				{
					byte b = Game1.ambientLight.B;
					Game1.ambientLight.B = b - 1;
				}
			}
		}

		// Token: 0x06003248 RID: 12872 RVA: 0x00282E4C File Offset: 0x0028104C
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			Game1.background = null;
		}

		// Token: 0x04002197 RID: 8599
		public const float submergeTime = 20000f;

		// Token: 0x04002198 RID: 8600
		public const string MainTileSheetId = "submarine tiles";

		// Token: 0x04002199 RID: 8601
		[XmlElement("submerged")]
		public readonly NetBool submerged = new NetBool();

		// Token: 0x0400219A RID: 8602
		[XmlElement("ascending")]
		public readonly NetBool ascending = new NetBool();

		// Token: 0x0400219B RID: 8603
		private Texture2D submarineSprites;

		// Token: 0x0400219C RID: 8604
		private float curtainMovement;

		// Token: 0x0400219D RID: 8605
		private float curtainOpenPercent;

		// Token: 0x0400219E RID: 8606
		private float submergeTimer;

		// Token: 0x0400219F RID: 8607
		private Color ambientLightTargetColor;

		// Token: 0x040021A0 RID: 8608
		private bool hasLitSubmergeLight;

		// Token: 0x040021A1 RID: 8609
		private bool hasLitAscendLight;

		// Token: 0x040021A2 RID: 8610
		private bool doneUntilReset;

		// Token: 0x040021A3 RID: 8611
		private bool localAscending;
	}
}

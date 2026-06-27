using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002D3 RID: 723
	public class IslandEast : IslandForestLocation
	{
		// Token: 0x06002F7D RID: 12157 RVA: 0x00257E68 File Offset: 0x00256068
		public IslandEast()
		{
		}

		// Token: 0x06002F7E RID: 12158 RVA: 0x00257E92 File Offset: 0x00256092
		public IslandEast(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002F7F RID: 12159 RVA: 0x00257EC0 File Offset: 0x002560C0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.bananaShrineEvent.NetFields, "bananaShrineEvent.NetFields").AddField(this.bananaShrineComplete, "bananaShrineComplete").AddField(this.bananaShrineNutAwarded, "bananaShrineNutAwarded");
			this.bananaShrineEvent.onEvent += this.OnBananaShrine;
		}

		// Token: 0x06002F80 RID: 12160 RVA: 0x00257F28 File Offset: 0x00256128
		public virtual void AddTorchLights()
		{
			base.removeTemporarySpritesWithIDLocal(6666);
			int torch_x = 1280;
			int torch_y = 704;
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1965, 8, 8), new Vector2((float)(torch_x + 24), (float)(torch_y + 48)), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 7,
				lightId = "IslandEast_TorchLight_1",
				id = 6666,
				lightRadius = 1f,
				scale = 3f,
				layerDepth = (float)(torch_y + 48) / 10000f + 0.0001f,
				delayBeforeAnimationStart = 0
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1984, 12, 12), new Vector2((float)(torch_x + 16), (float)(torch_y + 28)), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "IslandEast_TorchLight_2",
				id = 6666,
				lightRadius = 1f,
				scale = 3f,
				layerDepth = (float)(torch_y + 28) / 10000f + 0.0001f,
				delayBeforeAnimationStart = 0
			});
			torch_x = 1472;
			torch_y = 704;
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1965, 8, 8), new Vector2((float)(torch_x + 24), (float)(torch_y + 48)), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 7,
				lightId = "IslandEast_TorchLight_3",
				id = 6666,
				lightRadius = 1f,
				scale = 3f,
				layerDepth = (float)(torch_y + 48) / 10000f + 0.0001f,
				delayBeforeAnimationStart = 0
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1984, 12, 12), new Vector2((float)(torch_x + 16), (float)(torch_y + 28)), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "IslandEast_TorchLight_4",
				id = 6666,
				lightRadius = 1f,
				scale = 3f,
				layerDepth = (float)(torch_y + 28) / 10000f + 0.0001f,
				delayBeforeAnimationStart = 0
			});
		}

		// Token: 0x06002F81 RID: 12161 RVA: 0x002581F8 File Offset: 0x002563F8
		protected override void resetLocalState()
		{
			this._parrotTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
			base.resetLocalState();
			for (int i = 0; i < 5; i++)
			{
				Vector2 v = Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(14, 3, 16, 12), Game1.random);
				this.critters.Add(new Firefly(v));
			}
			this.AddTorchLights();
			if (this.bananaShrineComplete.Value)
			{
				this.AddGorillaShrineTorches(0);
			}
			this._parrots = new PerchingBirds(this._parrotTextures, 3, 24, 24, new Vector2(12f, 19f), new Point[]
			{
				new Point(18, 8),
				new Point(17, 9),
				new Point(20, 7),
				new Point(21, 8),
				new Point(22, 7),
				new Point(23, 8),
				new Point(18, 12),
				new Point(25, 11),
				new Point(27, 8)
			}, new Point[0]);
			this._parrots.peckDuration = 0;
			for (int j = 0; j < 5; j++)
			{
				this._parrots.AddBird(Game1.random.Next(0, 4));
			}
			if (this.bananaShrineComplete.Value && Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 1111.0, 0.0, 0.0).NextDouble() < 0.1)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32), 500f, 2, 999, new Vector2(15.5f, 19f) * 64f, false, false, 0.1216f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					id = 888,
					yStopCoordinate = 1497,
					motion = new Vector2(0f, 1f),
					reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.gorillaReachedShrineCosmetic),
					delayBeforeAnimationStart = 1000
				});
			}
			base.addOneTimeGiftBox(ItemRegistry.Create("(O)TentKit", 3, 0, false), 30, 40, 4);
		}

		// Token: 0x06002F82 RID: 12162 RVA: 0x0025847E File Offset: 0x0025667E
		public override void cleanupBeforePlayerExit()
		{
			this._parrots = null;
			this._parrotTextures = null;
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06002F83 RID: 12163 RVA: 0x00258494 File Offset: 0x00256694
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.bananaShrineEvent.Poll();
			PerchingBirds parrots = this._parrots;
			if (parrots != null)
			{
				parrots.Update(time);
			}
			if (this.bananaShrineComplete.Value && Game1.random.NextDouble() < 0.005)
			{
				TemporaryAnimatedSprite t = base.getTemporarySpriteByID(888);
				if (t != null && t.motion.Equals(Vector2.Zero))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 352, 32, 32), (float)(200 + ((Game1.random.NextDouble() < 0.1) ? Game1.random.Next(1000, 3000) : 0)), 1, 1, t.position, false, false, 0.12224f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
				}
			}
		}

		// Token: 0x06002F84 RID: 12164 RVA: 0x002585A0 File Offset: 0x002567A0
		public virtual void SpawnBananaNutReward()
		{
			if (!this.bananaShrineNutAwarded.Value && Game1.IsMasterGame)
			{
				Game1.player.team.MarkCollectedNut("BananaShrine");
				this.bananaShrineNutAwarded.Value = true;
				for (int i = 0; i < 3; i++)
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(16.5f, 25f) * 64f, 0, this, 1280, false);
				}
			}
		}

		// Token: 0x06002F85 RID: 12165 RVA: 0x00258624 File Offset: 0x00256824
		public override void DayUpdate(int dayOfMonth)
		{
			if (Game1.IsMasterGame && this.bananaShrineComplete.Value && !this.bananaShrineNutAwarded.Value)
			{
				this.SpawnBananaNutReward();
			}
			base.DayUpdate(dayOfMonth);
			Microsoft.Xna.Framework.Rectangle parrot_platform_rect = new Microsoft.Xna.Framework.Rectangle(27, 27, 3, 3);
			for (int i = 0; i < 8; i++)
			{
				Vector2 v = base.getRandomTile(null);
				if (v.Y < 24f)
				{
					v.Y += 24f;
				}
				if (v.X > 4f && !base.hasTileAt((int)v.X, (int)v.Y, "AlwaysFront", null) && this.CanItemBePlacedHere(v, false, CollisionMask.All, CollisionMask.None, false, false) && this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back") == "Grass" && !this.IsNoSpawnTile(v, "All", false) && this.doesTileHavePropertyNoNull((int)v.X + 1, (int)v.Y, "Type", "Back") != "Stone" && this.doesTileHavePropertyNoNull((int)v.X - 1, (int)v.Y, "Type", "Back") != "Stone" && this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y + 1, "Type", "Back") != "Stone" && this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y - 1, "Type", "Back") != "Stone" && !parrot_platform_rect.Contains((int)v.X, (int)v.Y))
				{
					if (Game1.random.NextDouble() < 0.04)
					{
						Object fiddlehead = ItemRegistry.Create<Object>("(O)259", 1, 0, false);
						fiddlehead.isSpawnedObject.Value = true;
						this.objects.Add(v, fiddlehead);
					}
					else
					{
						this.objects.Add(v, ItemRegistry.Create<Object>("(O)" + (882 + Game1.random.Next(3)).ToString(), 1, 0, false));
					}
				}
			}
		}

		// Token: 0x06002F86 RID: 12166 RVA: 0x0025886F File Offset: 0x00256A6F
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			PerchingBirds parrots = this._parrots;
			if (parrots != null)
			{
				parrots.Draw(b);
			}
			base.drawAboveAlwaysFrontLayer(b);
		}

		// Token: 0x06002F87 RID: 12167 RVA: 0x0025888C File Offset: 0x00256A8C
		public virtual void AddGorillaShrineTorches(int delay)
		{
			if (base.getTemporarySpriteByID(12038) == null)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(15f, 24f) * 64f + new Vector2(8f, -16f), false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandEast_GorillaTorch_1",
					lightRadius = 2f,
					delayBeforeAnimationStart = delay,
					scale = 4f,
					layerDepth = 0.16704f,
					id = 12038
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(17f, 24f) * 64f + new Vector2(8f, -16f), false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandEast_GorillaTorch_2",
					lightRadius = 2f,
					delayBeforeAnimationStart = delay,
					scale = 4f,
					layerDepth = 0.16704f,
					id = 12097
				});
			}
		}

		// Token: 0x06002F88 RID: 12168 RVA: 0x00258A20 File Offset: 0x00256C20
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandEast location = l as IslandEast;
			if (location != null)
			{
				this.bananaShrineComplete.Value = location.bananaShrineComplete.Value;
				this.bananaShrineNutAwarded.Value = location.bananaShrineNutAwarded.Value;
			}
		}

		// Token: 0x06002F89 RID: 12169 RVA: 0x00258A6C File Offset: 0x00256C6C
		public virtual void OnBananaShrine()
		{
			Location tileLocation = new Location(16, 26);
			this.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(304, 48, 16, 16), new Vector2(16f, (float)(tileLocation.Y - 1)) * 64f, false, 0f, Color.White)
			{
				id = 88976,
				scale = 4f,
				layerDepth = ((float)tileLocation.Y + 1.2f) * 64f / 10000f,
				dontClearOnAreaEntry = true
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32), 400f, 2, 999, new Vector2(15.5f, 19f) * 64f, false, false, 0.1216f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
			{
				id = 777,
				yStopCoordinate = 1497,
				motion = new Vector2(0f, 2f),
				reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.gorillaReachedShrine),
				delayBeforeAnimationStart = 1000,
				dontClearOnAreaEntry = true
			});
			if (Game1.currentLocation == this)
			{
				Game1.playSound("coin", null);
				DelayedAction.playSoundAfterDelay("fireball", 800, null, null, -1, false);
			}
			this.AddGorillaShrineTorches(800);
			if (Game1.currentLocation == this)
			{
				DelayedAction.playSoundAfterDelay("grassyStep", 1400, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("grassyStep", 1800, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("grassyStep", 2200, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("grassyStep", 2600, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("grassyStep", 3000, null, null, -1, false);
				Game1.changeMusicTrack("none", false, MusicContext.Default);
				DelayedAction.playSoundAfterDelay("gorilla_intro", 2000, null, null, -1, false);
			}
		}

		// Token: 0x06002F8A RID: 12170 RVA: 0x00258CC8 File Offset: 0x00256EC8
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "BananaShrine")
			{
				Item currentItem = who.CurrentItem;
				if (((currentItem != null) ? currentItem.QualifiedItemId : null) == "(O)91" && base.getTemporarySpriteByID(777) == null && !this.bananaShrineComplete.Value)
				{
					this.bananaShrineComplete.Value = true;
					who.reduceActiveItemByOne();
					this.bananaShrineEvent.Fire();
					return true;
				}
				if (base.getTemporarySpriteByID(777) == null && !this.bananaShrineComplete.Value)
				{
					who.doEmote(8);
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06002F8B RID: 12171 RVA: 0x00258D6C File Offset: 0x00256F6C
		private void gorillaReachedShrine(int extra)
		{
			TemporaryAnimatedSprite temporarySpriteByID = base.getTemporarySpriteByID(777);
			temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			temporarySpriteByID.sourceRectStartingPos = Utility.PointToVector2(temporarySpriteByID.sourceRect.Location);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 1;
			temporarySpriteByID.interval = 1000f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.motion = Vector2.Zero;
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaGrabBanana);
		}

		// Token: 0x06002F8C RID: 12172 RVA: 0x00258DF8 File Offset: 0x00256FF8
		private void gorillaReachedShrineCosmetic(int extra)
		{
			TemporaryAnimatedSprite temporarySpriteByID = base.getTemporarySpriteByID(888);
			temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(192, 352, 32, 32);
			temporarySpriteByID.sourceRectStartingPos = Utility.PointToVector2(temporarySpriteByID.sourceRect.Location);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 999999;
			temporarySpriteByID.interval = 8000f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.motion = Vector2.Zero;
			temporarySpriteByID.animationLength = 1;
		}

		// Token: 0x06002F8D RID: 12173 RVA: 0x00258E78 File Offset: 0x00257078
		private void gorillaGrabBanana(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			DelayedAction.functionAfterDelay(delegate
			{
				base.removeTemporarySpritesWithID(88976);
			}, 50);
			if (Game1.currentLocation == this)
			{
				Game1.playSound("slimeHit", null);
			}
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.timer = 0f;
			gorilla.animationLength = 1;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaEatBanana);
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06002F8E RID: 12174 RVA: 0x00258F38 File Offset: 0x00257138
		private void gorillaEatBanana(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(128, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 5;
			gorilla.interval = 300f;
			gorilla.timer = 0f;
			gorilla.animationLength = 2;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaAfterEat);
			if (Game1.currentLocation == this)
			{
				Game1.playSound("eat", null);
				DelayedAction.playSoundAfterDelay("eat", 600, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("eat", 1200, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("eat", 1800, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("eat", 2400, null, null, -1, false);
			}
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06002F8F RID: 12175 RVA: 0x00259054 File Offset: 0x00257254
		private void gorillaAfterEat(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.timer = 0f;
			gorilla.motion = Vector2.Zero;
			gorilla.animationLength = 1;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaSpawnNut);
			gorilla.shakeIntensity = 1f;
			gorilla.shakeIntensityChange = -0.01f;
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06002F90 RID: 12176 RVA: 0x00259104 File Offset: 0x00257304
		private void gorillaSpawnNut(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.shakeIntensity = 2f;
			gorilla.shakeIntensityChange = -0.01f;
			if (Game1.currentLocation == this)
			{
				Game1.playSound("grunt", null);
			}
			if (Game1.IsMasterGame)
			{
				this.SpawnBananaNutReward();
			}
			gorilla.timer = 0f;
			gorilla.motion = Vector2.Zero;
			gorilla.animationLength = 1;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaReturn);
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06002F91 RID: 12177 RVA: 0x002591DC File Offset: 0x002573DC
		private void gorillaReturn(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 6;
			gorilla.interval = 200f;
			gorilla.timer = 0f;
			gorilla.motion = new Vector2(0f, -3f);
			gorilla.animationLength = 2;
			gorilla.yStopCoordinate = 1280;
			gorilla.reachedStopCoordinate = delegate(int x)
			{
				base.removeTemporarySpritesWithID(777);
			};
			this.temporarySprites.Add(gorilla);
			if (Game1.currentLocation == this)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playMorningSong(false);
				}, 3000);
			}
		}

		// Token: 0x0400203B RID: 8251
		protected PerchingBirds _parrots;

		// Token: 0x0400203C RID: 8252
		protected Texture2D _parrotTextures;

		// Token: 0x0400203D RID: 8253
		protected NetEvent0 bananaShrineEvent = new NetEvent0(false);

		// Token: 0x0400203E RID: 8254
		public NetBool bananaShrineComplete = new NetBool();

		// Token: 0x0400203F RID: 8255
		public NetBool bananaShrineNutAwarded = new NetBool();
	}
}

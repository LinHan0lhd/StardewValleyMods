using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events
{
	// Token: 0x0200032C RID: 812
	public class WitchEvent : BaseFarmEvent
	{
		// Token: 0x060034C0 RID: 13504 RVA: 0x002A217C File Offset: 0x002A037C
		public override bool setUp()
		{
			this.lightSourceId = this.GenerateLightSourceId();
			this.f = Game1.getFarm();
			this.r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
			foreach (Building b in this.f.buildings)
			{
				if (b.buildingType.Value == "Big Coop" || b.buildingType.Value == "Deluxe Coop")
				{
					AnimalHouse animalHouse = (AnimalHouse)b.GetIndoors();
					if (!animalHouse.isFull() && animalHouse.objects.Length < 50 && this.r.NextDouble() < 0.8)
					{
						this.targetBuilding = b;
						if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && this.r.NextDouble() < 0.6)
						{
							this.goldenWitch = true;
						}
					}
				}
			}
			if (this.targetBuilding == null)
			{
				foreach (Building b2 in this.f.buildings)
				{
					if (b2.buildingType.Value == "Slime Hutch")
					{
						GameLocation indoors = b2.GetIndoors();
						if (indoors.characters.Count > 0 && this.r.NextBool() && indoors.numberOfObjectsOfType("83", true) == 0)
						{
							this.targetBuilding = b2;
						}
					}
				}
			}
			if (this.targetBuilding == null)
			{
				return true;
			}
			Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, this.witchPosition, 2f, Color.Black, LightSource.LightContext.None, 0L, null));
			Game1.currentLocation = this.f;
			this.f.resetForPlayerEntry();
			Game1.fadeClear();
			Game1.nonWarpFade = true;
			Game1.timeOfDay = 2400;
			Game1.ambientLight = new Color(200, 190, 40);
			Game1.displayHUD = false;
			Game1.freezeControls = true;
			Game1.viewportFreeze = true;
			Game1.displayFarmer = false;
			Game1.viewport.X = Math.Max(0, Math.Min(this.f.map.DisplayWidth - Game1.viewport.Width, this.targetBuilding.tileX.Value * 64 - Game1.viewport.Width / 2));
			Game1.viewport.Y = Math.Max(0, Math.Min(this.f.map.DisplayHeight - Game1.viewport.Height, (this.targetBuilding.tileY.Value - 3) * 64 - Game1.viewport.Height / 2));
			this.witchPosition = new Vector2((float)(Game1.viewport.X + Game1.viewport.Width + 128), (float)(this.targetBuilding.tileY.Value * 64 - 64));
			Game1.changeMusicTrack("nightTime", false, MusicContext.Default);
			DelayedAction.playSoundAfterDelay(this.goldenWitch ? "yoba" : "cacklingWitch", 3200, null, null, -1, false);
			return false;
		}

		// Token: 0x060034C1 RID: 13505 RVA: 0x002A2508 File Offset: 0x002A0708
		public override bool tickUpdate(GameTime time)
		{
			if (this.terminate)
			{
				return true;
			}
			Game1.UpdateGameClock(time);
			this.f.UpdateWhenCurrentLocation(time);
			this.f.updateEvenIfFarmerIsntHere(time, false);
			Game1.UpdateOther(time);
			Utility.repositionLightSource(this.lightSourceId, this.witchPosition + new Vector2(32f, 32f));
			if (this.animationLoopsDone < 1)
			{
				this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
			}
			if (this.witchPosition.X > (float)(this.targetBuilding.tileX.Value * 64 + 96))
			{
				if (this.timerSinceFade < 2000)
				{
					return false;
				}
				this.witchPosition.X = this.witchPosition.X - (float)time.ElapsedGameTime.Milliseconds * 0.4f;
				this.witchPosition.Y = this.witchPosition.Y + (float)Math.Cos((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 512.0) * 1f;
			}
			else if (this.animationLoopsDone < 4)
			{
				this.witchPosition.Y = this.witchPosition.Y + (float)Math.Cos((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 512.0) * 1f;
				this.witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
				if (this.witchAnimationTimer > 2000)
				{
					this.witchAnimationTimer = 0;
					if (!this.animateLeft)
					{
						this.witchFrame++;
						if (this.witchFrame == 1)
						{
							this.animateLeft = true;
							for (int i = 0; i < 75; i++)
							{
								this.f.temporarySprites.Add(new TemporaryAnimatedSprite(10, this.witchPosition + new Vector2(8f, 80f), this.goldenWitch ? (this.r.NextBool() ? Color.Gold : new Color(255, 150, 0)) : (this.r.NextBool() ? Color.Lime : Color.DarkViolet), 8, false, 100f, 0, -1, -1f, -1, 0)
								{
									motion = new Vector2((float)this.r.Next(-100, 100) / 100f, 1.5f),
									alphaFade = 0.015f,
									delayBeforeAnimationStart = i * 30,
									layerDepth = 1f
								});
							}
							Game1.playSound(this.goldenWitch ? "discoverMineral" : "debuffSpell", null);
						}
					}
					else
					{
						this.witchFrame--;
						this.animationLoopsDone = 4;
						DelayedAction.playSoundAfterDelay(this.goldenWitch ? "yoba" : "cacklingWitch", 2500, null, null, -1, false);
					}
				}
			}
			else
			{
				this.witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
				this.witchFrame = 0;
				if (this.witchAnimationTimer > 1000 && this.witchPosition.X > -999999f)
				{
					this.witchPosition.Y = this.witchPosition.Y + (float)Math.Cos((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 256.0) * 2f;
					this.witchPosition.X = this.witchPosition.X - (float)time.ElapsedGameTime.Milliseconds * 0.4f;
				}
				if (this.witchPosition.X < (float)(Game1.viewport.X - 128) || float.IsNaN(this.witchPosition.X))
				{
					if (!Game1.fadeToBlack && this.witchPosition.X != -999999f)
					{
						Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.afterLastFade), 0.02f);
						Game1.changeMusicTrack("none", false, MusicContext.Default);
						this.timerSinceFade = 0;
						this.witchPosition.X = -999999f;
					}
					this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
				}
			}
			return false;
		}

		// Token: 0x060034C2 RID: 13506 RVA: 0x002A295D File Offset: 0x002A0B5D
		public void afterLastFade()
		{
			this.terminate = true;
			Game1.globalFadeToClear(null, 0.02f);
		}

		// Token: 0x060034C3 RID: 13507 RVA: 0x002A2974 File Offset: 0x002A0B74
		public override void draw(SpriteBatch b)
		{
			if (this.goldenWitch)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, this.witchPosition), new Rectangle?(new Rectangle(215, 262 + this.witchFrame * 29, 34, 29)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
				return;
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.witchPosition), new Rectangle?(new Rectangle(277, 1886 + this.witchFrame * 29, 34, 29)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}

		// Token: 0x060034C4 RID: 13508 RVA: 0x002A2A38 File Offset: 0x002A0C38
		public override void makeChangesToLocation()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			GameLocation indoors = this.targetBuilding.GetIndoors();
			if (this.targetBuilding.buildingType.Value == "Slime Hutch")
			{
				using (List<NPC>.Enumerator enumerator = indoors.characters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NPC npc = enumerator.Current;
						GreenSlime slime = npc as GreenSlime;
						if (slime != null)
						{
							slime.color.Value = new Color(40 + this.r.Next(10), 40 + this.r.Next(10), 40 + this.r.Next(10));
						}
					}
					return;
				}
			}
			for (int tries = 0; tries < 200; tries++)
			{
				Vector2 v = new Vector2((float)this.r.Next(2, indoors.Map.Layers[0].LayerWidth - 2), (float)this.r.Next(2, indoors.Map.Layers[0].LayerHeight - 2));
				TerrainFeature terrainFeature;
				if ((indoors.CanItemBePlacedHere(v, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || (indoors.terrainFeatures.TryGetValue(v, out terrainFeature) && terrainFeature is Flooring)) && !indoors.objects.ContainsKey(v))
				{
					Object egg = ItemRegistry.Create<Object>(this.goldenWitch ? "(O)928" : "(O)305", 1, 0, false);
					egg.CanBeSetDown = false;
					egg.IsSpawnedObject = true;
					indoors.objects.Add(v, egg);
					return;
				}
			}
		}

		// Token: 0x04002282 RID: 8834
		public string lightSourceId;

		// Token: 0x04002283 RID: 8835
		private Vector2 witchPosition;

		// Token: 0x04002284 RID: 8836
		private Building targetBuilding;

		// Token: 0x04002285 RID: 8837
		private Farm f;

		// Token: 0x04002286 RID: 8838
		private Random r;

		// Token: 0x04002287 RID: 8839
		private int witchFrame;

		// Token: 0x04002288 RID: 8840
		private int witchAnimationTimer;

		// Token: 0x04002289 RID: 8841
		private int animationLoopsDone;

		// Token: 0x0400228A RID: 8842
		private int timerSinceFade;

		// Token: 0x0400228B RID: 8843
		private bool animateLeft;

		// Token: 0x0400228C RID: 8844
		private bool terminate;

		// Token: 0x0400228D RID: 8845
		public bool goldenWitch;
	}
}

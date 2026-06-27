using System;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;

namespace StardewValley.Locations
{
	// Token: 0x020002CF RID: 719
	public class FarmCave : GameLocation
	{
		// Token: 0x06002F01 RID: 12033 RVA: 0x0024DC67 File Offset: 0x0024BE67
		public FarmCave()
		{
		}

		// Token: 0x06002F02 RID: 12034 RVA: 0x0024DC6F File Offset: 0x0024BE6F
		public FarmCave(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002F03 RID: 12035 RVA: 0x0024DC7C File Offset: 0x0024BE7C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.MasterPlayer.caveChoice.Value == 1)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(0f, 0f), false, 0f, Color.White)
				{
					interval = 3000f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					layerDepth = 1f,
					lightId = "FarmCave_1",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(8f, 0f), false, 0f, Color.White)
				{
					interval = 3000f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					layerDepth = 1f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(320f, -64f), false, 0f, Color.White)
				{
					interval = 2000f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 500,
					layerDepth = 1f,
					lightId = "FarmCave_2",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(328f, -64f), false, 0f, Color.White)
				{
					interval = 2000f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 500,
					layerDepth = 1f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(128f, (float)(this.map.Layers[0].LayerHeight * 64 - 64)), false, 0f, Color.White)
				{
					interval = 1600f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 250,
					layerDepth = 1f,
					lightId = "FarmCave_3",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(136f, (float)(this.map.Layers[0].LayerHeight * 64 - 64)), false, 0f, Color.White)
				{
					interval = 1600f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 250,
					layerDepth = 1f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2((float)((this.map.Layers[0].LayerWidth + 1) * 64 + 4), 192f), false, 0f, Color.White)
				{
					interval = 2800f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 750,
					layerDepth = 1f,
					lightId = "FarmCave_4",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2((float)((this.map.Layers[0].LayerWidth + 1) * 64 + 12), 192f), false, 0f, Color.White)
				{
					interval = 2800f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 750,
					layerDepth = 1f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2((float)((this.map.Layers[0].LayerWidth + 1) * 64 + 4), 576f), false, 0f, Color.White)
				{
					interval = 2200f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 750,
					layerDepth = 1f,
					lightId = "FarmCave_5",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2((float)((this.map.Layers[0].LayerWidth + 1) * 64 + 12), 576f), false, 0f, Color.White)
				{
					interval = 2200f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 750,
					layerDepth = 1f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(-60f, 128f), false, 0f, Color.White)
				{
					interval = 2600f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 750,
					layerDepth = 1f,
					lightId = "FarmCave_6",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(-52f, 128f), false, 0f, Color.White)
				{
					interval = 2600f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 750,
					layerDepth = 1f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(-64f, 384f), false, 0f, Color.White)
				{
					interval = 3400f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 650,
					layerDepth = 1f,
					lightId = "FarmCave_7",
					lightRadius = 0.5f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(374, 358, 1, 1), new Vector2(-52f, 384f), false, 0f, Color.White)
				{
					interval = 3400f,
					animationLength = 3,
					totalNumberOfLoops = 99999,
					scale = 4f,
					delayBeforeAnimationStart = 650,
					layerDepth = 1f
				});
				Game1.ambientLight = new Color(70, 90, 0);
			}
		}

		// Token: 0x06002F04 RID: 12036 RVA: 0x0024E4B4 File Offset: 0x0024C6B4
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (Game1.MasterPlayer.caveChoice.Value == 1)
			{
				if (Game1.random.NextDouble() < 0.002 && Game1.currentLocation == this)
				{
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2((float)Game1.random.Next(this.map.Layers[0].LayerWidth), (float)this.map.Layers[0].LayerHeight) * 64f, false, false, 1f, 0f, Color.Black, 4f, 0f, 0f, 0f, false)
					{
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 64f,
						motion = new Vector2(0f, -8f)
					});
					if (Game1.random.NextDouble() < 0.15 && Game1.currentLocation == this)
					{
						base.localSound("batScreech", null, null, SoundContext.Default);
					}
					for (int i = 1; i < 5; i++)
					{
						DelayedAction.playSoundAfterDelay("batFlap", 320 * i - 80, null, null, -1, false);
					}
					return;
				}
				if (Game1.random.NextDouble() < 0.005)
				{
					this.temporarySprites.Add(new BatTemporarySprite(new Vector2((float)(Game1.random.NextBool() ? 0 : (this.map.DisplayWidth - 64)), (float)(this.map.DisplayHeight - 64))));
				}
			}
		}

		// Token: 0x06002F05 RID: 12037 RVA: 0x0024E691 File Offset: 0x0024C891
		public override void checkForMusic(GameTime time)
		{
		}

		// Token: 0x06002F06 RID: 12038 RVA: 0x0024E693 File Offset: 0x0024C893
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			if (Game1.currentLocation == this)
			{
				this.UpdateReadyFlag();
			}
			base.performTenMinuteUpdate(timeOfDay);
		}

		// Token: 0x06002F07 RID: 12039 RVA: 0x0024E6AA File Offset: 0x0024C8AA
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			this.UpdateReadyFlag();
		}

		// Token: 0x06002F08 RID: 12040 RVA: 0x0024E6B8 File Offset: 0x0024C8B8
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (Game1.MasterPlayer.caveChoice.Value == 1)
			{
				while (Game1.random.NextDouble() < 0.66)
				{
					string fruitId;
					switch (Game1.random.Next(5))
					{
					case 0:
						fruitId = "296";
						break;
					case 1:
						fruitId = "396";
						break;
					case 2:
						fruitId = "406";
						break;
					case 3:
						fruitId = "410";
						break;
					default:
						fruitId = ((Game1.random.NextDouble() < 0.1) ? "613" : Game1.random.Next(634, 639).ToString());
						break;
					}
					Vector2 v = new Vector2((float)Game1.random.Next(1, this.map.Layers[0].LayerWidth - 1), (float)Game1.random.Next(1, this.map.Layers[0].LayerHeight - 4));
					Object fruit = ItemRegistry.Create<Object>("(O)" + fruitId, 1, 0, false);
					fruit.IsSpawnedObject = true;
					if (this.CanItemBePlacedHere(v, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						base.setObject(v, fruit);
					}
				}
			}
			this.UpdateReadyFlag();
		}

		// Token: 0x06002F09 RID: 12041 RVA: 0x0024E80C File Offset: 0x0024CA0C
		public virtual void UpdateReadyFlag()
		{
			bool flag_value = false;
			foreach (Object o in this.objects.Values)
			{
				if (o.isSpawnedObject.Value)
				{
					flag_value = true;
					break;
				}
				if (o.bigCraftable.Value && o.heldObject.Value != null && o.MinutesUntilReady <= 0 && o.QualifiedItemId == "(BC)128")
				{
					flag_value = true;
					break;
				}
			}
			Game1.getFarm().farmCaveReady.Value = flag_value;
		}

		// Token: 0x06002F0A RID: 12042 RVA: 0x0024E8BC File Offset: 0x0024CABC
		public void setUpMushroomHouse()
		{
			foreach (int y in new int[]
			{
				5,
				7
			})
			{
				foreach (int x in new int[]
				{
					4,
					6,
					8
				})
				{
					Object mushroomBox = ItemRegistry.Create<Object>("(BC)128", 1, 0, false);
					mushroomBox.fragility.Value = 2;
					base.setObject(new Vector2((float)x, (float)y), mushroomBox);
				}
			}
			base.setObject(new Vector2(10f, 5f), ItemRegistry.Create<Object>("(BC)Dehydrator", 1, 0, false));
		}
	}
}

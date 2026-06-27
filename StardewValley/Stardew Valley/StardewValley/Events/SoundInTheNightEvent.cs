using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using xTile.Layers;

namespace StardewValley.Events
{
	// Token: 0x0200032B RID: 811
	public class SoundInTheNightEvent : BaseFarmEvent
	{
		// Token: 0x060034B7 RID: 13495 RVA: 0x002A18B3 File Offset: 0x0029FAB3
		public SoundInTheNightEvent() : this(0)
		{
		}

		// Token: 0x060034B8 RID: 13496 RVA: 0x002A18BC File Offset: 0x0029FABC
		public SoundInTheNightEvent(int which)
		{
			this.behavior.Value = which;
		}

		// Token: 0x060034B9 RID: 13497 RVA: 0x002A18E6 File Offset: 0x0029FAE6
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.behavior, "behavior");
		}

		// Token: 0x060034BA RID: 13498 RVA: 0x002A1908 File Offset: 0x0029FB08
		public override bool setUp()
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
			Farm f = Game1.getFarm();
			f.updateMap();
			this.timer = 0f;
			switch (this.behavior.Value)
			{
			case 0:
			{
				this.soundName = "UFO";
				this.message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_UFO");
				int attempts = 50;
				Layer backLayer = f.map.RequireLayer("Back");
				while (attempts > 0)
				{
					this.targetLocation = new Vector2((float)r.Next(5, backLayer.LayerWidth - 4), (float)r.Next(5, backLayer.LayerHeight - 4));
					if (f.CanItemBePlacedHere(this.targetLocation, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						break;
					}
					attempts--;
				}
				if (attempts <= 0)
				{
					return true;
				}
				break;
			}
			case 1:
			{
				this.soundName = "Meteorite";
				this.message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Meteorite");
				Layer backLayer2 = f.map.RequireLayer("Back");
				this.targetLocation = new Vector2((float)r.Next(5, backLayer2.LayerWidth - 20), (float)r.Next(5, backLayer2.LayerHeight - 4));
				int x = (int)this.targetLocation.X;
				while ((float)x <= this.targetLocation.X + 1f)
				{
					int y = (int)this.targetLocation.Y;
					while ((float)y <= this.targetLocation.Y + 1f)
					{
						Vector2 v = new Vector2((float)x, (float)y);
						if (!f.isTileOpenBesidesTerrainFeatures(v) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y - 1f)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X, v.Y - 1f)) || f.isWaterTile((int)v.X, (int)v.Y) || f.isWaterTile((int)v.X + 1, (int)v.Y))
						{
							return true;
						}
						y++;
					}
					x++;
				}
				break;
			}
			case 2:
				this.soundName = "dogs";
				if (r.NextBool())
				{
					return true;
				}
				foreach (Building b in f.buildings)
				{
					AnimalHouse animalHouse = b.GetIndoors() as AnimalHouse;
					if (animalHouse != null && !b.animalDoorOpen.Value && animalHouse.animalsThatLiveHere.Count > animalHouse.animals.Length && r.NextDouble() < (double)(1f / (float)f.buildings.Count))
					{
						this.targetBuilding = b;
						break;
					}
				}
				return this.targetBuilding == null;
			case 3:
			{
				this.soundName = "owl";
				int attempts2 = 50;
				Layer backLayer3 = f.map.RequireLayer("Back");
				while (attempts2 > 0)
				{
					this.targetLocation = new Vector2((float)r.Next(5, backLayer3.LayerWidth - 4), (float)r.Next(5, backLayer3.LayerHeight - 4));
					if (f.CanItemBePlacedHere(this.targetLocation, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						break;
					}
					attempts2--;
				}
				if (attempts2 <= 0)
				{
					return true;
				}
				break;
			}
			case 4:
				this.soundName = "thunder_small";
				this.message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Earthquake");
				break;
			case 5:
				this.soundName = "windstorm";
				this.message = Game1.content.LoadString("Strings\\1_6_Strings:windstorm");
				this.timeUntilText = 14000f;
				Game1.player.mailReceived.Add("raccoonTreeFallen");
				break;
			}
			Game1.freezeControls = true;
			return false;
		}

		// Token: 0x060034BB RID: 13499 RVA: 0x002A1D4C File Offset: 0x0029FF4C
		public override bool tickUpdate(GameTime time)
		{
			this.timer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > 1500f && !this.playedSound)
			{
				if (!string.IsNullOrEmpty(this.soundName))
				{
					Game1.playSound(this.soundName, null);
					this.playedSound = true;
				}
				if (!this.playedSound && this.message != null)
				{
					Game1.drawObjectDialogue(this.message);
					Game1.globalFadeToClear(null, 0.02f);
					this.showedMessage = true;
					if (this.message == null)
					{
						this.finished = true;
					}
					else
					{
						Game1.afterDialogues = delegate()
						{
							this.finished = true;
						};
					}
				}
			}
			if (this.timer > this.timeUntilText && !this.showedMessage)
			{
				Game1.pauseThenMessage(10, this.message);
				this.showedMessage = true;
				if (this.message == null)
				{
					this.finished = true;
				}
				else
				{
					Game1.afterDialogues = delegate()
					{
						this.finished = true;
					};
				}
			}
			if (this.finished)
			{
				Game1.freezeControls = false;
				return true;
			}
			return false;
		}

		// Token: 0x060034BC RID: 13500 RVA: 0x002A1E64 File Offset: 0x002A0064
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
			if (!this.showedMessage)
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2(12f, (float)(Game1.viewport.Height - 12 - 76)), new Rectangle?(new Rectangle(256 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 19, 413, 19, 19)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x060034BD RID: 13501 RVA: 0x002A1F44 File Offset: 0x002A0144
		public override void makeChangesToLocation()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farm f = Game1.getFarm();
			switch (this.behavior.Value)
			{
			case 0:
			{
				Object o = ItemRegistry.Create<Object>("(BC)96", 1, 0, false);
				o.MinutesUntilReady = 24000 - Game1.timeOfDay;
				f.objects.Add(this.targetLocation, o);
				return;
			}
			case 1:
				f.terrainFeatures.Remove(this.targetLocation);
				f.terrainFeatures.Remove(this.targetLocation + new Vector2(1f, 0f));
				f.terrainFeatures.Remove(this.targetLocation + new Vector2(1f, 1f));
				f.terrainFeatures.Remove(this.targetLocation + new Vector2(0f, 1f));
				f.resourceClumps.Add(new ResourceClump(622, 2, 2, this.targetLocation, null, null));
				return;
			case 2:
			{
				AnimalHouse indoors = (AnimalHouse)this.targetBuilding.GetIndoors();
				long idOfRemove = 0L;
				foreach (long a in indoors.animalsThatLiveHere)
				{
					if (!indoors.animals.ContainsKey(a))
					{
						idOfRemove = a;
						break;
					}
				}
				if (!Game1.getFarm().animals.Remove(idOfRemove))
				{
					return;
				}
				indoors.animalsThatLiveHere.Remove(idOfRemove);
				using (NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection.Enumerator enumerator2 = Game1.getFarm().animals.Pairs.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						KeyValuePair<long, FarmAnimal> a2 = enumerator2.Current;
						a2.Value.moodMessage.Value = 5;
					}
					return;
				}
				break;
			}
			case 3:
				break;
			default:
				return;
			}
			f.objects.Add(this.targetLocation, ItemRegistry.Create<Object>("(BC)95", 1, 0, false));
		}

		// Token: 0x04002272 RID: 8818
		public const int cropCircle = 0;

		// Token: 0x04002273 RID: 8819
		public const int meteorite = 1;

		// Token: 0x04002274 RID: 8820
		public const int dogs = 2;

		// Token: 0x04002275 RID: 8821
		public const int owl = 3;

		// Token: 0x04002276 RID: 8822
		public const int earthquake = 4;

		// Token: 0x04002277 RID: 8823
		public const int raccoonStump = 5;

		// Token: 0x04002278 RID: 8824
		private readonly NetInt behavior = new NetInt();

		// Token: 0x04002279 RID: 8825
		private float timer;

		// Token: 0x0400227A RID: 8826
		private float timeUntilText = 7000f;

		// Token: 0x0400227B RID: 8827
		private string soundName;

		// Token: 0x0400227C RID: 8828
		private string message;

		// Token: 0x0400227D RID: 8829
		private bool playedSound;

		// Token: 0x0400227E RID: 8830
		private bool showedMessage;

		// Token: 0x0400227F RID: 8831
		private bool finished;

		// Token: 0x04002280 RID: 8832
		private Vector2 targetLocation;

		// Token: 0x04002281 RID: 8833
		private Building targetBuilding;
	}
}

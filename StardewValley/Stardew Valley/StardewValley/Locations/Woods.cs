using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002F6 RID: 758
	public class Woods : GameLocation
	{
		// Token: 0x060032E4 RID: 13028 RVA: 0x00295C43 File Offset: 0x00293E43
		public Woods()
		{
		}

		// Token: 0x060032E5 RID: 13029 RVA: 0x00295C78 File Offset: 0x00293E78
		public Woods(string map, string name) : base(map, name)
		{
			this.isOutdoors.Value = true;
			this.ignoreDebrisWeather.Value = true;
			this.ignoreOutdoorLighting.Value = true;
		}

		// Token: 0x060032E6 RID: 13030 RVA: 0x00295CE0 File Offset: 0x00293EE0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.addedSlimesToday, "addedSlimesToday").AddField(this.statueAnimationEvent, "statueAnimationEvent").AddField(this.hasUnlockedStatue, "hasUnlockedStatue");
			this.statueAnimationEvent.onEvent += this.doStatueAnimation;
		}

		// Token: 0x060032E7 RID: 13031 RVA: 0x00295D44 File Offset: 0x00293F44
		public static void ResetLostItemsShop()
		{
			IInventory shopItems = Woods.GetLostItemsShopInventory();
			shopItems.Clear();
			Dictionary<string, int> itemsInSave = new Dictionary<string, int>();
			Utility.ForEachItem(delegate(Item item)
			{
				itemsInSave[item.QualifiedItemId] = itemsInSave.GetValueOrDefault(item.QualifiedItemId) + item.Stack;
				return true;
			});
			Dictionary<string, int> eventsSeen = new Dictionary<string, int>();
			Dictionary<string, int> mailFlags = new Dictionary<string, int>();
			foreach (Farmer player in Game1.getAllFarmers())
			{
				foreach (string eventSeen in player.eventsSeen)
				{
					eventsSeen[eventSeen] = eventsSeen.GetValueOrDefault(eventSeen) + 1;
				}
				foreach (string mailFlag in player.mailReceived)
				{
					mailFlags[mailFlag] = mailFlags.GetValueOrDefault(mailFlag) + 1;
				}
			}
			foreach (LostItem entry in DataLoader.LostItemsShop(Game1.content))
			{
				int unlocked;
				if (entry.RequireMailReceived != null)
				{
					unlocked = mailFlags.GetValueOrDefault(entry.RequireMailReceived);
				}
				else
				{
					if (entry.RequireEventSeen == null)
					{
						continue;
					}
					unlocked = eventsSeen.GetValueOrDefault(entry.RequireEventSeen);
				}
				int existInWorld = itemsInSave.GetValueOrDefault(entry.ItemId);
				int missing = unlocked - existInWorld;
				if (missing > 0)
				{
					for (int i = 0; i < missing; i++)
					{
						shopItems.Add(ItemRegistry.Create(entry.ItemId, 1, 0, false));
					}
				}
			}
		}

		// Token: 0x060032E8 RID: 13032 RVA: 0x00295F2C File Offset: 0x0029412C
		public bool localPlayerHasFoundStardrop()
		{
			return Game1.player.hasOrWillReceiveMail("CF_Statue");
		}

		// Token: 0x060032E9 RID: 13033 RVA: 0x00295F3D File Offset: 0x0029413D
		public void statueAnimation(Farmer who)
		{
			if (this.hasUnlockedStatue.Value)
			{
				return;
			}
			who.reduceActiveItemByOne();
			this.hasUnlockedStatue.Value = true;
			this.statueAnimationEvent.Fire();
		}

		// Token: 0x060032EA RID: 13034 RVA: 0x00295F6C File Offset: 0x0029416C
		private void doStatueAnimation()
		{
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(8f, 7f) * 64f, Color.White, 9, false, 50f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(9f, 7f) * 64f, Color.Orange, 9, false, 70f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(8f, 6f) * 64f, Color.White, 9, false, 60f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(9f, 6f) * 64f, Color.OrangeRed, 9, false, 120f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(8f, 5f) * 64f, Color.Red, 9, false, 100f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(9f, 5f) * 64f, Color.White, 9, false, 170f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(544f, 464f), Color.Orange, 9, false, 40f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(608f, 464f), Color.White, 9, false, 90f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(544f, 400f), Color.OrangeRed, 9, false, 190f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(608f, 400f), Color.White, 9, false, 80f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(544f, 336f), Color.Red, 9, false, 69f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(608f, 336f), Color.OrangeRed, 9, false, 130f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(480f, 464f), Color.Orange, 9, false, 40f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(672f, 368f), Color.White, 9, false, 90f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(480f, 464f), Color.Red, 9, false, 30f, 0, -1, -1f, -1, 0));
			this.temporarySprites.Add(new TemporaryAnimatedSprite(11, new Vector2(672f, 368f), Color.White, 9, false, 180f, 0, -1, -1f, -1, 0));
			base.localSound("secret1", null, null, SoundContext.Default);
			this.updateStatueEyes();
		}

		// Token: 0x060032EB RID: 13035 RVA: 0x0029634C File Offset: 0x0029454C
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (who.IsLocalPlayer)
			{
				int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet");
				if (tileIndexAt - 1140 <= 1)
				{
					if (!this.hasUnlockedStatue.Value)
					{
						Object activeObject = who.ActiveObject;
						if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)417")
						{
							this.statueTimer = 1000;
							who.freezePause = 1000;
							Game1.changeMusicTrack("none", false, MusicContext.Default);
							base.playSound("newArtifact", null, null, SoundContext.Default);
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Woods_Statue").Replace('\n', '^'));
						}
					}
					if (this.hasUnlockedStatue.Value && !this.localPlayerHasFoundStardrop() && who.freeSpotsInInventory() > 0)
					{
						who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)434", 1, 0, false), null, false);
						Game1.player.mailReceived.Add("CF_Statue");
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x060032EC RID: 13036 RVA: 0x00296460 File Offset: 0x00294660
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			Woods.GetLostItemShopMutex().ReleaseLock();
			this.characters.RemoveWhere((NPC npc) => npc is Monster);
			this.addedSlimesToday.Value = false;
		}

		// Token: 0x060032ED RID: 13037 RVA: 0x002964B5 File Offset: 0x002946B5
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			List<Vector2> list = this.baubles;
			if (list != null)
			{
				list.Clear();
			}
			List<WeatherDebris> list2 = this.weatherDebris;
			if (list2 == null)
			{
				return;
			}
			list2.Clear();
		}

		// Token: 0x060032EE RID: 13038 RVA: 0x002964E0 File Offset: 0x002946E0
		protected override void resetSharedState()
		{
			if (!this.addedSlimesToday.Value)
			{
				this.addedSlimesToday.Value = true;
				Random rand = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 12.0, 0.0, 0.0);
				for (int tries = 50; tries > 0; tries--)
				{
					Vector2 tile = base.getRandomTile(null);
					if (rand.NextDouble() < 0.25 && this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						switch (base.GetSeason())
						{
						case Season.Spring:
							this.characters.Add(new GreenSlime(tile * 64f, 0));
							break;
						case Season.Summer:
							this.characters.Add(new GreenSlime(tile * 64f, 0));
							break;
						case Season.Fall:
							this.characters.Add(new GreenSlime(tile * 64f, rand.Choose(0, 40)));
							break;
						case Season.Winter:
							this.characters.Add(new GreenSlime(tile * 64f, 40));
							break;
						}
					}
				}
			}
			base.resetSharedState();
		}

		// Token: 0x060032EF RID: 13039 RVA: 0x0029662C File Offset: 0x0029482C
		protected void _updateWoodsLighting()
		{
			if (Game1.currentLocation != this)
			{
				return;
			}
			int fade_start_time = Utility.ConvertTimeToMinutes(Game1.getStartingToGetDarkTime(this));
			int fade_end_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime(this));
			int light_fade_start_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime(this));
			int light_fade_end_time = Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime(this));
			float num = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameMinute;
			float lerp = Utility.Clamp((num - (float)fade_start_time) / (float)(fade_end_time - fade_start_time), 0f, 1f);
			float light_lerp = Utility.Clamp((num - (float)light_fade_start_time) / (float)(light_fade_end_time - light_fade_start_time), 0f, 1f);
			Game1.ambientLight.R = (byte)Utility.Lerp((float)this._ambientLightColor.R, (float)Math.Max(this._ambientLightColor.R, Game1.isRaining ? Game1.ambientLight.R : Game1.outdoorLight.R), lerp);
			Game1.ambientLight.G = (byte)Utility.Lerp((float)this._ambientLightColor.G, (float)Math.Max(this._ambientLightColor.G, Game1.isRaining ? Game1.ambientLight.G : Game1.outdoorLight.G), lerp);
			Game1.ambientLight.B = (byte)Utility.Lerp((float)this._ambientLightColor.B, (float)Math.Max(this._ambientLightColor.B, Game1.isRaining ? Game1.ambientLight.B : Game1.outdoorLight.B), lerp);
			Game1.ambientLight.A = (byte)Utility.Lerp((float)this._ambientLightColor.A, (float)Math.Max(this._ambientLightColor.A, Game1.isRaining ? Game1.ambientLight.A : Game1.outdoorLight.A), lerp);
			Color light_color = Color.Black;
			light_color.A = (byte)Utility.Lerp(255f, 0f, light_lerp);
			foreach (LightSource light in Game1.currentLightSources.Values)
			{
				if (light.lightContext.Value == LightSource.LightContext.MapLight)
				{
					light.color.Value = light_color;
				}
			}
		}

		// Token: 0x060032F0 RID: 13040 RVA: 0x00296870 File Offset: 0x00294A70
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			this.UpdateLostItemsShopTile();
			this.updateStatueEyes();
		}

		// Token: 0x060032F1 RID: 13041 RVA: 0x00296888 File Offset: 0x00294A88
		protected override void resetLocalState()
		{
			this._ambientLightColor = new Color(150, 120, 50);
			this.ignoreOutdoorLighting.Value = false;
			Game1.player.mailReceived.Add("beenToWoods");
			base.resetLocalState();
			this._updateWoodsLighting();
			Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			int numberOfBaubles = 25 + r.Next(0, 75);
			if (!base.IsRainingHere())
			{
				this.baubles = new List<Vector2>();
				for (int i = 0; i < numberOfBaubles; i++)
				{
					this.baubles.Add(new Vector2((float)Game1.random.Next(0, this.map.DisplayWidth), (float)Game1.random.Next(0, this.map.DisplayHeight)));
				}
				Season season = base.GetSeason();
				if (season != Season.Winter)
				{
					this.weatherDebris = new List<WeatherDebris>();
					int spacing = 192;
					int leafType = 1;
					if (season == Season.Fall)
					{
						leafType = 2;
					}
					for (int j = 0; j < numberOfBaubles; j++)
					{
						this.weatherDebris.Add(new WeatherDebris(new Vector2((float)(j * spacing % Game1.graphics.GraphicsDevice.Viewport.Width + Game1.random.Next(spacing)), (float)(j * spacing / Game1.graphics.GraphicsDevice.Viewport.Width * spacing % Game1.graphics.GraphicsDevice.Viewport.Height + Game1.random.Next(spacing))), leafType, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
					}
				}
			}
			if (Game1.timeOfDay >= 1200)
			{
				Random asdfTime = Utility.CreateDaySaveRandom(15.0, 0.0, 0.0);
				int time = Utility.ModifyTime(1920, asdfTime.Next(390));
				int delayBeforeStart = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, time) * Game1.realMilliSecondsPerGameMinute;
				if (delayBeforeStart > 0)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(0f, 0f), false, 0f, Color.White)
					{
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = (float)delayBeforeStart,
						endFunction = delegate(int x)
						{
							bool passed = true;
							foreach (Farmer f in this.farmers)
							{
								if (f.position.X < 640f || f.position.Y > 1280f)
								{
									passed = false;
								}
							}
							if (passed)
							{
								foreach (LightSource k in this.sharedLights.Values)
								{
									if (k.position.X < 1600f && k.position.Y > 1184f)
									{
										passed = false;
										break;
									}
								}
								if (passed)
								{
									this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(22f, 24.3f) * 64f, true, 0f, Color.White)
									{
										animationLength = 8,
										totalNumberOfLoops = 88,
										interval = 90f,
										motion = new Vector2(-7f, 0f),
										scale = 5.5f,
										layerDepth = 0.176f
									});
								}
							}
						}
					});
				}
			}
		}

		// Token: 0x060032F2 RID: 13042 RVA: 0x00296B30 File Offset: 0x00294D30
		private void UpdateLostItemsShopTile()
		{
			IInventory lostItemsShopInventory = Woods.GetLostItemsShopInventory();
			lostItemsShopInventory.RemoveWhere((Item item) => item == null || item.Stack <= 0);
			if (lostItemsShopInventory.HasAny())
			{
				if (base.Map.GetTileSheet("lostItemsShop") == null)
				{
					Texture2D texture = Game1.content.Load<Texture2D>("Characters\\Crow");
					this.map.AddTileSheet(new TileSheet("lostItemsShop", this.map, "Characters\\Crow", new Size(texture.Width / 16, texture.Height / 16), new Size(16)));
				}
				base.setAnimatedMapTile(12, 4, Enumerable.Range(0, 32).ToArray<int>(), 100L, "Front", "lostItemsShop", null, true);
				base.setAnimatedMapTile(12, 5, Enumerable.Range(32, 32).ToArray<int>(), 100L, "Buildings", "lostItemsShop", "LostItemsShop", true);
				for (int i = 0; i < 3; i++)
				{
					base.setTileProperty(11 + i, 6, "Buildings", "Action", "LostItemsShop");
				}
				base.setMapTile(10, 4, 0, "Buildings", "untitled tile sheet", null, true);
				base.setMapTile(14, 5, 0, "Buildings", "untitled tile sheet", null, true);
				return;
			}
			base.removeMapTile(12, 4, "Front");
			base.removeMapTile(12, 5, "Buildings");
			for (int j = 0; j < 3; j++)
			{
				base.removeTileProperty(11, 6 + j, "Buildings", "Action");
			}
			base.removeMapTile(10, 4, "Buildings");
			base.removeMapTile(14, 5, "Buildings");
		}

		// Token: 0x060032F3 RID: 13043 RVA: 0x00296CD4 File Offset: 0x00294ED4
		private void updateStatueEyes()
		{
			Layer frontLayer = this.map.RequireLayer("Front");
			if (this.hasUnlockedStatue.Value && !this.localPlayerHasFoundStardrop())
			{
				frontLayer.Tiles[8, 6].TileIndex = 1117;
				frontLayer.Tiles[9, 6].TileIndex = 1118;
				return;
			}
			frontLayer.Tiles[8, 6].TileIndex = 1115;
			frontLayer.Tiles[9, 6].TileIndex = 1116;
		}

		// Token: 0x060032F4 RID: 13044 RVA: 0x00296D66 File Offset: 0x00294F66
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			this.statueAnimationEvent.Poll();
		}

		// Token: 0x060032F5 RID: 13045 RVA: 0x00296D7C File Offset: 0x00294F7C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this._updateWoodsLighting();
			if (this.statueTimer > 0)
			{
				this.statueTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.statueTimer <= 0)
				{
					this.statueAnimation(Game1.player);
				}
			}
			if (this.baubles != null)
			{
				for (int i = 0; i < this.baubles.Count; i++)
				{
					Vector2 v = default(Vector2);
					v.X = this.baubles[i].X - Math.Max(0.4f, Math.Min(1f, (float)i * 0.01f)) - (float)((double)((float)i * 0.01f) * Math.Sin(6.283185307179586 * (double)time.TotalGameTime.Milliseconds / 8000.0));
					v.Y = this.baubles[i].Y + Math.Max(0.5f, Math.Min(1.2f, (float)i * 0.02f));
					if (v.Y > (float)this.map.DisplayHeight || v.X < 0f)
					{
						v.X = (float)Game1.random.Next(0, this.map.DisplayWidth);
						v.Y = -64f;
					}
					this.baubles[i] = v;
				}
			}
			if (this.weatherDebris != null)
			{
				foreach (WeatherDebris weatherDebris in this.weatherDebris)
				{
					weatherDebris.update();
				}
				Game1.updateDebrisWeatherForMovement(this.weatherDebris);
			}
		}

		// Token: 0x060032F6 RID: 13046 RVA: 0x00296F4C File Offset: 0x0029514C
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (this.baubles != null)
			{
				for (int i = 0; i < this.baubles.Count; i++)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.baubles[i]), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(346 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(i * 25)) % 600.0) / 150 * 5, 1971, 5, 5)), Color.White, (float)i * 0.3926991f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			if (this.weatherDebris != null && this.currentEvent == null)
			{
				foreach (WeatherDebris weatherDebris in this.weatherDebris)
				{
					weatherDebris.draw(b);
				}
			}
		}

		// Token: 0x060032F7 RID: 13047 RVA: 0x0029705C File Offset: 0x0029525C
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "LostItemsShop")
			{
				Woods.GetLostItemShopMutex().RequestLock(delegate
				{
					if (Utility.TryOpenShopMenu("LostItems", null, true))
					{
						ShopMenu shopMenu = Game1.activeClickableMenu as ShopMenu;
						if (shopMenu != null)
						{
							shopMenu.behaviorBeforeCleanup = new Action<IClickableMenu>(this.OnLostItemsShopClosed);
						}
					}
				}, null);
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x060032F8 RID: 13048 RVA: 0x00297095 File Offset: 0x00295295
		public static IInventory GetLostItemsShopInventory()
		{
			return Game1.player.team.GetOrCreateGlobalInventory("LostItemsShop");
		}

		// Token: 0x060032F9 RID: 13049 RVA: 0x002970AB File Offset: 0x002952AB
		public static NetMutex GetLostItemShopMutex()
		{
			return Game1.player.team.GetOrCreateGlobalInventoryMutex("LostItemsShop");
		}

		// Token: 0x060032FA RID: 13050 RVA: 0x002970C1 File Offset: 0x002952C1
		private void OnLostItemsShopClosed(IClickableMenu shopMenu)
		{
			Woods.GetLostItemShopMutex().ReleaseLock();
		}

		// Token: 0x040021F1 RID: 8689
		public const int numBaubles = 25;

		// Token: 0x040021F2 RID: 8690
		private List<Vector2> baubles;

		// Token: 0x040021F3 RID: 8691
		private List<WeatherDebris> weatherDebris;

		// Token: 0x040021F4 RID: 8692
		[XmlElement("hasUnlockedStatue")]
		public readonly NetBool hasUnlockedStatue = new NetBool();

		// Token: 0x040021F5 RID: 8693
		[XmlElement("addedSlimesToday")]
		private readonly NetBool addedSlimesToday = new NetBool();

		// Token: 0x040021F6 RID: 8694
		[XmlIgnore]
		private readonly NetEvent0 statueAnimationEvent = new NetEvent0(false);

		// Token: 0x040021F7 RID: 8695
		protected Color _ambientLightColor = Color.White;

		// Token: 0x040021F8 RID: 8696
		private int statueTimer;
	}
}

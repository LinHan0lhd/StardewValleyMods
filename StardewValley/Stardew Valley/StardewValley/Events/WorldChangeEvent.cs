using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Events
{
	// Token: 0x0200032D RID: 813
	public class WorldChangeEvent : BaseFarmEvent
	{
		// Token: 0x060034C6 RID: 13510 RVA: 0x002A2BEC File Offset: 0x002A0DEC
		public WorldChangeEvent() : this(0)
		{
		}

		// Token: 0x060034C7 RID: 13511 RVA: 0x002A2BF5 File Offset: 0x002A0DF5
		public WorldChangeEvent(int which)
		{
			this.whichEvent.Value = which;
		}

		// Token: 0x060034C8 RID: 13512 RVA: 0x002A2C1F File Offset: 0x002A0E1F
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.whichEvent, "whichEvent");
		}

		// Token: 0x060034C9 RID: 13513 RVA: 0x002A2C40 File Offset: 0x002A0E40
		private void obliterateJojaMartDoor()
		{
			Town town = Game1.RequireLocation<Town>("Town", false);
			town.crackOpenAbandonedJojaMartDoor();
			for (int i = 0; i < 16; i++)
			{
				town.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(96f, 50f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), 0f), false, 0.002f, Color.Gray)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -0.5f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
					interval = 99999f,
					layerDepth = 0.95f + (float)i * 0.001f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
					delayBeforeAnimationStart = i * 25
				});
			}
			Utility.addDirtPuffs(town, 95, 49, 2, 2, 5);
			town.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(96f, 50f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), 0f), false, 0f, Color.Gray)
			{
				alpha = 0.01f,
				interval = 99999f,
				layerDepth = 0.9f,
				lightId = this.GenerateLightSourceId() + "_obliterateJojaMartDoor",
				lightRadius = 4f,
				lightcolor = new Color(1, 1, 1)
			});
		}

		// Token: 0x060034CA RID: 13514 RVA: 0x002A2E58 File Offset: 0x002A1058
		public override bool setUp()
		{
			this.preEventLocation = Game1.currentLocation;
			this.location = null;
			Point targetTile = Point.Zero;
			this.wasRaining = Game1.isRaining;
			switch (this.whichEvent.Value)
			{
			case 0:
			case 1:
				this.location = Game1.getFarm();
				targetTile = ((Game1.whichFarm == 5) ? new Point(39, 32) : new Point(28, 13));
				using (List<Building>.Enumerator enumerator = this.location.buildings.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Building b = enumerator.Current;
						if (b is GreenhouseBuilding)
						{
							targetTile = new Point(b.tileX.Value + 3, b.tileY.Value + 3);
							break;
						}
					}
					goto IL_23A;
				}
				break;
			case 2:
			case 3:
				this.location = Game1.RequireLocation("Town", false);
				targetTile = new Point(105, 79);
				goto IL_23A;
			case 4:
			case 5:
				this.location = Game1.RequireLocation("Mountain", false);
				targetTile = new Point(95, 27);
				goto IL_23A;
			case 6:
			case 7:
				break;
			case 8:
			case 9:
				this.location = Game1.RequireLocation("Mountain", false);
				targetTile = new Point(48, 5);
				goto IL_23A;
			case 10:
				this.location = Game1.RequireLocation("Town", false);
				targetTile = new Point(52, 18);
				goto IL_23A;
			case 11:
				this.location = Game1.RequireLocation("Town", false);
				targetTile = new Point(95, 48);
				goto IL_23A;
			case 12:
				this.location = Game1.RequireLocation("Town", false);
				targetTile = new Point(95, 48);
				goto IL_23A;
			case 13:
				this.location = Game1.RequireLocation("BoatTunnel", false);
				targetTile = new Point(7, 7);
				goto IL_23A;
			case 14:
				this.location = Game1.RequireLocation("Mountain", false);
				targetTile = new Point(16, 7);
				goto IL_23A;
			case 15:
				this.location = Game1.RequireLocation("IslandNorth", false);
				targetTile = new Point(40, 23);
				goto IL_23A;
			default:
				goto IL_23A;
			}
			this.location = Game1.RequireLocation("BusStop", false);
			targetTile = new Point(24, 8);
			IL_23A:
			Game1.currentLocation = this.location;
			this.resetForPlayerEntry(targetTile);
			return false;
		}

		// Token: 0x060034CB RID: 13515 RVA: 0x002A30C4 File Offset: 0x002A12C4
		public void resetForPlayerEntry(Point targetTile)
		{
			this.location.resetForPlayerEntry();
			this.cutsceneLengthTimer = 8000;
			this.wasRaining = Game1.isRaining;
			Game1.isRaining = false;
			Game1.changeMusicTrack("nightTime", false, MusicContext.Default);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("WorldChangeEvent");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.whichEvent.Value);
			string lightSourceId = defaultInterpolatedStringHandler.ToStringAndClear();
			switch (this.whichEvent.Value)
			{
			case 0:
			{
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2((float)((targetTile.X - 3) * 64 + 8), (float)((targetTile.Y - 1) * 64 - 32)), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2((float)((targetTile.X + 3) * 64 - 16), (float)((targetTile.Y - 2) * 64)), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2((float)(targetTile.X * 64 + 8), (float)((targetTile.Y - 4) * 64)), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.soundInterval = 560;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L, null));
				this.sound = "axchop";
				float depthY = (float)((targetTile.Y + 3) * 64) / 10000f;
				TemporaryAnimatedSprite hole = new TemporaryAnimatedSprite("Buildings\\Greenhouse", new Rectangle(25, 133, 31, 19), 99999f, 1, 999, new Vector2((float)(targetTile.X * 64), (float)((targetTile.Y - 1) * 64 - 64)) + new Vector2(-23f, 53f) * 4f, false, false)
				{
					scale = 4f,
					layerDepth = depthY + 0.0008f
				};
				this.location.temporarySprites.Add(hole);
				TemporaryAnimatedSprite raccoon = new TemporaryAnimatedSprite("Characters\\raccoon", new Rectangle(0, 32, 32, 32), 99999f, 1, 999, new Vector2((float)(targetTile.X * 64), (float)((targetTile.Y - 1) * 64 - 64)) + new Vector2(-20f, 40f) * 4f, false, false)
				{
					scale = 4f,
					shakeIntensity = 1f,
					layerDepth = depthY + 0.0004f,
					delayBeforeAnimationStart = 3000,
					motion = new Vector2(-1f, -6f),
					acceleration = new Vector2(0f, 0.17f),
					xStopCoordinate = targetTile.X * 64 - 136,
					startSound = "Raccoon"
				};
				TemporaryAnimatedSprite raccoon2;
				TemporaryAnimatedSprite.endBehavior <>9__3;
				TemporaryAnimatedSprite.endBehavior <>9__2;
				raccoon.reachedStopCoordinate = delegate(int x)
				{
					hole.layerDepth = 0f;
					TemporaryAnimatedSprite raccoon;
					raccoon.motion.X = -1f;
					raccoon.yStopCoordinate = targetTile.Y * 64 + 72;
					raccoon = raccoon;
					TemporaryAnimatedSprite.endBehavior reachedStopCoordinate;
					if ((reachedStopCoordinate = <>9__2) == null)
					{
						reachedStopCoordinate = (<>9__2 = delegate(int y)
						{
							raccoon.motion = new Vector2(0f, 4f);
							raccoon.acceleration = Vector2.Zero;
							raccoon.sourceRect = new Rectangle(0, 0, 32, 32);
							raccoon.animationLength = 8;
							raccoon.interval = 80f;
							raccoon.sourceRectStartingPos = Vector2.Zero;
							raccoon.yStopCoordinate = targetTile.Y * 64 + 160;
							TemporaryAnimatedSprite raccoon2 = raccoon;
							TemporaryAnimatedSprite.endBehavior reachedStopCoordinate2;
							if ((reachedStopCoordinate2 = <>9__3) == null)
							{
								reachedStopCoordinate2 = (<>9__3 = delegate(int z)
								{
									raccoon.layerDepth = -1f;
									raccoon.motion = new Vector2(0f, 4f);
									raccoon.layerDepthOffset = 0.0128f;
								});
							}
							raccoon2.reachedStopCoordinate = reachedStopCoordinate2;
						});
					}
					raccoon.reachedStopCoordinate = reachedStopCoordinate;
				};
				this.location.temporarySprites.Add(raccoon);
				break;
			}
			case 1:
			{
				Utility.addSprinklesToLocation(this.location, targetTile.X, targetTile.Y - 1, 7, 7, 15000, 150, Color.LightCyan, null, false);
				Utility.addStarsAndSpirals(this.location, targetTile.X, targetTile.Y - 1, 7, 7, 15000, 150, Color.White, null, false);
				Game1.player.activeDialogueEvents.TryAdd("cc_Greenhouse", 3);
				this.sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2((float)(targetTile.X * 64), (float)((targetTile.Y - 1) * 64 - 64)), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					lightId = lightSourceId,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.soundInterval = 800;
				float depthY2 = (float)((targetTile.Y + 3) * 64) / 10000f;
				TemporaryAnimatedSprite hole2 = new TemporaryAnimatedSprite("Buildings\\Greenhouse", new Rectangle(25, 133, 31, 19), 99999f, 1, 999, new Vector2((float)(targetTile.X * 64), (float)((targetTile.Y - 1) * 64 - 64)) + new Vector2(-23f, 53f) * 4f, false, false)
				{
					scale = 4f,
					layerDepth = depthY2 + 0.0008f
				};
				this.location.temporarySprites.Add(hole2);
				TemporaryAnimatedSprite raccoon2 = new TemporaryAnimatedSprite("Characters\\raccoon", new Rectangle(0, 32, 32, 32), 99999f, 1, 999, new Vector2((float)(targetTile.X * 64), (float)((targetTile.Y - 1) * 64 - 64)) + new Vector2(-20f, 40f) * 4f, false, false)
				{
					scale = 4f,
					shakeIntensity = 1f,
					layerDepth = depthY2 + 0.0004f,
					delayBeforeAnimationStart = 3000,
					motion = new Vector2(-1f, -6f),
					acceleration = new Vector2(0f, 0.17f),
					xStopCoordinate = targetTile.X * 64 - 136,
					startSound = "Raccoon"
				};
				TemporaryAnimatedSprite raccoon;
				TemporaryAnimatedSprite.endBehavior <>9__5;
				TemporaryAnimatedSprite.endBehavior <>9__4;
				raccoon2.reachedStopCoordinate = delegate(int x)
				{
					hole2.layerDepth = 0f;
					raccoon2.motion.X = -1f;
					raccoon2.yStopCoordinate = targetTile.Y * 64 + 72;
					TemporaryAnimatedSprite raccoon = raccoon2;
					TemporaryAnimatedSprite.endBehavior reachedStopCoordinate;
					if ((reachedStopCoordinate = <>9__4) == null)
					{
						reachedStopCoordinate = (<>9__4 = delegate(int y)
						{
							TemporaryAnimatedSprite raccoon2;
							raccoon2.motion = new Vector2(0f, 4f);
							raccoon2.acceleration = Vector2.Zero;
							raccoon2.sourceRect = new Rectangle(0, 0, 32, 32);
							raccoon2.animationLength = 8;
							raccoon2.interval = 80f;
							raccoon2.sourceRectStartingPos = Vector2.Zero;
							raccoon2.yStopCoordinate = targetTile.Y * 64 + 160;
							raccoon2 = raccoon2;
							TemporaryAnimatedSprite.endBehavior reachedStopCoordinate2;
							if ((reachedStopCoordinate2 = <>9__5) == null)
							{
								reachedStopCoordinate2 = (<>9__5 = delegate(int z)
								{
									raccoon2.layerDepth = -1f;
									raccoon2.motion = new Vector2(0f, 4f);
									raccoon2.layerDepthOffset = 0.0128f;
								});
							}
							raccoon2.reachedStopCoordinate = reachedStopCoordinate2;
						});
					}
					raccoon.reachedStopCoordinate = reachedStopCoordinate;
				};
				this.location.temporarySprites.Add(raccoon2);
				break;
			}
			case 2:
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 100f, 5, 999, new Vector2(6656f, 5024f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1406, 22, 26), 700f, 2, 999, new Vector2(6888f, 5014f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(6792f, 4864f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(6912f, 5136f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.TryAdd("cc_Minecart", 7);
				this.soundInterval = 500;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L, null));
				this.sound = "clank";
				break;
			case 3:
				Utility.addSprinklesToLocation(this.location, targetTile.X + 1, targetTile.Y, 6, 4, 15000, 350, Color.LightCyan, null, false);
				Utility.addStarsAndSpirals(this.location, targetTile.X + 1, targetTile.Y, 6, 4, 15000, 350, Color.White, null, false);
				Game1.player.activeDialogueEvents.TryAdd("cc_Minecart", 7);
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6656f, 5056f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					lightId = lightSourceId + "_1",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6912f, 5056f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.HotPink,
					lightId = lightSourceId + "_2",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.soundInterval = 800;
				break;
			case 4:
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(383, 1378, 28, 27), 400f, 2, 999, new Vector2(5504f, 1632f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					motion = new Vector2(0.5f, 0f)
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1406, 22, 26), 350f, 2, 999, new Vector2(6272f, 1632f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(358, 1415, 31, 20), 999f, 1, 9999, new Vector2(5888f, 1648f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(6400f, 1648f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(5824f, 1584f), false, false)
				{
					scale = 4f,
					layerDepth = 0.8f
				});
				Game1.player.activeDialogueEvents.TryAdd("cc_Bridge", 7);
				this.soundInterval = 700;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L, null));
				this.sound = "axchop";
				break;
			case 5:
				Utility.addSprinklesToLocation(this.location, targetTile.X, targetTile.Y, 7, 4, 15000, 150, Color.LightCyan, null, false);
				Utility.addStarsAndSpirals(this.location, targetTile.X + 1, targetTile.Y, 7, 4, 15000, 350, Color.White, null, false);
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(5824f, 1648f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					lightId = lightSourceId + "_1",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6336f, 1648f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.Yellow,
					lightId = lightSourceId + "_2",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				Game1.player.activeDialogueEvents.TryAdd("cc_Bridge", 7);
				this.sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.soundInterval = 800;
				break;
			case 6:
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2(1856f, 480f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2(1280f, 512f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(1544f, 192f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.TryAdd("cc_Bus", 7);
				this.soundInterval = 560;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L, null));
				this.sound = "clank";
				break;
			case 7:
				Utility.addSprinklesToLocation(this.location, targetTile.X, targetTile.Y, 9, 4, 10000, 200, Color.LightCyan, null, true);
				Utility.addStarsAndSpirals(this.location, targetTile.X, targetTile.Y, 9, 4, 15000, 150, Color.White, null, false);
				this.sound = "junimoMeep1";
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1280f, 640f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					lightId = lightSourceId + "_1",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1408f, 640f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.Pink,
					lightId = lightSourceId + "_2",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1536f, 640f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2200f,
					xPeriodicRange = 16f,
					color = Color.Yellow,
					lightId = lightSourceId + "_3",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1664f, 640f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2100f,
					xPeriodicRange = 16f,
					color = Color.LightBlue,
					lightId = lightSourceId + "_4",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				Game1.player.activeDialogueEvents.TryAdd("cc_Bus", 7);
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.soundInterval = 500;
				break;
			case 8:
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 100f, 5, 999, new Vector2(2880f, 288f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 1340, 17, 37), 50f, 2, 99999, new Vector2(3040f, 160f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicLoopTime = 100f,
					yPeriodicRange = 2f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(2816f, 368f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(3200f, 368f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.TryAdd("cc_Boulder", 7);
				this.soundInterval = 100;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L, null));
				this.sound = "thudStep";
				break;
			case 9:
				Game1.player.activeDialogueEvents.TryAdd("cc_Boulder", 7);
				Utility.addSprinklesToLocation(this.location, targetTile.X, targetTile.Y, 4, 4, 15000, 350, Color.LightCyan, null, false);
				Utility.addStarsAndSpirals(this.location, targetTile.X + 1, targetTile.Y, 4, 4, 15000, 550, Color.White, null, false);
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(2880f, 368f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					lightId = lightSourceId + "_1",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(3200f, 368f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2300f,
					xPeriodicRange = 16f,
					color = Color.Yellow,
					lightId = lightSourceId + "_2",
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(lightSourceId + "_1", 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 1f, LightSource.LightContext.None, 0L, null));
				Game1.currentLightSources.Add(new LightSource(lightSourceId + "_2", 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 1f, Color.DarkCyan, LightSource.LightContext.None, 0L, null));
				Game1.currentLightSources.Add(new LightSource(lightSourceId + "_3", 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.soundInterval = 1000;
				break;
			case 10:
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2(3760f, 1056f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2(2948f, 1088f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2(3144f, 1280f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				Game1.player.activeDialogueEvents.TryAdd("movieTheater", 3);
				this.soundInterval = 560;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L, null));
				this.sound = "axchop";
				break;
			case 11:
				Utility.addSprinklesToLocation(this.location, targetTile.X, targetTile.Y, 7, 7, 15000, 150, Color.LightCyan, null, false);
				Utility.addStarsAndSpirals(this.location, targetTile.X, targetTile.Y, 7, 7, 15000, 150, Color.White, null, false);
				Game1.player.activeDialogueEvents.TryAdd("movieTheater", 3);
				this.sound = "junimoMeep1";
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6080f, 2880f), false, false)
				{
					scale = 4f,
					layerDepth = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 16f,
					lightId = lightSourceId,
					lightcolor = Color.DarkGoldenrod,
					lightRadius = 1f
				});
				this.soundInterval = 800;
				break;
			case 12:
			{
				this.cutsceneLengthTimer += 3000;
				Game1.isRaining = true;
				Game1.changeMusicTrack("rain", false, MusicContext.Default);
				if (Game1.IsMasterGame)
				{
					Game1.addMailForTomorrow("abandonedJojaMartAccessible", true, false);
				}
				Rectangle lightningSourceRect = new Rectangle(644, 1078, 37, 57);
				Vector2 strikePosition = new Vector2(96f, 50f) * 64f;
				Vector2 drawPosition = strikePosition + new Vector2((float)(-(float)lightningSourceRect.Width * 4 / 2), (float)(-(float)lightningSourceRect.Height * 4));
				while (drawPosition.Y > (float)(-(float)lightningSourceRect.Height * 4))
				{
					this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", lightningSourceRect, 9999f, 1, 999, drawPosition, false, Game1.random.NextBool(), (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						lightId = lightSourceId,
						lightRadius = 2f,
						delayBeforeAnimationStart = 6200,
						lightcolor = Color.Black
					});
					drawPosition.Y -= (float)(lightningSourceRect.Height * 4);
				}
				DelayedAction.playSoundAfterDelay("thunder_small", 6000, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("boulderBreak", 6300, null, null, -1, false);
				DelayedAction.screenFlashAfterDelay(1f, 6000, null);
				DelayedAction.functionAfterDelay(new Action(this.obliterateJojaMartDoor), 6050);
				break;
			}
			case 13:
				if (Game1.IsMasterGame)
				{
					Game1.addMailForTomorrow("willyBoatFixed", true, false);
				}
				Game1.mailbox.Add("willyHours");
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Willy", new Rectangle(0, 320, 16, 32), 120f, 3, 999, new Vector2(412f, 332f), false, false)
				{
					pingPong = true,
					scale = 4f,
					layerDepth = 1f
				});
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Robin", new Rectangle(0, 192, 16, 32), 140f, 4, 999, new Vector2(704f, 256f), false, false)
				{
					scale = 4f,
					layerDepth = 1f
				});
				this.soundInterval = 560;
				this.sound = "crafting";
				break;
			case 14:
				this.cutsceneLengthTimer = 12000;
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2((float)targetTile.X, (float)targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L, null));
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(0, 0, 24, 24), new Vector2(14f, 4.5f) * 64f, false, 0f, Color.White)
				{
					id = 777,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 1f,
					drawAboveAlwaysFront = true
				});
				DelayedAction.functionAfterDelay(new Action(this.ParrotSquawk), 2000);
				for (int i = 0; i < 16; i++)
				{
					Rectangle rect = new Rectangle(15, 5, 3, 3);
					TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(rect, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
					{
						motion = new Vector2((float)Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = (float)(1000 + Game1.random.Next(500)),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (rect.Bottom + 1) * 64,
						delayBeforeAnimationStart = 4000 + i * 250
					};
					t.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(t.bounce);
					this.location.TemporarySprites.Add(t);
					t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(rect, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
					{
						motion = new Vector2((float)Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = (float)(1000 + Game1.random.Next(500)),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = 4500 + i * 250,
						yStopCoordinate = (rect.Bottom + 1) * 64
					};
					t.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(t.bounce);
					this.location.TemporarySprites.Add(t);
				}
				for (int j = 0; j < 20; j++)
				{
					Vector2 start_point = new Vector2(Utility.RandomFloat(13f, 19f, null), 0f) * 64f;
					float x_offset = 1024f - start_point.X;
					TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48 + Game1.random.Next(2) * 72, Game1.random.Next(2) * 48, 24, 24), start_point, false, 0f, Color.White)
					{
						motion = new Vector2(x_offset * 0.01f, 10f),
						acceleration = new Vector2(0f, -0.05f),
						id = 778,
						scale = 4f,
						yStopCoordinate = 448,
						totalNumberOfLoops = 99999,
						interval = 50f,
						animationLength = 3,
						flipped = (x_offset > 0f),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = 3500 + j * 250,
						alpha = 0f,
						alphaFade = -0.1f
					};
					DelayedAction.playSoundAfterDelay("batFlap", 3500 + j * 250, null, null, -1, false);
					parrot.reachedStopCoordinateSprite = new Action<TemporaryAnimatedSprite>(this.ParrotBounce);
					this.location.temporarySprites.Add(parrot);
				}
				DelayedAction.functionAfterDelay(new Action(this.FinishTreehouse), 8000);
				DelayedAction.functionAfterDelay(new Action(this.ParrotSquawk), 9000);
				DelayedAction.functionAfterDelay(new Action(this.ParrotFlyAway), 11000);
				break;
			case 15:
			{
				Game1.changeMusicTrack("jungle_ambience", false, MusicContext.Default);
				this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(200, 89, 28, 32), new Vector2(39f, 32f) * 64f, false, 0f, Color.White)
				{
					animationLength = 2,
					interval = 700f,
					totalNumberOfLoops = 999,
					layerDepth = 0.1f,
					lightId = lightSourceId + "_1",
					lightcolor = Color.Black,
					lightRadius = 2f,
					scale = 4f
				});
				int walnutsBought = 130 - Game1.netWorldState.Value.GoldenWalnutsFound;
				int bags = 1 + walnutsBought / 10;
				for (int k = 0; k < bags; k++)
				{
					this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(184, 104, 14, 15), new Vector2((float)(39 + k % 3), 34.1f + (float)(k / 3) * 0.5f) * 64f, false, 0f, Color.White)
					{
						animationLength = 1,
						interval = 700f,
						totalNumberOfLoops = 999,
						layerDepth = 0.1f + (float)k * 0.01f,
						scale = 4f
					});
				}
				this.cutsceneLengthTimer = 10000;
				for (int l = 0; l < 20; l++)
				{
					Vector2 start_point2 = Utility.getRandomPositionInThisRectangle(new Rectangle(20, 1, 40, 2), Game1.random) * 64f;
					float xMotion = (float)((start_point2.X > (float)(this.location.Map.DisplayWidth / 2)) ? -1 : 1);
					TemporaryAnimatedSprite parrot2 = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48 + Game1.random.Next(2) * 72, 96, 24, 24), start_point2, false, 0f, Color.White)
					{
						motion = new Vector2(xMotion * 3f, 6f + (float)Game1.random.NextDouble()),
						acceleration = new Vector2(0f, -0.01f),
						id = 778,
						scale = 4f,
						yStopCoordinate = (int)start_point2.Y + Game1.random.Next(19, 27) * 64 + l * 64 / 2,
						totalNumberOfLoops = 99999,
						interval = 80f,
						animationLength = 3,
						pingPong = true,
						flipped = (xMotion > 0f),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						lightId = lightSourceId + "_2",
						lightcolor = Color.Black,
						lightRadius = 2f,
						alpha = 0.001f,
						alphaFade = -0.01f,
						delayBeforeAnimationStart = l * 250
					};
					DelayedAction.playSoundAfterDelay("parrot_flap", 500 + l * 250, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("parrot_flap", 5500 + l * 250, null, null, -1, false);
					parrot2.reachedStopCoordinateSprite = new Action<TemporaryAnimatedSprite>(this.GoldenParrotBounce);
					this.location.temporarySprites.Add(parrot2);
				}
				DelayedAction.functionAfterDelay(new Action(this.ParrotSquawk), 9000);
				DelayedAction.functionAfterDelay(new Action(this.ParrotFlyAway), 11000);
				break;
			}
			}
			this.soundTimer = this.soundInterval;
			Game1.fadeClear();
			Game1.nonWarpFade = true;
			Game1.timeOfDay = 2400;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.player.position.X = -999999f;
			Game1.viewport.X = Math.Max(0, Math.Min(this.location.map.DisplayWidth - Game1.viewport.Width, targetTile.X * 64 - Game1.viewport.Width / 2));
			Game1.viewport.Y = Math.Max(0, Math.Min(this.location.map.DisplayHeight - Game1.viewport.Height, targetTile.Y * 64 - Game1.viewport.Height / 2));
			if (!this.location.IsOutdoors)
			{
				Game1.viewport.X = targetTile.X * 64 - Game1.viewport.Width / 2;
				Game1.viewport.Y = targetTile.Y * 64 - Game1.viewport.Height / 2;
			}
			Game1.previousViewportPosition = new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
			List<WeatherDebris> debrisWeather = Game1.debrisWeather;
			if (debrisWeather != null && debrisWeather.Count > 0)
			{
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			Game1.randomizeRainPositions();
		}

		// Token: 0x060034CC RID: 13516 RVA: 0x002A5BAC File Offset: 0x002A3DAC
		public virtual void ParrotFlyAway()
		{
			this.location.removeTemporarySpritesWithIDLocal(777);
			this.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48, 0, 24, 24), new Vector2(14f, 4.5f) * 64f, false, 0f, Color.White)
			{
				id = 777,
				scale = 4f,
				totalNumberOfLoops = 99999,
				layerDepth = 1f,
				drawAboveAlwaysFront = true,
				interval = 50f,
				animationLength = 3,
				motion = new Vector2(-2f, 0f),
				acceleration = new Vector2(0f, -0.1f)
			});
		}

		// Token: 0x060034CD RID: 13517 RVA: 0x002A5C84 File Offset: 0x002A3E84
		public virtual void ParrotSquawk()
		{
			TemporaryAnimatedSprite parrot = this.location.getTemporarySpriteByID(777);
			if (parrot != null)
			{
				parrot.shakeIntensity = 1f;
				parrot.sourceRectStartingPos.X = 24f;
				parrot.sourceRect.X = 24;
				DelayedAction.functionAfterDelay(new Action(this.ParrotStopSquawk), 500);
			}
			Game1.playSound("parrot", null);
		}

		// Token: 0x060034CE RID: 13518 RVA: 0x002A5CF9 File Offset: 0x002A3EF9
		public virtual void ParrotStopSquawk()
		{
			TemporaryAnimatedSprite temporarySpriteByID = this.location.getTemporarySpriteByID(777);
			temporarySpriteByID.shakeIntensity = 0f;
			temporarySpriteByID.sourceRectStartingPos.X = 0f;
			temporarySpriteByID.sourceRect.X = 0;
		}

		// Token: 0x060034CF RID: 13519 RVA: 0x002A5D34 File Offset: 0x002A3F34
		public virtual void FinishTreehouse()
		{
			Game1.flashAlpha = 1f;
			Game1.playSound("yoba", null);
			Game1.playSound("axchop", null);
			(this.location as Mountain).ApplyTreehouseIfNecessary();
			this.location.removeTemporarySpritesWithIDLocal(778);
			for (int i = 0; i < 20; i++)
			{
				Vector2 start_point = new Vector2(Utility.RandomFloat(13f, 19f, null), Utility.RandomFloat(4f, 7f, null)) * 64f;
				float x_offset = 1024f - start_point.X;
				TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(192, Game1.random.Next(2) * 48, 24, 24), start_point, false, 0f, Color.White)
				{
					motion = new Vector2(x_offset * -0.01f, Utility.RandomFloat(-2f, 0f, null)),
					acceleration = new Vector2(0f, -0.05f),
					id = 778,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 50f,
					animationLength = 3,
					flipped = (x_offset > 0f),
					layerDepth = 1f,
					drawAboveAlwaysFront = true
				};
				this.location.TemporarySprites.Add(parrot);
			}
		}

		// Token: 0x060034D0 RID: 13520 RVA: 0x002A5EB4 File Offset: 0x002A40B4
		public void ParrotBounce(TemporaryAnimatedSprite sprite)
		{
			float x_offset = 1024f - sprite.Position.X;
			sprite.motion.X = (float)Math.Sign(x_offset) * Utility.RandomFloat(0.5f, 4f, null);
			sprite.motion.Y = Utility.RandomFloat(-15f, -10f, null);
			sprite.acceleration.Y = 0.5f;
			sprite.yStopCoordinate = 448;
			sprite.flipped = (x_offset > 0f);
			sprite.sourceRectStartingPos.X = (float)(48 + Game1.random.Next(2) * 72);
			if (Game1.random.NextDouble() < 0.05000000074505806)
			{
				Game1.playSound("axe", null);
				return;
			}
			if (Game1.random.NextDouble() < 0.05000000074505806)
			{
				Game1.playSound("crafting", null);
				return;
			}
			Game1.playSound("dirtyHit", null);
		}

		// Token: 0x060034D1 RID: 13521 RVA: 0x002A5FC0 File Offset: 0x002A41C0
		public void GoldenParrotBounce(TemporaryAnimatedSprite sprite)
		{
			sprite.motion.Y = Utility.RandomFloat(-3f, -5f, null);
			Game1.playSound("dirtyHit", null);
			this.location.temporarySprites.Add(new TemporaryAnimatedSprite(12, sprite.position, Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0));
		}

		// Token: 0x060034D2 RID: 13522 RVA: 0x002A6038 File Offset: 0x002A4238
		public override bool tickUpdate(GameTime time)
		{
			Game1.UpdateGameClock(time);
			this.location.updateWater(time);
			if (this.whichEvent.Value == 15)
			{
				int y = Game1.viewport.Y;
				Game1.viewport.Y = y + 1;
			}
			this.cutsceneLengthTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.timerSinceFade > 0)
			{
				this.timerSinceFade -= time.ElapsedGameTime.Milliseconds;
				Game1.globalFade = true;
				Game1.fadeToBlackAlpha = 1f;
				return this.timerSinceFade <= 0;
			}
			if (this.cutsceneLengthTimer <= 0 && !Game1.globalFade)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.endEvent), 0.01f);
			}
			this.soundTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.soundTimer <= 0 && this.sound != null)
			{
				Game1.playSound(this.sound, null);
				this.soundTimer = this.soundInterval;
			}
			return false;
		}

		// Token: 0x060034D3 RID: 13523 RVA: 0x002A614A File Offset: 0x002A434A
		public override void makeChangesToLocation()
		{
			base.makeChangesToLocation();
			if (this.whichEvent.Value == 15 && Game1.IsMasterGame)
			{
				ParrotUpgradePerch.ActivateGoldenParrot();
			}
		}

		// Token: 0x060034D4 RID: 13524 RVA: 0x002A6170 File Offset: 0x002A4370
		public void endEvent()
		{
			this.location.cleanupBeforePlayerExit();
			if (this.preEventLocation != null)
			{
				Game1.currentLocation = this.preEventLocation;
				Game1.currentLocation.resetForPlayerEntry();
				this.preEventLocation = null;
			}
			this.timerSinceFade = 1500;
			Game1.isRaining = this.wasRaining;
			Game1.getFarm().temporarySprites.Clear();
		}

		// Token: 0x0400228E RID: 8846
		public const int identifier = 942066;

		// Token: 0x0400228F RID: 8847
		public const int jojaGreenhouse = 0;

		// Token: 0x04002290 RID: 8848
		public const int junimoGreenHouse = 1;

		// Token: 0x04002291 RID: 8849
		public const int jojaBoiler = 2;

		// Token: 0x04002292 RID: 8850
		public const int junimoBoiler = 3;

		// Token: 0x04002293 RID: 8851
		public const int jojaBridge = 4;

		// Token: 0x04002294 RID: 8852
		public const int junimoBridge = 5;

		// Token: 0x04002295 RID: 8853
		public const int jojaBus = 6;

		// Token: 0x04002296 RID: 8854
		public const int junimoBus = 7;

		// Token: 0x04002297 RID: 8855
		public const int jojaBoulder = 8;

		// Token: 0x04002298 RID: 8856
		public const int junimoBoulder = 9;

		// Token: 0x04002299 RID: 8857
		public const int jojaMovieTheater = 10;

		// Token: 0x0400229A RID: 8858
		public const int junimoMovieTheater = 11;

		// Token: 0x0400229B RID: 8859
		public const int movieTheaterLightning = 12;

		// Token: 0x0400229C RID: 8860
		public const int willyBoatRepair = 13;

		// Token: 0x0400229D RID: 8861
		public const int treehouseBuild = 14;

		// Token: 0x0400229E RID: 8862
		public const int goldenParrots = 15;

		// Token: 0x0400229F RID: 8863
		public readonly NetInt whichEvent = new NetInt();

		// Token: 0x040022A0 RID: 8864
		private int cutsceneLengthTimer;

		// Token: 0x040022A1 RID: 8865
		private int timerSinceFade;

		// Token: 0x040022A2 RID: 8866
		private int soundTimer;

		// Token: 0x040022A3 RID: 8867
		private int soundInterval = 99999;

		// Token: 0x040022A4 RID: 8868
		private GameLocation location;

		// Token: 0x040022A5 RID: 8869
		private string sound;

		// Token: 0x040022A6 RID: 8870
		private bool wasRaining;

		// Token: 0x040022A7 RID: 8871
		public GameLocation preEventLocation;
	}
}

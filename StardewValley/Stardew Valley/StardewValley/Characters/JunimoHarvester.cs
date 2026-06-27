using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace StardewValley.Characters
{
	// Token: 0x0200037A RID: 890
	public class JunimoHarvester : NPC
	{
		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x0600367B RID: 13947 RVA: 0x002B096D File Offset: 0x002AEB6D
		// (set) Token: 0x0600367C RID: 13948 RVA: 0x002B097A File Offset: 0x002AEB7A
		public Guid HomeId
		{
			get
			{
				return this.netHome.Value;
			}
			set
			{
				this.netHome.Value = value;
			}
		}

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x0600367D RID: 13949 RVA: 0x002B0988 File Offset: 0x002AEB88
		// (set) Token: 0x0600367E RID: 13950 RVA: 0x002B09BC File Offset: 0x002AEBBC
		[XmlIgnore]
		public JunimoHut home
		{
			get
			{
				Building building;
				if (!base.currentLocation.buildings.TryGetValue(this.netHome.Value, out building))
				{
					return null;
				}
				return building as JunimoHut;
			}
			set
			{
				this.netHome.Value = base.currentLocation.buildings.GuidOf(value);
			}
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x0600367F RID: 13951 RVA: 0x002B09DA File Offset: 0x002AEBDA
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003680 RID: 13952 RVA: 0x002B09E0 File Offset: 0x002AEBE0
		public JunimoHarvester()
		{
		}

		// Token: 0x06003681 RID: 13953 RVA: 0x002B0A38 File Offset: 0x002AEC38
		public JunimoHarvester(GameLocation location, Vector2 position, JunimoHut hut, int whichJunimoNumberFromThisHut, Color? c) : base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo", null)
		{
			base.currentLocation = location;
			this.home = hut;
			this.whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
			if (c == null)
			{
				this.pickColor();
			}
			else
			{
				this.color.Value = c.Value;
			}
			this.nextPosition = this.GetBoundingBox();
			base.Breather = false;
			base.speed = 3;
			this.forceUpdateTimer = 9999;
			this.collidesWithOtherCharacters.Value = true;
			this.ignoreMovementAnimation = true;
			this.farmerPassesThrough = true;
			base.Scale = 0.75f;
			base.willDestroyObjectsUnderfoot = false;
			Vector2 tileToPathfindTo = Vector2.Zero;
			switch (whichJunimoNumberFromThisHut)
			{
			case 0:
				tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((float)(hut.tileX.Value + 1), (float)(hut.tileY.Value + hut.tilesHigh.Value + 1)), 30, true);
				break;
			case 1:
				tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((float)(hut.tileX.Value - 1), (float)hut.tileY.Value), 30, true);
				break;
			case 2:
				tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((float)(hut.tileX.Value + hut.tilesWide.Value), (float)hut.tileY.Value), 30, true);
				break;
			}
			if (tileToPathfindTo != Vector2.Zero)
			{
				this.controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(tileToPathfindTo), -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
			}
			PathFindController controller = this.controller;
			if (((controller != null) ? controller.pathToEndPoint : null) == null && Game1.IsMasterGame)
			{
				this.pathfindToRandomSpotAroundHut();
				PathFindController controller2 = this.controller;
				if (((controller2 != null) ? controller2.pathToEndPoint : null) == null)
				{
					this.destroy = true;
				}
			}
			this.collidesWithOtherCharacters.Value = false;
		}

		// Token: 0x06003682 RID: 13954 RVA: 0x002B0C74 File Offset: 0x002AEE74
		protected virtual void pickColor()
		{
			JunimoHut hut = this.home;
			if (hut == null)
			{
				this.color.Value = Color.White;
				return;
			}
			Random r = Utility.CreateRandom((double)hut.tileX.Value, (double)hut.tileY.Value * 777.0, (double)this.whichJunimoFromThisHut, 0.0, 0.0);
			if (r.NextBool(0.25))
			{
				if (r.NextBool(0.01))
				{
					this.color.Value = Color.White;
					return;
				}
				switch (r.Next(8))
				{
				case 0:
					this.color.Value = Color.Red;
					return;
				case 1:
					this.color.Value = Color.Goldenrod;
					return;
				case 2:
					this.color.Value = Color.Yellow;
					return;
				case 3:
					this.color.Value = Color.Lime;
					return;
				case 4:
					this.color.Value = new Color(0, 255, 180);
					return;
				case 5:
					this.color.Value = new Color(0, 100, 255);
					return;
				case 6:
					this.color.Value = Color.MediumPurple;
					return;
				default:
					this.color.Value = Color.Salmon;
					return;
				}
			}
			else
			{
				switch (r.Next(8))
				{
				case 0:
					this.color.Value = Color.LimeGreen;
					return;
				case 1:
					this.color.Value = Color.Orange;
					return;
				case 2:
					this.color.Value = Color.LightGreen;
					return;
				case 3:
					this.color.Value = Color.Tan;
					return;
				case 4:
					this.color.Value = Color.GreenYellow;
					return;
				case 5:
					this.color.Value = Color.LawnGreen;
					return;
				case 6:
					this.color.Value = Color.PaleGreen;
					return;
				default:
					this.color.Value = Color.Turquoise;
					return;
				}
			}
		}

		// Token: 0x06003683 RID: 13955 RVA: 0x002B0E90 File Offset: 0x002AF090
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.color, "color").AddField(this.netHome.NetFields, "netHome.NetFields").AddField(this.netAnimationEvent, "netAnimationEvent").AddField(this.isPrismatic, "isPrismatic");
			this.netAnimationEvent.onEvent += this.doAnimationEvent;
		}

		// Token: 0x06003684 RID: 13956 RVA: 0x002B0F07 File Offset: 0x002AF107
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite(content ?? Game1.content, "Characters\\Junimo");
			}
		}

		// Token: 0x06003685 RID: 13957 RVA: 0x002B0F2C File Offset: 0x002AF12C
		protected virtual void doAnimationEvent(int animId)
		{
			switch (animId)
			{
			case 0:
				this.Sprite.CurrentAnimation = null;
				return;
			case 1:
				break;
			case 2:
				this.Sprite.currentFrame = 0;
				return;
			case 3:
				this.Sprite.currentFrame = 1;
				return;
			case 4:
				this.Sprite.currentFrame = 2;
				return;
			case 5:
				this.Sprite.currentFrame = 44;
				return;
			case 6:
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(12, 200),
					new FarmerSprite.AnimationFrame(13, 200),
					new FarmerSprite.AnimationFrame(14, 200),
					new FarmerSprite.AnimationFrame(15, 200)
				});
				return;
			case 7:
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(44, 200),
					new FarmerSprite.AnimationFrame(45, 200),
					new FarmerSprite.AnimationFrame(46, 200),
					new FarmerSprite.AnimationFrame(47, 200)
				});
				return;
			case 8:
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 100),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(31, 100)
				});
				break;
			default:
				return;
			}
		}

		// Token: 0x06003686 RID: 13958 RVA: 0x002B10A4 File Offset: 0x002AF2A4
		public virtual void reachFirstDestinationFromHut(Character c, GameLocation l)
		{
			this.tryToHarvestHere();
		}

		// Token: 0x06003687 RID: 13959 RVA: 0x002B10AC File Offset: 0x002AF2AC
		public virtual void tryToHarvestHere()
		{
			if (base.currentLocation != null)
			{
				if (this.isHarvestable())
				{
					this.harvestTimer = 2000;
					return;
				}
				this.pokeToHarvest();
			}
		}

		// Token: 0x06003688 RID: 13960 RVA: 0x002B10D0 File Offset: 0x002AF2D0
		public virtual void pokeToHarvest()
		{
			JunimoHut hut = this.home;
			if (hut == null)
			{
				return;
			}
			if (!hut.isTilePassable(base.Tile) && Game1.IsMasterGame)
			{
				this.destroy = true;
				return;
			}
			if (this.harvestTimer <= 0 && Game1.random.NextDouble() < 0.7)
			{
				this.pathfindToNewCrop();
			}
		}

		// Token: 0x06003689 RID: 13961 RVA: 0x002B1129 File Offset: 0x002AF329
		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		// Token: 0x0600368A RID: 13962 RVA: 0x002B112C File Offset: 0x002AF32C
		public void setMoving(int xSpeed, int ySpeed)
		{
			this.motion.X = (float)xSpeed;
			this.motion.Y = (float)ySpeed;
		}

		// Token: 0x0600368B RID: 13963 RVA: 0x002B1148 File Offset: 0x002AF348
		public void setMoving(Vector2 motion)
		{
			this.motion = motion;
		}

		// Token: 0x0600368C RID: 13964 RVA: 0x002B1151 File Offset: 0x002AF351
		public override void Halt()
		{
			base.Halt();
			this.motion = Vector2.Zero;
		}

		// Token: 0x0600368D RID: 13965 RVA: 0x002B1164 File Offset: 0x002AF364
		public override bool canTalk()
		{
			return false;
		}

		// Token: 0x0600368E RID: 13966 RVA: 0x002B1167 File Offset: 0x002AF367
		public void junimoReachedHut(Character c, GameLocation l)
		{
			this.controller = null;
			this.motion.X = 0f;
			this.motion.Y = -1f;
			this.destroy = true;
		}

		// Token: 0x0600368F RID: 13967 RVA: 0x002B1198 File Offset: 0x002AF398
		public virtual bool foundCropEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			TerrainFeature terrainFeature;
			if (location.terrainFeatures.TryGetValue(new Vector2((float)currentNode.x, (float)currentNode.y), out terrainFeature))
			{
				if (location.isCropAtTile(currentNode.x, currentNode.y) && (terrainFeature as HoeDirt).readyForHarvest())
				{
					return true;
				}
				Bush bush = terrainFeature as Bush;
				if (bush != null && bush.readyForHarvest())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003690 RID: 13968 RVA: 0x002B1200 File Offset: 0x002AF400
		public virtual void pathfindToNewCrop()
		{
			JunimoHut hut = this.home;
			if (hut == null)
			{
				return;
			}
			if (Game1.timeOfDay > 1900)
			{
				if (this.controller == null)
				{
					this.returnToJunimoHut(base.currentLocation);
				}
				return;
			}
			if (Game1.random.NextDouble() < 0.035 || hut.noHarvest.Value)
			{
				this.pathfindToRandomSpotAroundHut();
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new PathFindController.isAtEnd(this.foundCropEndFunction), -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100, Point.Zero, true);
			Stack<Point> pathToEndPoint;
			Point? endpoint = ((pathToEndPoint = this.controller.pathToEndPoint) != null) ? new Point?(pathToEndPoint.Last<Point>()) : null;
			if (endpoint != null && Math.Abs(endpoint.Value.X - (hut.tileX.Value + 1)) <= hut.cropHarvestRadius && Math.Abs(endpoint.Value.Y - (hut.tileY.Value + 1)) <= hut.cropHarvestRadius)
			{
				this.netAnimationEvent.Fire(0);
				return;
			}
			if (Game1.random.NextBool() && !hut.lastKnownCropLocation.Equals(Point.Zero))
			{
				this.controller = new PathFindController(this, base.currentLocation, hut.lastKnownCropLocation, -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
				return;
			}
			if (Game1.random.NextDouble() < 0.25)
			{
				this.netAnimationEvent.Fire(0);
				this.returnToJunimoHut(base.currentLocation);
				return;
			}
			this.pathfindToRandomSpotAroundHut();
		}

		// Token: 0x06003691 RID: 13969 RVA: 0x002B139C File Offset: 0x002AF59C
		public virtual void returnToJunimoHut(GameLocation location)
		{
			if (Utility.isOnScreen(Utility.Vector2ToPoint(this.position.Value / 64f), 64, base.currentLocation))
			{
				this.jump();
			}
			this.collidesWithOtherCharacters.Value = false;
			if (Game1.IsMasterGame)
			{
				JunimoHut hut = this.home;
				if (hut == null)
				{
					return;
				}
				this.controller = new PathFindController(this, location, new Point(hut.tileX.Value + 1, hut.tileY.Value + 1), 0, new PathFindController.endBehavior(this.junimoReachedHut));
				if (this.controller.pathToEndPoint == null || this.controller.pathToEndPoint.Count == 0 || location.isCollidingPosition(this.nextPosition, Game1.viewport, false, 0, false, this))
				{
					this.destroy = true;
				}
			}
			if (Utility.isOnScreen(Utility.Vector2ToPoint(this.position.Value / 64f), 64, base.currentLocation))
			{
				location.playSound("junimoMeep1", null, null, SoundContext.Default);
			}
		}

		// Token: 0x06003692 RID: 13970 RVA: 0x002B14B6 File Offset: 0x002AF6B6
		public override void faceDirection(int direction)
		{
		}

		// Token: 0x06003693 RID: 13971 RVA: 0x002B14B8 File Offset: 0x002AF6B8
		protected override void updateSlaveAnimation(GameTime time)
		{
		}

		// Token: 0x06003694 RID: 13972 RVA: 0x002B14BC File Offset: 0x002AF6BC
		protected virtual bool isHarvestable()
		{
			TerrainFeature terrainFeature;
			if (base.currentLocation.terrainFeatures.TryGetValue(base.Tile, out terrainFeature))
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null)
				{
					return dirt.readyForHarvest();
				}
				Bush bush = terrainFeature as Bush;
				if (bush != null)
				{
					return bush.readyForHarvest();
				}
			}
			return false;
		}

		// Token: 0x06003695 RID: 13973 RVA: 0x002B1508 File Offset: 0x002AF708
		public override void update(GameTime time, GameLocation location)
		{
			this.netAnimationEvent.Poll();
			base.update(time, location);
			if (this.isPrismatic.Value)
			{
				this.color.Value = Utility.GetPrismaticColor(this.whichJunimoFromThisHut, 1f);
			}
			this.forceUpdateTimer = 99999;
			if (this.EventActor)
			{
				return;
			}
			if (this.destroy)
			{
				this.alphaChange = -0.05f;
			}
			this.alpha += this.alphaChange;
			if (this.alpha > 1f)
			{
				this.alpha = 1f;
			}
			else if (this.alpha < 0f)
			{
				this.alpha = 0f;
				if (this.destroy && Game1.IsMasterGame)
				{
					location.characters.Remove(this);
					JunimoHut home = this.home;
					if (home != null)
					{
						home.myJunimos.Remove(this);
					}
				}
			}
			if (Game1.IsMasterGame)
			{
				if (this.harvestTimer > 0)
				{
					int oldTimer = this.harvestTimer;
					this.harvestTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.harvestTimer > 1800)
					{
						this.netAnimationEvent.Fire(2);
					}
					else if (this.harvestTimer > 1600)
					{
						this.netAnimationEvent.Fire(3);
					}
					else if (this.harvestTimer > 1000)
					{
						this.netAnimationEvent.Fire(4);
						base.shake(50);
					}
					else if (oldTimer >= 1000 && this.harvestTimer < 1000)
					{
						this.netAnimationEvent.Fire(2);
						JunimoHut hut = this.home;
						if (base.currentLocation != null && hut != null && !hut.noHarvest.Value && this.isHarvestable())
						{
							this.netAnimationEvent.Fire(5);
							this.lastItemHarvested = null;
							TerrainFeature terrainFeature = base.currentLocation.terrainFeatures[base.Tile];
							Bush bush = terrainFeature as Bush;
							if (bush == null)
							{
								HoeDirt dirt = terrainFeature as HoeDirt;
								if (dirt != null)
								{
									if (dirt.crop.harvest(base.TilePoint.X, base.TilePoint.Y, dirt, this, false))
									{
										dirt.destroyCrop(base.currentLocation.farmers.Any());
									}
								}
							}
							else if (bush.readyForHarvest())
							{
								this.tryToAddItemToHut(ItemRegistry.Create("(O)815", 1, 0, false));
								bush.tileSheetOffset.Value = 0;
								bush.setUpSourceRect();
								if (Utility.isOnScreen(base.TilePoint, 64, base.currentLocation))
								{
									bush.performUseAction(base.Tile);
								}
								if (Utility.isOnScreen(base.TilePoint, 64, base.currentLocation))
								{
									DelayedAction.playSoundAfterDelay("coin", 260, base.currentLocation, null, -1, false);
								}
							}
							if (this.lastItemHarvested != null)
							{
								bool gotDouble = false;
								if (this.home.raisinDays.Value > 0 && Game1.random.NextDouble() < 0.2)
								{
									gotDouble = true;
									Item i = this.lastItemHarvested.getOne();
									i.Quality = this.lastItemHarvested.Quality;
									this.tryToAddItemToHut(i);
								}
								if (base.currentLocation.farmers.Any())
								{
									ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this.lastItemHarvested.QualifiedItemId);
									float mainDrawLayer = (float)base.StandingPixel.Y / 10000f + 0.01f;
									if (gotDouble)
									{
										for (int j = 0; j < 2; j++)
										{
											Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
											{
												new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(0, null), 1000f, 1, 0, base.Position + new Vector2(0f, -40f), false, false, mainDrawLayer, 0.02f, Color.White, 4f, -0.01f, 0f, 0f, false)
												{
													motion = new Vector2((float)((j == 0) ? -1 : 1) * 0.5f, -0.25f),
													delayBeforeAnimationStart = 200
												}
											});
											ColoredObject coloredObj2 = this.lastItemHarvested as ColoredObject;
											if (coloredObj2 != null)
											{
												Rectangle colored_source_rect = ItemRegistry.GetDataOrErrorItem(this.lastItemHarvested.QualifiedItemId).GetSourceRect(1, null);
												Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
												{
													new TemporaryAnimatedSprite(itemData.TextureName, colored_source_rect, 1000f, 1, 0, base.Position + new Vector2(0f, -40f), false, false, mainDrawLayer + 0.005f, 0.02f, coloredObj2.color.Value, 4f, -0.01f, 0f, 0f, false)
													{
														motion = new Vector2((float)((j == 0) ? -1 : 1) * 0.5f, -0.25f),
														delayBeforeAnimationStart = 200
													}
												});
											}
										}
									}
									else
									{
										Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
										{
											new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(0, null), 1000f, 1, 0, base.Position + new Vector2(0f, -40f), false, false, mainDrawLayer, 0.02f, Color.White, 4f, -0.01f, 0f, 0f, false)
											{
												motion = new Vector2(0.08f, -0.25f)
											}
										});
										ColoredObject coloredObj3 = this.lastItemHarvested as ColoredObject;
										if (coloredObj3 != null)
										{
											Rectangle colored_source_rect2 = ItemRegistry.GetDataOrErrorItem(this.lastItemHarvested.QualifiedItemId).GetSourceRect(1, null);
											Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
											{
												new TemporaryAnimatedSprite(itemData.TextureName, colored_source_rect2, 1000f, 1, 0, base.Position + new Vector2(0f, -40f), false, false, mainDrawLayer + 0.005f, 0.02f, coloredObj3.color.Value, 4f, -0.01f, 0f, 0f, false)
												{
													motion = new Vector2(0.08f, -0.25f)
												}
											});
										}
									}
								}
							}
						}
					}
					else if (this.harvestTimer <= 0)
					{
						this.pokeToHarvest();
					}
				}
				else if (this.alpha > 0f && this.controller == null)
				{
					if ((this.addedSpeed > 0f || base.speed > 3 || this.isCharging) && Game1.IsMasterGame)
					{
						this.destroy = true;
					}
					this.nextPosition = this.GetBoundingBox();
					this.nextPosition.X = this.nextPosition.X + (int)this.motion.X;
					bool sparkle = false;
					if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this))
					{
						this.position.X += (float)((int)this.motion.X);
						sparkle = true;
					}
					this.nextPosition.X = this.nextPosition.X - (int)this.motion.X;
					this.nextPosition.Y = this.nextPosition.Y + (int)this.motion.Y;
					if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this))
					{
						this.position.Y += (float)((int)this.motion.Y);
						sparkle = true;
					}
					if (!this.motion.Equals(Vector2.Zero) && sparkle && Game1.random.NextDouble() < 0.005)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(Game1.random.Choose(10, 11), base.Position, this.color.Value, 8, false, 100f, 0, -1, -1f, -1, 0)
							{
								motion = this.motion / 4f,
								alphaFade = 0.01f,
								layerDepth = 0.8f,
								scale = 0.75f,
								alpha = 0.75f
							}
						});
					}
					if (Game1.random.NextDouble() < 0.002)
					{
						switch (Game1.random.Next(6))
						{
						case 0:
							this.netAnimationEvent.Fire(6);
							break;
						case 1:
							this.netAnimationEvent.Fire(7);
							break;
						case 2:
							this.netAnimationEvent.Fire(0);
							break;
						case 3:
							this.jumpWithoutSound(8f);
							this.yJumpVelocity /= 2f;
							this.netAnimationEvent.Fire(0);
							break;
						case 4:
						{
							JunimoHut hut2 = this.home;
							if (hut2 != null && !hut2.noHarvest.Value)
							{
								this.pathfindToNewCrop();
							}
							break;
						}
						case 5:
							this.netAnimationEvent.Fire(8);
							break;
						}
					}
				}
			}
			bool moveRight = this.moveRight;
			bool moveLeft = this.moveLeft;
			bool moveUp = this.moveUp;
			bool moveDown = this.moveDown;
			if (Game1.IsMasterGame)
			{
				if (this.controller == null && this.motion.Equals(Vector2.Zero))
				{
					return;
				}
				moveRight |= (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X > 0f);
				moveLeft |= (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X < 0f);
				moveUp |= (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y < 0f);
				moveDown |= (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y > 0f);
			}
			else
			{
				moveLeft = (base.IsRemoteMoving() && this.FacingDirection == 3);
				moveRight = (base.IsRemoteMoving() && this.FacingDirection == 1);
				moveUp = (base.IsRemoteMoving() && this.FacingDirection == 0);
				moveDown = (base.IsRemoteMoving() && this.FacingDirection == 2);
				if (!moveRight && !moveLeft && !moveUp && !moveDown)
				{
					return;
				}
			}
			this.Sprite.CurrentAnimation = null;
			if (moveRight)
			{
				this.flip = false;
				if (this.Sprite.Animate(time, 16, 8, 50f))
				{
					this.Sprite.currentFrame = 16;
					return;
				}
			}
			else
			{
				if (moveLeft)
				{
					if (this.Sprite.Animate(time, 16, 8, 50f))
					{
						this.Sprite.currentFrame = 16;
					}
					this.flip = true;
					return;
				}
				if (moveUp)
				{
					if (this.Sprite.Animate(time, 32, 8, 50f))
					{
						this.Sprite.currentFrame = 32;
						return;
					}
				}
				else if (moveDown)
				{
					this.Sprite.Animate(time, 0, 8, 50f);
				}
			}
		}

		// Token: 0x06003696 RID: 13974 RVA: 0x002B2084 File Offset: 0x002B0284
		public virtual void pathfindToRandomSpotAroundHut()
		{
			JunimoHut hut = this.home;
			if (hut == null)
			{
				return;
			}
			Vector2 tileToPathfindTo = new Vector2((float)(hut.tileX.Value + 1 + Game1.random.Next(-hut.cropHarvestRadius, hut.cropHarvestRadius + 1)), (float)(hut.tileY.Value + 1 + Game1.random.Next(-hut.cropHarvestRadius, hut.cropHarvestRadius + 1)));
			this.controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(tileToPathfindTo), -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
		}

		// Token: 0x06003697 RID: 13975 RVA: 0x002B211C File Offset: 0x002B031C
		public virtual void tryToAddItemToHut(Item i)
		{
			this.lastItemHarvested = i;
			JunimoHut home = this.home;
			Item result = (home != null) ? home.GetOutputChest().addItem(i) : null;
			if (result != null)
			{
				for (int j = 0; j < result.Stack; j++)
				{
					Game1.createItemDebris(i.getOne(), base.Position, -1, base.currentLocation, -1, false);
				}
			}
		}

		// Token: 0x06003698 RID: 13976 RVA: 0x002B2178 File Offset: 0x002B0378
		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			if (this.alpha > 0f)
			{
				float mainDrawLayer = (float)(base.StandingPixel.Y + 2) / 10000f;
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow((double)(this.Sprite.SpriteHeight / 16), 2.0) + (float)this.yJumpOffset - 8f) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(this.Sprite.SourceRect), this.color.Value * this.alpha, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : mainDrawLayer));
				if (!this.swimming.Value)
				{
					b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2((float)(this.Sprite.SpriteWidth * 4) / 2f, 44f)), new Rectangle?(Game1.shadowTexture.Bounds), this.color.Value * this.alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (4f + (float)this.yJumpOffset / 40f) * this.scale.Value, SpriteEffects.None, Math.Max(0f, mainDrawLayer) - 1E-06f);
				}
			}
		}

		// Token: 0x040023A2 RID: 9122
		protected float alpha = 1f;

		// Token: 0x040023A3 RID: 9123
		protected float alphaChange;

		// Token: 0x040023A4 RID: 9124
		protected Vector2 motion = Vector2.Zero;

		// Token: 0x040023A5 RID: 9125
		protected new Rectangle nextPosition;

		// Token: 0x040023A6 RID: 9126
		protected readonly NetColor color = new NetColor();

		// Token: 0x040023A7 RID: 9127
		protected bool destroy;

		// Token: 0x040023A8 RID: 9128
		protected Item lastItemHarvested;

		// Token: 0x040023A9 RID: 9129
		public int whichJunimoFromThisHut;

		// Token: 0x040023AA RID: 9130
		protected int harvestTimer;

		// Token: 0x040023AB RID: 9131
		public readonly NetBool isPrismatic = new NetBool(false);

		// Token: 0x040023AC RID: 9132
		protected readonly NetGuid netHome = new NetGuid();

		// Token: 0x040023AD RID: 9133
		protected readonly NetEvent1Field<int, NetInt> netAnimationEvent = new NetEvent1Field<int, NetInt>();
	}
}

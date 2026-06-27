using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley.Characters
{
	// Token: 0x02000376 RID: 886
	public class Child : NPC
	{
		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06003610 RID: 13840 RVA: 0x002A9D74 File Offset: 0x002A7F74
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003611 RID: 13841 RVA: 0x002A9D78 File Offset: 0x002A7F78
		public Child()
		{
		}

		// Token: 0x06003612 RID: 13842 RVA: 0x002A9DD4 File Offset: 0x002A7FD4
		public Child(string name, bool isMale, bool isDarkSkinned, Farmer parent)
		{
			base.Age = 2;
			this.Gender = (isMale ? Gender.Male : Gender.Female);
			this.darkSkinned.Value = isDarkSkinned;
			this.reloadSprite(false);
			base.Name = name;
			this.displayName = name;
			base.DefaultMap = "FarmHouse";
			base.HideShadow = true;
			base.speed = 1;
			this.idOfParent.Value = parent.UniqueMultiplayerID;
			base.Breather = false;
		}

		// Token: 0x06003613 RID: 13843 RVA: 0x002A9E94 File Offset: 0x002A8094
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.setStateEvent, "setStateEvent").AddField(this.darkSkinned, "darkSkinned").AddField(this.daysOld, "daysOld").AddField(this.idOfParent, "idOfParent").AddField(this.mutex.NetFields, "mutex.NetFields").AddField(this.hat, "hat");
			this.age.fieldChangeVisibleEvent += delegate(NetInt a, int b, int c)
			{
				this.reloadSprite(false);
			};
			this.setStateEvent.onEvent += this.doSetState;
			this.name.FilterStringEvent += Utility.FilterDirtyWords;
		}

		// Token: 0x06003614 RID: 13844 RVA: 0x002A9F58 File Offset: 0x002A8158
		public override void reloadSprite(bool onlyAppearance = false)
		{
			if (Game1.IsMasterGame && !onlyAppearance)
			{
				Farmer parent = Game1.GetPlayer(this.idOfParent.Value, false);
				if (this.idOfParent.Value == 0L || parent == null)
				{
					long parent_unique_id = Game1.MasterPlayer.UniqueMultiplayerID;
					if (base.currentLocation is FarmHouse)
					{
						foreach (Farmer farmer in Game1.getAllFarmers())
						{
							if (Utility.getHomeOfFarmer(farmer) == base.currentLocation)
							{
								parent_unique_id = farmer.UniqueMultiplayerID;
								break;
							}
						}
					}
					this.idOfParent.Value = parent_unique_id;
				}
			}
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite("Characters\\Baby" + (this.darkSkinned.Value ? "_dark" : ""), 0, 22, 16);
			}
			if (base.Age >= 3)
			{
				this.Sprite.textureName.Value = "Characters\\Toddler" + ((this.Gender == Gender.Male) ? "" : "_girl") + (this.darkSkinned.Value ? "_dark" : "");
				this.Sprite.SpriteWidth = 16;
				this.Sprite.SpriteHeight = 32;
				this.Sprite.currentFrame = 0;
				base.HideShadow = false;
			}
			else
			{
				this.Sprite.textureName.Value = "Characters\\Baby" + (this.darkSkinned.Value ? "_dark" : "");
				this.Sprite.SpriteWidth = 22;
				this.Sprite.SpriteHeight = ((base.Age == 1) ? 32 : 16);
				this.Sprite.currentFrame = 0;
				int age = base.Age;
				if (age != 1)
				{
					if (age == 2)
					{
						this.Sprite.currentFrame = 32;
					}
				}
				else
				{
					this.Sprite.currentFrame = 4;
				}
				base.HideShadow = true;
			}
			this.Sprite.UpdateSourceRect();
			base.Breather = false;
		}

		// Token: 0x06003615 RID: 13845 RVA: 0x002AA178 File Offset: 0x002A8378
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
			AnimatedSprite sprite = this.Sprite;
			if (((sprite != null) ? sprite.Texture : null) == null)
			{
				this.reloadSprite(true);
			}
		}

		// Token: 0x06003616 RID: 13846 RVA: 0x002AA195 File Offset: 0x002A8395
		protected override void updateSlaveAnimation(GameTime time)
		{
			if (base.Age >= 2)
			{
				if (this.Sprite.currentFrame <= 7 && this.Sprite.SpriteHeight == 16)
				{
					return;
				}
				base.updateSlaveAnimation(time);
			}
		}

		// Token: 0x06003617 RID: 13847 RVA: 0x002AA1C8 File Offset: 0x002A83C8
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
			{
				base.MovePosition(time, viewport, currentLocation);
				return;
			}
			if (!Game1.IsMasterGame)
			{
				this.moveLeft = (base.IsRemoteMoving() && this.FacingDirection == 3);
				this.moveRight = (base.IsRemoteMoving() && this.FacingDirection == 1);
				this.moveUp = (base.IsRemoteMoving() && this.FacingDirection == 0);
				this.moveDown = (base.IsRemoteMoving() && this.FacingDirection == 2);
			}
			if (this.moveUp)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, 0, false, this) || this.isCharging)
				{
					if (Game1.IsMasterGame)
					{
						this.position.Y -= (float)base.speed + this.addedSpeed;
					}
					if (base.Age == 3)
					{
						this.Sprite.AnimateUp(time, 0, "");
						this.FacingDirection = 0;
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.moveUp = false;
					this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
					this.Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						this.setCrawlerInNewDirection();
					}
				}
			}
			else if (this.moveRight)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, 0, false, this) || this.isCharging)
				{
					if (Game1.IsMasterGame)
					{
						this.position.X += (float)base.speed + this.addedSpeed;
					}
					if (base.Age == 3)
					{
						this.Sprite.AnimateRight(time, 0, "");
						this.FacingDirection = 1;
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.moveRight = false;
					this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
					this.Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						this.setCrawlerInNewDirection();
					}
				}
			}
			else if (this.moveDown)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, 0, false, this) || this.isCharging)
				{
					if (Game1.IsMasterGame)
					{
						this.position.Y += (float)base.speed + this.addedSpeed;
					}
					if (base.Age == 3)
					{
						this.Sprite.AnimateDown(time, 0, "");
						this.FacingDirection = 2;
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.moveDown = false;
					this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
					this.Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						this.setCrawlerInNewDirection();
					}
				}
			}
			else if (this.moveLeft)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, 0, false, this) || this.isCharging)
				{
					if (Game1.IsMasterGame)
					{
						this.position.X -= (float)base.speed + this.addedSpeed;
					}
					if (base.Age == 3)
					{
						this.Sprite.AnimateLeft(time, 0, "");
						this.FacingDirection = 3;
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.moveLeft = false;
					this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
					this.Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						this.setCrawlerInNewDirection();
					}
				}
			}
			if (this.blockedInterval >= 3000 && (float)this.blockedInterval <= 3750f && !Game1.eventUp)
			{
				base.doEmote(Game1.random.Choose(8, 40), true);
				this.blockedInterval = 3750;
				return;
			}
			if (this.blockedInterval >= 5000)
			{
				base.speed = 1;
				this.isCharging = true;
				this.blockedInterval = 0;
			}
		}

		// Token: 0x06003618 RID: 13848 RVA: 0x002AA70B File Offset: 0x002A890B
		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		// Token: 0x06003619 RID: 13849 RVA: 0x002AA710 File Offset: 0x002A8910
		public override void resetForNewDay(int dayOfMonth)
		{
			base.resetForNewDay(dayOfMonth);
			FarmHouse farmhouse = base.currentLocation as FarmHouse;
			if (farmhouse != null && farmhouse.GetChildBed(this.GetChildIndex()) == null)
			{
				this.sleptInBed.Value = false;
			}
		}

		// Token: 0x0600361A RID: 13850 RVA: 0x002AA74D File Offset: 0x002A894D
		protected override string translateName()
		{
			return this.name.Value.TrimEnd();
		}

		// Token: 0x0600361B RID: 13851 RVA: 0x002AA75F File Offset: 0x002A895F
		public override void reloadData()
		{
		}

		// Token: 0x0600361C RID: 13852 RVA: 0x002AA764 File Offset: 0x002A8964
		public override void dayUpdate(int dayOfMonth)
		{
			base.UpdateInvisibilityOnNewDay();
			this.resetForNewDay(dayOfMonth);
			this.mutex.ReleaseLock();
			this.moveUp = false;
			this.moveDown = false;
			this.moveLeft = false;
			this.moveRight = false;
			int parent_unique_id = (int)Game1.MasterPlayer.UniqueMultiplayerID;
			FarmHouse farmhouse = Game1.currentLocation as FarmHouse;
			if (farmhouse != null && farmhouse.HasOwner)
			{
				parent_unique_id = (int)farmhouse.OwnerId;
			}
			Random r = Utility.CreateDaySaveRandom((double)(parent_unique_id * 2), 0.0, 0.0);
			this.daysOld.Value = this.daysOld.Value + 1;
			if (this.daysOld.Value >= 55)
			{
				base.Age = 3;
				base.speed = 4;
			}
			else if (this.daysOld.Value >= 27)
			{
				base.Age = 2;
			}
			else if (this.daysOld.Value >= 13)
			{
				base.Age = 1;
			}
			if (this.age.Value == 0 || this.age.Value == 1)
			{
				base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
			}
			if (base.Age == 2)
			{
				base.speed = 1;
				Point p = (base.currentLocation as FarmHouse).getRandomOpenPointInHouse(r, 1, 200);
				if (!p.Equals(Point.Zero))
				{
					base.setTilePosition(p);
				}
				else
				{
					base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -24f);
				}
				this.Sprite.CurrentAnimation = null;
			}
			if (base.Age == 3)
			{
				Point p2 = (base.currentLocation as FarmHouse).getRandomOpenPointInHouse(r, 1, 200);
				if (!p2.Equals(Point.Zero))
				{
					base.setTilePosition(p2);
				}
				else
				{
					p2 = (base.currentLocation as FarmHouse).GetChildBedSpot(this.GetChildIndex());
					if (!p2.Equals(Point.Zero))
					{
						base.setTilePosition(p2);
					}
				}
				this.Sprite.CurrentAnimation = null;
			}
			this.reloadSprite(false);
			if (base.Age == 2)
			{
				this.setCrawlerInNewDirection();
			}
		}

		// Token: 0x0600361D RID: 13853 RVA: 0x002AA9A8 File Offset: 0x002A8BA8
		public bool isInCrib()
		{
			Point tile = base.TilePoint;
			return tile.X >= 30 && tile.X <= 32 && tile.Y >= 13 && tile.Y <= 14;
		}

		// Token: 0x0600361E RID: 13854 RVA: 0x002AA9E9 File Offset: 0x002A8BE9
		public override bool hasDarkSkin()
		{
			return this.darkSkinned.Value;
		}

		// Token: 0x0600361F RID: 13855 RVA: 0x002AA9F8 File Offset: 0x002A8BF8
		public void toss(Farmer who)
		{
			if (base.IsInvisible)
			{
				return;
			}
			if (Game1.timeOfDay >= 1800 || this.Sprite.SpriteHeight <= 16)
			{
				return;
			}
			if (who == Game1.player)
			{
				this.mutex.RequestLock(delegate
				{
					this.performToss(who);
				}, null);
				return;
			}
			this.performToss(who);
		}

		// Token: 0x06003620 RID: 13856 RVA: 0x002AAA70 File Offset: 0x002A8C70
		public void performToss(Farmer who)
		{
			who.forceTimePass = true;
			who.faceDirection(2);
			who.FarmerSprite.PauseForSingleAnimation = false;
			base.Position = who.Position + new Vector2(-16f, -96f);
			Stats stats = who.stats;
			if (stats != null)
			{
				stats.Increment("timesTossedBaby", 1);
			}
			if (Game1.random.NextDouble() < 0.01 && who.stats.Get("timesTossedBaby") > 3U)
			{
				this.yJumpVelocity = 30f;
				who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
				{
					new FarmerSprite.AnimationFrame(57, 2500, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doneTossing), true)
				}, null);
				who.freezePause = 2500;
				Game1.playSound("crit", null);
			}
			else
			{
				who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
				{
					new FarmerSprite.AnimationFrame(57, 1500, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doneTossing), true)
				}, null);
				who.freezePause = 1500;
				this.yJumpVelocity = (float)Game1.random.Next(12, 19);
				Game1.playSound("dwop", null);
			}
			this.yJumpOffset = -1;
			who.CanMove = false;
			this.drawOnTop = true;
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(4, 100),
				new FarmerSprite.AnimationFrame(5, 100),
				new FarmerSprite.AnimationFrame(6, 100),
				new FarmerSprite.AnimationFrame(7, 100)
			});
		}

		// Token: 0x06003621 RID: 13857 RVA: 0x002AAC1C File Offset: 0x002A8E1C
		public void doneTossing(Farmer who)
		{
			who.forceTimePass = false;
			this.resetForPlayerEntry(who.currentLocation);
			who.CanMove = true;
			who.forceCanMove();
			who.faceDirection(0);
			this.drawOnTop = false;
			base.doEmote(20, true);
			if (!who.friendshipData.ContainsKey(base.Name))
			{
				who.friendshipData.Add(base.Name, new Friendship(250));
			}
			who.talkToFriend(this, 20);
			Game1.playSound("tinyWhip", null);
			if (this.mutex.IsLockHeld())
			{
				this.mutex.ReleaseLock();
			}
		}

		// Token: 0x06003622 RID: 13858 RVA: 0x002AACC4 File Offset: 0x002A8EC4
		public override Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
		{
			switch (base.Age)
			{
			case 0:
				return new Microsoft.Xna.Framework.Rectangle(0, 0, 22, 16);
			case 1:
				return new Microsoft.Xna.Framework.Rectangle(0, 42, 22, 24);
			case 2:
				return new Microsoft.Xna.Framework.Rectangle(0, 112, 22, 16);
			case 3:
				return new Microsoft.Xna.Framework.Rectangle(0, 4, 16, 24);
			default:
				return Microsoft.Xna.Framework.Rectangle.Empty;
			}
		}

		// Token: 0x06003623 RID: 13859 RVA: 0x002AAD27 File Offset: 0x002A8F27
		private void setState(int state)
		{
			this.setStateEvent.Fire(state);
		}

		// Token: 0x06003624 RID: 13860 RVA: 0x002AAD38 File Offset: 0x002A8F38
		private void doSetState(int state)
		{
			switch (state)
			{
			case 0:
				base.SetMovingOnlyUp();
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(32, 160),
					new FarmerSprite.AnimationFrame(33, 160),
					new FarmerSprite.AnimationFrame(34, 160),
					new FarmerSprite.AnimationFrame(35, 160)
				});
				return;
			case 1:
				base.SetMovingOnlyRight();
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 160),
					new FarmerSprite.AnimationFrame(29, 160),
					new FarmerSprite.AnimationFrame(30, 160),
					new FarmerSprite.AnimationFrame(31, 160)
				});
				return;
			case 2:
				base.SetMovingOnlyDown();
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(24, 160),
					new FarmerSprite.AnimationFrame(25, 160),
					new FarmerSprite.AnimationFrame(26, 160),
					new FarmerSprite.AnimationFrame(27, 160)
				});
				return;
			case 3:
				base.SetMovingOnlyLeft();
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(36, 160),
					new FarmerSprite.AnimationFrame(37, 160),
					new FarmerSprite.AnimationFrame(38, 160),
					new FarmerSprite.AnimationFrame(39, 160)
				});
				return;
			case 4:
				this.Halt();
				this.Sprite.SpriteHeight = 16;
				this.Sprite.setCurrentAnimation(this.getRandomCrawlerAnimation(0));
				return;
			case 5:
				this.Halt();
				this.Sprite.SpriteHeight = 16;
				this.Sprite.setCurrentAnimation(this.getRandomCrawlerAnimation(1));
				return;
			default:
				return;
			}
		}

		// Token: 0x06003625 RID: 13861 RVA: 0x002AAF2C File Offset: 0x002A912C
		private void setCrawlerInNewDirection()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			base.speed = 1;
			int state = Game1.random.Next(6);
			if (Game1.timeOfDay >= 1800 && this.isInCrib())
			{
				this.Sprite.currentFrame = 7;
				this.Sprite.UpdateSourceRect();
				return;
			}
			if (this.previousState >= 4 && Game1.random.NextDouble() < 0.6)
			{
				state = this.previousState;
			}
			if (state < 4)
			{
				while (state == this.previousState)
				{
					state = Game1.random.Next(6);
				}
			}
			else if (this.previousState >= 4)
			{
				state = this.previousState;
			}
			if (this.isInCrib())
			{
				state = Game1.random.Next(4, 6);
			}
			this.setState(state);
			this.previousState = state;
		}

		// Token: 0x06003626 RID: 13862 RVA: 0x002AAFF5 File Offset: 0x002A91F5
		public override bool hasSpecialCollisionRules()
		{
			return true;
		}

		// Token: 0x06003627 RID: 13863 RVA: 0x002AAFF8 File Offset: 0x002A91F8
		public override bool isColliding(GameLocation l, Vector2 tile)
		{
			return !l.isTilePlaceable(tile, false);
		}

		// Token: 0x06003628 RID: 13864 RVA: 0x002AB008 File Offset: 0x002A9208
		public void tenMinuteUpdate()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (base.Age == 2)
			{
				this.setCrawlerInNewDirection();
				return;
			}
			if (Game1.timeOfDay % 100 == 0 && base.Age == 3 && Game1.timeOfDay < 1900)
			{
				base.IsWalkingInSquare = false;
				this.Halt();
				FarmHouse farmHouse = base.currentLocation as FarmHouse;
				if (farmHouse.characters.Contains(this))
				{
					this.controller = new PathFindController(this, farmHouse, farmHouse.getRandomOpenPointInHouse(Game1.random, 1, 30), -1, new PathFindController.endBehavior(this.toddlerReachedDestination));
					if (this.controller.pathToEndPoint == null || !farmHouse.isTileOnMap(this.controller.pathToEndPoint.Last<Point>()))
					{
						this.controller = null;
						return;
					}
				}
			}
			else if (base.Age == 3 && Game1.timeOfDay == 1900)
			{
				base.IsWalkingInSquare = false;
				this.Halt();
				FarmHouse farmHouse2 = base.currentLocation as FarmHouse;
				if (farmHouse2.characters.Contains(this))
				{
					int child_index = this.GetChildIndex();
					BedFurniture bed = farmHouse2.GetChildBed(child_index);
					Point bed_point = farmHouse2.GetChildBedSpot(child_index);
					if (!bed_point.Equals(Point.Zero))
					{
						this.controller = new PathFindController(this, farmHouse2, bed_point, -1, new PathFindController.endBehavior(this.toddlerReachedDestination));
						if (this.controller.pathToEndPoint == null || !farmHouse2.isTileOnMap(this.controller.pathToEndPoint.Last<Point>()))
						{
							this.controller = null;
							return;
						}
						if (bed != null)
						{
							bed.ReserveForNPC();
						}
					}
				}
			}
		}

		// Token: 0x06003629 RID: 13865 RVA: 0x002AB194 File Offset: 0x002A9394
		public virtual int GetChildIndex()
		{
			Farmer parent = Game1.GetPlayer(this.idOfParent.Value, false);
			if (parent != null)
			{
				List<Child> children = parent.getChildren();
				children.Sort((Child a, Child b) => a.daysOld.Value.CompareTo(b.daysOld.Value));
				children.Reverse();
				return children.IndexOf(this);
			}
			return (int)this.Gender;
		}

		// Token: 0x0600362A RID: 13866 RVA: 0x002AB1F4 File Offset: 0x002A93F4
		public void toddlerReachedDestination(Character c, GameLocation l)
		{
			if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 2)
			{
				List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(16, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(17, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(18, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(19, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(18, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(17, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(16, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(0, 1000, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(16, 100, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(17, 100, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(18, 100, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(19, 100, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(18, 300, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(17, 100, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(16, 100, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(0, 2000, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(16, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(17, 180, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(16, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(0, 800, 0, false, false, null, false, 0)
				};
				this.Sprite.setCurrentAnimation(animation);
				return;
			}
			if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 1)
			{
				List<FarmerSprite.AnimationFrame> animation2 = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(20, 120, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(21, 70, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(22, 70, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(23, 70, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(22, 999999, 0, false, false, null, false, 0)
				};
				this.Sprite.setCurrentAnimation(animation2);
				return;
			}
			if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 3)
			{
				List<FarmerSprite.AnimationFrame> animation3 = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(20, 120, 0, false, true, null, false, 0),
					new FarmerSprite.AnimationFrame(21, 70, 0, false, true, null, false, 0),
					new FarmerSprite.AnimationFrame(22, 70, 0, false, true, null, false, 0),
					new FarmerSprite.AnimationFrame(23, 70, 0, false, true, null, false, 0),
					new FarmerSprite.AnimationFrame(22, 999999, 0, false, true, null, false, 0)
				};
				this.Sprite.setCurrentAnimation(animation3);
				return;
			}
			if (c.FacingDirection == 0)
			{
				this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64);
				this.squareMovementFacingPreference = -1;
				base.walkInSquare(4, 4, 2000);
			}
		}

		// Token: 0x0600362B RID: 13867 RVA: 0x002AB570 File Offset: 0x002A9770
		public override bool canTalk()
		{
			Friendship friendship;
			return Game1.player.friendshipData.TryGetValue(base.Name, out friendship) && !friendship.TalkedToToday;
		}

		// Token: 0x0600362C RID: 13868 RVA: 0x002AB5A4 File Offset: 0x002A97A4
		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (base.IsInvisible)
			{
				return false;
			}
			if (!who.friendshipData.ContainsKey(base.Name))
			{
				who.friendshipData.Add(base.Name, new Friendship(250));
			}
			if (base.Age >= 2 && !who.hasTalkedToFriendToday(base.Name))
			{
				who.talkToFriend(this, 20);
				base.doEmote(20, false);
				if (base.Age == 3)
				{
					base.faceTowardFarmerForPeriod(4000, 3, false, who);
				}
				return true;
			}
			if (Game1.CurrentEvent != null)
			{
				return false;
			}
			if (base.Age >= 3 && who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat)
			{
				if (this.hat.Value != null)
				{
					Game1.createItemDebris(this.hat.Value, base.Position, this.FacingDirection, null, -1, false);
					this.hat.Value = null;
				}
				else
				{
					Hat hatItem = who.Items[who.CurrentToolIndex] as Hat;
					who.Items[who.CurrentToolIndex] = null;
					this.hat.Value = hatItem;
					Game1.playSound("dirtyHit", null);
				}
			}
			return false;
		}

		// Token: 0x0600362D RID: 13869 RVA: 0x002AB70C File Offset: 0x002A990C
		private List<FarmerSprite.AnimationFrame> getRandomCrawlerAnimation(int which = -1)
		{
			List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
			double d = Game1.random.NextDouble();
			if (which == 0 || d < 0.5)
			{
				animation.Add(new FarmerSprite.AnimationFrame(40, 500, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(43, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(40, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(43, 1900, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(40, 500, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(40, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(40, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(40, 1500, 0, false, false, null, false, 0));
			}
			else if (which == 1 || d >= 0.5)
			{
				animation.Add(new FarmerSprite.AnimationFrame(44, 1500, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(45, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(46, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(45, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(46, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(45, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(46, 200, 0, false, false, null, false, 0));
			}
			return animation;
		}

		// Token: 0x0600362E RID: 13870 RVA: 0x002ABA28 File Offset: 0x002A9C28
		private List<FarmerSprite.AnimationFrame> getRandomNewbornAnimation()
		{
			List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
			if (Game1.random.NextBool())
			{
				animation.Add(new FarmerSprite.AnimationFrame(0, 400, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(1, 400, 0, false, false, null, false, 0));
			}
			else
			{
				animation.Add(new FarmerSprite.AnimationFrame(1, 3400, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(2, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(3, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(4, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(5, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(6, 4400, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(5, 3400, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(4, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(3, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(2, 100, 0, false, false, null, false, 0));
			}
			return animation;
		}

		// Token: 0x0600362F RID: 13871 RVA: 0x002ABB4C File Offset: 0x002A9D4C
		private List<FarmerSprite.AnimationFrame> getRandomBabyAnimation()
		{
			List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
			if (Game1.random.NextBool())
			{
				animation.Add(new FarmerSprite.AnimationFrame(4, 120, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(5, 120, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(6, 120, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(7, 120, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(4, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(5, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(6, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(7, 100, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(4, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(5, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(6, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(7, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(4, 2000, 0, false, false, null, false, 0));
				if (Game1.random.NextBool())
				{
					animation.Add(new FarmerSprite.AnimationFrame(8, 1950, 0, false, false, null, false, 0));
					animation.Add(new FarmerSprite.AnimationFrame(9, 1200, 0, false, false, null, false, 0));
					animation.Add(new FarmerSprite.AnimationFrame(10, 180, 0, false, false, null, false, 0));
					animation.Add(new FarmerSprite.AnimationFrame(11, 1500, 0, false, false, null, false, 0));
					animation.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, false, false, null, false, 0));
				}
			}
			else
			{
				animation.Add(new FarmerSprite.AnimationFrame(8, 250, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(9, 250, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(10, 250, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(11, 250, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(8, 1950, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(9, 1200, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(10, 180, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(11, 1500, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(9, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(10, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(11, 150, 0, false, false, null, false, 0));
				animation.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, false, false, null, false, 0));
			}
			return animation;
		}

		// Token: 0x06003630 RID: 13872 RVA: 0x002ABE40 File Offset: 0x002AA040
		public override void update(GameTime time, GameLocation location)
		{
			this.setStateEvent.Poll();
			this.mutex.Update(location);
			base.update(time, location);
			if (base.Age >= 2 && (Game1.IsMasterGame || base.Age < 3))
			{
				this.MovePosition(time, Game1.viewport, location);
			}
			if (this.yJumpVelocity > 18f)
			{
				Utility.addSmokePuff(location, base.Position + new Vector2(32f, (float)this.yJumpOffset), 0, this.yJumpVelocity / 8f, 0.01f, 0.75f, 0.01f);
			}
		}

		// Token: 0x06003631 RID: 13873 RVA: 0x002ABEE0 File Offset: 0x002AA0E0
		public void resetForPlayerEntry(GameLocation l)
		{
			switch (base.Age)
			{
			case 0:
				base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -24f);
				if (Game1.timeOfDay >= 1800 && this.Sprite != null)
				{
					this.Sprite.StopAnimation();
					this.Sprite.currentFrame = Game1.random.Next(7);
				}
				else
				{
					AnimatedSprite sprite = this.Sprite;
					if (sprite != null)
					{
						sprite.setCurrentAnimation(this.getRandomNewbornAnimation());
					}
				}
				break;
			case 1:
				base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -12f);
				if (Game1.timeOfDay >= 1800 && this.Sprite != null)
				{
					this.Sprite.StopAnimation();
					this.Sprite.SpriteHeight = 16;
					this.Sprite.currentFrame = Game1.random.Next(7);
				}
				else if (this.Sprite != null)
				{
					this.Sprite.SpriteHeight = 32;
					this.Sprite.setCurrentAnimation(this.getRandomBabyAnimation());
				}
				break;
			case 2:
				if (this.Sprite != null)
				{
					this.Sprite.SpriteHeight = 16;
				}
				if (Game1.timeOfDay >= 1800)
				{
					base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -24f);
					if (this.Sprite != null)
					{
						this.Sprite.StopAnimation();
						this.Sprite.SpriteHeight = 16;
						this.Sprite.currentFrame = 7;
					}
				}
				break;
			}
			if (this.Sprite != null)
			{
				this.Sprite.loop = true;
			}
			if (this.drawOnTop && !this.mutex.IsLocked())
			{
				this.drawOnTop = false;
			}
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x06003632 RID: 13874 RVA: 0x002AC0FC File Offset: 0x002AA2FC
		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			Microsoft.Xna.Framework.Rectangle cached_source_rect = this.Sprite.SourceRect;
			int cached_sprite_height = this.Sprite.SpriteHeight;
			int cached_y_offset = this.yJumpOffset;
			if (!base.IsInvisible && this.hat.Value != null && this.hat.Value.hairDrawType.Value != 0)
			{
				Microsoft.Xna.Framework.Rectangle source_rect = this.Sprite.SourceRect;
				int new_height = 17;
				switch (this.Sprite.CurrentFrame)
				{
				case 0:
					new_height = 17;
					break;
				case 1:
					new_height = 18;
					break;
				case 2:
					new_height = 17;
					break;
				case 3:
					new_height = 16;
					break;
				case 4:
					new_height = 17;
					break;
				case 5:
					new_height = 18;
					break;
				case 6:
					new_height = 17;
					break;
				case 7:
					new_height = 16;
					break;
				case 8:
					new_height = 17;
					break;
				case 9:
					new_height = 18;
					break;
				case 10:
					new_height = 17;
					break;
				case 11:
					new_height = 16;
					break;
				case 12:
					new_height = 17;
					break;
				case 13:
					new_height = 16;
					break;
				case 14:
					new_height = 17;
					break;
				case 15:
					new_height = 18;
					break;
				case 16:
					new_height = 17;
					break;
				case 17:
					new_height = 17;
					break;
				case 18:
					new_height = 16;
					break;
				case 19:
					new_height = 16;
					break;
				case 20:
					new_height = 17;
					break;
				case 21:
					new_height = 16;
					break;
				case 22:
					new_height = 15;
					break;
				case 23:
					new_height = 14;
					break;
				}
				int height_difference = cached_source_rect.Height - new_height;
				source_rect.Y += cached_source_rect.Height - new_height;
				source_rect.Height = new_height;
				this.Sprite.SourceRect = source_rect;
				this.Sprite.SpriteHeight = new_height;
				this.yJumpOffset = height_difference;
			}
			base.draw(b, 1f);
			this.Sprite.SpriteHeight = cached_sprite_height;
			this.Sprite.SourceRect = cached_source_rect;
			this.yJumpOffset = cached_y_offset;
		}

		// Token: 0x06003633 RID: 13875 RVA: 0x002AC2F0 File Offset: 0x002AA4F0
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (base.IsInvisible)
			{
				return;
			}
			if (base.IsEmoting && !Game1.eventUp)
			{
				Vector2 emotePosition = base.getLocalPosition(Game1.viewport);
				emotePosition.Y -= (float)(32 + this.Sprite.SpriteHeight * 4 - ((base.Age == 1 || base.Age == 3) ? 64 : 0));
				emotePosition.X += (float)((base.Age == 1) ? 8 : 0);
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)base.StandingPixel.Y / 10000f);
			}
			bool draw_hat = true;
			if (this.hat.Value != null)
			{
				Vector2 hatOffset = Vector2.Zero;
				hatOffset *= 4f;
				if (hatOffset.X <= -100f)
				{
					return;
				}
				float horse_draw_layer = (float)base.StandingPixel.Y / 10000f;
				hatOffset.X = -36f;
				hatOffset.Y = -12f;
				if (draw_hat)
				{
					horse_draw_layer += 1E-07f;
					int direction = 2;
					bool flipped = this.sprite.Value.CurrentAnimation != null && this.sprite.Value.CurrentAnimation[this.sprite.Value.currentAnimationIndex].flip;
					switch (this.Sprite.CurrentFrame)
					{
					case 1:
						hatOffset.Y -= 4f;
						direction = 2;
						break;
					case 3:
						hatOffset.Y += 4f;
						direction = 2;
						break;
					case 4:
						direction = 1;
						break;
					case 5:
						hatOffset.Y -= 4f;
						direction = 1;
						break;
					case 6:
						direction = 1;
						break;
					case 7:
						hatOffset.Y += 4f;
						direction = 1;
						break;
					case 8:
						direction = 0;
						break;
					case 9:
						hatOffset.Y -= 4f;
						direction = 0;
						break;
					case 10:
						direction = 0;
						break;
					case 11:
						hatOffset.Y += 4f;
						direction = 0;
						break;
					case 12:
						direction = 3;
						break;
					case 13:
						hatOffset.Y += 4f;
						direction = 3;
						break;
					case 14:
						direction = 3;
						break;
					case 15:
						hatOffset.Y -= 4f;
						direction = 3;
						break;
					case 18:
					case 19:
						hatOffset.Y += 4f;
						direction = 2;
						break;
					case 20:
						direction = 1;
						break;
					case 21:
						hatOffset.Y += 4f;
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? 1 : -1) * 4);
						break;
					case 22:
						hatOffset.Y += 8f;
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? 2 : -2) * 4);
						break;
					case 23:
						hatOffset.Y += 12f;
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? 2 : -2) * 4);
						break;
					}
					if (this.shakeTimer > 0)
					{
						hatOffset += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
					}
					this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f), 1.3333334f, 1f, horse_draw_layer, direction, false);
				}
			}
		}

		// Token: 0x06003634 RID: 13876 RVA: 0x002AC711 File Offset: 0x002AA911
		public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			this.reloadSprite(false);
		}

		// Token: 0x04002375 RID: 9077
		public const int newborn = 0;

		// Token: 0x04002376 RID: 9078
		public const int baby = 1;

		// Token: 0x04002377 RID: 9079
		public const int crawler = 2;

		// Token: 0x04002378 RID: 9080
		public const int toddler = 3;

		// Token: 0x04002379 RID: 9081
		[XmlElement("daysOld")]
		public readonly NetInt daysOld = new NetInt(0);

		// Token: 0x0400237A RID: 9082
		[XmlElement("idOfParent")]
		public NetLong idOfParent = new NetLong(0L);

		// Token: 0x0400237B RID: 9083
		[XmlElement("darkSkinned")]
		public readonly NetBool darkSkinned = new NetBool(false);

		// Token: 0x0400237C RID: 9084
		private readonly NetEvent1Field<int, NetInt> setStateEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x0400237D RID: 9085
		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		// Token: 0x0400237E RID: 9086
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		// Token: 0x0400237F RID: 9087
		private int previousState;
	}
}

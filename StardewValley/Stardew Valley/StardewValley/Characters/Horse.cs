using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Characters
{
	// Token: 0x02000378 RID: 888
	public class Horse : NPC
	{
		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06003637 RID: 13879 RVA: 0x002AC756 File Offset: 0x002AA956
		// (set) Token: 0x06003638 RID: 13880 RVA: 0x002AC763 File Offset: 0x002AA963
		public Guid HorseId
		{
			get
			{
				return this.horseId.Value;
			}
			set
			{
				this.horseId.Value = value;
			}
		}

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x06003639 RID: 13881 RVA: 0x002AC771 File Offset: 0x002AA971
		// (set) Token: 0x0600363A RID: 13882 RVA: 0x002AC77E File Offset: 0x002AA97E
		[XmlIgnore]
		public Farmer rider
		{
			get
			{
				return this.netRider.Value;
			}
			set
			{
				this.netRider.Value = value;
			}
		}

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x0600363B RID: 13883 RVA: 0x002AC78C File Offset: 0x002AA98C
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600363C RID: 13884 RVA: 0x002AC790 File Offset: 0x002AA990
		public Horse()
		{
			base.willDestroyObjectsUnderfoot = false;
			base.HideShadow = true;
			this.drawOffset = new Vector2(-16f, 0f);
			this.onFootstepAction = new Action<string>(this.PerformDefaultHorseFootstep);
			this.ChooseAppearance(null);
			this.faceDirection(3);
			base.Breather = false;
		}

		// Token: 0x0600363D RID: 13885 RVA: 0x002AC83C File Offset: 0x002AAA3C
		public Horse(Guid horseId, int xTile, int yTile) : this()
		{
			base.Name = "";
			this.displayName = base.Name;
			base.Position = new Vector2((float)xTile, (float)yTile) * 64f;
			base.currentLocation = Game1.currentLocation;
			this.HorseId = horseId;
		}

		// Token: 0x0600363E RID: 13886 RVA: 0x002AC891 File Offset: 0x002AAA91
		public override void reloadData()
		{
		}

		// Token: 0x0600363F RID: 13887 RVA: 0x002AC893 File Offset: 0x002AAA93
		protected override string translateName()
		{
			return this.name.Value.Trim();
		}

		// Token: 0x06003640 RID: 13888 RVA: 0x002AC8A5 File Offset: 0x002AAAA5
		public override bool canTalk()
		{
			return false;
		}

		// Token: 0x06003641 RID: 13889 RVA: 0x002AC8A8 File Offset: 0x002AAAA8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.horseId, "horseId").AddField(this.netRider.NetFields, "netRider.NetFields").AddField(this.mounting, "mounting").AddField(this.dismounting, "dismounting").AddField(this.hat, "hat").AddField(this.mutex.NetFields, "mutex.NetFields").AddField(this.ownerId, "ownerId");
			this.position.Field.AxisAlignedMovement = false;
			this.facingDirection.fieldChangeEvent += delegate(NetDirection <p0>, int <p1>, int <p2>)
			{
				base.ClearCachedPosition();
			};
		}

		// Token: 0x06003642 RID: 13890 RVA: 0x002AC964 File Offset: 0x002AAB64
		public Farmer getOwner()
		{
			if (this.ownerId.Value == 0L)
			{
				return null;
			}
			return Game1.GetPlayer(this.ownerId.Value, false);
		}

		// Token: 0x06003643 RID: 13891 RVA: 0x002AC986 File Offset: 0x002AAB86
		public override void reloadSprite(bool onlyAppearance = false)
		{
		}

		// Token: 0x06003644 RID: 13892 RVA: 0x002AC988 File Offset: 0x002AAB88
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite("Animals\\horse", 0, 32, 32);
				this.Sprite.textureUsesFlippedRightForLeft = true;
				this.Sprite.loop = true;
			}
		}

		// Token: 0x06003645 RID: 13893 RVA: 0x002AC9BF File Offset: 0x002AABBF
		public override void dayUpdate(int dayOfMonth)
		{
			this.ateCarrotToday = false;
			this.faceDirection(3);
		}

		// Token: 0x06003646 RID: 13894 RVA: 0x002AC9D0 File Offset: 0x002AABD0
		public override Rectangle GetBoundingBox()
		{
			Rectangle r = base.GetBoundingBox();
			if (this.squeezingThroughGate && (this.FacingDirection == 0 || this.FacingDirection == 2))
			{
				r.Inflate(-36, 0);
			}
			return r;
		}

		// Token: 0x06003647 RID: 13895 RVA: 0x002ACA08 File Offset: 0x002AAC08
		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		// Token: 0x06003648 RID: 13896 RVA: 0x002ACA0B File Offset: 0x002AAC0B
		public void squeezeForGate()
		{
			if (!this.squeezingThroughGate)
			{
				this.squeezingThroughGate = true;
				base.ClearCachedPosition();
			}
			Farmer rider = this.rider;
			if (rider == null)
			{
				return;
			}
			rider.TemporaryPassableTiles.Add(this.GetBoundingBox());
		}

		// Token: 0x06003649 RID: 13897 RVA: 0x002ACA40 File Offset: 0x002AAC40
		public override void update(GameTime time, GameLocation location)
		{
			base.currentLocation = location;
			this.mutex.Update(location);
			if (this.squeezingThroughGate)
			{
				this.squeezingThroughGate = false;
				base.ClearCachedPosition();
			}
			this.faceTowardFarmer = false;
			this.faceTowardFarmerTimer = -1;
			this.Sprite.loop = (this.rider != null && !this.rider.hidden.Value);
			if (this.rider != null && this.rider.hidden.Value)
			{
				return;
			}
			if (this.munchingCarrotTimer > 0)
			{
				this.munchingCarrotTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (this.munchingCarrotTimer <= 0)
				{
					this.mutex.ReleaseLock();
				}
				base.update(time, location);
				return;
			}
			if (this.rider != null && this.rider.isAnimatingMount)
			{
				this.rider.showNotCarrying();
			}
			if (this.mounting.Value)
			{
				if (this.rider == null || !this.rider.IsLocalPlayer)
				{
					return;
				}
				if (this.rider.mount != null)
				{
					this.mounting.Value = false;
					this.rider.isAnimatingMount = false;
					this.rider = null;
					this.Halt();
					this.farmerPassesThrough = false;
					return;
				}
				Rectangle horseBounds = this.GetBoundingBox();
				int anchorX = horseBounds.X + 16;
				if (this.rider.Position.X < (float)(anchorX - 4))
				{
					this.rider.position.X += 4f;
				}
				else if (this.rider.Position.X > (float)(anchorX + 4))
				{
					this.rider.position.X -= 4f;
				}
				int riderStandingY = this.rider.StandingPixel.Y;
				if (riderStandingY < horseBounds.Y - 4)
				{
					this.rider.position.Y += 4f;
				}
				else if (riderStandingY > horseBounds.Y + 4)
				{
					this.rider.position.Y -= 4f;
				}
				if (this.rider.yJumpOffset >= -8 && this.rider.yJumpVelocity <= 0f)
				{
					this.Halt();
					this.Sprite.loop = true;
					base.currentLocation.characters.Remove(this);
					this.rider.mount = this;
					this.rider.freezePause = -1;
					this.mounting.Value = false;
					this.rider.isAnimatingMount = false;
					this.rider.canMove = true;
					if (this.FacingDirection == 1)
					{
						this.rider.xOffset += 8f;
					}
				}
			}
			else if (this.dismounting.Value)
			{
				if (this.rider == null || !this.rider.IsLocalPlayer)
				{
					this.Halt();
					return;
				}
				if (this.rider.isAnimatingMount)
				{
					this.rider.faceDirection(this.FacingDirection);
				}
				Vector2 targetPosition = new Vector2(this.dismountTile.X * 64f + 32f - (float)(this.rider.GetBoundingBox().Width / 2), this.dismountTile.Y * 64f + 4f);
				if (Math.Abs(this.rider.Position.X - targetPosition.X) > 4f)
				{
					if (this.rider.Position.X < targetPosition.X)
					{
						this.rider.position.X += Math.Min(4f, targetPosition.X - this.rider.Position.X);
					}
					else if (this.rider.Position.X > targetPosition.X)
					{
						this.rider.position.X += Math.Max(-4f, targetPosition.X - this.rider.Position.X);
					}
				}
				if (Math.Abs(this.rider.Position.Y - targetPosition.Y) > 4f)
				{
					if (this.rider.Position.Y < targetPosition.Y)
					{
						this.rider.position.Y += Math.Min(4f, targetPosition.Y - this.rider.Position.Y);
					}
					else if (this.rider.Position.Y > targetPosition.Y)
					{
						this.rider.position.Y += Math.Max(-4f, targetPosition.Y - this.rider.Position.Y);
					}
				}
				if (this.rider.yJumpOffset >= 0 && this.rider.yJumpVelocity <= 0f)
				{
					this.rider.position.Y += 8f;
					this.rider.position.X = targetPosition.X;
					int tries = 0;
					while (this.rider.currentLocation.isCollidingPosition(this.rider.GetBoundingBox(), Game1.viewport, true, 0, false, this.rider) && tries < 6)
					{
						tries++;
						this.rider.position.Y -= 4f;
					}
					if (tries == 6)
					{
						this.rider.Position = base.Position;
						this.dismounting.Value = false;
						this.rider.isAnimatingMount = false;
						this.rider.freezePause = -1;
						this.rider.canMove = true;
						return;
					}
					this.dismount(false);
				}
			}
			else if (this.rider == null && this.FacingDirection != 2 && this.Sprite.CurrentAnimation == null && Game1.random.NextDouble() < 0.002)
			{
				this.Sprite.loop = false;
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(25, Game1.random.Next(250, 750)),
						new FarmerSprite.AnimationFrame(14, 10)
					});
					break;
				case 1:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(21, 100),
						new FarmerSprite.AnimationFrame(22, 100),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(22, 100),
						new FarmerSprite.AnimationFrame(21, 100)
					});
					break;
				case 3:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(21, 100, false, true, null, false),
						new FarmerSprite.AnimationFrame(22, 100, false, true, null, false),
						new FarmerSprite.AnimationFrame(23, 100, false, true, null, false),
						new FarmerSprite.AnimationFrame(24, 400, false, true, null, false),
						new FarmerSprite.AnimationFrame(23, 400, false, true, null, false),
						new FarmerSprite.AnimationFrame(24, 400, false, true, null, false),
						new FarmerSprite.AnimationFrame(23, 400, false, true, null, false),
						new FarmerSprite.AnimationFrame(24, 400, false, true, null, false),
						new FarmerSprite.AnimationFrame(23, 400, false, true, null, false),
						new FarmerSprite.AnimationFrame(22, 100, false, true, null, false),
						new FarmerSprite.AnimationFrame(21, 100, false, true, null, false)
					});
					break;
				}
			}
			else if (this.rider != null)
			{
				if (this.FacingDirection != this.rider.FacingDirection || this.ridingAnimationDirection != this.FacingDirection)
				{
					this.Sprite.StopAnimation();
					this.faceDirection(this.rider.FacingDirection);
				}
				bool flag = (this.rider.movementDirections.Any<int>() && this.rider.CanMove) || this.rider.position.Field.IsInterpolating();
				this.SyncPositionToRider();
				if (!flag)
				{
					this.Sprite.StopAnimation();
					this.faceDirection(this.rider.FacingDirection);
				}
				else if (this.Sprite.CurrentAnimation == null)
				{
					switch (this.FacingDirection)
					{
					case 0:
					{
						AnimatedSprite sprite = this.Sprite;
						List<FarmerSprite.AnimationFrame> list;
						(list = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(15, 70),
							new FarmerSprite.AnimationFrame(16, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false)
						}).Add(new FarmerSprite.AnimationFrame(17, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list2;
						(list2 = list).Add(new FarmerSprite.AnimationFrame(18, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list3;
						(list3 = list2).Add(new FarmerSprite.AnimationFrame(19, 70));
						List<FarmerSprite.AnimationFrame> list4 = list3;
						list4.Add(new FarmerSprite.AnimationFrame(20, 70));
						sprite.setCurrentAnimation(list4);
						break;
					}
					case 1:
					{
						AnimatedSprite sprite2 = this.Sprite;
						List<FarmerSprite.AnimationFrame> list5;
						(list5 = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(8, 70),
							new FarmerSprite.AnimationFrame(9, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false)
						}).Add(new FarmerSprite.AnimationFrame(10, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list6;
						(list6 = list5).Add(new FarmerSprite.AnimationFrame(11, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list7;
						(list7 = list6).Add(new FarmerSprite.AnimationFrame(12, 70));
						List<FarmerSprite.AnimationFrame> list8 = list7;
						list8.Add(new FarmerSprite.AnimationFrame(13, 70));
						sprite2.setCurrentAnimation(list8);
						break;
					}
					case 2:
					{
						AnimatedSprite sprite3 = this.Sprite;
						List<FarmerSprite.AnimationFrame> list9;
						(list9 = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(1, 70),
							new FarmerSprite.AnimationFrame(2, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false)
						}).Add(new FarmerSprite.AnimationFrame(3, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list10;
						(list10 = list9).Add(new FarmerSprite.AnimationFrame(4, 70, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list11;
						(list11 = list10).Add(new FarmerSprite.AnimationFrame(5, 70));
						List<FarmerSprite.AnimationFrame> list12 = list11;
						list12.Add(new FarmerSprite.AnimationFrame(6, 70));
						sprite3.setCurrentAnimation(list12);
						break;
					}
					case 3:
					{
						AnimatedSprite sprite4 = this.Sprite;
						List<FarmerSprite.AnimationFrame> list13;
						(list13 = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(8, 70, false, true, null, false),
							new FarmerSprite.AnimationFrame(9, 70, false, true, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false)
						}).Add(new FarmerSprite.AnimationFrame(10, 70, false, true, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list14;
						(list14 = list13).Add(new FarmerSprite.AnimationFrame(11, 70, false, true, new AnimatedSprite.endOfAnimationBehavior(this.OnMountFootstep), false));
						List<FarmerSprite.AnimationFrame> list15;
						(list15 = list14).Add(new FarmerSprite.AnimationFrame(12, 70, false, true, null, false));
						List<FarmerSprite.AnimationFrame> list16 = list15;
						list16.Add(new FarmerSprite.AnimationFrame(13, 70, false, true, null, false));
						sprite4.setCurrentAnimation(list16);
						break;
					}
					}
					this.ridingAnimationDirection = this.FacingDirection;
				}
			}
			if (this.FacingDirection == 3)
			{
				this.drawOffset = Vector2.Zero;
			}
			else
			{
				this.drawOffset = new Vector2(-16f, 0f);
			}
			this.flip = (this.FacingDirection == 3);
			base.update(time, location);
		}

		// Token: 0x0600364A RID: 13898 RVA: 0x002AD660 File Offset: 0x002AB860
		public override void OnLocationRemoved()
		{
			base.OnLocationRemoved();
			GameLocation location;
			Stable stable;
			if (Game1.IsMasterGame && this.TryFindStable(out location, out stable))
			{
				Game1.warpCharacter(this, location, Utility.PointToVector2(stable.GetDefaultHorseTile()));
			}
		}

		// Token: 0x0600364B RID: 13899 RVA: 0x002AD698 File Offset: 0x002AB898
		public virtual void OnMountFootstep(Farmer who)
		{
			if (this.onFootstepAction != null && this.rider != null)
			{
				string step_type = this.rider.currentLocation.doesTileHaveProperty(this.rider.TilePoint.X, this.rider.TilePoint.Y, "Type", "Back", false);
				this.onFootstepAction(step_type);
			}
		}

		// Token: 0x0600364C RID: 13900 RVA: 0x002AD700 File Offset: 0x002AB900
		public virtual void PerformDefaultHorseFootstep(string step_type)
		{
			if (this.rider == null)
			{
				return;
			}
			if (!(step_type == "Stone"))
			{
				if (!(step_type == "Wood"))
				{
					if (this.rider.ShouldHandleAnimationSound())
					{
						this.rider.currentLocation.localSound("thudStep", new Vector2?(base.Tile), null, SoundContext.Default);
					}
					if (this.rider == Game1.player)
					{
						Rumble.rumble(0.3f, 50f);
					}
				}
				else
				{
					if (this.rider.ShouldHandleAnimationSound())
					{
						this.rider.currentLocation.localSound("woodyStep", new Vector2?(base.Tile), null, SoundContext.Default);
					}
					if (this.rider == Game1.player)
					{
						Rumble.rumble(0.1f, 50f);
						return;
					}
				}
			}
			else
			{
				if (this.rider.ShouldHandleAnimationSound())
				{
					this.rider.currentLocation.localSound("stoneStep", new Vector2?(base.Tile), null, SoundContext.Default);
				}
				if (this.rider == Game1.player)
				{
					Rumble.rumble(0.1f, 50f);
					return;
				}
			}
		}

		// Token: 0x0600364D RID: 13901 RVA: 0x002AD834 File Offset: 0x002ABA34
		public void dismount(bool from_demolish = false)
		{
			this.mutex.ReleaseLock();
			this.rider.mount = null;
			if (base.currentLocation == null)
			{
				return;
			}
			if (!from_demolish && this.TryFindStable() != null && !base.currentLocation.characters.Any(delegate(NPC c)
			{
				Horse horse = c as Horse;
				return horse != null && horse.HorseId == this.HorseId;
			}))
			{
				base.currentLocation.characters.Add(this);
			}
			this.SyncPositionToRider();
			this.rider.TemporaryPassableTiles.Add(new Rectangle((int)this.dismountTile.X * 64, (int)this.dismountTile.Y * 64, 64, 64));
			this.rider.freezePause = -1;
			this.dismounting.Value = false;
			this.rider.isAnimatingMount = false;
			this.rider.canMove = true;
			this.rider.forceCanMove();
			this.rider.xOffset = 0f;
			this.rider = null;
			this.Halt();
			this.farmerPassesThrough = false;
		}

		// Token: 0x0600364E RID: 13902 RVA: 0x002AD938 File Offset: 0x002ABB38
		public Stable TryFindStable()
		{
			GameLocation gameLocation;
			Stable stable;
			if (!this.TryFindStable(out gameLocation, out stable))
			{
				return null;
			}
			return stable;
		}

		// Token: 0x0600364F RID: 13903 RVA: 0x002AD954 File Offset: 0x002ABB54
		public bool TryFindStable(out GameLocation location, out Stable stable)
		{
			GameLocation foundLocation = null;
			Stable foundStable = null;
			Utility.ForEachLocation(delegate(GameLocation curLocation)
			{
				foreach (Building building in curLocation.buildings)
				{
					Stable curStable = building as Stable;
					if (curStable != null && curStable.HorseId == this.HorseId && !curStable.isUnderConstruction(true))
					{
						foundLocation = curLocation;
						foundStable = curStable;
						if (curLocation.IsActiveLocation())
						{
							return false;
						}
					}
				}
				return true;
			}, true, false);
			location = foundLocation;
			stable = foundStable;
			return stable != null;
		}

		// Token: 0x06003650 RID: 13904 RVA: 0x002AD9A4 File Offset: 0x002ABBA4
		public void nameHorse(string name)
		{
			if (name.Length > 0)
			{
				Game1.multiplayer.globalChatInfoMessage("HorseNamed", new string[]
				{
					Game1.player.Name,
					name
				});
				Utility.ForEachVillager(delegate(NPC n)
				{
					if (n.Name == name)
					{
						name += " ";
					}
					return true;
				}, false);
				base.Name = name;
				this.displayName = name;
				if (Game1.player.horseName.Value == null)
				{
					Game1.player.horseName.Value = name;
				}
				Game1.exitActiveMenu();
				Game1.playSound("newArtifact", null);
				if (this.mutex.IsLockHeld())
				{
					this.mutex.ReleaseLock();
				}
			}
		}

		// Token: 0x06003651 RID: 13905 RVA: 0x002ADA7C File Offset: 0x002ABC7C
		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (who != null && !who.canMove)
			{
				return false;
			}
			if (this.munchingCarrotTimer > 0)
			{
				return false;
			}
			if (this.rider == null)
			{
				this.mutex.RequestLock(delegate
				{
					if (who.mount != null || this.rider != null || who.FarmerSprite.PauseForSingleAnimation || this.currentLocation != who.currentLocation)
					{
						this.mutex.ReleaseLock();
						return;
					}
					Stable stable = this.TryFindStable();
					if (stable != null)
					{
						if ((this.getOwner() == Game1.player || (this.getOwner() == null && (string.IsNullOrEmpty(Game1.player.horseName.Value) || Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID) == null))) && this.Name.Length <= 0)
						{
							stable.owner.Value = who.UniqueMultiplayerID;
							stable.updateHorseOwnership();
							Utility.ForEachBuilding<Stable>(delegate(Stable curStable)
							{
								if (curStable.owner.Value == who.UniqueMultiplayerID && curStable != stable)
								{
									stable.owner.Value = 0L;
									stable.updateHorseOwnership();
								}
								return true;
							}, true);
							if (string.IsNullOrEmpty(Game1.player.horseName.Value))
							{
								Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(this.nameHorse), Game1.content.LoadString("Strings\\Characters:NameYourHorse"), Game1.content.LoadString("Strings\\Characters:DefaultHorseName"));
								return;
							}
						}
						else
						{
							if (who.Items.Count > who.CurrentToolIndex)
							{
								Hat newHat = who.Items[who.CurrentToolIndex] as Hat;
								if (newHat != null)
								{
									if (this.hat.Value != null)
									{
										Game1.createItemDebris(this.hat.Value, this.Position, this.FacingDirection, null, -1, false);
										this.hat.Value = null;
									}
									else
									{
										who.Items[who.CurrentToolIndex] = null;
										this.hat.Value = newHat;
										Game1.playSound("dirtyHit", null);
									}
									this.mutex.ReleaseLock();
									return;
								}
							}
							if (!this.ateCarrotToday && who.Items.Count > who.CurrentToolIndex)
							{
								Object o = who.Items[who.CurrentToolIndex] as Object;
								if (o != null && o.QualifiedItemId == "(O)Carrot")
								{
									this.Sprite.StopAnimation();
									this.Sprite.faceDirection(this.FacingDirection);
									Game1.playSound("eat", null);
									DelayedAction.playSoundAfterDelay("eat", 600, null, null, -1, false);
									DelayedAction.playSoundAfterDelay("eat", 1200, null, null, -1, false);
									this.munchingCarrotTimer = 1500;
									this.doEmote(20, 32);
									who.reduceActiveItemByOne();
									this.ateCarrotToday = true;
									return;
								}
							}
						}
					}
					this.rider = who;
					this.rider.freezePause = 5000;
					this.rider.synchronizedJump(6f);
					this.rider.Halt();
					if (this.rider.Position.X < this.Position.X)
					{
						this.rider.faceDirection(1);
					}
					l.playSound("dwop", null, null, SoundContext.Default);
					this.mounting.Value = true;
					this.rider.isAnimatingMount = true;
					this.rider.completelyStopAnimatingOrDoingAction();
					this.rider.faceGeneralDirection(Utility.PointToVector2(this.StandingPixel), 0, false, false);
				}, null);
				return true;
			}
			this.dismounting.Value = true;
			this.rider.isAnimatingMount = true;
			this.farmerPassesThrough = false;
			this.rider.TemporaryPassableTiles.Clear();
			Vector2 position = Utility.recursiveFindOpenTileForCharacter(this.rider, this.rider.currentLocation, base.Tile, 8, true);
			base.Position = new Vector2(position.X * 64f + 32f - (float)(this.GetBoundingBox().Width / 2), position.Y * 64f + 4f);
			this.roomForHorseAtDismountTile = !base.currentLocation.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, true, 0, false, this);
			base.Position = this.rider.Position;
			this.dismounting.Value = false;
			this.rider.isAnimatingMount = false;
			this.Halt();
			if (!position.Equals(Vector2.Zero) && Vector2.Distance(position, base.Tile) < 2f)
			{
				this.rider.synchronizedJump(6f);
				l.playSound("dwop", null, null, SoundContext.Default);
				this.rider.freezePause = 5000;
				this.rider.Halt();
				this.rider.xOffset = 0f;
				this.dismounting.Value = true;
				this.rider.isAnimatingMount = true;
				this.dismountTile = position;
			}
			else
			{
				this.dismount(false);
			}
			return true;
		}

		// Token: 0x06003652 RID: 13906 RVA: 0x002ADC71 File Offset: 0x002ABE71
		public void SyncPositionToRider()
		{
			if (this.rider != null && (!this.dismounting.Value || this.roomForHorseAtDismountTile))
			{
				base.Position = this.rider.Position;
			}
		}

		// Token: 0x06003653 RID: 13907 RVA: 0x002ADCA4 File Offset: 0x002ABEA4
		public override void draw(SpriteBatch b)
		{
			this.flip = (this.FacingDirection == 3);
			this.Sprite.UpdateSourceRect();
			base.draw(b);
			if (this.FacingDirection == 2 && this.rider != null)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(48f, -24f - this.rider.yOffset), new Rectangle?(new Rectangle(160, 96, 9, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f);
			}
			bool draw_hat = true;
			if (this.hat.Value != null)
			{
				Vector2 hatOffset = Vector2.Zero;
				string itemId = this.hat.Value.ItemId;
				if (itemId != null)
				{
					int length = itemId.Length;
					if (length != 1)
					{
						if (length != 2)
						{
							goto IL_33C;
						}
						switch (itemId[1])
						{
						case '0':
							if (!(itemId == "10"))
							{
								goto IL_33C;
							}
							hatOffset.Y += 3f;
							if (this.FacingDirection == 0)
							{
								draw_hat = false;
								goto IL_33C;
							}
							goto IL_33C;
						case '1':
							if (itemId == "31")
							{
								hatOffset.Y += 1f;
								goto IL_33C;
							}
							if (!(itemId == "11"))
							{
								goto IL_33C;
							}
							break;
						case '2':
							if (!(itemId == "32"))
							{
								goto IL_33C;
							}
							goto IL_278;
						case '3':
						case '5':
						case '8':
							goto IL_33C;
						case '4':
							if (!(itemId == "14"))
							{
								goto IL_33C;
							}
							if (this.FacingDirection == 0)
							{
								hatOffset.X = -100f;
								goto IL_33C;
							}
							goto IL_33C;
						case '6':
							if (!(itemId == "26"))
							{
								if (!(itemId == "56"))
								{
									goto IL_33C;
								}
								goto IL_332;
							}
							else
							{
								if (this.FacingDirection != 3 && this.FacingDirection != 1)
								{
									goto IL_33C;
								}
								if (this.flip)
								{
									hatOffset.X += 1f;
									goto IL_33C;
								}
								hatOffset.X -= 1f;
								goto IL_33C;
							}
							break;
						case '7':
							if (!(itemId == "67"))
							{
								goto IL_33C;
							}
							goto IL_332;
						case '9':
							if (!(itemId == "39"))
							{
								goto IL_33C;
							}
							break;
						default:
							goto IL_33C;
						}
						if (this.FacingDirection != 3 && this.FacingDirection != 1)
						{
							goto IL_33C;
						}
						if (this.flip)
						{
							hatOffset.X += 2f;
							goto IL_33C;
						}
						hatOffset.X -= 2f;
						goto IL_33C;
						IL_332:
						if (this.FacingDirection == 0)
						{
							draw_hat = false;
							goto IL_33C;
						}
						goto IL_33C;
					}
					else
					{
						char c = itemId[0];
						if (c != '6')
						{
							if (c != '9')
							{
								goto IL_33C;
							}
						}
						else
						{
							hatOffset.Y += 2f;
							if (this.FacingDirection == 2)
							{
								hatOffset.Y -= 1f;
								goto IL_33C;
							}
							goto IL_33C;
						}
					}
					IL_278:
					if (this.FacingDirection == 0 || this.FacingDirection == 2)
					{
						hatOffset.Y += 1f;
					}
				}
				IL_33C:
				hatOffset *= 4f;
				if (this.shakeTimer > 0)
				{
					hatOffset += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				}
				if (hatOffset.X <= -100f)
				{
					return;
				}
				float horse_draw_layer = (float)base.StandingPixel.Y / 10000f;
				if (this.rider != null)
				{
					if (this.FacingDirection == 2)
					{
						horse_draw_layer = (this.position.Y + 64f + 1f) / 10000f;
					}
					else if (this.FacingDirection != 0)
					{
						horse_draw_layer = (this.position.Y + 48f - 1f) / 10000f;
					}
				}
				if (this.munchingCarrotTimer > 0)
				{
					switch (this.FacingDirection)
					{
					case 1:
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), new Rectangle?(new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, horse_draw_layer + 1E-07f);
						break;
					case 2:
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), new Rectangle?(new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, horse_draw_layer + 1E-07f);
						break;
					case 3:
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), new Rectangle?(new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, horse_draw_layer + 1E-07f);
						break;
					}
				}
				if (draw_hat)
				{
					horse_draw_layer += 2E-07f;
					switch (this.Sprite.CurrentFrame)
					{
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f - ((this.rider != null) ? this.rider.yOffset : 0f))), 1.3333334f, 1f, horse_draw_layer, 2, false);
						return;
					case 7:
					case 11:
						if (this.flip)
						{
							this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -74f), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -74f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 8:
						if (this.flip)
						{
							this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -74f), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -74f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 9:
						if (this.flip)
						{
							this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -70f), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -70f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 10:
						if (this.flip)
						{
							this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -70f), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -70f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 12:
						if (this.flip)
						{
							this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -78f)), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -78f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 13:
						if (this.flip)
						{
							this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -78f)), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -78f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 14:
					case 15:
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
					case 25:
						this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(28f, -106f - ((this.rider != null) ? this.rider.yOffset : 0f)), 1.3333334f, 1f, horse_draw_layer, 0, false);
						return;
					case 21:
						if (this.flip)
						{
							this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -66f)), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -66f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 22:
						if (this.flip)
						{
							this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -54f)), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -54f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 23:
						if (this.flip)
						{
							this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					case 24:
						if (this.flip)
						{
							this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.3333334f, 1f, horse_draw_layer, 3, false);
							return;
						}
						this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.3333334f, 1f, horse_draw_layer, 1, false);
						return;
					default:
						return;
					}
				}
			}
			else if (this.munchingCarrotTimer > 0)
			{
				switch (this.FacingDirection)
				{
				case 1:
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), new Rectangle?(new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f + 1E-07f);
					return;
				case 2:
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), new Rectangle?(new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f + 1E-07f);
					return;
				case 3:
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), new Rectangle?(new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (base.Position.Y + 64f) / 10000f + 1E-07f);
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x04002380 RID: 9088
		private readonly NetGuid horseId = new NetGuid();

		// Token: 0x04002381 RID: 9089
		private readonly NetFarmerRef netRider = new NetFarmerRef();

		// Token: 0x04002382 RID: 9090
		public readonly NetLong ownerId = new NetLong();

		// Token: 0x04002383 RID: 9091
		[XmlIgnore]
		public readonly NetBool mounting = new NetBool();

		// Token: 0x04002384 RID: 9092
		[XmlIgnore]
		public readonly NetBool dismounting = new NetBool();

		// Token: 0x04002385 RID: 9093
		private Vector2 dismountTile;

		// Token: 0x04002386 RID: 9094
		private int ridingAnimationDirection;

		// Token: 0x04002387 RID: 9095
		private bool roomForHorseAtDismountTile;

		// Token: 0x04002388 RID: 9096
		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		// Token: 0x04002389 RID: 9097
		public readonly NetMutex mutex = new NetMutex();

		// Token: 0x0400238A RID: 9098
		[XmlIgnore]
		public Action<string> onFootstepAction;

		// Token: 0x0400238B RID: 9099
		[XmlIgnore]
		public bool ateCarrotToday;

		// Token: 0x0400238C RID: 9100
		private bool squeezingThroughGate;

		// Token: 0x0400238D RID: 9101
		private int munchingCarrotTimer;
	}
}

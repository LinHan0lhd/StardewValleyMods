using System;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley
{
	// Token: 0x02000088 RID: 136
	[InstanceStatics]
	[XmlInclude(typeof(FarmAnimal))]
	[XmlInclude(typeof(Farmer))]
	[XmlInclude(typeof(NPC))]
	[NotImplicitNetField]
	public class Character : INetObject<NetFields>, IHaveModData
	{
		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x0001A80A File Offset: 0x00018A0A
		// (set) Token: 0x06000502 RID: 1282 RVA: 0x0001A812 File Offset: 0x00018A12
		public virtual Gender Gender { get; set; } = Gender.Undefined;

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x06000503 RID: 1283 RVA: 0x0001A81B File Offset: 0x00018A1B
		// (set) Token: 0x06000504 RID: 1284 RVA: 0x0001A828 File Offset: 0x00018A28
		[XmlIgnore]
		public int speed
		{
			get
			{
				return this.netSpeed.Value;
			}
			set
			{
				this.netSpeed.Value = value;
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x06000505 RID: 1285 RVA: 0x0001A836 File Offset: 0x00018A36
		// (set) Token: 0x06000506 RID: 1286 RVA: 0x0001A843 File Offset: 0x00018A43
		[XmlIgnore]
		public virtual float addedSpeed
		{
			get
			{
				return this.netAddedSpeed.Value;
			}
			set
			{
				this.netAddedSpeed.Value = value;
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x06000507 RID: 1287 RVA: 0x0001A854 File Offset: 0x00018A54
		// (set) Token: 0x06000508 RID: 1288 RVA: 0x0001A87A File Offset: 0x00018A7A
		[XmlIgnore]
		public virtual string displayName
		{
			get
			{
				string result;
				if ((result = this._displayName) == null)
				{
					result = (this._displayName = this.translateName());
				}
				return result;
			}
			set
			{
				this._displayName = value;
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000509 RID: 1289 RVA: 0x0001A883 File Offset: 0x00018A83
		// (set) Token: 0x0600050A RID: 1290 RVA: 0x0001A890 File Offset: 0x00018A90
		[XmlIgnore]
		public virtual bool EventActor
		{
			get
			{
				return this.netEventActor.Value;
			}
			set
			{
				this.netEventActor.Value = value;
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x0600050B RID: 1291 RVA: 0x0001A89E File Offset: 0x00018A9E
		// (set) Token: 0x0600050C RID: 1292 RVA: 0x0001A8AB File Offset: 0x00018AAB
		public bool willDestroyObjectsUnderfoot
		{
			get
			{
				return this._willDestroyObjectsUnderfoot.Value;
			}
			set
			{
				this._willDestroyObjectsUnderfoot.Value = value;
			}
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x0600050D RID: 1293 RVA: 0x0001A8B9 File Offset: 0x00018AB9
		// (set) Token: 0x0600050E RID: 1294 RVA: 0x0001A8C6 File Offset: 0x00018AC6
		public Vector2 Position
		{
			get
			{
				return this.position.Value;
			}
			set
			{
				if (this.position.Value != value)
				{
					this.position.Set(value);
				}
			}
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x0600050F RID: 1295 RVA: 0x0001A8E8 File Offset: 0x00018AE8
		public Point StandingPixel
		{
			get
			{
				if (this.position.X != this.pixelPositionForCachedStandingPixel.X || this.position.Y != this.pixelPositionForCachedStandingPixel.Y)
				{
					this.cachedStandingPixel = this.GetBoundingBox().Center;
					this.pixelPositionForCachedStandingPixel = this.position.Value;
				}
				return this.cachedStandingPixel;
			}
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000510 RID: 1296 RVA: 0x0001A950 File Offset: 0x00018B50
		public Vector2 Tile
		{
			get
			{
				if (this.position.X != this.pixelPositionForCachedTile.X || this.position.Y != this.pixelPositionForCachedTile.Y)
				{
					Point pixel = this.StandingPixel;
					this.cachedTile = new Vector2((float)(pixel.X / 64), (float)(pixel.Y / 64));
					this.pixelPositionForCachedTile = this.position.Value;
				}
				return this.cachedTile;
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000511 RID: 1297 RVA: 0x0001A9CC File Offset: 0x00018BCC
		public Point TilePoint
		{
			get
			{
				if (this.position.X != this.pixelPositionForCachedTilePoint.X || this.position.Y != this.pixelPositionForCachedTilePoint.Y)
				{
					Vector2 tile = this.Tile;
					this.cachedTilePoint = new Point((int)tile.X, (int)tile.Y);
					this.pixelPositionForCachedTilePoint = this.position.Value;
				}
				return this.cachedTilePoint;
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000512 RID: 1298 RVA: 0x0001AA40 File Offset: 0x00018C40
		// (set) Token: 0x06000513 RID: 1299 RVA: 0x0001AA48 File Offset: 0x00018C48
		public int Speed
		{
			get
			{
				return this.speed;
			}
			set
			{
				this.speed = value;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000514 RID: 1300 RVA: 0x0001AA51 File Offset: 0x00018C51
		// (set) Token: 0x06000515 RID: 1301 RVA: 0x0001AA5E File Offset: 0x00018C5E
		public virtual int FacingDirection
		{
			get
			{
				return this.facingDirection.Value;
			}
			set
			{
				this.facingDirection.Set(value);
			}
		}

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000516 RID: 1302 RVA: 0x0001AA6C File Offset: 0x00018C6C
		// (set) Token: 0x06000517 RID: 1303 RVA: 0x0001AA79 File Offset: 0x00018C79
		[XmlIgnore]
		public string Name
		{
			get
			{
				return this.name.Value;
			}
			set
			{
				this.name.Set(value);
			}
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000518 RID: 1304 RVA: 0x0001AA87 File Offset: 0x00018C87
		// (set) Token: 0x06000519 RID: 1305 RVA: 0x0001AA94 File Offset: 0x00018C94
		[XmlIgnore]
		public bool SimpleNonVillagerNPC
		{
			get
			{
				return this.simpleNonVillagerNPC.Value;
			}
			set
			{
				this.simpleNonVillagerNPC.Set(value);
			}
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x0600051A RID: 1306 RVA: 0x0001AAA2 File Offset: 0x00018CA2
		// (set) Token: 0x0600051B RID: 1307 RVA: 0x0001AAAF File Offset: 0x00018CAF
		[XmlIgnore]
		public virtual AnimatedSprite Sprite
		{
			get
			{
				return this.sprite.Value;
			}
			set
			{
				this.sprite.Value = value;
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x0600051C RID: 1308 RVA: 0x0001AABD File Offset: 0x00018CBD
		// (set) Token: 0x0600051D RID: 1309 RVA: 0x0001AAC5 File Offset: 0x00018CC5
		public bool IsEmoting
		{
			get
			{
				return this.isEmoting;
			}
			set
			{
				this.isEmoting = value;
			}
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x0600051E RID: 1310 RVA: 0x0001AACE File Offset: 0x00018CCE
		// (set) Token: 0x0600051F RID: 1311 RVA: 0x0001AAD6 File Offset: 0x00018CD6
		public int CurrentEmote
		{
			get
			{
				return this.currentEmote;
			}
			set
			{
				this.currentEmote = value;
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000520 RID: 1312 RVA: 0x0001AADF File Offset: 0x00018CDF
		public int CurrentEmoteIndex
		{
			get
			{
				return this.currentEmoteFrame;
			}
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000521 RID: 1313 RVA: 0x0001AAE7 File Offset: 0x00018CE7
		public virtual bool IsMonster
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000522 RID: 1314 RVA: 0x0001AAEA File Offset: 0x00018CEA
		[XmlIgnore]
		public virtual bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000523 RID: 1315 RVA: 0x0001AAED File Offset: 0x00018CED
		// (set) Token: 0x06000524 RID: 1316 RVA: 0x0001AAFA File Offset: 0x00018CFA
		public float Scale
		{
			get
			{
				return this.scale.Value;
			}
			set
			{
				this.scale.Value = value;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000525 RID: 1317 RVA: 0x0001AB08 File Offset: 0x00018D08
		// (set) Token: 0x06000526 RID: 1318 RVA: 0x0001AB15 File Offset: 0x00018D15
		[XmlIgnore]
		public GameLocation currentLocation
		{
			get
			{
				return this.currentLocationRef.Value;
			}
			set
			{
				this.currentLocationRef.Value = value;
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000527 RID: 1319 RVA: 0x0001AB23 File Offset: 0x00018D23
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000528 RID: 1320 RVA: 0x0001AB2B File Offset: 0x00018D2B
		// (set) Token: 0x06000529 RID: 1321 RVA: 0x0001AB38 File Offset: 0x00018D38
		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return this.modData.GetForSerialization();
			}
			set
			{
				this.modData.SetFromSerialization(value);
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x0600052A RID: 1322 RVA: 0x0001AB46 File Offset: 0x00018D46
		public NetFields NetFields { get; }

		// Token: 0x0600052B RID: 1323 RVA: 0x0001AB50 File Offset: 0x00018D50
		public Character()
		{
			this.NetFields = new NetFields(NetFields.GetNameForInstance<Character>(this));
			this.initNetFields();
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x0001AC94 File Offset: 0x00018E94
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.sprite, "sprite").AddField(this.position.NetFields, "position.NetFields").AddField(this.facingDirection, "facingDirection").AddField(this.netSpeed, "netSpeed").AddField(this.netAddedSpeed, "netAddedSpeed").AddField(this.name, "name").AddField(this.scale, "scale").AddField(this.currentLocationRef.NetFields, "currentLocationRef.NetFields").AddField(this.swimming, "swimming").AddField(this.collidesWithOtherCharacters, "collidesWithOtherCharacters").AddField(this.facingDirectionBeforeSpeakingToPlayer, "facingDirectionBeforeSpeakingToPlayer").AddField(this.faceTowardFarmerRadius, "faceTowardFarmerRadius").AddField(this.faceAwayFromFarmer, "faceAwayFromFarmer").AddField(this.whoToFace.NetFields, "whoToFace.NetFields").AddField(this.faceTowardFarmerEvent, "faceTowardFarmerEvent").AddField(this._willDestroyObjectsUnderfoot, "_willDestroyObjectsUnderfoot").AddField(this.forceOneTileWide, "forceOneTileWide").AddField(this.simpleNonVillagerNPC, "simpleNonVillagerNPC").AddField(this.hideFromAnimalSocialMenu, "hideFromAnimalSocialMenu").AddField(this.netEventActor, "netEventActor").AddField(this.modData, "modData");
			this.facingDirection.Position = this.position;
			this.faceTowardFarmerEvent.onEvent += this.performFaceTowardFarmerEvent;
			this.sprite.fieldChangeEvent += delegate(NetRef<AnimatedSprite> field, AnimatedSprite value, AnimatedSprite newValue)
			{
				if (newValue != null)
				{
					newValue.SetOwner(this);
				}
				this.ClearCachedPosition();
			};
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x0001AE4C File Offset: 0x0001904C
		public Character(AnimatedSprite sprite, Vector2 position, int speed, string name) : this()
		{
			this.Sprite = sprite;
			this.Position = position;
			this.speed = speed;
			this.Name = name;
			if (sprite != null)
			{
				this.originalSourceRect = sprite.SourceRect;
			}
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0001AE80 File Offset: 0x00019080
		protected virtual string translateName()
		{
			return this.name.Value;
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x0001AE8D File Offset: 0x0001908D
		internal void ClearCachedPosition()
		{
			this.pixelPositionForCachedStandingPixel = Character.ClearPositionValue;
			this.pixelPositionForCachedTile = Character.ClearPositionValue;
			this.pixelPositionForCachedTilePoint = Character.ClearPositionValue;
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x0001AEB0 File Offset: 0x000190B0
		protected void resetCachedDisplayName()
		{
			this._displayName = null;
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x0001AEB9 File Offset: 0x000190B9
		public virtual void SetMovingUp(bool b)
		{
			this.moveUp = b;
			if (!b)
			{
				this.Halt();
			}
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x0001AECB File Offset: 0x000190CB
		public virtual void SetMovingRight(bool b)
		{
			this.moveRight = b;
			if (!b)
			{
				this.Halt();
			}
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0001AEDD File Offset: 0x000190DD
		public virtual void SetMovingDown(bool b)
		{
			this.moveDown = b;
			if (!b)
			{
				this.Halt();
			}
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x0001AEEF File Offset: 0x000190EF
		public virtual void SetMovingLeft(bool b)
		{
			this.moveLeft = b;
			if (!b)
			{
				this.Halt();
			}
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x0001AF04 File Offset: 0x00019104
		public void setMovingInFacingDirection()
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.SetMovingUp(true);
				return;
			case 1:
				this.SetMovingRight(true);
				return;
			case 2:
				this.SetMovingDown(true);
				return;
			case 3:
				this.SetMovingLeft(true);
				return;
			default:
				return;
			}
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x0001AF4E File Offset: 0x0001914E
		public int getFacingDirection()
		{
			if (this.Sprite.currentFrame < 4)
			{
				return 2;
			}
			if (this.Sprite.currentFrame < 8)
			{
				return 1;
			}
			if (this.Sprite.currentFrame < 12)
			{
				return 0;
			}
			return 3;
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x0001AF82 File Offset: 0x00019182
		public void setTrajectory(int xVelocity, int yVelocity)
		{
			this.setTrajectory(new Vector2((float)xVelocity, (float)yVelocity));
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x0001AF93 File Offset: 0x00019193
		public virtual void setTrajectory(Vector2 trajectory)
		{
			this.xVelocity = trajectory.X;
			this.yVelocity = trajectory.Y;
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x0001AFAD File Offset: 0x000191AD
		public virtual void Halt()
		{
			this.moveUp = false;
			this.moveDown = false;
			this.moveRight = false;
			this.moveLeft = false;
			this.Sprite.StopAnimation();
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x0001AFD8 File Offset: 0x000191D8
		public void extendSourceRect(int horizontal, int vertical, bool ignoreSourceRectUpdates = true)
		{
			this.Sprite.sourceRect.Inflate(Math.Abs(horizontal) / 2, Math.Abs(vertical) / 2);
			this.Sprite.sourceRect.Offset(horizontal / 2, vertical / 2);
			Microsoft.Xna.Framework.Rectangle rectangle = this.originalSourceRect;
			if (this.Sprite.SourceRect.Equals(this.originalSourceRect))
			{
				this.Sprite.ignoreSourceRectUpdates = false;
				return;
			}
			this.Sprite.ignoreSourceRectUpdates = ignoreSourceRectUpdates;
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x0001B056 File Offset: 0x00019256
		public virtual bool collideWith(Object o)
		{
			return true;
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x0001B059 File Offset: 0x00019259
		public virtual void faceDirection(int direction)
		{
			if (this.SimpleNonVillagerNPC)
			{
				return;
			}
			if (direction != -3)
			{
				this.FacingDirection = direction;
				AnimatedSprite animatedSprite = this.Sprite;
				if (animatedSprite != null)
				{
					animatedSprite.faceDirection(direction);
				}
				this.faceTowardFarmer = false;
				return;
			}
			this.faceTowardFarmer = true;
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x0001B091 File Offset: 0x00019291
		public int getDirection()
		{
			if (this.moveUp)
			{
				return 0;
			}
			if (this.moveRight)
			{
				return 1;
			}
			if (this.moveDown)
			{
				return 2;
			}
			if (this.moveLeft)
			{
				return 3;
			}
			if (this.IsRemoteMoving())
			{
				return this.FacingDirection;
			}
			return -1;
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x0001B0CB File Offset: 0x000192CB
		public bool IsRemoteMoving()
		{
			if (LocalMultiplayer.IsLocalMultiplayer(true))
			{
				return this.position.moving.Value || this.position.Field.IsInterpolating();
			}
			return this.position.Field.IsInterpolating();
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x0001B10C File Offset: 0x0001930C
		public void tryToMoveInDirection(int direction, bool isFarmer, int damagesFarmer, bool glider)
		{
			if (this.currentLocation.isCollidingPosition(this.nextPosition(direction), Game1.viewport, isFarmer, damagesFarmer, glider, this))
			{
				return;
			}
			switch (direction)
			{
			case 0:
				this.position.Y -= (float)this.speed + this.addedSpeed;
				return;
			case 1:
				this.position.X += (float)this.speed + this.addedSpeed;
				return;
			case 2:
				this.position.Y += (float)this.speed + this.addedSpeed;
				return;
			case 3:
				this.position.X -= (float)this.speed + this.addedSpeed;
				return;
			default:
				return;
			}
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x0001B1D5 File Offset: 0x000193D5
		public virtual Vector2 GetShadowOffset()
		{
			if (this.shouldShadowBeOffset)
			{
				return this.drawOffset;
			}
			return Vector2.Zero;
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x0001B1EB File Offset: 0x000193EB
		public virtual bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return this.controller == null && !this.IsMonster;
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x0001B200 File Offset: 0x00019400
		protected void applyVelocity(GameLocation currentLocation)
		{
			Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
			nextPosition.X += (int)this.xVelocity;
			nextPosition.Y -= (int)this.yVelocity;
			if (currentLocation == null || !currentLocation.isCollidingPosition(nextPosition, Game1.viewport, false, 0, false, this))
			{
				this.position.X += this.xVelocity;
				this.position.Y -= this.yVelocity;
			}
			this.xVelocity = (float)((int)(this.xVelocity - this.xVelocity / 2f));
			this.yVelocity = (float)((int)(this.yVelocity - this.yVelocity / 2f));
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x0001B2B4 File Offset: 0x000194B4
		public virtual void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (this is FarmAnimal)
			{
				this.willDestroyObjectsUnderfoot = false;
			}
			bool flag;
			if (this.willDestroyObjectsUnderfoot)
			{
				PathFindController pathFindController = this.controller;
				flag = (pathFindController == null || !pathFindController.nonDestructivePathing);
			}
			else
			{
				flag = false;
			}
			bool shouldDestroyUnderfootObjects = flag;
			if (this.xVelocity != 0f || this.yVelocity != 0f)
			{
				this.applyVelocity(currentLocation);
			}
			else if (this.moveUp)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, 0, false, this) || this.isCharging)
				{
					this.position.Y -= (float)this.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimation)
					{
						this.Sprite.AnimateUp(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
						this.faceDirection(0);
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !shouldDestroyUnderfootObjects)
				{
					this.Halt();
				}
				else if (shouldDestroyUnderfootObjects)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(0), true))
					{
						this.doEmote(12, true);
						this.position.Y -= (float)this.speed + this.addedSpeed;
					}
					else
					{
						this.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else if (this.moveRight)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, 0, false, this) || this.isCharging)
				{
					this.position.X += (float)this.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimation)
					{
						this.Sprite.AnimateRight(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
						this.faceDirection(1);
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !shouldDestroyUnderfootObjects)
				{
					this.Halt();
				}
				else if (shouldDestroyUnderfootObjects)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(1), true))
					{
						this.doEmote(12, true);
						this.position.X += (float)this.speed + this.addedSpeed;
					}
					else
					{
						this.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else if (this.moveDown)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, 0, false, this) || this.isCharging)
				{
					this.position.Y += (float)this.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimation)
					{
						this.Sprite.AnimateDown(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
						this.faceDirection(2);
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !shouldDestroyUnderfootObjects)
				{
					this.Halt();
				}
				else if (shouldDestroyUnderfootObjects)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(2), true))
					{
						this.doEmote(12, true);
						this.position.Y += (float)this.speed + this.addedSpeed;
					}
					else
					{
						this.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else if (this.moveLeft)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, 0, false, this) || this.isCharging)
				{
					this.position.X -= (float)this.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimation)
					{
						this.Sprite.AnimateLeft(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
						this.faceDirection(3);
					}
				}
				else if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !shouldDestroyUnderfootObjects)
				{
					this.Halt();
				}
				else if (shouldDestroyUnderfootObjects)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(3), true))
					{
						this.doEmote(12, true);
						this.position.X -= (float)this.speed + this.addedSpeed;
					}
					else
					{
						this.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else
			{
				this.Sprite.animateOnce(time);
			}
			if (shouldDestroyUnderfootObjects && currentLocation != null && this.isMoving())
			{
				currentLocation.characterTrampleTile(this.Tile);
			}
			if (this.blockedInterval >= 3000 && (float)this.blockedInterval <= 3750f && !Game1.eventUp)
			{
				this.doEmote(Game1.random.Choose(8, 40), true);
				this.blockedInterval = 3750;
				return;
			}
			if (this.blockedInterval >= 5000)
			{
				this.speed = 4;
				this.isCharging = true;
				this.blockedInterval = 0;
			}
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x0001B80A File Offset: 0x00019A0A
		public virtual bool canPassThroughActionTiles()
		{
			return false;
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x0001B810 File Offset: 0x00019A10
		public virtual Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
		{
			Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
			switch (direction)
			{
			case 0:
				nextPosition.Y -= this.speed + (int)this.addedSpeed;
				break;
			case 1:
				nextPosition.X += this.speed + (int)this.addedSpeed;
				break;
			case 2:
				nextPosition.Y += this.speed + (int)this.addedSpeed;
				break;
			case 3:
				nextPosition.X -= this.speed + (int)this.addedSpeed;
				break;
			}
			return nextPosition;
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x0001B8A8 File Offset: 0x00019AA8
		public Location nextPositionPoint()
		{
			Location nextPositionTile = default(Location);
			Point standingPixel = this.StandingPixel;
			switch (this.getDirection())
			{
			case 0:
				nextPositionTile = new Location(standingPixel.X, standingPixel.Y - 64);
				break;
			case 1:
				nextPositionTile = new Location(standingPixel.X + 64, standingPixel.Y);
				break;
			case 2:
				nextPositionTile = new Location(standingPixel.X, standingPixel.Y + 64);
				break;
			case 3:
				nextPositionTile = new Location(standingPixel.X - 64, standingPixel.Y);
				break;
			}
			return nextPositionTile;
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x0001B942 File Offset: 0x00019B42
		public int getHorizontalMovement()
		{
			if (this.moveRight)
			{
				return this.speed + (int)this.addedSpeed;
			}
			if (!this.moveLeft)
			{
				return 0;
			}
			return -this.speed - (int)this.addedSpeed;
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0001B974 File Offset: 0x00019B74
		public int getVerticalMovement()
		{
			if (this.moveDown)
			{
				return this.speed + (int)this.addedSpeed;
			}
			if (!this.moveUp)
			{
				return 0;
			}
			return -this.speed - (int)this.addedSpeed;
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x0001B9A8 File Offset: 0x00019BA8
		public Vector2 nextPositionVector2()
		{
			Point standingPixel = this.StandingPixel;
			return new Vector2((float)(standingPixel.X + this.getHorizontalMovement()), (float)(standingPixel.Y + this.getVerticalMovement()));
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0001B9E0 File Offset: 0x00019BE0
		public Location nextPositionTile()
		{
			Location nextPositionTile = this.nextPositionPoint();
			nextPositionTile.X /= 64;
			nextPositionTile.Y /= 64;
			return nextPositionTile;
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0001BA10 File Offset: 0x00019C10
		public virtual void doEmote(int whichEmote, bool playSound, bool nextEventCommand = true)
		{
			if (!this.isEmoting && (!Game1.eventUp || this is Farmer || (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.actors.Contains(this))))
			{
				this.emoteYOffset = 0;
				this.isEmoting = true;
				this.currentEmote = whichEmote;
				this.currentEmoteFrame = 0;
				this.emoteInterval = 0f;
				this.nextEventcommandAfterEmote = nextEventCommand;
			}
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0001BA85 File Offset: 0x00019C85
		public void doEmote(int whichEmote, bool nextEventCommand = true)
		{
			this.doEmote(whichEmote, true, nextEventCommand);
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0001BA90 File Offset: 0x00019C90
		public void doEmote(int whichEmote, int emoteYOffset)
		{
			this.doEmote(whichEmote, true, false);
			this.emoteYOffset = emoteYOffset;
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0001BAA4 File Offset: 0x00019CA4
		public void updateEmote(GameTime time)
		{
			if (this.isEmoting)
			{
				this.emoteInterval += (float)time.ElapsedGameTime.Milliseconds;
				if (this.emoteFading && this.emoteInterval > 20f)
				{
					this.emoteInterval = 0f;
					this.currentEmoteFrame--;
					if (this.currentEmoteFrame < 0)
					{
						this.emoteFading = false;
						this.isEmoting = false;
						if (this.nextEventcommandAfterEmote && Game1.currentLocation.currentEvent != null && (Game1.currentLocation.currentEvent.actors.Contains(this) || Game1.currentLocation.currentEvent.farmerActors.Contains(this) || this.Name.Equals(Game1.player.Name)))
						{
							Event currentEvent = Game1.currentLocation.currentEvent;
							int currentCommand = currentEvent.CurrentCommand;
							currentEvent.CurrentCommand = currentCommand + 1;
							return;
						}
					}
				}
				else if (!this.emoteFading && this.emoteInterval > 20f && this.currentEmoteFrame <= 3)
				{
					this.emoteInterval = 0f;
					this.currentEmoteFrame++;
					if (this.currentEmoteFrame == 4)
					{
						this.currentEmoteFrame = this.currentEmote;
						return;
					}
				}
				else if (!this.emoteFading && this.emoteInterval > 250f)
				{
					this.emoteInterval = 0f;
					this.currentEmoteFrame++;
					if (this.currentEmoteFrame >= this.currentEmote + 4)
					{
						this.emoteFading = true;
						this.currentEmoteFrame = 3;
					}
				}
			}
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0001BC3C File Offset: 0x00019E3C
		public void playNearbySoundLocal(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
		{
			if (this.currentLocation == null)
			{
				Farmer farmer = this as Farmer;
				if (farmer == null || !farmer.IsLocalPlayer)
				{
					return;
				}
			}
			ICue cue;
			Game1.sounds.PlayLocal(audioName, this.currentLocation, new Vector2?(this.Tile), pitch, context, out cue);
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0001BC8C File Offset: 0x00019E8C
		public void playNearbySoundAll(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
		{
			if (this.currentLocation != null)
			{
				Game1.sounds.PlayAll(audioName, this.currentLocation, new Vector2?(this.Tile), pitch, context);
				return;
			}
			Farmer farmer = this as Farmer;
			if (farmer == null || !farmer.IsLocalPlayer)
			{
				return;
			}
			ICue cue;
			Game1.sounds.PlayLocal(audioName, null, null, pitch, context, out cue);
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0001BCF4 File Offset: 0x00019EF4
		public Vector2 GetGrabTile()
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
			switch (this.FacingDirection)
			{
			case 0:
				return new Vector2((float)((boundingBox.X + boundingBox.Width / 2) / 64), (float)((boundingBox.Y - 5) / 64));
			case 1:
				return new Vector2((float)((boundingBox.X + boundingBox.Width + 5) / 64), (float)((boundingBox.Y + boundingBox.Height / 2) / 64));
			case 2:
				return new Vector2((float)((boundingBox.X + boundingBox.Width / 2) / 64), (float)((boundingBox.Y + boundingBox.Height + 5) / 64));
			case 3:
				return new Vector2((float)((boundingBox.X - 5) / 64), (float)((boundingBox.Y + boundingBox.Height / 2) / 64));
			default:
				return this.getStandingPosition();
			}
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x0001BDD4 File Offset: 0x00019FD4
		public Vector2 GetDropLocation()
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
			switch (this.FacingDirection)
			{
			case 0:
				return new Vector2((float)(boundingBox.X + 16), (float)(boundingBox.Y - 64));
			case 1:
				return new Vector2((float)(boundingBox.X + boundingBox.Width + 64), (float)(boundingBox.Y + 16));
			case 2:
				return new Vector2((float)(boundingBox.X + 16), (float)(boundingBox.Y + boundingBox.Height + 64));
			case 3:
				return new Vector2((float)(boundingBox.X - 64), (float)(boundingBox.Y + 16));
			default:
				return this.getStandingPosition();
			}
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x0001BE84 File Offset: 0x0001A084
		public virtual Vector2 GetToolLocation(Vector2 target_position, bool ignoreClick = false)
		{
			int direction = this.FacingDirection;
			if ((Game1.player.CurrentTool == null || !Game1.player.CurrentTool.CanUseOnStandingTile()) && (int)(target_position.X / 64f) == Game1.player.TilePoint.X && (int)(target_position.Y / 64f) == Game1.player.TilePoint.Y)
			{
				Microsoft.Xna.Framework.Rectangle bb = this.GetBoundingBox();
				switch (this.FacingDirection)
				{
				case 0:
					return new Vector2((float)(bb.X + bb.Width / 2), (float)(bb.Y - 64));
				case 1:
					return new Vector2((float)(bb.X + bb.Width + 64), (float)(bb.Y + bb.Height / 2));
				case 2:
					return new Vector2((float)(bb.X + bb.Width / 2), (float)(bb.Y + bb.Height + 64));
				case 3:
					return new Vector2((float)(bb.X - 64), (float)(bb.Y + bb.Height / 2));
				}
			}
			if (!ignoreClick && !target_position.Equals(Vector2.Zero) && this.Name.Equals(Game1.player.Name))
			{
				bool allow_clicking_on_same_tile = false;
				if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.CanUseOnStandingTile())
				{
					allow_clicking_on_same_tile = true;
				}
				if (Utility.withinRadiusOfPlayer((int)target_position.X, (int)target_position.Y, 1, Game1.player))
				{
					direction = Game1.player.getGeneralDirectionTowards(new Vector2((float)((int)target_position.X), (float)((int)target_position.Y)), 0, false, true);
					if (allow_clicking_on_same_tile)
					{
						return target_position;
					}
					Point playerPixel = Game1.player.StandingPixel;
					if (Math.Abs(target_position.X - (float)playerPixel.X) >= 32f || Math.Abs(target_position.Y - (float)playerPixel.Y) >= 32f)
					{
						return target_position;
					}
				}
			}
			Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
			switch (direction)
			{
			case 0:
				return new Vector2((float)(boundingBox.X + boundingBox.Width / 2), (float)(boundingBox.Y - 48));
			case 1:
				return new Vector2((float)(boundingBox.X + boundingBox.Width + 48), (float)(boundingBox.Y + boundingBox.Height / 2));
			case 2:
				return new Vector2((float)(boundingBox.X + boundingBox.Width / 2), (float)(boundingBox.Y + boundingBox.Height + 48));
			case 3:
				return new Vector2((float)(boundingBox.X - 48), (float)(boundingBox.Y + boundingBox.Height / 2));
			default:
				return this.getStandingPosition();
			}
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0001C143 File Offset: 0x0001A343
		public virtual Vector2 GetToolLocation(bool ignoreClick = false)
		{
			if (!Game1.wasMouseVisibleThisFrame || Game1.isAnyGamePadButtonBeingHeld())
			{
				ignoreClick = true;
			}
			return this.GetToolLocation(this.lastClick, ignoreClick);
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x0001C164 File Offset: 0x0001A364
		public int getGeneralDirectionTowards(Vector2 target, int yBias = 0, bool opposite = false, bool useTileCalculations = true)
		{
			int multiplier = opposite ? -1 : 1;
			Point playerPixel = this.StandingPixel;
			int xDif;
			int yDif;
			if (useTileCalculations)
			{
				Point playerTile = this.TilePoint;
				xDif = ((int)(target.X / 64f) - playerTile.X) * multiplier;
				yDif = ((int)(target.Y / 64f) - playerTile.Y) * multiplier;
				if (xDif == 0 && yDif == 0)
				{
					Vector2 vector = new Vector2(((float)((int)(target.X / 64f)) + 0.5f) * 64f, ((float)((int)(target.Y / 64f)) + 0.5f) * 64f);
					xDif = (int)(vector.X - (float)playerPixel.X) * multiplier;
					yDif = (int)(vector.Y - (float)playerPixel.Y) * multiplier;
					yBias *= 64;
				}
			}
			else
			{
				xDif = (int)(target.X - (float)playerPixel.X) * multiplier;
				yDif = (int)(target.Y - (float)playerPixel.Y) * multiplier;
			}
			if (xDif > Math.Abs(yDif) + yBias)
			{
				return 1;
			}
			if (Math.Abs(xDif) > Math.Abs(yDif) + yBias)
			{
				return 3;
			}
			if (yDif > 0 || ((float)playerPixel.Y - target.Y) * (float)multiplier < 0f)
			{
				return 2;
			}
			return 0;
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x0001C291 File Offset: 0x0001A491
		public void faceGeneralDirection(Vector2 target, int yBias, bool opposite, bool useTileCalculations)
		{
			this.faceDirection(this.getGeneralDirectionTowards(target, yBias, opposite, useTileCalculations));
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x0001C2A4 File Offset: 0x0001A4A4
		public void faceGeneralDirection(Vector2 target, int yBias = 0, bool opposite = false)
		{
			this.faceGeneralDirection(target, yBias, opposite, true);
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x0001C2B0 File Offset: 0x0001A4B0
		public virtual void draw(SpriteBatch b)
		{
			this.draw(b, 1f);
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0001C2BE File Offset: 0x0001A4BE
		public virtual void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x0001C2C0 File Offset: 0x0001A4C0
		public virtual void draw(SpriteBatch b, float alpha = 1f)
		{
			Vector2 draw_position = this.Position;
			this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, draw_position), (float)this.StandingPixel.Y / 10000f);
			if (this.IsEmoting)
			{
				Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
				emotePosition.Y -= 96f;
				emotePosition.Y += (float)this.emoteYOffset;
				emotePosition.X += (float)(this.Sprite.SourceRect.Width * 4) / 2f - 32f;
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.StandingPixel.Y / 10000f);
			}
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x0001C3D4 File Offset: 0x0001A5D4
		public virtual void draw(SpriteBatch b, int ySourceRectOffset, float alpha = 1f)
		{
			Microsoft.Xna.Framework.Rectangle box = this.GetBoundingBox();
			this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.Position) + new Vector2((float)(this.GetSpriteWidthForPositioning() * 4 / 2), (float)(box.Height / 2)), (float)box.Center.Y / 10000f, 0, ySourceRectOffset, Color.White, false, 4f, 0f, true);
			if (this.IsEmoting)
			{
				Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
				emotePosition.Y -= 96f;
				emotePosition.Y += (float)this.emoteYOffset;
				emotePosition.X += (float)(this.Sprite.SourceRect.Width * 4) / 2f - 32f;
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.StandingPixel.Y / 10000f);
			}
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x0001C51D File Offset: 0x0001A71D
		public int GetSpriteWidthForPositioning()
		{
			if (this.forceOneTileWide.Value)
			{
				return 16;
			}
			return this.Sprite.SpriteWidth;
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x0001C53C File Offset: 0x0001A73C
		public virtual Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			if (this.Sprite == null)
			{
				return Microsoft.Xna.Framework.Rectangle.Empty;
			}
			Vector2 position = this.Position;
			int width = this.GetSpriteWidthForPositioning() * 4 * 3 / 4;
			return new Microsoft.Xna.Framework.Rectangle((int)position.X + 8, (int)position.Y + 16, width, 32);
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0001C586 File Offset: 0x0001A786
		public void stopWithoutChangingFrame()
		{
			this.moveDown = false;
			this.moveLeft = false;
			this.moveRight = false;
			this.moveUp = false;
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0001C5A4 File Offset: 0x0001A7A4
		public virtual void collisionWithFarmerBehavior()
		{
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x0001C5A8 File Offset: 0x0001A7A8
		public Vector2 getStandingPosition()
		{
			Point pixel = this.StandingPixel;
			return new Vector2((float)pixel.X, (float)pixel.Y);
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x0001C5D0 File Offset: 0x0001A7D0
		public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
		{
			Vector2 position = this.Position;
			return new Vector2(position.X - (float)viewport.X, position.Y - (float)viewport.Y + (float)this.yJumpOffset) + this.drawOffset;
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0001C61A File Offset: 0x0001A81A
		public virtual bool isMoving()
		{
			return this.moveUp || this.moveDown || this.moveRight || this.moveLeft || this.position.Field.IsInterpolating();
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0001C650 File Offset: 0x0001A850
		public void setTileLocation(Vector2 tileLocation)
		{
			float standingX = (tileLocation.X + 0.5f) * 64f;
			float standingY = (tileLocation.Y + 0.5f) * 64f;
			Vector2 pos = this.Position;
			Microsoft.Xna.Framework.Rectangle box = this.GetBoundingBox();
			pos.X += standingX - (float)box.Center.X;
			pos.Y += standingY - (float)box.Center.Y;
			this.Position = pos;
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0001C6CC File Offset: 0x0001A8CC
		public void startGlowing(Color glowingColor, bool border, float glowRate)
		{
			if (!this.glowingColor.Equals(glowingColor))
			{
				this.isGlowing = true;
				this.coloredBorder = border;
				this.glowingColor = glowingColor;
				this.glowUp = true;
				this.glowRate = glowRate;
				this.glowingTransparency = 0f;
			}
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0001C70A File Offset: 0x0001A90A
		public void stopGlowing()
		{
			this.isGlowing = false;
			this.glowingColor = Color.White;
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0001C71E File Offset: 0x0001A91E
		public virtual void jumpWithoutSound(float velocity = 8f)
		{
			this.yJumpVelocity = velocity;
			this.yJumpOffset = -1;
			this.yJumpGravity = -0.5f;
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0001C73C File Offset: 0x0001A93C
		public virtual void jump()
		{
			this.yJumpVelocity = 8f;
			this.yJumpOffset = -1;
			this.yJumpGravity = -0.5f;
			this.wasJumpWithSound = true;
			GameLocation currentLocation = this.currentLocation;
			if (currentLocation == null)
			{
				return;
			}
			currentLocation.localSound("dwop", null, null, SoundContext.Default);
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0001C798 File Offset: 0x0001A998
		public virtual void jump(float jumpVelocity)
		{
			this.yJumpVelocity = jumpVelocity;
			this.yJumpOffset = -1;
			this.yJumpGravity = -0.5f;
			this.wasJumpWithSound = true;
			GameLocation currentLocation = this.currentLocation;
			if (currentLocation == null)
			{
				return;
			}
			currentLocation.localSound("dwop", null, null, SoundContext.Default);
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0001C7F0 File Offset: 0x0001A9F0
		public void faceTowardFarmerForPeriod(int milliseconds, int radius, bool faceAway, Farmer who)
		{
			if (this.SimpleNonVillagerNPC)
			{
				return;
			}
			if ((this.Sprite != null && this.Sprite.CurrentAnimation == null) || this.isMoving())
			{
				if (this.isMoving())
				{
					milliseconds /= 2;
				}
				this.faceTowardFarmerEvent.Fire(milliseconds);
				this.faceTowardFarmerEvent.Poll();
				if (this.facingDirectionBeforeSpeakingToPlayer.Value == -1)
				{
					this.facingDirectionBeforeSpeakingToPlayer.Value = this.FacingDirection;
				}
				this.faceTowardFarmerRadius.Value = radius;
				this.faceAwayFromFarmer.Value = faceAway;
				this.whoToFace.Value = who;
				this.hasJustStartedFacingPlayer = true;
			}
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0001C892 File Offset: 0x0001AA92
		protected void performFaceTowardFarmerEvent(int milliseconds)
		{
			if ((this.Sprite != null && this.Sprite.CurrentAnimation == null) || this.isMoving())
			{
				this.Halt();
				this.faceTowardFarmerTimer = milliseconds;
				this.movementPause = milliseconds;
			}
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x0001C8C5 File Offset: 0x0001AAC5
		public virtual void update(GameTime time, GameLocation location)
		{
			this.position.UpdateExtrapolation((float)this.speed + this.addedSpeed);
			this.update(time, location, 0L, true);
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0001C8EB File Offset: 0x0001AAEB
		public virtual void OnLocationRemoved()
		{
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0001C8ED File Offset: 0x0001AAED
		public virtual void checkForFootstep()
		{
			Game1.currentLocation.playTerrainSound(this.Tile, this, true);
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0001C904 File Offset: 0x0001AB04
		public virtual void update(GameTime time, GameLocation location, long id, bool move)
		{
			this.position.UpdateExtrapolation((float)this.speed + this.addedSpeed);
			this.currentLocation = location;
			this.faceTowardFarmerEvent.Poll();
			if (this.yJumpOffset != 0)
			{
				this.yJumpVelocity += this.yJumpGravity;
				this.yJumpOffset -= (int)this.yJumpVelocity;
				if (this.yJumpOffset >= 0)
				{
					this.yJumpOffset = 0;
					this.yJumpVelocity = 0f;
					if (!this.IsMonster && (location == null || location.Equals(Game1.currentLocation)) && this.wasJumpWithSound)
					{
						this.checkForFootstep();
					}
				}
			}
			if (this.forceUpdateTimer > 0)
			{
				this.forceUpdateTimer -= time.ElapsedGameTime.Milliseconds;
			}
			this.updateGlow();
			this.updateEmote(time);
			this.updateFaceTowardsFarmer(time, location);
			bool is_event_controlled_character = false;
			if (location.currentEvent != null)
			{
				if (location.IsTemporary)
				{
					is_event_controlled_character = true;
				}
				else if (location.currentEvent.actors.Contains(this))
				{
					is_event_controlled_character = true;
				}
			}
			if (Game1.IsMasterGame || is_event_controlled_character)
			{
				if (this.controller == null && move && !this.freezeMotion)
				{
					this.updateMovement(location, time);
				}
				if (this.controller != null && !this.freezeMotion && this.controller.update(time))
				{
					this.controller = null;
				}
			}
			else
			{
				this.updateSlaveAnimation(time);
			}
			this.hasJustStartedFacingPlayer = false;
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x0001CA6C File Offset: 0x0001AC6C
		public virtual void updateFaceTowardsFarmer(GameTime time, GameLocation location)
		{
			if (this.faceTowardFarmerTimer > 0)
			{
				this.faceTowardFarmerTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.whoToFace.Value != null)
				{
					Vector2 tile = this.Tile;
					if (!this.faceTowardFarmer && this.faceTowardFarmerTimer > 0 && Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, this.faceTowardFarmerRadius.Value, this.whoToFace.Value))
					{
						this.faceTowardFarmer = true;
					}
					else if (!Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, this.faceTowardFarmerRadius.Value, this.whoToFace.Value) || this.faceTowardFarmerTimer <= 0)
					{
						this.faceDirection(this.facingDirectionBeforeSpeakingToPlayer.Value);
						if (this.faceTowardFarmerTimer <= 0)
						{
							this.facingDirectionBeforeSpeakingToPlayer.Value = -1;
							this.faceTowardFarmer = false;
							this.faceAwayFromFarmer.Value = false;
							this.faceTowardFarmerTimer = 0;
						}
					}
				}
			}
			if ((Game1.IsMasterGame || location.currentEvent != null) && this.faceTowardFarmer && this.whoToFace.Value != null)
			{
				this.faceGeneralDirection(this.whoToFace.Value.getStandingPosition(), 0, false, true);
				if (this.faceAwayFromFarmer.Value)
				{
					this.faceDirection((this.FacingDirection + 2) % 4);
				}
			}
			this.hasJustStartedFacingPlayer = false;
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0001CBD1 File Offset: 0x0001ADD1
		public virtual bool hasSpecialCollisionRules()
		{
			return false;
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0001CBD4 File Offset: 0x0001ADD4
		public virtual bool isColliding(GameLocation l, Vector2 tile)
		{
			return false;
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0001CBD8 File Offset: 0x0001ADD8
		public virtual void animateInFacingDirection(GameTime time)
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.AnimateUp(time, 0, "");
				return;
			case 1:
				this.Sprite.AnimateRight(time, 0, "");
				return;
			case 2:
				this.Sprite.AnimateDown(time, 0, "");
				return;
			case 3:
				this.Sprite.AnimateLeft(time, 0, "");
				return;
			default:
				return;
			}
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0001CC4E File Offset: 0x0001AE4E
		public virtual void updateMovement(GameLocation location, GameTime time)
		{
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0001CC50 File Offset: 0x0001AE50
		protected virtual void updateSlaveAnimation(GameTime time)
		{
			if (this.Sprite.CurrentAnimation != null)
			{
				this.Sprite.animateOnce(time);
				return;
			}
			if (this.SimpleNonVillagerNPC)
			{
				return;
			}
			this.faceDirection(this.FacingDirection);
			if (this.isMoving())
			{
				this.animateInFacingDirection(time);
				return;
			}
			this.Sprite.StopAnimation();
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0001CCA8 File Offset: 0x0001AEA8
		public void updateGlow()
		{
			if (this.isGlowing)
			{
				if (this.glowUp)
				{
					this.glowingTransparency += this.glowRate;
					if (this.glowingTransparency >= 1f)
					{
						this.glowingTransparency = 1f;
						this.glowUp = false;
						return;
					}
				}
				else
				{
					this.glowingTransparency -= this.glowRate;
					if (this.glowingTransparency <= 0f)
					{
						this.glowingTransparency = 0f;
						this.glowUp = true;
					}
				}
			}
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0001CD2C File Offset: 0x0001AF2C
		public void convertEventMotionCommandToMovement(Vector2 command)
		{
			if (command.X < 0f)
			{
				this.SetMovingLeft(true);
				return;
			}
			if (command.X > 0f)
			{
				this.SetMovingRight(true);
				return;
			}
			if (command.Y < 0f)
			{
				this.SetMovingUp(true);
				return;
			}
			if (command.Y > 0f)
			{
				this.SetMovingDown(true);
			}
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0001CD8C File Offset: 0x0001AF8C
		public virtual void DrawShadow(SpriteBatch b)
		{
			int offsetX = this.GetSpriteWidthForPositioning() * 4 / 2;
			int offsetY = this.GetBoundingBox().Height;
			float shadowScale = Math.Max(0f, 4f + (float)this.yJumpOffset / 40f) * this.scale.Value;
			if (!this.IsMonster)
			{
				if (Game1.CurrentEvent != null && this.Sprite.SpriteHeight <= 16)
				{
					offsetY += -4;
				}
				else
				{
					offsetY += 12;
				}
			}
			CharacterData data;
			if (this.IsVillager && NPC.TryGetData(this.Name, out data) && data.Shadow != null)
			{
				CharacterShadowData shadow = data.Shadow;
				if (!shadow.Visible)
				{
					return;
				}
				offsetX += shadow.Offset.X;
				offsetY += shadow.Offset.Y;
				shadowScale = Math.Max(0f, shadowScale * shadow.Scale);
			}
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.GetShadowOffset() + this.Position + new Vector2((float)offsetX, (float)offsetY)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), shadowScale, SpriteEffects.None, Math.Max(0f, (float)this.StandingPixel.Y / 10000f) - 1E-06f);
		}

		// Token: 0x04000233 RID: 563
		public const float emoteBeginInterval = 20f;

		// Token: 0x04000234 RID: 564
		public const float emoteNormalInterval = 250f;

		// Token: 0x04000235 RID: 565
		public const int emptyCanEmote = 4;

		// Token: 0x04000236 RID: 566
		public const int questionMarkEmote = 8;

		// Token: 0x04000237 RID: 567
		public const int angryEmote = 12;

		// Token: 0x04000238 RID: 568
		public const int exclamationEmote = 16;

		// Token: 0x04000239 RID: 569
		public const int heartEmote = 20;

		// Token: 0x0400023A RID: 570
		public const int sleepEmote = 24;

		// Token: 0x0400023B RID: 571
		public const int sadEmote = 28;

		// Token: 0x0400023C RID: 572
		public const int happyEmote = 32;

		// Token: 0x0400023D RID: 573
		public const int xEmote = 36;

		// Token: 0x0400023E RID: 574
		public const int pauseEmote = 40;

		// Token: 0x0400023F RID: 575
		public const int videoGameEmote = 52;

		// Token: 0x04000240 RID: 576
		public const int musicNoteEmote = 56;

		// Token: 0x04000241 RID: 577
		public const int blushEmote = 60;

		// Token: 0x04000242 RID: 578
		public const int blockedIntervalBeforeEmote = 3000;

		// Token: 0x04000243 RID: 579
		public const int blockedIntervalBeforeSprint = 5000;

		// Token: 0x04000244 RID: 580
		public const double chanceForSound = 0.001;

		// Token: 0x04000245 RID: 581
		private static Vector2 ClearPositionValue = new Vector2(-2.1474836E+09f);

		// Token: 0x04000246 RID: 582
		private Point cachedStandingPixel;

		// Token: 0x04000247 RID: 583
		private Vector2 cachedTile;

		// Token: 0x04000248 RID: 584
		private Point cachedTilePoint;

		// Token: 0x04000249 RID: 585
		private Vector2 pixelPositionForCachedStandingPixel;

		// Token: 0x0400024A RID: 586
		private Vector2 pixelPositionForCachedTile;

		// Token: 0x0400024B RID: 587
		private Vector2 pixelPositionForCachedTilePoint;

		// Token: 0x0400024D RID: 589
		[XmlIgnore]
		public readonly NetBool hideFromAnimalSocialMenu = new NetBool();

		// Token: 0x0400024E RID: 590
		[XmlIgnore]
		public readonly NetRef<AnimatedSprite> sprite = new NetRef<AnimatedSprite>();

		// Token: 0x0400024F RID: 591
		[XmlIgnore]
		public readonly NetPosition position = new NetPosition();

		// Token: 0x04000250 RID: 592
		[XmlIgnore]
		private readonly NetInt netSpeed = new NetInt();

		// Token: 0x04000251 RID: 593
		[XmlIgnore]
		private readonly NetFloat netAddedSpeed = new NetFloat();

		// Token: 0x04000252 RID: 594
		[XmlIgnore]
		public readonly NetDirection facingDirection = new NetDirection(2);

		// Token: 0x04000253 RID: 595
		[XmlIgnore]
		public int blockedInterval;

		// Token: 0x04000254 RID: 596
		[XmlIgnore]
		public int faceTowardFarmerTimer;

		// Token: 0x04000255 RID: 597
		[XmlIgnore]
		public int forceUpdateTimer;

		// Token: 0x04000256 RID: 598
		[XmlIgnore]
		public int movementPause;

		// Token: 0x04000257 RID: 599
		[XmlIgnore]
		public NetEvent1Field<int, NetInt> faceTowardFarmerEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04000258 RID: 600
		[XmlIgnore]
		public readonly NetInt faceTowardFarmerRadius = new NetInt();

		// Token: 0x04000259 RID: 601
		[XmlIgnore]
		public readonly NetBool simpleNonVillagerNPC = new NetBool();

		// Token: 0x0400025A RID: 602
		[XmlIgnore]
		public int nonVillagerNPCTimesTalked;

		// Token: 0x0400025B RID: 603
		[XmlElement("name")]
		public readonly NetString name = new NetString();

		// Token: 0x0400025C RID: 604
		[XmlElement("forceOneTileWide")]
		public readonly NetBool forceOneTileWide = new NetBool(false);

		// Token: 0x0400025D RID: 605
		protected bool moveUp;

		// Token: 0x0400025E RID: 606
		protected bool moveRight;

		// Token: 0x0400025F RID: 607
		protected bool moveDown;

		// Token: 0x04000260 RID: 608
		protected bool moveLeft;

		// Token: 0x04000261 RID: 609
		protected bool freezeMotion;

		// Token: 0x04000262 RID: 610
		[XmlIgnore]
		private string _displayName;

		// Token: 0x04000263 RID: 611
		public bool isEmoting;

		// Token: 0x04000264 RID: 612
		public bool isCharging;

		// Token: 0x04000265 RID: 613
		public bool isGlowing;

		// Token: 0x04000266 RID: 614
		public bool coloredBorder;

		// Token: 0x04000267 RID: 615
		public bool flip;

		// Token: 0x04000268 RID: 616
		public bool drawOnTop;

		// Token: 0x04000269 RID: 617
		public bool faceTowardFarmer;

		// Token: 0x0400026A RID: 618
		public bool ignoreMovementAnimation;

		// Token: 0x0400026B RID: 619
		[XmlIgnore]
		public bool hasJustStartedFacingPlayer;

		// Token: 0x0400026C RID: 620
		[XmlElement("faceAwayFromFarmer")]
		public readonly NetBool faceAwayFromFarmer = new NetBool();

		// Token: 0x0400026D RID: 621
		protected int currentEmote;

		// Token: 0x0400026E RID: 622
		protected int currentEmoteFrame;

		// Token: 0x0400026F RID: 623
		protected readonly NetInt facingDirectionBeforeSpeakingToPlayer = new NetInt(-1);

		// Token: 0x04000270 RID: 624
		[XmlIgnore]
		public float emoteInterval;

		// Token: 0x04000271 RID: 625
		[XmlIgnore]
		public float xVelocity;

		// Token: 0x04000272 RID: 626
		[XmlIgnore]
		public float yVelocity;

		// Token: 0x04000273 RID: 627
		[XmlIgnore]
		public Vector2 lastClick = Vector2.Zero;

		// Token: 0x04000274 RID: 628
		public readonly NetFloat scale = new NetFloat(1f);

		// Token: 0x04000275 RID: 629
		public float glowingTransparency;

		// Token: 0x04000276 RID: 630
		public float glowRate;

		// Token: 0x04000277 RID: 631
		private bool glowUp;

		// Token: 0x04000278 RID: 632
		[XmlIgnore]
		public readonly NetBool swimming = new NetBool();

		// Token: 0x04000279 RID: 633
		[XmlIgnore]
		public bool nextEventcommandAfterEmote;

		// Token: 0x0400027A RID: 634
		[XmlIgnore]
		public bool farmerPassesThrough;

		// Token: 0x0400027B RID: 635
		[XmlIgnore]
		public NetBool netEventActor = new NetBool();

		// Token: 0x0400027C RID: 636
		[XmlIgnore]
		public readonly NetBool collidesWithOtherCharacters = new NetBool();

		// Token: 0x0400027D RID: 637
		protected bool ignoreMovementAnimations;

		// Token: 0x0400027E RID: 638
		[XmlIgnore]
		public int yJumpOffset;

		// Token: 0x0400027F RID: 639
		[XmlIgnore]
		public int ySourceRectOffset;

		// Token: 0x04000280 RID: 640
		[XmlIgnore]
		public float yJumpVelocity;

		// Token: 0x04000281 RID: 641
		[XmlIgnore]
		public float yJumpGravity = -0.5f;

		// Token: 0x04000282 RID: 642
		[XmlIgnore]
		public bool wasJumpWithSound;

		// Token: 0x04000283 RID: 643
		[XmlIgnore]
		private readonly NetFarmerRef whoToFace = new NetFarmerRef();

		// Token: 0x04000284 RID: 644
		[XmlIgnore]
		public Color glowingColor;

		// Token: 0x04000285 RID: 645
		[XmlIgnore]
		public PathFindController controller;

		// Token: 0x04000286 RID: 646
		private bool emoteFading;

		// Token: 0x04000287 RID: 647
		[XmlIgnore]
		private readonly NetBool _willDestroyObjectsUnderfoot = new NetBool(true);

		// Token: 0x04000288 RID: 648
		[XmlIgnore]
		protected readonly NetLocationRef currentLocationRef = new NetLocationRef();

		// Token: 0x0400028B RID: 651
		private Microsoft.Xna.Framework.Rectangle originalSourceRect;

		// Token: 0x0400028C RID: 652
		protected int emoteYOffset;

		// Token: 0x0400028D RID: 653
		public static readonly Vector2[] AdjacentTilesOffsets = new Vector2[]
		{
			new Vector2(1f, 0f),
			new Vector2(-1f, 0f),
			new Vector2(0f, -1f),
			new Vector2(0f, 1f)
		};

		// Token: 0x0400028E RID: 654
		[XmlIgnore]
		public Vector2 drawOffset = Vector2.Zero;

		// Token: 0x0400028F RID: 655
		[XmlIgnore]
		public bool shouldShadowBeOffset;
	}
}

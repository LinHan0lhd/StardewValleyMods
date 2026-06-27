using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace StardewValley.Projectiles
{
	// Token: 0x0200019A RID: 410
	public abstract class Projectile : INetObject<NetFields>, IHaveModData
	{
		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06001D30 RID: 7472 RVA: 0x0014EA84 File Offset: 0x0014CC84
		// (set) Token: 0x06001D31 RID: 7473 RVA: 0x0014EAB4 File Offset: 0x0014CCB4
		protected float rotation
		{
			get
			{
				if (this._rotation == null)
				{
					this._rotation = new float?(this.startingRotation.Value);
				}
				return this._rotation.Value;
			}
			set
			{
				this._rotation = new float?(value);
			}
		}

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06001D32 RID: 7474 RVA: 0x0014EAC2 File Offset: 0x0014CCC2
		// (set) Token: 0x06001D33 RID: 7475 RVA: 0x0014EACF File Offset: 0x0014CCCF
		public bool IgnoreLocationCollision
		{
			get
			{
				return this.ignoreLocationCollision.Value;
			}
			set
			{
				this.ignoreLocationCollision.Value = value;
			}
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06001D34 RID: 7476 RVA: 0x0014EADD File Offset: 0x0014CCDD
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06001D35 RID: 7477 RVA: 0x0014EAE5 File Offset: 0x0014CCE5
		// (set) Token: 0x06001D36 RID: 7478 RVA: 0x0014EAF2 File Offset: 0x0014CCF2
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

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06001D37 RID: 7479 RVA: 0x0014EB00 File Offset: 0x0014CD00
		public NetFields NetFields { get; } = new NetFields("Projectile");

		// Token: 0x06001D38 RID: 7480 RVA: 0x0014EB08 File Offset: 0x0014CD08
		public Projectile()
		{
			this.InitNetFields();
			this.uniqueID.Value = Game1.random.Next();
		}

		// Token: 0x06001D39 RID: 7481 RVA: 0x0014ECFC File Offset: 0x0014CEFC
		protected virtual void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.currentTileSheetIndex, "currentTileSheetIndex").AddField(this.position.NetFields, "position.NetFields").AddField(this.tailLength, "tailLength").AddField(this.bouncesLeft, "bouncesLeft").AddField(this.bounceSound, "bounceSound").AddField(this.rotationVelocity, "rotationVelocity").AddField(this.startingRotation, "startingRotation").AddField(this.xVelocity, "xVelocity").AddField(this.yVelocity, "yVelocity").AddField(this.damagesMonsters, "damagesMonsters").AddField(this.theOneWhoFiredMe.NetFields, "theOneWhoFiredMe.NetFields").AddField(this.ignoreLocationCollision, "ignoreLocationCollision").AddField(this.maxTravelDistance, "maxTravelDistance").AddField(this.ignoreTravelGracePeriod, "ignoreTravelGracePeriod").AddField(this.ignoreMeleeAttacks, "ignoreMeleeAttacks").AddField(this.height, "height").AddField(this.startingScale, "startingScale").AddField(this.scaleGrow, "scaleGrow").AddField(this.color, "color").AddField(this.light, "light").AddField(this.itemId, "itemId").AddField(this.projectileID, "projectileID").AddField(this.ignoreObjectCollisions, "ignoreObjectCollisions").AddField(this.acceleration, "acceleration").AddField(this.maxVelocity, "maxVelocity").AddField(this.alpha, "alpha").AddField(this.alphaChange, "alphaChange").AddField(this.boundingBoxWidth, "boundingBoxWidth").AddField(this.ignoreCharacterCollisions, "ignoreCharacterCollisions").AddField(this.uniqueID, "uniqueID").AddField(this.modData, "modData");
		}

		// Token: 0x06001D3A RID: 7482 RVA: 0x0014EF10 File Offset: 0x0014D110
		private void behaviorOnCollision(GameLocation location, Character target, TerrainFeature terrainFeature)
		{
			bool successfulHit = true;
			Farmer player = target as Farmer;
			if (player == null)
			{
				NPC npc = target as NPC;
				if (npc == null)
				{
					if (terrainFeature != null)
					{
						this.behaviorOnCollisionWithTerrainFeature(terrainFeature, terrainFeature.Tile, location);
					}
					else
					{
						this.behaviorOnCollisionWithOther(location);
					}
				}
				else if (!npc.IsInvisible)
				{
					this.behaviorOnCollisionWithMonster(npc, location);
				}
				else
				{
					successfulHit = false;
				}
			}
			else
			{
				this.behaviorOnCollisionWithPlayer(location, player);
			}
			if (successfulHit && this.piercesLeft.Value <= 0 && this.hasLit)
			{
				LightSource lightSource = Utility.getLightSource(this.lightSourceId);
				if (lightSource != null)
				{
					lightSource.fadeOut.Value = 3;
				}
			}
		}

		// Token: 0x06001D3B RID: 7483
		public abstract void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player);

		// Token: 0x06001D3C RID: 7484
		public abstract void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location);

		// Token: 0x06001D3D RID: 7485
		public abstract void behaviorOnCollisionWithOther(GameLocation location);

		// Token: 0x06001D3E RID: 7486
		public abstract void behaviorOnCollisionWithMonster(NPC n, GameLocation location);

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06001D3F RID: 7487 RVA: 0x0014EFA3 File Offset: 0x0014D1A3
		// (set) Token: 0x06001D40 RID: 7488 RVA: 0x0014EFD3 File Offset: 0x0014D1D3
		[XmlIgnore]
		public virtual float localScale
		{
			get
			{
				if (this._localScale == null)
				{
					this._localScale = new float?(this.startingScale.Value);
				}
				return this._localScale.Value;
			}
			set
			{
				this._localScale = new float?(value);
			}
		}

		// Token: 0x06001D41 RID: 7489 RVA: 0x0014EFE4 File Offset: 0x0014D1E4
		public virtual bool update(GameTime time, GameLocation location)
		{
			if (Game1.isTimePaused)
			{
				return false;
			}
			if (Game1.IsMasterGame && this.hostTimeUntilAttackable > 0f)
			{
				this.hostTimeUntilAttackable -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.hostTimeUntilAttackable <= 0f)
				{
					this.ignoreMeleeAttacks.Value = false;
					this.hostTimeUntilAttackable = -1f;
				}
			}
			if (this.light.Value)
			{
				if (!this.hasLit)
				{
					this.hasLit = true;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
					defaultInterpolatedStringHandler.AppendLiteral("_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
					this.lightSourceId = defaultInterpolatedStringHandler.ToStringAndClear();
					if (location.Equals(Game1.currentLocation))
					{
						Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, this.position.Value + new Vector2(32f, 32f), 1f, new Color(Utility.getOppositeColor(this.color.Value).ToVector4() * this.alpha.Value), LightSource.LightContext.None, 0L, location.NameOrUniqueName));
					}
				}
				else
				{
					LightSource i = Utility.getLightSource(this.lightSourceId);
					if (i != null)
					{
						i.color.A = (byte)(255f * this.alpha.Value);
					}
					Utility.repositionLightSource(this.lightSourceId, this.position.Value + new Vector2(32f, 32f));
				}
			}
			this.alpha.Value += this.alphaChange.Value;
			this.alpha.Value = Utility.Clamp(this.alpha.Value, 0f, 1f);
			this.rotation += this.rotationVelocity.Value;
			this.travelTime += time.ElapsedGameTime.Milliseconds;
			if (this.scaleGrow.Value != 0f)
			{
				this.localScale += this.scaleGrow.Value;
			}
			Vector2 old_position = this.position.Value;
			this.updatePosition(time);
			this.updateTail(time);
			this.travelDistance += (old_position - this.position.Value).Length();
			if (this.maxTravelDistance.Value >= 0)
			{
				if (this.travelDistance > (float)(this.maxTravelDistance.Value - 128))
				{
					this.alpha.Value = ((float)this.maxTravelDistance.Value - this.travelDistance) / 128f;
				}
				if (this.travelDistance >= (float)this.maxTravelDistance.Value)
				{
					if (this.hasLit)
					{
						Utility.removeLightSource(this.lightSourceId);
					}
					return true;
				}
			}
			Character target;
			TerrainFeature terrainFeature;
			if ((this.travelTime > 100 || this.ignoreTravelGracePeriod.Value) && this.isColliding(location, out target, out terrainFeature) && this.ShouldApplyCollisionLocally(location))
			{
				if (this.bouncesLeft.Value <= 0 || target != null)
				{
					this.behaviorOnCollision(location, target, terrainFeature);
					return this.piercesLeft.Value <= 0;
				}
				NetInt netInt = this.bouncesLeft;
				int value = netInt.Value;
				netInt.Value = value - 1;
				bool[] array = Utility.horizontalOrVerticalCollisionDirections(this.getBoundingBox(), this.theOneWhoFiredMe.Get(location), true);
				if (array[0])
				{
					this.xVelocity.Value = -this.xVelocity.Value;
				}
				if (array[1])
				{
					this.yVelocity.Value = -this.yVelocity.Value;
				}
				if (!string.IsNullOrEmpty(this.bounceSound.Value) && location != null)
				{
					location.playSound(this.bounceSound.Value, null, null, SoundContext.Default);
				}
			}
			return false;
		}

		// Token: 0x06001D42 RID: 7490 RVA: 0x0014F3F8 File Offset: 0x0014D5F8
		protected virtual bool ShouldApplyCollisionLocally(GameLocation location)
		{
			Farmer firedBy = this.theOneWhoFiredMe.Get(location) as Farmer;
			return firedBy == null || firedBy == Game1.player || (Game1.IsMasterGame && firedBy.currentLocation != location);
		}

		// Token: 0x06001D43 RID: 7491 RVA: 0x0014F43C File Offset: 0x0014D63C
		protected virtual void updateTail(GameTime time)
		{
			this.tailCounter -= time.ElapsedGameTime.Milliseconds;
			if (this.tailCounter <= 0)
			{
				this.tailCounter = 50;
				this.tail.Enqueue(this.position.Value);
				if (this.tail.Count > this.tailLength.Value)
				{
					this.tail.Dequeue();
				}
			}
		}

		// Token: 0x06001D44 RID: 7492 RVA: 0x0014F4B0 File Offset: 0x0014D6B0
		public virtual bool isColliding(GameLocation location, out Character target, out TerrainFeature terrainFeature)
		{
			target = null;
			terrainFeature = null;
			Rectangle boundingBox = this.getBoundingBox();
			if (!this.ignoreCharacterCollisions.Value)
			{
				if (this.damagesMonsters.Value)
				{
					Character npc = location.doesPositionCollideWithCharacter(boundingBox, false);
					if (npc != null)
					{
						if (npc is NPC && (npc as NPC).IsInvisible)
						{
							return false;
						}
						target = npc;
						return true;
					}
				}
				else if (Game1.player.currentLocation == location && Game1.player.GetBoundingBox().Intersects(boundingBox))
				{
					target = Game1.player;
					return true;
				}
			}
			foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(boundingBox))
			{
				TerrainFeature feature;
				if (location.terrainFeatures.TryGetValue(tile, out feature) && !feature.isPassable(null))
				{
					terrainFeature = feature;
					return true;
				}
			}
			return !location.isTileOnMap(this.position.Value / 64f) || (!this.ignoreLocationCollision.Value && location.isCollidingPosition(boundingBox, Game1.viewport, false, 0, true, this.theOneWhoFiredMe.Get(location), false, true, false, false));
		}

		// Token: 0x06001D45 RID: 7493
		public abstract void updatePosition(GameTime time);

		// Token: 0x06001D46 RID: 7494 RVA: 0x0014F5EC File Offset: 0x0014D7EC
		public virtual Rectangle getBoundingBox()
		{
			Vector2 pos = this.position.Value;
			int damageSize = this.boundingBoxWidth.Value + (this.damagesMonsters.Value ? 8 : 0);
			float current_scale = this.localScale;
			damageSize = (int)((float)damageSize * current_scale);
			return new Rectangle((int)pos.X + 32 - damageSize / 2, (int)pos.Y + 32 - damageSize / 2, damageSize, damageSize);
		}

		// Token: 0x06001D47 RID: 7495 RVA: 0x0014F654 File Offset: 0x0014D854
		public virtual void draw(SpriteBatch b)
		{
			float current_scale = 4f * this.localScale;
			Texture2D texture = this.GetTexture();
			Rectangle sourceRect = this.GetSourceRect();
			Vector2 pixelPosition = this.position.Value;
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(0f, -this.height.Value) + new Vector2(32f, 32f)), new Rectangle?(sourceRect), this.color.Value * this.alpha.Value, this.rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
			if (this.height.Value > 0f)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * this.alpha.Value * 0.75f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (pixelPosition.Y - 1f) / 10000f);
			}
			float tailAlpha = this.alpha.Value;
			for (int i = this.tail.Count - 1; i >= 0; i--)
			{
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == this.tail.Count - 1) ? pixelPosition : this.tail.ElementAt(i + 1), this.tail.ElementAt(i), (float)this.tailCounter / 50f) + new Vector2(0f, -this.height.Value) + new Vector2(32f, 32f)), new Rectangle?(sourceRect), this.color.Value * tailAlpha, this.rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y - (float)(this.tail.Count - i) + 96f) / 10000f);
				tailAlpha -= 1f / (float)this.tail.Count;
				current_scale = 0.8f * (float)(4 - 4 / (i + 4));
			}
		}

		// Token: 0x06001D48 RID: 7496 RVA: 0x0014F8FD File Offset: 0x0014DAFD
		public Texture2D GetTexture()
		{
			if (this.itemId.Value == null)
			{
				return Projectile.projectileSheet;
			}
			return ItemRegistry.GetDataOrErrorItem(this.itemId.Value).GetTexture();
		}

		// Token: 0x06001D49 RID: 7497 RVA: 0x0014F928 File Offset: 0x0014DB28
		public Rectangle GetSourceRect()
		{
			if (this.itemId.Value == null)
			{
				return Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, this.currentTileSheetIndex.Value, 16, 16);
			}
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(this.itemId.Value);
			string value = this.itemId.Value;
			if (value != null)
			{
				int length = value.Length;
				if (length == 6)
				{
					switch (value[5])
					{
					case '0':
						if (!(value == "(O)390") && !(value == "(O)380"))
						{
							goto IL_104;
						}
						break;
					case '1':
					case '3':
					case '5':
					case '7':
						goto IL_104;
					case '2':
						if (!(value == "(O)382"))
						{
							goto IL_104;
						}
						break;
					case '4':
						if (!(value == "(O)384"))
						{
							goto IL_104;
						}
						break;
					case '6':
						if (!(value == "(O)386"))
						{
							goto IL_104;
						}
						break;
					case '8':
						if (!(value == "(O)388") && !(value == "(O)378"))
						{
							goto IL_104;
						}
						break;
					default:
						goto IL_104;
					}
					return data.GetSourceRect(1, null);
				}
			}
			IL_104:
			return data.GetSourceRect(0, null);
		}

		// Token: 0x040011D6 RID: 4566
		public const int travelTimeBeforeCollisionPossible = 100;

		// Token: 0x040011D7 RID: 4567
		public const int goblinsCurseIndex = 0;

		// Token: 0x040011D8 RID: 4568
		public const int flameBallIndex = 1;

		// Token: 0x040011D9 RID: 4569
		public const int fearBolt = 2;

		// Token: 0x040011DA RID: 4570
		public const int shadowBall = 3;

		// Token: 0x040011DB RID: 4571
		public const int bone = 4;

		// Token: 0x040011DC RID: 4572
		public const int throwingKnife = 5;

		// Token: 0x040011DD RID: 4573
		public const int snowBall = 6;

		// Token: 0x040011DE RID: 4574
		public const int shamanBolt = 7;

		// Token: 0x040011DF RID: 4575
		public const int frostBall = 8;

		// Token: 0x040011E0 RID: 4576
		public const int frozenBolt = 9;

		// Token: 0x040011E1 RID: 4577
		public const int fireball = 10;

		// Token: 0x040011E2 RID: 4578
		public const int slash = 11;

		// Token: 0x040011E3 RID: 4579
		public const int arrowBolt = 12;

		// Token: 0x040011E4 RID: 4580
		public const int launchedSlime = 13;

		// Token: 0x040011E5 RID: 4581
		public const int magicArrow = 14;

		// Token: 0x040011E6 RID: 4582
		public const int iceOrb = 15;

		// Token: 0x040011E7 RID: 4583
		public const string projectileSheetName = "TileSheets\\Projectiles";

		// Token: 0x040011E8 RID: 4584
		public const int timePerTailUpdate = 50;

		// Token: 0x040011E9 RID: 4585
		public readonly NetInt boundingBoxWidth = new NetInt(21);

		// Token: 0x040011EA RID: 4586
		public static Texture2D projectileSheet;

		// Token: 0x040011EB RID: 4587
		protected float startingAlpha = 1f;

		// Token: 0x040011EC RID: 4588
		[XmlIgnore]
		public readonly NetInt currentTileSheetIndex = new NetInt();

		// Token: 0x040011ED RID: 4589
		[XmlIgnore]
		public readonly NetString itemId = new NetString();

		// Token: 0x040011EE RID: 4590
		[XmlIgnore]
		public readonly NetPosition position = new NetPosition();

		// Token: 0x040011EF RID: 4591
		[XmlIgnore]
		public readonly NetInt tailLength = new NetInt();

		// Token: 0x040011F0 RID: 4592
		[XmlIgnore]
		public int tailCounter = 50;

		// Token: 0x040011F1 RID: 4593
		public readonly NetString bounceSound = new NetString();

		// Token: 0x040011F2 RID: 4594
		[XmlIgnore]
		public readonly NetInt bouncesLeft = new NetInt();

		// Token: 0x040011F3 RID: 4595
		public readonly NetInt piercesLeft = new NetInt(1);

		// Token: 0x040011F4 RID: 4596
		public int travelTime;

		// Token: 0x040011F5 RID: 4597
		protected float? _rotation;

		// Token: 0x040011F6 RID: 4598
		[XmlIgnore]
		public float hostTimeUntilAttackable = -1f;

		// Token: 0x040011F7 RID: 4599
		public readonly NetFloat startingRotation = new NetFloat();

		// Token: 0x040011F8 RID: 4600
		[XmlIgnore]
		public readonly NetFloat rotationVelocity = new NetFloat();

		// Token: 0x040011F9 RID: 4601
		public readonly NetFloat alpha = new NetFloat(1f);

		// Token: 0x040011FA RID: 4602
		public readonly NetFloat alphaChange = new NetFloat(0f);

		// Token: 0x040011FB RID: 4603
		[XmlIgnore]
		public readonly NetFloat xVelocity = new NetFloat();

		// Token: 0x040011FC RID: 4604
		[XmlIgnore]
		public readonly NetFloat yVelocity = new NetFloat();

		// Token: 0x040011FD RID: 4605
		public readonly NetVector2 acceleration = new NetVector2();

		// Token: 0x040011FE RID: 4606
		public readonly NetFloat maxVelocity = new NetFloat(-1f);

		// Token: 0x040011FF RID: 4607
		public readonly NetColor color = new NetColor(Color.White);

		// Token: 0x04001200 RID: 4608
		[XmlIgnore]
		public Queue<Vector2> tail = new Queue<Vector2>();

		// Token: 0x04001201 RID: 4609
		public readonly NetInt maxTravelDistance = new NetInt(-1);

		// Token: 0x04001202 RID: 4610
		public float travelDistance;

		// Token: 0x04001203 RID: 4611
		public readonly NetInt projectileID = new NetInt(-1);

		// Token: 0x04001204 RID: 4612
		public readonly NetInt uniqueID = new NetInt(-1);

		// Token: 0x04001205 RID: 4613
		public NetFloat height = new NetFloat(0f);

		// Token: 0x04001206 RID: 4614
		[XmlIgnore]
		public readonly NetBool damagesMonsters = new NetBool();

		// Token: 0x04001207 RID: 4615
		[XmlIgnore]
		public readonly NetCharacterRef theOneWhoFiredMe = new NetCharacterRef();

		// Token: 0x04001208 RID: 4616
		public readonly NetBool ignoreTravelGracePeriod = new NetBool(false);

		// Token: 0x04001209 RID: 4617
		public readonly NetBool ignoreLocationCollision = new NetBool();

		// Token: 0x0400120A RID: 4618
		public readonly NetBool ignoreObjectCollisions = new NetBool();

		// Token: 0x0400120B RID: 4619
		public readonly NetBool ignoreMeleeAttacks = new NetBool(false);

		// Token: 0x0400120C RID: 4620
		public readonly NetBool ignoreCharacterCollisions = new NetBool(false);

		// Token: 0x0400120D RID: 4621
		public bool destroyMe;

		// Token: 0x0400120E RID: 4622
		public readonly NetFloat startingScale = new NetFloat(1f);

		// Token: 0x0400120F RID: 4623
		protected float? _localScale;

		// Token: 0x04001210 RID: 4624
		public readonly NetFloat scaleGrow = new NetFloat(0f);

		// Token: 0x04001211 RID: 4625
		public NetBool light = new NetBool();

		// Token: 0x04001212 RID: 4626
		public bool hasLit;

		// Token: 0x04001213 RID: 4627
		[XmlIgnore]
		public string lightSourceId;
	}
}

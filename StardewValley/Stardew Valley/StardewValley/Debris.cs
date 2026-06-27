using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x02000092 RID: 146
	public class Debris : INetObject<NetFields>
	{
		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000651 RID: 1617 RVA: 0x000222E4 File Offset: 0x000204E4
		// (set) Token: 0x06000652 RID: 1618 RVA: 0x000222F1 File Offset: 0x000204F1
		public int itemQuality
		{
			get
			{
				return this.netItemQuality.Value;
			}
			set
			{
				this.netItemQuality.Value = value;
			}
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000653 RID: 1619 RVA: 0x000222FF File Offset: 0x000204FF
		// (set) Token: 0x06000654 RID: 1620 RVA: 0x0002230C File Offset: 0x0002050C
		public int chunkFinalYLevel
		{
			get
			{
				return this.netChunkFinalYLevel.Value;
			}
			set
			{
				this.netChunkFinalYLevel.Value = value;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000655 RID: 1621 RVA: 0x0002231A File Offset: 0x0002051A
		// (set) Token: 0x06000656 RID: 1622 RVA: 0x00022327 File Offset: 0x00020527
		public int chunkFinalYTarget
		{
			get
			{
				return this.netChunkFinalYTarget.Value;
			}
			set
			{
				this.netChunkFinalYTarget.Value = value;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000657 RID: 1623 RVA: 0x00022335 File Offset: 0x00020535
		// (set) Token: 0x06000658 RID: 1624 RVA: 0x00022342 File Offset: 0x00020542
		public bool chunksMoveTowardPlayer
		{
			get
			{
				return this._chunksMoveTowardsPlayer.Value;
			}
			set
			{
				this._chunksMoveTowardsPlayer.Value = value;
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x00022350 File Offset: 0x00020550
		public Texture2D spriteChunkSheet
		{
			get
			{
				if (this._spriteChunkSheet == null && this.spriteChunkSheetName.Value != null)
				{
					this._spriteChunkSheet = Game1.content.Load<Texture2D>(this.spriteChunkSheetName.Value);
				}
				return this._spriteChunkSheet;
			}
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x0600065A RID: 1626 RVA: 0x00022388 File Offset: 0x00020588
		// (set) Token: 0x0600065B RID: 1627 RVA: 0x00022395 File Offset: 0x00020595
		public Item item
		{
			get
			{
				return this.netItem.Value;
			}
			set
			{
				this.netItem.Value = value;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x0600065C RID: 1628 RVA: 0x000223A3 File Offset: 0x000205A3
		public NetFields NetFields { get; } = new NetFields("Debris");

		// Token: 0x0600065D RID: 1629 RVA: 0x000223AC File Offset: 0x000205AC
		public Debris()
		{
			this.InitNetFields();
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x000224DC File Offset: 0x000206DC
		public virtual void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.chunks, "chunks").AddField(this.chunkType, "chunkType").AddField(this.sizeOfSourceRectSquares, "sizeOfSourceRectSquares").AddField(this.netItemQuality, "netItemQuality").AddField(this.netChunkFinalYLevel, "netChunkFinalYLevel").AddField(this.netChunkFinalYTarget, "netChunkFinalYTarget").AddField(this.scale, "scale").AddField(this.floppingFish, "floppingFish").AddField(this.debrisType, "debrisType").AddField(this.isSinking, "isSinking").AddField(this.debrisMessage, "debrisMessage").AddField(this.nonSpriteChunkColor, "nonSpriteChunkColor").AddField(this.chunksColor, "chunksColor").AddField(this.spriteChunkSheetName, "spriteChunkSheetName").AddField(this.netItem, "netItem").AddField(this.player.NetFields, "player.NetFields").AddField(this.DroppedByPlayerID, "DroppedByPlayerID").AddField(this._chunksMoveTowardsPlayer, "_chunksMoveTowardsPlayer").AddField(this.itemId, "itemId");
			this.player.Delayed(false);
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x0600065F RID: 1631 RVA: 0x00022638 File Offset: 0x00020838
		public NetObjectShrinkList<Chunk> Chunks
		{
			get
			{
				return this.chunks;
			}
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x00022640 File Offset: 0x00020840
		public Debris(int debris_type, Vector2 debrisOrigin, Vector2 playerPosition) : this(debris_type, 1, debrisOrigin, playerPosition, 1f)
		{
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x00022651 File Offset: 0x00020851
		public Debris(int resource_type, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f) : this()
		{
			this.InitializeResource(resource_type);
			this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, velocityMultiplyer);
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0002266C File Offset: 0x0002086C
		public Debris(int debrisType, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, Color? color = null) : this()
		{
			this.debrisType.Value = Debris.DebrisType.CHUNKS;
			this.chunkType.Value = debrisType;
			this.chunksColor.Value = (color ?? Debris.getColorForDebris(debrisType));
			this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, 1f);
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x000226CC File Offset: 0x000208CC
		public Debris(string item_id, Vector2 debrisOrigin, Vector2 playerPosition) : this(item_id, 1, debrisOrigin, playerPosition, 1f)
		{
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x000226DD File Offset: 0x000208DD
		public Debris(string item_id, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f) : this()
		{
			this.InitializeItem(item_id);
			this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, velocityMultiplyer);
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x000226F8 File Offset: 0x000208F8
		public virtual void InitializeItem(string item_id)
		{
			if (this.debrisType.Value == Debris.DebrisType.CHUNKS)
			{
				this.debrisType.Value = Debris.DebrisType.OBJECT;
			}
			this.itemId.Value = item_id;
			ParsedItemData data = ItemRegistry.GetData(this.itemId.Value);
			if (this.item == null)
			{
				if (data.HasTypeObject())
				{
					this.floppingFish.Value = (data.Category == -4 && data.InternalName != "Mussel");
					this.isFishable = (data.ObjectType == "Fish");
					if (data.ObjectType == "Arch")
					{
						this.debrisType.Value = Debris.DebrisType.ARCHAEOLOGY;
						return;
					}
				}
				else
				{
					this.item = ItemRegistry.Create(this.itemId.Value, 1, 0, false);
				}
			}
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x000227C4 File Offset: 0x000209C4
		public virtual void InitializeResource(int item_id)
		{
			this.debrisType.Value = Debris.DebrisType.OBJECT;
			switch (item_id)
			{
			case 0:
				break;
			case 1:
			case 3:
			case 5:
				goto IL_16B;
			case 2:
				goto IL_B1;
			case 4:
				goto IL_14D;
			case 6:
				goto IL_D2;
			default:
				switch (item_id)
				{
				case 10:
					break;
				case 11:
				case 13:
					goto IL_16B;
				case 12:
					goto IL_111;
				case 14:
					goto IL_12F;
				default:
					switch (item_id)
					{
					case 378:
						goto IL_90;
					case 379:
					case 381:
					case 383:
					case 385:
					case 387:
					case 389:
						goto IL_16B;
					case 380:
						goto IL_B1;
					case 382:
						goto IL_14D;
					case 384:
						goto IL_D2;
					case 386:
						break;
					case 388:
						goto IL_111;
					case 390:
						goto IL_12F;
					default:
						goto IL_16B;
					}
					break;
				}
				this.itemId.Value = "(O)386";
				this.debrisType.Value = Debris.DebrisType.RESOURCE;
				goto IL_187;
				IL_111:
				this.itemId.Value = "(O)388";
				this.debrisType.Value = Debris.DebrisType.RESOURCE;
				goto IL_187;
				IL_12F:
				this.itemId.Value = "(O)390";
				this.debrisType.Value = Debris.DebrisType.RESOURCE;
				goto IL_187;
			}
			IL_90:
			this.itemId.Value = "(O)378";
			this.debrisType.Value = Debris.DebrisType.RESOURCE;
			goto IL_187;
			IL_B1:
			this.itemId.Value = "(O)380";
			this.debrisType.Value = Debris.DebrisType.RESOURCE;
			goto IL_187;
			IL_D2:
			this.itemId.Value = "(O)384";
			this.debrisType.Value = Debris.DebrisType.RESOURCE;
			goto IL_187;
			IL_14D:
			this.itemId.Value = "(O)382";
			this.debrisType.Value = Debris.DebrisType.RESOURCE;
			goto IL_187;
			IL_16B:
			this.itemId.Value = "(O)" + item_id.ToString();
			IL_187:
			if (this.itemId.Value != null)
			{
				this.InitializeItem(this.itemId.Value);
			}
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x00022976 File Offset: 0x00020B76
		public Debris(Item item, Vector2 debrisOrigin) : this()
		{
			this.item = item;
			item.resetState();
			this.InitializeItem(item.QualifiedItemId);
			this.InitializeChunks(1, debrisOrigin, Utility.PointToVector2(Game1.player.StandingPixel), 1f);
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x000229B3 File Offset: 0x00020BB3
		public Debris(Item item, Vector2 debrisOrigin, Vector2 targetLocation) : this()
		{
			this.item = item;
			item.resetState();
			this.InitializeItem(item.QualifiedItemId);
			this.InitializeChunks(1, debrisOrigin, targetLocation, 1f);
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x000229E4 File Offset: 0x00020BE4
		public Debris(int number, Vector2 debrisOrigin, Color messageColor, float scale, Character toHover) : this()
		{
			this.chunkType.Value = number;
			this.debrisType.Value = Debris.DebrisType.NUMBERS;
			this.nonSpriteChunkColor.Value = messageColor;
			this.InitializeChunks(1, debrisOrigin, Game1.player.Position, 1f);
			this.chunks[0].scale = scale;
			this.toHover = toHover;
			this.chunks[0].xVelocity.Value = (float)Game1.random.Next(-1, 2);
			this.updateHoverPosition(this.chunks[0]);
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x00022A84 File Offset: 0x00020C84
		public Debris(string message, int numberOfChunks, Vector2 debrisOrigin, Color messageColor, float scale, float rotation) : this()
		{
			this.debrisType.Value = Debris.DebrisType.LETTERS;
			this.debrisMessage.Value = message;
			this.nonSpriteChunkColor.Value = messageColor;
			this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position, 1f);
			this.chunks[0].rotation = rotation;
			this.chunks[0].scale = scale;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x00022AFC File Offset: 0x00020CFC
		public Debris(string spriteSheet, int numberOfChunks, Vector2 debrisOrigin) : this()
		{
			this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position, 1f);
			this.debrisType.Value = Debris.DebrisType.SPRITECHUNKS;
			this.spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < this.chunks.Count; i++)
			{
				Chunk chunk = this.chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(0, 56);
				chunk.ySpriteSheet.Value = Game1.random.Next(0, 88);
				chunk.scale = 1f;
			}
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x00022B9C File Offset: 0x00020D9C
		public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin) : this()
		{
			this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position, 1f);
			this.debrisType.Value = Debris.DebrisType.SPRITECHUNKS;
			this.spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < this.chunks.Count; i++)
			{
				Chunk chunk = this.chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
				chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
				chunk.scale = 1f;
			}
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x00022C60 File Offset: 0x00020E60
		public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, int sizeOfSourceRectSquares) : this()
		{
			this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position, 0.6f);
			this.sizeOfSourceRectSquares.Value = sizeOfSourceRectSquares;
			this.debrisType.Value = Debris.DebrisType.SPRITECHUNKS;
			this.spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < this.chunks.Count; i++)
			{
				Chunk chunk = this.chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.X;
				chunk.ySpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.Y;
				chunk.rotationVelocity = (Game1.random.NextBool() ? ((float)(3.141592653589793 / (double)Game1.random.Next(-32, -16))) : ((float)(3.141592653589793 / (double)Game1.random.Next(16, 32))));
				chunk.xVelocity.Value *= 1.2f;
				chunk.yVelocity.Value *= 1.2f;
				chunk.scale = 4f;
			}
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x00022D98 File Offset: 0x00020F98
		public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel) : this()
		{
			this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, 1f);
			this.debrisType.Value = Debris.DebrisType.SPRITECHUNKS;
			this.spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < this.chunks.Count; i++)
			{
				Chunk chunk = this.chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
				chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
				chunk.scale = 1f;
			}
			this.chunkFinalYLevel = groundLevel;
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x00022E5C File Offset: 0x0002105C
		public virtual bool isEssentialItem()
		{
			if (!(this.itemId.Value == "(O)73"))
			{
				Item item = this.item;
				if (!(((item != null) ? item.QualifiedItemId : null) == "(O)73"))
				{
					return this.item != null && !this.item.canBeTrashed();
				}
			}
			return true;
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x00022EB8 File Offset: 0x000210B8
		public virtual bool collect(Farmer farmer, Chunk chunk = null)
		{
			if (this.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY)
			{
				Game1.farmerFindsArtifact(this.itemId.Value);
			}
			else if (this.item != null)
			{
				Item tmpItem = this.item;
				this.item = null;
				if (!farmer.addItemToInventoryBool(tmpItem, false))
				{
					this.item = tmpItem;
					return false;
				}
			}
			else if ((this.debrisType.Value != Debris.DebrisType.CHUNKS || this.chunkType.Value != 8) && !farmer.addItemToInventoryBool(ItemRegistry.Create(this.itemId.Value, 1, this.itemQuality, false), false))
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x00022F50 File Offset: 0x00021150
		public static Color getColorForDebris(int type)
		{
			if (type == 12)
			{
				return new Color(170, 106, 46);
			}
			switch (type)
			{
			case 100001:
				return Color.LightGreen;
			case 100002:
				return Color.LightBlue;
			case 100003:
				return Color.Red;
			case 100004:
				return Color.Yellow;
			case 100005:
				return Color.Black;
			case 100006:
				return Color.Gray;
			case 100007:
				return Color.DimGray;
			default:
				return Color.White;
			}
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x00022FCC File Offset: 0x000211CC
		public void InitializeChunks(int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
		{
			if (this.itemId.Value != null || this.chunkType.Value != -1)
			{
				playerPosition -= (playerPosition - debrisOrigin) * 2f;
			}
			int minYVelocity;
			int maxYVelocity;
			int minXVelocity;
			int maxXVelocity;
			if (playerPosition.Y >= debrisOrigin.Y - 32f && playerPosition.Y <= debrisOrigin.Y + 32f)
			{
				this.chunkFinalYLevel = (int)debrisOrigin.Y - 32;
				minYVelocity = 250;
				maxYVelocity = 300;
				if (playerPosition.X < debrisOrigin.X)
				{
					minXVelocity = 20;
					maxXVelocity = 110;
				}
				else
				{
					minXVelocity = -110;
					maxXVelocity = -20;
				}
			}
			else if (playerPosition.Y < debrisOrigin.Y - 32f)
			{
				this.chunkFinalYLevel = (int)debrisOrigin.Y + (int)(32f * velocityMultiplyer);
				minYVelocity = 180;
				maxYVelocity = 230;
				minXVelocity = -50;
				maxXVelocity = 50;
			}
			else
			{
				this.movingFinalYLevel = true;
				this.chunkFinalYLevel = (int)debrisOrigin.Y - 1;
				this.chunkFinalYTarget = (int)debrisOrigin.Y - (int)(96f * velocityMultiplyer);
				this.movingUp = true;
				minYVelocity = 350;
				maxYVelocity = 400;
				minXVelocity = -50;
				maxXVelocity = 50;
			}
			debrisOrigin.X -= 32f;
			debrisOrigin.Y -= 32f;
			minXVelocity = (int)((float)minXVelocity * velocityMultiplyer);
			maxXVelocity = (int)((float)maxXVelocity * velocityMultiplyer);
			minYVelocity = (int)((float)minYVelocity * velocityMultiplyer);
			maxYVelocity = (int)((float)maxYVelocity * velocityMultiplyer);
			for (int i = 0; i < numberOfChunks; i++)
			{
				this.chunks.Add(new Chunk(debrisOrigin, (float)Game1.recentMultiplayerRandom.Next(minXVelocity, maxXVelocity) / 40f, (float)Game1.recentMultiplayerRandom.Next(minYVelocity, maxYVelocity) / 40f, Game1.recentMultiplayerRandom.Next(0, 2)));
			}
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0002318C File Offset: 0x0002138C
		private Vector2 approximatePosition()
		{
			Vector2 total = default(Vector2);
			foreach (Chunk chunk in this.Chunks)
			{
				total += chunk.position.Value;
			}
			return total / (float)this.Chunks.Count;
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x00023204 File Offset: 0x00021404
		private bool playerInRange(Vector2 position, Farmer farmer)
		{
			if (this.isEssentialItem())
			{
				return true;
			}
			int applied_magnetic_radius = farmer.GetAppliedMagneticRadius();
			Point playerPixel = farmer.StandingPixel;
			return Math.Abs(position.X + 32f - (float)playerPixel.X) <= (float)applied_magnetic_radius && Math.Abs(position.Y + 32f - (float)playerPixel.Y) <= (float)applied_magnetic_radius;
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x00023268 File Offset: 0x00021468
		private Farmer findBestPlayer(GameLocation location)
		{
			bool? flag = (location != null) ? new bool?(location.IsTemporary) : null;
			if (flag != null && flag.GetValueOrDefault())
			{
				return Game1.player;
			}
			Vector2 position = this.approximatePosition();
			float bestDistance = float.MaxValue;
			Farmer bestFarmer = null;
			foreach (Farmer farmer in location.farmers)
			{
				if ((farmer.UniqueMultiplayerID != this.DroppedByPlayerID.Value || bestFarmer == null) && this.playerInRange(position, farmer))
				{
					float distance = (farmer.Position - position).LengthSquared();
					if (distance < bestDistance || (bestFarmer != null && bestFarmer.UniqueMultiplayerID == this.DroppedByPlayerID.Value))
					{
						bestFarmer = farmer;
						bestDistance = distance;
					}
				}
			}
			return bestFarmer;
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x00023358 File Offset: 0x00021558
		public bool shouldControlThis(GameLocation location)
		{
			return Game1.IsMasterGame || (((location != null) ? new bool?(location.IsTemporary) : null) ?? false);
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0002339C File Offset: 0x0002159C
		public bool updateChunks(GameTime time, GameLocation location)
		{
			if (this.chunks.Count == 0)
			{
				return true;
			}
			this.timeSinceDoneBouncing += (float)time.ElapsedGameTime.Milliseconds;
			if (this.timeSinceDoneBouncing >= (this.floppingFish.Value ? 2500f : ((this.debrisType.Value == Debris.DebrisType.SPRITECHUNKS || this.debrisType.Value == Debris.DebrisType.NUMBERS) ? 1800f : 600f)))
			{
				switch (this.debrisType.Value)
				{
				case Debris.DebrisType.CHUNKS:
					if (this.chunkType.Value != 8)
					{
						return true;
					}
					this.chunksMoveTowardPlayer = true;
					break;
				case Debris.DebrisType.LETTERS:
				case Debris.DebrisType.SPRITECHUNKS:
				case Debris.DebrisType.NUMBERS:
					return true;
				case Debris.DebrisType.ARCHAEOLOGY:
				case Debris.DebrisType.OBJECT:
				case Debris.DebrisType.RESOURCE:
					this.chunksMoveTowardPlayer = true;
					break;
				}
				this.timeSinceDoneBouncing = 0f;
			}
			if (!location.farmers.Any() && !location.IsTemporary)
			{
				return false;
			}
			Vector2 position = this.approximatePosition();
			Farmer farmer = this.player.Value;
			if (this.isEssentialItem() && this.shouldControlThis(location) && farmer == null)
			{
				farmer = this.findBestPlayer(location);
			}
			if (this.chunksMoveTowardPlayer)
			{
				if (this.timeBeforeReturnToDroppingPlayer > 0)
				{
					this.timeBeforeReturnToDroppingPlayer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				}
				if (!this.isEssentialItem())
				{
					if (this.player.Value != null && this.player.Value == Game1.player && !this.playerInRange(position, this.player.Value))
					{
						this.player.Value = null;
						farmer = null;
					}
					if (this.shouldControlThis(location))
					{
						if (this.player.Value != null && this.player.Value.currentLocation != location)
						{
							this.player.Value = null;
							farmer = null;
						}
						if (farmer == null)
						{
							farmer = this.findBestPlayer(location);
						}
					}
					if (farmer != null && this.timeBeforeReturnToDroppingPlayer > 0 && farmer.UniqueMultiplayerID == this.DroppedByPlayerID.Value)
					{
						farmer = null;
					}
				}
			}
			bool anyCouldMove = false;
			for (int i = this.chunks.Count - 1; i >= 0; i--)
			{
				Chunk chunk = this.chunks[i];
				chunk.position.UpdateExtrapolation(chunk.getSpeed());
				if (chunk.alpha > 0.1f && (this.debrisType.Value == Debris.DebrisType.SPRITECHUNKS || this.debrisType.Value == Debris.DebrisType.NUMBERS) && this.timeSinceDoneBouncing > 600f)
				{
					chunk.alpha = (1800f - this.timeSinceDoneBouncing) / 1000f;
				}
				if (chunk.position.X < -128f || chunk.position.Y < -64f || chunk.position.X >= (float)(location.map.DisplayWidth + 64) || chunk.position.Y >= (float)(location.map.DisplayHeight + 64))
				{
					this.chunks.RemoveAt(i);
				}
				else
				{
					Item item = this.item;
					if (((item != null) ? item.QualifiedItemId : null) == "(O)GoldCoin")
					{
						this.animationTimer += (float)((int)time.ElapsedGameTime.TotalMilliseconds);
						if (this.animationTimer > 700f)
						{
							this.animationTimer = 0f;
							location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), 100f, 6, 1, Utility.getRandomPositionInThisRectangle(new Rectangle((int)chunk.position.X + 32 - 4, (int)chunk.position.Y + 32 - 4, 32, 28), Game1.random), false, false, ((float)(this.chunkFinalYLevel + 64 + 8) + (chunk.position.X + 1f) / 10000f) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
						}
					}
					bool canMoveTowardPlayer = farmer != null;
					if (canMoveTowardPlayer)
					{
						Debris.DebrisType value = this.debrisType.Value;
						if (value - Debris.DebrisType.ARCHAEOLOGY > 1)
						{
							canMoveTowardPlayer = (value != Debris.DebrisType.RESOURCE || farmer.couldInventoryAcceptThisItem(this.itemId.Value, 1, 0));
						}
						else if (this.item != null)
						{
							canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(this.item);
						}
						else
						{
							canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(this.itemId.Value, 1, this.itemQuality);
							if (this.itemId.Value == "(O)102" && farmer.hasMenuOpen.Value)
							{
								canMoveTowardPlayer = false;
							}
						}
						anyCouldMove = (anyCouldMove || canMoveTowardPlayer);
						if (canMoveTowardPlayer && this.shouldControlThis(location))
						{
							this.player.Value = farmer;
						}
					}
					if ((this.chunksMoveTowardPlayer || this.isFishable) && canMoveTowardPlayer && this.player.Value != null)
					{
						if (this.player.Value.IsLocalPlayer)
						{
							if (chunk.position.X < this.player.Value.Position.X - 12f)
							{
								chunk.xVelocity.Value = Math.Min(chunk.xVelocity.Value + 0.8f, 8f);
							}
							else if (chunk.position.X > this.player.Value.Position.X + 12f)
							{
								chunk.xVelocity.Value = Math.Max(chunk.xVelocity.Value - 0.8f, -8f);
							}
							int playerStandingY = this.player.Value.StandingPixel.Y;
							if (chunk.position.Y + 32f < (float)(playerStandingY - 12))
							{
								chunk.yVelocity.Value = Math.Max(chunk.yVelocity.Value - 0.8f, -8f);
							}
							else if (chunk.position.Y + 32f > (float)(playerStandingY + 12))
							{
								chunk.yVelocity.Value = Math.Min(chunk.yVelocity.Value + 0.8f, 8f);
							}
							chunk.position.X += chunk.xVelocity.Value;
							chunk.position.Y -= chunk.yVelocity.Value;
							Point playerPixel = this.player.Value.StandingPixel;
							if (Math.Abs(chunk.position.X + 32f - (float)playerPixel.X) <= 64f && Math.Abs(chunk.position.Y + 32f - (float)playerPixel.Y) <= 64f)
							{
								Item old = this.item;
								if (this.collect(this.player.Value, chunk))
								{
									if (Game1.debrisSoundInterval <= 0f)
									{
										Game1.debrisSoundInterval = 10f;
										if (((old != null) ? old.QualifiedItemId : null) != "(O)73" && this.itemId.Value != "(O)73")
										{
											location.localSound("coin", null, null, SoundContext.Default);
										}
									}
									this.chunks.RemoveAt(i);
								}
							}
						}
					}
					else
					{
						if (this.debrisType.Value == Debris.DebrisType.NUMBERS)
						{
							this.updateHoverPosition(chunk);
						}
						chunk.position.X += chunk.xVelocity.Value;
						chunk.position.Y -= chunk.yVelocity.Value;
						if (this.movingFinalYLevel)
						{
							this.chunkFinalYLevel -= (int)Math.Ceiling((double)(chunk.yVelocity.Value / 2f));
							if (this.chunkFinalYLevel <= this.chunkFinalYTarget)
							{
								this.chunkFinalYLevel = this.chunkFinalYTarget;
								this.movingFinalYLevel = false;
							}
						}
						if (chunk.bounces <= (this.floppingFish.Value ? 65 : 2))
						{
							if (this.debrisType.Value == Debris.DebrisType.SPRITECHUNKS)
							{
								chunk.yVelocity.Value -= 0.25f;
							}
							else
							{
								chunk.yVelocity.Value -= 0.4f;
							}
						}
						bool destroyThisChunk = false;
						if (chunk.position.Y >= (float)this.chunkFinalYLevel && chunk.hasPassedRestingLineOnce.Value)
						{
							Vector2 chunkTile = new Vector2((float)((int)((chunk.position.X + 32f) / 64f)), (float)((int)((chunk.position.Y + 32f) / 64f)));
							bool stillBouncing = chunk.bounces <= (this.floppingFish.Value ? 65 : 2);
							if (stillBouncing)
							{
								Point tile_point = new Point((int)chunk.position.X / 64, this.chunkFinalYLevel / 64);
								if (Game1.currentLocation is IslandNorth && (this.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY || this.debrisType.Value == Debris.DebrisType.OBJECT || this.debrisType.Value == Debris.DebrisType.RESOURCE || this.debrisType.Value == Debris.DebrisType.CHUNKS) && Game1.currentLocation.isTileOnMap(tile_point.X, tile_point.Y) && !Game1.currentLocation.hasTileAt(tile_point, "Back", null))
								{
									this.chunkFinalYLevel += 48;
								}
								chunk.bounces++;
								if (this.floppingFish.Value)
								{
									chunk.yVelocity.Value = Math.Abs(chunk.yVelocity.Value) * ((this.movingUp && chunk.bounces < 2) ? 0.6f : 0.9f);
									chunk.xVelocity.Value = (float)Game1.random.Next(-250, 250) / 100f;
								}
								else
								{
									chunk.yVelocity.Value = Math.Abs(chunk.yVelocity.Value * 2f / 3f);
									chunk.rotationVelocity = (Game1.random.NextBool() ? (chunk.rotationVelocity / 2f) : (-chunk.rotationVelocity * 2f / 3f));
									chunk.xVelocity.Value -= chunk.xVelocity.Value / 2f;
								}
								if (this.debrisType.Value != Debris.DebrisType.LETTERS && this.debrisType.Value != Debris.DebrisType.SPRITECHUNKS && this.debrisType.Value != Debris.DebrisType.NUMBERS && location.doesTileSinkDebris((int)chunkTile.X, (int)chunkTile.Y, this.debrisType.Value))
								{
									destroyThisChunk = location.sinkDebris(this, chunkTile, chunk.position.Value);
									if (this.isSinking.Value)
									{
										chunk.xVelocity.Value = 0f;
										chunk.yVelocity.Value = 0f;
									}
								}
								else if (this.debrisType.Value != Debris.DebrisType.LETTERS && this.debrisType.Value != Debris.DebrisType.NUMBERS && this.debrisType.Value != Debris.DebrisType.SPRITECHUNKS && (this.debrisType.Value != Debris.DebrisType.CHUNKS || this.chunkType.Value == 8) && this.shouldControlThis(location))
								{
									location.playSound("shiny4", null, null, SoundContext.Default);
								}
							}
							if (this.isSinking.Value)
							{
								if (!stillBouncing)
								{
									chunk.bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(position.X / 32f)) * 4f;
								}
								chunk.sinkTimer.Value -= time.ElapsedGameTime.Milliseconds;
								if (chunk.sinkTimer.Value <= 0)
								{
									destroyThisChunk = location.sinkDebris(this, chunkTile, chunk.position.Value);
								}
							}
						}
						int tile_x = (int)((chunk.position.X + 32f) / 64f);
						int tile_y = (int)((chunk.position.Y + 32f) / 64f);
						if ((!chunk.hitWall && location.Map.RequireLayer("Buildings").Tiles[tile_x, tile_y] != null && location.doesTileHaveProperty(tile_x, tile_y, "Passable", "Buildings", false) == null) || location.Map.RequireLayer("Back").Tiles[tile_x, tile_y] == null)
						{
							chunk.xVelocity.Value = -chunk.xVelocity.Value;
							chunk.hitWall = true;
						}
						if (chunk.position.Y < (float)this.chunkFinalYLevel)
						{
							chunk.hasPassedRestingLineOnce.Value = true;
						}
						if (chunk.bounces > (this.floppingFish.Value ? 65 : 2))
						{
							chunk.yVelocity.Value = 0f;
							chunk.xVelocity.Value = 0f;
							chunk.rotationVelocity = 0f;
						}
						chunk.rotation += chunk.rotationVelocity;
						if (destroyThisChunk)
						{
							this.chunks.RemoveAt(i);
						}
					}
				}
			}
			if (!anyCouldMove && this.shouldControlThis(location))
			{
				this.player.Value = null;
			}
			return this.chunks.Count == 0;
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x00024170 File Offset: 0x00022370
		public void updateHoverPosition(Chunk chunk)
		{
			if (this.toHover != null)
			{
				this.relativeXPosition += chunk.xVelocity.Value;
				chunk.position.X = this.toHover.Position.X + 32f + this.relativeXPosition;
				chunk.scale = Math.Min(2f, Math.Max(1f, 0.9f + Math.Abs(chunk.position.Y - (float)this.chunkFinalYLevel) / 128f));
				this.chunkFinalYLevel = this.toHover.StandingPixel.Y + 8;
				if (this.timeSinceDoneBouncing > 250f)
				{
					chunk.alpha = Math.Max(0f, chunk.alpha - 0.033f);
				}
				if (!(this.toHover is Farmer) && !this.nonSpriteChunkColor.Equals(Color.Yellow) && !this.nonSpriteChunkColor.Equals(Color.Green))
				{
					this.nonSpriteChunkColor.R = (byte)Math.Max((double)Math.Min(255, 200 + this.chunkType.Value), Math.Min((double)Math.Min(255, 220 + this.chunkType.Value), 400.0 * Math.Sin((double)this.timeSinceDoneBouncing / 804.247719318987 + 0.2617993877991494)));
					this.nonSpriteChunkColor.G = (byte)Math.Max((double)(150 - this.chunkType.Value), Math.Min((double)(255 - this.chunkType.Value), (this.nonSpriteChunkColor.R > 220) ? (300.0 * Math.Sin((double)this.timeSinceDoneBouncing / 804.247719318987 + 0.2617993877991494)) : 0.0));
					this.nonSpriteChunkColor.B = (byte)Math.Max(0, Math.Min(255, (int)((this.nonSpriteChunkColor.G > 200) ? (this.nonSpriteChunkColor.G - 20) : 0)));
				}
			}
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x000243BC File Offset: 0x000225BC
		public static string getNameOfDebrisTypeFromIntId(int id)
		{
			switch (id)
			{
			case 0:
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.621");
			case 2:
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.622");
			case 4:
			case 5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.623");
			case 6:
			case 7:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.624");
			case 8:
			case 9:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.625");
			case 10:
			case 11:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.626");
			case 12:
			case 13:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.627");
			case 14:
			case 15:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.628");
			case 28:
			case 29:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.629");
			case 30:
			case 31:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.630");
			}
			return "???";
		}

		// Token: 0x040002F3 RID: 755
		public const int copperDebris = 0;

		// Token: 0x040002F4 RID: 756
		public const int ironDebris = 2;

		// Token: 0x040002F5 RID: 757
		public const int coalDebris = 4;

		// Token: 0x040002F6 RID: 758
		public const int goldDebris = 6;

		// Token: 0x040002F7 RID: 759
		public const int coinsDebris = 8;

		// Token: 0x040002F8 RID: 760
		public const int iridiumDebris = 10;

		// Token: 0x040002F9 RID: 761
		public const int woodDebris = 12;

		// Token: 0x040002FA RID: 762
		public const int stoneDebris = 14;

		// Token: 0x040002FB RID: 763
		public const int bigStoneDebris = 32;

		// Token: 0x040002FC RID: 764
		public const int bigWoodDebris = 34;

		// Token: 0x040002FD RID: 765
		public const int timesToBounce = 2;

		// Token: 0x040002FE RID: 766
		public const float gravity = 0.4f;

		// Token: 0x040002FF RID: 767
		public const float timeToWaitBeforeRemoval = 600f;

		// Token: 0x04000300 RID: 768
		public const int marginForChunkPickup = 64;

		// Token: 0x04000301 RID: 769
		public const int white = 10000;

		// Token: 0x04000302 RID: 770
		public const int green = 100001;

		// Token: 0x04000303 RID: 771
		public const int blue = 100002;

		// Token: 0x04000304 RID: 772
		public const int red = 100003;

		// Token: 0x04000305 RID: 773
		public const int yellow = 100004;

		// Token: 0x04000306 RID: 774
		public const int black = 100005;

		// Token: 0x04000307 RID: 775
		public const int charcoal = 100007;

		// Token: 0x04000308 RID: 776
		public const int gray = 100006;

		// Token: 0x04000309 RID: 777
		private float relativeXPosition;

		// Token: 0x0400030A RID: 778
		private readonly NetObjectShrinkList<Chunk> chunks = new NetObjectShrinkList<Chunk>();

		// Token: 0x0400030B RID: 779
		public readonly NetInt chunkType = new NetInt();

		// Token: 0x0400030C RID: 780
		public readonly NetInt sizeOfSourceRectSquares = new NetInt(8);

		// Token: 0x0400030D RID: 781
		private readonly NetInt netItemQuality = new NetInt();

		// Token: 0x0400030E RID: 782
		private readonly NetInt netChunkFinalYLevel = new NetInt();

		// Token: 0x0400030F RID: 783
		private readonly NetInt netChunkFinalYTarget = new NetInt();

		// Token: 0x04000310 RID: 784
		public float timeSinceDoneBouncing;

		// Token: 0x04000311 RID: 785
		public readonly NetFloat scale = new NetFloat(1f).Interpolated(true, true);

		// Token: 0x04000312 RID: 786
		protected NetBool _chunksMoveTowardsPlayer = new NetBool(false).Interpolated(false, false);

		// Token: 0x04000313 RID: 787
		public readonly NetLong DroppedByPlayerID = new NetLong().Interpolated(false, false);

		// Token: 0x04000314 RID: 788
		private bool movingUp;

		// Token: 0x04000315 RID: 789
		public readonly NetBool floppingFish = new NetBool();

		// Token: 0x04000316 RID: 790
		public bool isFishable;

		// Token: 0x04000317 RID: 791
		public bool movingFinalYLevel;

		// Token: 0x04000318 RID: 792
		public readonly NetEnum<Debris.DebrisType> debrisType = new NetEnum<Debris.DebrisType>(Debris.DebrisType.CHUNKS);

		// Token: 0x04000319 RID: 793
		public readonly NetBool isSinking = new NetBool(false);

		// Token: 0x0400031A RID: 794
		public readonly NetString debrisMessage = new NetString("");

		// Token: 0x0400031B RID: 795
		public readonly NetColor nonSpriteChunkColor = new NetColor(Color.White);

		// Token: 0x0400031C RID: 796
		public readonly NetColor chunksColor = new NetColor();

		// Token: 0x0400031D RID: 797
		private float animationTimer;

		// Token: 0x0400031E RID: 798
		private int timeBeforeReturnToDroppingPlayer = 1200;

		// Token: 0x0400031F RID: 799
		public readonly NetString spriteChunkSheetName = new NetString();

		// Token: 0x04000320 RID: 800
		private Texture2D _spriteChunkSheet;

		// Token: 0x04000321 RID: 801
		public readonly NetString itemId = new NetString();

		// Token: 0x04000322 RID: 802
		private readonly NetRef<Item> netItem = new NetRef<Item>();

		// Token: 0x04000323 RID: 803
		public Character toHover;

		// Token: 0x04000324 RID: 804
		public readonly NetFarmerRef player = new NetFarmerRef();

		// Token: 0x02000408 RID: 1032
		public enum DebrisType
		{
			// Token: 0x04002704 RID: 9988
			CHUNKS,
			// Token: 0x04002705 RID: 9989
			LETTERS,
			// Token: 0x04002706 RID: 9990
			ARCHAEOLOGY = 3,
			// Token: 0x04002707 RID: 9991
			OBJECT,
			// Token: 0x04002708 RID: 9992
			SPRITECHUNKS,
			// Token: 0x04002709 RID: 9993
			RESOURCE,
			// Token: 0x0400270A RID: 9994
			NUMBERS
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;

namespace StardewValley
{
	// Token: 0x0200010A RID: 266
	public class TemporaryAnimatedSprite
	{
		// Token: 0x0600154B RID: 5451 RVA: 0x000F9E85 File Offset: 0x000F8085
		public static float GetFireworkLifetimeMultiplier(int id)
		{
			return TemporaryAnimatedSprite.FireworksLifetimeMultiplier[id];
		}

		// Token: 0x0600154C RID: 5452 RVA: 0x000F9E8E File Offset: 0x000F808E
		public static Color GetFireworkColor(int id)
		{
			return TemporaryAnimatedSprite.FireworksColors[id];
		}

		// Token: 0x0600154D RID: 5453 RVA: 0x000F9E9B File Offset: 0x000F809B
		public static Vector2[] GetFireworkLights(int id)
		{
			return TemporaryAnimatedSprite.FireworksLights[id];
		}

		// Token: 0x0600154E RID: 5454 RVA: 0x000F9EA4 File Offset: 0x000F80A4
		public static Vector2[] GetFireworkPoints(int id)
		{
			return TemporaryAnimatedSprite.FireworksPoints[id];
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x0600154F RID: 5455 RVA: 0x000F9EAD File Offset: 0x000F80AD
		public bool Pooled
		{
			get
			{
				return this._pooled;
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06001550 RID: 5456 RVA: 0x000F9EB5 File Offset: 0x000F80B5
		// (set) Token: 0x06001551 RID: 5457 RVA: 0x000F9EBD File Offset: 0x000F80BD
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = value;
			}
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06001552 RID: 5458 RVA: 0x000F9EC6 File Offset: 0x000F80C6
		public Texture2D Texture
		{
			get
			{
				return this.texture;
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06001553 RID: 5459 RVA: 0x000F9ECE File Offset: 0x000F80CE
		// (set) Token: 0x06001554 RID: 5460 RVA: 0x000F9ED6 File Offset: 0x000F80D6
		public GameLocation Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;
			}
		}

		// Token: 0x06001555 RID: 5461 RVA: 0x000F9EE0 File Offset: 0x000F80E0
		public TemporaryAnimatedSprite getClone()
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite();
			temporaryAnimatedSprite.texture = this.texture;
			temporaryAnimatedSprite.interval = this.interval;
			temporaryAnimatedSprite.currentParentTileIndex = this.currentParentTileIndex;
			temporaryAnimatedSprite.oldCurrentParentTileIndex = this.oldCurrentParentTileIndex;
			temporaryAnimatedSprite.initialParentTileIndex = this.initialParentTileIndex;
			temporaryAnimatedSprite.totalNumberOfLoops = this.totalNumberOfLoops;
			temporaryAnimatedSprite.currentNumberOfLoops = this.currentNumberOfLoops;
			temporaryAnimatedSprite.xStopCoordinate = this.xStopCoordinate;
			temporaryAnimatedSprite.yStopCoordinate = this.yStopCoordinate;
			temporaryAnimatedSprite.animationLength = this.animationLength;
			temporaryAnimatedSprite.bombRadius = this.bombRadius;
			temporaryAnimatedSprite.bombDamage = this.bombDamage;
			temporaryAnimatedSprite.pingPongMotion = this.pingPongMotion;
			temporaryAnimatedSprite.fireworkType = this.fireworkType;
			temporaryAnimatedSprite.flicker = this.flicker;
			temporaryAnimatedSprite.timeBasedMotion = this.timeBasedMotion;
			temporaryAnimatedSprite.overrideLocationDestroy = this.overrideLocationDestroy;
			temporaryAnimatedSprite.pingPong = this.pingPong;
			temporaryAnimatedSprite.holdLastFrame = this.holdLastFrame;
			temporaryAnimatedSprite.extraInfoForEndBehavior = this.extraInfoForEndBehavior;
			temporaryAnimatedSprite.lightId = this.lightId;
			temporaryAnimatedSprite.acceleration = this.acceleration;
			temporaryAnimatedSprite.accelerationChange = this.accelerationChange;
			temporaryAnimatedSprite.alpha = this.alpha;
			temporaryAnimatedSprite.alphaFade = this.alphaFade;
			temporaryAnimatedSprite.attachedCharacter = this.attachedCharacter;
			temporaryAnimatedSprite.bigCraftable = this.bigCraftable;
			temporaryAnimatedSprite.color = this.color;
			temporaryAnimatedSprite.delayBeforeAnimationStart = this.delayBeforeAnimationStart;
			temporaryAnimatedSprite.ticksBeforeAnimationStart = this.ticksBeforeAnimationStart;
			temporaryAnimatedSprite.destroyable = this.destroyable;
			temporaryAnimatedSprite.endFunction = this.endFunction;
			temporaryAnimatedSprite.endSound = this.endSound;
			temporaryAnimatedSprite.flash = this.flash;
			temporaryAnimatedSprite.flipped = this.flipped;
			temporaryAnimatedSprite.hasLit = this.hasLit;
			temporaryAnimatedSprite.id = this.id;
			temporaryAnimatedSprite.initialPosition = this.initialPosition;
			temporaryAnimatedSprite.lightFade = this.lightFade;
			temporaryAnimatedSprite.local = this.local;
			temporaryAnimatedSprite.motion = this.motion;
			temporaryAnimatedSprite.owner = this.owner;
			temporaryAnimatedSprite.parent = this.parent;
			temporaryAnimatedSprite.parentSprite = this.parentSprite;
			temporaryAnimatedSprite.position = this.position;
			temporaryAnimatedSprite.rotation = this.rotation;
			temporaryAnimatedSprite.rotationChange = this.rotationChange;
			temporaryAnimatedSprite.scale = this.scale;
			temporaryAnimatedSprite.scaleChange = this.scaleChange;
			temporaryAnimatedSprite.scaleChangeChange = this.scaleChangeChange;
			temporaryAnimatedSprite.shakeIntensity = this.shakeIntensity;
			temporaryAnimatedSprite.shakeIntensityChange = this.shakeIntensityChange;
			temporaryAnimatedSprite.sourceRect = this.sourceRect;
			temporaryAnimatedSprite.sourceRectStartingPos = this.sourceRectStartingPos;
			temporaryAnimatedSprite.startSound = this.startSound;
			temporaryAnimatedSprite.timeBasedMotion = this.timeBasedMotion;
			temporaryAnimatedSprite.verticalFlipped = this.verticalFlipped;
			temporaryAnimatedSprite.xPeriodic = this.xPeriodic;
			temporaryAnimatedSprite.xPeriodicLoopTime = this.xPeriodicLoopTime;
			temporaryAnimatedSprite.xPeriodicRange = this.xPeriodicRange;
			temporaryAnimatedSprite.yPeriodic = this.yPeriodic;
			temporaryAnimatedSprite.yPeriodicLoopTime = this.yPeriodicLoopTime;
			temporaryAnimatedSprite.yPeriodicRange = this.yPeriodicRange;
			temporaryAnimatedSprite.yStopCoordinate = this.yStopCoordinate;
			temporaryAnimatedSprite.totalNumberOfLoops = this.totalNumberOfLoops;
			temporaryAnimatedSprite.stopAcceleratingWhenVelocityIsZero = this.stopAcceleratingWhenVelocityIsZero;
			temporaryAnimatedSprite.afterAccelStopMotionX = this.afterAccelStopMotionX;
			temporaryAnimatedSprite.afterAccelStopMotionY = this.afterAccelStopMotionY;
			temporaryAnimatedSprite.layerDepthOffset = this.layerDepthOffset;
			temporaryAnimatedSprite.positionFollowsAttachedCharacter = this.positionFollowsAttachedCharacter;
			temporaryAnimatedSprite.dontClearOnAreaEntry = this.dontClearOnAreaEntry;
			return temporaryAnimatedSprite;
		}

		// Token: 0x06001556 RID: 5462 RVA: 0x000FA248 File Offset: 0x000F8448
		public virtual void Pool()
		{
			this.timer = 0f;
			this.interval = 200f;
			this.currentParentTileIndex = 0;
			this.oldCurrentParentTileIndex = 0;
			this.initialParentTileIndex = 0;
			this.totalNumberOfLoops = 0;
			this.currentNumberOfLoops = 0;
			this.xStopCoordinate = -1;
			this.yStopCoordinate = -1;
			this.animationLength = 0;
			this.bombRadius = 0;
			this.pingPongMotion = 1;
			this.bombDamage = -1;
			this.fireworkType = -1;
			this.flicker = false;
			this.timeBasedMotion = false;
			this.overrideLocationDestroy = false;
			this.pingPong = false;
			this.holdLastFrame = false;
			this.pulse = false;
			this.extraInfoForEndBehavior = 0;
			this.lightId = null;
			this.bigCraftable = false;
			this.swordswipe = false;
			this.flash = false;
			this.flipped = false;
			this.verticalFlipped = false;
			this.local = false;
			this.hasLit = false;
			this.xPeriodic = false;
			this.yPeriodic = false;
			this.destroyable = true;
			this.paused = false;
			this.stopAcceleratingWhenVelocityIsZero = false;
			this.positionFollowsAttachedCharacter = false;
			this.rotation = 0f;
			this.alpha = 1f;
			this.alphaFade = 0f;
			this.layerDepth = -1f;
			this.scale = 1f;
			this.scaleChange = 0f;
			this.scaleChangeChange = 0f;
			this.rotationChange = 0f;
			this.id = 0;
			this.lightRadius = 0f;
			this.xPeriodicRange = 0f;
			this.yPeriodicRange = 0f;
			this.xPeriodicLoopTime = 0f;
			this.yPeriodicLoopTime = 0f;
			this.shakeIntensityChange = 0f;
			this.shakeIntensity = 0f;
			this.pulseTime = 0f;
			this.pulseAmount = 1.1f;
			this.alphaFadeFade = 0f;
			this.lightFade = -1;
			this.layerDepthOffset = 0f;
			this.afterAccelStopMotionX = 0f;
			this.afterAccelStopMotionY = 0f;
			this.position = Vector2.Zero;
			this.sourceRectStartingPos = Vector2.Zero;
			this.parent = null;
			this.textureName = null;
			this.texture = null;
			this.sourceRect = Rectangle.Empty;
			this.color = Color.White;
			this.lightcolor = Color.White;
			this.owner = null;
			this.motion = Vector2.Zero;
			this.acceleration = Vector2.Zero;
			this.accelerationChange = Vector2.Zero;
			this.initialPosition = Vector2.Zero;
			this.delayBeforeAnimationStart = 0;
			this.ticksBeforeAnimationStart = 0;
			this.startSound = null;
			this.endSound = null;
			this.text = null;
			this.endFunction = null;
			this.reachedStopCoordinate = null;
			this.reachedStopCoordinateSprite = null;
			this.parentSprite = null;
			this.attachedCharacter = null;
			this.pulseTimer = 0f;
			this.originalScale = 0f;
			this.drawAboveAlwaysFront = false;
			this.dontClearOnAreaEntry = false;
			TemporaryAnimatedSprite._pool.Add(this);
		}

		// Token: 0x06001557 RID: 5463 RVA: 0x000FA53C File Offset: 0x000F873C
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite()
		{
			TemporaryAnimatedSprite s = null;
			if (TemporaryAnimatedSprite._pool == null)
			{
				TemporaryAnimatedSprite._pool = new List<TemporaryAnimatedSprite>();
				for (int i = 0; i < 256; i++)
				{
					TemporaryAnimatedSprite newInstance = new TemporaryAnimatedSprite
					{
						_pooled = true
					};
					TemporaryAnimatedSprite._pool.Add(newInstance);
				}
			}
			if (TemporaryAnimatedSprite._pool.Count > 0)
			{
				s = TemporaryAnimatedSprite._pool[TemporaryAnimatedSprite._pool.Count - 1];
				TemporaryAnimatedSprite._pool.RemoveAt(TemporaryAnimatedSprite._pool.Count - 1);
			}
			if (s == null)
			{
				s = new TemporaryAnimatedSprite();
			}
			return s;
		}

		// Token: 0x06001558 RID: 5464 RVA: 0x000FA5C8 File Offset: 0x000F87C8
		public TemporaryAnimatedSprite()
		{
		}

		// Token: 0x06001559 RID: 5465 RVA: 0x000FA67C File Offset: 0x000F887C
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
		{
			TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite();
			if (s.initialParentTileIndex == -1)
			{
				s.swordswipe = true;
				s.currentParentTileIndex = 0;
			}
			else
			{
				s.currentParentTileIndex = initialParentTileIndex;
			}
			s.initialParentTileIndex = initialParentTileIndex;
			s.interval = animationInterval;
			s.totalNumberOfLoops = numberOfLoops;
			s.position = position;
			s.animationLength = animationLength;
			s.flicker = flicker;
			s.flipped = flipped;
			return s;
		}

		// Token: 0x0600155A RID: 5466 RVA: 0x000FA6E4 File Offset: 0x000F88E4
		public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
		{
			if (initialParentTileIndex == -1)
			{
				this.swordswipe = true;
				this.currentParentTileIndex = 0;
			}
			else
			{
				this.currentParentTileIndex = initialParentTileIndex;
			}
			this.initialParentTileIndex = initialParentTileIndex;
			this.interval = animationInterval;
			this.totalNumberOfLoops = numberOfLoops;
			this.position = position;
			this.animationLength = animationLength;
			this.flicker = flicker;
			this.flipped = flipped;
		}

		// Token: 0x0600155B RID: 5467 RVA: 0x000FA7E8 File Offset: 0x000F89E8
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int rowInAnimationTexture, Vector2 position, Color color, int animationLength = 8, bool flipped = false, float animationInterval = 100f, int numberOfLoops = 0, int sourceRectWidth = -1, float layerDepth = -1f, int sourceRectHeight = -1, int delay = 0)
		{
			TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, rowInAnimationTexture * 64, sourceRectWidth, sourceRectHeight), animationInterval, animationLength, numberOfLoops, position, false, flipped, layerDepth, 0f, color, 1f, 0f, 0f, 0f, false);
			if (sourceRectWidth == -1)
			{
				sourceRectWidth = 64;
				s.sourceRect.Width = 64;
			}
			if (sourceRectHeight == -1)
			{
				sourceRectHeight = 64;
				s.sourceRect.Height = 64;
			}
			if (s.layerDepth == -1f)
			{
				s.layerDepth = (s.position.Y + 32f) / 10000f;
			}
			s.delayBeforeAnimationStart = delay;
			return s;
		}

		// Token: 0x0600155C RID: 5468 RVA: 0x000FA894 File Offset: 0x000F8A94
		public TemporaryAnimatedSprite(int rowInAnimationTexture, Vector2 position, Color color, int animationLength = 8, bool flipped = false, float animationInterval = 100f, int numberOfLoops = 0, int sourceRectWidth = -1, float layerDepth = -1f, int sourceRectHeight = -1, int delay = 0) : this("TileSheets\\animations", new Rectangle(0, rowInAnimationTexture * 64, sourceRectWidth, sourceRectHeight), animationInterval, animationLength, numberOfLoops, position, false, flipped, layerDepth, 0f, color, 1f, 0f, 0f, 0f, false)
		{
			if (sourceRectWidth == -1)
			{
				sourceRectWidth = 64;
				this.sourceRect.Width = 64;
			}
			if (sourceRectHeight == -1)
			{
				sourceRectHeight = 64;
				this.sourceRect.Height = 64;
			}
			if (layerDepth == -1f)
			{
				layerDepth = (position.Y + 32f) / 10000f;
			}
			this.delayBeforeAnimationStart = delay;
		}

		// Token: 0x0600155D RID: 5469 RVA: 0x000FA932 File Offset: 0x000F8B32
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, bool verticalFlipped, float rotation)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
			temporaryAnimatedSprite.rotation = rotation;
			temporaryAnimatedSprite.verticalFlipped = verticalFlipped;
			return temporaryAnimatedSprite;
		}

		// Token: 0x0600155E RID: 5470 RVA: 0x000FA953 File Offset: 0x000F8B53
		public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, bool verticalFlipped, float rotation) : this(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
		{
			this.rotation = rotation;
			this.verticalFlipped = verticalFlipped;
		}

		// Token: 0x0600155F RID: 5471 RVA: 0x000FA978 File Offset: 0x000F8B78
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool bigCraftable, bool flipped)
		{
			TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
			s.bigCraftable = bigCraftable;
			if (s.bigCraftable)
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite = s;
				temporaryAnimatedSprite.position.Y = temporaryAnimatedSprite.position.Y - 64f;
			}
			return s;
		}

		// Token: 0x06001560 RID: 5472 RVA: 0x000FA9BA File Offset: 0x000F8BBA
		public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool bigCraftable, bool flipped) : this(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
		{
			this.bigCraftable = bigCraftable;
			if (bigCraftable)
			{
				this.position.Y = this.position.Y - 64f;
			}
		}

		// Token: 0x06001561 RID: 5473 RVA: 0x000FA9F0 File Offset: 0x000F8BF0
		public TemporaryAnimatedSprite GetTemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
			temporaryAnimatedSprite.textureName = textureName;
			temporaryAnimatedSprite.loadTexture();
			temporaryAnimatedSprite.sourceRect = sourceRect;
			temporaryAnimatedSprite.sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y);
			temporaryAnimatedSprite.initialPosition = position;
			return temporaryAnimatedSprite;
		}

		// Token: 0x06001562 RID: 5474 RVA: 0x000FAA44 File Offset: 0x000F8C44
		public TemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped) : this(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
		{
			this.textureName = textureName;
			this.loadTexture();
			this.sourceRect = sourceRect;
			this.sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y);
			this.initialPosition = position;
		}

		// Token: 0x06001563 RID: 5475 RVA: 0x000FAA98 File Offset: 0x000F8C98
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, float layerDepth, float alphaFade, Color color, float scale, float scaleChange, float rotation, float rotationChange, bool local = false)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
			temporaryAnimatedSprite.textureName = textureName;
			temporaryAnimatedSprite.loadTexture();
			temporaryAnimatedSprite.sourceRect = sourceRect;
			temporaryAnimatedSprite.sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y);
			temporaryAnimatedSprite.layerDepth = layerDepth;
			temporaryAnimatedSprite.alphaFade = Math.Max(0f, alphaFade);
			temporaryAnimatedSprite.color = color;
			temporaryAnimatedSprite.scale = scale;
			temporaryAnimatedSprite.scaleChange = scaleChange;
			temporaryAnimatedSprite.rotation = rotation;
			temporaryAnimatedSprite.rotationChange = rotationChange;
			temporaryAnimatedSprite.local = local;
			temporaryAnimatedSprite.initialPosition = position;
			return temporaryAnimatedSprite;
		}

		// Token: 0x06001564 RID: 5476 RVA: 0x000FAB34 File Offset: 0x000F8D34
		public TemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, float layerDepth, float alphaFade, Color color, float scale, float scaleChange, float rotation, float rotationChange, bool local = false) : this(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
		{
			this.textureName = textureName;
			this.loadTexture();
			this.sourceRect = sourceRect;
			this.sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y);
			this.layerDepth = layerDepth;
			this.alphaFade = Math.Max(0f, alphaFade);
			this.color = color;
			this.scale = scale;
			this.scaleChange = scaleChange;
			this.rotation = rotation;
			this.rotationChange = rotationChange;
			this.local = local;
			this.initialPosition = position;
		}

		// Token: 0x06001565 RID: 5477 RVA: 0x000FABD4 File Offset: 0x000F8DD4
		public virtual void CopyAppearanceFromItemId(string itemId, int offset = 0)
		{
			this.scale = 4f * this.scale;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(itemId);
			this.textureName = itemData.TextureName;
			this.loadTexture();
			this.sourceRect = itemData.GetSourceRect(offset, null);
			this.sourceRectStartingPos = Utility.PointToVector2(this.sourceRect.Location);
			this.currentParentTileIndex = 0;
			this.initialParentTileIndex = 0;
		}

		// Token: 0x06001566 RID: 5478 RVA: 0x000FAC48 File Offset: 0x000F8E48
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(string textureName, Rectangle sourceRect, Vector2 position, bool flipped, float alphaFade, Color color)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(0, 999999f, 1, 0, position, false, flipped);
			temporaryAnimatedSprite.textureName = textureName;
			temporaryAnimatedSprite.loadTexture();
			temporaryAnimatedSprite.sourceRect = sourceRect;
			temporaryAnimatedSprite.sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y);
			temporaryAnimatedSprite.initialPosition = position;
			temporaryAnimatedSprite.alphaFade = Math.Max(0f, alphaFade);
			temporaryAnimatedSprite.color = color;
			return temporaryAnimatedSprite;
		}

		// Token: 0x06001567 RID: 5479 RVA: 0x000FACB4 File Offset: 0x000F8EB4
		public TemporaryAnimatedSprite(string textureName, Rectangle sourceRect, Vector2 position, bool flipped, float alphaFade, Color color) : this(0, 999999f, 1, 0, position, false, flipped)
		{
			this.textureName = textureName;
			this.loadTexture();
			this.sourceRect = sourceRect;
			this.sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y);
			this.initialPosition = position;
			this.alphaFade = Math.Max(0f, alphaFade);
			this.color = color;
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x000FAD24 File Offset: 0x000F8F24
		public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, GameLocation parent, Farmer owner)
		{
			TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
			s.position.X = (float)((int)s.position.X);
			s.position.Y = (float)((int)s.position.Y);
			s.parent = parent;
			switch (s.initialParentTileIndex)
			{
			case 286:
				s.bombRadius = 3;
				break;
			case 287:
				s.bombRadius = 5;
				break;
			case 288:
				s.bombRadius = 7;
				break;
			}
			s.owner = owner;
			return s;
		}

		// Token: 0x06001569 RID: 5481 RVA: 0x000FADBC File Offset: 0x000F8FBC
		public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, GameLocation parent, Farmer owner) : this(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
		{
			this.position.X = (float)((int)this.position.X);
			this.position.Y = (float)((int)this.position.Y);
			this.parent = parent;
			switch (initialParentTileIndex)
			{
			case 286:
				this.bombRadius = 3;
				break;
			case 287:
				this.bombRadius = 5;
				break;
			case 288:
				this.bombRadius = 7;
				break;
			}
			this.owner = owner;
		}

		// Token: 0x0600156A RID: 5482 RVA: 0x000FAE50 File Offset: 0x000F9050
		private void loadTexture()
		{
			string text = this.textureName;
			if (text == null)
			{
				this.texture = null;
				return;
			}
			if (!(text == ""))
			{
				this.texture = Game1.content.Load<Texture2D>(this.textureName);
				return;
			}
			this.texture = Game1.staminaRect;
		}

		// Token: 0x0600156B RID: 5483 RVA: 0x000FAEA0 File Offset: 0x000F90A0
		public void Read(BinaryReader reader, GameLocation location)
		{
			this.timer = 0f;
			BitArray bitArray = reader.ReadBitArray();
			int i = 0;
			if (bitArray[i++])
			{
				this.interval = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.currentParentTileIndex = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.oldCurrentParentTileIndex = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.initialParentTileIndex = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.totalNumberOfLoops = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.currentNumberOfLoops = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.xStopCoordinate = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.yStopCoordinate = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.animationLength = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.bombRadius = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.bombDamage = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.pingPongMotion = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.fireworkType = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.flicker = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.timeBasedMotion = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.overrideLocationDestroy = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.pingPong = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.holdLastFrame = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.pulse = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.extraInfoForEndBehavior = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.lightId = reader.ReadString();
			}
			if (bitArray[i++])
			{
				this.bigCraftable = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.swordswipe = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.flash = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.flipped = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.verticalFlipped = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.local = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.lightFade = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.hasLit = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.xPeriodic = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.yPeriodic = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.destroyable = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.paused = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.rotation = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.alpha = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.alphaFade = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.layerDepth = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.scale = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.scaleChange = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.scaleChangeChange = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.rotationChange = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.id = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.lightRadius = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.xPeriodicRange = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.yPeriodicRange = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.xPeriodicLoopTime = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.yPeriodicLoopTime = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.shakeIntensityChange = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.shakeIntensity = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.pulseTime = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.pulseAmount = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.position = reader.ReadVector2();
			}
			if (bitArray[i++])
			{
				this.sourceRectStartingPos = reader.ReadVector2();
			}
			if (bitArray[i++])
			{
				this.sourceRect = reader.ReadRectangle();
			}
			if (bitArray[i++])
			{
				this.color = reader.ReadColor();
			}
			if (bitArray[i++])
			{
				this.lightcolor = reader.ReadColor();
			}
			if (bitArray[i++])
			{
				this.motion = reader.ReadVector2();
			}
			if (bitArray[i++])
			{
				this.acceleration = reader.ReadVector2();
			}
			if (bitArray[i++])
			{
				this.accelerationChange = reader.ReadVector2();
			}
			if (bitArray[i++])
			{
				this.initialPosition = reader.ReadVector2();
			}
			if (bitArray[i++])
			{
				this.delayBeforeAnimationStart = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.ticksBeforeAnimationStart = reader.ReadInt32();
			}
			if (bitArray[i++])
			{
				this.startSound = reader.ReadString();
			}
			if (bitArray[i++])
			{
				this.endSound = reader.ReadString();
			}
			if (bitArray[i++])
			{
				this.text = reader.ReadString();
			}
			if (bitArray[i++])
			{
				this.textureName = reader.ReadString();
			}
			if (bitArray[i++])
			{
				this.owner = (Game1.GetPlayer(reader.ReadInt64(), false) ?? Game1.MasterPlayer);
			}
			if (bitArray[i++])
			{
				this.stopAcceleratingWhenVelocityIsZero = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.layerDepthOffset = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.afterAccelStopMotionX = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.afterAccelStopMotionY = reader.ReadSingle();
			}
			if (bitArray[i++])
			{
				this.positionFollowsAttachedCharacter = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.dontClearOnAreaEntry = reader.ReadBoolean();
			}
			if (bitArray[i++])
			{
				this.drawAboveAlwaysFront = reader.ReadBoolean();
			}
			this.parent = location;
			this.loadTexture();
			byte b = reader.ReadByte();
			if (b == 1)
			{
				this.attachedCharacter = (Game1.GetPlayer(reader.ReadInt64(), false) ?? Game1.MasterPlayer);
				return;
			}
			if (b != 2)
			{
				return;
			}
			Guid guid = reader.ReadGuid();
			if (!location.characters.ContainsGuid(guid))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to find character with GUID ");
				defaultInterpolatedStringHandler.AppendFormatted<Guid>(guid);
				defaultInterpolatedStringHandler.AppendLiteral(" for TemporaryAniamtedSprite.attachedCharacter");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			this.attachedCharacter = location.characters[guid];
		}

		// Token: 0x0600156C RID: 5484 RVA: 0x000FB6A4 File Offset: 0x000F98A4
		private void checkDirty<T>(BitArray dirtyBits, ref int i, T value, T defaultValue = default(T))
		{
			int num = i;
			i = num + 1;
			dirtyBits[num] = !object.Equals(value, defaultValue);
		}

		// Token: 0x0600156D RID: 5485 RVA: 0x000FB6D8 File Offset: 0x000F98D8
		public void Write(BinaryWriter writer, GameLocation location)
		{
			if (base.GetType() != typeof(TemporaryAnimatedSprite))
			{
				throw new InvalidOperationException("TemporaryAnimatedSprite.Write is not implemented for other types");
			}
			BitArray dirtyBits = new BitArray(80);
			int i = 0;
			this.checkDirty<float>(dirtyBits, ref i, this.interval, 200f);
			this.checkDirty<int>(dirtyBits, ref i, this.currentParentTileIndex, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.oldCurrentParentTileIndex, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.initialParentTileIndex, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.totalNumberOfLoops, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.currentNumberOfLoops, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.xStopCoordinate, -1);
			this.checkDirty<int>(dirtyBits, ref i, this.yStopCoordinate, -1);
			this.checkDirty<int>(dirtyBits, ref i, this.animationLength, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.bombRadius, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.bombDamage, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.pingPongMotion, -1);
			this.checkDirty<int>(dirtyBits, ref i, this.fireworkType, -1);
			this.checkDirty<bool>(dirtyBits, ref i, this.flicker, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.timeBasedMotion, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.overrideLocationDestroy, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.pingPong, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.holdLastFrame, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.pulse, false);
			this.checkDirty<int>(dirtyBits, ref i, this.extraInfoForEndBehavior, 0);
			this.checkDirty<string>(dirtyBits, ref i, this.lightId, null);
			this.checkDirty<bool>(dirtyBits, ref i, this.bigCraftable, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.swordswipe, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.flash, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.flipped, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.verticalFlipped, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.local, false);
			this.checkDirty<int>(dirtyBits, ref i, this.lightFade, 0);
			this.checkDirty<bool>(dirtyBits, ref i, this.hasLit, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.xPeriodic, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.yPeriodic, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.destroyable, true);
			this.checkDirty<bool>(dirtyBits, ref i, this.paused, false);
			this.checkDirty<float>(dirtyBits, ref i, this.rotation, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.alpha, 1f);
			this.checkDirty<float>(dirtyBits, ref i, this.alphaFade, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.layerDepth, -1f);
			this.checkDirty<float>(dirtyBits, ref i, this.scale, 1f);
			this.checkDirty<float>(dirtyBits, ref i, this.scaleChange, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.scaleChangeChange, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.rotationChange, 0f);
			this.checkDirty<int>(dirtyBits, ref i, this.id, 0);
			this.checkDirty<float>(dirtyBits, ref i, this.lightRadius, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.xPeriodicRange, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.yPeriodicRange, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.xPeriodicLoopTime, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.yPeriodicLoopTime, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.shakeIntensityChange, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.shakeIntensity, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.pulseTime, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.pulseAmount, 1.1f);
			this.checkDirty<Vector2>(dirtyBits, ref i, this.position, default(Vector2));
			this.checkDirty<Vector2>(dirtyBits, ref i, this.sourceRectStartingPos, default(Vector2));
			this.checkDirty<Rectangle>(dirtyBits, ref i, this.sourceRect, default(Rectangle));
			this.checkDirty<Color>(dirtyBits, ref i, this.color, Color.White);
			this.checkDirty<Color>(dirtyBits, ref i, this.lightcolor, Color.White);
			this.checkDirty<Vector2>(dirtyBits, ref i, this.motion, Vector2.Zero);
			this.checkDirty<Vector2>(dirtyBits, ref i, this.acceleration, Vector2.Zero);
			this.checkDirty<Vector2>(dirtyBits, ref i, this.accelerationChange, Vector2.Zero);
			this.checkDirty<Vector2>(dirtyBits, ref i, this.initialPosition, default(Vector2));
			this.checkDirty<int>(dirtyBits, ref i, this.delayBeforeAnimationStart, 0);
			this.checkDirty<int>(dirtyBits, ref i, this.ticksBeforeAnimationStart, 0);
			this.checkDirty<string>(dirtyBits, ref i, this.startSound, null);
			this.checkDirty<string>(dirtyBits, ref i, this.endSound, null);
			this.checkDirty<string>(dirtyBits, ref i, this.text, null);
			this.checkDirty<Texture2D>(dirtyBits, ref i, this.texture, null);
			this.checkDirty<Farmer>(dirtyBits, ref i, this.owner, null);
			this.checkDirty<bool>(dirtyBits, ref i, this.stopAcceleratingWhenVelocityIsZero, false);
			this.checkDirty<float>(dirtyBits, ref i, this.layerDepthOffset, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.afterAccelStopMotionX, 0f);
			this.checkDirty<float>(dirtyBits, ref i, this.afterAccelStopMotionY, 0f);
			this.checkDirty<bool>(dirtyBits, ref i, this.positionFollowsAttachedCharacter, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.dontClearOnAreaEntry, false);
			this.checkDirty<bool>(dirtyBits, ref i, this.drawAboveAlwaysFront, false);
			writer.WriteBitArray(dirtyBits);
			i = 0;
			if (dirtyBits[i++])
			{
				writer.Write(this.interval);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.currentParentTileIndex);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.oldCurrentParentTileIndex);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.initialParentTileIndex);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.totalNumberOfLoops);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.currentNumberOfLoops);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.xStopCoordinate);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.yStopCoordinate);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.animationLength);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.bombRadius);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.bombDamage);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.pingPongMotion);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.fireworkType);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.flicker);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.timeBasedMotion);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.overrideLocationDestroy);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.pingPong);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.holdLastFrame);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.pulse);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.extraInfoForEndBehavior);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.lightId);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.bigCraftable);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.swordswipe);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.flash);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.flipped);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.verticalFlipped);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.local);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.lightFade);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.hasLit);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.xPeriodic);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.yPeriodic);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.destroyable);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.paused);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.rotation);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.alpha);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.alphaFade);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.layerDepth);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.scale);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.scaleChange);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.scaleChangeChange);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.rotationChange);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.id);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.lightRadius);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.xPeriodicRange);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.yPeriodicRange);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.xPeriodicLoopTime);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.yPeriodicLoopTime);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.shakeIntensityChange);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.shakeIntensity);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.pulseTime);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.pulseAmount);
			}
			if (dirtyBits[i++])
			{
				writer.WriteVector2(this.position);
			}
			if (dirtyBits[i++])
			{
				writer.WriteVector2(this.sourceRectStartingPos);
			}
			if (dirtyBits[i++])
			{
				writer.WriteRectangle(this.sourceRect);
			}
			if (dirtyBits[i++])
			{
				writer.WriteColor(this.color);
			}
			if (dirtyBits[i++])
			{
				writer.WriteColor(this.lightcolor);
			}
			if (dirtyBits[i++])
			{
				writer.WriteVector2(this.motion);
			}
			if (dirtyBits[i++])
			{
				writer.WriteVector2(this.acceleration);
			}
			if (dirtyBits[i++])
			{
				writer.WriteVector2(this.accelerationChange);
			}
			if (dirtyBits[i++])
			{
				writer.WriteVector2(this.initialPosition);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.delayBeforeAnimationStart);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.ticksBeforeAnimationStart);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.startSound);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.endSound);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.text);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.textureName);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.owner.uniqueMultiplayerID.Value);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.stopAcceleratingWhenVelocityIsZero);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.layerDepthOffset);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.afterAccelStopMotionX);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.afterAccelStopMotionY);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.positionFollowsAttachedCharacter);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.dontClearOnAreaEntry);
			}
			if (dirtyBits[i++])
			{
				writer.Write(this.drawAboveAlwaysFront);
			}
			Character character = this.attachedCharacter;
			if (character == null)
			{
				writer.Write(0);
				return;
			}
			Farmer farmer = character as Farmer;
			if (farmer != null)
			{
				writer.Write(1);
				writer.Write(farmer.UniqueMultiplayerID);
				return;
			}
			NPC npc = character as NPC;
			if (npc == null)
			{
				throw new ArgumentException();
			}
			writer.Write(2);
			writer.WriteGuid(location.characters.GuidOf(npc));
		}

		// Token: 0x0600156E RID: 5486 RVA: 0x000FC3EC File Offset: 0x000FA5EC
		public virtual void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			if (this.local)
			{
				localPosition = true;
			}
			if (this.currentParentTileIndex >= 0 && this.delayBeforeAnimationStart <= 0 && this.ticksBeforeAnimationStart <= 0)
			{
				if (this.text != null)
				{
					if (this.extraInfoForEndBehavior == -777)
					{
						Vector2 v = Game1.GlobalToLocal(this.position);
						SpriteText.drawString(spriteBatch, this.text, (int)v.X, (int)v.Y, 999999, -1, 999999, this.alpha, this.layerDepth, false, -1, "", new Color?(this.color.Equals(Color.White) ? SpriteText.color_White : SpriteText.color_Black), SpriteText.ScrollTextAlignment.Left);
						return;
					}
					spriteBatch.DrawString(Game1.dialogueFont, this.text, localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, this.Position), this.color * this.alpha * extraAlpha, this.rotation, Vector2.Zero, this.scale, SpriteEffects.None, this.layerDepth + this.layerDepthOffset);
					return;
				}
				else if (this.Texture != null)
				{
					if (this.positionFollowsAttachedCharacter && this.attachedCharacter != null)
					{
						spriteBatch.Draw(this.Texture, (localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, this.attachedCharacter.Position + new Vector2((float)((int)this.Position.X + xOffset), (float)((int)this.Position.Y + yOffset)))) + new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)) * this.scale + new Vector2((float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), (float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0)), new Rectangle?(this.sourceRect), this.color * this.alpha * extraAlpha, this.rotation, new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)), this.scale, this.flipped ? SpriteEffects.FlipHorizontally : (this.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None), ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + (float)this.sourceRect.Height) / 10000f)) + this.layerDepthOffset);
						return;
					}
					if (!this.vectorScale.Equals(Vector2.Zero))
					{
						spriteBatch.Draw(this.Texture, (localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.Position.X + xOffset), (float)((int)this.Position.Y + yOffset)))) + new Vector2((float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), (float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0)), new Rectangle?(this.sourceRect), this.color * this.alpha * extraAlpha, this.rotation, new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)), this.vectorScale, this.flipped ? SpriteEffects.FlipHorizontally : (this.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None), ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + (float)this.sourceRect.Height) / 10000f)) + this.layerDepthOffset);
						return;
					}
					spriteBatch.Draw(this.Texture, (localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.Position.X + xOffset), (float)((int)this.Position.Y + yOffset)))) + new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)) * this.scale + new Vector2((float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), (float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0)), new Rectangle?(this.sourceRect), this.color * this.alpha * extraAlpha, this.rotation, new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)), this.scale, this.flipped ? SpriteEffects.FlipHorizontally : (this.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None), ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + (float)this.sourceRect.Height) / 10000f)) + this.layerDepthOffset);
					return;
				}
				else
				{
					if (this.bigCraftable)
					{
						spriteBatch.Draw(Game1.bigCraftableSpriteSheet, localPosition ? this.Position : (Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.Position.X + xOffset), (float)((int)this.Position.Y + yOffset))) + new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2))), new Rectangle?(Object.getSourceRectForBigCraftable(this.currentParentTileIndex)), Color.White * extraAlpha, 0f, new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)), this.scale, SpriteEffects.None, (this.Position.Y + 32f) / 10000f + this.layerDepthOffset);
						return;
					}
					if (!this.swordswipe)
					{
						if (this.attachedCharacter != null)
						{
							if (this.local)
							{
								this.attachedCharacter.Position = new Vector2((float)Game1.viewport.X + this.Position.X, (float)Game1.viewport.Y + this.Position.Y);
							}
							this.attachedCharacter.draw(spriteBatch);
							return;
						}
						spriteBatch.Draw(Game1.objectSpriteSheet, localPosition ? this.Position : (Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.Position.X + xOffset), (float)((int)this.Position.Y + yOffset))) + new Vector2(8f, 8f) * 4f + new Vector2((float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), (float)((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0))), new Rectangle?(GameLocation.getSourceRectForObject(this.currentParentTileIndex)), (this.flash ? (Color.LightBlue * 0.85f) : this.color) * this.alpha * extraAlpha, this.rotation, new Vector2(8f, 8f), 4f * this.scale, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + 32f) / 10000f)) + this.layerDepthOffset);
					}
				}
			}
		}

		// Token: 0x0600156F RID: 5487 RVA: 0x000FCC3C File Offset: 0x000FAE3C
		public void bounce(int extraInfo)
		{
			if ((float)extraInfo <= 1f)
			{
				if (this.extraInfoForEndBehavior != -777)
				{
					this.alphaFade = 0.01f;
				}
				this.motion.X = 0f;
				return;
			}
			this.motion.Y = (float)(-(float)extraInfo) / 2f;
			this.motion.X = this.motion.X / 2f;
			this.rotationChange = this.motion.Y / 50f;
			this.acceleration.Y = 0.7f;
			this.yStopCoordinate = (int)this.initialPosition.Y;
			GameLocation gameLocation = this.parent;
			if (gameLocation == null)
			{
				return;
			}
			gameLocation.playSound("thudStep", null, null, SoundContext.Default);
		}

		// Token: 0x06001570 RID: 5488 RVA: 0x000FCD07 File Offset: 0x000FAF07
		public void unload()
		{
			this.PlaySound(this.endSound);
			TemporaryAnimatedSprite.endBehavior endBehavior = this.endFunction;
			if (endBehavior != null)
			{
				endBehavior(this.extraInfoForEndBehavior);
			}
			if (this.hasLit)
			{
				Utility.removeLightSource(this.lightId);
			}
		}

		// Token: 0x06001571 RID: 5489 RVA: 0x000FCD40 File Offset: 0x000FAF40
		public void reset()
		{
			this.sourceRect.X = (int)this.sourceRectStartingPos.X;
			this.sourceRect.Y = (int)this.sourceRectStartingPos.Y;
			this.currentParentTileIndex = 0;
			this.oldCurrentParentTileIndex = 0;
			this.timer = 0f;
			this.totalTimer = 0f;
			this.currentNumberOfLoops = 0;
			this.pingPongMotion = 1;
		}

		// Token: 0x06001572 RID: 5490 RVA: 0x000FCDAD File Offset: 0x000FAFAD
		public void resetEnd()
		{
			this.reset();
			this.currentParentTileIndex = this.initialParentTileIndex + this.animationLength - 1;
		}

		// Token: 0x06001573 RID: 5491 RVA: 0x000FCDCC File Offset: 0x000FAFCC
		public virtual bool update(GameTime time)
		{
			if (this.paused)
			{
				return false;
			}
			int elapsedMs = (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this.usePreciseTiming)
			{
				if (this.stopWatch == null)
				{
					this.stopWatch = new Stopwatch();
					this.stopWatch.Start();
				}
				elapsedMs = (int)(this.stopWatch.ElapsedMilliseconds - this.previousStopwatchTime);
				this.previousStopwatchTime = this.stopWatch.ElapsedMilliseconds;
			}
			if (this.bombRadius > 0 && !Game1.shouldTimePass(false))
			{
				return false;
			}
			if (this.ticksBeforeAnimationStart > 0)
			{
				this.ticksBeforeAnimationStart--;
				return false;
			}
			if (this.delayBeforeAnimationStart > 0)
			{
				this.delayBeforeAnimationStart -= elapsedMs;
				if (this.delayBeforeAnimationStart <= 0)
				{
					this.PlaySound(this.startSound);
					this.timer = (float)(-(float)this.delayBeforeAnimationStart);
				}
				if (this.delayBeforeAnimationStart <= 0 && this.parentSprite != null)
				{
					this.position = this.parentSprite.position + this.position;
				}
				return false;
			}
			if (float.IsNaN(this.motion.X))
			{
				this.motion.X = 0f;
			}
			if (float.IsNaN(this.motion.Y))
			{
				this.motion.Y = 0f;
			}
			this.timer += (float)elapsedMs;
			this.totalTimer += (float)elapsedMs;
			this.alpha -= this.alphaFade * (float)(this.timeBasedMotion ? elapsedMs : 1);
			this.alphaFade -= this.alphaFadeFade * (float)(this.timeBasedMotion ? elapsedMs : 1);
			if (this.alphaFade > 0f && this.lightId != null && this.alpha < 1f && this.alpha >= 0f)
			{
				LightSource ls = Utility.getLightSource(this.lightId);
				if (ls != null)
				{
					ls.color.A = (byte)(255f * this.alpha);
				}
			}
			this.shakeIntensity += this.shakeIntensityChange * (float)elapsedMs;
			this.scale += this.scaleChange * (float)(this.timeBasedMotion ? elapsedMs : 1);
			this.scaleChange += this.scaleChangeChange * (float)(this.timeBasedMotion ? elapsedMs : 1);
			this.rotation += this.rotationChange;
			if (this.xPeriodic)
			{
				this.position.X = this.initialPosition.X + this.xPeriodicRange * (float)Math.Sin(6.283185307179586 / (double)this.xPeriodicLoopTime * (double)this.totalTimer);
			}
			else
			{
				this.position.X = this.position.X + this.motion.X * (float)(this.timeBasedMotion ? elapsedMs : 1);
			}
			if (this.yPeriodic)
			{
				this.position.Y = this.initialPosition.Y + this.yPeriodicRange * (float)Math.Sin(6.283185307179586 / (double)this.yPeriodicLoopTime * (double)(this.totalTimer + this.yPeriodicLoopTime / 2f));
			}
			else
			{
				this.position.Y = this.position.Y + this.motion.Y * (float)(this.timeBasedMotion ? elapsedMs : 1);
			}
			if (this.attachedCharacter != null && !this.positionFollowsAttachedCharacter)
			{
				if (this.xPeriodic)
				{
					this.attachedCharacter.position.X = this.initialPosition.X + this.xPeriodicRange * (float)Math.Sin(6.283185307179586 / (double)this.xPeriodicLoopTime * (double)this.totalTimer);
				}
				else
				{
					this.attachedCharacter.position.X += this.motion.X * (float)(this.timeBasedMotion ? elapsedMs : 1);
				}
				if (this.yPeriodic)
				{
					this.attachedCharacter.position.Y = this.initialPosition.Y + this.yPeriodicRange * (float)Math.Sin(6.283185307179586 / (double)this.yPeriodicLoopTime * (double)this.totalTimer);
				}
				else
				{
					this.attachedCharacter.position.Y += this.motion.Y * (float)(this.timeBasedMotion ? elapsedMs : 1);
				}
			}
			int sign = Math.Sign(this.motion.X);
			this.motion.X = this.motion.X + this.acceleration.X * (float)(this.timeBasedMotion ? elapsedMs : 1);
			if (this.stopAcceleratingWhenVelocityIsZero && Math.Sign(this.motion.X) != sign)
			{
				this.motion.X = this.afterAccelStopMotionX;
				this.acceleration.X = 0f;
				this.accelerationChange.X = 0f;
			}
			sign = Math.Sign(this.motion.Y);
			this.motion.Y = this.motion.Y + this.acceleration.Y * (float)(this.timeBasedMotion ? elapsedMs : 1);
			if (this.stopAcceleratingWhenVelocityIsZero && Math.Sign(this.motion.Y) != sign)
			{
				this.motion.Y = this.afterAccelStopMotionY;
				this.acceleration.Y = 0f;
				this.accelerationChange.Y = 0f;
			}
			this.acceleration.X = this.acceleration.X + this.accelerationChange.X;
			this.acceleration.Y = this.acceleration.Y + this.accelerationChange.Y;
			if (this.xStopCoordinate != -1 || this.yStopCoordinate != -1)
			{
				int oldY = (int)this.motion.Y;
				if (this.xStopCoordinate != -1 && Math.Abs(this.position.X - (float)this.xStopCoordinate) <= Math.Abs(this.motion.X))
				{
					this.motion.X = 0f;
					this.acceleration.X = 0f;
					this.xStopCoordinate = -1;
				}
				if (this.yStopCoordinate != -1 && Math.Abs(this.position.Y - (float)this.yStopCoordinate) <= Math.Abs(this.motion.Y))
				{
					this.motion.Y = 0f;
					this.acceleration.Y = 0f;
					this.yStopCoordinate = -1;
				}
				if (this.xStopCoordinate == -1 && this.yStopCoordinate == -1)
				{
					this.rotationChange = 0f;
					TemporaryAnimatedSprite.endBehavior endBehavior = this.reachedStopCoordinate;
					if (endBehavior != null)
					{
						endBehavior(oldY);
					}
					Action<TemporaryAnimatedSprite> action = this.reachedStopCoordinateSprite;
					if (action != null)
					{
						action(this);
					}
				}
			}
			if (!this.pingPong)
			{
				this.pingPongMotion = 1;
			}
			if (this.pulse)
			{
				this.pulseTimer -= (float)elapsedMs;
				if (this.originalScale == 0f)
				{
					this.originalScale = this.scale;
				}
				if (this.pulseTimer <= 0f)
				{
					this.pulseTimer = this.pulseTime;
					this.scale = this.originalScale * this.pulseAmount;
				}
				if (this.scale > this.originalScale)
				{
					this.scale -= this.pulseAmount / 100f * (float)elapsedMs;
				}
			}
			if (this.lightId != null)
			{
				if (!this.hasLit)
				{
					this.hasLit = true;
					if (this.parent == null || Game1.currentLocation == this.parent)
					{
						Game1.currentLightSources.Add(new LightSource(this.lightId, 4, this.position + new Vector2(32f, 32f), this.lightRadius, this.lightcolor.Equals(Color.White) ? new Color(0, 65, 128) : this.lightcolor, LightSource.LightContext.None, 0L, null)
						{
							fadeOut = 
							{
								this.lightFade
							}
						});
					}
				}
				else
				{
					Utility.repositionLightSource(this.lightId, this.position + new Vector2(32f, 32f));
				}
			}
			if (this.alpha <= 0f || (this.position.X < -2000f && !this.overrideLocationDestroy) || this.scale <= 0f)
			{
				this.unload();
				return this.destroyable;
			}
			if (this.timer > this.interval)
			{
				this.currentParentTileIndex += this.pingPongMotion;
				this.sourceRect.X = this.sourceRect.X + this.sourceRect.Width * this.pingPongMotion;
				if (this.Texture != null)
				{
					if (!this.pingPong && this.sourceRect.X >= this.Texture.Width)
					{
						this.sourceRect.Y = this.sourceRect.Y + this.sourceRect.Height;
					}
					if (!this.pingPong)
					{
						this.sourceRect.X = this.sourceRect.X % this.Texture.Width;
					}
					if (this.pingPong)
					{
						if ((float)this.sourceRect.X + ((float)this.sourceRect.Y - this.sourceRectStartingPos.Y) / (float)this.sourceRect.Height * (float)this.Texture.Width >= this.sourceRectStartingPos.X + (float)(this.sourceRect.Width * this.animationLength))
						{
							this.pingPongMotion = -1;
							this.sourceRect.X = this.sourceRect.X - this.sourceRect.Width * 2;
							this.currentParentTileIndex--;
							if (this.sourceRect.X < 0)
							{
								this.sourceRect.X = this.Texture.Width + this.sourceRect.X;
							}
						}
						else if ((float)this.sourceRect.X < this.sourceRectStartingPos.X && (float)this.sourceRect.Y == this.sourceRectStartingPos.Y)
						{
							this.pingPongMotion = 1;
							this.sourceRect.X = (int)this.sourceRectStartingPos.X + this.sourceRect.Width;
							this.currentParentTileIndex++;
							this.currentNumberOfLoops++;
							if (this.endFunction != null)
							{
								this.endFunction(this.extraInfoForEndBehavior);
								this.endFunction = null;
							}
							if (this.currentNumberOfLoops >= this.totalNumberOfLoops)
							{
								this.unload();
								return this.destroyable;
							}
						}
					}
					else if (this.totalNumberOfLoops >= 1 && (float)this.sourceRect.X + ((float)this.sourceRect.Y - this.sourceRectStartingPos.Y) / (float)this.sourceRect.Height * (float)this.Texture.Width >= this.sourceRectStartingPos.X + (float)(this.sourceRect.Width * this.animationLength))
					{
						this.sourceRect.X = (int)this.sourceRectStartingPos.X;
						this.sourceRect.Y = (int)this.sourceRectStartingPos.Y;
					}
				}
				this.timer -= this.interval;
				if (this.flicker)
				{
					if (this.currentParentTileIndex < 0 || this.flash)
					{
						this.currentParentTileIndex = this.oldCurrentParentTileIndex;
						this.flash = false;
					}
					else
					{
						this.oldCurrentParentTileIndex = this.currentParentTileIndex;
						if (this.bombRadius > 0)
						{
							this.flash = true;
						}
						else
						{
							this.currentParentTileIndex = -100;
						}
					}
				}
				if (this.currentParentTileIndex - this.initialParentTileIndex >= this.animationLength)
				{
					this.currentNumberOfLoops++;
					if (this.holdLastFrame)
					{
						this.currentParentTileIndex = this.initialParentTileIndex + this.animationLength - 1;
						if (this.texture != null)
						{
							this.setSourceRectToCurrentTileIndex();
						}
						if (this.endFunction != null)
						{
							this.endFunction(this.extraInfoForEndBehavior);
							this.endFunction = null;
						}
						return false;
					}
					this.currentParentTileIndex = this.initialParentTileIndex;
					if (this.currentNumberOfLoops >= this.totalNumberOfLoops)
					{
						if (this.bombRadius > 0)
						{
							if (Game1.currentLocation == this.parent)
							{
								Game1.flashAlpha = 1f;
							}
							if (Game1.IsMasterGame)
							{
								this.parent.netAudio.StopPlaying("fuse");
								this.parent.playSound("explosion", null, null, SoundContext.Default);
								this.parent.explode(new Vector2((float)((int)(this.position.X / 64f)), (float)((int)(this.position.Y / 64f))), this.bombRadius, this.owner, true, this.bombDamage, true);
							}
						}
						if (this.fireworkType >= 0)
						{
							float mult = TemporaryAnimatedSprite.GetFireworkLifetimeMultiplier(this.fireworkType);
							Color col = TemporaryAnimatedSprite.GetFireworkColor(this.fireworkType);
							if (Game1.currentLocation == this.parent)
							{
								Game1.screenGlowOnce(col * 0.8f, false, 0.005f, 0.3f);
							}
							if (Game1.IsMasterGame)
							{
								float outMult = 0.3f;
								float inDiv = (float)this.id;
								Vector2[] fireworkLights = TemporaryAnimatedSprite.GetFireworkLights(this.fireworkType);
								Vector2[] points = TemporaryAnimatedSprite.GetFireworkPoints(this.fireworkType);
								List<TemporaryAnimatedSprite> fireworkSprites = new List<TemporaryAnimatedSprite>();
								foreach (Vector2 point in fireworkLights)
								{
									List<TemporaryAnimatedSprite> list = fireworkSprites;
									TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(0, 0, 1, 1), 1800f * mult, 1, 0, this.position, false, false, -1f, 0f, Color.Transparent, 1f, 0f, 0f, 0f, false);
									temporaryAnimatedSprite.motion = point;
									temporaryAnimatedSprite.acceleration = point * outMult;
									temporaryAnimatedSprite.accelerationChange = -point / inDiv;
									temporaryAnimatedSprite.stopAcceleratingWhenVelocityIsZero = true;
									temporaryAnimatedSprite.afterAccelStopMotionX = (float)Math.Sign(point.X) * 0.1f;
									temporaryAnimatedSprite.afterAccelStopMotionY = 0.33f;
									temporaryAnimatedSprite.layerDepthOffset = 320f;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 3);
									defaultInterpolatedStringHandler.AppendLiteral("Firework_");
									defaultInterpolatedStringHandler.AppendFormatted<int>(this.id);
									defaultInterpolatedStringHandler.AppendLiteral("_");
									defaultInterpolatedStringHandler.AppendFormatted<float>(point.X);
									defaultInterpolatedStringHandler.AppendLiteral("_");
									defaultInterpolatedStringHandler.AppendFormatted<float>(point.Y);
									temporaryAnimatedSprite.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
									temporaryAnimatedSprite.lightRadius = 1.3f;
									temporaryAnimatedSprite.drawAboveAlwaysFront = true;
									temporaryAnimatedSprite.lightFade = 2;
									list.Add(temporaryAnimatedSprite);
								}
								foreach (Vector2 point2 in points)
								{
									fireworkSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(304, 364 + this.fireworkType * 11, 11, 11), 75f * mult + (float)Game1.random.Next(-20, 21), 12, 1, this.position, false, false, -1f, 0f, Color.White, 4f, 0f, (float)(Game1.random.NextDouble() * 3.141592653589793) * 0.5f, 0f, false)
									{
										motion = point2,
										acceleration = point2 * outMult,
										accelerationChange = -point2 / inDiv,
										stopAcceleratingWhenVelocityIsZero = true,
										afterAccelStopMotionX = (float)Math.Sign(point2.X) * 0.1f,
										afterAccelStopMotionY = 0.33f,
										alpha = 1f,
										alphaFade = 0.01f,
										alphaFadeFade = 0.00025f,
										drawAboveAlwaysFront = true
									});
									int which = (Game1.random.Next(3) != 0) ? 1 : 0;
									fireworkSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 64 * (10 + which), 64, 64), 100f * mult, (which == 0) ? 9 : 6, 2, this.position, false, false, -1f, 0f, Utility.getBlendedColor(col, Color.White), 1f, 0f, (float)(Game1.random.NextDouble() * 3.141592653589793) * 0.5f, 0f, false)
									{
										motion = point2 * 0.75f,
										acceleration = point2 * outMult,
										accelerationChange = -point2 / inDiv,
										stopAcceleratingWhenVelocityIsZero = true,
										afterAccelStopMotionX = (float)Math.Sign(point2.X) * 0.1f,
										afterAccelStopMotionY = 0.33f,
										drawAboveAlwaysFront = true,
										alpha = 0.5f,
										delayBeforeAnimationStart = Game1.random.Next(50, 100)
									});
								}
								if (this.id == 30)
								{
									for (int i = 0; i < 8; i++)
									{
										Vector2 mot = points[Game1.random.Next(points.Length)];
										List<TemporaryAnimatedSprite> list2 = fireworkSprites;
										TemporaryAnimatedSprite temporaryAnimatedSprite2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(304, 397, 11, 11), 75f * mult, 12, 5, this.position, false, false, -1f, 0f, Utility.getBlendedColor(Color.White, Utility.getRandomRainbowColor(null)), 4f, 0f, 0f, 0f, false);
										temporaryAnimatedSprite2.motion = mot * 1.1f;
										temporaryAnimatedSprite2.alpha = 1f;
										temporaryAnimatedSprite2.alphaFade = 0.01f;
										temporaryAnimatedSprite2.acceleration = mot * outMult;
										temporaryAnimatedSprite2.accelerationChange = -mot / ((float)this.id * 1.25f);
										temporaryAnimatedSprite2.stopAcceleratingWhenVelocityIsZero = true;
										temporaryAnimatedSprite2.afterAccelStopMotionX = (float)Math.Sign(mot.X) * 0.1f;
										temporaryAnimatedSprite2.afterAccelStopMotionY = 0.33f;
										temporaryAnimatedSprite2.drawAboveAlwaysFront = true;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 2);
										defaultInterpolatedStringHandler.AppendLiteral("Firework_");
										defaultInterpolatedStringHandler.AppendFormatted<int>(this.id);
										defaultInterpolatedStringHandler.AppendLiteral("_");
										defaultInterpolatedStringHandler.AppendFormatted<int>(i);
										temporaryAnimatedSprite2.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
										temporaryAnimatedSprite2.lightRadius = 0.33f;
										temporaryAnimatedSprite2.lightFade = 3;
										list2.Add(temporaryAnimatedSprite2);
									}
								}
								Game1.multiplayer.broadcastSprites(this.parent, fireworkSprites.ToArray());
								this.parent.netAudio.StopPlaying("fuse");
							}
						}
						this.unload();
						return this.destroyable;
					}
					if (this.bombRadius > 0 && this.currentNumberOfLoops == this.totalNumberOfLoops - 5)
					{
						this.interval -= this.interval / 3f;
					}
				}
			}
			return false;
		}

		// Token: 0x06001574 RID: 5492 RVA: 0x000FE0BA File Offset: 0x000FC2BA
		public bool clearOnAreaEntry()
		{
			return !this.dontClearOnAreaEntry && this.bombRadius <= 0;
		}

		// Token: 0x06001575 RID: 5493 RVA: 0x000FE0D4 File Offset: 0x000FC2D4
		private void setSourceRectToCurrentTileIndex()
		{
			this.sourceRect.X = (int)(this.sourceRectStartingPos.X + (float)(this.currentParentTileIndex * this.sourceRect.Width)) % this.texture.Width;
			if (this.sourceRect.X < 0)
			{
				this.sourceRect.X = 0;
			}
			this.sourceRect.Y = (int)this.sourceRectStartingPos.Y;
		}

		// Token: 0x06001576 RID: 5494 RVA: 0x000FE14C File Offset: 0x000FC34C
		private void PlaySound(string sound)
		{
			if (sound == null)
			{
				return;
			}
			if (this.parent == null)
			{
				Game1.playSound(sound, null);
				return;
			}
			this.parent.localSound(sound, null, null, SoundContext.Default);
		}

		// Token: 0x06001577 RID: 5495 RVA: 0x000FE198 File Offset: 0x000FC398
		public static TemporaryAnimatedSprite CreateFromData(TemporaryAnimatedSpriteDefinition temporarySprite, float x, float y, float sortLayer)
		{
			return new TemporaryAnimatedSprite(temporarySprite.Texture, temporarySprite.SourceRect, temporarySprite.Interval, temporarySprite.Frames, temporarySprite.Loops, new Vector2(x, y) * 64f + temporarySprite.PositionOffset * 4f, temporarySprite.Flicker, temporarySprite.Flip, sortLayer + temporarySprite.SortOffset, temporarySprite.AlphaFade, Utility.StringToColor(temporarySprite.Color) ?? Color.White, temporarySprite.Scale * 4f, temporarySprite.ScaleChange, temporarySprite.Rotation, temporarySprite.RotationChange, false);
		}

		// Token: 0x04000D80 RID: 3456
		public const int FireworkType_Heart = 0;

		// Token: 0x04000D81 RID: 3457
		public const int FireworkType_Star = 1;

		// Token: 0x04000D82 RID: 3458
		public const int FireworkType_Junimo = 2;

		// Token: 0x04000D83 RID: 3459
		public static float[] FireworksLifetimeMultiplier = new float[]
		{
			1f,
			1f,
			1.3f
		};

		// Token: 0x04000D84 RID: 3460
		public static Color[] FireworksColors = new Color[]
		{
			new Color(252, 56, 37),
			new Color(144, 51, 237),
			new Color(92, 237, 213)
		};

		// Token: 0x04000D85 RID: 3461
		public static Vector2[][] FireworksLights = new Vector2[][]
		{
			new Vector2[]
			{
				new Vector2(0f, 0f)
			},
			new Vector2[]
			{
				new Vector2(0f, 0f)
			},
			new Vector2[]
			{
				new Vector2(-2.5f, 0f),
				new Vector2(2.5f, 0f)
			}
		};

		// Token: 0x04000D86 RID: 3462
		public static Vector2[][] FireworksPoints = new Vector2[][]
		{
			new Vector2[]
			{
				new Vector2(0f, -3f),
				new Vector2(2f, -5f),
				new Vector2(4f, -5f),
				new Vector2(6f, -3f),
				new Vector2(6f, -1f),
				new Vector2(4f, 1f),
				new Vector2(2f, 3f),
				new Vector2(0f, 5f),
				new Vector2(-2f, 3f),
				new Vector2(-4f, 1f),
				new Vector2(-6f, -1f),
				new Vector2(-6f, -3f),
				new Vector2(-4f, -5f),
				new Vector2(-2f, -5f)
			},
			new Vector2[]
			{
				new Vector2(0f, -6f),
				new Vector2(1f, -4f),
				new Vector2(2f, -2f),
				new Vector2(4f, -2f),
				new Vector2(6f, -2f),
				new Vector2(4f, 0f),
				new Vector2(2f, 1f),
				new Vector2(3f, 3f),
				new Vector2(4f, 5f),
				new Vector2(2f, 4f),
				new Vector2(0f, 3f),
				new Vector2(-2f, 4f),
				new Vector2(-4f, 5f),
				new Vector2(-3f, 3f),
				new Vector2(-2f, 1f),
				new Vector2(-4f, 0f),
				new Vector2(-6f, -2f),
				new Vector2(-4f, -2f),
				new Vector2(-2f, -2f),
				new Vector2(-1f, -4f)
			},
			new Vector2[]
			{
				new Vector2(-1f, -8f),
				new Vector2(0f, -6f),
				new Vector2(0f, -4f),
				new Vector2(2f, -4f),
				new Vector2(4f, -4f),
				new Vector2(6f, -2f),
				new Vector2(8f, -1f),
				new Vector2(9f, -3f),
				new Vector2(8f, -5f),
				new Vector2(6f, 0f),
				new Vector2(6f, 2f),
				new Vector2(3f, 2f),
				new Vector2(3f, 1f),
				new Vector2(5f, 4f),
				new Vector2(3f, 5f),
				new Vector2(3f, 7f),
				new Vector2(1f, 5f),
				new Vector2(-1f, 5f),
				new Vector2(-3f, 7f),
				new Vector2(-3f, 5f),
				new Vector2(-5f, 4f),
				new Vector2(-3f, 2f),
				new Vector2(-3f, 1f),
				new Vector2(-6f, 2f),
				new Vector2(-6f, 0f),
				new Vector2(-8f, -5f),
				new Vector2(-9f, -3f),
				new Vector2(-8f, -1f),
				new Vector2(-6f, -2f),
				new Vector2(-4f, -4f),
				new Vector2(-2f, -4f)
			}
		};

		// Token: 0x04000D87 RID: 3463
		public float timer;

		// Token: 0x04000D88 RID: 3464
		public float interval = 200f;

		// Token: 0x04000D89 RID: 3465
		public int currentParentTileIndex;

		// Token: 0x04000D8A RID: 3466
		public int oldCurrentParentTileIndex;

		// Token: 0x04000D8B RID: 3467
		public int initialParentTileIndex;

		// Token: 0x04000D8C RID: 3468
		public int totalNumberOfLoops;

		// Token: 0x04000D8D RID: 3469
		public int currentNumberOfLoops;

		// Token: 0x04000D8E RID: 3470
		public int xStopCoordinate = -1;

		// Token: 0x04000D8F RID: 3471
		public int yStopCoordinate = -1;

		// Token: 0x04000D90 RID: 3472
		public int animationLength;

		// Token: 0x04000D91 RID: 3473
		public int bombRadius;

		// Token: 0x04000D92 RID: 3474
		public int pingPongMotion = 1;

		// Token: 0x04000D93 RID: 3475
		public int bombDamage = -1;

		// Token: 0x04000D94 RID: 3476
		public int fireworkType = -1;

		// Token: 0x04000D95 RID: 3477
		public bool flicker;

		// Token: 0x04000D96 RID: 3478
		public bool timeBasedMotion;

		// Token: 0x04000D97 RID: 3479
		public bool overrideLocationDestroy;

		// Token: 0x04000D98 RID: 3480
		public bool pingPong;

		// Token: 0x04000D99 RID: 3481
		public bool holdLastFrame;

		// Token: 0x04000D9A RID: 3482
		public bool pulse;

		// Token: 0x04000D9B RID: 3483
		public int extraInfoForEndBehavior;

		// Token: 0x04000D9C RID: 3484
		public string lightId;

		// Token: 0x04000D9D RID: 3485
		public int id;

		// Token: 0x04000D9E RID: 3486
		public bool bigCraftable;

		// Token: 0x04000D9F RID: 3487
		public bool swordswipe;

		// Token: 0x04000DA0 RID: 3488
		public bool flash;

		// Token: 0x04000DA1 RID: 3489
		public bool flipped;

		// Token: 0x04000DA2 RID: 3490
		public bool verticalFlipped;

		// Token: 0x04000DA3 RID: 3491
		public bool local;

		// Token: 0x04000DA4 RID: 3492
		public bool hasLit;

		// Token: 0x04000DA5 RID: 3493
		public bool xPeriodic;

		// Token: 0x04000DA6 RID: 3494
		public bool yPeriodic;

		// Token: 0x04000DA7 RID: 3495
		public bool destroyable = true;

		// Token: 0x04000DA8 RID: 3496
		public bool paused;

		// Token: 0x04000DA9 RID: 3497
		public bool stopAcceleratingWhenVelocityIsZero;

		// Token: 0x04000DAA RID: 3498
		public bool positionFollowsAttachedCharacter;

		// Token: 0x04000DAB RID: 3499
		public bool usePreciseTiming;

		// Token: 0x04000DAC RID: 3500
		public float rotation;

		// Token: 0x04000DAD RID: 3501
		public float alpha = 1f;

		// Token: 0x04000DAE RID: 3502
		public float alphaFade;

		// Token: 0x04000DAF RID: 3503
		public float layerDepth = -1f;

		// Token: 0x04000DB0 RID: 3504
		public float scale = 1f;

		// Token: 0x04000DB1 RID: 3505
		public float scaleChange;

		// Token: 0x04000DB2 RID: 3506
		public float scaleChangeChange;

		// Token: 0x04000DB3 RID: 3507
		public float rotationChange;

		// Token: 0x04000DB4 RID: 3508
		public float lightRadius;

		// Token: 0x04000DB5 RID: 3509
		public float xPeriodicRange;

		// Token: 0x04000DB6 RID: 3510
		public float yPeriodicRange;

		// Token: 0x04000DB7 RID: 3511
		public float xPeriodicLoopTime;

		// Token: 0x04000DB8 RID: 3512
		public float yPeriodicLoopTime;

		// Token: 0x04000DB9 RID: 3513
		public float shakeIntensityChange;

		// Token: 0x04000DBA RID: 3514
		public float shakeIntensity;

		// Token: 0x04000DBB RID: 3515
		public float pulseTime;

		// Token: 0x04000DBC RID: 3516
		public float pulseAmount = 1.1f;

		// Token: 0x04000DBD RID: 3517
		public float alphaFadeFade;

		// Token: 0x04000DBE RID: 3518
		public int lightFade = -1;

		// Token: 0x04000DBF RID: 3519
		public float afterAccelStopMotionX;

		// Token: 0x04000DC0 RID: 3520
		public float afterAccelStopMotionY;

		// Token: 0x04000DC1 RID: 3521
		public float layerDepthOffset;

		// Token: 0x04000DC2 RID: 3522
		public Vector2 position;

		// Token: 0x04000DC3 RID: 3523
		public Vector2 sourceRectStartingPos;

		// Token: 0x04000DC4 RID: 3524
		protected GameLocation parent;

		// Token: 0x04000DC5 RID: 3525
		public string textureName;

		// Token: 0x04000DC6 RID: 3526
		public Texture2D texture;

		// Token: 0x04000DC7 RID: 3527
		public Rectangle sourceRect;

		// Token: 0x04000DC8 RID: 3528
		public Color color = Color.White;

		// Token: 0x04000DC9 RID: 3529
		public Color lightcolor = Color.White;

		// Token: 0x04000DCA RID: 3530
		public Farmer owner;

		// Token: 0x04000DCB RID: 3531
		public Vector2 motion = Vector2.Zero;

		// Token: 0x04000DCC RID: 3532
		public Vector2 acceleration = Vector2.Zero;

		// Token: 0x04000DCD RID: 3533
		public Vector2 accelerationChange = Vector2.Zero;

		// Token: 0x04000DCE RID: 3534
		public Vector2 initialPosition;

		// Token: 0x04000DCF RID: 3535
		public Vector2 vectorScale;

		// Token: 0x04000DD0 RID: 3536
		public int delayBeforeAnimationStart;

		// Token: 0x04000DD1 RID: 3537
		public int ticksBeforeAnimationStart;

		// Token: 0x04000DD2 RID: 3538
		public string startSound;

		// Token: 0x04000DD3 RID: 3539
		public string endSound;

		// Token: 0x04000DD4 RID: 3540
		public string text;

		// Token: 0x04000DD5 RID: 3541
		public TemporaryAnimatedSprite.endBehavior endFunction;

		// Token: 0x04000DD6 RID: 3542
		public TemporaryAnimatedSprite.endBehavior reachedStopCoordinate;

		// Token: 0x04000DD7 RID: 3543
		public Action<TemporaryAnimatedSprite> reachedStopCoordinateSprite;

		// Token: 0x04000DD8 RID: 3544
		public TemporaryAnimatedSprite parentSprite;

		// Token: 0x04000DD9 RID: 3545
		public Character attachedCharacter;

		// Token: 0x04000DDA RID: 3546
		private float pulseTimer;

		// Token: 0x04000DDB RID: 3547
		private float originalScale;

		// Token: 0x04000DDC RID: 3548
		public bool drawAboveAlwaysFront;

		// Token: 0x04000DDD RID: 3549
		public bool dontClearOnAreaEntry;

		// Token: 0x04000DDE RID: 3550
		private Stopwatch stopWatch;

		// Token: 0x04000DDF RID: 3551
		private long previousStopwatchTime;

		// Token: 0x04000DE0 RID: 3552
		protected bool _pooled;

		// Token: 0x04000DE1 RID: 3553
		public static List<TemporaryAnimatedSprite> _pool;

		// Token: 0x04000DE2 RID: 3554
		private float totalTimer;

		// Token: 0x020004DD RID: 1245
		// (Invoke) Token: 0x06003FA4 RID: 16292
		public delegate void endBehavior(int extraInfo);
	}
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;

namespace StardewValley
{
	// Token: 0x020000C4 RID: 196
	[NotImplicitNetField]
	public class LightSource : INetObject<NetFields>
	{
		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000D87 RID: 3463 RVA: 0x00092B4C File Offset: 0x00090D4C
		// (set) Token: 0x06000D88 RID: 3464 RVA: 0x00092B59 File Offset: 0x00090D59
		public string Id
		{
			get
			{
				return this.netId.Value;
			}
			set
			{
				this.netId.Value = value;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06000D89 RID: 3465 RVA: 0x00092B67 File Offset: 0x00090D67
		// (set) Token: 0x06000D8A RID: 3466 RVA: 0x00092B74 File Offset: 0x00090D74
		public long PlayerID
		{
			get
			{
				return this.playerID.Value;
			}
			set
			{
				this.playerID.Value = value;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000D8B RID: 3467 RVA: 0x00092B82 File Offset: 0x00090D82
		public NetFields NetFields { get; } = new NetFields("LightSource");

		// Token: 0x06000D8C RID: 3468 RVA: 0x00092B8C File Offset: 0x00090D8C
		public LightSource()
		{
			this.NetFields.SetOwner(this).AddField(this.textureIndex, "textureIndex").AddField(this.position, "position").AddField(this.color, "color").AddField(this.radius, "radius").AddField(this.netId, "netId").AddField(this.lightContext, "lightContext").AddField(this.playerID, "playerID").AddField(this.fadeOut, "fadeOut").AddField(this.onlyLocation, "onlyLocation");
			this.textureIndex.fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
			{
				this.loadTextureFromConstantValue(newValue);
			};
		}

		// Token: 0x06000D8D RID: 3469 RVA: 0x00092CE0 File Offset: 0x00090EE0
		public LightSource(string id, int textureIndex, Vector2 position, float radius, Color color, LightSource.LightContext lightContext = LightSource.LightContext.None, long playerID = 0L, string onlyLocation = null) : this()
		{
			this.netId.Value = id;
			this.textureIndex.Value = textureIndex;
			this.position.Value = position;
			this.radius.Value = radius;
			this.color.Value = color;
			this.lightContext.Value = lightContext;
			this.playerID.Value = playerID;
			this.onlyLocation.Value = onlyLocation;
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x00092D58 File Offset: 0x00090F58
		public LightSource(string id, int textureIndex, Vector2 position, float radius, LightSource.LightContext lightContext = LightSource.LightContext.None, long playerID = 0L, string onlyLocation = null) : this(id, textureIndex, position, radius, Color.Black, lightContext, playerID, onlyLocation)
		{
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x00092D7C File Offset: 0x00090F7C
		private void loadTextureFromConstantValue(int value)
		{
			switch (value)
			{
			case 1:
				this.lightTexture = Game1.lantern;
				return;
			case 2:
				this.lightTexture = Game1.windowLight;
				return;
			case 3:
				break;
			case 4:
				this.lightTexture = Game1.sconceLight;
				return;
			case 5:
				this.lightTexture = Game1.cauldronLight;
				return;
			case 6:
				this.lightTexture = Game1.indoorWindowLight;
				return;
			case 7:
				this.lightTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Lighting\\projectorLight");
				return;
			case 8:
				this.lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\fishTankLight");
				return;
			case 9:
				this.lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\treeLights");
				return;
			case 10:
				this.lightTexture = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\pinpointLight");
				break;
			default:
				return;
			}
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x00092E50 File Offset: 0x00091050
		public bool IsOnScreen()
		{
			if (this.onlyLocation.Value != null)
			{
				string value = this.onlyLocation.Value;
				GameLocation currentLocation = Game1.currentLocation;
				if (value != ((currentLocation != null) ? currentLocation.NameOrUniqueName : null))
				{
					return false;
				}
			}
			if (!Utility.isOnScreen(this.position.Value, (int)(this.radius.Value * 64f * 4f)))
			{
				return false;
			}
			if (this.PlayerID != 0L && this.PlayerID != Game1.player.UniqueMultiplayerID)
			{
				Farmer farmer = Game1.GetPlayer(this.PlayerID, false);
				if (farmer != null && !farmer.hidden.Value)
				{
					if (farmer.currentLocation == null)
					{
						return true;
					}
					string name = farmer.currentLocation.Name;
					GameLocation currentLocation2 = Game1.currentLocation;
					if (!(name != ((currentLocation2 != null) ? currentLocation2.Name : null)))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x00092F20 File Offset: 0x00091120
		public virtual void Draw(SpriteBatch spriteBatch, GameLocation location, float lightMultiplier)
		{
			if (this.fadeOut.Value > 0)
			{
				if (this.color.Value.A <= 0)
				{
					return;
				}
				this.color.Value = new Color((int)this.color.R, (int)this.color.G, (int)this.color.B, (int)this.color.A - this.fadeOut.Value);
			}
			if (this.lightContext.Value == LightSource.LightContext.WindowLight && (Game1.IsRainingHere(null) || Game1.isTimeToTurnOffLighting(location)))
			{
				this.fadeOut.Value = 4;
			}
			if (!this.IsOnScreen())
			{
				return;
			}
			Texture2D texture = this.lightTexture;
			int lightQuality = Game1.options.lightingQuality;
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, this.position.Value) / (float)(lightQuality / 2), new Rectangle?(texture.Bounds), this.color.Value * lightMultiplier, 0f, new Vector2((float)(texture.Bounds.Width / 2), (float)(texture.Bounds.Height / 2)), this.radius.Value / (float)(lightQuality / 2), SpriteEffects.None, 0.9f);
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x00093060 File Offset: 0x00091260
		public LightSource Clone()
		{
			return new LightSource(this.Id, this.textureIndex.Value, this.position.Value, this.radius.Value, this.color.Value, this.lightContext.Value, this.playerID.Value, null)
			{
				onlyLocation = 
				{
					Value = this.onlyLocation.Value
				}
			};
		}

		// Token: 0x0400090C RID: 2316
		public const int lantern = 1;

		// Token: 0x0400090D RID: 2317
		public const int windowLight = 2;

		// Token: 0x0400090E RID: 2318
		public const int sconceLight = 4;

		// Token: 0x0400090F RID: 2319
		public const int cauldronLight = 5;

		// Token: 0x04000910 RID: 2320
		public const int indoorWindowLight = 6;

		// Token: 0x04000911 RID: 2321
		public const int projectorLight = 7;

		// Token: 0x04000912 RID: 2322
		public const int fishTankLight = 8;

		// Token: 0x04000913 RID: 2323
		public const int townWinterTreeLight = 9;

		// Token: 0x04000914 RID: 2324
		public const int pinpointLight = 10;

		// Token: 0x04000915 RID: 2325
		public readonly NetInt textureIndex = new NetInt().Interpolated(false, false);

		// Token: 0x04000916 RID: 2326
		public Texture2D lightTexture;

		// Token: 0x04000917 RID: 2327
		public readonly NetVector2 position = new NetVector2().Interpolated(true, true);

		// Token: 0x04000918 RID: 2328
		public readonly NetColor color = new NetColor();

		// Token: 0x04000919 RID: 2329
		public readonly NetFloat radius = new NetFloat();

		// Token: 0x0400091A RID: 2330
		public readonly NetString netId = new NetString();

		// Token: 0x0400091B RID: 2331
		public readonly NetEnum<LightSource.LightContext> lightContext = new NetEnum<LightSource.LightContext>();

		// Token: 0x0400091C RID: 2332
		public readonly NetLong playerID = new NetLong(0L).Interpolated(false, false);

		// Token: 0x0400091D RID: 2333
		public readonly NetInt fadeOut = new NetInt(-1);

		// Token: 0x0400091E RID: 2334
		public readonly NetString onlyLocation = new NetString();

		// Token: 0x0200046D RID: 1133
		public enum LightContext
		{
			// Token: 0x04002834 RID: 10292
			None,
			// Token: 0x04002835 RID: 10293
			MapLight,
			// Token: 0x04002836 RID: 10294
			WindowLight
		}
	}
}

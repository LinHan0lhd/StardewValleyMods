using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x0200008A RID: 138
	public class Chunk : INetObject<NetFields>
	{
		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000581 RID: 1409 RVA: 0x0001D859 File Offset: 0x0001BA59
		// (set) Token: 0x06000582 RID: 1410 RVA: 0x0001D866 File Offset: 0x0001BA66
		public int randomOffset
		{
			get
			{
				return this.netDebrisType.Value;
			}
			set
			{
				this.netDebrisType.Value = value;
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x0001D874 File Offset: 0x0001BA74
		// (set) Token: 0x06000584 RID: 1412 RVA: 0x0001D881 File Offset: 0x0001BA81
		public float scale
		{
			get
			{
				return this.netScale.Value;
			}
			set
			{
				this.netScale.Value = value;
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000585 RID: 1413 RVA: 0x0001D88F File Offset: 0x0001BA8F
		// (set) Token: 0x06000586 RID: 1414 RVA: 0x0001D89C File Offset: 0x0001BA9C
		public float alpha
		{
			get
			{
				return this.netAlpha.Value;
			}
			set
			{
				this.netAlpha.Value = value;
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000587 RID: 1415 RVA: 0x0001D8AA File Offset: 0x0001BAAA
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("Chunk");

		// Token: 0x06000588 RID: 1416 RVA: 0x0001D8B4 File Offset: 0x0001BAB4
		public Chunk()
		{
			this.sinkTimer.Value = Game1.random.Next(1900, 2401);
			this.NetFields.SetOwner(this).AddField(this.position.NetFields, "position.NetFields").AddField(this.xVelocity, "xVelocity").AddField(this.yVelocity, "yVelocity").AddField(this.sinkTimer, "sinkTimer").AddField(this.netDebrisType, "netDebrisType").AddField(this.xSpriteSheet, "xSpriteSheet").AddField(this.ySpriteSheet, "ySpriteSheet").AddField(this.netScale, "netScale").AddField(this.netAlpha, "netAlpha").AddField(this.hasPassedRestingLineOnce, "hasPassedRestingLineOnce");
			if (LocalMultiplayer.IsLocalMultiplayer(true))
			{
				this.NetFields.DeltaAggregateTicks = 10;
				return;
			}
			this.NetFields.DeltaAggregateTicks = 30;
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x0001DA4F File Offset: 0x0001BC4F
		public Chunk(Vector2 position, float xVelocity, float yVelocity, int random_offset) : this()
		{
			this.position.Value = position;
			this.xVelocity.Value = xVelocity;
			this.yVelocity.Value = yVelocity;
			this.randomOffset = random_offset;
			this.alpha = 1f;
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x0001DA8E File Offset: 0x0001BC8E
		public float getSpeed()
		{
			return (float)Math.Sqrt((double)(this.xVelocity.Value * this.xVelocity.Value + this.yVelocity.Value * this.yVelocity.Value));
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0001DAC6 File Offset: 0x0001BCC6
		public Vector2 GetVisualPosition()
		{
			if (this.bob == 0f)
			{
				return this.position.Value;
			}
			return new Vector2(this.position.X, this.position.Y + this.bob);
		}

		// Token: 0x04000292 RID: 658
		public const int MinSinkTimer = 1900;

		// Token: 0x04000293 RID: 659
		public const int MaxSinkTimer = 2400;

		// Token: 0x04000294 RID: 660
		[XmlElement("position")]
		public NetPosition position = new NetPosition();

		// Token: 0x04000295 RID: 661
		[XmlIgnore]
		public readonly NetFloat xVelocity = new NetFloat().Interpolated(true, true);

		// Token: 0x04000296 RID: 662
		[XmlIgnore]
		public readonly NetFloat yVelocity = new NetFloat().Interpolated(true, true);

		// Token: 0x04000297 RID: 663
		[XmlIgnore]
		public readonly NetBool hasPassedRestingLineOnce = new NetBool(false);

		// Token: 0x04000298 RID: 664
		[XmlIgnore]
		public int bounces;

		// Token: 0x04000299 RID: 665
		[XmlIgnore]
		public float bob;

		// Token: 0x0400029A RID: 666
		public readonly NetInt sinkTimer = new NetInt();

		// Token: 0x0400029B RID: 667
		public readonly NetInt netDebrisType = new NetInt();

		// Token: 0x0400029C RID: 668
		[XmlIgnore]
		public bool hitWall;

		// Token: 0x0400029D RID: 669
		[XmlElement("xSpriteSheet")]
		public readonly NetInt xSpriteSheet = new NetInt();

		// Token: 0x0400029E RID: 670
		[XmlElement("ySpriteSheet")]
		public readonly NetInt ySpriteSheet = new NetInt();

		// Token: 0x0400029F RID: 671
		[XmlIgnore]
		public float rotation;

		// Token: 0x040002A0 RID: 672
		[XmlIgnore]
		public float rotationVelocity;

		// Token: 0x040002A1 RID: 673
		private readonly NetFloat netScale = new NetFloat().Interpolated(true, true);

		// Token: 0x040002A2 RID: 674
		private readonly NetFloat netAlpha = new NetFloat();
	}
}

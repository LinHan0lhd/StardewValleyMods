using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x02000094 RID: 148
	public class DebugMetricsComponent : DrawableGameComponent
	{
		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x0600067E RID: 1662 RVA: 0x00024AA4 File Offset: 0x00022CA4
		// (set) Token: 0x0600067F RID: 1663 RVA: 0x00024AAC File Offset: 0x00022CAC
		public SpriteFont Font
		{
			get
			{
				return this._font;
			}
			set
			{
				this._font = value;
			}
		}

		// Token: 0x06000680 RID: 1664 RVA: 0x00024AB5 File Offset: 0x00022CB5
		public DebugMetricsComponent(Game game) : base(game)
		{
			this._game = game;
			base.DrawOrder = int.MaxValue;
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x00024AF0 File Offset: 0x00022CF0
		protected override void LoadContent()
		{
			this._spriteBatch = new SpriteBatch(base.GraphicsDevice);
			int w = 2;
			int h = 2;
			this._opaqueWhite = new Texture2D(base.GraphicsDevice, w, h, false, SurfaceFormat.Color)
			{
				Name = "@DebugMetricsComponent._opaqueWhite"
			};
			Color[] data = new Color[w * h];
			this._opaqueWhite.GetData<Color>(data);
			for (int i = 0; i < w * h; i++)
			{
				data[i] = Color.White;
			}
			this._opaqueWhite.SetData<Color>(data);
			base.LoadContent();
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x00024B74 File Offset: 0x00022D74
		public override void Update(GameTime gameTime)
		{
			if (Game1.IsServer)
			{
				this.bandwidthMonitor = Game1.server;
			}
			else if (Game1.IsClient)
			{
				this.bandwidthMonitor = Game1.client;
			}
			else
			{
				this.bandwidthMonitor = null;
			}
			if (this.bandwidthMonitor == null || !this.bandwidthMonitor.LogBandwidth)
			{
				this.bandwidthDownGraph = null;
				this.bandwidthUpGraph = null;
			}
			if (this.bandwidthMonitor != null && this.bandwidthMonitor.LogBandwidth && (this.bandwidthDownGraph == null || this.bandwidthUpGraph == null))
			{
				int barGraphWidth = 200;
				int barGraphHeight = 150;
				int buffer = 50;
				this.bandwidthUpGraph = new BarGraph(this.bandwidthMonitor.BandwidthLogger.LoggedAvgBitsUp, Game1.uiViewport.Width - barGraphWidth - buffer, buffer, barGraphWidth, barGraphHeight, 2, BarGraph.DYNAMIC_SCALE_MAX, Color.Yellow * 0.8f, this._opaqueWhite);
				this.bandwidthDownGraph = new BarGraph(this.bandwidthMonitor.BandwidthLogger.LoggedAvgBitsDown, Game1.uiViewport.Width - barGraphWidth - buffer, buffer + barGraphHeight + buffer, barGraphWidth, barGraphHeight, 2, BarGraph.DYNAMIC_SCALE_MAX, Color.Cyan * 0.8f, this._opaqueWhite);
			}
		}

		// Token: 0x06000683 RID: 1667 RVA: 0x00024CA4 File Offset: 0x00022EA4
		public override void Draw(GameTime gameTime)
		{
			if (!Game1.displayHUD || !Game1.debugMode)
			{
				return;
			}
			if (gameTime.ElapsedGameTime.TotalSeconds > 0.0)
			{
				this._fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
				this._mspf = gameTime.ElapsedGameTime.TotalSeconds * 1000.0;
			}
			if (gameTime.IsRunningSlowly)
			{
				this._runningSlowly = true;
			}
			if (this._font != null)
			{
				this._spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
				this._drawX = this.XOffset;
				this._drawY = this.YOffset;
				StringBuilder sb = this._stringBuilder;
				Utility.makeSafe(ref this._drawX, ref this._drawY, 0, 0);
				int collection = GC.CollectionCount(0);
				float memory = (float)GC.GetTotalMemory(false) / 1048576f;
				if (this._lastCollection != collection)
				{
					this._lastCollection = collection;
					this._lastBaseMB = memory;
				}
				float diff = memory - this._lastBaseMB;
				sb.AppendFormatEx("FPS: {0,3}   GC: {1,3}   {2:0.00}MB   +{3:0.00}MB", (int)Math.Round(this._fps), this._lastCollection % 1000, this._lastBaseMB, diff);
				Color col = Color.Yellow;
				if (this._runningSlowly)
				{
					sb.Append("   [IsRunningSlowly]");
					this._runningSlowly = false;
					col = Color.Red;
				}
				this.DrawLine(col, sb, this._drawX);
				if (Game1.IsMultiplayer)
				{
					col = Color.Yellow;
					if (Game1.IsServer)
					{
						using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								KeyValuePair<long, Farmer> farmer = enumerator.Current;
								sb.AppendFormat("Ping({0}): {1:0.0}ms", farmer.Value.Name, Game1.server.getPingToClient(farmer.Key));
								this.DrawLine(col, sb, this._drawX);
							}
							goto IL_216;
						}
					}
					sb.AppendFormat("Ping: {0:0.0}ms", Game1.client.GetPingToHost());
					this.DrawLine(col, sb, this._drawX);
				}
				IL_216:
				if (this.bandwidthMonitor != null && this.bandwidthMonitor.LogBandwidth)
				{
					sb.AppendFormat("Up - b/s: {0}  Avg b/s: {1}", (int)this.bandwidthMonitor.BandwidthLogger.BitsUpPerSecond, (int)this.bandwidthMonitor.BandwidthLogger.AvgBitsUpPerSecond);
					this.DrawLine(col, sb, this._drawX);
					sb.AppendFormat("Down - b/s: {0}  Avg b/s: {1}", (int)this.bandwidthMonitor.BandwidthLogger.BitsDownPerSecond, (int)this.bandwidthMonitor.BandwidthLogger.AvgBitsDownPerSecond);
					this.DrawLine(col, sb, this._drawX);
					sb.AppendFormat("Total MB Up: {0:0.00}  Total MB Down: {1:0.00}  Total Seconds: {2:0.00}", (float)this.bandwidthMonitor.BandwidthLogger.TotalBitsUp / 8f / 1000f / 1000f, (float)this.bandwidthMonitor.BandwidthLogger.TotalBitsDown / 8f / 1000f / 1000f, (float)this.bandwidthMonitor.BandwidthLogger.TotalMs / 1000f);
					this.DrawLine(col, sb, this._drawX);
					if (this.bandwidthUpGraph != null && this.bandwidthDownGraph != null)
					{
						this.bandwidthUpGraph.Draw(this._spriteBatch);
						this.bandwidthDownGraph.Draw(this._spriteBatch);
					}
				}
				this._spriteBatch.End();
			}
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x00025044 File Offset: 0x00023244
		private void DrawLine(Color color, StringBuilder sb, int x)
		{
			if (sb == null)
			{
				return;
			}
			Vector2 size = this._font.MeasureString(sb);
			int y = this._drawY;
			int yoffset = this._font.LineSpacing;
			yoffset -= yoffset / 10;
			this._spriteBatch.Draw(this._opaqueWhite, new Rectangle(x - 1, y, (int)size.X + 2, yoffset), null, Color.Black * 0.5f);
			this._spriteBatch.DrawString(this._font, sb, new Vector2((float)x, (float)y), color);
			this._drawY += yoffset;
			sb.Clear();
		}

		// Token: 0x04000328 RID: 808
		private readonly Game _game;

		// Token: 0x04000329 RID: 809
		private SpriteFont _font;

		// Token: 0x0400032A RID: 810
		private SpriteBatch _spriteBatch;

		// Token: 0x0400032B RID: 811
		private int _drawX;

		// Token: 0x0400032C RID: 812
		private int _drawY;

		// Token: 0x0400032D RID: 813
		private double _fps;

		// Token: 0x0400032E RID: 814
		private double _mspf;

		// Token: 0x0400032F RID: 815
		private int _lastCollection;

		// Token: 0x04000330 RID: 816
		private float _lastBaseMB;

		// Token: 0x04000331 RID: 817
		private bool _runningSlowly;

		// Token: 0x04000332 RID: 818
		private StringBuilder _stringBuilder = new StringBuilder(512);

		// Token: 0x04000333 RID: 819
		private Texture2D _opaqueWhite;

		// Token: 0x04000334 RID: 820
		public int XOffset = 10;

		// Token: 0x04000335 RID: 821
		public int YOffset = 10;

		// Token: 0x04000336 RID: 822
		private IBandwidthMonitor bandwidthMonitor;

		// Token: 0x04000337 RID: 823
		private BarGraph bandwidthUpGraph;

		// Token: 0x04000338 RID: 824
		private BarGraph bandwidthDownGraph;
	}
}

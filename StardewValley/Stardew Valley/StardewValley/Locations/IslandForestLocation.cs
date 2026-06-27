using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations
{
	// Token: 0x020002D7 RID: 727
	public class IslandForestLocation : IslandLocation
	{
		// Token: 0x06002FDB RID: 12251 RVA: 0x0025CF0D File Offset: 0x0025B10D
		public IslandForestLocation()
		{
		}

		// Token: 0x06002FDC RID: 12252 RVA: 0x0025CF20 File Offset: 0x0025B120
		public IslandForestLocation(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002FDD RID: 12253 RVA: 0x0025CF35 File Offset: 0x0025B135
		public override void tryToAddCritters(bool onlyIfOnScreen = false)
		{
		}

		// Token: 0x06002FDE RID: 12254 RVA: 0x0025CF38 File Offset: 0x0025B138
		protected override void resetLocalState()
		{
			this._raySeed = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			this._rayTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LightRays");
			this._ambientLightColor = new Color(150, 120, 50);
			this.ignoreOutdoorLighting.Value = false;
			base.resetLocalState();
			this._updateWoodsLighting();
			this._wisps = new List<Wisp>();
			for (int i = 0; i < 30; i++)
			{
				Wisp wisp = new Wisp(i);
				this._wisps.Add(wisp);
			}
			this.weatherDebris = new List<WeatherDebris>();
			int spacing = 192;
			int leafType = 3;
			for (int j = 0; j < 10; j++)
			{
				this.weatherDebris.Add(new WeatherDebris(new Vector2((float)(j * spacing % Game1.graphics.GraphicsDevice.Viewport.Width + Game1.random.Next(spacing)), (float)(j * spacing / Game1.graphics.GraphicsDevice.Viewport.Width * spacing % Game1.graphics.GraphicsDevice.Viewport.Height + Game1.random.Next(spacing))), leafType, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
			}
		}

		// Token: 0x06002FDF RID: 12255 RVA: 0x0025D0B5 File Offset: 0x0025B2B5
		public override void cleanupBeforePlayerExit()
		{
			List<Wisp> wisps = this._wisps;
			if (wisps != null)
			{
				wisps.Clear();
			}
			List<WeatherDebris> list = this.weatherDebris;
			if (list != null)
			{
				list.Clear();
			}
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06002FE0 RID: 12256 RVA: 0x0025D0E0 File Offset: 0x0025B2E0
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this._updateWoodsLighting();
			if (this._wisps != null)
			{
				for (int i = 0; i < this._wisps.Count; i++)
				{
					this._wisps[i].Update(time);
				}
			}
			if (this.weatherDebris != null)
			{
				foreach (WeatherDebris weatherDebris in this.weatherDebris)
				{
					weatherDebris.update();
				}
				Game1.updateDebrisWeatherForMovement(this.weatherDebris);
			}
		}

		// Token: 0x06002FE1 RID: 12257 RVA: 0x0025D180 File Offset: 0x0025B380
		protected void _updateWoodsLighting()
		{
			if (Game1.currentLocation != this)
			{
				return;
			}
			int fade_start_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime(this)) - 60;
			int fade_end_time = Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime(this));
			int light_fade_start_time = Utility.ConvertTimeToMinutes(Game1.getStartingToGetDarkTime(this));
			int light_fade_end_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime(this));
			float num = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameMinute;
			float lerp = Utility.Clamp((num - (float)fade_start_time) / (float)(fade_end_time - fade_start_time), 0f, 1f);
			float light_lerp = Utility.Clamp((num - (float)light_fade_start_time) / (float)(light_fade_end_time - light_fade_start_time), 0f, 1f);
			Game1.ambientLight.R = (byte)Utility.Lerp((float)this._ambientLightColor.R, (float)Game1.eveningColor.R, lerp);
			Game1.ambientLight.G = (byte)Utility.Lerp((float)this._ambientLightColor.G, (float)Game1.eveningColor.G, lerp);
			Game1.ambientLight.B = (byte)Utility.Lerp((float)this._ambientLightColor.B, (float)Game1.eveningColor.B, lerp);
			Game1.ambientLight.A = (byte)Utility.Lerp((float)this._ambientLightColor.A, (float)Game1.eveningColor.A, lerp);
			Color light_color = Color.Black;
			light_color.A = (byte)Utility.Lerp(255f, 0f, light_lerp);
			foreach (LightSource light in Game1.currentLightSources.Values)
			{
				if (light.lightContext.Value == LightSource.LightContext.MapLight)
				{
					light.color.Value = light_color;
				}
			}
		}

		// Token: 0x06002FE2 RID: 12258 RVA: 0x0025D33C File Offset: 0x0025B53C
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this._wisps != null)
			{
				for (int i = 0; i < this._wisps.Count; i++)
				{
					this._wisps[i].Draw(b);
				}
			}
		}

		// Token: 0x06002FE3 RID: 12259 RVA: 0x0025D380 File Offset: 0x0025B580
		public virtual void DrawRays(SpriteBatch b)
		{
			Random random = Utility.CreateRandom((double)this._raySeed, 0.0, 0.0, 0.0, 0.0);
			float zoom = (float)Game1.graphics.GraphicsDevice.Viewport.Height * 0.6f / 128f;
			int num = -(int)(128f / zoom);
			int max = Game1.graphics.GraphicsDevice.Viewport.Width / (int)(32f * zoom);
			for (int i = num; i < max; i++)
			{
				Color color = Color.White;
				float deg = (float)Game1.viewport.X * Utility.RandomFloat(0.75f, 1f, random) + (float)Game1.viewport.Y * Utility.RandomFloat(0.2f, 0.5f, random) + (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * 20f;
				deg %= 360f;
				float rad = deg * 0.017453292f;
				color *= Utility.Clamp((float)Math.Sin((double)rad), 0f, 1f) * Utility.RandomFloat(0.15f, 0.4f, random);
				float offset = Utility.Lerp(-Utility.RandomFloat(24f, 32f, random), 0f, deg / 360f);
				b.Draw(this._rayTexture, new Vector2(((float)(i * 32) - offset) * zoom, Utility.RandomFloat(0f, -32f * zoom, random)), new Rectangle?(new Rectangle(128 * random.Next(0, 2), 0, 128, 128)), color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x06002FE4 RID: 12260 RVA: 0x0025D54E File Offset: 0x0025B74E
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			this.DrawRays(b);
		}

		// Token: 0x04002067 RID: 8295
		protected Color _ambientLightColor = Color.White;

		// Token: 0x04002068 RID: 8296
		private List<Wisp> _wisps;

		// Token: 0x04002069 RID: 8297
		private List<WeatherDebris> weatherDebris;

		// Token: 0x0400206A RID: 8298
		protected Texture2D _rayTexture;

		// Token: 0x0400206B RID: 8299
		protected int _raySeed;
	}
}

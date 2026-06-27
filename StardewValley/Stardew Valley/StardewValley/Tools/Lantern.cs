using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.Tools
{
	// Token: 0x0200012C RID: 300
	public class Lantern : Tool
	{
		// Token: 0x0600183B RID: 6203 RVA: 0x0011B774 File Offset: 0x00119974
		public Lantern() : base("Lantern", 0, 74, 74, false, 0)
		{
			base.InstantUse = true;
		}

		// Token: 0x0600183C RID: 6204 RVA: 0x0011B78F File Offset: 0x0011998F
		protected override Item GetOneNew()
		{
			return new Lantern();
		}

		// Token: 0x0600183D RID: 6205 RVA: 0x0011B798 File Offset: 0x00119998
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			this.on = !this.on;
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
			Utility.removeLightSource(this.lightSourceId);
			if (this.on)
			{
				this.lightSourceId = this.GenerateLightSourceId(who);
				Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 2.5f + (float)this.fuelLeft / 100f * 10f * 0.75f, new Color(0, 131, 255), LightSource.LightContext.None, 0L, null));
			}
		}

		// Token: 0x0600183E RID: 6206 RVA: 0x0011B864 File Offset: 0x00119A64
		public override void tickUpdate(GameTime time, Farmer who)
		{
			if (this.on && this.fuelLeft > 0 && Game1.drawLighting)
			{
				this.fuelTimer += time.ElapsedGameTime.Milliseconds;
				if (this.fuelTimer > 6000)
				{
					this.fuelLeft--;
					this.fuelTimer = 0;
				}
				Vector2 lightPosition = new Vector2(who.Position.X + 21f, who.Position.Y + 64f);
				LightSource light;
				if (Game1.currentLightSources.TryGetValue(this.lightSourceId, out light))
				{
					light.position.Value = lightPosition;
				}
				else
				{
					this.lightSourceId = this.GenerateLightSourceId(who);
					Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 1, lightPosition, 2.5f + (float)this.fuelLeft / 100f * 10f * 0.75f, new Color(0, 131, 255), LightSource.LightContext.None, 0L, null));
				}
			}
			if (this.on && this.fuelLeft <= 0)
			{
				Utility.removeLightSource(this.GenerateLightSourceId(who));
			}
		}

		// Token: 0x04000EAF RID: 3759
		public const float baseRadius = 10f;

		// Token: 0x04000EB0 RID: 3760
		public const int millisecondsPerFuelUnit = 6000;

		// Token: 0x04000EB1 RID: 3761
		public const int maxFuel = 100;

		// Token: 0x04000EB2 RID: 3762
		public int fuelLeft;

		// Token: 0x04000EB3 RID: 3763
		private int fuelTimer;

		// Token: 0x04000EB4 RID: 3764
		public bool on;

		// Token: 0x04000EB5 RID: 3765
		[XmlIgnore]
		public string lightSourceId;
	}
}

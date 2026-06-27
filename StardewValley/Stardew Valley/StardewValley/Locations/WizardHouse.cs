using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Locations
{
	// Token: 0x020002F5 RID: 757
	public class WizardHouse : GameLocation
	{
		// Token: 0x060032DF RID: 13023 RVA: 0x00295800 File Offset: 0x00293A00
		public WizardHouse()
		{
		}

		// Token: 0x060032E0 RID: 13024 RVA: 0x00295813 File Offset: 0x00293A13
		public WizardHouse(string m, string name) : base(m, name)
		{
		}

		// Token: 0x060032E1 RID: 13025 RVA: 0x00295828 File Offset: 0x00293A28
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this.wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			this.cauldronTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.cauldronTimer <= 0)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(3f, 20f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), (float)Game1.random.Next(16)), false, 0.002f, Color.Lime)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2(-0.002f, 0f),
					interval = 99999f,
					layerDepth = 0.144f - (float)Game1.random.Next(100) / 10000f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
				});
				this.cauldronTimer = 100;
			}
		}

		// Token: 0x060032E2 RID: 13026 RVA: 0x0029597B File Offset: 0x00293B7B
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (Game1.player.eventsSeen.Contains("418172"))
			{
				base.setMapTile(2, 12, 2143, "Front", "untitled tile sheet", null, true);
			}
		}

		// Token: 0x060032E3 RID: 13027 RVA: 0x002959B8 File Offset: 0x00293BB8
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(10f, 12f) * 64f + new Vector2(32f, -32f), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "WizardHouse_1",
				lightRadius = 2f,
				scale = 4f
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(2f, 21f) * 64f + new Vector2(51f, 32f), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "WizardHouse_2",
				lightRadius = 1f,
				scale = 2f
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(3f, 21f) * 64f + new Vector2(16f, 32f), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "WizardHouse_3",
				lightRadius = 1f,
				scale = 3f
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(4f, 21f) * 64f + new Vector2(-16f, 32f), false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "WizardHouse_4",
				lightRadius = 1f,
				scale = 2f
			});
		}

		// Token: 0x040021F0 RID: 8688
		private int cauldronTimer = 250;
	}
}

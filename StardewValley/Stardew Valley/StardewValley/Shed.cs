using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace StardewValley
{
	// Token: 0x020000D8 RID: 216
	public class Shed : DecoratableLocation
	{
		// Token: 0x06001085 RID: 4229 RVA: 0x000C6C56 File Offset: 0x000C4E56
		public Shed()
		{
		}

		// Token: 0x06001086 RID: 4230 RVA: 0x000C6C5E File Offset: 0x000C4E5E
		public Shed(string m, string name) : base(m, name)
		{
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x000C6C68 File Offset: 0x000C4E68
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.isDarkOut(this))
			{
				Game1.ambientLight = new Color(180, 180, 0);
			}
			Building buildingUnderConstruction = Game1.GetBuildingUnderConstruction("Robin");
			this.isRobinUpgrading = (buildingUnderConstruction != null && buildingUnderConstruction.HasIndoorsName(base.NameOrUniqueName));
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x000C6CBC File Offset: 0x000C4EBC
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.isRobinUpgrading)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 64f)), new Rectangle?(new Rectangle(90, 0, 33, 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01546f);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 84f)), new Rectangle?(new Rectangle(90, 0, 33, 31)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.015360001f);
			}
		}

		// Token: 0x04000A09 RID: 2569
		private bool isRobinUpgrading;
	}
}

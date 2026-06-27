using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.Locations
{
	// Token: 0x020002C7 RID: 711
	public class Cellar : GameLocation
	{
		// Token: 0x06002E22 RID: 11810 RVA: 0x00240EAE File Offset: 0x0023F0AE
		public Cellar()
		{
		}

		// Token: 0x06002E23 RID: 11811 RVA: 0x00240EB6 File Offset: 0x0023F0B6
		public Cellar(string mapPath, string name) : base(mapPath, name)
		{
			this.setUpAgingBoards();
		}

		// Token: 0x06002E24 RID: 11812 RVA: 0x00240EC8 File Offset: 0x0023F0C8
		public void setUpAgingBoards()
		{
			for (int i = 6; i < 17; i++)
			{
				Vector2 v = new Vector2((float)i, 8f);
				if (!this.objects.ContainsKey(v))
				{
					this.objects.Add(v, new Cask(v));
				}
				v = new Vector2((float)i, 10f);
				if (!this.objects.ContainsKey(v))
				{
					this.objects.Add(v, new Cask(v));
				}
				v = new Vector2((float)i, 12f);
				if (!this.objects.ContainsKey(v))
				{
					this.objects.Add(v, new Cask(v));
				}
			}
		}

		// Token: 0x06002E25 RID: 11813 RVA: 0x00240F74 File Offset: 0x0023F174
		protected override void resetLocalState()
		{
			base.resetLocalState();
			string target = "Farmhouse";
			bool targetFound = false;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				Cabin cabin = location as Cabin;
				if (cabin != null && cabin.GetCellarName() == this.Name)
				{
					target = cabin.NameOrUniqueName;
					targetFound = true;
					return false;
				}
				return true;
			}, true, false);
			foreach (Warp warp in this.warps)
			{
				warp.TargetName = target;
			}
		}

		// Token: 0x06002E26 RID: 11814 RVA: 0x00241004 File Offset: 0x0023F204
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(-Game1.viewport.X, -Game1.viewport.Y - 256, 512, 256), Color.Black);
			base.drawAboveAlwaysFrontLayer(b);
		}
	}
}

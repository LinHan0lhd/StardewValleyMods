using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Locations
{
	// Token: 0x020002E7 RID: 743
	public class Mine : GameLocation
	{
		// Token: 0x0600312E RID: 12590 RVA: 0x0026F250 File Offset: 0x0026D450
		public Mine()
		{
		}

		// Token: 0x0600312F RID: 12591 RVA: 0x0026F258 File Offset: 0x0026D458
		public Mine(string map, string name) : base(map, name)
		{
			Vector2 tile = this.GetBoulderPosition();
			this.objects.Add(tile, new Object(tile, "78", false));
		}

		// Token: 0x06003130 RID: 12592 RVA: 0x0026F28C File Offset: 0x0026D48C
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			MineShaft.mushroomLevelsGeneratedToday.Clear();
		}

		// Token: 0x06003131 RID: 12593 RVA: 0x0026F29F File Offset: 0x0026D49F
		public Vector2 GetBoulderPosition()
		{
			return new Vector2(27f, 8f);
		}
	}
}

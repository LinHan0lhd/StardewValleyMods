using System;

namespace StardewValley.Pathfinding
{
	// Token: 0x0200019B RID: 411
	public class LocationWarpRoute
	{
		// Token: 0x06001D4A RID: 7498 RVA: 0x0014FA4A File Offset: 0x0014DC4A
		public LocationWarpRoute(string[] locationNames, Gender? onlyGender)
		{
			this.LocationNames = locationNames;
			this.OnlyGender = onlyGender;
		}

		// Token: 0x04001216 RID: 4630
		public readonly string[] LocationNames;

		// Token: 0x04001217 RID: 4631
		public readonly Gender? OnlyGender;
	}
}

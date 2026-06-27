using System;
using System.Collections.Generic;

namespace StardewValley.Delegates
{
	// Token: 0x02000361 RID: 865
	public readonly struct GameStateQueryContext
	{
		// Token: 0x060035B6 RID: 13750 RVA: 0x002A781C File Offset: 0x002A5A1C
		public GameStateQueryContext(GameLocation location, Farmer player, Item targetItem, Item inputItem, Random random, HashSet<string> ignoreQueryKeys = null, Dictionary<string, object> customFields = null)
		{
			this.ExplicitTargetLocation = location;
			this.Location = (location ?? (((player != null) ? player.currentLocation : null) ?? Game1.currentLocation));
			this.Player = (player ?? Game1.player);
			this.TargetItem = targetItem;
			this.InputItem = inputItem;
			this.Random = (random ?? Game1.random);
			this.IgnoreQueryKeys = ignoreQueryKeys;
			this.CustomFields = customFields;
		}

		// Token: 0x040022AF RID: 8879
		public readonly GameLocation Location;

		// Token: 0x040022B0 RID: 8880
		public readonly GameLocation ExplicitTargetLocation;

		// Token: 0x040022B1 RID: 8881
		public readonly Farmer Player;

		// Token: 0x040022B2 RID: 8882
		public readonly Item TargetItem;

		// Token: 0x040022B3 RID: 8883
		public readonly Item InputItem;

		// Token: 0x040022B4 RID: 8884
		public readonly Random Random;

		// Token: 0x040022B5 RID: 8885
		public readonly HashSet<string> IgnoreQueryKeys;

		// Token: 0x040022B6 RID: 8886
		public readonly Dictionary<string, object> CustomFields;
	}
}

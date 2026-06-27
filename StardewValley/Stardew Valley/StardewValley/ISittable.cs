using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	// Token: 0x02000100 RID: 256
	public interface ISittable
	{
		// Token: 0x06001467 RID: 5223
		bool IsSittingHere(Farmer who);

		// Token: 0x06001468 RID: 5224
		bool HasSittingFarmers();

		// Token: 0x06001469 RID: 5225
		void RemoveSittingFarmer(Farmer farmer);

		// Token: 0x0600146A RID: 5226
		int GetSittingFarmerCount();

		// Token: 0x0600146B RID: 5227
		List<Vector2> GetSeatPositions(bool ignore_offsets = false);

		// Token: 0x0600146C RID: 5228
		Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false);

		// Token: 0x0600146D RID: 5229
		Vector2? AddSittingFarmer(Farmer who);

		// Token: 0x0600146E RID: 5230
		int GetSittingDirection();

		// Token: 0x0600146F RID: 5231
		Rectangle GetSeatBounds();

		// Token: 0x06001470 RID: 5232
		bool IsSeatHere(GameLocation location);
	}
}

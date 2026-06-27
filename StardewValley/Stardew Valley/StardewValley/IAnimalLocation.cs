using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000CD RID: 205
	[Obsolete("All locations allow animals now, so there's no need to check for this interface anymore.")]
	public interface IAnimalLocation
	{
		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000DFD RID: 3581
		NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals { get; }

		// Token: 0x06000DFE RID: 3582
		bool CheckPetAnimal(Vector2 position, Farmer who);

		// Token: 0x06000DFF RID: 3583
		bool CheckPetAnimal(Rectangle rect, Farmer who);

		// Token: 0x06000E00 RID: 3584
		bool CheckInspectAnimal(Vector2 position, Farmer who);

		// Token: 0x06000E01 RID: 3585
		bool CheckInspectAnimal(Rectangle rect, Farmer who);
	}
}

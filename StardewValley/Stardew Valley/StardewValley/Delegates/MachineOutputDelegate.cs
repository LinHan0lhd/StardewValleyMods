using System;
using StardewValley.GameData.Machines;

namespace StardewValley.Delegates
{
	// Token: 0x02000365 RID: 869
	// (Invoke) Token: 0x060035C4 RID: 13764
	public delegate Item MachineOutputDelegate(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady);
}

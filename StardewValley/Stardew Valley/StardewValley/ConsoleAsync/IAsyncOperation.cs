using System;

namespace StardewValley.ConsoleAsync
{
	// Token: 0x02000370 RID: 880
	public interface IAsyncOperation
	{
		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x060035E4 RID: 13796
		bool Started { get; }

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x060035E5 RID: 13797
		bool Done { get; }

		// Token: 0x060035E6 RID: 13798
		void Begin();

		// Token: 0x060035E7 RID: 13799
		void Conclude();
	}
}

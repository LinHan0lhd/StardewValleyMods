using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200002B RID: 43
	public interface INetSerializable
	{
		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000146 RID: 326
		// (set) Token: 0x06000147 RID: 327
		uint DirtyTick { get; set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000148 RID: 328
		bool Dirty { get; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000149 RID: 329
		// (set) Token: 0x0600014A RID: 330
		bool NeedsTick { get; set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600014B RID: 331
		// (set) Token: 0x0600014C RID: 332
		bool ChildNeedsTick { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600014D RID: 333
		// (set) Token: 0x0600014E RID: 334
		string Name { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600014F RID: 335
		// (set) Token: 0x06000150 RID: 336
		INetSerializable Parent { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000151 RID: 337
		INetRoot Root { get; }

		// Token: 0x06000152 RID: 338
		void MarkDirty();

		// Token: 0x06000153 RID: 339
		void MarkClean();

		// Token: 0x06000154 RID: 340
		bool Tick();

		// Token: 0x06000155 RID: 341
		void Read(BinaryReader reader, NetVersion version);

		// Token: 0x06000156 RID: 342
		void Write(BinaryWriter writer);

		// Token: 0x06000157 RID: 343
		void ReadFull(BinaryReader reader, NetVersion version);

		// Token: 0x06000158 RID: 344
		void WriteFull(BinaryWriter writer);
	}
}

using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000037 RID: 55
	public class NetEvent0 : AbstractNetSerializable
	{
		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06000235 RID: 565 RVA: 0x0000DAE0 File Offset: 0x0000BCE0
		// (remove) Token: 0x06000236 RID: 566 RVA: 0x0000DB18 File Offset: 0x0000BD18
		public event NetEvent0.Event onEvent;

		// Token: 0x06000237 RID: 567 RVA: 0x0000DB4D File Offset: 0x0000BD4D
		public NetEvent0(bool interpolate = false)
		{
			this.Counter.InterpolationEnabled = interpolate;
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0000DB6C File Offset: 0x0000BD6C
		public void Fire()
		{
			NetInt counter = this.Counter;
			int value = counter.Value + 1;
			counter.Value = value;
			this.Poll();
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0000DB94 File Offset: 0x0000BD94
		public void Poll()
		{
			if (this.Counter.Value != this.currentCount)
			{
				this.currentCount = this.Counter.Value;
				if (this.onEvent != null)
				{
					this.onEvent();
				}
			}
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0000DBCD File Offset: 0x0000BDCD
		public void Clear()
		{
			this.Counter.Set(0);
			this.currentCount = 0;
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0000DBE2 File Offset: 0x0000BDE2
		public override void Read(BinaryReader reader, NetVersion version)
		{
			this.Counter.Read(reader, version);
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0000DBF1 File Offset: 0x0000BDF1
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.Counter.ReadFull(reader, version);
			this.currentCount = this.Counter.Value;
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0000DC11 File Offset: 0x0000BE11
		public override void Write(BinaryWriter writer)
		{
			this.Counter.Write(writer);
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000DC1F File Offset: 0x0000BE1F
		public override void WriteFull(BinaryWriter writer)
		{
			this.Counter.WriteFull(writer);
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000DC2D File Offset: 0x0000BE2D
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(this.Counter);
		}

		// Token: 0x04000160 RID: 352
		public readonly NetInt Counter = new NetInt();

		// Token: 0x04000161 RID: 353
		private int currentCount;

		// Token: 0x020003DA RID: 986
		// (Invoke) Token: 0x060039CA RID: 14794
		public delegate void Event();
	}
}

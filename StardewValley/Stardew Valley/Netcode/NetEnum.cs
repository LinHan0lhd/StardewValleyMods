using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	// Token: 0x02000035 RID: 53
	public class NetEnum<T> : NetFieldBase<T, NetEnum<T>>, IEnumerable<string>, IEnumerable where T : struct, IConvertible
	{
		// Token: 0x06000225 RID: 549 RVA: 0x0000D833 File Offset: 0x0000BA33
		public NetEnum()
		{
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000D83B File Offset: 0x0000BA3B
		public NetEnum(T value) : base(value)
		{
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0000D844 File Offset: 0x0000BA44
		public override void Set(T newValue)
		{
			if (!EqualityComparer<T>.Default.Equals(newValue, this.value))
			{
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000D868 File Offset: 0x0000BA68
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			T newValue = (T)((object)Enum.ToObject(typeof(T), reader.ReadInt16()));
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0000D8A6 File Offset: 0x0000BAA6
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(Convert.ToInt16(this.value));
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000D8BE File Offset: 0x0000BABE
		public IEnumerator<string> GetEnumerator()
		{
			return Enumerable.Repeat<string>(Convert.ToString(base.Get()), 1).GetEnumerator();
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000D8DB File Offset: 0x0000BADB
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000D8E4 File Offset: 0x0000BAE4
		public void Add(string value)
		{
			if (this.xmlInitialized || base.Parent != null)
			{
				throw new InvalidOperationException(base.GetType().Name + " already has value " + this.ToString());
			}
			base.cleanSet((T)((object)Enum.Parse(typeof(T), value)));
			this.xmlInitialized = true;
		}

		// Token: 0x0400015D RID: 349
		private bool xmlInitialized;
	}
}

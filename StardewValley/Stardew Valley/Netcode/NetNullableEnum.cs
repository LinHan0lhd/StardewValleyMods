using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	// Token: 0x02000036 RID: 54
	public class NetNullableEnum<T> : NetField<T?, NetNullableEnum<T>>, IEnumerable<string>, IEnumerable where T : struct, IConvertible
	{
		// Token: 0x0600022D RID: 557 RVA: 0x0000D944 File Offset: 0x0000BB44
		public NetNullableEnum() : base(null)
		{
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0000D960 File Offset: 0x0000BB60
		public NetNullableEnum(T value) : base(new T?(value))
		{
		}

		// Token: 0x0600022F RID: 559 RVA: 0x0000D96E File Offset: 0x0000BB6E
		public override void Set(T? newValue)
		{
			if (!EqualityComparer<T?>.Default.Equals(newValue, this.value))
			{
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x06000230 RID: 560 RVA: 0x0000D990 File Offset: 0x0000BB90
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			T? newValue = null;
			if (reader.ReadBoolean())
			{
				newValue = new T?((T)((object)Enum.ToObject(typeof(T), reader.ReadInt16())));
			}
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x06000231 RID: 561 RVA: 0x0000D9E4 File Offset: 0x0000BBE4
		protected override void WriteDelta(BinaryWriter writer)
		{
			if (this.value == null)
			{
				writer.Write(false);
				return;
			}
			writer.Write(true);
			writer.Write(Convert.ToInt16(this.value));
		}

		// Token: 0x06000232 RID: 562 RVA: 0x0000DA18 File Offset: 0x0000BC18
		public new IEnumerator<string> GetEnumerator()
		{
			T? value = base.Get();
			if (value == null)
			{
				return Enumerable.Repeat<string>(null, 1).GetEnumerator();
			}
			return Enumerable.Repeat<string>(Convert.ToString(value), 1).GetEnumerator();
		}

		// Token: 0x06000233 RID: 563 RVA: 0x0000DA58 File Offset: 0x0000BC58
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000234 RID: 564 RVA: 0x0000DA60 File Offset: 0x0000BC60
		public void Add(string value)
		{
			if (this.xmlInitialized || base.Parent != null)
			{
				throw new InvalidOperationException(base.GetType().Name + " already has value " + this.ToString());
			}
			if (!string.IsNullOrEmpty(value))
			{
				base.cleanSet(new T?((T)((object)Enum.Parse(typeof(T), value))));
			}
			else
			{
				base.cleanSet(null);
			}
			this.xmlInitialized = true;
		}

		// Token: 0x0400015E RID: 350
		private bool xmlInitialized;
	}
}

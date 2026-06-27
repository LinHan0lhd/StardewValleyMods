using System;
using System.IO;
using System.Runtime.CompilerServices;
using StardewValley.Util;

namespace Netcode
{
	// Token: 0x0200005A RID: 90
	public class NetExtendableRef<T, TSelf> : NetRefBase<T, TSelf> where T : class, INetObject<INetSerializable> where TSelf : NetExtendableRef<!0, !1>
	{
		// Token: 0x060003BF RID: 959 RVA: 0x00011FFD File Offset: 0x000101FD
		public NetExtendableRef()
		{
			base.notifyOnTargetValueChange = true;
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0001200C File Offset: 0x0001020C
		public NetExtendableRef(T value) : this()
		{
			base.cleanSet(value);
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x0001201B File Offset: 0x0001021B
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			if (this.targetValue != null)
			{
				childAction(this.targetValue.NetFields);
			}
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x00012040 File Offset: 0x00010240
		protected override void ReadValueFull(T value, BinaryReader reader, NetVersion version)
		{
			value.NetFields.ReadFull(reader, version);
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00012054 File Offset: 0x00010254
		protected override void ReadValueDelta(BinaryReader reader, NetVersion version)
		{
			this.targetValue.NetFields.Read(reader, version);
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0001206D File Offset: 0x0001026D
		private void clearValueParent(T targetValue)
		{
			if (targetValue.NetFields.Parent == this)
			{
				targetValue.NetFields.Parent = null;
			}
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x00012094 File Offset: 0x00010294
		private void setValueParent(T targetValue)
		{
			T t = targetValue;
			if (((t != null) ? t.NetFields : null) == null)
			{
				string message;
				if (targetValue != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Can't change net field parent for ");
					defaultInterpolatedStringHandler.AppendFormatted(targetValue.GetType().FullName);
					defaultInterpolatedStringHandler.AppendLiteral(" type's null ");
					defaultInterpolatedStringHandler.AppendFormatted("NetFields");
					defaultInterpolatedStringHandler.AppendLiteral(" to '");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					message = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					message = "Can't change net field parent for null target to '" + base.Name + ".";
				}
				NetHelper.LogWarning(message);
				NetHelper.LogVerbose(new StackTraceHelper().ToString());
				return;
			}
			if (base.Parent != null || base.Root == this)
			{
				if (targetValue.NetFields.Parent != null && targetValue.NetFields.Parent != this)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Changing net field parent for '");
					defaultInterpolatedStringHandler.AppendFormatted(targetValue.NetFields.Name);
					defaultInterpolatedStringHandler.AppendLiteral("' collection from '");
					defaultInterpolatedStringHandler.AppendFormatted(targetValue.NetFields.Parent.Name);
					defaultInterpolatedStringHandler.AppendLiteral("' to '");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					NetHelper.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
					NetHelper.LogVerbose(new StackTraceHelper().ToString());
				}
				targetValue.NetFields.Parent = this;
			}
			targetValue.NetFields.MarkClean();
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x00012252 File Offset: 0x00010452
		protected override void targetValueChanged(T oldValue, T newValue)
		{
			base.targetValueChanged(oldValue, newValue);
			if (oldValue != null)
			{
				this.clearValueParent(oldValue);
			}
			if (newValue != null)
			{
				this.setValueParent(newValue);
			}
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0001227A File Offset: 0x0001047A
		protected override void WriteValueFull(BinaryWriter writer)
		{
			this.targetValue.NetFields.WriteFull(writer);
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x00012292 File Offset: 0x00010492
		protected override void WriteValueDelta(BinaryWriter writer)
		{
			this.targetValue.NetFields.Write(writer);
		}
	}
}

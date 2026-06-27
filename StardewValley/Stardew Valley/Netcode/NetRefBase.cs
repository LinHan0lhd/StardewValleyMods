using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using StardewValley.SaveSerialization;

namespace Netcode
{
	// Token: 0x02000059 RID: 89
	public abstract class NetRefBase<T, TSelf> : NetField<T, TSelf> where T : class where TSelf : NetRefBase<!0, !1>
	{
		// Token: 0x14000011 RID: 17
		// (add) Token: 0x060003A8 RID: 936 RVA: 0x00011B24 File Offset: 0x0000FD24
		// (remove) Token: 0x060003A9 RID: 937 RVA: 0x00011B5C File Offset: 0x0000FD5C
		public event NetRefBase<T, TSelf>.ConflictResolveEvent OnConflictResolve;

		// Token: 0x060003AA RID: 938 RVA: 0x00011B91 File Offset: 0x0000FD91
		public NetRefBase()
		{
		}

		// Token: 0x060003AB RID: 939 RVA: 0x00011B99 File Offset: 0x0000FD99
		public NetRefBase(T value) : this()
		{
			base.cleanSet(value);
		}

		// Token: 0x060003AC RID: 940 RVA: 0x00011BA8 File Offset: 0x0000FDA8
		protected override void SetParent(INetSerializable parent)
		{
			if (parent == null || parent.Root != base.Root)
			{
				this.reassigned.Clear();
			}
			base.SetParent(parent);
		}

		// Token: 0x060003AD RID: 941 RVA: 0x00011BCD File Offset: 0x0000FDCD
		protected override void CleanImpl()
		{
			base.CleanImpl();
			this.deltaType = NetRefBase<T, TSelf>.RefDeltaType.ChildDelta;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00011BDC File Offset: 0x0000FDDC
		public void MarkReassigned()
		{
			this.deltaType = NetRefBase<T, TSelf>.RefDeltaType.Reassigned;
			if (base.Root != null)
			{
				this.reassigned.Set(base.Root.Clock.netVersion);
			}
			base.MarkDirty();
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00011C10 File Offset: 0x0000FE10
		public override void Set(T newValue)
		{
			if (newValue != base.Value)
			{
				this.deltaType = NetRefBase<T, TSelf>.RefDeltaType.Reassigned;
				if (base.Root != null)
				{
					this.reassigned.Set(base.Root.Clock.netVersion);
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x00011C68 File Offset: 0x0000FE68
		private T createType(Type type)
		{
			if (type == null)
			{
				return default(T);
			}
			if (!typeof(T).IsAssignableFrom(type))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(88, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Net ref field '");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' received invalid type '");
				defaultInterpolatedStringHandler.AppendFormatted(type.FullName);
				defaultInterpolatedStringHandler.AppendLiteral("', which can't be converted to expected type '");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(T).FullName);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidCastException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return (T)((object)Activator.CreateInstance(type));
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x00011D16 File Offset: 0x0000FF16
		protected T ReadType(BinaryReader reader)
		{
			return this.createType(reader.ReadType());
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x00011D24 File Offset: 0x0000FF24
		protected void WriteType(BinaryWriter writer)
		{
			writer.WriteTypeOf(this.targetValue);
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00011D34 File Offset: 0x0000FF34
		private void serialize(BinaryWriter writer, XmlSerializer serializer = null)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				(serializer ?? this.Serializer).SerializeFast(stream, this.targetValue);
				stream.Seek(0L, SeekOrigin.Begin);
				writer.Write((int)stream.Length);
				writer.Write(stream.ToArray());
			}
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00011DA4 File Offset: 0x0000FFA4
		private T deserialize(BinaryReader reader, XmlSerializer serializer = null)
		{
			int length = reader.ReadInt32();
			T result;
			using (MemoryStream stream = new MemoryStream(reader.ReadBytes(length)))
			{
				result = (T)((object)(serializer ?? this.Serializer).DeserializeFast(stream));
			}
			return result;
		}

		// Token: 0x060003B5 RID: 949
		protected abstract void ReadValueFull(T value, BinaryReader reader, NetVersion version);

		// Token: 0x060003B6 RID: 950
		protected abstract void ReadValueDelta(BinaryReader reader, NetVersion version);

		// Token: 0x060003B7 RID: 951
		protected abstract void WriteValueFull(BinaryWriter writer);

		// Token: 0x060003B8 RID: 952
		protected abstract void WriteValueDelta(BinaryWriter writer);

		// Token: 0x060003B9 RID: 953 RVA: 0x00011DFC File Offset: 0x0000FFFC
		private void writeBaseValue(BinaryWriter writer)
		{
			if (this.Serializer != null)
			{
				this.serialize(writer, null);
				return;
			}
			this.WriteType(writer);
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00011E16 File Offset: 0x00010016
		private T readBaseValue(BinaryReader reader, NetVersion version)
		{
			if (this.Serializer != null)
			{
				return this.deserialize(reader, null);
			}
			return this.ReadType(reader);
		}

		// Token: 0x060003BB RID: 955 RVA: 0x00011E30 File Offset: 0x00010030
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			if (reader.ReadByte() == 1)
			{
				reader.ReadSkippable(delegate
				{
					NetVersion remoteReassign = default(NetVersion);
					remoteReassign.Read(reader);
					T remoteValue = this.readBaseValue(reader, version);
					if (remoteValue != null)
					{
						this.ReadValueFull(remoteValue, reader, version);
					}
					if (remoteReassign.IsIndependent(this.reassigned))
					{
						if (!remoteReassign.IsPriorityOver(this.reassigned))
						{
							if (this.OnConflictResolve != null)
							{
								this.OnConflictResolve(remoteValue, this.targetValue);
							}
							return;
						}
						if (this.OnConflictResolve != null)
						{
							this.OnConflictResolve(this.targetValue, remoteValue);
						}
					}
					else if (!remoteReassign.IsPriorityOver(this.reassigned))
					{
						return;
					}
					this.reassigned.Set(remoteReassign);
					this.setInterpolationTarget(remoteValue);
				});
				return;
			}
			reader.ReadSkippable(delegate
			{
				if (!version.IsPrecededBy(this.reassigned))
				{
					return;
				}
				if (this.targetValue != null)
				{
					this.ReadValueDelta(reader, version);
				}
			});
		}

		// Token: 0x060003BC RID: 956 RVA: 0x00011E98 File Offset: 0x00010098
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Push((this.targetValue != null) ? this.targetValue.GetType().Name : "null");
			writer.Write((byte)this.deltaType);
			if (this.deltaType == NetRefBase<T, TSelf>.RefDeltaType.Reassigned)
			{
				writer.WriteSkippable(delegate
				{
					this.reassigned.Write(writer);
					this.writeBaseValue(writer);
					if (this.targetValue != null)
					{
						this.WriteValueFull(writer);
					}
				});
			}
			else
			{
				writer.WriteSkippable(delegate
				{
					if (this.targetValue != null)
					{
						this.WriteValueDelta(writer);
					}
				});
			}
			this.deltaType = NetRefBase<T, TSelf>.RefDeltaType.ChildDelta;
			writer.Pop();
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00011F4C File Offset: 0x0001014C
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.reassigned.Read(reader);
			T remoteValue = this.readBaseValue(reader, version);
			if (remoteValue != null)
			{
				this.ReadValueFull(remoteValue, reader, version);
			}
			base.cleanSet(remoteValue);
			this.ChangeVersion.Merge(version);
		}

		// Token: 0x060003BE RID: 958 RVA: 0x00011F94 File Offset: 0x00010194
		public override void WriteFull(BinaryWriter writer)
		{
			writer.Push((this.targetValue != null) ? this.targetValue.GetType().Name : "null");
			this.reassigned.Write(writer);
			this.writeBaseValue(writer);
			if (this.targetValue != null)
			{
				this.WriteValueFull(writer);
			}
			writer.Pop();
		}

		// Token: 0x0400018B RID: 395
		public XmlSerializer Serializer;

		// Token: 0x0400018C RID: 396
		private NetRefBase<T, TSelf>.RefDeltaType deltaType;

		// Token: 0x0400018D RID: 397
		protected NetVersion reassigned;

		// Token: 0x020003ED RID: 1005
		private enum RefDeltaType : byte
		{
			// Token: 0x040026C9 RID: 9929
			ChildDelta,
			// Token: 0x040026CA RID: 9930
			Reassigned
		}

		// Token: 0x020003EE RID: 1006
		// (Invoke) Token: 0x06003A02 RID: 14850
		public delegate void ConflictResolveEvent(T rejected, T accepted);
	}
}

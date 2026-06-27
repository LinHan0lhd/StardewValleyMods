using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Netcode.Validation;

namespace Netcode
{
	// Token: 0x0200004D RID: 77
	public class NetFields : AbstractNetSerializable
	{
		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000309 RID: 777 RVA: 0x0000FA0B File Offset: 0x0000DC0B
		public new string Name { get; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600030A RID: 778 RVA: 0x0000FA13 File Offset: 0x0000DC13
		// (set) Token: 0x0600030B RID: 779 RVA: 0x0000FA1B File Offset: 0x0000DC1B
		public INetObject<NetFields> Owner { get; private set; }

		// Token: 0x0600030C RID: 780 RVA: 0x0000FA24 File Offset: 0x0000DC24
		public NetFields(string name)
		{
			this.Name = name;
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0000FA3E File Offset: 0x0000DC3E
		public NetFields SetOwner(INetObject<NetFields> owner)
		{
			this.Owner = owner;
			return this;
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0000FA48 File Offset: 0x0000DC48
		public static string GetNameForInstance<TBaseType>(TBaseType instance)
		{
			Type baseType = typeof(TBaseType);
			Type instanceType = instance.GetType();
			if (!(baseType == instanceType))
			{
				return baseType.Name + " (" + instanceType.Name + ")";
			}
			return baseType.Name;
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0000FA99 File Offset: 0x0000DC99
		public IEnumerable<INetSerializable> GetFields()
		{
			return this.fields;
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000FAA4 File Offset: 0x0000DCA4
		public void CancelInterpolation()
		{
			foreach (INetSerializable netSerializable in this.fields)
			{
				InterpolationCancellable cancellable = netSerializable as InterpolationCancellable;
				if (cancellable != null)
				{
					cancellable.CancelInterpolation();
				}
			}
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0000FB00 File Offset: 0x0000DD00
		public NetFields AddField(INetSerializable field, [CallerArgumentExpression("field")] string name = null)
		{
			name = (name ?? field.GetType().FullName);
			if (this.Owner == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Field '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' was added to the '");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' net fields before ");
				defaultInterpolatedStringHandler.AppendFormatted("SetOwner");
				defaultInterpolatedStringHandler.AppendLiteral(" was called.");
				NetHelper.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (field.Parent != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't add field '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' to the '");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' net fields because it's already part of the ");
				defaultInterpolatedStringHandler.AppendFormatted(field.Parent.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" tree.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (base.Parent != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't add field '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' to the '");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' net fields, because they've already been added to a tree.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (Netcode.NetFields.ShouldValidateNetFields)
			{
				foreach (INetSerializable otherField in this.fields)
				{
					if (field == otherField)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Field '");
						defaultInterpolatedStringHandler.AppendFormatted(name);
						defaultInterpolatedStringHandler.AppendLiteral("' was added to the '");
						defaultInterpolatedStringHandler.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler.AppendLiteral("' net fields multiple times.");
						NetHelper.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
						break;
					}
				}
			}
			field.Name = this.Name + ": " + name;
			this.fields.Add(field);
			return this;
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000FD20 File Offset: 0x0000DF20
		protected override void SetParent(INetSerializable parent)
		{
			base.SetParent(parent);
			this.ValidateNetFields();
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0000FD30 File Offset: 0x0000DF30
		protected void ValidateNetFields()
		{
			if (this.Owner == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 3);
				defaultInterpolatedStringHandler.AppendFormatted("NetFields");
				defaultInterpolatedStringHandler.AppendLiteral(" collection '");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' was initialized without calling ");
				defaultInterpolatedStringHandler.AppendFormatted("SetOwner");
				defaultInterpolatedStringHandler.AppendLiteral(", so it can't be validated.");
				NetHelper.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			if (this != this.Owner.NetFields)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(100, 4);
				defaultInterpolatedStringHandler.AppendFormatted("NetFields");
				defaultInterpolatedStringHandler.AppendLiteral(" collection '");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' has its own owner set to an ");
				INetObject<NetFields> owner = this.Owner;
				defaultInterpolatedStringHandler.AppendFormatted((owner != null) ? owner.GetType().FullName : null);
				defaultInterpolatedStringHandler.AppendLiteral(" instance whose ");
				defaultInterpolatedStringHandler.AppendFormatted("NetFields");
				defaultInterpolatedStringHandler.AppendLiteral(" field doesn't reference this collection.");
				NetHelper.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			if (Netcode.NetFields.ShouldValidateNetFields)
			{
				NetFieldValidator.ValidateNetFields(this.Owner, new Action<string>(NetHelper.LogWarning));
			}
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0000FE60 File Offset: 0x0000E060
		public override void Read(BinaryReader reader, NetVersion version)
		{
			BitArray dirtyBits = reader.ReadBitArray();
			if (this.fields.Count != dirtyBits.Length)
			{
				throw new InvalidOperationException();
			}
			for (int i = 0; i < this.fields.Count; i++)
			{
				if (dirtyBits[i])
				{
					INetSerializable field = this.fields[i];
					try
					{
						field.Read(reader, version);
					}
					catch (Exception ex)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Failed reading ");
						defaultInterpolatedStringHandler.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" field '");
						defaultInterpolatedStringHandler.AppendFormatted(field.Name);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
					}
				}
			}
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0000FF30 File Offset: 0x0000E130
		public override void Write(BinaryWriter writer)
		{
			BitArray dirtyBits = new BitArray(this.fields.Count);
			for (int i = 0; i < this.fields.Count; i++)
			{
				dirtyBits[i] = this.fields[i].Dirty;
			}
			writer.WriteBitArray(dirtyBits);
			for (int j = 0; j < this.fields.Count; j++)
			{
				if (dirtyBits[j])
				{
					INetSerializable field = this.fields[j];
					writer.Push(Convert.ToString(j));
					try
					{
						field.Write(writer);
					}
					catch (Exception ex)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Failed writing ");
						defaultInterpolatedStringHandler.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" field '");
						defaultInterpolatedStringHandler.AppendFormatted(field.Name);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
					}
					writer.Pop();
				}
			}
		}

		// Token: 0x06000316 RID: 790 RVA: 0x0001003C File Offset: 0x0000E23C
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			foreach (INetSerializable field in this.fields)
			{
				try
				{
					field.ReadFull(reader, version);
				}
				catch (Exception ex)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed reading ");
					defaultInterpolatedStringHandler.AppendFormatted(this.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" field '");
					defaultInterpolatedStringHandler.AppendFormatted(field.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				}
			}
		}

		// Token: 0x06000317 RID: 791 RVA: 0x000100F4 File Offset: 0x0000E2F4
		public override void WriteFull(BinaryWriter writer)
		{
			for (int i = 0; i < this.fields.Count; i++)
			{
				INetSerializable field = this.fields[i];
				writer.Push(Convert.ToString(i));
				try
				{
					field.WriteFull(writer);
				}
				catch (Exception ex)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed writing ");
					defaultInterpolatedStringHandler.AppendFormatted(this.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" field '");
					defaultInterpolatedStringHandler.AppendFormatted(field.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				}
				writer.Pop();
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x000101AC File Offset: 0x0000E3AC
		public virtual void CopyFrom(NetFields source)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						using (BinaryReader reader = new BinaryReader(stream))
						{
							source.WriteFull(writer);
							stream.Seek(0L, SeekOrigin.Begin);
							if (base.Root == null)
							{
								this.ReadFull(reader, new NetClock().netVersion);
							}
							else
							{
								this.ReadFull(reader, base.Root.Clock.netVersion);
							}
							base.MarkClean();
						}
					}
				}
			}
			catch (Exception ex)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed copying ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" fields from '");
				defaultInterpolatedStringHandler.AppendFormatted(source.Name);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
			}
		}

		// Token: 0x06000319 RID: 793 RVA: 0x000102C8 File Offset: 0x0000E4C8
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			foreach (INetSerializable field in this.fields)
			{
				childAction(field);
			}
		}

		// Token: 0x0400017A RID: 378
		public static bool ShouldValidateNetFields;

		// Token: 0x0400017D RID: 381
		private readonly List<INetSerializable> fields = new List<INetSerializable>();
	}
}

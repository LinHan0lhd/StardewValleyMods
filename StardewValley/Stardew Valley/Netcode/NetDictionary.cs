using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	// Token: 0x02000032 RID: 50
	public abstract class NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> : AbstractNetSerializable, IEquatable<!4>, IEnumerable<!3>, IEnumerable where TField : class, INetObject<INetSerializable>, new() where TSerialDict : IDictionary<!0, !1>, new() where TSelf : NetDictionary<!0, !1, !2, !3, !4>
	{
		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060001CF RID: 463 RVA: 0x0000C683 File Offset: 0x0000A883
		public int Length
		{
			get
			{
				return this.dict.Count;
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000C690 File Offset: 0x0000A890
		public bool Any()
		{
			return this.dict.Count > 0;
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x0000C6A0 File Offset: 0x0000A8A0
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060001D2 RID: 466 RVA: 0x0000C6A4 File Offset: 0x0000A8A4
		// (remove) Token: 0x060001D3 RID: 467 RVA: 0x0000C6DC File Offset: 0x0000A8DC
		public event NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ContentsChangeEvent OnValueAdded;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x060001D4 RID: 468 RVA: 0x0000C714 File Offset: 0x0000A914
		// (remove) Token: 0x060001D5 RID: 469 RVA: 0x0000C74C File Offset: 0x0000A94C
		public event NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ContentsChangeEvent OnValueRemoved;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060001D6 RID: 470 RVA: 0x0000C784 File Offset: 0x0000A984
		// (remove) Token: 0x060001D7 RID: 471 RVA: 0x0000C7BC File Offset: 0x0000A9BC
		public event NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ContentsUpdateEvent OnValueTargetUpdated;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060001D8 RID: 472 RVA: 0x0000C7F4 File Offset: 0x0000A9F4
		// (remove) Token: 0x060001D9 RID: 473 RVA: 0x0000C82C File Offset: 0x0000AA2C
		public event NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ConflictResolveEvent OnConflictResolve;

		// Token: 0x17000053 RID: 83
		public TValue this[TKey key]
		{
			get
			{
				return this.getFieldValue(this.dict[key]);
			}
			set
			{
				TField field;
				if (!this.dict.TryGetValue(key, out field))
				{
					field = (this.dict[key] = Activator.CreateInstance<TField>());
					this.dictReassigns[key] = base.GetLocalVersion();
					this.setFieldValue(field, key, value);
					this.added(key, field, this.dictReassigns[key]);
					return;
				}
				this.setFieldValue(field, key, value);
				this.addedEvent(key, field);
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060001DC RID: 476 RVA: 0x0000C8E9 File Offset: 0x0000AAE9
		public NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection Keys
		{
			get
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection(this.dict);
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060001DD RID: 477 RVA: 0x0000C8F6 File Offset: 0x0000AAF6
		public NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ValuesCollection Values
		{
			get
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ValuesCollection(this);
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060001DE RID: 478 RVA: 0x0000C8FE File Offset: 0x0000AAFE
		public NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.PairsCollection Pairs
		{
			get
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.PairsCollection(this);
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060001DF RID: 479 RVA: 0x0000C906 File Offset: 0x0000AB06
		public Dictionary<TKey, TField> FieldDict
		{
			get
			{
				return this.dict;
			}
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000C90E File Offset: 0x0000AB0E
		public NetDictionary()
		{
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000C949 File Offset: 0x0000AB49
		public NetDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dict) : this()
		{
			this.CopyFrom(dict);
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000C958 File Offset: 0x0000AB58
		protected override bool tickImpl()
		{
			List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange> triggeredChanges = null;
			foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange ch in this.incomingChanges)
			{
				if (base.Root != null && base.GetLocalTick() < ch.Tick)
				{
					break;
				}
				if (triggeredChanges == null)
				{
					triggeredChanges = new List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange>();
				}
				triggeredChanges.Add(ch);
			}
			if (triggeredChanges != null && triggeredChanges.Count > 0)
			{
				foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange c in triggeredChanges)
				{
					this.incomingChanges.Remove(c);
				}
				foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange ch2 in triggeredChanges)
				{
					if (ch2.Removal)
					{
						this.performIncomingRemove(ch2);
					}
					else
					{
						this.performIncomingAdd(ch2);
					}
				}
			}
			return this.incomingChanges.Count > 0;
		}

		// Token: 0x060001E3 RID: 483
		protected abstract void setFieldValue(TField field, TKey key, TValue value);

		// Token: 0x060001E4 RID: 484
		protected abstract TValue getFieldValue(TField field);

		// Token: 0x060001E5 RID: 485
		protected abstract TValue getFieldTargetValue(TField field);

		// Token: 0x060001E6 RID: 486 RVA: 0x0000CA80 File Offset: 0x0000AC80
		protected TField createField(TKey key, TValue value)
		{
			TField field = Activator.CreateInstance<TField>();
			this.setFieldValue(field, key, value);
			return field;
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000CAA0 File Offset: 0x0000ACA0
		public void CopyFrom(IEnumerable<KeyValuePair<TKey, TValue>> dict)
		{
			foreach (KeyValuePair<TKey, TValue> pair in dict)
			{
				this[pair.Key] = pair.Value;
			}
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000CAF8 File Offset: 0x0000ACF8
		public void Set(IEnumerable<KeyValuePair<TKey, TValue>> dict)
		{
			this.Clear();
			this.CopyFrom(dict);
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000CB08 File Offset: 0x0000AD08
		public void MoveFrom(TSelf dict)
		{
			List<KeyValuePair<TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>(dict.Pairs);
			dict.Clear();
			this.Set(pairs);
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000CB3D File Offset: 0x0000AD3D
		public void SetEqualityComparer(IEqualityComparer<TKey> comparer)
		{
			this.dict = new Dictionary<TKey, TField>(this.dict, comparer);
			this.dictReassigns = new Dictionary<TKey, NetVersion>(this.dictReassigns, comparer);
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000CB64 File Offset: 0x0000AD64
		private void setFieldParent(TField arg)
		{
			INetObject<INetSerializable> value = arg;
			if (base.Parent != null)
			{
				value.NetFields.Parent = this;
			}
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000CB8C File Offset: 0x0000AD8C
		private void added(TKey key, TField field, NetVersion reassign)
		{
			this.outgoingChanges.Add(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange(false, key, field, reassign));
			this.setFieldParent(field);
			base.MarkDirty();
			this.addedEvent(key, field);
			foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange change2 in this.incomingChanges)
			{
				if (!change2.Removal && object.Equals(change2.Key, key))
				{
					this.clearFieldParent(change2.Field);
					if (this.OnConflictResolve != null)
					{
						this.OnConflictResolve(key, change2.Field, field);
					}
				}
			}
			this.incomingChanges.RemoveAll((NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange change) => object.Equals(key, change.Key));
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000CC84 File Offset: 0x0000AE84
		private void addedEvent(TKey key, TField field)
		{
			if (this.OnValueAdded != null)
			{
				this.OnValueAdded(key, this.getFieldValue(field));
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000CCA1 File Offset: 0x0000AEA1
		private void updatedEvent(TKey key, TValue old_target_value, TValue new_target_value)
		{
			if (this.OnValueTargetUpdated != null)
			{
				this.OnValueTargetUpdated(key, old_target_value, new_target_value);
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000CCBC File Offset: 0x0000AEBC
		private void clearFieldParent(TField arg)
		{
			INetObject<INetSerializable> field = arg;
			if (field.NetFields.Parent == this)
			{
				field.NetFields.Parent = null;
			}
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000CCEA File Offset: 0x0000AEEA
		private void removed(TKey key, TField field, NetVersion reassign)
		{
			this.outgoingChanges.Add(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange(true, key, field, reassign));
			this.clearFieldParent(field);
			base.MarkDirty();
			this.removedEvent(key, field);
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x0000CD15 File Offset: 0x0000AF15
		private void removedEvent(TKey key, TField field)
		{
			if (this.OnValueRemoved != null)
			{
				this.OnValueRemoved(key, this.getFieldValue(field));
			}
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000CD34 File Offset: 0x0000AF34
		public void Add(TKey key, TValue value)
		{
			TField field = this.createField(key, value);
			this.Add(key, field);
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000CD52 File Offset: 0x0000AF52
		public void Add(TKey key, TField field)
		{
			this.dict.Add(key, field);
			this.dictReassigns.Add(key, base.GetLocalVersion());
			this.added(key, field, this.dictReassigns[key]);
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000CD88 File Offset: 0x0000AF88
		public bool TryAdd(TKey key, TValue value)
		{
			if (this.dict.ContainsKey(key))
			{
				return false;
			}
			TField field = this.createField(key, value);
			this.Add(key, field);
			return true;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000CDB8 File Offset: 0x0000AFB8
		public void Clear()
		{
			NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection keys = this.Keys;
			while (keys.Any())
			{
				this.Remove(keys.First());
			}
			this.outgoingChanges.RemoveAll((NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange ch) => !ch.Removal);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000CE10 File Offset: 0x0000B010
		public bool ContainsKey(TKey key)
		{
			return this.dict.ContainsKey(key);
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000CE1E File Offset: 0x0000B01E
		public int Count()
		{
			return this.dict.Count;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000CE2C File Offset: 0x0000B02C
		public bool Remove(TKey key)
		{
			TField field;
			if (this.dict.TryGetValue(key, out field))
			{
				NetVersion reassign = this.dictReassigns[key];
				this.dict.Remove(key);
				this.dictReassigns.Remove(key);
				this.removed(key, field, reassign);
				return true;
			}
			return false;
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x0000CE7C File Offset: 0x0000B07C
		public int RemoveWhere(Func<KeyValuePair<TKey, TValue>, bool> match)
		{
			if (this.dict.Count == 0)
			{
				return 0;
			}
			int removed = 0;
			foreach (KeyValuePair<TKey, TValue> pair in this.Pairs)
			{
				if (match(pair))
				{
					this.Remove(pair.Key);
					removed++;
				}
			}
			return removed;
		}

		// Token: 0x060001FA RID: 506 RVA: 0x0000CEF8 File Offset: 0x0000B0F8
		[Obsolete("Use RemoveWhere instead.")]
		public void Filter(Func<KeyValuePair<TKey, TValue>, bool> f)
		{
			this.RemoveWhere((KeyValuePair<TKey, TValue> pair) => !f(pair));
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000CF28 File Offset: 0x0000B128
		public bool TryGetValue(TKey key, out TValue value)
		{
			TField field;
			if (this.dict.TryGetValue(key, out field))
			{
				value = this.getFieldValue(field);
				return true;
			}
			value = default(TValue);
			return false;
		}

		// Token: 0x060001FC RID: 508 RVA: 0x0000CF5C File Offset: 0x0000B15C
		public TValue GetValueOrDefault(TKey key, TValue defaultValue = default(TValue))
		{
			TField field;
			if (!this.dict.TryGetValue(key, out field))
			{
				return defaultValue;
			}
			return this.getFieldValue(field);
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0000CF82 File Offset: 0x0000B182
		public bool Equals(TSelf other)
		{
			return object.Equals(this.dict, other.dict);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x0000CF9A File Offset: 0x0000B19A
		protected override void CleanImpl()
		{
			base.CleanImpl();
			this.outgoingChanges.Clear();
		}

		// Token: 0x060001FF RID: 511
		protected abstract TKey ReadKey(BinaryReader reader);

		// Token: 0x06000200 RID: 512
		protected abstract void WriteKey(BinaryWriter writer, TKey key);

		// Token: 0x06000201 RID: 513 RVA: 0x0000CFB0 File Offset: 0x0000B1B0
		private void readMultiple(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ReadFunc readFunc, BinaryReader reader, NetVersion version)
		{
			uint count = reader.Read7BitEncoded();
			for (uint i = 0U; i < count; i += 1U)
			{
				readFunc(reader, version);
			}
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0000CFD8 File Offset: 0x0000B1D8
		private void writeMultiple<T>(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.WriteFunc<T> writeFunc, BinaryWriter writer, IEnumerable<T> values)
		{
			writer.Write7BitEncoded((uint)values.Count<T>());
			foreach (T value in values)
			{
				writeFunc(writer, value);
			}
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000D030 File Offset: 0x0000B230
		protected virtual TField ReadFieldFull(BinaryReader reader, NetVersion version)
		{
			TField tfield = Activator.CreateInstance<TField>();
			tfield.NetFields.ReadFull(reader, version);
			return tfield;
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000D049 File Offset: 0x0000B249
		protected virtual void WriteFieldFull(BinaryWriter writer, TField field)
		{
			field.NetFields.WriteFull(writer);
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000D05C File Offset: 0x0000B25C
		private void readAddition(BinaryReader reader, NetVersion version)
		{
			TKey key = this.ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			TField field = this.ReadFieldFull(reader, version);
			this.setFieldParent(field);
			this.queueIncomingChange(false, key, field, reassign);
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000D09C File Offset: 0x0000B29C
		protected virtual bool resolveConflict(TKey key, TField currentField, NetVersion currentReassign, TField incomingField, NetVersion incomingReassign)
		{
			if (incomingReassign.IsPriorityOver(currentReassign))
			{
				this.clearFieldParent(currentField);
				if (this.OnConflictResolve != null)
				{
					this.OnConflictResolve(key, currentField, incomingField);
				}
				return true;
			}
			this.clearFieldParent(incomingField);
			if (this.OnConflictResolve != null)
			{
				this.OnConflictResolve(key, incomingField, currentField);
			}
			return false;
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0000D0F4 File Offset: 0x0000B2F4
		private KeyValuePair<NetVersion, TField>? findConflict(TKey key)
		{
			foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange change in this.incomingChanges.AsEnumerable<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange>().Reverse<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange>())
			{
				if (object.Equals(change.Key, key))
				{
					if (change.Removal)
					{
						KeyValuePair<NetVersion, TField>? result = null;
						return result;
					}
					return new KeyValuePair<NetVersion, TField>?(new KeyValuePair<NetVersion, TField>(change.Reassigned, change.Field));
				}
			}
			TField value;
			if (this.dict.TryGetValue(key, out value))
			{
				return new KeyValuePair<NetVersion, TField>?(new KeyValuePair<NetVersion, TField>(this.dictReassigns[key], value));
			}
			return null;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000D1C0 File Offset: 0x0000B3C0
		private void queueIncomingChange(bool removal, TKey key, TField field, NetVersion fieldReassign)
		{
			if (!removal)
			{
				KeyValuePair<NetVersion, TField>? conflict = this.findConflict(key);
				if (conflict != null && !this.resolveConflict(key, conflict.Value.Value, conflict.Value.Key, field, fieldReassign))
				{
					return;
				}
			}
			uint timestamp = base.GetLocalTick() + (uint)((this.InterpolationWait && base.Root != null) ? base.Root.Clock.InterpolationTicks : 0);
			this.incomingChanges.Add(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange(timestamp, removal, key, field, fieldReassign));
			base.NeedsTick = true;
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000D254 File Offset: 0x0000B454
		private void performIncomingAdd(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange add)
		{
			this.dict[add.Key] = add.Field;
			this.dictReassigns[add.Key] = add.Reassigned;
			this.addedEvent(add.Key, add.Field);
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000D2A4 File Offset: 0x0000B4A4
		private void readRemoval(BinaryReader reader, NetVersion version)
		{
			TKey key = this.ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			this.queueIncomingChange(true, key, default(TField), reassign);
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000D2DB File Offset: 0x0000B4DB
		private void readDictChange(BinaryReader reader, NetVersion version)
		{
			if (reader.ReadByte() != 0)
			{
				this.readRemoval(reader, version);
				return;
			}
			this.readAddition(reader, version);
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000D2F8 File Offset: 0x0000B4F8
		private void performIncomingRemove(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange remove)
		{
			TField field;
			if (this.dict.TryGetValue(remove.Key, out field))
			{
				this.clearFieldParent(field);
				this.dict.Remove(remove.Key);
				this.dictReassigns.Remove(remove.Key);
				this.removedEvent(remove.Key, field);
			}
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000D354 File Offset: 0x0000B554
		private void readUpdate(BinaryReader reader, NetVersion version)
		{
			TKey key = this.ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			Predicate<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange> <>9__1;
			reader.ReadSkippable(delegate
			{
				List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange> list = this.incomingChanges;
				Predicate<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange> match;
				if ((match = <>9__1) == null)
				{
					match = (<>9__1 = ((NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange ch) => !ch.Removal && object.Equals(ch.Key, key) && reassign.Equals(ch.Reassigned)));
				}
				int changeIndex = list.FindLastIndex(match);
				if (changeIndex == -1)
				{
					TField field;
					if (this.dict.TryGetValue(key, out field) && this.dictReassigns[key].Equals(reassign))
					{
						if (this.OnValueTargetUpdated != null)
						{
							TValue old_value = this.getFieldTargetValue(field);
							field.NetFields.Read(reader, version);
							this.updatedEvent(key, old_value, this.getFieldTargetValue(field));
							return;
						}
						field.NetFields.Read(reader, version);
					}
					return;
				}
				TField field2 = this.incomingChanges[changeIndex].Field;
				if (this.OnValueTargetUpdated != null)
				{
					TValue old_value2 = this.getFieldTargetValue(field2);
					field2.NetFields.Read(reader, version);
					this.updatedEvent(key, old_value2, this.getFieldTargetValue(field2));
					return;
				}
				field2.NetFields.Read(reader, version);
			});
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000D3C2 File Offset: 0x0000B5C2
		public override void Read(BinaryReader reader, NetVersion version)
		{
			this.readMultiple(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ReadFunc(this.readDictChange), reader, version);
			this.readMultiple(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ReadFunc(this.readUpdate), reader, version);
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000D3EC File Offset: 0x0000B5EC
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.dict.Clear();
			this.dictReassigns.Clear();
			this.outgoingChanges.Clear();
			this.incomingChanges.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				TKey key = this.ReadKey(reader);
				NetVersion reassign = default(NetVersion);
				reassign.Read(reader);
				TField field = this.ReadFieldFull(reader, version);
				this.dict.Add(key, field);
				this.dictReassigns.Add(key, reassign);
				this.setFieldParent(field);
				this.addedEvent(key, field);
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0000D486 File Offset: 0x0000B686
		private void writeAddition(BinaryWriter writer, NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange update)
		{
			this.WriteKey(writer, update.Key);
			update.Reassigned.Write(writer);
			this.WriteFieldFull(writer, update.Field);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000D4AE File Offset: 0x0000B6AE
		private void writeRemoval(BinaryWriter writer, NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange update)
		{
			this.WriteKey(writer, update.Key);
			update.Reassigned.Write(writer);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000D4C9 File Offset: 0x0000B6C9
		private void writeDictChange(BinaryWriter writer, NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange ch)
		{
			if (ch.Removal)
			{
				writer.Write(1);
				this.writeRemoval(writer, ch);
				return;
			}
			writer.Write(0);
			this.writeAddition(writer, ch);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000D4F4 File Offset: 0x0000B6F4
		private void writeUpdate(BinaryWriter writer, NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange update)
		{
			this.WriteKey(writer, update.Key);
			update.Reassigned.Write(writer);
			writer.WriteSkippable(delegate
			{
				update.Field.NetFields.Write(writer);
			});
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0000D559 File Offset: 0x0000B759
		private IEnumerable<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange> updates()
		{
			foreach (KeyValuePair<TKey, TField> pair in this.dict)
			{
				if (pair.Value.NetFields.Dirty)
				{
					yield return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange(false, pair.Key, pair.Value, this.dictReassigns[pair.Key]);
				}
			}
			Dictionary<TKey, TField>.Enumerator enumerator = default(Dictionary<TKey, TField>.Enumerator);
			foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange removal in from ch in this.outgoingChanges
			where ch.Removal
			select ch)
			{
				if (removal.Field.NetFields.Dirty)
				{
					yield return removal;
				}
			}
			IEnumerator<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange> enumerator2 = null;
			yield break;
			yield break;
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000D569 File Offset: 0x0000B769
		public override void Write(BinaryWriter writer)
		{
			this.writeMultiple<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange>(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.WriteFunc<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange>(this.writeDictChange), writer, this.outgoingChanges);
			this.writeMultiple<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange>(new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.WriteFunc<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange>(this.writeUpdate), writer, this.updates());
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000D5A0 File Offset: 0x0000B7A0
		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(this.Length);
			foreach (TKey key in this.dict.Keys)
			{
				this.WriteKey(writer, key);
				this.dictReassigns[key].Write(writer);
				this.WriteFieldFull(writer, this.dict[key]);
			}
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000D630 File Offset: 0x0000B830
		public IEnumerator<TSerialDict> GetEnumerator()
		{
			TSerialDict serial = Activator.CreateInstance<TSerialDict>();
			foreach (KeyValuePair<TKey, TField> kvp in this.dict)
			{
				ref TSerialDict ptr = ref serial;
				if (default(TSerialDict) == null)
				{
					TSerialDict tserialDict = serial;
					ptr = ref tserialDict;
				}
				ptr.Add(kvp.Key, this.getFieldValue(kvp.Value));
			}
			return new List<TSerialDict>
			{
				serial
			}.GetEnumerator();
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000D6D4 File Offset: 0x0000B8D4
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000D6DC File Offset: 0x0000B8DC
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			foreach (NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange ch in this.incomingChanges)
			{
				if (ch.Field != null)
				{
					childAction(ch.Field.NetFields);
				}
			}
			foreach (TField field in this.dict.Values)
			{
				childAction(field.NetFields);
			}
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000D79C File Offset: 0x0000B99C
		public void Add(TSerialDict dict)
		{
			this.Set(dict);
		}

		// Token: 0x0600021B RID: 539 RVA: 0x0000D7AA File Offset: 0x0000B9AA
		protected override void ValidateChildren()
		{
			if ((base.Parent != null || base.Root == this) && !base.NeedsTick)
			{
				this.ForEachChild(new Action<INetSerializable>(this.ValidateChild));
			}
		}

		// Token: 0x04000154 RID: 340
		public bool InterpolationWait = true;

		// Token: 0x04000155 RID: 341
		private Dictionary<TKey, TField> dict = new Dictionary<TKey, TField>();

		// Token: 0x04000156 RID: 342
		private Dictionary<TKey, NetVersion> dictReassigns = new Dictionary<TKey, NetVersion>();

		// Token: 0x04000157 RID: 343
		private List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange> outgoingChanges = new List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.OutgoingChange>();

		// Token: 0x04000158 RID: 344
		private List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange> incomingChanges = new List<NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.IncomingChange>();

		// Token: 0x020003CA RID: 970
		public class IncomingChange
		{
			// Token: 0x0600398B RID: 14731 RVA: 0x002D74E7 File Offset: 0x002D56E7
			public IncomingChange(uint tick, bool removal, TKey key, TField field, NetVersion reassigned)
			{
				this.Tick = tick;
				this.Removal = removal;
				this.Key = key;
				this.Field = field;
				this.Reassigned = reassigned;
			}

			// Token: 0x0400268B RID: 9867
			public uint Tick;

			// Token: 0x0400268C RID: 9868
			public bool Removal;

			// Token: 0x0400268D RID: 9869
			public TKey Key;

			// Token: 0x0400268E RID: 9870
			public TField Field;

			// Token: 0x0400268F RID: 9871
			public NetVersion Reassigned;
		}

		// Token: 0x020003CB RID: 971
		public class OutgoingChange
		{
			// Token: 0x0600398C RID: 14732 RVA: 0x002D7514 File Offset: 0x002D5714
			public OutgoingChange(bool removal, TKey key, TField field, NetVersion reassigned)
			{
				this.Removal = removal;
				this.Key = key;
				this.Field = field;
				this.Reassigned = reassigned;
			}

			// Token: 0x04002690 RID: 9872
			public bool Removal;

			// Token: 0x04002691 RID: 9873
			public TKey Key;

			// Token: 0x04002692 RID: 9874
			public TField Field;

			// Token: 0x04002693 RID: 9875
			public NetVersion Reassigned;
		}

		// Token: 0x020003CC RID: 972
		// (Invoke) Token: 0x0600398E RID: 14734
		public delegate void ContentsChangeEvent(TKey key, TValue value);

		// Token: 0x020003CD RID: 973
		// (Invoke) Token: 0x06003992 RID: 14738
		public delegate void ConflictResolveEvent(TKey key, TField rejected, TField accepted);

		// Token: 0x020003CE RID: 974
		// (Invoke) Token: 0x06003996 RID: 14742
		public delegate void ContentsUpdateEvent(TKey key, TValue old_target_value, TValue new_target_value);

		// Token: 0x020003CF RID: 975
		// (Invoke) Token: 0x0600399A RID: 14746
		private delegate void ReadFunc(BinaryReader reader, NetVersion version);

		// Token: 0x020003D0 RID: 976
		// (Invoke) Token: 0x0600399E RID: 14750
		private delegate void WriteFunc<T>(BinaryWriter writer, T value);

		// Token: 0x020003D1 RID: 977
		public struct PairsCollection : IEnumerable<KeyValuePair<!0, !1>>, IEnumerable
		{
			// Token: 0x060039A1 RID: 14753 RVA: 0x002D7539 File Offset: 0x002D5739
			public PairsCollection(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
			{
				this._net = net;
			}

			// Token: 0x060039A2 RID: 14754 RVA: 0x002D7542 File Offset: 0x002D5742
			public int Count()
			{
				return this._net.dict.Count;
			}

			// Token: 0x060039A3 RID: 14755 RVA: 0x002D7554 File Offset: 0x002D5754
			public KeyValuePair<TKey, TValue> ElementAt(int index)
			{
				int count = 0;
				foreach (KeyValuePair<TKey, TValue> pair in this)
				{
					if (count == index)
					{
						return pair;
					}
					count++;
				}
				throw new ArgumentOutOfRangeException();
			}

			// Token: 0x060039A4 RID: 14756 RVA: 0x002D75B0 File Offset: 0x002D57B0
			public NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.PairsCollection.Enumerator GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.PairsCollection.Enumerator(this._net);
			}

			// Token: 0x060039A5 RID: 14757 RVA: 0x002D75BD File Offset: 0x002D57BD
			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<!0, !1>>.GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.PairsCollection.Enumerator(this._net);
			}

			// Token: 0x060039A6 RID: 14758 RVA: 0x002D75CF File Offset: 0x002D57CF
			IEnumerator IEnumerable.GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.PairsCollection.Enumerator(this._net);
			}

			// Token: 0x04002694 RID: 9876
			private NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

			// Token: 0x020006DC RID: 1756
			public struct Enumerator : IEnumerator<KeyValuePair<!0, !1>>, IEnumerator, IDisposable
			{
				// Token: 0x0600465F RID: 18015 RVA: 0x0032352A File Offset: 0x0032172A
				public Enumerator(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
				{
					this._net = net;
					this._enumerator = this._net.dict.GetEnumerator();
					this._current = default(KeyValuePair<TKey, TValue>);
					this._done = false;
				}

				// Token: 0x06004660 RID: 18016 RVA: 0x0032355C File Offset: 0x0032175C
				public bool MoveNext()
				{
					if (!this._enumerator.MoveNext())
					{
						this._done = true;
						this._current = default(KeyValuePair<TKey, TValue>);
						return false;
					}
					KeyValuePair<TKey, TField> pair = this._enumerator.Current;
					this._current = new KeyValuePair<TKey, TValue>(pair.Key, this._net.getFieldValue(pair.Value));
					return true;
				}

				// Token: 0x1700052C RID: 1324
				// (get) Token: 0x06004661 RID: 18017 RVA: 0x003235BE File Offset: 0x003217BE
				public KeyValuePair<TKey, TValue> Current
				{
					get
					{
						return this._current;
					}
				}

				// Token: 0x06004662 RID: 18018 RVA: 0x003235C6 File Offset: 0x003217C6
				public void Dispose()
				{
				}

				// Token: 0x1700052D RID: 1325
				// (get) Token: 0x06004663 RID: 18019 RVA: 0x003235C8 File Offset: 0x003217C8
				object IEnumerator.Current
				{
					get
					{
						if (this._done)
						{
							throw new InvalidOperationException();
						}
						return this._current;
					}
				}

				// Token: 0x06004664 RID: 18020 RVA: 0x003235E3 File Offset: 0x003217E3
				void IEnumerator.Reset()
				{
					this._enumerator = this._net.dict.GetEnumerator();
					this._current = default(KeyValuePair<TKey, TValue>);
					this._done = false;
				}

				// Token: 0x040030B8 RID: 12472
				private readonly NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

				// Token: 0x040030B9 RID: 12473
				private Dictionary<TKey, TField>.Enumerator _enumerator;

				// Token: 0x040030BA RID: 12474
				private KeyValuePair<TKey, TValue> _current;

				// Token: 0x040030BB RID: 12475
				private bool _done;
			}
		}

		// Token: 0x020003D2 RID: 978
		public struct ValuesCollection : IEnumerable<!1>, IEnumerable
		{
			// Token: 0x060039A7 RID: 14759 RVA: 0x002D75E1 File Offset: 0x002D57E1
			public ValuesCollection(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
			{
				this._net = net;
			}

			// Token: 0x060039A8 RID: 14760 RVA: 0x002D75EA File Offset: 0x002D57EA
			public NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ValuesCollection.Enumerator GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ValuesCollection.Enumerator(this._net);
			}

			// Token: 0x060039A9 RID: 14761 RVA: 0x002D75F7 File Offset: 0x002D57F7
			IEnumerator<TValue> IEnumerable<!1>.GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ValuesCollection.Enumerator(this._net);
			}

			// Token: 0x060039AA RID: 14762 RVA: 0x002D7609 File Offset: 0x002D5809
			IEnumerator IEnumerable.GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.ValuesCollection.Enumerator(this._net);
			}

			// Token: 0x04002695 RID: 9877
			private NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

			// Token: 0x020006DD RID: 1757
			public struct Enumerator : IEnumerator<!1>, IEnumerator, IDisposable
			{
				// Token: 0x06004665 RID: 18021 RVA: 0x0032360E File Offset: 0x0032180E
				public Enumerator(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
				{
					this._net = net;
					this._enumerator = this._net.dict.GetEnumerator();
					this._current = default(TValue);
					this._done = false;
				}

				// Token: 0x06004666 RID: 18022 RVA: 0x00323640 File Offset: 0x00321840
				public bool MoveNext()
				{
					if (!this._enumerator.MoveNext())
					{
						this._done = true;
						this._current = default(TValue);
						return false;
					}
					KeyValuePair<TKey, TField> pair = this._enumerator.Current;
					this._current = this._net.getFieldValue(pair.Value);
					return true;
				}

				// Token: 0x1700052E RID: 1326
				// (get) Token: 0x06004667 RID: 18023 RVA: 0x00323696 File Offset: 0x00321896
				public TValue Current
				{
					get
					{
						return this._current;
					}
				}

				// Token: 0x06004668 RID: 18024 RVA: 0x0032369E File Offset: 0x0032189E
				public void Dispose()
				{
				}

				// Token: 0x1700052F RID: 1327
				// (get) Token: 0x06004669 RID: 18025 RVA: 0x003236A0 File Offset: 0x003218A0
				object IEnumerator.Current
				{
					get
					{
						if (this._done)
						{
							throw new InvalidOperationException();
						}
						return this._current;
					}
				}

				// Token: 0x0600466A RID: 18026 RVA: 0x003236BB File Offset: 0x003218BB
				void IEnumerator.Reset()
				{
					this._enumerator = this._net.dict.GetEnumerator();
					this._current = default(TValue);
					this._done = false;
				}

				// Token: 0x040030BC RID: 12476
				private readonly NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

				// Token: 0x040030BD RID: 12477
				private Dictionary<TKey, TField>.Enumerator _enumerator;

				// Token: 0x040030BE RID: 12478
				private TValue _current;

				// Token: 0x040030BF RID: 12479
				private bool _done;
			}
		}

		// Token: 0x020003D3 RID: 979
		public struct KeysCollection : IEnumerable<!0>, IEnumerable
		{
			// Token: 0x060039AB RID: 14763 RVA: 0x002D761B File Offset: 0x002D581B
			public KeysCollection(Dictionary<TKey, TField> dict)
			{
				this._dict = dict;
			}

			// Token: 0x060039AC RID: 14764 RVA: 0x002D7624 File Offset: 0x002D5824
			public bool Any()
			{
				return this._dict.Count > 0;
			}

			// Token: 0x060039AD RID: 14765 RVA: 0x002D7634 File Offset: 0x002D5834
			public TKey First()
			{
				using (Dictionary<TKey, TField>.Enumerator enumerator = this._dict.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						KeyValuePair<TKey, TField> pair = enumerator.Current;
						return pair.Key;
					}
				}
				return default(TKey);
			}

			// Token: 0x060039AE RID: 14766 RVA: 0x002D7698 File Offset: 0x002D5898
			public bool Contains(TKey key)
			{
				return this._dict.ContainsKey(key);
			}

			// Token: 0x060039AF RID: 14767 RVA: 0x002D76A6 File Offset: 0x002D58A6
			public NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection.Enumerator GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection.Enumerator(this._dict);
			}

			// Token: 0x060039B0 RID: 14768 RVA: 0x002D76B3 File Offset: 0x002D58B3
			IEnumerator<TKey> IEnumerable<!0>.GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection.Enumerator(this._dict);
			}

			// Token: 0x060039B1 RID: 14769 RVA: 0x002D76C5 File Offset: 0x002D58C5
			IEnumerator IEnumerable.GetEnumerator()
			{
				return new NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>.KeysCollection.Enumerator(this._dict);
			}

			// Token: 0x04002696 RID: 9878
			private Dictionary<TKey, TField> _dict;

			// Token: 0x020006DE RID: 1758
			public struct Enumerator : IEnumerator<!0>, IEnumerator, IDisposable
			{
				// Token: 0x0600466B RID: 18027 RVA: 0x003236E6 File Offset: 0x003218E6
				public Enumerator(Dictionary<TKey, TField> dict)
				{
					this._dict = dict;
					this._enumerator = this._dict.GetEnumerator();
					this._current = default(TKey);
					this._done = false;
				}

				// Token: 0x0600466C RID: 18028 RVA: 0x00323714 File Offset: 0x00321914
				public bool MoveNext()
				{
					if (!this._enumerator.MoveNext())
					{
						this._done = true;
						this._current = default(TKey);
						return false;
					}
					KeyValuePair<TKey, TField> pair = this._enumerator.Current;
					this._current = pair.Key;
					return true;
				}

				// Token: 0x17000530 RID: 1328
				// (get) Token: 0x0600466D RID: 18029 RVA: 0x0032375F File Offset: 0x0032195F
				public TKey Current
				{
					get
					{
						return this._current;
					}
				}

				// Token: 0x0600466E RID: 18030 RVA: 0x00323767 File Offset: 0x00321967
				public void Dispose()
				{
				}

				// Token: 0x17000531 RID: 1329
				// (get) Token: 0x0600466F RID: 18031 RVA: 0x00323769 File Offset: 0x00321969
				object IEnumerator.Current
				{
					get
					{
						if (this._done)
						{
							throw new InvalidOperationException();
						}
						return this._current;
					}
				}

				// Token: 0x06004670 RID: 18032 RVA: 0x00323784 File Offset: 0x00321984
				void IEnumerator.Reset()
				{
					this._enumerator = this._dict.GetEnumerator();
					this._current = default(TKey);
					this._done = false;
				}

				// Token: 0x040030C0 RID: 12480
				private readonly Dictionary<TKey, TField> _dict;

				// Token: 0x040030C1 RID: 12481
				private Dictionary<TKey, TField>.Enumerator _enumerator;

				// Token: 0x040030C2 RID: 12482
				private TKey _current;

				// Token: 0x040030C3 RID: 12483
				private bool _done;
			}
		}
	}
}

using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200003D RID: 61
	public abstract class NetFieldBase<T, TSelf> : AbstractNetSerializable, IEquatable<!1>, InterpolationCancellable where TSelf : NetFieldBase<!0, !1>
	{
		// Token: 0x1400000A RID: 10
		// (add) Token: 0x0600025B RID: 603 RVA: 0x0000E0E4 File Offset: 0x0000C2E4
		// (remove) Token: 0x0600025C RID: 604 RVA: 0x0000E11C File Offset: 0x0000C31C
		public event FieldChange<TSelf, T> fieldChangeEvent;

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600025D RID: 605 RVA: 0x0000E154 File Offset: 0x0000C354
		// (remove) Token: 0x0600025E RID: 606 RVA: 0x0000E18C File Offset: 0x0000C38C
		public event FieldChange<TSelf, T> fieldChangeVisibleEvent;

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600025F RID: 607 RVA: 0x0000E1C1 File Offset: 0x0000C3C1
		// (set) Token: 0x06000260 RID: 608 RVA: 0x0000E1CE File Offset: 0x0000C3CE
		public bool InterpolationEnabled
		{
			get
			{
				return (this._bools & NetFieldBase<T, TSelf>.NetFieldBaseBool.InterpolationEnabled) > NetFieldBase<T, TSelf>.NetFieldBaseBool.None;
			}
			set
			{
				if (value)
				{
					this._bools |= NetFieldBase<T, TSelf>.NetFieldBaseBool.InterpolationEnabled;
					return;
				}
				this._bools &= ~NetFieldBase<T, TSelf>.NetFieldBaseBool.InterpolationEnabled;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000261 RID: 609 RVA: 0x0000E1F4 File Offset: 0x0000C3F4
		// (set) Token: 0x06000262 RID: 610 RVA: 0x0000E201 File Offset: 0x0000C401
		public bool ExtrapolationEnabled
		{
			get
			{
				return (this._bools & NetFieldBase<T, TSelf>.NetFieldBaseBool.ExtrapolationEnabled) > NetFieldBase<T, TSelf>.NetFieldBaseBool.None;
			}
			set
			{
				if (value)
				{
					this._bools |= NetFieldBase<T, TSelf>.NetFieldBaseBool.ExtrapolationEnabled;
					return;
				}
				this._bools &= ~NetFieldBase<T, TSelf>.NetFieldBaseBool.ExtrapolationEnabled;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000263 RID: 611 RVA: 0x0000E227 File Offset: 0x0000C427
		// (set) Token: 0x06000264 RID: 612 RVA: 0x0000E234 File Offset: 0x0000C434
		public bool InterpolationWait
		{
			get
			{
				return (this._bools & NetFieldBase<T, TSelf>.NetFieldBaseBool.InterpolationWait) > NetFieldBase<T, TSelf>.NetFieldBaseBool.None;
			}
			set
			{
				if (value)
				{
					this._bools |= NetFieldBase<T, TSelf>.NetFieldBaseBool.InterpolationWait;
					return;
				}
				this._bools &= ~NetFieldBase<T, TSelf>.NetFieldBaseBool.InterpolationWait;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000265 RID: 613 RVA: 0x0000E25A File Offset: 0x0000C45A
		// (set) Token: 0x06000266 RID: 614 RVA: 0x0000E267 File Offset: 0x0000C467
		protected bool notifyOnTargetValueChange
		{
			get
			{
				return (this._bools & NetFieldBase<T, TSelf>.NetFieldBaseBool.notifyOnTargetValueChange) > NetFieldBase<T, TSelf>.NetFieldBaseBool.None;
			}
			set
			{
				if (value)
				{
					this._bools |= NetFieldBase<T, TSelf>.NetFieldBaseBool.notifyOnTargetValueChange;
					return;
				}
				this._bools &= ~NetFieldBase<T, TSelf>.NetFieldBaseBool.notifyOnTargetValueChange;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000267 RID: 615 RVA: 0x0000E28D File Offset: 0x0000C48D
		public T TargetValue
		{
			get
			{
				return this.targetValue;
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000268 RID: 616 RVA: 0x0000E295 File Offset: 0x0000C495
		// (set) Token: 0x06000269 RID: 617 RVA: 0x0000E29D File Offset: 0x0000C49D
		public T Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.Set(value);
			}
		}

		// Token: 0x0600026A RID: 618 RVA: 0x0000E2A6 File Offset: 0x0000C4A6
		public NetFieldBase()
		{
			this.InterpolationWait = true;
			this.value = default(T);
			this.previousValue = default(T);
			this.targetValue = default(T);
		}

		// Token: 0x0600026B RID: 619 RVA: 0x0000E2D9 File Offset: 0x0000C4D9
		public NetFieldBase(T value) : this()
		{
			this.cleanSet(value);
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0000E2E8 File Offset: 0x0000C4E8
		public TSelf Interpolated(bool interpolate, bool wait)
		{
			this.InterpolationEnabled = interpolate;
			this.InterpolationWait = wait;
			return (TSelf)((object)this);
		}

		// Token: 0x0600026D RID: 621 RVA: 0x0000E2FE File Offset: 0x0000C4FE
		protected virtual int InterpolationTicks()
		{
			if (base.Root == null)
			{
				return 0;
			}
			return base.Root.Clock.InterpolationTicks;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0000E31A File Offset: 0x0000C51A
		protected float InterpolationFactor()
		{
			return (base.Root.Clock.GetLocalTick() - this.interpolationStartTick) / (float)this.InterpolationTicks();
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0000E33D File Offset: 0x0000C53D
		public bool IsInterpolating()
		{
			return this.InterpolationEnabled && base.NeedsTick;
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0000E34F File Offset: 0x0000C54F
		public bool IsChanging()
		{
			return base.NeedsTick;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000E358 File Offset: 0x0000C558
		protected override bool tickImpl()
		{
			if (base.Root != null && this.InterpolationTicks() > 0)
			{
				float factor = this.InterpolationFactor();
				bool shouldExtrapolate = this.ExtrapolationEnabled && this.ChangeVersion[0] == base.Root.Clock.netVersion[0];
				if ((factor < 1f && this.InterpolationEnabled) || (shouldExtrapolate && factor < 3f))
				{
					this.value = this.interpolate(this.previousValue, this.targetValue, factor);
					return true;
				}
				if (factor < 1f && this.InterpolationWait)
				{
					this.value = this.previousValue;
					return true;
				}
			}
			T oldValue = this.previousValue;
			this.CancelInterpolation();
			if (this.fieldChangeVisibleEvent != null)
			{
				this.fieldChangeVisibleEvent((TSelf)((object)this), oldValue, this.value);
			}
			return false;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0000E435 File Offset: 0x0000C635
		public void CancelInterpolation()
		{
			if (base.NeedsTick)
			{
				this.value = this.targetValue;
				this.previousValue = default(T);
				base.NeedsTick = false;
			}
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0000E45E File Offset: 0x0000C65E
		public T Get()
		{
			return this.value;
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0000E466 File Offset: 0x0000C666
		protected virtual T interpolate(T startValue, T endValue, float factor)
		{
			return startValue;
		}

		// Token: 0x06000275 RID: 629
		public abstract void Set(T newValue);

		// Token: 0x06000276 RID: 630 RVA: 0x0000E469 File Offset: 0x0000C669
		protected bool canShortcutSet()
		{
			return this.Dirty && this.fieldChangeEvent == null && this.fieldChangeVisibleEvent == null;
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0000E486 File Offset: 0x0000C686
		protected virtual void targetValueChanged(T oldValue, T newValue)
		{
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0000E488 File Offset: 0x0000C688
		protected void cleanSet(T newValue)
		{
			T oldValue = this.value;
			T oldTargetValue = this.targetValue;
			this.targetValue = newValue;
			this.value = newValue;
			this.previousValue = default(T);
			base.NeedsTick = false;
			if (this.notifyOnTargetValueChange)
			{
				this.targetValueChanged(oldTargetValue, newValue);
			}
			if (this.fieldChangeEvent != null)
			{
				this.fieldChangeEvent((TSelf)((object)this), oldValue, newValue);
			}
			if (this.fieldChangeVisibleEvent != null)
			{
				this.fieldChangeVisibleEvent((TSelf)((object)this), oldValue, newValue);
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0000E50A File Offset: 0x0000C70A
		protected virtual bool setUpInterpolation(T oldValue, T newValue)
		{
			return true;
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0000E510 File Offset: 0x0000C710
		protected void setInterpolationTarget(T newValue)
		{
			T oldValue = this.value;
			if (!this.InterpolationWait || base.Root == null || !this.setUpInterpolation(oldValue, newValue))
			{
				this.cleanSet(newValue);
				return;
			}
			T oldTargetValue = this.targetValue;
			this.previousValue = oldValue;
			base.NeedsTick = true;
			this.targetValue = newValue;
			this.interpolationStartTick = base.Root.Clock.GetLocalTick();
			if (this.notifyOnTargetValueChange)
			{
				this.targetValueChanged(oldTargetValue, newValue);
			}
			if (this.fieldChangeEvent != null)
			{
				this.fieldChangeEvent((TSelf)((object)this), oldValue, newValue);
			}
		}

		// Token: 0x0600027B RID: 635
		protected abstract void ReadDelta(BinaryReader reader, NetVersion version);

		// Token: 0x0600027C RID: 636
		protected abstract void WriteDelta(BinaryWriter writer);

		// Token: 0x0600027D RID: 637 RVA: 0x0000E5A3 File Offset: 0x0000C7A3
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.ReadDelta(reader, version);
			this.CancelInterpolation();
			this.ChangeVersion.Merge(version);
		}

		// Token: 0x0600027E RID: 638 RVA: 0x0000E5BF File Offset: 0x0000C7BF
		public override void WriteFull(BinaryWriter writer)
		{
			this.WriteDelta(writer);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000E5C8 File Offset: 0x0000C7C8
		public override void Read(BinaryReader reader, NetVersion version)
		{
			this.ReadDelta(reader, version);
			this.ChangeVersion.Merge(version);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0000E5DE File Offset: 0x0000C7DE
		public override void Write(BinaryWriter writer)
		{
			this.WriteDelta(writer);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000E5E7 File Offset: 0x0000C7E7
		public override string ToString()
		{
			if (this.value != null)
			{
				return this.value.ToString();
			}
			return "null";
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0000E610 File Offset: 0x0000C810
		public override bool Equals(object obj)
		{
			TSelf otherField = obj as TSelf;
			return (otherField != null && this.Equals(otherField)) || object.Equals(this.Value, obj);
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0000E64D File Offset: 0x0000C84D
		public bool Equals(TSelf other)
		{
			return object.Equals(this.Value, other.Value);
		}

		// Token: 0x06000284 RID: 644 RVA: 0x0000E66F File Offset: 0x0000C86F
		public static bool operator ==(NetFieldBase<T, TSelf> self, TSelf other)
		{
			return self == other || object.Equals(self, other);
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0000E688 File Offset: 0x0000C888
		public static bool operator !=(NetFieldBase<T, TSelf> self, TSelf other)
		{
			return self != other && !object.Equals(self, other);
		}

		// Token: 0x06000286 RID: 646 RVA: 0x0000E6A4 File Offset: 0x0000C8A4
		public override int GetHashCode()
		{
			return ((this.value != null) ? this.value.GetHashCode() : 0) ^ -858436897;
		}

		// Token: 0x04000168 RID: 360
		protected NetFieldBase<T, TSelf>.NetFieldBaseBool _bools;

		// Token: 0x04000169 RID: 361
		protected uint interpolationStartTick;

		// Token: 0x0400016A RID: 362
		protected T value;

		// Token: 0x0400016B RID: 363
		protected T previousValue;

		// Token: 0x0400016C RID: 364
		protected T targetValue;

		// Token: 0x020003E0 RID: 992
		[Flags]
		protected enum NetFieldBaseBool : byte
		{
			// Token: 0x040026AF RID: 9903
			None = 0,
			// Token: 0x040026B0 RID: 9904
			InterpolationEnabled = 1,
			// Token: 0x040026B1 RID: 9905
			ExtrapolationEnabled = 2,
			// Token: 0x040026B2 RID: 9906
			InterpolationWait = 4,
			// Token: 0x040026B3 RID: 9907
			notifyOnTargetValueChange = 8
		}
	}
}

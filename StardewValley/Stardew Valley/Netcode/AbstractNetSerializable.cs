using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Netcode
{
	// Token: 0x0200002E RID: 46
	public abstract class AbstractNetSerializable : INetSerializable, INetObject<INetSerializable>
	{
		// Token: 0x0600015B RID: 347 RVA: 0x0000B4B8 File Offset: 0x000096B8
		public void ResetNewestReceivedChangeVersion()
		{
			this.ChangeVersion.Clear();
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600015C RID: 348 RVA: 0x0000B4C5 File Offset: 0x000096C5
		// (set) Token: 0x0600015D RID: 349 RVA: 0x0000B4CD File Offset: 0x000096CD
		public uint DirtyTick
		{
			get
			{
				return this.dirtyTick;
			}
			set
			{
				if (value < this.dirtyTick)
				{
					this.SetDirtySooner(value);
					return;
				}
				if (value > this.dirtyTick)
				{
					this.SetDirtyLater(value);
				}
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x0600015E RID: 350 RVA: 0x0000B4F0 File Offset: 0x000096F0
		public virtual bool Dirty
		{
			get
			{
				return this.dirtyTick != uint.MaxValue;
			}
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000B500 File Offset: 0x00009700
		protected void SetDirtySooner(uint tick)
		{
			tick = Math.Max(tick, this.minNextDirtyTime);
			if (this.dirtyTick <= tick)
			{
				return;
			}
			this.dirtyTick = tick;
			if (this.Parent != null)
			{
				this.Parent.DirtyTick = Math.Min(this.Parent.DirtyTick, tick);
			}
			if (this.Root != null)
			{
				this.minNextDirtyTime = this.Root.Clock.GetLocalTick() + (uint)this.DeltaAggregateTicks;
				this.ChangeVersion.Set(this.Root.Clock.netVersion);
				return;
			}
			this.minNextDirtyTime = 0U;
			this.ChangeVersion.Clear();
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000B5A4 File Offset: 0x000097A4
		protected void SetDirtyLater(uint tick)
		{
			if (this.dirtyTick >= tick)
			{
				return;
			}
			this.dirtyTick = tick;
			this.ForEachChild(delegate(INetSerializable child)
			{
				child.DirtyTick = Math.Max(child.DirtyTick, tick);
			});
			if (tick == 4294967295U)
			{
				this.CleanImpl();
			}
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000B5FA File Offset: 0x000097FA
		protected virtual void CleanImpl()
		{
			if (this.Root == null)
			{
				this.minNextDirtyTime = 0U;
				return;
			}
			this.minNextDirtyTime = this.Root.Clock.GetLocalTick() + (uint)this.DeltaAggregateTicks;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x0000B629 File Offset: 0x00009829
		public void MarkDirty()
		{
			if (this.Root == null)
			{
				this.SetDirtySooner(0U);
				return;
			}
			this.SetDirtySooner(this.Root.Clock.GetLocalTick());
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000B651 File Offset: 0x00009851
		public void MarkClean()
		{
			this.SetDirtyLater(uint.MaxValue);
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000164 RID: 356 RVA: 0x0000B65A File Offset: 0x0000985A
		// (set) Token: 0x06000165 RID: 357 RVA: 0x0000B662 File Offset: 0x00009862
		public bool NeedsTick
		{
			get
			{
				return this.needsTick;
			}
			set
			{
				if (value != this.needsTick)
				{
					this.needsTick = value;
					if (value && this.Parent != null)
					{
						this.Parent.ChildNeedsTick = true;
					}
				}
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000166 RID: 358 RVA: 0x0000B68B File Offset: 0x0000988B
		// (set) Token: 0x06000167 RID: 359 RVA: 0x0000B693 File Offset: 0x00009893
		public bool ChildNeedsTick
		{
			get
			{
				return this.childNeedsTick;
			}
			set
			{
				if (value != this.childNeedsTick)
				{
					this.childNeedsTick = value;
					if (value && this.Parent != null)
					{
						this.Parent.ChildNeedsTick = true;
					}
				}
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000168 RID: 360 RVA: 0x0000B6BC File Offset: 0x000098BC
		// (set) Token: 0x06000169 RID: 361 RVA: 0x0000B6C4 File Offset: 0x000098C4
		public string Name { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x0600016A RID: 362 RVA: 0x0000B6CD File Offset: 0x000098CD
		// (set) Token: 0x0600016B RID: 363 RVA: 0x0000B6D5 File Offset: 0x000098D5
		public INetRoot Root { get; protected set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x0600016C RID: 364 RVA: 0x0000B6DE File Offset: 0x000098DE
		// (set) Token: 0x0600016D RID: 365 RVA: 0x0000B6E6 File Offset: 0x000098E6
		public INetSerializable Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.SetParent(value);
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x0600016E RID: 366 RVA: 0x0000B6EF File Offset: 0x000098EF
		public INetSerializable NetFields
		{
			get
			{
				return this;
			}
		}

		// Token: 0x0600016F RID: 367 RVA: 0x0000B6F2 File Offset: 0x000098F2
		protected virtual bool tickImpl()
		{
			return false;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0000B6F8 File Offset: 0x000098F8
		public bool Tick()
		{
			if (this.needsTick)
			{
				this.needsTick = this.tickImpl();
			}
			if (this.childNeedsTick)
			{
				this.childNeedsTick = false;
				this.ForEachChild(delegate(INetSerializable child)
				{
					if (child.NeedsTick || child.ChildNeedsTick)
					{
						this.childNeedsTick |= child.Tick();
					}
				});
			}
			return this.childNeedsTick | this.needsTick;
		}

		// Token: 0x06000171 RID: 369
		public abstract void Read(BinaryReader reader, NetVersion version);

		// Token: 0x06000172 RID: 370
		public abstract void Write(BinaryWriter writer);

		// Token: 0x06000173 RID: 371
		public abstract void ReadFull(BinaryReader reader, NetVersion version);

		// Token: 0x06000174 RID: 372
		public abstract void WriteFull(BinaryWriter writer);

		// Token: 0x06000175 RID: 373 RVA: 0x0000B747 File Offset: 0x00009947
		protected uint GetLocalTick()
		{
			if (this.Root != null)
			{
				return this.Root.Clock.GetLocalTick();
			}
			return 0U;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0000B764 File Offset: 0x00009964
		protected NetVersion GetLocalVersion()
		{
			NetVersion version = default(NetVersion);
			if (this.Root != null)
			{
				version.Set(this.Root.Clock.netVersion);
			}
			return version;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x0000B799 File Offset: 0x00009999
		protected virtual void SetParent(INetSerializable parent)
		{
			this.parent = parent;
			if (parent != null)
			{
				this.Root = parent.Root;
				this.SetChildParents();
			}
			else
			{
				this.ClearChildParents();
			}
			this.MarkClean();
			this.ChangeVersion.Clear();
			this.minNextDirtyTime = 0U;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x0000B7D7 File Offset: 0x000099D7
		protected virtual void SetChildParents()
		{
			this.ForEachChild(delegate(INetSerializable child)
			{
				child.Parent = this;
			});
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000B7EB File Offset: 0x000099EB
		protected virtual void ClearChildParents()
		{
			this.ForEachChild(delegate(INetSerializable child)
			{
				if (child.Parent == this)
				{
					child.Parent = null;
				}
			});
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000B800 File Offset: 0x00009A00
		protected virtual void ValidateChild(INetSerializable child)
		{
			if (child == null)
			{
				throw new InvalidOperationException("Net field '" + this.Name + "' incorrectly contains a null field.");
			}
			if ((this.Parent != null || this.Root == this) && child.Parent != this)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Net field '");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' has child '");
				defaultInterpolatedStringHandler.AppendFormatted(child.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' which is already linked to parent '");
				INetSerializable netSerializable = child.Parent;
				defaultInterpolatedStringHandler.AppendFormatted(((netSerializable != null) ? netSerializable.Name : null) ?? "<null>");
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000B8CE File Offset: 0x00009ACE
		protected virtual void ValidateChildren()
		{
			if (this.Parent != null || this.Root == this)
			{
				this.ForEachChild(new Action<INetSerializable>(this.ValidateChild));
			}
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000B8F4 File Offset: 0x00009AF4
		protected virtual void ForEachChild(Action<INetSerializable> childAction)
		{
		}

		// Token: 0x04000143 RID: 323
		private uint dirtyTick = uint.MaxValue;

		// Token: 0x04000144 RID: 324
		private uint minNextDirtyTime;

		// Token: 0x04000145 RID: 325
		protected NetVersion ChangeVersion;

		// Token: 0x04000146 RID: 326
		public ushort DeltaAggregateTicks;

		// Token: 0x04000147 RID: 327
		private bool needsTick;

		// Token: 0x04000148 RID: 328
		private bool childNeedsTick;

		// Token: 0x0400014B RID: 331
		private INetSerializable parent;
	}
}

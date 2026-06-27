using System;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E8 RID: 488
	public abstract class NetPausableField<T, TField, TBaseField> : INetObject<NetFields> where TField : !2, new() where TBaseField : NetFieldBase<!0, !2>, new()
	{
		// Token: 0x17000383 RID: 899
		// (get) Token: 0x0600218B RID: 8587 RVA: 0x00173C8B File Offset: 0x00171E8B
		// (set) Token: 0x0600218C RID: 8588 RVA: 0x00173C93 File Offset: 0x00171E93
		public T Value
		{
			get
			{
				return this.Get();
			}
			set
			{
				this.Set(value);
			}
		}

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x0600218D RID: 8589 RVA: 0x00173C9C File Offset: 0x00171E9C
		// (set) Token: 0x0600218E RID: 8590 RVA: 0x00173CAF File Offset: 0x00171EAF
		public bool Paused
		{
			get
			{
				this.pauseEvent.Poll();
				return this.paused;
			}
			set
			{
				if (value != this.paused)
				{
					this.pauseEvent.Fire(value);
					this.pauseEvent.Poll();
				}
			}
		}

		// Token: 0x17000385 RID: 901
		// (get) Token: 0x0600218F RID: 8591
		public abstract NetFields NetFields { get; }

		// Token: 0x06002190 RID: 8592 RVA: 0x00173CD1 File Offset: 0x00171ED1
		public NetPausableField(TField field)
		{
			this.Field = field;
			this.initNetFields();
		}

		// Token: 0x06002191 RID: 8593 RVA: 0x00173CF4 File Offset: 0x00171EF4
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.Field, "Field").AddField(this.pauseEvent, "pauseEvent");
			this.pauseEvent.onEvent += delegate(bool newPauseValue)
			{
				this.paused = newPauseValue;
			};
		}

		// Token: 0x06002192 RID: 8594 RVA: 0x00173D4A File Offset: 0x00171F4A
		public NetPausableField() : this(Activator.CreateInstance<TField>())
		{
		}

		// Token: 0x06002193 RID: 8595 RVA: 0x00173D57 File Offset: 0x00171F57
		public virtual T Get()
		{
			if (this.Paused)
			{
				this.Field.CancelInterpolation();
			}
			return this.Field.Get();
		}

		// Token: 0x06002194 RID: 8596 RVA: 0x00173D81 File Offset: 0x00171F81
		public void Set(T value)
		{
			this.Field.Set(value);
		}

		// Token: 0x06002195 RID: 8597 RVA: 0x00173D94 File Offset: 0x00171F94
		public bool IsPausePending()
		{
			return this.pauseEvent.HasPendingEvent((bool p) => p);
		}

		// Token: 0x06002196 RID: 8598 RVA: 0x00173DC0 File Offset: 0x00171FC0
		public bool IsInterpolating()
		{
			return this.Field.IsInterpolating() && !this.Paused;
		}

		// Token: 0x0400140C RID: 5132
		private bool paused;

		// Token: 0x0400140D RID: 5133
		public readonly TField Field;

		// Token: 0x0400140E RID: 5134
		private readonly NetEvent1Field<bool, NetBool> pauseEvent = new NetEvent1Field<bool, NetBool>();
	}
}

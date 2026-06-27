using System;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001EA RID: 490
	public sealed class NetPosition : NetPausableField<Vector2, NetVector2, NetVector2>
	{
		// Token: 0x17000386 RID: 902
		// (get) Token: 0x0600219C RID: 8604 RVA: 0x00173E36 File Offset: 0x00172036
		public override NetFields NetFields { get; } = new NetFields("NetPosition");

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x0600219D RID: 8605 RVA: 0x00173E40 File Offset: 0x00172040
		// (remove) Token: 0x0600219E RID: 8606 RVA: 0x00173E78 File Offset: 0x00172078
		public event FieldChange<NetPosition, Vector2> fieldChangeEvent;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x0600219F RID: 8607 RVA: 0x00173EB0 File Offset: 0x001720B0
		// (remove) Token: 0x060021A0 RID: 8608 RVA: 0x00173EE8 File Offset: 0x001720E8
		public event FieldChange<NetPosition, Vector2> fieldChangeVisibleEvent;

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x060021A1 RID: 8609 RVA: 0x00173F1D File Offset: 0x0017211D
		// (set) Token: 0x060021A2 RID: 8610 RVA: 0x00173F2A File Offset: 0x0017212A
		public float X
		{
			get
			{
				return this.Get().X;
			}
			set
			{
				base.Set(new Vector2(value, this.Y));
			}
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x060021A3 RID: 8611 RVA: 0x00173F3E File Offset: 0x0017213E
		// (set) Token: 0x060021A4 RID: 8612 RVA: 0x00173F4B File Offset: 0x0017214B
		public float Y
		{
			get
			{
				return this.Get().Y;
			}
			set
			{
				base.Set(new Vector2(this.X, value));
			}
		}

		// Token: 0x060021A5 RID: 8613 RVA: 0x00173F5F File Offset: 0x0017215F
		public NetPosition() : base(new NetVector2().Interpolated(true, true))
		{
		}

		// Token: 0x060021A6 RID: 8614 RVA: 0x00173F95 File Offset: 0x00172195
		public NetPosition(NetVector2 field) : base(field)
		{
		}

		// Token: 0x060021A7 RID: 8615 RVA: 0x00173FC0 File Offset: 0x001721C0
		protected override void initNetFields()
		{
			base.initNetFields();
			this.NetFields.AddField(this.moving, "moving");
			this.NetFields.DeltaAggregateTicks = 0;
			this.Field.fieldChangeEvent += delegate(NetVector2 f, Vector2 oldValue, Vector2 newValue)
			{
				if (this.IsMaster())
				{
					this.moving.Value = true;
				}
				FieldChange<NetPosition, Vector2> fieldChange = this.fieldChangeEvent;
				if (fieldChange == null)
				{
					return;
				}
				fieldChange(this, oldValue, newValue);
			};
			this.Field.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 oldValue, Vector2 newValue)
			{
				FieldChange<NetPosition, Vector2> fieldChange = this.fieldChangeVisibleEvent;
				if (fieldChange == null)
				{
					return;
				}
				fieldChange(this, oldValue, newValue);
			};
			this.moving.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (!this.IsMaster())
				{
					this.Field.ExtrapolationEnabled = (newValue && this.ExtrapolationEnabled);
				}
			};
		}

		// Token: 0x060021A8 RID: 8616 RVA: 0x0017403B File Offset: 0x0017223B
		protected bool IsMaster()
		{
			INetRoot root = this.NetFields.Root;
			return root != null && root.Clock.LocalId == 0;
		}

		// Token: 0x060021A9 RID: 8617 RVA: 0x0017405B File Offset: 0x0017225B
		public override Vector2 Get()
		{
			if (Game1.HostPaused)
			{
				this.Field.CancelInterpolation();
			}
			return base.Get();
		}

		// Token: 0x060021AA RID: 8618 RVA: 0x00174075 File Offset: 0x00172275
		public Vector2 CurrentInterpolationDirection()
		{
			if (base.Paused)
			{
				return Vector2.Zero;
			}
			return this.Field.CurrentInterpolationDirection();
		}

		// Token: 0x060021AB RID: 8619 RVA: 0x00174090 File Offset: 0x00172290
		public float CurrentInterpolationSpeed()
		{
			if (base.Paused)
			{
				return 0f;
			}
			return this.Field.CurrentInterpolationSpeed();
		}

		// Token: 0x060021AC RID: 8620 RVA: 0x001740AC File Offset: 0x001722AC
		public void UpdateExtrapolation(float extrapolationSpeed)
		{
			this.NetFields.DeltaAggregateTicks = ((this.NetFields.Root != null) ? ((ushort)((float)this.NetFields.Root.Clock.InterpolationTicks * 0.8f)) : 0);
			this.ExtrapolationEnabled = true;
			this.Field.ExtrapolationSpeed = extrapolationSpeed;
			if (this.IsMaster())
			{
				this.moving.Value = false;
			}
		}

		// Token: 0x0400140F RID: 5135
		private const float SmoothingFudge = 0.8f;

		// Token: 0x04001410 RID: 5136
		private const ushort DefaultDeltaAggregateTicks = 0;

		// Token: 0x04001414 RID: 5140
		public bool ExtrapolationEnabled;

		// Token: 0x04001415 RID: 5141
		public readonly NetBool moving = new NetBool().Interpolated(false, false);
	}
}

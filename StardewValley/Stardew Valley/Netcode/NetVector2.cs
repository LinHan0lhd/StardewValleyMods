using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode
{
	// Token: 0x02000049 RID: 73
	public sealed class NetVector2 : NetField<Vector2, NetVector2>
	{
		// Token: 0x060002CD RID: 717 RVA: 0x0000EE02 File Offset: 0x0000D002
		public NetVector2()
		{
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000EE20 File Offset: 0x0000D020
		public NetVector2(Vector2 value) : base(value)
		{
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060002CF RID: 719 RVA: 0x0000EE3F File Offset: 0x0000D03F
		// (set) Token: 0x060002D0 RID: 720 RVA: 0x0000EE4C File Offset: 0x0000D04C
		public float X
		{
			get
			{
				return base.Value.X;
			}
			set
			{
				Vector2 vector = this.value;
				if (vector.X != value)
				{
					Vector2 newValue = new Vector2(value, vector.Y);
					if (base.canShortcutSet())
					{
						this.value = newValue;
						return;
					}
					base.cleanSet(newValue);
					base.MarkDirty();
				}
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060002D1 RID: 721 RVA: 0x0000EE94 File Offset: 0x0000D094
		// (set) Token: 0x060002D2 RID: 722 RVA: 0x0000EEA4 File Offset: 0x0000D0A4
		public float Y
		{
			get
			{
				return base.Value.Y;
			}
			set
			{
				Vector2 vector = this.value;
				if (vector.Y != value)
				{
					Vector2 newValue = new Vector2(vector.X, value);
					if (base.canShortcutSet())
					{
						this.value = newValue;
						return;
					}
					base.cleanSet(newValue);
					base.MarkDirty();
				}
			}
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000EEEC File Offset: 0x0000D0EC
		public void Set(float x, float y)
		{
			this.Set(new Vector2(x, y));
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000EEFB File Offset: 0x0000D0FB
		public override void Set(Vector2 newValue)
		{
			if (base.canShortcutSet())
			{
				this.value = newValue;
				return;
			}
			if (newValue != this.value)
			{
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000EF28 File Offset: 0x0000D128
		public Vector2 InterpolationDelta()
		{
			if (base.NeedsTick)
			{
				return this.targetValue - this.previousValue;
			}
			return Vector2.Zero;
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000EF4C File Offset: 0x0000D14C
		protected override bool setUpInterpolation(Vector2 oldValue, Vector2 newValue)
		{
			if ((newValue - oldValue).LengthSquared() >= this.MaxInterpolationDistance * this.MaxInterpolationDistance)
			{
				return false;
			}
			if (this.AxisAlignedMovement)
			{
				if (base.NeedsTick)
				{
					Vector2 delta = this.targetValue - this.previousValue;
					Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
					if (this.interpolateXFirst)
					{
						this.interpolateXFirst = (base.InterpolationFactor() * (absDelta.X + absDelta.Y) < absDelta.X);
					}
					else
					{
						this.interpolateXFirst = (base.InterpolationFactor() * (absDelta.X + absDelta.Y) > absDelta.Y);
					}
				}
				else
				{
					Vector2 delta2 = newValue - oldValue;
					Vector2 absDelta2 = new Vector2(Math.Abs(delta2.X), Math.Abs(delta2.Y));
					this.interpolateXFirst = (absDelta2.X < absDelta2.Y);
				}
			}
			return true;
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0000F048 File Offset: 0x0000D248
		public Vector2 CurrentInterpolationDirection()
		{
			if (!this.AxisAlignedMovement)
			{
				Vector2 delta = this.InterpolationDelta();
				delta.Normalize();
				return delta;
			}
			float factor = base.InterpolationFactor();
			Vector2 delta2 = this.InterpolationDelta();
			float traveledLength = (Math.Abs(delta2.X) + Math.Abs(delta2.Y)) * factor;
			if (Math.Abs(delta2.X) < this.MinDeltaForDirectionChange && Math.Abs(delta2.Y) < this.MinDeltaForDirectionChange)
			{
				return Vector2.Zero;
			}
			if (Math.Abs(delta2.X) < this.MinDeltaForDirectionChange)
			{
				return new Vector2(0f, (float)Math.Sign(delta2.Y));
			}
			if (Math.Abs(delta2.Y) < this.MinDeltaForDirectionChange)
			{
				return new Vector2((float)Math.Sign(delta2.X), 0f);
			}
			if (this.interpolateXFirst)
			{
				if (traveledLength > Math.Abs(delta2.X))
				{
					return new Vector2(0f, (float)Math.Sign(delta2.Y));
				}
				return new Vector2((float)Math.Sign(delta2.X), 0f);
			}
			else
			{
				if (traveledLength > Math.Abs(delta2.Y))
				{
					return new Vector2((float)Math.Sign(delta2.X), 0f);
				}
				return new Vector2(0f, (float)Math.Sign(delta2.Y));
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0000F198 File Offset: 0x0000D398
		public float CurrentInterpolationSpeed()
		{
			float distance = this.InterpolationDelta().Length();
			if (this.InterpolationTicks() == 0)
			{
				return distance;
			}
			if (base.InterpolationFactor() > 1f)
			{
				return this.ExtrapolationSpeed;
			}
			return distance / (float)this.InterpolationTicks();
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000F1DC File Offset: 0x0000D3DC
		protected override Vector2 interpolate(Vector2 startValue, Vector2 endValue, float factor)
		{
			if (this.AxisAlignedMovement && factor <= 1f && !this.isFixingExtrapolation)
			{
				this.isExtrapolating = false;
				Vector2 delta = this.InterpolationDelta();
				Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
				float traveledLength = (absDelta.X + absDelta.Y) * factor;
				float x;
				float y;
				if (this.interpolateXFirst)
				{
					if (traveledLength > absDelta.X)
					{
						x = endValue.X;
						y = startValue.Y + (traveledLength - absDelta.X) * (float)Math.Sign(delta.Y);
					}
					else
					{
						x = startValue.X + traveledLength * (float)Math.Sign(delta.X);
						y = startValue.Y;
					}
				}
				else if (traveledLength > absDelta.Y)
				{
					y = endValue.Y;
					x = startValue.X + (traveledLength - absDelta.Y) * (float)Math.Sign(delta.X);
				}
				else
				{
					y = startValue.Y + traveledLength * (float)Math.Sign(delta.Y);
					x = startValue.X;
				}
				return new Vector2(x, y);
			}
			if (factor > 1f)
			{
				this.isExtrapolating = true;
				uint extrapolationTicks = base.Root.Clock.GetLocalTick() - this.interpolationStartTick - (uint)this.InterpolationTicks();
				Vector2 direction = endValue - startValue;
				if (direction.LengthSquared() > this.ExtrapolationSpeed * this.ExtrapolationSpeed)
				{
					direction.Normalize();
					return endValue + direction * extrapolationTicks * this.ExtrapolationSpeed;
				}
			}
			this.isExtrapolating = false;
			return startValue + (endValue - startValue) * factor;
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000F380 File Offset: 0x0000D580
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			float newX = reader.ReadSingle();
			float newY = reader.ReadSingle();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				this.isFixingExtrapolation = this.isExtrapolating;
				base.setInterpolationTarget(new Vector2(newX, newY));
				this.isExtrapolating = false;
			}
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000F3CA File Offset: 0x0000D5CA
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(base.Value.X);
			writer.Write(base.Value.Y);
		}

		// Token: 0x04000173 RID: 371
		public bool AxisAlignedMovement;

		// Token: 0x04000174 RID: 372
		public float ExtrapolationSpeed;

		// Token: 0x04000175 RID: 373
		public float MinDeltaForDirectionChange = 8f;

		// Token: 0x04000176 RID: 374
		public float MaxInterpolationDistance = 320f;

		// Token: 0x04000177 RID: 375
		private bool interpolateXFirst;

		// Token: 0x04000178 RID: 376
		private bool isExtrapolating;

		// Token: 0x04000179 RID: 377
		private bool isFixingExtrapolation;
	}
}

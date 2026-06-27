using System;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001DE RID: 478
	public sealed class NetDirection : NetField<int, NetDirection>
	{
		// Token: 0x06002134 RID: 8500 RVA: 0x00172B30 File Offset: 0x00170D30
		public NetDirection()
		{
			base.InterpolationEnabled = true;
			base.InterpolationWait = true;
		}

		// Token: 0x06002135 RID: 8501 RVA: 0x00172B46 File Offset: 0x00170D46
		public NetDirection(int value) : base(value)
		{
			base.InterpolationEnabled = true;
			base.InterpolationWait = true;
		}

		// Token: 0x06002136 RID: 8502 RVA: 0x00172B5D File Offset: 0x00170D5D
		public override void Set(int newValue)
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

		// Token: 0x06002137 RID: 8503 RVA: 0x00172B85 File Offset: 0x00170D85
		protected override bool setUpInterpolation(int oldValue, int newValue)
		{
			return true;
		}

		// Token: 0x06002138 RID: 8504 RVA: 0x00172B88 File Offset: 0x00170D88
		public int getInterpolatedDirection()
		{
			if (this.Position != null && this.Position.IsInterpolating() && !this.Position.IsPausePending())
			{
				Vector2 dir = this.Position.CurrentInterpolationDirection();
				if (Math.Abs(dir.X) > Math.Abs(dir.Y))
				{
					if (dir.X < 0f)
					{
						return 3;
					}
					return 1;
				}
				else if (Math.Abs(dir.Y) > Math.Abs(dir.X))
				{
					if (dir.Y < 0f)
					{
						return 0;
					}
					return 2;
				}
			}
			return this.value;
		}

		// Token: 0x06002139 RID: 8505 RVA: 0x00172C1C File Offset: 0x00170E1C
		protected override int interpolate(int startValue, int endValue, float factor)
		{
			if (this.Position != null && this.Position.IsInterpolating() && !this.Position.IsPausePending())
			{
				Vector2 dir = this.Position.CurrentInterpolationDirection();
				if (Math.Abs(dir.X) > Math.Abs(dir.Y))
				{
					if (dir.X < 0f)
					{
						return 3;
					}
					return 1;
				}
				else if (Math.Abs(dir.Y) > Math.Abs(dir.X))
				{
					if (dir.Y < 0f)
					{
						return 0;
					}
					return 2;
				}
			}
			return startValue;
		}

		// Token: 0x0600213A RID: 8506 RVA: 0x00172CAC File Offset: 0x00170EAC
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newValue = reader.ReadInt32();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x0600213B RID: 8507 RVA: 0x00172CD6 File Offset: 0x00170ED6
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}

		// Token: 0x040013EE RID: 5102
		public NetPosition Position;
	}
}

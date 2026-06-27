using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x02000087 RID: 135
	public struct ButtonCollection
	{
		// Token: 0x060004FD RID: 1277 RVA: 0x0001A024 File Offset: 0x00018224
		public ButtonCollection(ref GamePadState padState, ref GamePadState oldPadState)
		{
			this._count = 0;
			this._pressed = (Buttons)0;
			if (padState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
			{
				this._pressed |= Buttons.A;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.B) && !oldPadState.IsButtonDown(Buttons.B))
			{
				this._pressed |= Buttons.B;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
			{
				this._pressed |= Buttons.X;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.Y) && !oldPadState.IsButtonDown(Buttons.Y))
			{
				this._pressed |= Buttons.Y;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.Start) && !oldPadState.IsButtonDown(Buttons.Start))
			{
				this._pressed |= Buttons.Start;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.Back) && !oldPadState.IsButtonDown(Buttons.Back))
			{
				this._pressed |= Buttons.Back;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.RightTrigger) && !oldPadState.IsButtonDown(Buttons.RightTrigger))
			{
				this._pressed |= Buttons.RightTrigger;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftTrigger) && !oldPadState.IsButtonDown(Buttons.LeftTrigger))
			{
				this._pressed |= Buttons.LeftTrigger;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.RightShoulder) && !oldPadState.IsButtonDown(Buttons.RightShoulder))
			{
				this._pressed |= Buttons.RightShoulder;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftShoulder) && !oldPadState.IsButtonDown(Buttons.LeftShoulder))
			{
				this._pressed |= Buttons.LeftShoulder;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadUp) && !oldPadState.IsButtonDown(Buttons.DPadUp))
			{
				this._pressed |= Buttons.DPadUp;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadRight) && !oldPadState.IsButtonDown(Buttons.DPadRight))
			{
				this._pressed |= Buttons.DPadRight;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadDown) && !oldPadState.IsButtonDown(Buttons.DPadDown))
			{
				this._pressed |= Buttons.DPadDown;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadLeft) && !oldPadState.IsButtonDown(Buttons.DPadLeft))
			{
				this._pressed |= Buttons.DPadLeft;
				this._count++;
			}
			if ((double)padState.ThumbSticks.Left.Y > 0.2 && (double)oldPadState.ThumbSticks.Left.Y <= 0.2 && Utility.thumbstickIsInDirection(0, padState))
			{
				this._pressed |= Buttons.LeftThumbstickUp;
				this._count++;
			}
			if ((double)padState.ThumbSticks.Left.X > 0.2 && (double)oldPadState.ThumbSticks.Left.X <= 0.2 && Utility.thumbstickIsInDirection(1, padState))
			{
				this._pressed |= Buttons.LeftThumbstickRight;
				this._count++;
			}
			if ((double)padState.ThumbSticks.Left.Y < -0.2 && (double)oldPadState.ThumbSticks.Left.Y >= -0.2 && Utility.thumbstickIsInDirection(2, padState))
			{
				this._pressed |= Buttons.LeftThumbstickDown;
				this._count++;
			}
			if ((double)padState.ThumbSticks.Left.X < -0.2 && (double)oldPadState.ThumbSticks.Left.X >= -0.2 && Utility.thumbstickIsInDirection(3, padState))
			{
				this._pressed |= Buttons.LeftThumbstickLeft;
				this._count++;
			}
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x0001A4DC File Offset: 0x000186DC
		public ButtonCollection(ref GamePadState padState)
		{
			this._count = 0;
			this._pressed = (Buttons)0;
			if (padState.IsButtonDown(Buttons.A))
			{
				this._pressed |= Buttons.A;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.B))
			{
				this._pressed |= Buttons.B;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.X))
			{
				this._pressed |= Buttons.X;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.Y))
			{
				this._pressed |= Buttons.Y;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.Start))
			{
				this._pressed |= Buttons.Start;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.Back))
			{
				this._pressed |= Buttons.Back;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.RightTrigger))
			{
				this._pressed |= Buttons.RightTrigger;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftTrigger))
			{
				this._pressed |= Buttons.LeftTrigger;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.RightShoulder))
			{
				this._pressed |= Buttons.RightShoulder;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftShoulder))
			{
				this._pressed |= Buttons.LeftShoulder;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadUp))
			{
				this._pressed |= Buttons.DPadUp;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadRight))
			{
				this._pressed |= Buttons.DPadRight;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadDown))
			{
				this._pressed |= Buttons.DPadDown;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.DPadLeft))
			{
				this._pressed |= Buttons.DPadLeft;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickUp))
			{
				this._pressed |= Buttons.LeftThumbstickUp;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickRight))
			{
				this._pressed |= Buttons.LeftThumbstickRight;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickDown))
			{
				this._pressed |= Buttons.LeftThumbstickDown;
				this._count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickLeft))
			{
				this._pressed |= Buttons.LeftThumbstickLeft;
				this._count++;
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060004FF RID: 1279 RVA: 0x0001A7F5 File Offset: 0x000189F5
		public int Count
		{
			get
			{
				return this._count;
			}
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0001A7FD File Offset: 0x000189FD
		public ButtonCollection.ButtonEnumerator GetEnumerator()
		{
			return new ButtonCollection.ButtonEnumerator(this._pressed);
		}

		// Token: 0x04000231 RID: 561
		private readonly Buttons _pressed;

		// Token: 0x04000232 RID: 562
		private readonly int _count;

		// Token: 0x02000402 RID: 1026
		public struct ButtonEnumerator
		{
			// Token: 0x06003A4C RID: 14924 RVA: 0x002D872B File Offset: 0x002D692B
			public ButtonEnumerator(Buttons pressed)
			{
				this._pressed = pressed;
				this._current = -1;
			}

			// Token: 0x170004B9 RID: 1209
			// (get) Token: 0x06003A4D RID: 14925 RVA: 0x002D873B File Offset: 0x002D693B
			public Buttons Current
			{
				get
				{
					if (this._current < 0 || this._current > 32)
					{
						throw new InvalidOperationException();
					}
					return (Buttons)(1 << this._current);
				}
			}

			// Token: 0x06003A4E RID: 14926 RVA: 0x002D8761 File Offset: 0x002D6961
			public bool MoveNext()
			{
				if (this._pressed == (Buttons)0)
				{
					return false;
				}
				while (this._current < 31)
				{
					this._current++;
					if ((this._pressed & (Buttons)(1 << this._current)) != (Buttons)0)
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06003A4F RID: 14927 RVA: 0x002D879C File Offset: 0x002D699C
			public void Reset()
			{
				this._current = -1;
			}

			// Token: 0x040026F6 RID: 9974
			private readonly Buttons _pressed;

			// Token: 0x040026F7 RID: 9975
			private int _current;
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000B6 RID: 182
	public class InputState
	{
		// Token: 0x06000CA2 RID: 3234 RVA: 0x0008EA06 File Offset: 0x0008CC06
		public virtual void UpdateStates()
		{
			this._currentKeyboardState = Keyboard.GetState();
			this._currentMouseState = Mouse.GetState();
			if (Game1.playerOneIndex >= PlayerIndex.One)
			{
				this._currentGamepadState = GamePad.GetState(Game1.playerOneIndex);
				return;
			}
			this._currentGamepadState = default(GamePadState);
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x0008EA43 File Offset: 0x0008CC43
		public virtual void Update()
		{
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x0008EA45 File Offset: 0x0008CC45
		public virtual void IgnoreKeys(Keys[] keys)
		{
			if (keys.Length != 0)
			{
				this._ignoredKeys.AddRange(keys);
			}
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x0008EA58 File Offset: 0x0008CC58
		public virtual KeyboardState GetKeyboardState()
		{
			if (!Game1.game1.IsMainInstance || !Game1.game1.HasKeyboardFocus())
			{
				return default(KeyboardState);
			}
			if (this._lastKeyStateTick != Game1.ticks || this._keyState == null)
			{
				if (this._ignoredKeys.Count == 0)
				{
					this._keyState = new KeyboardState?(this._currentKeyboardState);
				}
				else
				{
					this._pressedKeys.Clear();
					this._pressedKeys.AddRange(this._currentKeyboardState.GetPressedKeys());
					this._ignoredKeys.RemoveAll((Keys key) => !this._pressedKeys.Contains(key));
					this._pressedKeys.RemoveAll((Keys key) => this._ignoredKeys.Contains(key));
					this._keyState = new KeyboardState?(new KeyboardState(this._pressedKeys.ToArray()));
				}
				this._lastKeyStateTick = Game1.ticks;
			}
			return this._keyState.Value;
		}

		// Token: 0x06000CA6 RID: 3238 RVA: 0x0008EB48 File Offset: 0x0008CD48
		public virtual GamePadState GetGamePadState()
		{
			if (Game1.options.gamepadMode == Options.GamepadModes.ForceOff || Game1.playerOneIndex == (PlayerIndex)(-1))
			{
				return default(GamePadState);
			}
			return this._currentGamepadState;
		}

		// Token: 0x06000CA7 RID: 3239 RVA: 0x0008EB7A File Offset: 0x0008CD7A
		public virtual MouseState GetMouseState()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return new MouseState(this._simulatedMousePosition.X, this._simulatedMousePosition.Y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
			}
			return this._currentMouseState;
		}

		// Token: 0x06000CA8 RID: 3240 RVA: 0x0008EBB0 File Offset: 0x0008CDB0
		public virtual void SetMousePosition(int x, int y)
		{
			if (!Game1.game1.IsMainInstance)
			{
				this._simulatedMousePosition.X = x;
				this._simulatedMousePosition.Y = y;
				return;
			}
			Mouse.SetPosition(x, y);
			this._currentMouseState = new MouseState(x, y, this._currentMouseState.ScrollWheelValue, this._currentMouseState.LeftButton, this._currentMouseState.MiddleButton, this._currentMouseState.RightButton, this._currentMouseState.XButton1, this._currentMouseState.XButton2);
		}

		// Token: 0x040008AC RID: 2220
		protected Point _simulatedMousePosition = Point.Zero;

		// Token: 0x040008AD RID: 2221
		protected List<Keys> _ignoredKeys = new List<Keys>();

		// Token: 0x040008AE RID: 2222
		protected List<Keys> _pressedKeys = new List<Keys>();

		// Token: 0x040008AF RID: 2223
		protected KeyboardState? _keyState;

		// Token: 0x040008B0 RID: 2224
		protected int _lastKeyStateTick = -1;

		// Token: 0x040008B1 RID: 2225
		protected KeyboardState _currentKeyboardState;

		// Token: 0x040008B2 RID: 2226
		protected MouseState _currentMouseState;

		// Token: 0x040008B3 RID: 2227
		protected GamePadState _currentGamepadState;
	}
}

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.NativeClipboard;

namespace StardewValley
{
	// Token: 0x020000C3 RID: 195
	public class KeyboardDispatcher
	{
		// Token: 0x06000D7B RID: 3451 RVA: 0x000925F4 File Offset: 0x000907F4
		public void Cleanup()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				this._window.TextInput -= this.Event_TextInput;
			}
			else
			{
				KeyboardInput.CharEntered -= this.EventInput_CharEntered;
				KeyboardInput.KeyDown -= this.EventInput_KeyDown;
			}
			this._window = null;
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x00092660 File Offset: 0x00090860
		public KeyboardDispatcher(GameWindow window)
		{
			this._commandInputs = new List<char>();
			this._keysDown = new List<Keys>();
			this._charsEntered = new List<char>();
			this._window = window;
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				window.TextInput += this.Event_TextInput;
				return;
			}
			if (Game1.game1.IsMainInstance)
			{
				KeyboardInput.Initialize(window);
			}
			KeyboardInput.CharEntered += this.EventInput_CharEntered;
			KeyboardInput.KeyDown += this.EventInput_KeyDown;
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x00092728 File Offset: 0x00090928
		private void Event_KeyDown(object sender, Keys key)
		{
			if (this._subscriber == null)
			{
				return;
			}
			if (key != Keys.Back)
			{
				if (key != Keys.Tab)
				{
					if (key == Keys.Enter)
					{
						this._commandInputs.Add('\r');
					}
				}
				else
				{
					this._commandInputs.Add('\t');
				}
			}
			else
			{
				this._commandInputs.Add('\b');
			}
			this._keysDown.Add(key);
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x00092784 File Offset: 0x00090984
		private void Event_TextInput(object sender, TextInputEventArgs e)
		{
			if (this._subscriber == null)
			{
				return;
			}
			Keys key = e.Key;
			if (key == Keys.Back)
			{
				this._commandInputs.Add('\b');
				return;
			}
			if (key == Keys.Tab)
			{
				this._commandInputs.Add('\t');
				return;
			}
			if (key == Keys.Enter)
			{
				this._commandInputs.Add('\r');
				return;
			}
			if (char.IsControl(e.Character))
			{
				return;
			}
			this._charsEntered.Add(e.Character);
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x000927F9 File Offset: 0x000909F9
		private void EventInput_KeyDown(object sender, KeyEventArgs e)
		{
			this._keysDown.Add(e.KeyCode);
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x0009280C File Offset: 0x00090A0C
		private void EventInput_CharEntered(object sender, CharacterEventArgs e)
		{
			if (this._subscriber == null)
			{
				return;
			}
			if (!char.IsControl(e.Character))
			{
				this._charsEntered.Add(e.Character);
				return;
			}
			if (e.Character == '\u0016')
			{
				Thread thread = new Thread(new ThreadStart(this.PasteThread));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
				this._enteredText = this._pasteResult;
				return;
			}
			this._commandInputs.Add(e.Character);
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x0009288C File Offset: 0x00090A8C
		public bool ShouldSuppress()
		{
			return false;
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x0009288F File Offset: 0x00090A8F
		public void Discard()
		{
			this._enteredText = null;
			this._charsEntered.Clear();
			this._commandInputs.Clear();
			this._keysDown.Clear();
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x000928BC File Offset: 0x00090ABC
		public void Poll()
		{
			KeyboardState keyboard_state = Game1.input.GetKeyboardState();
			bool modifier_held = (SdlClipboard.Platform == ClipboardPlatformType.OSX) ? (keyboard_state.IsKeyDown(Keys.LeftWindows) || keyboard_state.IsKeyDown(Keys.RightWindows)) : (keyboard_state.IsKeyDown(Keys.LeftControl) || keyboard_state.IsKeyDown(Keys.RightControl));
			if (keyboard_state.IsKeyDown(Keys.V) && !this._oldKeyboardState.IsKeyDown(Keys.V) && modifier_held)
			{
				string pasted_text = null;
				DesktopClipboard.GetText(ref pasted_text);
				if (pasted_text != null)
				{
					this._enteredText = pasted_text;
				}
			}
			this._oldKeyboardState = keyboard_state;
			if (this._enteredText != null)
			{
				if (this._subscriber != null && !this.ShouldSuppress())
				{
					this._subscriber.RecieveTextInput(this._enteredText);
				}
				this._enteredText = null;
			}
			if (this._charsEntered.Count > 0)
			{
				if (this._subscriber != null && !this.ShouldSuppress())
				{
					foreach (char key in this._charsEntered)
					{
						this._subscriber.RecieveTextInput(key);
						if (this._subscriber == null)
						{
							break;
						}
					}
				}
				this._charsEntered.Clear();
			}
			if (this._commandInputs.Count > 0)
			{
				if (this._subscriber != null && !this.ShouldSuppress())
				{
					foreach (char key2 in this._commandInputs)
					{
						this._subscriber.RecieveCommandInput(key2);
						if (this._subscriber == null)
						{
							break;
						}
					}
				}
				this._commandInputs.Clear();
			}
			if (this._keysDown.Count > 0)
			{
				if (this._subscriber != null && !this.ShouldSuppress())
				{
					foreach (Keys key3 in this._keysDown)
					{
						this._subscriber.RecieveSpecialInput(key3);
						if (this._subscriber == null)
						{
							break;
						}
					}
				}
				this._keysDown.Clear();
			}
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000D84 RID: 3460 RVA: 0x00092AFC File Offset: 0x00090CFC
		// (set) Token: 0x06000D85 RID: 3461 RVA: 0x00092B04 File Offset: 0x00090D04
		public IKeyboardSubscriber Subscriber
		{
			get
			{
				return this._subscriber;
			}
			set
			{
				if (this._subscriber == value)
				{
					return;
				}
				if (this._subscriber != null)
				{
					this._subscriber.Selected = false;
				}
				this._subscriber = value;
				if (this._subscriber != null)
				{
					this._subscriber.Selected = true;
				}
			}
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x00092B3F File Offset: 0x00090D3F
		[STAThread]
		private void PasteThread()
		{
			this._pasteResult = "";
		}

		// Token: 0x04000904 RID: 2308
		protected string _enteredText;

		// Token: 0x04000905 RID: 2309
		protected List<char> _commandInputs = new List<char>();

		// Token: 0x04000906 RID: 2310
		protected List<Keys> _keysDown = new List<Keys>();

		// Token: 0x04000907 RID: 2311
		protected List<char> _charsEntered = new List<char>();

		// Token: 0x04000908 RID: 2312
		protected GameWindow _window;

		// Token: 0x04000909 RID: 2313
		protected KeyboardState _oldKeyboardState;

		// Token: 0x0400090A RID: 2314
		private IKeyboardSubscriber _subscriber;

		// Token: 0x0400090B RID: 2315
		private string _pasteResult = "";
	}
}

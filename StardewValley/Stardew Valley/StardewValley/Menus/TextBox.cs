using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.SDKs.Steam;

namespace StardewValley.Menus
{
	// Token: 0x020002B2 RID: 690
	public class TextBox : IKeyboardSubscriber
	{
		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06002CF9 RID: 11513 RVA: 0x0022F098 File Offset: 0x0022D298
		public SpriteFont Font
		{
			get
			{
				return this._font;
			}
		}

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06002CFA RID: 11514 RVA: 0x0022F0A0 File Offset: 0x0022D2A0
		public Color TextColor
		{
			get
			{
				return this._textColor;
			}
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06002CFB RID: 11515 RVA: 0x0022F0A8 File Offset: 0x0022D2A8
		// (set) Token: 0x06002CFC RID: 11516 RVA: 0x0022F0B0 File Offset: 0x0022D2B0
		public int X { get; set; }

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06002CFD RID: 11517 RVA: 0x0022F0B9 File Offset: 0x0022D2B9
		// (set) Token: 0x06002CFE RID: 11518 RVA: 0x0022F0C1 File Offset: 0x0022D2C1
		public int Y { get; set; }

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x06002CFF RID: 11519 RVA: 0x0022F0CA File Offset: 0x0022D2CA
		// (set) Token: 0x06002D00 RID: 11520 RVA: 0x0022F0D2 File Offset: 0x0022D2D2
		public int Width { get; set; }

		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x06002D01 RID: 11521 RVA: 0x0022F0DB File Offset: 0x0022D2DB
		// (set) Token: 0x06002D02 RID: 11522 RVA: 0x0022F0E3 File Offset: 0x0022D2E3
		public int Height { get; set; }

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06002D03 RID: 11523 RVA: 0x0022F0EC File Offset: 0x0022D2EC
		// (set) Token: 0x06002D04 RID: 11524 RVA: 0x0022F0F4 File Offset: 0x0022D2F4
		public bool PasswordBox { get; set; }

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06002D05 RID: 11525 RVA: 0x0022F0FD File Offset: 0x0022D2FD
		// (set) Token: 0x06002D06 RID: 11526 RVA: 0x0022F108 File Offset: 0x0022D308
		public string Text
		{
			get
			{
				return this._text;
			}
			set
			{
				this._text = value;
				if (this._text == null)
				{
					this._text = "";
				}
				if (this._text != "")
				{
					this._text = Utility.FilterDirtyWordsIfStrictPlatform(this._text);
					if (this.limitWidth && this._font.MeasureString(this._text).X > (float)(this.Width - 21))
					{
						this.Text = this._text.Substring(0, this._text.Length - 1);
					}
				}
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06002D07 RID: 11527 RVA: 0x0022F19B File Offset: 0x0022D39B
		// (set) Token: 0x06002D08 RID: 11528 RVA: 0x0022F1A3 File Offset: 0x0022D3A3
		public string TitleText { get; set; }

		// Token: 0x06002D09 RID: 11529 RVA: 0x0022F1AC File Offset: 0x0022D3AC
		public TextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor)
		{
			this._textBoxTexture = textBoxTexture;
			if (textBoxTexture != null)
			{
				this.Width = textBoxTexture.Width;
				this.Height = textBoxTexture.Height;
			}
			this._caretTexture = caretTexture;
			this._font = font;
			this._textColor = textColor;
		}

		// Token: 0x06002D0A RID: 11530 RVA: 0x0022F210 File Offset: 0x0022D410
		public void SelectMe()
		{
			this.Selected = true;
		}

		// Token: 0x06002D0B RID: 11531 RVA: 0x0022F21C File Offset: 0x0022D41C
		public void Update()
		{
			Point mousePoint = new Point(Game1.getMouseX(), Game1.getMouseY());
			Rectangle position = new Rectangle(this.X, this.Y, this.Width, this.Height);
			if (position.Contains(mousePoint))
			{
				this.Selected = true;
			}
			else
			{
				this.Selected = false;
			}
			if (this._showKeyboard)
			{
				if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
				{
					Game1.showTextEntry(this);
				}
				this._showKeyboard = false;
			}
		}

		// Token: 0x06002D0C RID: 11532 RVA: 0x0022F29C File Offset: 0x0022D49C
		public virtual void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
		{
			bool caretVisible = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
			string toDraw = this.Text;
			if (this.PasswordBox)
			{
				toDraw = "";
				for (int i = 0; i < this.Text.Length; i++)
				{
					toDraw += "•";
				}
			}
			if (this._textBoxTexture != null)
			{
				spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X, this.Y, 16, this.Height), new Rectangle?(new Rectangle(0, 0, 16, this.Height)), Color.White);
				spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X + 16, this.Y, this.Width - 32, this.Height), new Rectangle?(new Rectangle(16, 0, 4, this.Height)), Color.White);
				spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X + this.Width - 16, this.Y, 16, this.Height), new Rectangle?(new Rectangle(this._textBoxTexture.Bounds.Width - 16, 0, 16, this.Height)), Color.White);
			}
			else
			{
				Game1.drawDialogueBox(this.X - 32, this.Y - 112 + 10, this.Width + 80, this.Height, false, true, null, false, true, -1, -1, -1);
			}
			Vector2 size = this._font.MeasureString(toDraw);
			while (size.X > (float)this.Width)
			{
				toDraw = toDraw.Substring(1);
				size = this._font.MeasureString(toDraw);
			}
			if (caretVisible && this.Selected)
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(this.X + 16 + (int)size.X + 2, this.Y + 8, 4, 32), this._textColor);
			}
			if (drawShadow)
			{
				Utility.drawTextWithShadow(spriteBatch, toDraw, this._font, new Vector2((float)(this.X + 16), (float)(this.Y + ((this._textBoxTexture != null) ? 12 : 8))), this._textColor, 1f, -1f, -1, -1, 1f, 3);
				return;
			}
			spriteBatch.DrawString(this._font, toDraw, new Vector2((float)(this.X + 16), (float)(this.Y + ((this._textBoxTexture != null) ? 12 : 8))), this._textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
		}

		// Token: 0x06002D0D RID: 11533 RVA: 0x0022F538 File Offset: 0x0022D738
		public virtual void RecieveTextInput(char inputChar)
		{
			if (this.Selected && (!this.numbersOnly || char.IsDigit(inputChar)) && (this.textLimit == -1 || this.Text.Length < this.textLimit))
			{
				if (Game1.gameMode != 3)
				{
					if (inputChar <= '*')
					{
						if (inputChar == '"')
						{
							return;
						}
						if (inputChar == '$')
						{
							Game1.playSound("money", null);
							goto IL_F2;
						}
						if (inputChar == '*')
						{
							Game1.playSound("hammer", null);
							goto IL_F2;
						}
					}
					else
					{
						if (inputChar == '+')
						{
							Game1.playSound("slimeHit", null);
							goto IL_F2;
						}
						if (inputChar == '<')
						{
							Game1.playSound("crystal", new int?(0));
							goto IL_F2;
						}
						if (inputChar == '=')
						{
							Game1.playSound("coin", null);
							goto IL_F2;
						}
					}
					Game1.playSound("cowboy_monsterhit", null);
				}
				IL_F2:
				this.Text += inputChar.ToString();
			}
		}

		// Token: 0x06002D0E RID: 11534 RVA: 0x0022F650 File Offset: 0x0022D850
		public virtual void RecieveTextInput(string text)
		{
			int dummy = -1;
			if (this.Selected && (!this.numbersOnly || int.TryParse(text, out dummy)) && (this.textLimit == -1 || this.Text.Length < this.textLimit))
			{
				this.Text += text;
			}
		}

		// Token: 0x06002D0F RID: 11535 RVA: 0x0022F6A8 File Offset: 0x0022D8A8
		public virtual void RecieveCommandInput(char command)
		{
			if (this.Selected)
			{
				if (command != '\b')
				{
					if (command != '\t')
					{
						if (command != '\r')
						{
							return;
						}
						TextBoxEvent onEnterPressed = this.OnEnterPressed;
						if (onEnterPressed == null)
						{
							return;
						}
						onEnterPressed(this);
						return;
					}
					else
					{
						TextBoxEvent onTabPressed = this.OnTabPressed;
						if (onTabPressed == null)
						{
							return;
						}
						onTabPressed(this);
					}
				}
				else if (this.Text.Length > 0)
				{
					if (this.OnBackspacePressed != null)
					{
						this.OnBackspacePressed(this);
						return;
					}
					this.Text = this.Text.Substring(0, this.Text.Length - 1);
					if (Game1.gameMode != 3)
					{
						Game1.playSound("tinyWhip", null);
						return;
					}
				}
			}
		}

		// Token: 0x06002D10 RID: 11536 RVA: 0x0022F751 File Offset: 0x0022D951
		public void RecieveSpecialInput(Keys key)
		{
		}

		// Token: 0x06002D11 RID: 11537 RVA: 0x0022F753 File Offset: 0x0022D953
		public void Hover(int x, int y)
		{
			if (x > this.X && x < this.X + this.Width && y > this.Y && y < this.Y + this.Height)
			{
				Game1.SetFreeCursorDrag();
			}
		}

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x06002D12 RID: 11538 RVA: 0x0022F78C File Offset: 0x0022D98C
		// (remove) Token: 0x06002D13 RID: 11539 RVA: 0x0022F7C4 File Offset: 0x0022D9C4
		public event TextBoxEvent OnEnterPressed;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x06002D14 RID: 11540 RVA: 0x0022F7FC File Offset: 0x0022D9FC
		// (remove) Token: 0x06002D15 RID: 11541 RVA: 0x0022F834 File Offset: 0x0022DA34
		public event TextBoxEvent OnTabPressed;

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x06002D16 RID: 11542 RVA: 0x0022F86C File Offset: 0x0022DA6C
		// (remove) Token: 0x06002D17 RID: 11543 RVA: 0x0022F8A4 File Offset: 0x0022DAA4
		public event TextBoxEvent OnBackspacePressed;

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06002D18 RID: 11544 RVA: 0x0022F8D9 File Offset: 0x0022DAD9
		// (set) Token: 0x06002D19 RID: 11545 RVA: 0x0022F8E4 File Offset: 0x0022DAE4
		public bool Selected
		{
			get
			{
				return this._selected;
			}
			set
			{
				if (this._selected == value)
				{
					return;
				}
				this._selected = value;
				if (this._selected)
				{
					Game1.keyboardDispatcher.Subscriber = this;
					this._showKeyboard = true;
					return;
				}
				this._showKeyboard = false;
				SteamHelper steamHelper = Program.sdk as SteamHelper;
				if (steamHelper != null && steamHelper.active)
				{
					steamHelper.CancelKeyboard();
				}
				if (Game1.keyboardDispatcher.Subscriber == this)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
			}
		}

		// Token: 0x04001EAA RID: 7850
		protected Texture2D _textBoxTexture;

		// Token: 0x04001EAB RID: 7851
		protected Texture2D _caretTexture;

		// Token: 0x04001EAC RID: 7852
		protected SpriteFont _font;

		// Token: 0x04001EAD RID: 7853
		protected Color _textColor;

		// Token: 0x04001EB3 RID: 7859
		public bool numbersOnly;

		// Token: 0x04001EB4 RID: 7860
		public int textLimit = -1;

		// Token: 0x04001EB5 RID: 7861
		public bool limitWidth = true;

		// Token: 0x04001EB6 RID: 7862
		private string _text = "";

		// Token: 0x04001EBB RID: 7867
		private bool _showKeyboard;

		// Token: 0x04001EBC RID: 7868
		private bool _selected;
	}
}

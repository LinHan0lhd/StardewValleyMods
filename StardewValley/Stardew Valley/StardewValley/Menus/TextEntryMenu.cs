using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x020002B3 RID: 691
	public class TextEntryMenu : IClickableMenu
	{
		// Token: 0x06002D1A RID: 11546 RVA: 0x0022F958 File Offset: 0x0022DB58
		public override void receiveGamePadButton(Buttons button)
		{
			if (button <= Buttons.B)
			{
				if (button == Buttons.Start)
				{
					this.OnSubmit();
					return;
				}
				if (button == Buttons.B)
				{
					this.Close();
					return;
				}
			}
			else
			{
				if (button == Buttons.X)
				{
					this.OnBackSpace();
					return;
				}
				if (button == Buttons.Y)
				{
					this.OnSpaceBar();
					return;
				}
			}
			base.receiveGamePadButton(button);
		}

		// Token: 0x06002D1B RID: 11547 RVA: 0x0022F9AF File Offset: 0x0022DBAF
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				this.Close();
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x06002D1C RID: 11548 RVA: 0x0022F9C4 File Offset: 0x0022DBC4
		public TextEntryMenu(TextBox target) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352, 0, 0).Y, 672, 352, false)
		{
			this._target = target;
			this._lettersPerRow = this.letterMaps[0][0].Length;
			for (int i = 0; i < this.letterMaps[0].Length; i++)
			{
				for (int j = 0; j < this._lettersPerRow; j++)
				{
					ClickableTextureComponent key_component = new ClickableTextureComponent(new Rectangle(0, 0, 1024, 1024), Game1.mouseCursors2, new Rectangle(32, 176, 16, 16), 4f, false);
					key_component.myID = i * this._lettersPerRow + j;
					key_component.leftNeighborID = -99998;
					key_component.rightNeighborID = -99998;
					key_component.upNeighborID = -99998;
					key_component.downNeighborID = -99998;
					if (i == this.letterMaps[0].Length - 1)
					{
						if (j >= 2 && j <= this._lettersPerRow - 4)
						{
							key_component.downNeighborID = 99991;
							key_component.downNeighborImmutable = true;
						}
						if (j >= this._lettersPerRow - 3 && j <= this._lettersPerRow - 2)
						{
							key_component.downNeighborID = 99990;
							key_component.downNeighborImmutable = true;
						}
					}
					this.keys.Add(key_component);
				}
			}
			this.backspaceButton = new ClickableTextureComponent(new Rectangle(0, 0, 128, 64), Game1.mouseCursors2, new Rectangle(32, 144, 32, 16), 4f, false)
			{
				myID = 99990,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.spaceButton = new ClickableTextureComponent(new Rectangle(0, 0, 320, 64), Game1.mouseCursors2, new Rectangle(0, 160, 80, 16), 4f, false)
			{
				myID = 99991,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.okButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 144, 16, 16), 4f, false)
			{
				myID = 99992,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.upperCaseButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 144, 16, 16), 4f, false)
			{
				myID = 99993,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.symbolsButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 144, 16, 16), 4f, false)
			{
				myID = 99994,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.ShowKeyboard(0, false);
			this.RepositionElements();
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
			Game1.playSound("bigSelect", null);
		}

		// Token: 0x06002D1D RID: 11549 RVA: 0x0022FE08 File Offset: 0x0022E008
		public override bool readyToClose()
		{
			return false;
		}

		// Token: 0x06002D1E RID: 11550 RVA: 0x0022FE0C File Offset: 0x0022E00C
		public void ShowKeyboard(int index, bool play_sound = true)
		{
			this._currentKeyboard = index;
			int key_index = 0;
			foreach (string key_map in this.letterMaps[index])
			{
				foreach (char key_character in key_map)
				{
					this.keys[key_index].name = (key_character.ToString() ?? "");
					key_index++;
				}
			}
			this.upperCaseButton.sourceRect = new Rectangle(0, 144, 16, 16);
			this.symbolsButton.sourceRect = new Rectangle(16, 144, 16, 16);
			int j = this._currentKeyboard;
			if (j != 1)
			{
				if (j == 2)
				{
					this.symbolsButton.sourceRect = new Rectangle(16, 176, 16, 16);
				}
			}
			else
			{
				this.upperCaseButton.sourceRect = new Rectangle(0, 176, 16, 16);
			}
			if (play_sound)
			{
				Game1.playSound("button1", null);
			}
		}

		// Token: 0x06002D1F RID: 11551 RVA: 0x0022FF16 File Offset: 0x0022E116
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(this._lettersPerRow);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002D20 RID: 11552 RVA: 0x0022FF30 File Offset: 0x0022E130
		public void RepositionElements()
		{
			this.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352, 0, 0).X;
			this.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 256, 0, 0).Y;
			for (int y = 0; y < this.keys.Count / this._lettersPerRow; y++)
			{
				for (int x = 0; x < this._lettersPerRow; x++)
				{
					this.keys[x + y * this._lettersPerRow].bounds = new Rectangle(this.xPositionOnScreen + 16 + x * 16 * 4, this.yPositionOnScreen + 16 + y * 16 * 4, 64, 64);
				}
			}
			this.upperCaseButton.bounds = new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + 256, this.upperCaseButton.bounds.Width, this.upperCaseButton.bounds.Height);
			this.symbolsButton.bounds = new Rectangle(this.xPositionOnScreen + 16 + 64, this.yPositionOnScreen + 16 + 256, this.symbolsButton.bounds.Width, this.symbolsButton.bounds.Height);
			this.backspaceButton.bounds = new Rectangle(this.xPositionOnScreen + 16 + 448, this.yPositionOnScreen + 16 + 256, this.backspaceButton.bounds.Width, this.backspaceButton.bounds.Height);
			this.spaceButton.bounds = new Rectangle(this.xPositionOnScreen + 16 + 128, this.yPositionOnScreen + 16 + 256, this.spaceButton.bounds.Width, this.spaceButton.bounds.Height);
			this.okButton.bounds = new Rectangle(this.xPositionOnScreen + 16 + 576, this.yPositionOnScreen + 16 + 256, this.okButton.bounds.Width, this.okButton.bounds.Height);
		}

		// Token: 0x06002D21 RID: 11553 RVA: 0x00230165 File Offset: 0x0022E365
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.RepositionElements();
		}

		// Token: 0x06002D22 RID: 11554 RVA: 0x00230178 File Offset: 0x0022E378
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableTextureComponent clickableTextureComponent in this.keys)
			{
				clickableTextureComponent.tryHover(x, y, 0.1f);
			}
			this.spaceButton.tryHover(x, y, 0.1f);
			this.backspaceButton.tryHover(x, y, 0.1f);
			this.okButton.tryHover(x, y, 0.1f);
			this.symbolsButton.tryHover(x, y, 0.1f);
			this.upperCaseButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002D23 RID: 11555 RVA: 0x00230234 File Offset: 0x0022E434
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in this.keys)
			{
				if (component.containsPoint(x, y))
				{
					this.OnLetter(component.name);
				}
			}
			if (this.okButton.containsPoint(x, y))
			{
				this.OnSubmit();
				return;
			}
			if (this.spaceButton.containsPoint(x, y))
			{
				this.OnSpaceBar();
			}
			if (this.upperCaseButton.containsPoint(x, y))
			{
				if (this._currentKeyboard != 1)
				{
					this.ShowKeyboard(1, true);
				}
				else
				{
					this.ShowKeyboard(0, true);
				}
			}
			if (this.symbolsButton.containsPoint(x, y))
			{
				if (this._currentKeyboard != 2)
				{
					this.ShowKeyboard(2, true);
				}
				else
				{
					this.ShowKeyboard(0, true);
				}
			}
			if (this.backspaceButton.containsPoint(x, y))
			{
				this.OnBackSpace();
			}
		}

		// Token: 0x06002D24 RID: 11556 RVA: 0x0023032C File Offset: 0x0022E52C
		public void OnSubmit()
		{
			this._target.RecieveCommandInput('\r');
			this.Close();
		}

		// Token: 0x06002D25 RID: 11557 RVA: 0x00230341 File Offset: 0x0022E541
		public void OnSpaceBar()
		{
			this._target.RecieveTextInput(' ');
		}

		// Token: 0x06002D26 RID: 11558 RVA: 0x00230350 File Offset: 0x0022E550
		public void OnBackSpace()
		{
			this._target.RecieveCommandInput('\b');
		}

		// Token: 0x06002D27 RID: 11559 RVA: 0x0023035E File Offset: 0x0022E55E
		public void OnLetter(string letter)
		{
			if (letter.Length > 0)
			{
				this._target.RecieveTextInput(letter[0]);
			}
		}

		// Token: 0x06002D28 RID: 11560 RVA: 0x0023037C File Offset: 0x0022E57C
		public void Close()
		{
			Game1.playSound("bigDeSelect", null);
			Game1.closeTextEntry();
		}

		// Token: 0x06002D29 RID: 11561 RVA: 0x002303A4 File Offset: 0x0022E5A4
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
			}
			Game1.DrawBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, null);
			foreach (ClickableTextureComponent key in this.keys)
			{
				key.draw(b);
				Vector2 size = Game1.dialogueFont.MeasureString(key.name);
				b.DrawString(Game1.dialogueFont, key.name, Utility.snapDrawPosition(new Vector2((float)key.bounds.Center.X - size.X / 2f, (float)key.bounds.Center.Y - size.Y / 2f)), Color.Black);
			}
			this.backspaceButton.draw(b);
			this.okButton.draw(b);
			this.spaceButton.draw(b);
			this.symbolsButton.draw(b);
			this.upperCaseButton.draw(b);
			if (this._target != null)
			{
				int x = this._target.X;
				int y = this._target.Y;
				this._target.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(this._target.Width, this._target.Height * 4, 0, 0).X;
				this._target.Y = this.yPositionOnScreen - 96;
				this._target.Draw(b, true);
				this._target.X = x;
				this._target.Y = y;
			}
			base.draw(b);
			base.drawMouse(b, true, -1);
		}

		// Token: 0x06002D2A RID: 11562 RVA: 0x002305AC File Offset: 0x0022E7AC
		public override void update(GameTime time)
		{
			if (this._target == null || !this._target.Selected)
			{
				this.Close();
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftStick) && !Game1.oldPadState.IsButtonDown(Buttons.LeftStick))
			{
				if (this._currentKeyboard != 1)
				{
					this.ShowKeyboard(1, true);
				}
				else
				{
					this.ShowKeyboard(0, true);
				}
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && !Game1.oldPadState.IsButtonDown(Buttons.RightStick))
			{
				if (this._currentKeyboard != 2)
				{
					this.ShowKeyboard(2, true);
					return;
				}
				this.ShowKeyboard(0, true);
			}
		}

		// Token: 0x04001EBD RID: 7869
		public const int borderSpace = 4;

		// Token: 0x04001EBE RID: 7870
		public const int buttonSize = 16;

		// Token: 0x04001EBF RID: 7871
		public const int windowWidth = 168;

		// Token: 0x04001EC0 RID: 7872
		public const int windowHeight = 88;

		// Token: 0x04001EC1 RID: 7873
		public string[][] letterMaps = new string[][]
		{
			new string[]
			{
				"1234567890",
				"qwertyuiop",
				"asdfghjkl'",
				"zxcvbnm,.?"
			},
			new string[]
			{
				"!@#$%^&*()",
				"QWERTYUIOP",
				"ASDFGHJKL\"",
				"ZXCVBNM,.?"
			},
			new string[]
			{
				"&%#|~$£~/\\",
				"-+=<>:;'\"`",
				"()[]{}.^°ñ",
				"áéíóúü¡!¿?"
			}
		};

		// Token: 0x04001EC2 RID: 7874
		public List<ClickableTextureComponent> keys = new List<ClickableTextureComponent>();

		// Token: 0x04001EC3 RID: 7875
		public ClickableTextureComponent backspaceButton;

		// Token: 0x04001EC4 RID: 7876
		public ClickableTextureComponent spaceButton;

		// Token: 0x04001EC5 RID: 7877
		public ClickableTextureComponent okButton;

		// Token: 0x04001EC6 RID: 7878
		public ClickableTextureComponent upperCaseButton;

		// Token: 0x04001EC7 RID: 7879
		public ClickableTextureComponent symbolsButton;

		// Token: 0x04001EC8 RID: 7880
		protected int _lettersPerRow;

		// Token: 0x04001EC9 RID: 7881
		protected TextBox _target;

		// Token: 0x04001ECA RID: 7882
		public int _currentKeyboard;
	}
}

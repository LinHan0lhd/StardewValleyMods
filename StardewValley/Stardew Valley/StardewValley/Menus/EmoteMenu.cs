using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x0200026B RID: 619
	public class EmoteMenu : IClickableMenu
	{
		// Token: 0x06002908 RID: 10504 RVA: 0x001E20A8 File Offset: 0x001E02A8
		public EmoteMenu()
		{
			this.menuBackgroundTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
			this.width = 256;
			this.height = 256;
			this.xPositionOnScreen = (int)((float)(Game1.viewport.Width / 2) - (float)this.width / 2f);
			this.yPositionOnScreen = (int)((float)(Game1.viewport.Height / 2) - (float)this.height / 2f);
			this.emotes = new List<string>();
			foreach (string emote_string in Game1.player.GetEmoteFavorites())
			{
				this.emotes.Add(emote_string);
			}
			this._mouseStartPosition = Game1.getMousePosition(false);
			this._alpha = 0f;
			this._menuCloseGracePeriod = 300;
			this._CreateEmoteButtons();
			this._SnapToPlayerPosition();
		}

		// Token: 0x06002909 RID: 10505 RVA: 0x001E21D4 File Offset: 0x001E03D4
		protected void _CreateEmoteButtons()
		{
			this._emoteButtons = new List<ClickableTextureComponent>();
			for (int i = 0; i < this.emotes.Count; i++)
			{
				int emote_index = -1;
				for (int j = 0; j < Farmer.EMOTES.Length; j++)
				{
					if (Farmer.EMOTES[j].emoteString == this.emotes[i])
					{
						emote_index = j;
						break;
					}
				}
				ClickableTextureComponent emote_button = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), this.menuBackgroundTexture, EmoteMenu.GetEmoteNonBubbleSpriteRect(emote_index), 4f, false);
				this._emoteButtons.Add(emote_button);
			}
			this._RepositionButtons();
		}

		// Token: 0x0600290A RID: 10506 RVA: 0x001E226E File Offset: 0x001E046E
		public static Rectangle GetEmoteSpriteRect(int emote_index)
		{
			if (emote_index <= 0)
			{
				return new Rectangle(48, 0, 16, 16);
			}
			return new Rectangle(emote_index % 4 * 16 + 48, emote_index / 4 * 16, 16, 16);
		}

		// Token: 0x0600290B RID: 10507 RVA: 0x001E2299 File Offset: 0x001E0499
		public static Rectangle GetEmoteNonBubbleSpriteRect(int emote_index)
		{
			return new Rectangle(emote_index % 4 * 16, emote_index / 4 * 16, 16, 16);
		}

		// Token: 0x0600290C RID: 10508 RVA: 0x001E22B0 File Offset: 0x001E04B0
		public override void applyMovementKey(int direction)
		{
		}

		// Token: 0x0600290D RID: 10509 RVA: 0x001E22B2 File Offset: 0x001E04B2
		protected override void cleanupBeforeExit()
		{
			Game1.emoteMenu = null;
			Game1.oldMouseState = Game1.input.GetMouseState();
			base.cleanupBeforeExit();
		}

		// Token: 0x0600290E RID: 10510 RVA: 0x001E22D0 File Offset: 0x001E04D0
		public override void performHoverAction(int x, int y)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale((float)x);
			y = (int)Utility.ModifyCoordinateFromUIScale((float)y);
			if (!this.gamepadMode)
			{
				for (int i = 0; i < this._emoteButtons.Count; i++)
				{
					if (this._emoteButtons[i].containsPoint(x, y))
					{
						this._selectedEmote = this.emotes[i];
						this._selectedIndex = i;
						if (this._selectedIndex != this._oldSelection)
						{
							this._selectedTime = 0;
						}
						return;
					}
				}
				this._selectedEmote = null;
				this._selectedIndex = -1;
			}
		}

		// Token: 0x0600290F RID: 10511 RVA: 0x001E2364 File Offset: 0x001E0564
		protected void _RepositionButtons()
		{
			for (int i = 0; i < this._emoteButtons.Count; i++)
			{
				ClickableTextureComponent emote_button = this._emoteButtons[i];
				float radians = Utility.Lerp(0f, 6.2831855f, (float)i / (float)this._emoteButtons.Count);
				emote_button.bounds.X = (int)((float)(this.xPositionOnScreen + this.width / 2 + (int)(Math.Cos((double)radians) * (double)this._buttonRadius) * 4) - (float)emote_button.bounds.Width / 2f);
				emote_button.bounds.Y = (int)((float)(this.yPositionOnScreen + this.height / 2 + (int)(-Math.Sin((double)radians) * (double)this._buttonRadius) * 4) - (float)emote_button.bounds.Height / 2f);
			}
		}

		// Token: 0x06002910 RID: 10512 RVA: 0x001E2440 File Offset: 0x001E0640
		protected void _SnapToPlayerPosition()
		{
			if (Game1.player == null)
			{
				return;
			}
			Vector2 player_position = Game1.player.getLocalPosition(Game1.viewport) + new Vector2((float)(-(float)this.width) / 2f, (float)(-(float)this.height) / 2f);
			this.xPositionOnScreen = (int)player_position.X + 32;
			this.yPositionOnScreen = (int)player_position.Y - 64;
			if (this.xPositionOnScreen + this.width > Game1.viewport.Width)
			{
				this.xPositionOnScreen -= this.xPositionOnScreen + this.width - Game1.viewport.Width;
			}
			if (this.xPositionOnScreen < 0)
			{
				this.xPositionOnScreen -= this.xPositionOnScreen;
			}
			if (this.yPositionOnScreen + this.height > Game1.viewport.Height)
			{
				this.yPositionOnScreen -= this.yPositionOnScreen + this.height - Game1.viewport.Height;
			}
			if (this.yPositionOnScreen < 0)
			{
				this.yPositionOnScreen -= this.yPositionOnScreen;
			}
			this._RepositionButtons();
		}

		// Token: 0x06002911 RID: 10513 RVA: 0x001E2568 File Offset: 0x001E0768
		public override void update(GameTime time)
		{
			this._age += time.ElapsedGameTime.Milliseconds;
			if (this._age > this._expandTime)
			{
				this._age = this._expandTime;
			}
			if (!this.gamepadMode && Game1.options.gamepadControls && (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f))
			{
				this.gamepadMode = true;
			}
			this._alpha = (float)this._age / (float)this._expandTime;
			this._buttonRadius = (int)((float)this._age / (float)this._expandTime * (float)this._expandedButtonRadius);
			this._SnapToPlayerPosition();
			Vector2 offset = default(Vector2);
			if (this.gamepadMode)
			{
				this._mouseStartPosition = Game1.getMousePosition(false);
				if (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f)
				{
					this._hasSelectedEmote = true;
					offset = new Vector2(Game1.input.GetGamePadState().ThumbSticks.Right.X, Game1.input.GetGamePadState().ThumbSticks.Right.Y);
					offset.Y *= -1f;
					offset.Normalize();
					float highest_dot = -1f;
					for (int i = 0; i < this._emoteButtons.Count; i++)
					{
						Vector2 button_offset = new Vector2((float)this._emoteButtons[i].bounds.Center.X - ((float)this.xPositionOnScreen + (float)this.width / 2f), (float)this._emoteButtons[i].bounds.Center.Y - ((float)this.yPositionOnScreen + (float)this.height / 2f));
						float dot = Vector2.Dot(offset, button_offset);
						if (dot > highest_dot)
						{
							highest_dot = dot;
							this._selectedEmote = this.emotes[i];
							this._selectedIndex = i;
						}
					}
					this._menuCloseGracePeriod = 100;
					if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) && this._selectedIndex >= 0)
					{
						Game1.activeClickableMenu = new EmoteSelector(this._selectedIndex, this.emotes[this._selectedIndex]);
						base.exitThisMenuNoSound();
						return;
					}
				}
				else
				{
					if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && this._menuCloseGracePeriod < 100)
					{
						this._menuCloseGracePeriod = 100;
					}
					if (this._menuCloseGracePeriod >= 0)
					{
						this._menuCloseGracePeriod -= time.ElapsedGameTime.Milliseconds;
					}
					if (this._menuCloseGracePeriod <= 0 && !Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
					{
						this.ConfirmSelection();
					}
				}
			}
			for (int j = 0; j < this._emoteButtons.Count; j++)
			{
				if (this._emoteButtons[j].scale > 4f)
				{
					this._emoteButtons[j].scale = Utility.MoveTowards(this._emoteButtons[j].scale, 4f, (float)time.ElapsedGameTime.Milliseconds / 1000f * 10f);
				}
			}
			if (this._selectedEmote != null && this._selectedIndex > -1)
			{
				this._emoteButtons[this._selectedIndex].scale = 5f;
			}
			if (this._oldSelection != this._selectedIndex)
			{
				this._oldSelection = this._selectedIndex;
				this._selectedTime = 0;
			}
			this._selectedTime += time.ElapsedGameTime.Milliseconds;
			base.update(time);
		}

		// Token: 0x06002912 RID: 10514 RVA: 0x001E29AC File Offset: 0x001E0BAC
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale((float)x);
			y = (int)Utility.ModifyCoordinateFromUIScale((float)y);
			for (int i = 0; i < this._emoteButtons.Count; i++)
			{
				if (this._emoteButtons[i].containsPoint(x, y) && Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new EmoteSelector(i, this.emotes[i]);
					base.exitThisMenuNoSound();
					return;
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002913 RID: 10515 RVA: 0x001E2A26 File Offset: 0x001E0C26
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale((float)x);
			y = (int)Utility.ModifyCoordinateFromUIScale((float)y);
			this.ConfirmSelection();
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002914 RID: 10516 RVA: 0x001E2A4B File Offset: 0x001E0C4B
		public void ConfirmSelection()
		{
			if (this._selectedEmote != null)
			{
				Game1.chatBox.textBoxEnter("/emote " + this._selectedEmote);
			}
			base.exitThisMenu(false);
		}

		// Token: 0x06002915 RID: 10517 RVA: 0x001E2A78 File Offset: 0x001E0C78
		public override void draw(SpriteBatch b)
		{
			Game1.StartWorldDrawInUI(b);
			Color background_color = Color.White;
			background_color.A = (byte)Utility.Lerp(0f, 255f, this._alpha);
			foreach (ClickableTextureComponent clickableTextureComponent in this._emoteButtons)
			{
				clickableTextureComponent.draw(b, background_color, 0.86f, 0, 0, 0);
			}
			if (this._selectedEmote != null)
			{
				foreach (Farmer.EmoteType emote_type in Farmer.EMOTES)
				{
					if (emote_type.emoteString == this._selectedEmote)
					{
						SpriteText.drawStringWithScrollCenteredAt(b, emote_type.displayName, this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height, "", 1f, null, 0, 0.88f, false);
						break;
					}
				}
			}
			if (this._selectedIndex >= 0 && this._selectedTime >= 250)
			{
				Vector2 draw_position = Utility.PointToVector2(this._emoteButtons[this._selectedIndex].bounds.Center);
				draw_position.X += 16f;
				if (!this.gamepadMode)
				{
					draw_position = Utility.PointToVector2(Game1.getMousePosition(false)) + new Vector2(32f, 32f);
					b.Draw(this.menuBackgroundTexture, draw_position, new Rectangle?(new Rectangle(64, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
				}
				else
				{
					b.Draw(Game1.controllerMaps, draw_position, new Rectangle?(Utility.controllerMapSourceRect(new Rectangle(625, 260, 28, 28))), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				}
				draw_position.X += 32f;
				b.Draw(this.menuBackgroundTexture, draw_position, new Rectangle?(new Rectangle(64, 16, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
			}
			Game1.EndWorldDrawInUI(b);
		}

		// Token: 0x04001ACD RID: 6861
		public Texture2D menuBackgroundTexture;

		// Token: 0x04001ACE RID: 6862
		public List<string> emotes;

		// Token: 0x04001ACF RID: 6863
		protected Point _mouseStartPosition;

		// Token: 0x04001AD0 RID: 6864
		public bool _hasSelectedEmote;

		// Token: 0x04001AD1 RID: 6865
		protected List<ClickableTextureComponent> _emoteButtons;

		// Token: 0x04001AD2 RID: 6866
		protected string _selectedEmote;

		// Token: 0x04001AD3 RID: 6867
		protected int _selectedIndex = -1;

		// Token: 0x04001AD4 RID: 6868
		protected int _oldSelection;

		// Token: 0x04001AD5 RID: 6869
		protected int _selectedTime;

		// Token: 0x04001AD6 RID: 6870
		protected float _alpha;

		// Token: 0x04001AD7 RID: 6871
		protected int _menuCloseGracePeriod = -1;

		// Token: 0x04001AD8 RID: 6872
		protected int _age;

		// Token: 0x04001AD9 RID: 6873
		public bool gamepadMode;

		// Token: 0x04001ADA RID: 6874
		protected int _expandTime = 200;

		// Token: 0x04001ADB RID: 6875
		protected int _expandedButtonRadius = 24;

		// Token: 0x04001ADC RID: 6876
		protected int _buttonRadius;
	}
}

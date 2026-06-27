using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	// Token: 0x020002A3 RID: 675
	public class RenovateMenu : IClickableMenu
	{
		// Token: 0x06002C11 RID: 11281 RVA: 0x00219B54 File Offset: 0x00217D54
		public RenovateMenu(HouseRenovation renovation) : base(Game1.uiViewport.Width / 2 - RenovateMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - RenovateMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4, RenovateMenu.menuWidth + IClickableMenu.borderWidth * 2, RenovateMenu.menuHeight + IClickableMenu.borderWidth, false)
		{
			this.height += 64;
			this.okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 101,
				upNeighborID = 103,
				leftNeighborID = 103
			};
			this._renovation = renovation;
			RenovateMenu.menuHeight = 320;
			RenovateMenu.menuWidth = 448;
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			this.SetupForRenovationPlacement();
		}

		// Token: 0x06002C12 RID: 11282 RVA: 0x00219C76 File Offset: 0x00217E76
		public override bool shouldClampGamePadCursor()
		{
			return true;
		}

		// Token: 0x06002C13 RID: 11283 RVA: 0x00219C79 File Offset: 0x00217E79
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002C14 RID: 11284 RVA: 0x00219C90 File Offset: 0x00217E90
		public void SetupForReturn()
		{
			this.freeze = true;
			LocationRequest locationRequest = Game1.getLocationRequest(this._oldLocation, false);
			locationRequest.OnWarp += delegate()
			{
				Game1.player.viewingLocation.Value = null;
				Game1.displayHUD = true;
				Game1.displayFarmer = true;
				this.freeze = false;
				Game1.viewportFreeze = false;
				this.FinalizeReturn();
			};
			Game1.warpFarmer(locationRequest, this._oldPosition.X, this._oldPosition.Y, Game1.player.FacingDirection);
		}

		// Token: 0x06002C15 RID: 11285 RVA: 0x00219CE7 File Offset: 0x00217EE7
		public void FinalizeReturn()
		{
			base.exitThisMenu(false);
			Game1.player.forceCanMove();
			this.freeze = false;
		}

		// Token: 0x06002C16 RID: 11286 RVA: 0x00219D04 File Offset: 0x00217F04
		public void SetupForRenovationPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.displayFarmer = false;
			this._oldLocation = Game1.currentLocation.NameOrUniqueName;
			this._oldPosition = Game1.player.TilePoint;
			Game1.currentLocation = this._renovation.location;
			Game1.player.viewingLocation.Value = this._renovation.location.NameOrUniqueName;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear(null, 0.02f);
			this.freeze = false;
			this.okButton.bounds.X = Game1.uiViewport.Width - 128;
			this.okButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Vector2 center = default(Vector2);
			int count = 0;
			foreach (List<Microsoft.Xna.Framework.Rectangle> list in this._renovation.renovationBounds)
			{
				foreach (Microsoft.Xna.Framework.Rectangle rectangle in list)
				{
					center.X += (float)rectangle.Center.X;
					center.Y += (float)rectangle.Center.Y;
					count++;
				}
			}
			if (count > 0)
			{
				center.X = (float)((int)Math.Round((double)(center.X / (float)count)));
				center.Y = (float)((int)Math.Round((double)(center.Y / (float)count)));
			}
			Game1.viewport.Location = new Location((int)((center.X + 0.5f) * 64f) - Game1.viewport.Width / 2, (int)((center.Y + 0.5f) * 64f) - Game1.viewport.Height / 2);
			Game1.panScreen(0, 0);
		}

		// Token: 0x06002C17 RID: 11287 RVA: 0x00219F1C File Offset: 0x0021811C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade || this.freeze)
			{
				return;
			}
			if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
			{
				this.SetupForReturn();
				Game1.playSound("smallSelect", null);
				return;
			}
			Vector2 clickTile = new Vector2((Utility.ModifyCoordinateFromUIScale((float)x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale((float)y) + (float)Game1.viewport.Y) / 64f);
			for (int i = 0; i < this._renovation.renovationBounds.Count; i++)
			{
				foreach (Microsoft.Xna.Framework.Rectangle rectangle in this._renovation.renovationBounds[i])
				{
					if (rectangle.Contains((int)clickTile.X, (int)clickTile.Y))
					{
						this.CompleteRenovation(i);
						return;
					}
				}
			}
		}

		// Token: 0x06002C18 RID: 11288 RVA: 0x0021A034 File Offset: 0x00218234
		public virtual void AnimateRenovation()
		{
			if (this._buildAnimationTimer != 0)
			{
				this._buildAnimationTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this._buildAnimationTimer <= 0)
				{
					if (this._buildAnimationCount > 0)
					{
						this._buildAnimationCount--;
						if (this._renovation.animationType == HouseRenovation.AnimationType.Destroy)
						{
							this._buildAnimationTimer = 50;
							for (int i = 0; i < 5; i++)
							{
								Microsoft.Xna.Framework.Rectangle rectangle = Game1.random.ChooseFrom(this._renovation.renovationBounds[this._animatingIndex]);
								int x = (int)Utility.RandomFloat((float)((rectangle.Left - 1) * 64), (float)(64 * rectangle.Right), null);
								int y = (int)Utility.RandomFloat((float)((rectangle.Top - 1) * 64), (float)(64 * rectangle.Bottom), null);
								this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2((float)x, (float)y), false, Game1.random.NextBool()));
								this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2((float)x, (float)y), false, Game1.random.NextBool()));
								this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2((float)x, (float)y), false, 0f, Color.White)
								{
									interval = 30f,
									totalNumberOfLoops = 99999,
									animationLength = 4,
									scale = 4f,
									alphaFade = 0.01f
								});
							}
							return;
						}
						this._buildAnimationTimer = 500;
						Game1.playSound("axe", null);
						for (int j = 0; j < 20; j++)
						{
							Microsoft.Xna.Framework.Rectangle rectangle2 = Game1.random.ChooseFrom(this._renovation.renovationBounds[this._animatingIndex]);
							int x2 = (int)Utility.RandomFloat((float)((rectangle2.Left - 1) * 64), (float)(64 * rectangle2.Right), null);
							int y2 = (int)Utility.RandomFloat((float)((rectangle2.Top - 1) * 64), (float)(64 * rectangle2.Bottom), null);
							this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)(Game1.random.Next(30, 90) - 64), 6, 1, new Vector2((float)x2, (float)y2), false, Game1.random.NextBool()));
							this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)(Game1.random.Next(30, 90) - 64), 6, 1, new Vector2((float)x2, (float)y2), false, Game1.random.NextBool()));
						}
						return;
					}
					else
					{
						this._buildAnimationTimer = 0;
						this.SetupForReturn();
					}
				}
			}
		}

		// Token: 0x06002C19 RID: 11289 RVA: 0x0021A360 File Offset: 0x00218560
		public virtual void CompleteRenovation(int selected_index)
		{
			if (this._renovation.validate == null || this._renovation.validate(this._renovation, selected_index))
			{
				if (Game1.player.Money < this._renovation.Price)
				{
					Game1.playSound("cancel", null);
					return;
				}
				bool isRefund = this._renovation.Price < 0;
				if (!isRefund || Game1.player.mailReceived.Contains("FirstPurchase_" + this._renovation.RoomId))
				{
					if (isRefund)
					{
						Game1.player._money -= this._renovation.Price;
					}
					else
					{
						Game1.player.Money -= this._renovation.Price;
						Game1.player.mailReceived.Add("FirstPurchase_" + this._renovation.RoomId);
					}
				}
				this.freeze = true;
				if (this._renovation.animationType == HouseRenovation.AnimationType.Destroy)
				{
					Game1.playSound("explosion", null);
					this._buildAnimationCount = 10;
				}
				else
				{
					this._buildAnimationCount = 3;
				}
				this._buildAnimationTimer = -1;
				this._animatingIndex = this._selectedIndex;
				if (this._renovation.onRenovation != null)
				{
					this._renovation.onRenovation(this._renovation, selected_index);
					Game1.player.renovateEvent.Fire(this._renovation.location.NameOrUniqueName);
				}
				this.AnimateRenovation();
			}
		}

		// Token: 0x06002C1A RID: 11290 RVA: 0x0021A4F8 File Offset: 0x002186F8
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return true;
		}

		// Token: 0x06002C1B RID: 11291 RVA: 0x0021A4FC File Offset: 0x002186FC
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button == Buttons.B && !Game1.globalFade)
			{
				this.SetupForReturn();
				Game1.playSound("smallSelect", null);
			}
		}

		// Token: 0x06002C1C RID: 11292 RVA: 0x0021A539 File Offset: 0x00218739
		public override bool readyToClose()
		{
			return !this.freeze && base.readyToClose();
		}

		// Token: 0x06002C1D RID: 11293 RVA: 0x0021A54C File Offset: 0x0021874C
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade || this.freeze)
			{
				return;
			}
			if (!Game1.globalFade)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
				{
					this.SetupForReturn();
					return;
				}
				if (!Game1.options.SnappyMenus && !this.freeze)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
						return;
					}
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.globalFade)
			{
				if (this.readyToClose())
				{
					Game1.player.forceCanMove();
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect", null);
					return;
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x06002C1E RID: 11294 RVA: 0x0021A690 File Offset: 0x00218890
		public override void update(GameTime time)
		{
			base.update(time);
			this.AnimateRenovation();
			int mouseX = Game1.getOldMouseX(false) + Game1.viewport.X;
			int mouseY = Game1.getOldMouseY(false) + Game1.viewport.Y;
			if (!this.freeze)
			{
				if (mouseX - Game1.viewport.X < 64)
				{
					Game1.panScreen(-8, 0);
				}
				else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
				{
					Game1.panScreen(8, 0);
				}
				if (mouseY - Game1.viewport.Y < 64)
				{
					Game1.panScreen(0, -8);
				}
				else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
				{
					Game1.panScreen(0, 8);
				}
			}
			foreach (Keys key in Game1.oldKBState.GetPressedKeys())
			{
				this.receiveKeyPress(key);
			}
		}

		// Token: 0x06002C1F RID: 11295 RVA: 0x0021A774 File Offset: 0x00218974
		public override void performHoverAction(int x, int y)
		{
			this.hovered = null;
			if (Game1.globalFade || this.freeze)
			{
				return;
			}
			if (this.okButton != null)
			{
				if (this.okButton.containsPoint(x, y))
				{
					this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
				}
				else
				{
					this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
				}
			}
			Vector2 clickTile = new Vector2((Utility.ModifyCoordinateFromUIScale((float)x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale((float)y) + (float)Game1.viewport.Y) / 64f);
			this._selectedIndex = -1;
			for (int i = 0; i < this._renovation.renovationBounds.Count; i++)
			{
				foreach (Microsoft.Xna.Framework.Rectangle rectangle in this._renovation.renovationBounds[i])
				{
					if (rectangle.Contains((int)clickTile.X, (int)clickTile.Y))
					{
						this._selectedIndex = i;
						break;
					}
				}
			}
		}

		// Token: 0x06002C20 RID: 11296 RVA: 0x0021A8C4 File Offset: 0x00218AC4
		public override void draw(SpriteBatch b)
		{
			if (!Game1.globalFade && !this.freeze)
			{
				Game1.StartWorldDrawInUI(b);
				for (int i = 0; i < this._renovation.renovationBounds.Count; i++)
				{
					foreach (Microsoft.Xna.Framework.Rectangle rectangle in this._renovation.renovationBounds[i])
					{
						for (int x = rectangle.Left; x < rectangle.Right; x++)
						{
							for (int y = rectangle.Top; y < rectangle.Bottom; y++)
							{
								int index = 0;
								if (i == this._selectedIndex)
								{
									index = 1;
								}
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + index * 16, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
							}
						}
					}
				}
				Game1.EndWorldDrawInUI(b);
			}
			if (!Game1.globalFade && !this.freeze)
			{
				string s = this._renovation.placementText;
				SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s, 999999) / 2, 16, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
			}
			if (!Game1.globalFade && !this.freeze && this.okButton != null)
			{
				this.okButton.draw(b);
			}
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001DC0 RID: 7616
		public const int region_okButton = 101;

		// Token: 0x04001DC1 RID: 7617
		public const int region_randomButton = 103;

		// Token: 0x04001DC2 RID: 7618
		public static int menuHeight = 320;

		// Token: 0x04001DC3 RID: 7619
		public static int menuWidth = 448;

		// Token: 0x04001DC4 RID: 7620
		public ClickableTextureComponent okButton;

		// Token: 0x04001DC5 RID: 7621
		public ClickableTextureComponent hovered;

		// Token: 0x04001DC6 RID: 7622
		private bool freeze;

		// Token: 0x04001DC7 RID: 7623
		protected HouseRenovation _renovation;

		// Token: 0x04001DC8 RID: 7624
		protected string _oldLocation;

		// Token: 0x04001DC9 RID: 7625
		protected Point _oldPosition;

		// Token: 0x04001DCA RID: 7626
		protected int _selectedIndex = -1;

		// Token: 0x04001DCB RID: 7627
		protected int _animatingIndex = -1;

		// Token: 0x04001DCC RID: 7628
		protected int _buildAnimationTimer;

		// Token: 0x04001DCD RID: 7629
		protected int _buildAnimationCount;
	}
}

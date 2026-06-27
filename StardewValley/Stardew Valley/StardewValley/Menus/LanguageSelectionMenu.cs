using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;

namespace StardewValley.Menus
{
	// Token: 0x02000282 RID: 642
	public class LanguageSelectionMenu : IClickableMenu
	{
		// Token: 0x06002A74 RID: 10868 RVA: 0x001FD6AC File Offset: 0x001FB8AC
		public LanguageSelectionMenu()
		{
			Texture2D texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LanguageButtons");
			this.languages = new LanguageSelectionMenu.LanguageEntry[]
			{
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.en, null, texture, 0),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.ru, null, texture, 3),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.zh, null, texture, 4),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.de, null, texture, 6),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.pt, null, texture, 2),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.fr, null, texture, 7),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.es, null, texture, 1),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.ja, null, texture, 5),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.ko, null, texture, 8),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.it, null, texture, 10),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.tr, null, texture, 9),
				new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.hu, null, texture, 11)
			}.ToDictionary((LanguageSelectionMenu.LanguageEntry p) => p.LanguageCode.ToString());
			foreach (ModLanguage modLanguage in DataLoader.AdditionalLanguages(Game1.content))
			{
				Texture2D customTexture = Game1.temporaryContent.Load<Texture2D>(modLanguage.ButtonTexture);
				this.languages["ModLanguage_" + modLanguage.Id] = new LanguageSelectionMenu.LanguageEntry(LocalizedContentManager.LanguageCode.mod, modLanguage, customTexture, 0);
			}
			this._pageCount = (int)Math.Floor((double)((float)(this.languages.Count - 1) / 12f)) + 1;
			this.SetupButtons();
		}

		// Token: 0x06002A75 RID: 10869 RVA: 0x001FD848 File Offset: 0x001FBA48
		private void SetupButtons()
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)((float)LanguageSelectionMenu.width * 2.5f), LanguageSelectionMenu.height, 0, 0);
			this.languageButtons.Clear();
			int buttonWidth = LanguageSelectionMenu.width - 128;
			int buttonHeight = 83;
			int minIndex = 12 * this._currentPage;
			int maxIndex = minIndex + 11;
			int index = 0;
			int row = 0;
			int column = 0;
			foreach (KeyValuePair<string, LanguageSelectionMenu.LanguageEntry> pair in this.languages)
			{
				if (index < minIndex)
				{
					index++;
				}
				else
				{
					if (index > maxIndex)
					{
						break;
					}
					this.languageButtons.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64 + column * 6 * 64, (int)topLeft.Y + LanguageSelectionMenu.height - 30 - buttonHeight * (6 - row) - 16, buttonWidth, buttonHeight), pair.Key, null)
					{
						myID = index - minIndex,
						downNeighborID = -99998,
						leftNeighborID = -99998,
						rightNeighborID = -99998,
						upNeighborID = -99998
					});
					index++;
					column++;
					if (column > 2)
					{
						row++;
						column = 0;
					}
				}
			}
			this.previousPageButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + 4, (int)topLeft.Y + LanguageSelectionMenu.height / 2 - 25, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 554,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				visible = (this._currentPage > 0)
			};
			this.nextPageButton = new ClickableTextureComponent(new Rectangle((int)(topLeft.X + (float)LanguageSelectionMenu.width * 2.5f) - 32, (int)topLeft.Y + LanguageSelectionMenu.height / 2 - 25, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 555,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				visible = (this._currentPage < this._pageCount - 1)
			};
			if (Game1.options.SnappyMenus)
			{
				ClickableComponent currentlySnappedComponent = this.currentlySnappedComponent;
				int id = (currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 0;
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(id);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002A76 RID: 10870 RVA: 0x001FDB14 File Offset: 0x001FBD14
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A77 RID: 10871 RVA: 0x001FDB2C File Offset: 0x001FBD2C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (this.nextPageButton.visible && this.nextPageButton.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this._currentPage++;
				this.SetupButtons();
				return;
			}
			if (this.previousPageButton.visible && this.previousPageButton.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this._currentPage--;
				this.SetupButtons();
				return;
			}
			foreach (ClickableComponent component in this.languageButtons)
			{
				if (component.containsPoint(x, y))
				{
					Game1.playSound("select", null);
					LanguageSelectionMenu.LanguageEntry entry = this.languages.GetValueOrDefault(component.name);
					if (entry != null)
					{
						if (Game1.options.SnappyMenus)
						{
							Game1.activeClickableMenu.setCurrentlySnappedComponentTo(81118);
							Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
						}
						this.ApplyLanguage(entry);
						base.exitThisMenu(true);
						break;
					}
					Game1.log.Error("Received click on unknown language button '" + component.name + "'.", null);
				}
			}
		}

		// Token: 0x06002A78 RID: 10872 RVA: 0x001FDCA0 File Offset: 0x001FBEA0
		public virtual void ApplyLanguage(LanguageSelectionMenu.LanguageEntry entry)
		{
			if (entry.ModLanguage != null)
			{
				LocalizedContentManager.SetModLanguage(entry.ModLanguage);
				return;
			}
			LocalizedContentManager.CurrentLanguageCode = entry.LanguageCode;
		}

		// Token: 0x06002A79 RID: 10873 RVA: 0x001FDCC4 File Offset: 0x001FBEC4
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent component in this.languageButtons)
			{
				if (component.containsPoint(x, y))
				{
					if (component.label == null)
					{
						Game1.playSound("Cowboy_Footstep", null);
						component.label = "hovered";
					}
				}
				else
				{
					component.label = null;
				}
			}
			this.previousPageButton.tryHover(x, y, 0.1f);
			this.nextPageButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002A7A RID: 10874 RVA: 0x001FDD78 File Offset: 0x001FBF78
		public override void draw(SpriteBatch b)
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)((float)LanguageSelectionMenu.width * 2.5f), LanguageSelectionMenu.height, 0, 0);
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X + 32, (int)topLeft.Y + 156, (int)((float)LanguageSelectionMenu.width * 2.55f) - 64, LanguageSelectionMenu.height / 2 + 25, Color.White, 4f, true, -1f);
			foreach (ClickableComponent c in this.languageButtons)
			{
				LanguageSelectionMenu.LanguageEntry entry = this.languages.GetValueOrDefault(c.name);
				if (entry != null)
				{
					int buttonSourceY = (entry.SpriteIndex <= 6) ? (entry.SpriteIndex * 78) : ((entry.SpriteIndex - 7) * 78);
					buttonSourceY += ((c.label != null) ? 39 : 0);
					int buttonSourceX = (entry.SpriteIndex > 6) ? 174 : 0;
					b.Draw(entry.Texture, c.bounds, new Rectangle?(new Rectangle(buttonSourceX, buttonSourceY, 174, 40)), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
				}
			}
			this.previousPageButton.draw(b);
			this.nextPageButton.draw(b);
			if (Game1.activeClickableMenu == this)
			{
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x06002A7B RID: 10875 RVA: 0x001FDF54 File Offset: 0x001FC154
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.SetupButtons();
		}

		// Token: 0x04001C26 RID: 7206
		public new static int width = 500;

		// Token: 0x04001C27 RID: 7207
		public new static int height = 728;

		// Token: 0x04001C28 RID: 7208
		protected int _currentPage;

		// Token: 0x04001C29 RID: 7209
		protected int _pageCount;

		// Token: 0x04001C2A RID: 7210
		public readonly Dictionary<string, LanguageSelectionMenu.LanguageEntry> languages;

		// Token: 0x04001C2B RID: 7211
		public readonly List<ClickableComponent> languageButtons = new List<ClickableComponent>();

		// Token: 0x04001C2C RID: 7212
		public ClickableTextureComponent nextPageButton;

		// Token: 0x04001C2D RID: 7213
		public ClickableTextureComponent previousPageButton;

		// Token: 0x0200061B RID: 1563
		public class LanguageEntry
		{
			// Token: 0x06004438 RID: 17464 RVA: 0x0031BBC7 File Offset: 0x00319DC7
			public LanguageEntry(LocalizedContentManager.LanguageCode languageCode, ModLanguage modLanguage, Texture2D texture, int spriteIndex)
			{
				this.LanguageCode = languageCode;
				this.ModLanguage = modLanguage;
				this.Texture = texture;
				this.SpriteIndex = spriteIndex;
			}

			// Token: 0x04002E80 RID: 11904
			public readonly LocalizedContentManager.LanguageCode LanguageCode;

			// Token: 0x04002E81 RID: 11905
			public readonly ModLanguage ModLanguage;

			// Token: 0x04002E82 RID: 11906
			public readonly Texture2D Texture;

			// Token: 0x04002E83 RID: 11907
			public readonly int SpriteIndex;
		}
	}
}

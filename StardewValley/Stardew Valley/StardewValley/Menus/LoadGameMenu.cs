using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.SaveSerialization;

namespace StardewValley.Menus
{
	// Token: 0x02000285 RID: 645
	public class LoadGameMenu : IClickableMenu, IDisposable
	{
		// Token: 0x06002AB1 RID: 10929 RVA: 0x00202C20 File Offset: 0x00200E20
		public bool IsDoingTask()
		{
			return this._initTask != null || this._deleteTask != null || this.loading || this.deleting;
		}

		// Token: 0x06002AB2 RID: 10930 RVA: 0x00202C42 File Offset: 0x00200E42
		public override bool readyToClose()
		{
			return !this.IsDoingTask() && this._updatesSinceLastDeleteConfirmScreen > 1;
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06002AB3 RID: 10931 RVA: 0x00202C57 File Offset: 0x00200E57
		// (set) Token: 0x06002AB4 RID: 10932 RVA: 0x00202C5F File Offset: 0x00200E5F
		public virtual List<LoadGameMenu.MenuSlot> MenuSlots
		{
			get
			{
				return this.menuSlots;
			}
			set
			{
				this.menuSlots = value;
			}
		}

		// Token: 0x06002AB5 RID: 10933 RVA: 0x00202C68 File Offset: 0x00200E68
		public LoadGameMenu(string filter = null) : base(Game1.uiViewport.Width / 2 - (1100 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1100 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false)
		{
			this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
			{
				myID = 81114,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false)
			{
				myID = 800,
				downNeighborID = 801,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 902
			};
			this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false)
			{
				myID = 801,
				upNeighborID = 800,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				region = 902
			};
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 64 - this.upArrow.bounds.Height - 28);
			this.okDeleteButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10992"), new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).X - 64, (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).Y + 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 802,
				rightNeighborID = 803,
				region = 903
			};
			this.cancelDeleteButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993"), new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).X + 64, (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).Y + 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 803,
				leftNeighborID = 802,
				region = 903
			};
			for (int i = 0; i < 4; i++)
			{
				this.slotButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * (this.height / 4), this.width - 32, this.height / 4 + 4), i.ToString() ?? "")
				{
					myID = i,
					region = 900,
					downNeighborID = ((i < 3) ? -99998 : -7777),
					upNeighborID = ((i > 0) ? -99998 : -7777),
					rightNeighborID = -99998,
					fullyImmutable = true
				});
				if (this.hasDeleteButtons())
				{
					this.deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width - 64 - 4, this.yPositionOnScreen + 32 + 4 + i * (this.height / 4), 48, 48), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994"), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3f, false)
					{
						myID = i + 100,
						region = 901,
						leftNeighborID = -99998,
						downNeighborImmutable = true,
						downNeighborID = -99998,
						upNeighborImmutable = true,
						upNeighborID = ((i > 0) ? -99998 : -1),
						rightNeighborID = -99998
					});
				}
			}
			this.startListPopulation(filter);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			this.UpdateButtons();
		}

		// Token: 0x06002AB6 RID: 10934 RVA: 0x00203259 File Offset: 0x00201459
		protected virtual bool hasDeleteButtons()
		{
			return true;
		}

		// Token: 0x06002AB7 RID: 10935 RVA: 0x0020325C File Offset: 0x0020145C
		protected virtual void startListPopulation(string filter)
		{
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				this.addSaveFiles(LoadGameMenu.FindSaveGames(filter));
				this.saveFileScanComplete();
				return;
			}
			this._initTask = new Task<List<Farmer>>(delegate()
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				return LoadGameMenu.FindSaveGames(filter);
			});
			Game1.hooks.StartTask<List<Farmer>>(this._initTask, "Find Save Games");
		}

		// Token: 0x06002AB8 RID: 10936 RVA: 0x002032C4 File Offset: 0x002014C4
		public virtual void UpdateButtons()
		{
			for (int i = 0; i < this.slotButtons.Count; i++)
			{
				ClickableTextureComponent delete_button = null;
				if (this.hasDeleteButtons() && i >= 0 && i < this.deleteButtons.Count)
				{
					delete_button = this.deleteButtons[i];
				}
				if (this.currentItemIndex + i < this.MenuSlots.Count)
				{
					this.slotButtons[i].visible = true;
					if (delete_button != null)
					{
						delete_button.visible = true;
					}
				}
				else
				{
					this.slotButtons[i].visible = false;
					if (delete_button != null)
					{
						delete_button.visible = false;
					}
				}
			}
		}

		// Token: 0x06002AB9 RID: 10937 RVA: 0x00203364 File Offset: 0x00201564
		protected virtual void addSaveFiles(List<Farmer> files)
		{
			int startNumbersAt = this.MenuSlots.Count + 1;
			for (int i = 0; i < files.Count; i++)
			{
				Farmer file = files[i];
				if (file != null)
				{
					this.MenuSlots.Add(new LoadGameMenu.SaveFileSlot(this, file, new int?(startNumbersAt + i)));
				}
			}
			this.UpdateButtons();
		}

		// Token: 0x06002ABA RID: 10938 RVA: 0x002033BC File Offset: 0x002015BC
		private static List<Farmer> FindSaveGames(string filter)
		{
			List<Farmer> results = new List<Farmer>();
			string pathToDirectory = Program.GetSavesFolder();
			if (Directory.Exists(pathToDirectory))
			{
				foreach (string s in Directory.EnumerateDirectories(pathToDirectory).ToList<string>())
				{
					string saveName = s.Split(Path.DirectorySeparatorChar, StringSplitOptions.None).Last<string>();
					LoadGameMenu.<>c__DisplayClass45_0 CS$<>8__locals1;
					CS$<>8__locals1.pathToFile = Path.Combine(pathToDirectory, s, "SaveGameInfo");
					CS$<>8__locals1.pathToSave = Path.Combine(pathToDirectory, s, saveName);
					if (File.Exists(CS$<>8__locals1.pathToSave) || File.Exists(CS$<>8__locals1.pathToSave + "_old") || File.Exists(CS$<>8__locals1.pathToSave + "_STARDEWVALLEYSAVETMP"))
					{
						Farmer f = null;
						try
						{
							Exception error;
							Farmer farmer2;
							Exception ex;
							if ((farmer2 = LoadGameMenu.<FindSaveGames>g__TryReadSaveInfo|45_1(null, out error, ref CS$<>8__locals1)) == null && (farmer2 = (LoadGameMenu.<FindSaveGames>g__TryReadSaveInfo|45_1("_old", out ex, ref CS$<>8__locals1) ?? LoadGameMenu.<FindSaveGames>g__TryReadSaveInfo|45_1("_STARDEWVALLEYSAVETMP", out ex, ref CS$<>8__locals1))) == null && (farmer2 = LoadGameMenu.<FindSaveGames>g__TryReadSaveData|45_2(null, out ex, ref CS$<>8__locals1)) == null)
							{
								farmer2 = (LoadGameMenu.<FindSaveGames>g__TryReadSaveData|45_2("_old", out ex, ref CS$<>8__locals1) ?? LoadGameMenu.<FindSaveGames>g__TryReadSaveData|45_2("_STARDEWVALLEYSAVETMP", out ex, ref CS$<>8__locals1));
							}
							f = farmer2;
							if (f == null)
							{
								Game1.log.Error("Exception occurred trying to access file '" + CS$<>8__locals1.pathToFile + "'", error);
							}
							else
							{
								SaveGame.loadDataToFarmer(f);
								f.slotName = saveName;
								results.Add(f);
							}
						}
						catch (Exception e)
						{
							Game1.log.Error("Exception occurred trying to access file '" + CS$<>8__locals1.pathToFile + "'", e);
							if (f != null)
							{
								f.unload();
							}
						}
					}
				}
			}
			results.Sort();
			if (!string.IsNullOrWhiteSpace(filter))
			{
				for (int i = 0; i < results.Count; i++)
				{
					Farmer farmer = results[i];
					string name = farmer.Name;
					if (name != null && name.IndexOfIgnoreCase(filter) == -1)
					{
						string value = farmer.farmName.Value;
						if (value != null && value.IndexOfIgnoreCase(filter) == -1)
						{
							results[i] = null;
						}
					}
				}
			}
			return results;
		}

		// Token: 0x06002ABB RID: 10939 RVA: 0x0020360C File Offset: 0x0020180C
		public override void receiveGamePadButton(Buttons button)
		{
			if (button == Buttons.B && this.deleteConfirmationScreen)
			{
				this.deleteConfirmationScreen = false;
				this.selectedForDelete = -1;
				Game1.playSound("smallSelect", null);
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					this.currentlySnappedComponent = base.getComponentWithID(0);
					this.snapCursorToCurrentSnappedComponent();
				}
			}
		}

		// Token: 0x06002ABC RID: 10940 RVA: 0x00203676 File Offset: 0x00201876
		public override void snapToDefaultClickableComponent()
		{
			if (this.deleteConfirmationScreen)
			{
				this.currentlySnappedComponent = base.getComponentWithID(803);
			}
			else
			{
				this.currentlySnappedComponent = base.getComponentWithID(0);
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002ABD RID: 10941 RVA: 0x002036A8 File Offset: 0x002018A8
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (direction != 0)
			{
				if (direction == 2 && this.currentItemIndex < Math.Max(0, this.MenuSlots.Count - 4))
				{
					this.downArrowPressed();
					this.currentlySnappedComponent = base.getComponentWithID(3);
					this.snapCursorToCurrentSnappedComponent();
					return;
				}
			}
			else if (this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				this.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002ABE RID: 10942 RVA: 0x00203714 File Offset: 0x00201914
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.xPositionOnScreen = (newBounds.Width - this.width) / 2;
			this.yPositionOnScreen = (newBounds.Height - (this.height + 32)) / 2;
			this.backButton.bounds.X = Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2;
			this.backButton.bounds.Y = Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom;
			this.upArrow.bounds.X = this.xPositionOnScreen + this.width + 16;
			this.upArrow.bounds.Y = this.yPositionOnScreen + 16;
			this.downArrow.bounds.X = this.xPositionOnScreen + this.width + 16;
			this.downArrow.bounds.Y = this.yPositionOnScreen + this.height - 64;
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 64 - this.upArrow.bounds.Height - 28);
			this.okDeleteButton.bounds.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).X - 64;
			this.okDeleteButton.bounds.Y = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).Y + 128;
			this.cancelDeleteButton.bounds.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).X + 64;
			this.cancelDeleteButton.bounds.Y = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64, 0, 0).Y + 128;
			for (int i = 0; i < this.slotButtons.Count; i++)
			{
				this.slotButtons[i].bounds.X = this.xPositionOnScreen + 16;
				this.slotButtons[i].bounds.Y = this.yPositionOnScreen + 16 + i * (this.height / 4);
			}
			for (int j = 0; j < this.deleteButtons.Count; j++)
			{
				this.deleteButtons[j].bounds.X = this.xPositionOnScreen + this.width - 64 - 4;
				this.deleteButtons[j].bounds.Y = this.yPositionOnScreen + 32 + 4 + j * (this.height / 4);
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				int id = (this.currentlySnappedComponent != null) ? this.currentlySnappedComponent.myID : 81114;
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(id);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002ABF RID: 10943 RVA: 0x00203A9C File Offset: 0x00201C9C
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			base.performHoverAction(x, y);
			if (!this.deleteConfirmationScreen)
			{
				this.upArrow.tryHover(x, y, 0.1f);
				this.downArrow.tryHover(x, y, 0.1f);
				this.scrollBar.tryHover(x, y, 0.1f);
				foreach (ClickableTextureComponent clickableTextureComponent in this.deleteButtons)
				{
					clickableTextureComponent.tryHover(x, y, 0.2f);
					if (clickableTextureComponent.containsPoint(x, y))
					{
						this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994");
						return;
					}
				}
				if (this.scrolling)
				{
					return;
				}
				for (int i = 0; i < this.slotButtons.Count; i++)
				{
					if (this.currentItemIndex + i < this.MenuSlots.Count && this.slotButtons[i].containsPoint(x, y))
					{
						if (this.slotButtons[i].scale == 1f)
						{
							Game1.playSound("Cowboy_gunshot", null);
						}
						this.slotButtons[i].scale = Math.Min(this.slotButtons[i].scale + 0.03f, 1.1f);
					}
					else
					{
						this.slotButtons[i].scale = Math.Max(1f, this.slotButtons[i].scale - 0.03f);
					}
				}
				return;
			}
			this.okDeleteButton.tryHover(x, y, 0.1f);
			this.cancelDeleteButton.tryHover(x, y, 0.1f);
			if (this.okDeleteButton.containsPoint(x, y))
			{
				this.hoverText = "";
				return;
			}
			if (this.cancelDeleteButton.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993");
			}
		}

		// Token: 0x06002AC0 RID: 10944 RVA: 0x00203CB0 File Offset: 0x00201EB0
		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				int y2 = this.scrollBar.bounds.Y;
				this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + 20));
				float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
				this.currentItemIndex = Math.Min(this.MenuSlots.Count - 4, Math.Max(0, (int)((float)this.MenuSlots.Count * percentage)));
				this.setScrollBarToCurrentIndex();
				if (y2 != this.scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4", null);
				}
			}
		}

		// Token: 0x06002AC1 RID: 10945 RVA: 0x00203DAE File Offset: 0x00201FAE
		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}

		// Token: 0x06002AC2 RID: 10946 RVA: 0x00203DC0 File Offset: 0x00201FC0
		protected void setScrollBarToCurrentIndex()
		{
			if (this.MenuSlots.Count > 0)
			{
				this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.MenuSlots.Count - 4 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + 4;
				if (this.currentItemIndex == this.MenuSlots.Count - 4)
				{
					this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
				}
			}
			this.UpdateButtons();
		}

		// Token: 0x06002AC3 RID: 10947 RVA: 0x00203E78 File Offset: 0x00202078
		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				return;
			}
			if (direction < 0 && this.currentItemIndex < Math.Max(0, this.MenuSlots.Count - 4))
			{
				this.downArrowPressed();
			}
		}

		// Token: 0x06002AC4 RID: 10948 RVA: 0x00203EC8 File Offset: 0x002020C8
		private void downArrowPressed()
		{
			this.downArrow.scale = this.downArrow.baseScale;
			this.currentItemIndex++;
			Game1.playSound("shwip", null);
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002AC5 RID: 10949 RVA: 0x00203F14 File Offset: 0x00202114
		private void upArrowPressed()
		{
			this.upArrow.scale = this.upArrow.baseScale;
			this.currentItemIndex--;
			Game1.playSound("shwip", null);
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002AC6 RID: 10950 RVA: 0x00203F60 File Offset: 0x00202160
		private void deleteFile(int which)
		{
			LoadGameMenu.SaveFileSlot slot = this.MenuSlots[which] as LoadGameMenu.SaveFileSlot;
			if (slot == null)
			{
				return;
			}
			string filenameNoTmpString = slot.Farmer.slotName;
			string saveFolderPath = Path.Combine(Program.GetSavesFolder(), filenameNoTmpString);
			if (Directory.Exists(saveFolderPath))
			{
				Directory.Delete(saveFolderPath, true);
			}
			int i = 0;
			while (i < 50 && Directory.Exists(saveFolderPath))
			{
				Thread.Sleep(100);
				i++;
			}
		}

		// Token: 0x06002AC7 RID: 10951 RVA: 0x00203FC8 File Offset: 0x002021C8
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.timerToLoad > 0 || this.loading || this.deleting)
			{
				return;
			}
			if (this.deleteConfirmationScreen)
			{
				if (this.cancelDeleteButton.containsPoint(x, y))
				{
					this.deleteConfirmationScreen = false;
					this.selectedForDelete = -1;
					Game1.playSound("smallSelect", null);
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						this.currentlySnappedComponent = base.getComponentWithID(0);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
				else if (this.okDeleteButton.containsPoint(x, y))
				{
					this.deleting = true;
					if (LocalMultiplayer.IsLocalMultiplayer(false))
					{
						this.deleteFile(this.selectedForDelete);
						this.deleting = false;
					}
					else
					{
						this._deleteTask = new Task(delegate()
						{
							Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
							this.deleteFile(this.selectedForDelete);
						});
						Game1.hooks.StartTask(this._deleteTask, "Farm_Delete");
					}
					this.deleteConfirmationScreen = false;
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						this.currentlySnappedComponent = base.getComponentWithID(0);
						this.snapCursorToCurrentSnappedComponent();
					}
					Game1.playSound("trashcan", null);
				}
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.MenuSlots.Count - 4))
			{
				this.downArrowPressed();
			}
			else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
			}
			else if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
			}
			else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height)
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
			}
			if (this.selected == -1)
			{
				for (int i = 0; i < this.deleteButtons.Count; i++)
				{
					if (this.deleteButtons[i].containsPoint(x, y) && i < this.MenuSlots.Count && !this.deleteConfirmationScreen)
					{
						this.deleteConfirmationScreen = true;
						Game1.playSound("drumkit6", null);
						this.selectedForDelete = this.currentItemIndex + i;
						if (Game1.options.snappyMenus && Game1.options.gamepadControls)
						{
							this.currentlySnappedComponent = base.getComponentWithID(803);
							this.snapCursorToCurrentSnappedComponent();
						}
						return;
					}
				}
			}
			if (!this.deleteConfirmationScreen)
			{
				for (int j = 0; j < this.slotButtons.Count; j++)
				{
					if (this.slotButtons[j].containsPoint(x, y) && j < this.MenuSlots.Count)
					{
						LoadGameMenu.SaveFileSlot menu_save_slot = this.MenuSlots[this.currentItemIndex + j] as LoadGameMenu.SaveFileSlot;
						if (menu_save_slot != null && menu_save_slot.versionComparison < 0)
						{
							menu_save_slot.redTimer = Game1.currentGameTime.TotalGameTime.TotalSeconds + 1.0;
							Game1.playSound("cancel", null);
						}
						else
						{
							Game1.playSound("select", null);
							this.timerToLoad = this.MenuSlots[this.currentItemIndex + j].ActivateDelay;
							if (this.timerToLoad > 0)
							{
								this.loading = true;
								this.selected = this.currentItemIndex + j;
								return;
							}
							this.MenuSlots[this.currentItemIndex + j].Activate();
							return;
						}
					}
				}
			}
			this.currentItemIndex = Math.Max(0, Math.Min(this.MenuSlots.Count - 4, this.currentItemIndex));
		}

		// Token: 0x06002AC8 RID: 10952 RVA: 0x002043CA File Offset: 0x002025CA
		protected virtual void saveFileScanComplete()
		{
			Game1.game1.ResetGameStateOnTitleScreen();
		}

		// Token: 0x06002AC9 RID: 10953 RVA: 0x002043D8 File Offset: 0x002025D8
		protected virtual bool checkListPopulation()
		{
			if (!this.deleteConfirmationScreen)
			{
				this._updatesSinceLastDeleteConfirmScreen++;
			}
			else
			{
				this._updatesSinceLastDeleteConfirmScreen = 0;
			}
			if (this._initTask != null)
			{
				if (this._initTask.IsCanceled || this._initTask.IsCompleted || this._initTask.IsFaulted)
				{
					if (this._initTask.IsCompleted)
					{
						this.addSaveFiles(this._initTask.Result);
						this.saveFileScanComplete();
					}
					this._initTask = null;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002ACA RID: 10954 RVA: 0x00204464 File Offset: 0x00202664
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.checkListPopulation())
			{
				return;
			}
			if (this._deleteTask != null)
			{
				if (this._deleteTask.IsCanceled || this._deleteTask.IsCompleted || this._deleteTask.IsFaulted)
				{
					if (!this._deleteTask.IsCompleted)
					{
						this.selectedForDelete = -1;
					}
					this._deleteTask = null;
					this.deleting = false;
				}
				return;
			}
			if (this.selectedForDelete != -1 && !this.deleteConfirmationScreen && !this.deleting)
			{
				LoadGameMenu.SaveFileSlot slot = this.MenuSlots[this.selectedForDelete] as LoadGameMenu.SaveFileSlot;
				if (slot != null)
				{
					slot.Farmer.unload();
					this.MenuSlots.RemoveAt(this.selectedForDelete);
					this.selectedForDelete = -1;
					this.slotButtons.Clear();
					this.deleteButtons.Clear();
					for (int i = 0; i < 4; i++)
					{
						this.slotButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * (this.height / 4), this.width - 32, this.height / 4 + 4), i.ToString() ?? ""));
						if (this.hasDeleteButtons())
						{
							this.deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width - 64 - 4, this.yPositionOnScreen + 32 + 4 + i * (this.height / 4), 48, 48), "", "Delete File", Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3f, false));
						}
					}
					if (this.MenuSlots.Count <= 4)
					{
						this.currentItemIndex = 0;
						this.setScrollBarToCurrentIndex();
					}
				}
			}
			if (this.timerToLoad > 0)
			{
				this.timerToLoad -= time.ElapsedGameTime.Milliseconds;
				if (this.timerToLoad <= 0)
				{
					if (this.MenuSlots.Count > this.selected)
					{
						this.MenuSlots[this.selected].Activate();
						return;
					}
					Game1.ExitToTitle(null);
				}
			}
		}

		// Token: 0x06002ACB RID: 10955 RVA: 0x002046A0 File Offset: 0x002028A0
		protected virtual string getStatusText()
		{
			if (this._initTask != null)
			{
				return Game1.content.LoadString("Strings\\UI:LoadGameMenu_LookingForSavedGames");
			}
			if (this.deleting)
			{
				return Game1.content.LoadString("Strings\\UI:LoadGameMenu_Deleting");
			}
			if (this.MenuSlots.Count == 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11022");
			}
			return null;
		}

		// Token: 0x06002ACC RID: 10956 RVA: 0x002046FB File Offset: 0x002028FB
		protected virtual void drawExtra(SpriteBatch b)
		{
		}

		// Token: 0x06002ACD RID: 10957 RVA: 0x00204700 File Offset: 0x00202900
		protected virtual void drawSlotBackground(SpriteBatch b, int i, LoadGameMenu.MenuSlot slot)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.slotButtons[i].bounds.X, this.slotButtons[i].bounds.Y, this.slotButtons[i].bounds.Width, this.slotButtons[i].bounds.Height, ((this.currentItemIndex + i == this.selected && this.timerToLoad % 150 > 75 && this.timerToLoad > 1000) || (this.selected == -1 && this.slotButtons[i].scale > 1f && !this.scrolling && !this.deleteConfirmationScreen)) ? ((this.deleteButtons.Count > i && this.deleteButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) ? Color.White : Color.Wheat) : Color.White, 4f, false, -1f);
		}

		// Token: 0x06002ACE RID: 10958 RVA: 0x0020482A File Offset: 0x00202A2A
		protected virtual void drawBefore(SpriteBatch b)
		{
		}

		// Token: 0x06002ACF RID: 10959 RVA: 0x0020482C File Offset: 0x00202A2C
		protected virtual void drawStatusText(SpriteBatch b)
		{
			string text = this.getStatusText();
			if (text != null)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
			}
		}

		// Token: 0x06002AD0 RID: 10960 RVA: 0x002048B8 File Offset: 0x00202AB8
		public override void draw(SpriteBatch b)
		{
			this.drawBefore(b);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height + 32, Color.White, 4f, true, -1f);
			if (this.selectedForDelete == -1 || !this.deleting || this.deleteConfirmationScreen)
			{
				for (int i = 0; i < this.slotButtons.Count; i++)
				{
					if (this.currentItemIndex + i < this.MenuSlots.Count)
					{
						this.drawSlotBackground(b, i, this.MenuSlots[this.currentItemIndex + i]);
						this.MenuSlots[this.currentItemIndex + i].Draw(b, i);
						if (this.deleteButtons.Count > i)
						{
							this.deleteButtons[i].draw(b, Color.White * 0.75f, 1f, 0, 0, 0);
						}
					}
				}
			}
			this.drawStatusText(b);
			this.upArrow.draw(b);
			this.downArrow.draw(b);
			if (this.MenuSlots.Count > 4)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, false, -1f);
				this.scrollBar.draw(b);
			}
			if (this.deleteConfirmationScreen)
			{
				LoadGameMenu.SaveFileSlot slot = this.MenuSlots[this.selectedForDelete] as LoadGameMenu.SaveFileSlot;
				if (slot != null)
				{
					b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
					string toDisplay = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11023", slot.Farmer.Name);
					int middlePosX = this.okDeleteButton.bounds.X + (this.cancelDeleteButton.bounds.X - this.okDeleteButton.bounds.X) / 2 + this.okDeleteButton.bounds.Width / 2;
					SpriteText.drawString(b, toDisplay, middlePosX - SpriteText.getWidthOfString(toDisplay, 999999) / 2, (int)Utility.getTopLeftPositionForCenteringOnScreen(192, 64, 0, 0).Y, 9999, -1, 9999, 1f, 1f, false, -1, "", new Color?(SpriteText.color_White), SpriteText.ScrollTextAlignment.Left);
					this.okDeleteButton.draw(b);
					this.cancelDeleteButton.draw(b);
				}
			}
			base.draw(b);
			if (this.hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			this.drawExtra(b);
			if (this.selected != -1 && this.timerToLoad < 1000)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * (1f - (float)this.timerToLoad / 1000f));
			}
			if (Game1.activeClickableMenu == this && (!Game1.options.SnappyMenus || this.currentlySnappedComponent != null) && !this.IsDoingTask())
			{
				base.drawMouse(b, false, this.loading ? 1 : -1);
			}
			this.drawn = true;
		}

		// Token: 0x06002AD1 RID: 10961 RVA: 0x00204C88 File Offset: 0x00202E88
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					if (this.MenuSlots != null)
					{
						foreach (LoadGameMenu.MenuSlot menuSlot in this.MenuSlots)
						{
							menuSlot.Dispose();
						}
						this.MenuSlots.Clear();
						this.MenuSlots = null;
					}
					this._initTask = null;
					this._deleteTask = null;
				}
				this.disposedValue = true;
			}
		}

		// Token: 0x06002AD2 RID: 10962 RVA: 0x00204D14 File Offset: 0x00202F14
		~LoadGameMenu()
		{
			this.Dispose(false);
		}

		// Token: 0x06002AD3 RID: 10963 RVA: 0x00204D44 File Offset: 0x00202F44
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x06002AD4 RID: 10964 RVA: 0x00204D54 File Offset: 0x00202F54
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (a.region == 901 && b.region != 901 && direction == 2 && b.myID != 81114) || ((a.region != 901 || direction != 3 || b.region == 900) && (direction != 1 || a.region != 900 || !this.hasDeleteButtons() || b.region == 901) && (a.region == 903 || b.region != 903) && ((direction != 0 && direction != 2) || a.myID != 81114 || b.region != 902) && base.IsAutomaticSnapValid(direction, a, b));
		}

		// Token: 0x06002AD5 RID: 10965 RVA: 0x00204E1E File Offset: 0x0020301E
		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return false;
		}

		// Token: 0x06002AD6 RID: 10966 RVA: 0x00204E21 File Offset: 0x00203021
		[Conditional("LOG_FS_IO")]
		private static void LogFsio(string format, params object[] args)
		{
			Game1.log.Verbose(string.Format(format, args));
		}

		// Token: 0x06002AD7 RID: 10967 RVA: 0x00204E34 File Offset: 0x00203034
		[CompilerGenerated]
		internal static TData <FindSaveGames>g__TryReadFile|45_0<TData>(string path, out Exception loadError, Func<Stream, TData> load)
		{
			TData result;
			try
			{
				using (FileStream stream = File.OpenRead(path))
				{
					loadError = null;
					result = load(stream);
				}
			}
			catch (Exception ex)
			{
				loadError = ex;
				result = default(TData);
			}
			return result;
		}

		// Token: 0x06002AD8 RID: 10968 RVA: 0x00204E8C File Offset: 0x0020308C
		[CompilerGenerated]
		internal static Farmer <FindSaveGames>g__TryReadSaveInfo|45_1(string suffix, out Exception loadError, ref LoadGameMenu.<>c__DisplayClass45_0 A_2)
		{
			return LoadGameMenu.<FindSaveGames>g__TryReadFile|45_0<Farmer>(A_2.pathToFile + suffix, out loadError, new Func<Stream, Farmer>(SaveSerializer.Deserialize<Farmer>));
		}

		// Token: 0x06002AD9 RID: 10969 RVA: 0x00204EAC File Offset: 0x002030AC
		[CompilerGenerated]
		internal static Farmer <FindSaveGames>g__TryReadSaveData|45_2(string suffix, out Exception loadError, ref LoadGameMenu.<>c__DisplayClass45_0 A_2)
		{
			SaveGame saveGame = LoadGameMenu.<FindSaveGames>g__TryReadFile|45_0<SaveGame>(A_2.pathToSave + suffix, out loadError, new Func<Stream, SaveGame>(SaveSerializer.Deserialize<SaveGame>));
			if (saveGame == null)
			{
				return null;
			}
			return saveGame.player;
		}

		// Token: 0x04001C67 RID: 7271
		protected const int CenterOffset = 0;

		// Token: 0x04001C68 RID: 7272
		public const int region_upArrow = 800;

		// Token: 0x04001C69 RID: 7273
		public const int region_downArrow = 801;

		// Token: 0x04001C6A RID: 7274
		public const int region_okDelete = 802;

		// Token: 0x04001C6B RID: 7275
		public const int region_cancelDelete = 803;

		// Token: 0x04001C6C RID: 7276
		public const int region_slots = 900;

		// Token: 0x04001C6D RID: 7277
		public const int region_deleteButtons = 901;

		// Token: 0x04001C6E RID: 7278
		public const int region_navigationButtons = 902;

		// Token: 0x04001C6F RID: 7279
		public const int region_deleteConfirmations = 903;

		// Token: 0x04001C70 RID: 7280
		public const int itemsPerPage = 4;

		// Token: 0x04001C71 RID: 7281
		public List<ClickableComponent> slotButtons = new List<ClickableComponent>();

		// Token: 0x04001C72 RID: 7282
		public List<ClickableTextureComponent> deleteButtons = new List<ClickableTextureComponent>();

		// Token: 0x04001C73 RID: 7283
		public int currentItemIndex;

		// Token: 0x04001C74 RID: 7284
		public int timerToLoad;

		// Token: 0x04001C75 RID: 7285
		public int selected = -1;

		// Token: 0x04001C76 RID: 7286
		public int selectedForDelete = -1;

		// Token: 0x04001C77 RID: 7287
		public ClickableTextureComponent upArrow;

		// Token: 0x04001C78 RID: 7288
		public ClickableTextureComponent downArrow;

		// Token: 0x04001C79 RID: 7289
		public ClickableTextureComponent scrollBar;

		// Token: 0x04001C7A RID: 7290
		public ClickableTextureComponent okDeleteButton;

		// Token: 0x04001C7B RID: 7291
		public ClickableTextureComponent cancelDeleteButton;

		// Token: 0x04001C7C RID: 7292
		public ClickableComponent backButton;

		// Token: 0x04001C7D RID: 7293
		public bool scrolling;

		// Token: 0x04001C7E RID: 7294
		public bool deleteConfirmationScreen;

		// Token: 0x04001C7F RID: 7295
		protected List<LoadGameMenu.MenuSlot> menuSlots = new List<LoadGameMenu.MenuSlot>();

		// Token: 0x04001C80 RID: 7296
		private Rectangle scrollBarRunner;

		// Token: 0x04001C81 RID: 7297
		protected string hoverText = "";

		// Token: 0x04001C82 RID: 7298
		public bool loading;

		// Token: 0x04001C83 RID: 7299
		public bool drawn;

		// Token: 0x04001C84 RID: 7300
		public bool deleting;

		// Token: 0x04001C85 RID: 7301
		private int _updatesSinceLastDeleteConfirmScreen;

		// Token: 0x04001C86 RID: 7302
		private Task<List<Farmer>> _initTask;

		// Token: 0x04001C87 RID: 7303
		private Task _deleteTask;

		// Token: 0x04001C88 RID: 7304
		private bool disposedValue;

		// Token: 0x0200061F RID: 1567
		public abstract class MenuSlot : IDisposable
		{
			// Token: 0x06004441 RID: 17473 RVA: 0x0031BC6B File Offset: 0x00319E6B
			public MenuSlot(LoadGameMenu menu)
			{
				this.menu = menu;
			}

			// Token: 0x06004442 RID: 17474
			public abstract void Activate();

			// Token: 0x06004443 RID: 17475
			public abstract void Draw(SpriteBatch b, int i);

			// Token: 0x06004444 RID: 17476 RVA: 0x0031BC7A File Offset: 0x00319E7A
			public virtual void Dispose()
			{
			}

			// Token: 0x04002E89 RID: 11913
			public int ActivateDelay;

			// Token: 0x04002E8A RID: 11914
			protected LoadGameMenu menu;
		}

		// Token: 0x02000620 RID: 1568
		public class SaveFileSlot : LoadGameMenu.MenuSlot
		{
			// Token: 0x06004445 RID: 17477 RVA: 0x0031BC7C File Offset: 0x00319E7C
			public SaveFileSlot(LoadGameMenu menu, Farmer farmer, int? slotNumber) : base(menu)
			{
				this.ActivateDelay = 2150;
				this.Farmer = farmer;
				this.SlotNumber = slotNumber;
				this.versionComparison = Utility.CompareGameVersions(Game1.version, farmer.gameVersion, true);
			}

			// Token: 0x06004446 RID: 17478 RVA: 0x0031BCB5 File Offset: 0x00319EB5
			public override void Activate()
			{
				SaveGame.Load(this.Farmer.slotName);
				Game1.exitActiveMenu();
			}

			// Token: 0x06004447 RID: 17479 RVA: 0x0031BCCC File Offset: 0x00319ECC
			protected virtual void drawSlotSaveNumber(SpriteBatch b, int i)
			{
				LoadGameMenu.MenuSlot menuSlot = this.menu.MenuSlots[this.menu.currentItemIndex + i];
				ClickableComponent button = this.menu.slotButtons[i];
				LoadGameMenu.SaveFileSlot saveFileSlot = menuSlot as LoadGameMenu.SaveFileSlot;
				string slotNumberLabel = (((saveFileSlot != null) ? saveFileSlot.SlotNumber : null) ?? (this.menu.currentItemIndex + i + 1)).ToString() + ".";
				SpriteText.drawString(b, slotNumberLabel, button.bounds.X + 28 + 32 - SpriteText.getWidthOfString(slotNumberLabel, 999999) / 2, button.bounds.Y + 36, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}

			// Token: 0x06004448 RID: 17480 RVA: 0x0031BDB2 File Offset: 0x00319FB2
			protected virtual string slotName()
			{
				return this.Farmer.Name;
			}

			// Token: 0x06004449 RID: 17481 RVA: 0x0031BDBF File Offset: 0x00319FBF
			public virtual float getSlotAlpha()
			{
				return 1f;
			}

			// Token: 0x0600444A RID: 17482 RVA: 0x0031BDC8 File Offset: 0x00319FC8
			protected virtual void drawSlotName(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, this.slotName(), this.menu.slotButtons[i].bounds.X + 128 + 36, this.menu.slotButtons[i].bounds.Y + 36, 999999, -1, 999999, this.getSlotAlpha(), 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}

			// Token: 0x0600444B RID: 17483 RVA: 0x0031BE4C File Offset: 0x0031A04C
			protected virtual void drawSlotShadow(SpriteBatch b, int i)
			{
				Vector2 offset = this.portraitOffset();
				b.Draw(Game1.shadowTexture, new Vector2((float)this.menu.slotButtons[i].bounds.X + offset.X + 32f, (float)(this.menu.slotButtons[i].bounds.Y + 128 + 16)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.8f);
			}

			// Token: 0x0600444C RID: 17484 RVA: 0x0031BF17 File Offset: 0x0031A117
			protected virtual Vector2 portraitOffset()
			{
				return new Vector2(92f, 20f);
			}

			// Token: 0x0600444D RID: 17485 RVA: 0x0031BF28 File Offset: 0x0031A128
			protected virtual void drawSlotFarmer(SpriteBatch b, int i)
			{
				Vector2 offset = this.portraitOffset();
				FarmerRenderer.isDrawingForUI = true;
				this.Farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, false, false, null, false), 0, new Rectangle(0, 0, 16, 32), new Vector2((float)this.menu.slotButtons[i].bounds.X + offset.X, (float)this.menu.slotButtons[i].bounds.Y + offset.Y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, this.Farmer);
				FarmerRenderer.isDrawingForUI = false;
			}

			// Token: 0x0600444E RID: 17486 RVA: 0x0031BFDC File Offset: 0x0031A1DC
			protected virtual void drawSlotDate(SpriteBatch b, int i)
			{
				string dateStringForSaveGame;
				if (this.Farmer.dayOfMonthForSaveGame != null && this.Farmer.seasonForSaveGame != null && this.Farmer.yearForSaveGame != null)
				{
					dateStringForSaveGame = Utility.getDateStringFor(this.Farmer.dayOfMonthForSaveGame.Value, this.Farmer.seasonForSaveGame.Value, this.Farmer.yearForSaveGame.Value);
				}
				else
				{
					dateStringForSaveGame = this.Farmer.dateStringForSaveGame;
				}
				Utility.drawTextWithShadow(b, dateStringForSaveGame, Game1.dialogueFont, new Vector2((float)(this.menu.slotButtons[i].bounds.X + 128 + 32), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 40)), Game1.textColor * this.getSlotAlpha(), 1f, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x0600444F RID: 17487 RVA: 0x0031C0DD File Offset: 0x0031A2DD
			protected virtual string slotSubName()
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11019", this.Farmer.farmName);
			}

			// Token: 0x06004450 RID: 17488 RVA: 0x0031C0FC File Offset: 0x0031A2FC
			protected virtual void drawSlotSubName(SpriteBatch b, int i)
			{
				string subName = this.slotSubName();
				Utility.drawTextWithShadow(b, subName, Game1.dialogueFont, new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width - 128) - Game1.dialogueFont.MeasureString(subName).X, (float)(this.menu.slotButtons[i].bounds.Y + 44)), Game1.textColor * this.getSlotAlpha(), 1f, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x06004451 RID: 17489 RVA: 0x0031C1A4 File Offset: 0x0031A3A4
			protected virtual void drawSlotMoney(SpriteBatch b, int i)
			{
				string cashText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", Utility.getNumberWithCommas(this.Farmer.Money));
				if (this.Farmer.Money == 1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
				{
					cashText = cashText.Substring(0, cashText.Length - 1);
				}
				int moneyWidth = (int)Game1.dialogueFont.MeasureString(cashText).X;
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width - 192 - 100 - moneyWidth), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 44)), new Rectangle(193, 373, 9, 9), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Vector2 position = new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width - 192 - 60 - moneyWidth), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 44));
				if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
				{
					position.Y += 5f;
				}
				Utility.drawTextWithShadow(b, cashText, Game1.dialogueFont, position, Game1.textColor * this.getSlotAlpha(), 1f, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x06004452 RID: 17490 RVA: 0x0031C344 File Offset: 0x0031A544
			protected virtual void drawSlotTimer(SpriteBatch b, int i)
			{
				Vector2 position = new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width - 192 - 44), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 36));
				Utility.drawWithShadow(b, Game1.mouseCursors, position, new Rectangle(595, 1748, 9, 11), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				position = new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width - 192 - 4), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 44));
				if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
				{
					position.Y += 5f;
				}
				Utility.drawTextWithShadow(b, Utility.getHoursMinutesStringFromMilliseconds(this.Farmer.millisecondsPlayed), Game1.dialogueFont, position, Game1.textColor * this.getSlotAlpha(), 1f, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x06004453 RID: 17491 RVA: 0x0031C49C File Offset: 0x0031A69C
			public virtual void drawVersionMismatchSlot(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, this.slotName(), this.menu.slotButtons[i].bounds.X + 128, this.menu.slotButtons[i].bounds.Y + 36, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				string farm_name = this.slotSubName();
				Utility.drawTextWithShadow(b, farm_name, Game1.dialogueFont, new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width - 128) - Game1.dialogueFont.MeasureString(farm_name).X, (float)(this.menu.slotButtons[i].bounds.Y + 44)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				string game_version = this.Farmer.gameVersion;
				if (game_version == "-1")
				{
					game_version = "<1.4";
				}
				string mismatch_text = Game1.content.LoadString("Strings\\UI:VersionMismatch", game_version);
				Color text_color = Game1.textColor;
				if (Game1.currentGameTime.TotalGameTime.TotalSeconds < this.redTimer && (int)((this.redTimer - Game1.currentGameTime.TotalGameTime.TotalSeconds) / 0.25) % 2 == 1)
				{
					text_color = Color.Red;
				}
				Utility.drawTextWithShadow(b, mismatch_text, Game1.dialogueFont, new Vector2((float)(this.menu.slotButtons[i].bounds.X + 128), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 40)), text_color, 1f, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x06004454 RID: 17492 RVA: 0x0031C694 File Offset: 0x0031A894
			public override void Draw(SpriteBatch b, int i)
			{
				this.drawSlotSaveNumber(b, i);
				if (this.versionComparison < 0)
				{
					this.drawVersionMismatchSlot(b, i);
					return;
				}
				this.drawSlotName(b, i);
				this.drawSlotShadow(b, i);
				this.drawSlotFarmer(b, i);
				this.drawSlotDate(b, i);
				this.drawSlotSubName(b, i);
				this.drawSlotMoney(b, i);
				this.drawSlotTimer(b, i);
			}

			// Token: 0x06004455 RID: 17493 RVA: 0x0031C6F3 File Offset: 0x0031A8F3
			public new void Dispose()
			{
				this.Farmer.unload();
			}

			// Token: 0x04002E8B RID: 11915
			public Farmer Farmer;

			// Token: 0x04002E8C RID: 11916
			public int? SlotNumber;

			// Token: 0x04002E8D RID: 11917
			public double redTimer;

			// Token: 0x04002E8E RID: 11918
			public int versionComparison;
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;

namespace StardewValley.Menus
{
	// Token: 0x02000252 RID: 594
	public class BuildingPaintMenu : IClickableMenu
	{
		// Token: 0x06002771 RID: 10097 RVA: 0x001C1FCC File Offset: 0x001C01CC
		public BuildingPaintMenu(Building target_building) : base(Game1.uiViewport.Width / 2 - BuildingPaintMenu.WINDOW_WIDTH / 2, Game1.uiViewport.Height / 2 - BuildingPaintMenu.WINDOW_HEIGHT / 2, BuildingPaintMenu.WINDOW_WIDTH, BuildingPaintMenu.WINDOW_HEIGHT, false)
		{
			this.InitializeSavedColors();
			this._paintData = DataLoader.PaintData(Game1.content);
			Game1.player.Halt();
			this.building = target_building;
			this.colorTarget = target_building.netBuildingPaintColor.Value;
			this.buildingType = this.building.buildingType.Value;
			this.SetRegion(0);
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002772 RID: 10098 RVA: 0x001C20B6 File Offset: 0x001C02B6
		public virtual void InitializeSavedColors()
		{
			if (BuildingPaintMenu.savedColors == null)
			{
				BuildingPaintMenu.savedColors = new List<Vector3>();
			}
		}

		// Token: 0x06002773 RID: 10099 RVA: 0x001C20C9 File Offset: 0x001C02C9
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002774 RID: 10100 RVA: 0x001C20DF File Offset: 0x001C02DF
		public override void applyMovementKey(int direction)
		{
			if (this.colorSliderPanel.ApplyMovementKey(direction))
			{
				return;
			}
			base.applyMovementKey(direction);
		}

		// Token: 0x06002775 RID: 10101 RVA: 0x001C20F8 File Offset: 0x001C02F8
		public override void receiveGamePadButton(Buttons button)
		{
			if (button != Buttons.RightTrigger)
			{
				if (button == Buttons.LeftTrigger)
				{
					Game1.playSound("shwip", null);
					this.SetRegion((this.currentPaintRegion - 1 + this.regions.Count) % this.regions.Count);
				}
			}
			else
			{
				Game1.playSound("shwip", null);
				this.SetRegion((this.currentPaintRegion + 1 + this.regions.Count) % this.regions.Count);
			}
			base.receiveGamePadButton(button);
		}

		// Token: 0x06002776 RID: 10102 RVA: 0x001C2194 File Offset: 0x001C0394
		public override void update(GameTime time)
		{
			BuildingPaintMenu.BuildingColorSlider buildingColorSlider = this.activeSlider;
			if (buildingColorSlider != null)
			{
				buildingColorSlider.Update(Game1.getMouseX(), Game1.getMouseY());
			}
			base.update(time);
		}

		// Token: 0x06002777 RID: 10103 RVA: 0x001C21B8 File Offset: 0x001C03B8
		public override void releaseLeftClick(int x, int y)
		{
			this.activeSlider = null;
			base.releaseLeftClick(x, y);
		}

		// Token: 0x06002778 RID: 10104 RVA: 0x001C21CC File Offset: 0x001C03CC
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			for (int i = 0; i < this.savedColorButtons.Count; i++)
			{
				if (this.savedColorButtons[i].containsPoint(x, y))
				{
					BuildingPaintMenu.savedColors.RemoveAt(i);
					this.RepositionElements();
					Game1.playSound("coin", null);
					return;
				}
			}
			base.receiveRightClick(x, y, playSound);
		}

		// Token: 0x06002779 RID: 10105 RVA: 0x001C2234 File Offset: 0x001C0434
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.colorSliderPanel.ReceiveLeftClick(x, y, playSound))
			{
				return;
			}
			if (this.defaultColorButton.containsPoint(x, y))
			{
				int num = this.currentPaintRegion;
				if (num != 0)
				{
					if (num != 1)
					{
						this.colorTarget.Color3Default.Value = true;
					}
					else
					{
						this.colorTarget.Color2Default.Value = true;
					}
				}
				else
				{
					this.colorTarget.Color1Default.Value = true;
				}
				Game1.playSound("coin", null);
				this.RepositionElements();
				return;
			}
			for (int i = 0; i < this.savedColorButtons.Count; i++)
			{
				if (this.savedColorButtons[i].containsPoint(x, y))
				{
					this.colorSliderPanel.hueSlider.SetValue((int)BuildingPaintMenu.savedColors[i].X, false);
					this.colorSliderPanel.saturationSlider.SetValue((int)BuildingPaintMenu.savedColors[i].Y, false);
					this.colorSliderPanel.lightnessSlider.SetValue((int)Utility.Lerp((float)this.colorSliderPanel.lightnessSlider.min, (float)this.colorSliderPanel.lightnessSlider.max, BuildingPaintMenu.savedColors[i].Z), false);
					Game1.playSound("coin", null);
					return;
				}
			}
			if (this.copyColorButton.containsPoint(x, y))
			{
				if (this.SaveColor())
				{
					Game1.playSound("coin", null);
					this.RepositionElements();
					return;
				}
				Game1.playSound("cancel", null);
				return;
			}
			else
			{
				if (this.okButton.containsPoint(x, y))
				{
					base.exitThisMenu(playSound);
					return;
				}
				if (this.appearanceButton.containsPoint(x, y))
				{
					Game1.playSound("smallSelect", null);
					BuildingSkinMenu skinMenu = new BuildingSkinMenu(this.building, false);
					BuildingSkinMenu buildingSkinMenu = skinMenu;
					buildingSkinMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(buildingSkinMenu.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu menu)
					{
						if (this.building.CanBePainted())
						{
							BuildingPaintMenu reloadedMenu = new BuildingPaintMenu(this.building);
							IClickableMenu currentMenu = Game1.activeClickableMenu;
							IClickableMenu parentMenu = null;
							while (currentMenu.GetChildMenu() != null)
							{
								parentMenu = currentMenu;
								currentMenu = currentMenu.GetChildMenu();
								if (currentMenu is BuildingPaintMenu)
								{
									break;
								}
							}
							if (parentMenu == null)
							{
								Game1.activeClickableMenu = reloadedMenu;
							}
							else
							{
								parentMenu.SetChildMenu(reloadedMenu);
							}
							if (Game1.options.SnappyMenus)
							{
								reloadedMenu.setCurrentlySnappedComponentTo(109);
								reloadedMenu.snapCursorToCurrentSnappedComponent();
								return;
							}
						}
						else
						{
							base.exitThisMenuNoSound();
						}
					}));
					base.SetChildMenu(skinMenu);
					return;
				}
				if (this.previousRegionButton.containsPoint(x, y))
				{
					Game1.playSound("shwip", null);
					this.SetRegion((this.currentPaintRegion - 1 + this.regions.Count) % this.regions.Count);
					return;
				}
				if (this.nextRegionButton.containsPoint(x, y))
				{
					Game1.playSound("shwip", null);
					this.SetRegion((this.currentPaintRegion + 1) % this.regions.Count);
					return;
				}
				base.receiveLeftClick(x, y, playSound);
				return;
			}
		}

		// Token: 0x0600277A RID: 10106 RVA: 0x001C24E4 File Offset: 0x001C06E4
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.okButton.tryHover(x, y, 0.1f);
			this.previousRegionButton.tryHover(x, y, 0.1f);
			this.nextRegionButton.tryHover(x, y, 0.1f);
			this.copyColorButton.tryHover(x, y, 0.1f);
			this.defaultColorButton.tryHover(x, y, 0.1f);
			this.appearanceButton.tryHover(x, y, 0.1f);
			if (this.appearanceButton.containsPoint(x, y))
			{
				this.hoverText = this.appearanceButton.name;
			}
			foreach (ClickableTextureComponent clickableTextureComponent in this.savedColorButtons)
			{
				clickableTextureComponent.tryHover(x, y, 0.1f);
			}
			this.colorSliderPanel.PerformHoverAction(x, y);
		}

		// Token: 0x0600277B RID: 10107 RVA: 0x001C25E0 File Offset: 0x001C07E0
		public virtual void RepositionElements()
		{
			this.previewPane.X = this.xPositionOnScreen;
			this.previewPane.Y = this.yPositionOnScreen;
			this.previewPane.Width = 512;
			this.previewPane.Height = 576;
			this.colorPane.Width = 448;
			this.colorPane.X = this.xPositionOnScreen + this.width - this.colorPane.Width;
			this.colorPane.Y = this.yPositionOnScreen;
			this.colorPane.Height = 576;
			Rectangle panel_rectangle = this.colorPane;
			panel_rectangle.Inflate(-32, -32);
			this.previousRegionButton = new ClickableTextureComponent(new Rectangle(panel_rectangle.Left, panel_rectangle.Top, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
			{
				myID = 103,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = 105,
				upNeighborID = -99998,
				fullyImmutable = true
			};
			this.nextRegionButton = new ClickableTextureComponent(new Rectangle(panel_rectangle.Right - 64, panel_rectangle.Top, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
			{
				myID = 102,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = 105,
				upNeighborID = -99998,
				fullyImmutable = true
			};
			panel_rectangle.Y += 64;
			panel_rectangle.Height = 0;
			int color_x = panel_rectangle.Left;
			this.defaultColorButton = new ClickableTextureComponent(new Rectangle(color_x, panel_rectangle.Bottom, 64, 64), Game1.mouseCursors2, new Rectangle(80, 144, 16, 16), 4f, false)
			{
				region = 1000,
				myID = 105,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				fullyImmutable = true
			};
			color_x += 80;
			this.savedColorButtons.Clear();
			this.buttonColors.Clear();
			for (int i = 0; i < BuildingPaintMenu.savedColors.Count; i++)
			{
				if (color_x + 64 > panel_rectangle.X + panel_rectangle.Width)
				{
					color_x = panel_rectangle.X;
					panel_rectangle.Y += 72;
				}
				ClickableTextureComponent color_button = new ClickableTextureComponent(new Rectangle(color_x, panel_rectangle.Bottom, 64, 64), Game1.mouseCursors2, new Rectangle(96, 144, 16, 16), 4f, false)
				{
					region = 1000,
					myID = i,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					fullyImmutable = true
				};
				color_x += 80;
				this.savedColorButtons.Add(color_button);
				Vector3 saved_color = BuildingPaintMenu.savedColors[i];
				int r;
				int g;
				int b;
				Utility.HSLtoRGB((double)saved_color.X, (double)(saved_color.Y / 100f), (double)Utility.Lerp(0.25f, 0.5f, saved_color.Z), out r, out g, out b);
				this.buttonColors.Add(new Color((int)((byte)r), (int)((byte)g), (int)((byte)b)));
			}
			if (color_x + 64 > panel_rectangle.X + panel_rectangle.Width)
			{
				color_x = panel_rectangle.X;
				panel_rectangle.Y += 72;
			}
			this.copyColorButton = new ClickableTextureComponent(new Rectangle(color_x, panel_rectangle.Bottom, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f, false)
			{
				region = 1000,
				myID = 104,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				fullyImmutable = true
			};
			panel_rectangle.Y += 80;
			panel_rectangle = this.colorSliderPanel.Reposition(panel_rectangle);
			panel_rectangle.Y += 64;
			this.okButton = new ClickableTextureComponent(new Rectangle(this.colorPane.Right - 64 - 16, this.colorPane.Bottom - 64 - 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101,
				upNeighborID = 108,
				leftNeighborID = 109
			};
			this.appearanceButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_ChangeAppearance"), new Rectangle(this.previewPane.Right - 64 - 16, this.colorPane.Bottom - 64 - 16, 64, 64), null, null, Game1.mouseCursors2, new Rectangle(96, 208, 16, 16), 4f, false)
			{
				myID = 109,
				upNeighborID = 108,
				rightNeighborID = 101,
				visible = this.building.CanBeReskinned(false)
			};
			this.populateClickableComponentList();
		}

		// Token: 0x0600277C RID: 10108 RVA: 0x001C2B24 File Offset: 0x001C0D24
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region == 1000 && b.region != 1000)
			{
				switch (direction)
				{
				case 1:
				case 3:
					return false;
				case 2:
					if (b.myID != 106)
					{
						return false;
					}
					break;
				}
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x0600277D RID: 10109 RVA: 0x001C2B78 File Offset: 0x001C0D78
		public virtual bool SaveColor()
		{
			if ((this.currentPaintRegion == 0 && this.colorTarget.Color1Default.Value) || (this.currentPaintRegion == 1 && this.colorTarget.Color2Default.Value) || (this.currentPaintRegion == 2 && this.colorTarget.Color3Default.Value))
			{
				return false;
			}
			Vector3 saved_color = new Vector3((float)this.colorSliderPanel.hueSlider.GetValue(), (float)this.colorSliderPanel.saturationSlider.GetValue(), (float)(this.colorSliderPanel.lightnessSlider.GetValue() - this.colorSliderPanel.lightnessSlider.min) / (float)(this.colorSliderPanel.lightnessSlider.max - this.colorSliderPanel.lightnessSlider.min));
			if (BuildingPaintMenu.savedColors.Count >= 8)
			{
				BuildingPaintMenu.savedColors.RemoveAt(0);
			}
			BuildingPaintMenu.savedColors.Add(saved_color);
			return true;
		}

		// Token: 0x0600277E RID: 10110 RVA: 0x001C2C6C File Offset: 0x001C0E6C
		public virtual void SetRegion(int new_region)
		{
			if (this.regions == null)
			{
				this.LoadRegionData();
			}
			if (new_region < this.regions.Count && new_region >= 0)
			{
				this.currentPaintRegion = new_region;
				BuildingPaintMenu.RegionData region = this.regions[new_region];
				this.colorSliderPanel = new BuildingPaintMenu.ColorSliderPanel(this, new_region, region.Id, region.MinBrightness, region.MaxBrightness);
			}
			this.RepositionElements();
		}

		// Token: 0x0600277F RID: 10111 RVA: 0x001C2CD4 File Offset: 0x001C0ED4
		public virtual void LoadRegionData()
		{
			if (this.regions == null)
			{
				this.regions = new List<BuildingPaintMenu.RegionData>();
				string lookupName = this.building.GetPaintDataKey(this._paintData);
				string rawData;
				string data = (lookupName != null && this._paintData.TryGetValue(lookupName, out rawData)) ? rawData.Replace("\n", "").Replace("\t", "") : null;
				if (data != null)
				{
					string[] data_split = data.Split('/', StringSplitOptions.None);
					for (int i = 0; i < data_split.Length / 2; i++)
					{
						if (!(data_split[i].Trim() == ""))
						{
							string regionId = data_split[i * 2];
							string[] brightness_split = ArgUtility.SplitBySpace(data_split[i * 2 + 1]);
							int min_brightness = -100;
							int max_brightness = 100;
							if (brightness_split.Length >= 2)
							{
								try
								{
									min_brightness = int.Parse(brightness_split[0]);
									max_brightness = int.Parse(brightness_split[1]);
								}
								catch (Exception)
								{
								}
							}
							string region_name = Game1.content.LoadStringReturnNullIfNotFound("Strings/Buildings:Paint_Region_" + regionId, true) ?? regionId;
							this.regions.Add(new BuildingPaintMenu.RegionData(regionId, region_name, min_brightness, max_brightness));
						}
					}
				}
			}
		}

		// Token: 0x06002780 RID: 10112 RVA: 0x001C2E08 File Offset: 0x001C1008
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			Game1.DrawBox(this.previewPane.X, this.previewPane.Y, this.previewPane.Width, this.previewPane.Height, null);
			Rectangle rectangle = this.previewPane;
			rectangle.Inflate(0, 0);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			b.GraphicsDevice.ScissorRectangle = rectangle;
			Vector2 building_draw_center = new Vector2((float)(this.previewPane.X + this.previewPane.Width / 2), (float)(this.previewPane.Y + this.previewPane.Height / 2 - 16));
			Rectangle sourceRect = this.building.getSourceRectForMenu() ?? this.building.getSourceRect();
			this.building.drawInMenu(b, (int)building_draw_center.X - (int)((float)this.building.tilesWide.Value / 2f * 64f), (int)building_draw_center.Y - sourceRect.Height * 4 / 2);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			Game1.DrawBox(this.colorPane.X, this.colorPane.Y, this.colorPane.Width, this.colorPane.Height, null);
			BuildingPaintMenu.RegionData region = this.regions[this.currentPaintRegion];
			int text_height = SpriteText.getHeightOfString(region.DisplayName, 999999);
			SpriteText.drawStringHorizontallyCenteredAt(b, region.DisplayName, this.colorPane.X + this.colorPane.Width / 2, this.nextRegionButton.bounds.Center.Y - text_height / 2, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
			this.okButton.draw(b);
			this.appearanceButton.draw(b);
			this.colorSliderPanel.Draw(b);
			this.nextRegionButton.draw(b);
			this.previousRegionButton.draw(b);
			this.copyColorButton.draw(b);
			this.defaultColorButton.draw(b);
			for (int i = 0; i < this.savedColorButtons.Count; i++)
			{
				this.savedColorButtons[i].draw(b, this.buttonColors[i], 1f, 0, 0, 0);
			}
			if (base.GetChildMenu() == null)
			{
				base.drawMouse(b, false, -1);
				string text = this.hoverText;
				if (text != null && text.Length > 0)
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
			}
		}

		// Token: 0x040018AC RID: 6316
		public const int region_colorButtons = 1000;

		// Token: 0x040018AD RID: 6317
		public const int region_okButton = 101;

		// Token: 0x040018AE RID: 6318
		public const int region_nextRegion = 102;

		// Token: 0x040018AF RID: 6319
		public const int region_prevRegion = 103;

		// Token: 0x040018B0 RID: 6320
		public const int region_copyColor = 104;

		// Token: 0x040018B1 RID: 6321
		public const int region_defaultColor = 105;

		// Token: 0x040018B2 RID: 6322
		public const int region_hueSlider = 106;

		// Token: 0x040018B3 RID: 6323
		public const int region_saturationSlider = 107;

		// Token: 0x040018B4 RID: 6324
		public const int region_lightnessSlider = 108;

		// Token: 0x040018B5 RID: 6325
		public const int region_appearanceButton = 109;

		// Token: 0x040018B6 RID: 6326
		public static int WINDOW_WIDTH = 1024;

		// Token: 0x040018B7 RID: 6327
		public static int WINDOW_HEIGHT = 576;

		// Token: 0x040018B8 RID: 6328
		public Rectangle previewPane;

		// Token: 0x040018B9 RID: 6329
		public Rectangle colorPane;

		// Token: 0x040018BA RID: 6330
		public BuildingPaintMenu.BuildingColorSlider activeSlider;

		// Token: 0x040018BB RID: 6331
		public ClickableTextureComponent appearanceButton;

		// Token: 0x040018BC RID: 6332
		public ClickableTextureComponent okButton;

		// Token: 0x040018BD RID: 6333
		public static List<Vector3> savedColors = null;

		// Token: 0x040018BE RID: 6334
		public List<Color> buttonColors = new List<Color>();

		// Token: 0x040018BF RID: 6335
		public BuildingPaintMenu.ColorSliderPanel colorSliderPanel;

		// Token: 0x040018C0 RID: 6336
		private string hoverText = "";

		// Token: 0x040018C1 RID: 6337
		public Building building;

		// Token: 0x040018C2 RID: 6338
		public string buildingType = "";

		// Token: 0x040018C3 RID: 6339
		public BuildingPaintColor colorTarget;

		// Token: 0x040018C4 RID: 6340
		protected Dictionary<string, string> _paintData;

		// Token: 0x040018C5 RID: 6341
		public int currentPaintRegion;

		// Token: 0x040018C6 RID: 6342
		public List<BuildingPaintMenu.RegionData> regions;

		// Token: 0x040018C7 RID: 6343
		public ClickableTextureComponent nextRegionButton;

		// Token: 0x040018C8 RID: 6344
		public ClickableTextureComponent previousRegionButton;

		// Token: 0x040018C9 RID: 6345
		public ClickableTextureComponent copyColorButton;

		// Token: 0x040018CA RID: 6346
		public ClickableTextureComponent defaultColorButton;

		// Token: 0x040018CB RID: 6347
		public List<ClickableTextureComponent> savedColorButtons = new List<ClickableTextureComponent>();

		// Token: 0x040018CC RID: 6348
		public List<ClickableComponent> sliderHandles = new List<ClickableComponent>();

		// Token: 0x020005EB RID: 1515
		public class RegionData
		{
			// Token: 0x170004F6 RID: 1270
			// (get) Token: 0x0600437D RID: 17277 RVA: 0x003197D7 File Offset: 0x003179D7
			public string Id { get; }

			// Token: 0x170004F7 RID: 1271
			// (get) Token: 0x0600437E RID: 17278 RVA: 0x003197DF File Offset: 0x003179DF
			public string DisplayName { get; }

			// Token: 0x170004F8 RID: 1272
			// (get) Token: 0x0600437F RID: 17279 RVA: 0x003197E7 File Offset: 0x003179E7
			public int MinBrightness { get; }

			// Token: 0x170004F9 RID: 1273
			// (get) Token: 0x06004380 RID: 17280 RVA: 0x003197EF File Offset: 0x003179EF
			public int MaxBrightness { get; }

			// Token: 0x06004381 RID: 17281 RVA: 0x003197F7 File Offset: 0x003179F7
			public RegionData(string id, string displayName, int minBrightness, int maxBrightness)
			{
				this.Id = id;
				this.DisplayName = displayName;
				this.MinBrightness = minBrightness;
				this.MaxBrightness = maxBrightness;
			}
		}

		// Token: 0x020005EC RID: 1516
		public class ColorSliderPanel
		{
			// Token: 0x06004382 RID: 17282 RVA: 0x0031981C File Offset: 0x00317A1C
			public ColorSliderPanel(BuildingPaintMenu menu, int region_index, string regionId, int min_brightness = -100, int max_brightness = 100)
			{
				this.regionIndex = region_index;
				this.buildingPaintMenu = menu;
				this.regionId = regionId;
				this.minimumBrightness = min_brightness;
				this.maximumBrightness = max_brightness;
			}

			// Token: 0x06004383 RID: 17283 RVA: 0x0031987A File Offset: 0x00317A7A
			public virtual int GetHeight()
			{
				return this.rectangle.Height;
			}

			// Token: 0x06004384 RID: 17284 RVA: 0x00319888 File Offset: 0x00317A88
			public virtual Rectangle Reposition(Rectangle start_rect)
			{
				this.buildingPaintMenu.sliderHandles.Clear();
				this.rectangle.X = start_rect.X;
				this.rectangle.Y = start_rect.Y;
				this.rectangle.Width = start_rect.Width;
				this.rectangle.Height = 0;
				this.lightnessSlider = null;
				this.hueSlider = null;
				this.saturationSlider = null;
				this.colorDrawPosition = new Vector2((float)(start_rect.X + start_rect.Width - 64), (float)start_rect.Y);
				this.hueSlider = new BuildingPaintMenu.BuildingColorSlider(this.buildingPaintMenu, 106, new Rectangle(this.rectangle.Left, this.rectangle.Bottom, this.rectangle.Width - 100, 12), 0, 360, delegate(int v)
				{
					int num2 = this.regionIndex;
					if (num2 != 0)
					{
						if (num2 != 1)
						{
							this.buildingPaintMenu.colorTarget.Color3Default.Value = false;
						}
						else
						{
							this.buildingPaintMenu.colorTarget.Color2Default.Value = false;
						}
					}
					else
					{
						this.buildingPaintMenu.colorTarget.Color1Default.Value = false;
					}
					this.ApplyColors();
				});
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider = this.hueSlider;
				buildingColorSlider.getDrawColor = (Func<float, Color>)Delegate.Combine(buildingColorSlider.getDrawColor, new Func<float, Color>((float val) => this.GetColorForValues(val, 100f)));
				int num = this.regionIndex;
				if (num != 0)
				{
					if (num != 1)
					{
						this.hueSlider.SetValue(this.buildingPaintMenu.colorTarget.Color3Hue.Value, true);
					}
					else
					{
						this.hueSlider.SetValue(this.buildingPaintMenu.colorTarget.Color2Hue.Value, true);
					}
				}
				else
				{
					this.hueSlider.SetValue(this.buildingPaintMenu.colorTarget.Color1Hue.Value, true);
				}
				this.rectangle.Height = this.rectangle.Height + 24;
				this.saturationSlider = new BuildingPaintMenu.BuildingColorSlider(this.buildingPaintMenu, 107, new Rectangle(this.rectangle.Left, this.rectangle.Bottom, this.rectangle.Width - 100, 12), 0, 75, delegate(int v)
				{
					int num2 = this.regionIndex;
					if (num2 != 0)
					{
						if (num2 != 1)
						{
							this.buildingPaintMenu.colorTarget.Color3Default.Value = false;
						}
						else
						{
							this.buildingPaintMenu.colorTarget.Color2Default.Value = false;
						}
					}
					else
					{
						this.buildingPaintMenu.colorTarget.Color1Default.Value = false;
					}
					this.ApplyColors();
				});
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider2 = this.saturationSlider;
				buildingColorSlider2.getDrawColor = (Func<float, Color>)Delegate.Combine(buildingColorSlider2.getDrawColor, new Func<float, Color>((float val) => this.GetColorForValues((float)this.hueSlider.GetValue(), val)));
				num = this.regionIndex;
				if (num != 0)
				{
					if (num != 1)
					{
						this.saturationSlider.SetValue(this.buildingPaintMenu.colorTarget.Color3Saturation.Value, true);
					}
					else
					{
						this.saturationSlider.SetValue(this.buildingPaintMenu.colorTarget.Color2Saturation.Value, true);
					}
				}
				else
				{
					this.saturationSlider.SetValue(this.buildingPaintMenu.colorTarget.Color1Saturation.Value, true);
				}
				this.rectangle.Height = this.rectangle.Height + 24;
				this.lightnessSlider = new BuildingPaintMenu.BuildingColorSlider(this.buildingPaintMenu, 108, new Rectangle(this.rectangle.Left, this.rectangle.Bottom, this.rectangle.Width - 100, 12), this.minimumBrightness, this.maximumBrightness, delegate(int v)
				{
					int num2 = this.regionIndex;
					if (num2 != 0)
					{
						if (num2 != 1)
						{
							this.buildingPaintMenu.colorTarget.Color3Default.Value = false;
						}
						else
						{
							this.buildingPaintMenu.colorTarget.Color2Default.Value = false;
						}
					}
					else
					{
						this.buildingPaintMenu.colorTarget.Color1Default.Value = false;
					}
					this.ApplyColors();
				});
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider3 = this.lightnessSlider;
				buildingColorSlider3.getDrawColor = (Func<float, Color>)Delegate.Combine(buildingColorSlider3.getDrawColor, new Func<float, Color>((float val) => this.GetColorForValues((float)this.hueSlider.GetValue(), (float)this.saturationSlider.GetValue(), val)));
				num = this.regionIndex;
				if (num != 0)
				{
					if (num != 1)
					{
						this.lightnessSlider.SetValue(this.buildingPaintMenu.colorTarget.Color3Lightness.Value, true);
					}
					else
					{
						this.lightnessSlider.SetValue(this.buildingPaintMenu.colorTarget.Color2Lightness.Value, true);
					}
				}
				else
				{
					this.lightnessSlider.SetValue(this.buildingPaintMenu.colorTarget.Color1Lightness.Value, true);
				}
				this.rectangle.Height = this.rectangle.Height + 24;
				if ((this.regionIndex == 0 && this.buildingPaintMenu.colorTarget.Color1Default.Value) || (this.regionIndex == 1 && this.buildingPaintMenu.colorTarget.Color2Default.Value) || (this.regionIndex == 2 && this.buildingPaintMenu.colorTarget.Color3Default.Value))
				{
					this.hueSlider.SetValue(this.hueSlider.min, true);
					this.saturationSlider.SetValue(this.saturationSlider.max, true);
					this.lightnessSlider.SetValue((this.lightnessSlider.min + this.lightnessSlider.max) / 2, true);
				}
				this.buildingPaintMenu.sliderHandles.Add(this.hueSlider.handle);
				this.buildingPaintMenu.sliderHandles.Add(this.saturationSlider.handle);
				this.buildingPaintMenu.sliderHandles.Add(this.lightnessSlider.handle);
				this.hueSlider.handle.upNeighborID = 104;
				this.hueSlider.handle.downNeighborID = 107;
				this.saturationSlider.handle.downNeighborID = 108;
				this.saturationSlider.handle.upNeighborID = 106;
				this.lightnessSlider.handle.upNeighborID = 107;
				this.rectangle.Height = this.rectangle.Height + 32;
				start_rect.Y += this.rectangle.Height;
				return start_rect;
			}

			// Token: 0x06004385 RID: 17285 RVA: 0x00319DA4 File Offset: 0x00317FA4
			public virtual void ApplyColors()
			{
				int num = this.regionIndex;
				if (num == 0)
				{
					this.buildingPaintMenu.colorTarget.Color1Hue.Value = this.hueSlider.GetValue();
					this.buildingPaintMenu.colorTarget.Color1Saturation.Value = this.saturationSlider.GetValue();
					this.buildingPaintMenu.colorTarget.Color1Lightness.Value = this.lightnessSlider.GetValue();
					return;
				}
				if (num != 1)
				{
					this.buildingPaintMenu.colorTarget.Color3Hue.Value = this.hueSlider.GetValue();
					this.buildingPaintMenu.colorTarget.Color3Saturation.Value = this.saturationSlider.GetValue();
					this.buildingPaintMenu.colorTarget.Color3Lightness.Value = this.lightnessSlider.GetValue();
					return;
				}
				this.buildingPaintMenu.colorTarget.Color2Hue.Value = this.hueSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color2Saturation.Value = this.saturationSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color2Lightness.Value = this.lightnessSlider.GetValue();
			}

			// Token: 0x06004386 RID: 17286 RVA: 0x00319EE8 File Offset: 0x003180E8
			public virtual void Draw(SpriteBatch b)
			{
				if ((this.regionIndex != 0 || !this.buildingPaintMenu.colorTarget.Color1Default.Value) && (this.regionIndex != 1 || !this.buildingPaintMenu.colorTarget.Color2Default.Value) && (this.regionIndex != 2 || !this.buildingPaintMenu.colorTarget.Color3Default.Value))
				{
					Color drawn_color = this.GetColorForValues((float)this.hueSlider.GetValue(), (float)this.saturationSlider.GetValue(), (float)this.lightnessSlider.GetValue());
					b.Draw(Game1.staminaRect, new Rectangle((int)this.colorDrawPosition.X - 4, (int)this.colorDrawPosition.Y - 4, 72, 72), null, Game1.textColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					b.Draw(Game1.staminaRect, new Rectangle((int)this.colorDrawPosition.X, (int)this.colorDrawPosition.Y, 64, 64), null, drawn_color, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider = this.hueSlider;
				if (buildingColorSlider != null)
				{
					buildingColorSlider.Draw(b);
				}
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider2 = this.saturationSlider;
				if (buildingColorSlider2 != null)
				{
					buildingColorSlider2.Draw(b);
				}
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider3 = this.lightnessSlider;
				if (buildingColorSlider3 == null)
				{
					return;
				}
				buildingColorSlider3.Draw(b);
			}

			// Token: 0x06004387 RID: 17287 RVA: 0x0031A054 File Offset: 0x00318254
			public Color GetColorForValues(float hue_slider, float saturation_slider)
			{
				int red;
				int green;
				int blue;
				Utility.HSLtoRGB((double)hue_slider, (double)(saturation_slider / 100f), 0.5, out red, out green, out blue);
				return new Color((int)((byte)red), green, blue);
			}

			// Token: 0x06004388 RID: 17288 RVA: 0x0031A088 File Offset: 0x00318288
			public Color GetColorForValues(float hue_slider, float saturation_slider, float lightness_slider)
			{
				int red;
				int green;
				int blue;
				Utility.HSLtoRGB((double)hue_slider, (double)(saturation_slider / 100f), (double)Utility.Lerp(0.25f, 0.5f, (lightness_slider - (float)this.lightnessSlider.min) / (float)(this.lightnessSlider.max - this.lightnessSlider.min)), out red, out green, out blue);
				return new Color((int)((byte)red), green, blue);
			}

			// Token: 0x06004389 RID: 17289 RVA: 0x0031A0EC File Offset: 0x003182EC
			public virtual bool ApplyMovementKey(int direction)
			{
				if (direction == 3 || direction == 1)
				{
					if (this.saturationSlider.handle == this.buildingPaintMenu.currentlySnappedComponent)
					{
						this.saturationSlider.ApplyMovementKey(direction);
						return true;
					}
					if (this.hueSlider.handle == this.buildingPaintMenu.currentlySnappedComponent)
					{
						this.hueSlider.ApplyMovementKey(direction);
						return true;
					}
					if (this.lightnessSlider.handle == this.buildingPaintMenu.currentlySnappedComponent)
					{
						this.lightnessSlider.ApplyMovementKey(direction);
						return true;
					}
				}
				return false;
			}

			// Token: 0x0600438A RID: 17290 RVA: 0x0031A174 File Offset: 0x00318374
			public virtual void PerformHoverAction(int x, int y)
			{
			}

			// Token: 0x0600438B RID: 17291 RVA: 0x0031A176 File Offset: 0x00318376
			public virtual bool ReceiveLeftClick(int x, int y, bool play_sound = true)
			{
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider = this.hueSlider;
				if (buildingColorSlider != null)
				{
					buildingColorSlider.ReceiveLeftClick(x, y);
				}
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider2 = this.saturationSlider;
				if (buildingColorSlider2 != null)
				{
					buildingColorSlider2.ReceiveLeftClick(x, y);
				}
				BuildingPaintMenu.BuildingColorSlider buildingColorSlider3 = this.lightnessSlider;
				if (buildingColorSlider3 != null)
				{
					buildingColorSlider3.ReceiveLeftClick(x, y);
				}
				return false;
			}

			// Token: 0x04002DF7 RID: 11767
			public BuildingPaintMenu buildingPaintMenu;

			// Token: 0x04002DF8 RID: 11768
			public int regionIndex;

			// Token: 0x04002DF9 RID: 11769
			public string regionId = "Paint Region Name";

			// Token: 0x04002DFA RID: 11770
			public Rectangle rectangle;

			// Token: 0x04002DFB RID: 11771
			public Vector2 colorDrawPosition;

			// Token: 0x04002DFC RID: 11772
			public List<KeyValuePair<string, List<int>>> colors = new List<KeyValuePair<string, List<int>>>();

			// Token: 0x04002DFD RID: 11773
			public int selectedColor;

			// Token: 0x04002DFE RID: 11774
			public BuildingPaintMenu.BuildingColorSlider hueSlider;

			// Token: 0x04002DFF RID: 11775
			public BuildingPaintMenu.BuildingColorSlider saturationSlider;

			// Token: 0x04002E00 RID: 11776
			public BuildingPaintMenu.BuildingColorSlider lightnessSlider;

			// Token: 0x04002E01 RID: 11777
			public int minimumBrightness = -100;

			// Token: 0x04002E02 RID: 11778
			public int maximumBrightness = 100;
		}

		// Token: 0x020005ED RID: 1517
		public class BuildingColorSlider
		{
			// Token: 0x06004392 RID: 17298 RVA: 0x0031A338 File Offset: 0x00318538
			public BuildingColorSlider(BuildingPaintMenu bpm, int handle_id, Rectangle bounds, int min, int max, Action<int> on_value_set = null)
			{
				this.handle = new ClickableTextureComponent(new Rectangle(0, 0, 4, 5), Game1.mouseCursors, new Rectangle(72, 256, 16, 20), 1f, false);
				this.handle.myID = handle_id;
				this.handle.upNeighborID = -99998;
				this.handle.upNeighborImmutable = true;
				this.handle.downNeighborID = -99998;
				this.handle.downNeighborImmutable = true;
				this.handle.leftNeighborImmutable = true;
				this.handle.rightNeighborImmutable = true;
				this.buildingPaintMenu = bpm;
				this.bounds = bounds;
				this.min = min;
				this.max = max;
				this.onValueSet = on_value_set;
			}

			// Token: 0x06004393 RID: 17299 RVA: 0x0031A3FC File Offset: 0x003185FC
			public virtual void ApplyMovementKey(int direction)
			{
				int amount = Math.Max((this.max - this.min) / 50, 1);
				if (direction == 3)
				{
					this.SetValue(this._displayedValue - amount, false);
				}
				else
				{
					this.SetValue(this._displayedValue + amount, false);
				}
				if (this.buildingPaintMenu.currentlySnappedComponent == this.handle && Game1.options.SnappyMenus)
				{
					this.buildingPaintMenu.snapCursorToCurrentSnappedComponent();
				}
			}

			// Token: 0x06004394 RID: 17300 RVA: 0x0031A46E File Offset: 0x0031866E
			public virtual void ReceiveLeftClick(int x, int y)
			{
				if (this.bounds.Contains(x, y))
				{
					this.buildingPaintMenu.activeSlider = this;
					this.SetValueFromPosition(x, y);
				}
			}

			// Token: 0x06004395 RID: 17301 RVA: 0x0031A494 File Offset: 0x00318694
			public virtual void SetValueFromPosition(int x, int y)
			{
				if (this.bounds.Width == 0)
				{
					return;
				}
				if (this.min == this.max)
				{
					return;
				}
				float new_value = (float)(x - this.bounds.Left);
				new_value /= (float)this.bounds.Width;
				if (new_value < 0f)
				{
					new_value = 0f;
				}
				if (new_value > 1f)
				{
					new_value = 1f;
				}
				int steps = this.max - this.min;
				new_value /= (float)steps;
				new_value *= (float)steps;
				if (this._sliderPosition != new_value)
				{
					this._sliderPosition = new_value;
					this.SetValue(this.min + (int)(this._sliderPosition * (float)steps), false);
				}
			}

			// Token: 0x06004396 RID: 17302 RVA: 0x0031A538 File Offset: 0x00318738
			public void SetValue(int value, bool skip_value_set = false)
			{
				if (value > this.max)
				{
					value = this.max;
				}
				if (value < this.min)
				{
					value = this.min;
				}
				this._sliderPosition = (float)(value - this.min) / (float)(this.max - this.min);
				this.handle.bounds.X = (int)Utility.Lerp((float)this.bounds.Left, (float)this.bounds.Right, this._sliderPosition) - this.handle.bounds.Width / 2 * 4;
				this.handle.bounds.Y = this.bounds.Top - 4;
				if (this._displayedValue != value)
				{
					this._displayedValue = value;
					if (!skip_value_set)
					{
						Action<int> action = this.onValueSet;
						if (action == null)
						{
							return;
						}
						action(value);
					}
				}
			}

			// Token: 0x06004397 RID: 17303 RVA: 0x0031A60F File Offset: 0x0031880F
			public int GetValue()
			{
				return this._displayedValue;
			}

			// Token: 0x06004398 RID: 17304 RVA: 0x0031A618 File Offset: 0x00318818
			public virtual void Draw(SpriteBatch b)
			{
				int divisions = 20;
				for (int i = 0; i < divisions; i++)
				{
					Rectangle section_bounds = new Rectangle((int)((float)this.bounds.X + (float)this.bounds.Width / (float)divisions * (float)i), this.bounds.Y, (int)Math.Ceiling((double)((float)this.bounds.Width / (float)divisions)), this.bounds.Height);
					Color drawn_color = Color.Black;
					if (this.getDrawColor != null)
					{
						drawn_color = this.getDrawColor(Utility.Lerp((float)this.min, (float)this.max, (float)i / (float)divisions));
					}
					b.Draw(Game1.staminaRect, section_bounds, drawn_color);
				}
				this.handle.draw(b);
			}

			// Token: 0x06004399 RID: 17305 RVA: 0x0031A6D8 File Offset: 0x003188D8
			public virtual void Update(int x, int y)
			{
				this.SetValueFromPosition(x, y);
			}

			// Token: 0x04002E03 RID: 11779
			public ClickableTextureComponent handle;

			// Token: 0x04002E04 RID: 11780
			public BuildingPaintMenu buildingPaintMenu;

			// Token: 0x04002E05 RID: 11781
			public Rectangle bounds;

			// Token: 0x04002E06 RID: 11782
			protected float _sliderPosition;

			// Token: 0x04002E07 RID: 11783
			public int min;

			// Token: 0x04002E08 RID: 11784
			public int max;

			// Token: 0x04002E09 RID: 11785
			public Action<int> onValueSet;

			// Token: 0x04002E0A RID: 11786
			public Func<float, Color> getDrawColor;

			// Token: 0x04002E0B RID: 11787
			protected int _displayedValue;
		}
	}
}

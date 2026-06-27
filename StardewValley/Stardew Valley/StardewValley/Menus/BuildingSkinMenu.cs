using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus
{
	// Token: 0x02000253 RID: 595
	public class BuildingSkinMenu : IClickableMenu
	{
		// Token: 0x06002783 RID: 10115 RVA: 0x001C3208 File Offset: 0x001C1408
		public BuildingSkinMenu(Building targetBuilding, bool ignoreSeparateConstructionEntries = false) : base(Game1.uiViewport.Width / 2 - BuildingSkinMenu.WindowWidth / 2, Game1.uiViewport.Height / 2 - BuildingSkinMenu.WindowHeight / 2, BuildingSkinMenu.WindowWidth, BuildingSkinMenu.WindowHeight, false)
		{
			Game1.player.Halt();
			this.Building = targetBuilding;
			BuildingData buildingData = targetBuilding.GetData();
			this.BuildingDisplayName = TokenParser.ParseText(buildingData.Name, null, null, null);
			this.BuildingDescription = TokenParser.ParseText(buildingData.Description, null, null, null);
			int index = 0;
			this.Skins.Add(new BuildingSkinMenu.SkinEntry(index++, null, this.BuildingDisplayName, this.BuildingDescription));
			if (buildingData.Skins != null)
			{
				foreach (BuildingSkin skin2 in buildingData.Skins)
				{
					if (!(skin2.Id != this.Building.skinId.Value) || ((!ignoreSeparateConstructionEntries || !skin2.ShowAsSeparateConstructionEntry) && GameStateQuery.CheckConditions(skin2.Condition, this.Building.GetParentLocation(), null, null, null, null, null)))
					{
						this.Skins.Add(new BuildingSkinMenu.SkinEntry(index++, skin2));
					}
				}
			}
			this.RepositionElements();
			this.SetSkin(Math.Max(this.Skins.FindIndex((BuildingSkinMenu.SkinEntry skin) => skin.Id == this.Building.skinId.Value), 0));
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002784 RID: 10116 RVA: 0x001C33A4 File Offset: 0x001C15A4
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002785 RID: 10117 RVA: 0x001C33BC File Offset: 0x001C15BC
		public override void receiveGamePadButton(Buttons button)
		{
			if (button != Buttons.RightTrigger)
			{
				if (button == Buttons.LeftTrigger)
				{
					Game1.playSound("shwip", null);
					this.SetSkin(this.Skin.Index - 1);
				}
			}
			else
			{
				Game1.playSound("shwip", null);
				this.SetSkin(this.Skin.Index + 1);
			}
			base.receiveGamePadButton(button);
		}

		// Token: 0x06002786 RID: 10118 RVA: 0x001C3434 File Offset: 0x001C1634
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.OkButton.containsPoint(x, y))
			{
				base.exitThisMenu(playSound);
				return;
			}
			if (this.PreviousSkinButton.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this.SetSkin(this.Skin.Index - 1);
				return;
			}
			if (this.NextSkinButton.containsPoint(x, y))
			{
				this.SetSkin(this.Skin.Index + 1);
				Game1.playSound("shwip", null);
				return;
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002787 RID: 10119 RVA: 0x001C34D0 File Offset: 0x001C16D0
		public void SetSkin(int index)
		{
			if (this.Skins.Count == 0)
			{
				this.SetSkin(null);
				return;
			}
			index %= this.Skins.Count;
			if (index < 0)
			{
				index = this.Skins.Count + index;
			}
			this.SetSkin(this.Skins[index]);
		}

		// Token: 0x06002788 RID: 10120 RVA: 0x001C3528 File Offset: 0x001C1728
		public virtual void SetSkin(BuildingSkinMenu.SkinEntry skin)
		{
			this.Skin = skin;
			if (this.Building.skinId.Value != skin.Id)
			{
				this.Building.skinId.Value = skin.Id;
				this.Building.netBuildingPaintColor.Value.Color1Default.Value = true;
				this.Building.netBuildingPaintColor.Value.Color2Default.Value = true;
				this.Building.netBuildingPaintColor.Value.Color3Default.Value = true;
				BuildingData buildingData = this.Building.GetData();
				if (buildingData != null && this.Building.daysOfConstructionLeft.Value == buildingData.BuildDays)
				{
					NetFieldBase<int, NetInt> daysOfConstructionLeft = this.Building.daysOfConstructionLeft;
					BuildingSkin data = skin.Data;
					daysOfConstructionLeft.Value = (((data != null) ? data.BuildDays : null) ?? buildingData.BuildDays);
				}
			}
		}

		// Token: 0x06002789 RID: 10121 RVA: 0x001C362D File Offset: 0x001C182D
		public override void performHoverAction(int x, int y)
		{
			this.OkButton.tryHover(x, y, 0.1f);
			this.PreviousSkinButton.tryHover(x, y, 0.1f);
			this.NextSkinButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x0600278A RID: 10122 RVA: 0x001C3668 File Offset: 0x001C1868
		public virtual void RepositionElements()
		{
			this.PreviewPane.Y = this.yPositionOnScreen + 48;
			this.PreviewPane.Width = 576;
			this.PreviewPane.Height = 576;
			this.PreviewPane.X = this.xPositionOnScreen + this.width / 2 - this.PreviewPane.Width / 2;
			Rectangle panelRectangle = this.PreviewPane;
			panelRectangle.Inflate(-16, -16);
			this.PreviousSkinButton = new ClickableTextureComponent(new Rectangle(panelRectangle.Left, panelRectangle.Center.Y - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
			{
				myID = 103,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = 101,
				upNeighborID = -99998,
				fullyImmutable = true
			};
			this.NextSkinButton = new ClickableTextureComponent(new Rectangle(panelRectangle.Right - 64, panelRectangle.Center.Y - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
			{
				myID = 102,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = 101,
				upNeighborID = -99998,
				fullyImmutable = true
			};
			panelRectangle.Y += 64;
			panelRectangle.Height = 0;
			panelRectangle.Y += 80;
			panelRectangle.Y += 64;
			this.OkButton = new ClickableTextureComponent(new Rectangle(this.PreviewPane.Right - 64 - 16, this.PreviewPane.Bottom - 64 - 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101,
				upNeighborID = 102
			};
			if (this.Skins.Count == 0)
			{
				this.NextSkinButton.visible = false;
				this.PreviousSkinButton.visible = false;
			}
			this.populateClickableComponentList();
		}

		// Token: 0x0600278B RID: 10123 RVA: 0x001C3895 File Offset: 0x001C1A95
		public virtual bool SaveColor()
		{
			return true;
		}

		// Token: 0x0600278C RID: 10124 RVA: 0x001C3898 File Offset: 0x001C1A98
		public virtual void SetRegion(int newRegion)
		{
			this.RepositionElements();
		}

		// Token: 0x0600278D RID: 10125 RVA: 0x001C38A0 File Offset: 0x001C1AA0
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			Game1.DrawBox(this.PreviewPane.X, this.PreviewPane.Y, this.PreviewPane.Width, this.PreviewPane.Height, null);
			Rectangle rectangle = this.PreviewPane;
			rectangle.Inflate(0, 0);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			b.GraphicsDevice.ScissorRectangle = rectangle;
			Vector2 buildingDrawCenter = new Vector2((float)(this.PreviewPane.X + this.PreviewPane.Width / 2), (float)(this.PreviewPane.Y + this.PreviewPane.Height / 2 - 16));
			Rectangle sourceRect = this.Building.getSourceRectForMenu() ?? this.Building.getSourceRect();
			Building building = this.Building;
			if (building != null)
			{
				building.drawInMenu(b, (int)buildingDrawCenter.X - (int)((float)this.Building.tilesWide.Value / 2f * 64f), (int)buildingDrawCenter.Y - sourceRect.Height * 4 / 2);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\Buildings:BuildingSkinMenu_ChooseAppearance", this.BuildingDisplayName), this.xPositionOnScreen + this.width / 2, this.PreviewPane.Top - 96, "", 1f, null, 0, 0.88f, false);
			this.OkButton.draw(b);
			this.NextSkinButton.draw(b);
			this.PreviousSkinButton.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x040018CD RID: 6349
		public const int region_okButton = 101;

		// Token: 0x040018CE RID: 6350
		public const int region_nextSkin = 102;

		// Token: 0x040018CF RID: 6351
		public const int region_prevSkin = 103;

		// Token: 0x040018D0 RID: 6352
		public static int WindowWidth = 576;

		// Token: 0x040018D1 RID: 6353
		public static int WindowHeight = 576;

		// Token: 0x040018D2 RID: 6354
		public Rectangle PreviewPane;

		// Token: 0x040018D3 RID: 6355
		public ClickableTextureComponent OkButton;

		// Token: 0x040018D4 RID: 6356
		public Building Building;

		// Token: 0x040018D5 RID: 6357
		public ClickableTextureComponent NextSkinButton;

		// Token: 0x040018D6 RID: 6358
		public ClickableTextureComponent PreviousSkinButton;

		// Token: 0x040018D7 RID: 6359
		public string BuildingDisplayName;

		// Token: 0x040018D8 RID: 6360
		public string BuildingDescription;

		// Token: 0x040018D9 RID: 6361
		public List<BuildingSkinMenu.SkinEntry> Skins = new List<BuildingSkinMenu.SkinEntry>();

		// Token: 0x040018DA RID: 6362
		public BuildingSkinMenu.SkinEntry Skin;

		// Token: 0x020005EE RID: 1518
		public class SkinEntry
		{
			// Token: 0x0600439A RID: 17306 RVA: 0x0031A6E2 File Offset: 0x003188E2
			public SkinEntry(int index, BuildingSkin skin) : this(index, skin, TokenParser.ParseText(skin.Name, null, null, null), TokenParser.ParseText(skin.Description, null, null, null))
			{
			}

			// Token: 0x0600439B RID: 17307 RVA: 0x0031A708 File Offset: 0x00318908
			public SkinEntry(int index, BuildingSkin skin, string displayName, string description)
			{
				this.Index = index;
				this.Id = ((skin != null) ? skin.Id : null);
				this.Data = skin;
				this.DisplayName = displayName;
				this.Description = description;
			}

			// Token: 0x04002E0C RID: 11788
			public int Index;

			// Token: 0x04002E0D RID: 11789
			public readonly string Id;

			// Token: 0x04002E0E RID: 11790
			public readonly string DisplayName;

			// Token: 0x04002E0F RID: 11791
			public readonly string Description;

			// Token: 0x04002E10 RID: 11792
			public readonly BuildingSkin Data;
		}
	}
}

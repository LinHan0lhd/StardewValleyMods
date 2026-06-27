using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Powers;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus
{
	// Token: 0x0200029A RID: 666
	public class PowersTab : IClickableMenu
	{
		// Token: 0x06002B93 RID: 11155 RVA: 0x00210A04 File Offset: 0x0020EC04
		public PowersTab(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 706,
				rightNeighborID = -7777
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width - 32 - 60, this.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 707,
				leftNeighborID = -7777
			};
		}

		// Token: 0x06002B94 RID: 11156 RVA: 0x00210AF2 File Offset: 0x0020ECF2
		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B95 RID: 11157 RVA: 0x00210B10 File Offset: 0x0020ED10
		public override void populateClickableComponentList()
		{
			if (this.powers == null)
			{
				this.powers = new List<List<ClickableTextureComponent>>();
				Dictionary<string, PowersData> powersData = null;
				try
				{
					powersData = DataLoader.Powers(Game1.content);
				}
				catch (Exception)
				{
				}
				if (powersData != null)
				{
					int collectionWidth = 9;
					int widthUsed = 0;
					int baseX = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
					int baseY = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
					foreach (KeyValuePair<string, PowersData> power in powersData)
					{
						int xPos = baseX + widthUsed % collectionWidth * 76;
						int yPos = baseY + widthUsed / collectionWidth * 76;
						bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition, null, null, null, null, null, null);
						string name = TokenParser.ParseText(power.Value.DisplayName, null, null, null) ?? power.Key;
						string description = TokenParser.ParseText(power.Value.Description, null, null, null) ?? "";
						Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
						if (this.powers.Count == 0 || yPos > this.yPositionOnScreen + this.height - 128)
						{
							this.powers.Add(new List<ClickableTextureComponent>());
							widthUsed = 0;
							xPos = baseX;
							yPos = baseY;
						}
						List<ClickableTextureComponent> list = this.powers.Last<List<ClickableTextureComponent>>();
						list.Add(new ClickableTextureComponent(power.Key, new Rectangle(xPos, yPos, 64, 64), name, description, texture, new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f, unlocked)
						{
							myID = list.Count,
							rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? -1 : (list.Count + 1)),
							leftNeighborID = ((list.Count % collectionWidth == 0) ? -1 : (list.Count - 1)),
							downNeighborID = ((yPos + 76 > this.yPositionOnScreen + this.height - 128) ? -7777 : (list.Count + collectionWidth)),
							upNeighborID = ((list.Count < collectionWidth) ? 12346 : (list.Count - collectionWidth)),
							fullyImmutable = true,
							drawLabel = false
						});
						widthUsed++;
					}
				}
			}
			base.populateClickableComponentList();
		}

		// Token: 0x06002B96 RID: 11158 RVA: 0x00210DBC File Offset: 0x0020EFBC
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.descriptionText = "";
			base.performHoverAction(x, y);
			foreach (ClickableTextureComponent c in this.powers[this.currentPage])
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
					this.hoverText = (c.drawShadow ? c.label : "???");
					this.descriptionText = Game1.parseText(c.hoverText, Game1.smallFont, Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverText).X, 320));
				}
				else
				{
					c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
				}
			}
			this.forwardButton.tryHover(x, y, 0.5f);
			this.backButton.tryHover(x, y, 0.5f);
		}

		// Token: 0x06002B97 RID: 11159 RVA: 0x00210EF8 File Offset: 0x0020F0F8
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.backButton.containsPoint(x, y) && this.currentPage > 0)
			{
				if (playSound)
				{
					Game1.playSound("shwip", null);
				}
				this.currentPage--;
				return;
			}
			if (this.forwardButton.containsPoint(x, y) && this.currentPage < this.powers.Count - 1)
			{
				if (playSound)
				{
					Game1.playSound("shwip", null);
				}
				this.currentPage++;
				return;
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002B98 RID: 11160 RVA: 0x00210F98 File Offset: 0x0020F198
		public override void draw(SpriteBatch b)
		{
			if (this.currentPage > 0)
			{
				this.backButton.draw(b);
			}
			if (this.currentPage < this.powers.Count - 1)
			{
				this.forwardButton.draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			foreach (ClickableTextureComponent clickableTextureComponent in this.powers[this.currentPage])
			{
				clickableTextureComponent.draw(b, clickableTextureComponent.drawShadow ? Color.White : (Color.Black * 0.2f), 0.86f, 0, 0, 0);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (!this.descriptionText.Equals("") && this.hoverText != "???")
			{
				IClickableMenu.drawHoverText(b, this.descriptionText, Game1.smallFont, 0, 0, -1, this.hoverText, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				return;
			}
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x04001D33 RID: 7475
		public const int region_forwardButton = 707;

		// Token: 0x04001D34 RID: 7476
		public const int region_backButton = 706;

		// Token: 0x04001D35 RID: 7477
		public const int distanceFromMenuBottomBeforeNewPage = 128;

		// Token: 0x04001D36 RID: 7478
		public int currentPage;

		// Token: 0x04001D37 RID: 7479
		public string descriptionText = "";

		// Token: 0x04001D38 RID: 7480
		public string hoverText = "";

		// Token: 0x04001D39 RID: 7481
		public ClickableTextureComponent backButton;

		// Token: 0x04001D3A RID: 7482
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001D3B RID: 7483
		public List<List<ClickableTextureComponent>> powers;
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	// Token: 0x020002B0 RID: 688
	public class TailorRecipeListTool : IClickableMenu
	{
		// Token: 0x06002CEA RID: 11498 RVA: 0x0022E4BC File Offset: 0x0022C6BC
		public TailorRecipeListTool() : base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64, false)
		{
			TailoringMenu tailoring_menu = new TailoringMenu();
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			Item cloth = ItemRegistry.Create<Object>("(O)428", 1, 0, false);
			foreach (string itemId in ItemRegistry.GetObjectTypeDefinition().GetAllIds())
			{
				Object key = new Object(itemId, 1, false, -1, 0);
				if (!key.Name.Contains("Seeds") && !key.Name.Contains("Floor") && !key.Name.Equals("Lumber") && !key.Name.Contains("Fence") && !key.Name.Equals("Gate") && !key.Name.Contains("Starter") && !key.Name.Equals("Secret Note") && !key.Name.Contains("Guide") && !key.Name.Contains("Path") && !key.Name.Contains("Ring") && key.category.Value != -22 && key.Category != -999 && !key.isSapling())
				{
					Item value = tailoring_menu.CraftItem(cloth, key);
					TailorItemRecipe recipe = tailoring_menu.GetRecipeForItems(cloth, key);
					KeyValuePair<Item, Item> kvp = new KeyValuePair<Item, Item>(key, value);
					this._recipeLookup[Utility.getStandardDescriptionFromItem(key, 1, ' ')] = kvp;
					string metadata = "";
					Color? dye_color = TailoringMenu.GetDyeColor(key);
					if (dye_color != null)
					{
						this._recipeColors[Utility.getStandardDescriptionFromItem(key, 1, ' ')] = dye_color.Value;
					}
					if (recipe != null)
					{
						metadata = "clothes id: " + recipe.CraftedItemId + " from ";
						foreach (string context_tag in recipe.SecondItemTags)
						{
							metadata = metadata + context_tag + " ";
						}
						metadata.Trim();
					}
					this._recipeOutputIds[Utility.getStandardDescriptionFromItem(key, 1, ' ')] = (TailoringMenu.ConvertLegacyItemId((recipe != null) ? recipe.CraftedItemId : null) ?? value.QualifiedItemId);
					this._recipeHoverTexts[Utility.getStandardDescriptionFromItem(key, 1, ' ')] = metadata;
					ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, default(Rectangle), 1f, false);
					component.myID = 0;
					component.name = Utility.getStandardDescriptionFromItem(key, 1, ' ');
					component.label = key.DisplayName;
					this.recipeComponents.Add(component);
				}
			}
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.RepositionElements();
		}

		// Token: 0x06002CEB RID: 11499 RVA: 0x0022E910 File Offset: 0x0022CB10
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			this.RepositionElements();
		}

		// Token: 0x06002CEC RID: 11500 RVA: 0x0022E974 File Offset: 0x0022CB74
		private void RepositionElements()
		{
			this.scrollView = new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, this.width - IClickableMenu.borderWidth, 500);
			if (this.scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
			{
				int size_difference = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - this.scrollView.Left;
				this.scrollView.X = this.scrollView.X + size_difference;
				this.scrollView.Width = this.scrollView.Width - size_difference;
			}
			if (this.scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
			{
				int size_difference2 = this.scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
				this.scrollView.X = this.scrollView.X - size_difference2;
				this.scrollView.Width = this.scrollView.Width - size_difference2;
			}
			if (this.scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
			{
				int size_difference3 = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - this.scrollView.Top;
				this.scrollView.Y = this.scrollView.Y + size_difference3;
				this.scrollView.Width = this.scrollView.Width - size_difference3;
			}
			if (this.scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
			{
				int size_difference4 = this.scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
				this.scrollView.Y = this.scrollView.Y - size_difference4;
				this.scrollView.Width = this.scrollView.Width - size_difference4;
			}
			this.RepositionScrollElements();
		}

		// Token: 0x06002CED RID: 11501 RVA: 0x0022EB64 File Offset: 0x0022CD64
		public void RepositionScrollElements()
		{
			int y_offset = (int)this.scrollY;
			if (this.scrollY > 0f)
			{
				this.scrollY = 0f;
			}
			foreach (ClickableTextureComponent component in this.recipeComponents)
			{
				component.bounds.X = this.scrollView.X;
				component.bounds.Y = this.scrollView.Y + y_offset;
				y_offset += component.bounds.Height;
				if (this.scrollView.Intersects(component.bounds))
				{
					component.visible = true;
				}
				else
				{
					component.visible = false;
				}
			}
		}

		// Token: 0x06002CEE RID: 11502 RVA: 0x0022EC30 File Offset: 0x0022CE30
		public override void snapToDefaultClickableComponent()
		{
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002CEF RID: 11503 RVA: 0x0022EC38 File Offset: 0x0022CE38
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in this.recipeComponents)
			{
				if (component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
				{
					try
					{
						Item item = ItemRegistry.Create(this._recipeOutputIds[component.name], 1, 0, false);
						Clothing clothing = item as Clothing;
						Color color;
						if (clothing != null && this._recipeColors.TryGetValue(component.name, out color))
						{
							clothing.Dye(color, 1f);
						}
						Game1.player.addItemToInventoryBool(item, false);
					}
					catch (Exception)
					{
					}
				}
			}
			if (this.okButton.containsPoint(x, y))
			{
				base.exitThisMenu(true);
			}
		}

		// Token: 0x06002CF0 RID: 11504 RVA: 0x0022ED20 File Offset: 0x0022CF20
		public override void receiveKeyPress(Keys key)
		{
		}

		// Token: 0x06002CF1 RID: 11505 RVA: 0x0022ED22 File Offset: 0x0022CF22
		public override void receiveScrollWheelAction(int direction)
		{
			this.scrollY += (float)direction;
			this.RepositionScrollElements();
			base.receiveScrollWheelAction(direction);
		}

		// Token: 0x06002CF2 RID: 11506 RVA: 0x0022ED40 File Offset: 0x0022CF40
		public override void performHoverAction(int x, int y)
		{
			this.hoveredItem = null;
			this.hoverText = "";
			foreach (ClickableTextureComponent component in this.recipeComponents)
			{
				if (component.containsPoint(x, y))
				{
					this.hoveredItem = this._recipeLookup[component.name].Value;
					this.hoverText = this._recipeHoverTexts[component.name];
				}
			}
		}

		// Token: 0x06002CF3 RID: 11507 RVA: 0x0022EDE0 File Offset: 0x0022CFE0
		public bool canLeaveMenu()
		{
			return true;
		}

		// Token: 0x06002CF4 RID: 11508 RVA: 0x0022EDE4 File Offset: 0x0022CFE4
		public override void draw(SpriteBatch b)
		{
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			b.GraphicsDevice.ScissorRectangle = this.scrollView;
			foreach (ClickableTextureComponent component in this.recipeComponents)
			{
				if (component.visible)
				{
					base.drawHorizontalPartition(b, component.bounds.Bottom - 32, true, -1, -1, -1);
					KeyValuePair<Item, Item> kvp = this._recipeLookup[component.name];
					component.draw(b);
					kvp.Key.drawInMenu(b, new Vector2((float)component.bounds.X, (float)component.bounds.Y), 1f);
					Color color;
					if (this._recipeColors.TryGetValue(component.name, out color))
					{
						int size = 24;
						b.Draw(Game1.staminaRect, new Rectangle(this.scrollView.Left + this.scrollView.Width / 2 - size / 2, component.bounds.Center.Y - size / 2, size, size), color);
					}
					Item value = kvp.Value;
					if (value != null)
					{
						value.drawInMenu(b, new Vector2((float)(this.scrollView.Left + this.scrollView.Width - 128), (float)component.bounds.Y), 1f);
					}
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			this.okButton.draw(b);
			base.drawMouse(b, false, -1);
			if (this.hoveredItem != null)
			{
				Utility.drawTextWithShadow(b, this.hoverText, Game1.smallFont, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + this.height - 64)), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem, false, -1, 0, null, -1, null, -1, null);
				}
			}
		}

		// Token: 0x04001EA0 RID: 7840
		public Rectangle scrollView;

		// Token: 0x04001EA1 RID: 7841
		public List<ClickableTextureComponent> recipeComponents = new List<ClickableTextureComponent>();

		// Token: 0x04001EA2 RID: 7842
		public ClickableTextureComponent okButton;

		// Token: 0x04001EA3 RID: 7843
		public float scrollY;

		// Token: 0x04001EA4 RID: 7844
		public Dictionary<string, KeyValuePair<Item, Item>> _recipeLookup = new Dictionary<string, KeyValuePair<Item, Item>>();

		// Token: 0x04001EA5 RID: 7845
		public Item hoveredItem;

		// Token: 0x04001EA6 RID: 7846
		public string hoverText = "";

		// Token: 0x04001EA7 RID: 7847
		public Dictionary<string, string> _recipeHoverTexts = new Dictionary<string, string>();

		// Token: 0x04001EA8 RID: 7848
		public Dictionary<string, string> _recipeOutputIds = new Dictionary<string, string>();

		// Token: 0x04001EA9 RID: 7849
		public Dictionary<string, Color> _recipeColors = new Dictionary<string, Color>();
	}
}

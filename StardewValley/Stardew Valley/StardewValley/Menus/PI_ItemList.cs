using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus
{
	// Token: 0x0200029E RID: 670
	public class PI_ItemList : ProfileItem
	{
		// Token: 0x06002BC7 RID: 11207 RVA: 0x0021492B File Offset: 0x00212B2B
		public PI_ItemList(ProfileMenu context, string name, List<Item> values) : base(context, name)
		{
			this._items = values;
			this._components = new List<ClickableTextureComponent>();
			this._height = 0f;
			this._emptyBoxPositions = new List<Vector2>();
			this._UpdateIcons();
		}

		// Token: 0x06002BC8 RID: 11208 RVA: 0x00214963 File Offset: 0x00212B63
		public override void Unload()
		{
			base.Unload();
			this._ClearItems();
		}

		// Token: 0x06002BC9 RID: 11209 RVA: 0x00214974 File Offset: 0x00212B74
		protected void _ClearItems()
		{
			for (int i = 0; i < this._components.Count; i++)
			{
				this._context.UnregisterClickable(this._components[i]);
			}
			this._components.Clear();
		}

		// Token: 0x06002BCA RID: 11210 RVA: 0x002149BC File Offset: 0x00212BBC
		protected void _UpdateIcons()
		{
			this._ClearItems();
			Vector2 draw_position = new Vector2(0f, 0f);
			for (int i = 0; i < this._items.Count; i++)
			{
				Item item = this._items[i];
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
				ClickableTextureComponent component = new ClickableTextureComponent(item.DisplayName, new Rectangle((int)draw_position.X, (int)draw_position.Y, 32, 32), null, "", itemData.GetTexture(), itemData.GetSourceRect(0, null), 2f, false)
				{
					myID = 0,
					name = item.DisplayName,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					region = 502
				};
				this._components.Add(component);
				this._context.RegisterClickable(component);
			}
		}

		// Token: 0x06002BCB RID: 11211 RVA: 0x00214AC0 File Offset: 0x00212CC0
		public override float HandleLayout(float draw_y, Rectangle content_rectangle, int index)
		{
			this._emptyBoxPositions.Clear();
			draw_y = base.HandleLayout(draw_y, content_rectangle, index);
			int draw_x = 0;
			int lowest_drawn_position = (int)draw_y;
			Point padding = new Point(4, 4);
			for (int i = 0; i < this._components.Count; i++)
			{
				ClickableTextureComponent component = this._components[i];
				if (draw_x + component.bounds.Width + padding.Y > content_rectangle.Width)
				{
					draw_x = 0;
					draw_y += (float)(component.bounds.Height + padding.Y);
				}
				component.bounds.X = content_rectangle.Left + draw_x;
				component.bounds.Y = (int)draw_y;
				draw_x += component.bounds.Width + padding.X;
				lowest_drawn_position = Math.Max((int)draw_y + component.bounds.Height, lowest_drawn_position);
			}
			while (draw_x + 32 + padding.X <= content_rectangle.Width)
			{
				this._emptyBoxPositions.Add(new Vector2((float)(content_rectangle.Left + draw_x), draw_y));
				draw_x += 32 + padding.X;
			}
			return (float)(lowest_drawn_position + 8);
		}

		// Token: 0x06002BCC RID: 11212 RVA: 0x00214BE4 File Offset: 0x00212DE4
		public override void DrawItem(SpriteBatch b)
		{
			for (int i = 0; i < this._components.Count; i++)
			{
				ClickableTextureComponent component = this._components[i];
				b.Draw(Game1.menuTexture, new Rectangle(component.bounds.X, component.bounds.Y, 32, 32), new Rectangle?(new Rectangle(64, 128, 64, 64)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
				b.Draw(Game1.menuTexture, new Rectangle(component.bounds.X, component.bounds.Y, 32, 32), new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
				this._components[i].draw(b, Color.White, 4.1E-05f, 0, 0, 0);
				if (Game1.player.Items.ContainsId(this._items[i].ItemId))
				{
					b.Draw(Game1.mouseCursors, new Rectangle(this._components[i].bounds.X + 32 - 11, this._components[i].bounds.Y + 32 - 13, 11, 13), new Rectangle?(new Rectangle(268, 1436, 11, 13)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4E-05f);
				}
			}
			for (int j = 0; j < this._emptyBoxPositions.Count; j++)
			{
				b.Draw(Game1.menuTexture, new Rectangle((int)this._emptyBoxPositions[j].X, (int)this._emptyBoxPositions[j].Y, 32, 32), new Rectangle?(new Rectangle(64, 896, 64, 64)), Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
				b.Draw(Game1.menuTexture, new Rectangle((int)this._emptyBoxPositions[j].X, (int)this._emptyBoxPositions[j].Y, 32, 32), new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
			}
		}

		// Token: 0x06002BCD RID: 11213 RVA: 0x00214E6C File Offset: 0x0021306C
		public override void performHover(int x, int y)
		{
			for (int i = 0; i < this._components.Count; i++)
			{
				if (this._components[i].bounds.Contains(new Point(x, y)))
				{
					this._context.hoveredItem = this._items[i];
				}
			}
		}

		// Token: 0x06002BCE RID: 11214 RVA: 0x00214EC5 File Offset: 0x002130C5
		public override bool ShouldDraw()
		{
			return this._items.Count > 0;
		}

		// Token: 0x04001D81 RID: 7553
		protected List<Item> _items;

		// Token: 0x04001D82 RID: 7554
		protected List<ClickableTextureComponent> _components;

		// Token: 0x04001D83 RID: 7555
		protected float _height;

		// Token: 0x04001D84 RID: 7556
		protected List<Vector2> _emptyBoxPositions;
	}
}

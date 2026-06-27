using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Internal;

namespace StardewValley.Objects
{
	// Token: 0x020001B6 RID: 438
	public class Sign : Object
	{
		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06001F4E RID: 8014 RVA: 0x0016783F File Offset: 0x00165A3F
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001F4F RID: 8015 RVA: 0x00167846 File Offset: 0x00165A46
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.displayItem, "displayItem").AddField(this.displayType, "displayType");
		}

		// Token: 0x06001F50 RID: 8016 RVA: 0x00167875 File Offset: 0x00165A75
		public Sign()
		{
		}

		// Token: 0x06001F51 RID: 8017 RVA: 0x00167893 File Offset: 0x00165A93
		public Sign(Vector2 tile, string itemId) : base(tile, itemId, false)
		{
		}

		// Token: 0x06001F52 RID: 8018 RVA: 0x001678B4 File Offset: 0x00165AB4
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return who.CurrentItem != null;
			}
			Item dropIn = who.CurrentItem;
			if (dropIn != null)
			{
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				this.displayItem.Value = dropIn.getOne();
				Game1.playSound("coin", null);
				this.displayType.Value = 1;
				Item value = this.displayItem.Value;
				if (!(value is Hat))
				{
					if (!(value is Ring))
					{
						if (!(value is Furniture))
						{
							Object obj = value as Object;
							if (obj != null)
							{
								this.displayType.Value = (obj.bigCraftable.Value ? 3 : 1);
							}
						}
						else
						{
							this.displayType.Value = 5;
						}
					}
					else
					{
						this.displayType.Value = 4;
					}
				}
				else
				{
					this.displayType.Value = 2;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001F53 RID: 8019 RVA: 0x00167994 File Offset: 0x00165B94
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (this.displayItem.Value != null)
			{
				switch (this.displayType.Value)
				{
				case 1:
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, (float)(y * 64 - 64 + 21 + 8 - 2))), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, false);
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, (float)(y * 64 - 64 + 21 + 4 - 1))), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, false);
					return;
				case 2:
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, (float)(y * 64 - 64 + 21 + 8 - 1))), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, false);
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, (float)(y * 64 - 64 + 21 + 4 - 1))), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, false);
					return;
				case 3:
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64 + 21 + 4 - 1))), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.White, false);
					return;
				case 4:
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) - 1f, (float)(y * 64 - 64 + 21 + 8 - 1))), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, false);
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) - 1f, (float)(y * 64 - 64 + 21 + 4 - 1))), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, false);
					return;
				case 5:
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64 + 21 + 8 - 1))), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, false);
					this.displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64 + 21 + 4 - 1))), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, false);
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06001F54 RID: 8020 RVA: 0x00167DEB File Offset: 0x00165FEB
		public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
		{
			return base.ForEachItem(handler, getPath) && ForEachItemHelper.ApplyToField<Item>(this.displayItem, handler, getPath, null);
		}

		// Token: 0x04001347 RID: 4935
		public const int OBJECT = 1;

		// Token: 0x04001348 RID: 4936
		public const int HAT = 2;

		// Token: 0x04001349 RID: 4937
		public const int BIG_OBJECT = 3;

		// Token: 0x0400134A RID: 4938
		public const int RING = 4;

		// Token: 0x0400134B RID: 4939
		public const int FURNITURE = 5;

		// Token: 0x0400134C RID: 4940
		[XmlElement("displayItem")]
		public readonly NetRef<Item> displayItem = new NetRef<Item>();

		// Token: 0x0400134D RID: 4941
		[XmlElement("displayType")]
		public readonly NetInt displayType = new NetInt();
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley
{
	// Token: 0x020000F5 RID: 245
	public class PurchaseableKeyItem : ISalable, IHaveItemTypeId
	{
		// Token: 0x17000237 RID: 567
		// (get) Token: 0x0600140E RID: 5134 RVA: 0x000F46AC File Offset: 0x000F28AC
		public string TypeDefinitionId
		{
			get
			{
				return "(Salable)";
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x0600140F RID: 5135 RVA: 0x000F46B4 File Offset: 0x000F28B4
		public string QualifiedItemId
		{
			get
			{
				return this.TypeDefinitionId + "PurchaseableKeyItem." + this.id.ToString();
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06001410 RID: 5136 RVA: 0x000F46DF File Offset: 0x000F28DF
		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06001411 RID: 5137 RVA: 0x000F46E7 File Offset: 0x000F28E7
		public int id
		{
			get
			{
				return this._id;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06001412 RID: 5138 RVA: 0x000F46EF File Offset: 0x000F28EF
		public List<string> tags
		{
			get
			{
				return this._tags;
			}
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06001413 RID: 5139 RVA: 0x000F46F7 File Offset: 0x000F28F7
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06001414 RID: 5140 RVA: 0x000F46FF File Offset: 0x000F28FF
		// (set) Token: 0x06001415 RID: 5141 RVA: 0x000F4702 File Offset: 0x000F2902
		public bool IsRecipe
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06001416 RID: 5142 RVA: 0x000F4704 File Offset: 0x000F2904
		// (set) Token: 0x06001417 RID: 5143 RVA: 0x000F4707 File Offset: 0x000F2907
		public int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06001418 RID: 5144 RVA: 0x000F4709 File Offset: 0x000F2909
		// (set) Token: 0x06001419 RID: 5145 RVA: 0x000F470C File Offset: 0x000F290C
		public int Quality
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x0600141A RID: 5146 RVA: 0x000F4710 File Offset: 0x000F2910
		public PurchaseableKeyItem(string display_name, string display_description, int parent_sheet_index, Action<Farmer> on_purchase = null)
		{
			this._id = parent_sheet_index;
			this._name = display_name;
			this._displayName = display_name;
			this._description = display_description;
			this._onPurchase = on_purchase;
		}

		// Token: 0x0600141B RID: 5147 RVA: 0x000F4768 File Offset: 0x000F2968
		public string GetItemTypeId()
		{
			return this.TypeDefinitionId;
		}

		// Token: 0x0600141C RID: 5148 RVA: 0x000F4770 File Offset: 0x000F2970
		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((float)((int)(32f * scaleSize)), (float)((int)(32f * scaleSize))), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this._id, 16, 16)), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
		}

		// Token: 0x0600141D RID: 5149 RVA: 0x000F47EB File Offset: 0x000F29EB
		public bool ShouldDrawIcon()
		{
			return true;
		}

		// Token: 0x0600141E RID: 5150 RVA: 0x000F47EE File Offset: 0x000F29EE
		public string getDescription()
		{
			return this._description;
		}

		// Token: 0x0600141F RID: 5151 RVA: 0x000F47F6 File Offset: 0x000F29F6
		public int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001420 RID: 5152 RVA: 0x000F47F9 File Offset: 0x000F29F9
		public int addToStack(Item stack)
		{
			return 1;
		}

		// Token: 0x06001421 RID: 5153 RVA: 0x000F47FC File Offset: 0x000F29FC
		public bool canStackWith(ISalable other)
		{
			return false;
		}

		// Token: 0x06001422 RID: 5154 RVA: 0x000F47FF File Offset: 0x000F29FF
		public int sellToStorePrice(long specificPlayerID = -1L)
		{
			return -1;
		}

		// Token: 0x06001423 RID: 5155 RVA: 0x000F4802 File Offset: 0x000F2A02
		public int salePrice(bool ignoreProfitMargins = false)
		{
			return this._price;
		}

		// Token: 0x06001424 RID: 5156 RVA: 0x000F480A File Offset: 0x000F2A0A
		public bool appliesProfitMargins()
		{
			return false;
		}

		// Token: 0x06001425 RID: 5157 RVA: 0x000F480D File Offset: 0x000F2A0D
		public bool actionWhenPurchased(string shopId)
		{
			Action<Farmer> onPurchase = this._onPurchase;
			if (onPurchase != null)
			{
				onPurchase(Game1.player);
			}
			return true;
		}

		// Token: 0x06001426 RID: 5158 RVA: 0x000F4826 File Offset: 0x000F2A26
		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		// Token: 0x06001427 RID: 5159 RVA: 0x000F4829 File Offset: 0x000F2A29
		public bool IsInfiniteStock()
		{
			return false;
		}

		// Token: 0x06001428 RID: 5160 RVA: 0x000F482C File Offset: 0x000F2A2C
		public ISalable GetSalableInstance()
		{
			return this;
		}

		// Token: 0x06001429 RID: 5161 RVA: 0x000F482F File Offset: 0x000F2A2F
		public void FixStackSize()
		{
		}

		// Token: 0x0600142A RID: 5162 RVA: 0x000F4831 File Offset: 0x000F2A31
		public void FixQuality()
		{
		}

		// Token: 0x04000C9B RID: 3227
		protected string _displayName = "";

		// Token: 0x04000C9C RID: 3228
		protected string _name = "";

		// Token: 0x04000C9D RID: 3229
		protected string _description = "";

		// Token: 0x04000C9E RID: 3230
		protected int _price;

		// Token: 0x04000C9F RID: 3231
		protected int _id;

		// Token: 0x04000CA0 RID: 3232
		protected List<string> _tags;

		// Token: 0x04000CA1 RID: 3233
		protected Action<Farmer> _onPurchase;
	}
}

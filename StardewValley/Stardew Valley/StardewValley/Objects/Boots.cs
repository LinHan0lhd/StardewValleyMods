using System;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects
{
	// Token: 0x020001A2 RID: 418
	public class Boots : Item
	{
		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06001D94 RID: 7572 RVA: 0x00152159 File Offset: 0x00150359
		public override string TypeDefinitionId { get; } = "(B)";

		// Token: 0x06001D95 RID: 7573 RVA: 0x00152164 File Offset: 0x00150364
		public Boots()
		{
			base.Category = -97;
		}

		// Token: 0x06001D96 RID: 7574 RVA: 0x001521CC File Offset: 0x001503CC
		public Boots(string itemId) : this()
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			base.ItemId = itemId;
			this.reloadData();
			base.Category = -97;
		}

		// Token: 0x06001D97 RID: 7575 RVA: 0x001521F4 File Offset: 0x001503F4
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = this.indexInTileSheet.Value.ToString();
		}

		// Token: 0x06001D98 RID: 7576 RVA: 0x0015221C File Offset: 0x0015041C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.defenseBonus, "defenseBonus").AddField(this.immunityBonus, "immunityBonus").AddField(this.indexInTileSheet, "indexInTileSheet").AddField(this.price, "price").AddField(this.indexInColorSheet, "indexInColorSheet").AddField(this.appliedBootSheetIndex, "appliedBootSheetIndex");
		}

		// Token: 0x06001D99 RID: 7577 RVA: 0x00152298 File Offset: 0x00150498
		public virtual void reloadData()
		{
			ParsedItemData parsedData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			string[] data = DataLoader.Boots(Game1.content)[base.ItemId].Split('/', StringSplitOptions.None);
			this.Name = (ArgUtility.Get(data, 0, null, false) ?? parsedData.InternalName);
			this.price.Value = Convert.ToInt32(data[2]);
			this.defenseBonus.Value = Convert.ToInt32(data[3]);
			this.immunityBonus.Value = Convert.ToInt32(data[4]);
			this.indexInColorSheet.Value = Convert.ToInt32(data[5]);
			this.indexInTileSheet.Value = parsedData.SpriteIndex;
		}

		// Token: 0x06001D9A RID: 7578 RVA: 0x00152348 File Offset: 0x00150548
		public void applyStats(Boots applied_boots)
		{
			this.reloadData();
			if (this.defenseBonus.Value == applied_boots.defenseBonus.Value && this.immunityBonus.Value == applied_boots.immunityBonus.Value)
			{
				this.appliedBootSheetIndex.Value = null;
			}
			else
			{
				this.appliedBootSheetIndex.Value = applied_boots.getStatsIndex();
			}
			this.defenseBonus.Value = applied_boots.defenseBonus.Value;
			this.immunityBonus.Value = applied_boots.immunityBonus.Value;
			this.price.Value = applied_boots.price.Value;
			this.loadDisplayFields();
		}

		// Token: 0x06001D9B RID: 7579 RVA: 0x001523F3 File Offset: 0x001505F3
		public virtual string getStatsIndex()
		{
			return this.appliedBootSheetIndex.Value ?? base.ItemId;
		}

		// Token: 0x06001D9C RID: 7580 RVA: 0x0015240A File Offset: 0x0015060A
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			return this.defenseBonus.Value * 100 + this.immunityBonus.Value * 100;
		}

		// Token: 0x06001D9D RID: 7581 RVA: 0x00152429 File Offset: 0x00150629
		public override void onEquip(Farmer who)
		{
			base.onEquip(who);
			who.changeShoeColor(this.GetBootsColorString());
		}

		// Token: 0x06001D9E RID: 7582 RVA: 0x0015243E File Offset: 0x0015063E
		public override void onUnequip(Farmer who)
		{
			base.onUnequip(who);
			who.changeShoeColor("12");
		}

		// Token: 0x06001D9F RID: 7583 RVA: 0x00152454 File Offset: 0x00150654
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.Defense.Value += (float)this.defenseBonus.Value;
			effects.Immunity.Value += (float)this.immunityBonus.Value;
		}

		// Token: 0x06001DA0 RID: 7584 RVA: 0x001524A4 File Offset: 0x001506A4
		public string GetBootsColorString()
		{
			string rawData;
			if (DataLoader.Boots(Game1.content).TryGetValue(base.ItemId, out rawData))
			{
				string[] split = rawData.Split('/', StringSplitOptions.None);
				if (split.Length > 7 && split[7] != "")
				{
					return split[7] + ":" + this.indexInColorSheet.Value.ToString();
				}
			}
			return this.indexInColorSheet.Value.ToString();
		}

		// Token: 0x06001DA1 RID: 7585 RVA: 0x0015251D File Offset: 0x0015071D
		public int getNumberOfDescriptionCategories()
		{
			if (this.immunityBonus.Value > 0 && this.defenseBonus.Value > 0)
			{
				return 2;
			}
			return 1;
		}

		// Token: 0x06001DA2 RID: 7586 RVA: 0x00152540 File Offset: 0x00150740
		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			y += (int)font.MeasureString(Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth())).Y;
			if (this.defenseBonus.Value > 0)
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", this.defenseBonus), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
			if (this.immunityBonus.Value > 0)
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", this.immunityBonus), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}

		// Token: 0x06001DA3 RID: 7587 RVA: 0x00152780 File Offset: 0x00150980
		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			int maxStat = 9999;
			Point dimensions = new Point(0, startingHeight);
			dimensions.Y -= (int)font.MeasureString(descriptionText).Y;
			dimensions.Y += (int)((float)(this.getNumberOfDescriptionCategories() * 4 * 12) + font.MeasureString(Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth())).Y);
			dimensions.X = (int)Math.Max((float)minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", maxStat)).X + (float)horizontalBuffer));
			return dimensions;
		}

		// Token: 0x06001DA4 RID: 7588 RVA: 0x0015284C File Offset: 0x00150A4C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(data.GetTexture(), location + new Vector2(32f, 32f) * scaleSize, new Rectangle?(data.GetSourceRect(0, null)), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001DA5 RID: 7589 RVA: 0x001528E9 File Offset: 0x00150AE9
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001DA6 RID: 7590 RVA: 0x001528EC File Offset: 0x00150AEC
		public override string getCategoryName()
		{
			return Object.GetCategoryDisplayName(-97);
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x001528F8 File Offset: 0x00150AF8
		public override string getDescription()
		{
			if (this.description == null)
			{
				this.loadDisplayFields();
			}
			return Game1.parseText(this.description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12500", this.immunityBonus.Value + this.defenseBonus.Value), Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x00152964 File Offset: 0x00150B64
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06001DA9 RID: 7593 RVA: 0x00152967 File Offset: 0x00150B67
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (this.displayName == null)
				{
					this.loadDisplayFields();
				}
				return this.displayName;
			}
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x0015297E File Offset: 0x00150B7E
		protected override Item GetOneNew()
		{
			return new Boots(base.ItemId);
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x0015298C File Offset: 0x00150B8C
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Boots fromBoots = source as Boots;
			if (fromBoots != null)
			{
				this.appliedBootSheetIndex.Value = fromBoots.appliedBootSheetIndex.Value;
				this.indexInColorSheet.Value = fromBoots.indexInColorSheet.Value;
				this.defenseBonus.Value = fromBoots.defenseBonus.Value;
				this.immunityBonus.Value = fromBoots.immunityBonus.Value;
				this.loadDisplayFields();
			}
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x00152A0C File Offset: 0x00150C0C
		protected virtual bool loadDisplayFields()
		{
			string rawData;
			if (DataLoader.Boots(Game1.content).TryGetValue(base.ItemId, out rawData))
			{
				string[] data = rawData.Split('/', StringSplitOptions.None);
				this.displayName = this.Name;
				if (data.Length > 6)
				{
					this.displayName = data[6];
				}
				if (this.appliedBootSheetIndex.Value != null)
				{
					this.displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CustomizedBootItemName", this.DisplayName);
				}
				this.description = data[1];
				return true;
			}
			return false;
		}

		// Token: 0x04001243 RID: 4675
		[XmlElement("defenseBonus")]
		public readonly NetInt defenseBonus = new NetInt();

		// Token: 0x04001244 RID: 4676
		[XmlElement("immunityBonus")]
		public readonly NetInt immunityBonus = new NetInt();

		// Token: 0x04001245 RID: 4677
		[XmlElement("indexInTileSheet")]
		public readonly NetInt indexInTileSheet = new NetInt();

		// Token: 0x04001246 RID: 4678
		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		// Token: 0x04001247 RID: 4679
		[XmlElement("indexInColorSheet")]
		public readonly NetInt indexInColorSheet = new NetInt();

		// Token: 0x04001248 RID: 4680
		[XmlElement("appliedBootSheetIndex")]
		public readonly NetString appliedBootSheetIndex = new NetString();

		// Token: 0x04001249 RID: 4681
		[XmlIgnore]
		public string displayName;

		// Token: 0x0400124A RID: 4682
		[XmlIgnore]
		public string description;
	}
}

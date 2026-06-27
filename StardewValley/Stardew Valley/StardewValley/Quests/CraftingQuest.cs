using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x02000188 RID: 392
	public class CraftingQuest : Quest
	{
		// Token: 0x06001C89 RID: 7305 RVA: 0x00146C42 File Offset: 0x00144E42
		public CraftingQuest()
		{
		}

		// Token: 0x06001C8A RID: 7306 RVA: 0x00146C55 File Offset: 0x00144E55
		public CraftingQuest(string itemId)
		{
			this.ItemId.Value = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
		}

		// Token: 0x06001C8B RID: 7307 RVA: 0x00146C7E File Offset: 0x00144E7E
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.ItemId, "ItemId");
		}

		// Token: 0x06001C8C RID: 7308 RVA: 0x00146CA0 File Offset: 0x00144EA0
		public override bool OnRecipeCrafted(CraftingRecipe recipe, Item item, bool probe = false)
		{
			bool baseChanged = base.OnRecipeCrafted(recipe, item, probe);
			if (item.QualifiedItemId == this.ItemId.Value)
			{
				if (!probe)
				{
					this.questComplete();
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x0400116E RID: 4462
		[XmlElement("isBigCraftable")]
		public bool? obsolete_isBigCraftable;

		// Token: 0x0400116F RID: 4463
		[XmlElement("indexToCraft")]
		public readonly NetString ItemId = new NetString();
	}
}

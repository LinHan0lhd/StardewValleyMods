using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x02000190 RID: 400
	public class ItemHarvestQuest : Quest
	{
		// Token: 0x06001CB3 RID: 7347 RVA: 0x00149CD7 File Offset: 0x00147ED7
		public ItemHarvestQuest()
		{
		}

		// Token: 0x06001CB4 RID: 7348 RVA: 0x00149CF8 File Offset: 0x00147EF8
		public ItemHarvestQuest(string itemId, int number = 1)
		{
			this.ItemId.Value = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
			this.Number.Value = number;
			this.questType.Value = 9;
		}

		// Token: 0x06001CB5 RID: 7349 RVA: 0x00149D50 File Offset: 0x00147F50
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.ItemId, "ItemId").AddField(this.Number, "Number");
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x00149D80 File Offset: 0x00147F80
		public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
		{
			bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
			if (!this.completed.Value && (item.QualifiedItemId == this.ItemId.Value || (this.ItemId.Value.StartsWith('-') && item.Category.ToString() == this.ItemId.Value)))
			{
				int newNumber = this.Number.Value - numberAdded;
				bool complete = newNumber <= 0;
				if (!probe)
				{
					this.Number.Value = newNumber;
					if (complete)
					{
						this.questComplete();
					}
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x04001186 RID: 4486
		[XmlElement("itemIndex")]
		public readonly NetString ItemId = new NetString();

		// Token: 0x04001187 RID: 4487
		[XmlElement("number")]
		public readonly NetInt Number = new NetInt();
	}
}

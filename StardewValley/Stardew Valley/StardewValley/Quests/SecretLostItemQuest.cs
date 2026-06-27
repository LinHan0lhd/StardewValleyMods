using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x02000195 RID: 405
	public class SecretLostItemQuest : Quest
	{
		// Token: 0x06001D05 RID: 7429 RVA: 0x0014C18C File Offset: 0x0014A38C
		public SecretLostItemQuest()
		{
		}

		// Token: 0x06001D06 RID: 7430 RVA: 0x0014C1CC File Offset: 0x0014A3CC
		public SecretLostItemQuest(string npcName, string itemId, int friendshipReward, string exclusiveQuestId)
		{
			this.npcName.Value = npcName;
			this.ItemId.Value = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
			this.friendshipReward.Value = friendshipReward;
			this.exclusiveQuestId.Value = exclusiveQuestId;
			this.questType.Value = 9;
		}

		// Token: 0x06001D07 RID: 7431 RVA: 0x0014C260 File Offset: 0x0014A460
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.npcName, "npcName").AddField(this.friendshipReward, "friendshipReward").AddField(this.exclusiveQuestId, "exclusiveQuestId").AddField(this.ItemId, "ItemId").AddField(this.itemFound, "itemFound");
		}

		// Token: 0x06001D08 RID: 7432 RVA: 0x0014C2CA File Offset: 0x0014A4CA
		public override bool isSecretQuest()
		{
			return true;
		}

		// Token: 0x06001D09 RID: 7433 RVA: 0x0014C2D0 File Offset: 0x0014A4D0
		public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
		{
			bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
			if (!this.completed.Value && !this.itemFound.Value && ((item != null) ? item.QualifiedItemId : null) == this.ItemId.Value)
			{
				if (!probe)
				{
					this.itemFound.Value = true;
					Game1.playSound("jingle1", null);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001D0A RID: 7434 RVA: 0x0014C348 File Offset: 0x0014A548
		public override bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			bool baseChanged = base.OnNpcSocialized(npc, probe);
			if (!this.completed.Value && this.itemFound.Value && npc.IsVillager && npc.Name == this.npcName.Value && Game1.player.Items.ContainsId(this.ItemId.Value))
			{
				if (!probe)
				{
					this.questComplete();
					string[] fields = Quest.GetRawQuestFields(this.id.Value);
					Dialogue thankYou = new Dialogue(npc, null, ArgUtility.Get(fields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", false));
					npc.setNewDialogue(thankYou, false, false);
					Game1.drawDialogue(npc);
					Game1.player.changeFriendship(this.friendshipReward.Value, npc);
					Game1.player.removeFirstOfThisItemFromInventory(this.ItemId.Value, 1);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001D0B RID: 7435 RVA: 0x0014C430 File Offset: 0x0014A630
		public override void questComplete()
		{
			if (!this.completed.Value)
			{
				this.completed.Value = true;
				Game1.player.questLog.Remove(this);
				foreach (Quest q in Game1.player.questLog)
				{
					if (q != null && q.id.Value == this.exclusiveQuestId.Value)
					{
						q.destroy.Value = true;
					}
				}
				Game1.playSound("questcomplete", null);
			}
		}

		// Token: 0x040011B8 RID: 4536
		[XmlElement("npcName")]
		public readonly NetString npcName = new NetString();

		// Token: 0x040011B9 RID: 4537
		[XmlElement("friendshipReward")]
		public readonly NetInt friendshipReward = new NetInt();

		// Token: 0x040011BA RID: 4538
		[XmlElement("exclusiveQuestId")]
		public readonly NetString exclusiveQuestId = new NetString();

		// Token: 0x040011BB RID: 4539
		[XmlElement("itemIndex")]
		public readonly NetString ItemId = new NetString();

		// Token: 0x040011BC RID: 4540
		[XmlElement("itemFound")]
		public readonly NetBool itemFound = new NetBool();
	}
}

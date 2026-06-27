using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests
{
	// Token: 0x02000191 RID: 401
	public class LostItemQuest : Quest
	{
		// Token: 0x06001CB7 RID: 7351 RVA: 0x00149E24 File Offset: 0x00148024
		public LostItemQuest()
		{
		}

		// Token: 0x06001CB8 RID: 7352 RVA: 0x00149E84 File Offset: 0x00148084
		public LostItemQuest(string npcName, string locationOfItem, string itemId, int tileX, int tileY)
		{
			this.npcName.Value = npcName;
			this.locationOfItem.Value = locationOfItem;
			this.ItemId.Value = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
			this.tileX.Value = tileX;
			this.tileY.Value = tileY;
			this.questType.Value = 9;
			if (!ItemRegistry.GetDataOrErrorItem(this.ItemId.Value).HasTypeObject())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't create ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler.AppendLiteral(" #");
				defaultInterpolatedStringHandler.AppendFormatted(this.id.Value);
				defaultInterpolatedStringHandler.AppendLiteral(" because the lost item (");
				defaultInterpolatedStringHandler.AppendFormatted(this.ItemId.Value);
				defaultInterpolatedStringHandler.AppendLiteral(") isn't an object-type item.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x00149FD0 File Offset: 0x001481D0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.objective, "objective").AddField(this.npcName, "npcName").AddField(this.locationOfItem, "locationOfItem").AddField(this.ItemId, "ItemId").AddField(this.tileX, "tileX").AddField(this.tileY, "tileY").AddField(this.itemFound, "itemFound");
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x0014A05C File Offset: 0x0014825C
		public override bool OnWarped(GameLocation location, bool probe = false)
		{
			bool baseChanged = base.OnWarped(location, probe);
			if (!this.itemFound.Value && location.name.Equals(this.locationOfItem.Value))
			{
				Vector2 position = new Vector2((float)this.tileX.Value, (float)this.tileY.Value);
				location.overlayObjects.Remove(position);
				Object o = ItemRegistry.Create<Object>(this.ItemId.Value, 1, 0, false);
				o.TileLocation = position;
				o.questItem.Value = true;
				o.questId.Value = this.id.Value;
				o.IsSpawnedObject = true;
				location.overlayObjects.Add(position, o);
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x0014A11A File Offset: 0x0014831A
		public new void reloadObjective()
		{
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x0014A140 File Offset: 0x00148340
		public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
		{
			bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
			if (!this.completed.Value && !this.itemFound.Value && item != null && item.QualifiedItemId == this.ItemId.Value)
			{
				if (!probe)
				{
					this.itemFound.Value = true;
					string npcDisplayName = this.npcName.Value;
					NPC namedNpc = Game1.getCharacterFromName(this.npcName.Value, true, false);
					if (namedNpc != null)
					{
						npcDisplayName = namedNpc.displayName;
					}
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Quests:MessageFoundLostItem", item.DisplayName, npcDisplayName));
					this.objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", new object[]
					{
						namedNpc
					});
					Game1.playSound("jingle1", null);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x0014A230 File Offset: 0x00148430
		public override bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			bool baseChanged = base.OnNpcSocialized(npc, probe);
			if (!this.completed.Value && this.itemFound.Value && npc.Name == this.npcName.Value && npc.IsVillager && Game1.player.Items.ContainsId(this.ItemId.Value))
			{
				if (!probe)
				{
					this.questComplete();
					string[] fields = Quest.GetRawQuestFields(this.id.Value);
					Dialogue thankYou = new Dialogue(npc, null, ArgUtility.Get(fields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", false));
					npc.setNewDialogue(thankYou, false, false);
					Game1.drawDialogue(npc);
					Game1.player.changeFriendship(250, npc);
					Game1.player.removeFirstOfThisItemFromInventory(this.ItemId.Value, 1);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x04001188 RID: 4488
		[XmlElement("npcName")]
		public readonly NetString npcName = new NetString();

		// Token: 0x04001189 RID: 4489
		[XmlElement("locationOfItem")]
		public readonly NetString locationOfItem = new NetString();

		// Token: 0x0400118A RID: 4490
		[XmlElement("itemIndex")]
		public readonly NetString ItemId = new NetString();

		// Token: 0x0400118B RID: 4491
		[XmlElement("tileX")]
		public readonly NetInt tileX = new NetInt();

		// Token: 0x0400118C RID: 4492
		[XmlElement("tileY")]
		public readonly NetInt tileY = new NetInt();

		// Token: 0x0400118D RID: 4493
		[XmlElement("itemFound")]
		public readonly NetBool itemFound = new NetBool();

		// Token: 0x0400118E RID: 4494
		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();
	}
}

using System;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests
{
	// Token: 0x02000194 RID: 404
	public class ResourceCollectionQuest : Quest
	{
		// Token: 0x06001CFE RID: 7422 RVA: 0x0014B534 File Offset: 0x00149734
		public ResourceCollectionQuest()
		{
			this.questType.Value = 10;
		}

		// Token: 0x06001CFF RID: 7423 RVA: 0x0014B5B8 File Offset: 0x001497B8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective").AddField(this.target, "target").AddField(this.targetMessage, "targetMessage").AddField(this.numberCollected, "numberCollected").AddField(this.number, "number").AddField(this.reward, "reward").AddField(this.ItemId, "ItemId");
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x0014B664 File Offset: 0x00149864
		public void loadQuestInfo()
		{
			if (this.target.Value != null)
			{
				return;
			}
			if (Game1.gameMode != 6)
			{
				Random random = base.CreateInitializationRandom();
				base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
				int randomResource = random.Next(6) * 2;
				for (int i = 0; i < random.Next(1, 100); i++)
				{
					random.Next();
				}
				int highest_mining_level = 0;
				int highest_foraging_level = 0;
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					highest_mining_level = Math.Max(highest_mining_level, farmer.MiningLevel);
				}
				foreach (Farmer farmer2 in Game1.getAllFarmers())
				{
					highest_foraging_level = Math.Max(highest_foraging_level, farmer2.ForagingLevel);
				}
				switch (randomResource)
				{
				case 0:
					this.ItemId.Value = "(O)378";
					this.number.Value = 20 + highest_mining_level * 2 + random.Next(-2, 4) * 2;
					this.reward.Value = this.number.Value * 10;
					this.number.Value = this.number.Value - this.number.Value % 5;
					this.target.Value = "Clint";
					goto IL_457;
				case 2:
					this.ItemId.Value = "(O)380";
					this.number.Value = 15 + highest_mining_level + random.Next(-1, 3) * 2;
					this.reward.Value = this.number.Value * 15;
					this.number.Value = (int)((float)this.number.Value * 0.75f);
					this.number.Value = this.number.Value - this.number.Value % 5;
					this.target.Value = "Clint";
					goto IL_457;
				case 4:
					this.ItemId.Value = "(O)382";
					this.number.Value = 10 + highest_mining_level + random.Next(-1, 3) * 2;
					this.reward.Value = this.number.Value * 25;
					this.number.Value = (int)((float)this.number.Value * 0.75f);
					this.number.Value = this.number.Value - this.number.Value % 5;
					this.target.Value = "Clint";
					goto IL_457;
				case 6:
					this.ItemId.Value = ((Utility.GetAllPlayerDeepestMineLevel() > 40) ? "(O)384" : "(O)378");
					this.number.Value = 8 + highest_mining_level / 2 + random.Next(-1, 1) * 2;
					this.reward.Value = this.number.Value * 30;
					this.number.Value = (int)((float)this.number.Value * 0.75f);
					this.number.Value = this.number.Value - this.number.Value % 2;
					this.target.Value = "Clint";
					goto IL_457;
				case 8:
					this.ItemId.Value = "(O)388";
					this.number.Value = 25 + highest_foraging_level + random.Next(-3, 3) * 2;
					this.number.Value = this.number.Value - this.number.Value % 5;
					this.reward.Value = this.number.Value * 8;
					this.target.Value = "Robin";
					goto IL_457;
				}
				this.ItemId.Value = "(O)390";
				this.number.Value = 25 + highest_mining_level + random.Next(-3, 3) * 2;
				this.number.Value = this.number.Value - this.number.Value % 5;
				this.reward.Value = this.number.Value * 8;
				this.target.Value = "Robin";
				IL_457:
				if (this.target.Value == null)
				{
					return;
				}
				Item item = ItemRegistry.Create(this.ItemId.Value, 1, 0, false);
				if (this.ItemId.Value != "(O)388" && this.ItemId.Value != "(O)390")
				{
					this.parts.Clear();
					int rand = random.Next(4);
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13647", new object[]
					{
						this.number.Value,
						item,
						new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + (new string[]
						{
							"13649",
							"13650",
							"13651",
							"13652"
						})[rand], Array.Empty<object>())
					}));
					if (rand == 3)
					{
						this.dialogueparts.Clear();
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13655");
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13656", "13657", "13658"));
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13659");
					}
					else
					{
						this.dialogueparts.Clear();
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13662");
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13656", "13657", "13658"));
						this.dialogueparts.Add(random.NextBool() ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13667", new object[]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13668", "13669", "13670"), Array.Empty<object>())
						}) : new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13672", Array.Empty<object>()));
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13673");
					}
				}
				else
				{
					this.parts.Clear();
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13674", new object[]
					{
						this.number.Value,
						item
					}));
					this.dialogueparts.Clear();
					this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13677", new object[]
					{
						(this.ItemId.Value == "(O)388") ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13678", Array.Empty<object>()) : new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13679", Array.Empty<object>())
					}));
					this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13681", "13682", "13683"));
				}
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", new object[]
				{
					this.reward.Value
				}));
				this.parts.Add(this.target.Value.Equals("Clint") ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13688" : "");
				this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", new object[]
				{
					"0",
					this.number.Value,
					item
				});
			}
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x0014BE3C File Offset: 0x0014A03C
		public override void reloadDescription()
		{
			if (this._questDescription == "")
			{
				this.loadQuestInfo();
			}
			if (this.parts.Count == 0 || this.parts == null || this.dialogueparts.Count == 0 || this.dialogueparts == null)
			{
				return;
			}
			string descriptionBuilder = "";
			string messageBuilder = "";
			foreach (DescriptionElement a in this.parts)
			{
				descriptionBuilder += a.loadDescriptionElement();
			}
			foreach (DescriptionElement b in this.dialogueparts)
			{
				messageBuilder += b.loadDescriptionElement();
			}
			base.questDescription = descriptionBuilder;
			this.targetMessage.Value = messageBuilder;
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x0014BF44 File Offset: 0x0014A144
		public override void reloadObjective()
		{
			if (this.numberCollected.Value < this.number.Value)
			{
				Item item = ItemRegistry.Create(this.ItemId.Value, 1, 0, false);
				this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", new object[]
				{
					this.numberCollected.Value,
					this.number.Value,
					item
				});
			}
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}

		// Token: 0x06001D03 RID: 7427 RVA: 0x0014BFE8 File Offset: 0x0014A1E8
		public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
		{
			bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
			if (!this.completed.Value && ((item != null) ? item.QualifiedItemId : null) == this.ItemId.Value && numberAdded != -1 && this.numberCollected.Value < this.number.Value)
			{
				if (!probe)
				{
					this.numberCollected.Value = Math.Min(this.number.Value, this.numberCollected.Value + numberAdded);
					Game1.dayTimeMoneyBox.pingQuest(this);
					if (this.numberCollected.Value >= this.number.Value)
					{
						NPC actualTarget = Game1.getCharacterFromName(this.target.Value, true, false);
						this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", new object[]
						{
							actualTarget
						});
						Game1.playSound("jingle1", null);
					}
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001D04 RID: 7428 RVA: 0x0014C0EC File Offset: 0x0014A2EC
		public override bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			bool baseChanged = base.OnNpcSocialized(npc, probe);
			if (!this.completed.Value && npc.IsVillager && npc.Name == this.target.Value && this.numberCollected.Value >= this.number.Value)
			{
				if (!probe)
				{
					npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage.Value));
					this.moneyReward.Value = this.reward.Value;
					this.questComplete();
					Game1.drawDialogue(npc);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x040011AF RID: 4527
		[XmlElement("target")]
		public readonly NetString target = new NetString();

		// Token: 0x040011B0 RID: 4528
		[XmlElement("targetMessage")]
		public readonly NetString targetMessage = new NetString();

		// Token: 0x040011B1 RID: 4529
		[XmlElement("numberCollected")]
		public readonly NetInt numberCollected = new NetInt();

		// Token: 0x040011B2 RID: 4530
		[XmlElement("number")]
		public readonly NetInt number = new NetInt();

		// Token: 0x040011B3 RID: 4531
		[XmlElement("reward")]
		public readonly NetInt reward = new NetInt();

		// Token: 0x040011B4 RID: 4532
		[XmlElement("resource")]
		public readonly NetString ItemId = new NetString();

		// Token: 0x040011B5 RID: 4533
		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		// Token: 0x040011B6 RID: 4534
		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		// Token: 0x040011B7 RID: 4535
		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();
	}
}

using System;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests
{
	// Token: 0x0200018C RID: 396
	public class FishingQuest : Quest
	{
		// Token: 0x06001C98 RID: 7320 RVA: 0x00146FEC File Offset: 0x001451EC
		public FishingQuest()
		{
			this.questType.Value = 7;
		}

		// Token: 0x06001C99 RID: 7321 RVA: 0x00147064 File Offset: 0x00145264
		public FishingQuest(string itemId, int numberToFish, string target, string questTitle, string questDescription, string returnDialogue) : this()
		{
			this.ItemId.Value = ItemRegistry.QualifyItemId(itemId);
			this.numberToFish.Value = numberToFish;
			this.target.Value = target;
			base.questDescription = questDescription;
			base.questTitle = questTitle;
			this._loadedTitle = true;
			this.targetMessage = returnDialogue;
		}

		// Token: 0x06001C9A RID: 7322 RVA: 0x001470C0 File Offset: 0x001452C0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective").AddField(this.target, "target").AddField(this.numberToFish, "numberToFish").AddField(this.reward, "reward").AddField(this.numberFished, "numberFished").AddField(this.ItemId, "ItemId");
		}

		// Token: 0x06001C9B RID: 7323 RVA: 0x0014715C File Offset: 0x0014535C
		public void loadQuestInfo()
		{
			if (this.target.Value != null && this.ItemId.Value != null)
			{
				return;
			}
			Random random = base.CreateInitializationRandom();
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
			if (random.NextBool())
			{
				switch (Game1.season)
				{
				case Season.Spring:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)129",
						"(O)131",
						"(O)136",
						"(O)137",
						"(O)142",
						"(O)143",
						"(O)145",
						"(O)147"
					});
					break;
				case Season.Summer:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)130",
						"(O)136",
						"(O)138",
						"(O)142",
						"(O)144",
						"(O)145",
						"(O)146",
						"(O)149",
						"(O)150"
					});
					break;
				case Season.Fall:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)129",
						"(O)131",
						"(O)136",
						"(O)137",
						"(O)139",
						"(O)142",
						"(O)143",
						"(O)150"
					});
					break;
				case Season.Winter:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)130",
						"(O)131",
						"(O)136",
						"(O)141",
						"(O)144",
						"(O)146",
						"(O)147",
						"(O)150",
						"(O)151"
					});
					break;
				}
				Item fish = ItemRegistry.Create(this.ItemId.Value, 1, 0, false);
				bool isOctopus = this.ItemId.Value == "(O)149";
				this.numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, this.GetGoldRewardPerItem(fish))) + Game1.player.FishingLevel / 5;
				this.reward.Value = this.numberToFish.Value * this.GetGoldRewardPerItem(fish);
				this.target.Value = "Demetrius";
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13228", new object[]
				{
					fish,
					this.numberToFish.Value
				}));
				this.dialogueparts.Clear();
				this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13231", new object[]
				{
					fish,
					random.Choose(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13233", Array.Empty<object>()), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13234", Array.Empty<object>()), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13235", Array.Empty<object>()), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13236", new object[]
					{
						fish
					}))
				}));
				this.objective.Value = (isOctopus ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", new object[]
				{
					0,
					this.numberToFish.Value
				}) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", new object[]
				{
					0,
					this.numberToFish.Value,
					fish
				}));
			}
			else
			{
				switch (Game1.season)
				{
				case Season.Spring:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)129",
						"(O)131",
						"(O)136",
						"(O)137",
						"(O)142",
						"(O)143",
						"(O)145",
						"(O)147",
						"(O)702"
					});
					break;
				case Season.Summer:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)128",
						"(O)130",
						"(O)136",
						"(O)138",
						"(O)142",
						"(O)144",
						"(O)145",
						"(O)146",
						"(O)149",
						"(O)150",
						"(O)702"
					});
					break;
				case Season.Fall:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)129",
						"(O)131",
						"(O)136",
						"(O)137",
						"(O)139",
						"(O)142",
						"(O)143",
						"(O)150",
						"(O)699",
						"(O)702",
						"(O)705"
					});
					break;
				case Season.Winter:
					this.ItemId.Value = random.Choose(new string[]
					{
						"(O)130",
						"(O)131",
						"(O)136",
						"(O)141",
						"(O)143",
						"(O)144",
						"(O)146",
						"(O)147",
						"(O)151",
						"(O)699",
						"(O)702",
						"(O)705"
					});
					break;
				}
				this.target.Value = "Willy";
				Item fish2 = ItemRegistry.Create(this.ItemId.Value, 1, 0, false);
				bool isSquid = this.ItemId.Value == "(O)151";
				this.numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, this.GetGoldRewardPerItem(fish2))) + Game1.player.FishingLevel / 5;
				this.reward.Value = this.numberToFish.Value * this.GetGoldRewardPerItem(fish2);
				this.parts.Clear();
				this.parts.Add(isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", new object[]
				{
					this.reward.Value,
					this.numberToFish.Value,
					new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13253", Array.Empty<object>())
				}) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", new object[]
				{
					this.reward.Value,
					this.numberToFish.Value,
					fish2
				}));
				this.dialogueparts.Clear();
				this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13256", new object[]
				{
					fish2
				}));
				this.dialogueparts.Add(random.Choose(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13258", Array.Empty<object>()), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13259", Array.Empty<object>()), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13260", new object[]
				{
					new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs." + random.Choose(new string[]
					{
						"13261",
						"13262",
						"13263",
						"13264",
						"13265",
						"13266"
					}), Array.Empty<object>())
				}), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13267", Array.Empty<object>())));
				this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13268", Array.Empty<object>()));
				this.objective.Value = (isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", new object[]
				{
					0,
					this.numberToFish.Value
				}) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", new object[]
				{
					0,
					this.numberToFish.Value,
					fish2
				}));
			}
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", new object[]
			{
				this.reward.Value
			}));
			this.parts.Add("Strings\\StringsFromCSFiles:FishingQuest.cs.13275");
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x001479A8 File Offset: 0x00145BA8
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
			this.targetMessage = messageBuilder;
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x00147AA8 File Offset: 0x00145CA8
		public override void reloadObjective()
		{
			bool isOctopus = this.ItemId.Value == "(O)149";
			bool isSquid = this.ItemId.Value == "(O)151";
			if (this.numberFished.Value < this.numberToFish.Value)
			{
				this.objective.Value = (isOctopus ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", new object[]
				{
					this.numberFished.Value,
					this.numberToFish.Value
				}) : (isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", new object[]
				{
					this.numberFished.Value,
					this.numberToFish.Value
				}) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", new object[]
				{
					this.numberFished.Value,
					this.numberToFish.Value,
					ItemRegistry.Create(this.ItemId.Value, 1, 0, false)
				})));
			}
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x00147BF0 File Offset: 0x00145DF0
		public override bool OnFishCaught(string fishId, int numberCaught, int size, bool probe = false)
		{
			bool baseChanged = base.OnFishCaught(fishId, numberCaught, size, probe);
			this.loadQuestInfo();
			if (fishId == this.ItemId.Value && this.numberFished.Value < this.numberToFish.Value)
			{
				if (!probe)
				{
					this.numberFished.Value = Math.Min(this.numberToFish.Value, this.numberFished.Value + numberCaught);
					Game1.dayTimeMoneyBox.pingQuest(this);
					if (this.numberFished.Value >= this.numberToFish.Value)
					{
						if (this.target.Value == null)
						{
							this.target.Value = "Willy";
						}
						NPC actualTarget = Game1.getCharacterFromName(this.target.Value, true, false);
						this.objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", new object[]
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

		// Token: 0x06001C9F RID: 7327 RVA: 0x00147CF8 File Offset: 0x00145EF8
		public override bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			bool baseChanged = base.OnNpcSocialized(npc, probe);
			if (this.numberFished.Value >= this.numberToFish.Value && this.target.Value != null && npc.Name == this.target.Value && npc.IsVillager && !this.completed.Value)
			{
				if (!probe)
				{
					npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage));
					this.moneyReward.Value = this.reward.Value;
					this.questComplete();
					Game1.drawDialogue(npc);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001CA0 RID: 7328 RVA: 0x00147DA0 File Offset: 0x00145FA0
		private int GetGoldRewardPerItem(Item item)
		{
			Object obj = item as Object;
			if (obj != null)
			{
				return obj.Price;
			}
			return (int)((float)item.salePrice(false) * 1.5f);
		}

		// Token: 0x04001174 RID: 4468
		[XmlElement("target")]
		public readonly NetString target = new NetString();

		// Token: 0x04001175 RID: 4469
		public string targetMessage;

		// Token: 0x04001176 RID: 4470
		[XmlElement("numberToFish")]
		public readonly NetInt numberToFish = new NetInt();

		// Token: 0x04001177 RID: 4471
		[XmlElement("reward")]
		public readonly NetInt reward = new NetInt();

		// Token: 0x04001178 RID: 4472
		[XmlElement("numberFished")]
		public readonly NetInt numberFished = new NetInt();

		// Token: 0x04001179 RID: 4473
		[XmlElement("whichFish")]
		public readonly NetString ItemId = new NetString();

		// Token: 0x0400117A RID: 4474
		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		// Token: 0x0400117B RID: 4475
		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		// Token: 0x0400117C RID: 4476
		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();
	}
}

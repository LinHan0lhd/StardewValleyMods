using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Monsters;

namespace StardewValley.Quests
{
	// Token: 0x02000192 RID: 402
	[XmlInclude(typeof(CraftingQuest))]
	[XmlInclude(typeof(DescriptionElement))]
	[XmlInclude(typeof(FishingQuest))]
	[XmlInclude(typeof(GoSomewhereQuest))]
	[XmlInclude(typeof(HaveBuildingQuest))]
	[XmlInclude(typeof(ItemDeliveryQuest))]
	[XmlInclude(typeof(ItemHarvestQuest))]
	[XmlInclude(typeof(LostItemQuest))]
	[XmlInclude(typeof(ResourceCollectionQuest))]
	[XmlInclude(typeof(SecretLostItemQuest))]
	[XmlInclude(typeof(SlayMonsterQuest))]
	[XmlInclude(typeof(SocializeQuest))]
	public class Quest : INetObject<NetFields>, IQuest, IHaveModData
	{
		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06001CBE RID: 7358 RVA: 0x0014A310 File Offset: 0x00148510
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06001CBF RID: 7359 RVA: 0x0014A318 File Offset: 0x00148518
		// (set) Token: 0x06001CC0 RID: 7360 RVA: 0x0014A325 File Offset: 0x00148525
		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return this.modData.GetForSerialization();
			}
			set
			{
				this.modData.SetFromSerialization(value);
			}
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06001CC1 RID: 7361 RVA: 0x0014A333 File Offset: 0x00148533
		public NetFields NetFields { get; }

		// Token: 0x06001CC2 RID: 7362 RVA: 0x0014A33C File Offset: 0x0014853C
		public Quest()
		{
			this.NetFields = new NetFields(NetFields.GetNameForInstance<Quest>(this));
			this.initNetFields();
		}

		// Token: 0x06001CC3 RID: 7363 RVA: 0x0014A424 File Offset: 0x00148624
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.rewardDescription, "rewardDescription").AddField(this.accepted, "accepted").AddField(this.completed, "completed").AddField(this.dailyQuest, "dailyQuest").AddField(this.showNew, "showNew").AddField(this.canBeCancelled, "canBeCancelled").AddField(this.destroy, "destroy").AddField(this.id, "id").AddField(this.moneyReward, "moneyReward").AddField(this.questType, "questType").AddField(this.daysLeft, "daysLeft").AddField(this.nextQuests, "nextQuests").AddField(this.dayQuestAccepted, "dayQuestAccepted").AddField(this.modData, "modData");
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06001CC4 RID: 7364 RVA: 0x0014A520 File Offset: 0x00148720
		// (set) Token: 0x06001CC5 RID: 7365 RVA: 0x0014A773 File Offset: 0x00148973
		public string questTitle
		{
			get
			{
				if (!this._loadedTitle)
				{
					switch (this.questType.Value)
					{
					case 3:
					{
						ItemDeliveryQuest deliveryQuest = this as ItemDeliveryQuest;
						if (deliveryQuest != null && deliveryQuest.target.Value != null)
						{
							this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ItemDeliveryQuestTitle", NPC.GetDisplayName(deliveryQuest.target.Value));
						}
						else
						{
							this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13285");
						}
						break;
					}
					case 4:
					{
						SlayMonsterQuest slayQuest = this as SlayMonsterQuest;
						if (slayQuest != null && slayQuest.monsterName.Value != null)
						{
							this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:MonsterQuestTitle", Monster.GetDisplayName(slayQuest.monsterName.Value));
						}
						else
						{
							this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
						}
						break;
					}
					case 5:
						this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
						break;
					case 7:
					{
						FishingQuest fishQuest = this as FishingQuest;
						if (fishQuest != null && fishQuest.ItemId.Value != null)
						{
							string fishName = "???";
							ParsedItemData data = ItemRegistry.GetDataOrErrorItem(fishQuest.ItemId.Value);
							if (!data.IsErrorItem)
							{
								fishName = data.DisplayName;
							}
							this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:FishingQuestTitle", fishName);
						}
						else
						{
							this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
						}
						break;
					}
					case 10:
					{
						ResourceCollectionQuest collectQuest = this as ResourceCollectionQuest;
						if (collectQuest != null && collectQuest.ItemId.Value != null)
						{
							string resourceName = "???";
							ParsedItemData data2 = ItemRegistry.GetDataOrErrorItem(collectQuest.ItemId.Value);
							if (!data2.IsErrorItem)
							{
								resourceName = data2.DisplayName;
							}
							this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ResourceQuestTitle", resourceName);
						}
						else
						{
							this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
						}
						break;
					}
					}
					string[] fields = Quest.GetRawQuestFields(this.id.Value);
					this._questTitle = ArgUtility.Get(fields, 1, this._questTitle, true);
					this._loadedTitle = true;
				}
				if (this._questTitle == null)
				{
					this._questTitle = "";
				}
				return this._questTitle;
			}
			set
			{
				this._questTitle = value;
			}
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06001CC6 RID: 7366 RVA: 0x0014A77C File Offset: 0x0014897C
		// (set) Token: 0x06001CC7 RID: 7367 RVA: 0x0014A7DC File Offset: 0x001489DC
		[XmlIgnore]
		public string questDescription
		{
			get
			{
				if (!this._loadedDescription)
				{
					this.reloadDescription();
					string[] fields = Quest.GetRawQuestFields(this.id.Value);
					this._questDescription = ArgUtility.Get(fields, 2, this._questDescription, true);
					this._loadedDescription = true;
				}
				if (this._questDescription == null)
				{
					this._questDescription = "";
				}
				return this._questDescription;
			}
			set
			{
				this._questDescription = value;
			}
		}

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06001CC8 RID: 7368 RVA: 0x0014A7E8 File Offset: 0x001489E8
		// (set) Token: 0x06001CC9 RID: 7369 RVA: 0x0014A839 File Offset: 0x00148A39
		[XmlIgnore]
		public string currentObjective
		{
			get
			{
				string[] fields = Quest.GetRawQuestFields(this.id.Value);
				this._currentObjective = ArgUtility.Get(fields, 3, this._currentObjective, false);
				this.reloadObjective();
				if (this._currentObjective == null)
				{
					this._currentObjective = "";
				}
				return this._currentObjective;
			}
			set
			{
				this._currentObjective = value;
			}
		}

		// Token: 0x06001CCA RID: 7370 RVA: 0x0014A844 File Offset: 0x00148A44
		public static string[] GetRawQuestFields(string id)
		{
			if (id == null)
			{
				return null;
			}
			Dictionary<string, string> questData = DataLoader.Quests(Game1.content);
			string rawData;
			if (questData == null || !questData.TryGetValue(id, out rawData))
			{
				return null;
			}
			return rawData.Split('/', StringSplitOptions.None);
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x0014A87C File Offset: 0x00148A7C
		public static Quest getQuestFromId(string id)
		{
			string[] fields = Quest.GetRawQuestFields(id);
			if (fields == null)
			{
				return null;
			}
			string questType;
			string error;
			string title;
			string description;
			string objective;
			string rawNextQuests;
			int moneyReward;
			string rewardDescription;
			bool canBeCancelled;
			if (!ArgUtility.TryGet(fields, 0, out questType, out error, false, "string questType") || !ArgUtility.TryGet(fields, 1, out title, out error, false, "string title") || !ArgUtility.TryGet(fields, 2, out description, out error, false, "string description") || !ArgUtility.TryGetOptional(fields, 3, out objective, out error, null, false, "string objective") || !ArgUtility.TryGetOptional(fields, 5, out rawNextQuests, out error, null, false, "string rawNextQuests") || !ArgUtility.TryGetInt(fields, 6, out moneyReward, out error, "int moneyReward") || !ArgUtility.TryGetOptional(fields, 7, out rewardDescription, out error, null, false, "string rewardDescription") || !ArgUtility.TryGetOptionalBool(fields, 8, out canBeCancelled, out error, false, "bool canBeCancelled"))
			{
				return Quest.LogParseError(id, error);
			}
			string[] nextQuests = ArgUtility.SplitBySpace(rawNextQuests);
			if (questType != null)
			{
				Quest q;
				switch (questType.Length)
				{
				case 5:
					if (!(questType == "Basic"))
					{
						goto IL_685;
					}
					q = new Quest();
					q.questType.Value = 1;
					break;
				case 6:
				{
					if (!(questType == "Social"))
					{
						goto IL_685;
					}
					SocializeQuest socializeQuest = new SocializeQuest();
					socializeQuest.loadQuestInfo();
					q = socializeQuest;
					break;
				}
				case 7:
				{
					if (!(questType == "Monster"))
					{
						goto IL_685;
					}
					string[] conditions;
					if (!Quest.TryParseConditions(fields, out conditions, out error, false))
					{
						return Quest.LogParseError(id, error);
					}
					string monsterName;
					int numberToKill;
					string targetNpc;
					bool ignoreFarmMonsters;
					if (!ArgUtility.TryGet(conditions, 0, out monsterName, out error, false, "string monsterName") || !ArgUtility.TryGetInt(conditions, 1, out numberToKill, out error, "int numberToKill") || !ArgUtility.TryGetOptional(conditions, 2, out targetNpc, out error, null, true, "string targetNpc") || !ArgUtility.TryGetOptionalBool(conditions, 3, out ignoreFarmMonsters, out error, true, "bool ignoreFarmMonsters"))
					{
						return Quest.LogConditionsParseError(id, error);
					}
					SlayMonsterQuest slayQuest = new SlayMonsterQuest();
					slayQuest.loadQuestInfo();
					slayQuest.monster.Value.Name = monsterName.Replace('_', ' ');
					slayQuest.monsterName.Value = slayQuest.monster.Value.Name;
					slayQuest.numberToKill.Value = numberToKill;
					slayQuest.ignoreFarmMonsters.Value = ignoreFarmMonsters;
					slayQuest.target.Value = (targetNpc ?? "null");
					slayQuest.questType.Value = 4;
					q = slayQuest;
					break;
				}
				case 8:
				{
					char c = questType[2];
					if (c <= 'c')
					{
						if (c != 'a')
						{
							if (c != 'c')
							{
								goto IL_685;
							}
							if (!(questType == "Location"))
							{
								goto IL_685;
							}
							string[] conditions2;
							if (!Quest.TryParseConditions(fields, out conditions2, out error, false))
							{
								return Quest.LogParseError(id, error);
							}
							string locationName;
							if (!ArgUtility.TryGet(conditions2, 0, out locationName, out error, false, "string locationName"))
							{
								return Quest.LogConditionsParseError(id, error);
							}
							q = new GoSomewhereQuest(locationName);
							q.questType.Value = 6;
						}
						else
						{
							if (!(questType == "Crafting"))
							{
								goto IL_685;
							}
							string[] conditions3;
							if (!Quest.TryParseConditions(fields, out conditions3, out error, false))
							{
								return Quest.LogParseError(id, error);
							}
							string itemId;
							if (!ArgUtility.TryGet(conditions3, 0, out itemId, out error, false, "string itemId"))
							{
								return Quest.LogConditionsParseError(id, error);
							}
							bool? isBigCraftable = null;
							if (ArgUtility.HasIndex<string>(conditions3, 1))
							{
								bool isBigCraftableValue;
								if (!ArgUtility.TryGetOptionalBool(conditions3, 1, out isBigCraftableValue, out error, false, "bool isBigCraftableValue"))
								{
									return Quest.LogConditionsParseError(id, error);
								}
								isBigCraftable = new bool?(isBigCraftableValue);
							}
							if (!ItemRegistry.IsQualifiedItemId(itemId))
							{
								if (isBigCraftable != null)
								{
									itemId = (isBigCraftable.Value ? ("(BC)" + itemId) : ("(O)" + itemId));
								}
								else
								{
									itemId = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
								}
							}
							q = new CraftingQuest(itemId);
							q.questType.Value = 2;
						}
					}
					else if (c != 'i')
					{
						if (c != 's')
						{
							goto IL_685;
						}
						if (!(questType == "LostItem"))
						{
							goto IL_685;
						}
						string[] conditions4;
						if (!Quest.TryParseConditions(fields, out conditions4, out error, false))
						{
							return Quest.LogParseError(id, error);
						}
						string npcName;
						string itemId2;
						string locationOfItem;
						int tileX;
						int tileY;
						if (!ArgUtility.TryGet(conditions4, 0, out npcName, out error, false, "string npcName") || !ArgUtility.TryGet(conditions4, 1, out itemId2, out error, false, "string itemId") || !ArgUtility.TryGet(conditions4, 2, out locationOfItem, out error, false, "string locationOfItem") || !ArgUtility.TryGetInt(conditions4, 3, out tileX, out error, "int tileX") || !ArgUtility.TryGetInt(conditions4, 4, out tileY, out error, "int tileY"))
						{
							return Quest.LogConditionsParseError(id, error);
						}
						q = new LostItemQuest(npcName, locationOfItem, itemId2, tileX, tileY);
					}
					else
					{
						if (!(questType == "Building"))
						{
							goto IL_685;
						}
						string[] conditions5;
						if (!Quest.TryParseConditions(fields, out conditions5, out error, false))
						{
							return Quest.LogParseError(id, error);
						}
						string buildingType;
						if (!ArgUtility.TryGet(conditions5, 0, out buildingType, out error, false, "string buildingType"))
						{
							return Quest.LogConditionsParseError(id, error);
						}
						q = new HaveBuildingQuest(buildingType);
					}
					break;
				}
				case 9:
				case 10:
				case 13:
					goto IL_685;
				case 11:
				{
					if (!(questType == "ItemHarvest"))
					{
						goto IL_685;
					}
					string[] conditions6;
					if (!Quest.TryParseConditions(fields, out conditions6, out error, false))
					{
						return Quest.LogParseError(id, error);
					}
					string itemId3;
					int numberRequired;
					if (!ArgUtility.TryGet(conditions6, 0, out itemId3, out error, false, "string itemId") || !ArgUtility.TryGetOptionalInt(conditions6, 1, out numberRequired, out error, 1, "int numberRequired"))
					{
						return Quest.LogConditionsParseError(id, error);
					}
					q = new ItemHarvestQuest(itemId3, numberRequired);
					break;
				}
				case 12:
				{
					if (!(questType == "ItemDelivery"))
					{
						goto IL_685;
					}
					string[] conditions7;
					string targetMessage;
					if (!Quest.TryParseConditions(fields, out conditions7, out error, false) || !ArgUtility.TryGet(fields, 9, out targetMessage, out error, false, "string targetMessage"))
					{
						return Quest.LogParseError(id, error);
					}
					string npcName2;
					string itemId4;
					int numberRequired2;
					if (!ArgUtility.TryGet(conditions7, 0, out npcName2, out error, false, "string npcName") || !ArgUtility.TryGet(conditions7, 1, out itemId4, out error, false, "string itemId") || !ArgUtility.TryGetOptionalInt(conditions7, 2, out numberRequired2, out error, 1, "int numberRequired"))
					{
						return Quest.LogConditionsParseError(id, error);
					}
					q = new ItemDeliveryQuest(npcName2, itemId4)
					{
						targetMessage = targetMessage,
						number = 
						{
							Value = numberRequired2
						},
						questType = 
						{
							Value = 3
						}
					};
					break;
				}
				case 14:
				{
					if (!(questType == "SecretLostItem"))
					{
						goto IL_685;
					}
					string[] conditions8;
					if (!Quest.TryParseConditions(fields, out conditions8, out error, false))
					{
						return Quest.LogParseError(id, error);
					}
					string npcName3;
					string itemId5;
					int friendshipReward;
					string exclusiveQuestId;
					if (!ArgUtility.TryGet(conditions8, 0, out npcName3, out error, false, "string npcName") || !ArgUtility.TryGet(conditions8, 1, out itemId5, out error, false, "string itemId") || !ArgUtility.TryGetInt(conditions8, 2, out friendshipReward, out error, "int friendshipReward") || !ArgUtility.TryGetOptional(conditions8, 3, out exclusiveQuestId, out error, null, false, "string exclusiveQuestId"))
					{
						return Quest.LogConditionsParseError(id, error);
					}
					q = new SecretLostItemQuest(npcName3, itemId5, friendshipReward, exclusiveQuestId);
					break;
				}
				default:
					goto IL_685;
				}
				q.id.Value = id;
				q.questTitle = title;
				q.questDescription = description;
				q.currentObjective = objective;
				string[] array = nextQuests;
				int i = 0;
				while (i < array.Length)
				{
					string nextQuest = array[i];
					if (!nextQuest.StartsWith('h'))
					{
						goto IL_6EF;
					}
					if (Game1.IsMasterGame)
					{
						nextQuest = nextQuest.Substring(1);
						goto IL_6EF;
					}
					IL_6FD:
					i++;
					continue;
					IL_6EF:
					q.nextQuests.Add(nextQuest);
					goto IL_6FD;
				}
				q.showNew.Value = true;
				q.moneyReward.Value = moneyReward;
				q.rewardDescription.Value = ((moneyReward == -1) ? null : rewardDescription);
				q.canBeCancelled.Value = canBeCancelled;
				return q;
			}
			IL_685:
			return Quest.LogParseError(id, "quest type '" + questType + "' doesn't match a known type.");
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x0014AFD5 File Offset: 0x001491D5
		public virtual void reloadObjective()
		{
		}

		// Token: 0x06001CCD RID: 7373 RVA: 0x0014AFD7 File Offset: 0x001491D7
		public virtual void reloadDescription()
		{
		}

		// Token: 0x06001CCE RID: 7374 RVA: 0x0014AFD9 File Offset: 0x001491D9
		public virtual void accept()
		{
			this.accepted.Value = true;
		}

		// Token: 0x06001CCF RID: 7375 RVA: 0x0014AFE7 File Offset: 0x001491E7
		public virtual bool OnBuildingExists(string buildingType, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD0 RID: 7376 RVA: 0x0014AFEA File Offset: 0x001491EA
		public virtual bool OnFishCaught(string fishId, int numberCaught, int size, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD1 RID: 7377 RVA: 0x0014AFED File Offset: 0x001491ED
		public virtual bool OnItemReceived(Item item, int numberAdded, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD2 RID: 7378 RVA: 0x0014AFF0 File Offset: 0x001491F0
		public virtual bool OnMonsterSlain(GameLocation location, Monster monster, bool killedByBomb, bool isTameMonster, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD3 RID: 7379 RVA: 0x0014AFF3 File Offset: 0x001491F3
		public virtual bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD4 RID: 7380 RVA: 0x0014AFF6 File Offset: 0x001491F6
		public virtual bool OnRecipeCrafted(CraftingRecipe recipe, Item item, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD5 RID: 7381 RVA: 0x0014AFF9 File Offset: 0x001491F9
		public virtual bool OnWarped(GameLocation location, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD6 RID: 7382 RVA: 0x0014AFFC File Offset: 0x001491FC
		public virtual bool OnItemOfferedToNpc(NPC npc, Item item, bool probe = false)
		{
			return false;
		}

		// Token: 0x06001CD7 RID: 7383 RVA: 0x0014AFFF File Offset: 0x001491FF
		public bool hasReward()
		{
			if (this.moneyReward.Value <= 0)
			{
				string value = this.rewardDescription.Value;
				return value != null && value.Length > 2;
			}
			return true;
		}

		// Token: 0x06001CD8 RID: 7384 RVA: 0x0014B02A File Offset: 0x0014922A
		public virtual bool isSecretQuest()
		{
			return false;
		}

		// Token: 0x06001CD9 RID: 7385 RVA: 0x0014B030 File Offset: 0x00149230
		public virtual void questComplete()
		{
			if (!this.completed.Value)
			{
				if (this.dailyQuest.Value)
				{
					Game1.stats.Increment("BillboardQuestsDone", 1U);
					if (!Game1.player.mailReceived.Contains("completedFirstBillboardQuest"))
					{
						Game1.player.mailReceived.Add("completedFirstBillboardQuest");
					}
					if (Game1.stats.Get("BillboardQuestsDone") % 3U == 0U)
					{
						if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)PrizeTicket", 1, 0, false), false))
						{
							Game1.createItemDebris(ItemRegistry.Create("(O)PrizeTicket", 1, 0, false), Game1.player.getStandingPosition(), 2, null, -1, false);
						}
						if (Game1.stats.Get("BillboardQuestsDone") >= 6U && !Game1.player.mailReceived.Contains("gotFirstBillboardPrizeTicket"))
						{
							Game1.player.mailReceived.Add("gotFirstBillboardPrizeTicket");
						}
					}
				}
				if (this.dailyQuest.Value || this.questType.Value == 7)
				{
					Stats stats = Game1.stats;
					uint questsCompleted = stats.QuestsCompleted;
					stats.QuestsCompleted = questsCompleted + 1U;
				}
				this.completed.Value = true;
				GameLocation currentLocation = Game1.player.currentLocation;
				if (currentLocation != null)
				{
					currentLocation.customQuestCompleteBehavior(this.id.Value);
				}
				if (this.nextQuests.Count > 0)
				{
					foreach (string i in this.nextQuests)
					{
						if (this.IsValidId(i))
						{
							Game1.player.addQuest(i);
						}
					}
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
				}
				if (this.moneyReward.Value <= 0 && (this.rewardDescription.Value == null || this.rewardDescription.Value.Length <= 2))
				{
					Game1.player.questLog.Remove(this);
				}
				else
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
				}
				Game1.playSound("questcomplete", null);
				if (this.id.Value == "126")
				{
					Game1.player.mailReceived.Add("emilyFiber");
					Game1.player.activeDialogueEvents["emilyFiber"] = 2;
				}
				Game1.dayTimeMoneyBox.questsDirty = true;
				Game1.player.autoGenerateActiveDialogueEvent("questComplete_" + this.id.Value, 4);
			}
		}

		// Token: 0x06001CDA RID: 7386 RVA: 0x0014B2D8 File Offset: 0x001494D8
		public string GetName()
		{
			return this.questTitle;
		}

		// Token: 0x06001CDB RID: 7387 RVA: 0x0014B2E0 File Offset: 0x001494E0
		public string GetDescription()
		{
			return this.questDescription;
		}

		// Token: 0x06001CDC RID: 7388 RVA: 0x0014B2E8 File Offset: 0x001494E8
		public bool IsHidden()
		{
			return this.isSecretQuest();
		}

		// Token: 0x06001CDD RID: 7389 RVA: 0x0014B2F0 File Offset: 0x001494F0
		public List<string> GetObjectiveDescriptions()
		{
			return new List<string>
			{
				this.currentObjective
			};
		}

		// Token: 0x06001CDE RID: 7390 RVA: 0x0014B303 File Offset: 0x00149503
		public bool CanBeCancelled()
		{
			return this.canBeCancelled.Value;
		}

		// Token: 0x06001CDF RID: 7391 RVA: 0x0014B310 File Offset: 0x00149510
		public bool HasReward()
		{
			if (!this.HasMoneyReward())
			{
				string value = this.rewardDescription.Value;
				return value != null && value.Length > 2;
			}
			return true;
		}

		// Token: 0x06001CE0 RID: 7392 RVA: 0x0014B335 File Offset: 0x00149535
		public bool HasMoneyReward()
		{
			return this.completed.Value && this.moneyReward.Value > 0;
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x0014B354 File Offset: 0x00149554
		public void MarkAsViewed()
		{
			this.showNew.Value = false;
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x0014B362 File Offset: 0x00149562
		public bool ShouldDisplayAsNew()
		{
			return this.showNew.Value;
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x0014B36F File Offset: 0x0014956F
		public bool ShouldDisplayAsComplete()
		{
			return this.completed.Value && !this.IsHidden();
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x0014B389 File Offset: 0x00149589
		public bool IsTimedQuest()
		{
			return this.dailyQuest.Value || this.GetDaysLeft() > 0;
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x0014B3A3 File Offset: 0x001495A3
		public int GetDaysLeft()
		{
			return this.daysLeft.Value;
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x0014B3B0 File Offset: 0x001495B0
		public int GetMoneyReward()
		{
			return this.moneyReward.Value;
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x0014B3BD File Offset: 0x001495BD
		public void OnMoneyRewardClaimed()
		{
			this.moneyReward.Value = 0;
			this.destroy.Value = true;
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x0014B3D8 File Offset: 0x001495D8
		public bool OnLeaveQuestPage()
		{
			if (this.completed.Value && this.moneyReward.Value <= 0)
			{
				this.destroy.Value = true;
			}
			if (this.destroy.Value)
			{
				Game1.player.questLog.Remove(this);
				return true;
			}
			return false;
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x0014B42D File Offset: 0x0014962D
		protected bool HasId()
		{
			return this.IsValidId(this.id.Value);
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x0014B440 File Offset: 0x00149640
		protected bool IsValidId(string id)
		{
			if (!(id == "7"))
			{
				return id != null && !(id == "-1") && !(id == "0");
			}
			return Game1.GetFarmTypeID() != "MeadowlandsFarm";
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x0014B48C File Offset: 0x0014968C
		protected Random CreateInitializationRandom()
		{
			return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
		}

		// Token: 0x06001CEC RID: 7404 RVA: 0x0014B4C4 File Offset: 0x001496C4
		protected static bool TryParseConditions(string[] questFields, out string[] conditions, out string error, bool allowBlank = false)
		{
			string rawConditions;
			if (!ArgUtility.TryGet(questFields, 4, out rawConditions, out error, allowBlank, "string rawConditions"))
			{
				conditions = null;
				return false;
			}
			conditions = ArgUtility.SplitBySpace(rawConditions);
			error = null;
			return true;
		}

		// Token: 0x06001CED RID: 7405 RVA: 0x0014B4F4 File Offset: 0x001496F4
		protected static Quest LogParseError(string id, string error)
		{
			Game1.log.Error("Failed to parse data for quest '" + id + "': " + error, null);
			return null;
		}

		// Token: 0x06001CEE RID: 7406 RVA: 0x0014B513 File Offset: 0x00149713
		protected static Quest LogConditionsParseError(string id, string error)
		{
			Game1.log.Error("Failed to parse for quest '" + id + "': conditions field (index 4) is invalid: " + error, null);
			return null;
		}

		// Token: 0x0400118F RID: 4495
		public const int type_basic = 1;

		// Token: 0x04001190 RID: 4496
		public const int type_crafting = 2;

		// Token: 0x04001191 RID: 4497
		public const int type_itemDelivery = 3;

		// Token: 0x04001192 RID: 4498
		public const int type_monster = 4;

		// Token: 0x04001193 RID: 4499
		public const int type_socialize = 5;

		// Token: 0x04001194 RID: 4500
		public const int type_location = 6;

		// Token: 0x04001195 RID: 4501
		public const int type_fishing = 7;

		// Token: 0x04001196 RID: 4502
		public const int type_building = 8;

		// Token: 0x04001197 RID: 4503
		public const int type_harvest = 9;

		// Token: 0x04001198 RID: 4504
		public const int type_resource = 10;

		// Token: 0x04001199 RID: 4505
		public const int type_weeding = 11;

		// Token: 0x0400119A RID: 4506
		public string _currentObjective = "";

		// Token: 0x0400119B RID: 4507
		public string _questDescription = "";

		// Token: 0x0400119C RID: 4508
		public string _questTitle = "";

		// Token: 0x0400119D RID: 4509
		[XmlElement("rewardDescription")]
		public readonly NetString rewardDescription = new NetString();

		// Token: 0x0400119E RID: 4510
		[XmlElement("accepted")]
		public readonly NetBool accepted = new NetBool();

		// Token: 0x0400119F RID: 4511
		[XmlElement("completed")]
		public readonly NetBool completed = new NetBool();

		// Token: 0x040011A0 RID: 4512
		[XmlElement("dailyQuest")]
		public readonly NetBool dailyQuest = new NetBool();

		// Token: 0x040011A1 RID: 4513
		[XmlElement("showNew")]
		public readonly NetBool showNew = new NetBool();

		// Token: 0x040011A2 RID: 4514
		[XmlElement("canBeCancelled")]
		public readonly NetBool canBeCancelled = new NetBool();

		// Token: 0x040011A3 RID: 4515
		[XmlElement("destroy")]
		public readonly NetBool destroy = new NetBool();

		// Token: 0x040011A4 RID: 4516
		[XmlElement("id")]
		public readonly NetString id = new NetString();

		// Token: 0x040011A5 RID: 4517
		[XmlElement("moneyReward")]
		public readonly NetInt moneyReward = new NetInt();

		// Token: 0x040011A6 RID: 4518
		[XmlElement("questType")]
		public readonly NetInt questType = new NetInt();

		// Token: 0x040011A7 RID: 4519
		[XmlElement("daysLeft")]
		public readonly NetInt daysLeft = new NetInt();

		// Token: 0x040011A8 RID: 4520
		[XmlElement("dayQuestAccepted")]
		public readonly NetInt dayQuestAccepted = new NetInt(-1);

		// Token: 0x040011A9 RID: 4521
		[XmlArrayItem("int")]
		public readonly NetStringList nextQuests = new NetStringList();

		// Token: 0x040011AB RID: 4523
		[XmlElement("completionString")]
		public string obsolete_completionString;

		// Token: 0x040011AD RID: 4525
		private bool _loadedDescription;

		// Token: 0x040011AE RID: 4526
		protected bool _loadedTitle;
	}
}

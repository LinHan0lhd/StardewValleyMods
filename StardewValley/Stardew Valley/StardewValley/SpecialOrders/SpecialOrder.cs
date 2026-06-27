using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using Netcode.Validation;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.SpecialOrders;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Quests;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.SpecialOrders.Rewards;
using StardewValley.TokenizableStrings;

namespace StardewValley.SpecialOrders
{
	// Token: 0x0200014C RID: 332
	[XmlInclude(typeof(OrderObjective))]
	[XmlInclude(typeof(OrderReward))]
	[NotImplicitNetField]
	public class SpecialOrder : INetObject<NetFields>, IQuest
	{
		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06001A5E RID: 6750 RVA: 0x0013821C File Offset: 0x0013641C
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("SpecialOrder");

		// Token: 0x06001A5F RID: 6751 RVA: 0x00138224 File Offset: 0x00136424
		public SpecialOrder()
		{
			this.InitializeNetFields();
		}

		// Token: 0x06001A60 RID: 6752 RVA: 0x00138370 File Offset: 0x00136570
		public virtual void SetDuration(QuestDuration duration)
		{
			this.questDuration.Value = duration;
			WorldDate date = new WorldDate();
			switch (duration)
			{
			case QuestDuration.Week:
			{
				date = new WorldDate(Game1.year, Game1.season, (Game1.dayOfMonth - 1) / 7 * 7);
				WorldDate worldDate = date;
				int totalDays = worldDate.TotalDays;
				worldDate.TotalDays = totalDays + 1;
				date.TotalDays += 7;
				break;
			}
			case QuestDuration.Month:
			{
				date = new WorldDate(Game1.year, Game1.season, 0);
				WorldDate worldDate2 = date;
				int totalDays = worldDate2.TotalDays;
				worldDate2.TotalDays = totalDays + 1;
				date.TotalDays += 28;
				break;
			}
			case QuestDuration.TwoWeeks:
			{
				date = new WorldDate(Game1.year, Game1.season, (Game1.dayOfMonth - 1) / 7 * 7);
				WorldDate worldDate3 = date;
				int totalDays = worldDate3.TotalDays;
				worldDate3.TotalDays = totalDays + 1;
				date.TotalDays += 14;
				break;
			}
			case QuestDuration.TwoDays:
				date = WorldDate.Now();
				date.TotalDays += 2;
				break;
			case QuestDuration.ThreeDays:
				date = WorldDate.Now();
				date.TotalDays += 3;
				break;
			case QuestDuration.OneDay:
				date = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
				date.TotalDays++;
				break;
			}
			this.dueDate.Value = date.TotalDays;
		}

		// Token: 0x06001A61 RID: 6753 RVA: 0x001384C4 File Offset: 0x001366C4
		public virtual void OnFail()
		{
			foreach (OrderObjective orderObjective in this.objectives)
			{
				orderObjective.OnFail();
			}
			for (int i = 0; i < this.donatedItems.Count; i++)
			{
				Item item = this.donatedItems[i];
				this.donatedItems[i] = null;
				if (item != null)
				{
					Game1.player.team.returnedDonations.Add(item);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
			}
			if (Game1.IsMasterGame)
			{
				this.HostHandleQuestEnd();
			}
			this.questState.Value = SpecialOrderStatus.Failed;
			this._RemoveSpecialRuleIfNecessary();
		}

		// Token: 0x06001A62 RID: 6754 RVA: 0x00138590 File Offset: 0x00136790
		public virtual int GetCompleteObjectivesCount()
		{
			int count = 0;
			using (NetList<OrderObjective, NetRef<OrderObjective>>.Enumerator enumerator = this.objectives.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsComplete())
					{
						count++;
					}
				}
			}
			return count;
		}

		// Token: 0x06001A63 RID: 6755 RVA: 0x001385EC File Offset: 0x001367EC
		public virtual void ConfirmCompleteDonations()
		{
			foreach (OrderObjective orderObjective in this.objectives)
			{
				DonateObjective donateObjective = orderObjective as DonateObjective;
				if (donateObjective != null)
				{
					donateObjective.Confirm();
				}
			}
		}

		// Token: 0x06001A64 RID: 6756 RVA: 0x00138648 File Offset: 0x00136848
		public virtual void UpdateDonationCounts()
		{
			this._highlightLookup = null;
			int old_completed_objectives_count = 0;
			int new_completed_objectives_count = 0;
			foreach (OrderObjective orderObjective in this.objectives)
			{
				DonateObjective donate_objective = orderObjective as DonateObjective;
				if (donate_objective != null)
				{
					int count = 0;
					if (donate_objective.GetCount() >= donate_objective.GetMaxCount())
					{
						old_completed_objectives_count++;
					}
					foreach (Item item in this.donatedItems)
					{
						if (donate_objective.IsValidItem(item))
						{
							count += item.Stack;
						}
					}
					donate_objective.SetCount(count);
					if (donate_objective.GetCount() >= donate_objective.GetMaxCount())
					{
						new_completed_objectives_count++;
					}
				}
			}
			if (new_completed_objectives_count > old_completed_objectives_count)
			{
				Game1.playSound("newArtifact", null);
			}
		}

		// Token: 0x06001A65 RID: 6757 RVA: 0x00138748 File Offset: 0x00136948
		public bool HighlightAcceptableItems(Item item)
		{
			bool acceptable;
			if (this._highlightLookup != null && this._highlightLookup.TryGetValue(item, out acceptable))
			{
				return acceptable;
			}
			if (this._highlightLookup == null)
			{
				this._highlightLookup = new Dictionary<Item, bool>();
			}
			foreach (OrderObjective orderObjective in this.objectives)
			{
				DonateObjective donate_objective = orderObjective as DonateObjective;
				if (donate_objective != null && donate_objective.GetAcceptCount(item, 1) > 0)
				{
					this._highlightLookup[item] = true;
					return true;
				}
			}
			this._highlightLookup[item] = false;
			return false;
		}

		// Token: 0x06001A66 RID: 6758 RVA: 0x001387F8 File Offset: 0x001369F8
		public virtual int GetAcceptCount(Item item)
		{
			int total_accepted_count = 0;
			int total_stacks = item.Stack;
			foreach (OrderObjective orderObjective in this.objectives)
			{
				DonateObjective donate_objective = orderObjective as DonateObjective;
				if (donate_objective != null)
				{
					int accepted_count = donate_objective.GetAcceptCount(item, total_stacks);
					total_stacks -= accepted_count;
					total_accepted_count += accepted_count;
				}
			}
			return total_accepted_count;
		}

		// Token: 0x06001A67 RID: 6759 RVA: 0x0013886C File Offset: 0x00136A6C
		public static bool CheckTags(string tag_list)
		{
			if (tag_list == null)
			{
				return true;
			}
			string[] tags = tag_list.Split(',', StringSplitOptions.None);
			for (int i = 0; i < tags.Length; i++)
			{
				tags[i] = tags[i].Trim();
			}
			foreach (string current_tag in tags)
			{
				if (current_tag.Length != 0)
				{
					bool match = true;
					if (current_tag.StartsWith('!'))
					{
						match = false;
						current_tag = current_tag.Substring(1);
					}
					if (SpecialOrder.CheckTag(current_tag) != match)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001A68 RID: 6760 RVA: 0x001388E8 File Offset: 0x00136AE8
		public static bool CheckTag(string tag)
		{
			if (tag == "NOT_IMPLEMENTED")
			{
				return false;
			}
			if (tag.StartsWith("dropbox_"))
			{
				string value = tag.Substring("dropbox_".Length);
				using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator = Game1.player.team.specialOrders.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.UsesDropBox(value))
						{
							return true;
						}
					}
				}
			}
			if (tag.StartsWith("rule_"))
			{
				string value2 = tag.Substring("rule_".Length);
				if (Game1.player.team.SpecialOrderRuleActive(value2, null))
				{
					return true;
				}
			}
			if (tag.StartsWith("completed_"))
			{
				string value3 = tag.Substring("completed_".Length);
				if (Game1.player.team.completedSpecialOrders.Contains(value3))
				{
					return true;
				}
			}
			if (tag.StartsWith("season_"))
			{
				string value4 = tag.Substring("season_".Length);
				if (Game1.currentSeason == value4)
				{
					return true;
				}
			}
			else if (tag.StartsWith("mail_"))
			{
				string value5 = tag.Substring("mail_".Length);
				if (Game1.MasterPlayer.hasOrWillReceiveMail(value5))
				{
					return true;
				}
			}
			else if (tag.StartsWith("event_"))
			{
				string value6 = tag.Substring("event_".Length);
				if (Game1.MasterPlayer.eventsSeen.Contains(value6))
				{
					return true;
				}
			}
			else
			{
				if (tag == "island")
				{
					return Utility.doesAnyFarmerHaveOrWillReceiveMail("seenBoatJourney");
				}
				if (tag.StartsWith("knows_"))
				{
					string value7 = tag.Substring("knows_".Length);
					using (IEnumerator<Farmer> enumerator2 = Game1.getAllFarmers().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.friendshipData.ContainsKey(value7))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06001A69 RID: 6761 RVA: 0x00138B08 File Offset: 0x00136D08
		public bool IsIslandOrder()
		{
			SpecialOrderData data;
			if (this._isIslandOrder == -1 && DataLoader.SpecialOrders(Game1.content).TryGetValue(this.questKey.Value, out data))
			{
				string requiredTags = data.RequiredTags;
				this._isIslandOrder = ((requiredTags != null && requiredTags.Contains("island")) ? (this._isIslandOrder = 1) : (this._isIslandOrder = 0));
			}
			return this._isIslandOrder == 1;
		}

		// Token: 0x06001A6A RID: 6762 RVA: 0x00138B7A File Offset: 0x00136D7A
		public static bool IsSpecialOrdersBoardUnlocked()
		{
			return Game1.stats.DaysPlayed >= 58U;
		}

		// Token: 0x06001A6B RID: 6763 RVA: 0x00138B90 File Offset: 0x00136D90
		public static void RemoveAllSpecialOrders(string orderType)
		{
			Game1.player.team.availableSpecialOrders.RemoveWhere((SpecialOrder order) => order.orderType.Value == orderType);
			Game1.player.team.acceptedSpecialOrderTypes.Remove(orderType);
		}

		// Token: 0x06001A6C RID: 6764 RVA: 0x00138BE8 File Offset: 0x00136DE8
		public static void UpdateAvailableSpecialOrders(string orderType, bool forceRefresh)
		{
			foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
			{
				if ((order.questDuration.Value == QuestDuration.TwoDays || order.questDuration.Value == QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
				{
					order.SetDuration(order.questDuration.Value);
				}
			}
			if (!forceRefresh)
			{
				using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator = Game1.player.team.availableSpecialOrders.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.orderType.Value == orderType)
						{
							return;
						}
					}
				}
			}
			SpecialOrder.RemoveAllSpecialOrders(orderType);
			List<string> keyQueue = new List<string>();
			foreach (KeyValuePair<string, SpecialOrderData> pair in DataLoader.SpecialOrders(Game1.content))
			{
				if (pair.Value.OrderType == orderType && SpecialOrder.CanStartOrderNow(pair.Key, pair.Value))
				{
					keyQueue.Add(pair.Key);
				}
			}
			List<string> keysIncludingCompleted = new List<string>(keyQueue);
			if (orderType == "")
			{
				keyQueue.RemoveAll((string id) => Game1.player.team.completedSpecialOrders.Contains(id));
			}
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 1.3, 0.0, 0.0, 0.0);
			for (int i = 0; i < 2; i++)
			{
				if (keyQueue.Count == 0)
				{
					if (keysIncludingCompleted.Count == 0)
					{
						break;
					}
					keyQueue = new List<string>(keysIncludingCompleted);
				}
				string key = r.ChooseFrom(keyQueue);
				Game1.player.team.availableSpecialOrders.Add(SpecialOrder.GetSpecialOrder(key, new int?(r.Next())));
				keyQueue.Remove(key);
				keysIncludingCompleted.Remove(key);
			}
		}

		// Token: 0x06001A6D RID: 6765 RVA: 0x00138E54 File Offset: 0x00137054
		public static bool CanStartOrderNow(string orderId, SpecialOrderData order)
		{
			if (!order.Repeatable && Game1.MasterPlayer.team.completedSpecialOrders.Contains(orderId))
			{
				return false;
			}
			if (Game1.dayOfMonth >= 16 && order.Duration == QuestDuration.Month)
			{
				return false;
			}
			if (!SpecialOrder.CheckTags(order.RequiredTags))
			{
				return false;
			}
			if (!GameStateQuery.CheckConditions(order.Condition, null, null, null, null, null, null))
			{
				return false;
			}
			using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator = Game1.player.team.specialOrders.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.questKey.Value == orderId)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001A6E RID: 6766 RVA: 0x00138F1C File Offset: 0x0013711C
		public static SpecialOrder GetSpecialOrder(string key, int? generation_seed)
		{
			try
			{
				if (generation_seed == null)
				{
					generation_seed = new int?(Game1.random.Next());
				}
				SpecialOrderData data;
				if (DataLoader.SpecialOrders(Game1.content).TryGetValue(key, out data))
				{
					Random r = Utility.CreateRandom((double)generation_seed.Value, 0.0, 0.0, 0.0, 0.0);
					SpecialOrder order = new SpecialOrder();
					order.generationSeed.Value = generation_seed.Value;
					order._orderData = data;
					order.questKey.Value = key;
					order.questName.Value = data.Name;
					order.requester.Value = data.Requester;
					order.orderType.Value = data.OrderType.Trim();
					order.specialRule.Value = data.SpecialRule.Trim();
					if (data.ItemToRemoveOnEnd != null)
					{
						order.itemToRemoveOnEnd.Value = data.ItemToRemoveOnEnd;
					}
					if (data.MailToRemoveOnEnd != null)
					{
						order.mailToRemoveOnEnd.Value = data.MailToRemoveOnEnd;
					}
					order.selectedRandomElements.Clear();
					if (data.RandomizedElements != null)
					{
						foreach (RandomizedElement randomized_element in data.RandomizedElements)
						{
							List<int> valid_indices = new List<int>();
							for (int i = 0; i < randomized_element.Values.Count; i++)
							{
								if (SpecialOrder.CheckTags(randomized_element.Values[i].RequiredTags))
								{
									valid_indices.Add(i);
								}
							}
							int selected_index = r.ChooseFrom(valid_indices);
							order.selectedRandomElements[randomized_element.Name] = selected_index;
							string value = randomized_element.Values[selected_index].Value;
							if (value.StartsWith("PICK_ITEM"))
							{
								value = value.Substring("PICK_ITEM".Length);
								string[] array = value.Split(',', StringSplitOptions.None);
								List<string> valid_item_ids = new List<string>();
								string[] array2 = array;
								for (int j = 0; j < array2.Length; j++)
								{
									string valid_item_name = array2[j].Trim();
									if (valid_item_name.Length != 0)
									{
										ParsedItemData parsedData = ItemRegistry.GetData(valid_item_name);
										if (parsedData != null)
										{
											valid_item_ids.Add(parsedData.QualifiedItemId);
										}
										else
										{
											Item item = Utility.fuzzyItemSearch(valid_item_name, 1, false);
											valid_item_ids.Add(item.QualifiedItemId);
										}
									}
								}
								order.preSelectedItems[randomized_element.Name] = r.ChooseFrom(valid_item_ids);
							}
						}
					}
					order.SetDuration(data.Duration);
					order.questDescription.Value = data.Text;
					string objectivesNamespace = typeof(OrderObjective).Namespace;
					string rewardsNamespace = typeof(OrderReward).Namespace;
					foreach (SpecialOrderObjectiveData objective_data in data.Objectives)
					{
						Type objective_type = Type.GetType(objectivesNamespace + "." + objective_data.Type.Trim() + "Objective");
						if (!(objective_type == null) && objective_type.IsSubclassOf(typeof(OrderObjective)))
						{
							OrderObjective objective = (OrderObjective)Activator.CreateInstance(objective_type);
							if (objective != null)
							{
								objective.description.Value = objective_data.Text;
								objective.maxCount.Value = int.Parse(order.Parse(objective_data.RequiredCount));
								objective.Load(order, objective_data.Data);
								order.objectives.Add(objective);
							}
						}
					}
					foreach (SpecialOrderRewardData reward_data in data.Rewards)
					{
						Type reward_type = Type.GetType(rewardsNamespace + "." + reward_data.Type.Trim() + "Reward");
						if (!(reward_type == null) && reward_type.IsSubclassOf(typeof(OrderReward)))
						{
							OrderReward reward = (OrderReward)Activator.CreateInstance(reward_type);
							if (reward != null)
							{
								reward.Load(order, reward_data.Data);
								order.rewards.Add(reward);
							}
						}
					}
					return order;
				}
			}
			catch (Exception ex)
			{
				Game1.log.Error("Failed loading special order '" + key + "'.", ex);
			}
			return null;
		}

		// Token: 0x06001A6F RID: 6767 RVA: 0x001393F4 File Offset: 0x001375F4
		public static string MakeLocalizationReplacements(string data)
		{
			data = data.Trim();
			for (;;)
			{
				int open_index = data.LastIndexOf('[');
				if (open_index >= 0)
				{
					int close_index = data.IndexOf(']', open_index);
					if (close_index == -1)
					{
						break;
					}
					string inner = data.Substring(open_index + 1, close_index - open_index - 1);
					string value = Game1.content.LoadString("Strings\\SpecialOrderStrings:" + inner);
					data = data.Remove(open_index, close_index - open_index + 1);
					data = data.Insert(open_index, value);
				}
				if (open_index < 0)
				{
					return data;
				}
			}
			return data;
		}

		// Token: 0x06001A70 RID: 6768 RVA: 0x00139468 File Offset: 0x00137668
		public virtual string Parse(string data)
		{
			data = data.Trim();
			this.GetData();
			data = SpecialOrder.MakeLocalizationReplacements(data);
			for (;;)
			{
				int open_index = data.LastIndexOf('{');
				if (open_index >= 0)
				{
					int close_index = data.IndexOf('}', open_index);
					if (close_index == -1)
					{
						break;
					}
					string inner = data.Substring(open_index + 1, close_index - open_index - 1);
					string value = inner;
					string key = inner;
					string subkey = null;
					if (inner.Contains(':'))
					{
						string[] split = inner.Split(':', StringSplitOptions.None);
						key = split[0];
						if (split.Length > 1)
						{
							subkey = split[1];
						}
					}
					if (this._orderData.RandomizedElements != null)
					{
						string itemId;
						int index;
						if (this.preSelectedItems.TryGetValue(key, out itemId))
						{
							Item requested_item = ItemRegistry.Create(itemId, 1, 0, false);
							if (!(subkey == "Text"))
							{
								if (!(subkey == "TextPlural"))
								{
									if (!(subkey == "TextPluralCapitalized"))
									{
										if (!(subkey == "Tags"))
										{
											if (subkey == "Price")
											{
												Object obj = requested_item as Object;
												value = ((obj != null) ? (obj.sellToStorePrice(-1L).ToString() ?? "") : "1");
											}
										}
										else
										{
											string alternate_id = "id_" + Utility.getStandardDescriptionFromItem(requested_item, 0, '_');
											alternate_id = alternate_id.Substring(0, alternate_id.Length - 2).ToLower();
											value = alternate_id;
										}
									}
									else
									{
										value = Utility.capitalizeFirstLetter(Lexicon.makePlural(requested_item.DisplayName, false));
									}
								}
								else
								{
									value = Lexicon.makePlural(requested_item.DisplayName, false);
								}
							}
							else
							{
								value = requested_item.DisplayName;
							}
						}
						else if (this.selectedRandomElements.TryGetValue(key, out index))
						{
							foreach (RandomizedElement randomized_element in this._orderData.RandomizedElements)
							{
								if (randomized_element.Name == key)
								{
									value = SpecialOrder.MakeLocalizationReplacements(randomized_element.Values[index].Value);
									break;
								}
							}
						}
					}
					if (subkey != null)
					{
						string[] split2 = value.Split('|', StringSplitOptions.None);
						for (int i = 0; i < split2.Length; i += 2)
						{
							if (i + 1 <= split2.Length && split2[i] == subkey)
							{
								value = split2[i + 1];
								break;
							}
						}
					}
					data = data.Remove(open_index, close_index - open_index + 1);
					data = data.Insert(open_index, value);
				}
				if (open_index < 0)
				{
					return data;
				}
			}
			return data;
		}

		// Token: 0x06001A71 RID: 6769 RVA: 0x001396F0 File Offset: 0x001378F0
		public virtual SpecialOrderData GetData()
		{
			if (this._orderData == null)
			{
				SpecialOrder.TryGetData(this.questKey.Value, out this._orderData);
			}
			return this._orderData;
		}

		// Token: 0x06001A72 RID: 6770 RVA: 0x00139717 File Offset: 0x00137917
		public static bool TryGetData(string id, out SpecialOrderData data)
		{
			if (id == null)
			{
				data = null;
				return false;
			}
			return DataLoader.SpecialOrders(Game1.content).TryGetValue(id, out data);
		}

		// Token: 0x06001A73 RID: 6771 RVA: 0x00139734 File Offset: 0x00137934
		public virtual void InitializeNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.questName, "questName").AddField(this.questDescription, "questDescription").AddField(this.dueDate, "dueDate").AddField(this.objectives, "objectives").AddField(this.rewards, "rewards").AddField(this.questState, "questState").AddField(this.donatedItems, "donatedItems").AddField(this.questKey, "questKey").AddField(this.requester, "requester").AddField(this.generationSeed, "generationSeed").AddField(this.selectedRandomElements, "selectedRandomElements").AddField(this.preSelectedItems, "preSelectedItems").AddField(this.orderType, "orderType").AddField(this.specialRule, "specialRule").AddField(this.participants, "participants").AddField(this.seenParticipants, "seenParticipants").AddField(this.unclaimedRewards, "unclaimedRewards").AddField(this.donateMutex.NetFields, "donateMutex.NetFields").AddField(this.itemToRemoveOnEnd, "itemToRemoveOnEnd").AddField(this.mailToRemoveOnEnd, "mailToRemoveOnEnd").AddField(this.questDuration, "questDuration").AddField(this.readyForRemoval, "readyForRemoval");
			this.objectives.OnArrayReplaced += delegate(NetList<OrderObjective, NetRef<OrderObjective>> <p0>, IList<OrderObjective> <p1>, IList<OrderObjective> <p2>)
			{
				this._objectiveRegistrationDirty = true;
			};
			this.objectives.OnElementChanged += delegate(NetList<OrderObjective, NetRef<OrderObjective>> <p0>, int <p1>, OrderObjective <p2>, OrderObjective <p3>)
			{
				this._objectiveRegistrationDirty = true;
			};
		}

		// Token: 0x06001A74 RID: 6772 RVA: 0x001398E4 File Offset: 0x00137AE4
		protected virtual void _UpdateObjectiveRegistration()
		{
			for (int i = 0; i < this._registeredObjectives.Count; i++)
			{
				OrderObjective objective = this._registeredObjectives[i];
				if (!this.objectives.Contains(objective))
				{
					objective.Unregister();
				}
			}
			foreach (OrderObjective objective2 in this.objectives)
			{
				if (!this._registeredObjectives.Contains(objective2))
				{
					objective2.Register(this);
					this._registeredObjectives.Add(objective2);
				}
			}
		}

		// Token: 0x06001A75 RID: 6773 RVA: 0x00139988 File Offset: 0x00137B88
		public bool UsesDropBox(string box_id)
		{
			if (this.questState.Value != SpecialOrderStatus.InProgress)
			{
				return false;
			}
			foreach (OrderObjective orderObjective in this.objectives)
			{
				DonateObjective donateObjective = orderObjective as DonateObjective;
				if (donateObjective != null && donateObjective.dropBox.Value == box_id)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A76 RID: 6774 RVA: 0x00139A08 File Offset: 0x00137C08
		public int GetMinimumDropBoxCapacity(string box_id)
		{
			int minimum_capacity = 9;
			foreach (OrderObjective orderObjective in this.objectives)
			{
				DonateObjective donateObjective = orderObjective as DonateObjective;
				if (donateObjective != null && donateObjective.dropBox.Value == box_id && donateObjective.minimumCapacity.Value > 0)
				{
					minimum_capacity = Math.Max(minimum_capacity, donateObjective.minimumCapacity.Value);
				}
			}
			return minimum_capacity;
		}

		// Token: 0x06001A77 RID: 6775 RVA: 0x00139A94 File Offset: 0x00137C94
		public virtual void Update()
		{
			this._AddSpecialRulesIfNecessary();
			if (this._objectiveRegistrationDirty)
			{
				this._objectiveRegistrationDirty = false;
				this._UpdateObjectiveRegistration();
			}
			if (!this.readyForRemoval.Value)
			{
				SpecialOrderStatus value = this.questState.Value;
				if (value != SpecialOrderStatus.InProgress)
				{
					if (value == SpecialOrderStatus.Complete)
					{
						if (this.unclaimedRewards.Remove(Game1.player.UniqueMultiplayerID))
						{
							Stats stats = Game1.stats;
							uint questsCompleted = stats.QuestsCompleted;
							stats.QuestsCompleted = questsCompleted + 1U;
							Game1.playSound("questcomplete", null);
							Game1.dayTimeMoneyBox.questsDirty = true;
							if (this.orderType.Value == "" && !this.questKey.Value.Contains("QiChallenge") && !this.questKey.Value.Contains("DesertFestival"))
							{
								Game1.player.stats.Increment("specialOrderPrizeTickets", 1U);
							}
							foreach (OrderReward orderReward in this.rewards)
							{
								orderReward.Grant();
							}
						}
						if (this.participants.ContainsKey(Game1.player.UniqueMultiplayerID) && this.GetMoneyReward() <= 0)
						{
							this.RemoveFromParticipants();
						}
					}
				}
				else
				{
					this.participants.TryAdd(Game1.player.UniqueMultiplayerID, true);
				}
			}
			this.donateMutex.Update(Game1.getOnlineFarmers());
			if (this.donateMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				this.donateMutex.ReleaseLock();
			}
			if (Game1.activeClickableMenu == null)
			{
				this._highlightLookup = null;
			}
			if (Game1.IsMasterGame && this.questState.Value != SpecialOrderStatus.InProgress)
			{
				this.MarkForRemovalIfEmpty();
				if (this.readyForRemoval.Value)
				{
					this._RemoveSpecialRuleIfNecessary();
					Game1.player.team.specialOrders.Remove(this);
				}
			}
		}

		// Token: 0x06001A78 RID: 6776 RVA: 0x00139C94 File Offset: 0x00137E94
		public virtual void RemoveFromParticipants()
		{
			this.participants.Remove(Game1.player.UniqueMultiplayerID);
			this.MarkForRemovalIfEmpty();
		}

		// Token: 0x06001A79 RID: 6777 RVA: 0x00139CB2 File Offset: 0x00137EB2
		public virtual void MarkForRemovalIfEmpty()
		{
			if (this.participants.Length == 0)
			{
				this.readyForRemoval.Value = true;
			}
		}

		// Token: 0x06001A7A RID: 6778 RVA: 0x00139CD0 File Offset: 0x00137ED0
		public virtual void HostHandleQuestEnd()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (this.itemToRemoveOnEnd.Value != null && !Game1.player.team.itemsToRemoveOvernight.Contains(this.itemToRemoveOnEnd.Value))
			{
				Game1.player.team.itemsToRemoveOvernight.Add(this.itemToRemoveOnEnd.Value);
			}
			if (this.mailToRemoveOnEnd.Value != null && !Game1.player.team.mailToRemoveOvernight.Contains(this.mailToRemoveOnEnd.Value))
			{
				Game1.player.team.mailToRemoveOvernight.Add(this.mailToRemoveOnEnd.Value);
			}
		}

		// Token: 0x06001A7B RID: 6779 RVA: 0x00139D80 File Offset: 0x00137F80
		protected void _AddSpecialRulesIfNecessary()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (this.appliedSpecialRules)
			{
				return;
			}
			if (this.questState.Value != SpecialOrderStatus.InProgress)
			{
				return;
			}
			this.appliedSpecialRules = true;
			string[] array = this.specialRule.Value.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string formatted_rule = array[i].Trim();
				if (!Game1.player.team.SpecialOrderRuleActive(formatted_rule, this))
				{
					this.AddSpecialRule(formatted_rule);
					if (Game1.player.team.specialRulesRemovedToday.Contains(formatted_rule))
					{
						Game1.player.team.specialRulesRemovedToday.Remove(formatted_rule);
					}
				}
			}
		}

		// Token: 0x06001A7C RID: 6780 RVA: 0x00139E28 File Offset: 0x00138028
		protected void _RemoveSpecialRuleIfNecessary()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (!this.appliedSpecialRules)
			{
				return;
			}
			this.appliedSpecialRules = false;
			string[] array = this.specialRule.Value.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string formatted_rule = array[i].Trim();
				if (!Game1.player.team.SpecialOrderRuleActive(formatted_rule, this))
				{
					this.RemoveSpecialRule(formatted_rule);
					if (!Game1.player.team.specialRulesRemovedToday.Contains(formatted_rule))
					{
						Game1.player.team.specialRulesRemovedToday.Add(formatted_rule);
					}
				}
			}
		}

		// Token: 0x06001A7D RID: 6781 RVA: 0x00139EC0 File Offset: 0x001380C0
		public virtual void AddSpecialRule(string rule)
		{
			if (rule == "MINE_HARD")
			{
				Game1.netWorldState.Value.MinesDifficulty++;
				Game1.player.team.kickOutOfMinesEvent.Fire(120);
				Game1.netWorldState.Value.LowestMineLevelForOrder = 0;
				return;
			}
			if (!(rule == "SC_HARD"))
			{
				return;
			}
			Game1.netWorldState.Value.SkullCavesDifficulty++;
			Game1.player.team.kickOutOfMinesEvent.Fire(121);
		}

		// Token: 0x06001A7E RID: 6782 RVA: 0x00139F54 File Offset: 0x00138154
		public static void RemoveSpecialRuleAtEndOfDay(string rule)
		{
			if (!(rule == "MINE_HARD"))
			{
				if (!(rule == "SC_HARD"))
				{
					if (!(rule == "QI_COOKING"))
					{
						return;
					}
					Utility.ForEachItem(delegate(Item item)
					{
						Object obj = item as Object;
						if (obj != null && obj.orderData.Value == "QI_COOKING")
						{
							obj.orderData.Value = null;
							obj.MarkContextTagsDirty();
						}
						return true;
					});
				}
				else if (Game1.netWorldState.Value.SkullCavesDifficulty > 0)
				{
					Game1.netWorldState.Value.SkullCavesDifficulty--;
					return;
				}
				return;
			}
			if (Game1.netWorldState.Value.MinesDifficulty > 0)
			{
				Game1.netWorldState.Value.MinesDifficulty--;
			}
			Game1.netWorldState.Value.LowestMineLevelForOrder = -1;
		}

		// Token: 0x06001A7F RID: 6783 RVA: 0x0013A014 File Offset: 0x00138214
		public virtual void RemoveSpecialRule(string rule)
		{
			if (rule == "QI_BEANS")
			{
				Game1.player.team.itemsToRemoveOvernight.Add("890");
				Game1.player.team.itemsToRemoveOvernight.Add("889");
			}
		}

		// Token: 0x06001A80 RID: 6784 RVA: 0x0013A060 File Offset: 0x00138260
		public virtual bool HasMoneyReward()
		{
			return this.questState.Value == SpecialOrderStatus.Complete && this.GetMoneyReward() > 0 && this.participants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		// Token: 0x06001A81 RID: 6785 RVA: 0x0013A090 File Offset: 0x00138290
		public virtual void Fail()
		{
		}

		// Token: 0x06001A82 RID: 6786 RVA: 0x0013A092 File Offset: 0x00138292
		public virtual void AddObjective(OrderObjective objective)
		{
			this.objectives.Add(objective);
		}

		// Token: 0x06001A83 RID: 6787 RVA: 0x0013A0A0 File Offset: 0x001382A0
		public void CheckCompletion()
		{
			if (this.questState.Value != SpecialOrderStatus.InProgress)
			{
				return;
			}
			foreach (OrderObjective objective in this.objectives)
			{
				if (objective.failOnCompletion.Value && objective.IsComplete())
				{
					this.OnFail();
					return;
				}
			}
			foreach (OrderObjective objective2 in this.objectives)
			{
				if (!objective2.failOnCompletion.Value && !objective2.IsComplete())
				{
					return;
				}
			}
			if (Game1.IsMasterGame)
			{
				foreach (long farmer_id in this.participants.Keys)
				{
					this.unclaimedRewards.TryAdd(farmer_id, true);
				}
				Game1.multiplayer.globalChatInfoMessage("CompletedSpecialOrder", new string[]
				{
					TokenStringBuilder.SpecialOrderName(this.questKey.Value)
				});
				this.HostHandleQuestEnd();
				Game1.player.team.completedSpecialOrders.Add(this.questKey.Value);
				this.questState.Value = SpecialOrderStatus.Complete;
				this._RemoveSpecialRuleIfNecessary();
			}
		}

		// Token: 0x06001A84 RID: 6788 RVA: 0x0013A230 File Offset: 0x00138430
		public override string ToString()
		{
			string temp = "";
			foreach (OrderObjective objective in this.objectives)
			{
				temp += objective.description.Value;
				if (objective.GetMaxCount() > 1)
				{
					temp = string.Concat(new string[]
					{
						temp,
						" (",
						objective.GetCount().ToString(),
						"/",
						objective.GetMaxCount().ToString(),
						")"
					});
				}
				temp += "\n";
			}
			return temp.Trim();
		}

		// Token: 0x06001A85 RID: 6789 RVA: 0x0013A2FC File Offset: 0x001384FC
		public string GetName()
		{
			if (this._localizedName == null)
			{
				this._localizedName = SpecialOrder.MakeLocalizationReplacements(this.questName.Value);
			}
			return this._localizedName;
		}

		// Token: 0x06001A86 RID: 6790 RVA: 0x0013A322 File Offset: 0x00138522
		public string GetDescription()
		{
			if (this._localizedDescription == null)
			{
				this._localizedDescription = this.Parse(this.questDescription.Value).Trim();
			}
			return this._localizedDescription;
		}

		// Token: 0x06001A87 RID: 6791 RVA: 0x0013A350 File Offset: 0x00138550
		public List<string> GetObjectiveDescriptions()
		{
			List<string> objective_descriptions = new List<string>();
			foreach (OrderObjective objective in this.objectives)
			{
				objective_descriptions.Add(this.Parse(objective.GetDescription()));
			}
			return objective_descriptions;
		}

		// Token: 0x06001A88 RID: 6792 RVA: 0x0013A3B8 File Offset: 0x001385B8
		public bool CanBeCancelled()
		{
			return false;
		}

		// Token: 0x06001A89 RID: 6793 RVA: 0x0013A3BB File Offset: 0x001385BB
		public void MarkAsViewed()
		{
			this.seenParticipants.TryAdd(Game1.player.UniqueMultiplayerID, true);
		}

		// Token: 0x06001A8A RID: 6794 RVA: 0x0013A3D4 File Offset: 0x001385D4
		public bool IsHidden()
		{
			return !this.participants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		// Token: 0x06001A8B RID: 6795 RVA: 0x0013A3EE File Offset: 0x001385EE
		public bool ShouldDisplayAsNew()
		{
			return !this.seenParticipants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		// Token: 0x06001A8C RID: 6796 RVA: 0x0013A408 File Offset: 0x00138608
		public bool HasReward()
		{
			return this.HasMoneyReward();
		}

		// Token: 0x06001A8D RID: 6797 RVA: 0x0013A410 File Offset: 0x00138610
		public int GetMoneyReward()
		{
			if (this._moneyReward == -1)
			{
				this._moneyReward = 0;
				foreach (OrderReward orderReward in this.rewards)
				{
					MoneyReward moneyReward = orderReward as MoneyReward;
					if (moneyReward != null)
					{
						this._moneyReward += moneyReward.GetRewardMoneyAmount();
					}
				}
			}
			return this._moneyReward;
		}

		// Token: 0x06001A8E RID: 6798 RVA: 0x0013A490 File Offset: 0x00138690
		public bool ShouldDisplayAsComplete()
		{
			return this.questState.Value > SpecialOrderStatus.InProgress;
		}

		// Token: 0x06001A8F RID: 6799 RVA: 0x0013A4A0 File Offset: 0x001386A0
		public bool IsTimedQuest()
		{
			return true;
		}

		// Token: 0x06001A90 RID: 6800 RVA: 0x0013A4A3 File Offset: 0x001386A3
		public int GetDaysLeft()
		{
			if (this.questState.Value != SpecialOrderStatus.InProgress)
			{
				return 0;
			}
			return this.dueDate.Value - Game1.Date.TotalDays;
		}

		// Token: 0x06001A91 RID: 6801 RVA: 0x0013A4CA File Offset: 0x001386CA
		public void OnMoneyRewardClaimed()
		{
			this.participants.Remove(Game1.player.UniqueMultiplayerID);
			this.MarkForRemovalIfEmpty();
		}

		// Token: 0x06001A92 RID: 6802 RVA: 0x0013A4E8 File Offset: 0x001386E8
		public bool OnLeaveQuestPage()
		{
			if (!this.participants.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				this.MarkForRemovalIfEmpty();
				return true;
			}
			return false;
		}

		// Token: 0x04001039 RID: 4153
		[XmlIgnore]
		public Action<Farmer, Item, int> onItemShipped;

		// Token: 0x0400103A RID: 4154
		[XmlIgnore]
		public Action<Farmer, Monster> onMonsterSlain;

		// Token: 0x0400103B RID: 4155
		[XmlIgnore]
		public Action<Farmer, Item> onFishCaught;

		// Token: 0x0400103C RID: 4156
		[XmlIgnore]
		public Action<Farmer, NPC, Item> onGiftGiven;

		// Token: 0x0400103D RID: 4157
		[XmlIgnore]
		public Func<Farmer, NPC, Item, bool, int> onItemDelivered;

		// Token: 0x0400103E RID: 4158
		[XmlIgnore]
		public Action<Farmer, Item> onItemCollected;

		// Token: 0x0400103F RID: 4159
		[XmlIgnore]
		public Action<Farmer, int> onMineFloorReached;

		// Token: 0x04001040 RID: 4160
		[XmlIgnore]
		public Action<Farmer, int> onJKScoreAchieved;

		// Token: 0x04001041 RID: 4161
		[XmlIgnore]
		protected bool _objectiveRegistrationDirty;

		// Token: 0x04001042 RID: 4162
		[XmlElement("preSelectedItems")]
		public NetStringDictionary<string, NetString> preSelectedItems = new NetStringDictionary<string, NetString>();

		// Token: 0x04001043 RID: 4163
		[XmlElement("selectedRandomElements")]
		public NetStringDictionary<int, NetInt> selectedRandomElements = new NetStringDictionary<int, NetInt>();

		// Token: 0x04001044 RID: 4164
		[XmlElement("objectives")]
		public NetList<OrderObjective, NetRef<OrderObjective>> objectives = new NetList<OrderObjective, NetRef<OrderObjective>>();

		// Token: 0x04001045 RID: 4165
		[XmlElement("generationSeed")]
		public NetInt generationSeed = new NetInt();

		// Token: 0x04001046 RID: 4166
		[XmlElement("seenParticipantsIDs")]
		public NetLongDictionary<bool, NetBool> seenParticipants = new NetLongDictionary<bool, NetBool>();

		// Token: 0x04001047 RID: 4167
		[XmlElement("participantsIDs")]
		public NetLongDictionary<bool, NetBool> participants = new NetLongDictionary<bool, NetBool>();

		// Token: 0x04001048 RID: 4168
		[XmlElement("unclaimedRewardsIDs")]
		public NetLongDictionary<bool, NetBool> unclaimedRewards = new NetLongDictionary<bool, NetBool>();

		// Token: 0x04001049 RID: 4169
		[XmlElement("donatedItems")]
		public readonly NetCollection<Item> donatedItems = new NetCollection<Item>();

		// Token: 0x0400104A RID: 4170
		[XmlElement("appliedSpecialRules")]
		public bool appliedSpecialRules;

		// Token: 0x0400104B RID: 4171
		[XmlIgnore]
		public readonly NetMutex donateMutex = new NetMutex();

		// Token: 0x0400104C RID: 4172
		[XmlIgnore]
		protected int _isIslandOrder = -1;

		// Token: 0x0400104D RID: 4173
		[XmlElement("rewards")]
		public NetList<OrderReward, NetRef<OrderReward>> rewards = new NetList<OrderReward, NetRef<OrderReward>>();

		// Token: 0x0400104E RID: 4174
		[XmlIgnore]
		protected int _moneyReward = -1;

		// Token: 0x0400104F RID: 4175
		[XmlElement("questKey")]
		public NetString questKey = new NetString();

		// Token: 0x04001050 RID: 4176
		[XmlElement("questName")]
		public NetString questName = new NetString("Strings\\SpecialOrders:PlaceholderName");

		// Token: 0x04001051 RID: 4177
		[XmlElement("questDescription")]
		public NetString questDescription = new NetString("Strings\\SpecialOrders:PlaceholderDescription");

		// Token: 0x04001052 RID: 4178
		[XmlElement("requester")]
		public NetString requester = new NetString();

		// Token: 0x04001053 RID: 4179
		[XmlElement("orderType")]
		public NetString orderType = new NetString("");

		// Token: 0x04001054 RID: 4180
		[XmlElement("specialRule")]
		public NetString specialRule = new NetString("");

		// Token: 0x04001055 RID: 4181
		[XmlElement("readyForRemoval")]
		public NetBool readyForRemoval = new NetBool(false);

		// Token: 0x04001056 RID: 4182
		[XmlElement("itemToRemoveOnEnd")]
		public NetString itemToRemoveOnEnd = new NetString();

		// Token: 0x04001057 RID: 4183
		[XmlElement("mailToRemoveOnEnd")]
		public NetString mailToRemoveOnEnd = new NetString();

		// Token: 0x04001058 RID: 4184
		[XmlIgnore]
		protected string _localizedName;

		// Token: 0x04001059 RID: 4185
		[XmlIgnore]
		protected string _localizedDescription;

		// Token: 0x0400105A RID: 4186
		[XmlElement("dueDate")]
		public NetInt dueDate = new NetInt();

		// Token: 0x0400105B RID: 4187
		[XmlElement("duration")]
		public NetEnum<QuestDuration> questDuration = new NetEnum<QuestDuration>();

		// Token: 0x0400105D RID: 4189
		[XmlIgnore]
		protected List<OrderObjective> _registeredObjectives = new List<OrderObjective>();

		// Token: 0x0400105E RID: 4190
		[XmlIgnore]
		protected Dictionary<Item, bool> _highlightLookup;

		// Token: 0x0400105F RID: 4191
		[XmlIgnore]
		protected SpecialOrderData _orderData;

		// Token: 0x04001060 RID: 4192
		[XmlElement("questState")]
		public NetEnum<SpecialOrderStatus> questState = new NetEnum<SpecialOrderStatus>(SpecialOrderStatus.InProgress);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Network;

namespace StardewValley.Quests
{
	// Token: 0x0200018F RID: 399
	public class ItemDeliveryQuest : Quest
	{
		// Token: 0x06001CA9 RID: 7337 RVA: 0x00147EEC File Offset: 0x001460EC
		public ItemDeliveryQuest()
		{
			this.questType.Value = 3;
		}

		// Token: 0x06001CAA RID: 7338 RVA: 0x00147F4E File Offset: 0x0014614E
		public ItemDeliveryQuest(string target, string itemId) : this()
		{
			this.target.Value = target;
			this.ItemId.Value = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x00147F78 File Offset: 0x00146178
		public ItemDeliveryQuest(string target, string itemId, string questTitle, string questDescription, string objective, string returnDialogue) : this(target, itemId)
		{
			base.questDescription = questDescription;
			base.questTitle = questTitle;
			this._loadedTitle = true;
			this.targetMessage = returnDialogue;
			this.objective = new NetDescriptionElementRef(new DescriptionElement(objective, Array.Empty<object>()));
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x00147FB8 File Offset: 0x001461B8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.target, "target").AddField(this.ItemId, "ItemId").AddField(this.number, "number").AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective");
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x00148034 File Offset: 0x00146234
		public List<NPC> GetValidTargetList()
		{
			Farmer[] source = Game1.getAllFarmers().ToArray<Farmer>();
			HashSet<string> friendshipKeys = new HashSet<string>(source.SelectMany((Farmer player) => player.friendshipData.Keys));
			HashSet<string> spouses = new HashSet<string>(from p in source
			select p.spouse);
			List<NPC> validTargets = new List<NPC>();
			foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
			{
				CharacterData data = pair.Value;
				if (GameStateQuery.CheckConditions(data.CanSocialize, null, null, null, null, null, null) && ((data.ItemDeliveryQuests != null) ? GameStateQuery.CheckConditions(data.ItemDeliveryQuests, null, null, null, null, null, null) : (data.HomeRegion == "Town")) && friendshipKeys.Contains(pair.Key) && !spouses.Contains(pair.Key) && pair.Value.Age != NpcAge.Child)
				{
					NPC npc = Game1.getCharacterFromName(pair.Key, true, false);
					if (npc != null && !npc.IsInvisible)
					{
						validTargets.Add(npc);
					}
				}
			}
			return validTargets;
		}

		// Token: 0x06001CAE RID: 7342 RVA: 0x00148188 File Offset: 0x00146388
		public void loadQuestInfo()
		{
			if (this.target.Value != null)
			{
				return;
			}
			Random random = base.CreateInitializationRandom();
			List<NPC> valid_targets = this.GetValidTargetList();
			NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = Game1.player.friendshipData;
			if (friendshipData == null || friendshipData.Length <= 0 || valid_targets.Count <= 0)
			{
				return;
			}
			NPC actualTarget = valid_targets[random.Next(valid_targets.Count)];
			if (actualTarget == null)
			{
				return;
			}
			this.target.Value = actualTarget.name.Value;
			if (this.target.Value.Equals("Wizard") && !Game1.player.mailReceived.Contains("wizardJunimoNote") && !Game1.player.mailReceived.Contains("JojaMember"))
			{
				this.target.Value = "Demetrius";
				actualTarget = Game1.getCharacterFromName(this.target.Value, true, false);
			}
			base.questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ItemDeliveryQuestTitle", NPC.GetDisplayName(this.target.Value));
			Item item;
			string value;
			if (Game1.season != Season.Winter && random.NextDouble() < 0.15)
			{
				this.ItemId.Value = random.ChooseFrom(Utility.possibleCropsAtThisTime(Game1.season, Game1.dayOfMonth <= 7));
				this.ItemId.Value = (ItemRegistry.QualifyItemId(this.ItemId.Value) ?? this.ItemId.Value);
				item = ItemRegistry.Create(this.ItemId.Value, 1, 0, false);
				if (this.dailyQuest.Value || this.moneyReward.Value == 0)
				{
					this.moneyReward.Value = this.GetGoldRewardPerItem(item);
				}
				value = this.target.Value;
				if (!(value == "Demetrius"))
				{
					if (!(value == "Marnie"))
					{
						if (!(value == "Sebastian"))
						{
							this.parts.Clear();
							this.parts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13299", "13300", "13301"));
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13302", "13303", "13304"), new object[]
							{
								item
							}));
							this.parts.Add(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13306", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13307", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13308"));
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
							{
								actualTarget
							}));
						}
						else
						{
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13324", "13327"), new object[]
							{
								item
							}));
						}
					}
					else
					{
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13317", "13320"), new object[]
						{
							item
						}));
					}
				}
				else
				{
					this.parts.Clear();
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13311", "13314"), new object[]
					{
						item
					}));
				}
			}
			else
			{
				string rawId = Utility.getRandomItemFromSeason(Game1.season, 1000, true, true);
				if (!(rawId == "-5"))
				{
					if (!(rawId == "-6"))
					{
						this.ItemId.Value = (ItemRegistry.QualifyItemId(rawId) ?? rawId);
					}
					else
					{
						this.ItemId.Value = "(O)184";
					}
				}
				else
				{
					this.ItemId.Value = "(O)176";
				}
				item = ItemRegistry.Create(this.ItemId.Value, 1, 0, false);
				if (this.dailyQuest.Value || this.moneyReward.Value == 0)
				{
					this.moneyReward.Value = this.GetGoldRewardPerItem(item);
				}
				DescriptionElement[] questDescriptions = null;
				DescriptionElement[] questDescriptions2 = null;
				DescriptionElement[] questDescriptions3 = null;
				Object @object = item as Object;
				if (((@object != null) ? @object.Type : null) == "Cooking" && this.target.Value != "Wizard")
				{
					if (random.NextDouble() < 0.33)
					{
						DescriptionElement[] questStrings = new DescriptionElement[]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341", Array.Empty<object>()),
							(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156")) ? ((Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new object[]
							{
								new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157", Array.Empty<object>())
							}) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346", Array.Empty<object>())) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new object[]
							{
								new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156", Array.Empty<object>())
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13349", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13350", Array.Empty<object>()),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13351", Array.Empty<object>()),
							(Game1.season == Season.Winter) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13353", Array.Empty<object>()) : ((Game1.season == Season.Summer) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13355", Array.Empty<object>()) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13356", Array.Empty<object>())),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357", Array.Empty<object>())
						};
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13333", "13334"), new object[]
						{
							item,
							random.ChooseFrom(questStrings)
						}));
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}));
					}
					else
					{
						DescriptionElement day;
						switch (Game1.dayOfMonth % 7)
						{
						case 0:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3042", Array.Empty<object>());
							break;
						case 1:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3043", Array.Empty<object>());
							break;
						case 2:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3044", Array.Empty<object>());
							break;
						case 3:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3045", Array.Empty<object>());
							break;
						case 4:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3046", Array.Empty<object>());
							break;
						case 5:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3047", Array.Empty<object>());
							break;
						default:
							day = new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3048", Array.Empty<object>());
							break;
						}
						questDescriptions = new DescriptionElement[]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13360", new object[]
							{
								item
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13364", new object[]
							{
								item
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13367", new object[]
							{
								item
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13370", new object[]
							{
								item
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13373", new object[]
							{
								day,
								item,
								actualTarget
							})
						};
						questDescriptions2 = new DescriptionElement[]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
							{
								actualTarget
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
							{
								actualTarget
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
							{
								actualTarget
							}),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
							{
								actualTarget
							}),
							new DescriptionElement("", Array.Empty<object>())
						};
						questDescriptions3 = new DescriptionElement[]
						{
							new DescriptionElement("", Array.Empty<object>()),
							new DescriptionElement("", Array.Empty<object>()),
							new DescriptionElement("", Array.Empty<object>()),
							new DescriptionElement("", Array.Empty<object>()),
							new DescriptionElement("", Array.Empty<object>())
						};
					}
					this.parts.Clear();
					int rand = random.Next(questDescriptions.Length);
					this.parts.Add(questDescriptions[rand]);
					this.parts.Add(questDescriptions2[rand]);
					this.parts.Add(questDescriptions3[rand]);
					if (this.target.Value.Equals("Sebastian"))
					{
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13378", "13381"), new object[]
						{
							item
						}));
					}
				}
				else
				{
					if (random.NextBool())
					{
						Object object2 = item as Object;
						if (object2 != null && object2.Edibility > 0)
						{
							questDescriptions = new DescriptionElement[]
							{
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13383", new object[]
								{
									item,
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose(new string[]
									{
										"13385",
										"13386",
										"13387",
										"13388",
										"13389",
										"13390",
										"13391",
										"13392",
										"13393",
										"13394",
										"13395",
										"13396"
									}), Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13400", new object[]
									{
										item
									})
								})
							};
							questDescriptions2 = new DescriptionElement[]
							{
								new DescriptionElement(random.Choose("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13398"), Array.Empty<object>()),
								new DescriptionElement(random.Choose("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13402"), Array.Empty<object>())
							};
							questDescriptions3 = new DescriptionElement[]
							{
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
								{
									actualTarget
								}),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
								{
									actualTarget
								})
							};
							if (random.NextDouble() < 0.33)
							{
								DescriptionElement[] questStrings2 = new DescriptionElement[]
								{
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341", Array.Empty<object>()),
									(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156")) ? ((Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new object[]
									{
										new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157", Array.Empty<object>())
									}) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346", Array.Empty<object>())) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new object[]
									{
										new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156", Array.Empty<object>())
									}),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13420", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13421", Array.Empty<object>()),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13422", Array.Empty<object>()),
									(Game1.season == Season.Winter) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13424", Array.Empty<object>()) : ((Game1.season == Season.Summer) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13426", Array.Empty<object>()) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13427", Array.Empty<object>())),
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357", Array.Empty<object>())
								};
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13333", "13334"), new object[]
								{
									item,
									random.ChooseFrom(questStrings2)
								}));
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
								{
									actualTarget
								}));
							}
							else
							{
								this.parts.Clear();
								int rand2 = random.Next(questDescriptions.Length);
								this.parts.Add(questDescriptions[rand2]);
								this.parts.Add(questDescriptions2[rand2]);
								this.parts.Add(questDescriptions3[rand2]);
							}
							value = this.target.Value;
							if (value == "Demetrius")
							{
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13311", "13314"), new object[]
								{
									item
								}));
								goto IL_134B;
							}
							if (value == "Marnie")
							{
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13317", "13320"), new object[]
								{
									item
								}));
								goto IL_134B;
							}
							if (value == "Harvey")
							{
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13446", new object[]
								{
									item,
									new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose(new string[]
									{
										"13448",
										"13449",
										"13450",
										"13451",
										"13452",
										"13453",
										"13454",
										"13455",
										"13456",
										"13457",
										"13458",
										"13459"
									}), Array.Empty<object>())
								}));
								goto IL_134B;
							}
							if (!(value == "Gus"))
							{
								goto IL_134B;
							}
							if (random.NextDouble() < 0.6)
							{
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13462", new object[]
								{
									item
								}));
								goto IL_134B;
							}
							goto IL_134B;
						}
					}
					if (random.NextBool())
					{
						Object object3 = item as Object;
						if (object3 == null || object3.Edibility < 0)
						{
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13464", new object[]
							{
								item,
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose(new string[]
								{
									"13465",
									"13466",
									"13467",
									"13468",
									"13469"
								}), Array.Empty<object>())
							}));
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
							{
								actualTarget
							}));
							if (this.target.Value.Equals("Emily"))
							{
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13473", "13476"), new object[]
								{
									item
								}));
								goto IL_134B;
							}
							goto IL_134B;
						}
					}
					questDescriptions = new DescriptionElement[]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13480", new object[]
						{
							actualTarget,
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13481", new object[]
						{
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13485", new object[]
						{
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13491", "13492"), new object[]
						{
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", new object[]
						{
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13497", new object[]
						{
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13500", new object[]
						{
							item,
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose(new string[]
							{
								"13502",
								"13503",
								"13504",
								"13505",
								"13506",
								"13507",
								"13508",
								"13509",
								"13510",
								"13511",
								"13512",
								"13513"
							}), Array.Empty<object>())
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13518", new object[]
						{
							actualTarget,
							item
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13520", "13523"), new object[]
						{
							item
						})
					};
					questDescriptions2 = new DescriptionElement[]
					{
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13482", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13483"), Array.Empty<object>()),
						new DescriptionElement(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13487", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13488", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13489"), Array.Empty<object>()),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13514", "13516"), Array.Empty<object>()),
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						})
					};
					questDescriptions3 = new DescriptionElement[]
					{
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}),
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", new object[]
						{
							actualTarget
						}),
						new DescriptionElement("", Array.Empty<object>()),
						new DescriptionElement("", Array.Empty<object>())
					};
					this.parts.Clear();
					int rand3 = random.Next(questDescriptions.Length);
					this.parts.Add(questDescriptions[rand3]);
					this.parts.Add(questDescriptions2[rand3]);
					this.parts.Add(questDescriptions3[rand3]);
				}
			}
			IL_134B:
			this.dialogueparts.Clear();
			this.dialogueparts.Add((random.NextBool(0.3) || this.target.Value == "Evelyn") ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13526", Array.Empty<object>()) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13527", "13528"), Array.Empty<object>()));
			this.dialogueparts.Add(random.NextBool(0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13530", new object[]
			{
				item
			}) : (random.NextBool() ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13532", Array.Empty<object>()) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13534", "13535", "13536"), Array.Empty<object>())));
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13538", "13539", "13540"));
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13542", "13543", "13544"));
			value = this.target.Value;
			if (value != null)
			{
				switch (value.Length)
				{
				case 3:
					if (value == "Sam")
					{
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13568", "13571"), new object[]
						{
							item
						}));
						this.dialogueparts.Clear();
						this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13577", Array.Empty<object>()));
					}
					break;
				case 4:
					if (value == "Maru")
					{
						bool rand4 = random.NextBool();
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand4 ? "13580" : "13583"), new object[]
						{
							item
						}));
						this.dialogueparts.Clear();
						this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand4 ? "13585" : "13587"), Array.Empty<object>()));
					}
					break;
				case 5:
					if (value == "Haley")
					{
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13557", "13560"), new object[]
						{
							item
						}));
						this.dialogueparts.Clear();
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13566");
					}
					break;
				case 6:
					if (value == "Wizard")
					{
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13546", "13548", "13551", "13553"), new object[]
						{
							item
						}));
						this.dialogueparts.Clear();
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13555");
					}
					break;
				case 7:
				{
					char c = value[0];
					if (c != 'A')
					{
						if (c == 'E')
						{
							if (value == "Elliott")
							{
								this.dialogueparts.Clear();
								this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13604", new object[]
								{
									item
								}));
							}
						}
					}
					else if (value == "Abigail")
					{
						bool rand5 = random.NextBool();
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand5 ? "13590" : "13593"), new object[]
						{
							item
						}));
						this.dialogueparts.Clear();
						this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand5 ? "13597" : "13599"), Array.Empty<object>()));
					}
					break;
				}
				case 9:
					if (value == "Sebastian")
					{
						this.dialogueparts.Clear();
						this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13602");
					}
					break;
				}
			}
			DescriptionElement lastPart = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13608", "13610", "13612"), new object[]
			{
				actualTarget
			});
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", new object[]
			{
				this.moneyReward.Value
			}));
			this.parts.Add(lastPart);
			this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13614", new object[]
			{
				actualTarget,
				item
			});
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x00149A28 File Offset: 0x00147C28
		public override void reloadDescription()
		{
			if (this._questDescription == "")
			{
				this.loadQuestInfo();
			}
			string descriptionBuilder = "";
			string messageBuilder = "";
			if (this.parts != null && this.parts.Count != 0)
			{
				foreach (DescriptionElement a in this.parts)
				{
					descriptionBuilder += a.loadDescriptionElement();
				}
				base.questDescription = descriptionBuilder;
			}
			if (this.dialogueparts != null && this.dialogueparts.Count != 0)
			{
				foreach (DescriptionElement b in this.dialogueparts)
				{
					messageBuilder += b.loadDescriptionElement();
				}
				this.targetMessage = messageBuilder;
				return;
			}
			if (base.HasId())
			{
				string[] fields = Quest.GetRawQuestFields(this.id.Value);
				this.targetMessage = ArgUtility.Get(fields, 9, this.targetMessage, false);
			}
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x00149B58 File Offset: 0x00147D58
		public override void reloadObjective()
		{
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}

		// Token: 0x06001CB1 RID: 7345 RVA: 0x00149B80 File Offset: 0x00147D80
		public override bool OnItemOfferedToNpc(NPC npc, Item item, bool probe = false)
		{
			bool baseChanged = base.OnItemOfferedToNpc(npc, item, probe);
			if (this.completed.Value)
			{
				return false;
			}
			if (npc.IsVillager && npc.Name == this.target.Value && item.QualifiedItemId == this.ItemId.Value)
			{
				if (item.Stack >= this.number.Value)
				{
					if (!probe)
					{
						Game1.player.Items.Reduce(item, this.number.Value, false);
						this.reloadDescription();
						npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage));
						Game1.drawDialogue(npc);
						if (this.dailyQuest.Value)
						{
							Game1.player.changeFriendship(150, npc);
						}
						else
						{
							Game1.player.changeFriendship(255, npc);
						}
						this.questComplete();
					}
					return true;
				}
				if (!probe)
				{
					npc.CurrentDialogue.Push(Dialogue.FromTranslation(npc, "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13615", this.number.Value));
					Game1.drawDialogue(npc);
				}
			}
			return baseChanged;
		}

		// Token: 0x06001CB2 RID: 7346 RVA: 0x00149CA8 File Offset: 0x00147EA8
		public int GetGoldRewardPerItem(Item item)
		{
			Object obj = item as Object;
			if (obj != null)
			{
				return obj.Price * 3;
			}
			return (int)((float)item.salePrice(false) * 1.5f);
		}

		// Token: 0x0400117F RID: 4479
		public string targetMessage;

		// Token: 0x04001180 RID: 4480
		[XmlElement("target")]
		public readonly NetString target = new NetString();

		// Token: 0x04001181 RID: 4481
		[XmlElement("item")]
		public readonly NetString ItemId = new NetString();

		// Token: 0x04001182 RID: 4482
		[XmlElement("number")]
		public readonly NetInt number = new NetInt(1);

		// Token: 0x04001183 RID: 4483
		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		// Token: 0x04001184 RID: 4484
		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		// Token: 0x04001185 RID: 4485
		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();
	}
}

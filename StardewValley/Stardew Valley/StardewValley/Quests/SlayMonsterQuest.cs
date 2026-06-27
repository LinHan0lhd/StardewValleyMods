using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace StardewValley.Quests
{
	// Token: 0x02000196 RID: 406
	public class SlayMonsterQuest : Quest
	{
		// Token: 0x06001D0C RID: 7436 RVA: 0x0014C4F0 File Offset: 0x0014A6F0
		public SlayMonsterQuest()
		{
			this.questType.Value = 4;
		}

		// Token: 0x06001D0D RID: 7437 RVA: 0x0014C580 File Offset: 0x0014A780
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective").AddField(this.monsterName, "monsterName").AddField(this.target, "target").AddField(this.monster, "monster").AddField(this.numberToKill, "numberToKill").AddField(this.reward, "reward").AddField(this.numberKilled, "numberKilled").AddField(this.ignoreFarmMonsters, "ignoreFarmMonsters");
		}

		// Token: 0x06001D0E RID: 7438 RVA: 0x0014C63C File Offset: 0x0014A83C
		public void loadQuestInfo()
		{
			if (this.target.Value != null && this.monster != null)
			{
				return;
			}
			Random random = base.CreateInitializationRandom();
			for (int i = 0; i < random.Next(1, 100); i++)
			{
				random.Next();
			}
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
			List<string> possibleMonsters = new List<string>();
			int mineLevel = Utility.GetAllPlayerDeepestMineLevel();
			if (mineLevel < 39)
			{
				possibleMonsters.Add("Green Slime");
				if (mineLevel > 10)
				{
					possibleMonsters.Add("Rock Crab");
				}
				if (mineLevel > 30)
				{
					possibleMonsters.Add("Duggy");
				}
			}
			else if (mineLevel < 79)
			{
				possibleMonsters.Add("Frost Jelly");
				if (mineLevel > 70)
				{
					possibleMonsters.Add("Skeleton");
				}
				possibleMonsters.Add("Dust Spirit");
			}
			else
			{
				possibleMonsters.Add("Sludge");
				possibleMonsters.Add("Ghost");
				possibleMonsters.Add("Lava Crab");
				possibleMonsters.Add("Squid Kid");
			}
			bool flag = this.monsterName.Value == null || this.numberToKill.Value == 0;
			if (flag)
			{
				this.monsterName.Value = random.ChooseFrom(possibleMonsters);
			}
			if (this.monsterName.Value == "Frost Jelly" || this.monsterName.Value == "Sludge")
			{
				this.monster.Value = new Monster("Green Slime", Vector2.Zero);
				this.monster.Value.Name = this.monsterName.Value;
			}
			else
			{
				this.monster.Value = new Monster(this.monsterName.Value, Vector2.Zero);
			}
			string value;
			if (flag)
			{
				value = this.monsterName.Value;
				if (value != null)
				{
					switch (value.Length)
					{
					case 5:
					{
						char c = value[0];
						if (c != 'D')
						{
							if (c == 'G')
							{
								if (value == "Ghost")
								{
									this.numberToKill.Value = random.Next(2, 4);
									this.reward.Value = this.numberToKill.Value * 250;
									goto IL_612;
								}
							}
						}
						else if (value == "Duggy")
						{
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13711", new object[]
							{
								this.numberToKill.Value
							}));
							this.target.Value = "Clint";
							this.numberToKill.Value = random.Next(2, 4);
							this.reward.Value = this.numberToKill.Value * 150;
							goto IL_612;
						}
						break;
					}
					case 6:
						if (value == "Sludge")
						{
							this.numberToKill.Value = random.Next(4, 11);
							this.numberToKill.Value = this.numberToKill.Value - this.numberToKill.Value % 2;
							this.reward.Value = this.numberToKill.Value * 125;
							goto IL_612;
						}
						break;
					case 8:
						if (value == "Skeleton")
						{
							this.numberToKill.Value = random.Next(6, 12);
							this.reward.Value = this.numberToKill.Value * 100;
							goto IL_612;
						}
						break;
					case 9:
					{
						char c = value[0];
						if (c != 'L')
						{
							if (c != 'R')
							{
								if (c == 'S')
								{
									if (value == "Squid Kid")
									{
										this.numberToKill.Value = random.Next(1, 3);
										this.reward.Value = this.numberToKill.Value * 350;
										goto IL_612;
									}
								}
							}
							else if (value == "Rock Crab")
							{
								this.numberToKill.Value = random.Next(2, 6);
								this.reward.Value = this.numberToKill.Value * 75;
								goto IL_612;
							}
						}
						else if (value == "Lava Crab")
						{
							this.numberToKill.Value = random.Next(2, 6);
							this.reward.Value = this.numberToKill.Value * 180;
							goto IL_612;
						}
						break;
					}
					case 11:
						switch (value[0])
						{
						case 'D':
							if (value == "Dust Spirit")
							{
								this.numberToKill.Value = random.Next(10, 21);
								this.reward.Value = this.numberToKill.Value * 60;
								goto IL_612;
							}
							break;
						case 'F':
							if (value == "Frost Jelly")
							{
								this.numberToKill.Value = random.Next(4, 11);
								this.numberToKill.Value = this.numberToKill.Value - this.numberToKill.Value % 2;
								this.reward.Value = this.numberToKill.Value * 85;
								goto IL_612;
							}
							break;
						case 'G':
							if (value == "Green Slime")
							{
								this.numberToKill.Value = random.Next(4, 11);
								this.numberToKill.Value = this.numberToKill.Value - this.numberToKill.Value % 2;
								this.reward.Value = this.numberToKill.Value * 60;
								goto IL_612;
							}
							break;
						}
						break;
					}
				}
				this.numberToKill.Value = random.Next(3, 7);
				this.reward.Value = this.numberToKill.Value * 120;
			}
			IL_612:
			value = this.monsterName.Value;
			if (!(value == "Green Slime") && !(value == "Frost Jelly") && !(value == "Sludge"))
			{
				if (!(value == "Rock Crab") && !(value == "Lava Crab"))
				{
					this.parts.Clear();
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13752", new object[]
					{
						this.monster.Value,
						this.numberToKill.Value,
						new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13755", "13756", "13757"), Array.Empty<object>())
					}));
					this.target.Value = "Wizard";
					this.dialogueparts.Clear();
					this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13760");
				}
				else
				{
					this.parts.Clear();
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13747", new object[]
					{
						this.numberToKill.Value
					}));
					this.target.Value = "Demetrius";
					this.dialogueparts.Clear();
					this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13750", new object[]
					{
						this.monster.Value
					}));
				}
			}
			else
			{
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13723", new object[]
				{
					this.numberToKill.Value,
					this.monsterName.Value.Equals("Frost Jelly") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13725", Array.Empty<object>()) : (this.monsterName.Value.Equals("Sludge") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13727", Array.Empty<object>()) : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13728", Array.Empty<object>()))
				}));
				this.target.Value = "Lewis";
				this.dialogueparts.Clear();
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13730");
				if (random.NextBool())
				{
					this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13731");
					this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13732", "13733"));
					this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13734", new object[]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13735", "13736"), Array.Empty<object>()),
						new DescriptionElement("Strings\\StringsFromCSFiles:Dialogue.cs." + random.Choose(new string[]
						{
							"795",
							"796",
							"797",
							"798",
							"799",
							"800",
							"801",
							"802",
							"803",
							"804",
							"805",
							"806",
							"807",
							"808",
							"809",
							"810"
						}), Array.Empty<object>()),
						new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13740", "13741", "13742"), Array.Empty<object>())
					}));
				}
				else
				{
					this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13744");
				}
			}
			if (this.target.Value.Equals("Wizard") && !Utility.doesAnyFarmerHaveMail("wizardJunimoNote") && !Utility.doesAnyFarmerHaveMail("JojaMember"))
			{
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13764", new object[]
				{
					this.numberToKill.Value,
					this.monster.Value
				}));
				this.target.Value = "Lewis";
				this.dialogueparts.Clear();
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13767");
			}
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", new object[]
			{
				this.reward.Value
			}));
			this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", new object[]
			{
				"0",
				this.numberToKill.Value,
				this.monster.Value
			});
		}

		// Token: 0x06001D0F RID: 7439 RVA: 0x0014D128 File Offset: 0x0014B328
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

		// Token: 0x06001D10 RID: 7440 RVA: 0x0014D258 File Offset: 0x0014B458
		public override void reloadObjective()
		{
			if (this.numberKilled.Value == 0 && base.HasId())
			{
				return;
			}
			if (this.numberKilled.Value < this.numberToKill.Value)
			{
				this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", new object[]
				{
					this.numberKilled.Value,
					this.numberToKill.Value,
					this.monster.Value
				});
			}
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}

		// Token: 0x06001D11 RID: 7441 RVA: 0x0014D305 File Offset: 0x0014B505
		private bool isSlimeName(string s)
		{
			return s.Contains("Slime") || s.Contains("Jelly") || s.Contains("Sludge");
		}

		// Token: 0x06001D12 RID: 7442 RVA: 0x0014D334 File Offset: 0x0014B534
		public override bool OnMonsterSlain(GameLocation location, Monster monster, bool killedByBomb, bool isTameMonster, bool probe = false)
		{
			bool baseChanged = base.OnMonsterSlain(location, monster, killedByBomb, isTameMonster, probe);
			if (!this.completed.Value && (monster.Name.Contains(this.monsterName.Value) || (this.id.Value == "15" && this.isSlimeName(monster.Name))) && this.numberKilled.Value < this.numberToKill.Value)
			{
				if (!probe)
				{
					this.numberKilled.Value = Math.Min(this.numberToKill.Value, this.numberKilled.Value + 1);
					Game1.dayTimeMoneyBox.pingQuest(this);
					if (this.numberKilled.Value >= this.numberToKill.Value)
					{
						if (this.target.Value == null || this.target.Value.Equals("null"))
						{
							this.questComplete();
						}
						else
						{
							NPC actualTarget = Game1.getCharacterFromName(this.target.Value, true, false);
							this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", new object[]
							{
								actualTarget
							});
							Game1.playSound("jingle1", null);
						}
					}
					else if (this.monster.Value == null)
					{
						if (this.monsterName.Value == "Frost Jelly" || this.monsterName.Value == "Sludge")
						{
							this.monster.Value = new Monster("Green Slime", Vector2.Zero);
							this.monster.Value.Name = this.monsterName.Value;
						}
						else
						{
							this.monster.Value = new Monster(this.monsterName.Value, Vector2.Zero);
						}
					}
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x06001D13 RID: 7443 RVA: 0x0014D524 File Offset: 0x0014B724
		public override bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			bool baseChanged = base.OnNpcSocialized(npc, probe);
			if (!this.completed.Value && this.target.Value != null && this.target.Value != "null" && this.numberKilled.Value >= this.numberToKill.Value && npc.Name == this.target.Value && npc.IsVillager)
			{
				if (!probe)
				{
					this.reloadDescription();
					npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage));
					this.moneyReward.Value = this.reward.Value;
					this.questComplete();
					Game1.drawDialogue(npc);
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x040011BD RID: 4541
		public string targetMessage;

		// Token: 0x040011BE RID: 4542
		[XmlElement("monsterName")]
		public readonly NetString monsterName = new NetString();

		// Token: 0x040011BF RID: 4543
		[XmlElement("target")]
		public readonly NetString target = new NetString();

		// Token: 0x040011C0 RID: 4544
		[XmlElement("monster")]
		public readonly NetRef<Monster> monster = new NetRef<Monster>();

		// Token: 0x040011C1 RID: 4545
		[XmlElement("numberToKill")]
		public readonly NetInt numberToKill = new NetInt();

		// Token: 0x040011C2 RID: 4546
		[XmlElement("reward")]
		public readonly NetInt reward = new NetInt();

		// Token: 0x040011C3 RID: 4547
		[XmlElement("numberKilled")]
		public readonly NetInt numberKilled = new NetInt();

		// Token: 0x040011C4 RID: 4548
		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		// Token: 0x040011C5 RID: 4549
		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		// Token: 0x040011C6 RID: 4550
		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		// Token: 0x040011C7 RID: 4551
		[XmlElement("ignoreFarmMonsters")]
		public readonly NetBool ignoreFarmMonsters = new NetBool(true);
	}
}

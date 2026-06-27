using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;

namespace StardewValley.Quests
{
	// Token: 0x02000197 RID: 407
	public class SocializeQuest : Quest
	{
		// Token: 0x06001D14 RID: 7444 RVA: 0x0014D5EF File Offset: 0x0014B7EF
		public SocializeQuest()
		{
			this.questType.Value = 5;
		}

		// Token: 0x06001D15 RID: 7445 RVA: 0x0014D630 File Offset: 0x0014B830
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.whoToGreet, "whoToGreet").AddField(this.total, "total").AddField(this.parts, "parts").AddField(this.objective, "objective");
		}

		// Token: 0x06001D16 RID: 7446 RVA: 0x0014D68C File Offset: 0x0014B88C
		public void loadQuestInfo()
		{
			if (this.whoToGreet.Count > 0)
			{
				return;
			}
			Random random = base.CreateInitializationRandom();
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13786", new object[]
			{
				new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs." + random.Choose("13787", "13788", "13789"), Array.Empty<object>())
			}));
			this.parts.Add("Strings\\StringsFromCSFiles:SocializeQuest.cs.13791");
			int curTotal = 0;
			foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
			{
				string name = entry.Key;
				CharacterData data = entry.Value;
				if (data.IntroductionsQuest ?? (data.HomeRegion == "Town"))
				{
					curTotal++;
					if (data.SocialTab != SocialTabBehavior.AlwaysShown || this.dailyQuest.Value)
					{
						this.whoToGreet.Add(name);
					}
				}
			}
			this.total.Value = curTotal;
			this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", new object[]
			{
				this.total.Value - this.whoToGreet.Count,
				this.total.Value
			});
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x0014D824 File Offset: 0x0014BA24
		public override void reloadDescription()
		{
			if (this._questDescription == "")
			{
				this.loadQuestInfo();
			}
			if (this.parts.Count == 0 || this.parts == null)
			{
				return;
			}
			string descriptionBuilder = "";
			foreach (DescriptionElement a in this.parts)
			{
				descriptionBuilder += a.loadDescriptionElement();
			}
			base.questDescription = descriptionBuilder;
		}

		// Token: 0x06001D18 RID: 7448 RVA: 0x0014D8B8 File Offset: 0x0014BAB8
		public override void reloadObjective()
		{
			this.loadQuestInfo();
			if (this.objective.Value == null && this.whoToGreet.Count > 0)
			{
				this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", new object[]
				{
					this.total.Value - this.whoToGreet.Count,
					this.total.Value
				});
			}
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x0014D958 File Offset: 0x0014BB58
		public override bool OnNpcSocialized(NPC npc, bool probe = false)
		{
			bool changed = base.OnNpcSocialized(npc, probe);
			this.loadQuestInfo();
			if (this.whoToGreet.Contains(npc.Name))
			{
				if (!probe)
				{
					this.whoToGreet.Remove(npc.Name);
					Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(228f, 244f), false, false, 1f, 0.01f, Color.White, 4f, 0.3f, 0f, 0f, false)
					{
						scaleChangeChange = -0.012f
					});
					Game1.dayTimeMoneyBox.pingQuest(this);
				}
				changed = true;
			}
			if (this.whoToGreet.Count == 0 && !this.completed.Value)
			{
				if (!probe)
				{
					foreach (string s in Game1.player.friendshipData.Keys)
					{
						if (Game1.player.friendshipData[s].Points < 2729)
						{
							Game1.player.changeFriendship(100, Game1.getCharacterFromName(s, true, false));
						}
					}
					this.questComplete();
				}
				return true;
			}
			if (!probe)
			{
				this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", new object[]
				{
					this.total.Value - this.whoToGreet.Count,
					this.total.Value
				});
			}
			return changed;
		}

		// Token: 0x040011C8 RID: 4552
		public readonly NetStringList whoToGreet = new NetStringList();

		// Token: 0x040011C9 RID: 4553
		[XmlElement("total")]
		public readonly NetInt total = new NetInt();

		// Token: 0x040011CA RID: 4554
		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		// Token: 0x040011CB RID: 4555
		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();
	}
}

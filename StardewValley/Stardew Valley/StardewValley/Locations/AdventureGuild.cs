using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002BE RID: 702
	public class AdventureGuild : GameLocation
	{
		// Token: 0x06002D94 RID: 11668 RVA: 0x00239844 File Offset: 0x00237A44
		public AdventureGuild()
		{
		}

		// Token: 0x06002D95 RID: 11669 RVA: 0x00239890 File Offset: 0x00237A90
		public AdventureGuild(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002D96 RID: 11670 RVA: 0x002398DC File Offset: 0x00237ADC
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "1");
			if (tileIndexAt - 1291 > 1)
			{
				if (tileIndexAt == 1306)
				{
					this.showMonsterKillList();
					return true;
				}
				if (tileIndexAt - 1355 > 3)
				{
					return base.checkAction(tileLocation, viewport, who);
				}
			}
			this.gil();
			return true;
		}

		// Token: 0x06002D97 RID: 11671 RVA: 0x00239932 File Offset: 0x00237B32
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.talkedToGil = false;
			Game1.player.mailReceived.Add("guildMember");
			base.addOneTimeGiftBox(ItemRegistry.Create("(O)Book_Marlon", 1, 0, false), 10, 4, 2);
		}

		// Token: 0x06002D98 RID: 11672 RVA: 0x00239970 File Offset: 0x00237B70
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(504f, 464f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.064801f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 504f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12)), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.06481f);
			}
		}

		// Token: 0x06002D99 RID: 11673 RVA: 0x00239A9C File Offset: 0x00237C9C
		private string killListLine(string monsterNamePlural, int killCount, int target)
		{
			if (killCount == 0)
			{
				return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_None", killCount, target, monsterNamePlural) + "^";
			}
			if (killCount >= target)
			{
				return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_OverTarget", killCount, target, monsterNamePlural) + "^";
			}
			return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat", killCount, target, monsterNamePlural) + "^";
		}

		// Token: 0x06002D9A RID: 11674 RVA: 0x00239B24 File Offset: 0x00237D24
		public void showMonsterKillList()
		{
			Game1.player.mailReceived.Add("checkedMonsterBoard");
			StringBuilder s = new StringBuilder();
			s.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Header").Replace('\n', '^') + "^");
			foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
			{
				int count = 0;
				if (questData.Targets != null)
				{
					foreach (string targetType in questData.Targets)
					{
						count += Game1.stats.getMonstersKilled(targetType);
					}
				}
				s.Append(this.killListLine(TokenParser.ParseText(questData.DisplayName, null, null, null), count, questData.Count));
			}
			s.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Footer").Replace('\n', '^'));
			Game1.drawLetterMessage(s.ToString());
		}

		// Token: 0x06002D9B RID: 11675 RVA: 0x00239C60 File Offset: 0x00237E60
		public static bool areAllMonsterSlayerQuestsComplete()
		{
			foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
			{
				int count = 0;
				if (questData.Targets != null)
				{
					foreach (string targetType in questData.Targets)
					{
						count += Game1.stats.getMonstersKilled(targetType);
						if (count >= questData.Count)
						{
							break;
						}
					}
					if (count < questData.Count)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002D9C RID: 11676 RVA: 0x00239D28 File Offset: 0x00237F28
		public static bool willThisKillCompleteAMonsterSlayerQuest(string nameOfMonster)
		{
			foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
			{
				if (questData.Targets.Contains(nameOfMonster))
				{
					int count = 0;
					if (questData.Targets != null)
					{
						foreach (string targetType in questData.Targets)
						{
							count += Game1.stats.getMonstersKilled(targetType);
							if (count >= questData.Count)
							{
								break;
							}
						}
						if (count < questData.Count && count + 1 >= questData.Count)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002D9D RID: 11677 RVA: 0x00239E10 File Offset: 0x00238010
		public void OnRewardCollected(Item item, Farmer who, List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals)
		{
			if (item == null)
			{
				return;
			}
			int goalIndex = item.SpecialVariable;
			if (goalIndex < 0 || goalIndex >= completedGoals.Count)
			{
				return;
			}
			KeyValuePair<string, MonsterSlayerQuestData> goal = completedGoals[goalIndex];
			who.mailReceived.Add("Gil_" + goal.Key);
		}

		// Token: 0x06002D9E RID: 11678 RVA: 0x00239E5C File Offset: 0x0023805C
		private void gil()
		{
			List<Item> rewards = new List<Item>();
			List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals = new List<KeyValuePair<string, MonsterSlayerQuestData>>();
			List<string> dialogues = new List<string>();
			foreach (KeyValuePair<string, MonsterSlayerQuestData> pair in DataLoader.MonsterSlayerQuests(Game1.content))
			{
				string id = pair.Key;
				MonsterSlayerQuestData questData = pair.Value;
				if (!AdventureGuild.HasCollectedReward(Game1.player, id) && AdventureGuild.IsComplete(questData))
				{
					completedGoals.Add(pair);
					if (questData.RewardItemId != null)
					{
						Item item = ItemRegistry.Create(questData.RewardItemId, 1, 0, false);
						item.SpecialVariable = completedGoals.Count - 1;
						Object obj = item as Object;
						if (obj != null)
						{
							obj.specialItem = true;
						}
						rewards.Add(item);
					}
					if (questData.RewardDialogue != null && (questData.RewardDialogueFlag == null || !Game1.player.mailReceived.Contains(questData.RewardDialogueFlag)))
					{
						dialogues.Add(TokenParser.ParseText(questData.RewardDialogue, null, null, null));
					}
					if (questData.RewardMail != null)
					{
						Game1.addMailForTomorrow(questData.RewardMail, false, false);
					}
					if (questData.RewardMailAll != null)
					{
						Game1.addMailForTomorrow(questData.RewardMailAll, false, true);
					}
					if (questData.RewardFlag != null)
					{
						Game1.addMail(questData.RewardFlag, true, false);
					}
					if (questData.RewardFlagAll != null)
					{
						Game1.addMail(questData.RewardFlagAll, true, true);
					}
				}
			}
			if (rewards.Count <= 0 && dialogues.Count <= 0)
			{
				if (this.talkedToGil)
				{
					Game1.DrawDialogue(this.Gil, "Characters\\Dialogue\\Gil:Snoring");
				}
				else
				{
					Game1.DrawDialogue(this.Gil, "Characters\\Dialogue\\Gil:ComeBackLater");
				}
				this.talkedToGil = true;
				return;
			}
			if (dialogues.Count > 0)
			{
				Game1.DrawDialogue(new Dialogue(this.Gil, null, string.Join("#$b#", dialogues)));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
				{
					this.OpenRewardMenuIfNeeded(rewards, completedGoals);
				}));
				return;
			}
			this.OpenRewardMenuIfNeeded(rewards, completedGoals);
		}

		// Token: 0x06002D9F RID: 11679 RVA: 0x0023A0B0 File Offset: 0x002382B0
		public static bool HasCollectedReward(Farmer player, string goalId)
		{
			return player.mailReceived.Contains("Gil_" + goalId);
		}

		// Token: 0x06002DA0 RID: 11680 RVA: 0x0023A0C8 File Offset: 0x002382C8
		public static bool IsComplete(MonsterSlayerQuestData goal)
		{
			if (goal.Targets == null)
			{
				return true;
			}
			int count = 0;
			foreach (string targetType in goal.Targets)
			{
				count += Game1.stats.getMonstersKilled(targetType);
				if (count >= goal.Count)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002DA1 RID: 11681 RVA: 0x0023A140 File Offset: 0x00238340
		private void OpenRewardMenuIfNeeded(List<Item> rewards, List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals)
		{
			if (rewards.Count == 0)
			{
				return;
			}
			Game1.activeClickableMenu = new ItemGrabMenu(rewards, this)
			{
				behaviorOnItemGrab = delegate(Item item, Farmer who)
				{
					this.OnRewardCollected(item, who, completedGoals);
				}
			};
		}

		// Token: 0x04001F4A RID: 8010
		public NPC Gil = new NPC(null, new Vector2(-1000f, -1000f), "AdventureGuild", 2, "Gil", false, Game1.content.Load<Texture2D>("Portraits\\Gil"));

		// Token: 0x04001F4B RID: 8011
		public bool talkedToGil;
	}
}

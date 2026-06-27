using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Events
{
	// Token: 0x02000324 RID: 804
	public class DiaryEvent : BaseFarmEvent
	{
		// Token: 0x0600348A RID: 13450 RVA: 0x0029D4D0 File Offset: 0x0029B6D0
		public override bool setUp()
		{
			if (Game1.player.isMarriedOrRoommates())
			{
				return true;
			}
			foreach (string s in Game1.player.mailReceived)
			{
				if (s.Contains("diary"))
				{
					string name = s.Split('_', StringSplitOptions.None)[1];
					if (Game1.player.mailReceived.Add("diary_" + name + "_finished"))
					{
						this.NPCname = name.Split('/', StringSplitOptions.None)[0];
						NPC who = Game1.getCharacterFromName(this.NPCname, true, false);
						string question = string.Concat(new string[]
						{
							Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6658") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6660"),
							Environment.NewLine,
							Environment.NewLine,
							"-",
							Utility.capitalizeFirstLetter(Game1.CurrentSeasonDisplayName),
							" ",
							Game1.dayOfMonth.ToString(),
							"-",
							Environment.NewLine,
							Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6664", this.NPCname)
						});
						Response[] diaryOptions = new Response[]
						{
							new Response("...We're", Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6667")),
							new Response("...I", (who.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6669") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6670")),
							new Response("(Write", Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6672"))
						};
						Game1.currentLocation.createQuestionDialogue(Game1.parseText(question), diaryOptions, "diary");
						Game1.messagePause = true;
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0600348B RID: 13451 RVA: 0x0029D6D8 File Offset: 0x0029B8D8
		public override bool tickUpdate(GameTime time)
		{
			return !Game1.dialogueUp;
		}

		// Token: 0x0600348C RID: 13452 RVA: 0x0029D6E2 File Offset: 0x0029B8E2
		public override void makeChangesToLocation()
		{
			Game1.messagePause = false;
		}

		// Token: 0x04002245 RID: 8773
		public string NPCname;
	}
}

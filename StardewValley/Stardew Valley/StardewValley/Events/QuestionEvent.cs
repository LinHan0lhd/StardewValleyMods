using System;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace StardewValley.Events
{
	// Token: 0x0200032A RID: 810
	public class QuestionEvent : BaseFarmEvent
	{
		// Token: 0x060034B1 RID: 13489 RVA: 0x002A151A File Offset: 0x0029F71A
		public QuestionEvent(int whichQuestion)
		{
			this.whichQuestion = whichQuestion;
		}

		// Token: 0x060034B2 RID: 13490 RVA: 0x002A152C File Offset: 0x0029F72C
		public override bool setUp()
		{
			switch (this.whichQuestion)
			{
			case 1:
			{
				Response[] answers = new Response[]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
				};
				NPC spouse = Game1.RequireCharacter(Game1.player.spouse, true);
				string dialogueKey = (!spouse.isAdoptionSpouse()) ? "Strings\\Events:HaveBabyQuestion" : "Strings\\Events:HaveBabyQuestion_Adoption";
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString(dialogueKey, Game1.player.Name), answers, new GameLocation.afterQuestionBehavior(this.answerPregnancyQuestion), spouse);
				Game1.messagePause = true;
				return false;
			}
			case 2:
			{
				FarmAnimal a = null;
				Utility.ForEachBuilding(delegate(Building b)
				{
					if ((b.owner.Value == Game1.player.UniqueMultiplayerID || !Game1.IsMultiplayer) && b.AllowsAnimalPregnancy())
					{
						AnimalHouse house = b.GetIndoors() as AnimalHouse;
						if (house != null && !house.isFull() && Game1.random.NextDouble() < (double)house.animalsThatLiveHere.Count * 0.0055)
						{
							a = Utility.getAnimal(house.animalsThatLiveHere[Game1.random.Next(house.animalsThatLiveHere.Count)]);
							this.animalHouse = house;
							return false;
						}
					}
					return true;
				}, true);
				if (a != null && !a.isBaby() && a.allowReproduction.Value && a.CanHavePregnancy())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:AnimalBirth", a.displayName, a.shortDisplayType()));
					Game1.messagePause = true;
					this.animal = a;
					return false;
				}
				break;
			}
			case 3:
			{
				Response[] answers2 = new Response[]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
				};
				long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				Farmer spouse2 = Game1.otherFarmers[spouseID];
				if (spouse2.IsMale != Game1.player.IsMale)
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", spouse2.displayName), answers2, new GameLocation.afterQuestionBehavior(this.answerPlayerPregnancyQuestion), null);
				}
				else
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion_Adoption", spouse2.displayName), answers2, new GameLocation.afterQuestionBehavior(this.answerPlayerPregnancyQuestion), null);
				}
				Game1.messagePause = true;
				return false;
			}
			}
			return true;
		}

		// Token: 0x060034B3 RID: 13491 RVA: 0x002A1788 File Offset: 0x0029F988
		private void answerPregnancyQuestion(Farmer who, string answer)
		{
			if (answer.Equals("Yes"))
			{
				WorldDate birthingDate = new WorldDate(Game1.Date);
				birthingDate.TotalDays += 14;
				who.GetSpouseFriendship().NextBirthingDate = birthingDate;
			}
		}

		// Token: 0x060034B4 RID: 13492 RVA: 0x002A17C8 File Offset: 0x0029F9C8
		private void answerPlayerPregnancyQuestion(Farmer who, string answer)
		{
			if (answer.Equals("Yes"))
			{
				long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				Farmer spouse = Game1.otherFarmers[spouseID];
				Game1.player.team.SendProposal(spouse, ProposalType.Baby, null);
			}
		}

		// Token: 0x060034B5 RID: 13493 RVA: 0x002A1824 File Offset: 0x0029FA24
		public override bool tickUpdate(GameTime time)
		{
			if (this.forceProceed)
			{
				return true;
			}
			if (this.whichQuestion == 2 && !Game1.dialogueUp)
			{
				if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(this.animalHouse.addNewHatchedAnimal), (this.animal != null) ? Game1.content.LoadString("Strings\\Events:AnimalNamingTitle", this.animal.displayType) : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestionEvent.cs.6692"), null);
				}
				return false;
			}
			return !Game1.dialogueUp;
		}

		// Token: 0x060034B6 RID: 13494 RVA: 0x002A18AB File Offset: 0x0029FAAB
		public override void makeChangesToLocation()
		{
			Game1.messagePause = false;
		}

		// Token: 0x0400226B RID: 8811
		public const int pregnancyQuestion = 1;

		// Token: 0x0400226C RID: 8812
		public const int barnBirth = 2;

		// Token: 0x0400226D RID: 8813
		public const int playerPregnancyQuestion = 3;

		// Token: 0x0400226E RID: 8814
		private int whichQuestion;

		// Token: 0x0400226F RID: 8815
		private AnimalHouse animalHouse;

		// Token: 0x04002270 RID: 8816
		public FarmAnimal animal;

		// Token: 0x04002271 RID: 8817
		public bool forceProceed;
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewValley.Events
{
	// Token: 0x02000323 RID: 803
	public class BirthingEvent : BaseFarmEvent
	{
		// Token: 0x06003485 RID: 13445 RVA: 0x0029CF74 File Offset: 0x0029B174
		public override bool setUp()
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
			NPC spouse = Game1.RequireCharacter(Game1.player.spouse, true);
			Game1.player.CanMove = false;
			if (Game1.player.getNumberOfChildren() == 0)
			{
				this.isMale = r.NextBool();
			}
			else
			{
				this.isMale = (Game1.player.getChildren()[0].Gender == Gender.Female);
			}
			if (spouse.isAdoptionSpouse())
			{
				this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(this.isMale));
			}
			else if (spouse.Gender == Gender.Male)
			{
				this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(this.isMale));
			}
			else
			{
				this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(this.isMale), spouse.displayName);
			}
			return false;
		}

		// Token: 0x06003486 RID: 13446 RVA: 0x0029D084 File Offset: 0x0029B284
		public void returnBabyName(string name)
		{
			this.babyName = name;
			Game1.exitActiveMenu();
		}

		// Token: 0x06003487 RID: 13447 RVA: 0x0029D092 File Offset: 0x0029B292
		public void afterMessage()
		{
			this.getBabyName = true;
		}

		// Token: 0x06003488 RID: 13448 RVA: 0x0029D09C File Offset: 0x0029B29C
		public override bool tickUpdate(GameTime time)
		{
			Game1.player.CanMove = false;
			this.timer += time.ElapsedGameTime.Milliseconds;
			Game1.fadeToBlackAlpha = 1f;
			if (this.timer > 1500 && !this.playedSound && !this.getBabyName)
			{
				if (!string.IsNullOrEmpty(this.soundName))
				{
					Game1.playSound(this.soundName, null);
					this.playedSound = true;
				}
				if (!this.playedSound && this.message != null && !Game1.dialogueUp && Game1.activeClickableMenu == null)
				{
					Game1.drawObjectDialogue(this.message);
					Game1.afterDialogues = new Game1.afterFadeFunction(this.afterMessage);
				}
			}
			else if (this.getBabyName)
			{
				if (!this.naming)
				{
					Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(this.returnBabyName), Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
					this.naming = true;
				}
				if (!string.IsNullOrEmpty(this.babyName) && this.babyName.Length > 0)
				{
					NPC spouse = Game1.player.getSpouse();
					double chance = (spouse.hasDarkSkin() ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
					bool isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0).NextBool(chance);
					string newBabyName = this.babyName;
					List<NPC> all_characters = Utility.getAllCharacters();
					bool collision_found;
					do
					{
						collision_found = false;
						if (Game1.characterData.ContainsKey(newBabyName))
						{
							newBabyName += " ";
							collision_found = true;
						}
						else
						{
							using (List<NPC>.Enumerator enumerator = all_characters.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									if (enumerator.Current.Name == newBabyName)
									{
										newBabyName += " ";
										collision_found = true;
									}
								}
							}
						}
					}
					while (collision_found);
					Child baby = new Child(newBabyName, this.isMale, isDarkSkinned, Game1.player);
					baby.Age = 0;
					baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
					Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
					Game1.stats.checkForFullHouseAchievement(true);
					Game1.playSound("smallSelect", null);
					spouse.daysAfterLastBirth = 5;
					Game1.player.GetSpouseFriendship().NextBirthingDate = null;
					if (Game1.player.getChildrenCount() == 2)
					{
						spouse.shouldSayMarriageDialogue.Value = true;
						spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3).ToString(), true, Array.Empty<string>()));
					}
					else if (spouse.isAdoptionSpouse())
					{
						spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, new string[]
						{
							this.babyName
						}));
					}
					else
					{
						spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, new string[]
						{
							this.babyName
						}));
					}
					Game1.morningQueue.Enqueue(delegate
					{
						NPC characterFromName = Game1.getCharacterFromName(Game1.player.spouse, true, false);
						string spouseName = ((characterFromName != null) ? characterFromName.GetTokenizedDisplayName() : null) ?? Game1.player.spouse;
						Game1.multiplayer.globalChatInfoMessage("Baby", new string[]
						{
							Lexicon.capitalize(Game1.player.Name),
							spouseName,
							Lexicon.getTokenizedGenderedChildTerm(this.isMale),
							Lexicon.getTokenizedPronoun(this.isMale),
							baby.displayName
						});
					});
					if (Game1.keyboardDispatcher != null)
					{
						Game1.keyboardDispatcher.Subscriber = null;
					}
					Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).GetPlayerBedSpot()) * 64f;
					Game1.globalFadeToClear(null, 0.02f);
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400223D RID: 8765
		private int timer;

		// Token: 0x0400223E RID: 8766
		private string soundName;

		// Token: 0x0400223F RID: 8767
		private string message;

		// Token: 0x04002240 RID: 8768
		private string babyName;

		// Token: 0x04002241 RID: 8769
		private bool playedSound;

		// Token: 0x04002242 RID: 8770
		private bool isMale;

		// Token: 0x04002243 RID: 8771
		private bool getBabyName;

		// Token: 0x04002244 RID: 8772
		private bool naming;
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Events
{
	// Token: 0x02000328 RID: 808
	public class PlayerCoupleBirthingEvent : BaseFarmEvent
	{
		// Token: 0x060034A4 RID: 13476 RVA: 0x002A043C File Offset: 0x0029E63C
		public PlayerCoupleBirthingEvent()
		{
			this.spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
			Game1.otherFarmers.TryGetValue(this.spouseID, out this.spouse);
			this.farmHouse = this.chooseHome();
		}

		// Token: 0x060034A5 RID: 13477 RVA: 0x002A0499 File Offset: 0x0029E699
		private bool isSuitableHome(FarmHouse home)
		{
			return home.getChildrenCount() < 2 && home.upgradeLevel >= 2;
		}

		// Token: 0x060034A6 RID: 13478 RVA: 0x002A04B4 File Offset: 0x0029E6B4
		private FarmHouse chooseHome()
		{
			List<Farmer> parents = new List<Farmer>
			{
				Game1.player,
				this.spouse
			};
			parents.Sort((Farmer p1, Farmer p2) => p1.UniqueMultiplayerID.CompareTo(p2.UniqueMultiplayerID));
			foreach (Farmer parent in parents)
			{
				FarmHouse home = Game1.getLocationFromName(parent.homeLocation.Value) as FarmHouse;
				if (home != null && home == parent.currentLocation && this.isSuitableHome(home))
				{
					return home;
				}
			}
			foreach (Farmer farmer in parents)
			{
				FarmHouse home2 = Game1.getLocationFromName(farmer.homeLocation.Value) as FarmHouse;
				if (home2 != null && this.isSuitableHome(home2))
				{
					return home2;
				}
			}
			return Game1.player.currentLocation as FarmHouse;
		}

		// Token: 0x060034A7 RID: 13479 RVA: 0x002A05E0 File Offset: 0x0029E7E0
		public override bool setUp()
		{
			if (this.spouse == null || this.farmHouse == null)
			{
				return true;
			}
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays, 0.0, 0.0, 0.0);
			Game1.player.CanMove = false;
			if (this.farmHouse.getChildrenCount() == 0)
			{
				this.isMale = r.NextBool();
			}
			else
			{
				this.isMale = (this.farmHouse.getChildren()[0].Gender == Gender.Female);
			}
			Friendship friendship = Game1.player.GetSpouseFriendship();
			this.isPlayersTurn = (friendship.Proposer != Game1.player.UniqueMultiplayerID == (this.farmHouse.getChildrenCount() % 2 == 0));
			if (this.spouse.IsMale == Game1.player.IsMale)
			{
				this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(this.isMale));
			}
			else if (this.spouse.IsMale)
			{
				this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(this.isMale));
			}
			else
			{
				this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(this.isMale), this.spouse.Name);
			}
			return false;
		}

		// Token: 0x060034A8 RID: 13480 RVA: 0x002A0743 File Offset: 0x0029E943
		public void returnBabyName(string name)
		{
			this.babyName = name;
			Game1.exitActiveMenu();
		}

		// Token: 0x060034A9 RID: 13481 RVA: 0x002A0754 File Offset: 0x0029E954
		public void afterMessage()
		{
			if (this.isPlayersTurn)
			{
				this.getBabyName = true;
				double chance = this.spouse.hasDarkSkin() ? 0.5 : 0.0;
				chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
				bool isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0).NextDouble() < chance;
				this.farmHouse.characters.Add(this.child = new Child("Baby", this.isMale, isDarkSkinned, Game1.player));
				this.child.Age = 0;
				this.child.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
				Game1.player.stats.checkForFullHouseAchievement(true);
				Game1.player.GetSpouseFriendship().NextBirthingDate = null;
				return;
			}
			Game1.afterDialogues = delegate()
			{
				this.getBabyName = true;
			};
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseNaming_" + (this.isMale ? "Male" : "Female"), this.spouse.Name));
		}

		// Token: 0x060034AA RID: 13482 RVA: 0x002A08D4 File Offset: 0x0029EAD4
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
				if (!this.isPlayersTurn)
				{
					Game1.globalFadeToClear(null, 0.02f);
					return true;
				}
				if (!this.naming)
				{
					Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(this.returnBabyName), Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
					this.naming = true;
				}
				if (!string.IsNullOrEmpty(this.babyName) && this.babyName.Length > 0)
				{
					string newBabyName = this.babyName;
					List<NPC> all_characters = Utility.getAllCharacters();
					bool collision_found;
					do
					{
						collision_found = false;
						using (List<NPC>.Enumerator enumerator = all_characters.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.Name == newBabyName)
								{
									newBabyName += " ";
									collision_found = true;
									break;
								}
							}
						}
					}
					while (collision_found);
					this.child.Name = newBabyName;
					Game1.playSound("smallSelect", null);
					if (Game1.keyboardDispatcher != null)
					{
						Game1.keyboardDispatcher.Subscriber = null;
					}
					Game1.globalFadeToClear(null, 0.02f);
					return true;
				}
			}
			return false;
		}

		// Token: 0x04002258 RID: 8792
		private int timer;

		// Token: 0x04002259 RID: 8793
		private string soundName;

		// Token: 0x0400225A RID: 8794
		private string message;

		// Token: 0x0400225B RID: 8795
		private string babyName;

		// Token: 0x0400225C RID: 8796
		private bool playedSound;

		// Token: 0x0400225D RID: 8797
		private bool isMale;

		// Token: 0x0400225E RID: 8798
		private bool getBabyName;

		// Token: 0x0400225F RID: 8799
		private bool naming;

		// Token: 0x04002260 RID: 8800
		private FarmHouse farmHouse;

		// Token: 0x04002261 RID: 8801
		private long spouseID;

		// Token: 0x04002262 RID: 8802
		private Farmer spouse;

		// Token: 0x04002263 RID: 8803
		private bool isPlayersTurn;

		// Token: 0x04002264 RID: 8804
		private Child child;
	}
}

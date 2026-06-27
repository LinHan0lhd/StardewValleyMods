using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Menus;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E3 RID: 739
	public class JojaMart : GameLocation
	{
		// Token: 0x060030E6 RID: 12518 RVA: 0x0026A00C File Offset: 0x0026820C
		public JojaMart()
		{
		}

		// Token: 0x060030E7 RID: 12519 RVA: 0x0026A014 File Offset: 0x00268214
		public JojaMart(string map, string name) : base(map, name)
		{
		}

		// Token: 0x060030E8 RID: 12520 RVA: 0x0026A020 File Offset: 0x00268220
		private bool signUpForJoja(int response)
		{
			if (response == 0)
			{
				base.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:JojaMart_SignUp")), base.createYesNoResponses(), "JojaSignUp");
				return true;
			}
			Game1.dialogueUp = false;
			Game1.player.forceCanMove();
			base.localSound("smallSelect", null, null, SoundContext.Default);
			Game1.currentSpeaker = null;
			Game1.dialogueTyping = false;
			return true;
		}

		// Token: 0x060030E9 RID: 12521 RVA: 0x0026A094 File Offset: 0x00268294
		public override bool answerDialogue(Response answer)
		{
			if (this.lastQuestionKey != null && this.afterQuestion == null && ArgUtility.SplitBySpaceAndGet(this.lastQuestionKey, 0, null) + "_" + answer.responseKey == "JojaSignUp_Yes")
			{
				if (Game1.player.Money >= 5000)
				{
					Game1.player.Money -= 5000;
					Game1.addMailForTomorrow("JojaMember", true, true);
					Game1.player.removeQuest("26");
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_PlayerSignedUp", false, false);
					Game1.drawDialogue(JojaMart.Morris);
				}
				else if (Game1.player.Money < 5000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
				return true;
			}
			return base.answerDialogue(answer);
		}

		// Token: 0x060030EA RID: 12522 RVA: 0x0026A170 File Offset: 0x00268370
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings", false) == "JoinJoja")
			{
				JojaMart.Morris.CurrentDialogue.Clear();
				if (Game1.player.mailForTomorrow.Contains("JojaMember%&NL&%"))
				{
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_ComeBackLater", false, false);
					Game1.drawDialogue(JojaMart.Morris);
				}
				else if (!Game1.player.mailReceived.Contains("JojaMember"))
				{
					if (Game1.player.mailReceived.Add("JojaGreeting"))
					{
						JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_Greeting", false, false);
						Game1.drawDialogue(JojaMart.Morris);
					}
					else if (Game1.stats.DaysPlayed < 0U)
					{
						string greeting = (Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 6) ? "Data\\ExtraDialogue:Morris_WeekendGreeting" : "Data\\ExtraDialogue:Morris_FirstGreeting";
						JojaMart.Morris.setNewDialogue(greeting, false, false);
						Game1.drawDialogue(JojaMart.Morris);
					}
					else
					{
						string greeting2 = (Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 6) ? "Data\\ExtraDialogue:Morris_WeekendGreeting" : "Data\\ExtraDialogue:Morris_FirstGreeting";
						if (Game1.IsMasterGame)
						{
							if (!Game1.player.eventsSeen.Contains("611439"))
							{
								JojaMart.Morris.setNewDialogue(greeting2, false, false);
								Game1.drawDialogue(JojaMart.Morris);
							}
							else if (Game1.player.mailReceived.Contains("ccIsComplete"))
							{
								JojaMart.Morris.setNewDialogue(greeting2 + "_CommunityCenterComplete", false, false);
								Game1.drawDialogue(JojaMart.Morris);
							}
							else
							{
								JojaMart.Morris.setNewDialogue(Dialogue.FromTranslation(JojaMart.Morris, greeting2 + "_MembershipAvailable", 5000), false, false);
								JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = new Dialogue.onAnswerQuestion(this.signUpForJoja);
								Game1.drawDialogue(JojaMart.Morris);
							}
						}
						else
						{
							JojaMart.Morris.setNewDialogue(greeting2 + "_SecondPlayer", false, false);
							Game1.drawDialogue(JojaMart.Morris);
						}
					}
				}
				else
				{
					if (Game1.player.eventsSeen.Contains("502261") && !Game1.player.hasOrWillReceiveMail("ccMovieTheater"))
					{
						JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_BuyMovieTheater", false, false);
						JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = new Dialogue.onAnswerQuestion(this.buyMovieTheater);
					}
					else if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%"))
					{
						JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_StillProcessingOrder", false, false);
					}
					else if (Game1.player.eventsSeen.Contains("502261"))
					{
						JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_NoMoreCD", false, false);
					}
					else
					{
						JojaMart.Morris.setNewDialogue(Game1.player.IsMale ? "Data\\ExtraDialogue:Morris_CommunityDevelopmentForm_PlayerMale" : "Data\\ExtraDialogue:Morris_CommunityDevelopmentForm_PlayerFemale", false, false);
						JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = new Dialogue.onAnswerQuestion(this.viewJojaNote);
					}
					Game1.drawDialogue(JojaMart.Morris);
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x060030EB RID: 12523 RVA: 0x0026A4FC File Offset: 0x002686FC
		private bool buyMovieTheater(int response)
		{
			if (response == 0)
			{
				if (Game1.player.Money >= 500000)
				{
					Game1.player.Money -= 500000;
					Game1.addMailForTomorrow("ccMovieTheater", true, true);
					Game1.addMailForTomorrow("ccMovieTheaterJoja", true, true);
					if (Game1.player.team.theaterBuildDate.Value < 0L)
					{
						Game1.player.team.theaterBuildDate.Set((long)(Game1.Date.TotalDays + 1));
					}
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_TheaterBought", false, false);
					Game1.drawDialogue(JojaMart.Morris);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
				}
			}
			return true;
		}

		// Token: 0x060030EC RID: 12524 RVA: 0x0026A5BC File Offset: 0x002687BC
		private bool viewJojaNote(int response)
		{
			if (response == 0)
			{
				Game1.activeClickableMenu = new JojaCDMenu(this.communityDevelopmentTexture);
				Game1.player.activeDialogueEvents.TryAdd("joja_Begin", 7);
			}
			Game1.dialogueUp = false;
			Game1.player.forceCanMove();
			base.localSound("smallSelect", null, null, SoundContext.Default);
			Game1.currentSpeaker = null;
			Game1.dialogueTyping = false;
			return true;
		}

		// Token: 0x060030ED RID: 12525 RVA: 0x0026A630 File Offset: 0x00268830
		protected override void resetLocalState()
		{
			this.communityDevelopmentTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JojaCDForm");
			JojaMart.Morris = new NPC(null, Vector2.Zero, "JojaMart", 2, "Morris", false, Game1.temporaryContent.Load<Texture2D>("Portraits\\Morris"));
			base.resetLocalState();
		}

		// Token: 0x040020E5 RID: 8421
		public const int JojaMembershipPrice = 5000;

		// Token: 0x040020E6 RID: 8422
		public static NPC Morris;

		// Token: 0x040020E7 RID: 8423
		private Texture2D communityDevelopmentTexture;
	}
}

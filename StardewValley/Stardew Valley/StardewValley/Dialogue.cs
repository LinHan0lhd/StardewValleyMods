using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Logging;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace StardewValley
{
	// Token: 0x0200009A RID: 154
	public class Dialogue
	{
		// Token: 0x060006BD RID: 1725 RVA: 0x00025E58 File Offset: 0x00024058
		private static void TranslateArraysOfStrings()
		{
			Dialogue.colors = new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.795"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.796"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.797"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.798"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.799"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.800"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.801"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.802"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.803"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.804"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.805"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.806"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.807"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.808"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.809"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.810")
			};
			Dialogue.adjectives = new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.679"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.680"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.681"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.682"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.683"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.684"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.685"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.686"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.687"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.688"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.689"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.690"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.691"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.692"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.693"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.694"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.695"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.696"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.697"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.698")
			};
			Dialogue.nouns = new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.699"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.700"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.701"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.702"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.703"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.704"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.705"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.706"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.707"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.708"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.709"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.710"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.711"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.712"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.713"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.714"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.715"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.716"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.717"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.718"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.719"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.720"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721")
			};
			Dialogue.verbs = new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.722"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.723"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.724"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.725"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.726"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.727"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.728"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.729"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.730"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.731"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.732"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.733"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.734")
			};
			Dialogue.positional = new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.735"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.736"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.737"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.738"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.739"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.740"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.741"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.742"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.743"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.744"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.745"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.746"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.747")
			};
			Dialogue.places = new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.748"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.749"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.750"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.751"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.752"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.753"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.754"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.755"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.756"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.757"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.758"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.759")
			};
			Dialogue.nameArraysTranslated = true;
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x060006BE RID: 1726 RVA: 0x000265B0 File Offset: 0x000247B0
		// (set) Token: 0x060006BF RID: 1727 RVA: 0x000265C1 File Offset: 0x000247C1
		public string CurrentEmotion
		{
			get
			{
				return this.currentEmotion ?? "$neutral";
			}
			set
			{
				this.currentEmotion = value;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x060006C0 RID: 1728 RVA: 0x000265CA File Offset: 0x000247CA
		public bool CurrentEmotionSetExplicitly
		{
			get
			{
				return this.currentEmotion != null;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x000265D5 File Offset: 0x000247D5
		public Farmer farmer
		{
			get
			{
				if (Game1.CurrentEvent != null)
				{
					return Game1.CurrentEvent.farmer;
				}
				return Game1.player;
			}
		}

		// Token: 0x060006C2 RID: 1730 RVA: 0x000265F0 File Offset: 0x000247F0
		public Dialogue(NPC speaker, string translationKey, string dialogueText)
		{
			if (!Dialogue.nameArraysTranslated)
			{
				Dialogue.TranslateArraysOfStrings();
			}
			this.speaker = speaker;
			this.TranslationKey = translationKey;
			try
			{
				this.parseDialogueString(dialogueText, translationKey);
				this.checkForSpecialDialogueAttributes();
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Failed parsing dialogue string for NPC ");
				defaultInterpolatedStringHandler.AppendFormatted((speaker != null) ? speaker.Name : null);
				defaultInterpolatedStringHandler.AppendLiteral(" (key: ");
				defaultInterpolatedStringHandler.AppendFormatted(translationKey);
				defaultInterpolatedStringHandler.AppendLiteral(", text: ");
				defaultInterpolatedStringHandler.AppendFormatted(dialogueText);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				this.parseDialogueString(Dialogue.GetFallbackTextForError(), "Strings\\Characters:FallbackDialogueForError");
				this.checkForSpecialDialogueAttributes();
			}
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x000266E0 File Offset: 0x000248E0
		public Dialogue(NPC speaker, string translationKey, bool isGendered = false) : this(speaker, translationKey, isGendered ? Game1.LoadStringByGender(speaker.Gender, translationKey) : Game1.content.LoadString(translationKey))
		{
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x00026708 File Offset: 0x00024908
		public Dialogue(Dialogue other)
		{
			foreach (DialogueLine line in other.dialogues)
			{
				this.dialogues.Add(new DialogueLine(line.Text, line.SideEffects));
			}
			this.indexesWithoutPortrait = new HashSet<int>(other.indexesWithoutPortrait);
			if (other.playerResponses != null)
			{
				this.playerResponses = new List<NPCDialogueResponse>();
				foreach (NPCDialogueResponse response in other.playerResponses)
				{
					this.playerResponses.Add(new NPCDialogueResponse(response));
				}
			}
			if (other.quickResponses != null)
			{
				this.quickResponses = new List<string>(other.quickResponses);
			}
			this.isLastDialogueInteractive = other.isLastDialogueInteractive;
			this.quickResponse = other.quickResponse;
			this.isCurrentStringContinuedOnNextScreen = other.isCurrentStringContinuedOnNextScreen;
			this.finishedLastDialogue = other.finishedLastDialogue;
			this.showPortrait = other.showPortrait;
			this.removeOnNextMove = other.removeOnNextMove;
			this.dontFaceFarmer = other.dontFaceFarmer;
			this.temporaryDialogueKey = other.temporaryDialogueKey;
			this.currentDialogueIndex = other.currentDialogueIndex;
			this.currentEmotion = other.currentEmotion;
			this.speaker = other.speaker;
			this.answerQuestionBehavior = other.answerQuestionBehavior;
			this.overridePortrait = other.overridePortrait;
			this.onFinish = other.onFinish;
			this.TranslationKey = other.TranslationKey;
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x000268CC File Offset: 0x00024ACC
		public static Dialogue TryGetDialogue(NPC speaker, string translationKey)
		{
			string text = Game1.content.LoadStringReturnNullIfNotFound(translationKey, true);
			if (text == null)
			{
				return null;
			}
			return new Dialogue(speaker, translationKey, text);
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x000268F3 File Offset: 0x00024AF3
		public static Dialogue FromTranslation(NPC speaker, string translationKey)
		{
			return new Dialogue(speaker, translationKey, false);
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x000268FD File Offset: 0x00024AFD
		public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1)
		{
			return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1));
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x00026912 File Offset: 0x00024B12
		public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1, object sub2)
		{
			return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1, sub2));
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x00026928 File Offset: 0x00024B28
		public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1, object sub2, object sub3)
		{
			return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1, sub2, sub3));
		}

		// Token: 0x060006CA RID: 1738 RVA: 0x00026940 File Offset: 0x00024B40
		public static Dialogue FromTranslation(NPC speaker, string translationKey, params object[] substitutions)
		{
			return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, substitutions));
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x00026955 File Offset: 0x00024B55
		public static Dialogue GetFallbackForError(NPC speaker)
		{
			return Dialogue.TryGetDialogue(speaker, "Strings\\Characters:FallbackDialogueForError") ?? new Dialogue(speaker, "Strings\\Characters:FallbackDialogueForError", "...");
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x00026976 File Offset: 0x00024B76
		public static string GetFallbackTextForError()
		{
			return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:FallbackDialogueForError", true) ?? "...";
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x00026991 File Offset: 0x00024B91
		public static string getRandomVerb()
		{
			if (!Dialogue.nameArraysTranslated)
			{
				Dialogue.TranslateArraysOfStrings();
			}
			return Game1.random.Choose(Dialogue.verbs);
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x000269AE File Offset: 0x00024BAE
		public static string getRandomAdjective()
		{
			if (!Dialogue.nameArraysTranslated)
			{
				Dialogue.TranslateArraysOfStrings();
			}
			return Game1.random.Choose(Dialogue.adjectives);
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x000269CB File Offset: 0x00024BCB
		public static string getRandomNoun()
		{
			if (!Dialogue.nameArraysTranslated)
			{
				Dialogue.TranslateArraysOfStrings();
			}
			return Game1.random.Choose(Dialogue.nouns);
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x000269E8 File Offset: 0x00024BE8
		public static string getRandomPositional()
		{
			if (!Dialogue.nameArraysTranslated)
			{
				Dialogue.TranslateArraysOfStrings();
			}
			return Game1.random.Choose(Dialogue.positional);
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x00026A08 File Offset: 0x00024C08
		public int getPortraitIndex()
		{
			if (this.speaker != null && Game1.isGreenRain && this.speaker.Name.Equals("Demetrius") && Game1.year == 1)
			{
				return 7;
			}
			string a = this.CurrentEmotion;
			if (a == "$neutral")
			{
				return 0;
			}
			if (a == "$h")
			{
				return 1;
			}
			if (a == "$s")
			{
				return 2;
			}
			if (a == "$u")
			{
				return 3;
			}
			if (a == "$l")
			{
				return 4;
			}
			if (a == "$a")
			{
				return 5;
			}
			int index;
			if (!int.TryParse(this.CurrentEmotion.Substring(1), out index))
			{
				return 0;
			}
			return index;
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x00026AC0 File Offset: 0x00024CC0
		protected virtual void parseDialogueString(string masterString, string translationKey)
		{
			masterString = TokenParser.ParseText(masterString ?? "...", null, null, null);
			string[] multipleWeeklyDialogueSplit = masterString.Split("||", StringSplitOptions.None);
			if (multipleWeeklyDialogueSplit.Length > 1)
			{
				masterString = multipleWeeklyDialogueSplit[(int)(checked((IntPtr)(unchecked((ulong)(Game1.stats.DaysPlayed / 7U) % (ulong)((long)multipleWeeklyDialogueSplit.Length)))))];
			}
			List<NPCDialogueResponse> list = this.playerResponses;
			if (list != null)
			{
				list.Clear();
			}
			string[] masterDialogueSplit = masterString.Split('#', StringSplitOptions.None);
			for (int i = 0; i < masterDialogueSplit.Length; i++)
			{
				string curDialogue = masterDialogueSplit[i];
				if (curDialogue.Length >= 2)
				{
					curDialogue = (masterDialogueSplit[i] = this.checkForSpecialCharacters(curDialogue));
					bool handledCommand = false;
					if (curDialogue.StartsWith('$'))
					{
						string[] parts = ArgUtility.SplitBySpace(curDialogue, 2);
						string commandToken = parts[0];
						string commandArgs = ArgUtility.Get(parts, 1, null, true);
						handledCommand = true;
						if (commandToken != null)
						{
							int length = commandToken.Length;
							if (length != 2)
							{
								if (length != 6)
								{
									if (length == 7)
									{
										if (commandToken == "$action")
										{
											this.dialogues.Add(new DialogueLine("", delegate()
											{
												string error;
												Exception ex;
												if (!TriggerActionManager.TryRunAction(commandArgs, out error, out ex))
												{
													DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
													defaultInterpolatedStringHandler.AppendLiteral("Failed to parse ");
													defaultInterpolatedStringHandler.AppendFormatted("$action");
													defaultInterpolatedStringHandler.AppendLiteral(" token for ");
													string value;
													if ((value = translationKey) == null)
													{
														NPC npc = this.speaker;
														value = (((npc != null) ? npc.Name : null) ?? ("\"" + masterString + "\""));
													}
													defaultInterpolatedStringHandler.AppendFormatted(value);
													defaultInterpolatedStringHandler.AppendLiteral(": ");
													defaultInterpolatedStringHandler.AppendFormatted(error);
													defaultInterpolatedStringHandler.AppendLiteral(".");
													error = defaultInterpolatedStringHandler.ToStringAndClear();
													if (ex == null)
													{
														Game1.log.Warn(error);
														return;
													}
													Game1.log.Error(error, ex);
												}
											}));
											goto IL_8A5;
										}
									}
								}
								else if (commandToken == "$query")
								{
									string commandArgs2 = commandArgs;
									string text = ArgUtility.Get(masterString.Split('#', 2, StringSplitOptions.None), 1, null, true);
									string[] dialogueOptions = ((text != null) ? text.Split('|', StringSplitOptions.None) : null) ?? LegacyShims.EmptyArray<string>();
									masterDialogueSplit = (GameStateQuery.CheckConditions(commandArgs2, null, null, null, null, null, null) ? dialogueOptions[0].Split('#', StringSplitOptions.None) : ArgUtility.Get(dialogueOptions, 1, dialogueOptions[0], true).Split('#', StringSplitOptions.None));
									i--;
									goto IL_8A5;
								}
							}
							else
							{
								char c = commandToken[1];
								if (c <= 'e')
								{
									if (c != '1')
									{
										switch (c)
										{
										case 'b':
											if (commandToken == "$b")
											{
												if (this.dialogues.Count > 0)
												{
													DialogueLine dialogueLine = this.dialogues[this.dialogues.Count - 1];
													dialogueLine.Text += "{";
													goto IL_8A5;
												}
												goto IL_8A5;
											}
											break;
										case 'c':
											if (commandToken == "$c")
											{
												string rawChance = ArgUtility.SplitBySpaceAndGet(commandArgs, 0, null);
												if (rawChance != null)
												{
													double chance = Convert.ToDouble(rawChance);
													if (!Game1.random.NextBool(chance))
													{
														i++;
														goto IL_8A5;
													}
													this.dialogues.Add(new DialogueLine(masterDialogueSplit[i + 1], null));
													i += 3;
													goto IL_8A5;
												}
											}
											break;
										case 'd':
											if (commandToken == "$d")
											{
												string[] array = ArgUtility.SplitBySpace(commandArgs);
												string prerequisiteDialogue = masterString.Substring(masterString.IndexOf('#') + 1);
												bool worldStateConfirmed = false;
												string a = array[0].ToLower();
												if (!(a == "joja"))
												{
													if (!(a == "cc") && !(a == "communitycenter"))
													{
														if (!(a == "bus"))
														{
															if (a == "kent")
															{
																worldStateConfirmed = (Game1.year >= 2);
															}
														}
														else
														{
															worldStateConfirmed = Game1.MasterPlayer.mailReceived.Contains("ccVault");
														}
													}
													else
													{
														worldStateConfirmed = Game1.isLocationAccessible("CommunityCenter");
													}
												}
												else
												{
													worldStateConfirmed = Game1.isLocationAccessible("JojaMart");
												}
												char toLookFor = prerequisiteDialogue.Contains('|') ? '|' : '#';
												if (worldStateConfirmed)
												{
													masterDialogueSplit = prerequisiteDialogue.Split(toLookFor, StringSplitOptions.None)[0].Split('#', StringSplitOptions.None);
												}
												else
												{
													masterDialogueSplit = prerequisiteDialogue.Split(toLookFor, StringSplitOptions.None)[1].Split('#', StringSplitOptions.None);
												}
												i--;
												goto IL_8A5;
											}
											break;
										case 'e':
											if (commandToken == "$e")
											{
												goto IL_8A5;
											}
											break;
										}
									}
									else if (commandToken == "$1")
									{
										string messageId = ArgUtility.SplitBySpaceAndGet(commandArgs, 0, null);
										if (messageId != null)
										{
											if (!this.farmer.mailReceived.Contains(messageId))
											{
												masterDialogueSplit[i + 1] = this.checkForSpecialCharacters(masterDialogueSplit[i + 1]);
												this.dialogues.Add(new DialogueLine(messageId + "}" + masterDialogueSplit[i + 1], null));
												i = 99999;
												goto IL_8A5;
											}
											i += 3;
											if (i < masterDialogueSplit.Length)
											{
												masterDialogueSplit[i] = this.checkForSpecialCharacters(masterDialogueSplit[i]);
												this.dialogues.Add(new DialogueLine(masterDialogueSplit[i], null));
												goto IL_8A5;
											}
											goto IL_8A5;
										}
									}
								}
								else if (c != 'k')
								{
									switch (c)
									{
									case 'p':
										if (commandToken == "$p")
										{
											string[] prerequisiteSplit = ArgUtility.SplitBySpace(commandArgs);
											string[] prerequisiteDialogueSplit = masterDialogueSplit[i + 1].Split('|', StringSplitOptions.None);
											bool choseOne = false;
											for (int j = 0; j < prerequisiteSplit.Length; j++)
											{
												if (this.farmer.DialogueQuestionsAnswered.Contains(prerequisiteSplit[j]))
												{
													choseOne = true;
													break;
												}
											}
											if (choseOne)
											{
												masterDialogueSplit = prerequisiteDialogueSplit[0].Split('#', StringSplitOptions.None);
												i = -1;
												goto IL_8A5;
											}
											masterDialogueSplit[i + 1] = masterDialogueSplit[i + 1].Split('|', StringSplitOptions.None).Last<string>();
											goto IL_8A5;
										}
										break;
									case 'q':
										if (commandToken == "$q")
										{
											if (this.dialogues.Count > 0)
											{
												DialogueLine dialogueLine2 = this.dialogues[this.dialogues.Count - 1];
												dialogueLine2.Text += "{";
											}
											string[] questionSplit = ArgUtility.SplitBySpace(commandArgs);
											string[] answerIDs = questionSplit[0].Split('/', StringSplitOptions.None);
											bool alreadySeenAnswer = false;
											for (int k = 0; k < answerIDs.Length; k++)
											{
												if (this.farmer.DialogueQuestionsAnswered.Contains(answerIDs[k]))
												{
													alreadySeenAnswer = true;
													break;
												}
											}
											if (!alreadySeenAnswer || !(answerIDs[0] != "-1"))
											{
												this.isLastDialogueInteractive = true;
												goto IL_8A5;
											}
											if (!questionSplit[1].Equals("null"))
											{
												masterDialogueSplit = masterDialogueSplit.Take(i).Concat(this.speaker.Dialogue[questionSplit[1]].Split('#', StringSplitOptions.None)).ToArray<string>();
												i--;
												goto IL_8A5;
											}
											goto IL_8A5;
										}
										break;
									case 'r':
										if (commandToken == "$r")
										{
											string[] responseSplit = ArgUtility.SplitBySpace(commandArgs);
											if (this.playerResponses == null)
											{
												this.playerResponses = new List<NPCDialogueResponse>();
											}
											this.isLastDialogueInteractive = true;
											this.playerResponses.Add(new NPCDialogueResponse(responseSplit[0], Convert.ToInt32(responseSplit[1]), responseSplit[2], masterDialogueSplit[i + 1], null));
											i++;
											goto IL_8A5;
										}
										break;
									case 's':
										break;
									case 't':
										if (commandToken == "$t")
										{
											this.dialogues.Add(new DialogueLine("", delegate()
											{
												string[] fields = ArgUtility.SplitBySpace(commandArgs);
												string topicId;
												string error;
												int daysDuration;
												if (!ArgUtility.TryGet(fields, 0, out topicId, out error, false, "string topicId") || !ArgUtility.TryGetOptionalInt(fields, 1, out daysDuration, out error, 4, "int daysDuration"))
												{
													IGameLogger log = Game1.log;
													DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
													defaultInterpolatedStringHandler.AppendLiteral("Failed to parse ");
													defaultInterpolatedStringHandler.AppendFormatted("$t");
													defaultInterpolatedStringHandler.AppendLiteral(" token for ");
													string value;
													if ((value = translationKey) == null)
													{
														NPC npc = this.speaker;
														value = (((npc != null) ? npc.Name : null) ?? ("\"" + masterString + "\""));
													}
													defaultInterpolatedStringHandler.AppendFormatted(value);
													defaultInterpolatedStringHandler.AppendLiteral(": ");
													defaultInterpolatedStringHandler.AppendFormatted(error);
													defaultInterpolatedStringHandler.AppendLiteral(".");
													log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
													return;
												}
												Game1.player.activeDialogueEvents.TryAdd(topicId, daysDuration);
											}));
											goto IL_8A5;
										}
										break;
									default:
										if (c == 'y')
										{
											if (commandToken == "$y")
											{
												this.quickResponse = true;
												this.isLastDialogueInteractive = true;
												if (this.quickResponses == null)
												{
													this.quickResponses = new List<string>();
												}
												if (this.playerResponses == null)
												{
													this.playerResponses = new List<NPCDialogueResponse>();
												}
												string raw = curDialogue.Substring(curDialogue.IndexOf('\'') + 1);
												raw = raw.Substring(0, raw.Length - 1);
												string[] rawSplit = raw.Split('_', StringSplitOptions.None);
												this.dialogues.Add(new DialogueLine(rawSplit[0], null));
												for (int l = 1; l < rawSplit.Length; l += 2)
												{
													string choice = rawSplit[l];
													string response = rawSplit[l + 1];
													if (response.Contains("*"))
													{
														response = response.Replace("**", "<<<<asterisk>>>>").Replace("*", "#$b#").Replace("<<<<asterisk>>>>", "*");
													}
													this.playerResponses.Add(new NPCDialogueResponse(null, -1, "quickResponse" + l.ToString(), Game1.parseText(choice), null));
													this.quickResponses.Add(response);
												}
												goto IL_8A5;
											}
										}
										break;
									}
								}
								else if (commandToken == "$k")
								{
									goto IL_8A5;
								}
							}
						}
						handledCommand = false;
					}
					IL_8A5:
					if (!handledCommand)
					{
						curDialogue = this.applyGenderSwitch(curDialogue, false);
						this.dialogues.Add(new DialogueLine(curDialogue, null));
					}
				}
			}
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x000273A4 File Offset: 0x000255A4
		public virtual void prepareDialogueForDisplay()
		{
			Friendship friendship;
			if (this.dialogues.Count > 0 && this.speaker != null && this.speaker.shouldWearIslandAttire.Value && Game1.player.friendshipData.TryGetValue(this.speaker.Name, out friendship) && friendship.IsDivorced() && this.CurrentEmotion == "$u")
			{
				this.CurrentEmotion = "$neutral";
			}
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0002741C File Offset: 0x0002561C
		public virtual void prepareCurrentDialogueForDisplay()
		{
			this.applyAndSkipPlainSideEffects();
			if (this.currentDialogueIndex >= this.dialogues.Count)
			{
				return;
			}
			string currentDialogue = this.dialogues[this.currentDialogueIndex].Text;
			currentDialogue = Utility.ParseGiftReveals(currentDialogue);
			this.showPortrait = true;
			if (!currentDialogue.StartsWith("$v"))
			{
				if (currentDialogue.Contains('}'))
				{
					this.farmer.mailReceived.Add(currentDialogue.Split('}', StringSplitOptions.None)[0]);
					currentDialogue = currentDialogue.Substring(currentDialogue.IndexOf("}") + 1);
					currentDialogue = currentDialogue.Replace("$k", "");
				}
				if (currentDialogue.Contains("$k"))
				{
					currentDialogue = currentDialogue.Replace("$k", "");
					this.dialogues.RemoveRange(this.currentDialogueIndex + 1, this.dialogues.Count - 1 - this.currentDialogueIndex);
					if (currentDialogue.Length < 2)
					{
						this.finishedLastDialogue = true;
					}
				}
				if (currentDialogue.StartsWith('%'))
				{
					bool isToken = false;
					foreach (string token in Dialogue.percentTokens)
					{
						if (currentDialogue.StartsWith(token))
						{
							isToken = true;
							break;
						}
					}
					if (!isToken)
					{
						this.indexesWithoutPortrait.Add(this.currentDialogueIndex);
						this.showPortrait = false;
						currentDialogue = currentDialogue.Substring(1);
					}
				}
				else if (this.indexesWithoutPortrait.Contains(this.currentDialogueIndex))
				{
					this.showPortrait = false;
				}
				currentDialogue = this.ReplacePlayerEnteredStrings(currentDialogue);
				if (currentDialogue.Contains('['))
				{
					int open_index = -1;
					do
					{
						open_index = currentDialogue.IndexOf('[', Math.Max(open_index, 0));
						if (open_index >= 0)
						{
							int close_index = currentDialogue.IndexOf(']', open_index);
							if (close_index < 0)
							{
								break;
							}
							string[] split = ArgUtility.SplitBySpace(currentDialogue.Substring(open_index + 1, close_index - open_index - 1));
							bool fail = false;
							string[] array = split;
							for (int i = 0; i < array.Length; i++)
							{
								if (ItemRegistry.GetData(array[i]) == null)
								{
									fail = true;
									break;
								}
							}
							if (fail)
							{
								open_index++;
							}
							else
							{
								Item item = ItemRegistry.Create(Game1.random.Choose(split), 1, 0, false);
								if (item != null)
								{
									if (this.farmer.addItemToInventoryBool(item, true))
									{
										this.farmer.showCarrying();
									}
									else
									{
										this.farmer.addItemByMenuIfNecessary(item, null, true);
									}
								}
								currentDialogue = currentDialogue.Remove(open_index, close_index - open_index + 1);
							}
						}
					}
					while (open_index >= 0 && open_index < currentDialogue.Length);
				}
				currentDialogue = currentDialogue.Replace("%time", Game1.getTimeOfDayString(Game1.timeOfDay));
				NPC npc = this.speaker;
				bool? flag = (npc != null) ? new bool?(npc.SpeaksDwarvish()) : null;
				if (flag != null && flag.GetValueOrDefault() && !this.farmer.canUnderstandDwarves)
				{
					currentDialogue = Dialogue.convertToDwarvish(currentDialogue);
				}
				this.dialogues[this.currentDialogueIndex].Text = currentDialogue;
				return;
			}
			string[] split2 = ArgUtility.SplitBySpace(currentDialogue);
			string eventId = split2[1];
			bool checkPrecondition = true;
			bool checkSeen = true;
			if (split2.Length > 2 && split2[2] == "false")
			{
				checkPrecondition = false;
			}
			if (split2.Length > 3 && split2[3] == "false")
			{
				checkSeen = false;
			}
			if (Game1.PlayEvent(eventId, checkPrecondition, checkSeen))
			{
				this.dialogues.Clear();
				this.exitCurrentDialogue();
				return;
			}
			this.exitCurrentDialogue();
			if (!this.isDialogueFinished())
			{
				this.prepareCurrentDialogueForDisplay();
			}
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x00027774 File Offset: 0x00025974
		public virtual string getCurrentDialogue()
		{
			if (this.currentDialogueIndex >= this.dialogues.Count || this.finishedLastDialogue)
			{
				return "";
			}
			if (this.dialogues.Count <= 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.792");
			}
			return this.dialogues[this.currentDialogueIndex].Text;
		}

		// Token: 0x060006D6 RID: 1750 RVA: 0x000277D6 File Offset: 0x000259D6
		public bool isItemGrabDialogue()
		{
			return this.currentDialogueIndex < this.dialogues.Count && this.dialogues[this.currentDialogueIndex].Text.Contains('[');
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x0002780C File Offset: 0x00025A0C
		public bool isOnFinalDialogue()
		{
			for (int i = this.currentDialogueIndex + 1; i < this.dialogues.Count; i++)
			{
				if (this.dialogues[i].HasText)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x0002784C File Offset: 0x00025A4C
		public bool isDialogueFinished()
		{
			return this.finishedLastDialogue;
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x00027854 File Offset: 0x00025A54
		public string ReplacePlayerEnteredStrings(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			string farmer_name = Utility.FilterUserName(this.farmer.Name);
			str = str.Replace("@", farmer_name);
			if (str.Contains('%'))
			{
				str = str.Replace("%firstnameletter", farmer_name.Substring(0, Math.Max(0, farmer_name.Length / 2)));
				if (str.Contains("%spouse"))
				{
					if (this.farmer.spouse != null)
					{
						string spouseName = NPC.GetDisplayName(this.farmer.spouse);
						str = str.Replace("%spouse", spouseName);
					}
					else
					{
						long? spouseId = this.farmer.team.GetSpouse(this.farmer.UniqueMultiplayerID);
						if (spouseId != null)
						{
							Farmer spouse = Game1.GetPlayer(spouseId.Value, false);
							str = str.Replace("%spouse", spouse.Name);
						}
					}
				}
				string farmName = Utility.FilterUserName(this.farmer.farmName.Value);
				str = str.Replace("%farm", farmName);
				string favoriteThing = Utility.FilterUserName(this.farmer.favoriteThing.Value);
				str = str.Replace("%favorite", favoriteThing);
				int kids = this.farmer.getNumberOfChildren();
				str = str.Replace("%kid1", (kids > 0) ? this.farmer.getChildren()[0].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.793"));
				str = str.Replace("%kid2", (kids > 1) ? this.farmer.getChildren()[1].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.794"));
				str = str.Replace("%pet", this.farmer.getPetDisplayName());
			}
			return str;
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x00027A1C File Offset: 0x00025C1C
		public string checkForSpecialCharacters(string str)
		{
			str = this.applyGenderSwitch(str, true);
			if (str.Contains('%'))
			{
				str = str.Replace("%adj", Game1.random.Choose(Dialogue.adjectives).ToLower());
				if (str.Contains("%noun"))
				{
					str = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns)) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns))) : (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns).ToLower()) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns).ToLower())));
				}
				str = str.Replace("%place", Game1.random.Choose(Dialogue.places));
				str = str.Replace("%name", Dialogue.randomName());
				str = str.Replace("%band", Game1.samBandName);
				if (str.Contains("%book"))
				{
					str = str.Replace("%book", Game1.elliottBookName);
				}
				str = str.Replace("%year", Game1.year.ToString() ?? "");
				str = str.Replace("%season", Game1.CurrentSeasonDisplayName);
				if (str.Contains("%fork"))
				{
					str = str.Replace("%fork", "");
					if (Game1.currentLocation.currentEvent != null)
					{
						Game1.currentLocation.currentEvent.specialEventVariable1 = true;
					}
				}
			}
			return str;
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x00027C34 File Offset: 0x00025E34
		public string applyGenderSwitch(string str, bool altTokenOnly = false)
		{
			return Dialogue.applyGenderSwitch(this.farmer.Gender, str, altTokenOnly);
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x00027C48 File Offset: 0x00025E48
		public static string applyGenderSwitch(Gender gender, string str, bool altTokenOnly = false)
		{
			str = Dialogue.applyGenderSwitchBlocks(gender, str);
			int splitIndex = (!altTokenOnly) ? str.IndexOf('^') : -1;
			if (splitIndex == -1)
			{
				splitIndex = str.IndexOf('¦');
			}
			if (splitIndex != -1)
			{
				str = ((gender == Gender.Male) ? str.Substring(0, splitIndex) : str.Substring(splitIndex + 1));
			}
			return str;
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x00027C9C File Offset: 0x00025E9C
		public static string applyGenderSwitchBlocks(Gender gender, string str)
		{
			int startIndex = 0;
			for (;;)
			{
				int index = str.IndexOf("${", startIndex, StringComparison.Ordinal);
				if (index == -1)
				{
					break;
				}
				int endIndex = str.IndexOf("}$", index, StringComparison.Ordinal);
				if (endIndex == -1)
				{
					return str;
				}
				string originalSubstr = str.Substring(index + 2, endIndex - index - 2);
				string[] parts = originalSubstr.Contains('¦') ? originalSubstr.Split('¦', StringSplitOptions.None) : originalSubstr.Split('^', StringSplitOptions.None);
				string newSubstr;
				if (gender != Gender.Male)
				{
					if (gender != Gender.Female)
					{
						newSubstr = ArgUtility.Get(parts, 2, parts[0], true);
					}
					else
					{
						newSubstr = ArgUtility.Get(parts, 1, parts[0], true);
					}
				}
				else
				{
					newSubstr = parts[0];
				}
				str = str.Substring(0, index) + newSubstr + str.Substring(endIndex + "}$".Length);
				startIndex = index + newSubstr.Length;
			}
			return str;
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x00027D6C File Offset: 0x00025F6C
		public void applyAndSkipPlainSideEffects()
		{
			while (this.currentDialogueIndex < this.dialogues.Count)
			{
				DialogueLine entry = this.dialogues[this.currentDialogueIndex];
				if (entry.HasText)
				{
					break;
				}
				Action sideEffects = entry.SideEffects;
				if (sideEffects != null)
				{
					sideEffects();
				}
				this.currentDialogueIndex++;
			}
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x00027DC8 File Offset: 0x00025FC8
		public static string randomName()
		{
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.ja:
			{
				string[] names = new string[]
				{
					"ローゼン",
					"ミルド",
					"ココ",
					"ナミ",
					"こころ",
					"サルコ",
					"ハンゾー",
					"クッキー",
					"ココナツ",
					"せん",
					"ハル",
					"ラン",
					"オサム",
					"ヨシ",
					"ソラ",
					"ホシ",
					"まこと",
					"マサ",
					"ナナ",
					"リオ",
					"リン",
					"フジ",
					"うどん",
					"ミント",
					"さくら",
					"ボンボン",
					"レオ",
					"モリ",
					"コーヒー",
					"ミルク",
					"マロン",
					"クルミ",
					"サムライ",
					"カミ",
					"ゴロ",
					"マル",
					"チビ",
					"ユキダマ"
				};
				return Game1.random.Choose(names);
			}
			case LocalizedContentManager.LanguageCode.ru:
			{
				string[] names2 = new string[]
				{
					"Августина",
					"Альф",
					"Анфиса",
					"Ариша",
					"Афоня",
					"Баламут",
					"Балкан",
					"Бандит",
					"Бланка",
					"Бобик",
					"Боня",
					"Борька",
					"Буренка",
					"Бусинка",
					"Вася",
					"Гаврюша",
					"Глаша",
					"Гоша",
					"Дуня",
					"Дуся",
					"Зорька",
					"Ивонна",
					"Игнат",
					"Кеша",
					"Клара",
					"Кузя",
					"Лада",
					"Максимус",
					"Маня",
					"Марта",
					"Маруся",
					"Моня",
					"Мотя",
					"Мурзик",
					"Мурка",
					"Нафаня",
					"Ника",
					"Нюша",
					"Проша",
					"Пятнушка",
					"Сеня",
					"Сивка",
					"Тихон",
					"Тоша",
					"Фунтик",
					"Шайтан",
					"Юнона",
					"Юпитер",
					"Ягодка",
					"Яшка"
				};
				return Game1.random.Choose(names2);
			}
			case LocalizedContentManager.LanguageCode.zh:
			{
				string[] names3 = new string[]
				{
					"雨果",
					"蛋挞",
					"小百合",
					"毛毛",
					"小雨",
					"小溪",
					"精灵",
					"安琪儿",
					"小糕",
					"玫瑰",
					"小黄",
					"晓雨",
					"阿江",
					"铃铛",
					"马琪",
					"果粒",
					"郁金香",
					"小黑",
					"雨露",
					"小江",
					"灵力",
					"萝拉",
					"豆豆",
					"小莲",
					"斑点",
					"小雾",
					"阿川",
					"丽丹",
					"玛雅",
					"阿豆",
					"花花",
					"琉璃",
					"滴答",
					"阿山",
					"丹麦",
					"梅西",
					"橙子",
					"花儿",
					"晓璃",
					"小夕",
					"山大",
					"咪咪",
					"卡米",
					"红豆",
					"花朵",
					"洋洋",
					"太阳",
					"小岩",
					"汪汪",
					"玛利亚",
					"小菜",
					"花瓣",
					"阳阳",
					"小夏",
					"石头",
					"阿狗",
					"邱洁",
					"苹果",
					"梨花",
					"小希",
					"天天",
					"浪子",
					"阿猫",
					"艾薇儿",
					"雪梨",
					"桃花",
					"阿喜",
					"云朵",
					"风儿",
					"狮子",
					"绮丽",
					"雪莉",
					"樱花",
					"小喜",
					"朵朵",
					"田田",
					"小红",
					"宝娜",
					"梅子",
					"小樱",
					"嘻嘻",
					"云儿",
					"小草",
					"小黄",
					"纳香",
					"阿梅",
					"茶花",
					"哈哈",
					"芸儿",
					"东东",
					"小羽",
					"哈豆",
					"桃子",
					"茶叶",
					"双双",
					"沫沫",
					"楠楠",
					"小爱",
					"麦当娜",
					"杏仁",
					"椰子",
					"小王",
					"泡泡",
					"小林",
					"小灰",
					"马格",
					"鱼蛋",
					"小叶",
					"小李",
					"晨晨",
					"小琳",
					"小慧",
					"布鲁",
					"晓梅",
					"绿叶",
					"甜豆",
					"小雪",
					"晓林",
					"康康",
					"安妮",
					"樱桃",
					"香板",
					"甜甜",
					"雪花",
					"虹儿",
					"美美",
					"葡萄",
					"薇儿",
					"金豆",
					"雪玲",
					"瑶瑶",
					"龙眼",
					"丁香",
					"晓云",
					"雪豆",
					"琪琪",
					"麦子",
					"糖果",
					"雪丽",
					"小艺",
					"小麦",
					"小圆",
					"雨佳",
					"小火",
					"麦茶",
					"圆圆",
					"春儿",
					"火灵",
					"板子",
					"黑点",
					"冬冬",
					"火花",
					"米粒",
					"喇叭",
					"晓秋",
					"跟屁虫",
					"米果",
					"欢欢",
					"爱心",
					"松子",
					"丫头",
					"双子",
					"豆芽",
					"小子",
					"彤彤",
					"棉花糖",
					"阿贵",
					"仙儿",
					"冰淇淋",
					"小彬",
					"贤儿",
					"冰棒",
					"仔仔",
					"格子",
					"水果",
					"悠悠",
					"莹莹",
					"巧克力",
					"梦洁",
					"汤圆",
					"静香",
					"茄子",
					"珍珠"
				};
				return Game1.random.Choose(names3);
			}
			default:
			{
				int nameLength = Game1.random.Next(3, 6);
				string[] startingConsonants = new string[]
				{
					"B",
					"Br",
					"J",
					"F",
					"S",
					"M",
					"C",
					"Ch",
					"L",
					"P",
					"K",
					"W",
					"G",
					"Z",
					"Tr",
					"T",
					"Gr",
					"Fr",
					"Pr",
					"N",
					"Sn",
					"R",
					"Sh",
					"St"
				};
				string[] consonants = new string[]
				{
					"ll",
					"tch",
					"l",
					"m",
					"n",
					"p",
					"r",
					"s",
					"t",
					"c",
					"rt",
					"ts"
				};
				string[] vowels = new string[]
				{
					"a",
					"e",
					"i",
					"o",
					"u"
				};
				string[] consonantEndings = new string[]
				{
					"ie",
					"o",
					"a",
					"ers",
					"ley"
				};
				Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
				dictionary["a"] = new string[]
				{
					"nie",
					"bell",
					"bo",
					"boo",
					"bella",
					"s"
				};
				dictionary["e"] = new string[]
				{
					"ll",
					"llo",
					"",
					"o"
				};
				dictionary["i"] = new string[]
				{
					"ck",
					"e",
					"bo",
					"ba",
					"lo",
					"la",
					"to",
					"ta",
					"no",
					"na",
					"ni",
					"a",
					"o",
					"zor",
					"que",
					"ca",
					"co",
					"mi"
				};
				dictionary["o"] = new string[]
				{
					"nie",
					"ze",
					"dy",
					"da",
					"o",
					"ver",
					"la",
					"lo",
					"s",
					"ny",
					"mo",
					"ra"
				};
				dictionary["u"] = new string[]
				{
					"rt",
					"mo",
					"",
					"s"
				};
				Dictionary<string, string[]> endings = dictionary;
				dictionary = new Dictionary<string, string[]>();
				dictionary["a"] = new string[]
				{
					"nny",
					"sper",
					"trina",
					"bo",
					"-bell",
					"boo",
					"lbert",
					"sko",
					"sh",
					"ck",
					"ishe",
					"rk"
				};
				dictionary["e"] = new string[]
				{
					"lla",
					"llo",
					"rnard",
					"cardo",
					"ffe",
					"ppo",
					"ppa",
					"tch",
					"x"
				};
				dictionary["i"] = new string[]
				{
					"llard",
					"lly",
					"lbo",
					"cky",
					"card",
					"ne",
					"nnie",
					"lbert",
					"nono",
					"nano",
					"nana",
					"ana",
					"nsy",
					"msy",
					"skers",
					"rdo",
					"rda",
					"sh"
				};
				dictionary["o"] = new string[]
				{
					"nie",
					"zzy",
					"do",
					"na",
					"la",
					"la",
					"ver",
					"ng",
					"ngus",
					"ny",
					"-mo",
					"llo",
					"ze",
					"ra",
					"ma",
					"cco",
					"z"
				};
				dictionary["u"] = new string[]
				{
					"ssie",
					"bbie",
					"ffy",
					"bba",
					"rt",
					"s",
					"mby",
					"mbo",
					"mbus",
					"ngus",
					"cky"
				};
				Dictionary<string, string[]> endingsForShortNames = dictionary;
				string name = startingConsonants[Game1.random.Next(startingConsonants.Length - 1)];
				for (int i = 1; i < nameLength - 1; i++)
				{
					if (i % 2 == 0)
					{
						name += Game1.random.Choose(consonants);
					}
					else
					{
						name += Game1.random.Choose(vowels);
					}
					if (name.Length >= nameLength)
					{
						break;
					}
				}
				string lastLetter = name[name.Length - 1].ToString();
				if (Game1.random.NextBool() && !vowels.Contains(lastLetter))
				{
					name += Game1.random.Choose(consonantEndings);
				}
				else if (vowels.Contains(lastLetter))
				{
					if (Game1.random.NextDouble() < 0.8)
					{
						if (name.Length <= 3)
						{
							name += Game1.random.ChooseFrom(endingsForShortNames[lastLetter]);
						}
						else
						{
							name += Game1.random.ChooseFrom(endings[lastLetter]);
						}
					}
				}
				else
				{
					name += Game1.random.Choose(vowels);
				}
				for (int j = name.Length - 1; j > 2; j--)
				{
					if (vowels.Contains(name[j].ToString()) && vowels.Contains(name[j - 2].ToString()))
					{
						char c = name[j - 1];
						if (c != 'c')
						{
							if (c != 'l')
							{
								if (c == 'r')
								{
									name = name.Substring(0, j - 1) + "k" + name.Substring(j);
									j--;
								}
							}
							else
							{
								name = name.Substring(0, j - 1) + "n" + name.Substring(j);
								j--;
							}
						}
						else
						{
							name = name.Substring(0, j) + "k" + name.Substring(j);
							j--;
						}
					}
				}
				if (name.Length <= 3 && Game1.random.NextDouble() < 0.1)
				{
					name = (Game1.random.NextBool() ? (name + name) : (name + "-" + name));
				}
				if (name.Length <= 2 && name.Last<char>() == 'e')
				{
					name += Game1.random.Choose('m', 'p', 'b').ToString();
				}
				return Dialogue.ReplaceBadRandomName(name);
			}
			}
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x00029104 File Offset: 0x00027304
		public static string ReplaceBadRandomName(string name)
		{
			string lowerName = name.ToLower();
			if (lowerName.Contains("bitch") || lowerName.Contains("cock") || lowerName.Contains("cum") || lowerName.Contains("fuck") || lowerName.Contains("goock") || lowerName.Contains("gook") || lowerName.Contains("kike") || lowerName.Contains("nigg") || lowerName.Contains("pusie") || lowerName.Contains("puss") || lowerName.Contains("puta") || lowerName.Contains("rape") || lowerName.Contains("sex") || lowerName.Contains("shart") || lowerName.Contains("shit") || lowerName.Contains("taboo") || lowerName.Contains("trann") || lowerName.Contains("willy"))
			{
				return Game1.random.Choose("Bobo", "Wumbus");
			}
			if (lowerName != null)
			{
				switch (lowerName.Length)
				{
				case 5:
				{
					char c = lowerName[3];
					if (c != 'e')
					{
						switch (c)
						{
						case 'i':
							if (!(lowerName == "rapie"))
							{
								return name;
							}
							return "Rapimi";
						case 'j':
						case 'l':
						case 'm':
						case 'q':
						case 'r':
						case 't':
							return name;
						case 'k':
							if (lowerName == "cucka" || lowerName == "cucke" || lowerName == "cucko" || lowerName == "cucky")
							{
								goto IL_3AB;
							}
							if (!(lowerName == "packi"))
							{
								return name;
							}
							goto IL_3BD;
						case 'n':
							if (!(lowerName == "trani"))
							{
								return name;
							}
							goto IL_3CF;
						case 'o':
							if (!(lowerName == "penos"))
							{
								return name;
							}
							break;
						case 'p':
							if (lowerName == "grope")
							{
								goto IL_3B1;
							}
							if (!(lowerName == "trapi"))
							{
								return name;
							}
							goto IL_3CF;
						case 's':
							if (!(lowerName == "natsi"))
							{
								return name;
							}
							return "Natsia";
						case 'u':
							if (!(lowerName == "penus"))
							{
								return name;
							}
							break;
						default:
							return name;
						}
						return "Penono";
					}
					if (!(lowerName == "boner"))
					{
						return name;
					}
					break;
				}
				case 6:
				{
					char c = lowerName[3];
					if (c <= 'k')
					{
						if (c != 'e')
						{
							if (c != 'k')
							{
								return name;
							}
							if (lowerName == "cuckas" || lowerName == "cuckie" || lowerName == "cuckos")
							{
								goto IL_3AB;
							}
							if (!(lowerName == "packie"))
							{
								return name;
							}
							goto IL_3BD;
						}
						else if (!(lowerName == "boners"))
						{
							return name;
						}
					}
					else if (c != 'n')
					{
						if (c != 'p')
						{
							if (c != 's')
							{
								return name;
							}
							if (!(lowerName == "bussie"))
							{
								return name;
							}
							return "Busu";
						}
						else
						{
							if (!(lowerName == "trapie"))
							{
								return name;
							}
							goto IL_3CF;
						}
					}
					else
					{
						if (!(lowerName == "tranie"))
						{
							return name;
						}
						goto IL_3CF;
					}
					break;
				}
				case 7:
				{
					char c = lowerName[0];
					if (c != 'c')
					{
						if (c != 'g')
						{
							return name;
						}
						if (!(lowerName == "gropers"))
						{
							return name;
						}
						goto IL_3B1;
					}
					else
					{
						if (!(lowerName == "cuckers"))
						{
							return name;
						}
						goto IL_3AB;
					}
					break;
				}
				case 8:
					if (!(lowerName == "trananie"))
					{
						return name;
					}
					goto IL_3CF;
				default:
					return name;
				}
				return "Boneo";
				IL_3AB:
				return "Cubbie";
				IL_3B1:
				return "Gropello";
				IL_3BD:
				return "Packina";
				IL_3CF:
				return "Tranello";
			}
			return name;
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x000294E8 File Offset: 0x000276E8
		public virtual string exitCurrentDialogue()
		{
			if (this.isOnFinalDialogue())
			{
				this.currentDialogueIndex++;
				this.applyAndSkipPlainSideEffects();
				Action action = this.onFinish;
				if (action != null)
				{
					action();
				}
			}
			bool flag = this.isCurrentStringContinuedOnNextScreen;
			if (this.currentDialogueIndex < this.dialogues.Count - 1)
			{
				this.currentDialogueIndex++;
				this.applyAndSkipPlainSideEffects();
				this.checkForSpecialDialogueAttributes();
			}
			else
			{
				this.finishedLastDialogue = true;
			}
			if (flag)
			{
				return this.getCurrentDialogue();
			}
			return null;
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0002956C File Offset: 0x0002776C
		private void checkForSpecialDialogueAttributes()
		{
			this.CurrentEmotion = null;
			this.isCurrentStringContinuedOnNextScreen = false;
			this.dontFaceFarmer = false;
			if (this.currentDialogueIndex >= this.dialogues.Count)
			{
				return;
			}
			DialogueLine dialogueLine = this.dialogues[this.currentDialogueIndex];
			if (dialogueLine.Text.Contains("{"))
			{
				dialogueLine.Text = dialogueLine.Text.Replace("{", "");
				this.isCurrentStringContinuedOnNextScreen = true;
			}
			if (dialogueLine.Text.Contains("%noturn"))
			{
				dialogueLine.Text = dialogueLine.Text.Replace("%noturn", "");
				this.dontFaceFarmer = true;
			}
			this.checkEmotions();
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x00029624 File Offset: 0x00027824
		private void checkEmotions()
		{
			this.CurrentEmotion = null;
			if (this.currentDialogueIndex >= this.dialogues.Count)
			{
				return;
			}
			DialogueLine dialogueLine = this.dialogues[this.currentDialogueIndex];
			string text = dialogueLine.Text;
			int emoteIndex = text.IndexOf('$');
			if (emoteIndex == -1)
			{
				return;
			}
			if (this.dialogues.Count > 0)
			{
				if (text.Contains("$h"))
				{
					this.CurrentEmotion = "$h";
					dialogueLine.Text = text.Replace("$h", "");
					return;
				}
				if (text.Contains("$s"))
				{
					this.CurrentEmotion = "$s";
					dialogueLine.Text = text.Replace("$s", "");
					return;
				}
				if (text.Contains("$u"))
				{
					this.CurrentEmotion = "$u";
					dialogueLine.Text = text.Replace("$u", "");
					return;
				}
				if (text.Contains("$l"))
				{
					this.CurrentEmotion = "$l";
					dialogueLine.Text = text.Replace("$l", "");
					return;
				}
				if (text.Contains("$a"))
				{
					this.CurrentEmotion = "$a";
					dialogueLine.Text = text.Replace("$a", "");
					return;
				}
				int digits = 0;
				int i = emoteIndex + 1;
				while (i < text.Length && char.IsDigit(text[i]))
				{
					digits++;
					i++;
				}
				if (digits > 0)
				{
					string emote = text.Substring(emoteIndex, digits + 1);
					this.CurrentEmotion = emote;
					dialogueLine.Text = text.Replace(emote, "");
				}
			}
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x000297C6 File Offset: 0x000279C6
		public List<NPCDialogueResponse> getNPCResponseOptions()
		{
			return this.playerResponses;
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x000297CE File Offset: 0x000279CE
		public Response[] getResponseOptions()
		{
			return this.playerResponses.Cast<Response>().ToArray<Response>();
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x000297E0 File Offset: 0x000279E0
		public bool isCurrentDialogueAQuestion()
		{
			return this.isLastDialogueInteractive && this.currentDialogueIndex == this.dialogues.Count - 1;
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x00029804 File Offset: 0x00027A04
		public virtual bool chooseResponse(Response response)
		{
			int i = 0;
			while (i < this.playerResponses.Count)
			{
				if (this.playerResponses[i].responseKey != null && response.responseKey != null && this.playerResponses[i].responseKey.Equals(response.responseKey))
				{
					if (this.answerQuestionBehavior != null)
					{
						if (this.answerQuestionBehavior(i))
						{
							Game1.currentSpeaker = null;
						}
						this.isLastDialogueInteractive = false;
						this.finishedLastDialogue = true;
						this.answerQuestionBehavior = null;
						return true;
					}
					if (this.quickResponse)
					{
						this.isLastDialogueInteractive = false;
						this.finishedLastDialogue = true;
						this.isCurrentStringContinuedOnNextScreen = true;
						this.speaker.setNewDialogue(new Dialogue(this.speaker, null, this.quickResponses[i]), false, false);
						Game1.drawDialogue(this.speaker);
						this.speaker.faceTowardFarmerForPeriod(4000, 3, false, this.farmer);
						return true;
					}
					if (Game1.isFestival())
					{
						Game1.currentLocation.currentEvent.answerDialogueQuestion(this.speaker, this.playerResponses[i].responseKey);
						this.isLastDialogueInteractive = false;
						this.finishedLastDialogue = true;
						return false;
					}
					this.farmer.changeFriendship(this.playerResponses[i].friendshipChange, this.speaker);
					if (this.playerResponses[i].id != null)
					{
						this.farmer.addSeenResponse(this.playerResponses[i].id);
					}
					if (this.playerResponses[i].extraArgument != null)
					{
						try
						{
							this.performDialogueResponseExtraArgument(this.farmer, this.playerResponses[i].extraArgument);
						}
						catch (Exception)
						{
						}
					}
					this.isLastDialogueInteractive = false;
					this.finishedLastDialogue = false;
					this.parseDialogueString(this.speaker.Dialogue[this.playerResponses[i].responseKey], this.speaker.LoadedDialogueKey + ":" + this.playerResponses[i].responseKey);
					this.isCurrentStringContinuedOnNextScreen = true;
					return false;
				}
				else
				{
					i++;
				}
			}
			return false;
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x00029A44 File Offset: 0x00027C44
		public void performDialogueResponseExtraArgument(Farmer farmer, string argument)
		{
			string[] split = argument.Split("_", StringSplitOptions.None);
			if (split[0].EqualsIgnoreCase("friend"))
			{
				farmer.changeFriendship(Convert.ToInt32(split[2]), Game1.getCharacterFromName(split[1], true, false));
			}
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x00029A88 File Offset: 0x00027C88
		public void convertToDwarvish()
		{
			for (int i = 0; i < this.dialogues.Count; i++)
			{
				this.dialogues[i].Text = Dialogue.convertToDwarvish(this.dialogues[i].Text);
			}
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x00029AD4 File Offset: 0x00027CD4
		public static string convertToDwarvish(string str)
		{
			if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
			{
				string charset = "bcdfghjklmnpqrstvwxyz";
				string charset2 = "bcd fghj klmn pqrst vwxy z";
				StringBuilder result = new StringBuilder();
				bool nextCapital = true;
				foreach (char cjk in str)
				{
					int code = (int)cjk;
					if ((19968 <= code && code <= 40959) || (12352 <= code && code <= 12543) || cjk == '々' || (44032 <= code && code <= 55215))
					{
						char @char = charset[code % charset.Length];
						if (nextCapital)
						{
							@char = char.ToUpper(@char);
							nextCapital = false;
						}
						result.Append(@char);
						char char2 = charset2[(code >> 1) % charset2.Length];
						result.Append(char2);
					}
					else
					{
						result.Append(cjk);
						if (cjk != ' ')
						{
							nextCapital = true;
						}
					}
				}
				return result.ToString();
			}
			StringBuilder translated = new StringBuilder();
			int i = 0;
			while (i < str.Length)
			{
				char c = str[i];
				if (c <= '?')
				{
					if (c <= '\'')
					{
						if (c != '\n')
						{
							switch (c)
							{
							case ' ':
							case '!':
							case '"':
								goto IL_3B4;
							default:
								if (c != '\'')
								{
									goto IL_3C6;
								}
								goto IL_3B4;
							}
						}
					}
					else if (c <= '5')
					{
						switch (c)
						{
						case ',':
						case '.':
							goto IL_3B4;
						case '-':
						case '/':
							goto IL_3C6;
						case '0':
							translated.Append('Q');
							break;
						case '1':
							translated.Append('M');
							break;
						default:
							if (c != '5')
							{
								goto IL_3C6;
							}
							translated.Append('X');
							break;
						}
					}
					else if (c != '9')
					{
						if (c != '?')
						{
							goto IL_3C6;
						}
						goto IL_3B4;
					}
					else
					{
						translated.Append('V');
					}
				}
				else if (c <= 'I')
				{
					if (c != 'A')
					{
						if (c != 'E')
						{
							if (c != 'I')
							{
								goto IL_3C6;
							}
							translated.Append("E");
						}
						else
						{
							translated.Append('U');
						}
					}
					else
					{
						translated.Append('O');
					}
				}
				else if (c <= 'u')
				{
					if (c != 'O')
					{
						switch (c)
						{
						case 'U':
							translated.Append("I");
							break;
						case 'V':
						case 'W':
						case 'X':
						case '[':
						case '\\':
						case ']':
						case '^':
						case '_':
						case '`':
						case 'b':
						case 'f':
						case 'j':
						case 'k':
						case 'l':
						case 'q':
						case 'r':
							goto IL_3C6;
						case 'Y':
							translated.Append("Ol");
							break;
						case 'Z':
							translated.Append('B');
							break;
						case 'a':
							translated.Append('o');
							break;
						case 'c':
							translated.Append('t');
							break;
						case 'd':
							translated.Append('p');
							break;
						case 'e':
							translated.Append('u');
							break;
						case 'g':
							translated.Append('l');
							break;
						case 'h':
						case 'm':
						case 's':
							goto IL_3B4;
						case 'i':
							translated.Append("e");
							break;
						case 'n':
						case 'p':
							break;
						case 'o':
							translated.Append('a');
							break;
						case 't':
							translated.Append('n');
							break;
						case 'u':
							translated.Append("i");
							break;
						default:
							goto IL_3C6;
						}
					}
					else
					{
						translated.Append('A');
					}
				}
				else if (c != 'y')
				{
					if (c != 'z')
					{
						goto IL_3C6;
					}
					translated.Append('b');
				}
				else
				{
					translated.Append("ol");
				}
				IL_3E8:
				i++;
				continue;
				IL_3B4:
				translated.Append(str[i]);
				goto IL_3E8;
				IL_3C6:
				if (char.IsLetterOrDigit(str[i]))
				{
					translated.Append(str[i] + '\u0002');
					goto IL_3E8;
				}
				goto IL_3E8;
			}
			return translated.ToString().Replace("nhu", "doo");
		}

		// Token: 0x04000350 RID: 848
		public const string dialogueHappy = "$h";

		// Token: 0x04000351 RID: 849
		public const string dialogueSad = "$s";

		// Token: 0x04000352 RID: 850
		public const string dialogueUnique = "$u";

		// Token: 0x04000353 RID: 851
		public const string dialogueNeutral = "$neutral";

		// Token: 0x04000354 RID: 852
		public const string dialogueLove = "$l";

		// Token: 0x04000355 RID: 853
		public const string dialogueAngry = "$a";

		// Token: 0x04000356 RID: 854
		public const string dialogueEnd = "$e";

		// Token: 0x04000357 RID: 855
		public const char dialogueCommandPrefix = '$';

		// Token: 0x04000358 RID: 856
		public const string dialogueBreak = "$b";

		// Token: 0x04000359 RID: 857
		public const string dialogueBreakDelimited = "#$b#";

		// Token: 0x0400035A RID: 858
		public const string multipleDialogueDelineator = "||";

		// Token: 0x0400035B RID: 859
		public const string dialogueKill = "$k";

		// Token: 0x0400035C RID: 860
		public const string dialogueChance = "$c";

		// Token: 0x0400035D RID: 861
		public const string dialogueDependingOnWorldState = "$d";

		// Token: 0x0400035E RID: 862
		public const string dialogueEvent = "$v";

		// Token: 0x0400035F RID: 863
		public const string dialogueQuickResponse = "$y";

		// Token: 0x04000360 RID: 864
		public const string dialoguePrerequisite = "$p";

		// Token: 0x04000361 RID: 865
		public const string dialogueSingle = "$1";

		// Token: 0x04000362 RID: 866
		public const string dialogueGameStateQuery = "$query";

		// Token: 0x04000363 RID: 867
		public const string dialogueGenderSwitch_startBlock = "${";

		// Token: 0x04000364 RID: 868
		public const string dialogueGenderSwitch_endBlock = "}$";

		// Token: 0x04000365 RID: 869
		public const string dialogueRunAction = "$action";

		// Token: 0x04000366 RID: 870
		public const string dialogueStartConversationTopic = "$t";

		// Token: 0x04000367 RID: 871
		public const string dialogueQuestion = "$q";

		// Token: 0x04000368 RID: 872
		public const string dialogueResponse = "$r";

		// Token: 0x04000369 RID: 873
		public const string breakSpecialCharacter = "{";

		// Token: 0x0400036A RID: 874
		public const string playerNameSpecialCharacter = "@";

		// Token: 0x0400036B RID: 875
		public const char genderDialogueSplitCharacter = '^';

		// Token: 0x0400036C RID: 876
		public const char genderDialogueSplitCharacter2 = '¦';

		// Token: 0x0400036D RID: 877
		public const string quickResponseDelineator = "*";

		// Token: 0x0400036E RID: 878
		public const string randomAdjectiveSpecialCharacter = "%adj";

		// Token: 0x0400036F RID: 879
		public const string randomNounSpecialCharacter = "%noun";

		// Token: 0x04000370 RID: 880
		public const string randomPlaceSpecialCharacter = "%place";

		// Token: 0x04000371 RID: 881
		public const string spouseSpecialCharacter = "%spouse";

		// Token: 0x04000372 RID: 882
		public const string randomNameSpecialCharacter = "%name";

		// Token: 0x04000373 RID: 883
		public const string firstNameLettersSpecialCharacter = "%firstnameletter";

		// Token: 0x04000374 RID: 884
		public const string timeSpecialCharacter = "%time";

		// Token: 0x04000375 RID: 885
		public const string bandNameSpecialCharacter = "%band";

		// Token: 0x04000376 RID: 886
		public const string bookNameSpecialCharacter = "%book";

		// Token: 0x04000377 RID: 887
		public const string petSpecialCharacter = "%pet";

		// Token: 0x04000378 RID: 888
		public const string farmNameSpecialCharacter = "%farm";

		// Token: 0x04000379 RID: 889
		public const string favoriteThingSpecialCharacter = "%favorite";

		// Token: 0x0400037A RID: 890
		public const string eventForkSpecialCharacter = "%fork";

		// Token: 0x0400037B RID: 891
		public const string yearSpecialCharacter = "%year";

		// Token: 0x0400037C RID: 892
		public const string kid1specialCharacter = "%kid1";

		// Token: 0x0400037D RID: 893
		public const string kid2SpecialCharacter = "%kid2";

		// Token: 0x0400037E RID: 894
		public const string revealTasteCharacter = "%revealtaste";

		// Token: 0x0400037F RID: 895
		public const string seasonCharacter = "%season";

		// Token: 0x04000380 RID: 896
		public const string dontfacefarmer = "%noturn";

		// Token: 0x04000381 RID: 897
		public const char noPortraitPrefix = '%';

		// Token: 0x04000382 RID: 898
		public const string FallbackDialogueForErrorKey = "Strings\\Characters:FallbackDialogueForError";

		// Token: 0x04000383 RID: 899
		public static readonly string[] percentTokens = new string[]
		{
			"%adj",
			"%noun",
			"%place",
			"%spouse",
			"%name",
			"%firstnameletter",
			"%time",
			"%band",
			"%book",
			"%pet",
			"%farm",
			"%favorite",
			"%fork",
			"%year",
			"%kid1",
			"%kid2",
			"%revealtaste",
			"%season"
		};

		// Token: 0x04000384 RID: 900
		private static bool nameArraysTranslated = false;

		// Token: 0x04000385 RID: 901
		public static string[] adjectives = new string[]
		{
			"Purple",
			"Gooey",
			"Chalky",
			"Green",
			"Plush",
			"Chunky",
			"Gigantic",
			"Greasy",
			"Gloomy",
			"Practical",
			"Lanky",
			"Dopey",
			"Crusty",
			"Fantastic",
			"Rubbery",
			"Silly",
			"Courageous",
			"Reasonable",
			"Lonely",
			"Bitter"
		};

		// Token: 0x04000386 RID: 902
		public static string[] nouns = new string[]
		{
			"Dragon",
			"Buffet",
			"Biscuit",
			"Robot",
			"Planet",
			"Pepper",
			"Tomb",
			"Hyena",
			"Lip",
			"Quail",
			"Cheese",
			"Disaster",
			"Raincoat",
			"Shoe",
			"Castle",
			"Elf",
			"Pump",
			"Chip",
			"Wig",
			"Mermaid",
			"Drumstick",
			"Puppet",
			"Submarine"
		};

		// Token: 0x04000387 RID: 903
		public static string[] verbs = new string[]
		{
			"ran",
			"danced",
			"spoke",
			"galloped",
			"ate",
			"floated",
			"stood",
			"flowed",
			"smelled",
			"swam",
			"grilled",
			"cracked",
			"melted"
		};

		// Token: 0x04000388 RID: 904
		public static string[] positional = new string[]
		{
			"atop",
			"near",
			"with",
			"alongside",
			"away from",
			"too close to",
			"dangerously close to",
			"far, far away from",
			"uncomfortably close to",
			"way above the",
			"miles below",
			"on a different planet from",
			"in a different century than"
		};

		// Token: 0x04000389 RID: 905
		public static string[] places = new string[]
		{
			"Castle Village",
			"Basket Town",
			"Pine Mesa City",
			"Point Drake",
			"Minister Valley",
			"Grampleton",
			"Zuzu City",
			"a small island off the coast",
			"Fort Josa",
			"Chestervale",
			"Fern Islands",
			"Tanker Grove"
		};

		// Token: 0x0400038A RID: 906
		public static string[] colors = new string[]
		{
			"/crimson",
			"/green",
			"/tan",
			"/purple",
			"/deep blue",
			"/neon pink",
			"/pale/yellow",
			"/chocolate/brown",
			"/sky/blue",
			"/bubblegum/pink",
			"/blood/red",
			"/bright/orange",
			"/aquamarine",
			"/silvery",
			"/glimmering/gold",
			"/rainbow"
		};

		// Token: 0x0400038B RID: 907
		public List<DialogueLine> dialogues = new List<DialogueLine>();

		// Token: 0x0400038C RID: 908
		public HashSet<int> indexesWithoutPortrait = new HashSet<int>();

		// Token: 0x0400038D RID: 909
		private List<NPCDialogueResponse> playerResponses;

		// Token: 0x0400038E RID: 910
		private List<string> quickResponses;

		// Token: 0x0400038F RID: 911
		private bool isLastDialogueInteractive;

		// Token: 0x04000390 RID: 912
		private bool quickResponse;

		// Token: 0x04000391 RID: 913
		public bool isCurrentStringContinuedOnNextScreen;

		// Token: 0x04000392 RID: 914
		private bool finishedLastDialogue;

		// Token: 0x04000393 RID: 915
		public bool showPortrait;

		// Token: 0x04000394 RID: 916
		public bool removeOnNextMove;

		// Token: 0x04000395 RID: 917
		public bool dontFaceFarmer;

		// Token: 0x04000396 RID: 918
		public string temporaryDialogueKey;

		// Token: 0x04000397 RID: 919
		public int currentDialogueIndex;

		// Token: 0x04000398 RID: 920
		private string currentEmotion;

		// Token: 0x04000399 RID: 921
		public NPC speaker;

		// Token: 0x0400039A RID: 922
		public Dialogue.onAnswerQuestion answerQuestionBehavior;

		// Token: 0x0400039B RID: 923
		public Texture2D overridePortrait;

		// Token: 0x0400039C RID: 924
		public Action onFinish;

		// Token: 0x0400039D RID: 925
		public readonly string TranslationKey;

		// Token: 0x0200040C RID: 1036
		// (Invoke) Token: 0x06003BE8 RID: 15336
		public delegate bool onAnswerQuestion(int whichResponse);
	}
}

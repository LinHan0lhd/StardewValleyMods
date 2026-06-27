using System;
using System.Linq;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000399 RID: 921
	public class Lexicon
	{
		// Token: 0x0600385D RID: 14429 RVA: 0x002C9C08 File Offset: 0x002C7E08
		public static string getRandomNegativeItemSlanderNoun()
		{
			Random random = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			string[] choices = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeItemNoun").Split('#', StringSplitOptions.None);
			return random.Choose(choices);
		}

		// Token: 0x0600385E RID: 14430 RVA: 0x002C9C54 File Offset: 0x002C7E54
		public static string getProperArticleForWord(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				return "";
			}
			if (word != null && word.Length > 0)
			{
				char c = word.ToLower()[0];
				if (c <= 'e')
				{
					if (c != 'a' && c != 'e')
					{
						goto IL_4C;
					}
				}
				else if (c != 'i' && c != 'o' && c != 'u')
				{
					goto IL_4C;
				}
				return "an";
			}
			IL_4C:
			return "a";
		}

		// Token: 0x0600385F RID: 14431 RVA: 0x002C9CB4 File Offset: 0x002C7EB4
		public static string capitalize(string text)
		{
			if (string.IsNullOrEmpty(text) || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				return text;
			}
			int positionOfFirstCapitalizableCharacter = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					positionOfFirstCapitalizableCharacter = i;
					break;
				}
			}
			if (positionOfFirstCapitalizableCharacter == 0)
			{
				return text[0].ToString().ToUpper() + text.Substring(1);
			}
			return text.Substring(0, positionOfFirstCapitalizableCharacter) + text[positionOfFirstCapitalizableCharacter].ToString().ToUpper() + text.Substring(positionOfFirstCapitalizableCharacter + 1);
		}

		// Token: 0x06003860 RID: 14432 RVA: 0x002C9D48 File Offset: 0x002C7F48
		public static string makePlural(string word, bool ignore = false)
		{
			if (ignore || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en || word == null)
			{
				return word;
			}
			char c;
			if (word != null)
			{
				switch (word.Length)
				{
				case 3:
					if (!(word == "Hay"))
					{
						goto IL_5C7;
					}
					break;
				case 4:
					c = word[3];
					if (c <= 'l')
					{
						if (c != 'b')
						{
							if (c != 'l')
							{
								goto IL_5C7;
							}
							if (!(word == "Coal"))
							{
								goto IL_5C7;
							}
							return "lumps of Coal";
						}
						else if (!(word == "Chub"))
						{
							goto IL_5C7;
						}
					}
					else
					{
						switch (c)
						{
						case 'p':
							if (!(word == "Carp"))
							{
								goto IL_5C7;
							}
							break;
						case 'q':
						case 'r':
							goto IL_5C7;
						case 's':
							if (!(word == "Hops"))
							{
								goto IL_5C7;
							}
							break;
						case 't':
							if (!(word == "Salt"))
							{
								goto IL_5C7;
							}
							return "pieces of Salt";
						default:
							if (c != 'y')
							{
								goto IL_5C7;
							}
							if (!(word == "Clay"))
							{
								goto IL_5C7;
							}
							break;
						}
					}
					break;
				case 5:
					c = word[4];
					if (c <= 's')
					{
						if (c != 'm')
						{
							if (c != 's')
							{
								goto IL_5C7;
							}
							if (!(word == "Weeds"))
							{
								goto IL_5C7;
							}
						}
						else if (!(word == "Bream"))
						{
							goto IL_5C7;
						}
					}
					else if (c != 't')
					{
						if (c != 'y')
						{
							goto IL_5C7;
						}
						if (!(word == "Jelly"))
						{
							goto IL_5C7;
						}
						return "Jellies";
					}
					else
					{
						if (!(word == "Wheat"))
						{
							goto IL_5C7;
						}
						return "bushels of Wheat";
					}
					break;
				case 6:
					c = word[1];
					if (c != 'a')
					{
						if (c != 'i')
						{
							goto IL_5C7;
						}
						if (!(word == "Ginger"))
						{
							goto IL_5C7;
						}
						return "pieces of Ginger";
					}
					else
					{
						if (!(word == "Garlic"))
						{
							goto IL_5C7;
						}
						return "bulbs of Garlic";
					}
					break;
				case 7:
					if (!(word == "Pickles"))
					{
						goto IL_5C7;
					}
					break;
				case 8:
					c = word[0];
					if (c != 'B')
					{
						if (c != 'P')
						{
							if (c != 'S')
							{
								goto IL_5C7;
							}
							if (!(word == "Sandfish"))
							{
								goto IL_5C7;
							}
						}
						else if (!(word == "Pancakes"))
						{
							goto IL_5C7;
						}
					}
					else if (!(word == "Bok Choy"))
					{
						goto IL_5C7;
					}
					break;
				case 9:
					c = word[0];
					if (c != 'D')
					{
						if (c != 'G')
						{
							if (c != 'R')
							{
								goto IL_5C7;
							}
							if (!(word == "Red Canes"))
							{
								goto IL_5C7;
							}
						}
						else if (!(word == "Ghostfish"))
						{
							goto IL_5C7;
						}
					}
					else if (!(word == "Driftwood"))
					{
						goto IL_5C7;
					}
					break;
				case 10:
					c = word[0];
					if (c <= 'C')
					{
						if (c != 'A')
						{
							if (c != 'C')
							{
								goto IL_5C7;
							}
							if (!(word == "Crab Cakes"))
							{
								goto IL_5C7;
							}
						}
						else
						{
							if (!(word == "Algae Soup"))
							{
								goto IL_5C7;
							}
							return "bowls of Algae Soup";
						}
					}
					else if (c != 'H')
					{
						if (c != 'T')
						{
							goto IL_5C7;
						}
						if (!(word == "Tea Leaves"))
						{
							goto IL_5C7;
						}
					}
					else if (!(word == "Hashbrowns"))
					{
						goto IL_5C7;
					}
					break;
				case 11:
					c = word[4];
					if (c != ' ')
					{
						switch (c)
						{
						case 'b':
							if (!(word == "Cranberries"))
							{
								goto IL_5C7;
							}
							break;
						case 'c':
							goto IL_5C7;
						case 'd':
							if (!(word == "Mixed Seeds"))
							{
								goto IL_5C7;
							}
							break;
						case 'e':
							if (!(word == "Glazed Yams"))
							{
								goto IL_5C7;
							}
							break;
						default:
							if (c != 'n')
							{
								goto IL_5C7;
							}
							if (!(word == "Green Canes"))
							{
								goto IL_5C7;
							}
							break;
						}
					}
					else if (!(word == "Star Shards"))
					{
						goto IL_5C7;
					}
					break;
				case 12:
					c = word[0];
					if (c != 'D')
					{
						if (c != 'G')
						{
							if (c != 'R')
							{
								goto IL_5C7;
							}
							if (!(word == "Rice Pudding"))
							{
								goto IL_5C7;
							}
							return "bowls of Rice Pudding";
						}
						else if (!(word == "Glass Shards"))
						{
							goto IL_5C7;
						}
					}
					else
					{
						if (!(word == "Dragon Tooth"))
						{
							goto IL_5C7;
						}
						return "Dragon Teeth";
					}
					break;
				case 13:
				case 18:
				case 19:
				case 20:
					goto IL_5C7;
				case 14:
					c = word[0];
					if (c != 'B')
					{
						if (c != 'P')
						{
							goto IL_5C7;
						}
						if (!(word == "Pepper Poppers"))
						{
							goto IL_5C7;
						}
					}
					else if (!(word == "Broken Glasses"))
					{
						goto IL_5C7;
					}
					break;
				case 15:
					c = word[0];
					if (c != 'F')
					{
						if (c != 'L')
						{
							if (c != 'S')
							{
								goto IL_5C7;
							}
							if (!(word == "Smallmouth Bass"))
							{
								goto IL_5C7;
							}
						}
						else if (!(word == "Largemouth Bass"))
						{
							goto IL_5C7;
						}
					}
					else if (!(word == "Fossilized Ribs"))
					{
						goto IL_5C7;
					}
					break;
				case 16:
					if (!(word == "Dried Sunflowers"))
					{
						goto IL_5C7;
					}
					break;
				case 17:
					c = word[0];
					if (c != 'D')
					{
						if (c != 'R')
						{
							goto IL_5C7;
						}
						if (!(word == "Roasted Hazelnuts"))
						{
							goto IL_5C7;
						}
					}
					else if (!(word == "Dried Cranberries"))
					{
						goto IL_5C7;
					}
					break;
				case 21:
					if (!(word == "Warp Totem: Mountains"))
					{
						goto IL_5C7;
					}
					break;
				default:
					goto IL_5C7;
				}
				return word;
			}
			IL_5C7:
			c = word.Last<char>();
			if (c != 's')
			{
				switch (c)
				{
				case 'x':
				case 'z':
					return word + "es";
				case 'y':
					return word.Substring(0, word.Length - 1) + "ies";
				default:
					if (word.Length > 2)
					{
						string ending = word.Substring(word.Length - 2);
						if (ending == "sh" || ending == "ch")
						{
							return word + "es";
						}
					}
					return word + "s";
				}
			}
			else
			{
				if (!word.EndsWith(" Seeds") && !word.EndsWith(" Shorts") && !word.EndsWith(" Bass") && !word.EndsWith(" Flowers") && !word.EndsWith(" Peach"))
				{
					return word + "es";
				}
				return word;
			}
		}

		// Token: 0x06003861 RID: 14433 RVA: 0x002CA3FD File Offset: 0x002C85FD
		public static string prependArticle(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				return word;
			}
			return Lexicon.getProperArticleForWord(word) + " " + word;
		}

		// Token: 0x06003862 RID: 14434 RVA: 0x002CA419 File Offset: 0x002C8619
		public static string prependTokenizedArticle(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				return word;
			}
			return TokenStringBuilder.ArticleFor(word) + " " + word;
		}

		// Token: 0x06003863 RID: 14435 RVA: 0x002CA438 File Offset: 0x002C8638
		public static string getRandomPositiveAdjectiveForEventOrPerson(NPC n = null)
		{
			Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			string[] choices;
			if (n != null && n.Age != 0)
			{
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_Child").Split('#', StringSplitOptions.None);
			}
			else
			{
				Gender? gender = (n != null) ? new Gender?(n.Gender) : null;
				if (gender != null)
				{
					Gender valueOrDefault = gender.GetValueOrDefault();
					if (valueOrDefault == Gender.Male)
					{
						choices = Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultMale").Split('#', StringSplitOptions.None);
						goto IL_CA;
					}
					if (valueOrDefault == Gender.Female)
					{
						choices = Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultFemale").Split('#', StringSplitOptions.None);
						goto IL_CA;
					}
				}
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_PlaceOrEvent").Split('#', StringSplitOptions.None);
			}
			IL_CA:
			return r.Choose(choices);
		}

		// Token: 0x06003864 RID: 14436 RVA: 0x002CA518 File Offset: 0x002C8718
		public static string getRandomDeliciousAdjective(NPC n = null)
		{
			Random random = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			string[] choices;
			if (n != null && n.Age == 2)
			{
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective_Child").Split('#', StringSplitOptions.None);
			}
			else
			{
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective").Split('#', StringSplitOptions.None);
			}
			return random.Choose(choices);
		}

		// Token: 0x06003865 RID: 14437 RVA: 0x002CA58C File Offset: 0x002C878C
		public static string getRandomNegativeFoodAdjective(NPC n = null)
		{
			Random random = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			string[] choices;
			if (n != null && n.Age == 2)
			{
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Child").Split('#', StringSplitOptions.None);
			}
			else if (n != null && n.Manners == 1)
			{
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Polite").Split('#', StringSplitOptions.None);
			}
			else
			{
				choices = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective").Split('#', StringSplitOptions.None);
			}
			return random.Choose(choices);
		}

		// Token: 0x06003866 RID: 14438 RVA: 0x002CA624 File Offset: 0x002C8824
		public static string getRandomSlightlyPositiveAdjectiveForEdibleNoun(NPC n = null)
		{
			Random random = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			string[] choices = Game1.content.LoadString("Strings\\Lexicon:RandomSlightlyPositiveFoodAdjective").Split('#', StringSplitOptions.None);
			return random.Choose(choices);
		}

		// Token: 0x06003867 RID: 14439 RVA: 0x002CA66F File Offset: 0x002C886F
		public static string getGenderedChildTerm(bool isMale)
		{
			return Game1.content.LoadString(isMale ? "Strings\\Lexicon:ChildTerm_Male" : "Strings\\Lexicon:ChildTerm_Female");
		}

		// Token: 0x06003868 RID: 14440 RVA: 0x002CA68A File Offset: 0x002C888A
		public static string getTokenizedGenderedChildTerm(bool isMale)
		{
			return TokenStringBuilder.LocalizedText(isMale ? "Strings\\Lexicon:ChildTerm_Male" : "Strings\\Lexicon:ChildTerm_Female");
		}

		// Token: 0x06003869 RID: 14441 RVA: 0x002CA6A0 File Offset: 0x002C88A0
		public static string getPronoun(bool isMale)
		{
			return Game1.content.LoadString(isMale ? "Strings\\Lexicon:Pronoun_Male" : "Strings\\Lexicon:Pronoun_Female");
		}

		// Token: 0x0600386A RID: 14442 RVA: 0x002CA6BB File Offset: 0x002C88BB
		public static string getTokenizedPronoun(bool isMale)
		{
			return TokenStringBuilder.LocalizedText(isMale ? "Strings\\Lexicon:Pronoun_Male" : "Strings\\Lexicon:Pronoun_Female");
		}

		// Token: 0x0600386B RID: 14443 RVA: 0x002CA6D1 File Offset: 0x002C88D1
		public static string getPossessivePronoun(bool isMale)
		{
			return Game1.content.LoadString(isMale ? "Strings\\Lexicon:Possessive_Pronoun_Male" : "Strings\\Lexicon:Possessive_Pronoun_Female");
		}

		// Token: 0x0600386C RID: 14444 RVA: 0x002CA6EC File Offset: 0x002C88EC
		public static string getTokenizedPossessivePronoun(bool isMale)
		{
			return TokenStringBuilder.LocalizedText(isMale ? "Strings\\Lexicon:Possessive_Pronoun_Male" : "Strings\\Lexicon:Possessive_Pronoun_Female");
		}
	}
}

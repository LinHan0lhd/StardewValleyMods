using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace StardewValley.Tests
{
	// Token: 0x0200013A RID: 314
	public class SyntaxAbstractor
	{
		// Token: 0x060018FE RID: 6398 RVA: 0x00124CD0 File Offset: 0x00122ED0
		public ExtractSyntaxDelegate GetSyntaxHandler(string baseAssetName)
		{
			ExtractSyntaxDelegate handler;
			if (this.SyntaxHandlers.TryGetValue(baseAssetName, out handler))
			{
				return handler;
			}
			int splitIndex = baseAssetName.LastIndexOf('/');
			if (splitIndex != -1 && this.SyntaxHandlers.TryGetValue(baseAssetName.Substring(0, splitIndex) + "/*", out handler))
			{
				return handler;
			}
			return null;
		}

		// Token: 0x060018FF RID: 6399 RVA: 0x00124D20 File Offset: 0x00122F20
		public string ExtractSyntaxFor(string baseAssetName, string key, string value)
		{
			if (value.Contains("${"))
			{
				value = Regex.Replace(value, "\\$\\{.+?\\}\\$", "text");
			}
			ExtractSyntaxDelegate syntaxHandler = this.GetSyntaxHandler(baseAssetName);
			return ((syntaxHandler != null) ? syntaxHandler(this, baseAssetName, key, value) : null) ?? value;
		}

		// Token: 0x06001900 RID: 6400 RVA: 0x00124D5D File Offset: 0x00122F5D
		public string ExtractPlainTextSyntax(string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				return "text";
			}
			return string.Empty;
		}

		// Token: 0x06001901 RID: 6401 RVA: 0x00124D74 File Offset: 0x00122F74
		public string ExtractDialogueSyntax(string value)
		{
			StringBuilder syntax = new StringBuilder();
			int index = 0;
			this.ExtractDialogueSyntaxImpl(value, '#', ref index, syntax, -1);
			return syntax.ToString();
		}

		// Token: 0x06001902 RID: 6402 RVA: 0x00124D9C File Offset: 0x00122F9C
		public string ExtractDialogueSyntax(string baseAssetName, string key, string value)
		{
			if (!(baseAssetName == "Data/ExtraDialogue"))
			{
				if (!(baseAssetName == "Strings/Locations"))
				{
					if (baseAssetName == "Strings/StringsFromCSFiles")
					{
						uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
						if (num <= 2125317731U)
						{
							if (num <= 1185502384U)
							{
								if (num <= 308266181U)
								{
									if (num <= 207453372U)
									{
										if (num != 139357158U)
										{
											if (num != 190822848U)
											{
												if (num != 207453372U)
												{
													goto IL_9C4;
												}
												if (!(key == "NPC.cs.4446"))
												{
													goto IL_9C4;
												}
											}
											else if (!(key == "NPC.cs.4455"))
											{
												goto IL_9C4;
											}
										}
										else if (!(key == "NPC.cs.4422"))
										{
											goto IL_9C4;
										}
									}
									else if (num <= 275402491U)
									{
										if (num != 224230991U)
										{
											if (num != 275402491U)
											{
												goto IL_9C4;
											}
											if (!(key == "NPC.cs.4498"))
											{
												goto IL_9C4;
											}
										}
										else if (!(key == "NPC.cs.4447"))
										{
											goto IL_9C4;
										}
									}
									else if (num != 283846781U)
									{
										if (num != 308266181U)
										{
											goto IL_9C4;
										}
										if (!(key == "NPC.cs.4452"))
										{
											goto IL_9C4;
										}
									}
									else if (!(key == "NPC.cs.4500"))
									{
										goto IL_9C4;
									}
								}
								else if (num <= 510181327U)
								{
									if (num <= 426293232U)
									{
										if (num != 324896705U)
										{
											if (num != 426293232U)
											{
												goto IL_9C4;
											}
											if (!(key == "Event.cs.1501"))
											{
												goto IL_9C4;
											}
										}
										else if (!(key == "NPC.cs.4449"))
										{
											goto IL_9C4;
										}
									}
									else if (num != 443070851U)
									{
										if (num != 510181327U)
										{
											goto IL_9C4;
										}
										if (!(key == "Event.cs.1504"))
										{
											goto IL_9C4;
										}
									}
									else if (!(key == "Event.cs.1500"))
									{
										goto IL_9C4;
									}
								}
								else if (num <= 609969133U)
								{
									if (num != 542858657U)
									{
										if (num != 609969133U)
										{
											goto IL_9C4;
										}
										if (!(key == "NPC.cs.4470"))
										{
											goto IL_9C4;
										}
									}
									else if (!(key == "NPC.cs.4474"))
									{
										goto IL_9C4;
									}
								}
								else if (num != 1135022432U)
								{
									if (num != 1185502384U)
									{
										goto IL_9C4;
									}
									if (!(key == "NPC.cs.4147"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "NPC.cs.4154"))
								{
									goto IL_9C4;
								}
							}
							else if (num <= 1610318974U)
							{
								if (num <= 1252465765U)
								{
									if (num <= 1235688146U)
									{
										if (num != 1202280003U)
										{
											if (num != 1235688146U)
											{
												goto IL_9C4;
											}
											if (!(key == "NPC.cs.4152"))
											{
												goto IL_9C4;
											}
										}
										else if (!(key == "NPC.cs.4146"))
										{
											goto IL_9C4;
										}
									}
									else if (num != 1235835241U)
									{
										if (num != 1252465765U)
										{
											goto IL_9C4;
										}
										if (!(key == "NPC.cs.4153"))
										{
											goto IL_9C4;
										}
									}
									else if (!(key == "NPC.cs.4144"))
									{
										goto IL_9C4;
									}
								}
								else if (num <= 1420389050U)
								{
									if (num != 1286168098U)
									{
										if (num != 1420389050U)
										{
											goto IL_9C4;
										}
										if (!(key == "NPC.cs.4149"))
										{
											goto IL_9C4;
										}
									}
									else if (!(key == "NPC.cs.4141"))
									{
										goto IL_9C4;
									}
								}
								else if (num != 1595659926U)
								{
									if (num != 1610318974U)
									{
										goto IL_9C4;
									}
									if (!(key == "NPC.cs.4079"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "NPC.cs.4088"))
								{
									goto IL_9C4;
								}
							}
							else if (num <= 1802400802U)
							{
								if (num <= 1635195417U)
								{
									if (num != 1612437545U)
									{
										if (num != 1635195417U)
										{
											goto IL_9C4;
										}
										if (!(key == "Pipe"))
										{
											goto IL_9C4;
										}
										return "text";
									}
									else if (!(key == "NPC.cs.4089"))
									{
										goto IL_9C4;
									}
								}
								else if (num != 1729880878U)
								{
									if (num != 1802400802U)
									{
										goto IL_9C4;
									}
									if (!(key == "NPC.cs.3957"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "NPC.cs.4080"))
								{
									goto IL_9C4;
								}
							}
							else if (num <= 1835956040U)
							{
								if (num != 1813916068U)
								{
									if (num != 1835956040U)
									{
										goto IL_9C4;
									}
									if (!(key == "NPC.cs.3959"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "NPC.cs.4091"))
								{
									goto IL_9C4;
								}
							}
							else if (num != 2030843167U)
							{
								if (num != 2125317731U)
								{
									goto IL_9C4;
								}
								if (!(key == "NPC.cs.4274"))
								{
									goto IL_9C4;
								}
							}
							else
							{
								if (!(key == "OptionsPage.cs.11300"))
								{
									goto IL_9C4;
								}
								goto IL_9B6;
							}
						}
						else if (num <= 2542505066U)
						{
							if (num <= 2341173638U)
							{
								if (num <= 2307618400U)
								{
									if (num != 2142095350U)
									{
										if (num != 2158872969U)
										{
											if (num != 2307618400U)
											{
												goto IL_9C4;
											}
											if (!(key == "OptionsPage.cs.11297"))
											{
												goto IL_9C4;
											}
											goto IL_9B6;
										}
										else if (!(key == "NPC.cs.4276"))
										{
											goto IL_9C4;
										}
									}
									else if (!(key == "NPC.cs.4277"))
									{
										goto IL_9C4;
									}
								}
								else if (num <= 2324396019U)
								{
									if (num != 2309871540U)
									{
										if (num != 2324396019U)
										{
											goto IL_9C4;
										}
										if (!(key == "OptionsPage.cs.11296"))
										{
											goto IL_9C4;
										}
										goto IL_9B6;
									}
									else if (!(key == "NPC.cs.4279"))
									{
										goto IL_9C4;
									}
								}
								else if (num != 2326649159U)
								{
									if (num != 2341173638U)
									{
										goto IL_9C4;
									}
									if (!(key == "OptionsPage.cs.11295"))
									{
										goto IL_9C4;
									}
									goto IL_9B6;
								}
								else if (!(key == "NPC.cs.4278"))
								{
									goto IL_9C4;
								}
							}
							else if (num <= 2391506495U)
							{
								if (num <= 2357951257U)
								{
									if (num != 2343720968U)
									{
										if (num != 2357951257U)
										{
											goto IL_9C4;
										}
										if (!(key == "OptionsPage.cs.11294"))
										{
											goto IL_9C4;
										}
										goto IL_9B6;
									}
									else if (!(key == "NPC.cs.4293"))
									{
										goto IL_9C4;
									}
								}
								else if (num != 2374728876U)
								{
									if (num != 2391506495U)
									{
										goto IL_9C4;
									}
									if (!(key == "OptionsPage.cs.11292"))
									{
										goto IL_9C4;
									}
									goto IL_9B6;
								}
								else
								{
									if (!(key == "OptionsPage.cs.11293"))
									{
										goto IL_9C4;
									}
									goto IL_9B6;
								}
							}
							else if (num <= 2425061733U)
							{
								if (num != 2408284114U)
								{
									if (num != 2425061733U)
									{
										goto IL_9C4;
									}
									if (!(key == "OptionsPage.cs.11290"))
									{
										goto IL_9C4;
									}
									goto IL_9B6;
								}
								else
								{
									if (!(key == "OptionsPage.cs.11291"))
									{
										goto IL_9C4;
									}
									goto IL_9B6;
								}
							}
							else if (num != 2475247495U)
							{
								if (num != 2542505066U)
								{
									goto IL_9C4;
								}
								if (!(key == "OptionsPage.cs.11299"))
								{
									goto IL_9C4;
								}
								goto IL_9B6;
							}
							else
							{
								if (!(key == "OptionsPage.cs.11289"))
								{
									goto IL_9C4;
								}
								goto IL_9B6;
							}
						}
						else if (num <= 3533530401U)
						{
							if (num <= 2741462221U)
							{
								if (num <= 2590463650U)
								{
									if (num != 2559282685U)
									{
										if (num != 2590463650U)
										{
											goto IL_9C4;
										}
										if (!(key == "NPC.cs.4488"))
										{
											goto IL_9C4;
										}
									}
									else
									{
										if (!(key == "OptionsPage.cs.11298"))
										{
											goto IL_9C4;
										}
										goto IL_9B6;
									}
								}
								else if (num != 2690143626U)
								{
									if (num != 2741462221U)
									{
										goto IL_9C4;
									}
									if (!(key == "NPC.cs.4481"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "NPC.cs.4462"))
								{
									goto IL_9C4;
								}
							}
							else if (num <= 3078816606U)
							{
								if (num != 2978150892U)
								{
									if (num != 3078816606U)
									{
										goto IL_9C4;
									}
									if (!(key == "Event.cs.1499"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "Event.cs.1497"))
								{
									goto IL_9C4;
								}
							}
							else if (num != 3095594225U)
							{
								if (num != 3533530401U)
								{
									goto IL_9C4;
								}
								if (!(key == "NPC.cs.4113"))
								{
									goto IL_9C4;
								}
							}
							else if (!(key == "Event.cs.1498"))
							{
								goto IL_9C4;
							}
						}
						else if (num <= 4150723009U)
						{
							if (num <= 3966169200U)
							{
								if (num != 3567085639U)
								{
									if (num != 3966169200U)
									{
										goto IL_9C4;
									}
									if (!(key == "NPC.cs.3968"))
									{
										goto IL_9C4;
									}
								}
								else if (!(key == "NPC.cs.4115"))
								{
									goto IL_9C4;
								}
							}
							else if (num != 4133945390U)
							{
								if (num != 4150723009U)
								{
									goto IL_9C4;
								}
								if (!(key == "NPC.cs.3963"))
								{
									goto IL_9C4;
								}
							}
							else if (!(key == "NPC.cs.3962"))
							{
								goto IL_9C4;
							}
						}
						else if (num <= 4201055866U)
						{
							if (num != 4184278247U)
							{
								if (num != 4201055866U)
								{
									goto IL_9C4;
								}
								if (!(key == "NPC.cs.3966"))
								{
									goto IL_9C4;
								}
							}
							else if (!(key == "NPC.cs.3965"))
							{
								goto IL_9C4;
							}
						}
						else if (num != 4251535818U)
						{
							if (num != 4268313437U)
							{
								goto IL_9C4;
							}
							if (!(key == "NPC.cs.3974"))
							{
								goto IL_9C4;
							}
						}
						else if (!(key == "NPC.cs.3975"))
						{
							goto IL_9C4;
						}
						return this.ExtractNpcGenderedDialogueSyntax(value);
						IL_9B6:
						if (!string.IsNullOrWhiteSpace(value))
						{
							return "text";
						}
					}
				}
				else if (key == "FarmHouse_SpouseAttacked3")
				{
					return "text";
				}
			}
			else if (key == "NewChild_Adoption" || key == "NewChild_FirstChild" || key == "NewChild_SecondChild1" || key == "NewChild_SecondChild2")
			{
				return this.ExtractNpcGenderedDialogueSyntax(value);
			}
			IL_9C4:
			return this.ExtractDialogueSyntax(value);
		}

		// Token: 0x06001903 RID: 6403 RVA: 0x00125774 File Offset: 0x00123974
		public string ExtractEventSyntax(string value)
		{
			StringBuilder syntax = new StringBuilder();
			int index = 0;
			this.ExtractEventSyntaxImpl(value, ref index, syntax, -1);
			return syntax.ToString();
		}

		// Token: 0x06001904 RID: 6404 RVA: 0x0012579C File Offset: 0x0012399C
		public string ExtractFestivalSyntax(string baseAssetName, string key, string value)
		{
			if (key != null)
			{
				switch (key.Length)
				{
				case 6:
					if (!(key == "set-up"))
					{
						goto IL_1F1;
					}
					break;
				case 7:
					if (!(key == "AbbyWin"))
					{
						goto IL_1F1;
					}
					goto IL_1C7;
				case 8:
				case 11:
				case 13:
				case 14:
				case 16:
					goto IL_1F1;
				case 9:
				{
					char c = key[0];
					if (c != 'm')
					{
						if (c != 's')
						{
							goto IL_1F1;
						}
						if (!(key == "set-up_y2"))
						{
							goto IL_1F1;
						}
					}
					else if (!(key == "mainEvent"))
					{
						goto IL_1F1;
					}
					break;
				}
				case 10:
					if (!(key == "conditions"))
					{
						goto IL_1F1;
					}
					break;
				case 12:
				{
					char c = key[0];
					if (c != 'a')
					{
						if (c != 'm')
						{
							goto IL_1F1;
						}
						if (!(key == "mainEvent_y2"))
						{
							goto IL_1F1;
						}
					}
					else
					{
						if (!(key == "afterEggHunt"))
						{
							goto IL_1F1;
						}
						goto IL_1C7;
					}
					break;
				}
				case 15:
					if (!(key == "afterEggHunt_y2"))
					{
						goto IL_1F1;
					}
					goto IL_1C7;
				case 17:
					switch (key[16])
					{
					case '0':
						if (!(key == "governorReaction0"))
						{
							goto IL_1F1;
						}
						break;
					case '1':
						if (!(key == "governorReaction1"))
						{
							goto IL_1F1;
						}
						break;
					case '2':
						if (!(key == "governorReaction2"))
						{
							goto IL_1F1;
						}
						break;
					case '3':
						if (!(key == "governorReaction3"))
						{
							goto IL_1F1;
						}
						break;
					case '4':
						if (!(key == "governorReaction4"))
						{
							goto IL_1F1;
						}
						break;
					case '5':
						if (!(key == "governorReaction5"))
						{
							goto IL_1F1;
						}
						break;
					case '6':
						if (!(key == "governorReaction6"))
						{
							goto IL_1F1;
						}
						break;
					default:
						goto IL_1F1;
					}
					if (baseAssetName == "Data/Festivals/summer11")
					{
						return this.ExtractEventSyntax(value);
					}
					goto IL_1F1;
				default:
					goto IL_1F1;
				}
				return this.ExtractEventSyntax(value);
				IL_1C7:
				if (baseAssetName == "Data/Festivals/spring13")
				{
					return this.ExtractEventSyntax(value);
				}
			}
			IL_1F1:
			return this.ExtractDialogueSyntax(value);
		}

		// Token: 0x06001905 RID: 6405 RVA: 0x001259A4 File Offset: 0x00123BA4
		public string ExtractCreditsSyntax(string text)
		{
			if (text.Length == 0)
			{
				return text;
			}
			if (text.StartsWith('['))
			{
				if (text.StartsWith("[image]"))
				{
					return text;
				}
				if (text.StartsWith("[link]"))
				{
					string[] parts = text.Split(' ', 3, StringSplitOptions.None);
					parts[2] = "text";
					return string.Join(" ", parts);
				}
			}
			StringBuilder syntax = new StringBuilder();
			int index = 0;
			bool hasText = false;
			while (index < text.Length)
			{
				if (text[index] == '[')
				{
					this.EndTextContext(ref hasText, syntax);
					this.ExtractTagSyntax(text, ref index, syntax);
				}
				else
				{
					hasText = true;
				}
				index++;
			}
			this.EndTextContext(ref hasText, syntax);
			return syntax.ToString();
		}

		// Token: 0x06001906 RID: 6406 RVA: 0x00125A4C File Offset: 0x00123C4C
		public string ExtractMailSyntax(string text)
		{
			text = text.Replace("%secretsanta", "text");
			StringBuilder syntax = new StringBuilder();
			int index = 0;
			bool hasText = false;
			while (index < text.Length)
			{
				char ch = text[index];
				if (ch != '%')
				{
					if (ch != '[')
					{
						if (ch == '¦')
						{
							this.EndTextContext(ref hasText, syntax);
							syntax.Append(ch);
						}
						else if (ch != ' ' && !hasText)
						{
							hasText = true;
						}
					}
					else
					{
						this.EndTextContext(ref hasText, syntax);
						this.ExtractTagSyntax(text, ref index, syntax);
					}
				}
				else if (index >= text.Length || char.IsWhiteSpace(text[index + 1]) || char.IsDigit(text[index + 1]))
				{
					hasText = true;
				}
				else
				{
					this.EndTextContext(ref hasText, syntax);
					this.ExtractMailCommandSyntax(text, ref index, syntax);
				}
				index++;
			}
			this.EndTextContext(ref hasText, syntax);
			return syntax.ToString();
		}

		// Token: 0x06001907 RID: 6407 RVA: 0x00125B27 File Offset: 0x00123D27
		public string ExtractDelimitedDataSyntax(string text, char delimiter, params int[] textFields)
		{
			return this.ExtractDelimitedDataSyntax(text, delimiter, textFields, null);
		}

		// Token: 0x06001908 RID: 6408 RVA: 0x00125B34 File Offset: 0x00123D34
		public string ExtractDelimitedDataSyntax(string text, char delimiter, int[] textFields, int[] dialogueFields)
		{
			string[] parts = text.Split(delimiter, StringSplitOptions.None);
			foreach (int index in textFields)
			{
				if (ArgUtility.HasIndex<string>(parts, index))
				{
					parts[index] = "text";
				}
			}
			if (dialogueFields != null)
			{
				foreach (int index2 in dialogueFields)
				{
					if (ArgUtility.HasIndex<string>(parts, index2))
					{
						parts[index2] = this.ExtractDialogueSyntax(parts[index2]);
					}
				}
			}
			return string.Join(delimiter.ToString(), parts);
		}

		// Token: 0x06001909 RID: 6409 RVA: 0x00125BB0 File Offset: 0x00123DB0
		public string Extract16StringsSyntax(string key, string text)
		{
			if (key.StartsWith("Renovation_"))
			{
				return this.ExtractDelimitedDataSyntax(text, '/', LegacyShims.EmptyArray<int>(), new int[]
				{
					0,
					1,
					2
				});
			}
			if (key == "ForestPylonEvent")
			{
				return this.ExtractEventSyntax(text);
			}
			if (!(key == "StarterChicken_Names"))
			{
				return this.ExtractDialogueSyntax(text);
			}
			string[] array = text.Split('|', StringSplitOptions.None);
			StringBuilder syntax = new StringBuilder();
			bool omittedPairs = false;
			foreach (string entry in array)
			{
				if (entry.Split(',', 3, StringSplitOptions.None).Length == 2)
				{
					if (syntax.Length == 0)
					{
						syntax.Append("name,name");
					}
					else
					{
						omittedPairs = true;
					}
				}
				else
				{
					if (syntax.Length > 0)
					{
						syntax.Append(" | ");
					}
					StringBuilder stringBuilder = syntax;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("<invalid pair: ");
					appendInterpolatedStringHandler.AppendFormatted(entry.Trim());
					appendInterpolatedStringHandler.AppendLiteral(">");
					stringBuilder2.Append(ref appendInterpolatedStringHandler);
				}
			}
			if (omittedPairs)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
				defaultInterpolatedStringHandler.AppendFormatted<StringBuilder>(syntax);
				defaultInterpolatedStringHandler.AppendLiteral(" | ...");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			if (syntax.Length > 0)
			{
				return syntax.ToString();
			}
			return string.Empty;
		}

		// Token: 0x0600190A RID: 6410 RVA: 0x00125CFC File Offset: 0x00123EFC
		public string ExtractLexiconSyntax(string key, string text)
		{
			string[] parts = text.Split('#', StringSplitOptions.None);
			for (int i = 0; i < parts.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(parts[i]))
				{
					string raw = parts[i];
					int prefixSpaces = raw.Length - raw.TrimStart().Length;
					int suffixSpaces = raw.Length - raw.TrimEnd().Length;
					parts[i] = ((prefixSpaces > 0 || suffixSpaces > 0) ? ("".PadRight(prefixSpaces) + "text" + "".PadRight(suffixSpaces)) : "text");
				}
			}
			if (key.StartsWith("Random") && parts.Length > 2)
			{
				return parts[0] + "#" + parts[1] + "#...";
			}
			return string.Join("#", parts);
		}

		// Token: 0x0600190B RID: 6411 RVA: 0x00125DBE File Offset: 0x00123FBE
		private static string DialogueSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
		{
			return syntaxAbstractor.ExtractDialogueSyntax(baseAssetName, key, text);
		}

		// Token: 0x0600190C RID: 6412 RVA: 0x00125DC9 File Offset: 0x00123FC9
		private static string PlainTextSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
		{
			return syntaxAbstractor.ExtractPlainTextSyntax(text);
		}

		// Token: 0x0600190D RID: 6413 RVA: 0x00125DD2 File Offset: 0x00123FD2
		private static string EventSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
		{
			return syntaxAbstractor.ExtractEventSyntax(text);
		}

		// Token: 0x0600190E RID: 6414 RVA: 0x00125DDB File Offset: 0x00123FDB
		private static string FestivalSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
		{
			return syntaxAbstractor.ExtractFestivalSyntax(baseAssetName, key, text);
		}

		// Token: 0x0600190F RID: 6415 RVA: 0x00125DE8 File Offset: 0x00123FE8
		private void ExtractEventSyntaxImpl(string text, ref int index, StringBuilder syntax, int maxIndex = -1)
		{
			string[] array = ArgUtility.SplitQuoteAware((index == 0 && maxIndex < 0) ? text : text.Substring(index, maxIndex - index + 1), '/', StringSplitOptions.TrimEntries, true);
			bool isFirstCommand = true;
			foreach (string rawCommand in array)
			{
				if (!isFirstCommand)
				{
					syntax.Append('/');
				}
				if (!string.IsNullOrWhiteSpace(rawCommand))
				{
					string[] args = ArgUtility.SplitBySpaceQuoteAware(rawCommand);
					string commandName = args[0];
					syntax.Append(commandName);
					int nextArg = 1;
					string actualName;
					if (Event.TryResolveCommandName(commandName, out actualName) && actualName != null)
					{
						switch (actualName.Length)
						{
						case 3:
							if (actualName == "End")
							{
								string action = ArgUtility.Get(args, 1, null, true);
								if (action == "dialogue" || action == "dialogueWarpOut")
								{
									this.AppendEventCommandArg(syntax, args, 1, true);
									this.AppendEventCommandArg(syntax, args, 2, true);
									this.AppendEventCommandDialogueArg(syntax, args, 3, true, true);
									nextArg = 4;
								}
							}
							break;
						case 5:
							if (actualName == "Speak")
							{
								this.AppendEventCommandArg(syntax, args, 1, true);
								this.AppendEventCommandDialogueArg(syntax, args, 2, true, true);
								nextArg = 3;
							}
							break;
						case 7:
							if (actualName == "Message")
							{
								this.AppendEventCommandDialogueArg(syntax, args, 1, true, true);
								nextArg = 2;
							}
							break;
						case 8:
							if (actualName == "Question")
							{
								this.AppendEventCommandArg(syntax, args, 1, true);
								this.AppendEventCommandDialogueArg(syntax, args, 2, true, true);
								nextArg = 3;
							}
							break;
						case 10:
						{
							char c = actualName[2];
							if (c != 'l')
							{
								if (c == 'r')
								{
									if (actualName == "SpriteText")
									{
										this.AppendEventCommandArg(syntax, args, 1, true);
										this.AppendEventCommandDialogueArg(syntax, args, 2, true, true);
										nextArg = 3;
									}
								}
							}
							else if (actualName == "SplitSpeak")
							{
								string text2 = ArgUtility.Get(args, 2, null, true);
								string[] dialogues = (text2 != null) ? text2.Split('~', StringSplitOptions.None) : null;
								this.AppendEventCommandArg(syntax, args, 1, true);
								if (dialogues != null)
								{
									syntax.Append(" \"");
									for (int i = 0; i < dialogues.Length; i++)
									{
										if (i > 0)
										{
											syntax.Append('~');
										}
										this.AppendEventCommandDialogueArg(syntax, dialogues, i, false, false);
									}
									syntax.Append('"');
								}
								nextArg = 3;
							}
							break;
						}
						case 13:
						{
							char c = actualName[0];
							if (c != 'Q')
							{
								if (c == 'T')
								{
									if (actualName == "TextAboveHead")
									{
										this.AppendEventCommandArg(syntax, args, 1, true);
										this.AppendEventCommandDialogueArg(syntax, args, 2, true, true);
										nextArg = 3;
									}
								}
							}
							else if (actualName == "QuickQuestion")
							{
								string[] masterSplit = LegacyShims.SplitAndTrim(rawCommand.Substring(rawCommand.IndexOf(' ')), "(break)", StringSplitOptions.None);
								string[] questionAndAnswerSplit = LegacyShims.SplitAndTrim(masterSplit[0], '#', StringSplitOptions.None);
								syntax.Append(" \"");
								this.AppendEventCommandDialogueArg(syntax, questionAndAnswerSplit, 0, true, false);
								for (int j = 1; j < questionAndAnswerSplit.Length; j++)
								{
									syntax.Append('#');
									this.AppendEventCommandDialogueArg(syntax, questionAndAnswerSplit, j, false, false);
								}
								for (int k = 1; k < masterSplit.Length; k++)
								{
									masterSplit[k] = masterSplit[k].Replace('\\', '/');
									syntax.Append("(break)");
									int tempIndex = 0;
									this.ExtractEventSyntaxImpl(masterSplit[k], ref tempIndex, syntax, -1);
								}
								syntax.Append('"');
								nextArg = args.Length;
							}
							break;
						}
						}
					}
					while (nextArg < args.Length)
					{
						this.AppendEventCommandArg(syntax, args, nextArg, true);
						nextArg++;
					}
				}
				isFirstCommand = false;
			}
			index = ((maxIndex > 0) ? maxIndex : (text.Length - 1));
		}

		// Token: 0x06001910 RID: 6416 RVA: 0x001261EC File Offset: 0x001243EC
		private void AppendEventCommandArg(StringBuilder syntax, string[] args, int index, bool prependSpace = true)
		{
			if (!ArgUtility.HasIndex<string>(args, index))
			{
				return;
			}
			string text = args[index];
			bool flag = text.Contains(' ');
			if (prependSpace)
			{
				syntax.Append(' ');
			}
			if (flag)
			{
				syntax.Append('"');
			}
			syntax.Append(text);
			if (flag)
			{
				syntax.Append('"');
			}
		}

		// Token: 0x06001911 RID: 6417 RVA: 0x0012623C File Offset: 0x0012443C
		private void AppendEventCommandDialogueArg(StringBuilder syntax, string[] args, int index, bool prependSpace = true, bool quote = true)
		{
			if (!ArgUtility.HasIndex<string>(args, index))
			{
				return;
			}
			string text = args[index];
			int tempIndex = 0;
			if (prependSpace)
			{
				syntax.Append(' ');
			}
			if (quote)
			{
				syntax.Append('"');
			}
			this.ExtractDialogueSyntaxImpl(text, '/', ref tempIndex, syntax, -1);
			if (quote)
			{
				syntax.Append('"');
			}
		}

		// Token: 0x06001912 RID: 6418 RVA: 0x00126290 File Offset: 0x00124490
		private string ExtractNpcGenderedDialogueSyntax(string text)
		{
			if (!text.Contains('/'))
			{
				return this.ExtractDialogueSyntax(text);
			}
			string[] parts = text.Split('/', StringSplitOptions.None);
			for (int i = 0; i < parts.Length; i++)
			{
				parts[i] = this.ExtractDialogueSyntax(parts[i]);
			}
			if (parts.Length != 2 || !(parts[0] == parts[1]))
			{
				return string.Join("/", parts);
			}
			return parts[0];
		}

		// Token: 0x06001913 RID: 6419 RVA: 0x001262F8 File Offset: 0x001244F8
		private void ExtractDialogueSyntaxImpl(string text, char commandDelimiter, ref int index, StringBuilder syntax, int maxIndex = -1)
		{
			bool hasText = false;
			bool hasSpaces = false;
			if (maxIndex < 0 || maxIndex > text.Length - 1)
			{
				maxIndex = text.Length - 1;
			}
			while (index <= maxIndex)
			{
				char ch = text[index];
				if (ch <= '$')
				{
					if (ch != '#' && ch != '$')
					{
						goto IL_C5;
					}
					goto IL_53;
				}
				else if (ch != '[')
				{
					if (ch != ']')
					{
						if (ch == '|')
						{
							goto IL_53;
						}
						goto IL_C5;
					}
					else
					{
						this.EndTextContext(ref hasText, syntax);
						syntax.Append(']');
						hasSpaces = false;
					}
				}
				else
				{
					this.EndTextContext(ref hasText, syntax);
					this.ExtractDialogueItemSpawnSyntax(text, ref index, syntax);
					hasSpaces = false;
				}
				IL_D0:
				index++;
				continue;
				IL_53:
				if (ch == '$' && hasSpaces && !hasText)
				{
					syntax.Append("text");
				}
				this.EndTextContext(ref hasText, syntax);
				hasSpaces = false;
				if (ch == '$')
				{
					this.ExtractDialogueCommandSyntax(text, ref index, syntax, commandDelimiter);
					goto IL_D0;
				}
				syntax.Append(ch);
				goto IL_D0;
				IL_C5:
				if (ch == ' ')
				{
					hasSpaces = true;
					goto IL_D0;
				}
				hasText = true;
				goto IL_D0;
			}
			this.EndTextContext(ref hasText, syntax);
		}

		// Token: 0x06001914 RID: 6420 RVA: 0x001263F0 File Offset: 0x001245F0
		private void ExtractDialogueCommandSyntax(string text, ref int index, StringBuilder syntax, char commandDelimiter)
		{
			int startIndex = index;
			index++;
			while (index < text.Length && (char.IsLetter(text[index]) || char.IsNumber(text[index])))
			{
				index++;
			}
			string commandName = text.Substring(startIndex, index - startIndex);
			syntax.Append(commandName);
			if (commandName != null)
			{
				int length = commandName.Length;
				if (length == 2)
				{
					char c = commandName[1];
					if (c <= 'c')
					{
						if (c != '1')
						{
							if (c != 'c')
							{
								goto IL_3A0;
							}
							if (!(commandName == "$c"))
							{
								goto IL_3A0;
							}
						}
						else if (!(commandName == "$1"))
						{
							goto IL_3A0;
						}
					}
					else if (c != 'd')
					{
						switch (c)
						{
						case 'p':
							if (!(commandName == "$p"))
							{
								goto IL_3A0;
							}
							goto IL_1A8;
						case 'q':
							if (!(commandName == "$q"))
							{
								goto IL_3A0;
							}
							break;
						case 'r':
							if (!(commandName == "$r"))
							{
								goto IL_3A0;
							}
							break;
						case 's':
							goto IL_3A0;
						case 't':
							if (!(commandName == "$t"))
							{
								goto IL_3A0;
							}
							break;
						default:
						{
							if (c != 'y')
							{
								goto IL_3A0;
							}
							if (!(commandName == "$y"))
							{
								goto IL_3A0;
							}
							int startIndex2 = index;
							while (index < text.Length && text[index] == ' ')
							{
								index++;
							}
							if (text[index] != '\'')
							{
								index = startIndex2;
								return;
							}
							index++;
							syntax.Append(text.Substring(startIndex2, index - startIndex2).TrimEnd(' '));
							int endIndex = index;
							int maxIndex = text.IndexOf(commandDelimiter, index);
							if (maxIndex == -1)
							{
								maxIndex = text.Length;
							}
							for (;;)
							{
								int nextIndex = text.IndexOf('\'', endIndex + 1);
								if (nextIndex == -1 || nextIndex > maxIndex)
								{
									break;
								}
								endIndex = nextIndex;
							}
							if (endIndex <= index)
							{
								return;
							}
							bool hasText = false;
							while (index < endIndex - 1)
							{
								char ch = text[index];
								if (ch == '_')
								{
									if (hasText)
									{
										syntax.Append("text");
										hasText = false;
									}
									syntax.Append(ch);
								}
								else
								{
									hasText = true;
								}
								index++;
							}
							if (hasText)
							{
								syntax.Append("text");
							}
							index++;
							syntax.Append(text[index]);
							index++;
							goto IL_3A0;
						}
						}
					}
					else
					{
						if (!(commandName == "$d"))
						{
							goto IL_3A0;
						}
						goto IL_1A8;
					}
					int startIndex3 = index;
					while (index < text.Length && text[index] != '#')
					{
						index++;
					}
					syntax.Append(text.Substring(startIndex3, index - startIndex3).TrimEnd(' '));
					goto IL_3A0;
				}
				if (length != 6)
				{
					goto IL_3A0;
				}
				if (!(commandName == "$query"))
				{
					goto IL_3A0;
				}
				IL_1A8:
				int startIndex4 = index;
				while (index < text.Length && text[index] != '#')
				{
					index++;
				}
				index++;
				syntax.Append(text.Substring(startIndex4, index - startIndex4).TrimEnd(' '));
				int endIndex2 = index;
				while (endIndex2 < text.Length && text[endIndex2] != '#' && text[endIndex2] != '|')
				{
					endIndex2++;
				}
				this.ExtractDialogueSyntaxImpl(text, commandDelimiter, ref index, syntax, endIndex2 - 1);
				if (index >= text.Length || text[index] != '|')
				{
					return;
				}
				syntax.Append(text[index]);
				index++;
				int endIndex3 = index;
				while (endIndex3 < text.Length && text[endIndex3] != '#' && text[endIndex3] != '|')
				{
					endIndex3++;
				}
				this.ExtractDialogueSyntaxImpl(text, commandDelimiter, ref index, syntax, endIndex3 - 1);
			}
			IL_3A0:
			index--;
		}

		// Token: 0x06001915 RID: 6421 RVA: 0x001267A4 File Offset: 0x001249A4
		private void ExtractDialogueItemSpawnSyntax(string text, ref int index, StringBuilder syntax)
		{
			int startIndex = index;
			int endIndex = index;
			endIndex++;
			bool foundEnd = false;
			while (endIndex < text.Length)
			{
				char ch = text[endIndex];
				if (ch == ' ' || ch == '.' || char.IsLetter(ch) || char.IsNumber(ch))
				{
					endIndex++;
				}
				else
				{
					if (ch == ']')
					{
						foundEnd = true;
						break;
					}
					break;
				}
			}
			if (foundEnd)
			{
				syntax.Append(text.Substring(startIndex, endIndex - startIndex + 1).TrimEnd(' '));
				index = endIndex;
				return;
			}
			syntax.Append(text[index]);
			index++;
		}

		// Token: 0x06001916 RID: 6422 RVA: 0x00126830 File Offset: 0x00124A30
		private void ExtractMailCommandSyntax(string text, ref int index, StringBuilder syntax)
		{
			int startIndex = index;
			index++;
			while (index < text.Length && (char.IsLetter(text[index]) || char.IsNumber(text[index])))
			{
				index++;
			}
			string commandName = text.Substring(startIndex, index - startIndex);
			if (commandName == "%item")
			{
				syntax.Append(commandName);
				int startIndex2 = index;
				while (index < text.Length)
				{
					index++;
					if (index > 1 && text[index] == '%' && text[index - 1] == '%')
					{
						break;
					}
				}
				string command = (text[index] == '%' && text[index - 1] == '%' && char.IsWhiteSpace(text[index - 2])) ? (text.Substring(startIndex2, index - startIndex2 - 1).TrimEnd() + "%%") : text.Substring(startIndex2, index - startIndex2 + 1);
				syntax.Append(command);
				return;
			}
			if (!(commandName == "%revealtaste"))
			{
				syntax.Append(commandName);
				index--;
				return;
			}
			index -= "%revealtaste".Length;
			this.ExtractRevealTasteCommandSyntax(text, ref index, syntax);
		}

		// Token: 0x06001917 RID: 6423 RVA: 0x00126968 File Offset: 0x00124B68
		private void ExtractTagSyntax(string text, ref int index, StringBuilder syntax)
		{
			int startIndex = index;
			index++;
			while (index < text.Length - 1 && text[index] != ']')
			{
				index++;
			}
			syntax.Append(text.Substring(startIndex, index - startIndex + 1));
		}

		// Token: 0x06001918 RID: 6424 RVA: 0x001269B4 File Offset: 0x00124BB4
		private void ExtractRevealTasteCommandSyntax(string text, ref int index, StringBuilder syntax)
		{
			int startIndex = index;
			while (index < text.Length - 1)
			{
				char next = text[index + 1];
				if (char.IsWhiteSpace(next) || next == '#' || next == '%' || next == '$' || next == '{' || next == '^' || next == '*' || next == '[')
				{
					break;
				}
				index++;
			}
			syntax.Append(text.Substring(startIndex, index - startIndex + 1));
		}

		// Token: 0x06001919 RID: 6425 RVA: 0x00126A22 File Offset: 0x00124C22
		private void EndTextContext(ref bool hasText, StringBuilder syntax)
		{
			if (hasText)
			{
				syntax.Append("text");
				hasText = false;
			}
		}

		// Token: 0x0600191A RID: 6426 RVA: 0x00126A38 File Offset: 0x00124C38
		public SyntaxAbstractor()
		{
			Dictionary<string, ExtractSyntaxDelegate> dictionary = new Dictionary<string, ExtractSyntaxDelegate>(StringComparer.OrdinalIgnoreCase);
			dictionary["Characters/Dialogue/*"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Data/EngagementDialogue"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Data/ExtraDialogue"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/animationDescriptions"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/Buildings"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/Characters"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/Events"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/Locations"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/MovieReactions"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/Objects"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/Quests"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/schedules/*"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/SimpleNonVillagerDialogues"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/SpecialOrderStrings"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/SpeechBubbles"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/StringsFromCSFiles"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/StringsFromMaps"] = new ExtractSyntaxDelegate(SyntaxAbstractor.DialogueSyntaxHandler);
			dictionary["Strings/BigCraftables"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/BundleNames"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/EnchantmentNames"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/FarmAnimals"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/Furniture"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/MovieConcessions"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/Movies"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/NPCNames"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/Pants"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/Shirts"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/Tools"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/TV/TipChannel"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/UI"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/Weapons"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Strings/WorldMap"] = new ExtractSyntaxDelegate(SyntaxAbstractor.PlainTextSyntaxHandler);
			dictionary["Data/Events/*"] = new ExtractSyntaxDelegate(SyntaxAbstractor.EventSyntaxHandler);
			dictionary["Data/Festivals/*"] = new ExtractSyntaxDelegate(SyntaxAbstractor.FestivalSyntaxHandler);
			dictionary["Data/Achievements"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '^', new int[]
			{
				0,
				1
			}));
			dictionary["Data/Boots"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
			{
				1,
				6
			}));
			dictionary["Data/Bundles"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
			{
				6
			}));
			dictionary["Data/hats"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
			{
				1,
				5
			}));
			dictionary["Data/Monsters"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
			{
				14
			}));
			dictionary["Data/NPCGiftTastes"] = delegate(SyntaxAbstractor syntaxBuilder, string _, string key, string text)
			{
				if (!key.StartsWith("Universal_"))
				{
					return syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
					{
						0,
						2,
						4,
						6,
						8
					});
				}
				return text;
			};
			dictionary["Data/Quests"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
			{
				1,
				2,
				3
			}, new int[]
			{
				9
			}));
			dictionary["Data/TV/CookingChannel"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[]
			{
				1
			}));
			dictionary["Data/mail"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractMailSyntax(text));
			dictionary["Data/Notes"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractMailSyntax(text));
			dictionary["Data/SecretNotes"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractMailSyntax(text));
			dictionary["Strings/credits"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractCreditsSyntax(text));
			dictionary["Strings/1_6_Strings"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.Extract16StringsSyntax(key, text));
			dictionary["Strings/Lexicon"] = ((SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractLexiconSyntax(key, text));
			this.SyntaxHandlers = dictionary;
			base..ctor();
		}

		// Token: 0x04000F07 RID: 3847
		public const string TextMarker = "text";

		// Token: 0x04000F08 RID: 3848
		public readonly Dictionary<string, ExtractSyntaxDelegate> SyntaxHandlers;
	}
}

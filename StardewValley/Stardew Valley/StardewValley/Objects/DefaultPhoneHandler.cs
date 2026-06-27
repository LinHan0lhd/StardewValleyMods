using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects
{
	// Token: 0x020001BD RID: 445
	public class DefaultPhoneHandler : IPhoneHandler
	{
		// Token: 0x06001FBE RID: 8126 RVA: 0x0016C7E0 File Offset: 0x0016A9E0
		public string CheckForIncomingCall(Random random)
		{
			List<string> validCalls = new List<string>();
			bool baseChancePassed = random.NextDouble() < 0.01;
			foreach (KeyValuePair<string, IncomingPhoneCallData> entry in DataLoader.IncomingPhoneCalls(Game1.content))
			{
				if ((baseChancePassed || entry.Value.IgnoreBaseChance) && (entry.Value.TriggerCondition == null || GameStateQuery.CheckConditions(entry.Value.TriggerCondition, Game1.currentLocation, Game1.player, null, null, random, null)))
				{
					validCalls.Add(entry.Key);
				}
			}
			return random.ChooseFrom(validCalls);
		}

		// Token: 0x06001FBF RID: 8127 RVA: 0x0016C89C File Offset: 0x0016AA9C
		public bool TryHandleIncomingCall(string callId, out Action showDialogue)
		{
			showDialogue = null;
			IncomingPhoneCallData call;
			if (!DataLoader.IncomingPhoneCalls(Game1.content).TryGetValue(callId, out call))
			{
				return false;
			}
			int previousCalls;
			if (call.MaxCalls > -1 && Game1.player.callsReceived.TryGetValue(callId, out previousCalls) && previousCalls >= call.MaxCalls)
			{
				return false;
			}
			if (call.RingCondition != null && !GameStateQuery.CheckConditions(call.RingCondition, Game1.currentLocation, Game1.player, null, null, null, null))
			{
				return false;
			}
			if (Game1.IsGreenRainingHere(null))
			{
				return false;
			}
			showDialogue = delegate()
			{
				if (!string.IsNullOrWhiteSpace(call.SimpleDialogueSplitBy))
				{
					Game1.multipleDialogues((TokenParser.ParseText(call.Dialogue, null, null, null) ?? Dialogue.GetFallbackTextForError()).Split(call.SimpleDialogueSplitBy, StringSplitOptions.None));
					return;
				}
				NPC portraitNpc = null;
				if (call.FromNpc != null)
				{
					portraitNpc = Game1.getCharacterFromName(call.FromNpc, true, false);
					if (portraitNpc == null)
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Can't find NPC '");
						defaultInterpolatedStringHandler.AppendFormatted(call.FromNpc);
						defaultInterpolatedStringHandler.AppendLiteral("' for incoming call ID '");
						defaultInterpolatedStringHandler.AppendFormatted(callId);
						defaultInterpolatedStringHandler.AppendLiteral("'.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				string customDisplayName = TokenParser.ParseText(call.FromDisplayName, null, null, null);
				Texture2D customPortrait = null;
				if (call.FromPortrait != null)
				{
					if (!Game1.content.DoesAssetExist<Texture2D>(call.FromPortrait))
					{
						IGameLogger log2 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Can't load custom portrait '");
						defaultInterpolatedStringHandler.AppendFormatted(call.FromPortrait);
						defaultInterpolatedStringHandler.AppendLiteral("' for incoming call ID '");
						defaultInterpolatedStringHandler.AppendFormatted(callId);
						defaultInterpolatedStringHandler.AppendLiteral("' because that texture doesn't exist.");
						log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						customPortrait = Game1.content.Load<Texture2D>(call.FromPortrait);
					}
				}
				if (customPortrait != null || customDisplayName != null)
				{
					if (portraitNpc != null)
					{
						portraitNpc = new NPC(portraitNpc.Sprite, Vector2.Zero, "", 0, portraitNpc.Name, customPortrait ?? portraitNpc.Portrait, false);
						portraitNpc.displayName = (customDisplayName ?? portraitNpc.displayName);
					}
					else if (customPortrait != null)
					{
						portraitNpc = new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 16), Vector2.Zero, "", 0, "???", customPortrait, false)
						{
							displayName = (customDisplayName ?? "???")
						};
					}
				}
				string dialogueKey = "Data\\IncomingPhoneCalls:" + callId;
				string dialogueText = TokenParser.ParseText(call.Dialogue, null, null, null) ?? Dialogue.GetFallbackTextForError();
				Game1.DrawDialogue(new Dialogue(portraitNpc, dialogueKey, dialogueText));
			};
			return true;
		}

		// Token: 0x06001FC0 RID: 8128 RVA: 0x0016C958 File Offset: 0x0016AB58
		public IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers()
		{
			DefaultPhoneHandler.<>c__DisplayClass3_0 CS$<>8__locals1;
			CS$<>8__locals1.numbers = new List<KeyValuePair<string, string>>(6);
			DefaultPhoneHandler.<GetOutgoingNumbers>g__AddNumber|3_0("Carpenter", "Robin", ref CS$<>8__locals1);
			DefaultPhoneHandler.<GetOutgoingNumbers>g__AddNumber|3_0("Blacksmith", "Clint", ref CS$<>8__locals1);
			DefaultPhoneHandler.<GetOutgoingNumbers>g__AddNumber|3_0("SeedShop", "Pierre", ref CS$<>8__locals1);
			DefaultPhoneHandler.<GetOutgoingNumbers>g__AddNumber|3_0("AnimalShop", "Marnie", ref CS$<>8__locals1);
			DefaultPhoneHandler.<GetOutgoingNumbers>g__AddNumber|3_0("Saloon", "Gus", ref CS$<>8__locals1);
			if (Game1.player.mailReceived.Contains("Gil_Telephone") || Game1.player.mailReceived.Contains("Gil_FlameSpirits"))
			{
				DefaultPhoneHandler.<GetOutgoingNumbers>g__AddNumber|3_0("AdventureGuild", "Marlon", ref CS$<>8__locals1);
			}
			return CS$<>8__locals1.numbers;
		}

		// Token: 0x06001FC1 RID: 8129 RVA: 0x0016CA0C File Offset: 0x0016AC0C
		public bool TryHandleOutgoingCall(string callId)
		{
			if (callId == "AdventureGuild")
			{
				this.CallAdventureGuild();
				return true;
			}
			if (callId == "AnimalShop")
			{
				this.CallAnimalShop();
				return true;
			}
			if (callId == "Blacksmith")
			{
				this.CallBlacksmith();
				return true;
			}
			if (callId == "Carpenter")
			{
				this.CallCarpenter();
				return true;
			}
			if (callId == "Saloon")
			{
				this.CallSaloon();
				return true;
			}
			if (!(callId == "SeedShop"))
			{
				return false;
			}
			this.CallSeedShop();
			return true;
		}

		// Token: 0x06001FC2 RID: 8130 RVA: 0x0016CA9C File Offset: 0x0016AC9C
		public void CallAdventureGuild()
		{
			Game1.currentLocation.playShopPhoneNumberSounds("AdventureGuild");
			Game1.player.freezePause = 4950;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("bigSelect", null);
				NPC character = Game1.getCharacterFromName("Marlon", true, false);
				if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
				{
					Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_AlreadyRecovering");
					return;
				}
				Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_Open");
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
				{
					if (Game1.player.itemsLostLastDeath.Count > 0)
					{
						Game1.player.forceCanMove();
						Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon", true);
						return;
					}
					Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_NoDeathItems");
				}));
			}, 4950);
		}

		// Token: 0x06001FC3 RID: 8131 RVA: 0x0016CAF4 File Offset: 0x0016ACF4
		public void CallAnimalShop()
		{
			GameLocation location = Game1.currentLocation;
			location.playShopPhoneNumberSounds("AnimalShop");
			Game1.player.freezePause = 4950;
			Game1.afterFadeFunction <>9__1;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("bigSelect", null);
				NPC character = Game1.getCharacterFromName("Marnie", true, false);
				if (GameLocation.AreStoresClosedForFestival())
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Marnie_ClosedDay", Array.Empty<object>());
				}
				else if (character.ScheduleKey == "fall_18" || character.ScheduleKey == "winter_18" || character.ScheduleKey == "Tue" || character.ScheduleKey == "Mon")
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Marnie_ClosedDay", Array.Empty<object>());
				}
				else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
				{
					Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marnie_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
				}
				else
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Marnie_Closed", Array.Empty<object>());
				}
				Delegate afterDialogues = Game1.afterDialogues;
				Game1.afterFadeFunction b;
				if ((b = <>9__1) == null)
				{
					b = (<>9__1 = delegate()
					{
						Response[] responses = new Response[]
						{
							new Response("AnimalShop_CheckAnimalPrices", Game1.content.LoadString("Strings\\Characters:Phone_CheckAnimalPrices")),
							new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
						};
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), responses, "telephone");
					});
				}
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(afterDialogues, b);
			}, 4950);
		}

		// Token: 0x06001FC4 RID: 8132 RVA: 0x0016CB48 File Offset: 0x0016AD48
		public void CallBlacksmith()
		{
			GameLocation location = Game1.currentLocation;
			location.playShopPhoneNumberSounds("Blacksmith");
			Game1.player.freezePause = 4950;
			Game1.afterFadeFunction <>9__1;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("bigSelect", null);
				NPC character = Game1.getCharacterFromName("Clint", true, false);
				if (GameLocation.AreStoresClosedForFestival())
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Clint_Festival", Array.Empty<object>());
				}
				else if (Game1.player.daysLeftForToolUpgrade.Value > 0)
				{
					int daysLeft = Game1.player.daysLeftForToolUpgrade.Value;
					if (daysLeft == 1)
					{
						Game1.DrawDialogue(character, "Strings\\Characters:Phone_Clint_Working_OneDay");
					}
					else
					{
						Game1.DrawDialogue(character, "Strings\\Characters:Phone_Clint_Working", new object[]
						{
							daysLeft
						});
					}
				}
				else
				{
					string scheduleKey = character.ScheduleKey;
					if (!(scheduleKey == "winter_16"))
					{
						if (!(scheduleKey == "Fri"))
						{
							if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
							{
								Game1.DrawDialogue(character, "Strings\\Characters:Phone_Clint_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
							}
							else
							{
								Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Clint_Closed", Array.Empty<object>());
							}
						}
						else
						{
							Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Clint_Festival", Array.Empty<object>());
						}
					}
					else
					{
						Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Clint_Festival", Array.Empty<object>());
					}
				}
				Delegate afterDialogues = Game1.afterDialogues;
				Game1.afterFadeFunction b;
				if ((b = <>9__1) == null)
				{
					b = (<>9__1 = delegate()
					{
						Response[] responses = new Response[]
						{
							new Response("Blacksmith_UpgradeCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckToolCost")),
							new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
						};
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), responses, "telephone");
					});
				}
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(afterDialogues, b);
			}, 4950);
		}

		// Token: 0x06001FC5 RID: 8133 RVA: 0x0016CB9C File Offset: 0x0016AD9C
		public void CallCarpenter()
		{
			GameLocation location = Game1.currentLocation;
			location.playShopPhoneNumberSounds("Carpenter");
			Game1.player.freezePause = 4950;
			Game1.afterFadeFunction <>9__1;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("bigSelect", null);
				NPC character = Game1.getCharacterFromName("Robin", true, false);
				if (GameLocation.AreStoresClosedForFestival())
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Robin_Festival", Array.Empty<object>());
				}
				else
				{
					Town town = Game1.getLocationFromName("Town") as Town;
					if (town != null && town.daysUntilCommunityUpgrade.Value > 0)
					{
						int daysLeft = town.daysUntilCommunityUpgrade.Value;
						if (daysLeft == 1)
						{
							Game1.DrawDialogue(character, "Strings\\Characters:Phone_Robin_Working_OneDay");
						}
						else
						{
							Game1.DrawDialogue(character, "Strings\\Characters:Phone_Robin_Working", new object[]
							{
								daysLeft
							});
						}
					}
					else if (Game1.IsThereABuildingUnderConstruction("Robin"))
					{
						BuilderData builderData = Game1.netWorldState.Value.GetBuilderData("Robin");
						int daysLeft2 = 0;
						if (builderData != null)
						{
							daysLeft2 = builderData.daysUntilBuilt.Value;
						}
						if (daysLeft2 == 1)
						{
							Game1.DrawDialogue(character, "Strings\\Characters:Phone_Robin_Working_OneDay");
						}
						else
						{
							Game1.DrawDialogue(character, "Strings\\Characters:Phone_Robin_Working", new object[]
							{
								daysLeft2
							});
						}
					}
					else
					{
						string scheduleKey = character.ScheduleKey;
						if (!(scheduleKey == "summer_18"))
						{
							if (!(scheduleKey == "Tue"))
							{
								if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
								{
									Game1.DrawDialogue(character, "Strings\\Characters:Phone_Robin_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
								}
								else
								{
									Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Robin_Closed", Array.Empty<object>());
								}
							}
							else
							{
								Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Robin_Workout", Array.Empty<object>());
							}
						}
						else
						{
							Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Robin_Festival", Array.Empty<object>());
						}
					}
				}
				Delegate afterDialogues = Game1.afterDialogues;
				Game1.afterFadeFunction b;
				if ((b = <>9__1) == null)
				{
					b = (<>9__1 = delegate()
					{
						List<Response> responses = new List<Response>();
						responses.Add(new Response("Carpenter_ShopStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock")));
						if (Game1.player.houseUpgradeLevel.Value < 3)
						{
							responses.Add(new Response("Carpenter_HouseCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckHouseCost")));
						}
						responses.Add(new Response("Carpenter_BuildingCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckBuildingCost")));
						responses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), responses.ToArray(), "telephone");
					});
				}
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(afterDialogues, b);
			}, 4950);
		}

		// Token: 0x06001FC6 RID: 8134 RVA: 0x0016CBF0 File Offset: 0x0016ADF0
		public void CallSaloon()
		{
			GameLocation location = Game1.currentLocation;
			location.playShopPhoneNumberSounds("Saloon");
			Game1.player.freezePause = 4950;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("bigSelect", null);
				NPC character = Game1.getCharacterFromName("Gus", true, false);
				if (GameLocation.AreStoresClosedForFestival())
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Gus_Festival", Array.Empty<object>());
				}
				else if (Game1.timeOfDay >= 1200 && Game1.timeOfDay < 2400 && (character.ScheduleKey != "fall_4" || Game1.timeOfDay >= 1700))
				{
					if (Game1.dishOfTheDay != null)
					{
						Game1.DrawDialogue(character, "Strings\\Characters:Phone_Gus_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""), new object[]
						{
							Game1.dishOfTheDay.DisplayName
						});
					}
					else
					{
						Game1.DrawDialogue(character, "Strings\\Characters:Phone_Gus_Open_NoDishOfTheDay");
					}
				}
				else if (Game1.dishOfTheDay != null && Game1.timeOfDay < 2400)
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Gus_Closed", new object[]
					{
						Game1.dishOfTheDay.DisplayName
					});
				}
				else
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Gus_Closed_NoDishOfTheDay", Array.Empty<object>());
				}
				location.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
			}, 4950);
		}

		// Token: 0x06001FC7 RID: 8135 RVA: 0x0016CC44 File Offset: 0x0016AE44
		public void CallSeedShop()
		{
			GameLocation location = Game1.currentLocation;
			location.playShopPhoneNumberSounds("SeedShop");
			Game1.player.freezePause = 4950;
			Game1.afterFadeFunction <>9__1;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("bigSelect", null);
				NPC character = Game1.getCharacterFromName("Pierre", true, false);
				string dayName = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
				if (GameLocation.AreStoresClosedForFestival())
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Pierre_Festival", Array.Empty<object>());
				}
				else if ((Game1.isLocationAccessible("CommunityCenter") || dayName != "Wed") && Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
				{
					Game1.DrawDialogue(character, "Strings\\Characters:Phone_Pierre_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
				}
				else
				{
					Game1.DrawAnsweringMachineDialogue(character, "Strings\\Characters:Phone_Pierre_Closed", Array.Empty<object>());
				}
				Delegate afterDialogues = Game1.afterDialogues;
				Game1.afterFadeFunction b;
				if ((b = <>9__1) == null)
				{
					b = (<>9__1 = delegate()
					{
						Response[] responses = new Response[]
						{
							new Response("SeedShop_CheckSeedStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock")),
							new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
						};
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), responses, "telephone");
					});
				}
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(afterDialogues, b);
			}, 4950);
		}

		// Token: 0x06001FC9 RID: 8137 RVA: 0x0016CCA0 File Offset: 0x0016AEA0
		[CompilerGenerated]
		internal static void <GetOutgoingNumbers>g__AddNumber|3_0(string callerId, string npcName, ref DefaultPhoneHandler.<>c__DisplayClass3_0 A_2)
		{
			NPC callerNpc = Game1.getCharacterFromName(npcName, true, false);
			if (callerNpc != null)
			{
				A_2.numbers.Add(new KeyValuePair<string, string>(callerId, callerNpc.displayName));
			}
		}

		// Token: 0x02000561 RID: 1377
		public static class OutgoingCallIds
		{
			// Token: 0x04002B66 RID: 11110
			public const string AdventureGuild = "AdventureGuild";

			// Token: 0x04002B67 RID: 11111
			public const string AnimalShop = "AnimalShop";

			// Token: 0x04002B68 RID: 11112
			public const string Blacksmith = "Blacksmith";

			// Token: 0x04002B69 RID: 11113
			public const string Carpenter = "Carpenter";

			// Token: 0x04002B6A RID: 11114
			public const string Saloon = "Saloon";

			// Token: 0x04002B6B RID: 11115
			public const string SeedShop = "SeedShop";
		}
	}
}

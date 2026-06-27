using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace StardewValley
{
	// Token: 0x02000089 RID: 137
	public static class ChatCommands
	{
		// Token: 0x170000AC RID: 172
		// (get) Token: 0x0600057A RID: 1402 RVA: 0x0001CFA4 File Offset: 0x0001B1A4
		public static bool AllowCheats
		{
			get
			{
				if (!Program.enableCheats)
				{
					Farmer player = Game1.player;
					bool? flag;
					if (player == null)
					{
						flag = null;
					}
					else
					{
						FarmerTeam team = player.team;
						flag = ((team != null) ? new bool?(team.allowChatCheats.Value) : null);
					}
					bool? flag2 = flag;
					return flag2.GetValueOrDefault();
				}
				return true;
			}
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0001CFFC File Offset: 0x0001B1FC
		static ChatCommands()
		{
			ChatCommands.Register("qi", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Qi), null, null, false, false, false);
			ChatCommands.Register("concernedApe", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.ConcernedApe), null, new string[]
			{
				"ape",
				"ca"
			}, false, false, false);
			ChatCommands.Register("cheat", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Cheat), null, new string[]
			{
				"showMeTheMoney",
				"imACheat",
				"cheats",
				"freeGold",
				"rosebud"
			}, false, false, false);
			ChatCommands.Register("money", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Money), null, null, false, false, true);
			ChatCommands.Register("help", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Help), null, new string[]
			{
				"h"
			}, false, false, false);
			ChatCommands.Register("clear", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Clear), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Clear_Help", name), null, false, false, false);
			ChatCommands.Register("list", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.List), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_List_Help", name), new string[]
			{
				"users",
				"players"
			}, false, false, false);
			ChatCommands.Register("color", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Color), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Color_Help", name), null, false, false, false);
			ChatCommands.Register("color-list", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.ColorList), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_ColorList_Help", name), null, false, false, false);
			ChatCommands.Register("emote", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Emote), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Emote_Help", name), new string[]
			{
				"e"
			}, false, false, false);
			ChatCommands.Register("mapScreenshot", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.MapScreenshot), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_MapScreenshot_Help", name), null, false, false, false);
			ChatCommands.Register("pause", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Pause), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Pause_Help", name), null, false, false, false);
			ChatCommands.Register("resume", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Resume), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Resume_Help", name), null, false, false, false);
			ChatCommands.Register("message", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Message), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Message_Help", name), new string[]
			{
				"dm",
				"pm",
				"whisper"
			}, false, true, false);
			ChatCommands.Register("reply", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Reply), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Reply_Help", name), new string[]
			{
				"r"
			}, false, true, false);
			ChatCommands.Register("ping", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Ping), null, null, false, true, false);
			ChatCommands.Register("kick", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Kick), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Kick_Help", name), null, true, true, false);
			ChatCommands.Register("ban", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Ban), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Ban_Help", name), null, true, true, false);
			ChatCommands.Register("unban", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Unban), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_Help", name), null, true, true, false);
			ChatCommands.Register("unbanAll", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.UnbanAll), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_UnbanAll_Help", name), null, true, true, false);
			ChatCommands.Register("moveBuildingPermission", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.MoveBuildingPermission), null, new string[]
			{
				"mbp",
				"movePermission"
			}, true, true, false);
			ChatCommands.Register("sleepAnnounceMode", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.SleepAnnounceMode), null, null, true, true, false);
			ChatCommands.Register("unlinkPlayer", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.UnlinkPlayer), (string name) => Game1.content.LoadString("Strings\\UI:ChatCommands_UnlinkPlayer_Help", name), null, true, true, false);
			ChatCommands.Register("debug", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.Debug), null, null, false, false, true);
			ChatCommands.Register("logFile", ChatCommands.GetDebugPassThrough("LogFile"), null, null, false, false, false);
			ChatCommands.Register("printDiag", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.PrintDiag), null, null, false, false, false);
			ChatCommands.Register("recountNuts", new ChatCommandHandlerDelegate(ChatCommands.DefaultHandlers.RecountNuts), null, null, false, false, false);
			ChatCommands.Register("sdlVersion", ChatCommands.GetDebugPassThrough("SdlVersion"), null, new string[]
			{
				"sdlv"
			}, false, false, false);
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0001D4B5 File Offset: 0x0001B6B5
		public static bool Exists(string commandName)
		{
			return commandName != null && (ChatCommands.Handlers.ContainsKey(commandName) || ChatCommands.Aliases.ContainsKey(commandName));
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0001D4D8 File Offset: 0x0001B6D8
		public static void Register(string commandName, ChatCommandHandlerDelegate handler, Func<string, string> helpDescription, string[] aliases = null, bool mainOnly = false, bool multiplayerOnly = false, bool cheatsOnly = false)
		{
			commandName = ((commandName != null) ? commandName.Trim() : null);
			if (string.IsNullOrWhiteSpace(commandName))
			{
				throw new ArgumentException("The chat command name can't be null or empty.", "commandName");
			}
			if (ChatCommands.Handlers.ContainsKey(commandName))
			{
				throw new InvalidOperationException("The chat command name '" + commandName + "' is already registered.");
			}
			string aliasFor;
			if (ChatCommands.Aliases.TryGetValue(commandName, out aliasFor))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The chat command name '");
				defaultInterpolatedStringHandler.AppendFormatted(commandName);
				defaultInterpolatedStringHandler.AppendLiteral("' is already registered as an alias of '");
				defaultInterpolatedStringHandler.AppendFormatted(aliasFor);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			ChatCommands.Handlers[commandName] = new ChatCommands.ChatCommand(commandName, helpDescription, handler, mainOnly, multiplayerOnly, cheatsOnly);
			if (aliases != null && aliases.Length != 0)
			{
				for (int i = 0; i < aliases.Length; i++)
				{
					ChatCommands.RegisterAlias(aliases[i], commandName);
				}
			}
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0001D5D0 File Offset: 0x0001B7D0
		public static void RegisterAlias(string alias, string commandName)
		{
			alias = ((alias != null) ? alias.Trim() : null);
			if (string.IsNullOrWhiteSpace(alias))
			{
				throw new ArgumentException("The alias can't be null or empty.", "alias");
			}
			if (ChatCommands.Handlers.ContainsKey(alias))
			{
				throw new InvalidOperationException("The alias '" + alias + "' is already registered as a chat command name.");
			}
			string otherQuery;
			if (ChatCommands.Aliases.TryGetValue(alias, out otherQuery))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' is already registered for '");
				defaultInterpolatedStringHandler.AppendFormatted(otherQuery);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (string.IsNullOrWhiteSpace(commandName))
			{
				throw new ArgumentException("The chat command name can't be null or empty.", "alias");
			}
			if (!ChatCommands.Handlers.ContainsKey(commandName))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' can't be registered for '");
				defaultInterpolatedStringHandler.AppendFormatted(commandName);
				defaultInterpolatedStringHandler.AppendLiteral("' because there's no chat command with that name.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			ChatCommands.Aliases[alias] = commandName;
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0001D6FC File Offset: 0x0001B8FC
		public static bool TryHandle(string[] command, ChatBox chat)
		{
			string commandName = ArgUtility.Get(command, 0, null, true);
			if (string.IsNullOrWhiteSpace(commandName))
			{
				return false;
			}
			string aliasTarget;
			if (ChatCommands.Aliases.TryGetValue(commandName, out aliasTarget))
			{
				commandName = aliasTarget;
			}
			ChatCommands.ChatCommand handler;
			if (!ChatCommands.Handlers.TryGetValue(commandName, out handler))
			{
				return false;
			}
			if (handler.IsMainPlayerOnly && !Game1.IsMasterGame)
			{
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_HostOnly"));
				return true;
			}
			if (handler.IsMultiplayerOnly && !Game1.IsServer && !Game1.IsMultiplayer)
			{
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_MultiplayerOnly"));
				return true;
			}
			if (!handler.IsCheatsOnly || ChatCommands.AllowCheats)
			{
				bool result;
				try
				{
					handler.Handler(command, chat);
					result = true;
				}
				catch (Exception ex)
				{
					Game1.log.Error("Error running chat command '" + string.Join(" ", command) + "'.", ex);
					result = false;
				}
				return result;
			}
			string name = handler.Name;
			if (name == "cheat" || name == "debug" || name == "money")
			{
				chat.addNiceTryEasterEggMessage();
				return true;
			}
			chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_CheatsOnly"));
			return true;
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0001D840 File Offset: 0x0001BA40
		public static ChatCommandHandlerDelegate GetDebugPassThrough(string debugCommandName)
		{
			ChatCommands.<>c__DisplayClass9_0 CS$<>8__locals1 = new ChatCommands.<>c__DisplayClass9_0();
			CS$<>8__locals1.debugCommandName = debugCommandName;
			return new ChatCommandHandlerDelegate(CS$<>8__locals1.<GetDebugPassThrough>g__Handle|0);
		}

		// Token: 0x04000290 RID: 656
		private static readonly Dictionary<string, ChatCommands.ChatCommand> Handlers = new Dictionary<string, ChatCommands.ChatCommand>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000291 RID: 657
		private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x02000403 RID: 1027
		public class ChatCommand
		{
			// Token: 0x06003A50 RID: 14928 RVA: 0x002D87A5 File Offset: 0x002D69A5
			public ChatCommand(string name, Func<string, string> helpDescription, ChatCommandHandlerDelegate handler, bool isMainPlayerOnly, bool isMultiplayerOnly, bool isCheatsOnly)
			{
				this.Name = name;
				this.HelpDescription = helpDescription;
				this.Handler = handler;
				this.IsMainPlayerOnly = isMainPlayerOnly;
				this.IsMultiplayerOnly = isMultiplayerOnly;
				this.IsCheatsOnly = isCheatsOnly;
			}

			// Token: 0x06003A51 RID: 14929 RVA: 0x002D87DA File Offset: 0x002D69DA
			public bool IsVisible()
			{
				return (!this.IsMainPlayerOnly || Game1.IsMasterGame) && (!this.IsMultiplayerOnly || Game1.IsServer || Game1.IsMultiplayer) && (!this.IsCheatsOnly || ChatCommands.AllowCheats);
			}

			// Token: 0x040026F8 RID: 9976
			public readonly string Name;

			// Token: 0x040026F9 RID: 9977
			public readonly ChatCommandHandlerDelegate Handler;

			// Token: 0x040026FA RID: 9978
			public readonly Func<string, string> HelpDescription;

			// Token: 0x040026FB RID: 9979
			public readonly bool IsMainPlayerOnly;

			// Token: 0x040026FC RID: 9980
			public readonly bool IsMultiplayerOnly;

			// Token: 0x040026FD RID: 9981
			public readonly bool IsCheatsOnly;
		}

		// Token: 0x02000404 RID: 1028
		public static class DefaultHandlers
		{
			// Token: 0x06003A52 RID: 14930 RVA: 0x002D8818 File Offset: 0x002D6A18
			public static void Ban(string[] command, ChatBox chat)
			{
				int index = 0;
				Farmer farmer = chat.findMatchingFarmer(command, ref index, true, true);
				if (farmer == null)
				{
					chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchOnlinePlayer"));
					chat.listPlayers(true, true);
					return;
				}
				string userId = Game1.server.ban(farmer.UniqueMultiplayerID);
				string userName;
				if (userId == null || !Game1.bannedUsers.TryGetValue(userId, out userName))
				{
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Ban_Failed"));
					return;
				}
				string userDisplay = (userName != null) ? (userName + " (" + userId + ")") : userId;
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Ban_Done", userDisplay));
			}

			// Token: 0x06003A53 RID: 14931 RVA: 0x002D88BD File Offset: 0x002D6ABD
			public static void Cheat(string[] command, ChatBox chat)
			{
				chat.addNiceTryEasterEggMessage();
			}

			// Token: 0x06003A54 RID: 14932 RVA: 0x002D88C5 File Offset: 0x002D6AC5
			public static void Clear(string[] command, ChatBox chat)
			{
				chat.messages.Clear();
			}

			// Token: 0x06003A55 RID: 14933 RVA: 0x002D88D2 File Offset: 0x002D6AD2
			public static void Color(string[] command, ChatBox chat)
			{
				if (command.Length > 1)
				{
					Game1.player.defaultChatColor = command[1];
				}
			}

			// Token: 0x06003A56 RID: 14934 RVA: 0x002D88E8 File Offset: 0x002D6AE8
			public static void ConcernedApe(string[] command, ChatBox chat)
			{
				if (Game1.player.mailReceived.Add("apeChat1"))
				{
					chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ConcernedApe_1"), new Color(104, 214, 255));
					return;
				}
				chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ConcernedApe_2"), Microsoft.Xna.Framework.Color.Yellow);
			}

			// Token: 0x06003A57 RID: 14935 RVA: 0x002D894C File Offset: 0x002D6B4C
			public static void ColorList(string[] command, ChatBox chat)
			{
				chat.addMessage("white, red, blue, green, jade, yellowgreen, pink, purple, yellow, orange, brown, gray, cream, salmon, peach, aqua, jungle, plum", Microsoft.Xna.Framework.Color.White);
			}

			// Token: 0x06003A58 RID: 14936 RVA: 0x002D8960 File Offset: 0x002D6B60
			public static void Debug(string[] command, ChatBox chat)
			{
				string commandText = ArgUtility.UnsplitQuoteAware(command, ' ', 1, int.MaxValue);
				if (string.IsNullOrWhiteSpace(commandText))
				{
					chat.addErrorMessage("invalid usage: requires a debug command to run");
					return;
				}
				chat.cheat(commandText, true);
			}

			// Token: 0x06003A59 RID: 14937 RVA: 0x002D8998 File Offset: 0x002D6B98
			public static void Emote(string[] command, ChatBox chat)
			{
				if (!Game1.player.CanEmote())
				{
					return;
				}
				bool validEmote = false;
				if (command.Length > 1)
				{
					string emoteType = command[1].ToLowerInvariant();
					emoteType = emoteType.Substring(0, Math.Min(emoteType.Length, 16));
					for (int i = 0; i < Farmer.EMOTES.Length; i++)
					{
						if (emoteType == Farmer.EMOTES[i].emoteString)
						{
							validEmote = true;
							break;
						}
					}
					if (validEmote)
					{
						Game1.player.netDoEmote(emoteType);
					}
				}
				if (!validEmote)
				{
					string emoteList = "";
					for (int j = 0; j < Farmer.EMOTES.Length; j++)
					{
						if (!Farmer.EMOTES[j].hidden)
						{
							emoteList += Farmer.EMOTES[j].emoteString;
							if (j < Farmer.EMOTES.Length - 1)
							{
								emoteList += ", ";
							}
						}
					}
					chat.addMessage(emoteList, Microsoft.Xna.Framework.Color.White);
				}
			}

			// Token: 0x06003A5A RID: 14938 RVA: 0x002D8A78 File Offset: 0x002D6C78
			public static void Help(string[] command, ChatBox chat)
			{
				string searchCommandName = ArgUtility.Get(command, 1, null, true);
				if (searchCommandName != null)
				{
					ChatCommands.ChatCommand handler;
					if (ChatCommands.Handlers.TryGetValue(searchCommandName, out handler))
					{
						Func<string, string> helpDescription = handler.HelpDescription;
						string description = (helpDescription != null) ? helpDescription(handler.Name) : null;
						if (description != null)
						{
							chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_CommandDescription", description));
							return;
						}
					}
					chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_NoSuchCommand", searchCommandName));
				}
				List<string> commandNames = new List<string>();
				List<string> multiplayerCommandNames = new List<string>();
				foreach (ChatCommands.ChatCommand handler2 in ChatCommands.Handlers.Values)
				{
					if (handler2.IsVisible())
					{
						Func<string, string> helpDescription2 = handler2.HelpDescription;
						if (((helpDescription2 != null) ? helpDescription2(handler2.Name) : null) != null)
						{
							if (handler2.IsMultiplayerOnly)
							{
								multiplayerCommandNames.Add(handler2.Name);
							}
							else
							{
								commandNames.Add(handler2.Name);
							}
						}
					}
				}
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_Intro"));
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_CommandList", string.Join(", ", commandNames)));
				if (multiplayerCommandNames.Count > 0)
				{
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Help_MultiplayerCommandList", string.Join(", ", multiplayerCommandNames)));
				}
			}

			// Token: 0x06003A5B RID: 14939 RVA: 0x002D8BE8 File Offset: 0x002D6DE8
			public static void Kick(string[] command, ChatBox chat)
			{
				int index = 0;
				Farmer farmer = chat.findMatchingFarmer(command, ref index, true, true);
				if (farmer != null)
				{
					Game1.server.kick(farmer.UniqueMultiplayerID);
					return;
				}
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchOnlinePlayer"));
				chat.listPlayers(true, true);
			}

			// Token: 0x06003A5C RID: 14940 RVA: 0x002D8C34 File Offset: 0x002D6E34
			public static void List(string[] command, ChatBox chat)
			{
				chat.listPlayers(false, true);
			}

			// Token: 0x06003A5D RID: 14941 RVA: 0x002D8C40 File Offset: 0x002D6E40
			public static void MapScreenshot(string[] command, ChatBox chat)
			{
				if (!Game1.game1.CanTakeScreenshots())
				{
					return;
				}
				int scale = 25;
				string screenshotName = null;
				if (command.Length > 2 && !int.TryParse(command[2], out scale))
				{
					scale = 25;
				}
				if (command.Length > 1)
				{
					screenshotName = command[1];
				}
				if (scale <= 10)
				{
					scale = 10;
				}
				string result = Game1.game1.takeMapScreenshot(new float?((float)scale / 100f), screenshotName, null);
				if (result != null)
				{
					chat.addMessage("Wrote '" + result + "'.", Microsoft.Xna.Framework.Color.White);
					return;
				}
				chat.addMessage("Failed.", Microsoft.Xna.Framework.Color.Red);
			}

			// Token: 0x06003A5E RID: 14942 RVA: 0x002D8CCF File Offset: 0x002D6ECF
			public static void Message(string[] command, ChatBox chat)
			{
				chat.sendPrivateMessage(command);
			}

			// Token: 0x06003A5F RID: 14943 RVA: 0x002D8CD8 File Offset: 0x002D6ED8
			public static void Money(string[] command, ChatBox chat)
			{
				ChatCommands.GetDebugPassThrough("Money")(command, chat);
			}

			// Token: 0x06003A60 RID: 14944 RVA: 0x002D8CEC File Offset: 0x002D6EEC
			public static void MoveBuildingPermission(string[] command, ChatBox chat)
			{
				if (command.Length <= 1)
				{
					chat.addMessage("off, owned, on", Microsoft.Xna.Framework.Color.White);
					return;
				}
				string a = command[1].ToLowerInvariant();
				if (!(a == "off"))
				{
					if (!(a == "owned"))
					{
						if (a == "on")
						{
							Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
						}
					}
					else
					{
						Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
					}
				}
				else
				{
					Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("moveBuildingPermission ");
				defaultInterpolatedStringHandler.AppendFormatted<FarmerTeam.RemoteBuildingPermissions>(Game1.player.team.farmhandsCanMoveBuildings.Value);
				chat.addMessage(defaultInterpolatedStringHandler.ToStringAndClear(), Microsoft.Xna.Framework.Color.White);
			}

			// Token: 0x06003A61 RID: 14945 RVA: 0x002D8DC8 File Offset: 0x002D6FC8
			public static void Pause(string[] command, ChatBox chat)
			{
				if (!Game1.IsMasterGame)
				{
					chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_HostOnly"));
					return;
				}
				Game1.netWorldState.Value.IsPaused = !Game1.netWorldState.Value.IsPaused;
				chat.globalInfoMessage(Game1.netWorldState.Value.IsPaused ? "Paused" : "Resumed", Array.Empty<string>());
			}

			// Token: 0x06003A62 RID: 14946 RVA: 0x002D8E3C File Offset: 0x002D703C
			public static void Ping(string[] command, ChatBox chat)
			{
				if (Game1.IsMultiplayer)
				{
					StringBuilder sb = new StringBuilder();
					if (Game1.IsServer)
					{
						using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								KeyValuePair<long, Farmer> farmer = enumerator.Current;
								sb.Clear();
								sb.AppendFormat("Ping({0}) {1}ms ", farmer.Value.Name, (int)Game1.server.getPingToClient(farmer.Key));
								chat.addMessage(sb.ToString(), Microsoft.Xna.Framework.Color.White);
							}
							return;
						}
					}
					sb.AppendFormat("Ping: {0}ms", (int)Game1.client.GetPingToHost());
					chat.addMessage(sb.ToString(), Microsoft.Xna.Framework.Color.White);
				}
			}

			// Token: 0x06003A63 RID: 14947 RVA: 0x002D8F14 File Offset: 0x002D7114
			public static void PrintDiag(string[] command, ChatBox chat)
			{
				StringBuilder sb = new StringBuilder();
				Program.AppendDiagnostics(sb);
				chat.addInfoMessage(sb.ToString());
				Game1.log.Info(sb.ToString());
			}

			// Token: 0x06003A64 RID: 14948 RVA: 0x002D8F4C File Offset: 0x002D714C
			public static void Qi(string[] command, ChatBox chat)
			{
				if (Game1.player.mailReceived.Add("QiChat1"))
				{
					chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Qi_1"), new Color(100, 50, 255));
					return;
				}
				if (Game1.player.mailReceived.Add("QiChat2"))
				{
					chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Qi_2"), new Color(100, 50, 255));
					chat.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Qi_3"), Microsoft.Xna.Framework.Color.Yellow);
				}
			}

			// Token: 0x06003A65 RID: 14949 RVA: 0x002D8FE6 File Offset: 0x002D71E6
			public static void RecountNuts(string[] command, ChatBox chat)
			{
				Game1.game1.RecountWalnuts();
			}

			// Token: 0x06003A66 RID: 14950 RVA: 0x002D8FF2 File Offset: 0x002D71F2
			public static void Reply(string[] command, ChatBox chat)
			{
				chat.replyPrivateMessage(command);
			}

			// Token: 0x06003A67 RID: 14951 RVA: 0x002D8FFC File Offset: 0x002D71FC
			public static void Resume(string[] command, ChatBox chat)
			{
				if (!Game1.IsMasterGame)
				{
					chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_HostOnly"));
					return;
				}
				if (Game1.netWorldState.Value.IsPaused)
				{
					Game1.netWorldState.Value.IsPaused = false;
					chat.globalInfoMessage("Resumed", Array.Empty<string>());
				}
			}

			// Token: 0x06003A68 RID: 14952 RVA: 0x002D9058 File Offset: 0x002D7258
			public static void SleepAnnounceMode(string[] command, ChatBox chat)
			{
				if (command.Length <= 1)
				{
					return;
				}
				string a = command[1].ToLowerInvariant();
				if (!(a == "all"))
				{
					if (!(a == "first"))
					{
						if (a == "off")
						{
							Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.Off;
						}
					}
					else
					{
						Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.First;
					}
				}
				else
				{
					Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.All;
				}
				Multiplayer multiplayer = Game1.multiplayer;
				string messageKey = "SleepAnnounceModeSet";
				string[] array = new string[1];
				int num = 0;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Strings\\UI:ChatCommands_SleepAnnounceMode_");
				defaultInterpolatedStringHandler.AppendFormatted<FarmerTeam.SleepAnnounceModes>(Game1.player.team.sleepAnnounceMode.Value);
				array[num] = TokenStringBuilder.LocalizedText(defaultInterpolatedStringHandler.ToStringAndClear());
				multiplayer.globalChatInfoMessage(messageKey, array);
			}

			// Token: 0x06003A69 RID: 14953 RVA: 0x002D9138 File Offset: 0x002D7338
			public static void Unban(string[] command, ChatBox chat)
			{
				if (Game1.bannedUsers.Count == 0)
				{
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_NoPlayersBanned"));
					return;
				}
				bool listUnbannablePlayers = false;
				if (command.Length > 1)
				{
					string unbanId = command[1];
					string userId = null;
					string userName;
					if (Game1.bannedUsers.TryGetValue(unbanId, out userName))
					{
						userId = unbanId;
					}
					else
					{
						foreach (KeyValuePair<string, string> bannedUser in Game1.bannedUsers)
						{
							if (bannedUser.Value == unbanId)
							{
								userId = bannedUser.Key;
								userName = bannedUser.Value;
								break;
							}
						}
					}
					if (userId != null)
					{
						string userDisplay = (userName != null) ? (userName + " (" + userId + ")") : userId;
						chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_Done", userDisplay));
						Game1.bannedUsers.Remove(userId);
					}
					else
					{
						listUnbannablePlayers = true;
						chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_PlayerNotFound"));
					}
				}
				else
				{
					listUnbannablePlayers = true;
				}
				if (listUnbannablePlayers)
				{
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_PlayerList"));
					foreach (KeyValuePair<string, string> bannedUser2 in Game1.bannedUsers)
					{
						string userDisplay2 = "- " + bannedUser2.Key;
						if (bannedUser2.Value != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
							defaultInterpolatedStringHandler.AppendLiteral("- ");
							defaultInterpolatedStringHandler.AppendFormatted(bannedUser2.Value);
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted(bannedUser2.Key);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							userDisplay2 = defaultInterpolatedStringHandler.ToStringAndClear();
						}
						chat.addInfoMessage(userDisplay2);
					}
				}
			}

			// Token: 0x06003A6A RID: 14954 RVA: 0x002D9318 File Offset: 0x002D7518
			public static void UnbanAll(string[] command, ChatBox chat)
			{
				if (Game1.bannedUsers.Count == 0)
				{
					chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Unban_NoPlayersBanned"));
					return;
				}
				chat.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_UnbanAll_Done"));
				Game1.bannedUsers.Clear();
			}

			// Token: 0x06003A6B RID: 14955 RVA: 0x002D9368 File Offset: 0x002D7568
			public static void UnlinkPlayer(string[] command, ChatBox chat)
			{
				int index = 0;
				Farmer farmer = chat.findMatchingFarmer(command, ref index, true, false);
				if (farmer != null)
				{
					farmer.userID.Value = string.Empty;
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Unlinked ");
					defaultInterpolatedStringHandler.AppendFormatted(farmer.isActive() ? "active" : "inactive");
					defaultInterpolatedStringHandler.AppendLiteral(" player ");
					defaultInterpolatedStringHandler.AppendFormatted<NetLong>(farmer.uniqueMultiplayerID);
					defaultInterpolatedStringHandler.AppendLiteral(" ('");
					defaultInterpolatedStringHandler.AppendFormatted(farmer.Name);
					defaultInterpolatedStringHandler.AppendLiteral("').");
					log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				chat.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchPlayer"));
				chat.listPlayers(true, false);
			}
		}
	}
}

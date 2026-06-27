using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley
{
	// Token: 0x0200009C RID: 156
	public class Event
	{
		// Token: 0x060006EE RID: 1774 RVA: 0x0002A368 File Offset: 0x00028568
		public static void RegisterCommand(string name, EventCommandDelegate action)
		{
			Event.SetupEventCommandsIfNeeded();
			if (Event.Commands.ContainsKey(name))
			{
				Game1.log.Warn("Warning: event command " + name + " is already defined and will be overwritten.");
			}
			Event.Commands[name] = action;
			Event.CommandNames.Add(name);
			Game1.log.Verbose("Registered event command: " + name);
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x0002A3D0 File Offset: 0x000285D0
		public static void RegisterCommandAlias(string alias, string commandName)
		{
			Event.SetupEventCommandsIfNeeded();
			if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(commandName))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(124, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event command alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for '");
				defaultInterpolatedStringHandler.AppendFormatted(commandName);
				defaultInterpolatedStringHandler.AppendLiteral("' because the alias and command name must both be non-null and non-empty strings.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			if (Event.Commands.ContainsKey(alias))
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(95, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event command alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for command '");
				defaultInterpolatedStringHandler.AppendFormatted(commandName);
				defaultInterpolatedStringHandler.AppendLiteral("', because there's a command with that name.");
				log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			string conflictingName;
			if (Event.CommandAliases.TryGetValue(alias, out conflictingName))
			{
				IGameLogger log3 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event command alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for command '");
				defaultInterpolatedStringHandler.AppendFormatted(commandName);
				defaultInterpolatedStringHandler.AppendLiteral("', because that's already an alias for '");
				defaultInterpolatedStringHandler.AppendFormatted(conflictingName);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log3.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			if (!Event.Commands.ContainsKey(commandName))
			{
				IGameLogger log4 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event command alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for command '");
				defaultInterpolatedStringHandler.AppendFormatted(commandName);
				defaultInterpolatedStringHandler.AppendLiteral("', because there's no such command.");
				log4.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			Event.CommandAliases[alias] = commandName;
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x0002A57F File Offset: 0x0002877F
		public static bool TryResolveCommandName(string name, out string actualName)
		{
			Event.SetupEventCommandsIfNeeded();
			if (Event.CommandAliases.TryGetValue(name, out actualName))
			{
				return true;
			}
			if (Event.CommandNames.TryGetValue(name, out actualName))
			{
				return true;
			}
			actualName = null;
			return false;
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x0002A5AC File Offset: 0x000287AC
		public static void RegisterPrecondition(string name, EventPreconditionDelegate action)
		{
			Event.SetupEventCommandsIfNeeded();
			if (Event.Preconditions.ContainsKey(name))
			{
				Game1.log.Warn("Warning: event precondition " + name + " is already defined and will be overwritten.");
			}
			if (Event.PreconditionAliases.Remove(name))
			{
				Game1.log.Warn("Warning: '" + name + "' was previously registered as a precondition alias. The alias was removed.");
			}
			Event.Preconditions[name] = action;
			Game1.log.Verbose("Registered precondition: " + name);
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x0002A630 File Offset: 0x00028830
		public static void RegisterPreconditionAlias(string alias, string preconditionName)
		{
			Event.SetupEventCommandsIfNeeded();
			if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(preconditionName))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(134, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event precondition alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for '");
				defaultInterpolatedStringHandler.AppendFormatted(preconditionName);
				defaultInterpolatedStringHandler.AppendLiteral("' because the alias and precondition name must both be non-null and non-empty strings.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			if (Event.Preconditions.ContainsKey(alias))
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(110, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event precondition alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for precondition '");
				defaultInterpolatedStringHandler.AppendFormatted(preconditionName);
				defaultInterpolatedStringHandler.AppendLiteral("', because there's a precondition with that name.");
				log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			string conflictingName;
			if (Event.PreconditionAliases.TryGetValue(alias, out conflictingName))
			{
				IGameLogger log3 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(103, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event precondition alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for precondition '");
				defaultInterpolatedStringHandler.AppendFormatted(preconditionName);
				defaultInterpolatedStringHandler.AppendLiteral("', because that's already an alias for '");
				defaultInterpolatedStringHandler.AppendFormatted(conflictingName);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log3.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			if (!Event.Preconditions.ContainsKey(preconditionName))
			{
				IGameLogger log4 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(101, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't register event precondition alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' for precondition '");
				defaultInterpolatedStringHandler.AppendFormatted(preconditionName);
				defaultInterpolatedStringHandler.AppendLiteral("', because there's no such precondition.");
				log4.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			Event.PreconditionAliases[alias] = preconditionName;
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x0002A7E4 File Offset: 0x000289E4
		private static void SetupEventCommandsIfNeeded()
		{
			if (Event.Commands.Count == 0)
			{
				MethodInfo[] methods = typeof(Event.DefaultCommands).GetMethods(BindingFlags.Static | BindingFlags.Public);
				foreach (MethodInfo method in methods)
				{
					EventCommandDelegate command = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), method);
					Event.Commands.Add(method.Name, command);
					Event.CommandNames.Add(method.Name);
				}
				foreach (MethodInfo method2 in methods)
				{
					OtherNamesAttribute attribute = method2.GetCustomAttribute<OtherNamesAttribute>();
					if (attribute != null)
					{
						string[] aliases = attribute.Aliases;
						for (int j = 0; j < aliases.Length; j++)
						{
							Event.RegisterCommandAlias(aliases[j], method2.Name);
						}
					}
				}
			}
			if (Event.Preconditions.Count == 0)
			{
				MethodInfo[] methods2 = typeof(Preconditions).GetMethods(BindingFlags.Static | BindingFlags.Public);
				foreach (MethodInfo method3 in methods2)
				{
					EventPreconditionDelegate preconditionDelegate = (EventPreconditionDelegate)Delegate.CreateDelegate(typeof(EventPreconditionDelegate), method3);
					Event.Preconditions[method3.Name] = preconditionDelegate;
				}
				foreach (MethodInfo method4 in methods2)
				{
					OtherNamesAttribute attribute2 = method4.GetCustomAttribute<OtherNamesAttribute>();
					if (attribute2 != null)
					{
						string[] aliases = attribute2.Aliases;
						for (int j = 0; j < aliases.Length; j++)
						{
							Event.RegisterPreconditionAlias(aliases[j], method4.Name);
						}
					}
				}
			}
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0002A964 File Offset: 0x00028B64
		public static bool TryGetPreconditionHandler(string key, out EventPreconditionDelegate handler)
		{
			Event.SetupEventCommandsIfNeeded();
			string aliasTarget;
			if (Event.PreconditionAliases.TryGetValue(key, out aliasTarget))
			{
				key = aliasTarget;
			}
			return Event.Preconditions.TryGetValue(key, out handler);
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x0002A994 File Offset: 0x00028B94
		public static bool CheckPrecondition(GameLocation location, string eventId, string precondition)
		{
			string[] preconditionSplit = ArgUtility.SplitBySpaceQuoteAware(precondition);
			string key = preconditionSplit[0];
			bool match = true;
			if (key.StartsWith('!'))
			{
				key = key.Substring(1);
				match = false;
			}
			EventPreconditionDelegate handler;
			if (!Event.TryGetPreconditionHandler(key, out handler))
			{
				Game1.log.Warn("Unknown precondition for event " + eventId + ": " + precondition);
				return false;
			}
			bool result;
			try
			{
				result = (handler(location, eventId, preconditionSplit) == match);
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed checking precondition '");
				defaultInterpolatedStringHandler.AppendFormatted(precondition);
				defaultInterpolatedStringHandler.AppendLiteral("' for event ");
				defaultInterpolatedStringHandler.AppendFormatted(eventId);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				result = false;
			}
			return result;
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x0002AA64 File Offset: 0x00028C64
		public static bool TryGetEventCommandHandler(string key, out EventCommandDelegate handler)
		{
			string aliasTarget;
			if (Event.CommandAliases.TryGetValue(key, out aliasTarget))
			{
				key = aliasTarget;
			}
			return Event.Commands.TryGetValue(key, out handler);
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0002AA90 File Offset: 0x00028C90
		public virtual void tryEventCommand(GameLocation location, GameTime time, string[] args)
		{
			string commandName = ArgUtility.Get(args, 0, null, true);
			if (string.IsNullOrWhiteSpace(commandName))
			{
				this.LogCommandErrorAndSkip(args, "can't run an empty or null command", false);
				return;
			}
			EventCommandDelegate command;
			if (!Event.TryGetEventCommandHandler(commandName, out command))
			{
				this.LogCommandErrorAndSkip(args, "unknown command '" + commandName + "'", false);
				return;
			}
			try
			{
				EventContext context = new EventContext(this, location, time, args);
				command(this, args, context);
			}
			catch (Exception e)
			{
				this.LogErrorAndHalt(e);
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x060006F8 RID: 1784 RVA: 0x0002AB10 File Offset: 0x00028D10
		public string FestivalName
		{
			get
			{
				string name;
				if (!this.TryGetFestivalDataForYear("name", out name))
				{
					return "";
				}
				return name;
			}
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x060006F9 RID: 1785 RVA: 0x0002AB33 File Offset: 0x00028D33
		// (set) Token: 0x060006FA RID: 1786 RVA: 0x0002AB3B File Offset: 0x00028D3B
		public bool playerControlSequence
		{
			get
			{
				return this._playerControlSequence;
			}
			set
			{
				if (this._playerControlSequence != value)
				{
					this._playerControlSequence = value;
					if (!this._playerControlSequence)
					{
						this.OnPlayerControlSequenceEnd(this.playerControlSequenceID);
					}
				}
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x060006FB RID: 1787 RVA: 0x0002AB61 File Offset: 0x00028D61
		public Farmer farmer
		{
			get
			{
				if (this.farmerActors.Count <= 0)
				{
					return Game1.player;
				}
				return this.farmerActors[0];
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060006FC RID: 1788 RVA: 0x0002AB83 File Offset: 0x00028D83
		public Texture2D festivalTexture
		{
			get
			{
				if (this._festivalTexture == null)
				{
					this._festivalTexture = this.festivalContent.Load<Texture2D>("Maps\\Festivals");
				}
				return this._festivalTexture;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x0002ABA9 File Offset: 0x00028DA9
		// (set) Token: 0x060006FE RID: 1790 RVA: 0x0002ABB1 File Offset: 0x00028DB1
		public int CurrentCommand
		{
			get
			{
				return this.currentCommand;
			}
			set
			{
				this.currentCommand = value;
			}
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0002ABBA File Offset: 0x00028DBA
		public Event(string eventString, Farmer farmerActor = null) : this(eventString, null, "-1", farmerActor)
		{
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0002ABCC File Offset: 0x00028DCC
		public Event(string eventString, string fromAssetName, string eventID, Farmer farmerActor = null) : this()
		{
			this.fromAssetName = fromAssetName;
			this.id = eventID;
			this.eventCommands = Event.ParseCommands(eventString, farmerActor);
			this.actorPositionsAfterMove = new Dictionary<string, Vector3>();
			this.previousAmbientLight = Game1.ambientLight;
			if (farmerActor != null)
			{
				this.farmerActors.Add(farmerActor);
			}
			this.farmer.canOnlyWalk = true;
			this.farmer.showNotCarrying();
			this.drawTool = false;
			if (eventID == "-2")
			{
				this.isWedding = true;
			}
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x0002AC54 File Offset: 0x00028E54
		public Event()
		{
			Event.SetupEventCommandsIfNeeded();
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x0002AD10 File Offset: 0x00028F10
		~Event()
		{
			this.notifyDone();
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x0002AD3C File Offset: 0x00028F3C
		public static void OnNewDay()
		{
			LocalizedContentManager festivalReadContentLoader = Event.FestivalReadContentLoader;
			if (festivalReadContentLoader == null)
			{
				return;
			}
			festivalReadContentLoader.Unload();
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x0002AD50 File Offset: 0x00028F50
		public static bool tryToLoadFestivalData(string festival, out string assetName, out Dictionary<string, string> data, out string locationName, out int startTime, out int endTime)
		{
			assetName = "Data\\Festivals\\" + festival;
			data = null;
			locationName = null;
			startTime = 0;
			endTime = 0;
			if (Event.invalidFestivals.Contains(festival))
			{
				return false;
			}
			if (Event.FestivalReadContentLoader == null)
			{
				Event.FestivalReadContentLoader = Game1.content.CreateTemporary();
			}
			try
			{
				if (!Event.FestivalReadContentLoader.DoesAssetExist<Dictionary<string, string>>(assetName))
				{
					Event.invalidFestivals.Add(festival);
					return false;
				}
				data = Event.FestivalReadContentLoader.Load<Dictionary<string, string>>(assetName);
			}
			catch
			{
				Event.invalidFestivals.Add(festival);
				return false;
			}
			string rawConditions;
			if (!data.TryGetValue("conditions", out rawConditions))
			{
				Game1.log.Error("Festival '" + festival + "' doesn't have the required 'conditions' data field.", null);
				return false;
			}
			string[] fields = LegacyShims.SplitAndTrim(rawConditions, '/', StringSplitOptions.None);
			string error;
			string rawTimeSpan;
			if (!ArgUtility.TryGet(fields, 0, out locationName, out error, false, "locationName") || !ArgUtility.TryGet(fields, 1, out rawTimeSpan, out error, false, "string rawTimeSpan"))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Festival '");
				defaultInterpolatedStringHandler.AppendFormatted(festival);
				defaultInterpolatedStringHandler.AppendLiteral("' has preconditions '");
				defaultInterpolatedStringHandler.AppendFormatted(rawConditions);
				defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			string[] timeParts = ArgUtility.SplitBySpace(rawTimeSpan);
			if (!ArgUtility.TryGetInt(timeParts, 0, out startTime, out error, "startTime") || !ArgUtility.TryGetInt(timeParts, 1, out endTime, out error, "endTime"))
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Festival '");
				defaultInterpolatedStringHandler.AppendFormatted(festival);
				defaultInterpolatedStringHandler.AppendLiteral("' has preconditions '");
				defaultInterpolatedStringHandler.AppendFormatted(rawConditions);
				defaultInterpolatedStringHandler.AppendLiteral("' with time range '");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", timeParts));
				defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			return true;
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x0002AF74 File Offset: 0x00029174
		public static bool tryToLoadFestival(string festival, out Event ev)
		{
			ev = null;
			string dataAssetName;
			Dictionary<string, string> data;
			string locationName;
			int startTime;
			int endTime;
			if (!Event.tryToLoadFestivalData(festival, out dataAssetName, out data, out locationName, out startTime, out endTime))
			{
				return false;
			}
			if (locationName != Game1.currentLocation.Name || Game1.timeOfDay < startTime || Game1.timeOfDay >= endTime)
			{
				return false;
			}
			ev = new Event
			{
				id = "festival_" + festival,
				isFestival = true,
				festivalDataAssetName = dataAssetName,
				festivalData = data,
				actorPositionsAfterMove = new Dictionary<string, Vector3>(),
				previousAmbientLight = Game1.ambientLight
			};
			ev.festivalData["file"] = festival;
			string rawSetUp;
			if (!ev.TryGetFestivalDataForYear("set-up", out rawSetUp))
			{
				Game1.log.Error("Festival " + ev.id + " doesn't have the required 'set-up' data field.", null);
			}
			ev.eventCommands = Event.ParseCommands(rawSetUp, ev.farmer);
			Game1.player.festivalScore = 0;
			Game1.setRichPresence("festival", festival);
			return true;
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x0002B070 File Offset: 0x00029270
		public bool TryGetFestivalDialogueForYear(NPC npc, string key, out Dialogue dialogue)
		{
			string text;
			string actualKey;
			if (this.TryGetFestivalDataForYear(key, out text, out actualKey))
			{
				dialogue = new Dialogue(npc, this.festivalDataAssetName + ":" + actualKey, text);
				return true;
			}
			dialogue = null;
			return false;
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x0002B0AC File Offset: 0x000292AC
		public bool TryGetFestivalDataForYear(string key, out string data, out string actualKey)
		{
			if (this.festivalData == null)
			{
				data = null;
				actualKey = null;
				return false;
			}
			int years = 1;
			for (;;)
			{
				Dictionary<string, string> dictionary = this.festivalData;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted(key);
				defaultInterpolatedStringHandler.AppendLiteral("_y");
				defaultInterpolatedStringHandler.AppendFormatted<int>(years + 1);
				if (!dictionary.ContainsKey(defaultInterpolatedStringHandler.ToStringAndClear()))
				{
					break;
				}
				years++;
			}
			int selected_year = Game1.year % years;
			if (selected_year == 0)
			{
				selected_year = years;
			}
			string text;
			if (selected_year <= 1)
			{
				text = key;
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted(key);
				defaultInterpolatedStringHandler.AppendLiteral("_y");
				defaultInterpolatedStringHandler.AppendFormatted<int>(selected_year);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			actualKey = text;
			if (this.festivalData.TryGetValue(actualKey, out data))
			{
				return true;
			}
			actualKey = null;
			data = null;
			return false;
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x0002B168 File Offset: 0x00029368
		public bool TryGetFestivalDataForYear(string key, out string data)
		{
			string actualKey;
			return this.TryGetFestivalDataForYear(key, out data, out actualKey);
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x0002B17F File Offset: 0x0002937F
		public void setExitLocation(Warp warp)
		{
			this.setExitLocation(warp.TargetName, warp.TargetX, warp.TargetY);
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x0002B199 File Offset: 0x00029399
		public void setExitLocation(string location, int x, int y)
		{
			if (string.IsNullOrEmpty(Game1.player.locationBeforeForcedEvent.Value))
			{
				this.exitLocation = Game1.getLocationRequest(location, false);
				Game1.player.positionBeforeEvent = new Vector2((float)x, (float)y);
			}
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x0002B1D1 File Offset: 0x000293D1
		public void endBehaviors(GameLocation location = null)
		{
			this.endBehaviors(LegacyShims.EmptyArray<string>(), location ?? Game1.currentLocation);
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x0002B1E8 File Offset: 0x000293E8
		public void endBehaviors(string[] args, GameLocation location)
		{
			if (Game1.getMusicTrackName(MusicContext.Default).Contains(Game1.currentSeason) && ArgUtility.Get(this.eventCommands, 0, null, true) != "continue")
			{
				Game1.stopMusicTrack(MusicContext.Default);
			}
			string text = ArgUtility.Get(args, 1, null, true);
			if (text != null)
			{
				switch (text.Length)
				{
				case 3:
				{
					char c = text[0];
					if (c != 'L')
					{
						if (c == 'b')
						{
							if (text == "bed")
							{
								Game1.player.Position = Game1.player.mostRecentBed + new Vector2(0f, 64f);
							}
						}
					}
					else if (text == "Leo")
					{
						if (!this.isMemory)
						{
							Game1.addMailForTomorrow("leoMoved", true, true);
							Game1.player.team.requestLeoMove.Fire();
						}
					}
					break;
				}
				case 5:
					if (text == "Maru1")
					{
						NPC npc3 = Game1.getCharacterFromName("Demetrius", true, false) ?? this.getActorByName("Demetrius", false);
						if (npc3 != null)
						{
							npc3.setNewDialogue("Strings\\StringsFromCSFiles:Event.cs.1018", false, false);
						}
						NPC npc4 = Game1.getCharacterFromName("Maru", true, false) ?? this.getActorByName("Maru", false);
						if (npc4 != null)
						{
							npc4.setNewDialogue("Strings\\StringsFromCSFiles:Event.cs.1020", false, false);
						}
						this.setExitLocation(location.GetFirstPlayerWarp());
						Game1.fadeScreenToBlack();
						Game1.eventOver = true;
						this.CurrentCommand += 2;
					}
					break;
				case 6:
					if (text == "newDay")
					{
						Game1.player.faceDirection(2);
						this.setExitLocation(Game1.player.homeLocation.Value, (int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
						if (!Game1.IsMultiplayer)
						{
							this.exitLocation.OnWarp += delegate()
							{
								Game1.NewDay(0f);
								Game1.player.currentLocation.lastTouchActionLocation = new Vector2((float)((int)Game1.player.mostRecentBed.X / 64), (float)((int)Game1.player.mostRecentBed.Y / 64));
							};
						}
						Game1.player.completelyStopAnimatingOrDoingAction();
						if (Game1.player.bathingClothes.Value)
						{
							Game1.player.changeOutOfSwimSuit();
						}
						Game1.player.swimming.Value = false;
						Game1.player.CanMove = false;
						Game1.changeMusicTrack("none", false, MusicContext.Default);
					}
					break;
				case 7:
				{
					char c = text[1];
					if (c != 'a')
					{
						if (c != 'e')
						{
							if (c == 'r')
							{
								if (text == "credits")
								{
									Game1.debrisWeather.Clear();
									Game1.isDebrisWeather = false;
									Game1.changeMusicTrack("wedding", false, MusicContext.Event);
									Game1.gameMode = 10;
									this.CurrentCommand += 2;
								}
							}
						}
						else if (text == "wedding")
						{
							Game1.RequireCharacter("Lewis", true).CurrentDialogue.Push(new Dialogue(Game1.getCharacterFromName("Lewis", true, false), "Strings\\StringsFromCSFiles:Event.cs.1025", false));
							FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
							Point porch = homeOfFarmer.getPorchStandingSpot();
							if (homeOfFarmer is Cabin)
							{
								this.setExitLocation("Farm", porch.X + 1, porch.Y);
							}
							else
							{
								this.setExitLocation("Farm", porch.X - 1, porch.Y);
							}
							if (Game1.IsMasterGame)
							{
								NPC spouse = Game1.getCharacterFromName(this.farmer.spouse, true, false);
								if (spouse != null)
								{
									spouse.ClearSchedule();
									spouse.ignoreScheduleToday = true;
									spouse.shouldPlaySpousePatioAnimation.Value = false;
									spouse.controller = null;
									spouse.temporaryController = null;
									spouse.currentMarriageDialogue.Clear();
									Game1.warpCharacter(spouse, "Farm", Utility.getHomeOfFarmer(this.farmer).getPorchStandingSpot());
									spouse.faceDirection(2);
									if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + spouse.Name + "_AfterWedding", true) != null)
									{
										spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", spouse.Name + "_AfterWedding", false, Array.Empty<string>());
									}
									else
									{
										spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "Game1.cs.2782", false, Array.Empty<string>());
									}
								}
							}
						}
					}
					else if (text == "warpOut")
					{
						this.setExitLocation(location.GetFirstPlayerWarp());
						Game1.eventOver = true;
						this.CurrentCommand += 2;
						Game1.screenGlowHold = false;
					}
					break;
				}
				case 8:
				{
					char c = text[0];
					if (c != 'd')
					{
						if (c == 'p')
						{
							if (text == "position")
							{
								Vector2 position;
								string error;
								if (!ArgUtility.TryGetVector2(args, 2, out position, out error, true, "Vector2 position"))
								{
									this.LogCommandError(args, error, false);
								}
								else if (string.IsNullOrEmpty(Game1.player.locationBeforeForcedEvent.Value))
								{
									Game1.player.positionBeforeEvent = position;
								}
							}
						}
					}
					else if (text == "dialogue")
					{
						string npcName;
						string error2;
						string dialogue;
						if (!ArgUtility.TryGet(args, 2, out npcName, out error2, false, "string npcName") || !ArgUtility.TryGet(args, 3, out dialogue, out error2, true, "string dialogue"))
						{
							this.LogCommandError(args, error2, false);
						}
						else
						{
							NPC i = Game1.getCharacterFromName(npcName, true, false);
							if (i == null)
							{
								this.LogCommandError(args, "NPC '" + npcName + "' not found", false);
							}
							else
							{
								i.shouldSayMarriageDialogue.Value = false;
								i.currentMarriageDialogue.Clear();
								i.CurrentDialogue.Clear();
								i.CurrentDialogue.Push(new Dialogue(i, null, dialogue));
							}
						}
					}
					break;
				}
				case 9:
				{
					char c = text[0];
					if (c != 'b')
					{
						if (c == 'i')
						{
							if (text == "invisible")
							{
								string npcName2;
								string error3;
								if (!ArgUtility.TryGet(args, 2, out npcName2, out error3, false, "string npcName"))
								{
									this.LogCommandError(args, error3, false);
								}
								else if (!this.isMemory)
								{
									NPC npc = Game1.getCharacterFromName(npcName2, true, false);
									if (npc == null)
									{
										this.LogCommandError(args, "NPC '" + npcName2 + "' not found", false);
									}
									else
									{
										npc.IsInvisible = true;
										npc.daysUntilNotInvisible = 1;
									}
								}
							}
						}
					}
					else if (text == "beginGame")
					{
						Game1.gameMode = 3;
						this.setExitLocation("FarmHouse", 9, 9);
						Game1.NewDay(1000f);
						this.exitEvent();
						Game1.eventFinished();
						return;
					}
					break;
				}
				case 12:
				{
					char c = text[0];
					if (c != 'i')
					{
						if (c == 't')
						{
							if (text == "tunnelDepart")
							{
								if (Game1.player.hasOrWillReceiveMail("seenBoatJourney"))
								{
									Game1.warpFarmer("IslandSouth", 21, 43, 0);
								}
							}
						}
					}
					else if (text == "islandDepart")
					{
						Game1.player.orientationBeforeEvent = 2;
						string whereIsTodaysFest = Game1.whereIsTodaysFest;
						if (!(whereIsTodaysFest == "Beach"))
						{
							if (!(whereIsTodaysFest == "Town"))
							{
								this.setExitLocation("BoatTunnel", 6, 9);
							}
							else
							{
								Game1.player.orientationBeforeEvent = 3;
								this.setExitLocation("BusStop", 43, 23);
							}
						}
						else
						{
							Game1.player.orientationBeforeEvent = 0;
							this.setExitLocation("Town", 54, 109);
						}
						GameLocation left_location = Game1.currentLocation;
						this.exitLocation.OnLoad += delegate()
						{
							foreach (NPC npc5 in this.actors)
							{
								npc5.shouldShadowBeOffset = true;
								npc5.drawOffset.Y = 0f;
							}
							foreach (Farmer farmer in this.farmerActors)
							{
								farmer.shouldShadowBeOffset = true;
								farmer.drawOffset.Y = 0f;
							}
							Game1.player.drawOffset = Vector2.Zero;
							Game1.player.shouldShadowBeOffset = false;
							IslandSouth islandSouth = left_location as IslandSouth;
							if (islandSouth != null)
							{
								islandSouth.ResetBoat();
							}
						};
					}
					break;
				}
				case 13:
					if (text == "qiSummitCheat")
					{
						Game1.playSound("death", null);
						Game1.player.health = -1;
						Game1.player.position.X = -99999f;
						Game1.background = null;
						Game1.viewport.X = -999999;
						Game1.viewport.Y = -999999;
						Game1.viewportHold = 6000;
						Game1.eventOver = true;
						this.CurrentCommand += 2;
						Game1.screenGlowHold = false;
						Game1.screenGlowOnce(Color.Black, true, 1f, 1f);
					}
					break;
				case 15:
					if (text == "dialogueWarpOut")
					{
						string npcName3;
						string error4;
						string dialogue2;
						if (!ArgUtility.TryGet(args, 2, out npcName3, out error4, false, "string npcName") || !ArgUtility.TryGet(args, 3, out dialogue2, out error4, true, "string dialogue"))
						{
							this.LogCommandError(args, error4, false);
						}
						else
						{
							this.setExitLocation(location.GetFirstPlayerWarp());
							NPC j = Game1.getCharacterFromName(npcName3, true, false);
							if (j == null)
							{
								this.LogCommandError(args, "NPC '" + npcName3 + "' not found", false);
							}
							else
							{
								j.CurrentDialogue.Clear();
								j.CurrentDialogue.Push(new Dialogue(j, null, dialogue2));
								Game1.eventOver = true;
								this.CurrentCommand += 2;
								Game1.screenGlowHold = false;
							}
						}
					}
					break;
				case 16:
					if (text == "invisibleWarpOut")
					{
						string npcName4;
						string error5;
						if (!ArgUtility.TryGet(args, 2, out npcName4, out error5, false, "string npcName"))
						{
							this.LogCommandError(args, error5, false);
						}
						else
						{
							NPC npc2 = Game1.getCharacterFromName(npcName4, true, false);
							if (npc2 == null)
							{
								this.LogCommandError(args, "NPC '" + npcName4 + "' not found", false);
							}
							else
							{
								npc2.IsInvisible = true;
								npc2.daysUntilNotInvisible = 1;
								this.setExitLocation(location.GetFirstPlayerWarp());
								Game1.fadeScreenToBlack();
								Game1.eventOver = true;
								this.CurrentCommand += 2;
								Game1.screenGlowHold = false;
							}
						}
					}
					break;
				}
			}
			this.exitEvent();
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x0002BC14 File Offset: 0x00029E14
		public void exitEvent()
		{
			this.eventFinished = true;
			if (!string.IsNullOrEmpty(this.id) && this.id != "-1")
			{
				if (this.markEventSeen)
				{
					Game1.player.eventsSeen.Add(this.id);
				}
				Game1.eventsSeenSinceLastLocationChange.Add(this.id);
			}
			this.notifyDone();
			Game1.stopMusicTrack(MusicContext.Event);
			this.StopTrackedSounds();
			if (this.id == "1039573")
			{
				Game1.addMail("addedParrotBoy", true, true);
				Game1.player.team.requestAddCharacterEvent.Fire("Leo");
			}
			Game1.player.ignoreCollisions = false;
			Game1.player.canOnlyWalk = false;
			Game1.nonWarpFade = true;
			if (!Game1.fadeIn || Game1.fadeToBlackAlpha >= 1f)
			{
				Game1.fadeScreenToBlack();
			}
			Game1.eventOver = true;
			Game1.fadeToBlack = true;
			Game1.setBGColor(5, 3, 4);
			this.CurrentCommand += 2;
			Game1.screenGlowHold = false;
			if (this.isFestival)
			{
				Game1.timeOfDayAfterFade = 2200;
				if (this.festivalData != null && (this.isSpecificFestival("summer28") || this.isSpecificFestival("fall27")))
				{
					Game1.timeOfDayAfterFade = 2400;
				}
				int timePass = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, Game1.timeOfDayAfterFade);
				if (Game1.IsMasterGame)
				{
					Point house_entry = Game1.getFarm().GetMainFarmHouseEntry();
					this.setExitLocation("Farm", house_entry.X, house_entry.Y);
				}
				else
				{
					Point porchSpot = Utility.getHomeOfFarmer(Game1.player).getPorchStandingSpot();
					this.setExitLocation("Farm", porchSpot.X, porchSpot.Y);
				}
				Game1.player.toolOverrideFunction = null;
				this.isFestival = false;
				foreach (NPC i in this.actors)
				{
					if (i != null)
					{
						this.resetDialogueIfNecessary(i);
					}
				}
				if (Game1.IsMasterGame)
				{
					foreach (NPC j in Utility.getAllVillagers())
					{
						if (j.getSpouse() != null)
						{
							Farmer spouse_farmer = j.getSpouse();
							if (spouse_farmer.isMarriedOrRoommates())
							{
								j.controller = null;
								j.temporaryController = null;
								FarmHouse home_location = Utility.getHomeOfFarmer(spouse_farmer);
								j.Halt();
								Game1.warpCharacter(j, home_location, Utility.PointToVector2(home_location.getSpouseBedSpot(spouse_farmer.spouse)));
								if (home_location.GetSpouseBed() != null)
								{
									FarmHouse.spouseSleepEndFunction(j, Utility.getHomeOfFarmer(spouse_farmer));
								}
								j.ignoreScheduleToday = true;
								if (Game1.timeOfDayAfterFade >= 1800)
								{
									j.currentMarriageDialogue.Clear();
									j.checkForMarriageDialogue(1800, Utility.getHomeOfFarmer(spouse_farmer));
									continue;
								}
								if (Game1.timeOfDayAfterFade >= 1100)
								{
									j.currentMarriageDialogue.Clear();
									j.checkForMarriageDialogue(1100, Utility.getHomeOfFarmer(spouse_farmer));
									continue;
								}
								continue;
							}
						}
						if (j.currentLocation != null && j.defaultMap.Value != null)
						{
							j.doingEndOfRouteAnimation.Value = false;
							j.nextEndOfRouteMessage = null;
							j.endOfRouteMessage.Value = null;
							j.controller = null;
							j.temporaryController = null;
							j.Halt();
							Game1.warpCharacter(j, j.defaultMap.Value, j.DefaultPosition / 64f);
							j.ignoreScheduleToday = true;
						}
					}
				}
				foreach (GameLocation k in Game1.locations)
				{
					foreach (Vector2 position in new List<Vector2>(k.objects.Keys))
					{
						if (k.objects[position].minutesElapsed(timePass))
						{
							k.objects.Remove(position);
						}
					}
					Farm farm = k as Farm;
					if (farm != null)
					{
						farm.timeUpdate(timePass);
					}
				}
				Game1.player.freezePause = 1500;
				return;
			}
			Game1.player.forceCanMove();
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x0002C0C8 File Offset: 0x0002A2C8
		public void notifyDone()
		{
			if (this.id == "-1" || string.IsNullOrEmpty(this.id))
			{
				return;
			}
			if (!this.notifyWhenDone || this.notifyLocationName == null || !Game1.HasDedicatedHost || Game1.client == null)
			{
				return;
			}
			Game1.client.sendMessage(33, new object[]
			{
				0,
				this.notifyLocationName,
				this.notifyLocationIsStructure,
				this.id
			});
			this.notifyWhenDone = false;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0002C156 File Offset: 0x0002A356
		public void resetDialogueIfNecessary(NPC n)
		{
			if (!Game1.player.hasTalkedToFriendToday(n.Name))
			{
				n.resetCurrentDialogue();
				return;
			}
			Stack<Dialogue> currentDialogue = n.CurrentDialogue;
			if (currentDialogue == null)
			{
				return;
			}
			currentDialogue.Clear();
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x0002C184 File Offset: 0x0002A384
		public void incrementCommandAfterFade()
		{
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
			Game1.globalFade = false;
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x0002C1A7 File Offset: 0x0002A3A7
		public void cleanup()
		{
			Game1.ambientLight = this.previousAmbientLight;
			this._festivalTexture = null;
			this.festivalContent.Unload();
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x0002C1C8 File Offset: 0x0002A3C8
		private void changeLocation(string locationName, int x, int y, Action onComplete = null)
		{
			Event e = Game1.currentLocation.currentEvent;
			Game1.currentLocation.currentEvent = null;
			LocationRequest locationRequest = Game1.getLocationRequest(locationName, false);
			locationRequest.OnLoad += delegate()
			{
				if (!e.isFestival)
				{
					Game1.currentLocation.currentEvent = e;
				}
				this.temporaryLocation = null;
				Action onComplete2 = onComplete;
				if (onComplete2 != null)
				{
					onComplete2();
				}
				locationRequest.Location.ResetForEvent(this);
			};
			locationRequest.OnWarp += delegate()
			{
				this.farmer.currentLocation = Game1.currentLocation;
				if (e.isFestival)
				{
					Game1.currentLocation.currentEvent = e;
				}
			};
			Game1.warpFarmer(locationRequest, x, y, this.farmer.FacingDirection);
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0002C258 File Offset: 0x0002A458
		public void LogCommandError(string[] args, string error, bool willSkip = false)
		{
			IGameLogger log = Game1.log;
			string error2;
			if (!willSkip)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Event '");
				defaultInterpolatedStringHandler.AppendFormatted(this.id);
				defaultInterpolatedStringHandler.AppendLiteral("' has command '");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", args));
				defaultInterpolatedStringHandler.AppendLiteral("' which reported errors: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				error2 = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Event '");
				defaultInterpolatedStringHandler.AppendFormatted(this.id);
				defaultInterpolatedStringHandler.AppendLiteral("' has command '");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", args));
				defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				error2 = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			log.Error(error2, null);
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x0002C348 File Offset: 0x0002A548
		public void LogCommandErrorAndSkip(string[] args, string error, bool hideError = false)
		{
			if (!hideError)
			{
				this.LogCommandError(args, error, true);
			}
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x0002C374 File Offset: 0x0002A574
		public void LogErrorAndHalt(string error, Exception e = null)
		{
			string technicalError = "Error running event script " + this.fromAssetName + "#" + this.id;
			Game1.chatBox.addErrorMessage("Event script error: " + error);
			string commandText = this.GetCurrentCommand();
			if (commandText != null)
			{
				string str = technicalError;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
				defaultInterpolatedStringHandler.AppendLiteral(" on line #");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.CurrentCommand);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(commandText);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				technicalError = str + defaultInterpolatedStringHandler.ToStringAndClear();
				ChatBox chatBox = Game1.chatBox;
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 2);
				defaultInterpolatedStringHandler.AppendLiteral("On line #");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.CurrentCommand);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted(commandText);
				chatBox.addErrorMessage(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			Game1.log.Error(technicalError + ".", e);
			this.skipEvent();
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x0002C474 File Offset: 0x0002A674
		public void LogErrorAndHalt(Exception e)
		{
			this.LogErrorAndHalt(((e != null) ? e.Message : null) ?? "An unknown error occurred.", e);
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x0002C494 File Offset: 0x0002A694
		public static bool LogPreconditionError(GameLocation location, string eventId, string[] args, string error)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Event '");
			defaultInterpolatedStringHandler.AppendFormatted(eventId);
			defaultInterpolatedStringHandler.AppendLiteral("' in location '");
			defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
			defaultInterpolatedStringHandler.AppendLiteral("' has invalid event precondition '");
			defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", args));
			defaultInterpolatedStringHandler.AppendLiteral("': ");
			defaultInterpolatedStringHandler.AppendFormatted(error);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			return false;
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x0002C52C File Offset: 0x0002A72C
		public void Update(GameLocation location, GameTime time)
		{
			try
			{
				if (!this.eventFinished)
				{
					bool flag = this.CurrentCommand == 0 && !this.forked && !this.eventSwitched;
					if (flag)
					{
						this.InitializeEvent(location, time);
					}
					bool runNextCommand = this.UpdateBeforeNextCommand(location, time);
					if (!flag && runNextCommand)
					{
						this.CheckForNextCommand(location, time);
					}
				}
			}
			catch (Exception e)
			{
				this.LogErrorAndHalt(e);
			}
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x0002C5A0 File Offset: 0x0002A7A0
		protected void InitializeEvent(GameLocation location, GameTime time)
		{
			this.farmer.speed = 2;
			this.farmer.running = false;
			Game1.eventOver = false;
			string musicId;
			string error;
			string rawCameraPosition;
			string rawCharacterPositions;
			string rawOption;
			if (!ArgUtility.TryGet(this.eventCommands, 0, out musicId, out error, true, "string musicId") || !ArgUtility.TryGet(this.eventCommands, 1, out rawCameraPosition, out error, false, "string rawCameraPosition") || !ArgUtility.TryGet(this.eventCommands, 2, out rawCharacterPositions, out error, false, "string rawCharacterPositions") || !ArgUtility.TryGetOptional(this.eventCommands, 3, out rawOption, out error, null, true, "string rawOption"))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Event '");
				defaultInterpolatedStringHandler.AppendFormatted(this.id);
				defaultInterpolatedStringHandler.AppendLiteral("' has initial fields '");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join("/", this.eventCommands.Take(3)));
				defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				this.LogErrorAndHalt("event script is invalid", null);
				return;
			}
			if (string.IsNullOrWhiteSpace(musicId))
			{
				musicId = "none";
			}
			Point cameraPosition;
			if (rawCameraPosition != "follow")
			{
				string[] cameraParts = ArgUtility.SplitBySpace(rawCameraPosition);
				if (!ArgUtility.TryGetPoint(cameraParts, 0, out cameraPosition, out error, "cameraPosition"))
				{
					IGameLogger log2 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(118, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Event '");
					defaultInterpolatedStringHandler.AppendFormatted(this.id);
					defaultInterpolatedStringHandler.AppendLiteral("' has initial fields '");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("/", this.eventCommands.Take(3)));
					defaultInterpolatedStringHandler.AppendLiteral("' with camera value '");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", cameraParts));
					defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed (must be 'follow' or tile coordinates): ");
					defaultInterpolatedStringHandler.AppendFormatted(error);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					this.LogErrorAndHalt("event script is invalid", null);
					return;
				}
			}
			else
			{
				cameraPosition = new Point(-1000, -1000);
			}
			if (rawOption == "ignoreEventTileOffset")
			{
				this.ignoreTileOffsets = true;
			}
			if ((musicId != "none" || !Game1.isRaining) && musicId != "continue" && !musicId.Contains("pause"))
			{
				Game1.changeMusicTrack(musicId, false, MusicContext.Event);
			}
			if (location is Farm && cameraPosition.X >= -1000 && this.id != "-2" && !this.ignoreTileOffsets)
			{
				Point p = Farm.getFrontDoorPositionForFarmer(this.farmer);
				p.X *= 64;
				p.Y *= 64;
				Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(p.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.graphics.GraphicsDevice.Viewport.Width)) : (p.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2));
				Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(p.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.graphics.GraphicsDevice.Viewport.Height)) : (p.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2));
			}
			else if (rawCameraPosition != "follow")
			{
				try
				{
					Game1.viewportFreeze = true;
					int centerX = this.OffsetTileX(cameraPosition.X) * 64 + 32;
					int centerY = this.OffsetTileY(cameraPosition.Y) * 64 + 32;
					if (centerX < 0)
					{
						Game1.viewport.X = centerX;
						Game1.viewport.Y = centerY;
					}
					else
					{
						Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(centerX - Game1.viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width)) : (centerX - Game1.viewport.Width / 2));
						Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(centerY - Game1.viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) : (centerY - Game1.viewport.Height / 2));
					}
					if (centerX > 0 && Game1.graphics.GraphicsDevice.Viewport.Width > Game1.currentLocation.Map.DisplayWidth)
					{
						Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
					}
					if (centerY > 0 && Game1.graphics.GraphicsDevice.Viewport.Height > Game1.currentLocation.Map.DisplayHeight)
					{
						Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
					}
				}
				catch (Exception)
				{
					this.forked = true;
					return;
				}
			}
			this.setUpCharacters(rawCharacterPositions, location);
			this.trySpecialSetUp(location);
			this.populateWalkLocationsList();
			this.CurrentCommand = 3;
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x0002CB88 File Offset: 0x0002AD88
		protected bool UpdateBeforeNextCommand(GameLocation location, GameTime time)
		{
			if (this.skipped || Game1.farmEvent != null)
			{
				return false;
			}
			foreach (NPC i in this.actors)
			{
				i.update(time, Game1.currentLocation);
				if (i.Sprite.CurrentAnimation != null)
				{
					i.Sprite.animateOnce(time);
				}
			}
			TemporaryAnimatedSpriteList temporaryAnimatedSpriteList = this.aboveMapSprites;
			if (temporaryAnimatedSpriteList != null)
			{
				temporaryAnimatedSpriteList.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			}
			if (this.underwaterSprites != null)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.underwaterSprites)
				{
					temporaryAnimatedSprite.update(time);
				}
			}
			if (!this.playerControlSequence)
			{
				this.farmer.setRunning(false, false);
			}
			if (this.npcControllers != null)
			{
				for (int j = this.npcControllers.Count - 1; j >= 0; j--)
				{
					this.npcControllers[j].puppet.isCharging = !this.isFestival;
					if (this.npcControllers[j].update(time, location, this.npcControllers))
					{
						this.npcControllers.RemoveAt(j);
					}
				}
			}
			if (this.isFestival)
			{
				this.festivalUpdate(time);
			}
			if (this.temporaryLocation != null && !Game1.currentLocation.Equals(this.temporaryLocation))
			{
				this.temporaryLocation.updateEvenIfFarmerIsntHere(time, true);
			}
			if (!Game1.fadeToBlack || this.actorPositionsAfterMove.Count > 0 || this.CurrentCommand > 3 || this.forked)
			{
				if (this.eventCommands.Length <= this.CurrentCommand)
				{
					return false;
				}
				if (this.viewportTarget != Vector3.Zero)
				{
					int playerSpeed = this.farmer.speed;
					this.farmer.speed = (int)this.viewportTarget.X;
					int oldX = Game1.viewport.X;
					Game1.viewport.X = Game1.viewport.X + (int)this.viewportTarget.X;
					if (oldX > 0 && Game1.viewport.X <= 0 && location.IsOutdoors)
					{
						Game1.viewport.X = 0;
						this.viewportTarget.X = 0f;
					}
					else if (oldX < location.map.DisplayWidth - Game1.viewport.Width && Game1.viewport.X >= location.Map.DisplayWidth - Game1.viewport.Width)
					{
						Game1.viewport.X = location.Map.DisplayWidth - Game1.viewport.Width;
						this.viewportTarget.X = 0f;
					}
					if (this.viewportTarget.X != 0f)
					{
						Game1.updateRainDropPositionForPlayerMovement((this.viewportTarget.X < 0f) ? 3 : 1, Math.Abs(this.viewportTarget.X + (float)((this.farmer.isMoving() && this.farmer.FacingDirection == 3) ? (-(float)this.farmer.speed) : ((this.farmer.isMoving() && this.farmer.FacingDirection == 1) ? this.farmer.speed : 0))));
					}
					int oldY = Game1.viewport.Y;
					Game1.viewport.Y = Game1.viewport.Y + (int)this.viewportTarget.Y;
					if (oldY > 0 && Game1.viewport.Y <= 0 && location.IsOutdoors)
					{
						Game1.viewport.Y = 0;
						this.viewportTarget.Y = 0f;
					}
					else if (oldY < location.map.DisplayHeight - Game1.viewport.Height && Game1.viewport.Y >= location.Map.DisplayHeight - Game1.viewport.Height)
					{
						Game1.viewport.Y = location.Map.DisplayHeight - Game1.viewport.Height;
						this.viewportTarget.Y = 0f;
					}
					this.farmer.speed = (int)this.viewportTarget.Y;
					if (this.viewportTarget.Y != 0f)
					{
						Game1.updateRainDropPositionForPlayerMovement((this.viewportTarget.Y < 0f) ? 0 : 2, Math.Abs(this.viewportTarget.Y - (float)((this.farmer.isMoving() && this.farmer.FacingDirection == 0) ? (-(float)this.farmer.speed) : ((this.farmer.isMoving() && this.farmer.FacingDirection == 2) ? this.farmer.speed : 0))));
					}
					this.farmer.speed = playerSpeed;
					this.viewportTarget.Z = this.viewportTarget.Z - (float)time.ElapsedGameTime.Milliseconds;
					if (this.viewportTarget.Z <= 0f)
					{
						this.viewportTarget = Vector3.Zero;
					}
				}
				if (this.actorPositionsAfterMove.Count > 0)
				{
					foreach (string s in this.actorPositionsAfterMove.Keys.ToArray<string>())
					{
						Microsoft.Xna.Framework.Rectangle targetTile = new Microsoft.Xna.Framework.Rectangle((int)this.actorPositionsAfterMove[s].X * 64, (int)this.actorPositionsAfterMove[s].Y * 64, 64, 64);
						targetTile.Inflate(-4, 0);
						NPC actor = this.getActorByName(s, false);
						if (actor != null)
						{
							Microsoft.Xna.Framework.Rectangle bounds = actor.GetBoundingBox();
							if (bounds.Width > 64)
							{
								targetTile.Inflate(4, 0);
								targetTile.Width = bounds.Width + 4;
								targetTile.Height = bounds.Height + 4;
								targetTile.X += 8;
								targetTile.Y += 16;
							}
						}
						int farmerNumber;
						if (this.IsFarmerActorId(s, out farmerNumber))
						{
							Farmer f = this.GetFarmerActor(farmerNumber);
							if (f != null)
							{
								Microsoft.Xna.Framework.Rectangle bounds2 = f.GetBoundingBox();
								float moveSpeed = f.getMovementSpeed();
								if (targetTile.Contains(bounds2) && (((float)(bounds2.Y - targetTile.Top) <= 16f + moveSpeed && f.FacingDirection != 2) || ((float)(targetTile.Bottom - bounds2.Bottom) <= 16f + moveSpeed && f.FacingDirection == 2)))
								{
									f.showNotCarrying();
									f.Halt();
									f.faceDirection((int)this.actorPositionsAfterMove[s].Z);
									f.FarmerSprite.StopAnimation();
									f.Halt();
									this.actorPositionsAfterMove.Remove(s);
								}
								else if (f != null)
								{
									f.canOnlyWalk = false;
									f.setRunning(false, true);
									f.canOnlyWalk = true;
									f.lastPosition = this.farmer.Position;
									f.MovePosition(time, Game1.viewport, location);
								}
							}
						}
						else
						{
							foreach (NPC k in this.actors)
							{
								Microsoft.Xna.Framework.Rectangle bounds3 = k.GetBoundingBox();
								if (k.Name.Equals(s) && targetTile.Contains(bounds3) && bounds3.Y - targetTile.Top <= 16)
								{
									k.Halt();
									k.faceDirection((int)this.actorPositionsAfterMove[s].Z);
									this.actorPositionsAfterMove.Remove(s);
									break;
								}
								if (k.Name.Equals(s))
								{
									if (k is Monster)
									{
										k.MovePosition(time, Game1.viewport, location);
										break;
									}
									k.MovePosition(time, Game1.viewport, null);
									break;
								}
							}
						}
					}
					if (this.actorPositionsAfterMove.Count == 0)
					{
						if (this.continueAfterMove)
						{
							this.continueAfterMove = false;
						}
						else
						{
							int l = this.CurrentCommand;
							this.CurrentCommand = l + 1;
						}
					}
					if (!this.continueAfterMove)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x0002D414 File Offset: 0x0002B614
		protected void CheckForNextCommand(GameLocation location, GameTime time)
		{
			string[] args = ArgUtility.SplitBySpaceQuoteAware(this.eventCommands[Math.Min(this.eventCommands.Length - 1, this.CurrentCommand)]);
			string text = ArgUtility.Get(args, 0, null, true);
			bool flag = text != null && text.StartsWith("--");
			if (this.temporaryLocation != null && !Game1.currentLocation.Equals(this.temporaryLocation))
			{
				this.temporaryLocation.updateEvenIfFarmerIsntHere(time, true);
			}
			if (flag)
			{
				int num = this.CurrentCommand;
				this.CurrentCommand = num + 1;
				return;
			}
			this.tryEventCommand(location, time, args);
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x0002D4A0 File Offset: 0x0002B6A0
		public string GetCurrentCommand()
		{
			return ArgUtility.Get(this.eventCommands, this.currentCommand, null, true);
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x0002D4B5 File Offset: 0x0002B6B5
		public void ReplaceCurrentCommand(string command)
		{
			if (ArgUtility.HasIndex<string>(this.eventCommands, this.currentCommand))
			{
				this.eventCommands[this.currentCommand] = command;
			}
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x0002D4D8 File Offset: 0x0002B6D8
		public void ReplaceAllCommands(params string[] commands)
		{
			this.eventCommands = commands;
			this.CurrentCommand = 0;
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x0002D4E8 File Offset: 0x0002B6E8
		public void InsertNextCommand(string command)
		{
			int index = this.currentCommand + 1;
			List<string> commands = this.eventCommands.ToList<string>();
			if (index <= commands.Count)
			{
				commands.Insert(index, command);
			}
			else
			{
				commands.Add(command);
			}
			this.eventCommands = commands.ToArray();
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x0002D530 File Offset: 0x0002B730
		public void TrackSound(ICue cue)
		{
			if (cue == null)
			{
				return;
			}
			List<ICue> sounds;
			if (!this.CustomSounds.TryGetValue(cue.Name, out sounds))
			{
				sounds = (this.CustomSounds[cue.Name] = new List<ICue>());
			}
			sounds.Add(cue);
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x0002D578 File Offset: 0x0002B778
		public void StopTrackedSound(string cueId, bool immediate)
		{
			List<ICue> sounds;
			if (cueId != null && this.CustomSounds.TryGetValue(cueId, out sounds))
			{
				foreach (ICue cue in sounds)
				{
					cue.Stop(immediate ? AudioStopOptions.Immediate : AudioStopOptions.AsAuthored);
				}
				if (immediate)
				{
					this.CustomSounds.Remove(cueId);
				}
			}
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x0002D5F0 File Offset: 0x0002B7F0
		public void StopTrackedSounds()
		{
			foreach (List<ICue> list in this.CustomSounds.Values)
			{
				foreach (ICue cue in list)
				{
					cue.Stop(AudioStopOptions.Immediate);
				}
			}
			this.CustomSounds.Clear();
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x0002D680 File Offset: 0x0002B880
		public bool isTileWalkedOn(int x, int y)
		{
			return this.characterWalkLocations.Contains(new Vector2((float)x, (float)y));
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x0002D698 File Offset: 0x0002B898
		private void populateWalkLocationsList()
		{
			this.characterWalkLocations.Add(this.farmer.Tile);
			foreach (NPC i in this.actors)
			{
				this.characterWalkLocations.Add(i.Tile);
			}
			for (int j = 2; j < this.eventCommands.Length; j++)
			{
				string[] args = ArgUtility.SplitBySpace(this.eventCommands[j]);
				if (!(ArgUtility.Get(args, 0, null, true) != "move") && (!(ArgUtility.Get(args, 1, null, true) == "false") || args.Length != 2))
				{
					string actorName;
					string error;
					Point position;
					if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetPoint(args, 2, out position, out error, "Point position"))
					{
						this.LogCommandError(args, error, false);
					}
					else
					{
						Character character = this.IsCurrentFarmerActorId(actorName) ? this.farmer : this.getActorByName(actorName, false);
						if (character != null)
						{
							Vector2 pos = character.Tile;
							for (int x = 0; x < Math.Abs(position.X); x++)
							{
								pos.X += (float)Math.Sign(position.X);
								this.characterWalkLocations.Add(pos);
							}
							for (int y = 0; y < Math.Abs(position.Y); y++)
							{
								pos.Y += (float)Math.Sign(position.Y);
								this.characterWalkLocations.Add(pos);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x0002D84C File Offset: 0x0002BA4C
		public NPC getActorByName(string name, bool legacyReplaceUnderscores = false)
		{
			bool flag;
			return this.getActorByName(name, out flag, legacyReplaceUnderscores);
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x0002D864 File Offset: 0x0002BA64
		public NPC getActorByName(string name, out bool isOptionalNpc, bool legacyReplaceUnderscores = false)
		{
			isOptionalNpc = (((name != null) ? new bool?(name.EndsWith('?')) : null) ?? false);
			if (isOptionalNpc)
			{
				name = name.Substring(0, name.Length - 1);
			}
			if (name != null)
			{
				if (name == "spouse")
				{
					name = this.farmer.spouse;
				}
				foreach (NPC i in this.actors)
				{
					if (i.Name == name)
					{
						return i;
					}
				}
				if (legacyReplaceUnderscores)
				{
					string newName = name.Replace('_', ' ');
					if (newName != name)
					{
						foreach (NPC j in this.actors)
						{
							if (j.Name == newName)
							{
								return j;
							}
						}
					}
				}
				return null;
			}
			return null;
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x0002D99C File Offset: 0x0002BB9C
		private void addActor(string name, int x, int y, int facingDirection, GameLocation location)
		{
			bool isOptionalNpc;
			NPC duplicate = this.getActorByName(name, out isOptionalNpc, false);
			if (duplicate != null)
			{
				duplicate.Position = new Vector2((float)(x * 64), (float)(y * 64));
				duplicate.FacingDirection = facingDirection;
				return;
			}
			if (isOptionalNpc)
			{
				name = name.Substring(0, name.Length - 1);
				CharacterData data;
				if (!NPC.TryGetData(name, out data) || !GameStateQuery.CheckConditions(data.UnlockConditions, null, null, null, null, null, null))
				{
					return;
				}
			}
			NPC i;
			try
			{
				string spriteName = NPC.getTextureNameForCharacter(name);
				Texture2D portrait = null;
				try
				{
					portrait = Game1.content.Load<Texture2D>("Portraits\\" + spriteName);
				}
				catch (Exception)
				{
				}
				int height = (name.Contains("Dwarf") || name.Equals("Krobus")) ? 96 : 128;
				i = new NPC(new AnimatedSprite("Characters\\" + spriteName, 0, 16, height / 4), new Vector2((float)(x * 64), (float)(y * 64)), location.Name, facingDirection, name, portrait, true);
				i.EventActor = true;
				if (this.isFestival)
				{
					try
					{
						Dialogue dialogue;
						if (this.TryGetFestivalDialogueForYear(i, i.Name, out dialogue))
						{
							i.setNewDialogue(dialogue, false, false);
						}
					}
					catch (Exception)
					{
					}
				}
				if (i.name.Equals("MrQi"))
				{
					i.displayName = Game1.content.LoadString("Strings\\NPCNames:MisterQi");
				}
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Event '");
				defaultInterpolatedStringHandler.AppendFormatted(this.id);
				defaultInterpolatedStringHandler.AppendLiteral("' has character '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be added.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				return;
			}
			i.EventActor = true;
			this.actors.Add(i);
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x0002DB7C File Offset: 0x0002BD7C
		public Farmer GetFarmerActor(int farmerNumber)
		{
			Farmer player = (farmerNumber < 1) ? this.farmer : Utility.getFarmerFromFarmerNumber(farmerNumber);
			if (player == null)
			{
				return null;
			}
			foreach (Farmer actor in this.farmerActors)
			{
				if (actor.UniqueMultiplayerID == player.UniqueMultiplayerID)
				{
					return actor;
				}
			}
			return player;
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x0002DBF8 File Offset: 0x0002BDF8
		public bool IsCurrentFarmerActorId(string actor)
		{
			int farmerNumber;
			return this.IsFarmerActorId(actor, out farmerNumber) && this.IsCurrentFarmerActorId(farmerNumber);
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x0002DC19 File Offset: 0x0002BE19
		public bool IsCurrentFarmerActorId(int farmerNumber)
		{
			return farmerNumber < 1 || farmerNumber == Utility.getFarmerNumberFromFarmer(Game1.player);
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x0002DC30 File Offset: 0x0002BE30
		public bool IsFarmerActorId(string actor, out int farmerNumber)
		{
			if (actor == null || !actor.StartsWith("farmer"))
			{
				farmerNumber = -1;
				return false;
			}
			if (actor.Length == "farmer".Length)
			{
				farmerNumber = -1;
				return true;
			}
			return int.TryParse(actor.Substring("farmer".Length), out farmerNumber);
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x0002DC80 File Offset: 0x0002BE80
		public Character getCharacterByName(string name)
		{
			int farmerNumber;
			if (this.IsFarmerActorId(name, out farmerNumber))
			{
				return this.GetFarmerActor(farmerNumber);
			}
			foreach (NPC i in this.actors)
			{
				if (i.Name.Equals(name))
				{
					return i;
				}
			}
			return null;
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x0002DCF4 File Offset: 0x0002BEF4
		public Vector3 getPositionAfterMove(Character c, int xMove, int yMove, int facingDirection)
		{
			Vector2 tileLocation = c.Tile;
			return new Vector3(tileLocation.X + (float)xMove, tileLocation.Y + (float)yMove, (float)facingDirection);
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x0002DD24 File Offset: 0x0002BF24
		private void trySpecialSetUp(GameLocation location)
		{
			string text = this.id;
			if (text != null)
			{
				int length = text.Length;
				if (length != 6)
				{
					if (length == 7)
					{
						switch (text[6])
						{
						case '0':
						{
							if (!(text == "9333220"))
							{
								return;
							}
							FarmHouse house = location as FarmHouse;
							if (house != null && house.upgradeLevel == 1)
							{
								this.farmer.Position = new Vector2(1920f, 400f);
								this.getActorByName("Sebastian", false).setTilePosition(31, 6);
								return;
							}
							break;
						}
						case '1':
							if (!(text == "8675611"))
							{
								if (!(text == "3917601"))
								{
									return;
								}
								DecoratableLocation decoratableLocation = location as DecoratableLocation;
								if (decoratableLocation != null)
								{
									foreach (Furniture f in decoratableLocation.furniture)
									{
										if (f.furniture_type.Value == 14 && !location.IsTileBlockedBy(f.TileLocation + new Vector2(0f, 1f), CollisionMask.All, CollisionMask.All, false) && !location.IsTileBlockedBy(f.TileLocation + new Vector2(1f, 1f), CollisionMask.All, CollisionMask.All, false))
										{
											this.getActorByName("Emily", false).setTilePosition((int)f.TileLocation.X, (int)f.TileLocation.Y + 1);
											this.farmer.Position = new Vector2((f.TileLocation.X + 1f) * 64f, (f.tileLocation.Y + 1f) * 64f + 16f);
											f.isOn.Value = true;
											f.setFireplace(false, false);
											return;
										}
									}
									FarmHouse house2 = location as FarmHouse;
									if (house2 != null && house2.upgradeLevel == 1)
									{
										this.getActorByName("Emily", false).setTilePosition(4, 5);
										this.farmer.Position = new Vector2(320f, 336f);
										return;
									}
								}
							}
							else
							{
								FarmHouse house3 = location as FarmHouse;
								if (house3 != null && house3.upgradeLevel == 1)
								{
									this.getActorByName("Haley", false).setTilePosition(4, 5);
									this.farmer.Position = new Vector2(320f, 336f);
									return;
								}
							}
							break;
						case '2':
						{
							if (!(text == "3912132"))
							{
								return;
							}
							FarmHouse house4 = location as FarmHouse;
							if (house4 != null)
							{
								Point bed_spot = house4.GetPlayerBedSpot();
								bed_spot.X--;
								if (!location.CanItemBePlacedHere(Utility.PointToVector2(bed_spot) + new Vector2(-2f, 0f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
								{
									bed_spot.X++;
								}
								this.farmer.setTileLocation(Utility.PointToVector2(bed_spot));
								this.getActorByName("Elliott", false).setTileLocation(Utility.PointToVector2(bed_spot) + new Vector2(-2f, 0f));
								for (int i = 0; i < this.eventCommands.Length; i++)
								{
									if (this.eventCommands[i].StartsWith("makeInvisible"))
									{
										string[] args = ArgUtility.SplitBySpace(this.eventCommands[i]);
										Point tile;
										string error;
										if (!ArgUtility.TryGetPoint(args, 1, out tile, out error, "Point tile"))
										{
											this.LogCommandError(args, error, false);
										}
										else
										{
											args[1] = ((tile.X - 26 + bed_spot.X).ToString() ?? "");
											args[2] = ((tile.Y - 13 + bed_spot.Y).ToString() ?? "");
											if (location.getObjectAtTile(tile.X, tile.Y, false) == house4.GetPlayerBed())
											{
												this.eventCommands[i] = "makeInvisible -1000 -1000";
											}
											else
											{
												this.eventCommands[i] = string.Join(" ", args);
											}
										}
									}
								}
								return;
							}
							break;
						}
						case '3':
						{
							if (!(text == "4324303"))
							{
								return;
							}
							FarmHouse house5 = location as FarmHouse;
							if (house5 != null)
							{
								Point bed_spot2 = house5.GetPlayerBedSpot();
								bed_spot2.X--;
								this.farmer.Position = new Vector2((float)(bed_spot2.X * 64), (float)(bed_spot2.Y * 64 + 16));
								this.getActorByName("Penny", false).setTilePosition(bed_spot2.X - 1, bed_spot2.Y);
								Microsoft.Xna.Framework.Rectangle room = new Microsoft.Xna.Framework.Rectangle(23, 12, 10, 10);
								if (house5.upgradeLevel == 1)
								{
									room = new Microsoft.Xna.Framework.Rectangle(20, 3, 8, 7);
								}
								Point room_center = room.Center;
								if (!room.Contains(Game1.player.TilePoint))
								{
									List<string> commands = new List<string>(this.eventCommands);
									int command_index = 56;
									commands.Insert(command_index, "globalFade 0.03");
									command_index++;
									commands.Insert(command_index, "beginSimultaneousCommand");
									command_index++;
									commands.Insert(command_index, "viewport " + room_center.X.ToString() + " " + room_center.Y.ToString());
									command_index++;
									commands.Insert(command_index, "globalFadeToClear 0.03");
									command_index++;
									commands.Insert(command_index, "endSimultaneousCommand");
									command_index++;
									commands.Insert(command_index, "pause 2000");
									command_index++;
									commands.Insert(command_index, "globalFade 0.03");
									command_index++;
									commands.Insert(command_index, "beginSimultaneousCommand");
									command_index++;
									List<string> list = commands;
									int index = command_index;
									string str = "viewport ";
									Point tilePoint = Game1.player.TilePoint;
									string str2 = tilePoint.X.ToString();
									string str3 = " ";
									tilePoint = Game1.player.TilePoint;
									list.Insert(index, str + str2 + str3 + tilePoint.Y.ToString());
									command_index++;
									commands.Insert(command_index, "globalFadeToClear 0.03");
									command_index++;
									commands.Insert(command_index, "endSimultaneousCommand");
									command_index++;
									this.eventCommands = commands.ToArray();
								}
								for (int j = 0; j < this.eventCommands.Length; j++)
								{
									if (this.eventCommands[j].StartsWith("makeInvisible"))
									{
										string[] args2 = ArgUtility.SplitBySpace(this.eventCommands[j]);
										Point tile2;
										string error2;
										if (!ArgUtility.TryGetPoint(args2, 1, out tile2, out error2, "Point tile"))
										{
											this.LogCommandError(args2, error2, false);
										}
										else
										{
											args2[1] = ((tile2.X - 26 + bed_spot2.X).ToString() ?? "");
											args2[2] = ((tile2.Y - 13 + bed_spot2.Y).ToString() ?? "");
											if (location.getObjectAtTile(tile2.X, tile2.Y, false) == house5.GetPlayerBed())
											{
												this.eventCommands[j] = "makeInvisible -1000 -1000";
											}
											else
											{
												this.eventCommands[j] = string.Join(" ", args2);
											}
										}
									}
								}
								return;
							}
							break;
						}
						case '4':
						{
							if (!(text == "4325434"))
							{
								return;
							}
							FarmHouse house6 = location as FarmHouse;
							if (house6 != null && house6.upgradeLevel == 1)
							{
								this.farmer.Position = new Vector2(512f, 336f);
								this.getActorByName("Penny", false).setTilePosition(5, 5);
								return;
							}
							break;
						}
						case '5':
							break;
						case '6':
						{
							if (!(text == "3917666"))
							{
								return;
							}
							FarmHouse house7 = location as FarmHouse;
							if (house7 != null && house7.upgradeLevel == 1)
							{
								this.getActorByName("Maru", false).setTilePosition(4, 5);
								this.farmer.Position = new Vector2(320f, 336f);
							}
							break;
						}
						default:
							return;
						}
					}
				}
				else
				{
					if (!(text == "739330"))
					{
						return;
					}
					if (!Game1.player.friendshipData.ContainsKey("Willy"))
					{
						Game1.player.friendshipData.Add("Willy", new Friendship(0));
					}
					NPC willy = Game1.getCharacterFromName("Willy", true, false);
					Game1.player.NotifyQuests((Quest quest) => quest.OnNpcSocialized(willy, false), false);
					return;
				}
			}
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x0002E5CC File Offset: 0x0002C7CC
		private void setUpCharacters(string description, GameLocation location)
		{
			this.farmer.Halt();
			if (string.IsNullOrEmpty(Game1.player.locationBeforeForcedEvent.Value) && !this.isMemory)
			{
				Game1.player.positionBeforeEvent = Game1.player.Tile;
				Game1.player.orientationBeforeEvent = Game1.player.FacingDirection;
			}
			string[] args = ArgUtility.SplitBySpace(description);
			for (int i = 0; i < args.Length; i += 4)
			{
				string actorName;
				string error;
				Point tile;
				int direction;
				if (!ArgUtility.TryGet(args, i, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetPoint(args, i + 1, out tile, out error, "Point tile") || !ArgUtility.TryGetInt(args, i + 3, out direction, out error, "int direction"))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Event '");
					defaultInterpolatedStringHandler.AppendFormatted(this.id);
					defaultInterpolatedStringHandler.AppendLiteral("' has character positions '");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", args));
					defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
					defaultInterpolatedStringHandler.AppendFormatted(error);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				else
				{
					int farmerNumber;
					bool isFarmerId = this.IsFarmerActorId(actorName, out farmerNumber);
					bool isCurrentFarmer = isFarmerId && this.IsCurrentFarmerActorId(farmerNumber);
					if (tile.X == -1 && !isCurrentFarmer)
					{
						using (List<NPC>.Enumerator enumerator = location.characters.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								NPC j = enumerator.Current;
								if (j.Name == actorName)
								{
									this.actors.Add(j);
								}
							}
							goto IL_709;
						}
					}
					if (actorName != "farmer")
					{
						if (actorName == "otherFarmers")
						{
							int x = this.OffsetTileX(tile.X);
							int y = this.OffsetTileY(tile.Y);
							using (FarmerCollection.Enumerator enumerator2 = Game1.getOnlineFarmers().GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									Farmer f = enumerator2.Current;
									if (f.UniqueMultiplayerID != this.farmer.UniqueMultiplayerID)
									{
										Farmer fake = f.CreateFakeEventFarmer();
										fake.completelyStopAnimatingOrDoingAction();
										fake.hidden.Value = false;
										fake.faceDirection(direction);
										fake.setTileLocation(new Vector2((float)x, (float)y));
										fake.currentLocation = Game1.currentLocation;
										x++;
										this.farmerActors.Add(fake);
									}
								}
								goto IL_709;
							}
						}
						if (isFarmerId)
						{
							int x2 = this.OffsetTileX(tile.X);
							int y2 = this.OffsetTileY(tile.Y);
							Farmer f2 = this.GetFarmerActor(farmerNumber);
							if (f2 != null)
							{
								Farmer fake2 = f2.CreateFakeEventFarmer();
								fake2.completelyStopAnimatingOrDoingAction();
								fake2.hidden.Value = false;
								fake2.faceDirection(direction);
								fake2.setTileLocation(new Vector2((float)x2, (float)y2));
								fake2.currentLocation = Game1.currentLocation;
								fake2.isFakeEventActor = true;
								this.farmerActors.Add(fake2);
							}
						}
						else
						{
							string name;
							if (actorName == "spouse")
							{
								name = this.farmer.spouse;
							}
							else
							{
								name = actorName;
							}
							if (!(actorName == "cat"))
							{
								if (!(actorName == "dog"))
								{
									if (!(actorName == "pet"))
									{
										if (!(actorName == "golem"))
										{
											if (!(actorName == "Junimo"))
											{
												int xPos = this.OffsetTileX(tile.X);
												int yPos = this.OffsetTileY(tile.Y);
												int facingDir = direction;
												if (location is Farm && this.id != "-2" && !this.ignoreTileOffsets)
												{
													xPos = Farm.getFrontDoorPositionForFarmer(this.farmer).X;
													yPos = Farm.getFrontDoorPositionForFarmer(this.farmer).Y + 2;
													facingDir = 0;
												}
												this.addActor(name, xPos, yPos, facingDir, location);
											}
											else
											{
												this.actors.Add(new Junimo(this.OffsetPosition(new Vector2((float)(tile.X * 64), (float)(tile.Y * 64 - 32))), Game1.currentLocation.Name.Equals("AbandonedJojaMart") ? 6 : -1, false)
												{
													Name = "Junimo",
													EventActor = true
												});
											}
										}
										else
										{
											NPC golem = new NPC(new AnimatedSprite("Characters\\Monsters\\Wilderness Golem", 0, 16, 24), this.OffsetPosition(new Vector2((float)tile.X, (float)tile.Y) * 64f), 0, "Golem", null);
											golem.AllowDynamicAppearance = false;
											this.actors.Add(golem);
										}
									}
									else
									{
										Pet pet = new Pet(this.OffsetTileX(tile.X), this.OffsetTileY(tile.Y), Game1.player.whichPetBreed, Game1.player.whichPetType);
										pet.Name = "PetActor";
										PetData data;
										if (Pet.TryGetData(Game1.player.whichPetType, out data))
										{
											pet.Position = new Vector2(pet.Position.X + (float)data.EventOffset.X, pet.Position.Y + (float)data.EventOffset.Y);
										}
										this.actors.Add(pet);
									}
								}
								else
								{
									Pet dog = new Pet(this.OffsetTileX(tile.X), this.OffsetTileY(tile.Y), Game1.player.whichPetBreed, "Dog");
									dog.Name = "Dog";
									dog.position.X -= 42f;
									this.actors.Add(dog);
								}
							}
							else
							{
								Pet cat = new Pet(this.OffsetTileX(tile.X), this.OffsetTileY(tile.Y), Game1.player.whichPetBreed, "Cat");
								cat.Name = "Cat";
								cat.position.X -= 32f;
								this.actors.Add(cat);
							}
						}
					}
					else if (tile.X != -1)
					{
						this.farmer.position.X = this.OffsetPositionX((float)(tile.X * 64));
						this.farmer.position.Y = this.OffsetPositionY((float)(tile.Y * 64 + 16));
						this.farmer.faceDirection(direction);
						if (location is Farm && this.id != "-2" && !this.ignoreTileOffsets)
						{
							this.farmer.position.X = (float)(Farm.getFrontDoorPositionForFarmer(this.farmer).X * 64);
							this.farmer.position.Y = (float)((Farm.getFrontDoorPositionForFarmer(this.farmer).Y + 1) * 64);
							this.farmer.faceDirection(2);
						}
						this.farmer.FarmerSprite.StopAnimation();
					}
				}
				IL_709:;
			}
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x0002ED0C File Offset: 0x0002CF0C
		private void beakerSmashEndFunction(int extraInfo)
		{
			Game1.playSound("breakingGlass", null);
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.LightBlue, 10, false, 100f, 0, -1, -1f, -1, 0));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(400, 3008, 64, 64), 99999f, 2, 0, new Vector2(9f, 16f) * 64f, false, false, 0.01f, 0f, Color.LightBlue, 1f, 0f, 0f, 0f, false)
			{
				delayBeforeAnimationStart = 700
			});
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(9f, 16f) * 64f, Color.White * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				motion = new Vector2(0f, -1f)
			});
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x0002EE54 File Offset: 0x0002D054
		private void eggSmashEndFunction(int extraInfo)
		{
			Game1.playSound("slimedead", null);
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(177, 99999f, 9999, 0, new Vector2(6f, 5f) * 64f, false, false)
			{
				layerDepth = 1E-06f
			});
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x0002EF04 File Offset: 0x0002D104
		private void balloonInSky(int extraInfo)
		{
			TemporaryAnimatedSprite t = Game1.currentLocation.getTemporarySpriteByID(2);
			if (t != null)
			{
				t.motion = Vector2.Zero;
			}
			t = Game1.currentLocation.getTemporarySpriteByID(1);
			if (t != null)
			{
				t.motion = Vector2.Zero;
			}
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x0002EF48 File Offset: 0x0002D148
		private void marcelloBalloonLand(int extraInfo)
		{
			Game1.playSound("thudStep", null);
			Game1.playSound("dirtyHit", null);
			TemporaryAnimatedSprite t = Game1.currentLocation.getTemporarySpriteByID(2);
			if (t != null)
			{
				t.motion = Vector2.Zero;
			}
			t = Game1.currentLocation.getTemporarySpriteByID(3);
			if (t != null)
			{
				t.scaleChange = 0f;
			}
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(25f, 39f) + this.eventPositionTileOffset) * 64f + new Vector2(-32f, 32f), false, true, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(27f, 39f) + this.eventPositionTileOffset) * 64f + new Vector2(0f, 48f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false)
			{
				delayBeforeAnimationStart = 300
			});
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x0002F0E8 File Offset: 0x0002D2E8
		private void samPreOllie(int extraInfo)
		{
			this.getActorByName("Sam", false).Sprite.currentFrame = 27;
			this.farmer.faceDirection(0);
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.xStopCoordinate = 1408;
			temporarySpriteByID.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.samOllie);
			temporarySpriteByID.motion = new Vector2(2f, 0f);
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x0002F15C File Offset: 0x0002D35C
		private void samOllie(int extraInfo)
		{
			Game1.playSound("crafting", null);
			this.getActorByName("Sam", false).Sprite.currentFrame = 26;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 1;
			temporarySpriteByID.motion.Y = -9f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 530f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.endFunction = new TemporaryAnimatedSprite.endBehavior(this.samGrind);
			temporarySpriteByID.destroyable = false;
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x0002F21C File Offset: 0x0002D41C
		private void samGrind(int extraInfo)
		{
			Game1.playSound("hammer", null);
			this.getActorByName("Sam", false).Sprite.currentFrame = 28;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 9999;
			temporarySpriteByID.motion.Y = 0f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 99999f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.xStopCoordinate = 1664;
			temporarySpriteByID.yStopCoordinate = -1;
			temporarySpriteByID.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.samDropOff);
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x0002F2EC File Offset: 0x0002D4EC
		private void samDropOff(int extraInfo)
		{
			NPC actorByName = this.getActorByName("Sam", false);
			actorByName.Sprite.currentFrame = 31;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 9999;
			temporarySpriteByID.totalNumberOfLoops = 0;
			temporarySpriteByID.motion.Y = 0f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 99999f;
			temporarySpriteByID.yStopCoordinate = 5760;
			temporarySpriteByID.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.samGround);
			temporarySpriteByID.endFunction = null;
			actorByName.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(29, 100),
				new FarmerSprite.AnimationFrame(30, 100),
				new FarmerSprite.AnimationFrame(31, 100),
				new FarmerSprite.AnimationFrame(32, 100)
			});
			actorByName.Sprite.loop = false;
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x0002F3F4 File Offset: 0x0002D5F4
		private void samGround(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			Game1.playSound("thudStep", null);
			temporarySpriteByID.attachedCharacter = null;
			temporarySpriteByID.reachedStopCoordinate = null;
			temporarySpriteByID.totalNumberOfLoops = -1;
			temporarySpriteByID.interval = 0f;
			temporarySpriteByID.destroyable = true;
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x0002F45C File Offset: 0x0002D65C
		private void catchFootball(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
			Game1.playSound("fishSlap", null);
			temporarySpriteByID.motion = new Vector2(2f, -8f);
			temporarySpriteByID.rotationChange = 0.1308997f;
			temporarySpriteByID.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.footballLand);
			temporarySpriteByID.yStopCoordinate = 1088;
			this.farmer.jump();
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x0002F4D4 File Offset: 0x0002D6D4
		private void footballLand(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
			Game1.playSound("sandyStep", null);
			temporarySpriteByID.motion = new Vector2(0f, 0f);
			temporarySpriteByID.rotationChange = 0f;
			temporarySpriteByID.reachedStopCoordinate = null;
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 999999f;
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x0002F54C File Offset: 0x0002D74C
		private void parrotSplat(int extraInfo)
		{
			Game1.playSound("drumkit0", null);
			DelayedAction.playSoundAfterDelay("drumkit5", 100, null, null, -1, false);
			Game1.playSound("slimeHit", null);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.aboveMapSprites)
			{
				temporaryAnimatedSprite.alpha = 0f;
			}
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), false, false, 0.02f, 0.01f, Color.White, 4f, 0f, 1.5707964f, 0.049087387f, false)
			{
				motion = new Vector2(2f, -2f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), false, false, 0.02f, 0.01f, Color.White, 4f, 0f, 0.7853982f, 0.049087387f, false)
			{
				motion = new Vector2(-2f, -1f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), false, false, 0.02f, 0.01f, Color.White, 4f, 0f, 3.1415927f, 0.049087387f, false)
			{
				motion = new Vector2(1f, 1f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), false, false, 0.02f, 0.01f, Color.White, 4f, 0f, 0f, 0.049087387f, false)
			{
				motion = new Vector2(-2f, -2f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(148, 165, 25, 23), 99999f, 1, 99999, new Vector2(1504f, 5568f), false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
			{
				id = 666
			});
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x0002F8B8 File Offset: 0x0002DAB8
		public virtual Vector2 OffsetPosition(Vector2 original)
		{
			return new Vector2(this.OffsetPositionX(original.X), this.OffsetPositionY(original.Y));
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x0002F8D7 File Offset: 0x0002DAD7
		public virtual Vector2 OffsetTile(Vector2 original)
		{
			return new Vector2((float)this.OffsetTileX((int)original.X), (float)this.OffsetTileY((int)original.Y));
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x0002F8FA File Offset: 0x0002DAFA
		public virtual float OffsetPositionX(float original)
		{
			if (original < 0f || this.ignoreTileOffsets)
			{
				return original;
			}
			return original + this.eventPositionTileOffset.X * 64f;
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x0002F921 File Offset: 0x0002DB21
		public virtual float OffsetPositionY(float original)
		{
			if (original < 0f || this.ignoreTileOffsets)
			{
				return original;
			}
			return original + this.eventPositionTileOffset.Y * 64f;
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x0002F948 File Offset: 0x0002DB48
		public virtual int OffsetTileX(int original)
		{
			if (original < 0 || this.ignoreTileOffsets)
			{
				return original;
			}
			return (int)((float)original + this.eventPositionTileOffset.X);
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x0002F967 File Offset: 0x0002DB67
		public virtual int OffsetTileY(int original)
		{
			if (original < 0 || this.ignoreTileOffsets)
			{
				return original;
			}
			return (int)((float)original + this.eventPositionTileOffset.Y);
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x0002F988 File Offset: 0x0002DB88
		private void addSpecificTemporarySprite(string key, GameLocation location, string[] args)
		{
			if (key != null)
			{
				switch (key.Length)
				{
				case 3:
					if (!(key == "wed"))
					{
						return;
					}
					this.aboveMapSprites = new TemporaryAnimatedSpriteList();
					Game1.flashAlpha = 1f;
					for (int i = 0; i < 150; i++)
					{
						Vector2 position = new Vector2((float)Game1.random.Next(Game1.viewport.Width - 128), (float)Game1.random.Next(Game1.viewport.Height));
						int scale = Game1.random.Next(2, 5);
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(424, 1266, 8, 8), 60f + (float)Game1.random.Next(-10, 10), 7, 999999, position, false, false, 0.99f, 0f, Color.White, (float)scale, 0f, 0f, 0f, false)
						{
							local = true,
							motion = new Vector2(0.1625f, -0.25f) * (float)scale
						});
					}
					Game1.changeMusicTrack("wedding", false, MusicContext.Event);
					Game1.musicPlayerVolume = 0f;
					return;
				case 4:
				case 21:
				case 22:
					break;
				case 5:
				{
					char c = key[0];
					if (c != 'h')
					{
						if (c != 'r')
						{
							if (c != 's')
							{
								return;
							}
							if (!(key == "samTV"))
							{
								return;
							}
							Texture2D tempTxture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 350, 25, 29),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(368f, 350f),
								interval = 5000f,
								totalNumberOfLoops = 99999,
								position = new Vector2(52f, 24f) * 64f + new Vector2(4f, -12f) * 4f,
								scale = 4f,
								layerDepth = 0.9f
							});
							return;
						}
						else
						{
							if (!(key == "robot"))
							{
								return;
							}
							TemporaryAnimatedSprite parent2 = new TemporaryAnimatedSprite(this.getActorByName("robot", false).Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(35, 42, 35, 42), 50f, 1, 9999, new Vector2(13f, 27f) * 64f - new Vector2(0f, 32f), false, false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								acceleration = new Vector2(0f, -0.01f),
								accelerationChange = new Vector2(0f, -0.0001f)
							};
							location.temporarySprites.Add(parent2);
							for (int j = 0; j < 420; j++)
							{
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 64, 320, 64, 64), new Vector2((float)Game1.random.Next(96), 136f), false, 0.01f, Color.White * 0.75f)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = j * 10,
									animationLength = 1,
									currentNumberOfLoops = 0,
									interval = 9999f,
									motion = new Vector2((float)(Game1.random.Next(-100, 100) / (j + 20)), 0.25f + (float)j / 100f),
									parentSprite = parent2
								});
							}
							return;
						}
					}
					else
					{
						if (!(key == "heart"))
						{
							return;
						}
						Vector2 tile;
						string error;
						if (!ArgUtility.TryGetVector2(args, 2, out tile, out error, true, "Vector2 tile"))
						{
							this.LogCommandError(args, error, false);
							return;
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, this.OffsetPosition(tile) * 64f + new Vector2(-16f, -16f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(0f, -0.5f),
							alphaFade = 0.01f
						});
						return;
					}
					break;
				}
				case 6:
				{
					if (!(key == "qiCave"))
					{
						return;
					}
					Texture2D tempTxt = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(415, 216, 96, 89),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(415f, 216f),
						interval = 999999f,
						totalNumberOfLoops = 99999,
						position = new Vector2(2f, 2f) * 64f + new Vector2(112f, 25f) * 4f,
						scale = 4f,
						layerDepth = 1E-07f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(370, 272, 107, 64),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(370f, 216f),
						interval = 999999f,
						totalNumberOfLoops = 99999,
						position = new Vector2(2f, 2f) * 64f + new Vector2(67f, 81f) * 4f,
						scale = 4f,
						layerDepth = 1.1E-07f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.objectSpriteSheet,
						sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16),
						sourceRectStartingPos = new Vector2((float)Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).X, (float)Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).Y),
						animationLength = 1,
						interval = 999999f,
						id = 803,
						totalNumberOfLoops = 99999,
						position = new Vector2(13f, 7f) * 64f + new Vector2(1f, 9f) * 4f,
						scale = 4f,
						layerDepth = 2.1E-06f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 100f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(8f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 90f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(5f, 7f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 120f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(7f, 10f) * 64f,
						scale = 4f,
						layerDepth = 1f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 80f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(15f, 7f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 100f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(12f, 11f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 105f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(16f, 10f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxt,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 85f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(3f, 9f) * 64f,
						scale = 4f
					});
					return;
				}
				case 7:
				{
					char c = key[1];
					if (c <= 'i')
					{
						if (c != 'a')
						{
							if (c != 'e')
							{
								if (c != 'i')
								{
									return;
								}
								if (!(key == "dickBag"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(528, 1435, 16, 16), 99999f, 1, 99999, new Vector2(48f, 7f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								return;
							}
							else
							{
								if (!(key == "wedding"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1196, 98, 54), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1250, 98, 25), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, 54f) * 4f + new Vector2(0f, -64f), false, false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 62f) * 64f, false, false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 62f) * 64f, false, false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 69f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 69f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								return;
							}
						}
						else
						{
							if (!(key == "jasGift"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(22f, 16f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999,
								paused = true,
								holdLastFrame = true
							});
							return;
						}
					}
					else if (c != 'o')
					{
						if (c != 'r')
						{
							if (c != 'u')
							{
								return;
							}
							if (!(key == "sunroom"))
							{
								return;
							}
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(304, 486, 24, 26),
								sourceRectStartingPos = new Vector2(304f, 486f),
								animationLength = 1,
								totalNumberOfLoops = 997,
								interval = 99999f,
								scale = 4f,
								position = new Vector2(4f, 8f) * 64f + new Vector2(8f, -8f) * 4f,
								layerDepth = 0.0512f,
								id = 996
							});
							location.addCritter(new Butterfly(location, location.getRandomTile(null), false, false, -1, false).setStayInbounds(true));
							while (Game1.random.NextBool())
							{
								location.addCritter(new Butterfly(location, location.getRandomTile(null), false, false, -1, false).setStayInbounds(true));
							}
							return;
						}
						else
						{
							if (!(key == "dropEgg"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite(176, 800f, 1, 0, new Vector2(6f, 4f) * 64f + new Vector2(0f, 32f), false, false)
							{
								rotationChange = 0.1308997f,
								motion = new Vector2(0f, -7f),
								acceleration = new Vector2(0f, 0.3f),
								endFunction = new TemporaryAnimatedSprite.endBehavior(this.eggSmashEndFunction),
								layerDepth = 1f
							});
							return;
						}
					}
					else
					{
						if (key == "JoshMom")
						{
							TemporaryAnimatedSprite parent3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(416, 1931, 58, 65), 750f, 2, 99999, new Vector2((float)(Game1.viewport.Width / 2), (float)Game1.viewport.Height), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								alpha = 0.6f,
								local = true,
								xPeriodic = true,
								xPeriodicLoopTime = 2000f,
								xPeriodicRange = 32f,
								motion = new Vector2(0f, -1.25f),
								initialPosition = new Vector2((float)(Game1.viewport.Width / 2), (float)Game1.viewport.Height)
							};
							location.temporarySprites.Add(parent3);
							for (int k = 0; k < 19; k++)
							{
								location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(516, 1916, 7, 10), 99999f, 1, 99999, new Vector2(64f, 32f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									alphaFade = 0.01f,
									local = true,
									motion = new Vector2(-1f, -1f),
									parentSprite = parent3,
									delayBeforeAnimationStart = (k + 1) * 1000
								});
							}
							return;
						}
						if (!(key == "joshDog"))
						{
							return;
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1916, 12, 20), 500f, 6, 9999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							id = 1
						});
						return;
					}
					break;
				}
				case 8:
				{
					char c = key[4];
					if (c <= 'T')
					{
						if (c != 'J')
						{
							if (c != 'S')
							{
								if (c != 'T')
								{
									return;
								}
								if (!(key == "leahTree"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(744, 999999f, 1, 0, new Vector2(42f, 8f) * 64f, false, false));
								return;
							}
							else
							{
								if (!(key == "leahShow"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(29f, 59f) * 64f - new Vector2(0f, 16f), false, false, 0.37750003f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(112, 656, 16, 64), 9999f, 1, 999, new Vector2(29f, 56f) * 64f, false, false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 59f) * 64f - new Vector2(0f, 16f), false, false, 0.37750003f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(128, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 58f) * 64f, false, false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(160, 656, 32, 64), 9999f, 1, 999, new Vector2(29f, 60f) * 64f, false, false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(34f, 63f) * 64f, false, false, 0.4031f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(113, 592, 16, 64), 100f, 4, 99999, new Vector2(34f, 60f) * 64f, false, false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								NPC l = new NPC(new AnimatedSprite(this.festivalContent, "Characters\\" + (this.farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(46f, 57f) * 64f, 2, "LeahEx", null);
								l.AllowDynamicAppearance = false;
								this.actors.Add(l);
								return;
							}
						}
						else
						{
							if (!(key == "frogJump"))
							{
								return;
							}
							TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(777);
							temporarySpriteByID.motion = new Vector2(-2f, 0f);
							temporarySpriteByID.animationLength = 4;
							temporarySpriteByID.interval = 150f;
							return;
						}
					}
					else if (c <= 'm')
					{
						if (c != 'e')
						{
							if (c != 'm')
							{
								return;
							}
							if (!(key == "golemDie"))
							{
								return;
							}
							location.temporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(40f, 11f) * 64f, Color.DarkGray, 10, false, 100f, 0, -1, -1f, -1, 0));
							Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, new Vector2(40f, 11f) * 64f, Color.LimeGreen, 10, false, 100f, 0, -1, -1f, -1, 0), location, 2, 64, 64);
							Texture2D tempTxture2 = Game1.temporaryContent.Load<Texture2D>("Characters\\Monsters\\Wilderness Golem");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture2,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(0f, 0f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(40f, 11f) * 64f + new Vector2(2f, -8f) * 4f,
								scale = 4f,
								layerDepth = 0.01f,
								rotation = 1.5707964f,
								motion = new Vector2(0f, 4f),
								yStopCoordinate = 832
							});
							return;
						}
						else
						{
							if (!(key == "umbrella"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1843, 27, 23), 80f, 3, 9999, new Vector2(12f, 39f) * 64f + new Vector2(-20f, -104f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						}
					}
					else if (c != 'o')
					{
						if (c != 'y')
						{
							return;
						}
						if (!(key == "WillyWad"))
						{
							return;
						}
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors2"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(192, 61, 32, 32),
							sourceRectStartingPos = new Vector2(192f, 61f),
							animationLength = 2,
							totalNumberOfLoops = 99999,
							interval = 400f,
							scale = 4f,
							position = new Vector2(50f, 23f) * 64f,
							layerDepth = 0.1536f,
							id = 996
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3328f, 1728f), Color.White, 10, false, 80f, 999999, -1, -1f, -1, 0));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3264f, 1792f), Color.White, 10, false, 70f, 999999, -1, -1f, -1, 0));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3392f, 1792f), Color.White, 10, false, 85f, 999999, -1, -1f, -1, 0));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(53f, 24f) * 64f, false, false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(54f, 23f) * 64f, false, false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
						return;
					}
					else
					{
						if (!(key == "parrots1"))
						{
							return;
						}
						this.aboveMapSprites = new TemporaryAnimatedSpriteList
						{
							new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width, 256f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(-3f, 0f),
								yPeriodic = true,
								yPeriodicLoopTime = 2000f,
								yPeriodicRange = 32f,
								delayBeforeAnimationStart = 0,
								local = true
							},
							new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width, 192f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(-3f, 0f),
								yPeriodic = true,
								yPeriodicLoopTime = 2000f,
								yPeriodicRange = 32f,
								delayBeforeAnimationStart = 600,
								local = true
							},
							new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width, 320f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(-3f, 0f),
								yPeriodic = true,
								yPeriodicLoopTime = 2000f,
								yPeriodicRange = 32f,
								delayBeforeAnimationStart = 1200,
								local = true
							}
						};
						return;
					}
					break;
				}
				case 9:
				{
					char c = key[5];
					if (c <= 'S')
					{
						switch (c)
						{
						case 'B':
							if (key == "shakeBush")
							{
								location.getTemporarySpriteByID(777).shakeIntensity = 1f;
								return;
							}
							if (!(key == "movieBush"))
							{
								return;
							}
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\bushes"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(65, 58, 30, 35),
								sourceRectStartingPos = new Vector2(65f, 58f),
								animationLength = 1,
								totalNumberOfLoops = 999,
								interval = 999f,
								scale = 4f,
								position = new Vector2(4f, 1f) * 64f + new Vector2(33f, 13f) * 4f,
								layerDepth = 0.99f,
								id = 777
							});
							return;
						case 'C':
							if (!(key == "pennyCook"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, false, 0f, Color.White)
							{
								layerDepth = 1f,
								animationLength = 6,
								interval = 75f,
								motion = new Vector2(0f, -0.5f)
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(16f, 0f), false, 0f, Color.White)
							{
								layerDepth = 0.1f,
								animationLength = 6,
								interval = 75f,
								motion = new Vector2(0f, -0.5f),
								delayBeforeAnimationStart = 500
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(-16f, 0f), false, 0f, Color.White)
							{
								layerDepth = 1f,
								animationLength = 6,
								interval = 75f,
								motion = new Vector2(0f, -0.5f),
								delayBeforeAnimationStart = 750
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, false, 0f, Color.White)
							{
								layerDepth = 0.1f,
								animationLength = 6,
								interval = 75f,
								motion = new Vector2(0f, -0.5f),
								delayBeforeAnimationStart = 1000
							});
							return;
						case 'D':
						case 'E':
							break;
						case 'F':
							if (!(key == "sauceFire"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.mouseCursors,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
								animationLength = 4,
								sourceRectStartingPos = new Vector2(276f, 1985f),
								interval = 100f,
								totalNumberOfLoops = 5,
								position = this.OffsetPosition(new Vector2(64f, 16f) * 64f + new Vector2(3f, -4f) * 4f),
								scale = 4f,
								layerDepth = 1f
							});
							this.aboveMapSprites = new TemporaryAnimatedSpriteList();
							for (int m = 0; m < 8; m++)
							{
								this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), this.OffsetPosition(new Vector2(64f, 16f) * 64f) + new Vector2((float)Game1.random.Next(-16, 32), 0f), false, 0.002f, Color.Gray)
								{
									alpha = 0.75f,
									motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
									interval = 99999f,
									layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
									scale = 3f,
									scaleChange = 0.01f,
									rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
									delayBeforeAnimationStart = m * 25
								});
							}
							return;
						case 'G':
							if (!(key == "sauceGood"))
							{
								return;
							}
							Utility.addSprinklesToLocation(location, this.OffsetTileX(64), this.OffsetTileY(16), 3, 1, 800, 200, Color.White, null, false);
							return;
						default:
							if (c != 'M')
							{
								if (c != 'S')
								{
									return;
								}
								if (!(key == "EmilySign"))
								{
									return;
								}
								this.aboveMapSprites = new TemporaryAnimatedSpriteList();
								for (int numRainbows = 0; numRainbows < 10; numRainbows++)
								{
									int iter = 0;
									int yPos = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height - 128);
									for (int xPos = Game1.graphics.GraphicsDevice.Viewport.Width; xPos >= -64; xPos -= 48)
									{
										this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2((float)xPos, (float)yPos), false, false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											delayBeforeAnimationStart = numRainbows * 600 + iter * 25,
											startSound = ((iter == 0) ? "dwoop" : null),
											local = true
										});
										iter++;
									}
								}
								return;
							}
							else
							{
								if (!(key == "pennyMess"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(739, 999999f, 1, 0, new Vector2(10f, 5f) * 64f, false, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(740, 999999f, 1, 0, new Vector2(15f, 5f) * 64f, false, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(741, 999999f, 1, 0, new Vector2(16f, 6f) * 64f, false, false));
								return;
							}
							break;
						}
					}
					else if (c <= 'a')
					{
						if (c != 'T')
						{
							if (c != 'a')
							{
								return;
							}
							if (!(key == "samSkate1"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0), 9999f, 1, 999, new Vector2(12f, 90f) * 64f, false, false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(4f, 0f),
								acceleration = new Vector2(-0.008f, 0f),
								xStopCoordinate = 1344,
								reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.samPreOllie),
								attachedCharacter = this.getActorByName("Sam", false),
								id = 92473
							});
							return;
						}
						else
						{
							if (!(key == "shakeTent"))
							{
								return;
							}
							location.getTemporarySpriteByID(999).shakeIntensity = 1f;
							return;
						}
					}
					else if (c != 't')
					{
						if (c != 'u')
						{
							return;
						}
						if (!(key == "abbyOuija"))
						{
							return;
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, new Vector2(6f, 9f) * 64f, false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false));
						return;
					}
					else
					{
						if (!(key == "joshSteak"))
						{
							return;
						}
						location.temporarySprites.Clear();
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							id = 1
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), 999f, 1, 9999, new Vector2(50f, 68f) * 64f + new Vector2(32f, -8f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
						return;
					}
					break;
				}
				case 10:
				{
					char c = key[6];
					if (c <= 'G')
					{
						if (c != 'B')
						{
							if (c != 'C')
							{
								if (c != 'G')
								{
									return;
								}
								if (key == "parrotGone")
								{
									location.removeTemporarySpritesWithID(666);
									return;
								}
								if (!(key == "secretGift"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), new Vector2(30f, 70f) * 64f + new Vector2(0f, -21f), false, 0f, Color.White)
								{
									animationLength = 1,
									interval = 999999f,
									id = 666,
									scale = 4f
								});
								return;
							}
							else
							{
								if (!(key == "junimoCage"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 19), 60f, 3, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), false, false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("junimoCage_1"),
									lightRadius = 1f,
									lightcolor = Color.Black,
									id = 1,
									shakeIntensity = 0f
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("junimoCage_2"),
									lightRadius = 0.5f,
									lightcolor = Color.Black,
									id = 1,
									xPeriodic = true,
									xPeriodicLoopTime = 2000f,
									xPeriodicRange = 24f,
									yPeriodic = true,
									yPeriodicLoopTime = 2000f,
									yPeriodicRange = 24f
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, -4f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("junimoCage_3"),
									lightRadius = 0.5f,
									lightcolor = Color.Black,
									id = 1,
									xPeriodic = true,
									xPeriodicLoopTime = 2000f,
									xPeriodicRange = -24f,
									yPeriodic = true,
									yPeriodicLoopTime = 2000f,
									yPeriodicRange = 24f,
									delayBeforeAnimationStart = 250
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, 52f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("junimoCage_3"),
									lightRadius = 0.5f,
									lightcolor = Color.Black,
									id = 1,
									xPeriodic = true,
									xPeriodicLoopTime = 2000f,
									xPeriodicRange = -24f,
									yPeriodic = true,
									yPeriodicLoopTime = 2000f,
									yPeriodicRange = 24f,
									delayBeforeAnimationStart = 450
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, 52f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("junimoCage_4"),
									lightRadius = 0.5f,
									lightcolor = Color.Black,
									id = 1,
									xPeriodic = true,
									xPeriodicLoopTime = 2000f,
									xPeriodicRange = 24f,
									yPeriodic = true,
									yPeriodicLoopTime = 2000f,
									yPeriodicRange = 24f,
									delayBeforeAnimationStart = 650
								});
								return;
							}
						}
						else
						{
							if (key == "arcaneBook")
							{
								for (int n = 0; n < 16; n++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(128f, 792f) + new Vector2((float)Game1.random.Next(32), (float)(Game1.random.Next(32) - n * 4)), false, 0f, Color.White)
									{
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 7,
										layerDepth = 1f,
										scale = 4f,
										alphaFade = 0.008f,
										motion = new Vector2(0f, -0.5f)
									});
								}
								this.aboveMapSprites = new TemporaryAnimatedSpriteList
								{
									new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 18), new Vector2(160f, 800f), false, 0f, Color.White)
									{
										interval = 25f,
										totalNumberOfLoops = 99999,
										animationLength = 3,
										layerDepth = 1f,
										scale = 1f,
										scaleChange = 1f,
										scaleChangeChange = -0.05f,
										alpha = 0.65f,
										alphaFade = 0.005f,
										motion = new Vector2(-8f, -8f),
										acceleration = new Vector2(0.4f, 0.4f)
									}
								};
								for (int i2 = 0; i2 < 16; i2++)
								{
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 12f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), 0f), false, 0.002f, Color.Gray)
									{
										alpha = 0.75f,
										motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
										interval = 99999f,
										layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
										scale = 3f,
										scaleChange = 0.01f,
										rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
										delayBeforeAnimationStart = i2 * 25
									});
								}
								location.setMapTile(2, 12, 2143, "Front", "untitled tile sheet", null, true);
								return;
							}
							if (!(key == "candleBoat"))
							{
								return;
							}
							this.showGroundObjects = false;
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(240, 112, 16, 32), 1000f, 2, 99999, new Vector2(22f, 36f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 1,
								lightId = this.GenerateLightSourceId("candleBoat"),
								lightRadius = 2f,
								lightcolor = Color.Black
							});
							return;
						}
					}
					else if (c <= 'S')
					{
						if (c != 'L')
						{
							if (c != 'S')
							{
								return;
							}
							if (!(key == "junimoShow"))
							{
								return;
							}
							Texture2D tempTxture3 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture3,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 350, 19, 14),
								animationLength = 6,
								sourceRectStartingPos = new Vector2(393f, 350f),
								interval = 90f,
								totalNumberOfLoops = 86,
								position = new Vector2(52f, 24f) * 64f + new Vector2(7f, -2f) * 4f,
								scale = 4f,
								layerDepth = 0.95f
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture3,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 364, 19, 14),
								animationLength = 4,
								sourceRectStartingPos = new Vector2(393f, 364f),
								interval = 90f,
								totalNumberOfLoops = 31,
								position = new Vector2(52f, 24f) * 64f + new Vector2(7f, -2f) * 4f,
								scale = 4f,
								layerDepth = 0.97f,
								delayBeforeAnimationStart = 11034
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture3,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 378, 19, 14),
								animationLength = 6,
								sourceRectStartingPos = new Vector2(393f, 378f),
								interval = 90f,
								totalNumberOfLoops = 21,
								position = new Vector2(52f, 24f) * 64f + new Vector2(7f, -2f) * 4f,
								scale = 4f,
								layerDepth = 1f,
								delayBeforeAnimationStart = 22069
							});
							return;
						}
						else
						{
							if (!(key == "abbyAtLake"))
							{
								return;
							}
							int lightIndex = 1;
							TemporaryAnimatedSpriteList temporarySprites = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(735, 999999f, 1, 0, new Vector2(48f, 30f) * 64f, false, false);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite.lightRadius = 2f;
							temporarySprites.Add(temporaryAnimatedSprite);
							TemporaryAnimatedSpriteList temporarySprites2 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite2.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite2.lightRadius = 0.2f;
							temporaryAnimatedSprite2.xPeriodic = true;
							temporaryAnimatedSprite2.yPeriodic = true;
							temporaryAnimatedSprite2.xPeriodicLoopTime = 2000f;
							temporaryAnimatedSprite2.yPeriodicLoopTime = 1600f;
							temporaryAnimatedSprite2.xPeriodicRange = 32f;
							temporaryAnimatedSprite2.yPeriodicRange = 21f;
							temporarySprites2.Add(temporaryAnimatedSprite2);
							TemporaryAnimatedSpriteList temporarySprites3 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite3 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite3.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite3.lightRadius = 0.2f;
							temporaryAnimatedSprite3.xPeriodic = true;
							temporaryAnimatedSprite3.yPeriodic = true;
							temporaryAnimatedSprite3.xPeriodicLoopTime = 1000f;
							temporaryAnimatedSprite3.yPeriodicLoopTime = 1600f;
							temporaryAnimatedSprite3.xPeriodicRange = 16f;
							temporaryAnimatedSprite3.yPeriodicRange = 21f;
							temporarySprites3.Add(temporaryAnimatedSprite3);
							TemporaryAnimatedSpriteList temporarySprites4 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite4 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite4.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite4.lightRadius = 0.2f;
							temporaryAnimatedSprite4.xPeriodic = true;
							temporaryAnimatedSprite4.yPeriodic = true;
							temporaryAnimatedSprite4.xPeriodicLoopTime = 2400f;
							temporaryAnimatedSprite4.yPeriodicLoopTime = 2800f;
							temporaryAnimatedSprite4.xPeriodicRange = 21f;
							temporaryAnimatedSprite4.yPeriodicRange = 32f;
							temporarySprites4.Add(temporaryAnimatedSprite4);
							TemporaryAnimatedSpriteList temporarySprites5 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite5 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite5.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite5.lightRadius = 0.2f;
							temporaryAnimatedSprite5.xPeriodic = true;
							temporaryAnimatedSprite5.yPeriodic = true;
							temporaryAnimatedSprite5.xPeriodicLoopTime = 2000f;
							temporaryAnimatedSprite5.yPeriodicLoopTime = 2400f;
							temporaryAnimatedSprite5.xPeriodicRange = 16f;
							temporaryAnimatedSprite5.yPeriodicRange = 16f;
							temporarySprites5.Add(temporaryAnimatedSprite5);
							TemporaryAnimatedSpriteList temporarySprites6 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite6 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(-32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite6.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite6.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite6.lightRadius = 0.2f;
							temporaryAnimatedSprite6.xPeriodic = true;
							temporaryAnimatedSprite6.yPeriodic = true;
							temporaryAnimatedSprite6.xPeriodicLoopTime = 2000f;
							temporaryAnimatedSprite6.yPeriodicLoopTime = 2600f;
							temporaryAnimatedSprite6.xPeriodicRange = 21f;
							temporaryAnimatedSprite6.yPeriodicRange = 48f;
							temporarySprites6.Add(temporaryAnimatedSprite6);
							TemporaryAnimatedSpriteList temporarySprites7 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite7 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite7.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite7.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite7.lightRadius = 0.2f;
							temporaryAnimatedSprite7.xPeriodic = true;
							temporaryAnimatedSprite7.yPeriodic = true;
							temporaryAnimatedSprite7.xPeriodicLoopTime = 2000f;
							temporaryAnimatedSprite7.yPeriodicLoopTime = 2600f;
							temporaryAnimatedSprite7.xPeriodicRange = 32f;
							temporaryAnimatedSprite7.yPeriodicRange = 21f;
							temporarySprites7.Add(temporaryAnimatedSprite7);
							TemporaryAnimatedSpriteList temporarySprites8 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite8 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite8.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite8.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite8.lightRadius = 0.2f;
							temporaryAnimatedSprite8.xPeriodic = true;
							temporaryAnimatedSprite8.yPeriodic = true;
							temporaryAnimatedSprite8.xPeriodicLoopTime = 4000f;
							temporaryAnimatedSprite8.yPeriodicLoopTime = 5000f;
							temporaryAnimatedSprite8.xPeriodicRange = 42f;
							temporaryAnimatedSprite8.yPeriodicRange = 32f;
							temporarySprites8.Add(temporaryAnimatedSprite8);
							TemporaryAnimatedSpriteList temporarySprites9 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite9 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(0f, -32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite9.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite9.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite9.lightRadius = 0.2f;
							temporaryAnimatedSprite9.xPeriodic = true;
							temporaryAnimatedSprite9.yPeriodic = true;
							temporaryAnimatedSprite9.xPeriodicLoopTime = 4000f;
							temporaryAnimatedSprite9.yPeriodicLoopTime = 5500f;
							temporaryAnimatedSprite9.xPeriodicRange = 32f;
							temporaryAnimatedSprite9.yPeriodicRange = 32f;
							temporarySprites9.Add(temporaryAnimatedSprite9);
							TemporaryAnimatedSpriteList temporarySprites10 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite10 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(-32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite10.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite10.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite10.lightRadius = 0.2f;
							temporaryAnimatedSprite10.xPeriodic = true;
							temporaryAnimatedSprite10.yPeriodic = true;
							temporaryAnimatedSprite10.xPeriodicLoopTime = 2400f;
							temporaryAnimatedSprite10.yPeriodicLoopTime = 3600f;
							temporaryAnimatedSprite10.xPeriodicRange = 32f;
							temporaryAnimatedSprite10.yPeriodicRange = 21f;
							temporarySprites10.Add(temporaryAnimatedSprite10);
							TemporaryAnimatedSpriteList temporarySprites11 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite11 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite11.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite11.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite11.lightRadius = 0.2f;
							temporaryAnimatedSprite11.xPeriodic = true;
							temporaryAnimatedSprite11.yPeriodic = true;
							temporaryAnimatedSprite11.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite11.yPeriodicLoopTime = 3600f;
							temporaryAnimatedSprite11.xPeriodicRange = 42f;
							temporaryAnimatedSprite11.yPeriodicRange = 51f;
							temporarySprites11.Add(temporaryAnimatedSprite11);
							TemporaryAnimatedSpriteList temporarySprites12 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite12 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite12.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite12.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite12.lightRadius = 0.2f;
							temporaryAnimatedSprite12.xPeriodic = true;
							temporaryAnimatedSprite12.yPeriodic = true;
							temporaryAnimatedSprite12.xPeriodicLoopTime = 4500f;
							temporaryAnimatedSprite12.yPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite12.xPeriodicRange = 21f;
							temporaryAnimatedSprite12.yPeriodicRange = 32f;
							temporarySprites12.Add(temporaryAnimatedSprite12);
							TemporaryAnimatedSpriteList temporarySprites13 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite13 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(0f, -32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite13.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite13.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite13.lightRadius = 0.2f;
							temporaryAnimatedSprite13.xPeriodic = true;
							temporaryAnimatedSprite13.yPeriodic = true;
							temporaryAnimatedSprite13.xPeriodicLoopTime = 5000f;
							temporaryAnimatedSprite13.yPeriodicLoopTime = 4500f;
							temporaryAnimatedSprite13.xPeriodicRange = 64f;
							temporaryAnimatedSprite13.yPeriodicRange = 48f;
							temporarySprites13.Add(temporaryAnimatedSprite13);
							TemporaryAnimatedSpriteList temporarySprites14 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite14 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(-32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite14.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite14.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite14.lightRadius = 0.2f;
							temporaryAnimatedSprite14.xPeriodic = true;
							temporaryAnimatedSprite14.yPeriodic = true;
							temporaryAnimatedSprite14.xPeriodicLoopTime = 2000f;
							temporaryAnimatedSprite14.yPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite14.xPeriodicRange = 32f;
							temporaryAnimatedSprite14.yPeriodicRange = 21f;
							temporarySprites14.Add(temporaryAnimatedSprite14);
							TemporaryAnimatedSpriteList temporarySprites15 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite15 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite15.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite15.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite15.lightRadius = 0.2f;
							temporaryAnimatedSprite15.xPeriodic = true;
							temporaryAnimatedSprite15.yPeriodic = true;
							temporaryAnimatedSprite15.xPeriodicLoopTime = 2900f;
							temporaryAnimatedSprite15.yPeriodicLoopTime = 3200f;
							temporaryAnimatedSprite15.xPeriodicRange = 21f;
							temporaryAnimatedSprite15.yPeriodicRange = 32f;
							temporarySprites15.Add(temporaryAnimatedSprite15);
							TemporaryAnimatedSpriteList temporarySprites16 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite16 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite16.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite16.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite16.lightRadius = 0.2f;
							temporaryAnimatedSprite16.xPeriodic = true;
							temporaryAnimatedSprite16.yPeriodic = true;
							temporaryAnimatedSprite16.xPeriodicLoopTime = 4200f;
							temporaryAnimatedSprite16.yPeriodicLoopTime = 3300f;
							temporaryAnimatedSprite16.xPeriodicRange = 16f;
							temporaryAnimatedSprite16.yPeriodicRange = 32f;
							temporarySprites16.Add(temporaryAnimatedSprite16);
							TemporaryAnimatedSpriteList temporarySprites17 = location.TemporarySprites;
							TemporaryAnimatedSprite temporaryAnimatedSprite17 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(0f, -32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite17.lightcolor = Color.Orange;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler.AppendLiteral("abbyAtLake_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex++);
							temporaryAnimatedSprite17.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite17.lightRadius = 0.2f;
							temporaryAnimatedSprite17.xPeriodic = true;
							temporaryAnimatedSprite17.yPeriodic = true;
							temporaryAnimatedSprite17.xPeriodicLoopTime = 5100f;
							temporaryAnimatedSprite17.yPeriodicLoopTime = 4000f;
							temporaryAnimatedSprite17.xPeriodicRange = 32f;
							temporaryAnimatedSprite17.yPeriodicRange = 16f;
							temporarySprites17.Add(temporaryAnimatedSprite17);
						}
					}
					else if (c != 'W')
					{
						switch (c)
						{
						case 'a':
							if (!(key == "maruBeaker"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 1380f, 1, 0, new Vector2(9f, 14f) * 64f + new Vector2(0f, 32f), false, false)
							{
								rotationChange = 0.1308997f,
								motion = new Vector2(0f, -7f),
								acceleration = new Vector2(0f, 0.2f),
								endFunction = new TemporaryAnimatedSprite.endBehavior(this.beakerSmashEndFunction),
								layerDepth = 1f
							});
							return;
						case 'b':
							if (!(key == "evilRabbit"))
							{
								return;
							}
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(264, 209, 19, 16),
								sourceRectStartingPos = new Vector2(264f, 209f),
								animationLength = 1,
								totalNumberOfLoops = 999,
								interval = 999f,
								scale = 4f,
								position = new Vector2(4f, 1f) * 64f + new Vector2(38f, 23f) * 4f,
								layerDepth = 1f,
								motion = new Vector2(-2f, -2f),
								acceleration = new Vector2(0f, 0.1f),
								yStopCoordinate = 204,
								xStopCoordinate = 316,
								flipped = true,
								id = 778
							});
							return;
						case 'c':
						{
							if (!(key == "leahPicnic"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(96, 1808, 32, 48), 9999f, 1, 999, new Vector2(75f, 37f) * 64f, false, false, 0.2496f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							NPC l = new NPC(new AnimatedSprite(this.festivalContent, "Characters\\" + (this.farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(-100f, -100f) * 64f, 2, "LeahEx", null);
							l.AllowDynamicAppearance = false;
							this.actors.Add(l);
							return;
						}
						case 'd':
						case 'f':
						case 'g':
						case 'i':
						case 'j':
						case 'k':
						case 'm':
						case 'q':
						case 's':
						case 'u':
						case 'v':
							break;
						case 'e':
							if (!(key == "abbyOneBat"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, false, false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								xPeriodic = true,
								xPeriodicLoopTime = 2000f,
								xPeriodicRange = 128f,
								motion = new Vector2(0f, -8f)
							});
							return;
						case 'h':
							if (!(key == "waterShane"))
							{
								return;
							}
							this.drawTool = true;
							this.farmer.TemporaryItem = ItemRegistry.Create("(T)WateringCan", 1, 0, false);
							this.farmer.CurrentTool.Update(1, 0, this.farmer);
							this.farmer.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
							{
								new FarmerSprite.AnimationFrame(58, 0, false, false, null, false),
								new FarmerSprite.AnimationFrame(58, 75, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false),
								new FarmerSprite.AnimationFrame(59, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true),
								new FarmerSprite.AnimationFrame(45, 500, true, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true)
							}, null);
							return;
						case 'l':
							if (!(key == "witchFlyby"))
							{
								return;
							}
							Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width, 192f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(-4f, 0f),
								acceleration = new Vector2(-0.025f, 0f),
								yPeriodic = true,
								yPeriodicLoopTime = 2000f,
								yPeriodicRange = 64f,
								local = true
							});
							return;
						case 'n':
							if (!(key == "joshDinner"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite(649, 9999f, 1, 9999, new Vector2(6f, 4f) * 64f + new Vector2(8f, 32f), false, false)
							{
								layerDepth = 0.0256f
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite(664, 9999f, 1, 9999, new Vector2(8f, 4f) * 64f + new Vector2(-8f, 32f), false, false)
							{
								layerDepth = 0.0256f
							});
							return;
						case 'o':
							if (key == "luauShorts")
							{
								Vector2 shortsSpot = (Game1.year % 2 == 0) ? new Vector2(24f, 10f) : new Vector2(35f, 10f);
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 512, 16, 16), 9999f, 1, 99999, shortsSpot * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(-2f, -8f),
									acceleration = new Vector2(0f, 0.25f),
									yStopCoordinate = ((int)shortsSpot.Y + 1) * 64,
									xStopCoordinate = ((int)shortsSpot.X - 2) * 64
								});
								return;
							}
							if (!(key == "linusMoney"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1002f, -1000f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 10,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1003f, -1002f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 100,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-999f, -1000f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 200,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1004f, -1001f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 300,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1001f, -998f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 400,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -999f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 500,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -1002f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 600,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-997f, -1001f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								startSound = "money",
								delayBeforeAnimationStart = 700,
								overrideLocationDestroy = true
							});
							return;
						case 'p':
							if (!(key == "leahLaptop"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(130, 1849, 19, 19), 9999f, 1, 999, new Vector2(12f, 10f) * 64f + new Vector2(0f, 24f), false, false, 0.1856f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						case 'r':
						{
							if (key == "BoatParrot")
							{
								this.aboveMapSprites = new TemporaryAnimatedSpriteList();
								TemporaryAnimatedSprite.endBehavior <>9__5;
								this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(48, 0, 24, 24), 100f, 3, 99999, new Vector2((float)(Game1.viewport.X - 64), 2112f), false, true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									id = 999,
									motion = new Vector2(6f, 1f),
									delayBeforeAnimationStart = 0,
									pingPong = true,
									xStopCoordinate = 1040,
									reachedStopCoordinate = delegate(int param2)
									{
										TemporaryAnimatedSprite tempsprite = this.aboveMapSprites[0];
										if (tempsprite != null)
										{
											tempsprite.motion = new Vector2(0f, 2f);
											tempsprite.yStopCoordinate = 2336;
											TemporaryAnimatedSprite temporaryAnimatedSprite61 = tempsprite;
											TemporaryAnimatedSprite.endBehavior reachedStopCoordinate;
											if ((reachedStopCoordinate = <>9__5) == null)
											{
												reachedStopCoordinate = (<>9__5 = delegate(int param3)
												{
													TemporaryAnimatedSprite temporaryAnimatedSprite62 = this.aboveMapSprites[0];
													temporaryAnimatedSprite62.animationLength = 1;
													temporaryAnimatedSprite62.pingPong = false;
													temporaryAnimatedSprite62.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24);
													temporaryAnimatedSprite62.sourceRectStartingPos = Vector2.Zero;
												});
											}
											temporaryAnimatedSprite61.reachedStopCoordinate = reachedStopCoordinate;
										}
									}
								});
								return;
							}
							if (!(key == "movieFrame"))
							{
								return;
							}
							string movieId;
							string error2;
							int frame;
							int duration;
							if (!ArgUtility.TryGet(args, 2, out movieId, out error2, true, "string movieId") || !ArgUtility.TryGetInt(args, 3, out frame, out error2, "int frame") || !ArgUtility.TryGetInt(args, 4, out duration, out error2, "int duration"))
							{
								this.LogCommandError(args, error2, false);
								return;
							}
							movieId = MovieTheater.GetMovieIdFromLegacyIndex(movieId);
							MovieData data;
							if (!MovieTheater.TryGetMovieData(movieId, out data))
							{
								this.LogCommandError(args, "no movie found with ID '" + movieId + "'", false);
								return;
							}
							Microsoft.Xna.Framework.Rectangle sourceRect = MovieTheater.GetSourceRectForScreen(data.SheetIndex, frame);
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>(data.Texture ?? "LooseSprites\\Movies"),
								sourceRect = sourceRect,
								sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y),
								animationLength = 1,
								totalNumberOfLoops = 1,
								interval = (float)duration,
								scale = 4f,
								position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
								shakeIntensity = 0.25f,
								layerDepth = 0.0192f,
								id = 997
							});
							return;
						}
						case 't':
							if (!(key == "beachStuff"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1887, 47, 29), 9999f, 1, 999, new Vector2(44f, 21f) * 64f, false, false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						case 'w':
						{
							if (!(key == "swordswipe"))
							{
								return;
							}
							Vector2 position2;
							string error3;
							if (!ArgUtility.TryGetVector2(args, 2, out position2, out error3, true, "Vector2 position"))
							{
								this.LogCommandError(args, error3, false);
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, position2 * 64f + new Vector2(0f, -32f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false));
							return;
						}
						default:
							return;
						}
					}
					else
					{
						if (!(key == "wizardWarp"))
						{
							return;
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(8f, 16f) * 64f + new Vector2(0f, 4f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(2f, -2f),
							acceleration = new Vector2(0.1f, 0f),
							scaleChange = -0.02f,
							alphaFade = 0.001f
						});
						return;
					}
					break;
				}
				case 11:
				{
					char c = key[5];
					if (c <= 'L')
					{
						if (c != 'C')
						{
							if (c != 'L')
							{
								return;
							}
							if (!(key == "linusLights"))
							{
								return;
							}
							string lightSourceId = this.GenerateLightSourceId("linusLights");
							Game1.currentLightSources.Add(new LightSource(lightSourceId + "1", 2, new Vector2(55f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L, null));
							Game1.currentLightSources.Add(new LightSource(lightSourceId + "2", 2, new Vector2(60f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L, null));
							Game1.currentLightSources.Add(new LightSource(lightSourceId + "3", 2, new Vector2(57f, 60f) * 64f, 3f, LightSource.LightContext.None, 0L, null));
							Game1.currentLightSources.Add(new LightSource(lightSourceId + "4", 2, new Vector2(57f, 60f) * 64f, 2f, LightSource.LightContext.None, 0L, null));
							Game1.currentLightSources.Add(new LightSource(lightSourceId + "5", 2, new Vector2(47f, 70f) * 64f, 2f, LightSource.LightContext.None, 0L, null));
							Game1.currentLightSources.Add(new LightSource(lightSourceId + "6", 2, new Vector2(52f, 63f) * 64f, 2f, LightSource.LightContext.None, 0L, null));
							return;
						}
						else
						{
							if (!(key == "shaneCliffs"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f + new Vector2(-16f, 0f), false, false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(84f, 99f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(82f, 98f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(83f, 99f) * 64f + new Vector2(-8f, 4f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						}
					}
					else
					{
						switch (c)
						{
						case 'd':
							if (!(key == "wizardWarp2"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(54f, 34f) * 64f + new Vector2(0f, 4f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(-1f, 2f),
								acceleration = new Vector2(-0.1f, 0.2f),
								scaleChange = 0.03f,
								alphaFade = 0.001f
							});
							return;
						case 'e':
						case 'h':
						case 'j':
						case 'k':
							break;
						case 'f':
							if (!(key == "jasGiftOpen"))
							{
								return;
							}
							location.getTemporarySpriteByID(999).paused = false;
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(537, 1850, 11, 10), 1500f, 1, 1, new Vector2(23f, 16f) * 64f + new Vector2(16f, -48f), false, false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(0f, -0.25f),
								delayBeforeAnimationStart = 500,
								yStopCoordinate = 928
							});
							location.temporarySprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(1440, 992, 128, 64), 5, Color.White, 300, 0, ""));
							return;
						case 'g':
							if (!(key == "springOnion"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(1, 129, 16, 16), 200f, 8, 999999, new Vector2(84f, 39f) * 64f, false, false, 0.4736f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							return;
						case 'i':
							if (!(key == "curtainOpen"))
							{
								return;
							}
							location.getTemporarySpriteByID(999).sourceRect.X = 672;
							Game1.playSound("shwip", null);
							return;
						case 'l':
							if (!(key == "dickGlitter"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f, false, false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f, false));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 0f), false, false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
							{
								delayBeforeAnimationStart = 200
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 32f), false, false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
							{
								delayBeforeAnimationStart = 300
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(0f, 32f), false, false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
							{
								delayBeforeAnimationStart = 100
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(16f, 16f), false, false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
							{
								delayBeforeAnimationStart = 400
							});
							return;
						default:
							if (c != 'o')
							{
								switch (c)
								{
								case 's':
								{
									if (key == "krobusBeach")
									{
										for (int i3 = 0; i3 < 8; i3++)
										{
											location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 4, 0, new Vector2(84f + ((i3 % 2 == 0) ? 0.25f : -0.05f), 41f) * 64f, false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
											{
												delayBeforeAnimationStart = 500 + i3 * 1000,
												startSound = "waterSlosh"
											});
										}
										this.underwaterSprites = new TemporaryAnimatedSpriteList
										{
											new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(0f, -1f),
												xPeriodic = true,
												xPeriodicLoopTime = 3000f,
												xPeriodicRange = 16f,
												lightId = this.GenerateLightSourceId("krobusBeach_1"),
												lightcolor = Color.Black,
												lightRadius = 1f,
												yStopCoordinate = 2688,
												delayBeforeAnimationStart = 0,
												pingPong = true
											},
											new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(0f, -1f),
												xPeriodic = true,
												xPeriodicLoopTime = 3000f,
												xPeriodicRange = 16f,
												lightId = this.GenerateLightSourceId("krobusBeach_2"),
												lightcolor = Color.Black,
												lightRadius = 1f,
												yStopCoordinate = 3008,
												delayBeforeAnimationStart = 2000,
												pingPong = true
											},
											new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(0f, -1f),
												xPeriodic = true,
												xPeriodicLoopTime = 3000f,
												xPeriodicRange = 16f,
												lightId = this.GenerateLightSourceId("krobusBeach_3"),
												lightcolor = Color.Black,
												lightRadius = 1f,
												yStopCoordinate = 2688,
												delayBeforeAnimationStart = 150,
												pingPong = true
											},
											new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(0f, -1f),
												xPeriodic = true,
												xPeriodicLoopTime = 3000f,
												xPeriodicRange = 16f,
												lightId = this.GenerateLightSourceId("krobusBeach_4"),
												lightcolor = Color.Black,
												lightRadius = 1f,
												yStopCoordinate = 3008,
												delayBeforeAnimationStart = 2000,
												pingPong = true
											},
											new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(90f, 52f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(0f, -1f),
												xPeriodic = true,
												xPeriodicLoopTime = 3000f,
												xPeriodicRange = 16f,
												lightId = this.GenerateLightSourceId("krobusBeach_5"),
												lightcolor = Color.Black,
												lightRadius = 1f,
												yStopCoordinate = 2816,
												delayBeforeAnimationStart = 300,
												pingPong = true
											},
											new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(79f, 52f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(0f, -1f),
												xPeriodic = true,
												xPeriodicLoopTime = 3000f,
												xPeriodicRange = 16f,
												lightId = this.GenerateLightSourceId("krobusBeach_6"),
												lightcolor = Color.Black,
												lightRadius = 1f,
												yStopCoordinate = 2816,
												delayBeforeAnimationStart = 1000,
												pingPong = true
											}
										};
										return;
									}
									if (!(key == "krobusraven"))
									{
										return;
									}
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 100f, 5, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
									{
										pingPong = true,
										motion = new Vector2(-2f, 0f),
										yPeriodic = true,
										yPeriodicLoopTime = 3000f,
										yPeriodicRange = 16f,
										startSound = "shadowpeep"
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 32, 32, 32), 30f, 5, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
									{
										motion = new Vector2(-2.5f, 0f),
										yPeriodic = true,
										yPeriodicLoopTime = 2800f,
										yPeriodicRange = 16f,
										delayBeforeAnimationStart = 8000
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 64, 32, 39), 100f, 4, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
									{
										pingPong = true,
										motion = new Vector2(-3f, 0f),
										yPeriodic = true,
										yPeriodicLoopTime = 2000f,
										yPeriodicRange = 16f,
										delayBeforeAnimationStart = 15000,
										startSound = "fireball"
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										motion = new Vector2(-3f, 0f),
										yPeriodic = true,
										yPeriodicLoopTime = 2200f,
										yPeriodicRange = 32f,
										local = true,
										delayBeforeAnimationStart = 20000
									});
									for (int i4 = 0; i4 < 12; i4++)
									{
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(16, 594, 16, 12), 100f, 2, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											motion = new Vector2(-2f, 0f),
											yPeriodic = true,
											yPeriodicLoopTime = (float)Game1.random.Next(1500, 2000),
											yPeriodicRange = 32f,
											local = true,
											delayBeforeAnimationStart = 24000 + i4 * 200,
											startSound = ((i4 == 0) ? "yoba" : null)
										});
									}
									int whenToStart = 0;
									if (Game1.player.mailReceived.Contains("Capsule_Broken"))
									{
										for (int i5 = 0; i5 < 3; i5++)
										{
											location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(639, 785, 16, 16), 100f, 4, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(-2f, 0f),
												yPeriodic = true,
												yPeriodicLoopTime = (float)Game1.random.Next(1500, 2000),
												yPeriodicRange = 16f,
												local = true,
												delayBeforeAnimationStart = 30000 + i5 * 500,
												startSound = ((i5 == 0) ? "UFO" : null)
											});
										}
										whenToStart += 5000;
									}
									if (Game1.year <= 2)
									{
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2((float)(Game1.viewport.Width + 4), (float)Game1.viewport.Height * 0.33f + 44f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
										{
											motion = new Vector2(-2f, 0f),
											yPeriodic = true,
											yPeriodicLoopTime = 3000f,
											yPeriodicRange = 8f,
											delayBeforeAnimationStart = 30000 + whenToStart
										});
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(2, 129, 120, 27), 1090f, 1, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
										{
											motion = new Vector2(-2f, 0f),
											yPeriodic = true,
											yPeriodicLoopTime = 3000f,
											yPeriodicRange = 8f,
											startSound = "discoverMineral",
											delayBeforeAnimationStart = 30000 + whenToStart
										});
										whenToStart += 5000;
									}
									else if (Game1.year <= 3)
									{
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2((float)(Game1.viewport.Width + 4), (float)Game1.viewport.Height * 0.33f + 44f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
										{
											motion = new Vector2(-2f, 0f),
											yPeriodic = true,
											yPeriodicLoopTime = 3000f,
											yPeriodicRange = 8f,
											delayBeforeAnimationStart = 30000 + whenToStart
										});
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(1, 104, 100, 24), 1090f, 1, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
										{
											motion = new Vector2(-2f, 0f),
											yPeriodic = true,
											yPeriodicLoopTime = 3000f,
											yPeriodicRange = 8f,
											startSound = "newArtifact",
											delayBeforeAnimationStart = 30000 + whenToStart
										});
										whenToStart += 5000;
									}
									if (Game1.MasterPlayer.totalMoneyEarned >= 100000000U)
									{
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(125, 108, 34, 50), 1090f, 1, 999999, new Vector2((float)Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), false, false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
										{
											motion = new Vector2(-2f, 0f),
											yPeriodic = true,
											yPeriodicLoopTime = 3000f,
											yPeriodicRange = 8f,
											startSound = "discoverMineral",
											delayBeforeAnimationStart = 30000 + whenToStart
										});
										whenToStart += 5000;
										return;
									}
									break;
								}
								case 't':
									if (key == "parrotSlide")
									{
										location.getTemporarySpriteByID(666).yStopCoordinate = 5632;
										location.getTemporarySpriteByID(666).motion.X = 0f;
										location.getTemporarySpriteByID(666).motion.Y = 1f;
										return;
									}
									if (key == "parrotSplat")
									{
										this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2((float)(Game1.viewport.X + Game1.graphics.GraphicsDevice.Viewport.Width), (float)(Game1.viewport.Y + 64)), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											id = 999,
											motion = new Vector2(-2f, 4f),
											acceleration = new Vector2(-0.1f, 0f),
											delayBeforeAnimationStart = 0,
											yStopCoordinate = 5568,
											xStopCoordinate = 1504,
											reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.parrotSplat)
										});
										return;
									}
									if (!(key == "elliottBoat"))
									{
										return;
									}
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(461, 1843, 32, 51), 1000f, 2, 9999, new Vector2(15f, 26f) * 64f + new Vector2(-28f, 0f), false, false, 0.1664f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
									return;
								case 'u':
								case 'v':
									break;
								case 'w':
									if (!(key == "woodswalker"))
									{
										return;
									}
									location.temporarySprites.Add(new TemporaryAnimatedSprite
									{
										texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
										sourceRect = new Microsoft.Xna.Framework.Rectangle(448, 419, 16, 21),
										sourceRectStartingPos = new Vector2(448f, 419f),
										animationLength = 4,
										totalNumberOfLoops = 7,
										interval = 150f,
										scale = 4f,
										position = new Vector2(4f, 1f) * 64f + new Vector2(5f, 22f) * 4f,
										shakeIntensity = 1f,
										motion = new Vector2(1f, 0f),
										xStopCoordinate = 576,
										layerDepth = 1f,
										id = 996
									});
									return;
								default:
									return;
								}
							}
							else
							{
								if (!(key == "raccoonSong"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(3706f, 340f) - new Vector2(6.5f, 12f) * 4f, false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									usePreciseTiming = true
								});
								for (int i6 = 0; i6 < 8; i6++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(304, 397, 11, 11), 49f, 12, 1, new Vector2(3706f, 340f) + new Vector2(14f, -12f) * 4f, false, false)
									{
										scale = 4f,
										layerDepth = 0.05057f,
										delayBeforeAnimationStart = 2376 * i6,
										usePreciseTiming = true,
										motion = new Vector2(1f, 0f),
										acceleration = new Vector2(0f, 0.001f),
										color = new Color(255, 200, 200),
										rotationChange = (float)Game1.random.Next(-20, 20) / 1000f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(455, 414, 14, 17), 2376f, 1, 999, new Vector2(3706f, 340f) + new Vector2(7f, -12f) * 4f, false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i6,
										alphaFade = 0.02f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(374, 55, 12, 15), 297f, 8, 999, new Vector2(54f, 4f) * 64f + new Vector2(0f, -16f), false, true)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 297,
									usePreciseTiming = true
								});
								for (int i7 = 0; i7 < 8; i7++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(385, 414, 14, 17), 2376f, 1, 999, new Vector2(54f, 4f) * 64f + new Vector2(16f, -8f) + new Vector2(-15f, -17f) * 4f, false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i7 + 297,
										alphaFade = 0.02f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(3462f, 433f) - new Vector2(6.5f, 12f) * 4f, false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 594,
									usePreciseTiming = true
								});
								for (int i8 = 0; i8 < 8; i8++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(304, 397, 11, 11), 49f, 12, 1, new Vector2(3462f, 433f) + new Vector2(-20f, -16f) + new Vector2(-15f, -17f) * 4f, false, false)
									{
										scale = 4f,
										layerDepth = 0.05057f,
										delayBeforeAnimationStart = 2376 * i8 + 594,
										usePreciseTiming = true,
										motion = new Vector2(-1f, -1f),
										acceleration = new Vector2(0f, 0.001f),
										color = new Color(180, 200, 255),
										rotationChange = (float)Game1.random.Next(-20, 20) / 1000f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(371, 414, 14, 17), 2376f, 1, 999, new Vector2(3462f, 433f) + new Vector2(-20f, -16f) + new Vector2(-15f, -17f) * 4f, false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i8 + 594,
										alphaFade = 0.013f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(374, 55, 12, 15), 297f, 8, 999, new Vector2(58f, 4f) * 64f + new Vector2(0f, -24f), false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 891,
									usePreciseTiming = true
								});
								for (int i9 = 0; i9 < 8; i9++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(440, 415, 14, 15), 2376f, 1, 999, new Vector2(58f, 4f) * 64f + new Vector2(48f, -56f), false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i9 + 891,
										alphaFade = 0.02f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(3770f, 408f) - new Vector2(6.5f, 12f) * 4f, false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 1188,
									usePreciseTiming = true
								});
								for (int i10 = 0; i10 < 8; i10++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(469, 415, 14, 14), 2376f, 1, 999, new Vector2(3770f, 408f) + new Vector2(24f, -64f), false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i10 + 1188,
										alphaFade = 0.02f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(55f, 3f) * 64f + new Vector2(12f, 4f) - new Vector2(6.5f, 12f) * 4f, false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 1485,
									usePreciseTiming = true
								});
								for (int i11 = 0; i11 < 8; i11++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(400, 414, 12, 16), 2376f, 1, 999, new Vector2(55f, 3f) * 64f + new Vector2(-32f, -100f), false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i11 + 1485,
										alphaFade = 0.02f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(56f, 3f) * 64f + new Vector2(40f, -8f) - new Vector2(6.5f, 12f) * 4f, false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 1782,
									usePreciseTiming = true
								});
								for (int i12 = 0; i12 < 8; i12++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(304, 397, 11, 11), 49f, 12, 1, new Vector2(56f, 3f) * 64f + new Vector2(12f, -112f), false, false)
									{
										scale = 4f,
										layerDepth = 0.05057f,
										delayBeforeAnimationStart = 2376 * i12 + 1782,
										usePreciseTiming = true,
										motion = new Vector2(-0.25f, -1.5f),
										acceleration = new Vector2(0f, 0.001f),
										color = new Color(220, 255, 180),
										rotationChange = (float)Game1.random.Next(-20, 20) / 1000f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(414, 414, 12, 16), 2376f, 1, 999, new Vector2(56f, 3f) * 64f + new Vector2(12f, -112f), false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i12 + 1782,
										alphaFade = 0.013f,
										usePreciseTiming = true
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(374, 55, 12, 15), 297f, 8, 999, new Vector2(58f, 3f) * 64f + new Vector2(-24f, -52f), false, false)
								{
									scale = 4f,
									layerDepth = 0.044809997f,
									delayBeforeAnimationStart = 2079,
									usePreciseTiming = true
								});
								for (int i13 = 0; i13 < 8; i13++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(426, 414, 14, 15), 2376f, 1, 999, new Vector2(58f, 3f) * 64f + new Vector2(28f, -88f), false, false)
									{
										scale = 4f,
										layerDepth = 0.051209997f,
										delayBeforeAnimationStart = 2376 * i13 + 2079,
										alphaFade = 0.02f,
										usePreciseTiming = true
									});
								}
								return;
							}
							break;
						}
					}
					break;
				}
				case 12:
				{
					char c = key[4];
					if (c <= 'T')
					{
						if (c <= 'F')
						{
							if (c != 'C')
							{
								if (c != 'F')
								{
									return;
								}
								if (!(key == "joshFootball"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1916, 14, 8), 40f, 6, 9999, new Vector2(25f, 16f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									rotation = -0.7853982f,
									rotationChange = 0.015707964f,
									motion = new Vector2(6f, -4f),
									acceleration = new Vector2(0f, 0.2f),
									xStopCoordinate = 1856,
									reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.catchFootball),
									layerDepth = 1f,
									id = 56232
								});
								return;
							}
							else
							{
								if (!(key == "jojaCeremony"))
								{
									return;
								}
								this.aboveMapSprites = new TemporaryAnimatedSpriteList();
								for (int i14 = 0; i14 < 16; i14++)
								{
									Vector2 position3 = new Vector2((float)Game1.random.Next(Game1.viewport.Width - 128), (float)(Game1.viewport.Height + i14 * 64));
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, position3, false, false, 0.99f, 0f, Color.DeepSkyBlue, 4f, 0f, 0f, 0f, false)
									{
										local = true,
										motion = new Vector2(0.25f, -1.5f),
										acceleration = new Vector2(0f, -0.001f),
										id = 79797 + i14
									});
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, position3 + new Vector2(0f, 0f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										local = true,
										motion = new Vector2(0.25f, -1.5f),
										acceleration = new Vector2(0f, -0.001f),
										id = 79797 + i14
									});
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1363, 114, 58), 99999f, 1, 99999, new Vector2(50f, 20f) * 64f, false, false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(48f, 20f) * 64f, false, false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									pingPong = true
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(49f, 20f) * 64f, false, false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									pingPong = true
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 210f, 3, 99999, new Vector2(62f, 20f) * 64f, false, false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									pingPong = true
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 190f, 3, 99999, new Vector2(60f, 20f) * 64f, false, false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									pingPong = true
								});
								return;
							}
						}
						else if (c != 'M')
						{
							if (c != 'T')
							{
								return;
							}
							if (!(key == "maruTrapdoor"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1632, 16, 32), 150f, 4, 0, new Vector2(1f, 5f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(688, 1632, 16, 32), 99999f, 1, 0, new Vector2(1f, 5f) * 64f, false, false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								delayBeforeAnimationStart = 500
							});
							return;
						}
						else
						{
							if (!(key == "abbyManyBats"))
							{
								return;
							}
							for (int i15 = 0; i15 < 100; i15++)
							{
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, false, false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									xPeriodic = true,
									xPeriodicLoopTime = (float)Game1.random.Next(1500, 2500),
									xPeriodicRange = (float)Game1.random.Next(64, 192),
									motion = new Vector2((float)Game1.random.Next(-2, 3), (float)Game1.random.Next(-8, -4)),
									delayBeforeAnimationStart = i15 * 30,
									startSound = ((i15 % 10 == 0 || Game1.random.NextDouble() < 0.1) ? "batScreech" : null)
								});
							}
							for (int i16 = 0; i16 < 100; i16++)
							{
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, false, false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2((float)Game1.random.Next(-4, 5), (float)Game1.random.Next(-8, -4)),
									delayBeforeAnimationStart = 10 + i16 * 30
								});
							}
							return;
						}
					}
					else if (c <= 'i')
					{
						switch (c)
						{
						case 'a':
							if (!(key == "curtainClose"))
							{
								return;
							}
							location.getTemporarySpriteByID(999).sourceRect.X = 644;
							Game1.playSound("shwip", null);
							return;
						case 'b':
						case 'c':
							break;
						case 'd':
							if (!(key == "grandpaNight"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 1f) * 64f, false, false, 0.9f, 0f, Color.Cyan, 4f, 0f, 0f, 0f, true)
							{
								alpha = 0.01f,
								alphaFade = -0.002f,
								local = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 768f), false, true, 0.9f, 0f, Color.Blue, 4f, 0f, 0f, 0f, true)
							{
								alpha = 0.01f,
								alphaFade = -0.002f,
								local = true
							});
							return;
						case 'e':
							if (!(key == "marcelloLand"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, (new Vector2(25f, 19f) + this.eventPositionTileOffset) * 64f + new Vector2(-23f, 0f) * 4f, false, false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(0f, 2f),
								yStopCoordinate = (41 + (int)this.eventPositionTileOffset.Y) * 64 - 640,
								reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.marcelloBalloonLand),
								attachedCharacter = this.getActorByName("Marcello", false),
								id = 1
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, (new Vector2(25f, 19f) + this.eventPositionTileOffset) * 64f + new Vector2(0f, 134f) * 4f, false, false, (41f + this.eventPositionTileOffset.Y) * 64f / 10000f + 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(0f, 2f),
								id = 2
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(24, 1343, 36, 19), 7000f, 1, 99999, (new Vector2(25f, 40f) + this.eventPositionTileOffset) * 64f, false, false, 1E-05f, 0f, Color.White, 0f, 0f, 0f, 0f, false)
							{
								scaleChange = 0.01f,
								id = 3
							});
							return;
						default:
							if (c != 'i')
							{
								return;
							}
							if (!(key == "staticSprite"))
							{
								if (!(key == "morrisFlying"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(105, 1318, 13, 31), 9999f, 1, 99999, new Vector2(32f, 13f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(4f, -8f),
									rotationChange = 0.19634955f,
									shakeIntensity = 1f
								});
								return;
							}
							else
							{
								string textureName;
								string error4;
								Microsoft.Xna.Framework.Rectangle sourceRect2;
								Vector2 tile2;
								int id;
								float layerDepth;
								if (!ArgUtility.TryGet(args, 2, out textureName, out error4, true, "string textureName") || !ArgUtility.TryGetRectangle(args, 3, out sourceRect2, out error4, "Rectangle sourceRect") || !ArgUtility.TryGetVector2(args, 7, out tile2, out error4, false, "Vector2 tile") || !ArgUtility.TryGetOptionalInt(args, 9, out id, out error4, 999, "int id") || !ArgUtility.TryGetOptionalFloat(args, 10, out layerDepth, out error4, 1f, "float layerDepth"))
								{
									this.LogCommandError(args, error4, false);
									return;
								}
								location.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect2, tile2 * 64f, false, 0f, Color.White)
								{
									animationLength = 1,
									interval = 999999f,
									scale = 4f,
									layerDepth = layerDepth,
									id = id
								});
								return;
							}
							break;
						}
					}
					else if (c != 'o')
					{
						if (c != 'v')
						{
							if (c != 'y')
							{
								return;
							}
							if (key == "EmilyCamping")
							{
								this.showGroundObjects = false;
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1578, 59, 53), 999999f, 1, 99999, new Vector2(26f, 9f) * 64f + new Vector2(-16f, 0f), false, false, 0.0788f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									id = 999
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(675, 1299, 29, 24), 999999f, 1, 99999, new Vector2(27f, 14f) * 64f, false, false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									id = 99
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(27f, 14f) * 64f + new Vector2(8f, 4f) * 4f, false, 0f, Color.White)
								{
									interval = 50f,
									totalNumberOfLoops = 99999,
									animationLength = 4,
									lightId = this.GenerateLightSourceId("EmilyCamping_1"),
									id = 666,
									lightRadius = 2f,
									scale = 4f,
									layerDepth = 0.01f
								});
								Game1.currentLightSources.Add(new LightSource(this.GenerateLightSourceId("EmilyCamping_2"), 4, new Vector2(27f, 14f) * 64f, 2f, LightSource.LightContext.None, 0L, null));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(585, 1846, 26, 22), 999999f, 1, 99999, new Vector2(25f, 12f) * 64f + new Vector2(-32f, 0f), false, false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									id = 96
								});
								AmbientLocationSounds.addSound(new Vector2(27f, 14f), 1);
								return;
							}
							if (!(key == "EmilyBoomBox"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(586, 1871, 24, 14), 99999f, 1, 99999, new Vector2(15f, 4f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							return;
						}
						else
						{
							if (!(key == "removeSprite"))
							{
								return;
							}
							int spriteId;
							string error5;
							if (!ArgUtility.TryGetInt(args, 2, out spriteId, out error5, "int spriteId"))
							{
								this.LogCommandError(args, error5, false);
								return;
							}
							location.removeTemporarySpritesWithID(spriteId);
							return;
						}
					}
					else
					{
						if (!(key == "balloonBirds"))
						{
							return;
						}
						int positionOffset;
						string error6;
						if (!ArgUtility.TryGetOptionalInt(args, 2, out positionOffset, out error6, 0, "int positionOffset"))
						{
							this.LogCommandError(args, error6, false);
							return;
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, (float)(positionOffset + 12)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1500
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, (float)(positionOffset + 13)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1250
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, (float)(positionOffset + 14)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1100
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, (float)(positionOffset + 15)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1000
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, (float)(positionOffset + 16)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1080
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, (float)(positionOffset + 17)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1300
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, (float)(positionOffset + 18)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1450
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, (float)(positionOffset + 15)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-4f, 0f),
							delayBeforeAnimationStart = 5450
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, (float)(positionOffset + 10)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 500
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, (float)(positionOffset + 11)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 250
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, (float)(positionOffset + 12)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 100
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, (float)(positionOffset + 13)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f)
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, (float)(positionOffset + 14)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 80
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, (float)(positionOffset + 15)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 300
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, (float)(positionOffset + 16)) * 64f, false, false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 450
						});
						return;
					}
					break;
				}
				case 13:
				{
					char c = key[9];
					if (c <= 'F')
					{
						if (c != 'D')
						{
							if (c != 'F')
							{
								return;
							}
							if (!(key == "sebastianFrog"))
							{
								return;
							}
							Texture2D crittersText = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = crittersText,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
								animationLength = 4,
								sourceRectStartingPos = new Vector2(0f, 224f),
								interval = 120f,
								totalNumberOfLoops = 9999,
								position = new Vector2(45f, 36f) * 64f,
								scale = 4f,
								layerDepth = 0.00064f,
								motion = new Vector2(2f, 0f),
								xStopCoordinate = 3136,
								id = 777,
								reachedStopCoordinate = delegate(int param1)
								{
									int num = this.CurrentCommand;
									this.CurrentCommand = num + 1;
									location.removeTemporarySpritesWithID(777);
								}
							});
							return;
						}
						else
						{
							if (!(key == "haleyRoomDark"))
							{
								return;
							}
							Game1.currentLightSources.Clear();
							Game1.ambientLight = new Color(200, 200, 100);
							location.TemporarySprites.Add(new TemporaryAnimatedSprite(743, 999999f, 1, 0, new Vector2(4f, 1f) * 64f, false, false)
							{
								lightId = this.GenerateLightSourceId("haleyRoomDark"),
								lightcolor = new Color(0, 255, 255),
								lightRadius = 2f
							});
							return;
						}
					}
					else
					{
						switch (c)
						{
						case 'R':
							if (!(key == "sebastianRide"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1843, 14, 9), 40f, 4, 999, new Vector2(19f, 8f) * 64f + new Vector2(0f, 28f), false, false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(-2f, 0f)
							});
							return;
						case 'S':
							if (!(key == "shakeBushStop"))
							{
								return;
							}
							location.getTemporarySpriteByID(777).shakeIntensity = 0f;
							return;
						case 'T':
							if (key == "trashBearTown")
							{
								this.aboveMapSprites = new TemporaryAnimatedSpriteList();
								this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(46, 80, 46, 56), new Vector2(43f, 64f) * 64f, false, 0f, Color.White)
								{
									animationLength = 1,
									interval = 999999f,
									motion = new Vector2(4f, 0f),
									scale = 4f,
									layerDepth = 1f,
									yPeriodic = true,
									yPeriodicLoopTime = 2000f,
									yPeriodicRange = 32f,
									id = 777,
									xStopCoordinate = 3392,
									reachedStopCoordinate = delegate(int param)
									{
										this.aboveMapSprites[0].xStopCoordinate = -1;
										this.aboveMapSprites[0].motion = new Vector2(4f, 0f);
										location.ApplyMapOverride("Town-TrashGone", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(57, 68, 17, 5)));
										location.ApplyMapOverride("Town-DogHouse", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(51, 65, 5, 6)));
										Game1.flashAlpha = 0.75f;
										Game1.screenGlowOnce(Color.Lime, false, 0.25f, 1f);
										location.playSound("yoba", null, null, SoundContext.Default);
										TemporaryAnimatedSprite tmpsprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), new Vector2(3456f, 4160f), false, 0f, Color.White)
										{
											yStopCoordinate = 4372,
											motion = new Vector2(-0.5f, -10f),
											acceleration = new Vector2(0f, 0.25f),
											scale = 4f,
											alphaFade = 0f,
											extraInfoForEndBehavior = -777
										};
										tmpsprite.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(tmpsprite.bounce);
										tmpsprite.initialPosition.Y = 4372f;
										this.aboveMapSprites.Add(tmpsprite);
										this.aboveMapSprites.AddRange(Utility.getStarsAndSpirals(location, 54, 69, 6, 5, 1000, 10, Color.Lime, null, false));
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											id = 1,
											delayBeforeAnimationStart = 3000,
											startSound = "dogWhining"
										});
									}
								});
								return;
							}
							if (!(key == "stopShakeTent"))
							{
								return;
							}
							location.getTemporarySpriteByID(999).shakeIntensity = 0f;
							return;
						case 'U':
						case 'V':
							break;
						case 'W':
						{
							if (!(key == "haleyCakeWalk"))
							{
								return;
							}
							Texture2D tempTxture4 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture4,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 400, 144, 112),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(0f, 400f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(26f, 65f) * 64f,
								scale = 4f,
								layerDepth = 0.00064f
							});
							return;
						}
						default:
							switch (c)
							{
							case 'a':
							{
								if (!(key == "pamYobaStatue"))
								{
									return;
								}
								location.objects.Remove(new Vector2(26f, 9f));
								location.objects.Add(new Vector2(26f, 9f), ItemRegistry.Create<Object>("(BC)34", 1, 0, false));
								GameLocation gameLocation = Game1.RequireLocation("Trailer_Big", false);
								gameLocation.objects.Remove(new Vector2(26f, 9f));
								gameLocation.objects.Add(new Vector2(26f, 9f), ItemRegistry.Create<Object>("(BC)34", 1, 0, false));
								return;
							}
							case 'b':
							case 'e':
							case 'h':
							case 'j':
							case 'k':
							case 'l':
							case 'o':
							case 'q':
							case 's':
							case 'u':
							case 'v':
							case 'x':
								break;
							case 'c':
								if (!(key == "maruTelescope"))
								{
									return;
								}
								for (int i17 = 0; i17 < 9; i17++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(256, 1680, 16, 16), 80f, 5, 0, new Vector2((float)Game1.random.Next(1, 28), (float)Game1.random.Next(1, 20)) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										delayBeforeAnimationStart = 8000 + i17 * Game1.random.Next(2000),
										motion = new Vector2(4f, 4f)
									});
								}
								if (this.id == "5183338")
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(206, 1827, 15, 27), 80f, 4, 999, new Vector2(-2f, 13f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 1.2f, 0f, false)
									{
										delayBeforeAnimationStart = 7000,
										motion = new Vector2(2f, -0.5f),
										alpha = 0.01f,
										alphaFade = -0.005f
									});
									return;
								}
								break;
							case 'd':
								if (!(key == "skateboardFly"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1875, 16, 6), 9999f, 1, 999, new Vector2(26f, 90f) * 64f, false, false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									rotationChange = 0.1308997f,
									motion = new Vector2(-8f, -10f),
									acceleration = new Vector2(0.02f, 0.3f),
									yStopCoordinate = 5824,
									xStopCoordinate = 1024,
									layerDepth = 1f
								});
								return;
							case 'f':
								if (!(key == "linusCampfire"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 99999, new Vector2(29f, 9f) * 64f + new Vector2(8f, 0f), false, false, 0.0576f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("linusCampfire"),
									lightRadius = 3f,
									lightcolor = Color.Black
								});
								return;
							case 'g':
								if (!(key == "alexDiningDog"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(7f, 2f) * 64f + new Vector2(2f, -8f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									id = 1
								});
								return;
							case 'i':
							{
								if (key == "shaneHospital")
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 10), 99999f, 1, 99999, new Vector2(20f, 3f) * 64f + new Vector2(16f, 12f), false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										id = 999
									});
									return;
								}
								if (!(key == "grandpaSpirit"))
								{
									return;
								}
								TemporaryAnimatedSprite p = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(555, 1956, 18, 35), 9999f, 1, 99999, new Vector2(-1000f, -1010f) * 64f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									yStopCoordinate = -64128,
									xPeriodic = true,
									xPeriodicLoopTime = 3000f,
									xPeriodicRange = 16f,
									motion = new Vector2(0f, 1f),
									overrideLocationDestroy = true,
									id = 77777
								};
								location.temporarySprites.Add(p);
								for (int i18 = 0; i18 < 19; i18++)
								{
									location.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
									{
										parentSprite = p,
										delayBeforeAnimationStart = (i18 + 1) * 500,
										overrideLocationDestroy = true,
										scale = 1f,
										alpha = 1f
									});
								}
								return;
							}
							case 'm':
								if (!(key == "WizardPromise"))
								{
									return;
								}
								Utility.addSprinklesToLocation(location, 16, 15, 9, 9, 2000, 50, Color.White, null, false);
								return;
							case 'n':
							{
								if (key == "raccoondance2")
								{
									location.removeTemporarySpritesWithIDLocal(9786);
									TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(9785);
									temporarySpriteByID2.sourceRect.Y = 64;
									temporarySpriteByID2.sourceRectStartingPos.Y = 64f;
									temporarySpriteByID2.currentParentTileIndex = 0;
									temporarySpriteByID2.motion.X = 0f;
									temporarySpriteByID2.interval *= 2f;
									temporarySpriteByID2.timer = 0f;
									temporarySpriteByID2.sourceRect.X = 0;
									temporarySpriteByID2.position.X = temporarySpriteByID2.position.X - 32f;
									temporarySpriteByID2.position.Y = temporarySpriteByID2.position.Y + 8f;
									return;
								}
								if (!(key == "raccoondance1"))
								{
									return;
								}
								TemporaryAnimatedSprite temporarySpriteByID3 = location.getTemporarySpriteByID(9786);
								TemporaryAnimatedSprite mrs_raccoon = location.getTemporarySpriteByID(9785);
								temporarySpriteByID3.sourceRect.Y = 96;
								temporarySpriteByID3.sourceRectStartingPos.Y = 96f;
								temporarySpriteByID3.currentParentTileIndex = 1;
								temporarySpriteByID3.motion.X = 0.07f;
								temporarySpriteByID3.timer = 0f;
								mrs_raccoon.sourceRect.Y = 32;
								mrs_raccoon.sourceRectStartingPos.Y = 32f;
								mrs_raccoon.currentParentTileIndex = 1;
								mrs_raccoon.motion.X = -0.07f;
								mrs_raccoon.timer = 0f;
								return;
							}
							case 'p':
								if (!(key == "EmilySleeping"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(574, 1892, 11, 11), 1000f, 2, 99999, new Vector2(20f, 3f) * 64f + new Vector2(8f, 32f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									id = 999
								});
								return;
							case 'r':
								if (!(key == "raccoonCircle"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\raccoon", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 148f, 8, 999, new Vector2(54.5f, 7f) * 64f, false, false)
								{
									scale = 4f,
									layerDepth = 0.051840004f,
									usePreciseTiming = true,
									id = 9786
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\mrs_raccoon", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 148f, 8, 999, new Vector2(56.5f, 7f) * 64f, false, false)
								{
									scale = 4f,
									layerDepth = 0.0512f,
									usePreciseTiming = true,
									id = 9785
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), Vector2.Zero, false, 0f, Color.White)
								{
									drawAboveAlwaysFront = true,
									vectorScale = new Vector2(3090f, 1052f),
									interval = 99999f,
									totalNumberOfLoops = 1,
									id = 997799
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(56.5f, 0f) * 64f + new Vector2(131.5f, 0f) * 4f, false, 0f, Color.White)
								{
									drawAboveAlwaysFront = true,
									vectorScale = new Vector2(5536f, 1052f),
									interval = 99999f,
									totalNumberOfLoops = 1,
									id = 997799
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(0f, 876f), false, 0f, Color.White)
								{
									drawAboveAlwaysFront = true,
									vectorScale = new Vector2(7552f, 7488f),
									interval = 99999f,
									totalNumberOfLoops = 1,
									id = 997799
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 263, 263), new Vector2(56.5f, 0f) * 64f - new Vector2(131.5f, 44f) * 4f, false, 0f, Color.Black)
								{
									drawAboveAlwaysFront = true,
									interval = 297f,
									animationLength = 3,
									totalNumberOfLoops = 99999,
									id = 997799,
									scale = 4f
								});
								return;
							case 't':
								if (!(key == "ccCelebration"))
								{
									return;
								}
								this.aboveMapSprites = new TemporaryAnimatedSpriteList();
								for (int i19 = 0; i19 < 32; i19++)
								{
									Vector2 position4 = new Vector2((float)Game1.random.Next(Game1.viewport.Width - 128), (float)(Game1.viewport.Height + i19 * 64));
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, position4, false, false, 1f, 0f, Utility.getRandomRainbowColor(null), 4f, 0f, 0f, 0f, false)
									{
										local = true,
										motion = new Vector2(0.25f, -1.5f),
										acceleration = new Vector2(0f, -0.001f),
										id = 79797 + i19
									});
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, position4 + new Vector2(0f, 0f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										local = true,
										motion = new Vector2(0.25f, -1.5f),
										acceleration = new Vector2(0f, -0.001f),
										id = 79797 + i19
									});
								}
								if (Game1.IsWinter)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\marnie_winter_dance", new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 26), 400f, 3, 99999, new Vector2(53f, 21f) * 64f, false, false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										pingPong = true
									});
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(558, 1425, 20, 26), 400f, 3, 99999, new Vector2(53f, 21f) * 64f, false, false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									pingPong = true
								});
								return;
							case 'w':
								if (!(key == "shaneThrowCan"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(103f, 95f) * 64f + new Vector2(0f, 4f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(0f, -4f),
									acceleration = new Vector2(0f, 0.25f),
									rotationChange = 0.024543693f
								});
								Game1.playSound("shwip", null);
								return;
							case 'y':
								if (!(key == "abbyGraveyard"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(736, 999999f, 1, 0, new Vector2(48f, 86f) * 64f, false, false));
								return;
							default:
								return;
							}
							break;
						}
					}
					break;
				}
				case 14:
				{
					char c = key[6];
					if (c <= 'L')
					{
						if (c <= 'C')
						{
							if (c != 'B')
							{
								if (c != 'C')
								{
									return;
								}
								if (!(key == "junimoCageGone"))
								{
									return;
								}
								location.removeTemporarySpritesWithID(1);
								return;
							}
							else
							{
								if (!(key == "candleBoatMove"))
								{
									return;
								}
								this.showGroundObjects = false;
								location.getTemporarySpriteByID(1).motion = new Vector2(0f, 2f);
								return;
							}
						}
						else if (c != 'G')
						{
							if (c != 'L')
							{
								return;
							}
							if (!(key == "georgeLeekGift"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(17f, 19f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999,
								paused = false,
								holdLastFrame = true
							});
							return;
						}
						else
						{
							if (!(key == "secretGiftOpen"))
							{
								return;
							}
							TemporaryAnimatedSprite t = location.getTemporarySpriteByID(666);
							if (t != null)
							{
								t.animationLength = 6;
								t.interval = 100f;
								t.totalNumberOfLoops = 1;
								t.timer = 0f;
								t.holdLastFrame = true;
								return;
							}
						}
					}
					else if (c <= 'a')
					{
						if (c != 'P')
						{
							if (c != 'a')
							{
								return;
							}
							if (!(key == "shanePassedOut"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f + new Vector2(-16f, 0f), false, false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						}
						else
						{
							if (!(key == "parrotPerchHut"))
							{
								return;
							}
							location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24), new Vector2(7f, 4f) * 64f, false, 0f, Color.White)
							{
								animationLength = 1,
								interval = 999999f,
								scale = 4f,
								layerDepth = 1f,
								id = 999
							});
							return;
						}
					}
					else
					{
						switch (c)
						{
						case 'e':
							if (!(key == "trashBearMagic"))
							{
								return;
							}
							Utility.addStarsAndSpirals(location, 95, 103, 24, 12, 2000, 10, Color.Lime, null, false);
							(location as Forest).removeSewerTrash();
							Game1.flashAlpha = 0.75f;
							Game1.screenGlowOnce(Color.Lime, false, 0.25f, 1f);
							return;
						case 'f':
						case 'g':
							break;
						case 'h':
							if (!(key == "waterShaneDone"))
							{
								return;
							}
							this.farmer.completelyStopAnimatingOrDoingAction();
							this.farmer.TemporaryItem = null;
							this.drawTool = false;
							location.removeTemporarySpritesWithID(999);
							return;
						case 'i':
							if (!(key == "pennyFieldTrip"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1813, 86, 54), 999999f, 1, 0, new Vector2(68f, 44f) * 64f, false, false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						default:
							if (c != 'l')
							{
								if (c != 'n')
								{
									return;
								}
								if (!(key == "raccoonCircle2"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 263, 263), new Vector2(56.5f, 0f) * 64f - new Vector2(131.5f, 44f) * 4f, false, 0f, Color.White)
								{
									drawAboveAlwaysFront = true,
									interval = 297f,
									animationLength = 3,
									totalNumberOfLoops = 99999,
									id = 997797,
									scale = 4f,
									alpha = 0.01f,
									alphaFade = -0.003f,
									layerDepth = 0.8f
								});
								return;
							}
							else
							{
								if (!(key == "gridballGameTV"))
								{
									return;
								}
								Texture2D tempTxture5 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
								location.TemporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = tempTxture5,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
									animationLength = 7,
									sourceRectStartingPos = new Vector2(368f, 336f),
									interval = 5000f,
									totalNumberOfLoops = 99999,
									position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
									scale = 4f,
									layerDepth = 1f
								});
								return;
							}
							break;
						}
					}
					break;
				}
				case 15:
				{
					char c = key[10];
					if (c <= 'G')
					{
						if (c != 'C')
						{
							if (c != 'G')
							{
								return;
							}
							if (!(key == "junimoCageGone2"))
							{
								return;
							}
							location.removeTemporarySpritesWithID(1);
							Game1.viewportFreeze = true;
							Game1.viewport.X = -1000;
							Game1.viewport.Y = -1000;
							return;
						}
						else
						{
							if (!(key == "iceFishingCatch"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(68f, 30f) * 64f, false, false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(74f, 30f) * 64f, false, false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 490f, 3, 99999, new Vector2(67f, 36f) * 64f, false, false, 0.2368f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(76f, 35f) * 64f, false, false, 0.2304f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						}
					}
					else if (c != 'L')
					{
						if (c != 'P')
						{
							switch (c)
							{
							case 'a':
								if (!(key == "sebastianGarage"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1843, 48, 42), 9999f, 1, 999, new Vector2(17f, 23f) * 64f + new Vector2(0f, 8f), false, false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								this.getActorByName("Sebastian", false).HideShadow = true;
								return;
							case 'b':
							case 'd':
							case 'f':
							case 'g':
							case 'h':
							case 'i':
							case 'j':
							case 'k':
							case 'p':
								break;
							case 'c':
								if (!(key == "abbyvideoscreen"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 9999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, false, false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								return;
							case 'e':
							{
								if (key == "harveyDinnerSet")
								{
									Vector2 centerPoint = new Vector2(5f, 16f);
									DecoratableLocation decoratableLocation = location as DecoratableLocation;
									if (decoratableLocation != null)
									{
										foreach (Furniture f in decoratableLocation.furniture)
										{
											if (f.furniture_type.Value == 14 && !location.hasTileAt((int)f.tileLocation.X, (int)f.tileLocation.Y + 1, "Buildings", null) && !location.hasTileAt((int)f.tileLocation.X + 1, (int)f.tileLocation.Y + 1, "Buildings", null) && !location.hasTileAt((int)f.tileLocation.X + 2, (int)f.tileLocation.Y + 1, "Buildings", null) && !location.hasTileAt((int)f.tileLocation.X - 1, (int)f.tileLocation.Y + 1, "Buildings", null))
											{
												centerPoint = new Vector2((float)((int)f.TileLocation.X), (float)((int)f.TileLocation.Y + 1));
												f.isOn.Value = true;
												f.setFireplace(false, false);
												break;
											}
										}
									}
									location.TemporarySprites.Clear();
									this.getActorByName("Harvey", false).setTilePosition((int)centerPoint.X + 2, (int)centerPoint.Y);
									this.getActorByName("Harvey", false).Position = new Vector2(this.getActorByName("Harvey", false).Position.X - 32f, this.getActorByName("Harvey", false).Position.Y);
									this.farmer.Position = new Vector2(centerPoint.X * 64f - 32f, centerPoint.Y * 64f + 32f);
									Object o = location.getObjectAtTile((int)centerPoint.X, (int)centerPoint.Y, false);
									if (o != null)
									{
										o.isTemporarilyInvisible = true;
									}
									o = location.getObjectAtTile((int)centerPoint.X + 1, (int)centerPoint.Y, false);
									if (o != null)
									{
										o.isTemporarilyInvisible = true;
									}
									o = location.getObjectAtTile((int)centerPoint.X - 1, (int)centerPoint.Y, false);
									if (o != null)
									{
										o.isTemporarilyInvisible = true;
									}
									o = location.getObjectAtTile((int)centerPoint.X + 2, (int)centerPoint.Y, false);
									if (o != null)
									{
										o.isTemporarilyInvisible = true;
									}
									Texture2D tempTxture6 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
									location.TemporarySprites.Add(new TemporaryAnimatedSprite
									{
										texture = tempTxture6,
										sourceRect = new Microsoft.Xna.Framework.Rectangle(385, 423, 48, 32),
										animationLength = 1,
										sourceRectStartingPos = new Vector2(385f, 423f),
										interval = 5000f,
										totalNumberOfLoops = 9999,
										position = centerPoint * 64f + new Vector2(-8f, -16f) * 4f,
										scale = 4f,
										layerDepth = (centerPoint.Y + 0.2f) * 64f / 10000f,
										lightId = this.GenerateLightSourceId("harveyDinnerSet"),
										lightRadius = 4f,
										lightcolor = Color.Black
									});
									List<string> tmp = this.eventCommands.ToList<string>();
									tmp.Insert(this.CurrentCommand + 1, string.Concat(new string[]
									{
										"viewport ",
										((int)centerPoint.X).ToString(),
										" ",
										((int)centerPoint.Y).ToString(),
										" true"
									}));
									this.eventCommands = tmp.ToArray();
									return;
								}
								if (key == "ClothingTherapy")
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1405, 28, 46), 999999f, 1, 99999, new Vector2(5f, 6f) * 64f + new Vector2(-32f, -144f), false, false, 0.0424f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										id = 999
									});
									return;
								}
								if (!(key == "getEndSlideshow"))
								{
									return;
								}
								Summit summit = location as Summit;
								string[] s = Event.ParseCommands(summit.getEndSlideshow(), null);
								List<string> commandsList = this.eventCommands.ToList<string>();
								commandsList.InsertRange(this.CurrentCommand + 1, s);
								this.eventCommands = commandsList.ToArray();
								summit.isShowingEndSlideshow = true;
								return;
							}
							case 'l':
								if (!(key == "junimoSpotlight"))
								{
									return;
								}
								this.actors[0].drawOnTop = true;
								location.TemporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
									sourceRect = new Microsoft.Xna.Framework.Rectangle(316, 123, 67, 43),
									sourceRectStartingPos = new Vector2(316f, 123f),
									animationLength = 1,
									interval = 5000f,
									totalNumberOfLoops = 9999,
									scale = 4f,
									position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 268, 172, 0, -20),
									layerDepth = 0.0001f,
									local = true,
									id = 999
								});
								return;
							case 'm':
							{
								if (!(key == "grandpaThumbsUp"))
								{
									return;
								}
								TemporaryAnimatedSprite temporarySpriteByID4 = location.getTemporarySpriteByID(77777);
								temporarySpriteByID4.texture = Game1.mouseCursors2;
								temporarySpriteByID4.sourceRect = new Microsoft.Xna.Framework.Rectangle(186, 265, 22, 34);
								temporarySpriteByID4.sourceRectStartingPos = new Vector2(186f, 265f);
								temporarySpriteByID4.yPeriodic = true;
								temporarySpriteByID4.yPeriodicLoopTime = 1000f;
								temporarySpriteByID4.yPeriodicRange = 16f;
								temporarySpriteByID4.xPeriodicLoopTime = 2500f;
								temporarySpriteByID4.xPeriodicRange = 16f;
								temporarySpriteByID4.initialPosition = temporarySpriteByID4.position;
								return;
							}
							case 'n':
								if (key == "shaneSaloonCola")
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite
									{
										texture = Game1.mouseCursors,
										sourceRect = new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21),
										animationLength = 1,
										sourceRectStartingPos = new Vector2(552f, 1862f),
										interval = 999999f,
										totalNumberOfLoops = 99999,
										position = new Vector2(32f, 17f) * 64f + new Vector2(10f, 3f) * 4f,
										scale = 4f,
										layerDepth = 1E-07f
									});
									return;
								}
								if (key == "springOnionPeel")
								{
									TemporaryAnimatedSprite temporarySpriteByID5 = location.getTemporarySpriteByID(777);
									temporarySpriteByID5.sourceRectStartingPos = new Vector2(144f, 327f);
									temporarySpriteByID5.sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 327, 112, 112);
									return;
								}
								if (key == "springOnionDemo")
								{
									Texture2D tempTex = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
									location.TemporarySprites.Add(new TemporaryAnimatedSprite
									{
										texture = tempTex,
										sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 215, 112, 112),
										animationLength = 2,
										sourceRectStartingPos = new Vector2(144f, 215f),
										interval = 200f,
										totalNumberOfLoops = 99999,
										id = 777,
										position = new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 264), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 3 - 264)),
										local = true,
										scale = 4f,
										destroyable = false,
										overrideLocationDestroy = true
									});
									return;
								}
								if (!(key == "sebastianOnBike"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 80f, 8, 9999, new Vector2(19f, 27f) * 64f + new Vector2(32f, -16f), false, true, 0.1792f, 0f, Color.White, 1f, 0f, 0f, 0f, false));
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1854, 47, 33), 9999f, 1, 999, new Vector2(17f, 27f) * 64f + new Vector2(0f, -8f), false, false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
								return;
							case 'o':
								if (!(key == "LeoLinusCooking"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(240, 128, 16, 16), 9999f, 1, 1, new Vector2(29f, 8.5f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									layerDepth = 1f
								});
								for (int smokePuffs = 0; smokePuffs < 10; smokePuffs++)
								{
									Utility.addSmokePuff(location, new Vector2(29.5f, 8.6f) * 64f, smokePuffs * 500, 2f, 0.02f, 0.75f, 0.002f);
								}
								return;
							case 'q':
								if (!(key == "parrotHutSquawk"))
								{
									return;
								}
								(location as IslandHut).parrotUpgradePerches[0].timeUntilSqwawk = 1f;
								return;
							case 'r':
							{
								if (!(key == "coldstarMiracle"))
								{
									return;
								}
								MovieData data2;
								if (!MovieTheater.TryGetMovieData("winter_movie_0", out data2))
								{
									Game1.log.Error("Can't find data for movie 'winter_movie_0'.", null);
									return;
								}
								Microsoft.Xna.Framework.Rectangle sourceRect3 = MovieTheater.GetSourceRectForScreen(data2.SheetIndex, 9);
								location.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.temporaryContent.Load<Texture2D>(data2.Texture ?? "LooseSprites\\Movies"),
									sourceRect = sourceRect3,
									sourceRectStartingPos = new Vector2((float)sourceRect3.X, (float)sourceRect3.Y),
									animationLength = 1,
									totalNumberOfLoops = 1,
									interval = 99999f,
									alpha = 0.01f,
									alphaFade = -0.01f,
									scale = 4f,
									position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
									layerDepth = 0.8535f,
									id = 989
								});
								return;
							}
							case 's':
								if (!(key == "LeoWillyFishing"))
								{
									return;
								}
								for (int i20 = 0; i20 < 20; i20++)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite(0, new Vector2(42.5f, 38f) * 64f + new Vector2((float)Game1.random.Next(64), (float)Game1.random.Next(64)), Color.White * 0.7f, 8, false, 100f, 0, -1, -1f, -1, 0)
									{
										layerDepth = (float)(1280 + i20) / 10000f,
										delayBeforeAnimationStart = i20 * 150
									});
								}
								return;
							default:
								return;
							}
						}
						else
						{
							if (!(key == "shaneCliffProps"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(104f, 96f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 999
							});
							return;
						}
					}
					else
					{
						if (!(key == "BoatParrotLeave"))
						{
							return;
						}
						TemporaryAnimatedSprite temporaryAnimatedSprite18 = this.aboveMapSprites[0];
						temporaryAnimatedSprite18.motion = new Vector2(4f, -6f);
						temporaryAnimatedSprite18.sourceRect.X = 48;
						temporaryAnimatedSprite18.sourceRectStartingPos.X = 48f;
						temporaryAnimatedSprite18.animationLength = 3;
						temporaryAnimatedSprite18.pingPong = true;
						return;
					}
					break;
				}
				case 16:
				{
					char c = key[0];
					if (c <= 'a')
					{
						if (c != 'B')
						{
							if (c != 'E')
							{
								if (c != 'a')
								{
									return;
								}
								if (!(key == "abbyOuijaCandles"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(5f, 9f) * 64f, false, false)
								{
									lightId = this.GenerateLightSourceId("abbyOuijaCandles_1"),
									lightRadius = 1f
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(7f, 8f) * 64f, false, false)
								{
									lightId = this.GenerateLightSourceId("abbyOuijaCandles_2"),
									lightRadius = 1f
								});
								return;
							}
							else
							{
								if (!(key == "EmilyBoomBoxStop"))
								{
									return;
								}
								location.getTemporarySpriteByID(999).pulse = false;
								location.getTemporarySpriteByID(999).scale = 4f;
								return;
							}
						}
						else
						{
							if (!(key == "BoatParrotSquawk"))
							{
								return;
							}
							TemporaryAnimatedSprite temporaryAnimatedSprite19 = this.aboveMapSprites[0];
							temporaryAnimatedSprite19.sourceRect.X = 24;
							temporaryAnimatedSprite19.sourceRectStartingPos.X = 24f;
							Game1.playSound("parrot_squawk", null);
							return;
						}
					}
					else
					{
						switch (c)
						{
						case 'i':
							if (!(key == "islandFishSplash"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 544, 16, 16), 100000f, 1, 1, new Vector2(81f, 92f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 9999,
								motion = new Vector2(-2f, -8f),
								acceleration = new Vector2(0f, 0.2f),
								flipped = true,
								rotationChange = -0.02f,
								yStopCoordinate = 5952,
								layerDepth = 0.99f,
								reachedStopCoordinate = delegate(int param1)
								{
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, location.getTemporarySpriteByID(9999).position, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										layerDepth = 1f
									});
									location.removeTemporarySpritesWithID(9999);
									Game1.playSound("waterSlosh", null);
								}
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, new Vector2(81f, 92f) * 64f, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								layerDepth = 1f
							});
							return;
						case 'j':
						case 'k':
							break;
						case 'l':
						{
							if (!(key == "leahHoldPainting"))
							{
								return;
							}
							Texture2D tempTxture7 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							TemporaryAnimatedSprite temporarySpriteByID6 = location.getTemporarySpriteByID(999);
							temporarySpriteByID6.sourceRect.X = temporarySpriteByID6.sourceRect.X + 15;
							TemporaryAnimatedSprite temporarySpriteByID7 = location.getTemporarySpriteByID(999);
							temporarySpriteByID7.sourceRectStartingPos.X = temporarySpriteByID7.sourceRectStartingPos.X + 15f;
							int whichPainting = Game1.netWorldState.Value.hasWorldStateID("m_painting0") ? 0 : (Game1.netWorldState.Value.hasWorldStateID("m_painting1") ? 1 : 2);
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture7,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(400 + whichPainting * 25, 394, 25, 23),
								animationLength = 1,
								sourceRectStartingPos = new Vector2((float)(400 + whichPainting * 25), 394f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(73f, 38f) * 64f + new Vector2(-2f, -16f) * 4f,
								scale = 4f,
								layerDepth = 1f,
								id = 777
							});
							return;
						}
						case 'm':
						{
							if (!(key == "moonlightJellies"))
							{
								return;
							}
							int lightIndex2 = 1;
							this.showGroundObjects = false;
							List<NPCController> list = this.npcControllers;
							if (list != null)
							{
								list.Clear();
							}
							TemporaryAnimatedSpriteList temporaryAnimatedSpriteList = new TemporaryAnimatedSpriteList();
							TemporaryAnimatedSprite temporaryAnimatedSprite20 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(26f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite20.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite20.xPeriodic = true;
							temporaryAnimatedSprite20.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite20.xPeriodicRange = 16f;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite20.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite20.lightcolor = Color.Black;
							temporaryAnimatedSprite20.lightRadius = 1f;
							temporaryAnimatedSprite20.yStopCoordinate = 2560;
							temporaryAnimatedSprite20.delayBeforeAnimationStart = 10000;
							temporaryAnimatedSprite20.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite20);
							TemporaryAnimatedSprite temporaryAnimatedSprite21 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(29f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite21.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite21.xPeriodic = true;
							temporaryAnimatedSprite21.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite21.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite21.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite21.lightcolor = Color.Black;
							temporaryAnimatedSprite21.lightRadius = 1f;
							temporaryAnimatedSprite21.yStopCoordinate = 2560;
							temporaryAnimatedSprite21.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite21);
							TemporaryAnimatedSprite temporaryAnimatedSprite22 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(31f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite22.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite22.xPeriodic = true;
							temporaryAnimatedSprite22.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite22.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite22.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite22.lightcolor = Color.Black;
							temporaryAnimatedSprite22.lightRadius = 1f;
							temporaryAnimatedSprite22.yStopCoordinate = 2624;
							temporaryAnimatedSprite22.delayBeforeAnimationStart = 12000;
							temporaryAnimatedSprite22.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite22);
							TemporaryAnimatedSprite temporaryAnimatedSprite23 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(20f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite23.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite23.xPeriodic = true;
							temporaryAnimatedSprite23.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite23.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite23.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite23.lightcolor = Color.Black;
							temporaryAnimatedSprite23.lightRadius = 1f;
							temporaryAnimatedSprite23.yStopCoordinate = 1728;
							temporaryAnimatedSprite23.delayBeforeAnimationStart = 14000;
							temporaryAnimatedSprite23.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite23);
							TemporaryAnimatedSprite temporaryAnimatedSprite24 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite24.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite24.xPeriodic = true;
							temporaryAnimatedSprite24.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite24.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite24.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite24.lightcolor = Color.Black;
							temporaryAnimatedSprite24.lightRadius = 1f;
							temporaryAnimatedSprite24.yStopCoordinate = 1856;
							temporaryAnimatedSprite24.delayBeforeAnimationStart = 19500;
							temporaryAnimatedSprite24.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite24);
							TemporaryAnimatedSprite temporaryAnimatedSprite25 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite25.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite25.xPeriodic = true;
							temporaryAnimatedSprite25.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite25.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite25.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite25.lightcolor = Color.Black;
							temporaryAnimatedSprite25.lightRadius = 1f;
							temporaryAnimatedSprite25.yStopCoordinate = 2048;
							temporaryAnimatedSprite25.delayBeforeAnimationStart = 20300;
							temporaryAnimatedSprite25.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite25);
							TemporaryAnimatedSprite temporaryAnimatedSprite26 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite26.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite26.xPeriodic = true;
							temporaryAnimatedSprite26.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite26.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite26.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite26.lightcolor = Color.Black;
							temporaryAnimatedSprite26.lightRadius = 1f;
							temporaryAnimatedSprite26.yStopCoordinate = 2496;
							temporaryAnimatedSprite26.delayBeforeAnimationStart = 21500;
							temporaryAnimatedSprite26.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite26);
							TemporaryAnimatedSprite temporaryAnimatedSprite27 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite27.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite27.xPeriodic = true;
							temporaryAnimatedSprite27.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite27.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite27.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite27.lightcolor = Color.Black;
							temporaryAnimatedSprite27.lightRadius = 1f;
							temporaryAnimatedSprite27.yStopCoordinate = 2816;
							temporaryAnimatedSprite27.delayBeforeAnimationStart = 22400;
							temporaryAnimatedSprite27.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite27);
							TemporaryAnimatedSprite temporaryAnimatedSprite28 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(12f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite28.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite28.xPeriodic = true;
							temporaryAnimatedSprite28.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite28.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite28.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite28.lightcolor = Color.Black;
							temporaryAnimatedSprite28.lightRadius = 1f;
							temporaryAnimatedSprite28.yStopCoordinate = 2688;
							temporaryAnimatedSprite28.delayBeforeAnimationStart = 23200;
							temporaryAnimatedSprite28.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite28);
							TemporaryAnimatedSprite temporaryAnimatedSprite29 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(9f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite29.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite29.xPeriodic = true;
							temporaryAnimatedSprite29.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite29.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite29.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite29.lightcolor = Color.Black;
							temporaryAnimatedSprite29.lightRadius = 1f;
							temporaryAnimatedSprite29.yStopCoordinate = 2752;
							temporaryAnimatedSprite29.delayBeforeAnimationStart = 24000;
							temporaryAnimatedSprite29.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite29);
							TemporaryAnimatedSprite temporaryAnimatedSprite30 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(18f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite30.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite30.xPeriodic = true;
							temporaryAnimatedSprite30.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite30.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite30.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite30.lightcolor = Color.Black;
							temporaryAnimatedSprite30.lightRadius = 1f;
							temporaryAnimatedSprite30.yStopCoordinate = 1920;
							temporaryAnimatedSprite30.delayBeforeAnimationStart = 24600;
							temporaryAnimatedSprite30.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite30);
							TemporaryAnimatedSprite temporaryAnimatedSprite31 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(33f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite31.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite31.xPeriodic = true;
							temporaryAnimatedSprite31.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite31.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite31.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite31.lightcolor = Color.Black;
							temporaryAnimatedSprite31.lightRadius = 1f;
							temporaryAnimatedSprite31.yStopCoordinate = 2560;
							temporaryAnimatedSprite31.delayBeforeAnimationStart = 25600;
							temporaryAnimatedSprite31.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite31);
							TemporaryAnimatedSprite temporaryAnimatedSprite32 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(36f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite32.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite32.xPeriodic = true;
							temporaryAnimatedSprite32.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite32.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite32.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite32.lightcolor = Color.Black;
							temporaryAnimatedSprite32.lightRadius = 1f;
							temporaryAnimatedSprite32.yStopCoordinate = 2496;
							temporaryAnimatedSprite32.delayBeforeAnimationStart = 26900;
							temporaryAnimatedSprite32.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite32);
							TemporaryAnimatedSprite temporaryAnimatedSprite33 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(21f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite33.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite33.xPeriodic = true;
							temporaryAnimatedSprite33.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite33.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite33.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite33.lightcolor = Color.Black;
							temporaryAnimatedSprite33.lightRadius = 1f;
							temporaryAnimatedSprite33.yStopCoordinate = 2176;
							temporaryAnimatedSprite33.delayBeforeAnimationStart = 28000;
							temporaryAnimatedSprite33.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite33);
							TemporaryAnimatedSprite temporaryAnimatedSprite34 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(20f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite34.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite34.xPeriodic = true;
							temporaryAnimatedSprite34.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite34.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite34.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite34.lightcolor = Color.Black;
							temporaryAnimatedSprite34.lightRadius = 1f;
							temporaryAnimatedSprite34.yStopCoordinate = 2240;
							temporaryAnimatedSprite34.delayBeforeAnimationStart = 28500;
							temporaryAnimatedSprite34.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite34);
							TemporaryAnimatedSprite temporaryAnimatedSprite35 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(22f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite35.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite35.xPeriodic = true;
							temporaryAnimatedSprite35.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite35.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite35.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite35.lightcolor = Color.Black;
							temporaryAnimatedSprite35.lightRadius = 1f;
							temporaryAnimatedSprite35.yStopCoordinate = 2304;
							temporaryAnimatedSprite35.delayBeforeAnimationStart = 28500;
							temporaryAnimatedSprite35.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite35);
							TemporaryAnimatedSprite temporaryAnimatedSprite36 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(33f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite36.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite36.xPeriodic = true;
							temporaryAnimatedSprite36.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite36.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite36.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite36.lightcolor = Color.Black;
							temporaryAnimatedSprite36.lightRadius = 1f;
							temporaryAnimatedSprite36.yStopCoordinate = 2752;
							temporaryAnimatedSprite36.delayBeforeAnimationStart = 29000;
							temporaryAnimatedSprite36.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite36);
							TemporaryAnimatedSprite temporaryAnimatedSprite37 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(36f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite37.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite37.xPeriodic = true;
							temporaryAnimatedSprite37.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite37.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite37.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite37.lightcolor = Color.Black;
							temporaryAnimatedSprite37.lightRadius = 1f;
							temporaryAnimatedSprite37.yStopCoordinate = 2752;
							temporaryAnimatedSprite37.delayBeforeAnimationStart = 30000;
							temporaryAnimatedSprite37.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite37);
							TemporaryAnimatedSprite temporaryAnimatedSprite38 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 32, 16, 16), 250f, 3, 9999, new Vector2(28f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite38.motion = new Vector2(-0.5f, -0.5f);
							temporaryAnimatedSprite38.xPeriodic = true;
							temporaryAnimatedSprite38.xPeriodicLoopTime = 4000f;
							temporaryAnimatedSprite38.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite38.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite38.lightcolor = Color.Black;
							temporaryAnimatedSprite38.lightRadius = 2f;
							temporaryAnimatedSprite38.xStopCoordinate = 1216;
							temporaryAnimatedSprite38.yStopCoordinate = 2432;
							temporaryAnimatedSprite38.delayBeforeAnimationStart = 32000;
							temporaryAnimatedSprite38.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite38);
							TemporaryAnimatedSprite temporaryAnimatedSprite39 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(40f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite39.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite39.xPeriodic = true;
							temporaryAnimatedSprite39.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite39.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite39.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite39.lightcolor = Color.Black;
							temporaryAnimatedSprite39.lightRadius = 1f;
							temporaryAnimatedSprite39.yStopCoordinate = 2560;
							temporaryAnimatedSprite39.delayBeforeAnimationStart = 10000;
							temporaryAnimatedSprite39.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite39);
							TemporaryAnimatedSprite temporaryAnimatedSprite40 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(42f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite40.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite40.xPeriodic = true;
							temporaryAnimatedSprite40.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite40.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite40.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite40.lightcolor = Color.Black;
							temporaryAnimatedSprite40.lightRadius = 1f;
							temporaryAnimatedSprite40.yStopCoordinate = 2752;
							temporaryAnimatedSprite40.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite40);
							TemporaryAnimatedSprite temporaryAnimatedSprite41 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(43f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite41.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite41.xPeriodic = true;
							temporaryAnimatedSprite41.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite41.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite41.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite41.lightcolor = Color.Black;
							temporaryAnimatedSprite41.lightRadius = 1f;
							temporaryAnimatedSprite41.yStopCoordinate = 2624;
							temporaryAnimatedSprite41.delayBeforeAnimationStart = 12000;
							temporaryAnimatedSprite41.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite41);
							TemporaryAnimatedSprite temporaryAnimatedSprite42 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(45f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite42.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite42.xPeriodic = true;
							temporaryAnimatedSprite42.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite42.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite42.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite42.lightcolor = Color.Black;
							temporaryAnimatedSprite42.lightRadius = 1f;
							temporaryAnimatedSprite42.yStopCoordinate = 2496;
							temporaryAnimatedSprite42.delayBeforeAnimationStart = 14000;
							temporaryAnimatedSprite42.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite42);
							TemporaryAnimatedSprite temporaryAnimatedSprite43 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(46f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite43.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite43.xPeriodic = true;
							temporaryAnimatedSprite43.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite43.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite43.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite43.lightcolor = Color.Black;
							temporaryAnimatedSprite43.lightRadius = 1f;
							temporaryAnimatedSprite43.yStopCoordinate = 1856;
							temporaryAnimatedSprite43.delayBeforeAnimationStart = 19500;
							temporaryAnimatedSprite43.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite43);
							TemporaryAnimatedSprite temporaryAnimatedSprite44 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(48f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite44.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite44.xPeriodic = true;
							temporaryAnimatedSprite44.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite44.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite44.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite44.lightcolor = Color.Black;
							temporaryAnimatedSprite44.lightRadius = 1f;
							temporaryAnimatedSprite44.yStopCoordinate = 2240;
							temporaryAnimatedSprite44.delayBeforeAnimationStart = 20300;
							temporaryAnimatedSprite44.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite44);
							TemporaryAnimatedSprite temporaryAnimatedSprite45 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(49f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite45.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite45.xPeriodic = true;
							temporaryAnimatedSprite45.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite45.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite45.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite45.lightcolor = Color.Black;
							temporaryAnimatedSprite45.lightRadius = 1f;
							temporaryAnimatedSprite45.yStopCoordinate = 2560;
							temporaryAnimatedSprite45.delayBeforeAnimationStart = 21500;
							temporaryAnimatedSprite45.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite45);
							TemporaryAnimatedSprite temporaryAnimatedSprite46 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(50f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite46.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite46.xPeriodic = true;
							temporaryAnimatedSprite46.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite46.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite46.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite46.lightcolor = Color.Black;
							temporaryAnimatedSprite46.lightRadius = 1f;
							temporaryAnimatedSprite46.yStopCoordinate = 1920;
							temporaryAnimatedSprite46.delayBeforeAnimationStart = 22400;
							temporaryAnimatedSprite46.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite46);
							TemporaryAnimatedSprite temporaryAnimatedSprite47 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(51f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite47.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite47.xPeriodic = true;
							temporaryAnimatedSprite47.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite47.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite47.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite47.lightcolor = Color.Black;
							temporaryAnimatedSprite47.lightRadius = 1f;
							temporaryAnimatedSprite47.yStopCoordinate = 2112;
							temporaryAnimatedSprite47.delayBeforeAnimationStart = 23200;
							temporaryAnimatedSprite47.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite47);
							TemporaryAnimatedSprite temporaryAnimatedSprite48 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(52f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite48.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite48.xPeriodic = true;
							temporaryAnimatedSprite48.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite48.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite48.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite48.lightcolor = Color.Black;
							temporaryAnimatedSprite48.lightRadius = 1f;
							temporaryAnimatedSprite48.yStopCoordinate = 2432;
							temporaryAnimatedSprite48.delayBeforeAnimationStart = 24000;
							temporaryAnimatedSprite48.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite48);
							TemporaryAnimatedSprite temporaryAnimatedSprite49 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(53f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite49.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite49.xPeriodic = true;
							temporaryAnimatedSprite49.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite49.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite49.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite49.lightcolor = Color.Black;
							temporaryAnimatedSprite49.lightRadius = 1f;
							temporaryAnimatedSprite49.yStopCoordinate = 2240;
							temporaryAnimatedSprite49.delayBeforeAnimationStart = 24600;
							temporaryAnimatedSprite49.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite49);
							TemporaryAnimatedSprite temporaryAnimatedSprite50 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(54f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite50.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite50.xPeriodic = true;
							temporaryAnimatedSprite50.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite50.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite50.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite50.lightcolor = Color.Black;
							temporaryAnimatedSprite50.lightRadius = 1f;
							temporaryAnimatedSprite50.yStopCoordinate = 1920;
							temporaryAnimatedSprite50.delayBeforeAnimationStart = 25600;
							temporaryAnimatedSprite50.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite50);
							TemporaryAnimatedSprite temporaryAnimatedSprite51 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(55f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite51.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite51.xPeriodic = true;
							temporaryAnimatedSprite51.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite51.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite51.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite51.lightcolor = Color.Black;
							temporaryAnimatedSprite51.lightRadius = 1f;
							temporaryAnimatedSprite51.yStopCoordinate = 2560;
							temporaryAnimatedSprite51.delayBeforeAnimationStart = 26900;
							temporaryAnimatedSprite51.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite51);
							TemporaryAnimatedSprite temporaryAnimatedSprite52 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(4f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite52.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite52.xPeriodic = true;
							temporaryAnimatedSprite52.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite52.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite52.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite52.lightcolor = Color.Black;
							temporaryAnimatedSprite52.lightRadius = 1f;
							temporaryAnimatedSprite52.yStopCoordinate = 1920;
							temporaryAnimatedSprite52.delayBeforeAnimationStart = 24000;
							temporaryAnimatedSprite52.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite52);
							TemporaryAnimatedSprite temporaryAnimatedSprite53 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(5f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite53.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite53.xPeriodic = true;
							temporaryAnimatedSprite53.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite53.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite53.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite53.lightcolor = Color.Black;
							temporaryAnimatedSprite53.lightRadius = 1f;
							temporaryAnimatedSprite53.yStopCoordinate = 2560;
							temporaryAnimatedSprite53.delayBeforeAnimationStart = 24600;
							temporaryAnimatedSprite53.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite53);
							TemporaryAnimatedSprite temporaryAnimatedSprite54 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(3f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite54.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite54.xPeriodic = true;
							temporaryAnimatedSprite54.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite54.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite54.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite54.lightcolor = Color.Black;
							temporaryAnimatedSprite54.lightRadius = 1f;
							temporaryAnimatedSprite54.yStopCoordinate = 2176;
							temporaryAnimatedSprite54.delayBeforeAnimationStart = 25600;
							temporaryAnimatedSprite54.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite54);
							TemporaryAnimatedSprite temporaryAnimatedSprite55 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(6f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite55.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite55.xPeriodic = true;
							temporaryAnimatedSprite55.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite55.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite55.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite55.lightcolor = Color.Black;
							temporaryAnimatedSprite55.lightRadius = 1f;
							temporaryAnimatedSprite55.yStopCoordinate = 2368;
							temporaryAnimatedSprite55.delayBeforeAnimationStart = 26900;
							temporaryAnimatedSprite55.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite55);
							TemporaryAnimatedSprite temporaryAnimatedSprite56 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(8f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite56.motion = new Vector2(0f, -1f);
							temporaryAnimatedSprite56.xPeriodic = true;
							temporaryAnimatedSprite56.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite56.xPeriodicRange = 16f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite56.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite56.lightcolor = Color.Black;
							temporaryAnimatedSprite56.lightRadius = 1f;
							temporaryAnimatedSprite56.yStopCoordinate = 2688;
							temporaryAnimatedSprite56.delayBeforeAnimationStart = 26900;
							temporaryAnimatedSprite56.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite56);
							TemporaryAnimatedSprite temporaryAnimatedSprite57 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(50f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite57.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite57.xPeriodic = true;
							temporaryAnimatedSprite57.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite57.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite57.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite57.lightcolor = Color.Black;
							temporaryAnimatedSprite57.lightRadius = 1f;
							temporaryAnimatedSprite57.yStopCoordinate = 2688;
							temporaryAnimatedSprite57.delayBeforeAnimationStart = 28500;
							temporaryAnimatedSprite57.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite57);
							TemporaryAnimatedSprite temporaryAnimatedSprite58 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(51f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite58.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite58.xPeriodic = true;
							temporaryAnimatedSprite58.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite58.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite58.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite58.lightcolor = Color.Black;
							temporaryAnimatedSprite58.lightRadius = 1f;
							temporaryAnimatedSprite58.yStopCoordinate = 2752;
							temporaryAnimatedSprite58.delayBeforeAnimationStart = 28500;
							temporaryAnimatedSprite58.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite58);
							TemporaryAnimatedSprite temporaryAnimatedSprite59 = new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(52f, 49f) * 64f, false, false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
							temporaryAnimatedSprite59.motion = new Vector2(0f, -1.5f);
							temporaryAnimatedSprite59.xPeriodic = true;
							temporaryAnimatedSprite59.xPeriodicLoopTime = 2500f;
							temporaryAnimatedSprite59.xPeriodicRange = 10f;
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("moonlightJellies_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(lightIndex2++);
							temporaryAnimatedSprite59.lightId = this.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
							temporaryAnimatedSprite59.lightcolor = Color.Black;
							temporaryAnimatedSprite59.lightRadius = 1f;
							temporaryAnimatedSprite59.yStopCoordinate = 2816;
							temporaryAnimatedSprite59.delayBeforeAnimationStart = 29000;
							temporaryAnimatedSprite59.pingPong = true;
							temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite59);
							this.underwaterSprites = temporaryAnimatedSpriteList;
							return;
						}
						default:
							if (c != 't')
							{
								if (c != 'w')
								{
									return;
								}
								if (!(key == "wizardSewerMagic"))
								{
									return;
								}
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(15f, 13f) * 64f + new Vector2(8f, 0f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("wizardSewerMagic_1"),
									lightRadius = 1f,
									lightcolor = Color.Black,
									alphaFade = 0.005f
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(17f, 13f) * 64f + new Vector2(8f, 0f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									lightId = this.GenerateLightSourceId("wizardSewerMagic_2"),
									lightRadius = 1f,
									lightcolor = Color.Black,
									alphaFade = 0.005f
								});
								return;
							}
							else
							{
								if (!(key == "trashBearPrelude"))
								{
									return;
								}
								Utility.addStarsAndSpirals(location, 95, 106, 23, 4, 10000, 275, Color.Lime, null, false);
								return;
							}
							break;
						}
					}
					break;
				}
				case 17:
				{
					char c = key[0];
					if (c <= 'd')
					{
						if (c != 'E')
						{
							if (c != 'd')
							{
								return;
							}
							if (!(key == "doneWithSlideShow"))
							{
								return;
							}
							(location as Summit).isShowingEndSlideshow = false;
							return;
						}
						else
						{
							if (!(key == "EmilyBoomBoxStart"))
							{
								return;
							}
							location.getTemporarySpriteByID(999).pulse = true;
							location.getTemporarySpriteByID(999).pulseTime = 420f;
							return;
						}
					}
					else if (c != 'l')
					{
						if (c != 'm')
						{
							if (c != 's')
							{
								return;
							}
							if (!(key == "springOnionRemove"))
							{
								return;
							}
							location.removeTemporarySpritesWithID(777);
							return;
						}
						else
						{
							if (!(key == "maruElectrocution"))
							{
								return;
							}
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1664, 16, 32), 40f, 1, 20, new Vector2(7f, 5f) * 64f - new Vector2(-4f, 8f), true, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
							return;
						}
					}
					else
					{
						if (!(key == "leahPaintingSetup"))
						{
							return;
						}
						Texture2D tempTxture8 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture8,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(368f, 393f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(72f, 38f) * 64f + new Vector2(3f, -13f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 999
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture8,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(368f, 393f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(74f, 40f) * 64f + new Vector2(3f, -17f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 888
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture8,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(369, 424, 11, 15),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(369f, 424f),
							interval = 9999f,
							totalNumberOfLoops = 99999,
							position = new Vector2(75f, 40f) * 64f + new Vector2(-2f, -11f) * 4f,
							scale = 4f,
							layerDepth = 0.01f,
							id = 444
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 1822, 32, 34),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(96f, 1822f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(79f, 36f) * 64f,
							scale = 4f,
							layerDepth = 0.1f
						});
						return;
					}
					break;
				}
				case 18:
				{
					char c = key[10];
					if (c <= 'a')
					{
						if (c != 'P')
						{
							if (c != 'a')
							{
								return;
							}
							if (!(key == "terraria_cat_leave"))
							{
								return;
							}
							TemporaryAnimatedSprite terraria_cat = location.getTemporarySpriteByID(777);
							if (terraria_cat != null)
							{
								terraria_cat.sourceRect.Y = 0;
								terraria_cat.sourceRect.X = terraria_cat.currentParentTileIndex * 16;
								terraria_cat.paused = false;
								terraria_cat.motion = new Vector2(1f, 0f);
								terraria_cat.xStopCoordinate = 1152;
								terraria_cat.flipped = true;
								Microsoft.Xna.Framework.Rectangle warpRect2 = new Microsoft.Xna.Framework.Rectangle(1024, 120, 144, 272);
								terraria_cat.reachedStopCoordinate = delegate(int param2)
								{
									terraria_cat.position.X = -4000f;
									location.removeTemporarySpritesWithID(888);
									Game1.playSound("terraria_warp", null);
									for (int i23 = 0; i23 < 80; i23++)
									{
										Vector2 warpSparklePos2 = Utility.getRandomPositionInThisRectangle(warpRect2, Game1.random);
										Vector2 warpSkarleMotion2 = warpSparklePos2 - Utility.PointToVector2(warpRect2.Center);
										warpSkarleMotion2.Normalize();
										warpSkarleMotion2 *= (float)(Game1.random.Next(10, 21) / 10);
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(113 + Game1.random.Next(3) * 5, 123, 5, 5), 999f, 1, 9999, warpSparklePos2, false, false, 0.8f, 0.02f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											layerDepth = 0.99f,
											rotationChange = (float)Game1.random.Next(-10, 10) / 100f,
											motion = warpSkarleMotion2,
											acceleration = -warpSkarleMotion2 / 150f,
											scaleChange = (float)Game1.random.Next(-10, 0) / 500f,
											delayBeforeAnimationStart = i23 * 5
										});
									}
								};
								return;
							}
						}
						else
						{
							if (!(key == "farmerHoldPainting"))
							{
								return;
							}
							Texture2D tempTxture9 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							TemporaryAnimatedSprite temporarySpriteByID8 = location.getTemporarySpriteByID(888);
							temporarySpriteByID8.sourceRect.X = temporarySpriteByID8.sourceRect.X + 15;
							TemporaryAnimatedSprite temporarySpriteByID9 = location.getTemporarySpriteByID(888);
							temporarySpriteByID9.sourceRectStartingPos.X = temporarySpriteByID9.sourceRectStartingPos.X + 15f;
							location.removeTemporarySpritesWithID(444);
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture9,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(476, 394, 25, 22),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(476f, 394f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(75f, 40f) * 64f + new Vector2(-4f, -33f) * 4f,
								scale = 4f,
								layerDepth = 1f,
								id = 777
							});
							return;
						}
					}
					else
					{
						switch (c)
						{
						case 'e':
							if (!(key == "movieTheater_setup"))
							{
								return;
							}
							Game1.currentLightSources.Add(new LightSource("Event_MovieProjector", 7, new Vector2(192f, 64f) + new Vector2(64f, 80f) * 4f, 4f, LightSource.LightContext.None, 0L, null));
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("Maps\\MovieTheaterScreen_TileSheet"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(224, 0, 96, 112),
								sourceRectStartingPos = new Vector2(224f, 0f),
								animationLength = 1,
								interval = 5000f,
								totalNumberOfLoops = 9999,
								scale = 4f,
								position = new Vector2(4f, 4f) * 64f,
								layerDepth = 1f,
								id = 999,
								delayBeforeAnimationStart = 7950
							});
							return;
						case 'f':
						case 'g':
							break;
						case 'h':
						{
							if (key == "harveyKitchenFlame")
							{
								location.TemporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.mouseCursors,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
									animationLength = 4,
									sourceRectStartingPos = new Vector2(276f, 1985f),
									interval = 100f,
									totalNumberOfLoops = 6,
									position = new Vector2(22f, 22f) * 64f + new Vector2(8f, 5f) * 4f,
									scale = 4f,
									layerDepth = 0.15584001f
								});
								return;
							}
							if (!(key == "harveyKitchenSetup"))
							{
								return;
							}
							Texture2D tempTxture10 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture10,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(379, 251, 31, 13),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(379f, 251f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(22f, 22f) * 64f + new Vector2(-2f, 6f) * 4f,
								scale = 4f,
								layerDepth = 0.15551999f
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture10,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(391, 235, 5, 13),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(391f, 235f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(21f, 22f) * 64f + new Vector2(8f, 4f) * 4f,
								scale = 4f,
								layerDepth = 0.15551999f
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture10,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(399, 229, 11, 21),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(399f, 229f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(19f, 22f) * 64f + new Vector2(8f, -5f) * 4f,
								scale = 4f,
								layerDepth = 0.15551999f
							});
							location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(21f, 22f) * 64f + new Vector2(0f, -5f) * 4f, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0)
							{
								totalNumberOfLoops = 999,
								layerDepth = 0.15616f
							});
							location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(21f, 22f) * 64f + new Vector2(24f, -5f) * 4f, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0)
							{
								totalNumberOfLoops = 999,
								flipped = true,
								delayBeforeAnimationStart = 400,
								layerDepth = 0.15616f
							});
							return;
						}
						case 'i':
						{
							if (!(key == "missingJunimoStars"))
							{
								return;
							}
							location.removeTemporarySpritesWithID(999);
							Texture2D tempTxture11 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							for (int i21 = 0; i21 < 48; i21++)
							{
								location.TemporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = tempTxture11,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(477, 306, 28, 28),
									sourceRectStartingPos = new Vector2(477f, 306f),
									animationLength = 1,
									interval = 5000f,
									totalNumberOfLoops = 10,
									scale = (float)Game1.random.Next(1, 5),
									position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 84, 84, 0, 0) + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)),
									rotationChange = 3.1415927f / (float)Game1.random.Next(16, 128),
									motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(20, 90) * -0.1f),
									acceleration = new Vector2(0f, 0.05f),
									local = true,
									layerDepth = (float)i21 / 100f,
									color = (Game1.random.NextBool() ? Color.White : Utility.getRandomRainbowColor(null))
								});
							}
							return;
						}
						default:
							if (c != 'm')
							{
								switch (c)
								{
								case 'r':
								{
									if (!(key == "sebastianFrogHouse"))
									{
										return;
									}
									Point frog_spot = (location as FarmHouse).GetSpouseRoomCorner();
									frog_spot.X++;
									frog_spot.Y += 6;
									Vector2 spot = Utility.PointToVector2(frog_spot);
									location.TemporarySprites.Add(new TemporaryAnimatedSprite
									{
										texture = Game1.mouseCursors,
										sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
										animationLength = 1,
										sourceRectStartingPos = new Vector2(641f, 1534f),
										interval = 5000f,
										totalNumberOfLoops = 9999,
										position = spot * 64f + new Vector2(0f, -5f) * 4f,
										scale = 4f,
										layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
									});
									Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
									location.TemporarySprites.Add(new TemporaryAnimatedSprite
									{
										texture = crittersText2,
										sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
										animationLength = 1,
										sourceRectStartingPos = new Vector2(0f, 224f),
										interval = 5000f,
										totalNumberOfLoops = 9999,
										position = spot * 64f + new Vector2(25f, 2f) * 4f,
										scale = 4f,
										flipped = true,
										layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
										id = 777
									});
									return;
								}
								case 's':
								{
									if (!(key == "farmerForestVision"))
									{
										return;
									}
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(393, 1973, 1, 1), 9999f, 1, 999999, new Vector2(0f, 0f) * 64f, false, false, 0.9f, 0f, Color.LimeGreen * 0.85f, (float)(Game1.viewport.Width * 2), 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.002f,
										id = 1
									});
									Game1.player.mailReceived.Add("canReadJunimoText");
									int x = -64;
									int y = -64;
									int index = 0;
									int yIndex = 0;
									while (y < Game1.viewport.Height + 128)
									{
										location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(367 + ((index % 2 == 0) ? 8 : 0), 1969, 8, 8), 9999f, 1, 999999, new Vector2((float)x, (float)y), false, false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
										{
											alpha = 0f,
											alphaFade = -0.0015f,
											xPeriodic = true,
											xPeriodicLoopTime = 4000f,
											xPeriodicRange = 64f,
											yPeriodic = true,
											yPeriodicLoopTime = 5000f,
											yPeriodicRange = 96f,
											rotationChange = (float)Game1.random.Next(-1, 2) * 3.1415927f / 256f,
											id = 1,
											delayBeforeAnimationStart = 20 * index
										});
										x += 128;
										if (x > Game1.viewport.Width + 64)
										{
											yIndex++;
											x = ((yIndex % 2 == 0) ? -64 : 64);
											y += 128;
										}
										index++;
									}
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width / 2 - 100), (float)(Game1.viewport.Height / 2 - 240)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 6000,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width / 4 - 100), (float)(Game1.viewport.Height / 4 - 120)), false, false, 0.99f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 9000,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width * 3 / 4), (float)(Game1.viewport.Height / 3 - 120)), false, false, 0.98f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 12000,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width / 3 - 60), (float)(Game1.viewport.Height * 3 / 4 - 120)), false, false, 0.97f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 15000,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width * 2 / 3), (float)(Game1.viewport.Height * 2 / 3 - 120)), false, false, 0.96f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 18000,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width / 8), (float)(Game1.viewport.Height / 5 - 120)), false, false, 0.95f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 19500,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width * 2 / 3), (float)(Game1.viewport.Height / 5 - 120)), false, false, 0.94f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
									{
										alpha = 0f,
										alphaFade = -0.001f,
										id = 1,
										delayBeforeAnimationStart = 21000,
										scaleChange = 0.004f,
										xPeriodic = true,
										xPeriodicLoopTime = 4000f,
										xPeriodicRange = 64f,
										yPeriodic = true,
										yPeriodicLoopTime = 5000f,
										yPeriodicRange = 32f
									});
									return;
								}
								case 't':
								{
									if (!(key == "raccoonbutterflies"))
									{
										return;
									}
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 336, 16, 16), new Vector2(52.5f, 0f) * 64f - new Vector2(131.5f, -60f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 32f,
										xPeriodicLoopTime = 2800f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 8f,
										yPeriodicLoopTime = 3800f,
										overrideLocationDestroy = true
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(192, 336, 16, 16), new Vector2(56.5f, 0f) * 64f - new Vector2(131.5f, 0f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 32f,
										xPeriodicLoopTime = 2600f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 4f,
										yPeriodicLoopTime = 2900f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 288, 16, 16), new Vector2(53.5f, 0f) * 64f + new Vector2(263f, 24f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 32f,
										xPeriodicLoopTime = 3000f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 6f,
										yPeriodicLoopTime = 3100f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(192, 288, 16, 16), new Vector2(52.5f, 0f) * 64f + new Vector2(131.5f, 220f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 32f,
										xPeriodicLoopTime = 2400f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 12f,
										yPeriodicLoopTime = 2800f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(64, 288, 16, 16), new Vector2(52.5f, 0f) * 64f + new Vector2(186.5f, 150f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 32f,
										xPeriodicLoopTime = 3400f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 4f,
										yPeriodicLoopTime = 3200f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 96, 16, 16), new Vector2(52.5f, 0f) * 64f + new Vector2(211.5f, 180f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 32f,
										xPeriodicLoopTime = 3500f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 4f,
										yPeriodicLoopTime = 2700f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(192, 112, 16, 16), new Vector2(52.5f, 0f) * 64f - new Vector2(126.5f, -120f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 16f,
										xPeriodicLoopTime = 2500f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 4f,
										yPeriodicLoopTime = 3300f
									});
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 288, 16, 16), new Vector2(49.5f, 0f) * 64f - new Vector2(126.5f, -100f) * 4f, false, 0f, Color.White)
									{
										drawAboveAlwaysFront = true,
										interval = 148f,
										animationLength = 4,
										pingPong = true,
										totalNumberOfLoops = 99999,
										id = 997799,
										scale = 4f,
										xPeriodic = true,
										xPeriodicRange = 16f,
										xPeriodicLoopTime = 2200f,
										alpha = 0.01f,
										alphaFade = -0.01f,
										yPeriodic = true,
										yPeriodicRange = 4f,
										yPeriodicLoopTime = 3400f
									});
									TemporaryAnimatedSprite temporarySpriteByID10 = location.getTemporarySpriteByID(9786);
									TemporaryAnimatedSprite mrs_raccoon2 = location.getTemporarySpriteByID(9785);
									temporarySpriteByID10.sourceRect.Y = 224;
									temporarySpriteByID10.sourceRectStartingPos.Y = 224f;
									temporarySpriteByID10.currentParentTileIndex = 3;
									temporarySpriteByID10.timer = 0f;
									temporarySpriteByID10.sourceRect.X = 96;
									mrs_raccoon2.sourceRect.Y = 224;
									mrs_raccoon2.sourceRectStartingPos.Y = 224f;
									mrs_raccoon2.currentParentTileIndex = 3;
									mrs_raccoon2.timer = 0f;
									mrs_raccoon2.sourceRect.X = 96;
									return;
								}
								default:
									return;
								}
							}
							else
							{
								if (!(key == "trashBearUmbrella1"))
								{
									return;
								}
								location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 80, 46, 56), new Vector2(102f, 94.5f) * 64f, false, 0f, Color.White)
								{
									animationLength = 1,
									interval = 999999f,
									motion = new Vector2(0f, -9f),
									acceleration = new Vector2(0f, 0.4f),
									scale = 4f,
									layerDepth = 1f,
									id = 777,
									yStopCoordinate = 6144,
									reachedStopCoordinate = delegate(int param)
									{
										location.getTemporarySpriteByID(777).yStopCoordinate = -1;
										location.getTemporarySpriteByID(777).motion = new Vector2(0f, (float)param * 0.75f);
										location.getTemporarySpriteByID(777).acceleration = new Vector2(0.04f, -0.19f);
										location.getTemporarySpriteByID(777).accelerationChange = new Vector2(0f, 0.0015f);
										TemporaryAnimatedSprite temporarySpriteByID13 = location.getTemporarySpriteByID(777);
										temporarySpriteByID13.sourceRect.X = temporarySpriteByID13.sourceRect.X + 46;
										location.playSound("batFlap", null, null, SoundContext.Default);
										location.playSound("tinyWhip", null, null, SoundContext.Default);
									}
								});
								return;
							}
							break;
						}
					}
					break;
				}
				case 19:
				{
					char c = key[0];
					if (c <= 'm')
					{
						if (c != 'E')
						{
							if (c != 'm')
							{
								return;
							}
							if (!(key == "movieTheater_screen"))
							{
								return;
							}
							string movieId2;
							string error7;
							int screenIndex;
							bool shake;
							if (!ArgUtility.TryGet(args, 2, out movieId2, out error7, true, "string movieId") || !ArgUtility.TryGetInt(args, 3, out screenIndex, out error7, "int screenIndex") || !ArgUtility.TryGetBool(args, 4, out shake, out error7, "bool shake"))
							{
								this.LogCommandError(args, error7, false);
								return;
							}
							movieId2 = MovieTheater.GetMovieIdFromLegacyIndex(movieId2);
							MovieData data3;
							if (!MovieTheater.TryGetMovieData(movieId2, out data3))
							{
								this.LogCommandError(args, "No movie found with ID '" + movieId2 + "'.", false);
								return;
							}
							Microsoft.Xna.Framework.Rectangle sourceRect4 = MovieTheater.GetSourceRectForScreen(data3.SheetIndex, screenIndex);
							location.removeTemporarySpritesWithIDLocal(998);
							if (screenIndex >= 0)
							{
								location.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.temporaryContent.Load<Texture2D>(data3.Texture ?? "LooseSprites\\Movies"),
									sourceRect = sourceRect4,
									sourceRectStartingPos = new Vector2((float)sourceRect4.X, (float)sourceRect4.Y),
									animationLength = 1,
									totalNumberOfLoops = 9999,
									interval = 5000f,
									scale = 4f,
									position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
									shakeIntensity = (shake ? 1f : 0f),
									layerDepth = 0.0128f,
									id = 998
								});
								return;
							}
						}
						else
						{
							if (!(key == "EmilySongBackLights"))
							{
								return;
							}
							this.aboveMapSprites = new TemporaryAnimatedSpriteList();
							for (int lightcolumns = 0; lightcolumns < 5; lightcolumns++)
							{
								for (int yPos2 = 0; yPos2 < Game1.graphics.GraphicsDevice.Viewport.Height + 48; yPos2 += 48)
								{
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(681, 1890, 18, 12), 42241f, 1, 1, new Vector2((float)((lightcolumns + 1) * Game1.graphics.GraphicsDevice.Viewport.Width / 5 - Game1.graphics.GraphicsDevice.Viewport.Width / 7), (float)(-24 + yPos2)), false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										xPeriodic = true,
										xPeriodicLoopTime = 1760f,
										xPeriodicRange = (float)(128 + yPos2 / 12 * 4),
										delayBeforeAnimationStart = lightcolumns * 100 + yPos2 / 4,
										local = true
									});
								}
							}
							for (int numFlyers = 0; numFlyers < 27; numFlyers++)
							{
								int flyerNumber = 0;
								int yPos3 = Game1.random.Next(64, Game1.graphics.GraphicsDevice.Viewport.Height - 64);
								int loopTime = Game1.random.Next(800, 2000);
								int loopRange = Game1.random.Next(32, 64);
								bool pulse = Game1.random.NextDouble() < 0.25;
								int speed = Game1.random.Next(-6, -3);
								for (int tails = 0; tails < 8; tails++)
								{
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(616 + flyerNumber * 10, 1891, 10, 10), 42241f, 1, 1, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width, (float)yPos3), false, false, 0.01f, 0f, Color.White * (1f - (float)tails * 0.11f), 4f, 0f, 0f, 0f, false)
									{
										yPeriodic = true,
										motion = new Vector2((float)speed, 0f),
										yPeriodicLoopTime = (float)loopTime,
										pulse = pulse,
										pulseTime = 440f,
										pulseAmount = 1.5f,
										yPeriodicRange = (float)loopRange,
										delayBeforeAnimationStart = 14000 + numFlyers * 900 + tails * 100,
										local = true
									});
								}
							}
							for (int numRainbows2 = 0; numRainbows2 < 15; numRainbows2++)
							{
								int it = 0;
								int yPos4 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width - 128);
								for (int xPos2 = Game1.graphics.GraphicsDevice.Viewport.Height; xPos2 >= -64; xPos2 -= 48)
								{
									this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2((float)yPos4, (float)xPos2), false, false, 1f, 0.02f, Color.White, 4f, 0f, -1.5707964f, 0f, false)
									{
										delayBeforeAnimationStart = 27500 + numRainbows2 * 880 + it * 25,
										local = true
									});
									it++;
								}
							}
							for (int numRainbows3 = 0; numRainbows3 < 120; numRainbows3++)
							{
								this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(626 + numRainbows3 / 28 * 10, 1891, 10, 10), 2000f, 1, 1, new Vector2((float)Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height)), false, false, 0.01f, 0f, Color.White, 0.1f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(0f, -2f),
									alphaFade = 0.002f,
									scaleChange = 0.5f,
									scaleChangeChange = -0.0085f,
									delayBeforeAnimationStart = 27500 + numRainbows3 * 110,
									local = true
								});
							}
							return;
						}
					}
					else if (c != 't')
					{
						if (c != 'w')
						{
							return;
						}
						if (!(key == "willyCrabExperiment"))
						{
							return;
						}
						Texture2D tempTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 250f,
							totalNumberOfLoops = 99999,
							id = 11,
							position = new Vector2(2f, 4f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 200f,
							totalNumberOfLoops = 99999,
							id = 1,
							initialPosition = new Vector2(2f, 6f) * 64f,
							yPeriodic = true,
							yPeriodicLoopTime = 8000f,
							yPeriodicRange = 32f,
							position = new Vector2(2f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 100f,
							totalNumberOfLoops = 99999,
							id = 11,
							position = new Vector2(1f, 5.75f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 100f,
							totalNumberOfLoops = 99999,
							id = 11,
							position = new Vector2(5f, 3f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 140f,
							totalNumberOfLoops = 99999,
							id = 22,
							position = new Vector2(4f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 140f,
							totalNumberOfLoops = 99999,
							id = 22,
							position = new Vector2(8.5f, 5f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 170f,
							totalNumberOfLoops = 99999,
							id = 222,
							position = new Vector2(6f, 3.25f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 190f,
							totalNumberOfLoops = 99999,
							id = 222,
							position = new Vector2(6f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 150f,
							totalNumberOfLoops = 99999,
							id = 222,
							position = new Vector2(7f, 4f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 200f,
							totalNumberOfLoops = 99999,
							id = 2,
							position = new Vector2(4f, 7f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 180f,
							totalNumberOfLoops = 99999,
							id = 3,
							position = new Vector2(8f, 6f) * 64f,
							yPeriodic = true,
							yPeriodicLoopTime = 10000f,
							yPeriodicRange = 32f,
							initialPosition = new Vector2(8f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 220f,
							totalNumberOfLoops = 99999,
							id = 33,
							position = new Vector2(9f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTexture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 150f,
							totalNumberOfLoops = 99999,
							id = 33,
							position = new Vector2(10f, 5f) * 64f,
							scale = 4f
						});
						return;
					}
					else
					{
						if (!(key == "terraria_warp_begin"))
						{
							return;
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 18, 36, 68), 90f, 3, 9999, new Vector2(16f, 5f) * 64f + new Vector2(0f, -50f) * 4f, false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							layerDepth = 0.8f,
							id = 888
						});
						TemporaryAnimatedSprite cat_sprite = new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 90f, 8, 9999, new Vector2(16f, 5f) * 64f + new Vector2(34f, -12f) * 4f, false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							id = 777,
							layerDepth = 0.85f,
							motion = new Vector2(-1f, 0f),
							delayBeforeAnimationStart = 1000,
							xStopCoordinate = 960
						};
						Action <>9__2;
						Action <>9__3;
						cat_sprite.reachedStopCoordinate = delegate(int param)
						{
							cat_sprite.paused = true;
							cat_sprite.sourceRect = new Microsoft.Xna.Framework.Rectangle(112, 16, 16, 16);
							Action func;
							if ((func = <>9__2) == null)
							{
								func = (<>9__2 = delegate()
								{
									Game1.playSound("terraria_meowmere", null);
									cat_sprite.shakeIntensity = 1f;
									location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\weapons", new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16), 1000f, 1, 1, new Vector2(15f, 5f) * 64f, false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
									{
										layerDepth = 0.86f,
										motion = new Vector2(-1f, -4f),
										acceleration = new Vector2(0f, 0.1f)
									});
								});
							}
							DelayedAction.functionAfterDelay(func, 1000);
							Action func2;
							if ((func2 = <>9__3) == null)
							{
								func2 = (<>9__3 = delegate()
								{
									cat_sprite.shakeIntensity = 0f;
								});
							}
							DelayedAction.functionAfterDelay(func2, 1300);
						};
						location.TemporarySprites.Add(cat_sprite);
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(4, 88, 19, 15), 90f, 3, 9999, new Vector2(16f, 5f) * 64f + new Vector2(31f, -10f) * 4f, false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							layerDepth = 0.9f,
							id = 888
						});
						Microsoft.Xna.Framework.Rectangle warpRect = new Microsoft.Xna.Framework.Rectangle(1024, 120, 144, 272);
						for (int i22 = 0; i22 < 80; i22++)
						{
							Vector2 warpSparklePos = Utility.getRandomPositionInThisRectangle(warpRect, Game1.random);
							Vector2 warpSkarleMotion = warpSparklePos - Utility.PointToVector2(warpRect.Center);
							warpSkarleMotion.Normalize();
							warpSkarleMotion *= (float)(Game1.random.Next(10, 21) / 10);
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(113 + Game1.random.Next(3) * 5, 123, 5, 5), 999f, 1, 9999, warpSparklePos, false, false, 0.8f, 0.02f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								layerDepth = 0.99f,
								rotationChange = (float)Game1.random.Next(-10, 10) / 100f,
								motion = warpSkarleMotion,
								acceleration = -warpSkarleMotion / 150f,
								scaleChange = (float)Game1.random.Next(-10, 0) / 500f,
								delayBeforeAnimationStart = i22 * 5
							});
						}
						return;
					}
					break;
				}
				case 20:
				{
					if (!(key == "BoatParrotSquawkStop"))
					{
						return;
					}
					TemporaryAnimatedSprite temporaryAnimatedSprite60 = this.aboveMapSprites[0];
					temporaryAnimatedSprite60.sourceRect.X = 0;
					temporaryAnimatedSprite60.sourceRectStartingPos.X = 0f;
					return;
				}
				case 23:
				{
					if (!(key == "leahStopHoldingPainting"))
					{
						return;
					}
					TemporaryAnimatedSprite temporarySpriteByID11 = location.getTemporarySpriteByID(999);
					temporarySpriteByID11.sourceRect.X = temporarySpriteByID11.sourceRect.X - 15;
					TemporaryAnimatedSprite temporarySpriteByID12 = location.getTemporarySpriteByID(999);
					temporarySpriteByID12.sourceRectStartingPos.X = temporarySpriteByID12.sourceRectStartingPos.X - 15f;
					location.removeTemporarySpritesWithIDLocal(777);
					Game1.playSound("thudStep", null);
					return;
				}
				default:
					return;
				}
			}
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x00042BBC File Offset: 0x00040DBC
		private Microsoft.Xna.Framework.Rectangle skipBounds()
		{
			int scale = 4;
			int width = 22 * scale;
			Microsoft.Xna.Framework.Rectangle skipBounds = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width - width - 8, Game1.viewport.Height - 64, width, 15 * scale);
			Utility.makeSafe(ref skipBounds);
			return skipBounds;
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x00042C00 File Offset: 0x00040E00
		public void receiveMouseClick(int x, int y)
		{
			if (!Game1.options.SnappyMenus && !this.skipped && this.skippable && this.skipBounds().Contains(x, y))
			{
				this.skipped = true;
				this.skipEvent();
				Game1.freezeControls = false;
			}
			this.popBalloons(x, y);
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x00042C58 File Offset: 0x00040E58
		public void skipEvent()
		{
			if (this.playerControlSequence)
			{
				this.EndPlayerControlSequence();
			}
			Game1.playSound("drumkit6", null);
			this.actorPositionsAfterMove.Clear();
			foreach (NPC i in this.actors)
			{
				bool ignore_stop_animation = i.Sprite.ignoreStopAnimation;
				i.Sprite.ignoreStopAnimation = true;
				i.Halt();
				i.Sprite.ignoreStopAnimation = ignore_stop_animation;
				this.resetDialogueIfNecessary(i);
			}
			this.farmer.Halt();
			this.farmer.ignoreCollisions = false;
			Game1.exitActiveMenu();
			Game1.fadeClear();
			Game1.dialogueUp = false;
			Game1.dialogueTyping = false;
			Game1.pauseTime = 0f;
			string[] array = this.actionsOnSkip;
			if (array != null && array.Length != 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
				foreach (string action in this.actionsOnSkip)
				{
					string error;
					Exception ex;
					if (!TriggerActionManager.TryRunAction(action, out error, out ex))
					{
						IGameLogger log = Game1.log;
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Event '");
						defaultInterpolatedStringHandler.AppendFormatted(this.id);
						defaultInterpolatedStringHandler.AppendLiteral("' failed applying post-skip action '");
						defaultInterpolatedStringHandler.AppendFormatted(action);
						defaultInterpolatedStringHandler.AppendLiteral("': ");
						defaultInterpolatedStringHandler.AppendFormatted(error);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
					}
				}
				IGameLogger log2 = Game1.log;
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Event '");
				defaultInterpolatedStringHandler.AppendFormatted(this.id);
				defaultInterpolatedStringHandler.AppendLiteral("' applied post-skip actions [");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", this.actionsOnSkip));
				defaultInterpolatedStringHandler.AppendLiteral("].");
				log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			string text = this.id;
			if (text != null)
			{
				switch (text.Length)
				{
				case 2:
					switch (text[0])
					{
					case '1':
						if (!(text == "19"))
						{
							goto IL_AD1;
						}
						Game1.player.cookingRecipes.TryAdd("Cookies", 0);
						this.endBehaviors(null);
						return;
					case '2':
						if (!(text == "26"))
						{
							goto IL_AD1;
						}
						Game1.player.craftingRecipes.TryAdd("Wild Bait", 0);
						this.endBehaviors(null);
						return;
					case '3':
						if (!(text == "33"))
						{
							goto IL_AD1;
						}
						Game1.player.craftingRecipes.TryAdd("Drum Block", 0);
						Game1.player.craftingRecipes.TryAdd("Flute Block", 0);
						this.endBehaviors(null);
						return;
					default:
						goto IL_AD1;
					}
					break;
				case 3:
					if (!(text == "112"))
					{
						goto IL_AD1;
					}
					this.endBehaviors(null);
					Game1.player.mailReceived.Add("canReadJunimoText");
					return;
				case 4:
				case 8:
				case 9:
					goto IL_AD1;
				case 5:
					if (!(text == "60367"))
					{
						goto IL_AD1;
					}
					this.endBehaviors(new string[]
					{
						"End",
						"beginGame"
					}, Game1.currentLocation);
					return;
				case 6:
					switch (text[0])
					{
					case '-':
						if (!(text == "-78765"))
						{
							goto IL_AD1;
						}
						this.endBehaviors(new string[]
						{
							"End",
							"tunnelDepart"
						}, Game1.currentLocation);
						return;
					case '.':
					case '/':
					case '0':
					case '2':
					case '3':
						goto IL_AD1;
					case '1':
						if (text == "191393")
						{
							if (!Game1.player.Items.ContainsId("(BC)116"))
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)116", 1, 0, false), null, false);
							}
							this.endBehaviors(new string[]
							{
								"End",
								"position",
								"52",
								"20"
							}, Game1.currentLocation);
							return;
						}
						if (!(text == "100162"))
						{
							goto IL_AD1;
						}
						if (!Game1.player.Items.ContainsId("(W)0"))
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(W)0", 1, 0, false), null, false);
						}
						Game1.player.Position = new Vector2(-9999f, -99999f);
						this.endBehaviors(null);
						return;
					case '4':
						if (!(text == "404798"))
						{
							goto IL_AD1;
						}
						if (!Game1.player.Items.ContainsId("(T)Pan"))
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)Pan", 1, 0, false), null, false);
						}
						this.endBehaviors(null);
						return;
					case '5':
						if (!(text == "558292"))
						{
							goto IL_AD1;
						}
						Game1.player.eventsSeen.Remove("2146991");
						this.endBehaviors(new string[]
						{
							"End",
							"bed"
						}, Game1.currentLocation);
						return;
					case '6':
						if (text == "690006")
						{
							if (!Game1.player.Items.ContainsId("(O)680"))
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)680", 1, 0, false), null, false);
							}
							this.endBehaviors(null);
							return;
						}
						if (!(text == "611173"))
						{
							goto IL_AD1;
						}
						if (!Game1.player.activeDialogueEvents.ContainsKey("pamHouseUpgradeAnonymous"))
						{
							Game1.player.activeDialogueEvents.TryAdd("pamHouseUpgrade", 4);
						}
						this.endBehaviors(null);
						return;
					case '7':
						if (!(text == "739330"))
						{
							goto IL_AD1;
						}
						if (!Game1.player.Items.ContainsId("(T)BambooPole"))
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)BambooPole", 1, 0, false), null, false);
						}
						this.endBehaviors(new string[]
						{
							"End",
							"position",
							"43",
							"36"
						}, Game1.currentLocation);
						return;
					case '8':
						if (!(text == "897405"))
						{
							goto IL_AD1;
						}
						break;
					case '9':
						if (text == "980559")
						{
							if (Game1.player.GetSkillLevel(1) < 1)
							{
								Game1.player.setSkillLevel("Fishing", 1);
							}
							if (!Game1.player.Items.ContainsId("(T)TrainingRod"))
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)TrainingRod", 1, 0, false), null, false);
							}
							this.endBehaviors(null);
							return;
						}
						if (text == "992553")
						{
							Game1.player.craftingRecipes.TryAdd("Furnace", 0);
							Game1.player.addQuest("11");
							this.endBehaviors(null);
							return;
						}
						if (text == "900553")
						{
							Game1.player.craftingRecipes.TryAdd("Garden Pot", 0);
							if (!Game1.player.Items.ContainsId("(BC)62"))
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)62", 1, 0, false), null, false);
							}
							this.endBehaviors(null);
							return;
						}
						if (!(text == "980558"))
						{
							goto IL_AD1;
						}
						Game1.player.craftingRecipes.TryAdd("Mini-Jukebox", 0);
						if (!Game1.player.Items.ContainsId("(BC)209"))
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)209", 1, 0, false), null, false);
						}
						this.endBehaviors(null);
						return;
					default:
						goto IL_AD1;
					}
					break;
				case 7:
					switch (text[1])
					{
					case '0':
						if (!(text == "3091462"))
						{
							goto IL_AD1;
						}
						if (!Game1.player.Items.ContainsId("(F)1802"))
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1802", 1, 0, false), null, false);
						}
						this.endBehaviors(null);
						return;
					case '1':
						if (!(text == "2123343"))
						{
							goto IL_AD1;
						}
						this.endBehaviors(new string[]
						{
							"End",
							"newDay"
						}, Game1.currentLocation);
						return;
					case '2':
					case '3':
					case '7':
						goto IL_AD1;
					case '4':
						if (!(text == "6497428"))
						{
							goto IL_AD1;
						}
						this.endBehaviors(new string[]
						{
							"End",
							"Leo"
						}, Game1.currentLocation);
						return;
					case '5':
						if (!(text == "1590166"))
						{
							goto IL_AD1;
						}
						break;
					case '6':
						if (!(text == "-666777"))
						{
							goto IL_AD1;
						}
						if (!Game1.netWorldState.Value.ActivatedGoldenParrot)
						{
							Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
						}
						if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
						{
							Game1.addMailForTomorrow("gotBirdieReward", true, true);
						}
						Game1.player.craftingRecipes.TryAdd("Fairy Dust", 0);
						this.endBehaviors(null);
						return;
					case '8':
					{
						if (!(text == "-888999"))
						{
							goto IL_AD1;
						}
						Object o = ItemRegistry.Create<Object>("(O)864", 1, 0, false);
						o.specialItem = true;
						o.questItem.Value = true;
						Game1.player.addItemByMenuIfNecessary(o, null, false);
						Game1.player.addQuest("130");
						this.endBehaviors(null);
						return;
					}
					case '9':
						if (!(text == "3918602"))
						{
							goto IL_AD1;
						}
						if (!Game1.player.Items.ContainsId("(F)1309"))
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1309", 1, 0, false), null, false);
						}
						this.endBehaviors(null);
						return;
					default:
						goto IL_AD1;
					}
					break;
				case 10:
					if (!(text == "-157039427"))
					{
						goto IL_AD1;
					}
					this.endBehaviors(new string[]
					{
						"End",
						"islandDepart"
					}, Game1.currentLocation);
					return;
				default:
					goto IL_AD1;
				}
				if (!this.gotPet)
				{
					string defaultName;
					if (Game1.player.IsMale)
					{
						defaultName = Game1.content.LoadString(Game1.player.catPerson ? "Strings\\StringsFromCSFiles:Event.cs.1794" : "Strings\\StringsFromCSFiles:Event.cs.1795");
					}
					else
					{
						defaultName = Game1.content.LoadString((Game1.player.whichPetType == "Dog") ? "Strings\\StringsFromCSFiles:Event.cs.1797" : "Strings\\StringsFromCSFiles:Event.cs.1796");
					}
					this.namePet(defaultName);
				}
				this.endBehaviors(null);
				return;
			}
			IL_AD1:
			this.endBehaviors(null);
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00043750 File Offset: 0x00041950
		public void receiveActionPress(int xTile, int yTile)
		{
			if (xTile == this.playerControlTargetTile.X && yTile == this.playerControlTargetTile.Y)
			{
				string a = this.playerControlSequenceID;
				if (a == "haleyBeach")
				{
					this.props.Clear();
					Game1.playSound("coin", null);
					this.playerControlTargetTile = new Point(35, 11);
					this.playerControlSequenceID = "haleyBeach2";
					return;
				}
				if (!(a == "haleyBeach2"))
				{
					return;
				}
				this.EndPlayerControlSequence();
				int num = this.CurrentCommand;
				this.CurrentCommand = num + 1;
			}
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x000437F0 File Offset: 0x000419F0
		public void startSecretSantaEvent()
		{
			this.playerControlSequence = false;
			this.playerControlSequenceID = null;
			string rawCommands;
			if (!this.TryGetFestivalDataForYear("secretSanta", out rawCommands))
			{
				Game1.log.Error("Festival " + this.id + " doesn't have the required 'secretSanta' data field.", null);
			}
			this.eventCommands = Event.ParseCommands(rawCommands, null);
			this.doingSecretSanta = true;
			this.setUpSecretSantaCommands();
			this.currentCommand = 0;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x0004385C File Offset: 0x00041A5C
		public void festivalUpdate(GameTime time)
		{
			Game1.player.team.festivalScoreStatus.UpdateState(Game1.player.festivalScore.ToString() ?? "");
			if (this.festivalTimer > 0)
			{
				int oldTime = this.festivalTimer;
				this.festivalTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.playerControlSequenceID == "iceFishing")
				{
					if (!Game1.player.UsingTool)
					{
						Game1.player.forceCanMove();
					}
					if (oldTime % 500 < this.festivalTimer % 500)
					{
						NPC temp = this.getActorByName("Pam", false);
						temp.Sprite.sourceRect.Offset(temp.Sprite.SourceRect.Width, 0);
						if (temp.Sprite.sourceRect.X >= temp.Sprite.Texture.Width)
						{
							temp.Sprite.sourceRect.Offset(-temp.Sprite.Texture.Width, 0);
						}
						temp = this.getActorByName("Elliott", false);
						temp.Sprite.sourceRect.Offset(temp.Sprite.SourceRect.Width, 0);
						if (temp.Sprite.sourceRect.X >= temp.Sprite.Texture.Width)
						{
							temp.Sprite.sourceRect.Offset(-temp.Sprite.Texture.Width, 0);
						}
						temp = this.getActorByName("Willy", false);
						temp.Sprite.sourceRect.Offset(temp.Sprite.SourceRect.Width, 0);
						if (temp.Sprite.sourceRect.X >= temp.Sprite.Texture.Width)
						{
							temp.Sprite.sourceRect.Offset(-temp.Sprite.Texture.Width, 0);
						}
					}
					if (oldTime % 29900 < this.festivalTimer % 29900)
					{
						this.getActorByName("Willy", false).shake(500);
						Game1.playSound("dwop", null);
						this.temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), this.getActorByName("Willy", false).Position + new Vector2(0f, -96f), false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
					if (oldTime % 45900 < this.festivalTimer % 45900)
					{
						this.getActorByName("Pam", false).shake(500);
						Game1.playSound("dwop", null);
						this.temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), this.getActorByName("Pam", false).Position + new Vector2(0f, -96f), false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
					if (oldTime % 59900 < this.festivalTimer % 59900)
					{
						this.getActorByName("Elliott", false).shake(500);
						Game1.playSound("dwop", null);
						this.temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), this.getActorByName("Elliott", false).Position + new Vector2(0f, -96f), false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
				}
				if (this.festivalTimer <= 0)
				{
					Game1.player.Halt();
					string a = this.playerControlSequenceID;
					if (!(a == "eggHunt"))
					{
						if (a == "iceFishing")
						{
							this.EndPlayerControlSequence();
							string rawCommands;
							if (!this.TryGetFestivalDataForYear("afterIceFishing", out rawCommands))
							{
								Game1.log.Error("Festival " + this.id + " doesn't have the required 'afterIceFishing' data field.", null);
							}
							this.eventCommands = Event.ParseCommands(rawCommands, null);
							this.currentCommand = 0;
							if (Game1.activeClickableMenu != null)
							{
								Game1.activeClickableMenu.emergencyShutDown();
							}
							Game1.activeClickableMenu = null;
							if (Game1.player.UsingTool)
							{
								FishingRod rod = Game1.player.CurrentTool as FishingRod;
								if (rod != null)
								{
									rod.doneFishing(Game1.player, false);
								}
							}
							Game1.screenOverlayTempSprites.Clear();
							Game1.player.forceCanMove();
						}
					}
					else
					{
						this.EndPlayerControlSequence();
						string rawCommands2;
						if (!this.TryGetFestivalDataForYear("afterEggHunt", out rawCommands2))
						{
							Game1.log.Error("Festival " + this.id + " doesn't have the required 'afterEggHunt' data field.", null);
						}
						this.eventCommands = Event.ParseCommands(rawCommands2, null);
						this.currentCommand = 0;
					}
				}
			}
			if (this.startSecretSantaAfterDialogue && !Game1.dialogueUp)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.startSecretSantaEvent), 0.01f);
				this.startSecretSantaAfterDialogue = false;
			}
			Game1.player.festivalScore = Math.Min(Game1.player.festivalScore, 9999);
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x00043E54 File Offset: 0x00042054
		private void setUpSecretSantaCommands()
		{
			Point secretSantaTile;
			try
			{
				secretSantaTile = this.getActorByName(this.mySecretSanta.Name, false).TilePoint;
			}
			catch
			{
				this.mySecretSanta = this.getActorByName("Lewis", false);
				secretSantaTile = this.getActorByName(this.mySecretSanta.Name, false).TilePoint;
			}
			Dictionary<string, string> dialogue = this.mySecretSanta.Dialogue;
			string beforeDialogue = (dialogue != null) ? dialogue.GetValueOrDefault("WinterStar_GiveGift_Before") : null;
			Dictionary<string, string> dialogue2 = this.mySecretSanta.Dialogue;
			string afterDialogue = (dialogue2 != null) ? dialogue2.GetValueOrDefault("WinterStar_GiveGift_After") : null;
			if (Game1.player.spouse == this.mySecretSanta.Name)
			{
				Dictionary<string, string> dialogue3 = this.mySecretSanta.Dialogue;
				beforeDialogue = (((dialogue3 != null) ? dialogue3.GetValueOrDefault("WinterStar_GiveGift_Before_" + (Game1.player.isRoommate(this.mySecretSanta.Name) ? "Roommate" : "Spouse")) : null) ?? beforeDialogue);
				Dictionary<string, string> dialogue4 = this.mySecretSanta.Dialogue;
				afterDialogue = (((dialogue4 != null) ? dialogue4.GetValueOrDefault("WinterStar_GiveGift_After_" + (Game1.player.isRoommate(this.mySecretSanta.Name) ? "Roommate" : "Spouse")) : null) ?? afterDialogue);
			}
			if (this.mySecretSanta.Age == 2)
			{
				if (beforeDialogue == null)
				{
					beforeDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1497");
				}
				if (afterDialogue == null)
				{
					afterDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1498");
				}
			}
			else if (this.mySecretSanta.Manners == 2)
			{
				if (beforeDialogue == null)
				{
					beforeDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1501");
				}
				if (afterDialogue == null)
				{
					afterDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1504");
				}
			}
			else
			{
				if (beforeDialogue == null)
				{
					beforeDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1499");
				}
				if (afterDialogue == null)
				{
					afterDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1500");
				}
			}
			for (int i = 0; i < this.eventCommands.Length; i++)
			{
				this.eventCommands[i] = this.eventCommands[i].Replace("secretSanta", this.mySecretSanta.Name);
				this.eventCommands[i] = this.eventCommands[i].Replace("warpX", secretSantaTile.X.ToString() ?? "");
				this.eventCommands[i] = this.eventCommands[i].Replace("warpY", secretSantaTile.Y.ToString() ?? "");
				this.eventCommands[i] = this.eventCommands[i].Replace("dialogue1", beforeDialogue);
				this.eventCommands[i] = this.eventCommands[i].Replace("dialogue2", afterDialogue);
			}
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x00044130 File Offset: 0x00042330
		public void drawFarmers(SpriteBatch b)
		{
			foreach (Farmer farmer in this.farmerActors)
			{
				farmer.draw(b);
			}
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00044184 File Offset: 0x00042384
		public virtual bool ShouldHideCharacter(NPC n)
		{
			return n is Child && this.doingSecretSanta;
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x0004419C File Offset: 0x0004239C
		public void draw(SpriteBatch b)
		{
			if (this.currentCustomEventScript != null)
			{
				this.currentCustomEventScript.draw(b);
				return;
			}
			foreach (NPC i in this.actors)
			{
				if (!this.ShouldHideCharacter(i))
				{
					i.Name.Equals("Marcello");
					if (i.ySourceRectOffset == 0)
					{
						i.draw(b);
					}
					else
					{
						i.draw(b, i.ySourceRectOffset, 1f);
					}
				}
			}
			foreach (Object @object in this.props)
			{
				@object.drawAsProp(b);
			}
			foreach (Prop prop in this.festivalProps)
			{
				prop.draw(b);
			}
			if (this.isSpecificFestival("fall16"))
			{
				Vector2 start = Game1.GlobalToLocal(Game1.viewport, new Vector2(37f, 56f) * 64f);
				start.X += 4f;
				int xCutoff = (int)start.X + 168;
				start.Y += 8f;
				for (int j = 0; j < Game1.player.team.grangeDisplay.Count; j++)
				{
					if (Game1.player.team.grangeDisplay[j] != null)
					{
						start.Y += 42f;
						start.X += 4f;
						b.Draw(Game1.shadowTexture, start, new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
						start.Y -= 42f;
						start.X -= 4f;
						Game1.player.team.grangeDisplay[j].drawInMenu(b, start, 1f, 1f, (float)j / 1000f + 0.001f, StackDrawType.Hide);
					}
					start.X += 60f;
					if (start.X >= (float)xCutoff)
					{
						start.X = (float)(xCutoff - 168);
						start.Y += 64f;
					}
				}
			}
			if (this.drawTool)
			{
				Game1.drawTool(this.farmer);
			}
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x00044460 File Offset: 0x00042660
		public void drawUnderWater(SpriteBatch b)
		{
			if (this.underwaterSprites != null)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.underwaterSprites)
				{
					temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
				}
			}
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x000444BC File Offset: 0x000426BC
		public void drawAfterMap(SpriteBatch b)
		{
			if (this.aboveMapSprites != null)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.aboveMapSprites)
				{
					temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
				}
			}
			if (!Game1.game1.takingMapScreenshot && this.playerControlSequenceID != null)
			{
				string a = this.playerControlSequenceID;
				if (!(a == "eggHunt"))
				{
					if (!(a == "fair"))
					{
						if (a == "iceFishing")
						{
							b.End();
							b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
							b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 128), Color.Black * 0.75f);
							b.Draw(this.festivalTexture, new Vector2(32f, 16f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
							Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(96f, (float)(21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8)))), 0f, 1f, 1f, false);
							Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(this.festivalTimer), Color.Black, Color.White, new Vector2(32f, 93f), 0f, 1f, 1f, false);
							b.End();
							b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
							if (Game1.IsMultiplayer)
							{
								Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, (float)(Game1.viewport.Height - 32)), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
							}
						}
					}
					else
					{
						b.End();
						Game1.PushUIMode();
						b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
						b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(72f, (float)(21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8)))), 0f, 1f, 1f, false);
						if (Game1.activeClickableMenu == null)
						{
							Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.dayTimeMoneyBox.xPositionOnScreen, 4);
						}
						b.End();
						Game1.PopUIMode();
						b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						if (Game1.IsMultiplayer)
						{
							Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, (float)(Game1.viewport.Height - 32)), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
						}
					}
				}
				else
				{
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(32, 32, 224, 160), Color.Black * 0.5f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", this.festivalTimer / 1000), Color.Black, Color.Yellow, new Vector2(64f, 64f), 0f, 1f, 1f, false);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1515", Game1.player.festivalScore), Color.Black, Color.Pink, new Vector2(64f, 128f), 0f, 1f, 1f, false);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, (float)(Game1.viewport.Height - 32)), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
				}
			}
			string text = this.spriteTextToDraw;
			if (text != null && text.Length > 0)
			{
				Color color = SpriteText.getColorFromIndex(this.int_useMeForAnything2);
				SpriteText.drawStringHorizontallyCenteredAt(b, this.spriteTextToDraw, Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 192, this.int_useMeForAnything, -1, 999999, 1f, 1f, false, new Color?(color), 99999);
			}
			foreach (NPC npc in this.actors)
			{
				npc.drawAboveAlwaysFrontLayer(b);
			}
			if (this.skippable && !Game1.options.SnappyMenus && !Game1.game1.takingMapScreenshot)
			{
				Microsoft.Xna.Framework.Rectangle skipBounds = this.skipBounds();
				Color renderCol = Color.White;
				if (skipBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					renderCol *= 0.5f;
				}
				Microsoft.Xna.Framework.Rectangle srcBounds = new Microsoft.Xna.Framework.Rectangle(205, 406, 22, 15);
				b.Draw(Game1.mouseCursors, Utility.PointToVector2(skipBounds.Location), new Microsoft.Xna.Framework.Rectangle?(srcBounds), renderCol, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
			}
			ICustomEventScript customEventScript = this.currentCustomEventScript;
			if (customEventScript == null)
			{
				return;
			}
			customEventScript.drawAboveAlwaysFront(b);
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00044B40 File Offset: 0x00042D40
		public void EndPlayerControlSequence()
		{
			this.playerControlSequence = false;
			this.playerControlSequenceID = null;
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x00044B50 File Offset: 0x00042D50
		public void OnPlayerControlSequenceEnd(string id)
		{
			Game1.player.StopSitting(true);
			Game1.player.CanMove = false;
			Game1.player.Halt();
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x00044B74 File Offset: 0x00042D74
		public void setUpPlayerControlSequence(string id)
		{
			this.playerControlSequenceID = id;
			this.playerControlSequence = true;
			Game1.player.CanMove = true;
			Game1.viewportFreeze = false;
			Game1.forceSnapOnNextViewportUpdate = true;
			Game1.globalFade = false;
			this.doingSecretSanta = false;
			if (id != null)
			{
				switch (id.Length)
				{
				case 4:
				{
					char c = id[0];
					if (c != 'f')
					{
						if (c != 'l')
						{
							return;
						}
						if (!(id == "luau"))
						{
							return;
						}
						this.festivalHost = this.getActorByName("Lewis", false);
						this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1527";
						return;
					}
					else
					{
						if (!(id == "fair"))
						{
							return;
						}
						this.festivalHost = this.getActorByName("Lewis", false);
						this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1535";
						return;
					}
					break;
				}
				case 5:
				case 6:
				case 12:
				case 13:
					break;
				case 7:
				{
					char c = id[0];
					if (c != 'e')
					{
						if (c != 'j')
						{
							return;
						}
						if (!(id == "jellies"))
						{
							return;
						}
						this.festivalHost = this.getActorByName("Lewis", false);
						this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1531";
						return;
					}
					else
					{
						if (!(id == "eggHunt"))
						{
							return;
						}
						Layer pathsLayer = Game1.currentLocation.map.RequireLayer("Paths");
						for (int x = 0; x < pathsLayer.LayerWidth; x++)
						{
							for (int y = 0; y < pathsLayer.LayerHeight; y++)
							{
								Tile tile = pathsLayer.Tiles[x, y];
								if (tile != null && tile.TileSheet.Id.StartsWith("fest"))
								{
									this.festivalProps.Add(new Prop(this.festivalTexture, tile.TileIndex, 1, 1, 1, x, y, true));
								}
							}
						}
						this.festivalTimer = 52000;
						this.currentCommand++;
						return;
					}
					break;
				}
				case 8:
					if (!(id == "boatRide"))
					{
						return;
					}
					Game1.viewportFreeze = true;
					Game1.currentViewportTarget = Utility.PointToVector2(Game1.viewportCenter);
					this.currentCommand++;
					return;
				case 9:
				{
					char c = id[0];
					if (c != 'c')
					{
						if (c != 'h')
						{
							return;
						}
						if (!(id == "halloween"))
						{
							return;
						}
						if (Game1.year % 2 == 0)
						{
							this.temporaryLocation.objects.Add(new Vector2(63f, 16f), new Chest(new List<Item>
							{
								ItemRegistry.Create("(O)PrizeTicket", 1, 0, false)
							}, new Vector2(63f, 16f), false, 0, false));
							return;
						}
						this.temporaryLocation.objects.Add(new Vector2(33f, 13f), new Chest(new List<Item>
						{
							ItemRegistry.Create("(O)373", 1, 0, false)
						}, new Vector2(33f, 13f), false, 0, false));
						return;
					}
					else
					{
						if (!(id == "christmas"))
						{
							return;
						}
						this.secretSantaRecipient = Utility.GetRandomWinterStarParticipant(null);
						this.mySecretSanta = (Utility.GetRandomWinterStarParticipant((string name) => name == this.secretSantaRecipient.Name || NPC.IsDivorcedFrom(this.farmer, name)) ?? this.secretSantaRecipient);
						Game1.debugOutput = "Secret Santa Recipient: " + this.secretSantaRecipient.Name + "  My Secret Santa: " + this.mySecretSanta.Name;
						return;
					}
					break;
				}
				case 10:
				{
					char c = id[0];
					if (c != 'h')
					{
						if (c != 'i')
						{
							if (c != 'p')
							{
								return;
							}
							if (!(id == "parrotRide"))
							{
								return;
							}
							Game1.player.canOnlyWalk = false;
							this.currentCommand++;
							return;
						}
						else
						{
							if (!(id == "iceFishing"))
							{
								return;
							}
							Tool rod = ItemRegistry.Create<Tool>("(T)BambooPole", 1, 0, false);
							rod.AttachmentSlotsCount = 2;
							rod.attachments[1] = ItemRegistry.Create<Object>("(O)687", 1, 0, false);
							this.festivalTimer = 120000;
							this.farmer.festivalScore = 0;
							this.farmer.CurrentToolIndex = 0;
							this.farmer.TemporaryItem = rod;
							this.farmer.CurrentToolIndex = 0;
						}
					}
					else
					{
						if (!(id == "haleyBeach"))
						{
							return;
						}
						Vector2 tile2 = new Vector2(53f, 8f);
						Object item = ItemRegistry.Create<Object>("(O)742", 1, 0, false);
						item.TileLocation = tile2;
						item.Flipped = false;
						this.props.Add(item);
						this.playerControlTargetTile = new Point(53, 8);
						Game1.player.canOnlyWalk = false;
						return;
					}
					break;
				}
				case 11:
				{
					char c = id[0];
					if (c != 'e')
					{
						if (c != 'i')
						{
							return;
						}
						if (!(id == "iceFestival"))
						{
							return;
						}
						this.festivalHost = this.getActorByName("Lewis", false);
						this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1548";
						if (Game1.year % 2 == 0)
						{
							this.temporaryLocation.setFireplace(true, 46, 16, false, -28, 28);
							this.temporaryLocation.setFireplace(true, 61, 43, false, -28, 28);
						}
						else
						{
							this.temporaryLocation.setFireplace(true, 11, 44, false, -28, 28);
							this.temporaryLocation.setFireplace(true, 65, 45, false, -28, 28);
						}
						if (Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
						{
							for (int x2 = 52; x2 < 60; x2++)
							{
								for (int y2 = 0; y2 < 2; y2++)
								{
									this.temporaryLocation.removeTile(x2, y2, "AlwaysFront");
								}
							}
							if (!NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
							{
								this.temporaryLocation.ApplyMapOverride("Forest_RaccoonStump", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6)));
								return;
							}
							this.temporaryLocation.ApplyMapOverride("Forest_RaccoonHouse", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6)));
							return;
						}
					}
					else
					{
						if (!(id == "eggFestival"))
						{
							return;
						}
						this.festivalHost = this.getActorByName("Lewis", false);
						this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1521";
						return;
					}
					break;
				}
				case 14:
					if (!(id == "flowerFestival"))
					{
						return;
					}
					this.festivalHost = this.getActorByName("Lewis", false);
					this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1524";
					if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
					{
						Game1.currentLocation.removeMapTile(62, 28, "Buildings");
						Game1.currentLocation.removeMapTile(64, 28, "Buildings");
						Game1.currentLocation.removeMapTile(73, 48, "Buildings");
						return;
					}
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x00045208 File Offset: 0x00043408
		public bool canMoveAfterDialogue()
		{
			if (this.playerControlSequenceID != null && this.playerControlSequenceID.Equals("eggHunt"))
			{
				Game1.player.canMove = true;
				int num = this.CurrentCommand;
				this.CurrentCommand = num + 1;
			}
			return this.playerControlSequence;
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x00045250 File Offset: 0x00043450
		public void forceFestivalContinue()
		{
			bool isFallFestival = this.isSpecificFestival("fall16");
			if (isFallFestival)
			{
				this.initiateGrangeJudging();
			}
			else
			{
				Game1.dialogueUp = false;
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.emergencyShutDown();
				}
				Game1.exitActiveMenu();
				string rawCommands;
				if (!this.TryGetFestivalDataForYear("mainEvent", out rawCommands))
				{
					Game1.log.Error("Festival " + this.id + " doesn't have the required 'mainEvent' data field.", null);
				}
				string[] newCommands = Event.ParseCommands(rawCommands, null);
				this.eventCommands = newCommands;
				this.CurrentCommand = 0;
				this.eventSwitched = true;
				this.playerControlSequence = false;
				this.setUpFestivalMainEvent();
				Game1.player.Halt();
			}
			if (Game1.IsServer && (isFallFestival || !Game1.HasDedicatedHost))
			{
				Game1.multiplayer.sendServerToClientsMessage("festivalEvent");
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00045313 File Offset: 0x00043513
		public static string[] SplitPreconditions(string rawScript)
		{
			return ArgUtility.SplitQuoteAware(rawScript, '/', StringSplitOptions.RemoveEmptyEntries, true);
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00045320 File Offset: 0x00043520
		public static string[] ParseCommands(string rawScript, Farmer player = null)
		{
			Gender? gender;
			if (player == null)
			{
				Farmer player2 = Game1.player;
				gender = ((player2 != null) ? new Gender?(player2.Gender) : null);
			}
			else
			{
				gender = new Gender?(player.Gender);
			}
			Gender? gender2 = gender;
			rawScript = Dialogue.applyGenderSwitchBlocks(gender2.GetValueOrDefault(), rawScript);
			rawScript = TokenParser.ParseText(rawScript, null, null, null);
			return ArgUtility.SplitQuoteAware(rawScript, '/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, true);
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00045380 File Offset: 0x00043580
		public bool isSpecificFestival(string festivalId)
		{
			return this.isFestival && this.id == "festival_" + festivalId;
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x000453A4 File Offset: 0x000435A4
		public void setUpFestivalMainEvent()
		{
			if (this.isSpecificFestival("spring24"))
			{
				List<NetDancePartner> females = new List<NetDancePartner>();
				List<NetDancePartner> males = new List<NetDancePartner>();
				List<string> leftoverFemales = new List<string>
				{
					"Abigail",
					"Penny",
					"Leah",
					"Maru",
					"Haley",
					"Emily"
				};
				List<string> leftoverMales = new List<string>
				{
					"Sebastian",
					"Sam",
					"Elliott",
					"Harvey",
					"Alex",
					"Shane"
				};
				List<Farmer> farmers = (from f in Game1.getOnlineFarmers()
				orderby f.UniqueMultiplayerID
				select f).ToList<Farmer>();
				while (farmers.Count > 0)
				{
					Farmer f2 = farmers[0];
					farmers.RemoveAt(0);
					if (!Game1.multiplayer.isDisconnecting(f2) && f2.dancePartner.Value != null)
					{
						if (f2.dancePartner.GetGender() == Gender.Female)
						{
							females.Add(f2.dancePartner);
							if (f2.dancePartner.IsVillager())
							{
								leftoverFemales.Remove(f2.dancePartner.TryGetVillager().Name);
							}
							males.Add(new NetDancePartner(f2));
						}
						else
						{
							males.Add(f2.dancePartner);
							if (f2.dancePartner.IsVillager())
							{
								leftoverMales.Remove(f2.dancePartner.TryGetVillager().Name);
							}
							females.Add(new NetDancePartner(f2));
						}
						if (f2.dancePartner.IsFarmer())
						{
							farmers.Remove(f2.dancePartner.TryGetFarmer());
						}
					}
				}
				while (females.Count < 6)
				{
					string female = leftoverFemales.Last<string>();
					if (leftoverMales.Contains(Utility.getLoveInterest(female)))
					{
						females.Add(new NetDancePartner(female));
						males.Add(new NetDancePartner(Utility.getLoveInterest(female)));
					}
					leftoverFemales.Remove(female);
				}
				string rawFestivalData;
				if (!this.TryGetFestivalDataForYear("mainEvent", out rawFestivalData))
				{
					rawFestivalData = string.Empty;
				}
				for (int i = 1; i <= 6; i++)
				{
					string female2;
					if (females[i - 1].IsVillager())
					{
						female2 = females[i - 1].TryGetVillager().Name;
					}
					else
					{
						female2 = "farmer" + Utility.getFarmerNumberFromFarmer(females[i - 1].TryGetFarmer()).ToString();
					}
					string male;
					if (males[i - 1].IsVillager())
					{
						male = males[i - 1].TryGetVillager().Name;
					}
					else
					{
						male = "farmer" + Utility.getFarmerNumberFromFarmer(males[i - 1].TryGetFarmer()).ToString();
					}
					rawFestivalData = rawFestivalData.Replace("Girl" + i.ToString(), female2);
					rawFestivalData = rawFestivalData.Replace("Guy" + i.ToString(), male);
				}
				List<KeyValuePair<NetDancePartner, NetDancePartner>> pairsByInnermost = new List<KeyValuePair<NetDancePartner, NetDancePartner>>();
				List<KeyValuePair<NetDancePartner, NetDancePartner>> playerPairs = new List<KeyValuePair<NetDancePartner, NetDancePartner>>();
				for (int j = females.Count - 1; j >= 0; j--)
				{
					NetDancePartner female3 = females[j];
					NetDancePartner male2 = males[j];
					if (female3.IsFarmer() || male2.IsFarmer())
					{
						playerPairs.Add(new KeyValuePair<NetDancePartner, NetDancePartner>(female3, male2));
						females.RemoveAt(j);
						males.RemoveAt(j);
					}
				}
				pairsByInnermost.AddRange(playerPairs.OrderBy(delegate(KeyValuePair<NetDancePartner, NetDancePartner> pair)
				{
					int female4 = Utility.getFarmerNumberFromFarmer(pair.Key.TryGetFarmer());
					int male3 = Utility.getFarmerNumberFromFarmer(pair.Value.TryGetFarmer());
					if (female4 > -1 && male3 > -1)
					{
						return Math.Min(female4, male3);
					}
					if (female4 <= -1)
					{
						return male3;
					}
					return female4;
				}));
				for (int k = 0; k < females.Count; k++)
				{
					pairsByInnermost.Add(new KeyValuePair<NetDancePartner, NetDancePartner>(females[k], males[k]));
				}
				females.Clear();
				males.Clear();
				bool addLeft = true;
				foreach (KeyValuePair<NetDancePartner, NetDancePartner> pair2 in pairsByInnermost)
				{
					if (addLeft)
					{
						females.Insert(0, pair2.Key);
						males.Insert(0, pair2.Value);
					}
					else
					{
						females.Add(pair2.Key);
						males.Add(pair2.Value);
					}
					addLeft = !addLeft;
				}
				List<string> commandsToAdd = new List<string>(Event.ParseCommands(rawFestivalData, null));
				for (int l = 0; l < commandsToAdd.Count; l++)
				{
					string command = commandsToAdd[l];
					List<NetDancePartner> dancers = null;
					string token = null;
					if (command.Contains("Girls"))
					{
						token = "Girls";
						dancers = females;
					}
					else if (command.Contains("Guys"))
					{
						token = "Guys";
						dancers = males;
					}
					if (dancers != null)
					{
						float spacing = 10f / (float)(dancers.Count - 1);
						if (spacing < 1f)
						{
							spacing = 1f;
						}
						for (int m = 0; m < dancers.Count; m++)
						{
							string name = dancers[m].IsVillager() ? dancers[m].TryGetVillager().Name : ("farmer" + Utility.getFarmerNumberFromFarmer(dancers[m].TryGetFarmer()).ToString());
							string newCommand = command.Replace(token, name);
							if (newCommand.StartsWith("warp "))
							{
								string[] warp = ArgUtility.SplitBySpace(newCommand);
								int x = int.Parse(warp[2]);
								warp[2] = (x + (int)Math.Round((double)((float)m * spacing))).ToString();
								newCommand = string.Join(" ", warp);
							}
							commandsToAdd.Insert(l + m, newCommand);
						}
						l += dancers.Count;
						commandsToAdd.RemoveAt(l);
						l--;
					}
				}
				rawFestivalData = string.Join("/", commandsToAdd);
				Regex regex = new Regex("showFrame (?<farmerName>farmer\\d) 44");
				Regex showFrameGirl = new Regex("showFrame (?<farmerName>farmer\\d) 40");
				Regex animation1Guy = new Regex("animate (?<farmerName>farmer\\d) false true 600 44 45");
				Regex animation1Girl = new Regex("animate (?<farmerName>farmer\\d) false true 600 43 41 43 42");
				Regex animation2Guy = new Regex("animate (?<farmerName>farmer\\d) false true 300 46 47");
				Regex animation2Girl = new Regex("animate (?<farmerName>farmer\\d) false true 600 46 47");
				rawFestivalData = regex.Replace(rawFestivalData, "showFrame $1 12/faceDirection $1 0");
				rawFestivalData = showFrameGirl.Replace(rawFestivalData, "showFrame $1 0/faceDirection $1 2");
				rawFestivalData = animation1Guy.Replace(rawFestivalData, "animate $1 false true 600 12 13 12 14");
				rawFestivalData = animation1Girl.Replace(rawFestivalData, "animate $1 false true 596 4 0");
				rawFestivalData = animation2Guy.Replace(rawFestivalData, "animate $1 false true 150 12 13 12 14");
				rawFestivalData = animation2Girl.Replace(rawFestivalData, "animate $1 false true 600 0 3");
				this.eventCommands = Event.ParseCommands(rawFestivalData, null);
			}
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x00045A74 File Offset: 0x00043C74
		private void judgeGrange()
		{
			int pointsEarned = 14;
			Dictionary<int, bool> categoriesRepresented = new Dictionary<int, bool>();
			int nullsCount = 0;
			bool purpleShorts = false;
			foreach (Item i in Game1.player.team.grangeDisplay)
			{
				Object obj = i as Object;
				if (obj != null)
				{
					if (Event.IsItemMayorShorts(obj))
					{
						purpleShorts = true;
					}
					pointsEarned += obj.Quality + 1;
					int num = obj.sellToStorePrice(-1L);
					if (num >= 20)
					{
						pointsEarned++;
					}
					if (num >= 90)
					{
						pointsEarned++;
					}
					if (num >= 200)
					{
						pointsEarned++;
					}
					if (num >= 300 && obj.Quality < 2)
					{
						pointsEarned++;
					}
					if (num >= 400 && obj.Quality < 1)
					{
						pointsEarned++;
					}
					int category = obj.Category;
					if (category <= -27)
					{
						switch (category)
						{
						case -81:
						case -80:
							break;
						case -79:
							categoriesRepresented[-79] = true;
							continue;
						case -78:
						case -77:
						case -76:
							continue;
						case -75:
							categoriesRepresented[-75] = true;
							continue;
						default:
							if (category != -27)
							{
								continue;
							}
							break;
						}
						categoriesRepresented[-81] = true;
					}
					else if (category != -26)
					{
						if (category != -18)
						{
							switch (category)
							{
							case -14:
							case -6:
							case -5:
								break;
							case -13:
							case -11:
							case -10:
							case -9:
							case -8:
							case -3:
								continue;
							case -12:
							case -2:
								categoriesRepresented[-12] = true;
								continue;
							case -7:
								categoriesRepresented[-7] = true;
								continue;
							case -4:
								categoriesRepresented[-4] = true;
								continue;
							default:
								continue;
							}
						}
						categoriesRepresented[-5] = true;
					}
					else
					{
						categoriesRepresented[-26] = true;
					}
				}
				else if (i == null)
				{
					nullsCount++;
				}
			}
			pointsEarned += Math.Min(30, categoriesRepresented.Count * 5);
			int displayFilledPoints = 9 - 2 * nullsCount;
			pointsEarned += displayFilledPoints;
			this.grangeScore = pointsEarned;
			if (purpleShorts)
			{
				this.grangeScore = -666;
			}
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00045C8C File Offset: 0x00043E8C
		private void lewisDoneJudgingGrange()
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1584")));
				Game1.player.Halt();
			}
			this.interpretGrangeResults();
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00045CC0 File Offset: 0x00043EC0
		public void interpretGrangeResults()
		{
			List<Character> winners = new List<Character>
			{
				this.getActorByName("Pierre", false),
				this.getActorByName("Marnie", false),
				this.getActorByName("Willy", false)
			};
			if (this.grangeScore >= 90)
			{
				winners.Insert(0, Game1.player);
			}
			else if (this.grangeScore >= 75)
			{
				winners.Insert(1, Game1.player);
			}
			else if (this.grangeScore >= 60)
			{
				winners.Insert(2, Game1.player);
			}
			else
			{
				winners.Add(Game1.player);
			}
			NPC npc = winners[0] as NPC;
			bool pierreWon = ((npc != null) ? npc.Name : null) == "Pierre";
			bool playerSkipped = Game1.player.team.grangeDisplay.Count == 0;
			bool usedPurpleShorts = this.grangeScore == -666;
			foreach (NPC actor in this.actors)
			{
				Dialogue dialogue;
				if (pierreWon)
				{
					Dialogue dialogue2;
					if ((dialogue2 = (usedPurpleShorts ? actor.TryGetDialogue("Fair_Judged_PlayerLost_PurpleShorts") : null)) == null && (dialogue2 = (playerSkipped ? actor.TryGetDialogue("Fair_Judged_PlayerLost_Skipped") : null)) == null)
					{
						dialogue2 = (actor.TryGetDialogue("Fair_Judged_PlayerLost") ?? actor.TryGetDialogue("Fair_Judged"));
					}
					dialogue = dialogue2;
				}
				else
				{
					dialogue = (actor.TryGetDialogue("Fair_Judged_PlayerWon") ?? actor.TryGetDialogue("Fair_Judged"));
				}
				if (dialogue != null)
				{
					actor.setNewDialogue(dialogue, false, false);
				}
			}
			this.grangeJudged = true;
			if (winners[0] is Farmer)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					farmer.autoGenerateActiveDialogueEvent("wonGrange", 4);
				}
			}
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x00045EC4 File Offset: 0x000440C4
		private void initiateGrangeJudging()
		{
			this.judgeGrange();
			this.hostMessageKey = null;
			this.setUpAdvancedMove(ArgUtility.SplitBySpace("advancedMove Lewis False 2 0 0 7 8 0 4 3000 3 0 4 3000 3 0 4 3000 3 0 4 3000 -14 0 2 1000"), new NPCController.endBehavior(this.lewisDoneJudgingGrange));
			this.getActorByName("Lewis", false).CurrentDialogue.Clear();
			if (this.getActorByName("Marnie", false) != null)
			{
				this.npcControllers.RemoveAll((NPCController npcController) => npcController.puppet.Name == "Marnie");
			}
			this.setUpAdvancedMove(ArgUtility.SplitBySpace("advancedMove Marnie False 0 1 4 1000"), null);
			foreach (NPC actor in this.actors)
			{
				Dialogue dialogue = actor.TryGetDialogue("Fair_Judging");
				if (dialogue != null)
				{
					actor.setNewDialogue(dialogue, false, false);
				}
			}
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x00045FB4 File Offset: 0x000441B4
		public void answerDialogueQuestion(NPC who, string answerKey)
		{
			if (this.isFestival)
			{
				if (!(answerKey == "yes"))
				{
					if (!(answerKey == "no"))
					{
						if (!(answerKey == "danceAsk"))
						{
							return;
						}
						if (Game1.player.spouse != null && who.Name == Game1.player.spouse)
						{
							Game1.player.dancePartner.Value = who;
							Dialogue dialogue2;
							if ((dialogue2 = who.TryGetDialogue("FlowerDance_Accept_" + (Game1.player.isRoommate(who.Name) ? "Roommate" : "Spouse"))) == null)
							{
								dialogue2 = (who.TryGetDialogue("FlowerDance_Accept") ?? new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1632", false));
							}
							who.setNewDialogue(dialogue2, false, false);
							using (List<NPC>.Enumerator enumerator = this.actors.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									NPC i = enumerator.Current;
									Stack<Dialogue> currentDialogue = i.CurrentDialogue;
									if (currentDialogue != null && currentDialogue.Count > 0 && i.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
									{
										i.CurrentDialogue.Clear();
									}
								}
								goto IL_2F2;
							}
						}
						if (!who.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(who.Name) >= 1000 && !who.isMarried())
						{
							try
							{
								Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name, true, false));
							}
							catch
							{
							}
							Game1.player.dancePartner.Value = who;
							who.setNewDialogue(who.TryGetDialogue("FlowerDance_Accept") ?? ((who.Gender == Gender.Female) ? new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1634", false) : new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1633", false)), false, false);
							using (List<NPC>.Enumerator enumerator = this.actors.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									NPC j = enumerator.Current;
									Stack<Dialogue> currentDialogue2 = j.CurrentDialogue;
									if (currentDialogue2 != null && currentDialogue2.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
									{
										j.CurrentDialogue.Clear();
									}
								}
								goto IL_2F2;
							}
						}
						if (who.HasPartnerForDance)
						{
							who.setNewDialogue("Strings\\StringsFromCSFiles:Event.cs.1635", false, false);
						}
						else
						{
							Dialogue dialogue = who.TryGetDialogue("FlowerDance_Decline") ?? who.TryGetDialogue("danceRejection");
							if (dialogue == null)
							{
								return;
							}
							who.setNewDialogue(dialogue, false, false);
						}
						IL_2F2:
						Game1.drawDialogue(who);
						who.immediateSpeak = true;
						who.facePlayer(Game1.player);
						who.Halt();
					}
				}
				else
				{
					if (!Game1.HasDedicatedHost)
					{
						this.forceFestivalContinue();
						return;
					}
					if (!this.isSpecificFestival("fall16"))
					{
						string festivalCheck = "MainEvent_" + this.id;
						Game1.netReady.SetLocalReady(festivalCheck, true);
						Game1.activeClickableMenu = new ReadyCheckDialog(festivalCheck, true, delegate(Farmer farmer)
						{
							this.forceFestivalContinue();
						}, null);
						return;
					}
					if (Game1.IsServer)
					{
						this.forceFestivalContinue();
						return;
					}
					Game1.dedicatedServer.DoHostAction("JudgeGrange", Array.Empty<object>());
					return;
				}
			}
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x000462FC File Offset: 0x000444FC
		public void addItemToGrangeDisplay(Item i, int position, bool force)
		{
			while (Game1.player.team.grangeDisplay.Count < 9)
			{
				Game1.player.team.grangeDisplay.Add(null);
			}
			if (position < 0 || position >= Game1.player.team.grangeDisplay.Count || (Game1.player.team.grangeDisplay[position] != null && !force))
			{
				return;
			}
			Game1.player.team.grangeDisplay[position] = i;
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x00046384 File Offset: 0x00044584
		private bool onGrangeChange(Item i, int position, Item old, StorageContainer container, bool onRemoval)
		{
			if (!onRemoval)
			{
				if (i.Stack > 1 || (i.Stack == 1 && old != null && old.Stack == 1 && i.canStackWith(old)))
				{
					if (old != null && i != null && old.canStackWith(i))
					{
						container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
						container.heldItem = old;
						return false;
					}
					if (old != null)
					{
						Utility.addItemToInventory(old, position, container.ItemsToGrabMenu.actualInventory, null);
						container.heldItem = i;
						return false;
					}
					int allButOne = i.Stack - 1;
					Item reject = i.getOne();
					reject.Stack = allButOne;
					container.heldItem = reject;
					i.Stack = 1;
				}
			}
			else if (old != null && old.Stack > 1 && !old.Equals(i))
			{
				return false;
			}
			this.addItemToGrangeDisplay((onRemoval && (old == null || old.Equals(i))) ? null : i, position, true);
			return true;
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x0004647A File Offset: 0x0004467A
		public bool canPlayerUseTool()
		{
			if (this.isSpecificFestival("winter8") && this.festivalTimer > 0 && !Game1.player.UsingTool)
			{
				this.previousFacingDirection = Game1.player.FacingDirection;
				return true;
			}
			return false;
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x000464B4 File Offset: 0x000446B4
		public bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (this.isFestival)
			{
				Object tempObj;
				if (this.temporaryLocation != null && this.temporaryLocation.objects.TryGetValue(new Vector2((float)tileLocation.X, (float)tileLocation.Y), out tempObj))
				{
					tempObj.checkForAction(who, false);
				}
				GameLocation location = Game1.currentLocation;
				string a = this.id;
				if (!(a == "festival_fall16"))
				{
					if (!(a == "festival_fall27"))
					{
						if (a == "festival_winter8")
						{
							int num = location.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "fest");
							if (num - 1009 <= 1 || num - 1012 <= 1)
							{
								Game1.playSound("pig", null);
								return true;
							}
						}
					}
					else if (location.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "Landscape") == 958 && ((tileLocation.X == 44 && tileLocation.Y == 9) || (tileLocation.X == 61 && tileLocation.Y == 13)))
					{
						if (who.IsLocalPlayer)
						{
							location.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SpiritsEveCart"), location.createYesNoResponses(), "spirits_eve_shortcut");
						}
						return true;
					}
				}
				else
				{
					int num = location.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "untitled tile sheet");
					if (num <= 309)
					{
						if (num - 87 <= 1)
						{
							if (who.IsLocalPlayer)
							{
								Response[] responses = new Response[]
								{
									new Response("Buy", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1654")),
									new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1656"))
								};
								location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1659"), responses, "StarTokenShop");
							}
							return true;
						}
						if (num - 175 <= 1)
						{
							if (who.IsLocalPlayer)
							{
								Game1.player.eatObject(ItemRegistry.Create<Object>("(O)241", 1, 0, false), true);
							}
							return true;
						}
						if (num - 308 <= 1)
						{
							if (who.IsLocalPlayer)
							{
								Response[] colors = new Response[]
								{
									new Response("Orange", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1645")),
									new Response("Green", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1647")),
									new Response("I", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1650"))
								};
								location.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1652")), colors, "wheelBet");
							}
							return true;
						}
					}
					else
					{
						if (num - 349 <= 2)
						{
							Game1.player.team.grangeMutex.RequestLock(delegate
							{
								while (Game1.player.team.grangeDisplay.Count < 9)
								{
									Game1.player.team.grangeDisplay.Add(null);
								}
								Game1.activeClickableMenu = new StorageContainer(Game1.player.team.grangeDisplay.ToList<Item>(), 9, 3, new StorageContainer.behaviorOnItemChange(this.onGrangeChange), new InventoryMenu.highlightThisItem(Utility.highlightSmallObjects));
							}, null);
							return true;
						}
						switch (num)
						{
						case 501:
						case 502:
							if (who.IsLocalPlayer)
							{
								Response[] responses2 = new Response[]
								{
									new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
									new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
								};
								location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1666"), responses2, "slingshotGame");
							}
							return true;
						case 503:
						case 504:
							if (who.IsLocalPlayer)
							{
								Response[] responses3 = new Response[]
								{
									new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
									new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
								};
								location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1681"), responses3, "fishingGame");
							}
							return true;
						case 505:
						case 506:
							if (who.IsLocalPlayer)
							{
								if (who.Money >= 100 && !who.mailReceived.Contains("fortuneTeller" + Game1.year.ToString()))
								{
									Response[] responses4 = new Response[]
									{
										new Response("Read", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1688")),
										new Response("No", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1690"))
									};
									location.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1691")), responses4, "fortuneTeller");
								}
								else if (who.mailReceived.Contains("fortuneTeller" + Game1.year.ToString()))
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1694")));
								}
								else
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1695")));
								}
								who.Halt();
							}
							return true;
						case 507:
						case 508:
						case 509:
							break;
						case 510:
						case 511:
							if (who.IsLocalPlayer)
							{
								location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1672"), location.createYesNoResponses(), "starTokenShop");
							}
							return true;
						default:
							if (num == 540)
							{
								if (who.IsLocalPlayer)
								{
									if (who.TilePoint.X == 29)
									{
										Game1.activeClickableMenu = new StrengthGame();
									}
									else
									{
										Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1684")));
									}
								}
								return true;
							}
							break;
						}
					}
				}
				string tileAction = location.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings", false);
				if (tileAction != null)
				{
					try
					{
						string[] args = ArgUtility.SplitBySpace(tileAction);
						a = ArgUtility.Get(args, 0, null, true);
						if (!(a == "OpenShop") && !(a == "Shop"))
						{
							if (!(a == "Message"))
							{
								if (!(a == "Dialogue"))
								{
									if (a == "LuauSoup")
									{
										if (!this.specialEventVariable2)
										{
											Game1.activeClickableMenu = new ItemGrabMenu(null, true, false, new InventoryMenu.highlightThisItem(Utility.highlightLuauSoupItems), new ItemGrabMenu.behaviorOnItemSelect(this.clickToAddItemToLuauSoup), Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1719"), null, false, true, true, true, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
										}
									}
								}
								else
								{
									string dialogue;
									string error;
									if (!ArgUtility.TryGetRemainder(args, 1, out dialogue, out error, ' ', "string dialogue"))
									{
										location.LogTileActionError(args, tileLocation.X, tileLocation.Y, error);
										return false;
									}
									Game1.drawObjectDialogue(dialogue.Replace("#", " "));
								}
							}
							else
							{
								string translationKey;
								string error2;
								if (!ArgUtility.TryGet(args, 1, out translationKey, out error2, true, "string translationKey"))
								{
									location.LogTileActionError(args, tileLocation.X, tileLocation.Y, error2);
									return false;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + translationKey.Replace("\"", "")));
							}
						}
						else
						{
							string shop_id;
							string error3;
							if (!ArgUtility.TryGet(args, 1, out shop_id, out error3, true, "string shop_id"))
							{
								location.LogTileActionError(args, tileLocation.X, tileLocation.Y, error3);
								return false;
							}
							if (!who.IsLocalPlayer)
							{
								return false;
							}
							bool opened = false;
							if (shop_id == "shop" && this.isFestival)
							{
								string text = this.id;
								if (text != null)
								{
									switch (text.Length)
									{
									case 15:
										if (text == "festival_fall27")
										{
											shop_id = "Festival_SpiritsEve_Pierre";
										}
										break;
									case 16:
										if (text == "festival_winter8")
										{
											shop_id = "Festival_FestivalOfIce_TravelingMerchant";
										}
										break;
									case 17:
										switch (text[16])
										{
										case '1':
											if (text == "festival_summer11")
											{
												shop_id = "Festival_Luau_Pierre";
											}
											break;
										case '3':
											if (text == "festival_spring13")
											{
												shop_id = "Festival_EggFestival_Pierre";
											}
											break;
										case '4':
											if (text == "festival_spring24")
											{
												shop_id = "Festival_FlowerDance_Pierre";
											}
											break;
										case '5':
											if (text == "festival_winter25")
											{
												shop_id = "Festival_FeastOfTheWinterStar_Pierre";
											}
											break;
										case '8':
											if (text == "festival_summer28")
											{
												shop_id = "Festival_DanceOfTheMoonlightJellies_Pierre";
											}
											break;
										}
										break;
									}
								}
							}
							string legacyShopData;
							if (this.festivalData.TryGetValue(shop_id, out legacyShopData))
							{
								if (this.festivalShops == null)
								{
									this.festivalShops = new Dictionary<string, Dictionary<ISalable, ItemStockInformation>>();
								}
								Dictionary<ISalable, ItemStockInformation> stockList;
								if (!this.festivalShops.TryGetValue(shop_id, out stockList))
								{
									string[] inventoryList = ArgUtility.SplitBySpace(legacyShopData);
									stockList = new Dictionary<ISalable, ItemStockInformation>();
									for (int i = 0; i < inventoryList.Length; i += 4)
									{
										string type;
										string itemId;
										int price;
										int stock;
										if (!ArgUtility.TryGet(args, i, out type, out error3, true, "string type") || !ArgUtility.TryGet(args, i + 1, out itemId, out error3, true, "string itemId") || !ArgUtility.TryGetInt(args, i + 2, out price, out error3, "int price") || !ArgUtility.TryGetInt(args, i + 3, out stock, out error3, "int stock"))
										{
											IGameLogger log = Game1.log;
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 3);
											defaultInterpolatedStringHandler.AppendLiteral("Festival '");
											defaultInterpolatedStringHandler.AppendFormatted(this.id);
											defaultInterpolatedStringHandler.AppendLiteral("' has legacy shop inventory '");
											defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", inventoryList));
											defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed: ");
											defaultInterpolatedStringHandler.AppendFormatted(error3);
											defaultInterpolatedStringHandler.AppendLiteral(".");
											log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
											break;
										}
										Item item = Utility.getItemFromStandardTextDescription(type, itemId, stock, who);
										if (item != null)
										{
											if (item.Category == -74)
											{
												price = (int)Math.Max(1f, (float)price * Game1.MasterPlayer.difficultyModifier);
											}
											if (!item.IsRecipe || !who.knowsRecipe(item.Name))
											{
												stockList.Add(item, new ItemStockInformation(price, (stock <= 0) ? int.MaxValue : stock, null, null, LimitedStockMode.Player, null, null, null, null));
											}
										}
									}
									this.festivalShops[shop_id] = stockList;
								}
								if (stockList != null && stockList.Count > 0)
								{
									who.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(who.currentLocation.Name + shop_id, stockList);
									Game1.activeClickableMenu = new ShopMenu(this.id + "_" + shop_id, stockList, 0, null, null, null, true);
									opened = true;
								}
							}
							bool showedClosedMessage = false;
							if (!opened && Utility.TryOpenShopMenu(shop_id, this.temporaryLocation, null, null, false, true, delegate(string message)
							{
								showedClosedMessage = true;
								Game1.drawObjectDialogue(message);
							}))
							{
								opened = true;
							}
							if (!opened && !showedClosedMessage)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1714"));
							}
						}
					}
					catch (Exception)
					{
					}
					return false;
				}
				if (who.IsLocalPlayer && (!this.playerControlSequence || !this.playerControlSequenceID.Equals("iceFishing")))
				{
					foreach (NPC j in this.actors)
					{
						Point tile = j.TilePoint;
						Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
						if (tile.X == tileLocation.X && tile.Y == tileLocation.Y)
						{
							Child child = j as Child;
							if (child != null)
							{
								child.checkAction(who, this.temporaryLocation);
								return true;
							}
						}
						if (((tile.X == tileLocation.X && (tile.Y == tileLocation.Y || tile.Y == tileLocation.Y + 1)) || j.GetBoundingBox().Intersects(tileRect)) && (j.CurrentDialogue.Count >= 1 || (j.CurrentDialogue.Count > 0 && !j.CurrentDialogue.Peek().isOnFinalDialogue()) || (j.Equals(this.festivalHost) || (j.datable.Value && this.isSpecificFestival("spring24"))) || (this.secretSantaRecipient != null && j.Name.Equals(this.secretSantaRecipient.Name))))
						{
							Friendship friendship;
							bool divorced = who.friendshipData.TryGetValue(j.Name, out friendship) && friendship.IsDivorced();
							if ((this.grangeScore > -100 || this.grangeScore == -666) && j.Equals(this.festivalHost) && this.grangeJudged)
							{
								Dialogue message2;
								if (this.grangeScore >= 90)
								{
									Game1.playSound("reward", null);
									message2 = Dialogue.FromTranslation(j, "Strings\\StringsFromCSFiles:Event.cs.1723", this.grangeScore);
									Game1.player.festivalScore += 1000;
									Game1.getAchievement(37, true);
								}
								else if (this.grangeScore >= 75)
								{
									Game1.playSound("reward", null);
									message2 = Dialogue.FromTranslation(j, "Strings\\StringsFromCSFiles:Event.cs.1726", this.grangeScore);
									Game1.player.festivalScore += 500;
								}
								else if (this.grangeScore >= 60)
								{
									Game1.playSound("newArtifact", null);
									message2 = Dialogue.FromTranslation(j, "Strings\\StringsFromCSFiles:Event.cs.1729", this.grangeScore);
									Game1.player.festivalScore += 250;
								}
								else if (this.grangeScore == -666)
								{
									Game1.playSound("secret1", null);
									message2 = new Dialogue(j, "Strings\\StringsFromCSFiles:Event.cs.1730", false);
									Game1.player.festivalScore += 750;
								}
								else
								{
									Game1.playSound("newArtifact", null);
									message2 = Dialogue.FromTranslation(j, "Strings\\StringsFromCSFiles:Event.cs.1732", this.grangeScore);
									Game1.player.festivalScore += 50;
								}
								this.grangeScore = -100;
								j.setNewDialogue(message2, false, false);
							}
							else if ((Game1.HasDedicatedHost || Game1.serverHost == null || Game1.player.Equals(Game1.serverHost.Value)) && j.Equals(this.festivalHost) && (j.CurrentDialogue.Count == 0 || j.CurrentDialogue.Peek().isOnFinalDialogue()) && this.hostMessageKey != null)
							{
								j.setNewDialogue(this.hostMessageKey, false, false);
							}
							if (this.isSpecificFestival("spring24") && !divorced)
							{
								CharacterData data = j.GetData();
								if (((data != null) ? data.FlowerDanceCanDance : null) ?? (j.datable.Value || j.Name == who.spouse))
								{
									j.grantConversationFriendship(who, 20);
									if (who.dancePartner.Value == null)
									{
										if (j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
										{
											j.CurrentDialogue.Clear();
										}
										if (j.CurrentDialogue.Count == 0)
										{
											j.CurrentDialogue.Push(new Dialogue(j, null, "..."));
											if (j.name.Value == who.spouse)
											{
												j.setNewDialogue(Dialogue.FromTranslation(j, "Strings\\StringsFromCSFiles:Event.cs.1736", j.displayName), true, false);
											}
											else
											{
												j.setNewDialogue(Dialogue.FromTranslation(j, "Strings\\StringsFromCSFiles:Event.cs.1738", j.displayName), true, false);
											}
										}
										else if (j.CurrentDialogue.Peek().isOnFinalDialogue())
										{
											Dialogue d = j.CurrentDialogue.Peek();
											if (who.spouse != null && j.Name == who.spouse)
											{
												Dialogue dialogue2 = null;
												if (j.isRoommate())
												{
													this.TryGetFestivalDialogueForYear(j, j.Name + "_roommate", out dialogue2);
												}
												if (dialogue2 == null)
												{
													this.TryGetFestivalDialogueForYear(j, j.Name + "_spouse", out dialogue2);
												}
												if (dialogue2 != null)
												{
													j.CurrentDialogue.Clear();
													j.CurrentDialogue.Push(dialogue2);
													d = j.CurrentDialogue.Peek();
												}
											}
											Game1.drawDialogue(j);
											j.faceTowardFarmerForPeriod(3000, 2, false, who);
											who.Halt();
											j.CurrentDialogue = new Stack<Dialogue>();
											j.CurrentDialogue.Push(new Dialogue(j, null, "..."));
											j.CurrentDialogue.Push(d);
											return true;
										}
									}
									else if (j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
									{
										j.CurrentDialogue.Clear();
									}
								}
							}
							if (!divorced && this.secretSantaRecipient != null && j.Name.Equals(this.secretSantaRecipient.Name))
							{
								j.grantConversationFriendship(who, 20);
								location.createQuestionDialogue(Game1.parseText((this.secretSantaRecipient.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1740", this.secretSantaRecipient.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1741", this.secretSantaRecipient.displayName)), location.createYesNoResponses(), "secretSanta");
								who.Halt();
								return true;
							}
							if (j.CurrentDialogue.Count == 0)
							{
								return true;
							}
							if (who.spouse != null && j.Name == who.spouse && !this.isSpecificFestival("spring24"))
							{
								Dialogue dialogue3 = null;
								if (j.isRoommate())
								{
									this.TryGetFestivalDialogueForYear(j, j.Name + "_roommate", out dialogue3);
								}
								if (dialogue3 == null)
								{
									this.TryGetFestivalDialogueForYear(j, j.Name + "_spouse", out dialogue3);
								}
								if (dialogue3 != null && (j.CurrentDialogue.Count == 0 || !j.CurrentDialogue.Peek().TranslationKey.Equals(dialogue3.TranslationKey)))
								{
									j.CurrentDialogue.Clear();
									j.CurrentDialogue.Push(dialogue3);
								}
							}
							if (divorced)
							{
								j.CurrentDialogue.Clear();
								j.CurrentDialogue.Push(new Dialogue(j, "Characters\\Dialogue\\" + j.Name + ":divorced", false));
							}
							j.grantConversationFriendship(who, 20);
							if (j.CurrentDialogue == null || j.CurrentDialogue.Count == 0 || !j.CurrentDialogue.Peek().dontFaceFarmer)
							{
								j.faceTowardFarmerForPeriod(3000, 2, false, who);
							}
							Game1.drawDialogue(j);
							who.Halt();
							return true;
						}
					}
				}
				if (this.festivalData != null && this.isSpecificFestival("spring13"))
				{
					Microsoft.Xna.Framework.Rectangle tile2 = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
					for (int k = this.festivalProps.Count - 1; k >= 0; k--)
					{
						if (this.festivalProps[k].isColliding(tile2))
						{
							int num = who.festivalScore;
							who.festivalScore = num + 1;
							this.festivalProps.RemoveAt(k);
							who.team.FestivalPropsRemoved(tile2);
							if (who.IsLocalPlayer)
							{
								Game1.playSound("coin", null);
							}
							return true;
						}
					}
				}
				foreach (MapSeat seat in location.mapSeats)
				{
					if (seat.OccupiesTile(tileLocation.X, tileLocation.Y) && !seat.IsBlocked(location))
					{
						who.BeginSitting(seat);
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00047994 File Offset: 0x00045B94
		public void removeFestivalProps(Microsoft.Xna.Framework.Rectangle rect)
		{
			this.festivalProps.RemoveAll((Prop prop) => prop.isColliding(rect));
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x000479C6 File Offset: 0x00045BC6
		public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
		{
			if (this.isFestival && this.festivalHost != null && this.festivalHost.Tile == tileLocation)
			{
				Game1.mouseCursor = Game1.cursor_talk;
			}
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x000479F5 File Offset: 0x00045BF5
		public void forceEndFestival(Farmer who)
		{
			Game1.currentMinigame = null;
			Game1.exitActiveMenu();
			Game1.player.Halt();
			this.endBehaviors(null);
			if (Game1.IsServer)
			{
				Game1.multiplayer.sendServerToClientsMessage("endFest");
			}
			Game1.changeMusicTrack("none", false, MusicContext.Default);
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00047A38 File Offset: 0x00045C38
		public bool checkForCollision(Microsoft.Xna.Framework.Rectangle position, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
			foreach (NPC i in this.actors)
			{
				Microsoft.Xna.Framework.Rectangle actorBounds = i.GetBoundingBox();
				if (actorBounds.Intersects(position) && !this.farmer.temporarilyInvincible && this.farmer.TemporaryPassableTiles.IsEmpty() && !i.IsInvisible && !playerBounds.Intersects(actorBounds) && !i.farmerPassesThrough)
				{
					return true;
				}
			}
			if (Game1.currentLocation.IsOutOfBounds(position))
			{
				this.TryStartEndFestivalDialogue(who);
				return true;
			}
			using (List<Object>.Enumerator enumerator2 = this.props.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.GetBoundingBox().Intersects(position))
					{
						return true;
					}
				}
			}
			if (this.temporaryLocation != null)
			{
				using (Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator3 = this.temporaryLocation.objects.Values.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						if (enumerator3.Current.GetBoundingBox().Intersects(position))
						{
							return true;
						}
					}
				}
			}
			using (List<Prop>.Enumerator enumerator4 = this.festivalProps.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					if (enumerator4.Current.isColliding(position))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00047BF8 File Offset: 0x00045DF8
		public bool TryStartEndFestivalDialogue(Farmer who)
		{
			if (!who.IsLocalPlayer || !this.isFestival)
			{
				return false;
			}
			who.Halt();
			who.Position = who.lastPosition;
			if (!Game1.IsMultiplayer && Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1758", this.FestivalName), new ConfirmationDialog.behavior(this.forceEndFestival), null);
			}
			else if (Game1.activeClickableMenu == null)
			{
				Game1.netReady.SetLocalReady("festivalEnd", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", true, new ConfirmationDialog.behavior(this.forceEndFestival), null);
			}
			return true;
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00047C9C File Offset: 0x00045E9C
		public void answerDialogue(string questionKey, int answerChoice)
		{
			this.previousAnswerChoice = answerChoice;
			if (questionKey.Contains("fork"))
			{
				int forkAnswer = Convert.ToInt32(questionKey.Replace("fork", ""));
				if (answerChoice == forkAnswer)
				{
					this.specialEventVariable1 = !this.specialEventVariable1;
					return;
				}
			}
			else
			{
				if (questionKey.Contains("quickQuestion"))
				{
					string text = this.eventCommands[Math.Min(this.eventCommands.Length - 1, this.CurrentCommand)];
					string[] newCommands = text.Substring(text.IndexOf(' ') + 1).Split("(break)", StringSplitOptions.None)[1 + answerChoice].Split('\\', StringSplitOptions.None);
					List<string> tmp = this.eventCommands.ToList<string>();
					tmp.InsertRange(this.CurrentCommand + 1, newCommands);
					this.eventCommands = tmp.ToArray();
					return;
				}
				if (questionKey != null)
				{
					int length = questionKey.Length;
					switch (length)
					{
					case 3:
						if (!(questionKey == "pet"))
						{
							return;
						}
						if (answerChoice == 0)
						{
							string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
							string defaultName;
							if (Game1.player.IsMale)
							{
								defaultName = Game1.content.LoadString(Game1.player.catPerson ? "Strings\\StringsFromCSFiles:Event.cs.1794" : "Strings\\StringsFromCSFiles:Event.cs.1795");
							}
							else
							{
								defaultName = Game1.content.LoadString((Game1.player.whichPetType == "Dog") ? "Strings\\StringsFromCSFiles:Event.cs.1797" : "Strings\\StringsFromCSFiles:Event.cs.1796");
							}
							Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(this.namePet), title, defaultName);
							return;
						}
						Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "rejectedPet", MailType.Received, true, null);
						this.eventCommands = new string[2];
						this.eventCommands[1] = "end";
						this.eventCommands[0] = "speak Marnie \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1798") + "\"";
						this.currentCommand = 0;
						this.eventSwitched = true;
						this.specialEventVariable1 = true;
						break;
					case 4:
						if (!(questionKey == "cave"))
						{
							return;
						}
						Game1.dedicatedServer.DoHostAction("ChooseCave", new object[]
						{
							answerChoice
						});
						return;
					case 5:
					case 6:
					case 7:
					case 10:
					case 12:
					case 14:
						break;
					case 8:
					{
						char c = questionKey[0];
						if (c != 'b')
						{
							if (c != 'w')
							{
								return;
							}
							if (!(questionKey == "wheelBet"))
							{
								return;
							}
							this.specialEventVariable2 = (answerChoice == 1);
							if (answerChoice != 2)
							{
								Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1776"), new NumberSelectionMenu.behaviorOnNumberSelect(this.betStarTokens), -1, 1, Game1.player.festivalScore, Math.Min(1, Game1.player.festivalScore));
								return;
							}
						}
						else
						{
							if (!(questionKey == "bandFork"))
							{
								return;
							}
							switch (answerChoice)
							{
							case 76:
								this.specialEventVariable1 = true;
								this.eventCommands[this.currentCommand + 1] = "fork poppy";
								return;
							case 77:
								this.specialEventVariable1 = true;
								this.eventCommands[this.currentCommand + 1] = "fork heavy";
								return;
							case 78:
								this.specialEventVariable1 = true;
								this.eventCommands[this.currentCommand + 1] = "fork techno";
								return;
							case 79:
								this.specialEventVariable1 = true;
								this.eventCommands[this.currentCommand + 1] = "fork honkytonk";
								return;
							default:
								return;
							}
						}
						break;
					}
					case 9:
						if (!(questionKey == "shaneLoan"))
						{
							return;
						}
						if (answerChoice != 0)
						{
							return;
						}
						this.specialEventVariable1 = true;
						this.eventCommands[this.currentCommand + 1] = "fork giveShaneLoan";
						Game1.player.Money -= 3000;
						return;
					case 11:
						switch (questionKey[1])
						{
						case 'e':
							if (!(questionKey == "secretSanta"))
							{
								return;
							}
							if (answerChoice == 0)
							{
								Game1.activeClickableMenu = new ItemGrabMenu(null, true, false, new InventoryMenu.highlightThisItem(Utility.highlightSantaObjects), new ItemGrabMenu.behaviorOnItemSelect(this.chooseSecretSantaGift), Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1788", this.secretSantaRecipient.displayName), null, false, false, true, true, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
								return;
							}
							break;
						case 'f':
						case 'g':
							break;
						case 'h':
							if (!(questionKey == "shaneCliffs"))
							{
								return;
							}
							switch (answerChoice)
							{
							case 0:
								this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1760") + "\"";
								return;
							case 1:
								this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1761") + "\"";
								return;
							case 2:
								this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1763") + "\"";
								return;
							case 3:
								this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1764") + "\"";
								return;
							default:
								return;
							}
							break;
						case 'i':
							if (!(questionKey == "fishingGame"))
							{
								return;
							}
							if (answerChoice == 0)
							{
								if (Game1.player.Money >= 50)
								{
									Game1.globalFadeToBlack(new Game1.afterFadeFunction(FishingGame.startMe), 0.01f);
									Game1.player.Money -= 50;
									return;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
								return;
							}
							break;
						default:
							return;
						}
						break;
					case 13:
					{
						char c = questionKey[0];
						if (c <= 'f')
						{
							if (c != 'S')
							{
								if (c != 'f')
								{
									return;
								}
								if (!(questionKey == "fortuneTeller"))
								{
									return;
								}
								if (answerChoice == 0)
								{
									Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.readFortune), 0.02f);
									Game1.player.Money -= 100;
									Game1.player.mailReceived.Add("fortuneTeller" + Game1.year.ToString());
									return;
								}
							}
							else
							{
								if (!(questionKey == "StarTokenShop"))
								{
									return;
								}
								if (answerChoice == 0)
								{
									Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1774"), new NumberSelectionMenu.behaviorOnNumberSelect(this.buyStarTokens), 50, 0, 999, 0);
									return;
								}
							}
						}
						else if (c != 'h')
						{
							if (c != 's')
							{
								return;
							}
							if (!(questionKey == "slingshotGame"))
							{
								if (!(questionKey == "starTokenShop"))
								{
									return;
								}
								if (answerChoice == 0 && Utility.TryOpenShopMenu("Festival_StardewValleyFair_StarTokens", this.temporaryLocation, null, null, false, false, null))
								{
									ShopMenu shop = Game1.activeClickableMenu as ShopMenu;
									if (shop != null)
									{
										if (shop.IsOutOfStock())
										{
											shop.exitThisMenuNoSound();
											Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1785")));
											return;
										}
										shop.PlayOpenSound();
										return;
									}
								}
							}
							else if (answerChoice == 0)
							{
								if (Game1.player.Money >= 50)
								{
									Game1.globalFadeToBlack(new Game1.afterFadeFunction(TargetGame.startMe), 0.01f);
									Game1.player.Money -= 50;
									return;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
								return;
							}
						}
						else
						{
							if (!(questionKey == "haleyDarkRoom"))
							{
								return;
							}
							switch (answerChoice)
							{
							case 0:
								this.specialEventVariable1 = true;
								this.eventCommands[this.currentCommand + 1] = "fork decorate";
								return;
							case 1:
								this.specialEventVariable1 = true;
								this.eventCommands[this.currentCommand + 1] = "fork leave";
								return;
							case 2:
								break;
							default:
								return;
							}
						}
						break;
					}
					case 15:
						if (!(questionKey == "chooseCharacter"))
						{
							return;
						}
						switch (answerChoice)
						{
						case 0:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork warrior";
							return;
						case 1:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork healer";
							return;
						case 2:
							break;
						default:
							return;
						}
						break;
					default:
						if (length != 20)
						{
							return;
						}
						if (!(questionKey == "spirits_eve_shortcut"))
						{
							return;
						}
						if (answerChoice == 0)
						{
							Game1.player.freezePause = 2000;
							Game1.globalFadeToBlack(delegate
							{
								Game1.player.Position = new Vector2(32f, 49f) * 64f;
								Game1.player.faceDirection(2);
								Game1.playSound("stairsdown", null);
								Game1.globalFadeToClear(null, 0.02f);
							}, 0.02f);
							return;
						}
						break;
					}
				}
			}
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x000484FE File Offset: 0x000466FE
		internal static void hostActionChooseCave(Farmer who, BinaryReader reader)
		{
			if (reader.ReadInt32() == 0)
			{
				Game1.MasterPlayer.caveChoice.Value = 2;
				Game1.RequireLocation<FarmCave>("FarmCave", false).setUpMushroomHouse();
				return;
			}
			Game1.MasterPlayer.caveChoice.Value = 1;
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x0004853C File Offset: 0x0004673C
		internal static void hostActionNamePet(Farmer who, BinaryReader reader)
		{
			string name = reader.ReadString();
			Pet p = new Pet(68, 13, Game1.player.whichPetBreed, Game1.player.whichPetType);
			p.warpToFarmHouse(Game1.player);
			p.Name = name;
			p.displayName = p.name.Value;
			foreach (Building building in Game1.getFarm().buildings)
			{
				PetBowl bowl = building as PetBowl;
				if (bowl != null && !bowl.HasPet())
				{
					bowl.AssignPet(p);
					break;
				}
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				farmer.autoGenerateActiveDialogueEvent("gotPet", 4);
			}
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00048634 File Offset: 0x00046834
		private void namePet(string name)
		{
			this.gotPet = true;
			Game1.dedicatedServer.DoHostAction("NamePet", new object[]
			{
				name
			});
			Game1.exitActiveMenu();
			int num = this.CurrentCommand;
			this.CurrentCommand = num + 1;
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00048678 File Offset: 0x00046878
		public void chooseSecretSantaGift(Item i, Farmer who)
		{
			if (i == null)
			{
				return;
			}
			Object obj = i as Object;
			if (obj != null)
			{
				if (obj.Stack > 1)
				{
					Object obj2 = obj;
					int stack = obj2.Stack;
					obj2.Stack = stack - 1;
					who.addItemToInventory(obj);
				}
				Game1.exitActiveMenu();
				NPC recipient = this.getActorByName(this.secretSantaRecipient.Name, false);
				recipient.faceTowardFarmerForPeriod(15000, 5, false, who);
				recipient.receiveGift(obj, who, false, 5f, false);
				recipient.CurrentDialogue.Clear();
				string article = Lexicon.getProperArticleForWord(obj.DisplayName);
				Stack<Dialogue> currentDialogue = recipient.CurrentDialogue;
				Dialogue item;
				if ((item = recipient.TryGetDialogue("WinterStar_ReceiveGift_" + obj.QualifiedItemId, new object[]
				{
					obj.DisplayName,
					article
				})) == null)
				{
					if ((item = (from tag in obj.GetContextTags()
					select recipient.TryGetDialogue("WinterStar_ReceiveGift_" + tag, new object[]
					{
						obj.DisplayName,
						article
					})).FirstOrDefault((Dialogue p) => p != null)) == null)
					{
						item = (recipient.TryGetDialogue("WinterStar_ReceiveGift", new object[]
						{
							obj.DisplayName,
							article
						}) ?? Dialogue.FromTranslation(recipient, "Strings\\StringsFromCSFiles:Event.cs.1801", obj.DisplayName, article));
					}
				}
				currentDialogue.Push(item);
				Game1.drawDialogue(recipient);
				this.secretSantaRecipient = null;
				this.startSecretSantaAfterDialogue = true;
				who.Halt();
				who.completelyStopAnimatingOrDoingAction();
				who.faceGeneralDirection(recipient.Position, 0, false, false);
				return;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1803"));
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00048880 File Offset: 0x00046A80
		public void perfectFishing()
		{
			if (this.isFestival)
			{
				FishingGame fishingGame = Game1.currentMinigame as FishingGame;
				if (fishingGame != null && this.isSpecificFestival("fall16"))
				{
					fishingGame.perfections++;
				}
			}
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x000488C0 File Offset: 0x00046AC0
		public void caughtFish(string itemId, int size, Farmer who)
		{
			if (itemId == null)
			{
				return;
			}
			if (this.isFestival)
			{
				FishingGame fishingGame = Game1.currentMinigame as FishingGame;
				if (fishingGame != null && this.isSpecificFestival("fall16"))
				{
					fishingGame.score += ((size > 0) ? (size + 5) : 1);
					if (size > 0)
					{
						fishingGame.fishCaught++;
					}
					Game1.player.FarmerSprite.PauseForSingleAnimation = false;
					Game1.player.FarmerSprite.StopAnimation();
					return;
				}
				if (this.isSpecificFestival("winter8"))
				{
					if (size > 0 && who.TilePoint.X < 79 && who.TilePoint.Y < 43)
					{
						int festivalScore = who.festivalScore;
						who.festivalScore = festivalScore + 1;
						Game1.playSound("newArtifact", null);
					}
					who.forceCanMove();
					if (this.previousFacingDirection != -1)
					{
						who.faceDirection(this.previousFacingDirection);
					}
				}
			}
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x000489B0 File Offset: 0x00046BB0
		public void readFortune()
		{
			Game1.globalFade = true;
			Game1.fadeToBlackAlpha = 1f;
			NPC topRomance = Utility.getTopRomanticInterest(Game1.player);
			NPC topFriend = Utility.getTopNonRomanticInterest(Game1.player);
			int topSkill = Utility.getHighestSkill(Game1.player);
			string[] fortune = new string[5];
			if (topFriend != null && Game1.player.getFriendshipLevelForNPC(topFriend.Name) > 100)
			{
				if (Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topFriend.Name) - 100, Game1.player.getFriendshipLevelForNPC(topFriend.Name), false) > 3 && Game1.random.NextBool())
				{
					fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1810");
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1811", topFriend.displayName);
						break;
					case 1:
						fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1813", topFriend.displayName) + ((topFriend.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1815") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1816"));
						break;
					case 2:
						fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1818", topFriend.displayName);
						break;
					case 3:
						fortune[0] = ((topFriend.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1820") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1821")) + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1823", topFriend.displayName);
						break;
					}
				}
			}
			else
			{
				fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1825");
			}
			if (topRomance != null && Game1.player.getFriendshipLevelForNPC(topRomance.Name) > 250)
			{
				if (Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topRomance.Name) - 100, Game1.player.getFriendshipLevelForNPC(topRomance.Name), true) > 2)
				{
					fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826");
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", topRomance.displayName);
						break;
					case 1:
						fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1829", topRomance.displayName);
						break;
					case 2:
						fortune[1] = ((topRomance.Gender == Gender.Male) ? ((topRomance.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832")) : ((topRomance.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834"))) + " " + ((topRomance.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", topRomance.displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", topRomance.displayName[0]));
						break;
					case 3:
						fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", topRomance.displayName);
						break;
					}
				}
			}
			else
			{
				fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1845");
			}
			switch (topSkill)
			{
			case 0:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1846");
				break;
			case 1:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1849");
				break;
			case 2:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1850");
				break;
			case 3:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1847");
				break;
			case 4:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1848");
				break;
			case 5:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1851");
				break;
			}
			fortune[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1852");
			fortune[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1853");
			Game1.multipleDialogues(fortune);
			Game1.afterDialogues = new Game1.afterFadeFunction(this.fadeClearAndviewportUnfreeze);
			Game1.viewportFreeze = true;
			Game1.viewport.X = -9999;
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00048DF8 File Offset: 0x00046FF8
		public void fadeClearAndviewportUnfreeze()
		{
			Game1.fadeClear();
			Game1.viewportFreeze = false;
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00048E08 File Offset: 0x00047008
		public void betStarTokens(int value, int price, Farmer who)
		{
			if (value <= who.festivalScore)
			{
				Game1.playSound("smallSelect", null);
				Game1.activeClickableMenu = new WheelSpinGame(value);
			}
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x00048E40 File Offset: 0x00047040
		public void buyStarTokens(int value, int price, Farmer who)
		{
			if (value > 0 && value * price <= who.Money)
			{
				who.Money -= price * value;
				who.festivalScore += value;
				Game1.playSound("purchase", null);
				Game1.exitActiveMenu();
			}
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x00048E93 File Offset: 0x00047093
		public void clickToAddItemToLuauSoup(Item i, Farmer who)
		{
			this.addItemToLuauSoup(i, who);
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00048EA0 File Offset: 0x000470A0
		public void setUpAdvancedMove(string[] args, NPCController.endBehavior endBehavior = null)
		{
			string actorName;
			string error;
			bool loop;
			if (!ArgUtility.TryGet(args, 1, out actorName, out error, false, "string actorName") || !ArgUtility.TryGetBool(args, 2, out loop, out error, "bool loop"))
			{
				this.LogCommandError(args, error, false);
				return;
			}
			List<Vector2> path = new List<Vector2>();
			for (int i = 3; i < args.Length; i += 2)
			{
				Vector2 tile;
				if (ArgUtility.TryGetVector2(args, i, out tile, out error, true, "Vector2 tile"))
				{
					path.Add(tile);
				}
				else
				{
					this.LogCommandError(args, error, false);
				}
			}
			if (this.npcControllers == null)
			{
				this.npcControllers = new List<NPCController>();
			}
			int farmerNumber;
			if (this.IsFarmerActorId(actorName, out farmerNumber))
			{
				Farmer f = this.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					this.npcControllers.Add(new NPCController(f, path, loop, endBehavior));
					return;
				}
			}
			else
			{
				NPC j = this.getActorByName(actorName, true);
				if (j != null)
				{
					this.npcControllers.Add(new NPCController(j, path, loop, endBehavior));
				}
			}
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00048F80 File Offset: 0x00047180
		public static bool IsItemMayorShorts(Item i)
		{
			return ((i != null) ? i.QualifiedItemId : null) == "(O)789" || ((i != null) ? i.QualifiedItemId : null) == "(O)71";
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00048FB4 File Offset: 0x000471B4
		public void addItemToLuauSoup(Item i, Farmer who)
		{
			if (i == null)
			{
				return;
			}
			who.team.luauIngredients.Add(i.getOne());
			if (who.IsLocalPlayer)
			{
				this.specialEventVariable2 = true;
				bool is_shorts = Event.IsItemMayorShorts(i);
				if (i != null && i.Stack > 1 && !is_shorts)
				{
					int stack = i.Stack;
					i.Stack = stack - 1;
					who.addItemToInventory(i);
				}
				else if (is_shorts)
				{
					who.addItemToInventory(i);
				}
				Game1.exitActiveMenu();
				Game1.playSound("dropItemInWater", null);
				if (i != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1857", i.DisplayName));
				}
				string qualityString = "";
				switch (i.Quality)
				{
				case 1:
					qualityString = " ([51])";
					break;
				case 2:
					qualityString = " ([52])";
					break;
				case 4:
					qualityString = " ([53])";
					break;
				}
				if (!is_shorts)
				{
					Game1.multiplayer.globalChatInfoMessage("LuauSoup", new string[]
					{
						Game1.player.Name,
						TokenStringBuilder.ItemNameFor(i, null) + qualityString
					});
				}
			}
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x000490D0 File Offset: 0x000472D0
		private void governorTaste()
		{
			int likeLevel = 5;
			foreach (Item item in Game1.player.team.luauIngredients)
			{
				Object o = item as Object;
				int itemLevel = 5;
				if (Event.IsItemMayorShorts(o))
				{
					likeLevel = 6;
					break;
				}
				if ((o.Quality >= 2 && o.price.Value >= 160) || (o.Quality == 1 && o.price.Value >= 300 && o.edibility.Value > 10))
				{
					itemLevel = 4;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 120, "Town");
				}
				else if (o.edibility.Value >= 20 || o.price.Value >= 100 || (o.price.Value >= 70 && o.Quality >= 1))
				{
					itemLevel = 3;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 60, "Town");
				}
				else if ((o.price.Value > 20 && o.edibility.Value >= 10) || (o.price.Value >= 40 && o.edibility.Value >= 5))
				{
					itemLevel = 2;
				}
				else if (o.edibility.Value >= 0)
				{
					itemLevel = 1;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -50, "Town");
				}
				if (o.edibility.Value > -300 && o.edibility.Value < 0)
				{
					itemLevel = 0;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -100, "Town");
				}
				if (itemLevel < likeLevel)
				{
					likeLevel = itemLevel;
				}
			}
			int numPlayers = Game1.numberOfPlayers() - ((Game1.HasDedicatedHost > false) ? 1 : 0);
			if (likeLevel != 6 && Game1.player.team.luauIngredients.Count < numPlayers)
			{
				likeLevel = 5;
			}
			this.eventCommands[this.CurrentCommand + 1] = "switchEvent governorReaction" + likeLevel.ToString();
			if (likeLevel == 4)
			{
				Game1.getAchievement(38, true);
			}
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x000492F0 File Offset: 0x000474F0
		private void eggHuntWinner()
		{
			int numberOfEggsToWin;
			switch (Game1.numberOfPlayers() - ((Game1.HasDedicatedHost > false) ? 1 : 0))
			{
			case 1:
				numberOfEggsToWin = 9;
				break;
			case 2:
				numberOfEggsToWin = 6;
				break;
			case 3:
				numberOfEggsToWin = 5;
				break;
			default:
				numberOfEggsToWin = 4;
				break;
			}
			List<Farmer> winners = new List<Farmer>();
			int mostEggsScore = Game1.player.festivalScore;
			foreach (Farmer temp in Game1.getOnlineFarmers())
			{
				if (temp.festivalScore > mostEggsScore)
				{
					mostEggsScore = temp.festivalScore;
				}
			}
			foreach (Farmer temp2 in Game1.getOnlineFarmers())
			{
				if (temp2.festivalScore == mostEggsScore)
				{
					winners.Add(temp2);
					this.festivalWinners.Add(temp2.UniqueMultiplayerID);
				}
			}
			string winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1862");
			if (mostEggsScore >= numberOfEggsToWin)
			{
				foreach (Farmer farmer in winners)
				{
					farmer.autoGenerateActiveDialogueEvent("wonEggHunt", 4);
				}
				if (winners.Count == 1)
				{
					winnerDialogue = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? ("¡" + winners[0].displayName + "!") : (winners[0].displayName + "!"));
				}
				else
				{
					winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
					for (int i = 0; i < winners.Count; i++)
					{
						if (i == winners.Count - 1)
						{
							winnerDialogue += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
						}
						winnerDialogue = winnerDialogue + " " + winners[i].displayName;
						if (i < winners.Count - 1)
						{
							winnerDialogue += ",";
						}
					}
					winnerDialogue += "!";
				}
				this.specialEventVariable1 = false;
			}
			else
			{
				this.specialEventVariable1 = true;
			}
			NPC lewis = this.getActorByName("Lewis", false);
			lewis.CurrentDialogue.Push(new Dialogue(lewis, null, winnerDialogue));
			Game1.drawDialogue(lewis);
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00049558 File Offset: 0x00047758
		private void iceFishingWinner()
		{
			int numberOfFishToWin = 5;
			this.iceFishWinners = new List<Farmer>();
			int mostFishScore = Game1.player.festivalScore;
			for (int i = 1; i <= Game1.numberOfPlayers(); i++)
			{
				Farmer temp = this.GetFarmerActor(i);
				if (temp != null && temp.festivalScore > mostFishScore)
				{
					mostFishScore = temp.festivalScore;
				}
			}
			for (int j = 1; j <= Game1.numberOfPlayers(); j++)
			{
				Farmer temp2 = this.GetFarmerActor(j);
				if (temp2 != null && temp2.festivalScore == mostFishScore)
				{
					this.iceFishWinners.Add(temp2);
					this.festivalWinners.Add(temp2.UniqueMultiplayerID);
				}
			}
			string winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1871");
			if (mostFishScore >= numberOfFishToWin)
			{
				foreach (Farmer farmer in this.iceFishWinners)
				{
					farmer.autoGenerateActiveDialogueEvent("wonIceFishing", 4);
				}
				if (this.iceFishWinners.Count == 1)
				{
					winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1872", this.iceFishWinners[0].displayName, this.iceFishWinners[0].festivalScore);
				}
				else
				{
					winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
					for (int k = 0; k < this.iceFishWinners.Count; k++)
					{
						if (k == this.iceFishWinners.Count - 1)
						{
							winnerDialogue += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
						}
						winnerDialogue = winnerDialogue + " " + this.iceFishWinners[k].displayName;
						if (k < this.iceFishWinners.Count - 1)
						{
							winnerDialogue += ",";
						}
					}
					winnerDialogue += "!";
				}
				this.specialEventVariable1 = false;
			}
			else
			{
				this.specialEventVariable1 = true;
			}
			NPC lewis = this.getActorByName("Lewis", false);
			lewis.CurrentDialogue.Push(new Dialogue(lewis, null, winnerDialogue));
			Game1.drawDialogue(lewis);
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00049778 File Offset: 0x00047978
		private void iceFishingWinnerMP()
		{
			this.specialEventVariable1 = !this.iceFishWinners.Contains(Game1.player);
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00049794 File Offset: 0x00047994
		public void popBalloons(int x, int y)
		{
			if ((this.id.Equals("191393") || this.id.Equals("502261")) && this.aboveMapSprites != null)
			{
				List<int> idsToRemove = new List<int>();
				for (int i = this.aboveMapSprites.Count - 1; i >= 0; i--)
				{
					TemporaryAnimatedSprite t = this.aboveMapSprites[i];
					int width = t.sourceRect.Width * 4;
					int height = t.sourceRect.Height * 4;
					Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)t.Position.X, (int)t.Position.Y, width, height);
					if (r.Contains(x, y))
					{
						idsToRemove.Add(t.id);
						if (t.sourceRect.Height <= 16)
						{
							for (int z = 0; z < 3; z++)
							{
								this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(280 + Game1.random.Choose(8, 0), 1954, 8, 8), 1000f, 1, 99, Utility.getRandomPositionInThisRectangle(r, Game1.random), false, false, 1f, 0f, t.color, 4f, 0f, 0f, (float)Game1.random.Next(-10, 11) / 100f, false)
								{
									motion = new Vector2((float)Game1.random.Next(-4, 5), -8f + (float)Game1.random.Next(-10, 1) / 100f),
									acceleration = new Vector2(0f, 0.3f),
									local = true
								});
							}
						}
					}
				}
				this.aboveMapSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.id == 9988 || idsToRemove.Contains(sprite.id));
				if (idsToRemove.Count > 0)
				{
					this.int_useMeForAnything++;
					this.aboveMapSprites.Add(new TemporaryAnimatedSprite(null, Microsoft.Xna.Framework.Rectangle.Empty, new Vector2(16f, 16f), false, 0f, Color.White)
					{
						text = (this.int_useMeForAnything.ToString() ?? ""),
						layerDepth = 1f,
						animationLength = 1,
						totalNumberOfLoops = 10,
						interval = 300f,
						scale = 2f,
						local = true,
						id = 9988
					});
					Game1.playSound("coin", null);
				}
			}
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00049A3C File Offset: 0x00047C3C
		public virtual string GenerateLightSourceId(string suffix)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted("Event");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(this.id);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(suffix);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040003A0 RID: 928
		protected static readonly Dictionary<string, EventCommandDelegate> Commands = new Dictionary<string, EventCommandDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x040003A1 RID: 929
		protected static readonly Dictionary<string, string> CommandAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x040003A2 RID: 930
		protected static readonly HashSet<string> CommandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x040003A3 RID: 931
		protected static readonly Dictionary<string, EventPreconditionDelegate> Preconditions = new Dictionary<string, EventPreconditionDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x040003A4 RID: 932
		private static readonly Dictionary<string, string> PreconditionAliases = new Dictionary<string, string>();

		// Token: 0x040003A5 RID: 933
		private const float timeBetweenSpeech = 500f;

		// Token: 0x040003A6 RID: 934
		public const string festivalTextureName = "Maps\\Festivals";

		// Token: 0x040003A7 RID: 935
		private string festivalDataAssetName;

		// Token: 0x040003A8 RID: 936
		public string id = "-1";

		// Token: 0x040003A9 RID: 937
		public string fromAssetName;

		// Token: 0x040003AA RID: 938
		public bool isFestival;

		// Token: 0x040003AB RID: 939
		public bool isWedding;

		// Token: 0x040003AC RID: 940
		public bool isMemory;

		// Token: 0x040003AD RID: 941
		public bool skippable;

		// Token: 0x040003AE RID: 942
		public string[] actionsOnSkip;

		// Token: 0x040003AF RID: 943
		public bool skipped;

		// Token: 0x040003B0 RID: 944
		public bool forked;

		// Token: 0x040003B1 RID: 945
		public bool eventSwitched;

		// Token: 0x040003B2 RID: 946
		internal bool notifyWhenDone;

		// Token: 0x040003B3 RID: 947
		internal string notifyLocationName;

		// Token: 0x040003B4 RID: 948
		internal byte notifyLocationIsStructure;

		// Token: 0x040003B5 RID: 949
		private readonly LocalizedContentManager festivalContent = Game1.content.CreateTemporary();

		// Token: 0x040003B6 RID: 950
		public string[] eventCommands;

		// Token: 0x040003B7 RID: 951
		public int currentCommand;

		// Token: 0x040003B8 RID: 952
		private Dictionary<string, Vector3> actorPositionsAfterMove;

		// Token: 0x040003B9 RID: 953
		private float timeAccumulator;

		// Token: 0x040003BA RID: 954
		private Vector3 viewportTarget;

		// Token: 0x040003BB RID: 955
		private Color previousAmbientLight;

		// Token: 0x040003BC RID: 956
		private HashSet<long> festivalWinners = new HashSet<long>();

		// Token: 0x040003BD RID: 957
		private GameLocation temporaryLocation;

		// Token: 0x040003BE RID: 958
		private Dictionary<string, string> festivalData;

		// Token: 0x040003BF RID: 959
		private Texture2D _festivalTexture;

		// Token: 0x040003C0 RID: 960
		private bool drawTool;

		// Token: 0x040003C1 RID: 961
		private string hostMessageKey;

		// Token: 0x040003C2 RID: 962
		private int previousFacingDirection = -1;

		// Token: 0x040003C3 RID: 963
		private int previousAnswerChoice = -1;

		// Token: 0x040003C4 RID: 964
		private bool startSecretSantaAfterDialogue;

		// Token: 0x040003C5 RID: 965
		private List<Farmer> iceFishWinners;

		// Token: 0x040003C6 RID: 966
		protected static LocalizedContentManager FestivalReadContentLoader;

		// Token: 0x040003C7 RID: 967
		protected bool _playerControlSequence;

		// Token: 0x040003C8 RID: 968
		protected bool _repeatingLocationSpecificCommand;

		// Token: 0x040003C9 RID: 969
		[NonInstancedStatic]
		public static HashSet<string> invalidFestivals = new HashSet<string>();

		// Token: 0x040003CA RID: 970
		public List<NPC> actors = new List<NPC>();

		// Token: 0x040003CB RID: 971
		public List<Object> props = new List<Object>();

		// Token: 0x040003CC RID: 972
		public List<Prop> festivalProps = new List<Prop>();

		// Token: 0x040003CD RID: 973
		public List<Farmer> farmerActors = new List<Farmer>();

		// Token: 0x040003CE RID: 974
		public Dictionary<string, Dictionary<ISalable, ItemStockInformation>> festivalShops;

		// Token: 0x040003CF RID: 975
		public List<NPCController> npcControllers;

		// Token: 0x040003D0 RID: 976
		internal NPC festivalHost;

		// Token: 0x040003D1 RID: 977
		public NPC secretSantaRecipient;

		// Token: 0x040003D2 RID: 978
		public NPC mySecretSanta;

		// Token: 0x040003D3 RID: 979
		public TemporaryAnimatedSpriteList underwaterSprites;

		// Token: 0x040003D4 RID: 980
		public TemporaryAnimatedSpriteList aboveMapSprites;

		// Token: 0x040003D5 RID: 981
		public IDictionary<string, List<ICue>> CustomSounds = new Dictionary<string, List<ICue>>();

		// Token: 0x040003D6 RID: 982
		public ICustomEventScript currentCustomEventScript;

		// Token: 0x040003D7 RID: 983
		public bool simultaneousCommand;

		// Token: 0x040003D8 RID: 984
		public int farmerAddedSpeed;

		// Token: 0x040003D9 RID: 985
		public int int_useMeForAnything;

		// Token: 0x040003DA RID: 986
		public int int_useMeForAnything2;

		// Token: 0x040003DB RID: 987
		public float float_useMeForAnything;

		// Token: 0x040003DC RID: 988
		public string playerControlSequenceID;

		// Token: 0x040003DD RID: 989
		public string spriteTextToDraw;

		// Token: 0x040003DE RID: 990
		public bool showActiveObject;

		// Token: 0x040003DF RID: 991
		public bool continueAfterMove;

		// Token: 0x040003E0 RID: 992
		public bool specialEventVariable1;

		// Token: 0x040003E1 RID: 993
		public bool specialEventVariable2;

		// Token: 0x040003E2 RID: 994
		public bool showGroundObjects = true;

		// Token: 0x040003E3 RID: 995
		public bool doingSecretSanta;

		// Token: 0x040003E4 RID: 996
		public bool showWorldCharacters;

		// Token: 0x040003E5 RID: 997
		public bool ignoreObjectCollisions = true;

		// Token: 0x040003E6 RID: 998
		public Point playerControlTargetTile;

		// Token: 0x040003E7 RID: 999
		public List<Vector2> characterWalkLocations = new List<Vector2>();

		// Token: 0x040003E8 RID: 1000
		public Vector2 eventPositionTileOffset = Vector2.Zero;

		// Token: 0x040003E9 RID: 1001
		public int festivalTimer;

		// Token: 0x040003EA RID: 1002
		public int grangeScore = -1000;

		// Token: 0x040003EB RID: 1003
		public bool grangeJudged;

		// Token: 0x040003EC RID: 1004
		public bool ignoreTileOffsets;

		// Token: 0x040003ED RID: 1005
		private Stopwatch stopWatch;

		// Token: 0x040003EE RID: 1006
		public LocationRequest exitLocation;

		// Token: 0x040003EF RID: 1007
		public Action onEventFinished;

		// Token: 0x040003F0 RID: 1008
		public bool markEventSeen = true;

		// Token: 0x040003F1 RID: 1009
		private bool eventFinished;

		// Token: 0x040003F2 RID: 1010
		private bool gotPet;

		// Token: 0x0200040F RID: 1039
		public static class DefaultCommands
		{
			// Token: 0x06003BEF RID: 15343 RVA: 0x002E48D8 File Offset: 0x002E2AD8
			public static void IgnoreEventTileOffset(Event @event, string[] args, EventContext context)
			{
				@event.ignoreTileOffsets = true;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BF0 RID: 15344 RVA: 0x002E48FC File Offset: 0x002E2AFC
			public static void Move(Event @event, string[] args, EventContext context)
			{
				bool? continueAfterMove = null;
				int fieldsAfterMoves = (args.Length - 1) % 4;
				if (fieldsAfterMoves == 1)
				{
					bool rawValue;
					string error;
					if (!ArgUtility.TryGetOptionalBool(args, args.Length - 1, out rawValue, out error, false, "bool rawValue"))
					{
						context.LogErrorAndSkip(error, false);
						return;
					}
					continueAfterMove = new bool?(rawValue);
				}
				else if (fieldsAfterMoves > 1)
				{
					context.LogErrorAndSkip("invalid number of arguments, expected sets of [actor x y direction] fields plus an optional continue-after-move boolean field", false);
					return;
				}
				if (continueAfterMove == null || args.Length > 2)
				{
					int i = 1;
					while (i < args.Length && ArgUtility.HasIndex<string>(args, i + 3))
					{
						string actorName;
						string error2;
						Point tile;
						int facingDirection;
						int farmerNumber;
						if (!ArgUtility.TryGet(args, i, out actorName, out error2, true, "string actorName") || !ArgUtility.TryGetPoint(args, i + 1, out tile, out error2, "Point tile") || !ArgUtility.TryGetDirection(args, i + 3, out facingDirection, out error2, "int facingDirection"))
						{
							context.LogError(error2, false);
						}
						else if (@event.IsFarmerActorId(actorName, out farmerNumber))
						{
							if (!@event.actorPositionsAfterMove.ContainsKey(actorName))
							{
								Farmer farmer = @event.GetFarmerActor(farmerNumber);
								if (farmer != null)
								{
									farmer.canOnlyWalk = false;
									farmer.setRunning(false, true);
									farmer.canOnlyWalk = true;
									farmer.convertEventMotionCommandToMovement(Utility.PointToVector2(tile));
									@event.actorPositionsAfterMove.Add(actorName, @event.getPositionAfterMove(farmer, tile.X, tile.Y, facingDirection));
								}
							}
						}
						else
						{
							bool isOptionalNpc;
							NPC j = @event.getActorByName(actorName, out isOptionalNpc, false);
							if (j == null)
							{
								if (!isOptionalNpc)
								{
									context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", false);
									return;
								}
							}
							else if (!@event.actorPositionsAfterMove.ContainsKey(j.Name))
							{
								j.convertEventMotionCommandToMovement(Utility.PointToVector2(tile));
								@event.actorPositionsAfterMove.Add(j.Name, @event.getPositionAfterMove(j, tile.X, tile.Y, facingDirection));
							}
						}
						i += 4;
					}
				}
				if (continueAfterMove != null)
				{
					if (continueAfterMove.GetValueOrDefault())
					{
						@event.continueAfterMove = true;
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
						return;
					}
					@event.continueAfterMove = false;
					if (args.Length == 2 && @event.actorPositionsAfterMove.Count == 0)
					{
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
					}
				}
			}

			// Token: 0x06003BF1 RID: 15345 RVA: 0x002E4B2C File Offset: 0x002E2D2C
			public static void Action(Event @event, string[] args, EventContext context)
			{
				string action;
				string error;
				if (!ArgUtility.TryGetRemainder(args, 1, out action, out error, ' ', "string action"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Exception ex;
				if (!TriggerActionManager.TryRunAction(action, out error, out ex))
				{
					if (ex != null)
					{
						string str = error;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
						defaultInterpolatedStringHandler.AppendLiteral("\n");
						defaultInterpolatedStringHandler.AppendFormatted<Exception>(ex);
						error = str + defaultInterpolatedStringHandler.ToStringAndClear();
					}
					context.LogErrorAndSkip(error, false);
					return;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BF2 RID: 15346 RVA: 0x002E4BAC File Offset: 0x002E2DAC
			public static void Speak(Event @event, string[] args, EventContext context)
			{
				if (@event.skipped)
				{
					return;
				}
				string actorName;
				string error;
				string textOrTranslationKey;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out textOrTranslationKey, out error, true, "string textOrTranslationKey"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!Game1.dialogueUp)
				{
					@event.timeAccumulator += (float)context.Time.ElapsedGameTime.Milliseconds;
					if (@event.timeAccumulator < 500f)
					{
						return;
					}
					@event.timeAccumulator = 0f;
					bool isOptionalNpc;
					NPC n = @event.getActorByName(actorName, out isOptionalNpc, false) ?? Game1.getCharacterFromName(actorName.TrimEnd('?'), true, false);
					if (n == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						if (!isOptionalNpc)
						{
							Game1.eventFinished();
						}
						return;
					}
					Game1.player.NotifyQuests((Quest quest) => quest.OnNpcSocialized(n, false), false);
					if (n.CanSocialize && !Game1.player.friendshipData.ContainsKey(n.Name))
					{
						Game1.player.friendshipData.Add(n.Name, new Friendship(0));
					}
					Dialogue dialogue = Game1.content.IsValidTranslationKey(textOrTranslationKey) ? new Dialogue(n, textOrTranslationKey, false) : new Dialogue(n, null, textOrTranslationKey);
					n.CurrentDialogue.Push(dialogue);
					Game1.drawDialogue(n);
				}
			}

			// Token: 0x06003BF3 RID: 15347 RVA: 0x002E4D34 File Offset: 0x002E2F34
			public static void BeginSimultaneousCommand(Event @event, string[] args, EventContext context)
			{
				@event.simultaneousCommand = true;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BF4 RID: 15348 RVA: 0x002E4D58 File Offset: 0x002E2F58
			public static void EndSimultaneousCommand(Event @event, string[] args, EventContext context)
			{
				@event.simultaneousCommand = false;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BF5 RID: 15349 RVA: 0x002E4D7C File Offset: 0x002E2F7C
			public static void MineDeath(Event @event, string[] args, EventContext context)
			{
				if (!Game1.dialogueUp)
				{
					Random r = Utility.CreateDaySaveRandom((double)Game1.timeOfDay, 0.0, 0.0);
					int moneyToLose = r.Next(Game1.player.Money / 40, Game1.player.Money / 8);
					moneyToLose = Math.Min(moneyToLose, 15000);
					moneyToLose -= (int)((double)Game1.player.LuckLevel * 0.01 * (double)moneyToLose);
					moneyToLose -= moneyToLose % 100;
					int numberOfItemsLost = Game1.player.LoseItemsOnDeath(r);
					Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
					Game1.player.Money = Math.Max(0, Game1.player.Money - moneyToLose);
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1057") + " " + ((moneyToLose <= 0) ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1058", moneyToLose)) + ((numberOfItemsLost > 0) ? ((moneyToLose <= 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1060") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1063") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost)))) : ((moneyToLose <= 0) ? "" : ".")));
					@event.InsertNextCommand("showItemsLost");
				}
			}

			// Token: 0x06003BF6 RID: 15350 RVA: 0x002E4F30 File Offset: 0x002E3130
			public static void HospitalDeath(Event @event, string[] args, EventContext context)
			{
				if (!Game1.dialogueUp)
				{
					int numberOfItemsLost = Game1.player.LoseItemsOnDeath(null);
					Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
					int moneyToLose = Math.Min(1000, Game1.player.Money);
					Game1.player.Money -= moneyToLose;
					Game1.drawObjectDialogue(((moneyToLose > 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1068", moneyToLose) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1070")) + ((numberOfItemsLost > 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1071") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : ""));
					@event.InsertNextCommand("showItemsLost");
				}
			}

			// Token: 0x06003BF7 RID: 15351 RVA: 0x002E5020 File Offset: 0x002E3220
			public static void ShowItemsLost(Event @event, string[] args, EventContext context)
			{
				if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new ItemListMenu(Game1.content.LoadString("Strings\\UI:ItemList_ItemsLost"), Game1.player.itemsLostLastDeath.ToList<Item>());
				}
			}

			// Token: 0x06003BF8 RID: 15352 RVA: 0x002E5051 File Offset: 0x002E3251
			public static void End(Event @event, string[] args, EventContext context)
			{
				@event.endBehaviors(args, context.Location);
			}

			// Token: 0x06003BF9 RID: 15353 RVA: 0x002E5060 File Offset: 0x002E3260
			public static void LocationSpecificCommand(Event @event, string[] args, EventContext context)
			{
				string command;
				string error;
				if (!ArgUtility.TryGet(args, 1, out command, out error, true, "string command"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				string[] commandArgs = args.Skip(2).ToArray<string>();
				if (context.Location.RunLocationSpecificEventCommand(@event, command, !@event._repeatingLocationSpecificCommand, commandArgs))
				{
					@event._repeatingLocationSpecificCommand = false;
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					return;
				}
				@event._repeatingLocationSpecificCommand = true;
			}

			// Token: 0x06003BFA RID: 15354 RVA: 0x002E50D0 File Offset: 0x002E32D0
			public static void Unskippable(Event @event, string[] args, EventContext context)
			{
				@event.skippable = false;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BFB RID: 15355 RVA: 0x002E50F4 File Offset: 0x002E32F4
			public static void Skippable(Event @event, string[] args, EventContext context)
			{
				@event.skippable = true;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BFC RID: 15356 RVA: 0x002E5118 File Offset: 0x002E3318
			public static void SetSkipActions(Event @event, string[] args, EventContext context)
			{
				string skipActions;
				string error;
				if (!ArgUtility.TryGetRemainder(args, 1, out skipActions, out error, ' ', "string skipActions"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int i;
				if (string.IsNullOrWhiteSpace(skipActions))
				{
					@event.actionsOnSkip = null;
				}
				else
				{
					string[] actions = LegacyShims.SplitAndTrim(skipActions, '#', StringSplitOptions.None);
					string[] array = actions;
					for (i = 0; i < array.Length; i++)
					{
						if (!TriggerActionManager.TryValidateActionExists(array[i], out error))
						{
							context.LogErrorAndSkip(error, false);
							return;
						}
					}
					@event.actionsOnSkip = actions;
				}
				i = @event.CurrentCommand;
				@event.CurrentCommand = i + 1;
			}

			// Token: 0x06003BFD RID: 15357 RVA: 0x002E51A0 File Offset: 0x002E33A0
			public static void Emote(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int emoteId;
				bool nextCommandImmediate;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out emoteId, out error, "int emoteId") || !ArgUtility.TryGetOptionalBool(args, 3, out nextCommandImmediate, out error, false, "bool nextCommandImmediate"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer farmerActor = @event.GetFarmerActor(farmerNumber);
					if (farmerActor != null)
					{
						farmerActor.doEmote(emoteId, !nextCommandImmediate);
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					if (!i.isEmoting)
					{
						i.doEmote(emoteId, !nextCommandImmediate);
					}
				}
				if (nextCommandImmediate)
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					@event.Update(context.Location, context.Time);
				}
			}

			// Token: 0x06003BFE RID: 15358 RVA: 0x002E5278 File Offset: 0x002E3478
			public static void StopMusic(Event @event, string[] args, EventContext context)
			{
				Game1.changeMusicTrack("none", false, MusicContext.Event);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003BFF RID: 15359 RVA: 0x002E52A4 File Offset: 0x002E34A4
			public static void PlayPetSound(Event @event, string[] args, EventContext context)
			{
				string sound;
				string error;
				if (!ArgUtility.TryGet(args, 1, out sound, out error, true, "string sound"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Pet pet = null;
				foreach (NPC actor in @event.actors)
				{
					if (actor is Pet)
					{
						pet = (actor as Pet);
						break;
					}
				}
				if (pet == null)
				{
					pet = Game1.player.getPet();
				}
				float pitch = 1200f;
				if (pet != null)
				{
					PetData pet_data = pet.GetPetData();
					PetBreed breed = (pet_data != null) ? pet_data.GetBreedById(pet.whichBreed.Value, false) : null;
					if (breed != null)
					{
						pitch *= breed.VoicePitch;
						if (sound == pet_data.BarkSound && breed.BarkOverride != null)
						{
							sound = breed.BarkOverride;
						}
					}
				}
				Game1.playSound(sound, new int?((int)pitch));
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C00 RID: 15360 RVA: 0x002E53C0 File Offset: 0x002E35C0
			public static void PlaySound(Event @event, string[] args, EventContext context)
			{
				string soundId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out soundId, out error, true, "string soundId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				ICue sound;
				Game1.playSound(soundId, out sound);
				@event.TrackSound(sound);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C01 RID: 15361 RVA: 0x002E541C File Offset: 0x002E361C
			public static void StopSound(Event @event, string[] args, EventContext context)
			{
				string soundId;
				string error;
				bool immediate;
				if (!ArgUtility.TryGet(args, 1, out soundId, out error, true, "string soundId") || !ArgUtility.TryGetOptionalBool(args, 2, out immediate, out error, true, "bool immediate"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.StopTrackedSound(soundId, immediate);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C02 RID: 15362 RVA: 0x002E5484 File Offset: 0x002E3684
			public static void TossConcession(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string concessionId;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out concessionId, out error, true, "string concessionId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				MovieConcession concession = MovieTheater.GetConcessionItem(concessionId);
				if (concession == null)
				{
					context.LogErrorAndSkip("no concession found with ID '" + concessionId + "'", false);
					return;
				}
				Texture2D texture = concession.GetTexture();
				int spriteIndex = concession.GetSpriteIndex();
				Game1.playSound("dwop", null);
				context.Location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = texture,
					sourceRect = Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16),
					animationLength = 1,
					totalNumberOfLoops = 1,
					motion = new Vector2(0f, -6f),
					acceleration = new Vector2(0f, 0.2f),
					interval = 1000f,
					scale = 4f,
					position = @event.OffsetPosition(new Vector2(actor.Position.X, actor.Position.Y - 96f)),
					layerDepth = (float)actor.StandingPixel.Y / 10000f
				});
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C03 RID: 15363 RVA: 0x002E561C File Offset: 0x002E381C
			public static void Pause(Event @event, string[] args, EventContext context)
			{
				int pauseTime;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out pauseTime, out error, "int pauseTime"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (Game1.pauseTime <= 0f)
				{
					Game1.pauseTime = (float)pauseTime;
				}
			}

			// Token: 0x06003C04 RID: 15364 RVA: 0x002E5658 File Offset: 0x002E3858
			public static void PrecisePause(Event @event, string[] args, EventContext context)
			{
				int pauseTime;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out pauseTime, out error, "int pauseTime"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (@event.stopWatch == null)
				{
					@event.stopWatch = new Stopwatch();
				}
				if (!@event.stopWatch.IsRunning)
				{
					@event.stopWatch.Start();
				}
				if (@event.stopWatch.ElapsedMilliseconds >= (long)pauseTime)
				{
					@event.stopWatch.Stop();
					@event.stopWatch.Reset();
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
			}

			// Token: 0x06003C05 RID: 15365 RVA: 0x002E56E0 File Offset: 0x002E38E0
			public static void ResetVariable(Event @event, string[] args, EventContext context)
			{
				@event.specialEventVariable1 = false;
				@event.currentCommand++;
			}

			// Token: 0x06003C06 RID: 15366 RVA: 0x002E56F8 File Offset: 0x002E38F8
			public static void FaceDirection(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int faceDirection;
				bool continueImmediate;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetDirection(args, 2, out faceDirection, out error, "int faceDirection") || !ArgUtility.TryGetOptionalBool(args, 3, out continueImmediate, out error, false, "bool continueImmediate"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.FarmerSprite.StopAnimation();
						f.completelyStopAnimatingOrDoingAction();
						f.faceDirection(faceDirection);
					}
				}
				else if (actorName.Contains("spouse"))
				{
					NPC spouse = @event.getActorByName(Game1.player.spouse, false);
					if (spouse != null && !Game1.player.hasRoommate())
					{
						spouse.faceDirection(faceDirection);
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					i.faceDirection(faceDirection);
				}
				if (continueImmediate)
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					@event.Update(context.Location, context.Time);
					return;
				}
				if (Game1.pauseTime <= 0f)
				{
					Game1.pauseTime = 500f;
				}
			}

			// Token: 0x06003C07 RID: 15367 RVA: 0x002E5828 File Offset: 0x002E3A28
			public static void Warp(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				Vector2 tile;
				bool continueImmediate;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetVector2(args, 2, out tile, out error, true, "Vector2 tile") || !ArgUtility.TryGetOptionalBool(args, 4, out continueImmediate, out error, false, "bool continueImmediate"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.setTileLocation(@event.OffsetTile(tile));
						f.position.Y -= 16f;
						if (@event.farmerActors.Contains(f))
						{
							f.completelyStopAnimatingOrDoingAction();
						}
					}
				}
				else if (actorName.Contains("spouse"))
				{
					NPC spouse = @event.getActorByName(Game1.player.spouse, false);
					if (spouse != null && !Game1.player.hasRoommate())
					{
						List<NPCController> npcControllers = @event.npcControllers;
						if (npcControllers != null)
						{
							npcControllers.RemoveAll((NPCController npcController) => npcController.puppet.Name == Game1.player.spouse);
						}
						spouse.Position = @event.OffsetPosition(tile * 64f);
						spouse.IsWalkingInSquare = false;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					i.position.X = @event.OffsetPositionX(tile.X * 64f + 4f);
					i.position.Y = @event.OffsetPositionY(tile.Y * 64f);
					i.IsWalkingInSquare = false;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				if (continueImmediate)
				{
					@event.Update(context.Location, context.Time);
				}
			}

			// Token: 0x06003C08 RID: 15368 RVA: 0x002E59FC File Offset: 0x002E3BFC
			public static void WarpFarmers(Event @event, string[] args, EventContext context)
			{
				int nonWarpFields = (args.Length - 1) % 3;
				if (args.Length < 5 || nonWarpFields != 1)
				{
					context.LogErrorAndSkip("invalid number of arguments; expected zero or more [x y direction] triplets, one offset direction (up/down/left/right), and one triplet which applies to any other farmer", false);
					return;
				}
				int defaultsIndex = args.Length - 4;
				int offsetDirection;
				string error;
				Point defaultPosition;
				int defaultFacingDirection;
				if (!ArgUtility.TryGetDirection(args, defaultsIndex, out offsetDirection, out error, "int offsetDirection") || !ArgUtility.TryGetPoint(args, defaultsIndex + 1, out defaultPosition, out error, "Point defaultPosition") || !ArgUtility.TryGetDirection(args, defaultsIndex + 3, out defaultFacingDirection, out error, "int defaultFacingDirection"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				List<Vector3> positions = new List<Vector3>();
				for (int i = 1; i < defaultsIndex; i += 3)
				{
					Point position;
					int facingDirection;
					if (!ArgUtility.TryGetPoint(args, i, out position, out error, "Point position") || !ArgUtility.TryGetDirection(args, i + 2, out facingDirection, out error, "int facingDirection"))
					{
						context.LogErrorAndSkip(error, false);
						return;
					}
					positions.Add(new Vector3((float)position.X, (float)position.Y, (float)facingDirection));
				}
				Point offset;
				switch (offsetDirection)
				{
				case 0:
					offset = new Point(0, -1);
					break;
				case 1:
					offset = new Point(1, 0);
					break;
				case 2:
					offset = new Point(0, 1);
					break;
				case 3:
					offset = new Point(-1, 0);
					break;
				default:
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 1);
					defaultInterpolatedStringHandler.AppendLiteral("invalid offset direction '");
					defaultInterpolatedStringHandler.AppendFormatted<int>(offsetDirection);
					defaultInterpolatedStringHandler.AppendLiteral("'; must be one of 'left', 'right', 'up', or 'down'");
					context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear(), false);
					return;
				}
				}
				int currentX = defaultPosition.X;
				int currentY = defaultPosition.Y;
				for (int j = 0; j < Game1.numberOfPlayers(); j++)
				{
					Farmer farmer = @event.GetFarmerActor(j + 1);
					float x;
					float y;
					int direction;
					if (j < positions.Count)
					{
						x = positions[j].X;
						y = positions[j].Y;
						direction = (int)positions[j].Z;
					}
					else
					{
						x = (float)currentX;
						y = (float)currentY;
						direction = defaultFacingDirection;
						currentX += offset.X;
						currentY += offset.Y;
						Layer layer = context.Location.map.GetLayer("Buildings");
						if (((layer != null) ? layer.Tiles[currentX, currentY] : null) != null && offset != Point.Zero)
						{
							currentX -= offset.X;
							currentY -= offset.Y;
							offset = Point.Zero;
						}
					}
					if (farmer != null)
					{
						farmer.setTileLocation(@event.OffsetTile(new Vector2(x, y)));
						farmer.faceDirection(direction);
						farmer.position.Y -= 16f;
						farmer.completelyStopAnimatingOrDoingAction();
					}
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C09 RID: 15369 RVA: 0x002E5CA0 File Offset: 0x002E3EA0
			public static void Speed(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int speed;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out speed, out error, "int speed"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					if (@event.IsCurrentFarmerActorId(farmerNumber))
					{
						@event.farmerAddedSpeed = speed;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (actor == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					actor.speed = speed;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C0A RID: 15370 RVA: 0x002E5D3C File Offset: 0x002E3F3C
			public static void StopAdvancedMoves(Event @event, string[] args, EventContext context)
			{
				string option = ArgUtility.Get(args, 1, null, true);
				if (option != null)
				{
					if (option == "next")
					{
						using (List<NPCController>.Enumerator enumerator = @event.npcControllers.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								NPCController npccontroller = enumerator.Current;
								npccontroller.destroyAtNextCrossroad();
							}
							goto IL_74;
						}
					}
					context.LogErrorAndSkip("unknown option " + option + ", must be 'next' or omitted", false);
					return;
				}
				@event.npcControllers.Clear();
				IL_74:
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C0B RID: 15371 RVA: 0x002E5DE0 File Offset: 0x002E3FE0
			public static void DoAction(Event @event, string[] args, EventContext context)
			{
				Point tile;
				string error;
				if (!ArgUtility.TryGetPoint(args, 1, out tile, out error, "Point tile"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Location tileLocation = new Location(@event.OffsetTileX(tile.X), @event.OffsetTileY(tile.Y));
				Game1.hooks.OnGameLocation_CheckAction(context.Location, tileLocation, Game1.viewport, @event.farmer, () => context.Location.checkAction(tileLocation, Game1.viewport, @event.farmer));
				Event event2 = @event;
				int currentCommand = event2.CurrentCommand;
				event2.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C0C RID: 15372 RVA: 0x002E5E9C File Offset: 0x002E409C
			public static void RemoveTile(Event @event, string[] args, EventContext context)
			{
				Point tile;
				string error;
				string layerId;
				if (!ArgUtility.TryGetPoint(args, 1, out tile, out error, "Point tile") || !ArgUtility.TryGet(args, 3, out layerId, out error, true, "string layerId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				context.Location.removeTile(@event.OffsetTileX(tile.X), @event.OffsetTileY(tile.Y), layerId);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C0D RID: 15373 RVA: 0x002E5F0C File Offset: 0x002E410C
			public static void TextAboveHead(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string text;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out text, out error, true, "string text"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (i == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				i.showTextAboveHead(text, null, 2, 3000, 0);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C0E RID: 15374 RVA: 0x002E5F9C File Offset: 0x002E419C
			public static void ShowFrame(Event @event, string[] args, EventContext context)
			{
				bool flip = false;
				string actorName;
				int frame;
				string error2;
				if (args.Length == 2)
				{
					actorName = "farmer";
					string error;
					if (!ArgUtility.TryGetInt(args, 1, out frame, out error, "frame"))
					{
						context.LogErrorAndSkip(error, false);
						return;
					}
				}
				else if (!ArgUtility.TryGet(args, 1, out actorName, out error2, true, "actorName") || !ArgUtility.TryGetInt(args, 2, out frame, out error2, "frame") || !ArgUtility.TryGetOptionalBool(args, 3, out flip, out error2, false, "flip"))
				{
					context.LogErrorAndSkip(error2, false);
					return;
				}
				int farmerNumber;
				if (!@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					if (actorName == "spouse" && i.Gender == Gender.Male && frame >= 36 && frame <= 38)
					{
						frame += 12;
					}
					i.Sprite.CurrentFrame = frame;
				}
				else
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.FarmerSprite.setCurrentAnimation(new FarmerSprite.AnimationFrame[]
						{
							new FarmerSprite.AnimationFrame(frame, 100, false, flip, null, false)
						});
						f.FarmerSprite.loop = true;
						f.FarmerSprite.loopThisAnimation = true;
						f.FarmerSprite.PauseForSingleAnimation = true;
						f.Sprite.currentFrame = frame;
					}
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C0F RID: 15375 RVA: 0x002E6108 File Offset: 0x002E4308
			public static void FarmerAnimation(Event @event, string[] args, EventContext context)
			{
				int animationId;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out animationId, out error, "int animationId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.farmer.FarmerSprite.setCurrentSingleAnimation(animationId);
				@event.farmer.FarmerSprite.PauseForSingleAnimation = true;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C10 RID: 15376 RVA: 0x002E6164 File Offset: 0x002E4364
			public static void IgnoreMovementAnimation(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				bool ignore;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetOptionalBool(args, 2, out ignore, out error, true, "bool ignore"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerId;
				if (@event.IsFarmerActorId(actorName, out farmerId))
				{
					Farmer f = @event.GetFarmerActor(farmerId);
					if (f != null)
					{
						f.ignoreMovementAnimation = ignore;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, true);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					i.ignoreMovementAnimation = ignore;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C11 RID: 15377 RVA: 0x002E6218 File Offset: 0x002E4418
			public static void Animate(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				bool flip;
				bool loop;
				int frameDuration;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetBool(args, 2, out flip, out error, "bool flip") || !ArgUtility.TryGetBool(args, 3, out loop, out error, "bool loop") || !ArgUtility.TryGetInt(args, 4, out frameDuration, out error, "int frameDuration"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>();
				for (int i = 5; i < args.Length; i++)
				{
					int frame;
					if (!ArgUtility.TryGetInt(args, i, out frame, out error, "int frame"))
					{
						context.LogErrorAndSkip(error, false);
						return;
					}
					animationFrames.Add(new FarmerSprite.AnimationFrame(frame, frameDuration, false, flip, null, false));
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
						f.FarmerSprite.loop = true;
						f.FarmerSprite.loopThisAnimation = loop;
						f.FarmerSprite.PauseForSingleAnimation = true;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC j = @event.getActorByName(actorName, out isOptionalNpc, true);
					if (j == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					j.Sprite.setCurrentAnimation(animationFrames);
					j.Sprite.loop = loop;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C12 RID: 15378 RVA: 0x002E6380 File Offset: 0x002E4580
			public static void StopAnimation(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int endFrame;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetOptionalInt(args, 2, out endFrame, out error, -1, "int endFrame"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.completelyStopAnimatingOrDoingAction();
						f.Halt();
						f.FarmerSprite.CurrentAnimation = null;
						switch (f.FacingDirection)
						{
						case 0:
							f.FarmerSprite.setCurrentSingleFrame(12, 32000, false, false);
							break;
						case 1:
							f.FarmerSprite.setCurrentSingleFrame(6, 32000, false, false);
							break;
						case 2:
							f.FarmerSprite.setCurrentSingleFrame(0, 32000, false, false);
							break;
						case 3:
							f.FarmerSprite.setCurrentSingleFrame(6, 32000, false, true);
							break;
						}
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					i.Sprite.StopAnimation();
					if (endFrame > -1)
					{
						i.Sprite.currentFrame = endFrame;
						i.Sprite.UpdateSourceRect();
					}
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C13 RID: 15379 RVA: 0x002E64EC File Offset: 0x002E46EC
			public static void ChangeLocation(Event @event, string[] args, EventContext context)
			{
				string locationName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out locationName, out error, true, "string locationName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Point playerTile = @event.farmer.TilePoint;
				@event.changeLocation(locationName, playerTile.X, playerTile.Y, delegate
				{
					Game1.currentLocation.ResetForEvent(@event);
					Event event2 = @event;
					int currentCommand = event2.CurrentCommand;
					event2.CurrentCommand = currentCommand + 1;
				});
			}

			// Token: 0x06003C14 RID: 15380 RVA: 0x002E6558 File Offset: 0x002E4758
			public static void Halt(Event @event, string[] args, EventContext context)
			{
				foreach (NPC npc in @event.actors)
				{
					npc.Halt();
				}
				@event.farmer.Halt();
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.continueAfterMove = false;
				@event.actorPositionsAfterMove.Clear();
			}

			// Token: 0x06003C15 RID: 15381 RVA: 0x002E65D8 File Offset: 0x002E47D8
			public static void Message(Event @event, string[] args, EventContext context)
			{
				string dialogue;
				string error;
				if (!ArgUtility.TryGet(args, 1, out dialogue, out error, true, "string dialogue"))
				{
					context.LogError(error, false);
				}
				if (!Game1.dialogueUp && Game1.activeClickableMenu == null)
				{
					Game1.drawDialogueNoTyping(Game1.parseText(dialogue));
				}
			}

			// Token: 0x06003C16 RID: 15382 RVA: 0x002E661C File Offset: 0x002E481C
			public static void AddCookingRecipe(Event @event, string[] args, EventContext context)
			{
				string recipeKey;
				string error;
				if (!ArgUtility.TryGetRemainder(args, 1, out recipeKey, out error, ' ', "string recipeKey"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.cookingRecipes.TryAdd(recipeKey, 0);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C17 RID: 15383 RVA: 0x002E6668 File Offset: 0x002E4868
			public static void ItemAboveHead(Event @event, string[] args, EventContext context)
			{
				string itemId = ArgUtility.Get(args, 1, null, true);
				bool showMessage = ArgUtility.GetBool(args, 2, true);
				string text = (itemId != null) ? itemId.ToLower() : null;
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
					{
						char c = text[2];
						if (c <= 'e')
						{
							if (c != 'd')
							{
								if (c == 'e')
								{
									if (text == "ore")
									{
										@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(O)334", 1, 0, false), showMessage);
										goto IL_2F4;
									}
								}
							}
							else if (text == "rod")
							{
								@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(T)BambooPole", 1, 0, false), showMessage);
								goto IL_2F4;
							}
						}
						else if (c != 'n')
						{
							if (c == 't')
							{
								if (text == "pot")
								{
									showMessage = ArgUtility.GetBool(args, 2, false);
									@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)62", 1, 0, false), showMessage);
									goto IL_2F4;
								}
							}
						}
						else if (text == "pan")
						{
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(T)Pan", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					}
					case 4:
					{
						char c = text[0];
						if (c != 'h')
						{
							if (c == 'j')
							{
								if (text == "joja")
								{
									@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)117", 1, 0, false), showMessage);
									goto IL_2F4;
								}
							}
						}
						else if (text == "hero")
						{
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)116", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					}
					case 5:
						if (text == "sword")
						{
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(W)0", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					case 7:
						if (text == "jukebox")
						{
							showMessage = ArgUtility.GetBool(args, 2, false);
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)209", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					case 8:
						if (text == "slimeegg")
						{
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(O)680", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					case 9:
						if (text == "sculpture")
						{
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(F)1306", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					case 10:
						if (text == "samboombox")
						{
							@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(F)1309", 1, 0, false), showMessage);
							goto IL_2F4;
						}
						break;
					}
				}
				if (text != null)
				{
					@event.farmer.holdUpItemThenMessage(ItemRegistry.Create(itemId, 1, 0, false), showMessage);
				}
				else
				{
					@event.farmer.holdUpItemThenMessage(null, false);
				}
				IL_2F4:
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C18 RID: 15384 RVA: 0x002E697C File Offset: 0x002E4B7C
			public static void AddCraftingRecipe(Event @event, string[] args, EventContext context)
			{
				string recipeKey;
				string error;
				if (!ArgUtility.TryGetRemainder(args, 1, out recipeKey, out error, ' ', "string recipeKey"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.craftingRecipes.TryAdd(recipeKey, 0);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C19 RID: 15385 RVA: 0x002E69C8 File Offset: 0x002E4BC8
			public static void HostMail(Event @event, string[] args, EventContext context)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out mailId, out error, true, "string mailId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (Game1.IsMasterGame && !Game1.player.hasOrWillReceiveMail(mailId))
				{
					Game1.addMailForTomorrow(mailId, false, false);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C1A RID: 15386 RVA: 0x002E6A20 File Offset: 0x002E4C20
			public static void Mail(Event @event, string[] args, EventContext context)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out mailId, out error, true, "string mailId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!Game1.player.hasOrWillReceiveMail(mailId))
				{
					Game1.addMailForTomorrow(mailId, false, false);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C1B RID: 15387 RVA: 0x002E6A70 File Offset: 0x002E4C70
			public static void MailToday(Event @event, string[] args, EventContext context)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out mailId, out error, true, "string mailId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!Game1.player.hasOrWillReceiveMail(mailId))
				{
					Game1.addMail(mailId, false, false);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C1C RID: 15388 RVA: 0x002E6AC0 File Offset: 0x002E4CC0
			public static void Shake(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int duration;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out duration, out error, "int duration"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				actor.shake(duration);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C1D RID: 15389 RVA: 0x002E6B3C File Offset: 0x002E4D3C
			public static void TemporaryAnimatedSprite(Event @event, string[] args, EventContext context)
			{
				string textureName;
				string error;
				Microsoft.Xna.Framework.Rectangle sourceRect;
				float animationInterval;
				int animationLength;
				int numberOfLoops;
				Vector2 tile;
				bool flicker;
				bool flip;
				float layerDepth;
				float alphaFade;
				int scale;
				float scaleChange;
				float rotation;
				float rotationChange;
				if (!ArgUtility.TryGet(args, 1, out textureName, out error, true, "string textureName") || !ArgUtility.TryGetRectangle(args, 2, out sourceRect, out error, "Rectangle sourceRect") || !ArgUtility.TryGetFloat(args, 6, out animationInterval, out error, "float animationInterval") || !ArgUtility.TryGetInt(args, 7, out animationLength, out error, "int animationLength") || !ArgUtility.TryGetInt(args, 8, out numberOfLoops, out error, "int numberOfLoops") || !ArgUtility.TryGetVector2(args, 9, out tile, out error, true, "Vector2 tile") || !ArgUtility.TryGetBool(args, 11, out flicker, out error, "bool flicker") || !ArgUtility.TryGetBool(args, 12, out flip, out error, "bool flip") || !ArgUtility.TryGetFloat(args, 13, out layerDepth, out error, "float layerDepth") || !ArgUtility.TryGetFloat(args, 14, out alphaFade, out error, "float alphaFade") || !ArgUtility.TryGetInt(args, 15, out scale, out error, "int scale") || !ArgUtility.TryGetFloat(args, 16, out scaleChange, out error, "float scaleChange") || !ArgUtility.TryGetFloat(args, 17, out rotation, out error, "float rotation") || !ArgUtility.TryGetFloat(args, 18, out rotationChange, out error, "float rotationChange"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, animationInterval, animationLength, numberOfLoops, @event.OffsetPosition(tile * 64f), flicker, flip, @event.OffsetPosition(new Vector2(0f, layerDepth) * 64f).Y / 10000f, alphaFade, Color.White, (float)(4 * scale), scaleChange, rotation, rotationChange, false);
				for (int i = 19; i < args.Length; i++)
				{
					string a = args[i];
					string rawColor;
					if (!(a == "color"))
					{
						if (!(a == "hold_last_frame"))
						{
							if (!(a == "ping_pong"))
							{
								Vector2 value3;
								if (!(a == "motion"))
								{
									Vector2 value2;
									if (!(a == "acceleration"))
									{
										Vector2 value;
										if (!(a == "acceleration_change"))
										{
											context.LogError("unknown option '" + args[i] + "'", false);
										}
										else if (!ArgUtility.TryGetVector2(args, i + 1, out value, out error, false, "Vector2 value"))
										{
											context.LogError(error, false);
										}
										else
										{
											tempSprite.accelerationChange = value;
											i += 2;
										}
									}
									else if (!ArgUtility.TryGetVector2(args, i + 1, out value2, out error, false, "Vector2 value"))
									{
										context.LogError(error, false);
									}
									else
									{
										tempSprite.acceleration = value2;
										i += 2;
									}
								}
								else if (!ArgUtility.TryGetVector2(args, i + 1, out value3, out error, false, "Vector2 value"))
								{
									context.LogError(error, false);
								}
								else
								{
									tempSprite.motion = value3;
									i += 2;
								}
							}
							else
							{
								tempSprite.pingPong = true;
							}
						}
						else
						{
							tempSprite.holdLastFrame = true;
						}
					}
					else if (!ArgUtility.TryGet(args, i + 1, out rawColor, out error, true, "string rawColor"))
					{
						context.LogError(error, false);
					}
					else
					{
						Color? color = Utility.StringToColor(rawColor);
						if (color != null)
						{
							tempSprite.color = color.Value;
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
							defaultInterpolatedStringHandler.AppendLiteral("index ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i + 1);
							defaultInterpolatedStringHandler.AppendLiteral(" has value '");
							defaultInterpolatedStringHandler.AppendFormatted(rawColor);
							defaultInterpolatedStringHandler.AppendLiteral("', which can't be parsed as a color");
							context.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						}
						i++;
					}
				}
				context.Location.TemporarySprites.Add(tempSprite);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C1E RID: 15390 RVA: 0x002E6EE4 File Offset: 0x002E50E4
			public static void TemporarySprite(Event @event, string[] args, EventContext context)
			{
				Vector2 tile;
				string error;
				int rowInAnimationSheet;
				int animationLength;
				float animationInterval;
				bool flipped;
				float layerDepth;
				if (!ArgUtility.TryGetVector2(args, 1, out tile, out error, true, "Vector2 tile") || !ArgUtility.TryGetInt(args, 3, out rowInAnimationSheet, out error, "int rowInAnimationSheet") || !ArgUtility.TryGetInt(args, 4, out animationLength, out error, "int animationLength") || !ArgUtility.TryGetOptionalFloat(args, 5, out animationInterval, out error, 300f, "float animationInterval") || !ArgUtility.TryGetOptionalBool(args, 6, out flipped, out error, false, "bool flipped") || !ArgUtility.TryGetOptionalFloat(args, 7, out layerDepth, out error, -1f, "float layerDepth"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				context.Location.TemporarySprites.Add(new TemporaryAnimatedSprite(rowInAnimationSheet, @event.OffsetPosition(tile * 64f), Color.White, animationLength, flipped, animationInterval, 0, 64, layerDepth, -1, 0));
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C1F RID: 15391 RVA: 0x002E6FBC File Offset: 0x002E51BC
			public static void RemoveTemporarySprites(Event @event, string[] args, EventContext context)
			{
				context.Location.TemporarySprites.Clear();
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C20 RID: 15392 RVA: 0x002E6FE9 File Offset: 0x002E51E9
			public static void Null(Event @event, string[] args, EventContext context)
			{
			}

			// Token: 0x06003C21 RID: 15393 RVA: 0x002E6FEC File Offset: 0x002E51EC
			public static void SpecificTemporarySprite(Event @event, string[] args, EventContext context)
			{
				string spriteId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out spriteId, out error, true, "string spriteId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.addSpecificTemporarySprite(spriteId, context.Location, args);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C22 RID: 15394 RVA: 0x002E7034 File Offset: 0x002E5234
			public static void PlayMusic(Event @event, string[] args, EventContext context)
			{
				string musicId;
				string error;
				if (!ArgUtility.TryGetRemainder(args, 1, out musicId, out error, ' ', "string musicId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (musicId == "samBand")
				{
					if (Game1.player.DialogueQuestionsAnswered.Contains("78"))
					{
						Game1.changeMusicTrack("shimmeringbastion", false, MusicContext.Event);
					}
					else if (Game1.player.DialogueQuestionsAnswered.Contains("79"))
					{
						Game1.changeMusicTrack("honkytonky", false, MusicContext.Event);
					}
					else if (Game1.player.DialogueQuestionsAnswered.Contains("77"))
					{
						Game1.changeMusicTrack("heavy", false, MusicContext.Event);
					}
					else
					{
						Game1.changeMusicTrack("poppy", false, MusicContext.Event);
					}
				}
				else if (Game1.options.musicVolumeLevel > 0f)
				{
					Game1.changeMusicTrack(musicId, false, MusicContext.Event);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C23 RID: 15395 RVA: 0x002E7110 File Offset: 0x002E5310
			public static void MakeInvisible(Event @event, string[] args, EventContext context)
			{
				Point tile;
				string error;
				int width;
				int height;
				if (!ArgUtility.TryGetPoint(args, 1, out tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(args, 3, out width, out error, 1, "int width") || !ArgUtility.TryGetOptionalInt(args, 4, out height, out error, 1, "int height"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				GameLocation location = context.Location;
				int originX = @event.OffsetTileX(tile.X);
				int originY = @event.OffsetTileY(tile.Y);
				int currentCommand;
				for (int y = originY; y < originY + height; y++)
				{
					for (int x = originX; x < originX + width; x++)
					{
						Object o = location.getObjectAtTile(x, y, false);
						TerrainFeature terrainFeature;
						if (o != null)
						{
							BedFurniture bed = o as BedFurniture;
							if (bed != null && bed.GetBoundingBox().Contains(Utility.Vector2ToPoint(Game1.player.mostRecentBed)))
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
							o.isTemporarilyInvisible = true;
						}
						else if (location.terrainFeatures.TryGetValue(new Vector2((float)x, (float)y), out terrainFeature))
						{
							terrainFeature.isTemporarilyInvisible = true;
						}
					}
				}
				currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C24 RID: 15396 RVA: 0x002E7248 File Offset: 0x002E5448
			public static void AddObject(Event @event, string[] args, EventContext context)
			{
				Point tile;
				string error;
				string itemId;
				float layerDepth;
				if (!ArgUtility.TryGetPoint(args, 1, out tile, out error, "Point tile") || !ArgUtility.TryGet(args, 3, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalFloat(args, 4, out layerDepth, out error, -1f, "float layerDepth"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Vector2 pixelPos = @event.OffsetPosition(new Vector2((float)(tile.X * 64), (float)(tile.Y * 64)));
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, Microsoft.Xna.Framework.Rectangle.Empty, pixelPos, false, 0f, Color.White)
				{
					layerDepth = ((layerDepth >= 0f) ? layerDepth : ((float)(@event.OffsetTileY(tile.Y) * 64) / 10000f))
				};
				sprite.CopyAppearanceFromItemId(itemId, 0);
				context.Location.TemporarySprites.Add(sprite);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C25 RID: 15397 RVA: 0x002E733C File Offset: 0x002E553C
			public static void AddBigProp(Event @event, string[] args, EventContext context)
			{
				Vector2 tile;
				string error;
				string itemId;
				if (!ArgUtility.TryGetVector2(args, 1, out tile, out error, true, "Vector2 tile") || !ArgUtility.TryGet(args, 3, out itemId, out error, true, "string itemId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Object prop = ItemRegistry.Create<Object>("(BC)" + itemId, 1, 0, false);
				prop.TileLocation = @event.OffsetTile(tile);
				@event.props.Add(prop);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C26 RID: 15398 RVA: 0x002E73C9 File Offset: 0x002E55C9
			public static void AddFloorProp(Event @event, string[] args, EventContext context)
			{
				Event.DefaultCommands.AddProp(@event, args, context);
			}

			// Token: 0x06003C27 RID: 15399 RVA: 0x002E73D4 File Offset: 0x002E55D4
			public static void AddProp(Event @event, string[] args, EventContext context)
			{
				string commandName;
				string error;
				int index;
				Point tile;
				int drawWidth;
				int drawHeight;
				int boundingHeight;
				int tilesHorizontal;
				int tilesVertical;
				if (!ArgUtility.TryGet(args, 0, out commandName, out error, true, "string commandName") || !ArgUtility.TryGetInt(args, 1, out index, out error, "int index") || !ArgUtility.TryGetPoint(args, 2, out tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(args, 4, out drawWidth, out error, 1, "int drawWidth") || !ArgUtility.TryGetOptionalInt(args, 5, out drawHeight, out error, 1, "int drawHeight") || !ArgUtility.TryGetOptionalInt(args, 6, out boundingHeight, out error, drawHeight, "int boundingHeight") || !ArgUtility.TryGetOptionalInt(args, 7, out tilesHorizontal, out error, 0, "int tilesHorizontal") || !ArgUtility.TryGetOptionalInt(args, 8, out tilesVertical, out error, 0, "int tilesVertical"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int tileX = @event.OffsetTileX(tile.X);
				int tileY = @event.OffsetTileY(tile.Y);
				bool solid = !commandName.EqualsIgnoreCase("AddFloorProp");
				@event.festivalProps.Add(new Prop(@event.festivalTexture, index, drawWidth, boundingHeight, drawHeight, tileX, tileY, solid));
				if (tilesHorizontal != 0)
				{
					for (int x = tileX + tilesHorizontal; x != tileX; x -= Math.Sign(tilesHorizontal))
					{
						@event.festivalProps.Add(new Prop(@event.festivalTexture, index, drawWidth, boundingHeight, drawHeight, x, tileY, solid));
					}
				}
				if (tilesVertical != 0)
				{
					for (int y = tileY + tilesVertical; y != tileY; y -= Math.Sign(tilesVertical))
					{
						@event.festivalProps.Add(new Prop(@event.festivalTexture, index, drawWidth, boundingHeight, drawHeight, tileX, y, solid));
					}
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C28 RID: 15400 RVA: 0x002E757C File Offset: 0x002E577C
			public static void RemoveObject(Event @event, string[] args, EventContext context)
			{
				Point tile;
				string error;
				if (!ArgUtility.TryGetPoint(args, 1, out tile, out error, "Point tile"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				GameLocation location = context.Location;
				Vector2 position = @event.OffsetPosition(new Vector2((float)tile.X, (float)tile.Y) * 64f);
				location.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.position == position);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(location, context.Time);
			}

			// Token: 0x06003C29 RID: 15401 RVA: 0x002E7614 File Offset: 0x002E5814
			public static void Glow(Event @event, string[] args, EventContext context)
			{
				int red;
				string error;
				int green;
				int blue;
				bool hold;
				if (!ArgUtility.TryGetInt(args, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(args, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(args, 3, out blue, out error, "int blue") || !ArgUtility.TryGetOptionalBool(args, 4, out hold, out error, false, "bool hold"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.screenGlowOnce(new Color(red, green, blue), hold, 0.005f, 0.3f);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C2A RID: 15402 RVA: 0x002E76A0 File Offset: 0x002E58A0
			public static void StopGlowing(Event @event, string[] args, EventContext context)
			{
				Game1.screenGlowUp = false;
				Game1.screenGlowHold = false;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C2B RID: 15403 RVA: 0x002E76CC File Offset: 0x002E58CC
			public static void AddQuest(Event @event, string[] args, EventContext context)
			{
				string questId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out questId, out error, true, "string questId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.addQuest(questId);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C2C RID: 15404 RVA: 0x002E7710 File Offset: 0x002E5910
			public static void RemoveQuest(Event @event, string[] args, EventContext context)
			{
				string questId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out questId, out error, true, "string questId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.removeQuest(questId);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C2D RID: 15405 RVA: 0x002E7754 File Offset: 0x002E5954
			public static void AddSpecialOrder(Event @event, string[] args, EventContext context)
			{
				string orderId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out orderId, out error, true, "string orderId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.team.AddSpecialOrder(orderId, null, false);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C2E RID: 15406 RVA: 0x002E77A8 File Offset: 0x002E59A8
			public static void RemoveSpecialOrder(Event @event, string[] args, EventContext context)
			{
				string orderId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out orderId, out error, true, "string orderId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.team.specialOrders.RemoveWhere((SpecialOrder order) => order.questKey.Value == orderId);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C2F RID: 15407 RVA: 0x002E780C File Offset: 0x002E5A0C
			public static void AddItem(Event @event, string[] args, EventContext context)
			{
				string itemId;
				string error;
				int count;
				int quality;
				if (!ArgUtility.TryGet(args, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(args, 2, out count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(args, 3, out quality, out error, 0, "int quality"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Item i = ItemRegistry.Create(itemId, count, quality, false);
				if (i != null)
				{
					Game1.player.addItemByMenuIfNecessary(i, null, false);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C30 RID: 15408 RVA: 0x002E788C File Offset: 0x002E5A8C
			public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
			{
				string text;
				int currentCommand;
				if (args.Length == 1)
				{
					text = @event.id;
					if (text == "festival_spring13")
					{
						if (@event.festivalWinners.Contains(Game1.player.UniqueMultiplayerID))
						{
							if (Game1.player.mailReceived.Add("Egg Festival"))
							{
								if (Game1.activeClickableMenu == null)
								{
									Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(H)4", 1, 0, false), null, false);
								}
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								if (Game1.activeClickableMenu == null)
								{
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
							else
							{
								if (Game1.activeClickableMenu == null)
								{
									Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)PrizeTicket", 1, 0, false), null, false);
								}
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								if (Game1.activeClickableMenu == null)
								{
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
						}
						else
						{
							@event.CurrentCommand += 2;
						}
						return;
					}
					if (text == "festival_winter8")
					{
						if (@event.festivalWinners.Contains(Game1.player.UniqueMultiplayerID))
						{
							if (Game1.player.mailReceived.Add("Ice Festival"))
							{
								if (Game1.activeClickableMenu == null)
								{
									Game1.activeClickableMenu = new ItemGrabMenu(new Item[]
									{
										ItemRegistry.Create("(H)17", 1, 0, false),
										ItemRegistry.Create("(O)687", 1, 0, false),
										ItemRegistry.Create("(O)691", 1, 0, false),
										ItemRegistry.Create("(O)703", 1, 0, false)
									}, @event).setEssential(true, false);
								}
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
							if (Game1.activeClickableMenu == null)
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)PrizeTicket", 1, 0, false), null, false);
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
						}
						else
						{
							@event.CurrentCommand += 2;
						}
						return;
					}
				}
				string itemId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out itemId, out error, true, "string itemId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				text = itemId.ToLower();
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
					{
						char c = text[2];
						if (c != 'd')
						{
							if (c != 'n')
							{
								if (c == 't')
								{
									if (text == "pot")
									{
										Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)62", 1, 0, false), null, false);
										if (Game1.activeClickableMenu == null)
										{
											currentCommand = @event.CurrentCommand;
											@event.CurrentCommand = currentCommand + 1;
										}
										currentCommand = @event.CurrentCommand;
										@event.CurrentCommand = currentCommand + 1;
										return;
									}
								}
							}
							else if (text == "pan")
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)Pan", 1, 0, false), null, false);
								if (Game1.activeClickableMenu == null)
								{
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
								}
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
						}
						else if (text == "rod")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)BambooPole", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					}
					case 4:
					{
						char c = text[0];
						if (c != 'h')
						{
							if (c == 'j')
							{
								if (text == "joja")
								{
									Game1.getSteamAchievement("Achievement_Joja");
									Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)117", 1, 0, false), null, false);
									if (Game1.activeClickableMenu == null)
									{
										currentCommand = @event.CurrentCommand;
										@event.CurrentCommand = currentCommand + 1;
									}
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
						}
						else if (text == "hero")
						{
							Game1.getSteamAchievement("Achievement_LocalLegend");
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)116", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					}
					case 5:
						if (text == "sword")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(W)0", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					case 6:
						if (text == "qimilk")
						{
							if (Game1.player.mailReceived.Add("qiCave"))
							{
								Game1.player.maxHealth += 25;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					case 7:
					{
						char c = text[0];
						if (c != 'j')
						{
							if (c == 'm')
							{
								if (text == "memento")
								{
									Object o = ItemRegistry.Create<Object>("(O)864", 1, 0, false);
									o.specialItem = true;
									o.questItem.Value = true;
									Game1.player.addItemByMenuIfNecessary(o, null, false);
									if (Game1.activeClickableMenu == null)
									{
										currentCommand = @event.CurrentCommand;
										@event.CurrentCommand = currentCommand + 1;
									}
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
						}
						else if (text == "jukebox")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)209", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					}
					case 8:
					{
						char c = text[0];
						if (c != 'm')
						{
							if (c == 's')
							{
								if (text == "slimeegg")
								{
									Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)680", 1, 0, false), null, false);
									if (Game1.activeClickableMenu == null)
									{
										currentCommand = @event.CurrentCommand;
										@event.CurrentCommand = currentCommand + 1;
									}
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
						}
						else if (text == "meowmere")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(W)65", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					}
					case 9:
						if (text == "sculpture")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1306", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					case 10:
						if (text == "samboombox")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1309", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					case 12:
					{
						char c = text[0];
						if (c != 'b')
						{
							if (c == 'e')
							{
								if (text == "emilyclothes")
								{
									Clothing pants = ItemRegistry.Create<Clothing>("(P)8", 1, 0, false);
									pants.Dye(new Color(0, 143, 239), 1f);
									Game1.player.addItemsByMenuIfNecessary(new List<Item>
									{
										ItemRegistry.Create("(B)804", 1, 0, false),
										ItemRegistry.Create("(H)41", 1, 0, false),
										ItemRegistry.Create("(S)1127", 1, 0, false),
										pants
									}, null, false);
									if (Game1.activeClickableMenu == null)
									{
										currentCommand = @event.CurrentCommand;
										@event.CurrentCommand = currentCommand + 1;
									}
									currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
						}
						else if (text == "birdiereward")
						{
							Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
							if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
							{
								Game1.addMailForTomorrow("gotBirdieReward", true, true);
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					}
					case 14:
						if (text == "marniepainting")
						{
							Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1802", 1, 0, false), null, false);
							if (Game1.activeClickableMenu == null)
							{
								currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
							return;
						}
						break;
					}
				}
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create(itemId, 1, 0, false), null, false);
				if (Game1.activeClickableMenu == null)
				{
					currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
				currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C31 RID: 15409 RVA: 0x002E81E4 File Offset: 0x002E63E4
			public static void AttachCharacterToTempSprite(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				TemporaryAnimatedSprite t = context.Location.temporarySprites.Last<TemporaryAnimatedSprite>();
				if (t != null)
				{
					t.attachedCharacter = @event.getActorByName(actorName, false);
				}
			}

			// Token: 0x06003C32 RID: 15410 RVA: 0x002E8230 File Offset: 0x002E6430
			public static void Fork(Event @event, string[] args, EventContext context)
			{
				string requiredId;
				string error;
				string newKey;
				bool isTranslationKey;
				if (!ArgUtility.TryGet(args, 1, out requiredId, out error, true, "string requiredId") || !ArgUtility.TryGetOptional(args, 2, out newKey, out error, null, true, "string newKey") || !ArgUtility.TryGetOptionalBool(args, 3, out isTranslationKey, out error, false, "bool isTranslationKey"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (newKey == null)
				{
					newKey = requiredId;
					requiredId = null;
				}
				if (!((requiredId != null) ? (Game1.player.mailReceived.Contains(requiredId) || Game1.player.dialogueQuestionsAnswered.Contains(requiredId)) : @event.specialEventVariable1))
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					return;
				}
				string[] commands;
				if (isTranslationKey)
				{
					string raw = Game1.content.LoadStringReturnNullIfNotFound(newKey, true);
					if (raw == null)
					{
						context.LogErrorAndSkip("can't load new script from translation key '" + newKey + "' because that translation wasn't found", false);
						return;
					}
					commands = Event.ParseCommands(raw, context.Event.farmer);
				}
				else if (@event.isFestival)
				{
					string raw2;
					if (!@event.TryGetFestivalDataForYear(newKey, out raw2))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
						defaultInterpolatedStringHandler.AppendLiteral("can't load new script from festival field '");
						defaultInterpolatedStringHandler.AppendFormatted(newKey);
						defaultInterpolatedStringHandler.AppendLiteral("' because there's no such key in the '");
						defaultInterpolatedStringHandler.AppendFormatted(@event.id);
						defaultInterpolatedStringHandler.AppendLiteral("' festival");
						context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						return;
					}
					commands = Event.ParseCommands(raw2, context.Event.farmer);
				}
				else
				{
					string assetName = "Data\\Events\\" + Game1.currentLocation.Name;
					if (!Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
					{
						context.LogErrorAndSkip("can't load new script from event asset '" + assetName + "' because it doesn't exist", false);
						return;
					}
					string raw3;
					if (!Game1.content.Load<Dictionary<string, string>>(assetName).TryGetValue(newKey, out raw3))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(88, 2);
						defaultInterpolatedStringHandler.AppendLiteral("can't load new script from event asset '");
						defaultInterpolatedStringHandler.AppendFormatted(assetName);
						defaultInterpolatedStringHandler.AppendLiteral("' because it doesn't contain the required '");
						defaultInterpolatedStringHandler.AppendFormatted(newKey);
						defaultInterpolatedStringHandler.AppendLiteral("' key");
						context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						return;
					}
					commands = Event.ParseCommands(raw3, context.Event.farmer);
				}
				@event.ReplaceAllCommands(commands);
				@event.forked = true;
			}

			// Token: 0x06003C33 RID: 15411 RVA: 0x002E8454 File Offset: 0x002E6654
			public static void SwitchEvent(Event @event, string[] args, EventContext context)
			{
				string newKey;
				string error;
				if (!ArgUtility.TryGet(args, 1, out newKey, out error, true, "string newKey"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				string[] commands;
				if (@event.isFestival)
				{
					string raw;
					if (!@event.TryGetFestivalDataForYear(newKey, out raw))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 2);
						defaultInterpolatedStringHandler.AppendLiteral("can't load new event from festival field '");
						defaultInterpolatedStringHandler.AppendFormatted(newKey);
						defaultInterpolatedStringHandler.AppendLiteral("' because there's no such key in the '");
						defaultInterpolatedStringHandler.AppendFormatted(@event.id);
						defaultInterpolatedStringHandler.AppendLiteral("' festival");
						context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						return;
					}
					commands = Event.ParseCommands(raw, context.Event.farmer);
				}
				else
				{
					string assetName = "Data\\Events\\" + Game1.currentLocation.Name;
					if (!Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
					{
						context.LogErrorAndSkip("can't load new event from asset '" + assetName + "' because it doesn't exist", false);
						return;
					}
					string raw2;
					if (!Game1.content.Load<Dictionary<string, string>>(assetName).TryGetValue(newKey, out raw2))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
						defaultInterpolatedStringHandler.AppendLiteral("can't load new event from asset '");
						defaultInterpolatedStringHandler.AppendFormatted(assetName);
						defaultInterpolatedStringHandler.AppendLiteral("' because it doesn't contain the required '");
						defaultInterpolatedStringHandler.AppendFormatted(newKey);
						defaultInterpolatedStringHandler.AppendLiteral("' key");
						context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						return;
					}
					commands = Event.ParseCommands(raw2, context.Event.farmer);
				}
				@event.ReplaceAllCommands(commands);
				@event.eventSwitched = true;
			}

			// Token: 0x06003C34 RID: 15412 RVA: 0x002E85BC File Offset: 0x002E67BC
			public static void GlobalFade(Event @event, string[] args, EventContext context)
			{
				float fadeSpeed;
				string error;
				bool continueEventDuringFade;
				if (!ArgUtility.TryGetOptionalFloat(args, 1, out fadeSpeed, out error, 0.007f, "float fadeSpeed") || !ArgUtility.TryGetOptionalBool(args, 2, out continueEventDuringFade, out error, false, "bool continueEventDuringFade"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!Game1.globalFade)
				{
					if (continueEventDuringFade)
					{
						Game1.globalFadeToBlack(null, fadeSpeed);
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
						return;
					}
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(@event.incrementCommandAfterFade), fadeSpeed);
				}
			}

			// Token: 0x06003C35 RID: 15413 RVA: 0x002E8630 File Offset: 0x002E6830
			public static void GlobalFadeToClear(Event @event, string[] args, EventContext context)
			{
				float fadeSpeed;
				string error;
				bool continueEventDuringFade;
				if (!ArgUtility.TryGetOptionalFloat(args, 1, out fadeSpeed, out error, 0.007f, "float fadeSpeed") || !ArgUtility.TryGetOptionalBool(args, 2, out continueEventDuringFade, out error, false, "bool continueEventDuringFade"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!Game1.globalFade)
				{
					if (continueEventDuringFade)
					{
						Game1.globalFadeToClear(null, fadeSpeed);
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
						return;
					}
					Game1.globalFadeToClear(new Game1.afterFadeFunction(@event.incrementCommandAfterFade), fadeSpeed);
				}
			}

			// Token: 0x06003C36 RID: 15414 RVA: 0x002E86A4 File Offset: 0x002E68A4
			public static void Cutscene(Event @event, string[] args, EventContext context)
			{
				string cutsceneId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out cutsceneId, out error, true, "string cutsceneId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				GameLocation location = context.Location;
				GameTime time = context.Time;
				if (@event.currentCustomEventScript != null)
				{
					if (@event.currentCustomEventScript.update(time, @event))
					{
						@event.currentCustomEventScript = null;
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
						return;
					}
				}
				else if (Game1.currentMinigame == null)
				{
					if (cutsceneId != null)
					{
						switch (cutsceneId.Length)
						{
						case 5:
						{
							char c = cutsceneId[0];
							if (c != 'p')
							{
								if (c == 'r')
								{
									if (cutsceneId == "robot")
									{
										Game1.currentMinigame = new RobotBlastoff();
									}
								}
							}
							else if (cutsceneId == "plane")
							{
								Game1.currentMinigame = new PlaneFlyBy();
							}
							break;
						}
						case 8:
						{
							char c = cutsceneId[0];
							if (c != 'b')
							{
								if (c == 'g')
								{
									if (cutsceneId == "greenTea")
									{
										@event.currentCustomEventScript = new EventScript_GreenTea(new Vector2(-64000f, -64000f), @event);
									}
								}
							}
							else if (cutsceneId == "bandFork")
							{
								int whichBand = 76;
								if (Game1.player.dialogueQuestionsAnswered.Contains("77"))
								{
									whichBand = 77;
								}
								else if (Game1.player.dialogueQuestionsAnswered.Contains("78"))
								{
									whichBand = 78;
								}
								else if (Game1.player.dialogueQuestionsAnswered.Contains("79"))
								{
									whichBand = 79;
								}
								@event.answerDialogue("bandFork", whichBand);
								int currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
							break;
						}
						case 9:
						{
							char c = cutsceneId[0];
							if (c != 'b')
							{
								if (c != 'h')
								{
									if (c == 'm')
									{
										if (cutsceneId == "marucomet")
										{
											Game1.currentMinigame = new MaruComet();
										}
									}
								}
								else if (cutsceneId == "haleyCows")
								{
									Game1.currentMinigame = new HaleyCowPictures();
								}
							}
							else if (cutsceneId == "boardGame")
							{
								Game1.currentMinigame = new FantasyBoardGame();
								int currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							break;
						}
						case 11:
							if (cutsceneId == "AbigailGame")
							{
								Game1.currentMinigame = new AbigailGame(@event.getActorByName("Abigail", false) ?? Game1.RequireCharacter("Abigail", true));
							}
							break;
						case 13:
						{
							char c = cutsceneId[0];
							if (c != 'b')
							{
								if (c != 'e')
								{
									if (c == 'g')
									{
										if (cutsceneId == "governorTaste")
										{
											@event.governorTaste();
											@event.currentCommand++;
											return;
										}
									}
								}
								else if (cutsceneId == "eggHuntWinner")
								{
									@event.eggHuntWinner();
									int currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
									return;
								}
							}
							else if (cutsceneId == "balloonDepart")
							{
								TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(1);
								temporarySpriteByID.attachedCharacter = @event.farmer;
								temporarySpriteByID.motion = new Vector2(0f, -2f);
								TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(2);
								temporarySpriteByID2.attachedCharacter = @event.getActorByName("Harvey", false);
								temporarySpriteByID2.motion = new Vector2(0f, -2f);
								location.getTemporarySpriteByID(3).scaleChange = -0.01f;
								int currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
							break;
						}
						case 14:
							if (cutsceneId == "linusMoneyGone")
							{
								foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in location.temporarySprites)
								{
									temporaryAnimatedSprite.alphaFade = 0.01f;
									temporaryAnimatedSprite.motion = new Vector2(0f, -1f);
								}
								int currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
								return;
							}
							break;
						case 16:
						{
							char c = cutsceneId[0];
							if (c != 'b')
							{
								if (c != 'c')
								{
									if (c == 'i')
									{
										if (cutsceneId == "iceFishingWinner")
										{
											@event.iceFishingWinner();
											@event.currentCommand++;
											return;
										}
									}
								}
								else if (cutsceneId == "clearTempSprites")
								{
									location.temporarySprites.Clear();
									int currentCommand = @event.CurrentCommand;
									@event.CurrentCommand = currentCommand + 1;
								}
							}
							else if (cutsceneId == "balloonChangeMap")
							{
								@event.eventPositionTileOffset = Vector2.Zero;
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, @event.OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(-23f, 0f) * 4f), false, false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(0f, -2f),
									yStopCoordinate = (int)@event.OffsetPositionY(576f),
									reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(@event.balloonInSky),
									attachedCharacter = @event.farmer,
									id = 1
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, @event.OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(0f, 134f) * 4f), false, false, 0.2625f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(0f, -2f),
									id = 2,
									attachedCharacter = @event.getActorByName("Harvey", false)
								});
								int currentCommand = @event.CurrentCommand;
								@event.CurrentCommand = currentCommand + 1;
							}
							break;
						}
						case 18:
						{
							char c = cutsceneId[0];
							if (c != 'a')
							{
								if (c == 'i')
								{
									if (cutsceneId == "iceFishingWinnerMP")
									{
										@event.iceFishingWinnerMP();
										@event.currentCommand++;
										return;
									}
								}
							}
							else if (cutsceneId == "addSecretSantaItem")
							{
								Item o = Utility.getGiftFromNPC(@event.mySecretSanta);
								Game1.player.addItemByMenuIfNecessaryElseHoldUp(o, null, false);
								@event.currentCommand++;
								return;
							}
							break;
						}
						}
					}
					Game1.globalFadeToClear(null, 0.01f);
				}
			}

			// Token: 0x06003C37 RID: 15415 RVA: 0x002E8E24 File Offset: 0x002E7024
			public static void WaitForTempSprite(Event @event, string[] args, EventContext context)
			{
				int spriteId;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out spriteId, out error, "int spriteId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (Game1.currentLocation.getTemporarySpriteByID(spriteId) != null)
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
			}

			// Token: 0x06003C38 RID: 15416 RVA: 0x002E8E6C File Offset: 0x002E706C
			public static void Cave(Event @event, string[] args, EventContext context)
			{
				if (Game1.activeClickableMenu == null)
				{
					Response[] responses = new Response[]
					{
						new Response("Mushrooms", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1220")),
						new Response("Bats", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1222"))
					};
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1223"), responses, "cave");
					Game1.dialogueTyping = false;
				}
			}

			// Token: 0x06003C39 RID: 15417 RVA: 0x002E8EE4 File Offset: 0x002E70E4
			public static void UpdateMinigame(Event @event, string[] args, EventContext context)
			{
				int eventData;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out eventData, out error, "int eventData"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				IMinigame currentMinigame = Game1.currentMinigame;
				if (currentMinigame != null)
				{
					currentMinigame.receiveEventPoke(eventData);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C3A RID: 15418 RVA: 0x002E8F30 File Offset: 0x002E7130
			public static void StartJittering(Event @event, string[] args, EventContext context)
			{
				@event.farmer.jitterStrength = 1f;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C3B RID: 15419 RVA: 0x002E8F60 File Offset: 0x002E7160
			public static void Money(Event @event, string[] args, EventContext context)
			{
				int amount;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out amount, out error, "int amount"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.farmer.Money += amount;
				if (@event.farmer.Money < 0)
				{
					@event.farmer.Money = 0;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C3C RID: 15420 RVA: 0x002E8FC8 File Offset: 0x002E71C8
			public static void StopJittering(Event @event, string[] args, EventContext context)
			{
				@event.farmer.stopJittering();
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C3D RID: 15421 RVA: 0x002E8FF0 File Offset: 0x002E71F0
			public static void AddLantern(Event @event, string[] args, EventContext context)
			{
				int initialParentSheetIndex;
				string error;
				Vector2 tile;
				int lightRadius;
				if (!ArgUtility.TryGetInt(args, 1, out initialParentSheetIndex, out error, "int initialParentSheetIndex") || !ArgUtility.TryGetVector2(args, 2, out tile, out error, true, "Vector2 tile") || !ArgUtility.TryGetInt(args, 4, out lightRadius, out error, "int lightRadius"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				TemporaryAnimatedSpriteList temporarySprites = context.Location.TemporarySprites;
				TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(initialParentSheetIndex, 999999f, 1, 0, @event.OffsetPosition(tile * 64f), false, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler.AppendFormatted("AddLantern");
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<float>(tile.X);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<float>(tile.Y);
				temporaryAnimatedSprite.lightId = @event.GenerateLightSourceId(defaultInterpolatedStringHandler.ToStringAndClear());
				temporaryAnimatedSprite.lightRadius = (float)lightRadius;
				temporarySprites.Add(temporaryAnimatedSprite);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C3E RID: 15422 RVA: 0x002E90E4 File Offset: 0x002E72E4
			public static void RustyKey(Event @event, string[] args, EventContext context)
			{
				Game1.player.hasRustyKey = true;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C3F RID: 15423 RVA: 0x002E910C File Offset: 0x002E730C
			public static void Swimming(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer farmer = @event.GetFarmerActor(farmerNumber);
					if (farmer != null)
					{
						farmer.bathingClothes.Value = true;
						farmer.swimming.Value = true;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (actor == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					actor.swimming.Value = true;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C40 RID: 15424 RVA: 0x002E91B0 File Offset: 0x002E73B0
			public static void StopSwimming(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer farmer = @event.GetFarmerActor(farmerNumber);
					if (farmer != null)
					{
						farmer.bathingClothes.Value = (context.Location is BathHousePool);
						farmer.swimming.Value = false;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (actor == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					actor.swimming.Value = false;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C41 RID: 15425 RVA: 0x002E925E File Offset: 0x002E745E
			public static void TutorialMenu(Event @event, string[] args, EventContext context)
			{
				if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new TutorialMenu();
				}
			}

			// Token: 0x06003C42 RID: 15426 RVA: 0x002E9274 File Offset: 0x002E7474
			public static void AnimalNaming(Event @event, string[] args, EventContext context)
			{
				GameLocation currentLocation = Game1.currentLocation;
				AnimalHouse animalHouse = currentLocation as AnimalHouse;
				if (animalHouse == null)
				{
					context.LogErrorAndSkip("this command only works when run in an AnimalHouse location", false);
					return;
				}
				if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new NamingMenu(delegate(string animalName)
					{
						animalHouse.addNewHatchedAnimal(animalName);
						Event event2 = @event;
						int currentCommand = event2.CurrentCommand;
						event2.CurrentCommand = currentCommand + 1;
					}, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"), null);
				}
			}

			// Token: 0x06003C43 RID: 15427 RVA: 0x002E92E4 File Offset: 0x002E74E4
			public static void SplitSpeak(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string dialogue;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out dialogue, out error, true, "string dialogue"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				string[] choiceAnswers = LegacyShims.SplitAndTrim(dialogue, '~', StringSplitOptions.None);
				if (!Game1.dialogueUp)
				{
					@event.timeAccumulator += (float)context.Time.ElapsedGameTime.Milliseconds;
					if (@event.timeAccumulator < 500f)
					{
						return;
					}
					@event.timeAccumulator = 0f;
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false) ?? Game1.getCharacterFromName(actorName, true, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					if (!ArgUtility.HasIndex<string>(choiceAnswers, @event.previousAnswerChoice))
					{
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
						return;
					}
					i.CurrentDialogue.Push(new Dialogue(i, null, choiceAnswers[@event.previousAnswerChoice]));
					Game1.drawDialogue(i);
				}
			}

			// Token: 0x06003C44 RID: 15428 RVA: 0x002E93E8 File Offset: 0x002E75E8
			public static void CatQuestion(Event @event, string[] args, EventContext context)
			{
				if (!Game1.isQuestion && Game1.activeClickableMenu == null)
				{
					PetData data;
					string petType = Pet.TryGetData(Game1.player.whichPetType, out data) ? (TokenParser.ParseText(data.DisplayName, null, null, null) ?? "pet") : "pet";
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:AdoptPet", petType), Game1.currentLocation.createYesNoResponses(), "pet");
				}
			}

			// Token: 0x06003C45 RID: 15429 RVA: 0x002E9460 File Offset: 0x002E7660
			public static void AmbientLight(Event @event, string[] args, EventContext context)
			{
				int red;
				string error;
				int green;
				int blue;
				if (!ArgUtility.TryGetInt(args, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(args, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(args, 3, out blue, out error, "int blue"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.ambientLight = new Color(red, green, blue);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C46 RID: 15430 RVA: 0x002E94CC File Offset: 0x002E76CC
			public static void BgColor(Event @event, string[] args, EventContext context)
			{
				int red;
				string error;
				int green;
				int blue;
				if (!ArgUtility.TryGetInt(args, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(args, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(args, 3, out blue, out error, "int blue"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.setBGColor((byte)red, (byte)green, (byte)blue);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C47 RID: 15431 RVA: 0x002E9538 File Offset: 0x002E7738
			public static void ElliottBookTalk(Event @event, string[] args, EventContext context)
			{
				if (!Game1.dialogueUp)
				{
					string speechKey;
					if (Game1.player.dialogueQuestionsAnswered.Contains("958699"))
					{
						speechKey = "Strings\\StringsFromCSFiles:Event.cs.1257";
					}
					else if (Game1.player.dialogueQuestionsAnswered.Contains("958700"))
					{
						speechKey = "Strings\\StringsFromCSFiles:Event.cs.1258";
					}
					else if (Game1.player.dialogueQuestionsAnswered.Contains("9586701"))
					{
						speechKey = "Strings\\StringsFromCSFiles:Event.cs.1259";
					}
					else
					{
						speechKey = "Strings\\StringsFromCSFiles:Event.cs.1260";
					}
					NPC i = @event.getActorByName("Elliott", false) ?? Game1.getCharacterFromName("Elliott", true, false);
					i.CurrentDialogue.Push(new Dialogue(i, speechKey, false));
					Game1.drawDialogue(i);
				}
			}

			// Token: 0x06003C48 RID: 15432 RVA: 0x002E95E8 File Offset: 0x002E77E8
			public static void RemoveItem(Event @event, string[] args, EventContext context)
			{
				string itemId;
				string error;
				int count;
				if (!ArgUtility.TryGet(args, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(args, 2, out count, out error, 1, "int count"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.removeFirstOfThisItemFromInventory(itemId, count);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C49 RID: 15433 RVA: 0x002E9640 File Offset: 0x002E7840
			public static void Friendship(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int friendshipChange;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out friendshipChange, out error, "int friendshipChange"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				NPC character = Game1.getCharacterFromName(actorName, true, false);
				if (character == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", false);
					return;
				}
				Game1.player.changeFriendship(friendshipChange, character);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C4A RID: 15434 RVA: 0x002E96C0 File Offset: 0x002E78C0
			public static void SetRunning(Event @event, string[] args, EventContext context)
			{
				@event.farmer.setRunning(true, false);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C4B RID: 15435 RVA: 0x002E96EC File Offset: 0x002E78EC
			public static void ExtendSourceRect(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string rawOption;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out rawOption, out error, true, "string rawOption"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isReset = rawOption == "reset";
				int horizontal = -1;
				int vertical = -1;
				bool ignoreSourceRectUpdates = false;
				if (!isReset && (!ArgUtility.TryGetInt(args, 2, out horizontal, out error, "horizontal") || !ArgUtility.TryGetInt(args, 3, out vertical, out error, "vertical") || !ArgUtility.TryGetOptionalBool(args, 4, out ignoreSourceRectUpdates, out error, false, "ignoreSourceRectUpdates")))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (isReset)
				{
					actor.reloadSprite(false);
					actor.Sprite.SpriteWidth = 16;
					actor.Sprite.SpriteHeight = 32;
					actor.HideShadow = false;
				}
				else
				{
					actor.extendSourceRect(horizontal, vertical, ignoreSourceRectUpdates);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C4C RID: 15436 RVA: 0x002E97FC File Offset: 0x002E79FC
			public static void WaitForOtherPlayers(Event @event, string[] args, EventContext context)
			{
				string gateId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out gateId, out error, true, "string gateId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (Game1.IsMultiplayer)
				{
					Game1.netReady.SetLocalReady(gateId, true);
					if (Game1.netReady.IsReady(gateId))
					{
						if (Game1.activeClickableMenu is ReadyCheckDialog)
						{
							Game1.exitActiveMenu();
						}
						int currentCommand = @event.CurrentCommand;
						@event.CurrentCommand = currentCommand + 1;
						return;
					}
					if (Game1.activeClickableMenu == null)
					{
						Game1.activeClickableMenu = new ReadyCheckDialog(gateId, false, null, null);
						return;
					}
				}
				else
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
			}

			// Token: 0x06003C4D RID: 15437 RVA: 0x002E988D File Offset: 0x002E7A8D
			public static void RequestMovieEnd(Event @event, string[] args, EventContext context)
			{
				Game1.player.team.requestMovieEndEvent.Fire(Game1.player.UniqueMultiplayerID);
			}

			// Token: 0x06003C4E RID: 15438 RVA: 0x002E98B0 File Offset: 0x002E7AB0
			public static void RestoreStashedItem(Event @event, string[] args, EventContext context)
			{
				Game1.player.TemporaryItem = null;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C4F RID: 15439 RVA: 0x002E98D8 File Offset: 0x002E7AD8
			public static void AdvancedMove(Event @event, string[] args, EventContext context)
			{
				@event.setUpAdvancedMove(args, null);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C50 RID: 15440 RVA: 0x002E9900 File Offset: 0x002E7B00
			public static void StopRunning(Event @event, string[] args, EventContext context)
			{
				@event.farmer.setRunning(false, false);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C51 RID: 15441 RVA: 0x002E992C File Offset: 0x002E7B2C
			public static void Eyes(Event @event, string[] args, EventContext context)
			{
				int eyes;
				string error;
				int blinkTimer;
				if (!ArgUtility.TryGetInt(args, 1, out eyes, out error, "int eyes") || !ArgUtility.TryGetInt(args, 2, out blinkTimer, out error, "int blinkTimer"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.farmer.currentEyes = eyes;
				@event.farmer.blinkTimer = blinkTimer;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C52 RID: 15442 RVA: 0x002E9990 File Offset: 0x002E7B90
			[OtherNames(new string[]
			{
				"mailReceived"
			})]
			public static void AddMailReceived(Event @event, string[] args, EventContext context)
			{
				string mailId;
				string error;
				bool add;
				if (!ArgUtility.TryGet(args, 1, out mailId, out error, true, "string mailId") || !ArgUtility.TryGetOptionalBool(args, 2, out add, out error, true, "bool add"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.mailReceived.Toggle(mailId, add);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C53 RID: 15443 RVA: 0x002E99F0 File Offset: 0x002E7BF0
			public static void AddWorldState(Event @event, string[] args, EventContext context)
			{
				string worldStateId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out worldStateId, out error, true, "string worldStateId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.worldStateIDs.Add(worldStateId);
				Game1.netWorldState.Value.addWorldStateID(worldStateId);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C54 RID: 15444 RVA: 0x002E9A48 File Offset: 0x002E7C48
			public static void Fade(Event @event, string[] args, EventContext context)
			{
				string option = ArgUtility.Get(args, 1, null, true);
				if (option == "unfade")
				{
					Game1.fadeIn = false;
					Game1.fadeToBlack = false;
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					return;
				}
				Game1.fadeToBlack = true;
				Game1.fadeIn = true;
				if (Game1.fadeToBlackAlpha >= 0.97f)
				{
					if (option == null)
					{
						Game1.fadeIn = false;
					}
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
			}

			// Token: 0x06003C55 RID: 15445 RVA: 0x002E9ABC File Offset: 0x002E7CBC
			public static void ChangeMapTile(Event @event, string[] args, EventContext context)
			{
				string layerId;
				string error;
				Point tilePos;
				int newTileIndex;
				if (!ArgUtility.TryGet(args, 1, out layerId, out error, true, "string layerId") || !ArgUtility.TryGetPoint(args, 2, out tilePos, out error, "Point tilePos") || !ArgUtility.TryGetInt(args, 4, out newTileIndex, out error, "int newTileIndex"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Layer layer = context.Location.map.GetLayer(layerId);
				if (layer == null)
				{
					context.LogErrorAndSkip("the '" + context.Location.NameOrUniqueName + "' location doesn't have required map layer " + layerId, false);
					return;
				}
				int tileX = @event.OffsetTileX(tilePos.X);
				int tileY = @event.OffsetTileY(tilePos.Y);
				Tile tile = layer.Tiles[tileX, tileY];
				if (tile == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 3);
					defaultInterpolatedStringHandler.AppendLiteral("the '");
					defaultInterpolatedStringHandler.AppendFormatted(context.Location.NameOrUniqueName);
					defaultInterpolatedStringHandler.AppendLiteral("' location doesn't have required tile (");
					defaultInterpolatedStringHandler.AppendFormatted<int>(tilePos.X);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(tilePos.Y);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					string str = defaultInterpolatedStringHandler.ToStringAndClear();
					string str2;
					if (tileX == tilePos.X && tileY == tilePos.Y)
					{
						str2 = "";
					}
					else
					{
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
						defaultInterpolatedStringHandler.AppendLiteral(" (adjusted to (");
						defaultInterpolatedStringHandler.AppendFormatted<int>(tileX);
						defaultInterpolatedStringHandler.AppendLiteral(", ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(tileY);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						str2 = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					context.LogErrorAndSkip(str + str2 + " on layer " + layerId, false);
					return;
				}
				tile.TileIndex = newTileIndex;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C56 RID: 15446 RVA: 0x002E9C74 File Offset: 0x002E7E74
			public static void ChangeSprite(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string spriteSuffix;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetOptional(args, 2, out spriteSuffix, out error, null, true, "string spriteSuffix"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (spriteSuffix != null)
				{
					actor.spriteOverridden = true;
					actor.Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(actor.Name) + "_" + spriteSuffix, true);
				}
				else
				{
					actor.spriteOverridden = false;
					actor.ChooseAppearance(null);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C57 RID: 15447 RVA: 0x002E9D2C File Offset: 0x002E7F2C
			public static void WaitForAllStationary(Event @event, string[] args, EventContext context)
			{
				List<NPCController> npcControllers = @event.npcControllers;
				bool anyMoving = npcControllers != null && npcControllers.Count > 0;
				if (!anyMoving)
				{
					using (List<NPC>.Enumerator enumerator = @event.actors.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.isMoving())
							{
								anyMoving = true;
								break;
							}
						}
					}
				}
				if (!anyMoving)
				{
					using (List<Farmer>.Enumerator enumerator2 = @event.farmerActors.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.isMoving())
							{
								anyMoving = true;
								break;
							}
						}
					}
				}
				if (!anyMoving)
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
			}

			// Token: 0x06003C58 RID: 15448 RVA: 0x002E9DF8 File Offset: 0x002E7FF8
			public static void ProceedPosition(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Character character = @event.getCharacterByName(actorName);
				if (character == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", false);
					return;
				}
				@event.continueAfterMove = true;
				try
				{
					if (character.isMoving())
					{
						List<NPCController> npcControllers = @event.npcControllers;
						if (npcControllers == null || npcControllers.Count != 0)
						{
							goto IL_7B;
						}
					}
					character.Halt();
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					IL_7B:;
				}
				catch
				{
					int currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
				}
			}

			// Token: 0x06003C59 RID: 15449 RVA: 0x002E9EA8 File Offset: 0x002E80A8
			public static void ChangePortrait(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string portraitSuffix;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetOptional(args, 2, out portraitSuffix, out error, null, true, "string portraitSuffix"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC i = @event.getActorByName(actorName, out isOptionalNpc, false) ?? Game1.getCharacterFromName(actorName, true, false);
				if (i == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (portraitSuffix != null)
				{
					i.portraitOverridden = true;
					i.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(i.Name) + "_" + portraitSuffix);
				}
				else
				{
					i.portraitOverridden = false;
					i.ChooseAppearance(null);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C5A RID: 15450 RVA: 0x002E9F70 File Offset: 0x002E8170
			public static void ChangeYSourceRectOffset(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				int ySourceRectOffset;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out ySourceRectOffset, out error, "int ySourceRectOffset"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (i == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				i.ySourceRectOffset = ySourceRectOffset;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C5B RID: 15451 RVA: 0x002E9FEC File Offset: 0x002E81EC
			public static void ChangeName(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string newName;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out newName, out error, true, "string newName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (i == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				i.displayName = newName;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C5C RID: 15452 RVA: 0x002EA068 File Offset: 0x002E8268
			public static void TranslateName(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string translationKey;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out translationKey, out error, true, "string translationKey"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (i == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				i.displayName = Game1.content.LoadString(translationKey);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C5D RID: 15453 RVA: 0x002EA0F0 File Offset: 0x002E82F0
			public static void ReplaceWithClone(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				@event.actors.Remove(actor);
				List<NPC> actors = @event.actors;
				NPC npc = new NPC(actor.Sprite.Clone(), actor.Position, actor.FacingDirection, actor.Name, null);
				npc.Birthday_Day = actor.Birthday_Day;
				npc.Birthday_Season = actor.Birthday_Season;
				npc.Gender = actor.Gender;
				npc.Portrait = actor.Portrait;
				npc.EventActor = true;
				npc.displayName = actor.displayName;
				npc.drawOffset = actor.drawOffset;
				npc.TemporaryDialogue = new Stack<Dialogue>(from p in actor.CurrentDialogue
				select new Dialogue(p));
				actors.Add(npc);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C5E RID: 15454 RVA: 0x002EA210 File Offset: 0x002E8410
			public static void PlayFramesAhead(Event @event, string[] args, EventContext context)
			{
				int framesToSkip;
				string error;
				if (!ArgUtility.TryGetInt(args, 1, out framesToSkip, out error, "int framesToSkip"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				for (int i = 0; i < framesToSkip; i++)
				{
					@event.Update(context.Location, context.Time);
				}
			}

			// Token: 0x06003C5F RID: 15455 RVA: 0x002EA268 File Offset: 0x002E8468
			public static void ShowKissFrame(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				bool flip;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetOptionalBool(args, 2, out flip, out error, false, "bool flip"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				CharacterData data = actor.GetData();
				int spouseFrame = (data != null) ? data.KissSpriteIndex : 28;
				bool facingRight = data == null || data.KissSpriteFacingRight;
				if (flip)
				{
					facingRight = !facingRight;
				}
				actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(spouseFrame, 1000, false, facingRight, null, false)
				});
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C60 RID: 15456 RVA: 0x002EA334 File Offset: 0x002E8534
			public static void AddTemporaryActor(Event @event, string[] args, EventContext context)
			{
				string spriteAssetName;
				string error;
				Point spriteSize;
				Vector2 tile;
				int facingDirection;
				bool isBreather;
				string typeOrDisplayName;
				string overrideName;
				if (!ArgUtility.TryGet(args, 1, out spriteAssetName, out error, true, "string spriteAssetName") || !ArgUtility.TryGetPoint(args, 2, out spriteSize, out error, "Point spriteSize") || !ArgUtility.TryGetVector2(args, 4, out tile, out error, false, "Vector2 tile") || !ArgUtility.TryGetDirection(args, 6, out facingDirection, out error, "int facingDirection") || !ArgUtility.TryGetOptionalBool(args, 7, out isBreather, out error, true, "bool isBreather") || !ArgUtility.TryGetOptional(args, 8, out typeOrDisplayName, out error, null, true, "string typeOrDisplayName") || !ArgUtility.TryGetOptional(args, 9, out overrideName, out error, null, false, "string overrideName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				string textureLocation = "Characters\\";
				bool hasValidTypeKey = true;
				string a = (typeOrDisplayName != null) ? typeOrDisplayName.ToLower() : null;
				if (!(a == "animal"))
				{
					if (!(a == "monster"))
					{
						if (!(a == "character"))
						{
							hasValidTypeKey = false;
						}
					}
					else
					{
						textureLocation = "Characters\\Monsters\\";
					}
				}
				else
				{
					textureLocation = "Animals\\";
				}
				string fullSpriteAssetName = textureLocation + spriteAssetName;
				if (!Game1.content.DoesAssetExist<Texture2D>(fullSpriteAssetName))
				{
					string newSpriteAssetName = spriteAssetName.Replace('_', ' ');
					string newFullSpriteAssetName = textureLocation + newSpriteAssetName;
					if (newSpriteAssetName != spriteAssetName && Game1.content.DoesAssetExist<Texture2D>(newFullSpriteAssetName))
					{
						spriteAssetName = newSpriteAssetName;
						fullSpriteAssetName = newFullSpriteAssetName;
					}
				}
				NPC i = new NPC(new AnimatedSprite(@event.festivalContent, fullSpriteAssetName, 0, spriteSize.X, spriteSize.Y), @event.OffsetPosition(tile * 64f), facingDirection, spriteAssetName, @event.festivalContent);
				i.AllowDynamicAppearance = false;
				i.Breather = isBreather;
				i.HideShadow = (i.Sprite.SpriteWidth >= 32);
				i.TemporaryDialogue = new Stack<Dialogue>();
				if (!hasValidTypeKey && typeOrDisplayName != null)
				{
					i.displayName = typeOrDisplayName;
				}
				Dialogue dialogue;
				if (@event.isFestival && @event.TryGetFestivalDialogueForYear(i, i.Name, out dialogue))
				{
					i.CurrentDialogue.Push(dialogue);
				}
				if (overrideName != null)
				{
					i.Name = overrideName;
				}
				i.EventActor = true;
				@event.actors.Add(i);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C61 RID: 15457 RVA: 0x002EA55C File Offset: 0x002E875C
			public static void ChangeToTemporaryMap(Event @event, string[] args, EventContext context)
			{
				string mapName;
				string error;
				bool shouldPan;
				if (!ArgUtility.TryGet(args, 1, out mapName, out error, true, "string mapName") || !ArgUtility.TryGetOptionalBool(args, 2, out shouldPan, out error, true, "bool shouldPan"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.temporaryLocation = ((mapName == "Town") ? new Town("Maps\\Town", "Temp") : ((@event.isFestival && mapName.Contains("Town")) ? new Town("Maps\\" + mapName, "Temp") : new GameLocation("Maps\\" + mapName, "Temp")));
				@event.temporaryLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
				Event e = Game1.currentLocation.currentEvent;
				Game1.currentLocation.cleanupBeforePlayerExit();
				Game1.currentLocation.currentEvent = null;
				Game1.currentLightSources.Clear();
				Game1.currentLocation = @event.temporaryLocation;
				Game1.currentLocation.resetForPlayerEntry();
				Game1.currentLocation.UpdateMapSeats();
				Game1.currentLocation.currentEvent = e;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				Game1.player.currentLocation = Game1.currentLocation;
				@event.farmer.currentLocation = Game1.currentLocation;
				Game1.currentLocation.ResetForEvent(@event);
				if (shouldPan)
				{
					Game1.panScreen(0, 0);
				}
			}

			// Token: 0x06003C62 RID: 15458 RVA: 0x002EA6AC File Offset: 0x002E88AC
			public static void PositionOffset(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				Point offset;
				bool continueImmediately;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetPoint(args, 2, out offset, out error, "Point offset") || !ArgUtility.TryGetOptionalBool(args, 4, out continueImmediately, out error, false, "bool continueImmediately"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.position.X += (float)offset.X;
						f.position.Y += (float)offset.Y;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					i.position.X += (float)offset.X;
					i.position.Y += (float)offset.Y;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				if (continueImmediately)
				{
					@event.Update(context.Location, context.Time);
				}
			}

			// Token: 0x06003C63 RID: 15459 RVA: 0x002EA7D0 File Offset: 0x002E89D0
			public static void Question(Event @event, string[] args, EventContext context)
			{
				string dialogueKey;
				string error;
				string rawQuestionsAndAnswers;
				if (!ArgUtility.TryGet(args, 1, out dialogueKey, out error, true, "string dialogueKey") || !ArgUtility.TryGet(args, 2, out rawQuestionsAndAnswers, out error, true, "string rawQuestionsAndAnswers"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!Game1.isQuestion && Game1.activeClickableMenu == null)
				{
					string[] questionAndAnswers = LegacyShims.SplitAndTrim(rawQuestionsAndAnswers, '#', StringSplitOptions.None);
					string question = questionAndAnswers[0];
					Response[] answers = new Response[questionAndAnswers.Length - 1];
					for (int i = 1; i < questionAndAnswers.Length; i++)
					{
						answers[i - 1] = new Response((i - 1).ToString(), questionAndAnswers[i]);
					}
					Game1.currentLocation.createQuestionDialogue(question, answers, dialogueKey);
				}
			}

			// Token: 0x06003C64 RID: 15460 RVA: 0x002EA874 File Offset: 0x002E8A74
			public static void QuickQuestion(Event @event, string[] args, EventContext context)
			{
				if (!Game1.isQuestion && Game1.activeClickableMenu == null)
				{
					string currentCommand = @event.GetCurrentCommand();
					string[] questionAndAnswerSplit = LegacyShims.SplitAndTrim(LegacyShims.SplitAndTrim(currentCommand.Substring(currentCommand.IndexOf(' ') + 1), "(break)", StringSplitOptions.None)[0], '#', StringSplitOptions.None);
					string question = questionAndAnswerSplit[0];
					Response[] answers = new Response[questionAndAnswerSplit.Length - 1];
					for (int i = 1; i < questionAndAnswerSplit.Length; i++)
					{
						answers[i - 1] = new Response((i - 1).ToString(), questionAndAnswerSplit[i]);
					}
					Game1.currentLocation.createQuestionDialogue(question, answers, "quickQuestion");
				}
			}

			// Token: 0x06003C65 RID: 15461 RVA: 0x002EA904 File Offset: 0x002E8B04
			public static void DrawOffset(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				Vector2 offset;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetVector2(args, 2, out offset, out error, true, "Vector2 offset"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc = false;
				int farmerNumber;
				Character character = @event.IsFarmerActorId(actorName, out farmerNumber) ? @event.GetFarmerActor(farmerNumber) : @event.getActorByName(actorName, out isOptionalNpc, false);
				if (character == null)
				{
					context.LogErrorAndSkip("no actor found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				character.drawOffset = offset * 4f;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C66 RID: 15462 RVA: 0x002EA9A4 File Offset: 0x002E8BA4
			public static void HideShadow(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				bool hideShadow;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetBool(args, 2, out hideShadow, out error, "bool hideShadow"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				bool isOptionalNpc;
				NPC character = @event.getActorByName(actorName, out isOptionalNpc, false);
				if (character == null)
				{
					context.LogErrorAndSkip("no actor found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				character.HideShadow = hideShadow;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C67 RID: 15463 RVA: 0x002EAA20 File Offset: 0x002E8C20
			public static void AnimateHeight(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				string rawHeight;
				string rawGravity;
				string rawVelocity;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGet(args, 2, out rawHeight, out error, true, "string rawHeight") || !ArgUtility.TryGet(args, 3, out rawGravity, out error, true, "string rawGravity") || !ArgUtility.TryGet(args, 4, out rawVelocity, out error, true, "string rawVelocity"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int? height = null;
				float? jumpGravity = null;
				float? jumpVelocity = null;
				if (rawHeight != "keep")
				{
					int parsed;
					if (!int.TryParse(rawHeight, out parsed))
					{
						context.LogErrorAndSkip("required index 2 must be 'keep' or an integer height", false);
						return;
					}
					height = new int?(parsed);
				}
				if (rawGravity != "keep")
				{
					float parsed2;
					if (!float.TryParse(rawGravity, out parsed2))
					{
						context.LogErrorAndSkip("required index 3 must be 'keep' or a float gravity value", false);
						return;
					}
					jumpGravity = new float?(parsed2);
				}
				if (rawVelocity != "keep")
				{
					float parsed3;
					if (!float.TryParse(rawVelocity, out parsed3))
					{
						context.LogErrorAndSkip("required index 4 must be 'keep' or a float velocity value", false);
						return;
					}
					jumpVelocity = new float?(parsed3);
				}
				bool isOptionalNpc = false;
				int farmerNumber;
				Character character = @event.IsFarmerActorId(actorName, out farmerNumber) ? @event.GetFarmerActor(farmerNumber) : @event.getActorByName(actorName, out isOptionalNpc, false);
				if (character == null)
				{
					context.LogErrorAndSkip("no actor found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (height != null)
				{
					character.yJumpOffset = -height.Value;
				}
				if (jumpGravity != null)
				{
					character.yJumpGravity = jumpGravity.Value;
				}
				if (jumpVelocity != null)
				{
					character.yJumpVelocity = jumpVelocity.Value;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C68 RID: 15464 RVA: 0x002EABBC File Offset: 0x002E8DBC
			public static void Jump(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				float jumpV;
				bool noSound;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName") || !ArgUtility.TryGetOptionalFloat(args, 2, out jumpV, out error, 8f, "float jumpV") || !ArgUtility.TryGetOptionalBool(args, 3, out noSound, out error, false, "bool noSound"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer farmerActor = @event.GetFarmerActor(farmerNumber);
					if (farmerActor != null)
					{
						farmerActor.jump(jumpV);
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC actor = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (actor == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					if (noSound)
					{
						actor.jumpWithoutSound(jumpV);
					}
					else
					{
						actor.jump(jumpV);
					}
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
				@event.Update(context.Location, context.Time);
			}

			// Token: 0x06003C69 RID: 15465 RVA: 0x002EAC94 File Offset: 0x002E8E94
			public static void FarmerEat(Event @event, string[] args, EventContext context)
			{
				string itemId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out itemId, out error, true, "string itemId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Object toEat = ItemRegistry.Create<Object>("(O)" + itemId, 1, 0, false);
				@event.farmer.eatObject(toEat, true);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C6A RID: 15466 RVA: 0x002EACF0 File Offset: 0x002E8EF0
			public static void SpriteText(Event @event, string[] args, EventContext context)
			{
				int colorIndex;
				string error;
				string text;
				if (!ArgUtility.TryGetInt(args, 1, out colorIndex, out error, "int colorIndex") || !ArgUtility.TryGet(args, 2, out text, out error, true, "string text"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				@event.int_useMeForAnything2 = colorIndex;
				@event.float_useMeForAnything += (float)context.Time.ElapsedGameTime.Milliseconds;
				if (@event.float_useMeForAnything > 80f)
				{
					if (@event.int_useMeForAnything >= text.Length)
					{
						if (@event.float_useMeForAnything >= 2500f)
						{
							@event.int_useMeForAnything = 0;
							@event.float_useMeForAnything = 0f;
							@event.spriteTextToDraw = "";
							int currentCommand = @event.CurrentCommand;
							@event.CurrentCommand = currentCommand + 1;
						}
					}
					else
					{
						@event.int_useMeForAnything++;
						@event.float_useMeForAnything = 0f;
						Game1.playSound("dialogueCharacter", null);
					}
				}
				@event.spriteTextToDraw = text;
			}

			// Token: 0x06003C6B RID: 15467 RVA: 0x002EADE4 File Offset: 0x002E8FE4
			public static void IgnoreCollisions(Event @event, string[] args, EventContext context)
			{
				string actorName;
				string error;
				if (!ArgUtility.TryGet(args, 1, out actorName, out error, true, "string actorName"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int farmerNumber;
				if (@event.IsFarmerActorId(actorName, out farmerNumber))
				{
					Farmer f = @event.GetFarmerActor(farmerNumber);
					if (f != null)
					{
						f.ignoreCollisions = true;
					}
				}
				else
				{
					bool isOptionalNpc;
					NPC i = @event.getActorByName(actorName, out isOptionalNpc, false);
					if (i == null)
					{
						context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
						return;
					}
					i.isCharging = true;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C6C RID: 15468 RVA: 0x002EAE70 File Offset: 0x002E9070
			public static void ScreenFlash(Event @event, string[] args, EventContext context)
			{
				float flashAlpha;
				string error;
				if (!ArgUtility.TryGetFloat(args, 1, out flashAlpha, out error, "float flashAlpha"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.flashAlpha = flashAlpha;
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C6D RID: 15469 RVA: 0x002EAEB0 File Offset: 0x002E90B0
			public static void GrandpaCandles(Event @event, string[] args, EventContext context)
			{
				int candles = Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore());
				Game1.getFarm().grandpaScore.Value = candles;
				for (int i = 0; i < candles; i++)
				{
					DelayedAction.playSoundAfterDelay("fireball", 100 * i, null, null, -1, false);
				}
				Game1.getFarm().addGrandpaCandles();
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C6E RID: 15470 RVA: 0x002EAF18 File Offset: 0x002E9118
			public static void GrandpaEvaluation2(Event @event, string[] args, EventContext context)
			{
				switch (Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore()))
				{
				case 1:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1306") + "\"");
					break;
				case 2:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1307") + "\"");
					break;
				case 3:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1308") + "\"");
					break;
				case 4:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1309") + "\"");
					break;
				}
				Game1.player.eventsSeen.Remove("2146991");
			}

			// Token: 0x06003C6F RID: 15471 RVA: 0x002EAFF8 File Offset: 0x002E91F8
			public static void GrandpaEvaluation(Event @event, string[] args, EventContext context)
			{
				switch (Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore()))
				{
				case 1:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1315") + "\"");
					return;
				case 2:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1316") + "\"");
					return;
				case 3:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1317") + "\"");
					return;
				case 4:
					@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1318") + "\"");
					return;
				default:
					return;
				}
			}

			// Token: 0x06003C70 RID: 15472 RVA: 0x002EB0BC File Offset: 0x002E92BC
			public static void LoadActors(Event @event, string[] args, EventContext context)
			{
				Event.DefaultCommands.<>c__DisplayClass129_0 CS$<>8__locals1;
				CS$<>8__locals1.@event = @event;
				string layerId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out layerId, out error, true, "string layerId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				GameLocation temporaryLocation = CS$<>8__locals1.@event.temporaryLocation;
				Layer layer = (temporaryLocation != null) ? temporaryLocation.map.GetLayer(layerId) : null;
				if (layer == null)
				{
					context.LogErrorAndSkip("the '" + context.Location.NameOrUniqueName + "' location doesn't have required map layer " + layerId, false);
					return;
				}
				CS$<>8__locals1.@event.actors.Clear();
				List<NPCController> npcControllers = CS$<>8__locals1.@event.npcControllers;
				if (npcControllers != null)
				{
					npcControllers.Clear();
				}
				Dictionary<int, string> actorNamesByIndex = new Dictionary<int, string>();
				foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
				{
					int index = entry.Value.FestivalVanillaActorIndex;
					if (index >= 0 && !actorNamesByIndex.TryAdd(index, entry.Key))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(110, 2);
						defaultInterpolatedStringHandler.AppendLiteral("NPC '");
						defaultInterpolatedStringHandler.AppendFormatted(entry.Key);
						defaultInterpolatedStringHandler.AppendLiteral("' has the same festival actor index as '");
						defaultInterpolatedStringHandler.AppendFormatted(actorNamesByIndex[index]);
						defaultInterpolatedStringHandler.AppendLiteral("' in Data/Characters, so it'll be ignored for festival placement.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				HashSet<string> npcNames = new HashSet<string>();
				for (int x = 0; x < layer.LayerWidth; x++)
				{
					for (int y = 0; y < layer.LayerHeight; y++)
					{
						Tile tile = layer.Tiles[x, y];
						if (tile != null)
						{
							int tileIndex = tile.TileIndex;
							int actorIndex = tileIndex / 4;
							int actorFacingDirection = tileIndex % 4;
							string actorName;
							if (actorNamesByIndex.TryGetValue(actorIndex, out actorName) && Game1.getCharacterFromName(actorName, true, false) != null && (!(actorName == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved")))
							{
								CS$<>8__locals1.@event.addActor(actorName, x, y, actorFacingDirection, CS$<>8__locals1.@event.temporaryLocation);
								npcNames.Add(actorName);
							}
						}
					}
				}
				string data;
				string keyName;
				int j;
				if (CS$<>8__locals1.@event.festivalData != null && CS$<>8__locals1.@event.TryGetFestivalDataForYear(layerId + "_additionalCharacters", out data, out keyName))
				{
					string[] array = Event.ParseCommands(data, context.Event.farmer);
					for (j = 0; j < array.Length; j++)
					{
						string[] curArgs = ArgUtility.SplitBySpaceQuoteAware(array[j]);
						string actorName2;
						Point tile2;
						int direction;
						if (!ArgUtility.TryGet(curArgs, 0, out actorName2, out error, true, "string actorName") || !ArgUtility.TryGetPoint(curArgs, 1, out tile2, out error, "Point tile") || !ArgUtility.TryGetDirection(curArgs, 3, out direction, out error, "int direction"))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 3);
							defaultInterpolatedStringHandler.AppendLiteral("'");
							defaultInterpolatedStringHandler.AppendFormatted(keyName);
							defaultInterpolatedStringHandler.AppendLiteral("' festival field has invalid additional character entry '");
							defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", curArgs));
							defaultInterpolatedStringHandler.AppendLiteral("': ");
							defaultInterpolatedStringHandler.AppendFormatted(error);
							context.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						}
						else if (Game1.getCharacterFromName(actorName2, true, false) != null)
						{
							if (!(actorName2 == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
							{
								CS$<>8__locals1.@event.addActor(actorName2, tile2.X, tile2.Y, direction, CS$<>8__locals1.@event.temporaryLocation);
								npcNames.Add(actorName2);
							}
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 3);
							defaultInterpolatedStringHandler.AppendLiteral("'");
							defaultInterpolatedStringHandler.AppendFormatted(keyName);
							defaultInterpolatedStringHandler.AppendLiteral("' festival field has invalid additional character entry '");
							defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", curArgs));
							defaultInterpolatedStringHandler.AppendLiteral("': no NPC found with name '");
							defaultInterpolatedStringHandler.AppendFormatted(actorName2);
							defaultInterpolatedStringHandler.AppendLiteral("'");
							context.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						}
					}
				}
				if (layerId == "Set-Up")
				{
					foreach (string npcName in npcNames)
					{
						NPC npc = Game1.getCharacterFromName(npcName, true, false);
						if (npc.isMarried() && npc.getSpouse() != null && npc.getSpouse().getChildren().Count > 0)
						{
							Farmer spouse = Game1.player;
							if (npc.getSpouse() != null)
							{
								spouse = npc.getSpouse();
							}
							List<Child> children = spouse.getChildren();
							npc = (CS$<>8__locals1.@event.getCharacterByName(npcName) as NPC);
							for (int childIndex = 0; childIndex < children.Count; childIndex++)
							{
								Child child = children[childIndex];
								if (child.Age >= 3)
								{
									Child childActor = new Child(child.Name, child.Gender == Gender.Male, child.darkSkinned.Value, spouse);
									childActor.NetFields.CopyFrom(child.NetFields);
									childActor.Halt();
									Point[] directionOffsets;
									switch (npc.FacingDirection)
									{
									case 0:
										directionOffsets = new Point[]
										{
											new Point(0, 1),
											new Point(-1, 0),
											new Point(1, 0),
											new Point(0, -1)
										};
										break;
									case 1:
										directionOffsets = new Point[]
										{
											new Point(-1, 0),
											new Point(0, 1),
											new Point(0, -1),
											new Point(1, 0)
										};
										break;
									case 2:
										directionOffsets = new Point[]
										{
											new Point(0, -1),
											new Point(1, 0),
											new Point(-1, 0),
											new Point(0, 1)
										};
										break;
									case 3:
										directionOffsets = new Point[]
										{
											new Point(1, 0),
											new Point(0, -1),
											new Point(0, 1),
											new Point(-1, 0)
										};
										break;
									default:
										directionOffsets = new Point[]
										{
											new Point(-1, 0),
											new Point(1, 0),
											new Point(0, -1),
											new Point(0, 1)
										};
										break;
									}
									Point spawnPoint = npc.TilePoint;
									List<Point> pointsToCheck = new List<Point>();
									foreach (Point offset in directionOffsets)
									{
										pointsToCheck.Add(new Point(spawnPoint.X + offset.X, spawnPoint.Y + offset.Y));
									}
									bool foundSpawn = false;
									int iteration = 0;
									while (iteration < 5 && !foundSpawn)
									{
										int currentCheckCount = pointsToCheck.Count;
										for (int i = 0; i < currentCheckCount; i++)
										{
											Point currentPoint = pointsToCheck[0];
											pointsToCheck.RemoveAt(0);
											if (Event.DefaultCommands.<LoadActors>g__IsWalkableTileCheck|129_0(currentPoint, ref CS$<>8__locals1))
											{
												if (Event.DefaultCommands.<LoadActors>g__HasClearanceCheck|129_1(currentPoint, ref CS$<>8__locals1))
												{
													foundSpawn = true;
													spawnPoint = currentPoint;
													break;
												}
												foreach (Point offset2 in directionOffsets)
												{
													pointsToCheck.Add(new Point(currentPoint.X + offset2.X, currentPoint.Y + offset2.Y));
												}
											}
										}
										iteration++;
									}
									if (foundSpawn)
									{
										childActor.setTilePosition(spawnPoint.X, spawnPoint.Y);
										childActor.DefaultPosition = npc.DefaultPosition;
										childActor.faceDirection(npc.FacingDirection);
										childActor.EventActor = true;
										childActor.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(spawnPoint.X * 64, spawnPoint.Y * 64, 64, 64);
										childActor.squareMovementFacingPreference = -1;
										childActor.walkInSquare(3, 3, 2000);
										childActor.controller = null;
										childActor.temporaryController = null;
										CS$<>8__locals1.@event.actors.Add(childActor);
									}
								}
							}
						}
					}
				}
				Event event2 = CS$<>8__locals1.@event;
				j = event2.CurrentCommand;
				event2.CurrentCommand = j + 1;
			}

			// Token: 0x06003C71 RID: 15473 RVA: 0x002EB954 File Offset: 0x002E9B54
			public static void PlayerControl(Event @event, string[] args, EventContext context)
			{
				string sequenceId;
				string error;
				if (!ArgUtility.TryGet(args, 1, out sequenceId, out error, true, "string sequenceId"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!@event.playerControlSequence)
				{
					@event.setUpPlayerControlSequence(sequenceId);
				}
			}

			// Token: 0x06003C72 RID: 15474 RVA: 0x002EB98C File Offset: 0x002E9B8C
			public static void RemoveSprite(Event @event, string[] args, EventContext context)
			{
				Vector2 tile;
				string error;
				if (!ArgUtility.TryGetVector2(args, 1, out tile, out error, true, "Vector2 tile"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Vector2 tilePixel = @event.OffsetPosition(tile * 64f);
				Game1.currentLocation.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.position == tilePixel);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C73 RID: 15475 RVA: 0x002EBA00 File Offset: 0x002E9C00
			public static void Viewport(Event @event, string[] args, EventContext context)
			{
				if (ArgUtility.Get(args, 1, null, true) == "move")
				{
					Point direction;
					string error;
					int duration;
					if (!ArgUtility.TryGetPoint(args, 2, out direction, out error, "Point direction") || !ArgUtility.TryGetInt(args, 4, out duration, out error, "int duration"))
					{
						context.LogErrorAndSkip(error, false);
						return;
					}
					@event.viewportTarget = new Vector3((float)direction.X, (float)direction.Y, (float)duration);
				}
				else
				{
					Point position = Point.Zero;
					string action = null;
					bool shouldFade = false;
					string option = null;
					int test;
					string NPCTarget;
					string error2;
					string error3;
					if (!int.TryParse(args[1], out test) && ArgUtility.TryGet(args, 1, out NPCTarget, out error2, true, "string NPCTarget"))
					{
						if (NPCTarget == "player")
						{
							position = Game1.MasterPlayer.TilePoint;
						}
						else
						{
							position = @event.getActorByName(NPCTarget, false).TilePoint;
						}
						if (!ArgUtility.TryGetOptional(args, 2, out action, out error2, null, true, "action") || !ArgUtility.TryGetOptionalBool(args, (action == "clamp") ? 3 : 2, out shouldFade, out error2, false, "shouldFade") || !ArgUtility.TryGetOptional(args, (action == "clamp") ? 4 : 2, out option, out error2, null, true, "option"))
						{
							context.LogErrorAndSkip(error2, false);
						}
					}
					else if (!ArgUtility.TryGetPoint(args, 1, out position, out error3, "position") || !ArgUtility.TryGetOptional(args, 3, out action, out error3, null, true, "action") || !ArgUtility.TryGetOptionalBool(args, (action == "clamp") ? 4 : 3, out shouldFade, out error3, false, "shouldFade") || !ArgUtility.TryGetOptional(args, (action == "clamp") ? 5 : 4, out option, out error3, null, true, "option"))
					{
						context.LogErrorAndSkip(error3, false);
						return;
					}
					if (@event.aboveMapSprites != null && position.X < 0)
					{
						@event.aboveMapSprites.Clear();
						@event.aboveMapSprites = null;
					}
					Game1.viewportFreeze = true;
					int targetTileX = @event.OffsetTileX(position.X);
					int targetTileY = @event.OffsetTileY(position.Y);
					if (@event.id == "2146991")
					{
						Point grandpaShrinePosition = Game1.getFarm().GetGrandpaShrinePosition();
						targetTileX = grandpaShrinePosition.X;
						targetTileY = grandpaShrinePosition.Y;
					}
					Game1.viewport.X = targetTileX * 64 + 32 - Game1.viewport.Width / 2;
					Game1.viewport.Y = targetTileY * 64 + 32 - Game1.viewport.Height / 2;
					if (Game1.viewport.X > 0 && Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
					{
						Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
					}
					if (Game1.viewport.Y > 0 && Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
					{
						Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
					}
					if (action == "clamp")
					{
						if (Game1.currentLocation.map.DisplayWidth >= Game1.viewport.Width)
						{
							if (Game1.viewport.X + Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
							{
								Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
							}
							if (Game1.viewport.X < 0)
							{
								Game1.viewport.X = 0;
							}
						}
						else
						{
							Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth / 2 - Game1.viewport.Width / 2;
						}
						if (Game1.currentLocation.map.DisplayHeight >= Game1.viewport.Height)
						{
							if (Game1.viewport.Y + Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
							{
								Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height;
							}
						}
						else
						{
							Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight / 2 - Game1.viewport.Height / 2;
						}
						if (Game1.viewport.Y < 0)
						{
							Game1.viewport.Y = 0;
						}
					}
					if (shouldFade)
					{
						Game1.fadeScreenToBlack();
						Game1.fadeToBlackAlpha = 1f;
						Game1.nonWarpFade = true;
					}
					if (option == "unfreeze")
					{
						Game1.viewportFreeze = false;
					}
					if (Game1.gameMode == 2)
					{
						Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
					}
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C74 RID: 15476 RVA: 0x002EBEC8 File Offset: 0x002EA0C8
			public static void BroadcastEvent(Event @event, string[] args, EventContext context)
			{
				bool useLocalFarmer;
				string error;
				if (!ArgUtility.TryGetOptionalBool(args, 1, out useLocalFarmer, out error, false, "bool useLocalFarmer"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (@event.farmer == Game1.player)
				{
					if (@event.id == "558291" || @event.id == "558292")
					{
						useLocalFarmer = true;
					}
					Game1.multiplayer.broadcastEvent(@event, Game1.currentLocation, Game1.player.positionBeforeEvent, useLocalFarmer, false);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C75 RID: 15477 RVA: 0x002EBF50 File Offset: 0x002EA150
			public static void AddConversationTopic(Event @event, string[] args, EventContext context)
			{
				string topicId;
				string error;
				int daysDuration;
				if (!ArgUtility.TryGet(args, 1, out topicId, out error, true, "string topicId") || !ArgUtility.TryGetOptionalInt(args, 2, out daysDuration, out error, 4, "int daysDuration"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				int currentCommand;
				if (@event.isMemory)
				{
					currentCommand = @event.CurrentCommand;
					@event.CurrentCommand = currentCommand + 1;
					return;
				}
				Game1.player.activeDialogueEvents.TryAdd(topicId, daysDuration);
				currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C76 RID: 15478 RVA: 0x002EBFC8 File Offset: 0x002EA1C8
			public static void Dump(Event @event, string[] args, EventContext context)
			{
				string which;
				string error;
				if (!ArgUtility.TryGet(args, 1, out which, out error, true, "string which"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				if (!(which == "girls"))
				{
					if (!(which == "guys"))
					{
						context.LogErrorAndSkip("unknown ID '" + which + "', expected 'girls' or 'guys'", false);
						return;
					}
					Game1.player.activeDialogueEvents["dumped_Guys"] = 7;
					Game1.player.activeDialogueEvents["secondChance_Guys"] = 14;
				}
				else
				{
					Game1.player.activeDialogueEvents["dumped_Girls"] = 7;
					Game1.player.activeDialogueEvents["secondChance_Girls"] = 14;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C77 RID: 15479 RVA: 0x002EC090 File Offset: 0x002EA290
			public static void EventSeen(Event @event, string[] args, EventContext context)
			{
				string eventId;
				string error;
				bool seen;
				if (!ArgUtility.TryGet(args, 1, out eventId, out error, false, "string eventId") || !ArgUtility.TryGetOptionalBool(args, 2, out seen, out error, true, "bool seen"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.eventsSeen.Toggle(eventId, seen);
				if (eventId == @event.id)
				{
					@event.markEventSeen = false;
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C78 RID: 15480 RVA: 0x002EC104 File Offset: 0x002EA304
			public static void QuestionAnswered(Event @event, string[] args, EventContext context)
			{
				string questionId;
				string error;
				bool seen;
				if (!ArgUtility.TryGet(args, 1, out questionId, out error, false, "string questionId") || !ArgUtility.TryGetOptionalBool(args, 2, out seen, out error, true, "bool seen"))
				{
					context.LogErrorAndSkip(error, false);
					return;
				}
				Game1.player.dialogueQuestionsAnswered.Toggle(questionId, seen);
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C79 RID: 15481 RVA: 0x002EC164 File Offset: 0x002EA364
			public static void GainSkill(Event @event, string[] args, EventContext context)
			{
				int whichSkill = Farmer.getSkillNumberFromName(args[1]);
				int level = Convert.ToInt32(args[2]);
				if (Game1.player.GetUnmodifiedSkillLevel(whichSkill) < level)
				{
					Game1.player.setSkillLevel(args[1], level);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C7A RID: 15482 RVA: 0x002EC1B0 File Offset: 0x002EA3B0
			public static void MoveToSoup(Event @event, string[] args, EventContext context)
			{
				if (Game1.year % 2 == 1)
				{
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Gus",
						"false",
						"0",
						"-1",
						"5",
						"0",
						"4",
						"1000"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Jodi",
						"false",
						"0",
						"-2"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Clint",
						"false",
						"0",
						"1",
						"-1",
						"0",
						"0",
						"3",
						"-2",
						"0"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Emily",
						"false",
						"3",
						"0"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Pam",
						"false",
						"0",
						"2",
						"7",
						"0"
					}, null);
				}
				else
				{
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Pierre",
						"false",
						"3",
						"0"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Pam",
						"false",
						"0",
						"2",
						"-4",
						"0",
						"0",
						"1"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Abigail",
						"false",
						"4",
						"0",
						"0",
						"-3",
						"1",
						"4000"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Alex",
						"false",
						"-5",
						"0",
						"0",
						"-1",
						"3",
						"2000"
					}, null);
					@event.setUpAdvancedMove(new string[]
					{
						"",
						"Gus",
						"false",
						"0",
						"-1"
					}, null);
				}
				int currentCommand = @event.CurrentCommand;
				@event.CurrentCommand = currentCommand + 1;
			}

			// Token: 0x06003C7B RID: 15483 RVA: 0x002EC4B8 File Offset: 0x002EA6B8
			[CompilerGenerated]
			internal static bool <LoadActors>g__IsWalkableTileCheck|129_0(Point point, ref Event.DefaultCommands.<>c__DisplayClass129_0 A_1)
			{
				return A_1.@event.temporaryLocation.isTilePassable(new Location(point.X, point.Y), Game1.viewport);
			}

			// Token: 0x06003C7C RID: 15484 RVA: 0x002EC4E0 File Offset: 0x002EA6E0
			[CompilerGenerated]
			internal static bool <LoadActors>g__HasClearanceCheck|129_1(Point point, ref Event.DefaultCommands.<>c__DisplayClass129_0 A_1)
			{
				int clearance = 1;
				for (int x = point.X - clearance; x <= point.X + clearance; x++)
				{
					for (int y = point.Y - clearance; y <= point.Y + clearance; y++)
					{
						if (A_1.@event.temporaryLocation.IsTileBlockedBy(new Vector2((float)x, (float)y), CollisionMask.All, CollisionMask.None, false))
						{
							return false;
						}
						foreach (NPC actor in A_1.@event.actors)
						{
							if (!(actor is Child))
							{
								Point tile = actor.TilePoint;
								if (tile.X == x && tile.Y == y)
								{
									return false;
								}
							}
						}
					}
				}
				return true;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using StardewValley.Delegates;
using StardewValley.GameData;
using StardewValley.Logging;
using StardewValley.Network.NetEvents;
using StardewValley.SpecialOrders;

namespace StardewValley.Triggers
{
	// Token: 0x02000126 RID: 294
	public static class TriggerActionManager
	{
		// Token: 0x060017E1 RID: 6113 RVA: 0x00112708 File Offset: 0x00110908
		public static void RegisterTrigger(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				Game1.log.Error("Can't register an empty trigger type for Data/Triggers.", null);
				return;
			}
			TriggerActionManager.ValidTriggerTypes.Add(name);
			Game1.log.Verbose("Registered trigger type for Data/Triggers: " + name + ".");
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x00112754 File Offset: 0x00110954
		public static void RegisterAction(string name, TriggerActionDelegate action)
		{
			if (TriggerActionManager.ActionHandlers.TryAdd(name, action))
			{
				Game1.log.Verbose("Registered trigger action handler '" + name + "'.");
				return;
			}
			Game1.log.Warn("Can't add trigger action handler '" + name + "' because that name is already registered.");
		}

		// Token: 0x060017E3 RID: 6115 RVA: 0x001127A4 File Offset: 0x001109A4
		public static void Raise(string trigger, object[] triggerArgs = null, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null)
		{
			string actualTrigger;
			if (TriggerActionManager.ValidTriggerTypes.TryGetValue(trigger, out actualTrigger))
			{
				trigger = actualTrigger;
				triggerArgs = (triggerArgs ?? LegacyShims.EmptyArray<object>());
				foreach (CachedTriggerAction entry in TriggerActionManager.GetActionsForTrigger(trigger))
				{
					TriggerActionManager.TryRunActions(entry, trigger, triggerArgs, location, player, targetItem, inputItem);
				}
				return;
			}
			Game1.log.Error("Can't raise unknown trigger type '" + trigger + "'.", null);
		}

		// Token: 0x060017E4 RID: 6116 RVA: 0x00112834 File Offset: 0x00110A34
		public static CachedAction ParseAction(string action)
		{
			if (string.IsNullOrWhiteSpace(action))
			{
				return TriggerActionManager.NullAction;
			}
			action = action.Trim();
			CachedAction parsed;
			if (!TriggerActionManager.ActionCache.TryGetValue(action, out parsed))
			{
				string[] args = ArgUtility.SplitBySpaceQuoteAware(action);
				string actionKey = args[0];
				TriggerActionDelegate handler;
				CachedAction cachedAction;
				if (!TriggerActionManager.TryGetActionHandler(actionKey, out handler))
				{
					string[] args2 = args;
					TriggerActionDelegate handler2 = TriggerActionManager.NullAction.Handler;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
					defaultInterpolatedStringHandler.AppendLiteral("unknown action '");
					defaultInterpolatedStringHandler.AppendFormatted(actionKey);
					defaultInterpolatedStringHandler.AppendLiteral("' ignored (expected one of '");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", TriggerActionManager.ActionHandlers.Keys.OrderBy((string p) => p, StringComparer.OrdinalIgnoreCase)));
					defaultInterpolatedStringHandler.AppendLiteral("')");
					cachedAction = new CachedAction(args2, handler2, defaultInterpolatedStringHandler.ToStringAndClear(), true);
				}
				else
				{
					cachedAction = new CachedAction(args, handler, null, false);
				}
				parsed = cachedAction;
				TriggerActionManager.ActionCache[action] = parsed;
			}
			return parsed;
		}

		// Token: 0x060017E5 RID: 6117 RVA: 0x0011292C File Offset: 0x00110B2C
		public static bool TryValidateActionExists(string action, out string error)
		{
			CachedAction parsed = TriggerActionManager.ParseAction(action);
			error = parsed.Error;
			return error == null;
		}

		// Token: 0x060017E6 RID: 6118 RVA: 0x0011294D File Offset: 0x00110B4D
		public static bool TryRunAction(string action, out string error, out Exception exception)
		{
			bool flag = TriggerActionManager.TryRunAction(TriggerActionManager.ParseAction(action), TriggerActionManager.EmptyManualContext, out error, out exception);
			if (!flag && string.IsNullOrWhiteSpace(error))
			{
				error = ((exception != null) ? "an unhandled error occurred" : "the action failed but didn't provide an error message");
			}
			return flag;
		}

		// Token: 0x060017E7 RID: 6119 RVA: 0x00112980 File Offset: 0x00110B80
		public static bool TryRunAction(string action, string trigger, object[] triggerArgs, out string error, out Exception exception)
		{
			if (trigger == null)
			{
				throw new ArgumentNullException("trigger");
			}
			if (triggerArgs == null)
			{
				throw new ArgumentNullException("triggerArgs");
			}
			TriggerActionContext context = (trigger == "Manual" && triggerArgs.Length == 0) ? TriggerActionManager.EmptyManualContext : new TriggerActionContext(trigger, triggerArgs, null, null);
			return TriggerActionManager.TryRunAction(TriggerActionManager.ParseAction(action), context, out error, out exception);
		}

		// Token: 0x060017E8 RID: 6120 RVA: 0x001129DC File Offset: 0x00110BDC
		public static bool TryRunAction(CachedAction action, TriggerActionContext context, out string error, out Exception exception)
		{
			if (action == null)
			{
				error = null;
				exception = null;
				return true;
			}
			if (action.Error != null)
			{
				error = action.Error;
				exception = null;
				return false;
			}
			bool result;
			try
			{
				action.Handler(action.Args, context, out error);
				if (error != null)
				{
					exception = null;
					result = false;
				}
				else
				{
					exception = null;
					result = true;
				}
			}
			catch (Exception ex)
			{
				error = "an unexpected error occurred";
				exception = ex;
				result = false;
			}
			return result;
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x00112A50 File Offset: 0x00110C50
		public static bool TryRunActions(CachedTriggerAction entry, string trigger, object[] triggerArgs = null, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null)
		{
			TriggerActionData data = entry.Data;
			if (Game1.player.triggerActionsRun.Contains(data.Id))
			{
				return false;
			}
			if (data.SkipPermanentlyCondition != null && GameStateQuery.CheckConditions(data.SkipPermanentlyCondition, null, null, null, null, null, null))
			{
				Game1.player.triggerActionsRun.Add(data.Id);
				return false;
			}
			if (!TriggerActionManager.CanApplyIgnoringRun(data, location, player, targetItem, inputItem))
			{
				return false;
			}
			TriggerActionContext context = new TriggerActionContext(trigger, triggerArgs, data, null);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			foreach (CachedAction action in entry.Actions)
			{
				string error;
				Exception ex;
				if (!TriggerActionManager.TryRunAction(action, context, out error, out ex))
				{
					IGameLogger log = Game1.log;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Trigger action '");
					defaultInterpolatedStringHandler.AppendFormatted(data.Id);
					defaultInterpolatedStringHandler.AppendLiteral("' has action string '");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", action.Args));
					defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be applied: ");
					defaultInterpolatedStringHandler.AppendFormatted(error);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				}
			}
			if (data.MarkActionApplied)
			{
				Game1.player.triggerActionsRun.Add(data.Id);
			}
			IGameLogger log2 = Game1.log;
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Applied trigger action '");
			defaultInterpolatedStringHandler.AppendFormatted(data.Id);
			defaultInterpolatedStringHandler.AppendLiteral("' with actions [");
			defaultInterpolatedStringHandler.AppendFormatted(string.Join("], [", entry.ActionStrings));
			defaultInterpolatedStringHandler.AppendLiteral("].");
			log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			return true;
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x00112BF7 File Offset: 0x00110DF7
		public static bool TryGetActionHandler(string key, out TriggerActionDelegate handler)
		{
			return TriggerActionManager.ActionHandlers.TryGetValue(key, out handler);
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x00112C08 File Offset: 0x00110E08
		public static IReadOnlyList<CachedTriggerAction> GetActionsForTrigger(string trigger)
		{
			List<CachedTriggerAction> cached;
			if (TriggerActionManager.GetActionsByTrigger().TryGetValue(trigger, out cached))
			{
				return cached;
			}
			return LegacyShims.EmptyArray<CachedTriggerAction>();
		}

		// Token: 0x060017EC RID: 6124 RVA: 0x00112C2B File Offset: 0x00110E2B
		public static bool CanApply(TriggerActionData action, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null)
		{
			return !Game1.player.triggerActionsRun.Contains(action.Id) && TriggerActionManager.CanApplyIgnoringRun(action, location, player, targetItem, inputItem);
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x00112C51 File Offset: 0x00110E51
		public static void ResetDataCache()
		{
			TriggerActionManager.ActionCache.Clear();
			TriggerActionManager.ActionsByTrigger.Clear();
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x00112C68 File Offset: 0x00110E68
		static TriggerActionManager()
		{
			foreach (MethodInfo method in typeof(TriggerActionManager.DefaultActions).GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				TriggerActionDelegate action = (TriggerActionDelegate)Delegate.CreateDelegate(typeof(TriggerActionDelegate), method);
				TriggerActionManager.ActionHandlers.Add(method.Name, action);
			}
			TriggerActionManager.NullAction = new CachedAction(LegacyShims.EmptyArray<string>(), TriggerActionManager.ActionHandlers["Null"], null, true);
		}

		// Token: 0x060017EF RID: 6127 RVA: 0x00112D64 File Offset: 0x00110F64
		private static Dictionary<string, List<CachedTriggerAction>> GetActionsByTrigger()
		{
			Dictionary<string, List<CachedTriggerAction>> actionsByTrigger = TriggerActionManager.ActionsByTrigger;
			if (actionsByTrigger.Count == 0)
			{
				foreach (string triggerType in TriggerActionManager.ValidTriggerTypes)
				{
					actionsByTrigger[triggerType] = new List<CachedTriggerAction>();
				}
				HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				List<CachedAction> actions = new List<CachedAction>();
				foreach (TriggerActionData data in DataLoader.TriggerActions(Game1.content))
				{
					if (string.IsNullOrWhiteSpace(data.Id))
					{
						Game1.log.Error("Trigger action has no ID field and will be ignored.", null);
					}
					else if (string.IsNullOrWhiteSpace(data.Trigger))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Trigger action '");
						defaultInterpolatedStringHandler.AppendFormatted(data.Id);
						defaultInterpolatedStringHandler.AppendLiteral("' has no trigger; expected one of '");
						defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", TriggerActionManager.ValidTriggerTypes));
						defaultInterpolatedStringHandler.AppendLiteral("'.");
						log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					else
					{
						if (string.IsNullOrWhiteSpace(data.Action))
						{
							List<string> actions2 = data.Actions;
							if (actions2 == null || actions2.Count <= 0)
							{
								Game1.log.Error("Trigger action '" + data.Id + "' has no defined actions.", null);
								continue;
							}
						}
						if (!seenIds.Add(data.Id))
						{
							Game1.log.Error("Trigger action '" + data.Id + "' has a duplicate ID. Only the first instance will be used.", null);
						}
						else
						{
							actions.Clear();
							if (data.Action != null)
							{
								CachedAction parsed = TriggerActionManager.ParseAction(data.Action);
								if (parsed.Error != null)
								{
									IGameLogger log2 = Game1.log;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 3);
									defaultInterpolatedStringHandler.AppendLiteral("Trigger action '");
									defaultInterpolatedStringHandler.AppendFormatted(data.Id);
									defaultInterpolatedStringHandler.AppendLiteral("' will skip invalid action '");
									defaultInterpolatedStringHandler.AppendFormatted(data.Action);
									defaultInterpolatedStringHandler.AppendLiteral("': ");
									defaultInterpolatedStringHandler.AppendFormatted(parsed.Error);
									defaultInterpolatedStringHandler.AppendLiteral(".");
									log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
								}
								else if (!parsed.IsNullHandler)
								{
									actions.Add(parsed);
								}
							}
							if (data.Actions != null)
							{
								foreach (string action in data.Actions)
								{
									CachedAction parsed2 = TriggerActionManager.ParseAction(action);
									if (parsed2.Error != null)
									{
										IGameLogger log3 = Game1.log;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 3);
										defaultInterpolatedStringHandler.AppendLiteral("Trigger action '");
										defaultInterpolatedStringHandler.AppendFormatted(data.Id);
										defaultInterpolatedStringHandler.AppendLiteral("' will skip invalid action '");
										defaultInterpolatedStringHandler.AppendFormatted(data.Action);
										defaultInterpolatedStringHandler.AppendLiteral("': ");
										defaultInterpolatedStringHandler.AppendFormatted(parsed2.Error);
										defaultInterpolatedStringHandler.AppendLiteral(".");
										log3.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
									}
									else if (!parsed2.IsNullHandler)
									{
										actions.Add(parsed2);
									}
								}
							}
							CachedTriggerAction cachedTriggerAction = new CachedTriggerAction(data, actions.ToArray());
							foreach (string trigger in ArgUtility.SplitBySpace(data.Trigger))
							{
								if (!TriggerActionManager.ValidTriggerTypes.Contains(trigger))
								{
									IGameLogger log4 = Game1.log;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 3);
									defaultInterpolatedStringHandler.AppendLiteral("Trigger action '");
									defaultInterpolatedStringHandler.AppendFormatted(data.Id);
									defaultInterpolatedStringHandler.AppendLiteral("' has unknown trigger '");
									defaultInterpolatedStringHandler.AppendFormatted(trigger);
									defaultInterpolatedStringHandler.AppendLiteral("'; expected one of '");
									defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", TriggerActionManager.ValidTriggerTypes));
									defaultInterpolatedStringHandler.AppendLiteral("'.");
									log4.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
								}
								else
								{
									actionsByTrigger[trigger].Add(cachedTriggerAction);
								}
							}
						}
					}
				}
			}
			return actionsByTrigger;
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x001131D4 File Offset: 0x001113D4
		private static bool CanApplyIgnoringRun(TriggerActionData action, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null)
		{
			return (!action.HostOnly || Game1.IsMasterGame) && GameStateQuery.CheckConditions(action.Condition, location, player, targetItem, inputItem, null, null);
		}

		// Token: 0x04000E5E RID: 3678
		public const string trigger_dayEnding = "DayEnding";

		// Token: 0x04000E5F RID: 3679
		public const string trigger_dayStarted = "DayStarted";

		// Token: 0x04000E60 RID: 3680
		public const string trigger_locationChanged = "LocationChanged";

		// Token: 0x04000E61 RID: 3681
		public const string trigger_manual = "Manual";

		// Token: 0x04000E62 RID: 3682
		private static readonly HashSet<string> ValidTriggerTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"DayEnding",
			"DayStarted",
			"LocationChanged",
			"Manual"
		};

		// Token: 0x04000E63 RID: 3683
		private static readonly Dictionary<string, TriggerActionDelegate> ActionHandlers = new Dictionary<string, TriggerActionDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000E64 RID: 3684
		private static readonly Dictionary<string, List<CachedTriggerAction>> ActionsByTrigger = new Dictionary<string, List<CachedTriggerAction>>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000E65 RID: 3685
		private static readonly Dictionary<string, CachedAction> ActionCache = new Dictionary<string, CachedAction>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000E66 RID: 3686
		private static readonly CachedAction NullAction;

		// Token: 0x04000E67 RID: 3687
		private static readonly TriggerActionContext EmptyManualContext = new TriggerActionContext("Manual", LegacyShims.EmptyArray<object>(), null, null);

		// Token: 0x0200050E RID: 1294
		public static class DefaultActions
		{
			// Token: 0x0600403E RID: 16446 RVA: 0x003020BD File Offset: 0x003002BD
			public static bool Null(string[] args, TriggerActionContext context, out string error)
			{
				error = null;
				return true;
			}

			// Token: 0x0600403F RID: 16447 RVA: 0x003020C4 File Offset: 0x003002C4
			public static bool If(string[] args, TriggerActionContext context, out string error)
			{
				int startTrueIndex = -1;
				for (int i = 1; i < args.Length; i++)
				{
					if (args[i] == "##")
					{
						startTrueIndex = i + 1;
						break;
					}
				}
				if (startTrueIndex == -1 || startTrueIndex == args.Length)
				{
					return TriggerActionManager.DefaultActions.<If>g__InvalidFormatError|1_0(out error);
				}
				int startFalseIndex = -1;
				for (int j = startTrueIndex + 1; j < args.Length; j++)
				{
					if (args[j] == "##")
					{
						startFalseIndex = j + 1;
						break;
					}
				}
				if (startFalseIndex == args.Length - 1)
				{
					return TriggerActionManager.DefaultActions.<If>g__InvalidFormatError|1_0(out error);
				}
				if (GameStateQuery.CheckConditions(ArgUtility.UnsplitQuoteAware(args, ' ', 1, startTrueIndex - 1 - 1), null, null, null, null, null, null))
				{
					int maxCount = (startFalseIndex > -1) ? (startFalseIndex - startTrueIndex - 1) : int.MaxValue;
					string action = ArgUtility.UnsplitQuoteAware(args, ' ', startTrueIndex, maxCount);
					Exception ex;
					if (!TriggerActionManager.TryRunAction(action, out error, out ex))
					{
						error = "failed applying if-true action '" + action + "': " + error;
						return false;
					}
				}
				else if (startFalseIndex > -1)
				{
					string action2 = ArgUtility.UnsplitQuoteAware(args, ' ', startFalseIndex, int.MaxValue);
					Exception ex;
					if (!TriggerActionManager.TryRunAction(action2, out error, out ex))
					{
						error = "failed applying if-false action '" + action2 + "': " + error;
						return false;
					}
				}
				error = null;
				return true;
			}

			// Token: 0x06004040 RID: 16448 RVA: 0x003021D8 File Offset: 0x003003D8
			public static bool AddBuff(string[] args, TriggerActionContext context, out string error)
			{
				string buffId;
				int duration;
				if (!ArgUtility.TryGet(args, 1, out buffId, out error, true, "string buffId") || !ArgUtility.TryGetOptionalInt(args, 2, out duration, out error, -1, "int duration"))
				{
					return false;
				}
				Buff buff = new Buff(buffId, null, null, duration, null, -1, null, null, null, null);
				Game1.player.applyBuff(buff);
				return true;
			}

			// Token: 0x06004041 RID: 16449 RVA: 0x00302230 File Offset: 0x00300430
			public static bool RemoveBuff(string[] args, TriggerActionContext context, out string error)
			{
				string buffId;
				if (!ArgUtility.TryGet(args, 1, out buffId, out error, true, "string buffId"))
				{
					return false;
				}
				Game1.player.buffs.Remove(buffId);
				return true;
			}

			// Token: 0x06004042 RID: 16450 RVA: 0x00302264 File Offset: 0x00300464
			public static bool AddMail(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string mailId;
				MailType mailType;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out mailId, out error, true, "string mailId") || !ArgUtility.TryGetOptionalEnum<MailType>(args, 3, out mailType, out error, MailType.Tomorrow, "MailType mailType"))
				{
					return false;
				}
				Game1.player.team.RequestSetMail(playerTarget, mailId, mailType, true, null);
				return true;
			}

			// Token: 0x06004043 RID: 16451 RVA: 0x003022C8 File Offset: 0x003004C8
			public static bool RemoveMail(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string mailId;
				MailType mailType;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out mailId, out error, true, "string mailId") || !ArgUtility.TryGetOptionalEnum<MailType>(args, 3, out mailType, out error, MailType.All, "MailType mailType"))
				{
					return false;
				}
				Game1.player.team.RequestSetMail(playerTarget, mailId, mailType, false, null);
				return true;
			}

			// Token: 0x06004044 RID: 16452 RVA: 0x0030232C File Offset: 0x0030052C
			public static bool AddQuest(string[] args, TriggerActionContext context, out string error)
			{
				string questId;
				if (!ArgUtility.TryGet(args, 1, out questId, out error, true, "string questId"))
				{
					return false;
				}
				Game1.player.addQuest(questId);
				return true;
			}

			// Token: 0x06004045 RID: 16453 RVA: 0x0030235C File Offset: 0x0030055C
			public static bool RemoveQuest(string[] args, TriggerActionContext context, out string error)
			{
				string questId;
				if (!ArgUtility.TryGet(args, 1, out questId, out error, true, "string questId"))
				{
					return false;
				}
				Game1.player.removeQuest(questId);
				return true;
			}

			// Token: 0x06004046 RID: 16454 RVA: 0x0030238C File Offset: 0x0030058C
			public static bool AddSpecialOrder(string[] args, TriggerActionContext context, out string error)
			{
				string orderId;
				if (!ArgUtility.TryGet(args, 1, out orderId, out error, true, "string orderId"))
				{
					return false;
				}
				Game1.player.team.AddSpecialOrder(orderId, null, false);
				return true;
			}

			// Token: 0x06004047 RID: 16455 RVA: 0x003023C8 File Offset: 0x003005C8
			public static bool RemoveSpecialOrder(string[] args, TriggerActionContext context, out string error)
			{
				string orderId;
				if (!ArgUtility.TryGet(args, 1, out orderId, out error, true, "string orderId"))
				{
					return false;
				}
				Game1.player.team.specialOrders.RemoveWhere((SpecialOrder order) => order.questKey.Value == orderId);
				return true;
			}

			// Token: 0x06004048 RID: 16456 RVA: 0x00302418 File Offset: 0x00300618
			public static bool AddItem(string[] args, TriggerActionContext context, out string error)
			{
				string itemId;
				int count;
				int quality;
				if (!ArgUtility.TryGet(args, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(args, 2, out count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(args, 3, out quality, out error, 0, "int quality"))
				{
					return false;
				}
				Item item = ItemRegistry.Create(itemId, count, quality, false);
				if (item != null)
				{
					Game1.player.addItemByMenuIfNecessary(item, null, false);
				}
				return true;
			}

			// Token: 0x06004049 RID: 16457 RVA: 0x00302478 File Offset: 0x00300678
			public static bool RemoveItem(string[] args, TriggerActionContext context, out string error)
			{
				string itemId;
				int count;
				if (!ArgUtility.TryGet(args, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(args, 2, out count, out error, 1, "int count"))
				{
					return false;
				}
				Game1.player.removeFirstOfThisItemFromInventory(itemId, count);
				return true;
			}

			// Token: 0x0600404A RID: 16458 RVA: 0x003024B8 File Offset: 0x003006B8
			public static bool AddMoney(string[] args, TriggerActionContext context, out string error)
			{
				int amount;
				if (!ArgUtility.TryGetInt(args, 1, out amount, out error, "int amount"))
				{
					return false;
				}
				Game1.player.Money += amount;
				if (Game1.player.Money < 0)
				{
					Game1.player.Money = 0;
				}
				return true;
			}

			// Token: 0x0600404B RID: 16459 RVA: 0x00302504 File Offset: 0x00300704
			public static bool AddFriendshipPoints(string[] args, TriggerActionContext context, out string error)
			{
				string npcName;
				int points;
				if (!ArgUtility.TryGet(args, 1, out npcName, out error, true, "string npcName") || !ArgUtility.TryGetInt(args, 2, out points, out error, "int points"))
				{
					return false;
				}
				NPC npc = Game1.getCharacterFromName(npcName, true, false);
				if (npc == null)
				{
					error = "no NPC found with name '" + npcName + "'";
					return false;
				}
				Game1.player.changeFriendship(points, npc);
				return true;
			}

			// Token: 0x0600404C RID: 16460 RVA: 0x00302564 File Offset: 0x00300764
			public static bool AddConversationTopic(string[] args, TriggerActionContext context, out string error)
			{
				string topicId;
				int daysDuration;
				if (!ArgUtility.TryGet(args, 1, out topicId, out error, true, "string topicId") || !ArgUtility.TryGetOptionalInt(args, 2, out daysDuration, out error, 4, "int daysDuration"))
				{
					return false;
				}
				Game1.player.activeDialogueEvents[topicId] = daysDuration;
				return true;
			}

			// Token: 0x0600404D RID: 16461 RVA: 0x003025AC File Offset: 0x003007AC
			public static bool RemoveConversationTopic(string[] args, TriggerActionContext context, out string error)
			{
				string topicId;
				if (!ArgUtility.TryGet(args, 1, out topicId, out error, true, "string topicId"))
				{
					return false;
				}
				Game1.player.activeDialogueEvents.Remove(topicId);
				return true;
			}

			// Token: 0x0600404E RID: 16462 RVA: 0x003025E0 File Offset: 0x003007E0
			public static bool IncrementStat(string[] args, TriggerActionContext context, out string error)
			{
				string statKey;
				int amount;
				if (!ArgUtility.TryGet(args, 1, out statKey, out error, false, "string statKey") || !ArgUtility.TryGetOptionalInt(args, 2, out amount, out error, 1, "int amount"))
				{
					return false;
				}
				Game1.player.stats.Increment(statKey, amount);
				return true;
			}

			// Token: 0x0600404F RID: 16463 RVA: 0x00302628 File Offset: 0x00300828
			public static bool MarkActionApplied(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string actionId;
				bool applied;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out actionId, out error, false, "string actionId") || !ArgUtility.TryGetOptionalBool(args, 3, out applied, out error, true, "bool applied"))
				{
					return false;
				}
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.ActionApplied, playerTarget, actionId, applied, null);
				return true;
			}

			// Token: 0x06004050 RID: 16464 RVA: 0x0030268C File Offset: 0x0030088C
			public static bool MarkCookingRecipeKnown(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string recipeKey;
				bool learned;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out recipeKey, out error, true, "string recipeKey") || !ArgUtility.TryGetOptionalBool(args, 3, out learned, out error, true, "bool learned"))
				{
					return false;
				}
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.CookingRecipeKnown, playerTarget, recipeKey, learned, null);
				return true;
			}

			// Token: 0x06004051 RID: 16465 RVA: 0x003026F0 File Offset: 0x003008F0
			public static bool MarkCraftingRecipeKnown(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string recipeKey;
				bool learned;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out recipeKey, out error, true, "string recipeKey") || !ArgUtility.TryGetOptionalBool(args, 3, out learned, out error, true, "bool learned"))
				{
					return false;
				}
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.CraftingRecipeKnown, playerTarget, recipeKey, learned, null);
				return true;
			}

			// Token: 0x06004052 RID: 16466 RVA: 0x00302754 File Offset: 0x00300954
			public static bool MarkEventSeen(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string eventId;
				bool seen;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out eventId, out error, false, "string eventId") || !ArgUtility.TryGetOptionalBool(args, 3, out seen, out error, true, "bool seen"))
				{
					return false;
				}
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.EventSeen, playerTarget, eventId, seen, null);
				return true;
			}

			// Token: 0x06004053 RID: 16467 RVA: 0x003027B8 File Offset: 0x003009B8
			public static bool MarkQuestionAnswered(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string questionId;
				bool answered;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out questionId, out error, false, "string questionId") || !ArgUtility.TryGetOptionalBool(args, 3, out answered, out error, true, "bool answered"))
				{
					return false;
				}
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.DialogueAnswerSelected, playerTarget, questionId, answered, null);
				return true;
			}

			// Token: 0x06004054 RID: 16468 RVA: 0x0030281C File Offset: 0x00300A1C
			public static bool MarkSongHeard(string[] args, TriggerActionContext context, out string error)
			{
				PlayerActionTarget playerTarget;
				string trackId;
				bool heard;
				if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out playerTarget, out error, "PlayerActionTarget playerTarget") || !ArgUtility.TryGet(args, 2, out trackId, out error, false, "string trackId") || !ArgUtility.TryGetOptionalBool(args, 3, out heard, out error, true, "bool heard"))
				{
					return false;
				}
				Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.SongHeard, playerTarget, trackId, heard, null);
				return true;
			}

			// Token: 0x06004055 RID: 16469 RVA: 0x0030287D File Offset: 0x00300A7D
			public static bool RemoveTemporaryAnimatedSprites(string[] args, TriggerActionContext context, out string error)
			{
				GameLocation currentLocation = Game1.currentLocation;
				if (currentLocation != null)
				{
					currentLocation.TemporarySprites.Clear();
				}
				error = null;
				return true;
			}

			// Token: 0x06004056 RID: 16470 RVA: 0x00302898 File Offset: 0x00300A98
			public static bool SetNpcInvisible(string[] args, TriggerActionContext context, out string error)
			{
				string npcName;
				int daysDuration;
				if (!ArgUtility.TryGet(args, 1, out npcName, out error, false, "string npcName") || !ArgUtility.TryGetInt(args, 2, out daysDuration, out error, "int daysDuration"))
				{
					return false;
				}
				NPC npc = Game1.getCharacterFromName(npcName, true, false);
				if (npc == null)
				{
					error = "no NPC found with name '" + npcName + "'";
					return false;
				}
				npc.IsInvisible = true;
				npc.daysUntilNotInvisible = daysDuration;
				return true;
			}

			// Token: 0x06004057 RID: 16471 RVA: 0x003028FC File Offset: 0x00300AFC
			public static bool SetNpcVisible(string[] args, TriggerActionContext context, out string error)
			{
				string npcName;
				if (!ArgUtility.TryGet(args, 1, out npcName, out error, false, "string npcName"))
				{
					return false;
				}
				NPC npc = Game1.getCharacterFromName(npcName, true, false);
				if (npc == null)
				{
					error = "no NPC found with name '" + npcName + "'";
					return false;
				}
				npc.IsInvisible = false;
				npc.daysUntilNotInvisible = 0;
				return true;
			}

			// Token: 0x06004058 RID: 16472 RVA: 0x0030294C File Offset: 0x00300B4C
			[CompilerGenerated]
			internal static bool <If>g__InvalidFormatError|1_0(out string outError)
			{
				outError = "invalid format: expected a string in the form 'If <game state query> ## <do if true>' or 'If <game state query> ## <do if true> ## <do if false>'";
				return false;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Machines;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Logging;
using StardewValley.Objects;

namespace StardewValley
{
	// Token: 0x020000DC RID: 220
	public static class MachineDataUtility
	{
		// Token: 0x0600109C RID: 4252 RVA: 0x000C7444 File Offset: 0x000C5644
		public static bool HasAdditionalRequirements(IInventory inventory, IList<MachineItemAdditionalConsumedItems> requirements, out MachineItemAdditionalConsumedItems failedRequirement)
		{
			if (requirements != null && requirements.Count > 0)
			{
				foreach (MachineItemAdditionalConsumedItems requirement in requirements)
				{
					if (inventory.CountId(requirement.ItemId) < requirement.RequiredCount)
					{
						failedRequirement = requirement;
						return false;
					}
				}
			}
			failedRequirement = null;
			return true;
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x000C74B4 File Offset: 0x000C56B4
		public static bool CanApplyOutput(Object machine, MachineOutputRule rule, MachineOutputTrigger trigger, Item inputItem, Farmer who, GameLocation location, out MachineOutputTriggerRule triggerRule, out bool matchesExceptCount)
		{
			matchesExceptCount = false;
			triggerRule = null;
			if (rule.Triggers == null)
			{
				return false;
			}
			foreach (MachineOutputTriggerRule curTrigger in rule.Triggers)
			{
				if (curTrigger.Trigger.HasFlag(trigger) && (curTrigger.Condition == null || GameStateQuery.CheckConditions(curTrigger.Condition, location, who, null, inputItem, null, null)))
				{
					if (trigger.HasFlag(MachineOutputTrigger.ItemPlacedInMachine) || trigger.HasFlag(MachineOutputTrigger.OutputCollected))
					{
						if (curTrigger.RequiredItemId != null && !ItemRegistry.HasItemId(inputItem, curTrigger.RequiredItemId))
						{
							continue;
						}
						List<string> requiredTags = curTrigger.RequiredTags;
						if (requiredTags != null && requiredTags.Count > 0 && !ItemContextTagManager.DoAllTagsMatch(curTrigger.RequiredTags, inputItem.GetContextTags()))
						{
							continue;
						}
						if (curTrigger.RequiredCount > inputItem.Stack)
						{
							triggerRule = curTrigger;
							matchesExceptCount = true;
							continue;
						}
					}
					triggerRule = curTrigger;
					matchesExceptCount = false;
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x000C75E0 File Offset: 0x000C57E0
		public static bool TryGetMachineOutputRule(Object machine, MachineData machineData, MachineOutputTrigger trigger, Item inputItem, Farmer who, GameLocation location, out MachineOutputRule rule, out MachineOutputTriggerRule triggerRule, out MachineOutputRule ruleIgnoringCount, out MachineOutputTriggerRule triggerIgnoringCount)
		{
			rule = null;
			triggerRule = null;
			ruleIgnoringCount = null;
			triggerIgnoringCount = null;
			if (((machineData != null) ? machineData.OutputRules : null) == null)
			{
				return false;
			}
			foreach (MachineOutputRule curRule in machineData.OutputRules)
			{
				bool matchesExceptCount;
				if (MachineDataUtility.CanApplyOutput(machine, curRule, trigger, inputItem, who, location, out triggerRule, out matchesExceptCount))
				{
					rule = curRule;
					return true;
				}
				if (matchesExceptCount && (ruleIgnoringCount == null || (ruleIgnoringCount.InvalidCountMessage == null && curRule.InvalidCountMessage != null)))
				{
					ruleIgnoringCount = curRule;
					triggerIgnoringCount = triggerRule;
				}
			}
			return false;
		}

		// Token: 0x0600109F RID: 4255 RVA: 0x000C7690 File Offset: 0x000C5890
		public static MachineItemOutput GetOutputData(Object machine, MachineData machineData, MachineOutputRule outputRule, Item inputItem, Farmer who, GameLocation location)
		{
			MachineOutputTriggerRule machineOutputTriggerRule;
			MachineOutputRule machineOutputRule;
			MachineOutputTriggerRule machineOutputTriggerRule2;
			if (outputRule == null && !MachineDataUtility.TryGetMachineOutputRule(machine, machineData, MachineOutputTrigger.ItemPlacedInMachine, inputItem, who, location, out outputRule, out machineOutputTriggerRule, out machineOutputRule, out machineOutputTriggerRule2))
			{
				return null;
			}
			return MachineDataUtility.GetOutputData(outputRule.OutputItem, outputRule.UseFirstValidOutput, inputItem, who, location);
		}

		// Token: 0x060010A0 RID: 4256 RVA: 0x000C76D0 File Offset: 0x000C58D0
		public static MachineItemOutput GetOutputData(List<MachineItemOutput> outputs, bool useFirstValidOutput, Item inputItem, Farmer who, GameLocation location)
		{
			if (outputs == null || outputs.Count <= 0)
			{
				return null;
			}
			List<MachineItemOutput> validOutputs = (!useFirstValidOutput) ? new List<MachineItemOutput>() : null;
			foreach (MachineItemOutput possibleOutput in outputs)
			{
				if (GameStateQuery.CheckConditions(possibleOutput.Condition, location, who, null, inputItem, null, null))
				{
					if (useFirstValidOutput)
					{
						return possibleOutput;
					}
					validOutputs.Add(possibleOutput);
				}
			}
			if (useFirstValidOutput)
			{
				return null;
			}
			return Game1.random.ChooseFrom(validOutputs);
		}

		// Token: 0x060010A1 RID: 4257 RVA: 0x000C7768 File Offset: 0x000C5968
		public static Item GetOutputItem(Object machine, MachineItemOutput outputData, Item inputItem, Farmer who, bool probe, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			if (outputData == null)
			{
				return null;
			}
			ItemQueryContext context = new ItemQueryContext(machine.Location, who, Game1.random, "machine '" + machine.QualifiedItemId + "' > output rules");
			Item item;
			if (outputData.OutputMethod != null)
			{
				MachineOutputDelegate method;
				string error2;
				if (!StaticDelegateBuilder.TryCreateDelegate<MachineOutputDelegate>(outputData.OutputMethod, out method, out error2))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Machine ");
					defaultInterpolatedStringHandler.AppendFormatted(machine.QualifiedItemId);
					defaultInterpolatedStringHandler.AppendLiteral(" has invalid item output method '");
					defaultInterpolatedStringHandler.AppendFormatted(outputData.OutputMethod);
					defaultInterpolatedStringHandler.AppendLiteral("': ");
					defaultInterpolatedStringHandler.AppendFormatted(error2);
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					return null;
				}
				item = method(machine, inputItem, probe, outputData, who, out overrideMinutesUntilReady);
				item = (Item)ItemQueryResolver.ApplyItemFields(item, outputData, context, inputItem);
			}
			else if (outputData.ItemId == "DROP_IN")
			{
				Item inputItem2 = inputItem;
				item = ((inputItem2 != null) ? inputItem2.getOne() : null);
				item = (Item)ItemQueryResolver.ApplyItemFields(item, outputData, context, inputItem);
			}
			else
			{
				item = ItemQueryResolver.TryResolveRandomItem(outputData, context, false, null, (string id) => MachineDataUtility.FormatOutputId(id, machine, outputData, inputItem, who), inputItem, delegate(string query, string error)
				{
					IGameLogger log2 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 4);
					defaultInterpolatedStringHandler2.AppendLiteral("Machine '");
					defaultInterpolatedStringHandler2.AppendFormatted(machine.QualifiedItemId);
					defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
					defaultInterpolatedStringHandler2.AppendFormatted(query);
					defaultInterpolatedStringHandler2.AppendLiteral("' for output '");
					defaultInterpolatedStringHandler2.AppendFormatted(outputData.Id);
					defaultInterpolatedStringHandler2.AppendLiteral("': ");
					defaultInterpolatedStringHandler2.AppendFormatted(error);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					log2.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				});
			}
			if (item == null)
			{
				return null;
			}
			if (outputData.CopyColor)
			{
				ColoredObject coloredObject = inputItem as ColoredObject;
				Color? color = (coloredObject != null) ? new Color?(coloredObject.color.Value) : ItemContextTagManager.GetColorFromTags(inputItem);
				ColoredObject coloredObj;
				if (color != null && ColoredObject.TrySetColor(item, color.Value, out coloredObj))
				{
					item = coloredObj;
				}
			}
			if (outputData.CopyQuality && inputItem != null)
			{
				item.Quality = inputItem.Quality;
				List<QuantityModifier> qualityModifiers = outputData.QualityModifiers;
				if (qualityModifiers != null && qualityModifiers.Count > 0)
				{
					item.Quality = (int)Utility.ApplyQuantityModifiers((float)item.Quality, outputData.QualityModifiers, outputData.QualityModifierMode, machine.Location, who, item, inputItem, null);
				}
			}
			Object obj = item as Object;
			if (obj != null && outputData.ObjectInternalName != null)
			{
				Item item2 = obj;
				string objectInternalName = outputData.ObjectInternalName;
				Item inputItem3 = inputItem;
				item2.Name = string.Format(objectInternalName, ((inputItem3 != null) ? inputItem3.Name : null) ?? "");
			}
			Object heldObj = item as Object;
			if (heldObj != null)
			{
				Object inputObj = inputItem as Object;
				if (outputData.CopyPrice && inputObj != null)
				{
					heldObj.Price = inputObj.Price;
				}
				List<QuantityModifier> priceModifiers = outputData.PriceModifiers;
				if (priceModifiers != null && priceModifiers.Count > 0)
				{
					heldObj.Price = (int)Utility.ApplyQuantityModifiers((float)heldObj.Price, outputData.PriceModifiers, outputData.PriceModifierMode, machine.Location, who, item, inputItem, null);
				}
				if (!string.IsNullOrWhiteSpace(outputData.PreserveType))
				{
					heldObj.preserve.Value = new Object.PreserveType?((Object.PreserveType)Enum.Parse(typeof(Object.PreserveType), outputData.PreserveType));
				}
				if (!string.IsNullOrWhiteSpace(outputData.PreserveId))
				{
					string preserveId = outputData.PreserveId;
					if (!(preserveId == "DROP_IN"))
					{
						if (!(preserveId == "DROP_IN_PRESERVE"))
						{
							heldObj.preservedParentSheetIndex.Value = outputData.PreserveId;
						}
						else
						{
							heldObj.preservedParentSheetIndex.Value = ((inputObj != null) ? inputObj.GetPreservedItemId() : null);
						}
					}
					else
					{
						NetFieldBase<string, NetString> preservedParentSheetIndex = heldObj.preservedParentSheetIndex;
						Item inputItem4 = inputItem;
						preservedParentSheetIndex.Value = ((inputItem4 != null) ? inputItem4.ItemId : null);
					}
				}
			}
			return item;
		}

		// Token: 0x060010A2 RID: 4258 RVA: 0x000C7BBC File Offset: 0x000C5DBC
		public static void UpdateStats(List<StatIncrement> stats, Item item, int amount)
		{
			if (stats == null)
			{
				return;
			}
			foreach (StatIncrement stat in stats)
			{
				if (stat.RequiredItemId == null || ItemRegistry.HasItemId(item, stat.RequiredItemId))
				{
					List<string> requiredTags = stat.RequiredTags;
					if (requiredTags == null || requiredTags.Count <= 0 || ItemContextTagManager.DoAllTagsMatch(stat.RequiredTags, item.GetContextTags()))
					{
						Game1.stats.Increment(stat.StatName, amount);
					}
				}
			}
		}

		// Token: 0x060010A3 RID: 4259 RVA: 0x000C7C5C File Offset: 0x000C5E5C
		public static bool PlayEffects(Object machine, MachineEffects effect, bool playSounds = true)
		{
			if (effect == null)
			{
				return false;
			}
			string condition = effect.Condition;
			GameLocation location = machine.Location;
			Farmer player = null;
			Item value = machine.lastInputItem.Value;
			if (!GameStateQuery.CheckConditions(condition, location, player, machine.heldObject.Value, value, null, null))
			{
				return false;
			}
			if (playSounds)
			{
				List<MachineSoundData> sounds = effect.Sounds;
				if (sounds != null && sounds.Count > 0)
				{
					foreach (MachineSoundData sound in effect.Sounds)
					{
						if (sound.Delay <= 0)
						{
							machine.Location.playSound(sound.Id, new Vector2?(machine.TileLocation), null, SoundContext.Default);
						}
						else
						{
							DelayedAction.playSoundAfterDelay(sound.Id, sound.Delay, machine.Location, new Vector2?(machine.TileLocation), -1, false);
						}
					}
				}
			}
			if (effect.ShakeDuration >= 0)
			{
				machine.shakeTimer = effect.ShakeDuration;
			}
			if (effect.TemporarySprites != null)
			{
				foreach (TemporaryAnimatedSpriteDefinition temporarySprite in effect.TemporarySprites)
				{
					string condition2 = temporarySprite.Condition;
					GameLocation location2 = machine.Location;
					Farmer player2 = null;
					value = machine.lastInputItem.Value;
					if (GameStateQuery.CheckConditions(condition2, location2, player2, machine.heldObject.Value, value, null, null))
					{
						TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.CreateFromData(temporarySprite, machine.tileLocation.X, machine.tileLocation.Y, (machine.tileLocation.Y + 1f) * 64f / 10000f);
						Game1.multiplayer.broadcastSprites(machine.Location, new TemporaryAnimatedSprite[]
						{
							sprite
						});
					}
				}
			}
			return true;
		}

		// Token: 0x060010A4 RID: 4260 RVA: 0x000C7E3C File Offset: 0x000C603C
		public static string FormatOutputId(string id, Object machine, MachineItemOutput outputData, Item inputItem, Farmer who)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return id;
			}
			bool changed = false;
			string[] words = ArgUtility.SplitBySpace(id);
			for (int i = 0; i < words.Length; i++)
			{
				MachineDataUtility.GetOutputTokenValueDelegate getValue;
				if (MachineDataUtility.OutputTokens.TryGetValue(words[i], out getValue))
				{
					string oldValue = words[i];
					words[i] = getValue(words[i], machine, outputData, inputItem, who);
					changed = (changed || words[i] != oldValue);
				}
			}
			if (!changed)
			{
				return id;
			}
			return string.Join(" ", words);
		}

		// Token: 0x060010A5 RID: 4261 RVA: 0x000C7EB4 File Offset: 0x000C60B4
		private static string GetTokenValue(string key, Object machine, MachineItemOutput outputData, Item inputItem, Farmer who)
		{
			if (key == "DROP_IN_ID")
			{
				return ((inputItem != null) ? inputItem.QualifiedItemId : null) ?? "0";
			}
			if (key == "DROP_IN_PRESERVE")
			{
				Object @object = inputItem as Object;
				return ((@object != null) ? @object.GetPreservedItemId() : null) ?? "0";
			}
			if (key == "NEARBY_FLOWER_ID")
			{
				return MachineDataUtility.GetNearbyFlowerItemId(machine) ?? "-1";
			}
			if (!(key == "DROP_IN_QUALITY"))
			{
				return key;
			}
			return ((inputItem != null) ? new int?(inputItem.Quality) : null).ToString() ?? "";
		}

		// Token: 0x060010A6 RID: 4262 RVA: 0x000C7F6C File Offset: 0x000C616C
		public static string GetNearbyFlowerItemId(Object machine)
		{
			Crop crop = Utility.findCloseFlower(machine.Location, machine.tileLocation.Value, 5, (Crop curCrop) => !curCrop.forageCrop.Value);
			if (crop == null)
			{
				return null;
			}
			return crop.indexOfHarvest.Value;
		}

		// Token: 0x060010A7 RID: 4263 RVA: 0x000C7FC0 File Offset: 0x000C61C0
		// Note: this type is marked as 'beforefieldinit'.
		static MachineDataUtility()
		{
			Dictionary<string, MachineDataUtility.GetOutputTokenValueDelegate> dictionary = new Dictionary<string, MachineDataUtility.GetOutputTokenValueDelegate>();
			dictionary["DROP_IN_ID"] = new MachineDataUtility.GetOutputTokenValueDelegate(MachineDataUtility.GetTokenValue);
			dictionary["DROP_IN_PRESERVE"] = new MachineDataUtility.GetOutputTokenValueDelegate(MachineDataUtility.GetTokenValue);
			dictionary["NEARBY_FLOWER_ID"] = new MachineDataUtility.GetOutputTokenValueDelegate(MachineDataUtility.GetTokenValue);
			dictionary["DROP_IN_QUALITY"] = new MachineDataUtility.GetOutputTokenValueDelegate(MachineDataUtility.GetTokenValue);
			MachineDataUtility.OutputTokens = dictionary;
		}

		// Token: 0x04000A0E RID: 2574
		public static readonly IDictionary<string, MachineDataUtility.GetOutputTokenValueDelegate> OutputTokens;

		// Token: 0x020004A9 RID: 1193
		// (Invoke) Token: 0x06003EE0 RID: 16096
		public delegate string GetOutputTokenValueDelegate(string key, Object machine, MachineItemOutput outputData, Item inputItem, Farmer who);
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Netcode.Validation
{
	// Token: 0x02000062 RID: 98
	public static class NetFieldValidator
	{
		// Token: 0x06000414 RID: 1044 RVA: 0x00012DD4 File Offset: 0x00010FD4
		public static void ValidateNetFields(INetObject<NetFields> owner, Action<string> onError)
		{
			string collectionName = owner.NetFields.Name;
			HashSet<INetSerializable> trackedFields = new HashSet<INetSerializable>(owner.NetFields.GetFields(), ReferenceEqualityComparer.Instance);
			List<NetFieldValidatorEntry> ownerFields = new List<NetFieldValidatorEntry>();
			foreach (FieldInfo fieldInfo in owner.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				NetFieldValidatorEntry netField;
				if (NetFieldValidatorEntry.TryGetNetField(owner, fieldInfo, out netField))
				{
					if (netField.IsMarkedNotNetField())
					{
						if (!NetFieldValidator.IsInCollection(trackedFields, netField))
						{
							goto IL_7B;
						}
						onError(NetFieldValidator.GetFieldError(collectionName, netField, "is marked [NotNetFieldAttribute] but still added to the collection"));
					}
					ownerFields.Add(netField);
				}
				IL_7B:;
			}
			foreach (NetFieldValidatorEntry entry in ownerFields)
			{
				if (entry.Value == null)
				{
					onError(NetFieldValidator.GetFieldError(collectionName, entry, "is null"));
				}
				else if (string.IsNullOrWhiteSpace(entry.Name))
				{
					onError(NetFieldValidator.GetFieldError(collectionName, entry, "has no name (and likely isn't in the collection)"));
				}
				else if (!NetFieldValidator.IsInCollection(trackedFields, entry.Value))
				{
					onError(NetFieldValidator.GetFieldError(collectionName, entry, "isn't in the collection"));
				}
			}
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00012F08 File Offset: 0x00011108
		private static string GetFieldError(string collectionName, NetFieldValidatorEntry entry, string phrase)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 4);
			defaultInterpolatedStringHandler.AppendLiteral("The owner of ");
			defaultInterpolatedStringHandler.AppendFormatted("NetFields");
			defaultInterpolatedStringHandler.AppendLiteral(" collection '");
			defaultInterpolatedStringHandler.AppendFormatted(collectionName);
			defaultInterpolatedStringHandler.AppendLiteral("' has field '");
			defaultInterpolatedStringHandler.AppendFormatted(entry.FromField.Name);
			defaultInterpolatedStringHandler.AppendLiteral("' which ");
			defaultInterpolatedStringHandler.AppendFormatted(phrase);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x00012F90 File Offset: 0x00011190
		private static bool IsInCollection(HashSet<INetSerializable> trackedFields, object netField)
		{
			INetSerializable field = netField as INetSerializable;
			if (field == null)
			{
				INetObject<NetFields> container = netField as INetObject<NetFields>;
				return container != null && trackedFields.Contains(container.NetFields);
			}
			return trackedFields.Contains(field);
		}
	}
}

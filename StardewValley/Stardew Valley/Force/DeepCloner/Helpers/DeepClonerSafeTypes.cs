using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x0200006C RID: 108
	internal static class DeepClonerSafeTypes
	{
		// Token: 0x0600044F RID: 1103 RVA: 0x000148F0 File Offset: 0x00012AF0
		static DeepClonerSafeTypes()
		{
			foreach (Type x in new Type[]
			{
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
				typeof(char),
				typeof(string),
				typeof(bool),
				typeof(DateTime),
				typeof(IntPtr),
				typeof(UIntPtr),
				typeof(Guid),
				Type.GetType("System.RuntimeType"),
				Type.GetType("System.RuntimeTypeHandle")
			})
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(x, true);
			}
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x00014A30 File Offset: 0x00012C30
		private static bool CanReturnSameType(Type type, HashSet<Type> processingTypes)
		{
			bool isSafe;
			if (DeepClonerSafeTypes.KnownTypes.TryGetValue(type, out isSafe))
			{
				return isSafe;
			}
			if (type.IsEnum() || type.IsPointer)
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (type.FullName.StartsWith("System.DBNull"))
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (type.FullName.StartsWith("System.RuntimeType"))
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (type.FullName.StartsWith("System.Reflection.") && object.Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (type.IsSubclassOfTypeByName("CriticalFinalizerObject"))
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (type.FullName.StartsWith("Microsoft.Extensions.DependencyInjection."))
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (type.FullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
				return true;
			}
			if (!type.IsValueType())
			{
				DeepClonerSafeTypes.KnownTypes.TryAdd(type, false);
				return false;
			}
			if (processingTypes == null)
			{
				processingTypes = new HashSet<Type>();
			}
			processingTypes.Add(type);
			List<FieldInfo> fi = new List<FieldInfo>();
			Type tp = type;
			do
			{
				fi.AddRange(tp.GetAllFields());
				tp = tp.BaseType();
			}
			while (tp != null);
			foreach (FieldInfo fieldInfo in fi)
			{
				Type fieldType = fieldInfo.FieldType;
				if (!processingTypes.Contains(fieldType) && !DeepClonerSafeTypes.CanReturnSameType(fieldType, processingTypes))
				{
					DeepClonerSafeTypes.KnownTypes.TryAdd(type, false);
					return false;
				}
			}
			DeepClonerSafeTypes.KnownTypes.TryAdd(type, true);
			return true;
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x00014C18 File Offset: 0x00012E18
		public static bool CanReturnSameObject(Type type)
		{
			return DeepClonerSafeTypes.CanReturnSameType(type, null);
		}

		// Token: 0x0400019F RID: 415
		internal static readonly ConcurrentDictionary<Type, bool> KnownTypes = new ConcurrentDictionary<Type, bool>();
	}
}

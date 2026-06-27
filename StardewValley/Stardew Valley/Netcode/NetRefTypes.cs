using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Netcode
{
	// Token: 0x0200005C RID: 92
	internal static class NetRefTypes
	{
		// Token: 0x060003CB RID: 971 RVA: 0x000122BC File Offset: 0x000104BC
		public static Type ReadType(this BinaryReader reader)
		{
			Type genericType = reader.ReadGenericType();
			if (genericType == null || !genericType.IsGenericTypeDefinition)
			{
				return genericType;
			}
			int numArgs = genericType.GetGenericArguments().Length;
			Type[] arguments = new Type[numArgs];
			for (int i = 0; i < numArgs; i++)
			{
				arguments[i] = reader.ReadType();
			}
			return genericType.MakeGenericType(arguments);
		}

		// Token: 0x060003CC RID: 972 RVA: 0x00012310 File Offset: 0x00010510
		private static Type ReadGenericType(this BinaryReader reader)
		{
			string typeName = reader.ReadString();
			if (typeName.Length == 0)
			{
				return null;
			}
			Type type = NetRefTypes.GetType(typeName);
			if (type == null)
			{
				throw new InvalidOperationException();
			}
			return type;
		}

		// Token: 0x060003CD RID: 973 RVA: 0x00012344 File Offset: 0x00010544
		public static void WriteType(this BinaryWriter writer, Type type)
		{
			Type genericType = type;
			if (type != null && type.IsGenericType)
			{
				genericType = type.GetGenericTypeDefinition();
			}
			writer.WriteGenericType(genericType);
			if (genericType == null || !genericType.IsGenericType)
			{
				return;
			}
			foreach (Type argument in type.GetGenericArguments())
			{
				writer.WriteType(argument);
			}
		}

		// Token: 0x060003CE RID: 974 RVA: 0x000123A4 File Offset: 0x000105A4
		private static void WriteGenericType(this BinaryWriter writer, Type type)
		{
			if (type == null)
			{
				writer.Write("");
				return;
			}
			writer.Write(type.FullName);
		}

		// Token: 0x060003CF RID: 975 RVA: 0x000123C7 File Offset: 0x000105C7
		public static void WriteTypeOf<T>(this BinaryWriter writer, T value)
		{
			if (value == null)
			{
				writer.WriteType(null);
				return;
			}
			writer.WriteType(value.GetType());
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x000123EC File Offset: 0x000105EC
		private static Type GetType(string typeName)
		{
			Type type;
			if (NetRefTypes.types.TryGetValue(typeName, out type))
			{
				return type;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type = assemblies[i].GetType(typeName);
				if (type != null)
				{
					NetRefTypes.types[typeName] = type;
					return type;
				}
			}
			return null;
		}

		// Token: 0x0400018F RID: 399
		private static Dictionary<string, Type> types = new Dictionary<string, Type>();
	}
}

using System;
using System.Reflection;

namespace Netcode.Validation
{
	// Token: 0x02000063 RID: 99
	public class NetFieldValidatorEntry
	{
		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000417 RID: 1047 RVA: 0x00012FC9 File Offset: 0x000111C9
		public string Name { get; }

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000418 RID: 1048 RVA: 0x00012FD1 File Offset: 0x000111D1
		public object Value { get; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x00012FD9 File Offset: 0x000111D9
		public FieldInfo FromField { get; }

		// Token: 0x0600041A RID: 1050 RVA: 0x00012FE1 File Offset: 0x000111E1
		public NetFieldValidatorEntry(string name, object value, FieldInfo fromField)
		{
			this.Name = name;
			this.Value = value;
			this.FromField = fromField;
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x00013000 File Offset: 0x00011200
		public static bool TryGetNetField(INetObject<NetFields> owner, FieldInfo field, out NetFieldValidatorEntry netField)
		{
			if (field.Name != "NetFields" && field.Name[0] != '<')
			{
				Type valueType = field.FieldType;
				if (typeof(INetSerializable).IsAssignableFrom(valueType) && !NetFieldValidatorEntry.IsMarkedNotImplicitNetField(valueType))
				{
					INetSerializable value = (INetSerializable)field.GetValue(owner);
					netField = new NetFieldValidatorEntry((value != null) ? value.Name : null, value, field);
					return true;
				}
				if (typeof(INetObject<NetFields>).IsAssignableFrom(valueType) && !NetFieldValidatorEntry.IsMarkedNotImplicitNetField(valueType))
				{
					INetObject<NetFields> value2 = (INetObject<NetFields>)field.GetValue(owner);
					netField = new NetFieldValidatorEntry((value2 != null) ? value2.NetFields.Name : null, value2, field);
					return true;
				}
			}
			netField = null;
			return false;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x000130C1 File Offset: 0x000112C1
		public bool IsMarkedNotNetField()
		{
			return this.FromField.GetCustomAttribute<NotNetFieldAttribute>() != null;
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x000130D1 File Offset: 0x000112D1
		public static bool IsMarkedNotImplicitNetField(Type type)
		{
			return type.GetCustomAttribute(true) != null;
		}
	}
}

using System;
using System.Linq;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x0200006E RID: 110
	internal static class ReflectionHelper
	{
		// Token: 0x06000455 RID: 1109 RVA: 0x00014CDF File Offset: 0x00012EDF
		public static bool IsEnum(this Type t)
		{
			return t.GetTypeInfo().IsEnum;
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00014CEC File Offset: 0x00012EEC
		public static bool IsValueType(this Type t)
		{
			return t.GetTypeInfo().IsValueType;
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00014CF9 File Offset: 0x00012EF9
		public static bool IsClass(this Type t)
		{
			return t.GetTypeInfo().IsClass;
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x00014D06 File Offset: 0x00012F06
		public static Type BaseType(this Type t)
		{
			return t.GetTypeInfo().BaseType;
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x00014D13 File Offset: 0x00012F13
		public static FieldInfo[] GetAllFields(this Type t)
		{
			return (from x in t.GetTypeInfo().DeclaredFields
			where !x.IsStatic
			select x).ToArray<FieldInfo>();
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00014D49 File Offset: 0x00012F49
		public static PropertyInfo[] GetPublicProperties(this Type t)
		{
			return t.GetTypeInfo().DeclaredProperties.ToArray<PropertyInfo>();
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00014D5B File Offset: 0x00012F5B
		public static FieldInfo[] GetDeclaredFields(this Type t)
		{
			return (from x in t.GetTypeInfo().DeclaredFields
			where !x.IsStatic
			select x).ToArray<FieldInfo>();
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x00014D91 File Offset: 0x00012F91
		public static ConstructorInfo[] GetPrivateConstructors(this Type t)
		{
			return t.GetTypeInfo().DeclaredConstructors.ToArray<ConstructorInfo>();
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x00014DA3 File Offset: 0x00012FA3
		public static ConstructorInfo[] GetPublicConstructors(this Type t)
		{
			return t.GetTypeInfo().DeclaredConstructors.ToArray<ConstructorInfo>();
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00014DB5 File Offset: 0x00012FB5
		public static MethodInfo GetPrivateMethod(this Type t, string methodName)
		{
			return t.GetTypeInfo().GetDeclaredMethod(methodName);
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x00014DC3 File Offset: 0x00012FC3
		public static MethodInfo GetMethod(this Type t, string methodName)
		{
			return t.GetTypeInfo().GetDeclaredMethod(methodName);
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00014DD1 File Offset: 0x00012FD1
		public static MethodInfo GetPrivateStaticMethod(this Type t, string methodName)
		{
			return t.GetTypeInfo().GetDeclaredMethod(methodName);
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00014DDF File Offset: 0x00012FDF
		public static FieldInfo GetPrivateField(this Type t, string fieldName)
		{
			return t.GetTypeInfo().GetDeclaredField(fieldName);
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00014DED File Offset: 0x00012FED
		public static bool IsSubclassOfTypeByName(this Type t, string typeName)
		{
			while (t != null)
			{
				if (t.Name == typeName)
				{
					return true;
				}
				t = t.BaseType();
			}
			return false;
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00014E13 File Offset: 0x00013013
		public static bool IsAssignableFrom(this Type from, Type to)
		{
			return from.GetTypeInfo().IsAssignableFrom(to.GetTypeInfo());
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00014E26 File Offset: 0x00013026
		public static bool IsInstanceOfType(this Type from, object to)
		{
			return from.IsAssignableFrom(to.GetType());
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00014E34 File Offset: 0x00013034
		public static Type[] GenericArguments(this Type t)
		{
			return t.GetTypeInfo().GenericTypeArguments;
		}
	}
}

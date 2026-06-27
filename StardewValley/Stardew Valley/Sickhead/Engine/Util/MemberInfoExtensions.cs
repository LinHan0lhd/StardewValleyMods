using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sickhead.Engine.Util
{
	// Token: 0x02000025 RID: 37
	public static class MemberInfoExtensions
	{
		// Token: 0x06000116 RID: 278 RVA: 0x0000ADB0 File Offset: 0x00008FB0
		public static Type GetDataType(this MemberInfo info)
		{
			PropertyInfo pi = info as PropertyInfo;
			if (pi != null)
			{
				return pi.PropertyType;
			}
			FieldInfo fi = info as FieldInfo;
			if (fi == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 1);
				defaultInterpolatedStringHandler.AppendLiteral("MemberInfo.GetDataType is not possible for type=");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(info.GetType());
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return fi.FieldType;
		}

		// Token: 0x06000117 RID: 279 RVA: 0x0000AE10 File Offset: 0x00009010
		public static object GetValue(this MemberInfo info, object obj)
		{
			return info.GetValue(obj, null);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x0000AE1A File Offset: 0x0000901A
		public static void SetValue(this MemberInfo info, object obj, object value)
		{
			info.SetValue(obj, value, null);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0000AE28 File Offset: 0x00009028
		public static object GetValue(this MemberInfo info, object obj, object[] index)
		{
			PropertyInfo pi = info as PropertyInfo;
			if (pi != null)
			{
				return pi.GetValue(obj, index);
			}
			FieldInfo fi = info as FieldInfo;
			if (fi == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
				defaultInterpolatedStringHandler.AppendLiteral("MemberInfo.GetValue is not possible for type=");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(info.GetType());
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return fi.GetValue(obj);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x0000AE8C File Offset: 0x0000908C
		public static void SetValue(this MemberInfo info, object obj, object value, object[] index)
		{
			PropertyInfo pi = info as PropertyInfo;
			if (pi != null)
			{
				pi.SetValue(obj, value, index);
				return;
			}
			FieldInfo fi = info as FieldInfo;
			if (fi == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
				defaultInterpolatedStringHandler.AppendLiteral("MemberInfo.SetValue is not possible for type=");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(info.GetType());
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			fi.SetValue(obj, value);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0000AEF4 File Offset: 0x000090F4
		public static bool IsStatic(this MemberInfo info)
		{
			PropertyInfo pi = info as PropertyInfo;
			if (pi != null)
			{
				return pi.GetGetMethod(true).IsStatic;
			}
			FieldInfo fi = info as FieldInfo;
			if (fi != null)
			{
				return fi.IsStatic;
			}
			MethodInfo mi = info as MethodInfo;
			if (mi == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
				defaultInterpolatedStringHandler.AppendLiteral("MemberInfo.IsStatic is not possible for type=");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(info.GetType());
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return mi.IsStatic;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x0000AF6C File Offset: 0x0000916C
		public static bool CanBeSet(this MemberInfo info)
		{
			PropertyInfo pi = info as PropertyInfo;
			if (pi != null)
			{
				MethodAttributes methodAtt = pi.GetSetMethod().Attributes;
				return !pi.CanWrite || ((methodAtt & MethodAttributes.Public) != MethodAttributes.Public && (methodAtt & MethodAttributes.Assembly) != MethodAttributes.Assembly);
			}
			FieldInfo fi = info as FieldInfo;
			if (fi == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
				defaultInterpolatedStringHandler.AppendLiteral("MemberInfo.CanSet is not possible for type=");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(info.GetType());
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return !fi.IsPrivate && !fi.IsFamily;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x0000AFFA File Offset: 0x000091FA
		public static Delegate CreateDelegate(this MethodInfo method, Type type, object target)
		{
			return Delegate.CreateDelegate(type, target, method);
		}

		// Token: 0x0600011E RID: 286 RVA: 0x0000B004 File Offset: 0x00009204
		public static Delegate CreateDelegate(this MethodInfo method, Type type)
		{
			return Delegate.CreateDelegate(type, method);
		}
	}
}

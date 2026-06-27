using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sickhead.Engine.Util;

namespace StardewValley.Extensions
{
	// Token: 0x02000320 RID: 800
	public static class ReflectionExtensions
	{
		// Token: 0x06003475 RID: 13429 RVA: 0x0029CDAC File Offset: 0x0029AFAC
		public static bool TrySetValueFromString(this MemberInfo info, object obj, string rawValue, object[] index, out string error)
		{
			FieldInfo field = info as FieldInfo;
			Type valueType;
			bool canWrite;
			if (field == null)
			{
				PropertyInfo property = info as PropertyInfo;
				if (property == null)
				{
					error = "the member is not a field or property";
					return false;
				}
				valueType = property.PropertyType;
				canWrite = property.CanWrite;
			}
			else
			{
				valueType = field.FieldType;
				canWrite = (!field.IsLiteral && !field.IsLiteral);
			}
			if (!canWrite)
			{
				error = "the " + ((info is FieldInfo) ? "field" : "property") + " property is read-only";
				return false;
			}
			object value;
			try
			{
				value = Convert.ChangeType(rawValue, valueType);
			}
			catch (FormatException)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 2);
				defaultInterpolatedStringHandler.AppendLiteral("can't convert value '");
				defaultInterpolatedStringHandler.AppendFormatted(rawValue);
				defaultInterpolatedStringHandler.AppendLiteral("' to the '");
				defaultInterpolatedStringHandler.AppendFormatted(valueType.FullName);
				defaultInterpolatedStringHandler.AppendLiteral("' type");
				error = defaultInterpolatedStringHandler.ToStringAndClear();
				return false;
			}
			bool result;
			try
			{
				info.SetValue(obj, value, index);
				error = null;
				result = true;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				result = false;
			}
			return result;
		}
	}
}

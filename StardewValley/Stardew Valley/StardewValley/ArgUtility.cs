using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	// Token: 0x02000080 RID: 128
	public static class ArgUtility
	{
		// Token: 0x060004BA RID: 1210 RVA: 0x00016A66 File Offset: 0x00014C66
		public static string[] SplitBySpace(string value)
		{
			return ((value != null) ? value.Split(' ', StringSplitOptions.RemoveEmptyEntries) : null) ?? LegacyShims.EmptyArray<string>();
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00016A80 File Offset: 0x00014C80
		public static string[] SplitBySpace(string value, int limit)
		{
			return ((value != null) ? value.Split(' ', limit, StringSplitOptions.RemoveEmptyEntries) : null) ?? LegacyShims.EmptyArray<string>();
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00016A9B File Offset: 0x00014C9B
		public static string SplitBySpaceAndGet(string value, int index, string defaultValue = null)
		{
			if (value == null)
			{
				return defaultValue;
			}
			return ArgUtility.Get(value.Split(' ', index + 2, StringSplitOptions.RemoveEmptyEntries), index, defaultValue, true);
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x00016AB6 File Offset: 0x00014CB6
		public static string[] SplitBySpaceQuoteAware(string input)
		{
			return ArgUtility.SplitQuoteAware(input, ' ', StringSplitOptions.RemoveEmptyEntries, false);
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00016AC4 File Offset: 0x00014CC4
		public static string[] SplitQuoteAware(string input, char delimiter, StringSplitOptions splitOptions = StringSplitOptions.None, bool keepQuotesAndEscapes = false)
		{
			if (string.IsNullOrEmpty(input))
			{
				return LegacyShims.EmptyArray<string>();
			}
			if (!input.Contains('"'))
			{
				return input.Split(delimiter, splitOptions);
			}
			bool shouldTrimEntries = false;
			if (splitOptions.HasFlag(StringSplitOptions.TrimEntries))
			{
				shouldTrimEntries = true;
				splitOptions &= ~StringSplitOptions.TrimEntries;
			}
			bool splitOptionsRemoveEmpty = splitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries);
			string[] segments = input.Split('"', StringSplitOptions.None);
			List<string> values = new List<string>(segments.Length * 4);
			bool isQuoted = true;
			bool prevEndsWithDelimiter = true;
			string prevValue = null;
			int i = 0;
			int last = segments.Length - 1;
			while (i <= last)
			{
				isQuoted = !isQuoted;
				string segment = segments[i];
				bool overwritePrev = false;
				bool appendToPrev = false;
				bool endsWithDelimiter = segment.EndsWith(delimiter);
				if (keepQuotesAndEscapes && i != 0)
				{
					segment = "\"" + segment;
				}
				if (!prevEndsWithDelimiter)
				{
					if (prevValue.EndsWith('\\'))
					{
						segment = (keepQuotesAndEscapes ? (prevValue + segment) : (prevValue.Substring(0, prevValue.Length - 1) + "\"" + segment));
						isQuoted = !isQuoted;
						overwritePrev = true;
					}
					else if (isQuoted || !segment.StartsWith(delimiter))
					{
						appendToPrev = true;
					}
					else
					{
						segment = segment.Substring(1);
					}
				}
				if (values.Count == 0)
				{
					overwritePrev = false;
					appendToPrev = false;
				}
				if (isQuoted)
				{
					if (overwritePrev)
					{
						values[values.Count - 1] = segment;
					}
					else if (appendToPrev)
					{
						List<string> list = values;
						int num = values.Count - 1;
						list[num] += segment;
						segment = values[values.Count - 1];
					}
					else
					{
						values.Add(segment);
					}
					prevValue = segment;
					prevEndsWithDelimiter = false;
				}
				else
				{
					if (endsWithDelimiter && !splitOptionsRemoveEmpty && i != last && segment.Length > 0)
					{
						segment = segment.Substring(0, segment.Length - 1);
					}
					string[] split = segment.Split(delimiter, splitOptions);
					int num = split.Length;
					if (num != 0)
					{
						if (num == 1)
						{
							if (endsWithDelimiter && split[0] == string.Empty)
							{
								prevValue = string.Empty;
								goto IL_27B;
							}
						}
						if (overwritePrev)
						{
							values.RemoveAt(values.Count - 1);
							values.AddRange(split);
						}
						else if (appendToPrev)
						{
							List<string> list = values;
							int index = values.Count - 1;
							list[index] += split[0];
							if (split.Length > 1)
							{
								values.AddRange(new ArraySegment<string>(split, 1, split.Length - 1));
							}
						}
						else
						{
							values.AddRange(split);
						}
						prevValue = split[split.Length - 1];
					}
					else
					{
						prevValue = string.Empty;
					}
					IL_27B:
					prevEndsWithDelimiter = endsWithDelimiter;
				}
				i++;
			}
			if (shouldTrimEntries)
			{
				for (int j = values.Count - 1; j >= 0; j--)
				{
					values[j] = values[j].Trim();
					if (splitOptionsRemoveEmpty && values[j].Length == 0)
					{
						values.RemoveAt(j);
					}
				}
			}
			return values.ToArray();
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x00016DB0 File Offset: 0x00014FB0
		public static string UnsplitQuoteAware(string[] input, char delimiter, int startAt = 0, int count = 2147483647)
		{
			if (startAt < 0)
			{
				throw new ArgumentException("Can't start unsplitting before the bounds of the array.", "startAt");
			}
			if (input == null || count == 0 || startAt >= input.Length)
			{
				return string.Empty;
			}
			count = Math.Min(count, input.Length - startAt);
			string[] result = new string[count];
			int i = startAt;
			int endAt = startAt + count - 1;
			while (i <= endAt)
			{
				string arg = input[i];
				if (arg.Contains('"'))
				{
					arg = ArgUtility.EscapeQuotes(arg);
				}
				if (arg.Contains(delimiter))
				{
					arg = "\"" + arg + "\"";
				}
				result[i - startAt] = arg;
				i++;
			}
			return string.Join(delimiter, result);
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x00016E45 File Offset: 0x00015045
		public static string EscapeQuotes(string input)
		{
			return input.Replace("\"", "\\\"");
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x00016E57 File Offset: 0x00015057
		public static bool HasIndex<T>(T[] array, int index)
		{
			return index >= 0 && array != null && array.Length > index;
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00016E6C File Offset: 0x0001506C
		public static T[] GetSubsetOf<T>(T[] array, int startAt, int length = -1)
		{
			if (startAt < 0)
			{
				throw new ArgumentException("Can't start copying before the bounds of the array.", "startAt");
			}
			if (array == null || length == 0 || startAt > array.Length - 1)
			{
				return LegacyShims.EmptyArray<T>();
			}
			if (startAt == 0 && (length == -1 || length == array.Length))
			{
				return array.ToArray<T>();
			}
			if (length < 0)
			{
				length = array.Length - startAt;
			}
			T[] subArray = new T[length];
			Array.Copy(array, startAt, subArray, 0, length);
			return subArray;
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x00016ED4 File Offset: 0x000150D4
		public static string Get(string[] array, int index, string defaultValue = null, bool allowBlank = true)
		{
			if (index >= 0)
			{
				int? num = (array != null) ? new int?(array.Length) : null;
				if (index < num.GetValueOrDefault() & num != null)
				{
					string value = array[index];
					if (allowBlank || !string.IsNullOrWhiteSpace(value))
					{
						return value;
					}
				}
			}
			return defaultValue;
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00016F24 File Offset: 0x00015124
		public static bool TryGet(string[] array, int index, out string value, out string error, bool allowBlank = true, [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null)
			{
				value = null;
				error = "argument list is null";
				return false;
			}
			if (index < 0 || index >= array.Length)
			{
				value = null;
				error = ArgUtility.GetMissingRequiredIndexError(array, index, name);
				return false;
			}
			value = array[index];
			if (!allowBlank && string.IsNullOrWhiteSpace(value))
			{
				value = null;
				error = "required " + ArgUtility.GetFieldLabel(index, name) + " has a blank value";
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00016F90 File Offset: 0x00015190
		public static bool TryGetOptional(string[] array, int index, out string value, out string error, string defaultValue = null, bool allowBlank = true, [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null || index < 0 || index >= array.Length || (!allowBlank && array[index] == string.Empty))
			{
				value = defaultValue;
				error = null;
				return true;
			}
			value = array[index];
			if (!allowBlank && string.IsNullOrWhiteSpace(value))
			{
				value = defaultValue;
				error = "optional " + ArgUtility.GetFieldLabel(index, name) + " can't have a blank value";
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x00016FFC File Offset: 0x000151FC
		public static bool GetBool(string[] array, int index, bool defaultValue = false)
		{
			bool value;
			if (!bool.TryParse(ArgUtility.Get(array, index, null, true), out value))
			{
				return defaultValue;
			}
			return value;
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x00017020 File Offset: 0x00015220
		public static bool TryGetBool(string[] array, int index, out bool value, out string error, [CallerArgumentExpression("value")] string name = null)
		{
			string raw;
			if (!ArgUtility.TryGet(array, index, out raw, out error, false, name))
			{
				value = false;
				return false;
			}
			if (!bool.TryParse(raw, out value))
			{
				value = false;
				error = ArgUtility.GetValueParseError(array, index, name, true, "a boolean (should be 'true' or 'false')");
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x00017064 File Offset: 0x00015264
		public static bool TryGetOptionalBool(string[] array, int index, out bool value, out string error, bool defaultValue = false, [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null || index < 0 || index >= array.Length || array[index] == string.Empty)
			{
				error = null;
				value = defaultValue;
				return true;
			}
			if (!bool.TryParse(array[index], out value))
			{
				error = ArgUtility.GetValueParseError(array, index, name, false, "a boolean");
				value = defaultValue;
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x000170BC File Offset: 0x000152BC
		public static int GetDirection(string[] array, int index, int defaultValue = 0)
		{
			int value;
			if (!Utility.TryParseDirection(ArgUtility.Get(array, index, null, true), out value))
			{
				return defaultValue;
			}
			return value;
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x000170E0 File Offset: 0x000152E0
		public static bool TryGetDirection(string[] array, int index, out int value, out string error, [CallerArgumentExpression("value")] string name = null)
		{
			string raw;
			if (!ArgUtility.TryGet(array, index, out raw, out error, false, name))
			{
				value = 0;
				return false;
			}
			if (!Utility.TryParseDirection(raw, out value))
			{
				value = 0;
				error = ArgUtility.GetValueParseError(array, index, name, true, "a direction (should be 'up', 'down', 'left', or 'right')");
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x00017124 File Offset: 0x00015324
		public static bool TryGetOptionalDirection(string[] array, int index, out int value, out string error, int defaultValue = 0, [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null || index < 0 || index >= array.Length || array[index] == string.Empty)
			{
				error = null;
				value = defaultValue;
				return true;
			}
			if (!Utility.TryParseDirection(array[index], out value))
			{
				error = ArgUtility.GetValueParseError(array, index, name, true, "a direction (should be one of 'up', 'down', 'left', or 'right')");
				value = defaultValue;
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x0001717C File Offset: 0x0001537C
		public static TEnum GetEnum<TEnum>(string[] array, int index, TEnum defaultValue = default(TEnum)) where TEnum : struct
		{
			TEnum value;
			if (!Utility.TryParseEnum<TEnum>(ArgUtility.Get(array, index, null, true), out value))
			{
				return defaultValue;
			}
			return value;
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x000171A0 File Offset: 0x000153A0
		public static bool TryGetEnum<TEnum>(string[] array, int index, out TEnum value, out string error, [CallerArgumentExpression("value")] string name = null) where TEnum : struct
		{
			string raw;
			if (!ArgUtility.TryGet(array, index, out raw, out error, false, name))
			{
				value = default(TEnum);
				return false;
			}
			if (!Utility.TryParseEnum<TEnum>(raw, out value))
			{
				Type type = typeof(TEnum);
				value = default(TEnum);
				bool required = true;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
				defaultInterpolatedStringHandler.AppendLiteral("an enum of type '");
				defaultInterpolatedStringHandler.AppendFormatted(type.FullName ?? type.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' (should be one of ");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", Enum.GetNames(typeof(TEnum))));
				defaultInterpolatedStringHandler.AppendLiteral(")");
				error = ArgUtility.GetValueParseError(array, index, name, required, defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x00017264 File Offset: 0x00015464
		public static bool TryGetOptionalEnum<TEnum>(string[] array, int index, out TEnum value, out string error, TEnum defaultValue = default(TEnum), [CallerArgumentExpression("value")] string name = null) where TEnum : struct
		{
			if (array == null || index < 0 || index >= array.Length || array[index] == string.Empty)
			{
				error = null;
				value = defaultValue;
				return true;
			}
			if (!Utility.TryParseEnum<TEnum>(array[index], out value))
			{
				Type type = typeof(TEnum);
				error = ArgUtility.GetValueParseError(array, index, name, false, "an enum of type '" + (type.FullName ?? type.Name) + "'");
				value = defaultValue;
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x000172EC File Offset: 0x000154EC
		public static float GetFloat(string[] array, int index, float defaultValue = 0f)
		{
			float value;
			if (!float.TryParse(ArgUtility.Get(array, index, null, true), out value))
			{
				return defaultValue;
			}
			return value;
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x00017310 File Offset: 0x00015510
		public static bool TryGetFloat(string[] array, int index, out float value, out string error, [CallerArgumentExpression("value")] string name = null)
		{
			string raw;
			if (!ArgUtility.TryGet(array, index, out raw, out error, false, name))
			{
				value = 0f;
				return false;
			}
			if (!float.TryParse(raw, out value))
			{
				value = 0f;
				error = ArgUtility.GetValueParseError(array, index, name, true, "a number");
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x0001735C File Offset: 0x0001555C
		public static bool TryGetOptionalFloat(string[] array, int index, out float value, out string error, float defaultValue = 0f, [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null || index < 0 || index >= array.Length || array[index] == string.Empty)
			{
				error = null;
				value = defaultValue;
				return true;
			}
			if (!float.TryParse(array[index], out value))
			{
				error = ArgUtility.GetValueParseError(array, index, name, false, "a float");
				value = defaultValue;
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x000173B4 File Offset: 0x000155B4
		public static int GetInt(string[] array, int index, int defaultValue = 0)
		{
			int value;
			if (!int.TryParse(ArgUtility.Get(array, index, null, true), out value))
			{
				return defaultValue;
			}
			return value;
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x000173D8 File Offset: 0x000155D8
		public static bool TryGetInt(string[] array, int index, out int value, out string error, [CallerArgumentExpression("value")] string name = null)
		{
			string raw;
			if (!ArgUtility.TryGet(array, index, out raw, out error, false, name))
			{
				value = 0;
				return false;
			}
			if (!int.TryParse(raw, out value))
			{
				value = 0;
				error = ArgUtility.GetValueParseError(array, index, name, true, "an integer");
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0001741C File Offset: 0x0001561C
		public static bool TryGetOptionalInt(string[] array, int index, out int value, out string error, int defaultValue = 0, [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null || index < 0 || index >= array.Length || array[index] == string.Empty)
			{
				error = null;
				value = defaultValue;
				return true;
			}
			if (!int.TryParse(array[index], out value))
			{
				error = ArgUtility.GetValueParseError(array, index, name, false, "an integer");
				value = defaultValue;
				return false;
			}
			error = null;
			return true;
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00017474 File Offset: 0x00015674
		public static bool TryGetPoint(string[] array, int index, out Point value, out string error, [CallerArgumentExpression("value")] string name = null)
		{
			int x;
			int y;
			if (!ArgUtility.TryGetInt(array, index, out x, out error, (name != null) ? (name + " > x") : null) || !ArgUtility.TryGetInt(array, index + 1, out y, out error, (name != null) ? (name + " > y") : null))
			{
				value = Point.Zero;
				return false;
			}
			error = null;
			value = new Point(x, y);
			return true;
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x000174E0 File Offset: 0x000156E0
		public static bool TryGetRectangle(string[] array, int index, out Rectangle value, out string error, [CallerArgumentExpression("value")] string name = null)
		{
			int x;
			int y;
			int width;
			int height;
			if (!ArgUtility.TryGetInt(array, index, out x, out error, (name != null) ? (name + " > x") : null) || !ArgUtility.TryGetInt(array, index + 1, out y, out error, (name != null) ? (name + " > y") : null) || !ArgUtility.TryGetInt(array, index + 2, out width, out error, (name != null) ? (name + " > width") : null) || !ArgUtility.TryGetInt(array, index + 3, out height, out error, (name != null) ? (name + " > height") : null))
			{
				value = Rectangle.Empty;
				return false;
			}
			error = null;
			value = new Rectangle(x, y, width, height);
			return true;
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00017590 File Offset: 0x00015790
		public static bool TryGetVector2(string[] array, int index, out Vector2 value, out string error, bool integerOnly = false, [CallerArgumentExpression("value")] string name = null)
		{
			string xName = (name != null) ? (name + " > x") : null;
			string yName = (name != null) ? (name + " > y") : null;
			float x2;
			float y2;
			if (integerOnly)
			{
				int x;
				int y;
				if (ArgUtility.TryGetInt(array, index, out x, out error, xName) && ArgUtility.TryGetInt(array, index + 1, out y, out error, yName))
				{
					value = new Vector2((float)x, (float)y);
					return true;
				}
			}
			else if (ArgUtility.TryGetFloat(array, index, out x2, out error, xName) && ArgUtility.TryGetFloat(array, index + 1, out y2, out error, yName))
			{
				value = new Vector2(x2, y2);
				return true;
			}
			value = Vector2.Zero;
			return false;
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0001762F File Offset: 0x0001582F
		public static string GetRemainder(string[] array, int index, string defaultValue = null, char delimiter = ' ')
		{
			if (array == null || index < 0 || index >= array.Length)
			{
				return defaultValue;
			}
			if (array.Length - index == 1)
			{
				return array[index];
			}
			return string.Join(delimiter, RuntimeHelpers.GetSubArray<string>(array, Range.StartAt(index)));
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00017664 File Offset: 0x00015864
		public static bool TryGetRemainder(string[] array, int index, out string value, out string error, char delimiter = ' ', [CallerArgumentExpression("value")] string name = null)
		{
			if (array == null)
			{
				value = null;
				error = "argument list is null";
				return false;
			}
			if (index < 0 || index >= array.Length)
			{
				value = null;
				error = ArgUtility.GetMissingRequiredIndexError(array, index, name);
				return false;
			}
			if (array.Length - index == 1)
			{
				value = array[index];
			}
			else
			{
				value = string.Join(delimiter, RuntimeHelpers.GetSubArray<string>(array, Range.StartAt(index)));
			}
			error = null;
			return true;
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x000176C7 File Offset: 0x000158C7
		public static bool TryGetOptionalRemainder(string[] array, int index, out string value, string defaultValue = null, char delimiter = ' ')
		{
			if (array == null || index < 0 || index >= array.Length)
			{
				value = defaultValue;
				return true;
			}
			if (array.Length - index == 1)
			{
				value = array[index];
			}
			else
			{
				value = string.Join(delimiter, RuntimeHelpers.GetSubArray<string>(array, Range.StartAt(index)));
			}
			return true;
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x00017708 File Offset: 0x00015908
		internal static string GetMissingRequiredIndexError(string[] array, int index, string name)
		{
			int num = array.Length;
			if (num == 0)
			{
				return "required " + ArgUtility.GetFieldLabel(index, name) + " not found (list is empty)";
			}
			if (num != 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
				defaultInterpolatedStringHandler.AppendLiteral("required ");
				defaultInterpolatedStringHandler.AppendFormatted(ArgUtility.GetFieldLabel(index, name));
				defaultInterpolatedStringHandler.AppendLiteral(" not found (list has indexes 0 through ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(array.Length - 1);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return "required " + ArgUtility.GetFieldLabel(index, name) + " not found (list has a single value at index 0)";
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x000177A0 File Offset: 0x000159A0
		internal static string GetValueParseError(string[] array, int index, string name, bool required, string typeSummary)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 4);
			defaultInterpolatedStringHandler.AppendFormatted(required ? "required" : "optional");
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted(ArgUtility.GetFieldLabel(index, name));
			defaultInterpolatedStringHandler.AppendLiteral(" has value '");
			defaultInterpolatedStringHandler.AppendFormatted(array[index]);
			defaultInterpolatedStringHandler.AppendLiteral("', which can't be parsed as ");
			defaultInterpolatedStringHandler.AppendFormatted(typeSummary);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0001781C File Offset: 0x00015A1C
		private static string GetFieldLabel(int index, string name)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (name == null)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
				defaultInterpolatedStringHandler.AppendLiteral("index ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(index);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
			defaultInterpolatedStringHandler.AppendLiteral("index ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(index);
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}

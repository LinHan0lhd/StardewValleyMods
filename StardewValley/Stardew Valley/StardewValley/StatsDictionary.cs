using System;
using StardewValley.Extensions;

namespace StardewValley
{
	// Token: 0x02000108 RID: 264
	public class StatsDictionary<TValue> : SerializableDictionaryWithCaseInsensitiveKeys<TValue>
	{
		// Token: 0x0600152A RID: 5418 RVA: 0x000F9638 File Offset: 0x000F7838
		protected override void AddDuringDeserialization(string key, TValue value)
		{
			TValue oldValue;
			if (!base.TryGetValue(key, out oldValue))
			{
				base.AddDuringDeserialization(key, value);
				return;
			}
			long valueLong = Convert.ToInt64(value);
			long oldValueLong = Convert.ToInt64(oldValue);
			if (key.EqualsIgnoreCase("averageBedtime"))
			{
				if (oldValueLong == 0L)
				{
					base[key] = value;
					return;
				}
			}
			else
			{
				base[key] = (TValue)((object)Convert.ChangeType(oldValueLong + valueLong, typeof(TValue)));
			}
		}
	}
}

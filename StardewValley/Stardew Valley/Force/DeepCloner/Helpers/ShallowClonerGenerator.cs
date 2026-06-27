using System;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x0200006F RID: 111
	internal static class ShallowClonerGenerator
	{
		// Token: 0x06000466 RID: 1126 RVA: 0x00014E44 File Offset: 0x00013044
		public static T CloneObject<T>(T obj)
		{
			if (obj is ValueType)
			{
				if (typeof(T) == obj.GetType())
				{
					return obj;
				}
				return (T)((object)ShallowObjectCloner.CloneObject(obj));
			}
			else
			{
				if (obj == null)
				{
					return (T)((object)null);
				}
				if (DeepClonerSafeTypes.CanReturnSameObject(obj.GetType()))
				{
					return obj;
				}
				return (T)((object)ShallowObjectCloner.CloneObject(obj));
			}
		}
	}
}

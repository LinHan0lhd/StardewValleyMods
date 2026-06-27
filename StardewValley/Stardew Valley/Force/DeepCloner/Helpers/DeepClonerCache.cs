using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x02000069 RID: 105
	internal static class DeepClonerCache
	{
		// Token: 0x06000436 RID: 1078 RVA: 0x00013AB8 File Offset: 0x00011CB8
		public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
		{
			object value;
			if (DeepClonerCache._typeCache.TryGetValue(type, out value))
			{
				return value;
			}
			lock (type)
			{
				value = DeepClonerCache._typeCache.GetOrAdd(type, ([Nullable(1)] Type t) => adder(t));
			}
			return value;
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00013B24 File Offset: 0x00011D24
		public static object GetOrAddDeepClassTo<T>(Type type, Func<Type, T> adder)
		{
			object value;
			if (DeepClonerCache._typeCacheDeepTo.TryGetValue(type, out value))
			{
				return value;
			}
			lock (type)
			{
				value = DeepClonerCache._typeCacheDeepTo.GetOrAdd(type, ([Nullable(1)] Type t) => adder(t));
			}
			return value;
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x00013B90 File Offset: 0x00011D90
		public static object GetOrAddShallowClassTo<T>(Type type, Func<Type, T> adder)
		{
			object value;
			if (DeepClonerCache._typeCacheShallowTo.TryGetValue(type, out value))
			{
				return value;
			}
			lock (type)
			{
				value = DeepClonerCache._typeCacheShallowTo.GetOrAdd(type, ([Nullable(1)] Type t) => adder(t));
			}
			return value;
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00013BFC File Offset: 0x00011DFC
		public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
		{
			object value;
			if (DeepClonerCache._structAsObjectCache.TryGetValue(type, out value))
			{
				return value;
			}
			lock (type)
			{
				value = DeepClonerCache._structAsObjectCache.GetOrAdd(type, ([Nullable(1)] Type t) => adder(t));
			}
			return value;
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x00013C68 File Offset: 0x00011E68
		public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
		{
			return (T)((object)DeepClonerCache._typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), ([Nullable(new byte[]
			{
				1,
				0,
				0
			})] Tuple<Type, Type> tuple) => adder(tuple.Item1, tuple.Item2)));
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00013CA4 File Offset: 0x00011EA4
		public static void ClearCache()
		{
			DeepClonerCache._typeCache.Clear();
			DeepClonerCache._typeCacheDeepTo.Clear();
			DeepClonerCache._typeCacheShallowTo.Clear();
			DeepClonerCache._structAsObjectCache.Clear();
			DeepClonerCache._typeConvertCache.Clear();
		}

		// Token: 0x04000198 RID: 408
		private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

		// Token: 0x04000199 RID: 409
		private static readonly ConcurrentDictionary<Type, object> _typeCacheDeepTo = new ConcurrentDictionary<Type, object>();

		// Token: 0x0400019A RID: 410
		private static readonly ConcurrentDictionary<Type, object> _typeCacheShallowTo = new ConcurrentDictionary<Type, object>();

		// Token: 0x0400019B RID: 411
		private static readonly ConcurrentDictionary<Type, object> _structAsObjectCache = new ConcurrentDictionary<Type, object>();

		// Token: 0x0400019C RID: 412
		private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache = new ConcurrentDictionary<Tuple<Type, Type>, object>();
	}
}

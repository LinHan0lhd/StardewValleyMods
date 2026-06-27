using System;
using System.Linq;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x0200006B RID: 107
	internal static class DeepClonerGenerator
	{
		// Token: 0x06000443 RID: 1091 RVA: 0x000143FC File Offset: 0x000125FC
		public static T CloneObject<T>(T obj)
		{
			if (obj is ValueType)
			{
				Type type = obj.GetType();
				if (typeof(T) == type)
				{
					if (DeepClonerSafeTypes.CanReturnSameObject(type))
					{
						return obj;
					}
					return DeepClonerGenerator.CloneStructInternal<T>(obj, new DeepCloneState());
				}
			}
			return (T)((object)DeepClonerGenerator.CloneClassRoot(obj));
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x0001445C File Offset: 0x0001265C
		private static object CloneClassRoot(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			Func<object, DeepCloneState, object> cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass<object>(obj.GetType(), (Type t) => DeepClonerGenerator.GenerateCloner(t, true));
			if (cloner == null)
			{
				return obj;
			}
			return cloner(obj, new DeepCloneState());
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x000144B0 File Offset: 0x000126B0
		internal static object CloneClassInternal(object obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			Func<object, DeepCloneState, object> cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass<object>(obj.GetType(), (Type t) => DeepClonerGenerator.GenerateCloner(t, true));
			if (cloner == null)
			{
				return obj;
			}
			object knownRef = state.GetKnownRef(obj);
			if (knownRef != null)
			{
				return knownRef;
			}
			return cloner(obj, state);
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x0001450C File Offset: 0x0001270C
		private static T CloneStructInternal<T>(T obj, DeepCloneState state)
		{
			Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
			if (cloner == null)
			{
				return obj;
			}
			return cloner(obj, state);
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x0001452C File Offset: 0x0001272C
		internal static T[] Clone1DimArraySafeInternal<T>(T[] obj, DeepCloneState state)
		{
			T[] outArray = new T[obj.Length];
			state.AddKnownRef(obj, outArray);
			Array.Copy(obj, outArray, obj.Length);
			return outArray;
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00014558 File Offset: 0x00012758
		internal static T[] Clone1DimArrayStructInternal<T>(T[] obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int i = obj.Length;
			T[] outArray = new T[i];
			state.AddKnownRef(obj, outArray);
			Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
			for (int j = 0; j < i; j++)
			{
				outArray[j] = cloner(obj[j], state);
			}
			return outArray;
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x000145A8 File Offset: 0x000127A8
		internal static T[] Clone1DimArrayClassInternal<T>(T[] obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int i = obj.Length;
			T[] outArray = new T[i];
			state.AddKnownRef(obj, outArray);
			for (int j = 0; j < i; j++)
			{
				outArray[j] = (T)((object)DeepClonerGenerator.CloneClassInternal(obj[j], state));
			}
			return outArray;
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x000145F8 File Offset: 0x000127F8
		internal static T[,] Clone2DimArrayInternal<T>(T[,] obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int l = obj.GetLength(0);
			int l2 = obj.GetLength(1);
			T[,] outArray = new T[l, l2];
			state.AddKnownRef(obj, outArray);
			if (DeepClonerSafeTypes.CanReturnSameObject(typeof(T)))
			{
				Array.Copy(obj, outArray, obj.Length);
				return outArray;
			}
			if (typeof(T).IsValueType())
			{
				Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
				for (int i = 0; i < l; i++)
				{
					for (int j = 0; j < l2; j++)
					{
						outArray[i, j] = cloner(obj[i, j], state);
					}
				}
			}
			else
			{
				for (int k = 0; k < l; k++)
				{
					for (int m = 0; m < l2; m++)
					{
						outArray[k, m] = (T)((object)DeepClonerGenerator.CloneClassInternal(obj[k, m], state));
					}
				}
			}
			return outArray;
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x000146E4 File Offset: 0x000128E4
		internal static Array CloneAbstractArrayInternal(Array obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int rank = obj.Rank;
			int[] lowerBounds = Enumerable.Range(0, rank).Select(new Func<int, int>(obj.GetLowerBound)).ToArray<int>();
			int[] lengths = Enumerable.Range(0, rank).Select(new Func<int, int>(obj.GetLength)).ToArray<int>();
			int[] idxes = Enumerable.Range(0, rank).Select(new Func<int, int>(obj.GetLowerBound)).ToArray<int>();
			Array outArray = Array.CreateInstance(obj.GetType().GetElementType(), lengths, lowerBounds);
			state.AddKnownRef(obj, outArray);
			for (;;)
			{
				outArray.SetValue(DeepClonerGenerator.CloneClassInternal(obj.GetValue(idxes), state), idxes);
				int ofs = rank - 1;
				for (;;)
				{
					idxes[ofs]++;
					if (idxes[ofs] < lowerBounds[ofs] + lengths[ofs])
					{
						break;
					}
					idxes[ofs] = lowerBounds[ofs];
					ofs--;
					if (ofs < 0)
					{
						return outArray;
					}
				}
			}
			return outArray;
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x000147BF File Offset: 0x000129BF
		internal static Func<T, DeepCloneState, T> GetClonerForValueType<T>()
		{
			return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddStructAsObject<object>(typeof(T), (Type t) => DeepClonerGenerator.GenerateCloner(t, false));
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x000147F4 File Offset: 0x000129F4
		private static object GenerateCloner(Type t, bool asObject)
		{
			if (DeepClonerSafeTypes.CanReturnSameObject(t) && asObject && !t.IsValueType())
			{
				return null;
			}
			return DeepClonerExprGenerator.GenerateClonerInternal(t, asObject);
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x00014814 File Offset: 0x00012A14
		public static object CloneObjectTo(object objFrom, object objTo, bool isDeep)
		{
			if (objTo == null)
			{
				return null;
			}
			if (objFrom == null)
			{
				throw new ArgumentNullException("objFrom", "Cannot copy null object to another");
			}
			Type type = objFrom.GetType();
			if (!type.IsInstanceOfType(objTo))
			{
				throw new InvalidOperationException("From object should be derived from From object, but From object has type " + objFrom.GetType().FullName + " and to " + objTo.GetType().FullName);
			}
			if (objFrom is string)
			{
				throw new InvalidOperationException("It is forbidden to clone strings");
			}
			object obj;
			if (!isDeep)
			{
				obj = DeepClonerCache.GetOrAddShallowClassTo<object>(type, (Type t) => ClonerToExprGenerator.GenerateClonerInternal(t, false));
			}
			else
			{
				obj = DeepClonerCache.GetOrAddDeepClassTo<object>(type, (Type t) => ClonerToExprGenerator.GenerateClonerInternal(t, true));
			}
			Func<object, object, DeepCloneState, object> cloner = (Func<object, object, DeepCloneState, object>)obj;
			if (cloner == null)
			{
				return objTo;
			}
			return cloner(objFrom, objTo, new DeepCloneState());
		}
	}
}

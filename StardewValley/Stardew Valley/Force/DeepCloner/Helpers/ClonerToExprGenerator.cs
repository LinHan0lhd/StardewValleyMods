using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x02000068 RID: 104
	internal static class ClonerToExprGenerator
	{
		// Token: 0x0600042D RID: 1069 RVA: 0x000131CC File Offset: 0x000113CC
		internal static object GenerateClonerInternal(Type realType, bool isDeepClone)
		{
			if (realType.IsValueType())
			{
				throw new InvalidOperationException("Operation is valid only for reference types");
			}
			return ClonerToExprGenerator.GenerateProcessMethod(realType, isDeepClone);
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x000131E8 File Offset: 0x000113E8
		private static object GenerateProcessMethod(Type type, bool isDeepClone)
		{
			if (type.IsArray)
			{
				return ClonerToExprGenerator.GenerateProcessArrayMethod(type, isDeepClone);
			}
			Type methodType = typeof(object);
			List<Expression> expressionList = new List<Expression>();
			ParameterExpression from = Expression.Parameter(methodType);
			ParameterExpression to = Expression.Parameter(methodType);
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			ParameterExpression fromLocal = Expression.Variable(type);
			ParameterExpression toLocal = Expression.Variable(type);
			expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));
			expressionList.Add(Expression.Assign(toLocal, Expression.Convert(to, type)));
			if (isDeepClone)
			{
				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, to));
			}
			List<FieldInfo> fi = new List<FieldInfo>();
			Type tp = type;
			while (!(tp.Name == "ContextBoundObject"))
			{
				fi.AddRange(tp.GetDeclaredFields());
				tp = tp.BaseType();
				if (!(tp != null))
				{
					break;
				}
			}
			foreach (FieldInfo fieldInfo in fi)
			{
				if (isDeepClone && !DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
				{
					MethodInfo method = fieldInfo.FieldType.IsValueType() ? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal").MakeGenericMethod(new Type[]
					{
						fieldInfo.FieldType
					}) : typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal");
					MemberExpression get = Expression.Field(fromLocal, fieldInfo);
					Expression call = Expression.Call(method, get, state);
					if (!fieldInfo.FieldType.IsValueType())
					{
						call = Expression.Convert(call, fieldInfo.FieldType);
					}
					if (fieldInfo.IsInitOnly)
					{
						MethodInfo setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
						expressionList.Add(Expression.Call(setMethod, Expression.Constant(fieldInfo), Expression.Convert(toLocal, typeof(object)), Expression.Convert(call, typeof(object))));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
					}
				}
				else
				{
					expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), Expression.Field(fromLocal, fieldInfo)));
				}
			}
			expressionList.Add(Expression.Convert(toLocal, methodType));
			Type delegateType = typeof(Func<, , , >).MakeGenericType(new Type[]
			{
				methodType,
				methodType,
				typeof(DeepCloneState),
				methodType
			});
			List<ParameterExpression> blockParams = new List<ParameterExpression>();
			if (from != fromLocal)
			{
				blockParams.Add(fromLocal);
			}
			if (to != toLocal)
			{
				blockParams.Add(toLocal);
			}
			return Expression.Lambda(delegateType, Expression.Block(blockParams, expressionList), new ParameterExpression[]
			{
				from,
				to,
				state
			}).Compile();
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x000134C8 File Offset: 0x000116C8
		private static object GenerateProcessArrayMethod(Type type, bool isDeep)
		{
			Type elementType = type.GetElementType();
			int rank = type.GetArrayRank();
			ParameterExpression from = Expression.Parameter(typeof(object));
			ParameterExpression to = Expression.Parameter(typeof(object));
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			Type funcType = typeof(Func<, , , >).MakeGenericType(new Type[]
			{
				typeof(object),
				typeof(object),
				typeof(DeepCloneState),
				typeof(object)
			});
			if (rank != 1 || !(type == elementType.MakeArrayType()))
			{
				MethodCallExpression callS = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod((rank == 2 && type == elementType.MakeArrayType()) ? "Clone2DimArrayInternal" : "CloneAbstractArrayInternal"), Expression.Convert(from, type), Expression.Convert(to, type), state, Expression.Constant(isDeep));
				return Expression.Lambda(funcType, callS, new ParameterExpression[]
				{
					from,
					to,
					state
				}).Compile();
			}
			if (!isDeep)
			{
				MethodCallExpression callS2 = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod("ShallowClone1DimArraySafeInternal").MakeGenericMethod(new Type[]
				{
					elementType
				}), Expression.Convert(from, type), Expression.Convert(to, type));
				return Expression.Lambda(funcType, callS2, new ParameterExpression[]
				{
					from,
					to,
					state
				}).Compile();
			}
			string methodName = "Clone1DimArrayClassInternal";
			if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
			{
				methodName = "Clone1DimArraySafeInternal";
			}
			else if (elementType.IsValueType())
			{
				methodName = "Clone1DimArrayStructInternal";
			}
			MethodCallExpression callS3 = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(new Type[]
			{
				elementType
			}), Expression.Convert(from, type), Expression.Convert(to, type), state);
			return Expression.Lambda(funcType, callS3, new ParameterExpression[]
			{
				from,
				to,
				state
			}).Compile();
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x000136C4 File Offset: 0x000118C4
		internal static T[] ShallowClone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo)
		{
			int i = Math.Min(objFrom.Length, objTo.Length);
			Array.Copy(objFrom, objTo, i);
			return objTo;
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x000136E8 File Offset: 0x000118E8
		internal static T[] Clone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
		{
			int i = Math.Min(objFrom.Length, objTo.Length);
			state.AddKnownRef(objFrom, objTo);
			Array.Copy(objFrom, objTo, i);
			return objTo;
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00013714 File Offset: 0x00011914
		internal static T[] Clone1DimArrayStructInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int i = Math.Min(objFrom.Length, objTo.Length);
			state.AddKnownRef(objFrom, objTo);
			Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
			for (int j = 0; j < i; j++)
			{
				objTo[j] = cloner(objTo[j], state);
			}
			return objTo;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x00013768 File Offset: 0x00011968
		internal static T[] Clone1DimArrayClassInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int i = Math.Min(objFrom.Length, objTo.Length);
			state.AddKnownRef(objFrom, objTo);
			for (int j = 0; j < i; j++)
			{
				objTo[j] = (T)((object)DeepClonerGenerator.CloneClassInternal(objFrom[j], state));
			}
			return objTo;
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x000137BC File Offset: 0x000119BC
		internal static T[,] Clone2DimArrayInternal<T>(T[,] objFrom, T[,] objTo, DeepCloneState state, bool isDeep)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int l = Math.Min(objFrom.GetLength(0), objTo.GetLength(0));
			int l2 = Math.Min(objFrom.GetLength(1), objTo.GetLength(1));
			state.AddKnownRef(objFrom, objTo);
			if ((!isDeep || DeepClonerSafeTypes.CanReturnSameObject(typeof(T))) && objFrom.GetLength(0) == objTo.GetLength(0) && objFrom.GetLength(1) == objTo.GetLength(1))
			{
				Array.Copy(objFrom, objTo, objFrom.Length);
				return objTo;
			}
			if (!isDeep)
			{
				for (int i = 0; i < l; i++)
				{
					for (int j = 0; j < l2; j++)
					{
						objTo[i, j] = objFrom[i, j];
					}
				}
				return objTo;
			}
			if (typeof(T).IsValueType())
			{
				Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
				for (int k = 0; k < l; k++)
				{
					for (int m = 0; m < l2; m++)
					{
						objTo[k, m] = cloner(objFrom[k, m], state);
					}
				}
			}
			else
			{
				for (int n = 0; n < l; n++)
				{
					for (int k2 = 0; k2 < l2; k2++)
					{
						objTo[n, k2] = (T)((object)DeepClonerGenerator.CloneClassInternal(objFrom[n, k2], state));
					}
				}
			}
			return objTo;
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0001390C File Offset: 0x00011B0C
		internal static Array CloneAbstractArrayInternal(Array objFrom, Array objTo, DeepCloneState state, bool isDeep)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int rank = objFrom.Rank;
			if (objTo.Rank != rank)
			{
				throw new InvalidOperationException("Invalid rank of target array");
			}
			int[] lowerBoundsFrom = Enumerable.Range(0, rank).Select(new Func<int, int>(objFrom.GetLowerBound)).ToArray<int>();
			int[] lowerBoundsTo = Enumerable.Range(0, rank).Select(new Func<int, int>(objTo.GetLowerBound)).ToArray<int>();
			int[] lengths = (from x in Enumerable.Range(0, rank)
			select Math.Min(objFrom.GetLength(x), objTo.GetLength(x))).ToArray<int>();
			int[] idxesFrom = Enumerable.Range(0, rank).Select(new Func<int, int>(objFrom.GetLowerBound)).ToArray<int>();
			int[] idxesTo = Enumerable.Range(0, rank).Select(new Func<int, int>(objTo.GetLowerBound)).ToArray<int>();
			state.AddKnownRef(objFrom, objTo);
			for (;;)
			{
				if (isDeep)
				{
					objTo.SetValue(DeepClonerGenerator.CloneClassInternal(objFrom.GetValue(idxesFrom), state), idxesTo);
				}
				else
				{
					objTo.SetValue(objFrom.GetValue(idxesFrom), idxesTo);
				}
				int ofs = rank - 1;
				for (;;)
				{
					idxesFrom[ofs]++;
					idxesTo[ofs]++;
					if (idxesFrom[ofs] < lowerBoundsFrom[ofs] + lengths[ofs])
					{
						break;
					}
					idxesFrom[ofs] = lowerBoundsFrom[ofs];
					idxesTo[ofs] = lowerBoundsTo[ofs];
					ofs--;
					if (ofs < 0)
					{
						goto Block_5;
					}
				}
			}
			Block_5:
			return objTo;
		}
	}
}

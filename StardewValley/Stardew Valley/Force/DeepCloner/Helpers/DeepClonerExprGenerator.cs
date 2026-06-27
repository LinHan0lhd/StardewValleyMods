using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x0200006A RID: 106
	internal static class DeepClonerExprGenerator
	{
		// Token: 0x0600043D RID: 1085 RVA: 0x00013D0C File Offset: 0x00011F0C
		internal static object GenerateClonerInternal(Type realType, bool asObject)
		{
			return DeepClonerExprGenerator.GenerateProcessMethod(realType, asObject && realType.IsValueType());
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00013D20 File Offset: 0x00011F20
		internal static void ForceSetField(FieldInfo field, object obj, object value)
		{
			FieldInfo fieldInfo = field.GetType().GetPrivateField("m_fieldAttributes");
			if (fieldInfo == null)
			{
				return;
			}
			object ov = fieldInfo.GetValue(field);
			if (!(ov is FieldAttributes))
			{
				return;
			}
			FieldAttributes v = (FieldAttributes)ov;
			FieldInfo obj2 = fieldInfo;
			lock (obj2)
			{
				fieldInfo.SetValue(field, v & ~FieldAttributes.InitOnly);
				field.SetValue(obj, value);
				fieldInfo.SetValue(field, v | FieldAttributes.InitOnly);
			}
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00013DB4 File Offset: 0x00011FB4
		private static object GenerateProcessMethod(Type type, bool unboxStruct)
		{
			if (type.IsArray)
			{
				return DeepClonerExprGenerator.GenerateProcessArrayMethod(type);
			}
			if (type.FullName != null && type.FullName.StartsWith("System.Tuple`"))
			{
				Type[] genericArguments = type.GenericArguments();
				if (genericArguments.Length < 10 && genericArguments.All(new Func<Type, bool>(DeepClonerSafeTypes.CanReturnSameObject)))
				{
					return DeepClonerExprGenerator.GenerateProcessTupleMethod(type);
				}
			}
			Type methodType = (unboxStruct || type.IsClass()) ? typeof(object) : type;
			List<Expression> expressionList = new List<Expression>();
			ParameterExpression from = Expression.Parameter(methodType);
			ParameterExpression fromLocal = from;
			ParameterExpression toLocal = Expression.Variable(type);
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			if (!type.IsValueType())
			{
				MethodInfo methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");
				expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));
				fromLocal = Expression.Variable(type);
				expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));
				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, toLocal));
			}
			else if (unboxStruct)
			{
				expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
				fromLocal = Expression.Variable(type);
				expressionList.Add(Expression.Assign(fromLocal, toLocal));
			}
			else
			{
				expressionList.Add(Expression.Assign(toLocal, from));
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
				if (!DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
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
					if (DeepClonerExprGenerator._readonlyFields.GetOrAdd(fieldInfo, (FieldInfo f) => f.IsInitOnly))
					{
						MethodInfo setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
						expressionList.Add(Expression.Call(setMethod, Expression.Constant(fieldInfo), Expression.Convert(toLocal, typeof(object)), Expression.Convert(call, typeof(object))));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
					}
				}
			}
			expressionList.Add(Expression.Convert(toLocal, methodType));
			Type delegateType = typeof(Func<, , >).MakeGenericType(new Type[]
			{
				methodType,
				typeof(DeepCloneState),
				methodType
			});
			List<ParameterExpression> blockParams = new List<ParameterExpression>();
			if (from != fromLocal)
			{
				blockParams.Add(fromLocal);
			}
			blockParams.Add(toLocal);
			return Expression.Lambda(delegateType, Expression.Block(blockParams, expressionList), new ParameterExpression[]
			{
				from,
				state
			}).Compile();
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00014130 File Offset: 0x00012330
		private static object GenerateProcessArrayMethod(Type type)
		{
			Type elementType = type.GetElementType();
			int rank = type.GetArrayRank();
			MethodInfo methodInfo;
			if (rank != 1 || type != elementType.MakeArrayType())
			{
				if (rank == 2 && type == elementType.MakeArrayType())
				{
					methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod("Clone2DimArrayInternal").MakeGenericMethod(new Type[]
					{
						elementType
					});
				}
				else
				{
					methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneAbstractArrayInternal");
				}
			}
			else
			{
				string methodName = "Clone1DimArrayClassInternal";
				if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
				{
					methodName = "Clone1DimArraySafeInternal";
				}
				else if (elementType.IsValueType())
				{
					methodName = "Clone1DimArrayStructInternal";
				}
				methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(new Type[]
				{
					elementType
				});
			}
			ParameterExpression from = Expression.Parameter(typeof(object));
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			MethodCallExpression call = Expression.Call(methodInfo, Expression.Convert(from, type), state);
			return Expression.Lambda(typeof(Func<, , >).MakeGenericType(new Type[]
			{
				typeof(object),
				typeof(DeepCloneState),
				typeof(object)
			}), call, new ParameterExpression[]
			{
				from,
				state
			}).Compile();
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x00014280 File Offset: 0x00012480
		private static object GenerateProcessTupleMethod(Type type)
		{
			ParameterExpression from = Expression.Parameter(typeof(object));
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			ParameterExpression local = Expression.Variable(type);
			BinaryExpression assign = Expression.Assign(local, Expression.Convert(from, type));
			Type typeFromHandle = typeof(Func<object, DeepCloneState, object>);
			int tupleLength = type.GenericArguments().Length;
			BinaryExpression constructor = Expression.Assign(local, Expression.New(type.GetPublicConstructors().First((ConstructorInfo x) => x.GetParameters().Length == tupleLength), from x in type.GetPublicProperties()
			orderby x.Name
			where x.CanRead && x.Name.StartsWith("Item") && char.IsDigit(x.Name[4])
			select Expression.Property(local, x.Name)));
			return Expression.Lambda(typeFromHandle, Expression.Block(new ParameterExpression[]
			{
				local
			}, new Expression[]
			{
				assign,
				constructor,
				Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, local),
				from
			}), new ParameterExpression[]
			{
				from,
				state
			}).Compile();
		}

		// Token: 0x0400019D RID: 413
		private static readonly ConcurrentDictionary<FieldInfo, bool> _readonlyFields = new ConcurrentDictionary<FieldInfo, bool>();

		// Token: 0x0400019E RID: 414
		private static FieldInfo _attributesFieldInfo = typeof(FieldInfo).GetPrivateField("m_fieldAttributes");
	}
}

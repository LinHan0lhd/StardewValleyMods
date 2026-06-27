using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StardewValley.Internal
{
	// Token: 0x02000317 RID: 791
	public static class StaticDelegateBuilder
	{
		// Token: 0x06003441 RID: 13377 RVA: 0x0029C154 File Offset: 0x0029A354
		public static bool TryCreateDelegate<TDelegate>(string fullMethodName, out TDelegate createdDelegate, out string error) where TDelegate : Delegate
		{
			if (string.IsNullOrWhiteSpace(fullMethodName))
			{
				error = "the method name can't be empty";
				createdDelegate = default(TDelegate);
				return false;
			}
			Dictionary<string, StaticDelegateBuilder.CachedDelegate> cacheByName;
			if (!StaticDelegateBuilder.CachedDelegates.TryGetValue(typeof(TDelegate), out cacheByName))
			{
				cacheByName = (StaticDelegateBuilder.CachedDelegates[typeof(TDelegate)] = new Dictionary<string, StaticDelegateBuilder.CachedDelegate>());
			}
			StaticDelegateBuilder.CachedDelegate cached;
			if (!cacheByName.TryGetValue(fullMethodName, out cached))
			{
				string[] parts = LegacyShims.SplitAndTrim(fullMethodName, ':', StringSplitOptions.None);
				if (parts.Length != 2)
				{
					error = "invalid method name format, expected a type full name and method separated with a colon (:)";
					createdDelegate = default(TDelegate);
					return false;
				}
				string fullTypeName = parts[0];
				string methodName = parts[1];
				if (Game1.GameAssemblyName != "Stardew Valley" && fullTypeName.Contains("Stardew Valley"))
				{
					string[] parts2 = LegacyShims.SplitAndTrim(fullTypeName, ',', StringSplitOptions.None);
					if (ArgUtility.Get(parts2, 1, null, true) == "Stardew Valley")
					{
						parts2[1] = Game1.GameAssemblyName;
						fullTypeName = string.Join(", ", parts2);
					}
				}
				Type type = Type.GetType(fullTypeName);
				if (type == null)
				{
					error = "could not find type '" + fullTypeName + "'";
					createdDelegate = default(TDelegate);
					return false;
				}
				MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 2);
					defaultInterpolatedStringHandler.AppendLiteral("could not find method '");
					defaultInterpolatedStringHandler.AppendFormatted(methodName);
					defaultInterpolatedStringHandler.AppendLiteral("' on type '");
					defaultInterpolatedStringHandler.AppendFormatted(fullTypeName);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					error = defaultInterpolatedStringHandler.ToStringAndClear();
					createdDelegate = default(TDelegate);
					return false;
				}
				if (!method.IsStatic)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
					defaultInterpolatedStringHandler.AppendLiteral("found method '");
					defaultInterpolatedStringHandler.AppendFormatted(methodName);
					defaultInterpolatedStringHandler.AppendLiteral("' on type '");
					defaultInterpolatedStringHandler.AppendFormatted(fullTypeName);
					defaultInterpolatedStringHandler.AppendLiteral("', but the method isn't static");
					error = defaultInterpolatedStringHandler.ToStringAndClear();
					createdDelegate = default(TDelegate);
					return false;
				}
				try
				{
					createdDelegate = (TDelegate)((object)Delegate.CreateDelegate(typeof(TDelegate), null, method));
					error = null;
				}
				catch (ArgumentException)
				{
					MethodInfo delegateMethod = typeof(TDelegate).GetMethod("Invoke");
					createdDelegate = default(TDelegate);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 3);
					defaultInterpolatedStringHandler.AppendLiteral("failed to bind method '");
					defaultInterpolatedStringHandler.AppendFormatted(fullMethodName);
					defaultInterpolatedStringHandler.AppendLiteral("': it didn't match the expected signature ");
					defaultInterpolatedStringHandler.AppendFormatted<Type>(delegateMethod.ReturnType);
					defaultInterpolatedStringHandler.AppendLiteral(" method(");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", delegateMethod.GetParameters().Select(delegate(ParameterInfo p)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler2.AppendFormatted<Type>(p.ParameterType);
						defaultInterpolatedStringHandler2.AppendLiteral(" ");
						defaultInterpolatedStringHandler2.AppendFormatted(p.Name);
						return defaultInterpolatedStringHandler2.ToStringAndClear();
					})));
					defaultInterpolatedStringHandler.AppendLiteral(")");
					error = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				Dictionary<string, StaticDelegateBuilder.CachedDelegate> dictionary = cacheByName;
				cached = new StaticDelegateBuilder.CachedDelegate(createdDelegate, error);
				dictionary[fullMethodName] = cached;
			}
			createdDelegate = (TDelegate)((object)cached.CreatedDelegate);
			error = cached.Error;
			return createdDelegate != null;
		}

		// Token: 0x0400223A RID: 8762
		private static readonly Dictionary<Type, Dictionary<string, StaticDelegateBuilder.CachedDelegate>> CachedDelegates = new Dictionary<Type, Dictionary<string, StaticDelegateBuilder.CachedDelegate>>();

		// Token: 0x0200068D RID: 1677
		private struct CachedDelegate
		{
			// Token: 0x060045D0 RID: 17872 RVA: 0x00320CE2 File Offset: 0x0031EEE2
			public CachedDelegate(object createdDelegate, string error)
			{
				this.CreatedDelegate = createdDelegate;
				this.Error = error;
			}

			// Token: 0x04002FF5 RID: 12277
			public readonly object CreatedDelegate;

			// Token: 0x04002FF6 RID: 12278
			public readonly string Error;
		}
	}
}

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x02000070 RID: 112
	public abstract class ShallowObjectCloner
	{
		// Token: 0x06000467 RID: 1127
		protected abstract object DoCloneObject(object obj);

		// Token: 0x06000468 RID: 1128 RVA: 0x00014EC4 File Offset: 0x000130C4
		public static object CloneObject(object obj)
		{
			return ShallowObjectCloner._instance.DoCloneObject(obj);
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00014ED1 File Offset: 0x000130D1
		internal static bool IsSafeVariant()
		{
			return ShallowObjectCloner._instance is ShallowObjectCloner.ShallowSafeObjectCloner;
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x00014EE0 File Offset: 0x000130E0
		static ShallowObjectCloner()
		{
			ShallowObjectCloner._unsafeInstance = ShallowObjectCloner._instance;
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x00014EF6 File Offset: 0x000130F6
		internal static void SwitchTo(bool isSafe)
		{
			DeepClonerCache.ClearCache();
			if (isSafe)
			{
				ShallowObjectCloner._instance = new ShallowObjectCloner.ShallowSafeObjectCloner();
				return;
			}
			ShallowObjectCloner._instance = ShallowObjectCloner._unsafeInstance;
		}

		// Token: 0x040001A3 RID: 419
		private static readonly ShallowObjectCloner _unsafeInstance;

		// Token: 0x040001A4 RID: 420
		private static ShallowObjectCloner _instance = new ShallowObjectCloner.ShallowSafeObjectCloner();

		// Token: 0x02000400 RID: 1024
		private class ShallowSafeObjectCloner : ShallowObjectCloner
		{
			// Token: 0x06003A45 RID: 14917 RVA: 0x002D86C4 File Offset: 0x002D68C4
			static ShallowSafeObjectCloner()
			{
				MethodInfo methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");
				ParameterExpression p = Expression.Parameter(typeof(object));
				ShallowObjectCloner.ShallowSafeObjectCloner._cloneFunc = Expression.Lambda<Func<object, object>>(Expression.Call(p, methodInfo), new ParameterExpression[]
				{
					p
				}).Compile();
			}

			// Token: 0x06003A46 RID: 14918 RVA: 0x002D8716 File Offset: 0x002D6916
			protected override object DoCloneObject(object obj)
			{
				return ShallowObjectCloner.ShallowSafeObjectCloner._cloneFunc(obj);
			}

			// Token: 0x040026F5 RID: 9973
			private static readonly Func<object, object> _cloneFunc;
		}
	}
}

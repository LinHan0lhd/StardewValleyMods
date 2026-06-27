using System;
using System.Security;
using Force.DeepCloner.Helpers;

namespace Force.DeepCloner
{
	// Token: 0x02000067 RID: 103
	public static class DeepClonerExtensions
	{
		// Token: 0x06000427 RID: 1063 RVA: 0x00013131 File Offset: 0x00011331
		public static T DeepClone<T>(this T obj)
		{
			return DeepClonerGenerator.CloneObject<T>(obj);
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00013139 File Offset: 0x00011339
		public static TTo DeepCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, !!0
		{
			return (TTo)((object)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, true));
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00013152 File Offset: 0x00011352
		public static TTo ShallowCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, !!0
		{
			return (TTo)((object)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, false));
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0001316B File Offset: 0x0001136B
		public static T ShallowClone<T>(this T obj)
		{
			return ShallowClonerGenerator.CloneObject<T>(obj);
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x00013173 File Offset: 0x00011373
		static DeepClonerExtensions()
		{
			if (!DeepClonerExtensions.PermissionCheck())
			{
				throw new SecurityException("DeepCloner should have enough permissions to run. Grant FullTrust or Reflection permission.");
			}
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x00013188 File Offset: 0x00011388
		private static bool PermissionCheck()
		{
			try
			{
				new object().ShallowClone<object>();
			}
			catch (VerificationException)
			{
				return false;
			}
			catch (MemberAccessException)
			{
				return false;
			}
			return true;
		}
	}
}

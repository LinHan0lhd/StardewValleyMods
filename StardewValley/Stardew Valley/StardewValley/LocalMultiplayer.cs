using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace StardewValley
{
	// Token: 0x020000C9 RID: 201
	public class LocalMultiplayer
	{
		// Token: 0x06000DBF RID: 3519 RVA: 0x00093D8F File Offset: 0x00091F8F
		public static bool IsLocalMultiplayer(bool is_local_only = false)
		{
			if (is_local_only)
			{
				return Game1.hasLocalClientsOnly;
			}
			return GameRunner.instance.gameInstances.Count > 1;
		}

		// Token: 0x06000DC0 RID: 3520 RVA: 0x00093DAC File Offset: 0x00091FAC
		public static void Initialize()
		{
			LocalMultiplayer.GetStaticFieldsAndDefaults();
			LocalMultiplayer.GenerateDynamicMethodsForStatics();
		}

		// Token: 0x06000DC1 RID: 3521 RVA: 0x00093DB8 File Offset: 0x00091FB8
		private static void GetStaticFieldsAndDefaults()
		{
			LocalMultiplayer.staticFields = new List<FieldInfo>();
			LocalMultiplayer.staticDefaults = new List<object>();
			HashSet<string> ignored_assembly_roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Microsoft",
				"MonoGame",
				"mscorlib",
				"NetCode",
				"System",
				"xTile"
			};
			List<Type> types = new List<Type>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!ignored_assembly_roots.Contains(assembly.GetName().Name.Split('.', StringSplitOptions.None)[0]))
				{
					foreach (Type type in assembly.GetTypes())
					{
						types.Add(type);
					}
				}
			}
			foreach (Type type2 in types)
			{
				if (type2.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
				{
					bool include_by_default = type2.GetCustomAttribute<InstanceStatics>() != null;
					foreach (FieldInfo field in type2.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (!field.IsInitOnly && field.IsStatic && !field.IsLiteral && (include_by_default || field.GetCustomAttribute<InstancedStatic>() != null) && field.GetCustomAttribute<NonInstancedStatic>() == null)
						{
							RuntimeHelpers.RunClassConstructor(field.DeclaringType.TypeHandle);
							LocalMultiplayer.staticFields.Add(field);
							LocalMultiplayer.staticDefaults.Add(field.GetValue(null));
						}
					}
				}
			}
		}

		// Token: 0x06000DC2 RID: 3522 RVA: 0x00093F74 File Offset: 0x00092174
		private static void GenerateDynamicMethodsForStatics()
		{
			TypeBuilder typeBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("StardewValley.StaticInstanceVars"), AssemblyBuilderAccess.RunAndCollect).DefineDynamicModule("MainModule").DefineType("StardewValley.StaticInstanceVars", TypeAttributes.Public | TypeAttributes.AutoClass);
			foreach (FieldInfo field in LocalMultiplayer.staticFields)
			{
				typeBuilder.DefineField(field.DeclaringType.Name + "_" + field.Name, field.FieldType, FieldAttributes.Public);
			}
			LocalMultiplayer.StaticVarHolderType = typeBuilder.CreateType();
			LocalMultiplayer.staticDefaultMethod = new DynamicMethod("SetStaticVarsToDefault", null, new Type[]
			{
				typeof(object)
			}, typeof(Game1).Module, true);
			ILGenerator il = LocalMultiplayer.staticDefaultMethod.GetILGenerator();
			LocalBuilder local = il.DeclareLocal(LocalMultiplayer.StaticVarHolderType);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Castclass, LocalMultiplayer.StaticVarHolderType);
			il.Emit(OpCodes.Stloc_0);
			FieldInfo defaultsField = typeof(LocalMultiplayer).GetField("staticDefaults", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo listIndexOperator = typeof(List<object>).GetMethod("get_Item");
			for (int i = 0; i < LocalMultiplayer.staticFields.Count; i++)
			{
				FieldInfo field2 = LocalMultiplayer.staticFields[i];
				il.Emit(OpCodes.Ldloc, local.LocalIndex);
				il.Emit(OpCodes.Ldsfld, defaultsField);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Callvirt, listIndexOperator);
				if (field2.FieldType.IsValueType)
				{
					il.Emit(OpCodes.Unbox_Any, field2.FieldType);
				}
				else
				{
					il.Emit(OpCodes.Castclass, field2.FieldType);
				}
				il.Emit(OpCodes.Stfld, LocalMultiplayer.StaticVarHolderType.GetField(field2.DeclaringType.Name + "_" + field2.Name));
			}
			il.Emit(OpCodes.Ret);
			LocalMultiplayer.StaticSetDefault = (LocalMultiplayer.StaticInstanceMethod)LocalMultiplayer.staticDefaultMethod.CreateDelegate(typeof(LocalMultiplayer.StaticInstanceMethod));
			LocalMultiplayer.staticSaveMethod = new DynamicMethod("SaveStaticVars", null, new Type[]
			{
				typeof(object)
			}, typeof(Game1).Module, true);
			il = LocalMultiplayer.staticSaveMethod.GetILGenerator();
			local = il.DeclareLocal(LocalMultiplayer.StaticVarHolderType);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Castclass, LocalMultiplayer.StaticVarHolderType);
			il.Emit(OpCodes.Stloc_0);
			foreach (FieldInfo field3 in LocalMultiplayer.staticFields)
			{
				il.Emit(OpCodes.Ldloc, local.LocalIndex);
				il.Emit(OpCodes.Ldsfld, field3);
				il.Emit(OpCodes.Stfld, LocalMultiplayer.StaticVarHolderType.GetField(field3.DeclaringType.Name + "_" + field3.Name));
			}
			il.Emit(OpCodes.Ret);
			LocalMultiplayer.StaticSave = (LocalMultiplayer.StaticInstanceMethod)LocalMultiplayer.staticSaveMethod.CreateDelegate(typeof(LocalMultiplayer.StaticInstanceMethod));
			LocalMultiplayer.staticLoadMethod = new DynamicMethod("LoadStaticVars", null, new Type[]
			{
				typeof(object)
			}, typeof(Game1).Module, true);
			il = LocalMultiplayer.staticLoadMethod.GetILGenerator();
			local = il.DeclareLocal(LocalMultiplayer.StaticVarHolderType);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Castclass, LocalMultiplayer.StaticVarHolderType);
			il.Emit(OpCodes.Stloc_0);
			foreach (FieldInfo field4 in LocalMultiplayer.staticFields)
			{
				il.Emit(OpCodes.Ldloc, local.LocalIndex);
				il.Emit(OpCodes.Ldfld, LocalMultiplayer.StaticVarHolderType.GetField(field4.DeclaringType.Name + "_" + field4.Name));
				il.Emit(OpCodes.Stsfld, field4);
			}
			il.Emit(OpCodes.Ret);
			LocalMultiplayer.StaticLoad = (LocalMultiplayer.StaticInstanceMethod)LocalMultiplayer.staticLoadMethod.CreateDelegate(typeof(LocalMultiplayer.StaticInstanceMethod));
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x00094400 File Offset: 0x00092600
		public static void SaveOptions()
		{
			if (Game1.player != null && Game1.player.isCustomized.Value)
			{
				Game1.splitscreenOptions[Game1.player.UniqueMultiplayerID] = Game1.options;
			}
		}

		// Token: 0x0400092B RID: 2347
		internal static List<FieldInfo> staticFields;

		// Token: 0x0400092C RID: 2348
		internal static List<object> staticDefaults;

		// Token: 0x0400092D RID: 2349
		public static Type StaticVarHolderType;

		// Token: 0x0400092E RID: 2350
		private static DynamicMethod staticDefaultMethod;

		// Token: 0x0400092F RID: 2351
		private static DynamicMethod staticSaveMethod;

		// Token: 0x04000930 RID: 2352
		private static DynamicMethod staticLoadMethod;

		// Token: 0x04000931 RID: 2353
		public static LocalMultiplayer.StaticInstanceMethod StaticSetDefault;

		// Token: 0x04000932 RID: 2354
		public static LocalMultiplayer.StaticInstanceMethod StaticSave;

		// Token: 0x04000933 RID: 2355
		public static LocalMultiplayer.StaticInstanceMethod StaticLoad;

		// Token: 0x02000470 RID: 1136
		// (Invoke) Token: 0x06003E31 RID: 15921
		public delegate void StaticInstanceMethod(object staticVarsHolder);
	}
}

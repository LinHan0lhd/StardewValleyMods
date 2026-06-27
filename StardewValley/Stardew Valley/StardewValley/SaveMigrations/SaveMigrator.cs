using System;
using System.Collections.Generic;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000183 RID: 387
	public static class SaveMigrator
	{
		// Token: 0x06001C6A RID: 7274 RVA: 0x0014123C File Offset: 0x0013F43C
		public static void ApplySaveFixes()
		{
			if (!Game1.hasApplied1_3_UpdateChanges)
			{
				SaveMigrator_1_3.ApplyLegacyChanges();
			}
			if (!Game1.hasApplied1_4_UpdateChanges)
			{
				SaveMigrator_1_4.ApplyLegacyChanges();
			}
			if (Game1.lastAppliedSaveFix < SaveMigrator.LatestSaveFix)
			{
				List<ISaveMigrator> migrations = SaveMigrator.GetAllMigrators(true);
				for (SaveFixes saveFix = Game1.lastAppliedSaveFix + 1; saveFix < SaveFixes.MAX; saveFix++)
				{
					if (Enum.IsDefined(typeof(SaveFixes), saveFix))
					{
						Game1.log.Debug("Applying save fix: " + saveFix.ToString());
						using (List<ISaveMigrator>.Enumerator enumerator = migrations.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.ApplySaveFix(saveFix))
								{
									break;
								}
							}
						}
					}
					Game1.lastAppliedSaveFix = saveFix;
				}
			}
		}

		// Token: 0x06001C6B RID: 7275 RVA: 0x0014130C File Offset: 0x0013F50C
		public static void ApplySingleSaveFix(SaveFixes fix, List<Item> loadedItems)
		{
			using (List<ISaveMigrator>.Enumerator enumerator = SaveMigrator.GetAllMigrators(false).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ApplySaveFix(fix))
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001C6C RID: 7276 RVA: 0x00141364 File Offset: 0x0013F564
		public static List<ISaveMigrator> GetAllMigrators(bool reverse = false)
		{
			List<ISaveMigrator> migrations = new List<ISaveMigrator>();
			foreach (Type type in typeof(ISaveMigrator).Assembly.GetTypes())
			{
				if (type.IsClass && !type.IsAbstract && typeof(ISaveMigrator).IsAssignableFrom(type))
				{
					ISaveMigrator saveMigrator = (ISaveMigrator)Activator.CreateInstance(type);
					if (saveMigrator == null)
					{
						throw new InvalidOperationException("Failed to create instance of save migration '" + type.FullName + "'.");
					}
					ISaveMigrator migration = saveMigrator;
					migrations.Add(migration);
				}
			}
			if (reverse)
			{
				migrations.Sort((ISaveMigrator a, ISaveMigrator b) => -a.GameVersion.CompareTo(b.GameVersion));
			}
			else
			{
				migrations.Sort((ISaveMigrator a, ISaveMigrator b) => a.GameVersion.CompareTo(b.GameVersion));
			}
			return migrations;
		}

		// Token: 0x04001169 RID: 4457
		public static readonly SaveFixes LatestSaveFix = SaveFixes.FixDuplicateMissedMail;
	}
}

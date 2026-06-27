using System;

namespace StardewValley.SDKs
{
	// Token: 0x0200015F RID: 351
	public class NullSDKHelper : SDKHelper
	{
		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06001B02 RID: 6914 RVA: 0x0013BEA7 File Offset: 0x0013A0A7
		public bool IsEnterButtonAssignmentFlipped
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06001B03 RID: 6915 RVA: 0x0013BEAA File Offset: 0x0013A0AA
		public bool IsJapaneseRegionRelease
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001B04 RID: 6916 RVA: 0x0013BEAD File Offset: 0x0013A0AD
		public void EarlyInitialize()
		{
		}

		// Token: 0x06001B05 RID: 6917 RVA: 0x0013BEAF File Offset: 0x0013A0AF
		public void Initialize()
		{
		}

		// Token: 0x06001B06 RID: 6918 RVA: 0x0013BEB1 File Offset: 0x0013A0B1
		public bool RetroactiveAchievementsAllowed()
		{
			return true;
		}

		// Token: 0x06001B07 RID: 6919 RVA: 0x0013BEB4 File Offset: 0x0013A0B4
		public void GetAchievement(string achieve)
		{
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x0013BEB6 File Offset: 0x0013A0B6
		public void ResetAchievements()
		{
		}

		// Token: 0x06001B09 RID: 6921 RVA: 0x0013BEB8 File Offset: 0x0013A0B8
		public void Update()
		{
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x0013BEBA File Offset: 0x0013A0BA
		public void Shutdown()
		{
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x0013BEBC File Offset: 0x0013A0BC
		public void DebugInfo()
		{
		}

		// Token: 0x06001B0C RID: 6924 RVA: 0x0013BEBE File Offset: 0x0013A0BE
		public string FilterDirtyWords(string words)
		{
			return words;
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06001B0D RID: 6925 RVA: 0x0013BEC1 File Offset: 0x0013A0C1
		public virtual string Name { get; } = "?";

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06001B0E RID: 6926 RVA: 0x0013BEC9 File Offset: 0x0013A0C9
		public SDKNetHelper Networking { get; }

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06001B0F RID: 6927 RVA: 0x0013BED1 File Offset: 0x0013A0D1
		public bool ConnectionFinished { get; } = 1;

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06001B10 RID: 6928 RVA: 0x0013BED9 File Offset: 0x0013A0D9
		public int ConnectionProgress { get; }

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06001B11 RID: 6929 RVA: 0x0013BEE1 File Offset: 0x0013A0E1
		public bool HasOverlay
		{
			get
			{
				return false;
			}
		}
	}
}

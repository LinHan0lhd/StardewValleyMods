using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Minigames
{
	// Token: 0x0200023E RID: 574
	public class NetLeaderboards : INetObject<NetFields>
	{
		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x0600265A RID: 9818 RVA: 0x001B35F1 File Offset: 0x001B17F1
		public NetFields NetFields { get; } = new NetFields("NetLeaderboards");

		// Token: 0x0600265B RID: 9819 RVA: 0x001B35F9 File Offset: 0x001B17F9
		public void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.entries, "entries").AddField(this.maxEntries, "maxEntries");
		}

		// Token: 0x0600265C RID: 9820 RVA: 0x001B3628 File Offset: 0x001B1828
		public NetLeaderboards()
		{
			this.InitNetFields();
		}

		// Token: 0x0600265D RID: 9821 RVA: 0x001B3660 File Offset: 0x001B1860
		public void AddScore(string name, int score)
		{
			List<NetLeaderboardsEntry> temp_entries = new List<NetLeaderboardsEntry>(this.entries);
			temp_entries.Add(new NetLeaderboardsEntry(name, score));
			temp_entries.Sort((NetLeaderboardsEntry a, NetLeaderboardsEntry b) => a.score.Value.CompareTo(b.score.Value));
			temp_entries.Reverse();
			while (temp_entries.Count > this.maxEntries.Value)
			{
				temp_entries.RemoveAt(temp_entries.Count - 1);
			}
			this.entries.Set(temp_entries);
		}

		// Token: 0x0600265E RID: 9822 RVA: 0x001B36E0 File Offset: 0x001B18E0
		public List<KeyValuePair<string, int>> GetScores()
		{
			List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();
			foreach (NetLeaderboardsEntry entry in this.entries)
			{
				scores.Add(new KeyValuePair<string, int>(entry.name.Value, entry.score.Value));
			}
			scores.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => a.Value.CompareTo(b.Value));
			scores.Reverse();
			return scores;
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x001B3780 File Offset: 0x001B1980
		public void LoadScores(List<KeyValuePair<string, int>> scores)
		{
			this.entries.Clear();
			foreach (KeyValuePair<string, int> score in scores)
			{
				this.AddScore(score.Key, score.Value);
			}
		}

		// Token: 0x040017D6 RID: 6102
		public NetObjectList<NetLeaderboardsEntry> entries = new NetObjectList<NetLeaderboardsEntry>();

		// Token: 0x040017D7 RID: 6103
		public NetInt maxEntries = new NetInt(10);
	}
}

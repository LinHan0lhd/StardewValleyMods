using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Audio
{
	// Token: 0x020003B8 RID: 952
	public class LoopingCueManager
	{
		// Token: 0x06003959 RID: 14681 RVA: 0x002D6A60 File Offset: 0x002D4C60
		public virtual void Update(GameLocation currentLocation)
		{
			NetDictionary<string, bool, NetBool, SerializableDictionary<string, bool>, NetStringDictionary<bool, NetBool>>.KeysCollection activeCues = currentLocation.netAudio.ActiveCues;
			foreach (string cue in activeCues)
			{
				if (!this.playingCues.ContainsKey(cue))
				{
					ICue instance;
					Game1.playSound(cue, out instance);
					this.playingCues[cue] = instance;
				}
			}
			foreach (KeyValuePair<string, ICue> pair in this.playingCues)
			{
				string cue2 = pair.Key;
				if (!activeCues.Contains(cue2))
				{
					this.cuesToStop.Add(cue2);
				}
			}
			foreach (string cue3 in this.cuesToStop)
			{
				this.playingCues[cue3].Stop(AudioStopOptions.AsAuthored);
				this.playingCues.Remove(cue3);
			}
			this.cuesToStop.Clear();
		}

		// Token: 0x0600395A RID: 14682 RVA: 0x002D6BA0 File Offset: 0x002D4DA0
		public void StopAll()
		{
			foreach (ICue cue in this.playingCues.Values)
			{
				cue.Stop(AudioStopOptions.Immediate);
			}
			this.playingCues.Clear();
		}

		// Token: 0x040025F9 RID: 9721
		private Dictionary<string, ICue> playingCues = new Dictionary<string, ICue>();

		// Token: 0x040025FA RID: 9722
		private List<string> cuesToStop = new List<string>();
	}
}

using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	// Token: 0x0200008F RID: 143
	public class CueWrapper : ICue, IDisposable
	{
		// Token: 0x060005E3 RID: 1507 RVA: 0x00021CAB File Offset: 0x0001FEAB
		public CueWrapper(Cue cue)
		{
			this.cue = cue;
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00021CBC File Offset: 0x0001FEBC
		public void Play()
		{
			try
			{
				this.cue.Play();
			}
			catch (Exception ex)
			{
				Game1.log.Error("Error playing sound '" + this.Name + "'.", ex);
			}
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00021D0C File Offset: 0x0001FF0C
		public void Pause()
		{
			try
			{
				this.cue.Pause();
			}
			catch (Exception ex)
			{
				Game1.log.Error("Error pausing sound '" + this.Name + "'.", ex);
			}
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x00021D5C File Offset: 0x0001FF5C
		public void Resume()
		{
			try
			{
				this.cue.Resume();
			}
			catch (Exception ex)
			{
				Game1.log.Error("Error resuming sound '" + this.Name + "'.", ex);
			}
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x00021DAC File Offset: 0x0001FFAC
		public void Stop(AudioStopOptions options)
		{
			try
			{
				this.cue.Stop(options);
			}
			catch (Exception ex)
			{
				Game1.log.Error("Error stopping sound '" + this.Name + "'.", ex);
			}
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x00021DFC File Offset: 0x0001FFFC
		public void SetVariable(string var, int val)
		{
			this.cue.SetVariable(var, (float)val);
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x00021E0C File Offset: 0x0002000C
		public void SetVariable(string var, float val)
		{
			this.cue.SetVariable(var, val);
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00021E1B File Offset: 0x0002001B
		public float GetVariable(string var)
		{
			return this.cue.GetVariable(var);
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x060005EB RID: 1515 RVA: 0x00021E29 File Offset: 0x00020029
		public bool IsStopped
		{
			get
			{
				return this.cue.IsStopped;
			}
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060005EC RID: 1516 RVA: 0x00021E36 File Offset: 0x00020036
		public bool IsStopping
		{
			get
			{
				return this.cue.IsStopping;
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060005ED RID: 1517 RVA: 0x00021E43 File Offset: 0x00020043
		public bool IsPlaying
		{
			get
			{
				return this.cue.IsPlaying;
			}
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060005EE RID: 1518 RVA: 0x00021E50 File Offset: 0x00020050
		public bool IsPaused
		{
			get
			{
				return this.cue.IsPaused;
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060005EF RID: 1519 RVA: 0x00021E5D File Offset: 0x0002005D
		public string Name
		{
			get
			{
				return this.cue.Name;
			}
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00021E6A File Offset: 0x0002006A
		public void Dispose()
		{
			this.cue.Dispose();
			this.cue = null;
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060005F1 RID: 1521 RVA: 0x00021E7E File Offset: 0x0002007E
		// (set) Token: 0x060005F2 RID: 1522 RVA: 0x00021E8B File Offset: 0x0002008B
		public float Volume
		{
			get
			{
				return this.cue.Volume;
			}
			set
			{
				this.cue.Volume = value;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060005F3 RID: 1523 RVA: 0x00021E99 File Offset: 0x00020099
		// (set) Token: 0x060005F4 RID: 1524 RVA: 0x00021EA6 File Offset: 0x000200A6
		public float Pitch
		{
			get
			{
				return this.cue.Pitch;
			}
			set
			{
				this.cue.Pitch = value;
			}
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060005F5 RID: 1525 RVA: 0x00021EB4 File Offset: 0x000200B4
		public bool IsPitchBeingControlledByRPC
		{
			get
			{
				return this.cue.IsPitchBeingControlledByRPC;
			}
		}

		// Token: 0x040002F2 RID: 754
		private Cue cue;
	}
}

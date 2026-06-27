using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	// Token: 0x02000090 RID: 144
	public class DummyCue : ICue, IDisposable
	{
		// Token: 0x060005F6 RID: 1526 RVA: 0x00021EC1 File Offset: 0x000200C1
		public void Play()
		{
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00021EC3 File Offset: 0x000200C3
		public void Pause()
		{
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00021EC5 File Offset: 0x000200C5
		public void Resume()
		{
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00021EC7 File Offset: 0x000200C7
		public void SetVariable(string var, int val)
		{
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x00021EC9 File Offset: 0x000200C9
		public void SetVariable(string var, float val)
		{
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x00021ECB File Offset: 0x000200CB
		public float GetVariable(string var)
		{
			return 0f;
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060005FC RID: 1532 RVA: 0x00021ED2 File Offset: 0x000200D2
		public bool IsStopped
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060005FD RID: 1533 RVA: 0x00021ED5 File Offset: 0x000200D5
		public bool IsStopping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x00021ED8 File Offset: 0x000200D8
		public bool IsPlaying
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060005FF RID: 1535 RVA: 0x00021EDB File Offset: 0x000200DB
		public bool IsPaused
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000600 RID: 1536 RVA: 0x00021EDE File Offset: 0x000200DE
		public string Name
		{
			get
			{
				return "";
			}
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00021EE5 File Offset: 0x000200E5
		public void Stop(AudioStopOptions options)
		{
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00021EE7 File Offset: 0x000200E7
		public void Dispose()
		{
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000603 RID: 1539 RVA: 0x00021EE9 File Offset: 0x000200E9
		// (set) Token: 0x06000604 RID: 1540 RVA: 0x00021EF0 File Offset: 0x000200F0
		public float Volume
		{
			get
			{
				return 1f;
			}
			set
			{
			}
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000605 RID: 1541 RVA: 0x00021EF2 File Offset: 0x000200F2
		// (set) Token: 0x06000606 RID: 1542 RVA: 0x00021EF9 File Offset: 0x000200F9
		public float Pitch
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000607 RID: 1543 RVA: 0x00021EFB File Offset: 0x000200FB
		public bool IsPitchBeingControlledByRPC
		{
			get
			{
				return true;
			}
		}
	}
}

using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	// Token: 0x02000104 RID: 260
	public class DummySoundBank : ISoundBank, IDisposable
	{
		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06001494 RID: 5268 RVA: 0x000F79D4 File Offset: 0x000F5BD4
		public bool IsInUse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06001495 RID: 5269 RVA: 0x000F79D7 File Offset: 0x000F5BD7
		public bool IsDisposed
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001496 RID: 5270 RVA: 0x000F79DA File Offset: 0x000F5BDA
		public bool Exists(string name)
		{
			return true;
		}

		// Token: 0x06001497 RID: 5271 RVA: 0x000F79DD File Offset: 0x000F5BDD
		public ICue GetCue(string name)
		{
			return DummySoundBank.DummyCue;
		}

		// Token: 0x06001498 RID: 5272 RVA: 0x000F79E4 File Offset: 0x000F5BE4
		public void PlayCue(string name)
		{
		}

		// Token: 0x06001499 RID: 5273 RVA: 0x000F79E6 File Offset: 0x000F5BE6
		public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
		{
		}

		// Token: 0x0600149A RID: 5274 RVA: 0x000F79E8 File Offset: 0x000F5BE8
		public void AddCue(CueDefinition definition)
		{
		}

		// Token: 0x0600149B RID: 5275 RVA: 0x000F79EA File Offset: 0x000F5BEA
		public CueDefinition GetCueDefinition(string name)
		{
			return null;
		}

		// Token: 0x0600149C RID: 5276 RVA: 0x000F79ED File Offset: 0x000F5BED
		public void Dispose()
		{
		}

		// Token: 0x04000D2A RID: 3370
		internal static readonly ICue DummyCue = new DummyCue();
	}
}

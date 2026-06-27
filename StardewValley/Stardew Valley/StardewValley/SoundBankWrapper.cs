using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	// Token: 0x02000103 RID: 259
	public class SoundBankWrapper : ISoundBank, IDisposable
	{
		// Token: 0x17000243 RID: 579
		// (get) Token: 0x0600148A RID: 5258 RVA: 0x000F78E0 File Offset: 0x000F5AE0
		public bool IsInUse
		{
			get
			{
				return this.soundBank.IsInUse;
			}
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x0600148B RID: 5259 RVA: 0x000F78ED File Offset: 0x000F5AED
		public bool IsDisposed
		{
			get
			{
				return this.soundBank.IsDisposed;
			}
		}

		// Token: 0x0600148C RID: 5260 RVA: 0x000F78FA File Offset: 0x000F5AFA
		public SoundBankWrapper(SoundBank soundBank)
		{
			this.soundBank = soundBank;
		}

		// Token: 0x0600148D RID: 5261 RVA: 0x000F7914 File Offset: 0x000F5B14
		public ICue GetCue(string name)
		{
			if (!this.Exists(name))
			{
				Game1.log.Error("Can't get audio ID '" + name + "' because it doesn't exist.", null);
				name = this.DefaultCueName;
			}
			return new CueWrapper(this.soundBank.GetCue(name));
		}

		// Token: 0x0600148E RID: 5262 RVA: 0x000F7953 File Offset: 0x000F5B53
		public void PlayCue(string name)
		{
			if (!this.Exists(name))
			{
				Game1.log.Error("Can't play audio ID '" + name + "' because it doesn't exist.", null);
				name = this.DefaultCueName;
			}
			this.soundBank.PlayCue(name);
		}

		// Token: 0x0600148F RID: 5263 RVA: 0x000F798D File Offset: 0x000F5B8D
		public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
		{
			this.soundBank.PlayCue(name, listener, emitter);
		}

		// Token: 0x06001490 RID: 5264 RVA: 0x000F799D File Offset: 0x000F5B9D
		public void Dispose()
		{
			this.soundBank.Dispose();
		}

		// Token: 0x06001491 RID: 5265 RVA: 0x000F79AA File Offset: 0x000F5BAA
		public void AddCue(CueDefinition definition)
		{
			this.soundBank.AddCue(definition);
		}

		// Token: 0x06001492 RID: 5266 RVA: 0x000F79B8 File Offset: 0x000F5BB8
		public bool Exists(string name)
		{
			return this.soundBank.Exists(name);
		}

		// Token: 0x06001493 RID: 5267 RVA: 0x000F79C6 File Offset: 0x000F5BC6
		public CueDefinition GetCueDefinition(string name)
		{
			return this.soundBank.GetCueDefinition(name);
		}

		// Token: 0x04000D28 RID: 3368
		private string DefaultCueName = "shiny4";

		// Token: 0x04000D29 RID: 3369
		private SoundBank soundBank;
	}
}

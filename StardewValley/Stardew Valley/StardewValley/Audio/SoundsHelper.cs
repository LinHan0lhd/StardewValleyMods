using System;
using System.Text;
using Microsoft.Xna.Framework;

namespace StardewValley.Audio
{
	// Token: 0x020003BA RID: 954
	public class SoundsHelper : ISoundsHelper
	{
		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x0600395C RID: 14684 RVA: 0x002D6C22 File Offset: 0x002D4E22
		// (set) Token: 0x0600395D RID: 14685 RVA: 0x002D6C2D File Offset: 0x002D4E2D
		public virtual bool LogSounds
		{
			get
			{
				return this.LogSound != null;
			}
			set
			{
				if (value)
				{
					this.LogSound = new Action<string, GameLocation, Vector2?, int?, float, SoundContext, string>(this.LogSoundImpl);
					return;
				}
				this.LogSound = null;
			}
		}

		// Token: 0x0600395E RID: 14686 RVA: 0x002D6C4D File Offset: 0x002D4E4D
		public virtual bool ShouldPlayLocal(SoundContext context)
		{
			return context != SoundContext.NPC || !Game1.eventUp;
		}

		// Token: 0x0600395F RID: 14687 RVA: 0x002D6C60 File Offset: 0x002D4E60
		public virtual float GetVolumeForDistance(GameLocation location, Vector2? position)
		{
			if (location == null)
			{
				return 1f;
			}
			string nameOrUniqueName = location.NameOrUniqueName;
			GameLocation currentLocation = Game1.currentLocation;
			if (nameOrUniqueName != ((currentLocation != null) ? currentLocation.NameOrUniqueName : null))
			{
				return 0f;
			}
			if (position == null)
			{
				return 1f;
			}
			float tileDistance = Utility.distanceFromScreen(position.Value * 64f) / 64f;
			if (tileDistance <= 0f)
			{
				return 1f;
			}
			if (tileDistance >= (float)SoundsHelper.MaxDistanceFromScreen)
			{
				return 0f;
			}
			return 1f - tileDistance / (float)SoundsHelper.MaxDistanceFromScreen;
		}

		// Token: 0x06003960 RID: 14688 RVA: 0x002D6CF4 File Offset: 0x002D4EF4
		public virtual bool PlayLocal(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context, out ICue cue)
		{
			bool result;
			try
			{
				cue = Game1.soundBank.GetCue(cueName);
				ICue cue2 = cue;
				int? num = pitch;
				this.SetPitch(cue2, (num != null) ? ((float)num.GetValueOrDefault()) : 1200f, pitch != null);
				if (!this.ShouldPlayLocal(context))
				{
					Action<string, GameLocation, Vector2?, int?, float, SoundContext, string> logSound = this.LogSound;
					if (logSound != null)
					{
						logSound(cueName, location, position, pitch, 1f, context, "disabled for context");
					}
					result = false;
				}
				else
				{
					float volume = this.GetVolumeForDistance(location, position);
					if (volume <= 0f)
					{
						Action<string, GameLocation, Vector2?, int?, float, SoundContext, string> logSound2 = this.LogSound;
						if (logSound2 != null)
						{
							logSound2(cueName, location, position, pitch, volume, context, "disabled for distance");
						}
						result = false;
					}
					else
					{
						cue.Play();
						if (volume < 1f)
						{
							cue.Volume *= volume;
						}
						Action<string, GameLocation, Vector2?, int?, float, SoundContext, string> logSound3 = this.LogSound;
						if (logSound3 != null)
						{
							logSound3(cueName, location, position, pitch, volume, context, null);
						}
						result = true;
					}
				}
			}
			catch (Exception ex)
			{
				Game1.debugOutput = Game1.parseText(ex.Message);
				Game1.log.Error("Error playing sound.", ex);
				cue = DummySoundBank.DummyCue;
				result = false;
			}
			return result;
		}

		// Token: 0x06003961 RID: 14689 RVA: 0x002D6E1C File Offset: 0x002D501C
		public virtual void PlayAll(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context)
		{
			if (this.CanSkipSoundSync(location, position, context))
			{
				ICue cue;
				this.PlayLocal(cueName, location, position, pitch, context, out cue);
				return;
			}
			location.netAudio.Fire(cueName, position, pitch, context);
		}

		// Token: 0x06003962 RID: 14690 RVA: 0x002D6E58 File Offset: 0x002D5058
		public void SetPitch(ICue cue, float pitch, bool forcePitch = true)
		{
			if (cue == null)
			{
				return;
			}
			cue.SetVariable("Pitch", pitch);
			if (forcePitch)
			{
				try
				{
					if (!cue.IsPitchBeingControlledByRPC)
					{
						cue.Pitch = Utility.Lerp(-1f, 1f, pitch / 2400f);
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x06003963 RID: 14691 RVA: 0x002D6EB4 File Offset: 0x002D50B4
		public virtual bool CanSkipSoundSync(GameLocation location, Vector2? position, SoundContext context)
		{
			if (!LocalMultiplayer.IsLocalMultiplayer(true))
			{
				return false;
			}
			if (Game1.eventUp && context == SoundContext.NPC)
			{
				return false;
			}
			if (this.ShouldPlayLocal(context) && this.GetVolumeForDistance(location, position) > 0f)
			{
				return true;
			}
			if (location != null)
			{
				bool someoneCanHear = false;
				foreach (Game1 game in GameRunner.instance.gameInstances)
				{
					GameLocation instanceGameLocation = game.instanceGameLocation;
					if (((instanceGameLocation != null) ? instanceGameLocation.NameOrUniqueName : null) == location.NameOrUniqueName)
					{
						someoneCanHear = true;
						break;
					}
				}
				if (someoneCanHear && position != null && position != Vector2.Zero)
				{
					someoneCanHear = false;
					GameRunner.instance.ExecuteForInstances(delegate(Game1 _)
					{
						if (someoneCanHear)
						{
							return;
						}
						if (this.ShouldPlayLocal(context) && this.GetVolumeForDistance(location, position) > 0f)
						{
							someoneCanHear = true;
						}
					});
				}
				return someoneCanHear;
			}
			return true;
		}

		// Token: 0x06003964 RID: 14692 RVA: 0x002D700C File Offset: 0x002D520C
		protected virtual void LogSoundImpl(string cueName, GameLocation location, Vector2? position, int? pitch, float volume, SoundContext context, string skipReason = null)
		{
			bool flag = skipReason != null;
			StringBuilder summary = new StringBuilder();
			summary.Append("Played sound '").Append(cueName).Append("'");
			if (location == null)
			{
				summary.Append(" everywhere");
			}
			else
			{
				summary.Append(" in ").Append(location.NameOrUniqueName);
				if (position != null)
				{
					summary.Append(" (").Append(position.Value.X).Append(", ").Append(position.Value.Y).Append(")");
				}
			}
			if (pitch != null)
			{
				summary.Append(" with pitch ").Append(pitch.Value);
			}
			if (!flag && volume < 1f)
			{
				summary.Append(" with distance").Append(volume);
			}
			if (flag)
			{
				summary.Append(" (").Append(skipReason).Append(")");
			}
			Game1.log.Debug(summary.ToString());
		}

		// Token: 0x040025FE RID: 9726
		public const float DefaultPitch = 1200f;

		// Token: 0x040025FF RID: 9727
		public const float MaxPitch = 2400f;

		// Token: 0x04002600 RID: 9728
		public static int MaxDistanceFromScreen = 12;

		// Token: 0x04002601 RID: 9729
		private Action<string, GameLocation, Vector2?, int?, float, SoundContext, string> LogSound;
	}
}

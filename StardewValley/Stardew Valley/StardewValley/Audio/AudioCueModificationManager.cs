using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Audio
{
	// Token: 0x020003B1 RID: 945
	public class AudioCueModificationManager
	{
		// Token: 0x06003937 RID: 14647 RVA: 0x002D6725 File Offset: 0x002D4925
		public void OnStartup()
		{
			this.cueModificationData = DataLoader.AudioChanges(Game1.content);
			this.ApplyAllCueModifications();
		}

		// Token: 0x06003938 RID: 14648 RVA: 0x002D6740 File Offset: 0x002D4940
		public virtual void ApplyAllCueModifications()
		{
			foreach (string key in this.cueModificationData.Keys)
			{
				this.ApplyCueModification(key);
			}
		}

		// Token: 0x06003939 RID: 14649 RVA: 0x002D6798 File Offset: 0x002D4998
		public virtual string GetFilePath(string filePath)
		{
			return Path.Combine(Game1.content.RootDirectory, filePath);
		}

		// Token: 0x0600393A RID: 14650 RVA: 0x002D67AC File Offset: 0x002D49AC
		public virtual void ApplyCueModification(string key)
		{
			try
			{
				AudioCueData modification_data;
				if (this.cueModificationData.TryGetValue(key, out modification_data))
				{
					bool is_modification = false;
					int category_index = Game1.audioEngine.GetCategoryIndex("Default");
					CueDefinition cue_definition;
					if (Game1.soundBank.Exists(modification_data.Id))
					{
						cue_definition = Game1.soundBank.GetCueDefinition(modification_data.Id);
						is_modification = true;
					}
					else
					{
						cue_definition = new CueDefinition();
						cue_definition.name = modification_data.Id;
					}
					if (modification_data.Category != null)
					{
						category_index = Game1.audioEngine.GetCategoryIndex(modification_data.Category);
					}
					if (modification_data.FilePaths != null)
					{
						SoundEffect[] effects = new SoundEffect[modification_data.FilePaths.Count];
						for (int i = 0; i < modification_data.FilePaths.Count; i++)
						{
							string file_path = this.GetFilePath(modification_data.FilePaths[i]);
							bool vorbis = Path.GetExtension(file_path).EqualsIgnoreCase(".ogg");
							int invalid_sounds = 0;
							try
							{
								SoundEffect sound_effect;
								if (vorbis && modification_data.StreamedVorbis)
								{
									sound_effect = new OggStreamSoundEffect(file_path);
								}
								else
								{
									using (FileStream stream = new FileStream(file_path, FileMode.Open))
									{
										sound_effect = SoundEffect.FromStream(stream, vorbis);
									}
								}
								effects[i - invalid_sounds] = sound_effect;
							}
							catch (Exception e)
							{
								Game1.log.Error("Error loading sound: " + file_path, e);
								invalid_sounds++;
							}
							if (invalid_sounds > 0)
							{
								Array.Resize<SoundEffect>(ref effects, effects.Length - invalid_sounds);
							}
						}
						cue_definition.SetSound(effects, category_index, modification_data.Looped, modification_data.UseReverb);
						if (is_modification)
						{
							Action onModified = cue_definition.OnModified;
							if (onModified != null)
							{
								onModified();
							}
						}
					}
					Game1.soundBank.AddCue(cue_definition);
				}
			}
			catch (NoAudioHardwareException)
			{
				Game1.log.Warn("Can't apply modifications for audio cue '" + key + "' because there's no audio hardware available.");
			}
		}

		// Token: 0x040025F4 RID: 9716
		public Dictionary<string, AudioCueData> cueModificationData;
	}
}

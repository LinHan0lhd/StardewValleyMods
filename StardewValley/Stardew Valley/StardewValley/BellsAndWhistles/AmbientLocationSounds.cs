using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200038E RID: 910
	[InstanceStatics]
	public class AmbientLocationSounds
	{
		// Token: 0x06003819 RID: 14361 RVA: 0x002C5A8C File Offset: 0x002C3C8C
		public static void InitShared()
		{
			if (AmbientLocationSounds.babblingBrook == null)
			{
				Game1.playSound("babblingBrook", out AmbientLocationSounds.babblingBrook);
				AmbientLocationSounds.babblingBrook.Pause();
			}
			if (AmbientLocationSounds.cracklingFire == null)
			{
				Game1.playSound("cracklingFire", out AmbientLocationSounds.cracklingFire);
				AmbientLocationSounds.cracklingFire.Pause();
			}
			if (AmbientLocationSounds.engine == null)
			{
				Game1.playSound("heavyEngine", out AmbientLocationSounds.engine);
				AmbientLocationSounds.engine.Pause();
			}
			if (AmbientLocationSounds.cricket == null)
			{
				Game1.playSound("cricketsAmbient", out AmbientLocationSounds.cricket);
				AmbientLocationSounds.cricket.Pause();
			}
			if (AmbientLocationSounds.waterfall == null)
			{
				Game1.playSound("waterfall", out AmbientLocationSounds.waterfall);
				AmbientLocationSounds.waterfall.Pause();
			}
			if (AmbientLocationSounds.waterfallBig == null)
			{
				Game1.playSound("waterfall_big", out AmbientLocationSounds.waterfallBig);
				AmbientLocationSounds.waterfallBig.Pause();
			}
			AmbientLocationSounds.shortestDistanceForCue = new float[6];
		}

		// Token: 0x0600381A RID: 14362 RVA: 0x002C5B6C File Offset: 0x002C3D6C
		public static void update(GameTime time)
		{
			if (AmbientLocationSounds.sounds.Count == 0)
			{
				return;
			}
			if (AmbientLocationSounds.volumeOverrideForLocChange < 1f)
			{
				AmbientLocationSounds.volumeOverrideForLocChange += (float)time.ElapsedGameTime.Milliseconds * 0.0003f;
			}
			AmbientLocationSounds.updateTimer -= time.ElapsedGameTime.Milliseconds;
			if (AmbientLocationSounds.updateTimer <= 0)
			{
				for (int i = 0; i < AmbientLocationSounds.shortestDistanceForCue.Length; i++)
				{
					AmbientLocationSounds.shortestDistanceForCue[i] = 9999999f;
				}
				Vector2 farmerPosition = Game1.player.getStandingPosition();
				foreach (KeyValuePair<Vector2, int> pair in AmbientLocationSounds.sounds)
				{
					float distance = Vector2.Distance(pair.Key, farmerPosition);
					if (AmbientLocationSounds.shortestDistanceForCue[pair.Value] > distance)
					{
						AmbientLocationSounds.shortestDistanceForCue[pair.Value] = distance;
					}
				}
				if (AmbientLocationSounds.volumeOverrideForLocChange >= 0f)
				{
					for (int j = 0; j < AmbientLocationSounds.shortestDistanceForCue.Length; j++)
					{
						if (AmbientLocationSounds.shortestDistanceForCue[j] <= (float)AmbientLocationSounds.farthestSoundDistance * 1.5f)
						{
							float volume = Math.Min(AmbientLocationSounds.volumeOverrideForLocChange, Math.Min(1f, 1f - AmbientLocationSounds.shortestDistanceForCue[j] / ((float)AmbientLocationSounds.farthestSoundDistance * 1.5f)));
							volume = (float)Math.Pow((double)volume, 5.0);
							switch (j)
							{
							case 0:
								if (AmbientLocationSounds.babblingBrook != null)
								{
									AmbientLocationSounds.babblingBrook.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
									AmbientLocationSounds.babblingBrook.Resume();
								}
								break;
							case 1:
								if (AmbientLocationSounds.cracklingFire != null)
								{
									AmbientLocationSounds.cracklingFire.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
									AmbientLocationSounds.cracklingFire.Resume();
								}
								break;
							case 2:
								if (AmbientLocationSounds.engine != null)
								{
									AmbientLocationSounds.engine.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
									AmbientLocationSounds.engine.Resume();
								}
								break;
							case 3:
								if (AmbientLocationSounds.cricket != null)
								{
									AmbientLocationSounds.cricket.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
									AmbientLocationSounds.cricket.Resume();
								}
								break;
							case 4:
								if (AmbientLocationSounds.waterfall != null)
								{
									AmbientLocationSounds.waterfall.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
									AmbientLocationSounds.waterfall.Resume();
								}
								break;
							case 5:
								if (AmbientLocationSounds.waterfallBig != null)
								{
									AmbientLocationSounds.waterfallBig.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
									AmbientLocationSounds.waterfallBig.Resume();
								}
								break;
							}
						}
						else
						{
							switch (j)
							{
							case 0:
							{
								ICue cue = AmbientLocationSounds.babblingBrook;
								if (cue != null)
								{
									cue.Pause();
								}
								break;
							}
							case 1:
							{
								ICue cue2 = AmbientLocationSounds.cracklingFire;
								if (cue2 != null)
								{
									cue2.Pause();
								}
								break;
							}
							case 2:
							{
								ICue cue3 = AmbientLocationSounds.engine;
								if (cue3 != null)
								{
									cue3.Pause();
								}
								break;
							}
							case 3:
							{
								ICue cue4 = AmbientLocationSounds.cricket;
								if (cue4 != null)
								{
									cue4.Pause();
								}
								break;
							}
							case 4:
							{
								ICue cue5 = AmbientLocationSounds.waterfall;
								if (cue5 != null)
								{
									cue5.Pause();
								}
								break;
							}
							case 5:
							{
								ICue cue6 = AmbientLocationSounds.waterfallBig;
								if (cue6 != null)
								{
									cue6.Pause();
								}
								break;
							}
							}
						}
					}
				}
				AmbientLocationSounds.updateTimer = 100;
			}
		}

		// Token: 0x0600381B RID: 14363 RVA: 0x002C5F4C File Offset: 0x002C414C
		public static void changeSpecificVariable(string variableName, float value, int whichSound)
		{
			if (whichSound == 2)
			{
				ICue cue = AmbientLocationSounds.engine;
				if (cue == null)
				{
					return;
				}
				cue.SetVariable(variableName, value);
			}
		}

		// Token: 0x0600381C RID: 14364 RVA: 0x002C5F63 File Offset: 0x002C4163
		public static void addSound(Vector2 tileLocation, int whichSound)
		{
			AmbientLocationSounds.sounds.TryAdd(tileLocation * 64f, whichSound);
		}

		// Token: 0x0600381D RID: 14365 RVA: 0x002C5F7C File Offset: 0x002C417C
		public static void removeSound(Vector2 tileLocation)
		{
			int sound;
			if (AmbientLocationSounds.sounds.TryGetValue(tileLocation * 64f, out sound))
			{
				switch (sound)
				{
				case 0:
				{
					ICue cue = AmbientLocationSounds.babblingBrook;
					if (cue != null)
					{
						cue.Pause();
					}
					break;
				}
				case 1:
				{
					ICue cue2 = AmbientLocationSounds.cracklingFire;
					if (cue2 != null)
					{
						cue2.Pause();
					}
					break;
				}
				case 2:
				{
					ICue cue3 = AmbientLocationSounds.engine;
					if (cue3 != null)
					{
						cue3.Pause();
					}
					break;
				}
				case 3:
				{
					ICue cue4 = AmbientLocationSounds.cricket;
					if (cue4 != null)
					{
						cue4.Pause();
					}
					break;
				}
				case 4:
				{
					ICue cue5 = AmbientLocationSounds.waterfall;
					if (cue5 != null)
					{
						cue5.Pause();
					}
					break;
				}
				case 5:
				{
					ICue cue6 = AmbientLocationSounds.waterfallBig;
					if (cue6 != null)
					{
						cue6.Pause();
					}
					break;
				}
				}
				AmbientLocationSounds.sounds.Remove(tileLocation * 64f);
			}
		}

		// Token: 0x0600381E RID: 14366 RVA: 0x002C6048 File Offset: 0x002C4248
		public static void onLocationLeave()
		{
			AmbientLocationSounds.sounds.Clear();
			AmbientLocationSounds.volumeOverrideForLocChange = -0.5f;
			ICue cue = AmbientLocationSounds.babblingBrook;
			if (cue != null)
			{
				cue.Pause();
			}
			ICue cue2 = AmbientLocationSounds.cracklingFire;
			if (cue2 != null)
			{
				cue2.Pause();
			}
			if (AmbientLocationSounds.engine != null)
			{
				AmbientLocationSounds.engine.SetVariable("Frequency", 100f);
				AmbientLocationSounds.engine.Pause();
			}
			ICue cue3 = AmbientLocationSounds.cricket;
			if (cue3 != null)
			{
				cue3.Pause();
			}
			ICue cue4 = AmbientLocationSounds.waterfall;
			if (cue4 != null)
			{
				cue4.Pause();
			}
			ICue cue5 = AmbientLocationSounds.waterfallBig;
			if (cue5 == null)
			{
				return;
			}
			cue5.Pause();
		}

		// Token: 0x0400246A RID: 9322
		public const int sound_babblingBrook = 0;

		// Token: 0x0400246B RID: 9323
		public const int sound_cracklingFire = 1;

		// Token: 0x0400246C RID: 9324
		public const int sound_engine = 2;

		// Token: 0x0400246D RID: 9325
		public const int sound_cricket = 3;

		// Token: 0x0400246E RID: 9326
		public const int sound_waterfall = 4;

		// Token: 0x0400246F RID: 9327
		public const int sound_waterfall_big = 5;

		// Token: 0x04002470 RID: 9328
		public const int numberOfSounds = 6;

		// Token: 0x04002471 RID: 9329
		public const float doNotPlay = 9999999f;

		// Token: 0x04002472 RID: 9330
		private static Dictionary<Vector2, int> sounds = new Dictionary<Vector2, int>();

		// Token: 0x04002473 RID: 9331
		private static int updateTimer = 100;

		// Token: 0x04002474 RID: 9332
		private static int farthestSoundDistance = 1024;

		// Token: 0x04002475 RID: 9333
		private static float[] shortestDistanceForCue;

		// Token: 0x04002476 RID: 9334
		private static ICue babblingBrook;

		// Token: 0x04002477 RID: 9335
		private static ICue cracklingFire;

		// Token: 0x04002478 RID: 9336
		private static ICue engine;

		// Token: 0x04002479 RID: 9337
		private static ICue cricket;

		// Token: 0x0400247A RID: 9338
		private static ICue waterfall;

		// Token: 0x0400247B RID: 9339
		private static ICue waterfallBig;

		// Token: 0x0400247C RID: 9340
		private static float volumeOverrideForLocChange;
	}
}

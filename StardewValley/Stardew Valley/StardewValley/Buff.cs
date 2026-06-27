using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.Buffs;
using StardewValley.TokenizableStrings;

namespace StardewValley
{
	// Token: 0x02000083 RID: 131
	public class Buff
	{
		// Token: 0x060004E6 RID: 1254 RVA: 0x00018ADC File Offset: 0x00016CDC
		public Buff(string id, string source = null, string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1, BuffEffects effects = null, bool? isDebuff = null, string displayName = null, string description = null)
		{
			this.id = id;
			this.source = source;
			this.displaySource = displaySource;
			bool defaultIsDebuff = false;
			BuffData data;
			if (id != null && DataLoader.Buffs(Game1.content).TryGetValue(id, out data))
			{
				this.displayName = TokenParser.ParseText(data.DisplayName, null, null, null);
				this.description = TokenParser.ParseText(data.Description, null, null, null);
				this.glow = (Utility.StringToColor(data.GlowColor) ?? this.glow);
				this.millisecondsDuration = ((data.MaxDuration > 0 && data.MaxDuration > data.Duration) ? Game1.random.Next(data.Duration, data.MaxDuration + 1) : data.Duration);
				this.iconTexture = ((data.IconTexture == "TileSheets\\BuffsIcons") ? Game1.buffsIcons : Game1.content.Load<Texture2D>(data.IconTexture));
				this.iconSheetIndex = data.IconSpriteIndex;
				this.effects.Add(data.Effects);
				List<string> list = data.ActionsOnApply;
				this.actionsOnApply = ((list != null) ? list.ToArray() : null);
				defaultIsDebuff = data.IsDebuff;
				this.customFields.TryAddMany(data.CustomFields);
			}
			if (duration != -1)
			{
				this.millisecondsDuration = duration;
			}
			if (iconTexture != null)
			{
				this.iconTexture = iconTexture;
			}
			if (iconSheetIndex != -1)
			{
				this.iconSheetIndex = iconSheetIndex;
			}
			if (displayName != null)
			{
				this.displayName = displayName;
			}
			if (description != null)
			{
				this.description = description;
			}
			if (isDebuff.GetValueOrDefault(defaultIsDebuff) && Game1.player.isWearingRing("525") && this.millisecondsDuration != -2)
			{
				this.millisecondsDuration /= 2;
			}
			this.totalMillisecondsDuration = this.millisecondsDuration;
			if (effects != null)
			{
				this.effects.Add(effects);
			}
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x00018CDA File Offset: 0x00016EDA
		public bool HasAnyEffects()
		{
			return this.effects.HasAnyValue();
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x00018CE8 File Offset: 0x00016EE8
		public string getTimeLeft()
		{
			return string.Concat(new string[]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.476"),
				(this.millisecondsDuration / 60000).ToString(),
				":",
				(this.millisecondsDuration % 60000 / 10000).ToString(),
				(this.millisecondsDuration % 60000 % 10000 / 1000).ToString()
			});
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x00018D74 File Offset: 0x00016F74
		public virtual bool update(GameTime time)
		{
			if (this.millisecondsDuration == -2 || !Game1.shouldTimePass(false))
			{
				return false;
			}
			int old = this.millisecondsDuration;
			this.millisecondsDuration -= time.ElapsedGameTime.Milliseconds;
			if (this.id == "13" && old % 500 < this.millisecondsDuration % 500 && old < 3000)
			{
				Game1.multiplayer.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(44, Game1.player.getStandingPosition() + new Vector2((float)(-40 + Game1.random.Next(-8, 12)), (float)Game1.random.Next(-32, -16)), Color.Green * 0.5f, 8, Game1.random.NextBool(), 70f, 0, -1, -1f, -1, 0)
					{
						scale = 1f
					}
				});
			}
			return this.millisecondsDuration <= 0;
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x00018E88 File Offset: 0x00017088
		public virtual void OnAdded()
		{
			if (this.id == "19")
			{
				Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), Game1.player.getStandingPosition() + new Vector2(-32f, -21f), false, 0f, Color.White)
				{
					layerDepth = (float)(Game1.player.StandingPixel.Y + 1) / 10000f,
					animationLength = 1,
					interval = 2000f,
					scale = 4f
				});
			}
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x00018F3C File Offset: 0x0001713C
		public virtual void OnRemoved()
		{
		}

		// Token: 0x040001EF RID: 495
		public const float glowRate = 0.05f;

		// Token: 0x040001F0 RID: 496
		public const int ENDLESS = -2;

		// Token: 0x040001F1 RID: 497
		public const int farming = 0;

		// Token: 0x040001F2 RID: 498
		public const int fishing = 1;

		// Token: 0x040001F3 RID: 499
		public const int mining = 2;

		// Token: 0x040001F4 RID: 500
		public const int luck = 4;

		// Token: 0x040001F5 RID: 501
		public const int foraging = 5;

		// Token: 0x040001F6 RID: 502
		public const int maxStamina = 7;

		// Token: 0x040001F7 RID: 503
		public const int magneticRadius = 8;

		// Token: 0x040001F8 RID: 504
		public const int speed = 9;

		// Token: 0x040001F9 RID: 505
		public const int defense = 10;

		// Token: 0x040001FA RID: 506
		public const int attack = 11;

		// Token: 0x040001FB RID: 507
		public const string goblinsCurse = "12";

		// Token: 0x040001FC RID: 508
		public const string slimed = "13";

		// Token: 0x040001FD RID: 509
		public const string evilEye = "14";

		// Token: 0x040001FE RID: 510
		public const string tipsy = "17";

		// Token: 0x040001FF RID: 511
		public const string fear = "18";

		// Token: 0x04000200 RID: 512
		public const string frozen = "19";

		// Token: 0x04000201 RID: 513
		public const string warriorEnergy = "20";

		// Token: 0x04000202 RID: 514
		public const string yobaBlessing = "21";

		// Token: 0x04000203 RID: 515
		public const string adrenalineRush = "22";

		// Token: 0x04000204 RID: 516
		public const string avoidMonsters = "23";

		// Token: 0x04000205 RID: 517
		public const string full = "6";

		// Token: 0x04000206 RID: 518
		public const string quenched = "7";

		// Token: 0x04000207 RID: 519
		public const string spawnMonsters = "24";

		// Token: 0x04000208 RID: 520
		public const string nauseous = "25";

		// Token: 0x04000209 RID: 521
		public const string darkness = "26";

		// Token: 0x0400020A RID: 522
		public const string weakness = "27";

		// Token: 0x0400020B RID: 523
		public const string squidInkRavioli = "28";

		// Token: 0x0400020C RID: 524
		public const int fullnessLength = 180000;

		// Token: 0x0400020D RID: 525
		public const int quenchedLength = 60000;

		// Token: 0x0400020E RID: 526
		public int millisecondsDuration;

		// Token: 0x0400020F RID: 527
		public int totalMillisecondsDuration;

		// Token: 0x04000210 RID: 528
		public readonly BuffEffects effects = new BuffEffects();

		// Token: 0x04000211 RID: 529
		public readonly string id;

		// Token: 0x04000212 RID: 530
		public string displayName;

		// Token: 0x04000213 RID: 531
		public string description;

		// Token: 0x04000214 RID: 532
		public string source;

		// Token: 0x04000215 RID: 533
		public string displaySource;

		// Token: 0x04000216 RID: 534
		public Texture2D iconTexture;

		// Token: 0x04000217 RID: 535
		public int iconSheetIndex;

		// Token: 0x04000218 RID: 536
		public Color glow;

		// Token: 0x04000219 RID: 537
		public float displayAlphaTimer;

		// Token: 0x0400021A RID: 538
		public bool alreadyUpdatedIconAlpha;

		// Token: 0x0400021B RID: 539
		public string[] actionsOnApply;

		// Token: 0x0400021C RID: 540
		public bool visible = true;

		// Token: 0x0400021D RID: 541
		public readonly Dictionary<string, string> customFields = new Dictionary<string, string>();
	}
}

using System;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Buffs
{
	// Token: 0x0200038B RID: 907
	public class BuffAttributeDisplay
	{
		// Token: 0x060037F2 RID: 14322 RVA: 0x002C4C11 File Offset: 0x002C2E11
		public BuffAttributeDisplay(Func<Texture2D> texture, int spriteIndex, Func<Buff, float> value, Func<float, string> description)
		{
			this.Texture = texture;
			this.SpriteIndex = spriteIndex;
			this.Value = value;
			this.Description = description;
		}

		// Token: 0x060037F3 RID: 14323 RVA: 0x002C4C38 File Offset: 0x002C2E38
		public BuffAttributeDisplay(int spriteIndex, Func<BuffEffects, NetFloat> value, string descriptionKey)
		{
			this.Texture = (() => Game1.buffsIcons);
			this.SpriteIndex = spriteIndex;
			this.Value = ((Buff buff) => value(buff.effects).Value);
			this.Description = delegate(float buffValue)
			{
				string valueString = (buffValue > 0f) ? ("+" + buffValue.ToString()) : (buffValue.ToString() ?? "");
				string name = Game1.content.LoadString(descriptionKey);
				LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
				if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.es || currentLanguageCode == LocalizedContentManager.LanguageCode.ko)
				{
					return name + valueString;
				}
				return valueString + name;
			};
		}

		// Token: 0x0400244B RID: 9291
		public readonly Func<Texture2D> Texture;

		// Token: 0x0400244C RID: 9292
		public readonly int SpriteIndex;

		// Token: 0x0400244D RID: 9293
		public readonly Func<Buff, float> Value;

		// Token: 0x0400244E RID: 9294
		public readonly Func<float, string> Description;
	}
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x02000249 RID: 585
	internal class TextCreditsBlock : ICreditsBlock
	{
		// Token: 0x060026E9 RID: 9961 RVA: 0x001B82A4 File Offset: 0x001B64A4
		public TextCreditsBlock(string rawtext)
		{
			string[] split = rawtext.Split(']', StringSplitOptions.None);
			if (split.Length > 1)
			{
				this.text = split[1];
				this.color = SpriteText.getColorFromIndex(Convert.ToInt32(split[0].Substring(1)));
			}
			else
			{
				this.text = split[0];
				this.color = SpriteText.color_White;
			}
			if (SpriteText.IsMissingCharacters(rawtext))
			{
				this.renderNameInEnglish = true;
			}
		}

		// Token: 0x060026EA RID: 9962 RVA: 0x001B8310 File Offset: 0x001B6510
		public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
			if (!this.renderNameInEnglish)
			{
				SpriteText.drawString(b, this.text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, false, -1, "", new Color?(this.color), SpriteText.ScrollTextAlignment.Left);
				return;
			}
			int parenthesis_index = this.text.IndexOf('(');
			if (parenthesis_index != -1 && parenthesis_index > 0)
			{
				string name = this.text.Substring(0, parenthesis_index);
				string parenthesis_text = this.text.Substring(parenthesis_index);
				SpriteText.forceEnglishFont = true;
				int width_of_text = (int)((float)SpriteText.getWidthOfString(name, 999999) / SpriteText.FontPixelZoom * 3f);
				SpriteText.drawString(b, name, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, false, -1, "", new Color?(this.color), SpriteText.ScrollTextAlignment.Left);
				SpriteText.forceEnglishFont = false;
				SpriteText.drawString(b, parenthesis_text, topLeftX + width_of_text, topLeftY, 999999, -1, 99999, 1f, 0.88f, false, -1, "", new Color?(this.color), SpriteText.ScrollTextAlignment.Left);
				return;
			}
			SpriteText.forceEnglishFont = true;
			SpriteText.drawString(b, this.text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, false, -1, "", new Color?(this.color), SpriteText.ScrollTextAlignment.Left);
			SpriteText.forceEnglishFont = false;
		}

		// Token: 0x060026EB RID: 9963 RVA: 0x001B8467 File Offset: 0x001B6667
		public override int getHeight(int maxWidth)
		{
			if (!(this.text == ""))
			{
				return SpriteText.getHeightOfString(this.text, maxWidth);
			}
			return 64;
		}

		// Token: 0x04001818 RID: 6168
		private string text;

		// Token: 0x04001819 RID: 6169
		private Color color;

		// Token: 0x0400181A RID: 6170
		private bool renderNameInEnglish;
	}
}

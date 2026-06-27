using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x0200024A RID: 586
	internal class LinkCreditsBlock : ICreditsBlock
	{
		// Token: 0x060026EC RID: 9964 RVA: 0x001B848A File Offset: 0x001B668A
		public LinkCreditsBlock(string text, string url)
		{
			this.text = text;
			this.url = url;
		}

		// Token: 0x060026ED RID: 9965 RVA: 0x001B84A0 File Offset: 0x001B66A0
		public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
			SpriteText.drawString(b, this.text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, false, -1, "", new Color?(this.currentlyHovered ? SpriteText.color_Green : SpriteText.color_Cyan), SpriteText.ScrollTextAlignment.Left);
			this.currentlyHovered = false;
		}

		// Token: 0x060026EE RID: 9966 RVA: 0x001B84F9 File Offset: 0x001B66F9
		public override int getHeight(int maxWidth)
		{
			if (!(this.text == ""))
			{
				return SpriteText.getHeightOfString(this.text, maxWidth);
			}
			return 64;
		}

		// Token: 0x060026EF RID: 9967 RVA: 0x001B851C File Offset: 0x001B671C
		public override void hovered()
		{
			this.currentlyHovered = true;
		}

		// Token: 0x060026F0 RID: 9968 RVA: 0x001B8525 File Offset: 0x001B6725
		private static void LaunchBrowser(string url)
		{
			Process.Start(new ProcessStartInfo(url)
			{
				UseShellExecute = true
			});
		}

		// Token: 0x060026F1 RID: 9969 RVA: 0x001B853C File Offset: 0x001B673C
		public override void clicked()
		{
			Game1.playSound("bigSelect", null);
			try
			{
				LinkCreditsBlock.LaunchBrowser(this.url);
			}
			catch (Exception e)
			{
				Game1.log.Error("Could not open credit link.", e);
			}
		}

		// Token: 0x0400181B RID: 6171
		private string text;

		// Token: 0x0400181C RID: 6172
		private string url;

		// Token: 0x0400181D RID: 6173
		private bool currentlyHovered;
	}
}

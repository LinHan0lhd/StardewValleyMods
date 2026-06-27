using System;
using System.Collections.Generic;
using System.Linq;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using xTile;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003AA RID: 938
	public class SpriteText
	{
		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x060038F0 RID: 14576 RVA: 0x002D2061 File Offset: 0x002D0261
		public static float FontPixelZoom
		{
			get
			{
				return SpriteText.fontPixelZoom + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? ((Game1.options.dialogueFontScale - 1f) / (Game1.options.useChineseSmoothFont ? 4f : 2f)) : 0f);
			}
		}

		// Token: 0x060038F1 RID: 14577 RVA: 0x002D20A4 File Offset: 0x002D02A4
		public static void drawStringHorizontallyCenteredAt(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, Color? color = null, int maxWidth = 99999)
		{
			SpriteText.drawString(b, s, x - SpriteText.getWidthOfString(s, maxWidth) / 2, y, characterPosition, width, height, alpha, layerDepth, junimoText, -1, "", color, SpriteText.ScrollTextAlignment.Left);
		}

		// Token: 0x060038F2 RID: 14578 RVA: 0x002D20DC File Offset: 0x002D02DC
		public static int getWidthOfString(string s, int widthConstraint = 999999)
		{
			SpriteText.setUpCharacterMap();
			int width = 0;
			int maxWidth = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (SpriteText.isUsingNonSpriteSheetFont() && !SpriteText.forceEnglishFont)
				{
					FontChar c;
					if (SpriteText.characterMap.TryGetValue(s[i], out c))
					{
						width += c.XAdvance;
					}
					maxWidth = Math.Max(width, maxWidth);
					if (s[i] == '^' || (float)width * SpriteText.FontPixelZoom > (float)widthConstraint)
					{
						width = 0;
					}
				}
				else
				{
					width += 8 + SpriteText.getWidthOffsetForChar(s[i]);
					if (i > 0)
					{
						width += SpriteText.getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
					}
					maxWidth = Math.Max(width, maxWidth);
					float pos = (float)SpriteText.positionOfNextSpace(s, i, (int)((float)width * SpriteText.FontPixelZoom), 0);
					if (s[i] == '^' || (float)width * SpriteText.FontPixelZoom >= (float)widthConstraint || pos >= (float)widthConstraint)
					{
						width = 0;
					}
				}
			}
			return (int)((float)maxWidth * SpriteText.FontPixelZoom);
		}

		// Token: 0x060038F3 RID: 14579 RVA: 0x002D21C8 File Offset: 0x002D03C8
		public static bool IsMissingCharacters(string text)
		{
			SpriteText.setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin && !SpriteText.forceEnglishFont)
			{
				for (int i = 0; i < text.Length; i++)
				{
					if (!SpriteText.characterMap.ContainsKey(text[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060038F4 RID: 14580 RVA: 0x002D2210 File Offset: 0x002D0410
		public static int getHeightOfString(string s, int widthConstraint = 999999)
		{
			if (s.Length == 0)
			{
				return 0;
			}
			Vector2 position = default(Vector2);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			s = s.Replace(Environment.NewLine, "");
			SpriteText.setUpCharacterMap();
			if (SpriteText.isUsingNonSpriteSheetFont() && !SpriteText.forceEnglishFont)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '^')
					{
						position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
						position.X = 0f;
					}
					else
					{
						if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= widthConstraint)
						{
							position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
							accumulatedHorizontalSpaceBetweenCharacters = 0;
							position.X = 0f;
						}
						FontChar c;
						if (SpriteText.characterMap.TryGetValue(s[i], out c))
						{
							position.X += (float)c.XAdvance * SpriteText.FontPixelZoom;
						}
					}
				}
				return (int)(position.Y + (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom);
			}
			for (int j = 0; j < s.Length; j++)
			{
				if (s[j] == '^')
				{
					position.Y += 18f * SpriteText.FontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
				}
				else
				{
					if (SpriteText.positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= widthConstraint)
					{
						position.Y += 18f * SpriteText.FontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = 0f;
					}
					position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)SpriteText.getWidthOffsetForChar(s[j]) * SpriteText.FontPixelZoom;
					if (j > 0)
					{
						position.X += (float)SpriteText.getWidthOffsetForChar(s[j - 1]) * SpriteText.FontPixelZoom;
					}
					accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
				}
			}
			return (int)(position.Y + 16f * SpriteText.FontPixelZoom);
		}

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x060038F5 RID: 14581 RVA: 0x002D243B File Offset: 0x002D063B
		public static Color color_Default
		{
			get
			{
				if (!LocalizedContentManager.CurrentLanguageLatin && (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ru || Game1.options.useAlternateFont))
				{
					return new Color(86, 22, 12);
				}
				return Color.White;
			}
		}

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x060038F6 RID: 14582 RVA: 0x002D2469 File Offset: 0x002D0669
		public static Color color_Black { get; } = Color.Black;

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x060038F7 RID: 14583 RVA: 0x002D2470 File Offset: 0x002D0670
		public static Color color_Blue { get; } = Color.SkyBlue;

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x060038F8 RID: 14584 RVA: 0x002D2477 File Offset: 0x002D0677
		public static Color color_Red { get; } = Color.Red;

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x060038F9 RID: 14585 RVA: 0x002D247E File Offset: 0x002D067E
		public static Color color_Purple { get; } = new Color(110, 43, 255);

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x060038FA RID: 14586 RVA: 0x002D2485 File Offset: 0x002D0685
		public static Color color_White { get; } = Color.White;

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x060038FB RID: 14587 RVA: 0x002D248C File Offset: 0x002D068C
		public static Color color_Orange { get; } = Color.OrangeRed;

		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x060038FC RID: 14588 RVA: 0x002D2493 File Offset: 0x002D0693
		public static Color color_Green { get; } = Color.LimeGreen;

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x060038FD RID: 14589 RVA: 0x002D249A File Offset: 0x002D069A
		public static Color color_Cyan { get; } = Color.Cyan;

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x060038FE RID: 14590 RVA: 0x002D24A1 File Offset: 0x002D06A1
		public static Color color_Gray { get; } = new Color(60, 60, 60);

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x060038FF RID: 14591 RVA: 0x002D24A8 File Offset: 0x002D06A8
		public static Color color_JojaBlue { get; } = new Color(52, 50, 122);

		// Token: 0x06003900 RID: 14592 RVA: 0x002D24B0 File Offset: 0x002D06B0
		public static Color getColorFromIndex(int index)
		{
			switch (index)
			{
			case -1:
				return SpriteText.color_Default;
			case 1:
				return SpriteText.color_Blue;
			case 2:
				return SpriteText.color_Red;
			case 3:
				return SpriteText.color_Purple;
			case 4:
				return SpriteText.color_White;
			case 5:
				return SpriteText.color_Orange;
			case 6:
				return SpriteText.color_Green;
			case 7:
				return SpriteText.color_Cyan;
			case 8:
				return SpriteText.color_Gray;
			case 9:
				return SpriteText.color_JojaBlue;
			}
			return Color.Black;
		}

		// Token: 0x06003901 RID: 14593 RVA: 0x002D2534 File Offset: 0x002D0734
		public static string getSubstringBeyondHeight(string s, int width, int height)
		{
			Vector2 position = default(Vector2);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			s = s.Replace(Environment.NewLine, "");
			SpriteText.setUpCharacterMap();
			if (SpriteText.isUsingNonSpriteSheetFont())
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '^')
					{
						position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
						position.X = 0f;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
					}
					else
					{
						FontChar c;
						if (SpriteText.characterMap.TryGetValue(s[i], out c))
						{
							if (i > 0)
							{
								position.X += (float)c.XAdvance * SpriteText.FontPixelZoom;
							}
							if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
							{
								position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
								accumulatedHorizontalSpaceBetweenCharacters = 0;
								position.X = 0f;
							}
						}
						if (position.Y >= (float)height - (float)SpriteText.FontFile.Common.LineHeight * SpriteText.FontPixelZoom * 2f)
						{
							return s.Substring(SpriteText.getLastSpace(s, i));
						}
					}
				}
				return "";
			}
			for (int j = 0; j < s.Length; j++)
			{
				if (s[j] == '^')
				{
					position.Y += 18f * SpriteText.FontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
				}
				else
				{
					if (j > 0)
					{
						position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(SpriteText.getWidthOffsetForChar(s[j]) + SpriteText.getWidthOffsetForChar(s[j - 1])) * SpriteText.FontPixelZoom;
					}
					accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
					if (SpriteText.positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
					{
						position.Y += 18f * SpriteText.FontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = 0f;
					}
					if (position.Y >= (float)height - 16f * SpriteText.FontPixelZoom * 2f)
					{
						return s.Substring(SpriteText.getLastSpace(s, j));
					}
				}
			}
			return "";
		}

		// Token: 0x06003902 RID: 14594 RVA: 0x002D277C File Offset: 0x002D097C
		public static int getIndexOfSubstringBeyondHeight(string s, int width, int height)
		{
			Vector2 position = default(Vector2);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			s = s.Replace(Environment.NewLine, "");
			SpriteText.setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '^')
					{
						position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
						position.X = 0f;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
					}
					else
					{
						FontChar c;
						if (SpriteText.characterMap.TryGetValue(s[i], out c))
						{
							if (i > 0)
							{
								position.X += (float)c.XAdvance * SpriteText.FontPixelZoom;
							}
							if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
							{
								position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
								accumulatedHorizontalSpaceBetweenCharacters = 0;
								position.X = 0f;
							}
						}
						if (position.Y >= (float)height - (float)SpriteText.FontFile.Common.LineHeight * SpriteText.FontPixelZoom * 2f)
						{
							return i - 1;
						}
					}
				}
				return s.Length - 1;
			}
			for (int j = 0; j < s.Length; j++)
			{
				if (s[j] == '^')
				{
					position.Y += 18f * SpriteText.FontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
				}
				else
				{
					if (j > 0)
					{
						position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(SpriteText.getWidthOffsetForChar(s[j]) + SpriteText.getWidthOffsetForChar(s[j - 1])) * SpriteText.FontPixelZoom;
					}
					accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
					if (SpriteText.positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
					{
						position.Y += 18f * SpriteText.FontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = 0f;
					}
					if (position.Y >= (float)height - 16f * SpriteText.FontPixelZoom)
					{
						return j - 1;
					}
				}
			}
			return s.Length - 1;
		}

		// Token: 0x06003903 RID: 14595 RVA: 0x002D29B0 File Offset: 0x002D0BB0
		public static List<string> getStringBrokenIntoSectionsOfHeight(string s, int width, int height)
		{
			List<string> brokenUp = new List<string>();
			while (s.Length > 0)
			{
				string tmp = SpriteText.getStringPreviousToThisHeightCutoff(s, width, height);
				if (tmp.Length <= 0)
				{
					break;
				}
				brokenUp.Add(tmp);
				s = s.Substring(brokenUp.Last<string>().Length);
			}
			return brokenUp;
		}

		// Token: 0x06003904 RID: 14596 RVA: 0x002D29FB File Offset: 0x002D0BFB
		public static string getStringPreviousToThisHeightCutoff(string s, int width, int height)
		{
			return s.Substring(0, SpriteText.getIndexOfSubstringBeyondHeight(s, width, height) + 1);
		}

		// Token: 0x06003905 RID: 14597 RVA: 0x002D2A10 File Offset: 0x002D0C10
		private static int getLastSpace(string s, int startIndex)
		{
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				return startIndex;
			}
			for (int i = startIndex; i >= 0; i--)
			{
				if (s[i] == ' ')
				{
					return i;
				}
			}
			return startIndex;
		}

		// Token: 0x06003906 RID: 14598 RVA: 0x002D2A54 File Offset: 0x002D0C54
		public static int getWidthOffsetForChar(char c)
		{
			if (c > '^')
			{
				if (c <= '¡')
				{
					switch (c)
					{
					case 'i':
						break;
					case 'j':
					case 'l':
						return -1;
					case 'k':
						return 0;
					default:
						if (c != '¡')
						{
							return 0;
						}
						return -1;
					}
				}
				else
				{
					switch (c)
					{
					case 'ì':
					case 'í':
					case 'î':
					case 'ï':
						break;
					default:
						if (c != 'ı')
						{
							if (c != 'ş')
							{
								return 0;
							}
							return -1;
						}
						break;
					}
				}
				return -1;
			}
			if (c <= '$')
			{
				if (c != '!')
				{
					if (c != '$')
					{
						return 0;
					}
					return 1;
				}
			}
			else
			{
				if (c == ',' || c == '.')
				{
					return -2;
				}
				if (c != '^')
				{
					return 0;
				}
				return -8;
			}
			return -1;
		}

		// Token: 0x06003907 RID: 14599 RVA: 0x002D2AF0 File Offset: 0x002D0CF0
		public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, int width, float alpha = 1f, Color? color = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
		{
			SpriteText.drawString(b, s, x - width / 2, y, 999999, width, 999999, alpha, layerDepth, junimoText, scrollType, "", color, SpriteText.ScrollTextAlignment.Center);
		}

		// Token: 0x06003908 RID: 14600 RVA: 0x002D2B28 File Offset: 0x002D0D28
		public static void drawSmallTextBubble(SpriteBatch b, string s, Vector2 positionOfBottomCenter, int maxWidth = -1, float layerDepth = -1f, bool drawPointerOnTop = false)
		{
			if (maxWidth != -1)
			{
				s = Game1.parseText(s, Game1.smallFont, maxWidth - 16);
			}
			s = s.Trim();
			Vector2 size = Game1.smallFont.MeasureString(s);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(241, 503, 9, 9), (int)(positionOfBottomCenter.X - size.X / 2f - 4f), (int)(positionOfBottomCenter.Y - size.Y), (int)size.X + 16, (int)size.Y + 12, Color.White, 4f, false, layerDepth);
			if (drawPointerOnTop)
			{
				b.Draw(Game1.mouseCursors_1_6, positionOfBottomCenter + new Vector2(-4f, -3f) * 4f + new Vector2(size.X / 2f, -size.Y), new Rectangle?(new Rectangle(251, 506, 5, 5)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, layerDepth + 1E-05f);
			}
			else
			{
				b.Draw(Game1.mouseCursors_1_6, positionOfBottomCenter + new Vector2(-2.5f, 1f) * 4f, new Rectangle?(new Rectangle(251, 506, 5, 5)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 1E-05f);
			}
			Utility.drawTextWithShadow(b, s, Game1.smallFont, positionOfBottomCenter - size + new Vector2(4f + size.X / 2f, 8f), Game1.textColor, 1f, layerDepth + 2E-05f, -1, -1, 0.5f, 3);
		}

		// Token: 0x06003909 RID: 14601 RVA: 0x002D2CF0 File Offset: 0x002D0EF0
		public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, Color? color = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
		{
			SpriteText.drawString(b, s, x - SpriteText.getWidthOfString((placeHolderWidthText.Length > 0) ? placeHolderWidthText : s, 999999) / 2, y, 999999, -1, 999999, alpha, layerDepth, junimoText, scrollType, placeHolderWidthText, color, SpriteText.ScrollTextAlignment.Center);
		}

		// Token: 0x0600390A RID: 14602 RVA: 0x002D2D3C File Offset: 0x002D0F3C
		public static void drawStringWithScrollBackground(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, Color? color = null, SpriteText.ScrollTextAlignment scroll_text_alignment = SpriteText.ScrollTextAlignment.Left)
		{
			SpriteText.drawString(b, s, x, y, 999999, -1, 999999, alpha, 0.88f, false, 0, placeHolderWidthText, color, scroll_text_alignment);
		}

		// Token: 0x0600390B RID: 14603 RVA: 0x002D2D6C File Offset: 0x002D0F6C
		private static FontFile loadFont(string assetName)
		{
			return FontLoader.Parse(Game1.content.Load<XmlSource>(assetName).Source);
		}

		// Token: 0x0600390C RID: 14604 RVA: 0x002D2D83 File Offset: 0x002D0F83
		private static void setUpCharacterMap()
		{
			if (!LocalizedContentManager.CurrentLanguageLatin && SpriteText.characterMap == null)
			{
				LocalizedContentManager.OnLanguageChange += SpriteText.OnLanguageChange;
				SpriteText.LoadFontData(LocalizedContentManager.CurrentLanguageCode);
			}
		}

		// Token: 0x0600390D RID: 14605 RVA: 0x002D2DB0 File Offset: 0x002D0FB0
		public static void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "", Color? color = null, SpriteText.ScrollTextAlignment scroll_text_alignment = SpriteText.ScrollTextAlignment.Left)
		{
			SpriteText.setUpCharacterMap();
			bool isCustomColor = color != null;
			color = new Color?(color ?? SpriteText.color_Default);
			bool widthSpecified = width != -1;
			if (!widthSpecified)
			{
				width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
				if (drawBGScroll == 1)
				{
					width = SpriteText.getWidthOfString(s, 999999) * 2;
				}
			}
			if (SpriteText.FontPixelZoom < 4f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.zh)
			{
				y += (int)((4f - SpriteText.FontPixelZoom) * 4f);
			}
			Vector2 position = new Vector2((float)x, (float)y);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			if (drawBGScroll != 1)
			{
				if (position.X + (float)width > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 4))
				{
					position.X = (float)(Game1.graphics.GraphicsDevice.Viewport.Width - width - 4);
				}
				if (position.X < 0f)
				{
					position.X = 0f;
				}
			}
			switch (drawBGScroll)
			{
			case 0:
			case 2:
			case 3:
			{
				int scroll_width = SpriteText.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s, 999999);
				if (widthSpecified)
				{
					scroll_width = width;
				}
				switch (drawBGScroll)
				{
				case 0:
					b.Draw(Game1.mouseCursors, position + new Vector2(-12f, -3f) * 4f, new Rectangle?(new Rectangle(325, 318, 12, 18)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(337, 318, 1, 18)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)scroll_width, 4f), SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2((float)scroll_width, -12f), new Rectangle?(new Rectangle(338, 318, 12, 18)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, position + new Vector2(-3f, -3f) * 4f, new Rectangle?(new Rectangle(327, 281, 3, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(330, 281, 1, 17)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)(scroll_width + 4), 4f), SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2((float)(scroll_width + 4), -12f), new Rectangle?(new Rectangle(333, 281, 3, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					break;
				case 3:
					b.Draw(Game1.mouseCursors_1_6, position + new Vector2(-3f, -3f) * 4f, new Rectangle?(new Rectangle(86, 145, 3, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors_1_6, position + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(89, 145, 1, 17)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)(scroll_width + 4), 4f), SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors_1_6, position + new Vector2((float)(scroll_width + 4), -12f), new Rectangle?(new Rectangle(92, 145, 3, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					break;
				}
				if (scroll_text_alignment != SpriteText.ScrollTextAlignment.Center)
				{
					if (scroll_text_alignment == SpriteText.ScrollTextAlignment.Right)
					{
						x += scroll_width - SpriteText.getWidthOfString(s, 999999);
						position.X = (float)x;
					}
				}
				else
				{
					x += (scroll_width - SpriteText.getWidthOfString(s, 999999)) / 2;
					position.X = (float)x;
				}
				position.Y += (4f - SpriteText.FontPixelZoom) * 4f;
				break;
			}
			case 1:
			{
				int text_width = SpriteText.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s, 999999);
				Vector2 speech_position = position;
				GameLocation currentLocation = Game1.currentLocation;
				bool flag;
				if (currentLocation == null)
				{
					flag = (null != null);
				}
				else
				{
					Map map = currentLocation.map;
					flag = (((map != null) ? map.Layers[0] : null) != null);
				}
				if (flag)
				{
					int left_edge = 0 - Game1.viewport.X + 28;
					int right_edge = 0 - Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
					if (position.X < (float)left_edge)
					{
						position.X = (float)left_edge;
					}
					if (position.X + (float)text_width > (float)right_edge)
					{
						position.X = (float)(right_edge - text_width);
					}
					speech_position.X += (float)(text_width / 2);
					if (speech_position.X < position.X)
					{
						position.X += speech_position.X - position.X;
					}
					if (speech_position.X > position.X + (float)text_width - 24f)
					{
						position.X += speech_position.X - (position.X + (float)text_width - 24f);
					}
					speech_position.X = Utility.Clamp(speech_position.X, position.X, position.X + (float)text_width - 24f);
				}
				b.Draw(Game1.mouseCursors, position + new Vector2(-7f, -3f) * 4f, new Rectangle?(new Rectangle(324, 299, 7, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(331, 299, 1, 17)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)SpriteText.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s, 999999), 4f), SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2((float)text_width, -12f), new Rectangle?(new Rectangle(332, 299, 7, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, speech_position + new Vector2(0f, 52f), new Rectangle?(new Rectangle(341, 308, 6, 5)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
				x = (int)position.X;
				if (placeHolderScrollWidthText.Length > 0)
				{
					x += SpriteText.getWidthOfString(placeHolderScrollWidthText, 999999) / 2 - SpriteText.getWidthOfString(s, 999999) / 2;
					position.X = (float)x;
				}
				position.Y += (4f - SpriteText.FontPixelZoom) * 4f;
				break;
			}
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				position.Y -= 8f;
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				if (drawBGScroll != -1)
				{
					float factor = 3.5f;
					if (Game1.options.useChineseSmoothFont)
					{
						position.Y -= 2f;
						factor = 3.8f;
					}
					else
					{
						position.Y += 4f;
					}
					position.Y -= (SpriteText.FontPixelZoom - 0.75f) * 4f * factor;
				}
				else
				{
					position.Y += 4f;
				}
			}
			s = s.Replace(Environment.NewLine, "");
			if (!junimoText && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.FontApplyYOffset)))
			{
				position.Y -= (4f - SpriteText.FontPixelZoom) * 4f;
			}
			s = s.Replace('♡', '<');
			for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
			{
				if (LocalizedContentManager.CurrentLanguageLatin || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru && !Game1.options.useAlternateFont) || SpriteText.IsSpecialCharacter(s[i]) || junimoText || SpriteText.forceEnglishFont)
				{
					float tempzoom = SpriteText.fontPixelZoom;
					if (SpriteText.IsSpecialCharacter(s[i]) || junimoText || SpriteText.forceEnglishFont)
					{
						SpriteText.fontPixelZoom = 3f;
					}
					if (s[i] == '^')
					{
						position.Y += 18f * SpriteText.FontPixelZoom;
						position.X = (float)x;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						SpriteText.fontPixelZoom = tempzoom;
					}
					else
					{
						accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
						bool upper = char.IsUpper(s[i]) || s[i] == 'ß';
						Vector2 spriteFontOffset = new Vector2(0f, (float)(-1 + ((!junimoText && upper) ? -3 : 0)));
						if (s[i] == 'Ç')
						{
							spriteFontOffset.Y += 2f;
						}
						if (SpriteText.positionOfNextSpace(s, i, (int)position.X - x, accumulatedHorizontalSpaceBetweenCharacters) >= width)
						{
							position.Y += 18f * SpriteText.FontPixelZoom;
							accumulatedHorizontalSpaceBetweenCharacters = 0;
							position.X = (float)x;
							if (s[i] == ' ')
							{
								SpriteText.fontPixelZoom = tempzoom;
								goto IL_E95;
							}
						}
						Rectangle srcRect = SpriteText.getSourceRectForChar(s[i], junimoText);
						b.Draw(isCustomColor ? SpriteText.coloredTexture : SpriteText.spriteTexture, position + spriteFontOffset * SpriteText.FontPixelZoom, new Rectangle?(srcRect), ((SpriteText.IsSpecialCharacter(s[i]) || junimoText) ? Color.White : color.Value) * alpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
						if (i < s.Length - 1)
						{
							position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)SpriteText.getWidthOffsetForChar(s[i + 1]) * SpriteText.FontPixelZoom;
						}
						if (s[i] != '^')
						{
							position.X += (float)SpriteText.getWidthOffsetForChar(s[i]) * SpriteText.FontPixelZoom;
						}
						SpriteText.fontPixelZoom = tempzoom;
					}
				}
				else if (s[i] == '^')
				{
					position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
					position.X = (float)x;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
				}
				else
				{
					if (i > 0 && SpriteText.IsSpecialCharacter(s[i - 1]))
					{
						position.X += 24f;
					}
					FontChar fc;
					if (SpriteText.characterMap.TryGetValue(s[i], out fc))
					{
						Rectangle sourcerect = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
						Texture2D _texture = SpriteText.fontPages[fc.Page];
						if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= x + width - 4)
						{
							position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
							accumulatedHorizontalSpaceBetweenCharacters = 0;
							position.X = (float)x;
						}
						Vector2 position2 = new Vector2(position.X + (float)fc.XOffset * SpriteText.FontPixelZoom, position.Y + (float)fc.YOffset * SpriteText.FontPixelZoom);
						if (drawBGScroll != -1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
						{
							position2.Y -= 8f;
						}
						if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
						{
							Vector2 offset = new Vector2(-1f, 1f) * SpriteText.FontPixelZoom;
							b.Draw(_texture, position2 + offset, new Rectangle?(sourcerect), color.Value * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
							b.Draw(_texture, position2 + new Vector2(0f, offset.Y), new Rectangle?(sourcerect), color.Value * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
							b.Draw(_texture, position2 + new Vector2(offset.X, 0f), new Rectangle?(sourcerect), color.Value * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
						}
						b.Draw(_texture, position2, new Rectangle?(sourcerect), color.Value * alpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
						position.X += (float)fc.XAdvance * SpriteText.FontPixelZoom;
					}
				}
				IL_E95:;
			}
		}

		// Token: 0x0600390E RID: 14606 RVA: 0x002D3C6C File Offset: 0x002D1E6C
		private static bool IsSpecialCharacter(char c)
		{
			return c.Equals('<') || c.Equals('=') || c.Equals('>') || c.Equals('@') || c.Equals('$') || c.Equals('`') || c.Equals('+');
		}

		// Token: 0x0600390F RID: 14607 RVA: 0x002D3CC6 File Offset: 0x002D1EC6
		private static void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			SpriteText.LoadFontData(code);
		}

		// Token: 0x06003910 RID: 14608 RVA: 0x002D3CD0 File Offset: 0x002D1ED0
		public static void LoadFontData(LocalizedContentManager.LanguageCode code)
		{
			if (SpriteText.characterMap != null)
			{
				SpriteText.characterMap.Clear();
			}
			else
			{
				SpriteText.characterMap = new Dictionary<char, FontChar>();
			}
			if (SpriteText.fontPages != null)
			{
				SpriteText.fontPages.Clear();
			}
			else
			{
				SpriteText.fontPages = new List<Texture2D>();
			}
			string pathBase = "Fonts\\";
			switch (code)
			{
			case LocalizedContentManager.LanguageCode.ja:
				SpriteText.FontFile = SpriteText.loadFont(pathBase + "Japanese");
				SpriteText.fontPixelZoom = 1.75f;
				goto IL_17D;
			case LocalizedContentManager.LanguageCode.ru:
				SpriteText.FontFile = SpriteText.loadFont(pathBase + "Russian");
				SpriteText.fontPixelZoom = 3f;
				goto IL_17D;
			case LocalizedContentManager.LanguageCode.zh:
				if (Game1.options.useChineseSmoothFont)
				{
					pathBase += "Chinese_round\\";
					SpriteText.fontPixelZoom = 1f;
				}
				else
				{
					SpriteText.fontPixelZoom = 1.5f;
				}
				SpriteText.FontFile = SpriteText.loadFont(pathBase + "Chinese");
				goto IL_17D;
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.fr:
				break;
			case LocalizedContentManager.LanguageCode.th:
				SpriteText.FontFile = SpriteText.loadFont(pathBase + "Thai");
				SpriteText.fontPixelZoom = 1.5f;
				goto IL_17D;
			case LocalizedContentManager.LanguageCode.ko:
				SpriteText.FontFile = SpriteText.loadFont(pathBase + "Korean");
				SpriteText.fontPixelZoom = 1.5f;
				goto IL_17D;
			default:
				if (code == LocalizedContentManager.LanguageCode.mod)
				{
					SpriteText.FontFile = SpriteText.loadFont(LocalizedContentManager.CurrentModLanguage.FontFile);
					SpriteText.fontPixelZoom = LocalizedContentManager.CurrentModLanguage.FontPixelZoom;
					goto IL_17D;
				}
				break;
			}
			SpriteText.FontFile = null;
			SpriteText.fontPixelZoom = 3f;
			IL_17D:
			if (SpriteText.FontFile != null)
			{
				foreach (FontChar fontCharacter in SpriteText.FontFile.Chars)
				{
					char c = (char)fontCharacter.ID;
					SpriteText.characterMap.Add(c, fontCharacter);
				}
				foreach (FontPage fontPage in SpriteText.FontFile.Pages)
				{
					SpriteText.fontPages.Add(Game1.content.Load<Texture2D>(pathBase + fontPage.File));
				}
			}
		}

		// Token: 0x06003911 RID: 14609 RVA: 0x002D3F20 File Offset: 0x002D2120
		public static int positionOfNextSpace(string s, int index, int currentXPosition, int accumulatedHorizontalSpaceBetweenCharacters)
		{
			SpriteText.setUpCharacterMap();
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.zh || currentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				float result = (float)currentXPosition;
				foreach (char c in Game1.asianSpacingRegex.Match(s, index).Value)
				{
					FontChar fc;
					if (SpriteText.characterMap.TryGetValue(c, out fc))
					{
						result += (float)fc.XAdvance * SpriteText.FontPixelZoom;
					}
				}
				return (int)result;
			}
			for (int i = index; i < s.Length; i++)
			{
				if (SpriteText.isUsingNonSpriteSheetFont())
				{
					if (s[i] == ' ' || s[i] == '^')
					{
						return currentXPosition;
					}
					FontChar fc2;
					if (SpriteText.characterMap.TryGetValue(s[i], out fc2))
					{
						currentXPosition += (int)((float)fc2.XAdvance * SpriteText.FontPixelZoom);
					}
					else
					{
						currentXPosition += (int)((float)SpriteText.FontFile.Common.LineHeight * SpriteText.FontPixelZoom);
					}
				}
				else
				{
					if (s[i] == ' ' || s[i] == '^')
					{
						return currentXPosition;
					}
					currentXPosition += (int)(8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(SpriteText.getWidthOffsetForChar(s[i]) + SpriteText.getWidthOffsetForChar(s[Math.Max(0, i - 1)])) * SpriteText.FontPixelZoom);
					accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
				}
			}
			return currentXPosition;
		}

		// Token: 0x06003912 RID: 14610 RVA: 0x002D4085 File Offset: 0x002D2285
		private static bool isUsingNonSpriteSheetFont()
		{
			return !LocalizedContentManager.CurrentLanguageLatin && (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ru || Game1.options.useAlternateFont);
		}

		// Token: 0x06003913 RID: 14611 RVA: 0x002D40A4 File Offset: 0x002D22A4
		private static Rectangle getSourceRectForChar(char c, bool junimoText)
		{
			int i = (int)(c - ' ');
			if (c <= 'Ű')
			{
				if (c <= 'ğ')
				{
					if (c <= 'Ę')
					{
						switch (c)
						{
						case 'Ą':
							i = 576;
							goto IL_3CD;
						case 'ą':
							i = 578;
							goto IL_3CD;
						case 'Ć':
							i = 579;
							goto IL_3CD;
						case 'ć':
							i = 580;
							goto IL_3CD;
						default:
							if (c == 'Ę')
							{
								i = 581;
								goto IL_3CD;
							}
							break;
						}
					}
					else
					{
						if (c == 'ę')
						{
							i = 582;
							goto IL_3CD;
						}
						if (c == 'Ğ')
						{
							i = 102;
							goto IL_3CD;
						}
						if (c == 'ğ')
						{
							i = 103;
							goto IL_3CD;
						}
					}
				}
				else if (c <= 'ń')
				{
					if (c == 'İ')
					{
						i = 98;
						goto IL_3CD;
					}
					if (c == 'ı')
					{
						i = 99;
						goto IL_3CD;
					}
					switch (c)
					{
					case 'Ł':
						i = 583;
						goto IL_3CD;
					case 'ł':
						i = 584;
						goto IL_3CD;
					case 'Ń':
						i = 585;
						goto IL_3CD;
					case 'ń':
						i = 586;
						goto IL_3CD;
					}
				}
				else
				{
					switch (c)
					{
					case 'Ő':
						i = 105;
						goto IL_3CD;
					case 'ő':
						i = 106;
						goto IL_3CD;
					case 'Œ':
						i = 96;
						goto IL_3CD;
					case 'œ':
						i = 97;
						goto IL_3CD;
					default:
						switch (c)
						{
						case 'Ś':
							i = 574;
							goto IL_3CD;
						case 'ś':
							i = 575;
							goto IL_3CD;
						case 'Ŝ':
						case 'ŝ':
							break;
						case 'Ş':
							i = 100;
							goto IL_3CD;
						case 'ş':
							i = 101;
							goto IL_3CD;
						default:
							if (c == 'Ű')
							{
								i = 107;
								goto IL_3CD;
							}
							break;
						}
						break;
					}
				}
			}
			else if (c <= 'ў')
			{
				if (c <= 'Ї')
				{
					if (c == 'ű')
					{
						i = 108;
						goto IL_3CD;
					}
					switch (c)
					{
					case 'Ź':
						i = 587;
						goto IL_3CD;
					case 'ź':
						i = 588;
						goto IL_3CD;
					case 'Ż':
						i = 589;
						goto IL_3CD;
					case 'ż':
						i = 590;
						goto IL_3CD;
					default:
						switch (c)
						{
						case 'Ё':
							i = 512;
							goto IL_3CD;
						case 'Є':
							i = 514;
							goto IL_3CD;
						case 'І':
							i = 515;
							goto IL_3CD;
						case 'Ї':
							i = 516;
							goto IL_3CD;
						}
						break;
					}
				}
				else
				{
					if (c == 'Ў')
					{
						i = 517;
						goto IL_3CD;
					}
					switch (c)
					{
					case 'ё':
						i = 560;
						goto IL_3CD;
					case 'ђ':
					case 'ѓ':
					case 'ѕ':
						break;
					case 'є':
						i = 562;
						goto IL_3CD;
					case 'і':
						i = 563;
						goto IL_3CD;
					case 'ї':
						i = 564;
						goto IL_3CD;
					default:
						if (c == 'ў')
						{
							i = 565;
							goto IL_3CD;
						}
						break;
					}
				}
			}
			else if (c <= '–')
			{
				if (c == 'Ґ')
				{
					i = 513;
					goto IL_3CD;
				}
				if (c == 'ґ')
				{
					i = 561;
					goto IL_3CD;
				}
				if (c == '–')
				{
					i = 464;
					goto IL_3CD;
				}
			}
			else
			{
				if (c == '—')
				{
					i = 465;
					goto IL_3CD;
				}
				if (c == '’')
				{
					i = 104;
					goto IL_3CD;
				}
				if (c == '№')
				{
					i = 466;
					goto IL_3CD;
				}
			}
			if (i >= 1008 && i < 1040)
			{
				i -= 528;
			}
			else if (i >= 1040 && i < 1072)
			{
				i -= 512;
			}
			IL_3CD:
			return new Rectangle(i * 8 % SpriteText.spriteTexture.Width, i * 8 / SpriteText.spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
		}

		// Token: 0x04002590 RID: 9616
		public const int scrollStyle_scroll = 0;

		// Token: 0x04002591 RID: 9617
		public const int scrollStyle_speechBubble = 1;

		// Token: 0x04002592 RID: 9618
		public const int scrollStyle_darkMetal = 2;

		// Token: 0x04002593 RID: 9619
		public const int scrollStyle_blueMetal = 3;

		// Token: 0x04002594 RID: 9620
		public const int maxCharacter = 999999;

		// Token: 0x04002595 RID: 9621
		public const int maxHeight = 999999;

		// Token: 0x04002596 RID: 9622
		public const int characterWidth = 8;

		// Token: 0x04002597 RID: 9623
		public const int characterHeight = 16;

		// Token: 0x04002598 RID: 9624
		public const int horizontalSpaceBetweenCharacters = 0;

		// Token: 0x04002599 RID: 9625
		public const int verticalSpaceBetweenCharacters = 2;

		// Token: 0x0400259A RID: 9626
		public const char newLine = '^';

		// Token: 0x0400259B RID: 9627
		public static float fontPixelZoom = 3f;

		// Token: 0x0400259C RID: 9628
		public static float shadowAlpha = 0.15f;

		// Token: 0x0400259D RID: 9629
		public static Dictionary<char, FontChar> characterMap;

		// Token: 0x0400259E RID: 9630
		public static FontFile FontFile = null;

		// Token: 0x0400259F RID: 9631
		public static List<Texture2D> fontPages = null;

		// Token: 0x040025A0 RID: 9632
		public static Texture2D spriteTexture;

		// Token: 0x040025A1 RID: 9633
		public static Texture2D coloredTexture;

		// Token: 0x040025A2 RID: 9634
		public const int color_index_Default = -1;

		// Token: 0x040025A3 RID: 9635
		public const int color_index_Black = 0;

		// Token: 0x040025A4 RID: 9636
		public const int color_index_Blue = 1;

		// Token: 0x040025A5 RID: 9637
		public const int color_index_Red = 2;

		// Token: 0x040025A6 RID: 9638
		public const int color_index_Purple = 3;

		// Token: 0x040025A7 RID: 9639
		public const int color_index_White = 4;

		// Token: 0x040025A8 RID: 9640
		public const int color_index_Orange = 5;

		// Token: 0x040025A9 RID: 9641
		public const int color_index_Green = 6;

		// Token: 0x040025AA RID: 9642
		public const int color_index_Cyan = 7;

		// Token: 0x040025AB RID: 9643
		public const int color_index_Gray = 8;

		// Token: 0x040025AC RID: 9644
		public const int color_index_JojaBlue = 9;

		// Token: 0x040025B7 RID: 9655
		public static bool forceEnglishFont = false;

		// Token: 0x020006C2 RID: 1730
		public enum ScrollTextAlignment
		{
			// Token: 0x040030B0 RID: 12464
			Left,
			// Token: 0x040030B1 RID: 12465
			Center,
			// Token: 0x040030B2 RID: 12466
			Right
		}
	}
}

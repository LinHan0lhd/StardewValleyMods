using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000258 RID: 600
	public class ChatMessage
	{
		// Token: 0x060027E5 RID: 10213 RVA: 0x001D0374 File Offset: 0x001CE574
		public void parseMessageForEmoji(string messagePlaintext)
		{
			if (messagePlaintext != null)
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < messagePlaintext.Length; i++)
				{
					if (messagePlaintext[i] == '[')
					{
						if (sb.Length > 0)
						{
							this.breakNewLines(sb);
						}
						sb.Clear();
						int tag_close_index = messagePlaintext.IndexOf(']', i);
						int next_open_index = -1;
						if (i + 1 < messagePlaintext.Length)
						{
							next_open_index = messagePlaintext.IndexOf('[', i + 1);
						}
						if (tag_close_index != -1 && (next_open_index == -1 || next_open_index > tag_close_index))
						{
							string sub = messagePlaintext.Substring(i + 1, tag_close_index - i - 1);
							int emojiIndex;
							if (int.TryParse(sub, out emojiIndex))
							{
								if (emojiIndex < EmojiMenu.totalEmojis)
								{
									this.message.Add(new ChatSnippet(emojiIndex));
								}
							}
							else
							{
								if (sub != null)
								{
									switch (sub.Length)
									{
									case 3:
										if (!(sub == "red"))
										{
											goto IL_314;
										}
										break;
									case 4:
									{
										char c = sub[0];
										if (c <= 'b')
										{
											if (c != 'a')
											{
												if (c != 'b')
												{
													goto IL_314;
												}
												if (!(sub == "blue"))
												{
													goto IL_314;
												}
											}
											else if (!(sub == "aqua"))
											{
												goto IL_314;
											}
										}
										else if (c != 'g')
										{
											if (c != 'j')
											{
												if (c != 'p')
												{
													goto IL_314;
												}
												if (!(sub == "pink") && !(sub == "plum"))
												{
													goto IL_314;
												}
											}
											else if (!(sub == "jade"))
											{
												goto IL_314;
											}
										}
										else if (!(sub == "gray"))
										{
											goto IL_314;
										}
										break;
									}
									case 5:
									{
										char c = sub[0];
										if (c <= 'c')
										{
											if (c != 'b')
											{
												if (c != 'c')
												{
													goto IL_314;
												}
												if (!(sub == "cream"))
												{
													goto IL_314;
												}
											}
											else if (!(sub == "brown"))
											{
												goto IL_314;
											}
										}
										else if (c != 'g')
										{
											if (c != 'p')
											{
												goto IL_314;
											}
											if (!(sub == "peach"))
											{
												goto IL_314;
											}
										}
										else if (!(sub == "green"))
										{
											goto IL_314;
										}
										break;
									}
									case 6:
									{
										char c = sub[0];
										if (c != 'j')
										{
											switch (c)
											{
											case 'o':
												if (!(sub == "orange"))
												{
													goto IL_314;
												}
												break;
											case 'p':
												if (!(sub == "purple"))
												{
													goto IL_314;
												}
												break;
											case 'q':
											case 'r':
												goto IL_314;
											case 's':
												if (!(sub == "salmon"))
												{
													goto IL_314;
												}
												break;
											default:
												if (c != 'y')
												{
													goto IL_314;
												}
												if (!(sub == "yellow"))
												{
													goto IL_314;
												}
												break;
											}
										}
										else if (!(sub == "jungle"))
										{
											goto IL_314;
										}
										break;
									}
									case 7:
									case 8:
									case 9:
									case 10:
										goto IL_314;
									case 11:
										if (!(sub == "yellowgreen"))
										{
											goto IL_314;
										}
										break;
									default:
										goto IL_314;
									}
									if (this.color.Equals(Color.White))
									{
										this.color = ChatMessage.getColorFromName(sub);
										goto IL_335;
									}
									goto IL_335;
								}
								IL_314:
								sb.Append("[");
								sb.Append(sub);
								sb.Append("]");
							}
							IL_335:
							i = tag_close_index;
						}
						else
						{
							sb.Append("[");
						}
					}
					else
					{
						sb.Append(messagePlaintext[i]);
					}
				}
				if (sb.Length > 0)
				{
					this.breakNewLines(sb);
				}
			}
		}

		// Token: 0x060027E6 RID: 10214 RVA: 0x001D06F8 File Offset: 0x001CE8F8
		public static Color getColorFromName(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 3:
					if (name == "red")
					{
						return new Color(220, 20, 20);
					}
					break;
				case 4:
				{
					char c = name[0];
					if (c <= 'b')
					{
						if (c != 'a')
						{
							if (c == 'b')
							{
								if (name == "blue")
								{
									return Color.DodgerBlue;
								}
							}
						}
						else if (name == "aqua")
						{
							return Color.MediumTurquoise;
						}
					}
					else if (c != 'g')
					{
						if (c != 'j')
						{
							if (c == 'p')
							{
								if (name == "pink")
								{
									return Color.HotPink;
								}
								if (name == "plum")
								{
									return new Color(190, 0, 190);
								}
							}
						}
						else if (name == "jade")
						{
							return new Color(50, 230, 150);
						}
					}
					else if (name == "gray")
					{
						return Color.Gray;
					}
					break;
				}
				case 5:
				{
					char c = name[0];
					if (c <= 'c')
					{
						if (c != 'b')
						{
							if (c == 'c')
							{
								if (name == "cream")
								{
									return new Color(255, 255, 180);
								}
							}
						}
						else if (name == "brown")
						{
							return new Color(160, 80, 30);
						}
					}
					else if (c != 'g')
					{
						if (c == 'p')
						{
							if (name == "peach")
							{
								return new Color(255, 180, 120);
							}
						}
					}
					else if (name == "green")
					{
						return new Color(0, 180, 10);
					}
					break;
				}
				case 6:
				{
					char c = name[0];
					if (c != 'j')
					{
						switch (c)
						{
						case 'o':
							if (name == "orange")
							{
								return new Color(255, 100, 0);
							}
							break;
						case 'p':
							if (name == "purple")
							{
								return new Color(138, 43, 250);
							}
							break;
						case 'q':
						case 'r':
							break;
						case 's':
							if (name == "salmon")
							{
								return Color.Salmon;
							}
							break;
						default:
							if (c == 'y')
							{
								if (name == "yellow")
								{
									return new Color(240, 200, 0);
								}
							}
							break;
						}
					}
					else if (name == "jungle")
					{
						return Color.SeaGreen;
					}
					break;
				}
				case 11:
					if (name == "yellowgreen")
					{
						return new Color(182, 214, 0);
					}
					break;
				}
			}
			return Color.White;
		}

		// Token: 0x060027E7 RID: 10215 RVA: 0x001D0A30 File Offset: 0x001CEC30
		private void breakNewLines(StringBuilder sb)
		{
			string[] split = sb.ToString().Split(Environment.NewLine, StringSplitOptions.None);
			for (int i = 0; i < split.Length; i++)
			{
				this.message.Add(new ChatSnippet(split[i], this.language));
				if (i != split.Length - 1)
				{
					this.message.Add(new ChatSnippet(Environment.NewLine, this.language));
				}
			}
		}

		// Token: 0x060027E8 RID: 10216 RVA: 0x001D0A9C File Offset: 0x001CEC9C
		public static string makeMessagePlaintext(List<ChatSnippet> message, bool include_color_information)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ChatSnippet cs in message)
			{
				if (cs.message != null)
				{
					sb.Append(cs.message);
				}
				else if (cs.emojiIndex != -1)
				{
					sb.Append("[" + cs.emojiIndex.ToString() + "]");
				}
			}
			if (include_color_information && Game1.player.defaultChatColor != null && !ChatMessage.getColorFromName(Game1.player.defaultChatColor).Equals(Color.White))
			{
				sb.Append(" [");
				sb.Append(Game1.player.defaultChatColor);
				sb.Append("]");
			}
			return sb.ToString();
		}

		// Token: 0x060027E9 RID: 10217 RVA: 0x001D0B88 File Offset: 0x001CED88
		public void draw(SpriteBatch b, int x, int y)
		{
			float xPositionSoFar = 0f;
			float yPositionSoFar = 0f;
			for (int i = 0; i < this.message.Count; i++)
			{
				if (this.message[i].emojiIndex != -1)
				{
					b.Draw(ChatBox.emojiTexture, new Vector2((float)x + xPositionSoFar + 1f, (float)y + yPositionSoFar - 4f), new Rectangle?(new Rectangle(this.message[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, this.message[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9)), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
				else if (this.message[i].message != null)
				{
					if (this.message[i].message.Equals(Environment.NewLine))
					{
						xPositionSoFar = 0f;
						yPositionSoFar += ChatBox.messageFont(this.language).MeasureString("(").Y;
					}
					else
					{
						b.DrawString(ChatBox.messageFont(this.language), this.message[i].message, new Vector2((float)x + xPositionSoFar, (float)y + yPositionSoFar), this.color * this.alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
					}
				}
				xPositionSoFar += this.message[i].myLength;
				if (xPositionSoFar >= 888f)
				{
					xPositionSoFar = 0f;
					yPositionSoFar += ChatBox.messageFont(this.language).MeasureString("(").Y;
					if (this.message.Count > i + 1 && this.message[i + 1].message != null && this.message[i + 1].message.Equals(Environment.NewLine))
					{
						i++;
					}
				}
			}
		}

		// Token: 0x04001999 RID: 6553
		public List<ChatSnippet> message = new List<ChatSnippet>();

		// Token: 0x0400199A RID: 6554
		public int timeLeftToDisplay;

		// Token: 0x0400199B RID: 6555
		public int verticalSize;

		// Token: 0x0400199C RID: 6556
		public float alpha = 1f;

		// Token: 0x0400199D RID: 6557
		public Color color;

		// Token: 0x0400199E RID: 6558
		public LocalizedContentManager.LanguageCode language;
	}
}

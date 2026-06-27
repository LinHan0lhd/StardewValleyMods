using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StardewValley.Util
{
	// Token: 0x02000121 RID: 289
	internal class ProfanityFilter
	{
		// Token: 0x060017C6 RID: 6086 RVA: 0x00112188 File Offset: 0x00110388
		public ProfanityFilter() : this("Content/profanity.regex")
		{
		}

		// Token: 0x060017C7 RID: 6087 RVA: 0x00112198 File Offset: 0x00110398
		public ProfanityFilter(string profanityFile)
		{
			this._cleanup = new StringBuilder(2048);
			string[] profanity = File.ReadAllLines(profanityFile);
			this._words = new List<Regex>(profanity.Length);
			for (int i = 0; i < profanity.Length; i++)
			{
				Regex expr = new Regex(profanity[i], RegexOptions.IgnoreCase | RegexOptions.Compiled);
				this._words.Add(expr);
			}
		}

		// Token: 0x060017C8 RID: 6088 RVA: 0x001121F8 File Offset: 0x001103F8
		public string Filter(string words)
		{
			if (string.IsNullOrWhiteSpace(words))
			{
				return words;
			}
			for (int i = 0; i < this._words.Count; i++)
			{
				MatchCollection matches = this._words[i].Matches(words);
				if (matches.Count != 0)
				{
					this._cleanup.Clear();
					this._cleanup.Append(words);
					for (int j = 0; j < matches.Count; j++)
					{
						Match match = matches[j];
						int end = match.Index + match.Length;
						for (int p = match.Index; p < end; p++)
						{
							if (!char.IsWhiteSpace(this._cleanup[p]))
							{
								this._cleanup[p] = '*';
							}
						}
					}
					words = this._cleanup.ToString();
				}
			}
			return words;
		}

		// Token: 0x04000E50 RID: 3664
		private readonly List<Regex> _words;

		// Token: 0x04000E51 RID: 3665
		private readonly StringBuilder _cleanup;
	}
}

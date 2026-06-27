using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StardewValley.Tests
{
	// Token: 0x0200013B RID: 315
	public class TranslationValidator
	{
		// Token: 0x0600191B RID: 6427 RVA: 0x00126FB8 File Offset: 0x001251B8
		public IEnumerable<TranslationValidatorResult> Compare<TValue>(Dictionary<string, TValue> baseData, Dictionary<string, TValue> translatedData, Func<TValue, string> getText, string baseAssetName)
		{
			return this.Compare<TValue>(baseData, translatedData, getText, (string key, string text) => this.Abstractor.ExtractSyntaxFor(baseAssetName, key, text));
		}

		// Token: 0x0600191C RID: 6428 RVA: 0x00126FEF File Offset: 0x001251EF
		public IEnumerable<TranslationValidatorResult> Compare<TValue>(Dictionary<string, TValue> baseData, Dictionary<string, TValue> translatedData, Func<TValue, string> getText, Func<string, string, string> getSyntax)
		{
			foreach (KeyValuePair<string, TValue> basePair in baseData)
			{
				string key = basePair.Key;
				string baseText = getText(basePair.Value);
				TValue translationEntry;
				if (!translatedData.TryGetValue(key, out translationEntry))
				{
					yield return new TranslationValidatorResult(TranslationValidatorIssue.MissingKey, key, getSyntax(key, baseText), baseText, null, null, "Key not found in the translated asset.");
				}
				else
				{
					string translationText = getText(translationEntry);
					TranslationValidatorResult syntaxResult = this.CompareEntry(key, baseText, translationText, (string value) => getSyntax(key, value));
					if (syntaxResult != null)
					{
						yield return syntaxResult;
					}
				}
			}
			Dictionary<string, TValue>.Enumerator enumerator = default(Dictionary<string, TValue>.Enumerator);
			foreach (KeyValuePair<string, TValue> translatedPair in translatedData)
			{
				string key2 = translatedPair.Key;
				if (!baseData.ContainsKey(key2))
				{
					string translationText2 = getText(translatedPair.Value);
					string translationSyntax = getSyntax(key2, translationText2);
					yield return new TranslationValidatorResult(TranslationValidatorIssue.UnknownKey, key2, null, null, translationSyntax, translationText2, "Unknown key in translation which isn't in the base asset.");
				}
			}
			enumerator = default(Dictionary<string, TValue>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x0600191D RID: 6429 RVA: 0x0012701C File Offset: 0x0012521C
		public TranslationValidatorResult CompareEntry(string key, string baseText, string translationText, Func<string, string> getSyntax)
		{
			string baseSyntax = getSyntax(baseText);
			string translationSyntax = getSyntax(translationText);
			if (baseSyntax != translationSyntax)
			{
				TranslationValidatorIssue issue = TranslationValidatorIssue.SyntaxMismatch;
				string baseSyntax2 = baseSyntax;
				string translationSyntax2 = translationSyntax;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(148, 6);
				defaultInterpolatedStringHandler.AppendLiteral("The translation has a different syntax than the base text.\n");
				defaultInterpolatedStringHandler.AppendLiteral("Syntax:\n");
				defaultInterpolatedStringHandler.AppendLiteral("    base:  ");
				defaultInterpolatedStringHandler.AppendFormatted(baseSyntax);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("    local: ");
				defaultInterpolatedStringHandler.AppendFormatted(translationSyntax);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("           ");
				defaultInterpolatedStringHandler.AppendFormatted("".PadRight(this.GetDiffIndex(baseSyntax, translationSyntax), ' '));
				defaultInterpolatedStringHandler.AppendLiteral("^\n");
				defaultInterpolatedStringHandler.AppendLiteral("Text:\n");
				defaultInterpolatedStringHandler.AppendLiteral("    base:  ");
				defaultInterpolatedStringHandler.AppendFormatted(baseText);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("    local: ");
				defaultInterpolatedStringHandler.AppendFormatted(translationText);
				defaultInterpolatedStringHandler.AppendLiteral("\n\n");
				defaultInterpolatedStringHandler.AppendLiteral("           ");
				defaultInterpolatedStringHandler.AppendFormatted("".PadRight(this.GetDiffIndex(baseText, translationText), ' '));
				defaultInterpolatedStringHandler.AppendLiteral("^\n");
				return new TranslationValidatorResult(issue, key, baseSyntax2, baseText, translationSyntax2, translationText, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			string error;
			string errorBlock;
			if (!this.ValidateGenderSwitchBlocks(baseText, out error, out errorBlock))
			{
				TranslationValidatorIssue issue2 = TranslationValidatorIssue.MalformedSyntax;
				string baseSyntax3 = baseSyntax;
				string translationSyntax3 = translationSyntax;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Base text has invalid gender switch block: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".\nAffected block: ");
				defaultInterpolatedStringHandler.AppendFormatted(errorBlock);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				return new TranslationValidatorResult(issue2, key, baseSyntax3, baseText, translationSyntax3, translationText, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (!this.ValidateGenderSwitchBlocks(baseText, out error, out errorBlock))
			{
				TranslationValidatorIssue issue3 = TranslationValidatorIssue.MalformedSyntax;
				string baseSyntax4 = baseSyntax;
				string translationSyntax4 = translationSyntax;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Translated text has invalid gender switch block: ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".\nAffected block: ");
				defaultInterpolatedStringHandler.AppendFormatted(errorBlock);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				return new TranslationValidatorResult(issue3, key, baseSyntax4, baseText, translationSyntax4, translationText, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return null;
		}

		// Token: 0x0600191E RID: 6430 RVA: 0x00127230 File Offset: 0x00125430
		public bool ValidateGenderSwitchBlocks(string text, out string error, out string errorBlock)
		{
			int minIndex = 0;
			int start;
			char splitCharacter;
			string[] branches;
			string firstSyntax;
			int i;
			string curSyntax;
			for (;;)
			{
				start = text.IndexOf("${", minIndex, StringComparison.OrdinalIgnoreCase);
				if (start == -1)
				{
					goto IL_212;
				}
				int end = text.IndexOf("}$", start, StringComparison.OrdinalIgnoreCase);
				if (end == -1)
				{
					break;
				}
				errorBlock = text.Substring(start, end - start);
				string text2 = text.Substring(start + 2, end - start - 2);
				splitCharacter = (text2.Contains('^') ? '^' : '¦');
				branches = text2.Split(splitCharacter, StringSplitOptions.None);
				if (text2.Contains("${"))
				{
					goto Block_4;
				}
				if (branches.Length < 2)
				{
					goto Block_5;
				}
				if (branches.Length > 3)
				{
					goto Block_6;
				}
				firstSyntax = this.Abstractor.ExtractDialogueSyntax(branches[0]);
				for (i = 1; i < branches.Length; i++)
				{
					curSyntax = this.Abstractor.ExtractDialogueSyntax(branches[1]);
					if (firstSyntax != curSyntax)
					{
						goto Block_7;
					}
				}
				minIndex = end + 2;
			}
			error = "closing '}$' not found";
			errorBlock = text.Substring(start);
			return false;
			Block_4:
			error = "can't start a new gender-switch block inside another";
			return false;
			Block_5:
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
			defaultInterpolatedStringHandler.AppendLiteral("must have at least two branches delimited by ");
			defaultInterpolatedStringHandler.AppendFormatted<char>('^');
			defaultInterpolatedStringHandler.AppendLiteral(" or ");
			defaultInterpolatedStringHandler.AppendFormatted<char>('¦');
			error = defaultInterpolatedStringHandler.ToStringAndClear();
			return false;
			Block_6:
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 5);
			defaultInterpolatedStringHandler.AppendLiteral("found ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(branches.Length);
			defaultInterpolatedStringHandler.AppendLiteral(" branches delimited by ");
			defaultInterpolatedStringHandler.AppendFormatted<char>(splitCharacter);
			defaultInterpolatedStringHandler.AppendLiteral(", must be two (male");
			defaultInterpolatedStringHandler.AppendFormatted<char>(splitCharacter);
			defaultInterpolatedStringHandler.AppendLiteral("female) or three (male");
			defaultInterpolatedStringHandler.AppendFormatted<char>(splitCharacter);
			defaultInterpolatedStringHandler.AppendLiteral("female");
			defaultInterpolatedStringHandler.AppendFormatted<char>(splitCharacter);
			defaultInterpolatedStringHandler.AppendLiteral("other)");
			error = defaultInterpolatedStringHandler.ToStringAndClear();
			return false;
			Block_7:
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 3);
			defaultInterpolatedStringHandler.AppendLiteral("branches have different syntax (0: `");
			defaultInterpolatedStringHandler.AppendFormatted(firstSyntax);
			defaultInterpolatedStringHandler.AppendLiteral("`, ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(i);
			defaultInterpolatedStringHandler.AppendLiteral(": `");
			defaultInterpolatedStringHandler.AppendFormatted(curSyntax);
			defaultInterpolatedStringHandler.AppendLiteral("`)");
			error = defaultInterpolatedStringHandler.ToStringAndClear();
			return false;
			IL_212:
			error = null;
			errorBlock = null;
			return true;
		}

		// Token: 0x0600191F RID: 6431 RVA: 0x00127458 File Offset: 0x00125658
		public int GetDiffIndex(string baseText, string translatedText)
		{
			int minLength = Math.Min(baseText.Length, translatedText.Length);
			for (int i = 0; i < minLength; i++)
			{
				if (baseText[i] != translatedText[i])
				{
					return i;
				}
			}
			return minLength;
		}

		// Token: 0x04000F09 RID: 3849
		private readonly SyntaxAbstractor Abstractor = new SyntaxAbstractor();
	}
}

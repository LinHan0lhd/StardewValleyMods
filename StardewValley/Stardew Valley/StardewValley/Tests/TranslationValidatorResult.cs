using System;

namespace StardewValley.Tests
{
	// Token: 0x0200013D RID: 317
	public class TranslationValidatorResult
	{
		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06001921 RID: 6433 RVA: 0x001274A9 File Offset: 0x001256A9
		public TranslationValidatorIssue Issue { get; }

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06001922 RID: 6434 RVA: 0x001274B1 File Offset: 0x001256B1
		public string Key { get; }

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06001923 RID: 6435 RVA: 0x001274B9 File Offset: 0x001256B9
		public string BaseSyntax { get; }

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06001924 RID: 6436 RVA: 0x001274C1 File Offset: 0x001256C1
		public string BaseText { get; }

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06001925 RID: 6437 RVA: 0x001274C9 File Offset: 0x001256C9
		public string TranslationSyntax { get; }

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06001926 RID: 6438 RVA: 0x001274D1 File Offset: 0x001256D1
		public string TranslationText { get; }

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06001927 RID: 6439 RVA: 0x001274D9 File Offset: 0x001256D9
		public string SuggestedError { get; }

		// Token: 0x06001928 RID: 6440 RVA: 0x001274E1 File Offset: 0x001256E1
		public TranslationValidatorResult(TranslationValidatorIssue issue, string key, string baseSyntax, string baseText, string translationSyntax, string translationText, string suggestedError)
		{
			this.Issue = issue;
			this.Key = key;
			this.BaseSyntax = baseSyntax;
			this.BaseText = baseText;
			this.TranslationSyntax = translationSyntax;
			this.TranslationText = translationText;
			this.SuggestedError = suggestedError;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ContentManifest;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData;
using StardewValley.Logging;

namespace StardewValley
{
	// Token: 0x020000C5 RID: 197
	public class LocalizedContentManager : ContentManager
	{
		// Token: 0x14000015 RID: 21
		// (add) Token: 0x06000D94 RID: 3476 RVA: 0x000930DC File Offset: 0x000912DC
		// (remove) Token: 0x06000D95 RID: 3477 RVA: 0x00093110 File Offset: 0x00091310
		public static event LocalizedContentManager.LanguageChangedHandler OnLanguageChange;

		// Token: 0x06000D96 RID: 3478 RVA: 0x00093144 File Offset: 0x00091344
		private void PlatformEnsureManifestInitialized()
		{
			if (LocalizedContentManager._manifest == null)
			{
				LocalizedContentManager._manifest = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				string manifestPath = Path.Combine(this.GetContentRoot(), "ContentHashes.json");
				if (File.Exists(manifestPath))
				{
					Dictionary<string, object> contentHashes = null;
					try
					{
						contentHashes = ContentHashParser.ParseFromFile(manifestPath);
					}
					catch (Exception ex)
					{
						Game1.log.Error("Error parsing ContentHashes.json:", ex);
					}
					if (contentHashes == null || contentHashes.Count == 0)
					{
						Game1.log.Warn("Parsing ContentHashes.json resulted in a null or empty dictionary.");
						return;
					}
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Successfully loaded ContentHashes.json containing ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(contentHashes.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" file(s);");
					log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
					if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					{
						using (Dictionary<string, object>.KeyCollection.Enumerator enumerator = contentHashes.Keys.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								string assetNameKey = enumerator.Current;
								LocalizedContentManager._manifest.Add(assetNameKey.Replace('/', '\\'));
							}
							return;
						}
					}
					LocalizedContentManager._manifest.UnionWith(contentHashes.Keys);
					return;
				}
				else
				{
					Game1.log.Warn("Could not find ContentHashes at path '" + manifestPath + "'");
				}
			}
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x0009329C File Offset: 0x0009149C
		private void EnsureManifestInitialized()
		{
			if (LocalizedContentManager._manifest != null)
			{
				return;
			}
			object manifestLocker = LocalizedContentManager.ManifestLocker;
			lock (manifestLocker)
			{
				if (LocalizedContentManager._manifest == null)
				{
					Stopwatch stopwatch = Stopwatch.StartNew();
					this.PlatformEnsureManifestInitialized();
					stopwatch.Stop();
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 1);
					defaultInterpolatedStringHandler.AppendLiteral("EnsureManifestInitialized() finished, elapsed = '");
					defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(stopwatch.Elapsed);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x00093338 File Offset: 0x00091538
		public static LocalizedContentManager.LanguageCode GetDefaultLanguageCode()
		{
			return LocalizedContentManager.LanguageCode.en;
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000D99 RID: 3481 RVA: 0x0009333B File Offset: 0x0009153B
		public static string CurrentLanguageString
		{
			get
			{
				if (LocalizedContentManager._currentLangString == null)
				{
					LocalizedContentManager._currentLangString = LocalizedContentManager.LanguageCodeString(LocalizedContentManager.CurrentLanguageCode);
				}
				return LocalizedContentManager._currentLangString;
			}
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06000D9A RID: 3482 RVA: 0x00093358 File Offset: 0x00091558
		// (set) Token: 0x06000D9B RID: 3483 RVA: 0x00093360 File Offset: 0x00091560
		public static LocalizedContentManager.LanguageCode CurrentLanguageCode
		{
			get
			{
				return LocalizedContentManager._currentLangCode;
			}
			set
			{
				if (LocalizedContentManager._currentLangCode == value)
				{
					return;
				}
				LocalizedContentManager.LanguageCode prev = LocalizedContentManager._currentLangCode;
				LocalizedContentManager._currentLangCode = value;
				LocalizedContentManager._currentLangString = null;
				if (LocalizedContentManager._currentLangCode != LocalizedContentManager.LanguageCode.mod)
				{
					LocalizedContentManager._currentModLanguage = null;
				}
				Game1.log.Verbose(string.Concat(new string[]
				{
					"LocalizedContentManager.CurrentLanguageCode CHANGING from '",
					prev.ToString(),
					"' to '",
					LocalizedContentManager._currentLangCode.ToString(),
					"'"
				}));
				LocalizedContentManager.LanguageChangedHandler onLanguageChange = LocalizedContentManager.OnLanguageChange;
				if (onLanguageChange != null)
				{
					onLanguageChange(LocalizedContentManager._currentLangCode);
				}
				Game1.log.Verbose(string.Concat(new string[]
				{
					"LocalizedContentManager.CurrentLanguageCode CHANGED from '",
					prev.ToString(),
					"' to '",
					LocalizedContentManager._currentLangCode.ToString(),
					"'"
				}));
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000D9C RID: 3484 RVA: 0x0009344C File Offset: 0x0009164C
		public static bool CurrentLanguageLatin
		{
			get
			{
				return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager._currentModLanguage.UseLatinFont);
			}
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x000934B2 File Offset: 0x000916B2
		public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture) : base(serviceProvider, rootDirectory)
		{
			this.CurrentCulture = currentCulture;
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x000934C3 File Offset: 0x000916C3
		public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory) : this(serviceProvider, rootDirectory, Thread.CurrentThread.CurrentUICulture)
		{
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000D9F RID: 3487 RVA: 0x000934D7 File Offset: 0x000916D7
		public static ModLanguage CurrentModLanguage
		{
			get
			{
				return LocalizedContentManager._currentModLanguage;
			}
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x000934E0 File Offset: 0x000916E0
		protected static bool _IsStringAt(string source, string string_to_find, int index)
		{
			for (int i = 0; i < string_to_find.Length; i++)
			{
				int source_index = index + i;
				if (source_index >= source.Length)
				{
					return false;
				}
				if (source[source_index] != string_to_find[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x00093520 File Offset: 0x00091720
		public static StringBuilder FormatTimeString(int time, string format)
		{
			LocalizedContentManager._timeFormatStringBuilder.Clear();
			int brace_start_index = -1;
			for (int i = 0; i < format.Length; i++)
			{
				char character = format[i];
				if (character == '[')
				{
					if (brace_start_index < 0)
					{
						brace_start_index = i;
					}
					else
					{
						for (int j = brace_start_index; j <= i; j++)
						{
							LocalizedContentManager._timeFormatStringBuilder.Append(format[j]);
						}
						brace_start_index = i;
					}
				}
				else if (character == ']' && brace_start_index >= 0)
				{
					if (LocalizedContentManager._IsStringAt(format, "[HOURS_12]", brace_start_index))
					{
						LocalizedContentManager._timeFormatStringBuilder.Append((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString());
					}
					else if (LocalizedContentManager._IsStringAt(format, "[HOURS_12_0]", brace_start_index))
					{
						LocalizedContentManager._timeFormatStringBuilder.Append((time / 100 % 12 == 0) ? "0" : (time / 100 % 12).ToString());
					}
					else if (LocalizedContentManager._IsStringAt(format, "[HOURS_24]", brace_start_index))
					{
						LocalizedContentManager._timeFormatStringBuilder.Append(time / 100 % 24);
					}
					else if (LocalizedContentManager._IsStringAt(format, "[HOURS_24_00]", brace_start_index))
					{
						LocalizedContentManager._timeFormatStringBuilder.Append((time / 100 % 24).ToString("00"));
					}
					else if (LocalizedContentManager._IsStringAt(format, "[MINUTES]", brace_start_index))
					{
						LocalizedContentManager._timeFormatStringBuilder.Append((time % 100).ToString("00"));
					}
					else if (LocalizedContentManager._IsStringAt(format, "[AM_PM]", brace_start_index))
					{
						if (time < 1200 || time >= 2400)
						{
							LocalizedContentManager._timeFormatStringBuilder.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370"));
						}
						else
						{
							LocalizedContentManager._timeFormatStringBuilder.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
						}
					}
					else
					{
						for (int k = brace_start_index; k <= i; k++)
						{
							LocalizedContentManager._timeFormatStringBuilder.Append(format[k]);
						}
					}
					brace_start_index = -1;
				}
				else if (brace_start_index < 0)
				{
					LocalizedContentManager._timeFormatStringBuilder.Append(character);
				}
			}
			return LocalizedContentManager._timeFormatStringBuilder;
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x00093730 File Offset: 0x00091930
		public static void SetModLanguage(ModLanguage new_mod_language)
		{
			if (new_mod_language == LocalizedContentManager._currentModLanguage)
			{
				return;
			}
			LocalizedContentManager._currentModLanguage = new_mod_language;
			LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.mod;
		}

		// Token: 0x06000DA3 RID: 3491 RVA: 0x00093748 File Offset: 0x00091948
		public virtual string GetContentRoot()
		{
			if (this._CachedContentRoot == null)
			{
				PropertyInfo property = typeof(TitleContainer).GetProperty("Location", BindingFlags.Static | BindingFlags.NonPublic);
				if (property == null)
				{
					throw new InvalidOperationException("Can't get TitleContainer.Location property from MonoGame");
				}
				string text = (string)property.GetValue(null, null);
				if (text == null)
				{
					throw new InvalidOperationException("Can't get value of TitleContainer.Location property from MonoGame");
				}
				string basePath = text;
				this._CachedContentRoot = Path.Combine(basePath, base.RootDirectory);
			}
			return this._CachedContentRoot;
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x000937B8 File Offset: 0x000919B8
		public virtual bool DoesAssetExist<T>(string assetName)
		{
			if (assetName == null)
			{
				return false;
			}
			bool lastCharWasSlash = false;
			char assetPathSeparator = (Environment.OSVersion.Platform == PlatformID.Win32NT) ? '\\' : '/';
			StringBuilder sb = new StringBuilder(assetName.Length + 4);
			int ci = 0;
			while (ci < assetName.Length)
			{
				char c = assetName[ci];
				if (c != '/' && c != '\\')
				{
					lastCharWasSlash = false;
					goto IL_50;
				}
				if (!lastCharWasSlash)
				{
					c = assetPathSeparator;
					lastCharWasSlash = true;
					goto IL_50;
				}
				IL_59:
				ci++;
				continue;
				IL_50:
				sb.Append(c);
				goto IL_59;
			}
			sb.Append(".xnb");
			string xnbAssetPath = sb.ToString();
			this.EnsureManifestInitialized();
			return LocalizedContentManager._manifest.Contains(xnbAssetPath);
		}

		// Token: 0x06000DA5 RID: 3493 RVA: 0x00093856 File Offset: 0x00091A56
		public virtual T LoadImpl<T>(string baseAssetName, string localizedAssetName, LocalizedContentManager.LanguageCode languageCode)
		{
			if (!this.DoesAssetExist<T>(localizedAssetName))
			{
				throw new ContentLoadException("Could not load " + localizedAssetName + " asset!");
			}
			return base.Load<T>(localizedAssetName);
		}

		// Token: 0x06000DA6 RID: 3494 RVA: 0x0009387E File Offset: 0x00091A7E
		public override T Load<T>(string assetName)
		{
			return this.Load<T>(assetName, LocalizedContentManager.CurrentLanguageCode);
		}

		// Token: 0x06000DA7 RID: 3495 RVA: 0x0009388C File Offset: 0x00091A8C
		public virtual T Load<T>(string assetName, LocalizedContentManager.LanguageCode language)
		{
			if (language != LocalizedContentManager.LanguageCode.en)
			{
				string text;
				if (!LocalizedContentManager.localizedAssetNames.TryGetValue(assetName, out text))
				{
					bool fail = false;
					string localizedAssetName = assetName + "." + ((language == LocalizedContentManager.CurrentLanguageCode) ? LocalizedContentManager.CurrentLanguageString : LocalizedContentManager.LanguageCodeString(language));
					if (!this.DoesAssetExist<T>(localizedAssetName))
					{
						fail = true;
					}
					if (!fail)
					{
						try
						{
							this.LoadImpl<T>(assetName, localizedAssetName, language);
							LocalizedContentManager.localizedAssetNames[assetName] = localizedAssetName;
						}
						catch (ContentLoadException)
						{
							fail = true;
						}
					}
					if (fail)
					{
						fail = false;
						localizedAssetName = assetName + "_international";
						if (!this.DoesAssetExist<T>(localizedAssetName))
						{
							fail = true;
						}
						if (!fail)
						{
							try
							{
								this.LoadImpl<T>(assetName, localizedAssetName, language);
								LocalizedContentManager.localizedAssetNames[assetName] = localizedAssetName;
							}
							catch (ContentLoadException)
							{
								fail = true;
							}
						}
						if (fail)
						{
							LocalizedContentManager.localizedAssetNames[assetName] = assetName;
						}
					}
				}
				return this.LoadImpl<T>(assetName, LocalizedContentManager.localizedAssetNames[assetName], language);
			}
			return this.LoadImpl<T>(assetName, assetName, LocalizedContentManager.LanguageCode.en);
		}

		// Token: 0x06000DA8 RID: 3496 RVA: 0x00093984 File Offset: 0x00091B84
		public static string LanguageCodeString(LocalizedContentManager.LanguageCode code)
		{
			switch (code)
			{
			case LocalizedContentManager.LanguageCode.ja:
				return "ja-JP";
			case LocalizedContentManager.LanguageCode.ru:
				return "ru-RU";
			case LocalizedContentManager.LanguageCode.zh:
				return "zh-CN";
			case LocalizedContentManager.LanguageCode.pt:
				return "pt-BR";
			case LocalizedContentManager.LanguageCode.es:
				return "es-ES";
			case LocalizedContentManager.LanguageCode.de:
				return "de-DE";
			case LocalizedContentManager.LanguageCode.th:
				return "th-TH";
			case LocalizedContentManager.LanguageCode.fr:
				return "fr-FR";
			case LocalizedContentManager.LanguageCode.ko:
				return "ko-KR";
			case LocalizedContentManager.LanguageCode.it:
				return "it-IT";
			case LocalizedContentManager.LanguageCode.tr:
				return "tr-TR";
			case LocalizedContentManager.LanguageCode.hu:
				return "hu-HU";
			case LocalizedContentManager.LanguageCode.mod:
			{
				ModLanguage currentModLanguage = LocalizedContentManager._currentModLanguage;
				if (currentModLanguage == null)
				{
					throw new InvalidOperationException("The game language is set to a custom one, but the language info is no longer available.");
				}
				return currentModLanguage.LanguageCode;
			}
			default:
				return "";
			}
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x00093A36 File Offset: 0x00091C36
		public LocalizedContentManager.LanguageCode GetCurrentLanguage()
		{
			return LocalizedContentManager.CurrentLanguageCode;
		}

		// Token: 0x06000DAA RID: 3498 RVA: 0x00093A40 File Offset: 0x00091C40
		private string GetString(Dictionary<string, string> strings, string key)
		{
			if (strings == null)
			{
				return null;
			}
			string result;
			if (strings.TryGetValue(key + ".desktop", out result))
			{
				return result;
			}
			if (!strings.TryGetValue(key, out result))
			{
				return null;
			}
			return result;
		}

		// Token: 0x06000DAB RID: 3499 RVA: 0x00093A78 File Offset: 0x00091C78
		public virtual bool IsValidTranslationKey(string path)
		{
			bool result;
			try
			{
				result = (this.LoadString(path) != path);
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000DAC RID: 3500 RVA: 0x00093AAC File Offset: 0x00091CAC
		public virtual string LoadStringReturnNullIfNotFound(string path, bool localeFallback = true)
		{
			string assetName;
			string key;
			this.parseStringPath(path, out assetName, out key);
			Dictionary<string, string> strings = this.Load<Dictionary<string, string>>(assetName);
			string text = this.GetString(strings, key) ?? (localeFallback ? this.LoadBaseStringOrNull(path) : null);
			return this.PreprocessString(text);
		}

		// Token: 0x06000DAD RID: 3501 RVA: 0x00093AF0 File Offset: 0x00091CF0
		public virtual string LoadStringReturnNullIfNotFound(string path, string sub1, bool localeFallback = true)
		{
			string text = this.LoadStringReturnNullIfNotFound(path, localeFallback);
			if (text != null)
			{
				text = string.Format(text, sub1);
			}
			return text;
		}

		// Token: 0x06000DAE RID: 3502 RVA: 0x00093B14 File Offset: 0x00091D14
		public virtual string LoadStringReturnNullIfNotFound(string path, string sub1, string sub2, bool localeFallback = true)
		{
			string text = this.LoadStringReturnNullIfNotFound(path, localeFallback);
			if (text != null)
			{
				text = string.Format(text, sub1, sub2);
			}
			return text;
		}

		// Token: 0x06000DAF RID: 3503 RVA: 0x00093B38 File Offset: 0x00091D38
		public virtual string LoadStringReturnNullIfNotFound(string path, object[] substitutions, bool localeFallback = true)
		{
			string text = this.LoadStringReturnNullIfNotFound(path, localeFallback);
			if (text != null)
			{
				text = string.Format(text, substitutions);
			}
			return text;
		}

		// Token: 0x06000DB0 RID: 3504 RVA: 0x00093B5A File Offset: 0x00091D5A
		public virtual string LoadString(string path)
		{
			return this.LoadStringReturnNullIfNotFound(path, true) ?? path;
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x00093B69 File Offset: 0x00091D69
		public virtual string PreprocessString(string text)
		{
			if (text == null)
			{
				return null;
			}
			Farmer player = Game1.player;
			Gender gender = (player != null) ? player.Gender : Gender.Male;
			text = Dialogue.applyGenderSwitchBlocks(gender, text);
			text = Dialogue.applyGenderSwitch(gender, text, true);
			return text;
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x00093B94 File Offset: 0x00091D94
		public virtual bool ShouldUseGenderedCharacterTranslations()
		{
			return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage != null && LocalizedContentManager.CurrentModLanguage.UseGenderedCharacterTranslations);
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x00093BBC File Offset: 0x00091DBC
		public virtual string LoadString(string path, object sub1)
		{
			string sentence = this.LoadString(path);
			try
			{
				return string.Format(sentence, sub1);
			}
			catch (Exception)
			{
			}
			return sentence;
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x00093BF4 File Offset: 0x00091DF4
		public virtual string LoadString(string path, object sub1, object sub2)
		{
			string sentence = this.LoadString(path);
			try
			{
				return string.Format(sentence, sub1, sub2);
			}
			catch (Exception)
			{
			}
			return sentence;
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x00093C2C File Offset: 0x00091E2C
		public virtual string LoadString(string path, object sub1, object sub2, object sub3)
		{
			string sentence = this.LoadString(path);
			try
			{
				return string.Format(sentence, sub1, sub2, sub3);
			}
			catch (Exception)
			{
			}
			return sentence;
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x00093C64 File Offset: 0x00091E64
		public virtual string LoadString(string path, params object[] substitutions)
		{
			string sentence = this.LoadString(path);
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(sentence, substitutions);
				}
				catch (Exception)
				{
				}
				return sentence;
			}
			return sentence;
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x00093CA0 File Offset: 0x00091EA0
		public virtual string LoadBaseStringOrNull(string path)
		{
			string assetName;
			string key;
			this.parseStringPath(path, out assetName, out key);
			Dictionary<string, string> strings = this.LoadImpl<Dictionary<string, string>>(assetName, assetName, LocalizedContentManager.LanguageCode.en);
			return this.GetString(strings, key);
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x00093CCA File Offset: 0x00091ECA
		public virtual string LoadBaseString(string path)
		{
			return this.LoadBaseStringOrNull(path) ?? path;
		}

		// Token: 0x06000DB9 RID: 3513 RVA: 0x00093CD8 File Offset: 0x00091ED8
		private void parseStringPath(string path, out string assetName, out string key)
		{
			int i = path.IndexOf(':');
			if (i == -1)
			{
				throw new ContentLoadException("Unable to parse string path: " + path);
			}
			assetName = path.Substring(0, i);
			key = path.Substring(i + 1, path.Length - i - 1);
		}

		// Token: 0x06000DBA RID: 3514 RVA: 0x00093D22 File Offset: 0x00091F22
		public virtual LocalizedContentManager CreateTemporary()
		{
			return new LocalizedContentManager(base.ServiceProvider, base.RootDirectory, this.CurrentCulture);
		}

		// Token: 0x04000920 RID: 2336
		private const bool OnlyCheckManifest = true;

		// Token: 0x04000921 RID: 2337
		private static readonly object ManifestLocker = new object();

		// Token: 0x04000922 RID: 2338
		private static HashSet<string> _manifest = null;

		// Token: 0x04000924 RID: 2340
		public static readonly Dictionary<string, string> localizedAssetNames = new Dictionary<string, string>();

		// Token: 0x04000925 RID: 2341
		protected string _CachedContentRoot;

		// Token: 0x04000926 RID: 2342
		private static LocalizedContentManager.LanguageCode _currentLangCode = LocalizedContentManager.GetDefaultLanguageCode();

		// Token: 0x04000927 RID: 2343
		private static string _currentLangString = null;

		// Token: 0x04000928 RID: 2344
		private static ModLanguage _currentModLanguage = null;

		// Token: 0x04000929 RID: 2345
		public CultureInfo CurrentCulture;

		// Token: 0x0400092A RID: 2346
		protected static StringBuilder _timeFormatStringBuilder = new StringBuilder();

		// Token: 0x0200046E RID: 1134
		// (Invoke) Token: 0x06003E2D RID: 15917
		public delegate void LanguageChangedHandler(LocalizedContentManager.LanguageCode code);

		// Token: 0x0200046F RID: 1135
		public enum LanguageCode
		{
			// Token: 0x04002838 RID: 10296
			en,
			// Token: 0x04002839 RID: 10297
			ja,
			// Token: 0x0400283A RID: 10298
			ru,
			// Token: 0x0400283B RID: 10299
			zh,
			// Token: 0x0400283C RID: 10300
			pt,
			// Token: 0x0400283D RID: 10301
			es,
			// Token: 0x0400283E RID: 10302
			de,
			// Token: 0x0400283F RID: 10303
			th,
			// Token: 0x04002840 RID: 10304
			fr,
			// Token: 0x04002841 RID: 10305
			ko,
			// Token: 0x04002842 RID: 10306
			it,
			// Token: 0x04002843 RID: 10307
			tr,
			// Token: 0x04002844 RID: 10308
			hu,
			// Token: 0x04002845 RID: 10309
			mod
		}
	}
}

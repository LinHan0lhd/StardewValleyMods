using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using StardewValley.GameData;
using StardewValley.SaveSerialization;

namespace StardewValley
{
	// Token: 0x02000106 RID: 262
	public class StartupPreferences
	{
		// Token: 0x17000247 RID: 583
		// (get) Token: 0x0600149F RID: 5279 RVA: 0x000F7A04 File Offset: 0x000F5C04
		[XmlIgnore]
		public bool IsBusy
		{
			get
			{
				bool result;
				lock (this)
				{
					if (!this._isBusy)
					{
						result = false;
					}
					else
					{
						if (this._task == null)
						{
							throw new Exception("StartupPreferences.IsBusy; was busy but task is null?");
						}
						if (this._task.IsFaulted)
						{
							Exception e = this._task.Exception.GetBaseException();
							Game1.log.Error("StartupPreferences._task failed with an exception.", e);
							throw e;
						}
						if (this._task.IsCompleted)
						{
							this._task = null;
							this._isBusy = false;
							if (this._pendingApplyLanguage)
							{
								this._SetLanguageFromCode(this.languageCode);
							}
						}
						result = this._isBusy;
					}
				}
				return result;
			}
		}

		// Token: 0x060014A0 RID: 5280 RVA: 0x000F7AC4 File Offset: 0x000F5CC4
		private void Init()
		{
			this.isLoaded = false;
			this.ensureFolderStructureExists();
		}

		// Token: 0x060014A1 RID: 5281 RVA: 0x000F7AD4 File Offset: 0x000F5CD4
		public void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			string language_id = code.ToString();
			if (code == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage != null)
			{
				language_id = LocalizedContentManager.CurrentModLanguage.Id;
			}
			if (this.isLoaded && this.languageCode != language_id)
			{
				this.savePreferences(false, true);
			}
		}

		// Token: 0x060014A2 RID: 5282 RVA: 0x000F7B24 File Offset: 0x000F5D24
		private void ensureFolderStructureExists()
		{
			Program.GetAppDataFolder(null, true);
		}

		// Token: 0x060014A3 RID: 5283 RVA: 0x000F7B30 File Offset: 0x000F5D30
		public void savePreferences(bool async, bool update_language_from_ingame_language = false)
		{
			lock (this)
			{
				if (update_language_from_ingame_language)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
					{
						this.languageCode = LocalizedContentManager.CurrentModLanguage.Id;
					}
					else
					{
						this.languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
					}
				}
				try
				{
					this._savePreferences();
				}
				catch (Exception ex)
				{
					Game1.log.Error("StartupPreferences._task failed with an exception.", ex);
					throw ex;
				}
			}
		}

		// Token: 0x060014A4 RID: 5284 RVA: 0x000F7BC4 File Offset: 0x000F5DC4
		private void _savePreferences()
		{
			string fullFilePath = Path.Combine(Program.GetAppDataFolder(null, true), StartupPreferences._filename);
			try
			{
				this.ensureFolderStructureExists();
				if (File.Exists(fullFilePath))
				{
					File.Delete(fullFilePath);
				}
				using (FileStream stream = File.Create(fullFilePath))
				{
					this.writeSettings(stream);
				}
			}
			catch (Exception ex)
			{
				Game1.debugOutput = Game1.parseText(ex.Message);
			}
		}

		// Token: 0x060014A5 RID: 5285 RVA: 0x000F7C44 File Offset: 0x000F5E44
		private long writeSettings(Stream stream)
		{
			XmlWriterSettings settings = new XmlWriterSettings
			{
				CloseOutput = true,
				Indent = true
			};
			long length;
			using (XmlWriter writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				StartupPreferences.serializer.SerializeFast(writer, this);
				writer.WriteEndDocument();
				writer.Flush();
				length = stream.Length;
			}
			return length;
		}

		// Token: 0x060014A6 RID: 5286 RVA: 0x000F7CB0 File Offset: 0x000F5EB0
		public void loadPreferences(bool async, bool applyLanguage)
		{
			lock (this)
			{
				this._pendingApplyLanguage = applyLanguage;
				this.Init();
				try
				{
					this._loadPreferences();
				}
				catch (Exception ex)
				{
					AggregateException exception = this._task.Exception;
					Exception e = ((exception != null) ? exception.GetBaseException() : null) ?? ex;
					Game1.log.Error("StartupPreferences._task failed with an exception.", e);
					throw e;
				}
				if (applyLanguage)
				{
					this._SetLanguageFromCode(this.languageCode);
				}
			}
		}

		// Token: 0x060014A7 RID: 5287 RVA: 0x000F7D48 File Offset: 0x000F5F48
		protected virtual void _SetLanguageFromCode(string language_code_string)
		{
			List<ModLanguage> mod_languages = DataLoader.AdditionalLanguages(Game1.content);
			bool found_language = false;
			if (mod_languages != null)
			{
				foreach (ModLanguage mod_language in mod_languages)
				{
					if (mod_language.Id == language_code_string)
					{
						LocalizedContentManager.SetModLanguage(mod_language);
						found_language = true;
						break;
					}
				}
			}
			if (!found_language)
			{
				LocalizedContentManager.LanguageCode language_code;
				if (Utility.TryParseEnum<LocalizedContentManager.LanguageCode>(language_code_string, out language_code) && language_code != LocalizedContentManager.LanguageCode.mod)
				{
					LocalizedContentManager.CurrentLanguageCode = language_code;
					return;
				}
				LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.GetDefaultLanguageCode();
			}
		}

		// Token: 0x060014A8 RID: 5288 RVA: 0x000F7DDC File Offset: 0x000F5FDC
		private void _loadPreferences()
		{
			string fullFilePath = Path.Combine(Program.GetAppDataFolder(null, true), StartupPreferences._filename);
			if (!File.Exists(fullFilePath))
			{
				Game1.log.Verbose("path '" + fullFilePath + "' did not exist and will be created");
				try
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
					{
						this.languageCode = LocalizedContentManager.CurrentModLanguage.Id;
					}
					else
					{
						this.languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
					}
					using (FileStream stream = File.Create(fullFilePath))
					{
						this.writeSettings(stream);
					}
				}
				catch (Exception e)
				{
					Game1.log.Error("_loadPreferences; exception occurred trying to create/write.", e);
					Game1.debugOutput = Game1.parseText(e.Message);
					return;
				}
			}
			try
			{
				using (FileStream stream2 = File.Open(fullFilePath, FileMode.Open, FileAccess.Read))
				{
					this.readSettings(stream2);
				}
				this.isLoaded = true;
			}
			catch (Exception e2)
			{
				Game1.log.Error("_loadPreferences; exception occurred trying open/read.", e2);
				Game1.debugOutput = Game1.parseText(e2.Message);
			}
		}

		// Token: 0x060014A9 RID: 5289 RVA: 0x000F7F18 File Offset: 0x000F6118
		private void readSettings(Stream stream)
		{
			StartupPreferences p = (StartupPreferences)StartupPreferences.serializer.DeserializeFast(stream);
			this.startMuted = p.startMuted;
			this.timesPlayed = p.timesPlayed + 1;
			this.levelTenCombat = p.levelTenCombat;
			this.levelTenFishing = p.levelTenFishing;
			this.levelTenForaging = p.levelTenForaging;
			this.levelTenMining = p.levelTenMining;
			this.skipWindowPreparation = p.skipWindowPreparation;
			this.windowMode = p.windowMode;
			this.displayIndex = p.displayIndex;
			this.playerLimit = p.playerLimit;
			this.gamepadMode = p.gamepadMode;
			this.fullscreenResolutionX = p.fullscreenResolutionX;
			this.fullscreenResolutionY = p.fullscreenResolutionY;
			this.lastEnteredIP = p.lastEnteredIP;
			this.languageCode = p.languageCode;
			this.clientOptions = p.clientOptions;
		}

		// Token: 0x04000D30 RID: 3376
		public const int windowed_borderless = 0;

		// Token: 0x04000D31 RID: 3377
		public const int windowed = 1;

		// Token: 0x04000D32 RID: 3378
		public const int fullscreen = 2;

		// Token: 0x04000D33 RID: 3379
		private static readonly string _filename = "startup_preferences";

		// Token: 0x04000D34 RID: 3380
		public static XmlSerializer serializer = null;

		// Token: 0x04000D35 RID: 3381
		public bool startMuted;

		// Token: 0x04000D36 RID: 3382
		public bool levelTenFishing;

		// Token: 0x04000D37 RID: 3383
		public bool levelTenMining;

		// Token: 0x04000D38 RID: 3384
		public bool levelTenForaging;

		// Token: 0x04000D39 RID: 3385
		public bool levelTenCombat;

		// Token: 0x04000D3A RID: 3386
		public bool skipWindowPreparation;

		// Token: 0x04000D3B RID: 3387
		public bool sawAdvancedCharacterCreationIndicator;

		// Token: 0x04000D3C RID: 3388
		public int timesPlayed;

		// Token: 0x04000D3D RID: 3389
		public int windowMode;

		// Token: 0x04000D3E RID: 3390
		public int displayIndex = -1;

		// Token: 0x04000D3F RID: 3391
		public Options.GamepadModes gamepadMode;

		// Token: 0x04000D40 RID: 3392
		public int playerLimit = -1;

		// Token: 0x04000D41 RID: 3393
		public int fullscreenResolutionX;

		// Token: 0x04000D42 RID: 3394
		public int fullscreenResolutionY;

		// Token: 0x04000D43 RID: 3395
		public string lastEnteredIP = "";

		// Token: 0x04000D44 RID: 3396
		public string languageCode;

		// Token: 0x04000D45 RID: 3397
		public Options clientOptions = new Options();

		// Token: 0x04000D46 RID: 3398
		[XmlIgnore]
		public bool isLoaded;

		// Token: 0x04000D47 RID: 3399
		private bool _isBusy;

		// Token: 0x04000D48 RID: 3400
		private bool _pendingApplyLanguage;

		// Token: 0x04000D49 RID: 3401
		private Task _task;
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000CB RID: 203
	public class GameRunner : Game
	{
		// Token: 0x06000DDB RID: 3547 RVA: 0x00094510 File Offset: 0x00092710
		public GameRunner()
		{
			Program.sdk.EarlyInitialize();
			if (!Program.releaseBuild)
			{
				base.InactiveSleepTime = new TimeSpan(0L);
			}
			Game1.graphics = new GraphicsDeviceManager(this);
			Game1.graphics.PreparingDeviceSettings += delegate([Nullable(2)] object sender, PreparingDeviceSettingsEventArgs args)
			{
				args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
			};
			Game1.graphics.PreferredBackBufferWidth = 1280;
			Game1.graphics.PreferredBackBufferHeight = 720;
			base.Content.RootDirectory = "Content";
			SpriteBatch.TextureTuckAmount = 0.001f;
			LocalMultiplayer.Initialize();
			ItemRegistry.RegisterItemTypes();
			GameRunner.MaxTextureSize = int.MaxValue;
			base.Window.AllowUserResizing = true;
			this.SubscribeClientSizeChange();
			base.Exiting += delegate([Nullable(2)] object sender, EventArgs args)
			{
				this.ExecuteForInstances(delegate(Game1 instance)
				{
					instance.exitEvent(sender, args);
				});
				Process.GetCurrentProcess().Kill();
			};
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			LocalizedContentManager.OnLanguageChange += delegate(LocalizedContentManager.LanguageCode code)
			{
				this.ExecuteForInstances(delegate(Game1 instance)
				{
					instance.TranslateFields();
				});
			};
			DebugTools.GameConstructed(this);
		}

		// Token: 0x06000DDC RID: 3548 RVA: 0x00094638 File Offset: 0x00092838
		protected override void OnActivated(object sender, EventArgs args)
		{
			this.ExecuteForInstances(delegate(Game1 instance)
			{
				instance.Instance_OnActivated(sender, args);
			});
		}

		// Token: 0x06000DDD RID: 3549 RVA: 0x0009466B File Offset: 0x0009286B
		public void SubscribeClientSizeChange()
		{
			base.Window.ClientSizeChanged += this.OnWindowSizeChange;
		}

		// Token: 0x06000DDE RID: 3550 RVA: 0x00094684 File Offset: 0x00092884
		public void OnWindowSizeChange(object sender, EventArgs args)
		{
			base.Window.ClientSizeChanged -= this.OnWindowSizeChange;
			this._windowSizeChanged = true;
		}

		// Token: 0x06000DDF RID: 3551 RVA: 0x000946A4 File Offset: 0x000928A4
		protected override void Draw(GameTime gameTime)
		{
			if (this._windowSizeChanged)
			{
				this.ExecuteForInstances(delegate(Game1 instance)
				{
					instance.Window_ClientSizeChanged(null, null);
				});
				this._windowSizeChanged = false;
				this.SubscribeClientSizeChange();
			}
			foreach (Game1 instance2 in this.gameInstances)
			{
				GameRunner.LoadInstance(instance2, false);
				Viewport old_viewport = base.GraphicsDevice.Viewport;
				Game1.graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Math.Min(instance2.localMultiplayerWindow.Width, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth), Math.Min(instance2.localMultiplayerWindow.Height, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight));
				instance2.Instance_Draw(gameTime);
				base.GraphicsDevice.Viewport = old_viewport;
				GameRunner.SaveInstance(instance2, false);
			}
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				base.GraphicsDevice.Clear(Game1.bgColor);
				foreach (Game1 game in this.gameInstances)
				{
					Game1.isRenderingScreenBuffer = true;
					game.DrawSplitScreenWindow();
					Game1.isRenderingScreenBuffer = false;
				}
			}
			base.Draw(gameTime);
		}

		// Token: 0x06000DE0 RID: 3552 RVA: 0x00094828 File Offset: 0x00092A28
		public int GetNewInstanceID()
		{
			int num = this.nextInstanceId;
			this.nextInstanceId = num + 1;
			return num;
		}

		// Token: 0x06000DE1 RID: 3553 RVA: 0x00094846 File Offset: 0x00092A46
		protected override void Initialize()
		{
			DebugTools.BeforeGameInitialize(this);
			this.InitializeMainInstance();
			base.IsFixedTimeStep = true;
			base.Initialize();
			Game1.graphics.SynchronizeWithVerticalRetrace = true;
			Program.sdk.Initialize();
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x00094876 File Offset: 0x00092A76
		public bool WasWindowSizeChanged()
		{
			return this._windowSizeChanged;
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x0009487E File Offset: 0x00092A7E
		public int GetMaxSimultaneousPlayers()
		{
			return 4;
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x00094881 File Offset: 0x00092A81
		public void InitializeMainInstance()
		{
			this.gameInstances = new List<Game1>();
			this.AddGameInstance(PlayerIndex.One);
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x00094898 File Offset: 0x00092A98
		public virtual void ExecuteForInstances(Action<Game1> action)
		{
			Game1 old_game = Game1.game1;
			if (old_game != null)
			{
				GameRunner.SaveInstance(old_game, false);
			}
			foreach (Game1 instance in this.gameInstances)
			{
				GameRunner.LoadInstance(instance, false);
				action(instance);
				GameRunner.SaveInstance(instance, false);
			}
			if (old_game != null)
			{
				GameRunner.LoadInstance(old_game, false);
				return;
			}
			Game1.game1 = null;
		}

		// Token: 0x06000DE6 RID: 3558 RVA: 0x0009491C File Offset: 0x00092B1C
		public virtual void RemoveGameInstance(Game1 instance)
		{
			if (this.gameInstances.Contains(instance) && !this.gameInstancesToRemove.Contains(instance))
			{
				this.gameInstancesToRemove.Add(instance);
			}
		}

		// Token: 0x06000DE7 RID: 3559 RVA: 0x00094948 File Offset: 0x00092B48
		public virtual void AddGameInstance(PlayerIndex player_index)
		{
			Game1 old_game = Game1.game1;
			if (old_game != null)
			{
				GameRunner.SaveInstance(old_game, true);
			}
			if (this.gameInstances.Count > 0)
			{
				Game1 game = this.gameInstances[0];
				GameRunner.LoadInstance(game, false);
				Game1.StartLocalMultiplayerIfNecessary();
				GameRunner.SaveInstance(game, true);
			}
			Game1 new_instance = (this.gameInstances.Count == 0) ? this.CreateGameInstance(PlayerIndex.One, 0) : this.CreateGameInstance(player_index, this.gameInstances.Count);
			this.gameInstances.Add(new_instance);
			if (this.gamePtr == null)
			{
				this.gamePtr = new_instance;
			}
			if (this.gameInstances.Count > 0)
			{
				new_instance.staticVarHolder = Activator.CreateInstance(LocalMultiplayer.StaticVarHolderType);
				GameRunner.SetInstanceDefaults(new_instance);
				GameRunner.LoadInstance(new_instance, false);
			}
			Game1.game1 = new_instance;
			new_instance.Instance_Initialize();
			if (this.shouldLoadContent)
			{
				new_instance.Instance_LoadContent();
			}
			GameRunner.SaveInstance(new_instance, false);
			if (old_game != null)
			{
				GameRunner.LoadInstance(old_game, false);
			}
			else
			{
				Game1.game1 = null;
			}
			this._windowSizeChanged = true;
		}

		// Token: 0x06000DE8 RID: 3560 RVA: 0x00094A3B File Offset: 0x00092C3B
		public virtual Game1 CreateGameInstance(PlayerIndex player_index = PlayerIndex.One, int index = 0)
		{
			return new Game1(player_index, index);
		}

		// Token: 0x06000DE9 RID: 3561 RVA: 0x00094A44 File Offset: 0x00092C44
		protected override void LoadContent()
		{
			Game1.graphics.PreferredBackBufferWidth = 1280;
			Game1.graphics.PreferredBackBufferHeight = 720;
			Game1.graphics.ApplyChanges();
			GameRunner.LoadInstance(this.gamePtr, false);
			this.gamePtr.Instance_LoadContent();
			GameRunner.SaveInstance(this.gamePtr, false);
			DebugTools.GameLoadContent(this);
			foreach (Game1 instance in this.gameInstances)
			{
				if (instance != this.gamePtr)
				{
					GameRunner.LoadInstance(instance, false);
					instance.Instance_LoadContent();
					GameRunner.SaveInstance(instance, false);
				}
			}
			this.shouldLoadContent = true;
			base.LoadContent();
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x00094B0C File Offset: 0x00092D0C
		protected override void UnloadContent()
		{
			this.gamePtr.Instance_UnloadContent();
			base.UnloadContent();
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x00094B20 File Offset: 0x00092D20
		protected override void Update(GameTime gameTime)
		{
			GameStateQuery.Update();
			for (int i = 0; i < this.activeNewDayProcesses.Count; i++)
			{
				KeyValuePair<Game1, IEnumerator<int>> active_new_days = this.activeNewDayProcesses[i];
				Game1 instance = this.activeNewDayProcesses[i].Key;
				GameRunner.LoadInstance(instance, false);
				if (!active_new_days.Value.MoveNext())
				{
					instance.isLocalMultiplayerNewDayActive = false;
					this.activeNewDayProcesses.RemoveAt(i);
					i--;
					Utility.CollectGarbage("", 0);
				}
				GameRunner.SaveInstance(instance, false);
			}
			while (this.startButtonState.Count < 4)
			{
				this.startButtonState.Add(-1);
			}
			for (PlayerIndex player_index = PlayerIndex.One; player_index <= PlayerIndex.Four; player_index++)
			{
				if (GamePad.GetState(player_index).IsButtonDown(Buttons.Start))
				{
					if (this.startButtonState[(int)player_index] >= 0)
					{
						List<int> list = this.startButtonState;
						int index = (int)player_index;
						int num = list[index];
						list[index] = num + 1;
					}
				}
				else
				{
					this.startButtonState[(int)player_index] = 0;
				}
			}
			for (int j = 0; j < this.gameInstances.Count; j++)
			{
				Game1 instance2 = this.gameInstances[j];
				GameRunner.LoadInstance(instance2, false);
				if (j == 0)
				{
					PlayerIndex start_player_index = PlayerIndex.Two;
					if (instance2.instanceOptions.gamepadMode == Options.GamepadModes.ForceOff)
					{
						start_player_index = PlayerIndex.One;
					}
					for (PlayerIndex player_index2 = start_player_index; player_index2 <= PlayerIndex.Four; player_index2++)
					{
						bool fail = false;
						using (List<Game1>.Enumerator enumerator = this.gameInstances.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.instancePlayerOneIndex == player_index2)
								{
									fail = true;
									break;
								}
							}
						}
						if (!fail && instance2.IsLocalCoopJoinable() && this.IsStartDown(player_index2) && instance2.ShowLocalCoopJoinMenu())
						{
							this.InvalidateStartPress(player_index2);
						}
					}
				}
				else
				{
					Game1.options.gamepadMode = Options.GamepadModes.ForceOn;
				}
				Game1.debugTimings.StartUpdateTimer();
				instance2.Instance_Update(gameTime);
				Game1.debugTimings.StopUpdateTimer();
				GameRunner.SaveInstance(instance2, false);
			}
			if (this.gameInstancesToRemove.Count > 0)
			{
				foreach (Game1 instance3 in this.gameInstancesToRemove)
				{
					GameRunner.LoadInstance(instance3, false);
					instance3.exitEvent(null, null);
					this.gameInstances.Remove(instance3);
					Game1.game1 = null;
				}
				for (int k = 0; k < this.gameInstances.Count; k++)
				{
					this.gameInstances[k].instanceIndex = k;
				}
				if (this.gameInstances.Count == 1)
				{
					Game1 game = this.gameInstances[0];
					GameRunner.LoadInstance(game, true);
					game.staticVarHolder = null;
					Game1.EndLocalMultiplayer();
				}
				bool controller_1_assigned = false;
				if (this.gameInstances.Count > 0)
				{
					using (List<Game1>.Enumerator enumerator = this.gameInstances.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.instancePlayerOneIndex == PlayerIndex.One)
							{
								controller_1_assigned = true;
								break;
							}
						}
					}
					if (!controller_1_assigned)
					{
						this.gameInstances[0].instancePlayerOneIndex = PlayerIndex.One;
					}
				}
				this.gameInstancesToRemove.Clear();
				this._windowSizeChanged = true;
			}
			base.Update(gameTime);
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x00094E88 File Offset: 0x00093088
		public virtual void InvalidateStartPress(PlayerIndex index)
		{
			if (index >= PlayerIndex.One && index < (PlayerIndex)this.startButtonState.Count)
			{
				this.startButtonState[(int)index] = -1;
			}
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x00094EA9 File Offset: 0x000930A9
		public virtual bool IsStartDown(PlayerIndex index)
		{
			return index >= PlayerIndex.One && index < (PlayerIndex)this.startButtonState.Count && this.startButtonState[(int)index] == 1;
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x00094ED0 File Offset: 0x000930D0
		private static void SetInstanceDefaults(InstanceGame instance)
		{
			for (int i = 0; i < LocalMultiplayer.staticDefaults.Count; i++)
			{
				object value = LocalMultiplayer.staticDefaults[i];
				value = ((value != null) ? value.DeepClone<object>() : null);
				LocalMultiplayer.staticFields[i].SetValue(null, value);
			}
			GameRunner.SaveInstance(instance, false);
		}

		// Token: 0x06000DEF RID: 3567 RVA: 0x00094F24 File Offset: 0x00093124
		public static void SaveInstance(InstanceGame instance, bool force = false)
		{
			if (!force && !LocalMultiplayer.IsLocalMultiplayer(false))
			{
				return;
			}
			if (instance.staticVarHolder == null)
			{
				instance.staticVarHolder = Activator.CreateInstance(LocalMultiplayer.StaticVarHolderType);
			}
			LocalMultiplayer.StaticSave(instance.staticVarHolder);
		}

		// Token: 0x06000DF0 RID: 3568 RVA: 0x00094F5C File Offset: 0x0009315C
		public static void LoadInstance(InstanceGame instance, bool force = false)
		{
			Game1.game1 = (instance as Game1);
			if (!force && !LocalMultiplayer.IsLocalMultiplayer(false))
			{
				return;
			}
			if (instance.staticVarHolder == null)
			{
				return;
			}
			LocalMultiplayer.StaticLoad(instance.staticVarHolder);
			Options options;
			if (Game1.player != null && Game1.player.isCustomized.Value && Game1.splitscreenOptions.TryGetValue(Game1.player.UniqueMultiplayerID, out options))
			{
				Game1.options = options;
			}
		}

		// Token: 0x04000935 RID: 2357
		public static GameRunner instance;

		// Token: 0x04000936 RID: 2358
		public List<Game1> gameInstances = new List<Game1>();

		// Token: 0x04000937 RID: 2359
		public List<Game1> gameInstancesToRemove = new List<Game1>();

		// Token: 0x04000938 RID: 2360
		public Game1 gamePtr;

		// Token: 0x04000939 RID: 2361
		public bool shouldLoadContent;

		// Token: 0x0400093A RID: 2362
		protected bool _initialized;

		// Token: 0x0400093B RID: 2363
		protected bool _windowSizeChanged;

		// Token: 0x0400093C RID: 2364
		public List<int> startButtonState = new List<int>();

		// Token: 0x0400093D RID: 2365
		public List<KeyValuePair<Game1, IEnumerator<int>>> activeNewDayProcesses = new List<KeyValuePair<Game1, IEnumerator<int>>>();

		// Token: 0x0400093E RID: 2366
		public int nextInstanceId;

		// Token: 0x0400093F RID: 2367
		public static int MaxTextureSize = 4096;
	}
}

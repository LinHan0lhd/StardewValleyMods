using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Movies;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;

namespace StardewValley.Minigames
{
	// Token: 0x02000234 RID: 564
	public class CraneGame : IMinigame
	{
		// Token: 0x06002549 RID: 9545 RVA: 0x001A1278 File Offset: 0x0019F478
		public CraneGame()
		{
			Utility.farmerHeardSong("crane_game");
			Utility.farmerHeardSong("crane_game_fast");
			this._effect = Game1.content.Load<Effect>("Effects\\ShadowRemoveMG3.8.0");
			this._content = Game1.content.CreateTemporary();
			this.spriteSheet = this._content.Load<Texture2D>("LooseSprites\\CraneGame");
			this._buttonStates = new Dictionary<CraneGame.GameButtons, int>();
			this._gameObjects = new List<CraneGame.CraneGameObject>();
			this._gameObjectTypes = new List<Type>();
			this._gameObjectsByType = new Dictionary<Type, List<CraneGame.CraneGameObject>>();
			this.changeScreenSize();
			new CraneGame.GameLogic(this);
			for (int i = 0; i < 9; i++)
			{
				this._buttonStates[(CraneGame.GameButtons)i] = 0;
			}
		}

		// Token: 0x0600254A RID: 9546 RVA: 0x001A1343 File Offset: 0x0019F543
		public void Quit()
		{
			if (!this._shouldQuit)
			{
				Action action = this.onQuit;
				if (action != null)
				{
					action();
				}
				this._shouldQuit = true;
			}
		}

		// Token: 0x0600254B RID: 9547 RVA: 0x001A1368 File Offset: 0x0019F568
		protected void _UpdateInput()
		{
			HashSet<InputButton> additional_keys = new HashSet<InputButton>();
			if (Game1.options.gamepadControls)
			{
				GamePadState pad_state = Game1.input.GetGamePadState();
				foreach (Buttons b in new ButtonCollection(ref pad_state))
				{
					Keys key = Utility.mapGamePadButtonToKey(b);
					additional_keys.Add(new InputButton(key));
				}
			}
			if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
			{
				additional_keys.Add(new InputButton(true));
			}
			else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed)
			{
				additional_keys.Add(new InputButton(false));
			}
			this._UpdateButtonState(CraneGame.GameButtons.Action, Game1.options.actionButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Tool, Game1.options.useToolButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Confirm, Game1.options.menuButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Cancel, Game1.options.cancelButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Run, Game1.options.runButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Up, Game1.options.moveUpButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Down, Game1.options.moveDownButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Left, Game1.options.moveLeftButton, additional_keys);
			this._UpdateButtonState(CraneGame.GameButtons.Right, Game1.options.moveRightButton, additional_keys);
		}

		// Token: 0x0600254C RID: 9548 RVA: 0x001A14B3 File Offset: 0x0019F6B3
		public bool IsButtonPressed(CraneGame.GameButtons button)
		{
			return this._buttonStates[button] == 1;
		}

		// Token: 0x0600254D RID: 9549 RVA: 0x001A14C4 File Offset: 0x0019F6C4
		public bool IsButtonDown(CraneGame.GameButtons button)
		{
			return this._buttonStates[button] > 0;
		}

		// Token: 0x0600254E RID: 9550 RVA: 0x001A14D8 File Offset: 0x0019F6D8
		protected void _UpdateButtonState(CraneGame.GameButtons button, InputButton[] keys, HashSet<InputButton> emulated_keys)
		{
			bool down = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), keys);
			for (int i = 0; i < keys.Length; i++)
			{
				if (emulated_keys.Contains(keys[i]))
				{
					down = true;
					break;
				}
			}
			if (this._buttonStates[button] == -1)
			{
				this._buttonStates[button] = 0;
			}
			if (down)
			{
				Dictionary<CraneGame.GameButtons, int> buttonStates = this._buttonStates;
				int num = buttonStates[button];
				buttonStates[button] = num + 1;
				return;
			}
			if (this._buttonStates[button] > 0)
			{
				this._buttonStates[button] = -1;
			}
		}

		// Token: 0x0600254F RID: 9551 RVA: 0x001A1568 File Offset: 0x0019F768
		public T GetObjectAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGame.CraneGameObject
		{
			foreach (CraneGame.CraneGameObject craneGameObject in this._gameObjects)
			{
				T match = craneGameObject as T;
				if (match != null && match.GetBounds().Contains((int)point.X, (int)point.Y))
				{
					return match;
				}
			}
			return default(T);
		}

		// Token: 0x06002550 RID: 9552 RVA: 0x001A15FC File Offset: 0x0019F7FC
		public List<T> GetObjectsAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGame.CraneGameObject
		{
			List<T> results = new List<T>();
			foreach (CraneGame.CraneGameObject craneGameObject in this._gameObjects)
			{
				T match = craneGameObject as T;
				if (match != null && match.GetBounds().Contains((int)point.X, (int)point.Y))
				{
					results.Add(match);
					if (max_count >= 0 && results.Count >= max_count)
					{
						return results;
					}
				}
			}
			return results;
		}

		// Token: 0x06002551 RID: 9553 RVA: 0x001A16A0 File Offset: 0x0019F8A0
		public T GetObjectOfType<T>() where T : CraneGame.CraneGameObject
		{
			List<CraneGame.CraneGameObject> gameObjects;
			if (this._gameObjectsByType.TryGetValue(typeof(T), out gameObjects) && gameObjects.Count > 0)
			{
				return gameObjects[0] as T;
			}
			return default(T);
		}

		// Token: 0x06002552 RID: 9554 RVA: 0x001A16EC File Offset: 0x0019F8EC
		public List<T> GetObjectsOfType<T>() where T : CraneGame.CraneGameObject
		{
			List<T> results = new List<T>();
			foreach (CraneGame.CraneGameObject craneGameObject in this._gameObjects)
			{
				T match = craneGameObject as T;
				if (match != null)
				{
					results.Add(match);
				}
			}
			return results;
		}

		// Token: 0x06002553 RID: 9555 RVA: 0x001A1758 File Offset: 0x0019F958
		public List<T> GetOverlaps<T>(CraneGame.CraneGameObject target, int max_count = -1) where T : CraneGame.CraneGameObject
		{
			List<T> results = new List<T>();
			foreach (CraneGame.CraneGameObject craneGameObject in this._gameObjects)
			{
				T match = craneGameObject as T;
				if (match != null && target.GetBounds().Intersects(match.GetBounds()) && target != match)
				{
					results.Add(match);
					if (max_count >= 0 && results.Count >= max_count)
					{
						return results;
					}
				}
			}
			return results;
		}

		// Token: 0x06002554 RID: 9556 RVA: 0x001A1800 File Offset: 0x0019FA00
		public bool tick(GameTime time)
		{
			if (this._shouldQuit)
			{
				return true;
			}
			if (this.freezeFrames > 0)
			{
				this.freezeFrames--;
			}
			else
			{
				this._UpdateInput();
				for (int i = 0; i < this._gameObjects.Count; i++)
				{
					if (this._gameObjects[i] != null)
					{
						this._gameObjects[i].Update(time);
					}
				}
			}
			if (this.IsButtonPressed(CraneGame.GameButtons.Confirm))
			{
				this.Quit();
				Game1.playSound("bigDeSelect", null);
				CraneGame.GameLogic logic = this.GetObjectOfType<CraneGame.GameLogic>();
				if (logic != null && logic.collectedItems.Count > 0)
				{
					List<Item> items = new List<Item>();
					foreach (Item item in logic.collectedItems)
					{
						items.Add(item.getOne());
					}
					Game1.activeClickableMenu = new ItemGrabMenu(items, false, true, null, null, "Rewards", null, false, false, false, false, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				}
			}
			return false;
		}

		// Token: 0x06002555 RID: 9557 RVA: 0x001A1920 File Offset: 0x0019FB20
		public bool forceQuit()
		{
			this.Quit();
			this.unload();
			CraneGame.GameLogic logic = this.GetObjectOfType<CraneGame.GameLogic>();
			if (logic != null)
			{
				foreach (Item item in logic.collectedItems)
				{
					Utility.CollectOrDrop(item.getOne());
				}
			}
			return true;
		}

		// Token: 0x06002556 RID: 9558 RVA: 0x001A1990 File Offset: 0x0019FB90
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002557 RID: 9559 RVA: 0x001A199C File Offset: 0x0019FB9C
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x06002558 RID: 9560 RVA: 0x001A199F File Offset: 0x0019FB9F
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002559 RID: 9561 RVA: 0x001A19A1 File Offset: 0x0019FBA1
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x0600255A RID: 9562 RVA: 0x001A19A3 File Offset: 0x0019FBA3
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600255B RID: 9563 RVA: 0x001A19A5 File Offset: 0x0019FBA5
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x0600255C RID: 9564 RVA: 0x001A19A7 File Offset: 0x0019FBA7
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x0600255D RID: 9565 RVA: 0x001A19A9 File Offset: 0x0019FBA9
		public void receiveKeyPress(Keys k)
		{
		}

		// Token: 0x0600255E RID: 9566 RVA: 0x001A19AB File Offset: 0x0019FBAB
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x0600255F RID: 9567 RVA: 0x001A19B0 File Offset: 0x0019FBB0
		public void RegisterGameObject(CraneGame.CraneGameObject game_object)
		{
			if (!this._gameObjectTypes.Contains(game_object.GetType()))
			{
				this._gameObjectTypes.Add(game_object.GetType());
				this._gameObjectsByType[game_object.GetType()] = new List<CraneGame.CraneGameObject>();
			}
			this._gameObjectsByType[game_object.GetType()].Add(game_object);
			this._gameObjects.Add(game_object);
		}

		// Token: 0x06002560 RID: 9568 RVA: 0x001A1A1A File Offset: 0x0019FC1A
		public void UnregisterGameObject(CraneGame.CraneGameObject game_object)
		{
			this._gameObjectsByType[game_object.GetType()].Remove(game_object);
			this._gameObjects.Remove(game_object);
		}

		// Token: 0x06002561 RID: 9569 RVA: 0x001A1A44 File Offset: 0x0019FC44
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, this._effect, null);
			b.Draw(this.spriteSheet, this.upperLeft, new Rectangle?(new Rectangle(0, 0, this.gameWidth, this.gameHeight)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Dictionary<CraneGame.CraneGameObject, float> depth_lookup = new Dictionary<CraneGame.CraneGameObject, float>();
			float lowest_depth = 0f;
			float highest_depth = 0f;
			for (int i = 0; i < this._gameObjects.Count; i++)
			{
				if (this._gameObjects[i] != null)
				{
					float depth = this._gameObjects[i].GetRendererLayerDepth();
					depth_lookup[this._gameObjects[i]] = depth;
					if (depth < lowest_depth)
					{
						lowest_depth = depth;
					}
					if (depth > highest_depth)
					{
						highest_depth = depth;
					}
				}
			}
			for (int j = 0; j < this._gameObjectTypes.Count; j++)
			{
				Type type = this._gameObjectTypes[j];
				for (int k = 0; k < this._gameObjectsByType[type].Count; k++)
				{
					float drawn_depth = Utility.Lerp(0.1f, 0.9f, (depth_lookup[this._gameObjectsByType[type][k]] - lowest_depth) / (highest_depth - lowest_depth));
					this._gameObjectsByType[type][k].Draw(b, drawn_depth);
				}
			}
			b.End();
		}

		// Token: 0x06002562 RID: 9570 RVA: 0x001A1BD0 File Offset: 0x0019FDD0
		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			Rectangle localMultiplayerWindow = Game1.game1.localMultiplayerWindow;
			float w = (float)localMultiplayerWindow.Width;
			float h = (float)localMultiplayerWindow.Height;
			Vector2 tmp = new Vector2(w / 2f, h / 2f) * pixel_zoom_adjustment;
			tmp.X -= (float)(this.gameWidth / 2 * 4);
			tmp.Y -= (float)(this.gameHeight / 2 * 4);
			this.upperLeft = tmp;
		}

		// Token: 0x06002563 RID: 9571 RVA: 0x001A1C54 File Offset: 0x0019FE54
		public void unload()
		{
			Game1.stopMusicTrack(MusicContext.MiniGame);
			ICue cue = this.music;
			bool? flag = (cue != null) ? new bool?(cue.IsPlaying) : null;
			if (flag != null && flag.GetValueOrDefault())
			{
				this.music.Stop(AudioStopOptions.Immediate);
			}
			ICue cue2 = this.fastMusic;
			flag = ((cue2 != null) ? new bool?(cue2.IsPlaying) : null);
			if (flag != null && flag.GetValueOrDefault())
			{
				this.fastMusic.Stop(AudioStopOptions.Immediate);
			}
			ICue cue3 = this.craneSound;
			flag = ((cue3 != null) ? new bool?(cue3.IsPlaying) : null);
			if (flag != null && flag.GetValueOrDefault())
			{
				this.craneSound.Stop(AudioStopOptions.Immediate);
			}
			this._content.Unload();
		}

		// Token: 0x06002564 RID: 9572 RVA: 0x001A1D2C File Offset: 0x0019FF2C
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x06002565 RID: 9573 RVA: 0x001A1D2E File Offset: 0x0019FF2E
		public string minigameId()
		{
			return "CraneGame";
		}

		// Token: 0x040016AA RID: 5802
		public int gameWidth = 304;

		// Token: 0x040016AB RID: 5803
		public int gameHeight = 150;

		// Token: 0x040016AC RID: 5804
		protected LocalizedContentManager _content;

		// Token: 0x040016AD RID: 5805
		public Texture2D spriteSheet;

		// Token: 0x040016AE RID: 5806
		public Vector2 upperLeft;

		// Token: 0x040016AF RID: 5807
		protected List<CraneGame.CraneGameObject> _gameObjects;

		// Token: 0x040016B0 RID: 5808
		protected Dictionary<CraneGame.GameButtons, int> _buttonStates;

		// Token: 0x040016B1 RID: 5809
		protected bool _shouldQuit;

		// Token: 0x040016B2 RID: 5810
		public Action onQuit;

		// Token: 0x040016B3 RID: 5811
		public ICue music;

		// Token: 0x040016B4 RID: 5812
		public ICue fastMusic;

		// Token: 0x040016B5 RID: 5813
		public Effect _effect;

		// Token: 0x040016B6 RID: 5814
		public int freezeFrames;

		// Token: 0x040016B7 RID: 5815
		public ICue craneSound;

		// Token: 0x040016B8 RID: 5816
		public List<Type> _gameObjectTypes;

		// Token: 0x040016B9 RID: 5817
		public Dictionary<Type, List<CraneGame.CraneGameObject>> _gameObjectsByType;

		// Token: 0x0200059B RID: 1435
		public enum GameButtons
		{
			// Token: 0x04002C4D RID: 11341
			Action,
			// Token: 0x04002C4E RID: 11342
			Tool,
			// Token: 0x04002C4F RID: 11343
			Confirm,
			// Token: 0x04002C50 RID: 11344
			Cancel,
			// Token: 0x04002C51 RID: 11345
			Run,
			// Token: 0x04002C52 RID: 11346
			Up,
			// Token: 0x04002C53 RID: 11347
			Left,
			// Token: 0x04002C54 RID: 11348
			Down,
			// Token: 0x04002C55 RID: 11349
			Right,
			// Token: 0x04002C56 RID: 11350
			MAX
		}

		// Token: 0x0200059C RID: 1436
		public class GameLogic : CraneGame.CraneGameObject
		{
			// Token: 0x060041FD RID: 16893 RVA: 0x0030CF80 File Offset: 0x0030B180
			public GameLogic(CraneGame game) : base(game)
			{
				Game1.playSound("crane_game", out this._game.music);
				this._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
				this._claw = new CraneGame.Claw(this._game);
				this._claw.position = this._startPosition;
				this._claw.zPosition = 50f;
				this.collectedItems = new List<Item>();
				this.SetState(CraneGame.GameLogic.GameStates.Setup);
				new CraneGame.Bush(this._game, 55, 2, 3, 31, 111);
				new CraneGame.Bush(this._game, 45, 2, 2, 112, 84);
				new CraneGame.Bush(this._game, 45, 2, 2, 63, 63);
				new CraneGame.Bush(this._game, 48, 1, 2, 56, 80);
				new CraneGame.Bush(this._game, 48, 1, 2, 72, 80);
				new CraneGame.Bush(this._game, 48, 1, 2, 56, 96);
				new CraneGame.Bush(this._game, 48, 1, 2, 72, 96);
				new CraneGame.Bush(this._game, 48, 1, 2, 56, 112);
				new CraneGame.Bush(this._game, 48, 1, 2, 72, 112);
				new CraneGame.Bush(this._game, 45, 2, 2, 159, 63);
				new CraneGame.Bush(this._game, 48, 1, 2, 152, 80);
				new CraneGame.Bush(this._game, 48, 1, 2, 168, 80);
				new CraneGame.Bush(this._game, 48, 1, 2, 152, 96);
				new CraneGame.Bush(this._game, 48, 1, 2, 168, 96);
				new CraneGame.Bush(this._game, 48, 1, 2, 152, 112);
				new CraneGame.Bush(this._game, 48, 1, 2, 168, 112);
				this.sunShockedFace = new CraneGame.CraneGameObject(this._game);
				this.sunShockedFace.SetSpriteFromIndex(9);
				this.sunShockedFace.position = new Vector2(96f, 0f);
				this.sunShockedFace.spriteAnchor = Vector2.Zero;
				CraneGame.CraneGameObject craneGameObject = new CraneGame.CraneGameObject(this._game);
				craneGameObject.position.X = 16f;
				craneGameObject.position.Y = 87f;
				craneGameObject.SetSpriteFromIndex(3);
				craneGameObject.spriteRect.Width = 32;
				craneGameObject.spriteAnchor = new Vector2(0f, 15f);
				this.joystick = new CraneGame.CraneGameObject(this._game);
				this.joystick.position.X = 151f;
				this.joystick.position.Y = 134f;
				this.joystick.SetSpriteFromIndex(28);
				this.joystick.spriteRect.Width = 32;
				this.joystick.spriteRect.Height = 48;
				this.joystick.spriteAnchor = new Vector2(15f, 47f);
				this.lives = this.maxLives;
				this.moveRightIndicator = new CraneGame.CraneGameObject(this._game);
				this.moveRightIndicator.position.X = 21f;
				this.moveRightIndicator.position.Y = 126f;
				this.moveRightIndicator.SetSpriteFromIndex(26);
				this.moveRightIndicator.spriteAnchor = Vector2.Zero;
				this.moveRightIndicator.visible = false;
				this.moveDownIndicator = new CraneGame.CraneGameObject(this._game);
				this.moveDownIndicator.position.X = 49f;
				this.moveDownIndicator.position.Y = 126f;
				this.moveDownIndicator.SetSpriteFromIndex(27);
				this.moveDownIndicator.spriteAnchor = Vector2.Zero;
				this.moveDownIndicator.visible = false;
				this.creditsDisplay = new CraneGame.CraneGameObject(this._game);
				this.creditsDisplay.SetSpriteFromIndex(70);
				this.creditsDisplay.position = new Vector2(234f, 125f);
				this.creditsDisplay.spriteAnchor = Vector2.Zero;
				this.timeDisplay1 = new CraneGame.CraneGameObject(this._game);
				this.timeDisplay1.SetSpriteFromIndex(70);
				this.timeDisplay1.position = new Vector2(274f, 125f);
				this.timeDisplay1.spriteAnchor = Vector2.Zero;
				this.timeDisplay2 = new CraneGame.CraneGameObject(this._game);
				this.timeDisplay2.SetSpriteFromIndex(70);
				this.timeDisplay2.position = new Vector2(285f, 125f);
				this.timeDisplay2.spriteAnchor = Vector2.Zero;
				int level_width = 17;
				for (int i = 0; i < this.conveyerBeltTiles.Length; i++)
				{
					if (this.conveyerBeltTiles[i] != 0)
					{
						int x = i % level_width + 1;
						int y = i / level_width + 3;
						switch (this.conveyerBeltTiles[i])
						{
						case 1:
							new CraneGame.ConveyerBelt(this._game, x, y, 0).SetSpriteFromCorner(240, 224);
							break;
						case 2:
							new CraneGame.ConveyerBelt(this._game, x, y, 2);
							break;
						case 3:
							new CraneGame.ConveyerBelt(this._game, x, y, 3).SetSpriteFromCorner(240, 256);
							break;
						case 4:
							new CraneGame.ConveyerBelt(this._game, x, y, 3);
							break;
						case 6:
							new CraneGame.ConveyerBelt(this._game, x, y, 1);
							break;
						case 7:
							new CraneGame.ConveyerBelt(this._game, x, y, 1).SetSpriteFromCorner(240, 272);
							break;
						case 8:
							new CraneGame.ConveyerBelt(this._game, x, y, 0);
							break;
						case 9:
							new CraneGame.ConveyerBelt(this._game, x, y, 2).SetSpriteFromCorner(240, 240);
							break;
						}
					}
				}
				Dictionary<int, List<Item>> possible_items = new Dictionary<int, List<Item>>();
				possible_items[1] = new List<Item>
				{
					ItemRegistry.Create("(F)1760", 1, 0, false),
					ItemRegistry.Create("(F)1761", 1, 0, false),
					ItemRegistry.Create("(F)1762", 1, 0, false),
					ItemRegistry.Create("(F)1763", 1, 0, false),
					ItemRegistry.Create("(F)1764", 1, 0, false),
					ItemRegistry.Create("(F)1365", 1, 0, false)
				};
				List<Item> item_list = new List<Item>();
				item_list.Add(ItemRegistry.Create("(F)1669", 1, 0, false));
				switch (Game1.season)
				{
				case Season.Spring:
					item_list.Add(ItemRegistry.Create("(F)1960", 1, 0, false));
					break;
				case Season.Summer:
					item_list.Add(ItemRegistry.Create("(F)1294", 1, 0, false));
					break;
				case Season.Fall:
					item_list.Add(ItemRegistry.Create("(F)1918", 1, 0, false));
					break;
				case Season.Winter:
					item_list.Add(ItemRegistry.Create("(F)1961", 1, 0, false));
					break;
				}
				item_list.Add(ItemRegistry.Create("(F)FancyHousePlant5", 1, 0, false));
				item_list.Add(ItemRegistry.Create("(F)FancyHousePlant4", 1, 0, false));
				item_list.Add(ItemRegistry.Create<Object>("(BC)2", 1, 0, false));
				possible_items[2] = item_list;
				item_list = new List<Item>();
				switch (Game1.season)
				{
				case Season.Spring:
					item_list.Add(ItemRegistry.Create<Object>("(BC)107", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)36", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)48", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)184", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)188", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)192", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)204", 1, 0, false));
					break;
				case Season.Summer:
					item_list.Add(ItemRegistry.Create("(F)985", 1, 0, false));
					item_list.Add(ItemRegistry.Create("(F)984", 1, 0, false));
					break;
				case Season.Fall:
					item_list.Add(ItemRegistry.Create("(F)1917", 1, 0, false));
					item_list.Add(ItemRegistry.Create("(F)1307", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)47", 1, 0, false));
					item_list.Add(ItemRegistry.Create("(F)1471", 1, 0, false));
					item_list.Add(ItemRegistry.Create("(F)1375", 1, 0, false));
					break;
				case Season.Winter:
					item_list.Add(ItemRegistry.Create("(F)1440", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)44", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)40", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)41", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)43", 1, 0, false));
					item_list.Add(ItemRegistry.Create<Object>("(BC)42", 1, 0, false));
					break;
				}
				possible_items[3] = item_list;
				MovieData movieData = MovieTheater.GetMovieToday();
				MovieData movieData3 = movieData;
				bool flag;
				if (movieData3 == null)
				{
					flag = false;
				}
				else
				{
					List<int> clearDefaultCranePrizeGroups = movieData3.ClearDefaultCranePrizeGroups;
					int? num = (clearDefaultCranePrizeGroups != null) ? new int?(clearDefaultCranePrizeGroups.Count) : null;
					int num2 = 0;
					flag = (num.GetValueOrDefault() > num2 & num != null);
				}
				if (flag)
				{
					foreach (int rarity in movieData.ClearDefaultCranePrizeGroups)
					{
						List<Item> itemList;
						if (!possible_items.TryGetValue(rarity, out itemList))
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Movie '");
							defaultInterpolatedStringHandler.AppendFormatted(movieData.Id);
							defaultInterpolatedStringHandler.AppendLiteral("' clears prize list for invalid rarity '");
							defaultInterpolatedStringHandler.AppendFormatted<int>(rarity);
							defaultInterpolatedStringHandler.AppendLiteral("', expected one of '");
							defaultInterpolatedStringHandler.AppendFormatted(string.Join<int>("', '", from p in possible_items.Keys
							orderby p
							select p));
							defaultInterpolatedStringHandler.AppendLiteral("'.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						else
						{
							itemList.Clear();
						}
					}
				}
				MovieData movieData2 = movieData;
				bool flag2;
				if (movieData2 == null)
				{
					flag2 = false;
				}
				else
				{
					List<MovieCranePrizeData> cranePrizes = movieData2.CranePrizes;
					int? num = (cranePrizes != null) ? new int?(cranePrizes.Count) : null;
					int num2 = 0;
					flag2 = (num.GetValueOrDefault() > num2 & num != null);
				}
				if (flag2)
				{
					using (List<MovieCranePrizeData>.Enumerator enumerator2 = movieData.CranePrizes.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							MovieCranePrizeData prize = enumerator2.Current;
							if (prize.Condition == null || GameStateQuery.CheckConditions(prize.Condition, null, null, null, null, null, null))
							{
								List<Item> itemList2;
								if (!possible_items.TryGetValue(prize.Rarity, out itemList2))
								{
									IGameLogger log2 = Game1.log;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 3);
									defaultInterpolatedStringHandler.AppendLiteral("Movie '");
									defaultInterpolatedStringHandler.AppendFormatted(movieData.Id);
									defaultInterpolatedStringHandler.AppendLiteral("' has invalid rarity '");
									defaultInterpolatedStringHandler.AppendFormatted<int>(prize.Rarity);
									defaultInterpolatedStringHandler.AppendLiteral("', expected one of '");
									defaultInterpolatedStringHandler.AppendFormatted(string.Join<int>("', '", from p in possible_items.Keys
									orderby p
									select p));
									defaultInterpolatedStringHandler.AppendLiteral("'.");
									log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
								}
								else
								{
									ISpawnItemData prize9 = prize;
									GameLocation location = null;
									Farmer player = null;
									Random random = null;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 2);
									defaultInterpolatedStringHandler.AppendLiteral("movie '");
									defaultInterpolatedStringHandler.AppendFormatted(movieData.Id);
									defaultInterpolatedStringHandler.AppendLiteral("' > crane prize '");
									defaultInterpolatedStringHandler.AppendFormatted(prize.Id);
									defaultInterpolatedStringHandler.AppendLiteral("'");
									Item item = ItemQueryResolver.TryResolveRandomItem(prize9, new ItemQueryContext(location, player, random, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, null, null, delegate(string query, string error)
									{
										IGameLogger log3 = Game1.log;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(58, 4);
										defaultInterpolatedStringHandler2.AppendLiteral("Movie '");
										defaultInterpolatedStringHandler2.AppendFormatted(movieData.Id);
										defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
										defaultInterpolatedStringHandler2.AppendFormatted(query);
										defaultInterpolatedStringHandler2.AppendLiteral("' for crane prize '");
										defaultInterpolatedStringHandler2.AppendFormatted(prize.Id);
										defaultInterpolatedStringHandler2.AppendLiteral("': ");
										defaultInterpolatedStringHandler2.AppendFormatted(error);
										log3.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
									});
									if (item != null)
									{
										itemList2.Add(item);
									}
								}
							}
						}
					}
				}
				for (int j = 0; j < this.prizeMap.Length; j++)
				{
					if (this.prizeMap[j] != 0)
					{
						int x2 = j % level_width + 1;
						int y2 = j / level_width + 3;
						Item item2 = null;
						int prize_rarity = j;
						while (prize_rarity > 0 && item2 == null)
						{
							int index = this.prizeMap[j];
							if (index - 1 <= 2)
							{
								item2 = Game1.random.ChooseFrom(possible_items[index]);
							}
							prize_rarity--;
						}
						CraneGame.Prize prize2 = new CraneGame.Prize(this._game, item2);
						prize2.position.X = (float)(x2 * 16 + 8);
						prize2.position.Y = (float)(y2 * 16 + 8);
					}
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					Item item3 = null;
					Vector2 prizePosition = new Vector2(0f, 4f);
					switch (Game1.random.Next(4))
					{
					case 0:
						item3 = ItemRegistry.Create("(O)107", 1, 0, false);
						break;
					case 1:
						item3 = ItemRegistry.Create("(O)749", 5, 0, false);
						break;
					case 2:
						item3 = ItemRegistry.Create("(O)688", 5, 0, false);
						break;
					case 3:
						item3 = ItemRegistry.Create("(O)288", 5, 0, false);
						break;
					}
					CraneGame.Prize prize3 = new CraneGame.Prize(this._game, item3);
					prize3.position.X = prizePosition.X * 16f + 30f;
					prize3.position.Y = prizePosition.Y * 16f + 32f;
				}
				else if (Game1.random.NextDouble() < 0.2)
				{
					CraneGame.Prize prize4 = new CraneGame.Prize(this._game, ItemRegistry.Create("(O)809", 1, 0, false));
					prize4.position.X = 160f;
					prize4.position.Y = 58f;
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					CraneGame.Prize prize5 = new CraneGame.Prize(this._game, ItemRegistry.Create("(F)986", 1, 0, false));
					prize5.position = new Vector2(263f, 56f);
					prize5.zPosition = 0f;
					CraneGame.Prize prize6 = new CraneGame.Prize(this._game, ItemRegistry.Create("(F)986", 1, 0, false));
					prize6.position = new Vector2(215f, 56f);
					prize6.zPosition = 0f;
					return;
				}
				CraneGame.Prize prize7 = new CraneGame.Prize(this._game, ItemRegistry.Create("(F)989", 1, 0, false));
				prize7.position = new Vector2(263f, 56f);
				prize7.zPosition = 0f;
				CraneGame.Prize prize8 = new CraneGame.Prize(this._game, ItemRegistry.Create("(F)989", 1, 0, false));
				prize8.position = new Vector2(215f, 56f);
				prize8.zPosition = 0f;
			}

			// Token: 0x060041FE RID: 16894 RVA: 0x0030DFA0 File Offset: 0x0030C1A0
			public CraneGame.GameLogic.GameStates GetCurrentState()
			{
				return this._currentState;
			}

			// Token: 0x060041FF RID: 16895 RVA: 0x0030DFA8 File Offset: 0x0030C1A8
			public override void Update(GameTime time)
			{
				float desired_joystick_rotation = 0f;
				foreach (CraneGame.Shadow shadow in this._game.GetObjectsOfType<CraneGame.Shadow>())
				{
					if (this.prizeChute.Contains(new Point((int)shadow.position.X, (int)shadow.position.Y)))
					{
						shadow.visible = false;
					}
					else
					{
						shadow.visible = true;
					}
				}
				int displayed_time = this.currentTimer / 60;
				if (this._currentState == CraneGame.GameLogic.GameStates.Setup)
				{
					this.creditsDisplay.SetSpriteFromIndex(70);
				}
				else
				{
					this.creditsDisplay.SetSpriteFromIndex(70 + this.lives);
				}
				this.timeDisplay1.SetSpriteFromIndex(70 + displayed_time / 10);
				this.timeDisplay2.SetSpriteFromIndex(70 + displayed_time % 10);
				if (this.currentTimer < 0)
				{
					this.timeDisplay1.SetSpriteFromIndex(80);
					this.timeDisplay2.SetSpriteFromIndex(81);
				}
				switch (this._currentState)
				{
				case CraneGame.GameLogic.GameStates.Setup:
				{
					if (!this._game.music.IsPlaying)
					{
						this._game.music.Play();
					}
					this._claw.openAngle = 40f;
					bool is_something_busy = false;
					using (List<CraneGame.Prize>.Enumerator enumerator2 = this._game.GetObjectsOfType<CraneGame.Prize>().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (!enumerator2.Current.CanBeGrabbed())
							{
								is_something_busy = true;
								break;
							}
						}
					}
					if (!is_something_busy)
					{
						if (this._stateTimer >= 10)
						{
							this.SetState(CraneGame.GameLogic.GameStates.Idle);
						}
					}
					else
					{
						this._stateTimer = 0;
					}
					break;
				}
				case CraneGame.GameLogic.GameStates.Idle:
					if (!this._game.music.IsPlaying)
					{
						this._game.music.Play();
					}
					if (this._game.fastMusic.IsPlaying)
					{
						this._game.fastMusic.Stop(AudioStopOptions.Immediate);
						this._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					}
					this.currentTimer = 900;
					this.moveRightIndicator.visible = (Game1.ticks / 20 % 2 == 0);
					if (this._game.IsButtonPressed(CraneGame.GameButtons.Tool) || this._game.IsButtonPressed(CraneGame.GameButtons.Action) || this._game.IsButtonPressed(CraneGame.GameButtons.Right))
					{
						Game1.playSound("bigSelect", null);
						this.SetState(CraneGame.GameLogic.GameStates.MoveClawRight);
					}
					break;
				case CraneGame.GameLogic.GameStates.MoveClawRight:
					desired_joystick_rotation = 15f;
					if (this._stateTimer < 15)
					{
						if (!this._game.IsButtonDown(CraneGame.GameButtons.Tool) && !this._game.IsButtonDown(CraneGame.GameButtons.Action) && !this._game.IsButtonDown(CraneGame.GameButtons.Right))
						{
							Game1.playSound("bigDeSelect", null);
							this.SetState(CraneGame.GameLogic.GameStates.Idle);
							return;
						}
					}
					else
					{
						if (this._game.craneSound == null || !this._game.craneSound.IsPlaying)
						{
							Game1.playSound("crane", out this._game.craneSound);
						}
						this.currentTimer--;
						if (this.currentTimer <= 0)
						{
							this.SetState(CraneGame.GameLogic.GameStates.ClawDescend);
							this.currentTimer = -1;
							if (this._game.craneSound != null && !this._game.craneSound.IsStopped)
							{
								this._game.craneSound.Stop(AudioStopOptions.Immediate);
							}
						}
						this.moveRightIndicator.visible = true;
						if (this._stateTimer > 10)
						{
							if (this._stateTimer == 11)
							{
								this._claw.ApplyDrawEffect(new CraneGame.ShakeEffect(1f, 1f, 10));
								this._claw.ApplyDrawEffect(new CraneGame.SwayEffect(2f, 10f, 20));
								this._claw.ApplyDrawEffectToArms(new CraneGame.SwayEffect(15f, 4f, 50));
							}
							if (!this._game.IsButtonDown(CraneGame.GameButtons.Tool) && !this._game.IsButtonDown(CraneGame.GameButtons.Right) && !this._game.IsButtonDown(CraneGame.GameButtons.Action))
							{
								Game1.playSound("bigDeSelect", null);
								this._claw.ApplyDrawEffect(new CraneGame.SwayEffect(2f, 10f, 20));
								this._claw.ApplyDrawEffectToArms(new CraneGame.SwayEffect(15f, 4f, 100));
								this.SetState(CraneGame.GameLogic.GameStates.WaitForMoveDown);
								this.moveRightIndicator.visible = false;
								if (this._game.craneSound != null && !this._game.craneSound.IsStopped)
								{
									this._game.craneSound.Stop(AudioStopOptions.Immediate);
								}
							}
							else
							{
								this._claw.Move(0.5f, 0f);
								if (this._claw.GetBounds().Right >= this.playArea.Right)
								{
									this._claw.Move(-0.5f, 0f);
								}
							}
						}
					}
					break;
				case CraneGame.GameLogic.GameStates.WaitForMoveDown:
					this.currentTimer--;
					if (this.currentTimer <= 0)
					{
						this.SetState(CraneGame.GameLogic.GameStates.ClawDescend);
						this.currentTimer = -1;
					}
					this.moveDownIndicator.visible = (Game1.ticks / 20 % 2 == 0);
					if (this._game.IsButtonPressed(CraneGame.GameButtons.Tool) || this._game.IsButtonPressed(CraneGame.GameButtons.Down) || this._game.IsButtonPressed(CraneGame.GameButtons.Action))
					{
						Game1.playSound("bigSelect", null);
						this.SetState(CraneGame.GameLogic.GameStates.MoveClawDown);
					}
					break;
				case CraneGame.GameLogic.GameStates.MoveClawDown:
					if (this._game.craneSound == null || !this._game.craneSound.IsPlaying)
					{
						Game1.playSound("crane", out this._game.craneSound);
					}
					this.currentTimer--;
					if (this.currentTimer <= 0)
					{
						this.SetState(CraneGame.GameLogic.GameStates.ClawDescend);
						this.currentTimer = -1;
						if (this._game.craneSound != null && !this._game.craneSound.IsStopped)
						{
							this._game.craneSound.Stop(AudioStopOptions.Immediate);
						}
					}
					desired_joystick_rotation = -5f;
					this.moveDownIndicator.visible = true;
					if (this._stateTimer > 10)
					{
						if (this._stateTimer == 11)
						{
							this._claw.ApplyDrawEffect(new CraneGame.ShakeEffect(1f, 1f, 10));
							this._claw.ApplyDrawEffect(new CraneGame.SwayEffect(2f, 10f, 20));
							this._claw.ApplyDrawEffectToArms(new CraneGame.SwayEffect(15f, 4f, 50));
						}
						if (!this._game.IsButtonDown(CraneGame.GameButtons.Tool) && !this._game.IsButtonDown(CraneGame.GameButtons.Down) && !this._game.IsButtonDown(CraneGame.GameButtons.Action))
						{
							Game1.playSound("bigDeSelect", null);
							this._claw.ApplyDrawEffect(new CraneGame.SwayEffect(2f, 10f, 20));
							this._claw.ApplyDrawEffectToArms(new CraneGame.SwayEffect(15f, 4f, 100));
							this.moveDownIndicator.visible = false;
							this.SetState(CraneGame.GameLogic.GameStates.ClawDescend);
							if (this._game.craneSound != null && !this._game.craneSound.IsStopped)
							{
								this._game.craneSound.Stop(AudioStopOptions.Immediate);
							}
						}
						else
						{
							this._claw.Move(0f, 0.5f);
							if (this._claw.GetBounds().Bottom >= this.playArea.Bottom)
							{
								this._claw.Move(0f, -0.5f);
							}
						}
					}
					break;
				case CraneGame.GameLogic.GameStates.ClawDescend:
					if (this._claw.openAngle < 40f)
					{
						this._claw.openAngle += 1.5f;
						this._stateTimer = 0;
					}
					else if (this._stateTimer > 30)
					{
						if (this._game.craneSound != null && this._game.craneSound.IsPlaying)
						{
							Game1.sounds.SetPitch(this._game.craneSound, 2000f, true);
						}
						else
						{
							Game1.playSound("crane", 2000, out this._game.craneSound);
						}
						if (this._claw.zPosition > 0f)
						{
							this._claw.zPosition -= 0.5f;
							if (this._claw.zPosition <= 0f)
							{
								this._claw.zPosition = 0f;
								this.SetState(CraneGame.GameLogic.GameStates.ClawAscend);
								if (this._game.craneSound != null && !this._game.craneSound.IsStopped)
								{
									this._game.craneSound.Stop(AudioStopOptions.Immediate);
								}
							}
						}
					}
					break;
				case CraneGame.GameLogic.GameStates.ClawAscend:
					if (this._claw.openAngle > 0f && this._claw.GetGrabbedPrize() == null)
					{
						this._claw.openAngle -= 1f;
						if (this._claw.openAngle == 15f)
						{
							this._claw.GrabObject();
							if (this._claw.GetGrabbedPrize() != null)
							{
								Game1.playSound("FishHit", null);
								this.sunShockedFace.ApplyDrawEffect(new CraneGame.ShakeEffect(1f, 1f, 5));
								this._game.freezeFrames = 60;
								if (this._game.music.IsPlaying)
								{
									this._game.music.Stop(AudioStopOptions.Immediate);
									this._game.music = Game1.soundBank.GetCue("crane_game");
								}
							}
						}
						else if (this._claw.openAngle == 0f && this._claw.GetGrabbedPrize() == null)
						{
							if (this.lives == 1)
							{
								this._game.music.Stop(AudioStopOptions.Immediate);
								Game1.playSound("fishEscape", null);
							}
							else
							{
								Game1.playSound("stoneStep", null);
							}
						}
						this._stateTimer = 0;
					}
					else
					{
						if (this._claw.GetGrabbedPrize() != null)
						{
							if (!this._game.fastMusic.IsPlaying)
							{
								this._game.fastMusic.Play();
							}
						}
						else if (this._game.fastMusic.IsPlaying)
						{
							this._game.fastMusic.Stop(AudioStopOptions.AsAuthored);
							this._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
						}
						if (this._claw.zPosition < 50f)
						{
							this._claw.zPosition += 0.5f;
							if (this._claw.zPosition >= 50f)
							{
								this._claw.zPosition = 50f;
								this.SetState(CraneGame.GameLogic.GameStates.ClawReturn);
								if (this._claw.GetGrabbedPrize() == null && this.lives == 1)
								{
									this.SetState(CraneGame.GameLogic.GameStates.EndGame);
								}
							}
						}
						this._claw.CheckDropPrize();
					}
					break;
				case CraneGame.GameLogic.GameStates.ClawReturn:
					if (this._claw.GetGrabbedPrize() != null)
					{
						if (!this._game.fastMusic.IsPlaying)
						{
							this._game.fastMusic.Play();
						}
					}
					else if (this._game.fastMusic.IsPlaying)
					{
						this._game.fastMusic.Stop(AudioStopOptions.AsAuthored);
						this._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					}
					if (this._stateTimer > 10)
					{
						if (this._claw.position.Equals(this._dropPosition))
						{
							this.SetState(CraneGame.GameLogic.GameStates.ClawRelease);
						}
						else
						{
							float move_speed = 0.5f;
							if (this._claw.GetGrabbedPrize() == null)
							{
								move_speed = 0.75f;
							}
							if (this._claw.position.X != this._dropPosition.X)
							{
								this._claw.position.X = Utility.MoveTowards(this._claw.position.X, this._dropPosition.X, move_speed);
							}
							if (this._claw.position.X != this._dropPosition.Y)
							{
								this._claw.position.Y = Utility.MoveTowards(this._claw.position.Y, this._dropPosition.Y, move_speed);
							}
						}
					}
					this._claw.CheckDropPrize();
					break;
				case CraneGame.GameLogic.GameStates.ClawRelease:
				{
					bool clawHadPrize = this._claw.GetGrabbedPrize() != null;
					if (this._stateTimer > 10)
					{
						this._claw.ReleaseGrabbedObject();
						if (this._claw.openAngle < 40f)
						{
							CraneGame.Claw claw = this._claw;
							float openAngle = claw.openAngle;
							claw.openAngle = openAngle + 1f;
						}
						else
						{
							this.SetState(CraneGame.GameLogic.GameStates.ClawReset);
							if (!clawHadPrize)
							{
								Game1.playSound("button1", null);
								this._claw.ApplyDrawEffect(new CraneGame.ShakeEffect(1f, 1f, 10));
							}
						}
					}
					break;
				}
				case CraneGame.GameLogic.GameStates.ClawReset:
					if (this._stateTimer > 50)
					{
						if (this._claw.position.Equals(this._startPosition))
						{
							this.lives--;
							if (this.lives <= 0)
							{
								this.SetState(CraneGame.GameLogic.GameStates.EndGame);
							}
							else
							{
								this.SetState(CraneGame.GameLogic.GameStates.Idle);
							}
						}
						else
						{
							float move_speed2 = 0.5f;
							if (this._claw.position.X != this._startPosition.X)
							{
								this._claw.position.X = Utility.MoveTowards(this._claw.position.X, this._startPosition.X, move_speed2);
							}
							if (this._claw.position.X != this._startPosition.Y)
							{
								this._claw.position.Y = Utility.MoveTowards(this._claw.position.Y, this._startPosition.Y, move_speed2);
							}
						}
					}
					break;
				case CraneGame.GameLogic.GameStates.EndGame:
				{
					if (this._game.music.IsPlaying)
					{
						this._game.music.Stop(AudioStopOptions.Immediate);
					}
					if (this._game.fastMusic.IsPlaying)
					{
						this._game.fastMusic.Stop(AudioStopOptions.Immediate);
					}
					bool is_something_busy2 = false;
					using (List<CraneGame.Prize>.Enumerator enumerator2 = this._game.GetObjectsOfType<CraneGame.Prize>().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (!enumerator2.Current.CanBeGrabbed())
							{
								is_something_busy2 = true;
								break;
							}
						}
					}
					if (!is_something_busy2 && this._stateTimer >= 20)
					{
						if (this.collectedItems.Count > 0)
						{
							List<Item> items = new List<Item>();
							foreach (Item item in this.collectedItems)
							{
								items.Add(item.getOne());
							}
							Game1.activeClickableMenu = new ItemGrabMenu(items, false, true, null, null, "Rewards", null, false, false, false, false, false, 0, null, -1, this._game, ItemExitBehavior.ReturnToPlayer, false);
						}
						this._game.Quit();
					}
					break;
				}
				}
				this.sunShockedFace.visible = (this._claw.GetGrabbedPrize() != null);
				this.joystick.rotation = Utility.MoveTowards(this.joystick.rotation, desired_joystick_rotation, 2f);
				this._stateTimer++;
			}

			// Token: 0x06004200 RID: 16896 RVA: 0x0030EFA8 File Offset: 0x0030D1A8
			public override void Draw(SpriteBatch b, float layer_depth)
			{
			}

			// Token: 0x06004201 RID: 16897 RVA: 0x0030EFAA File Offset: 0x0030D1AA
			public void SetState(CraneGame.GameLogic.GameStates new_state)
			{
				this._currentState = new_state;
				this._stateTimer = 0;
			}

			// Token: 0x04002C57 RID: 11351
			public List<Item> collectedItems;

			// Token: 0x04002C58 RID: 11352
			public const int CLAW_HEIGHT = 50;

			// Token: 0x04002C59 RID: 11353
			protected CraneGame.Claw _claw;

			// Token: 0x04002C5A RID: 11354
			public int maxLives = 3;

			// Token: 0x04002C5B RID: 11355
			public int lives = 3;

			// Token: 0x04002C5C RID: 11356
			public Vector2 _startPosition = new Vector2(24f, 56f);

			// Token: 0x04002C5D RID: 11357
			public Vector2 _dropPosition = new Vector2(32f, 56f);

			// Token: 0x04002C5E RID: 11358
			public Rectangle playArea = new Rectangle(16, 48, 272, 64);

			// Token: 0x04002C5F RID: 11359
			public Rectangle prizeChute = new Rectangle(16, 48, 32, 32);

			// Token: 0x04002C60 RID: 11360
			protected CraneGame.GameLogic.GameStates _currentState;

			// Token: 0x04002C61 RID: 11361
			protected int _stateTimer;

			// Token: 0x04002C62 RID: 11362
			public CraneGame.CraneGameObject moveRightIndicator;

			// Token: 0x04002C63 RID: 11363
			public CraneGame.CraneGameObject moveDownIndicator;

			// Token: 0x04002C64 RID: 11364
			public CraneGame.CraneGameObject creditsDisplay;

			// Token: 0x04002C65 RID: 11365
			public CraneGame.CraneGameObject timeDisplay1;

			// Token: 0x04002C66 RID: 11366
			public CraneGame.CraneGameObject timeDisplay2;

			// Token: 0x04002C67 RID: 11367
			public CraneGame.CraneGameObject sunShockedFace;

			// Token: 0x04002C68 RID: 11368
			public int currentTimer;

			// Token: 0x04002C69 RID: 11369
			public CraneGame.CraneGameObject joystick;

			// Token: 0x04002C6A RID: 11370
			public int[] conveyerBeltTiles = new int[]
			{
				0,
				0,
				0,
				0,
				7,
				6,
				6,
				9,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				8,
				0,
				0,
				2,
				0,
				0,
				0,
				7,
				6,
				6,
				6,
				6,
				9,
				0,
				0,
				0,
				0,
				8,
				0,
				0,
				2,
				0,
				0,
				0,
				8,
				0,
				0,
				0,
				0,
				2,
				0,
				0,
				0,
				0,
				1,
				4,
				4,
				3,
				0,
				0,
				0,
				1,
				4,
				4,
				4,
				4,
				3
			};

			// Token: 0x04002C6B RID: 11371
			public int[] prizeMap = new int[]
			{
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				1,
				0,
				2,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				1,
				0,
				2,
				0,
				3
			};

			// Token: 0x0200074D RID: 1869
			[XmlType("CraneGame.GameStates")]
			public enum GameStates
			{
				// Token: 0x04003195 RID: 12693
				Setup,
				// Token: 0x04003196 RID: 12694
				Idle,
				// Token: 0x04003197 RID: 12695
				MoveClawRight,
				// Token: 0x04003198 RID: 12696
				WaitForMoveDown,
				// Token: 0x04003199 RID: 12697
				MoveClawDown,
				// Token: 0x0400319A RID: 12698
				ClawDescend,
				// Token: 0x0400319B RID: 12699
				ClawAscend,
				// Token: 0x0400319C RID: 12700
				ClawReturn,
				// Token: 0x0400319D RID: 12701
				ClawRelease,
				// Token: 0x0400319E RID: 12702
				ClawReset,
				// Token: 0x0400319F RID: 12703
				EndGame
			}
		}

		// Token: 0x0200059D RID: 1437
		public class Trampoline : CraneGame.CraneGameObject
		{
			// Token: 0x06004202 RID: 16898 RVA: 0x0030EFBC File Offset: 0x0030D1BC
			public Trampoline(CraneGame game, int x, int y) : base(game)
			{
				base.SetSpriteFromIndex(30);
				this.spriteRect.Width = 32;
				this.spriteRect.Height = 32;
				this.spriteAnchor.X = 15f;
				this.spriteAnchor.Y = 15f;
				this.position.X = (float)x;
				this.position.Y = (float)y;
			}
		}

		// Token: 0x0200059E RID: 1438
		public class Shadow : CraneGame.CraneGameObject
		{
			// Token: 0x06004203 RID: 16899 RVA: 0x0030F02C File Offset: 0x0030D22C
			public Shadow(CraneGame game, CraneGame.CraneGameObject target) : base(game)
			{
				base.SetSpriteFromIndex(2);
				this.layerDepth = 900f;
				this._target = target;
			}

			// Token: 0x06004204 RID: 16900 RVA: 0x0030F050 File Offset: 0x0030D250
			public override void Update(GameTime time)
			{
				if (this._target != null)
				{
					this.position = this._target.position;
				}
				CraneGame.Prize prize = this._target as CraneGame.Prize;
				if (prize != null && prize.grabbed)
				{
					this.visible = false;
				}
				if (this._target.IsDestroyed())
				{
					this.Destroy();
					return;
				}
				this.color.A = (byte)(Math.Min(1f, this._target.zPosition / 50f) * 255f);
				this.scale = Utility.Lerp(1f, 0.5f, Math.Min(this._target.zPosition / 100f, 1f)) * new Vector2(1f, 1f);
			}

			// Token: 0x04002C6C RID: 11372
			public CraneGame.CraneGameObject _target;
		}

		// Token: 0x0200059F RID: 1439
		public class Claw : CraneGame.CraneGameObject
		{
			// Token: 0x170004E8 RID: 1256
			// (get) Token: 0x06004205 RID: 16901 RVA: 0x0030F11A File Offset: 0x0030D31A
			// (set) Token: 0x06004206 RID: 16902 RVA: 0x0030F127 File Offset: 0x0030D327
			public float openAngle
			{
				get
				{
					return this._leftArm.rotation;
				}
				set
				{
					this._leftArm.rotation = value;
				}
			}

			// Token: 0x06004207 RID: 16903 RVA: 0x0030F138 File Offset: 0x0030D338
			public Claw(CraneGame game) : base(game)
			{
				base.SetSpriteFromIndex(0);
				this.spriteAnchor = new Vector2(8f, 24f);
				this._leftArm = new CraneGame.CraneGameObject(game);
				this._leftArm.SetSpriteFromIndex(1);
				this._leftArm.spriteAnchor = new Vector2(16f, 0f);
				this._rightArm = new CraneGame.CraneGameObject(game);
				this._rightArm.SetSpriteFromIndex(1);
				this._rightArm.flipX = true;
				this._rightArm.spriteAnchor = new Vector2(0f, 0f);
				new CraneGame.Shadow(this._game, this);
			}

			// Token: 0x06004208 RID: 16904 RVA: 0x0030F1E8 File Offset: 0x0030D3E8
			public void CheckDropPrize()
			{
				if (this._grabbedPrize == null)
				{
					return;
				}
				this._nextDropCheckTimer--;
				if (this._nextDropCheckTimer <= 0)
				{
					float drop_chance = this._prizePositionOffset.Length() * 0.1f;
					drop_chance += this.zPosition * 0.001f;
					if (this._grabbedPrize.isLargeItem)
					{
						drop_chance += 0.1f;
					}
					double roll = Game1.random.NextDouble();
					if (roll < (double)drop_chance)
					{
						this._dropChances--;
						if (this._dropChances <= 0)
						{
							Game1.playSound("fishEscape", null);
							this.ReleaseGrabbedObject();
						}
						else
						{
							Game1.playSound("bob", null);
							this._grabbedPrize.ApplyDrawEffect(new CraneGame.ShakeEffect(2f, 2f, 50));
							this._grabbedPrize.rotation += (float)Game1.random.NextDouble() * 10f;
						}
					}
					else if (roll < (double)drop_chance)
					{
						Game1.playSound("dwop", null);
						this._grabbedPrize.ApplyDrawEffect(new CraneGame.ShakeEffect(1f, 1f, 50));
					}
					this._nextDropCheckTimer = Game1.random.Next(50, 100);
				}
			}

			// Token: 0x06004209 RID: 16905 RVA: 0x0030F335 File Offset: 0x0030D535
			public void ApplyDrawEffectToArms(CraneGame.DrawEffect new_effect)
			{
				this._leftArm.ApplyDrawEffect(new_effect);
				this._rightArm.ApplyDrawEffect(new_effect);
			}

			// Token: 0x0600420A RID: 16906 RVA: 0x0030F34F File Offset: 0x0030D54F
			public void ReleaseGrabbedObject()
			{
				if (this._grabbedPrize != null)
				{
					this._grabbedPrize.grabbed = false;
					this._grabbedPrize.OnDrop();
					this._grabbedPrize = null;
				}
			}

			// Token: 0x0600420B RID: 16907 RVA: 0x0030F378 File Offset: 0x0030D578
			public void GrabObject()
			{
				CraneGame.Prize closest_prize = null;
				float closest_distance = 0f;
				foreach (CraneGame.Prize prize in this._game.GetObjectsAtPoint<CraneGame.Prize>(this.position, -1))
				{
					if (!prize.IsDestroyed() && prize.CanBeGrabbed())
					{
						float distance = (this.position - prize.position).LengthSquared();
						if (closest_prize == null || distance < closest_distance)
						{
							closest_distance = distance;
							closest_prize = prize;
						}
					}
				}
				if (closest_prize != null)
				{
					this._grabbedPrize = closest_prize;
					this._grabbedPrize.grabbed = true;
					this._prizePositionOffset = this._grabbedPrize.position - this.position;
					this._nextDropCheckTimer = Game1.random.Next(50, 100);
					this._dropChances = 3;
					Game1.playSound("pickUpItem", null);
					this._grabTime = 0;
					this._grabbedPrize.ApplyDrawEffect(new CraneGame.StretchEffect(0.95f, 1.1f, 10));
					this._grabbedPrize.ApplyDrawEffect(new CraneGame.ShakeEffect(1f, 1f, 20));
				}
			}

			// Token: 0x0600420C RID: 16908 RVA: 0x0030F4B8 File Offset: 0x0030D6B8
			public CraneGame.Prize GetGrabbedPrize()
			{
				return this._grabbedPrize;
			}

			// Token: 0x0600420D RID: 16909 RVA: 0x0030F4C0 File Offset: 0x0030D6C0
			public override void Update(GameTime time)
			{
				this._leftArm.position = this.position + new Vector2(0f, -16f);
				this._rightArm.position = this.position + new Vector2(0f, -16f);
				this._rightArm.rotation = -this._leftArm.rotation;
				this._leftArm.layerDepth = (this._rightArm.layerDepth = base.GetRendererLayerDepth() + 0.01f);
				this._leftArm.zPosition = (this._rightArm.zPosition = this.zPosition);
				if (this._grabbedPrize != null)
				{
					this._grabbedPrize.position = this.position + this._prizePositionOffset * Utility.Lerp(1f, 0.25f, Math.Min(1f, (float)this._grabTime / 200f));
					this._grabbedPrize.zPosition = this.zPosition + this._grabbedPrize.GetRestingZPosition();
				}
				this._grabTime++;
			}

			// Token: 0x0600420E RID: 16910 RVA: 0x0030F5EC File Offset: 0x0030D7EC
			public override void Destroy()
			{
				this._leftArm.Destroy();
				this._rightArm.Destroy();
				base.Destroy();
			}

			// Token: 0x04002C6D RID: 11373
			protected CraneGame.CraneGameObject _leftArm;

			// Token: 0x04002C6E RID: 11374
			protected CraneGame.CraneGameObject _rightArm;

			// Token: 0x04002C6F RID: 11375
			protected CraneGame.Prize _grabbedPrize;

			// Token: 0x04002C70 RID: 11376
			protected Vector2 _prizePositionOffset;

			// Token: 0x04002C71 RID: 11377
			protected int _nextDropCheckTimer;

			// Token: 0x04002C72 RID: 11378
			protected int _dropChances;

			// Token: 0x04002C73 RID: 11379
			protected int _grabTime;
		}

		// Token: 0x020005A0 RID: 1440
		public class ConveyerBelt : CraneGame.CraneGameObject
		{
			// Token: 0x0600420F RID: 16911 RVA: 0x0030F60A File Offset: 0x0030D80A
			public int GetDirection()
			{
				return this._direction;
			}

			// Token: 0x06004210 RID: 16912 RVA: 0x0030F614 File Offset: 0x0030D814
			public ConveyerBelt(CraneGame game, int x, int y, int direction) : base(game)
			{
				this.position.X = (float)(x * 16);
				this.position.Y = (float)(y * 16);
				this._direction = direction;
				this.spriteAnchor = Vector2.Zero;
				this.layerDepth = 1000f;
				switch (this._direction)
				{
				case 0:
					base.SetSpriteFromIndex(5);
					break;
				case 1:
					base.SetSpriteFromIndex(20);
					break;
				case 2:
					base.SetSpriteFromIndex(10);
					break;
				case 3:
					base.SetSpriteFromIndex(15);
					break;
				}
				this._spriteStartPosition = new Vector2((float)this.spriteRect.X, (float)this.spriteRect.Y);
			}

			// Token: 0x06004211 RID: 16913 RVA: 0x0030F6CD File Offset: 0x0030D8CD
			public void SetSpriteFromCorner(int x, int y)
			{
				this.spriteRect.X = x;
				this.spriteRect.Y = y;
				this._spriteStartPosition = new Vector2((float)this.spriteRect.X, (float)this.spriteRect.Y);
			}

			// Token: 0x06004212 RID: 16914 RVA: 0x0030F70C File Offset: 0x0030D90C
			public override void Update(GameTime time)
			{
				int ticks_per_frame = 4;
				int frame_count = 4;
				this.spriteRect.X = (int)this._spriteStartPosition.X + this._spriteOffset / ticks_per_frame * 16;
				this._spriteOffset++;
				if (this._spriteOffset >= (frame_count - 1) * ticks_per_frame)
				{
					this._spriteOffset = 0;
				}
			}

			// Token: 0x04002C74 RID: 11380
			protected int _direction;

			// Token: 0x04002C75 RID: 11381
			protected Vector2 _spriteStartPosition;

			// Token: 0x04002C76 RID: 11382
			protected int _spriteOffset;
		}

		// Token: 0x020005A1 RID: 1441
		public class Bush : CraneGame.CraneGameObject
		{
			// Token: 0x06004213 RID: 16915 RVA: 0x0030F764 File Offset: 0x0030D964
			public Bush(CraneGame game, int tile_index, int tile_width, int tile_height, int x, int y) : base(game)
			{
				base.SetSpriteFromIndex(tile_index);
				this.spriteRect.Width = tile_width * 16;
				this.spriteRect.Height = tile_height * 16;
				this.spriteAnchor.X = (float)this.spriteRect.Width / 2f;
				this.spriteAnchor.Y = (float)this.spriteRect.Height;
				if (tile_height > 16)
				{
					this.spriteAnchor.Y = this.spriteAnchor.Y - 8f;
				}
				else
				{
					this.spriteAnchor.Y = this.spriteAnchor.Y - 4f;
				}
				this.position.X = (float)x;
				this.position.Y = (float)y;
			}

			// Token: 0x06004214 RID: 16916 RVA: 0x0030F820 File Offset: 0x0030DA20
			public override void Update(GameTime time)
			{
				this.rotation = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 0.0024999999441206455 + (double)this.position.Y + (double)(this.position.X * 2f)) * 2f;
			}
		}

		// Token: 0x020005A2 RID: 1442
		public class Prize : CraneGame.CraneGameObject
		{
			// Token: 0x06004215 RID: 16917 RVA: 0x0030F877 File Offset: 0x0030DA77
			public float GetRestingZPosition()
			{
				return this._restingZPosition;
			}

			// Token: 0x06004216 RID: 16918 RVA: 0x0030F880 File Offset: 0x0030DA80
			public Prize(CraneGame game, Item item) : base(game)
			{
				base.SetSpriteFromIndex(3);
				this.spriteAnchor = new Vector2(8f, 12f);
				this._item = item;
				this._UpdateItemSprite();
				new CraneGame.Shadow(this._game, this);
			}

			// Token: 0x06004217 RID: 16919 RVA: 0x0030F8D5 File Offset: 0x0030DAD5
			public void OnDrop()
			{
				if (!this.isLargeItem)
				{
					this._angularSpeed = Utility.Lerp(-5f, 5f, (float)Game1.random.NextDouble());
					return;
				}
				this.rotation = 0f;
			}

			// Token: 0x06004218 RID: 16920 RVA: 0x0030F90C File Offset: 0x0030DB0C
			public void _UpdateItemSprite()
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._item.QualifiedItemId);
				this.texture = itemData.GetTexture();
				this.spriteRect = itemData.GetSourceRect(0, null);
				this.width = this.spriteRect.Width;
				this.height = this.spriteRect.Height;
				if (this.width > 16 || this.height > 16)
				{
					this.isLargeItem = true;
				}
				else
				{
					this.isLargeItem = false;
				}
				if (this.height <= 16)
				{
					this.spriteAnchor = new Vector2((float)(this.width / 2), (float)this.height - 4f);
				}
				else
				{
					this.spriteAnchor = new Vector2((float)(this.width / 2), (float)this.height - 8f);
				}
				this._restingZPosition = 0f;
			}

			// Token: 0x06004219 RID: 16921 RVA: 0x0030F9EB File Offset: 0x0030DBEB
			public bool CanBeGrabbed()
			{
				return !base.IsDestroyed() && !this._isBeingCollected && this.zPosition == this._restingZPosition;
			}

			// Token: 0x0600421A RID: 16922 RVA: 0x0030FA14 File Offset: 0x0030DC14
			public override void Update(GameTime time)
			{
				if (this._isBeingCollected)
				{
					Vector4 color_vector = this.color.ToVector4();
					color_vector.X = Utility.MoveTowards(color_vector.X, 0f, 0.05f);
					color_vector.Y = Utility.MoveTowards(color_vector.Y, 0f, 0.05f);
					color_vector.Z = Utility.MoveTowards(color_vector.Z, 0f, 0.05f);
					color_vector.W = Utility.MoveTowards(color_vector.W, 0f, 0.05f);
					this.color = new Color(color_vector);
					this.scale.X = Utility.MoveTowards(this.scale.X, 0.5f, 0.05f);
					this.scale.Y = Utility.MoveTowards(this.scale.Y, 0.5f, 0.05f);
					if (color_vector.W == 0f)
					{
						Game1.playSound("Ship", null);
						this.Destroy();
					}
					this.position.Y = this.position.Y + 0.5f;
					return;
				}
				if (this.grabbed)
				{
					return;
				}
				if (this._velocity.X != 0f || this._velocity.Y != 0f)
				{
					this.position.X = this.position.X + this._velocity.X;
					if (!this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].playArea.Contains(new Point((int)this.position.X, (int)this.position.Y)))
					{
						this.position.X = this.position.X - this._velocity.X;
						this._velocity.X = this._velocity.X * -1f;
					}
					this.position.Y = this.position.Y + this._velocity.Y;
					if (!this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].playArea.Contains(new Point((int)this.position.X, (int)this.position.Y)))
					{
						this.position.Y = this.position.Y - this._velocity.Y;
						this._velocity.Y = this._velocity.Y * -1f;
					}
				}
				if (this.zPosition < this._restingZPosition)
				{
					this.zPosition = this._restingZPosition;
				}
				if (this.zPosition > this._restingZPosition || this._velocity != Vector2.Zero || this.gravity != 0f)
				{
					if (!this.isLargeItem)
					{
						this.rotation += this._angularSpeed;
					}
					this._conveyerBeltMove = Vector2.Zero;
					if (this.zPosition > this._restingZPosition)
					{
						this.gravity += 0.1f;
					}
					this.zPosition -= this.gravity;
					if (this.zPosition < this._restingZPosition)
					{
						this.zPosition = this._restingZPosition;
						if (this.gravity >= 0f)
						{
							if (!this.isLargeItem)
							{
								this._angularSpeed = Utility.Lerp(-10f, 10f, (float)Game1.random.NextDouble());
							}
							this.gravity = -this.gravity * 0.6f;
							if (this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].prizeChute.Contains(new Point((int)this.position.X, (int)this.position.Y)))
							{
								if (this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].GetCurrentState() != CraneGame.GameLogic.GameStates.Setup)
								{
									Game1.playSound("reward", null);
									this._isBeingCollected = true;
									this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].collectedItems.Add(this._item);
									return;
								}
								this.gravity = -2.5f;
								Vector2 offset = new Vector2((float)this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].playArea.Center.X, (float)this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].playArea.Center.Y) - new Vector2(this.position.X, this.position.Y);
								offset.Normalize();
								this._velocity = offset * Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
								return;
							}
							else
							{
								if (this._game.GetOverlaps<CraneGame.Trampoline>(this, 1).Count > 0)
								{
									CraneGame.Trampoline trampoline = this._game.GetOverlaps<CraneGame.Trampoline>(this, 1)[0];
									Game1.playSound("axchop", null);
									trampoline.ApplyDrawEffect(new CraneGame.StretchEffect(0.75f, 0.75f, 5));
									trampoline.ApplyDrawEffect(new CraneGame.ShakeEffect(2f, 2f, 10));
									base.ApplyDrawEffect(new CraneGame.ShakeEffect(2f, 2f, 10));
									this.gravity = -2.5f;
									Vector2 offset2 = new Vector2((float)this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].playArea.Center.X, (float)this._game.GetObjectsOfType<CraneGame.GameLogic>()[0].playArea.Center.Y) - new Vector2(this.position.X, this.position.Y);
									offset2.Normalize();
									this._velocity = offset2 * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
									return;
								}
								if (Math.Abs(this.gravity) < 1.5f)
								{
									this.rotation = 0f;
									this._velocity = Vector2.Zero;
									this.gravity = 0f;
									return;
								}
								bool bumped_object = false;
								foreach (CraneGame.Prize prize in this._game.GetOverlaps<CraneGame.Prize>(this, -1))
								{
									if (prize.gravity == 0f && prize.CanBeGrabbed())
									{
										Vector2 offset3 = this.position - prize.position;
										offset3.Normalize();
										this._velocity = offset3 * Utility.Lerp(0.25f, 1f, (float)Game1.random.NextDouble());
										if (!prize.isLargeItem || this.isLargeItem)
										{
											prize._velocity = -offset3 * Utility.Lerp(0.75f, 1.5f, (float)Game1.random.NextDouble());
											prize.gravity = this.gravity * 0.75f;
											prize.ApplyDrawEffect(new CraneGame.ShakeEffect(2f, 2f, 20));
										}
										bumped_object = true;
									}
								}
								base.ApplyDrawEffect(new CraneGame.ShakeEffect(2f, 2f, 20));
								if (!bumped_object)
								{
									float rad_angle = Utility.Lerp(0f, 6.2831855f, (float)Game1.random.NextDouble());
									this._velocity = new Vector2((float)Math.Sin((double)rad_angle), (float)Math.Cos((double)rad_angle)) * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
									return;
								}
							}
						}
					}
				}
				else if (this._conveyerBeltMove.X == 0f && this._conveyerBeltMove.Y == 0f)
				{
					List<CraneGame.ConveyerBelt> belts = this._game.GetObjectsAtPoint<CraneGame.ConveyerBelt>(this.position, 1);
					if (belts.Count > 0)
					{
						switch (belts[0].GetDirection())
						{
						case 0:
							this._conveyerBeltMove = new Vector2(0f, -16f);
							return;
						case 1:
							this._conveyerBeltMove = new Vector2(16f, 0f);
							return;
						case 2:
							this._conveyerBeltMove = new Vector2(0f, 16f);
							return;
						case 3:
							this._conveyerBeltMove = new Vector2(-16f, 0f);
							return;
						default:
							return;
						}
					}
				}
				else
				{
					float move_speed = 0.3f;
					if (this._conveyerBeltMove.X != 0f)
					{
						this.Move(move_speed * (float)Math.Sign(this._conveyerBeltMove.X), 0f);
						this._conveyerBeltMove.X = Utility.MoveTowards(this._conveyerBeltMove.X, 0f, move_speed);
					}
					if (this._conveyerBeltMove.Y != 0f)
					{
						this.Move(0f, move_speed * (float)Math.Sign(this._conveyerBeltMove.Y));
						this._conveyerBeltMove.Y = Utility.MoveTowards(this._conveyerBeltMove.Y, 0f, move_speed);
					}
				}
			}

			// Token: 0x04002C77 RID: 11383
			protected Vector2 _conveyerBeltMove;

			// Token: 0x04002C78 RID: 11384
			public bool grabbed;

			// Token: 0x04002C79 RID: 11385
			public float gravity;

			// Token: 0x04002C7A RID: 11386
			protected Vector2 _velocity = Vector2.Zero;

			// Token: 0x04002C7B RID: 11387
			protected Item _item;

			// Token: 0x04002C7C RID: 11388
			protected float _restingZPosition;

			// Token: 0x04002C7D RID: 11389
			protected float _angularSpeed;

			// Token: 0x04002C7E RID: 11390
			protected bool _isBeingCollected;

			// Token: 0x04002C7F RID: 11391
			public bool isLargeItem;
		}

		// Token: 0x020005A3 RID: 1443
		public class CraneGameObject
		{
			// Token: 0x0600421B RID: 16923 RVA: 0x00310310 File Offset: 0x0030E510
			public CraneGameObject(CraneGame game)
			{
				this._game = game;
				this.texture = this._game.spriteSheet;
				this.spriteRect = new Rectangle(0, 0, 16, 16);
				this.spriteAnchor = new Vector2(8f, 8f);
				this.drawEffects = new List<CraneGame.DrawEffect>();
				this._game.RegisterGameObject(this);
			}

			// Token: 0x0600421C RID: 16924 RVA: 0x003103C5 File Offset: 0x0030E5C5
			public void SetSpriteFromIndex(int index = 0)
			{
				this.spriteRect.X = 304 + index % 5 * 16;
				this.spriteRect.Y = index / 5 * 16;
			}

			// Token: 0x0600421D RID: 16925 RVA: 0x003103EF File Offset: 0x0030E5EF
			public bool IsDestroyed()
			{
				return this._destroyed;
			}

			// Token: 0x0600421E RID: 16926 RVA: 0x003103F7 File Offset: 0x0030E5F7
			public virtual void Destroy()
			{
				this._destroyed = true;
				this._game.UnregisterGameObject(this);
			}

			// Token: 0x0600421F RID: 16927 RVA: 0x0031040C File Offset: 0x0030E60C
			public virtual void Move(float x, float y)
			{
				this.position.X = this.position.X + x;
				this.position.Y = this.position.Y + y;
			}

			// Token: 0x06004220 RID: 16928 RVA: 0x00310430 File Offset: 0x0030E630
			public Rectangle GetBounds()
			{
				return new Rectangle((int)(this.position.X - this.spriteAnchor.X), (int)(this.position.Y - this.spriteAnchor.Y), this.width, this.height);
			}

			// Token: 0x06004221 RID: 16929 RVA: 0x0031047E File Offset: 0x0030E67E
			public virtual void Update(GameTime time)
			{
			}

			// Token: 0x06004222 RID: 16930 RVA: 0x00310480 File Offset: 0x0030E680
			public float GetRendererLayerDepth()
			{
				float layer_depth = this.layerDepth;
				if (layer_depth < 0f)
				{
					layer_depth = (float)this._game.gameHeight - this.position.Y;
				}
				return layer_depth;
			}

			// Token: 0x06004223 RID: 16931 RVA: 0x003104B6 File Offset: 0x0030E6B6
			public void ApplyDrawEffect(CraneGame.DrawEffect new_effect)
			{
				this.drawEffects.Add(new_effect);
			}

			// Token: 0x06004224 RID: 16932 RVA: 0x003104C4 File Offset: 0x0030E6C4
			public virtual void Draw(SpriteBatch b, float layer_depth)
			{
				if (!this.visible)
				{
					return;
				}
				SpriteEffects effects = SpriteEffects.None;
				if (this.flipX)
				{
					effects |= SpriteEffects.FlipHorizontally;
				}
				if (this.flipY)
				{
					effects |= SpriteEffects.FlipVertically;
				}
				float drawn_rotation = this.rotation;
				Vector2 drawn_scale = this.scale;
				Vector2 drawn_position = this.position - new Vector2(0f, this.zPosition);
				this.drawEffects.RemoveAll((CraneGame.DrawEffect effect) => effect.Apply(ref drawn_position, ref drawn_rotation, ref drawn_scale));
				b.Draw(this.texture, this._game.upperLeft + drawn_position * 4f, new Rectangle?(this.spriteRect), this.color, drawn_rotation * 0.017453292f, this.spriteAnchor, 4f * drawn_scale, effects, layer_depth);
			}

			// Token: 0x04002C80 RID: 11392
			protected CraneGame _game;

			// Token: 0x04002C81 RID: 11393
			public Vector2 position = Vector2.Zero;

			// Token: 0x04002C82 RID: 11394
			public float rotation;

			// Token: 0x04002C83 RID: 11395
			public Vector2 scale = new Vector2(1f, 1f);

			// Token: 0x04002C84 RID: 11396
			public bool flipX;

			// Token: 0x04002C85 RID: 11397
			public bool flipY;

			// Token: 0x04002C86 RID: 11398
			public Rectangle spriteRect;

			// Token: 0x04002C87 RID: 11399
			public Texture2D texture;

			// Token: 0x04002C88 RID: 11400
			public Vector2 spriteAnchor;

			// Token: 0x04002C89 RID: 11401
			public Color color = Color.White;

			// Token: 0x04002C8A RID: 11402
			public float layerDepth = -1f;

			// Token: 0x04002C8B RID: 11403
			public int width = 16;

			// Token: 0x04002C8C RID: 11404
			public int height = 16;

			// Token: 0x04002C8D RID: 11405
			public float zPosition;

			// Token: 0x04002C8E RID: 11406
			public bool visible = true;

			// Token: 0x04002C8F RID: 11407
			public List<CraneGame.DrawEffect> drawEffects;

			// Token: 0x04002C90 RID: 11408
			protected bool _destroyed;
		}

		// Token: 0x020005A4 RID: 1444
		public class SwayEffect : CraneGame.DrawEffect
		{
			// Token: 0x06004225 RID: 16933 RVA: 0x003105AC File Offset: 0x0030E7AC
			public SwayEffect(float magnitude, float speed = 1f, int sway_duration = 10)
			{
				this.swayMagnitude = magnitude;
				this.swaySpeed = speed;
				this.swayDuration = sway_duration;
				this.age = 0;
			}

			// Token: 0x06004226 RID: 16934 RVA: 0x003105D8 File Offset: 0x0030E7D8
			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (this.age > this.swayDuration)
				{
					return true;
				}
				float progress = (float)this.age / (float)this.swayDuration;
				rotation += (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000.0 * 360.0 * (double)this.swaySpeed * 0.01745329238474369) * (1f - progress) * this.swayMagnitude;
				this.age++;
				return false;
			}

			// Token: 0x04002C91 RID: 11409
			public float swayMagnitude;

			// Token: 0x04002C92 RID: 11410
			public float swaySpeed;

			// Token: 0x04002C93 RID: 11411
			public int swayDuration = 1;

			// Token: 0x04002C94 RID: 11412
			public int age;
		}

		// Token: 0x020005A5 RID: 1445
		public class ShakeEffect : CraneGame.DrawEffect
		{
			// Token: 0x06004227 RID: 16935 RVA: 0x00310666 File Offset: 0x0030E866
			public ShakeEffect(float shake_x, float shake_y, int shake_duration = 10)
			{
				this.shakeAmount = new Vector2(shake_x, shake_y);
				this.shakeDuration = shake_duration;
				this.age = 0;
			}

			// Token: 0x06004228 RID: 16936 RVA: 0x00310690 File Offset: 0x0030E890
			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (this.age > this.shakeDuration)
				{
					return true;
				}
				float progress = (float)this.age / (float)this.shakeDuration;
				Vector2 current_shake = new Vector2(Utility.Lerp(this.shakeAmount.X, 1f, progress), Utility.Lerp(this.shakeAmount.Y, 1f, progress));
				position += new Vector2((float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.X, (float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.Y);
				this.age++;
				return false;
			}

			// Token: 0x04002C95 RID: 11413
			public Vector2 shakeAmount;

			// Token: 0x04002C96 RID: 11414
			public int shakeDuration = 1;

			// Token: 0x04002C97 RID: 11415
			public int age;
		}

		// Token: 0x020005A6 RID: 1446
		public class StretchEffect : CraneGame.DrawEffect
		{
			// Token: 0x06004229 RID: 16937 RVA: 0x00310759 File Offset: 0x0030E959
			public StretchEffect(float x_scale, float y_scale, int stretch_duration = 10)
			{
				this.stretchScale = new Vector2(x_scale, y_scale);
				this.stretchDuration = stretch_duration;
				this.age = 0;
			}

			// Token: 0x0600422A RID: 16938 RVA: 0x00310784 File Offset: 0x0030E984
			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (this.age > this.stretchDuration)
				{
					return true;
				}
				float progress = (float)this.age / (float)this.stretchDuration;
				Vector2 current_scale = new Vector2(Utility.Lerp(this.stretchScale.X, 1f, progress), Utility.Lerp(this.stretchScale.Y, 1f, progress));
				scale *= current_scale;
				this.age++;
				return false;
			}

			// Token: 0x04002C98 RID: 11416
			public Vector2 stretchScale;

			// Token: 0x04002C99 RID: 11417
			public int stretchDuration = 1;

			// Token: 0x04002C9A RID: 11418
			public int age;
		}

		// Token: 0x020005A7 RID: 1447
		public class DrawEffect
		{
			// Token: 0x0600422B RID: 16939 RVA: 0x00310805 File Offset: 0x0030EA05
			public virtual bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				return true;
			}
		}
	}
}

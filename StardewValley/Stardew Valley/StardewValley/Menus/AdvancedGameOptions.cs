using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x0200024B RID: 587
	public class AdvancedGameOptions : IClickableMenu
	{
		// Token: 0x060026F2 RID: 9970 RVA: 0x001B8590 File Offset: 0x001B6790
		public AdvancedGameOptions() : base(Game1.uiViewport.Width / 2 - 400, Game1.uiViewport.Height / 2 - 250, 800, 500, false)
		{
			this.ResetComponents();
		}

		// Token: 0x060026F3 RID: 9971 RVA: 0x001B8621 File Offset: 0x001B6821
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - 400;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - 250;
			this.ResetComponents();
		}

		// Token: 0x060026F4 RID: 9972 RVA: 0x001B8664 File Offset: 0x001B6864
		private void ResetComponents()
		{
			int scrollbarX = this.xPositionOnScreen + this.width + 16;
			this.upArrow = new ClickableTextureComponent(new Rectangle(scrollbarX, this.yPositionOnScreen, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
			this.downArrow = new ClickableTextureComponent(new Rectangle(scrollbarX, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
			this.scrollBarBounds = new Rectangle
			{
				X = this.upArrow.bounds.X + 12,
				Y = this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4,
				Width = 24
			};
			this.scrollBarBounds.Height = this.downArrow.bounds.Y - 4 - this.scrollBarBounds.Y;
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.scrollBarBounds.X, this.scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			this.optionSlots.Clear();
			for (int i = 0; i < 7; i++)
			{
				this.optionSlots.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + i * ((this.height - 16) / 7), this.width - 16, this.height / 7), i.ToString() ?? "")
				{
					myID = i,
					downNeighborID = ((i < 6) ? (i + 1) : -7777),
					upNeighborID = ((i > 0) ? (i - 1) : -7777),
					fullyImmutable = true
				});
			}
			this.PopulateOptions();
			this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = this.ID_okButton,
				upNeighborID = -99998
			};
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.setCurrentlySnappedComponentTo(this.ID_okButton);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x060026F5 RID: 9973 RVA: 0x001B88F4 File Offset: 0x001B6AF4
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			if (oldID != 0)
			{
				if (oldID == 6 && direction == 2)
				{
					if (this.currentItemIndex < Math.Max(0, this.options.Count - 7))
					{
						this.downArrowPressed();
						Game1.playSound("shiny4", null);
						return;
					}
					this.currentlySnappedComponent = base.getComponentWithID(this.ID_okButton);
					if (this.currentlySnappedComponent != null)
					{
						this.currentlySnappedComponent.upNeighborID = Math.Min(this.options.Count, 7) - 1;
						return;
					}
				}
			}
			else if (direction == 0)
			{
				if (this.currentItemIndex > 0)
				{
					this.upArrowPressed();
					Game1.playSound("shiny4", null);
					return;
				}
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x060026F6 RID: 9974 RVA: 0x001B89B8 File Offset: 0x001B6BB8
		public virtual void PopulateOptions()
		{
			this.options.Clear();
			this.tooltips.Clear();
			this.applySettingCallbacks.Clear();
			this.AddHeader(Game1.content.LoadString("Strings\\UI:AGO_Label"));
			this.AddDropdown<Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB"), Game1.content.LoadString("Strings\\UI:AGO_CCB_Tooltip"), true, () => Game1.bundleType, delegate(Game1.BundleType val)
			{
				Game1.bundleType = val;
			}, new KeyValuePair<string, Game1.BundleType>[]
			{
				new KeyValuePair<string, Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Normal"), Game1.BundleType.Default),
				new KeyValuePair<string, Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Remixed"), Game1.BundleType.Remixed)
			});
			this.AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_Year1Completable"), Game1.content.LoadString("Strings\\UI:AGO_Year1Completable_Tooltip"), () => Game1.game1.GetNewGameOption<bool>("YearOneCompletable"), delegate(bool val)
			{
				Game1.game1.SetNewGameOption<bool>("YearOneCompletable", val);
			});
			this.AddDropdown<Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_MineTreasureShuffle"), Game1.content.LoadString("Strings\\UI:AGO_MineTreasureShuffle_Tooltip"), true, () => Game1.game1.GetNewGameOption<Game1.MineChestType>("MineChests"), delegate(Game1.MineChestType val)
			{
				Game1.game1.SetNewGameOption<Game1.MineChestType>("MineChests", val);
			}, new KeyValuePair<string, Game1.MineChestType>[]
			{
				new KeyValuePair<string, Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Normal"), Game1.MineChestType.Default),
				new KeyValuePair<string, Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Remixed"), Game1.MineChestType.Remixed)
			});
			this.AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_FarmMonsters"), Game1.content.LoadString("Strings\\UI:AGO_FarmMonsters_Tooltip"), delegate
			{
				bool value = Game1.spawnMonstersAtNight;
				if (Game1.game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
				{
					value = Game1.game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
				}
				this.initialMonsterSpawnAtValue = value;
				return value;
			}, delegate(bool val)
			{
				if (this.initialMonsterSpawnAtValue != val)
				{
					Game1.game1.SetNewGameOption<bool>("SpawnMonstersAtNight", val);
				}
			});
			this.AddDropdown<float>(Game1.content.LoadString("Strings\\UI:Character_Difficulty"), Game1.content.LoadString("Strings\\UI:AGO_ProfitMargin_Tooltip"), false, () => Game1.player.difficultyModifier, delegate(float val)
			{
				Game1.player.difficultyModifier = val;
			}, new KeyValuePair<string, float>[]
			{
				new KeyValuePair<string, float>(Game1.content.LoadString("Strings\\UI:Character_Normal"), 1f),
				new KeyValuePair<string, float>("75%", 0.75f),
				new KeyValuePair<string, float>("50%", 0.5f),
				new KeyValuePair<string, float>("25%", 0.25f)
			});
			this.AddHeader(Game1.content.LoadString("Strings\\UI:AGO_MPOptions_Label"));
			KeyValuePair<string, int>[] startingCabinOptions = new KeyValuePair<string, int>[Game1.multiplayer.playerLimit];
			startingCabinOptions[0] = new KeyValuePair<string, int>(Game1.content.LoadString("Strings\\UI:Character_none"), 0);
			for (int i = 1; i < Game1.multiplayer.playerLimit; i++)
			{
				startingCabinOptions[i] = new KeyValuePair<string, int>(i.ToString(), i);
			}
			this.AddDropdown<int>(Game1.content.LoadString("Strings\\UI:Character_StartingCabins"), Game1.content.LoadString("Strings\\UI:AGO_StartingCabins_Tooltip"), false, () => Game1.startingCabins, delegate(int val)
			{
				Game1.startingCabins = val;
			}, startingCabinOptions);
			this.AddDropdown<bool>(Game1.content.LoadString("Strings\\UI:Character_CabinLayout"), Game1.content.LoadString("Strings\\UI:AGO_CabinLayout_Tooltip"), false, () => Game1.cabinsSeparate, delegate(bool val)
			{
				Game1.cabinsSeparate = val;
			}, new KeyValuePair<string, bool>[]
			{
				new KeyValuePair<string, bool>(Game1.content.LoadString("Strings\\UI:Character_Close"), false),
				new KeyValuePair<string, bool>(Game1.content.LoadString("Strings\\UI:Character_Separate"), true)
			});
			this.AddHeader(Game1.content.LoadString("Strings\\UI:AGO_OtherOptions_Label"));
			this.AddTextEntry(Game1.content.LoadString("Strings\\UI:AGO_RandomSeed"), Game1.content.LoadString("Strings\\UI:AGO_RandomSeed_Tooltip"), true, delegate
			{
				if (Game1.startingGameSeed == null)
				{
					return "";
				}
				return Game1.startingGameSeed.Value.ToString();
			}, delegate(string val)
			{
				val.Trim();
				if (string.IsNullOrEmpty(val))
				{
					Game1.startingGameSeed = null;
					return;
				}
				while (val.Length > 0)
				{
					ulong starting_seed;
					if (ulong.TryParse(val, out starting_seed))
					{
						Game1.startingGameSeed = new ulong?(starting_seed);
						return;
					}
					val = val.Substring(0, val.Length - 1);
				}
			}, delegate(OptionsTextEntry textbox)
			{
				textbox.textBox.numbersOnly = true;
				textbox.textBox.textLimit = 9;
			});
			this.AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_LegacyRandomization"), Game1.content.LoadString("Strings\\UI:AGO_LegacyRandomization_Tooltip"), () => Game1.UseLegacyRandom, delegate(bool val)
			{
				Game1.UseLegacyRandom = val;
			});
			for (int j = this.options.Count; j < 7; j++)
			{
				this.options.Add(new OptionsElement(""));
			}
		}

		// Token: 0x060026F7 RID: 9975 RVA: 0x001B8F3C File Offset: 0x001B713C
		public virtual void CloseAndApply()
		{
			foreach (Action action in this.applySettingCallbacks)
			{
				action();
			}
			this.applySettingCallbacks.Clear();
			base.exitThisMenu(true);
		}

		// Token: 0x060026F8 RID: 9976 RVA: 0x001B8FA0 File Offset: 0x001B71A0
		public virtual void AddHeader(string label)
		{
			this.options.Add(new OptionsElement(label));
		}

		// Token: 0x060026F9 RID: 9977 RVA: 0x001B8FB4 File Offset: 0x001B71B4
		public virtual void AddTextEntry(string label, string tooltip, bool labelOnSeparateLine, Func<string> get, Action<string> set, Action<OptionsTextEntry> configure = null)
		{
			if (labelOnSeparateLine)
			{
				OptionsElement labelElement = new OptionsElement(label)
				{
					style = OptionsElement.Style.OptionLabel
				};
				this.options.Add(labelElement);
				this.tooltips[labelElement] = tooltip;
			}
			OptionsTextEntry option_element = new OptionsTextEntry(labelOnSeparateLine ? string.Empty : label, -999, -1, -1);
			if (configure != null)
			{
				configure(option_element);
			}
			this.tooltips[option_element] = tooltip;
			option_element.textBox.Text = get();
			this.applySettingCallbacks.Add(delegate
			{
				set(option_element.textBox.Text);
			});
			this.options.Add(option_element);
		}

		// Token: 0x060026FA RID: 9978 RVA: 0x001B9078 File Offset: 0x001B7278
		public virtual void AddDropdown<T>(string label, string tooltip, bool labelOnSeparateLine, Func<T> get, Action<T> set, params KeyValuePair<string, T>[] dropdown_options)
		{
			if (labelOnSeparateLine)
			{
				OptionsElement labelElement = new OptionsElement(label)
				{
					style = OptionsElement.Style.OptionLabel
				};
				this.options.Add(labelElement);
				this.tooltips[labelElement] = tooltip;
			}
			OptionsDropDown option_element = new OptionsDropDown(labelOnSeparateLine ? string.Empty : label, -999, -1, -1);
			this.tooltips[option_element] = tooltip;
			foreach (KeyValuePair<string, T> option in dropdown_options)
			{
				option_element.dropDownDisplayOptions.Add(option.Key);
				List<string> dropDownOptions = option_element.dropDownOptions;
				T value = option.Value;
				dropDownOptions.Add(value.ToString());
			}
			option_element.RecalculateBounds();
			T selected_value = get();
			int selected_option = 0;
			int i = 0;
			while (i < dropdown_options.Length)
			{
				KeyValuePair<string, T> dropdown_option = dropdown_options[i];
				if (dropdown_option.Value != null || selected_value != null)
				{
					if (dropdown_option.Value != null && selected_value != null)
					{
						T value = dropdown_option.Value;
						if (value.Equals(selected_value))
						{
							goto IL_145;
						}
					}
					i++;
					continue;
				}
				IL_145:
				selected_option = i;
				break;
			}
			option_element.selectedOption = selected_option;
			this.applySettingCallbacks.Add(delegate
			{
				set(dropdown_options[option_element.selectedOption].Value);
			});
			this.options.Add(option_element);
		}

		// Token: 0x060026FB RID: 9979 RVA: 0x001B9218 File Offset: 0x001B7418
		public virtual void AddCheckbox(string label, string tooltip, Func<bool> get, Action<bool> set)
		{
			OptionsCheckbox option_element = new OptionsCheckbox(label, -999, -1, -1);
			this.tooltips[option_element] = tooltip;
			option_element.isChecked = get();
			this.applySettingCallbacks.Add(delegate
			{
				set(option_element.isChecked);
			});
			this.options.Add(option_element);
		}

		// Token: 0x060026FC RID: 9980 RVA: 0x001B9291 File Offset: 0x001B7491
		public override bool readyToClose()
		{
			return false;
		}

		// Token: 0x060026FD RID: 9981 RVA: 0x001B9294 File Offset: 0x001B7494
		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			this.currentlySnappedComponent = base.getComponentWithID(this.ID_okButton);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060026FE RID: 9982 RVA: 0x001B92B4 File Offset: 0x001B74B4
		public override void applyMovementKey(int direction)
		{
			if (!this.IsDropdownActive())
			{
				base.applyMovementKey(direction);
			}
		}

		// Token: 0x060026FF RID: 9983 RVA: 0x001B92C8 File Offset: 0x001B74C8
		private void setScrollBarToCurrentIndex()
		{
			if (this.options.Count > 0)
			{
				this.scrollBar.bounds.Y = this.scrollBarBounds.Y + this.scrollBarBounds.Height / Math.Max(1, this.options.Count - 7) * this.currentItemIndex;
				if (this.currentItemIndex == this.options.Count - 7)
				{
					this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
				}
			}
		}

		// Token: 0x06002700 RID: 9984 RVA: 0x001B9370 File Offset: 0x001B7570
		public override void snapCursorToCurrentSnappedComponent()
		{
			if (this.currentlySnappedComponent == null || this.currentlySnappedComponent.myID >= this.options.Count)
			{
				if (this.currentlySnappedComponent != null)
				{
					base.snapCursorToCurrentSnappedComponent();
				}
				return;
			}
			OptionsElement optionsElement = this.options[this.currentlySnappedComponent.myID + this.currentItemIndex];
			OptionsDropDown dropdown = optionsElement as OptionsDropDown;
			if (dropdown != null)
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + dropdown.bounds.Right - 32, this.currentlySnappedComponent.bounds.Center.Y - 4);
				return;
			}
			if (optionsElement is OptionsPlusMinusButton)
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + 64, this.currentlySnappedComponent.bounds.Center.Y + 4);
				return;
			}
			if (!(optionsElement is OptionsInputListener))
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + 48, this.currentlySnappedComponent.bounds.Center.Y - 12);
				return;
			}
			Game1.setMousePosition(this.currentlySnappedComponent.bounds.Right - 48, this.currentlySnappedComponent.bounds.Center.Y - 12);
		}

		// Token: 0x06002701 RID: 9985 RVA: 0x001B94BC File Offset: 0x001B76BC
		public virtual void SetScrollFromY(int y)
		{
			int y2 = this.scrollBar.bounds.Y;
			float percentage = (float)(y - this.scrollBarBounds.Y) / (float)this.scrollBarBounds.Height;
			percentage = Utility.Clamp(percentage, 0f, 1f);
			this.currentItemIndex = (int)Utility.Lerp(0f, (float)(this.options.Count - 7), percentage);
			this.setScrollBarToCurrentIndex();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06002702 RID: 9986 RVA: 0x001B9554 File Offset: 0x001B7754
		public override void leftClickHeld(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				this.SetScrollFromY(y);
				return;
			}
			if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickHeld(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
		}

		// Token: 0x06002703 RID: 9987 RVA: 0x001B95FB File Offset: 0x001B77FB
		public override void setCurrentlySnappedComponentTo(int id)
		{
			this.currentlySnappedComponent = base.getComponentWithID(id);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002704 RID: 9988 RVA: 0x001B9610 File Offset: 0x001B7810
		public override void receiveKeyPress(Keys key)
		{
			if ((this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
			{
				if (this.currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && this.options.Count > this.currentItemIndex + this.currentlySnappedComponent.myID && this.currentItemIndex + this.currentlySnappedComponent.myID >= 0)
				{
					this.options[this.currentItemIndex + this.currentlySnappedComponent.myID].receiveKeyPress(key);
				}
				else if (this.options.Count > this.currentItemIndex + this.optionsSlotHeld && this.currentItemIndex + this.optionsSlotHeld >= 0)
				{
					this.options[this.currentItemIndex + this.optionsSlotHeld].receiveKeyPress(key);
				}
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x06002705 RID: 9989 RVA: 0x001B9728 File Offset: 0x001B7928
		public override void receiveScrollWheelAction(int direction)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (this.IsDropdownActive())
			{
				return;
			}
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shiny4", null);
			}
			else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4", null);
			}
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002706 RID: 9990 RVA: 0x001B97C4 File Offset: 0x001B79C4
		public override void releaseLeftClick(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickReleased(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
			this.optionsSlotHeld = -1;
			this.scrolling = false;
		}

		// Token: 0x06002707 RID: 9991 RVA: 0x001B986C File Offset: 0x001B7A6C
		public bool IsDropdownActive()
		{
			return this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count && this.options[this.currentItemIndex + this.optionsSlotHeld] is OptionsDropDown;
		}

		// Token: 0x06002708 RID: 9992 RVA: 0x001B98BE File Offset: 0x001B7ABE
		private void downArrowPressed()
		{
			if (this.IsDropdownActive())
			{
				return;
			}
			this.downArrow.scale = this.downArrow.baseScale;
			this.currentItemIndex++;
			this.UnsubscribeFromSelectedTextbox();
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002709 RID: 9993 RVA: 0x001B98FC File Offset: 0x001B7AFC
		public virtual void UnsubscribeFromSelectedTextbox()
		{
			if (Game1.keyboardDispatcher.Subscriber != null)
			{
				foreach (OptionsElement optionsElement in this.options)
				{
					OptionsTextEntry entry = optionsElement as OptionsTextEntry;
					if (entry != null && Game1.keyboardDispatcher.Subscriber == entry.textBox)
					{
						Game1.keyboardDispatcher.Subscriber = null;
						break;
					}
				}
			}
		}

		// Token: 0x0600270A RID: 9994 RVA: 0x001B997C File Offset: 0x001B7B7C
		public void preWindowSizeChange()
		{
			AdvancedGameOptions._lastSelectedIndex = ((this.getCurrentlySnappedComponent() != null) ? this.getCurrentlySnappedComponent().myID : -1);
			AdvancedGameOptions._lastCurrentItemIndex = this.currentItemIndex;
		}

		// Token: 0x0600270B RID: 9995 RVA: 0x001B99A4 File Offset: 0x001B7BA4
		public void postWindowSizeChange()
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.setCurrentlySnappedComponentTo(AdvancedGameOptions._lastSelectedIndex);
			}
			this.currentItemIndex = AdvancedGameOptions._lastCurrentItemIndex;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x0600270C RID: 9996 RVA: 0x001B99D2 File Offset: 0x001B7BD2
		private void upArrowPressed()
		{
			if (this.IsDropdownActive())
			{
				return;
			}
			this.upArrow.scale = this.upArrow.baseScale;
			this.currentItemIndex--;
			this.UnsubscribeFromSelectedTextbox();
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x0600270D RID: 9997 RVA: 0x001B9A10 File Offset: 0x001B7C10
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shwip", null);
			}
			else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shwip", null);
			}
			else if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
			}
			else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height)
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
			}
			this.currentItemIndex = Math.Max(0, Math.Min(this.options.Count - 7, this.currentItemIndex));
			if (this.okButton.containsPoint(x, y))
			{
				this.CloseAndApply();
				return;
			}
			this.UnsubscribeFromSelectedTextbox();
			for (int i = 0; i < this.optionSlots.Count; i++)
			{
				if (this.optionSlots[i].bounds.Contains(x, y) && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
				{
					this.options[this.currentItemIndex + i].receiveLeftClick(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y);
					this.optionsSlotHeld = i;
					return;
				}
			}
		}

		// Token: 0x0600270E RID: 9998 RVA: 0x001B9C50 File Offset: 0x001B7E50
		public override void performHoverAction(int x, int y)
		{
			this.okButton.tryHover(x, y, 0.1f);
			for (int i = 0; i < this.optionSlots.Count; i++)
			{
				if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
				{
					Game1.SetFreeCursorDrag();
					break;
				}
			}
			if (this.scrollBarBounds.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
			}
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			this.hoverText = "";
			int hovered_index = -1;
			if (!this.IsDropdownActive())
			{
				for (int j = 0; j < this.optionSlots.Count; j++)
				{
					if (this.optionSlots[j].containsPoint(x, y) && j + this.currentItemIndex < this.options.Count && this.hoverText == "")
					{
						hovered_index = j + this.currentItemIndex;
					}
				}
			}
			if (this._lastHoveredIndex != hovered_index)
			{
				this._lastHoveredIndex = hovered_index;
				this._hoverDuration = 0;
			}
			else
			{
				this._hoverDuration += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			}
			if (this._lastHoveredIndex >= 0 && this._hoverDuration >= 500)
			{
				OptionsElement option = this.options[this._lastHoveredIndex];
				string tooltip;
				if (this.tooltips.TryGetValue(option, out tooltip))
				{
					this.hoverText = Game1.parseText(tooltip);
				}
			}
			this.upArrow.tryHover(x, y, 0.1f);
			this.downArrow.tryHover(x, y, 0.1f);
			this.scrollBar.tryHover(x, y, 0.1f);
		}

		// Token: 0x0600270F RID: 9999 RVA: 0x001B9E40 File Offset: 0x001B8040
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black * 0.75f);
			Game1.DrawBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, null);
			this.okButton.draw(b);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			for (int i = 0; i < this.optionSlots.Count; i++)
			{
				if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count)
				{
					this.options[this.currentItemIndex + i].draw(b, this.optionSlots[i].bounds.X, this.optionSlots[i].bounds.Y, this);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (this.options.Count > 7)
			{
				this.upArrow.draw(b);
				this.downArrow.draw(b);
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarBounds.X, this.scrollBarBounds.Y, this.scrollBarBounds.Width, this.scrollBarBounds.Height, Color.White, 4f, false, -1f);
				this.scrollBar.draw(b);
			}
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x0400181E RID: 6174
		public const int itemsPerPage = 7;

		// Token: 0x0400181F RID: 6175
		private string hoverText = "";

		// Token: 0x04001820 RID: 6176
		public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

		// Token: 0x04001821 RID: 6177
		public int currentItemIndex;

		// Token: 0x04001822 RID: 6178
		private ClickableTextureComponent upArrow;

		// Token: 0x04001823 RID: 6179
		private ClickableTextureComponent downArrow;

		// Token: 0x04001824 RID: 6180
		private ClickableTextureComponent scrollBar;

		// Token: 0x04001825 RID: 6181
		public ClickableTextureComponent okButton;

		// Token: 0x04001826 RID: 6182
		public List<Action> applySettingCallbacks = new List<Action>();

		// Token: 0x04001827 RID: 6183
		public Dictionary<OptionsElement, string> tooltips = new Dictionary<OptionsElement, string>();

		// Token: 0x04001828 RID: 6184
		public int ID_okButton = 10000;

		// Token: 0x04001829 RID: 6185
		private bool scrolling;

		// Token: 0x0400182A RID: 6186
		public List<OptionsElement> options = new List<OptionsElement>();

		// Token: 0x0400182B RID: 6187
		private Rectangle scrollBarBounds;

		// Token: 0x0400182C RID: 6188
		protected static int _lastSelectedIndex;

		// Token: 0x0400182D RID: 6189
		protected static int _lastCurrentItemIndex;

		// Token: 0x0400182E RID: 6190
		protected int _lastHoveredIndex;

		// Token: 0x0400182F RID: 6191
		protected int _hoverDuration;

		// Token: 0x04001830 RID: 6192
		public const int WINDOW_WIDTH = 800;

		// Token: 0x04001831 RID: 6193
		public const int WINDOW_HEIGHT = 500;

		// Token: 0x04001832 RID: 6194
		public bool initialMonsterSpawnAtValue;

		// Token: 0x04001833 RID: 6195
		private int optionsSlotHeld = -1;
	}
}

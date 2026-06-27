using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shirts;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Minigames;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	// Token: 0x02000256 RID: 598
	public class CharacterCustomization : IClickableMenu
	{
		// Token: 0x060027BB RID: 10171 RVA: 0x001C7658 File Offset: 0x001C5858
		public CharacterCustomization(Clothing item) : this(CharacterCustomization.Source.ClothesDye, false)
		{
			this._itemToDye = item;
			this.ResetComponents();
			if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
			{
				Game1.spawnMonstersAtNight = false;
			}
			this._recolorPantsAction = delegate()
			{
				this.DyeItem(this.pantsColorPicker.getSelectedColor());
			};
			Clothing.ClothesType value = this._itemToDye.clothesType.Value;
			if (value != Clothing.ClothesType.SHIRT)
			{
				if (value == Clothing.ClothesType.PANTS)
				{
					this._displayFarmer.Equip<Clothing>(this._itemToDye, this._displayFarmer.pantsItem);
				}
			}
			else
			{
				this._displayFarmer.Equip<Clothing>(this._itemToDye, this._displayFarmer.shirtItem);
			}
			this._displayFarmer.UpdateClothing();
		}

		// Token: 0x060027BC RID: 10172 RVA: 0x001C7704 File Offset: 0x001C5904
		public void DyeItem(Color color)
		{
			if (this._itemToDye != null)
			{
				this._itemToDye.Dye(color, 1f);
				this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
			}
		}

		// Token: 0x060027BD RID: 10173 RVA: 0x001C7730 File Offset: 0x001C5930
		public CharacterCustomization(CharacterCustomization.Source source, bool multiplayerServer = false) : base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64, false)
		{
			if (source == CharacterCustomization.Source.NewGame || source == CharacterCustomization.Source.HostNewFarm)
			{
				Game1.player.difficultyModifier = 1f;
				Game1.player.team.useSeparateWallets.Value = false;
				Game1.startingCabins = ((source == CharacterCustomization.Source.HostNewFarm) ? 1 : 0);
			}
			this.LoadFarmTypeData();
			this.oldName = Game1.player.Name;
			this._multiplayerServer = multiplayerServer;
			int items_to_dye = 0;
			if (source == CharacterCustomization.Source.ClothesDye || source == CharacterCustomization.Source.DyePots)
			{
				this._isDyeMenu = true;
				if (source != CharacterCustomization.Source.ClothesDye)
				{
					if (source == CharacterCustomization.Source.DyePots)
					{
						if (Game1.player.CanDyePants())
						{
							items_to_dye++;
						}
						if (Game1.player.CanDyeShirt())
						{
							items_to_dye++;
						}
					}
				}
				else
				{
					items_to_dye = 1;
				}
				this.height = 308 + IClickableMenu.borderWidth * 2 + 64 + 72 * items_to_dye - 4;
				this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
				this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2 - 64;
			}
			this.source = source;
			this.ResetComponents();
			this._recolorEyesAction = delegate()
			{
				Game1.player.changeEyeColor(this.eyeColorPicker.getSelectedColor());
			};
			this._recolorPantsAction = delegate()
			{
				Game1.player.changePantsColor(this.pantsColorPicker.getSelectedColor());
			};
			this._recolorHairAction = delegate()
			{
				Game1.player.changeHairColor(this.hairColorPicker.getSelectedColor());
			};
			if (source == CharacterCustomization.Source.DyePots)
			{
				this._recolorHairAction = delegate()
				{
					if (Game1.player.CanDyeShirt())
					{
						Game1.player.shirtItem.Value.clothesColor.Value = this.hairColorPicker.getSelectedColor();
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				};
				this._recolorPantsAction = delegate()
				{
					if (Game1.player.CanDyePants())
					{
						Game1.player.pantsItem.Value.clothesColor.Value = this.pantsColorPicker.getSelectedColor();
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				};
				this.favThingBoxCC.visible = false;
				this.nameBoxCC.visible = false;
				this.farmnameBoxCC.visible = false;
				this.favoriteLabel.visible = false;
				this.nameLabel.visible = false;
				this.farmLabel.visible = false;
			}
			this._displayFarmer = this.GetOrCreateDisplayFarmer();
		}

		// Token: 0x060027BE RID: 10174 RVA: 0x001C79C4 File Offset: 0x001C5BC4
		public Farmer GetOrCreateDisplayFarmer()
		{
			if (this._displayFarmer == null)
			{
				if (this.source == CharacterCustomization.Source.ClothesDye || this.source == CharacterCustomization.Source.DyePots)
				{
					this._displayFarmer = Game1.player.CreateFakeEventFarmer();
				}
				else
				{
					this._displayFarmer = Game1.player;
				}
				if (this.source == CharacterCustomization.Source.NewFarmhand)
				{
					if (this._displayFarmer.pants.Value == null)
					{
						this._displayFarmer.pants.Value = this._displayFarmer.GetPantsId();
					}
					if (this._displayFarmer.shirt.Value == null)
					{
						this._displayFarmer.shirt.Value = this._displayFarmer.GetShirtId();
					}
				}
				this._displayFarmer.faceDirection(2);
				this._displayFarmer.FarmerSprite.StopAnimation();
			}
			return this._displayFarmer;
		}

		// Token: 0x060027BF RID: 10175 RVA: 0x001C7A90 File Offset: 0x001C5C90
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.ResetComponents();
		}

		// Token: 0x060027C0 RID: 10176 RVA: 0x001C7AA0 File Offset: 0x001C5CA0
		public void showAdvancedCharacterCreationHighlight()
		{
			this.advancedCCHighlightTimer = 4000f;
		}

		// Token: 0x060027C1 RID: 10177 RVA: 0x001C7AB0 File Offset: 0x001C5CB0
		private void ResetComponents()
		{
			if (this._isDyeMenu)
			{
				this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
				this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2 - 64;
			}
			else
			{
				this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
				this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			}
			this.colorPickerCCs.Clear();
			if (this.source == CharacterCustomization.Source.ClothesDye && this._itemToDye == null)
			{
				return;
			}
			bool creatingNewSave = this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm;
			bool allow_clothing_changes = this.source != CharacterCustomization.Source.Wizard && this.source != CharacterCustomization.Source.ClothesDye && this.source != CharacterCustomization.Source.DyePots;
			bool allow_accessory_changes = this.source != CharacterCustomization.Source.ClothesDye && this.source != CharacterCustomization.Source.DyePots;
			this.labels.Clear();
			this.genderButtons.Clear();
			this.cabinLayoutButtons.Clear();
			this.leftSelectionButtons.Clear();
			this.rightSelectionButtons.Clear();
			this.farmTypeButtons.Clear();
			if (creatingNewSave)
			{
				this.advancedOptionsButton = new ClickableTextureComponent("Advanced", new Rectangle(this.xPositionOnScreen - 80, this.yPositionOnScreen + this.height - 80 - 16, 80, 80), null, null, Game1.mouseCursors2, new Rectangle(154, 154, 20, 20), 4f, false)
				{
					myID = 636,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
			else
			{
				this.advancedOptionsButton = null;
			}
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 505,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
			{
				myID = 81114,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
				Y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
				Text = Game1.player.Name
			};
			this.nameBoxCC = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 192, 48), "")
			{
				myID = 536,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			int textBoxLabelsXOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? -4 : 0;
			this.labels.Add(this.nameLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Name")));
			this.farmnameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
				Y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64,
				Text = Game1.MasterPlayer.farmName.Value
			};
			this.farmnameBoxCC = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 192, 48), "")
			{
				myID = 537,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			int farmLabelXOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? -16 : 0;
			this.labels.Add(this.farmLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + textBoxLabelsXOffset * 3 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4 + farmLabelXOffset, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Farm")));
			int favThingBoxXoffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 48 : 0;
			this.favThingBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256 + favThingBoxXoffset,
				Y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128,
				Text = Game1.player.favoriteThing.Value
			};
			this.favThingBoxCC = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 192, 48), "")
			{
				myID = 538,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.labels.Add(this.favoriteLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 1, 1), Game1.content.LoadString("Strings\\UI:Character_FavoriteThing")));
			this.randomButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f, false)
			{
				myID = 507,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			if (this.source == CharacterCustomization.Source.DyePots || this.source == CharacterCustomization.Source.ClothesDye)
			{
				this.randomButton.visible = false;
			}
			this.portraitBox = new Rectangle(this.xPositionOnScreen + 64 + 42 - 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);
			if (this._isDyeMenu)
			{
				this.portraitBox.X = this.xPositionOnScreen + (this.width - this.portraitBox.Width) / 2;
				this.randomButton.bounds.X = this.portraitBox.X - 56;
			}
			int yOffset = 128;
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.X - 32, this.portraitBox.Y + 144, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
			{
				myID = 520,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.Right - 32, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
			{
				myID = 521,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int leftSelectionXOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? -20 : 0;
			this.isModifyingExistingPet = false;
			if (creatingNewSave)
			{
				this.petPortraitBox = new Rectangle?(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 448 - 16 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 60 : 0), this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 - 16, 64, 64));
				this.labels.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8 + textBoxLabelsXOffset, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8 + 192, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Animal")));
			}
			if (creatingNewSave || this.source == CharacterCustomization.Source.NewFarmhand || this.source == CharacterCustomization.Source.Wizard)
			{
				this.genderButtons.Add(new ClickableTextureComponent("Male", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f, false)
				{
					myID = 508,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.genderButtons.Add(new ClickableTextureComponent("Female", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 64 + 24, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f, false)
				{
					myID = 509,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				if (this.source == CharacterCustomization.Source.Wizard)
				{
					List<ClickableComponent> list = this.genderButtons;
					if (list != null && list.Count > 0)
					{
						int start_x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 320 + 16;
						int start_y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64 + 48;
						for (int i = 0; i < this.genderButtons.Count; i++)
						{
							this.genderButtons[i].bounds.X = start_x + 80 * i;
							this.genderButtons[i].bounds.Y = start_y;
						}
					}
				}
				yOffset = 256;
				if (this.source == CharacterCustomization.Source.Wizard)
				{
					yOffset = 192;
				}
				leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? -20 : 0);
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + leftSelectionXOffset, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 518,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.skinLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + leftSelectionXOffset / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 519,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			if (creatingNewSave)
			{
				this.RefreshFarmTypeButtons();
			}
			if (this.source == CharacterCustomization.Source.HostNewFarm)
			{
				this.labels.Add(this.startingCabinsLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 21 - 128, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 84, 1, 1), Game1.content.LoadString("Strings\\UI:Character_StartingCabins")));
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 621,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 622,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.cabinLayoutLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 128 - (int)(Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_CabinLayout")).X / 2f), this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 120 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_CabinLayout")));
				this.cabinLayoutButtons.Add(new ClickableTextureComponent("Close", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Close"), Game1.mouseCursors, new Rectangle(208, 192, 16, 16), 4f, false)
				{
					myID = 623,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.cabinLayoutButtons.Add(new ClickableTextureComponent("Separate", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Separate"), Game1.mouseCursors, new Rectangle(224, 192, 16, 16), 4f, false)
				{
					myID = 624,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.difficultyModifierLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 21 - 128, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 56, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Difficulty")));
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 627,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 628,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				int walletY = this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 320 + 100;
				this.labels.Add(this.separateWalletLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 21 - 128, walletY - 24, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Wallets")));
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 631,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 632,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.coopHelpButton = new ClickableTextureComponent("CoopHelp", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 448 + 40, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_CoopHelp"), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 4f, false)
				{
					myID = 625,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				this.coopHelpOkButton = new ClickableTextureComponent("CoopHelpOK", new Rectangle(this.xPositionOnScreen - 256 - 12, this.yPositionOnScreen + this.height - 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
				{
					myID = 626,
					region = 635,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				this.noneString = Game1.content.LoadString("Strings\\UI:Character_none");
				this.normalDiffString = Game1.content.LoadString("Strings\\UI:Character_Normal");
				this.toughDiffString = Game1.content.LoadString("Strings\\UI:Character_Tough");
				this.hardDiffString = Game1.content.LoadString("Strings\\UI:Character_Hard");
				this.superDiffString = Game1.content.LoadString("Strings\\UI:Character_Super");
				this.separateWalletString = Game1.content.LoadString("Strings\\UI:Character_SeparateWallet");
				this.sharedWalletString = Game1.content.LoadString("Strings\\UI:Character_SharedWallet");
				this.coopHelpRightButton = new ClickableTextureComponent("CoopHelpRight", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 633,
					region = 635,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				this.coopHelpLeftButton = new ClickableTextureComponent("CoopHelpLeft", new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 634,
					region = 635,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
			Point top = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			int label_position = this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;
			if (this._isDyeMenu)
			{
				label_position = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
			}
			if (creatingNewSave || this.source == CharacterCustomization.Source.NewFarmhand || this.source == CharacterCustomization.Source.Wizard)
			{
				this.labels.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
				this.eyeColorPicker = new ColorPicker("Eyes", top.X, top.Y);
				this.eyeColorPicker.setColor(Game1.player.newEyeColor.Value);
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 522,
					downNeighborID = -99998,
					upNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 523,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 524,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				yOffset += 68;
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + leftSelectionXOffset, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 514,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.hairLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 515,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			top = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			if (creatingNewSave || this.source == CharacterCustomization.Source.NewFarmhand || this.source == CharacterCustomization.Source.Wizard)
			{
				this.labels.Add(new ClickableComponent(new Rectangle(label_position, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));
				this.hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
				this.hairColorPicker.setColor(Game1.player.hairstyleColor.Value);
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 525,
					downNeighborID = -99998,
					upNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 526,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 527,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
			}
			if (this.source == CharacterCustomization.Source.DyePots)
			{
				yOffset += 68;
				if (Game1.player.CanDyeShirt())
				{
					top = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
					top.X = this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
					this.labels.Add(new ClickableComponent(new Rectangle(label_position, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_ShirtColor")));
					this.hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
					this.hairColorPicker.setColor(Game1.player.GetShirtColor());
					this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
					{
						myID = 525,
						downNeighborID = -99998,
						upNeighborID = -99998,
						leftNeighborImmutable = true,
						rightNeighborImmutable = true
					});
					this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
					{
						myID = 526,
						upNeighborID = -99998,
						downNeighborID = -99998,
						leftNeighborImmutable = true,
						rightNeighborImmutable = true
					});
					this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
					{
						myID = 527,
						upNeighborID = -99998,
						downNeighborID = -99998,
						leftNeighborImmutable = true,
						rightNeighborImmutable = true
					});
					yOffset += 64;
				}
				if (Game1.player.CanDyePants())
				{
					top = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
					top.X = this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
					int pantsColorLabelYOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? -16 : 0;
					this.labels.Add(new ClickableComponent(new Rectangle(label_position, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16 + pantsColorLabelYOffset, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
					this.pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
					this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
					this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
					{
						myID = 528,
						downNeighborID = -99998,
						upNeighborID = -99998,
						rightNeighborImmutable = true,
						leftNeighborImmutable = true
					});
					this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
					{
						myID = 529,
						downNeighborID = -99998,
						upNeighborID = -99998,
						rightNeighborImmutable = true,
						leftNeighborImmutable = true
					});
					this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
					{
						myID = 530,
						downNeighborID = -99998,
						upNeighborID = -99998,
						rightNeighborImmutable = true,
						leftNeighborImmutable = true
					});
				}
			}
			else if (allow_clothing_changes)
			{
				yOffset += 68;
				int shirtArrowsExtraWidth = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 8 : 0;
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - shirtArrowsExtraWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 512,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.shirtLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + shirtArrowsExtraWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 513,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				int pantsColorLabelYOffset2 = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? -16 : 0;
				this.labels.Add(new ClickableComponent(new Rectangle(label_position, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16 + pantsColorLabelYOffset2, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
				top = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				this.pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
				this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 528,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 529,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 530,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
			}
			else if (this.source == CharacterCustomization.Source.ClothesDye)
			{
				yOffset += 60;
				top = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
				this.labels.Add(new ClickableComponent(new Rectangle(label_position, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_DyeColor")));
				this.pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
				this.pantsColorPicker.setColor(this._itemToDye.clothesColor.Value);
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 528,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 529,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 530,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
			}
			this.skipIntroButton = new ClickableTextureComponent("Skip Intro", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 - 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 80, 36, 36), null, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4f, false)
			{
				myID = 506,
				upNeighborID = 530,
				leftNeighborID = 517,
				rightNeighborID = 505
			};
			this.skipIntroButton.sourceRect.X = (this.skipIntro ? 236 : 227);
			if (allow_clothing_changes)
			{
				yOffset += 68;
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 629,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.pantsStyleLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 517,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			yOffset += 68;
			if (allow_accessory_changes)
			{
				int accessoryArrowsExtraWidth = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 32 : 0;
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - accessoryArrowsExtraWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 516,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.labels.Add(this.accLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + accessoryArrowsExtraWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 517,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			if (Game1.gameMode == 3)
			{
				IList<GameLocation> locations = Game1.locations;
			}
			if (this.petPortraitBox != null)
			{
				this.leftSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(this.petPortraitBox.Value.Left - 64, this.petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 511,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(this.petPortraitBox.Value.Left + 64, this.petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 510,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				List<ClickableComponent> list2 = this.colorPickerCCs;
				if (list2 != null && list2.Count > 0)
				{
					this.colorPickerCCs[0].upNeighborID = 511;
					this.colorPickerCCs[0].upNeighborImmutable = true;
				}
			}
			this._shouldShowBackButton = true;
			if (this.source == CharacterCustomization.Source.Dresser || this.source == CharacterCustomization.Source.Wizard || this.source == CharacterCustomization.Source.ClothesDye)
			{
				this._shouldShowBackButton = false;
			}
			if (this.source == CharacterCustomization.Source.Dresser || this.source == CharacterCustomization.Source.Wizard || this._isDyeMenu)
			{
				this.nameBoxCC.visible = false;
				this.farmnameBoxCC.visible = false;
				this.favThingBoxCC.visible = false;
				this.farmLabel.visible = false;
				this.nameLabel.visible = false;
				this.favoriteLabel.visible = false;
			}
			if (this.source == CharacterCustomization.Source.Wizard)
			{
				this.nameLabel.visible = true;
				this.nameBoxCC.visible = true;
				this.favThingBoxCC.visible = true;
				this.favoriteLabel.visible = true;
				this.favThingBoxCC.bounds.Y = this.farmnameBoxCC.bounds.Y;
				this.favoriteLabel.bounds.Y = this.farmLabel.bounds.Y;
				this.favThingBox.Y = this.farmnameBox.Y;
			}
			this.skipIntroButton.visible = creatingNewSave;
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x060027C2 RID: 10178 RVA: 0x001CAA14 File Offset: 0x001C8C14
		public virtual void LoadFarmTypeData()
		{
			List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
			this.farmTypeButtonNames.Add("Standard");
			this.farmTypeButtonNames.Add("Riverland");
			this.farmTypeButtonNames.Add("Forest");
			this.farmTypeButtonNames.Add("Hills");
			this.farmTypeButtonNames.Add("Wilderness");
			this.farmTypeButtonNames.Add("Four Corners");
			this.farmTypeButtonNames.Add("Beach");
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmStandard"));
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmFishing"));
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmForaging"));
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmMining"));
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmCombat"));
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmFourCorners"));
			this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmBeach"));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(0, 324, 22, 20)));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(22, 324, 22, 20)));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(44, 324, 22, 20)));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(66, 324, 22, 20)));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(88, 324, 22, 20)));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(0, 345, 22, 20)));
			this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(22, 345, 22, 20)));
			if (farm_types != null)
			{
				foreach (ModFarmType farm_type in farm_types)
				{
					this.farmTypeButtonNames.Add("ModFarm_" + farm_type.Id);
					this.farmTypeHoverText.Add(this.GetFarmTypeTooltip(farm_type.TooltipStringPath));
					if (farm_type.IconTexture != null)
					{
						Texture2D texture = Game1.content.Load<Texture2D>(farm_type.IconTexture);
						this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(texture, new Rectangle(0, 0, 22, 20)));
					}
					else
					{
						this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(1, 324, 22, 20)));
					}
				}
			}
			this._farmPages = 1;
			if (farm_types != null)
			{
				this._farmPages = (int)Math.Floor((double)((float)(this.farmTypeButtonNames.Count - 1) / 12f)) + 1;
			}
		}

		// Token: 0x060027C3 RID: 10179 RVA: 0x001CAD38 File Offset: 0x001C8F38
		public virtual void RefreshFarmTypeButtons()
		{
			this.farmTypeButtons.Clear();
			Point baseFarmButton = new Point(this.xPositionOnScreen + this.width + 4 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth);
			int index = this._currentFarmPage * 12;
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 88, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 531,
					downNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = 537
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 176, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 532,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 264, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 533,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 352, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 534,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 440, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 535,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 528, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 545,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 88, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 546,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 176, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 547,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 264, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 548,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 352, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 549,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 440, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 550,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			if (index < this.farmTypeButtonNames.Count)
			{
				this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 528, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f, false)
				{
					myID = 551,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				index++;
			}
			this.farmTypePreviousPageButton = null;
			this.farmTypeNextPageButton = null;
			if (this._currentFarmPage > 0)
			{
				this.farmTypePreviousPageButton = new ClickableTextureComponent("", new Rectangle(baseFarmButton.X - 64 + 16, baseFarmButton.Y + 352 + 12, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 647,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
			if (this._currentFarmPage < this._farmPages - 1)
			{
				this.farmTypeNextPageButton = new ClickableTextureComponent("", new Rectangle(baseFarmButton.X + 172, baseFarmButton.Y + 352 + 12, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 647,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
		}

		// Token: 0x060027C4 RID: 10180 RVA: 0x001CB7A8 File Offset: 0x001C99A8
		public override void snapToDefaultClickableComponent()
		{
			if (this.showingCoopHelp)
			{
				this.currentlySnappedComponent = base.getComponentWithID(626);
			}
			else
			{
				this.currentlySnappedComponent = base.getComponentWithID(521);
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060027C5 RID: 10181 RVA: 0x001CB7DC File Offset: 0x001C99DC
		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (this.currentlySnappedComponent != null)
			{
				if (b <= Buttons.DPadRight)
				{
					if (b == Buttons.DPadLeft)
					{
						goto IL_2F4;
					}
					if (b != Buttons.DPadRight)
					{
						return;
					}
				}
				else
				{
					if (b == Buttons.LeftThumbstickLeft)
					{
						goto IL_2F4;
					}
					if (b != Buttons.LeftThumbstickRight)
					{
						return;
					}
				}
				switch (this.currentlySnappedComponent.myID)
				{
				case 522:
					this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
					this.eyeColorPicker.changeHue(1);
					this.eyeColorPicker.Dirty = true;
					this._sliderOpTarget = this.eyeColorPicker;
					this._sliderAction = this._recolorEyesAction;
					return;
				case 523:
					this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
					this.eyeColorPicker.changeSaturation(1);
					this.eyeColorPicker.Dirty = true;
					this._sliderOpTarget = this.eyeColorPicker;
					this._sliderAction = this._recolorEyesAction;
					return;
				case 524:
					this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
					this.eyeColorPicker.changeValue(1);
					this.eyeColorPicker.Dirty = true;
					this._sliderOpTarget = this.eyeColorPicker;
					this._sliderAction = this._recolorEyesAction;
					return;
				case 525:
					this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
					this.hairColorPicker.changeHue(1);
					this.hairColorPicker.Dirty = true;
					this._sliderOpTarget = this.hairColorPicker;
					this._sliderAction = this._recolorHairAction;
					return;
				case 526:
					this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
					this.hairColorPicker.changeSaturation(1);
					this.hairColorPicker.Dirty = true;
					this._sliderOpTarget = this.hairColorPicker;
					this._sliderAction = this._recolorHairAction;
					return;
				case 527:
					this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
					this.hairColorPicker.changeValue(1);
					this.hairColorPicker.Dirty = true;
					this._sliderOpTarget = this.hairColorPicker;
					this._sliderAction = this._recolorHairAction;
					return;
				case 528:
					this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
					this.pantsColorPicker.changeHue(1);
					this.pantsColorPicker.Dirty = true;
					this._sliderOpTarget = this.pantsColorPicker;
					this._sliderAction = this._recolorPantsAction;
					return;
				case 529:
					this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
					this.pantsColorPicker.changeSaturation(1);
					this.pantsColorPicker.Dirty = true;
					this._sliderOpTarget = this.pantsColorPicker;
					this._sliderAction = this._recolorPantsAction;
					return;
				case 530:
					this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
					this.pantsColorPicker.changeValue(1);
					this.pantsColorPicker.Dirty = true;
					this._sliderOpTarget = this.pantsColorPicker;
					this._sliderAction = this._recolorPantsAction;
					return;
				default:
					return;
				}
				IL_2F4:
				switch (this.currentlySnappedComponent.myID)
				{
				case 522:
					this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
					this.eyeColorPicker.changeHue(-1);
					this.eyeColorPicker.Dirty = true;
					this._sliderOpTarget = this.eyeColorPicker;
					this._sliderAction = this._recolorEyesAction;
					return;
				case 523:
					this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
					this.eyeColorPicker.changeSaturation(-1);
					this.eyeColorPicker.Dirty = true;
					this._sliderOpTarget = this.eyeColorPicker;
					this._sliderAction = this._recolorEyesAction;
					return;
				case 524:
					this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
					this.eyeColorPicker.changeValue(-1);
					this.eyeColorPicker.Dirty = true;
					this._sliderOpTarget = this.eyeColorPicker;
					this._sliderAction = this._recolorEyesAction;
					return;
				case 525:
					this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
					this.hairColorPicker.changeHue(-1);
					this.hairColorPicker.Dirty = true;
					this._sliderOpTarget = this.hairColorPicker;
					this._sliderAction = this._recolorHairAction;
					return;
				case 526:
					this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
					this.hairColorPicker.changeSaturation(-1);
					this.hairColorPicker.Dirty = true;
					this._sliderOpTarget = this.hairColorPicker;
					this._sliderAction = this._recolorHairAction;
					return;
				case 527:
					this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
					this.hairColorPicker.changeValue(-1);
					this.hairColorPicker.Dirty = true;
					this._sliderOpTarget = this.hairColorPicker;
					this._sliderAction = this._recolorHairAction;
					return;
				case 528:
					this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
					this.pantsColorPicker.changeHue(-1);
					this.pantsColorPicker.Dirty = true;
					this._sliderOpTarget = this.pantsColorPicker;
					this._sliderAction = this._recolorPantsAction;
					return;
				case 529:
					this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
					this.pantsColorPicker.changeSaturation(-1);
					this.pantsColorPicker.Dirty = true;
					this._sliderOpTarget = this.pantsColorPicker;
					this._sliderAction = this._recolorPantsAction;
					return;
				case 530:
					this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
					this.pantsColorPicker.changeValue(-1);
					this.pantsColorPicker.Dirty = true;
					this._sliderOpTarget = this.pantsColorPicker;
					this._sliderAction = this._recolorPantsAction;
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x060027C6 RID: 10182 RVA: 0x001CBD98 File Offset: 0x001C9F98
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (this.currentlySnappedComponent != null)
			{
				if (button != Buttons.B)
				{
					if (button != Buttons.RightTrigger)
					{
						if (button != Buttons.LeftTrigger)
						{
							return;
						}
						int myID = this.currentlySnappedComponent.myID;
						if (myID - 512 <= 9)
						{
							this.selectionClick(this.currentlySnappedComponent.name, -1);
							return;
						}
					}
					else
					{
						int myID = this.currentlySnappedComponent.myID;
						if (myID - 512 <= 9)
						{
							this.selectionClick(this.currentlySnappedComponent.name, 1);
							return;
						}
					}
				}
				else if (this.showingCoopHelp)
				{
					this.receiveLeftClick(this.coopHelpOkButton.bounds.Center.X, this.coopHelpOkButton.bounds.Center.Y, true);
				}
			}
		}

		// Token: 0x060027C7 RID: 10183 RVA: 0x001CBE60 File Offset: 0x001CA060
		private void optionButtonClick(string name)
		{
			if (name.StartsWith("ModFarm_"))
			{
				if (this.source != CharacterCustomization.Source.NewGame && this.source != CharacterCustomization.Source.HostNewFarm)
				{
					goto IL_866;
				}
				List<ModFarmType> list = DataLoader.AdditionalFarms(Game1.content);
				string farmId = name.Substring("ModFarm_".Length);
				using (List<ModFarmType>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ModFarmType farmType = enumerator.Current;
						if (farmType.Id == farmId)
						{
							Game1.whichFarm = 7;
							Game1.whichModFarm = farmType;
							Game1.spawnMonstersAtNight = farmType.SpawnMonstersByDefault;
							break;
						}
					}
					goto IL_866;
				}
			}
			if (name != null)
			{
				switch (name.Length)
				{
				case 2:
					if (name == "OK")
					{
						if (!this.canLeaveMenu())
						{
							return;
						}
						if (this._itemToDye != null)
						{
							if (!Game1.player.IsEquippedItem(this._itemToDye))
							{
								Utility.CollectOrDrop(this._itemToDye);
							}
							this._itemToDye = null;
						}
						if (this.source == CharacterCustomization.Source.ClothesDye)
						{
							Game1.exitActiveMenu();
						}
						else
						{
							Game1.player.Name = Utility.FilterDirtyWordsIfStrictPlatform(this.nameBox.Text.Trim());
							Game1.player.displayName = Game1.player.Name;
							Game1.player.favoriteThing.Value = Utility.FilterDirtyWordsIfStrictPlatform(this.favThingBox.Text.Trim());
							Game1.player.isCustomized.Value = true;
							Game1.player.ConvertClothingOverrideToClothesItems();
							if (this.source == CharacterCustomization.Source.HostNewFarm)
							{
								Game1.multiplayerMode = 2;
							}
							try
							{
								if (Game1.player.Name != this.oldName)
								{
									int start = Game1.player.Name.IndexOf("[");
									int end = Game1.player.Name.IndexOf("]");
									if (start >= 0 && end > start)
									{
										ParsedItemData data = ItemRegistry.GetData(Game1.player.Name.Substring(start + 1, end - start - 1));
										string itemName = (data != null) ? data.DisplayName : null;
										if (itemName != null)
										{
											switch (Game1.random.Next(5))
											{
											case 0:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg1"), new Color(104, 214, 255));
												break;
											case 1:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg2", Lexicon.makePlural(itemName, false)), new Color(100, 50, 255));
												break;
											case 2:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg3", Lexicon.makePlural(itemName, false)), new Color(0, 220, 40));
												break;
											case 3:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg4"), new Color(0, 220, 40));
												DelayedAction.functionAfterDelay(delegate
												{
													Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg5"), new Color(104, 214, 255));
												}, 12000);
												break;
											case 4:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg6", Lexicon.getProperArticleForWord(itemName), itemName), new Color(100, 120, 255));
												break;
											}
										}
									}
								}
							}
							catch
							{
							}
							string changed_pet_name = null;
							if (this.petPortraitBox != null && Game1.IsMasterGame && Game1.gameMode == 3 && Game1.locations != null)
							{
								Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), false, false);
								if (pet != null && this.petHasChanges(pet))
								{
									pet.petType.Value = Game1.player.whichPetType;
									pet.whichBreed.Value = Game1.player.whichPetBreed;
									changed_pet_name = pet.getName();
								}
							}
							TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
							if (titleMenu != null)
							{
								titleMenu.createdNewCharacter(this.skipIntro);
							}
							else
							{
								Game1.exitActiveMenu();
								Intro intro = Game1.currentMinigame as Intro;
								if (intro != null)
								{
									intro.doneCreatingCharacter();
								}
								else
								{
									CharacterCustomization.Source source = this.source;
									if (source != CharacterCustomization.Source.Wizard)
									{
										if (source == CharacterCustomization.Source.ClothesDye)
										{
											Game1.playSound("yoba", null);
										}
									}
									else
									{
										if (changed_pet_name != null)
										{
											Game1.multiplayer.globalChatInfoMessage("Makeover_Pet", new string[]
											{
												Game1.player.Name,
												changed_pet_name
											});
										}
										else
										{
											Game1.multiplayer.globalChatInfoMessage("Makeover", new string[]
											{
												Game1.player.Name
											});
										}
										Game1.flashAlpha = 1f;
										Game1.playSound("yoba", null);
									}
								}
							}
						}
					}
					break;
				case 3:
				{
					char c = name[0];
					if (c != 'C')
					{
						if (c == 'D')
						{
							if (name == "Dog")
							{
								if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
								{
									Game1.player.whichPetType = "Dog";
								}
							}
						}
					}
					else if (name == "Cat")
					{
						if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
						{
							Game1.player.whichPetType = "Cat";
						}
					}
					break;
				}
				case 4:
					if (name == "Male")
					{
						Game1.player.changeGender(true);
						if (this.source != CharacterCustomization.Source.Wizard)
						{
							Game1.player.changeHairStyle(0);
						}
					}
					break;
				case 5:
				{
					char c = name[0];
					if (c != 'B')
					{
						if (c != 'C')
						{
							if (c == 'H')
							{
								if (name == "Hills")
								{
									if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
									{
										Game1.whichFarm = 3;
										Game1.whichModFarm = null;
										Game1.spawnMonstersAtNight = false;
									}
								}
							}
						}
						else if (name == "Close")
						{
							Game1.cabinsSeparate = false;
						}
					}
					else if (name == "Beach")
					{
						if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
						{
							Game1.whichFarm = 6;
							Game1.whichModFarm = null;
							Game1.spawnMonstersAtNight = false;
						}
					}
					break;
				}
				case 6:
				{
					char c = name[1];
					if (c != 'e')
					{
						if (c == 'o')
						{
							if (name == "Forest")
							{
								if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
								{
									Game1.whichFarm = 2;
									Game1.whichModFarm = null;
									Game1.spawnMonstersAtNight = false;
								}
							}
						}
					}
					else if (name == "Female")
					{
						Game1.player.changeGender(false);
						if (this.source != CharacterCustomization.Source.Wizard)
						{
							Game1.player.changeHairStyle(16);
						}
					}
					break;
				}
				case 8:
				{
					char c = name[1];
					if (c != 'e')
					{
						if (c == 't')
						{
							if (name == "Standard")
							{
								if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
								{
									Game1.whichFarm = 0;
									Game1.whichModFarm = null;
									Game1.spawnMonstersAtNight = false;
								}
							}
						}
					}
					else if (name == "Separate")
					{
						Game1.cabinsSeparate = true;
					}
					break;
				}
				case 9:
					if (name == "Riverland")
					{
						if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
						{
							Game1.whichFarm = 1;
							Game1.whichModFarm = null;
							Game1.spawnMonstersAtNight = false;
						}
					}
					break;
				case 10:
					if (name == "Wilderness")
					{
						if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
						{
							Game1.whichFarm = 4;
							Game1.whichModFarm = null;
							Game1.spawnMonstersAtNight = true;
						}
					}
					break;
				case 12:
					if (name == "Four Corners")
					{
						if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
						{
							Game1.whichFarm = 5;
							Game1.whichModFarm = null;
							Game1.spawnMonstersAtNight = false;
						}
					}
					break;
				}
			}
			IL_866:
			Game1.playSound("coin", null);
		}

		// Token: 0x060027C8 RID: 10184 RVA: 0x001CC71C File Offset: 0x001CA91C
		public bool petHasChanges(Pet pet)
		{
			return Game1.player.whichPetType != pet.petType.Value || Game1.player.whichPetBreed != pet.whichBreed.Value;
		}

		// Token: 0x060027C9 RID: 10185 RVA: 0x001CC75C File Offset: 0x001CA95C
		protected virtual string GetFarmTypeTooltip(string translationKey)
		{
			string text = Game1.content.LoadString(translationKey);
			string[] parts = text.Split('_', 2, StringSplitOptions.None);
			if (parts.Length == 1 || parts[1].Length == 0)
			{
				text = parts[0] + "_ ";
			}
			return text;
		}

		// Token: 0x060027CA RID: 10186 RVA: 0x001CC7A0 File Offset: 0x001CA9A0
		protected List<KeyValuePair<string, string>> GetPetTypesAndBreeds()
		{
			if (this._petTypesAndBreeds == null)
			{
				this._petTypesAndBreeds = new List<KeyValuePair<string, string>>();
				foreach (KeyValuePair<string, PetData> pair in Game1.petData)
				{
					if (!this.isModifyingExistingPet || !(Game1.player.whichPetType != pair.Key))
					{
						foreach (PetBreed breed in pair.Value.Breeds)
						{
							if (breed.CanBeChosenAtStart)
							{
								this._petTypesAndBreeds.Add(new KeyValuePair<string, string>(pair.Key, breed.Id));
							}
						}
					}
				}
			}
			return this._petTypesAndBreeds;
		}

		// Token: 0x060027CB RID: 10187 RVA: 0x001CC890 File Offset: 0x001CAA90
		private void selectionClick(string name, int change)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 3:
				{
					char c = name[0];
					if (c != 'A')
					{
						if (c != 'P')
						{
							return;
						}
						if (!(name == "Pet"))
						{
							return;
						}
						List<KeyValuePair<string, string>> pets = this.GetPetTypesAndBreeds();
						int index = pets.IndexOf(new KeyValuePair<string, string>(Game1.player.whichPetType, Game1.player.whichPetBreed));
						if (index == -1)
						{
							index = 0;
						}
						else
						{
							index += change;
						}
						if (index < 0)
						{
							index = pets.Count - 1;
						}
						else if (index >= pets.Count)
						{
							index = 0;
						}
						KeyValuePair<string, string> selectedPetType = pets[index];
						Game1.player.whichPetType = selectedPetType.Key;
						Game1.player.whichPetBreed = selectedPetType.Value;
						Game1.playSound("coin", null);
					}
					else
					{
						if (!(name == "Acc"))
						{
							return;
						}
						Game1.player.changeAccessory(Game1.player.accessory.Value + change);
						Game1.playSound("purchase", null);
						return;
					}
					break;
				}
				case 4:
				{
					char c = name[0];
					if (c != 'H')
					{
						if (c != 'S')
						{
							return;
						}
						if (!(name == "Skin"))
						{
							return;
						}
						Game1.player.changeSkinColor(Game1.player.skin.Value + change, false);
						Game1.playSound("skeletonStep", null);
						return;
					}
					else
					{
						if (!(name == "Hair"))
						{
							return;
						}
						List<int> all_hairs = Farmer.GetAllHairstyleIndices();
						int current_index = all_hairs.IndexOf(Game1.player.hair.Value);
						current_index += change;
						if (current_index >= all_hairs.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_hairs.Count - 1;
						}
						Game1.player.changeHairStyle(all_hairs[current_index]);
						Game1.playSound("grassyStep", null);
						return;
					}
					break;
				}
				case 5:
					if (!(name == "Shirt"))
					{
						return;
					}
					Game1.player.rotateShirt(change, this.GetValidShirtIds());
					Game1.playSound("coin", null);
					return;
				case 6:
					if (!(name == "Cabins"))
					{
						return;
					}
					if ((Game1.startingCabins != 0 || change >= 0) && (Game1.startingCabins != Game1.multiplayer.playerLimit - 1 || change <= 0))
					{
						Game1.playSound("axchop", null);
					}
					Game1.startingCabins += change;
					Game1.startingCabins = Math.Max(0, Math.Min(Game1.multiplayer.playerLimit - 1, Game1.startingCabins));
					return;
				case 7:
					if (!(name == "Wallets"))
					{
						return;
					}
					if (Game1.player.team.useSeparateWallets.Value)
					{
						Game1.playSound("coin", null);
						Game1.player.team.useSeparateWallets.Value = false;
						return;
					}
					Game1.playSound("coin", null);
					Game1.player.team.useSeparateWallets.Value = true;
					return;
				case 8:
					break;
				case 9:
					if (!(name == "Direction"))
					{
						return;
					}
					this._displayFarmer.faceDirection((this._displayFarmer.FacingDirection - change + 4) % 4);
					this._displayFarmer.FarmerSprite.StopAnimation();
					this._displayFarmer.completelyStopAnimatingOrDoingAction();
					Game1.playSound("pickUpItem", null);
					return;
				case 10:
					if (!(name == "Difficulty"))
					{
						return;
					}
					if (Game1.player.difficultyModifier < 1f && change < 0)
					{
						Game1.playSound("breathout", null);
						Game1.player.difficultyModifier += 0.25f;
						return;
					}
					if (Game1.player.difficultyModifier > 0.25f && change > 0)
					{
						Game1.playSound("batFlap", null);
						Game1.player.difficultyModifier -= 0.25f;
						return;
					}
					break;
				case 11:
					if (!(name == "Pants Style"))
					{
						return;
					}
					Game1.player.rotatePantStyle(change, this.GetValidPantsIds());
					Game1.playSound("coin", null);
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x060027CC RID: 10188 RVA: 0x001CCCF7 File Offset: 0x001CAEF7
		public void ShowAdvancedOptions()
		{
			base.AddDependency();
			(TitleMenu.subMenu = new AdvancedGameOptions()).exitFunction = delegate()
			{
				TitleMenu.subMenu = this;
				base.RemoveDependency();
				this.ResetComponents();
				this.populateClickableComponentList();
				if (Game1.options.SnappyMenus)
				{
					this.setCurrentlySnappedComponentTo(636);
					this.snapCursorToCurrentSnappedComponent();
				}
			};
		}

		// Token: 0x060027CD RID: 10189 RVA: 0x001CCD1C File Offset: 0x001CAF1C
		public override bool readyToClose()
		{
			if (this.showingCoopHelp)
			{
				return false;
			}
			if (Game1.lastCursorMotionWasMouse)
			{
				using (List<ClickableTextureComponent>.Enumerator enumerator = this.farmTypeButtons.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
						{
							return false;
						}
					}
				}
			}
			return base.readyToClose();
		}

		// Token: 0x060027CE RID: 10190 RVA: 0x001CCD98 File Offset: 0x001CAF98
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.showingCoopHelp)
			{
				if (this.coopHelpOkButton != null && this.coopHelpOkButton.containsPoint(x, y))
				{
					this.showingCoopHelp = false;
					Game1.playSound("bigDeSelect", null);
					if (Game1.options.SnappyMenus)
					{
						this.currentlySnappedComponent = this.coopHelpButton;
						this.snapCursorToCurrentSnappedComponent();
					}
				}
				if (this.coopHelpScreen == 0 && this.coopHelpRightButton != null && this.coopHelpRightButton.containsPoint(x, y))
				{
					this.coopHelpScreen++;
					this.coopHelpString = Game1.parseText(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString2").Replace("^", Environment.NewLine), Game1.dialogueFont, this.width + 384 - IClickableMenu.borderWidth * 2);
					Game1.playSound("shwip", null);
				}
				if (this.coopHelpScreen == 1 && this.coopHelpLeftButton != null && this.coopHelpLeftButton.containsPoint(x, y))
				{
					this.coopHelpScreen--;
					string rawText = string.Format(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.multiplayer.playerLimit - 1);
					this.coopHelpString = Game1.parseText(rawText, Game1.dialogueFont, this.width + 384 - IClickableMenu.borderWidth * 2);
					Game1.playSound("shwip", null);
				}
				return;
			}
			if (this.genderButtons.Count > 0)
			{
				foreach (ClickableComponent c in this.genderButtons)
				{
					if (c.containsPoint(x, y))
					{
						this.optionButtonClick(c.name);
						c.scale -= 0.5f;
						c.scale = Math.Max(3.5f, c.scale);
					}
				}
			}
			if (this.farmTypeNextPageButton != null && this.farmTypeNextPageButton.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this._currentFarmPage++;
				this.RefreshFarmTypeButtons();
			}
			else if (this.farmTypePreviousPageButton != null && this.farmTypePreviousPageButton.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this._currentFarmPage--;
				this.RefreshFarmTypeButtons();
			}
			else if (this.farmTypeButtons.Count > 0)
			{
				foreach (ClickableComponent c2 in this.farmTypeButtons)
				{
					if (c2.containsPoint(x, y) && !c2.name.Contains("Gray"))
					{
						this.optionButtonClick(c2.name);
						c2.scale -= 0.5f;
						c2.scale = Math.Max(3.5f, c2.scale);
					}
				}
			}
			if (this.cabinLayoutButtons.Count > 0)
			{
				foreach (ClickableComponent c3 in this.cabinLayoutButtons)
				{
					if (Game1.startingCabins > 0 && c3.containsPoint(x, y))
					{
						this.optionButtonClick(c3.name);
						c3.scale -= 0.5f;
						c3.scale = Math.Max(3.5f, c3.scale);
					}
				}
			}
			if (this.leftSelectionButtons.Count > 0)
			{
				foreach (ClickableComponent c4 in this.leftSelectionButtons)
				{
					if (c4.containsPoint(x, y))
					{
						this.selectionClick(c4.name, -1);
						if (c4.scale != 0f)
						{
							c4.scale -= 0.25f;
							c4.scale = Math.Max(0.75f, c4.scale);
						}
					}
				}
			}
			if (this.rightSelectionButtons.Count > 0)
			{
				foreach (ClickableComponent c5 in this.rightSelectionButtons)
				{
					if (c5.containsPoint(x, y))
					{
						this.selectionClick(c5.name, 1);
						if (c5.scale != 0f)
						{
							c5.scale -= 0.25f;
							c5.scale = Math.Max(0.75f, c5.scale);
						}
					}
				}
			}
			if (this.okButton.containsPoint(x, y) && this.canLeaveMenu())
			{
				this.optionButtonClick(this.okButton.name);
				this.okButton.scale -= 0.25f;
				this.okButton.scale = Math.Max(0.75f, this.okButton.scale);
			}
			if (this.hairColorPicker != null && this.hairColorPicker.containsPoint(x, y))
			{
				Color color = this.hairColorPicker.click(x, y);
				if (this.source == CharacterCustomization.Source.DyePots)
				{
					if (Game1.player.CanDyeShirt())
					{
						Game1.player.shirtItem.Value.clothesColor.Value = color;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				}
				else
				{
					Game1.player.changeHairColor(color);
				}
				this.lastHeldColorPicker = this.hairColorPicker;
			}
			else if (this.pantsColorPicker != null && this.pantsColorPicker.containsPoint(x, y))
			{
				Color color2 = this.pantsColorPicker.click(x, y);
				CharacterCustomization.Source source = this.source;
				if (source != CharacterCustomization.Source.ClothesDye)
				{
					if (source == CharacterCustomization.Source.DyePots)
					{
						if (Game1.player.CanDyePants())
						{
							Game1.player.pantsItem.Value.clothesColor.Value = color2;
							Game1.player.FarmerRenderer.MarkSpriteDirty();
							this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
						}
					}
					else
					{
						Game1.player.changePantsColor(color2);
					}
				}
				else
				{
					this.DyeItem(color2);
				}
				this.lastHeldColorPicker = this.pantsColorPicker;
			}
			else if (this.eyeColorPicker != null && this.eyeColorPicker.containsPoint(x, y))
			{
				Game1.player.changeEyeColor(this.eyeColorPicker.click(x, y));
				this.lastHeldColorPicker = this.eyeColorPicker;
			}
			if (this.source != CharacterCustomization.Source.Dresser && this.source != CharacterCustomization.Source.ClothesDye && this.source != CharacterCustomization.Source.DyePots)
			{
				this.nameBox.Update();
				if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
				{
					this.farmnameBox.Update();
				}
				else
				{
					this.farmnameBox.Text = Game1.MasterPlayer.farmName.Value;
				}
				this.favThingBox.Update();
				if ((this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm) && this.skipIntroButton.containsPoint(x, y))
				{
					Game1.playSound("drumkit6", null);
					this.skipIntroButton.sourceRect.X = ((this.skipIntroButton.sourceRect.X == 227) ? 236 : 227);
					this.skipIntro = !this.skipIntro;
				}
			}
			if (this.coopHelpButton != null && this.coopHelpButton.containsPoint(x, y))
			{
				if (Game1.options.SnappyMenus)
				{
					this.currentlySnappedComponent = this.coopHelpOkButton;
					this.snapCursorToCurrentSnappedComponent();
				}
				Game1.playSound("bigSelect", null);
				this.showingCoopHelp = true;
				this.coopHelpScreen = 0;
				string rawText2 = string.Format(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.multiplayer.playerLimit - 1);
				this.coopHelpString = Game1.parseText(rawText2, Game1.dialogueFont, this.width + 384 - IClickableMenu.borderWidth * 2);
				this.helpStringSize = Game1.dialogueFont.MeasureString(this.coopHelpString);
				this.coopHelpRightButton.bounds.Y = this.yPositionOnScreen + (int)this.helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
				this.coopHelpRightButton.bounds.X = this.xPositionOnScreen + (int)this.helpStringSize.X - IClickableMenu.borderWidth * 5;
				this.coopHelpLeftButton.bounds.Y = this.yPositionOnScreen + (int)this.helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
				this.coopHelpLeftButton.bounds.X = this.xPositionOnScreen - IClickableMenu.borderWidth * 4;
			}
			if (this.advancedOptionsButton != null && this.advancedOptionsButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6", null);
				this.ShowAdvancedOptions();
			}
			if (this.randomButton.containsPoint(x, y))
			{
				string sound = "drumkit6";
				if (this.timesRandom > 0)
				{
					switch (Game1.random.Next(15))
					{
					case 0:
						sound = "drumkit1";
						break;
					case 1:
						sound = "dirtyHit";
						break;
					case 2:
						sound = "axchop";
						break;
					case 3:
						sound = "hoeHit";
						break;
					case 4:
						sound = "fishSlap";
						break;
					case 5:
						sound = "drumkit6";
						break;
					case 6:
						sound = "drumkit5";
						break;
					case 7:
						sound = "drumkit6";
						break;
					case 8:
						sound = "junimoMeep1";
						break;
					case 9:
						sound = "coin";
						break;
					case 10:
						sound = "axe";
						break;
					case 11:
						sound = "hammer";
						break;
					case 12:
						sound = "drumkit2";
						break;
					case 13:
						sound = "drumkit4";
						break;
					case 14:
						sound = "drumkit3";
						break;
					}
				}
				Game1.playSound(sound, null);
				this.timesRandom++;
				if (this.accLabel != null && this.accLabel.visible)
				{
					if (Game1.random.NextDouble() < 0.33)
					{
						if (Game1.player.IsMale)
						{
							if (Game1.random.NextDouble() < 0.33)
							{
								if (Game1.random.NextDouble() < 0.8)
								{
									Game1.player.changeAccessory(Game1.random.Next(7));
								}
								else
								{
									Game1.player.changeAccessory(Game1.random.Next(19, 21));
								}
							}
							else if (Game1.random.NextDouble() < 0.33)
							{
								Game1.player.changeAccessory(Game1.random.Choose(new int[]
								{
									25,
									14,
									17,
									10,
									9
								}));
							}
							else if (Game1.random.NextDouble() < 0.1)
							{
								Game1.player.changeAccessory(Game1.random.Next(19));
							}
						}
						else if (Game1.random.NextDouble() < 0.33)
						{
							Game1.player.changeAccessory(Game1.random.Next(6, 19));
						}
						else if (Game1.random.NextDouble() < 0.5)
						{
							Game1.player.changeAccessory(Game1.random.Choose(23, 27, 28));
						}
						else
						{
							Game1.player.changeAccessory(Game1.random.Choose(new int[]
							{
								25,
								14,
								17,
								10,
								9
							}));
						}
					}
					else
					{
						Game1.player.changeAccessory(-1);
					}
				}
				if (this.skinLabel != null && this.skinLabel.visible)
				{
					Game1.player.changeSkinColor(Game1.random.Next(6), false);
					if (Game1.random.NextDouble() < 0.15)
					{
						Game1.player.changeSkinColor(Game1.random.Next(24), false);
					}
				}
				if (this.hairLabel != null && this.hairLabel.visible)
				{
					if (Game1.player.IsMale)
					{
						Game1.player.changeHairStyle(Game1.random.NextBool() ? Game1.random.Next(16) : Game1.random.Next(108, 118));
					}
					else
					{
						Game1.player.changeHairStyle(Game1.random.Next(16, 41));
					}
					Color hairColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
					if (Game1.random.NextBool())
					{
						hairColor.R /= 2;
						hairColor.G /= 2;
						hairColor.B /= 2;
					}
					if (Game1.random.NextBool())
					{
						hairColor.R = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						hairColor.G = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						hairColor.B = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						if (hairColor.B > hairColor.R)
						{
							hairColor.B = (byte)Math.Max(0, (int)(hairColor.B - 50));
						}
						if (hairColor.B > hairColor.G)
						{
							hairColor.B = (byte)Math.Max(0, (int)(hairColor.B - 50));
						}
						if (hairColor.G > hairColor.R)
						{
							hairColor.G = (byte)Math.Max(0, (int)(hairColor.R - 50));
						}
						hairColor.R = (byte)Math.Min(255, (int)(hairColor.R + 50));
						hairColor.G = (byte)Math.Min(255, (int)(hairColor.G + 50));
					}
					else if (Game1.random.NextDouble() < 0.33)
					{
						hairColor = new Color(Game1.random.Next(80, 130), Game1.random.Next(35, 70), 0);
					}
					if (hairColor.R < 100 && hairColor.G < 100 && hairColor.B < 100 && Game1.random.NextDouble() < 0.8)
					{
						hairColor = Utility.getBlendedColor(hairColor, Color.Tan);
					}
					if (Game1.player.hasDarkSkin() && Game1.random.NextDouble() < 0.5)
					{
						hairColor = new Color(Game1.random.Next(50, 100), Game1.random.Next(25, 40), 0);
					}
					Game1.player.changeHairColor(hairColor);
					this.hairColorPicker.setColor(hairColor);
				}
				if (this.shirtLabel != null && this.shirtLabel.visible)
				{
					string shirtSelection = "";
					IList<string> validShirtIds = this.GetValidShirtIds();
					ISet<string> except;
					if (!Game1.player.IsMale)
					{
						except = new HashSet<string>();
					}
					else
					{
						HashSet<string> hashSet = new HashSet<string>();
						hashSet.Add("1056");
						hashSet.Add("1057");
						hashSet.Add("1070");
						hashSet.Add("1046");
						hashSet.Add("1040");
						hashSet.Add("1060");
						hashSet.Add("1090");
						hashSet.Add("1051");
						hashSet.Add("1082");
						hashSet.Add("1107");
						hashSet.Add("1080");
						hashSet.Add("1083");
						hashSet.Add("1092");
						hashSet.Add("1072");
						hashSet.Add("1076");
						except = hashSet;
						hashSet.Add("1041");
					}
					Utility.TryGetRandomExcept<string>(validShirtIds, except, Game1.random, out shirtSelection);
					Game1.player.changeShirt(shirtSelection);
				}
				if (this.pantsStyleLabel != null && this.pantsStyleLabel.visible)
				{
					Color pantsColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
					if (Game1.random.NextBool())
					{
						pantsColor.R /= 2;
						pantsColor.G /= 2;
						pantsColor.B /= 2;
					}
					if (Game1.random.NextBool())
					{
						pantsColor.R = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						pantsColor.G = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						pantsColor.B = (byte)Game1.random.Next(15, 50);
					}
					int shirtIndex = Game1.player.GetShirtIndex();
					if (shirtIndex <= 72)
					{
						if (shirtIndex <= 7)
						{
							if (shirtIndex != 0 && shirtIndex != 7)
							{
								goto IL_1229;
							}
						}
						else
						{
							if (shirtIndex == 50)
							{
								pantsColor = new Color(226, 133, 160);
								goto IL_1229;
							}
							switch (shirtIndex)
							{
							case 67:
							case 72:
								pantsColor = new Color(108, 134, 224);
								goto IL_1229;
							case 68:
								goto IL_11F2;
							case 69:
							case 70:
								goto IL_1229;
							case 71:
								break;
							default:
								goto IL_1229;
							}
						}
						pantsColor = new Color(34, 29, 173);
						goto IL_1229;
					}
					if (shirtIndex <= 88)
					{
						if (shirtIndex != 79)
						{
							if (shirtIndex != 88)
							{
								goto IL_1229;
							}
							goto IL_11F2;
						}
					}
					else if (shirtIndex != 99 && shirtIndex != 103)
					{
						goto IL_1229;
					}
					pantsColor = new Color(55, 55, 60);
					goto IL_1229;
					IL_11F2:
					pantsColor = new Color(119, 215, 130);
					IL_1229:
					Game1.player.changePantsColor(pantsColor);
					this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
				}
				if (this.eyeColorPicker != null)
				{
					Color eyeColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
					eyeColor.R /= 2;
					eyeColor.G /= 2;
					eyeColor.B /= 2;
					if (Game1.random.NextBool())
					{
						eyeColor.R = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						eyeColor.G = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						eyeColor.B = (byte)Game1.random.Next(15, 50);
					}
					if (Game1.random.NextBool())
					{
						if (eyeColor.B > eyeColor.R)
						{
							eyeColor.B = (byte)Math.Max(0, (int)(eyeColor.B - 50));
						}
						if (eyeColor.B > eyeColor.G)
						{
							eyeColor.B = (byte)Math.Max(0, (int)(eyeColor.B - 50));
						}
						if (eyeColor.G > eyeColor.R)
						{
							eyeColor.G = (byte)Math.Max(0, (int)(eyeColor.R - 50));
						}
					}
					Game1.player.changeEyeColor(eyeColor);
					this.eyeColorPicker.setColor(Game1.player.newEyeColor.Value);
				}
				this.randomButton.scale = 3.5f;
			}
		}

		// Token: 0x060027CF RID: 10191 RVA: 0x001CE1C8 File Offset: 0x001CC3C8
		public List<string> GetValidClothingIds<TData>(string equippedId, IDictionary<string, TData> data, Func<TData, bool> canChooseDuringCharacterCustomization)
		{
			List<string> validIds = new List<string>();
			foreach (KeyValuePair<string, TData> pair in data)
			{
				if (pair.Key == equippedId || canChooseDuringCharacterCustomization(pair.Value))
				{
					validIds.Add(pair.Key);
				}
			}
			return validIds;
		}

		// Token: 0x060027D0 RID: 10192 RVA: 0x001CE23C File Offset: 0x001CC43C
		public List<string> GetValidPantsIds()
		{
			return this.GetValidClothingIds<PantsData>(Game1.player.pants.Value, Game1.pantsData, (PantsData data) => data.CanChooseDuringCharacterCustomization);
		}

		// Token: 0x060027D1 RID: 10193 RVA: 0x001CE277 File Offset: 0x001CC477
		public List<string> GetValidShirtIds()
		{
			return this.GetValidClothingIds<ShirtData>(Game1.player.shirt.Value, Game1.shirtData, (ShirtData data) => data.CanChooseDuringCharacterCustomization);
		}

		// Token: 0x060027D2 RID: 10194 RVA: 0x001CE2B4 File Offset: 0x001CC4B4
		public override void leftClickHeld(int x, int y)
		{
			this.colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (this.colorPickerTimer <= 0)
			{
				if (this.lastHeldColorPicker != null && !Game1.options.SnappyMenus)
				{
					if (this.lastHeldColorPicker.Equals(this.hairColorPicker))
					{
						Color color = this.hairColorPicker.clickHeld(x, y);
						if (this.source == CharacterCustomization.Source.DyePots)
						{
							if (Game1.player.CanDyeShirt())
							{
								Game1.player.shirtItem.Value.clothesColor.Value = color;
								Game1.player.FarmerRenderer.MarkSpriteDirty();
								this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
							}
						}
						else
						{
							Game1.player.changeHairColor(color);
						}
					}
					if (this.lastHeldColorPicker.Equals(this.pantsColorPicker))
					{
						Color color2 = this.pantsColorPicker.clickHeld(x, y);
						CharacterCustomization.Source source = this.source;
						if (source != CharacterCustomization.Source.ClothesDye)
						{
							if (source == CharacterCustomization.Source.DyePots)
							{
								if (Game1.player.CanDyePants())
								{
									Game1.player.pantsItem.Value.clothesColor.Value = color2;
									Game1.player.FarmerRenderer.MarkSpriteDirty();
									this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
								}
							}
							else
							{
								Game1.player.changePantsColor(color2);
							}
						}
						else
						{
							this.DyeItem(color2);
						}
					}
					if (this.lastHeldColorPicker.Equals(this.eyeColorPicker))
					{
						Game1.player.changeEyeColor(this.eyeColorPicker.clickHeld(x, y));
					}
				}
				this.colorPickerTimer = 100;
			}
		}

		// Token: 0x060027D3 RID: 10195 RVA: 0x001CE43F File Offset: 0x001CC63F
		public override void releaseLeftClick(int x, int y)
		{
			ColorPicker colorPicker = this.hairColorPicker;
			if (colorPicker != null)
			{
				colorPicker.releaseClick();
			}
			ColorPicker colorPicker2 = this.pantsColorPicker;
			if (colorPicker2 != null)
			{
				colorPicker2.releaseClick();
			}
			ColorPicker colorPicker3 = this.eyeColorPicker;
			if (colorPicker3 != null)
			{
				colorPicker3.releaseClick();
			}
			this.lastHeldColorPicker = null;
		}

		// Token: 0x060027D4 RID: 10196 RVA: 0x001CE47C File Offset: 0x001CC67C
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Tab)
			{
				switch (this.source)
				{
				case CharacterCustomization.Source.NewGame:
				case CharacterCustomization.Source.HostNewFarm:
					if (this.nameBox.Selected)
					{
						this.farmnameBox.SelectMe();
						this.nameBox.Selected = false;
					}
					else if (this.farmnameBox.Selected)
					{
						this.farmnameBox.Selected = false;
						this.favThingBox.SelectMe();
					}
					else
					{
						this.favThingBox.Selected = false;
						this.nameBox.SelectMe();
					}
					break;
				case CharacterCustomization.Source.NewFarmhand:
					if (this.nameBox.Selected)
					{
						this.favThingBox.SelectMe();
						this.nameBox.Selected = false;
					}
					else
					{
						this.favThingBox.Selected = false;
						this.nameBox.SelectMe();
					}
					break;
				}
			}
			if (Game1.options.SnappyMenus && !Game1.options.doesInputListContain(Game1.options.menuButton, key) && Game1.GetKeyboardState().GetPressedKeys().Length == 0)
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x060027D5 RID: 10197 RVA: 0x001CE590 File Offset: 0x001CC790
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.hoverTitle = "";
			foreach (ClickableComponent clickableComponent in this.leftSelectionButtons)
			{
				ClickableTextureComponent c = (ClickableTextureComponent)clickableComponent;
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
				}
				else
				{
					c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
				}
				if (c.name.Equals("Cabins") && Game1.startingCabins == 0)
				{
					c.scale = 0f;
				}
			}
			foreach (ClickableComponent clickableComponent2 in this.rightSelectionButtons)
			{
				ClickableTextureComponent c2 = (ClickableTextureComponent)clickableComponent2;
				if (c2.containsPoint(x, y))
				{
					c2.scale = Math.Min(c2.scale + 0.02f, c2.baseScale + 0.1f);
				}
				else
				{
					c2.scale = Math.Max(c2.scale - 0.02f, c2.baseScale);
				}
				if (c2.name.Equals("Cabins") && Game1.startingCabins == Game1.multiplayer.playerLimit - 1)
				{
					c2.scale = 0f;
				}
			}
			if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
			{
				foreach (ClickableTextureComponent c3 in this.farmTypeButtons)
				{
					if (c3.containsPoint(x, y) && !c3.name.Contains("Gray"))
					{
						c3.scale = Math.Min(c3.scale + 0.02f, c3.baseScale + 0.1f);
						this.hoverTitle = c3.hoverText.Split('_', StringSplitOptions.None)[0];
						this.hoverText = c3.hoverText.Split('_', StringSplitOptions.None)[1];
					}
					else
					{
						c3.scale = Math.Max(c3.scale - 0.02f, c3.baseScale);
						if (c3.name.Contains("Gray") && c3.containsPoint(x, y))
						{
							this.hoverText = "Reach level 10 " + Game1.content.LoadString("Strings\\UI:Character_" + c3.name.Split('_', StringSplitOptions.None)[1]) + " to unlock.";
						}
					}
				}
			}
			foreach (ClickableComponent clickableComponent3 in this.genderButtons)
			{
				ClickableTextureComponent c4 = (ClickableTextureComponent)clickableComponent3;
				if (c4.containsPoint(x, y))
				{
					c4.scale = Math.Min(c4.scale + 0.05f, c4.baseScale + 0.5f);
				}
				else
				{
					c4.scale = Math.Max(c4.scale - 0.05f, c4.baseScale);
				}
			}
			if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
			{
				foreach (ClickableTextureComponent c5 in this.cabinLayoutButtons)
				{
					if (Game1.startingCabins > 0 && c5.containsPoint(x, y))
					{
						c5.scale = Math.Min(c5.scale + 0.05f, c5.baseScale + 0.5f);
						this.hoverText = c5.hoverText;
					}
					else
					{
						c5.scale = Math.Max(c5.scale - 0.05f, c5.baseScale);
					}
				}
			}
			if (this.okButton.containsPoint(x, y) && this.canLeaveMenu())
			{
				this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.1f);
			}
			else
			{
				this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
			}
			if (this.coopHelpButton != null)
			{
				if (this.coopHelpButton.containsPoint(x, y))
				{
					this.coopHelpButton.scale = Math.Min(this.coopHelpButton.scale + 0.05f, this.coopHelpButton.baseScale + 0.5f);
					this.hoverText = this.coopHelpButton.hoverText;
				}
				else
				{
					this.coopHelpButton.scale = Math.Max(this.coopHelpButton.scale - 0.05f, this.coopHelpButton.baseScale);
				}
			}
			if (this.coopHelpOkButton != null)
			{
				if (this.coopHelpOkButton.containsPoint(x, y))
				{
					this.coopHelpOkButton.scale = Math.Min(this.coopHelpOkButton.scale + 0.025f, this.coopHelpOkButton.baseScale + 0.2f);
				}
				else
				{
					this.coopHelpOkButton.scale = Math.Max(this.coopHelpOkButton.scale - 0.025f, this.coopHelpOkButton.baseScale);
				}
			}
			if (this.coopHelpRightButton != null)
			{
				if (this.coopHelpRightButton.containsPoint(x, y))
				{
					this.coopHelpRightButton.scale = Math.Min(this.coopHelpRightButton.scale + 0.025f, this.coopHelpRightButton.baseScale + 0.2f);
				}
				else
				{
					this.coopHelpRightButton.scale = Math.Max(this.coopHelpRightButton.scale - 0.025f, this.coopHelpRightButton.baseScale);
				}
			}
			if (this.coopHelpLeftButton != null)
			{
				if (this.coopHelpLeftButton.containsPoint(x, y))
				{
					this.coopHelpLeftButton.scale = Math.Min(this.coopHelpLeftButton.scale + 0.025f, this.coopHelpLeftButton.baseScale + 0.2f);
				}
				else
				{
					this.coopHelpLeftButton.scale = Math.Max(this.coopHelpLeftButton.scale - 0.025f, this.coopHelpLeftButton.baseScale);
				}
			}
			ClickableTextureComponent clickableTextureComponent = this.advancedOptionsButton;
			if (clickableTextureComponent != null)
			{
				clickableTextureComponent.tryHover(x, y, 0.1f);
			}
			ClickableTextureComponent clickableTextureComponent2 = this.farmTypeNextPageButton;
			if (clickableTextureComponent2 != null)
			{
				clickableTextureComponent2.tryHover(x, y, 0.1f);
			}
			ClickableTextureComponent clickableTextureComponent3 = this.farmTypePreviousPageButton;
			if (clickableTextureComponent3 != null)
			{
				clickableTextureComponent3.tryHover(x, y, 0.1f);
			}
			this.randomButton.tryHover(x, y, 0.25f);
			this.randomButton.tryHover(x, y, 0.25f);
			if ((this.hairColorPicker != null && this.hairColorPicker.containsPoint(x, y)) || (this.pantsColorPicker != null && this.pantsColorPicker.containsPoint(x, y)) || (this.eyeColorPicker != null && this.eyeColorPicker.containsPoint(x, y)))
			{
				Game1.SetFreeCursorDrag();
			}
			this.nameBox.Hover(x, y);
			this.farmnameBox.Hover(x, y);
			this.favThingBox.Hover(x, y);
			this.skipIntroButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x060027D6 RID: 10198 RVA: 0x001CED4C File Offset: 0x001CCF4C
		public bool canLeaveMenu()
		{
			return this.source == CharacterCustomization.Source.ClothesDye || this.source == CharacterCustomization.Source.DyePots || (Game1.player.Name.Length > 0 && Game1.player.farmName.Length > 0 && Game1.player.favoriteThing.Length > 0);
		}

		// Token: 0x060027D7 RID: 10199 RVA: 0x001CEDA8 File Offset: 0x001CCFA8
		private string getNameOfDifficulty()
		{
			if (Game1.player.difficultyModifier < 0.5f)
			{
				return this.superDiffString;
			}
			if (Game1.player.difficultyModifier < 0.75f)
			{
				return this.hardDiffString;
			}
			if (Game1.player.difficultyModifier < 1f)
			{
				return this.toughDiffString;
			}
			return this.normalDiffString;
		}

		// Token: 0x060027D8 RID: 10200 RVA: 0x001CEE04 File Offset: 0x001CD004
		public override void draw(SpriteBatch b)
		{
			if (this.showingCoopHelp)
			{
				IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - 192, this.yPositionOnScreen + 64, (int)this.helpStringSize.X + IClickableMenu.borderWidth * 2, (int)this.helpStringSize.Y + IClickableMenu.borderWidth * 2, Color.White);
				Utility.drawTextWithShadow(b, this.coopHelpString, Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth - 192), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + 64)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				ClickableTextureComponent clickableTextureComponent = this.coopHelpOkButton;
				if (clickableTextureComponent != null)
				{
					clickableTextureComponent.draw(b, Color.White, 0.95f, 0, 0, 0);
				}
				ClickableTextureComponent clickableTextureComponent2 = this.coopHelpRightButton;
				if (clickableTextureComponent2 != null)
				{
					clickableTextureComponent2.draw(b, Color.White, 0.95f, 0, 0, 0);
				}
				ClickableTextureComponent clickableTextureComponent3 = this.coopHelpLeftButton;
				if (clickableTextureComponent3 != null)
				{
					clickableTextureComponent3.draw(b, Color.White, 0.95f, 0, 0, 0);
				}
				base.drawMouse(b, false, -1);
				return;
			}
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
			if (this.source == CharacterCustomization.Source.HostNewFarm)
			{
				IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - 256 + 4 - ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 25 : 0), this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 68, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 320 : 256, 512, Color.White);
				foreach (ClickableTextureComponent c in this.cabinLayoutButtons)
				{
					c.draw(b, Color.White * ((Game1.startingCabins > 0) ? 1f : 0.5f), 0.9f, 0, 0, 0);
					if (Game1.startingCabins > 0 && ((c.name.Equals("Close") && !Game1.cabinsSeparate) || (c.name.Equals("Separate") && Game1.cabinsSeparate)))
					{
						b.Draw(Game1.mouseCursors, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
					}
				}
			}
			b.Draw(Game1.daybg, new Vector2((float)this.portraitBox.X, (float)this.portraitBox.Y), Color.White);
			foreach (ClickableComponent clickableComponent in this.genderButtons)
			{
				ClickableTextureComponent c2 = (ClickableTextureComponent)clickableComponent;
				if (c2.visible)
				{
					c2.draw(b);
					if ((c2.name.Equals("Male") && Game1.player.IsMale) || (c2.name.Equals("Female") && !Game1.player.IsMale))
					{
						b.Draw(Game1.mouseCursors, c2.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
					}
				}
			}
			if (this.nameBoxCC.visible)
			{
				Game1.player.Name = this.nameBox.Text;
			}
			if (this.favThingBoxCC.visible)
			{
				Game1.player.favoriteThing.Value = this.favThingBox.Text;
			}
			if (this.farmnameBoxCC.visible)
			{
				Game1.player.farmName.Value = this.farmnameBox.Text;
			}
			if (this.source == CharacterCustomization.Source.NewFarmhand)
			{
				Game1.player.farmName.Value = Game1.MasterPlayer.farmName.Value;
			}
			foreach (ClickableComponent clickableComponent2 in this.leftSelectionButtons)
			{
				((ClickableTextureComponent)clickableComponent2).draw(b);
			}
			foreach (ClickableComponent c3 in this.labels)
			{
				if (c3.visible)
				{
					string sub = "";
					float offset = 0f;
					float subYOffset = 0f;
					Color color = Game1.textColor;
					if (c3 == this.nameLabel)
					{
						string name = Game1.player.Name;
						color = ((name != null && name.Length < 1) ? Color.Red : Game1.textColor);
					}
					else if (c3 == this.farmLabel)
					{
						color = ((Game1.player.farmName.Value != null && Game1.player.farmName.Length < 1) ? Color.Red : Game1.textColor);
					}
					else if (c3 == this.favoriteLabel)
					{
						color = ((Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Length < 1) ? Color.Red : Game1.textColor);
					}
					else if (c3 == this.shirtLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						sub = Game1.player.GetShirtIndex().ToString();
						int id;
						if (int.TryParse(sub, out id))
						{
							sub = (id + 1).ToString();
						}
					}
					else if (c3 == this.skinLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						sub = ((Game1.player.skin.Value + 1).ToString() ?? "");
					}
					else if (c3 == this.hairLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						if (!c3.name.Contains("Color"))
						{
							sub = ((Farmer.GetAllHairstyleIndices().IndexOf(Game1.player.hair.Value) + 1).ToString() ?? "");
						}
					}
					else if (c3 == this.accLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						sub = ((Game1.player.accessory.Value + 2).ToString() ?? "");
					}
					else if (c3 == this.pantsStyleLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						sub = Game1.player.GetPantsIndex().ToString();
						int id2;
						if (int.TryParse(sub, out id2))
						{
							sub = (id2 + 1).ToString();
						}
					}
					else if (c3 == this.startingCabinsLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						sub = ((Game1.startingCabins == 0 && this.noneString != null) ? this.noneString : (Game1.startingCabins.ToString() ?? ""));
						subYOffset = 4f;
					}
					else if (c3 == this.difficultyModifierLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						subYOffset = 4f;
						sub = this.getNameOfDifficulty();
					}
					else if (c3 == this.separateWalletLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
						subYOffset = 4f;
						sub = (Game1.player.team.useSeparateWallets.Value ? this.separateWalletString : this.sharedWalletString);
					}
					else
					{
						color = Game1.textColor;
					}
					Utility.drawTextWithShadow(b, c3.name, Game1.smallFont, new Vector2((float)c3.bounds.X + offset, (float)c3.bounds.Y), color, 1f, -1f, -1, -1, 1f, 3);
					if (sub.Length > 0)
					{
						Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2((float)(c3.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f, (float)(c3.bounds.Y + 32) + subYOffset), color, 1f, -1f, -1, -1, 1f, 3);
					}
				}
			}
			foreach (ClickableComponent clickableComponent3 in this.rightSelectionButtons)
			{
				((ClickableTextureComponent)clickableComponent3).draw(b);
			}
			if (this.farmTypeButtons.Count > 0)
			{
				IClickableMenu.drawTextureBox(b, this.farmTypeButtons[0].bounds.X - 16, this.farmTypeButtons[0].bounds.Y - 20, 220, 564, Color.White);
				for (int i = 0; i < this.farmTypeButtons.Count; i++)
				{
					this.farmTypeButtons[i].draw(b, this.farmTypeButtons[i].name.Contains("Gray") ? (Color.Black * 0.5f) : Color.White, 0.88f, 0, 0, 0);
					if (this.farmTypeButtons[i].name.Contains("Gray"))
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(this.farmTypeButtons[i].bounds.Center.X - 12), (float)(this.farmTypeButtons[i].bounds.Center.Y - 8)), new Rectangle?(new Rectangle(107, 442, 7, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
					}
					bool farm_is_selected = false;
					int index = i + this._currentFarmPage * 6;
					if (Game1.whichFarm == 7)
					{
						if ("ModFarm_" + Game1.whichModFarm.Id == this.farmTypeButtonNames[index])
						{
							farm_is_selected = true;
						}
					}
					else if (Game1.whichFarm == index)
					{
						farm_is_selected = true;
					}
					if (farm_is_selected)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.farmTypeButtons[i].bounds.X, this.farmTypeButtons[i].bounds.Y - 4, this.farmTypeButtons[i].bounds.Width, this.farmTypeButtons[i].bounds.Height + 8, Color.White, 4f, false, -1f);
					}
				}
				ClickableTextureComponent clickableTextureComponent4 = this.farmTypeNextPageButton;
				if (clickableTextureComponent4 != null)
				{
					clickableTextureComponent4.draw(b);
				}
				ClickableTextureComponent clickableTextureComponent5 = this.farmTypePreviousPageButton;
				if (clickableTextureComponent5 != null)
				{
					clickableTextureComponent5.draw(b);
				}
			}
			PetData petData;
			if (this.petPortraitBox != null && Pet.TryGetData(Game1.MasterPlayer.whichPetType, out petData))
			{
				Texture2D texture = null;
				Rectangle sourceRect = Rectangle.Empty;
				foreach (PetBreed breed in petData.Breeds)
				{
					if (breed.Id == Game1.MasterPlayer.whichPetBreed)
					{
						texture = Game1.content.Load<Texture2D>(breed.IconTexture);
						sourceRect = breed.IconSourceRect;
						break;
					}
				}
				if (texture != null)
				{
					b.Draw(texture, this.petPortraitBox.Value, new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
				}
			}
			ClickableTextureComponent clickableTextureComponent6 = this.advancedOptionsButton;
			if (clickableTextureComponent6 != null)
			{
				clickableTextureComponent6.draw(b);
			}
			if (this.canLeaveMenu())
			{
				this.okButton.draw(b, Color.White, 0.75f, 0, 0, 0);
			}
			else
			{
				this.okButton.draw(b, Color.White, 0.75f, 0, 0, 0);
				this.okButton.draw(b, Color.Black * 0.5f, 0.751f, 0, 0, 0);
			}
			ClickableTextureComponent clickableTextureComponent7 = this.coopHelpButton;
			if (clickableTextureComponent7 != null)
			{
				clickableTextureComponent7.draw(b, Color.White, 0.75f, 0, 0, 0);
			}
			ColorPicker colorPicker = this.hairColorPicker;
			if (colorPicker != null)
			{
				colorPicker.draw(b);
			}
			ColorPicker colorPicker2 = this.pantsColorPicker;
			if (colorPicker2 != null)
			{
				colorPicker2.draw(b);
			}
			ColorPicker colorPicker3 = this.eyeColorPicker;
			if (colorPicker3 != null)
			{
				colorPicker3.draw(b);
			}
			if (this.source != CharacterCustomization.Source.Dresser && this.source != CharacterCustomization.Source.DyePots && this.source != CharacterCustomization.Source.ClothesDye)
			{
				this.nameBox.Draw(b, true);
				this.favThingBox.Draw(b, true);
			}
			if (this.farmnameBoxCC.visible)
			{
				this.farmnameBox.Draw(b, true);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_FarmNameSuffix"), Game1.smallFont, new Vector2((float)(this.farmnameBox.X + this.farmnameBox.Width + 8), (float)(this.farmnameBox.Y + 12)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			if (this.skipIntroButton != null && this.skipIntroButton.visible)
			{
				this.skipIntroButton.draw(b);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.smallFont, new Vector2((float)(this.skipIntroButton.bounds.X + this.skipIntroButton.bounds.Width + 8), (float)(this.skipIntroButton.bounds.Y + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			if (this.advancedCCHighlightTimer > 0f)
			{
				b.Draw(Game1.mouseCursors, this.advancedOptionsButton.getVector2() + new Vector2(4f, 84f), new Rectangle?(new Rectangle(128 + ((this.advancedCCHighlightTimer % 500f < 250f) ? 16 : 0), 208, 16, 16)), Color.White * Math.Min(1f, this.advancedCCHighlightTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
			}
			this.randomButton.draw(b);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			this._displayFarmer.FarmerRenderer.draw(b, this._displayFarmer.FarmerSprite.CurrentAnimationFrame, this._displayFarmer.FarmerSprite.CurrentFrame, this._displayFarmer.FarmerSprite.SourceRect, new Vector2((float)(this.portraitBox.Center.X - 32), (float)(this.portraitBox.Bottom - 160)), Vector2.Zero, 0.8f, Color.White, 0f, 1f, this._displayFarmer);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			string text = this.hoverTitle;
			if (text != null && text.Length > 0)
			{
				int width = Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverTitle).X, 256);
				IClickableMenu.drawHoverText(b, Game1.parseText(this.hoverText, Game1.smallFont, width), Game1.smallFont, 0, 0, -1, this.hoverTitle, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x060027D9 RID: 10201 RVA: 0x001CFEFC File Offset: 0x001CE0FC
		public override void emergencyShutDown()
		{
			if (this._itemToDye != null)
			{
				if (!Game1.player.IsEquippedItem(this._itemToDye))
				{
					Utility.CollectOrDrop(this._itemToDye);
				}
				this._itemToDye = null;
			}
			base.emergencyShutDown();
		}

		// Token: 0x060027DA RID: 10202 RVA: 0x001CFF34 File Offset: 0x001CE134
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region != b.region)
			{
				return false;
			}
			if (this.advancedOptionsButton != null && this.backButton != null && a == this.advancedOptionsButton && b == this.backButton)
			{
				return false;
			}
			if (this.source == CharacterCustomization.Source.Wizard)
			{
				if (a == this.favThingBoxCC && b.myID >= 522 && b.myID <= 530)
				{
					return false;
				}
				if (b == this.favThingBoxCC && a.myID >= 522 && a.myID <= 530)
				{
					return false;
				}
			}
			if (this.source == CharacterCustomization.Source.Wizard)
			{
				if (a.name == "Direction" && b.name == "Pet")
				{
					return false;
				}
				if (b.name == "Direction" && a.name == "Pet")
				{
					return false;
				}
			}
			if (this.randomButton != null)
			{
				if (direction != 0)
				{
					if (direction == 3)
					{
						if (b == this.randomButton && a.name == "Direction")
						{
							return false;
						}
					}
					else
					{
						if (a == this.randomButton && b.name != "Direction")
						{
							return false;
						}
						if (b == this.randomButton && a.name != "Direction")
						{
							return false;
						}
					}
				}
				if (a.myID == 622 && direction == 1 && (b == this.nameBoxCC || b == this.favThingBoxCC || b == this.farmnameBoxCC))
				{
					return false;
				}
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x060027DB RID: 10203 RVA: 0x001D00C0 File Offset: 0x001CE2C0
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.showingCoopHelp)
			{
				this.backButton.visible = false;
				int num = this.coopHelpScreen;
				if (num != 0)
				{
					if (num == 1)
					{
						this.coopHelpRightButton.visible = false;
						this.coopHelpLeftButton.visible = true;
					}
				}
				else
				{
					this.coopHelpRightButton.visible = true;
					this.coopHelpLeftButton.visible = false;
				}
			}
			else
			{
				this.backButton.visible = this._shouldShowBackButton;
			}
			if (this._sliderOpTarget != null)
			{
				Color col = this._sliderOpTarget.getSelectedColor();
				if (this._sliderOpTarget.Dirty && this._sliderOpTarget.LastColor == col)
				{
					this._sliderAction();
					this._sliderOpTarget.LastColor = this._sliderOpTarget.getSelectedColor();
					this._sliderOpTarget.Dirty = false;
					this._sliderOpTarget = null;
				}
				else
				{
					this._sliderOpTarget.LastColor = col;
				}
			}
			if (this.advancedCCHighlightTimer > 0f)
			{
				this.advancedCCHighlightTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x04001909 RID: 6409
		public const int region_okbutton = 505;

		// Token: 0x0400190A RID: 6410
		public const int region_skipIntroButton = 506;

		// Token: 0x0400190B RID: 6411
		public const int region_randomButton = 507;

		// Token: 0x0400190C RID: 6412
		public const int region_male = 508;

		// Token: 0x0400190D RID: 6413
		public const int region_female = 509;

		// Token: 0x0400190E RID: 6414
		public const int region_dog = 510;

		// Token: 0x0400190F RID: 6415
		public const int region_cat = 511;

		// Token: 0x04001910 RID: 6416
		public const int region_shirtLeft = 512;

		// Token: 0x04001911 RID: 6417
		public const int region_shirtRight = 513;

		// Token: 0x04001912 RID: 6418
		public const int region_hairLeft = 514;

		// Token: 0x04001913 RID: 6419
		public const int region_hairRight = 515;

		// Token: 0x04001914 RID: 6420
		public const int region_accLeft = 516;

		// Token: 0x04001915 RID: 6421
		public const int region_accRight = 517;

		// Token: 0x04001916 RID: 6422
		public const int region_skinLeft = 518;

		// Token: 0x04001917 RID: 6423
		public const int region_skinRight = 519;

		// Token: 0x04001918 RID: 6424
		public const int region_directionLeft = 520;

		// Token: 0x04001919 RID: 6425
		public const int region_directionRight = 521;

		// Token: 0x0400191A RID: 6426
		public const int region_cabinsLeft = 621;

		// Token: 0x0400191B RID: 6427
		public const int region_cabinsRight = 622;

		// Token: 0x0400191C RID: 6428
		public const int region_cabinsClose = 623;

		// Token: 0x0400191D RID: 6429
		public const int region_cabinsSeparate = 624;

		// Token: 0x0400191E RID: 6430
		public const int region_coopHelp = 625;

		// Token: 0x0400191F RID: 6431
		public const int region_coopHelpOK = 626;

		// Token: 0x04001920 RID: 6432
		public const int region_difficultyLeft = 627;

		// Token: 0x04001921 RID: 6433
		public const int region_difficultyRight = 628;

		// Token: 0x04001922 RID: 6434
		public const int region_petLeft = 627;

		// Token: 0x04001923 RID: 6435
		public const int region_petRight = 628;

		// Token: 0x04001924 RID: 6436
		public const int region_pantsLeft = 629;

		// Token: 0x04001925 RID: 6437
		public const int region_pantsRight = 630;

		// Token: 0x04001926 RID: 6438
		public const int region_walletsLeft = 631;

		// Token: 0x04001927 RID: 6439
		public const int region_walletsRight = 632;

		// Token: 0x04001928 RID: 6440
		public const int region_coopHelpRight = 633;

		// Token: 0x04001929 RID: 6441
		public const int region_coopHelpLeft = 634;

		// Token: 0x0400192A RID: 6442
		public const int region_coopHelpButtons = 635;

		// Token: 0x0400192B RID: 6443
		public const int region_advancedOptions = 636;

		// Token: 0x0400192C RID: 6444
		public const int region_colorPicker1 = 522;

		// Token: 0x0400192D RID: 6445
		public const int region_colorPicker2 = 523;

		// Token: 0x0400192E RID: 6446
		public const int region_colorPicker3 = 524;

		// Token: 0x0400192F RID: 6447
		public const int region_colorPicker4 = 525;

		// Token: 0x04001930 RID: 6448
		public const int region_colorPicker5 = 526;

		// Token: 0x04001931 RID: 6449
		public const int region_colorPicker6 = 527;

		// Token: 0x04001932 RID: 6450
		public const int region_colorPicker7 = 528;

		// Token: 0x04001933 RID: 6451
		public const int region_colorPicker8 = 529;

		// Token: 0x04001934 RID: 6452
		public const int region_colorPicker9 = 530;

		// Token: 0x04001935 RID: 6453
		public const int region_farmSelection1 = 531;

		// Token: 0x04001936 RID: 6454
		public const int region_farmSelection2 = 532;

		// Token: 0x04001937 RID: 6455
		public const int region_farmSelection3 = 533;

		// Token: 0x04001938 RID: 6456
		public const int region_farmSelection4 = 534;

		// Token: 0x04001939 RID: 6457
		public const int region_farmSelection5 = 535;

		// Token: 0x0400193A RID: 6458
		public const int region_farmSelection6 = 545;

		// Token: 0x0400193B RID: 6459
		public const int region_farmSelection7 = 546;

		// Token: 0x0400193C RID: 6460
		public const int region_farmSelection8 = 547;

		// Token: 0x0400193D RID: 6461
		public const int region_farmSelection9 = 548;

		// Token: 0x0400193E RID: 6462
		public const int region_farmSelection10 = 549;

		// Token: 0x0400193F RID: 6463
		public const int region_farmSelection11 = 550;

		// Token: 0x04001940 RID: 6464
		public const int region_farmSelection12 = 551;

		// Token: 0x04001941 RID: 6465
		public const int region_farmSelectionLeft = 647;

		// Token: 0x04001942 RID: 6466
		public const int region_farmSelectionRight = 648;

		// Token: 0x04001943 RID: 6467
		public const int region_nameBox = 536;

		// Token: 0x04001944 RID: 6468
		public const int region_farmNameBox = 537;

		// Token: 0x04001945 RID: 6469
		public const int region_favThingBox = 538;

		// Token: 0x04001946 RID: 6470
		public const int colorPickerTimerDelay = 100;

		// Token: 0x04001947 RID: 6471
		public const int widthOfMultiplayerArea = 256;

		// Token: 0x04001948 RID: 6472
		private int colorPickerTimer;

		// Token: 0x04001949 RID: 6473
		public ColorPicker pantsColorPicker;

		// Token: 0x0400194A RID: 6474
		public ColorPicker hairColorPicker;

		// Token: 0x0400194B RID: 6475
		public ColorPicker eyeColorPicker;

		// Token: 0x0400194C RID: 6476
		public List<ClickableComponent> labels = new List<ClickableComponent>();

		// Token: 0x0400194D RID: 6477
		public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();

		// Token: 0x0400194E RID: 6478
		public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

		// Token: 0x0400194F RID: 6479
		public List<ClickableComponent> genderButtons = new List<ClickableComponent>();

		// Token: 0x04001950 RID: 6480
		public List<ClickableTextureComponent> farmTypeButtons = new List<ClickableTextureComponent>();

		// Token: 0x04001951 RID: 6481
		public ClickableTextureComponent farmTypeNextPageButton;

		// Token: 0x04001952 RID: 6482
		public ClickableTextureComponent farmTypePreviousPageButton;

		// Token: 0x04001953 RID: 6483
		private List<string> farmTypeButtonNames = new List<string>();

		// Token: 0x04001954 RID: 6484
		private List<string> farmTypeHoverText = new List<string>();

		// Token: 0x04001955 RID: 6485
		private List<KeyValuePair<Texture2D, Rectangle>> farmTypeIcons = new List<KeyValuePair<Texture2D, Rectangle>>();

		// Token: 0x04001956 RID: 6486
		protected int _currentFarmPage;

		// Token: 0x04001957 RID: 6487
		protected int _farmPages;

		// Token: 0x04001958 RID: 6488
		public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

		// Token: 0x04001959 RID: 6489
		public List<ClickableTextureComponent> cabinLayoutButtons = new List<ClickableTextureComponent>();

		// Token: 0x0400195A RID: 6490
		public ClickableTextureComponent okButton;

		// Token: 0x0400195B RID: 6491
		public ClickableTextureComponent skipIntroButton;

		// Token: 0x0400195C RID: 6492
		public ClickableTextureComponent randomButton;

		// Token: 0x0400195D RID: 6493
		public ClickableTextureComponent coopHelpButton;

		// Token: 0x0400195E RID: 6494
		public ClickableTextureComponent coopHelpOkButton;

		// Token: 0x0400195F RID: 6495
		public ClickableTextureComponent coopHelpRightButton;

		// Token: 0x04001960 RID: 6496
		public ClickableTextureComponent coopHelpLeftButton;

		// Token: 0x04001961 RID: 6497
		public ClickableTextureComponent advancedOptionsButton;

		// Token: 0x04001962 RID: 6498
		private TextBox nameBox;

		// Token: 0x04001963 RID: 6499
		private TextBox farmnameBox;

		// Token: 0x04001964 RID: 6500
		private TextBox favThingBox;

		// Token: 0x04001965 RID: 6501
		private bool skipIntro;

		// Token: 0x04001966 RID: 6502
		public bool isModifyingExistingPet;

		// Token: 0x04001967 RID: 6503
		public bool showingCoopHelp;

		// Token: 0x04001968 RID: 6504
		public int coopHelpScreen;

		// Token: 0x04001969 RID: 6505
		public CharacterCustomization.Source source;

		// Token: 0x0400196A RID: 6506
		private Vector2 helpStringSize;

		// Token: 0x0400196B RID: 6507
		private string hoverText;

		// Token: 0x0400196C RID: 6508
		private string hoverTitle;

		// Token: 0x0400196D RID: 6509
		private string coopHelpString;

		// Token: 0x0400196E RID: 6510
		private string noneString;

		// Token: 0x0400196F RID: 6511
		private string normalDiffString;

		// Token: 0x04001970 RID: 6512
		private string toughDiffString;

		// Token: 0x04001971 RID: 6513
		private string hardDiffString;

		// Token: 0x04001972 RID: 6514
		private string superDiffString;

		// Token: 0x04001973 RID: 6515
		private string sharedWalletString;

		// Token: 0x04001974 RID: 6516
		private string separateWalletString;

		// Token: 0x04001975 RID: 6517
		public ClickableComponent nameBoxCC;

		// Token: 0x04001976 RID: 6518
		public ClickableComponent farmnameBoxCC;

		// Token: 0x04001977 RID: 6519
		public ClickableComponent favThingBoxCC;

		// Token: 0x04001978 RID: 6520
		public ClickableComponent backButton;

		// Token: 0x04001979 RID: 6521
		private ClickableComponent nameLabel;

		// Token: 0x0400197A RID: 6522
		private ClickableComponent farmLabel;

		// Token: 0x0400197B RID: 6523
		private ClickableComponent favoriteLabel;

		// Token: 0x0400197C RID: 6524
		private ClickableComponent shirtLabel;

		// Token: 0x0400197D RID: 6525
		private ClickableComponent skinLabel;

		// Token: 0x0400197E RID: 6526
		private ClickableComponent hairLabel;

		// Token: 0x0400197F RID: 6527
		private ClickableComponent accLabel;

		// Token: 0x04001980 RID: 6528
		private ClickableComponent pantsStyleLabel;

		// Token: 0x04001981 RID: 6529
		private ClickableComponent startingCabinsLabel;

		// Token: 0x04001982 RID: 6530
		private ClickableComponent cabinLayoutLabel;

		// Token: 0x04001983 RID: 6531
		private ClickableComponent separateWalletLabel;

		// Token: 0x04001984 RID: 6532
		private ClickableComponent difficultyModifierLabel;

		// Token: 0x04001985 RID: 6533
		private ColorPicker _sliderOpTarget;

		// Token: 0x04001986 RID: 6534
		private Action _sliderAction;

		// Token: 0x04001987 RID: 6535
		private readonly Action _recolorEyesAction;

		// Token: 0x04001988 RID: 6536
		private readonly Action _recolorPantsAction;

		// Token: 0x04001989 RID: 6537
		private readonly Action _recolorHairAction;

		// Token: 0x0400198A RID: 6538
		protected Clothing _itemToDye;

		// Token: 0x0400198B RID: 6539
		protected bool _shouldShowBackButton = true;

		// Token: 0x0400198C RID: 6540
		protected bool _isDyeMenu;

		// Token: 0x0400198D RID: 6541
		protected Farmer _displayFarmer;

		// Token: 0x0400198E RID: 6542
		public Rectangle portraitBox;

		// Token: 0x0400198F RID: 6543
		public Rectangle? petPortraitBox;

		// Token: 0x04001990 RID: 6544
		public string oldName = "";

		// Token: 0x04001991 RID: 6545
		private bool _multiplayerServer;

		// Token: 0x04001992 RID: 6546
		private float advancedCCHighlightTimer;

		// Token: 0x04001993 RID: 6547
		protected List<KeyValuePair<string, string>> _petTypesAndBreeds;

		// Token: 0x04001994 RID: 6548
		private ColorPicker lastHeldColorPicker;

		// Token: 0x04001995 RID: 6549
		private int timesRandom;

		// Token: 0x020005F3 RID: 1523
		public enum Source
		{
			// Token: 0x04002E2A RID: 11818
			NewGame,
			// Token: 0x04002E2B RID: 11819
			NewFarmhand,
			// Token: 0x04002E2C RID: 11820
			Wizard,
			// Token: 0x04002E2D RID: 11821
			HostNewFarm,
			// Token: 0x04002E2E RID: 11822
			Dresser,
			// Token: 0x04002E2F RID: 11823
			ClothesDye,
			// Token: 0x04002E30 RID: 11824
			DyePots
		}
	}
}

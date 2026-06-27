using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;

namespace StardewValley.Menus
{
	// Token: 0x0200029C RID: 668
	public class ProfileMenu : IClickableMenu
	{
		// Token: 0x06002BA0 RID: 11168 RVA: 0x00211D74 File Offset: 0x0020FF74
		public ProfileMenu(SocialPage.SocialEntry subject, List<SocialPage.SocialEntry> allSocialEntries) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).Y, 1280, 720, true)
		{
			this._printedName = "";
			this._characterEntrancePosition = new Vector2(0f, 4f);
			foreach (SocialPage.SocialEntry entry in allSocialEntries)
			{
				if (entry.Character is NPC && entry.IsMet)
				{
					this.SocialEntries.Add(entry);
				}
			}
			this._profileItems = new List<ProfileItem>();
			this.clickableProfileItems = new List<ClickableComponent>();
			this.UpdateButtons();
			this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			this._SetCharacter(subject);
		}

		// Token: 0x06002BA1 RID: 11169 RVA: 0x00211EC8 File Offset: 0x002100C8
		protected void _SetCharacter(SocialPage.SocialEntry entry)
		{
			this.Current = entry;
			this._sortedItems = new Dictionary<int, List<Item>>();
			NPC npcData = this.Current.Character as NPC;
			if (npcData != null)
			{
				CharacterData data = npcData.GetData();
				string assetName = "Characters/" + npcData.getTextureName();
				try
				{
					this._animatedSprite = new AnimatedSprite(assetName, 0, (data != null) ? data.Size.X : 16, (data != null) ? data.Size.Y : 32);
				}
				catch (Exception ex)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(84, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Profile menu couldn't load sprite '");
					defaultInterpolatedStringHandler.AppendFormatted(assetName);
					defaultInterpolatedStringHandler.AppendLiteral("' for NPC '");
					defaultInterpolatedStringHandler.AppendFormatted(npcData.Name);
					defaultInterpolatedStringHandler.AppendLiteral("', defaulting to their current sprite.");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
					this._animatedSprite = npcData.Sprite.Clone();
					this._animatedSprite.tempSpriteHeight = -1;
					this._animatedSprite.SpriteWidth = ((data != null) ? data.Size.X : this._animatedSprite.SpriteWidth);
					this._animatedSprite.SpriteHeight = ((data != null) ? data.Size.Y : this._animatedSprite.SpriteHeight);
				}
				this._animatedSprite.faceDirection(2);
				foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					if (Game1.player.hasGiftTasteBeenRevealed(npcData, itemData.ItemId))
					{
						Object item = ItemRegistry.Create<Object>(itemData.QualifiedItemId, 1, 0, false);
						if (!item.IsBreakableStone())
						{
							for (int i = 0; i < ProfileMenu.itemCategories.Length; i++)
							{
								string categoryName = ProfileMenu.itemCategories[i].categoryName;
								if (!(categoryName == "Profile_Gift_Category_LikedGifts"))
								{
									if (!(categoryName == "Profile_Gift_Category_Misc"))
									{
										if (ProfileMenu.itemCategories[i].validCategories.Contains(item.Category))
										{
											List<Item> categoryItems;
											if (!this._sortedItems.TryGetValue(i, out categoryItems))
											{
												categoryItems = (this._sortedItems[i] = new List<Item>());
											}
											categoryItems.Add(item);
										}
									}
									else
									{
										bool isAccountedFor = false;
										foreach (ProfileMenu.ProfileItemCategory category in ProfileMenu.itemCategories)
										{
											if (category.validCategories != null && category.validCategories.Contains(item.Category))
											{
												isAccountedFor = true;
												break;
											}
										}
										if (!isAccountedFor)
										{
											List<Item> categoryItems2;
											if (!this._sortedItems.TryGetValue(i, out categoryItems2))
											{
												categoryItems2 = (this._sortedItems[i] = new List<Item>());
											}
											categoryItems2.Add(item);
										}
									}
								}
								else
								{
									int giftTaste = npcData.getGiftTasteForThisItem(item);
									if (giftTaste == 2 || giftTaste == 0)
									{
										List<Item> categoryItems3;
										if (!this._sortedItems.TryGetValue(i, out categoryItems3))
										{
											categoryItems3 = (this._sortedItems[i] = new List<Item>());
										}
										categoryItems3.Add(item);
									}
								}
							}
						}
					}
				}
				Gender gender = this.Current.Gender;
				bool isDatable = this.Current.IsDatable;
				bool housemate = this.Current.IsRoommateForCurrentPlayer();
				this._status = "";
				if (isDatable || housemate)
				{
					string text;
					if (housemate)
					{
						text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Male"));
					}
					else if (this.Current.IsMarriedToCurrentPlayer())
					{
						text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband"));
					}
					else if (this.Current.IsMarriedToAnyone())
					{
						text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc"));
					}
					else if (!Game1.player.isMarriedOrRoommates() && this.Current.IsDatingCurrentPlayer())
					{
						text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend"));
					}
					else if (this.Current.IsDivorcedFromCurrentPlayer())
					{
						text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband"));
					}
					else
					{
						text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male"));
					}
					text = Game1.parseText(text, Game1.smallFont, this.width);
					string status = text.Replace("(", "").Replace(")", "").Replace("（", "").Replace("）", "");
					status = Utility.capitalizeFirstLetter(status);
					this._status = status;
				}
				this._UpdateList();
			}
			this._directionChangeTimer = 2000f;
			this._currentDirection = 2;
			this._hiddenEmoteTimer = -1f;
		}

		// Token: 0x06002BA2 RID: 11170 RVA: 0x00212414 File Offset: 0x00210614
		public void ChangeCharacter(int offset)
		{
			int index = this.SocialEntries.IndexOf(this.Current);
			if (index == -1)
			{
				if (this.SocialEntries.Count > 0)
				{
					this._SetCharacter(this.SocialEntries[0]);
				}
				return;
			}
			for (index += offset; index < 0; index += this.SocialEntries.Count)
			{
			}
			while (index >= this.SocialEntries.Count)
			{
				index -= this.SocialEntries.Count;
			}
			this._SetCharacter(this.SocialEntries[index]);
			Game1.playSound("smallSelect", null);
			this._printedName = "";
			this._characterEntrancePosition = new Vector2((float)(Math.Sign(offset) * -4), 0f);
			if (Game1.options.SnappyMenus && (this.currentlySnappedComponent == null || !this.currentlySnappedComponent.visible))
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002BA3 RID: 11171 RVA: 0x00212500 File Offset: 0x00210700
		protected void _UpdateList()
		{
			for (int i = 0; i < this._profileItems.Count; i++)
			{
				this._profileItems[i].Unload();
			}
			this._profileItems.Clear();
			NPC npc = this.Current.Character as NPC;
			if (npc == null)
			{
				return;
			}
			List<Item> loved_items = new List<Item>();
			List<Item> liked_items = new List<Item>();
			List<Item> neutral_items = new List<Item>();
			List<Item> disliked_items = new List<Item>();
			List<Item> hated_items = new List<Item>();
			List<Item> categoryItems;
			if (this._sortedItems.TryGetValue(this._currentCategory, out categoryItems))
			{
				foreach (Item item in categoryItems)
				{
					switch (npc.getGiftTasteForThisItem(item))
					{
					case 0:
						loved_items.Add(item);
						break;
					case 2:
						liked_items.Add(item);
						break;
					case 4:
						disliked_items.Add(item);
						break;
					case 6:
						hated_items.Add(item);
						break;
					case 8:
						neutral_items.Add(item);
						break;
					}
				}
			}
			PI_ItemList item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Loved"), loved_items);
			this._profileItems.Add(item_display);
			item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Liked"), liked_items);
			this._profileItems.Add(item_display);
			item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Neutral"), neutral_items);
			this._profileItems.Add(item_display);
			item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Disliked"), disliked_items);
			this._profileItems.Add(item_display);
			item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Hated"), hated_items);
			this._profileItems.Add(item_display);
			this.SetupLayout();
			this.populateClickableComponentList();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && (this.currentlySnappedComponent == null || !this.allClickableComponents.Contains(this.currentlySnappedComponent)))
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002BA4 RID: 11172 RVA: 0x00212738 File Offset: 0x00210938
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (direction != 2 || a.region != 501 || b.region != 500) && base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x06002BA5 RID: 11173 RVA: 0x00212763 File Offset: 0x00210963
		public override void snapToDefaultClickableComponent()
		{
			if (this.clickableProfileItems.Count > 0)
			{
				this.currentlySnappedComponent = this.clickableProfileItems[0];
			}
			else
			{
				this.currentlySnappedComponent = this.backButton;
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002BA6 RID: 11174 RVA: 0x0021279C File Offset: 0x0021099C
		public void UpdateButtons()
		{
			this._clickableTextureComponents = new List<ClickableTextureComponent>();
			this.upArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false)
			{
				myID = 105,
				upNeighborID = 102,
				upNeighborImmutable = true,
				downNeighborID = 106,
				downNeighborImmutable = true,
				leftNeighborID = -99998,
				leftNeighborImmutable = true
			};
			this.downArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false)
			{
				myID = 106,
				upNeighborID = 105,
				upNeighborImmutable = true,
				leftNeighborID = -99998,
				leftNeighborImmutable = true
			};
			this.scrollBar = new ClickableTextureComponent(new Rectangle(0, 0, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				name = "Back Button",
				upNeighborID = -99998,
				downNeighborID = -99998,
				downNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 501
			};
			this._clickableTextureComponents.Add(this.backButton);
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				name = "Forward Button",
				upNeighborID = -99998,
				downNeighborID = -99998,
				downNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 501
			};
			this._clickableTextureComponents.Add(this.forwardButton);
			this.previousCharacterButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 0,
				name = "Previous Char",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 500
			};
			this._clickableTextureComponents.Add(this.previousCharacterButton);
			this.nextCharacterButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 0,
				name = "Next Char",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 500
			};
			this._clickableTextureComponents.Add(this.nextCharacterButton);
			this._clickableTextureComponents.Add(this.upArrow);
			this._clickableTextureComponents.Add(this.downArrow);
		}

		// Token: 0x06002BA7 RID: 11175 RVA: 0x00212B96 File Offset: 0x00210D96
		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0)
			{
				this.Scroll(-this.scrollStep);
				return;
			}
			if (direction < 0)
			{
				this.Scroll(this.scrollStep);
			}
		}

		// Token: 0x06002BA8 RID: 11176 RVA: 0x00212BC4 File Offset: 0x00210DC4
		public void ChangePage(int offset)
		{
			this.scrollPosition = 0;
			this._currentCategory += offset;
			while (this._currentCategory < 0)
			{
				this._currentCategory += ProfileMenu.itemCategories.Length;
			}
			while (this._currentCategory >= ProfileMenu.itemCategories.Length)
			{
				this._currentCategory -= ProfileMenu.itemCategories.Length;
			}
			Game1.playSound("shwip", null);
			this._UpdateList();
			if (Game1.options.SnappyMenus && (this.currentlySnappedComponent == null || !this.currentlySnappedComponent.visible))
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002BA9 RID: 11177 RVA: 0x00212C6C File Offset: 0x00210E6C
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).X;
			this.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).Y;
			this.UpdateButtons();
			this.SetupLayout();
			base.initializeUpperRightCloseButton();
			this.populateClickableComponentList();
		}

		// Token: 0x06002BAA RID: 11178 RVA: 0x00212CCC File Offset: 0x00210ECC
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button <= Buttons.LeftShoulder)
			{
				if (button != Buttons.Back)
				{
					if (button != Buttons.LeftShoulder)
					{
						return;
					}
					this.ChangeCharacter(-1);
					return;
				}
				else
				{
					this.PlayHiddenEmote();
				}
			}
			else
			{
				if (button == Buttons.RightShoulder)
				{
					this.ChangeCharacter(1);
					return;
				}
				if (button == Buttons.RightTrigger)
				{
					this.ChangePage(1);
					return;
				}
				if (button == Buttons.LeftTrigger)
				{
					this.ChangePage(-1);
					return;
				}
			}
		}

		// Token: 0x06002BAB RID: 11179 RVA: 0x00212D34 File Offset: 0x00210F34
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.None)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				base.exitThisMenu(true);
				return;
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && !this.overrideSnappyMenuCursorMovementBan())
			{
				base.applyMovementKey(key);
			}
		}

		// Token: 0x06002BAC RID: 11180 RVA: 0x00212D93 File Offset: 0x00210F93
		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			this.ConstrainSelectionToView();
		}

		// Token: 0x06002BAD RID: 11181 RVA: 0x00212DA2 File Offset: 0x00210FA2
		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}

		// Token: 0x06002BAE RID: 11182 RVA: 0x00212DB4 File Offset: 0x00210FB4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
			}
			else if (this.scrollBarRunner.Contains(x, y))
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
			}
			if (this.upperRightCloseButton != null && this.readyToClose() && this.upperRightCloseButton.containsPoint(x, y))
			{
				base.exitThisMenu(true);
				return;
			}
			if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				return;
			}
			if (this.backButton.containsPoint(x, y))
			{
				this.ChangePage(-1);
				return;
			}
			if (this.forwardButton.containsPoint(x, y))
			{
				this.ChangePage(1);
				return;
			}
			if (this.previousCharacterButton.containsPoint(x, y))
			{
				this.ChangeCharacter(-1);
				return;
			}
			if (this.nextCharacterButton.containsPoint(x, y))
			{
				this.ChangeCharacter(1);
				return;
			}
			if (this.downArrow.containsPoint(x, y))
			{
				this.Scroll(this.scrollStep);
			}
			if (this.upArrow.containsPoint(x, y))
			{
				this.Scroll(-this.scrollStep);
			}
			if (this.characterSpriteBox.Contains(x, y))
			{
				this.PlayHiddenEmote();
			}
		}

		// Token: 0x06002BAF RID: 11183 RVA: 0x00212EE0 File Offset: 0x002110E0
		public void PlayHiddenEmote()
		{
			if (this.Current.HeartLevel >= 4)
			{
				this._currentDirection = 2;
				this._characterSpriteRandomInt = Game1.random.Next(4);
				CharacterData data = this.Current.Data;
				Game1.playSound(((data != null) ? data.HiddenProfileEmoteSound : null) ?? "drumkit6", null);
				this._hiddenEmoteTimer = ((data != null && data.HiddenProfileEmoteDuration >= 0) ? ((float)data.HiddenProfileEmoteDuration) : 4000f);
				return;
			}
			this._currentDirection = 2;
			this._directionChangeTimer = 5000f;
			Game1.playSound("Cowboy_Footstep", null);
		}

		// Token: 0x06002BB0 RID: 11184 RVA: 0x00212F8C File Offset: 0x0021118C
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.hoveredItem = null;
			if (this._itemDisplayRect.Contains(x, y))
			{
				foreach (ProfileItem profileItem in this._profileItems)
				{
					profileItem.performHover(x, y);
				}
			}
			this.upArrow.tryHover(x, y, 0.1f);
			this.downArrow.tryHover(x, y, 0.1f);
			this.backButton.tryHover(x, y, 0.6f);
			this.forwardButton.tryHover(x, y, 0.6f);
			this.nextCharacterButton.tryHover(x, y, 0.6f);
			this.previousCharacterButton.tryHover(x, y, 0.6f);
		}

		// Token: 0x06002BB1 RID: 11185 RVA: 0x00213068 File Offset: 0x00211268
		public void ConstrainSelectionToView()
		{
			if (Game1.options.snappyMenus)
			{
				ClickableComponent currentlySnappedComponent = this.currentlySnappedComponent;
				if (currentlySnappedComponent != null && currentlySnappedComponent.region == 502 && !this._itemDisplayRect.Contains(this.currentlySnappedComponent.bounds))
				{
					if (this.currentlySnappedComponent.bounds.Bottom > this._itemDisplayRect.Bottom)
					{
						int scroll = (int)Math.Ceiling(((double)this.currentlySnappedComponent.bounds.Bottom - (double)this._itemDisplayRect.Bottom) / (double)this.scrollStep) * this.scrollStep;
						this.Scroll(scroll);
					}
					else if (this.currentlySnappedComponent.bounds.Top < this._itemDisplayRect.Top)
					{
						int scroll2 = (int)Math.Floor(((double)this.currentlySnappedComponent.bounds.Top - (double)this._itemDisplayRect.Top) / (double)this.scrollStep) * this.scrollStep;
						this.Scroll(scroll2);
					}
				}
				if (this.scrollPosition <= this.scrollStep)
				{
					this.scrollPosition = 0;
					this.UpdateScroll();
				}
			}
		}

		// Token: 0x06002BB2 RID: 11186 RVA: 0x0021318C File Offset: 0x0021138C
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.Current.DisplayName != null && this._printedName.Length < this.Current.DisplayName.Length)
			{
				this._printedName += this.Current.DisplayName[this._printedName.Length].ToString();
			}
			if (this._hideTooltipTime > 0)
			{
				this._hideTooltipTime -= time.ElapsedGameTime.Milliseconds;
				if (this._hideTooltipTime < 0)
				{
					this._hideTooltipTime = 0;
				}
			}
			if (this._characterEntrancePosition.X != 0f)
			{
				this._characterEntrancePosition.X = this._characterEntrancePosition.X - (float)Math.Sign(this._characterEntrancePosition.X) * 0.25f;
			}
			if (this._characterEntrancePosition.Y != 0f)
			{
				this._characterEntrancePosition.Y = this._characterEntrancePosition.Y - (float)Math.Sign(this._characterEntrancePosition.Y) * 0.25f;
			}
			if (this._animatedSprite != null)
			{
				if (this._hiddenEmoteTimer > 0f)
				{
					this._hiddenEmoteTimer -= (float)time.ElapsedGameTime.Milliseconds;
					if (this._hiddenEmoteTimer <= 0f)
					{
						this._hiddenEmoteTimer = -1f;
						this._currentDirection = 2;
						this._directionChangeTimer = 2000f;
						if (this.Current.InternalName == "Leo")
						{
							this.Current.Character.Sprite.AnimateDown(time, 0, "");
						}
					}
				}
				else if (this._directionChangeTimer > 0f)
				{
					this._directionChangeTimer -= (float)time.ElapsedGameTime.Milliseconds;
					if (this._directionChangeTimer <= 0f)
					{
						this._directionChangeTimer = 2000f;
						this._currentDirection = (this._currentDirection + 1) % 4;
					}
				}
				if (this._characterEntrancePosition != Vector2.Zero)
				{
					if (this._characterEntrancePosition.X < 0f)
					{
						this._animatedSprite.AnimateRight(time, 2, "");
						return;
					}
					if (this._characterEntrancePosition.X > 0f)
					{
						this._animatedSprite.AnimateLeft(time, 2, "");
						return;
					}
					if (this._characterEntrancePosition.Y > 0f)
					{
						this._animatedSprite.AnimateUp(time, 2, "");
						return;
					}
					if (this._characterEntrancePosition.Y < 0f)
					{
						this._animatedSprite.AnimateDown(time, 2, "");
						return;
					}
				}
				else if (this._hiddenEmoteTimer > 0f)
				{
					CharacterData data = this.Current.Data;
					if (data != null && data.HiddenProfileEmoteStartFrame >= 0)
					{
						int startFrame = (this.Current.InternalName == "Emily" && data.HiddenProfileEmoteStartFrame == 16) ? (data.HiddenProfileEmoteStartFrame + this._characterSpriteRandomInt * 2) : data.HiddenProfileEmoteStartFrame;
						this._animatedSprite.Animate(time, startFrame, data.HiddenProfileEmoteFrameCount, data.HiddenProfileEmoteFrameDuration);
						return;
					}
					this._animatedSprite.AnimateDown(time, 2, "");
					return;
				}
				else
				{
					switch (this._currentDirection)
					{
					case 0:
						this._animatedSprite.AnimateUp(time, 2, "");
						return;
					case 1:
						this._animatedSprite.AnimateRight(time, 2, "");
						break;
					case 2:
						this._animatedSprite.AnimateDown(time, 2, "");
						return;
					case 3:
						this._animatedSprite.AnimateLeft(time, 2, "");
						return;
					default:
						return;
					}
				}
			}
		}

		// Token: 0x06002BB3 RID: 11187 RVA: 0x0021352C File Offset: 0x0021172C
		public void SetupLayout()
		{
			int x = this.xPositionOnScreen + 64 - 12;
			int y = this.yPositionOnScreen + IClickableMenu.borderWidth;
			Rectangle left_pane_rectangle = new Rectangle(x, y, 400, 720 - IClickableMenu.borderWidth * 2);
			Rectangle content_rectangle = new Rectangle(x, y, 1204, 720 - IClickableMenu.borderWidth * 2);
			content_rectangle.X += left_pane_rectangle.Width;
			content_rectangle.Width -= left_pane_rectangle.Width;
			this._characterStatusDisplayBox = new Rectangle(left_pane_rectangle.X, left_pane_rectangle.Y, left_pane_rectangle.Width, left_pane_rectangle.Height);
			left_pane_rectangle.Y += 32;
			left_pane_rectangle.Height -= 32;
			this._characterSpriteDrawPosition = new Vector2((float)(left_pane_rectangle.X + (left_pane_rectangle.Width - Game1.nightbg.Width) / 2), (float)left_pane_rectangle.Y);
			this.characterSpriteBox = new Rectangle(this.xPositionOnScreen + 64 - 12 + (400 - Game1.nightbg.Width) / 2, this.yPositionOnScreen + IClickableMenu.borderWidth, Game1.nightbg.Width, Game1.nightbg.Height);
			this.previousCharacterButton.bounds.X = (int)this._characterSpriteDrawPosition.X - 64 - this.previousCharacterButton.bounds.Width / 2;
			this.previousCharacterButton.bounds.Y = (int)this._characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - this.previousCharacterButton.bounds.Height / 2;
			this.nextCharacterButton.bounds.X = (int)this._characterSpriteDrawPosition.X + Game1.nightbg.Width + 64 - this.nextCharacterButton.bounds.Width / 2;
			this.nextCharacterButton.bounds.Y = (int)this._characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - this.nextCharacterButton.bounds.Height / 2;
			left_pane_rectangle.Y += Game1.daybg.Height + 32;
			left_pane_rectangle.Height -= Game1.daybg.Height + 32;
			this._characterNamePosition = new Vector2((float)left_pane_rectangle.Center.X, (float)left_pane_rectangle.Top);
			left_pane_rectangle.Y += 96;
			left_pane_rectangle.Height -= 96;
			this._heartDisplayPosition = new Vector2((float)left_pane_rectangle.Center.X, (float)left_pane_rectangle.Top);
			NPC npc = this.Current.Character as NPC;
			if (npc != null)
			{
				left_pane_rectangle.Y += 56;
				left_pane_rectangle.Height -= 48;
				this._birthdayHeadingDisplayPosition = new Vector2((float)left_pane_rectangle.Center.X, (float)left_pane_rectangle.Top);
				if (npc.birthday_Season.Value != null && Utility.getSeasonNumber(npc.birthday_Season.Value) >= 0)
				{
					left_pane_rectangle.Y += 48;
					left_pane_rectangle.Height -= 48;
					this._birthdayDisplayPosition = new Vector2((float)left_pane_rectangle.Center.X, (float)left_pane_rectangle.Top);
					left_pane_rectangle.Y += 64;
					left_pane_rectangle.Height -= 64;
				}
				if (this._status != "")
				{
					this._statusHeadingDisplayPosition = new Vector2((float)left_pane_rectangle.Center.X, (float)left_pane_rectangle.Top);
					left_pane_rectangle.Y += 48;
					left_pane_rectangle.Height -= 48;
					this._statusDisplayPosition = new Vector2((float)left_pane_rectangle.Center.X, (float)left_pane_rectangle.Top);
					left_pane_rectangle.Y += 64;
					left_pane_rectangle.Height -= 64;
				}
			}
			content_rectangle.Height -= 96;
			content_rectangle.Y -= 8;
			this._giftLogHeadingDisplayPosition = new Vector2((float)content_rectangle.Center.X, (float)content_rectangle.Top);
			content_rectangle.Y += 80;
			content_rectangle.Height -= 70;
			this.backButton.bounds.X = content_rectangle.Left + 64 - this.forwardButton.bounds.Width / 2;
			this.backButton.bounds.Y = content_rectangle.Top;
			this.forwardButton.bounds.X = content_rectangle.Right - 64 - this.forwardButton.bounds.Width / 2;
			this.forwardButton.bounds.Y = content_rectangle.Top;
			content_rectangle.Width -= 250;
			content_rectangle.X += 125;
			this._giftLogCategoryDisplayPosition = new Vector2((float)content_rectangle.Center.X, (float)content_rectangle.Top);
			content_rectangle.Y += 64;
			content_rectangle.Y += 32;
			content_rectangle.Height -= 32;
			this._itemDisplayRect = content_rectangle;
			int scroll_inset = 64;
			this.scrollBarRunner = new Rectangle(content_rectangle.Right + 48, content_rectangle.Top + scroll_inset, this.scrollBar.bounds.Width, content_rectangle.Height - scroll_inset * 2);
			this.downArrow.bounds.Y = this.scrollBarRunner.Bottom + 16;
			this.downArrow.bounds.X = this.scrollBarRunner.Center.X - this.downArrow.bounds.Width / 2;
			this.upArrow.bounds.Y = this.scrollBarRunner.Top - 16 - this.upArrow.bounds.Height;
			this.upArrow.bounds.X = this.scrollBarRunner.Center.X - this.upArrow.bounds.Width / 2;
			float draw_y = 0f;
			if (this._profileItems.Count > 0)
			{
				int drawn_index = 0;
				for (int i = 0; i < this._profileItems.Count; i++)
				{
					ProfileItem profile_item = this._profileItems[i];
					if (profile_item.ShouldDraw())
					{
						draw_y = profile_item.HandleLayout(draw_y, this._itemDisplayRect, drawn_index);
						drawn_index++;
					}
				}
			}
			this.scrollSize = (int)draw_y - this._itemDisplayRect.Height;
			if (this.NeedsScrollBar())
			{
				this.upArrow.visible = true;
				this.downArrow.visible = true;
			}
			else
			{
				this.upArrow.visible = false;
				this.downArrow.visible = false;
			}
			this.UpdateScroll();
		}

		// Token: 0x06002BB4 RID: 11188 RVA: 0x00213C10 File Offset: 0x00211E10
		public override void leftClickHeld(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				int num = this.scrollPosition;
				this.scrollPosition = (int)Math.Round((double)((float)(y - this.scrollBarRunner.Top) / (float)this.scrollBarRunner.Height * (float)this.scrollSize / (float)this.scrollStep)) * this.scrollStep;
				this.UpdateScroll();
				if (num != this.scrollPosition)
				{
					Game1.playSound("shiny4", null);
				}
			}
		}

		// Token: 0x06002BB5 RID: 11189 RVA: 0x00213C9C File Offset: 0x00211E9C
		public bool NeedsScrollBar()
		{
			return this.scrollSize > 0;
		}

		// Token: 0x06002BB6 RID: 11190 RVA: 0x00213CA8 File Offset: 0x00211EA8
		public void Scroll(int offset)
		{
			if (!this.NeedsScrollBar())
			{
				return;
			}
			int num = this.scrollPosition;
			this.scrollPosition += offset;
			this.UpdateScroll();
			if (num != this.scrollPosition)
			{
				Game1.playSound("shwip", null);
			}
		}

		// Token: 0x06002BB7 RID: 11191 RVA: 0x00213CF4 File Offset: 0x00211EF4
		public virtual void UpdateScroll()
		{
			this.scrollPosition = Utility.Clamp(this.scrollPosition, 0, this.scrollSize);
			float draw_y = (float)(this._itemDisplayRect.Top - this.scrollPosition);
			this._errorMessagePosition = new Vector2((float)this._itemDisplayRect.Center.X, (float)this._itemDisplayRect.Center.Y);
			if (this._profileItems.Count > 0)
			{
				int drawn_index = 0;
				for (int i = 0; i < this._profileItems.Count; i++)
				{
					ProfileItem profile_item = this._profileItems[i];
					if (profile_item.ShouldDraw())
					{
						draw_y = profile_item.HandleLayout(draw_y, this._itemDisplayRect, drawn_index);
						drawn_index++;
					}
				}
			}
			if (this.scrollSize <= 0)
			{
				return;
			}
			this.scrollBar.bounds.X = this.scrollBarRunner.Center.X - this.scrollBar.bounds.Width / 2;
			this.scrollBar.bounds.Y = (int)Utility.Lerp((float)this.scrollBarRunner.Top, (float)(this.scrollBarRunner.Bottom - this.scrollBar.bounds.Height), (float)this.scrollPosition / (float)this.scrollSize);
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002BB8 RID: 11192 RVA: 0x00213E48 File Offset: 0x00212048
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}
			b.Draw(this.letterTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2), (float)(this.yPositionOnScreen + this.height / 2)), new Rectangle?(new Rectangle(0, 0, 320, 180)), Color.White, 0f, new Vector2(160f, 90f), 4f, SpriteEffects.None, 0.86f);
			Game1.DrawBox(this._characterStatusDisplayBox.X, this._characterStatusDisplayBox.Y, this._characterStatusDisplayBox.Width, this._characterStatusDisplayBox.Height, null);
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, this._characterSpriteDrawPosition, Color.White);
			Vector2 portraitPosition = new Vector2(this._characterSpriteDrawPosition.X + (float)((Game1.daybg.Width - this._animatedSprite.SpriteWidth * 4) / 2), this._characterSpriteDrawPosition.Y + 32f + (float)((32 - this._animatedSprite.SpriteHeight) * 4));
			NPC npc = this.Current.Character as NPC;
			if (npc != null)
			{
				this._animatedSprite.draw(b, portraitPosition, 0.8f);
				bool isCurrentSpouse = this.Current.IsMarriedToCurrentPlayer();
				int drawn_hearts = Math.Max(10, Utility.GetMaximumHeartsForCharacter(npc));
				float heart_draw_start_x = this._heartDisplayPosition.X - (float)(Math.Min(10, drawn_hearts) * 32 / 2);
				float heart_draw_offset_y = (drawn_hearts > 10) ? -16f : 0f;
				for (int hearts = 0; hearts < drawn_hearts; hearts++)
				{
					this.drawNPCSlotHeart(b, heart_draw_start_x, heart_draw_offset_y, this.Current, hearts, this.Current.IsDatingCurrentPlayer(), isCurrentSpouse);
				}
			}
			if (this._printedName.Length < this.Current.DisplayName.Length)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, "", (int)this._characterNamePosition.X, (int)this._characterNamePosition.Y, this._printedName, 1f, null, 0, 0.88f, false);
			}
			else
			{
				SpriteText.drawStringWithScrollCenteredAt(b, this.Current.DisplayName, (int)this._characterNamePosition.X, (int)this._characterNamePosition.Y, "", 1f, null, 0, 0.88f, false);
			}
			if (npc != null && npc.birthday_Season.Value != null)
			{
				int season_number = Utility.getSeasonNumber(npc.birthday_Season.Value);
				if (season_number >= 0)
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Birthday"), (int)this._birthdayHeadingDisplayPosition.X, (int)this._birthdayHeadingDisplayPosition.Y, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
					string birthday = Game1.content.LoadString("Strings\\UI:BirthdayOrder", npc.Birthday_Day, Utility.getSeasonNameFromNumber(season_number));
					b.DrawString(Game1.dialogueFont, birthday, new Vector2(-Game1.dialogueFont.MeasureString(birthday).X / 2f + this._birthdayDisplayPosition.X, this._birthdayDisplayPosition.Y), Game1.textColor);
				}
				if (this._status != "")
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Status"), (int)this._statusHeadingDisplayPosition.X, (int)this._statusHeadingDisplayPosition.Y, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
					b.DrawString(Game1.dialogueFont, this._status, new Vector2(-Game1.dialogueFont.MeasureString(this._status).X / 2f + this._statusDisplayPosition.X, this._statusDisplayPosition.Y), Game1.textColor);
				}
			}
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_GiftLog"), (int)this._giftLogHeadingDisplayPosition.X, (int)this._giftLogHeadingDisplayPosition.Y, "", 1f, null, 0, 0.88f, false);
			SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:" + ProfileMenu.itemCategories[this._currentCategory].categoryName, this.Current.DisplayName), (int)this._giftLogCategoryDisplayPosition.X, (int)this._giftLogCategoryDisplayPosition.Y, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
			bool drew_items = false;
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			b.GraphicsDevice.ScissorRectangle = this._itemDisplayRect;
			if (this._profileItems.Count > 0)
			{
				for (int i = 0; i < this._profileItems.Count; i++)
				{
					ProfileItem profile_item = this._profileItems[i];
					if (profile_item.ShouldDraw())
					{
						drew_items = true;
						profile_item.Draw(b);
					}
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (this.NeedsScrollBar())
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, false, -1f);
				this.scrollBar.draw(b);
			}
			if (!drew_items)
			{
				string error_string = Game1.content.LoadString("Strings\\UI:Profile_GiftLog_NoGiftsGiven");
				b.DrawString(Game1.smallFont, error_string, new Vector2(-Game1.smallFont.MeasureString(error_string).X / 2f + this._errorMessagePosition.X, this._errorMessagePosition.Y), Game1.textColor);
			}
			foreach (ClickableTextureComponent clickableTextureComponent in this._clickableTextureComponents)
			{
				clickableTextureComponent.draw(b);
			}
			base.draw(b);
			base.drawMouse(b, true, -1);
			if (this.hoveredItem != null)
			{
				bool draw_tooltip = true;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse && this._hideTooltipTime > 0)
				{
					draw_tooltip = false;
				}
				if (draw_tooltip)
				{
					string name = this.hoveredItem.DisplayName;
					string description = this.hoveredItem.getDescription();
					if (description.Contains("{0}") || this.hoveredItem.ItemId == "DriedMushrooms")
					{
						name = (Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoveredItem.ItemId + "_CollectionsTabName", true) ?? name);
						description = (Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoveredItem.ItemId + "_CollectionsTabDescription", true) ?? description);
					}
					IClickableMenu.drawToolTip(b, description, name, this.hoveredItem, false, -1, 0, null, -1, null, -1, null);
				}
			}
		}

		// Token: 0x06002BB9 RID: 11193 RVA: 0x00214624 File Offset: 0x00212824
		private void drawNPCSlotHeart(SpriteBatch b, float heartDrawStartX, float heartDrawStartY, SocialPage.SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
		{
			bool isLockedHeart = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;
			int heartX = (hearts < entry.HeartLevel || isLockedHeart) ? 211 : 218;
			Color heartTint = (hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White;
			if (hearts < 10)
			{
				b.Draw(Game1.mouseCursors, new Vector2(heartDrawStartX + (float)(hearts * 32), this._heartDisplayPosition.Y + heartDrawStartY), new Rectangle?(new Rectangle(heartX, 428, 7, 6)), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				return;
			}
			b.Draw(Game1.mouseCursors, new Vector2(heartDrawStartX + (float)((hearts - 10) * 32), this._heartDisplayPosition.Y + heartDrawStartY + 32f), new Rectangle?(new Rectangle(heartX, 428, 7, 6)), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}

		// Token: 0x06002BBA RID: 11194 RVA: 0x00214735 File Offset: 0x00212935
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			this.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002BBB RID: 11195 RVA: 0x00214740 File Offset: 0x00212940
		public void RegisterClickable(ClickableComponent clickable)
		{
			this.clickableProfileItems.Add(clickable);
		}

		// Token: 0x06002BBC RID: 11196 RVA: 0x0021474E File Offset: 0x0021294E
		public void UnregisterClickable(ClickableComponent clickable)
		{
			this.clickableProfileItems.Remove(clickable);
		}

		// Token: 0x04001D47 RID: 7495
		public const int region_characterSelectors = 500;

		// Token: 0x04001D48 RID: 7496
		public const int region_categorySelector = 501;

		// Token: 0x04001D49 RID: 7497
		public const int region_itemButtons = 502;

		// Token: 0x04001D4A RID: 7498
		public const int region_backButton = 101;

		// Token: 0x04001D4B RID: 7499
		public const int region_forwardButton = 102;

		// Token: 0x04001D4C RID: 7500
		public const int region_upArrow = 105;

		// Token: 0x04001D4D RID: 7501
		public const int region_downArrow = 106;

		// Token: 0x04001D4E RID: 7502
		public const int letterWidth = 320;

		// Token: 0x04001D4F RID: 7503
		public const int letterHeight = 180;

		// Token: 0x04001D50 RID: 7504
		public Texture2D letterTexture;

		// Token: 0x04001D51 RID: 7505
		protected string hoverText = "";

		// Token: 0x04001D52 RID: 7506
		protected List<ProfileItem> _profileItems;

		// Token: 0x04001D53 RID: 7507
		public Item hoveredItem;

		// Token: 0x04001D54 RID: 7508
		public ClickableTextureComponent backButton;

		// Token: 0x04001D55 RID: 7509
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001D56 RID: 7510
		public ClickableTextureComponent nextCharacterButton;

		// Token: 0x04001D57 RID: 7511
		public ClickableTextureComponent previousCharacterButton;

		// Token: 0x04001D58 RID: 7512
		protected Rectangle characterSpriteBox;

		// Token: 0x04001D59 RID: 7513
		protected int _currentCategory;

		// Token: 0x04001D5A RID: 7514
		protected AnimatedSprite _animatedSprite;

		// Token: 0x04001D5B RID: 7515
		protected float _directionChangeTimer;

		// Token: 0x04001D5C RID: 7516
		protected float _hiddenEmoteTimer = -1f;

		// Token: 0x04001D5D RID: 7517
		protected int _currentDirection;

		// Token: 0x04001D5E RID: 7518
		protected int _hideTooltipTime;

		// Token: 0x04001D5F RID: 7519
		protected SocialPage _socialPage;

		// Token: 0x04001D60 RID: 7520
		protected string _status = "";

		// Token: 0x04001D61 RID: 7521
		protected string _printedName = "";

		// Token: 0x04001D62 RID: 7522
		protected Vector2 _characterEntrancePosition = new Vector2(0f, 0f);

		// Token: 0x04001D63 RID: 7523
		public ClickableTextureComponent upArrow;

		// Token: 0x04001D64 RID: 7524
		public ClickableTextureComponent downArrow;

		// Token: 0x04001D65 RID: 7525
		protected ClickableTextureComponent scrollBar;

		// Token: 0x04001D66 RID: 7526
		protected Rectangle scrollBarRunner;

		// Token: 0x04001D67 RID: 7527
		public List<ClickableComponent> clickableProfileItems;

		// Token: 0x04001D68 RID: 7528
		public SocialPage.SocialEntry Current;

		// Token: 0x04001D69 RID: 7529
		public readonly List<SocialPage.SocialEntry> SocialEntries = new List<SocialPage.SocialEntry>();

		// Token: 0x04001D6A RID: 7530
		protected Vector2 _characterNamePosition;

		// Token: 0x04001D6B RID: 7531
		protected Vector2 _heartDisplayPosition;

		// Token: 0x04001D6C RID: 7532
		protected Vector2 _birthdayHeadingDisplayPosition;

		// Token: 0x04001D6D RID: 7533
		protected Vector2 _birthdayDisplayPosition;

		// Token: 0x04001D6E RID: 7534
		protected Vector2 _statusHeadingDisplayPosition;

		// Token: 0x04001D6F RID: 7535
		protected Vector2 _statusDisplayPosition;

		// Token: 0x04001D70 RID: 7536
		protected Vector2 _giftLogHeadingDisplayPosition;

		// Token: 0x04001D71 RID: 7537
		protected Vector2 _giftLogCategoryDisplayPosition;

		// Token: 0x04001D72 RID: 7538
		protected Vector2 _errorMessagePosition;

		// Token: 0x04001D73 RID: 7539
		protected Vector2 _characterSpriteDrawPosition;

		// Token: 0x04001D74 RID: 7540
		protected Rectangle _characterStatusDisplayBox;

		// Token: 0x04001D75 RID: 7541
		protected List<ClickableTextureComponent> _clickableTextureComponents;

		// Token: 0x04001D76 RID: 7542
		public Rectangle _itemDisplayRect;

		// Token: 0x04001D77 RID: 7543
		protected int scrollPosition;

		// Token: 0x04001D78 RID: 7544
		protected int scrollStep = 36;

		// Token: 0x04001D79 RID: 7545
		protected int scrollSize;

		// Token: 0x04001D7A RID: 7546
		public static ProfileMenu.ProfileItemCategory[] itemCategories = new ProfileMenu.ProfileItemCategory[]
		{
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_LikedGifts", null),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_FruitsAndVegetables", new int[]
			{
				-75,
				-79
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_AnimalProduce", new int[]
			{
				-6,
				-5,
				-14,
				-18
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_ArtisanItems", new int[]
			{
				-26
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_CookedItems", new int[]
			{
				-7
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_ForagedItems", new int[]
			{
				-80,
				-81,
				-23,
				-17
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_Fish", new int[]
			{
				-4
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_Ingredients", new int[]
			{
				-27,
				-25
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_MineralsAndGems", new int[]
			{
				-15,
				-12,
				-2
			}),
			new ProfileMenu.ProfileItemCategory("Profile_Gift_Category_Misc", null)
		};

		// Token: 0x04001D7B RID: 7547
		protected Dictionary<int, List<Item>> _sortedItems;

		// Token: 0x04001D7C RID: 7548
		public bool scrolling;

		// Token: 0x04001D7D RID: 7549
		private int _characterSpriteRandomInt;

		// Token: 0x0200062D RID: 1581
		public class ProfileItemCategory
		{
			// Token: 0x06004472 RID: 17522 RVA: 0x0031C92D File Offset: 0x0031AB2D
			public ProfileItemCategory(string name, int[] valid_categories)
			{
				this.categoryName = name;
				this.validCategories = valid_categories;
			}

			// Token: 0x04002EA7 RID: 11943
			public string categoryName;

			// Token: 0x04002EA8 RID: 11944
			public int[] validCategories;
		}
	}
}

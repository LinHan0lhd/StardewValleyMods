using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x02000260 RID: 608
	public class CollectionsPage : IClickableMenu
	{
		// Token: 0x06002848 RID: 10312 RVA: 0x001D4804 File Offset: 0x001D2A04
		public CollectionsPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			this.sideTabs.Add(0, new ClickableTextureComponent(0.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48 + CollectionsPage.widthToMoveActiveTab, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Shipped"), Game1.mouseCursors, new Rectangle(640, 80, 16, 16), 4f, false)
			{
				myID = 7001,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(0, new List<List<ClickableTextureComponent>>());
			this.sideTabs.Add(1, new ClickableTextureComponent(1.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Fish"), Game1.mouseCursors, new Rectangle(640, 64, 16, 16), 4f, false)
			{
				myID = 7002,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(1, new List<List<ClickableTextureComponent>>());
			this.sideTabs.Add(2, new ClickableTextureComponent(2.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Artifacts"), Game1.mouseCursors, new Rectangle(656, 64, 16, 16), 4f, false)
			{
				myID = 7003,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(2, new List<List<ClickableTextureComponent>>());
			this.sideTabs.Add(3, new ClickableTextureComponent(3.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Minerals"), Game1.mouseCursors, new Rectangle(672, 64, 16, 16), 4f, false)
			{
				myID = 7004,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(3, new List<List<ClickableTextureComponent>>());
			this.sideTabs.Add(4, new ClickableTextureComponent(4.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Cooking"), Game1.mouseCursors, new Rectangle(688, 64, 16, 16), 4f, false)
			{
				myID = 7005,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(4, new List<List<ClickableTextureComponent>>());
			this.sideTabs.Add(5, new ClickableTextureComponent(5.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Achievements"), Game1.mouseCursors, new Rectangle(656, 80, 16, 16), 4f, false)
			{
				myID = 7006,
				upNeighborID = 7005,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(5, new List<List<ClickableTextureComponent>>());
			this.sideTabs.Add(7, new ClickableTextureComponent(7.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Letters"), Game1.mouseCursors, new Rectangle(688, 80, 16, 16), 4f, false)
			{
				myID = 7008,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(7, new List<List<ClickableTextureComponent>>());
			if (Game1.player.secretNotesSeen.Count > 0)
			{
				this.sideTabs.Add(6, new ClickableTextureComponent(6.ToString() ?? "", new Rectangle(this.xPositionOnScreen - 48, this.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_SecretNotes"), Game1.mouseCursors, new Rectangle(672, 80, 16, 16), 4f, false)
				{
					myID = 7007,
					upNeighborID = -99998,
					rightNeighborID = 0
				});
				this.collections.Add(6, new List<List<ClickableTextureComponent>>());
			}
			this.sideTabs[0].upNeighborID = -1;
			this.sideTabs[0].upNeighborImmutable = true;
			int last_tab = 0;
			int last_y = 0;
			foreach (int key in this.sideTabs.Keys)
			{
				if (this.sideTabs[key].bounds.Y > last_y)
				{
					last_y = this.sideTabs[key].bounds.Y;
					last_tab = key;
				}
			}
			this.sideTabs[last_tab].downNeighborID = -1;
			this.sideTabs[last_tab].downNeighborImmutable = true;
			CollectionsPage.widthToMoveActiveTab = 8;
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 706,
				rightNeighborID = -7777
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width - 32 - 60, this.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 707,
				leftNeighborID = -7777
			};
			int[] widthUsed = new int[8];
			int baseX = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
			int baseY = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
			int collectionWidth = 10;
			List<ParsedItemData> dataEntries = new List<ParsedItemData>(from entry in ItemRegistry.GetObjectTypeDefinition().GetAllData()
			orderby entry.TextureName, entry.SpriteIndex
			select entry);
			List<ParsedItemData> wineAndFriends = new List<ParsedItemData>();
			for (int i = dataEntries.Count - 1; i >= 0; i--)
			{
				string s = dataEntries[i].InternalName;
				if (s.Equals("Wine") || s.Equals("Pickles") || s.Equals("Jelly") || s.Equals("Juice"))
				{
					wineAndFriends.Add(dataEntries[i]);
					dataEntries.RemoveAt(i);
				}
				if (wineAndFriends.Count == 4)
				{
					break;
				}
			}
			wineAndFriends.Sort((ParsedItemData a, ParsedItemData b) => a.InternalName.CompareTo(b.InternalName));
			dataEntries.Insert(278, wineAndFriends[2]);
			dataEntries.Insert(279, wineAndFriends[0]);
			dataEntries.Insert(283, wineAndFriends[3]);
			dataEntries.Insert(284, wineAndFriends[1]);
			foreach (ParsedItemData data in dataEntries)
			{
				string id = data.ItemId;
				string type = data.ObjectType;
				bool farmerHas = false;
				bool farmerHasButNotMade = false;
				int whichCollection;
				if (type == "Arch")
				{
					whichCollection = 2;
					farmerHas = LibraryMuseum.HasDonatedArtifact(id);
				}
				else if (type == "Fish")
				{
					ObjectData objData = data.RawData as ObjectData;
					if (objData != null && objData.ExcludeFromFishingCollection)
					{
						continue;
					}
					whichCollection = 1;
					farmerHas = Game1.player.fishCaught.ContainsKey(data.QualifiedItemId);
				}
				else if (type == "Minerals" || data.Category == -2)
				{
					whichCollection = 3;
					farmerHas = LibraryMuseum.HasDonatedArtifact(id);
				}
				else if (type == "Cooking" || data.Category == -7)
				{
					whichCollection = 4;
					string last_minute_1_5_hack_name = data.InternalName;
					if (last_minute_1_5_hack_name != null)
					{
						int length = last_minute_1_5_hack_name.Length;
						if (length != 6)
						{
							switch (length)
							{
							case 13:
								if (last_minute_1_5_hack_name == "Cheese Cauli.")
								{
									last_minute_1_5_hack_name = "Cheese Cauliflower";
								}
								break;
							case 15:
							{
								char c = last_minute_1_5_hack_name[0];
								if (c != 'C')
								{
									if (c == 'D')
									{
										if (last_minute_1_5_hack_name == "Dish O' The Sea")
										{
											last_minute_1_5_hack_name = "Dish o' The Sea";
										}
									}
								}
								else if (last_minute_1_5_hack_name == "Cranberry Sauce")
								{
									last_minute_1_5_hack_name = "Cran. Sauce";
								}
								break;
							}
							case 16:
								if (last_minute_1_5_hack_name == "Vegetable Medley")
								{
									last_minute_1_5_hack_name = "Vegetable Stew";
								}
								break;
							case 17:
								if (last_minute_1_5_hack_name == "Eggplant Parmesan")
								{
									last_minute_1_5_hack_name = "Eggplant Parm.";
								}
								break;
							case 18:
								if (last_minute_1_5_hack_name == "Cheese Cauliflower")
								{
									last_minute_1_5_hack_name = "Cheese Cauli.";
								}
								break;
							}
						}
						else if (last_minute_1_5_hack_name == "Cookie")
						{
							last_minute_1_5_hack_name = "Cookies";
						}
					}
					if (Game1.player.recipesCooked.ContainsKey(id))
					{
						farmerHas = true;
					}
					else if (Game1.player.cookingRecipes.ContainsKey(last_minute_1_5_hack_name))
					{
						farmerHasButNotMade = true;
					}
					if (id == "217" || id == "772" || id == "773" || id == "279")
					{
						continue;
					}
					if (id == "873")
					{
						continue;
					}
				}
				else
				{
					if (!Object.isPotentialBasicShipped(id, data.Category, data.ObjectType))
					{
						continue;
					}
					whichCollection = 0;
					farmerHas = Game1.player.basicShipped.ContainsKey(id);
				}
				int xPos = baseX + widthUsed[whichCollection] % collectionWidth * 68;
				int yPos = baseY + widthUsed[whichCollection] / collectionWidth * 68;
				if (yPos > this.yPositionOnScreen + height - 128)
				{
					this.collections[whichCollection].Add(new List<ClickableTextureComponent>());
					widthUsed[whichCollection] = 0;
					xPos = baseX;
					yPos = baseY;
				}
				if (this.collections[whichCollection].Count == 0)
				{
					this.collections[whichCollection].Add(new List<ClickableTextureComponent>());
				}
				List<ClickableTextureComponent> list = this.collections[whichCollection].Last<List<ClickableTextureComponent>>();
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
				list.Add(new ClickableTextureComponent(string.Concat(new string[]
				{
					id,
					" ",
					farmerHas.ToString(),
					" ",
					farmerHasButNotMade.ToString()
				}), new Rectangle(xPos, yPos, 64, 64), null, "", itemData.GetTexture(), itemData.GetSourceRect(0, null), 4f, farmerHas)
				{
					myID = list.Count,
					rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? -1 : (list.Count + 1)),
					leftNeighborID = ((list.Count % collectionWidth == 0) ? 7001 : (list.Count - 1)),
					downNeighborID = ((yPos + 68 > this.yPositionOnScreen + height - 128) ? -7777 : (list.Count + collectionWidth)),
					upNeighborID = ((list.Count < collectionWidth) ? 12347 : (list.Count - collectionWidth)),
					fullyImmutable = true
				});
				widthUsed[whichCollection]++;
			}
			if (this.collections[5].Count == 0)
			{
				this.collections[5].Add(new List<ClickableTextureComponent>());
			}
			foreach (KeyValuePair<int, string> kvp in Game1.achievements)
			{
				bool farmerHas2 = Game1.player.achievements.Contains(kvp.Key);
				string[] split = kvp.Value.Split('^', StringSplitOptions.None);
				if (farmerHas2 || (split[2].Equals("true") && (split[3].Equals("-1") || this.farmerHasAchievements(split[3]))))
				{
					int xPos2 = baseX + widthUsed[5] % collectionWidth * 68;
					int yPos2 = baseY + widthUsed[5] / collectionWidth * 68;
					this.collections[5][0].Add(new ClickableTextureComponent(kvp.Key.ToString() + " " + farmerHas2.ToString(), new Rectangle(xPos2, yPos2, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25, -1, -1), 1f, false));
					widthUsed[5]++;
				}
				else
				{
					int xPos3 = baseX + widthUsed[5] % collectionWidth * 68;
					int yPos3 = baseY + widthUsed[5] / collectionWidth * 68;
					this.collections[5][0].Add(new ClickableTextureComponent("??? false", new Rectangle(xPos3, yPos3, 64, 64), null, "???", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25, -1, -1), 1f, false));
					widthUsed[5]++;
				}
			}
			if (Game1.player.secretNotesSeen.Count > 0)
			{
				if (this.collections[6].Count == 0)
				{
					this.collections[6].Add(new List<ClickableTextureComponent>());
				}
				this.secretNotesData = DataLoader.SecretNotes(Game1.content);
				this.secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
				bool show_journals = Game1.player.secretNotesSeen.Contains(GameLocation.JOURNAL_INDEX + 1);
				foreach (int j in this.secretNotesData.Keys)
				{
					if (j >= GameLocation.JOURNAL_INDEX)
					{
						if (!show_journals)
						{
							continue;
						}
					}
					else if (!Game1.player.hasMagnifyingGlass)
					{
						continue;
					}
					int xPos4 = baseX + widthUsed[6] % collectionWidth * 68;
					int yPos4 = baseY + widthUsed[6] / collectionWidth * 68;
					if (j >= GameLocation.JOURNAL_INDEX)
					{
						this.collections[6][0].Add(new ClickableTextureComponent(j.ToString() + " " + Game1.player.secretNotesSeen.Contains(j).ToString(), new Rectangle(xPos4, yPos4, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 842, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(j)));
					}
					else
					{
						this.collections[6][0].Add(new ClickableTextureComponent(j.ToString() + " " + Game1.player.secretNotesSeen.Contains(j).ToString(), new Rectangle(xPos4, yPos4, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 79, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(j)));
					}
					widthUsed[6]++;
				}
			}
			if (this.collections[7].Count == 0)
			{
				this.collections[7].Add(new List<ClickableTextureComponent>());
			}
			List<ClickableTextureComponent> letters = this.collections[7].Last<List<ClickableTextureComponent>>();
			Dictionary<string, string> mail = DataLoader.Mail(Game1.content);
			foreach (string s2 in Game1.player.mailReceived)
			{
				string rawText;
				if (mail.TryGetValue(s2, out rawText))
				{
					int xPos5 = baseX + widthUsed[7] % collectionWidth * 68;
					int yPos5 = baseY + widthUsed[7] / collectionWidth * 68;
					string[] split2 = rawText.Split("[#]", StringSplitOptions.None);
					if (yPos5 > this.yPositionOnScreen + height - 128)
					{
						this.collections[7].Add(new List<ClickableTextureComponent>());
						widthUsed[7] = 0;
						xPos5 = baseX;
						yPos5 = baseY;
						letters = this.collections[7].Last<List<ClickableTextureComponent>>();
					}
					letters.Add(new ClickableTextureComponent(s2 + " true " + ((split2.Length > 1) ? split2[1] : "???"), new Rectangle(xPos5, yPos5, 64, 64), null, "", Game1.mouseCursors, new Rectangle(190, 423, 14, 11), 4f, true)
					{
						myID = letters.Count,
						rightNeighborID = (((letters.Count + 1) % collectionWidth == 0) ? -1 : (letters.Count + 1)),
						leftNeighborID = ((letters.Count % collectionWidth == 0) ? 7008 : (letters.Count - 1)),
						downNeighborID = ((yPos5 + 68 > this.yPositionOnScreen + height - 128) ? -7777 : (letters.Count + collectionWidth)),
						upNeighborID = ((letters.Count < collectionWidth) ? 12347 : (letters.Count - collectionWidth)),
						fullyImmutable = true
					});
					widthUsed[7]++;
				}
			}
		}

		// Token: 0x06002849 RID: 10313 RVA: 0x001D5BF0 File Offset: 0x001D3DF0
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			switch (direction)
			{
			case 1:
				if (oldID == 706 && this.collections[this.currentTab].Count > this.currentPage + 1)
				{
					this.currentlySnappedComponent = base.getComponentWithID(707);
				}
				break;
			case 2:
				if (this.currentPage > 0)
				{
					this.currentlySnappedComponent = base.getComponentWithID(706);
				}
				else if (this.currentPage == 0 && this.collections[this.currentTab].Count > 1)
				{
					this.currentlySnappedComponent = base.getComponentWithID(707);
				}
				this.backButton.upNeighborID = oldID;
				this.forwardButton.upNeighborID = oldID;
				return;
			case 3:
				if (oldID == 707 && this.currentPage > 0)
				{
					this.currentlySnappedComponent = base.getComponentWithID(706);
					return;
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x0600284A RID: 10314 RVA: 0x001D5CDE File Offset: 0x001D3EDE
		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x0600284B RID: 10315 RVA: 0x001D5CFC File Offset: 0x001D3EFC
		public void postWindowSizeChange(IClickableMenu oldPage)
		{
			CollectionsPage oldCollectionsPage = oldPage as CollectionsPage;
			if (oldCollectionsPage != null)
			{
				ClickableTextureComponent clickableTextureComponent = this.sideTabs[this.currentTab];
				clickableTextureComponent.bounds.X = clickableTextureComponent.bounds.X - CollectionsPage.widthToMoveActiveTab;
				this.currentTab = oldCollectionsPage.currentTab;
				this.currentPage = oldCollectionsPage.currentPage;
				ClickableTextureComponent clickableTextureComponent2 = this.sideTabs[this.currentTab];
				clickableTextureComponent2.bounds.X = clickableTextureComponent2.bounds.X + CollectionsPage.widthToMoveActiveTab;
			}
		}

		// Token: 0x0600284C RID: 10316 RVA: 0x001D5D74 File Offset: 0x001D3F74
		private bool farmerHasAchievements(string listOfAchievementNumbers)
		{
			foreach (string s in ArgUtility.SplitBySpace(listOfAchievementNumbers))
			{
				if (!Game1.player.achievements.Contains(Convert.ToInt32(s)))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600284D RID: 10317 RVA: 0x001D5DB4 File Offset: 0x001D3FB4
		public override bool readyToClose()
		{
			return this.letterviewerSubMenu == null && base.readyToClose();
		}

		// Token: 0x0600284E RID: 10318 RVA: 0x001D5DC8 File Offset: 0x001D3FC8
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.letterviewerSubMenu != null)
			{
				this.letterviewerSubMenu.update(time);
				if (this.letterviewerSubMenu.destroy)
				{
					this.letterviewerSubMenu = null;
					if (Game1.options.SnappyMenus)
					{
						this.snapCursorToCurrentSnappedComponent();
					}
				}
			}
		}

		// Token: 0x0600284F RID: 10319 RVA: 0x001D5E16 File Offset: 0x001D4016
		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			LetterViewerMenu letterViewerMenu = this.letterviewerSubMenu;
			if (letterViewerMenu == null)
			{
				return;
			}
			letterViewerMenu.receiveKeyPress(key);
		}

		// Token: 0x06002850 RID: 10320 RVA: 0x001D5E30 File Offset: 0x001D4030
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.letterviewerSubMenu != null)
			{
				this.letterviewerSubMenu.receiveLeftClick(x, y, true);
				return;
			}
			foreach (KeyValuePair<int, ClickableTextureComponent> v in this.sideTabs)
			{
				if (v.Value.containsPoint(x, y) && this.currentTab != v.Key)
				{
					Game1.playSound("smallSelect", null);
					ClickableTextureComponent clickableTextureComponent = this.sideTabs[this.currentTab];
					clickableTextureComponent.bounds.X = clickableTextureComponent.bounds.X - CollectionsPage.widthToMoveActiveTab;
					this.currentTab = Convert.ToInt32(v.Value.name);
					this.currentPage = 0;
					ClickableTextureComponent clickableTextureComponent2 = v.Value;
					clickableTextureComponent2.bounds.X = clickableTextureComponent2.bounds.X + CollectionsPage.widthToMoveActiveTab;
				}
			}
			if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
			{
				this.currentPage--;
				Game1.playSound("shwip", null);
				this.backButton.scale = this.backButton.baseScale;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == 0)
				{
					this.currentlySnappedComponent = this.forwardButton;
					Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
				}
			}
			if (this.currentPage < this.collections[this.currentTab].Count - 1 && this.forwardButton.containsPoint(x, y))
			{
				this.currentPage++;
				Game1.playSound("shwip", null);
				this.forwardButton.scale = this.forwardButton.baseScale;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == this.collections[this.currentTab].Count - 1)
				{
					this.currentlySnappedComponent = this.backButton;
					Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
				}
			}
			int num = this.currentTab;
			if (num != 6)
			{
				if (num != 7)
				{
					return;
				}
				Dictionary<string, string> mail = DataLoader.Mail(Game1.content);
				using (List<ClickableTextureComponent>.Enumerator enumerator2 = this.collections[this.currentTab][this.currentPage].GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ClickableComponent c = enumerator2.Current;
						if (c.containsPoint(x, y))
						{
							string id = ArgUtility.SplitBySpaceAndGet(c.name, 0, null);
							this.letterviewerSubMenu = new LetterViewerMenu(mail[id], id, true);
						}
					}
					return;
				}
			}
			foreach (ClickableComponent c2 in this.collections[this.currentTab][this.currentPage])
			{
				if (c2.containsPoint(x, y))
				{
					string[] split = ArgUtility.SplitBySpace(c2.name);
					int index;
					if (split[1] == "True" && int.TryParse(split[0], out index))
					{
						this.letterviewerSubMenu = new LetterViewerMenu(index);
						this.letterviewerSubMenu.isFromCollection = true;
						break;
					}
				}
			}
		}

		// Token: 0x06002851 RID: 10321 RVA: 0x001D61D0 File Offset: 0x001D43D0
		public override bool shouldDrawCloseButton()
		{
			return this.letterviewerSubMenu == null;
		}

		// Token: 0x06002852 RID: 10322 RVA: 0x001D61DB File Offset: 0x001D43DB
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			LetterViewerMenu letterViewerMenu = this.letterviewerSubMenu;
			if (letterViewerMenu == null)
			{
				return;
			}
			letterViewerMenu.receiveRightClick(x, y, true);
		}

		// Token: 0x06002853 RID: 10323 RVA: 0x001D61F0 File Offset: 0x001D43F0
		public override void applyMovementKey(int direction)
		{
			if (this.letterviewerSubMenu != null)
			{
				this.letterviewerSubMenu.applyMovementKey(direction);
				return;
			}
			base.applyMovementKey(direction);
		}

		// Token: 0x06002854 RID: 10324 RVA: 0x001D620E File Offset: 0x001D440E
		public override void gamePadButtonHeld(Buttons b)
		{
			if (this.letterviewerSubMenu != null)
			{
				this.letterviewerSubMenu.gamePadButtonHeld(b);
				return;
			}
			base.gamePadButtonHeld(b);
		}

		// Token: 0x06002855 RID: 10325 RVA: 0x001D622C File Offset: 0x001D442C
		public override void receiveGamePadButton(Buttons button)
		{
			if (this.letterviewerSubMenu != null)
			{
				this.letterviewerSubMenu.receiveGamePadButton(button);
				return;
			}
			base.receiveGamePadButton(button);
		}

		// Token: 0x06002856 RID: 10326 RVA: 0x001D624C File Offset: 0x001D444C
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.value = -1;
			this.secretNoteImage = -1;
			if (this.letterviewerSubMenu != null)
			{
				this.letterviewerSubMenu.performHoverAction(x, y);
				return;
			}
			foreach (ClickableTextureComponent c in this.sideTabs.Values)
			{
				if (c.containsPoint(x, y))
				{
					this.hoverText = c.hoverText;
					return;
				}
			}
			bool hoveredAny = false;
			foreach (ClickableTextureComponent c2 in this.collections[this.currentTab][this.currentPage])
			{
				if (c2.containsPoint(x, y, 2))
				{
					c2.scale = Math.Min(c2.scale + 0.02f, c2.baseScale + 0.1f);
					string[] data_split = ArgUtility.SplitBySpace(c2.name);
					if (this.currentTab == 5 || (data_split.Length > 1 && Convert.ToBoolean(data_split[1])) || (data_split.Length > 2 && Convert.ToBoolean(data_split[2])))
					{
						if (this.currentTab == 7)
						{
							this.hoverText = Game1.parseText(c2.name.Substring(c2.name.IndexOf(' ', c2.name.IndexOf(' ') + 1) + 1), Game1.smallFont, 256);
						}
						else
						{
							this.hoverText = this.createDescription(data_split[0]);
						}
					}
					else
					{
						if (this.hoverText != "???")
						{
							this.hoverItem = null;
						}
						this.hoverText = "???";
					}
					hoveredAny = true;
				}
				else
				{
					c2.scale = Math.Max(c2.scale - 0.02f, c2.baseScale);
				}
			}
			if (!hoveredAny)
			{
				this.hoverItem = null;
			}
			this.forwardButton.tryHover(x, y, 0.5f);
			this.backButton.tryHover(x, y, 0.5f);
		}

		// Token: 0x06002857 RID: 10327 RVA: 0x001D64A0 File Offset: 0x001D46A0
		public string createDescription(string id)
		{
			string description = "";
			int num = this.currentTab;
			if (num != 5)
			{
				if (num != 6)
				{
					ParsedItemData data = ItemRegistry.GetDataOrErrorItem(id);
					string displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabName", true) ?? data.DisplayName;
					string dataDescription = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabDescription", true) ?? data.Description;
					description = string.Concat(new string[]
					{
						description,
						displayName,
						Environment.NewLine,
						Environment.NewLine,
						Game1.parseText(dataDescription, Game1.smallFont, 256),
						Environment.NewLine,
						Environment.NewLine
					});
					string objectType = data.ObjectType;
					if (!(objectType == "Arch"))
					{
						if (!(objectType == "Cooking"))
						{
							int[] fields;
							if (!(objectType == "Fish"))
							{
								if (data.ObjectType == "Minerals" || data.Category == -2)
								{
									int timesFound;
									description += Game1.content.LoadString("Strings\\UI:Collections_Description_MineralsFound", Game1.player.mineralsFound.TryGetValue(id, out timesFound) ? timesFound : 0);
								}
								else
								{
									int timesFound2;
									description += Game1.content.LoadString("Strings\\UI:Collections_Description_NumberShipped", Game1.player.basicShipped.TryGetValue(id, out timesFound2) ? timesFound2 : 0);
								}
							}
							else if (Game1.player.fishCaught.TryGetValue("(O)" + id, out fields))
							{
								description += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", fields[0]);
								if (fields[1] > 0)
								{
									description = description + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en) ? Math.Round((double)fields[1] * 2.54) : ((double)fields[1])));
								}
							}
							else
							{
								description += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", 0);
							}
						}
						else
						{
							int timesCooked;
							description += (Game1.player.recipesCooked.TryGetValue(id, out timesCooked) ? Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", timesCooked) : "");
							if (this.hoverItem == null || this.hoverItem.ItemId != id)
							{
								this.hoverItem = new Object(id, 1, false, -1, 0);
								string last_minute_1_5_hack_name = this.hoverItem.Name;
								if (last_minute_1_5_hack_name != null)
								{
									int length = last_minute_1_5_hack_name.Length;
									if (length != 6)
									{
										switch (length)
										{
										case 13:
											if (last_minute_1_5_hack_name == "Cheese Cauli.")
											{
												last_minute_1_5_hack_name = "Cheese Cauliflower";
											}
											break;
										case 15:
										{
											char c = last_minute_1_5_hack_name[0];
											if (c != 'C')
											{
												if (c == 'D')
												{
													if (last_minute_1_5_hack_name == "Dish O' The Sea")
													{
														last_minute_1_5_hack_name = "Dish o' The Sea";
													}
												}
											}
											else if (last_minute_1_5_hack_name == "Cranberry Sauce")
											{
												last_minute_1_5_hack_name = "Cran. Sauce";
											}
											break;
										}
										case 16:
											if (last_minute_1_5_hack_name == "Vegetable Medley")
											{
												last_minute_1_5_hack_name = "Vegetable Stew";
											}
											break;
										case 17:
											if (last_minute_1_5_hack_name == "Eggplant Parmesan")
											{
												last_minute_1_5_hack_name = "Eggplant Parm.";
											}
											break;
										case 18:
											if (last_minute_1_5_hack_name == "Cheese Cauliflower")
											{
												last_minute_1_5_hack_name = "Cheese Cauli.";
											}
											break;
										}
									}
									else if (last_minute_1_5_hack_name == "Cookie")
									{
										last_minute_1_5_hack_name = "Cookies";
									}
								}
								this.hoverCraftingRecipe = new CraftingRecipe(last_minute_1_5_hack_name, true);
							}
						}
					}
					else
					{
						int[] fields2;
						description += (Game1.player.archaeologyFound.TryGetValue(id, out fields2) ? Game1.content.LoadString("Strings\\UI:Collections_Description_ArtifactsFound", fields2[0]) : "");
					}
					this.value = ObjectDataDefinition.GetRawPrice(data);
				}
				else if (this.secretNotesData != null)
				{
					int index = int.Parse(id);
					if (index < GameLocation.JOURNAL_INDEX)
					{
						description = description + Game1.content.LoadString("Strings\\Locations:Secret_Note_Name") + " #" + index.ToString();
					}
					else
					{
						description = description + Game1.content.LoadString("Strings\\Locations:Journal_Name") + " #" + (index - GameLocation.JOURNAL_INDEX).ToString();
					}
					if (this.secretNotesData[index][0] == '!')
					{
						this.secretNoteImage = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(this.secretNotesData[index], 1, null));
					}
					else
					{
						string letter_text = Game1.parseText(Utility.ParseGiftReveals(this.secretNotesData[index]).TrimStart(new char[]
						{
							' ',
							'^'
						}).Replace("^", Environment.NewLine).Replace("@", Game1.player.name.Value), Game1.smallFont, 512);
						string[] split = letter_text.Split(Environment.NewLine, StringSplitOptions.None);
						int max_lines = 15;
						if (split.Length > max_lines)
						{
							string[] new_split = new string[max_lines];
							for (int i = 0; i < max_lines; i++)
							{
								new_split[i] = split[i];
							}
							letter_text = string.Join(Environment.NewLine, new_split).Trim() + Environment.NewLine + "(...)";
						}
						description = description + Environment.NewLine + Environment.NewLine + letter_text;
					}
				}
			}
			else
			{
				if (id == "???")
				{
					return "???";
				}
				int index2 = int.Parse(id);
				string[] split2 = Game1.achievements[index2].Split('^', StringSplitOptions.None);
				description = description + split2[0] + Environment.NewLine + Environment.NewLine;
				description += split2[1];
			}
			return description;
		}

		// Token: 0x06002858 RID: 10328 RVA: 0x001D6AA4 File Offset: 0x001D4CA4
		public override void draw(SpriteBatch b)
		{
			foreach (ClickableTextureComponent clickableTextureComponent in this.sideTabs.Values)
			{
				clickableTextureComponent.draw(b);
			}
			if (this.currentPage > 0)
			{
				this.backButton.draw(b);
			}
			if (this.currentPage < this.collections[this.currentTab].Count - 1)
			{
				this.forwardButton.draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			foreach (ClickableTextureComponent c in this.collections[this.currentTab][this.currentPage])
			{
				string[] nameParts = ArgUtility.SplitBySpace(c.name);
				bool drawColor = Convert.ToBoolean(nameParts[1]);
				bool drawColorFaded = (this.currentTab == 4 && Convert.ToBoolean(nameParts[2])) || (this.currentTab == 5 && !drawColor && c.hoverText != "???");
				c.draw(b, drawColorFaded ? (Color.DimGray * 0.4f) : (drawColor ? Color.White : (Color.Black * 0.2f)), 0.86f, 0, 0, 0);
				if (this.currentTab == 5 && drawColor)
				{
					int startPos = Utility.CreateRandom((double)Convert.ToInt32(nameParts[0]), 0.0, 0.0, 0.0, 0.0).Next(12);
					b.Draw(Game1.mouseCursors, new Vector2((float)(c.bounds.X + 16 + 16), (float)(c.bounds.Y + 20 + 16)), new Rectangle?(new Rectangle(256 + startPos % 6 * 64 / 2, 128 + startPos / 6 * 64 / 2, 32, 32)), Color.White, 0f, new Vector2(16f, 16f), c.scale, SpriteEffects.None, 0.88f);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (this.hoverItem != null)
			{
				string desc = this.hoverItem.getDescription();
				string name = this.hoverItem.DisplayName;
				if (desc.Contains("{0}"))
				{
					string replaced_desc = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoverItem.Name + "_CollectionsTabDescription", true);
					if (replaced_desc != null)
					{
						desc = replaced_desc;
					}
					string replaced_name = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoverItem.Name + "_CollectionsTabName", true);
					if (replaced_name != null)
					{
						name = replaced_name;
					}
				}
				IClickableMenu.drawToolTip(b, desc, name, this.hoverItem, false, -1, 0, null, -1, this.hoverCraftingRecipe, -1, null);
			}
			else if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, this.value, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				if (this.secretNoteImage != -1)
				{
					IClickableMenu.drawTextureBox(b, Game1.getOldMouseX(), Game1.getOldMouseY() + 64 + 32, 288, 288, Color.White);
					b.Draw(this.secretNoteImageTexture, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 64 + 32 + 16)), new Rectangle?(new Rectangle(this.secretNoteImage * 64 % this.secretNoteImageTexture.Width, this.secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				}
			}
			LetterViewerMenu letterViewerMenu = this.letterviewerSubMenu;
			if (letterViewerMenu == null)
			{
				return;
			}
			letterViewerMenu.draw(b);
		}

		// Token: 0x04001A01 RID: 6657
		public const int region_sideTabShipped = 7001;

		// Token: 0x04001A02 RID: 6658
		public const int region_sideTabFish = 7002;

		// Token: 0x04001A03 RID: 6659
		public const int region_sideTabArtifacts = 7003;

		// Token: 0x04001A04 RID: 6660
		public const int region_sideTabMinerals = 7004;

		// Token: 0x04001A05 RID: 6661
		public const int region_sideTabCooking = 7005;

		// Token: 0x04001A06 RID: 6662
		public const int region_sideTabAchivements = 7006;

		// Token: 0x04001A07 RID: 6663
		public const int region_sideTabSecretNotes = 7007;

		// Token: 0x04001A08 RID: 6664
		public const int region_sideTabLetters = 7008;

		// Token: 0x04001A09 RID: 6665
		public const int region_forwardButton = 707;

		// Token: 0x04001A0A RID: 6666
		public const int region_backButton = 706;

		// Token: 0x04001A0B RID: 6667
		public static int widthToMoveActiveTab = 8;

		// Token: 0x04001A0C RID: 6668
		public const int organicsTab = 0;

		// Token: 0x04001A0D RID: 6669
		public const int fishTab = 1;

		// Token: 0x04001A0E RID: 6670
		public const int archaeologyTab = 2;

		// Token: 0x04001A0F RID: 6671
		public const int mineralsTab = 3;

		// Token: 0x04001A10 RID: 6672
		public const int cookingTab = 4;

		// Token: 0x04001A11 RID: 6673
		public const int achievementsTab = 5;

		// Token: 0x04001A12 RID: 6674
		public const int secretNotesTab = 6;

		// Token: 0x04001A13 RID: 6675
		public const int lettersTab = 7;

		// Token: 0x04001A14 RID: 6676
		public const int distanceFromMenuBottomBeforeNewPage = 128;

		// Token: 0x04001A15 RID: 6677
		private string hoverText = "";

		// Token: 0x04001A16 RID: 6678
		public ClickableTextureComponent backButton;

		// Token: 0x04001A17 RID: 6679
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001A18 RID: 6680
		public Dictionary<int, ClickableTextureComponent> sideTabs = new Dictionary<int, ClickableTextureComponent>();

		// Token: 0x04001A19 RID: 6681
		public int currentTab;

		// Token: 0x04001A1A RID: 6682
		public int currentPage;

		// Token: 0x04001A1B RID: 6683
		public int secretNoteImage = -1;

		// Token: 0x04001A1C RID: 6684
		public Dictionary<int, List<List<ClickableTextureComponent>>> collections = new Dictionary<int, List<List<ClickableTextureComponent>>>();

		// Token: 0x04001A1D RID: 6685
		public Dictionary<int, string> secretNotesData;

		// Token: 0x04001A1E RID: 6686
		public Texture2D secretNoteImageTexture;

		// Token: 0x04001A1F RID: 6687
		public LetterViewerMenu letterviewerSubMenu;

		// Token: 0x04001A20 RID: 6688
		private Item hoverItem;

		// Token: 0x04001A21 RID: 6689
		private CraftingRecipe hoverCraftingRecipe;

		// Token: 0x04001A22 RID: 6690
		private int value;
	}
}

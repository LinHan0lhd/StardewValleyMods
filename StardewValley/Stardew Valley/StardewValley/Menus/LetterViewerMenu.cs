using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Triggers;

namespace StardewValley.Menus
{
	// Token: 0x02000283 RID: 643
	public class LetterViewerMenu : IClickableMenu
	{
		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06002A7D RID: 10877 RVA: 0x001FDF7A File Offset: 0x001FC17A
		public bool HasQuestOrSpecialOrder
		{
			get
			{
				return this.questID != null || this.specialOrderId != null;
			}
		}

		// Token: 0x06002A7E RID: 10878 RVA: 0x001FDF90 File Offset: 0x001FC190
		public LetterViewerMenu(string text) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).Y, 1280, 720, true)
		{
			Game1.playSound("shwip", null);
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			text = this.ApplyCustomFormatting(text);
			this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(text, this.width - 64, this.height - 128);
			this.forwardButton.visible = (this.page < this.mailMessage.Count - 1);
			this.backButton.visible = (this.page > 0);
			this.OnPageChange();
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A7F RID: 10879 RVA: 0x001FE17C File Offset: 0x001FC37C
		public LetterViewerMenu(int secretNoteIndex) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).Y, 1280, 720, true)
		{
			Game1.playSound("shwip", null);
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			string data = DataLoader.SecretNotes(Game1.content)[secretNoteIndex];
			if (data[0] == '!')
			{
				this.secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
				this.secretNoteImage = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(data, 1, null));
			}
			else
			{
				this.whichBG = ((secretNoteIndex <= 1000) ? 1 : 0);
				string note_text = this.ApplyCustomFormatting(Utility.ParseGiftReveals(data.Replace("@", Game1.player.name.Value)));
				this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(note_text, this.width - 64, this.height - 128);
			}
			this.OnPageChange();
			this.forwardButton.visible = (this.page < this.mailMessage.Count - 1);
			this.backButton.visible = (this.page > 0);
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A80 RID: 10880 RVA: 0x001FE3DC File Offset: 0x001FC5DC
		public virtual void OnPageChange()
		{
			this.forwardButton.visible = (this.page < this.mailMessage.Count - 1);
			this.backButton.visible = (this.page > 0);
			foreach (ClickableComponent clickableComponent in this.itemsToGrab)
			{
				clickableComponent.visible = this.ShouldShowInteractable();
			}
			if (this.acceptQuestButton != null)
			{
				this.acceptQuestButton.visible = this.ShouldShowInteractable();
			}
			if (Game1.options.SnappyMenus && (this.currentlySnappedComponent == null || !this.currentlySnappedComponent.visible))
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A81 RID: 10881 RVA: 0x001FE4A8 File Offset: 0x001FC6A8
		public LetterViewerMenu(string mail, string mailTitle, bool fromCollection = false) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).Y, 1280, 720, true)
		{
			this.isFromCollection = fromCollection;
			this.mailTitle = mailTitle;
			this.isMail = true;
			Game1.playSound("shwip", null);
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			this.acceptQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 128, this.yPositionOnScreen + this.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 103,
				rightNeighborID = 102,
				leftNeighborID = 101
			};
			this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			if (mailTitle.Equals("winter_5_2") || mailTitle.Equals("winter_12_1") || mailTitle.ContainsIgnoreCase("wizard"))
			{
				this.whichBG = 2;
			}
			else if (mailTitle.Equals("Sandy"))
			{
				this.whichBG = 1;
			}
			else if (mailTitle.Contains("Krobus"))
			{
				this.whichBG = 3;
			}
			else if (mailTitle.Contains("passedOut1") || mailTitle.Equals("landslideDone") || mailTitle.Equals("FizzIntro"))
			{
				this.whichBG = 4;
			}
			try
			{
				mail = mail.Split("[#]", StringSplitOptions.None)[0];
				mail = mail.Replace("@", Game1.player.Name);
				mail = Dialogue.applyGenderSwitch(Game1.player.Gender, mail, true);
				mail = this.ApplyCustomFormatting(mail);
				mail = this.HandleActionCommand(mail);
				mail = this.HandleItemCommand(mail);
				bool hideSecretSanta = fromCollection && (Game1.season != Season.Winter || Game1.dayOfMonth < 18 || Game1.dayOfMonth > 25);
				mail = mail.Replace("%secretsanta", hideSecretSanta ? "???" : Utility.GetRandomWinterStarParticipant(null).displayName);
				if (mailTitle.Equals("winter_18") && !fromCollection)
				{
					Game1.player.mailReceived.Add("sawSecretSanta" + Game1.year.ToString());
				}
			}
			catch (Exception ex)
			{
				Game1.log.Error("Letter '" + this.mailTitle + "' couldn't be parsed.", ex);
				mail = "...";
			}
			if (mailTitle == "ccBulletinThankYou" && !Game1.player.hasOrWillReceiveMail("ccBulletinThankYouReceived"))
			{
				Utility.ForEachVillager(delegate(NPC n)
				{
					if (!n.datable.Value)
					{
						Game1.player.changeFriendship(500, n);
					}
					return true;
				}, false);
				Game1.addMailForTomorrow("ccBulletinThankYouReceived", true, false);
			}
			int page_height = this.height - 128;
			if (this.HasInteractable())
			{
				page_height = this.height - 128 - 32;
			}
			this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(mail, this.width - 64, page_height);
			if (this.mailMessage.Count == 0)
			{
				this.mailMessage.Add("[" + mailTitle + "]");
			}
			this.forwardButton.visible = (this.page < this.mailMessage.Count - 1);
			this.backButton.visible = (this.page > 0);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
				if (this.mailMessage.Count <= 1)
				{
					this.backButton.myID = -100;
					this.forwardButton.myID = -100;
				}
			}
		}

		// Token: 0x06002A82 RID: 10882 RVA: 0x001FE980 File Offset: 0x001FCB80
		public string HandleActionCommand(string mail)
		{
			int searchFromIndex = 0;
			for (;;)
			{
				int startItemIndex = mail.IndexOf("%action", searchFromIndex, StringComparison.InvariantCulture);
				if (startItemIndex < 0)
				{
					break;
				}
				int endItemIndex = mail.IndexOf("%%", startItemIndex, StringComparison.InvariantCulture);
				if (endItemIndex < 0)
				{
					break;
				}
				string substring = mail.Substring(startItemIndex, endItemIndex + 2 - startItemIndex);
				mail = mail.Substring(0, startItemIndex) + mail.Substring(startItemIndex + substring.Length);
				string action = substring.Substring("%action".Length, substring.Length - "%action".Length - "%%".Length);
				searchFromIndex = startItemIndex;
				string error;
				Exception ex;
				if (!this.isFromCollection && !TriggerActionManager.TryRunAction(action, out error, out ex))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Letter '");
					defaultInterpolatedStringHandler.AppendFormatted(this.mailTitle);
					defaultInterpolatedStringHandler.AppendLiteral("' has invalid action command '");
					defaultInterpolatedStringHandler.AppendFormatted(action);
					defaultInterpolatedStringHandler.AppendLiteral("': ");
					defaultInterpolatedStringHandler.AppendFormatted(error);
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				}
			}
			return mail;
		}

		// Token: 0x06002A83 RID: 10883 RVA: 0x001FEA98 File Offset: 0x001FCC98
		public string HandleItemCommand(string mail)
		{
			int searchFromIndex = 0;
			for (;;)
			{
				int startItemIndex = mail.IndexOf("%item", searchFromIndex, StringComparison.InvariantCulture);
				if (startItemIndex < 0)
				{
					break;
				}
				int endItemIndex = mail.IndexOf("%%", startItemIndex, StringComparison.InvariantCulture);
				if (endItemIndex < 0)
				{
					break;
				}
				string substring = mail.Substring(startItemIndex, endItemIndex + 2 - startItemIndex);
				mail = mail.Substring(0, startItemIndex) + mail.Substring(startItemIndex + substring.Length);
				string[] typeAndArgs = ArgUtility.SplitBySpace(substring.Substring("%item".Length, substring.Length - "%item".Length - "%%".Length), 2);
				string type = typeAndArgs[0];
				string[] args = (typeAndArgs.Length > 1) ? ArgUtility.SplitBySpace(typeAndArgs[1]) : LegacyShims.EmptyArray<string>();
				searchFromIndex = startItemIndex;
				if (!this.isFromCollection)
				{
					string text = type.ToLower();
					if (text != null)
					{
						int i = text.Length;
						switch (i)
						{
						case 2:
							if (text == "id")
							{
								string id;
								int count;
								if (args.Length == 1)
								{
									id = args[0];
									count = 1;
								}
								else
								{
									int index = Game1.random.Next(args.Length);
									index -= index % 2;
									id = args[index];
									count = int.Parse(args[index + 1]);
								}
								Item item = ItemRegistry.Create(id, count, 0, false);
								this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), item)
								{
									myID = 104,
									leftNeighborID = 101,
									rightNeighborID = 102
								});
								this.backButton.rightNeighborID = 104;
								this.forwardButton.leftNeighborID = 104;
							}
							break;
						case 3:
						case 4:
						case 7:
						case 8:
						case 10:
						case 11:
							break;
						case 5:
						{
							char c = text[0];
							if (c != 'm')
							{
								if (c != 'q')
								{
									if (c == 't')
									{
										if (text == "tools")
										{
											foreach (string arg in args)
											{
												Item tool = null;
												if (!(arg == "Axe") && !(arg == "Hoe") && !(arg == "Pickaxe"))
												{
													if (!(arg == "Can"))
													{
														if (arg == "Scythe")
														{
															tool = ItemRegistry.Create("(W)47", 1, 0, false);
														}
													}
													else
													{
														tool = ItemRegistry.Create("(T)WateringCan", 1, 0, false);
													}
												}
												else
												{
													tool = ItemRegistry.Create("(T)" + arg, 1, 0, false);
												}
												if (tool != null)
												{
													this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), tool));
												}
											}
										}
									}
								}
								else if (text == "quest")
								{
									this.questID = args[0];
									if (args.Length > 1)
									{
										if (!Game1.player.mailReceived.Contains("NOQUEST_" + this.questID))
										{
											Game1.player.addQuest(this.questID);
										}
										this.questID = null;
									}
									this.backButton.rightNeighborID = 103;
									this.forwardButton.leftNeighborID = 103;
								}
							}
							else if (text == "money")
							{
								int moneyToAdd = (args.Length > 1) ? Game1.random.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1])) : Convert.ToInt32(args[0]);
								moneyToAdd -= moneyToAdd % 10;
								Game1.player.Money += moneyToAdd;
								this.moneyIncluded = moneyToAdd;
							}
							break;
						}
						case 6:
							if (text == "object")
							{
								int which = Game1.random.Next(args.Length);
								which -= which % 2;
								Item o = ItemRegistry.Create(args[which], int.Parse(args[which + 1]), 0, false);
								this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), o)
								{
									myID = 104,
									leftNeighborID = 101,
									rightNeighborID = 102
								});
								this.backButton.rightNeighborID = 104;
								this.forwardButton.leftNeighborID = 104;
							}
							break;
						case 9:
						{
							char c = text[0];
							if (c != 'b')
							{
								if (c == 'f')
								{
									if (text == "furniture")
									{
										string id2 = Game1.random.ChooseFrom(args);
										Item o2 = ItemRegistry.Create("(F)" + id2, 1, 0, false);
										this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), o2)
										{
											myID = 104,
											leftNeighborID = 101,
											rightNeighborID = 102
										});
										this.backButton.rightNeighborID = 104;
										this.forwardButton.leftNeighborID = 104;
									}
								}
							}
							else if (text == "bigobject")
							{
								string id3 = Game1.random.ChooseFrom(args);
								Item o3 = ItemRegistry.Create("(BC)" + id3, 1, 0, false);
								this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), o3)
								{
									myID = 104,
									leftNeighborID = 101,
									rightNeighborID = 102
								});
								this.backButton.rightNeighborID = 104;
								this.forwardButton.leftNeighborID = 104;
							}
							break;
						}
						case 12:
						{
							char c = text[0];
							if (c != 'i')
							{
								if (c == 's')
								{
									if (text == "specialorder")
									{
										this.specialOrderId = args[0];
										bool addImmediately;
										string text2;
										if (ArgUtility.TryGetBool(args, 1, out addImmediately, out text2, "bool addImmediately") && addImmediately)
										{
											if (!Game1.player.mailReceived.Contains("NOSPECIALORDER_" + this.specialOrderId))
											{
												Game1.player.team.AddSpecialOrder(this.specialOrderId, null, false);
											}
											this.specialOrderId = null;
										}
										this.backButton.rightNeighborID = 103;
										this.forwardButton.leftNeighborID = 103;
									}
								}
							}
							else if (text == "itemrecovery")
							{
								if (Game1.player.recoveredItem != null)
								{
									Item item2 = Game1.player.recoveredItem;
									Game1.player.recoveredItem = null;
									this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), item2)
									{
										myID = 104,
										leftNeighborID = 101,
										rightNeighborID = 102
									});
									this.backButton.rightNeighborID = 104;
									this.forwardButton.leftNeighborID = 104;
								}
							}
							break;
						}
						case 13:
							if (text == "cookingrecipe")
							{
								Dictionary<string, string> cookingRecipes = CraftingRecipe.cookingRecipes;
								string recipeKey = string.Join(" ", args);
								if (string.IsNullOrWhiteSpace(recipeKey))
								{
									int lowest_required_heart_level = 1000;
									foreach (string s in cookingRecipes.Keys)
									{
										string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(cookingRecipes[s].Split('/', StringSplitOptions.None), 3, null, true));
										string conditionKey = ArgUtility.Get(getConditions, 0, null, true);
										string npcName = ArgUtility.Get(getConditions, 1, null, true);
										if (conditionKey == "f" && npcName == this.mailTitle.Replace("Cooking", "") && !Game1.player.cookingRecipes.ContainsKey(s))
										{
											int required_heart_level = Convert.ToInt32(getConditions[2]);
											if (required_heart_level <= lowest_required_heart_level)
											{
												lowest_required_heart_level = required_heart_level;
												recipeKey = s;
											}
										}
									}
								}
								if (!string.IsNullOrWhiteSpace(recipeKey))
								{
									if (cookingRecipes.ContainsKey(recipeKey))
									{
										Game1.player.cookingRecipes.TryAdd(recipeKey, 0);
										this.learnedRecipe = new CraftingRecipe(recipeKey, true).DisplayName;
										this.cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_cooking");
									}
									else
									{
										IGameLogger log = Game1.log;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
										defaultInterpolatedStringHandler.AppendLiteral("Letter '");
										defaultInterpolatedStringHandler.AppendFormatted(this.mailTitle);
										defaultInterpolatedStringHandler.AppendLiteral("' has unknown cooking recipe '");
										defaultInterpolatedStringHandler.AppendFormatted(recipeKey);
										defaultInterpolatedStringHandler.AppendLiteral("'.");
										log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
									}
								}
							}
							break;
						case 14:
							if (text == "craftingrecipe")
							{
								Dictionary<string, string> craftingRecipes = CraftingRecipe.craftingRecipes;
								if (craftingRecipes.ContainsKey(args[0]))
								{
									this.learnedRecipe = args[0];
								}
								else
								{
									string fallbackKey = args[0].Replace('_', ' ');
									if (!craftingRecipes.ContainsKey(fallbackKey))
									{
										IGameLogger log2 = Game1.log;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 3);
										defaultInterpolatedStringHandler.AppendLiteral("Letter '");
										defaultInterpolatedStringHandler.AppendFormatted(this.mailTitle);
										defaultInterpolatedStringHandler.AppendLiteral("' has unknown crafting recipe '");
										defaultInterpolatedStringHandler.AppendFormatted(args[0]);
										defaultInterpolatedStringHandler.AppendLiteral("'");
										defaultInterpolatedStringHandler.AppendFormatted((args[0] != fallbackKey) ? (" or '" + fallbackKey + "'") : "");
										defaultInterpolatedStringHandler.AppendLiteral(".");
										log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
										break;
									}
									this.learnedRecipe = fallbackKey;
								}
								Game1.player.craftingRecipes.TryAdd(this.learnedRecipe, 0);
								this.learnedRecipe = new CraftingRecipe(this.learnedRecipe, false).DisplayName;
								this.cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_crafting");
							}
							break;
						default:
							if (i == 17)
							{
								if (text == "conversationtopic")
								{
									string topic = args[0];
									int numDays = Convert.ToInt32(args[1]);
									Game1.player.activeDialogueEvents[topic] = numDays;
									if (topic.Equals("ElliottGone3"))
									{
										Utility.getHomeOfFarmer(Game1.player).fridge.Value.addItem(ItemRegistry.Create("(O)732", 1, 0, false));
									}
								}
							}
							break;
						}
					}
				}
			}
			return mail;
		}

		// Token: 0x06002A84 RID: 10884 RVA: 0x001FF5E8 File Offset: 0x001FD7E8
		public virtual string ApplyCustomFormatting(string text)
		{
			text = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
			for (int index = text.IndexOf("["); index >= 0; index = text.IndexOf("[", index + 1))
			{
				int end_index = text.IndexOf("]", index);
				if (end_index >= 0)
				{
					bool valid_tag = false;
					try
					{
						string[] split = ArgUtility.SplitBySpace(text.Substring(index + 1, end_index - index - 1));
						string a = split[0];
						if (!(a == "letterbg"))
						{
							if (a == "textcolor")
							{
								string color_string = split[1].ToLower();
								string[] color_lookup = new string[]
								{
									"black",
									"blue",
									"red",
									"purple",
									"white",
									"orange",
									"green",
									"cyan",
									"gray",
									"jojablue"
								};
								this.customTextColor = null;
								for (int i = 0; i < color_lookup.Length; i++)
								{
									if (color_string == color_lookup[i])
									{
										this.customTextColor = new Color?(SpriteText.getColorFromIndex(i));
										break;
									}
								}
								valid_tag = true;
							}
						}
						else
						{
							int num = split.Length;
							if (num != 2)
							{
								if (num == 3)
								{
									this.usingCustomBackground = true;
									this.letterTexture = Game1.temporaryContent.Load<Texture2D>(split[1]);
									this.whichBG = int.Parse(split[2]);
								}
							}
							else
							{
								this.whichBG = int.Parse(split[1]);
							}
							valid_tag = true;
						}
					}
					catch (Exception)
					{
					}
					if (valid_tag)
					{
						text = text.Remove(index, end_index - index + 1);
						index--;
					}
				}
			}
			return text;
		}

		// Token: 0x06002A85 RID: 10885 RVA: 0x001FF7AC File Offset: 0x001FD9AC
		public override void snapToDefaultClickableComponent()
		{
			if (this.HasQuestOrSpecialOrder && this.ShouldShowInteractable())
			{
				this.currentlySnappedComponent = base.getComponentWithID(103);
			}
			else if (this.itemsToGrab.Count > 0 && this.ShouldShowInteractable())
			{
				this.currentlySnappedComponent = base.getComponentWithID(104);
			}
			else if (this.currentlySnappedComponent == null || (this.currentlySnappedComponent != this.backButton && this.currentlySnappedComponent != this.forwardButton))
			{
				this.currentlySnappedComponent = this.forwardButton;
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A86 RID: 10886 RVA: 0x001FF838 File Offset: 0x001FDA38
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).X;
			this.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720, 0, 0).Y;
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			this.acceptQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 128, this.yPositionOnScreen + this.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 103,
				rightNeighborID = 102,
				leftNeighborID = 101
			};
			foreach (ClickableComponent clickableComponent in this.itemsToGrab)
			{
				clickableComponent.bounds = new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height - 32 - 96, 96, 96);
			}
		}

		// Token: 0x06002A87 RID: 10887 RVA: 0x001FFA50 File Offset: 0x001FDC50
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.None)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				base.exitThisMenu(this.ShouldPlayExitSound());
				return;
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x06002A88 RID: 10888 RVA: 0x001FFA8C File Offset: 0x001FDC8C
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button != Buttons.B)
			{
				if (button != Buttons.RightTrigger)
				{
					if (button != Buttons.LeftTrigger)
					{
						return;
					}
					if (this.page > 0)
					{
						this.page--;
						Game1.playSound("shwip", null);
						this.OnPageChange();
						return;
					}
				}
				else if (this.page < this.mailMessage.Count - 1)
				{
					this.page++;
					Game1.playSound("shwip", null);
					this.OnPageChange();
				}
			}
			else if (this.isFromCollection)
			{
				base.exitThisMenu(false);
				return;
			}
		}

		// Token: 0x06002A89 RID: 10889 RVA: 0x001FFB38 File Offset: 0x001FDD38
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.scale < 1f)
			{
				return;
			}
			if (this.upperRightCloseButton != null && this.readyToClose() && this.upperRightCloseButton.containsPoint(x, y))
			{
				if (playSound)
				{
					Game1.playSound("bigDeSelect", null);
				}
				if (!this.isFromCollection)
				{
					base.exitThisMenu(this.ShouldPlayExitSound());
				}
				else
				{
					this.destroy = true;
				}
			}
			if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				this.unload();
				return;
			}
			if (this.ShouldShowInteractable())
			{
				for (int i = 0; i < this.itemsToGrab.Count; i++)
				{
					ClickableComponent c = this.itemsToGrab[i];
					if (c.containsPoint(x, y) && c.item != null)
					{
						Game1.playSound("coin", null);
						Game1.player.addItemByMenuIfNecessary(c.item, null, false);
						c.item = null;
						if (this.itemsToGrab.Count > 1)
						{
							this.itemsToGrab.RemoveAt(i);
						}
						return;
					}
				}
			}
			if (this.backButton.containsPoint(x, y) && this.page > 0)
			{
				this.page--;
				Game1.playSound("shwip", null);
				this.OnPageChange();
				return;
			}
			if (this.forwardButton.containsPoint(x, y) && this.page < this.mailMessage.Count - 1)
			{
				this.page++;
				Game1.playSound("shwip", null);
				this.OnPageChange();
				return;
			}
			if (this.ShouldShowInteractable() && this.acceptQuestButton != null && this.acceptQuestButton.containsPoint(x, y))
			{
				this.AcceptQuest();
				return;
			}
			if (this.isWithinBounds(x, y))
			{
				if (this.page < this.mailMessage.Count - 1)
				{
					this.page++;
					Game1.playSound("shwip", null);
					this.OnPageChange();
					return;
				}
				if (!this.isMail)
				{
					base.exitThisMenuNoSound();
					Game1.playSound("shwip", null);
					return;
				}
				if (this.isFromCollection)
				{
					this.destroy = true;
					return;
				}
			}
			else if (!this.itemsLeftToGrab())
			{
				if (!this.isFromCollection)
				{
					base.exitThisMenuNoSound();
					Game1.playSound("shwip", null);
					return;
				}
				this.destroy = true;
			}
		}

		// Token: 0x06002A8A RID: 10890 RVA: 0x001FFDA1 File Offset: 0x001FDFA1
		public virtual bool ShouldPlayExitSound()
		{
			return !this.HasQuestOrSpecialOrder && !this.isFromCollection;
		}

		// Token: 0x06002A8B RID: 10891 RVA: 0x001FFDB8 File Offset: 0x001FDFB8
		public bool itemsLeftToGrab()
		{
			using (List<ClickableComponent>.Enumerator enumerator = this.itemsToGrab.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.item != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002A8C RID: 10892 RVA: 0x001FFE14 File Offset: 0x001FE014
		public void AcceptQuest()
		{
			if (this.questID != null)
			{
				Game1.player.addQuest(this.questID);
				if (this.questID == "20")
				{
					MineShaft.CheckForQiChallengeCompletion();
				}
				this.questID = null;
				Game1.playSound("newArtifact", null);
				return;
			}
			if (this.specialOrderId != null)
			{
				Game1.player.team.AddSpecialOrder(this.specialOrderId, null, false);
				this.specialOrderId = null;
				Game1.playSound("newArtifact", null);
			}
		}

		// Token: 0x06002A8D RID: 10893 RVA: 0x001FFEB0 File Offset: 0x001FE0B0
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (this.ShouldShowInteractable())
			{
				foreach (ClickableComponent c in this.itemsToGrab)
				{
					if (c.containsPoint(x, y))
					{
						c.scale = Math.Min(c.scale + 0.03f, 1.1f);
					}
					else
					{
						c.scale = Math.Max(1f, c.scale - 0.03f);
					}
				}
			}
			this.backButton.tryHover(x, y, 0.6f);
			this.forwardButton.tryHover(x, y, 0.6f);
			if (this.ShouldShowInteractable() && this.HasQuestOrSpecialOrder)
			{
				float oldScale = this.acceptQuestButton.scale;
				this.acceptQuestButton.scale = (this.acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (this.acceptQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
			}
		}

		// Token: 0x06002A8E RID: 10894 RVA: 0x001FFFE0 File Offset: 0x001FE1E0
		public override void update(GameTime time)
		{
			base.update(time);
			this.forwardButton.visible = (this.page < this.mailMessage.Count - 1);
			this.backButton.visible = (this.page > 0);
			if (this.scale < 1f)
			{
				this.scale += (float)time.ElapsedGameTime.Milliseconds * 0.003f;
				if (this.scale >= 1f)
				{
					this.scale = 1f;
				}
			}
			if (this.page < this.mailMessage.Count - 1 && !this.forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				this.forwardButton.scale = 4f + (float)Math.Sin((double)((float)time.TotalGameTime.Milliseconds) / 201.06192982974676) / 1.5f;
			}
		}

		// Token: 0x06002A8F RID: 10895 RVA: 0x002000D4 File Offset: 0x001FE2D4
		public virtual Color? getTextColor()
		{
			if (this.customTextColor != null)
			{
				return new Color?(this.customTextColor.Value);
			}
			if (this.usingCustomBackground)
			{
				return null;
			}
			switch (this.whichBG)
			{
			case 1:
				return new Color?(SpriteText.color_Gray);
			case 2:
				return new Color?(SpriteText.color_Cyan);
			case 3:
				return new Color?(SpriteText.color_White);
			case 4:
				return new Color?(SpriteText.color_JojaBlue);
			default:
				return null;
			}
		}

		// Token: 0x06002A90 RID: 10896 RVA: 0x00200168 File Offset: 0x001FE368
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}
			b.Draw(this.letterTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2), (float)(this.yPositionOnScreen + this.height / 2)), new Rectangle?(new Rectangle(this.whichBG % 4 * 320, (this.whichBG >= 4) ? (204 + (this.whichBG / 4 - 1) * 180) : 0, 320, 180)), Color.White, 0f, new Vector2(160f, 90f), 4f * this.scale, SpriteEffects.None, 0.86f);
			if (this.scale == 1f)
			{
				if (this.secretNoteImage != -1)
				{
					b.Draw(this.secretNoteImageTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 128 - 4), (float)(this.yPositionOnScreen + this.height / 2 - 128 + 8)), new Rectangle?(new Rectangle(this.secretNoteImage * 64 % this.secretNoteImageTexture.Width, this.secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64)), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(this.secretNoteImageTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 128), (float)(this.yPositionOnScreen + this.height / 2 - 128)), new Rectangle?(new Rectangle(this.secretNoteImage * 64 % this.secretNoteImageTexture.Width, this.secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(this.secretNoteImageTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 40), (float)(this.yPositionOnScreen + this.height / 2 - 192)), new Rectangle?(new Rectangle(193, 65, 14, 21)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.867f);
				}
				else
				{
					SpriteText.drawString(b, this.mailMessage[this.page], this.xPositionOnScreen + 32, this.yPositionOnScreen + 32, 999999, this.width - 64, 999999, 0.75f, 0.865f, false, -1, "", this.getTextColor(), SpriteText.ScrollTextAlignment.Left);
				}
				if (this.ShouldShowInteractable())
				{
					using (List<ClickableComponent>.Enumerator enumerator = this.itemsToGrab.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							ClickableComponent c = enumerator.Current;
							b.Draw(this.letterTexture, c.bounds, new Rectangle?(new Rectangle(this.whichBG * 24, 180, 24, 24)), Color.White);
							Item item = c.item;
							if (item != null)
							{
								item.drawInMenu(b, new Vector2((float)(c.bounds.X + 16), (float)(c.bounds.Y + 16)), c.scale);
							}
						}
					}
					if (this.moneyIncluded > 0)
					{
						string moneyText = Game1.content.LoadString("Strings\\UI:LetterViewer_MoneyIncluded", this.moneyIncluded);
						SpriteText.drawString(b, moneyText, this.xPositionOnScreen + this.width / 2 - SpriteText.getWidthOfString(moneyText, 999999) / 2, this.yPositionOnScreen + this.height - 96, 999999, -1, 9999, 0.75f, 0.865f, false, -1, "", this.getTextColor(), SpriteText.ScrollTextAlignment.Left);
					}
					else
					{
						string text = this.learnedRecipe;
						if (text != null && text.Length > 0)
						{
							string recipeText = Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipe", this.cookingOrCrafting);
							SpriteText.drawStringHorizontallyCenteredAt(b, recipeText, this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height - 32 - SpriteText.getHeightOfString(recipeText, 999999) * 2, 999999, -1, 9999, 0.65f, 0.865f, false, this.getTextColor(), 99999);
							SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipeName", this.learnedRecipe), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height - 32 - SpriteText.getHeightOfString("t", 999999), 999999, -1, 9999, 0.9f, 0.865f, false, this.getTextColor(), 99999);
						}
					}
				}
				base.draw(b);
				this.forwardButton.draw(b);
				this.backButton.draw(b);
				if (this.ShouldShowInteractable() && this.HasQuestOrSpecialOrder)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (this.acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.acceptQuestButton.scale, true, -1f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptQuestButton.bounds.X + 12), (float)(this.acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				}
			}
			if ((!Game1.options.SnappyMenus || this.scale >= 1f) && (!Game1.options.SnappyMenus || this.forwardButton.visible || this.backButton.visible || this.HasQuestOrSpecialOrder || this.itemsLeftToGrab()))
			{
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x06002A91 RID: 10897 RVA: 0x00200818 File Offset: 0x001FEA18
		public virtual bool ShouldShowInteractable()
		{
			return this.HasInteractable() && this.page == this.mailMessage.Count - 1;
		}

		// Token: 0x06002A92 RID: 10898 RVA: 0x0020083C File Offset: 0x001FEA3C
		public virtual bool HasInteractable()
		{
			if (this.isFromCollection)
			{
				return false;
			}
			if (this.HasQuestOrSpecialOrder)
			{
				return true;
			}
			if (this.moneyIncluded > 0)
			{
				return true;
			}
			if (this.itemsToGrab.Count > 0)
			{
				return true;
			}
			string text = this.learnedRecipe;
			return text != null && text.Length > 0;
		}

		// Token: 0x06002A93 RID: 10899 RVA: 0x00200892 File Offset: 0x001FEA92
		public void unload()
		{
		}

		// Token: 0x06002A94 RID: 10900 RVA: 0x00200894 File Offset: 0x001FEA94
		protected override void cleanupBeforeExit()
		{
			if (this.HasQuestOrSpecialOrder)
			{
				this.AcceptQuest();
			}
			if (this.itemsLeftToGrab())
			{
				List<Item> items = new List<Item>();
				foreach (ClickableComponent c in this.itemsToGrab)
				{
					if (c.item != null)
					{
						items.Add(c.item);
					}
				}
				this.itemsToGrab.Clear();
				if (items.Count > 0)
				{
					Game1.playSound("coin", null);
					Game1.player.addItemsByMenuIfNecessary(items, null, false);
				}
			}
			if (this.isFromCollection)
			{
				this.destroy = true;
				Game1.oldKBState = Game1.GetKeyboardState();
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = Game1.input.GetGamePadState();
			}
			base.cleanupBeforeExit();
		}

		// Token: 0x06002A95 RID: 10901 RVA: 0x00200984 File Offset: 0x001FEB84
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.isFromCollection)
			{
				this.destroy = true;
				return;
			}
			this.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x04001C2E RID: 7214
		public const int region_backButton = 101;

		// Token: 0x04001C2F RID: 7215
		public const int region_forwardButton = 102;

		// Token: 0x04001C30 RID: 7216
		public const int region_acceptQuestButton = 103;

		// Token: 0x04001C31 RID: 7217
		public const int region_itemGrabButton = 104;

		// Token: 0x04001C32 RID: 7218
		public const int letterWidth = 320;

		// Token: 0x04001C33 RID: 7219
		public const int letterHeight = 180;

		// Token: 0x04001C34 RID: 7220
		public Texture2D letterTexture;

		// Token: 0x04001C35 RID: 7221
		public Texture2D secretNoteImageTexture;

		// Token: 0x04001C36 RID: 7222
		public int moneyIncluded;

		// Token: 0x04001C37 RID: 7223
		public int secretNoteImage = -1;

		// Token: 0x04001C38 RID: 7224
		public int whichBG;

		// Token: 0x04001C39 RID: 7225
		public string questID;

		// Token: 0x04001C3A RID: 7226
		public string specialOrderId;

		// Token: 0x04001C3B RID: 7227
		public string learnedRecipe = "";

		// Token: 0x04001C3C RID: 7228
		public string cookingOrCrafting = "";

		// Token: 0x04001C3D RID: 7229
		public string mailTitle;

		// Token: 0x04001C3E RID: 7230
		public List<string> mailMessage = new List<string>();

		// Token: 0x04001C3F RID: 7231
		public int page;

		// Token: 0x04001C40 RID: 7232
		public readonly List<ClickableComponent> itemsToGrab = new List<ClickableComponent>();

		// Token: 0x04001C41 RID: 7233
		public float scale;

		// Token: 0x04001C42 RID: 7234
		public bool isMail;

		// Token: 0x04001C43 RID: 7235
		public bool isFromCollection;

		// Token: 0x04001C44 RID: 7236
		public new bool destroy;

		// Token: 0x04001C45 RID: 7237
		public Color? customTextColor;

		// Token: 0x04001C46 RID: 7238
		public bool usingCustomBackground;

		// Token: 0x04001C47 RID: 7239
		public ClickableTextureComponent backButton;

		// Token: 0x04001C48 RID: 7240
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001C49 RID: 7241
		public ClickableComponent acceptQuestButton;

		// Token: 0x04001C4A RID: 7242
		public const float scaleChange = 0.003f;
	}
}

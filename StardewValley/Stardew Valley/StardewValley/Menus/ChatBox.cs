using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.Logging;

namespace StardewValley.Menus
{
	// Token: 0x02000259 RID: 601
	public class ChatBox : IClickableMenu
	{
		// Token: 0x060027EB RID: 10219 RVA: 0x001D0DC4 File Offset: 0x001CEFC4
		public ChatBox()
		{
			this.CheatCommandChatLogger = new CheatCommandChatLogger(this);
			Texture2D chatboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");
			this.chatBox = new ChatTextBox(chatboxTexture, null, Game1.smallFont, Color.White);
			this.chatBox.OnEnterPressed += this.textBoxEnter;
			this.chatBox.TitleText = "Chat";
			this.chatBoxCC = new ClickableComponent(new Rectangle(this.chatBox.X, this.chatBox.Y, this.chatBox.Width, this.chatBox.Height), "")
			{
				myID = 101
			};
			Game1.keyboardDispatcher.Subscriber = this.chatBox;
			ChatBox.emojiTexture = Game1.content.Load<Texture2D>("LooseSprites\\emojis");
			this.emojiMenuIcon = new ClickableTextureComponent(new Rectangle(0, 0, 40, 36), ChatBox.emojiTexture, new Rectangle(0, 0, 9, 9), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			this.emojiMenu = new EmojiMenu(this, ChatBox.emojiTexture, chatboxTexture);
			this.chatBoxCC.rightNeighborID = 102;
			this.updatePosition();
			this.chatBox.Selected = false;
		}

		// Token: 0x060027EC RID: 10220 RVA: 0x001D0F31 File Offset: 0x001CF131
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060027ED RID: 10221 RVA: 0x001D0F48 File Offset: 0x001CF148
		private void updatePosition()
		{
			this.chatBox.Width = 896;
			this.chatBox.Height = 56;
			this.width = this.chatBox.Width;
			this.height = this.chatBox.Height;
			this.xPositionOnScreen = 0;
			this.yPositionOnScreen = Game1.uiViewport.Height - this.chatBox.Height;
			Utility.makeSafe(ref this.xPositionOnScreen, ref this.yPositionOnScreen, this.chatBox.Width, this.chatBox.Height);
			this.chatBox.X = this.xPositionOnScreen;
			this.chatBox.Y = this.yPositionOnScreen;
			this.chatBoxCC.bounds = new Rectangle(this.chatBox.X, this.chatBox.Y, this.chatBox.Width, this.chatBox.Height);
			this.emojiMenuIcon.bounds.Y = this.chatBox.Y + 8;
			this.emojiMenuIcon.bounds.X = this.chatBox.Width - this.emojiMenuIcon.bounds.Width - 8;
			if (this.emojiMenu != null)
			{
				this.emojiMenu.xPositionOnScreen = this.emojiMenuIcon.bounds.Center.X - 146;
				this.emojiMenu.yPositionOnScreen = this.emojiMenuIcon.bounds.Y - 248;
			}
		}

		// Token: 0x060027EE RID: 10222 RVA: 0x001D10D8 File Offset: 0x001CF2D8
		public virtual void textBoxEnter(string text_to_send)
		{
			if (text_to_send.Length < 1)
			{
				return;
			}
			if (text_to_send[0] == '/')
			{
				string text = ArgUtility.SplitBySpaceAndGet(text_to_send, 0, null);
				if (text != null && text.Length > 1)
				{
					this.runCommand(text_to_send.Substring(1));
					return;
				}
			}
			text_to_send = Program.sdk.FilterDirtyWords(text_to_send);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text_to_send, Multiplayer.AllPlayers);
			this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, text_to_send);
		}

		// Token: 0x060027EF RID: 10223 RVA: 0x001D115C File Offset: 0x001CF35C
		public virtual void textBoxEnter(TextBox sender)
		{
			ChatTextBox box = sender as ChatTextBox;
			if (box != null)
			{
				if (box.finalText.Count > 0)
				{
					bool include_color_information = true;
					string message = box.finalText[0].message;
					if (message != null && message.StartsWith('/'))
					{
						string text = ArgUtility.SplitBySpaceAndGet(box.finalText[0].message, 0, null);
						if (text != null && text.Length > 1)
						{
							include_color_information = false;
						}
					}
					if (box.finalText.Count == 1)
					{
						if (box.finalText[0].message == null && box.finalText[0].emojiIndex == -1)
						{
							goto IL_DC;
						}
						string message2 = box.finalText[0].message;
						if (message2 != null && message2.Trim().Length == 0)
						{
							goto IL_DC;
						}
					}
					string textToSend = ChatMessage.makeMessagePlaintext(box.finalText, include_color_information);
					this.textBoxEnter(textToSend);
				}
				IL_DC:
				box.reset();
				this.cheatHistoryPosition = -1;
			}
			sender.Text = "";
			this.clickAway();
		}

		// Token: 0x060027F0 RID: 10224 RVA: 0x001D1263 File Offset: 0x001CF463
		public virtual void addInfoMessage(string message)
		{
			this.receiveChatMessage(0L, 2, LocalizedContentManager.CurrentLanguageCode, message);
		}

		// Token: 0x060027F1 RID: 10225 RVA: 0x001D1274 File Offset: 0x001CF474
		public virtual void globalInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsMultiplayer)
			{
				Game1.multiplayer.globalChatInfoMessage(messageKey, args);
				return;
			}
			this.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_" + messageKey, args));
		}

		// Token: 0x060027F2 RID: 10226 RVA: 0x001D12B3 File Offset: 0x001CF4B3
		public virtual void addErrorMessage(string message)
		{
			this.receiveChatMessage(0L, 1, LocalizedContentManager.CurrentLanguageCode, message);
		}

		// Token: 0x060027F3 RID: 10227 RVA: 0x001D12C4 File Offset: 0x001CF4C4
		public virtual void listPlayers(bool otherPlayersOnly = false, bool onlineOnly = true)
		{
			this.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ListOnlinePlayers"));
			IEnumerable<Farmer> enumerable;
			if (!onlineOnly)
			{
				enumerable = Game1.getAllFarmers();
			}
			else
			{
				IEnumerable<Farmer> onlineFarmers = Game1.getOnlineFarmers();
				enumerable = onlineFarmers;
			}
			foreach (Farmer f in enumerable)
			{
				if (!otherPlayersOnly || f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					this.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ListOnlinePlayersEntry", ChatBox.formattedUserNameLong(f)));
				}
			}
		}

		// Token: 0x060027F4 RID: 10228 RVA: 0x001D135C File Offset: 0x001CF55C
		protected virtual void runCommand(string commandText)
		{
			if (!ChatCommands.TryHandle(ArgUtility.SplitBySpace(commandText), this) && (ChatCommands.AllowCheats || Game1.isRunningMacro))
			{
				this.cheat(commandText, false);
			}
		}

		// Token: 0x060027F5 RID: 10229 RVA: 0x001D1384 File Offset: 0x001CF584
		public virtual void cheat(string command, bool isDebug = false)
		{
			string fullCommand = (isDebug ? "debug " : "") + command;
			Game1.debugOutput = null;
			this.addInfoMessage("/" + fullCommand);
			if (!Game1.isRunningMacro)
			{
				this.cheatHistory.Insert(0, "/" + fullCommand);
			}
			if (Game1.game1.parseDebugInput(command, this.CheatCommandChatLogger))
			{
				if (!string.IsNullOrEmpty(Game1.debugOutput))
				{
					this.addInfoMessage(Game1.debugOutput);
					return;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(Game1.debugOutput))
				{
					this.addErrorMessage(Game1.debugOutput);
					return;
				}
				this.addErrorMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ChatBox.cs.10261") + " " + ArgUtility.SplitBySpaceAndGet(command, 0, null));
			}
		}

		// Token: 0x060027F6 RID: 10230 RVA: 0x001D1448 File Offset: 0x001CF648
		public void replyPrivateMessage(string[] command)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			if (this.lastReceivedPrivateMessagePlayerId == 0L)
			{
				this.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Reply_NoMessageFound"));
				return;
			}
			Farmer lastPlayer;
			if (!Game1.otherFarmers.TryGetValue(this.lastReceivedPrivateMessagePlayerId, out lastPlayer) || !lastPlayer.isActive())
			{
				this.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Reply_Failed"));
				return;
			}
			if (command.Length > 1)
			{
				string message = "";
				for (int i = 1; i < command.Length; i++)
				{
					message += command[i];
					if (i < command.Length - 1)
					{
						message += " ";
					}
				}
				message = Program.sdk.FilterDirtyWords(message);
				Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, this.lastReceivedPrivateMessagePlayerId);
				this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message);
			}
		}

		// Token: 0x060027F7 RID: 10231 RVA: 0x001D1520 File Offset: 0x001CF720
		public Farmer findMatchingFarmer(string[] command, ref int matchingIndex, bool allowMatchingByUserName = false, bool onlineOnly = true)
		{
			Farmer matchingFarmer = null;
			IEnumerable<Farmer> enumerable;
			if (!onlineOnly)
			{
				enumerable = Game1.getAllFarmers();
			}
			else
			{
				IEnumerable<Farmer> values = Game1.otherFarmers.Values;
				enumerable = values;
			}
			foreach (Farmer farmer in enumerable)
			{
				string[] farmerNameSplit = ArgUtility.SplitBySpace(farmer.displayName);
				bool isMatch = true;
				int i;
				for (i = 0; i < farmerNameSplit.Length; i++)
				{
					if (command.Length <= i + 1)
					{
						isMatch = false;
						break;
					}
					if (!command[i + 1].EqualsIgnoreCase(farmerNameSplit[i]))
					{
						isMatch = false;
						break;
					}
				}
				if (isMatch)
				{
					matchingFarmer = farmer;
					matchingIndex = i;
					break;
				}
				if (allowMatchingByUserName)
				{
					isMatch = true;
					string[] userNameSplit = ArgUtility.SplitBySpace(Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID));
					if (userNameSplit.Length != 0)
					{
						for (i = 0; i < userNameSplit.Length; i++)
						{
							if (command.Length <= i + 1)
							{
								isMatch = false;
								break;
							}
							if (!command[i + 1].EqualsIgnoreCase(userNameSplit[i]))
							{
								isMatch = false;
								break;
							}
						}
						if (isMatch)
						{
							matchingFarmer = farmer;
							matchingIndex = i;
							break;
						}
					}
				}
			}
			return matchingFarmer;
		}

		// Token: 0x060027F8 RID: 10232 RVA: 0x001D163C File Offset: 0x001CF83C
		public void sendPrivateMessage(string[] command)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			int matchingIndex = 0;
			Farmer matchingFarmer = this.findMatchingFarmer(command, ref matchingIndex, false, true);
			if (matchingFarmer == null)
			{
				this.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchOnlinePlayer"));
				return;
			}
			string message = "";
			for (int i = matchingIndex + 1; i < command.Length; i++)
			{
				message += command[i];
				if (i < command.Length - 1)
				{
					message += " ";
				}
			}
			message = Program.sdk.FilterDirtyWords(message);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, matchingFarmer.UniqueMultiplayerID);
			this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message);
		}

		// Token: 0x060027F9 RID: 10233 RVA: 0x001D16E5 File Offset: 0x001CF8E5
		public bool isActive()
		{
			return this.chatBox.Selected;
		}

		// Token: 0x060027FA RID: 10234 RVA: 0x001D16F2 File Offset: 0x001CF8F2
		public void activate()
		{
			this.chatBox.Selected = true;
			this.setText("");
		}

		// Token: 0x060027FB RID: 10235 RVA: 0x001D170C File Offset: 0x001CF90C
		public override void clickAway()
		{
			base.clickAway();
			if (!this.choosingEmoji || !this.emojiMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Escape))
			{
				bool selected = this.chatBox.Selected;
				this.chatBox.Selected = false;
				this.choosingEmoji = false;
				this.setText("");
				this.cheatHistoryPosition = -1;
				if (selected)
				{
					Game1.oldKBState = Game1.GetKeyboardState();
				}
			}
		}

		// Token: 0x060027FC RID: 10236 RVA: 0x001D1790 File Offset: 0x001CF990
		public override bool isWithinBounds(int x, int y)
		{
			return (x - this.xPositionOnScreen < this.width && x - this.xPositionOnScreen >= 0 && y - this.yPositionOnScreen < this.height && y - this.yPositionOnScreen >= -this.getOldMessagesBoxHeight()) || (this.choosingEmoji && this.emojiMenu.isWithinBounds(x, y));
		}

		// Token: 0x060027FD RID: 10237 RVA: 0x001D17F2 File Offset: 0x001CF9F2
		public virtual void setText(string text)
		{
			this.chatBox.setText(text);
		}

		// Token: 0x060027FE RID: 10238 RVA: 0x001D1800 File Offset: 0x001CFA00
		public override void receiveKeyPress(Keys key)
		{
			if (key != Keys.Up)
			{
				if (key == Keys.Down)
				{
					if (this.cheatHistoryPosition > 0)
					{
						this.cheatHistoryPosition--;
						string cheat = this.cheatHistory[this.cheatHistoryPosition];
						this.chatBox.setText(cheat);
					}
				}
			}
			else if (this.cheatHistoryPosition < this.cheatHistory.Count - 1)
			{
				this.cheatHistoryPosition++;
				string cheat2 = this.cheatHistory[this.cheatHistoryPosition];
				this.chatBox.setText(cheat2);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key) || Game1.options.doesInputListContain(Game1.options.moveRightButton, key) || Game1.options.doesInputListContain(Game1.options.moveDownButton, key) || Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				return;
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x060027FF RID: 10239 RVA: 0x001D18F5 File Offset: 0x001CFAF5
		public override bool readyToClose()
		{
			return false;
		}

		// Token: 0x06002800 RID: 10240 RVA: 0x001D18F8 File Offset: 0x001CFAF8
		public override void receiveGamePadButton(Buttons button)
		{
		}

		// Token: 0x06002801 RID: 10241 RVA: 0x001D18FA File Offset: 0x001CFAFA
		public bool isHoveringOverClickable(int x, int y)
		{
			return this.emojiMenuIcon.containsPoint(x, y) || (this.choosingEmoji && this.emojiMenu.isWithinBounds(x, y));
		}

		// Token: 0x06002802 RID: 10242 RVA: 0x001D1928 File Offset: 0x001CFB28
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.chatBox.Selected)
			{
				return;
			}
			if (this.emojiMenuIcon.containsPoint(x, y))
			{
				this.choosingEmoji = !this.choosingEmoji;
				Game1.playSound("shwip", null);
				this.emojiMenuIcon.scale = 4f;
				return;
			}
			if (this.choosingEmoji && this.emojiMenu.isWithinBounds(x, y))
			{
				this.emojiMenu.leftClick(x, y, this);
				return;
			}
			this.chatBox.Update();
			if (this.choosingEmoji)
			{
				this.choosingEmoji = false;
				this.emojiMenuIcon.scale = 4f;
			}
			if (this.isWithinBounds(x, y))
			{
				this.chatBox.Selected = true;
			}
		}

		// Token: 0x06002803 RID: 10243 RVA: 0x001D19EC File Offset: 0x001CFBEC
		public static string formattedUserName(Farmer farmer)
		{
			string name = farmer.Name;
			if (string.IsNullOrWhiteSpace(name))
			{
				name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
			}
			return Program.sdk.FilterDirtyWords(name);
		}

		// Token: 0x06002804 RID: 10244 RVA: 0x001D1A24 File Offset: 0x001CFC24
		public static string formattedUserNameLong(Farmer farmer)
		{
			string name = ChatBox.formattedUserName(farmer);
			string userName = Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID);
			if (string.IsNullOrWhiteSpace(userName))
			{
				return name;
			}
			return Game1.content.LoadString("Strings\\UI:Chat_PlayerName", name, userName);
		}

		// Token: 0x06002805 RID: 10245 RVA: 0x001D1A64 File Offset: 0x001CFC64
		public string formatMessage(long sourceFarmer, int chatKind, string message)
		{
			string userName = Game1.content.LoadString("Strings\\UI:Chat_UnknownUserName");
			Farmer farmer;
			if (sourceFarmer == Game1.player.UniqueMultiplayerID)
			{
				farmer = Game1.player;
			}
			else if (!Game1.otherFarmers.TryGetValue(sourceFarmer, out farmer))
			{
				farmer = null;
			}
			if (farmer != null)
			{
				userName = ChatBox.formattedUserName(farmer);
			}
			switch (chatKind)
			{
			case 0:
				return Game1.content.LoadString("Strings\\UI:Chat_ChatMessageFormat", userName, message);
			case 2:
				return Game1.content.LoadString("Strings\\UI:Chat_UserNotificationMessageFormat", message);
			case 3:
				return Game1.content.LoadString("Strings\\UI:Chat_PrivateMessageFormat", userName, message);
			}
			return Game1.content.LoadString("Strings\\UI:Chat_ErrorMessageFormat", message);
		}

		// Token: 0x06002806 RID: 10246 RVA: 0x001D1B0E File Offset: 0x001CFD0E
		public virtual Color messageColor(int chatKind)
		{
			switch (chatKind)
			{
			case 0:
				return this.chatBox.TextColor;
			case 2:
				return Color.Yellow;
			case 3:
				return Color.DarkCyan;
			}
			return Color.Red;
		}

		// Token: 0x06002807 RID: 10247 RVA: 0x001D1B48 File Offset: 0x001CFD48
		public virtual void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
		{
			string text = this.formatMessage(sourceFarmer, chatKind, message);
			ChatMessage c = new ChatMessage();
			string s = Game1.parseText(text, this.chatBox.Font, this.chatBox.Width - 16);
			c.timeLeftToDisplay = 600;
			c.verticalSize = (int)this.chatBox.Font.MeasureString(s).Y + 4;
			c.color = this.messageColor(chatKind);
			c.language = language;
			c.parseMessageForEmoji(s);
			this.messages.Add(c);
			if (this.messages.Count > this.maxMessages)
			{
				this.messages.RemoveAt(0);
			}
			if (chatKind == 3 && sourceFarmer != Game1.player.UniqueMultiplayerID)
			{
				this.lastReceivedPrivateMessagePlayerId = sourceFarmer;
			}
		}

		// Token: 0x06002808 RID: 10248 RVA: 0x001D1C0C File Offset: 0x001CFE0C
		public virtual void addMessage(string message, Color color)
		{
			ChatMessage c = new ChatMessage();
			string s = Game1.parseText(message, this.chatBox.Font, this.chatBox.Width - 8);
			c.timeLeftToDisplay = 600;
			c.verticalSize = (int)this.chatBox.Font.MeasureString(s).Y + 4;
			c.color = color;
			c.language = LocalizedContentManager.CurrentLanguageCode;
			c.parseMessageForEmoji(s);
			this.messages.Add(c);
			if (this.messages.Count > this.maxMessages)
			{
				this.messages.RemoveAt(0);
			}
		}

		// Token: 0x06002809 RID: 10249 RVA: 0x001D1CAC File Offset: 0x001CFEAC
		public void addNiceTryEasterEggMessage()
		{
			this.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NiceTry"), new Color(104, 214, 255));
		}

		// Token: 0x0600280A RID: 10250 RVA: 0x001D1CD4 File Offset: 0x001CFED4
		public override void performHoverAction(int x, int y)
		{
			this.emojiMenuIcon.tryHover(x, y, 1f);
			this.emojiMenuIcon.tryHover(x, y, 1f);
		}

		// Token: 0x0600280B RID: 10251 RVA: 0x001D1CFC File Offset: 0x001CFEFC
		public override void update(GameTime time)
		{
			KeyboardState keyState = Game1.input.GetKeyboardState();
			foreach (Keys key in keyState.GetPressedKeys())
			{
				if (!this.oldKBState.IsKeyDown(key))
				{
					this.receiveKeyPress(key);
				}
			}
			this.oldKBState = keyState;
			for (int i = 0; i < this.messages.Count; i++)
			{
				if (this.messages[i].timeLeftToDisplay > 0)
				{
					this.messages[i].timeLeftToDisplay--;
				}
				if (this.messages[i].timeLeftToDisplay < 75)
				{
					this.messages[i].alpha = (float)this.messages[i].timeLeftToDisplay / 75f;
				}
			}
			if (this.chatBox.Selected)
			{
				foreach (ChatMessage chatMessage in this.messages)
				{
					chatMessage.alpha = 1f;
				}
			}
			this.emojiMenuIcon.tryHover(0, 0, 1f);
		}

		// Token: 0x0600280C RID: 10252 RVA: 0x001D1E40 File Offset: 0x001D0040
		public override void receiveScrollWheelAction(int direction)
		{
			if (this.choosingEmoji)
			{
				this.emojiMenu.receiveScrollWheelAction(direction);
			}
		}

		// Token: 0x0600280D RID: 10253 RVA: 0x001D1E56 File Offset: 0x001D0056
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.updatePosition();
		}

		// Token: 0x0600280E RID: 10254 RVA: 0x001D1E5E File Offset: 0x001D005E
		public static SpriteFont messageFont(LocalizedContentManager.LanguageCode language)
		{
			return Game1.content.Load<SpriteFont>("Fonts\\SmallFont", language);
		}

		// Token: 0x0600280F RID: 10255 RVA: 0x001D1E70 File Offset: 0x001D0070
		public int getOldMessagesBoxHeight()
		{
			int heightSoFar = 20;
			for (int i = this.messages.Count - 1; i >= 0; i--)
			{
				ChatMessage message = this.messages[i];
				if (this.chatBox.Selected || message.alpha > 0.01f)
				{
					heightSoFar += message.verticalSize;
				}
			}
			return heightSoFar;
		}

		// Token: 0x06002810 RID: 10256 RVA: 0x001D1ECC File Offset: 0x001D00CC
		public override void draw(SpriteBatch b)
		{
			int heightSoFar = 0;
			bool drawBG = false;
			for (int i = this.messages.Count - 1; i >= 0; i--)
			{
				ChatMessage message = this.messages[i];
				if (this.chatBox.Selected || message.alpha > 0.01f)
				{
					heightSoFar += message.verticalSize;
					drawBG = true;
				}
			}
			if (drawBG)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), this.xPositionOnScreen, this.yPositionOnScreen - heightSoFar - 20 + (this.chatBox.Selected ? 0 : this.chatBox.Height), this.chatBox.Width, heightSoFar + 20, Color.White, 4f, false, -1f);
			}
			heightSoFar = 0;
			for (int j = this.messages.Count - 1; j >= 0; j--)
			{
				ChatMessage message2 = this.messages[j];
				heightSoFar += message2.verticalSize;
				message2.draw(b, this.xPositionOnScreen + 12, this.yPositionOnScreen - heightSoFar - 8 + (this.chatBox.Selected ? 0 : this.chatBox.Height));
			}
			if (this.chatBox.Selected)
			{
				this.chatBox.Draw(b, false);
				this.emojiMenuIcon.draw(b, Color.White, 0.99f, 0, 0, 0);
				if (this.choosingEmoji)
				{
					this.emojiMenu.draw(b);
				}
				if (this.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && !Game1.options.hardwareCursor)
				{
					Game1.mouseCursor = (Game1.options.gamepadControls ? Game1.cursor_gamepad_pointer : Game1.cursor_default);
				}
			}
		}

		// Token: 0x0400199F RID: 6559
		public const int chatMessage = 0;

		// Token: 0x040019A0 RID: 6560
		public const int errorMessage = 1;

		// Token: 0x040019A1 RID: 6561
		public const int userNotificationMessage = 2;

		// Token: 0x040019A2 RID: 6562
		public const int privateMessage = 3;

		// Token: 0x040019A3 RID: 6563
		public const int defaultMaxMessages = 10;

		// Token: 0x040019A4 RID: 6564
		public const int timeToDisplayMessages = 600;

		// Token: 0x040019A5 RID: 6565
		public const int chatboxWidth = 896;

		// Token: 0x040019A6 RID: 6566
		public const int chatboxHeight = 56;

		// Token: 0x040019A7 RID: 6567
		public const int region_chatBox = 101;

		// Token: 0x040019A8 RID: 6568
		public const int region_emojiButton = 102;

		// Token: 0x040019A9 RID: 6569
		public ChatTextBox chatBox;

		// Token: 0x040019AA RID: 6570
		public ClickableComponent chatBoxCC;

		// Token: 0x040019AB RID: 6571
		private readonly IGameLogger CheatCommandChatLogger;

		// Token: 0x040019AC RID: 6572
		public List<ChatMessage> messages = new List<ChatMessage>();

		// Token: 0x040019AD RID: 6573
		private KeyboardState oldKBState;

		// Token: 0x040019AE RID: 6574
		private List<string> cheatHistory = new List<string>();

		// Token: 0x040019AF RID: 6575
		private int cheatHistoryPosition = -1;

		// Token: 0x040019B0 RID: 6576
		public int maxMessages = 10;

		// Token: 0x040019B1 RID: 6577
		public static Texture2D emojiTexture;

		// Token: 0x040019B2 RID: 6578
		public ClickableTextureComponent emojiMenuIcon;

		// Token: 0x040019B3 RID: 6579
		public EmojiMenu emojiMenu;

		// Token: 0x040019B4 RID: 6580
		public bool choosingEmoji;

		// Token: 0x040019B5 RID: 6581
		private long lastReceivedPrivateMessagePlayerId;
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Characters;

namespace StardewValley.Menus
{
	// Token: 0x02000266 RID: 614
	public class DialogueBox : IClickableMenu
	{
		// Token: 0x060028BD RID: 10429 RVA: 0x001DCED4 File Offset: 0x001DB0D4
		public DialogueBox(int x, int y, int width, int height)
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		// Token: 0x060028BE RID: 10430 RVA: 0x001DCF80 File Offset: 0x001DB180
		public DialogueBox(string dialogue)
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			this.dialogues.AddRange(dialogue.Split('#', StringSplitOptions.None));
			this.width = Math.Min(1240, SpriteText.getWidthOfString(this.dialogues[0], 999999) + 64);
			this.height = SpriteText.getHeightOfString(this.dialogues[0], this.width - 20) + 4;
			this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
			this.y = Game1.uiViewport.Height - this.height - 64;
			this.setUpIcons();
		}

		// Token: 0x060028BF RID: 10431 RVA: 0x001DD0AC File Offset: 0x001DB2AC
		public DialogueBox(string dialogue, Response[] responses, int width = 1200)
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			this.dialogues.Add(dialogue);
			this.responses = responses;
			this.isQuestion = true;
			this.width = width;
			this.setUpQuestions();
			this.height = this.heightForQuestions;
			this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, this.height, 0, 0).X;
			this.y = Game1.uiViewport.Height - this.height - 64;
			this.setUpIcons();
			this.characterIndexInDialogue = dialogue.Length;
			if (responses != null)
			{
				foreach (Response response in responses)
				{
					response.responseText = Dialogue.applyGenderSwitch(Game1.player.Gender, response.responseText, true);
				}
			}
		}

		// Token: 0x060028C0 RID: 10432 RVA: 0x001DD1E8 File Offset: 0x001DB3E8
		public DialogueBox(Dialogue dialogue)
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			this.characterDialogue = dialogue;
			this.width = 1200;
			this.height = 384;
			this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
			this.y = Game1.uiViewport.Height - this.height - 64;
			this.friendshipJewel = new Rectangle(this.x + this.width - 64, this.y + 256, 44, 44);
			dialogue.prepareDialogueForDisplay();
			this.characterDialogue.prepareCurrentDialogueForDisplay();
			if (!this.characterDialogue.isDialogueFinished())
			{
				this.characterDialoguesBrokenUp.Push(dialogue.getCurrentDialogue());
				this.checkDialogue(dialogue);
			}
			else
			{
				this.dialogueFinished = true;
			}
			this.newPortaitShakeTimer = ((this.characterDialogue.getPortraitIndex() == 1) ? 250 : 0);
			this.setUpForGamePadMode();
		}

		// Token: 0x060028C1 RID: 10433 RVA: 0x001DD358 File Offset: 0x001DB558
		public DialogueBox(List<string> dialogues)
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			this.dialogues = dialogues;
			this.width = Math.Min(1200, SpriteText.getWidthOfString(dialogues[0], 999999) + 64);
			this.height = SpriteText.getHeightOfString(dialogues[0], this.width - 16);
			this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
			this.y = Game1.uiViewport.Height - this.height - 64;
			this.setUpIcons();
		}

		// Token: 0x060028C2 RID: 10434 RVA: 0x001DD46B File Offset: 0x001DB66B
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060028C3 RID: 10435 RVA: 0x001DD480 File Offset: 0x001DB680
		private void playOpeningSound()
		{
			Game1.playSound("breathin", null);
		}

		// Token: 0x060028C4 RID: 10436 RVA: 0x001DD4A4 File Offset: 0x001DB6A4
		public void closeDialogue()
		{
			if (Game1.activeClickableMenu.Equals(this))
			{
				Game1.exitActiveMenu();
				Game1.dialogueUp = false;
				Dialogue dialogue = this.characterDialogue;
				if (((dialogue != null) ? dialogue.speaker : null) != null && this.characterDialogue.speaker.CurrentDialogue.Count > 0 && this.dialogueFinished && this.characterDialogue.speaker.CurrentDialogue.Count > 0)
				{
					this.characterDialogue.speaker.CurrentDialogue.Pop();
				}
				if (Game1.messagePause)
				{
					Game1.pauseTime = 500f;
				}
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.currentObjectDialogue.Dequeue();
				}
				Game1.currentDialogueCharacterIndex = 0;
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.dialogueUp = true;
					Game1.questionChoices.Clear();
					Game1.dialogueTyping = true;
				}
				Dialogue dialogue2 = this.characterDialogue;
				if (((dialogue2 != null) ? dialogue2.speaker : null) != null && !this.characterDialogue.speaker.Name.Equals("Gunther") && !Game1.eventUp && !this.characterDialogue.speaker.doingEndOfRouteAnimation.Value)
				{
					this.characterDialogue.speaker.doneFacingPlayer(Game1.player);
				}
				Game1.currentSpeaker = null;
				if (!Game1.eventUp)
				{
					if (!Game1.isWarping)
					{
						Game1.player.CanMove = true;
					}
					Game1.player.movementDirections.Clear();
				}
				else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
				{
					if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
					{
						Event currentEvent = Game1.currentLocation.currentEvent;
						int currentCommand = currentEvent.CurrentCommand;
						currentEvent.CurrentCommand = currentCommand + 1;
					}
					else
					{
						Game1.player.CanMove = true;
					}
				}
				Game1.questionChoices.Clear();
			}
			if (Game1.afterDialogues != null)
			{
				Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
				Game1.afterDialogues = null;
				afterDialogues();
			}
		}

		// Token: 0x060028C5 RID: 10437 RVA: 0x001DD695 File Offset: 0x001DB895
		public void finishTyping()
		{
			this.characterIndexInDialogue = this.getCurrentString().Length;
		}

		// Token: 0x060028C6 RID: 10438 RVA: 0x001DD6A8 File Offset: 0x001DB8A8
		public void beginOutro()
		{
			this.transitioning = true;
			this.transitioningBigger = false;
			Game1.playSound("breathout", null);
		}

		// Token: 0x060028C7 RID: 10439 RVA: 0x001DD6D7 File Offset: 0x001DB8D7
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			this.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x060028C8 RID: 10440 RVA: 0x001DD6E2 File Offset: 0x001DB8E2
		private void tryOutro()
		{
			if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.Equals(this))
			{
				this.beginOutro();
			}
		}

		// Token: 0x060028C9 RID: 10441 RVA: 0x001DD700 File Offset: 0x001DB900
		public override void receiveKeyPress(Keys key)
		{
			if (this.transitioning)
			{
				return;
			}
			if (Game1.options.SnappyMenus && !this.isQuestion && Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				this.receiveLeftClick(0, 0, true);
				return;
			}
			if (!Game1.options.gamepadControls && Game1.options.doesInputListContain(Game1.options.actionButton, key))
			{
				this.receiveLeftClick(0, 0, true);
				return;
			}
			if (this.isQuestion && !Game1.eventUp && this.characterDialogue == null)
			{
				if (this.responses != null)
				{
					foreach (Response response in this.responses)
					{
						if (response.hotkey == key && Game1.currentLocation.answerDialogue(response))
						{
							Game1.playSound("smallSelect", null);
							this.selectedResponse = -1;
							this.tryOutro();
							return;
						}
					}
					if (key == Keys.N)
					{
						foreach (Response response2 in this.responses)
						{
							if (response2.hotkey == Keys.Escape && Game1.currentLocation.answerDialogue(response2))
							{
								Game1.playSound("smallSelect", null);
								this.selectedResponse = -1;
								this.tryOutro();
								return;
							}
						}
					}
				}
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) || key == Keys.N)
				{
					Response[] array2 = this.responses;
					if (array2 != null && array2.Length != 0 && Game1.currentLocation.answerDialogue(this.responses[this.responses.Length - 1]))
					{
						Game1.playSound("smallSelect", null);
					}
					this.selectedResponse = -1;
					this.tryOutro();
					return;
				}
				if (Game1.options.SnappyMenus)
				{
					this.safetyTimer = 0;
					base.receiveKeyPress(key);
					return;
				}
				if (key == Keys.Y)
				{
					Response[] array3 = this.responses;
					if (array3 != null && array3.Length != 0 && this.responses[0].responseKey.Equals("Yes") && Game1.currentLocation.answerDialogue(this.responses[0]))
					{
						Game1.playSound("smallSelect", null);
						this.selectedResponse = -1;
						this.tryOutro();
						return;
					}
				}
			}
			else if (Game1.options.SnappyMenus && this.isQuestion && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x060028CA RID: 10442 RVA: 0x001DD978 File Offset: 0x001DBB78
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.transitioning)
			{
				if (this.characterIndexInDialogue < this.getCurrentString().Length - 1)
				{
					this.characterIndexInDialogue = this.getCurrentString().Length - 1;
					return;
				}
				if (this.safetyTimer > 0)
				{
					return;
				}
				if (this.isQuestion)
				{
					if (this.selectedResponse == -1)
					{
						return;
					}
					this.questionFinishPauseTimer = (Game1.eventUp ? 600 : 200);
					this.transitioning = true;
					this.transitionInitialized = false;
					this.transitioningBigger = true;
					if (this.characterDialogue != null)
					{
						this.characterDialoguesBrokenUp.Pop();
						this.characterDialogue.chooseResponse(this.responses[this.selectedResponse]);
						this.characterDialoguesBrokenUp.Push("");
						Game1.playSound("smallSelect", null);
					}
					else
					{
						Game1.dialogueUp = false;
						if (Game1.eventUp && Game1.currentLocation.afterQuestion == null)
						{
							Game1.playSound("smallSelect", null);
							Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, this.selectedResponse);
							this.selectedResponse = -1;
							this.tryOutro();
							return;
						}
						if (Game1.currentLocation.answerDialogue(this.responses[this.selectedResponse]))
						{
							Game1.playSound("smallSelect", null);
						}
						this.selectedResponse = -1;
						this.tryOutro();
						return;
					}
				}
				else if (this.characterDialogue == null)
				{
					this.dialogues.RemoveAt(0);
					if (this.dialogues.Count == 0)
					{
						this.closeDialogue();
					}
					else
					{
						this.width = Math.Min(1200, SpriteText.getWidthOfString(this.dialogues[0], 999999) + 64);
						this.height = SpriteText.getHeightOfString(this.dialogues[0], this.width - 16);
						this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
						this.y = Game1.uiViewport.Height - this.height - 64;
						this.xPositionOnScreen = x;
						this.yPositionOnScreen = y;
						this.setUpIcons();
					}
				}
				this.characterIndexInDialogue = 0;
				if (this.characterDialogue != null)
				{
					int oldPortrait = this.characterDialogue.getPortraitIndex();
					if (this.characterDialoguesBrokenUp.Count == 0)
					{
						this.beginOutro();
						return;
					}
					this.characterDialoguesBrokenUp.Pop();
					if (this.characterDialoguesBrokenUp.Count == 0)
					{
						if (!this.characterDialogue.isCurrentStringContinuedOnNextScreen)
						{
							this.beginOutro();
						}
						this.characterDialogue.exitCurrentDialogue();
					}
					if (!this.characterDialogue.isDialogueFinished() && this.characterDialogue.getCurrentDialogue().Length > 0 && this.characterDialoguesBrokenUp.Count == 0)
					{
						this.characterDialogue.prepareCurrentDialogueForDisplay();
						if (this.characterDialogue.isDialogueFinished())
						{
							this.beginOutro();
							return;
						}
						this.characterDialoguesBrokenUp.Push(this.characterDialogue.getCurrentDialogue());
					}
					this.checkDialogue(this.characterDialogue);
					if (this.characterDialogue.getPortraitIndex() != oldPortrait)
					{
						this.newPortaitShakeTimer = ((this.characterDialogue.getPortraitIndex() == 1) ? 250 : 50);
					}
				}
				if (!this.transitioning)
				{
					Game1.playSound("smallSelect", null);
				}
				this.setUpIcons();
				this.safetyTimer = (Game1.IsDedicatedHost ? 0 : 750);
				if (this.getCurrentString() != null && this.getCurrentString().Length <= 20)
				{
					this.safetyTimer -= 200;
				}
			}
		}

		// Token: 0x060028CB RID: 10443 RVA: 0x001DDD1C File Offset: 0x001DBF1C
		private void setUpIcons()
		{
			this.dialogueIcon = null;
			if (this.isQuestion)
			{
				this.setUpQuestionIcon();
			}
			else if (this.characterDialogue != null && (this.characterDialogue.isCurrentStringContinuedOnNextScreen || this.characterDialoguesBrokenUp.Count > 1))
			{
				this.setUpNextPageIcon();
			}
			else
			{
				List<string> list = this.dialogues;
				if (list != null && list.Count > 1)
				{
					this.setUpNextPageIcon();
				}
				else
				{
					this.setUpCloseDialogueIcon();
				}
			}
			this.setUpForGamePadMode();
			if (this.getCurrentString() != null && this.getCurrentString().Length <= 20)
			{
				this.safetyTimer -= 200;
			}
		}

		// Token: 0x060028CC RID: 10444 RVA: 0x001DDDC0 File Offset: 0x001DBFC0
		public override void performHoverAction(int mouseX, int mouseY)
		{
			this.hoverText = "";
			if (!this.transitioning && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
			{
				base.performHoverAction(mouseX, mouseY);
				if (this.isQuestion)
				{
					int oldResponse = this.selectedResponse;
					this.selectedResponse = -1;
					if (Game1.options.gamepadControls && this.currentlySnappedComponent != null)
					{
						this.selectedResponse = this.currentlySnappedComponent.myID;
					}
					int responseY = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width - 16) + 48;
					int margin = 8;
					int i = 0;
					while (i < this.responses.Length)
					{
						if (mouseY >= responseY - margin && mouseY < responseY + SpriteText.getHeightOfString(this.responses[i].responseText, this.width - 16) + margin)
						{
							this.selectedResponse = i;
							int num = i;
							List<ClickableComponent> list = this.responseCC;
							int? num2 = (list != null) ? new int?(list.Count) : null;
							if (num < num2.GetValueOrDefault() & num2 != null)
							{
								this.currentlySnappedComponent = this.responseCC[i];
								break;
							}
							break;
						}
						else
						{
							responseY += SpriteText.getHeightOfString(this.responses[i].responseText, this.width - 16) + 16;
							i++;
						}
					}
					if (this.selectedResponse != oldResponse)
					{
						Game1.playSound("Cowboy_gunshot", null);
					}
				}
			}
			if (this.shouldDrawFriendshipJewel() && this.friendshipJewel.Contains(mouseX, mouseY))
			{
				this.hoverText = Game1.player.getFriendshipHeartLevelForNPC(this.characterDialogue.speaker.Name).ToString() + "/" + Utility.GetMaximumHeartsForCharacter(this.characterDialogue.speaker).ToString() + "<";
			}
			if (Game1.options.SnappyMenus && this.currentlySnappedComponent != null)
			{
				this.selectedResponse = this.currentlySnappedComponent.myID;
			}
		}

		// Token: 0x060028CD RID: 10445 RVA: 0x001DDFD0 File Offset: 0x001DC1D0
		public bool shouldDrawFriendshipJewel()
		{
			if (this.width >= 642 && !Game1.eventUp && !this.isQuestion && this.isPortraitBox() && !this.friendshipJewel.Equals(Rectangle.Empty))
			{
				Dialogue dialogue = this.characterDialogue;
				if (((dialogue != null) ? dialogue.speaker : null) != null && Game1.player.friendshipData.ContainsKey(this.characterDialogue.speaker.Name) && this.characterDialogue.speaker.Name != "Henchman")
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060028CE RID: 10446 RVA: 0x001DE067 File Offset: 0x001DC267
		private void setUpQuestionIcon()
		{
		}

		// Token: 0x060028CF RID: 10447 RVA: 0x001DE06C File Offset: 0x001DC26C
		private void setUpCloseDialogueIcon()
		{
			Vector2 iconPosition = new Vector2((float)(this.x + this.width - 40), (float)(this.y + this.height - 44));
			if (this.isPortraitBox())
			{
				iconPosition.X -= 492f;
			}
			this.dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(289, 342, 11, 12), 80f, 11, 999999, iconPosition, false, false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, true);
		}

		// Token: 0x060028D0 RID: 10448 RVA: 0x001DE110 File Offset: 0x001DC310
		private void setUpNextPageIcon()
		{
			Vector2 iconPosition = new Vector2((float)(this.x + this.width - 40), (float)(this.y + this.height - 40));
			if (this.isPortraitBox())
			{
				iconPosition.X -= 492f;
			}
			this.dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(232, 346, 9, 9), 90f, 6, 999999, iconPosition, false, false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
			{
				yPeriodic = true,
				yPeriodicLoopTime = 1500f,
				yPeriodicRange = 8f
			};
		}

		// Token: 0x060028D1 RID: 10449 RVA: 0x001DE1D0 File Offset: 0x001DC3D0
		private void checkDialogue(Dialogue d)
		{
			this.isQuestion = false;
			string sub = "";
			if (this.characterDialoguesBrokenUp.Count == 1)
			{
				sub = SpriteText.getSubstringBeyondHeight(this.characterDialoguesBrokenUp.Peek(), this.width - 460 - 20, this.height - 16);
			}
			if (sub.Length > 0)
			{
				string full = this.characterDialoguesBrokenUp.Pop().Replace(Environment.NewLine, "");
				this.characterDialoguesBrokenUp.Push(sub.Trim());
				this.characterDialoguesBrokenUp.Push(full.Substring(0, full.Length - sub.Length + 1).Trim());
			}
			if (d.getCurrentDialogue().Length == 0)
			{
				this.dialogueFinished = true;
			}
			if (d.isCurrentStringContinuedOnNextScreen || this.characterDialoguesBrokenUp.Count > 1)
			{
				this.dialogueContinuedOnNextPage = true;
			}
			else if (d.getCurrentDialogue().Length == 0)
			{
				this.beginOutro();
			}
			if (d.isCurrentDialogueAQuestion())
			{
				this.responses = d.getResponseOptions();
				this.isQuestion = true;
				this.setUpQuestions();
			}
		}

		// Token: 0x060028D2 RID: 10450 RVA: 0x001DE2E4 File Offset: 0x001DC4E4
		private void setUpQuestions()
		{
			int tmpwidth = this.width - 16;
			this.heightForQuestions = SpriteText.getHeightOfString(this.getCurrentString(), tmpwidth);
			foreach (Response r in this.responses)
			{
				this.heightForQuestions += SpriteText.getHeightOfString(r.responseText, tmpwidth) + 16;
			}
			this.heightForQuestions += 40;
		}

		// Token: 0x060028D3 RID: 10451 RVA: 0x001DE351 File Offset: 0x001DC551
		public bool isPortraitBox()
		{
			Dialogue dialogue = this.characterDialogue;
			bool flag;
			if (dialogue == null)
			{
				flag = (null != null);
			}
			else
			{
				NPC speaker = dialogue.speaker;
				flag = (((speaker != null) ? speaker.Portrait : null) != null);
			}
			return flag && this.characterDialogue.showPortrait && Game1.options.showPortraits;
		}

		// Token: 0x060028D4 RID: 10452 RVA: 0x001DE38C File Offset: 0x001DC58C
		public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
		{
			if (this.transitionInitialized)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle?(new Rectangle(306, 320, 16, 16)), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle?(new Rectangle(275, 328, 1, 8)), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle?(new Rectangle(264, 325, 8, 1)), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle?(new Rectangle(293, 324, 7, 1)), Color.White);
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 44), (float)(yPos - 28)), new Rectangle?(new Rectangle(261, 311, 14, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - 8), (float)(yPos - 28)), new Rectangle?(new Rectangle(291, 311, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - 8), (float)(yPos + boxHeight - 8)), new Rectangle?(new Rectangle(291, 326, 12, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 44), (float)(yPos + boxHeight - 4)), new Rectangle?(new Rectangle(261, 327, 14, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			}
		}

		// Token: 0x060028D5 RID: 10453 RVA: 0x001DE5EC File Offset: 0x001DC7EC
		private bool shouldPortraitShake(Dialogue d)
		{
			if (this.newPortaitShakeTimer > 0)
			{
				return true;
			}
			CharacterData data = d.speaker.GetData();
			List<int> shakePortraits = (data != null) ? data.ShakePortraits : null;
			return shakePortraits != null && shakePortraits.Count > 0 && shakePortraits.Contains(d.getPortraitIndex());
		}

		// Token: 0x060028D6 RID: 10454 RVA: 0x001DE638 File Offset: 0x001DC838
		public void drawPortrait(SpriteBatch b)
		{
			NPC speaker = this.characterDialogue.speaker;
			if (!Game1.IsMasterGame && !speaker.EventActor)
			{
				GameLocation currentLocation = speaker.currentLocation;
				if (currentLocation == null || !currentLocation.IsActiveLocation())
				{
					NPC actualSpeaker = Game1.getCharacterFromName(speaker.Name, true, false);
					if (actualSpeaker != null && actualSpeaker.currentLocation.IsActiveLocation())
					{
						speaker = actualSpeaker;
					}
				}
			}
			if (this.width >= 642)
			{
				int xPositionOfPortraitArea = this.x + this.width - 448 + 4;
				int widthOfPortraitArea = this.x + this.width - xPositionOfPortraitArea;
				b.Draw(Game1.mouseCursors, new Rectangle(xPositionOfPortraitArea - 40, this.y, 36, this.height), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPositionOfPortraitArea - 40), (float)(this.y - 20)), new Rectangle?(new Rectangle(278, 313, 10, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPositionOfPortraitArea - 40), (float)(this.y + this.height)), new Rectangle?(new Rectangle(278, 328, 10, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				int portraitBoxX = xPositionOfPortraitArea + 76;
				int portraitBoxY = this.y + this.height / 2 - 148 - 36;
				b.Draw(Game1.mouseCursors, new Vector2((float)(xPositionOfPortraitArea - 8), (float)this.y), new Rectangle?(new Rectangle(583, 411, 115, 97)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				Texture2D portraitTexture = this.characterDialogue.overridePortrait ?? speaker.Portrait;
				Rectangle portraitSource = Game1.getSourceRectForStandardTileSheet(portraitTexture, this.characterDialogue.getPortraitIndex(), 64, 64);
				if (!portraitTexture.Bounds.Contains(portraitSource))
				{
					portraitSource = new Rectangle(0, 0, 64, 64);
				}
				int xOffset = this.shouldPortraitShake(this.characterDialogue) ? Game1.random.Next(-1, 2) : 0;
				b.Draw(portraitTexture, new Vector2((float)(portraitBoxX + 16 + xOffset), (float)(portraitBoxY + 24)), new Rectangle?(portraitSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				SpriteText.drawStringHorizontallyCenteredAt(b, speaker.getName(), xPositionOfPortraitArea + widthOfPortraitArea / 2, portraitBoxY + 296 + 16, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
				if (this.shouldDrawFriendshipJewel())
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)this.friendshipJewel.X, (float)this.friendshipJewel.Y), new Rectangle?((Game1.player.getFriendshipHeartLevelForNPC(speaker.Name) >= 10) ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(speaker.Name) / 2 * 11), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
			}
		}

		// Token: 0x060028D7 RID: 10455 RVA: 0x001DE9EC File Offset: 0x001DCBEC
		public string getCurrentString()
		{
			if (this.characterDialogue != null)
			{
				string s;
				if (this.characterDialoguesBrokenUp.Count > 0)
				{
					s = this.characterDialoguesBrokenUp.Peek().Trim().Replace(Environment.NewLine, "");
				}
				else
				{
					s = this.characterDialogue.getCurrentDialogue().Trim().Replace(Environment.NewLine, "");
				}
				if (!Game1.options.showPortraits)
				{
					s = this.characterDialogue.speaker.getName() + ": " + s;
				}
				return s;
			}
			if (this.dialogues.Count > 0)
			{
				return this.dialogues[0].Trim().Replace(Environment.NewLine, "");
			}
			return "";
		}

		// Token: 0x060028D8 RID: 10456 RVA: 0x001DEAB0 File Offset: 0x001DCCB0
		public override void update(GameTime time)
		{
			base.update(time);
			if (Game1.options.SnappyMenus && !Game1.lastCursorMotionWasMouse)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			else
			{
				Game1.mouseCursorTransparency = 1f;
			}
			if (this.isQuestion && this.characterIndexInDialogue >= this.getCurrentString().Length - 1 && !this.transitioning)
			{
				Game1.mouseCursorTransparency = 1f;
				if (!this._showedOptions)
				{
					this._showedOptions = true;
					if (this.responses != null)
					{
						this.responseCC = new List<ClickableComponent>();
						int responseY = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width) + 48;
						for (int i = 0; i < this.responses.Length; i++)
						{
							this.responseCC.Add(new ClickableComponent(new Rectangle(this.x + 8, responseY, this.width - 8, SpriteText.getHeightOfString(this.responses[i].responseText, this.width) + 16), "")
							{
								myID = i,
								downNeighborID = ((i < this.responses.Length - 1) ? (i + 1) : -1),
								upNeighborID = ((i > 0) ? (i - 1) : -1)
							});
							responseY += SpriteText.getHeightOfString(this.responses[i].responseText, this.width) + 16;
						}
					}
					this.populateClickableComponentList();
					if (Game1.options.gamepadControls)
					{
						this.snapToDefaultClickableComponent();
						this.selectedResponse = this.currentlySnappedComponent.myID;
					}
				}
			}
			if (this.safetyTimer > 0)
			{
				this.safetyTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (!Game1.IsDedicatedHost && this.questionFinishPauseTimer > 0)
			{
				this.questionFinishPauseTimer -= time.ElapsedGameTime.Milliseconds;
				return;
			}
			if (this.transitioning)
			{
				if (!this.transitionInitialized)
				{
					this.transitionInitialized = true;
					this.transitionX = this.x + this.width / 2;
					this.transitionY = this.y + this.height / 2;
					this.transitionWidth = 0;
					this.transitionHeight = 0;
				}
				if (this.transitioningBigger)
				{
					bool flag = this.transitionWidth != 0;
					this.transitionX -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
					this.transitionY -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(this.isQuestion ? this.heightForQuestions : this.height) / (float)this.width));
					this.transitionX = Math.Max(this.x, this.transitionX);
					this.transitionY = Math.Max(this.isQuestion ? (this.y + this.height - this.heightForQuestions) : this.y, this.transitionY);
					this.transitionWidth += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
					this.transitionHeight += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(this.isQuestion ? this.heightForQuestions : this.height) / (float)this.width) * 2f);
					this.transitionWidth = Math.Min(this.width, this.transitionWidth);
					this.transitionHeight = Math.Min(this.isQuestion ? this.heightForQuestions : this.height, this.transitionHeight);
					if (!flag && this.transitionWidth > 0)
					{
						this.playOpeningSound();
					}
					if (Game1.IsDedicatedHost || (this.transitionX == this.x && this.transitionY == (this.isQuestion ? (this.y + this.height - this.heightForQuestions) : this.y)))
					{
						this.transitioning = false;
						this.characterAdvanceTimer = 90;
						this.setUpIcons();
						this.transitionX = this.x;
						this.transitionY = this.y;
						this.transitionWidth = this.width;
						this.transitionHeight = this.height;
					}
				}
				else
				{
					this.transitionX += (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
					this.transitionY += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)this.height / (float)this.width));
					this.transitionX = Math.Min(this.x + this.width / 2, this.transitionX);
					this.transitionY = Math.Min(this.y + this.height / 2, this.transitionY);
					this.transitionWidth -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
					this.transitionHeight -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)this.height / (float)this.width) * 2f);
					this.transitionWidth = Math.Max(0, this.transitionWidth);
					this.transitionHeight = Math.Max(0, this.transitionHeight);
					if (Game1.IsDedicatedHost || (this.transitionWidth == 0 && this.transitionHeight == 0))
					{
						this.closeDialogue();
					}
				}
			}
			if (!this.transitioning && !this.showTyping && this.characterIndexInDialogue < this.getCurrentString().Length)
			{
				this.finishTyping();
			}
			if (!this.transitioning && this.characterIndexInDialogue < this.getCurrentString().Length)
			{
				this.characterAdvanceTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.characterAdvanceTimer <= 0 || Game1.IsDedicatedHost)
				{
					this.characterAdvanceTimer = 30;
					int old = this.characterIndexInDialogue;
					this.characterIndexInDialogue = Math.Min(this.characterIndexInDialogue + 1, this.getCurrentString().Length);
					if (this.characterIndexInDialogue != old && this.characterIndexInDialogue == this.getCurrentString().Length)
					{
						Game1.playSound("dialogueCharacterClose", null);
					}
					if (this.characterIndexInDialogue > 1 && this.characterIndexInDialogue < this.getCurrentString().Length && Game1.options.dialogueTyping)
					{
						Game1.playSound("dialogueCharacter", null);
					}
				}
			}
			if (!this.transitioning && this.dialogueIcon != null)
			{
				this.dialogueIcon.update(time);
			}
			if (!this.transitioning && this.newPortaitShakeTimer > 0)
			{
				this.newPortaitShakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		// Token: 0x060028D9 RID: 10457 RVA: 0x001DF19C File Offset: 0x001DD39C
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.width = 1200;
			this.height = 384;
			this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
			this.y = Game1.uiViewport.Height - this.height - 64;
			this.friendshipJewel = new Rectangle(this.x + this.width - 64, this.y + 256, 44, 44);
			this.setUpIcons();
		}

		// Token: 0x060028DA RID: 10458 RVA: 0x001DF22C File Offset: 0x001DD42C
		public override void draw(SpriteBatch b)
		{
			if (this.width < 16 || this.height < 16)
			{
				return;
			}
			if (this.transitioning)
			{
				this.drawBox(b, this.transitionX, this.transitionY, this.transitionWidth, this.transitionHeight);
				base.drawMouse(b, false, -1);
				return;
			}
			if (this.isQuestion)
			{
				this.drawBox(b, this.x, this.y - (this.heightForQuestions - this.height), this.width, this.heightForQuestions);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(this.x + this.width - 72), (float)(this.y - (this.heightForQuestions - this.height) - 88)), new Rectangle?(new Rectangle(495, 461, 17, 19)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(this.x + this.width - 52), (float)(this.y - (this.heightForQuestions - this.height) - 88 + 16)), new Rectangle?(new Rectangle(470 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 900 / 150 * 7, 447, 7, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 12 - (this.heightForQuestions - this.height), this.characterIndexInDialogue, this.width - 16, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				if (this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
				{
					int responseY = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width - 16) + 48;
					for (int i = 0; i < this.responses.Length; i++)
					{
						if (i == this.selectedResponse)
						{
							IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.x + 4, responseY - 8, this.width - 8, SpriteText.getHeightOfString(this.responses[i].responseText, this.width) + 16, Color.White, 4f, false, -1f);
						}
						SpriteText.drawString(b, this.responses[i].responseText, this.x + 8, responseY, 999999, this.width, 999999, (this.selectedResponse == i) ? 1f : 0.6f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
						responseY += SpriteText.getHeightOfString(this.responses[i].responseText, this.width) + 16;
					}
				}
			}
			else
			{
				this.drawBox(b, this.x, this.y, this.width, this.height);
				if (!this.isPortraitBox() && !this.isQuestion)
				{
					SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 8, this.characterIndexInDialogue, this.width, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
			}
			if (this.isPortraitBox() && !this.isQuestion)
			{
				this.drawPortrait(b);
				if (!this.isQuestion)
				{
					SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 8, this.characterIndexInDialogue, this.width - 460 - 24, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
			}
			if (this.dialogueIcon != null && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
			{
				this.dialogueIcon.draw(b, true, 0, 0, 1f);
			}
			if (this.aboveDialogueImage != null)
			{
				this.drawBox(b, this.x + this.width / 2 - (int)((float)(this.aboveDialogueImage.sourceRect.Width / 2) * this.aboveDialogueImage.scale), this.y - 64 - 4 - (int)((float)this.aboveDialogueImage.sourceRect.Height * this.aboveDialogueImage.scale), (int)((float)this.aboveDialogueImage.sourceRect.Width * this.aboveDialogueImage.scale), (int)((float)this.aboveDialogueImage.sourceRect.Height * this.aboveDialogueImage.scale) + 8);
				Utility.drawWithShadow(b, this.aboveDialogueImage.texture, new Vector2((float)(this.x + this.width / 2) - (float)(this.aboveDialogueImage.sourceRect.Width / 2) * this.aboveDialogueImage.scale, (float)(this.y - 64 - (int)((float)this.aboveDialogueImage.sourceRect.Height * this.aboveDialogueImage.scale))), this.aboveDialogueImage.sourceRect, Color.White, 0f, Vector2.Zero, this.aboveDialogueImage.scale, false, 1f, -1, -1, 0.35f);
			}
			if (this.hoverText.Length > 0)
			{
				SpriteText.drawStringWithScrollBackground(b, this.hoverText, this.friendshipJewel.Center.X - SpriteText.getWidthOfString(this.hoverText, 999999) / 2, this.friendshipJewel.Y - 64, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001A82 RID: 6786
		public List<string> dialogues = new List<string>();

		// Token: 0x04001A83 RID: 6787
		public Dialogue characterDialogue;

		// Token: 0x04001A84 RID: 6788
		public Stack<string> characterDialoguesBrokenUp = new Stack<string>();

		// Token: 0x04001A85 RID: 6789
		public Response[] responses = LegacyShims.EmptyArray<Response>();

		// Token: 0x04001A86 RID: 6790
		public const int portraitBoxSize = 74;

		// Token: 0x04001A87 RID: 6791
		public const int nameTagWidth = 102;

		// Token: 0x04001A88 RID: 6792
		public const int nameTagHeight = 18;

		// Token: 0x04001A89 RID: 6793
		public const int portraitPlateWidth = 115;

		// Token: 0x04001A8A RID: 6794
		public const int nameTagSideMargin = 5;

		// Token: 0x04001A8B RID: 6795
		public const float transitionRate = 3f;

		// Token: 0x04001A8C RID: 6796
		public const int characterAdvanceDelay = 30;

		// Token: 0x04001A8D RID: 6797
		public const int safetyDelay = 750;

		// Token: 0x04001A8E RID: 6798
		public int questionFinishPauseTimer;

		// Token: 0x04001A8F RID: 6799
		protected bool _showedOptions;

		// Token: 0x04001A90 RID: 6800
		public Rectangle friendshipJewel = Rectangle.Empty;

		// Token: 0x04001A91 RID: 6801
		public List<ClickableComponent> responseCC;

		// Token: 0x04001A92 RID: 6802
		public int x;

		// Token: 0x04001A93 RID: 6803
		public int y;

		// Token: 0x04001A94 RID: 6804
		public int transitionX = -1;

		// Token: 0x04001A95 RID: 6805
		public int transitionY;

		// Token: 0x04001A96 RID: 6806
		public int transitionWidth;

		// Token: 0x04001A97 RID: 6807
		public int transitionHeight;

		// Token: 0x04001A98 RID: 6808
		public int characterAdvanceTimer;

		// Token: 0x04001A99 RID: 6809
		public int characterIndexInDialogue;

		// Token: 0x04001A9A RID: 6810
		public int safetyTimer = 750;

		// Token: 0x04001A9B RID: 6811
		public int heightForQuestions;

		// Token: 0x04001A9C RID: 6812
		public int selectedResponse = -1;

		// Token: 0x04001A9D RID: 6813
		public int newPortaitShakeTimer;

		// Token: 0x04001A9E RID: 6814
		public bool transitionInitialized;

		// Token: 0x04001A9F RID: 6815
		public bool showTyping = true;

		// Token: 0x04001AA0 RID: 6816
		public bool transitioning = true;

		// Token: 0x04001AA1 RID: 6817
		public bool transitioningBigger = true;

		// Token: 0x04001AA2 RID: 6818
		public bool dialogueContinuedOnNextPage;

		// Token: 0x04001AA3 RID: 6819
		public bool dialogueFinished;

		// Token: 0x04001AA4 RID: 6820
		public bool isQuestion;

		// Token: 0x04001AA5 RID: 6821
		public TemporaryAnimatedSprite dialogueIcon;

		// Token: 0x04001AA6 RID: 6822
		public TemporaryAnimatedSprite aboveDialogueImage;

		// Token: 0x04001AA7 RID: 6823
		private string hoverText = "";
	}
}

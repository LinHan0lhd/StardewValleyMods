using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus
{
	// Token: 0x02000271 RID: 625
	public class FieldOfficeMenu : MenuWithInventory
	{
		// Token: 0x0600293C RID: 10556 RVA: 0x001E4640 File Offset: 0x001E2840
		public FieldOfficeMenu(IslandFieldOffice office) : base(new InventoryMenu.highlightThisItem(FieldOfficeMenu.highlightBones), true, true, 16, 132, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			FieldOfficeMenu <>4__this = this;
			this.office = office;
			this.fieldOfficeMenuTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\FieldOfficeDonationMenu");
			Point topLeft = new Point(this.xPositionOnScreen + 32, this.yPositionOnScreen + 96);
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 76, topLeft.Y + 180, 64, 64), office.piecesDonated[0] ? ItemRegistry.Create("(O)823", 1, 0, false) : null)
			{
				label = "823"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 144, topLeft.Y + 180, 64, 64), office.piecesDonated[1] ? ItemRegistry.Create("(O)824", 1, 0, false) : null)
			{
				label = "824"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 212, topLeft.Y + 180, 64, 64), office.piecesDonated[2] ? ItemRegistry.Create("(O)823", 1, 0, false) : null)
			{
				label = "823"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 76, topLeft.Y + 112, 64, 64), office.piecesDonated[3] ? ItemRegistry.Create("(O)822", 1, 0, false) : null)
			{
				label = "822"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 144, topLeft.Y + 112, 64, 64), office.piecesDonated[4] ? ItemRegistry.Create("(O)821", 1, 0, false) : null)
			{
				label = "821"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 212, topLeft.Y + 112, 64, 64), office.piecesDonated[5] ? ItemRegistry.Create("(O)820", 1, 0, false) : null)
			{
				label = "820"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 412, topLeft.Y + 48, 64, 64), office.piecesDonated[6] ? ItemRegistry.Create("(O)826", 1, 0, false) : null)
			{
				label = "826"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 412, topLeft.Y + 128, 64, 64), office.piecesDonated[7] ? ItemRegistry.Create("(O)826", 1, 0, false) : null)
			{
				label = "826"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 412, topLeft.Y + 208, 64, 64), office.piecesDonated[8] ? ItemRegistry.Create("(O)825", 1, 0, false) : null)
			{
				label = "825"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 616, topLeft.Y + 36, 64, 64), office.piecesDonated[9] ? ItemRegistry.Create("(O)827", 1, 0, false) : null)
			{
				label = "827"
			});
			this.pieceHolders.Add(new ClickableComponent(new Rectangle(topLeft.X + 624, topLeft.Y + 156, 64, 64), office.piecesDonated[10] ? ItemRegistry.Create("(O)828", 1, 0, false) : null)
			{
				label = "828"
			});
			if (Game1.activeClickableMenu == null)
			{
				Game1.playSound("bigSelect", null);
			}
			for (int i = 0; i < this.pieceHolders.Count; i++)
			{
				ClickableComponent clickableComponent = this.pieceHolders[i];
				clickableComponent.upNeighborID = (clickableComponent.downNeighborID = (clickableComponent.rightNeighborID = (clickableComponent.leftNeighborID = -99998)));
				clickableComponent.myID = 1000 + i;
			}
			foreach (ClickableComponent clickableComponent2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
			{
				clickableComponent2.upNeighborID = -99998;
			}
			foreach (ClickableComponent clickableComponent3 in this.inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				clickableComponent3.rightNeighborID = 4857;
				clickableComponent3.rightNeighborImmutable = true;
			}
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
			this.trashCan.leftNeighborID = (this.okButton.leftNeighborID = 11);
			this.exitFunction = delegate()
			{
				if (<>4__this.madeADonation)
				{
					string baseKey = "Strings\\Locations:FieldOfficeDonated_" + Game1.random.Next(4).ToString();
					string text = Game1.content.LoadString(baseKey);
					if (<>4__this.gotReward)
					{
						text = text + "#$b#" + Game1.content.LoadString("Strings\\Locations:FieldOfficeDonated_Reward");
					}
					Game1.DrawDialogue(new Dialogue(office.getSafariGuy(), baseKey, text));
					if (<>4__this.gotReward)
					{
						Game1.multiplayer.globalChatInfoMessage("FieldOfficeCompleteSet", new string[]
						{
							Game1.player.Name
						});
					}
				}
			};
		}

		// Token: 0x0600293D RID: 10557 RVA: 0x001E4C24 File Offset: 0x001E2E24
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (b.myID != 5948 || b.myID == 4857) && base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x0600293E RID: 10558 RVA: 0x001E4C4B File Offset: 0x001E2E4B
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x0600293F RID: 10559 RVA: 0x001E4C60 File Offset: 0x001E2E60
		public static bool highlightBones(Item i)
		{
			if (i != null)
			{
				IslandFieldOffice office = Game1.RequireLocation<IslandFieldOffice>("IslandFieldOffice", false);
				string qualifiedItemId = i.QualifiedItemId;
				if (qualifiedItemId != null)
				{
					int length = qualifiedItemId.Length;
					if (length == 6)
					{
						switch (qualifiedItemId[5])
						{
						case '0':
							if (qualifiedItemId == "(O)820")
							{
								if (!office.piecesDonated[5])
								{
									return true;
								}
							}
							break;
						case '1':
							if (qualifiedItemId == "(O)821")
							{
								if (!office.piecesDonated[4])
								{
									return true;
								}
							}
							break;
						case '2':
							if (qualifiedItemId == "(O)822")
							{
								if (!office.piecesDonated[3])
								{
									return true;
								}
							}
							break;
						case '3':
							if (qualifiedItemId == "(O)823")
							{
								if (!office.piecesDonated[0] || !office.piecesDonated[2])
								{
									return true;
								}
							}
							break;
						case '4':
							if (qualifiedItemId == "(O)824")
							{
								if (!office.piecesDonated[1])
								{
									return true;
								}
							}
							break;
						case '5':
							if (qualifiedItemId == "(O)825")
							{
								if (!office.piecesDonated[8])
								{
									return true;
								}
							}
							break;
						case '6':
							if (qualifiedItemId == "(O)826")
							{
								if (!office.piecesDonated[7] || !office.piecesDonated[6])
								{
									return true;
								}
							}
							break;
						case '7':
							if (qualifiedItemId == "(O)827")
							{
								if (!office.piecesDonated[9])
								{
									return true;
								}
							}
							break;
						case '8':
							if (qualifiedItemId == "(O)828")
							{
								if (!office.piecesDonated[10])
								{
									return true;
								}
							}
							break;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002940 RID: 10560 RVA: 0x001E4E4C File Offset: 0x001E304C
		public static int getPieceIndexForDonationItem(string qualifiedItemId)
		{
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				if (length == 6)
				{
					switch (qualifiedItemId[5])
					{
					case '0':
						if (qualifiedItemId == "(O)820")
						{
							return 5;
						}
						break;
					case '1':
						if (qualifiedItemId == "(O)821")
						{
							return 4;
						}
						break;
					case '2':
						if (qualifiedItemId == "(O)822")
						{
							return 3;
						}
						break;
					case '3':
						if (qualifiedItemId == "(O)823")
						{
							return 0;
						}
						break;
					case '4':
						if (qualifiedItemId == "(O)824")
						{
							return 1;
						}
						break;
					case '5':
						if (qualifiedItemId == "(O)825")
						{
							return 8;
						}
						break;
					case '6':
						if (qualifiedItemId == "(O)826")
						{
							return 7;
						}
						break;
					case '7':
						if (qualifiedItemId == "(O)827")
						{
							return 9;
						}
						break;
					case '8':
						if (qualifiedItemId == "(O)828")
						{
							return 10;
						}
						break;
					}
				}
			}
			return -1;
		}

		// Token: 0x06002941 RID: 10561 RVA: 0x001E4F48 File Offset: 0x001E3148
		public static int getDonationPieceIndexNeededForSpot(int donationSpotIndex)
		{
			switch (donationSpotIndex)
			{
			case 0:
			case 2:
				return 823;
			case 1:
				return 824;
			case 3:
				return 822;
			case 4:
				return 821;
			case 5:
				return 820;
			case 6:
			case 7:
				return 826;
			case 8:
				return 825;
			case 9:
				return 827;
			case 10:
				return 828;
			default:
				return -1;
			}
		}

		// Token: 0x06002942 RID: 10562 RVA: 0x001E4FC0 File Offset: 0x001E31C0
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (base.heldItem != null)
			{
				int index = FieldOfficeMenu.getPieceIndexForDonationItem(base.heldItem.QualifiedItemId);
				if (index != -1)
				{
					string qualifiedItemId = base.heldItem.QualifiedItemId;
					if (!(qualifiedItemId == "(O)823"))
					{
						if (!(qualifiedItemId == "(O)826"))
						{
							this.donate(index, x, y);
						}
						else if (!this.donate(7, x, y))
						{
							this.donate(6, x, y);
							return;
						}
					}
					else if (!this.donate(0, x, y))
					{
						this.donate(2, x, y);
						return;
					}
				}
			}
		}

		// Token: 0x06002943 RID: 10563 RVA: 0x001E5054 File Offset: 0x001E3254
		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			if (this.office != null && this.office.isRangeAllTrue(0, 11) && this.office.plantsRestoredRight.Value && this.office.plantsRestoredLeft.Value && !Game1.player.hasOrWillReceiveMail("fieldOfficeFinale"))
			{
				this.office.triggerFinaleCutscene();
			}
		}

		// Token: 0x06002944 RID: 10564 RVA: 0x001E50C0 File Offset: 0x001E32C0
		private bool donate(int index, int x, int y)
		{
			if (this.pieceHolders[index].containsPoint(x, y) && this.pieceHolders[index].item == null)
			{
				Item item = base.heldItem;
				base.heldItem = item.ConsumeStack(1);
				this.pieceHolders[index].item = ItemRegistry.Create(item.QualifiedItemId, 1, 0, false);
				this.checkForSetFinish();
				this.gotReward = this.office.donatePiece(index);
				this.madeADonation = true;
				Game1.playSound("newArtifact", null);
				Game1.multiplayer.globalChatInfoMessage("FieldOfficeDonation", new string[]
				{
					Game1.player.Name,
					TokenStringBuilder.ItemNameFor(item, null)
				});
				return true;
			}
			return false;
		}

		// Token: 0x06002945 RID: 10565 RVA: 0x001E5190 File Offset: 0x001E3390
		public void checkForSetFinish()
		{
			if (!this.office.centerSkeletonRestored.Value && this.pieceHolders[0].item != null && this.pieceHolders[1].item != null && this.pieceHolders[2].item != null && this.pieceHolders[3].item != null && this.pieceHolders[4].item != null && this.pieceHolders[5].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					this.bearTimer = 500f;
					Game1.playSound("camel", null);
				}, 700);
			}
			if (!this.office.snakeRestored.Value && this.pieceHolders[6].item != null && this.pieceHolders[7].item != null && this.pieceHolders[8].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					this.snakeTimer = 1500f;
					Game1.playSound("steam", null);
				}, 700);
			}
			if (!this.office.batRestored.Value && this.pieceHolders[9].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					this.batTimer = 1500f;
					Game1.playSound("batScreech", null);
				}, 700);
			}
			if (!this.office.frogRestored.Value && this.pieceHolders[10].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					this.frogTimer = 1000f;
					Game1.playSound("croak", null);
				}, 700);
			}
		}

		// Token: 0x06002946 RID: 10566 RVA: 0x001E5318 File Offset: 0x001E3518
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.bearTimer > 0f)
			{
				this.bearTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.snakeTimer > 0f)
			{
				this.snakeTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.batTimer > 0f)
			{
				this.batTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.frogTimer > 0f)
			{
				this.frogTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x06002947 RID: 10567 RVA: 0x001E53D0 File Offset: 0x001E35D0
		public override void draw(SpriteBatch b)
		{
			base.draw(b, true, false, 0, 80, 80);
			b.Draw(this.fieldOfficeMenuTexture, new Vector2((float)(this.xPositionOnScreen + 32), (float)(this.yPositionOnScreen + 96)), new Rectangle?(new Rectangle(0, 0, 204, 80)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			b.Draw(this.fieldOfficeMenuTexture, new Vector2((float)(this.xPositionOnScreen + this.width - 160), (float)(this.yPositionOnScreen + 108) + ((this.batTimer > 0f) ? ((float)Math.Sin((double)((1500f - this.batTimer) / 80f)) * 64f / 4f) : 0f)), new Rectangle?(new Rectangle(68, 84, 30, 20)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			foreach (ClickableComponent c in this.pieceHolders)
			{
				Item item = c.item;
				if (item != null)
				{
					item.drawInMenu(b, Utility.PointToVector2(c.bounds.Location), 1f);
				}
			}
			if (this.bearTimer > 0f)
			{
				b.Draw(this.fieldOfficeMenuTexture, new Vector2((float)(this.xPositionOnScreen + 32 + 240), (float)(this.yPositionOnScreen + 96 + 36)), new Rectangle?(new Rectangle(0, 81, 37, 29)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
			else if (this.snakeTimer > 0f && this.snakeTimer / 300f % 2f != 0f)
			{
				b.Draw(this.fieldOfficeMenuTexture, new Vector2((float)(this.xPositionOnScreen + 32 + 484), (float)(this.yPositionOnScreen + 96 + 232)), new Rectangle?(new Rectangle(47, 84, 19, 19)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
			else if (this.frogTimer > 0f)
			{
				b.Draw(this.fieldOfficeMenuTexture, new Vector2((float)(this.xPositionOnScreen + 32 + 708), (float)(this.yPositionOnScreen + 96 + 140)), new Rectangle?(new Rectangle(100, 89, 18, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
			if (base.heldItem != null)
			{
				int highlight = FieldOfficeMenu.getPieceIndexForDonationItem(base.heldItem.QualifiedItemId);
				if (highlight != -1)
				{
					this.drawHighlightedSquare(highlight, b);
				}
			}
			base.drawMouse(b, false, -1);
			Item heldItem = base.heldItem;
			if (heldItem == null)
			{
				return;
			}
			heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
		}

		// Token: 0x06002948 RID: 10568 RVA: 0x001E56EC File Offset: 0x001E38EC
		private void drawHighlightedSquare(int index, SpriteBatch b)
		{
			Rectangle source = default(Rectangle);
			string qualifiedItemId = base.heldItem.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				if (length == 6)
				{
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(O)820"))
						{
							goto IL_146;
						}
						break;
					case '1':
						if (!(qualifiedItemId == "(O)821"))
						{
							goto IL_146;
						}
						break;
					case '2':
						if (!(qualifiedItemId == "(O)822"))
						{
							goto IL_146;
						}
						break;
					case '3':
						if (!(qualifiedItemId == "(O)823"))
						{
							goto IL_146;
						}
						break;
					case '4':
						if (!(qualifiedItemId == "(O)824"))
						{
							goto IL_146;
						}
						break;
					case '5':
						if (!(qualifiedItemId == "(O)825"))
						{
							goto IL_146;
						}
						goto IL_10C;
					case '6':
						if (!(qualifiedItemId == "(O)826"))
						{
							goto IL_146;
						}
						goto IL_10C;
					case '7':
						if (!(qualifiedItemId == "(O)827"))
						{
							goto IL_146;
						}
						source = new Rectangle(157, 86, 18, 18);
						goto IL_146;
					case '8':
						if (!(qualifiedItemId == "(O)828"))
						{
							goto IL_146;
						}
						source = new Rectangle(176, 86, 18, 18);
						goto IL_146;
					default:
						goto IL_146;
					}
					source = new Rectangle(119, 86, 18, 18);
					goto IL_146;
					IL_10C:
					source = new Rectangle(138, 86, 18, 18);
				}
			}
			IL_146:
			if (this.pieceHolders[index].item == null)
			{
				b.Draw(this.fieldOfficeMenuTexture, Utility.PointToVector2(this.pieceHolders[index].bounds.Location) + new Vector2(-1f, -1f) * 4f, new Rectangle?(source), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
			qualifiedItemId = base.heldItem.QualifiedItemId;
			if (!(qualifiedItemId == "(O)823"))
			{
				if (!(qualifiedItemId == "(O)826"))
				{
					return;
				}
				if (index == 7 && this.pieceHolders[6].item == null)
				{
					b.Draw(this.fieldOfficeMenuTexture, Utility.PointToVector2(this.pieceHolders[6].bounds.Location) + new Vector2(-1f, -1f) * 4f, new Rectangle?(source), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				}
			}
			else if (index == 0 && this.pieceHolders[2].item == null)
			{
				b.Draw(this.fieldOfficeMenuTexture, Utility.PointToVector2(this.pieceHolders[2].bounds.Location) + new Vector2(-1f, -1f) * 4f, new Rectangle?(source), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				return;
			}
		}

		// Token: 0x04001AED RID: 6893
		private Texture2D fieldOfficeMenuTexture;

		// Token: 0x04001AEE RID: 6894
		private IslandFieldOffice office;

		// Token: 0x04001AEF RID: 6895
		private bool madeADonation;

		// Token: 0x04001AF0 RID: 6896
		private bool gotReward;

		// Token: 0x04001AF1 RID: 6897
		public List<ClickableComponent> pieceHolders = new List<ClickableComponent>();

		// Token: 0x04001AF2 RID: 6898
		private float bearTimer;

		// Token: 0x04001AF3 RID: 6899
		private float snakeTimer;

		// Token: 0x04001AF4 RID: 6900
		private float batTimer;

		// Token: 0x04001AF5 RID: 6901
		private float frogTimer;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E5 RID: 741
	public class ManorHouse : GameLocation
	{
		// Token: 0x06003116 RID: 12566 RVA: 0x0026B965 File Offset: 0x00269B65
		public ManorHouse()
		{
		}

		// Token: 0x06003117 RID: 12567 RVA: 0x0026B978 File Offset: 0x00269B78
		public ManorHouse(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06003118 RID: 12568 RVA: 0x0026B990 File Offset: 0x00269B90
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (who.IsLocalPlayer)
			{
				string a = ArgUtility.Get(action, 0, null, true);
				if (!(a == "LostAndFound"))
				{
					if (!(a == "MayorFridge"))
					{
						if (!(a == "DivorceBook"))
						{
							if (a == "LedgerBook")
							{
								this.readLedgerBook();
							}
						}
						else if (Game1.player.divorceTonight.Value)
						{
							string s = null;
							if (Game1.player.hasCurrentOrPendingRoommate())
							{
								s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion_Krobus", Game1.player.getSpouse().displayName);
							}
							if (s == null)
							{
								s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion", true);
							}
							base.createQuestionDialogue(s, base.createYesNoResponses(), "divorceCancel");
						}
						else if (Game1.player.isMarriedOrRoommates())
						{
							string s2 = null;
							if (Game1.player.hasCurrentOrPendingRoommate())
							{
								s2 = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Question_Krobus", Game1.player.getSpouse().displayName);
							}
							if (s2 == null)
							{
								s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question", true);
							}
							base.createQuestionDialogue(s2, base.createYesNoResponses(), "divorce");
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_NoSpouse"));
						}
					}
					else if (who.Items.ContainsId("(O)284", 10) && !who.hasOrWillReceiveMail("TH_MayorFridge") && who.hasOrWillReceiveMail("TH_Railroad"))
					{
						who.Items.ReduceId("(O)284", 10);
						Game1.player.CanMove = false;
						base.localSound("coin", null, null, SoundContext.Default);
						Game1.player.mailReceived.Add("TH_MayorFridge");
						Game1.multipleDialogues(new string[]
						{
							Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_ConsumeBeets"),
							Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_MrQiNote")
						});
						Game1.player.removeQuest("3");
						Game1.player.addQuest("4");
					}
					else if (who.hasOrWillReceiveMail("TH_MayorFridge"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_MrQiNote"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_Initial"));
					}
				}
				else
				{
					this.CheckLostAndFound();
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06003119 RID: 12569 RVA: 0x0026BC0C File Offset: 0x00269E0C
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (Game1.eventUp && Game1.CurrentEvent.id != "prizeTicketIntro")
			{
				base.removeTile(4, 5, "Buildings");
				base.removeTile(4, 4, "Front");
				base.removeTile(4, 3, "Front");
				base.setMapTile(4, 6, 635, "Back", "1", null, true);
				return;
			}
			base.setMapTile(4, 5, 109, "Buildings", "untitled tile sheet2", "LostAndFound", true);
			base.setMapTile(4, 4, 77, "Front", "untitled tile sheet2", null, true);
			base.setMapTile(4, 3, 110, "Front", "untitled tile sheet2", null, true);
			base.setMapTile(4, 6, 604, "Back", "1", null, true);
		}

		// Token: 0x0600311A RID: 12570 RVA: 0x0026BCE4 File Offset: 0x00269EE4
		public void CheckLostAndFound()
		{
			string prompt = SpecialOrder.IsSpecialOrdersBoardUnlocked() ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check_OrdersUnlocked") : Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check");
			List<Response> choices = new List<Response>();
			if (Game1.player.team.returnedDonations.Count > 0 && !Game1.player.team.returnedDonationsMutex.IsLocked())
			{
				choices.Add(new Response("CheckDonations", Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_DonationItems")));
			}
			if (this.GetRetrievableFarmers().Count > 0)
			{
				choices.Add(new Response("RetrieveFarmhandItems", Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItems")));
			}
			if (choices.Count > 0)
			{
				choices.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
			}
			if (choices.Count > 0)
			{
				base.createQuestionDialogue(prompt, choices.ToArray(), "lostAndFound");
				return;
			}
			Game1.drawObjectDialogue(prompt);
		}

		// Token: 0x0600311B RID: 12571 RVA: 0x0026BDE4 File Offset: 0x00269FE4
		public List<Farmer> GetRetrievableFarmers()
		{
			List<Farmer> offline_farmers = new List<Farmer>(Game1.getAllFarmers());
			foreach (Farmer online_farmer in Game1.getOnlineFarmers())
			{
				offline_farmers.Remove(online_farmer);
			}
			for (int i = 0; i < offline_farmers.Count; i++)
			{
				Farmer farmer = offline_farmers[i];
				Cabin home = Utility.getHomeOfFarmer(farmer) as Cabin;
				if (home != null && (farmer.isUnclaimedFarmhand || home.inventoryMutex.IsLocked()))
				{
					offline_farmers.RemoveAt(i);
					i--;
				}
			}
			return offline_farmers;
		}

		// Token: 0x0600311C RID: 12572 RVA: 0x0026BE94 File Offset: 0x0026A094
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (Game1.player.team.returnedDonations.Count > 0 && !Game1.eventUp)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
				Vector2 lost_and_found_indicator_position = new Vector2(4f, 4f) * 64f + new Vector2(7f, 0f) * 4f;
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(lost_and_found_indicator_position.X, lost_and_found_indicator_position.Y + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10)), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x0600311D RID: 12573 RVA: 0x0026BF90 File Offset: 0x0026A190
		private void readLedgerBook()
		{
			if (Game1.player.useSeparateWallets)
			{
				if (Game1.IsMasterGame)
				{
					List<Response> choices = new List<Response>();
					choices.Add(new Response("SendMoney", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SendMoney")));
					if (Game1.player.changeWalletTypeTonight.Value)
					{
						choices.Add(new Response("CancelMerge", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_CancelMerge")));
					}
					else
					{
						choices.Add(new Response("MergeWallets", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_MergeWallets")));
					}
					choices.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Leave")));
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HostQuestion"), choices.ToArray(), "ledgerOptions");
					return;
				}
				this.ChooseRecipient();
				return;
			}
			else
			{
				if (!Game1.getAllFarmhands().Any<Farmer>())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Singleplayer"));
					return;
				}
				if (!Game1.IsMasterGame)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_Client"));
					return;
				}
				if (Game1.player.changeWalletTypeTonight.Value)
				{
					string s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_CancelQuestion");
					base.createQuestionDialogue(s, base.createYesNoResponses(), "cancelSeparateWallets");
					return;
				}
				string s2 = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_SeparateQuestion");
				base.createQuestionDialogue(s2, base.createYesNoResponses(), "separateWallets");
				return;
			}
		}

		// Token: 0x0600311E RID: 12574 RVA: 0x0026C108 File Offset: 0x0026A308
		public void ShowOfflineFarmhandItemList()
		{
			List<Response> choices = new List<Response>();
			foreach (Farmer farmer in this.GetRetrievableFarmers())
			{
				string key = farmer.UniqueMultiplayerID.ToString() ?? "";
				string name = farmer.Name;
				if (farmer.Name == "")
				{
					name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
				}
				choices.Add(new Response(key, name));
			}
			choices.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItemsQuestion"), choices.ToArray(), "CheckItems");
		}

		// Token: 0x0600311F RID: 12575 RVA: 0x0026C1EC File Offset: 0x0026A3EC
		public void ChooseRecipient()
		{
			this.sendMoneyMapping.Clear();
			List<Response> otherFarmers = new List<Response>();
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID && !farmer.isUnclaimedFarmhand)
				{
					string key = "Transfer" + (otherFarmers.Count + 1).ToString();
					string farmerName = farmer.Name;
					if (farmer.Name == "")
					{
						farmerName = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
					}
					otherFarmers.Add(new Response(key, farmerName));
					this.sendMoneyMapping.Add(key, farmer);
				}
			}
			if (otherFarmers.Count == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_NoFarmhands"));
				return;
			}
			otherFarmers.Sort((Response x, Response y) => string.Compare(x.responseKey, y.responseKey));
			otherFarmers.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_TransferQuestion"), otherFarmers.ToArray(), "chooseRecipient");
		}

		// Token: 0x06003120 RID: 12576 RVA: 0x0026C34C File Offset: 0x0026A54C
		private void beginSendMoney(Farmer recipient)
		{
			Game1.activeClickableMenu = new DigitEntryMenu(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HowMuchQuestion"), delegate(int currentValue, int price, Farmer who)
			{
				this.sendMoney(recipient, currentValue);
			}, -1, 1, Game1.player.Money, 0);
		}

		// Token: 0x06003121 RID: 12577 RVA: 0x0026C3A0 File Offset: 0x0026A5A0
		public void sendMoney(Farmer recipient, int amount)
		{
			Game1.playSound("smallSelect", null);
			Game1.player.Money -= amount;
			Game1.player.team.AddIndividualMoney(recipient, amount);
			Game1.player.stats.onMoneyGifted((uint)amount);
			if (amount == 1)
			{
				Game1.multiplayer.globalChatInfoMessage("Sent1g", new string[]
				{
					Game1.player.Name,
					recipient.Name
				});
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("SentMoney", new string[]
				{
					Game1.player.Name,
					recipient.Name,
					TokenStringBuilder.NumberWithSeparators(amount)
				});
			}
			Game1.exitActiveMenu();
		}

		// Token: 0x06003122 RID: 12578 RVA: 0x0026C460 File Offset: 0x0026A660
		public static void SeparateWallets()
		{
			if (Game1.player.useSeparateWallets || !Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.changeWalletTypeTonight.Value = false;
			int totalMoney = Game1.player.Money;
			int farmerCount = 0;
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.isUnclaimedFarmhand)
					{
						farmerCount++;
					}
				}
			}
			int splitMoney = totalMoney / Math.Max(farmerCount, 1);
			Game1.player.team.useSeparateWallets.Value = true;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (!farmer.isUnclaimedFarmhand)
				{
					Game1.player.team.SetIndividualMoney(farmer, splitMoney);
				}
			}
			Game1.multiplayer.globalChatInfoMessage("SeparatedWallets", new string[]
			{
				Game1.player.Name,
				splitMoney.ToString()
			});
		}

		// Token: 0x06003123 RID: 12579 RVA: 0x0026C57C File Offset: 0x0026A77C
		public static void MergeWallets()
		{
			if (!Game1.player.useSeparateWallets || !Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.changeWalletTypeTonight.Value = false;
			int totalMoney = 0;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (!farmer.isUnclaimedFarmhand)
				{
					totalMoney += Game1.player.team.GetIndividualMoney(farmer);
				}
			}
			Game1.player.team.useSeparateWallets.Value = false;
			Game1.player.team.money.Value = totalMoney;
			Game1.multiplayer.globalChatInfoMessage("MergedWallets", new string[]
			{
				Game1.player.Name
			});
		}

		// Token: 0x06003124 RID: 12580 RVA: 0x0026C650 File Offset: 0x0026A850
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			string s = null;
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer != null)
			{
				int length = questionAndAnswer.Length;
				switch (length)
				{
				case 11:
					if (questionAndAnswer == "divorce_Yes")
					{
						if (Game1.player.Money >= 50000 || Game1.player.hasCurrentOrPendingRoommate())
						{
							if (!Game1.player.hasRoommate())
							{
								Game1.player.Money -= 50000;
							}
							Game1.player.divorceTonight.Value = true;
							if (Game1.player.hasCurrentOrPendingRoommate())
							{
								s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Filed_Krobus", Game1.player.getSpouse().displayName);
							}
							if (s == null)
							{
								s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed", true);
							}
							Game1.drawObjectDialogue(s);
							if (!Game1.player.hasRoommate())
							{
								Game1.multiplayer.globalChatInfoMessage("Divorce", new string[]
								{
									Game1.player.Name
								});
							}
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
						}
					}
					break;
				case 12:
				case 13:
				case 14:
				case 15:
				case 18:
				case 20:
				case 21:
				case 24:
					break;
				case 16:
					if (questionAndAnswer == "mergeWallets_Yes")
					{
						if (ManorHouse.changeWalletTypeImmediately)
						{
							ManorHouse.MergeWallets();
						}
						else
						{
							Game1.player.changeWalletTypeTonight.Value = true;
							Game1.multiplayer.globalChatInfoMessage("MergeWallets", new string[]
							{
								Game1.player.Name
							});
						}
					}
					break;
				case 17:
					if (questionAndAnswer == "divorceCancel_Yes")
					{
						if (Game1.player.divorceTonight.Value)
						{
							Game1.player.divorceTonight.Value = false;
							if (!Game1.player.hasRoommate())
							{
								Game1.player.addUnearnedMoney(50000);
							}
							if (Game1.player.hasCurrentOrPendingRoommate())
							{
								s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Cancelled_Krobus", Game1.player.getSpouse().displayName);
							}
							if (s == null)
							{
								s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Cancelled", true);
							}
							Game1.drawObjectDialogue(s);
							if (!Game1.player.hasRoommate())
							{
								Game1.multiplayer.globalChatInfoMessage("DivorceCancel", new string[]
								{
									Game1.player.Name
								});
							}
						}
					}
					break;
				case 19:
					if (questionAndAnswer == "separateWallets_Yes")
					{
						if (ManorHouse.changeWalletTypeImmediately)
						{
							ManorHouse.SeparateWallets();
						}
						else
						{
							Game1.player.changeWalletTypeTonight.Value = true;
							Game1.multiplayer.globalChatInfoMessage("SeparateWallets", new string[]
							{
								Game1.player.Name
							});
						}
					}
					break;
				case 22:
					if (questionAndAnswer == "cancelMergeWallets_Yes")
					{
						Game1.player.changeWalletTypeTonight.Value = false;
						Game1.multiplayer.globalChatInfoMessage("MergeWalletsCancel", new string[]
						{
							Game1.player.Name
						});
					}
					break;
				case 23:
					if (questionAndAnswer == "ledgerOptions_SendMoney")
					{
						this.ChooseRecipient();
					}
					break;
				case 25:
				{
					char c = questionAndAnswer[0];
					if (c != 'c')
					{
						if (c == 'l')
						{
							if (questionAndAnswer == "ledgerOptions_CancelMerge")
							{
								s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_CancelQuestion");
								base.createQuestionDialogue(s, base.createYesNoResponses(), "cancelMergeWallets");
							}
						}
					}
					else if (questionAndAnswer == "cancelSeparateWallets_Yes")
					{
						Game1.player.changeWalletTypeTonight.Value = false;
						Game1.multiplayer.globalChatInfoMessage("SeparateWalletsCancel", new string[]
						{
							Game1.player.Name
						});
					}
					break;
				}
				case 26:
					if (questionAndAnswer == "ledgerOptions_MergeWallets")
					{
						s = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_MergeQuestion");
						base.createQuestionDialogue(s, base.createYesNoResponses(), "mergeWallets");
					}
					break;
				case 27:
					if (questionAndAnswer == "lostAndFound_CheckDonations")
					{
						Game1.player.team.CheckReturnedDonations();
						return true;
					}
					break;
				default:
					if (length == 34)
					{
						if (questionAndAnswer == "lostAndFound_RetrieveFarmhandItems")
						{
							this.ShowOfflineFarmhandItemList();
							return true;
						}
					}
					break;
				}
			}
			if (questionAndAnswer.StartsWith("CheckItems"))
			{
				long id;
				if (long.TryParse(questionAndAnswer.Split('_', StringSplitOptions.None)[1], out id))
				{
					Farmer farmhand = Game1.GetPlayer(id, false);
					if (farmhand != null)
					{
						Cabin home = Utility.getHomeOfFarmer(farmhand) as Cabin;
						if (home != null && !farmhand.isActive())
						{
							home.inventoryMutex.RequestLock(new Action(home.openFarmhandInventory), null);
						}
					}
				}
				return true;
			}
			if (questionAndAnswer.Contains("Transfer"))
			{
				string answer = questionAndAnswer.Split('_', StringSplitOptions.None)[1];
				this.beginSendMoney(this.sendMoneyMapping[answer]);
				this.sendMoneyMapping.Clear();
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x040020ED RID: 8429
		[XmlIgnore]
		private Dictionary<string, Farmer> sendMoneyMapping = new Dictionary<string, Farmer>();

		// Token: 0x040020EE RID: 8430
		private static readonly bool changeWalletTypeImmediately;
	}
}

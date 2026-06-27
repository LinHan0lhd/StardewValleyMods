using System;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Locations
{
	// Token: 0x020002EF RID: 751
	public class ShopLocation : GameLocation
	{
		// Token: 0x06003236 RID: 12854 RVA: 0x002810A7 File Offset: 0x0027F2A7
		public ShopLocation()
		{
		}

		// Token: 0x06003237 RID: 12855 RVA: 0x002810C5 File Offset: 0x0027F2C5
		public ShopLocation(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06003238 RID: 12856 RVA: 0x002810E5 File Offset: 0x0027F2E5
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.itemsFromPlayerToSell, "itemsFromPlayerToSell").AddField(this.itemsToStartSellingTomorrow, "itemsToStartSellingTomorrow");
		}

		// Token: 0x06003239 RID: 12857 RVA: 0x00281114 File Offset: 0x0027F314
		public virtual Dialogue getPurchasedItemDialogueForNPC(Object i, NPC n)
		{
			Dialogue response = null;
			string[] split = Game1.content.LoadString("Strings\\Lexicon:GenericPlayerTerm").Split('^', StringSplitOptions.None);
			string genderName = split[0];
			if (split.Length > 1 && !Game1.player.IsMale)
			{
				genderName = split[1];
			}
			string whatToCallPlayer = (Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(n.Name) / 1250)) ? Game1.player.Name : genderName;
			if (n.Age != 0)
			{
				whatToCallPlayer = Game1.player.Name;
			}
			string particle = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "";
			if ((i.Category == -4 || i.Category == -75 || i.Category == -79) && Game1.random.NextBool())
			{
				particle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SeedShop.cs.9701");
			}
			int whichDialogue = Game1.random.Next(5);
			if (n.Manners == 2)
			{
				whichDialogue = 2;
			}
			switch (whichDialogue)
			{
			case 0:
				if (Game1.random.NextDouble() < (double)i.quality.Value * 0.5 + 0.2)
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityHigh", new object[]
					{
						whatToCallPlayer,
						particle,
						i.DisplayName,
						Lexicon.getRandomDeliciousAdjective(n)
					});
				}
				else
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityLow", new object[]
					{
						whatToCallPlayer,
						particle,
						i.DisplayName,
						Lexicon.getRandomNegativeFoodAdjective(n)
					});
				}
				break;
			case 1:
				if (i.quality.Value == 0)
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityLow", whatToCallPlayer, particle, i.DisplayName);
				}
				else if (n.Name.Equals("Jodi"))
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi", whatToCallPlayer, particle, i.DisplayName);
				}
				else
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh", whatToCallPlayer, particle, i.DisplayName);
				}
				break;
			case 2:
				if (n.Manners == 2)
				{
					if (i.quality.Value != 2)
					{
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude", new object[]
						{
							whatToCallPlayer,
							particle,
							i.DisplayName,
							i.salePrice(false) / 2,
							Lexicon.getRandomNegativeFoodAdjective(n),
							Lexicon.getRandomNegativeItemSlanderNoun()
						});
					}
					else
					{
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude", new object[]
						{
							whatToCallPlayer,
							particle,
							i.DisplayName,
							i.salePrice(false) / 2,
							Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n)
						});
					}
				}
				else
				{
					Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_NonRude", new object[]
					{
						whatToCallPlayer,
						particle,
						i.DisplayName,
						i.salePrice(false) / 2
					});
				}
				break;
			case 3:
				response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_4", whatToCallPlayer, particle, i.DisplayName);
				break;
			case 4:
			{
				int category = i.Category;
				if (category != -79 && category != -75)
				{
					if (category != -7)
					{
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_5_Foraged", whatToCallPlayer, particle, i.DisplayName);
					}
					else
					{
						string adjective = Lexicon.getRandomPositiveAdjectiveForEventOrPerson(n);
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_5_Cooking", new object[]
						{
							whatToCallPlayer,
							particle,
							i.DisplayName,
							Lexicon.getProperArticleForWord(adjective),
							adjective
						});
					}
				}
				else
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_5_VegetableOrFruit", whatToCallPlayer, particle, i.DisplayName);
				}
				break;
			}
			}
			if (n.Age == 1 && Game1.random.NextDouble() < 0.6)
			{
				response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Teen", whatToCallPlayer, particle, i.DisplayName);
			}
			string name = n.Name;
			if (name != null)
			{
				switch (name.Length)
				{
				case 4:
				{
					char c = name[0];
					if (c != 'A')
					{
						if (c == 'L')
						{
							if (name == "Leah")
							{
								response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Leah", whatToCallPlayer, particle, i.DisplayName);
							}
						}
					}
					else if (name == "Alex")
					{
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Alex", whatToCallPlayer, particle, i.DisplayName);
					}
					break;
				}
				case 5:
					if (name == "Haley")
					{
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Haley", whatToCallPlayer, particle, i.DisplayName);
					}
					break;
				case 6:
					if (name == "Pierre")
					{
						string key = (i.quality.Value == 0) ? "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow" : "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh";
						response = Dialogue.FromTranslation(n, key, whatToCallPlayer, particle, i.DisplayName);
					}
					break;
				case 7:
				{
					char c = name[0];
					if (c != 'A')
					{
						if (c == 'E')
						{
							if (name == "Elliott")
							{
								response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Elliott", whatToCallPlayer, particle, i.DisplayName);
							}
						}
					}
					else if (name == "Abigail")
					{
						if (i.quality.Value == 0)
						{
							response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Abigail_QualityLow", new object[]
							{
								whatToCallPlayer,
								particle,
								i.DisplayName,
								Lexicon.getRandomNegativeItemSlanderNoun()
							});
						}
						else
						{
							response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Abigail_QualityHigh", whatToCallPlayer, particle, i.DisplayName);
						}
					}
					break;
				}
				case 8:
					if (name == "Caroline")
					{
						string key2 = (i.quality.Value == 0) ? "Data\\ExtraDialogue:PurchasedItem_Caroline_QualityLow" : "Data\\ExtraDialogue:PurchasedItem_Caroline_QualityHigh";
						response = Dialogue.FromTranslation(n, key2, whatToCallPlayer, particle, i.DisplayName);
					}
					break;
				}
			}
			return response;
		}

		// Token: 0x0600323A RID: 12858 RVA: 0x002816FC File Offset: 0x0027F8FC
		public override void DayUpdate(int dayOfMonth)
		{
			this.itemsToStartSellingTomorrow.RemoveWhere((Item p) => p == null);
			this.itemsFromPlayerToSell.RemoveWhere((Item p) => p == null);
			for (int i = this.itemsToStartSellingTomorrow.Count - 1; i >= 0; i--)
			{
				Item tomorrowItem = this.itemsToStartSellingTomorrow[i];
				if (this.itemsFromPlayerToSell.Count < 11)
				{
					bool stacked = false;
					foreach (Item item in this.itemsFromPlayerToSell)
					{
						if (item.Name == tomorrowItem.Name && item.Quality == tomorrowItem.Quality)
						{
							item.Stack += tomorrowItem.Stack;
							stacked = true;
							break;
						}
					}
					this.itemsToStartSellingTomorrow.RemoveAt(i);
					if (!stacked)
					{
						this.itemsFromPlayerToSell.Add(tomorrowItem);
					}
				}
			}
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x04002194 RID: 8596
		public const int maxItemsToSellFromPlayer = 11;

		// Token: 0x04002195 RID: 8597
		public readonly NetObjectList<Item> itemsFromPlayerToSell = new NetObjectList<Item>();

		// Token: 0x04002196 RID: 8598
		public readonly NetObjectList<Item> itemsToStartSellingTomorrow = new NetObjectList<Item>();
	}
}

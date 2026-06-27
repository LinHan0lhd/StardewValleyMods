using System;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002D1 RID: 721
	public class FishShop : ShopLocation
	{
		// Token: 0x06002F59 RID: 12121 RVA: 0x00254762 File Offset: 0x00252962
		public FishShop()
		{
		}

		// Token: 0x06002F5A RID: 12122 RVA: 0x0025476A File Offset: 0x0025296A
		public FishShop(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002F5B RID: 12123 RVA: 0x00254774 File Offset: 0x00252974
		public override Dialogue getPurchasedItemDialogueForNPC(Object i, NPC n)
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
			case 4:
				if (Game1.random.NextDouble() < (double)i.quality.Value * 0.5 + 0.2)
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityHigh_Willy", new object[]
					{
						whatToCallPlayer,
						particle,
						i.DisplayName,
						Lexicon.getRandomDeliciousAdjective(n)
					});
				}
				else
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityLow_Willy", new object[]
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
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityLow_Willy", whatToCallPlayer, particle, i.DisplayName);
				}
				else if (n.Name.Equals("Jodi"))
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi_Willy", whatToCallPlayer, particle, i.DisplayName);
				}
				else
				{
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Willy", whatToCallPlayer, particle, i.DisplayName);
				}
				break;
			case 2:
				if (n.Manners == 2)
				{
					if (i.quality.Value < 2)
					{
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude_Willy", new object[]
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
						response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude_Willy", new object[]
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
					response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_NonRude_Willy", new object[]
					{
						whatToCallPlayer,
						particle,
						i.DisplayName,
						i.salePrice(false) / 2
					});
				}
				break;
			case 3:
				response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_4_Willy", whatToCallPlayer, particle, i.DisplayName);
				break;
			}
			if (n.Name == "Willy")
			{
				string key = (i.quality.Value == 0) ? "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow_Willy" : "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh_Willy";
				response = Dialogue.FromTranslation(n, key, whatToCallPlayer, particle, i.DisplayName);
			}
			return response;
		}

		// Token: 0x06002F5C RID: 12124 RVA: 0x00254AB8 File Offset: 0x00252CB8
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "WarpBoatTunnel")
			{
				if (Game1.player.mailReceived.Contains("willyBackRoomInvitation"))
				{
					Game1.warpFarmer("BoatTunnel", 6, 12, false);
					base.playSound("doorClose", null, null, SoundContext.Default);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
				}
			}
			return base.performAction(action, who, tileLocation);
		}
	}
}

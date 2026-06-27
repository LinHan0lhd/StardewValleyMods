using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Companions;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shirts;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewValley
{
	// Token: 0x020000A3 RID: 163
	public class Farmer : Character, IComparable
	{
		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060007EE RID: 2030 RVA: 0x00052B54 File Offset: 0x00050D54
		public bool hasVisibleQuests
		{
			get
			{
				using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator = this.team.specialOrders.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator.Current.IsHidden())
						{
							return true;
						}
					}
				}
				foreach (Quest quest in this.questLog)
				{
					if (quest != null && !quest.IsHidden())
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060007EF RID: 2031 RVA: 0x00052BFC File Offset: 0x00050DFC
		// (set) Token: 0x060007F0 RID: 2032 RVA: 0x00052C09 File Offset: 0x00050E09
		public Item recoveredItem
		{
			get
			{
				return this._recoveredItem.Value;
			}
			set
			{
				this._recoveredItem.Value = value;
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060007F1 RID: 2033 RVA: 0x00052C18 File Offset: 0x00050E18
		// (set) Token: 0x060007F2 RID: 2034 RVA: 0x00052C2E File Offset: 0x00050E2E
		[XmlElement("isMale")]
		public bool? obsolete_isMale
		{
			get
			{
				return null;
			}
			set
			{
				if (value != null)
				{
					this.Gender = (value.Value ? Gender.Male : Gender.Female);
				}
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060007F3 RID: 2035 RVA: 0x00052C4C File Offset: 0x00050E4C
		[XmlIgnore]
		public bool catPerson
		{
			get
			{
				return this.whichPetType == "Cat";
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060007F4 RID: 2036 RVA: 0x00052C5E File Offset: 0x00050E5E
		// (set) Token: 0x060007F5 RID: 2037 RVA: 0x00052C6C File Offset: 0x00050E6C
		[XmlIgnore]
		public int festivalScore
		{
			get
			{
				return this.netFestivalScore.Value;
			}
			set
			{
				FarmerTeam team = this.team;
				if (((team != null) ? team.festivalScoreStatus : null) != null)
				{
					this.team.festivalScoreStatus.UpdateState(this.festivalScore.ToString() ?? "");
				}
				this.netFestivalScore.Value = value;
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060007F6 RID: 2038 RVA: 0x00052CC0 File Offset: 0x00050EC0
		// (set) Token: 0x060007F7 RID: 2039 RVA: 0x00052CCD File Offset: 0x00050ECD
		public int deepestMineLevel
		{
			get
			{
				return this.netDeepestMineLevel.Value;
			}
			set
			{
				this.netDeepestMineLevel.Value = value;
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060007F8 RID: 2040 RVA: 0x00052CDB File Offset: 0x00050EDB
		// (set) Token: 0x060007F9 RID: 2041 RVA: 0x00052CE8 File Offset: 0x00050EE8
		public float stamina
		{
			get
			{
				return this.netStamina.Value;
			}
			set
			{
				this.netStamina.Value = value;
			}
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060007FA RID: 2042 RVA: 0x00052CF6 File Offset: 0x00050EF6
		[XmlIgnore]
		public FarmerTeam team
		{
			get
			{
				if (Game1.player != null && this != Game1.player)
				{
					return Game1.player.team;
				}
				return this.teamRoot.Value;
			}
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060007FB RID: 2043 RVA: 0x00052D1D File Offset: 0x00050F1D
		// (set) Token: 0x060007FC RID: 2044 RVA: 0x00052D34 File Offset: 0x00050F34
		public uint totalMoneyEarned
		{
			get
			{
				return (uint)this.teamRoot.Value.totalMoneyEarned.Value;
			}
			set
			{
				if (this.teamRoot.Value.totalMoneyEarned.Value != 0)
				{
					if (value >= 15000U && this.teamRoot.Value.totalMoneyEarned.Value < 15000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned15k", new string[]
						{
							this.farmName.Value
						});
					}
					if (value >= 50000U && this.teamRoot.Value.totalMoneyEarned.Value < 50000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned50k", new string[]
						{
							this.farmName.Value
						});
					}
					if (value >= 250000U && this.teamRoot.Value.totalMoneyEarned.Value < 250000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned250k", new string[]
						{
							this.farmName.Value
						});
					}
					if (value >= 1000000U && this.teamRoot.Value.totalMoneyEarned.Value < 1000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned1m", new string[]
						{
							this.farmName.Value
						});
					}
					if (value >= 10000000U && this.teamRoot.Value.totalMoneyEarned.Value < 10000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned10m", new string[]
						{
							this.farmName.Value
						});
					}
					if (value >= 100000000U && this.teamRoot.Value.totalMoneyEarned.Value < 100000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned100m", new string[]
						{
							this.farmName.Value
						});
					}
				}
				this.teamRoot.Value.totalMoneyEarned.Value = (int)value;
			}
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060007FD RID: 2045 RVA: 0x00052F1B File Offset: 0x0005111B
		// (set) Token: 0x060007FE RID: 2046 RVA: 0x00052F28 File Offset: 0x00051128
		public ulong millisecondsPlayed
		{
			get
			{
				return (ulong)this.netMillisecondsPlayed.Value;
			}
			set
			{
				this.netMillisecondsPlayed.Value = (long)value;
			}
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060007FF RID: 2047 RVA: 0x00052F36 File Offset: 0x00051136
		// (set) Token: 0x06000800 RID: 2048 RVA: 0x00052F4C File Offset: 0x0005114C
		[XmlIgnore]
		public bool canUnderstandDwarves
		{
			get
			{
				return Game1.MasterPlayer.mailReceived.Contains("HasDwarvishTranslationGuide");
			}
			set
			{
				Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasDwarvishTranslationGuide", MailType.Received, value, null);
			}
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000801 RID: 2049 RVA: 0x00052F79 File Offset: 0x00051179
		// (set) Token: 0x06000802 RID: 2050 RVA: 0x00052F8B File Offset: 0x0005118B
		[XmlIgnore]
		public bool hasClubCard
		{
			get
			{
				return this.mailReceived.Contains("HasClubCard");
			}
			set
			{
				this.mailReceived.Toggle("HasClubCard", value);
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x06000803 RID: 2051 RVA: 0x00052F9E File Offset: 0x0005119E
		// (set) Token: 0x06000804 RID: 2052 RVA: 0x00052FB0 File Offset: 0x000511B0
		[XmlIgnore]
		public bool hasDarkTalisman
		{
			get
			{
				return this.mailReceived.Contains("HasDarkTalisman");
			}
			set
			{
				this.mailReceived.Toggle("HasDarkTalisman", value);
			}
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000805 RID: 2053 RVA: 0x00052FC3 File Offset: 0x000511C3
		// (set) Token: 0x06000806 RID: 2054 RVA: 0x00052FD5 File Offset: 0x000511D5
		[XmlIgnore]
		public bool hasMagicInk
		{
			get
			{
				return this.mailReceived.Contains("HasMagicInk");
			}
			set
			{
				this.mailReceived.Toggle("HasMagicInk", value);
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000807 RID: 2055 RVA: 0x00052FE8 File Offset: 0x000511E8
		// (set) Token: 0x06000808 RID: 2056 RVA: 0x00052FFA File Offset: 0x000511FA
		[XmlIgnore]
		public bool hasMagnifyingGlass
		{
			get
			{
				return this.mailReceived.Contains("HasMagnifyingGlass");
			}
			set
			{
				this.mailReceived.Toggle("HasMagnifyingGlass", value);
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000809 RID: 2057 RVA: 0x0005300D File Offset: 0x0005120D
		// (set) Token: 0x0600080A RID: 2058 RVA: 0x00053024 File Offset: 0x00051224
		[XmlIgnore]
		public bool hasRustyKey
		{
			get
			{
				return Game1.MasterPlayer.mailReceived.Contains("HasRustyKey");
			}
			set
			{
				Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasRustyKey", MailType.Received, value, null);
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x0600080B RID: 2059 RVA: 0x00053051 File Offset: 0x00051251
		// (set) Token: 0x0600080C RID: 2060 RVA: 0x00053068 File Offset: 0x00051268
		[XmlIgnore]
		public bool hasSkullKey
		{
			get
			{
				return Game1.MasterPlayer.mailReceived.Contains("HasSkullKey");
			}
			set
			{
				Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasSkullKey", MailType.Received, value, null);
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x0600080D RID: 2061 RVA: 0x00053095 File Offset: 0x00051295
		// (set) Token: 0x0600080E RID: 2062 RVA: 0x000530A7 File Offset: 0x000512A7
		[XmlIgnore]
		public bool hasSpecialCharm
		{
			get
			{
				return this.mailReceived.Contains("HasSpecialCharm");
			}
			set
			{
				this.mailReceived.Toggle("HasSpecialCharm", value);
			}
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x0600080F RID: 2063 RVA: 0x000530BA File Offset: 0x000512BA
		// (set) Token: 0x06000810 RID: 2064 RVA: 0x000530CC File Offset: 0x000512CC
		[XmlIgnore]
		public bool HasTownKey
		{
			get
			{
				return this.mailReceived.Contains("HasTownKey");
			}
			set
			{
				this.mailReceived.Toggle("HasTownKey", value);
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000811 RID: 2065 RVA: 0x000530DF File Offset: 0x000512DF
		// (set) Token: 0x06000812 RID: 2066 RVA: 0x000530F1 File Offset: 0x000512F1
		[XmlIgnore]
		public bool hasUnlockedSkullDoor
		{
			get
			{
				return this.mailReceived.Contains("HasUnlockedSkullDoor");
			}
			set
			{
				this.mailReceived.Toggle("HasUnlockedSkullDoor", value);
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000813 RID: 2067 RVA: 0x00053104 File Offset: 0x00051304
		[XmlIgnore]
		public bool hasPendingCompletedQuests
		{
			get
			{
				foreach (SpecialOrder quest in this.team.specialOrders)
				{
					if (quest.participants.ContainsKey(this.UniqueMultiplayerID) && quest.ShouldDisplayAsComplete())
					{
						return true;
					}
				}
				foreach (Quest quest2 in this.questLog)
				{
					if (!quest2.IsHidden() && quest2.ShouldDisplayAsComplete() && !quest2.destroy.Value)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x000531D8 File Offset: 0x000513D8
		// (set) Token: 0x06000815 RID: 2069 RVA: 0x000531EF File Offset: 0x000513EF
		[XmlElement("useSeparateWallets")]
		public bool useSeparateWallets
		{
			get
			{
				return this.teamRoot.Value.useSeparateWallets.Value;
			}
			set
			{
				this.teamRoot.Value.useSeparateWallets.Value = value;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x00053207 File Offset: 0x00051407
		// (set) Token: 0x06000817 RID: 2071 RVA: 0x0005321E File Offset: 0x0005141E
		[XmlElement("theaterBuildDate")]
		public long theaterBuildDate
		{
			get
			{
				return this.teamRoot.Value.theaterBuildDate.Value;
			}
			set
			{
				this.teamRoot.Value.theaterBuildDate.Value = value;
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06000818 RID: 2072 RVA: 0x00053236 File Offset: 0x00051436
		// (set) Token: 0x06000819 RID: 2073 RVA: 0x00053243 File Offset: 0x00051443
		public int timesReachedMineBottom
		{
			get
			{
				return this.netTimesReachedMineBottom.Value;
			}
			set
			{
				this.netTimesReachedMineBottom.Value = value;
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x0600081A RID: 2074 RVA: 0x00053251 File Offset: 0x00051451
		// (set) Token: 0x0600081B RID: 2075 RVA: 0x0005325E File Offset: 0x0005145E
		[XmlIgnore]
		public bool canReleaseTool
		{
			get
			{
				return this.netCanReleaseTool.Value;
			}
			set
			{
				this.netCanReleaseTool.Value = value;
			}
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x0600081C RID: 2076 RVA: 0x0005326C File Offset: 0x0005146C
		// (set) Token: 0x0600081D RID: 2077 RVA: 0x0005328D File Offset: 0x0005148D
		[XmlElement("spouse")]
		public string spouse
		{
			get
			{
				if (!string.IsNullOrEmpty(this.netSpouse.Value))
				{
					return this.netSpouse.Value;
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					this.netSpouse.Value = "";
					return;
				}
				this.netSpouse.Value = value;
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x0600081E RID: 2078 RVA: 0x000532AF File Offset: 0x000514AF
		[XmlIgnore]
		public bool isUnclaimedFarmhand
		{
			get
			{
				return !this.IsMainPlayer && !this.isCustomized.Value;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600081F RID: 2079 RVA: 0x000532C9 File Offset: 0x000514C9
		// (set) Token: 0x06000820 RID: 2080 RVA: 0x000532D6 File Offset: 0x000514D6
		[XmlIgnore]
		public Horse mount
		{
			get
			{
				return this.netMount.Value;
			}
			set
			{
				this.setMount(value);
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06000821 RID: 2081 RVA: 0x000532DF File Offset: 0x000514DF
		// (set) Token: 0x06000822 RID: 2082 RVA: 0x000532EC File Offset: 0x000514EC
		[XmlIgnore]
		public int MaxItems
		{
			get
			{
				return this.maxItems.Value;
			}
			set
			{
				this.maxItems.Value = value;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000823 RID: 2083 RVA: 0x000532FC File Offset: 0x000514FC
		[XmlIgnore]
		public int Level
		{
			get
			{
				return (this.farmingLevel.Value + this.fishingLevel.Value + this.foragingLevel.Value + this.combatLevel.Value + this.miningLevel.Value + this.luckLevel.Value) / 2;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000824 RID: 2084 RVA: 0x00053352 File Offset: 0x00051552
		[XmlIgnore]
		public int FarmingLevel
		{
			get
			{
				return Math.Max(this.farmingLevel.Value + this.buffs.FarmingLevel, 0);
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000825 RID: 2085 RVA: 0x00053371 File Offset: 0x00051571
		[XmlIgnore]
		public int MiningLevel
		{
			get
			{
				return Math.Max(this.miningLevel.Value + this.buffs.MiningLevel, 0);
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000826 RID: 2086 RVA: 0x00053390 File Offset: 0x00051590
		[XmlIgnore]
		public int CombatLevel
		{
			get
			{
				return Math.Max(this.combatLevel.Value + this.buffs.CombatLevel, 0);
			}
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000827 RID: 2087 RVA: 0x000533AF File Offset: 0x000515AF
		[XmlIgnore]
		public int ForagingLevel
		{
			get
			{
				return Math.Max(this.foragingLevel.Value + this.buffs.ForagingLevel, 0);
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000828 RID: 2088 RVA: 0x000533CE File Offset: 0x000515CE
		[XmlIgnore]
		public int FishingLevel
		{
			get
			{
				return Math.Max(this.fishingLevel.Value + this.buffs.FishingLevel, 0);
			}
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000829 RID: 2089 RVA: 0x000533ED File Offset: 0x000515ED
		[XmlIgnore]
		public int LuckLevel
		{
			get
			{
				return Math.Max(this.luckLevel.Value + this.buffs.LuckLevel, 0);
			}
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x0600082A RID: 2090 RVA: 0x0005340C File Offset: 0x0005160C
		[XmlIgnore]
		public double DailyLuck
		{
			get
			{
				return Math.Min(Math.Max(this.team.sharedDailyLuck.Value + (double)(this.hasSpecialCharm ? 0.025f : 0f), -0.20000000298023224), 0.20000000298023224);
			}
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x0600082B RID: 2091 RVA: 0x0005345B File Offset: 0x0005165B
		// (set) Token: 0x0600082C RID: 2092 RVA: 0x00053468 File Offset: 0x00051668
		[XmlIgnore]
		public int HouseUpgradeLevel
		{
			get
			{
				return this.houseUpgradeLevel.Value;
			}
			set
			{
				this.houseUpgradeLevel.Value = value;
			}
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x0600082D RID: 2093 RVA: 0x00053476 File Offset: 0x00051676
		// (set) Token: 0x0600082E RID: 2094 RVA: 0x0005347E File Offset: 0x0005167E
		[XmlIgnore]
		public BoundingBoxGroup TemporaryPassableTiles
		{
			get
			{
				return this.temporaryPassableTiles;
			}
			set
			{
				this.temporaryPassableTiles = value;
			}
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x0600082F RID: 2095 RVA: 0x00053487 File Offset: 0x00051687
		[XmlIgnore]
		public Inventory Items
		{
			get
			{
				return this.netItems.Value;
			}
		}

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000830 RID: 2096 RVA: 0x00053494 File Offset: 0x00051694
		[XmlIgnore]
		public int MagneticRadius
		{
			get
			{
				return Math.Max(this.BaseMagneticRadius + this.buffs.MagneticRadius, 0);
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000831 RID: 2097 RVA: 0x000534B0 File Offset: 0x000516B0
		// (set) Token: 0x06000832 RID: 2098 RVA: 0x0005351E File Offset: 0x0005171E
		[XmlIgnore]
		public Item ActiveItem
		{
			get
			{
				if (this.TemporaryItem != null)
				{
					return this.TemporaryItem;
				}
				if (this._itemStowed)
				{
					return null;
				}
				if (this.currentToolIndex.Value < this.Items.Count && this.Items[this.currentToolIndex.Value] != null)
				{
					return this.Items[this.currentToolIndex.Value];
				}
				return null;
			}
			set
			{
				this.netItemStowed.Set(false);
				if (value == null)
				{
					this.removeItemFromInventory(this.ActiveItem);
					return;
				}
				this.addItemToInventory(value, this.CurrentToolIndex);
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000833 RID: 2099 RVA: 0x0005354A File Offset: 0x0005174A
		// (set) Token: 0x06000834 RID: 2100 RVA: 0x00053557 File Offset: 0x00051757
		[XmlIgnore]
		public Object ActiveObject
		{
			get
			{
				return this.ActiveItem as Object;
			}
			set
			{
				this.ActiveItem = value;
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000835 RID: 2101 RVA: 0x00053560 File Offset: 0x00051760
		// (set) Token: 0x06000836 RID: 2102 RVA: 0x0005356D File Offset: 0x0005176D
		[XmlIgnore]
		public override Gender Gender
		{
			get
			{
				return this.netGender.Value;
			}
			set
			{
				this.netGender.Value = value;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000837 RID: 2103 RVA: 0x0005357B File Offset: 0x0005177B
		[XmlIgnore]
		public bool IsMale
		{
			get
			{
				return this.netGender.Value == Gender.Male;
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000838 RID: 2104 RVA: 0x0005358B File Offset: 0x0005178B
		[XmlIgnore]
		public ISet<string> DialogueQuestionsAnswered
		{
			get
			{
				return this.dialogueQuestionsAnswered;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000839 RID: 2105 RVA: 0x00053593 File Offset: 0x00051793
		// (set) Token: 0x0600083A RID: 2106 RVA: 0x0005359B File Offset: 0x0005179B
		[XmlIgnore]
		public bool CanMove
		{
			get
			{
				return this.canMove;
			}
			set
			{
				this.canMove = value;
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600083B RID: 2107 RVA: 0x000535A4 File Offset: 0x000517A4
		// (set) Token: 0x0600083C RID: 2108 RVA: 0x000535B1 File Offset: 0x000517B1
		[XmlIgnore]
		public bool UsingTool
		{
			get
			{
				return this.usingTool.Value;
			}
			set
			{
				this.usingTool.Set(value);
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x0600083D RID: 2109 RVA: 0x000535BF File Offset: 0x000517BF
		// (set) Token: 0x0600083E RID: 2110 RVA: 0x000535CC File Offset: 0x000517CC
		[XmlIgnore]
		public Tool CurrentTool
		{
			get
			{
				return this.CurrentItem as Tool;
			}
			set
			{
				while (this.CurrentToolIndex >= this.Items.Count)
				{
					this.Items.Add(null);
				}
				this.Items[this.CurrentToolIndex] = value;
			}
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x0600083F RID: 2111 RVA: 0x00053601 File Offset: 0x00051801
		// (set) Token: 0x06000840 RID: 2112 RVA: 0x0005360E File Offset: 0x0005180E
		[XmlIgnore]
		public Item TemporaryItem
		{
			get
			{
				return this.temporaryItem.Value;
			}
			set
			{
				this.temporaryItem.Value = value;
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x06000841 RID: 2113 RVA: 0x0005361C File Offset: 0x0005181C
		// (set) Token: 0x06000842 RID: 2114 RVA: 0x00053629 File Offset: 0x00051829
		public Item CursorSlotItem
		{
			get
			{
				return this.cursorSlotItem.Value;
			}
			set
			{
				this.cursorSlotItem.Value = value;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x06000843 RID: 2115 RVA: 0x00053638 File Offset: 0x00051838
		[XmlIgnore]
		public Item CurrentItem
		{
			get
			{
				if (this.TemporaryItem != null)
				{
					return this.TemporaryItem;
				}
				if (this._itemStowed)
				{
					return null;
				}
				if (this.currentToolIndex.Value >= this.Items.Count)
				{
					return null;
				}
				return this.Items[this.currentToolIndex.Value];
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000844 RID: 2116 RVA: 0x0005368E File Offset: 0x0005188E
		// (set) Token: 0x06000845 RID: 2117 RVA: 0x0005369C File Offset: 0x0005189C
		[XmlIgnore]
		public int CurrentToolIndex
		{
			get
			{
				return this.currentToolIndex.Value;
			}
			set
			{
				this.netItemStowed.Set(false);
				if (this.currentToolIndex.Value >= 0 && this.CurrentItem != null && value != this.currentToolIndex.Value)
				{
					this.CurrentItem.actionWhenStopBeingHeld(this);
				}
				this.currentToolIndex.Set(value);
			}
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000846 RID: 2118 RVA: 0x000536F1 File Offset: 0x000518F1
		// (set) Token: 0x06000847 RID: 2119 RVA: 0x000536F9 File Offset: 0x000518F9
		[XmlIgnore]
		public float Stamina
		{
			get
			{
				return this.stamina;
			}
			set
			{
				if (this.hasBuff("statue_of_blessings_2") && value < this.stamina)
				{
					return;
				}
				this.stamina = Math.Min((float)this.MaxStamina, Math.Max(value, -16f));
			}
		}

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000848 RID: 2120 RVA: 0x0005372F File Offset: 0x0005192F
		[XmlIgnore]
		public int MaxStamina
		{
			get
			{
				return Math.Max(this.maxStamina.Value + this.buffs.MaxStamina, 0);
			}
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000849 RID: 2121 RVA: 0x0005374E File Offset: 0x0005194E
		[XmlIgnore]
		public int Attack
		{
			get
			{
				return this.buffs.Attack;
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600084A RID: 2122 RVA: 0x0005375B File Offset: 0x0005195B
		[XmlIgnore]
		public int Immunity
		{
			get
			{
				return this.buffs.Immunity;
			}
		}

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x0600084B RID: 2123 RVA: 0x00053768 File Offset: 0x00051968
		// (set) Token: 0x0600084C RID: 2124 RVA: 0x000537D0 File Offset: 0x000519D0
		[XmlIgnore]
		public override float addedSpeed
		{
			get
			{
				return this.buffs.Speed + ((this.stats.Get("Book_Speed") > 0U && !this.isRidingHorse()) ? 0.25f : 0f) + ((this.stats.Get("Book_Speed2") > 0U && !this.isRidingHorse()) ? 0.25f : 0f);
			}
			[Obsolete("Player speed can't be changed directly. You can add a speed buff via applyBuff instead (and optionally mark it invisible).")]
			set
			{
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x0600084D RID: 2125 RVA: 0x000537D2 File Offset: 0x000519D2
		// (set) Token: 0x0600084E RID: 2126 RVA: 0x000537DF File Offset: 0x000519DF
		public long UniqueMultiplayerID
		{
			get
			{
				return this.uniqueMultiplayerID.Value;
			}
			set
			{
				this.uniqueMultiplayerID.Value = value;
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x0600084F RID: 2127 RVA: 0x000537ED File Offset: 0x000519ED
		[XmlIgnore]
		public bool IsLocalPlayer
		{
			get
			{
				return this.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID || (Game1.CurrentEvent != null && Game1.CurrentEvent.farmer == this);
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000850 RID: 2128 RVA: 0x00053819 File Offset: 0x00051A19
		[XmlIgnore]
		public bool IsMainPlayer
		{
			get
			{
				return (Game1.serverHost == null && this.IsLocalPlayer) || (Game1.serverHost != null && this.UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID);
			}
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000851 RID: 2129 RVA: 0x00053858 File Offset: 0x00051A58
		[XmlIgnore]
		public bool IsDedicatedPlayer
		{
			get
			{
				return Game1.HasDedicatedHost && Game1.serverHost != null && this.UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID;
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000852 RID: 2130 RVA: 0x00053887 File Offset: 0x00051A87
		// (set) Token: 0x06000853 RID: 2131 RVA: 0x0005388F File Offset: 0x00051A8F
		[XmlIgnore]
		public override AnimatedSprite Sprite
		{
			get
			{
				return base.Sprite;
			}
			set
			{
				base.Sprite = value;
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000854 RID: 2132 RVA: 0x00053898 File Offset: 0x00051A98
		// (set) Token: 0x06000855 RID: 2133 RVA: 0x000538A5 File Offset: 0x00051AA5
		[XmlIgnore]
		public FarmerSprite FarmerSprite
		{
			get
			{
				return (FarmerSprite)this.Sprite;
			}
			set
			{
				this.Sprite = value;
			}
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000856 RID: 2134 RVA: 0x000538AE File Offset: 0x00051AAE
		// (set) Token: 0x06000857 RID: 2135 RVA: 0x000538BB File Offset: 0x00051ABB
		[XmlIgnore]
		public FarmerRenderer FarmerRenderer
		{
			get
			{
				return this.farmerRenderer.Value;
			}
			set
			{
				this.farmerRenderer.Set(value);
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000858 RID: 2136 RVA: 0x000538C9 File Offset: 0x00051AC9
		// (set) Token: 0x06000859 RID: 2137 RVA: 0x000538E1 File Offset: 0x00051AE1
		[XmlElement("money")]
		public int _money
		{
			get
			{
				return this.teamRoot.Value.GetMoney(this).Value;
			}
			set
			{
				this.teamRoot.Value.GetMoney(this).Value = value;
			}
		}

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x0600085A RID: 2138 RVA: 0x000538FA File Offset: 0x00051AFA
		// (set) Token: 0x0600085B RID: 2139 RVA: 0x00053907 File Offset: 0x00051B07
		[XmlIgnore]
		public int QiGems
		{
			get
			{
				return this.netQiGems.Value;
			}
			set
			{
				this.netQiGems.Value = value;
			}
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x0600085C RID: 2140 RVA: 0x00053915 File Offset: 0x00051B15
		// (set) Token: 0x0600085D RID: 2141 RVA: 0x00053920 File Offset: 0x00051B20
		[XmlIgnore]
		public int Money
		{
			get
			{
				return this._money;
			}
			set
			{
				if (Game1.player != this)
				{
					throw new Exception("Cannot change another farmer's money. Use Game1.player.team.SetIndividualMoney");
				}
				int previousMoney = this._money;
				this._money = value;
				if (value > previousMoney)
				{
					uint earned = (uint)(value - previousMoney);
					this.totalMoneyEarned += earned;
					if (this.useSeparateWallets)
					{
						this.stats.IndividualMoneyEarned += earned;
					}
					Game1.stats.checkForMoneyAchievements();
				}
			}
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x00053989 File Offset: 0x00051B89
		public void addUnearnedMoney(int money)
		{
			this._money += money;
		}

		// Token: 0x0600085F RID: 2143 RVA: 0x0005399C File Offset: 0x00051B9C
		public List<string> GetEmoteFavorites()
		{
			if (this.emoteFavorites.Count == 0)
			{
				this.emoteFavorites.Add("question");
				this.emoteFavorites.Add("heart");
				this.emoteFavorites.Add("yes");
				this.emoteFavorites.Add("happy");
				this.emoteFavorites.Add("pause");
				this.emoteFavorites.Add("sad");
				this.emoteFavorites.Add("no");
				this.emoteFavorites.Add("angry");
			}
			return this.emoteFavorites;
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x00053A40 File Offset: 0x00051C40
		public Farmer()
		{
			this.farmerInit();
			this.Sprite = new FarmerSprite(null);
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x00054264 File Offset: 0x00052464
		public Farmer(FarmerSprite sprite, Vector2 position, int speed, string name, List<Item> initialTools, bool isMale) : base(sprite, position, speed, name)
		{
			this.farmerInit();
			base.Name = name;
			this.displayName = name;
			this.Gender = (isMale ? Gender.Male : Gender.Female);
			this.stamina = (float)this.maxStamina.Value;
			this.Items.OverwriteWith(initialTools);
			for (int i = this.Items.Count; i < this.maxItems.Value; i++)
			{
				this.Items.Add(null);
			}
			this.activeDialogueEvents["Introduction"] = 6;
			if (base.currentLocation != null)
			{
				this.mostRecentBed = Utility.PointToVector2((base.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f;
				return;
			}
			this.mostRecentBed = new Vector2(9f, 9f) * 64f;
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x00054B48 File Offset: 0x00052D48
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.uniqueMultiplayerID, "uniqueMultiplayerID").AddField(this.userID, "userID").AddField(this.platformType, "platformType").AddField(this.platformID, "platformID").AddField(this.hasMenuOpen, "hasMenuOpen").AddField(this.farmerRenderer, "farmerRenderer").AddField(this.netGender, "netGender").AddField(this.bathingClothes, "bathingClothes").AddField(this.shirt, "shirt").AddField(this.pants, "pants").AddField(this.hair, "hair").AddField(this.skin, "skin").AddField(this.shoes, "shoes").AddField(this.accessory, "accessory").AddField(this.facialHair, "facialHair").AddField(this.hairstyleColor, "hairstyleColor").AddField(this.pantsColor, "pantsColor").AddField(this.newEyeColor, "newEyeColor").AddField(this.netItems, "netItems").AddField(this.currentToolIndex, "currentToolIndex").AddField(this.temporaryItem, "temporaryItem").AddField(this.cursorSlotItem, "cursorSlotItem").AddField(this.fireToolEvent, "fireToolEvent").AddField(this.beginUsingToolEvent, "beginUsingToolEvent").AddField(this.endUsingToolEvent, "endUsingToolEvent").AddField(this.hat, "hat").AddField(this.boots, "boots").AddField(this.leftRing, "leftRing").AddField(this.rightRing, "rightRing").AddField(this.hidden, "hidden").AddField(this.usingTool, "usingTool").AddField(this.isInBed, "isInBed").AddField(this.bobberStyle, "bobberStyle").AddField(this.caveChoice, "caveChoice").AddField(this.houseUpgradeLevel, "houseUpgradeLevel").AddField(this.daysUntilHouseUpgrade, "daysUntilHouseUpgrade").AddField(this.netSpouse, "netSpouse").AddField(this.mailReceived, "mailReceived").AddField(this.mailForTomorrow, "mailForTomorrow").AddField(this.mailbox, "mailbox").AddField(this.triggerActionsRun, "triggerActionsRun").AddField(this.eventsSeen, "eventsSeen").AddField(this.locationsVisited, "locationsVisited").AddField(this.secretNotesSeen, "secretNotesSeen").AddField(this.netMount.NetFields, "netMount.NetFields").AddField(this.dancePartner.NetFields, "dancePartner.NetFields").AddField(this.divorceTonight, "divorceTonight").AddField(this.changeWalletTypeTonight, "changeWalletTypeTonight").AddField(this.isCustomized, "isCustomized").AddField(this.homeLocation, "homeLocation").AddField(this.farmName, "farmName").AddField(this.favoriteThing, "favoriteThing").AddField(this.horseName, "horseName").AddField(this.netMillisecondsPlayed, "netMillisecondsPlayed").AddField(this.netFestivalScore, "netFestivalScore").AddField(this.friendshipData, "friendshipData").AddField(this.drinkAnimationEvent, "drinkAnimationEvent").AddField(this.eatAnimationEvent, "eatAnimationEvent").AddField(this.sickAnimationEvent, "sickAnimationEvent").AddField(this.passOutEvent, "passOutEvent").AddField(this.doEmoteEvent, "doEmoteEvent").AddField(this.questLog, "questLog").AddField(this.professions, "professions").AddField(this.newLevels, "newLevels").AddField(this.experiencePoints, "experiencePoints").AddField(this.dialogueQuestionsAnswered, "dialogueQuestionsAnswered").AddField(this.cookingRecipes, "cookingRecipes").AddField(this.craftingRecipes, "craftingRecipes").AddField(this.activeDialogueEvents, "activeDialogueEvents").AddField(this.previousActiveDialogueEvents, "previousActiveDialogueEvents").AddField(this.achievements, "achievements").AddField(this.specialItems, "specialItems").AddField(this.specialBigCraftables, "specialBigCraftables").AddField(this.farmingLevel, "farmingLevel").AddField(this.miningLevel, "miningLevel").AddField(this.combatLevel, "combatLevel").AddField(this.foragingLevel, "foragingLevel").AddField(this.fishingLevel, "fishingLevel").AddField(this.luckLevel, "luckLevel").AddField(this.maxStamina, "maxStamina").AddField(this.netStamina, "netStamina").AddField(this.maxItems, "maxItems").AddField(this.chestConsumedMineLevels, "chestConsumedMineLevels").AddField(this.toolBeingUpgraded, "toolBeingUpgraded").AddField(this.daysLeftForToolUpgrade, "daysLeftForToolUpgrade").AddField(this.exhausted, "exhausted").AddField(this.netDeepestMineLevel, "netDeepestMineLevel").AddField(this.netTimesReachedMineBottom, "netTimesReachedMineBottom").AddField(this.netItemStowed, "netItemStowed").AddField(this.acceptedDailyQuest, "acceptedDailyQuest").AddField(this.lastSeenMovieWeek, "lastSeenMovieWeek").AddField(this.shirtItem, "shirtItem").AddField(this.pantsItem, "pantsItem").AddField(this.personalShippingBin, "personalShippingBin").AddField(this.viewingLocation, "viewingLocation").AddField(this.kissFarmerEvent, "kissFarmerEvent").AddField(this.haltAnimationEvent, "haltAnimationEvent").AddField(this.synchronizedJumpEvent, "synchronizedJumpEvent").AddField(this.tailoredItems, "tailoredItems").AddField(this.basicShipped, "basicShipped").AddField(this.mineralsFound, "mineralsFound").AddField(this.recipesCooked, "recipesCooked").AddField(this.archaeologyFound, "archaeologyFound").AddField(this.fishCaught, "fishCaught").AddField(this.biteChime, "biteChime").AddField(this._recoveredItem, "_recoveredItem").AddField(this.itemsLostLastDeath, "itemsLostLastDeath").AddField(this.renovateEvent, "renovateEvent").AddField(this.callsReceived, "callsReceived").AddField(this.onBridge, "onBridge").AddField(this.lastSleepLocation, "lastSleepLocation").AddField(this.lastSleepPoint, "lastSleepPoint").AddField(this.sleptInTemporaryBed, "sleptInTemporaryBed").AddField(this.timeWentToBed, "timeWentToBed").AddField(this.hasUsedDailyRevive, "hasUsedDailyRevive").AddField(this.jotpkProgress, "jotpkProgress").AddField(this.requestingTimePause, "requestingTimePause").AddField(this.isSitting, "isSitting").AddField(this.mapChairSitPosition, "mapChairSitPosition").AddField(this.netQiGems, "netQiGems").AddField(this.locationBeforeForcedEvent, "locationBeforeForcedEvent").AddField(this.hasCompletedAllMonsterSlayerQuests, "hasCompletedAllMonsterSlayerQuests").AddField(this.buffs.NetFields, "buffs.NetFields").AddField(this.trinketItems, "trinketItems").AddField(this.companions, "companions").AddField(this.prismaticHair, "prismaticHair").AddField(this.disconnectDay, "disconnectDay").AddField(this.disconnectLocation, "disconnectLocation").AddField(this.disconnectPosition, "disconnectPosition").AddField(this.tempFoodItemTextureName, "tempFoodItemTextureName").AddField(this.tempFoodItemSourceRect, "tempFoodItemSourceRect").AddField(this.toolHoldStartTime, "toolHoldStartTime").AddField(this.toolHold, "toolHold").AddField(this.toolPower, "toolPower").AddField(this.netCanReleaseTool, "netCanReleaseTool").AddField(this.lastGotPrizeFromGil, "lastGotPrizeFromGil").AddField(this.lastDesertFestivalFishingQuest, "lastDesertFestivalFishingQuest");
			this.fireToolEvent.onEvent += this.performFireTool;
			this.beginUsingToolEvent.onEvent += this.performBeginUsingTool;
			this.endUsingToolEvent.onEvent += this.performEndUsingTool;
			this.drinkAnimationEvent.onEvent += this.performDrinkAnimation;
			this.eatAnimationEvent.onEvent += this.performEatAnimation;
			this.sickAnimationEvent.onEvent += this.performSickAnimation;
			this.passOutEvent.onEvent += this.performPassOut;
			this.doEmoteEvent.onEvent += this.performPlayerEmote;
			this.kissFarmerEvent.onEvent += this.performKissFarmer;
			this.haltAnimationEvent.onEvent += this.performHaltAnimation;
			this.synchronizedJumpEvent.onEvent += this.performSynchronizedJump;
			this.renovateEvent.onEvent += this.performRenovation;
			this.netMount.fieldChangeEvent += delegate(NetRef<Horse> <p0>, Horse <p1>, Horse <p2>)
			{
				base.ClearCachedPosition();
			};
			this.shirtItem.fieldChangeVisibleEvent += delegate(NetRef<Clothing> <p0>, Clothing <p1>, Clothing <p2>)
			{
				this.UpdateClothing();
			};
			this.pantsItem.fieldChangeVisibleEvent += delegate(NetRef<Clothing> <p0>, Clothing <p1>, Clothing <p2>)
			{
				this.UpdateClothing();
			};
			this.trinketItems.OnArrayReplaced += this.OnTrinketArrayReplaced;
			this.trinketItems.OnElementChanged += this.OnTrinketChange;
			this.netItems.fieldChangeEvent += delegate(NetRef<Inventory> field, Inventory oldValue, Inventory newValue)
			{
				newValue.IsLocalPlayerInventory = this.IsLocalPlayer;
			};
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x000555A4 File Offset: 0x000537A4
		private void farmerInit()
		{
			this.buffs.SetOwner(this);
			this.FarmerRenderer = new FarmerRenderer("Characters\\Farmer\\farmer_" + (this.IsMale ? "" : "girl_") + "base", this);
			base.currentLocation = Game1.getLocationFromName(this.homeLocation.Value);
			this.Items.Clear();
			this.giftedItems = new SerializableDictionary<string, SerializableDictionary<string, int>>();
			this.LearnDefaultRecipes();
			this.songsHeard.Add("title_day");
			this.songsHeard.Add("title_night");
			this.changeShirt("1000");
			this.changeSkinColor(0, false);
			this.changeShoeColor("2");
			this.farmName.FilterStringEvent += Utility.FilterDirtyWords;
			this.name.FilterStringEvent += Utility.FilterDirtyWords;
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0005568C File Offset: 0x0005388C
		public virtual void OnWarp()
		{
			foreach (Companion companion in this.companions)
			{
				companion.OnOwnerWarp();
			}
			this.autoGenerateActiveDialogueEvent("firstVisit_" + base.currentLocation.Name, 4);
			if (!Stats.AllowRetroactiveAchievements)
			{
				string name = base.currentLocation.Name;
				if (name == "CommunityCenter" || name == "JojaMart")
				{
					Game1.stats.checkForCommunityCenterOrJojaAchievements(true);
					return;
				}
				if (!(name == "MasteryCave"))
				{
					return;
				}
				Game1.stats.checkForSkillAchievements(true);
				Game1.stats.checkForStardropAchievement(true);
			}
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x00055758 File Offset: 0x00053958
		public Trinket getFirstTrinketWithID(string id)
		{
			foreach (Trinket trinket in this.trinketItems)
			{
				if (trinket != null && trinket.ItemId == id)
				{
					return trinket;
				}
			}
			return null;
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x000557BC File Offset: 0x000539BC
		public bool hasTrinketWithID(string id)
		{
			foreach (Trinket trinket in this.trinketItems)
			{
				if (trinket != null && trinket.ItemId == id)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x00055820 File Offset: 0x00053A20
		public void resetAllTrinketEffects()
		{
			this.UnapplyAllTrinketEffects();
			this.ApplyAllTrinketEffects();
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x00055830 File Offset: 0x00053A30
		public virtual void ApplyAllTrinketEffects()
		{
			foreach (Trinket trinket in this.trinketItems)
			{
				if (trinket != null)
				{
					trinket.reloadSprite();
					trinket.Apply(this);
				}
			}
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0005588C File Offset: 0x00053A8C
		public virtual void UnapplyAllTrinketEffects()
		{
			foreach (Trinket trinket in this.trinketItems)
			{
				if (trinket != null)
				{
					trinket.Unapply(this);
				}
			}
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x000558E4 File Offset: 0x00053AE4
		public virtual void OnTrinketArrayReplaced(NetList<Trinket, NetRef<Trinket>> list, IList<Trinket> before, IList<Trinket> after)
		{
			if (Game1.gameMode != 0 && Utility.ShouldIgnoreValueChangeCallback())
			{
				return;
			}
			if (!this.IsLocalPlayer && !this.isFakeEventActor && Game1.gameMode != 0)
			{
				return;
			}
			foreach (Trinket trinket in before)
			{
				if (trinket != null)
				{
					trinket.Unapply(this);
				}
			}
			foreach (Trinket trinket2 in after)
			{
				if (trinket2 != null)
				{
					trinket2.Apply(this);
				}
			}
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x00055990 File Offset: 0x00053B90
		public virtual void OnTrinketChange(NetList<Trinket, NetRef<Trinket>> list, int index, Trinket old_value, Trinket new_value)
		{
			if (Game1.gameMode != 0 && Utility.ShouldIgnoreValueChangeCallback())
			{
				return;
			}
			if (!this.IsLocalPlayer && !this.isFakeEventActor && Game1.gameMode != 0)
			{
				return;
			}
			if (old_value != null)
			{
				old_value.Unapply(this);
			}
			if (new_value != null)
			{
				new_value.Apply(this);
			}
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x000559D0 File Offset: 0x00053BD0
		public bool CanEmote()
		{
			return Game1.farmEvent == null && (!Game1.eventUp || Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence || !this.IsLocalPlayer) && !this.usingSlingshot && !this.isEating && !this.UsingTool && (this.CanMove || !this.IsLocalPlayer) && !this.IsSitting() && !this.isRidingHorse() && !this.bathingClothes.Value;
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x00055A60 File Offset: 0x00053C60
		public void LearnDefaultRecipes()
		{
			foreach (KeyValuePair<string, string> recipe in CraftingRecipe.craftingRecipes)
			{
				if (!this.craftingRecipes.ContainsKey(recipe.Key) && ArgUtility.Get(recipe.Value.Split('/', StringSplitOptions.None), 4, null, true) == "default")
				{
					this.craftingRecipes.Add(recipe.Key, 0);
				}
			}
			foreach (KeyValuePair<string, string> recipe2 in CraftingRecipe.cookingRecipes)
			{
				if (!this.cookingRecipes.ContainsKey(recipe2.Key) && ArgUtility.Get(recipe2.Value.Split('/', StringSplitOptions.None), 3, null, true) == "default")
				{
					this.cookingRecipes.Add(recipe2.Key, 0);
				}
			}
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x00055B7C File Offset: 0x00053D7C
		public void AddMissedMailAndRecipes()
		{
			bool addRobinKitchenLetter = false;
			foreach (KeyValuePair<string, string> v in CraftingRecipe.cookingRecipes)
			{
				int reqSkillNumber;
				int minLevel;
				if (CraftingRecipe.TryParseLevelRequirement(v.Key, v.Value, true, out reqSkillNumber, out minLevel, true) && this.GetUnmodifiedSkillLevel(reqSkillNumber) >= minLevel)
				{
					this.cookingRecipes.TryAdd(v.Key, 0);
					addRobinKitchenLetter = true;
				}
			}
			foreach (KeyValuePair<string, string> v2 in CraftingRecipe.craftingRecipes)
			{
				int reqSkillNumber2;
				int minLevel2;
				if (CraftingRecipe.TryParseLevelRequirement(v2.Key, v2.Value, false, out reqSkillNumber2, out minLevel2, true) && this.GetUnmodifiedSkillLevel(reqSkillNumber2) >= minLevel2)
				{
					this.craftingRecipes.TryAdd(v2.Key, 0);
				}
			}
			if (addRobinKitchenLetter && !this.hasOrWillReceiveMail("robinKitchenLetter"))
			{
				this.team.RequestSetMail(PlayerActionTarget.All, "robinKitchenLetter", MailType.Now, true, new long?(this.uniqueMultiplayerID.Value));
			}
			if (this.farmingLevel.Value >= 10 && !this.hasOrWillReceiveMail("marnieAutoGrabber"))
			{
				this.team.RequestSetMail(PlayerActionTarget.All, "marnieAutoGrabber", MailType.Tomorrow, true, new long?(this.uniqueMultiplayerID.Value));
			}
			if (this.stats.Get("completedJunimoKart") > 0U && !this.hasOrWillReceiveMail("JunimoKart"))
			{
				this.team.RequestSetMail(PlayerActionTarget.All, "JunimoKart", MailType.Tomorrow, true, new long?(this.uniqueMultiplayerID.Value));
			}
			if (this.stats.Get("completedPrairieKing") > 0U && !this.hasOrWillReceiveMail("Beat_PK"))
			{
				this.team.RequestSetMail(PlayerActionTarget.All, "Beat_PK", MailType.Tomorrow, true, new long?(this.uniqueMultiplayerID.Value));
			}
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x00055D74 File Offset: 0x00053F74
		public void performRenovation(string location_name)
		{
			FarmHouse farmhouse = Game1.RequireLocation(location_name, false) as FarmHouse;
			if (farmhouse != null)
			{
				farmhouse.UpdateForRenovation();
			}
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00055D98 File Offset: 0x00053F98
		public void performPlayerEmote(string emote_string)
		{
			for (int i = 0; i < Farmer.EMOTES.Length; i++)
			{
				Farmer.EmoteType emote_type = Farmer.EMOTES[i];
				if (emote_type.emoteString == emote_string)
				{
					this.performedEmotes[emote_string] = true;
					if (emote_type.animationFrames != null)
					{
						if (!this.CanEmote())
						{
							return;
						}
						if (this.isEmoteAnimating)
						{
							this.EndEmoteAnimation();
						}
						else if (this.FarmerSprite.PauseForSingleAnimation)
						{
							return;
						}
						this.isEmoteAnimating = true;
						this._emoteGracePeriod = 200;
						if (this == Game1.player)
						{
							this.noMovementPause = Math.Max(this.noMovementPause, 200);
						}
						this.emoteFacingDirection = emote_type.facingDirection;
						this.FarmerSprite.animateOnce(emote_type.animationFrames, new AnimatedSprite.endOfAnimationBehavior(this.OnEmoteAnimationEnd));
					}
					if (emote_type.emoteIconIndex >= 0)
					{
						this.isEmoting = false;
						base.doEmote(emote_type.emoteIconIndex, false);
					}
				}
			}
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00055E8B File Offset: 0x0005408B
		public bool ShouldHandleAnimationSound()
		{
			return !LocalMultiplayer.IsLocalMultiplayer(true) || this.IsLocalPlayer;
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x00055EA4 File Offset: 0x000540A4
		public static List<Item> initialTools()
		{
			return new List<Item>
			{
				ItemRegistry.Create("(T)Axe", 1, 0, false),
				ItemRegistry.Create("(T)Hoe", 1, 0, false),
				ItemRegistry.Create("(T)WateringCan", 1, 0, false),
				ItemRegistry.Create("(T)Pickaxe", 1, 0, false),
				ItemRegistry.Create("(W)47", 1, 0, false)
			};
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x00055F18 File Offset: 0x00054118
		private void playHarpEmoteSound()
		{
			int[] notes = new int[]
			{
				1200,
				1600,
				1900,
				2400
			};
			switch (Game1.random.Next(5))
			{
			case 0:
				notes = new int[]
				{
					1200,
					1600,
					1900,
					2400
				};
				break;
			case 1:
				notes = new int[]
				{
					1200,
					1700,
					2100,
					2400
				};
				break;
			case 2:
				notes = new int[]
				{
					1100,
					1400,
					1900,
					2300
				};
				break;
			case 3:
				notes = new int[]
				{
					1600,
					1900,
					2400
				};
				break;
			case 4:
				notes = new int[]
				{
					700,
					1200,
					1900
				};
				break;
			}
			if (this.IsLocalPlayer)
			{
				if (Game1.IsMultiplayer && this.UniqueMultiplayerID % 111L == 0L)
				{
					notes = new int[]
					{
						800 + Game1.random.Next(4) * 100,
						1200 + Game1.random.Next(4) * 100,
						1600 + Game1.random.Next(4) * 100,
						2000 + Game1.random.Next(4) * 100
					};
					for (int i = 0; i < notes.Length; i++)
					{
						DelayedAction.playSoundAfterDelay("miniharp_note", Game1.random.Next(60, 150) * i, base.currentLocation, new Vector2?(base.Tile), notes[i], false);
						if (i > 1 && Game1.random.NextDouble() < 0.25)
						{
							return;
						}
					}
					return;
				}
				for (int j = 0; j < notes.Length; j++)
				{
					DelayedAction.playSoundAfterDelay("miniharp_note", (j > 0) ? (150 + Game1.random.Next(35, 51) * j) : 0, base.currentLocation, new Vector2?(base.Tile), notes[j], false);
				}
			}
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x000560F0 File Offset: 0x000542F0
		private static void removeLowestUpgradeLevelTool(List<Item> items, Type toolType)
		{
			Tool lowestItem = null;
			foreach (Item item in items)
			{
				Tool tool = item as Tool;
				if (tool != null && tool.GetType() == toolType && (lowestItem == null || tool.upgradeLevel.Value < lowestItem.upgradeLevel.Value))
				{
					lowestItem = tool;
				}
			}
			if (lowestItem != null)
			{
				items.Remove(lowestItem);
			}
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x00056178 File Offset: 0x00054378
		public static void removeInitialTools(List<Item> items)
		{
			Farmer.removeLowestUpgradeLevelTool(items, typeof(Axe));
			Farmer.removeLowestUpgradeLevelTool(items, typeof(Hoe));
			Farmer.removeLowestUpgradeLevelTool(items, typeof(WateringCan));
			Farmer.removeLowestUpgradeLevelTool(items, typeof(Pickaxe));
			Item scythe = items.FirstOrDefault((Item item) => item is MeleeWeapon && item.ItemId == "47");
			if (scythe != null)
			{
				items.Remove(scythe);
			}
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x000561F8 File Offset: 0x000543F8
		public Point getMailboxPosition()
		{
			foreach (Building b in Game1.getFarm().buildings)
			{
				if (b.isCabin && b.HasIndoorsName(this.homeLocation.Value))
				{
					return b.getMailboxPosition();
				}
			}
			return Game1.getFarm().GetMainMailboxPosition();
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x00056278 File Offset: 0x00054478
		public void ClearBuffs()
		{
			this.buffs.Clear();
			base.stopGlowing();
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x0005628B File Offset: 0x0005448B
		public bool isActive()
		{
			return this == Game1.player || Game1.otherFarmers.ContainsKey(this.UniqueMultiplayerID);
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x000562A7 File Offset: 0x000544A7
		public string getTexture()
		{
			return "Characters\\Farmer\\farmer_" + (this.IsMale ? "" : "girl_") + "base" + (this.isBald() ? "_bald" : "");
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x000562E0 File Offset: 0x000544E0
		public void unload()
		{
			FarmerRenderer farmerRenderer = this.FarmerRenderer;
			if (farmerRenderer == null)
			{
				return;
			}
			farmerRenderer.unload();
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x000562F4 File Offset: 0x000544F4
		public void setInventory(List<Item> newInventory)
		{
			this.Items.OverwriteWith(newInventory);
			for (int i = this.Items.Count; i < this.maxItems.Value; i++)
			{
				this.Items.Add(null);
			}
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x0005633C File Offset: 0x0005453C
		public void makeThisTheActiveObject(Object o)
		{
			if (this.freeSpotsInInventory() > 0)
			{
				Item i = this.CurrentItem;
				this.ActiveObject = o;
				this.addItemToInventory(i);
			}
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x00056368 File Offset: 0x00054568
		public int getNumberOfChildren()
		{
			return this.getChildrenCount();
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x00056370 File Offset: 0x00054570
		private void setMount(Horse mount)
		{
			if (mount != null)
			{
				this.netMount.Value = mount;
				this.xOffset = -11f;
				base.Position = Utility.PointToVector2(mount.GetBoundingBox().Location);
				this.position.Y -= 16f;
				this.position.X -= 8f;
				base.speed = 2;
				this.showNotCarrying();
				return;
			}
			this.netMount.Value = null;
			this.collisionNPC = null;
			this.running = false;
			base.speed = ((Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton) && !Game1.options.autoRun) ? 5 : 2);
			bool isRunning = base.speed == 5;
			this.running = isRunning;
			if (this.running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
			this.completelyStopAnimatingOrDoingAction();
			this.xOffset = 0f;
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x0005646C File Offset: 0x0005466C
		public bool isRidingHorse()
		{
			return this.mount != null && !Game1.eventUp;
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x00056480 File Offset: 0x00054680
		public List<Child> getChildren()
		{
			return Utility.getHomeOfFarmer(this).getChildren();
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x0005648D File Offset: 0x0005468D
		public int getChildrenCount()
		{
			return Utility.getHomeOfFarmer(this).getChildrenCount();
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x0005649C File Offset: 0x0005469C
		public Tool getToolFromName(string name)
		{
			foreach (Item item in this.Items)
			{
				Tool tool = item as Tool;
				if (tool != null && tool.Name.Contains(name))
				{
					return tool;
				}
			}
			return null;
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00056500 File Offset: 0x00054700
		public override void SetMovingDown(bool b)
		{
			this.setMoving((byte)(4 + (b ? 0 : 32)));
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x00056513 File Offset: 0x00054713
		public override void SetMovingRight(bool b)
		{
			this.setMoving((byte)(2 + (b ? 0 : 32)));
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x00056526 File Offset: 0x00054726
		public override void SetMovingUp(bool b)
		{
			this.setMoving((byte)(1 + (b ? 0 : 32)));
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x00056539 File Offset: 0x00054739
		public override void SetMovingLeft(bool b)
		{
			this.setMoving((byte)(8 + (b ? 0 : 32)));
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0005654C File Offset: 0x0005474C
		public int? tryGetFriendshipLevelForNPC(string name)
		{
			Friendship friendship;
			if (this.friendshipData.TryGetValue(name, out friendship))
			{
				return new int?(friendship.Points);
			}
			return null;
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x00056580 File Offset: 0x00054780
		public int getFriendshipLevelForNPC(string name)
		{
			Friendship friendship;
			if (this.friendshipData.TryGetValue(name, out friendship))
			{
				return friendship.Points;
			}
			return 0;
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x000565A5 File Offset: 0x000547A5
		public int getFriendshipHeartLevelForNPC(string name)
		{
			return this.getFriendshipLevelForNPC(name) / 250;
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x000565B4 File Offset: 0x000547B4
		public bool isRoommate(string name)
		{
			Friendship friendship;
			return name != null && this.friendshipData.TryGetValue(name, out friendship) && friendship.IsRoommate();
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x000565DC File Offset: 0x000547DC
		public bool hasCurrentOrPendingRoommate()
		{
			Friendship friendship;
			return this.spouse != null && this.friendshipData.TryGetValue(this.spouse, out friendship) && friendship.RoommateMarriage;
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x0005660E File Offset: 0x0005480E
		public bool hasRoommate()
		{
			return this.isRoommate(this.spouse);
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x0005661C File Offset: 0x0005481C
		public bool hasAFriendWithFriendshipPoints(int minPoints, bool datablesOnly, int maxPoints = 2147483647)
		{
			bool found = false;
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (!datablesOnly || n.datable.Value)
				{
					int points = this.getFriendshipLevelForNPC(n.Name);
					if (points >= minPoints && points <= maxPoints)
					{
						found = true;
					}
				}
				return !found;
			}, false);
			return found;
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x00056668 File Offset: 0x00054868
		public bool hasAFriendWithHeartLevel(int minHeartLevel, bool datablesOnly, int maxHeartLevel = 2147483647)
		{
			int minPoints = minHeartLevel * 250;
			int maxPoints = maxHeartLevel * 250;
			if (maxPoints < maxHeartLevel)
			{
				maxPoints = int.MaxValue;
			}
			return this.hasAFriendWithFriendshipPoints(minPoints, datablesOnly, maxPoints);
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x00056698 File Offset: 0x00054898
		public void shippedBasic(string itemId, int number)
		{
			int curValue;
			if (!this.basicShipped.TryGetValue(itemId, out curValue))
			{
				curValue = 0;
			}
			this.basicShipped[itemId] = curValue + number;
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x000566C8 File Offset: 0x000548C8
		public void shiftToolbar(bool right)
		{
			if (this.Items == null || this.Items.Count < 12)
			{
				return;
			}
			if (this.UsingTool || Game1.dialogueUp || !this.CanMove || !this.Items.HasAny() || Game1.eventUp || Game1.farmEvent != null)
			{
				return;
			}
			Game1.playSound("shwip", null);
			Item currentItem = this.CurrentItem;
			if (currentItem != null)
			{
				currentItem.actionWhenStopBeingHeld(this);
			}
			if (right)
			{
				IList<Item> toMove = this.Items.GetRange(0, 12);
				this.Items.RemoveRange(0, 12);
				this.Items.AddRange(toMove);
			}
			else
			{
				IList<Item> toMove2 = this.Items.GetRange(this.Items.Count - 12, 12);
				for (int i = 0; i < this.Items.Count - 12; i++)
				{
					toMove2.Add(this.Items[i]);
				}
				this.Items.OverwriteWith(toMove2);
			}
			this.netItemStowed.Set(false);
			Item currentItem2 = this.CurrentItem;
			if (currentItem2 != null)
			{
				currentItem2.actionWhenBeingHeld(this);
			}
			for (int j = 0; j < Game1.onScreenMenus.Count; j++)
			{
				Toolbar toolbar = Game1.onScreenMenus[j] as Toolbar;
				if (toolbar != null)
				{
					toolbar.shifted(right);
					return;
				}
			}
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x00056820 File Offset: 0x00054A20
		public void foundWalnut(int stack = 1)
		{
			if (Game1.netWorldState.Value.GoldenWalnutsFound >= 130)
			{
				return;
			}
			Game1.netWorldState.Value.GoldenWalnuts += stack;
			Game1.netWorldState.Value.GoldenWalnutsFound += stack;
			Game1.PerformActionWhenPlayerFree(new Action(this.showNutPickup));
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x00056884 File Offset: 0x00054A84
		public virtual void RemoveMail(string mail_key, bool from_broadcast_list = false)
		{
			mail_key = mail_key.Replace("%&NL&%", "");
			this.mailReceived.Remove(mail_key);
			this.mailbox.Remove(mail_key);
			this.mailForTomorrow.Remove(mail_key);
			this.mailForTomorrow.Remove(mail_key + "%&NL&%");
			if (from_broadcast_list)
			{
				this.team.broadcastedMail.Remove("%&SM&%" + mail_key);
				this.team.broadcastedMail.Remove("%&MFT&%" + mail_key);
				this.team.broadcastedMail.Remove("%&MB&%" + mail_key);
			}
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x00056938 File Offset: 0x00054B38
		public virtual void showNutPickup()
		{
			if (!this.hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
			{
				Game1.addMailForTomorrow("lostWalnutFound", true, false);
				this.completelyStopAnimatingOrDoingAction();
				this.holdUpItemThenMessage(ItemRegistry.Create("(O)73", 1, 0, false), true);
				return;
			}
			if (this.hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
			{
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 240, 16, 16), 100f, 4, 2, new Vector2(0f, -96f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(0f, -6f),
					acceleration = new Vector2(0f, 0.2f),
					stopAcceleratingWhenVelocityIsZero = true,
					attachedCharacter = this,
					positionFollowsAttachedCharacter = true
				});
			}
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x00056A40 File Offset: 0x00054C40
		public void foundArtifact(string itemId, int number)
		{
			bool shouldHoldUpArtifact = false;
			if (itemId == "102")
			{
				if (!this.hasOrWillReceiveMail("lostBookFound"))
				{
					Game1.addMailForTomorrow("lostBookFound", true, false);
					shouldHoldUpArtifact = true;
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14100"));
				}
				Game1.playSound("newRecipe", null);
				Game1.netWorldState.Value.LostBooksFound++;
				Game1.multiplayer.globalChatInfoMessage("LostBook", new string[]
				{
					this.displayName
				});
			}
			int[] artifactEntry;
			if (this.archaeologyFound.TryGetValue(itemId, out artifactEntry))
			{
				artifactEntry[0] += number;
				artifactEntry[1] += number;
				this.archaeologyFound[itemId] = artifactEntry;
			}
			else
			{
				if (this.archaeologyFound.Length == 0)
				{
					if (!this.eventsSeen.Contains("0") && itemId != "102")
					{
						this.addQuest("23");
					}
					this.mailReceived.Add("artifactFound");
					shouldHoldUpArtifact = true;
				}
				this.archaeologyFound.Add(itemId, new int[]
				{
					number,
					number
				});
			}
			if (shouldHoldUpArtifact)
			{
				this.holdUpItemThenMessage(ItemRegistry.Create("(O)" + itemId, 1, 0, false), true);
			}
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x00056B90 File Offset: 0x00054D90
		public void cookedRecipe(string itemId)
		{
			int curValue;
			if (!this.recipesCooked.TryGetValue(itemId, out curValue))
			{
				curValue = 0;
			}
			this.recipesCooked[itemId] = curValue + 1;
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x00056BC0 File Offset: 0x00054DC0
		public bool caughtFish(string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
		{
			ItemMetadata itemData = ItemRegistry.GetMetadata(itemId);
			itemId = itemData.QualifiedItemId;
			bool flag;
			if (!from_fish_pond && itemData.Exists() && !ItemContextTagManager.HasBaseTag(itemData.QualifiedItemId, "trash_item") && !(itemId == "(O)167"))
			{
				ParsedItemData parsedData = itemData.GetParsedData();
				flag = (((parsedData != null) ? parsedData.ObjectType : null) == "Fish" || itemData.QualifiedItemId == "(O)372");
			}
			else
			{
				flag = false;
			}
			bool sizeRecord = false;
			if (flag)
			{
				int[] fishEntry;
				if (this.fishCaught.TryGetValue(itemId, out fishEntry))
				{
					fishEntry[0] += numberCaught;
					Game1.stats.checkForFishingAchievements();
					if (size > this.fishCaught[itemId][1])
					{
						fishEntry[1] = size;
						sizeRecord = true;
					}
					this.fishCaught[itemId] = fishEntry;
				}
				else
				{
					this.fishCaught.Add(itemId, new int[]
					{
						numberCaught,
						size
					});
					Game1.stats.checkForFishingAchievements();
					this.autoGenerateActiveDialogueEvent("fishCaught_" + itemData.LocalItemId, 4);
				}
				this.NotifyQuests((Quest quest) => quest.OnFishCaught(itemId, numberCaught, size, false), false);
				if (Utility.GetDayOfPassiveFestival("SquidFest") > 0 && itemId == "(O)151")
				{
					Game1.stats.Increment(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year), numberCaught);
				}
			}
			return sizeRecord;
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x00056D74 File Offset: 0x00054F74
		public virtual void gainExperience(int which, int howMuch)
		{
			if (which == 5 || howMuch <= 0)
			{
				return;
			}
			if (!this.IsLocalPlayer && Game1.IsServer)
			{
				this.queueMessage(17, Game1.player, new object[]
				{
					which,
					howMuch
				});
				return;
			}
			if (this.Level >= 25)
			{
				int old = MasteryTrackerMenu.getCurrentMasteryLevel();
				Game1.stats.Increment("MasteryExp", Math.Max(1, (which == 0) ? (howMuch / 2) : howMuch));
				if (MasteryTrackerMenu.getCurrentMasteryLevel() > old)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
					Game1.playSound("newArtifact", null);
				}
			}
			int newLevel = Farmer.checkForLevelGain(this.experiencePoints[which], this.experiencePoints[which] + howMuch);
			NetArray<int, NetInt> netArray = this.experiencePoints;
			netArray[which] += howMuch;
			int oldLevel = -1;
			if (newLevel != -1)
			{
				switch (which)
				{
				case 0:
					oldLevel = this.farmingLevel.Value;
					this.farmingLevel.Value = newLevel;
					break;
				case 1:
					oldLevel = this.fishingLevel.Value;
					this.fishingLevel.Value = newLevel;
					break;
				case 2:
					oldLevel = this.foragingLevel.Value;
					this.foragingLevel.Value = newLevel;
					break;
				case 3:
					oldLevel = this.miningLevel.Value;
					this.miningLevel.Value = newLevel;
					break;
				case 4:
					oldLevel = this.combatLevel.Value;
					this.combatLevel.Value = newLevel;
					break;
				case 5:
					oldLevel = this.luckLevel.Value;
					this.luckLevel.Value = newLevel;
					break;
				}
			}
			if (newLevel > oldLevel)
			{
				for (int i = oldLevel + 1; i <= newLevel; i++)
				{
					this.newLevels.Add(new Point(which, i));
					if (this.newLevels.Count == 1)
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:NewIdeas"));
					}
				}
			}
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x00056F74 File Offset: 0x00055174
		public int getEffectiveSkillLevel(int whichSkill)
		{
			if (whichSkill < 0 || whichSkill > 5)
			{
				return -1;
			}
			int[] effectiveSkillLevels = new int[]
			{
				this.farmingLevel.Value,
				this.fishingLevel.Value,
				this.foragingLevel.Value,
				this.miningLevel.Value,
				this.combatLevel.Value,
				this.luckLevel.Value
			};
			for (int i = 0; i < this.newLevels.Count; i++)
			{
				effectiveSkillLevels[this.newLevels[i].X]--;
			}
			return effectiveSkillLevels[whichSkill];
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x0005701C File Offset: 0x0005521C
		public static int checkForLevelGain(int oldXP, int newXP)
		{
			for (int level = 10; level >= 1; level--)
			{
				if (oldXP < Farmer.getBaseExperienceForLevel(level) && newXP >= Farmer.getBaseExperienceForLevel(level))
				{
					return level;
				}
			}
			return -1;
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x0005704C File Offset: 0x0005524C
		public static int getBaseExperienceForLevel(int level)
		{
			switch (level)
			{
			case 1:
				return 100;
			case 2:
				return 380;
			case 3:
				return 770;
			case 4:
				return 1300;
			case 5:
				return 2150;
			case 6:
				return 3300;
			case 7:
				return 4800;
			case 8:
				return 6900;
			case 9:
				return 10000;
			case 10:
				return 15000;
			default:
				return -1;
			}
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x000570C8 File Offset: 0x000552C8
		public void revealGiftTaste(string npcName, string itemId)
		{
			if (npcName == null)
			{
				return;
			}
			SerializableDictionary<string, int> giftData;
			if (!this.giftedItems.TryGetValue(npcName, out giftData))
			{
				giftData = (this.giftedItems[npcName] = new SerializableDictionary<string, int>());
			}
			giftData.TryAdd(itemId, 0);
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x00057108 File Offset: 0x00055308
		public void onGiftGiven(NPC npc, Object item)
		{
			if (item.bigCraftable.Value)
			{
				return;
			}
			SerializableDictionary<string, int> giftData;
			if (!this.giftedItems.TryGetValue(npc.name.Value, out giftData))
			{
				giftData = (this.giftedItems[npc.name.Value] = new SerializableDictionary<string, int>());
			}
			int curValue = giftData.GetValueOrDefault(item.ItemId);
			giftData[item.ItemId] = curValue + 1;
			if (this.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in this.team.specialOrders)
				{
					Action<Farmer, NPC, Item> onGiftGiven = specialOrder.onGiftGiven;
					if (onGiftGiven != null)
					{
						onGiftGiven(this, npc, item);
					}
				}
			}
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x000571DC File Offset: 0x000553DC
		public bool hasGiftTasteBeenRevealed(NPC npc, string itemId)
		{
			SerializableDictionary<string, int> giftData;
			return this.hasItemBeenGifted(npc, itemId) || (this.giftedItems.TryGetValue(npc.name.Value, out giftData) && giftData.ContainsKey(itemId));
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x00057218 File Offset: 0x00055418
		public bool hasItemBeenGifted(NPC npc, string itemId)
		{
			SerializableDictionary<string, int> giftData;
			int value;
			return this.giftedItems.TryGetValue(npc.name.Value, out giftData) && giftData.TryGetValue(itemId, out value) && value > 0;
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x00057254 File Offset: 0x00055454
		public void MarkItemAsTailored(Item item)
		{
			if (item == null)
			{
				return;
			}
			string item_key = Utility.getStandardDescriptionFromItem(item, 1, ' ');
			int curValue;
			if (!this.tailoredItems.TryGetValue(item_key, out curValue))
			{
				curValue = 0;
			}
			this.tailoredItems[item_key] = curValue + 1;
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x00057290 File Offset: 0x00055490
		public bool HasTailoredThisItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			string item_key = Utility.getStandardDescriptionFromItem(item, 1, ' ');
			return this.tailoredItems.ContainsKey(item_key);
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x000572B8 File Offset: 0x000554B8
		public void foundMineral(string itemId)
		{
			int curValue;
			if (!this.mineralsFound.TryGetValue(itemId, out curValue))
			{
				curValue = 0;
			}
			this.mineralsFound[itemId] = curValue + 1;
			if (!this.hasOrWillReceiveMail("artifactFound"))
			{
				this.mailReceived.Add("artifactFound");
			}
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x00057304 File Offset: 0x00055504
		public void increaseBackpackSize(int howMuch)
		{
			this.MaxItems += howMuch;
			while (this.Items.Count < this.MaxItems)
			{
				this.Items.Add(null);
			}
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x00057338 File Offset: 0x00055538
		// (set) Token: 0x060008A4 RID: 2212 RVA: 0x00057396 File Offset: 0x00055596
		public override int FacingDirection
		{
			get
			{
				if (!this.IsLocalPlayer && !this.isFakeEventActor && this.UsingTool)
				{
					FishingRod rod = this.CurrentTool as FishingRod;
					if (rod != null && rod.CastDirection >= 0)
					{
						return rod.CastDirection;
					}
				}
				if (this.isEmoteAnimating)
				{
					return this.emoteFacingDirection;
				}
				return this.facingDirection.Value;
			}
			set
			{
				this.facingDirection.Set(value);
			}
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x000573A4 File Offset: 0x000555A4
		[Obsolete("Most code should use Items.CountId instead. However this method works a bit differently in that the item ID can be 858 (Qi Gems), 73 (Golden Walnuts), a category number, or -777 to match seasonal wild seeds.")]
		public int getItemCount(string itemId)
		{
			return this.getItemCountInList(this.Items, itemId);
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x000573B4 File Offset: 0x000555B4
		[Obsolete("Most code should use Items.CountId instead. However this method works a bit differently in that the item ID can be a category number, or -777 to match seasonal wild seeds.")]
		public int getItemCountInList(IList<Item> list, string itemId)
		{
			int number_found = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && CraftingRecipe.ItemMatchesForCrafting(list[i], itemId))
				{
					number_found += list[i].Stack;
				}
			}
			return number_found;
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x000573FC File Offset: 0x000555FC
		public int LoseItemsOnDeath(Random random = null)
		{
			if (random == null)
			{
				random = Utility.CreateDaySaveRandom((double)Game1.timeOfDay, 0.0, 0.0);
			}
			double itemLossRate = 0.22 - (double)this.LuckLevel * 0.04 - this.DailyLuck;
			int numberOfItemsLost = 0;
			this.itemsLostLastDeath.Clear();
			for (int i = this.Items.Count - 1; i >= 0; i--)
			{
				Item item = this.Items[i];
				if (item != null && item.CanBeLostOnDeath() && random.NextBool(itemLossRate))
				{
					numberOfItemsLost++;
					this.Items[i] = null;
					this.itemsLostLastDeath.Add(item);
					if (numberOfItemsLost == 3)
					{
						break;
					}
				}
			}
			return numberOfItemsLost;
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x000574B8 File Offset: 0x000556B8
		public void ShowSitting()
		{
			if (!this.IsSitting())
			{
				return;
			}
			if (this.sittingFurniture != null)
			{
				this.FacingDirection = this.sittingFurniture.GetSittingDirection();
			}
			if (this.yJumpOffset != 0)
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.FarmerSprite.setCurrentSingleFrame(12, 32000, false, false);
					return;
				case 1:
					this.FarmerSprite.setCurrentSingleFrame(6, 32000, false, false);
					return;
				case 2:
					this.FarmerSprite.setCurrentSingleFrame(0, 32000, false, false);
					return;
				case 3:
					this.FarmerSprite.setCurrentSingleFrame(6, 32000, false, true);
					return;
				default:
					return;
				}
			}
			else
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.FarmerSprite.setCurrentSingleFrame(113, 32000, false, false);
					this.xOffset = 0f;
					this.yOffset = -40f;
					return;
				case 1:
					this.FarmerSprite.setCurrentSingleFrame(117, 32000, false, false);
					this.xOffset = -4f;
					this.yOffset = -32f;
					return;
				case 2:
					this.FarmerSprite.setCurrentSingleFrame(107, 32000, true, false);
					this.xOffset = 0f;
					this.yOffset = -48f;
					return;
				case 3:
					this.FarmerSprite.setCurrentSingleFrame(117, 32000, false, true);
					this.xOffset = 4f;
					this.yOffset = -32f;
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x00057628 File Offset: 0x00055828
		public void showRiding()
		{
			if (!this.isRidingHorse())
			{
				return;
			}
			this.xOffset = -6f;
			switch (this.FacingDirection)
			{
			case 0:
				this.FarmerSprite.setCurrentSingleFrame(113, 32000, false, false);
				break;
			case 1:
				this.FarmerSprite.setCurrentSingleFrame(106, 32000, false, false);
				this.xOffset += 2f;
				break;
			case 2:
				this.FarmerSprite.setCurrentSingleFrame(107, 32000, false, false);
				break;
			case 3:
				this.FarmerSprite.setCurrentSingleFrame(106, 32000, false, true);
				this.xOffset = -12f;
				break;
			}
			if (!this.isMoving())
			{
				this.yOffset = 0f;
				return;
			}
			switch (this.mount.Sprite.currentAnimationIndex)
			{
			case 0:
				this.yOffset = 0f;
				return;
			case 1:
				this.yOffset = -4f;
				return;
			case 2:
				this.yOffset = -4f;
				return;
			case 3:
				this.yOffset = 0f;
				return;
			case 4:
				this.yOffset = 4f;
				return;
			case 5:
				this.yOffset = 4f;
				return;
			default:
				return;
			}
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x00057768 File Offset: 0x00055968
		public void showCarrying()
		{
			if (Game1.eventUp || this.isRidingHorse() || Game1.killScreen || this.IsSitting())
			{
				return;
			}
			if (this.bathingClothes.Value || this.onBridge.Value)
			{
				this.showNotCarrying();
				return;
			}
			if (!this.FarmerSprite.PauseForSingleAnimation && !this.isMoving())
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.FarmerSprite.setCurrentFrame(144);
					break;
				case 1:
					this.FarmerSprite.setCurrentFrame(136);
					break;
				case 2:
					this.FarmerSprite.setCurrentFrame(128);
					break;
				case 3:
					this.FarmerSprite.setCurrentFrame(152);
					break;
				}
			}
			if (this.ActiveObject != null)
			{
				this.mostRecentlyGrabbedItem = this.ActiveObject;
			}
			if (this.IsLocalPlayer)
			{
				Item item = this.mostRecentlyGrabbedItem;
				if (((item != null) ? item.QualifiedItemId : null) == "(O)434")
				{
					this.eatHeldObject();
				}
			}
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x00057870 File Offset: 0x00055A70
		public void showNotCarrying()
		{
			if (!this.FarmerSprite.PauseForSingleAnimation && !this.isMoving())
			{
				bool canOnlyWalk = this.canOnlyWalk || this.bathingClothes.Value || this.onBridge.Value;
				switch (this.FacingDirection)
				{
				case 0:
					this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 16 : 48, (canOnlyWalk > false) ? 1 : 0);
					return;
				case 1:
					this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 8 : 40, (canOnlyWalk > false) ? 1 : 0);
					return;
				case 2:
					this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 0 : 32, (canOnlyWalk > false) ? 1 : 0);
					return;
				case 3:
					this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 24 : 56, (canOnlyWalk > false) ? 1 : 0);
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x0005793B File Offset: 0x00055B3B
		public int GetDaysMarried()
		{
			Friendship spouseFriendship = this.GetSpouseFriendship();
			if (spouseFriendship == null)
			{
				return 0;
			}
			return spouseFriendship.DaysMarried;
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x00057950 File Offset: 0x00055B50
		public Friendship GetSpouseFriendship()
		{
			long? farmerSpouseId = this.team.GetSpouse(this.UniqueMultiplayerID);
			if (farmerSpouseId != null)
			{
				long spouseID = farmerSpouseId.Value;
				return this.team.GetFriendship(this.UniqueMultiplayerID, spouseID);
			}
			Friendship friendship;
			if (string.IsNullOrEmpty(this.spouse) || !this.friendshipData.TryGetValue(this.spouse, out friendship))
			{
				return null;
			}
			return friendship;
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x000579B8 File Offset: 0x00055BB8
		public bool hasDailyQuest()
		{
			for (int i = this.questLog.Count - 1; i >= 0; i--)
			{
				if (this.questLog[i].dailyQuest.Value)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x000579F8 File Offset: 0x00055BF8
		public void showToolUpgradeAvailability()
		{
			int day = Game1.dayOfMonth;
			if (this.toolBeingUpgraded != null && this.daysLeftForToolUpgrade.Value <= 0 && this.toolBeingUpgraded.Value != null && !Utility.isFestivalDay() && (Game1.shortDayNameFromDayOfSeason(day) != "Fri" || !this.hasCompletedCommunityCenter() || Game1.isRaining) && !this.hasReceivedToolUpgradeMessageYet)
			{
				if (Game1.newDay)
				{
					Game1.morningQueue.Enqueue(delegate
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", this.toolBeingUpgraded.Value.DisplayName));
					});
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", this.toolBeingUpgraded.Value.DisplayName));
				}
				this.hasReceivedToolUpgradeMessageYet = true;
			}
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x00057AB4 File Offset: 0x00055CB4
		public void dayupdate(int timeWentToSleep)
		{
			if (this.IsSitting())
			{
				this.StopSitting(false);
			}
			this.resetFriendshipsForNewDay();
			this.LearnDefaultRecipes();
			this.hasUsedDailyRevive.Value = false;
			this.hasBeenBlessedByStatueToday = false;
			this.acceptedDailyQuest.Set(false);
			this.dancePartner.Value = null;
			this.festivalScore = 0;
			this.forceTimePass = false;
			if (this.daysLeftForToolUpgrade.Value > 0)
			{
				NetInt netInt = this.daysLeftForToolUpgrade;
				int num = netInt.Value;
				netInt.Value = num - 1;
			}
			if (this.daysUntilHouseUpgrade.Value > 0)
			{
				NetInt netInt2 = this.daysUntilHouseUpgrade;
				int num = netInt2.Value;
				netInt2.Value = num - 1;
				if (this.daysUntilHouseUpgrade.Value <= 0)
				{
					FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(this);
					homeOfFarmer.moveObjectsForHouseUpgrade(this.houseUpgradeLevel.Value + 1);
					NetInt netInt3 = this.houseUpgradeLevel;
					num = netInt3.Value;
					netInt3.Value = num + 1;
					this.daysUntilHouseUpgrade.Value = -1;
					homeOfFarmer.setMapForUpgradeLevel(this.houseUpgradeLevel.Value);
					Game1.stats.checkForBuildingUpgradeAchievements();
					this.autoGenerateActiveDialogueEvent("houseUpgrade_" + this.houseUpgradeLevel.Value.ToString(), 4);
				}
			}
			this.questLog.RemoveWhere(delegate(Quest quest)
			{
				if (quest.IsTimedQuest())
				{
					NetInt daysLeft = quest.daysLeft;
					int value = daysLeft.Value;
					daysLeft.Value = value - 1;
					return quest.daysLeft.Value <= 0 && !quest.completed.Value;
				}
				return false;
			});
			this.ClearBuffs();
			if (this.MaxStamina >= 508)
			{
				this.mailReceived.Add("gotMaxStamina");
			}
			float oldStamina = this.Stamina;
			this.Stamina = (float)this.MaxStamina;
			bool wasExhausted = this.exhausted.Value;
			if (wasExhausted)
			{
				this.exhausted.Value = false;
				this.Stamina = (float)(this.MaxStamina / 2 + 1);
			}
			int bedTime = (this.timeWentToBed.Value == 0) ? timeWentToSleep : this.timeWentToBed.Value;
			if (bedTime > 2400)
			{
				float staminaRestorationReduction = (1f - (float)(2600 - Math.Min(2600, bedTime)) / 200f) * (float)(this.MaxStamina / 2);
				this.Stamina -= staminaRestorationReduction;
				if (timeWentToSleep > 2700)
				{
					this.Stamina /= 2f;
				}
			}
			if (timeWentToSleep < 2700 && oldStamina > this.Stamina && !wasExhausted)
			{
				this.Stamina = oldStamina;
			}
			this.health = this.maxHealth;
			this.activeDialogueEvents.RemoveWhere(delegate(KeyValuePair<string, int> pair)
			{
				string key2 = pair.Key;
				if (!key2.Contains("_memory_"))
				{
					this.previousActiveDialogueEvents.TryAdd(key2, 0);
				}
				NetStringDictionary<int, NetInt> netStringDictionary2 = this.activeDialogueEvents;
				string key3 = key2;
				int num2 = netStringDictionary2[key3];
				netStringDictionary2[key3] = num2 - 1;
				if (this.activeDialogueEvents[key2] < 0)
				{
					if (!(key2 == "pennyRedecorating") || Utility.getHomeOfFarmer(this).GetSpouseBed() != null)
					{
						return true;
					}
					this.activeDialogueEvents[key2] = 0;
				}
				return false;
			});
			foreach (string previousEvent in this.previousActiveDialogueEvents.Keys)
			{
				NetStringDictionary<int, NetInt> netStringDictionary = this.previousActiveDialogueEvents;
				string key = previousEvent;
				int num = netStringDictionary[key];
				netStringDictionary[key] = num + 1;
				if (this.previousActiveDialogueEvents[previousEvent] == 1)
				{
					this.activeDialogueEvents[previousEvent + "_memory_oneday"] = 4;
				}
				if (this.previousActiveDialogueEvents[previousEvent] == 7)
				{
					this.activeDialogueEvents[previousEvent + "_memory_oneweek"] = 4;
				}
				if (this.previousActiveDialogueEvents[previousEvent] == 14)
				{
					this.activeDialogueEvents[previousEvent + "_memory_twoweeks"] = 4;
				}
				if (this.previousActiveDialogueEvents[previousEvent] == 28)
				{
					this.activeDialogueEvents[previousEvent + "_memory_fourweeks"] = 4;
				}
				if (this.previousActiveDialogueEvents[previousEvent] == 56)
				{
					this.activeDialogueEvents[previousEvent + "_memory_eightweeks"] = 4;
				}
				if (this.previousActiveDialogueEvents[previousEvent] == 104)
				{
					this.activeDialogueEvents[previousEvent + "_memory_oneyear"] = 4;
				}
			}
			this.hasMoved = false;
			if (Game1.random.NextDouble() < 0.905 && !this.hasOrWillReceiveMail("RarecrowSociety") && Utility.doesItemExistAnywhere("(BC)136") && Utility.doesItemExistAnywhere("(BC)137") && Utility.doesItemExistAnywhere("(BC)138") && Utility.doesItemExistAnywhere("(BC)139") && Utility.doesItemExistAnywhere("(BC)140") && Utility.doesItemExistAnywhere("(BC)126") && Utility.doesItemExistAnywhere("(BC)110") && Utility.doesItemExistAnywhere("(BC)113"))
			{
				this.mailbox.Add("RarecrowSociety");
			}
			this.timeWentToBed.Value = 0;
			this.stats.Set("blessingOfWaters", 0);
			if (this.shirtItem.Value != null && this.pantsItem.Value != null && (base.currentLocation is FarmHouse || base.currentLocation is IslandFarmHouse || base.currentLocation is Shed))
			{
				foreach (Object @object in base.currentLocation.netObjects.Values)
				{
					Mannequin mannequin = @object as Mannequin;
					if (mannequin != null && mannequin.GetMannequinData().Cursed && Game1.random.NextDouble() < 0.005 && !mannequin.swappedWithFarmerTonight.Value)
					{
						mannequin.hat.Value = this.Equip<Hat>(mannequin.hat.Value, this.hat);
						mannequin.shirt.Value = this.Equip<Clothing>(mannequin.shirt.Value, this.shirtItem);
						mannequin.pants.Value = this.Equip<Clothing>(mannequin.pants.Value, this.pantsItem);
						mannequin.boots.Value = this.Equip<Boots>(mannequin.boots.Value, this.boots);
						mannequin.swappedWithFarmerTonight.Value = true;
						base.currentLocation.playSound("cursed_mannequin", null, null, SoundContext.Default);
						mannequin.eyeTimer = 1000;
					}
				}
			}
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x00058108 File Offset: 0x00056308
		public bool hasSeenActiveDialogueEvent(string eventName)
		{
			return this.activeDialogueEvents.ContainsKey(eventName) || this.previousActiveDialogueEvents.ContainsKey(eventName);
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x00058126 File Offset: 0x00056326
		public bool autoGenerateActiveDialogueEvent(string eventName, int duration = 4)
		{
			if (!this.hasSeenActiveDialogueEvent(eventName))
			{
				this.activeDialogueEvents[eventName] = duration;
				return true;
			}
			return false;
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x00058144 File Offset: 0x00056344
		public void removeDatingActiveDialogueEvents(string npcName)
		{
			this.activeDialogueEvents.Remove("dating_" + npcName);
			this.removeActiveDialogMemoryEvents("dating_" + npcName);
			this.previousActiveDialogueEvents.Remove("dating_" + npcName);
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x00058190 File Offset: 0x00056390
		public void removeMarriageActiveDialogueEvents(string npcName)
		{
			this.activeDialogueEvents.Remove("married_" + npcName);
			this.removeActiveDialogMemoryEvents("married_" + npcName);
			this.previousActiveDialogueEvents.Remove("married_" + npcName);
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x000581DC File Offset: 0x000563DC
		public void removeActiveDialogMemoryEvents(string activeDialogKey)
		{
			this.activeDialogueEvents.Remove(activeDialogKey + "_memory_oneday");
			this.activeDialogueEvents.Remove(activeDialogKey + "_memory_oneweek");
			this.activeDialogueEvents.Remove(activeDialogKey + "_memory_twoweeks");
			this.activeDialogueEvents.Remove(activeDialogKey + "_memory_fourweeks");
			this.activeDialogueEvents.Remove(activeDialogKey + "_memory_eightweeks");
			this.activeDialogueEvents.Remove(activeDialogKey + "_memory_oneyear");
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x00058274 File Offset: 0x00056474
		public void doDivorce()
		{
			this.divorceTonight.Value = false;
			if (!this.isMarriedOrRoommates())
			{
				return;
			}
			NPC currentSpouse = null;
			if (this.spouse != null)
			{
				currentSpouse = this.getSpouse();
				if (currentSpouse != null)
				{
					this.removeMarriageActiveDialogueEvents(currentSpouse.Name);
					if (!currentSpouse.isRoommate())
					{
						this.autoGenerateActiveDialogueEvent("divorced_" + currentSpouse.Name, 4);
					}
					this.spouse = null;
					this.specialItems.RemoveWhere((string id) => id == "460");
					Friendship friendship;
					if (this.friendshipData.TryGetValue(currentSpouse.name.Value, out friendship))
					{
						friendship.Points = 0;
						friendship.RoommateMarriage = false;
						friendship.Status = FriendshipStatus.Divorced;
					}
					Utility.getHomeOfFarmer(this).showSpouseRoom();
					Game1.getFarm().UpdatePatio();
					this.removeQuest("126");
				}
			}
			else if (this.team.GetSpouse(this.UniqueMultiplayerID) != null)
			{
				long spouseID = this.team.GetSpouse(this.UniqueMultiplayerID).Value;
				Friendship friendship2 = this.team.GetFriendship(this.UniqueMultiplayerID, spouseID);
				friendship2.Points = 0;
				friendship2.RoommateMarriage = false;
				friendship2.Status = FriendshipStatus.Divorced;
			}
			if (!(((currentSpouse != null) ? new bool?(currentSpouse.isRoommate()) : null) ?? false) && !this.autoGenerateActiveDialogueEvent("divorced_once", 4))
			{
				this.autoGenerateActiveDialogueEvent("divorced_twice", 4);
			}
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x00058404 File Offset: 0x00056604
		public static void showReceiveNewItemMessage(Farmer who, Item item, int countAdded)
		{
			string message = item.checkForSpecialItemHoldUpMeessage();
			if (message == null)
			{
				bool fromGiftbox;
				if (item.TryGetTempData<bool>("FromStarterGiftBox", out fromGiftbox) && fromGiftbox && item.QualifiedItemId == "(O)472" && countAdded == 15)
				{
					message = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1918");
				}
				else if (item.HasContextTag("book_item"))
				{
					message = Game1.content.LoadString("Strings\\1_6_Strings:FoundABook", item.DisplayName);
				}
				else
				{
					message = ((countAdded > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1922", countAdded, item.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", item.DisplayName, Lexicon.getProperArticleForWord(item.DisplayName)));
				}
			}
			Game1.drawObjectDialogue(new List<string>
			{
				message
			});
			who.completelyStopAnimatingOrDoingAction();
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x000584D8 File Offset: 0x000566D8
		public static void showEatingItem(Farmer who)
		{
			TemporaryAnimatedSprite tempSprite = null;
			if (who.itemToEat == null)
			{
				return;
			}
			TemporaryAnimatedSprite coloredTempSprite = null;
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(who.itemToEat.QualifiedItemId);
			string textureName = dataOrErrorItem.TextureName;
			Microsoft.Xna.Framework.Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
			Color color = Color.White;
			Color coloredObjectColor = Color.White;
			if (who.tempFoodItemTextureName.Value != null)
			{
				textureName = who.tempFoodItemTextureName.Value;
				sourceRect = who.tempFoodItemSourceRect.Value;
			}
			else
			{
				Object @object = who.itemToEat as Object;
				if (((@object != null) ? @object.preservedParentSheetIndex.Value : null) != null)
				{
					if (who.itemToEat.ItemId.Equals("SmokedFish"))
					{
						ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem("(O)" + (who.itemToEat as Object).preservedParentSheetIndex.Value);
						textureName = dataOrErrorItem2.TextureName;
						sourceRect = dataOrErrorItem2.GetSourceRect(0, null);
						color = new Color(130, 100, 83);
					}
					else
					{
						ColoredObject coloredO = who.itemToEat as ColoredObject;
						if (coloredO != null)
						{
							coloredObjectColor = coloredO.color.Value;
						}
					}
				}
			}
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				if (who.IsLocalPlayer && who.itemToEat.QualifiedItemId == "(O)434")
				{
					tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 62.75f, 8, 2, who.Position + new Vector2(-21f, -112f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
				}
				else
				{
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 254f, 1, 0, who.Position + new Vector2(-21f, -112f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, 0f, 0f, 0f, false);
					if (!coloredObjectColor.Equals(Color.White))
					{
						sourceRect.X += sourceRect.Width;
						coloredTempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 254f, 1, 0, who.Position + new Vector2(-21f, -112f), false, false, (float)(who.StandingPixel.Y + 1) / 10000f + 0.01f, 0f, coloredObjectColor, 4f, 0f, 0f, 0f, false);
					}
				}
				break;
			case 2:
				if (who.IsLocalPlayer && who.itemToEat.QualifiedItemId == "(O)434")
				{
					tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 81.25f, 8, 0, who.Position + new Vector2(-21f, -108f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f, false)
					{
						motion = new Vector2(0.8f, -11f),
						acceleration = new Vector2(0f, 0.5f)
					};
				}
				else
				{
					if (Game1.currentLocation == who.currentLocation)
					{
						Game1.playSound("dwop", null);
					}
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 650f, 1, 0, who.Position + new Vector2(-21f, -108f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, -0.01f, 0f, 0f, false)
					{
						motion = new Vector2(0.8f, -11f),
						acceleration = new Vector2(0f, 0.5f)
					};
					if (!coloredObjectColor.Equals(Color.White))
					{
						sourceRect.X += sourceRect.Width;
						coloredTempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 650f, 1, 0, who.Position + new Vector2(-21f, -108f), false, false, (float)(who.StandingPixel.Y + 1) / 10000f + 0.01f, 0f, coloredObjectColor, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(0.8f, -11f),
							acceleration = new Vector2(0f, 0.5f)
						};
					}
				}
				break;
			case 3:
				who.yJumpVelocity = 6f;
				who.yJumpOffset = 1;
				break;
			case 4:
				if (Game1.currentLocation == who.currentLocation && who.ShouldHandleAnimationSound())
				{
					Game1.playSound("eat", null);
				}
				for (int i = 0; i < 8; i++)
				{
					int size = Game1.random.Next(2, 4);
					Microsoft.Xna.Framework.Rectangle r = sourceRect.Clone();
					r.X += 8;
					r.Y += 8;
					r.Width = size;
					r.Height = size;
					tempSprite = new TemporaryAnimatedSprite(textureName, r, 400f, 1, 0, who.Position + new Vector2(24f, -48f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, 0f, 0f, 0f, false)
					{
						motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-6, -3)),
						acceleration = new Vector2(0f, 0.5f)
					};
					who.currentLocation.temporarySprites.Add(tempSprite);
				}
				return;
			default:
				who.freezePause = 0;
				break;
			}
			if (tempSprite != null)
			{
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
			if (coloredTempSprite != null)
			{
				who.currentLocation.temporarySprites.Add(coloredTempSprite);
			}
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x00058B47 File Offset: 0x00056D47
		public static void eatItem(Farmer who)
		{
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x00058B49 File Offset: 0x00056D49
		public bool hasBuff(string id)
		{
			return this.buffs.IsApplied(id);
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x00058B58 File Offset: 0x00056D58
		public void applyBuff(string id)
		{
			this.buffs.Apply(new Buff(id, null, null, -1, null, -1, null, null, null, null));
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x00058B87 File Offset: 0x00056D87
		public void applyBuff(Buff buff)
		{
			this.buffs.Apply(buff);
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x00058B95 File Offset: 0x00056D95
		public bool hasBuffWithNameContainingString(string idSubstr)
		{
			return this.buffs.HasBuffWithNameContaining(idSubstr);
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x00058BA4 File Offset: 0x00056DA4
		public bool hasOrWillReceiveMail(string id)
		{
			return this.mailReceived.Contains(id) || this.mailForTomorrow.Contains(id) || this.mailbox.Contains(id) || this.mailForTomorrow.Contains(id + "%&NL&%");
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x00058BF4 File Offset: 0x00056DF4
		public static void showHoldingItem(Farmer who, Item item)
		{
			SpecialItem specialItem = item as SpecialItem;
			if (specialItem != null)
			{
				TemporaryAnimatedSprite t = specialItem.getTemporarySpriteForHoldingUp(who.Position + new Vector2(0f, -124f));
				t.motion = new Vector2(0f, -0.1f);
				t.scale = 4f;
				t.interval = 2500f;
				t.totalNumberOfLoops = 0;
				t.animationLength = 1;
				Game1.currentLocation.temporarySprites.Add(t);
				return;
			}
			if (item is Slingshot || item is MeleeWeapon || item is Boots)
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(0f, -0.1f)
				};
				sprite.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
				Game1.currentLocation.temporarySprites.Add(sprite);
				return;
			}
			if (item is Hat)
			{
				TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(-8f, -124f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(0f, -0.1f)
				};
				sprite2.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
				Game1.currentLocation.temporarySprites.Add(sprite2);
				return;
			}
			if (!(item is Furniture))
			{
				if (!(item is Object) && !(item is Tool))
				{
					if (item is Ring)
					{
						TemporaryAnimatedSprite sprite3 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(-4f, -124f), false, false)
						{
							motion = new Vector2(0f, -0.1f),
							layerDepth = 1f
						};
						sprite3.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
						Game1.currentLocation.temporarySprites.Add(sprite3);
						return;
					}
					if (item == null)
					{
						Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(420, 489, 25, 18), 2500f, 1, 0, who.Position + new Vector2(-20f, -152f), false, false)
						{
							motion = new Vector2(0f, -0.1f),
							scale = 4f,
							layerDepth = 1f
						});
						return;
					}
					TemporaryAnimatedSprite sprite4 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), false, false)
					{
						motion = new Vector2(0f, -0.1f),
						layerDepth = 1f
					};
					sprite4.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
					Game1.currentLocation.temporarySprites.Add(sprite4);
				}
				else
				{
					Object obj = item as Object;
					if (obj != null && obj.bigCraftable.Value)
					{
						TemporaryAnimatedSprite sprite5 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -188f), false, false)
						{
							motion = new Vector2(0f, -0.1f),
							layerDepth = 1f
						};
						sprite5.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
						Game1.currentLocation.temporarySprites.Add(sprite5);
						return;
					}
					TemporaryAnimatedSprite sprite6 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), false, false)
					{
						motion = new Vector2(0f, -0.1f),
						layerDepth = 1f
					};
					sprite6.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
					Game1.currentLocation.temporarySprites.Add(sprite6);
					if (who.IsLocalPlayer && item.QualifiedItemId == "(O)434")
					{
						who.eatHeldObject();
						return;
					}
				}
				return;
			}
			TemporaryAnimatedSprite sprite7 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, Vector2.Zero, false, false)
			{
				motion = new Vector2(0f, -0.1f),
				layerDepth = 1f
			};
			sprite7.CopyAppearanceFromItemId(item.QualifiedItemId, 0);
			sprite7.initialPosition = (sprite7.position = who.Position + new Vector2((float)(32 - sprite7.sourceRect.Width / 2 * 4), -188f));
			Game1.currentLocation.temporarySprites.Add(sprite7);
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0005911C File Offset: 0x0005731C
		public void holdUpItemThenMessage(Item item, bool showMessage = true)
		{
			this.holdUpItemThenMessage(item, (item == null) ? 1 : item.Stack, showMessage);
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x00059134 File Offset: 0x00057334
		public void holdUpItemThenMessage(Item item, int countAdded, bool showMessage = true)
		{
			this.completelyStopAnimatingOrDoingAction();
			if (showMessage)
			{
				Game1.MusicDuckTimer = 2000f;
				DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750, null, null, -1, false);
			}
			this.faceDirection(2);
			this.freezePause = 4000;
			this.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(57, 0),
				new FarmerSprite.AnimationFrame(57, 2500, false, false, delegate(Farmer who)
				{
					Farmer.showHoldingItem(who, item);
				}, false),
				showMessage ? new FarmerSprite.AnimationFrame((int)((short)this.FarmerSprite.CurrentFrame), 500, false, false, delegate(Farmer who)
				{
					Farmer.showReceiveNewItemMessage(who, item, countAdded);
				}, true) : new FarmerSprite.AnimationFrame((int)((short)this.FarmerSprite.CurrentFrame), 500, false, false, null, false)
			}, null);
			this.mostRecentlyGrabbedItem = item;
			this.canMove = false;
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x00059238 File Offset: 0x00057438
		public void resetState()
		{
			this.mount = null;
			this.ClearBuffs();
			this.TemporaryItem = null;
			this.swimming.Value = false;
			this.bathingClothes.Value = false;
			this.ignoreCollisions = false;
			this.resetItemStates();
			this.fireToolEvent.Clear();
			this.beginUsingToolEvent.Clear();
			this.endUsingToolEvent.Clear();
			this.sickAnimationEvent.Clear();
			this.passOutEvent.Clear();
			this.drinkAnimationEvent.Clear();
			this.eatAnimationEvent.Clear();
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x000592CC File Offset: 0x000574CC
		public void resetItemStates()
		{
			for (int i = 0; i < this.Items.Count; i++)
			{
				Item item = this.Items[i];
				if (item != null)
				{
					item.resetState();
				}
			}
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x00059308 File Offset: 0x00057508
		public void clearBackpack()
		{
			for (int i = 0; i < this.Items.Count; i++)
			{
				this.Items[i] = null;
			}
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x00059338 File Offset: 0x00057538
		public void resetFriendshipsForNewDay()
		{
			foreach (string name in this.friendshipData.Keys)
			{
				bool single = false;
				NPC i = Game1.getCharacterFromName(name, true, false);
				if (i == null)
				{
					i = Game1.getCharacterFromName<Child>(name, false, false);
				}
				if (i != null)
				{
					if (i != null && i.datable.Value && !this.friendshipData[name].IsDating() && !i.isMarried())
					{
						single = true;
					}
					if (this.spouse != null && name == this.spouse && !this.hasPlayerTalkedToNPC(name))
					{
						this.changeFriendship(-20, i);
					}
					else if (i != null && this.friendshipData[name].IsDating() && !this.hasPlayerTalkedToNPC(name) && this.friendshipData[name].Points < 2500)
					{
						this.changeFriendship(-8, i);
					}
					if (this.hasPlayerTalkedToNPC(name))
					{
						this.friendshipData[name].TalkedToToday = false;
					}
					else if ((!single && this.friendshipData[name].Points < 2500) || (single && this.friendshipData[name].Points < 2000))
					{
						this.changeFriendship(-2, i);
					}
				}
			}
			this.updateFriendshipGifts(Game1.Date);
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x000594C4 File Offset: 0x000576C4
		public virtual int GetAppliedMagneticRadius()
		{
			return Math.Max(128, this.MagneticRadius);
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x000594D8 File Offset: 0x000576D8
		public void updateFriendshipGifts(WorldDate date)
		{
			foreach (string name in this.friendshipData.Keys)
			{
				int totalDays = date.TotalDays;
				WorldDate lastGiftDate = this.friendshipData[name].LastGiftDate;
				int? num = (lastGiftDate != null) ? new int?(lastGiftDate.TotalDays) : null;
				if (!(totalDays == num.GetValueOrDefault() & num != null))
				{
					this.friendshipData[name].GiftsToday = 0;
				}
				int totalSundayWeeks = date.TotalSundayWeeks;
				WorldDate lastGiftDate2 = this.friendshipData[name].LastGiftDate;
				num = ((lastGiftDate2 != null) ? new int?(lastGiftDate2.TotalSundayWeeks) : null);
				if (!(totalSundayWeeks == num.GetValueOrDefault() & num != null))
				{
					if (this.friendshipData[name].GiftsThisWeek >= 2)
					{
						this.changeFriendship(10, Game1.getCharacterFromName(name, true, false));
					}
					this.friendshipData[name].GiftsThisWeek = 0;
				}
			}
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x00059604 File Offset: 0x00057804
		public bool hasPlayerTalkedToNPC(string name)
		{
			Friendship friendship;
			if (!this.friendshipData.TryGetValue(name, out friendship) && NPC.CanSocializePerData(name, base.currentLocation))
			{
				friendship = (this.friendshipData[name] = new Friendship());
			}
			return friendship != null && friendship.TalkedToToday;
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x00059650 File Offset: 0x00057850
		public void fuelLantern(int units)
		{
			Tool lantern = this.getToolFromName("Lantern");
			if (lantern != null)
			{
				((Lantern)lantern).fuelLeft = Math.Min(100, ((Lantern)lantern).fuelLeft + units);
			}
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x0005968C File Offset: 0x0005788C
		public bool IsEquippedItem(Item item)
		{
			if (item != null)
			{
				using (IEnumerator<Item> enumerator = this.GetEquippedItems().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == item)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x000596E0 File Offset: 0x000578E0
		public IEnumerable<Item> GetEquippedItems()
		{
			return from item in new Item[]
			{
				this.CurrentTool,
				this.hat.Value,
				this.shirtItem.Value,
				this.pantsItem.Value,
				this.boots.Value,
				this.leftRing.Value,
				this.rightRing.Value
			}
			where item != null
			select item;
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x00059774 File Offset: 0x00057974
		public override bool collideWith(Object o)
		{
			base.collideWith(o);
			if (this.isRidingHorse() && o is Fence)
			{
				this.mount.squeezeForGate();
				int facingDirection = this.FacingDirection;
				if (facingDirection != 1)
				{
					if (facingDirection == 3 && o.tileLocation.X > base.Tile.X)
					{
						return false;
					}
				}
				else if (o.tileLocation.X < base.Tile.X)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x000597E8 File Offset: 0x000579E8
		public void changeIntoSwimsuit()
		{
			this.bathingClothes.Value = true;
			this.Halt();
			this.setRunning(false, false);
			this.canOnlyWalk = true;
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x0005980B File Offset: 0x00057A0B
		public void changeOutOfSwimSuit()
		{
			this.bathingClothes.Value = false;
			this.canOnlyWalk = false;
			this.Halt();
			this.FarmerSprite.StopAnimation();
			if (Game1.options.autoRun)
			{
				this.setRunning(true, false);
			}
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x00059848 File Offset: 0x00057A48
		public void showFrame(int frame, bool flip = false)
		{
			FarmerSprite.AnimationFrame[] animationFrames = new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(Convert.ToInt32(frame), 100, false, flip, null, false)
			};
			this.FarmerSprite.setCurrentAnimation(animationFrames);
			this.FarmerSprite.loop = true;
			this.FarmerSprite.PauseForSingleAnimation = true;
			this.Sprite.currentFrame = Convert.ToInt32(frame);
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x000598A9 File Offset: 0x00057AA9
		public void stopShowingFrame()
		{
			this.FarmerSprite.loop = false;
			this.FarmerSprite.PauseForSingleAnimation = false;
			this.completelyStopAnimatingOrDoingAction();
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x000598C9 File Offset: 0x00057AC9
		public Item addItemToInventory(Item item)
		{
			return this.addItemToInventory(item, null);
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x000598D4 File Offset: 0x00057AD4
		public Item addItemToInventory(Item item, List<Item> affected_items_list)
		{
			if (item == null)
			{
				return null;
			}
			bool needsInventorySpace;
			bool flag;
			this.GetItemReceiveBehavior(item, out needsInventorySpace, out flag);
			if (!needsInventorySpace)
			{
				this.OnItemReceived(item, item.Stack, null, false);
				return null;
			}
			int originalStack = item.Stack;
			int stackLeft = originalStack;
			foreach (Item slot in this.Items)
			{
				if (item.canStackWith(slot))
				{
					int stack = item.Stack;
					stackLeft = slot.addToStack(item);
					int added = stack - stackLeft;
					if (added > 0)
					{
						item.Stack = stackLeft;
						this.OnItemReceived(item, added, slot, true);
						if (affected_items_list != null)
						{
							affected_items_list.Add(slot);
						}
						if (stackLeft < 1)
						{
							break;
						}
					}
				}
			}
			if (stackLeft > 0)
			{
				int i = 0;
				while (i < this.maxItems.Value && i < this.Items.Count)
				{
					if (this.Items[i] == null)
					{
						item.onDetachedFromParent();
						this.Items[i] = item;
						stackLeft = 0;
						this.OnItemReceived(item, item.Stack, null, true);
						if (affected_items_list != null)
						{
							affected_items_list.Add(this.Items[i]);
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
			}
			if (originalStack > stackLeft)
			{
				this.ShowItemReceivedHudMessageIfNeeded(item, originalStack - stackLeft);
			}
			if (stackLeft <= 0)
			{
				return null;
			}
			return item;
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x00059A24 File Offset: 0x00057C24
		public Item addItemToInventory(Item item, int position)
		{
			if (item == null)
			{
				return null;
			}
			bool needsInventorySpace;
			bool flag;
			this.GetItemReceiveBehavior(item, out needsInventorySpace, out flag);
			if (!needsInventorySpace)
			{
				this.OnItemReceived(item, item.Stack, null, false);
				return null;
			}
			if (position >= 0 && position < this.Items.Count)
			{
				if (this.Items[position] == null)
				{
					this.Items[position] = item;
					this.OnItemReceived(item, item.Stack, null, false);
					return null;
				}
				if (!this.Items[position].canStackWith(item))
				{
					Item result = this.Items[position];
					this.Items[position] = item;
					this.OnItemReceived(item, item.Stack, null, false);
					return result;
				}
				int stack = item.Stack;
				int stackLeft = this.Items[position].addToStack(item);
				int added = stack - stackLeft;
				if (added > 0)
				{
					item.Stack = stackLeft;
					this.OnItemReceived(item, added, this.Items[position], false);
					if (stackLeft <= 0)
					{
						return null;
					}
					return item;
				}
			}
			return item;
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x00059B1C File Offset: 0x00057D1C
		public bool addItemToInventoryBool(Item item, bool makeActiveObject = false)
		{
			if (item == null)
			{
				return false;
			}
			if (this.IsLocalPlayer)
			{
				Item remainder = null;
				bool needsInventorySpace;
				bool flag;
				this.GetItemReceiveBehavior(item, out needsInventorySpace, out flag);
				if (needsInventorySpace)
				{
					remainder = this.addItemToInventory(item);
				}
				else
				{
					this.OnItemReceived(item, item.Stack, null, false);
				}
				int? num = (remainder != null) ? new int?(remainder.Stack) : null;
				int stack = item.Stack;
				bool success = !(num.GetValueOrDefault() == stack & num != null) || item is SpecialItem;
				if (makeActiveObject && success && !(item is SpecialItem) && remainder != null && item.Stack <= 1)
				{
					int newItemPosition = this.getIndexOfInventoryItem(item);
					if (newItemPosition > -1)
					{
						Item i = this.Items[this.currentToolIndex.Value];
						this.Items[this.currentToolIndex.Value] = this.Items[newItemPosition];
						this.Items[newItemPosition] = i;
					}
				}
				return success;
			}
			return false;
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x00059C1C File Offset: 0x00057E1C
		public void addItemByMenuIfNecessaryElseHoldUp(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
		{
			int countAdded = item.Stack;
			this.mostRecentlyGrabbedItem = item;
			this.addItemsByMenuIfNecessary(new List<Item>
			{
				item
			}, itemSelectedCallback, forceQueue);
			if (Game1.activeClickableMenu == null && ((item != null) ? item.QualifiedItemId : null) != "(O)434")
			{
				this.holdUpItemThenMessage(item, countAdded, true);
			}
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x00059C73 File Offset: 0x00057E73
		public void addItemByMenuIfNecessary(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
		{
			this.addItemsByMenuIfNecessary(new List<Item>
			{
				item
			}, itemSelectedCallback, forceQueue);
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x00059C8C File Offset: 0x00057E8C
		public void addItemsByMenuIfNecessary(List<Item> itemsToAdd, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
		{
			if (itemsToAdd == null || !this.IsLocalPlayer)
			{
				return;
			}
			if (itemsToAdd.Count > 0)
			{
				Item item2 = itemsToAdd[0];
				if (((item2 != null) ? item2.QualifiedItemId : null) == "(O)434")
				{
					if (Game1.activeClickableMenu == null && !forceQueue)
					{
						this.eatObject(itemsToAdd[0] as Object, true);
						return;
					}
					Game1.nextClickableMenu.Add(ItemGrabMenu.CreateOverflowMenu(itemsToAdd, null));
					return;
				}
			}
			for (int i = itemsToAdd.Count - 1; i >= 0; i--)
			{
				if (this.addItemToInventoryBool(itemsToAdd[i], false))
				{
					if (itemSelectedCallback != null)
					{
						itemSelectedCallback(itemsToAdd[i], this);
					}
					itemsToAdd.Remove(itemsToAdd[i]);
				}
			}
			if (itemsToAdd.Count > 0 && (forceQueue || Game1.activeClickableMenu != null))
			{
				for (int menuIndex = 0; menuIndex < Game1.nextClickableMenu.Count; menuIndex++)
				{
					ItemGrabMenu menu = Game1.nextClickableMenu[menuIndex] as ItemGrabMenu;
					if (menu != null && menu.source == 4)
					{
						IList<Item> inventory = menu.ItemsToGrabMenu.actualInventory;
						int capacity = menu.ItemsToGrabMenu.capacity;
						bool anyAdded = false;
						for (int j = 0; j < itemsToAdd.Count; j++)
						{
							Item item = itemsToAdd[j];
							int stack = item.Stack;
							item = (itemsToAdd[j] = Utility.addItemToThisInventoryList(item, inventory, capacity));
							int? num = (item != null) ? new int?(item.Stack) : null;
							if (!(stack == num.GetValueOrDefault() & num != null))
							{
								anyAdded = true;
								if (item == null)
								{
									itemsToAdd.RemoveAt(j);
									j--;
								}
							}
						}
						if (anyAdded)
						{
							Game1.nextClickableMenu[menuIndex] = ItemGrabMenu.CreateOverflowMenu(inventory, null);
						}
					}
					if (itemsToAdd.Count == 0)
					{
						break;
					}
				}
			}
			if (itemsToAdd.Count > 0)
			{
				ItemGrabMenu itemGrabMenu = ItemGrabMenu.CreateOverflowMenu(itemsToAdd, null);
				if (forceQueue || Game1.activeClickableMenu != null)
				{
					Game1.nextClickableMenu.Add(itemGrabMenu);
					return;
				}
				Game1.activeClickableMenu = itemGrabMenu;
			}
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x00059E84 File Offset: 0x00058084
		public virtual void BeginSitting(ISittable furniture)
		{
			if (furniture == null)
			{
				return;
			}
			if (this.bathingClothes.Value || this.swimming.Value || this.isRidingHorse() || !this.CanMove || this.UsingTool || base.IsEmoting)
			{
				return;
			}
			Vector2? sitting_position = furniture.AddSittingFarmer(this);
			if (sitting_position != null)
			{
				base.playNearbySoundAll("woodyStep", null, SoundContext.Default);
				this.Halt();
				this.synchronizedJump(4f);
				this.FarmerSprite.StopAnimation();
				this.sittingFurniture = furniture;
				this.mapChairSitPosition.Value = new Vector2(-1f, -1f);
				if (this.sittingFurniture is MapSeat)
				{
					Vector2? seat_position = this.sittingFurniture.GetSittingPosition(this, true);
					if (seat_position != null)
					{
						this.mapChairSitPosition.Value = seat_position.Value;
					}
				}
				this.isSitting.Value = true;
				this.LerpPosition(base.Position, new Vector2(sitting_position.Value.X * 64f, sitting_position.Value.Y * 64f), 0.15f);
				this.freezePause += 100;
			}
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x00059FC1 File Offset: 0x000581C1
		public virtual void LerpPosition(Vector2 start_position, Vector2 end_position, float duration)
		{
			this.freezePause = (int)(duration * 1000f);
			this.lerpStartPosition = start_position;
			this.lerpEndPosition = end_position;
			this.lerpPosition = 0f;
			this.lerpDuration = duration;
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x00059FF4 File Offset: 0x000581F4
		public virtual void StopSitting(bool animate = true)
		{
			if (this.sittingFurniture != null)
			{
				ISittable furniture = this.sittingFurniture;
				if (!animate)
				{
					this.mapChairSitPosition.Value = new Vector2(-1f, -1f);
					furniture.RemoveSittingFarmer(this);
				}
				bool furniture_is_in_this_location = false;
				bool location_found = false;
				Vector2 old_position = base.Position;
				if (furniture.IsSeatHere(base.currentLocation))
				{
					furniture_is_in_this_location = true;
					List<Vector2> exit_positions = new List<Vector2>();
					Vector2 sit_position = new Vector2((float)furniture.GetSeatBounds().Left, (float)furniture.GetSeatBounds().Top);
					if (furniture.IsSittingHere(this))
					{
						sit_position = furniture.GetSittingPosition(this, true).Value;
					}
					if (furniture.GetSittingDirection() == 2)
					{
						exit_positions.Add(sit_position + new Vector2(0f, 1f));
						this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(1f, 0f), sit_position + new Vector2(-1f, 0f), sit_position + new Vector2(0f, -1f));
					}
					else if (furniture.GetSittingDirection() == 1)
					{
						exit_positions.Add(sit_position + new Vector2(1f, 0f));
						this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(0f, -1f), sit_position + new Vector2(0f, 1f), sit_position + new Vector2(-1f, 0f));
					}
					else if (furniture.GetSittingDirection() == 3)
					{
						exit_positions.Add(sit_position + new Vector2(-1f, 0f));
						this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(0f, 1f), sit_position + new Vector2(0f, -1f), sit_position + new Vector2(1f, 0f));
					}
					else if (furniture.GetSittingDirection() == 0)
					{
						exit_positions.Add(sit_position + new Vector2(0f, -1f));
						this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(-1f, 0f), sit_position + new Vector2(1f, 0f), sit_position + new Vector2(0f, 1f));
					}
					Microsoft.Xna.Framework.Rectangle bounds = furniture.GetSeatBounds();
					bounds.Inflate(1, 1);
					foreach (Vector2 v in Utility.getBorderOfThisRectangle(bounds))
					{
						exit_positions.Add(v);
					}
					foreach (Vector2 exit_position in exit_positions)
					{
						base.setTileLocation(exit_position);
						Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
						base.Position = old_position;
						Object tile_object = base.currentLocation.getObjectAtTile((int)exit_position.X, (int)exit_position.Y, true);
						if (!base.currentLocation.isCollidingPosition(boundingBox, Game1.viewport, true, 0, false, this) && (tile_object == null || tile_object.isPassable()))
						{
							if (animate)
							{
								base.playNearbySoundAll("coin", null, SoundContext.Default);
								this.synchronizedJump(4f);
								this.LerpPosition(sit_position * 64f, exit_position * 64f, 0.15f);
							}
							location_found = true;
							break;
						}
					}
				}
				if (!location_found)
				{
					if (animate)
					{
						base.playNearbySoundAll("coin", null, SoundContext.Default);
					}
					base.Position = old_position;
					if (furniture_is_in_this_location)
					{
						Microsoft.Xna.Framework.Rectangle bounds2 = furniture.GetSeatBounds();
						bounds2.X *= 64;
						bounds2.Y *= 64;
						bounds2.Width *= 64;
						bounds2.Height *= 64;
						this.temporaryPassableTiles.Add(bounds2);
					}
				}
				if (!animate)
				{
					this.sittingFurniture = null;
					this.isSitting.Value = false;
					this.Halt();
					this.showNotCarrying();
				}
				else
				{
					this.isStopSitting = true;
				}
				Game1.haltAfterCheck = false;
				this.yOffset = 0f;
				this.xOffset = 0f;
			}
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x0005A460 File Offset: 0x00058660
		public void SortSeatExitPositions(List<Vector2> list, Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 mouse_pos = Utility.PointToVector2(Game1.getMousePosition(false)) + new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
			Vector2 move_direction = Vector2.Zero;
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveUpButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp))))
			{
				move_direction.Y -= 1f;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveDownButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown))))
			{
				move_direction.Y += 1f;
			}
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveLeftButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadLeft))))
			{
				move_direction.X -= 1f;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveRightButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadRight))))
			{
				move_direction.X += 1f;
			}
			if (move_direction != Vector2.Zero)
			{
				mouse_pos = base.getStandingPosition() + move_direction * 64f;
			}
			mouse_pos /= 64f;
			List<Vector2> exit_positions = new List<Vector2>
			{
				a,
				b,
				c
			};
			exit_positions.Sort((Vector2 d, Vector2 e) => (d + new Vector2(0.5f, 0.5f) - mouse_pos).Length().CompareTo((e + new Vector2(0.5f, 0.5f) - mouse_pos).Length()));
			list.AddRange(exit_positions);
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x0005A702 File Offset: 0x00058902
		public virtual bool IsSitting()
		{
			return this.isSitting.Value;
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x0005A710 File Offset: 0x00058910
		public bool isInventoryFull()
		{
			for (int i = 0; i < this.maxItems.Value; i++)
			{
				if (this.Items.Count > i && this.Items[i] == null)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x0005A754 File Offset: 0x00058954
		public bool couldInventoryAcceptThisItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (item.IsRecipe)
			{
				return true;
			}
			string qualifiedItemId = item.QualifiedItemId;
			if (qualifiedItemId == "(O)73" || qualifiedItemId == "(O)930" || qualifiedItemId == "(O)102" || qualifiedItemId == "(O)858" || qualifiedItemId == "(O)GoldCoin")
			{
				return true;
			}
			for (int i = 0; i < this.maxItems.Value; i++)
			{
				if (this.Items.Count > i && (this.Items[i] == null || (item is Object && this.Items[i] is Object && this.Items[i].Stack + item.Stack <= this.Items[i].maximumStackSize() && (this.Items[i] as Object).canStackWith(item))))
				{
					return true;
				}
			}
			if (this.IsLocalPlayer && this.isInventoryFull() && Game1.hudMessages.Count == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
			}
			return false;
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x0005A888 File Offset: 0x00058A88
		public bool couldInventoryAcceptThisItem(string id, int stack, int quality = 0)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
			string qualifiedItemId = itemData.QualifiedItemId;
			if (qualifiedItemId == "(O)73" || qualifiedItemId == "(O)930" || qualifiedItemId == "(O)102" || qualifiedItemId == "(O)858" || qualifiedItemId == "(O)GoldCoin")
			{
				return true;
			}
			for (int i = 0; i < this.maxItems.Value; i++)
			{
				if (this.Items.Count > i && (this.Items[i] == null || (this.Items[i].Stack + stack <= this.Items[i].maximumStackSize() && this.Items[i].QualifiedItemId == itemData.QualifiedItemId && this.Items[i].quality.Value == quality)))
				{
					return true;
				}
			}
			if (this.IsLocalPlayer && this.isInventoryFull() && Game1.hudMessages.Count == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
			}
			return false;
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x0005A9AC File Offset: 0x00058BAC
		public NPC getSpouse()
		{
			if (this.isMarriedOrRoommates() && this.spouse != null)
			{
				return Game1.getCharacterFromName(this.spouse, true, false);
			}
			return null;
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x0005A9D0 File Offset: 0x00058BD0
		public int freeSpotsInInventory()
		{
			int slotsUsed = this.Items.CountItemStacks();
			if (slotsUsed >= this.maxItems.Value)
			{
				return 0;
			}
			return this.maxItems.Value - slotsUsed;
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0005AA08 File Offset: 0x00058C08
		public void GetItemReceiveBehavior(Item item, out bool needsInventorySpace, out bool showNotification)
		{
			if (item is SpecialItem)
			{
				needsInventorySpace = false;
				showNotification = false;
				return;
			}
			string qualifiedItemId = item.QualifiedItemId;
			if (!(qualifiedItemId == "(O)73") && !(qualifiedItemId == "(O)102") && !(qualifiedItemId == "(O)858"))
			{
				if (!(qualifiedItemId == "(O)GoldCoin") && !(qualifiedItemId == "(O)930"))
				{
					needsInventorySpace = true;
					showNotification = true;
				}
				else
				{
					needsInventorySpace = false;
					showNotification = false;
				}
			}
			else
			{
				needsInventorySpace = false;
				showNotification = true;
			}
			if (item.IsRecipe)
			{
				needsInventorySpace = false;
				showNotification = true;
				return;
			}
		}

		// Token: 0x060008E3 RID: 2275 RVA: 0x0005AA94 File Offset: 0x00058C94
		public void OnItemReceived(Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification = false)
		{
			if (!this.IsLocalPlayer)
			{
				return;
			}
			Object @object = item as Object;
			if (@object != null)
			{
				@object.reloadSprite();
			}
			if (item.HasBeenInInventory)
			{
				return;
			}
			Item actualItem = mergedIntoStack ?? item;
			if (!hideHudNotification)
			{
				bool flag;
				bool showHudNotification;
				this.GetItemReceiveBehavior(actualItem, out flag, out showHudNotification);
				if (showHudNotification)
				{
					this.ShowItemReceivedHudMessage(actualItem, countAdded);
				}
			}
			if (this.freezePause <= 0)
			{
				this.mostRecentlyGrabbedItem = actualItem;
			}
			if (item.SetFlagOnPickup != null)
			{
				if (!this.hasOrWillReceiveMail(item.SetFlagOnPickup))
				{
					Game1.addMail(item.SetFlagOnPickup, true, false);
				}
				actualItem.SetFlagOnPickup = null;
			}
			SpecialItem specialItem = actualItem as SpecialItem;
			if (specialItem != null)
			{
				specialItem.actionWhenReceived(this);
			}
			Object obj = actualItem as Object;
			if (obj != null && obj.specialItem)
			{
				string key = obj.IsRecipe ? ("-" + obj.ItemId) : obj.ItemId;
				if (obj.bigCraftable.Value || obj is Furniture)
				{
					if (!this.specialBigCraftables.Contains(key))
					{
						this.specialBigCraftables.Add(key);
					}
				}
				else if (!this.specialItems.Contains(key))
				{
					this.specialItems.Add(key);
				}
			}
			if (item.IsRecipe)
			{
				item.LearnRecipe(null);
				Game1.playSound("newRecipe", null);
				return;
			}
			int originalStack = actualItem.Stack;
			try
			{
				actualItem.Stack = countAdded;
				this.NotifyQuests((Quest quest) => quest.OnItemReceived(actualItem, countAdded, false), false);
				if (this.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder in this.team.specialOrders)
					{
						Action<Farmer, Item> onItemCollected = specialOrder.onItemCollected;
						if (onItemCollected != null)
						{
							onItemCollected(this, actualItem);
						}
					}
				}
			}
			finally
			{
				actualItem.Stack = originalStack;
			}
			if (actualItem.HasTypeObject())
			{
				Object obj2 = actualItem as Object;
				if (obj2 != null)
				{
					if (obj2.Category == -2 || obj2.Type == "Minerals")
					{
						this.foundMineral(obj2.ItemId);
					}
					else if (obj2.Type == "Arch")
					{
						this.foundArtifact(obj2.ItemId, 1);
					}
				}
			}
			this.stats.checkForHeldItemAchievements();
			string qualifiedItemId = actualItem.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				switch (qualifiedItemId.Length)
				{
				case 5:
					switch (qualifiedItemId[4])
					{
					case '2':
						if (qualifiedItemId == "(O)72")
						{
							Stats stats = this.stats;
							uint num = stats.DiamondsFound;
							stats.DiamondsFound = num + 1U;
						}
						break;
					case '3':
						if (qualifiedItemId == "(O)73")
						{
							this.foundWalnut(countAdded);
							this.removeItemFromInventory(actualItem);
						}
						break;
					case '4':
						if (qualifiedItemId == "(O)74")
						{
							Stats stats2 = this.stats;
							uint num = stats2.PrismaticShardsFound;
							stats2.PrismaticShardsFound = num + 1U;
						}
						break;
					}
					break;
				case 6:
					switch (qualifiedItemId[3])
					{
					case '1':
						if (qualifiedItemId == "(O)102")
						{
							Game1.PerformActionWhenPlayerFree(delegate
							{
								this.foundArtifact(actualItem.ItemId, 1);
							});
							this.removeItemFromInventory(actualItem);
							Stats stats3 = this.stats;
							uint num = stats3.NotesFound;
							stats3.NotesFound = num + 1U;
						}
						break;
					case '3':
						if (!(qualifiedItemId == "(O)390"))
						{
							if (!(qualifiedItemId == "(O)384"))
							{
								if (!(qualifiedItemId == "(O)380"))
								{
									if (!(qualifiedItemId == "(O)386"))
									{
										if (qualifiedItemId == "(O)378")
										{
											this.stats.CopperFound += (uint)countAdded;
											if (!this.hasOrWillReceiveMail("copperFound"))
											{
												Game1.addMailForTomorrow("copperFound", true, false);
											}
										}
									}
									else
									{
										this.stats.IridiumFound += (uint)countAdded;
									}
								}
								else
								{
									this.stats.IronFound += (uint)countAdded;
								}
							}
							else
							{
								this.stats.GoldFound += (uint)countAdded;
							}
						}
						else
						{
							Stats stats4 = this.stats;
							uint num = stats4.StoneGathered;
							stats4.StoneGathered = num + 1U;
							if (this.stats.StoneGathered >= 100U && !this.hasOrWillReceiveMail("robinWell"))
							{
								Game1.addMailForTomorrow("robinWell", false, false);
							}
						}
						break;
					case '4':
						if (qualifiedItemId == "(O)428")
						{
							if (!this.hasOrWillReceiveMail("clothFound"))
							{
								Game1.addMailForTomorrow("clothFound", true, false);
							}
						}
						break;
					case '5':
						if (qualifiedItemId == "(O)535")
						{
							Game1.PerformActionWhenPlayerFree(delegate
							{
								if (!this.hasOrWillReceiveMail("geodeFound"))
								{
									this.mailReceived.Add("geodeFound");
									this.holdUpItemThenMessage(actualItem, true);
								}
							});
						}
						break;
					case '8':
						if (!(qualifiedItemId == "(O)858"))
						{
							if (!(qualifiedItemId == "(O)875"))
							{
								if (!(qualifiedItemId == "(O)876"))
								{
									if (qualifiedItemId == "(O)897")
									{
										if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
										{
											Game1.addMailForTomorrow("gotMissingStocklist", true, true);
										}
									}
								}
								else if (!Game1.MasterPlayer.hasOrWillReceiveMail("prismaticJellyDrop") && this.team.SpecialOrderActive("Wizard2"))
								{
									Game1.addMailForTomorrow("prismaticJellyDrop", true, true);
								}
							}
							else if (!Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop") && this.team.SpecialOrderActive("Wizard"))
							{
								Game1.addMailForTomorrow("ectoplasmDrop", true, true);
							}
						}
						else
						{
							this.QiGems += countAdded;
							Game1.playSound("qi_shop_purchase", null);
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), 100f, 1, 8, new Vector2(0f, -96f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								motion = new Vector2(0f, -6f),
								acceleration = new Vector2(0f, 0.2f),
								stopAcceleratingWhenVelocityIsZero = true,
								attachedCharacter = this,
								positionFollowsAttachedCharacter = true
							});
							this.removeItemFromInventory(actualItem);
						}
						break;
					case '9':
						if (qualifiedItemId == "(O)930")
						{
							int amount = 10 * countAdded;
							this.health = Math.Min(this.maxHealth, this.health + amount);
							base.currentLocation.debris.Add(new Debris(amount, base.getStandingPosition(), Color.Lime, 1f, this));
							Game1.playSound("healSound", null);
							this.removeItemFromInventory(actualItem);
						}
						break;
					}
					break;
				case 7:
				{
					char c = qualifiedItemId[5];
					if (c != '4')
					{
						if (c == '5')
						{
							if (qualifiedItemId == "(BC)256")
							{
								if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
								{
									Game1.addMailForTomorrow("gotFirstJunimoChest", true, true);
								}
							}
						}
					}
					else if (qualifiedItemId == "(BC)248")
					{
						Game1.netWorldState.Value.MiniShippingBinsObtained++;
					}
					break;
				}
				case 11:
					if (qualifiedItemId == "(O)GoldCoin")
					{
						Game1.playSound("moneyDial", null);
						int coinAmount = 250 * countAdded;
						if (Game1.IsSpring && Game1.dayOfMonth == 17 && base.currentLocation is Forest && base.Tile.Y > 90f)
						{
							coinAmount = 25;
						}
						this.Money += coinAmount;
						this.removeItemFromInventory(item);
						Game1.dayTimeMoneyBox.gotGoldCoin(coinAmount);
					}
					break;
				}
			}
			actualItem.HasBeenInInventory = true;
		}

		// Token: 0x060008E4 RID: 2276 RVA: 0x0005B3D0 File Offset: 0x000595D0
		public void ShowItemReceivedHudMessageIfNeeded(Item item, int countAdded)
		{
			bool flag;
			bool showHudNotification;
			this.GetItemReceiveBehavior(item, out flag, out showHudNotification);
			if (showHudNotification)
			{
				this.ShowItemReceivedHudMessage(item, countAdded);
			}
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x0005B3F3 File Offset: 0x000595F3
		public void ShowItemReceivedHudMessage(Item item, int countAdded)
		{
			if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ItemGrabMenu))
			{
				Game1.addHUDMessage(HUDMessage.ForItemGained(item, countAdded, null));
			}
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x0005B418 File Offset: 0x00059618
		public int getIndexOfInventoryItem(Item item)
		{
			for (int i = 0; i < this.Items.Count; i++)
			{
				if (this.Items[i] == item || (this.Items[i] != null && item != null && item.canStackWith(this.Items[i])))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x0005B474 File Offset: 0x00059674
		public void reduceActiveItemByOne()
		{
			if (this.CurrentItem != null)
			{
				Item currentItem = this.CurrentItem;
				int num = currentItem.Stack - 1;
				currentItem.Stack = num;
				if (num <= 0)
				{
					this.removeItemFromInventory(this.CurrentItem);
					this.showNotCarrying();
				}
			}
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x0005B4B4 File Offset: 0x000596B4
		public void ReequipEnchantments()
		{
			Tool tool = this.CurrentTool;
			if (tool != null)
			{
				foreach (BaseEnchantment baseEnchantment in tool.enchantments)
				{
					baseEnchantment.OnEquip(this);
				}
			}
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0005B510 File Offset: 0x00059710
		public void removeItemFromInventory(Item which)
		{
			if (which == null)
			{
				return;
			}
			int i = this.Items.IndexOf(which);
			if (i >= 0 && i < this.Items.Count)
			{
				this.Items[i].actionWhenStopBeingHeld(this);
				this.Items[i] = null;
			}
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x0005B560 File Offset: 0x00059760
		public bool isMarriedOrRoommates()
		{
			Friendship friendship;
			return this.team.IsMarried(this.UniqueMultiplayerID) || (this.spouse != null && this.friendshipData.TryGetValue(this.spouse, out friendship) && friendship.IsMarried());
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x0005B5A8 File Offset: 0x000597A8
		public bool isEngaged()
		{
			Friendship friendship;
			return this.team.IsEngaged(this.UniqueMultiplayerID) || (this.spouse != null && this.friendshipData.TryGetValue(this.spouse, out friendship) && friendship.IsEngaged());
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x0005B5F0 File Offset: 0x000597F0
		public void removeFirstOfThisItemFromInventory(string itemId, int count = 1)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == null)
			{
				return;
			}
			int remaining = count;
			Object activeObject = this.ActiveObject;
			if (((activeObject != null) ? activeObject.QualifiedItemId : null) == itemId)
			{
				int toRemove = Math.Min(remaining, this.ActiveObject.Stack);
				remaining -= toRemove;
				if (this.ActiveObject.ConsumeStack(toRemove) == null)
				{
					this.ActiveObject = null;
					this.showNotCarrying();
				}
			}
			if (remaining > 0)
			{
				this.Items.ReduceId(itemId, remaining);
			}
		}

		// Token: 0x060008ED RID: 2285 RVA: 0x0005B668 File Offset: 0x00059868
		public void rotateShirt(int direction, List<string> validIds = null)
		{
			string itemId = this.shirt.Value;
			if (validIds == null)
			{
				validIds = new List<string>(Game1.shirtData.Keys);
			}
			int index = validIds.IndexOf(itemId);
			if (index == -1)
			{
				itemId = validIds.FirstOrDefault<string>();
				if (itemId != null)
				{
					this.changeShirt(itemId);
				}
				return;
			}
			index = Utility.WrapIndex(index + direction, validIds.Count);
			itemId = validIds[index];
			this.changeShirt(itemId);
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x0005B6D1 File Offset: 0x000598D1
		public void changeShirt(string itemId)
		{
			this.shirt.Set(itemId);
			this.FarmerRenderer.changeShirt(itemId);
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x0005B6EC File Offset: 0x000598EC
		public void rotatePantStyle(int direction, List<string> validIds = null)
		{
			string itemId = this.pants.Value;
			if (validIds == null)
			{
				validIds = new List<string>(Game1.pantsData.Keys);
			}
			int index = validIds.IndexOf(itemId);
			if (index == -1)
			{
				itemId = validIds.FirstOrDefault<string>();
				if (itemId != null)
				{
					this.changePantStyle(itemId);
				}
				return;
			}
			index = Utility.WrapIndex(index + direction, validIds.Count);
			itemId = validIds[index];
			this.changePantStyle(itemId);
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x0005B755 File Offset: 0x00059955
		public void changePantStyle(string itemId)
		{
			this.pants.Set(itemId);
			this.FarmerRenderer.changePants(itemId);
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x0005B770 File Offset: 0x00059970
		public void ConvertClothingOverrideToClothesItems()
		{
			string pantsId;
			Color? color;
			if (this.IsOverridingPants(out pantsId, out color))
			{
				if (ItemRegistry.Exists("(P)" + pantsId))
				{
					this.Equip<Clothing>(new Clothing(pantsId)
					{
						clothesColor = 
						{
							Value = (color ?? Color.White)
						}
					}, this.pantsItem);
				}
				this.pants.Value = "-1";
			}
			string shirtId;
			if (this.IsOverridingShirt(out shirtId))
			{
				int index;
				if (int.TryParse(shirtId, out index) && index < 1000)
				{
					shirtId = (index + 1000).ToString();
				}
				if (ItemRegistry.Exists("(S)" + shirtId))
				{
					Clothing clothes = new Clothing(shirtId);
					this.Equip<Clothing>(clothes, this.shirtItem);
				}
				this.shirt.Value = "-1";
			}
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0005B84D File Offset: 0x00059A4D
		public static Dictionary<int, string> GetHairStyleMetadataFile()
		{
			if (Farmer.hairStyleMetadataFile == null)
			{
				Farmer.hairStyleMetadataFile = DataLoader.HairData(Game1.content);
			}
			return Farmer.hairStyleMetadataFile;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x0005B86C File Offset: 0x00059A6C
		public static HairStyleMetadata GetHairStyleMetadata(int hair_index)
		{
			Farmer.GetHairStyleMetadataFile();
			HairStyleMetadata hair_data;
			if (Farmer.hairStyleMetadata.TryGetValue(hair_index, out hair_data))
			{
				return hair_data;
			}
			try
			{
				string data;
				if (Farmer.hairStyleMetadataFile.TryGetValue(hair_index, out data))
				{
					string[] split = data.Split('/', StringSplitOptions.None);
					HairStyleMetadata new_hair_data = new HairStyleMetadata();
					new_hair_data.texture = Game1.content.Load<Texture2D>("Characters\\Farmer\\" + split[0]);
					new_hair_data.tileX = int.Parse(split[1]);
					new_hair_data.tileY = int.Parse(split[2]);
					if (split.Length > 3 && split[3].EqualsIgnoreCase("true"))
					{
						new_hair_data.usesUniqueLeftSprite = true;
					}
					else
					{
						new_hair_data.usesUniqueLeftSprite = false;
					}
					if (split.Length > 4)
					{
						new_hair_data.coveredIndex = int.Parse(split[4]);
					}
					if (split.Length > 5 && split[5].EqualsIgnoreCase("true"))
					{
						new_hair_data.isBaldStyle = true;
					}
					else
					{
						new_hair_data.isBaldStyle = false;
					}
					hair_data = new_hair_data;
				}
			}
			catch (Exception)
			{
			}
			Farmer.hairStyleMetadata[hair_index] = hair_data;
			return hair_data;
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x0005B970 File Offset: 0x00059B70
		public static List<int> GetAllHairstyleIndices()
		{
			if (Farmer.allHairStyleIndices != null)
			{
				return Farmer.allHairStyleIndices;
			}
			Farmer.GetHairStyleMetadataFile();
			Farmer.allHairStyleIndices = new List<int>();
			int highest_hair = FarmerRenderer.hairStylesTexture.Height / 96 * 8;
			for (int i = 0; i < highest_hair; i++)
			{
				Farmer.allHairStyleIndices.Add(i);
			}
			foreach (int key in Farmer.hairStyleMetadataFile.Keys)
			{
				if (key >= 0 && !Farmer.allHairStyleIndices.Contains(key))
				{
					Farmer.allHairStyleIndices.Add(key);
				}
			}
			Farmer.allHairStyleIndices.Sort();
			return Farmer.allHairStyleIndices;
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x0005BA30 File Offset: 0x00059C30
		public static int GetLastHairStyle()
		{
			return Farmer.GetAllHairstyleIndices()[Farmer.GetAllHairstyleIndices().Count - 1];
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0005BA48 File Offset: 0x00059C48
		public void changeHairStyle(int whichHair)
		{
			bool flag = this.isBald();
			if (Farmer.GetHairStyleMetadata(whichHair) != null)
			{
				this.hair.Set(whichHair);
			}
			else
			{
				if (whichHair < 0)
				{
					whichHair = Farmer.GetLastHairStyle();
				}
				else if (whichHair > Farmer.GetLastHairStyle())
				{
					whichHair = 0;
				}
				this.hair.Set(whichHair);
			}
			if (this.IsBaldHairStyle(whichHair))
			{
				this.FarmerRenderer.textureName.Set(this.getTexture());
			}
			if (flag && !this.isBald())
			{
				this.FarmerRenderer.textureName.Set(this.getTexture());
			}
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x0005BAD4 File Offset: 0x00059CD4
		public virtual bool IsBaldHairStyle(int style)
		{
			if (Farmer.GetHairStyleMetadata(this.hair.Value) != null)
			{
				return Farmer.GetHairStyleMetadata(this.hair.Value).isBaldStyle;
			}
			return style - 49 <= 6;
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x0005BB08 File Offset: 0x00059D08
		private bool isBald()
		{
			return this.IsBaldHairStyle(this.getHair(false));
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x0005BB17 File Offset: 0x00059D17
		public void changeShoeColor(string which)
		{
			this.FarmerRenderer.recolorShoes(which);
			this.shoes.Set(which);
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x0005BB31 File Offset: 0x00059D31
		public void changeHairColor(Color c)
		{
			this.hairstyleColor.Set(c);
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x0005BB3F File Offset: 0x00059D3F
		public void changePantsColor(Color color)
		{
			this.pantsColor.Set(color);
			Clothing value = this.pantsItem.Value;
			if (value == null)
			{
				return;
			}
			value.clothesColor.Set(color);
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0005BB68 File Offset: 0x00059D68
		public void changeHat(int newHat)
		{
			if (newHat < 0)
			{
				this.Equip<Hat>(null, this.hat);
				return;
			}
			this.Equip<Hat>(ItemRegistry.Create<Hat>("(H)" + newHat.ToString(), 1, 0, false), this.hat);
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x0005BBA3 File Offset: 0x00059DA3
		public void changeAccessory(int which)
		{
			if (which < -1)
			{
				which = 29;
			}
			if (which >= -1)
			{
				if (which >= 30)
				{
					which = -1;
				}
				this.accessory.Set(which);
			}
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x0005BBC5 File Offset: 0x00059DC5
		public void changeSkinColor(int which, bool force = false)
		{
			if (which < 0)
			{
				which = 23;
			}
			else if (which >= 24)
			{
				which = 0;
			}
			this.skin.Set(this.FarmerRenderer.recolorSkin(which, force));
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x0005BBF1 File Offset: 0x00059DF1
		public virtual bool hasDarkSkin()
		{
			return (this.skin.Value >= 4 && this.skin.Value <= 8 && this.skin.Value != 7) || this.skin.Value == 14;
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x0005BC2E File Offset: 0x00059E2E
		public void changeEyeColor(Color c)
		{
			this.newEyeColor.Set(c);
			this.FarmerRenderer.recolorEyes(c);
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x0005BC48 File Offset: 0x00059E48
		public int getHair(bool ignore_hat = false)
		{
			if (this.hat.Value != null && !this.bathingClothes.Value && !ignore_hat)
			{
				Hat.HairDrawType draw_type = (Hat.HairDrawType)this.hat.Value.hairDrawType.Value;
				if (draw_type != Hat.HairDrawType.DrawObscuredHair)
				{
					if (draw_type == Hat.HairDrawType.HideHair)
					{
						return -1;
					}
				}
				else
				{
					switch (this.hair.Value)
					{
					case 1:
					case 5:
					case 6:
					case 9:
					case 11:
					case 17:
					case 20:
					case 23:
					case 24:
					case 25:
					case 27:
					case 28:
					case 29:
					case 30:
					case 32:
					case 33:
					case 34:
					case 36:
					case 39:
					case 41:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
						return this.hair.Value;
					case 3:
						return 11;
					case 18:
					case 19:
					case 21:
					case 31:
						return 23;
					case 42:
						return 46;
					case 48:
						return 6;
					case 49:
						return 52;
					case 50:
					case 51:
					case 52:
					case 53:
					case 54:
					case 55:
						return this.hair.Value;
					}
					if (this.hair.Value < 16)
					{
						return 7;
					}
					if (this.hair.Value < 100)
					{
						return 30;
					}
					return this.hair.Value;
				}
			}
			return this.hair.Value;
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x0005BDF0 File Offset: 0x00059FF0
		public void changeGender(bool male)
		{
			if (male)
			{
				this.Gender = Gender.Male;
				this.FarmerRenderer.textureName.Set(this.getTexture());
				this.FarmerRenderer.heightOffset.Set(0);
			}
			else
			{
				this.Gender = Gender.Female;
				this.FarmerRenderer.heightOffset.Set(4);
				this.FarmerRenderer.textureName.Set(this.getTexture());
			}
			this.changeShirt(this.shirt.Value);
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x0005BE70 File Offset: 0x0005A070
		public void changeFriendship(int amount, NPC n)
		{
			if (n == null)
			{
				return;
			}
			if (!(n is Child) && !n.IsVillager)
			{
				return;
			}
			if (amount > 0 && this.stats.Get("Book_Friendship") > 0U)
			{
				amount = (int)((float)amount * 1.1f);
			}
			if (amount > 0 && n.SpeaksDwarvish() && !this.canUnderstandDwarves)
			{
				return;
			}
			Friendship friendship;
			if (this.friendshipData.TryGetValue(n.Name, out friendship))
			{
				if (n.isDivorcedFrom(this) && amount > 0)
				{
					return;
				}
				if (n.Equals(this.getSpouse()))
				{
					amount = (int)((float)amount * 0.66f);
				}
				friendship.Points = Math.Max(0, Math.Min(friendship.Points + amount, (Utility.GetMaximumHeartsForCharacter(n) + 1) * 250 - 1));
				if (n.datable.Value && friendship.Points >= 2000 && !this.hasOrWillReceiveMail("Bouquet"))
				{
					Game1.addMailForTomorrow("Bouquet", false, false);
				}
				if (n.datable.Value && friendship.Points >= 2500 && !this.hasOrWillReceiveMail("SeaAmulet"))
				{
					Game1.addMailForTomorrow("SeaAmulet", false, false);
				}
				if (friendship.Points < 0)
				{
					friendship.Points = 0;
				}
			}
			else
			{
				Game1.debugOutput = "Tried to change friendship for a friend that wasn't there.";
			}
			Game1.stats.checkForFriendshipAchievements();
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0005BFC0 File Offset: 0x0005A1C0
		public bool knowsRecipe(string name)
		{
			if (name.EndsWith(" Recipe"))
			{
				name = name.Substring(0, name.Length - " Recipe".Length);
			}
			return this.craftingRecipes.Keys.Contains(name) || this.cookingRecipes.Keys.Contains(name);
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x0005C020 File Offset: 0x0005A220
		public Vector2 getUniformPositionAwayFromBox(int direction, int distance)
		{
			Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBox();
			switch (this.FacingDirection)
			{
			case 0:
				return new Vector2((float)bounds.Center.X, (float)(bounds.Y - distance));
			case 1:
				return new Vector2((float)(bounds.Right + distance), (float)bounds.Center.Y);
			case 2:
				return new Vector2((float)bounds.Center.X, (float)(bounds.Bottom + distance));
			case 3:
				return new Vector2((float)(bounds.X - distance), (float)bounds.Center.Y);
			default:
				return Vector2.Zero;
			}
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x0005C0CC File Offset: 0x0005A2CC
		public bool hasTalkedToFriendToday(string npcName)
		{
			Friendship friendship;
			return this.friendshipData.TryGetValue(npcName, out friendship) && friendship.TalkedToToday;
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x0005C0F4 File Offset: 0x0005A2F4
		public void talkToFriend(NPC n, int friendshipPointChange = 20)
		{
			Friendship friendship;
			if (this.friendshipData.TryGetValue(n.Name, out friendship) && !friendship.TalkedToToday)
			{
				this.changeFriendship(friendshipPointChange, n);
				friendship.TalkedToToday = true;
			}
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x0005C130 File Offset: 0x0005A330
		public void moveRaft(GameLocation currentLocation, GameTime time)
		{
			float raftInertia = 0.2f;
			if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
			{
				this.yVelocity = Math.Max(this.yVelocity - raftInertia, -3f + Math.Abs(this.xVelocity) / 2f);
				this.faceDirection(0);
			}
			if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
			{
				this.xVelocity = Math.Min(this.xVelocity + raftInertia, 3f - Math.Abs(this.yVelocity) / 2f);
				this.faceDirection(1);
			}
			if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
			{
				this.yVelocity = Math.Min(this.yVelocity + raftInertia, 3f - Math.Abs(this.xVelocity) / 2f);
				this.faceDirection(2);
			}
			if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
			{
				this.xVelocity = Math.Max(this.xVelocity - raftInertia, -3f + Math.Abs(this.yVelocity) / 2f);
				this.faceDirection(3);
			}
			Microsoft.Xna.Framework.Rectangle collidingBox = new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y + 64f + 16f), 64, 64);
			collidingBox.X += (int)Math.Ceiling((double)this.xVelocity);
			if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, this))
			{
				this.position.X += this.xVelocity;
			}
			collidingBox.X -= (int)Math.Ceiling((double)this.xVelocity);
			collidingBox.Y += (int)Math.Floor((double)this.yVelocity);
			if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, this))
			{
				this.position.Y += this.yVelocity;
			}
			if (this.xVelocity != 0f || this.yVelocity != 0f)
			{
				this.raftPuddleCounter -= time.ElapsedGameTime.Milliseconds;
				if (this.raftPuddleCounter <= 0)
				{
					this.raftPuddleCounter = 250;
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.xVelocity) + Math.Abs(this.yVelocity)) * 3f, 8, 0, new Vector2((float)collidingBox.X, (float)(collidingBox.Y - 64)), false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false));
					if (Game1.random.NextDouble() < 0.6)
					{
						Game1.playSound("wateringCan", null);
					}
					if (Game1.random.NextDouble() < 0.6)
					{
						this.raftBobCounter /= 2;
					}
				}
			}
			this.raftBobCounter -= time.ElapsedGameTime.Milliseconds;
			if (this.raftBobCounter <= 0)
			{
				this.raftBobCounter = Game1.random.Next(15, 28) * 100;
				if (this.yOffset <= 0f)
				{
					this.yOffset = 4f;
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.xVelocity) + Math.Abs(this.yVelocity)) * 3f, 8, 0, new Vector2((float)collidingBox.X, (float)(collidingBox.Y - 64)), false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false));
				}
				else
				{
					this.yOffset = 0f;
				}
			}
			if (this.xVelocity > 0f)
			{
				this.xVelocity = Math.Max(0f, this.xVelocity - raftInertia / 2f);
			}
			else if (this.xVelocity < 0f)
			{
				this.xVelocity = Math.Min(0f, this.xVelocity + raftInertia / 2f);
			}
			if (this.yVelocity > 0f)
			{
				this.yVelocity = Math.Max(0f, this.yVelocity - raftInertia / 2f);
				return;
			}
			if (this.yVelocity < 0f)
			{
				this.yVelocity = Math.Min(0f, this.yVelocity + raftInertia / 2f);
			}
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x0005C608 File Offset: 0x0005A808
		public void warpFarmer(Warp w, int warp_collide_direction)
		{
			if (w != null && !Game1.eventUp)
			{
				this.Halt();
				int target_x = w.TargetX;
				int target_y = w.TargetY;
				if (this.isRidingHorse())
				{
					if (warp_collide_direction != 0)
					{
						if (warp_collide_direction == 3)
						{
							Game1.nextFarmerWarpOffsetX = -1;
						}
					}
					else
					{
						Game1.nextFarmerWarpOffsetY = -1;
					}
				}
				Game1.warpFarmer(w.TargetName, target_x, target_y, w.flipFarmer.Value);
			}
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0005C668 File Offset: 0x0005A868
		public void warpFarmer(Warp w)
		{
			this.warpFarmer(w, -1);
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x0005C672 File Offset: 0x0005A872
		public void startToPassOut()
		{
			this.passOutEvent.Fire();
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x0005C680 File Offset: 0x0005A880
		private void performPassOut()
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (!this.swimming.Value && this.bathingClothes.Value)
			{
				this.bathingClothes.Value = false;
			}
			if (this.passedOut || this.FarmerSprite.isPassingOut())
			{
				return;
			}
			this.faceDirection(2);
			this.completelyStopAnimatingOrDoingAction();
			this.animateOnce(293);
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x0005C6F0 File Offset: 0x0005A8F0
		public static void passOutFromTired(Farmer who)
		{
			if (!who.IsLocalPlayer)
			{
				return;
			}
			if (who.IsSitting())
			{
				who.StopSitting(false);
			}
			if (who.isRidingHorse())
			{
				who.mount.dismount(false);
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			who.completelyStopAnimatingOrDoingAction();
			if (who.bathingClothes.Value)
			{
				who.changeOutOfSwimSuit();
			}
			who.swimming.Value = false;
			who.CanMove = false;
			who.FarmerSprite.setCurrentSingleFrame(5, 3000, false, false);
			who.FarmerSprite.PauseForSingleAnimation = true;
			if (!who.IsDedicatedPlayer && who == Game1.player && who.team.sleepAnnounceMode.Value != FarmerTeam.SleepAnnounceModes.Off)
			{
				string key = "PassedOut";
				string possibleLocationKey = "PassedOut_" + who.currentLocation.Name.TrimEnd(new char[]
				{
					'0',
					'1',
					'2',
					'3',
					'4',
					'5',
					'6',
					'7',
					'8',
					'9'
				});
				if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:Chat_" + possibleLocationKey, true) != null)
				{
					Game1.multiplayer.globalChatInfoMessage(possibleLocationKey, new string[]
					{
						who.displayName
					});
				}
				else
				{
					int key_index = 0;
					for (int i = 0; i < 2; i++)
					{
						if (Game1.random.NextDouble() < 0.25)
						{
							key_index++;
						}
					}
					Game1.multiplayer.globalChatInfoMessage(key + key_index.ToString(), new string[]
					{
						who.displayName
					});
				}
			}
			FarmHouse farmhouse = Game1.currentLocation as FarmHouse;
			if (farmhouse != null)
			{
				who.lastSleepLocation.Value = farmhouse.NameOrUniqueName;
				who.lastSleepPoint.Value = farmhouse.GetPlayerBedSpot();
			}
			Game1.multiplayer.sendPassoutRequest();
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0005C8A8 File Offset: 0x0005AAA8
		public static void performPassoutWarp(Farmer who, string bed_location_name, Point bed_point, bool has_bed)
		{
			Farmer.<>c__DisplayClass672_0 CS$<>8__locals1 = new Farmer.<>c__DisplayClass672_0();
			CS$<>8__locals1.who = who;
			CS$<>8__locals1.passOutLocation = CS$<>8__locals1.who.currentLocationRef.Value;
			Vector2 bed = Utility.PointToVector2(bed_point) * 64f;
			CS$<>8__locals1.bed_tile = new Vector2((float)((int)bed.X / 64), (float)((int)bed.Y / 64));
			CS$<>8__locals1.bed_sleep_position = bed;
			if (!CS$<>8__locals1.who.isInBed.Value)
			{
				LocationRequest locationRequest = Game1.getLocationRequest(bed_location_name, false);
				Game1.warpFarmer(locationRequest, (int)bed.X / 64, (int)bed.Y / 64, 2);
				locationRequest.OnWarp += CS$<>8__locals1.<performPassoutWarp>g__ContinuePassOut|0;
				CS$<>8__locals1.who.FarmerSprite.setCurrentSingleFrame(5, 3000, false, false);
				CS$<>8__locals1.who.FarmerSprite.PauseForSingleAnimation = true;
				return;
			}
			CS$<>8__locals1.<performPassoutWarp>g__ContinuePassOut|0();
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x0005C986 File Offset: 0x0005AB86
		public static void doSleepEmote(Farmer who)
		{
			who.doEmote(24);
			who.yJumpVelocity = -2f;
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x0005C99C File Offset: 0x0005AB9C
		public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			if (this.mount != null && !this.mount.dismounting.Value)
			{
				return this.mount.GetBoundingBox();
			}
			Vector2 position = base.Position;
			return new Microsoft.Xna.Framework.Rectangle((int)position.X + 8, (int)position.Y + this.Sprite.getHeight() - 32, 48, 32);
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x0005CA00 File Offset: 0x0005AC00
		public string getPetName()
		{
			foreach (NPC i in Game1.getFarm().characters)
			{
				if (i is Pet)
				{
					return i.Name;
				}
			}
			foreach (Farmer who in Game1.getAllFarmers())
			{
				foreach (NPC j in Utility.getHomeOfFarmer(who).characters)
				{
					if (j is Pet)
					{
						return j.Name;
					}
				}
			}
			return "your pet";
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0005CAF4 File Offset: 0x0005ACF4
		public Pet getPet()
		{
			foreach (NPC npc in Game1.getFarm().characters)
			{
				Pet pet = npc as Pet;
				if (pet != null)
				{
					return pet;
				}
			}
			foreach (Farmer who in Game1.getAllFarmers())
			{
				foreach (NPC npc2 in Utility.getHomeOfFarmer(who).characters)
				{
					Pet pet2 = npc2 as Pet;
					if (pet2 != null)
					{
						return pet2;
					}
				}
			}
			return null;
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0005CBD8 File Offset: 0x0005ADD8
		public string getPetDisplayName()
		{
			foreach (NPC i in Game1.getFarm().characters)
			{
				if (i is Pet)
				{
					return i.displayName;
				}
			}
			foreach (Farmer who in Game1.getAllFarmers())
			{
				foreach (NPC j in Utility.getHomeOfFarmer(who).characters)
				{
					if (j is Pet)
					{
						return j.displayName;
					}
				}
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1972");
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x0005CCD4 File Offset: 0x0005AED4
		public bool hasPet()
		{
			using (List<NPC>.Enumerator enumerator = Game1.getFarm().characters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is Pet)
					{
						return true;
					}
				}
			}
			foreach (Farmer who in Game1.getAllFarmers())
			{
				using (List<NPC>.Enumerator enumerator = Utility.getHomeOfFarmer(who).characters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current is Pet)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x0005CDB0 File Offset: 0x0005AFB0
		public void UpdateClothing()
		{
			this.FarmerRenderer.MarkSpriteDirty();
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x0005CDC0 File Offset: 0x0005AFC0
		public bool IsOverridingPants(out string id, out Color? color)
		{
			if (this.pants.Value != null && this.pants.Value != "-1")
			{
				id = this.pants.Value;
				color = new Color?(this.pantsColor.Value);
				return true;
			}
			id = null;
			color = null;
			return false;
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x0005CE24 File Offset: 0x0005B024
		public bool CanDyePants()
		{
			Clothing value = this.pantsItem.Value;
			return ((value != null) ? new bool?(value.dyeable.Value) : null) ?? false;
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0005CE70 File Offset: 0x0005B070
		public void GetDisplayPants(out Texture2D texture, out int spriteIndex)
		{
			string id;
			Color? color;
			if (this.IsOverridingPants(out id, out color))
			{
				ParsedItemData itemData = ItemRegistry.GetData("(P)" + id);
				if (itemData != null && !itemData.IsErrorItem)
				{
					texture = itemData.GetTexture();
					spriteIndex = itemData.SpriteIndex;
					return;
				}
			}
			if (this.pantsItem.Value != null)
			{
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem(this.pantsItem.Value.QualifiedItemId);
				if (data != null && !data.IsErrorItem)
				{
					texture = data.GetTexture();
					spriteIndex = this.pantsItem.Value.indexInTileSheet.Value;
					return;
				}
			}
			texture = FarmerRenderer.pantsTexture;
			spriteIndex = 14;
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0005CF10 File Offset: 0x0005B110
		public string GetPantsId()
		{
			string id;
			Color? color;
			if (this.IsOverridingPants(out id, out color))
			{
				return id;
			}
			Clothing value = this.pantsItem.Value;
			return ((value != null) ? value.ItemId : null) ?? "14";
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0005CF4C File Offset: 0x0005B14C
		public int GetPantsIndex()
		{
			Texture2D texture2D;
			int index;
			this.GetDisplayPants(out texture2D, out index);
			return index;
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0005CF64 File Offset: 0x0005B164
		public bool IsOverridingShirt(out string id)
		{
			if (this.shirt.Value != null && this.shirt.Value != "-1")
			{
				id = this.shirt.Value;
				return true;
			}
			id = null;
			return false;
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x0005CFA0 File Offset: 0x0005B1A0
		public bool CanDyeShirt()
		{
			Clothing value = this.shirtItem.Value;
			return ((value != null) ? new bool?(value.dyeable.Value) : null) ?? false;
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x0005CFEC File Offset: 0x0005B1EC
		public void GetDisplayShirt(out Texture2D texture, out int spriteIndex)
		{
			string id;
			if (this.IsOverridingShirt(out id))
			{
				ParsedItemData itemData = ItemRegistry.GetData("(S)" + id);
				if (itemData != null && !itemData.IsErrorItem)
				{
					texture = itemData.GetTexture();
					spriteIndex = itemData.SpriteIndex;
					return;
				}
			}
			if (this.shirtItem.Value != null)
			{
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem(this.shirtItem.Value.QualifiedItemId);
				if (data != null && !data.IsErrorItem)
				{
					texture = data.GetTexture();
					spriteIndex = this.shirtItem.Value.indexInTileSheet.Value;
					return;
				}
			}
			texture = FarmerRenderer.shirtsTexture;
			spriteIndex = (this.IsMale ? 209 : 41);
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0005D098 File Offset: 0x0005B298
		public string GetShirtId()
		{
			string id;
			if (this.IsOverridingShirt(out id))
			{
				return id;
			}
			if (this.shirtItem.Value != null)
			{
				return this.shirtItem.Value.ItemId;
			}
			if (!this.IsMale)
			{
				return "1041";
			}
			return "1209";
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0005D0E4 File Offset: 0x0005B2E4
		public int GetShirtIndex()
		{
			Texture2D texture2D;
			int index;
			this.GetDisplayShirt(out texture2D, out index);
			return index;
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0005D0FC File Offset: 0x0005B2FC
		public bool ShirtHasSleeves()
		{
			string itemId;
			if (!this.IsOverridingShirt(out itemId))
			{
				Clothing value = this.shirtItem.Value;
				itemId = ((value != null) ? value.ItemId : null);
			}
			ShirtData data;
			return itemId == null || !Game1.shirtData.TryGetValue(itemId, out data) || data.HasSleeves;
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x0005D148 File Offset: 0x0005B348
		public Color GetShirtColor()
		{
			string id;
			ShirtData shirtData;
			if (this.IsOverridingShirt(out id) && Game1.shirtData.TryGetValue(id, out shirtData))
			{
				if (shirtData.IsPrismatic)
				{
					return Utility.GetPrismaticColor(0, 1f);
				}
				Color? color = Utility.StringToColor(shirtData.DefaultColor);
				if (color == null)
				{
					return Color.White;
				}
				return color.GetValueOrDefault();
			}
			else
			{
				if (this.shirtItem.Value == null)
				{
					return this.DEFAULT_SHIRT_COLOR;
				}
				if (this.shirtItem.Value.isPrismatic.Value)
				{
					return Utility.GetPrismaticColor(0, 1f);
				}
				return this.shirtItem.Value.clothesColor.Value;
			}
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x0005D1F4 File Offset: 0x0005B3F4
		public Color GetPantsColor()
		{
			string text;
			Color? color;
			if (this.IsOverridingPants(out text, out color))
			{
				Color? color2 = color;
				if (color2 == null)
				{
					return Color.White;
				}
				return color2.GetValueOrDefault();
			}
			else
			{
				if (this.pantsItem.Value == null)
				{
					return Color.White;
				}
				if (this.pantsItem.Value.isPrismatic.Value)
				{
					return Utility.GetPrismaticColor(0, 1f);
				}
				return this.pantsItem.Value.clothesColor.Value;
			}
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x0005D274 File Offset: 0x0005B474
		public bool movedDuringLastTick()
		{
			return !base.Position.Equals(this.lastPosition);
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x0005D298 File Offset: 0x0005B498
		public int CompareTo(object obj)
		{
			return ((Farmer)obj).saveTime - this.saveTime;
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0005D2AC File Offset: 0x0005B4AC
		public virtual void SetOnBridge(bool val)
		{
			if (this.onBridge.Value != val)
			{
				this.onBridge.Value = val;
				if (this.onBridge.Value)
				{
					this.showNotCarrying();
				}
			}
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x0005D2DC File Offset: 0x0005B4DC
		public float getDrawLayer()
		{
			if (this.onBridge.Value)
			{
				return (float)base.StandingPixel.Y / 10000f + this.drawLayerDisambiguator + 0.0256f;
			}
			if (this.IsSitting() && this.mapChairSitPosition.Value.X != -1f && this.mapChairSitPosition.Value.Y != -1f)
			{
				Vector2 sit_position = this.mapChairSitPosition.Value;
				return (sit_position.Y + 1f) * 64f / 10000f;
			}
			return (float)base.StandingPixel.Y / 10000f + this.drawLayerDisambiguator;
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x0005D38C File Offset: 0x0005B58C
		public override void draw(SpriteBatch b)
		{
			if (base.currentLocation == null || (!base.currentLocation.Equals(Game1.currentLocation) && !this.IsLocalPlayer && !Game1.currentLocation.IsTemporary && !this.isFakeEventActor))
			{
				return;
			}
			if (this.hidden.Value && (base.currentLocation.currentEvent == null || this != base.currentLocation.currentEvent.farmer) && (!this.IsLocalPlayer || Game1.locationRequest == null))
			{
				return;
			}
			if (this.viewingLocation.Value != null && this.IsLocalPlayer)
			{
				return;
			}
			float draw_layer = this.getDrawLayer();
			if (this.isRidingHorse())
			{
				this.mount.SyncPositionToRider();
				this.mount.draw(b);
				if (this.FacingDirection == 3 || this.FacingDirection == 1)
				{
					draw_layer += 0.0016f;
				}
			}
			float layerDepth = FarmerRenderer.GetLayerDepth(0f, FarmerRenderer.FarmerSpriteLayers.MAX, false);
			Vector2 origin = new Vector2(this.xOffset, (this.yOffset + 128f - (float)(this.GetBoundingBox().Height / 2)) / 4f + 4f);
			Point standingPixel = base.StandingPixel;
			Tile shadowTile = Game1.currentLocation.Map.RequireLayer("Buildings").PickTile(new Location(standingPixel.X, standingPixel.Y), Game1.viewport.Size);
			float glow_offset = layerDepth * 1f;
			float shadow_offset = layerDepth * 2f;
			if (this.isGlowing)
			{
				if (this.coloredBorder)
				{
					b.Draw(this.Sprite.Texture, new Vector2(base.getLocalPosition(Game1.viewport).X - 4f, base.getLocalPosition(Game1.viewport).Y - 4f), new Microsoft.Xna.Framework.Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, draw_layer + glow_offset);
				}
				else
				{
					this.FarmerRenderer.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport) + this.jitter + new Vector2(0f, (float)this.yJumpOffset), origin, draw_layer + glow_offset, this.glowingColor * this.glowingTransparency, this.rotation, this);
				}
			}
			if (!(((shadowTile != null) ? new bool?(shadowTile.TileIndexProperties.ContainsKey("Shadow")) : null) ?? false))
			{
				if (this.IsSitting() || !Game1.shouldTimePass(false) || !this.temporarilyInvincible || !this.flashDuringThisTemporaryInvincibility || this.temporaryInvincibilityTimer % 100 < 50)
				{
					this.farmerRenderer.Value.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport) + this.jitter + new Vector2(0f, (float)this.yJumpOffset), origin, draw_layer, Color.White, this.rotation, this);
				}
			}
			else
			{
				this.farmerRenderer.Value.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport), origin, draw_layer, Color.White, this.rotation, this);
				this.farmerRenderer.Value.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport), origin, draw_layer + shadow_offset, Color.Black * 0.25f, this.rotation, this);
			}
			if (this.isRafting)
			{
				b.Draw(Game1.toolSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2(0f, this.yOffset), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, 1, -1, -1)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, FarmerRenderer.GetLayerDepth(draw_layer, FarmerRenderer.FarmerSpriteLayers.ToolUp, false));
			}
			if (Game1.activeClickableMenu == null && !Game1.eventUp && this.IsLocalPlayer && this.CurrentTool != null && (Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.options.alwaysShowToolHitLocation) && this.CurrentTool.doesShowTileLocationMarker() && (!Game1.options.hideToolHitLocationWhenInMotion || !this.isMoving()))
			{
				Vector2 mouse_position = Utility.PointToVector2(Game1.getMousePosition()) + new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
				Vector2 draw_location = Game1.GlobalToLocal(Game1.viewport, Utility.clampToTile(this.GetToolLocation(mouse_position, false)));
				b.Draw(Game1.mouseCursors, draw_location, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29, -1, -1)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, draw_location.Y / 10000f);
			}
			if (base.IsEmoting)
			{
				Vector2 emotePosition = base.getLocalPosition(Game1.viewport);
				emotePosition.Y -= 160f;
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
			}
			if (this.ActiveObject != null && this.IsCarrying())
			{
				Game1.drawPlayerHeldObject(this);
			}
			SparklingText sparklingText = this.sparklingText;
			if (sparklingText != null)
			{
				sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(32f - this.sparklingText.textWidth / 2f, -128f)));
			}
			if (this.UsingTool && this.CurrentTool != null)
			{
				Game1.drawTool(this);
			}
			foreach (Companion companion in this.companions)
			{
				companion.Draw(b);
			}
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x0005D9CC File Offset: 0x0005BBCC
		public virtual void DrawUsername(SpriteBatch b)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			if (Game1.multiplayer == null || LocalMultiplayer.IsLocalMultiplayer(true))
			{
				return;
			}
			if (this.usernameDisplayTime <= 0f)
			{
				return;
			}
			string username = Game1.multiplayer.getUserName(this.UniqueMultiplayerID);
			if (username != null)
			{
				Vector2 string_size = Game1.smallFont.MeasureString(username);
				Vector2 draw_origin = base.getLocalPosition(Game1.viewport) + new Vector2(32f, -104f) - string_size / 2f;
				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						if (x != 0 || y != 0)
						{
							b.DrawString(Game1.smallFont, username, draw_origin + new Vector2((float)x, (float)y) * 2f, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
						}
					}
				}
				b.DrawString(Game1.smallFont, username, draw_origin, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0005DADC File Offset: 0x0005BCDC
		public static void drinkGlug(Farmer who)
		{
			Color c = Color.LightBlue;
			if (who.itemToEat != null)
			{
				string text = ArgUtility.SplitBySpace(who.itemToEat.Name).Last<string>();
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
						if (!(text == "Tea"))
						{
							goto IL_21F;
						}
						goto IL_1EB;
					case 4:
					{
						char c2 = text[0];
						if (c2 <= 'C')
						{
							if (c2 != 'B')
							{
								if (c2 != 'C')
								{
									goto IL_21F;
								}
								if (!(text == "Cola"))
								{
									goto IL_21F;
								}
							}
							else
							{
								if (!(text == "Beer"))
								{
									goto IL_21F;
								}
								c = Color.Orange;
								goto IL_21F;
							}
						}
						else if (c2 != 'M')
						{
							if (c2 != 'S')
							{
								if (c2 != 'W')
								{
									goto IL_21F;
								}
								if (!(text == "Wine"))
								{
									goto IL_21F;
								}
								c = Color.Purple;
								goto IL_21F;
							}
							else
							{
								if (!(text == "Soup"))
								{
									goto IL_21F;
								}
								c = Color.LightGreen;
								goto IL_21F;
							}
						}
						else
						{
							if (!(text == "Milk"))
							{
								goto IL_21F;
							}
							c = Color.White;
							goto IL_21F;
						}
						break;
					}
					case 5:
					{
						char c2 = text[0];
						if (c2 != 'J')
						{
							if (c2 != 'T')
							{
								goto IL_21F;
							}
							if (!(text == "Tonic"))
							{
								goto IL_21F;
							}
							c = Color.Red;
							goto IL_21F;
						}
						else
						{
							if (!(text == "Juice"))
							{
								goto IL_21F;
							}
							goto IL_1EB;
						}
						break;
					}
					case 6:
					{
						char c2 = text[0];
						if (c2 != 'C')
						{
							if (c2 != 'R')
							{
								goto IL_21F;
							}
							if (!(text == "Remedy"))
							{
								goto IL_21F;
							}
							c = Color.LimeGreen;
							goto IL_21F;
						}
						else if (!(text == "Coffee"))
						{
							goto IL_21F;
						}
						break;
					}
					case 7:
					case 9:
						goto IL_21F;
					case 8:
						if (!(text == "Espresso"))
						{
							goto IL_21F;
						}
						break;
					case 10:
						if (!(text == "Mayonnaise"))
						{
							goto IL_21F;
						}
						c = ((who.itemToEat.Name == "Void Mayonnaise") ? Color.Black : Color.White);
						goto IL_21F;
					default:
						goto IL_21F;
					}
					c = new Color(46, 20, 0);
					goto IL_21F;
					IL_1EB:
					c = Color.LightGreen;
				}
			}
			IL_21F:
			if (Game1.currentLocation == who.currentLocation)
			{
				Object o = who.itemToEat as Object;
				Game1.playSound((o != null && o.preserve.Value.GetValueOrDefault() == Object.PreserveType.Pickle) ? "eat" : "gulp", null);
			}
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2((float)(32 + Game1.random.Next(-2, 3) * 4), -48f), false, false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0.04f, c, 5f, 0f, 0f, 0f, false)
			{
				acceleration = new Vector2(0f, 0.5f)
			});
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0005DE00 File Offset: 0x0005C000
		public void handleDisconnect()
		{
			if (base.currentLocation != null)
			{
				Ring value = this.rightRing.Value;
				if (value != null)
				{
					value.onLeaveLocation(this, base.currentLocation);
				}
				Ring value2 = this.leftRing.Value;
				if (value2 != null)
				{
					value2.onLeaveLocation(this, base.currentLocation);
				}
			}
			this.UnapplyAllTrinketEffects();
			this.disconnectDay.Value = (int)Game1.stats.DaysPlayed;
			NetFieldBase<string, NetString> netFieldBase = this.disconnectLocation;
			GameLocation currentLocation = base.currentLocation;
			netFieldBase.Value = (((currentLocation != null) ? currentLocation.NameOrUniqueName : null) ?? "");
			this.disconnectPosition.Value = base.Position;
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x0005DEA4 File Offset: 0x0005C0A4
		public bool isDivorced()
		{
			using (NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>.ValuesCollection.Enumerator enumerator = this.friendshipData.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsDivorced())
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x0005DF08 File Offset: 0x0005C108
		public void wipeExMemories()
		{
			foreach (string npcName in this.friendshipData.Keys)
			{
				Friendship friendship = this.friendshipData[npcName];
				if (friendship.IsDivorced())
				{
					friendship.Clear();
					NPC i = Game1.getCharacterFromName(npcName, true, false);
					if (i != null)
					{
						i.CurrentDialogue.Clear();
						i.CurrentDialogue.Push(i.TryGetDialogue("WipedMemory") ?? new Dialogue(i, "Strings\\Characters:WipedMemory", false));
						Game1.stats.Increment("exMemoriesWiped", 1U);
					}
				}
			}
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x0005DFCC File Offset: 0x0005C1CC
		public void getRidOfChildren()
		{
			FarmHouse farmhouse = Utility.getHomeOfFarmer(this);
			for (int i = farmhouse.characters.Count - 1; i >= 0; i--)
			{
				Child child = farmhouse.characters[i] as Child;
				if (child != null)
				{
					BedFurniture childBed = farmhouse.GetChildBed((int)child.Gender);
					if (childBed != null)
					{
						childBed.mutex.ReleaseLock();
					}
					if (child.hat.Value != null)
					{
						Hat hat = child.hat.Value;
						child.hat.Value = null;
						this.team.returnedDonations.Add(hat);
						this.team.newLostAndFoundItems.Value = true;
					}
					farmhouse.characters.RemoveAt(i);
					Game1.stats.Increment("childrenTurnedToDoves", 1U);
				}
			}
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0005E096 File Offset: 0x0005C296
		public void animateOnce(int whichAnimation)
		{
			this.FarmerSprite.animateOnce(whichAnimation, 100f, 6);
			this.CanMove = false;
		}

		// Token: 0x0600092F RID: 2351 RVA: 0x0005E0B4 File Offset: 0x0005C2B4
		public static void showItemIntake(Farmer who)
		{
			TemporaryAnimatedSprite tempSprite = null;
			Object grabbedObj = who.mostRecentlyGrabbedItem as Object;
			Object toShow = (grabbedObj == null) ? ((who.ActiveObject == null) ? null : who.ActiveObject) : grabbedObj;
			if (toShow == null)
			{
				return;
			}
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(toShow.QualifiedItemId);
			string textureName = dataOrErrorItem.TextureName;
			Microsoft.Xna.Framework.Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
			switch (who.FacingDirection)
			{
			case 0:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -32f), false, false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -43f), false, false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -128f), false, false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), false, false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), false, false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f, false);
					break;
				}
				break;
			case 1:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(28f, -64f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(24f, -72f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(4f, -128f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f, false);
					break;
				}
				break;
			case 2:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -32f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -43f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -128f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f, false);
					break;
				}
				break;
			case 3:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-32f, -64f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-28f, -76f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-16f, -128f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), false, false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f, false);
					break;
				}
				break;
			}
			string qualifiedItemId = toShow.QualifiedItemId;
			Object activeObject = who.ActiveObject;
			if (qualifiedItemId == ((activeObject != null) ? activeObject.QualifiedItemId : null) && who.FarmerSprite.currentAnimationIndex == 5)
			{
				tempSprite = null;
			}
			if (tempSprite != null)
			{
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
			ColoredObject coloredObj = who.mostRecentlyGrabbedItem as ColoredObject;
			if (coloredObj != null && tempSprite != null)
			{
				Microsoft.Xna.Framework.Rectangle coloredSourceRect = ItemRegistry.GetDataOrErrorItem(coloredObj.QualifiedItemId).GetSourceRect(1, null);
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, coloredSourceRect, tempSprite.interval, 1, 0, tempSprite.Position, false, false, tempSprite.layerDepth + 0.0001f, tempSprite.alphaFade, coloredObj.color.Value, 4f, tempSprite.scaleChange, 0f, 0f, false));
			}
			if (who.FarmerSprite.currentAnimationIndex == 5)
			{
				who.Halt();
				who.FarmerSprite.CurrentAnimation = null;
			}
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x0005EAF8 File Offset: 0x0005CCF8
		public virtual void showSwordSwipe(Farmer who)
		{
			TemporaryAnimatedSprite tempSprite = null;
			Vector2 actionTile = who.GetToolLocation(true);
			bool dagger = false;
			MeleeWeapon weapon = who.CurrentTool as MeleeWeapon;
			if (weapon != null)
			{
				dagger = (weapon.type.Value == 1);
				if (!dagger)
				{
					weapon.DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
				}
			}
			int min_swipe_interval = 20;
			switch (who.FacingDirection)
			{
			case 0:
			{
				int currentAnimationIndex = who.FarmerSprite.currentAnimationIndex;
				if (currentAnimationIndex != 0)
				{
					if (currentAnimationIndex != 1)
					{
						if (currentAnimationIndex == 5)
						{
							who.yVelocity = -0.3f;
							tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(0f, -32f) * 4f, false, 0.07f, Color.White)
							{
								scale = 4f,
								animationLength = 1,
								interval = (float)Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
								alpha = 0.5f,
								rotation = 3.926991f
							};
						}
					}
					else
					{
						who.yVelocity = (dagger ? -0.5f : 0.5f);
					}
				}
				else if (dagger)
				{
					who.yVelocity = 0.6f;
				}
				break;
			}
			case 1:
			{
				int currentAnimationIndex = who.FarmerSprite.currentAnimationIndex;
				if (currentAnimationIndex != 0)
				{
					if (currentAnimationIndex != 1)
					{
						if (currentAnimationIndex == 5)
						{
							who.xVelocity = -0.3f;
							tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(4f, -12f) * 4f, false, 0.07f, Color.White)
							{
								scale = 4f,
								animationLength = 1,
								interval = (float)Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
								alpha = 0.5f
							};
						}
					}
					else
					{
						who.xVelocity = (dagger ? -0.5f : 0.5f);
					}
				}
				else if (dagger)
				{
					who.xVelocity = 0.6f;
				}
				break;
			}
			case 2:
			{
				int currentAnimationIndex = who.FarmerSprite.currentAnimationIndex;
				if (currentAnimationIndex != 0)
				{
					if (currentAnimationIndex != 1)
					{
						if (currentAnimationIndex == 5)
						{
							who.yVelocity = 0.3f;
							tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(503, 256, 42, 17), who.Position + new Vector2(-16f, -2f) * 4f, false, 0.07f, Color.White)
							{
								scale = 4f,
								animationLength = 1,
								interval = (float)Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
								alpha = 0.5f,
								layerDepth = (who.Position.Y + 64f) / 10000f
							};
						}
					}
					else
					{
						who.yVelocity = (dagger ? 0.5f : -0.5f);
					}
				}
				else if (dagger)
				{
					who.yVelocity = -0.6f;
				}
				break;
			}
			case 3:
			{
				int currentAnimationIndex = who.FarmerSprite.currentAnimationIndex;
				if (currentAnimationIndex != 0)
				{
					if (currentAnimationIndex != 1)
					{
						if (currentAnimationIndex == 5)
						{
							who.xVelocity = 0.3f;
							tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(-15f, -12f) * 4f, false, 0.07f, Color.White)
							{
								scale = 4f,
								animationLength = 1,
								interval = (float)Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
								flipped = true,
								alpha = 0.5f
							};
						}
					}
					else
					{
						who.xVelocity = (dagger ? 0.5f : -0.5f);
					}
				}
				else if (dagger)
				{
					who.xVelocity = -0.6f;
				}
				break;
			}
			}
			if (tempSprite != null)
			{
				Tool currentTool = who.CurrentTool;
				if (((currentTool != null) ? currentTool.QualifiedItemId : null) == "(W)4")
				{
					tempSprite.color = Color.HotPink;
				}
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x0005EF84 File Offset: 0x0005D184
		public static void showToolSwipeEffect(Farmer who)
		{
			if (!(who.CurrentTool is WateringCan))
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(18, who.Position + new Vector2(0f, -132f), Color.White, 4, false, (who.stamina <= 0f) ? 100f : 50f, 0, 64, 1f, 64, 0)
					{
						layerDepth = (float)(who.StandingPixel.Y - 9) / 10000f
					});
					break;
				case 1:
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(20f, -132f), Color.White, 4, false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128, 0)
					{
						layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
					});
					return;
				case 2:
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(19, who.Position + new Vector2(-4f, -128f), Color.White, 4, false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128, 0)
					{
						layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
					});
					return;
				case 3:
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(-92f, -132f), Color.White, 4, true, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128, 0)
					{
						layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
					});
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x0005F1B6 File Offset: 0x0005D3B6
		public static void canMoveNow(Farmer who)
		{
			who.CanMove = true;
			who.UsingTool = false;
			who.usingSlingshot = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.yVelocity = 0f;
			who.xVelocity = 0f;
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x0005F1EF File Offset: 0x0005D3EF
		public void FireTool()
		{
			this.fireToolEvent.Fire();
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x0005F1FC File Offset: 0x0005D3FC
		public void synchronizedJump(float velocity)
		{
			if (this.IsLocalPlayer)
			{
				this.synchronizedJumpEvent.Fire(velocity);
				this.synchronizedJumpEvent.Poll();
			}
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x0005F21D File Offset: 0x0005D41D
		protected void performSynchronizedJump(float velocity)
		{
			this.yJumpVelocity = velocity;
			this.yJumpOffset = -1;
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x0005F22D File Offset: 0x0005D42D
		private void performFireTool()
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			Tool currentTool = this.CurrentTool;
			if (currentTool == null)
			{
				return;
			}
			currentTool.leftClick(this);
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x0005F250 File Offset: 0x0005D450
		public static void useTool(Farmer who)
		{
			if (who.toolOverrideFunction != null)
			{
				who.toolOverrideFunction(who);
				return;
			}
			if (who.CurrentTool != null)
			{
				float oldStamina = who.stamina;
				if (who.IsLocalPlayer)
				{
					who.CurrentTool.DoFunction(who.currentLocation, (int)who.GetToolLocation(false).X, (int)who.GetToolLocation(false).Y, 1, who);
				}
				who.lastClick = Vector2.Zero;
				who.checkForExhaustion(oldStamina);
			}
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0005F2C8 File Offset: 0x0005D4C8
		public void BeginUsingTool()
		{
			this.beginUsingToolEvent.Fire();
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0005F2D8 File Offset: 0x0005D4D8
		private void performBeginUsingTool()
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (this.CurrentTool == null)
			{
				return;
			}
			this.CanMove = false;
			this.UsingTool = true;
			this.canReleaseTool = true;
			this.CurrentTool.beginUsing(base.currentLocation, (int)this.lastClick.X, (int)this.lastClick.Y, this);
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x0005F33C File Offset: 0x0005D53C
		public void EndUsingTool()
		{
			if (this == Game1.player)
			{
				this.endUsingToolEvent.Fire();
				return;
			}
			this.performEndUsingTool();
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x0005F358 File Offset: 0x0005D558
		private void performEndUsingTool()
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			Tool currentTool = this.CurrentTool;
			if (currentTool == null)
			{
				return;
			}
			currentTool.endUsing(base.currentLocation, this);
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x0005F380 File Offset: 0x0005D580
		public void checkForExhaustion(float oldStamina)
		{
			if (this.stamina <= 0f && oldStamina > 0f)
			{
				if (!this.exhausted.Value && this.IsLocalPlayer)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1986"));
				}
				this.setRunning(false, false);
				this.doEmote(36);
			}
			else if (this.stamina <= 15f && oldStamina > 15f && this.IsLocalPlayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1987"));
			}
			if (this.stamina <= 0f)
			{
				this.exhausted.Value = true;
			}
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0005F428 File Offset: 0x0005D628
		public void setMoving(byte command)
		{
			if (command <= 16)
			{
				switch (command)
				{
				case 1:
					if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(0) && !this.movementDirections.Contains(2))
					{
						this.movementDirections.Insert(0, 0);
					}
					break;
				case 2:
					if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(1) && !this.movementDirections.Contains(3))
					{
						this.movementDirections.Insert(0, 1);
					}
					break;
				case 3:
					break;
				case 4:
					if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(2) && !this.movementDirections.Contains(0))
					{
						this.movementDirections.Insert(0, 2);
					}
					break;
				default:
					if (command != 8)
					{
						if (command == 16)
						{
							this.setRunning(true, false);
						}
					}
					else if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(3) && !this.movementDirections.Contains(1))
					{
						this.movementDirections.Insert(0, 3);
					}
					break;
				}
			}
			else
			{
				switch (command)
				{
				case 33:
					this.movementDirections.Remove(0);
					break;
				case 34:
					this.movementDirections.Remove(1);
					break;
				case 35:
					break;
				case 36:
					this.movementDirections.Remove(2);
					break;
				default:
					if (command != 40)
					{
						if (command == 48)
						{
							this.setRunning(false, false);
						}
					}
					else
					{
						this.movementDirections.Remove(3);
					}
					break;
				}
			}
			if ((command & 64) == 64)
			{
				this.Halt();
				this.running = false;
			}
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x0005F600 File Offset: 0x0005D800
		public void toolPowerIncrease()
		{
			if (this.CurrentTool is Pan)
			{
				return;
			}
			if (this.toolPower.Value == 0)
			{
				this.toolPitchAccumulator = 0;
			}
			NetInt netInt = this.toolPower;
			int value = netInt.Value;
			netInt.Value = value + 1;
			if (this.CurrentTool is Pickaxe && this.toolPower.Value == 1)
			{
				this.toolPower.Value += 2;
			}
			Color powerUpColor = Color.White;
			int frameOffset = (this.FacingDirection == 0) ? 4 : ((this.FacingDirection == 2) ? 2 : 0);
			switch (this.toolPower.Value)
			{
			case 1:
				powerUpColor = Color.Orange;
				if (!(this.CurrentTool is WateringCan))
				{
					this.FarmerSprite.CurrentFrame = 72 + frameOffset;
				}
				this.jitterStrength = 0.25f;
				break;
			case 2:
				powerUpColor = Color.LightSteelBlue;
				if (!(this.CurrentTool is WateringCan))
				{
					FarmerSprite farmerSprite = this.FarmerSprite;
					int currentFrame = farmerSprite.CurrentFrame;
					farmerSprite.CurrentFrame = currentFrame + 1;
				}
				this.jitterStrength = 0.5f;
				break;
			case 3:
				powerUpColor = Color.Gold;
				this.jitterStrength = 1f;
				break;
			case 4:
				powerUpColor = Color.Violet;
				this.jitterStrength = 2f;
				break;
			case 5:
				powerUpColor = Color.BlueViolet;
				this.jitterStrength = 3f;
				break;
			}
			int xAnimation = (this.FacingDirection == 1) ? 40 : ((this.FacingDirection == 3) ? -40 : ((this.FacingDirection == 2) ? 32 : 0));
			int yAnimation = 192;
			if (this.CurrentTool is WateringCan)
			{
				switch (this.FacingDirection)
				{
				case 1:
					xAnimation = -48;
					break;
				case 2:
					xAnimation = 0;
					break;
				case 3:
					xAnimation = 48;
					break;
				}
				yAnimation = 128;
			}
			int standingY = base.StandingPixel.Y;
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(21, base.Position - new Vector2((float)xAnimation, (float)yAnimation), powerUpColor, 8, false, 70f, 0, 64, (float)standingY / 10000f + 0.005f, 128, 0));
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, 64, 64), 50f, 4, 0, base.Position - new Vector2((float)((this.FacingDirection == 1) ? 0 : -64), 128f), false, this.FacingDirection == 1, (float)standingY / 10000f, 0.01f, Color.White, 1f, 0f, 0f, 0f, false));
			int pitch = Utility.CreateRandom((double)Game1.dayOfMonth, (double)base.Position.X * 1000.0, (double)base.Position.Y, 0.0, 0.0).Next(12, 16) * 100 + this.toolPower.Value * 100;
			Game1.playSound("toolCharge", new int?(pitch));
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x0005F91C File Offset: 0x0005DB1C
		public void UpdateIfOtherPlayer(GameTime time)
		{
			if (base.currentLocation == null)
			{
				return;
			}
			this.position.UpdateExtrapolation(this.getMovementSpeed());
			this.position.Field.InterpolationEnabled = !this.currentLocationRef.IsChanging();
			if (Game1.ShouldShowOnscreenUsernames() && Game1.mouseCursorTransparency > 0f && base.currentLocation == Game1.currentLocation && Game1.currentMinigame == null && Game1.activeClickableMenu == null)
			{
				Vector2 local_position = base.getLocalPosition(Game1.viewport);
				Microsoft.Xna.Framework.Rectangle bounding_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, 128, 192);
				bounding_rect.X = (int)(local_position.X + 32f - (float)(bounding_rect.Width / 2));
				bounding_rect.Y = (int)(local_position.Y - (float)bounding_rect.Height + 48f);
				if (bounding_rect.Contains(Game1.getMouseX(false), Game1.getMouseY(false)))
				{
					this.usernameDisplayTime = 1f;
				}
			}
			if (this._lastSelectedItem != this.CurrentItem)
			{
				Item lastSelectedItem = this._lastSelectedItem;
				if (lastSelectedItem != null)
				{
					lastSelectedItem.actionWhenStopBeingHeld(this);
				}
				this._lastSelectedItem = this.CurrentItem;
			}
			this.fireToolEvent.Poll();
			this.beginUsingToolEvent.Poll();
			this.endUsingToolEvent.Poll();
			this.drinkAnimationEvent.Poll();
			this.eatAnimationEvent.Poll();
			this.sickAnimationEvent.Poll();
			this.passOutEvent.Poll();
			this.doEmoteEvent.Poll();
			this.kissFarmerEvent.Poll();
			this.haltAnimationEvent.Poll();
			this.synchronizedJumpEvent.Poll();
			this.renovateEvent.Poll();
			this.FarmerSprite.checkForSingleAnimation(time);
			this.updateCommon(time, base.currentLocation);
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x0005FAE0 File Offset: 0x0005DCE0
		public TItem Equip<TItem>(TItem newItem, NetRef<TItem> slot) where TItem : Item
		{
			TItem oldItem = slot.Value;
			TItem titem = oldItem;
			if (titem != null)
			{
				titem.onDetachedFromParent();
			}
			TItem titem2 = newItem;
			if (titem2 != null)
			{
				titem2.onDetachedFromParent();
			}
			this.Equip<TItem>(oldItem, newItem, delegate(TItem val)
			{
				slot.Value = val;
			});
			return oldItem;
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x0005FB40 File Offset: 0x0005DD40
		public void Equip<TItem>(TItem oldItem, TItem newItem, Action<TItem> equip) where TItem : Item
		{
			bool raiseEvents = Game1.hasLoadedGame && Game1.dayOfMonth > 0 && this.IsLocalPlayer;
			if (raiseEvents)
			{
				TItem titem = oldItem;
				if (titem != null)
				{
					titem.onUnequip(this);
				}
			}
			equip(newItem);
			if (newItem != null)
			{
				newItem.HasBeenInInventory = true;
				if (raiseEvents)
				{
					newItem.onEquip(this);
				}
			}
			TItem titem2 = oldItem;
			if (!(((titem2 != null) ? new bool?(titem2.HasEquipmentBuffs()) : null) ?? false))
			{
				TItem titem3 = newItem;
				bool? flag = (titem3 != null) ? new bool?(titem3.HasEquipmentBuffs()) : null;
				if (flag == null || !flag.GetValueOrDefault())
				{
					return;
				}
			}
			this.buffs.Dirty = true;
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x0005FC18 File Offset: 0x0005DE18
		public void forceCanMove()
		{
			this.forceTimePass = false;
			this.movementDirections.Clear();
			this.isEating = false;
			this.CanMove = true;
			Game1.freezeControls = false;
			this.freezePause = 0;
			this.UsingTool = false;
			this.usingSlingshot = false;
			this.FarmerSprite.PauseForSingleAnimation = false;
			FishingRod rod = this.CurrentTool as FishingRod;
			if (rod != null)
			{
				rod.isFishing = false;
			}
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x0005FC82 File Offset: 0x0005DE82
		public void dropItem(Item i)
		{
			if (i != null && i.canBeDropped())
			{
				Game1.createItemDebris(i.getOne(), base.getStandingPosition(), this.FacingDirection, null, -1, false);
			}
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0005FCAA File Offset: 0x0005DEAA
		public bool addEvent(string eventName, int daysActive)
		{
			return this.activeDialogueEvents.TryAdd(eventName, daysActive);
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x0005FCB9 File Offset: 0x0005DEB9
		public Vector2 getMostRecentMovementVector()
		{
			return new Vector2(base.Position.X - this.lastPosition.X, base.Position.Y - this.lastPosition.Y);
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x0005FCF0 File Offset: 0x0005DEF0
		public int GetSkillLevel(int index)
		{
			switch (index)
			{
			case 0:
				return this.FarmingLevel;
			case 1:
				return this.FishingLevel;
			case 2:
				return this.ForagingLevel;
			case 3:
				return this.MiningLevel;
			case 4:
				return this.CombatLevel;
			case 5:
				return this.LuckLevel;
			default:
				return 0;
			}
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x0005FD48 File Offset: 0x0005DF48
		public int GetUnmodifiedSkillLevel(int index)
		{
			switch (index)
			{
			case 0:
				return this.farmingLevel.Value;
			case 1:
				return this.fishingLevel.Value;
			case 2:
				return this.foragingLevel.Value;
			case 3:
				return this.miningLevel.Value;
			case 4:
				return this.combatLevel.Value;
			case 5:
				return this.luckLevel.Value;
			default:
				return 0;
			}
		}

		// Token: 0x06000948 RID: 2376 RVA: 0x0005FDC0 File Offset: 0x0005DFC0
		public static string getSkillNameFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return "Farming";
			case 1:
				return "Fishing";
			case 2:
				return "Foraging";
			case 3:
				return "Mining";
			case 4:
				return "Combat";
			case 5:
				return "Luck";
			default:
				return "";
			}
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0005FE18 File Offset: 0x0005E018
		public static int getSkillNumberFromName(string name)
		{
			string a = name.ToLower();
			if (a == "farming")
			{
				return 0;
			}
			if (a == "mining")
			{
				return 3;
			}
			if (a == "fishing")
			{
				return 1;
			}
			if (a == "foraging")
			{
				return 2;
			}
			if (a == "luck")
			{
				return 5;
			}
			if (!(a == "combat"))
			{
				return -1;
			}
			return 4;
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0005FE8C File Offset: 0x0005E08C
		public bool setSkillLevel(string nameOfSkill, int level)
		{
			int skillIndex = Farmer.getSkillNumberFromName(nameOfSkill);
			if (!(nameOfSkill == "Farming"))
			{
				if (!(nameOfSkill == "Fishing"))
				{
					if (!(nameOfSkill == "Foraging"))
					{
						if (!(nameOfSkill == "Mining"))
						{
							if (nameOfSkill == "Combat")
							{
								if (this.combatLevel.Value < level)
								{
									this.newLevels.Add(new Point(skillIndex, level - this.combatLevel.Value));
									this.combatLevel.Value = level;
									this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
									return true;
								}
							}
						}
						else if (this.miningLevel.Value < level)
						{
							this.newLevels.Add(new Point(skillIndex, level - this.miningLevel.Value));
							this.miningLevel.Value = level;
							this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
							return true;
						}
					}
					else if (this.foragingLevel.Value < level)
					{
						this.newLevels.Add(new Point(skillIndex, level - this.foragingLevel.Value));
						this.foragingLevel.Value = level;
						this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
						return true;
					}
				}
				else if (this.fishingLevel.Value < level)
				{
					this.newLevels.Add(new Point(skillIndex, level - this.fishingLevel.Value));
					this.fishingLevel.Value = level;
					this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
					return true;
				}
			}
			else if (this.farmingLevel.Value < level)
			{
				this.newLevels.Add(new Point(skillIndex, level - this.farmingLevel.Value));
				this.farmingLevel.Value = level;
				this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
				return true;
			}
			return false;
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0006007C File Offset: 0x0005E27C
		public static string getSkillDisplayNameFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1991");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1993");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1994");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1992");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1996");
			case 5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1995");
			default:
				return "";
			}
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x00060110 File Offset: 0x0005E310
		public bool hasCompletedCommunityCenter()
		{
			return this.mailReceived.Contains("ccBoilerRoom") && this.mailReceived.Contains("ccCraftsRoom") && this.mailReceived.Contains("ccPantry") && this.mailReceived.Contains("ccFishTank") && this.mailReceived.Contains("ccVault") && this.mailReceived.Contains("ccBulletin");
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0006018C File Offset: 0x0005E38C
		private bool localBusMoving()
		{
			GameLocation currentLocation = base.currentLocation;
			Desert desert = currentLocation as Desert;
			if (desert == null)
			{
				BusStop busStop = currentLocation as BusStop;
				return busStop != null && (busStop.drivingOff || busStop.drivingBack);
			}
			return desert.drivingOff || desert.drivingBack;
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x000601D9 File Offset: 0x0005E3D9
		public virtual bool CanBeDamaged()
		{
			return !this.IsDedicatedPlayer && !this.temporarilyInvincible && !this.isEating && !Game1.fadeToBlack && !this.hasBuff("21");
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0006020C File Offset: 0x0005E40C
		public void takeDamage(int damage, bool overrideParry, Monster damager)
		{
			if (Game1.eventUp)
			{
				return;
			}
			if (this.IsDedicatedPlayer || this.FarmerSprite.isPassingOut())
			{
				return;
			}
			if (this.isInBed.Value && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog)
			{
				return;
			}
			bool flag = damager != null && !damager.isInvincible() && !overrideParry;
			bool monsterDamageCapable = (damager == null || !damager.isInvincible()) && (damager == null || (!(damager is GreenSlime) && !(damager is BigSlime)) || !this.isWearingRing("520"));
			bool playerParryable = this.CurrentTool is MeleeWeapon && ((MeleeWeapon)this.CurrentTool).isOnSpecial && ((MeleeWeapon)this.CurrentTool).type.Value == 3;
			bool playerDamageable = this.CanBeDamaged();
			if (flag && playerParryable)
			{
				Rumble.rumble(0.75f, 150f);
				base.playNearbySoundAll("parry", null, SoundContext.Default);
				damager.parried(damage, this);
				return;
			}
			if (monsterDamageCapable && playerDamageable)
			{
				if (damager != null)
				{
					damager.onDealContactDamage(this);
				}
				damage += Game1.random.Next(Math.Min(-1, -damage / 8), Math.Max(1, damage / 8));
				int defense = this.buffs.Defense;
				if (this.stats.Get("Book_Defense") > 0U)
				{
					defense++;
				}
				if ((float)defense >= (float)damage * 0.5f)
				{
					defense -= (int)((float)defense * (float)Game1.random.Next(3) / 10f);
				}
				if (damager != null && this.isWearingRing("839"))
				{
					Microsoft.Xna.Framework.Rectangle monsterBox = damager.GetBoundingBox();
					Utility.getAwayFromPlayerTrajectory(monsterBox, this) / 2f;
					int damageToMonster = damage;
					int farmerDamage = Math.Max(1, damage - defense);
					if (farmerDamage < 10)
					{
						damageToMonster = (int)Math.Ceiling((double)(damageToMonster + farmerDamage) / 2.0);
					}
					int multiplier = this.getNumberOfWornRingsWithID("839");
					damageToMonster *= multiplier;
					GameLocation currentLocation = base.currentLocation;
					if (currentLocation != null)
					{
						currentLocation.damageMonster(monsterBox, damageToMonster, damageToMonster + 1, false, this, false);
					}
				}
				if (this.isWearingRing("524") && !this.hasBuff("21") && Game1.random.NextDouble() < (0.9 - (double)((float)this.health / 100f)) / (double)(3 - this.LuckLevel / 10) + ((this.health <= 15) ? 0.2 : 0.0))
				{
					base.playNearbySoundAll("yoba", null, SoundContext.Default);
					this.applyBuff("21");
					return;
				}
				Rumble.rumble(0.75f, 150f);
				damage = Math.Max(1, damage - defense);
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is MineShaft && Game1.mine.getMineArea(-1) == 121)
				{
					float adjustment = 1f;
					int sharpTeethAmount;
					if (this.team.calicoStatueEffects.TryGetValue(8, out sharpTeethAmount))
					{
						adjustment += (float)sharpTeethAmount * 0.25f;
					}
					int toothFileAmount;
					if (this.team.calicoStatueEffects.TryGetValue(14, out toothFileAmount))
					{
						adjustment -= (float)toothFileAmount * 0.25f;
					}
					damage = Math.Max(1, (int)((float)damage * adjustment));
				}
				this.health = Math.Max(0, this.health - damage);
				foreach (Trinket trinket in this.trinketItems)
				{
					if (trinket != null)
					{
						trinket.OnReceiveDamage(this, damage);
					}
				}
				if (this.health <= 0 && this.GetEffectsOfRingMultiplier("863") > 0 && !this.hasUsedDailyRevive.Value)
				{
					base.startGlowing(new Color(255, 255, 0), false, 0.25f);
					DelayedAction.functionAfterDelay(new Action(base.stopGlowing), 500);
					Game1.playSound("yoba", null);
					for (int i = 0; i < 13; i++)
					{
						float xPos = (float)Game1.random.Next(-32, 33);
						base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(114, 46, 2, 2), 200f, 5, 1, new Vector2(xPos + 32f, -96f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							attachedCharacter = this,
							positionFollowsAttachedCharacter = true,
							motion = new Vector2(xPos / 32f, -3f),
							delayBeforeAnimationStart = i * 50,
							alphaFade = 0.001f,
							acceleration = new Vector2(0f, 0.1f)
						});
					}
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(157, 280, 28, 19), 2000f, 1, 1, new Vector2(-20f, -16f), false, false, 1E-06f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						attachedCharacter = this,
						positionFollowsAttachedCharacter = true,
						alpha = 0.1f,
						alphaFade = -0.01f,
						alphaFadeFade = -0.00025f
					});
					this.health = (int)Math.Min((float)this.maxHealth, (float)this.maxHealth * 0.5f + (float)this.GetEffectsOfRingMultiplier("863"));
					this.hasUsedDailyRevive.Value = true;
				}
				this.temporarilyInvincible = true;
				this.flashDuringThisTemporaryInvincibility = true;
				this.temporaryInvincibilityTimer = 0;
				this.currentTemporaryInvincibilityDuration = 1200 + this.GetEffectsOfRingMultiplier("861") * 400;
				Point standingPixel = base.StandingPixel;
				base.currentLocation.debris.Add(new Debris(damage, new Vector2((float)(standingPixel.X + 8), (float)standingPixel.Y), Color.Red, 1f, this));
				base.playNearbySoundAll("ow", null, SoundContext.Default);
				Game1.hitShakeTimer = 100 * damage;
			}
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0006085C File Offset: 0x0005EA5C
		public int GetEffectsOfRingMultiplier(string ringId)
		{
			int count = 0;
			if (this.leftRing.Value != null)
			{
				count += this.leftRing.Value.GetEffectsOfRingMultiplier(ringId);
			}
			if (this.rightRing.Value != null)
			{
				count += this.rightRing.Value.GetEffectsOfRingMultiplier(ringId);
			}
			return count;
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x000608B0 File Offset: 0x0005EAB0
		private void checkDamage(GameLocation location)
		{
			if (Game1.eventUp)
			{
				return;
			}
			for (int i = location.characters.Count - 1; i >= 0; i--)
			{
				if (i < location.characters.Count)
				{
					Monster monster = location.characters[i] as Monster;
					if (monster != null && monster.OverlapsFarmerForDamage(this))
					{
						monster.currentLocation = location;
						monster.collisionWithFarmerBehavior();
						if (monster.DamageToFarmer > 0)
						{
							if (this.CurrentTool is MeleeWeapon && ((MeleeWeapon)this.CurrentTool).isOnSpecial && ((MeleeWeapon)this.CurrentTool).type.Value == 3)
							{
								this.takeDamage(monster.DamageToFarmer, false, monster);
							}
							else
							{
								this.takeDamage(Math.Max(1, monster.DamageToFarmer + Game1.random.Next(-monster.DamageToFarmer / 4, monster.DamageToFarmer / 4)), false, monster);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x000609A8 File Offset: 0x0005EBA8
		public bool checkAction(Farmer who, GameLocation location)
		{
			if (who.isRidingHorse())
			{
				who.Halt();
			}
			if (this.hidden.Value)
			{
				return false;
			}
			if (Game1.CurrentEvent != null)
			{
				if (Game1.CurrentEvent.isSpecificFestival("spring24") && who.dancePartner.Value == null)
				{
					who.Halt();
					who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
					string question = Game1.content.LoadString("Strings\\UI:AskToDance_" + (this.IsMale ? "Male" : "Female"), base.Name);
					location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
					{
						if (answer == "Yes")
						{
							who.team.SendProposal(this, ProposalType.Dance, null);
							Game1.activeClickableMenu = new PendingProposalDialog();
						}
					}, null);
					return true;
				}
				return false;
			}
			else
			{
				if (who.CurrentItem != null && who.CurrentItem.QualifiedItemId == "(O)801" && !this.isMarriedOrRoommates() && !this.isEngaged() && !who.isMarriedOrRoommates() && !who.isEngaged())
				{
					who.Halt();
					who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
					string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (this.IsMale ? "Male" : "Female"), base.Name);
					location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate(Farmer _, string answer)
					{
						if (answer == "Yes")
						{
							who.team.SendProposal(this, ProposalType.Marriage, who.CurrentItem.getOne());
							Game1.activeClickableMenu = new PendingProposalDialog();
						}
					}, null);
					return true;
				}
				if (who.CanMove)
				{
					Object activeObject = who.ActiveObject;
					bool? flag = (activeObject != null) ? new bool?(activeObject.canBeGivenAsGift()) : null;
					if (flag != null && flag.GetValueOrDefault() && !who.ActiveObject.questItem.Value)
					{
						who.Halt();
						who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
						string question3 = Game1.content.LoadString("Strings\\UI:GiftPlayerItem_" + (this.IsMale ? "Male" : "Female"), who.ActiveObject.DisplayName, base.Name);
						location.createQuestionDialogue(question3, location.createYesNoResponses(), delegate(Farmer _, string answer)
						{
							if (answer == "Yes")
							{
								who.team.SendProposal(this, ProposalType.Gift, who.ActiveObject.getOne());
								Game1.activeClickableMenu = new PendingProposalDialog();
							}
						}, null);
						return true;
					}
				}
				long? playerSpouseID = this.team.GetSpouse(this.UniqueMultiplayerID);
				bool flag2 = playerSpouseID != null;
				long num = who.UniqueMultiplayerID;
				long? num2 = playerSpouseID;
				if ((flag2 & (num == num2.GetValueOrDefault() & num2 != null)) && who.CanMove && !who.isMoving() && !this.isMoving() && Utility.IsHorizontalDirection(base.getGeneralDirectionTowards(who.getStandingPosition(), -10, false, false)))
				{
					who.Halt();
					who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
					who.kissFarmerEvent.Fire(this.UniqueMultiplayerID);
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							motion = new Vector2(0f, -0.5f),
							alphaFade = 0.01f
						}
					});
					base.playNearbySoundAll("dwop", null, SoundContext.NPC);
					return true;
				}
				return false;
			}
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x00060DB0 File Offset: 0x0005EFB0
		public void Update(GameTime time, GameLocation location)
		{
			if (this._lastEquippedTool != this.CurrentTool)
			{
				this.Equip<Tool>(this._lastEquippedTool, this.CurrentTool, delegate(Tool tool)
				{
					this._lastEquippedTool = tool;
				});
			}
			this.buffs.SetOwner(this);
			this.buffs.Update(time);
			this.position.UpdateExtrapolation(this.getMovementSpeed());
			this.fireToolEvent.Poll();
			this.beginUsingToolEvent.Poll();
			this.endUsingToolEvent.Poll();
			this.drinkAnimationEvent.Poll();
			this.eatAnimationEvent.Poll();
			this.sickAnimationEvent.Poll();
			this.passOutEvent.Poll();
			this.doEmoteEvent.Poll();
			this.kissFarmerEvent.Poll();
			this.synchronizedJumpEvent.Poll();
			this.renovateEvent.Poll();
			if (this.IsLocalPlayer)
			{
				if (base.currentLocation == null)
				{
					return;
				}
				this.hidden.Value = (this.IsDedicatedPlayer || this.localBusMoving() || (location.currentEvent != null && !location.currentEvent.isFestival) || (location.currentEvent != null && location.currentEvent.doingSecretSanta) || Game1.locationRequest != null || !Game1.displayFarmer);
				this.isInBed.Value = (base.currentLocation.doesTileHaveProperty(base.TilePoint.X, base.TilePoint.Y, "Bed", "Back", false) != null || this.sleptInTemporaryBed.Value);
				if (!Game1.options.allowStowing)
				{
					this.netItemStowed.Value = false;
				}
				this.hasMenuOpen.Value = (Game1.activeClickableMenu != null);
			}
			if (this.IsSitting())
			{
				this.movementDirections.Clear();
				if (this.IsSitting() && !this.isStopSitting)
				{
					if (!this.sittingFurniture.IsSeatHere(base.currentLocation))
					{
						this.StopSitting(false);
					}
					else
					{
						MapSeat mapSeat = this.sittingFurniture as MapSeat;
						if (mapSeat != null)
						{
							if (!base.currentLocation.mapSeats.Contains(this.sittingFurniture))
							{
								this.StopSitting(false);
							}
							else if (mapSeat.IsBlocked(base.currentLocation))
							{
								this.StopSitting(true);
							}
						}
					}
				}
			}
			if (Game1.CurrentEvent == null && !this.bathingClothes.Value && !this.onBridge.Value)
			{
				this.canOnlyWalk = false;
			}
			if (this.noMovementPause > 0)
			{
				this.CanMove = false;
				this.noMovementPause -= time.ElapsedGameTime.Milliseconds;
				if (this.noMovementPause <= 0)
				{
					this.CanMove = true;
				}
			}
			if (this.freezePause > 0)
			{
				this.CanMove = false;
				this.freezePause -= time.ElapsedGameTime.Milliseconds;
				if (this.freezePause <= 0)
				{
					this.CanMove = true;
				}
			}
			if (this.sparklingText != null && this.sparklingText.update(time))
			{
				this.sparklingText = null;
			}
			if (this.newLevelSparklingTexts.Count > 0 && this.sparklingText == null && !this.UsingTool && this.CanMove && Game1.activeClickableMenu == null)
			{
				this.sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2003", Farmer.getSkillDisplayNameFromIndex(this.newLevelSparklingTexts.Peek())), Color.White, Color.White, true, 0.1, 2500, -1, 500, 1f);
				this.newLevelSparklingTexts.Dequeue();
			}
			if (this.lerpPosition >= 0f)
			{
				this.lerpPosition += (float)time.ElapsedGameTime.TotalSeconds;
				if (this.lerpPosition >= this.lerpDuration)
				{
					this.lerpPosition = this.lerpDuration;
				}
				base.Position = new Vector2(Utility.Lerp(this.lerpStartPosition.X, this.lerpEndPosition.X, this.lerpPosition / this.lerpDuration), Utility.Lerp(this.lerpStartPosition.Y, this.lerpEndPosition.Y, this.lerpPosition / this.lerpDuration));
				if (this.lerpPosition >= this.lerpDuration)
				{
					this.lerpPosition = -1f;
				}
			}
			if (this.isStopSitting && this.lerpPosition < 0f)
			{
				this.isStopSitting = false;
				if (this.sittingFurniture != null)
				{
					this.mapChairSitPosition.Value = new Vector2(-1f, -1f);
					this.sittingFurniture.RemoveSittingFarmer(this);
					this.sittingFurniture = null;
					this.isSitting.Value = false;
				}
			}
			if (this.isInBed.Value && Game1.IsMultiplayer && Game1.shouldTimePass(false))
			{
				this.regenTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.regenTimer < 0)
				{
					this.regenTimer = 500;
					if (this.stamina < (float)this.MaxStamina)
					{
						float stamina = this.stamina;
						this.stamina = stamina + 1f;
					}
					if (this.health < this.maxHealth)
					{
						this.health++;
					}
				}
			}
			this.FarmerSprite.checkForSingleAnimation(time);
			if (this.CanMove)
			{
				this.rotation = 0f;
				if (this.health <= 0 && !Game1.killScreen && Game1.timeOfDay < 2600)
				{
					if (this.IsSitting())
					{
						this.StopSitting(false);
					}
					this.CanMove = false;
					Game1.screenGlowOnce(Color.Red, true, 0.005f, 0.3f);
					Game1.killScreen = true;
					this.faceDirection(2);
					this.FarmerSprite.setCurrentFrame(5);
					this.jitterStrength = 1f;
					Game1.pauseTime = 3000f;
					Rumble.rumbleAndFade(0.75f, 1500f);
					this.freezePause = 8000;
					if (Game1.currentSong != null && Game1.currentSong.IsPlaying)
					{
						Game1.currentSong.Stop(AudioStopOptions.Immediate);
					}
					Game1.changeMusicTrack("silence", false, MusicContext.Default);
					base.playNearbySoundAll("death", null, SoundContext.Default);
					Game1.dialogueUp = false;
					Stats stats = Game1.stats;
					uint timesUnconscious = stats.TimesUnconscious;
					stats.TimesUnconscious = timesUnconscious + 1U;
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.player.currentLocation is MineShaft && Game1.mine.getMineArea(-1) == 121)
					{
						float eggPercentToRemove = 0.2f;
						if (Game1.player.team.calicoStatueEffects.ContainsKey(5))
						{
							eggPercentToRemove = 0.5f;
						}
						int eggsRemoved = (int)(eggPercentToRemove * (float)Game1.player.getItemCount("CalicoEgg"));
						Game1.player.Items.ReduceId("CalicoEgg", eggsRemoved);
						this.itemsLostLastDeath.Clear();
						if (eggsRemoved > 0)
						{
							this.itemsLostLastDeath.Add(new Object("CalicoEgg", eggsRemoved, false, -1, 0));
						}
					}
					if (Game1.activeClickableMenu is GameMenu)
					{
						Game1.activeClickableMenu.emergencyShutDown();
						Game1.activeClickableMenu = null;
					}
				}
				if (this.collisionNPC != null)
				{
					this.collisionNPC.farmerPassesThrough = true;
				}
				NPC collider;
				if (this.movementDirections.Count > 0 && !this.isRidingHorse() && (collider = location.isCollidingWithCharacter(this.nextPosition(this.FacingDirection))) != null)
				{
					this.charactercollisionTimer += time.ElapsedGameTime.Milliseconds;
					if (this.charactercollisionTimer > collider.getTimeFarmerMustPushBeforeStartShaking())
					{
						collider.shake(50);
					}
					if (this.charactercollisionTimer >= collider.getTimeFarmerMustPushBeforePassingThrough() && this.collisionNPC == null)
					{
						this.collisionNPC = collider;
						if (this.collisionNPC.Name.Equals("Bouncer") && base.currentLocation != null && base.currentLocation.name.Equals("SandyHouse"))
						{
							this.collisionNPC.showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2010"), null, 2, 3000, 0);
							this.collisionNPC = null;
							this.charactercollisionTimer = 0;
						}
						else if (this.collisionNPC.name.Equals("Henchman") && base.currentLocation != null && base.currentLocation.name.Equals("WitchSwamp"))
						{
							this.collisionNPC = null;
							this.charactercollisionTimer = 0;
						}
						else if (this.collisionNPC is Raccoon)
						{
							this.collisionNPC = null;
							this.charactercollisionTimer = 0;
						}
					}
				}
				else
				{
					this.charactercollisionTimer = 0;
					if (this.collisionNPC != null && location.isCollidingWithCharacter(this.nextPosition(this.FacingDirection)) == null)
					{
						this.collisionNPC.farmerPassesThrough = false;
						this.collisionNPC = null;
					}
				}
			}
			if (Game1.shouldTimePass(false))
			{
				MeleeWeapon.weaponsTypeUpdate(time);
			}
			if (!Game1.eventUp || this.movementDirections.Count <= 0 || base.currentLocation.currentEvent == null || base.currentLocation.currentEvent.playerControlSequence || (this.controller != null && this.controller.allowPlayerPathingInEvent))
			{
				this.lastPosition = base.Position;
				if (this.controller != null)
				{
					if (this.controller.update(time))
					{
						this.controller = null;
					}
				}
				else if (this.controller == null)
				{
					this.MovePosition(time, Game1.viewport, location);
				}
			}
			if (Game1.actionsWhenPlayerFree.Count > 0 && this.IsLocalPlayer && !this.IsBusyDoingSomething())
			{
				Action action = Game1.actionsWhenPlayerFree[0];
				Game1.actionsWhenPlayerFree.RemoveAt(0);
				action();
			}
			this.updateCommon(time, location);
			this.position.Paused = (this.FarmerSprite.PauseForSingleAnimation || (this.UsingTool && !this.canStrafeForToolUse()) || this.isEating);
			this.checkDamage(location);
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x00061774 File Offset: 0x0005F974
		private void updateCommon(GameTime time, GameLocation location)
		{
			if (this.usernameDisplayTime > 0f)
			{
				this.usernameDisplayTime -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.usernameDisplayTime < 0f)
				{
					this.usernameDisplayTime = 0f;
				}
			}
			if (this.jitterStrength > 0f)
			{
				this.jitter = new Vector2((float)Game1.random.Next(-(int)(this.jitterStrength * 100f), (int)((this.jitterStrength + 1f) * 100f)) / 100f, (float)Game1.random.Next(-(int)(this.jitterStrength * 100f), (int)((this.jitterStrength + 1f) * 100f)) / 100f);
			}
			if (this._wasSitting != this.isSitting.Value)
			{
				if (this._wasSitting)
				{
					this.yOffset = 0f;
					this.xOffset = 0f;
				}
				this._wasSitting = this.isSitting.Value;
			}
			if (this.yJumpOffset != 0)
			{
				this.yJumpVelocity -= ((this.UsingTool && this.canStrafeForToolUse() && (this.movementDirections.Count > 0 || (!this.IsLocalPlayer && base.IsRemoteMoving()))) ? 0.25f : 0.5f);
				this.yJumpOffset -= (int)this.yJumpVelocity;
				if (this.yJumpOffset >= 0)
				{
					this.yJumpOffset = 0;
					this.yJumpVelocity = 0f;
				}
			}
			this.updateMovementAnimation(time);
			base.updateEmote(time);
			base.updateGlow();
			this.currentLocationRef.Update(false);
			if (this.exhausted.Value && this.stamina <= 1f)
			{
				this.currentEyes = 4;
				this.blinkTimer = -1000;
			}
			this.blinkTimer += time.ElapsedGameTime.Milliseconds;
			if (this.blinkTimer > 2200 && Game1.random.NextDouble() < 0.01)
			{
				this.blinkTimer = -150;
				this.currentEyes = 4;
			}
			else if (this.blinkTimer > -100)
			{
				if (this.blinkTimer < -50)
				{
					this.currentEyes = 1;
				}
				else if (this.blinkTimer < 0)
				{
					this.currentEyes = 4;
				}
				else
				{
					this.currentEyes = 0;
				}
			}
			if (this.isCustomized.Value && this.isInBed.Value && !Game1.eventUp && ((this.timerSinceLastMovement >= 3000 && Game1.timeOfDay >= 630) || this.timeWentToBed.Value != 0))
			{
				this.currentEyes = 1;
				this.blinkTimer = -10;
			}
			this.UpdateItemStow();
			if (this.swimming.Value)
			{
				this.yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
				int oldSwimTimer = this.swimTimer;
				this.swimTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.timerSinceLastMovement == 0)
				{
					if (oldSwimTimer > 400 && this.swimTimer <= 400 && this.IsLocalPlayer)
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.xVelocity) + Math.Abs(this.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, (float)(base.StandingPixel.Y - 32)), false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false)
						});
					}
					if (this.swimTimer < 0)
					{
						this.swimTimer = 800;
						if (this.IsLocalPlayer)
						{
							base.playNearbySoundAll("slosh", null, SoundContext.Default);
							Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.xVelocity) + Math.Abs(this.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, (float)(base.StandingPixel.Y - 32)), false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false)
							});
						}
					}
				}
				else if (!Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
				{
					if (this.timerSinceLastMovement > 800)
					{
						this.currentEyes = 1;
					}
					else if (this.timerSinceLastMovement > 700)
					{
						this.currentEyes = 4;
					}
					if (this.swimTimer < 0)
					{
						this.swimTimer = 100;
						if (this.Stamina < (float)this.MaxStamina)
						{
							float stamina = this.Stamina;
							this.Stamina = stamina + 1f;
						}
						if (this.health < this.maxHealth)
						{
							this.health++;
						}
					}
				}
			}
			if (!this.isMoving())
			{
				this.timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.timerSinceLastMovement = 0;
			}
			for (int i = this.Items.Count - 1; i >= 0; i--)
			{
				Tool tool = this.Items[i] as Tool;
				if (tool != null)
				{
					tool.tickUpdate(time, this);
				}
			}
			Tool tempTool = this.TemporaryItem as Tool;
			if (tempTool != null)
			{
				tempTool.tickUpdate(time, this);
			}
			Ring value = this.rightRing.Value;
			if (value != null)
			{
				value.update(time, location, this);
			}
			Ring value2 = this.leftRing.Value;
			if (value2 != null)
			{
				value2.update(time, location, this);
			}
			if (Game1.shouldTimePass(false) && this.IsLocalPlayer)
			{
				foreach (Trinket trinket in this.trinketItems)
				{
					if (trinket != null)
					{
						trinket.Update(this, time, location);
					}
				}
			}
			Horse mount = this.mount;
			if (mount != null)
			{
				mount.update(time, location);
			}
			Horse mount2 = this.mount;
			if (mount2 != null)
			{
				mount2.SyncPositionToRider();
			}
			foreach (Companion companion in this.companions)
			{
				companion.Update(time, location);
			}
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x00061E50 File Offset: 0x00060050
		public virtual bool IsBusyDoingSomething()
		{
			return Game1.eventUp || Game1.fadeToBlack || Game1.currentMinigame != null || Game1.activeClickableMenu != null || Game1.isWarping || this.UsingTool || Game1.killScreen || this.freezePause > 0 || !this.CanMove || this.FarmerSprite.PauseForSingleAnimation || this.usingSlingshot;
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x00061ECC File Offset: 0x000600CC
		public void UpdateItemStow()
		{
			if (this._itemStowed != this.netItemStowed.Value)
			{
				if (this.netItemStowed.Value && this.ActiveObject != null)
				{
					this.ActiveObject.actionWhenStopBeingHeld(this);
				}
				this._itemStowed = this.netItemStowed.Value;
				if (!this.netItemStowed.Value)
				{
					Object activeObject = this.ActiveObject;
					if (activeObject == null)
					{
						return;
					}
					activeObject.actionWhenBeingHeld(this);
				}
			}
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x00061F3C File Offset: 0x0006013C
		public void addQuest(string questId)
		{
			if (!this.hasQuest(questId))
			{
				Quest quest = Quest.getQuestFromId(questId);
				if (quest == null)
				{
					Game1.log.Warn("Can't add quest with ID '" + questId + "' because no such ID was found.");
					return;
				}
				this.questLog.Add(quest);
				if (!quest.IsHidden())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2011"), 2));
				}
				foreach (string key in Game1.player.team.constructedBuildings)
				{
					quest.OnBuildingExists(key, false);
				}
			}
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x00061FF4 File Offset: 0x000601F4
		public void removeQuest(string questID)
		{
			this.questLog.RemoveWhere((Quest quest) => quest.id.Value == questID);
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x00062028 File Offset: 0x00060228
		public void completeQuest(string questID)
		{
			for (int i = this.questLog.Count - 1; i >= 0; i--)
			{
				if (this.questLog[i].id.Value == questID)
				{
					this.questLog[i].questComplete();
				}
			}
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0006207C File Offset: 0x0006027C
		public bool hasQuest(string id)
		{
			for (int i = this.questLog.Count - 1; i >= 0; i--)
			{
				if (this.questLog[i].id.Value == id)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x000620C4 File Offset: 0x000602C4
		public bool hasNewQuestActivity()
		{
			foreach (SpecialOrder o in this.team.specialOrders)
			{
				if (!o.IsHidden() && (o.ShouldDisplayAsNew() || o.ShouldDisplayAsComplete()))
				{
					return true;
				}
			}
			foreach (Quest q in this.questLog)
			{
				if (!q.IsHidden() && (q.showNew.Value || (q.completed.Value && !q.destroy.Value)))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x000621AC File Offset: 0x000603AC
		public float getMovementSpeed()
		{
			if (this.UsingTool && this.canStrafeForToolUse())
			{
				return 2f;
			}
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				this.movementMultiplier = 0.066f;
				float movementSpeed;
				if (this.isRidingHorse())
				{
					movementSpeed = Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : (this.addedSpeed + 4.6f + (this.mount.ateCarrotToday ? 0.4f : 0f) + ((this.stats.Get("Book_Horse") > 0U) ? 0.5f : 0f)))) * this.movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds);
				}
				else
				{
					movementSpeed = Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : (this.addedSpeed + this.temporarySpeedBuff))) * this.movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds);
				}
				if (this.movementDirections.Count > 1)
				{
					movementSpeed *= 0.707f;
				}
				if (Game1.CurrentEvent == null && this.hasBuff("19"))
				{
					movementSpeed = 0f;
				}
				return movementSpeed;
			}
			float movementSpeed2 = Math.Max(1f, (float)base.speed + (Game1.eventUp ? ((float)Math.Max(0, Game1.CurrentEvent.farmerAddedSpeed - 2)) : (this.addedSpeed + (this.isRidingHorse() ? 5f : this.temporarySpeedBuff))));
			if (this.movementDirections.Count > 1)
			{
				movementSpeed2 = (float)Math.Max(1, (int)Math.Sqrt((double)(2f * (movementSpeed2 * movementSpeed2))) / 2);
			}
			return movementSpeed2;
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x00062374 File Offset: 0x00060574
		public bool isWearingRing(string itemId)
		{
			return (this.rightRing.Value != null && this.rightRing.Value.GetsEffectOfRing(itemId)) || (this.leftRing.Value != null && this.leftRing.Value.GetsEffectOfRing(itemId));
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x000623C4 File Offset: 0x000605C4
		public int getNumberOfWornRingsWithID(string itemId)
		{
			int num = 0;
			if (this.rightRing.Value != null && this.rightRing.Value.GetsEffectOfRing(itemId))
			{
				num++;
			}
			if (this.leftRing.Value != null && this.leftRing.Value.GetsEffectOfRing(itemId))
			{
				num++;
			}
			return num;
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x0006241C File Offset: 0x0006061C
		public override void Halt()
		{
			if (!this.FarmerSprite.PauseForSingleAnimation && !this.isRidingHorse() && !this.UsingTool)
			{
				base.Halt();
			}
			this.movementDirections.Clear();
			if (!this.isEmoteAnimating && !this.UsingTool)
			{
				this.stopJittering();
			}
			this.armOffset = Vector2.Zero;
			if (this.isRidingHorse())
			{
				this.mount.Halt();
				this.mount.Sprite.CurrentAnimation = null;
			}
			if (this.IsSitting())
			{
				this.ShowSitting();
			}
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x000624AA File Offset: 0x000606AA
		public void stopJittering()
		{
			this.jitterStrength = 0f;
			this.jitter = Vector2.Zero;
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x000624C4 File Offset: 0x000606C4
		public override Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
		{
			Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
			switch (direction)
			{
			case 0:
				nextPosition.Y -= (int)Math.Ceiling((double)this.getMovementSpeed());
				break;
			case 1:
				nextPosition.X += (int)Math.Ceiling((double)this.getMovementSpeed());
				break;
			case 2:
				nextPosition.Y += (int)Math.Ceiling((double)this.getMovementSpeed());
				break;
			case 3:
				nextPosition.X -= (int)Math.Ceiling((double)this.getMovementSpeed());
				break;
			}
			return nextPosition;
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x00062558 File Offset: 0x00060758
		public Microsoft.Xna.Framework.Rectangle nextPositionHalf(int direction)
		{
			Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
			switch (direction)
			{
			case 0:
				nextPosition.Y -= (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
				break;
			case 1:
				nextPosition.X += (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
				break;
			case 2:
				nextPosition.Y += (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
				break;
			case 3:
				nextPosition.X -= (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
				break;
			}
			return nextPosition;
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x00062618 File Offset: 0x00060818
		public int getProfessionForSkill(int skillType, int skillLevel)
		{
			if (skillLevel != 5)
			{
				if (skillLevel == 10)
				{
					switch (skillType)
					{
					case 0:
						if (this.professions.Contains(1))
						{
							if (this.professions.Contains(4))
							{
								return 4;
							}
							if (this.professions.Contains(5))
							{
								return 5;
							}
						}
						else
						{
							if (this.professions.Contains(2))
							{
								return 2;
							}
							if (this.professions.Contains(3))
							{
								return 3;
							}
						}
						break;
					case 1:
						if (this.professions.Contains(6))
						{
							if (this.professions.Contains(8))
							{
								return 8;
							}
							if (this.professions.Contains(9))
							{
								return 9;
							}
						}
						else
						{
							if (this.professions.Contains(10))
							{
								return 10;
							}
							if (this.professions.Contains(11))
							{
								return 11;
							}
						}
						break;
					case 2:
						if (this.professions.Contains(12))
						{
							if (this.professions.Contains(14))
							{
								return 14;
							}
							if (this.professions.Contains(15))
							{
								return 15;
							}
						}
						else
						{
							if (this.professions.Contains(16))
							{
								return 16;
							}
							if (this.professions.Contains(17))
							{
								return 17;
							}
						}
						break;
					case 3:
						if (this.professions.Contains(18))
						{
							if (this.professions.Contains(20))
							{
								return 20;
							}
							if (this.professions.Contains(21))
							{
								return 21;
							}
						}
						else
						{
							if (this.professions.Contains(23))
							{
								return 23;
							}
							if (this.professions.Contains(22))
							{
								return 22;
							}
						}
						break;
					case 4:
						if (this.professions.Contains(24))
						{
							if (this.professions.Contains(26))
							{
								return 26;
							}
							if (this.professions.Contains(27))
							{
								return 27;
							}
						}
						else
						{
							if (this.professions.Contains(28))
							{
								return 28;
							}
							if (this.professions.Contains(29))
							{
								return 29;
							}
						}
						break;
					}
				}
			}
			else
			{
				switch (skillType)
				{
				case 0:
					if (this.professions.Contains(0))
					{
						return 0;
					}
					if (this.professions.Contains(1))
					{
						return 1;
					}
					break;
				case 1:
					if (this.professions.Contains(6))
					{
						return 6;
					}
					if (this.professions.Contains(7))
					{
						return 7;
					}
					break;
				case 2:
					if (this.professions.Contains(12))
					{
						return 12;
					}
					if (this.professions.Contains(13))
					{
						return 13;
					}
					break;
				case 3:
					if (this.professions.Contains(18))
					{
						return 18;
					}
					if (this.professions.Contains(19))
					{
						return 19;
					}
					break;
				case 4:
					if (this.professions.Contains(24))
					{
						return 24;
					}
					if (this.professions.Contains(25))
					{
						return 25;
					}
					break;
				}
			}
			return -1;
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x000628E9 File Offset: 0x00060AE9
		public void behaviorOnMovement(int direction)
		{
			this.hasMoved = true;
		}

		// Token: 0x06000965 RID: 2405 RVA: 0x000628F2 File Offset: 0x00060AF2
		public void OnEmoteAnimationEnd(Farmer farmer)
		{
			if (farmer != this)
			{
				return;
			}
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x00062908 File Offset: 0x00060B08
		public void EndEmoteAnimation()
		{
			if (this.isEmoteAnimating)
			{
				if (this.jitterStrength > 0f)
				{
					this.stopJittering();
				}
				if (this.yJumpOffset != 0)
				{
					this.yJumpOffset = 0;
					this.yJumpVelocity = 0f;
				}
				this.FarmerSprite.PauseForSingleAnimation = false;
				this.FarmerSprite.StopAnimation();
				this.isEmoteAnimating = false;
			}
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x00062968 File Offset: 0x00060B68
		private void broadcastHaltAnimation(Farmer who)
		{
			if (this.IsLocalPlayer)
			{
				this.haltAnimationEvent.Fire();
				return;
			}
			Farmer.completelyStopAnimating(who);
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x00062984 File Offset: 0x00060B84
		private void performHaltAnimation()
		{
			this.completelyStopAnimatingOrDoingAction();
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x0006298C File Offset: 0x00060B8C
		public void performKissFarmer(long otherPlayerID)
		{
			Farmer spouse = Game1.GetPlayer(otherPlayerID, false);
			if (spouse == null)
			{
				return;
			}
			bool localPlayerOnLeft = base.StandingPixel.X < spouse.StandingPixel.X;
			this.PerformKiss(localPlayerOnLeft ? 1 : 3);
			spouse.PerformKiss(localPlayerOnLeft ? 3 : 1);
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x000629D8 File Offset: 0x00060BD8
		public void PerformKiss(int facingDirection)
		{
			if (Game1.eventUp || this.UsingTool || (this.IsLocalPlayer && Game1.activeClickableMenu != null) || this.isRidingHorse() || this.IsSitting() || base.IsEmoting || !this.CanMove)
			{
				return;
			}
			this.CanMove = false;
			this.FarmerSprite.PauseForSingleAnimation = false;
			this.faceDirection(facingDirection);
			this.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(101, 1000, 0, false, this.FacingDirection == 3, null, false, 0),
				new FarmerSprite.AnimationFrame(6, 1, false, this.FacingDirection == 3, new AnimatedSprite.endOfAnimationBehavior(this.broadcastHaltAnimation), false)
			}, null);
			if (!Stats.AllowRetroactiveAchievements)
			{
				Game1.stats.checkForFullHouseAchievement(true);
			}
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x00062AA8 File Offset: 0x00060CA8
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (this.IsSitting())
			{
				return;
			}
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				if (Game1.shouldTimePass(false) && this.temporarilyInvincible)
				{
					if (this.temporaryInvincibilityTimer < 0)
					{
						this.currentTemporaryInvincibilityDuration = 1200;
					}
					this.temporaryInvincibilityTimer += time.ElapsedGameTime.Milliseconds;
					if (this.temporaryInvincibilityTimer > this.currentTemporaryInvincibilityDuration)
					{
						this.temporarilyInvincible = false;
						this.temporaryInvincibilityTimer = 0;
					}
				}
			}
			else if (this.temporarilyInvincible)
			{
				this.temporarilyInvincible = false;
				this.temporaryInvincibilityTimer = 0;
			}
			if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
			{
				return;
			}
			if (this.isRafting)
			{
				this.moveRaft(currentLocation, time);
				return;
			}
			if (this.xVelocity != 0f || this.yVelocity != 0f)
			{
				if (double.IsNaN((double)this.xVelocity) || double.IsNaN((double)this.yVelocity))
				{
					this.xVelocity = 0f;
					this.yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBox();
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(bounds.X + (int)Math.Floor((double)this.xVelocity), bounds.Y - (int)Math.Floor((double)this.yVelocity), bounds.Width, bounds.Height);
				Microsoft.Xna.Framework.Rectangle nextPositionCeil = new Microsoft.Xna.Framework.Rectangle(bounds.X + (int)Math.Ceiling((double)this.xVelocity), bounds.Y - (int)Math.Ceiling((double)this.yVelocity), bounds.Width, bounds.Height);
				Microsoft.Xna.Framework.Rectangle nextPosition = Microsoft.Xna.Framework.Rectangle.Union(value, nextPositionCeil);
				if (!currentLocation.isCollidingPosition(nextPosition, viewport, true, -1, false, this))
				{
					this.position.X += this.xVelocity;
					this.position.Y -= this.yVelocity;
					this.xVelocity -= this.xVelocity / 16f;
					this.yVelocity -= this.yVelocity / 16f;
					if (Math.Abs(this.xVelocity) <= 0.05f)
					{
						this.xVelocity = 0f;
					}
					if (Math.Abs(this.yVelocity) <= 0.05f)
					{
						this.yVelocity = 0f;
					}
				}
				else
				{
					this.xVelocity -= this.xVelocity / 16f;
					this.yVelocity -= this.yVelocity / 16f;
					if (Math.Abs(this.xVelocity) <= 0.05f)
					{
						this.xVelocity = 0f;
					}
					if (Math.Abs(this.yVelocity) <= 0.05f)
					{
						this.yVelocity = 0f;
					}
				}
			}
			if (this.CanMove || Game1.eventUp || this.controller != null || this.canStrafeForToolUse())
			{
				this.temporaryPassableTiles.ClearNonIntersecting(this.GetBoundingBox());
				float movementSpeed = this.getMovementSpeed();
				this.temporarySpeedBuff = 0f;
				if (this.movementDirections.Contains(0) && this.MovePositionImpl(0, 0f, -movementSpeed, time, viewport))
				{
					return;
				}
				if (this.movementDirections.Contains(2) && this.MovePositionImpl(2, 0f, movementSpeed, time, viewport))
				{
					return;
				}
				if (this.movementDirections.Contains(1) && this.MovePositionImpl(1, movementSpeed, 0f, time, viewport))
				{
					return;
				}
				if (this.movementDirections.Contains(3) && this.MovePositionImpl(3, -movementSpeed, 0f, time, viewport))
				{
					return;
				}
			}
			if (this.movementDirections.Count > 0 && !this.UsingTool)
			{
				this.FarmerSprite.intervalModifier = 1f - (this.running ? 0.0255f : 0.025f) * (Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)((int)this.addedSpeed) + (this.isRidingHorse() ? 4.6f : 0f)))) * this.movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) * 1.25f);
			}
			else
			{
				this.FarmerSprite.intervalModifier = 1f;
			}
			if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
			{
				this.temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64));
			}
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x00062F18 File Offset: 0x00061118
		public bool canStrafeForToolUse()
		{
			return this.toolHold.Value != 0 && this.canReleaseTool && (this.toolPower.Value >= 1 || this.toolHoldStartTime.Value - this.toolHold.Value > 150);
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x00062F6C File Offset: 0x0006116C
		protected virtual bool MovePositionImpl(int direction, float movementSpeedX, float movementSpeedY, GameTime time, xTile.Dimensions.Rectangle viewport)
		{
			Microsoft.Xna.Framework.Rectangle targetPos = this.nextPosition(direction);
			Warp warp = Game1.currentLocation.isCollidingWithWarp(targetPos, this);
			if (warp != null && this.IsLocalPlayer)
			{
				if (Game1.eventUp)
				{
					Event currentEvent = Game1.CurrentEvent;
					bool? flag = (currentEvent != null) ? new bool?(currentEvent.isFestival) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						Game1.CurrentEvent.TryStartEndFestivalDialogue(this);
						return true;
					}
				}
				this.warpFarmer(warp, direction);
				return true;
			}
			bool flag2;
			if (Game1.eventUp)
			{
				Event currentEvent2 = Game1.CurrentEvent;
				if (!(((currentEvent2 != null) ? new bool?(currentEvent2.isFestival) : null) ?? true))
				{
					Event currentEvent3 = Game1.CurrentEvent;
					bool? flag = (currentEvent3 != null) ? new bool?(currentEvent3.playerControlSequence) : null;
					flag2 = (flag != null && !flag.GetValueOrDefault());
					goto IL_E3;
				}
			}
			flag2 = false;
			IL_E3:
			bool isCutscene = flag2;
			if (!base.currentLocation.isCollidingPosition(targetPos, viewport, true, 0, false, this) || this.ignoreCollisions || isCutscene)
			{
				this.position.X += movementSpeedX;
				this.position.Y += movementSpeedY;
				this.behaviorOnMovement(direction);
				return false;
			}
			if (!base.currentLocation.isCollidingPosition(this.nextPositionHalf(direction), viewport, true, 0, false, this))
			{
				this.position.X += movementSpeedX / 2f;
				this.position.Y += movementSpeedY / 2f;
				this.behaviorOnMovement(direction);
				return false;
			}
			if (this.movementDirections.Count == 1)
			{
				Microsoft.Xna.Framework.Rectangle tmp = targetPos;
				if (direction == 0 || direction == 2)
				{
					tmp.Width /= 4;
					bool leftCorner = base.currentLocation.isCollidingPosition(tmp, viewport, true, 0, false, this);
					tmp.X += tmp.Width * 3;
					bool rightCorner = base.currentLocation.isCollidingPosition(tmp, viewport, true, 0, false, this);
					if (leftCorner && !rightCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(1), viewport, true, 0, false, this))
					{
						this.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner && !leftCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(3), viewport, true, 0, false, this))
					{
						this.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
				}
				else
				{
					tmp.Height /= 4;
					bool topCorner = base.currentLocation.isCollidingPosition(tmp, viewport, true, 0, false, this);
					tmp.Y += tmp.Height * 3;
					bool bottomCorner = base.currentLocation.isCollidingPosition(tmp, viewport, true, 0, false, this);
					if (topCorner && !bottomCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(2), viewport, true, 0, false, this))
					{
						this.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner && !topCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(0), viewport, true, 0, false, this))
					{
						this.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
				}
			}
			return false;
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x00063318 File Offset: 0x00061518
		public void updateMovementAnimation(GameTime time)
		{
			if (this._emoteGracePeriod > 0)
			{
				this._emoteGracePeriod -= time.ElapsedGameTime.Milliseconds;
			}
			if (this.isEmoteAnimating && (((this.IsLocalPlayer ? (this.movementDirections.Count > 0) : base.IsRemoteMoving()) && this._emoteGracePeriod <= 0) || !this.FarmerSprite.PauseForSingleAnimation))
			{
				this.EndEmoteAnimation();
			}
			bool carrying = this.IsCarrying();
			if (!this.isRidingHorse())
			{
				this.xOffset = 0f;
			}
			FishingRod rod = this.CurrentTool as FishingRod;
			if (rod != null && (rod.isTimingCast || rod.isCasting))
			{
				rod.setTimingCastAnimation(this);
				return;
			}
			if (this.FarmerSprite.PauseForSingleAnimation || this.UsingTool)
			{
				if (this.UsingTool && this.canStrafeForToolUse() && (this.movementDirections.Count > 0 || (!this.IsLocalPlayer && base.IsRemoteMoving())) && this.yJumpOffset == 0)
				{
					this.jumpWithoutSound(2.5f);
				}
				return;
			}
			if (this.IsSitting())
			{
				this.ShowSitting();
				return;
			}
			if (this.IsLocalPlayer && !this.CanMove && !Game1.eventUp)
			{
				if (this.isRidingHorse() && this.mount != null && !this.isAnimatingMount)
				{
					this.showRiding();
					return;
				}
				if (carrying)
				{
					this.showCarrying();
				}
				return;
			}
			else
			{
				if (this.IsLocalPlayer || this.isFakeEventActor)
				{
					this.moveUp = this.movementDirections.Contains(0);
					this.moveRight = this.movementDirections.Contains(1);
					this.moveDown = this.movementDirections.Contains(2);
					this.moveLeft = this.movementDirections.Contains(3);
					if (this.moveLeft)
					{
						this.FacingDirection = 3;
					}
					else if (this.moveRight)
					{
						this.FacingDirection = 1;
					}
					else if (this.moveUp)
					{
						this.FacingDirection = 0;
					}
					else if (this.moveDown)
					{
						this.FacingDirection = 2;
					}
					if (this.isRidingHorse() && !this.mount.dismounting.Value)
					{
						base.speed = 2;
					}
				}
				else
				{
					this.moveLeft = (base.IsRemoteMoving() && this.FacingDirection == 3);
					this.moveRight = (base.IsRemoteMoving() && this.FacingDirection == 1);
					this.moveUp = (base.IsRemoteMoving() && this.FacingDirection == 0);
					this.moveDown = (base.IsRemoteMoving() && this.FacingDirection == 2);
					bool flag = this.moveUp || this.moveRight || this.moveDown || this.moveLeft;
					float speed = this.position.CurrentInterpolationSpeed() / ((float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.066f);
					this.running = (Math.Abs(speed - 5f) < Math.Abs(speed - 2f) && !this.bathingClothes.Value && !this.onBridge.Value);
					if (!flag)
					{
						this.FarmerSprite.StopAnimation();
					}
				}
				if (this.hasBuff("19"))
				{
					this.running = false;
					this.moveUp = false;
					this.moveDown = false;
					this.moveLeft = false;
					this.moveRight = false;
				}
				if (this.FarmerSprite.PauseForSingleAnimation || this.UsingTool)
				{
					return;
				}
				if (this.isRidingHorse() && !this.mount.dismounting.Value)
				{
					this.showRiding();
					return;
				}
				if (this.moveLeft && this.running && !carrying)
				{
					this.FarmerSprite.animate(56, time);
					return;
				}
				if (this.moveRight && this.running && !carrying)
				{
					this.FarmerSprite.animate(40, time);
					return;
				}
				if (this.moveUp && this.running && !carrying)
				{
					this.FarmerSprite.animate(48, time);
					return;
				}
				if (this.moveDown && this.running && !carrying)
				{
					this.FarmerSprite.animate(32, time);
					return;
				}
				if (this.moveLeft && this.running)
				{
					this.FarmerSprite.animate(152, time);
					return;
				}
				if (this.moveRight && this.running)
				{
					this.FarmerSprite.animate(136, time);
					return;
				}
				if (this.moveUp && this.running)
				{
					this.FarmerSprite.animate(144, time);
					return;
				}
				if (this.moveDown && this.running)
				{
					this.FarmerSprite.animate(128, time);
					return;
				}
				if (this.moveLeft && !carrying)
				{
					this.FarmerSprite.animate(24, time);
					return;
				}
				if (this.moveRight && !carrying)
				{
					this.FarmerSprite.animate(8, time);
					return;
				}
				if (this.moveUp && !carrying)
				{
					this.FarmerSprite.animate(16, time);
					return;
				}
				if (this.moveDown && !carrying)
				{
					this.FarmerSprite.animate(0, time);
					return;
				}
				if (this.moveLeft)
				{
					this.FarmerSprite.animate(120, time);
					return;
				}
				if (this.moveRight)
				{
					this.FarmerSprite.animate(104, time);
					return;
				}
				if (this.moveUp)
				{
					this.FarmerSprite.animate(112, time);
					return;
				}
				if (this.moveDown)
				{
					this.FarmerSprite.animate(96, time);
					return;
				}
				if (carrying)
				{
					this.showCarrying();
					return;
				}
				this.showNotCarrying();
				return;
			}
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x00063880 File Offset: 0x00061A80
		public bool IsCarrying()
		{
			return this.mount == null && !this.isAnimatingMount && !this.IsSitting() && !this.onBridge.Value && this.ActiveObject != null && !Game1.eventUp && !Game1.killScreen && this.ActiveObject.IsHeldOverHead();
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x000638E0 File Offset: 0x00061AE0
		public void doneEating()
		{
			this.isEating = false;
			this.tempFoodItemTextureName.Value = null;
			this.completelyStopAnimatingOrDoingAction();
			this.forceCanMove();
			if (this.mostRecentlyGrabbedItem == null || !this.IsLocalPlayer)
			{
				return;
			}
			Object consumed = this.itemToEat as Object;
			if (consumed.QualifiedItemId == "(O)434")
			{
				Game1.stats.checkForStardropAchievement(true);
				this.yOffset = 0f;
				this.yJumpOffset = 0;
				Game1.changeMusicTrack("none", false, MusicContext.Default);
				Game1.playSound("stardrop", null);
				string mid = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs." + Game1.random.Choose("3094", "3095"));
				if (this.favoriteThing.Value != null)
				{
					if (this.favoriteThing.Value.Contains("Stardew"))
					{
						mid = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3097");
					}
					else if (this.favoriteThing.Equals("ConcernedApe"))
					{
						mid = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3099");
					}
					else
					{
						mid += this.favoriteThing.Value;
					}
				}
				DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100") + mid + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101"), 6000);
				this.maxStamina.Value += 34;
				this.stamina = (float)this.MaxStamina;
				this.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
				{
					new FarmerSprite.AnimationFrame(57, 6000)
				}, null);
				base.startGlowing(new Color(200, 0, 255), false, 0.1f);
				this.jitterStrength = 1f;
				Game1.staminaShakeTimer = 12000;
				Game1.screenGlowOnce(new Color(200, 0, 255), true, 0.005f, 0.3f);
				this.CanMove = false;
				this.freezePause = 8000;
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 60f, 8, 40, base.Position + new Vector2(-8f, -128f), false, false, 1f, 0f, Color.White, 4f, 0.0075f, 0f, 0f, false)
				{
					alpha = 0.75f,
					alphaFade = 0.0025f,
					motion = new Vector2(0f, -0.25f)
				});
				if (Game1.displayHUD && !Game1.eventUp)
				{
					for (int i = 0; i < 40; i++)
					{
						Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.random.Next(10, 12), new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right / Game1.options.uiScale - 48f - 8f - (float)Game1.random.Next(64), (float)Game1.random.Next(-64, 64) + (float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom / Game1.options.uiScale - 224f - 16f - (float)((int)((double)(this.MaxStamina - 270) * 0.715))), Game1.random.Choose(Color.White, Color.Lime), 8, false, 50f, 0, -1, -1f, -1, 0)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = 200 * i,
							interval = 100f,
							local = true
						});
					}
				}
				Point tile = base.TilePoint;
				Utility.addSprinklesToLocation(base.currentLocation, tile.X, tile.Y, 9, 9, 6000, 100, new Color(200, 0, 255), null, true);
				DelayedAction.stopFarmerGlowing(6000);
				Utility.addSprinklesToLocation(base.currentLocation, tile.X, tile.Y, 9, 9, 6000, 300, Color.Cyan, null, true);
				this.mostRecentlyGrabbedItem = null;
			}
			else
			{
				if (consumed.HasContextTag("ginger_item"))
				{
					this.buffs.Remove("25");
				}
				foreach (Buff buff in consumed.GetFoodOrDrinkBuffs())
				{
					this.applyBuff(buff);
				}
				string qualifiedItemId = consumed.QualifiedItemId;
				if (!(qualifiedItemId == "(O)773"))
				{
					if (!(qualifiedItemId == "(O)351"))
					{
						if (qualifiedItemId == "(O)349")
						{
							this.Stamina = (float)this.MaxStamina;
						}
					}
					else
					{
						this.exhausted.Value = false;
					}
				}
				else
				{
					this.health = this.maxHealth;
				}
				float oldStam = this.Stamina;
				int oldHealth = this.health;
				int staminaToHeal = consumed.staminaRecoveredOnConsumption();
				int healthToHeal = consumed.healthRecoveredOnConsumption();
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is MineShaft && Game1.mine.getMineArea(-1) == 121 && this.team.calicoStatueEffects.ContainsKey(6))
				{
					staminaToHeal = Math.Max(1, staminaToHeal / 2);
					healthToHeal = Math.Max(1, healthToHeal / 2);
				}
				this.Stamina = Math.Min((float)this.MaxStamina, this.Stamina + (float)staminaToHeal);
				this.health = Math.Min(this.maxHealth, this.health + healthToHeal);
				if (oldStam < this.Stamina)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(this.Stamina - oldStam)), 4));
				}
				if (oldHealth < this.health)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", this.health - oldHealth), 5));
				}
			}
			if (consumed != null && consumed.Edibility < 0)
			{
				this.CanMove = false;
				this.sickAnimationEvent.Fire();
			}
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x00063F38 File Offset: 0x00062138
		public virtual bool NotifyQuests(Func<Quest, bool> check, bool onlyOneQuest = false)
		{
			bool changed = false;
			for (int i = this.questLog.Count - 1; i >= 0; i--)
			{
				Quest quest = this.questLog[i];
				if (!quest.completed.Value)
				{
					if (quest == null)
					{
						this.questLog.RemoveAt(i);
					}
					else if (check(quest))
					{
						changed = true;
						if (onlyOneQuest)
						{
							break;
						}
					}
				}
			}
			return changed;
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x00063F99 File Offset: 0x00062199
		public virtual void AddCompanion(Companion companion)
		{
			if (!this.companions.Contains(companion))
			{
				companion.InitializeCompanion(this);
				this.companions.Add(companion);
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x00063FBC File Offset: 0x000621BC
		public virtual void RemoveCompanion(Companion companion)
		{
			if (this.companions.Contains(companion))
			{
				this.companions.Remove(companion);
				companion.CleanupCompanion();
			}
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x00063FDF File Offset: 0x000621DF
		public static void completelyStopAnimating(Farmer who)
		{
			who.completelyStopAnimatingOrDoingAction();
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x00063FE8 File Offset: 0x000621E8
		public void completelyStopAnimatingOrDoingAction()
		{
			this.CanMove = !Game1.eventUp;
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (this.UsingTool)
			{
				this.EndUsingTool();
				FishingRod rod = this.CurrentTool as FishingRod;
				if (rod != null)
				{
					rod.resetState();
				}
			}
			if (this.usingSlingshot)
			{
				Slingshot slingshot = this.CurrentTool as Slingshot;
				if (slingshot != null)
				{
					slingshot.finish();
				}
			}
			this.UsingTool = false;
			this.isEating = false;
			this.FarmerSprite.PauseForSingleAnimation = false;
			this.usingSlingshot = false;
			this.canReleaseTool = false;
			this.Halt();
			this.Sprite.StopAnimation();
			MeleeWeapon weapon = this.CurrentTool as MeleeWeapon;
			if (weapon != null)
			{
				weapon.isOnSpecial = false;
			}
			this.stopJittering();
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x000640A6 File Offset: 0x000622A6
		public void doEmote(int whichEmote)
		{
			if (Game1.eventUp)
			{
				return;
			}
			if (!this.isEmoting)
			{
				this.isEmoting = true;
				this.currentEmote = whichEmote;
				this.currentEmoteFrame = 0;
				this.emoteInterval = 0f;
			}
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x000640D8 File Offset: 0x000622D8
		public void performTenMinuteUpdate()
		{
		}

		// Token: 0x06000978 RID: 2424 RVA: 0x000640DC File Offset: 0x000622DC
		public void setRunning(bool isRunning, bool force = false)
		{
			if (this.canOnlyWalk || (this.bathingClothes.Value && !this.running) || (Game1.CurrentEvent != null && isRunning && !Game1.CurrentEvent.isFestival && !Game1.CurrentEvent.playerControlSequence && (this.controller == null || !this.controller.allowPlayerPathingInEvent)))
			{
				return;
			}
			if (this.isRidingHorse())
			{
				this.running = true;
				return;
			}
			if (this.stamina <= 0f)
			{
				base.speed = 2;
				if (this.running)
				{
					this.Halt();
				}
				this.running = false;
				return;
			}
			if (!force && (!this.CanMove || this.isEating || Game1.currentLocation == null || (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.playerControlSequence) || (!isRunning && this.UsingTool) || (this.Sprite != null && ((FarmerSprite)this.Sprite).PauseForSingleAnimation)))
			{
				if (this.UsingTool)
				{
					this.running = isRunning;
					if (this.running)
					{
						base.speed = 5;
						return;
					}
					base.speed = 2;
				}
				return;
			}
			this.running = isRunning;
			if (this.running)
			{
				base.speed = 5;
				return;
			}
			base.speed = 2;
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x0006421B File Offset: 0x0006241B
		public void addSeenResponse(string id)
		{
			this.dialogueQuestionsAnswered.Add(id);
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x0006422C File Offset: 0x0006242C
		public void eatObject(Object o, bool overrideFullness = false)
		{
			if (((o != null) ? o.QualifiedItemId : null) == "(O)434")
			{
				Game1.MusicDuckTimer = 10000f;
				Game1.changeMusicTrack("none", false, MusicContext.Default);
				Game1.multiplayer.globalChatInfoMessage("Stardrop", new string[]
				{
					base.Name
				});
			}
			if (base.getFacingDirection() != 2)
			{
				this.faceDirection(2);
			}
			this.itemToEat = o;
			this.mostRecentlyGrabbedItem = o;
			this.forceCanMove();
			this.completelyStopAnimatingOrDoingAction();
			ObjectData data;
			if (Game1.objectData.TryGetValue(o.ItemId, out data) && data.IsDrink)
			{
				if (this.IsLocalPlayer && this.hasBuff("7") && !overrideFullness)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2898")));
					return;
				}
				this.drinkAnimationEvent.Fire(o.getOne() as Object);
			}
			else if (o.Edibility != -300)
			{
				if (this.hasBuff("6") && !overrideFullness)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2899")));
					return;
				}
				this.eatAnimationEvent.Fire(o.getOne() as Object);
			}
			this.freezePause = 20000;
			this.CanMove = false;
			this.isEating = true;
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x00064380 File Offset: 0x00062580
		public override void DrawShadow(SpriteBatch b)
		{
			float drawLayer = this.getDrawLayer() - 1E-06f;
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(this.GetShadowOffset() + base.Position + new Vector2(32f, 24f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f - (((this.running || this.UsingTool) && this.FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[this.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, drawLayer);
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x00064468 File Offset: 0x00062668
		private void performDrinkAnimation(Object item)
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (!this.IsLocalPlayer)
			{
				this.itemToEat = item;
			}
			this.FarmerSprite.animateOnce(294, 80f, 8);
			this.isEating = true;
			if (item != null && item.HasContextTag("mayo_item"))
			{
				NPC npc = Utility.isThereAFarmerOrCharacterWithinDistance(base.Tile, 7, base.currentLocation) as NPC;
				if (npc != null && npc.Age != 2)
				{
					int whichMessage = Game1.random.Next(3);
					if (npc.Manners == 2 || npc.SocialAnxiety == 1)
					{
						whichMessage = 3;
					}
					if (npc.Name == "Emily" || npc.Name == "Sandy" || npc.Name == "Linus" || (npc.Name == "Krobus" && item.QualifiedItemId == "(O)308"))
					{
						whichMessage = 4;
					}
					else if (npc.Name == "Krobus" || npc.Name == "Dwarf" || npc is Monster || npc is Horse || npc is Pet || npc is Child)
					{
						return;
					}
					npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Mayo_reaction" + whichMessage.ToString()), null, 2, 3000, 500);
					npc.faceTowardFarmerForPeriod(1500, 7, false, this);
				}
			}
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x000645F8 File Offset: 0x000627F8
		public Farmer CreateFakeEventFarmer()
		{
			Farmer fake_farmer = new Farmer(new FarmerSprite(this.FarmerSprite.textureName.Value), new Vector2(192f, 192f), 1, "", new List<Item>(), this.IsMale);
			fake_farmer.Name = base.Name;
			fake_farmer.displayName = this.displayName;
			fake_farmer.isFakeEventActor = true;
			fake_farmer.changeGender(this.IsMale);
			fake_farmer.changeHairStyle(this.hair.Value);
			fake_farmer.UniqueMultiplayerID = this.UniqueMultiplayerID;
			fake_farmer.shirtItem.Set(this.shirtItem.Value);
			fake_farmer.pantsItem.Set(this.pantsItem.Value);
			fake_farmer.shirt.Set(this.shirt.Value);
			fake_farmer.pants.Set(this.pants.Value);
			foreach (Trinket t in this.trinketItems)
			{
				fake_farmer.trinketItems.Add((Trinket)((t != null) ? t.getOne() : null));
			}
			fake_farmer.changeShoeColor(this.shoes.Value);
			fake_farmer.boots.Set(this.boots.Value);
			fake_farmer.leftRing.Set(this.leftRing.Value);
			fake_farmer.rightRing.Set(this.rightRing.Value);
			fake_farmer.hat.Set(this.hat.Value);
			fake_farmer.pantsColor.Set(this.pantsColor.Value);
			fake_farmer.changeHairColor(this.hairstyleColor.Value);
			fake_farmer.changeSkinColor(this.skin.Value, false);
			fake_farmer.accessory.Set(this.accessory.Value);
			fake_farmer.changeEyeColor(this.newEyeColor.Value);
			fake_farmer.UpdateClothing();
			return fake_farmer;
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x0006480C File Offset: 0x00062A0C
		private void performEatAnimation(Object item)
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (!this.IsLocalPlayer)
			{
				this.itemToEat = item;
			}
			this.FarmerSprite.animateOnce(216, 80f, 8);
			this.isEating = true;
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x00064848 File Offset: 0x00062A48
		public void netDoEmote(string emote_type)
		{
			this.doEmoteEvent.Fire(emote_type);
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x00064856 File Offset: 0x00062A56
		private void performSickAnimation()
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			this.isEating = false;
			this.FarmerSprite.animateOnce(224, 350f, 4);
			this.doEmote(12);
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0006488C File Offset: 0x00062A8C
		public void eatHeldObject()
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (Game1.fadeToBlack)
			{
				return;
			}
			Item oldActiveItem = null;
			int oldToolIndex = 0;
			bool swappedActiveObject = false;
			bool wasStowed = false;
			if (this.ActiveItem == null || this.ActiveItem != this.mostRecentlyGrabbedItem)
			{
				if (this.netItemStowed.Value)
				{
					wasStowed = true;
					this.netItemStowed.Value = false;
					this.UpdateItemStow();
				}
				if (this.ActiveItem == null)
				{
					this.ActiveItem = this.mostRecentlyGrabbedItem;
				}
				else if (this.ActiveItem != this.mostRecentlyGrabbedItem)
				{
					oldToolIndex = this.currentToolIndex.Value;
					if (this.currentToolIndex.Value < 0 || this.currentToolIndex.Value >= this.Items.Count)
					{
						this.currentToolIndex.Value = 0;
					}
					oldActiveItem = this.Items[this.currentToolIndex.Value];
					this.Items[this.currentToolIndex.Value] = this.mostRecentlyGrabbedItem;
					this.OnItemReceived(this.mostRecentlyGrabbedItem, this.mostRecentlyGrabbedItem.Stack, null, false);
					swappedActiveObject = true;
				}
			}
			this.eatObject(this.ActiveObject, false);
			if (this.isEating)
			{
				this.reduceActiveItemByOne();
				this.CanMove = false;
			}
			if (swappedActiveObject)
			{
				this.Items[this.currentToolIndex.Value] = oldActiveItem;
				this.currentToolIndex.Value = oldToolIndex;
			}
			if (wasStowed)
			{
				this.netItemStowed.Value = true;
				this.UpdateItemStow();
			}
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x00064A04 File Offset: 0x00062C04
		public void grabObject(Object obj)
		{
			if (this.isEmoteAnimating)
			{
				this.EndEmoteAnimation();
			}
			if (obj != null)
			{
				this.CanMove = false;
				switch (this.FacingDirection)
				{
				case 0:
					((FarmerSprite)this.Sprite).animateOnce(80, 50f, 8);
					break;
				case 1:
					((FarmerSprite)this.Sprite).animateOnce(72, 50f, 8);
					break;
				case 2:
					((FarmerSprite)this.Sprite).animateOnce(64, 50f, 8);
					break;
				case 3:
					((FarmerSprite)this.Sprite).animateOnce(88, 50f, 8);
					break;
				}
				Game1.playSound("pickUpItem", null);
			}
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x00064AC8 File Offset: 0x00062CC8
		public virtual void PlayFishBiteChime()
		{
			int bite_chime = this.biteChime.Value;
			if (bite_chime < 0)
			{
				bite_chime = Game1.game1.instanceIndex;
			}
			if (bite_chime > 3)
			{
				bite_chime = 3;
			}
			if (bite_chime == 0)
			{
				base.playNearbySoundLocal("fishBite", null, SoundContext.Default);
				return;
			}
			base.playNearbySoundLocal("fishBite_alternate_" + (bite_chime - 1).ToString(), null, SoundContext.Default);
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x00064B34 File Offset: 0x00062D34
		public string getTitle()
		{
			int level = this.Level;
			if (level >= 30)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2016");
			}
			switch (level)
			{
			case 3:
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2031");
			case 5:
			case 6:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2030");
			case 7:
			case 8:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2029");
			case 9:
			case 10:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2028");
			case 11:
			case 12:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2027");
			case 13:
			case 14:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2026");
			case 15:
			case 16:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2025");
			case 17:
			case 18:
				if (!this.IsMale)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2024");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2023");
			case 19:
			case 20:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2022");
			case 21:
			case 22:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2021");
			case 23:
			case 24:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2020");
			case 25:
			case 26:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2019");
			case 27:
			case 28:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2018");
			case 29:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2017");
			default:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2032");
			}
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x00064CDD File Offset: 0x00062EDD
		public void queueMessage(byte messageType, Farmer sourceFarmer, params object[] data)
		{
			this.queueMessage(new OutgoingMessage(messageType, sourceFarmer, data));
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00064CED File Offset: 0x00062EED
		public void queueMessage(OutgoingMessage message)
		{
			this.messageQueue.Add(message);
		}

		// Token: 0x04000460 RID: 1120
		public const int millisecondsPerSpeedUnit = 64;

		// Token: 0x04000461 RID: 1121
		public const byte halt = 64;

		// Token: 0x04000462 RID: 1122
		public const byte up = 1;

		// Token: 0x04000463 RID: 1123
		public const byte right = 2;

		// Token: 0x04000464 RID: 1124
		public const byte down = 4;

		// Token: 0x04000465 RID: 1125
		public const byte left = 8;

		// Token: 0x04000466 RID: 1126
		public const byte run = 16;

		// Token: 0x04000467 RID: 1127
		public const byte release = 32;

		// Token: 0x04000468 RID: 1128
		public const int farmingSkill = 0;

		// Token: 0x04000469 RID: 1129
		public const int miningSkill = 3;

		// Token: 0x0400046A RID: 1130
		public const int fishingSkill = 1;

		// Token: 0x0400046B RID: 1131
		public const int foragingSkill = 2;

		// Token: 0x0400046C RID: 1132
		public const int combatSkill = 4;

		// Token: 0x0400046D RID: 1133
		public const int luckSkill = 5;

		// Token: 0x0400046E RID: 1134
		public const float interpolationConstant = 0.5f;

		// Token: 0x0400046F RID: 1135
		public const int runningSpeed = 5;

		// Token: 0x04000470 RID: 1136
		public const int walkingSpeed = 2;

		// Token: 0x04000471 RID: 1137
		public const int caveNothing = 0;

		// Token: 0x04000472 RID: 1138
		public const int caveBats = 1;

		// Token: 0x04000473 RID: 1139
		public const int caveMushrooms = 2;

		// Token: 0x04000474 RID: 1140
		public const int millisecondsInvincibleAfterDamage = 1200;

		// Token: 0x04000475 RID: 1141
		public const int millisecondsPerFlickerWhenInvincible = 50;

		// Token: 0x04000476 RID: 1142
		public const int startingStamina = 270;

		// Token: 0x04000477 RID: 1143
		public const int totalLevels = 35;

		// Token: 0x04000478 RID: 1144
		public const int maxInventorySpace = 36;

		// Token: 0x04000479 RID: 1145
		public const int hotbarSize = 12;

		// Token: 0x0400047A RID: 1146
		public const int eyesOpen = 0;

		// Token: 0x0400047B RID: 1147
		public const int eyesHalfShut = 4;

		// Token: 0x0400047C RID: 1148
		public const int eyesClosed = 1;

		// Token: 0x0400047D RID: 1149
		public const int eyesRight = 2;

		// Token: 0x0400047E RID: 1150
		public const int eyesLeft = 3;

		// Token: 0x0400047F RID: 1151
		public const int eyesWide = 5;

		// Token: 0x04000480 RID: 1152
		public const int rancher = 0;

		// Token: 0x04000481 RID: 1153
		public const int tiller = 1;

		// Token: 0x04000482 RID: 1154
		public const int butcher = 2;

		// Token: 0x04000483 RID: 1155
		public const int shepherd = 3;

		// Token: 0x04000484 RID: 1156
		public const int artisan = 4;

		// Token: 0x04000485 RID: 1157
		public const int agriculturist = 5;

		// Token: 0x04000486 RID: 1158
		public const int fisher = 6;

		// Token: 0x04000487 RID: 1159
		public const int trapper = 7;

		// Token: 0x04000488 RID: 1160
		public const int angler = 8;

		// Token: 0x04000489 RID: 1161
		public const int pirate = 9;

		// Token: 0x0400048A RID: 1162
		public const int baitmaster = 10;

		// Token: 0x0400048B RID: 1163
		public const int mariner = 11;

		// Token: 0x0400048C RID: 1164
		public const int forester = 12;

		// Token: 0x0400048D RID: 1165
		public const int gatherer = 13;

		// Token: 0x0400048E RID: 1166
		public const int lumberjack = 14;

		// Token: 0x0400048F RID: 1167
		public const int tapper = 15;

		// Token: 0x04000490 RID: 1168
		public const int botanist = 16;

		// Token: 0x04000491 RID: 1169
		public const int tracker = 17;

		// Token: 0x04000492 RID: 1170
		public const int miner = 18;

		// Token: 0x04000493 RID: 1171
		public const int geologist = 19;

		// Token: 0x04000494 RID: 1172
		public const int blacksmith = 20;

		// Token: 0x04000495 RID: 1173
		public const int burrower = 21;

		// Token: 0x04000496 RID: 1174
		public const int excavator = 22;

		// Token: 0x04000497 RID: 1175
		public const int gemologist = 23;

		// Token: 0x04000498 RID: 1176
		public const int fighter = 24;

		// Token: 0x04000499 RID: 1177
		public const int scout = 25;

		// Token: 0x0400049A RID: 1178
		public const int brute = 26;

		// Token: 0x0400049B RID: 1179
		public const int defender = 27;

		// Token: 0x0400049C RID: 1180
		public const int acrobat = 28;

		// Token: 0x0400049D RID: 1181
		public const int desperado = 29;

		// Token: 0x0400049E RID: 1182
		public static int MaximumTrinkets = 1;

		// Token: 0x0400049F RID: 1183
		public readonly NetObjectList<Quest> questLog = new NetObjectList<Quest>();

		// Token: 0x040004A0 RID: 1184
		public readonly NetIntHashSet professions = new NetIntHashSet();

		// Token: 0x040004A1 RID: 1185
		public readonly NetList<Point, NetPoint> newLevels = new NetList<Point, NetPoint>();

		// Token: 0x040004A2 RID: 1186
		[XmlIgnore]
		public Queue<int> newLevelSparklingTexts = new Queue<int>();

		// Token: 0x040004A3 RID: 1187
		[XmlIgnore]
		public SparklingText sparklingText;

		// Token: 0x040004A4 RID: 1188
		public readonly NetArray<int, NetInt> experiencePoints = new NetArray<int, NetInt>(6);

		// Token: 0x040004A5 RID: 1189
		[XmlElement("items")]
		public readonly NetRef<Inventory> netItems = new NetRef<Inventory>(new Inventory());

		// Token: 0x040004A6 RID: 1190
		[XmlArrayItem("int")]
		public readonly NetStringHashSet dialogueQuestionsAnswered = new NetStringHashSet();

		// Token: 0x040004A7 RID: 1191
		[XmlElement("cookingRecipes")]
		public readonly NetStringDictionary<int, NetInt> cookingRecipes = new NetStringDictionary<int, NetInt>();

		// Token: 0x040004A8 RID: 1192
		[XmlElement("craftingRecipes")]
		public readonly NetStringDictionary<int, NetInt> craftingRecipes = new NetStringDictionary<int, NetInt>();

		// Token: 0x040004A9 RID: 1193
		[XmlElement("activeDialogueEvents")]
		public readonly NetStringDictionary<int, NetInt> activeDialogueEvents = new NetStringDictionary<int, NetInt>();

		// Token: 0x040004AA RID: 1194
		[XmlElement("previousActiveDialogueEvents")]
		public readonly NetStringDictionary<int, NetInt> previousActiveDialogueEvents = new NetStringDictionary<int, NetInt>();

		// Token: 0x040004AB RID: 1195
		public readonly NetStringHashSet triggerActionsRun = new NetStringHashSet();

		// Token: 0x040004AC RID: 1196
		[XmlArrayItem("int")]
		public readonly NetStringHashSet eventsSeen = new NetStringHashSet();

		// Token: 0x040004AD RID: 1197
		public readonly NetIntHashSet secretNotesSeen = new NetIntHashSet();

		// Token: 0x040004AE RID: 1198
		public HashSet<string> songsHeard = new HashSet<string>();

		// Token: 0x040004AF RID: 1199
		public readonly NetIntHashSet achievements = new NetIntHashSet();

		// Token: 0x040004B0 RID: 1200
		[XmlArrayItem("int")]
		public readonly NetStringList specialItems = new NetStringList();

		// Token: 0x040004B1 RID: 1201
		[XmlArrayItem("int")]
		public readonly NetStringList specialBigCraftables = new NetStringList();

		// Token: 0x040004B2 RID: 1202
		public readonly NetStringHashSet mailReceived = new NetStringHashSet();

		// Token: 0x040004B3 RID: 1203
		public readonly NetStringHashSet mailForTomorrow = new NetStringHashSet();

		// Token: 0x040004B4 RID: 1204
		public readonly NetStringList mailbox = new NetStringList();

		// Token: 0x040004B5 RID: 1205
		public readonly NetStringHashSet locationsVisited = new NetStringHashSet();

		// Token: 0x040004B6 RID: 1206
		public readonly NetInt timeWentToBed = new NetInt();

		// Token: 0x040004B7 RID: 1207
		[XmlIgnore]
		public readonly NetList<Companion, NetRef<Companion>> companions = new NetList<Companion, NetRef<Companion>>();

		// Token: 0x040004B8 RID: 1208
		[XmlIgnore]
		public bool hasMoved;

		// Token: 0x040004B9 RID: 1209
		[XmlIgnore]
		public bool hasBeenBlessedByStatueToday;

		// Token: 0x040004BA RID: 1210
		public readonly NetBool sleptInTemporaryBed = new NetBool();

		// Token: 0x040004BB RID: 1211
		[XmlIgnore]
		public readonly NetBool requestingTimePause = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x040004BC RID: 1212
		public Stats stats = new Stats();

		// Token: 0x040004BD RID: 1213
		[XmlIgnore]
		public readonly NetRef<Inventory> personalShippingBin = new NetRef<Inventory>(new Inventory());

		// Token: 0x040004BE RID: 1214
		[XmlIgnore]
		public IList<Item> displayedShippedItems = new List<Item>();

		// Token: 0x040004BF RID: 1215
		[XmlElement("biteChime")]
		public NetInt biteChime = new NetInt(-1);

		// Token: 0x040004C0 RID: 1216
		[XmlIgnore]
		public float usernameDisplayTime;

		// Token: 0x040004C1 RID: 1217
		[XmlIgnore]
		protected NetRef<Item> _recoveredItem = new NetRef<Item>();

		// Token: 0x040004C2 RID: 1218
		public NetObjectList<Item> itemsLostLastDeath = new NetObjectList<Item>();

		// Token: 0x040004C3 RID: 1219
		public List<int> movementDirections = new List<int>();

		// Token: 0x040004C4 RID: 1220
		[XmlElement("farmName")]
		public readonly NetString farmName = new NetString("");

		// Token: 0x040004C5 RID: 1221
		[XmlElement("favoriteThing")]
		public readonly NetString favoriteThing = new NetString();

		// Token: 0x040004C6 RID: 1222
		[XmlElement("horseName")]
		public readonly NetString horseName = new NetString();

		// Token: 0x040004C7 RID: 1223
		public string slotName;

		// Token: 0x040004C8 RID: 1224
		public bool slotCanHost;

		// Token: 0x040004C9 RID: 1225
		[XmlIgnore]
		public readonly NetString tempFoodItemTextureName = new NetString();

		// Token: 0x040004CA RID: 1226
		[XmlIgnore]
		public readonly NetRectangle tempFoodItemSourceRect = new NetRectangle();

		// Token: 0x040004CB RID: 1227
		[XmlIgnore]
		public bool hasReceivedToolUpgradeMessageYet;

		// Token: 0x040004CC RID: 1228
		[XmlIgnore]
		public readonly BuffManager buffs = new BuffManager();

		// Token: 0x040004CD RID: 1229
		[XmlIgnore]
		public IList<OutgoingMessage> messageQueue = new List<OutgoingMessage>();

		// Token: 0x040004CE RID: 1230
		[XmlIgnore]
		public readonly NetLong uniqueMultiplayerID = new NetLong(Utility.RandomLong(null));

		// Token: 0x040004CF RID: 1231
		[XmlElement("userID")]
		public readonly NetString userID = new NetString("");

		// Token: 0x040004D0 RID: 1232
		[XmlIgnore]
		public string previousLocationName = "";

		// Token: 0x040004D1 RID: 1233
		[XmlIgnore]
		public readonly NetString platformType = new NetString("");

		// Token: 0x040004D2 RID: 1234
		[XmlIgnore]
		public readonly NetString platformID = new NetString("");

		// Token: 0x040004D3 RID: 1235
		[XmlIgnore]
		public readonly NetBool hasMenuOpen = new NetBool(false);

		// Token: 0x040004D4 RID: 1236
		[XmlIgnore]
		public readonly Color DEFAULT_SHIRT_COLOR = Color.White;

		// Token: 0x040004D5 RID: 1237
		public string defaultChatColor;

		// Token: 0x040004D6 RID: 1238
		[XmlElement("catPerson")]
		public bool? obsolete_catPerson;

		// Token: 0x040004D7 RID: 1239
		[XmlElement("canUnderstandDwarves")]
		public bool? obsolete_canUnderstandDwarves;

		// Token: 0x040004D8 RID: 1240
		[XmlElement("hasClubCard")]
		public bool? obsolete_hasClubCard;

		// Token: 0x040004D9 RID: 1241
		[XmlElement("hasDarkTalisman")]
		public bool? obsolete_hasDarkTalisman;

		// Token: 0x040004DA RID: 1242
		[XmlElement("hasMagicInk")]
		public bool? obsolete_hasMagicInk;

		// Token: 0x040004DB RID: 1243
		[XmlElement("hasMagnifyingGlass")]
		public bool? obsolete_hasMagnifyingGlass;

		// Token: 0x040004DC RID: 1244
		[XmlElement("hasRustyKey")]
		public bool? obsolete_hasRustyKey;

		// Token: 0x040004DD RID: 1245
		[XmlElement("hasSkullKey")]
		public bool? obsolete_hasSkullKey;

		// Token: 0x040004DE RID: 1246
		[XmlElement("hasSpecialCharm")]
		public bool? obsolete_hasSpecialCharm;

		// Token: 0x040004DF RID: 1247
		[XmlElement("HasTownKey")]
		public bool? obsolete_hasTownKey;

		// Token: 0x040004E0 RID: 1248
		[XmlElement("hasUnlockedSkullDoor")]
		public bool? obsolete_hasUnlockedSkullDoor;

		// Token: 0x040004E1 RID: 1249
		[XmlElement("friendships")]
		public SerializableDictionary<string, int[]> obsolete_friendships;

		// Token: 0x040004E2 RID: 1250
		[XmlElement("daysMarried")]
		public int? obsolete_daysMarried;

		// Token: 0x040004E3 RID: 1251
		public string whichPetType = "Cat";

		// Token: 0x040004E4 RID: 1252
		public string whichPetBreed = "0";

		// Token: 0x040004E5 RID: 1253
		[XmlIgnore]
		public bool isAnimatingMount;

		// Token: 0x040004E6 RID: 1254
		[XmlElement("acceptedDailyQuest")]
		public readonly NetBool acceptedDailyQuest = new NetBool(false);

		// Token: 0x040004E7 RID: 1255
		[XmlIgnore]
		public Item mostRecentlyGrabbedItem;

		// Token: 0x040004E8 RID: 1256
		[XmlIgnore]
		public Item itemToEat;

		// Token: 0x040004E9 RID: 1257
		[XmlElement("farmerRenderer")]
		private readonly NetRef<FarmerRenderer> farmerRenderer = new NetRef<FarmerRenderer>();

		// Token: 0x040004EA RID: 1258
		[XmlIgnore]
		public readonly NetInt toolPower = new NetInt();

		// Token: 0x040004EB RID: 1259
		[XmlIgnore]
		public readonly NetInt toolHold = new NetInt();

		// Token: 0x040004EC RID: 1260
		public Vector2 mostRecentBed;

		// Token: 0x040004ED RID: 1261
		public static Dictionary<int, string> hairStyleMetadataFile = null;

		// Token: 0x040004EE RID: 1262
		public static List<int> allHairStyleIndices = null;

		// Token: 0x040004EF RID: 1263
		[XmlIgnore]
		public static Dictionary<int, HairStyleMetadata> hairStyleMetadata = new Dictionary<int, HairStyleMetadata>();

		// Token: 0x040004F0 RID: 1264
		[XmlElement("emoteFavorites")]
		public readonly List<string> emoteFavorites = new List<string>();

		// Token: 0x040004F1 RID: 1265
		[XmlElement("performedEmotes")]
		public readonly SerializableDictionary<string, bool> performedEmotes = new SerializableDictionary<string, bool>();

		// Token: 0x040004F2 RID: 1266
		[XmlElement("shirt")]
		public readonly NetString shirt = new NetString("1000");

		// Token: 0x040004F3 RID: 1267
		[XmlElement("hair")]
		public readonly NetInt hair = new NetInt(0);

		// Token: 0x040004F4 RID: 1268
		[XmlElement("skin")]
		public readonly NetInt skin = new NetInt(0);

		// Token: 0x040004F5 RID: 1269
		[XmlElement("shoes")]
		public readonly NetString shoes = new NetString("2");

		// Token: 0x040004F6 RID: 1270
		[XmlElement("accessory")]
		public readonly NetInt accessory = new NetInt(-1);

		// Token: 0x040004F7 RID: 1271
		[XmlElement("facialHair")]
		public readonly NetInt facialHair = new NetInt(-1);

		// Token: 0x040004F8 RID: 1272
		[XmlElement("pants")]
		public readonly NetString pants = new NetString("0");

		// Token: 0x040004F9 RID: 1273
		[XmlIgnore]
		public int currentEyes;

		// Token: 0x040004FA RID: 1274
		[XmlIgnore]
		public int blinkTimer;

		// Token: 0x040004FB RID: 1275
		[XmlIgnore]
		public readonly NetInt netFestivalScore = new NetInt();

		// Token: 0x040004FC RID: 1276
		public readonly NetRef<WorldDate> lastGotPrizeFromGil = new NetRef<WorldDate>();

		// Token: 0x040004FD RID: 1277
		public readonly NetRef<WorldDate> lastDesertFestivalFishingQuest = new NetRef<WorldDate>();

		// Token: 0x040004FE RID: 1278
		[XmlIgnore]
		public float temporarySpeedBuff;

		// Token: 0x040004FF RID: 1279
		[XmlElement("hairstyleColor")]
		public readonly NetColor hairstyleColor = new NetColor(new Color(193, 90, 50));

		// Token: 0x04000500 RID: 1280
		[XmlIgnore]
		public NetBool prismaticHair = new NetBool();

		// Token: 0x04000501 RID: 1281
		[XmlElement("pantsColor")]
		public readonly NetColor pantsColor = new NetColor(new Color(46, 85, 183));

		// Token: 0x04000502 RID: 1282
		[XmlElement("newEyeColor")]
		public readonly NetColor newEyeColor = new NetColor(new Color(122, 68, 52));

		// Token: 0x04000503 RID: 1283
		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		// Token: 0x04000504 RID: 1284
		[XmlElement("boots")]
		public readonly NetRef<Boots> boots = new NetRef<Boots>();

		// Token: 0x04000505 RID: 1285
		[XmlElement("leftRing")]
		public readonly NetRef<Ring> leftRing = new NetRef<Ring>();

		// Token: 0x04000506 RID: 1286
		[XmlElement("rightRing")]
		public readonly NetRef<Ring> rightRing = new NetRef<Ring>();

		// Token: 0x04000507 RID: 1287
		[XmlElement("shirtItem")]
		public readonly NetRef<Clothing> shirtItem = new NetRef<Clothing>();

		// Token: 0x04000508 RID: 1288
		[XmlElement("pantsItem")]
		public readonly NetRef<Clothing> pantsItem = new NetRef<Clothing>();

		// Token: 0x04000509 RID: 1289
		[XmlIgnore]
		public readonly NetDancePartner dancePartner = new NetDancePartner();

		// Token: 0x0400050A RID: 1290
		[XmlIgnore]
		public bool ridingMineElevator;

		// Token: 0x0400050B RID: 1291
		[XmlIgnore]
		public readonly NetBool exhausted = new NetBool();

		// Token: 0x0400050C RID: 1292
		[XmlElement("divorceTonight")]
		public readonly NetBool divorceTonight = new NetBool();

		// Token: 0x0400050D RID: 1293
		[XmlElement("changeWalletTypeTonight")]
		public readonly NetBool changeWalletTypeTonight = new NetBool();

		// Token: 0x0400050E RID: 1294
		[XmlIgnore]
		public AnimatedSprite.endOfAnimationBehavior toolOverrideFunction;

		// Token: 0x0400050F RID: 1295
		[XmlIgnore]
		public NetBool onBridge = new NetBool();

		// Token: 0x04000510 RID: 1296
		[XmlIgnore]
		public SuspensionBridge bridge;

		// Token: 0x04000511 RID: 1297
		private readonly NetInt netDeepestMineLevel = new NetInt();

		// Token: 0x04000512 RID: 1298
		[XmlElement("currentToolIndex")]
		private readonly NetInt currentToolIndex = new NetInt(0);

		// Token: 0x04000513 RID: 1299
		[XmlIgnore]
		private readonly NetRef<Item> temporaryItem = new NetRef<Item>();

		// Token: 0x04000514 RID: 1300
		[XmlIgnore]
		private readonly NetRef<Item> cursorSlotItem = new NetRef<Item>();

		// Token: 0x04000515 RID: 1301
		[XmlIgnore]
		public readonly NetBool netItemStowed = new NetBool(false);

		// Token: 0x04000516 RID: 1302
		protected bool _itemStowed;

		// Token: 0x04000517 RID: 1303
		public string gameVersion = "-1";

		// Token: 0x04000518 RID: 1304
		public string gameVersionLabel;

		// Token: 0x04000519 RID: 1305
		[XmlIgnore]
		public bool isFakeEventActor;

		// Token: 0x0400051A RID: 1306
		[XmlElement("bibberstyke")]
		public readonly NetInt bobberStyle = new NetInt(0);

		// Token: 0x0400051B RID: 1307
		public bool usingRandomizedBobber;

		// Token: 0x0400051C RID: 1308
		[XmlElement("caveChoice")]
		public readonly NetInt caveChoice = new NetInt();

		// Token: 0x0400051D RID: 1309
		[XmlElement("farmingLevel")]
		public readonly NetInt farmingLevel = new NetInt();

		// Token: 0x0400051E RID: 1310
		[XmlElement("miningLevel")]
		public readonly NetInt miningLevel = new NetInt();

		// Token: 0x0400051F RID: 1311
		[XmlElement("combatLevel")]
		public readonly NetInt combatLevel = new NetInt();

		// Token: 0x04000520 RID: 1312
		[XmlElement("foragingLevel")]
		public readonly NetInt foragingLevel = new NetInt();

		// Token: 0x04000521 RID: 1313
		[XmlElement("fishingLevel")]
		public readonly NetInt fishingLevel = new NetInt();

		// Token: 0x04000522 RID: 1314
		[XmlElement("luckLevel")]
		public readonly NetInt luckLevel = new NetInt();

		// Token: 0x04000523 RID: 1315
		[XmlElement("maxStamina")]
		public readonly NetInt maxStamina = new NetInt(270);

		// Token: 0x04000524 RID: 1316
		[XmlElement("maxItems")]
		public readonly NetInt maxItems = new NetInt(12);

		// Token: 0x04000525 RID: 1317
		[XmlElement("lastSeenMovieWeek")]
		public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

		// Token: 0x04000526 RID: 1318
		[XmlIgnore]
		public readonly NetString viewingLocation = new NetString();

		// Token: 0x04000527 RID: 1319
		private readonly NetFloat netStamina = new NetFloat(270f);

		// Token: 0x04000528 RID: 1320
		[XmlIgnore]
		public bool ignoreItemConsumptionThisFrame;

		// Token: 0x04000529 RID: 1321
		[XmlIgnore]
		[NotNetField]
		public NetRoot<FarmerTeam> teamRoot = new NetRoot<FarmerTeam>(new FarmerTeam());

		// Token: 0x0400052A RID: 1322
		public int clubCoins;

		// Token: 0x0400052B RID: 1323
		public int trashCanLevel;

		// Token: 0x0400052C RID: 1324
		private NetLong netMillisecondsPlayed = new NetLong
		{
			DeltaAggregateTicks = (ushort)(60 * (Game1.realMilliSecondsPerGameTenMinutes / 1000))
		};

		// Token: 0x0400052D RID: 1325
		[XmlElement("toolBeingUpgraded")]
		public readonly NetRef<Tool> toolBeingUpgraded = new NetRef<Tool>();

		// Token: 0x0400052E RID: 1326
		[XmlElement("daysLeftForToolUpgrade")]
		public readonly NetInt daysLeftForToolUpgrade = new NetInt();

		// Token: 0x0400052F RID: 1327
		[XmlElement("houseUpgradeLevel")]
		public readonly NetInt houseUpgradeLevel = new NetInt(0);

		// Token: 0x04000530 RID: 1328
		[XmlElement("daysUntilHouseUpgrade")]
		public readonly NetInt daysUntilHouseUpgrade = new NetInt(-1);

		// Token: 0x04000531 RID: 1329
		public bool showChestColorPicker = true;

		// Token: 0x04000532 RID: 1330
		public bool hasWateringCanEnchantment;

		// Token: 0x04000533 RID: 1331
		[XmlIgnore]
		public List<BaseEnchantment> enchantments = new List<BaseEnchantment>();

		// Token: 0x04000534 RID: 1332
		public readonly int BaseMagneticRadius = 128;

		// Token: 0x04000535 RID: 1333
		public int temporaryInvincibilityTimer;

		// Token: 0x04000536 RID: 1334
		public int currentTemporaryInvincibilityDuration = 1200;

		// Token: 0x04000537 RID: 1335
		[XmlIgnore]
		public float rotation;

		// Token: 0x04000538 RID: 1336
		private int craftingTime = 1000;

		// Token: 0x04000539 RID: 1337
		private int raftPuddleCounter = 250;

		// Token: 0x0400053A RID: 1338
		private int raftBobCounter = 1000;

		// Token: 0x0400053B RID: 1339
		public int health = 100;

		// Token: 0x0400053C RID: 1340
		public int maxHealth = 100;

		// Token: 0x0400053D RID: 1341
		private readonly NetInt netTimesReachedMineBottom = new NetInt(0);

		// Token: 0x0400053E RID: 1342
		public float difficultyModifier = 1f;

		// Token: 0x0400053F RID: 1343
		[XmlIgnore]
		public Vector2 jitter = Vector2.Zero;

		// Token: 0x04000540 RID: 1344
		[XmlIgnore]
		public Vector2 lastPosition;

		// Token: 0x04000541 RID: 1345
		[XmlIgnore]
		public Vector2 lastGrabTile = Vector2.Zero;

		// Token: 0x04000542 RID: 1346
		[XmlIgnore]
		public float jitterStrength;

		// Token: 0x04000543 RID: 1347
		[XmlIgnore]
		public float xOffset;

		// Token: 0x04000544 RID: 1348
		[XmlElement("gender")]
		public readonly NetEnum<Gender> netGender = new NetEnum<Gender>();

		// Token: 0x04000545 RID: 1349
		[XmlIgnore]
		public bool canMove = true;

		// Token: 0x04000546 RID: 1350
		[XmlIgnore]
		public bool running;

		// Token: 0x04000547 RID: 1351
		[XmlIgnore]
		public bool ignoreCollisions;

		// Token: 0x04000548 RID: 1352
		[XmlIgnore]
		public readonly NetBool usingTool = new NetBool(false);

		// Token: 0x04000549 RID: 1353
		[XmlIgnore]
		public bool isEating;

		// Token: 0x0400054A RID: 1354
		[XmlIgnore]
		public readonly NetBool isInBed = new NetBool(false);

		// Token: 0x0400054B RID: 1355
		[XmlIgnore]
		public bool forceTimePass;

		// Token: 0x0400054C RID: 1356
		[XmlIgnore]
		public bool isRafting;

		// Token: 0x0400054D RID: 1357
		[XmlIgnore]
		public bool usingSlingshot;

		// Token: 0x0400054E RID: 1358
		[XmlIgnore]
		public readonly NetBool bathingClothes = new NetBool(false);

		// Token: 0x0400054F RID: 1359
		[XmlIgnore]
		public bool canOnlyWalk;

		// Token: 0x04000550 RID: 1360
		[XmlIgnore]
		public bool temporarilyInvincible;

		// Token: 0x04000551 RID: 1361
		[XmlIgnore]
		public bool flashDuringThisTemporaryInvincibility = true;

		// Token: 0x04000552 RID: 1362
		private readonly NetBool netCanReleaseTool = new NetBool(false);

		// Token: 0x04000553 RID: 1363
		[XmlIgnore]
		public bool isCrafting;

		// Token: 0x04000554 RID: 1364
		[XmlIgnore]
		public bool isEmoteAnimating;

		// Token: 0x04000555 RID: 1365
		[XmlIgnore]
		public bool passedOut;

		// Token: 0x04000556 RID: 1366
		[XmlIgnore]
		protected int _emoteGracePeriod;

		// Token: 0x04000557 RID: 1367
		[XmlIgnore]
		private BoundingBoxGroup temporaryPassableTiles = new BoundingBoxGroup();

		// Token: 0x04000558 RID: 1368
		[XmlIgnore]
		public readonly NetBool hidden = new NetBool();

		// Token: 0x04000559 RID: 1369
		[XmlElement("basicShipped")]
		public readonly NetStringDictionary<int, NetInt> basicShipped = new NetStringDictionary<int, NetInt>();

		// Token: 0x0400055A RID: 1370
		[XmlElement("mineralsFound")]
		public readonly NetStringDictionary<int, NetInt> mineralsFound = new NetStringDictionary<int, NetInt>();

		// Token: 0x0400055B RID: 1371
		[XmlElement("recipesCooked")]
		public readonly NetStringDictionary<int, NetInt> recipesCooked = new NetStringDictionary<int, NetInt>();

		// Token: 0x0400055C RID: 1372
		[XmlElement("fishCaught")]
		public readonly NetStringIntArrayDictionary fishCaught = new NetStringIntArrayDictionary();

		// Token: 0x0400055D RID: 1373
		[XmlElement("archaeologyFound")]
		public readonly NetStringIntArrayDictionary archaeologyFound = new NetStringIntArrayDictionary();

		// Token: 0x0400055E RID: 1374
		[XmlElement("callsReceived")]
		public readonly NetStringDictionary<int, NetInt> callsReceived = new NetStringDictionary<int, NetInt>();

		// Token: 0x0400055F RID: 1375
		public SerializableDictionary<string, SerializableDictionary<string, int>> giftedItems;

		// Token: 0x04000560 RID: 1376
		[XmlElement("tailoredItems")]
		public readonly NetStringDictionary<int, NetInt> tailoredItems = new NetStringDictionary<int, NetInt>();

		// Token: 0x04000561 RID: 1377
		[XmlElement("friendshipData")]
		public readonly NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetStringDictionary<Friendship, NetRef<Friendship>>();

		// Token: 0x04000562 RID: 1378
		[XmlIgnore]
		public NetString locationBeforeForcedEvent = new NetString(null);

		// Token: 0x04000563 RID: 1379
		[XmlIgnore]
		public Vector2 positionBeforeEvent;

		// Token: 0x04000564 RID: 1380
		[XmlIgnore]
		public int orientationBeforeEvent;

		// Token: 0x04000565 RID: 1381
		[XmlIgnore]
		public int swimTimer;

		// Token: 0x04000566 RID: 1382
		[XmlIgnore]
		public int regenTimer;

		// Token: 0x04000567 RID: 1383
		[XmlIgnore]
		public int timerSinceLastMovement;

		// Token: 0x04000568 RID: 1384
		[XmlIgnore]
		public int noMovementPause;

		// Token: 0x04000569 RID: 1385
		[XmlIgnore]
		public int freezePause;

		// Token: 0x0400056A RID: 1386
		[XmlIgnore]
		public float yOffset;

		// Token: 0x0400056B RID: 1387
		protected readonly NetString netSpouse = new NetString();

		// Token: 0x0400056C RID: 1388
		public string dateStringForSaveGame;

		// Token: 0x0400056D RID: 1389
		public int? dayOfMonthForSaveGame;

		// Token: 0x0400056E RID: 1390
		public int? seasonForSaveGame;

		// Token: 0x0400056F RID: 1391
		public int? yearForSaveGame;

		// Token: 0x04000570 RID: 1392
		[XmlIgnore]
		public Vector2 armOffset;

		// Token: 0x04000571 RID: 1393
		[XmlIgnore]
		public readonly NetRef<Horse> netMount = new NetRef<Horse>();

		// Token: 0x04000572 RID: 1394
		[XmlIgnore]
		public ISittable sittingFurniture;

		// Token: 0x04000573 RID: 1395
		[XmlIgnore]
		public NetBool isSitting = new NetBool();

		// Token: 0x04000574 RID: 1396
		[XmlIgnore]
		public NetVector2 mapChairSitPosition = new NetVector2(new Vector2(-1f, -1f));

		// Token: 0x04000575 RID: 1397
		[XmlIgnore]
		public NetBool hasCompletedAllMonsterSlayerQuests = new NetBool(false);

		// Token: 0x04000576 RID: 1398
		[XmlIgnore]
		public bool isStopSitting;

		// Token: 0x04000577 RID: 1399
		[XmlIgnore]
		protected bool _wasSitting;

		// Token: 0x04000578 RID: 1400
		[XmlIgnore]
		public Vector2 lerpStartPosition;

		// Token: 0x04000579 RID: 1401
		[XmlIgnore]
		public Vector2 lerpEndPosition;

		// Token: 0x0400057A RID: 1402
		[XmlIgnore]
		public float lerpPosition = -1f;

		// Token: 0x0400057B RID: 1403
		[XmlIgnore]
		public float lerpDuration = -1f;

		// Token: 0x0400057C RID: 1404
		[XmlIgnore]
		protected Item _lastSelectedItem;

		// Token: 0x0400057D RID: 1405
		[XmlIgnore]
		protected internal Tool _lastEquippedTool;

		// Token: 0x0400057E RID: 1406
		[XmlElement("qiGems")]
		public NetIntDelta netQiGems = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x0400057F RID: 1407
		[XmlElement("JOTPKProgress")]
		public NetRef<AbigailGame.JOTPKProgress> jotpkProgress = new NetRef<AbigailGame.JOTPKProgress>();

		// Token: 0x04000580 RID: 1408
		[XmlIgnore]
		public NetBool hasUsedDailyRevive = new NetBool(false);

		// Token: 0x04000581 RID: 1409
		[XmlElement("trinketItem")]
		public readonly NetList<Trinket, NetRef<Trinket>> trinketItems = new NetList<Trinket, NetRef<Trinket>>();

		// Token: 0x04000582 RID: 1410
		private readonly NetEvent0 fireToolEvent = new NetEvent0(true);

		// Token: 0x04000583 RID: 1411
		private readonly NetEvent0 beginUsingToolEvent = new NetEvent0(true);

		// Token: 0x04000584 RID: 1412
		private readonly NetEvent0 endUsingToolEvent = new NetEvent0(true);

		// Token: 0x04000585 RID: 1413
		private readonly NetEvent0 sickAnimationEvent = new NetEvent0(false);

		// Token: 0x04000586 RID: 1414
		private readonly NetEvent0 passOutEvent = new NetEvent0(false);

		// Token: 0x04000587 RID: 1415
		private readonly NetEvent0 haltAnimationEvent = new NetEvent0(false);

		// Token: 0x04000588 RID: 1416
		private readonly NetEvent1Field<Object, NetRef<Object>> drinkAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

		// Token: 0x04000589 RID: 1417
		private readonly NetEvent1Field<Object, NetRef<Object>> eatAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

		// Token: 0x0400058A RID: 1418
		private readonly NetEvent1Field<string, NetString> doEmoteEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x0400058B RID: 1419
		private readonly NetEvent1Field<long, NetLong> kissFarmerEvent = new NetEvent1Field<long, NetLong>();

		// Token: 0x0400058C RID: 1420
		private readonly NetEvent1Field<float, NetFloat> synchronizedJumpEvent = new NetEvent1Field<float, NetFloat>();

		// Token: 0x0400058D RID: 1421
		public readonly NetEvent1Field<string, NetString> renovateEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x0400058E RID: 1422
		[XmlElement("chestConsumedLevels")]
		public readonly NetIntDictionary<bool, NetBool> chestConsumedMineLevels = new NetIntDictionary<bool, NetBool>();

		// Token: 0x0400058F RID: 1423
		public int saveTime;

		// Token: 0x04000590 RID: 1424
		[XmlIgnore]
		public float drawLayerDisambiguator;

		// Token: 0x04000591 RID: 1425
		[XmlElement("isCustomized")]
		public readonly NetBool isCustomized = new NetBool(false);

		// Token: 0x04000592 RID: 1426
		[XmlElement("homeLocation")]
		public readonly NetString homeLocation = new NetString("FarmHouse");

		// Token: 0x04000593 RID: 1427
		[XmlElement("lastSleepLocation")]
		public readonly NetString lastSleepLocation = new NetString();

		// Token: 0x04000594 RID: 1428
		[XmlElement("lastSleepPoint")]
		public readonly NetPoint lastSleepPoint = new NetPoint();

		// Token: 0x04000595 RID: 1429
		[XmlElement("disconnectDay")]
		public readonly NetInt disconnectDay = new NetInt(-1);

		// Token: 0x04000596 RID: 1430
		[XmlElement("disconnectLocation")]
		public readonly NetString disconnectLocation = new NetString();

		// Token: 0x04000597 RID: 1431
		[XmlElement("disconnectPosition")]
		public readonly NetVector2 disconnectPosition = new NetVector2();

		// Token: 0x04000598 RID: 1432
		public static readonly Farmer.EmoteType[] EMOTES = new Farmer.EmoteType[]
		{
			new Farmer.EmoteType("happy", "Emote_Happy", 32, null, 2, false),
			new Farmer.EmoteType("sad", "Emote_Sad", 28, null, 2, false),
			new Farmer.EmoteType("heart", "Emote_Heart", 20, null, 2, false),
			new Farmer.EmoteType("exclamation", "Emote_Exclamation", 16, null, 2, false),
			new Farmer.EmoteType("note", "Emote_Note", 56, null, 2, false),
			new Farmer.EmoteType("sleep", "Emote_Sleep", 24, null, 2, false),
			new Farmer.EmoteType("game", "Emote_Game", 52, null, 2, false),
			new Farmer.EmoteType("question", "Emote_Question", 8, null, 2, false),
			new Farmer.EmoteType("x", "Emote_X", 36, null, 2, false),
			new Farmer.EmoteType("pause", "Emote_Pause", 40, null, 2, false),
			new Farmer.EmoteType("blush", "Emote_Blush", 60, null, 2, true),
			new Farmer.EmoteType("angry", "Emote_Angry", 12, null, 2, false),
			new Farmer.EmoteType("yes", "Emote_Yes", 56, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(0, 250, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("jingle1", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(16, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(0, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(16, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(0, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(16, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(0, 250, false, false, null, false)
			}, 2, false),
			new Farmer.EmoteType("no", "Emote_No", 36, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(25, 250, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("cancel", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(27, 250, true, false, null, false),
				new FarmerSprite.AnimationFrame(25, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(27, 250, true, false, null, false),
				new FarmerSprite.AnimationFrame(25, 250, false, false, null, false)
			}, 2, false),
			new Farmer.EmoteType("sick", "Emote_Sick", 12, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(104, 350, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("croak", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(105, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(104, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(105, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(104, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(105, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(104, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(105, 350, false, false, null, false)
			}, 2, false),
			new Farmer.EmoteType("laugh", "Emote_Laugh", 56, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(102, 150, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("dustMeep", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(102, 150, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("dustMeep", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(102, 150, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("dustMeep", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(102, 150, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("dustMeep", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, false, false, null, false)
			}, 2, false),
			new Farmer.EmoteType("surprised", "Emote_Surprised", 16, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(94, 1500, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("batScreech", null, SoundContext.Default);
					}
					who.jumpWithoutSound(4f);
					who.jitterStrength = 1f;
				})
			}, 2, false),
			new Farmer.EmoteType("hi", "Emote_Hi", 56, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(3, 250, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("give_gift", null, SoundContext.Default);
					}
				}),
				new FarmerSprite.AnimationFrame(85, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(3, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(85, 250, false, false, null, false)
			}, 2, false),
			new Farmer.EmoteType("taunt", "Emote_Taunt", 12, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(3, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(102, 50, false, false, null, false),
				new FarmerSprite.AnimationFrame(10, 250, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("hitEnemy", null, SoundContext.Default);
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(102, 50, false, false, null, false),
				new FarmerSprite.AnimationFrame(10, 250, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("hitEnemy", null, SoundContext.Default);
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(102, 50, false, false, null, false),
				new FarmerSprite.AnimationFrame(10, 250, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("hitEnemy", null, SoundContext.Default);
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 500, false, false, null, false)
			}, 2, true),
			new Farmer.EmoteType("uh", "Emote_Uh", 40, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(10, 1500, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("clam_tone", null, SoundContext.Default);
					}
				})
			}, 2, false),
			new Farmer.EmoteType("music", "Emote_Music", 56, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(98, 150, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					who.playHarpEmoteSound();
				}),
				new FarmerSprite.AnimationFrame(99, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(100, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(98, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(99, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(100, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(98, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(99, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(100, 150, false, false, null, false)
			}, 2, true),
			new Farmer.EmoteType("jar", "Emote_Jar", -1, new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(111, 150, false, false, null, false),
				new FarmerSprite.AnimationFrame(111, 300, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("fishingRodBend", null, SoundContext.Default);
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(111, 500, false, false, null, false),
				new FarmerSprite.AnimationFrame(111, 300, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("fishingRodBend", null, SoundContext.Default);
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(111, 500, false, false, null, false),
				new FarmerSprite.AnimationFrame(112, 1000, false, false, null, false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.playNearbySoundLocal("coin", null, SoundContext.Default);
					}
					who.jumpWithoutSound(4f);
				})
			}, 1, true)
		};

		// Token: 0x04000599 RID: 1433
		[XmlIgnore]
		public int emoteFacingDirection = 2;

		// Token: 0x0400059A RID: 1434
		private int toolPitchAccumulator;

		// Token: 0x0400059B RID: 1435
		[XmlIgnore]
		public readonly NetInt toolHoldStartTime = new NetInt();

		// Token: 0x0400059C RID: 1436
		private int charactercollisionTimer;

		// Token: 0x0400059D RID: 1437
		private NPC collisionNPC;

		// Token: 0x0400059E RID: 1438
		public float movementMultiplier = 0.01f;

		// Token: 0x02000420 RID: 1056
		public class EmoteType
		{
			// Token: 0x06003CB1 RID: 15537 RVA: 0x002ED240 File Offset: 0x002EB440
			public EmoteType(string emote_string = "", string display_name_key = "", int icon_index = -1, FarmerSprite.AnimationFrame[] frames = null, int facing_direction = 2, bool is_hidden = false)
			{
				this.emoteString = emote_string;
				this.emoteIconIndex = icon_index;
				this.animationFrames = frames;
				this.facingDirection = facing_direction;
				this.hidden = is_hidden;
				this.displayNameKey = "Strings\\UI:" + display_name_key;
			}

			// Token: 0x170004BA RID: 1210
			// (get) Token: 0x06003CB2 RID: 15538 RVA: 0x002ED2A3 File Offset: 0x002EB4A3
			public string displayName
			{
				get
				{
					return Game1.content.LoadString(this.displayNameKey);
				}
			}

			// Token: 0x0400273C RID: 10044
			public string emoteString = "";

			// Token: 0x0400273D RID: 10045
			public int emoteIconIndex = -1;

			// Token: 0x0400273E RID: 10046
			public FarmerSprite.AnimationFrame[] animationFrames;

			// Token: 0x0400273F RID: 10047
			public bool hidden;

			// Token: 0x04002740 RID: 10048
			public int facingDirection = 2;

			// Token: 0x04002741 RID: 10049
			public string displayNameKey;
		}
	}
}

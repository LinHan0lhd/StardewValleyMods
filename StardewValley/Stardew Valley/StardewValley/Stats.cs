using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley
{
	// Token: 0x02000107 RID: 263
	public class Stats
	{
		// Token: 0x17000248 RID: 584
		// (get) Token: 0x060014AC RID: 5292 RVA: 0x000F8036 File Offset: 0x000F6236
		public static bool AllowRetroactiveAchievements
		{
			get
			{
				return Program.sdk.RetroactiveAchievementsAllowed();
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x060014AD RID: 5293 RVA: 0x000F8042 File Offset: 0x000F6242
		// (set) Token: 0x060014AE RID: 5294 RVA: 0x000F8050 File Offset: 0x000F6250
		[XmlIgnore]
		public uint AverageBedtime
		{
			get
			{
				return this.Get("averageBedtime");
			}
			set
			{
				uint prevAverage = this.Get("averageBedtime");
				uint daysPlayed = this.Get("daysPlayed");
				this.Set("averageBedtime", (prevAverage * (daysPlayed - 1U) + value) / Math.Max(1U, daysPlayed));
			}
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x060014AF RID: 5295 RVA: 0x000F808F File Offset: 0x000F628F
		// (set) Token: 0x060014B0 RID: 5296 RVA: 0x000F809C File Offset: 0x000F629C
		[XmlIgnore]
		public uint DaysPlayed
		{
			get
			{
				return this.Get("daysPlayed");
			}
			set
			{
				this.Set("daysPlayed", value);
			}
		}

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x060014B1 RID: 5297 RVA: 0x000F80AA File Offset: 0x000F62AA
		// (set) Token: 0x060014B2 RID: 5298 RVA: 0x000F80B8 File Offset: 0x000F62B8
		[XmlIgnore]
		public uint IndividualMoneyEarned
		{
			get
			{
				return this.Get("individualMoneyEarned");
			}
			set
			{
				uint previousEarned = this.Get("individualMoneyEarned");
				this.Set("individualMoneyEarned", value);
				if (previousEarned < 1000000U && value >= 1000000U)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned1mil_" + (Game1.player.IsMale ? "Male" : "Female"), new string[]
					{
						Game1.player.Name
					});
					return;
				}
				if (previousEarned < 100000U && value >= 100000U)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned100k_" + (Game1.player.IsMale ? "Male" : "Female"), new string[]
					{
						Game1.player.Name
					});
					return;
				}
				if (previousEarned < 10000U && value >= 10000U)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned10k_" + (Game1.player.IsMale ? "Male" : "Female"), new string[]
					{
						Game1.player.Name
					});
					return;
				}
				if (previousEarned < 1000U && value >= 1000U)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned1k_" + (Game1.player.IsMale ? "Male" : "Female"), new string[]
					{
						Game1.player.Name
					});
				}
			}
		}

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x060014B3 RID: 5299 RVA: 0x000F821C File Offset: 0x000F641C
		// (set) Token: 0x060014B4 RID: 5300 RVA: 0x000F8229 File Offset: 0x000F6429
		[XmlIgnore]
		public uint ItemsCooked
		{
			get
			{
				return this.Get("itemsCooked");
			}
			set
			{
				this.Set("itemsCooked", value);
			}
		}

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x060014B5 RID: 5301 RVA: 0x000F8237 File Offset: 0x000F6437
		// (set) Token: 0x060014B6 RID: 5302 RVA: 0x000F8244 File Offset: 0x000F6444
		[XmlIgnore]
		public uint ItemsCrafted
		{
			get
			{
				return this.Get("itemsCrafted");
			}
			set
			{
				this.Set("itemsCrafted", value);
				this.checkForCraftingAchievements();
			}
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x060014B7 RID: 5303 RVA: 0x000F8258 File Offset: 0x000F6458
		// (set) Token: 0x060014B8 RID: 5304 RVA: 0x000F8265 File Offset: 0x000F6465
		[XmlIgnore]
		public uint ItemsForaged
		{
			get
			{
				return this.Get("itemsForaged");
			}
			set
			{
				this.Set("itemsForaged", value);
			}
		}

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x060014B9 RID: 5305 RVA: 0x000F8273 File Offset: 0x000F6473
		// (set) Token: 0x060014BA RID: 5306 RVA: 0x000F8280 File Offset: 0x000F6480
		[XmlIgnore]
		public uint ItemsShipped
		{
			get
			{
				return this.Get("itemsShipped");
			}
			set
			{
				this.Set("itemsShipped", value);
			}
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x060014BB RID: 5307 RVA: 0x000F828E File Offset: 0x000F648E
		// (set) Token: 0x060014BC RID: 5308 RVA: 0x000F829B File Offset: 0x000F649B
		[XmlIgnore]
		public uint NotesFound
		{
			get
			{
				return this.Get("notesFound");
			}
			set
			{
				this.Set("notesFound", value);
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x060014BD RID: 5309 RVA: 0x000F82A9 File Offset: 0x000F64A9
		// (set) Token: 0x060014BE RID: 5310 RVA: 0x000F82B6 File Offset: 0x000F64B6
		[XmlIgnore]
		public uint StepsTaken
		{
			get
			{
				return this.Get("stepsTaken");
			}
			set
			{
				this.Set("stepsTaken", value);
			}
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x060014BF RID: 5311 RVA: 0x000F82C4 File Offset: 0x000F64C4
		// (set) Token: 0x060014C0 RID: 5312 RVA: 0x000F82D1 File Offset: 0x000F64D1
		[XmlIgnore]
		public uint StumpsChopped
		{
			get
			{
				return this.Get("stumpsChopped");
			}
			set
			{
				this.Set("stumpsChopped", value);
			}
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x060014C1 RID: 5313 RVA: 0x000F82DF File Offset: 0x000F64DF
		// (set) Token: 0x060014C2 RID: 5314 RVA: 0x000F82EC File Offset: 0x000F64EC
		[XmlIgnore]
		public uint TimesUnconscious
		{
			get
			{
				return this.Get("timesUnconscious");
			}
			set
			{
				this.Set("timesUnconscious", value);
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x060014C3 RID: 5315 RVA: 0x000F82FA File Offset: 0x000F64FA
		// (set) Token: 0x060014C4 RID: 5316 RVA: 0x000F8307 File Offset: 0x000F6507
		[XmlIgnore]
		public uint BeveragesMade
		{
			get
			{
				return this.Get("beveragesMade");
			}
			set
			{
				this.Set("beveragesMade", value);
			}
		}

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x060014C5 RID: 5317 RVA: 0x000F8315 File Offset: 0x000F6515
		// (set) Token: 0x060014C6 RID: 5318 RVA: 0x000F8322 File Offset: 0x000F6522
		[XmlIgnore]
		public uint CheeseMade
		{
			get
			{
				return this.Get("cheeseMade");
			}
			set
			{
				this.Set("cheeseMade", value);
			}
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x060014C7 RID: 5319 RVA: 0x000F8330 File Offset: 0x000F6530
		// (set) Token: 0x060014C8 RID: 5320 RVA: 0x000F833D File Offset: 0x000F653D
		[XmlIgnore]
		public uint ChickenEggsLayed
		{
			get
			{
				return this.Get("chickenEggsLayed");
			}
			set
			{
				this.Set("chickenEggsLayed", value);
			}
		}

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x060014C9 RID: 5321 RVA: 0x000F834B File Offset: 0x000F654B
		// (set) Token: 0x060014CA RID: 5322 RVA: 0x000F8358 File Offset: 0x000F6558
		[XmlIgnore]
		public uint CowMilkProduced
		{
			get
			{
				return this.Get("cowMilkProduced");
			}
			set
			{
				this.Set("cowMilkProduced", value);
			}
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x060014CB RID: 5323 RVA: 0x000F8366 File Offset: 0x000F6566
		// (set) Token: 0x060014CC RID: 5324 RVA: 0x000F8373 File Offset: 0x000F6573
		[XmlIgnore]
		public uint CropsShipped
		{
			get
			{
				return this.Get("cropsShipped");
			}
			set
			{
				this.Set("cropsShipped", value);
			}
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x060014CD RID: 5325 RVA: 0x000F8381 File Offset: 0x000F6581
		// (set) Token: 0x060014CE RID: 5326 RVA: 0x000F838E File Offset: 0x000F658E
		[XmlIgnore]
		public uint DirtHoed
		{
			get
			{
				return this.Get("dirtHoed");
			}
			set
			{
				this.Set("dirtHoed", value);
			}
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x060014CF RID: 5327 RVA: 0x000F839C File Offset: 0x000F659C
		// (set) Token: 0x060014D0 RID: 5328 RVA: 0x000F83A9 File Offset: 0x000F65A9
		[XmlIgnore]
		public uint DuckEggsLayed
		{
			get
			{
				return this.Get("duckEggsLayed");
			}
			set
			{
				this.Set("duckEggsLayed", value);
			}
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x060014D1 RID: 5329 RVA: 0x000F83B7 File Offset: 0x000F65B7
		// (set) Token: 0x060014D2 RID: 5330 RVA: 0x000F83C4 File Offset: 0x000F65C4
		[XmlIgnore]
		public uint GoatCheeseMade
		{
			get
			{
				return this.Get("goatCheeseMade");
			}
			set
			{
				this.Set("goatCheeseMade", value);
			}
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x060014D3 RID: 5331 RVA: 0x000F83D2 File Offset: 0x000F65D2
		// (set) Token: 0x060014D4 RID: 5332 RVA: 0x000F83DF File Offset: 0x000F65DF
		[XmlIgnore]
		public uint GoatMilkProduced
		{
			get
			{
				return this.Get("goatMilkProduced");
			}
			set
			{
				this.Set("goatMilkProduced", value);
			}
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x060014D5 RID: 5333 RVA: 0x000F83ED File Offset: 0x000F65ED
		// (set) Token: 0x060014D6 RID: 5334 RVA: 0x000F83FA File Offset: 0x000F65FA
		[XmlIgnore]
		public uint PiecesOfTrashRecycled
		{
			get
			{
				return this.Get("piecesOfTrashRecycled");
			}
			set
			{
				this.Set("piecesOfTrashRecycled", value);
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x060014D7 RID: 5335 RVA: 0x000F8408 File Offset: 0x000F6608
		// (set) Token: 0x060014D8 RID: 5336 RVA: 0x000F8415 File Offset: 0x000F6615
		[XmlIgnore]
		public uint PreservesMade
		{
			get
			{
				return this.Get("preservesMade");
			}
			set
			{
				this.Set("preservesMade", value);
			}
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x060014D9 RID: 5337 RVA: 0x000F8423 File Offset: 0x000F6623
		// (set) Token: 0x060014DA RID: 5338 RVA: 0x000F8430 File Offset: 0x000F6630
		[XmlIgnore]
		public uint RabbitWoolProduced
		{
			get
			{
				return this.Get("rabbitWoolProduced");
			}
			set
			{
				this.Set("rabbitWoolProduced", value);
			}
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x060014DB RID: 5339 RVA: 0x000F843E File Offset: 0x000F663E
		// (set) Token: 0x060014DC RID: 5340 RVA: 0x000F844B File Offset: 0x000F664B
		[XmlIgnore]
		public uint SeedsSown
		{
			get
			{
				return this.Get("seedsSown");
			}
			set
			{
				this.Set("seedsSown", value);
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x060014DD RID: 5341 RVA: 0x000F8459 File Offset: 0x000F6659
		// (set) Token: 0x060014DE RID: 5342 RVA: 0x000F8466 File Offset: 0x000F6666
		[XmlIgnore]
		public uint SheepWoolProduced
		{
			get
			{
				return this.Get("sheepWoolProduced");
			}
			set
			{
				this.Set("sheepWoolProduced", value);
			}
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x060014DF RID: 5343 RVA: 0x000F8474 File Offset: 0x000F6674
		// (set) Token: 0x060014E0 RID: 5344 RVA: 0x000F8481 File Offset: 0x000F6681
		[XmlIgnore]
		public uint TrufflesFound
		{
			get
			{
				return this.Get("trufflesFound");
			}
			set
			{
				this.Set("trufflesFound", value);
			}
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x060014E1 RID: 5345 RVA: 0x000F848F File Offset: 0x000F668F
		// (set) Token: 0x060014E2 RID: 5346 RVA: 0x000F849C File Offset: 0x000F669C
		[XmlIgnore]
		public uint WeedsEliminated
		{
			get
			{
				return this.Get("weedsEliminated");
			}
			set
			{
				this.Set("weedsEliminated", value);
			}
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x060014E3 RID: 5347 RVA: 0x000F84AA File Offset: 0x000F66AA
		// (set) Token: 0x060014E4 RID: 5348 RVA: 0x000F84B7 File Offset: 0x000F66B7
		[XmlIgnore]
		public uint MonstersKilled
		{
			get
			{
				return this.Get("monstersKilled");
			}
			set
			{
				this.Set("monstersKilled", value);
			}
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x060014E5 RID: 5349 RVA: 0x000F84C5 File Offset: 0x000F66C5
		// (set) Token: 0x060014E6 RID: 5350 RVA: 0x000F84D2 File Offset: 0x000F66D2
		[XmlIgnore]
		public uint SlimesKilled
		{
			get
			{
				return this.Get("slimesKilled");
			}
			set
			{
				this.Set("slimesKilled", value);
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x060014E7 RID: 5351 RVA: 0x000F84E0 File Offset: 0x000F66E0
		// (set) Token: 0x060014E8 RID: 5352 RVA: 0x000F84ED File Offset: 0x000F66ED
		[XmlIgnore]
		public uint FishCaught
		{
			get
			{
				return this.Get("fishCaught");
			}
			set
			{
				this.Set("fishCaught", value);
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x060014E9 RID: 5353 RVA: 0x000F84FB File Offset: 0x000F66FB
		// (set) Token: 0x060014EA RID: 5354 RVA: 0x000F8508 File Offset: 0x000F6708
		[XmlIgnore]
		public uint TimesFished
		{
			get
			{
				return this.Get("timesFished");
			}
			set
			{
				this.Set("timesFished", value);
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x060014EB RID: 5355 RVA: 0x000F8516 File Offset: 0x000F6716
		// (set) Token: 0x060014EC RID: 5356 RVA: 0x000F8523 File Offset: 0x000F6723
		[XmlIgnore]
		public uint CaveCarrotsFound
		{
			get
			{
				return this.Get("caveCarrotsFound");
			}
			set
			{
				this.Set("caveCarrotsFound", value);
			}
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x060014ED RID: 5357 RVA: 0x000F8531 File Offset: 0x000F6731
		// (set) Token: 0x060014EE RID: 5358 RVA: 0x000F853E File Offset: 0x000F673E
		[XmlIgnore]
		public uint CopperFound
		{
			get
			{
				return this.Get("copperFound");
			}
			set
			{
				this.Set("copperFound", value);
			}
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x060014EF RID: 5359 RVA: 0x000F854C File Offset: 0x000F674C
		// (set) Token: 0x060014F0 RID: 5360 RVA: 0x000F8559 File Offset: 0x000F6759
		[XmlIgnore]
		public uint DiamondsFound
		{
			get
			{
				return this.Get("diamondsFound");
			}
			set
			{
				this.Set("diamondsFound", value);
			}
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x060014F1 RID: 5361 RVA: 0x000F8567 File Offset: 0x000F6767
		// (set) Token: 0x060014F2 RID: 5362 RVA: 0x000F8574 File Offset: 0x000F6774
		[XmlIgnore]
		public uint GeodesCracked
		{
			get
			{
				return this.Get("geodesCracked");
			}
			set
			{
				this.Set("geodesCracked", value);
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x060014F3 RID: 5363 RVA: 0x000F8582 File Offset: 0x000F6782
		// (set) Token: 0x060014F4 RID: 5364 RVA: 0x000F858F File Offset: 0x000F678F
		[XmlIgnore]
		public uint GoldFound
		{
			get
			{
				return this.Get("goldFound");
			}
			set
			{
				this.Set("goldFound", value);
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x060014F5 RID: 5365 RVA: 0x000F859D File Offset: 0x000F679D
		// (set) Token: 0x060014F6 RID: 5366 RVA: 0x000F85AA File Offset: 0x000F67AA
		[XmlIgnore]
		public uint IridiumFound
		{
			get
			{
				return this.Get("iridiumFound");
			}
			set
			{
				this.Set("iridiumFound", value);
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x060014F7 RID: 5367 RVA: 0x000F85B8 File Offset: 0x000F67B8
		// (set) Token: 0x060014F8 RID: 5368 RVA: 0x000F85C5 File Offset: 0x000F67C5
		[XmlIgnore]
		public uint IronFound
		{
			get
			{
				return this.Get("ironFound");
			}
			set
			{
				this.Set("ironFound", value);
			}
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x060014F9 RID: 5369 RVA: 0x000F85D3 File Offset: 0x000F67D3
		// (set) Token: 0x060014FA RID: 5370 RVA: 0x000F85E0 File Offset: 0x000F67E0
		[XmlIgnore]
		public uint MysticStonesCrushed
		{
			get
			{
				return this.Get("mysticStonesCrushed");
			}
			set
			{
				this.Set("mysticStonesCrushed", value);
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x060014FB RID: 5371 RVA: 0x000F85EE File Offset: 0x000F67EE
		// (set) Token: 0x060014FC RID: 5372 RVA: 0x000F85FB File Offset: 0x000F67FB
		[XmlIgnore]
		public uint OtherPreciousGemsFound
		{
			get
			{
				return this.Get("otherPreciousGemsFound");
			}
			set
			{
				this.Set("otherPreciousGemsFound", value);
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x060014FD RID: 5373 RVA: 0x000F8609 File Offset: 0x000F6809
		// (set) Token: 0x060014FE RID: 5374 RVA: 0x000F8616 File Offset: 0x000F6816
		[XmlIgnore]
		public uint PrismaticShardsFound
		{
			get
			{
				return this.Get("prismaticShardsFound");
			}
			set
			{
				this.Set("prismaticShardsFound", value);
			}
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x060014FF RID: 5375 RVA: 0x000F8624 File Offset: 0x000F6824
		// (set) Token: 0x06001500 RID: 5376 RVA: 0x000F8631 File Offset: 0x000F6831
		[XmlIgnore]
		public uint RocksCrushed
		{
			get
			{
				return this.Get("rocksCrushed");
			}
			set
			{
				this.Set("rocksCrushed", value);
			}
		}

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06001501 RID: 5377 RVA: 0x000F863F File Offset: 0x000F683F
		// (set) Token: 0x06001502 RID: 5378 RVA: 0x000F864C File Offset: 0x000F684C
		[XmlIgnore]
		public uint StoneGathered
		{
			get
			{
				return this.Get("stoneGathered");
			}
			set
			{
				this.Set("stoneGathered", value);
			}
		}

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06001503 RID: 5379 RVA: 0x000F865A File Offset: 0x000F685A
		// (set) Token: 0x06001504 RID: 5380 RVA: 0x000F8667 File Offset: 0x000F6867
		[XmlIgnore]
		public uint GiftsGiven
		{
			get
			{
				return this.Get("giftsGiven");
			}
			set
			{
				this.Set("giftsGiven", value);
			}
		}

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06001505 RID: 5381 RVA: 0x000F8675 File Offset: 0x000F6875
		// (set) Token: 0x06001506 RID: 5382 RVA: 0x000F8682 File Offset: 0x000F6882
		[XmlIgnore]
		public uint GoodFriends
		{
			get
			{
				return this.Get("goodFriends");
			}
			set
			{
				this.Set("goodFriends", value);
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06001507 RID: 5383 RVA: 0x000F8690 File Offset: 0x000F6890
		// (set) Token: 0x06001508 RID: 5384 RVA: 0x000F869D File Offset: 0x000F689D
		[XmlIgnore]
		public uint QuestsCompleted
		{
			get
			{
				return this.Get("questsCompleted");
			}
			set
			{
				this.Set("questsCompleted", value);
				this.checkForQuestAchievements();
			}
		}

		// Token: 0x06001509 RID: 5385 RVA: 0x000F86B4 File Offset: 0x000F68B4
		public uint Get(string key)
		{
			uint value;
			if (!this.Values.TryGetValue(key, out value))
			{
				return 0U;
			}
			return value;
		}

		// Token: 0x0600150A RID: 5386 RVA: 0x000F86D4 File Offset: 0x000F68D4
		public void Set(string key, uint value)
		{
			if (value != 0U)
			{
				this.Values[key] = value;
				return;
			}
			this.Values.Remove(key);
		}

		// Token: 0x0600150B RID: 5387 RVA: 0x000F86F4 File Offset: 0x000F68F4
		public void Set(string key, int value)
		{
			if (value <= 0)
			{
				this.Set(key, 0U);
				return;
			}
			this.Set(key, (uint)value);
		}

		// Token: 0x0600150C RID: 5388 RVA: 0x000F870C File Offset: 0x000F690C
		public uint Decrement(string key, uint amount = 1U)
		{
			uint newValue = this.Get(key);
			newValue = ((amount >= newValue) ? 0U : (newValue - amount));
			this.Set(key, newValue);
			return newValue;
		}

		// Token: 0x0600150D RID: 5389 RVA: 0x000F8738 File Offset: 0x000F6938
		public uint Increment(string key, uint amount = 1U)
		{
			uint newValue = this.Get(key) + amount;
			this.Set(key, newValue);
			return newValue;
		}

		// Token: 0x0600150E RID: 5390 RVA: 0x000F8758 File Offset: 0x000F6958
		public uint Increment(string key, int amount)
		{
			if (amount >= 0)
			{
				return this.Increment(key, (uint)amount);
			}
			return this.Decrement(key, (uint)(-(uint)amount));
		}

		// Token: 0x0600150F RID: 5391 RVA: 0x000F8770 File Offset: 0x000F6970
		public void monsterKilled(string name)
		{
			if (AdventureGuild.willThisKillCompleteAMonsterSlayerQuest(name))
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Stats.cs.5129"));
				Game1.multiplayer.globalChatInfoMessage("MonsterSlayer" + Game1.random.Next(4).ToString(), new string[]
				{
					Game1.player.Name,
					TokenStringBuilder.MonsterName(name, null)
				});
			}
			this.specificMonstersKilled[name] = this.getMonstersKilled(name) + 1;
			this.checkForMonsterSlayerAchievement(true);
		}

		// Token: 0x06001510 RID: 5392 RVA: 0x000F87F9 File Offset: 0x000F69F9
		public int getMonstersKilled(string name)
		{
			return this.specificMonstersKilled.GetValueOrDefault(name);
		}

		// Token: 0x06001511 RID: 5393 RVA: 0x000F8808 File Offset: 0x000F6A08
		public void onMoneyGifted(uint amount)
		{
			uint previousMoney = this.Get("totalMoneyGifted");
			uint totalMoneyGifted = this.Increment("totalMoneyGifted", amount);
			if (previousMoney <= 1000000U && totalMoneyGifted > 1000000U)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted1mil", new string[]
				{
					Game1.player.Name
				});
				return;
			}
			if (previousMoney <= 100000U && totalMoneyGifted > 100000U)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted100k", new string[]
				{
					Game1.player.Name
				});
				return;
			}
			if (previousMoney <= 10000U && totalMoneyGifted > 10000U)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted10k", new string[]
				{
					Game1.player.Name
				});
				return;
			}
			if (previousMoney <= 1000U && totalMoneyGifted > 1000U)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted1k", new string[]
				{
					Game1.player.Name
				});
			}
		}

		// Token: 0x06001512 RID: 5394 RVA: 0x000F88FC File Offset: 0x000F6AFC
		public void takeStep()
		{
			uint stepsTaken = this.Increment("stepsTaken", 1U);
			if (stepsTaken <= 100000U)
			{
				if (stepsTaken == 10000U)
				{
					Game1.multiplayer.globalChatInfoMessage("Walked10k", new string[]
					{
						Game1.player.Name
					});
					return;
				}
				if (stepsTaken != 100000U)
				{
					return;
				}
				Game1.multiplayer.globalChatInfoMessage("Walked100k", new string[]
				{
					Game1.player.Name
				});
				return;
			}
			else
			{
				if (stepsTaken == 1000000U)
				{
					Game1.multiplayer.globalChatInfoMessage("Walked1m", new string[]
					{
						Game1.player.Name
					});
					return;
				}
				if (stepsTaken != 10000000U)
				{
					return;
				}
				Game1.multiplayer.globalChatInfoMessage("Walked10m", new string[]
				{
					Game1.player.Name
				});
				return;
			}
		}

		// Token: 0x06001513 RID: 5395 RVA: 0x000F89CC File Offset: 0x000F6BCC
		public void checkForBooksReadAchievement()
		{
			if (Game1.player.stats.Get("Book_Trash") > 0U && Game1.player.stats.Get("Book_Crabbing") > 0U && Game1.player.stats.Get("Book_Bombs") > 0U && Game1.player.stats.Get("Book_Roe") > 0U && Game1.player.stats.Get("Book_WildSeeds") > 0U && Game1.player.stats.Get("Book_Woodcutting") > 0U && Game1.player.stats.Get("Book_Defense") > 0U && Game1.player.stats.Get("Book_Friendship") > 0U && Game1.player.stats.Get("Book_Void") > 0U && Game1.player.stats.Get("Book_Speed") > 0U && Game1.player.stats.Get("Book_Marlon") > 0U && Game1.player.stats.Get("Book_PriceCatalogue") > 0U && Game1.player.stats.Get("Book_Diamonds") > 0U && Game1.player.stats.Get("Book_Mystery") > 0U && Game1.player.stats.Get("Book_AnimalCatalogue") > 0U && Game1.player.stats.Get("Book_Speed2") > 0U && Game1.player.stats.Get("Book_Artifact") > 0U && Game1.player.stats.Get("Book_Horse") > 0U && Game1.player.stats.Get("Book_Grass") > 0U)
			{
				Game1.getAchievement(35, true);
			}
		}

		// Token: 0x06001514 RID: 5396 RVA: 0x000F8BC0 File Offset: 0x000F6DC0
		public void checkForCookingAchievements()
		{
			Dictionary<string, string> recipes = CraftingRecipe.cookingRecipes;
			int numberOfRecipesCooked = 0;
			int numberOfMealsMade = 0;
			foreach (KeyValuePair<string, string> v in recipes)
			{
				if (Game1.player.cookingRecipes.ContainsKey(v.Key))
				{
					string recipe = ArgUtility.SplitBySpaceAndGet(v.Value.Split('/', StringSplitOptions.None)[2], 0, null);
					int timesCooked;
					if (Game1.player.recipesCooked.TryGetValue(recipe, out timesCooked))
					{
						numberOfMealsMade += timesCooked;
						numberOfRecipesCooked++;
					}
				}
			}
			this.Set("itemsCooked", numberOfMealsMade);
			if (numberOfRecipesCooked >= recipes.Count)
			{
				Game1.getAchievement(17, true);
			}
			if (numberOfRecipesCooked >= 25)
			{
				Game1.getAchievement(16, true);
			}
			if (numberOfRecipesCooked >= 10)
			{
				Game1.getAchievement(15, true);
			}
		}

		// Token: 0x06001515 RID: 5397 RVA: 0x000F8C9C File Offset: 0x000F6E9C
		public void checkForCraftingAchievements()
		{
			Dictionary<string, string> recipes = CraftingRecipe.craftingRecipes;
			int numberOfRecipesMade = 0;
			int numberOfItemsCrafted = 0;
			foreach (string s in recipes.Keys)
			{
				int timesCrafted;
				if (!(s == "Wedding Ring") && Game1.player.craftingRecipes.TryGetValue(s, out timesCrafted))
				{
					numberOfItemsCrafted += timesCrafted;
					if (Game1.player.craftingRecipes[s] > 0)
					{
						numberOfRecipesMade++;
					}
				}
			}
			this.Set("itemsCrafted", numberOfItemsCrafted);
			if (numberOfRecipesMade >= recipes.Count - 1)
			{
				Game1.getAchievement(22, true);
			}
			if (numberOfRecipesMade >= 30)
			{
				Game1.getAchievement(21, true);
			}
			if (numberOfRecipesMade >= 15)
			{
				Game1.getAchievement(20, true);
			}
		}

		// Token: 0x06001516 RID: 5398 RVA: 0x000F8D6C File Offset: 0x000F6F6C
		public void checkForShippingAchievements()
		{
			bool allPolycultureCropsShipped = true;
			bool anyMonocultureCropShipped = false;
			foreach (CropData data in Game1.cropData.Values)
			{
				if (data.CountForPolyculture)
				{
					allPolycultureCropsShipped = (allPolycultureCropsShipped && Stats.<checkForShippingAchievements>g__DidFarmerShip|203_0(data.HarvestItemId, 15));
				}
				if (data.CountForMonoculture)
				{
					anyMonocultureCropShipped = (anyMonocultureCropShipped || Stats.<checkForShippingAchievements>g__DidFarmerShip|203_0(data.HarvestItemId, 300));
				}
			}
			if (allPolycultureCropsShipped)
			{
				Game1.getAchievement(31, true);
			}
			if (anyMonocultureCropShipped)
			{
				Game1.getAchievement(32, true);
			}
			if (Utility.hasFarmerShippedAllItems())
			{
				Game1.getAchievement(34, true);
			}
		}

		// Token: 0x06001517 RID: 5399 RVA: 0x000F8E1C File Offset: 0x000F701C
		public void checkForFishingAchievements()
		{
			int numberOfFishCaught = 0;
			int differentKindsOfFishCaught = 0;
			int totalKindsOfFish = 0;
			foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (itemData.ObjectType == "Fish")
				{
					ObjectData objData = itemData.RawData as ObjectData;
					if (objData == null || !objData.ExcludeFromFishingCollection)
					{
						totalKindsOfFish++;
						int[] data;
						if (Game1.player.fishCaught.TryGetValue(itemData.QualifiedItemId, out data))
						{
							numberOfFishCaught += data[0];
							differentKindsOfFishCaught++;
						}
					}
				}
			}
			this.Set("fishCaught", numberOfFishCaught);
			if (numberOfFishCaught >= 100)
			{
				Game1.getAchievement(27, true);
			}
			if (differentKindsOfFishCaught >= totalKindsOfFish)
			{
				Game1.getAchievement(26, true);
				if (!Game1.player.hasOrWillReceiveMail("CF_Fish"))
				{
					Game1.addMailForTomorrow("CF_Fish", false, false);
				}
			}
			if (differentKindsOfFishCaught >= 24)
			{
				Game1.getAchievement(25, true);
			}
			if (differentKindsOfFishCaught >= 10)
			{
				Game1.getAchievement(24, true);
			}
		}

		// Token: 0x06001518 RID: 5400 RVA: 0x000F8F20 File Offset: 0x000F7120
		public void checkForArchaeologyAchievements()
		{
			int length = Game1.netWorldState.Value.MuseumPieces.Length;
			if (length >= LibraryMuseum.totalArtifacts)
			{
				Game1.getAchievement(5, true);
			}
			if (length >= 40)
			{
				Game1.getAchievement(28, true);
			}
		}

		// Token: 0x06001519 RID: 5401 RVA: 0x000F8F54 File Offset: 0x000F7154
		public void checkForHeldItemAchievements()
		{
			if (Game1.player.Items.ContainsId("(W)62") || Game1.player.Items.ContainsId("(W)63") || Game1.player.Items.ContainsId("(W)64"))
			{
				Game1.getAchievement(42, true);
			}
		}

		// Token: 0x0600151A RID: 5402 RVA: 0x000F8FAC File Offset: 0x000F71AC
		public void checkForMoneyAchievements()
		{
			if (Game1.player.totalMoneyEarned >= 10000000U)
			{
				Game1.getAchievement(4, true);
			}
			if (Game1.player.totalMoneyEarned >= 1000000U)
			{
				Game1.getAchievement(3, true);
			}
			if (Game1.player.totalMoneyEarned >= 250000U)
			{
				Game1.getAchievement(2, true);
			}
			if (Game1.player.totalMoneyEarned >= 50000U)
			{
				Game1.getAchievement(1, true);
			}
			if (Game1.player.totalMoneyEarned >= 15000U)
			{
				Game1.getAchievement(0, true);
			}
		}

		// Token: 0x0600151B RID: 5403 RVA: 0x000F9031 File Offset: 0x000F7231
		public void checkForBuildingUpgradeAchievements()
		{
			if (Game1.player.HouseUpgradeLevel >= 2)
			{
				Game1.getAchievement(19, true);
			}
			if (Game1.player.HouseUpgradeLevel >= 1)
			{
				Game1.getAchievement(18, true);
			}
		}

		// Token: 0x0600151C RID: 5404 RVA: 0x000F905D File Offset: 0x000F725D
		public void checkForQuestAchievements()
		{
			if (this.QuestsCompleted >= 40U)
			{
				Game1.getAchievement(30, true);
				Game1.addMailForTomorrow("quest35", false, false);
			}
			if (this.QuestsCompleted >= 10U)
			{
				Game1.getAchievement(29, true);
				Game1.addMailForTomorrow("quest10", false, false);
			}
		}

		// Token: 0x0600151D RID: 5405 RVA: 0x000F909C File Offset: 0x000F729C
		public void checkForFriendshipAchievements()
		{
			uint numberOf5Level = 0U;
			uint numberOf8Level = 0U;
			uint numberOf10Level = 0U;
			foreach (Friendship friendship3 in Game1.player.friendshipData.Values)
			{
				if (friendship3.Points >= 2500)
				{
					numberOf10Level += 1U;
				}
				if (friendship3.Points >= 2000)
				{
					numberOf8Level += 1U;
				}
				if (friendship3.Points >= 1250)
				{
					numberOf5Level += 1U;
				}
			}
			this.GoodFriends = numberOf8Level;
			if (numberOf5Level >= 20U)
			{
				Game1.getAchievement(13, true);
			}
			if (numberOf5Level >= 10U)
			{
				Game1.getAchievement(12, true);
			}
			if (numberOf5Level >= 4U)
			{
				Game1.getAchievement(11, true);
			}
			if (numberOf5Level >= 1U)
			{
				Game1.getAchievement(6, true);
			}
			if (numberOf10Level >= 8U)
			{
				Game1.getAchievement(9, true);
			}
			if (numberOf10Level >= 1U)
			{
				Game1.getAchievement(7, true);
			}
			foreach (KeyValuePair<string, string> pair in CraftingRecipe.cookingRecipes)
			{
				string recipeKey = pair.Key;
				string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(pair.Value.Split('/', StringSplitOptions.None), 3, null, true));
				if (!(ArgUtility.Get(getConditions, 0, null, true) != "f"))
				{
					string npcName = ArgUtility.Get(getConditions, 1, null, true);
					int minHearts = ArgUtility.GetInt(getConditions, 2, 0);
					Friendship friendship;
					if (npcName != null && Game1.player.friendshipData.TryGetValue(npcName, out friendship) && friendship.Points >= minHearts * 250 && !Game1.player.cookingRecipes.ContainsKey(recipeKey) && !Game1.player.hasOrWillReceiveMail(npcName + "Cooking"))
					{
						Game1.addMailForTomorrow(npcName + "Cooking", false, false);
					}
				}
			}
			foreach (KeyValuePair<string, string> pair2 in CraftingRecipe.craftingRecipes)
			{
				string recipeKey2 = pair2.Key;
				string[] getConditions2 = ArgUtility.SplitBySpace(ArgUtility.Get(pair2.Value.Split('/', StringSplitOptions.None), 4, null, true));
				if (!(ArgUtility.Get(getConditions2, 0, null, true) != "f"))
				{
					string npcName2 = ArgUtility.Get(getConditions2, 1, null, true);
					int minHearts2 = ArgUtility.GetInt(getConditions2, 2, 0);
					Friendship friendship2;
					if (npcName2 != null && Game1.player.friendshipData.TryGetValue(npcName2, out friendship2) && friendship2.Points >= minHearts2 * 250 && !Game1.player.craftingRecipes.ContainsKey(recipeKey2) && !Game1.player.hasOrWillReceiveMail(npcName2 + "Crafting"))
					{
						Game1.addMailForTomorrow(npcName2 + "Crafting", false, false);
					}
				}
			}
		}

		// Token: 0x0600151E RID: 5406 RVA: 0x000F9378 File Offset: 0x000F7578
		public void checkForCommunityCenterOrJojaAchievements(bool isDirectUnlock)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock))
			{
				if (Game1.player.eventsSeen.Contains("191393"))
				{
					Game1.getSteamAchievement("Achievement_LocalLegend");
				}
				if (Game1.player.eventsSeen.Contains("502261"))
				{
					Game1.getSteamAchievement("Achievement_Joja");
				}
			}
		}

		// Token: 0x0600151F RID: 5407 RVA: 0x000F93D0 File Offset: 0x000F75D0
		public void checkForMiniGameAchievements(bool isDirectUnlock)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock))
			{
				if (Game1.player.stats.Get("completedPrairieKing") > 0U)
				{
					Game1.getSteamAchievement("Achievement_PrairieKing");
				}
				if (Game1.player.stats.Get("completedPrairieKingWithoutDying") > 0U)
				{
					Game1.getSteamAchievement("Achievement_FectorsChallenge");
				}
			}
		}

		// Token: 0x06001520 RID: 5408 RVA: 0x000F9428 File Offset: 0x000F7628
		public void checkForFullHouseAchievement(bool isDirectUnlock)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock) && Game1.player.isMarriedOrRoommates() && Game1.player.getChildrenCount() >= 2)
			{
				Game1.getSteamAchievement("Achievement_FullHouse");
			}
		}

		// Token: 0x06001521 RID: 5409 RVA: 0x000F9456 File Offset: 0x000F7656
		public void checkForMineAchievement(bool isDirectUnlock, bool assumeDeepestLevel = false)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock) && (assumeDeepestLevel || Game1.player.deepestMineLevel >= 120))
			{
				Game1.getSteamAchievement("Achievement_TheBottom");
			}
		}

		// Token: 0x06001522 RID: 5410 RVA: 0x000F947C File Offset: 0x000F767C
		public void checkForMonsterSlayerAchievement(bool isDirectUnlock)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock) && AdventureGuild.areAllMonsterSlayerQuestsComplete())
			{
				Game1.player.hasCompletedAllMonsterSlayerQuests.Value = true;
				Game1.getSteamAchievement("Achievement_KeeperOfTheMysticRings");
			}
		}

		// Token: 0x06001523 RID: 5411 RVA: 0x000F94A8 File Offset: 0x000F76A8
		public void checkForSkillAchievements(bool isDirectUnlock)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock))
			{
				NetInt[] array = new NetInt[]
				{
					Game1.player.farmingLevel,
					Game1.player.miningLevel,
					Game1.player.fishingLevel,
					Game1.player.foragingLevel,
					Game1.player.combatLevel
				};
				bool anyMaxed = false;
				bool allMaxed = true;
				NetInt[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i].Value >= 10)
					{
						anyMaxed = true;
					}
					else
					{
						allMaxed = false;
					}
				}
				if (anyMaxed)
				{
					Game1.getSteamAchievement("Achievement_SingularTalent");
					if (allMaxed)
					{
						Game1.getSteamAchievement("Achievement_MasterOfTheFiveWays");
					}
				}
			}
		}

		// Token: 0x06001524 RID: 5412 RVA: 0x000F9547 File Offset: 0x000F7747
		public void checkForStardropAchievement(bool isDirectUnlock)
		{
			if (this.CanUnlockPlatformAchievements(isDirectUnlock) && Utility.foundAllStardrops(null))
			{
				Game1.getSteamAchievement("Achievement_Stardrop");
			}
		}

		// Token: 0x06001525 RID: 5413 RVA: 0x000F9564 File Offset: 0x000F7764
		public bool isSharedAchievement(int which)
		{
			return which <= 5 || which == 28;
		}

		// Token: 0x06001526 RID: 5414 RVA: 0x000F9574 File Offset: 0x000F7774
		public void checkForAchievements()
		{
			this.checkForBooksReadAchievement();
			this.checkForCookingAchievements();
			this.checkForCraftingAchievements();
			this.checkForShippingAchievements();
			this.checkForFishingAchievements();
			this.checkForArchaeologyAchievements();
			this.checkForHeldItemAchievements();
			this.checkForMoneyAchievements();
			this.checkForBuildingUpgradeAchievements();
			this.checkForQuestAchievements();
			this.checkForFriendshipAchievements();
			this.checkForCommunityCenterOrJojaAchievements(false);
			this.checkForMiniGameAchievements(false);
			this.checkForFullHouseAchievement(false);
			this.checkForMineAchievement(false, false);
			this.checkForMonsterSlayerAchievement(false);
			this.checkForSkillAchievements(false);
			this.checkForStardropAchievement(false);
		}

		// Token: 0x06001527 RID: 5415 RVA: 0x000F95F5 File Offset: 0x000F77F5
		public bool CanUnlockPlatformAchievements(bool isDirectUnlock)
		{
			return Stats.AllowRetroactiveAchievements || isDirectUnlock;
		}

		// Token: 0x06001529 RID: 5417 RVA: 0x000F961C File Offset: 0x000F781C
		[CompilerGenerated]
		internal static bool <checkForShippingAchievements>g__DidFarmerShip|203_0(string itemId, int number)
		{
			return Game1.player.basicShipped.GetValueOrDefault(itemId, 0) >= number;
		}

		// Token: 0x04000D4A RID: 3402
		public StatsDictionary<int> specificMonstersKilled = new StatsDictionary<int>();

		// Token: 0x04000D4B RID: 3403
		public StatsDictionary<uint> Values = new StatsDictionary<uint>();

		// Token: 0x04000D4C RID: 3404
		[XmlElement("stat_dictionary")]
		public SerializableDictionary<string, uint> obsolete_stat_dictionary;

		// Token: 0x04000D4D RID: 3405
		[XmlElement("averageBedtime")]
		public uint? obsolete_averageBedtime;

		// Token: 0x04000D4E RID: 3406
		[XmlElement("beveragesMade")]
		public uint? obsolete_beveragesMade;

		// Token: 0x04000D4F RID: 3407
		[XmlElement("caveCarrotsFound")]
		public uint? obsolete_caveCarrotsFound;

		// Token: 0x04000D50 RID: 3408
		[XmlElement("cheeseMade")]
		public uint? obsolete_cheeseMade;

		// Token: 0x04000D51 RID: 3409
		[XmlElement("chickenEggsLayed")]
		public uint? obsolete_chickenEggsLayed;

		// Token: 0x04000D52 RID: 3410
		[XmlElement("copperFound")]
		public uint? obsolete_copperFound;

		// Token: 0x04000D53 RID: 3411
		[XmlElement("cowMilkProduced")]
		public uint? obsolete_cowMilkProduced;

		// Token: 0x04000D54 RID: 3412
		[XmlElement("cropsShipped")]
		public uint? obsolete_cropsShipped;

		// Token: 0x04000D55 RID: 3413
		[XmlElement("daysPlayed")]
		public uint? obsolete_daysPlayed;

		// Token: 0x04000D56 RID: 3414
		[XmlElement("diamondsFound")]
		public uint? obsolete_diamondsFound;

		// Token: 0x04000D57 RID: 3415
		[XmlElement("dirtHoed")]
		public uint? obsolete_dirtHoed;

		// Token: 0x04000D58 RID: 3416
		[XmlElement("duckEggsLayed")]
		public uint? obsolete_duckEggsLayed;

		// Token: 0x04000D59 RID: 3417
		[XmlElement("fishCaught")]
		public uint? obsolete_fishCaught;

		// Token: 0x04000D5A RID: 3418
		[XmlElement("geodesCracked")]
		public uint? obsolete_geodesCracked;

		// Token: 0x04000D5B RID: 3419
		[XmlElement("giftsGiven")]
		public uint? obsolete_giftsGiven;

		// Token: 0x04000D5C RID: 3420
		[XmlElement("goatCheeseMade")]
		public uint? obsolete_goatCheeseMade;

		// Token: 0x04000D5D RID: 3421
		[XmlElement("goatMilkProduced")]
		public uint? obsolete_goatMilkProduced;

		// Token: 0x04000D5E RID: 3422
		[XmlElement("goldFound")]
		public uint? obsolete_goldFound;

		// Token: 0x04000D5F RID: 3423
		[XmlElement("goodFriends")]
		public uint? obsolete_goodFriends;

		// Token: 0x04000D60 RID: 3424
		[XmlElement("individualMoneyEarned")]
		public uint? obsolete_individualMoneyEarned;

		// Token: 0x04000D61 RID: 3425
		[XmlElement("iridiumFound")]
		public uint? obsolete_iridiumFound;

		// Token: 0x04000D62 RID: 3426
		[XmlElement("ironFound")]
		public uint? obsolete_ironFound;

		// Token: 0x04000D63 RID: 3427
		[XmlElement("itemsCooked")]
		public uint? obsolete_itemsCooked;

		// Token: 0x04000D64 RID: 3428
		[XmlElement("itemsCrafted")]
		public uint? obsolete_itemsCrafted;

		// Token: 0x04000D65 RID: 3429
		[XmlElement("itemsForaged")]
		public uint? obsolete_itemsForaged;

		// Token: 0x04000D66 RID: 3430
		[XmlElement("itemsShipped")]
		public uint? obsolete_itemsShipped;

		// Token: 0x04000D67 RID: 3431
		[XmlElement("monstersKilled")]
		public uint? obsolete_monstersKilled;

		// Token: 0x04000D68 RID: 3432
		[XmlElement("mysticStonesCrushed")]
		public uint? obsolete_mysticStonesCrushed;

		// Token: 0x04000D69 RID: 3433
		[XmlElement("notesFound")]
		public uint? obsolete_notesFound;

		// Token: 0x04000D6A RID: 3434
		[XmlElement("otherPreciousGemsFound")]
		public uint? obsolete_otherPreciousGemsFound;

		// Token: 0x04000D6B RID: 3435
		[XmlElement("piecesOfTrashRecycled")]
		public uint? obsolete_piecesOfTrashRecycled;

		// Token: 0x04000D6C RID: 3436
		[XmlElement("preservesMade")]
		public uint? obsolete_preservesMade;

		// Token: 0x04000D6D RID: 3437
		[XmlElement("prismaticShardsFound")]
		public uint? obsolete_prismaticShardsFound;

		// Token: 0x04000D6E RID: 3438
		[XmlElement("questsCompleted")]
		public uint? obsolete_questsCompleted;

		// Token: 0x04000D6F RID: 3439
		[XmlElement("rabbitWoolProduced")]
		public uint? obsolete_rabbitWoolProduced;

		// Token: 0x04000D70 RID: 3440
		[XmlElement("rocksCrushed")]
		public uint? obsolete_rocksCrushed;

		// Token: 0x04000D71 RID: 3441
		[XmlElement("sheepWoolProduced")]
		public uint? obsolete_sheepWoolProduced;

		// Token: 0x04000D72 RID: 3442
		[XmlElement("slimesKilled")]
		public uint? obsolete_slimesKilled;

		// Token: 0x04000D73 RID: 3443
		[XmlElement("stepsTaken")]
		public uint? obsolete_stepsTaken;

		// Token: 0x04000D74 RID: 3444
		[XmlElement("stoneGathered")]
		public uint? obsolete_stoneGathered;

		// Token: 0x04000D75 RID: 3445
		[XmlElement("stumpsChopped")]
		public uint? obsolete_stumpsChopped;

		// Token: 0x04000D76 RID: 3446
		[XmlElement("timesFished")]
		public uint? obsolete_timesFished;

		// Token: 0x04000D77 RID: 3447
		[XmlElement("timesUnconscious")]
		public uint? obsolete_timesUnconscious;

		// Token: 0x04000D78 RID: 3448
		[XmlElement("totalMoneyGifted")]
		public uint? obsolete_totalMoneyGifted;

		// Token: 0x04000D79 RID: 3449
		[XmlElement("trufflesFound")]
		public uint? obsolete_trufflesFound;

		// Token: 0x04000D7A RID: 3450
		[XmlElement("weedsEliminated")]
		public uint? obsolete_weedsEliminated;

		// Token: 0x04000D7B RID: 3451
		[XmlElement("seedsSown")]
		public uint? obsolete_seedsSown;
	}
}

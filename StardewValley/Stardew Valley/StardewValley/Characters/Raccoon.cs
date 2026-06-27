using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Characters
{
	// Token: 0x0200037C RID: 892
	public class Raccoon : NPC
	{
		// Token: 0x060036D8 RID: 14040 RVA: 0x002B531B File Offset: 0x002B351B
		public Raccoon()
		{
			this.reloadSprite(false);
		}

		// Token: 0x060036D9 RID: 14041 RVA: 0x002B5340 File Offset: 0x002B3540
		public Raccoon(bool mrs_racooon = false) : base(new AnimatedSprite("Characters\\raccoon", mrs_racooon ? 40 : 0, 32, 32), new Vector2(54.5f, 8.25f) * 64f, 2, "Raccoon", null)
		{
			base.HideShadow = true;
			this.mrs_raccoon.Value = mrs_racooon;
			base.Breather = false;
			if (mrs_racooon)
			{
				base.Position = new Vector2(56.5f, 8.25f) * 64f;
				base.Name = "MrsRaccoon";
			}
		}

		// Token: 0x060036DA RID: 14042 RVA: 0x002B53E8 File Offset: 0x002B35E8
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.HideShadow = true;
			base.Breather = false;
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite("Characters\\raccoon", this.mrs_raccoon.Value ? 40 : 0, 32, 32);
			}
			if (this.mrs_raccoon.Value)
			{
				base.Position = new Vector2(56.5f, 8.25f) * 64f;
				base.Name = "MrsRaccoon";
			}
		}

		// Token: 0x060036DB RID: 14043 RVA: 0x002B5468 File Offset: 0x002B3668
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.mrs_raccoon, "mrs_raccoon");
			base.NetFields.AddField(this.mutex.NetFields, "mutex.NetFields");
		}

		// Token: 0x060036DC RID: 14044 RVA: 0x002B54A4 File Offset: 0x002B36A4
		public void activate()
		{
			if (this.mrs_raccoon.Value)
			{
				Utility.TryOpenShopMenu("Raccoon", base.Name, true);
				return;
			}
			bool interim = Game1.netWorldState.Value.Date.TotalDays - Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished < 7;
			if (!this.wasTalkedTo)
			{
				int whichDialogue = Game1.netWorldState.Value.TimesFedRaccoons;
				if (whichDialogue == 0)
				{
					interim = false;
				}
				if (whichDialogue >= 5 && !interim)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_intro"));
				}
				else if (whichDialogue > 5 && interim)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_interim"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_" + (interim ? "interim_" : "intro_") + whichDialogue.ToString()));
				}
				if (!interim)
				{
					Game1.afterDialogues = delegate()
					{
						this.mutex.RequestLock(delegate
						{
							this._activateMrRaccoon();
						}, delegate
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_busy"));
						});
					};
					return;
				}
			}
			else if (!interim)
			{
				this.mutex.RequestLock(delegate
				{
					this._activateMrRaccoon();
				}, delegate
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_busy"));
				});
			}
		}

		// Token: 0x060036DD RID: 14045 RVA: 0x002B55D1 File Offset: 0x002B37D1
		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			this.wasTalkedTo = false;
			NetMutex netMutex = this.mutex;
			if (netMutex == null)
			{
				return;
			}
			netMutex.ReleaseLock();
		}

		// Token: 0x060036DE RID: 14046 RVA: 0x002B55F4 File Offset: 0x002B37F4
		private void _activateMrRaccoon()
		{
			this.wasTalkedTo = true;
			if (Game1.netWorldState.Value.SeasonOfCurrentRacconBundle == -1)
			{
				Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = (Game1.seasonIndex + ((Game1.dayOfMonth > 21) ? 1 : 0)) % 4;
			}
			JunimoNoteMenu junimoNoteMenu = new JunimoNoteMenu(Raccoon.GetBundle(), "LooseSprites\\raccoon_bundle_menu");
			junimoNoteMenu.onIngredientDeposit = delegate(int index)
			{
				Game1.netWorldState.Value.raccoonBundles[index] = true;
			};
			junimoNoteMenu.onBundleComplete = new Action<JunimoNoteMenu>(this.bundleComplete);
			junimoNoteMenu.onScreenSwipeFinished = new Action<JunimoNoteMenu>(this.bundleCompleteAfterSwipe);
			junimoNoteMenu.behaviorBeforeCleanup = delegate(IClickableMenu _)
			{
				NetMutex netMutex = this.mutex;
				if (netMutex == null)
				{
					return;
				}
				netMutex.ReleaseLock();
			};
			Game1.activeClickableMenu = junimoNoteMenu;
		}

		// Token: 0x060036DF RID: 14047 RVA: 0x002B56A9 File Offset: 0x002B38A9
		public static Bundle GetBundle()
		{
			return Raccoon.GetBundle(Game1.netWorldState.Value.TimesFedRaccoons);
		}

		// Token: 0x060036E0 RID: 14048 RVA: 0x002B56C0 File Offset: 0x002B38C0
		public static Bundle GetBundle(int timesFed)
		{
			Random raccoonRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)(timesFed * 377), 0.0, 0.0, 0.0);
			for (int i = 0; i < 10; i++)
			{
				raccoonRandom.Next();
			}
			int which = (timesFed < 5) ? (timesFed % 5) : raccoonRandom.Next(5);
			List<BundleIngredientDescription> ingredients = new List<BundleIngredientDescription>();
			Raccoon.AddNextIngredient(ingredients, which, raccoonRandom);
			Raccoon.AddNextIngredient(ingredients, which, raccoonRandom);
			Raccoon.AddNextIngredient(ingredients, which, raccoonRandom);
			return new Bundle("Seafood", null, ingredients, new bool[1], "")
			{
				bundleTextureOverride = Game1.content.Load<Texture2D>("LooseSprites\\BundleSprites"),
				bundleTextureIndexOverride = 14 + which,
				bundleIndex = which
			};
		}

		// Token: 0x060036E1 RID: 14049 RVA: 0x002B5780 File Offset: 0x002B3980
		public Item getBundleReward()
		{
			switch (Game1.netWorldState.Value.TimesFedRaccoons)
			{
			case 1:
				return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 25);
			case 2:
				Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null, Array.Empty<string>());
				return ItemRegistry.Create("(O)Book_WildSeeds", 1, 0, false);
			case 3:
				Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null, Array.Empty<string>());
				return ItemRegistry.Create("(H)RaccoonHat", 1, 0, false);
			case 4:
				Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null, Array.Empty<string>());
				return ItemRegistry.Create("(O)872", 5, 0, false);
			case 5:
				Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null, Array.Empty<string>());
				return ItemRegistry.Create("(F)JungleTank", 1, 0, false);
			case 6:
				Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Raccoon_expanded", false, null, Array.Empty<string>());
				break;
			}
			Random raccoonRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)(Game1.netWorldState.Value.TimesFedRaccoons * 377), 0.0, 0.0, 0.0);
			for (int i = 0; i < 10; i++)
			{
				raccoonRandom.Next();
			}
			switch (raccoonRandom.Next(5))
			{
			case 0:
				return ItemRegistry.Create("(O)872", 7, 0, false);
			case 1:
				return ItemRegistry.Create("(O)PurpleBook", 1, 0, false);
			case 2:
				if (Game1.netWorldState.Value.GoldenWalnutsFound >= 100 && Utility.getFarmerItemsShippedPercent(null) < 1f)
				{
					Item missed = Utility.recentlyDiscoveredMissingBasicShippedItem;
					if (missed != null && missed.Category != -26 && missed.ItemId != "812")
					{
						return missed;
					}
				}
				return ItemRegistry.Create("(O)MysteryBox", 5, 0, false);
			case 3:
				return ItemRegistry.Create("(O)StardropTea", 1, 0, false);
			case 4:
				return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 25);
			default:
				return ItemRegistry.Create("(O)MysteryBox", 3, 0, false);
			}
		}

		// Token: 0x060036E2 RID: 14050 RVA: 0x002B5994 File Offset: 0x002B3B94
		private void bundleCompleteAfterSwipe(JunimoNoteMenu menu)
		{
			Game1.activeClickableMenu = null;
			NetMutex netMutex = this.mutex;
			if (netMutex != null)
			{
				netMutex.ReleaseLock();
			}
			Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.Date.TotalDays;
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_receive"));
			Game1.afterDialogues = delegate()
			{
				Game1.player.addItemByMenuIfNecessaryElseHoldUp(this.getBundleReward(), null, false);
			};
		}

		// Token: 0x060036E3 RID: 14051 RVA: 0x002B5A00 File Offset: 0x002B3C00
		private void bundleComplete(JunimoNoteMenu menu)
		{
			JunimoNoteMenu.screenSwipe = new ScreenSwipe(1, -1f, -1, -1, -1);
			NetWorldState value = Game1.netWorldState.Value;
			int timesFedRaccoons = value.TimesFedRaccoons;
			value.TimesFedRaccoons = timesFedRaccoons + 1;
			Game1.netWorldState.Value.raccoonBundles[0] = false;
			Game1.netWorldState.Value.raccoonBundles[1] = false;
			Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = -1;
			this.wasTalkedTo = false;
		}

		// Token: 0x060036E4 RID: 14052 RVA: 0x002B5A7C File Offset: 0x002B3C7C
		private static void AddNextIngredient(List<BundleIngredientDescription> ingredients, int whichBundle, Random r)
		{
			int whichIngredient = ingredients.Count;
			int whichSeasonToChoose = Game1.netWorldState.Value.SeasonOfCurrentRacconBundle;
			switch (whichBundle)
			{
			case 0:
			{
				if (whichIngredient == 0)
				{
					ingredients.Add(new BundleIngredientDescription(r.ChooseFrom(new string[]
					{
						"722",
						"721",
						"716",
						"719",
						"723",
						"718",
						"372"
					}), 5, 0, Game1.netWorldState.Value.raccoonBundles[0], null));
					return;
				}
				if (whichIngredient != 1)
				{
					return;
				}
				string[][] fish = new string[][]
				{
					new string[]
					{
						"136",
						"132",
						"700",
						"702",
						"156",
						"267",
						"706"
					},
					new string[]
					{
						"136",
						"132",
						"700",
						"702",
						"156",
						"267",
						"706",
						"138",
						"701",
						"146",
						"130"
					},
					new string[]
					{
						"136",
						"132",
						"700",
						"702",
						"156",
						"701",
						"269",
						"139",
						"139"
					},
					new string[]
					{
						"136",
						"132",
						"700",
						"702",
						"156",
						"146",
						"130",
						"141",
						"269"
					}
				};
				ingredients.Add(new BundleIngredientDescription("SmokedFish", 1, 0, Game1.netWorldState.Value.raccoonBundles[1], r.ChooseFrom(fish[whichSeasonToChoose])));
				return;
			}
			case 1:
			{
				string[][] fruits = new string[][]
				{
					new string[]
					{
						"90",
						"634",
						"638",
						"400",
						"88"
					},
					new string[]
					{
						"90",
						"258",
						"260",
						"635",
						"636",
						"88",
						"396"
					},
					new string[]
					{
						"90",
						"613",
						"282",
						"637",
						"410",
						"88",
						"406"
					},
					new string[]
					{
						"90",
						"414",
						"414",
						"88",
						"Powdermelon",
						"Powdermelon"
					}
				};
				if (whichIngredient == 0)
				{
					ingredients.Add(new BundleIngredientDescription("DriedFruit", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom(fruits[whichSeasonToChoose])));
					return;
				}
				if (whichIngredient != 1)
				{
					return;
				}
				string fruit = "";
				while (fruit == "" || fruit == ingredients[0].preservesId)
				{
					fruit = r.ChooseFrom(fruits[whichSeasonToChoose]);
				}
				ingredients.Add(new BundleIngredientDescription("Jelly", 1, 0, Game1.netWorldState.Value.raccoonBundles[1], fruit));
				return;
			}
			case 2:
			{
				string[][] mushrooms = new string[][]
				{
					new string[]
					{
						"422",
						"404",
						"257"
					},
					new string[]
					{
						"422",
						"404"
					},
					new string[]
					{
						"422",
						"404",
						"281"
					},
					new string[]
					{
						"422",
						"404"
					}
				};
				if (whichIngredient == 0)
				{
					ingredients.Add(new BundleIngredientDescription("DriedMushroom", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom(mushrooms[whichSeasonToChoose])));
					return;
				}
				if (whichIngredient != 1)
				{
					return;
				}
				ingredients.Add(new BundleIngredientDescription(r.ChooseFrom(new string[]
				{
					"-5",
					"78",
					"157"
				}), 5, 0, Game1.netWorldState.Value.raccoonBundles[1], null));
				return;
			}
			case 3:
			{
				string[][] veggies = new string[][]
				{
					new string[]
					{
						"190",
						"188",
						"250",
						"192",
						"16",
						"22",
						"Carrot",
						"Carrot"
					},
					new string[]
					{
						"270",
						"264",
						"256",
						"78",
						"SummerSquash",
						"SummerSquash"
					},
					new string[]
					{
						"Broccoli",
						"Broccoli",
						"278",
						"272",
						"276"
					},
					new string[]
					{
						"416",
						"412",
						"78"
					}
				};
				if (whichIngredient == 0)
				{
					ingredients.Add(new BundleIngredientDescription("Juice", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom(veggies[whichSeasonToChoose])));
					return;
				}
				if (whichIngredient != 1)
				{
					return;
				}
				string fruit2 = "";
				while (fruit2 == "" || fruit2 == ingredients[0].preservesId)
				{
					fruit2 = r.ChooseFrom(veggies[whichSeasonToChoose]);
				}
				ingredients.Add(new BundleIngredientDescription("Pickle", 1, 0, Game1.netWorldState.Value.raccoonBundles[1], fruit2));
				return;
			}
			case 4:
			{
				string[] items = new string[]
				{
					"Moss_10",
					"110_1",
					"168_5",
					"766_99",
					"767_20",
					"535_8",
					"536_5",
					"537_3",
					"393_4",
					"397_2",
					"684_20",
					"72_1",
					"68_3",
					"156_3"
				};
				if (whichIngredient == 0)
				{
					string s = r.ChooseFrom(items);
					ingredients.Add(new BundleIngredientDescription(s.Split('_', StringSplitOptions.None)[0], Convert.ToInt32(s.Split('_', StringSplitOptions.None)[1]), 0, Game1.netWorldState.Value.raccoonBundles[0], null));
					return;
				}
				if (whichIngredient != 1)
				{
					return;
				}
				string fruit3 = "";
				while (fruit3 == "" || fruit3.Split("_", StringSplitOptions.None)[0] == ingredients[0].id)
				{
					fruit3 = r.ChooseFrom(items);
				}
				ingredients.Add(new BundleIngredientDescription(fruit3.Split('_', StringSplitOptions.None)[0], Convert.ToInt32(fruit3.Split('_', StringSplitOptions.None)[1]), 0, Game1.netWorldState.Value.raccoonBundles[1], null));
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x060036E5 RID: 14053 RVA: 0x002B6204 File Offset: 0x002B4404
		public override void update(GameTime time, GameLocation location)
		{
			int shakeTimer = this.shakeTimer;
			base.update(time, location);
			NetMutex netMutex = this.mutex;
			if (netMutex != null)
			{
				netMutex.Update(location);
			}
			if (this.mrs_raccoon.Value)
			{
				this.Sprite.CurrentFrame = ((time.TotalGameTime.TotalMilliseconds % 13200.0 > 10000.0) ? (40 + (int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0)) : (32 + (int)(time.TotalGameTime.TotalMilliseconds % 1200.0 / 150.0)));
				return;
			}
			if (Vector2.Distance(base.Position, Game1.player.getStandingPosition()) >= 256f)
			{
				this.Sprite.CurrentFrame = ((time.TotalGameTime.TotalMilliseconds % 8000.0 < 3200.0) ? ((int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0)) : (48 + (int)(time.TotalGameTime.TotalMilliseconds % 400.0 / 100.0)));
				return;
			}
			int generalDirectionTowards = base.getGeneralDirectionTowards(Game1.player.getStandingPosition(), 32, false, false);
			if (generalDirectionTowards == 0)
			{
				this.Sprite.CurrentFrame = 16 + (int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0);
				return;
			}
			if (generalDirectionTowards - 1 > 2)
			{
				return;
			}
			this.Sprite.CurrentFrame = (int)(time.TotalGameTime.TotalMilliseconds % 800.0 / 100.0);
		}

		// Token: 0x060036E6 RID: 14054 RVA: 0x002B63D4 File Offset: 0x002B45D4
		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (this.shakeTimer <= 0)
			{
				if (this.mrs_raccoon.Value)
				{
					base.playNearbySoundLocal("Raccoon", new int?(2400), SoundContext.Default);
				}
				else
				{
					base.playNearbySoundLocal("Raccoon", null, SoundContext.Default);
				}
				this.shakeTimer = 200;
				who.freezePause = 300;
				DelayedAction.functionAfterDelay(new Action(this.activate), 300);
			}
			return true;
		}

		// Token: 0x060036E7 RID: 14055 RVA: 0x002B6452 File Offset: 0x002B4652
		public override void performTenMinuteUpdate(int timeOfDay, GameLocation l)
		{
			base.performTenMinuteUpdate(timeOfDay, l);
		}

		// Token: 0x060036E8 RID: 14056 RVA: 0x002B645C File Offset: 0x002B465C
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}

		// Token: 0x040023C8 RID: 9160
		[XmlElement("mrs_raccoon")]
		public readonly NetBool mrs_raccoon = new NetBool();

		// Token: 0x040023C9 RID: 9161
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		// Token: 0x040023CA RID: 9162
		private bool wasTalkedTo;

		// Token: 0x040023CB RID: 9163
		private float updateFacingDirectionTimer;
	}
}

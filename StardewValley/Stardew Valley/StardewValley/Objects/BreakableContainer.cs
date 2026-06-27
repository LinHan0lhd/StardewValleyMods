using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects.Trinkets;
using StardewValley.Tools;

namespace StardewValley.Objects
{
	// Token: 0x020001A3 RID: 419
	public class BreakableContainer : Object
	{
		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06001DAD RID: 7597 RVA: 0x00152A8B File Offset: 0x00150C8B
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001DAE RID: 7598 RVA: 0x00152A94 File Offset: 0x00150C94
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.debris, "debris").AddField(this.health, "health").AddField(this.hitSound, "hitSound").AddField(this.breakSound, "breakSound").AddField(this.breakDebrisSource, "breakDebrisSource").AddField(this.breakDebrisSource2, "breakDebrisSource2");
		}

		// Token: 0x06001DAF RID: 7599 RVA: 0x00152B10 File Offset: 0x00150D10
		public BreakableContainer()
		{
		}

		// Token: 0x06001DB0 RID: 7600 RVA: 0x00152B68 File Offset: 0x00150D68
		public BreakableContainer(Vector2 tile, string itemId, int health = 3, int debrisType = 12, string hitSound = "woodWhack", string breakSound = "barrelBreak") : base(tile, itemId, false)
		{
			this.health.Value = health;
			this.debris.Value = debrisType;
			this.hitSound.Value = hitSound;
			this.breakSound.Value = breakSound;
			this.breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
			this.breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
		}

		// Token: 0x06001DB1 RID: 7601 RVA: 0x00152C30 File Offset: 0x00150E30
		public static BreakableContainer GetBarrelForMines(Vector2 tile, MineShaft mine)
		{
			int mineArea = mine.getMineArea(-1);
			string itemId;
			if (mine.GetAdditionalDifficulty() > 0)
			{
				itemId = (((mineArea == 0 || mineArea == 10) && !mine.isDarkArea()) ? "262" : "118");
			}
			else if (mineArea != 40)
			{
				if (mineArea != 80)
				{
					if (mineArea != 121)
					{
						itemId = "118";
					}
					else
					{
						itemId = "124";
					}
				}
				else
				{
					itemId = "122";
				}
			}
			else
			{
				itemId = "120";
			}
			BreakableContainer barrel = new BreakableContainer(tile, itemId, 3, 12, "woodWhack", "barrelBreak");
			if (Game1.random.NextBool())
			{
				barrel.showNextIndex.Value = true;
			}
			return barrel;
		}

		// Token: 0x06001DB2 RID: 7602 RVA: 0x00152CCC File Offset: 0x00150ECC
		public static BreakableContainer GetBarrelForVolcanoDungeon(Vector2 tile)
		{
			BreakableContainer barrel = new BreakableContainer(tile, "174", 4, 14, "clank", "boulderBreak");
			if (Game1.random.NextBool())
			{
				barrel.showNextIndex.Value = true;
			}
			return barrel;
		}

		// Token: 0x06001DB3 RID: 7603 RVA: 0x00152D0C File Offset: 0x00150F0C
		public override bool performToolAction(Tool t)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (t != null && t.isHeavyHitter())
			{
				NetInt netInt = this.health;
				int value = netInt.Value;
				netInt.Value = value - 1;
				MeleeWeapon weapon = t as MeleeWeapon;
				if (weapon != null && weapon.type.Value == 2)
				{
					NetInt netInt2 = this.health;
					value = netInt2.Value;
					netInt2.Value = value - 1;
				}
				if (this.health.Value <= 0)
				{
					if (!string.IsNullOrEmpty(this.breakSound.Value))
					{
						base.playNearbySoundAll(this.breakSound.Value, null, SoundContext.Default);
					}
					this.releaseContents(t.getLastFarmerToUse());
					location.objects.Remove(this.tileLocation.Value);
					int numDebris = Game1.random.Next(4, 12);
					Color chipColor = this.GetChipColor();
					for (int i = 0; i < numDebris; i++)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? this.breakDebrisSource.Value : this.breakDebrisSource2.Value, 999f, 1, 0, this.tileLocation.Value * 64f + new Vector2(32f, 32f), false, Game1.random.NextBool(), (this.tileLocation.Y * 64f + 32f) / 10000f, 0.01f, chipColor, 4f, 0f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 8f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 64f, false)
							{
								motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-10, -7)),
								acceleration = new Vector2(0f, 0.3f)
							}
						});
					}
				}
				else if (!string.IsNullOrEmpty(this.hitSound.Value))
				{
					this.shakeTimer = 300;
					base.playNearbySoundAll(this.hitSound.Value, null, SoundContext.Default);
					Color? debrisColor = (base.ItemId == "120") ? new Color?(Color.White) : null;
					Game1.createRadialDebris(location, this.debris.Value, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 7), false, -1, false, debrisColor);
				}
			}
			return false;
		}

		// Token: 0x06001DB4 RID: 7604 RVA: 0x00152FD0 File Offset: 0x001511D0
		public override bool onExplosion(Farmer who)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			GameLocation location = this.Location;
			if (location == null)
			{
				return true;
			}
			this.releaseContents(who);
			int numDebris = Game1.random.Next(4, 12);
			Color chipColor = this.GetChipColor();
			for (int i = 0; i < numDebris; i++)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? this.breakDebrisSource.Value : this.breakDebrisSource2.Value, 999f, 1, 0, this.tileLocation.Value * 64f + new Vector2(32f, 32f), false, Game1.random.NextBool(), (this.tileLocation.Y * 64f + 32f) / 10000f, 0.01f, chipColor, 4f, 0f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 8f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 64f, false)
					{
						motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-10, -7)),
						acceleration = new Vector2(0f, 0.3f)
					}
				});
			}
			return true;
		}

		// Token: 0x06001DB5 RID: 7605 RVA: 0x00153148 File Offset: 0x00151348
		public Color GetChipColor()
		{
			string itemId = base.ItemId;
			if (itemId == "120")
			{
				return Color.White;
			}
			if (itemId == "122")
			{
				return new Color(109, 122, 80);
			}
			if (!(itemId == "174"))
			{
				return new Color(130, 80, 30);
			}
			return new Color(107, 76, 83);
		}

		// Token: 0x06001DB6 RID: 7606 RVA: 0x001531B4 File Offset: 0x001513B4
		public void releaseContents(Farmer who)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			double seedA = (double)this.tileLocation.X;
			double seedB = (double)this.tileLocation.Y * 10000.0;
			double seedC = Game1.stats.DaysPlayed;
			MineShaft mineShaft2 = location as MineShaft;
			Random r = Utility.CreateRandom(seedA, seedB, seedC, (double)((mineShaft2 != null) ? mineShaft2.mineLevel : 0), 0.0);
			int x = (int)this.tileLocation.X;
			int y = (int)this.tileLocation.Y;
			int mineLevel = -1;
			int difficultyLevel = 0;
			MineShaft mine = location as MineShaft;
			if (mine != null)
			{
				mineLevel = mine.mineLevel;
				if (mine.isContainerPlatform(x, y))
				{
					mine.updateMineLevelData(0, -1);
				}
				difficultyLevel = mine.GetAdditionalDifficulty();
			}
			if (r.NextDouble() < 0.2)
			{
				if (r.NextDouble() < 0.1)
				{
					Game1.createMultipleItemDebris(Utility.getRaccoonSeedForCurrentTimeOfYear(who, r, -1), new Vector2((float)x, (float)y) * 64f + new Vector2(32f), -1, location, -1, false);
				}
				return;
			}
			MineShaft mineShaft = location as MineShaft;
			if (mineShaft != null)
			{
				if (mineShaft.mineLevel > 120 && !mineShaft.isSideBranch(-1))
				{
					int floor = mineShaft.mineLevel - 121;
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
					{
						float chance = (float)(floor + Game1.player.team.calicoEggSkullCavernRating.Value * 2) * 0.003f;
						if (chance > 0.33f)
						{
							chance = 0.33f;
						}
						if (r.NextBool(chance))
						{
							Game1.createMultipleObjectDebris("CalicoEgg", x, y, r.Next(1, 4), who.UniqueMultiplayerID, location);
						}
					}
				}
				int effectiveMineLevel = mineShaft.mineLevel;
				if (mineShaft.mineLevel == 77377)
				{
					effectiveMineLevel = 5000;
				}
				Trinket.TrySpawnTrinket(location, null, new Vector2((float)x, (float)y) * 64f + new Vector2(32f), 1.0 + (double)effectiveMineLevel * 0.001);
			}
			if (r.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
			{
				Game1.createMultipleObjectDebris("(O)890", x, y, r.Next(1, 3), who.UniqueMultiplayerID, location);
			}
			if (Utility.tryRollMysteryBox(0.0081 + Game1.player.team.AverageDailyLuck(null) / 15.0, r))
			{
				Game1.createItemDebris(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 1, 0, false), new Vector2((float)x, (float)y) * 64f + new Vector2(32f), -1, location, -1, false);
			}
			Utility.trySpawnRareObject(who, new Vector2((float)x, (float)y) * 64f, location, 1.5, 1.0, -1, r);
			if (difficultyLevel <= 0)
			{
				string itemId = base.ItemId;
				if (!(itemId == "118"))
				{
					if (!(itemId == "120"))
					{
						if (!(itemId == "124") && !(itemId == "122"))
						{
							if (!(itemId == "174"))
							{
								return;
							}
							if (r.NextDouble() < 0.1)
							{
								Game1.player.team.RequestLimitedNutDrops("VolcanoBarrel", location, x * 64, y * 64, 5, 1);
							}
							VolcanoDungeon dungeon = location as VolcanoDungeon;
							if (dungeon != null && dungeon.level.Value == 5 && x == 34)
							{
								Item item = ItemRegistry.Create("(O)851", 1, 0, false);
								item.Quality = 2;
								Game1.createItemDebris(item, new Vector2((float)x, (float)y) * 64f, 1, null, -1, false);
								return;
							}
							if (r.NextDouble() < 0.75)
							{
								if (r.NextDouble() < 0.8)
								{
									switch (r.Next(7))
									{
									case 0:
										Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
										return;
									case 1:
										Game1.createMultipleObjectDebris("(O)384", x, y, r.Next(1, 4), location);
										return;
									case 2:
										location.characters.Add(new DwarvishSentry(new Vector2((float)x, (float)y) * 64f));
										return;
									case 3:
										Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(2, 6), location);
										return;
									case 4:
										Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(2, 6), location);
										return;
									case 5:
										Game1.createMultipleObjectDebris("66", x, y, 1, location);
										return;
									case 6:
										Game1.createMultipleObjectDebris("(O)709", x, y, r.Next(2, 6), location);
										return;
									default:
										return;
									}
								}
								else
								{
									switch (r.Next(5))
									{
									case 0:
										Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
										return;
									case 1:
										Game1.createMultipleObjectDebris("(O)749", x, y, r.Next(1, 3), location);
										return;
									case 2:
										Game1.createMultipleObjectDebris("(O)60", x, y, 1, location);
										return;
									case 3:
										Game1.createMultipleObjectDebris("(O)64", x, y, 1, location);
										return;
									case 4:
										Game1.createMultipleObjectDebris("(O)68", x, y, 1, location);
										return;
									default:
										return;
									}
								}
							}
							else if (r.NextDouble() < 0.4)
							{
								switch (r.Next(9))
								{
								case 0:
									Game1.createMultipleObjectDebris("(O)72", x, y, 1, location);
									return;
								case 1:
									Game1.createMultipleObjectDebris("(O)831", x, y, r.Next(1, 4), location);
									return;
								case 2:
									Game1.createMultipleObjectDebris("(O)833", x, y, r.Next(1, 3), location);
									return;
								case 3:
									Game1.createMultipleObjectDebris("(O)749", x, y, 1, location);
									return;
								case 4:
									Game1.createMultipleObjectDebris("(O)386", x, y, 1, location);
									return;
								case 5:
									Game1.createMultipleObjectDebris("(O)848", x, y, 1, location);
									return;
								case 6:
									Game1.createMultipleObjectDebris("(O)856", x, y, 1, location);
									return;
								case 7:
									Game1.createMultipleObjectDebris("(O)886", x, y, 1, location);
									return;
								case 8:
									Game1.createMultipleObjectDebris("(O)688", x, y, 1, location);
									return;
								default:
									return;
								}
							}
							else
							{
								location.characters.Add(new DwarvishSentry(new Vector2((float)x, (float)y) * 64f));
							}
						}
						else if (r.NextDouble() < 0.65)
						{
							if (r.NextDouble() < 0.8)
							{
								switch (r.Next(8))
								{
								case 0:
									Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
									return;
								case 1:
									Game1.createMultipleObjectDebris("(O)384", x, y, r.Next(1, 4), location);
									return;
								case 2:
									break;
								case 3:
									Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(2, 6), location);
									return;
								case 4:
									Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(2, 6), location);
									return;
								case 5:
									Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
									return;
								case 6:
									Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 6), location);
									return;
								case 7:
									Game1.createMultipleObjectDebris("(O)881", x, y, r.Next(2, 6), location);
									return;
								default:
									return;
								}
							}
							else
							{
								switch (r.Next(4))
								{
								case 0:
									Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
									return;
								case 1:
									Game1.createMultipleObjectDebris("(O)537", x, y, r.Next(1, 3), location);
									return;
								case 2:
									Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)82" : "(O)78", x, y, r.Next(1, 3), location);
									return;
								case 3:
									Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
									return;
								default:
									return;
								}
							}
						}
						else if (r.NextDouble() < 0.4)
						{
							switch (r.Next(6))
							{
							case 0:
								Game1.createMultipleObjectDebris("(O)60", x, y, 1, location);
								return;
							case 1:
								Game1.createMultipleObjectDebris("(O)64", x, y, 1, location);
								return;
							case 2:
								Game1.createMultipleObjectDebris("(O)709", x, y, r.Next(1, 4), location);
								return;
							case 3:
								Game1.createMultipleObjectDebris("(O)749", x, y, 1, location);
								return;
							case 4:
								Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(mineLevel, x, y), new Vector2((float)x, (float)y) * 64f + new Vector2(32f, 32f), r.Next(4), location, -1, false);
								return;
							case 5:
								Game1.createMultipleObjectDebris("(O)688", x, y, 1, location);
								return;
							default:
								return;
							}
						}
					}
					else if (r.NextDouble() < 0.65)
					{
						if (r.NextDouble() < 0.8)
						{
							switch (r.Next(9))
							{
							case 0:
								Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
								return;
							case 1:
								Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(1, 4), location);
								return;
							case 2:
								break;
							case 3:
								Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(2, 6), location);
								return;
							case 4:
								Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 6), location);
								return;
							case 5:
								Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)84" : r.Choose("(O)92", "(O)371"), x, y, r.Choose(2, 3), location);
								return;
							case 6:
								Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 4), location);
								return;
							case 7:
								Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
								return;
							case 8:
								Game1.createMultipleObjectDebris("(O)770", x, y, 1, location);
								return;
							default:
								return;
							}
						}
						else
						{
							switch (r.Next(4))
							{
							case 0:
								Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
								return;
							case 1:
								Game1.createMultipleObjectDebris("(O)536", x, y, r.Next(1, 3), location);
								return;
							case 2:
								Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
								return;
							case 3:
								Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
								return;
							default:
								return;
							}
						}
					}
					else if (r.NextDouble() < 0.4)
					{
						switch (r.Next(5))
						{
						case 0:
							Game1.createMultipleObjectDebris("(O)62", x, y, 1, location);
							return;
						case 1:
							Game1.createMultipleObjectDebris("(O)70", x, y, 1, location);
							return;
						case 2:
							Game1.createMultipleObjectDebris("(O)709", x, y, r.Next(1, 4), location);
							return;
						case 3:
							Game1.createMultipleObjectDebris("(O)536", x, y, 1, location);
							return;
						case 4:
							Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(mineLevel, x, y), new Vector2((float)x, (float)y) * 64f + new Vector2(32f, 32f), r.Next(4), location, -1, false);
							return;
						default:
							return;
						}
					}
				}
				else if (r.NextDouble() < 0.65)
				{
					if (r.NextDouble() < 0.8)
					{
						switch (r.Next(9))
						{
						case 0:
							Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
							return;
						case 1:
							Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(1, 4), location);
							return;
						case 2:
							break;
						case 3:
							Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
							return;
						case 4:
							Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 3), location);
							return;
						case 5:
							Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)80" : r.Choose("(O)92", "(O)370"), x, y, r.Choose(2, 3), location);
							return;
						case 6:
							Game1.createMultipleObjectDebris("(O)388", x, y, r.Next(2, 6), location);
							return;
						case 7:
							Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
							return;
						case 8:
							Game1.createMultipleObjectDebris("(O)770", x, y, 1, location);
							return;
						default:
							return;
						}
					}
					else
					{
						switch (r.Next(4))
						{
						case 0:
							Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
							return;
						case 1:
							Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
							return;
						case 2:
							Game1.createMultipleObjectDebris("(O)78", x, y, r.Next(1, 3), location);
							return;
						case 3:
							Game1.createMultipleObjectDebris("(O)535", x, y, r.Next(1, 3), location);
							return;
						default:
							return;
						}
					}
				}
				else if (r.NextDouble() < 0.4)
				{
					switch (r.Next(5))
					{
					case 0:
						Game1.createMultipleObjectDebris("(O)66", x, y, 1, location);
						return;
					case 1:
						Game1.createMultipleObjectDebris("(O)68", x, y, 1, location);
						return;
					case 2:
						Game1.createMultipleObjectDebris("(O)709", x, y, 1, location);
						return;
					case 3:
						Game1.createMultipleObjectDebris("(O)535", x, y, 1, location);
						return;
					case 4:
						Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(mineLevel, x, y), new Vector2((float)x, (float)y) * 64f + new Vector2(32f, 32f), r.Next(4), location, -1, false);
						return;
					default:
						return;
					}
				}
				return;
			}
			if (r.NextDouble() < 0.15)
			{
				return;
			}
			if (r.NextDouble() < 0.008)
			{
				Game1.createMultipleObjectDebris("(O)858", x, y, 1, location);
			}
			if (r.NextDouble() < 0.01)
			{
				Game1.createItemDebris(ItemRegistry.Create("(BC)71", 1, 0, false), new Vector2((float)x, (float)y) * 64f + new Vector2(32f), 0, null, -1, false);
			}
			if (r.NextDouble() < 0.01)
			{
				Game1.createMultipleObjectDebris(r.Choose("(O)918", "(O)919", "(O)920"), x, y, 1, location);
			}
			if (r.NextDouble() < 0.01)
			{
				Game1.createMultipleObjectDebris("(O)386", x, y, r.Next(1, 4), location);
			}
			switch (r.Next(17))
			{
			case 0:
				Game1.createMultipleObjectDebris("(O)382", x, y, r.Next(1, 3), location);
				return;
			case 1:
				Game1.createMultipleObjectDebris("(O)380", x, y, r.Next(1, 4), location);
				return;
			case 2:
				Game1.createMultipleObjectDebris("(O)62", x, y, 1, location);
				return;
			case 3:
				Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
				return;
			case 4:
				Game1.createMultipleObjectDebris("(O)80", x, y, r.Next(2, 3), location);
				return;
			case 5:
				Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? "(O)84" : r.Choose("(O)92", "(O)370"), x, y, r.Choose(2, 3), location);
				return;
			case 6:
				Game1.createMultipleObjectDebris("(O)70", x, y, 1, location);
				return;
			case 7:
				Game1.createMultipleObjectDebris("(O)390", x, y, r.Next(2, 6), location);
				return;
			case 8:
				Game1.createMultipleObjectDebris("(O)" + r.Next(218, 245).ToString(), x, y, 1, location);
				return;
			case 9:
				Game1.createMultipleObjectDebris((Game1.whichFarm == 6) ? "(O)920" : "(O)749", x, y, 1, location);
				return;
			case 10:
				Game1.createMultipleObjectDebris("(O)286", x, y, 1, location);
				return;
			case 11:
				Game1.createMultipleObjectDebris("(O)378", x, y, r.Next(1, 4), location);
				return;
			case 12:
				Game1.createMultipleObjectDebris("(O)384", x, y, r.Next(1, 4), location);
				return;
			case 13:
				Game1.createMultipleObjectDebris("(O)287", x, y, 1, location);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001DB7 RID: 7607 RVA: 0x00154168 File Offset: 0x00152368
		public override void updateWhenCurrentLocation(GameTime time)
		{
			if (this.shakeTimer > 0)
			{
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		// Token: 0x06001DB8 RID: 7608 RVA: 0x0015419C File Offset: 0x0015239C
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 scaleFactor = this.getScale();
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
			Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			if (this.shakeTimer > 0)
			{
				int intensity = this.shakeTimer / 100 + 1;
				destination.X += Game1.random.Next(-intensity, intensity + 1);
				destination.Y += Game1.random.Next(-intensity, intensity + 1);
			}
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(data.GetTexture(), destination, new Rectangle?(data.GetSourceRect((this.showNextIndex.Value > false) ? 1 : 0, null)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
		}

		// Token: 0x0400124C RID: 4684
		public const string barrelId = "118";

		// Token: 0x0400124D RID: 4685
		public const string frostBarrelId = "120";

		// Token: 0x0400124E RID: 4686
		public const string darkBarrelId = "122";

		// Token: 0x0400124F RID: 4687
		public const string desertBarrelId = "124";

		// Token: 0x04001250 RID: 4688
		public const string volcanoBarrelId = "174";

		// Token: 0x04001251 RID: 4689
		public const string waterBarrelId = "262";

		// Token: 0x04001252 RID: 4690
		[XmlElement("debris")]
		private readonly NetInt debris = new NetInt();

		// Token: 0x04001253 RID: 4691
		private new int shakeTimer;

		// Token: 0x04001254 RID: 4692
		[XmlElement("health")]
		private new readonly NetInt health = new NetInt();

		// Token: 0x04001255 RID: 4693
		[XmlElement("hitSound")]
		private readonly NetString hitSound = new NetString();

		// Token: 0x04001256 RID: 4694
		[XmlElement("breakSound")]
		private readonly NetString breakSound = new NetString();

		// Token: 0x04001257 RID: 4695
		[XmlElement("breakDebrisSource")]
		private readonly NetRectangle breakDebrisSource = new NetRectangle();

		// Token: 0x04001258 RID: 4696
		[XmlElement("breakDebrisSource2")]
		private readonly NetRectangle breakDebrisSource2 = new NetRectangle();
	}
}

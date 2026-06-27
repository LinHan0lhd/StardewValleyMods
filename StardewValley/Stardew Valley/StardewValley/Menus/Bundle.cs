using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus
{
	// Token: 0x02000280 RID: 640
	public class Bundle : ClickableComponent
	{
		// Token: 0x06002A60 RID: 10848 RVA: 0x001FC8E0 File Offset: 0x001FAAE0
		public Bundle(string name, string displayName, List<BundleIngredientDescription> ingredients, bool[] completedIngredientsList, string rewardListString = "") : base(new Rectangle(0, 0, 64, 64), "")
		{
			this.name = name;
			this.label = displayName;
			this.rewardDescription = rewardListString;
			this.numberOfIngredientSlots = ingredients.Count;
			this.ingredients = ingredients;
		}

		// Token: 0x06002A61 RID: 10849 RVA: 0x001FC93C File Offset: 0x001FAB3C
		public Bundle(int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, Point position, string textureName, JunimoNoteMenu menu) : base(new Rectangle(position.X, position.Y, 64, 64), "")
		{
			if (menu != null && menu.fromGameMenu)
			{
				this.depositsAllowed = false;
			}
			this.bundleIndex = bundleIndex;
			string[] split = rawBundleInfo.Split('/', StringSplitOptions.None);
			this.name = split[0];
			this.label = split[6];
			this.rewardDescription = split[1];
			if (!string.IsNullOrWhiteSpace(split[5]))
			{
				try
				{
					string[] parts = split[5].Split(':', 2, StringSplitOptions.None);
					if (parts.Length == 2)
					{
						this.bundleTextureOverride = Game1.content.Load<Texture2D>(parts[0]);
						this.bundleTextureIndexOverride = int.Parse(parts[1]);
					}
					else
					{
						this.bundleTextureIndexOverride = int.Parse(split[5]);
					}
				}
				catch
				{
					this.bundleTextureOverride = null;
					this.bundleTextureIndexOverride = -1;
				}
			}
			string[] ingredientsSplit = ArgUtility.SplitBySpace(split[2]);
			this.complete = true;
			this.ingredients = new List<BundleIngredientDescription>();
			int tally = 0;
			for (int i = 0; i < ingredientsSplit.Length; i += 3)
			{
				this.ingredients.Add(new BundleIngredientDescription(ingredientsSplit[i], Convert.ToInt32(ingredientsSplit[i + 1]), Convert.ToInt32(ingredientsSplit[i + 2]), completedIngredientsList[i / 3], null));
				if (!completedIngredientsList[i / 3])
				{
					this.complete = false;
				}
				else
				{
					tally++;
				}
			}
			this.bundleColor = Convert.ToInt32(split[3]);
			this.numberOfIngredientSlots = ArgUtility.GetInt(split, 4, this.ingredients.Count);
			if (tally >= this.numberOfIngredientSlots)
			{
				this.complete = true;
			}
			this.sprite = new TemporaryAnimatedSprite(textureName, new Rectangle(this.bundleColor * 256 % 512, 244 + this.bundleColor * 256 / 512 * 16, 16, 16), 70f, 3, 99999, new Vector2((float)this.bounds.X, (float)this.bounds.Y), false, false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
			{
				pingPong = true
			};
			this.sprite.paused = true;
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.sprite;
			temporaryAnimatedSprite.sourceRect.X = temporaryAnimatedSprite.sourceRect.X + this.sprite.sourceRect.Width;
			if (this.name.ContainsIgnoreCase(Game1.currentSeason) && !this.complete)
			{
				this.shake(0.07363108f);
			}
			if (this.complete)
			{
				this.completionAnimation(menu, false, 0);
			}
		}

		// Token: 0x06002A62 RID: 10850 RVA: 0x001FCBD4 File Offset: 0x001FADD4
		public Item getReward()
		{
			return Utility.getItemFromStandardTextDescription(this.rewardDescription, Game1.player, ' ');
		}

		// Token: 0x06002A63 RID: 10851 RVA: 0x001FCBE8 File Offset: 0x001FADE8
		public void shake(float force = 0.07363108f)
		{
			if (this.sprite.paused)
			{
				this.maxShake = force;
			}
		}

		// Token: 0x06002A64 RID: 10852 RVA: 0x001FCC00 File Offset: 0x001FAE00
		public void shake(int extraInfo)
		{
			this.maxShake = 0.07363108f;
			if (extraInfo == 1)
			{
				Game1.playSound("leafrustle", null);
				TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(50, this.sprite.position, Bundle.getColorFromColorIndex(this.bundleColor), 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					motion = new Vector2(-1f, 0.5f),
					acceleration = new Vector2(0f, 0.02f)
				};
				TemporaryAnimatedSprite temporaryAnimatedSprite = tempSprite;
				temporaryAnimatedSprite.sourceRect.Y = temporaryAnimatedSprite.sourceRect.Y + 1;
				TemporaryAnimatedSprite temporaryAnimatedSprite2 = tempSprite;
				temporaryAnimatedSprite2.sourceRect.Height = temporaryAnimatedSprite2.sourceRect.Height - 1;
				JunimoNoteMenu.tempSprites.Add(tempSprite);
				tempSprite = new TemporaryAnimatedSprite(50, this.sprite.position, Bundle.getColorFromColorIndex(this.bundleColor), 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					motion = new Vector2(1f, 0.5f),
					acceleration = new Vector2(0f, 0.02f),
					flipped = true,
					delayBeforeAnimationStart = 50
				};
				TemporaryAnimatedSprite temporaryAnimatedSprite3 = tempSprite;
				temporaryAnimatedSprite3.sourceRect.Y = temporaryAnimatedSprite3.sourceRect.Y + 1;
				TemporaryAnimatedSprite temporaryAnimatedSprite4 = tempSprite;
				temporaryAnimatedSprite4.sourceRect.Height = temporaryAnimatedSprite4.sourceRect.Height - 1;
				JunimoNoteMenu.tempSprites.Add(tempSprite);
			}
		}

		// Token: 0x06002A65 RID: 10853 RVA: 0x001FCD48 File Offset: 0x001FAF48
		public void tryHoverAction(int x, int y)
		{
			if (this.bounds.Contains(x, y) && !this.complete)
			{
				this.sprite.paused = false;
				JunimoNoteMenu.hoverText = Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.label);
				return;
			}
			if (!this.complete)
			{
				this.sprite.reset();
				TemporaryAnimatedSprite temporaryAnimatedSprite = this.sprite;
				temporaryAnimatedSprite.sourceRect.X = temporaryAnimatedSprite.sourceRect.X + this.sprite.sourceRect.Width;
				this.sprite.paused = true;
			}
		}

		// Token: 0x06002A66 RID: 10854 RVA: 0x001FCDD8 File Offset: 0x001FAFD8
		public bool IsValidItemForThisIngredientDescription(Item item, BundleIngredientDescription ingredient)
		{
			if (item == null || ingredient.completed || ingredient.quality > item.Quality)
			{
				return false;
			}
			if (ingredient.preservesId != null)
			{
				ItemQueryContext context = new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random, "query 'FLAVORED_ITEM'");
				ItemQueryResult itemQueryResult = ItemQueryResolver.TryResolve("FLAVORED_ITEM " + ingredient.id + " " + ingredient.preservesId, context, ItemQuerySearchMode.All, null, null, false, null, null).FirstOrDefault<ItemQueryResult>();
				Object resultObj = ((itemQueryResult != null) ? itemQueryResult.Item : null) as Object;
				if (resultObj != null)
				{
					Object ingredientObj = item as Object;
					if (ingredientObj != null && ingredientObj.preservedParentSheetIndex.Value != null && item.QualifiedItemId == resultObj.QualifiedItemId && ingredientObj.preservedParentSheetIndex.Value.Contains(ingredient.preservesId))
					{
						return true;
					}
				}
				return false;
			}
			if (ingredient.category == null)
			{
				return ItemRegistry.HasItemId(item, ingredient.id);
			}
			if (item.QualifiedItemId == "(O)107" && ingredient.category.GetValueOrDefault() == -5)
			{
				return true;
			}
			int category = item.Category;
			int? category2 = ingredient.category;
			return category == category2.GetValueOrDefault() & category2 != null;
		}

		// Token: 0x06002A67 RID: 10855 RVA: 0x001FCF14 File Offset: 0x001FB114
		public int GetBundleIngredientDescriptionIndexForItem(Item item)
		{
			for (int i = 0; i < this.ingredients.Count; i++)
			{
				if (this.IsValidItemForThisIngredientDescription(item, this.ingredients[i]))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06002A68 RID: 10856 RVA: 0x001FCF4F File Offset: 0x001FB14F
		public bool canAcceptThisItem(Item item, ClickableTextureComponent slot)
		{
			return this.canAcceptThisItem(item, slot, false);
		}

		// Token: 0x06002A69 RID: 10857 RVA: 0x001FCF5C File Offset: 0x001FB15C
		public bool canAcceptThisItem(Item item, ClickableTextureComponent slot, bool ignore_stack_count = false)
		{
			if (!this.depositsAllowed)
			{
				return false;
			}
			for (int i = 0; i < this.ingredients.Count; i++)
			{
				if (this.IsValidItemForThisIngredientDescription(item, this.ingredients[i]) && (ignore_stack_count || this.ingredients[i].stack <= item.Stack) && ((slot != null) ? slot.item : null) == null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002A6A RID: 10858 RVA: 0x001FCFCC File Offset: 0x001FB1CC
		public Item tryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName, JunimoNoteMenu parentMenu)
		{
			if (!this.depositsAllowed)
			{
				if (Game1.player.hasCompletedCommunityCenter())
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtAJM"), true);
				}
				else
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:JunimoNote_MustBeAtCC"), true);
				}
				return item;
			}
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
			int i = 0;
			while (i < this.ingredients.Count)
			{
				BundleIngredientDescription ingredient = this.ingredients[i];
				if (this.IsValidItemForThisIngredientDescription(item, ingredient) && slot.item == null)
				{
					item = item.ConsumeStack(ingredient.stack);
					List<BundleIngredientDescription> list = this.ingredients;
					int index = i;
					ingredient = new BundleIngredientDescription(ingredient, true);
					list[index] = ingredient;
					this.ingredientDepositAnimation(slot, noteTextureName, false);
					string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
					if (ingredient.preservesId != null)
					{
						slot.item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
					}
					else
					{
						slot.item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality, false);
					}
					Game1.playSound("newArtifact", null);
					slot.sourceRect.X = 512;
					slot.sourceRect.Y = 244;
					if (parentMenu.onIngredientDeposit != null)
					{
						parentMenu.onIngredientDeposit(i);
						break;
					}
					communityCenter.bundles.FieldDict[this.bundleIndex][i] = true;
					Game1.multiplayer.globalChatInfoMessage("BundleDonate", new string[]
					{
						Game1.player.displayName,
						TokenStringBuilder.ItemNameFor(slot.item, null)
					});
					break;
				}
				else
				{
					i++;
				}
			}
			return item;
		}

		// Token: 0x06002A6B RID: 10859 RVA: 0x001FD180 File Offset: 0x001FB380
		public void ingredientDepositAnimation(ClickableTextureComponent slot, string noteTextureName, bool skipAnimation = false)
		{
			TemporaryAnimatedSprite t = new TemporaryAnimatedSprite(noteTextureName, new Rectangle(530, 244, 18, 18), 50f, 6, 1, new Vector2((float)slot.bounds.X, (float)slot.bounds.Y), false, false, 0.88f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
			{
				holdLastFrame = true,
				endSound = "cowboy_monsterhit"
			};
			if (skipAnimation)
			{
				t.sourceRect.Offset(t.sourceRect.Width * 5, 0);
				t.sourceRectStartingPos = new Vector2((float)t.sourceRect.X, (float)t.sourceRect.Y);
				t.animationLength = 1;
			}
			JunimoNoteMenu.tempSprites.Add(t);
		}

		// Token: 0x06002A6C RID: 10860 RVA: 0x001FD254 File Offset: 0x001FB454
		public bool canBeClicked()
		{
			return !this.complete;
		}

		// Token: 0x06002A6D RID: 10861 RVA: 0x001FD25F File Offset: 0x001FB45F
		public void completionAnimation(JunimoNoteMenu menu, bool playSound = true, int delay = 0)
		{
			if (delay <= 0)
			{
				this.completionAnimation(playSound);
				return;
			}
			this.completionTimer = delay;
		}

		// Token: 0x06002A6E RID: 10862 RVA: 0x001FD274 File Offset: 0x001FB474
		private void completionAnimation(bool playSound = true)
		{
			JunimoNoteMenu junimoNoteMenu = Game1.activeClickableMenu as JunimoNoteMenu;
			if (junimoNoteMenu != null)
			{
				junimoNoteMenu.takeDownBundleSpecificPage();
			}
			this.sprite.pingPong = false;
			this.sprite.paused = false;
			this.sprite.sourceRect.X = (int)this.sprite.sourceRectStartingPos.X;
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.sprite;
			temporaryAnimatedSprite.sourceRect.X = temporaryAnimatedSprite.sourceRect.X + this.sprite.sourceRect.Width;
			this.sprite.animationLength = 15;
			this.sprite.interval = 50f;
			this.sprite.totalNumberOfLoops = 0;
			this.sprite.holdLastFrame = true;
			this.sprite.endFunction = new TemporaryAnimatedSprite.endBehavior(this.shake);
			this.sprite.extraInfoForEndBehavior = 1;
			if (this.complete)
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite2 = this.sprite;
				temporaryAnimatedSprite2.sourceRect.X = temporaryAnimatedSprite2.sourceRect.X + this.sprite.sourceRect.Width * 14;
				this.sprite.sourceRectStartingPos = new Vector2((float)this.sprite.sourceRect.X, (float)this.sprite.sourceRect.Y);
				this.sprite.currentParentTileIndex = 14;
				this.sprite.interval = 0f;
				this.sprite.animationLength = 1;
				this.sprite.extraInfoForEndBehavior = 0;
			}
			else
			{
				if (playSound)
				{
					Game1.playSound("dwop", null);
				}
				this.bounds.Inflate(64, 64);
				JunimoNoteMenu.tempSprites.AddRange(Utility.sparkleWithinArea(this.bounds, 8, Bundle.getColorFromColorIndex(this.bundleColor) * 0.5f, 100, 0, ""));
				this.bounds.Inflate(-64, -64);
			}
			this.complete = true;
		}

		// Token: 0x06002A6F RID: 10863 RVA: 0x001FD454 File Offset: 0x001FB654
		public void update(GameTime time)
		{
			this.sprite.update(time);
			if (this.completionTimer > 0 && JunimoNoteMenu.screenSwipe == null)
			{
				this.completionTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.completionTimer <= 0)
				{
					this.completionAnimation(true);
				}
			}
			if (Game1.random.NextDouble() < 0.005 && (this.complete || this.name.ContainsIgnoreCase(Game1.currentSeason)))
			{
				this.shake(0.07363108f);
			}
			if (this.maxShake > 0f)
			{
				if (this.shakeLeft)
				{
					this.sprite.rotation -= 0.015707964f;
					if (this.sprite.rotation <= -this.maxShake)
					{
						this.shakeLeft = false;
					}
				}
				else
				{
					this.sprite.rotation += 0.015707964f;
					if (this.sprite.rotation >= this.maxShake)
					{
						this.shakeLeft = true;
					}
				}
			}
			if (this.maxShake > 0f)
			{
				this.maxShake = Math.Max(0f, this.maxShake - 0.0007669904f);
			}
		}

		// Token: 0x06002A70 RID: 10864 RVA: 0x001FD586 File Offset: 0x001FB786
		public void draw(SpriteBatch b)
		{
			this.sprite.draw(b, true, 0, 0, 1f);
		}

		// Token: 0x06002A71 RID: 10865 RVA: 0x001FD59C File Offset: 0x001FB79C
		public static Color getColorFromColorIndex(int color)
		{
			switch (color)
			{
			case 0:
				return Color.Lime;
			case 1:
				return Color.DeepPink;
			case 2:
				return Color.Orange;
			case 3:
				return Color.Orange;
			case 4:
				return Color.Red;
			case 5:
				return Color.LightBlue;
			case 6:
				return Color.Cyan;
			default:
				return Color.Lime;
			}
		}

		// Token: 0x04001C01 RID: 7169
		public const int NameIndex = 0;

		// Token: 0x04001C02 RID: 7170
		public const int RewardIndex = 1;

		// Token: 0x04001C03 RID: 7171
		public const int IngredientsIndex = 2;

		// Token: 0x04001C04 RID: 7172
		public const int ColorIndex = 3;

		// Token: 0x04001C05 RID: 7173
		public const int NumberOfSlotsIndex = 4;

		// Token: 0x04001C06 RID: 7174
		public const int SpriteIndex = 5;

		// Token: 0x04001C07 RID: 7175
		public const int DisplayNameIndex = 6;

		// Token: 0x04001C08 RID: 7176
		public const int FieldCount = 7;

		// Token: 0x04001C09 RID: 7177
		public const float shakeRate = 0.015707964f;

		// Token: 0x04001C0A RID: 7178
		public const float shakeDecayRate = 0.0030679617f;

		// Token: 0x04001C0B RID: 7179
		public const int Color_Green = 0;

		// Token: 0x04001C0C RID: 7180
		public const int Color_Purple = 1;

		// Token: 0x04001C0D RID: 7181
		public const int Color_Orange = 2;

		// Token: 0x04001C0E RID: 7182
		public const int Color_Yellow = 3;

		// Token: 0x04001C0F RID: 7183
		public const int Color_Red = 4;

		// Token: 0x04001C10 RID: 7184
		public const int Color_Blue = 5;

		// Token: 0x04001C11 RID: 7185
		public const int Color_Teal = 6;

		// Token: 0x04001C12 RID: 7186
		public const float DefaultShakeForce = 0.07363108f;

		// Token: 0x04001C13 RID: 7187
		public string rewardDescription;

		// Token: 0x04001C14 RID: 7188
		public List<BundleIngredientDescription> ingredients;

		// Token: 0x04001C15 RID: 7189
		public int bundleColor;

		// Token: 0x04001C16 RID: 7190
		public int numberOfIngredientSlots;

		// Token: 0x04001C17 RID: 7191
		public int bundleIndex;

		// Token: 0x04001C18 RID: 7192
		public int completionTimer;

		// Token: 0x04001C19 RID: 7193
		public bool complete;

		// Token: 0x04001C1A RID: 7194
		public bool depositsAllowed = true;

		// Token: 0x04001C1B RID: 7195
		public Texture2D bundleTextureOverride;

		// Token: 0x04001C1C RID: 7196
		public int bundleTextureIndexOverride = -1;

		// Token: 0x04001C1D RID: 7197
		public TemporaryAnimatedSprite sprite;

		// Token: 0x04001C1E RID: 7198
		private float maxShake;

		// Token: 0x04001C1F RID: 7199
		private bool shakeLeft;
	}
}

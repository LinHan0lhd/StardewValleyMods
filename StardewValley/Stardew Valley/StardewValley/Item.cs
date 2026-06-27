using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Delegates;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Tools;

namespace StardewValley
{
	// Token: 0x020000B9 RID: 185
	[XmlInclude(typeof(Boots))]
	[XmlInclude(typeof(Clothing))]
	[XmlInclude(typeof(Hat))]
	[XmlInclude(typeof(ModDataDictionary))]
	[XmlInclude(typeof(Object))]
	[XmlInclude(typeof(Ring))]
	[XmlInclude(typeof(SpecialItem))]
	[XmlInclude(typeof(Tool))]
	[InstanceStatics]
	[NotImplicitNetField]
	public abstract class Item : IComparable, INetObject<NetFields>, ISalable, IHaveItemTypeId, IHaveModData
	{
		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000CCE RID: 3278 RVA: 0x0008F5AC File Offset: 0x0008D7AC
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000CCF RID: 3279 RVA: 0x0008F5B4 File Offset: 0x0008D7B4
		// (set) Token: 0x06000CD0 RID: 3280 RVA: 0x0008F5C1 File Offset: 0x0008D7C1
		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return this.modData.GetForSerialization();
			}
			set
			{
				this.modData.SetFromSerialization(value);
			}
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000CD1 RID: 3281 RVA: 0x0008F5CF File Offset: 0x0008D7CF
		// (set) Token: 0x06000CD2 RID: 3282 RVA: 0x0008F5DC File Offset: 0x0008D7DC
		public int SpecialVariable
		{
			get
			{
				return this.specialVariable.Value;
			}
			set
			{
				this.specialVariable.Set(value);
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000CD3 RID: 3283 RVA: 0x0008F5EA File Offset: 0x0008D7EA
		// (set) Token: 0x06000CD4 RID: 3284 RVA: 0x0008F5F7 File Offset: 0x0008D7F7
		[XmlIgnore]
		public int Category
		{
			get
			{
				return this.category.Value;
			}
			set
			{
				this.category.Set(value);
			}
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000CD5 RID: 3285 RVA: 0x0008F605 File Offset: 0x0008D805
		// (set) Token: 0x06000CD6 RID: 3286 RVA: 0x0008F612 File Offset: 0x0008D812
		[XmlIgnore]
		public bool HasBeenInInventory
		{
			get
			{
				return this.hasbeenInInventory.Value;
			}
			set
			{
				this.hasbeenInInventory.Set(value);
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000CD7 RID: 3287 RVA: 0x0008F620 File Offset: 0x0008D820
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("Item");

		// Token: 0x06000CD8 RID: 3288 RVA: 0x0008F628 File Offset: 0x0008D828
		public bool IsInfiniteStock()
		{
			return this.isLostItem;
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x0008F635 File Offset: 0x0008D835
		public void MarkContextTagsDirty()
		{
			this._contextTagsDirty = true;
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x0008F63E File Offset: 0x0008D83E
		public HashSet<string> GetContextTags()
		{
			if (this._contextTags == null || this._contextTagsDirty)
			{
				this._GenerateContextTags();
			}
			return this._contextTags;
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x0008F65C File Offset: 0x0008D85C
		public bool HasContextTag(string tag)
		{
			return ItemContextTagManager.DoesTagMatch(tag, this.GetContextTags());
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x0008F66A File Offset: 0x0008D86A
		protected void _GenerateContextTags()
		{
			this._contextTagsDirty = false;
			this._contextTags = new HashSet<string>(ItemContextTagManager.GetBaseContextTags(this.QualifiedItemId), StringComparer.OrdinalIgnoreCase);
			this._PopulateContextTags(this._contextTags);
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x0008F69C File Offset: 0x0008D89C
		protected virtual void _PopulateContextTags(HashSet<string> tags)
		{
			switch (this.quality.Value)
			{
			case 0:
				tags.Add("quality_none");
				return;
			case 1:
				tags.Add("quality_silver");
				return;
			case 2:
				tags.Add("quality_gold");
				return;
			case 3:
				break;
			case 4:
				tags.Add("quality_iridium");
				break;
			default:
				return;
			}
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x0008F704 File Offset: 0x0008D904
		protected Item()
		{
			this.initNetFields();
			this.parentSheetIndex.Value = -1;
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000CDF RID: 3295 RVA: 0x0008F7AE File Offset: 0x0008D9AE
		// (set) Token: 0x06000CE0 RID: 3296 RVA: 0x0008F7BB File Offset: 0x0008D9BB
		[XmlIgnore]
		public int ParentSheetIndex
		{
			get
			{
				return this.parentSheetIndex.Value;
			}
			set
			{
				this.parentSheetIndex.Value = value;
			}
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000CE1 RID: 3297
		public abstract string TypeDefinitionId { get; }

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000CE2 RID: 3298 RVA: 0x0008F7C9 File Offset: 0x0008D9C9
		// (set) Token: 0x06000CE3 RID: 3299 RVA: 0x0008F7E9 File Offset: 0x0008D9E9
		[XmlIgnore]
		public string ItemId
		{
			get
			{
				if (this.itemId.Value == null)
				{
					this.MigrateLegacyItemId();
				}
				return this.itemId.Value;
			}
			set
			{
				this.itemId.Value = value;
				this._qualifiedItemId = null;
			}
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000CE4 RID: 3300 RVA: 0x0008F7FE File Offset: 0x0008D9FE
		[XmlIgnore]
		public string QualifiedItemId
		{
			get
			{
				if (this._qualifiedItemId == null)
				{
					this._qualifiedItemId = this.TypeDefinitionId + this.ItemId;
				}
				return this._qualifiedItemId;
			}
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x0008F825 File Offset: 0x0008DA25
		public virtual bool ShouldSerializeparentSheetIndex()
		{
			return this.parentSheetIndex.Value != -1;
		}

		// Token: 0x06000CE6 RID: 3302 RVA: 0x0008F838 File Offset: 0x0008DA38
		protected virtual void MigrateLegacyItemId()
		{
			this.itemId.Value = this.ParentSheetIndex.ToString();
		}

		// Token: 0x06000CE7 RID: 3303 RVA: 0x0008F860 File Offset: 0x0008DA60
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.specialVariable, "specialVariable").AddField(this.category, "category").AddField(this.netName, "netName").AddField(this.parentSheetIndex, "parentSheetIndex").AddField(this.hasbeenInInventory, "hasbeenInInventory").AddField(this.itemId, "itemId").AddField(this.stack, "stack").AddField(this.quality, "quality").AddField(this.isRecipe, "isRecipe").AddField(this.modData, "modData");
			this.itemId.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._qualifiedItemId = null;
				this.MarkContextTagsDirty();
			};
			this.netName.fieldChangeVisibleEvent += delegate(NetString field, string oldValue, string newValue)
			{
				if (newValue == null)
				{
					field.Value = "Error Item";
				}
			};
			this.quality.fieldChangeVisibleEvent += delegate(NetInt <p0>, int <p1>, int <p2>)
			{
				this.MarkContextTagsDirty();
			};
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x0008F972 File Offset: 0x0008DB72
		public void ResetParentSheetIndex()
		{
			this.ParentSheetIndex = ItemRegistry.GetDataOrErrorItem(this.QualifiedItemId).SpriteIndex;
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x0008F98C File Offset: 0x0008DB8C
		protected string ValidateUnqualifiedItemId(string id)
		{
			if (ItemRegistry.IsQualifiedItemId(id))
			{
				string qualifier = this.TypeDefinitionId;
				if (id.StartsWith(qualifier))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 3);
					defaultInterpolatedStringHandler.AppendLiteral("The ");
					defaultInterpolatedStringHandler.AppendFormatted(base.GetType().FullName);
					defaultInterpolatedStringHandler.AppendLiteral(" constructor was called with qualified item ID '");
					defaultInterpolatedStringHandler.AppendFormatted(id);
					defaultInterpolatedStringHandler.AppendLiteral("'. The '");
					defaultInterpolatedStringHandler.AppendFormatted(qualifier);
					defaultInterpolatedStringHandler.AppendLiteral("' prefix will be removed automatically.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					id = id.Substring(qualifier.Length).TrimStart();
				}
				else
				{
					IGameLogger log2 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(96, 2);
					defaultInterpolatedStringHandler.AppendLiteral("The ");
					defaultInterpolatedStringHandler.AppendFormatted(base.GetType().FullName);
					defaultInterpolatedStringHandler.AppendLiteral(" constructor was called with qualified item ID '");
					defaultInterpolatedStringHandler.AppendFormatted(id);
					defaultInterpolatedStringHandler.AppendLiteral("'. This will likely result in an error item.");
					log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			return id;
		}

		// Token: 0x06000CEA RID: 3306 RVA: 0x0008FA93 File Offset: 0x0008DC93
		public string GetItemTypeId()
		{
			return this.TypeDefinitionId;
		}

		// Token: 0x06000CEB RID: 3307 RVA: 0x0008FA9C File Offset: 0x0008DC9C
		public virtual void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			if (overrideText != null && overrideText.Length != 0 && (overrideText.Length != 1 || overrideText[0] != ' '))
			{
				spriteBatch.DrawString(font, overrideText, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
				spriteBatch.DrawString(font, overrideText, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
				spriteBatch.DrawString(font, overrideText, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
				spriteBatch.DrawString(font, overrideText, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Game1.textColor * 0.9f * alpha);
				y += (int)font.MeasureString(overrideText).Y + 4;
			}
		}

		// Token: 0x06000CEC RID: 3308 RVA: 0x0008FBD5 File Offset: 0x0008DDD5
		public virtual void ModifyItemBuffs(BuffEffects buffs)
		{
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x0008FBD7 File Offset: 0x0008DDD7
		public virtual Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			return Point.Zero;
		}

		// Token: 0x06000CEE RID: 3310 RVA: 0x0008FBDE File Offset: 0x0008DDDE
		public bool ShouldDrawIcon()
		{
			return true;
		}

		// Token: 0x06000CEF RID: 3311
		public abstract void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow);

		// Token: 0x06000CF0 RID: 3312 RVA: 0x0008FBE4 File Offset: 0x0008DDE4
		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber)
		{
			this.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, Color.White, true);
		}

		// Token: 0x06000CF1 RID: 3313 RVA: 0x0008FC08 File Offset: 0x0008DE08
		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
		{
			this.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, StackDrawType.Draw, Color.White, true);
		}

		// Token: 0x06000CF2 RID: 3314 RVA: 0x0008FC2C File Offset: 0x0008DE2C
		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize)
		{
			this.drawInMenu(spriteBatch, location, scaleSize, 1f, 0.9f, StackDrawType.Draw, Color.White, true);
		}

		// Token: 0x06000CF3 RID: 3315
		public abstract int maximumStackSize();

		// Token: 0x06000CF4 RID: 3316 RVA: 0x0008FC53 File Offset: 0x0008DE53
		public void AdjustMenuDrawForRecipes(ref float transparency, ref float scale)
		{
			if (this.isRecipe.Value)
			{
				transparency = 0.5f;
				scale *= 0.75f;
			}
		}

		// Token: 0x06000CF5 RID: 3317 RVA: 0x0008FC74 File Offset: 0x0008DE74
		public virtual void DrawMenuIcons(SpriteBatch sb, Vector2 location, float scale_size, float transparency, float layer_depth, StackDrawType drawStackNumber, Color color)
		{
			int drawnStack = this.Stack;
			bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && drawnStack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scale_size > 0.3 && drawnStack != int.MaxValue;
			if (this.IsRecipe)
			{
				shouldDrawStackNumber = false;
			}
			if (shouldDrawStackNumber)
			{
				Utility.drawTinyDigits(drawnStack, sb, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(drawnStack, 3f * scale_size)) + 3f * scale_size, 64f - 18f * scale_size + 1f), 3f * scale_size, Math.Min(1f, layer_depth + 1E-06f), color);
			}
			if (drawStackNumber != StackDrawType.Hide && this.quality.Value > 0)
			{
				Rectangle qualityRect = (this.quality.Value < 4) ? new Rectangle(338 + (this.quality.Value - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8);
				Texture2D qualitySheet = Game1.mouseCursors;
				float yOffset = (this.quality.Value < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * 3.141592653589793 / 512.0) + 1f) * 0.05f);
				sb.Draw(qualitySheet, location + new Vector2(12f, 52f + yOffset), new Rectangle?(qualityRect), color * transparency, 0f, new Vector2(4f, 4f), 3f * scale_size * (1f + yOffset), SpriteEffects.None, layer_depth);
			}
			else if (drawStackNumber != StackDrawType.Hide && this.Category == -102 && Game1.player.stats.Get(this.itemId.Value) > 0U)
			{
				sb.Draw(Game1.mouseCursors_1_6, location + new Vector2(12f, 44f), new Rectangle?(new Rectangle(244, 271, 9, 11)), color * transparency, 0f, new Vector2(4f, 4f), 3f * scale_size * 1f, SpriteEffects.None, layer_depth);
			}
			this.DrawIconBar(sb, location, scale_size, transparency, layer_depth, drawStackNumber, color);
			if (this.isRecipe.Value)
			{
				sb.Draw(Game1.objectSpriteSheet, location + new Vector2(16f, 16f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16)), color, 0f, Vector2.Zero, 3f, SpriteEffects.None, layer_depth + 0.0001f);
			}
		}

		// Token: 0x06000CF6 RID: 3318 RVA: 0x0008FF33 File Offset: 0x0008E133
		public virtual void DrawIconBar(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color)
		{
		}

		// Token: 0x06000CF7 RID: 3319 RVA: 0x0008FF38 File Offset: 0x0008E138
		public virtual int addToStack(Item otherStack)
		{
			int maxStack = this.maximumStackSize();
			if (maxStack == 1)
			{
				return otherStack.Stack;
			}
			this.stack.Value += otherStack.Stack;
			Object obj = this as Object;
			if (obj != null)
			{
				Object otherObject = otherStack as Object;
				if (otherObject != null && obj.IsSpawnedObject && !otherObject.IsSpawnedObject)
				{
					obj.IsSpawnedObject = false;
				}
			}
			if (this.stack.Value > maxStack)
			{
				int result = this.stack.Value - maxStack;
				this.stack.Value = maxStack;
				return result;
			}
			return 0;
		}

		// Token: 0x06000CF8 RID: 3320
		public abstract string getDescription();

		// Token: 0x06000CF9 RID: 3321
		public abstract bool isPlaceable();

		// Token: 0x06000CFA RID: 3322 RVA: 0x0008FFC3 File Offset: 0x0008E1C3
		public virtual int sellToStorePrice(long specificPlayerID = -1L)
		{
			return this.salePrice(false) / 2;
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x0008FFCE File Offset: 0x0008E1CE
		public virtual int salePrice(bool ignoreProfitMargins = false)
		{
			return -1;
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x0008FFD1 File Offset: 0x0008E1D1
		public virtual bool appliesProfitMargins()
		{
			return false;
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x0008FFD4 File Offset: 0x0008E1D4
		public virtual bool CanBeLostOnDeath()
		{
			return this.canBeTrashed() && !this.HasContextTag("prevent_loss_on_death");
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x0008FFF0 File Offset: 0x0008E1F0
		public virtual bool canBeTrashed()
		{
			if (this.specialItem)
			{
				return false;
			}
			MeleeWeapon weapon = this as MeleeWeapon;
			if (weapon == null)
			{
				return this is FishingRod || this is Pan || this is Slingshot || !(this is Tool);
			}
			return !weapon.isScythe();
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x00090048 File Offset: 0x0008E248
		public virtual bool actionWhenPurchased(string shopId)
		{
			if (this.isLostItem)
			{
				Game1.player.itemsLostLastDeath.Clear();
				this.isLostItem = false;
				Game1.player.recoveredItem = this;
				Game1.player.mailReceived.Remove("MarlonRecovery");
				Game1.addMailForTomorrow("MarlonRecovery", false, false);
				Game1.playSound("newArtifact", null);
				Game1.exitActiveMenu();
				bool use_plural = this.Stack > 1;
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon", true, false), use_plural ? "Strings\\StringsFromCSFiles:ItemRecovery_Engaged_Stack" : "Strings\\StringsFromCSFiles:ItemRecovery_Engaged", new object[]
				{
					Lexicon.makePlural(this.DisplayName, !use_plural)
				});
				return true;
			}
			return false;
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x00090100 File Offset: 0x0008E300
		public bool LearnRecipe(Farmer player = null)
		{
			if (player == null)
			{
				player = Game1.player;
			}
			return ((this.Category == -7) ? player.cookingRecipes : player.craftingRecipes).TryAdd(this.BaseName, 0);
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x00090130 File Offset: 0x0008E330
		public virtual bool CanBuyItem(Farmer who)
		{
			return Game1.player.couldInventoryAcceptThisItem(this);
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x0009013D File Offset: 0x0008E33D
		public virtual bool canBeDropped()
		{
			return true;
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x00090140 File Offset: 0x0008E340
		public virtual bool canBeShipped()
		{
			return false;
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x00090143 File Offset: 0x0008E343
		public virtual void onDetachedFromParent()
		{
			this.NetFields.Parent = null;
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x00090151 File Offset: 0x0008E351
		public virtual void onEquip(Farmer who)
		{
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x00090153 File Offset: 0x0008E353
		public virtual void onUnequip(Farmer who)
		{
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x00090155 File Offset: 0x0008E355
		public virtual void actionWhenBeingHeld(Farmer who)
		{
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x00090157 File Offset: 0x0008E357
		public virtual void actionWhenStopBeingHeld(Farmer who)
		{
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x00090159 File Offset: 0x0008E359
		public int getRemainingStackSpace()
		{
			return this.maximumStackSize() - this.Stack;
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x00090168 File Offset: 0x0008E368
		[NullableContext(2)]
		public virtual Item ConsumeStack(int amount)
		{
			if (amount == 0)
			{
				return this;
			}
			if (this.Stack - amount <= 0)
			{
				return null;
			}
			this.Stack -= amount;
			return this;
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x0009018B File Offset: 0x0008E38B
		public virtual int healthRecoveredOnConsumption()
		{
			return 0;
		}

		// Token: 0x06000D0C RID: 3340 RVA: 0x0009018E File Offset: 0x0008E38E
		public virtual int staminaRecoveredOnConsumption()
		{
			return 0;
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x00090191 File Offset: 0x0008E391
		public virtual string getHoverBoxText(Item hoveredItem)
		{
			return null;
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x00090194 File Offset: 0x0008E394
		public virtual bool canBeGivenAsGift()
		{
			return false;
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x00090197 File Offset: 0x0008E397
		public virtual void drawAttachments(SpriteBatch b, int x, int y)
		{
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x00090199 File Offset: 0x0008E399
		public virtual bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
		{
			return false;
		}

		// Token: 0x06000D11 RID: 3345 RVA: 0x0009019C File Offset: 0x0008E39C
		public virtual int attachmentSlots()
		{
			return 0;
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x0009019F File Offset: 0x0008E39F
		public virtual string getCategoryName()
		{
			return Object.GetCategoryDisplayName(this.Category);
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x000901AC File Offset: 0x0008E3AC
		public virtual Color getCategoryColor()
		{
			return Object.GetCategoryColor(this.Category);
		}

		// Token: 0x06000D14 RID: 3348 RVA: 0x000901BC File Offset: 0x0008E3BC
		public virtual bool canStackWith(ISalable other)
		{
			Item otherItem = other as Item;
			if (otherItem == null || other.GetType() != base.GetType())
			{
				return false;
			}
			ColoredObject coloredObj = this as ColoredObject;
			if (coloredObj != null)
			{
				ColoredObject otherColoredObj = other as ColoredObject;
				if (otherColoredObj != null && !coloredObj.color.Value.Equals(otherColoredObj.color.Value))
				{
					return false;
				}
			}
			if (this.maximumStackSize() <= 1 || other.maximumStackSize() <= 1)
			{
				return false;
			}
			Object obj = this as Object;
			if (obj != null)
			{
				Object otherObj = other as Object;
				if (otherObj != null && otherObj.orderData.Value != obj.orderData.Value)
				{
					return false;
				}
			}
			return this.quality.Value == otherItem.quality.Value && !(this.QualifiedItemId != otherItem.QualifiedItemId) && this.Name.Equals(other.Name);
		}

		// Token: 0x06000D15 RID: 3349 RVA: 0x000902AE File Offset: 0x0008E4AE
		public virtual string checkForSpecialItemHoldUpMeessage()
		{
			return null;
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000D16 RID: 3350
		public abstract string DisplayName { get; }

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000D17 RID: 3351 RVA: 0x000902B1 File Offset: 0x0008E4B1
		// (set) Token: 0x06000D18 RID: 3352 RVA: 0x000902BE File Offset: 0x0008E4BE
		[XmlIgnore]
		public virtual string Name
		{
			get
			{
				return this.netName.Value;
			}
			set
			{
				this.netName.Value = value;
			}
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06000D19 RID: 3353 RVA: 0x000902CC File Offset: 0x0008E4CC
		[XmlIgnore]
		public virtual string BaseName
		{
			get
			{
				return this.Name;
			}
		}

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06000D1A RID: 3354 RVA: 0x000902D4 File Offset: 0x0008E4D4
		// (set) Token: 0x06000D1B RID: 3355 RVA: 0x000902E7 File Offset: 0x0008E4E7
		[XmlIgnore]
		public virtual int Stack
		{
			get
			{
				return Math.Max(0, this.stack.Value);
			}
			set
			{
				if (Game1.gameMode != 3)
				{
					this.stack.Value = value;
					return;
				}
				this.stack.Value = Math.Min(Math.Max(0, value), (value == int.MaxValue) ? value : this.maximumStackSize());
			}
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000D1C RID: 3356 RVA: 0x00090326 File Offset: 0x0008E526
		// (set) Token: 0x06000D1D RID: 3357 RVA: 0x00090333 File Offset: 0x0008E533
		[XmlIgnore]
		public int Quality
		{
			get
			{
				return this.quality.Value;
			}
			set
			{
				this.quality.Value = value;
			}
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000D1E RID: 3358 RVA: 0x00090341 File Offset: 0x0008E541
		// (set) Token: 0x06000D1F RID: 3359 RVA: 0x0009034E File Offset: 0x0008E54E
		[XmlIgnore]
		public bool IsRecipe
		{
			get
			{
				return this.isRecipe.Value;
			}
			set
			{
				this.isRecipe.Value = value;
			}
		}

		// Token: 0x06000D20 RID: 3360 RVA: 0x0009035C File Offset: 0x0008E55C
		public Item getOne()
		{
			Item oneNew = this.GetOneNew();
			oneNew.GetOneCopyFrom(this);
			return oneNew;
		}

		// Token: 0x06000D21 RID: 3361
		protected abstract Item GetOneNew();

		// Token: 0x06000D22 RID: 3362 RVA: 0x0009036C File Offset: 0x0008E56C
		protected virtual void GetOneCopyFrom(Item source)
		{
			this.ItemId = source.ItemId;
			this.IsRecipe = source.isRecipe.Value;
			this.Quality = source.quality.Value;
			this.Stack = 1;
			this.HasBeenInInventory = source.HasBeenInInventory;
			this.SpecialVariable = source.SpecialVariable;
			Dictionary<string, object> dictionary = source.tempData;
			if (dictionary != null && dictionary.Count > 0)
			{
				foreach (KeyValuePair<string, object> pair in source.tempData)
				{
					this.SetTempData<object>(pair.Key, pair.Value);
				}
			}
			this.modData.Clear();
			foreach (string key in source.modData.Keys)
			{
				this.modData[key] = source.modData[key];
			}
		}

		// Token: 0x06000D23 RID: 3363 RVA: 0x0009049C File Offset: 0x0008E69C
		public void CopyFieldsFrom(Item source)
		{
			this.GetOneCopyFrom(source);
		}

		// Token: 0x06000D24 RID: 3364 RVA: 0x000904A5 File Offset: 0x0008E6A5
		public ISalable GetSalableInstance()
		{
			return this.getOne();
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x000904B0 File Offset: 0x0008E6B0
		public virtual int CompareTo(object other)
		{
			Item otherItem = other as Item;
			if (otherItem == null)
			{
				return 0;
			}
			if (otherItem.Category != this.Category)
			{
				return otherItem.getCategorySortValue() - this.getCategorySortValue();
			}
			string thisName = (this.Name == "") ? this.DisplayName : this.Name;
			string otherName = (otherItem.Name == "") ? otherItem.DisplayName : otherItem.Name;
			if (otherName != thisName)
			{
				Object curObj = this as Object;
				if (curObj != null)
				{
					Object otherObj = otherItem as Object;
					if (otherObj != null)
					{
						if (curObj.HasContextTag("use_reverse_name_for_sorting") || curObj is Trinket)
						{
							thisName = string.Join("", thisName.Split(' ', StringSplitOptions.None).Reverse<string>());
						}
						if (otherObj.HasContextTag("use_reverse_name_for_sorting") || otherObj is Trinket)
						{
							otherName = string.Join("", otherName.Split(' ', StringSplitOptions.None).Reverse<string>());
						}
						return string.Compare(curObj.type.Value + thisName, otherObj.type.Value + otherName);
					}
				}
				return string.Compare(thisName, otherItem.Name);
			}
			if (otherItem.Quality != this.Quality)
			{
				return otherItem.Quality.CompareTo(this.Quality);
			}
			ColoredObject curColored = this as ColoredObject;
			if (curColored != null)
			{
				ColoredObject otherColored = otherItem as ColoredObject;
				if (otherColored != null && curColored.color.Value != otherColored.color.Value)
				{
					return otherColored.GetHue().CompareTo(curColored.GetHue());
				}
			}
			return this.Stack - otherItem.Stack;
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x00090664 File Offset: 0x0008E864
		public int getCategorySortValue()
		{
			if (this.Category == -100)
			{
				return -94;
			}
			return this.Category;
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x0009067C File Offset: 0x0008E87C
		protected virtual int getDescriptionWidth()
		{
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			int minimumSize;
			if (currentLanguageCode != LocalizedContentManager.LanguageCode.fr)
			{
				if (currentLanguageCode != LocalizedContentManager.LanguageCode.tr)
				{
					minimumSize = 272;
				}
				else
				{
					minimumSize = 336;
				}
			}
			else
			{
				minimumSize = 384;
			}
			return Math.Max(minimumSize, (int)Game1.dialogueFont.MeasureString((this.DisplayName == null) ? "" : this.DisplayName).X);
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x000906DB File Offset: 0x0008E8DB
		public void SetTempData<T>(string key, T value)
		{
			if (this.tempData == null)
			{
				this.tempData = new Dictionary<string, object>();
			}
			this.tempData[key] = value;
		}

		// Token: 0x06000D29 RID: 3369 RVA: 0x00090704 File Offset: 0x0008E904
		public bool TryGetTempData<T>(string key, out T value)
		{
			object rawValue;
			if (this.tempData == null || !this.tempData.TryGetValue(key, out rawValue))
			{
				value = default(T);
				return false;
			}
			if (rawValue == null)
			{
				value = default(T);
				return value == null;
			}
			if (rawValue is T)
			{
				T parsed = (T)((object)rawValue);
				value = parsed;
				return true;
			}
			value = default(T);
			return false;
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x0009076B File Offset: 0x0008E96B
		public virtual void FixStackSize()
		{
			this.stack.Value = Utility.Clamp(this.stack.Value, 1, this.maximumStackSize());
		}

		// Token: 0x06000D2B RID: 3371 RVA: 0x0009078F File Offset: 0x0008E98F
		public virtual void FixQuality()
		{
			this.quality.Value = Utility.Clamp(this.quality.Value, 0, 4);
			if (this.quality.Value == 3)
			{
				this.quality.Value = 4;
			}
		}

		// Token: 0x06000D2C RID: 3372 RVA: 0x000907C8 File Offset: 0x0008E9C8
		public virtual void resetState()
		{
		}

		// Token: 0x06000D2D RID: 3373 RVA: 0x000907CC File Offset: 0x0008E9CC
		public virtual bool HasEquipmentBuffs()
		{
			BuffEffects effects = new BuffEffects();
			this.AddEquipmentEffects(effects);
			return effects.HasAnyValue();
		}

		// Token: 0x06000D2E RID: 3374 RVA: 0x000907EC File Offset: 0x0008E9EC
		public virtual void AddEquipmentEffects(BuffEffects effects)
		{
		}

		// Token: 0x06000D2F RID: 3375 RVA: 0x000907EE File Offset: 0x0008E9EE
		public virtual IEnumerable<Buff> GetFoodOrDrinkBuffs()
		{
			return LegacyShims.EmptyArray<Buff>();
		}

		// Token: 0x06000D30 RID: 3376 RVA: 0x000907F8 File Offset: 0x0008E9F8
		public virtual string GenerateLightSourceId(Farmer heldBy)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<long>((heldBy != null) ? heldBy.UniqueMultiplayerID : -1L);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x00090864 File Offset: 0x0008EA64
		public virtual bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
		{
			return true;
		}

		// Token: 0x040008C5 RID: 2245
		public const string ErrorItemName = "Error Item";

		// Token: 0x040008C6 RID: 2246
		public bool isLostItem;

		// Token: 0x040008C7 RID: 2247
		private readonly NetInt specialVariable = new NetInt();

		// Token: 0x040008C8 RID: 2248
		[XmlElement("category")]
		public readonly NetInt category = new NetInt();

		// Token: 0x040008C9 RID: 2249
		[XmlElement("hasBeenInInventory")]
		public readonly NetBool hasbeenInInventory = new NetBool();

		// Token: 0x040008CA RID: 2250
		private HashSet<string> _contextTags;

		// Token: 0x040008CB RID: 2251
		protected bool _contextTagsDirty;

		// Token: 0x040008CC RID: 2252
		[XmlIgnore]
		public Dictionary<string, object> tempData;

		// Token: 0x040008CE RID: 2254
		[XmlIgnore]
		public string SetFlagOnPickup;

		// Token: 0x040008D0 RID: 2256
		[XmlElement("name")]
		public readonly NetString netName = new NetString("Error Item");

		// Token: 0x040008D1 RID: 2257
		[XmlElement("parentSheetIndex")]
		public readonly NetInt parentSheetIndex = new NetInt();

		// Token: 0x040008D2 RID: 2258
		[XmlElement("itemId")]
		public NetString itemId = new NetString();

		// Token: 0x040008D3 RID: 2259
		[XmlIgnore]
		protected string _qualifiedItemId;

		// Token: 0x040008D4 RID: 2260
		public bool specialItem;

		// Token: 0x040008D5 RID: 2261
		[XmlElement("isRecipe")]
		public readonly NetBool isRecipe = new NetBool();

		// Token: 0x040008D6 RID: 2262
		[XmlElement("quality")]
		public readonly NetInt quality = new NetInt(0);

		// Token: 0x040008D7 RID: 2263
		[XmlElement("stack")]
		public readonly NetInt stack = new NetInt(1);
	}
}

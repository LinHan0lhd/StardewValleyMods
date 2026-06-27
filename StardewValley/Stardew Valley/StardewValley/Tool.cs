using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Buffs;
using StardewValley.Enchantments;
using StardewValley.GameData.Tools;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley
{
	// Token: 0x0200010C RID: 268
	[XmlInclude(typeof(Axe))]
	[XmlInclude(typeof(ErrorTool))]
	[XmlInclude(typeof(FishingRod))]
	[XmlInclude(typeof(GenericTool))]
	[XmlInclude(typeof(Hoe))]
	[XmlInclude(typeof(MeleeWeapon))]
	[XmlInclude(typeof(MilkPail))]
	[XmlInclude(typeof(Pan))]
	[XmlInclude(typeof(Pickaxe))]
	[XmlInclude(typeof(Shears))]
	[XmlInclude(typeof(Slingshot))]
	[XmlInclude(typeof(Wand))]
	[XmlInclude(typeof(WateringCan))]
	public abstract class Tool : Item
	{
		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06001589 RID: 5513 RVA: 0x000FEA93 File Offset: 0x000FCC93
		// (set) Token: 0x0600158A RID: 5514 RVA: 0x000FEAAF File Offset: 0x000FCCAF
		[XmlIgnore]
		public string description
		{
			get
			{
				if (this._description == null)
				{
					this._description = this.loadDescription();
				}
				return this._description;
			}
			set
			{
				this._description = value;
			}
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x0600158B RID: 5515 RVA: 0x000FEAB8 File Offset: 0x000FCCB8
		public override string TypeDefinitionId { get; } = "(T)";

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x0600158C RID: 5516 RVA: 0x000FEAC0 File Offset: 0x000FCCC0
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				return this.loadDisplayName();
			}
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x0600158D RID: 5517 RVA: 0x000FEAC8 File Offset: 0x000FCCC8
		public string Description
		{
			get
			{
				return this.description;
			}
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x0600158E RID: 5518 RVA: 0x000FEAD0 File Offset: 0x000FCCD0
		// (set) Token: 0x0600158F RID: 5519 RVA: 0x000FEADD File Offset: 0x000FCCDD
		[XmlIgnore]
		public int CurrentParentTileIndex
		{
			get
			{
				return this.currentParentTileIndex.Value;
			}
			set
			{
				this.currentParentTileIndex.Set(value);
			}
		}

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06001590 RID: 5520 RVA: 0x000FEAEB File Offset: 0x000FCCEB
		// (set) Token: 0x06001591 RID: 5521 RVA: 0x000FEAF8 File Offset: 0x000FCCF8
		public int InitialParentTileIndex
		{
			get
			{
				return this.initialParentTileIndex.Value;
			}
			set
			{
				this.initialParentTileIndex.Set(value);
			}
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06001592 RID: 5522 RVA: 0x000FEB06 File Offset: 0x000FCD06
		// (set) Token: 0x06001593 RID: 5523 RVA: 0x000FEB13 File Offset: 0x000FCD13
		public int IndexOfMenuItemView
		{
			get
			{
				return this.indexOfMenuItemView.Value;
			}
			set
			{
				this.indexOfMenuItemView.Set(value);
			}
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06001594 RID: 5524 RVA: 0x000FEB21 File Offset: 0x000FCD21
		// (set) Token: 0x06001595 RID: 5525 RVA: 0x000FEB2E File Offset: 0x000FCD2E
		[XmlIgnore]
		public int UpgradeLevel
		{
			get
			{
				return this.upgradeLevel.Value;
			}
			set
			{
				this.upgradeLevel.Value = value;
			}
		}

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x06001596 RID: 5526 RVA: 0x000FEB3C File Offset: 0x000FCD3C
		// (set) Token: 0x06001597 RID: 5527 RVA: 0x000FEB44 File Offset: 0x000FCD44
		[XmlIgnore]
		public int AttachmentSlotsCount
		{
			get
			{
				return this.attachmentSlots();
			}
			set
			{
				this.numAttachmentSlots.Value = value;
				this.attachments.SetCount(value);
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06001598 RID: 5528 RVA: 0x000FEB5E File Offset: 0x000FCD5E
		// (set) Token: 0x06001599 RID: 5529 RVA: 0x000FEB6B File Offset: 0x000FCD6B
		public bool InstantUse
		{
			get
			{
				return this.instantUse.Value;
			}
			set
			{
				this.instantUse.Value = value;
			}
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x0600159A RID: 5530 RVA: 0x000FEB79 File Offset: 0x000FCD79
		// (set) Token: 0x0600159B RID: 5531 RVA: 0x000FEB86 File Offset: 0x000FCD86
		public bool IsEfficient
		{
			get
			{
				return this.isEfficient.Value;
			}
			set
			{
				this.isEfficient.Value = value;
			}
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x0600159C RID: 5532 RVA: 0x000FEB94 File Offset: 0x000FCD94
		// (set) Token: 0x0600159D RID: 5533 RVA: 0x000FEBA1 File Offset: 0x000FCDA1
		public float AnimationSpeedModifier
		{
			get
			{
				return this.animationSpeedModifier.Value;
			}
			set
			{
				this.animationSpeedModifier.Value = value;
			}
		}

		// Token: 0x0600159E RID: 5534 RVA: 0x000FEBB0 File Offset: 0x000FCDB0
		public Tool()
		{
			this.initNetFields();
			base.Category = -99;
		}

		// Token: 0x0600159F RID: 5535 RVA: 0x000FEC78 File Offset: 0x000FCE78
		public Tool(string name, int upgradeLevel, int initialParentTileIndex, int indexOfMenuItemView, bool stackable, int numAttachmentSlots = 0) : this()
		{
			this.Name = (name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName);
			this.SetSpriteIndex(initialParentTileIndex);
			this.IndexOfMenuItemView = indexOfMenuItemView;
			this.AttachmentSlotsCount = Math.Max(0, numAttachmentSlots);
			base.Category = -99;
			this.UpgradeLevel = upgradeLevel;
		}

		// Token: 0x060015A0 RID: 5536 RVA: 0x000FECD2 File Offset: 0x000FCED2
		public virtual void SetSpriteIndex(int spriteIndex)
		{
			this.InitialParentTileIndex = spriteIndex;
			this.IndexOfMenuItemView = spriteIndex;
			this.CurrentParentTileIndex = spriteIndex;
		}

		// Token: 0x060015A1 RID: 5537 RVA: 0x000FECEC File Offset: 0x000FCEEC
		protected new virtual void initNetFields()
		{
			base.NetFields.SetOwner(this).AddField(this.initialParentTileIndex, "initialParentTileIndex").AddField(this.currentParentTileIndex, "currentParentTileIndex").AddField(this.indexOfMenuItemView, "indexOfMenuItemView").AddField(this.instantUse, "instantUse").AddField(this.upgradeLevel, "upgradeLevel").AddField(this.numAttachmentSlots, "numAttachmentSlots").AddField(this.attachments, "attachments").AddField(this.enchantments, "enchantments").AddField(this.isEfficient, "isEfficient").AddField(this.animationSpeedModifier, "animationSpeedModifier").AddField(this.previousEnchantments, "previousEnchantments");
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x000FEDB6 File Offset: 0x000FCFB6
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = base.GetType().Name;
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x000FEDC9 File Offset: 0x000FCFC9
		protected virtual string loadDisplayName()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x000FEDDB File Offset: 0x000FCFDB
		protected virtual string loadDescription()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x000FEDED File Offset: 0x000FCFED
		public override bool CanBeLostOnDeath()
		{
			if (base.CanBeLostOnDeath())
			{
				ToolData toolData = this.GetToolData();
				return toolData == null || toolData.CanBeLostOnDeath;
			}
			return false;
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x000FEE0A File Offset: 0x000FD00A
		public override string getCategoryName()
		{
			return Object.GetCategoryDisplayName(-99);
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x000FEE14 File Offset: 0x000FD014
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Tool fromTool = source as Tool;
			if (fromTool != null)
			{
				this.SetSpriteIndex(fromTool.InitialParentTileIndex);
				this.Name = source.Name;
				this.CurrentParentTileIndex = fromTool.CurrentParentTileIndex;
				this.IndexOfMenuItemView = fromTool.IndexOfMenuItemView;
				this.InstantUse = fromTool.InstantUse;
				this.IsEfficient = fromTool.IsEfficient;
				this.AnimationSpeedModifier = fromTool.AnimationSpeedModifier;
				this.UpgradeLevel = fromTool.UpgradeLevel;
				this.AttachmentSlotsCount = fromTool.AttachmentSlotsCount;
				this.CopyEnchantments(fromTool, this);
			}
		}

		// Token: 0x060015A8 RID: 5544 RVA: 0x000FEEA6 File Offset: 0x000FD0A6
		public virtual void UpgradeFrom(Tool other)
		{
			this.CopyEnchantments(other, this);
		}

		// Token: 0x060015A9 RID: 5545 RVA: 0x000FEEB0 File Offset: 0x000FD0B0
		public override Color getCategoryColor()
		{
			return Color.DarkSlateGray;
		}

		// Token: 0x060015AA RID: 5546 RVA: 0x000FEEB7 File Offset: 0x000FD0B7
		public ToolData GetToolData()
		{
			ParsedItemData data = ItemRegistry.GetData(base.QualifiedItemId);
			return ((data != null) ? data.RawData : null) as ToolData;
		}

		// Token: 0x060015AB RID: 5547 RVA: 0x000FEED8 File Offset: 0x000FD0D8
		public virtual void draw(SpriteBatch b)
		{
			Farmer farmer = this.lastUser;
			if (farmer != null && farmer.toolPower.Value > 0 && this.lastUser.canReleaseTool && this.lastUser.IsLocalPlayer)
			{
				foreach (Vector2 v in this.tilesAffected(this.lastUser.GetToolLocation(false) / 64f, this.lastUser.toolPower.Value, this.lastUser))
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)v.X * 64), (float)((int)v.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
				}
			}
		}

		// Token: 0x060015AC RID: 5548 RVA: 0x000FEFEC File Offset: 0x000FD1EC
		public override void drawAttachments(SpriteBatch b, int x, int y)
		{
			y += ((this.enchantments.Count > 0) ? 8 : 4);
			for (int slot = 0; slot < this.AttachmentSlotsCount; slot++)
			{
				this.DrawAttachmentSlot(slot, b, x, y + slot * 68);
			}
		}

		// Token: 0x060015AD RID: 5549 RVA: 0x000FF030 File Offset: 0x000FD230
		protected virtual void DrawAttachmentSlot(int slot, SpriteBatch b, int x, int y)
		{
			Vector2 pixel = new Vector2((float)x, (float)y);
			Texture2D texture;
			Rectangle sourceRect;
			this.GetAttachmentSlotSprite(slot, out texture, out sourceRect);
			b.Draw(texture, pixel, new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
			Object @object = this.attachments[slot];
			if (@object == null)
			{
				return;
			}
			@object.drawInMenu(b, pixel, 1f);
		}

		// Token: 0x060015AE RID: 5550 RVA: 0x000FF099 File Offset: 0x000FD299
		protected virtual void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
		{
			texture = Game1.menuTexture;
			sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1);
		}

		// Token: 0x060015AF RID: 5551 RVA: 0x000FF0B8 File Offset: 0x000FD2B8
		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
			foreach (BaseEnchantment enchantment in this.enchantments)
			{
				if (enchantment.ShouldBeDisplayed())
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors2, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(spriteBatch, BaseEnchantment.hideEnchantmentName ? "???" : enchantment.GetDisplayName(), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), new Color(120, 0, 210) * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
		}

		// Token: 0x060015B0 RID: 5552 RVA: 0x000FF1F8 File Offset: 0x000FD3F8
		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			Point dimensions = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
			dimensions.Y = startingHeight;
			using (NetList<BaseEnchantment, NetRef<BaseEnchantment>>.Enumerator enumerator = this.enchantments.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ShouldBeDisplayed())
					{
						dimensions.Y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					}
				}
			}
			return dimensions;
		}

		// Token: 0x060015B1 RID: 5553 RVA: 0x000FF28C File Offset: 0x000FD48C
		public virtual void tickUpdate(GameTime time, Farmer who)
		{
		}

		// Token: 0x060015B2 RID: 5554 RVA: 0x000FF28E File Offset: 0x000FD48E
		public virtual bool isHeavyHitter()
		{
			return this is MeleeWeapon || this is Hoe || this is Axe || this is Pickaxe;
		}

		// Token: 0x060015B3 RID: 5555 RVA: 0x000FF2B3 File Offset: 0x000FD4B3
		public virtual bool isScythe()
		{
			return false;
		}

		// Token: 0x060015B4 RID: 5556 RVA: 0x000FF2B8 File Offset: 0x000FD4B8
		public virtual void Update(int direction, int farmerMotionFrame, Farmer who)
		{
			int offset = 0;
			if (!(this is WateringCan))
			{
				if (!(this is FishingRod))
				{
					switch (direction)
					{
					case 0:
						offset = 3;
						break;
					case 1:
						offset = 2;
						break;
					case 3:
						offset = 2;
						break;
					}
				}
				else
				{
					switch (direction)
					{
					case 0:
						offset = 3;
						break;
					case 1:
						offset = 0;
						break;
					case 3:
						offset = 0;
						break;
					}
				}
			}
			else
			{
				switch (direction)
				{
				case 0:
					offset = 4;
					break;
				case 1:
					offset = 2;
					break;
				case 2:
					offset = 0;
					break;
				case 3:
					offset = 2;
					break;
				}
			}
			if (base.QualifiedItemId != "(T)WateringCan")
			{
				if (farmerMotionFrame < 1)
				{
					this.CurrentParentTileIndex = this.InitialParentTileIndex;
				}
				else if (who.FacingDirection == 0 || (who.FacingDirection == 2 && farmerMotionFrame >= 2))
				{
					this.CurrentParentTileIndex = this.InitialParentTileIndex + 1;
				}
			}
			else if (farmerMotionFrame < 5 || direction == 0)
			{
				this.CurrentParentTileIndex = this.InitialParentTileIndex;
			}
			else
			{
				this.CurrentParentTileIndex = this.InitialParentTileIndex + 1;
			}
			this.CurrentParentTileIndex += offset;
		}

		// Token: 0x060015B5 RID: 5557 RVA: 0x000FF3C4 File Offset: 0x000FD5C4
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			ToolData data = this.GetToolData();
			if (data == null || data.SalePrice < 0)
			{
				return base.salePrice(ignoreProfitMargins);
			}
			return data.SalePrice;
		}

		// Token: 0x060015B6 RID: 5558 RVA: 0x000FF3F2 File Offset: 0x000FD5F2
		public override int attachmentSlots()
		{
			return this.numAttachmentSlots.Value;
		}

		// Token: 0x060015B7 RID: 5559 RVA: 0x000FF3FF File Offset: 0x000FD5FF
		public Farmer getLastFarmerToUse()
		{
			return this.lastUser;
		}

		// Token: 0x060015B8 RID: 5560 RVA: 0x000FF407 File Offset: 0x000FD607
		public virtual void leftClick(Farmer who)
		{
		}

		// Token: 0x060015B9 RID: 5561 RVA: 0x000FF40C File Offset: 0x000FD60C
		public virtual void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			this.lastUser = who;
			Game1.recentMultiplayerRandom = Utility.CreateRandom((double)((short)Game1.random.Next(-32768, 32768)), 0.0, 0.0, 0.0, 0.0);
			if (this.isHeavyHitter() && !(this is MeleeWeapon))
			{
				Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 4.0), (float)(100 + Game1.random.Next(50)));
				location.damageMonster(new Rectangle(x - 32, y - 32, 64, 64), this.upgradeLevel.Value + 1, (this.upgradeLevel.Value + 1) * 3, false, who, false);
			}
			MeleeWeapon weapon = this as MeleeWeapon;
			if (weapon != null && (!who.UsingTool || Game1.mouseClickPolling >= 50 || weapon.type.Value == 1 || !(weapon.ItemId != "47") || MeleeWeapon.timedHitTimer > 0 || who.FarmerSprite.currentAnimationIndex != 5 || who.FarmerSprite.timer >= who.FarmerSprite.interval / 4f))
			{
				if (weapon.type.Value == 2 && weapon.isOnSpecial)
				{
					weapon.triggerClubFunction(who);
					return;
				}
				if (who.FarmerSprite.currentAnimationIndex > 0)
				{
					MeleeWeapon.timedHitTimer = 500;
				}
			}
		}

		// Token: 0x060015BA RID: 5562 RVA: 0x000FF58C File Offset: 0x000FD78C
		public virtual void endUsing(GameLocation location, Farmer who)
		{
			this.swingTicker++;
			who.stopJittering();
			who.canReleaseTool = false;
			int addedAnimationMultiplayer = (who.Stamina <= 0f) ? 2 : 1;
			if (Game1.isAnyGamePadButtonBeingPressed() || !who.IsLocalPlayer)
			{
				who.lastClick = who.GetToolLocation(false);
			}
			WateringCan wateringCan = this as WateringCan;
			if (wateringCan == null)
			{
				FishingRod rod = this as FishingRod;
				if (rod != null && who.IsLocalPlayer && Game1.activeClickableMenu == null)
				{
					if (!rod.hit)
					{
						this.DoFunction(who.currentLocation, (int)who.lastClick.X, (int)who.lastClick.Y, 1, who);
						return;
					}
				}
				else if (!(this is MeleeWeapon) && !(this is Pan) && !(this is Shears) && !(this is MilkPail) && !(this is Slingshot))
				{
					switch (who.FacingDirection)
					{
					case 0:
						((FarmerSprite)who.Sprite).animateOnce(176, 60f * (float)addedAnimationMultiplayer, 8);
						return;
					case 1:
						((FarmerSprite)who.Sprite).animateOnce(168, 60f * (float)addedAnimationMultiplayer, 8);
						return;
					case 2:
						((FarmerSprite)who.Sprite).animateOnce(160, 60f * (float)addedAnimationMultiplayer, 8);
						return;
					case 3:
						((FarmerSprite)who.Sprite).animateOnce(184, 60f * (float)addedAnimationMultiplayer, 8);
						break;
					default:
						return;
					}
				}
				return;
			}
			if (wateringCan.WaterLeft > 0 && who.ShouldHandleAnimationSound() && this.PlayUseSounds)
			{
				who.playNearbySoundLocal("wateringCan", null, SoundContext.Default);
			}
			switch (who.FacingDirection)
			{
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(180, 125f * (float)addedAnimationMultiplayer, 3);
				return;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(172, 125f * (float)addedAnimationMultiplayer, 3);
				return;
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(164, 125f * (float)addedAnimationMultiplayer, 3);
				return;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(188, 125f * (float)addedAnimationMultiplayer, 3);
				return;
			default:
				return;
			}
		}

		// Token: 0x060015BB RID: 5563 RVA: 0x000FF7D8 File Offset: 0x000FD9D8
		public virtual bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			this.lastUser = who;
			if (!this.instantUse.Value)
			{
				who.Halt();
				this.Update(who.FacingDirection, 0, who);
				if ((!(this is FishingRod) && this.upgradeLevel.Value <= 0 && !(this is MeleeWeapon)) || this is Pickaxe)
				{
					who.EndUsingTool();
					return true;
				}
			}
			if (this.instantUse.Value)
			{
				Game1.toolAnimationDone(who);
				who.CanMove = true;
				who.canReleaseTool = false;
				who.UsingTool = false;
			}
			else if (this is WateringCan && location.CanRefillWateringCanOnTile((int)who.GetToolLocation(false).X / 64, (int)who.GetToolLocation(false).Y / 64))
			{
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(182, 250f, 2);
					this.Update(0, 1, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(174, 250f, 2);
					this.Update(1, 0, who);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(166, 250f, 2);
					this.Update(2, 1, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(190, 250f, 2);
					this.Update(3, 0, who);
					break;
				}
				who.canReleaseTool = false;
			}
			else
			{
				WateringCan wateringCan = this as WateringCan;
				if (wateringCan != null && wateringCan.WaterLeft <= 0)
				{
					Game1.toolAnimationDone(who);
					who.CanMove = true;
					who.canReleaseTool = false;
				}
				else if (this is WateringCan)
				{
					who.jitterStrength = 0.25f;
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.setCurrentFrame(180);
						this.Update(0, 0, who);
						break;
					case 1:
						who.FarmerSprite.setCurrentFrame(172);
						this.Update(1, 0, who);
						break;
					case 2:
						who.FarmerSprite.setCurrentFrame(164);
						this.Update(2, 0, who);
						break;
					case 3:
						who.FarmerSprite.setCurrentFrame(188);
						this.Update(3, 0, who);
						break;
					}
				}
				else if (this is FishingRod)
				{
					switch (who.FacingDirection)
					{
					case 0:
						((FarmerSprite)who.Sprite).animateOnce(295, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
						this.Update(0, 0, who);
						break;
					case 1:
						((FarmerSprite)who.Sprite).animateOnce(296, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
						this.Update(1, 0, who);
						break;
					case 2:
						((FarmerSprite)who.Sprite).animateOnce(297, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
						this.Update(2, 0, who);
						break;
					case 3:
						((FarmerSprite)who.Sprite).animateOnce(298, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
						this.Update(3, 0, who);
						break;
					}
					who.canReleaseTool = false;
				}
				else if (this is MeleeWeapon)
				{
					((MeleeWeapon)this).setFarmerAnimating(who);
				}
				else
				{
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.setCurrentFrame(176);
						this.Update(0, 0, who);
						break;
					case 1:
						who.FarmerSprite.setCurrentFrame(168);
						this.Update(1, 0, who);
						break;
					case 2:
						who.FarmerSprite.setCurrentFrame(160);
						this.Update(2, 0, who);
						break;
					case 3:
						who.FarmerSprite.setCurrentFrame(184);
						this.Update(3, 0, who);
						break;
					}
				}
			}
			return false;
		}

		// Token: 0x060015BC RID: 5564 RVA: 0x000FFC19 File Offset: 0x000FDE19
		public virtual bool onRelease(GameLocation location, int x, int y, Farmer who)
		{
			return false;
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x000FFC1C File Offset: 0x000FDE1C
		public override bool canBeDropped()
		{
			return false;
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x000FFC20 File Offset: 0x000FDE20
		public virtual bool canThisBeAttached(Object o)
		{
			NetObjectArray<Object> netObjectArray = this.attachments;
			if (netObjectArray != null && netObjectArray.Count > 0)
			{
				if (o == null)
				{
					return true;
				}
				for (int slot = 0; slot < this.attachments.Length; slot++)
				{
					if (this.canThisBeAttached(o, slot))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060015BF RID: 5567 RVA: 0x000FFC6C File Offset: 0x000FDE6C
		protected virtual bool canThisBeAttached(Object o, int slot)
		{
			return true;
		}

		// Token: 0x060015C0 RID: 5568 RVA: 0x000FFC70 File Offset: 0x000FDE70
		public virtual Object attach(Object o)
		{
			if (o == null)
			{
				for (int slot = 0; slot < this.attachments.Length; slot++)
				{
					Object oldObj = this.attachments[slot];
					if (oldObj != null)
					{
						this.attachments[slot] = null;
						Game1.playSound("dwop", null);
						return oldObj;
					}
				}
				return null;
			}
			int originalStack = o.Stack;
			for (int slot2 = 0; slot2 < this.attachments.Length; slot2++)
			{
				if (this.canThisBeAttached(o, slot2))
				{
					Object oldObj2 = this.attachments[slot2];
					if (oldObj2 == null)
					{
						this.attachments[slot2] = o;
						o = null;
						break;
					}
					if (oldObj2.canStackWith(o))
					{
						int toRemove = o.Stack - oldObj2.addToStack(o);
						if (o.ConsumeStack(toRemove) == null)
						{
							o = null;
							break;
						}
					}
				}
			}
			if (o == null || o.Stack != originalStack)
			{
				Game1.playSound("button1", null);
				return o;
			}
			for (int slot3 = 0; slot3 < this.attachments.Length; slot3++)
			{
				Object oldObj3 = this.attachments[slot3];
				this.attachments[slot3] = null;
				if (this.canThisBeAttached(o, slot3))
				{
					this.attachments[slot3] = o;
					Game1.playSound("button1", null);
					return oldObj3;
				}
				this.attachments[slot3] = oldObj3;
			}
			return o;
		}

		// Token: 0x060015C1 RID: 5569 RVA: 0x000FFDE0 File Offset: 0x000FDFE0
		public virtual void actionWhenClaimed()
		{
			if (this is GenericTool)
			{
				int value = this.indexOfMenuItemView.Value;
				if (value - 13 <= 3)
				{
					Game1.player.trashCanLevel++;
				}
			}
		}

		// Token: 0x060015C2 RID: 5570 RVA: 0x000FFE1C File Offset: 0x000FE01C
		public override bool CanBuyItem(Farmer who)
		{
			return (Game1.player.toolBeingUpgraded.Value == null && (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan || (this is GenericTool && this.indexOfMenuItemView.Value >= 13 && this.indexOfMenuItemView.Value <= 16))) || base.CanBuyItem(who);
		}

		// Token: 0x060015C3 RID: 5571 RVA: 0x000FFE8C File Offset: 0x000FE08C
		public override bool actionWhenPurchased(string shopId)
		{
			if (shopId == "ClintUpgrade" && Game1.player.toolBeingUpgraded.Value == null)
			{
				if (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan || this is Pan)
				{
					ToolUpgradeData toolUpgradeData = ShopBuilder.GetToolUpgradeData(this.GetToolData(), Game1.player);
					string previousToolId = (toolUpgradeData != null) ? toolUpgradeData.RequireToolId : null;
					if (previousToolId != null)
					{
						Item oldItem = Game1.player.Items.GetById(previousToolId).FirstOrDefault<Item>();
						Game1.player.removeItemFromInventory(oldItem);
						Tool oldTool = oldItem as Tool;
						if (oldTool != null)
						{
							this.UpgradeFrom(oldTool);
						}
					}
					Game1.player.toolBeingUpgraded.Value = (Tool)base.getOne();
					Game1.player.daysLeftForToolUpgrade.Value = 2;
					Game1.playSound("parry", null);
					Game1.exitActiveMenu();
					Game1.DrawDialogue(Game1.getCharacterFromName("Clint", true, false), "Strings\\StringsFromCSFiles:Tool.cs.14317");
					return true;
				}
				if (this is GenericTool)
				{
					int value = this.indexOfMenuItemView.Value;
					if (value - 13 <= 3)
					{
						Game1.player.toolBeingUpgraded.Value = (Tool)base.getOne();
						Game1.player.daysLeftForToolUpgrade.Value = 2;
						Game1.playSound("parry", null);
						Game1.exitActiveMenu();
						Game1.DrawDialogue(Game1.getCharacterFromName("Clint", true, false), "Strings\\StringsFromCSFiles:Tool.cs.14317");
						return true;
					}
				}
			}
			return base.actionWhenPurchased(shopId);
		}

		// Token: 0x060015C4 RID: 5572 RVA: 0x00100020 File Offset: 0x000FE220
		protected List<Vector2> tilesAffected(Vector2 tileLocation, int power, Farmer who)
		{
			power++;
			List<Vector2> tileLocations = new List<Vector2>();
			tileLocations.Add(tileLocation);
			Vector2 extremePowerPosition = Vector2.Zero;
			switch (who.FacingDirection)
			{
			case 0:
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y - 2f);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, -1f));
						tileLocations.Add(tileLocation + new Vector2(0f, -2f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, -3f));
						tileLocations.Add(tileLocation + new Vector2(0f, -4f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(1f, -2f));
						tileLocations.Add(tileLocation + new Vector2(1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-1f, -2f));
						tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
					}
					if (power >= 5)
					{
						for (int i = tileLocations.Count - 1; i >= 0; i--)
						{
							tileLocations.Add(tileLocations[i] + new Vector2(0f, -3f));
						}
					}
				}
				break;
			case 1:
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X + 2f, tileLocation.Y);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(2f, 0f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(3f, 0f));
						tileLocations.Add(tileLocation + new Vector2(4f, 0f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(0f, -1f));
						tileLocations.Add(tileLocation + new Vector2(1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(2f, -1f));
						tileLocations.Add(tileLocation + new Vector2(0f, 1f));
						tileLocations.Add(tileLocation + new Vector2(1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(2f, 1f));
					}
					if (power >= 5)
					{
						for (int j = tileLocations.Count - 1; j >= 0; j--)
						{
							tileLocations.Add(tileLocations[j] + new Vector2(3f, 0f));
						}
					}
				}
				break;
			case 2:
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y + 2f);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, 1f));
						tileLocations.Add(tileLocation + new Vector2(0f, 2f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, 3f));
						tileLocations.Add(tileLocation + new Vector2(0f, 4f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(1f, 2f));
						tileLocations.Add(tileLocation + new Vector2(1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 2f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
					}
					if (power >= 5)
					{
						for (int k = tileLocations.Count - 1; k >= 0; k--)
						{
							tileLocations.Add(tileLocations[k] + new Vector2(0f, 3f));
						}
					}
				}
				break;
			case 3:
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X - 2f, tileLocation.Y);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-2f, 0f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(-3f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-4f, 0f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(0f, -1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(-2f, -1f));
						tileLocations.Add(tileLocation + new Vector2(0f, 1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(-2f, 1f));
					}
					if (power >= 5)
					{
						for (int l = tileLocations.Count - 1; l >= 0; l--)
						{
							tileLocations.Add(tileLocations[l] + new Vector2(-3f, 0f));
						}
					}
				}
				break;
			}
			if (power >= 6)
			{
				tileLocations.Clear();
				int x = (int)extremePowerPosition.X - 2;
				while ((float)x <= extremePowerPosition.X + 2f)
				{
					int y = (int)extremePowerPosition.Y - 2;
					while ((float)y <= extremePowerPosition.Y + 2f)
					{
						tileLocations.Add(new Vector2((float)x, (float)y));
						y++;
					}
					x++;
				}
			}
			return tileLocations;
		}

		// Token: 0x060015C5 RID: 5573 RVA: 0x0010073E File Offset: 0x000FE93E
		public virtual bool doesShowTileLocationMarker()
		{
			return true;
		}

		// Token: 0x060015C6 RID: 5574 RVA: 0x00100744 File Offset: 0x000FE944
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(data.GetTexture(), location + new Vector2(32f, 32f), new Rectangle?(data.GetSourceRect(0, null)), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x060015C7 RID: 5575 RVA: 0x001007D5 File Offset: 0x000FE9D5
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x060015C8 RID: 5576 RVA: 0x001007D8 File Offset: 0x000FE9D8
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x001007DB File Offset: 0x000FE9DB
		public override string getDescription()
		{
			return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x001007F4 File Offset: 0x000FE9F4
		protected override int getDescriptionWidth()
		{
			int amount = base.getDescriptionWidth();
			foreach (BaseEnchantment e in this.enchantments)
			{
				amount = Math.Max(amount, (int)(Game1.smallFont.MeasureString(e.GetDisplayName()).X + 128f));
			}
			return amount;
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x0010086C File Offset: 0x000FEA6C
		public virtual void ClearEnchantments()
		{
			for (int i = this.enchantments.Count - 1; i >= 0; i--)
			{
				this.enchantments[i].UnapplyTo(this, null);
			}
			this.enchantments.Clear();
		}

		// Token: 0x060015CC RID: 5580 RVA: 0x001008AF File Offset: 0x000FEAAF
		public virtual int GetMaxForges()
		{
			return 0;
		}

		// Token: 0x060015CD RID: 5581 RVA: 0x001008B4 File Offset: 0x000FEAB4
		public virtual bool CanAddEnchantment(BaseEnchantment enchantment)
		{
			if (!enchantment.IsForge() && !enchantment.IsSecondaryEnchantment())
			{
				return true;
			}
			if (this.GetTotalForgeLevels(false) >= this.GetMaxForges() && !enchantment.IsSecondaryEnchantment())
			{
				return false;
			}
			if (enchantment != null)
			{
				foreach (BaseEnchantment existing_enchantment in this.enchantments)
				{
					if (enchantment.GetType() == existing_enchantment.GetType())
					{
						if (existing_enchantment.GetMaximumLevel() < 0 || existing_enchantment.GetLevel() < existing_enchantment.GetMaximumLevel())
						{
							return true;
						}
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x060015CE RID: 5582 RVA: 0x00100968 File Offset: 0x000FEB68
		public virtual void CopyEnchantments(Tool source, Tool destination)
		{
			foreach (BaseEnchantment enchantment in source.enchantments)
			{
				destination.enchantments.Add(enchantment.GetOne());
				enchantment.GetOne().ApplyTo(destination, null);
			}
			destination.previousEnchantments.Clear();
			destination.previousEnchantments.AddRange(source.previousEnchantments);
		}

		// Token: 0x060015CF RID: 5583 RVA: 0x001009F0 File Offset: 0x000FEBF0
		public int GetTotalForgeLevels(bool for_unforge = false)
		{
			int total = 0;
			foreach (BaseEnchantment existing_enchantment in this.enchantments)
			{
				if (existing_enchantment is DiamondEnchantment)
				{
					if (for_unforge)
					{
						return total;
					}
				}
				else if (existing_enchantment.IsForge())
				{
					total += existing_enchantment.GetLevel();
				}
			}
			return total;
		}

		// Token: 0x060015D0 RID: 5584 RVA: 0x00100A64 File Offset: 0x000FEC64
		public virtual bool AddEnchantment(BaseEnchantment enchantment)
		{
			if (enchantment == null)
			{
				return false;
			}
			if (this is MeleeWeapon && (enchantment.IsForge() || enchantment.IsSecondaryEnchantment()))
			{
				foreach (BaseEnchantment existing_enchantment in this.enchantments)
				{
					if (enchantment.GetType() == existing_enchantment.GetType())
					{
						if (existing_enchantment.GetMaximumLevel() < 0 || existing_enchantment.GetLevel() < existing_enchantment.GetMaximumLevel())
						{
							existing_enchantment.SetLevel(this, existing_enchantment.GetLevel() + 1);
							return true;
						}
						return false;
					}
				}
				this.enchantments.Add(enchantment);
				enchantment.ApplyTo(this, this.lastUser);
				return true;
			}
			for (int i = this.enchantments.Count - 1; i >= 0; i--)
			{
				BaseEnchantment prevEnchantment = this.enchantments[i];
				if (!prevEnchantment.IsForge() && !prevEnchantment.IsSecondaryEnchantment())
				{
					prevEnchantment.UnapplyTo(this, null);
					this.enchantments.RemoveAt(i);
				}
			}
			this.enchantments.Add(enchantment);
			enchantment.ApplyTo(this, this.lastUser);
			return true;
		}

		// Token: 0x060015D1 RID: 5585 RVA: 0x00100BA4 File Offset: 0x000FEDA4
		public bool hasEnchantmentOfType<T>()
		{
			using (NetList<BaseEnchantment, NetRef<BaseEnchantment>>.Enumerator enumerator = this.enchantments.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is T)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060015D2 RID: 5586 RVA: 0x00100C00 File Offset: 0x000FEE00
		public virtual void RemoveEnchantment(BaseEnchantment enchantment)
		{
			if (enchantment != null)
			{
				this.enchantments.Remove(enchantment);
				enchantment.UnapplyTo(this, this.lastUser);
			}
		}

		// Token: 0x060015D3 RID: 5587 RVA: 0x00100C20 File Offset: 0x000FEE20
		public override void actionWhenBeingHeld(Farmer who)
		{
			base.actionWhenBeingHeld(who);
			if (who.IsLocalPlayer)
			{
				foreach (BaseEnchantment baseEnchantment in this.enchantments)
				{
					baseEnchantment.OnEquip(who);
				}
			}
		}

		// Token: 0x060015D4 RID: 5588 RVA: 0x00100C80 File Offset: 0x000FEE80
		public override void actionWhenStopBeingHeld(Farmer who)
		{
			base.actionWhenStopBeingHeld(who);
			if (who.UsingTool)
			{
				who.UsingTool = false;
				if (who.FarmerSprite.PauseForSingleAnimation)
				{
					who.FarmerSprite.PauseForSingleAnimation = false;
				}
			}
			if (who.IsLocalPlayer)
			{
				foreach (BaseEnchantment baseEnchantment in this.enchantments)
				{
					baseEnchantment.OnUnequip(who);
				}
			}
		}

		// Token: 0x060015D5 RID: 5589 RVA: 0x00100D08 File Offset: 0x000FEF08
		public virtual bool CanUseOnStandingTile()
		{
			return false;
		}

		// Token: 0x060015D6 RID: 5590 RVA: 0x00100D0B File Offset: 0x000FEF0B
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			if (this.hasEnchantmentOfType<MasterEnchantment>())
			{
				effects.FishingLevel.Value += 1f;
			}
		}

		// Token: 0x060015D7 RID: 5591 RVA: 0x00100D34 File Offset: 0x000FEF34
		public virtual bool CanForge(Item item)
		{
			BaseEnchantment enchantment = BaseEnchantment.GetEnchantmentFromItem(this, item);
			if (enchantment != null && this.CanAddEnchantment(enchantment))
			{
				return true;
			}
			if (item != null && item.QualifiedItemId == "(O)852")
			{
				MeleeWeapon weapon = this as MeleeWeapon;
				if (weapon != null && weapon.getItemLevel() < 15 && !this.Name.Contains("Galaxy"))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060015D8 RID: 5592 RVA: 0x00100D98 File Offset: 0x000FEF98
		public T GetEnchantmentOfType<T>() where T : BaseEnchantment
		{
			foreach (BaseEnchantment existing_enchantment in this.enchantments)
			{
				if (existing_enchantment.GetType() == typeof(T))
				{
					return existing_enchantment as T;
				}
			}
			return default(T);
		}

		// Token: 0x060015D9 RID: 5593 RVA: 0x00100E14 File Offset: 0x000FF014
		public int GetEnchantmentLevel<T>() where T : BaseEnchantment
		{
			int total = 0;
			foreach (BaseEnchantment existing_enchantment in this.enchantments)
			{
				if (existing_enchantment.GetType() == typeof(T))
				{
					total += existing_enchantment.GetLevel();
				}
			}
			return total;
		}

		// Token: 0x060015DA RID: 5594 RVA: 0x00100E84 File Offset: 0x000FF084
		public virtual bool Forge(Item item, bool count_towards_stats = false)
		{
			BaseEnchantment enchantment = BaseEnchantment.GetEnchantmentFromItem(this, item);
			if (enchantment != null)
			{
				if (this.AddEnchantment(enchantment))
				{
					if (!(enchantment is DiamondEnchantment))
					{
						if (enchantment is GalaxySoulEnchantment)
						{
							MeleeWeapon weapon = this as MeleeWeapon;
							if (weapon != null && weapon.isGalaxyWeapon() && weapon.GetEnchantmentLevel<GalaxySoulEnchantment>() >= 3)
							{
								string newItemId = null;
								string qualifiedItemId = base.QualifiedItemId;
								if (!(qualifiedItemId == "(W)4"))
								{
									if (!(qualifiedItemId == "(W)29"))
									{
										if (qualifiedItemId == "(W)23")
										{
											newItemId = "64";
										}
									}
									else
									{
										newItemId = "63";
									}
								}
								else
								{
									newItemId = "62";
								}
								if (newItemId != null)
								{
									weapon.transform(newItemId);
									if (count_towards_stats)
									{
										DelayedAction.playSoundAfterDelay("discoverMineral", 400, null, null, -1, false);
										Game1.multiplayer.globalChatInfoMessage("InfinityWeapon", new string[]
										{
											Game1.player.name.Value,
											TokenStringBuilder.ItemNameFor(this, null)
										});
										Game1.getAchievement(42, true);
									}
								}
								GalaxySoulEnchantment enchant = this.GetEnchantmentOfType<GalaxySoulEnchantment>();
								if (enchant != null)
								{
									this.RemoveEnchantment(enchant);
								}
							}
						}
					}
					else
					{
						int forges_left = this.GetMaxForges() - this.GetTotalForgeLevels(false);
						List<int> valid_forges = new List<int>();
						if (!this.hasEnchantmentOfType<EmeraldEnchantment>())
						{
							valid_forges.Add(0);
						}
						if (!this.hasEnchantmentOfType<AquamarineEnchantment>())
						{
							valid_forges.Add(1);
						}
						if (!this.hasEnchantmentOfType<RubyEnchantment>())
						{
							valid_forges.Add(2);
						}
						if (!this.hasEnchantmentOfType<AmethystEnchantment>())
						{
							valid_forges.Add(3);
						}
						if (!this.hasEnchantmentOfType<TopazEnchantment>())
						{
							valid_forges.Add(4);
						}
						if (!this.hasEnchantmentOfType<JadeEnchantment>())
						{
							valid_forges.Add(5);
						}
						for (int i = 0; i < forges_left; i++)
						{
							if (valid_forges.Count == 0)
							{
								break;
							}
							int index = Game1.random.Next(valid_forges.Count);
							int random_enchant = valid_forges[index];
							valid_forges.RemoveAt(index);
							switch (random_enchant)
							{
							case 0:
								this.AddEnchantment(new EmeraldEnchantment());
								break;
							case 1:
								this.AddEnchantment(new AquamarineEnchantment());
								break;
							case 2:
								this.AddEnchantment(new RubyEnchantment());
								break;
							case 3:
								this.AddEnchantment(new AmethystEnchantment());
								break;
							case 4:
								this.AddEnchantment(new TopazEnchantment());
								break;
							case 5:
								this.AddEnchantment(new JadeEnchantment());
								break;
							}
						}
					}
					if (count_towards_stats && !enchantment.IsForge())
					{
						this.previousEnchantments.Insert(0, enchantment.GetName());
						while (this.previousEnchantments.Count > 2)
						{
							this.previousEnchantments.RemoveAt(this.previousEnchantments.Count - 1);
						}
						Game1.stats.Increment("timesEnchanted", 1U);
					}
					return true;
				}
			}
			else if (item.QualifiedItemId == "(O)852")
			{
				MeleeWeapon weapon2 = this as MeleeWeapon;
				if (weapon2 != null)
				{
					List<BaseEnchantment> oldEnchantments = new List<BaseEnchantment>();
					weapon2.enchantments.RemoveWhere(delegate(BaseEnchantment curEnchantment)
					{
						if (curEnchantment.IsSecondaryEnchantment() && !(curEnchantment is GalaxySoulEnchantment))
						{
							oldEnchantments.Add(curEnchantment);
							return true;
						}
						return false;
					});
					MeleeWeapon.attemptAddRandomInnateEnchantment(weapon2, Game1.random, true, oldEnchantments);
					return true;
				}
			}
			return false;
		}

		// Token: 0x04000DE4 RID: 3556
		public const int standardStaminaReduction = 2;

		// Token: 0x04000DE5 RID: 3557
		public const int stone = 0;

		// Token: 0x04000DE6 RID: 3558
		public const int copper = 1;

		// Token: 0x04000DE7 RID: 3559
		public const int steel = 2;

		// Token: 0x04000DE8 RID: 3560
		public const int gold = 3;

		// Token: 0x04000DE9 RID: 3561
		public const int iridium = 4;

		// Token: 0x04000DEA RID: 3562
		public const int hammerSpriteIndex = 105;

		// Token: 0x04000DEB RID: 3563
		public const int wateringCanSpriteIndex = 273;

		// Token: 0x04000DEC RID: 3564
		public const int fishingRodSpriteIndex = 8;

		// Token: 0x04000DED RID: 3565
		public const int wateringCanMenuIndex = 296;

		// Token: 0x04000DEE RID: 3566
		public const string weaponsTextureName = "TileSheets\\weapons";

		// Token: 0x04000DEF RID: 3567
		public static Texture2D weaponsTexture;

		// Token: 0x04000DF0 RID: 3568
		[XmlElement("initialParentTileIndex")]
		public readonly NetInt initialParentTileIndex = new NetInt();

		// Token: 0x04000DF1 RID: 3569
		[XmlElement("currentParentTileIndex")]
		public readonly NetInt currentParentTileIndex = new NetInt();

		// Token: 0x04000DF2 RID: 3570
		[XmlElement("indexOfMenuItemView")]
		public readonly NetInt indexOfMenuItemView = new NetInt();

		// Token: 0x04000DF3 RID: 3571
		[XmlElement("instantUse")]
		public readonly NetBool instantUse = new NetBool();

		// Token: 0x04000DF4 RID: 3572
		[XmlElement("isEfficient")]
		public readonly NetBool isEfficient = new NetBool();

		// Token: 0x04000DF5 RID: 3573
		[XmlElement("animationSpeedModifier")]
		public readonly NetFloat animationSpeedModifier = new NetFloat(1f);

		// Token: 0x04000DF6 RID: 3574
		public int swingTicker = Game1.random.Next(999999);

		// Token: 0x04000DF7 RID: 3575
		[XmlIgnore]
		private string _description;

		// Token: 0x04000DF8 RID: 3576
		[XmlElement("upgradeLevel")]
		public readonly NetInt upgradeLevel = new NetInt();

		// Token: 0x04000DF9 RID: 3577
		[XmlElement("numAttachmentSlots")]
		public readonly NetInt numAttachmentSlots = new NetInt();

		// Token: 0x04000DFA RID: 3578
		[XmlIgnore]
		public Farmer lastUser;

		// Token: 0x04000DFB RID: 3579
		public readonly NetObjectArray<Object> attachments = new NetObjectArray<Object>();

		// Token: 0x04000DFD RID: 3581
		[XmlIgnore]
		protected string displayName;

		// Token: 0x04000DFE RID: 3582
		[XmlElement("enchantments")]
		public readonly NetList<BaseEnchantment, NetRef<BaseEnchantment>> enchantments = new NetList<BaseEnchantment, NetRef<BaseEnchantment>>();

		// Token: 0x04000DFF RID: 3583
		[XmlElement("previousEnchantments")]
		public readonly NetStringList previousEnchantments = new NetStringList();

		// Token: 0x04000E00 RID: 3584
		[XmlIgnore]
		public bool PlayUseSounds = true;
	}
}

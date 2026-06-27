using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Buffs;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Weapons;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace StardewValley.Tools
{
	// Token: 0x0200012D RID: 301
	public class MeleeWeapon : Tool
	{
		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x0600183F RID: 6207 RVA: 0x0011B98F File Offset: 0x00119B8F
		public override string TypeDefinitionId { get; } = "(W)";

		// Token: 0x06001840 RID: 6208 RVA: 0x0011B998 File Offset: 0x00119B98
		public MeleeWeapon()
		{
			base.Category = -98;
		}

		// Token: 0x06001841 RID: 6209 RVA: 0x0011BA5B File Offset: 0x00119C5B
		public MeleeWeapon(string itemId) : this()
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			base.ItemId = itemId;
			this.Stack = 1;
			this.ReloadData();
		}

		// Token: 0x06001842 RID: 6210 RVA: 0x0011BA80 File Offset: 0x00119C80
		protected void ReloadData()
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			WeaponData data;
			if (MeleeWeapon.TryGetData(this.itemId.Value, out data))
			{
				this.cachedData = data;
				this.Name = (data.Name ?? itemData.InternalName);
				this.minDamage.Value = data.MinDamage;
				this.maxDamage.Value = data.MaxDamage;
				this.knockback.Value = data.Knockback;
				this.speed.Value = data.Speed;
				this.addedPrecision.Value = data.Precision;
				this.addedDefense.Value = data.Defense;
				this.type.Value = data.Type;
				this.addedAreaOfEffect.Value = data.AreaOfEffect;
				this.critChance.Value = data.CritChance;
				this.critMultiplier.Value = data.CritMultiplier;
				if (this.type.Value == 0)
				{
					this.type.Value = 3;
				}
			}
			else
			{
				this.Name = "Error Item";
			}
			base.InitialParentTileIndex = itemData.SpriteIndex;
			base.CurrentParentTileIndex = itemData.SpriteIndex;
			base.IndexOfMenuItemView = itemData.SpriteIndex;
			base.Category = (this.isScythe() ? -99 : -98);
		}

		// Token: 0x06001843 RID: 6211 RVA: 0x0011BBD8 File Offset: 0x00119DD8
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = base.InitialParentTileIndex.ToString();
		}

		// Token: 0x06001844 RID: 6212 RVA: 0x0011BBF9 File Offset: 0x00119DF9
		public WeaponData GetData()
		{
			if (this.cachedData == null)
			{
				MeleeWeapon.TryGetData(base.ItemId, out this.cachedData);
			}
			return this.cachedData;
		}

		// Token: 0x06001845 RID: 6213 RVA: 0x0011BC1B File Offset: 0x00119E1B
		public static bool TryGetData(string itemId, out WeaponData data)
		{
			if (itemId == null)
			{
				data = null;
				return false;
			}
			return Game1.weaponData.TryGetValue(itemId, out data);
		}

		// Token: 0x06001846 RID: 6214 RVA: 0x0011BC31 File Offset: 0x00119E31
		public override bool CanBeLostOnDeath()
		{
			if (base.CanBeLostOnDeath())
			{
				WeaponData data = this.GetData();
				return data == null || data.CanBeLostOnDeath;
			}
			return false;
		}

		// Token: 0x06001847 RID: 6215 RVA: 0x0011BC50 File Offset: 0x00119E50
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.Defense.Value += (float)this.addedDefense.Value;
			foreach (BaseEnchantment baseEnchantment in this.enchantments)
			{
				baseEnchantment.AddEquipmentEffects(effects);
			}
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x0011BCC8 File Offset: 0x00119EC8
		public override int GetMaxForges()
		{
			return 3;
		}

		// Token: 0x06001849 RID: 6217 RVA: 0x0011BCCB File Offset: 0x00119ECB
		protected override Item GetOneNew()
		{
			return new MeleeWeapon(base.ItemId);
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x0011BCD8 File Offset: 0x00119ED8
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			MeleeWeapon fromWeapon = source as MeleeWeapon;
			if (fromWeapon != null)
			{
				this.appearance.Value = fromWeapon.appearance.Value;
				base.IndexOfMenuItemView = fromWeapon.IndexOfMenuItemView;
			}
		}

		// Token: 0x0600184B RID: 6219 RVA: 0x0011BD18 File Offset: 0x00119F18
		protected override string loadDisplayName()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
		}

		// Token: 0x0600184C RID: 6220 RVA: 0x0011BD2A File Offset: 0x00119F2A
		protected override string loadDescription()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
		}

		// Token: 0x0600184D RID: 6221 RVA: 0x0011BD3C File Offset: 0x00119F3C
		public override string getCategoryName()
		{
			if (!this.isScythe())
			{
				int value = this.type.Value;
				string typeNameKey;
				if (value != 1)
				{
					if (value != 2)
					{
						typeNameKey = "Strings\\StringsFromCSFiles:Tool.cs.14306";
					}
					else
					{
						typeNameKey = "Strings\\StringsFromCSFiles:Tool.cs.14305";
					}
				}
				else
				{
					typeNameKey = "Strings\\StringsFromCSFiles:Tool.cs.14304";
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14303", this.getItemLevel(), Game1.content.LoadString(typeNameKey));
			}
			return base.getCategoryName();
		}

		// Token: 0x0600184E RID: 6222 RVA: 0x0011BDAC File Offset: 0x00119FAC
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.type, "type").AddField(this.minDamage, "minDamage").AddField(this.maxDamage, "maxDamage").AddField(this.speed, "speed").AddField(this.addedPrecision, "addedPrecision").AddField(this.addedDefense, "addedDefense").AddField(this.addedAreaOfEffect, "addedAreaOfEffect").AddField(this.knockback, "knockback").AddField(this.critChance, "critChance").AddField(this.critMultiplier, "critMultiplier").AddField(this.appearance, "appearance").AddField(this.animateSpecialMoveEvent, "animateSpecialMoveEvent").AddField(this.defenseSwordEvent, "defenseSwordEvent").AddField(this.daggerEvent, "daggerEvent");
			this.animateSpecialMoveEvent.onEvent += this.doAnimateSpecialMove;
			this.defenseSwordEvent.onEvent += this.doDefenseSwordFunction;
			this.daggerEvent.onEvent += this.doDaggerFunction;
			this.itemId.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.ReloadData();
			};
		}

		// Token: 0x0600184F RID: 6223 RVA: 0x0011BF03 File Offset: 0x0011A103
		public override string checkForSpecialItemHoldUpMeessage()
		{
			if (base.QualifiedItemId == "(W)4")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14122");
			}
			return null;
		}

		// Token: 0x06001850 RID: 6224 RVA: 0x0011BF28 File Offset: 0x0011A128
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			float coolDownLevel = 0f;
			float addedScale = 0f;
			if (!this.isScythe())
			{
				switch (this.type.Value)
				{
				case 0:
				case 3:
					if (MeleeWeapon.defenseCooldown > 0)
					{
						coolDownLevel = (float)MeleeWeapon.defenseCooldown / 1500f;
					}
					addedScale = MeleeWeapon.addedSwordScale;
					break;
				case 1:
					if (MeleeWeapon.daggerCooldown > 0)
					{
						coolDownLevel = (float)MeleeWeapon.daggerCooldown / 3000f;
					}
					addedScale = MeleeWeapon.addedDaggerScale;
					break;
				case 2:
					if (MeleeWeapon.clubCooldown > 0)
					{
						coolDownLevel = (float)MeleeWeapon.clubCooldown / 6000f;
					}
					addedScale = MeleeWeapon.addedClubScale;
					break;
				}
			}
			bool drawing_as_debris = drawShadow && drawStackNumber == StackDrawType.Hide;
			if (!drawShadow || drawing_as_debris)
			{
				addedScale = 0f;
			}
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this.GetDrawnItemId());
			Texture2D texture = dataOrErrorItem.GetTexture();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
			spriteBatch.Draw(texture, location + ((this.type.Value == 1) ? new Vector2(38f, 25f) : new Vector2(32f, 32f)), new Rectangle?(sourceRect), color * transparency, 0f, new Vector2(8f, 8f), 4f * (scaleSize + addedScale), SpriteEffects.None, layerDepth);
			if (coolDownLevel > 0f && drawShadow && !drawing_as_debris && !this.isScythe() && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ShopMenu) || scaleSize != 1f))
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (64 - (int)(coolDownLevel * 64f)), 64, (int)(coolDownLevel * 64f)), Color.Red * 0.66f);
			}
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001851 RID: 6225 RVA: 0x0011C100 File Offset: 0x0011A300
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001852 RID: 6226 RVA: 0x0011C103 File Offset: 0x0011A303
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			if (!MeleeWeapon.IsScythe(this.itemId.Value))
			{
				return this.getItemLevel() * 100;
			}
			return 0;
		}

		// Token: 0x06001853 RID: 6227 RVA: 0x0011C124 File Offset: 0x0011A324
		public static void weaponsTypeUpdate(GameTime time)
		{
			if (MeleeWeapon.addedSwordScale > 0f)
			{
				MeleeWeapon.addedSwordScale -= 0.01f;
			}
			if (MeleeWeapon.addedClubScale > 0f)
			{
				MeleeWeapon.addedClubScale -= 0.01f;
			}
			if (MeleeWeapon.addedDaggerScale > 0f)
			{
				MeleeWeapon.addedDaggerScale -= 0.01f;
			}
			if ((float)MeleeWeapon.timedHitTimer > 0f)
			{
				MeleeWeapon.timedHitTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (MeleeWeapon.defenseCooldown > 0)
			{
				MeleeWeapon.defenseCooldown -= time.ElapsedGameTime.Milliseconds;
				if (MeleeWeapon.defenseCooldown <= 0)
				{
					MeleeWeapon.addedSwordScale = 0.5f;
					Game1.playSound("objectiveComplete", null);
				}
			}
			if (MeleeWeapon.attackSwordCooldown > 0)
			{
				MeleeWeapon.attackSwordCooldown -= time.ElapsedGameTime.Milliseconds;
				if (MeleeWeapon.attackSwordCooldown <= 0)
				{
					MeleeWeapon.addedSwordScale = 0.5f;
					Game1.playSound("objectiveComplete", null);
				}
			}
			if (MeleeWeapon.daggerCooldown > 0)
			{
				MeleeWeapon.daggerCooldown -= time.ElapsedGameTime.Milliseconds;
				if (MeleeWeapon.daggerCooldown <= 0)
				{
					MeleeWeapon.addedDaggerScale = 0.5f;
					Game1.playSound("objectiveComplete", null);
				}
			}
			if (MeleeWeapon.clubCooldown > 0)
			{
				MeleeWeapon.clubCooldown -= time.ElapsedGameTime.Milliseconds;
				if (MeleeWeapon.clubCooldown <= 0)
				{
					MeleeWeapon.addedClubScale = 0.5f;
					Game1.playSound("objectiveComplete", null);
				}
			}
		}

		// Token: 0x06001854 RID: 6228 RVA: 0x0011C2C8 File Offset: 0x0011A4C8
		public override void tickUpdate(GameTime time, Farmer who)
		{
			this.lastUser = who;
			base.tickUpdate(time, who);
			this.animateSpecialMoveEvent.Poll();
			this.defenseSwordEvent.Poll();
			this.daggerEvent.Poll();
			if (this.isOnSpecial && this.type.Value == 1 && MeleeWeapon.daggerHitsLeft > 0 && !who.UsingTool)
			{
				this.quickStab(who);
				this.triggerDaggerFunction(who, MeleeWeapon.daggerHitsLeft);
			}
			if (this.anotherClick)
			{
				this.leftClick(who);
			}
		}

		// Token: 0x06001855 RID: 6229 RVA: 0x0011C34D File Offset: 0x0011A54D
		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		// Token: 0x06001856 RID: 6230 RVA: 0x0011C350 File Offset: 0x0011A550
		public int getNumberOfDescriptionCategories()
		{
			int number = 1;
			if (this.speed.Value != ((this.type.Value == 2) ? -8 : 0))
			{
				number++;
			}
			if (this.addedDefense.Value > 0)
			{
				number++;
			}
			float effectiveCritChance = this.critChance.Value;
			if (this.type.Value == 1)
			{
				effectiveCritChance += 0.005f;
				effectiveCritChance *= 1.12f;
			}
			if ((double)effectiveCritChance / 0.02 >= 1.100000023841858)
			{
				number++;
			}
			if ((double)(this.critMultiplier.Value - 3f) / 0.02 >= 1.0)
			{
				number++;
			}
			if (this.knockback.Value != this.defaultKnockBackForThisType(this.type.Value))
			{
				number++;
			}
			if (this.enchantments.Count > 0 && this.enchantments[this.enchantments.Count - 1] is DiamondEnchantment)
			{
				number++;
			}
			return number;
		}

		// Token: 0x06001857 RID: 6231 RVA: 0x0011C45C File Offset: 0x0011A65C
		public override void leftClick(Farmer who)
		{
			if (who.health <= 0 || Game1.activeClickableMenu != null || Game1.farmEvent != null || Game1.eventUp || who.swimming.Value || who.bathingClothes.Value || who.onBridge.Value)
			{
				return;
			}
			if (!this.isScythe() && who.FarmerSprite.currentAnimationIndex > ((this.type.Value == 2) ? 5 : ((this.type.Value == 1) ? 0 : 5)))
			{
				who.completelyStopAnimatingOrDoingAction();
				who.CanMove = false;
				who.UsingTool = true;
				who.canReleaseTool = true;
				this.setFarmerAnimating(who);
				return;
			}
			if (!this.isScythe() && who.FarmerSprite.currentAnimationIndex > ((this.type.Value == 2) ? 3 : ((this.type.Value == 1) ? 0 : 3)))
			{
				this.anotherClick = true;
			}
		}

		// Token: 0x06001858 RID: 6232 RVA: 0x0011C549 File Offset: 0x0011A749
		public override bool isScythe()
		{
			return MeleeWeapon.IsScythe(base.QualifiedItemId);
		}

		// Token: 0x06001859 RID: 6233 RVA: 0x0011C558 File Offset: 0x0011A758
		public static bool IsScythe(string id)
		{
			return id == "(W)47" || id == "(W)53" || id == "(W)66" || id == "47" || id == "53" || id == "66";
		}

		// Token: 0x0600185A RID: 6234 RVA: 0x0011C5B8 File Offset: 0x0011A7B8
		public virtual int getItemLevel()
		{
			float weaponPoints = 0f;
			weaponPoints += (float)((int)((double)((this.maxDamage.Value + this.minDamage.Value) / 2) * (1.0 + 0.03 * (double)(Math.Max(0, this.speed.Value) + ((this.type.Value == 1) ? 15 : 0)))));
			weaponPoints += (float)((int)((double)(this.addedPrecision.Value / 2 + this.addedDefense.Value) + ((double)this.critChance.Value - 0.02) * 200.0 + (double)((this.critMultiplier.Value - 3f) * 6f)));
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(W)2"))
			{
				if (qualifiedItemId == "(W)3")
				{
					weaponPoints += 15f;
				}
			}
			else
			{
				weaponPoints += 20f;
			}
			weaponPoints += (float)(this.addedDefense.Value * 2);
			return (int)(weaponPoints / 7f + 1f);
		}

		// Token: 0x0600185B RID: 6235 RVA: 0x0011C6D4 File Offset: 0x0011A8D4
		public static Item attemptAddRandomInnateEnchantment(Item item, Random r, bool force = false, List<BaseEnchantment> enchantsToReroll = null)
		{
			if (r == null)
			{
				r = Game1.random;
			}
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null && (force || r.NextBool()))
			{
				for (;;)
				{
					int weaponLevel = weapon.getItemLevel();
					if (r.NextDouble() < 0.125 && weaponLevel <= 10)
					{
						weapon.AddEnchantment(new DefenseEnchantment
						{
							Level = Math.Max(1, Math.Min(2, r.Next(weaponLevel + 1) / 2 + 1))
						});
					}
					else if (r.NextDouble() < 0.125)
					{
						weapon.AddEnchantment(new LightweightEnchantment
						{
							Level = r.Next(1, 6)
						});
					}
					else if (r.NextDouble() < 0.125)
					{
						weapon.AddEnchantment(new SlimeGathererEnchantment());
					}
					switch (r.Next(5))
					{
					case 0:
						weapon.AddEnchantment(new AttackEnchantment
						{
							Level = Math.Max(1, Math.Min(5, r.Next(weaponLevel + 1) / 2 + 1))
						});
						break;
					case 1:
						weapon.AddEnchantment(new CritEnchantment
						{
							Level = Math.Max(1, Math.Min(3, r.Next(weaponLevel) / 3))
						});
						break;
					case 2:
						weapon.AddEnchantment(new WeaponSpeedEnchantment
						{
							Level = Math.Max(1, Math.Min(Math.Max(1, 4 - weapon.speed.Value), r.Next(weaponLevel)))
						});
						break;
					case 3:
						weapon.AddEnchantment(new SlimeSlayerEnchantment());
						break;
					case 4:
						weapon.AddEnchantment(new CritPowerEnchantment
						{
							Level = Math.Max(1, Math.Min(3, r.Next(weaponLevel) / 3))
						});
						break;
					}
					if (enchantsToReroll == null)
					{
						break;
					}
					bool foundMatch = false;
					foreach (BaseEnchantment e in enchantsToReroll)
					{
						foreach (BaseEnchantment w_e in weapon.enchantments)
						{
							if (e.GetType().Equals(w_e.GetType()))
							{
								foundMatch = true;
								break;
							}
						}
						if (foundMatch)
						{
							break;
						}
					}
					if (!foundMatch)
					{
						break;
					}
					weapon.enchantments.RemoveWhere((BaseEnchantment enchantment) => enchantment.IsSecondaryEnchantment() && !(enchantment is GalaxySoulEnchantment));
				}
			}
			return item;
		}

		// Token: 0x0600185C RID: 6236 RVA: 0x0011C95C File Offset: 0x0011AB5C
		public override string getDescription()
		{
			if (!this.isScythe())
			{
				StringBuilder b = new StringBuilder();
				b.AppendLine(Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth()));
				b.AppendLine();
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14132", this.minDamage, this.maxDamage));
				if (this.speed.Value != 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14134", (this.speed.Value > 0) ? "+" : "-", Math.Abs(this.speed.Value)));
				}
				if (this.addedAreaOfEffect.Value > 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14136", this.addedAreaOfEffect));
				}
				if (this.addedPrecision.Value > 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14138", this.addedPrecision));
				}
				if (this.addedDefense.Value > 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14140", this.addedDefense));
				}
				if ((double)this.critChance.Value / 0.02 >= 2.0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14142", (int)((double)this.critChance.Value / 0.02)));
				}
				if ((double)(this.critMultiplier.Value - 3f) / 0.02 >= 1.0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14144", (int)((double)(this.critMultiplier.Value - 3f) / 0.02)));
				}
				if (this.knockback.Value != this.defaultKnockBackForThisType(this.type.Value))
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14140", (this.knockback.Value > this.defaultKnockBackForThisType(this.type.Value)) ? "+" : "", (int)Math.Ceiling((double)(Math.Abs(this.knockback.Value - this.defaultKnockBackForThisType(this.type.Value)) * 10f))));
				}
				return b.ToString();
			}
			return Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x0600185D RID: 6237 RVA: 0x0011CBF1 File Offset: 0x0011ADF1
		public virtual float defaultKnockBackForThisType(int type)
		{
			switch (type)
			{
			case 0:
			case 3:
				return 1f;
			case 1:
				return 0.5f;
			case 2:
				return 1.5f;
			default:
				return -1f;
			}
		}

		// Token: 0x0600185E RID: 6238 RVA: 0x0011CC24 File Offset: 0x0011AE24
		public virtual Rectangle getAreaOfEffect(int x, int y, int facingDirection, ref Vector2 tileLocation1, ref Vector2 tileLocation2, Rectangle wielderBoundingBox, int indexInCurrentAnimation)
		{
			Rectangle areaOfEffect = Rectangle.Empty;
			int width;
			int height;
			int upHeightOffset;
			int horizontalYOffset;
			if (this.type.Value == 1)
			{
				width = 74;
				height = 48;
				upHeightOffset = 42;
				horizontalYOffset = -32;
			}
			else
			{
				width = 64;
				height = 64;
				horizontalYOffset = -32;
				upHeightOffset = 0;
			}
			if (this.type.Value == 1)
			{
				switch (facingDirection)
				{
				case 0:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset, width / 2, height + upHeightOffset);
					tileLocation1 = new Vector2((float)(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64), (float)(areaOfEffect.Top / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(areaOfEffect.Top / 64));
					areaOfEffect.Offset(20, -16);
					areaOfEffect.Height += 16;
					areaOfEffect.Width += 20;
					break;
				case 1:
					areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset, (int)((float)height * 1.15f), width);
					tileLocation1 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(areaOfEffect.Center.Y / 64));
					areaOfEffect.Offset(-4, 0);
					areaOfEffect.Width += 16;
					break;
				case 2:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, (int)((float)height * 1.75f));
					tileLocation1 = new Vector2((float)(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64), (float)(areaOfEffect.Center.Y / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(areaOfEffect.Center.Y / 64));
					areaOfEffect.Offset(12, -8);
					areaOfEffect.Width -= 21;
					break;
				case 3:
					areaOfEffect = new Rectangle(wielderBoundingBox.Left - (int)((float)height * 1.15f), y - height / 2 + horizontalYOffset, (int)((float)height * 1.15f), width);
					tileLocation1 = new Vector2((float)(areaOfEffect.Left / 64), (float)(Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Left / 64), (float)(areaOfEffect.Center.Y / 64));
					areaOfEffect.Offset(-12, 0);
					areaOfEffect.Width += 16;
					break;
				}
			}
			else
			{
				switch (facingDirection)
				{
				case 0:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset, width, height + upHeightOffset);
					tileLocation1 = new Vector2((float)(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64), (float)(areaOfEffect.Top / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(areaOfEffect.Top / 64));
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(-60, -12);
						break;
					case 1:
						areaOfEffect.Offset(-48, -56);
						areaOfEffect.Height += 32;
						break;
					case 2:
						areaOfEffect.Offset(-12, -68);
						areaOfEffect.Height += 48;
						break;
					case 3:
						areaOfEffect.Offset(40, -60);
						areaOfEffect.Height += 48;
						break;
					case 4:
						areaOfEffect.Offset(56, -32);
						areaOfEffect.Height += 32;
						break;
					case 5:
						areaOfEffect.Offset(76, -32);
						break;
					}
					break;
				case 1:
					areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset, height, width);
					tileLocation1 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(areaOfEffect.Center.Y / 64));
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(-44, -84);
						break;
					case 1:
						areaOfEffect.Offset(4, -44);
						break;
					case 2:
						areaOfEffect.Offset(12, -4);
						break;
					case 3:
						areaOfEffect.Offset(12, 37);
						break;
					case 4:
						areaOfEffect.Offset(-28, 60);
						break;
					case 5:
						areaOfEffect.Offset(-60, 72);
						break;
					}
					break;
				case 2:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, (int)((float)height * 1.5f));
					tileLocation1 = new Vector2((float)(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64), (float)(areaOfEffect.Center.Y / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Center.X / 64), (float)(areaOfEffect.Center.Y / 64));
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(72, -92);
						break;
					case 1:
						areaOfEffect.Offset(56, -32);
						break;
					case 2:
						areaOfEffect.Offset(40, -28);
						break;
					case 3:
						areaOfEffect.Offset(-12, -8);
						break;
					case 4:
						areaOfEffect.Offset(-80, -24);
						areaOfEffect.Width += 32;
						break;
					case 5:
						areaOfEffect.Offset(-68, -44);
						break;
					}
					break;
				case 3:
					areaOfEffect = new Rectangle(wielderBoundingBox.Left - height, y - height / 2 + horizontalYOffset, height, width);
					tileLocation1 = new Vector2((float)(areaOfEffect.Left / 64), (float)(Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64));
					tileLocation2 = new Vector2((float)(areaOfEffect.Left / 64), (float)(areaOfEffect.Center.Y / 64));
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(56, -76);
						break;
					case 1:
						areaOfEffect.Offset(-8, -56);
						break;
					case 2:
						areaOfEffect.Offset(-16, -4);
						break;
					case 3:
						areaOfEffect.Offset(0, 37);
						break;
					case 4:
						areaOfEffect.Offset(24, 60);
						break;
					case 5:
						areaOfEffect.Offset(64, 64);
						break;
					}
					break;
				}
			}
			areaOfEffect.Inflate(this.addedAreaOfEffect.Value, this.addedAreaOfEffect.Value);
			return areaOfEffect;
		}

		// Token: 0x0600185F RID: 6239 RVA: 0x0011D362 File Offset: 0x0011B562
		public void triggerDefenseSwordFunction(Farmer who)
		{
			this.defenseSwordEvent.Fire();
		}

		// Token: 0x06001860 RID: 6240 RVA: 0x0011D36F File Offset: 0x0011B56F
		private void doDefenseSwordFunction()
		{
			this.isOnSpecial = false;
			this.lastUser.UsingTool = false;
			this.lastUser.CanMove = true;
			this.lastUser.FarmerSprite.PauseForSingleAnimation = false;
		}

		// Token: 0x06001861 RID: 6241 RVA: 0x0011D3A1 File Offset: 0x0011B5A1
		public void triggerDaggerFunction(Farmer who, int dagger_hits_left)
		{
			this.daggerEvent.Fire(dagger_hits_left);
		}

		// Token: 0x06001862 RID: 6242 RVA: 0x0011D3B0 File Offset: 0x0011B5B0
		private void doDaggerFunction(int dagger_hits)
		{
			Vector2 v = this.lastUser.getUniformPositionAwayFromBox(this.lastUser.FacingDirection, 48);
			int num = MeleeWeapon.daggerHitsLeft;
			MeleeWeapon.daggerHitsLeft = dagger_hits;
			this.DoDamage(Game1.currentLocation, (int)v.X, (int)v.Y, this.lastUser.FacingDirection, 1, this.lastUser);
			MeleeWeapon.daggerHitsLeft = num;
			if (this.lastUser != null && this.lastUser.IsLocalPlayer)
			{
				MeleeWeapon.daggerHitsLeft--;
			}
			this.isOnSpecial = false;
			this.lastUser.UsingTool = false;
			this.lastUser.CanMove = true;
			this.lastUser.FarmerSprite.PauseForSingleAnimation = false;
			if (MeleeWeapon.daggerHitsLeft > 0 && this.lastUser != null && this.lastUser.IsLocalPlayer)
			{
				this.quickStab(this.lastUser);
			}
		}

		// Token: 0x06001863 RID: 6243 RVA: 0x0011D48C File Offset: 0x0011B68C
		public void triggerClubFunction(Farmer who)
		{
			if (this.PlayUseSounds)
			{
				who.playNearbySoundAll("clubSmash", null, SoundContext.Default);
			}
			who.currentLocation.damageMonster(new Rectangle((int)who.Position.X - 192, who.GetBoundingBox().Y - 192, 384, 384), this.minDamage.Value, this.maxDamage.Value, false, 1.5f, 100, 0f, 1f, false, who, false);
			Game1.viewport.Y = Game1.viewport.Y - 21;
			Game1.viewport.X = Game1.viewport.X + Game1.random.Next(-32, 32);
			Vector2 v = who.getUniformPositionAwayFromBox(who.FacingDirection, 64);
			switch (who.FacingDirection)
			{
			case 0:
			case 2:
				v.X -= 32f;
				v.Y -= 32f;
				break;
			case 1:
				v.X -= 42f;
				v.Y -= 32f;
				break;
			case 3:
				v.Y -= 32f;
				break;
			}
			Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 128, 64, 64), 40f, 4, 0, v, false, who.FacingDirection == 1)
			});
			who.jitterStrength = 2f;
		}

		// Token: 0x06001864 RID: 6244 RVA: 0x0011D622 File Offset: 0x0011B822
		private void beginSpecialMove(Farmer who)
		{
			if (!Game1.fadeToBlack)
			{
				this.isOnSpecial = true;
				who.UsingTool = true;
				who.CanMove = false;
			}
		}

		// Token: 0x06001865 RID: 6245 RVA: 0x0011D640 File Offset: 0x0011B840
		private void quickStab(Farmer who)
		{
			AnimatedSprite.endOfAnimationBehavior endOfAnimFunc = delegate(Farmer f)
			{
				this.triggerDaggerFunction(f, MeleeWeapon.daggerHitsLeft);
			};
			if (!who.IsLocalPlayer)
			{
				endOfAnimFunc = null;
			}
			switch (who.FacingDirection)
			{
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(276, 15f, 2, endOfAnimFunc);
				this.Update(0, 0, who);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(274, 15f, 2, endOfAnimFunc);
				this.Update(1, 0, who);
				break;
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(272, 15f, 2, endOfAnimFunc);
				this.Update(2, 0, who);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(278, 15f, 2, endOfAnimFunc);
				this.Update(3, 0, who);
				break;
			}
			this.FireProjectile(who);
			this.beginSpecialMove(who);
			if (this.PlayUseSounds)
			{
				who.playNearbySoundLocal("daggerswipe", null, SoundContext.Default);
			}
		}

		// Token: 0x06001866 RID: 6246 RVA: 0x0011D74C File Offset: 0x0011B94C
		protected virtual int specialCooldown()
		{
			switch (this.type.Value)
			{
			case 0:
				return MeleeWeapon.attackSwordCooldown;
			case 1:
				return MeleeWeapon.daggerCooldown;
			case 2:
				return MeleeWeapon.clubCooldown;
			case 3:
				return MeleeWeapon.defenseCooldown;
			default:
				return 0;
			}
		}

		// Token: 0x06001867 RID: 6247 RVA: 0x0011D798 File Offset: 0x0011B998
		public virtual void animateSpecialMove(Farmer who)
		{
			this.lastUser = who;
			if ((this.type.Value == 3 && (this.Name.Contains("Scythe") || this.isScythe())) || Game1.fadeToBlack)
			{
				return;
			}
			if (this.specialCooldown() <= 0)
			{
				this.animateSpecialMoveEvent.Fire();
			}
		}

		// Token: 0x06001868 RID: 6248 RVA: 0x0011D7F0 File Offset: 0x0011B9F0
		protected virtual void doAnimateSpecialMove()
		{
			if (this.lastUser == null || this.lastUser.CurrentTool != this)
			{
				return;
			}
			if (this.lastUser.isEmoteAnimating)
			{
				this.lastUser.EndEmoteAnimation();
			}
			switch (this.type.Value)
			{
			case 1:
				MeleeWeapon.daggerHitsLeft = 4;
				this.quickStab(this.lastUser);
				if (this.lastUser.IsLocalPlayer)
				{
					MeleeWeapon.daggerCooldown = 3000;
				}
				if (this.lastUser.professions.Contains(28))
				{
					MeleeWeapon.daggerCooldown /= 2;
				}
				if (base.hasEnchantmentOfType<ArtfulEnchantment>())
				{
					MeleeWeapon.daggerCooldown /= 2;
				}
				break;
			case 2:
			{
				AnimatedSprite.endOfAnimationBehavior endOfAnimFunc = new AnimatedSprite.endOfAnimationBehavior(this.triggerClubFunction);
				if (!this.lastUser.IsLocalPlayer)
				{
					endOfAnimFunc = null;
				}
				if (this.PlayUseSounds)
				{
					this.lastUser.playNearbySoundLocal("clubswipe", null, SoundContext.Default);
				}
				switch (this.lastUser.FacingDirection)
				{
				case 0:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(176, 40f, 8, endOfAnimFunc);
					this.Update(0, 0, this.lastUser);
					break;
				case 1:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(168, 40f, 8, endOfAnimFunc);
					this.Update(1, 0, this.lastUser);
					break;
				case 2:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(160, 40f, 8, endOfAnimFunc);
					this.Update(2, 0, this.lastUser);
					break;
				case 3:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(184, 40f, 8, endOfAnimFunc);
					this.Update(3, 0, this.lastUser);
					break;
				}
				this.beginSpecialMove(this.lastUser);
				if (this.lastUser.IsLocalPlayer)
				{
					MeleeWeapon.clubCooldown = 6000;
				}
				if (this.lastUser.professions.Contains(28))
				{
					MeleeWeapon.clubCooldown /= 2;
				}
				if (base.hasEnchantmentOfType<ArtfulEnchantment>())
				{
					MeleeWeapon.clubCooldown /= 2;
					return;
				}
				break;
			}
			case 3:
			{
				AnimatedSprite.endOfAnimationBehavior endOfAnimFunc2 = new AnimatedSprite.endOfAnimationBehavior(this.triggerDefenseSwordFunction);
				if (!this.lastUser.IsLocalPlayer)
				{
					endOfAnimFunc2 = null;
				}
				switch (this.lastUser.FacingDirection)
				{
				case 0:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(252, 500f, 1, endOfAnimFunc2);
					this.Update(0, 0, this.lastUser);
					break;
				case 1:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(243, 500f, 1, endOfAnimFunc2);
					this.Update(1, 0, this.lastUser);
					break;
				case 2:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(234, 500f, 1, endOfAnimFunc2);
					this.Update(2, 0, this.lastUser);
					break;
				case 3:
					((FarmerSprite)this.lastUser.Sprite).animateOnce(259, 500f, 1, endOfAnimFunc2);
					this.Update(3, 0, this.lastUser);
					break;
				}
				if (this.PlayUseSounds)
				{
					this.lastUser.playNearbySoundLocal("batFlap", null, SoundContext.Default);
				}
				this.beginSpecialMove(this.lastUser);
				if (this.lastUser.IsLocalPlayer)
				{
					MeleeWeapon.defenseCooldown = 1500;
				}
				if (this.lastUser.professions.Contains(28))
				{
					MeleeWeapon.defenseCooldown /= 2;
				}
				if (base.hasEnchantmentOfType<ArtfulEnchantment>())
				{
					MeleeWeapon.defenseCooldown /= 2;
					return;
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06001869 RID: 6249 RVA: 0x0011DBB8 File Offset: 0x0011BDB8
		public void doSwipe(int type, Vector2 position, int facingDirection, float swipeSpeed, Farmer f)
		{
			if (f == null || f.CurrentTool != this)
			{
				return;
			}
			if (f.IsLocalPlayer)
			{
				f.TemporaryPassableTiles.Clear();
				f.currentLocation.lastTouchActionLocation = Vector2.Zero;
			}
			swipeSpeed *= 1.3f;
			if (type != 2)
			{
				if (type == 3)
				{
					if (f.CurrentTool == this)
					{
						switch (f.FacingDirection)
						{
						case 0:
							((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 6);
							this.Update(0, 0, f);
							break;
						case 1:
							((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 6);
							this.Update(1, 0, f);
							break;
						case 2:
							((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 6);
							this.Update(2, 0, f);
							break;
						case 3:
							((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 6);
							this.Update(3, 0, f);
							break;
						}
					}
					if (this.PlayUseSounds && f.ShouldHandleAnimationSound())
					{
						f.playNearbySoundLocal("swordswipe", null, SoundContext.Default);
						return;
					}
				}
			}
			else
			{
				if (f.CurrentTool == this)
				{
					switch (f.FacingDirection)
					{
					case 0:
						((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 8);
						this.Update(0, 0, f);
						break;
					case 1:
						((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 8);
						this.Update(1, 0, f);
						break;
					case 2:
						((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 8);
						this.Update(2, 0, f);
						break;
					case 3:
						((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 8);
						this.Update(3, 0, f);
						break;
					}
				}
				if (this.PlayUseSounds)
				{
					f.playNearbySoundLocal("clubswipe", null, SoundContext.Default);
				}
			}
		}

		// Token: 0x0600186A RID: 6250 RVA: 0x0011DDE4 File Offset: 0x0011BFE4
		public virtual void FireProjectile(Farmer who)
		{
			WeaponData weaponData = this.cachedData;
			if (((weaponData != null) ? weaponData.Projectiles : null) == null)
			{
				return;
			}
			foreach (WeaponProjectile data in this.cachedData.Projectiles)
			{
				float shotAngle = 0f;
				float angleOffsetMultiplier = 1f;
				switch (who.facingDirection.Value)
				{
				case 0:
					shotAngle = 90f;
					break;
				case 1:
					shotAngle = 0f;
					break;
				case 2:
					shotAngle = 270f;
					break;
				case 3:
					shotAngle = 180f;
					angleOffsetMultiplier = -1f;
					break;
				}
				shotAngle += (data.MinAngleOffset + (float)Game1.random.NextDouble() * (data.MaxAngleOffset - data.MinAngleOffset)) * angleOffsetMultiplier;
				shotAngle *= 0.017453292f;
				string shotItemId = null;
				if (data.Item != null)
				{
					ISpawnItemData item = data.Item;
					GameLocation currentLocation = who.currentLocation;
					Random random = null;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 2);
					defaultInterpolatedStringHandler.AppendLiteral("weapon '");
					defaultInterpolatedStringHandler.AppendFormatted(base.QualifiedItemId);
					defaultInterpolatedStringHandler.AppendLiteral("' > projectile data '");
					defaultInterpolatedStringHandler.AppendFormatted(data.Id);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					Item item2 = ItemQueryResolver.TryResolveRandomItem(item, new ItemQueryContext(currentLocation, who, random, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, null, null, null);
					shotItemId = ((item2 != null) ? item2.QualifiedItemId : null);
					if (shotItemId == null)
					{
						continue;
					}
				}
				Vector2 shotOrigin = who.getStandingPosition() - new Vector2(32f, 32f);
				int damage = data.Damage;
				int spriteIndex = data.SpriteIndex;
				int bounces = data.Bounces;
				int tailLength = data.TailLength;
				float rotationVelocity = (float)data.RotationVelocity * 0.017453292f;
				float xVelocity = (float)data.Velocity * (float)Math.Cos((double)shotAngle);
				float yVelocity = (float)data.Velocity * (float)(-(float)Math.Sin((double)shotAngle));
				Vector2 startingPosition = shotOrigin;
				string fireSound = data.FireSound;
				BasicProjectile projectile = new BasicProjectile(damage, spriteIndex, bounces, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, data.CollisionSound, data.BounceSound, fireSound, data.Explodes, true, who.currentLocation, who, null, shotItemId);
				projectile.ignoreTravelGracePeriod.Value = true;
				projectile.ignoreMeleeAttacks.Value = true;
				projectile.maxTravelDistance.Value = data.MaxDistance * 64;
				projectile.height.Value = 32f;
				who.currentLocation.projectiles.Add(projectile);
			}
		}

		// Token: 0x0600186B RID: 6251 RVA: 0x0011E054 File Offset: 0x0011C254
		public virtual void setFarmerAnimating(Farmer who)
		{
			this.anotherClick = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.FarmerSprite.StopAnimation();
			this.swipeSpeed = (float)(400 - this.speed.Value * 40) - who.addedSpeed * 40f;
			this.swipeSpeed *= 1f - who.buffs.WeaponSpeedMultiplier;
			if (who.IsLocalPlayer)
			{
				foreach (BaseEnchantment baseEnchantment in this.enchantments)
				{
					BaseWeaponEnchantment weaponEnchantment = baseEnchantment as BaseWeaponEnchantment;
					if (weaponEnchantment != null)
					{
						weaponEnchantment.OnSwing(this, who);
					}
				}
				this.FireProjectile(who);
			}
			if (this.type.Value != 1)
			{
				this.doSwipe(this.type.Value, who.Position, who.FacingDirection, this.swipeSpeed / (float)((this.type.Value == 2) ? 5 : 8), who);
				who.lastClick = Vector2.Zero;
				Vector2 actionTile = who.GetToolLocation(true);
				this.DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
			}
			else
			{
				if (this.PlayUseSounds && who.IsLocalPlayer)
				{
					who.playNearbySoundAll("daggerswipe", null, SoundContext.Default);
				}
				this.swipeSpeed /= 4f;
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(276, this.swipeSpeed, 2);
					this.Update(0, 0, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(274, this.swipeSpeed, 2);
					this.Update(1, 0, who);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(272, this.swipeSpeed, 2);
					this.Update(2, 0, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(278, this.swipeSpeed, 2);
					this.Update(3, 0, who);
					break;
				}
				Vector2 actionTile2 = who.GetToolLocation(true);
				this.DoDamage(who.currentLocation, (int)actionTile2.X, (int)actionTile2.Y, who.FacingDirection, 1, who);
			}
			if (who.CurrentTool == null)
			{
				who.completelyStopAnimatingOrDoingAction();
				who.forceCanMove();
			}
		}

		// Token: 0x0600186C RID: 6252 RVA: 0x0011E2D8 File Offset: 0x0011C4D8
		public override void actionWhenStopBeingHeld(Farmer who)
		{
			who.UsingTool = false;
			this.anotherClick = false;
			base.actionWhenStopBeingHeld(who);
		}

		// Token: 0x0600186D RID: 6253 RVA: 0x0011E2F0 File Offset: 0x0011C4F0
		public virtual void RecalculateAppliedForges(bool force = false)
		{
			if (this.enchantments.Count == 0 && !force)
			{
				return;
			}
			foreach (BaseEnchantment enchantment in this.enchantments)
			{
				if (enchantment.IsForge())
				{
					enchantment.UnapplyTo(this, null);
				}
			}
			WeaponData data = this.GetData();
			if (data != null)
			{
				this.Name = data.Name;
				this.minDamage.Value = data.MinDamage;
				this.maxDamage.Value = data.MaxDamage;
				this.knockback.Value = data.Knockback;
				this.speed.Value = data.Speed;
				this.addedPrecision.Value = data.Precision;
				this.addedDefense.Value = data.Defense;
				this.type.Value = data.Type;
				this.addedAreaOfEffect.Value = data.AreaOfEffect;
				this.critChance.Value = data.CritChance;
				this.critMultiplier.Value = data.CritMultiplier;
				if (this.type.Value == 0)
				{
					this.type.Value = 3;
				}
			}
			foreach (BaseEnchantment enchantment2 in this.enchantments)
			{
				if (enchantment2.IsForge())
				{
					enchantment2.ApplyTo(this, null);
				}
			}
		}

		// Token: 0x0600186E RID: 6254 RVA: 0x0011E484 File Offset: 0x0011C684
		public virtual void DoDamage(GameLocation location, int x, int y, int facingDirection, int power, Farmer who)
		{
			if (!who.IsLocalPlayer)
			{
				return;
			}
			this.isOnSpecial = false;
			if (this.type.Value != 2)
			{
				base.DoFunction(location, x, y, power, who);
			}
			this.lastUser = who;
			Vector2 tileLocation = Vector2.Zero;
			Vector2 tileLocation2 = Vector2.Zero;
			Rectangle areaOfEffect = this.getAreaOfEffect(x, y, facingDirection, ref tileLocation, ref tileLocation2, who.GetBoundingBox(), who.FarmerSprite.currentAnimationIndex);
			this.mostRecentArea = areaOfEffect;
			float effectiveCritChance = this.critChance.Value;
			if (this.type.Value == 1)
			{
				effectiveCritChance += 0.005f;
				effectiveCritChance *= 1.12f;
			}
			if (location.damageMonster(areaOfEffect, (int)((float)this.minDamage.Value * (1f + who.buffs.AttackMultiplier)), (int)((float)this.maxDamage.Value * (1f + who.buffs.AttackMultiplier)), false, this.knockback.Value * (1f + who.buffs.KnockbackMultiplier), (int)((float)this.addedPrecision.Value * (1f + who.buffs.WeaponPrecisionMultiplier)), effectiveCritChance * (1f + who.buffs.CriticalChanceMultiplier), this.critMultiplier.Value * (1f + who.buffs.CriticalPowerMultiplier), this.type.Value != 1 || !this.isOnSpecial, who, false) && this.type.Value == 2 && this.PlayUseSounds)
			{
				who.playNearbySoundAll("clubhit", null, SoundContext.Default);
			}
			string soundToPlay = "";
			location.projectiles.RemoveWhere(delegate(Projectile projectile)
			{
				if (areaOfEffect.Intersects(projectile.getBoundingBox()) && !projectile.ignoreMeleeAttacks.Value)
				{
					projectile.behaviorOnCollisionWithOther(location);
				}
				return projectile.destroyMe;
			});
			foreach (Vector2 v in Utility.removeDuplicates(Utility.getListOfTileLocationsForBordersOfNonTileRectangle(areaOfEffect)))
			{
				TerrainFeature terrainFeature;
				if (location.terrainFeatures.TryGetValue(v, out terrainFeature) && terrainFeature.performToolAction(this, 0, v))
				{
					location.terrainFeatures.Remove(v);
				}
				Object obj;
				if (location.objects.TryGetValue(v, out obj) && obj.performToolAction(this))
				{
					location.objects.Remove(v);
				}
				if (location.performToolAction(this, (int)v.X, (int)v.Y))
				{
					break;
				}
			}
			if (this.PlayUseSounds && !soundToPlay.Equals(""))
			{
				Game1.playSound(soundToPlay, null);
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
			if (who != null && who.isRidingHorse())
			{
				who.completelyStopAnimatingOrDoingAction();
			}
		}

		// Token: 0x0600186F RID: 6255 RVA: 0x0011E794 File Offset: 0x0011C994
		public string GetDrawnItemId()
		{
			return this.appearance.Value ?? base.QualifiedItemId;
		}

		// Token: 0x06001870 RID: 6256 RVA: 0x0011E7AC File Offset: 0x0011C9AC
		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			y += (int)font.MeasureString(Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth())).Y;
			if (!this.isScythe())
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Color co = Game1.textColor;
				if (base.hasEnchantmentOfType<RubyEnchantment>())
				{
					co = new Color(0, 120, 120);
				}
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", this.minDamage, this.maxDamage), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), co * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				if (this.speed.Value != ((this.type.Value == 2) ? -8 : 0))
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(130, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
					bool negativeSpeed = (this.type.Value == 2 && this.speed.Value < -8) || (this.type.Value != 2 && this.speed.Value < 0);
					Color c = Game1.textColor;
					if (base.hasEnchantmentOfType<EmeraldEnchantment>())
					{
						c = new Color(0, 120, 120);
					}
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Speed", ((((this.type.Value == 2) ? (this.speed.Value - -8) : this.speed.Value) > 0) ? "+" : "") + (((this.type.Value == 2) ? (this.speed.Value - -8) : this.speed.Value) / 2).ToString()), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), negativeSpeed ? Color.DarkRed : (c * 0.9f * alpha), 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if (this.addedDefense.Value > 0)
				{
					Color c2 = Game1.textColor;
					if (base.hasEnchantmentOfType<TopazEnchantment>())
					{
						c2 = new Color(0, 120, 120);
					}
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", this.addedDefense), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), c2 * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				float effectiveCritChance = this.critChance.Value;
				if (this.type.Value == 1)
				{
					effectiveCritChance += 0.005f;
					effectiveCritChance *= 1.12f;
				}
				if ((double)effectiveCritChance / 0.02 >= 1.100000023841858)
				{
					Color c3 = Game1.textColor;
					if (base.hasEnchantmentOfType<AquamarineEnchantment>())
					{
						c3 = new Color(0, 120, 120);
					}
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(40, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", (int)Math.Round((double)(effectiveCritChance - 0.001f) / 0.02)), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), c3 * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if ((double)(this.critMultiplier.Value - 3f) / 0.02 >= 1.0)
				{
					Color c4 = Game1.textColor;
					if (base.hasEnchantmentOfType<JadeEnchantment>())
					{
						c4 = new Color(0, 120, 120);
					}
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16), (float)(y + 16 + 4)), new Rectangle(160, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", (int)((double)(this.critMultiplier.Value - 3f) / 0.02)), font, new Vector2((float)(x + 16 + 44), (float)(y + 16 + 12)), c4 * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if (this.knockback.Value != this.defaultKnockBackForThisType(this.type.Value))
				{
					Color c5 = Game1.textColor;
					if (base.hasEnchantmentOfType<AmethystEnchantment>())
					{
						c5 = new Color(0, 120, 120);
					}
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(70, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Weight", (((float)((int)Math.Ceiling((double)(Math.Abs(this.knockback.Value - this.defaultKnockBackForThisType(this.type.Value)) * 10f))) > this.defaultKnockBackForThisType(this.type.Value)) ? "+" : "") + ((int)Math.Ceiling((double)(Math.Abs(this.knockback.Value - this.defaultKnockBackForThisType(this.type.Value)) * 10f))).ToString()), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), c5 * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if (this.enchantments.Count > 0 && this.enchantments[this.enchantments.Count - 1] is DiamondEnchantment)
				{
					Color c6 = new Color(0, 120, 120);
					int random_forges = this.GetMaxForges() - base.GetTotalForgeLevels(false);
					string random_forge_string = (random_forges == 1) ? Game1.content.LoadString("Strings\\UI:ItemHover_DiamondForge_Singular", random_forges) : Game1.content.LoadString("Strings\\UI:ItemHover_DiamondForge_Plural", random_forges);
					Utility.drawTextWithShadow(spriteBatch, random_forge_string, font, new Vector2((float)(x + 16), (float)(y + 16 + 12)), c6 * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				foreach (BaseEnchantment enchantment in this.enchantments)
				{
					if (enchantment.ShouldBeDisplayed())
					{
						Color c7 = new Color(120, 0, 210);
						if (enchantment.IsSecondaryEnchantment())
						{
							Utility.drawWithShadow(spriteBatch, Game1.mouseCursors_1_6, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(502, 430, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
							c7 = new Color(120, 50, 100);
						}
						else
						{
							Utility.drawWithShadow(spriteBatch, Game1.mouseCursors2, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
						}
						Utility.drawTextWithShadow(spriteBatch, ((BaseEnchantment.hideEnchantmentName && !enchantment.IsSecondaryEnchantment()) || (BaseEnchantment.hideSecondaryEnchantName && enchantment.IsSecondaryEnchantment())) ? "???" : enchantment.GetDisplayName(), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), c7 * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					}
				}
			}
		}

		// Token: 0x06001871 RID: 6257 RVA: 0x0011F274 File Offset: 0x0011D474
		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			int maxStat = 9999;
			Point dimensions = new Point(0, 0);
			dimensions.Y += Math.Max(60, (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f) + 32) + (int)font.MeasureString("T").Y + (int)((moneyAmountToDisplayAtBottom > -1) ? (font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").Y + 4f) : 0f);
			dimensions.Y += (this.isScythe() ? 0 : (this.getNumberOfDescriptionCategories() * 4 * 12));
			dimensions.Y += (int)font.MeasureString(Game1.parseText(base.description, Game1.smallFont, this.getDescriptionWidth())).Y;
			dimensions.X = (int)Math.Max((float)minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Damage", maxStat, maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Speed", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Weight", maxStat)).X + (float)horizontalBuffer))))));
			if (this.enchantments.Count > 0 && this.enchantments[this.enchantments.Count - 1] is DiamondEnchantment)
			{
				dimensions.X = (int)Math.Max((float)dimensions.X, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DiamondForge_Plural", this.GetMaxForges())).X);
			}
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

		// Token: 0x06001872 RID: 6258 RVA: 0x0011F52C File Offset: 0x0011D72C
		public virtual void ResetIndexOfMenuItemView()
		{
			base.IndexOfMenuItemView = base.InitialParentTileIndex;
		}

		// Token: 0x06001873 RID: 6259 RVA: 0x0011F53A File Offset: 0x0011D73A
		public virtual void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f)
		{
			MeleeWeapon.drawDuringUse(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, this.GetDrawnItemId(), this.type.Value, this.isOnSpecial);
		}

		// Token: 0x06001874 RID: 6260 RVA: 0x0011F560 File Offset: 0x0011D760
		public override bool CanForge(Item item)
		{
			MeleeWeapon other_weapon = item as MeleeWeapon;
			return (other_weapon != null && other_weapon.type.Value == this.type.Value) || base.CanForge(item);
		}

		// Token: 0x06001875 RID: 6261 RVA: 0x0011F598 File Offset: 0x0011D798
		public override bool CanAddEnchantment(BaseEnchantment enchantment)
		{
			return (!(enchantment is GalaxySoulEnchantment) || this.isGalaxyWeapon()) && base.CanAddEnchantment(enchantment);
		}

		// Token: 0x06001876 RID: 6262 RVA: 0x0011F5B3 File Offset: 0x0011D7B3
		public bool isGalaxyWeapon()
		{
			return base.QualifiedItemId == "(W)4" || base.QualifiedItemId == "(W)23" || base.QualifiedItemId == "(W)29";
		}

		// Token: 0x06001877 RID: 6263 RVA: 0x0011F5EB File Offset: 0x0011D7EB
		public void transform(string newItemId)
		{
			base.ItemId = newItemId;
			this.appearance.Value = null;
			this.RecalculateAppliedForges(true);
		}

		// Token: 0x06001878 RID: 6264 RVA: 0x0011F608 File Offset: 0x0011D808
		public override bool Forge(Item item, bool count_towards_stats = false)
		{
			if (this.isScythe())
			{
				return false;
			}
			MeleeWeapon other_weapon = item as MeleeWeapon;
			if (other_weapon != null && other_weapon.type.Value == this.type.Value)
			{
				this.appearance.Value = other_weapon.QualifiedItemId;
				return true;
			}
			return base.Forge(item, count_towards_stats);
		}

		// Token: 0x06001879 RID: 6265 RVA: 0x0011F65C File Offset: 0x0011D85C
		public static void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, string weaponItemId, int type, bool isOnSpecial)
		{
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(weaponItemId);
			Texture2D texture = dataOrErrorItem.GetTexture() ?? Tool.weaponsTexture;
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
			float baseSortLayer = f.getDrawLayer();
			int facingDirection2 = f.FacingDirection;
			FarmerRenderer.FarmerSpriteLayers weaponSortLayer;
			if (facingDirection2 != 0)
			{
				if (facingDirection2 != 2)
				{
					weaponSortLayer = FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE;
				}
				else
				{
					weaponSortLayer = FarmerRenderer.FarmerSpriteLayers.ToolDown;
				}
			}
			else
			{
				weaponSortLayer = FarmerRenderer.FarmerSpriteLayers.ToolUp;
			}
			float sortBehindLayer = FarmerRenderer.GetLayerDepth(baseSortLayer, FarmerRenderer.FarmerSpriteLayers.ToolUp, false);
			float sortLayer = FarmerRenderer.GetLayerDepth(baseSortLayer, weaponSortLayer, false);
			if (type != 1)
			{
				if (isOnSpecial)
				{
					if (type != 2)
					{
						if (type == 3)
						{
							switch (f.FacingDirection)
							{
							case 0:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 44f), new Rectangle?(sourceRect), Color.White, -1.7671459f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
								return;
							case 1:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 4f), new Rectangle?(sourceRect), Color.White, -0.5890486f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
								return;
							case 2:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 52f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
								return;
							case 3:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 56f, playerPosition.Y - 4f), new Rectangle?(sourceRect), Color.White, 0.5890486f, new Vector2(15f, 15f), 4f, SpriteEffects.FlipHorizontally, sortLayer);
								return;
							default:
								return;
							}
						}
					}
					else if (facingDirection != 1)
					{
						if (facingDirection != 3)
						{
							switch (frameOfFarmerAnimation)
							{
							case 0:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 24f, playerPosition.Y - 21f - 8f - 64f), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								break;
							case 1:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f - 64f + 4f), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								break;
							case 2:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 20f - 64f), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								break;
							case 3:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), new Rectangle?(sourceRect), Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								}
								else
								{
									spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 32f - 64f), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								}
								break;
							case 4:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), new Rectangle?(sourceRect), Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								}
								break;
							case 5:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f - 20f), new Rectangle?(sourceRect), Color.White, 2.3561945f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								}
								break;
							case 6:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 54f), new Rectangle?(sourceRect), Color.White, 2.3561945f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								}
								break;
							case 7:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 58f), new Rectangle?(sourceRect), Color.White, 2.3561945f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								}
								break;
							}
							if (f.FacingDirection == 0)
							{
								f.FarmerRenderer.draw(spriteBatch, f.FarmerSprite, f.FarmerSprite.SourceRect, f.getLocalPosition(Game1.viewport), new Vector2(0f, (f.yOffset + 128f - (float)(f.GetBoundingBox().Height / 2)) / 4f + 4f), sortLayer, Color.White, 0f, f);
								return;
							}
						}
						else
						{
							switch (frameOfFarmerAnimation)
							{
							case 0:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f + 8f, playerPosition.Y - 56f - 64f), new Rectangle?(sourceRect), Color.White, 0.3926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 1:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f, playerPosition.Y - 32f), new Rectangle?(sourceRect), Color.White, -1.9634955f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 2:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 12f, playerPosition.Y + 8f), new Rectangle?(sourceRect), Color.White, -2.7488937f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 3:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f - 4f, playerPosition.Y + 8f), new Rectangle?(sourceRect), Color.White, -2.3561945f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 4:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f - 24f, playerPosition.Y + 64f + 12f - 64f), new Rectangle?(sourceRect), Color.White, 4.31969f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 5:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 20f, playerPosition.Y + 64f + 40f - 64f), new Rectangle?(sourceRect), Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 6:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y + 64f + 56f), new Rectangle?(sourceRect), Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							case 7:
								spriteBatch.Draw(texture, new Vector2(playerPosition.X - 8f, playerPosition.Y + 64f + 64f), new Rectangle?(sourceRect), Color.White, 3.7306414f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
								return;
							default:
								return;
							}
						}
					}
					else
					{
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f - 12f, playerPosition.Y - 80f), new Rectangle?(sourceRect), Color.White, -1.1780972f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 64f - 48f), new Rectangle?(sourceRect), Color.White, 0.3926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 128f - 16f, playerPosition.Y - 64f - 12f), new Rectangle?(sourceRect), Color.White, 1.1780972f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 72f, playerPosition.Y - 64f + 16f - 32f), new Rectangle?(sourceRect), Color.White, 0.3926991f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f, playerPosition.Y - 64f + 16f - 16f), new Rectangle?(sourceRect), Color.White, 0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 12f, playerPosition.Y - 64f + 16f), new Rectangle?(sourceRect), Color.White, 0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 16f, playerPosition.Y - 64f + 40f - 8f), new Rectangle?(sourceRect), Color.White, 0.7853982f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 8f, playerPosition.Y + 40f), new Rectangle?(sourceRect), Color.White, 0.98174775f, Vector2.Zero, 4f, SpriteEffects.None, sortLayer);
							return;
						default:
							return;
						}
					}
				}
				else
				{
					switch (facingDirection)
					{
					case 0:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), new Rectangle?(sourceRect), Color.White, -2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), new Rectangle?(sourceRect), Color.White, -1.5707964f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), new Rectangle?(sourceRect), Color.White, -1.1780972f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), new Rectangle?(sourceRect), Color.White, -0.3926991f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 44f, playerPosition.Y + 64f), new Rectangle?(sourceRect), Color.White, -1.9634954f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						default:
							return;
						}
						break;
					case 1:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 40f, playerPosition.Y - 64f + 8f), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.None, sortBehindLayer);
							return;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 64f + 28f), new Rectangle?(sourceRect), Color.White, 0f, MeleeWeapon.center, 4f, SpriteEffects.None, sortBehindLayer);
							return;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 4f), new Rectangle?(sourceRect), Color.White, 1.5707964f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 28f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, 1.9634955f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, 2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, 2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f), new Rectangle?(sourceRect), Color.White, 1.9634954f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						default:
							return;
						}
						break;
					case 2:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), new Rectangle?(sourceRect), Color.White, 1.5707964f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 1.5707964f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, 2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), new Rectangle?(sourceRect), Color.White, 3.1415927f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 3.5342917f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 3.5342917f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 64f), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
							return;
						default:
							return;
						}
						break;
					case 3:
						switch (frameOfFarmerAnimation)
						{
						case 0:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 64f - 16f), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortBehindLayer);
							return;
						case 1:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 48f, playerPosition.Y - 64f + 20f), new Rectangle?(sourceRect), Color.White, 0f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortBehindLayer);
							return;
						case 2:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 64f + 32f, playerPosition.Y + 16f), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
							return;
						case 3:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 4f, playerPosition.Y + 44f), new Rectangle?(sourceRect), Color.White, -1.5707964f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
							return;
						case 4:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 52f), new Rectangle?(sourceRect), Color.White, -1.9634955f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
							return;
						case 5:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), new Rectangle?(sourceRect), Color.White, -2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
							return;
						case 6:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), new Rectangle?(sourceRect), Color.White, -2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.FlipHorizontally, sortLayer);
							return;
						case 7:
							spriteBatch.Draw(texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon.center, 4f, SpriteEffects.FlipVertically, sortLayer);
							return;
						default:
							return;
						}
						break;
					default:
						return;
					}
				}
			}
			else
			{
				frameOfFarmerAnimation %= 2;
				switch (facingDirection)
				{
				case 0:
					if (frameOfFarmerAnimation == 0)
					{
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						return;
					}
					if (frameOfFarmerAnimation != 1)
					{
						return;
					}
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 48f), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					return;
				case 1:
					if (frameOfFarmerAnimation == 0)
					{
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						return;
					}
					if (frameOfFarmerAnimation != 1)
					{
						return;
					}
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 24f), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					return;
				case 2:
					if (frameOfFarmerAnimation == 0)
					{
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 8f), new Rectangle?(sourceRect), Color.White, 2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						return;
					}
					if (frameOfFarmerAnimation != 1)
					{
						return;
					}
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 21f, playerPosition.Y + 20f), new Rectangle?(sourceRect), Color.White, 2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					break;
				case 3:
					if (frameOfFarmerAnimation == 0)
					{
						spriteBatch.Draw(texture, new Vector2(playerPosition.X + 16f, playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, -2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
						return;
					}
					if (frameOfFarmerAnimation != 1)
					{
						return;
					}
					spriteBatch.Draw(texture, new Vector2(playerPosition.X + 8f, playerPosition.Y - 24f), new Rectangle?(sourceRect), Color.White, -2.3561945f, MeleeWeapon.center, 4f, SpriteEffects.None, sortLayer);
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x04000EB6 RID: 3766
		public const int defenseCooldownTime = 1500;

		// Token: 0x04000EB7 RID: 3767
		public const int daggerCooldownTime = 3000;

		// Token: 0x04000EB8 RID: 3768
		public const int clubCooldownTime = 6000;

		// Token: 0x04000EB9 RID: 3769
		public const int millisecondsPerSpeedPoint = 40;

		// Token: 0x04000EBA RID: 3770
		public const int defaultSpeed = 400;

		// Token: 0x04000EBB RID: 3771
		public const int stabbingSword = 0;

		// Token: 0x04000EBC RID: 3772
		public const int dagger = 1;

		// Token: 0x04000EBD RID: 3773
		public const int club = 2;

		// Token: 0x04000EBE RID: 3774
		public const int defenseSword = 3;

		// Token: 0x04000EBF RID: 3775
		public const int baseClubSpeed = -8;

		// Token: 0x04000EC0 RID: 3776
		public const string scytheId = "47";

		// Token: 0x04000EC1 RID: 3777
		public const string goldenScytheId = "53";

		// Token: 0x04000EC2 RID: 3778
		public const string iridiumScytheID = "66";

		// Token: 0x04000EC3 RID: 3779
		public const string galaxySwordId = "4";

		// Token: 0x04000EC4 RID: 3780
		public const int MAX_FORGES = 3;

		// Token: 0x04000EC5 RID: 3781
		[XmlElement("type")]
		public readonly NetInt type = new NetInt();

		// Token: 0x04000EC6 RID: 3782
		[XmlElement("minDamage")]
		public readonly NetInt minDamage = new NetInt();

		// Token: 0x04000EC7 RID: 3783
		[XmlElement("maxDamage")]
		public readonly NetInt maxDamage = new NetInt();

		// Token: 0x04000EC8 RID: 3784
		[XmlElement("speed")]
		public readonly NetInt speed = new NetInt();

		// Token: 0x04000EC9 RID: 3785
		[XmlElement("addedPrecision")]
		public readonly NetInt addedPrecision = new NetInt();

		// Token: 0x04000ECA RID: 3786
		[XmlElement("addedDefense")]
		public readonly NetInt addedDefense = new NetInt();

		// Token: 0x04000ECB RID: 3787
		[XmlElement("addedAreaOfEffect")]
		public readonly NetInt addedAreaOfEffect = new NetInt();

		// Token: 0x04000ECC RID: 3788
		[XmlElement("knockback")]
		public readonly NetFloat knockback = new NetFloat();

		// Token: 0x04000ECD RID: 3789
		[XmlElement("critChance")]
		public readonly NetFloat critChance = new NetFloat();

		// Token: 0x04000ECE RID: 3790
		[XmlElement("critMultiplier")]
		public readonly NetFloat critMultiplier = new NetFloat();

		// Token: 0x04000ECF RID: 3791
		[XmlElement("appearance")]
		public readonly NetString appearance = new NetString(null);

		// Token: 0x04000ED0 RID: 3792
		public bool isOnSpecial;

		// Token: 0x04000ED1 RID: 3793
		public static int defenseCooldown;

		// Token: 0x04000ED2 RID: 3794
		public static int attackSwordCooldown;

		// Token: 0x04000ED3 RID: 3795
		public static int daggerCooldown;

		// Token: 0x04000ED4 RID: 3796
		public static int clubCooldown;

		// Token: 0x04000ED5 RID: 3797
		public static int daggerHitsLeft;

		// Token: 0x04000ED6 RID: 3798
		public static int timedHitTimer;

		// Token: 0x04000ED7 RID: 3799
		private static float addedSwordScale = 0f;

		// Token: 0x04000ED8 RID: 3800
		private static float addedClubScale = 0f;

		// Token: 0x04000ED9 RID: 3801
		private static float addedDaggerScale = 0f;

		// Token: 0x04000EDA RID: 3802
		private float swipeSpeed;

		// Token: 0x04000EDB RID: 3803
		[XmlIgnore]
		public Rectangle mostRecentArea;

		// Token: 0x04000EDC RID: 3804
		[XmlIgnore]
		private readonly NetEvent0 animateSpecialMoveEvent = new NetEvent0(false);

		// Token: 0x04000EDD RID: 3805
		[XmlIgnore]
		private readonly NetEvent0 defenseSwordEvent = new NetEvent0(false);

		// Token: 0x04000EDE RID: 3806
		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> daggerEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04000EDF RID: 3807
		private WeaponData cachedData;

		// Token: 0x04000EE1 RID: 3809
		private bool anotherClick;

		// Token: 0x04000EE2 RID: 3810
		private static Vector2 center = new Vector2(1f, 15f);
	}
}

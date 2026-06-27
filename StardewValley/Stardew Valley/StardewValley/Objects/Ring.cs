using System;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace StardewValley.Objects
{
	// Token: 0x020001B5 RID: 437
	[XmlInclude(typeof(CombinedRing))]
	public class Ring : Item
	{
		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06001F32 RID: 7986 RVA: 0x00166947 File Offset: 0x00164B47
		public override string TypeDefinitionId { get; } = "(O)";

		// Token: 0x06001F33 RID: 7987 RVA: 0x00166950 File Offset: 0x00164B50
		protected override void MigrateLegacyItemId()
		{
			this.itemId.Value = (((this.obsolete_indexInTileSheet != null) ? this.obsolete_indexInTileSheet.GetValueOrDefault().ToString() : null) ?? base.ParentSheetIndex.ToString());
			this.obsolete_indexInTileSheet = null;
		}

		// Token: 0x06001F34 RID: 7988 RVA: 0x001669A5 File Offset: 0x00164BA5
		public Ring()
		{
		}

		// Token: 0x06001F35 RID: 7989 RVA: 0x001669C4 File Offset: 0x00164BC4
		public Ring(string itemId) : this()
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			ObjectData data = Game1.objectData[itemId];
			base.ItemId = itemId;
			base.Category = -96;
			this.Name = (data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName);
			this.price.Value = data.Price;
			base.ResetParentSheetIndex();
			this.loadDisplayFields();
		}

		// Token: 0x06001F36 RID: 7990 RVA: 0x00166A39 File Offset: 0x00164C39
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.price, "price");
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x00166A58 File Offset: 0x00164C58
		public override bool CanBeLostOnDeath()
		{
			return false;
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x00166A5C File Offset: 0x00164C5C
		public override void onEquip(Farmer who)
		{
			base.onEquip(who);
			GameLocation location = who.currentLocation;
			location.removeLightSource(this.lightSourceId);
			this.lightSourceId = null;
			string itemId = base.ItemId;
			if (itemId == "516")
			{
				this.lightSourceId = this.GenerateLightSourceId(who);
				location.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 5f, new Color(0, 50, 170), LightSource.LightContext.None, who.UniqueMultiplayerID, null));
				return;
			}
			if (itemId == "517")
			{
				this.lightSourceId = this.GenerateLightSourceId(who);
				location.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), LightSource.LightContext.None, who.UniqueMultiplayerID, null));
				return;
			}
			if (!(itemId == "888") && !(itemId == "527"))
			{
				return;
			}
			this.lightSourceId = this.GenerateLightSourceId(who);
			location.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), LightSource.LightContext.None, who.UniqueMultiplayerID, null));
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x00166BFC File Offset: 0x00164DFC
		public override void onUnequip(Farmer who)
		{
			base.onUnequip(who);
			string itemId = base.ItemId;
			if (itemId == "516" || itemId == "517" || itemId == "888" || itemId == "527")
			{
				who.currentLocation.removeLightSource(this.lightSourceId);
				this.lightSourceId = null;
			}
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x00166C64 File Offset: 0x00164E64
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			string itemId = base.ItemId;
			if (itemId != null)
			{
				int length = itemId.Length;
				if (length == 3)
				{
					switch (itemId[2])
					{
					case '0':
						if (itemId == "530")
						{
							effects.Defense.Value += 1f;
							return;
						}
						if (!(itemId == "810"))
						{
							return;
						}
						effects.Defense.Value += 5f;
						return;
					case '1':
						if (!(itemId == "531"))
						{
							return;
						}
						effects.CriticalChanceMultiplier.Value += 0.1f;
						return;
					case '2':
						if (!(itemId == "532"))
						{
							return;
						}
						effects.CriticalPowerMultiplier.Value += 0.1f;
						return;
					case '3':
						if (!(itemId == "533"))
						{
							return;
						}
						effects.WeaponSpeedMultiplier.Value += 0.1f;
						return;
					case '4':
						if (!(itemId == "534"))
						{
							return;
						}
						effects.AttackMultiplier.Value += 0.1f;
						return;
					case '5':
					case '6':
						break;
					case '7':
						if (itemId == "527")
						{
							effects.MagneticRadius.Value += 128f;
							effects.AttackMultiplier.Value += 0.1f;
							return;
						}
						if (!(itemId == "887"))
						{
							return;
						}
						effects.Immunity.Value += 4f;
						break;
					case '8':
						if (itemId == "518")
						{
							effects.MagneticRadius.Value += 64f;
							return;
						}
						if (!(itemId == "888"))
						{
							return;
						}
						effects.MagneticRadius.Value += 128f;
						return;
					case '9':
						if (itemId == "519")
						{
							effects.MagneticRadius.Value += 128f;
							return;
						}
						if (itemId == "529")
						{
							effects.KnockbackMultiplier.Value += 0.1f;
							return;
						}
						if (!(itemId == "859"))
						{
							return;
						}
						effects.LuckLevel.Value += 1f;
						return;
					default:
						return;
					}
				}
			}
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x00166EF3 File Offset: 0x001650F3
		public override string getCategoryName()
		{
			return Object.GetCategoryDisplayName(-96);
		}

		// Token: 0x06001F3C RID: 7996 RVA: 0x00166EFC File Offset: 0x001650FC
		public virtual void onNewLocation(Farmer who, GameLocation environment)
		{
			environment.removeLightSource(this.lightSourceId);
			this.lightSourceId = null;
			string itemId = base.ItemId;
			if (itemId == "516" || itemId == "517")
			{
				GameLocation oldLocation = who.currentLocation;
				who.currentLocation = environment;
				this.onEquip(who);
				who.currentLocation = oldLocation;
				return;
			}
			if (!(itemId == "888") && !(itemId == "527"))
			{
				return;
			}
			this.lightSourceId = this.GenerateLightSourceId(who);
			environment.sharedLights.AddLight(new LightSource(this.lightSourceId, 1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), LightSource.LightContext.None, who.UniqueMultiplayerID, null));
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x00166FDC File Offset: 0x001651DC
		public virtual void onLeaveLocation(Farmer who, GameLocation environment)
		{
			string itemId = base.ItemId;
			if (itemId == "516" || itemId == "517" || itemId == "527" || itemId == "888")
			{
				environment.removeLightSource(this.lightSourceId);
				this.lightSourceId = null;
			}
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x00167037 File Offset: 0x00165237
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			return this.price.Value;
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x00167044 File Offset: 0x00165244
		public virtual void onMonsterSlay(Monster monster, GameLocation location, Farmer who)
		{
			string itemId = base.ItemId;
			if (!(itemId == "811"))
			{
				if (itemId == "860")
				{
					if (Game1.random.NextBool(0.25))
					{
						if (monster != null)
						{
							monster.objectsToDrop.Add("395");
						}
					}
					else if (Game1.random.NextBool(0.1) && monster != null)
					{
						monster.objectsToDrop.Add("253");
					}
				}
			}
			else if (monster != null && location != null)
			{
				location.explode(monster.Tile, 2, who, false, -1, !(location is Farm) && !(location is SlimeHutch));
			}
			if (who.IsLocalPlayer)
			{
				itemId = base.ItemId;
				if (!(itemId == "521"))
				{
					if (itemId == "522")
					{
						who.health = Math.Min(who.maxHealth, who.health + 2);
						return;
					}
					if (itemId == "523")
					{
						who.applyBuff("22");
						return;
					}
					if (!(itemId == "862"))
					{
						return;
					}
					who.Stamina = Math.Min((float)who.MaxStamina, who.Stamina + 4f);
				}
				else if (Game1.random.NextBool(0.1 + (double)((float)who.LuckLevel / 100f)))
				{
					who.applyBuff("20");
					Game1.playSound("warrior", null);
					return;
				}
			}
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x001671C8 File Offset: 0x001653C8
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f) * scaleSize, new Rectangle?(itemData.GetSourceRect(0, null)), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001F41 RID: 8001 RVA: 0x00167268 File Offset: 0x00165468
		public virtual void update(GameTime time, GameLocation environment, Farmer who)
		{
			if (this.lightSourceId != null)
			{
				Vector2 offset = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					offset += who.drawOffset;
				}
				environment.repositionLightSource(this.lightSourceId, new Vector2(who.Position.X + 21f, who.Position.Y) + offset);
				if (!environment.isOutdoors.Value && !(environment is MineShaft) && !(environment is VolcanoDungeon))
				{
					LightSource i = environment.getLightSource(this.lightSourceId);
					if (i != null)
					{
						i.radius.Value = 3f;
					}
				}
			}
		}

		// Token: 0x06001F42 RID: 8002 RVA: 0x0016730B File Offset: 0x0016550B
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001F43 RID: 8003 RVA: 0x00167310 File Offset: 0x00165510
		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			Point dimensions = new Point(0, startingHeight);
			int extra_rows_needed = 0;
			if (this.GetsEffectOfRing("810"))
			{
				extra_rows_needed++;
			}
			if (this.GetsEffectOfRing("887") || this.GetsEffectOfRing("530"))
			{
				extra_rows_needed++;
			}
			if (this.GetsEffectOfRing("859"))
			{
				extra_rows_needed++;
			}
			dimensions.X = (int)Math.Max((float)minWidth, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 9999)).X + (float)horizontalBuffer);
			dimensions.Y += extra_rows_needed * Math.Max((int)font.MeasureString("TT").Y, 48);
			return dimensions;
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x001673C5 File Offset: 0x001655C5
		public virtual bool GetsEffectOfRing(string ringId)
		{
			return base.ItemId == ringId;
		}

		// Token: 0x06001F45 RID: 8005 RVA: 0x001673D3 File Offset: 0x001655D3
		public virtual int GetEffectsOfRingMultiplier(string ringId)
		{
			if (this.GetsEffectOfRing(ringId))
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x001673E4 File Offset: 0x001655E4
		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			if (this.description == null)
			{
				this.loadDisplayFields();
			}
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			y += (int)font.MeasureString(Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth())).Y;
			if (this.GetsEffectOfRing("810") || this.GetsEffectOfRing("530"))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", this.GetsEffectOfRing("810") ? (5 * this.GetEffectsOfRingMultiplier("810")) : this.GetEffectsOfRingMultiplier("530")), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
			if (this.GetsEffectOfRing("887"))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", 4 * this.GetEffectsOfRingMultiplier("887")), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
			if (this.GetsEffectOfRing("859"))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16 + 4)), new Rectangle(50, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
				Utility.drawTextWithShadow(spriteBatch, "+" + Game1.content.LoadString("Strings\\UI:ItemHover_Buff4", this.GetEffectsOfRingMultiplier("859")), font, new Vector2((float)(x + 16 + 52), (float)(y + 16 + 12)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x00167760 File Offset: 0x00165960
		public override string getDescription()
		{
			if (this.description == null)
			{
				this.loadDisplayFields();
			}
			return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x06001F48 RID: 8008 RVA: 0x00167787 File Offset: 0x00165987
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06001F49 RID: 8009 RVA: 0x0016778A File Offset: 0x0016598A
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (this.displayName == null)
				{
					this.loadDisplayFields();
				}
				return this.displayName;
			}
		}

		// Token: 0x06001F4A RID: 8010 RVA: 0x001677A1 File Offset: 0x001659A1
		protected override Item GetOneNew()
		{
			return new Ring(base.ItemId);
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x001677B0 File Offset: 0x001659B0
		protected virtual bool loadDisplayFields()
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			this.displayName = itemData.DisplayName;
			this.description = itemData.Description;
			return true;
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x001677E2 File Offset: 0x001659E2
		public virtual bool CanCombine(Ring ring)
		{
			return !(ring is CombinedRing) && !(this is CombinedRing) && !(base.QualifiedItemId == ring.QualifiedItemId);
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x0016780C File Offset: 0x00165A0C
		public Ring Combine(Ring ring)
		{
			return new CombinedRing
			{
				combinedRings = 
				{
					base.getOne() as Ring,
					ring.getOne() as Ring
				}
			};
		}

		// Token: 0x04001323 RID: 4899
		public const string SmallGlowRingId = "516";

		// Token: 0x04001324 RID: 4900
		public const string GlowRingId = "517";

		// Token: 0x04001325 RID: 4901
		public const string SmallMagnetRingId = "518";

		// Token: 0x04001326 RID: 4902
		public const string MagnetRingId = "519";

		// Token: 0x04001327 RID: 4903
		public const string SlimeCharmerRingId = "520";

		// Token: 0x04001328 RID: 4904
		public const string WarriorRingId = "521";

		// Token: 0x04001329 RID: 4905
		public const string VampireRingId = "522";

		// Token: 0x0400132A RID: 4906
		public const string SavageRingId = "523";

		// Token: 0x0400132B RID: 4907
		public const string YobaRingId = "524";

		// Token: 0x0400132C RID: 4908
		public const string SturdyRingId = "525";

		// Token: 0x0400132D RID: 4909
		public const string BurglarsRingId = "526";

		// Token: 0x0400132E RID: 4910
		public const string IridiumBandId = "527";

		// Token: 0x0400132F RID: 4911
		public const string AmethystRingId = "529";

		// Token: 0x04001330 RID: 4912
		public const string TopazRingId = "530";

		// Token: 0x04001331 RID: 4913
		public const string AquamarineRingId = "531";

		// Token: 0x04001332 RID: 4914
		public const string JadeRingId = "532";

		// Token: 0x04001333 RID: 4915
		public const string EmeraldRingId = "533";

		// Token: 0x04001334 RID: 4916
		public const string RubyRingId = "534";

		// Token: 0x04001335 RID: 4917
		public const string WeddingRingId = "801";

		// Token: 0x04001336 RID: 4918
		public const string CrabshellRingId = "810";

		// Token: 0x04001337 RID: 4919
		public const string NapalmRingId = "811";

		// Token: 0x04001338 RID: 4920
		public const string ThornsRingId = "839";

		// Token: 0x04001339 RID: 4921
		public const string LuckyRingId = "859";

		// Token: 0x0400133A RID: 4922
		public const string HotJavaRingId = "860";

		// Token: 0x0400133B RID: 4923
		public const string ProtectiveRingId = "861";

		// Token: 0x0400133C RID: 4924
		public const string SoulSapperRingId = "862";

		// Token: 0x0400133D RID: 4925
		public const string PhoenixRingId = "863";

		// Token: 0x0400133E RID: 4926
		public const string CombinedRingId = "880";

		// Token: 0x0400133F RID: 4927
		public const string ImmunityBandId = "887";

		// Token: 0x04001340 RID: 4928
		public const string GlowstoneRingId = "888";

		// Token: 0x04001341 RID: 4929
		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		// Token: 0x04001342 RID: 4930
		[XmlElement("indexInTileSheet")]
		public int? obsolete_indexInTileSheet;

		// Token: 0x04001343 RID: 4931
		[XmlIgnore]
		public string description;

		// Token: 0x04001344 RID: 4932
		[XmlIgnore]
		public string displayName;

		// Token: 0x04001345 RID: 4933
		[XmlIgnore]
		public string lightSourceId;
	}
}

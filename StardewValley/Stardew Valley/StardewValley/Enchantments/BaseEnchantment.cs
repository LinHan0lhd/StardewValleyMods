using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000335 RID: 821
	[XmlInclude(typeof(BaseWeaponEnchantment))]
	[XmlInclude(typeof(ArtfulEnchantment))]
	[XmlInclude(typeof(BugKillerEnchantment))]
	[XmlInclude(typeof(CrusaderEnchantment))]
	[XmlInclude(typeof(HaymakerEnchantment))]
	[XmlInclude(typeof(MagicEnchantment))]
	[XmlInclude(typeof(VampiricEnchantment))]
	[XmlInclude(typeof(AxeEnchantment))]
	[XmlInclude(typeof(HoeEnchantment))]
	[XmlInclude(typeof(MilkPailEnchantment))]
	[XmlInclude(typeof(PanEnchantment))]
	[XmlInclude(typeof(PickaxeEnchantment))]
	[XmlInclude(typeof(ShearsEnchantment))]
	[XmlInclude(typeof(WateringCanEnchantment))]
	[XmlInclude(typeof(ArchaeologistEnchantment))]
	[XmlInclude(typeof(AutoHookEnchantment))]
	[XmlInclude(typeof(BottomlessEnchantment))]
	[XmlInclude(typeof(EfficientToolEnchantment))]
	[XmlInclude(typeof(GenerousEnchantment))]
	[XmlInclude(typeof(MasterEnchantment))]
	[XmlInclude(typeof(PowerfulEnchantment))]
	[XmlInclude(typeof(PreservingEnchantment))]
	[XmlInclude(typeof(ReachingToolEnchantment))]
	[XmlInclude(typeof(ShavingEnchantment))]
	[XmlInclude(typeof(SwiftToolEnchantment))]
	[XmlInclude(typeof(FisherEnchantment))]
	[XmlInclude(typeof(AmethystEnchantment))]
	[XmlInclude(typeof(AquamarineEnchantment))]
	[XmlInclude(typeof(DiamondEnchantment))]
	[XmlInclude(typeof(EmeraldEnchantment))]
	[XmlInclude(typeof(JadeEnchantment))]
	[XmlInclude(typeof(RubyEnchantment))]
	[XmlInclude(typeof(TopazEnchantment))]
	[XmlInclude(typeof(AttackEnchantment))]
	[XmlInclude(typeof(DefenseEnchantment))]
	[XmlInclude(typeof(SlimeSlayerEnchantment))]
	[XmlInclude(typeof(CritEnchantment))]
	[XmlInclude(typeof(WeaponSpeedEnchantment))]
	[XmlInclude(typeof(CritPowerEnchantment))]
	[XmlInclude(typeof(LightweightEnchantment))]
	[XmlInclude(typeof(SlimeGathererEnchantment))]
	[XmlInclude(typeof(GalaxySoulEnchantment))]
	public class BaseEnchantment : INetObject<NetFields>
	{
		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x060034EE RID: 13550 RVA: 0x002A6394 File Offset: 0x002A4594
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("BaseEnchantment");

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x060034EF RID: 13551 RVA: 0x002A639C File Offset: 0x002A459C
		// (set) Token: 0x060034F0 RID: 13552 RVA: 0x002A63A9 File Offset: 0x002A45A9
		[XmlElement("level")]
		public int Level
		{
			get
			{
				return this.level.Value;
			}
			set
			{
				this.level.Value = value;
			}
		}

		// Token: 0x060034F1 RID: 13553 RVA: 0x002A63B7 File Offset: 0x002A45B7
		public BaseEnchantment()
		{
			this.InitializeNetFields();
		}

		// Token: 0x060034F2 RID: 13554 RVA: 0x002A63E4 File Offset: 0x002A45E4
		public static BaseEnchantment GetEnchantmentFromItem(Item base_item, Item item)
		{
			if (base_item != null)
			{
				MeleeWeapon w = base_item as MeleeWeapon;
				if (w == null || w.isScythe())
				{
					goto IL_16C;
				}
			}
			string text = (item != null) ? item.QualifiedItemId : null;
			if (text != null)
			{
				int length = text.Length;
				if (length != 5)
				{
					if (length == 6)
					{
						if (text == "(O)896")
						{
							MeleeWeapon meleeWeapon = base_item as MeleeWeapon;
							bool? flag = (meleeWeapon != null) ? new bool?(meleeWeapon.isGalaxyWeapon()) : null;
							if (flag != null && flag.GetValueOrDefault())
							{
								return new GalaxySoulEnchantment();
							}
						}
					}
				}
				else
				{
					switch (text[4])
					{
					case '0':
						if (text == "(O)60")
						{
							return new EmeraldEnchantment();
						}
						if (text == "(O)70")
						{
							return new JadeEnchantment();
						}
						break;
					case '2':
						if (text == "(O)62")
						{
							return new AquamarineEnchantment();
						}
						if (text == "(O)72")
						{
							return new DiamondEnchantment();
						}
						break;
					case '4':
						if (text == "(O)64")
						{
							return new RubyEnchantment();
						}
						break;
					case '6':
						if (text == "(O)66")
						{
							return new AmethystEnchantment();
						}
						break;
					case '8':
						if (text == "(O)68")
						{
							return new TopazEnchantment();
						}
						break;
					}
				}
			}
			IL_16C:
			if (((item != null) ? item.QualifiedItemId : null) == "(O)74")
			{
				return Utility.CreateRandom(Game1.stats.Get("timesEnchanted"), Game1.uniqueIDForThisGame, (double)Game1.player.UniqueMultiplayerID, 0.0, 0.0).ChooseFrom(BaseEnchantment.GetAvailableEnchantmentsForItem(base_item as Tool));
			}
			return null;
		}

		// Token: 0x060034F3 RID: 13555 RVA: 0x002A65C4 File Offset: 0x002A47C4
		public static List<BaseEnchantment> GetAvailableEnchantmentsForItem(Tool item)
		{
			List<BaseEnchantment> item_enchantments = new List<BaseEnchantment>();
			if (item == null)
			{
				return BaseEnchantment.GetAvailableEnchantments();
			}
			List<BaseEnchantment> enchantments = BaseEnchantment.GetAvailableEnchantments();
			HashSet<Type> applied_enchantments = new HashSet<Type>();
			foreach (BaseEnchantment enchantment in item.enchantments)
			{
				applied_enchantments.Add(enchantment.GetType());
			}
			foreach (BaseEnchantment enchantment2 in enchantments)
			{
				if (enchantment2.CanApplyTo(item) && !applied_enchantments.Contains(enchantment2.GetType()))
				{
					item_enchantments.Add(enchantment2);
				}
			}
			using (NetList<string, NetString>.Enumerator enumerator3 = item.previousEnchantments.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					string previousEnchantment = enumerator3.Current;
					if (item_enchantments.Count <= 1)
					{
						break;
					}
					item_enchantments.RemoveAll((BaseEnchantment cur) => cur.GetName() == previousEnchantment);
				}
			}
			return item_enchantments;
		}

		// Token: 0x060034F4 RID: 13556 RVA: 0x002A66FC File Offset: 0x002A48FC
		public static List<BaseEnchantment> GetAvailableEnchantments()
		{
			if (BaseEnchantment._enchantments == null)
			{
				BaseEnchantment._enchantments = new List<BaseEnchantment>
				{
					new ArtfulEnchantment(),
					new BugKillerEnchantment(),
					new VampiricEnchantment(),
					new CrusaderEnchantment(),
					new HaymakerEnchantment(),
					new PowerfulEnchantment(),
					new ReachingToolEnchantment(),
					new ShavingEnchantment(),
					new BottomlessEnchantment(),
					new GenerousEnchantment(),
					new ArchaeologistEnchantment(),
					new MasterEnchantment(),
					new AutoHookEnchantment(),
					new PreservingEnchantment(),
					new EfficientToolEnchantment(),
					new SwiftToolEnchantment(),
					new FisherEnchantment()
				};
			}
			return BaseEnchantment._enchantments;
		}

		// Token: 0x060034F5 RID: 13557 RVA: 0x002A67DD File Offset: 0x002A49DD
		public static void ResetEnchantments()
		{
			BaseEnchantment._enchantments = null;
		}

		// Token: 0x060034F6 RID: 13558 RVA: 0x002A67E5 File Offset: 0x002A49E5
		public virtual bool IsForge()
		{
			return false;
		}

		// Token: 0x060034F7 RID: 13559 RVA: 0x002A67E8 File Offset: 0x002A49E8
		public virtual bool IsSecondaryEnchantment()
		{
			return false;
		}

		// Token: 0x060034F8 RID: 13560 RVA: 0x002A67EB File Offset: 0x002A49EB
		public virtual void InitializeNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.level, "level");
		}

		// Token: 0x060034F9 RID: 13561 RVA: 0x002A680A File Offset: 0x002A4A0A
		public void OnEquip(Farmer farmer)
		{
			if (!this._applied)
			{
				farmer.enchantments.Add(this);
				this._applied = true;
				this._OnEquip(farmer);
			}
		}

		// Token: 0x060034FA RID: 13562 RVA: 0x002A682E File Offset: 0x002A4A2E
		public void OnUnequip(Farmer farmer)
		{
			if (this._applied)
			{
				farmer.enchantments.Remove(this);
				this._applied = false;
				this._OnUnequip(farmer);
			}
		}

		// Token: 0x060034FB RID: 13563 RVA: 0x002A6853 File Offset: 0x002A4A53
		protected virtual void _OnEquip(Farmer who)
		{
		}

		// Token: 0x060034FC RID: 13564 RVA: 0x002A6855 File Offset: 0x002A4A55
		protected virtual void _OnUnequip(Farmer who)
		{
		}

		// Token: 0x060034FD RID: 13565 RVA: 0x002A6857 File Offset: 0x002A4A57
		public virtual void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, ref int amount)
		{
		}

		// Token: 0x060034FE RID: 13566 RVA: 0x002A6859 File Offset: 0x002A4A59
		public virtual void OnDealtDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, int amount)
		{
		}

		// Token: 0x060034FF RID: 13567 RVA: 0x002A685B File Offset: 0x002A4A5B
		public virtual void OnMonsterSlay(Monster monster, GameLocation location, Farmer who, bool slainByBomb)
		{
		}

		// Token: 0x06003500 RID: 13568 RVA: 0x002A685D File Offset: 0x002A4A5D
		public virtual void AddEquipmentEffects(BuffEffects effects)
		{
		}

		// Token: 0x06003501 RID: 13569 RVA: 0x002A685F File Offset: 0x002A4A5F
		public void OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
		{
			this._OnCutWeed(tile_location, location, who);
		}

		// Token: 0x06003502 RID: 13570 RVA: 0x002A686A File Offset: 0x002A4A6A
		protected virtual void _OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
		{
		}

		// Token: 0x06003503 RID: 13571 RVA: 0x002A686C File Offset: 0x002A4A6C
		public virtual BaseEnchantment GetOne()
		{
			BaseEnchantment baseEnchantment = Activator.CreateInstance(base.GetType()) as BaseEnchantment;
			baseEnchantment.level.Value = this.level.Value;
			return baseEnchantment;
		}

		// Token: 0x06003504 RID: 13572 RVA: 0x002A6894 File Offset: 0x002A4A94
		public int GetLevel()
		{
			return this.level.Value;
		}

		// Token: 0x06003505 RID: 13573 RVA: 0x002A68A4 File Offset: 0x002A4AA4
		public void SetLevel(Item item, int new_level)
		{
			if (new_level < 1)
			{
				new_level = 1;
			}
			else if (this.GetMaximumLevel() >= 0 && new_level > this.GetMaximumLevel())
			{
				new_level = this.GetMaximumLevel();
			}
			if (this.level.Value != new_level)
			{
				this.UnapplyTo(item, null);
				this.level.Value = new_level;
				this.ApplyTo(item, null);
			}
		}

		// Token: 0x06003506 RID: 13574 RVA: 0x002A68FE File Offset: 0x002A4AFE
		public virtual int GetMaximumLevel()
		{
			return -1;
		}

		// Token: 0x06003507 RID: 13575 RVA: 0x002A6901 File Offset: 0x002A4B01
		public void ApplyTo(Item item, Farmer farmer = null)
		{
			this._ApplyTo(item);
			if (this.IsItemCurrentlyEquipped(item, farmer))
			{
				this.OnEquip(farmer);
			}
		}

		// Token: 0x06003508 RID: 13576 RVA: 0x002A691B File Offset: 0x002A4B1B
		protected virtual void _ApplyTo(Item item)
		{
		}

		// Token: 0x06003509 RID: 13577 RVA: 0x002A691D File Offset: 0x002A4B1D
		public bool IsItemCurrentlyEquipped(Item item, Farmer farmer)
		{
			return farmer != null && this._IsCurrentlyEquipped(item, farmer);
		}

		// Token: 0x0600350A RID: 13578 RVA: 0x002A692C File Offset: 0x002A4B2C
		protected virtual bool _IsCurrentlyEquipped(Item item, Farmer farmer)
		{
			return farmer.CurrentTool == item;
		}

		// Token: 0x0600350B RID: 13579 RVA: 0x002A6937 File Offset: 0x002A4B37
		public void UnapplyTo(Item item, Farmer farmer = null)
		{
			this._UnapplyTo(item);
			if (this.IsItemCurrentlyEquipped(item, farmer))
			{
				this.OnUnequip(farmer);
			}
		}

		// Token: 0x0600350C RID: 13580 RVA: 0x002A6951 File Offset: 0x002A4B51
		protected virtual void _UnapplyTo(Item item)
		{
		}

		// Token: 0x0600350D RID: 13581 RVA: 0x002A6953 File Offset: 0x002A4B53
		public virtual bool CanApplyTo(Item item)
		{
			return true;
		}

		// Token: 0x0600350E RID: 13582 RVA: 0x002A6958 File Offset: 0x002A4B58
		public string GetDisplayName()
		{
			if (this._displayName == null)
			{
				this._displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\EnchantmentNames:" + this.GetName(), true);
				if (this._displayName == null)
				{
					this._displayName = this.GetName();
				}
			}
			return this._displayName;
		}

		// Token: 0x0600350F RID: 13583 RVA: 0x002A69A8 File Offset: 0x002A4BA8
		public virtual string GetName()
		{
			return "Unknown Enchantment";
		}

		// Token: 0x06003510 RID: 13584 RVA: 0x002A69AF File Offset: 0x002A4BAF
		public virtual bool ShouldBeDisplayed()
		{
			return true;
		}

		// Token: 0x040022A8 RID: 8872
		[XmlIgnore]
		protected string _displayName;

		// Token: 0x040022AA RID: 8874
		[XmlIgnore]
		protected bool _applied;

		// Token: 0x040022AB RID: 8875
		[XmlIgnore]
		[InstancedStatic]
		public static bool hideEnchantmentName;

		// Token: 0x040022AC RID: 8876
		[XmlIgnore]
		[InstancedStatic]
		public static bool hideSecondaryEnchantName;

		// Token: 0x040022AD RID: 8877
		protected static List<BaseEnchantment> _enchantments;

		// Token: 0x040022AE RID: 8878
		protected readonly NetInt level = new NetInt(1);
	}
}

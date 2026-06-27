using System;
using Netcode;
using StardewValley.GameData.Buffs;

namespace StardewValley.Buffs
{
	// Token: 0x0200038C RID: 908
	public class BuffEffects : INetObject<NetFields>
	{
		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x060037F4 RID: 14324 RVA: 0x002C4CAF File Offset: 0x002C2EAF
		public NetFields NetFields { get; } = new NetFields("BuffEffects");

		// Token: 0x060037F5 RID: 14325 RVA: 0x002C4CB8 File Offset: 0x002C2EB8
		public BuffEffects()
		{
			this.AdditiveFields = new NetFloat[]
			{
				this.CombatLevel,
				this.FarmingLevel,
				this.FishingLevel,
				this.MiningLevel,
				this.LuckLevel,
				this.ForagingLevel,
				this.MaxStamina,
				this.MagneticRadius,
				this.Speed,
				this.Defense,
				this.Attack,
				this.Immunity
			};
			this.MultiplicativeFields = new NetFloat[]
			{
				this.AttackMultiplier,
				this.KnockbackMultiplier,
				this.WeaponSpeedMultiplier,
				this.CriticalChanceMultiplier,
				this.CriticalPowerMultiplier,
				this.WeaponPrecisionMultiplier
			};
			this.NetFields.SetOwner(this).AddField(this.CombatLevel, "CombatLevel").AddField(this.FarmingLevel, "FarmingLevel").AddField(this.FishingLevel, "FishingLevel").AddField(this.MiningLevel, "MiningLevel").AddField(this.LuckLevel, "LuckLevel").AddField(this.ForagingLevel, "ForagingLevel").AddField(this.MaxStamina, "MaxStamina").AddField(this.MagneticRadius, "MagneticRadius").AddField(this.Speed, "Speed").AddField(this.Defense, "Defense").AddField(this.Attack, "Attack").AddField(this.AttackMultiplier, "AttackMultiplier").AddField(this.Immunity, "Immunity").AddField(this.KnockbackMultiplier, "KnockbackMultiplier").AddField(this.WeaponSpeedMultiplier, "WeaponSpeedMultiplier").AddField(this.CriticalChanceMultiplier, "CriticalChanceMultiplier").AddField(this.CriticalPowerMultiplier, "CriticalPowerMultiplier").AddField(this.WeaponPrecisionMultiplier, "WeaponPrecisionMultiplier");
		}

		// Token: 0x060037F6 RID: 14326 RVA: 0x002C4FE6 File Offset: 0x002C31E6
		public BuffEffects(BuffAttributesData data) : this()
		{
			this.Add(data);
		}

		// Token: 0x060037F7 RID: 14327 RVA: 0x002C4FF8 File Offset: 0x002C31F8
		public void Add(BuffEffects other)
		{
			if (other != null)
			{
				for (int i = 0; i < this.AdditiveFields.Length; i++)
				{
					this.AdditiveFields[i].Value += other.AdditiveFields[i].Value;
				}
				for (int j = 0; j < this.MultiplicativeFields.Length; j++)
				{
					this.MultiplicativeFields[j].Value += other.MultiplicativeFields[j].Value;
				}
			}
		}

		// Token: 0x060037F8 RID: 14328 RVA: 0x002C5070 File Offset: 0x002C3270
		public void Add(BuffAttributesData data)
		{
			if (data != null)
			{
				this.CombatLevel.Value = data.CombatLevel;
				this.FarmingLevel.Value = data.FarmingLevel;
				this.FishingLevel.Value = data.FishingLevel;
				this.MiningLevel.Value = data.MiningLevel;
				this.LuckLevel.Value = data.LuckLevel;
				this.ForagingLevel.Value = data.ForagingLevel;
				this.MaxStamina.Value = data.MaxStamina;
				this.MagneticRadius.Value = data.MagneticRadius;
				this.Speed.Value = data.Speed;
				this.Defense.Value = data.Defense;
				this.Attack.Value = data.Attack;
				this.AttackMultiplier.Value = data.AttackMultiplier;
				this.Immunity.Value = data.Immunity;
				this.KnockbackMultiplier.Value = data.KnockbackMultiplier;
				this.WeaponSpeedMultiplier.Value = data.WeaponSpeedMultiplier;
				this.CriticalChanceMultiplier.Value = data.CriticalChanceMultiplier;
				this.CriticalPowerMultiplier.Value = data.CriticalPowerMultiplier;
				this.WeaponPrecisionMultiplier.Value = data.WeaponPrecisionMultiplier;
			}
		}

		// Token: 0x060037F9 RID: 14329 RVA: 0x002C51B8 File Offset: 0x002C33B8
		public bool HasAnyValue()
		{
			NetFloat[] array = this.AdditiveFields;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Value != 0f)
				{
					return true;
				}
			}
			array = this.MultiplicativeFields;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Value != 0f)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060037FA RID: 14330 RVA: 0x002C5214 File Offset: 0x002C3414
		public void Clear()
		{
			NetFloat[] array = this.AdditiveFields;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = 0f;
			}
			array = this.MultiplicativeFields;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = 0f;
			}
		}

		// Token: 0x060037FB RID: 14331 RVA: 0x002C5268 File Offset: 0x002C3468
		public string[] ToLegacyAttributeFormat()
		{
			return new string[]
			{
				((int)this.FarmingLevel.Value).ToString(),
				((int)this.FishingLevel.Value).ToString(),
				((int)this.MiningLevel.Value).ToString(),
				"0",
				((int)this.LuckLevel.Value).ToString(),
				((int)this.ForagingLevel.Value).ToString(),
				"0",
				((int)this.MaxStamina.Value).ToString(),
				((int)this.MagneticRadius.Value).ToString(),
				this.Speed.Value.ToString(),
				((int)this.Defense.Value).ToString(),
				((int)this.Attack.Value).ToString(),
				""
			};
		}

		// Token: 0x0400244F RID: 9295
		private readonly NetFloat[] AdditiveFields;

		// Token: 0x04002450 RID: 9296
		private readonly NetFloat[] MultiplicativeFields;

		// Token: 0x04002452 RID: 9298
		public readonly NetFloat CombatLevel = new NetFloat(0f);

		// Token: 0x04002453 RID: 9299
		public readonly NetFloat FarmingLevel = new NetFloat(0f);

		// Token: 0x04002454 RID: 9300
		public readonly NetFloat FishingLevel = new NetFloat(0f);

		// Token: 0x04002455 RID: 9301
		public readonly NetFloat MiningLevel = new NetFloat(0f);

		// Token: 0x04002456 RID: 9302
		public readonly NetFloat LuckLevel = new NetFloat(0f);

		// Token: 0x04002457 RID: 9303
		public readonly NetFloat ForagingLevel = new NetFloat(0f);

		// Token: 0x04002458 RID: 9304
		public readonly NetFloat MaxStamina = new NetFloat(0f);

		// Token: 0x04002459 RID: 9305
		public readonly NetFloat MagneticRadius = new NetFloat(0f);

		// Token: 0x0400245A RID: 9306
		public readonly NetFloat Speed = new NetFloat(0f);

		// Token: 0x0400245B RID: 9307
		public readonly NetFloat Defense = new NetFloat(0f);

		// Token: 0x0400245C RID: 9308
		public readonly NetFloat Attack = new NetFloat(0f);

		// Token: 0x0400245D RID: 9309
		public readonly NetFloat AttackMultiplier = new NetFloat(0f);

		// Token: 0x0400245E RID: 9310
		public readonly NetFloat Immunity = new NetFloat(0f);

		// Token: 0x0400245F RID: 9311
		public readonly NetFloat KnockbackMultiplier = new NetFloat(0f);

		// Token: 0x04002460 RID: 9312
		public readonly NetFloat WeaponSpeedMultiplier = new NetFloat(0f);

		// Token: 0x04002461 RID: 9313
		public readonly NetFloat CriticalChanceMultiplier = new NetFloat(0f);

		// Token: 0x04002462 RID: 9314
		public readonly NetFloat CriticalPowerMultiplier = new NetFloat(0f);

		// Token: 0x04002463 RID: 9315
		public readonly NetFloat WeaponPrecisionMultiplier = new NetFloat(0f);
	}
}

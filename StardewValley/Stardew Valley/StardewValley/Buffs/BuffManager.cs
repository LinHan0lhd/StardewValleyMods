using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Logging;
using StardewValley.Triggers;

namespace StardewValley.Buffs
{
	// Token: 0x0200038D RID: 909
	public class BuffManager : INetObject<NetFields>
	{
		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x060037FC RID: 14332 RVA: 0x002C537D File Offset: 0x002C357D
		public NetFields NetFields { get; } = new NetFields("BuffManager");

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x060037FD RID: 14333 RVA: 0x002C5385 File Offset: 0x002C3585
		public int CombatLevel
		{
			get
			{
				return (int)this.GetValues().CombatLevel.Value;
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x060037FE RID: 14334 RVA: 0x002C5398 File Offset: 0x002C3598
		public int FarmingLevel
		{
			get
			{
				return (int)this.GetValues().FarmingLevel.Value;
			}
		}

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x060037FF RID: 14335 RVA: 0x002C53AB File Offset: 0x002C35AB
		public int FishingLevel
		{
			get
			{
				return (int)this.GetValues().FishingLevel.Value;
			}
		}

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06003800 RID: 14336 RVA: 0x002C53BE File Offset: 0x002C35BE
		public int MiningLevel
		{
			get
			{
				return (int)this.GetValues().MiningLevel.Value;
			}
		}

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06003801 RID: 14337 RVA: 0x002C53D1 File Offset: 0x002C35D1
		public int LuckLevel
		{
			get
			{
				return (int)this.GetValues().LuckLevel.Value;
			}
		}

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06003802 RID: 14338 RVA: 0x002C53E4 File Offset: 0x002C35E4
		public int ForagingLevel
		{
			get
			{
				return (int)this.GetValues().ForagingLevel.Value;
			}
		}

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x06003803 RID: 14339 RVA: 0x002C53F7 File Offset: 0x002C35F7
		public int MaxStamina
		{
			get
			{
				return (int)this.GetValues().MaxStamina.Value;
			}
		}

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x06003804 RID: 14340 RVA: 0x002C540A File Offset: 0x002C360A
		public int MagneticRadius
		{
			get
			{
				return (int)this.GetValues().MagneticRadius.Value;
			}
		}

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x06003805 RID: 14341 RVA: 0x002C541D File Offset: 0x002C361D
		public float Speed
		{
			get
			{
				return this.GetValues().Speed.Value;
			}
		}

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06003806 RID: 14342 RVA: 0x002C542F File Offset: 0x002C362F
		public int Defense
		{
			get
			{
				return (int)this.GetValues().Defense.Value;
			}
		}

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06003807 RID: 14343 RVA: 0x002C5442 File Offset: 0x002C3642
		public int Attack
		{
			get
			{
				return (int)this.GetValues().Attack.Value;
			}
		}

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06003808 RID: 14344 RVA: 0x002C5455 File Offset: 0x002C3655
		public int Immunity
		{
			get
			{
				return (int)this.GetValues().Immunity.Value;
			}
		}

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06003809 RID: 14345 RVA: 0x002C5468 File Offset: 0x002C3668
		public float AttackMultiplier
		{
			get
			{
				return this.GetValues().AttackMultiplier.Value;
			}
		}

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x0600380A RID: 14346 RVA: 0x002C547A File Offset: 0x002C367A
		public float KnockbackMultiplier
		{
			get
			{
				return this.GetValues().KnockbackMultiplier.Value;
			}
		}

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x0600380B RID: 14347 RVA: 0x002C548C File Offset: 0x002C368C
		public float WeaponSpeedMultiplier
		{
			get
			{
				return this.GetValues().WeaponSpeedMultiplier.Value;
			}
		}

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x0600380C RID: 14348 RVA: 0x002C549E File Offset: 0x002C369E
		public float CriticalChanceMultiplier
		{
			get
			{
				return this.GetValues().CriticalChanceMultiplier.Value;
			}
		}

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x0600380D RID: 14349 RVA: 0x002C54B0 File Offset: 0x002C36B0
		public float CriticalPowerMultiplier
		{
			get
			{
				return this.GetValues().CriticalPowerMultiplier.Value;
			}
		}

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x0600380E RID: 14350 RVA: 0x002C54C2 File Offset: 0x002C36C2
		public float WeaponPrecisionMultiplier
		{
			get
			{
				return this.GetValues().WeaponPrecisionMultiplier.Value;
			}
		}

		// Token: 0x0600380F RID: 14351 RVA: 0x002C54D4 File Offset: 0x002C36D4
		public BuffManager()
		{
			this.NetFields.SetOwner(this).AddField(this.AppliedBuffIds, "AppliedBuffIds").AddField(this.CombinedEffects.NetFields, "CombinedEffects.NetFields");
		}

		// Token: 0x06003810 RID: 14352 RVA: 0x002C5554 File Offset: 0x002C3754
		public virtual BuffEffects GetValues()
		{
			if (!this.Dirty)
			{
				return this.CombinedEffects;
			}
			Farmer player = this.Player;
			this.CombinedEffects.Clear();
			player.stopGlowing();
			foreach (Buff buff in this.AppliedBuffs.Values)
			{
				this.CombinedEffects.Add(buff.effects);
				if (buff.glow != Color.White && buff.glow.A > 0)
				{
					player.startGlowing(buff.glow, false, 0.05f);
				}
			}
			this.AppliedBuffIds.Clear();
			foreach (string id in this.AppliedBuffs.Keys)
			{
				this.AppliedBuffIds.Add(id);
			}
			foreach (Item item in player.GetEquippedItems())
			{
				item.AddEquipmentEffects(this.CombinedEffects);
			}
			if (this.IsLocallyControlled())
			{
				Game1.buffsDisplay.dirty = true;
			}
			this.Dirty = false;
			player.stamina = Math.Min(player.stamina, (float)player.MaxStamina);
			return this.CombinedEffects;
		}

		// Token: 0x06003811 RID: 14353 RVA: 0x002C56D8 File Offset: 0x002C38D8
		public void SetOwner(Farmer player)
		{
			this.Player = player;
		}

		// Token: 0x06003812 RID: 14354 RVA: 0x002C56E1 File Offset: 0x002C38E1
		public bool IsApplied(string id)
		{
			return this.AppliedBuffIds.Contains(id);
		}

		// Token: 0x06003813 RID: 14355 RVA: 0x002C56F0 File Offset: 0x002C38F0
		public bool HasBuffWithNameContaining(string idSubstring)
		{
			using (NetList<string, NetString>.Enumerator enumerator = this.AppliedBuffIds.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Contains(idSubstring))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06003814 RID: 14356 RVA: 0x002C574C File Offset: 0x002C394C
		public virtual bool IsLocallyControlled()
		{
			return this.Player.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID;
		}

		// Token: 0x06003815 RID: 14357 RVA: 0x002C5768 File Offset: 0x002C3968
		public void Apply(Buff buff)
		{
			if (buff == null)
			{
				Game1.log.Warn("Ignored invalid null buff.");
				return;
			}
			if (string.IsNullOrWhiteSpace(buff.id))
			{
				Game1.log.Warn("Ignored invalid buff with no ID.");
				return;
			}
			if (buff.millisecondsDuration <= 0 && buff.millisecondsDuration != -2)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Ignored invalid buff '");
				defaultInterpolatedStringHandler.AppendFormatted(buff.id);
				defaultInterpolatedStringHandler.AppendLiteral("' with ");
				defaultInterpolatedStringHandler.AppendFormatted((buff.millisecondsDuration < 0) ? "negative" : "no");
				defaultInterpolatedStringHandler.AppendLiteral(" duration.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			if (!this.IsLocallyControlled())
			{
				return;
			}
			this.Remove(buff.id);
			this.AppliedBuffs[buff.id] = buff;
			this.AppliedBuffIds.Add(buff.id);
			string[] actionsOnApply = buff.actionsOnApply;
			if (actionsOnApply != null && actionsOnApply.Length != 0)
			{
				foreach (string action in buff.actionsOnApply)
				{
					string error;
					Exception exception;
					if (TriggerActionManager.TryRunAction(action, out error, out exception))
					{
						IGameLogger log2 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Applied action [");
						defaultInterpolatedStringHandler.AppendFormatted(action);
						defaultInterpolatedStringHandler.AppendLiteral("] from buff '");
						defaultInterpolatedStringHandler.AppendFormatted(buff.id);
						defaultInterpolatedStringHandler.AppendLiteral("'.");
						log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						IGameLogger log3 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Error applying Applied action [");
						defaultInterpolatedStringHandler.AppendFormatted(action);
						defaultInterpolatedStringHandler.AppendLiteral("] from buff '");
						defaultInterpolatedStringHandler.AppendFormatted(buff.id);
						defaultInterpolatedStringHandler.AppendLiteral("': ");
						defaultInterpolatedStringHandler.AppendFormatted(error);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						log3.Error(defaultInterpolatedStringHandler.ToStringAndClear(), exception);
					}
				}
			}
			Game1.buffsDisplay.updatedIDs.Add(buff.id);
			this.Dirty = true;
			buff.OnAdded();
		}

		// Token: 0x06003816 RID: 14358 RVA: 0x002C5980 File Offset: 0x002C3B80
		public void Remove(string id)
		{
			if (!this.IsLocallyControlled())
			{
				return;
			}
			Buff buff;
			if (this.AppliedBuffs.TryGetValue(id, out buff))
			{
				buff.OnRemoved();
			}
			if (this.AppliedBuffs.Remove(id) | this.AppliedBuffIds.Remove(id) | Game1.buffsDisplay.updatedIDs.Remove(id))
			{
				this.Dirty = true;
			}
		}

		// Token: 0x06003817 RID: 14359 RVA: 0x002C59E0 File Offset: 0x002C3BE0
		public void Clear()
		{
			if (!this.IsLocallyControlled())
			{
				return;
			}
			for (int i = this.AppliedBuffIds.Count - 1; i >= 0; i--)
			{
				this.Remove(this.AppliedBuffIds[i]);
			}
		}

		// Token: 0x06003818 RID: 14360 RVA: 0x002C5A20 File Offset: 0x002C3C20
		public void Update(GameTime time)
		{
			if (!this.IsLocallyControlled())
			{
				return;
			}
			for (int i = this.AppliedBuffIds.Count - 1; i >= 0; i--)
			{
				string id = this.AppliedBuffIds[i];
				Buff buff;
				if (!this.AppliedBuffs.TryGetValue(id, out buff) || buff.update(time))
				{
					this.Remove(id);
				}
			}
			if (this.Dirty)
			{
				this.GetValues();
			}
		}

		// Token: 0x04002464 RID: 9316
		protected Farmer Player;

		// Token: 0x04002465 RID: 9317
		protected readonly BuffEffects CombinedEffects = new BuffEffects();

		// Token: 0x04002466 RID: 9318
		public readonly IDictionary<string, Buff> AppliedBuffs = new Dictionary<string, Buff>();

		// Token: 0x04002467 RID: 9319
		public readonly NetStringList AppliedBuffIds = new NetStringList();

		// Token: 0x04002469 RID: 9321
		public bool Dirty = true;
	}
}

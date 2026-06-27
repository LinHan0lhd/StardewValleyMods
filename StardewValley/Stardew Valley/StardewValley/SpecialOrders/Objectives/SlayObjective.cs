using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Monsters;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x0200015E RID: 350
	public class SlayObjective : OrderObjective
	{
		// Token: 0x06001AFC RID: 6908 RVA: 0x0013BCCA File Offset: 0x00139ECA
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.targetNames, "targetNames").AddField(this.ignoreFarmMonsters, "ignoreFarmMonsters");
		}

		// Token: 0x06001AFD RID: 6909 RVA: 0x0013BCFC File Offset: 0x00139EFC
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			base.Load(order, data);
			string rawValue;
			if (data.TryGetValue("TargetName", out rawValue))
			{
				foreach (string target in order.Parse(rawValue).Split(',', StringSplitOptions.None))
				{
					this.targetNames.Add(target.Trim());
				}
			}
			string rawIgnoreFarmMonsters;
			if (data.TryGetValue("IgnoreFarmMonsters", out rawIgnoreFarmMonsters))
			{
				bool parsedIgnoreFarmMonsters;
				if (bool.TryParse(rawIgnoreFarmMonsters, out parsedIgnoreFarmMonsters))
				{
					this.ignoreFarmMonsters.Value = parsedIgnoreFarmMonsters;
					return;
				}
				Game1.log.Warn("Special order slay objective can't parse IgnoreFarmMonsters value '" + rawIgnoreFarmMonsters + "' as a boolean.");
			}
		}

		// Token: 0x06001AFE RID: 6910 RVA: 0x0013BD96 File Offset: 0x00139F96
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onMonsterSlain = (Action<Farmer, Monster>)Delegate.Combine(order.onMonsterSlain, new Action<Farmer, Monster>(this.OnMonsterSlain));
		}

		// Token: 0x06001AFF RID: 6911 RVA: 0x0013BDC6 File Offset: 0x00139FC6
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onMonsterSlain = (Action<Farmer, Monster>)Delegate.Remove(order.onMonsterSlain, new Action<Farmer, Monster>(this.OnMonsterSlain));
		}

		// Token: 0x06001B00 RID: 6912 RVA: 0x0013BDF8 File Offset: 0x00139FF8
		public virtual void OnMonsterSlain(Farmer farmer, Monster monster)
		{
			if (this.ignoreFarmMonsters.Value)
			{
				GameLocation currentLocation = monster.currentLocation;
				if (((currentLocation != null) ? currentLocation.Name : null) == "Farm")
				{
					return;
				}
			}
			foreach (string target in this.targetNames)
			{
				if (monster.Name.Contains(target))
				{
					this.IncrementCount(1);
					break;
				}
			}
		}

		// Token: 0x0400108A RID: 4234
		[XmlElement("targetNames")]
		public NetStringList targetNames = new NetStringList();

		// Token: 0x0400108B RID: 4235
		[XmlElement("ignoreFarmMonsters")]
		public NetBool ignoreFarmMonsters = new NetBool(true);
	}
}

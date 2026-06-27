using System;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace StardewValley.Enchantments
{
	// Token: 0x02000357 RID: 855
	public class VampiricEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600358F RID: 13711 RVA: 0x002A76E0 File Offset: 0x002A58E0
		public override void OnMonsterSlay(Monster monster, GameLocation location, Farmer who, bool slainByBomb)
		{
			base.OnMonsterSlay(monster, location, who, slainByBomb);
			if (!slainByBomb && Game1.random.NextDouble() < 0.09000000357627869)
			{
				int amount = Math.Max(1, (int)((float)(monster.MaxHealth + Game1.random.Next(-monster.MaxHealth / 10, monster.MaxHealth / 15 + 1)) * 0.1f));
				who.health = Math.Min(who.maxHealth, who.health + amount);
				location.debris.Add(new Debris(amount, who.getStandingPosition(), Color.Lime, 1f, who));
				Game1.playSound("healSound", null);
			}
		}

		// Token: 0x06003590 RID: 13712 RVA: 0x002A779B File Offset: 0x002A599B
		public override string GetName()
		{
			return "Vampiric";
		}
	}
}

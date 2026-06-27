using System;
using Microsoft.Xna.Framework;
using StardewValley.Projectiles;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000348 RID: 840
	public class MagicEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600355B RID: 13659 RVA: 0x002A70A4 File Offset: 0x002A52A4
		protected override void _OnSwing(MeleeWeapon weapon, Farmer farmer)
		{
			base._OnSwing(weapon, farmer);
			Vector2 shot_velocity = default(Vector2);
			Vector2 shot_origin = farmer.getStandingPosition() - new Vector2(32f, 32f);
			switch (farmer.facingDirection.Value)
			{
			case 0:
				shot_velocity.Y = -1f;
				break;
			case 1:
				shot_velocity.X = 1f;
				break;
			case 2:
				shot_velocity.Y = 1f;
				break;
			case 3:
				shot_velocity.X = -1f;
				break;
			}
			float rotation_velocity = 32f;
			shot_velocity *= 10f;
			BasicProjectile projectile = new BasicProjectile((int)Math.Ceiling((double)((float)weapon.minDamage.Value / 4f)), 11, 0, 1, rotation_velocity * 0.017453292f, shot_velocity.X, shot_velocity.Y, shot_origin, null, null, null, false, true, farmer.currentLocation, farmer, null, null);
			projectile.ignoreTravelGracePeriod.Value = true;
			projectile.ignoreMeleeAttacks.Value = true;
			projectile.maxTravelDistance.Value = 256;
			projectile.height.Value = 32f;
			farmer.currentLocation.projectiles.Add(projectile);
		}

		// Token: 0x0600355C RID: 13660 RVA: 0x002A71D8 File Offset: 0x002A53D8
		public override string GetName()
		{
			return "Starburst";
		}
	}
}

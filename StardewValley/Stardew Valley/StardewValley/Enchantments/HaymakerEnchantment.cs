using System;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.Enchantments
{
	// Token: 0x02000344 RID: 836
	public class HaymakerEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600354B RID: 13643 RVA: 0x002A6DE9 File Offset: 0x002A4FE9
		public override string GetName()
		{
			return "Haymaker";
		}

		// Token: 0x0600354C RID: 13644 RVA: 0x002A6DF0 File Offset: 0x002A4FF0
		protected override void _OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
		{
			base._OnCutWeed(tile_location, location, who);
			if (Game1.random.NextBool())
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)771", 1, 0, false), new Vector2(tile_location.X * 64f + 32f, tile_location.Y * 64f + 32f), -1, null, -1, false);
			}
			if (Game1.random.NextDouble() < 0.33)
			{
				if (GameLocation.StoreHayInAnySilo(1, location) == 0)
				{
					TemporaryAnimatedSprite tmpSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, who.Position - new Vector2(0f, 128f), false, false, who.Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0f, 0f, false);
					tmpSprite.motion.Y = -1f;
					tmpSprite.layerDepth = 1f - (float)Game1.random.Next(100) / 10000f;
					tmpSprite.delayBeforeAnimationStart = Game1.random.Next(350);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						tmpSprite
					});
					Game1.addHUDMessage(HUDMessage.ForItemGained(ItemRegistry.Create("(O)178", 1, 0, false), 1, null));
					return;
				}
				Game1.createItemDebris(ItemRegistry.Create("(O)178", 1, 0, false).getOne(), new Vector2(tile_location.X * 64f + 32f, tile_location.Y * 64f + 32f), -1, null, -1, false);
			}
		}
	}
}

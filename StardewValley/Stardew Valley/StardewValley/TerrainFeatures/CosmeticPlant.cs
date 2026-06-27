using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x0200013F RID: 319
	public class CosmeticPlant : Grass
	{
		// Token: 0x06001946 RID: 6470 RVA: 0x00128F3A File Offset: 0x0012713A
		public CosmeticPlant()
		{
		}

		// Token: 0x06001947 RID: 6471 RVA: 0x00128F63 File Offset: 0x00127163
		public CosmeticPlant(int which) : base(which, 1)
		{
			this.flipped.Value = Game1.random.NextBool();
		}

		// Token: 0x06001948 RID: 6472 RVA: 0x00128FA3 File Offset: 0x001271A3
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.flipped, "flipped").AddField(this.xOffset, "xOffset").AddField(this.yOffset, "yOffset");
		}

		// Token: 0x06001949 RID: 6473 RVA: 0x00128FE4 File Offset: 0x001271E4
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)(tileLocation.X * 64f + 16f), (int)((tileLocation.Y + 1f) * 64f - 8f - 4f), 8, 8);
		}

		// Token: 0x0600194A RID: 6474 RVA: 0x00129031 File Offset: 0x00127231
		public override string textureName()
		{
			return "TerrainFeatures\\upperCavePlants";
		}

		// Token: 0x0600194B RID: 6475 RVA: 0x00129038 File Offset: 0x00127238
		public override void loadSprite()
		{
			this.xOffset.Value = Game1.random.Next(-2, 3) * 4;
			this.yOffset.Value = Game1.random.Next(-2, 1) * 4;
		}

		// Token: 0x0600194C RID: 6476 RVA: 0x00129070 File Offset: 0x00127270
		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			MeleeWeapon weapon = t as MeleeWeapon;
			if ((weapon != null && weapon.type.Value != 2) || explosion > 0)
			{
				base.shake(0.2945243f, 0.07853982f, Game1.random.NextBool());
				int numberOfWeedsToDestroy = (explosion > 0) ? Math.Max(1, explosion + 2 - Game1.random.Next(2)) : ((t.upgradeLevel.Value == 3) ? 3 : (t.upgradeLevel.Value + 1));
				Game1.createRadialDebris(location, this.textureName(), new Rectangle((int)(this.grassType.Value * 16), 6, 7, 6), (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 14));
				this.numberOfWeeds.Value = this.numberOfWeeds.Value - numberOfWeedsToDestroy;
				if (this.numberOfWeeds.Value <= 0)
				{
					Random grassRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, (double)Game1.CurrentMineLevel, (double)Game1.player.timesReachedMineBottom);
					if (grassRandom.NextDouble() < 0.005)
					{
						Game1.createObjectDebris("(O)114", (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, location);
					}
					else if (grassRandom.NextDouble() < 0.01)
					{
						Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(1, 2), location);
					}
					else if (grassRandom.NextDouble() < 0.02)
					{
						Game1.createDebris(92, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(2, 4), location);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600194D RID: 6477 RVA: 0x00129234 File Offset: 0x00127434
		public override void draw(SpriteBatch spriteBatch)
		{
			Vector2 tileLocation = this.Tile;
			spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f) + new Vector2((float)(32 + this.xOffset.Value), (float)(60 + this.yOffset.Value))), new Rectangle?(new Rectangle((int)(this.grassType.Value * 16), 0, 16, 24)), Color.White, this.shakeRotation, new Vector2(8f, 23f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((float)(this.getBoundingBox().Y - 4) + tileLocation.X / 900f + 0.01f) / 10000f);
		}

		// Token: 0x04000F2F RID: 3887
		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		// Token: 0x04000F30 RID: 3888
		[XmlElement("xOffset")]
		private readonly NetInt xOffset = new NetInt();

		// Token: 0x04000F31 RID: 3889
		[XmlElement("yOffset")]
		private readonly NetInt yOffset = new NetInt();
	}
}

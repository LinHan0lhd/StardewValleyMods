using System;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Tools
{
	// Token: 0x02000134 RID: 308
	public class Wand : Tool
	{
		// Token: 0x060018C6 RID: 6342 RVA: 0x00123BCB File Offset: 0x00121DCB
		public Wand() : base("Return Scepter", 0, 2, 2, false, 0)
		{
			base.InstantUse = true;
		}

		// Token: 0x060018C7 RID: 6343 RVA: 0x00123BE4 File Offset: 0x00121DE4
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = "ReturnScepter";
		}

		// Token: 0x060018C8 RID: 6344 RVA: 0x00123BF1 File Offset: 0x00121DF1
		protected override Item GetOneNew()
		{
			return new Wand();
		}

		// Token: 0x060018C9 RID: 6345 RVA: 0x00123BF8 File Offset: 0x00121DF8
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			if (who.bathingClothes.Value || !who.IsLocalPlayer || who.onBridge.Value)
			{
				return;
			}
			this.indexOfMenuItemView.Value = 2;
			base.CurrentParentTileIndex = 2;
			for (int i = 0; i < 12; i++)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.position.X - 256, (int)who.position.X + 192), (float)Game1.random.Next((int)who.position.Y - 256, (int)who.position.Y + 192)), false, Game1.random.NextBool())
				});
			}
			if (this.PlayUseSounds)
			{
				who.playNearbySoundAll("wand", null, SoundContext.Default);
			}
			Game1.displayFarmer = false;
			who.temporarilyInvincible = true;
			who.temporaryInvincibilityTimer = -2000;
			who.Halt();
			who.faceDirection(2);
			who.CanMove = false;
			who.freezePause = 2000;
			Game1.flashAlpha = 1f;
			DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.wandWarpForReal), 1000);
			Rectangle playerBounds = who.GetBoundingBox();
			Rectangle r = new Rectangle(playerBounds.X, playerBounds.Y, 64, 64);
			r.Inflate(192, 192);
			int j = 0;
			Point playerTile = who.TilePoint;
			for (int xTile = playerTile.X + 8; xTile >= playerTile.X - 8; xTile--)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(6, new Vector2((float)xTile, (float)playerTile.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = j * 25,
						motion = new Vector2(-0.25f, 0f)
					}
				});
				j++;
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}

		// Token: 0x060018CA RID: 6346 RVA: 0x00123E5E File Offset: 0x0012205E
		public override bool actionWhenPurchased(string shopId)
		{
			Game1.player.mailReceived.Add("ReturnScepter");
			return base.actionWhenPurchased(shopId);
		}

		// Token: 0x060018CB RID: 6347 RVA: 0x00123E7C File Offset: 0x0012207C
		private void wandWarpForReal()
		{
			FarmHouse home = Utility.getHomeOfFarmer(Game1.player);
			if (home == null)
			{
				return;
			}
			Point position = home.getFrontDoorSpot();
			Game1.warpFarmer("Farm", position.X, position.Y, false);
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			this.lastUser.temporarilyInvincible = false;
			this.lastUser.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
			this.lastUser.CanMove = true;
		}
	}
}

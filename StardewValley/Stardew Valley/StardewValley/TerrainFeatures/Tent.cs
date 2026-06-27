using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000148 RID: 328
	public class Tent : LargeTerrainFeature
	{
		// Token: 0x06001A02 RID: 6658 RVA: 0x001330F0 File Offset: 0x001312F0
		public Tent() : base(true)
		{
			this.isDestroyedByNPCTrample = true;
		}

		// Token: 0x06001A03 RID: 6659 RVA: 0x0013310C File Offset: 0x0013130C
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.health, "health");
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x0013312B File Offset: 0x0013132B
		public Tent(Vector2 tileLocation) : base(true)
		{
			this.Tile = tileLocation;
			this.isDestroyedByNPCTrample = true;
		}

		// Token: 0x06001A05 RID: 6661 RVA: 0x00133150 File Offset: 0x00131350
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 1f) * 64, 192, 128);
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x00133193 File Offset: 0x00131393
		public override bool isPassable(Character c = null)
		{
			return c != null;
		}

		// Token: 0x06001A07 RID: 6663 RVA: 0x0013319C File Offset: 0x0013139C
		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
		{
			if (this.invincTimer <= 0)
			{
				this.health.Value--;
				this.invincTimer = 400;
				Game1.playSound("weed_cut", null);
			}
			return base.performToolAction(t, damage, tileLocation);
		}

		// Token: 0x06001A08 RID: 6664 RVA: 0x001331ED File Offset: 0x001313ED
		public override void dayUpdate()
		{
			this.health.Value = 0;
			Game1.displayFarmer = true;
			base.dayUpdate();
		}

		// Token: 0x06001A09 RID: 6665 RVA: 0x00133208 File Offset: 0x00131408
		public override bool performUseAction(Vector2 tileLocation)
		{
			Vector2 tilePosition = this.Tile;
			Vector2 playerGrab = Game1.player.GetGrabTile();
			if ((playerGrab == tilePosition || (playerGrab.X == tilePosition.X && playerGrab.Y >= tilePosition.Y)) && !Game1.newDay && Game1.shouldTimePass(false) && Game1.player.hasMoved && !Game1.player.passedOut)
			{
				Tent.lastTentTouchedByPlayer = tilePosition;
				this.Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), this.Location.createYesNoResponses(), "SleepTent", null);
			}
			return base.performUseAction(tileLocation);
		}

		// Token: 0x06001A0A RID: 6666 RVA: 0x001332AC File Offset: 0x001314AC
		public override void onDestroy()
		{
			GameLocation location = this.Location;
			Vector2 tilePosition = this.Tile;
			Game1.playSound("cut", null);
			Utility.addDirtPuffs(location, (int)tilePosition.X - 1, (int)tilePosition.Y - 1, 3, 2, 3);
			for (int i = 0; i < 16; i++)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(112 + Game1.random.Next(4) * 8, 248, 8, 8), 9999f, 1, 1, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random), false, false, tilePosition.Y * 64f / 10000f, 0.02f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2((float)Game1.random.Next(-1, 2), -5f),
					acceleration = new Vector2(0f, 0.16f)
				});
			}
		}

		// Token: 0x06001A0B RID: 6667 RVA: 0x001333B8 File Offset: 0x001315B8
		public override bool tickUpdate(GameTime time)
		{
			if (this.invincTimer > 0)
			{
				this.invincTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				this.shakeOffset = new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				if (this.invincTimer <= 0)
				{
					this.shakeOffset = Vector2.Zero;
				}
			}
			if (this.health.Value <= 0 && !this.goingToSleep)
			{
				this.onDestroy();
				return true;
			}
			return base.tickUpdate(time);
		}

		// Token: 0x06001A0C RID: 6668 RVA: 0x00133448 File Offset: 0x00131648
		public override void draw(SpriteBatch spriteBatch)
		{
			Vector2 tileLocation = this.Tile;
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(tileLocation * 64f + new Vector2(-2f, -1f) * 64f), new Rectangle?(new Rectangle(48, 208, 64, 48)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(tileLocation * 64f + new Vector2(-1f, -3f) * 64f) + this.shakeOffset, new Rectangle?(new Rectangle(0, 192, 48, 64)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, tileLocation.Y * 64f / 10000f);
		}

		// Token: 0x04000FF1 RID: 4081
		public readonly NetInt health = new NetInt(5);

		// Token: 0x04000FF2 RID: 4082
		private int invincTimer;

		// Token: 0x04000FF3 RID: 4083
		private Vector2 shakeOffset;

		// Token: 0x04000FF4 RID: 4084
		private bool goingToSleep;

		// Token: 0x04000FF5 RID: 4085
		public static Vector2 lastTentTouchedByPlayer = Vector2.Zero;
	}
}

using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using xTile.Layers;

namespace StardewValley.Monsters
{
	// Token: 0x02000227 RID: 551
	public class ShadowGirl : Monster
	{
		// Token: 0x06002480 RID: 9344 RVA: 0x0018EC92 File Offset: 0x0018CE92
		public ShadowGirl()
		{
		}

		// Token: 0x06002481 RID: 9345 RVA: 0x0018ECA8 File Offset: 0x0018CEA8
		public ShadowGirl(Vector2 position) : base("Shadow Girl", position)
		{
			base.IsWalkingTowardPlayer = false;
			this.moveTowardPlayerThreshold.Value = 8;
			Friendship friendship;
			if (Game1.MasterPlayer.friendshipData.TryGetValue("???", out friendship) && friendship.Points >= 1250)
			{
				base.DamageToFarmer = 0;
			}
		}

		// Token: 0x06002482 RID: 9346 RVA: 0x0018ED0B File Offset: 0x0018CF0B
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Girl");
		}

		// Token: 0x06002483 RID: 9347 RVA: 0x0018ED20 File Offset: 0x0018CF20
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x06002484 RID: 9348 RVA: 0x0018ED94 File Offset: 0x0018CF94
		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0));
		}

		// Token: 0x06002485 RID: 9349 RVA: 0x0018EDD4 File Offset: 0x0018CFD4
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, 64, 21), 64, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X + 10, this.Sprite.SourceRect.Y + 21, 64, 21), 42, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
		}

		// Token: 0x06002486 RID: 9350 RVA: 0x0018EEB8 File Offset: 0x0018D0B8
		public override void update(GameTime time, GameLocation location)
		{
			if (!location.farmers.Any())
			{
				return;
			}
			if (!base.Player.isRafting || !this.withinPlayerThreshold(4))
			{
				base.updateGlow();
				base.updateEmote(time);
				if (this.controller == null)
				{
					this.updateMovement(location, time);
				}
				if (this.controller != null && this.controller.update(time))
				{
					this.controller = null;
				}
			}
			this.behaviorAtGameTick(time);
			Layer backLayer = location.map.RequireLayer("Back");
			if (base.Position.X < 0f || base.Position.X > (float)(backLayer.LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(backLayer.LayerHeight * 64))
			{
				location.characters.Remove(this);
			}
		}

		// Token: 0x06002487 RID: 9351 RVA: 0x0018EF9C File Offset: 0x0018D19C
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			this.addedSpeed = 0f;
			base.speed = 3;
			if (this.howLongOnThisPosition > 500 && this.controller == null)
			{
				base.IsWalkingTowardPlayer = false;
				this.controller = new PathFindController(this, base.currentLocation, new Point(base.Player.TilePoint.X, base.Player.TilePoint.Y), Game1.random.Next(4), null, 300);
				this.timeBeforeAIMovementAgain = 2000f;
				this.howLongOnThisPosition = 0;
			}
			else if (this.controller == null)
			{
				base.IsWalkingTowardPlayer = true;
			}
			if (base.Position.Equals(this.lastPosition))
			{
				this.howLongOnThisPosition += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.howLongOnThisPosition = 0;
			}
			this.lastPosition = base.Position;
		}

		// Token: 0x04001589 RID: 5513
		public const int blockTimeBeforePathfinding = 500;

		// Token: 0x0400158A RID: 5514
		[XmlIgnore]
		public new Vector2 lastPosition = Vector2.Zero;

		// Token: 0x0400158B RID: 5515
		[XmlIgnore]
		public int howLongOnThisPosition;
	}
}

using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Monsters
{
	// Token: 0x0200022C RID: 556
	public class Spiker : Monster
	{
		// Token: 0x060024B7 RID: 9399 RVA: 0x00191CC9 File Offset: 0x0018FEC9
		public Spiker()
		{
		}

		// Token: 0x060024B8 RID: 9400 RVA: 0x00191CE0 File Offset: 0x0018FEE0
		public Spiker(Vector2 position, int direction) : base("Spiker", position)
		{
			this.Sprite.SpriteWidth = 16;
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
			this.targetDirection = direction;
			base.speed = 14;
			this.ignoreMovementAnimations = true;
			this.onCollision = new Monster.collisionBehavior(this.collide);
		}

		// Token: 0x060024B9 RID: 9401 RVA: 0x00191D52 File Offset: 0x0018FF52
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.moving, "moving");
		}

		// Token: 0x060024BA RID: 9402 RVA: 0x00191D74 File Offset: 0x0018FF74
		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			if (this.moving.Value != this._localMoving)
			{
				this._localMoving = this.moving.Value;
				if (this._localMoving)
				{
					if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
					{
						Game1.playSound("parry", null);
						return;
					}
				}
				else if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("hammer", null);
				}
			}
		}

		// Token: 0x060024BB RID: 9403 RVA: 0x00191E16 File Offset: 0x00190016
		public override void draw(SpriteBatch b)
		{
			this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position), (float)base.StandingPixel.Y / 10000f);
		}

		// Token: 0x060024BC RID: 9404 RVA: 0x00191E48 File Offset: 0x00190048
		private void collide(GameLocation location)
		{
			Rectangle bb = this.nextPosition(this.FacingDirection);
			using (FarmerCollection.Enumerator enumerator = location.farmers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetBoundingBox().Intersects(bb))
					{
						return;
					}
				}
			}
			if (this.moving.Value)
			{
				this.moving.Value = false;
				this.targetDirection = (this.targetDirection + 2) % 4;
				this.nextMoveCheck = 0.75f;
			}
		}

		// Token: 0x060024BD RID: 9405 RVA: 0x00191EE8 File Offset: 0x001900E8
		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		// Token: 0x060024BE RID: 9406 RVA: 0x00191EEA File Offset: 0x001900EA
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return -1;
		}

		// Token: 0x060024BF RID: 9407 RVA: 0x00191EF0 File Offset: 0x001900F0
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.nextMoveCheck > 0f)
			{
				this.nextMoveCheck -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this.nextMoveCheck <= 0f)
			{
				this.nextMoveCheck = 0.25f;
				foreach (Farmer farmer in base.currentLocation.farmers)
				{
					if ((this.targetDirection == 0 || this.targetDirection == 2) && Math.Abs(farmer.TilePoint.X - base.TilePoint.X) <= 1)
					{
						if (this.targetDirection == 0 && farmer.Position.Y < base.Position.Y)
						{
							this.moving.Value = true;
							break;
						}
						if (this.targetDirection == 2 && farmer.Position.Y > base.Position.Y)
						{
							this.moving.Value = true;
							break;
						}
					}
					if ((this.targetDirection == 3 || this.targetDirection == 1) && Math.Abs(farmer.TilePoint.Y - base.TilePoint.Y) <= 1)
					{
						if (this.targetDirection == 3 && farmer.Position.X < base.Position.X)
						{
							this.moving.Value = true;
							break;
						}
						if (this.targetDirection == 1 && farmer.Position.X > base.Position.X)
						{
							this.moving.Value = true;
							break;
						}
					}
				}
			}
			this.moveUp = false;
			this.moveDown = false;
			this.moveLeft = false;
			this.moveRight = false;
			if (this.moving.Value)
			{
				switch (this.targetDirection)
				{
				case 0:
					this.moveUp = true;
					break;
				case 1:
					this.moveRight = true;
					break;
				case 2:
					this.moveDown = true;
					break;
				case 3:
					this.moveLeft = true;
					break;
				}
				this.MovePosition(time, Game1.viewport, base.currentLocation);
			}
			this.faceDirection(2);
		}

		// Token: 0x040015AE RID: 5550
		[XmlIgnore]
		public int targetDirection;

		// Token: 0x040015AF RID: 5551
		[XmlIgnore]
		public NetBool moving = new NetBool(false);

		// Token: 0x040015B0 RID: 5552
		protected bool _localMoving;

		// Token: 0x040015B1 RID: 5553
		[XmlIgnore]
		public float nextMoveCheck;
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	// Token: 0x0200009E RID: 158
	public class NPCController
	{
		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000782 RID: 1922 RVA: 0x00049CF9 File Offset: 0x00047EF9
		private int CurrentPathX
		{
			get
			{
				if (this.pathIndex >= this.path.Count)
				{
					return 0;
				}
				return (int)this.path[this.pathIndex].X;
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000783 RID: 1923 RVA: 0x00049D27 File Offset: 0x00047F27
		private int CurrentPathY
		{
			get
			{
				if (this.pathIndex >= this.path.Count)
				{
					return 0;
				}
				return (int)this.path[this.pathIndex].Y;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000784 RID: 1924 RVA: 0x00049D55 File Offset: 0x00047F55
		private bool MovingHorizontally
		{
			get
			{
				return this.CurrentPathX != 0;
			}
		}

		// Token: 0x06000785 RID: 1925 RVA: 0x00049D60 File Offset: 0x00047F60
		public NPCController(Character n, List<Vector2> path, bool loop, NPCController.endBehavior endBehavior = null)
		{
			if (n == null)
			{
				return;
			}
			this.speed = n.speed;
			this.loop = loop;
			this.puppet = n;
			this.path = path;
			this.setMoving(true);
			this.behaviorAtEnd = endBehavior;
		}

		// Token: 0x06000786 RID: 1926 RVA: 0x00049DAF File Offset: 0x00047FAF
		public void destroyAtNextCrossroad()
		{
			this.destroyAtNextTurn = true;
		}

		// Token: 0x06000787 RID: 1927 RVA: 0x00049DB8 File Offset: 0x00047FB8
		private bool setMoving(bool newTarget)
		{
			if (this.puppet == null || this.pathIndex >= this.path.Count)
			{
				return false;
			}
			int facingDirection = 2;
			if (this.CurrentPathX > 0)
			{
				facingDirection = 1;
			}
			else if (this.CurrentPathX < 0)
			{
				facingDirection = 3;
			}
			else if (this.CurrentPathY < 0)
			{
				facingDirection = 0;
			}
			else if (this.CurrentPathY > 0)
			{
				facingDirection = 2;
			}
			this.puppet.Halt();
			this.puppet.faceDirection(facingDirection);
			if (this.CurrentPathX != 0 && this.CurrentPathY != 0)
			{
				this.pauseTime = this.CurrentPathY;
				facingDirection = this.CurrentPathX % 4;
				this.puppet.faceDirection(facingDirection);
				return true;
			}
			this.puppet.setMovingInFacingDirection();
			if (newTarget)
			{
				this.target = new Vector2(this.puppet.Position.X + (float)(this.CurrentPathX * 64), this.puppet.Position.Y + (float)(this.CurrentPathY * 64));
			}
			return true;
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x00049EB8 File Offset: 0x000480B8
		public bool update(GameTime time, GameLocation location, List<NPCController> allControllers)
		{
			this.puppet.speed = this.speed;
			bool reachedMeYet = false;
			foreach (NPCController i in allControllers)
			{
				if (i.puppet != null)
				{
					if (i.puppet.Equals(this.puppet))
					{
						reachedMeYet = true;
					}
					if (i.puppet.FacingDirection == this.puppet.FacingDirection && !i.puppet.Equals(this.puppet) && i.puppet.GetBoundingBox().Intersects(this.puppet.nextPosition(this.puppet.FacingDirection)))
					{
						if (reachedMeYet)
						{
							break;
						}
						return false;
					}
				}
			}
			Farmer player = this.puppet as Farmer;
			if (player != null)
			{
				player.setRunning(false, true);
				player.speed = 2;
				player.ignoreCollisions = true;
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.farmer != this.puppet)
				{
					player.updateMovementAnimation(time);
				}
			}
			this.puppet.MovePosition(time, Game1.viewport, location);
			if (this.pauseTime < 0 && !this.puppet.isMoving())
			{
				this.setMoving(false);
			}
			if (this.pauseTime < 0 && Math.Abs(Vector2.Distance(this.puppet.Position, this.target)) <= (float)this.puppet.Speed)
			{
				this.pathIndex++;
				if (this.destroyAtNextTurn)
				{
					return true;
				}
				if (!this.setMoving(true))
				{
					if (this.loop)
					{
						this.pathIndex = 0;
						this.setMoving(true);
					}
					else if (Game1.currentMinigame == null)
					{
						NPCController.endBehavior endBehavior = this.behaviorAtEnd;
						if (endBehavior != null)
						{
							endBehavior();
						}
						return true;
					}
				}
			}
			else if (this.pauseTime >= 0)
			{
				this.pauseTime -= time.ElapsedGameTime.Milliseconds;
				if (this.pauseTime < 0)
				{
					this.pathIndex++;
					if (this.destroyAtNextTurn)
					{
						return true;
					}
					if (!this.setMoving(true))
					{
						if (this.loop)
						{
							this.pathIndex = 0;
							this.setMoving(true);
						}
						else if (Game1.currentMinigame == null)
						{
							NPCController.endBehavior endBehavior2 = this.behaviorAtEnd;
							if (endBehavior2 != null)
							{
								endBehavior2();
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x040003F8 RID: 1016
		public Character puppet;

		// Token: 0x040003F9 RID: 1017
		private bool loop;

		// Token: 0x040003FA RID: 1018
		private bool destroyAtNextTurn;

		// Token: 0x040003FB RID: 1019
		private List<Vector2> path;

		// Token: 0x040003FC RID: 1020
		private Vector2 target;

		// Token: 0x040003FD RID: 1021
		private int pathIndex;

		// Token: 0x040003FE RID: 1022
		private int pauseTime = -1;

		// Token: 0x040003FF RID: 1023
		private int speed;

		// Token: 0x04000400 RID: 1024
		private NPCController.endBehavior behaviorAtEnd;

		// Token: 0x0200041C RID: 1052
		// (Invoke) Token: 0x06003CA4 RID: 15524
		public delegate void endBehavior();
	}
}

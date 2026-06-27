using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;

namespace StardewValley.Companions
{
	// Token: 0x02000374 RID: 884
	public class HungryFrogCompanion : HoppingCompanion
	{
		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06003604 RID: 13828 RVA: 0x002A8EF6 File Offset: 0x002A70F6
		// (set) Token: 0x06003605 RID: 13829 RVA: 0x002A8F1D File Offset: 0x002A711D
		private Monster attachedMonster
		{
			get
			{
				if (base.Owner != null)
				{
					return this.attachedMonsterField.Get(base.Owner.currentLocation) as Monster;
				}
				return null;
			}
			set
			{
				this.attachedMonsterField.Set(base.Owner.currentLocation, value);
			}
		}

		// Token: 0x06003606 RID: 13830 RVA: 0x002A8F38 File Offset: 0x002A7138
		public HungryFrogCompanion()
		{
		}

		// Token: 0x06003607 RID: 13831 RVA: 0x002A8F9C File Offset: 0x002A719C
		public HungryFrogCompanion(int variant)
		{
			this.whichVariant.Value = variant;
		}

		// Token: 0x06003608 RID: 13832 RVA: 0x002A900C File Offset: 0x002A720C
		public override void InitNetFields()
		{
			base.InitNetFields();
			base.NetFields.AddField(this.tongueOut, "tongueOut").AddField(this.tongueReturn, "tongueReturn").AddField(this.tonguePosition.NetFields, "tonguePosition.NetFields").AddField(this.tongueVelocity, "tongueVelocity").AddField(this.attachedMonsterField.NetFields, "attachedMonsterField.NetFields").AddField(this.fullnessTrigger, "fullnessTrigger");
			this.fullnessTrigger.onEvent += this.triggerFullnessTimer;
		}

		// Token: 0x06003609 RID: 13833 RVA: 0x002A90A8 File Offset: 0x002A72A8
		public override void Update(GameTime time, GameLocation location)
		{
			if (!this.tongueOut.Value)
			{
				base.Update(time, location);
			}
			if (!Game1.shouldTimePass(false))
			{
				return;
			}
			if (this.fullnessTime > 0f)
			{
				this.fullnessTime -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			this.lastHopTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.initialEquipDelay > 0f)
			{
				this.initialEquipDelay -= (float)time.ElapsedGameTime.TotalMilliseconds;
				return;
			}
			if (base.IsLocal)
			{
				this.monsterEatCheckTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.monsterEatCheckTimer >= 2000f && this.fullnessTime <= 0f && !this.tongueOut.Value)
				{
					this.monsterEatCheckTimer = 0f;
					if (!(location is SlimeHutch))
					{
						Monster closest_monster = Utility.findClosestMonsterWithinRange(location, base.Position, 300, false, null);
						if (closest_monster != null)
						{
							if (closest_monster is Bat && closest_monster.Age == 789)
							{
								this.monsterEatCheckTimer = 0f;
								return;
							}
							if (closest_monster.Name.Equals("Truffle Crab"))
							{
								this.monsterEatCheckTimer = 0f;
								return;
							}
							GreenSlime slime = closest_monster as GreenSlime;
							if (slime != null && slime.prismatic.Value)
							{
								this.monsterEatCheckTimer = 0f;
								return;
							}
							this.height = 0f;
							Vector2 motion = Utility.getVelocityTowardPoint(base.Position, closest_monster.getStandingPosition(), 12f);
							this.tongueOut.Value = true;
							this.tongueReturn.Value = false;
							this.tonguePosition.Value = base.Position + new Vector2(-32f, -32f) + new Vector2((float)((this.direction.Value != 3) ? 28 : 0), -20f);
							this.tongueVelocity.Value = motion;
							location.playSound("croak", null, null, SoundContext.Default);
							this.direction.Value = ((closest_monster.Position.X < base.Position.X) ? 3 : 1);
						}
					}
					this.tongueOutTimer = 0f;
				}
				if (this.tongueOut.Value)
				{
					this.tongueOutTimer += (float)time.ElapsedGameTime.TotalMilliseconds * (float)(this.tongueReturn.Value ? -1 : 1);
					this.tonguePosition.Value += this.tongueVelocity.Value;
					if (this.attachedMonster == null)
					{
						if (Vector2.Distance(base.Position, this.tonguePosition.Value) >= 300f)
						{
							this.tongueReachedMonster(null);
						}
						else
						{
							int damageSize = 40;
							Rectangle boundingBox = new Rectangle((int)this.tonguePosition.X + 32 - damageSize / 2, (int)this.tonguePosition.Y + 32 - damageSize / 2, damageSize, damageSize);
							Monster monster = base.Owner.currentLocation.doesPositionCollideWithCharacter(boundingBox, false) as Monster;
							if (monster != null)
							{
								this.tongueReachedMonster(monster);
							}
						}
					}
					if (this.attachedMonster != null)
					{
						this.attachedMonster.Position = this.tonguePosition.Value;
						this.attachedMonster.xVelocity = 0f;
						this.attachedMonster.yVelocity = 0f;
					}
					if (this.tongueReturn.Value)
					{
						Vector2 homingVector = Vector2.Subtract(base.Position + new Vector2(-32f, -32f) + new Vector2((float)((this.direction.Value != 3) ? 28 : 0), -20f), this.tonguePosition.Value);
						homingVector.Normalize();
						homingVector *= 12f;
						this.tongueVelocity.Value = homingVector;
					}
					if ((this.tongueReturn.Value && Vector2.Distance(base.Position, this.tonguePosition.Value) <= 48f) || this.tongueOutTimer <= 0f)
					{
						if (this.attachedMonster != null)
						{
							HotHead hothead = this.attachedMonster as HotHead;
							if (hothead != null && hothead.timeUntilExplode.Value > 0f)
							{
								GameLocation currentLocation = hothead.currentLocation;
								if (currentLocation != null)
								{
									currentLocation.netAudio.StopPlaying("fuse");
								}
							}
							if (this.attachedMonster.currentLocation != null)
							{
								this.attachedMonster.currentLocation.characters.Remove(this.attachedMonster);
							}
							else
							{
								location.characters.Remove(this.attachedMonster);
							}
							this.fullnessTrigger.Fire();
							this.attachedMonster = null;
						}
						Vector2.Distance(base.Position, this.tonguePosition.Value);
						this.tongueOut.Value = false;
						this.tongueReturn.Value = false;
					}
				}
			}
			else if (this.tongueOut.Value && this.attachedMonster != null)
			{
				this.attachedMonster.Position = this.tonguePosition.Value;
				this.attachedMonster.position.Paused = true;
				this.attachedMonster.xVelocity = 0f;
				this.attachedMonster.yVelocity = 0f;
			}
			this.fullnessTrigger.Poll();
		}

		// Token: 0x0600360A RID: 13834 RVA: 0x002A9621 File Offset: 0x002A7821
		public override void OnOwnerWarp()
		{
			this.attachedMonster = null;
			this.tongueOut.Value = false;
			this.tongueReturn.Value = false;
			base.OnOwnerWarp();
		}

		// Token: 0x0600360B RID: 13835 RVA: 0x002A9648 File Offset: 0x002A7848
		public override void Hop(float amount)
		{
			base.Hop(amount);
			if (this.fullnessTime > 0f)
			{
				Farmer owner = base.Owner;
				if (owner != null)
				{
					owner.currentLocation.localSound("frog_slap", null, null, SoundContext.Default);
				}
			}
			this.lastHopTimer = 0f;
		}

		// Token: 0x0600360C RID: 13836 RVA: 0x002A96A2 File Offset: 0x002A78A2
		private void triggerFullnessTimer()
		{
			this.fullnessTime = 12000f;
		}

		// Token: 0x0600360D RID: 13837 RVA: 0x002A96B0 File Offset: 0x002A78B0
		public void tongueReachedMonster(Monster m)
		{
			this.tongueReturn.Value = true;
			this.tongueVelocity.Value = this.tongueVelocity.Value * -1f;
			this.attachedMonster = m;
			if (m != null)
			{
				m.DamageToFarmer = 0;
				m.farmerPassesThrough = true;
				Farmer owner = base.Owner;
				if (owner == null)
				{
					return;
				}
				owner.currentLocation.localSound("fishSlap", null, null, SoundContext.Default);
			}
		}

		// Token: 0x0600360E RID: 13838 RVA: 0x002A9730 File Offset: 0x002A7930
		public override void Draw(SpriteBatch b)
		{
			Farmer owner = base.Owner;
			if (((owner != null) ? owner.currentLocation : null) == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
			{
				return;
			}
			Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
			SpriteEffects effect = SpriteEffects.None;
			Rectangle startingSourceRect = new Rectangle((this.fullnessTime > 0f) ? 128 : 0, 16 + this.whichVariant.Value * 16, 16, 16);
			Color c = (this.whichVariant.Value == 7) ? Utility.GetPrismaticColor(0, 1f) : Color.White;
			if (this.direction.Value == 3)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			if (this.tongueOut.Value)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 112, 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (this._position.Y - 12f) / 10000f);
			}
			else if (this.height > 0f)
			{
				if (this.gravity > 0f)
				{
					b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 16, 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (this._position.Y - 12f) / 10000f);
				}
				else if (this.gravity > -0.15f)
				{
					b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 32, 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (this._position.Y - 12f) / 10000f);
				}
				else
				{
					b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 48, 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (this._position.Y - 12f) / 10000f);
				}
			}
			else if (this.lastHopTimer > 5000f && !this.tongueOut.Value)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 80 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 >= 200.0) ? 16 : 0), 0)), c, 0f, new Vector2(8f, 16f), 4f, effect, (this._position.Y - 12f) / 10000f);
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(startingSourceRect), c, 0f, new Vector2(8f, 16f), 4f, effect, (this._position.Y - 12f) / 10000f);
			}
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(this.height, 1f)), SpriteEffects.None, 0f);
			if (this.tongueOut.Value)
			{
				Vector2 v = Game1.GlobalToLocal(this.tonguePosition.Value + new Vector2(32f));
				Vector2 v2 = Game1.GlobalToLocal(base.Position + new Vector2(-32f, -32f) + new Vector2((float)((this.direction.Value != 3) ? 44 : 24), 16f));
				Utility.drawLineWithScreenCoordinates((int)v2.X, (int)v2.Y, (int)v.X, (int)v.Y, b, Color.Red, 1f, 4);
				Texture2D projTex = Projectile.projectileSheet;
				Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 19, 16, 16);
				b.Draw(projTex, Game1.GlobalToLocal(this.tonguePosition.Value + new Vector2(32f, 32f)) + base.Owner.drawOffset, new Rectangle?(sourceRect), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x04002368 RID: 9064
		private const int RANGE = 300;

		// Token: 0x04002369 RID: 9065
		private const int FULLNESS_TIME = 12000;

		// Token: 0x0400236A RID: 9066
		public float fullnessTime;

		// Token: 0x0400236B RID: 9067
		private float monsterEatCheckTimer;

		// Token: 0x0400236C RID: 9068
		private float tongueOutTimer;

		// Token: 0x0400236D RID: 9069
		private readonly NetBool tongueOut = new NetBool(false);

		// Token: 0x0400236E RID: 9070
		private readonly NetBool tongueReturn = new NetBool(false);

		// Token: 0x0400236F RID: 9071
		private readonly NetPosition tonguePosition = new NetPosition();

		// Token: 0x04002370 RID: 9072
		private readonly NetVector2 tongueVelocity = new NetVector2();

		// Token: 0x04002371 RID: 9073
		private readonly NetNPCRef attachedMonsterField = new NetNPCRef();

		// Token: 0x04002372 RID: 9074
		private readonly NetEvent0 fullnessTrigger = new NetEvent0(false);

		// Token: 0x04002373 RID: 9075
		private float initialEquipDelay = 12000f;

		// Token: 0x04002374 RID: 9076
		private float lastHopTimer;
	}
}

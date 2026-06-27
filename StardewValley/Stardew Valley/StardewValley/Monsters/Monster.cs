using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Monsters
{
	// Token: 0x02000221 RID: 545
	[XmlInclude(typeof(AngryRoger))]
	[XmlInclude(typeof(Bat))]
	[XmlInclude(typeof(BigSlime))]
	[XmlInclude(typeof(BlueSquid))]
	[XmlInclude(typeof(Bug))]
	[XmlInclude(typeof(DinoMonster))]
	[XmlInclude(typeof(Duggy))]
	[XmlInclude(typeof(DustSpirit))]
	[XmlInclude(typeof(DwarvishSentry))]
	[XmlInclude(typeof(Fly))]
	[XmlInclude(typeof(Ghost))]
	[XmlInclude(typeof(GreenSlime))]
	[XmlInclude(typeof(Grub))]
	[XmlInclude(typeof(HotHead))]
	[XmlInclude(typeof(LavaLurk))]
	[XmlInclude(typeof(Leaper))]
	[XmlInclude(typeof(MetalHead))]
	[XmlInclude(typeof(Mummy))]
	[XmlInclude(typeof(RockCrab))]
	[XmlInclude(typeof(RockGolem))]
	[XmlInclude(typeof(Serpent))]
	[XmlInclude(typeof(ShadowBrute))]
	[XmlInclude(typeof(ShadowGirl))]
	[XmlInclude(typeof(ShadowGuy))]
	[XmlInclude(typeof(ShadowShaman))]
	[XmlInclude(typeof(Shooter))]
	[XmlInclude(typeof(Skeleton))]
	[XmlInclude(typeof(Spiker))]
	[XmlInclude(typeof(SquidKid))]
	public class Monster : NPC
	{
		// Token: 0x170003DA RID: 986
		// (get) Token: 0x060023F7 RID: 9207 RVA: 0x00188BC1 File Offset: 0x00186DC1
		[XmlIgnore]
		public Farmer Player
		{
			get
			{
				return this.findPlayer();
			}
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x060023F8 RID: 9208 RVA: 0x00188BC9 File Offset: 0x00186DC9
		// (set) Token: 0x060023F9 RID: 9209 RVA: 0x00188BD6 File Offset: 0x00186DD6
		[XmlIgnore]
		public int DamageToFarmer
		{
			get
			{
				return this.damageToFarmer.Value;
			}
			set
			{
				this.damageToFarmer.Value = value;
			}
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x060023FA RID: 9210 RVA: 0x00188BE4 File Offset: 0x00186DE4
		// (set) Token: 0x060023FB RID: 9211 RVA: 0x00188BF1 File Offset: 0x00186DF1
		[XmlIgnore]
		public int Health
		{
			get
			{
				return this.health.Value;
			}
			set
			{
				this.health.Value = value;
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x060023FC RID: 9212 RVA: 0x00188BFF File Offset: 0x00186DFF
		// (set) Token: 0x060023FD RID: 9213 RVA: 0x00188C0C File Offset: 0x00186E0C
		[XmlIgnore]
		public int MaxHealth
		{
			get
			{
				return this.maxHealth.Value;
			}
			set
			{
				this.maxHealth.Value = value;
			}
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x060023FE RID: 9214 RVA: 0x00188C1A File Offset: 0x00186E1A
		// (set) Token: 0x060023FF RID: 9215 RVA: 0x00188C27 File Offset: 0x00186E27
		[XmlIgnore]
		public int ExperienceGained
		{
			get
			{
				return this.experienceGained.Value;
			}
			set
			{
				this.experienceGained.Value = value;
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06002400 RID: 9216 RVA: 0x00188C35 File Offset: 0x00186E35
		// (set) Token: 0x06002401 RID: 9217 RVA: 0x00188C42 File Offset: 0x00186E42
		[XmlIgnore]
		public int Slipperiness
		{
			get
			{
				return this.slipperiness.Value;
			}
			set
			{
				this.slipperiness.Value = value;
			}
		}

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x06002402 RID: 9218 RVA: 0x00188C50 File Offset: 0x00186E50
		// (set) Token: 0x06002403 RID: 9219 RVA: 0x00188C5D File Offset: 0x00186E5D
		[XmlIgnore]
		public bool focusedOnFarmers
		{
			get
			{
				return this.netFocusedOnFarmers.Value;
			}
			set
			{
				this.netFocusedOnFarmers.Value = value;
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x06002404 RID: 9220 RVA: 0x00188C6B File Offset: 0x00186E6B
		// (set) Token: 0x06002405 RID: 9221 RVA: 0x00188C78 File Offset: 0x00186E78
		[XmlIgnore]
		public bool wildernessFarmMonster
		{
			get
			{
				return this.netWildernessFarmMonster.Value;
			}
			set
			{
				this.netWildernessFarmMonster.Value = value;
			}
		}

		// Token: 0x06002406 RID: 9222 RVA: 0x00188C88 File Offset: 0x00186E88
		public Monster()
		{
		}

		// Token: 0x06002407 RID: 9223 RVA: 0x00188DAB File Offset: 0x00186FAB
		public Monster(string name, Vector2 position) : this(name, position, 2)
		{
			base.Breather = false;
		}

		// Token: 0x06002408 RID: 9224 RVA: 0x00188DC0 File Offset: 0x00186FC0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.damageToFarmer, "damageToFarmer").AddField(this.health, "health").AddField(this.maxHealth, "maxHealth").AddField(this.resilience, "resilience").AddField(this.slipperiness, "slipperiness").AddField(this.experienceGained, "experienceGained").AddField(this.jitteriness, "jitteriness").AddField(this.missChance, "missChance").AddField(this.isGlider, "isGlider").AddField(this.mineMonster, "mineMonster").AddField(this.hasSpecialItem, "hasSpecialItem").AddField(this.objectsToDrop, "objectsToDrop").AddField(this.defaultAnimationInterval, "defaultAnimationInterval").AddField(this.netFocusedOnFarmers, "netFocusedOnFarmers").AddField(this.netWildernessFarmMonster, "netWildernessFarmMonster").AddField(this.deathAnimEvent, "deathAnimEvent").AddField(this.parryEvent, "parryEvent").AddField(this.trajectoryEvent, "trajectoryEvent").AddField(this.ignoreDamageLOS, "ignoreDamageLOS").AddField(this.synchedRotation, "synchedRotation").AddField(this.isHardModeMonster, "isHardModeMonster").AddField(this.stunTime, "stunTime");
			this.position.Field.AxisAlignedMovement = false;
			this.parryEvent.onEvent += this.handleParried;
			this.deathAnimEvent.onEvent += this.localDeathAnimation;
			this.trajectoryEvent.onEvent += this.doSetTrajectory;
		}

		// Token: 0x06002409 RID: 9225 RVA: 0x00188F94 File Offset: 0x00187194
		protected override Farmer findPlayer()
		{
			if (base.currentLocation == null)
			{
				return Game1.player;
			}
			Farmer bestFarmer = Game1.player;
			double bestPriority = double.MaxValue;
			foreach (Farmer f in base.currentLocation.farmers)
			{
				if (!f.hidden.Value)
				{
					double priority = this.findPlayerPriority(f);
					if (priority < bestPriority)
					{
						bestPriority = priority;
						bestFarmer = f;
					}
				}
			}
			return bestFarmer;
		}

		// Token: 0x0600240A RID: 9226 RVA: 0x00189024 File Offset: 0x00187224
		protected virtual double findPlayerPriority(Farmer f)
		{
			return (double)(f.Position - base.Position).LengthSquared();
		}

		// Token: 0x0600240B RID: 9227 RVA: 0x0018904B File Offset: 0x0018724B
		public virtual void onDealContactDamage(Farmer who)
		{
		}

		// Token: 0x0600240C RID: 9228 RVA: 0x0018904D File Offset: 0x0018724D
		public virtual List<Item> getExtraDropItems()
		{
			return new List<Item>();
		}

		// Token: 0x0600240D RID: 9229 RVA: 0x00189054 File Offset: 0x00187254
		public override bool withinPlayerThreshold()
		{
			return this.focusedOnFarmers || this.withinPlayerThreshold(this.moveTowardPlayerThreshold.Value);
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x0600240E RID: 9230 RVA: 0x00189071 File Offset: 0x00187271
		public override bool IsMonster
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x0600240F RID: 9231 RVA: 0x00189074 File Offset: 0x00187274
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002410 RID: 9232 RVA: 0x00189078 File Offset: 0x00187278
		public Monster(string name, Vector2 position, int facingDir) : base(new AnimatedSprite("Characters\\Monsters\\" + name), position, facingDir, name, null)
		{
			this.parseMonsterInfo(name);
			base.Breather = false;
		}

		// Token: 0x06002411 RID: 9233 RVA: 0x001891BD File Offset: 0x001873BD
		public virtual bool ShouldMonsterBeRemoved()
		{
			return this.Health <= 0;
		}

		// Token: 0x06002412 RID: 9234 RVA: 0x001891CB File Offset: 0x001873CB
		public virtual void drawAboveAllLayers(SpriteBatch b)
		{
		}

		// Token: 0x06002413 RID: 9235 RVA: 0x001891CD File Offset: 0x001873CD
		public override void draw(SpriteBatch b)
		{
			if (!this.isGlider.Value)
			{
				base.draw(b);
			}
		}

		// Token: 0x06002414 RID: 9236 RVA: 0x001891E3 File Offset: 0x001873E3
		public virtual bool isInvincible()
		{
			return this.invincibleCountdown > 0;
		}

		// Token: 0x06002415 RID: 9237 RVA: 0x001891EE File Offset: 0x001873EE
		public void setInvincibleCountdown(int time)
		{
			this.invincibleCountdown = time;
			base.startGlowing(new Color(255, 0, 0), false, 0.25f);
			this.glowingTransparency = 1f;
		}

		// Token: 0x06002416 RID: 9238 RVA: 0x0018921C File Offset: 0x0018741C
		protected int maxTimesReachedMineBottom()
		{
			int result = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				result = Math.Max(result, farmer.timesReachedMineBottom);
			}
			return result;
		}

		// Token: 0x06002417 RID: 9239 RVA: 0x00189278 File Offset: 0x00187478
		public virtual Debris ModifyMonsterLoot(Debris debris)
		{
			return debris;
		}

		// Token: 0x06002418 RID: 9240 RVA: 0x0018927B File Offset: 0x0018747B
		public virtual int GetBaseDifficultyLevel()
		{
			return 0;
		}

		// Token: 0x06002419 RID: 9241 RVA: 0x00189280 File Offset: 0x00187480
		public virtual void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			int target;
			if (this.DamageToFarmer != 0)
			{
				this.DamageToFarmer = (int)((float)this.DamageToFarmer * (1f + (float)additional_difficulty * 0.25f));
				target = 20 + (additional_difficulty - 1) * 20;
				if (this.DamageToFarmer < target)
				{
					this.DamageToFarmer = (int)Utility.Lerp((float)this.DamageToFarmer, (float)target, 0.5f);
				}
			}
			this.MaxHealth = (int)((float)this.MaxHealth * (1f + (float)additional_difficulty * 0.5f));
			target = 500 + (additional_difficulty - 1) * 300;
			if (this.MaxHealth < target)
			{
				this.MaxHealth = (int)Utility.Lerp((float)this.MaxHealth, (float)target, 0.5f);
			}
			this.Health = this.MaxHealth;
			this.resilience.Value += additional_difficulty * this.resilience.Value;
			this.isHardModeMonster.Value = true;
		}

		// Token: 0x0600241A RID: 9242 RVA: 0x00189368 File Offset: 0x00187568
		protected void parseMonsterInfo(string name)
		{
			string[] monsterInfo = DataLoader.Monsters(Game1.content)[name].Split('/', StringSplitOptions.None);
			this.Health = Convert.ToInt32(monsterInfo[0]);
			this.MaxHealth = this.Health;
			this.DamageToFarmer = Convert.ToInt32(monsterInfo[1]);
			this.isGlider.Value = Convert.ToBoolean(monsterInfo[4]);
			string[] objectsSplit = ArgUtility.SplitBySpace(monsterInfo[6]);
			this.objectsToDrop.Clear();
			for (int i = 0; i < objectsSplit.Length; i += 2)
			{
				if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
				{
					this.objectsToDrop.Add(objectsSplit[i]);
				}
			}
			this.resilience.Value = Convert.ToInt32(monsterInfo[7]);
			this.jitteriness.Value = Convert.ToDouble(monsterInfo[8]);
			base.willDestroyObjectsUnderfoot = false;
			base.moveTowardPlayer(Convert.ToInt32(monsterInfo[9]));
			base.speed = Convert.ToInt32(monsterInfo[10]);
			this.missChance.Value = Convert.ToDouble(monsterInfo[11]);
			this.mineMonster.Value = Convert.ToBoolean(monsterInfo[12]);
			if (this.maxTimesReachedMineBottom() >= 1 && this.mineMonster.Value)
			{
				this.resilience.Value += this.resilience.Value / 2;
				this.missChance.Value *= 2.0;
				this.Health += Game1.random.Next(0, this.Health);
				this.DamageToFarmer += Game1.random.Next(0, this.DamageToFarmer / 2);
			}
			try
			{
				this.ExperienceGained = Convert.ToInt32(monsterInfo[13]);
			}
			catch (Exception)
			{
				this.ExperienceGained = 1;
			}
			this.displayName = monsterInfo[14];
		}

		// Token: 0x0600241B RID: 9243 RVA: 0x0018954C File Offset: 0x0018774C
		public new static string GetDisplayName(string name)
		{
			string rawData;
			if (name == null || !DataLoader.Monsters(Game1.content).TryGetValue(name, out rawData))
			{
				return name;
			}
			return rawData.Split('/', StringSplitOptions.None)[14];
		}

		// Token: 0x0600241C RID: 9244 RVA: 0x00189580 File Offset: 0x00187780
		public virtual void InitializeForLocation(GameLocation location)
		{
			if (this.initializedForLocation)
			{
				return;
			}
			if (this.mineMonster.Value && this.maxTimesReachedMineBottom() >= 1)
			{
				double additional_chance = 0.0;
				MineShaft mine = location as MineShaft;
				if (mine != null)
				{
					additional_chance = (double)mine.GetAdditionalDifficulty() * 0.001;
				}
				if (Game1.random.NextDouble() < 0.001 + additional_chance)
				{
					this.objectsToDrop.Add(Game1.random.Choose("72", "74"));
				}
			}
			if (Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null) && Game1.random.NextDouble() < ((this.name.Value == "Dust Spirit") ? 0.02 : 0.05))
			{
				this.objectsToDrop.Add("890");
			}
			MineShaft mineShaft = location as MineShaft;
			if (mineShaft != null && mineShaft.mineLevel > 120 && !mineShaft.isSideBranch(-1))
			{
				int floor = mineShaft.mineLevel - 121;
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
				{
					float chance = 0.02f;
					chance += (float)(Game1.player.team.calicoEggSkullCavernRating.Value * 5 + 1 + floor) * 0.002f;
					if (chance > 0.5f)
					{
						chance = 0.5f;
					}
					if (Game1.random.NextBool(chance))
					{
						int count = Game1.random.Next(1, 4);
						for (int i = 0; i < count; i++)
						{
							this.objectsToDrop.Add("CalicoEgg");
						}
					}
				}
			}
			this.initializedForLocation = true;
		}

		// Token: 0x0600241D RID: 9245 RVA: 0x00189724 File Offset: 0x00187924
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name, 0, 16, 16);
		}

		// Token: 0x0600241E RID: 9246 RVA: 0x00189746 File Offset: 0x00187946
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
			AnimatedSprite sprite = this.Sprite;
			if (((sprite != null) ? sprite.Texture : null) == null)
			{
				this.reloadSprite(true);
			}
		}

		// Token: 0x0600241F RID: 9247 RVA: 0x00189763 File Offset: 0x00187963
		public virtual void shedChunks(int number)
		{
			this.shedChunks(number, 0.75f);
		}

		// Token: 0x06002420 RID: 9248 RVA: 0x00189774 File Offset: 0x00187974
		public virtual void shedChunks(int number, float scale)
		{
			if (this.Sprite.Texture.Height > this.Sprite.getHeight() * 4)
			{
				Point standingPixel = base.StandingPixel;
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, this.Sprite.getHeight() * 4 + 16, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f * scale);
			}
		}

		// Token: 0x06002421 RID: 9249 RVA: 0x00189801 File Offset: 0x00187A01
		public void deathAnimation()
		{
			this.sharedDeathAnimation();
			this.deathAnimEvent.Fire();
		}

		// Token: 0x06002422 RID: 9250 RVA: 0x00189814 File Offset: 0x00187A14
		protected virtual void sharedDeathAnimation()
		{
			this.shedChunks(Game1.random.Next(4, 9), 0.75f);
		}

		// Token: 0x06002423 RID: 9251 RVA: 0x0018982E File Offset: 0x00187A2E
		protected virtual void localDeathAnimation()
		{
		}

		// Token: 0x06002424 RID: 9252 RVA: 0x00189830 File Offset: 0x00187A30
		public void parried(int damage, Farmer who)
		{
			this.parryEvent.Fire(new ParryEventArgs(damage, who));
		}

		// Token: 0x06002425 RID: 9253 RVA: 0x00189844 File Offset: 0x00187A44
		private void handleParried(ParryEventArgs args)
		{
			int damage = args.damage;
			Farmer who = args.who;
			if (Game1.IsMasterGame)
			{
				float oldXVel = this.xVelocity;
				float oldYVel = this.yVelocity;
				if (this.xVelocity != 0f || this.yVelocity != 0f)
				{
					base.currentLocation.damageMonster(this.GetBoundingBox(), damage / 2, damage / 2 + 1, false, 0f, 0, 0f, 0f, false, who, false);
				}
				this.xVelocity = -oldXVel;
				this.yVelocity = -oldYVel;
				this.xVelocity *= (this.isGlider.Value ? 2f : 3.5f);
				this.yVelocity *= (this.isGlider.Value ? 2f : 3.5f);
			}
			this.setInvincibleCountdown(450);
		}

		// Token: 0x06002426 RID: 9254 RVA: 0x00189926 File Offset: 0x00187B26
		public virtual int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return this.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "hitEnemy");
		}

		// Token: 0x06002427 RID: 9255 RVA: 0x0018993C File Offset: 0x00187B3C
		public int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			this.slideAnimationTimer = 0;
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				this.Health -= actualDamage;
				base.currentLocation.playSound(hitSound, null, null, SoundContext.Default);
				base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
				if (this.Health <= 0)
				{
					this.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x06002428 RID: 9256 RVA: 0x001899D8 File Offset: 0x00187BD8
		public override void setTrajectory(Vector2 trajectory)
		{
			this.trajectoryEvent.Fire(trajectory);
		}

		// Token: 0x06002429 RID: 9257 RVA: 0x001899E8 File Offset: 0x00187BE8
		private void doSetTrajectory(Vector2 trajectory)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (Math.Abs(trajectory.X) > Math.Abs(this.xVelocity))
			{
				this.xVelocity = trajectory.X;
			}
			if (Math.Abs(trajectory.Y) > Math.Abs(this.yVelocity))
			{
				this.yVelocity = trajectory.Y;
			}
		}

		// Token: 0x0600242A RID: 9258 RVA: 0x00189A48 File Offset: 0x00187C48
		public virtual void behaviorAtGameTick(GameTime time)
		{
			if (this.timeBeforeAIMovementAgain > 0f)
			{
				this.timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (this.Player.isRafting && this.withinPlayerThreshold(4))
			{
				base.IsWalkingTowardPlayer = false;
				Point monsterPixel = base.StandingPixel;
				Point playerPixel = this.Player.StandingPixel;
				if (Math.Abs(playerPixel.Y - monsterPixel.Y) > 192)
				{
					if (playerPixel.X - monsterPixel.X > 0)
					{
						this.SetMovingLeft(true);
					}
					else
					{
						this.SetMovingRight(true);
					}
				}
				else if (playerPixel.Y - monsterPixel.Y > 0)
				{
					this.SetMovingUp(true);
				}
				else
				{
					this.SetMovingDown(true);
				}
				this.MovePosition(time, Game1.viewport, base.currentLocation);
			}
		}

		// Token: 0x0600242B RID: 9259 RVA: 0x00189B21 File Offset: 0x00187D21
		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		// Token: 0x0600242C RID: 9260 RVA: 0x00189B24 File Offset: 0x00187D24
		public override void update(GameTime time, GameLocation location)
		{
			if (Game1.IsMasterGame && !this.initializedForLocation && location != null)
			{
				this.InitializeForLocation(location);
				this.initializedForLocation = true;
			}
			this.parryEvent.Poll();
			this.trajectoryEvent.Poll();
			this.deathAnimEvent.Poll();
			this.position.UpdateExtrapolation((float)base.speed + this.addedSpeed);
			if (this.invincibleCountdown > 0)
			{
				this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (this.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			if (!location.farmers.Any())
			{
				return;
			}
			if (!this.Player.isRafting || !this.withinPlayerThreshold(4))
			{
				base.update(time, location);
			}
			if (Game1.IsMasterGame)
			{
				if (this.stunTime.Value <= 0)
				{
					this.behaviorAtGameTick(time);
				}
				else
				{
					this.stunTime.Value -= (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this.stunTime.Value < 0)
					{
						this.stunTime.Value = 0;
					}
				}
			}
			this.updateAnimation(time);
			if (Game1.IsMasterGame)
			{
				this.synchedRotation.Value = this.rotation;
			}
			else
			{
				this.rotation = this.synchedRotation.Value;
			}
			Layer backLayer = location.map.RequireLayer("Back");
			if (this.controller != null && this.withinPlayerThreshold(3))
			{
				this.controller = null;
			}
			if (!this.isGlider.Value && (base.Position.X < 0f || base.Position.X > (float)(backLayer.LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(backLayer.LayerHeight * 64)))
			{
				location.characters.Remove(this);
				return;
			}
			if (this.isGlider.Value && base.Position.X < -2000f)
			{
				this.Health = -500;
			}
		}

		// Token: 0x0600242D RID: 9261 RVA: 0x00189D35 File Offset: 0x00187F35
		protected void resetAnimationSpeed()
		{
			if (!this.ignoreMovementAnimations)
			{
				this.Sprite.interval = (float)this.defaultAnimationInterval.Value - ((float)base.speed + this.addedSpeed - 2f) * 20f;
			}
		}

		// Token: 0x0600242E RID: 9262 RVA: 0x00189D71 File Offset: 0x00187F71
		protected virtual void updateAnimation(GameTime time)
		{
			if (!Game1.IsMasterGame)
			{
				this.updateMonsterSlaveAnimation(time);
			}
			this.resetAnimationSpeed();
		}

		// Token: 0x0600242F RID: 9263 RVA: 0x00189D87 File Offset: 0x00187F87
		protected override void updateSlaveAnimation(GameTime time)
		{
		}

		// Token: 0x06002430 RID: 9264 RVA: 0x00189D89 File Offset: 0x00187F89
		protected virtual void updateMonsterSlaveAnimation(GameTime time)
		{
			this.Sprite.animateOnce(time);
		}

		// Token: 0x06002431 RID: 9265 RVA: 0x00189D98 File Offset: 0x00187F98
		public virtual bool ShouldActuallyMoveAwayFromPlayer()
		{
			return false;
		}

		// Token: 0x06002432 RID: 9266 RVA: 0x00189D9C File Offset: 0x00187F9C
		private void checkHorizontalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (who.Position.X > base.Position.X + 16f)
			{
				if (this.ShouldActuallyMoveAwayFromPlayer())
				{
					base.SetMovingOnlyLeft();
				}
				else
				{
					base.SetMovingOnlyRight();
				}
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(1), Game1.viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					success = true;
				}
				else
				{
					this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(this.lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (!success && who.Position.X < base.Position.X - 16f)
			{
				if (this.ShouldActuallyMoveAwayFromPlayer())
				{
					base.SetMovingOnlyRight();
				}
				else
				{
					base.SetMovingOnlyLeft();
				}
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(3), Game1.viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					success = true;
					return;
				}
				this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
				if (!base.Position.Equals(this.lastPosition))
				{
					scootSuccess = true;
				}
			}
		}

		// Token: 0x06002433 RID: 9267 RVA: 0x00189ED0 File Offset: 0x001880D0
		private void checkVerticalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (!success && who.Position.Y < base.Position.Y - 16f)
			{
				if (this.ShouldActuallyMoveAwayFromPlayer())
				{
					base.SetMovingOnlyDown();
				}
				else
				{
					base.SetMovingOnlyUp();
				}
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(0), Game1.viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					success = true;
				}
				else
				{
					this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(this.lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (!success && who.Position.Y > base.Position.Y + 16f)
			{
				if (this.ShouldActuallyMoveAwayFromPlayer())
				{
					base.SetMovingOnlyUp();
				}
				else
				{
					base.SetMovingOnlyDown();
				}
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(2), Game1.viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					success = true;
					return;
				}
				this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
				if (!base.Position.Equals(this.lastPosition))
				{
					scootSuccess = true;
				}
			}
		}

		// Token: 0x06002434 RID: 9268 RVA: 0x0018A00C File Offset: 0x0018820C
		public override void updateMovement(GameLocation location, GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
			{
				if ((this.moveTowardPlayerThreshold.Value == -1 || this.withinPlayerThreshold()) && this.timeBeforeAIMovementAgain <= 0f && this.IsMonster && !this.isGlider.Value)
				{
					Tile playerTile = location.map.RequireLayer("Back").Tiles[this.Player.TilePoint.X, this.Player.TilePoint.Y];
					if (playerTile == null || playerTile.Properties.ContainsKey("NPCBarrier"))
					{
						return;
					}
					if (this.skipHorizontal <= 0)
					{
						if (this.lastPosition.Equals(base.Position) && Game1.random.NextDouble() < 0.001)
						{
							switch (this.FacingDirection)
							{
							case 0:
							case 2:
								if (Game1.random.NextBool())
								{
									base.SetMovingOnlyRight();
								}
								else
								{
									base.SetMovingOnlyLeft();
								}
								break;
							case 1:
							case 3:
								if (Game1.random.NextBool())
								{
									base.SetMovingOnlyUp();
								}
								else
								{
									base.SetMovingOnlyDown();
								}
								break;
							}
							this.skipHorizontal = 700;
							return;
						}
						bool success = false;
						bool setMoving = false;
						bool scootSuccess = false;
						if (this.lastPosition.X == base.Position.X)
						{
							this.checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
							this.checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
						}
						else
						{
							this.checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
							this.checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
						}
						if (success)
						{
							this.skipHorizontal = 500;
						}
						else if (!setMoving)
						{
							this.Halt();
							base.faceGeneralDirection(this.Player.getStandingPosition(), 0, false);
						}
						if (scootSuccess)
						{
							return;
						}
					}
					else
					{
						this.skipHorizontal -= time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else
			{
				this.defaultMovementBehavior(time);
			}
			this.MovePosition(time, Game1.viewport, location);
			if (base.Position.Equals(this.lastPosition) && base.IsWalkingTowardPlayer && this.withinPlayerThreshold())
			{
				this.noMovementProgressNearPlayerBehavior();
			}
		}

		// Token: 0x06002435 RID: 9269 RVA: 0x0018A24B File Offset: 0x0018844B
		public virtual void noMovementProgressNearPlayerBehavior()
		{
			this.Halt();
			base.faceGeneralDirection(this.Player.getStandingPosition(), 0, false);
		}

		// Token: 0x06002436 RID: 9270 RVA: 0x0018A268 File Offset: 0x00188468
		public virtual void defaultMovementBehavior(GameTime time)
		{
			if (Game1.random.NextDouble() < this.jitteriness.Value * 1.8 && this.skipHorizontal <= 0)
			{
				switch (Game1.random.Next(6))
				{
				case 0:
					base.SetMovingOnlyUp();
					return;
				case 1:
					base.SetMovingOnlyRight();
					return;
				case 2:
					base.SetMovingOnlyDown();
					return;
				case 3:
					base.SetMovingOnlyLeft();
					return;
				default:
					this.Halt();
					break;
				}
			}
		}

		// Token: 0x06002437 RID: 9271 RVA: 0x0018A2E8 File Offset: 0x001884E8
		public virtual bool TakesDamageFromHitbox(Microsoft.Xna.Framework.Rectangle area_of_effect)
		{
			return this.GetBoundingBox().Intersects(area_of_effect);
		}

		// Token: 0x06002438 RID: 9272 RVA: 0x0018A304 File Offset: 0x00188504
		public virtual bool OverlapsFarmerForDamage(Farmer who)
		{
			return this.GetBoundingBox().Intersects(who.GetBoundingBox());
		}

		// Token: 0x06002439 RID: 9273 RVA: 0x0018A328 File Offset: 0x00188528
		public override void Halt()
		{
			int old_speed = base.speed;
			base.Halt();
			base.speed = old_speed;
		}

		// Token: 0x0600243A RID: 9274 RVA: 0x0018A34C File Offset: 0x0018854C
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (this.stunTime.Value > 0)
			{
				return;
			}
			this.lastPosition = base.Position;
			if (this.xVelocity != 0f || this.yVelocity != 0f)
			{
				if (double.IsNaN((double)this.xVelocity) || double.IsNaN((double)this.yVelocity))
				{
					this.xVelocity = 0f;
					this.yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
				int start_x = nextPosition.X;
				int start_y = nextPosition.Y;
				int end_x = nextPosition.X + (int)this.xVelocity;
				int end_y = nextPosition.Y - (int)this.yVelocity;
				int steps = 1;
				bool found_collision = false;
				bool isGroundedGlider = this is SquidKid;
				if (!this.isGlider.Value || isGroundedGlider)
				{
					if (nextPosition.Width > 0 && Math.Abs((int)this.xVelocity) > nextPosition.Width)
					{
						steps = (int)Math.Max((double)steps, Math.Ceiling((double)((float)Math.Abs((int)this.xVelocity) / (float)nextPosition.Width)));
					}
					if (nextPosition.Height > 0 && Math.Abs((int)this.yVelocity) > nextPosition.Height)
					{
						steps = (int)Math.Max((double)steps, Math.Ceiling((double)((float)Math.Abs((int)this.yVelocity) / (float)nextPosition.Height)));
					}
				}
				for (int i = 1; i <= steps; i++)
				{
					nextPosition.X = (int)Utility.Lerp((float)start_x, (float)end_x, (float)i / (float)steps);
					nextPosition.Y = (int)Utility.Lerp((float)start_y, (float)end_y, (float)i / (float)steps);
					bool isGliderForCollisions = this.isGlider.Value && !isGroundedGlider;
					if (currentLocation != null && currentLocation.isCollidingPosition(nextPosition, viewport, false, this.DamageToFarmer, isGliderForCollisions, this))
					{
						found_collision = true;
						break;
					}
				}
				if (!found_collision)
				{
					this.position.X += this.xVelocity;
					this.position.Y -= this.yVelocity;
					if (this.Slipperiness < 1000)
					{
						this.xVelocity -= this.xVelocity / (float)this.Slipperiness;
						this.yVelocity -= this.yVelocity / (float)this.Slipperiness;
						if (Math.Abs(this.xVelocity) <= 0.05f)
						{
							this.xVelocity = 0f;
						}
						if (Math.Abs(this.yVelocity) <= 0.05f)
						{
							this.yVelocity = 0f;
						}
					}
					if (!this.isGlider.Value && this.invincibleCountdown > 0)
					{
						this.slideAnimationTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.slideAnimationTimer < 0 && (Math.Abs(this.xVelocity) >= 3f || Math.Abs(this.yVelocity) >= 3f))
						{
							this.slideAnimationTimer = 100 - (int)(Math.Abs(this.xVelocity) * 2f + Math.Abs(this.yVelocity) * 2f);
							Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(6, base.getStandingPosition() + new Vector2(-32f, -32f), Color.White * 0.75f, 8, Game1.random.NextBool(), 20f, 0, -1, -1f, -1, 0)
								{
									scale = 0.75f
								}
							});
						}
					}
				}
				else if (this.isGlider.Value || this.Slipperiness >= 8)
				{
					if (this.isGlider.Value)
					{
						bool[] array = Utility.horizontalOrVerticalCollisionDirections(nextPosition, this, false);
						if (array[0])
						{
							this.xVelocity = -this.xVelocity;
							this.position.X += (float)Math.Sign(this.xVelocity);
							this.rotation += (float)(3.141592653589793 + (double)Game1.random.Next(-10, 11) * 3.141592653589793 / 500.0);
						}
						if (array[1])
						{
							this.yVelocity = -this.yVelocity;
							this.position.Y -= (float)Math.Sign(this.yVelocity);
							this.rotation += (float)(3.141592653589793 + (double)Game1.random.Next(-10, 11) * 3.141592653589793 / 500.0);
						}
					}
					if (this.Slipperiness < 1000)
					{
						this.xVelocity -= this.xVelocity / (float)this.Slipperiness / 4f;
						this.yVelocity -= this.yVelocity / (float)this.Slipperiness / 4f;
						if (Math.Abs(this.xVelocity) <= 0.05f)
						{
							this.xVelocity = 0f;
						}
						if (Math.Abs(this.yVelocity) <= 0.051f)
						{
							this.yVelocity = 0f;
						}
					}
				}
				else
				{
					this.xVelocity -= this.xVelocity / (float)this.Slipperiness;
					this.yVelocity -= this.yVelocity / (float)this.Slipperiness;
					if (Math.Abs(this.xVelocity) <= 0.05f)
					{
						this.xVelocity = 0f;
					}
					if (Math.Abs(this.yVelocity) <= 0.05f)
					{
						this.yVelocity = 0f;
					}
				}
				if (this.isGlider.Value)
				{
					return;
				}
			}
			if (this.moveUp)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, this.DamageToFarmer, this.isGlider.Value, this)) || this.isCharging)
				{
					this.position.Y -= (float)base.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateUp(time, 0, "");
					}
					this.FacingDirection = 0;
					this.faceDirection(0);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp = this.nextPosition(0);
					tmp.Width /= 4;
					bool leftCorner = currentLocation.isCollidingPosition(tmp, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					tmp.X += tmp.Width * 3;
					bool rightCorner = currentLocation.isCollidingPosition(tmp, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					if (leftCorner && !rightCorner && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner && !leftCorner && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(0), true))
						{
							currentLocation.playSound("stoneCrack", null, null, SoundContext.Default);
							this.position.Y -= (float)base.speed + this.addedSpeed;
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					Monster.collisionBehavior collisionBehavior = this.onCollision;
					if (collisionBehavior != null)
					{
						collisionBehavior(currentLocation);
					}
				}
			}
			else if (this.moveRight)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, this.DamageToFarmer, this.isGlider.Value, this)) || this.isCharging)
				{
					this.position.X += (float)base.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateRight(time, 0, "");
					}
					this.FacingDirection = 1;
					this.faceDirection(1);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp2 = this.nextPosition(1);
					tmp2.Height /= 4;
					bool topCorner = currentLocation.isCollidingPosition(tmp2, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					tmp2.Y += tmp2.Height * 3;
					bool bottomCorner = currentLocation.isCollidingPosition(tmp2, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					if (topCorner && !bottomCorner && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner && !topCorner && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(1), true))
						{
							currentLocation.playSound("stoneCrack", null, null, SoundContext.Default);
							this.position.X += (float)base.speed + this.addedSpeed;
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					Monster.collisionBehavior collisionBehavior2 = this.onCollision;
					if (collisionBehavior2 != null)
					{
						collisionBehavior2(currentLocation);
					}
				}
			}
			else if (this.moveDown)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, this.DamageToFarmer, this.isGlider.Value, this)) || this.isCharging)
				{
					this.position.Y += (float)base.speed + this.addedSpeed;
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateDown(time, 0, "");
					}
					this.FacingDirection = 2;
					this.faceDirection(2);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp3 = this.nextPosition(2);
					tmp3.Width /= 4;
					bool leftCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					tmp3.X += tmp3.Width * 3;
					bool rightCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					if (leftCorner2 && !rightCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner2 && !leftCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(2), true))
						{
							currentLocation.playSound("stoneCrack", null, null, SoundContext.Default);
							this.position.Y += (float)base.speed + this.addedSpeed;
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					Monster.collisionBehavior collisionBehavior3 = this.onCollision;
					if (collisionBehavior3 != null)
					{
						collisionBehavior3(currentLocation);
					}
				}
			}
			else if (this.moveLeft)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, this.DamageToFarmer, this.isGlider.Value, this)) || this.isCharging)
				{
					this.position.X -= (float)base.speed + this.addedSpeed;
					this.FacingDirection = 3;
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateLeft(time, 0, "");
					}
					this.faceDirection(3);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp4 = this.nextPosition(3);
					tmp4.Height /= 4;
					bool topCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					tmp4.Y += tmp4.Height * 3;
					bool bottomCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, false, this.DamageToFarmer, this.isGlider.Value, this);
					if (topCorner2 && !bottomCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner2 && !topCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, this.DamageToFarmer, this.isGlider.Value, this))
					{
						this.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(3), true))
						{
							currentLocation.playSound("stoneCrack", null, null, SoundContext.Default);
							this.position.X -= (float)base.speed + this.addedSpeed;
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					Monster.collisionBehavior collisionBehavior4 = this.onCollision;
					if (collisionBehavior4 != null)
					{
						collisionBehavior4(currentLocation);
					}
				}
			}
			else if (!this.ignoreMovementAnimations)
			{
				if (this.moveUp)
				{
					this.Sprite.AnimateUp(time, 0, "");
				}
				else if (this.moveRight)
				{
					this.Sprite.AnimateRight(time, 0, "");
				}
				else if (this.moveDown)
				{
					this.Sprite.AnimateDown(time, 0, "");
				}
				else if (this.moveLeft)
				{
					this.Sprite.AnimateLeft(time, 0, "");
				}
			}
			if (this.blockedInterval >= 5000)
			{
				base.speed = 4;
				this.isCharging = true;
				this.blockedInterval = 0;
			}
			if (this.DamageToFarmer > 0 && Game1.random.NextDouble() < 0.0003333333333333333)
			{
				string name = base.Name;
				if (!(name == "Shadow Guy"))
				{
					if (!(name == "Ghost"))
					{
						return;
					}
					currentLocation.playSound("ghost", null, null, SoundContext.Default);
				}
				else if (Game1.random.NextDouble() < 0.3)
				{
					if (Game1.random.NextBool())
					{
						currentLocation.playSound("grunt", null, null, SoundContext.Default);
						return;
					}
					currentLocation.playSound("shadowpeep", null, null, SoundContext.Default);
					return;
				}
			}
		}

		// Token: 0x0600243B RID: 9275 RVA: 0x0018B3BC File Offset: 0x001895BC
		protected virtual string GenerateLightSourceId(int identifier)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(identifier);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001550 RID: 5456
		public const int index_health = 0;

		// Token: 0x04001551 RID: 5457
		public const int index_damageToFarmer = 1;

		// Token: 0x04001552 RID: 5458
		public const int index_isGlider = 4;

		// Token: 0x04001553 RID: 5459
		public const int index_drops = 6;

		// Token: 0x04001554 RID: 5460
		public const int index_resilience = 7;

		// Token: 0x04001555 RID: 5461
		public const int index_jitteriness = 8;

		// Token: 0x04001556 RID: 5462
		public const int index_distanceThresholdToMoveTowardsPlayer = 9;

		// Token: 0x04001557 RID: 5463
		public const int index_speed = 10;

		// Token: 0x04001558 RID: 5464
		public const int index_missChance = 11;

		// Token: 0x04001559 RID: 5465
		public const int index_isMineMonster = 12;

		// Token: 0x0400155A RID: 5466
		public const int index_experiencePoints = 13;

		// Token: 0x0400155B RID: 5467
		public const int index_displayName = 14;

		// Token: 0x0400155C RID: 5468
		public const int defaultInvincibleCountdown = 450;

		// Token: 0x0400155D RID: 5469
		public float timeBeforeAIMovementAgain;

		// Token: 0x0400155E RID: 5470
		[XmlElement("damageToFarmer")]
		public readonly NetInt damageToFarmer = new NetInt();

		// Token: 0x0400155F RID: 5471
		[XmlElement("health")]
		public readonly NetIntDelta health = new NetIntDelta();

		// Token: 0x04001560 RID: 5472
		[XmlElement("maxHealth")]
		public readonly NetInt maxHealth = new NetInt();

		// Token: 0x04001561 RID: 5473
		[XmlElement("resilience")]
		public readonly NetInt resilience = new NetInt();

		// Token: 0x04001562 RID: 5474
		[XmlElement("slipperiness")]
		public readonly NetInt slipperiness = new NetInt(2);

		// Token: 0x04001563 RID: 5475
		[XmlElement("experienceGained")]
		public readonly NetInt experienceGained = new NetInt();

		// Token: 0x04001564 RID: 5476
		[XmlElement("jitteriness")]
		public readonly NetDouble jitteriness = new NetDouble();

		// Token: 0x04001565 RID: 5477
		[XmlElement("missChance")]
		public readonly NetDouble missChance = new NetDouble();

		// Token: 0x04001566 RID: 5478
		[XmlElement("isGlider")]
		public readonly NetBool isGlider = new NetBool();

		// Token: 0x04001567 RID: 5479
		[XmlElement("mineMonster")]
		public readonly NetBool mineMonster = new NetBool();

		// Token: 0x04001568 RID: 5480
		[XmlElement("hasSpecialItem")]
		public readonly NetBool hasSpecialItem = new NetBool();

		// Token: 0x04001569 RID: 5481
		[XmlIgnore]
		public readonly NetFloat synchedRotation = new NetFloat().Interpolated(true, true);

		// Token: 0x0400156A RID: 5482
		[XmlArrayItem("int")]
		public readonly NetStringList objectsToDrop = new NetStringList();

		// Token: 0x0400156B RID: 5483
		[XmlIgnore]
		public int skipHorizontal;

		// Token: 0x0400156C RID: 5484
		[XmlIgnore]
		public int invincibleCountdown;

		// Token: 0x0400156D RID: 5485
		[XmlIgnore]
		public readonly NetInt defaultAnimationInterval = new NetInt(175);

		// Token: 0x0400156E RID: 5486
		public readonly NetInt stunTime = new NetInt(0);

		// Token: 0x0400156F RID: 5487
		[XmlElement("initializedForLocation")]
		public bool initializedForLocation;

		// Token: 0x04001570 RID: 5488
		[XmlIgnore]
		public readonly NetBool netFocusedOnFarmers = new NetBool();

		// Token: 0x04001571 RID: 5489
		[XmlIgnore]
		public readonly NetBool netWildernessFarmMonster = new NetBool();

		// Token: 0x04001572 RID: 5490
		private readonly NetEvent1<ParryEventArgs> parryEvent = new NetEvent1<ParryEventArgs>
		{
			InterpolationWait = false
		};

		// Token: 0x04001573 RID: 5491
		private readonly NetEvent1Field<Vector2, NetVector2> trajectoryEvent = new NetEvent1Field<Vector2, NetVector2>
		{
			InterpolationWait = false
		};

		// Token: 0x04001574 RID: 5492
		[XmlIgnore]
		private readonly NetEvent0 deathAnimEvent = new NetEvent0(false);

		// Token: 0x04001575 RID: 5493
		[XmlElement("ignoreDamageLOS")]
		public readonly NetBool ignoreDamageLOS = new NetBool();

		// Token: 0x04001576 RID: 5494
		[XmlIgnore]
		public Monster.collisionBehavior onCollision;

		// Token: 0x04001577 RID: 5495
		[XmlElement("isHardModeMonster")]
		public NetBool isHardModeMonster = new NetBool(false);

		// Token: 0x04001578 RID: 5496
		private int slideAnimationTimer;

		// Token: 0x0200058B RID: 1419
		// (Invoke) Token: 0x060041C4 RID: 16836
		public delegate void collisionBehavior(GameLocation location);
	}
}

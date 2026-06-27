using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace StardewValley
{
	// Token: 0x020000A8 RID: 168
	public class FarmerSprite : AnimatedSprite
	{
		// Token: 0x17000134 RID: 308
		// (get) Token: 0x060009BF RID: 2495 RVA: 0x000683EF File Offset: 0x000665EF
		public override Character Owner
		{
			get
			{
				return this.owner;
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x060009C0 RID: 2496 RVA: 0x000683F7 File Offset: 0x000665F7
		public FarmerSprite.AnimationFrame CurrentAnimationFrame
		{
			get
			{
				if (base.CurrentAnimation == null)
				{
					return new FarmerSprite.AnimationFrame(0, 100, 0, false, false, null, false, 0);
				}
				return base.CurrentAnimation[this.currentAnimationIndex % base.CurrentAnimation.Count];
			}
		}

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x060009C1 RID: 2497 RVA: 0x0006842D File Offset: 0x0006662D
		public int CurrentSingleAnimation
		{
			get
			{
				if (base.CurrentAnimation != null)
				{
					return base.CurrentAnimation[0].frame;
				}
				return -1;
			}
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x0006844C File Offset: 0x0006664C
		public override void SetOwner(Character owner)
		{
			Farmer farmer = owner as Farmer;
			if (farmer == null)
			{
				throw new InvalidOperationException("The owner of a FarmerSprite must be a Farmer.");
			}
			this.owner = farmer;
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x060009C3 RID: 2499 RVA: 0x00068475 File Offset: 0x00066675
		// (set) Token: 0x060009C4 RID: 2500 RVA: 0x0006847D File Offset: 0x0006667D
		public override int CurrentFrame
		{
			get
			{
				return this.currentFrame;
			}
			set
			{
				if (this.currentFrame != value && !this.freezeUntilDialogueIsOver)
				{
					this.currentFrame = value;
					this.UpdateSourceRect();
				}
				if (value > FarmerRenderer.featureYOffsetPerFrame.Length - 1)
				{
					this.currentFrame = 0;
				}
			}
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x000684B0 File Offset: 0x000666B0
		public void setCurrentAnimation(FarmerSprite.AnimationFrame[] animation)
		{
			this.currentSingleAnimation = -1;
			this.currentAnimation.Clear();
			this.currentAnimation.AddRange(animation);
			this.oldFrame = this.CurrentFrame;
			this.currentAnimationIndex = 0;
			if (base.CurrentAnimation.Count > 0)
			{
				this.interval = (float)base.CurrentAnimation[0].milliseconds;
				this.CurrentFrame = base.CurrentAnimation[0].frame;
				this.currentAnimationFrames = base.CurrentAnimation.Count;
			}
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x0006853C File Offset: 0x0006673C
		public override void faceDirection(int direction)
		{
			Farmer farmer = this.owner;
			bool carrying = ((farmer != null) ? new bool?(farmer.IsCarrying()) : null) ?? false;
			if (!this.IsPlayingBasicAnimation(direction, carrying))
			{
				switch (direction)
				{
				case 0:
					this.setCurrentFrame(12, 1, 100, 1, false, carrying);
					break;
				case 1:
					this.setCurrentFrame(6, 1, 100, 1, false, carrying);
					break;
				case 2:
					this.setCurrentFrame(0, 1, 100, 1, false, carrying);
					break;
				case 3:
					this.setCurrentFrame(6, 1, 100, 1, true, carrying);
					break;
				}
				this.UpdateSourceRect();
			}
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x000685E0 File Offset: 0x000667E0
		public virtual bool IsPlayingBasicAnimation(int direction, bool carrying)
		{
			bool moving = false;
			if (this.owner != null && this.owner.CanMove && this.owner.isMoving())
			{
				moving = true;
			}
			switch (direction)
			{
			case 0:
				if (carrying)
				{
					if (!moving)
					{
						return this.CurrentFrame == 113;
					}
					if (this.currentSingleAnimation == 112 || this.currentSingleAnimation == 144)
					{
						return true;
					}
				}
				else
				{
					if (!moving)
					{
						return this.CurrentFrame == 17;
					}
					if (this.currentSingleAnimation == 16 || this.currentSingleAnimation == 48)
					{
						return true;
					}
				}
				break;
			case 1:
				if (carrying)
				{
					if (!moving)
					{
						return this.CurrentFrame == 105;
					}
					if (this.currentSingleAnimation == 104 || this.currentSingleAnimation == 136)
					{
						return true;
					}
				}
				else
				{
					if (!moving)
					{
						return this.CurrentFrame == 9;
					}
					if (this.currentSingleAnimation == 8 || this.currentSingleAnimation == 40)
					{
						return true;
					}
				}
				break;
			case 2:
				if (carrying)
				{
					if (!moving)
					{
						return this.CurrentFrame == 97;
					}
					if (this.currentSingleAnimation == 96 || this.currentSingleAnimation == 128)
					{
						return true;
					}
				}
				else
				{
					if (!moving)
					{
						return this.CurrentFrame == 1;
					}
					if (this.currentSingleAnimation == 0 || this.currentSingleAnimation == 32)
					{
						return true;
					}
				}
				break;
			case 3:
				if (carrying)
				{
					if (!moving)
					{
						return this.CurrentFrame == 121;
					}
					if (this.currentSingleAnimation == 120 || this.currentSingleAnimation == 152)
					{
						return true;
					}
				}
				else
				{
					if (!moving)
					{
						return this.CurrentFrame == 25;
					}
					if (this.currentSingleAnimation == 24 || this.currentSingleAnimation == 56)
					{
						return true;
					}
				}
				break;
			}
			return false;
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x00068788 File Offset: 0x00066988
		public void setCurrentSingleFrame(int which, short interval = 32000, bool secondaryArm = false, bool flip = false)
		{
			this.loopThisAnimation = false;
			this.currentAnimation.Clear();
			this.currentAnimation.Add(new FarmerSprite.AnimationFrame((int)((short)which), (int)interval, secondaryArm, flip, null, false));
			this.CurrentFrame = base.CurrentAnimation[0].frame;
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x000687D6 File Offset: 0x000669D6
		public void setCurrentFrame(int which)
		{
			this.setCurrentFrame(which, 0);
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x000687E0 File Offset: 0x000669E0
		public void setCurrentFrame(int which, int offset)
		{
			this.setCurrentFrame(which, offset, 100, 1, false, false);
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x000687F0 File Offset: 0x000669F0
		public void setCurrentFrameBackwards(int which, int offset, int interval, int numFrames, bool secondaryArm, bool flip)
		{
			this.getAnimationFromIndex(which, this, interval, numFrames, secondaryArm, flip);
			base.CurrentAnimation.Reverse();
			this.CurrentFrame = base.CurrentAnimation[Math.Min(base.CurrentAnimation.Count - 1, offset)].frame;
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x00068840 File Offset: 0x00066A40
		public void setCurrentFrame(int which, int offset, int interval, int numFrames, bool flip, bool secondaryArm)
		{
			this.getAnimationFromIndex(which, this, interval, numFrames, flip, secondaryArm);
			this.currentAnimationIndex = Math.Min(base.CurrentAnimation.Count - 1, offset);
			this.CurrentFrame = base.CurrentAnimation[this.currentAnimationIndex].frame;
			this.interval = (float)this.CurrentAnimationFrame.milliseconds;
			this.timer = 0f;
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x000688AE File Offset: 0x00066AAE
		// (set) Token: 0x060009CE RID: 2510 RVA: 0x000688B6 File Offset: 0x00066AB6
		public bool PauseForSingleAnimation
		{
			get
			{
				return this.pauseForSingleAnimation;
			}
			set
			{
				this.pauseForSingleAnimation = value;
			}
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x000688BF File Offset: 0x00066ABF
		// (set) Token: 0x060009D0 RID: 2512 RVA: 0x000688C7 File Offset: 0x00066AC7
		public int CurrentToolIndex
		{
			get
			{
				return this.currentToolIndex;
			}
			set
			{
				this.currentToolIndex = value;
			}
		}

		// Token: 0x060009D1 RID: 2513 RVA: 0x000688D0 File Offset: 0x00066AD0
		public FarmerSprite()
		{
			this.interval /= 2f;
			base.SpriteWidth = 16;
			base.SpriteHeight = 32;
			this.UpdateSourceRect();
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x00068934 File Offset: 0x00066B34
		public FarmerSprite(string texture) : base(texture)
		{
			this.interval /= 2f;
			base.SpriteWidth = 16;
			base.SpriteHeight = 32;
			this.UpdateSourceRect();
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x00068998 File Offset: 0x00066B98
		public void animate(int whichAnimation, GameTime time)
		{
			this.animate(whichAnimation, time.ElapsedGameTime.Milliseconds);
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x000689BC File Offset: 0x00066BBC
		public void animate(int whichAnimation, int milliseconds)
		{
			if (!this.PauseForSingleAnimation)
			{
				if (whichAnimation != this.currentSingleAnimation || base.CurrentAnimation == null || base.CurrentAnimation.Count <= 1)
				{
					float oldtimer = this.timer;
					int oldIndex = this.currentAnimationIndex;
					this.currentSingleAnimation = whichAnimation;
					this.setCurrentFrame(whichAnimation);
					this.timer = oldtimer;
					this.CurrentFrame = base.CurrentAnimation[Math.Min(oldIndex, base.CurrentAnimation.Count - 1)].frame;
					this.currentAnimationIndex = oldIndex % base.CurrentAnimation.Count;
					this.UpdateSourceRect();
				}
				this.animate(milliseconds);
			}
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x00068A5F File Offset: 0x00066C5F
		public void checkForSingleAnimation(GameTime time)
		{
			if (this.PauseForSingleAnimation)
			{
				if (!this.animateBackwards)
				{
					this.animateOnce(time);
					return;
				}
				this.animateBackwardsOnce(time);
			}
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x00068A80 File Offset: 0x00066C80
		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames)
		{
			this.animateOnce(whichAnimation, animationInterval, numberOfFrames, null);
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x00068A8C File Offset: 0x00066C8C
		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, AnimatedSprite.endOfAnimationBehavior endOfBehaviorFunction)
		{
			this.animateOnce(whichAnimation, animationInterval, numberOfFrames, endOfBehaviorFunction, false, false);
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x00068A9B File Offset: 0x00066C9B
		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, AnimatedSprite.endOfAnimationBehavior endOfBehaviorFunction, bool flip, bool secondaryArm)
		{
			this.animateOnce(whichAnimation, animationInterval, numberOfFrames, endOfBehaviorFunction, flip, secondaryArm, false);
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x00068AB0 File Offset: 0x00066CB0
		public void animateOnce(FarmerSprite.AnimationFrame[] animation, AnimatedSprite.endOfAnimationBehavior endOfBehaviorFunction = null)
		{
			this.currentSingleAnimation = -1;
			this.CurrentFrame = this.currentSingleAnimation;
			this.PauseForSingleAnimation = true;
			this.oldFrame = this.CurrentFrame;
			this.oldInterval = this.interval;
			this.currentSingleAnimationInterval = 100f;
			this.timer = 0f;
			this.currentAnimation.Clear();
			this.currentAnimation.AddRange(animation);
			this.CurrentFrame = base.CurrentAnimation[0].frame;
			this.currentAnimationFrames = base.CurrentAnimation.Count;
			this.currentAnimationIndex = 0;
			this.interval = (float)this.CurrentAnimationFrame.milliseconds;
			this.loopThisAnimation = false;
			this.endOfAnimationFunction = endOfBehaviorFunction;
			if (this.currentAnimationFrames > 0)
			{
				AnimatedSprite.endOfAnimationBehavior frameStartBehavior = base.CurrentAnimation[0].frameStartBehavior;
				if (frameStartBehavior == null)
				{
					return;
				}
				frameStartBehavior(this.owner);
			}
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x00068B95 File Offset: 0x00066D95
		public void showFrameUntilDialogueOver(int whichFrame)
		{
			this.freezeUntilDialogueIsOver = true;
			this.setCurrentFrame(whichFrame);
			this.UpdateSourceRect();
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00068BAC File Offset: 0x00066DAC
		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, AnimatedSprite.endOfAnimationBehavior endOfBehaviorFunction, bool flip, bool secondaryArm, bool backwards)
		{
			if (whichAnimation != this.currentSingleAnimation)
			{
				this.PauseForSingleAnimation = false;
			}
			if (!this.PauseForSingleAnimation && !this.freezeUntilDialogueIsOver)
			{
				this.currentSingleAnimation = whichAnimation;
				this.CurrentFrame = this.currentSingleAnimation;
				this.PauseForSingleAnimation = true;
				this.oldFrame = this.CurrentFrame;
				this.oldInterval = this.interval;
				this.currentSingleAnimationInterval = animationInterval;
				this.endOfAnimationFunction = endOfBehaviorFunction;
				this.timer = 0f;
				this.animatingBackwards = false;
				if (backwards)
				{
					this.animatingBackwards = true;
					this.setCurrentFrameBackwards(this.currentSingleAnimation, 0, (int)animationInterval, numberOfFrames, secondaryArm, flip);
				}
				else
				{
					this.setCurrentFrame(this.currentSingleAnimation, 0, (int)animationInterval, numberOfFrames, secondaryArm, flip);
				}
				AnimatedSprite.endOfAnimationBehavior frameStartBehavior = base.CurrentAnimation[0].frameStartBehavior;
				if (frameStartBehavior != null)
				{
					frameStartBehavior(this.owner);
				}
				if (this.owner.Stamina <= 0f && this.owner.usingTool.Value)
				{
					for (int i = 0; i < base.CurrentAnimation.Count; i++)
					{
						base.CurrentAnimation[i] = new FarmerSprite.AnimationFrame(base.CurrentAnimation[i].frame, base.CurrentAnimation[i].milliseconds * 2, base.CurrentAnimation[i].positionOffset, base.CurrentAnimation[i].armOffset, base.CurrentAnimation[i].flip, base.CurrentAnimation[i].frameStartBehavior, base.CurrentAnimation[i].frameEndBehavior, base.CurrentAnimation[i].xOffset);
					}
				}
				this.currentAnimationFrames = base.CurrentAnimation.Count;
				this.currentAnimationIndex = 0;
				this.interval = (float)this.CurrentAnimationFrame.milliseconds;
				if (this.owner.UsingTool && this.owner.CurrentTool != null)
				{
					this.CurrentToolIndex = this.owner.CurrentTool.CurrentParentTileIndex;
					if (this.owner.CurrentTool is FishingRod)
					{
						if (this.owner.FacingDirection == 3 || this.owner.FacingDirection == 1)
						{
							this.CurrentToolIndex = 55;
							return;
						}
						this.CurrentToolIndex = 48;
					}
				}
			}
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x00068E03 File Offset: 0x00067003
		public void animateBackwardsOnce(int whichAnimation, float animationInterval)
		{
			this.animateOnce(whichAnimation, animationInterval, 6, null, false, false, true);
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x00068E14 File Offset: 0x00067014
		public bool isUsingWeapon()
		{
			return this.PauseForSingleAnimation && ((this.currentSingleAnimation >= 232 && this.currentSingleAnimation < 264) || (this.currentSingleAnimation >= 272 && this.currentSingleAnimation < 280));
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x00068E63 File Offset: 0x00067063
		public int getWeaponTypeFromAnimation()
		{
			if (this.currentSingleAnimation >= 272 && this.currentSingleAnimation < 280)
			{
				return 1;
			}
			if (this.currentSingleAnimation >= 232 && this.currentSingleAnimation < 264)
			{
				return 3;
			}
			return -1;
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x00068EA0 File Offset: 0x000670A0
		public bool isOnToolAnimation()
		{
			return (this.PauseForSingleAnimation || this.owner.UsingTool) && ((this.currentSingleAnimation >= 160 && this.currentSingleAnimation < 192) || (this.currentSingleAnimation >= 232 && this.currentSingleAnimation < 264) || (this.currentSingleAnimation >= 272 && this.currentSingleAnimation < 280));
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x00068F16 File Offset: 0x00067116
		public bool isPassingOut()
		{
			return this.PauseForSingleAnimation && (this.currentSingleAnimation == 293 || this.CurrentFrame == 5);
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x00068F3C File Offset: 0x0006713C
		private void doneWithAnimation()
		{
			int currentFrame = this.CurrentFrame;
			this.CurrentFrame = currentFrame - 1;
			this.interval = this.oldInterval;
			if (!Game1.eventUp)
			{
				this.owner.CanMove = true;
				this.owner.Halt();
			}
			this.PauseForSingleAnimation = false;
			this.animatingBackwards = false;
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x00068F94 File Offset: 0x00067194
		private void currentAnimationTick()
		{
			if (this.currentAnimationIndex >= base.CurrentAnimation.Count)
			{
				return;
			}
			if (base.CurrentAnimation[this.currentAnimationIndex].frameEndBehavior != null)
			{
				base.CurrentAnimation[this.currentAnimationIndex].frameEndBehavior(this.owner);
			}
			this.currentAnimationIndex++;
			if (this.loopThisAnimation)
			{
				this.currentAnimationIndex %= base.CurrentAnimation.Count;
			}
			else if (this.currentAnimationIndex >= base.CurrentAnimation.Count)
			{
				this.loopThisAnimation = false;
				return;
			}
			if (base.CurrentAnimation[this.currentAnimationIndex].frameStartBehavior != null)
			{
				base.CurrentAnimation[this.currentAnimationIndex].frameStartBehavior(this.owner);
			}
			int currentAnimationIndex = this.currentAnimationIndex;
			List<FarmerSprite.AnimationFrame> currentAnimation = base.CurrentAnimation;
			int? num = (currentAnimation != null) ? new int?(currentAnimation.Count) : null;
			if (currentAnimationIndex < num.GetValueOrDefault() & num != null)
			{
				this.currentSingleAnimationInterval = (float)base.CurrentAnimation[this.currentAnimationIndex].milliseconds;
				this.CurrentFrame = base.CurrentAnimation[this.currentAnimationIndex].frame;
				this.interval = (float)base.CurrentAnimation[this.currentAnimationIndex].milliseconds;
				return;
			}
			this.owner.completelyStopAnimatingOrDoingAction();
			this.owner.forceCanMove();
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x00069118 File Offset: 0x00067318
		public override void UpdateSourceRect()
		{
			base.SourceRect = new Rectangle(this.CurrentFrame * base.SpriteWidth % 96, this.CurrentFrame * base.SpriteWidth / 96 * base.SpriteHeight, base.SpriteWidth, base.SpriteHeight);
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x00069158 File Offset: 0x00067358
		private new void animateOnce(GameTime time)
		{
			if (this.freezeUntilDialogueIsOver || this.owner == null)
			{
				return;
			}
			this.timer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > this.interval * this.intervalModifier)
			{
				this.currentAnimationTick();
				this.timer = 0f;
				if (this.currentAnimationIndex > this.currentAnimationFrames - 1)
				{
					AnimatedSprite.endOfAnimationBehavior frameEndBehavior = this.CurrentAnimationFrame.frameEndBehavior;
					if (frameEndBehavior != null)
					{
						frameEndBehavior(this.owner);
					}
					if (this.endOfAnimationFunction != null)
					{
						AnimatedSprite.endOfAnimationBehavior endOfAnimationFunction = this.endOfAnimationFunction;
						this.endOfAnimationFunction = null;
						endOfAnimationFunction(this.owner);
						MeleeWeapon weapon = this.owner.CurrentTool as MeleeWeapon;
						if (weapon == null || weapon.type.Value != 1)
						{
							this.doneWithAnimation();
						}
						return;
					}
					this.doneWithAnimation();
					if (this.owner.isEating)
					{
						this.owner.doneEating();
					}
				}
				int num = this.currentSingleAnimation;
				if (num <= 173)
				{
					if (num <= 165)
					{
						if (num - 160 <= 1 || num == 165)
						{
							Tool currentTool = this.owner.CurrentTool;
							if (currentTool != null)
							{
								currentTool.Update(2, this.currentAnimationIndex, this.owner);
							}
						}
					}
					else if (num != 168 && num - 172 > 1)
					{
					}
				}
				else if (num <= 181)
				{
					if (num == 176 || num - 180 <= 1)
					{
						Tool currentTool2 = this.owner.CurrentTool;
						if (currentTool2 != null)
						{
							currentTool2.Update(0, this.currentAnimationIndex, this.owner);
						}
					}
				}
				else if (num != 184 && num - 188 > 1)
				{
				}
				if (this.CurrentFrame == 109 && this.owner.ShouldHandleAnimationSound())
				{
					this.owner.playNearbySoundLocal("eat", null, SoundContext.Default);
				}
				if (this.isOnToolAnimation() && !this.isUsingWeapon() && this.currentAnimationIndex == 4 && this.currentToolIndex % 2 == 0 && !(this.owner.CurrentTool is FishingRod))
				{
					this.currentToolIndex++;
				}
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x00069388 File Offset: 0x00067588
		private void checkForFootstep()
		{
			if (Game1.player.isRidingHorse())
			{
				return;
			}
			if (this.owner == null || this.owner.currentLocation != Game1.currentLocation)
			{
				return;
			}
			Farmer farmer = this.owner;
			Vector2 tileLocationOfPlayer = (farmer != null) ? farmer.Tile : Game1.player.Tile;
			if (Game1.currentLocation.IsOutdoors || Game1.currentLocation.Name.ContainsIgnoreCase("mine") || Game1.currentLocation.Name.ContainsIgnoreCase("cave") || Game1.currentLocation.IsGreenhouse)
			{
				string stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocationOfPlayer.X, (int)tileLocationOfPlayer.Y, "Type", "Buildings", false);
				if (string.IsNullOrEmpty(stepType))
				{
					stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocationOfPlayer.X, (int)tileLocationOfPlayer.Y, "Type", "Back", false);
				}
				if (stepType != null)
				{
					if (!(stepType == "Dirt"))
					{
						if (!(stepType == "Stone"))
						{
							if (!(stepType == "Grass"))
							{
								if (stepType == "Wood")
								{
									this.currentStep = "woodyStep";
								}
							}
							else
							{
								this.currentStep = ((Game1.currentLocation.GetSeason() == Season.Winter) ? "snowyStep" : "grassyStep");
							}
						}
						else
						{
							this.currentStep = "stoneStep";
						}
					}
					else
					{
						this.currentStep = "sandyStep";
					}
				}
			}
			else
			{
				this.currentStep = "thudStep";
			}
			if (((this.currentSingleAnimation >= 32 && this.currentSingleAnimation <= 56) || (this.currentSingleAnimation >= 128 && this.currentSingleAnimation <= 152)) && this.currentAnimationIndex % 4 == 0)
			{
				string played_step = this.currentStep;
				played_step = this.owner.currentLocation.getFootstepSoundReplacement(played_step);
				if (this.owner.onBridge.Value)
				{
					if (this.owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(this.owner.Position, 384))
					{
						played_step = "thudStep";
					}
					SuspensionBridge bridge = this.owner.bridge;
					if (bridge != null)
					{
						bridge.OnFootstep(this.owner.Position);
					}
				}
				TerrainFeature terrainFeature;
				if (Game1.currentLocation.terrainFeatures.TryGetValue(tileLocationOfPlayer, out terrainFeature))
				{
					Flooring flooring = terrainFeature as Flooring;
					if (flooring != null)
					{
						played_step = flooring.getFootstepSound();
					}
				}
				Vector2 owner_position = this.owner.Position;
				if (this.owner.shouldShadowBeOffset)
				{
					owner_position += this.owner.drawOffset;
				}
				if (!(played_step == "sandyStep"))
				{
					if (played_step == "snowyStep")
					{
						TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(owner_position.X + 24f + (float)(Game1.random.Next(-4, 4) * 4), owner_position.Y + 8f + (float)(Game1.random.Next(-4, 4) * 4)), false, false, owner_position.Y / 10000000f, 0.01f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, (this.owner.FacingDirection == 1 || this.owner.FacingDirection == 3) ? -0.7853982f : 0f, 0f, false);
						Game1.currentLocation.temporarySprites.Add(sprite);
					}
				}
				else
				{
					TemporaryAnimatedSprite sprite2 = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(128, 2948, 64, 64), 80f, 8, 0, new Vector2(owner_position.X + 16f + (float)Game1.random.Next(-8, 8), owner_position.Y + (float)(Game1.random.Next(-3, -1) * 4)), false, Game1.random.NextBool(), owner_position.Y / 10000f, 0.03f, Color.Khaki * 0.45f, 0.75f + (float)Game1.random.Next(-3, 4) * 0.05f, 0f, 0f, 0f, false);
					Game1.currentLocation.temporarySprites.Add(sprite2);
					sprite2 = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(128, 2948, 64, 64), 80f, 8, 0, new Vector2(owner_position.X + 16f + (float)Game1.random.Next(-4, 4), owner_position.Y + (float)(Game1.random.Next(-3, -1) * 4)), false, Game1.random.NextBool(), owner_position.Y / 10000f, 0.03f, Color.Khaki * 0.45f, 0.55f + (float)Game1.random.Next(-3, 4) * 0.05f, 0f, 0f, 0f, false);
					sprite2.delayBeforeAnimationStart = 20;
					Game1.currentLocation.temporarySprites.Add(sprite2);
				}
				if (played_step != null && this.owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(this.owner.Position, 384) && (this.owner == Game1.player || !LocalMultiplayer.IsLocalMultiplayer(true)))
				{
					Game1.playSound(played_step, null);
					if (this.owner.boots.Value != null && this.owner.boots.Value.ItemId == "853")
					{
						Game1.playSound("jingleBell", null);
					}
				}
				foreach (Trinket trinket in this.owner.trinketItems)
				{
					if (trinket != null)
					{
						trinket.OnFootstep(this.owner);
					}
				}
				if (this.owner.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
				{
					Game1.stats.takeStep();
					return;
				}
			}
			else if ((this.currentSingleAnimation >= 0 && this.currentSingleAnimation <= 24) || (this.currentSingleAnimation >= 96 && this.currentSingleAnimation <= 120))
			{
				if (this.owner.onBridge.Value && this.currentAnimationIndex % 2 == 0)
				{
					if (this.owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(this.owner.Position, 384) && (this.owner == Game1.player || !LocalMultiplayer.IsLocalMultiplayer(true)))
					{
						Game1.playSound("thudStep", null);
					}
					SuspensionBridge bridge2 = this.owner.bridge;
					if (bridge2 != null)
					{
						bridge2.OnFootstep(this.owner.Position);
					}
					foreach (Trinket trinket2 in this.owner.trinketItems)
					{
						if (trinket2 != null)
						{
							trinket2.OnFootstep(this.owner);
						}
					}
				}
				if (this.currentAnimationIndex == 0 && this.owner.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
				{
					Game1.stats.takeStep();
				}
			}
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x00069AFC File Offset: 0x00067CFC
		private void animateBackwardsOnce(GameTime time)
		{
			this.timer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > this.currentSingleAnimationInterval)
			{
				int currentFrame = this.CurrentFrame;
				this.CurrentFrame = currentFrame - 1;
				this.timer = 0f;
				if (this.currentAnimationIndex > this.currentAnimationFrames - 1)
				{
					if (this.CurrentFrame < 63 || this.CurrentFrame > 96)
					{
						this.CurrentFrame = this.oldFrame;
					}
					else
					{
						this.CurrentFrame = this.CurrentFrame % 16 + 8;
					}
					this.interval = this.oldInterval;
					this.PauseForSingleAnimation = false;
					this.animatingBackwards = false;
					if (!Game1.eventUp)
					{
						this.owner.CanMove = true;
					}
					this.owner.Halt();
					if ((this.CurrentSingleAnimation >= 160 && this.CurrentSingleAnimation < 192) || (this.CurrentSingleAnimation >= 200 && this.CurrentSingleAnimation < 216) || (this.CurrentSingleAnimation >= 232 && this.CurrentSingleAnimation < 264))
					{
						Game1.toolAnimationDone(this.owner);
					}
				}
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x00069C30 File Offset: 0x00067E30
		public void setCurrentSingleAnimation(int which)
		{
			this.CurrentFrame = which;
			this.currentSingleAnimation = which;
			this.getAnimationFromIndex(which, this, 100, 1, false, false);
			List<FarmerSprite.AnimationFrame> currentAnimation = base.CurrentAnimation;
			if (currentAnimation != null && currentAnimation.Count > 0)
			{
				this.currentAnimationFrames = base.CurrentAnimation.Count;
				FarmerSprite.AnimationFrame frame = base.CurrentAnimation[0];
				this.interval = (float)frame.milliseconds;
				this.CurrentFrame = frame.frame;
			}
			if (this.interval <= 50f)
			{
				this.interval = 800f;
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x00069CC4 File Offset: 0x00067EC4
		private void animate(int Milliseconds)
		{
			this.timer += (float)Milliseconds;
			if (this.timer > this.interval * this.intervalModifier)
			{
				this.currentAnimationTick();
				this.timer = 0f;
				this.checkForFootstep();
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x00069D14 File Offset: 0x00067F14
		public override void StopAnimation()
		{
			bool animation_dirty = false;
			if (!this.pauseForSingleAnimation)
			{
				this.interval = 0f;
				if (this.CurrentFrame >= 64 && this.CurrentFrame <= 155 && this.owner != null && !this.owner.bathingClothes.Value)
				{
					switch (this.owner.FacingDirection)
					{
					case 0:
						this.CurrentFrame = 12;
						break;
					case 1:
						this.CurrentFrame = 6;
						break;
					case 2:
						this.CurrentFrame = 0;
						break;
					case 3:
						this.CurrentFrame = 6;
						break;
					}
					animation_dirty = true;
				}
				else if (this.owner != null)
				{
					bool carrying = this.owner.ActiveObject != null && this.owner.ActiveObject.IsHeldOverHead() && Game1.eventUp;
					if (!this.IsPlayingBasicAnimation(this.owner.FacingDirection, carrying))
					{
						animation_dirty = true;
						switch (this.owner.FacingDirection)
						{
						case 0:
							if (this.owner.ActiveObject != null && !Game1.eventUp)
							{
								this.setCurrentFrame(112, 1);
							}
							else
							{
								this.setCurrentFrame(16, 1);
							}
							break;
						case 1:
							if (this.owner.ActiveObject != null && !Game1.eventUp)
							{
								this.setCurrentFrame(104, 1);
							}
							else
							{
								this.setCurrentFrame(8, 1);
							}
							break;
						case 2:
							if (this.owner.ActiveObject != null && !Game1.eventUp)
							{
								this.setCurrentFrame(96, 1);
							}
							else
							{
								this.setCurrentFrame(0, 1);
							}
							break;
						case 3:
							if (this.owner.ActiveObject != null && !Game1.eventUp)
							{
								this.setCurrentFrame(120, 1);
							}
							else
							{
								this.setCurrentFrame(24, 1);
							}
							break;
						}
						this.currentSingleAnimation = -1;
					}
				}
				if (animation_dirty)
				{
					this.currentAnimationIndex = 0;
					this.UpdateSourceRect();
				}
			}
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x00069EEC File Offset: 0x000680EC
		public virtual void getAnimationFromIndex(int index, FarmerSprite requester, int interval, int numberOfFrames, bool flip, bool secondaryArm)
		{
			bool showCarryingArm = (index >= 96 && index < 160) || index == 232 || index == 248;
			Farmer farmer = requester.owner;
			if (((farmer != null) ? farmer.ActiveObject : null) != null && !requester.owner.ActiveObject.IsHeldOverHead())
			{
				showCarryingArm = false;
			}
			requester.loopThisAnimation = true;
			int frameOffset = 0;
			if (requester.owner != null && requester.owner.bathingClothes.Value)
			{
				frameOffset += 108;
			}
			List<FarmerSprite.AnimationFrame> outFrames = requester.currentAnimation;
			outFrames.Clear();
			float toolSpeedModifier = 1f;
			Farmer farmer2 = requester.owner;
			if (((farmer2 != null) ? farmer2.CurrentTool : null) != null)
			{
				toolSpeedModifier = requester.owner.CurrentTool.AnimationSpeedModifier;
			}
			requester.currentSingleAnimation = index;
			Farmer farmer3 = requester.owner;
			bool inBathingClothes = farmer3 != null && farmer3.bathingClothes.Value;
			if (index <= 88)
			{
				if (index <= 43)
				{
					if (index <= 16)
					{
						if (index <= 0)
						{
							if (index == -1)
							{
								outFrames.Add(new FarmerSprite.AnimationFrame(0, 100, showCarryingArm, false, null, false));
								return;
							}
							if (index != 0)
							{
								goto IL_22F0;
							}
						}
						else
						{
							if (index == 8)
							{
								goto IL_4D4;
							}
							if (index != 16)
							{
								goto IL_22F0;
							}
							goto IL_531;
						}
					}
					else if (index <= 32)
					{
						if (index == 24)
						{
							goto IL_592;
						}
						if (index != 32)
						{
							goto IL_22F0;
						}
						goto IL_5EF;
					}
					else
					{
						if (index == 40)
						{
							goto IL_694;
						}
						if (index != 43)
						{
							goto IL_22F0;
						}
						flip = (requester.owner.FacingDirection == 3);
						goto IL_22F0;
					}
				}
				else
				{
					if (index > 71)
					{
						if (index <= 83)
						{
							if (index != 72)
							{
								switch (index)
								{
								case 79:
									break;
								case 80:
									goto IL_15F0;
								case 81:
								case 82:
									goto IL_22F0;
								case 83:
									requester.loopThisAnimation = false;
									outFrames.Add(new FarmerSprite.AnimationFrame(0, 0, false, false, null, false));
									return;
								default:
									goto IL_22F0;
								}
							}
							requester.loopThisAnimation = false;
							outFrames.Add(new FarmerSprite.AnimationFrame(6, 0, false, false, null, false));
							return;
						}
						if (index != 87)
						{
							if (index != 88)
							{
								goto IL_22F0;
							}
							goto IL_163C;
						}
						IL_15F0:
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(12, 0, false, false, null, false));
						return;
					}
					if (index <= 56)
					{
						if (index == 48)
						{
							goto IL_73D;
						}
						if (index != 56)
						{
							goto IL_22F0;
						}
						goto IL_7E8;
					}
					else
					{
						if (index != 64 && index != 71)
						{
							goto IL_22F0;
						}
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(0, 0, false, false, null, false));
						return;
					}
				}
			}
			else if (index <= 152)
			{
				if (index <= 120)
				{
					if (index <= 104)
					{
						switch (index)
						{
						case 95:
							goto IL_163C;
						case 96:
							break;
						case 97:
							requester.loopThisAnimation = false;
							flip = (requester.owner.FacingDirection == 3);
							outFrames.Add(new FarmerSprite.AnimationFrame(97, 800, false, flip, null, false));
							return;
						default:
							if (index != 104)
							{
								goto IL_22F0;
							}
							goto IL_4D4;
						}
					}
					else
					{
						if (index == 112)
						{
							goto IL_531;
						}
						if (index != 120)
						{
							goto IL_22F0;
						}
						goto IL_592;
					}
				}
				else if (index <= 136)
				{
					if (index == 128)
					{
						goto IL_5EF;
					}
					if (index != 136)
					{
						goto IL_22F0;
					}
					goto IL_694;
				}
				else
				{
					if (index == 144)
					{
						goto IL_73D;
					}
					if (index != 152)
					{
						goto IL_22F0;
					}
					goto IL_7E8;
				}
			}
			else if (index <= 184)
			{
				if (index <= 168)
				{
					if (index == 160)
					{
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(66, (int)(150f * toolSpeedModifier), false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(67, (int)(40f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(68, (int)(40f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(69, (int)((short)((float)(170 + requester.owner.toolPower.Value * 30) * toolSpeedModifier)), false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(70, (int)(75f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
						return;
					}
					switch (index)
					{
					case 164:
					case 166:
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(54, 0, false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(54, 75, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(55, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
						outFrames.Add(new FarmerSprite.AnimationFrame(25, 500, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
						return;
					case 165:
					case 167:
						goto IL_22F0;
					case 168:
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(48, (int)(100f * toolSpeedModifier), false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(49, (int)(40f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(50, (int)(40f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(51, (int)((short)((float)(220 + requester.owner.toolPower.Value * 30) * toolSpeedModifier)), false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(52, (int)(75f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
						return;
					default:
						goto IL_22F0;
					}
				}
				else
				{
					switch (index)
					{
					case 172:
					case 174:
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(58, 0, false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(58, 75, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(59, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
						outFrames.Add(new FarmerSprite.AnimationFrame(45, 500, true, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
						return;
					case 173:
					case 175:
						goto IL_22F0;
					case 176:
						requester.loopThisAnimation = false;
						outFrames.Add(new FarmerSprite.AnimationFrame(36, (int)(100f * toolSpeedModifier), false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(37, (int)(40f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(38, (int)(40f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), false));
						outFrames.Add(new FarmerSprite.AnimationFrame(63, (int)((short)((float)(220 + requester.owner.toolPower.Value * 30) * toolSpeedModifier)), false, false, null, false));
						outFrames.Add(new FarmerSprite.AnimationFrame(62, (int)(75f * toolSpeedModifier), false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
						return;
					default:
						switch (index)
						{
						case 180:
						case 182:
							requester.loopThisAnimation = false;
							outFrames.Add(new FarmerSprite.AnimationFrame(62, 0, false, false, null, false));
							outFrames.Add(new FarmerSprite.AnimationFrame(62, 75, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
							outFrames.Add(new FarmerSprite.AnimationFrame(63, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
							outFrames.Add(new FarmerSprite.AnimationFrame(46, 500, true, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
							return;
						case 181:
						case 183:
							goto IL_22F0;
						case 184:
							requester.loopThisAnimation = false;
							outFrames.Add(new FarmerSprite.AnimationFrame(48, (int)(100f * toolSpeedModifier), false, true, null, false));
							outFrames.Add(new FarmerSprite.AnimationFrame(49, (int)(40f * toolSpeedModifier), false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
							outFrames.Add(new FarmerSprite.AnimationFrame(50, (int)(40f * toolSpeedModifier), false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), false));
							outFrames.Add(new FarmerSprite.AnimationFrame(51, (int)((short)((float)(220 + requester.owner.toolPower.Value * 30) * toolSpeedModifier)), false, true, null, false));
							outFrames.Add(new FarmerSprite.AnimationFrame(52, (int)(75f * toolSpeedModifier), false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
							return;
						default:
							goto IL_22F0;
						}
						break;
					}
				}
			}
			else if (index <= 216)
			{
				switch (index)
				{
				case 188:
				case 190:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 0, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 75, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 100, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					outFrames.Add(new FarmerSprite.AnimationFrame(45, 500, true, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 189:
				case 191:
				case 193:
				case 195:
				case 197:
					goto IL_22F0;
				case 192:
					index = 3;
					interval = 500;
					goto IL_22F0;
				case 194:
					index = 9;
					interval = 500;
					goto IL_22F0;
				case 196:
					index = 15;
					interval = 500;
					goto IL_22F0;
				case 198:
					index = 9;
					flip = true;
					interval = 500;
					goto IL_22F0;
				default:
				{
					if (index != 216)
					{
						goto IL_22F0;
					}
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 0));
					List<FarmerSprite.AnimationFrame> list = outFrames;
					int frame = 84;
					Farmer farmer4 = requester.owner;
					string a;
					if (farmer4 == null)
					{
						a = null;
					}
					else
					{
						Item mostRecentlyGrabbedItem = farmer4.mostRecentlyGrabbedItem;
						a = ((mostRecentlyGrabbedItem != null) ? mostRecentlyGrabbedItem.QualifiedItemId : null);
					}
					list.Add(new FarmerSprite.AnimationFrame(frame, (a == "(O)434") ? 1000 : 250, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showEatingItem), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(85, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showEatingItem), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(86, 1, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showEatingItem), true));
					outFrames.Add(new FarmerSprite.AnimationFrame(86, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showEatingItem), true));
					outFrames.Add(new FarmerSprite.AnimationFrame(87, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(88, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(87, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(88, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(87, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 250, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showEatingItem), false));
					return;
				}
				}
			}
			else
			{
				switch (index)
				{
				case 224:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(104, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(105, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(104, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(105, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(104, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(105, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(104, 350, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(105, 350, false, false, null, false));
					return;
				case 225:
				case 226:
				case 227:
				case 228:
				case 229:
				case 230:
				case 231:
				case 233:
				case 235:
				case 236:
				case 237:
				case 238:
				case 239:
				case 241:
				case 244:
				case 245:
				case 246:
				case 247:
				case 249:
				case 250:
				case 251:
				case 253:
				case 254:
				case 255:
				case 257:
				case 260:
				case 261:
				case 262:
				case 263:
				case 264:
				case 265:
				case 266:
				case 267:
				case 268:
				case 269:
				case 270:
				case 271:
				case 273:
				case 275:
				case 277:
					goto IL_22F0;
				case 232:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(24, 55, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(25, 45, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(26, 25, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(27, 25, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(28, 25, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(29, (int)((short)interval * 2), showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(29, 0, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 234:
					index = 28;
					secondaryArm = true;
					goto IL_22F0;
				case 240:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(30, 55, true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(31, 45, true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(32, 25, true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(33, 25, true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(34, 25, true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(35, (int)((short)interval * 2), true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(35, 0, true, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 242:
				case 243:
					index = 34;
					goto IL_22F0;
				case 248:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(36, 55, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(37, 45, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(38, 25, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(39, 25, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(40, 25, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(41, (int)((short)interval * 2), showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(41, 0, showCarryingArm, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 252:
					index = 40;
					secondaryArm = true;
					goto IL_22F0;
				case 256:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(30, 55, true, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(31, 45, true, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(32, 25, true, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(33, 25, true, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(34, 25, true, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(35, (int)((short)interval * 2), true, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(35, 0, true, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 258:
				case 259:
					index = 34;
					flip = true;
					goto IL_22F0;
				case 272:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(25, (int)((short)interval), true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(27, (int)((short)interval), true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(27, 0, true, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 274:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(34, (int)((short)interval), false, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(33, (int)((short)interval), false, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(33, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 276:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(40, (int)((short)interval), true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(38, (int)((short)interval), true, false, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(38, 0, true, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 278:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(34, (int)((short)interval), false, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(33, (int)((short)interval), false, true, new AnimatedSprite.endOfAnimationBehavior(this.owner.showSwordSwipe), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(33, 0, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true));
					return;
				case 279:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(62, 0, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(62, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(63, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(64, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(65, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(65, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					return;
				case 280:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 0, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(60, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(61, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(61, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					return;
				case 281:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(54, 0, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(54, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(55, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(56, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(57, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(57, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					return;
				case 282:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 0, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 100, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 100, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(60, 100, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(61, 100, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(61, 0, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showItemIntake), false));
					return;
				case 283:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(82, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(83, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Shears.playSnip), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(82, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(83, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 284:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(80, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(81, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Shears.playSnip), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(80, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(81, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 285:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(78, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(79, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Shears.playSnip), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(78, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(79, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 286:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(80, 400, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(81, 400, false, true, new AnimatedSprite.endOfAnimationBehavior(Shears.playSnip), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(80, 400, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(81, 400, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 287:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(62, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(63, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(62, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(63, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 288:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 289:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(54, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(55, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(54, 400));
					outFrames.Add(new FarmerSprite.AnimationFrame(55, 400, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 290:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 400, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 400, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(58, 400, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(59, 400, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), true));
					return;
				case 291:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(16, 1500));
					outFrames.Add(new FarmerSprite.AnimationFrame(16, 1, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.completelyStopAnimating), false));
					return;
				case 292:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(16, 500));
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 500));
					outFrames.Add(new FarmerSprite.AnimationFrame(16, 500));
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 500));
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 1, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.completelyStopAnimating), false));
					return;
				case 293:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(16, 1000));
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 500));
					outFrames.Add(new FarmerSprite.AnimationFrame(16, 1000));
					outFrames.Add(new FarmerSprite.AnimationFrame(4, 200));
					outFrames.Add(new FarmerSprite.AnimationFrame(5, 2000, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.doSleepEmote), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(5, 2000, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.passOutFromTired), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(5, 2000));
					return;
				case 294:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(0, 1));
					outFrames.Add(new FarmerSprite.AnimationFrame(90, 250));
					outFrames.Add(new FarmerSprite.AnimationFrame(91, 150));
					outFrames.Add(new FarmerSprite.AnimationFrame(92, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(93, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.drinkGlug), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(92, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(93, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.drinkGlug), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(92, 250, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(93, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.drinkGlug), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(91, 250));
					outFrames.Add(new FarmerSprite.AnimationFrame(90, 50));
					return;
				case 295:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(76, 100, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(38, 40, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(63, 40, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(62, 80, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(63, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(FishingRod.doneWithCastingAnimation), true));
					return;
				case 296:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(48, 100, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(49, 40, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(50, 40, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(51, 80, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(52, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(FishingRod.doneWithCastingAnimation), true));
					return;
				case 297:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(66, 100, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(67, 40, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(68, 40, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(69, 80, false, false, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(70, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(FishingRod.doneWithCastingAnimation), true));
					return;
				case 298:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(48, 100, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(49, 40, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(50, 40, false, true, new AnimatedSprite.endOfAnimationBehavior(Farmer.showToolSwipeEffect), false));
					outFrames.Add(new FarmerSprite.AnimationFrame(51, 80, false, true, null, false));
					outFrames.Add(new FarmerSprite.AnimationFrame(52, 200, false, true, new AnimatedSprite.endOfAnimationBehavior(FishingRod.doneWithCastingAnimation), true));
					return;
				case 299:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(76, 5000, false, false, null, false));
					return;
				case 300:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(72, 5000, false, false, null, false));
					return;
				case 301:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(74, 5000, false, false, null, false));
					return;
				case 302:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(72, 5000, false, true, null, false));
					return;
				case 303:
				{
					int armOffset = Math.Max(3, 3 * requester.owner.CurrentTool.UpgradeLevel);
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(124, 150, 0, armOffset, true, new AnimatedSprite.endOfAnimationBehavior(Pan.playSlosh), null, 0));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(125, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(124, 150, 0, armOffset, true, new AnimatedSprite.endOfAnimationBehavior(Pan.playSlosh), null, 0));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(125, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(124, 150, 0, armOffset, true, new AnimatedSprite.endOfAnimationBehavior(Pan.playSlosh), null, 0));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(125, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 150, armOffset, true));
					outFrames.Add(new FarmerSprite.AnimationFrame(124, 150, 0, armOffset, true, new AnimatedSprite.endOfAnimationBehavior(Pan.playSlosh), null, 0));
					outFrames.Add(new FarmerSprite.AnimationFrame(123, 500, 0, armOffset, true, null, new AnimatedSprite.endOfAnimationBehavior(Farmer.useTool), 0));
					return;
				}
				case 304:
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(84, 99999999, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.showEatingItem), false));
					return;
				default:
					if (index != 999996)
					{
						goto IL_22F0;
					}
					requester.loopThisAnimation = false;
					outFrames.Add(new FarmerSprite.AnimationFrame(96, 800, false, false, null, false));
					return;
				}
			}
			outFrames.Add(new FarmerSprite.AnimationFrame(1 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(2 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(frameOffset, 200, showCarryingArm, false, inBathingClothes));
			return;
			IL_4D4:
			outFrames.Add(new FarmerSprite.AnimationFrame(7 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(6 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(8 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(6 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			return;
			IL_531:
			outFrames.Add(new FarmerSprite.AnimationFrame(13 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(12 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(14 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(12 + frameOffset, 200, showCarryingArm, false, inBathingClothes));
			return;
			IL_592:
			outFrames.Add(new FarmerSprite.AnimationFrame(7 + frameOffset, 200, showCarryingArm, true, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(6 + frameOffset, 200, showCarryingArm, true, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(8 + frameOffset, 200, showCarryingArm, true, inBathingClothes));
			outFrames.Add(new FarmerSprite.AnimationFrame(6 + frameOffset, 200, showCarryingArm, true, inBathingClothes));
			return;
			IL_5EF:
			outFrames.Add(new FarmerSprite.AnimationFrame(0, 90, showCarryingArm, false, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(1, 60, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(18, 120, -4, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(1, 60, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(0, 90, showCarryingArm, false, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(2, 60, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(19, 120, -4, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(2, 60, -2, showCarryingArm, false, null, false, 0));
			return;
			IL_694:
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 80, showCarryingArm, false, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 10, -1, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(20, 140, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(11, 100, 0, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 80, showCarryingArm, false, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 10, -1, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(21, 140, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(17, 100, 0, showCarryingArm, false, null, false, 0));
			return;
			IL_73D:
			outFrames.Add(new FarmerSprite.AnimationFrame(12, 90, showCarryingArm, false, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(13, 60, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(22, 120, -3, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(13, 60, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(12, 90, showCarryingArm, false, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(14, 60, -2, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(23, 120, -3, showCarryingArm, false, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(14, 60, -2, showCarryingArm, false, null, false, 0));
			return;
			IL_7E8:
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 80, showCarryingArm, true, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 10, -1, showCarryingArm, true, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(20, 140, -2, showCarryingArm, true, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(11, 100, 0, showCarryingArm, true, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 80, showCarryingArm, true, null, false));
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 10, -1, showCarryingArm, true, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(21, 140, -2, showCarryingArm, true, null, false, 0));
			outFrames.Add(new FarmerSprite.AnimationFrame(17, 100, 0, showCarryingArm, true, null, false, 0));
			return;
			IL_163C:
			requester.loopThisAnimation = false;
			outFrames.Add(new FarmerSprite.AnimationFrame(6, 0, false, true, null, false));
			return;
			IL_22F0:
			if (index > FarmerRenderer.featureYOffsetPerFrame.Length - 1)
			{
				index = 0;
			}
			requester.loopThisAnimation = false;
			for (int i = 0; i < numberOfFrames; i++)
			{
				outFrames.Add(new FarmerSprite.AnimationFrame((int)((short)(i + index)), (int)((short)interval), secondaryArm, flip, null, false));
			}
		}

		// Token: 0x040005D2 RID: 1490
		public const int walkDown = 0;

		// Token: 0x040005D3 RID: 1491
		public const int walkRight = 8;

		// Token: 0x040005D4 RID: 1492
		public const int walkUp = 16;

		// Token: 0x040005D5 RID: 1493
		public const int walkLeft = 24;

		// Token: 0x040005D6 RID: 1494
		public const int runDown = 32;

		// Token: 0x040005D7 RID: 1495
		public const int runRight = 40;

		// Token: 0x040005D8 RID: 1496
		public const int runUp = 48;

		// Token: 0x040005D9 RID: 1497
		public const int runLeft = 56;

		// Token: 0x040005DA RID: 1498
		public const int grabDown = 64;

		// Token: 0x040005DB RID: 1499
		public const int grabRight = 72;

		// Token: 0x040005DC RID: 1500
		public const int grabUp = 80;

		// Token: 0x040005DD RID: 1501
		public const int grabLeft = 88;

		// Token: 0x040005DE RID: 1502
		public const int carryWalkDown = 96;

		// Token: 0x040005DF RID: 1503
		public const int carryWalkRight = 104;

		// Token: 0x040005E0 RID: 1504
		public const int carryWalkUp = 112;

		// Token: 0x040005E1 RID: 1505
		public const int carryWalkLeft = 120;

		// Token: 0x040005E2 RID: 1506
		public const int carryRunDown = 128;

		// Token: 0x040005E3 RID: 1507
		public const int carryRunRight = 136;

		// Token: 0x040005E4 RID: 1508
		public const int carryRunUp = 144;

		// Token: 0x040005E5 RID: 1509
		public const int carryRunLeft = 152;

		// Token: 0x040005E6 RID: 1510
		public const int toolDown = 160;

		// Token: 0x040005E7 RID: 1511
		public const int toolRight = 168;

		// Token: 0x040005E8 RID: 1512
		public const int toolUp = 176;

		// Token: 0x040005E9 RID: 1513
		public const int toolLeft = 184;

		// Token: 0x040005EA RID: 1514
		public const int toolChooseDown = 192;

		// Token: 0x040005EB RID: 1515
		public const int toolChooseRight = 194;

		// Token: 0x040005EC RID: 1516
		public const int toolChooseUp = 196;

		// Token: 0x040005ED RID: 1517
		public const int toolChooseLeft = 198;

		// Token: 0x040005EE RID: 1518
		public const int seedThrowDown = 200;

		// Token: 0x040005EF RID: 1519
		public const int seedThrowRight = 204;

		// Token: 0x040005F0 RID: 1520
		public const int seedThrowUp = 208;

		// Token: 0x040005F1 RID: 1521
		public const int seedThrowLeft = 212;

		// Token: 0x040005F2 RID: 1522
		public const int eat = 216;

		// Token: 0x040005F3 RID: 1523
		public const int sick = 224;

		// Token: 0x040005F4 RID: 1524
		public const int swordswipeDown = 232;

		// Token: 0x040005F5 RID: 1525
		public const int swordswipeRight = 240;

		// Token: 0x040005F6 RID: 1526
		public const int swordswipeUp = 248;

		// Token: 0x040005F7 RID: 1527
		public const int swordswipeLeft = 256;

		// Token: 0x040005F8 RID: 1528
		public const int punchDown = 272;

		// Token: 0x040005F9 RID: 1529
		public const int punchRight = 274;

		// Token: 0x040005FA RID: 1530
		public const int punchUp = 276;

		// Token: 0x040005FB RID: 1531
		public const int punchLeft = 278;

		// Token: 0x040005FC RID: 1532
		public const int harvestItemUp = 279;

		// Token: 0x040005FD RID: 1533
		public const int harvestItemRight = 280;

		// Token: 0x040005FE RID: 1534
		public const int harvestItemDown = 281;

		// Token: 0x040005FF RID: 1535
		public const int harvestItemLeft = 282;

		// Token: 0x04000600 RID: 1536
		public const int shearUp = 283;

		// Token: 0x04000601 RID: 1537
		public const int shearRight = 284;

		// Token: 0x04000602 RID: 1538
		public const int shearDown = 285;

		// Token: 0x04000603 RID: 1539
		public const int shearLeft = 286;

		// Token: 0x04000604 RID: 1540
		public const int milkUp = 287;

		// Token: 0x04000605 RID: 1541
		public const int milkRight = 288;

		// Token: 0x04000606 RID: 1542
		public const int milkDown = 289;

		// Token: 0x04000607 RID: 1543
		public const int milkLeft = 290;

		// Token: 0x04000608 RID: 1544
		public const int tired = 291;

		// Token: 0x04000609 RID: 1545
		public const int tired2 = 292;

		// Token: 0x0400060A RID: 1546
		public const int passOutTired = 293;

		// Token: 0x0400060B RID: 1547
		public const int drink = 294;

		// Token: 0x0400060C RID: 1548
		public const int fishingUp = 295;

		// Token: 0x0400060D RID: 1549
		public const int fishingRight = 296;

		// Token: 0x0400060E RID: 1550
		public const int fishingDown = 297;

		// Token: 0x0400060F RID: 1551
		public const int fishingLeft = 298;

		// Token: 0x04000610 RID: 1552
		public const int fishingDoneUp = 299;

		// Token: 0x04000611 RID: 1553
		public const int fishingDoneRight = 300;

		// Token: 0x04000612 RID: 1554
		public const int fishingDoneDown = 301;

		// Token: 0x04000613 RID: 1555
		public const int fishingDoneLeft = 302;

		// Token: 0x04000614 RID: 1556
		public const int pan = 303;

		// Token: 0x04000615 RID: 1557
		public const int showHoldingEdible = 304;

		// Token: 0x04000616 RID: 1558
		private int currentToolIndex;

		// Token: 0x04000617 RID: 1559
		private float oldInterval;

		// Token: 0x04000618 RID: 1560
		public bool pauseForSingleAnimation;

		// Token: 0x04000619 RID: 1561
		public bool animateBackwards;

		// Token: 0x0400061A RID: 1562
		public bool loopThisAnimation;

		// Token: 0x0400061B RID: 1563
		public bool freezeUntilDialogueIsOver;

		// Token: 0x0400061C RID: 1564
		public int currentSingleAnimation = -1;

		// Token: 0x0400061D RID: 1565
		public int currentAnimationFrames;

		// Token: 0x0400061E RID: 1566
		public float currentSingleAnimationInterval = 200f;

		// Token: 0x0400061F RID: 1567
		public float intervalModifier = 1f;

		// Token: 0x04000620 RID: 1568
		public string currentStep = "sandyStep";

		// Token: 0x04000621 RID: 1569
		private Farmer owner;

		// Token: 0x04000622 RID: 1570
		public bool animatingBackwards;

		// Token: 0x04000623 RID: 1571
		public const int cheer = 97;

		// Token: 0x0200042C RID: 1068
		public struct AnimationFrame
		{
			// Token: 0x06003CE6 RID: 15590 RVA: 0x002EDC84 File Offset: 0x002EBE84
			public AnimationFrame(int frame, int milliseconds, int position_offset, bool secondary_arm, bool flip, AnimatedSprite.endOfAnimationBehavior frame_start_behavior, AnimatedSprite.endOfAnimationBehavior frame_end_behavior, int x_offset, bool hideArms = false)
			{
				this.frame = frame;
				this.milliseconds = milliseconds;
				this.positionOffset = position_offset;
				if (hideArms)
				{
					this.armOffset = -1;
				}
				else
				{
					this.armOffset = (secondary_arm ? 12 : 6);
				}
				this.flip = flip;
				this.frameStartBehavior = frame_start_behavior;
				this.frameEndBehavior = frame_end_behavior;
				this.xOffset = x_offset;
			}

			// Token: 0x06003CE7 RID: 15591 RVA: 0x002EDCE2 File Offset: 0x002EBEE2
			public AnimationFrame(int frame, int milliseconds, int position_offset, int armOffset, bool flip, AnimatedSprite.endOfAnimationBehavior frame_start_behavior, AnimatedSprite.endOfAnimationBehavior frame_end_behavior, int x_offset)
			{
				this.frame = frame;
				this.milliseconds = milliseconds;
				this.positionOffset = position_offset;
				this.armOffset = armOffset;
				this.flip = flip;
				this.frameStartBehavior = frame_start_behavior;
				this.frameEndBehavior = frame_end_behavior;
				this.xOffset = x_offset;
			}

			// Token: 0x06003CE8 RID: 15592 RVA: 0x002EDD24 File Offset: 0x002EBF24
			public AnimationFrame(int frame, int milliseconds, int positionOffset, bool secondaryArm, bool flip, AnimatedSprite.endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false, int xOffset = 0)
			{
				this = new FarmerSprite.AnimationFrame(frame, milliseconds, positionOffset, secondaryArm, flip, null, null, xOffset, false);
				if (!behaviorAtEndOfFrame)
				{
					this.frameStartBehavior = frameBehavior;
					return;
				}
				this.frameEndBehavior = frameBehavior;
			}

			// Token: 0x06003CE9 RID: 15593 RVA: 0x002EDD58 File Offset: 0x002EBF58
			public AnimationFrame(int frame, int milliseconds, bool secondaryArm, bool flip, AnimatedSprite.endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false)
			{
				this = new FarmerSprite.AnimationFrame(frame, milliseconds, 0, secondaryArm, flip, frameBehavior, behaviorAtEndOfFrame, 0);
			}

			// Token: 0x06003CEA RID: 15594 RVA: 0x002EDD78 File Offset: 0x002EBF78
			public AnimationFrame(int frame, int milliseconds, bool secondaryArm, bool flip, bool hideArm)
			{
				this = new FarmerSprite.AnimationFrame(frame, milliseconds, 0, secondaryArm, flip, null, null, 0, hideArm);
			}

			// Token: 0x06003CEB RID: 15595 RVA: 0x002EDD96 File Offset: 0x002EBF96
			public AnimationFrame(int frame, int milliseconds)
			{
				this = new FarmerSprite.AnimationFrame(frame, milliseconds, false, false, null, false);
			}

			// Token: 0x06003CEC RID: 15596 RVA: 0x002EDDA4 File Offset: 0x002EBFA4
			public AnimationFrame(int frame, int milliseconds, int armOffset, bool flip = false)
			{
				this = new FarmerSprite.AnimationFrame(frame, milliseconds, 0, armOffset, flip, null, null, 0);
			}

			// Token: 0x06003CED RID: 15597 RVA: 0x002EDDC0 File Offset: 0x002EBFC0
			public FarmerSprite.AnimationFrame AddFrameAction(AnimatedSprite.endOfAnimationBehavior callback)
			{
				this.frameStartBehavior = (AnimatedSprite.endOfAnimationBehavior)Delegate.Combine(this.frameStartBehavior, callback);
				return this;
			}

			// Token: 0x06003CEE RID: 15598 RVA: 0x002EDDDF File Offset: 0x002EBFDF
			public FarmerSprite.AnimationFrame AddFrameEndAction(AnimatedSprite.endOfAnimationBehavior callback)
			{
				this.frameEndBehavior = (AnimatedSprite.endOfAnimationBehavior)Delegate.Combine(this.frameEndBehavior, callback);
				return this;
			}

			// Token: 0x04002773 RID: 10099
			public int frame;

			// Token: 0x04002774 RID: 10100
			public int milliseconds;

			// Token: 0x04002775 RID: 10101
			public int positionOffset;

			// Token: 0x04002776 RID: 10102
			public int xOffset;

			// Token: 0x04002777 RID: 10103
			public int armOffset;

			// Token: 0x04002778 RID: 10104
			public bool flip;

			// Token: 0x04002779 RID: 10105
			public AnimatedSprite.endOfAnimationBehavior frameStartBehavior;

			// Token: 0x0400277A RID: 10106
			public AnimatedSprite.endOfAnimationBehavior frameEndBehavior;
		}
	}
}

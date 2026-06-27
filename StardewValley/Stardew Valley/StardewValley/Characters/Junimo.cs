using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace StardewValley.Characters
{
	// Token: 0x02000379 RID: 889
	public class Junimo : NPC
	{
		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06003656 RID: 13910 RVA: 0x002AEBEE File Offset: 0x002ACDEE
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003657 RID: 13911 RVA: 0x002AEBF4 File Offset: 0x002ACDF4
		public Junimo()
		{
			this.forceUpdateTimer = 9999;
		}

		// Token: 0x06003658 RID: 13912 RVA: 0x002AECF0 File Offset: 0x002ACEF0
		public Junimo(Vector2 position, int whichArea, bool temporary = false) : base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo", null)
		{
			this.whichArea.Value = whichArea;
			try
			{
				this.friendly.Value = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).areasComplete[whichArea];
			}
			catch (Exception)
			{
				this.friendly.Value = true;
			}
			if (whichArea == 6)
			{
				this.friendly.Value = false;
			}
			this.temporaryJunimo.Value = temporary;
			this.nextPosition.Value = this.GetBoundingBox();
			base.Breather = false;
			base.speed = 3;
			this.forceUpdateTimer = 9999;
			this.collidesWithOtherCharacters.Value = true;
			this.farmerPassesThrough = true;
			base.Scale = 0.75f;
			if (this.temporaryJunimo.Value)
			{
				if (Game1.random.NextDouble() < 0.01)
				{
					switch (Game1.random.Next(8))
					{
					case 0:
						this.color.Value = Color.Red;
						break;
					case 1:
						this.color.Value = Color.Goldenrod;
						break;
					case 2:
						this.color.Value = Color.Yellow;
						break;
					case 3:
						this.color.Value = Color.Lime;
						break;
					case 4:
						this.color.Value = new Color(0, 255, 180);
						break;
					case 5:
						this.color.Value = new Color(0, 100, 255);
						break;
					case 6:
						this.color.Value = Color.MediumPurple;
						break;
					case 7:
						this.color.Value = Color.Salmon;
						break;
					}
					if (Game1.random.NextDouble() < 0.01)
					{
						this.color.Value = Color.White;
						return;
					}
				}
				else
				{
					switch (Game1.random.Next(8))
					{
					case 0:
						this.color.Value = Color.LimeGreen;
						return;
					case 1:
						this.color.Value = Color.Orange;
						return;
					case 2:
						this.color.Value = Color.LightGreen;
						return;
					case 3:
						this.color.Value = Color.Tan;
						return;
					case 4:
						this.color.Value = Color.GreenYellow;
						return;
					case 5:
						this.color.Value = Color.LawnGreen;
						return;
					case 6:
						this.color.Value = Color.PaleGreen;
						return;
					case 7:
						this.color.Value = Color.Turquoise;
						return;
					default:
						return;
					}
				}
			}
			else
			{
				switch (whichArea)
				{
				case -1:
				case 0:
					this.color.Value = Color.LimeGreen;
					return;
				case 1:
					this.color.Value = Color.Orange;
					return;
				case 2:
					this.color.Value = Color.Turquoise;
					return;
				case 3:
					this.color.Value = Color.Tan;
					return;
				case 4:
					this.color.Value = Color.Gold;
					return;
				case 5:
					this.color.Value = Color.BlanchedAlmond;
					return;
				case 6:
					this.color.Value = new Color(160, 20, 220);
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06003659 RID: 13913 RVA: 0x002AF13C File Offset: 0x002AD33C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.alpha, "alpha").AddField(this.alphaChange, "alphaChange").AddField(this.whichArea, "whichArea").AddField(this.friendly, "friendly").AddField(this.holdingStar, "holdingStar").AddField(this.holdingBundle, "holdingBundle").AddField(this.temporaryJunimo, "temporaryJunimo").AddField(this.stayPut, "stayPut").AddField(this.motion, "motion").AddField(this.nextPosition, "nextPosition").AddField(this.color, "color").AddField(this.bundleColor, "bundleColor").AddField(this.sayingGoodbye, "sayingGoodbye").AddField(this.setReturnToJunimoHutToFetchStarControllerEvent, "setReturnToJunimoHutToFetchStarControllerEvent").AddField(this.setBringBundleBackToHutControllerEvent, "setBringBundleBackToHutControllerEvent").AddField(this.setJunimoReachedHutToFetchStarControllerEvent, "setJunimoReachedHutToFetchStarControllerEvent").AddField(this.starDoneSpinningEvent, "starDoneSpinningEvent").AddField(this.returnToJunimoHutToFetchFinalStarEvent, "returnToJunimoHutToFetchFinalStarEvent");
			this.setReturnToJunimoHutToFetchStarControllerEvent.onEvent += this.setReturnToJunimoHutToFetchStarController;
			this.setBringBundleBackToHutControllerEvent.onEvent += this.setBringBundleBackToHutController;
			this.setJunimoReachedHutToFetchStarControllerEvent.onEvent += this.setJunimoReachedHutToFetchStarController;
			this.starDoneSpinningEvent.onEvent += this.performStartDoneSpinning;
			this.returnToJunimoHutToFetchFinalStarEvent.onEvent += this.returnToJunimoHutToFetchFinalStar;
			this.position.Field.AxisAlignedMovement = false;
		}

		// Token: 0x0600365A RID: 13914 RVA: 0x002AF2FA File Offset: 0x002AD4FA
		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		// Token: 0x0600365B RID: 13915 RVA: 0x002AF2FD File Offset: 0x002AD4FD
		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		// Token: 0x0600365C RID: 13916 RVA: 0x002AF300 File Offset: 0x002AD500
		public override bool canTalk()
		{
			return false;
		}

		// Token: 0x0600365D RID: 13917 RVA: 0x002AF303 File Offset: 0x002AD503
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
		}

		// Token: 0x0600365E RID: 13918 RVA: 0x002AF305 File Offset: 0x002AD505
		public void fadeAway()
		{
			this.collidesWithOtherCharacters.Value = false;
			this.alphaChange.Value = (this.stayPut.Value ? -0.005f : -0.015f);
		}

		// Token: 0x0600365F RID: 13919 RVA: 0x002AF337 File Offset: 0x002AD537
		public void setAlpha(float a)
		{
			this.alpha.Value = a;
		}

		// Token: 0x06003660 RID: 13920 RVA: 0x002AF345 File Offset: 0x002AD545
		public void fadeBack()
		{
			this.alpha.Value = 0f;
			this.alphaChange.Value = 0.02f;
			base.IsInvisible = false;
		}

		// Token: 0x06003661 RID: 13921 RVA: 0x002AF36E File Offset: 0x002AD56E
		public void setMoving(int xSpeed, int ySpeed)
		{
			this.motion.X = (float)xSpeed;
			this.motion.Y = (float)ySpeed;
		}

		// Token: 0x06003662 RID: 13922 RVA: 0x002AF38A File Offset: 0x002AD58A
		public void setMoving(Vector2 motion)
		{
			this.motion.Value = motion;
		}

		// Token: 0x06003663 RID: 13923 RVA: 0x002AF398 File Offset: 0x002AD598
		public override void Halt()
		{
			base.Halt();
			this.motion.Value = Vector2.Zero;
		}

		// Token: 0x06003664 RID: 13924 RVA: 0x002AF3B0 File Offset: 0x002AD5B0
		public void returnToJunimoHut(GameLocation location)
		{
			base.currentLocation = location;
			this.jump();
			this.collidesWithOtherCharacters.Value = false;
			this.controller = new PathFindController(this, location, new Point(25, 10), 0, new PathFindController.endBehavior(this.junimoReachedHut));
			location.playSound("junimoMeep1", null, null, SoundContext.Default);
		}

		// Token: 0x06003665 RID: 13925 RVA: 0x002AF417 File Offset: 0x002AD617
		public void stayStill()
		{
			this.stayPut.Value = true;
			this.motion.Value = Vector2.Zero;
		}

		// Token: 0x06003666 RID: 13926 RVA: 0x002AF435 File Offset: 0x002AD635
		public void allowToMoveAgain()
		{
			this.stayPut.Value = false;
		}

		// Token: 0x06003667 RID: 13927 RVA: 0x002AF443 File Offset: 0x002AD643
		private void returnToJunimoHutToFetchFinalStar()
		{
			if (base.currentLocation == Game1.currentLocation)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.finalCutscene), 0.005f);
				Game1.freezeControls = true;
				Game1.flashAlpha = 1f;
			}
		}

		// Token: 0x06003668 RID: 13928 RVA: 0x002AF478 File Offset: 0x002AD678
		public void returnToJunimoHutToFetchStar(GameLocation location)
		{
			base.currentLocation = location;
			this.friendly.Value = true;
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
			if (communityCenter.areAllAreasComplete())
			{
				this.returnToJunimoHutToFetchFinalStarEvent.Fire();
				this.collidesWithOtherCharacters.Value = false;
				this.farmerPassesThrough = false;
				this.stayStill();
				this.faceDirection(0);
				Game1.player.mailReceived.Add("ccIsComplete");
				if (Game1.currentLocation.Equals(communityCenter))
				{
					communityCenter.addStarToPlaque();
					return;
				}
			}
			else
			{
				DelayedAction.textAboveHeadAfterDelay(Game1.random.NextBool() ? Game1.content.LoadString("Strings\\Characters:JunimoTextAboveHead1") : Game1.content.LoadString("Strings\\Characters:JunimoTextAboveHead2"), this, Game1.random.Next(3000, 6000));
				this.setReturnToJunimoHutToFetchStarControllerEvent.Fire();
				location.playSound("junimoMeep1", null, null, SoundContext.Default);
				this.collidesWithOtherCharacters.Value = false;
				this.farmerPassesThrough = false;
				this.holdingBundle.Value = true;
				base.speed = 3;
			}
		}

		// Token: 0x06003669 RID: 13929 RVA: 0x002AF598 File Offset: 0x002AD798
		private void setReturnToJunimoHutToFetchStarController()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new Point(25, 10), 0, new PathFindController.endBehavior(this.junimoReachedHutToFetchStar));
		}

		// Token: 0x0600366A RID: 13930 RVA: 0x002AF5CC File Offset: 0x002AD7CC
		private void finalCutscene()
		{
			this.collidesWithOtherCharacters.Value = false;
			this.farmerPassesThrough = false;
			Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).prepareForJunimoDance();
			Game1.player.Position = new Vector2(29f, 11f) * 64f;
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.faceDirection(3);
			Point playerPixel = Game1.player.StandingPixel;
			Game1.UpdateViewPort(true, playerPixel);
			Game1.viewport.X = playerPixel.X - Game1.viewport.Width / 2;
			Game1.viewport.Y = playerPixel.Y - Game1.viewport.Height / 2;
			Game1.viewportTarget = Vector2.Zero;
			Game1.viewportCenter = playerPixel;
			Game1.moveViewportTo(new Vector2(32.5f, 6f) * 64f, 2f, 999999, null, null);
			Game1.globalFadeToClear(new Game1.afterFadeFunction(this.goodbyeDance), 0.005f);
			Game1.pauseTime = 1000f;
			Game1.freezeControls = true;
		}

		// Token: 0x0600366B RID: 13931 RVA: 0x002AF6E0 File Offset: 0x002AD8E0
		public void bringBundleBackToHut(Color bundleColor, GameLocation location)
		{
			base.currentLocation = location;
			if (!this.holdingBundle.Value)
			{
				base.Position = Utility.getRandomAdjacentOpenTile(Game1.player.Tile, location) * 64f;
				int iter = 0;
				while (location.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, this) && iter < 5)
				{
					base.Position = Utility.getRandomAdjacentOpenTile(Game1.player.Tile, location) * 64f;
					iter++;
				}
				if (iter >= 5)
				{
					return;
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					DelayedAction.textAboveHeadAfterDelay(Game1.random.NextBool() ? Game1.content.LoadString("Strings\\Characters:JunimoThankYou1") : Game1.content.LoadString("Strings\\Characters:JunimoThankYou2"), this, Game1.random.Next(3000, 6000));
				}
				this.fadeBack();
				this.bundleColor.Value = bundleColor;
				this.setBringBundleBackToHutControllerEvent.Fire();
				this.collidesWithOtherCharacters.Value = false;
				this.farmerPassesThrough = false;
				this.holdingBundle.Value = true;
				base.speed = 1;
			}
		}

		// Token: 0x0600366C RID: 13932 RVA: 0x002AF807 File Offset: 0x002ADA07
		private void setBringBundleBackToHutController()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new Point(25, 10), 0, new PathFindController.endBehavior(this.junimoReachedHutToReturnBundle));
		}

		// Token: 0x0600366D RID: 13933 RVA: 0x002AF83C File Offset: 0x002ADA3C
		private void junimoReachedHutToReturnBundle(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.holdingBundle.Value = false;
			this.collidesWithOtherCharacters.Value = true;
			this.farmerPassesThrough = true;
			l.playSound("Ship", null, null, SoundContext.Default);
		}

		// Token: 0x0600366E RID: 13934 RVA: 0x002AF890 File Offset: 0x002ADA90
		private void junimoReachedHutToFetchStar(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.holdingStar.Value = true;
			this.holdingBundle.Value = false;
			base.speed = 1;
			this.collidesWithOtherCharacters.Value = false;
			this.farmerPassesThrough = false;
			this.setJunimoReachedHutToFetchStarControllerEvent.Fire();
			l.playSound("dwop", null, null, SoundContext.Default);
			this.farmerPassesThrough = false;
		}

		// Token: 0x0600366F RID: 13935 RVA: 0x002AF906 File Offset: 0x002ADB06
		private void setJunimoReachedHutToFetchStarController()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new Point(32, 9), 2, new PathFindController.endBehavior(this.placeStar));
		}

		// Token: 0x06003670 RID: 13936 RVA: 0x002AF938 File Offset: 0x002ADB38
		private void placeStar(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.collidesWithOtherCharacters.Value = false;
			this.farmerPassesThrough = true;
			this.holdingStar.Value = false;
			l.playSound("tinyWhip", null, null, SoundContext.Default);
			this.friendly.Value = true;
			base.speed = 3;
			Game1.multiplayer.broadcastSprites(l, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 109, 16, 19), 40f, 8, 10, base.Position + new Vector2(0f, -64f), false, false, 1f, 0f, Color.White, 4f * this.scale.Value, 0f, 0f, 0f, false)
				{
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.starDoneSpinning),
					motion = new Vector2(0.22f, -2f),
					acceleration = new Vector2(0f, 0.01f),
					id = 777
				}
			});
		}

		// Token: 0x06003671 RID: 13937 RVA: 0x002AFA6C File Offset: 0x002ADC6C
		public void sayGoodbye()
		{
			this.sayingGoodbye.Value = true;
			this.farmerPassesThrough = true;
		}

		// Token: 0x06003672 RID: 13938 RVA: 0x002AFA81 File Offset: 0x002ADC81
		private void goodbyeDance()
		{
			Game1.player.faceDirection(3);
			Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).junimoGoodbyeDance();
		}

		// Token: 0x06003673 RID: 13939 RVA: 0x002AFA9E File Offset: 0x002ADC9E
		private void starDoneSpinning(int extraInfo)
		{
			this.starDoneSpinningEvent.Fire();
			(base.currentLocation as CommunityCenter).addStarToPlaque();
		}

		// Token: 0x06003674 RID: 13940 RVA: 0x002AFABC File Offset: 0x002ADCBC
		private void performStartDoneSpinning()
		{
			if (Game1.currentLocation is CommunityCenter)
			{
				Game1.playSound("yoba", null);
				Game1.flashAlpha = 1f;
				Game1.playSound("yoba", null);
			}
		}

		// Token: 0x06003675 RID: 13941 RVA: 0x002AFB08 File Offset: 0x002ADD08
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (this.textAboveHeadTimer > 0 && this.textAboveHead != null)
			{
				Point standingPixel = base.StandingPixel;
				Vector2 local = Game1.GlobalToLocal(new Vector2((float)standingPixel.X, (float)standingPixel.Y - 128f + (float)this.yJumpOffset));
				if (this.textAboveHeadStyle == 0)
				{
					local += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				}
				SpriteText.drawStringWithScrollCenteredAt(b, this.textAboveHead, (int)local.X, (int)local.Y, "", this.textAboveHeadAlpha, this.textAboveHeadColor, 1, (float)(base.TilePoint.Y * 64) / 10000f + 0.001f + (float)base.TilePoint.X / 10000f, !this.sayingGoodbye.Value);
			}
		}

		// Token: 0x06003676 RID: 13942 RVA: 0x002AFBF0 File Offset: 0x002ADDF0
		public void junimoReachedHut(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.fadeAway();
			this.controller = null;
			this.motion.X = 0f;
			this.motion.Y = -1f;
		}

		// Token: 0x06003677 RID: 13943 RVA: 0x002AFC28 File Offset: 0x002ADE28
		protected override void updateSlaveAnimation(GameTime time)
		{
			if (this.sayingGoodbye.Value || this.temporaryJunimo.Value)
			{
				return;
			}
			if (this.holdingStar.Value || this.holdingBundle.Value)
			{
				this.Sprite.Animate(time, 44, 4, 200f);
				return;
			}
			if (this.position.IsInterpolating())
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.Animate(time, 32, 8, 50f);
					return;
				case 1:
					this.flip = false;
					this.Sprite.Animate(time, 16, 8, 50f);
					return;
				case 3:
					this.flip = true;
					this.Sprite.Animate(time, 16, 8, 50f);
					return;
				}
				this.Sprite.Animate(time, 0, 8, 50f);
				return;
			}
			this.Sprite.Animate(time, 8, 4, 100f);
		}

		// Token: 0x06003678 RID: 13944 RVA: 0x002AFD28 File Offset: 0x002ADF28
		public override void update(GameTime time, GameLocation location)
		{
			base.currentLocation = location;
			this.setReturnToJunimoHutToFetchStarControllerEvent.Poll();
			this.setBringBundleBackToHutControllerEvent.Poll();
			this.setJunimoReachedHutToFetchStarControllerEvent.Poll();
			this.starDoneSpinningEvent.Poll();
			this.returnToJunimoHutToFetchFinalStarEvent.Poll();
			base.update(time, location);
			this.forceUpdateTimer = 99999;
			if (this.sayingGoodbye.Value)
			{
				this.flip = false;
				if (this.whichArea.Value % 2 == 0)
				{
					this.Sprite.Animate(time, 16, 8, 50f);
				}
				else
				{
					this.Sprite.Animate(time, 28, 4, 80f);
				}
				if (!base.IsInvisible && Game1.random.NextDouble() < 0.009999999776482582 && this.yJumpOffset == 0)
				{
					this.jump();
					if (Game1.random.NextDouble() < 0.15 && Game1.player.Tile.X == 29f && Game1.player.Tile.Y == 11f)
					{
						base.showTextAboveHead(Game1.random.NextBool() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Junimo.cs.6625") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Junimo.cs.6626"), null, 2, 3000, 0);
					}
				}
				this.alpha.Value += this.alphaChange.Value;
				if (this.alpha.Value > 1f)
				{
					this.alpha.Value = 1f;
					this.alphaChange.Value = 0f;
				}
				if (this.alpha.Value < 0f)
				{
					this.alpha.Value = 0f;
					base.IsInvisible = true;
					base.HideShadow = true;
				}
				return;
			}
			if (this.temporaryJunimo.Value)
			{
				this.Sprite.Animate(time, 12, 4, 100f);
				if (Game1.random.NextDouble() < 0.001)
				{
					this.jumpWithoutSound(8f);
					location.localSound("junimoMeep1", null, null, SoundContext.Default);
				}
				return;
			}
			if (this.EventActor)
			{
				return;
			}
			this.alpha.Value += this.alphaChange.Value;
			if (this.alpha.Value > 1f)
			{
				this.alpha.Value = 1f;
				base.HideShadow = false;
			}
			else if (this.alpha.Value < 0f)
			{
				this.alpha.Value = 0f;
				base.IsInvisible = true;
				base.HideShadow = true;
			}
			Junimo.soundTimer--;
			this.farmerCloseCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.sayingGoodbye.Value || this.temporaryJunimo.Value || !Game1.IsMasterGame)
			{
				return;
			}
			if (!base.IsInvisible && this.farmerCloseCheckTimer <= 0 && this.controller == null && this.alpha.Value >= 1f && !this.stayPut.Value && Game1.IsMasterGame)
			{
				this.farmerCloseCheckTimer = 100;
				if (this.holdingStar.Value)
				{
					this.setJunimoReachedHutToFetchStarController();
				}
				else
				{
					Farmer f = Utility.isThereAFarmerWithinDistance(base.Tile, 5, base.currentLocation);
					if (f != null)
					{
						if (this.friendly.Value && Vector2.Distance(base.Position, f.Position) > (float)(base.speed * 4))
						{
							if (this.motion.Equals(Vector2.Zero) && Junimo.soundTimer <= 0)
							{
								this.jump();
								location.localSound("junimoMeep1", null, null, SoundContext.Default);
								Junimo.soundTimer = 400;
							}
							if (Game1.random.NextDouble() < 0.007)
							{
								this.jumpWithoutSound((float)Game1.random.Next(6, 9));
							}
							this.setMoving(Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), (float)base.speed, f));
						}
						else if (!this.friendly.Value)
						{
							this.fadeAway();
							Vector2 v = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), f);
							v.Normalize();
							v.Y *= -1f;
							this.setMoving(v * (float)base.speed);
						}
						else if (this.alpha.Value >= 1f)
						{
							this.motion.Value = Vector2.Zero;
						}
					}
					else if (this.alpha.Value >= 1f)
					{
						this.motion.Value = Vector2.Zero;
					}
				}
			}
			if (!base.IsInvisible && this.controller == null)
			{
				this.nextPosition.Value = this.GetBoundingBox();
				this.nextPosition.X += (int)this.motion.X;
				bool sparkle = false;
				if (!location.isCollidingPosition(this.nextPosition.Value, Game1.viewport, this))
				{
					this.position.X += (float)((int)this.motion.X);
					sparkle = true;
				}
				this.nextPosition.X -= (int)this.motion.X;
				this.nextPosition.Y += (int)this.motion.Y;
				if (!location.isCollidingPosition(this.nextPosition.Value, Game1.viewport, this))
				{
					this.position.Y += (float)((int)this.motion.Y);
					sparkle = true;
				}
				if (!this.motion.Equals(Vector2.Zero) && sparkle && Game1.random.NextDouble() < 0.005)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.Choose(10, 11), base.Position, this.color.Value, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						motion = this.motion.Value / 4f,
						alphaFade = 0.01f,
						layerDepth = 0.8f,
						scale = 0.75f,
						alpha = 0.75f
					});
				}
			}
			if (this.controller == null && this.motion.Equals(Vector2.Zero))
			{
				this.Sprite.Animate(time, 8, 4, 100f);
				return;
			}
			if (this.holdingStar.Value || this.holdingBundle.Value)
			{
				this.Sprite.Animate(time, 44, 4, 200f);
				return;
			}
			if (this.moveRight || (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X > 0f))
			{
				this.flip = false;
				this.Sprite.Animate(time, 16, 8, 50f);
				return;
			}
			if (this.moveLeft || (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X < 0f))
			{
				this.Sprite.Animate(time, 16, 8, 50f);
				this.flip = true;
				return;
			}
			if (this.moveUp || (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y < 0f))
			{
				this.Sprite.Animate(time, 32, 8, 50f);
				return;
			}
			this.Sprite.Animate(time, 0, 8, 50f);
		}

		// Token: 0x06003679 RID: 13945 RVA: 0x002B0570 File Offset: 0x002AE770
		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			if (!base.IsInvisible)
			{
				this.Sprite.UpdateSourceRect();
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow((double)(this.Sprite.SpriteHeight / 16), 2.0) + (float)this.yJumpOffset - 8f) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(this.Sprite.SourceRect), this.color.Value * this.alpha.Value, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
				if (this.holdingStar.Value)
				{
					b.Draw(this.Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(8f, -64f * this.scale.Value + 4f + (float)this.yJumpOffset)), new Rectangle?(new Rectangle(0, 109, 16, 19)), Color.White * this.alpha.Value, 0f, Vector2.Zero, 4f * this.scale.Value, SpriteEffects.None, base.Position.Y / 10000f + 0.0001f);
					return;
				}
				if (this.holdingBundle.Value)
				{
					b.Draw(this.Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(8f, -64f * this.scale.Value + 20f + (float)this.yJumpOffset)), new Rectangle?(new Rectangle(0, 96, 16, 13)), this.bundleColor.Value * this.alpha.Value, 0f, Vector2.Zero, 4f * this.scale.Value, SpriteEffects.None, base.Position.Y / 10000f + 0.0001f);
				}
			}
		}

		// Token: 0x0600367A RID: 13946 RVA: 0x002B087C File Offset: 0x002AEA7C
		public override void DrawShadow(SpriteBatch b)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2((float)(this.Sprite.SpriteWidth * 4) / 2f, 44f)), new Rectangle?(Game1.shadowTexture.Bounds), this.color.Value * this.alpha.Value, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (4f + (float)this.yJumpOffset / 40f) * this.scale.Value, SpriteEffects.None, Math.Max(0f, (float)base.StandingPixel.Y / 10000f) - 1E-06f);
		}

		// Token: 0x0400238E RID: 9102
		private readonly NetFloat alpha = new NetFloat(1f);

		// Token: 0x0400238F RID: 9103
		private readonly NetFloat alphaChange = new NetFloat();

		// Token: 0x04002390 RID: 9104
		public readonly NetInt whichArea = new NetInt();

		// Token: 0x04002391 RID: 9105
		public readonly NetBool friendly = new NetBool();

		// Token: 0x04002392 RID: 9106
		public readonly NetBool holdingStar = new NetBool();

		// Token: 0x04002393 RID: 9107
		public readonly NetBool holdingBundle = new NetBool();

		// Token: 0x04002394 RID: 9108
		public readonly NetBool temporaryJunimo = new NetBool();

		// Token: 0x04002395 RID: 9109
		public readonly NetBool stayPut = new NetBool();

		// Token: 0x04002396 RID: 9110
		private readonly NetVector2 motion = new NetVector2(Vector2.Zero);

		// Token: 0x04002397 RID: 9111
		private new readonly NetRectangle nextPosition = new NetRectangle();

		// Token: 0x04002398 RID: 9112
		private readonly NetColor color = new NetColor();

		// Token: 0x04002399 RID: 9113
		private readonly NetColor bundleColor = new NetColor();

		// Token: 0x0400239A RID: 9114
		private readonly NetBool sayingGoodbye = new NetBool();

		// Token: 0x0400239B RID: 9115
		private readonly NetEvent0 setReturnToJunimoHutToFetchStarControllerEvent = new NetEvent0(false);

		// Token: 0x0400239C RID: 9116
		private readonly NetEvent0 setBringBundleBackToHutControllerEvent = new NetEvent0(false);

		// Token: 0x0400239D RID: 9117
		private readonly NetEvent0 setJunimoReachedHutToFetchStarControllerEvent = new NetEvent0(false);

		// Token: 0x0400239E RID: 9118
		private readonly NetEvent0 starDoneSpinningEvent = new NetEvent0(false);

		// Token: 0x0400239F RID: 9119
		private readonly NetEvent0 returnToJunimoHutToFetchFinalStarEvent = new NetEvent0(false);

		// Token: 0x040023A0 RID: 9120
		private int farmerCloseCheckTimer = 100;

		// Token: 0x040023A1 RID: 9121
		private static int soundTimer;
	}
}

using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002EC RID: 748
	public class Railroad : GameLocation
	{
		// Token: 0x06003216 RID: 12822 RVA: 0x00280038 File Offset: 0x0027E238
		static Railroad()
		{
			double trainChance = 0.09;
			if (trainChance < 0.0001)
			{
				trainChance = 0.0001;
			}
			else if (trainChance > 0.2499)
			{
				trainChance = 0.2499;
			}
			Railroad.DailyTrainChance = (1.0 - Math.Sqrt(1.0 - 4.0 * trainChance)) * 0.5;
		}

		// Token: 0x06003217 RID: 12823 RVA: 0x002800B0 File Offset: 0x0027E2B0
		public Railroad()
		{
		}

		// Token: 0x06003218 RID: 12824 RVA: 0x002800EE File Offset: 0x0027E2EE
		public Railroad(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06003219 RID: 12825 RVA: 0x00280130 File Offset: 0x0027E330
		public override void ResetForEvent(Event ev)
		{
			base.ResetForEvent(ev);
			if (((ev != null) ? ev.id : null) == "528052")
			{
				ev.eventPositionTileOffset.X = ev.eventPositionTileOffset.X - 8f;
				ev.eventPositionTileOffset.Y = ev.eventPositionTileOffset.Y - 2f;
			}
		}

		// Token: 0x0600321A RID: 12826 RVA: 0x00280184 File Offset: 0x0027E384
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.train, "train").AddField(this.hasTrainPassed, "hasTrainPassed").AddField(this.witchStatueGone, "witchStatueGone").AddField(this.trainTimer, "trainTimer");
			this.witchStatueGone.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				if (!oldValue && newValue && base.Map != null)
				{
					DelayedAction.removeTileAfterDelay(54, 35, 2000, this, "Buildings");
					DelayedAction.removeTileAfterDelay(54, 34, 2000, this, "Front");
				}
			};
		}

		// Token: 0x0600321B RID: 12827 RVA: 0x002801F8 File Offset: 0x0027E3F8
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (this.witchStatueGone.Value || Game1.MasterPlayer.mailReceived.Contains("witchStatueGone"))
			{
				base.removeTile(54, 35, "Buildings");
				base.removeTile(54, 34, "Front");
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				base.removeTile(24, 34, "Buildings");
				base.removeTile(25, 34, "Buildings");
				base.removeTile(24, 35, "Buildings");
				base.removeTile(25, 35, "Buildings");
			}
		}

		// Token: 0x0600321C RID: 12828 RVA: 0x0028029F File Offset: 0x0027E49F
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!base.IsWinterHere())
			{
				AmbientLocationSounds.addSound(new Vector2(15f, 56f), 0);
			}
		}

		// Token: 0x0600321D RID: 12829 RVA: 0x002802C4 File Offset: 0x0027E4C4
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			ICue cue = Railroad.trainLoop;
			if (cue != null)
			{
				cue.Stop(AudioStopOptions.Immediate);
			}
			Railroad.trainLoop = null;
		}

		// Token: 0x0600321E RID: 12830 RVA: 0x002802E4 File Offset: 0x0027E4E4
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (who.secretNotesSeen.Contains(16) && xLocation == 12 && yLocation == 38 && who.mailReceived.Add("SecretNote16_done"))
			{
				Game1.createObjectDebris("(O)166", xLocation, yLocation, who.UniqueMultiplayerID, this);
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		// Token: 0x0600321F RID: 12831 RVA: 0x00280348 File Offset: 0x0027E548
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet") == 287)
			{
				if (Game1.player.hasDarkTalisman)
				{
					Game1.player.freezePause = 7000;
					base.playSound("fireball", null, null, SoundContext.Default);
					DelayedAction.playSoundAfterDelay("secret1", 2000, null, null, -1, false);
					DelayedAction.removeTemporarySpriteAfterDelay(this, 9999, 2000);
					this.witchStatueGone.Value = true;
					who.mailReceived.Add("witchStatueGone");
					for (int i = 0; i < 22; i++)
					{
						DelayedAction.playSoundAfterDelay("batFlap", 2220 + 240 * i, null, null, -1, false);
					}
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(576, 271, 28, 31), 60f, 3, 999, new Vector2(54f, 34f) * 64f + new Vector2(-2f, 1f) * 4f, false, false, 0.2176f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 8000f,
							xPeriodicRange = 384f,
							motion = new Vector2(-2f, 0f),
							acceleration = new Vector2(0f, -0.015f),
							pingPong = true,
							delayBeforeAnimationStart = 2000
						}
					});
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 499, 10, 11), 50f, 7, 999, new Vector2(54f, 34f) * 64f + new Vector2(7f, 11f) * 4f, false, false, 0.2177f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 8000f,
							xPeriodicRange = 384f,
							motion = new Vector2(-2f, 0f),
							acceleration = new Vector2(0f, -0.015f),
							delayBeforeAnimationStart = 2000
						}
					});
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 499, 10, 11), 35.715f, 7, 8, new Vector2(54f, 34f) * 64f + new Vector2(3f, 10f) * 4f, false, false, 0.2305f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							id = 9999
						}
					});
				}
				else
				{
					Game1.drawObjectDialogue("???");
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06003220 RID: 12832 RVA: 0x002806B0 File Offset: 0x0027E8B0
		internal void ResetTrainForNewDay()
		{
			this.hasTrainPassed.Value = false;
			this.trainTime = -1;
			Random random = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2UL, 0.0, 0.0, 0.0);
			Random rngTomorrow = Utility.CreateRandom(Game1.stats.DaysPlayed + 1U, Game1.uniqueIDForThisGame / 2UL, 0.0, 0.0, 0.0);
			bool flag = random.NextDouble() < Railroad.DailyTrainChance;
			bool trainTomorrow = rngTomorrow.NextDouble() < Railroad.DailyTrainChance;
			if (flag && !trainTomorrow && Game1.isLocationAccessible("Railroad"))
			{
				this.trainTime = 900;
				this.trainTime -= this.trainTime % 10;
			}
		}

		// Token: 0x06003221 RID: 12833 RVA: 0x0028078B File Offset: 0x0027E98B
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.ResetTrainForNewDay();
		}

		// Token: 0x06003222 RID: 12834 RVA: 0x0028079C File Offset: 0x0027E99C
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return (!Game1.eventUp && this.train.Value != null && this.train.Value.getBoundingBox().Intersects(position)) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		// Token: 0x06003223 RID: 12835 RVA: 0x002807E9 File Offset: 0x0027E9E9
		public void setTrainComing(int delay)
		{
			this.trainTimer.Value = delay;
			if (Game1.IsMasterGame)
			{
				this.PlayTrainApproach();
				Game1.multiplayer.sendServerToClientsMessage("trainApproach");
			}
		}

		// Token: 0x06003224 RID: 12836 RVA: 0x00280814 File Offset: 0x0027EA14
		public void PlayTrainApproach()
		{
			GameLocation currentLocation = Game1.currentLocation;
			bool? flag = (currentLocation != null) ? new bool?(currentLocation.IsOutdoors) : null;
			if (flag != null && flag.GetValueOrDefault() && !Game1.isFestival() && Game1.currentLocation.InValleyContext())
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:Railroad_TrainComing"));
				ICue whistle;
				Game1.playSound("distantTrain", out whistle);
				whistle.SetVariable("Volume", 100f);
			}
		}

		// Token: 0x06003225 RID: 12837 RVA: 0x00280898 File Offset: 0x0027EA98
		public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Game1.player.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX) && !Game1.player.hasOrWillReceiveMail(GameLocation.CAROLINES_NECKLACE_MAIL))
			{
				Game1.player.mailForTomorrow.Add(GameLocation.CAROLINES_NECKLACE_MAIL + "%&NL&%");
				Item result = ItemRegistry.Create(GameLocation.CAROLINES_NECKLACE_ITEM_QID, 1, 0, false);
				Game1.player.addQuest("128");
				Game1.player.addQuest("129");
				return result;
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		// Token: 0x06003226 RID: 12838 RVA: 0x00280928 File Offset: 0x0027EB28
		public override bool isTileFishable(int tileX, int tileY)
		{
			return !base.IsWinterHere() && base.isTileFishable(tileX, tileY);
		}

		// Token: 0x06003227 RID: 12839 RVA: 0x0028093C File Offset: 0x0027EB3C
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			if (this.train.Value != null && this.train.Value.Update(time, this) && Game1.IsMasterGame)
			{
				this.train.Value = null;
			}
			if (Game1.IsMasterGame)
			{
				if (Game1.timeOfDay == this.trainTime - this.trainTime % 10 && this.trainTimer.Value <= 0 && !Game1.isFestival() && this.train.Value == null)
				{
					this.setTrainComing(15000);
				}
				if (this.trainTimer.Value > 0)
				{
					this.trainTimer.Value -= time.ElapsedGameTime.Milliseconds;
					if (this.trainTimer.Value <= 0)
					{
						this.train.Value = new Train();
						base.playSound("trainWhistle", null, null, SoundContext.Default);
					}
				}
			}
			if (this.trainTimer.Value > 0 && this.trainTimer.Value < 3500)
			{
				this.StartTrainLoopIfNeeded();
			}
			if (this.train.Value != null)
			{
				this.StartTrainLoopIfNeeded();
				ICue cue = Railroad.trainLoop;
				if (cue != null && cue.GetVariable("Volume") < (float)100)
				{
					Railroad.trainLoop.SetVariable("Volume", Railroad.trainLoop.GetVariable("Volume") + 0.5f);
					return;
				}
			}
			else if (Railroad.trainLoop != null && this.trainTimer.Value <= 0)
			{
				Railroad.trainLoop.SetVariable("Volume", Railroad.trainLoop.GetVariable("Volume") - 0.15f);
				if (Railroad.trainLoop.GetVariable("Volume") <= 0f)
				{
					Railroad.trainLoop.Stop(AudioStopOptions.Immediate);
					Railroad.trainLoop = null;
					return;
				}
			}
			else if (this.trainTimer.Value > 0 && Railroad.trainLoop != null)
			{
				Railroad.trainLoop.SetVariable("Volume", Railroad.trainLoop.GetVariable("Volume") + 0.15f);
			}
		}

		// Token: 0x06003228 RID: 12840 RVA: 0x00280B55 File Offset: 0x0027ED55
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.train.Value != null && !Game1.eventUp)
			{
				this.train.Value.draw(b, this);
			}
		}

		// Token: 0x06003229 RID: 12841 RVA: 0x00280B84 File Offset: 0x0027ED84
		private void StartTrainLoopIfNeeded()
		{
			if (Game1.currentLocation == this)
			{
				ICue cue = Railroad.trainLoop;
				if (!(((cue != null) ? new bool?(cue.IsPlaying) : null) ?? false))
				{
					Game1.playSound("trainLoop", out Railroad.trainLoop);
					Railroad.trainLoop.SetVariable("Volume", 0f);
				}
			}
		}

		// Token: 0x04002186 RID: 8582
		private const double TrainChance = 0.09;

		// Token: 0x04002187 RID: 8583
		public const int trainSoundDelay = 15000;

		// Token: 0x04002188 RID: 8584
		[XmlIgnore]
		public readonly NetRef<Train> train = new NetRef<Train>();

		// Token: 0x04002189 RID: 8585
		[XmlElement("hasTrainPassed")]
		private readonly NetBool hasTrainPassed = new NetBool(false);

		// Token: 0x0400218A RID: 8586
		private int trainTime = -1;

		// Token: 0x0400218B RID: 8587
		[XmlIgnore]
		public readonly NetInt trainTimer = new NetInt(0);

		// Token: 0x0400218C RID: 8588
		public static ICue trainLoop;

		// Token: 0x0400218D RID: 8589
		[XmlElement("witchStatueGone")]
		public readonly NetBool witchStatueGone = new NetBool(false);

		// Token: 0x0400218E RID: 8590
		private static double DailyTrainChance;
	}
}
